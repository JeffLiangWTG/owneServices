using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(UnapprovedTransactionFilterStripBusinessObject))]
	public class UATransactionFilterStripBusinessObjectTest : AccTransactionFilterStripBusinessObjectTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		void PrepareTestData()
		{
			companyDestination = TestObjectCreator.CreateNewCompany("D");
			companySource = TestObjectCreator.CreateNewCompany("S");
			branchS = TestObjectCreator.CreateNewBranch(companySource, "S");
			branchD = TestObjectCreator.CreateNewBranch(companyDestination, "D");
			Factory.Save();

			originalUserContext = Env.CurrentUserContext;
			Env.SetUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, branchD.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			proxyD = TestObjectCreator.CreateOrgHeader("BD", true, true);
			proxyS = TestObjectCreator.CreateOrgHeader("BS", true, true);
			proxyC = TestObjectCreator.CreateOrgHeader("CO", true, true);

			branchS.GB_OH_OrgProxy = proxyS.PK;
			branchD.GB_OH_OrgProxy = proxyD.PK;

			FilterCollection = new UnapprovedTransactionCandidateCollection(Factory);
			FilterBO = (UnapprovedTransactionFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		}

		protected override void AssertComplianceDocumentRecrodFilterVisibility(bool value)
		{
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
			{
				var complianceDocumentFilter = (ModuleTextFilter)(TestFilterBizO["Compliance Document Record"]);
				AssertNull("complianceDocumentFilter should be null.", complianceDocumentFilter);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		protected override void TearDown()
		{
			if (originalUserContext != null)
			{
				Env.SetUserContext(originalUserContext);
			}
			base.TearDown();
		}

		protected override Invoice CreateNewInvoice(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<UAInvoice>();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new UnapprovedTransactionFilterStripBusinessObject();
		}

		public void TestIssuingBranchFilter()
		{
			GlbCompany activeCompnay = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch activeBranch = Factory.NewWithValidTestData<GlbBranch>();
			activeBranch.GB_GC = activeCompnay.PK;
			activeBranch.GB_IsActive = true;
			Factory.Save();

			var allBranches = new GlbBranchCollection(Factory);
			var loadedBranches = (new UnapprovedTransactionFilterStripBusinessObject()).BranchList;
			allBranches.Load();
			loadedBranches.Load();

			var allBranchPKs = allBranches.Select(x => x.PK);
			var lookUpBranchPKs = loadedBranches.Select(x => x.PK);

			AssertContainsExactElementsInAnyOrder("All Active Branches should be loaded", allBranchPKs, lookUpBranchPKs);
		}

		public void TestBranchFilterDescriptionOverride()
		{
			ModuleGuidFilter filter = (ModuleGuidFilter)TestFilterBizO["Branch"];
			AssertEquals("Description should be overriden", "Issuing Branch", filter.MultilingualDescription);
		}

		public void TestCancelledStatusFilter()
		{
			UAInvoice uaTransaction1 = Factory.NewWithValidTestData<UAInvoice>();
			UACreditNote uaTransaction2 = Factory.NewWithValidTestData<UACreditNote>();
			uaTransaction2.AH_IsCancelled = true;
			Factory.Save();

			ModuleTextFilter cancelledStatusFilter = (ModuleTextFilter)TestFilterBizO["Canceled Status"];
			cancelledStatusFilter.Property = "ALL";
			cancelledStatusFilter.IsActive = true;

			UnapprovedTransactionCandidateCollection testTransactions = new UnapprovedTransactionCandidateCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Should capture all the transactions", 2, testTransactions.Count);
			Assert("Should contain uaTransaction1", testTransactions.Contains(uaTransaction1.PK));
			Assert("SHould contain uaTransaction2", testTransactions.Contains(uaTransaction2.PK));

			cancelledStatusFilter.Property = "ACT";
			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("Should contain uaTransaction1 only", 1, testTransactions.Count);
			Assert("Should contain uaTransaction1 only", testTransactions.Contains(uaTransaction1.PK));

			cancelledStatusFilter.Property = "CAN";
			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("Should contain uaTransaction2 only", 1, testTransactions.Count);
			Assert("Should contain uaTransaction2 only", testTransactions.Contains(uaTransaction2.PK));
		}

		public void TestGetSettlementGroupQuery()
		{
			PrepareTestData();
			OrgRelatedParty dataD = Factory.NewWithValidTestData<OrgRelatedParty>();
			OrgRelatedParty dataS = Factory.NewWithValidTestData<OrgRelatedParty>();
			OrgRelatedParty dataC = Factory.NewWithValidTestData<OrgRelatedParty>();

			dataD.PR_OH_RelatedParty = proxyD.PK;
			dataD.PR_OH_Parent = proxyD.PK;
			dataD.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			dataS.PR_OH_RelatedParty = proxyS.PK;
			dataS.PR_OH_Parent = proxyS.PK;
			dataS.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			dataC.PR_OH_RelatedParty = proxyC.PK;
			dataC.PR_OH_Parent = proxyC.PK;
			dataC.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;

			UAInvoice uaInvoice = Factory.NewWithValidTestData<UAInvoice>();
			uaInvoice.AH_OH = proxyD.PK;
			UAInvoice uaInvoice2 = Factory.NewWithValidTestData<UAInvoice>();
			uaInvoice2.AH_OH = proxyS.PK;
			ARInvoice arInvoice = CreateInvoice<ARInvoice>(branchS, proxyD, false, "101", ZGuid.Empty, "S00001011", 95);

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Settlement Group"];

			filter.Property = proxyS.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Must be 2 transactions", 2, FilterCollection.Count);
			Assert("Expecting collection to contain uaInvoice2", FilterCollection.Contains(uaInvoice2));
			Assert("Expecting collection to contain arInvoice", FilterCollection.Contains(arInvoice));

			filter.Property = proxyD.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Must be 1 transaction", 1, FilterCollection.Count);
			Assert("Expecting collection to contain uaInvoice", FilterCollection.Contains(uaInvoice));

			filter.Property = proxyC.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Must be 0 transaction", 0, FilterCollection.Count);

			branchS.GB_OH_OrgProxy = ZGuid.Empty;
			companySource.GC_OH_OrgProxy = proxyC.PK;
			Factory.Save();
			filter.Property = proxyC.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Must be 1 transaction", 1, FilterCollection.Count);
			Assert("Expecting collection to contain arInvoice", FilterCollection.Contains(arInvoice));
		}

		public void TestGetSettlementGroupQuery_Case2()
		{
			PrepareTestData();
			OrgRelatedParty dataD = Factory.NewWithValidTestData<OrgRelatedParty>();
			OrgRelatedParty dataC = Factory.NewWithValidTestData<OrgRelatedParty>();

			dataD.PR_OH_RelatedParty = proxyS.PK;
			dataD.PR_OH_Parent = proxyD.PK;
			dataD.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			dataC.PR_OH_RelatedParty = proxyS.PK;
			dataC.PR_OH_Parent = proxyC.PK;
			dataC.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;

			UAInvoice uaInvoice = Factory.NewWithValidTestData<UAInvoice>();
			uaInvoice.AH_OH = proxyD.PK;
			UAInvoice uaInvoice2 = Factory.NewWithValidTestData<UAInvoice>();
			uaInvoice2.AH_OH = proxyS.PK;
			ARInvoice arInvoice = CreateInvoice<ARInvoice>(branchS, proxyD, false, "101", ZGuid.Empty, "S00001011", 95);

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Settlement Group"];

			filter.Property = proxyS.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Must be 3 transaction", 3, FilterCollection.Count);
			Assert("Expecting collection to contain uaInvoice", FilterCollection.Contains(uaInvoice));
			Assert("Expecting collection to contain uaInvoice2", FilterCollection.Contains(uaInvoice2));
			Assert("Expecting collection to contain arInvoice", FilterCollection.Contains(arInvoice));

			filter.Property = proxyD.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Must be 0 transactions", 0, FilterCollection.Count);

			filter.Property = proxyC.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Must be 0 transaction", 0, FilterCollection.Count);

			branchS.GB_OH_OrgProxy = ZGuid.Empty;
			companySource.GC_OH_OrgProxy = proxyC.PK;
			Factory.Save();
			filter.Property = proxyC.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Must be 0 transaction", 0, FilterCollection.Count);

			dataC.Delete();
			Factory.Save();
			filter.Property = proxyC.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Must be 1 transaction", 1, FilterCollection.Count);
			Assert("Expecting collection to contain arInvoice", FilterCollection.Contains(arInvoice));
		}

		public void TestGetDebtorCreditorGroupQuery()
		{
			PrepareTestData();
			OrgCreditorGroup groupD = Factory.NewWithValidTestData<OrgCreditorGroup>();
			OrgCreditorGroup groupS = Factory.NewWithValidTestData<OrgCreditorGroup>();
			OrgCreditorGroup groupC = Factory.NewWithValidTestData<OrgCreditorGroup>();

			OrgCompanyData dataD = Factory.LoadTop1<OrgCompanyData>(new ZQuery(OrgCompanyDataSchema.OB_OH, proxyD.PK));
			OrgCompanyData dataS = Factory.LoadTop1<OrgCompanyData>(new ZQuery(OrgCompanyDataSchema.OB_OH, proxyS.PK));
			OrgCompanyData dataC = Factory.LoadTop1<OrgCompanyData>(new ZQuery(OrgCompanyDataSchema.OB_OH, proxyC.PK));

			dataD.OB_OG_APCreditorGroup = groupD.PK;
			dataS.OB_OG_APCreditorGroup = groupS.PK;
			dataC.OB_OG_APCreditorGroup = groupC.PK;

			UAInvoice uaInvoice = Factory.NewWithValidTestData<UAInvoice>();
			uaInvoice.AH_OH = proxyD.PK;
			UAInvoice uaInvoice2 = Factory.NewWithValidTestData<UAInvoice>();
			uaInvoice2.AH_OH = proxyS.PK;
			ARInvoice arInvoice = CreateInvoice<ARInvoice>(branchS, proxyD, false, "101", ZGuid.Empty, "S00001011", 95);

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Creditor Group"];

			filter.Property = groupS.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Must be 2 transactions", 2, FilterCollection.Count);
			Assert("Expecting collection to contain uaInvoice2", FilterCollection.Contains(uaInvoice2));
			Assert("Expecting collection to contain arInvoice", FilterCollection.Contains(arInvoice));

			filter.Property = groupD.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Must be 1 transaction", 1, FilterCollection.Count);
			Assert("Expecting collection to contain uaInvoice", FilterCollection.Contains(uaInvoice));

			filter.Property = groupC.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Must be 0 transaction", 0, FilterCollection.Count);

			branchS.GB_OH_OrgProxy = ZGuid.Empty;
			companySource.GC_OH_OrgProxy = proxyC.PK;
			Factory.Save();
			filter.Property = groupC.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Must be 1 transaction", 1, FilterCollection.Count);
			Assert("Expecting collection to contain arInvoice", FilterCollection.Contains(arInvoice));
		}

		public void TestGetCreditorDebtorQuery()
		{
			PrepareTestData();
			proxyD.OH_IsCreditor = false;
			proxyD.OH_IsDebtor = false;
			UAInvoice uaInvoice = Factory.NewWithValidTestData<UAInvoice>();
			uaInvoice.AH_OH = proxyD.PK;
			UAInvoice uaInvoice2 = Factory.NewWithValidTestData<UAInvoice>();
			uaInvoice2.AH_OH = proxyS.PK;
			ARInvoice arInvoice = CreateInvoice<ARInvoice>(branchS, proxyD, false, "101", ZGuid.Empty, "S00001011", 95);

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Creditor/Debtor"];

			filter.Property = proxyS.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Must be 2 transactions", 2, FilterCollection.Count);
			Assert("Expecting collection to contain uaInvoice2", FilterCollection.Contains(uaInvoice2));
			Assert("Expecting collection to contain arInvoice", FilterCollection.Contains(arInvoice));

			filter.Property = proxyD.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Must be 3 transactions", 3, FilterCollection.Count);
			Assert("Expecting collection to contain uaInvoice", FilterCollection.Contains(uaInvoice));
			Assert("Expecting collection to contain arInvoice", FilterCollection.Contains(uaInvoice2));
			Assert("Expecting collection to contain arInvoice", FilterCollection.Contains(arInvoice));

			filter.Property = proxyC.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Must be 0 transaction", 0, FilterCollection.Count);

			branchS.GB_OH_OrgProxy = ZGuid.Empty;
			companySource.GC_OH_OrgProxy = proxyC.PK;
			Factory.Save();
			filter.Property = proxyC.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Must be 1 transaction", 1, FilterCollection.Count);
			Assert("Expecting collection to contain arInvoice", FilterCollection.Contains(arInvoice));
		}

		public void TestOrganisationFilterListIncludesCreditorsAndOrgProxies()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			OrgHeader creditorOrg = newFactory.NewWithValidTestData<OrgHeader>();
			creditorOrg.CompanyData.OB_IsCreditor = true;
			OrgHeader justOrg = Factory.NewWithValidTestData<OrgHeader>();
			justOrg.CompanyData.OB_IsCreditor = false;
			justOrg.CompanyData.OB_IsDebtor = false;

			creditorOrg.OH_IsActive = true;
			justOrg.OH_IsActive = true;

			newFactory.Save();

			ModuleGuidFilter debtorOrCreditorFilter = (ModuleGuidFilter)TestFilterBizO[TestFilterBizO.CreditorDebtorText];
			((BusinessObjectCollection)debtorOrCreditorFilter.List).Load();

			Assert(debtorOrCreditorFilter.List.Contains(creditorOrg));
			Assert(debtorOrCreditorFilter.List.Contains(GlbCompany.CurrentCompany.OrgProxy));
			foreach (GlbBranch branch in GlbCompany.CurrentCompany.Branches)
			{
				Assert(debtorOrCreditorFilter.List.Contains(branch.OrgProxy));
			}
			Assert(!debtorOrCreditorFilter.List.Contains(justOrg));
		}

		public void TestGetTransactionTypeQuery()
		{
			PrepareTestData();
			UAInvoice uaInvoice = Factory.NewWithValidTestData<UAInvoice>();
			UACreditNote uaCreditNote = Factory.NewWithValidTestData<UACreditNote>();
			ARInvoice arInvoice = CreateInvoice<ARInvoice>(branchS, proxyD, false, "101", ZGuid.Empty, "S00001011", 95);
			ARCreditNote arCreditNote = CreateInvoice<ARCreditNote>(branchS, proxyD, false, "115", ZGuid.Empty, "S00001016", -30);

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Transaction Type"];

			filter.Property = TransactionTypes.Invoice;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain uaInvoice", FilterCollection.Contains(uaInvoice));
			Assert("Expecting collection not to contain uaCreditNote", !FilterCollection.Contains(uaCreditNote));
			Assert("Expecting collection to contain arInvoice", FilterCollection.Contains(arInvoice));
			Assert("Expecting collection not to contain arCreditNote", !FilterCollection.Contains(arCreditNote));

			filter.Property = TransactionTypes.CreditNote;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain uaInvoice", !FilterCollection.Contains(uaInvoice));
			Assert("Expecting collection to contain uaCreditNote", FilterCollection.Contains(uaCreditNote));
			Assert("Expecting collection not to contain arInvoice", !FilterCollection.Contains(arInvoice));
			Assert("Expecting collection to contain arCreditNote", FilterCollection.Contains(arCreditNote));

			filter.Property = "ALL";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain uaInvoice", FilterCollection.Contains(uaInvoice));
			Assert("Expecting collection to contain uaCreditNote", FilterCollection.Contains(uaCreditNote));
			Assert("Expecting collection to contain arInvoice", FilterCollection.Contains(arInvoice));
			Assert("Expecting collection to contain arCreditNote", FilterCollection.Contains(arCreditNote));

			filter.Property = TransactionTypes.UAInvoice;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain uaInvoice", FilterCollection.Contains(uaInvoice));
			Assert("Expecting collection not to contain uaCreditNote", !FilterCollection.Contains(uaCreditNote));
			Assert("Expecting collection not to contain arInvoice", !FilterCollection.Contains(arInvoice));
			Assert("Expecting collection not to contain arCreditNote", !FilterCollection.Contains(arCreditNote));

			filter.Property = TransactionTypes.UACreditNote;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain uaInvoice", !FilterCollection.Contains(uaInvoice));
			Assert("Expecting collection to contain uaCreditNote", FilterCollection.Contains(uaCreditNote));
			Assert("Expecting collection not to contain arInvoice", !FilterCollection.Contains(arInvoice));
			Assert("Expecting collection not to contain arCreditNote", !FilterCollection.Contains(arCreditNote));

			filter.Property = UnapprovedTransactionFilterStripBusinessObject.SisterCompanyUAInvoice;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain uaInvoice", !FilterCollection.Contains(uaInvoice));
			Assert("Expecting collection not to contain uaCreditNote", !FilterCollection.Contains(uaCreditNote));
			Assert("Expecting collection to contain arInvoice", FilterCollection.Contains(arInvoice));
			Assert("Expecting collection not to contain arCreditNote", !FilterCollection.Contains(arCreditNote));

			filter.Property = UnapprovedTransactionFilterStripBusinessObject.SisterCompanyUACreditNote;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain uaInvoice", !FilterCollection.Contains(uaInvoice));
			Assert("Expecting collection not to contain uaCreditNote", !FilterCollection.Contains(uaCreditNote));
			Assert("Expecting collection not to contain arInvoice", !FilterCollection.Contains(arInvoice));
			Assert("Expecting collection to contain arCreditNote", FilterCollection.Contains(arCreditNote));
		}

		public void TestSupplierCostReferenceQuery()
		{
			PrepareTestData();

			ZString supplierCostReference = "ABC";

			UAInvoice uaInvoice = Factory.NewWithValidTestData<UAInvoice>();
			uaInvoice.AH_ChequeOrReference = supplierCostReference;

			UACreditNote uaCreditNote = Factory.NewWithValidTestData<UACreditNote>();
			uaCreditNote.AH_ChequeOrReference = supplierCostReference;

			ARInvoice arInvoice = CreateInvoice<ARInvoice>(branchS, proxyD, false, "101", ZGuid.Empty, "S00001011", 95);
			arInvoice.AH_ChequeOrReference = supplierCostReference;

			ARCreditNote arCreditNote = CreateInvoice<ARCreditNote>(branchS, proxyD, false, "115", ZGuid.Empty, "S00001016", -30);
			arCreditNote.AH_ChequeOrReference = supplierCostReference;

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[AccountingUtils.NumberFilterTypes.SupplierCostReference];
			filter.Property = supplierCostReference;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain uaInvoice", FilterCollection.Contains(uaInvoice));
			Assert("Expecting collection to contain uaCreditNote", FilterCollection.Contains(uaCreditNote));
			Assert("Expecting collection not to contain arInvoice", !FilterCollection.Contains(arInvoice));
			Assert("Expecting collection not to contain arCreditNote", !FilterCollection.Contains(arCreditNote));
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestReceivingJobPropertyFilters()
		{
			PrepareTestData();

			ARInvoice arInvoice = CreateInvoice<ARInvoice>(branchS, proxyD, false, "101", ZGuid.Empty, "S00001011", 95);

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var sourceJob = TestObjectCreator.CreateJobHeader();
			var destinationJob = TestObjectCreator.CreateJobHeader();

			sourceJob.JH_JobNum = "S001001";
			sourceJob.JH_GC = companySource.PK;
			sourceJob.JH_GS_NKRepOps = "SN";
			sourceJob.JH_GB = branchS.PK;
			sourceJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			destinationJob.JH_JobNum = "S001001";
			destinationJob.JH_GC = companyDestination.PK;
			destinationJob.JH_GS_NKRepOps = "RV";
			destinationJob.JH_GB = branchD.PK;
			destinationJob.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;

			arInvoice.AH_JH = sourceJob.PK;

			Factory.Save();

			ModuleNkFilter receivingOperatorfilter = (ModuleNkFilter)FilterBO["Receiving Operator"];
			receivingOperatorfilter.Property = "RV";
			receivingOperatorfilter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain arInvoice", FilterCollection.Contains(arInvoice));

			receivingOperatorfilter = (ModuleNkFilter)FilterBO["Receiving Operator"];
			receivingOperatorfilter.Property = "SN";
			receivingOperatorfilter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain arInvoice", !FilterCollection.Contains(arInvoice));

			receivingOperatorfilter.IsActive = false;

			ModuleGuidFilter receivingBranchFilter = (ModuleGuidFilter)FilterBO["Receiving Branch"];
			receivingBranchFilter.Property = branchD.PK;
			receivingBranchFilter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain arInvoice", FilterCollection.Contains(arInvoice));

			receivingBranchFilter = (ModuleGuidFilter)FilterBO["Receiving Branch"];
			receivingBranchFilter.Property = branchS.PK;
			receivingBranchFilter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain arInvoice", !FilterCollection.Contains(arInvoice));

			receivingBranchFilter.IsActive = false;

			ModuleGuidFilter receivingDepartmentFilter = (ModuleGuidFilter)FilterBO["Receiving Department"];
			receivingDepartmentFilter.Property = TestObjectCreator.NonCurrentDepartment.PK;
			receivingDepartmentFilter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain arInvoice", FilterCollection.Contains(arInvoice));

			receivingDepartmentFilter = (ModuleGuidFilter)FilterBO["Receiving Department"];
			receivingDepartmentFilter.Property = GlbDepartment.CurrentDepartment.PK;
			receivingDepartmentFilter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain arInvoice", !FilterCollection.Contains(arInvoice));
		}

		public void TestRelatedTransactionsNotPaidFilterIsNotAvailable()
		{
			ModuleFlagsFilter relatedTransactionsNotPaidFilter = ((ModuleFlagsFilter)TestFilterBizO["Related Transactions Not Paid"]);
			AssertNull(relatedTransactionsNotPaidFilter);
		}

		public new void TestBranchManagementCodeFilter()
		{
			var branch1 = GlbCompany.CurrentCompany.ActiveBranches.ToArray()[0];
			var branch2 = GlbCompany.CurrentCompany.ActiveBranches.ToArray()[1];
			var branchCode1 = branch1.GB_Code;
			var branchCode2 = branch2.GB_Code;
			branch1.GB_AccountingGroupCode = branchCode1;
			branch2.GB_AccountingGroupCode = branchCode2;
			branch1.Factory.Save();
			var codeCollection = new BranchManagementCodeDescriptionBoolCollection();
			codeCollection.Add(branchCode1, null, true);
			codeCollection.Add(branchCode2, null, true);
			AccountingMasterFilesRegistry.Instance.BranchManagementCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeCollection);

			var invoice1 = CreateInvoice<ARInvoice>(branch1, branch1.OrgProxy, false, "101", ZGuid.Empty, "S00001011", 95);
			var invoice2 = CreateInvoice<ARInvoice>(branch2, branch2.OrgProxy, false, "101", ZGuid.Empty, "S00001011", 95);

			Factory.Save();

			var branchManagementCodeFilter = (ModuleTextFilter)TestFilterBizO["Branch Management Code"];
			branchManagementCodeFilter.Property = branchCode1;
			branchManagementCodeFilter.IsActive = true;
			var collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();
			AssertContainsExactElementsInAnyOrder("Collection should Contain invoice1", new[] { invoice1 }, collection);

			branchManagementCodeFilter.Property = branchCode2;
			collection = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			collection.Load();
			AssertContainsExactElementsInAnyOrder("Collection should Contain invoice2", new[] { invoice2 }, collection);
		}

		public void TestTaxBranchFilter()
		{
			var testCompany = TestObjectCreator.CreateNewCompany("ZZZ");
			var branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("GHI", testCompany);
			var invoice1 = CreateNewInvoice(Factory);
			invoice1.AH_GB_TaxBranch = branch1.PK;
			var invoice2 = CreateNewInvoice(Factory);
			invoice2.AH_GB_TaxBranch = branch2.PK;
			var invoice3 = CreateNewInvoice(Factory);
			invoice3.AH_GB_TaxBranch = branch3.PK;

			Factory.Save();

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var testFilterBO = GetNewFilterStripBusinessObject();
			AssertNull("Should not have Tax Branch filter when the value of Registry item Enable Tax Branch Reporting is false.", (ModuleGuidFilter)testFilterBO["Tax Branch"]);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			testFilterBO = GetNewFilterStripBusinessObject();
			var taxBranchFilter = (ModuleGuidFilter)testFilterBO["Tax Branch"];
			AssertNotNull("Should have Tax Branch filter when the value of Registry item Enable Tax Branch Reporting is true.", taxBranchFilter);

			var branchList = taxBranchFilter.List as GlbBranchCollection;
			if (branchList != null)
			{
				branchList.Load();
				Assert("Branch list of Tax Branch filter should contain branch which belongs to current company", taxBranchFilter.List.Contains(branch1));
				Assert("Branch list of Tax Branch filter should contain branch which belongs to current company", taxBranchFilter.List.Contains(branch2));
				Assert("Branch list of Tax Branch filter should contain branch which doesn't belong to current company", taxBranchFilter.List.Contains(branch3));
			}

			taxBranchFilter.IsActive = true;
			taxBranchFilter.Property = branch1.PK;
			var testTransactions = new TransactionHeaderCollection(Factory, testFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should only find one invoice", 1, testTransactions.Count);
			Assert("Should only find invoice1", testTransactions.Contains(invoice1.PK));

			taxBranchFilter.Property = branch2.PK;
			testTransactions.Load(testFilterBO.Filter);
			AssertEquals("Should only find one invoice", 1, testTransactions.Count);
			Assert("Should only find invoice2", testTransactions.Contains(invoice2.PK));

			taxBranchFilter.Property = branch3.PK;
			testTransactions.Load(testFilterBO.Filter);
			AssertEquals("Should only find one invoice", 1, testTransactions.Count);
			Assert("Should only find invoice3", testTransactions.Contains(invoice3.PK));
		}

		public new void TestAgreedPaymentMethodFiltering()
		{
			Assert("Don't have the filter", true);
		}

		#region TestEInvoicingFilter

		public override void TestEInvoicingFilters()
		{
			Assert(true);
		}

		public override void TestEReportingAuthNumberFilter()
		{
			Assert("Doesn't have the filter", true);
		}

		public new void TestDebtorGroupFiltering()
		{
			PrepareTestData();

			var testCreditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			var testCreditorGroup2 = Factory.NewWithValidTestData<OrgCreditorGroup>();

			proxyD.CompanyData.OB_OG_APCreditorGroup = testCreditorGroup.PK;
			proxyS.CompanyData.OB_OG_APCreditorGroup = testCreditorGroup.PK;

			var transaction1 = CreateInvoice<ARInvoice>(branchS, proxyD, false, "101", ZGuid.Empty, "S00001011", 95);
			var transaction2 = CreateInvoice<ARInvoice>(branchD, proxyS, false, "101", ZGuid.Empty, "S00001011", 95);

			Factory.Save();

			var company_New = Factory.NewWithValidTestData<GlbCompany>();
			var branch_New = company_New.Branches.AddNew();
			var staff_New = Factory.NewWithValidTestData<GlbStaff>();
			staff_New.GS_GB_HomeBranch = branch_New.PK;
			staff_New.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;
			staff_New.GS_LoginName = "newstaff";
			Factory.Save();

			using (Env.SetTemporaryUserContext("newstaff", branch_New.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var newFactory = new BusinessObjectFactory();
				var proxyDReloaded = newFactory.Load<OrgHeader>(proxyD.PK);
				proxyD.CompanyData.OB_OG_APCreditorGroup = testCreditorGroup2.PK;

				var transaction3 = CreateInvoice<ARInvoice>(branchS, proxyD, false, "101", ZGuid.Empty, "S00001011", 95);
				newFactory.Save();
			}

			var creditorDebtorFilter = (ModuleGuidFilter)FilterBO["Creditor Group"];
			creditorDebtorFilter.Property = testCreditorGroup.PK;
			creditorDebtorFilter.IsActive = true;

			var filter = creditorDebtorFilter.Query;
			var headers = new TransactionHeaderCollection(Factory, filter);

			Factory.Save();
			headers.Load();

			AssertEquals("There should be 1 transaction from TestDebtorGroup", 1, headers.Count);
			Assert("That transaction should be transaction2", headers.Contains(transaction2));
			creditorDebtorFilter.Property = testCreditorGroup2.PK;
			filter = creditorDebtorFilter.Query;
			headers.Load(filter);

			AssertEquals("There should be 0 transaction from TestDebtorGroup2", 0, headers.Count);
		}

		public override void TestEInvoicingFiltersVisibility()
		{
			SetEInvoicingEnabled(LedgerTypes.UnapprovedPayableTransactions);
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain);
			var eInvoicingFilters = new string[]
				{ "EInvoicing Status", "EInvoicing Last Response Received UTC", "E-Reporting Batch", "E-Reporting eHub #", "E-Reporting Govt #", "E-Reporting Auth #" };

			foreach (var item in eInvoicingFilters)
			{
				AssertNull("Filter should not be available", TestFilterBizO[item]);
			}
		}

		#endregion

		UnapprovedTransactionFilterStripBusinessObject FilterBO;

		T CreateInvoice<T>(GlbBranch branch, OrgHeader org, ZBool isPostedInternal, ZString invoiceNum, ZGuid transactionGroup, ZString transactionReference, ZDecimal amount) where T : InvoicingBase
		{
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				T result = Factory.New<T>();
				result.AH_Ledger = typeof(T).Name.Substring(0, 2);
				result.AH_PostedInternal = isPostedInternal;
				result.AH_OH = org.PK;
				result.AH_TransactionNum = invoiceNum;
				result.AH_TransactionBelongsToGroup = transactionGroup;
				result.AH_TransactionReference = transactionReference;

				InvoicingLineBase line = (InvoicingLineBase)result.Lines.AddNew();
				line.AL_LineAmount = line.AL_OSAmount = amount;
				line.AL_AG = TestObjectCreator.GLHeader1.PK;

				return result;
			}
		}

		UnapprovedTransactionCandidateCollection FilterCollection;
		OrgHeader proxyD;
		OrgHeader proxyS;
		OrgHeader proxyC;
		GlbBranch branchS;
		GlbBranch branchD;
		IUserContext originalUserContext;
		GlbCompany companySource;
		GlbCompany companyDestination;
	}
}
