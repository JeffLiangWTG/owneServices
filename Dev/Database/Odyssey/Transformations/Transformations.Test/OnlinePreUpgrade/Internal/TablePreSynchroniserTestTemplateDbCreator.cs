using System;
using System.Collections.Generic;
using System.Globalization;
using Enterprise.DbUpgrader.Schema.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing
{
	public class TablePreSynchroniserTestTemplateDbCreator : IAuxiliaryDbCreator
	{
		public TablePreSynchroniserTestTemplateDbCreator(string templateDbName, string targetTableName, IEnumerable<string> columnList)
			: this(templateDbName, (targetTableName, columnList))
		{
		}
		public TablePreSynchroniserTestTemplateDbCreator(string templateDbName, params (string targetTableName, IEnumerable<string> columnList)[] targetTables)
		{
			this.templateDbName = templateDbName;
			this.targetTables = targetTables;
		}

		readonly string templateDbName;
		readonly (string TargetTableName, IEnumerable<string> ColumnList)[] targetTables;

		public void CreateDropExisting()
		{
			((IAuxiliaryDbCreator)DbCreator).CreateDropExisting();
		}

		public void Drop()
		{
			((IAuxiliaryDbCreator)DbCreator).Drop();
		}

		TemplateDbCreatorForTest DbCreator
		{
			get { return dbCreator ?? (dbCreator = new TemplateDbCreatorForTest(new DummyUpgradeManager(), this.templateDbName, GetScripts())); }
		}
		TemplateDbCreatorForTest dbCreator;

		string[] GetScripts()
		{
			var scripts = new List<string>();
			foreach (var table in targetTables)
			{
				scripts.Add(String.Format(CultureInfo.InvariantCulture, @"
CREATE TABLE [dbo].[{0}]
(
	{1}
);
"
					, table.TargetTableName                    // 0
					, string.Join(",\r\n\t", table.ColumnList) // 1
					));
			}
			return scripts.ToArray();
		}
	}
}
