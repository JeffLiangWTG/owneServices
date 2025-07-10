using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Billing.Collectors;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Integration.Billing;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	[CodeAlive("Subtypes of BaseStlScript are loaded by reflection.")]
	abstract class BaseStlScript : IStlScript
	{
		string IStlItem.Code { get { return FeatureCode; } }
		string IStlItem.Role { get { return RoleName; } }
		string IStlItem.Module { get { return ModuleName; } }
		string IStlItem.Function { get { return FunctionName; } }
		string IStlItem.Feature { get { return FeatureName; } }
		StlDataGrain IStlItem.StlGrain { get { return StlItemGrain; } }
		bool IStlItem.IsSystemLevel { get { return (TransactionCompanyExpression == null || TransactionCompanyExpression == "''"); } }
		StlDateType IStlItem.DateType { get => ScriptDateType; }

		IEnumerable<ZSqlParameter> IStlItem.GetInputParameters(IDateTimeRange dateTimeRange)
		{
			SchemaColumn dateSchemaColumn;
			switch (ScriptDateType)
			{
				case StlDateType.DateTime:
					dateSchemaColumn = Schema.GenericDateTimeColumn;
					break;
				case StlDateType.DateTimeOffset:
					dateSchemaColumn = Schema.GenericDateTimeOffsetColumn;
					break;
				case StlDateType.SmallDateTime:
					dateSchemaColumn = Schema.GenericSmallDateTimeColumn;
					break;
				default:
					throw new ArgumentOutOfRangeException(ScriptDateType.ToString());
			}

			var startTime = dateTimeRange.StartDateTimeInclusive;
			var endTime = dateTimeRange.EndDateTimeExclusive;
			if ((StlItemGrain == StlDataGrain.MonthlyAllowHistoricalData || StlItemGrain == StlDataGrain.MonthlyCurrentDataOnly) && dateTimeRange.MonthlyRangeStartInclusive.HasValue)
			{
				startTime = dateTimeRange.MonthlyRangeStartInclusive.Value;
				endTime = dateTimeRange.MonthlyRangeEndExclusive.Value;
			}
			else if (StlItemGrain != StlDataGrain.Transactional && dateTimeRange.DailyRangeStartInclusive.HasValue)
			{
				startTime = dateTimeRange.DailyRangeStartInclusive.Value;
				endTime = dateTimeRange.DailyRangeEndInclusive.Value;
			}

			if (ScriptDateType == StlDateType.DateTimeOffset)
			{
				yield return ZSqlParameter.New(Constants.StartDateTimeInclusiveParamName, new DateTimeOffset(DateTime.SpecifyKind(startTime, DateTimeKind.Utc)), dateSchemaColumn);
				yield return ZSqlParameter.New(Constants.EndDateTimeExclusiveParamName, new DateTimeOffset(DateTime.SpecifyKind(endTime, DateTimeKind.Utc)), dateSchemaColumn);
			}
			else
			{
				yield return ZSqlParameter.New(Constants.StartDateTimeInclusiveParamName, startTime, dateSchemaColumn);
				yield return ZSqlParameter.New(Constants.EndDateTimeExclusiveParamName, endTime, dateSchemaColumn);
			}
		}

		public abstract IEnumerable<IStlTransaction> Run(IDateTimeRange dateTimeRange);

		#region SuppressResourceStringsCheckRegion

		/// <summary>
		/// Composes STL query from its parts.
		/// 
		/// The returned script expects the following parameters to be defined:
		///   - @StartDateTimeInclusive datetime = lower date range boundary, inclusive
		///   - @EndDateTimeExclusive   datetime = upper date range boundary, exclusive
		///   
		/// Columns in the ResultSet:
		///   - CompanyCode                 char(3),
		///   - BranchCode                  char(3),
		///   - TransactionDateUtc          datetime,
		///   - UserCode                    char(3),
		///   - TransactionReference01      varchar(50),
		///   - TransactionReference02      varchar(50),
		///   - TransactionReference03      varchar(50),
		///   - TransactionReference04      varchar(50),
		///   - TransactionGuidReference    varchar(36),
		///   - ItemCount                   int
		/// </summary>
		string IStlScript.ScriptText
		{
			get
			{
				var optionRecompileFormatted = WithOptionRecompile ? "OPTION (RECOMPILE)" : "";
				return $@"
					-- STL Collector query for FeatureCode={FeatureCode}, CollectorType={CollectorType}
					{PreparationScript ?? ""}

					SELECT
						CompanyCode,
						{((CompanyNameExpression != null) ? "CompanyName = max(CompanyName)," : "")}
						TransactionDateUtc,
						TransactionReference01,
						TransactionReference02,
						TransactionReference03, 
						TransactionReference04,
						TransactionGuidReference,
						BranchCode = max(TransactionBranch),
						UserCode = max(TransactionUser),
						ItemCount = convert(int, round(sum(TransactionCount), 0)),
						{(IncludeNonserializablePropertiesInAdditionalRefs ? AppendAdditionRefsExpression() : "AdditionalRefs")}
					FROM (
						SELECT
							CompanyCode = isnull({SubstituteNullOrEmpty(TransactionCompanyExpression, "NULL")}, ''),
							{((CompanyNameExpression != null) ? "CompanyName = " + CompanyNameExpression + "," : "")}
							TransactionDateUtc = {TransactionDateUtcExpression},
							TransactionReference01 = rtrim(left({SubstituteNullOrEmpty(TransactionReference01, "''")}, 50)),
							TransactionReference02 = rtrim(left({SubstituteNullOrEmpty(TransactionReference02, "''")}, 50)),
							TransactionReference03 = rtrim(left({SubstituteNullOrEmpty(TransactionReference03, "''")}, 50)),
							TransactionReference04 = rtrim(left({SubstituteNullOrEmpty(TransactionReference04, "''")}, 50)),
							TransactionGuidReference = rtrim(convert(varchar(36), {TransactionGuidReference})),
							TransactionBranch = isnull({SubstituteNullOrEmpty(BranchCodeExpression, "NULL")}, ''),
							TransactionUser = {SubstituteNullOrEmpty(CreateUserCodeExpression, "''")},
							TransactionCount = {TransactionCountExpression},
							AdditionalRefs = {(IncludeNonserializablePropertiesInAdditionalRefs ? VarcharAdditionalRefsExpression : SubstituteNullOrEmpty(AdditionalRefs, "0x"))}
						FROM
							{FromClause}
						{WhereClauseCombined}
					) Transactions
					GROUP BY
						CompanyCode,
						TransactionDateUtc,
						TransactionGuidReference,
						TransactionReference01,
						TransactionReference02,
						TransactionReference03,
						TransactionReference04,
						AdditionalRefs
					{optionRecompileFormatted}";
			}
		}

		string WhereClauseCombined
		{
			get
			{
				var timeFilter = (StlItemGrain == StlDataGrain.Snapshot && CollectionStartDateUtc == DateTime.MinValue) ?
					string.Empty :
					$@"{TransactionDateUtcExpression} >= {Constants.StartDateTimeInclusiveParamName}
							AND {TransactionDateUtcExpression} < {Constants.EndDateTimeExclusiveParamName}";
				var whereClauseFormatted = AddConditionToWhereClause(string.Empty, timeFilter);
				return AddConditionToWhereClause(whereClauseFormatted, WhereClause);
			}
		}

		string AddConditionToWhereClause(string currentWhereClause, string conditionToAdd)
		{
			if (string.IsNullOrWhiteSpace(conditionToAdd))
			{
				return currentWhereClause;
			}

			if (string.IsNullOrWhiteSpace(currentWhereClause))
			{
				return @"WHERE
							" + conditionToAdd;
			}

			return currentWhereClause + @"
							AND " + conditionToAdd;
		}

		int IStlScript.TimeoutSecs => (StlItemGrain == StlDataGrain.Transactional) ? 1200 : 3600;

		static string SubstituteNullOrEmpty(string value, string substitute) => string.IsNullOrEmpty(value) ? substitute : value;

		#endregion

		protected abstract string FeatureCode { get; }
		protected abstract string RoleName { get; }
		protected abstract string ModuleName { get; }
		protected abstract string FunctionName { get; }
		protected abstract string FeatureName { get; }

		protected virtual StlDataGrain StlItemGrain { get { return StlDataGrain.Transactional; } }

		protected abstract string TransactionCompanyExpression { get; }
		protected virtual string CompanyNameExpression { get { return null; } }
		protected abstract string BranchCodeExpression { get; }
		protected abstract string TransactionDateUtcExpression { get; }
		protected virtual string CreateUserCodeExpression { get { return null; } }

		protected abstract string TransactionGuidReference { get; }
		protected abstract string TransactionReference01 { get; }
		protected virtual string TransactionReference02 { get { return null; } }
		protected virtual string TransactionReference03 { get { return null; } }
		protected virtual string TransactionReference04 { get { return null; } }
		protected virtual string AdditionalRefs { get { return null; } }
		protected virtual string TransactionCountExpression { get { return "1"; } }

		protected virtual string PreparationScript { get { return null; } }
		protected abstract string FromClause { get; }
		protected abstract string WhereClause { get; }

		protected virtual bool WithOptionRecompile { get { return false; } }
		protected virtual StlDateType ScriptDateType { get { return StlDateType.DateTime; } }
		protected abstract string ActiveOn { get; }
		protected abstract string MinVersion { get; }
		protected abstract string MaxVersion { get; }
		protected abstract DateTime StartDateUtc { get; }

		public virtual bool IsMandatoryForMilestones => true;
		public virtual bool IsActive => true;

		public Exception CollectionException { get; set; }
		public bool CollectionOccurred { get; set; }
		public abstract StlCollectorType CollectorType { get; }
		public string Company => TransactionCompanyExpression;
		public string Branch => BranchCodeExpression;
		public string TransactionDateUtc => TransactionDateUtcExpression;
		public string Reference1 => TransactionReference01;
		public string Reference2 => TransactionReference02;
		public string Reference3 => TransactionReference03;
		public string Reference4 => TransactionReference04;
		public string GuidReference => TransactionGuidReference;
		string IStlScript.AdditionalRefs => AdditionalRefs;
		public string User => CreateUserCodeExpression;
		string IStlScript.ActiveOn => ActiveOn;
		public string Preparation => PreparationScript;
		public string From => FromClause;
		public string Where => WhereClause;
		public string BillableCount => TransactionCountExpression;
		public bool WithRecompile => WithOptionRecompile;
		public abstract string Name { get; }
		public string MinCW1Version => MinVersion;
		public string MaxCW1Version => MaxVersion;
		public int AdditionalRefsEncoding { get { return DoesAdditionalRefsContainMultiByteCharacter ? 1200 : 65001; } }
		public DateTime CollectionStartDateUtc => StartDateUtc;
		protected virtual bool IncludeNonserializablePropertiesInAdditionalRefs => false;
		bool DoesAdditionalRefsContainMultiByteCharacter
		{
			get
			{
				return AdditionalRefs != null && AdditionalRefs.Contains("CONVERT(NVARCHAR");
			}
		}

		string VarcharAdditionalRefsExpression
		{
			get
			{
				if (string.IsNullOrEmpty(AdditionalRefs))
				{
					return "'{}'";
				}

				var match = Regex.Match(AdditionalRefs, @"(\([\t\s\r\n]*SELECT.*FOR JSON PATH[^\)]*\))", RegexOptions.Singleline | RegexOptions.IgnoreCase);
				if (match.Success)
				{
					return match.Groups[1].Value.Trim();
				}

				var type = DoesAdditionalRefsContainMultiByteCharacter ? "NVARCHAR" : "VARCHAR";
				return $"CONVERT({type}(MAX), {AdditionalRefs})";
			}
		}

		string AppendAdditionRefsExpression()
		{
			string BuildExpression(string expression, string propName, string valueExpression) => $"JSON_MODIFY({expression}, '$.{propName}', CASE WHEN ISNULL({valueExpression}, '') <> '' THEN {valueExpression} ELSE NULL END)";

			var expression = "AdditionalRefs";
			foreach (var pair in NonSerializablePropertiesDictionary)
			{
				expression = BuildExpression(expression, pair.Key, pair.Value);
			}

			var type = DoesAdditionalRefsContainMultiByteCharacter ? "NVARCHAR" : "VARCHAR";
			return $"AdditionalRefs = CONVERT(VARBINARY(MAX), CONVERT({type}(MAX), {expression}))";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "literal String is safe to use in this Context")]
		internal static readonly Dictionary<string, string> NonSerializablePropertiesDictionary = new Dictionary<string, string>()
		{
			{ "Reference1", "TransactionReference01" },
			{ "Reference2", "TransactionReference02" },
			{ "Reference3", "TransactionReference03" },
			{ "Reference4", "TransactionReference04" },
			{ "Reference5", "TransactionGuidReference" },
			{ "ClientStaffCode", "max(TransactionUser)" },
		};
	}
}
