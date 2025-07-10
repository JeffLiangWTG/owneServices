using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Registry;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;
using JobDeclaration = Enterprise.Customs.FR.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.FR.Module
{
	public class EntryHeaderFilterBusinessObject : Customs.Module.EntryHeaderFilterBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public static class FilterConstants
		{
			public const string DeltaAgreement = "Delta Agreement(Profile Number)";
			public const string DeltaMode = "Delta Mode";
			public const string IsDeltaDOneStepSent = "Is Delta G2 One Step Sent";
			public const string IsDeltaDTwoStepSent = "Is Delta G2 Two Step Sent";
			public const string IsDeltaDStepTwoSentOKButZeroLiquidation = "Is Delta G2 Two Step Sent but 0 Liquidation";
			public const string BooleanFilterPrompt = "Ticked for yes, unticked for no";
			public const string DeltaGFallbackNumber = "Delta G Fallback Number";
			public const string DeltaGFallbackStatus = "Delta G Fallback Status";
			public const string AssessmentDate = "Assessment Date";
			public const string ExitedStatus = "Export Control Status";
			public const string ExportExitType = "Export Exit Type";
			public const string TrigPointForVal = "VAA Trig. Point";
			public const string CorrelationID = "LRN/Correlation ID";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			var deltaAgreementFilter = filters.AddNumberFilter(FilterConstants.DeltaAgreement, JobDeclarationSchema.JE_CustomsProfile);
			deltaAgreementFilter.SubGroup = JobDeclarationSubGroup;
			deltaAgreementFilter.Category = FilterCategories.NumbersAndReferences;
			deltaAgreementFilter.MultilingualDescription = ResString.GetMultilingualString("AF18662D-721B-4269-967D-B51B9D6B80BC", FilterConstants.DeltaAgreement);

			var deltaModeFilter = filters.AddTextFilter(FilterConstants.DeltaMode, JobDeclarationFilterQuery.GetDeltaModeQuery, DeltaModeList);
			deltaModeFilter.SubGroup = JobDeclarationSubGroup;
			deltaModeFilter.Category = FilterCategories.ModesAndTypes;
			deltaModeFilter.MultilingualDescription = ResString.GetMultilingualString("B6D78951-24E3-4DF6-92D9-3D3FFE8EBE7D", FilterConstants.DeltaMode);

			AddBooleanFilters(filters);

			var deltaGFallbackNumberfilter = filters.AddNumberFilter(FilterConstants.DeltaGFallbackNumber, GetDeltaGFallbackNumberQuery);
			deltaGFallbackNumberfilter.UseMultiSearch = false;
			deltaGFallbackNumberfilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			deltaGFallbackNumberfilter.MultilingualDescription = ResString.GetMultilingualString("85D2B321-425A-4FD2-9EB2-5CA87A381A79", FilterConstants.DeltaGFallbackNumber);

			var deltaGFallbackStatusfilter = filters.AddTextFilter(FilterConstants.DeltaGFallbackStatus, GetDeltaGFallbackStatusQuery);
			deltaGFallbackStatusfilter.Category = FilterCategories.StatusAndFlags;
			deltaGFallbackStatusfilter.MaxLength = CusEntryNumSchema.CE_EntryStatus.MaxLength;
			deltaGFallbackStatusfilter.MultilingualDescription = ResString.GetMultilingualString("199FF48D-FE9B-44BD-84DE-442DEDC79E95", FilterConstants.DeltaGFallbackStatus);

			var assessmentDateFilter = filters.AddDateFilter(FilterConstants.AssessmentDate, GetAssessmentDateQuery);
			assessmentDateFilter.SubGroup = EntryInstructionSubGroup;
			assessmentDateFilter.Category = FilterCategories.Dates;
			assessmentDateFilter.MultilingualDescription = ResString.GetMultilingualString("6D780F4E-B265-4DB0-93A9-9E73C827936A", FilterConstants.AssessmentDate);

			var exitedStatus = filters.AddTextFilter(FilterConstants.ExitedStatus, CusEntryHeaderSchema.CH_ExitedStatus, Factory.GetCachedValue<ExportControlStatusList>());
			exitedStatus.Category = FilterCategories.StatusAndFlags;
			exitedStatus.MaxLength = CusEntryHeaderSchema.CH_ExitedStatus.MaxLength;
			exitedStatus.MultilingualDescription = ResString.GetMultilingualString("5EF18E77-2A28-482B-A5AA-C0A218D33E92", FilterConstants.ExitedStatus);

			var trigPointForValFilter = filters.AddTextFilter(FilterConstants.TrigPointForVal, GetTrigPointForValQuery, Factory.GetCachedValue<TriggerPointsCodeList>());
			trigPointForValFilter.MultilingualDescription = ResString.GetMultilingualString("37F56110-3184-437C-A010-7079DE773D45", FilterConstants.TrigPointForVal);
			trigPointForValFilter.MaxLength = CusEntryHeader.Schema.CH_TriggeringPointForValidationMaxLength;
			trigPointForValFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			trigPointForValFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			trigPointForValFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			trigPointForValFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);

			var exportExitTypeFilter = filters.AddTextFilter(FilterConstants.ExportExitType, GetExportExitTypeQuery, Factory.GetCachedValue<ExportExitTypeList>());
			exportExitTypeFilter.Category = FilterCategories.StatusAndFlags;
			exportExitTypeFilter.MaxLength = JobDeclaration.Schema.JE_ExportExitTypeMaxLength;
			exportExitTypeFilter.MultilingualDescription = ResString.GetMultilingualString("E118ED02-F752-482F-AB00-B8F6610FEFEE", FilterConstants.ExportExitType);
			exportExitTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			exportExitTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			exportExitTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			exportExitTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);

			var correlationIdFilter = filters.AddTextFilter(FilterConstants.CorrelationID, GetCorrelationIDQuery);
			correlationIdFilter.Category = FilterCategories.NumbersAndReferences;
			correlationIdFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Contains;
			correlationIdFilter.MaxLength = CusEntryHeader.Schema.CorrelationMaxLength;
			correlationIdFilter.MultilingualDescription = ResString.GetMultilingualString("cba0955f-3040-4412-b541-ac89d889fd7b", FilterConstants.CorrelationID);

			return filters;
		}

		void AddBooleanFilters(ModuleFilterCollection filters)
		{
			var flagsFilter = filters.AddFlagsFilter(FilterConstants.IsDeltaDOneStepSent, new string[] { FilterConstants.BooleanFilterPrompt }, new GetFlagsQuery[] { GetIsDeltaDOneStepQuery });
			flagsFilter.MultilingualDescription = ResString.GetMultilingualString("54FB5B4B-76C8-4DDE-BCC9-AB89CDC0609B", FilterConstants.IsDeltaDOneStepSent);

			flagsFilter = filters.AddFlagsFilter(FilterConstants.IsDeltaDTwoStepSent, new string[] { FilterConstants.BooleanFilterPrompt }, new GetFlagsQuery[] { GetIsDeltaDTwoStepQuery });
			flagsFilter.MultilingualDescription = ResString.GetMultilingualString("C490F81E-C6FE-4E1E-86AA-7DC00234FD4A", FilterConstants.IsDeltaDTwoStepSent);

			flagsFilter = filters.AddFlagsFilter(FilterConstants.IsDeltaDStepTwoSentOKButZeroLiquidation, new string[] { FilterConstants.BooleanFilterPrompt }, new GetFlagsQuery[] { GetIsDeltaDTwoStepButZeroLiquidationQuery });
			flagsFilter.MultilingualDescription = ResString.GetMultilingualString("EC00D123-B31C-497E-A8D9-044430AB55C6", FilterConstants.IsDeltaDStepTwoSentOKButZeroLiquidation);
		}

		ZQuery GetTrigPointForValQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var additionalSql = @"CH_PK in
(SELECT a.CH_PK FROM dbo.CusEntryHeader a
INNER JOIN dbo.FRCusEntryHeader b ON a.CH_PK = b.CH_PK AND a.CH_ClusterKey = b.CH_ClusterKey
WHERE {0} {1} '{2}'
)";

			return GetEntryModuleFilterQuery(additionalSql, "CH_TriggeringPointForValidation", comparisonOperator, value);
		}

		ZQuery GetExportExitTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var additionalSql = @"CH_PK in
(SELECT CH_PK FROM dbo.CusEntryHeader WHERE CH_JE in (Select JE_PK From dbo.FRJobDeclaration Where {0} {1} '{2}'))";

			return GetEntryModuleFilterQuery(additionalSql, "JE_ExportExitType", comparisonOperator, value);
		}

		ZQuery GetEntryModuleFilterQuery(ZString sqlText, ZString columnName, SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var sqlPrefix = ZString.Empty;
			var sqlSuffix = ZString.Empty;
			var sqlOperator = "=";

			if (filterOperator == SQLComparisonOperator.StartsWith)
			{
				sqlOperator = "LIKE";
				sqlSuffix = "%";
			}
			else if (filterOperator == SQLComparisonOperator.DoesNotStartWith)
			{
				sqlOperator = "NOT LIKE";
				sqlSuffix = "%";
			}
			else if (filterOperator == SQLComparisonOperator.Contains)
			{
				sqlOperator = "LIKE";
				sqlPrefix = "%";
				sqlSuffix = "%";
			}
			else if (filterOperator == SQLComparisonOperator.NotContains)
			{
				sqlOperator = "NOT LIKE";
				sqlPrefix = "%";
				sqlSuffix = "%";
			}
			else if (filterOperator == SQLComparisonOperator.Equal)
			{
				sqlOperator = "=";
			}
			else if (filterOperator == SQLComparisonOperator.NotEqual)
			{
				sqlOperator = "<>";
			}
			else if (filterOperator == SQLComparisonOperator.IsBlank)
			{
				sqlOperator = "=";
				value = ZString.Empty;
			}
			else if (filterOperator == SQLComparisonOperator.IsNotBlank)
			{
				sqlOperator = "<>";
				value = ZString.Empty;
			}

			result.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.CurrentCulture, sqlText, columnName, sqlOperator, sqlPrefix + value + sqlSuffix), new ZSqlParameterCollection());
			return result;
		}

		ZQuery GetIsDeltaDOneStepQuery(ZBool value) => GetIsDeltaDStepQuery(EntryStatusDescriptionCodeList.Codes.ES100, value);

		ZQuery GetIsDeltaDTwoStepQuery(ZBool value) => GetIsDeltaDStepQuery(EntryStatusDescriptionCodeList.Codes.ES130, value);

		ZQuery GetIsDeltaDStepQuery(ZString entryStatus, ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var comparisonOperator = value ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;
			query.AddToFilter(CusEntryHeaderSchema.CH_EntryStatus, comparisonOperator, entryStatus);

			var declarationSubQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
			declarationSubQuery.AddSubQuery(JobDeclarationFilterQuery.GetDeltaDQuery(value), JoinCondition.And);

			var condition = value ? JoinCondition.And : JoinCondition.Or;
			query.AddSubQuery(declarationSubQuery, condition);
			return query;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "SQL strings")]
		ZQuery GetIsDeltaDTwoStepButZeroLiquidationQuery(ZBool value)
		{
			var query = GetIsDeltaDStepQuery(EntryStatusDescriptionCodeList.Codes.ES130, value) as ZDBOnlyQuery;

			var inOrnot = value ? "IN" : "NOT IN";
			var additionalSql = FormattableString.Invariant($@"{CusEntryHeaderSchema.Constants.PK} {inOrnot} 
(SELECT {CusEntryHeaderSchema.Constants.PK} FROM {CusEntryHeaderSchema.Constants.SqlSchemaName}.{CusEntryHeaderSchema.Constants.TableName}
INNER JOIN {CusEntryLineSchema.Constants.SqlSchemaName}.{CusEntryLineSchema.Constants.TableName} ON {CusEntryHeaderSchema.Constants.PK} = {CusEntryLineSchema.Constants.CL_CH}
LEFT JOIN {CusEntryLineFeeSchema.Constants.SqlSchemaName}.{CusEntryLineFeeSchema.Constants.TableName} ON {CusEntryLineFeeSchema.Constants.CF_CL} = {CusEntryLineSchema.Constants.PK}
AND
(
((SELECT COUNT(1) FROM {CusEntryLineFeeSchema.Constants.SqlSchemaName}.{CusEntryLineFeeSchema.Constants.TableName}
	WHERE {CusEntryLineFeeSchema.Constants.CF_CL} = {CusEntryLineSchema.Constants.PK} AND {CusEntryLineFeeSchema.Constants.CF_Source} = 'CUS') > 0
	AND {CusEntryLineFeeSchema.Constants.CF_Source} = 'CUS')
OR
((SELECT COUNT(1) FROM {CusEntryLineFeeSchema.Constants.SqlSchemaName}.{CusEntryLineFeeSchema.Constants.TableName}
	WHERE {CusEntryLineFeeSchema.Constants.CF_CL} = {CusEntryLineSchema.Constants.PK} AND {CusEntryLineFeeSchema.Constants.CF_Source} = 'CUS') = 0
	AND ({CusEntryLineFeeSchema.Constants.CF_Source} = 'CW1' OR {CusEntryLineFeeSchema.Constants.CF_Source} IS NULL))
)
GROUP BY {CusEntryHeaderSchema.Constants.PK}
HAVING SUM(ISNULL({CusEntryLineFeeSchema.Constants.CF_ChargeAmount}, 0)) = 0)");

			var condition = value ? JoinCondition.And : JoinCondition.Or;
			query.AddFilterAndZSQLParameterCollection(additionalSql, null, condition);

			return query;
		}

		ZQuery GetDeltaGFallbackNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var fallbackEntryNumSubQuery = GetEntryNumberQueryOfParentTableAndEntryTypeForEntryNum(comparisonOperator, value, CusEntryHeaderSchema.Constants.TableName, CusEntryNumberTypes.France.Fallback, Core.Constants.CountryCodes.France);
			query.AddSubQuery(fallbackEntryNumSubQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetDeltaGFallbackStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var fallbackEntryNumSubQuery = GetEntryNumberQueryOfParentTableAndEntryTypeForEntryStatus(comparisonOperator, value, CusEntryHeaderSchema.Constants.TableName, CusEntryNumberTypes.France.Fallback, Core.Constants.CountryCodes.France);
			query.AddSubQuery(fallbackEntryNumSubQuery, JoinCondition.And);
			return query;
		}

		protected override ZQuery GetEntryTypeFilter(string entryType, bool useOriginalEntryType = true)
		{
			if (useOriginalEntryType)
			{
				return base.GetEntryTypeFilter(entryType, useOriginalEntryType);
			}
			else
			{
				var query = new ZDBOnlyQuery(typeof(CusEntryNumber));

				var additionalSql = FormattableString.Invariant($@"{CusEntryNumSchema.Constants.CE_EntryType} IN
(SELECT {JobDeclarationSchema.Constants.JE_MessageType} FROM {JobDeclarationSchema.Constants.SqlSchemaName}.{JobDeclarationSchema.Constants.TableName}
INNER JOIN {CusEntryHeaderSchema.Constants.SqlSchemaName}.{CusEntryHeaderSchema.Constants.TableName}
ON {CusEntryHeaderSchema.Constants.CH_JE} = {JobDeclarationSchema.Constants.PK}
AND {CusEntryHeaderSchema.Constants.PK} = {CusEntryNumSchema.Constants.CE_ParentID})
");
				query.AddFilterAndZSQLParameterCollection(additionalSql, null, JoinCondition.And);
				return query;
			}
		}

		CodeDescriptionPairList DeltaModeList => Factory.GetCachedValue<OrgCusAccountDeltaGTypeList>();

		ZQuery GetAssessmentDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZQuery();
			AddDateRange(result, comparisonOperator, JoinCondition.And, CusEntryInstructionSchema.CEI_DateForDuty, value1.Date, value2.Date);
			return result;
		}

		ZQuery GetCorrelationIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var additionalSql = FormattableString.Invariant($@"{CusEntryHeaderSchema.Constants.PK} IN (
	SELECT {CusEntryHeaderSchema.Constants.PK} 
	FROM {CusEntryHeaderSchema.Constants.SqlSchemaName}.{CusEntryHeaderSchema.Constants.TableName} 
	INNER JOIN {CusEntryNumSchema.Constants.SqlSchemaName}.{CusEntryNumSchema.Constants.TableName} 
	ON {CusEntryNumSchema.Constants.CE_ParentTable} = '{CusEntryHeaderSchema.Constants.TableName}' 
	AND {CusEntryHeaderSchema.Constants.PK} = {CusEntryNumSchema.Constants.CE_ParentID}
	WHERE {CusEntryNumSchema.Constants.CE_EntryType} = '{CusEntryNumberTypes.Standard.LocalReferenceNumber}' AND {{0}} {{1}} '{{2}}')");

			return GetEntryModuleFilterQuery(additionalSql, CusEntryNumber.Schema.CE_EntryNum, comparisonOperator, value);
		}
	}
}
