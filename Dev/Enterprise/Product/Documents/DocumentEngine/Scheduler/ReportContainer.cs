using System;
using System.Collections;
using System.Xml.Serialization;

namespace Enterprise.DocumentEngine
{
	[XmlRoot("Report")]
	public class ReportContainer
	{
		public ReportContainer()
		{
			SqlParams = new ArrayList();
			UserDefinedFieldList = new ArrayList();
		}

		public string ReportName;
		public Guid PK;
		public Core.Constants.DataContext DataContext;
		public byte[] TemplateData;

		public string WhereClause;

		[System.Xml.Serialization.XmlArray("SqlParams")]
		[System.Xml.Serialization.XmlArrayItem(typeof(SqlParameter))]
		internal ArrayList SqlParams;

		public string SortOrderDisplayName;
		public string SortOrderFieldList;

		[System.Xml.Serialization.XmlArray("UserDefinedFieldList")]
		[System.Xml.Serialization.XmlArrayItem(typeof(SqlParameter))]
		internal ArrayList UserDefinedFieldList;
	}
}
