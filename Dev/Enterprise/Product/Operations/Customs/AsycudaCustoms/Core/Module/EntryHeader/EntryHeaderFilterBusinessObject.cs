using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Module
{
	public class EntryHeaderFilterBusinessObject : Customs.Module.EntryHeaderFilterBusinessObject
	{
		public static class EntryHeaderFilterConstants
		{
			public const string ContinuousGuarantee = "Continuous Guarantee"; // Filter types
			public const string LinkedGuarantee = "Linked Guarantee"; // Filter types
			public const string GuaranteeAmount = "Guaranteed Amount"; // Filter types
			public const string GuaranteeActivity = "Guarantee Activity"; // Filter types
			public const string GuaranteeStatus = "Guarantee Status"; // Filter types
			public const string AcquittedDate = "Acquitted Date";// Filter types
			public const string AcquitByDate = "Acquit By Date";// Filter types
			public const string TransitExpired = "Transit Expired";// Filter types
			public const string TransitEarliestExpiryDate = "Transit Earliest Expiry Date";// Filter types
			public const string TransitPermitNumber = "Transit Permit Number";// Filter types
			public const string TransitCompleted = "Transit Completed";// Filter types
			public const string DeclarationCreatedTimeUTC = "Declaration Created Time (UTC)";// Filter types
			public const string LiabilityCleared = "Liability Cleared";// Filter types
		}

		public new EntryHeaderFilterLookups Lookups => (EntryHeaderFilterLookups)base.Lookups;

		protected override Customs.Module.EntryHeaderFilterLookups GetNewLookups() => new EntryHeaderFilterLookups(this);

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();

			result.AddDateFilter(EntryHeaderFilterConstants.AcquitByDate, CusEntryHeaderSchema.CH_BondValidToDate).MultilingualDescription = ResString.GetMultilingualString("Asycuda|EntryHeaderFilter|BondValidToDate", EntryHeaderFilterConstants.AcquitByDate);
			result.AddDateFilter(EntryHeaderFilterConstants.AcquittedDate, CusEntryHeaderSchema.CH_BondAcquittedDate).MultilingualDescription = ResString.GetMultilingualString("Asycuda|EntryHeaderFilter|AcquittedDate", EntryHeaderFilterConstants.AcquittedDate);
			AddGuaranteeFilters(result);
			AddTransitFilters(result);
			var declarationCreatedTimeFilter = result.AddDateFilter(EntryHeaderFilterConstants.DeclarationCreatedTimeUTC, GetDeclarationCreatedTimeUTC);
			declarationCreatedTimeFilter.SubGroup = JobDeclarationSubGroup;
			declarationCreatedTimeFilter.MultilingualDescription = ResString.GetMultilingualString("Asycuda|EntryHeaderFilter|DeclarationCreatedTime", EntryHeaderFilterConstants.DeclarationCreatedTimeUTC);
			declarationCreatedTimeFilter.Visibility = FilterVisibility.AlwaysVisible;
			declarationCreatedTimeFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last3Mths;

			var liabilityClearedFilter = result.AddFlagsFilter(EntryHeaderFilterConstants.LiabilityCleared, new string[] { EntryHeaderFilterConstants.LiabilityCleared }, new GetFlagsQuery[] { GetLiabilityClearedQuery });
			liabilityClearedFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|LiabilityCleared", EntryHeaderFilterConstants.LiabilityCleared);

			return result;
		}

		protected override ZQuery GetEntryTypeFilter(string entryType, bool useOriginalEntryType = true) => new ZQuery();

		ZQuery GetLiabilityClearedQuery(ZBool value)
		{
			var isNegative = value ? ZBool.False : ZBool.True;
			var logSubQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, isNegative);
			logSubQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.LiabilityCleared.Code);
			logSubQuery.AddToFilter(StmALogSchema.SL_IsCancelled, ZBool.False);

			var result = new ZDBOnlyQuery(typeof(Business.CusEntryHeader));
			result.AddSubQuery(logSubQuery, JoinCondition.And);
			return result;
		}

		void AddTransitFilters(ModuleFilterCollection filters)
		{
			var expiredFilterName = ResString.GetMultilingualString("Asycuda|EntryHeaderFilter|TransitExpired", EntryHeaderFilterConstants.TransitExpired);
			var expiredFilter = filters.AddFlagsFilter(EntryHeaderFilterConstants.TransitExpired, new string[] { expiredFilterName }, new GetFlagsQuery[] { GetExpiredQuery });
			expiredFilter.MultilingualDescription = expiredFilterName;
			expiredFilter.Category = FilterCategories.StatusAndFlags;

			var completedFilterName = ResString.GetMultilingualString("Asycuda|EntryHeaderFilter|TransitCompleted", EntryHeaderFilterConstants.TransitCompleted);
			var completedFilter = filters.AddFlagsFilter(EntryHeaderFilterConstants.TransitCompleted, new string[] { completedFilterName }, new GetFlagsQuery[] { GetCompletedQuery });
			completedFilter.MultilingualDescription = completedFilterName;
			completedFilter.Category = FilterCategories.StatusAndFlags;

			var earliestExpiryDateFilter = filters.AddDateFilter(EntryHeaderFilterConstants.TransitEarliestExpiryDate, GetEarliestExpiryDate, true, true);
			earliestExpiryDateFilter.MultilingualDescription = ResString.GetMultilingualString("Asycuda|EntryHeaderFilter|TransitEarliestExpiryDate", EntryHeaderFilterConstants.TransitEarliestExpiryDate);
			earliestExpiryDateFilter.Category = FilterCategories.Dates;

			var permitNumberName = ResString.GetMultilingualString("Asycuda|EntryHeaderFilter|TransitPermitNumber", EntryHeaderFilterConstants.TransitPermitNumber);
			var permitNumberFilter = filters.AddNumberFilter(EntryHeaderFilterConstants.TransitPermitNumber, GetPermitNumberQuery);
			permitNumberFilter.MaxLength = CusEntryNumber.Schema.CE_EntryNumMaxLength;
			permitNumberFilter.MultilingualDescription = permitNumberName;
		}

		ZQuery GetDeclarationCreatedTimeUTC(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var declarationSubQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			var column = JobDeclarationSchema.JE_SystemCreateTimeUtc;
			switch (comparisonOperator)
			{
				case DateComparisonOperator.HasDateEntered:
					declarationSubQuery.AddToFilter(column, SQLComparisonOperator.IsNotBlank, ZDateTime.Empty);
					break;
				case DateComparisonOperator.HasNoDateEntered:
					declarationSubQuery.AddToFilter(column, ZDateTime.Empty);
					break;
				default:
					AddDateTimeRange(declarationSubQuery, comparisonOperator, JoinCondition.And, column, value1, value2);
					break;
			}

			return declarationSubQuery;
		}

		ZQuery GetExpiredQuery(ZBool value)
		{
			var cusEntryNumberSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusInBondMoveHeaderSchema.Constants.TableName);
			cusEntryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, Business.CusInBondMoveHeader.PermitNumberType);
			cusEntryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_ExpiryDate, SQLComparisonOperator.LessThan, ZDateTime.Today);

			var cusInBondMoveHeaderSubQuery = new ZDBOnlySubQuery(typeof(Business.CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			cusInBondMoveHeaderSubQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, Business.CusInBondMoveHeader.PermitNumberType);
			cusInBondMoveHeaderSubQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_ArrivalDate, ZDateTime.Empty);
			cusInBondMoveHeaderSubQuery.AddSubQuery(cusEntryNumberSubQuery, JoinCondition.And);

			var cusInBondHeaderSubQuery = new ZDBOnlySubQuery(typeof(Business.CusInBondHeader), CusInBondHeaderSchema.BH_ParentID);
			cusInBondHeaderSubQuery.AddToFilter(CusInBondHeaderSchema.BH_ParentTableCode, CusEntryInstructionSchema.Constants.Prefix);
			cusInBondHeaderSubQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, Business.CusInBondHeader.AsycudaEntryInstructionType);
			cusInBondHeaderSubQuery.AddSubQuery(cusInBondMoveHeaderSubQuery, JoinCondition.And);

			var isNegative = value ? ZBool.False : ZBool.True;
			var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(Business.CusEntryInstruction), CusEntryHeaderSchema.CH_CEI_Instruction, isNegative);
			entryInstructionSubQuery.AddSubQuery(cusInBondHeaderSubQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(Business.CusEntryHeader));
			result.AddSubQuery(entryInstructionSubQuery, JoinCondition.And);

			if (isNegative)
			{
				result.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_CEI_Instruction, null);
			}

			return result;
		}

		ZQuery GetCompletedQuery(ZBool value)
		{
			var cusInBondMoveHeaderNoArrivalDateSubQuery = new ZDBOnlySubQuery(typeof(Business.CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH, true);
			cusInBondMoveHeaderNoArrivalDateSubQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, Business.CusInBondMoveHeader.PermitNumberType);
			cusInBondMoveHeaderNoArrivalDateSubQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_ArrivalDate, ZDateTime.Empty);

			var cusInBondMoveHeaderSubQuery = new ZDBOnlySubQuery(typeof(Business.CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			cusInBondMoveHeaderSubQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, Business.CusInBondMoveHeader.PermitNumberType);
			cusInBondMoveHeaderSubQuery.AddSubQuery(CusInBondMoveHeaderSchema.BM_BH, CusInBondMoveHeaderSchema.BM_BH, cusInBondMoveHeaderNoArrivalDateSubQuery, JoinCondition.And);

			var cusInBondHeaderSubQuery = new ZDBOnlySubQuery(typeof(Business.CusInBondHeader), CusInBondHeaderSchema.BH_ParentID);
			cusInBondHeaderSubQuery.AddToFilter(CusInBondHeaderSchema.BH_ParentTableCode, CusEntryInstructionSchema.Constants.Prefix);
			cusInBondHeaderSubQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, Business.CusInBondHeader.AsycudaEntryInstructionType);
			cusInBondHeaderSubQuery.AddSubQuery(cusInBondMoveHeaderSubQuery, JoinCondition.And);

			var isNegative = value ? ZBool.False : ZBool.True;
			var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(Business.CusEntryInstruction), CusEntryHeaderSchema.CH_CEI_Instruction, isNegative);
			entryInstructionSubQuery.AddSubQuery(cusInBondHeaderSubQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(Business.CusEntryHeader));
			result.AddSubQuery(entryInstructionSubQuery, JoinCondition.And);

			if (isNegative)
			{
				result.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_CEI_Instruction, null);
			}

			return result;
		}

		ZQuery GetPermitNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var isNegative = comparisonOperator.IsNegativeSQLOperator();
			var negativeOperator = comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery();
			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				isNegative = true;
				negativeOperator = SpecialComparisonOperator.IsNotBlank;
			}
			else if (comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				isNegative = false;
			}

			var permitNumberQuery = GetEntryNumberQueryOfParentTableAndEntryTypeForEntryNum(negativeOperator, value, CusInBondMoveHeaderSchema.Constants.TableName, Business.CusInBondMoveHeader.PermitNumberType, ZString.Empty);

			var cusInBondMoveHeaderSubQuery = new ZDBOnlySubQuery(typeof(Business.CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			cusInBondMoveHeaderSubQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, Business.CusInBondMoveHeader.PermitNumberType);
			cusInBondMoveHeaderSubQuery.AddSubQuery(permitNumberQuery, JoinCondition.And);

			var cusInBondHeaderSubQuery = new ZDBOnlySubQuery(typeof(Business.CusInBondHeader), CusInBondHeaderSchema.BH_ParentID);
			cusInBondHeaderSubQuery.AddToFilter(CusInBondHeaderSchema.BH_ParentTableCode, CusEntryInstructionSchema.Constants.Prefix);
			cusInBondHeaderSubQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, Business.CusInBondHeader.AsycudaEntryInstructionType);
			cusInBondHeaderSubQuery.AddSubQuery(cusInBondMoveHeaderSubQuery, JoinCondition.And);

			var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(Business.CusEntryInstruction), CusEntryHeaderSchema.CH_CEI_Instruction, isNegative);
			entryInstructionSubQuery.AddSubQuery(cusInBondHeaderSubQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(Business.CusEntryHeader));
			result.AddSubQuery(entryInstructionSubQuery, JoinCondition.And);

			if (isNegative)
			{
				result.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_CEI_Instruction, null);
			}

			return result;
		}

		ZQuery GetEarliestExpiryDate(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			var query = new ZDBOnlyQuery(typeof(Business.CusEntryHeader));
			var parameters = new ZSqlParameterCollection();
			if (TryGetHavingClauseFromDateComparison(comparisonOperator, dateFrom, dateTo, parameters, StmEntityScreeningLogSchema.PJ_SystemCreateTimeUtc, out var havingClause, out var notIn))
			{
				var notInClause = notIn ? "NOT" : "";
				var orgPartyScreeningStatusQuery = FormattableString.Invariant($@"
				CH_CEI_Instruction {notInClause} IN (
SELECT BH_ParentID
FROM dbo.CusInBondHeader, CusInBondMoveHeader, CusEntryNum
WHERE
	1=1
	AND BH_ParentTableCode = @cusEntryInstructionPrefix
	AND	BH_ApplicationCode = @asycudaEntryInstructionType
	AND BH_PK = BM_BH
	AND BM_SubApplicationCode = @permitNumberType
	AND BM_ArrivalDate IS NULL
	AND BM_PK = CE_ParentID
	AND CE_ParentTable = @cusInBondMoveHeaderTableName
	AND CE_EntryType = @permitNumberType
	AND CE_ExpiryDate IS NOT NULL
GROUP BY
	BH_ParentID
    {havingClause}
)"  // This is a SQL query
);
				parameters.Add("@cusEntryInstructionPrefix", CusEntryInstructionSchema.Constants.Prefix, CusInBondHeaderSchema.BH_ParentTableCode);
				parameters.Add("@asycudaEntryInstructionType", Business.CusInBondHeader.AsycudaEntryInstructionType, CusInBondHeaderSchema.BH_ApplicationCode);
				parameters.Add("@permitNumberType", Business.CusInBondMoveHeader.PermitNumberType, CusInBondMoveHeaderSchema.BM_SubApplicationCode);
				parameters.Add("@cusInBondMoveHeaderTableName", CusInBondMoveHeaderSchema.Constants.TableName, CusEntryNumSchema.CE_ParentTable);
				parameters.Add("@dateToday", ZDateTime.Today, CusEntryNumSchema.CE_ExpiryDate);
				query.AddFilterAndZSQLParameterCollection(new ZString(orgPartyScreeningStatusQuery), parameters);
			}

			return query;
		}

		bool TryGetHavingClauseFromDateComparison(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo, ZSqlParameterCollection parameters, SchemaDateTimeColumn dateColumn, out ZString havingClause, out ZBool notIn)
		{
			var successful = true;
			notIn = false;
			havingClause = string.Empty;

			switch (comparisonOperator)
			{
				case DateComparisonOperator.HasNoDateEntered:
					notIn = true;
					break;
				case DateComparisonOperator.HasDateEntered:
					break;
				default:
					if (dateFrom.IsEmpty && dateTo.IsEmpty)
					{
						successful = false;
					}
					else if (!dateFrom.IsEmpty)
					{
						parameters.Add("@dateFrom", dateFrom, CusEntryNumSchema.CE_ExpiryDate);

						if (dateTo.IsEmpty)
						{
							havingClause = "HAVING MIN(CE_ExpiryDate) >= @dateFrom"; // SQL clause
						}
						else
						{
							parameters.Add("@dateTo", dateTo, CusEntryNumSchema.CE_ExpiryDate);
							havingClause = "HAVING MIN(CE_ExpiryDate) BETWEEN @dateFrom and @dateTo"; // SQL clause
						}
					}
					else
					{
						parameters.Add("@dateTo", dateTo, dateColumn);
						havingClause = "HAVING MIN(CE_ExpiryDate) <= @dateTo"; // SQL clause
					}

					break;
			}

			return successful;
		}

		void AddGuaranteeFilters(ModuleFilterCollection filters)
		{
			var guaranteeSubGroup = new GuaranteeSubGroup();
			var guaranteeCategory = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("EntryHeaderFilter|Guarantee", "Guarantee"));

			var guaranteeAmountFilter = filters.AddNumberRangeFilter(EntryHeaderFilterConstants.GuaranteeAmount, CusBondDetailSchema.PW_BondAmount);
			guaranteeAmountFilter.Decimals = 2;
			guaranteeAmountFilter.SubGroup = guaranteeSubGroup;
			guaranteeAmountFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|GuaranteedAmount", EntryHeaderFilterConstants.GuaranteeAmount);
			guaranteeAmountFilter.Category = guaranteeCategory;

			var guaranteeActivityFilter = filters.AddTextFilter(EntryHeaderFilterConstants.GuaranteeActivity, new GetTextQueryWithOperator((comparisonOperator, value) => GetEntryHeaderQueryOfSchemaColumn(comparisonOperator, value, CusBondDetailSchema.PW_ActivityCode)), GetGuaranteeActivityList)
													.WithMaxLengthOf<ModuleTextFilter>(CusBondDetailSchema.PW_ActivityCode);
			guaranteeActivityFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|GuaranteeActivity", EntryHeaderFilterConstants.GuaranteeActivity);
			guaranteeActivityFilter.Category = guaranteeCategory;
			guaranteeActivityFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			guaranteeActivityFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			guaranteeActivityFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			guaranteeActivityFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);

			var guaranteeStatusFilter = filters.AddTextFilter(EntryHeaderFilterConstants.GuaranteeStatus, new GetTextQueryWithOperator((comparisonOperator, value) => GetEntryHeaderQueryOfSchemaColumn(comparisonOperator, value, CusBondDetailSchema.PW_Status)), GetGuaranteeStatusList)
													.WithMaxLengthOf<ModuleTextFilter>(CusBondDetailSchema.PW_Status);
			guaranteeStatusFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|GuaranteeStatus", EntryHeaderFilterConstants.GuaranteeStatus);
			guaranteeStatusFilter.Category = guaranteeCategory;
			guaranteeStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			guaranteeStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			guaranteeStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			guaranteeStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);

			var continuousGuaranteeFilter = filters.AddFlagsFilter(EntryHeaderFilterConstants.ContinuousGuarantee, new string[] { MasterFiles.Business.GuaranteeBondTypeList.Descriptions.Continuous }, new GetFlagsQuery[] { GetContinuousGuaranteeQuery });
			continuousGuaranteeFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|ContinuousGuarantee", EntryHeaderFilterConstants.ContinuousGuarantee);
			continuousGuaranteeFilter.Category = guaranteeCategory;

			var linkedGuaranteeFilter = filters.AddGuidFilter(EntryHeaderFilterConstants.LinkedGuarantee, ModuleIDs.Customs.Guarantees, (comparisonOperator, value) => GetLinkedGuaranteeQuery(comparisonOperator, value), new CusGuaranteeHeaderCollection(Factory, new ZString[] { MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode }, Array.Empty<ZString>()));
			linkedGuaranteeFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|LinkedGuarantee", EntryHeaderFilterConstants.LinkedGuarantee);
			linkedGuaranteeFilter.Category = guaranteeCategory;
			linkedGuaranteeFilter.IsPublishedOnWeb = false;
			linkedGuaranteeFilter.ComparisonOperator_List.RemoveCode(ModuleGuidFilter.ComparisonConstants.IsBlank);
			linkedGuaranteeFilter.ComparisonOperator_List.RemoveCode(ModuleGuidFilter.ComparisonConstants.IsNotBlank);
		}

		ZQuery GetEntryHeaderQueryOfSchemaColumn(SQLComparisonOperator comparisonOperator, ZString value, SchemaColumn schemaColumn)
		{
			var isNegative = comparisonOperator.IsNegativeSQLOperator();
			if (comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				isNegative = comparisonOperator == SpecialComparisonOperator.IsBlank;
			}

			var guaranteeSubQuery = new ZDBOnlySubQuery(typeof(CusBondDetail), CusBondDetailSchema.PW_ParentID);
			guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_ParentTableCode, CusEntryInstructionSchema.Constants.Prefix);
			guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_ApplicationCode, ZString.Empty);

			var resultComparisonOperator = comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery();
			if (resultComparisonOperator == SpecialComparisonOperator.IsBlank || resultComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				guaranteeSubQuery.AddToFilter(schemaColumn, SQLComparisonOperator.NotEqual, string.Empty);
			}
			else
			{
				guaranteeSubQuery.AddToFilter(schemaColumn, resultComparisonOperator, value);
			}

			var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(Business.CusEntryInstruction), CusEntryHeaderSchema.CH_CEI_Instruction, isNegative);
			entryInstructionSubQuery.AddSubQuery(guaranteeSubQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(Business.CusEntryHeader));
			result.AddSubQuery(entryInstructionSubQuery, JoinCondition.And);

			if (isNegative)
			{
				result.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_CEI_Instruction, null);
			}

			return result;
		}

		CodeDescriptionPairList GetGuaranteeStatusList()
		{
			return new GuaranteeStatusList();
		}

		CodeDescriptionPairList GetGuaranteeActivityList()
		{
			return new MasterFiles.Business.GuaranteeActivityCodeList();
		}

		ZQuery GetContinuousGuaranteeQuery(ZBool value)
		{
			var guaranteeSubQuery = new ZDBOnlySubQuery(typeof(CusBondDetail), CusBondDetailSchema.PW_ParentID);
			guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_ParentTableCode, CusEntryInstructionSchema.Constants.Prefix);
			guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_ApplicationCode, ZString.Empty);
			guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_BondType, SQLComparisonOperator.Equal, MasterFiles.Business.GuaranteeBondTypeList.Codes.Continuous);

			var isNegative = value ? ZBool.False : ZBool.True;
			var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(Business.CusEntryInstruction), CusEntryHeaderSchema.CH_CEI_Instruction, isNegative);
			entryInstructionSubQuery.AddSubQuery(guaranteeSubQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(Business.CusEntryHeader));
			result.AddSubQuery(entryInstructionSubQuery, JoinCondition.And);

			if (isNegative)
			{
				result.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_CEI_Instruction, null);
			}

			return result;
		}

		ZQuery GetLinkedGuaranteeQuery(SQLComparisonOperator comparisonOperator, object value)
		{
			var isNegative = comparisonOperator.IsNegativeSQLOperator();
			if (comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				isNegative = comparisonOperator == SpecialComparisonOperator.IsBlank;
			}

			var linkedGuaranteeQuery = new ZDBOnlySubQuery(typeof(BaseCusGuaranteeHeader), CusBondDetailSchema.PW_CPH_Guarantee);
			linkedGuaranteeQuery.AddToFilter(CusPermitHeaderSchema.PK, SQLComparisonOperator.Equal, value);

			var guaranteeSubQuery = new ZDBOnlySubQuery(typeof(CusBondDetail), CusBondDetailSchema.PW_ParentID);
			guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_ParentTableCode, CusEntryInstructionSchema.Constants.Prefix);
			guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_ApplicationCode, ZString.Empty);
			guaranteeSubQuery.AddSubQuery(linkedGuaranteeQuery, JoinCondition.And);

			var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(Business.CusEntryInstruction), CusEntryHeaderSchema.CH_CEI_Instruction, isNegative);
			entryInstructionSubQuery.AddSubQuery(guaranteeSubQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(Business.CusEntryHeader));
			result.AddSubQuery(entryInstructionSubQuery, JoinCondition.And);

			if (isNegative)
			{
				result.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_CEI_Instruction, null);
			}

			return result;
		}

		class GuaranteeSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var guaranteeSubQuery = new ZDBOnlySubQuery(typeof(CusBondDetail), CusBondDetailSchema.PW_ParentID, CusEntryInstructionSchema.PK);
				guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_ParentTableCode, CusEntryInstructionSchema.Constants.Prefix);
				guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_ApplicationCode, ZString.Empty);
				guaranteeSubQuery.AddToFilter(filter);

				var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(Business.CusEntryInstruction), CusEntryHeaderSchema.CH_CEI_Instruction);
				entryInstructionSubQuery.AddSubQuery(guaranteeSubQuery, JoinCondition.And);

				var result = new ZDBOnlyQuery(typeof(Business.CusEntryHeader));
				result.AddSubQuery(entryInstructionSubQuery, JoinCondition.And);
				return result;
			}
		}
	}
}
