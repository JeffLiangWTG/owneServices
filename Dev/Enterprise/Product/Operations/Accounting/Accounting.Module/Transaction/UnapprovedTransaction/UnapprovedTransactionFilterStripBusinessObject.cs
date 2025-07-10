using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public partial class UnapprovedTransactionFilterStripBusinessObject : APTransactionFilterStripBusinessObject
	{
		internal const string SisterCompanyUACreditNote = "SUC";
		internal const string SisterCompanyUAInvoice = "SUI";

		protected override ZString[] LedgersToUse
		{
			get { return new ZString[] { ZArchitecture.Core.LedgerTypes.UnapprovedPayableTransactions, ZArchitecture.Core.LedgerTypes.AccountsReceivable }; }
		}

		protected override bool IsSingleLedger
		{
			get { return false; }
		}

		protected override bool ShouldAddRelatedTransactionsNotPaidFilter
		{
			get { return false; }
		}

		protected override bool ShouldAddEInvoicingFilter
		{
			get { return false; }
		}

		public override ZBool ShouldShowRelatedClaim
		{
			get { return false; }
		}

		protected override bool ShouldAddComplianceDocumentRecordFilter => false;

		protected override ZString[] TransactionTypesForSupplierCostReferenceFilter
		{
			get { return new ZString[] { TransactionTypes.UAInvoice, TransactionTypes.UACreditNote }; }
		}

		protected override void AddLedgerFilter(ZQuery filter)
		{
			//we don't need these filters here
		}

		protected override void AddStatusesFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter cancelledStatusFilter = filters.AddTextFilter("Canceled Status", GetCancelledStatusQuery, CancelledStatusList);
			cancelledStatusFilter.Category = FilterCategories.StatusAndFlags;
			cancelledStatusFilter.DefaultProperty = "ALL";
			cancelledStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|UnapprovedTransactionFilter|CancelledStatus", "Canceled Status");
			AddFlagsFilters(filters);
		}

		protected override void AddNumberFilters(ModuleFilterCollection filters)
		{
			AddAllNumbersFilter(filters);
			AddInternalReferenceNumberFilter(filters);
			AddJobNumberFilter(filters);
			AddTransactionNumberFilter(filters);
			AddSupplierCostReferenceFilter(filters);
		}

		protected override void AddOtherFilters(ModuleFilterCollection filters)
		{
		}

		protected override void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			base.AddOrganisationFilters(filters);

			ModuleNkFilter receivingOperatorFilter = filters.AddNkFilter("Receiving Operator", GetReceivingOperatorQuery, ModuleIDs.GlbStaff, StaffCollection);
			receivingOperatorFilter.Category = FilterCategories.Organisations;
			receivingOperatorFilter.MultilingualDescription = ResString.GetMultilingualString("36f673ca-ecd3-4a77-ad64-6b1caa807d2f", "Receiving Operator");

			ModuleGuidFilter receivingBranchFilter = filters.AddGuidFilter("Receiving Branch", ModuleIDs.GlbBranch, GetReceivingBranchQuery, BranchList);
			receivingBranchFilter.Category = FilterCategories.Organisations;
			receivingBranchFilter.MultilingualDescription = ResString.GetMultilingualString("f7e7fd23-8541-4774-9c07-13b55d5c177b", "Receiving Branch");

			ModuleGuidFilter receivingDepartmentFilter = filters.AddGuidFilter("Receiving Department", ModuleIDs.GlbDepartment, GetReceivingDepartmentQuery, AH_GEList);
			receivingDepartmentFilter.Category = FilterCategories.Organisations;
			receivingDepartmentFilter.MultilingualDescription = ResString.GetMultilingualString("1402958c-d155-4836-82d2-f4cfb81bf257", "Receiving Department");
		}

		ZQuery GetCancelledStatusQuery(ZString value)
		{
			ZQuery result = new ZQuery();
			if (value != "ALL")
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, value == "CAN");
			}
			return result;
		}

		#region Lookups

		CodeDescriptionPairList fCancelledStatusList;
		new CodeDescriptionPairList CancelledStatusList
		{
			get
			{
				if (fCancelledStatusList == null)
				{
					fCancelledStatusList = new CodeDescriptionPairList();
					fCancelledStatusList.AddPair("ACT", Res.GetString("Accounting|UnapprovedTransactionFilter|DisplayActiveTransactions", "Only list Transactions Pending Approval"));
					fCancelledStatusList.AddPair("CAN", Res.GetString("Accounting|UnapprovedTransactionFilter|DisplayCancelledTransactions", "Only list Rejected / Canceled Transactions"));
					fCancelledStatusList.AddPair("ALL", Res.GetString("Accounting|UnapprovedTransactionFilter|DisplayAllTransactions", "List All Transactions"));
				}
				return fCancelledStatusList;
			}
		}

		#endregion

		CodeDescriptionPairList fTransactionTypeList;
		protected override CodeDescriptionPairList TransactionTypeList
		{
			get
			{
				if (fTransactionTypeList == null)
				{
					fTransactionTypeList = new CodeDescriptionPairList();
					fTransactionTypeList.AddPair("ALL", Res.GetString("233a5afb-7d5a-429d-a85a-b01a4ff73f9d", "All Transactions"));
					fTransactionTypeList.AddPair(TransactionTypes.CreditNote, Res.GetString("E4DB76F0-5698-4258-9B6D-A835CF89AED9", "All Unapproved Credit Notes"));
					fTransactionTypeList.AddPair(TransactionTypes.Invoice, Res.GetString("DFCF5596-5C65-4B61-B3B9-7A6B63D730FA", "All Unapproved Invoices"));
					fTransactionTypeList.AddPair(SisterCompanyUACreditNote, Res.GetString("4266fd4a-3a22-4bf9-8b28-d516eebc98ce", "Sister Company Unapproved Credit Notes"));
					fTransactionTypeList.AddPair(SisterCompanyUAInvoice, Res.GetString("caebca9d-9d0a-4c02-b3ee-b63830e2c178", "Sister Company Unapproved Invoices"));
					fTransactionTypeList.AddPair(TransactionTypes.UACreditNote, Res.GetString("2e248a27-4cf4-4759-89ad-d0d388798f46", "Current Company Unapproved Credit Notes"));
					fTransactionTypeList.AddPair(TransactionTypes.UAInvoice, Res.GetString("24b10061-821e-4de4-9935-b1a23df38375", "Current Company Unapproved Invoices"));
				}
				return fTransactionTypeList;
			}
		}

		GlbStaffCollection StaffCollection
		{
			get
			{
				if (staffCollection == null)
				{
					staffCollection = new GlbStaffCollection(Factory);
				}
				return staffCollection;
			}
		}
		GlbStaffCollection staffCollection;

		protected override ZQuery GetTransactionTypeQuery(ZString value)
		{
			ZQuery result = new ZQuery();
			if (value == TransactionTypes.Invoice)
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
				result.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.UAInvoice);
			}
			else if (value == TransactionTypes.CreditNote)
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote);
				result.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.UACreditNote);
			}
			else if (value == TransactionTypes.UAInvoice)
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.UAInvoice);
			}
			else if (value == TransactionTypes.UACreditNote)
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.UACreditNote);
			}
			else if (value == SisterCompanyUAInvoice)
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
			}
			else if (value == SisterCompanyUACreditNote)
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote);
			}
			return result;
		}

		public override ZString CreditorDebtorText => AccountingUtils.OrganisationFilterTypes.CreditorDebtor;

		public override MultilingualString CreditorDebtorCaption
		{
			get { return ResString.GetMultilingualString("Accounting|UnapprovedTransactionFilter|CreditorDebtor", "Creditor/Debtor"); }
		}

		protected override ZString CreditorDebtorGroupText
		{
			get { return (NoResString)"Creditor Group"; }
		}

		protected override MultilingualString BranchDescription
		{
			get { return ResString.GetMultilingualString("ec00440a-bf99-4881-9d69-c17f0965b3c7", "Issuing Branch"); }
		}

		protected override ZQuery GetCreditorDebtorQuery(ZGuid oH_PK)
		{
			var result = base.GetCreditorDebtorQuery(oH_PK);

			var branchQuery = UnapprovedTransactionCandidateCollection.GetBranchSubQuery(oH_PK);
			var companyQuery = UnapprovedTransactionCandidateCollection.GetCompanySubQuery(oH_PK);
			var interCompany = UnapprovedTransactionCandidateCollection.GetBranchOrCompanySubQuery(companyQuery, branchQuery);
			result.AddToFilter(interCompany, JoinCondition.Or);

			return result;
		}

		protected override ZQuery GetAH_OHListFilter(bool isDebtor)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			subQuery.AddToFilter(isDebtor ? OrgCompanyDataSchema.OB_IsDebtor : OrgCompanyDataSchema.OB_IsCreditor, true);

			ZDBOnlySubQuery companyProxy = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.GC_OH_OrgProxy);
			companyProxy.AddToFilter(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK);

			ZDBOnlySubQuery branchProxy = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.GB_OH_OrgProxy);
			branchProxy.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);

			result.AddSubQuery(subQuery, JoinCondition.Or);
			result.AddSubQuery(companyProxy, JoinCondition.Or);
			result.AddSubQuery(branchProxy, JoinCondition.Or);

			return result;
		}

		protected override ZQuery GetDebtorCreditorGroupQuery(ZGuid debtorCreditorPK)
		{
			var orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_OG_APCreditorGroup, debtorCreditorPK);
			orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);

			var standardUA = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			standardUA.AddSubQuery(AccTransactionHeaderSchema.AH_OH, orgCompanyDataQuery, JoinCondition.And);
			standardUA.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.UnapprovedPayableTransactions);

			var branchQuery = UnapprovedTransactionCandidateCollection.GetBranchSubQuery(orgCompanyDataQuery);
			var companyQuery = UnapprovedTransactionCandidateCollection.GetCompanySubQuery(orgCompanyDataQuery);
			var interCompany = UnapprovedTransactionCandidateCollection.GetBranchOrCompanySubQuery(companyQuery, branchQuery);
			interCompany.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);

			return new ZQuery(standardUA, JoinCondition.Or, interCompany);
		}

		protected override ZQuery GetSettlementGroupQuery(ZGuid settlementGroup)
		{
			var settlementHeader = Factory.Load<OrgHeader>(settlementGroup);
			bool fallbackToMainOrg = settlementHeader != null && !settlementHeader.APSettlementGroupPK.IsValid;

			var orgRelatedPartyQuery = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent);
			orgRelatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.APSettlementGroup);
			orgRelatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, settlementGroup);
			orgRelatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);

			ZDBOnlyQuery standardUAOrgQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			standardUAOrgQuery.AddSubQuery(AccTransactionHeaderSchema.AH_OH, orgRelatedPartyQuery, JoinCondition.And);
			if (fallbackToMainOrg)
			{
				standardUAOrgQuery.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_OH, settlementGroup);
			}

			ZQuery standardUAQuery = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.UnapprovedPayableTransactions);
			standardUAQuery.AddToFilter(standardUAOrgQuery);

			var branchQuery = UnapprovedTransactionCandidateCollection.GetBranchSubQuery(orgRelatedPartyQuery);
			if (fallbackToMainOrg)
			{
				branchQuery.AddToFilter(JoinCondition.Or, GlbBranchSchema.GB_OH_OrgProxy, settlementGroup);
			}
			var companyQuery = UnapprovedTransactionCandidateCollection.GetCompanySubQuery(orgRelatedPartyQuery);
			if (fallbackToMainOrg)
			{
				companyQuery.AddToFilter(JoinCondition.Or, GlbCompanySchema.GC_OH_OrgProxy, settlementGroup);
			}
			var interCompanyQuery = UnapprovedTransactionCandidateCollection.GetBranchOrCompanySubQuery(companyQuery, branchQuery);
			interCompanyQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);

			return new ZQuery(standardUAQuery, JoinCondition.Or, interCompanyQuery);
		}

		ZQuery GetReceivingDepartmentQuery(ZGuid value)
		{
			return GetReceivingJobFilterQueries(JobHeaderSchema.JH_GE, value);
		}

		ZQuery GetReceivingBranchQuery(ZGuid value)
		{
			return GetReceivingJobFilterQueries(JobHeaderSchema.JH_GB, value);
		}

		ZQuery GetReceivingOperatorQuery(ZString value)
		{
			return GetReceivingJobFilterQueries(JobHeaderSchema.JH_GS_NKRepOps, value);
		}

		ZQuery GetReceivingJobFilterQueries(SchemaColumn column, IZType value)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			var innerSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_JobNum);
			innerSubQuery.AddToFilter(column, value);
			innerSubQuery.AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);

			var outerSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
			outerSubQuery.AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			outerSubQuery.AddSubQuery(JobHeaderSchema.JH_JobNum, innerSubQuery, JoinCondition.And);

			query.AddSubQuery(AccTransactionHeaderSchema.AH_JH, outerSubQuery, JoinCondition.And);

			return query;
		}

		protected override ZQuery GetInvoiceCompanyFilter()
		{
			return new ZQuery();
		}

		protected override void AddJobCompanyFilter(ZDBOnlySubQuery jobHeaderSubQuery)
		{
		}

		public override GlbBranchCollection BranchList
		{
			get
			{
				if (fBranchList == null)
				{
					ZQuery filter = new ZQuery();
					fBranchList = new GlbBranchCollection(Factory, filter);
				}
				return fBranchList;
			}
		}
	}
}
