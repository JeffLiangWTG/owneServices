using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data.SqlServer;

namespace CargoWise.Data
{
	public enum RefDbTypeEnum
	{
		Customs,
		Enterprise,
		Tariff,
		Single
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "CW1053: DontUseSRDBNameConstantRule", Justification = "Baseline issue")]
	public static class RefDbTableNameResolver
	{
		public const string SharedDbPrefix = "CW-";
		public const string RefDbAffix = "RefDb";
		public const string DefaultSingleRefDbName = "CW-RefDatabase";

		public const string SharedRefDbPrefix = SharedDbPrefix + RefDbAffix + "-";
		public const string SharedAvailabilityGroupRefDbPrefix = SharedDbPrefix + "AG-" + RefDbAffix + "-";
		public const string SingleRefDatabaseNameSynonymPrefix = "RefDatabase";
		public const string SingleRefDatabaseConsumeDATSnapshot = "SingleRefDatabaseConsumeDATSnapshot";
		static readonly Overridable<string> singleRefDatabaseName = new Overridable<string>();
		public static string SingleRefDatabaseName
		{
			get
			{
				if (string.IsNullOrEmpty(singleRefDatabaseName.Value))
				{
					if (!Db.ServerNameIsInitialized)
					{
						singleRefDatabaseName.Value = DefaultSingleRefDbName;
					}
					else
					{
						using (var connection = Db.NewAdminConnection())
						{
							connection.IsUpgradeCheckDisabled = true;
							singleRefDatabaseName.Value = GetSingleRefDatabaseName(connection);
						}
					}
				}
				return singleRefDatabaseName.Value;
			}
		}

		public static string GetSingleRefDatabaseName(DbConnection connection)
		{
			string dbName;
			try
			{
				dbName = DbRegistry.SingleRefDatabaseName.LoadValue(connection);
			}
			catch (SqlException ex) when (new[] { DbErrorType.InvalidObjectName, DbErrorType.CannotOpenDbRequestedInLogin }.Contains(new DbErrorMatch(ex).ExceptionType))
			{
				dbName = string.Empty;
			}

			return string.IsNullOrEmpty(dbName) ? DefaultSingleRefDbName : dbName;
		}

		public static void ResetSingleRefDatabaseName(string name = null)
		{
			singleRefDatabaseName.Value = name;
		}

		public static string GetRefDbSchemaClassFolder(RefDbTypeEnum refDbType, string refCountryCode)
		{
			if (refDbType == RefDbTypeEnum.Single)
			{
				return SingleRefDatabaseNameSynonymPrefix;
			}
			return string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}", RefDbAffix, refCountryCode, refDbType.ToString());
		}

		public static string GetSelectBizoViewNameQueryFromDbWithVersionedViewNameList(string dbName, string objectList)
		{
			return string.Format(CultureInfo.InvariantCulture, " select IIF(PATINDEX('%TableView_V[1-9]%', o.name)=0, SUBSTRING(o.name, 0, PATINDEX('%View_V[1-9]%', o.name) + 4),SUBSTRING(o.name, 0, PATINDEX('%TableView_V[1-9]%', o.name))) as name, c.name, c.column_id from [{0}].sys.objects o " +
	" join   [{0}].sys.columns c on c.object_id = o.object_id " +
	" where  o.type = 'v' and schema_id = schema_id('DBO') " +
						" and  o.name in ({1}) ", dbName, objectList);
		}

		public static string GetRefDbTableSynonym(RefDbTypeEnum refDbType, string refCountryCode, string tableName)
		{
			Argument.NotNullOrEmpty(tableName, nameof(tableName));

			if (refDbType == RefDbTypeEnum.Single)
			{
				return string.Format(CultureInfo.InvariantCulture, "{0}_{1}", SingleRefDatabaseNameSynonymPrefix, tableName);
			}

			return GetRefDbSynonymPrefix(refDbType, refCountryCode) + tableName;
		}

		public static string GetRefDbSynonymPrefix(RefDbTypeEnum refDbType, string refCountryCode)
		{
			Argument.NotNullOrEmpty(refCountryCode, nameof(refCountryCode));
			if (refDbType == RefDbTypeEnum.Single)
			{
				throw new NotSupportedException("GetRefDbSynonymPrefix() does not support RefDbTypeEnum.Single");
			}
			return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}_", RefDbAffix, GetDbType3LetterCode(refDbType), refCountryCode);
		}

		public static string GetDbType3LetterCode(RefDbTypeEnum refDbType)
		{
			switch (refDbType)
			{
				case RefDbTypeEnum.Customs:
					return "Cmr"; // not seen by user
				case RefDbTypeEnum.Enterprise:
					return "Ent"; // not seen by user
				case RefDbTypeEnum.Tariff:
					return "Trf"; // not seen by user
				case RefDbTypeEnum.Single:
					throw new NotSupportedException("GetDbType3LetterCode() does not support RefDbTypeEnum.Single"); // not seen by user
				default:
					throw new ArgumentException("Invalid Reference Database type: " + refDbType, nameof(refDbType));
			}
		}

		public static string GetExclusiveRefDbName(string mainDbName, RefDbTypeEnum refDbType, string refCountryCode)
		{
			Argument.NotNullOrEmpty(mainDbName, nameof(mainDbName));
			Argument.NotNullOrEmpty(refCountryCode, nameof(refCountryCode));

			return mainDbName + GetExclusiveRefDbNameSuffix(refDbType, refCountryCode);
		}

		public static string GetExclusiveRefDbNameSuffix(RefDbTypeEnum refDbType, string refCountryCode)
		{
			Argument.NotNullOrEmpty(refCountryCode, nameof(refCountryCode));
			if (refDbType == RefDbTypeEnum.Single)
			{
				throw new NotSupportedException("GetExclusiveRefDbNameSuffix() does not support RefDbTypeEnum.Single");
			}
			return FormattableString.Invariant($"_{RefDbAffix}_{GetDbType3LetterCode(refDbType)}_{refCountryCode}"); // SQL reference database suffix
		}

		public static string GetSharedAvailabilityGroupRefDbPrefix(string availabilityGroupName, RefDbTypeEnum refDbType, string refCountryCode)
		{
			Argument.NotNullOrEmpty(availabilityGroupName, nameof(availabilityGroupName));
			Argument.NotNullOrEmpty(refCountryCode, nameof(refCountryCode));

			return FormattableString.Invariant(
				$"{SharedAvailabilityGroupRefDbPrefix}{availabilityGroupName}-{GetDbType3LetterCode(refDbType)}-{refCountryCode}-"); // SQL reference database suffix
		}

		public static bool IsSharedDatabase(string dbName)
		{
			return
				dbName != null
				&&
				(
					dbName.StartsWith(SharedDbPrefix + RefDbAffix, StringComparison.OrdinalIgnoreCase)
					|| dbName.StartsWith(SharedAvailabilityGroupRefDbPrefix, StringComparison.OrdinalIgnoreCase)
					|| dbName == DefaultSingleRefDbName
					|| dbName == SingleRefDatabaseName
				);
		}

		public static bool IsExclusiveDatabase(string mainDbName, string dbName)
		{
			return
				mainDbName != null
				&& dbName != null
				&& dbName.StartsWith(string.Format(CultureInfo.InvariantCulture, "{0}_{1}_", mainDbName, RefDbAffix), StringComparison.OrdinalIgnoreCase);
		}

		public static bool ShouldUseSharedDatabases(DbConnection connection)
		{
			return
				DataUtils.IsWiseTechGlobalDatabaseServer(connection)
				&& !DataUtils.IsCmrMsgTestServer(connection.ServerName);
		}

		public static bool ShouldUseSharedAvailabilityGroupDatabases(DbConnection mainDbConnection)
		{
			return
				ShouldUseSharedDatabases(mainDbConnection)
				&& AlwaysOn.IsDbPartOfAlwaysOn(mainDbConnection, ((ICurrentDbControl)mainDbConnection).InitialDatabase);
		}
	}
}

#region Test
#if DEBUG

namespace CargoWise.Data
{
	using System.Text.RegularExpressions;

	public static class RefDbTableNameResolverTestHelper
	{
		const string RefDbSynonymPrefixRegexPattern = "^" + RefDbTableNameResolver.RefDbAffix + @"(?<type>\w{3})(?<country>\w{2})_(?<table>\w+)$";
		public static readonly Regex SynonymRegex = new Regex(RefDbSynonymPrefixRegexPattern, RegexOptions.Compiled);

		public static bool GetDbAndBaseTableFromSynonym(string synonymName, out string dbName, out string baseTable)
		{
			Argument.NotNull(synonymName, nameof(synonymName)); // Suggested By ReviewBot 
			dbName = null;
			baseTable = null;

			var synonymMatch = SynonymRegex.Match(synonymName);

			if (synonymMatch.Success)
			{
				string dbType = synonymMatch.Groups["type"].Value;
				string countryCode = synonymMatch.Groups["country"].Value;
				baseTable = synonymMatch.Groups["table"].Value;
				dbName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_{3}", Db.DatabaseName, RefDbTableNameResolver.RefDbAffix, dbType, countryCode);
				return true;
			}

			if (synonymName.StartsWith(RefDbTableNameResolver.SingleRefDatabaseNameSynonymPrefix))
			{
				var tableName = synonymName.Substring(RefDbTableNameResolver.SingleRefDatabaseNameSynonymPrefix.Length + 1);
				dbName = string.Format(CultureInfo.InvariantCulture, "{0}", RefDbTableNameResolver.DefaultSingleRefDbName);
				baseTable = tableName;
				return true;
			}

			return false;
		}
	}
}

#endif
#endregion
