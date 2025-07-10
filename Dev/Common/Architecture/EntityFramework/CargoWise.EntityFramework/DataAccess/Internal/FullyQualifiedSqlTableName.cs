using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public class FullyQualifiedSqlTableName
	{
		public FullyQualifiedSqlTableName()
		{
		}

		public FullyQualifiedSqlTableName(string tableName)
		{
			Table = tableName;
		}

		public string Database = string.Empty;
		public string Owner = (NoResString)"dbo"; // dbo schma name, no need to translate
		public string Table;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "dbo schema name does not need to translate")]
		public string FullyQualifiedName
		{
			get
			{
				if (Database.Length > 0 && Owner.Length > 0)
				{
					return Database + "." + Owner + "." + Table;
				}
				else if (Owner.Length > 0 && Owner != "dbo")
				{
					return Owner + "." + Table;
				}
				else
				{
					return Table;
				}
			}
			set
			{
				var sections = value.Split(new char[] { '.' });
				if (sections.Length == 1)
				{
					Table = sections[0];
				}
				else if (sections.Length == 2)
				{
					Owner = sections[0];
					Table = sections[1];
				}
				else
				{
					Database = sections[0];
					Owner = sections[1];
					Table = sections[2];
				}
			}
		}
	}
}
