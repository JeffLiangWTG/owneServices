using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static CargoWise.Bi.Configuration.DataSets.BiAutomationConfigDataSet;

namespace CargoWise.Bi.Configuration
{
	public static class BiScriptHelper
	{
		public static string GenerateAuditTableDefinition(CdcTableConfigRow cdcTableConfig)
		{
			return "CREATE TABLE [" + cdcTableConfig.SourceSchema + "].[" + cdcTableConfig.SourceTable + "]\r\n(\r\n"
				+ GetAuditColumnDefinitionClause(cdcTableConfig) + "\r\n"
				+ " ) ON [PRIMARY];\r\n\r\n"
				+ "CREATE CLUSTERED COLUMNSTORE INDEX [cci_" + cdcTableConfig.SourceSchema + "_" + cdcTableConfig.SourceTable + "] ON [" + cdcTableConfig.SourceSchema + "].[" + cdcTableConfig.SourceTable + "] WITH (DROP_EXISTING = OFF);\r\n\r\n";
		}

		public static string GenerateAuditStartLsnIndexDefinition(string schema, string table, string pkName)
		{
			return $"CREATE NONCLUSTERED INDEX IX_{table}_StartLsn ON [{schema}].[{table}] (__$start_lsn,__$command_id,__$seqval,__$operation) INCLUDE (__$update_mask{(string.IsNullOrEmpty(pkName) ? "" : $",{pkName}")}) WITH (DATA_COMPRESSION = PAGE);\r\n";
		}

		public static string GenerateAuditTableAddColumnsDefinition(CdcTableConfigRow cdcTableConfig, IEnumerable<string> columnNames)
		{
			var stringBuilder = new StringBuilder();

			stringBuilder.AppendLine("ALTER TABLE [" + cdcTableConfig.SourceSchema + "].[" + cdcTableConfig.SourceTable + "] ADD");
			var cdcColumnConfigRows = cdcTableConfig.GetOrderedCdcColumnConfigRows();
			stringBuilder.Append(string.Join(",\r\n", cdcColumnConfigRows
				.Where(cdcColumnConfig => columnNames.Any(x => x == cdcColumnConfig.SourceColumn))
				.Select(cdcColumnConfig => "\t" + GetAuditColumnDefinitionPart(cdcColumnConfig))));  // sql query building
			stringBuilder.Append(";");
			return stringBuilder.ToString();
		}

		static string GetAuditColumnDefinitionClause(CdcTableConfigRow cdcTableConfig)
		{
			var clause = new StringBuilder();

			clause.AppendLine("\t[__$start_lsn] binary(10) NOT NULL");
			clause.AppendLine("\t,[__$seqval] binary(10) NOT NULL");
			clause.AppendLine("\t,[__$operation] int NOT NULL");
			clause.AppendLine("\t,[__$update_mask] varbinary(128) NOT NULL");
			clause.AppendLine("\t,[__$lsn_period] smallint NOT NULL");
			clause.AppendLine("\t,[__$command_id] int NOT NULL DEFAULT 0");

			var cdcColumnConfigRows = cdcTableConfig.GetOrderedCdcColumnConfigRows();
			foreach (var cdcColumnConfig in cdcColumnConfigRows)
			{
				if (cdcColumnConfig.CdcEnabled || cdcColumnConfig.IsPrimaryKey)
				{
					clause.AppendLine("\t," + GetAuditColumnDefinitionPart(cdcColumnConfig)); // sql query building
				}
			}

			return clause.ToString();
		}

		static string GetAuditColumnDefinitionPart(CdcColumnConfigRow cdcColumnConfig)
		{
			return "[" + cdcColumnConfig.SourceColumn + "] " + GetDataTypeDefinition(cdcColumnConfig.DataType, cdcColumnConfig.MaxLength, cdcColumnConfig.Precision, cdcColumnConfig.Scale) + " NULL";
		}

		static string GetDataTypeDefinition(string dataType, int maxLength, int precision, int scale)
		{
			var dataTypeDefinition = dataType;

			switch (dataType)
			{
				case "decimal":
					dataTypeDefinition += "(" + precision + "," + scale + ")";
					break;
				case "char":
				case "varchar":
				case "nchar":
				case "nvarchar":
				case "varbinary":
					dataTypeDefinition += "(" + (maxLength == -1 ? "max" : maxLength.ToString()) + ")";
					break;
				case "datetimeoffset":
					if (scale >= 0 && scale < 7)
					{
						dataTypeDefinition += $"({scale})";
					}
					break;
				case "xml":
					dataTypeDefinition = "nvarchar(max)";
					break;
			}

			return dataTypeDefinition;
		}

		public static bool ContainsIllegalQueryElements(StringBuilder tableMessage, string expression)
		{
			var result = false;
			var hintRegex = new Regex(@"\bWITH\b\s*\(.+\)", RegexOptions.IgnoreCase);
			var subqueryRegex = new Regex(@"\(\s*\bSELECT\b(.|\s)*?\bFROM\b(.|\s)*?\)", RegexOptions.IgnoreCase);
			var isNullRegex = new Regex(@"\bISNULL\b\s*\(.+?\)", RegexOptions.IgnoreCase);
			var coalesceRegex = new Regex(@"\bCOALESCE\b\s*\(.+?\)", RegexOptions.IgnoreCase);

			if (string.IsNullOrEmpty(expression))
			{
				tableMessage.AppendLine("Expression cannot be empty.");
				result = true;
			}
			else if (hintRegex.Match(expression).Success)
			{
				tableMessage.AppendLine("Hints are not allowed. Remove WITH expression.");
				result = true;
			}
			else if (subqueryRegex.Match(expression).Success)
			{
				tableMessage.AppendLine("Subqueries are not supported.");
				result = true;
			}
			else if (isNullRegex.Match(expression).Success || coalesceRegex.Match(expression).Success)
			{
				tableMessage.AppendLine("Use CASE END for joins instead of ISNULL() or COALESCE.");
				result = true;
			}
			return result;
		}
	}
}
