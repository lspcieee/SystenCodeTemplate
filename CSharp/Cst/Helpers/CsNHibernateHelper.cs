using System;
using System.ComponentModel;
using System.Data;
using System.Collections.Generic;
using System.Text;
using CodeSmith.Engine;
using SchemaExplorer;
using NHibernateHelper;
using System.Text.RegularExpressions;

public class CsNHibernateHelper : NHibernateHelper.NHibernateHelper
{
    //输出提示信息
    public void Alert(string msg)
    {
        System.Windows.Forms.MessageBox.Show(
                msg,
                "CodeSmith - KD 模块生成",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Warning);
    }
    
    //字段备注中是否包含某字符串
    public bool DescriptionContains(EntityMember em,string str)
    {
        return em.Column.Description.Contains(str);
    }

    //判断是否包含逻辑删除字段
    public bool HasLogicDeleteEntityMember(EntityManager entityManager ){
        bool returnBool=false;
        foreach(EntityMember em in entityManager.MembersNoKeyNoVersion) { 
            if(em.PropertyName=="IsDeleted"){
                returnBool=true;
            }
        }
        return returnBool;
    }
    
    //获取清除字段标记的描述
    public string GetDescription(EntityMember em){
        string des= em.Column.Description;
        des=ClearNote(des);
        return des;
    }
    
    //清除数据库标记
    public string  ClearNote(string str){
        string des= str;
        des=des.Replace("[search]",string.Empty);
        des=des.Replace("[list]",string.Empty);
        des=des.Replace("[lazy]",string.Empty);
        des=des.Replace("[hide]",string.Empty);
        des=des.Replace("[noEdit]",string.Empty);
        des=des.Replace("[noList]",string.Empty);
        des=des.Replace("[noDetail]",string.Empty);
        des=des.Replace("[noLazy]",string.Empty);
        return des;
    }

	/// <summary>
    /// 字符串首字母小写
    /// </summary>
    public static string ToProperCase(string s)
    {
        string revised = string.Empty;
        if (s.Length > 0)
        {
            
            string firstLetter = s.Substring(0, 1).ToLower();
            revised = firstLetter + s.Substring(1, s.Length - 1);
        }
        return revised;
    }
    
    /// <summary>
    /// 字符串首字母大写
    /// </summary>
    public static string ToPascalCase(string input)
    {
        if(string.IsNullOrEmpty(input))
            return string.Empty;
        string[] ns = input.Split(new char[]{'_',' '},StringSplitOptions.RemoveEmptyEntries);
        for(int i=0;i< ns.Length;i++)
        {
            string tmps = ns[i].Substring(1);
            if(Regex.IsMatch(tmps,"[a-z]+"))
                ns[i] = ns[i].Substring(0,1).ToUpper()+tmps;
            else
                ns[i] = ns[i].Substring(0,1).ToUpper()+tmps.ToLower();
        }
        return string.Join("",ns);
    }
    
    //获取表对应的类名
    public static string GetTableClassName(SchemaExplorer.TableSchema table){
        string tableName=table.Name;
        
        string className=GetClassName(table);
        if(tableName.EndsWith("s")){
            if(!className.EndsWith("s"))
            {
                className=className+"s";
            }
        }
        return className;
    }
        
	private MapCollection _keyWords;
	public MapCollection KeyWords
	{
		get
		{
			if (_keyWords == null)
			{
				string path;
				Map.TryResolvePath("CSharpKeyWordEscape", this.CodeTemplateInfo.DirectoryName, out path);
				_keyWords = Map.Load(path);
			}
			return _keyWords;
		}
	}
	
	public string GetInitialization(ColumnSchema column)
    {
		if(column.AllowDBNull)
			return "null";
		
		Type type = column.SystemType;
        string result;

        if (type.Equals(typeof(String)))
            result = "String.Empty";
        else if (type.Equals(typeof(DateTime)))
            result = "new DateTime()";
        else if (type.Equals(typeof(Decimal)))
            result = "default(Decimal)";
        else if (type.Equals(typeof(Guid)))
            result = "Guid.Empty";
        else if (type.IsPrimitive)
            result = String.Format("default({0})", type.Name.ToString());
        else
            result = "null";
        return result;
    }
	
	public string GetMethodParameters(EntityManager entityManager, List<MemberColumnSchema> mcsList, bool isDeclaration)
	{
		StringBuilder result = new StringBuilder();
		bool isFirst = true;
		foreach (MemberColumnSchema mcs in mcsList)
		{
			if (isFirst)
				isFirst = false;
			else
				result.Append(", ");
			if (isDeclaration)
			{
				result.Append(mcs.SystemType.ToString());
				result.Append(" ");
			}
			result.Append(KeyWords[entityManager.GetEntityBaseFromColumn(mcs).VariableName]);
		}
		return result.ToString();
	}
	public string GetMethodParameters(EntityManager entityManager, MemberColumnSchemaCollection mcsc, bool isDeclaration)
	{
		List<MemberColumnSchema> mcsList = new List<MemberColumnSchema>();
		for (int x = 0; x < mcsc.Count; x++)
			mcsList.Add(mcsc[x]);
		return GetMethodParameters(entityManager, mcsList, isDeclaration);
	}
	public string GetMethodDeclaration(EntityManager entityManager, SearchCriteria sc)
	{
		StringBuilder result = new StringBuilder();
		result.Append(sc.MethodName);
		result.Append("(");
		result.Append(GetMethodParameters(entityManager, sc.Items, true));
		result.Append(")");
		return result.ToString();
	}
	public string GetPrimaryKeyCallParameters(List<MemberColumnSchema> mcsList)
	{
		System.Text.StringBuilder result = new System.Text.StringBuilder();
		for (int x = 0; x < mcsList.Count; x++)
		{
			if (x > 0)
				result.Append(", ");
	
			if (mcsList[x].SystemType == typeof(Guid))
				result.AppendFormat("new {0}(keys[{1}])", mcsList[x].SystemType, x);
			else if (mcsList[x].SystemType == typeof(string))
				result.AppendFormat("keys[{0}]", x);
			else
				result.AppendFormat("{0}.Parse(keys[{1}])", mcsList[x].SystemType, x);
		}
		return result.ToString();
	}
    /// <summary>
    /// 得到最后一个.后的字符
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <returns></returns>
    public string GetLastAssemblyName(string assemblyName)
    {
        var assemblyNames= assemblyName.Split('.');
    	 
    	return assemblyNames[assemblyNames.Length-1];
    }
    
}
