#if DEBUG

using System;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;

namespace NUnit.Framework
{
	public static class TestCaseHelper
	{
		public static void RunClientDbCreateScripts()
		{
			RunClientDbCreateScripts(false);
		}

		public static void RunClientDbCreateScripts(bool recreateObjectIfExists)
		{
			new ClientDbSchemaCreationForTesting().RunClientDbCreateScripts(recreateObjectIfExists);
		}

		#region Helper Methods for Tests

		public static Guid GetCommodityGuidFromCode(string code)
		{
			return GetColumnFromTableByValue(RefCommodityCodeSchema.Constants.PK, NormalizeTableName(RefCommodityCodeSchema.Constants.TableName, RefCommodityCodeSchema.Constants.SqlSchemaName), RefCommodityCodeSchema.Constants.RH_Code, code);
		}

		public static Guid GetServiceLevelGuidFromCode(string code)
		{
			return GetColumnFromTableByValue(RefServiceLevelSchema.Constants.PK, NormalizeTableName(RefServiceLevelSchema.Constants.TableName, RefServiceLevelSchema.Constants.SqlSchemaName), RefServiceLevelSchema.Constants.RS_Code, code);
		}

		public static Guid GetOrgHeaderGuidFromFullName(string fullName)
		{
			return GetColumnFromTableByValue(OrgHeaderSchema.Constants.PK, NormalizeTableName(OrgHeaderSchema.Constants.TableName, OrgHeaderSchema.Constants.SqlSchemaName), OrgHeaderSchema.Constants.OH_FullName, fullName);
		}

		public static Guid GetChargeCodeGuidFromCode(string code)
		{
			return GetColumnFromTableByValue(AccChargeCodeSchema.Constants.PK, NormalizeTableName(AccChargeCodeSchema.Constants.TableName, AccChargeCodeSchema.Constants.SqlSchemaName), AccChargeCodeSchema.Constants.AC_Code, code);
		}

		public static Guid GetUNLOCOGuidFromCode(string code)
		{
			return GetColumnFromTableByValue(RefUNLOCOSchema.Constants.PK, NormalizeTableName(RefUNLOCOSchema.Constants.TableName, RefUNLOCOSchema.Constants.SqlSchemaName), RefUNLOCOSchema.Constants.RL_Code, code);
		}

		public static Guid GetCurrencyGuidFromCode(string code)
		{
			return GetColumnFromTableByValue(RefCurrencySchema.Constants.PK, NormalizeTableName(RefCurrencySchema.Constants.TableName, RefCurrencySchema.Constants.SqlSchemaName), RefCurrencySchema.Constants.RX_Code, code);
		}

		public static Guid GetColumnFromTableByValue(string selectColumn, string table, string valueColumn, string value)
		{
			return (Guid)Db.Connection.ExecuteScalar("SELECT " + selectColumn + " FROM " + table + " WHERE " + valueColumn + " = '" + value + "'");
		}

		public static Guid GetFirstPKFromTable(string tableName)
		{
			return (Guid)Db.Connection.ExecuteScalar("SELECT TOP 1 " + ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(tableName).Name + " FROM " + NormalizeTableName(tableName));
		}

		public static void ClearTable(string tableName, string schemaName = null)
		{
			Db.Connection.ExecuteNonQuery("DELETE " + NormalizeTableName(tableName, schemaName));
		}

		static string NormalizeTableName(string tableName, string schemaName = null)
		{
			schemaName ??= DefaultSchemaName;
			var match = SchemaNameRegex.Match(tableName);
			if (match.Success)
			{
				if (match.Groups[1].Value.Equals("."))
				{
					tableName = schemaName + "." + match.Groups[2].Value;
				}
				return tableName;
			}

			return schemaName + "." + tableName;
		}

		static readonly Regex SchemaNameRegex = new(@"(\S*\.)(\S+)", RegexOptions.Compiled);
		const string DefaultSchemaName = "dbo";

		#endregion
	}
}

#endif
