using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.CN.Business.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.CN.Business.CusEntryInstruction;

namespace Enterprise.Customs.CN.Module
{
	public class EntryHeaderFilterBusinessObject : Customs.Module.EntryHeaderFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();

			var aCDANumberFilter = result.AddNumberFilter(EntryHeaderFilterConstants.ACDANumber, GetACDANumberQuery);
			aCDANumberFilter.UseMultiSearch = false;
			aCDANumberFilter.SubGroup = new EntryInstructionCusCodeDataSubGroup();
			aCDANumberFilter.MaxLength = CusCodeDataSchema.CY_Code.MaxLength;
			aCDANumberFilter.MultilingualDescription = ResString.GetMultilingualString("09547296-D0CE-46D7-A1AF-CC22C75FD19D", EntryHeaderFilterConstants.ACDANumber);

			var declarationUnifiedNumberFilter = result.AddNumberFilter(EntryHeaderFilterConstants.DeclarationUnifiedNumber, GetDeclarationUnifiedNumberQuery);
			declarationUnifiedNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			declarationUnifiedNumberFilter.MultilingualDescription = ResString.GetMultilingualString("99783E6B-2C17-45AA-AA09-343FC112C5DF", EntryHeaderFilterConstants.DeclarationUnifiedNumber);

			var ciqNumberFilter = result.AddNumberFilter(EntryHeaderFilterConstants.CIQNumber, GetCIQNumberQuery);
			ciqNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			ciqNumberFilter.MultilingualDescription = ResString.GetMultilingualString("2DAE9891-24C5-4A55-B2FD-5D06C87E09EE", EntryHeaderFilterConstants.CIQNumber);

			var billOfLadingFilter = result.AddNumberFilter(EntryHeaderFilterConstants.BillOfLading, GetBillOfLadingQuery);
			billOfLadingFilter.SubGroup = new EntryNumberInstructionSubGroup();
			billOfLadingFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			billOfLadingFilter.MultilingualDescription = ResString.GetMultilingualString("FE74A57D-5DC4-496B-AFCA-8AD8191C2759", EntryHeaderFilterConstants.BillOfLading);

			var entryNumberSubGroup = new EntryNumberSubGroup();
			var ciqStatusFilter = result.AddTextFilter(EntryHeaderFilterConstants.CIQStatus, GetCIQStatusQuery, () => Lookups.CIQStatusList);
			ciqStatusFilter.SubGroup = entryNumberSubGroup;
			ciqStatusFilter.Category = FilterCategories.StatusAndFlags;
			ciqStatusFilter.MultilingualDescription = ResString.GetMultilingualString("A8DC3EF3-B86C-4E26-BA4E-EFDF214D8EED", EntryHeaderFilterConstants.CIQStatus);

			var declarationDateFilter = result.AddDateFilter(EntryHeaderFilterConstants.DeclarationDate, GetDeclarationDateQuery);
			declarationDateFilter.SubGroup = entryNumberSubGroup;
			declarationDateFilter.MultilingualDescription = ResString.GetMultilingualString("8019B610-1366-4E82-9D92-FDEC6838867C", EntryHeaderFilterConstants.DeclarationDate);

			var customsProcedureFilter = result.AddTextFilter(EntryHeaderFilterConstants.CustomsProcedure, GetCustomsProcedureQuery, () => Lookups.CustomsProcedureList).WithMaxLengthOf<ModuleTextFilter>(CusEntryInstructionSchema.CEI_Style);
			customsProcedureFilter.SubGroup = EntryInstructionSubGroup;
			customsProcedureFilter.Category = FilterCategories.TextSearch;
			customsProcedureFilter.MultilingualDescription = ResString.GetMultilingualString("B86A1115-DFCD-4A52-9C89-BB8FD9E91301", EntryHeaderFilterConstants.CustomsProcedure);

			var decOfficeOfEntryExitFilter = result.AddNkFilter(EntryHeaderFilterConstants.DeclarationOfficeOfEntryExit, GetEntryHeaderDecOfficeOfEntryExitFilter, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.CustomsOfficeList)
				.WithMaxLengthOf<ModuleNkFilter>(CNJobDeclarationSchema.JE_OfficeOfEntryExit);
			decOfficeOfEntryExitFilter.MultilingualDescription = ResString.GetMultilingualString("033192C3-6865-4E1B-BA52-D5DB732372C4", EntryHeaderFilterConstants.DeclarationOfficeOfEntryExit);
			decOfficeOfEntryExitFilter.Category = FilterCategories.TextSearch;

			var insManualNumFilter = result.AddTextFilter(EntryHeaderFilterConstants.EntryInstructionManualNo, GetEntryHeaderInstructionManualNumQuery);
			insManualNumFilter.MaxLength = AutoCNCusEntryInstruction.Schema.CEI_ManualNoMaxLength;
			insManualNumFilter.MultilingualDescription = ResString.GetMultilingualString("469D1324-E49C-40CF-AB3E-50FE7ED5C294", EntryHeaderFilterConstants.EntryInstructionManualNo);

			var insPackagesFilter = result.AddNumberRangeFilter(EntryHeaderFilterConstants.EntryInstructionPackages, GetEntryHeaderInstructionPacksQuery, Convert.ToByte(Math.Log10(SchemaIntColumn.MaxValue)), 0);
			insPackagesFilter.MultilingualDescription = ResString.GetMultilingualString("CBE1FBCA-5514-42B9-BB5E-BC632A1EB759", EntryHeaderFilterConstants.EntryInstructionPackages);
			insPackagesFilter.Category = FilterCategories.Other;

			var manufacturerBuyerFilter = result.AddGuidFilter(DeclarationFilterConstants.ManufacturerBuyer, ModuleIDs.Organisation, GetManufacturerBuyerQuery, new OrgHeaderCollection(Factory), new OrgHeaderCollection(Factory));
			manufacturerBuyerFilter.SubGroup = JobDeclarationSubGroup;
			manufacturerBuyerFilter.SetItemDescriptions(Res.GetData("87C56095-D762-4006-97DD-EC95DAD9F53D", "Manufacturer"), Res.GetData("B99EEDC0-9164-47FC-B28C-D724F688B32D", "Buyer"));
			manufacturerBuyerFilter.Category = FilterCategories.Organisations;
			manufacturerBuyerFilter.MultilingualDescription = ResString.GetMultilingualString("34183728-7769-4679-8552-EC579AE3CE36", DeclarationFilterConstants.ManufacturerBuyer);

			var customsOfficeFilter = result.AddNkFilter(DeclarationFilterConstants.CustomsOffice, GetCustomsOfficeQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, CNRefCusCodeListTypes.GetCustomsOfficeList(Factory, ZDateTime.Now)).WithMaxLengthOf<ModuleNkFilter>(JobDeclarationSchema.JE_CustomsOffice);
			customsOfficeFilter.SubGroup = JobDeclarationSubGroup;
			customsOfficeFilter.Category = FilterCategories.TextSearch;
			customsOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("66E4EAB6-9D51-469E-8924-4640D103CE5C", DeclarationFilterConstants.CustomsOffice);

			var totalWeightFilter = result.AddNumberRangeFilter(DeclarationFilterConstants.TotalWeight, GetTotalWeightQuery);
			totalWeightFilter.Decimals = JobComInvoiceLineSchema.JI_Weight.Scale;
			totalWeightFilter.Category = FilterCategories.Other;
			totalWeightFilter.MultilingualDescription = ResString.GetMultilingualString("1FA52382-6C0C-402A-8C56-433894CFDC26", DeclarationFilterConstants.TotalWeight);

			var archiveDateFilter = result.AddDateFilter(EntryHeaderFilterConstants.ArchiveDate, (comparisonOperator, value1, value2) => GetLogDateQuery(Events.RecordArchivedCode, comparisonOperator, value1, value2));
			archiveDateFilter.MultilingualDescription = ResString.GetMultilingualString("5F22F518-BB71-404B-8F41-6842A8E748D4", EntryHeaderFilterConstants.ArchiveDate);

			var archiveUserFilter = result.AddNkFilter(EntryHeaderFilterConstants.ArchiveUser, (value) => GetLogUserQuery(Events.RecordArchivedCode, value), ModuleIDs.GlbStaff, new GlbStaffCollection(Factory));
			archiveUserFilter.Category = FilterCategories.Organisations;
			archiveUserFilter.MultilingualDescription = ResString.GetMultilingualString("50CA3C2E-7EC4-4C70-B6B5-C3ED64431A12", EntryHeaderFilterConstants.ArchiveUser);

			var auditedDateFilter = result.AddDateFilter(EntryHeaderFilterConstants.AuditedDate, (comparisonOperator, value1, value2) => GetLogDateQuery(Events.RecordAuditedCode, comparisonOperator, value1, value2));
			auditedDateFilter.MultilingualDescription = ResString.GetMultilingualString("D2998223-A72D-49D1-9BC8-38D27647306D", EntryHeaderFilterConstants.AuditedDate);

			var auditedUserFilter = result.AddNkFilter(EntryHeaderFilterConstants.AuditedUser, (value) => GetLogUserQuery(Events.RecordAuditedCode, value), ModuleIDs.GlbStaff, new GlbStaffCollection(Factory));
			auditedUserFilter.Category = FilterCategories.Organisations;
			auditedUserFilter.MultilingualDescription = ResString.GetMultilingualString("944DFABC-B931-48A1-82A3-D04D38786947", EntryHeaderFilterConstants.AuditedUser);

			var readyForCompleteStatusFilter = result.AddTextFilter(EntryHeaderFilterConstants.ReadyForCompleteDeclaration, GetReadyForCompleteDeclarationQuery, () => ReadyForCompleteDeclarationFilterHelper.OptionsList);
			readyForCompleteStatusFilter.Category = FilterCategories.StatusAndFlags;
			readyForCompleteStatusFilter.MultilingualDescription = ResString.GetMultilingualString("01AD4B30-C528-11EB-9345-0800200C9A66", EntryHeaderFilterConstants.ReadyForCompleteDeclaration);

			return result;
		}

		ZQuery GetACDANumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZQuery(CusCodeDataSchema.CY_Data, comparisonOperator, value);
			result.AddToFilter(CusCodeDataSchema.CY_Type, Business.Constants.CusCodeDataTypes.Codes.CusAttachment);
			result.AddToFilter(CusCodeDataSchema.CY_Code, CSDDocTypeList.Codes._10000001);
			return result;
		}

		protected override EntryHeaderFilterLookups GetNewLookups() => new EntryHeaderFilterBusinessObjectLookups(this);

		public new EntryHeaderFilterBusinessObjectLookups Lookups => (EntryHeaderFilterBusinessObjectLookups)base.Lookups;

		ModelViewColumnQueryHelper<JobDeclaration> DeclarationModelViewColumnHelper => declarationModelViewColumnHelper ??= new();
		ModelViewColumnQueryHelper<JobDeclaration> declarationModelViewColumnHelper;
		string DeclarationModelView => CNJobDeclarationSchema.Constants.TableName;
		string DeclarationModelViewPK => CNJobDeclarationSchema.Constants.PK;

		ZQuery GetEntryHeaderDecOfficeOfEntryExitFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return DeclarationModelViewColumnHelper.GetModuleFilterQuery(CusEntryHeaderSchema.Constants.CH_JE, DeclarationModelViewPK, DeclarationModelView, CNJobDeclarationSchema.Constants.JE_OfficeOfEntryExit, comparisonOperator, value);
		}

		ModelViewColumnQueryHelper<CusEntryInstruction> EntryInstructionModelViewColumnHelper => entryInstructionModelViewColumnHelper ??= new();
		ModelViewColumnQueryHelper<CusEntryInstruction> entryInstructionModelViewColumnHelper;
		string EntryInstructionModelView => CNCusEntryInstructionSchema.Constants.TableName;
		string EntryInstructionModelViewPK => CNCusEntryInstructionSchema.Constants.PK;

		ZQuery GetEntryHeaderInstructionManualNumQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return EntryInstructionModelViewColumnHelper.GetModuleFilterQuery(CusEntryHeaderSchema.Constants.CH_CEI_Instruction, EntryInstructionModelViewPK, EntryInstructionModelView, CNCusEntryInstructionSchema.Constants.CEI_ManualNo, comparisonOperator, value);
		}

		ZQuery GetLogUserQuery(string eventCode, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusEntryHeader));

			var staffQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
			staffQuery.AddToFilter(GlbStaffSchema.GS_Code, value);

			var logQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
			logQuery.AddToFilter(StmALogSchema.SL_Table, CusEntryHeaderSchema.Constants.TableName);
			logQuery.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventCode);
			logQuery.AddSubQuery(StmALogSchema.SL_GS_NKUser, GlbStaffSchema.GS_Code, staffQuery, JoinCondition.And);
			result.AddSubQuery(logQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetLogDateQuery(string eventCode, DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZDBOnlyQuery(typeof(CusEntryHeader));

			var logQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
			logQuery.AddToFilter(StmALogSchema.SL_Table, CusEntryHeaderSchema.Constants.TableName);
			logQuery.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			logQuery.AddToFilter(StmALogSchema.SL_IsEstimate, false);
			logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventCode);
			AddDateTimeRange(logQuery, comparisonOperator, JoinCondition.And, StmALogSchema.SL_EventTime, value1, value2);
			result.AddSubQuery(logQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetTotalWeightQuery(INumericZType value1, INumericZType value2)
		{
			var result = new ZDBOnlyQuery(typeof(CusEntryHeader));

			var weightText = ZString.Format((NoResString)"{0} >= @Weight1 AND {0} <= @Weight2", "SUM(ISNULL(JI_Weight, 0))");
			string queryText = FormattableString.Invariant($@"
(CH_PK IN
	(SELECT CH_PK FROM dbo.CusEntryHeader
			INNER JOIN dbo.JobDeclaration ON JE_PK = CH_JE
			INNER JOIN dbo.CusEntryLine ON CL_CH = CH_PK
			INNER JOIN dbo.JobComInvoiceLine ON JI_CL = CL_PK
		GROUP BY CH_PK HAVING {weightText}

		UNION ALL

		SELECT CH_PK FROM dbo.CusEntryHeader
			INNER JOIN dbo.JobDeclaration ON JE_PK = CH_JE
			INNER JOIN dbo.CusEntryLine ON CL_CH = CH_PK
			INNER JOIN dbo.CusUnderbondDec ON BU_CL = CL_PK
			INNER JOIN dbo.JobComInvoiceLine ON JI_PK = BU_JI
		GROUP BY CH_PK HAVING {weightText}
	)
)");

			result.AddFilterAndZSQLParameterCollection(queryText, new ZSqlParameterCollection { { "@Weight1", value1, JobComInvoiceLineSchema.JI_Weight }, { "@Weight2", value2, JobComInvoiceLineSchema.JI_Weight } });
			return result;
		}

		ZQuery GetEntryHeaderInstructionPacksQuery(INumericZType value1, INumericZType value2)
		{
			return EntryInstructionModelViewColumnHelper.GetModuleFilterQuery(
				CusEntryHeaderSchema.Constants.CH_CEI_Instruction,
				EntryInstructionModelViewPK,
				EntryInstructionModelView,
				notIn: false,
				filterColumnValuePairs: new (ZString, SQLComparisonOperator, object)[]
				{
					(CNCusEntryInstructionSchema.Constants.CEI_Packages, SQLComparisonOperator.GreaterThanOrEqualTo, value1),
					(CNCusEntryInstructionSchema.Constants.CEI_Packages, SQLComparisonOperator.LessThanOrEqualTo, value2)
				});
		}

		ZQuery GetCustomsOfficeQuery(ZString value)
		{
			return new ZQuery(JobDeclarationSchema.JE_CustomsOffice, SQLComparisonOperator.Equal, value);
		}

		ZQuery GetManufacturerBuyerQuery(ZGuid manufacturer, ZGuid buyer)
		{
			var result = new ZQuery();

			if (!manufacturer.IsEmpty)
			{
				result.AddToFilter(JobDeclarationSchema.JE_OH_Manufacturer, manufacturer);
			}

			if (!buyer.IsEmpty)
			{
				result.AddToFilter(JobDeclarationSchema.JE_OH_Buyer, buyer);
			}

			return result;
		}

		ZQuery GetDeclarationUnifiedNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var entryNumberQuery = GetEntryNumberQueryOfParentTableAndEntryTypeForEntryNum(comparisonOperator, value, CusEntryHeaderSchema.Constants.TableName, CusEntryNumberTypes.China.DeclarationUnifiedNumber, Core.Constants.CountryCodes.China);

			var result = new ZDBOnlyQuery(typeof(CusEntryHeader));
			result.AddSubQuery(entryNumberQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetCIQNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var entryNumberQuery = GetEntryNumberQueryOfParentTableAndEntryTypeForEntryNum(comparisonOperator, value, CusEntryHeaderSchema.Constants.TableName, CusEntryNumberTypes.China.CIQNumber, Core.Constants.CountryCodes.China);

			var result = new ZDBOnlyQuery(typeof(CusEntryHeader));
			result.AddSubQuery(entryNumberQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetBillOfLadingQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZQuery().AddToFilter_PossiblyCommaSeparated(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			result.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.China.BillOfLading);
			return result;
		}

		ZQuery GetCIQStatusQuery(ZString value)
		{
			var result = new ZQuery(CusEntryNumSchema.CE_EntryStatus, SQLComparisonOperator.Equal, value);
			result.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.China.CIQNumber);
			return result;
		}

		ZQuery GetDeclarationDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			AddDateTimeRange(result, comparisonOperator, JoinCondition.And, CusEntryNumSchema.CE_IssueDate, value1, value2);
			return result;
		}

		ZQuery GetCustomsProcedureQuery(ZString value)
		{
			return new ZQuery(CusEntryInstructionSchema.CEI_Style, SQLComparisonOperator.Equal, value);
		}

		ZQuery GetReadyForCompleteDeclarationQuery(ZString status)
		{
			var query = new ZQuery();

			status = status.Trim().ToUpper();
			if (status == ReadyForCompleteDeclarationFilterHelper.Options.Ready.ToUpper())
			{
				query.AddToFilter(CusEntryHeaderSchema.CH_Status, SQLComparisonOperator.Equal, JobMessageStatusList.Codes.ClearedPreliminaryDeclaration);
			}
			else if (status == ReadyForCompleteDeclarationFilterHelper.Options.NotReady.ToUpper())
			{
				query.AddToFilter(CusEntryHeaderSchema.CH_Status, SQLComparisonOperator.NotEqual, JobMessageStatusList.Codes.ClearedPreliminaryDeclaration);
			}

			return query;
		}

		protected override CodeDescriptionPairList GetEntryStatusList()
		{
			return Lookups.GetEntryStatusList();
		}

		#region SubGroups

		class EntryInstructionCusCodeDataSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var codeDataQuery = new ZDBOnlySubQuery(typeof(CusCodeData), CusCodeDataSchema.CY_ParentID);
				codeDataQuery.AddToFilter(filter);

				var entryInstructionQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.PK);
				entryInstructionQuery.AddSubQuery(codeDataQuery, JoinCondition.And);

				var result = new ZDBOnlyQuery(typeof(CusEntryHeader));
				result.AddSubQuery(CusEntryHeaderSchema.CH_CEI_Instruction, CusEntryInstructionSchema.PK, entryInstructionQuery, JoinCondition.And);

				return result;
			}
		}

		class EntryNumberInstructionSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryInstructionSchema.Constants.TableName);
				entryNumberQuery.AddToFilter(filter);

				var entryInstructionQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.PK);
				entryInstructionQuery.AddSubQuery(entryNumberQuery, JoinCondition.And);

				var result = new ZDBOnlyQuery(typeof(CusEntryHeader));
				result.AddSubQuery(CusEntryHeaderSchema.CH_CEI_Instruction, CusEntryInstructionSchema.PK, entryInstructionQuery, JoinCondition.And);

				return result;
			}
		}

		#endregion
	}
}
