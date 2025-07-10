using System;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			[AttributeUsage(AttributeTargets.Class)]
			public sealed class EnableRefererenceXMLExporterAttribute : Attribute
			{
				public EnableRefererenceXMLExporterAttribute(string tableName, string columnName)
				{
					this.TableName = tableName;
					this.ColumnName = columnName;
				}

				public string TableName { get; private set; }
				public string ColumnName { get; private set; }
			}
		}
	}
}