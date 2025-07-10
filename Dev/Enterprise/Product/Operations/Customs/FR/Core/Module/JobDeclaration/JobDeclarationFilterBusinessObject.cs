using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction;
using JobDeclaration = Enterprise.Customs.FR.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.FR.Module
{
	public class JobDeclarationFilterBusinessObject : EU.Module.JobDeclarationFilterBusinessObject
		, Integration.Customs.FR.IJobDeclarationFilterBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "FilterConstants")]
		public static class FilterConstants
		{
			public const string DeltaMode = "Delta Mode";
			public const string IsDeltaDOneStepSent = "Is Delta G2 One Step Sent";
			public const string IsDeltaDTwoStepSent = "Is Delta G2 Two Step Sent";
			public const string TaxesAndFees = "Total Taxes and Fees";
			public const string BooleanFilterPrompt = "Ticked for yes, unticked for no";
			public const string DeltaGFallbackNumber = "Delta G Fallback Number";
			public const string DeltaGFallbackStatus = "Delta G Fallback Status";
			public const string EntryExitedStatus = "Export Control Status";
			public const string AssessmentDate = "Assessment Date";
			public const string TrigPointForVal = "VAA Trig. Point";
			public const string ExportExitType = "Export Exit Type";
			public const string CorrelationID = "LRN/Correlation ID";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			var deltaModeFilter = filters.AddTextFilter(FilterConstants.DeltaMode, JobDeclarationFilterQuery.GetDeltaModeQuery, Lookups.DeltaModeList);
			deltaModeFilter.Category = FilterCategories.ModesAndTypes;
			deltaModeFilter.MultilingualDescription = ResString.GetMultilingualString("8F855711-344B-4340-9657-27198C0C540F", FilterConstants.DeltaMode);

			var taxesAndFeesFilter = filters.AddNumberRangeFilter(FilterConstants.TaxesAndFees, GetTaxesAndFeesQuery);
			taxesAndFeesFilter.Decimals = 2;
			taxesAndFeesFilter.PropertyType = ZCalcEditPropertyType.Decimal;
			taxesAndFeesFilter.MultilingualDescription = ResString.GetMultilingualString("B5DB2C10-E2AF-402B-8FB9-1583C3D8196D", FilterConstants.TaxesAndFees);

			AddBooleanFilters(filters);

			var deltaGFallbackNumberfilter = filters.AddNumberFilter(FilterConstants.DeltaGFallbackNumber, GetDeltaGFallbackNumberQuery);
			deltaGFallbackNumberfilter.UseMultiSearch = false;
			deltaGFallbackNumberfilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			deltaGFallbackNumberfilter.MultilingualDescription = ResString.GetMultilingualString("4F1B9EC8-8BD5-40DB-967B-4E0B6C293AA8", FilterConstants.DeltaGFallbackNumber);

			var deltaGFallbackStatusfilter = filters.AddTextFilter(FilterConstants.DeltaGFallbackStatus, GetDeltaGFallbackStatusQuery);
			deltaGFallbackStatusfilter.Category = FilterCategories.StatusAndFlags;
			deltaGFallbackStatusfilter.MaxLength = CusEntryNumSchema.CE_EntryStatus.MaxLength;
			deltaGFallbackStatusfilter.MultilingualDescription = ResString.GetMultilingualString("E90F7FCD-593E-4385-A7B4-AC8CCC7CEA53", FilterConstants.DeltaGFallbackStatus);

			var assessmentDateFilter = filters.AddDateFilter(FilterConstants.AssessmentDate, GetAssessmentDateQuery);
			assessmentDateFilter.Category = FilterCategories.Dates;
			assessmentDateFilter.MultilingualDescription = ResString.GetMultilingualString("6D780F4E-B265-4DB0-93A9-9E73C827936A", FilterConstants.AssessmentDate);

			var trigPointForValFilter = filters.AddTextFilter(FilterConstants.TrigPointForVal, GetTrigPointForValQuery, Factory.GetCachedValue<TriggerPointsCodeList>());
			trigPointForValFilter.MultilingualDescription = ResString.GetMultilingualString("37F56110-3184-437C-A010-7079DE773D45", FilterConstants.TrigPointForVal);
			trigPointForValFilter.MaxLength = CusEntryHeader.Schema.CH_TriggeringPointForValidationMaxLength;
			trigPointForValFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			trigPointForValFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			trigPointForValFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			trigPointForValFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);

			var exportExitTypeFilter = filters.AddTextFilter(FilterConstants.ExportExitType, GetExportExitTypeQuery, Factory.GetCachedValue<ExportExitTypeList>());
			exportExitTypeFilter.MultilingualDescription = ResString.GetMultilingualString("36D63B26-65D1-4AC9-9A92-9FBC2CED9E43", FilterConstants.ExportExitType);
			exportExitTypeFilter.Category = FilterCategories.StatusAndFlags;
			exportExitTypeFilter.MaxLength = JobDeclaration.Schema.JE_ExportExitTypeMaxLength;
			exportExitTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			exportExitTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			exportExitTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			exportExitTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);

			var messageSystemFilter = filters.FirstOrDefault(x => x.Description == Customs.Module.DeclarationFilterConstants.SubmitType) as ModuleTextFilter;
			if (messageSystemFilter != null)
			{
				messageSystemFilter.PropertyInfo.ValueChanged -= messageSystemPropertyFilterValueChanged;
				messageSystemFilter.PropertyInfo.ValueChanged += messageSystemPropertyFilterValueChanged;
			}

			return filters;

			void messageSystemPropertyFilterValueChanged(object sender, EventArgs e)
			{
				var messageSystemValue = messageSystemFilter.Property;
				var correlationIdValue = correlationIdFilter.Property;
				var prefix = new DeltaIEApplicationExtender().GetCorrelationIDPrefix();
				if (messageSystemValue == DeclarationApplicationCodeList.Codes.DeltaIE)
				{
					correlationIdFilter.DefaultProperty = prefix;
					if (!correlationIdValue.IsEmpty)
					{
						correlationIdFilter.Property = correlationIdValue;
					}
				}
				else
				{
					correlationIdFilter.DefaultProperty = ZString.Empty;
					if (correlationIdValue != prefix)
					{
						correlationIdFilter.Property = correlationIdValue;
					}
				}
			}
		}

		#region Lookups

		public new JobDeclarationFilterLookups Lookups => (JobDeclarationFilterLookups)base.Lookups;

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups() => new JobDeclarationFilterLookups(this);

		#endregion

		protected override ModuleTextFilter AddCusEntryHeaderFilters(ModuleFilterCollection filters)
		{
			var entryExitedStatus = filters.AddTextFilter(FilterConstants.EntryExitedStatus, GetEntryExitedStatusQuery, Factory.GetCachedValue<ExportControlStatusList>());
			entryExitedStatus.Category = FilterCategories.StatusAndFlags;
			entryExitedStatus.MaxLength = CusEntryHeaderSchema.CH_ExitedStatus.MaxLength;
			entryExitedStatus.MultilingualDescription = ResString.GetMultilingualString("573C8332-117E-46AD-828B-C0E2BCA476C7", FilterConstants.EntryExitedStatus);

			var correlationIdFilter = filters.AddTextFilter(FilterConstants.CorrelationID, GetCorrelationIDQuery);
			correlationIdFilter.Category = FilterCategories.NumbersAndReferences;
			correlationIdFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Contains;
			correlationIdFilter.MaxLength = CusEntryHeader.Schema.CorrelationMaxLength;
			correlationIdFilter.MultilingualDescription = ResString.GetMultilingualString("cba0955f-3040-4412-b541-ac89d889fd7b", FilterConstants.CorrelationID);
			this.correlationIdFilter = correlationIdFilter;

			return entryExitedStatus;
		}

		ModuleTextFilter correlationIdFilter;

		public override MultilingualString ApplicationCodeFilterCaption => ResString.GetMultilingualString("FRCustoms|DeclarationFilter|SubmitType", "Messaging System");

		ZQuery GetTrigPointForValQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var additionalSql = @"JE_PK {0} 
(SELECT CH_JE FROM dbo.FRCusEntryHeader a
WHERE {1} {2} '{3}'
)";

			return GetDeclarationModuleFilterQuery(additionalSql, "CH_TriggeringPointForValidation", comparisonOperator, value);
		}

		ZQuery GetExportExitTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var additionalSql = @"JE_PK {0} (SELECT JE_PK FROM dbo.FRJobDeclaration WHERE {1} {2} '{3}')";
			return GetDeclarationModuleFilterQuery(additionalSql, "JE_ExportExitType", comparisonOperator, value);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "SQL strings")]
		ZQuery GetDeclarationModuleFilterQuery(ZString sqlText, ZString columnName, SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var sqlPrefix = ZString.Empty;
			var sqlSuffix = ZString.Empty;
			var sqlOperator = "=";
			var inOrNot = "IN";

			if (filterOperator == SQLComparisonOperator.StartsWith)
			{
				inOrNot = "IN";
				sqlOperator = "LIKE";
				sqlSuffix = "%";
			}
			else if (filterOperator == SQLComparisonOperator.DoesNotStartWith)
			{
				inOrNot = "NOT IN";
				sqlOperator = "LIKE";
				sqlSuffix = "%";
			}
			else if (filterOperator == SQLComparisonOperator.Contains)
			{
				inOrNot = "IN";
				sqlOperator = "LIKE";
				sqlPrefix = "%";
				sqlSuffix = "%";
			}
			else if (filterOperator == SQLComparisonOperator.NotContains)
			{
				inOrNot = "NOT IN";
				sqlOperator = "LIKE";
				sqlPrefix = "%";
				sqlSuffix = "%";
			}
			else if (filterOperator == SQLComparisonOperator.Equal)
			{
				inOrNot = "IN";
				sqlOperator = "=";
			}
			else if (filterOperator == SQLComparisonOperator.NotEqual)
			{
				inOrNot = "NOT IN";
				sqlOperator = "=";
			}
			else if (filterOperator == SQLComparisonOperator.IsBlank)
			{
				inOrNot = "IN";
				sqlOperator = "=";
				value = ZString.Empty;
			}
			else if (filterOperator == SQLComparisonOperator.IsNotBlank)
			{
				inOrNot = "IN";
				sqlOperator = "<>";
				value = ZString.Empty;
			}

			result.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.CurrentCulture, sqlText, inOrNot, columnName, sqlOperator, sqlPrefix + value + sqlSuffix), new ZSqlParameterCollection());
			return result;
		}

		void AddBooleanFilters(ModuleFilterCollection filters)
		{
			var flagsFilter = filters.AddFlagsFilter(FilterConstants.IsDeltaDOneStepSent, new string[] { FilterConstants.BooleanFilterPrompt }, new GetFlagsQuery[] { GetIsDeltaDOneStepQuery });
			flagsFilter.MultilingualDescription = ResString.GetMultilingualString("4D25FAB0-9CC0-48A7-B98B-422C48DBE48A", FilterConstants.IsDeltaDOneStepSent);

			flagsFilter = filters.AddFlagsFilter(FilterConstants.IsDeltaDTwoStepSent, new string[] { FilterConstants.BooleanFilterPrompt }, new GetFlagsQuery[] { GetIsDeltaDTwoStepQuery });
			flagsFilter.MultilingualDescription = ResString.GetMultilingualString("34B6B182-27B5-40FE-AB4B-CD276ED01DAA", FilterConstants.IsDeltaDTwoStepSent);
		}

		ZQuery GetTaxesAndFeesQuery(INumericZType value, INumericZType value2)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			var additionalSql = FormattableString.Invariant($@"{JobDeclarationSchema.Constants.PK} in (
									SELECT {JobDeclarationSchema.Constants.PK} from {JobDeclarationSchema.Constants.SqlSchemaName}.{JobDeclarationSchema.Constants.TableName}
									INNER JOIN {CusEntryHeaderSchema.Constants.SqlSchemaName}.{CusEntryHeaderSchema.Constants.TableName} ON {CusEntryHeaderSchema.Constants.CH_JE} = {JobDeclarationSchema.Constants.PK} 
									GROUP BY {JobDeclarationSchema.Constants.PK}
									HAVING SUM({CusEntryHeaderSchema.Constants.CH_TotalPaid}) BETWEEN {value} AND {value2})");

			result.AddFilterAndZSQLParameterCollection(additionalSql, null);

			return result;
		}

		ZQuery GetIsDeltaDOneStepQuery(ZBool value) => GetIsDeltaDStepQuery(EntryStatusDescriptionCodeList.Codes.ES100, value);

		ZQuery GetIsDeltaDTwoStepQuery(ZBool value) => GetIsDeltaDStepQuery(EntryStatusDescriptionCodeList.Codes.ES130, value);

		ZQuery GetIsDeltaDStepQuery(ZString entryStatus, ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			var deltaModeQuery = JobDeclarationFilterQuery.GetDeltaDQuery(value);
			query.AddSubQuery(deltaModeQuery, JoinCondition.And);
			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, !value);
			entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_EntryStatus, SQLComparisonOperator.Equal, entryStatus);
			var joinCondition = value ? JoinCondition.And : JoinCondition.Or;
			query.AddSubQuery(entryHeaderQuery, joinCondition);
			return query;
		}

		ZQuery GetDeltaGFallbackNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				var entryHeaderQuery = GetDeltaGFallbackNumberSubQuery(comparisonOperator, value, true);
				query.AddSubQuery(entryHeaderQuery, JoinCondition.Or);
			}
			else if (comparisonOperator.IsNegativeSQLOperator())
			{
				var entryHeaderQuery = GetDeltaGFallbackNumberSubQuery(comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value, true);
				query.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			}
			else
			{
				var entryHeaderQuery = GetDeltaGFallbackNumberSubQuery(comparisonOperator, value, false);
				query.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			}
			return query;
		}

		ZDBOnlySubQuery GetDeltaGFallbackNumberSubQuery(SQLComparisonOperator comparisonOperator, ZString value, bool notIn)
		{
			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, notIn);
			var fallbackEntryNumSubQuery = JobDeclarationFilterQuery.GetFallbackEntryNumSubQuery();
			if (comparisonOperator != SQLComparisonOperator.IsBlank)
			{
				fallbackEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			}
			entryHeaderQuery.AddSubQuery(fallbackEntryNumSubQuery, JoinCondition.And);
			return entryHeaderQuery;
		}

		ZQuery GetDeltaGFallbackStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				var entryHeaderQuery = GetDeltaGFallbackStatusSubQuery(comparisonOperator, value, true);
				query.AddSubQuery(entryHeaderQuery, JoinCondition.Or);
			}
			else if (comparisonOperator.IsNegativeSQLOperator())
			{
				var entryHeaderQuery = GetDeltaGFallbackStatusSubQuery(comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value, true);
				query.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			}
			else
			{
				var entryHeaderQuery = GetDeltaGFallbackStatusSubQuery(comparisonOperator, value, false);
				query.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			}
			return query;
		}

		ZDBOnlySubQuery GetDeltaGFallbackStatusSubQuery(SQLComparisonOperator comparisonOperator, ZString value, bool notIn)
		{
			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, notIn);
			var fallbackEntryNumSubQuery = JobDeclarationFilterQuery.GetFallbackEntryNumSubQuery();
			if (comparisonOperator != SQLComparisonOperator.IsBlank)
			{
				fallbackEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryStatus, comparisonOperator, value);
			}
			entryHeaderQuery.AddSubQuery(fallbackEntryNumSubQuery, JoinCondition.And);
			return entryHeaderQuery;
		}

		ZQuery GetEntryExitedStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				var entryHeaderQuery = GetEntryExitedStatusSubQuery(comparisonOperator, value, true);
				query.AddSubQuery(entryHeaderQuery, JoinCondition.Or);
			}
			else if (comparisonOperator.IsNegativeSQLOperator())
			{
				var entryHeaderQuery = GetEntryExitedStatusSubQuery(comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value, true);
				query.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			}
			else
			{
				var entryHeaderQuery = GetEntryExitedStatusSubQuery(comparisonOperator, value, false);
				query.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			}
			return query;
		}

		ZDBOnlySubQuery GetEntryExitedStatusSubQuery(SQLComparisonOperator comparisonOperator, ZString value, bool notIn)
		{
			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, notIn);
			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_ExitedStatus, SQLComparisonOperator.IsNotBlank, value);
			}
			else
			{
				entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_ExitedStatus, comparisonOperator, value);
			}
			return entryHeaderQuery;
		}

		ZQuery GetAssessmentDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			var cusEntryInstructionSubQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.PK, CusEntryHeaderSchema.CH_CEI_Instruction);
			cusEntryInstructionSubQuery.AddToFilter(new DateQueryBuilder().CreateDateRange(comparisonOperator, CusEntryInstructionSchema.CEI_DateForDuty, value1.Date, value2.Date), JoinCondition.And);
			entryHeaderQuery.AddSubQuery(cusEntryInstructionSubQuery, JoinCondition.And);
			query.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetCorrelationIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var additionalSql = FormattableString.Invariant($@"{JobDeclarationSchema.Constants.PK} {{0}} (
	SELECT {JobDeclarationSchema.Constants.PK} 
	FROM {JobDeclarationSchema.Constants.SqlSchemaName}.{JobDeclarationSchema.Constants.TableName} 
	INNER JOIN {CusEntryHeaderSchema.Constants.SqlSchemaName}.{CusEntryHeaderSchema.Constants.TableName} 
	ON {JobDeclarationSchema.Constants.PK} = {CusEntryHeaderSchema.Constants.CH_JE}
	INNER JOIN {CusEntryNumSchema.Constants.SqlSchemaName}.{CusEntryNumSchema.Constants.TableName} 
	ON {CusEntryNumSchema.Constants.CE_ParentTable} = '{CusEntryHeaderSchema.Constants.TableName}' 
	AND {CusEntryHeaderSchema.Constants.PK} = {CusEntryNumSchema.Constants.CE_ParentID}
	WHERE {CusEntryNumSchema.Constants.CE_EntryType} = '{CusEntryNumberTypes.Standard.LocalReferenceNumber}' AND {{1}} {{2}} '{{3}}')");

			return GetDeclarationModuleFilterQuery(additionalSql, CusEntryNumber.Schema.CE_EntryNum, comparisonOperator, value);
		}
	}
}
