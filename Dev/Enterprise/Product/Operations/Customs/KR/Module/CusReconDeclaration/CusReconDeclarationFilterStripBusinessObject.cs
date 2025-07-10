using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Module
{
	public class CusReconDeclarationFilterStripBusinessObject : FilterStripBusinessObject
	{
		public CusReconDeclarationFilterStripBusinessObject()
		{
			QueryObjectType = typeof(CusReconDeclaration);
		}
		public static class Schema
		{
			public const string JobNumber = "Job Number";
			public const string ImportEntryNumber = "Import Entry Number";
			public const string RefundApprovalNo = "Refund Approval No.";
			public const string RefundDeclarationNumber = "Refund Declaration Number";
			public const string MessageStatus = "Message Status";
			public const string EntryStatus = "Entry Status";
			public const string AcceptedDate = "Accepted Date";
			public const string RefundApprovalDate = "Refund Approval Date";
			public const string CustomsOffice = "Customs Office";
			public const string Payer = "Payer";
		}

		public CusReconDeclarationFilterLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new CusReconDeclarationFilterLookups(this);
				}
				return lookups;
			}
		}

		CusReconDeclarationFilterLookups lookups;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var jobNumberFilter = result.AddNumberFilter(Schema.JobNumber, CusReconDeclarationSchema.CRD_JobReferenceNumber);
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("CusReconDeclarationFilterStripBusinessObject|JobNumber", Schema.JobNumber);
			SetModuleNumberFilterComparisonOperatorList(jobNumberFilter);

			var refundRequestNumberFilter = result.AddNumberFilter(Schema.RefundDeclarationNumber, GetRefundRequestNumberQuery);
			refundRequestNumberFilter.MultilingualDescription = ResString.GetMultilingualString("CusReconDeclarationFilterStripBusinessObject|RefundDeclarationNumber", Schema.RefundDeclarationNumber);
			SetModuleNumberFilterComparisonOperatorList(refundRequestNumberFilter);

			var refundApprovalNumberFilter = result.AddNumberFilter(Schema.RefundApprovalNo, GetRefundApprovalNumberQuery);
			refundApprovalNumberFilter.MultilingualDescription = ResString.GetMultilingualString("CusReconDeclarationFilterStripBusinessObject|RefundApprovalNo", Schema.RefundApprovalNo);
			SetRefundApprovalNumberFilterComparisonOperatorList(refundApprovalNumberFilter);

			var importEntryNumberFilter = result.AddNumberFilter(Schema.ImportEntryNumber, GetImportEntryNumberQuery);
			importEntryNumberFilter.MultilingualDescription = ResString.GetMultilingualString("CusReconDeclarationFilterStripBusinessObject|ImportEntryNumber", Schema.ImportEntryNumber);
			SetModuleNumberFilterComparisonOperatorList(importEntryNumberFilter);

			var messageStatusFilter = result.AddTextFilter(Schema.MessageStatus, CusReconDeclarationSchema.CRD_MessageStatus, Lookups.messageStatusList);
			messageStatusFilter.MultilingualDescription = ResString.GetMultilingualString("CusReconDeclarationFilterStripBusinessObject|MessageStatus", Schema.MessageStatus);
			SetTextFilterComparisonOperatorList(messageStatusFilter);

			var entryStatusFilter = result.AddTextFilter(Schema.EntryStatus, CusReconDeclarationSchema.CRD_CustomsStatus, Lookups.entryStatusList);
			entryStatusFilter.MultilingualDescription = ResString.GetMultilingualString("CusReconDeclarationFilterStripBusinessObject|EntryStatus", Schema.EntryStatus);
			SetTextFilterComparisonOperatorList(entryStatusFilter);

			var refundDeclarationAcceptanceDate = result.AddDateFilter(Schema.AcceptedDate, GetRefundDeclarationAcceptanceDateQuery);
			refundDeclarationAcceptanceDate.MultilingualDescription = ResString.GetMultilingualString("CusReconDeclarationFilterStripBusinessObject|AcceptedDate", Schema.AcceptedDate);
			refundDeclarationAcceptanceDate.Category = FilterCategories.Dates;

			var refundApprovedDateFilter = result.AddDateFilter(Schema.RefundApprovalDate, GetRefundApprovedDateQuery);
			refundApprovedDateFilter.MultilingualDescription = ResString.GetMultilingualString("CusReconDeclarationFilterStripBusinessObject|RefundApprovalDate", Schema.RefundApprovalDate);
			refundApprovedDateFilter.Category = FilterCategories.Dates;

			var customsOfficeFilter = result.AddNkFilter(Schema.CustomsOffice, CusReconDeclarationSchema.CRD_CustomsOffice, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.CustomsOffices);
			customsOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("CusReconDeclarationFilterStripBusinessObject|CustomsOffice", Schema.CustomsOffice);
			customsOfficeFilter.Category = FilterCategories.Locations;
			customsOfficeFilter.ComparisonOperator_List.Clear();
			customsOfficeFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			customsOfficeFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
			customsOfficeFilter.ComparisonOperator_List.DefaultCode = ModuleNkFilter.ComparisonConstants.Exact;

			var payerFilter = new ModuleGuidFilterForOrg(Schema.Payer, ModuleIDs.Organisation, GetOrgAddressColumnQueryWithOperatorDelegate(CusReconDeclarationSchema.CRD_OA_DeclarantAddress), new ConsigneeCollection(Factory));
			payerFilter.MultilingualDescription = ResString.GetMultilingualString("CusReconDeclarationFilterStripBusinessObject|Payer", Schema.Payer);
			payerFilter.Category = FilterCategories.Organisations;
			payerFilter.ComparisonOperator_List.Clear();
			payerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			payerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
			payerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank));
			payerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank));
			payerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.FiltersMatch));
			payerFilter.ComparisonOperator_List.DefaultCode = ModuleNkFilter.ComparisonConstants.Exact;
			result.AddFilter(payerFilter);

			return result;
		}

		void SetModuleNumberFilterComparisonOperatorList(ModuleNumberFilter moduleNumberFilter)
		{
			moduleNumberFilter.Category = FilterCategories.NumbersAndReferences;
			moduleNumberFilter.ComparisonOperator_List.Clear();
			moduleNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			moduleNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith));
			moduleNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains));
			moduleNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
			moduleNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotStartsWith));
			moduleNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotContain));
			moduleNumberFilter.ComparisonOperator_List.DefaultCode = ModuleNumberFilter.ComparisonConstants.StartsWith;
		}

		void SetRefundApprovalNumberFilterComparisonOperatorList(ModuleNumberFilter moduleNumberFilter)
		{
			SetModuleNumberFilterComparisonOperatorList(moduleNumberFilter);
			moduleNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank));
			moduleNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank));
		}

		void SetTextFilterComparisonOperatorList(ModuleTextFilter moduleTextFilter)
		{
			moduleTextFilter.Category = FilterCategories.StatusAndFlags;
			moduleTextFilter.ComparisonOperator_List.Clear();
			moduleTextFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			moduleTextFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
			moduleTextFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank));
			moduleTextFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank));
			moduleTextFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Exact;
		}

		ZQuery GetImportEntryNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusReconDeclaration));
			var cusReconEntryQuery = new ZDBOnlySubQuery(typeof(CusReconEntry), CusReconEntrySchema.PK);
			cusReconEntryQuery.AddToFilter(CusReconEntrySchema.CRE_OriginalEntryNumber, comparisonOperator, value.KeepAlphanumericCharacters());
			result.AddSubQuery(CusReconDeclarationSchema.PK, CusReconEntrySchema.CRE_CRD, cusReconEntryQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetRefundRequestNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetCE_EntryNumByEntryType(ElectronicDocumentTypeList.Codes._5UL, comparisonOperator, value);
		}

		ZQuery GetRefundApprovalNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetCE_EntryNumByEntryType(ElectronicDocumentTypeList.Codes._5UO, comparisonOperator, value);
		}
		static ZQuery GetCE_EntryNumByEntryType(ZString entryType, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusReconDeclaration));
			var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.PK);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value.KeepAlphanumericCharacters());
			result.AddSubQuery(CusReconDeclarationSchema.PK, CusEntryNumSchema.CE_ParentID, cusEntryNumQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetRefundDeclarationAcceptanceDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			return GetCE_IssueDateByEntryType(ElectronicDocumentTypeList.Codes._5UL, comparisonOperator, value1, value2);
		}

		ZQuery GetRefundApprovedDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			return GetCE_IssueDateByEntryType(ElectronicDocumentTypeList.Codes._5UO, comparisonOperator, value1, value2);
		}
		static ZQuery GetCE_IssueDateByEntryType(ZString entryType, DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZDBOnlyQuery(typeof(CusReconDeclaration));
			var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.PK);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			cusEntryNumQuery.AddToFilter(new DateQueryBuilder(false).CreateDateTimeRange(comparisonOperator, CusEntryNumSchema.CE_IssueDate, value1, value2, false, false), JoinCondition.And);
			result.AddSubQuery(CusReconDeclarationSchema.PK, CusEntryNumSchema.CE_ParentID, cusEntryNumQuery, JoinCondition.And);

			return result;
		}

		protected override void AddInitialAuditFilters(ModuleFilterCollection filters)
		{
			base.AddInitialAuditFilters(filters);

			var createdTimeFilter = filters[FilterDescriptions.CreatedTime] as ModuleDateFilter;
			if (createdTimeFilter != null)
			{
				createdTimeFilter.Visibility = FilterVisibility.AlwaysVisible;
				createdTimeFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last3Mths;
			}
		}

		static GetGuidQueryWithOperator GetOrgAddressColumnQueryWithOperatorDelegate(SchemaColumn orgAddressColumn)
		{
			return (SQLComparisonOperator comparisonOperator, object pK) => GetOrgAddressColumnQueryWithOperator(pK, orgAddressColumn, comparisonOperator);
		}

		static ZQuery GetOrgAddressColumnQueryWithOperator(object pK, SchemaColumn orgAddressColumn, SQLComparisonOperator comparisonOperator)
		{
			var result = new ZDBOnlyQuery(typeof(CusReconDeclaration));
			if (comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				result.AddToFilter(orgAddressColumn, comparisonOperator == SQLComparisonOperator.IsBlank ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, null);
			}
			else
			{
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), orgAddressColumn);
				orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, comparisonOperator, pK);
				result.AddSubQuery(orgAddressQuery, JoinCondition.And);
			}
			return result;
		}
	}
}
