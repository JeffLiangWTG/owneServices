using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public class MoveColumnFromAddInfoHelper
	{
		public MoveColumnFromAddInfoHelper(string sourceTable, string sourTablePrefix, string sourceColumnName, string sourceAddInfoColumnName, string targetColumnName, string offLineUpdateTargetSqlText, string addTargetColumnSqlText, string calculateTempColumnSqlText = "", bool pendingDrop = false)
		{
			this.SourceTable = sourceTable;
			this.SourTablePrefix = sourTablePrefix;
			this.SourceColumnName = sourceColumnName;
			this.TargetColumnName = targetColumnName;
			this.offLineUpdateTargetSqlText = offLineUpdateTargetSqlText;
			this.AddTargetColumnSqlText = addTargetColumnSqlText;
			this.SourceAddInfoColumnName = sourceAddInfoColumnName;
			this.CalculateTempColumnSqlText = calculateTempColumnSqlText;
			this.PendingDrop = pendingDrop;
		}

		public string SourceTable { get; set; }
		public string SourTablePrefix { get; set; }
		public string SourceColumnName { get; set; }
		public string TargetColumnName { get; set; }
		public string OtherColumnsForInclude { get; set; }

		public string AddTargetColumnSqlText { get; set; }
		public string SourceAddInfoColumnName { get; set; }
		public string CalculateTempColumnSqlText { get; set; }
		public bool PendingDrop { get; set; }

		public string ColumnsForIncludeInIndex
		{
			get
			{
				var columns = new List<string> { SourceColumnName };

				if (!string.IsNullOrWhiteSpace(OtherColumnsForInclude))
				{
					var array = OtherColumnsForInclude.Split(',');
					columns.AddRange(array);
				}

				return string.Join(",", columns.Select(c => c.QuoteName()));
			}
		}

		public string OffLineUpdateTargetSqlText
		{
			get
			{
				return (!offLineUpdateTargetSqlText.Contains(TempColumnFilterPlaceholder)) ?
					string.Format(CultureInfo.InvariantCulture, "{0} AND {1}.{2} = 1", offLineUpdateTargetSqlText, SourceTable, TempColumnNameForIndex) :
					offLineUpdateTargetSqlText.Replace(TempColumnFilterPlaceholder, string.Format(CultureInfo.InvariantCulture, " {0}.{1} = 1", SourceTable, TempColumnNameForIndex));
			}
		}
		readonly string offLineUpdateTargetSqlText;

		public string TempColumnNameForIndex
		{
			get { return string.Format(CultureInfo.InvariantCulture, "{0}_TMP_HasMoveColumn{1}", SourTablePrefix, SourceAddInfoColumnName); }
		}

		public string TempIndexName
		{
			get { return string.Format(CultureInfo.InvariantCulture, "{0}_Index", TempColumnNameForIndex); }
		}

		public string DropTempIndexSqlText
		{
			get { return string.Format(CultureInfo.InvariantCulture, "DROP INDEX {0} ON {1}", TempIndexName, SourceTable); }
		}

		public string DropTempColumnSqlText
		{
			get { return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0} DROP COLUMN {1}", SourceTable, TempColumnNameForIndex); }
		}

		public string GetCalculateTempColumnSqlText(string targetUpgraded)
		{
			string sqlColumnText;
			if (CalculateTempColumnSqlText.IsNullOrEmpty())
			{
				sqlColumnText = string.Format(CultureInfo.InvariantCulture, @"ALTER TABLE [{0}].[dbo].[{1}] ADD [{2}] AS CAST(CASE WHEN [{3}] like '%{4}=%' THEN 1 ELSE 0 END AS BIT)", targetUpgraded, SourceTable, TempColumnNameForIndex, SourceColumnName, SourceAddInfoColumnName);
			}
			else
			{
				sqlColumnText = string.Format(CultureInfo.InvariantCulture, @"ALTER TABLE [{0}].[dbo].[{1}] ADD [{2}] AS CAST({3})", targetUpgraded, SourceTable, TempColumnNameForIndex, CalculateTempColumnSqlText);
			}

			return sqlColumnText;
		}

		public const string TempColumnFilterPlaceholder = "#TempColumnFilter#";
	}
}
