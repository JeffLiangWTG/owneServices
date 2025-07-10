using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoiceBatchHeaderFilterBusinessObject))]
	public class InvoiceBatchHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region AR Ledger Only

		public void TestLedgerFiltering()
		{
			APJournal testAPJournal = Factory.NewWithValidTestData<APJournal>();
			testAPJournal.AH_OH = TestOrg.PK;

			ARJournal testARJournal = Factory.NewWithValidTestData<ARJournal>();
			testARJournal.AH_OH = TestOrg.PK;

			Factory.Save();

			ZQuery query = TestARFilterBizO.Filter;
			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, query);
			testTransactions.Load();

			AssertEquals("There should only be the ARJournal in the collection", 1, testTransactions.Count);
			Assert("There should only be the ARJournal in the collection", testTransactions.Contains(testARJournal.PK));
		}

		#endregion

		#region Current Company

		public virtual void TestFilterbyCurrentCompany()
		{
			Factory.Save();

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_ConsolidatedInvoiceRef = "S00024561";
			aRInv.AH_OH = TestOrg.PK;

			ARCreditNote aRCrd = Factory.NewWithValidTestData<ARCreditNote>();
			aRCrd.AH_ConsolidatedInvoiceRef = "S00004562";
			aRCrd.AH_OH = TestOrg.PK;

			ARInvoice aRInv2 = Factory.NewWithValidTestData<ARInvoice>();
			aRInv2.AH_OH = TestOrg.PK;
			aRInv2.AH_GB = new TestObjectCreator(Factory).NonCurrentCompanyBranch.PK;
			aRInv2.AH_ConsolidatedInvoiceRef = "S00006543";

			Factory.Save();

			InvoicingBase[] transactions = Factory.Load<InvoicingBase>(TestARFilterBizO.Filter);

			AssertEquals("There should be 2 matching transactions", 2, transactions.Length);
			TransactionHeaderCollection transactionsColx = new TransactionHeaderCollection(Factory);
			transactionsColx.AddRange(transactions);
			Assert("Collection should contain ARInv", transactionsColx.Contains(aRInv));
			Assert("Collection should contain ARCrd", transactionsColx.Contains(aRCrd));
		}

		#endregion

		#region Filter after loading Layout has Organisation and Currency

		public virtual void TestFilterAfterLoadingLayoutHasOrganisationAndCurrency()
		{
			ZString localCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ParentBatchHeader.AH_OH = TestOrg.PK;
			ParentBatchHeader.AH_RX_NKTransactionCurrency = localCurrency;
			TestARFilterBizO.SetParent(ParentBatchHeader);

			if (TestARFilterBizO.ShouldAddOrganisationFilter_ForTestOnly)
			{
				AssertEquals("Organisation on the filter should be set from the Batch Header when setting Parent", TestOrg.PK, ((ModuleGuidFilter)TestARFilterBizO["Organisation"]).Property);
			}
			else
			{
				AssertNull("No Organisation filter should be added", TestARFilterBizO["Organisation"]);
			}

			AssertEquals("Currency on the Filter should be set from the Batch Header when setting Parent", localCurrency, ((ModuleNkFilter)TestARFilterBizO["Currency"]).Property);

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(TestARFilterBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			TestARFilterBizO.LoadLayout(savedLayout);

			if (TestARFilterBizO.ShouldAddOrganisationFilter_ForTestOnly)
			{
				AssertEquals("Organisation on the filter should be empty", ZGuid.Empty, ((ModuleGuidFilter)TestARFilterBizO["Organisation"]).Property);
			}
			else
			{
				AssertNull("No Organisation filter should be added", TestARFilterBizO["Organisation"]);
			}

			AssertEquals("Currency on the filter should be empty", ZString.Empty, ((ModuleNkFilter)TestARFilterBizO["Currency"]).Property);

			// Get Filter to cause Organisation and Currency set up
			ZQuery query = TestARFilterBizO.Filter;
			if (TestARFilterBizO.ShouldAddOrganisationFilter_ForTestOnly)
			{
				AssertEquals("Organisation on the filter should be set from the Batch Header when getting Filter", TestOrg.PK, ((ModuleGuidFilter)TestARFilterBizO["Organisation"]).Property);
			}
			else
			{
				AssertNull("No Organisation filter should be added", TestARFilterBizO["Organisation"]);
			}

			AssertEquals("Currency on the Filter should be set from the Batch Header when getting Filter", localCurrency, ((ModuleNkFilter)TestARFilterBizO["Currency"]).Property);
		}

		#endregion

		#region Job Type

		public void TestFilterByJobType_ShipmentType()
		{
			Factory.Save();

			SetInvoicesToTest(JobShipmentSchema.Constants.Prefix);

			ForwardingShipment shipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			shipment.JS_IsForwardRegistered = ZBool.True;

			ForwardingShipment cFSShipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			cFSShipment.JS_IsForwardRegistered = ZBool.False;
			cFSShipment.JS_IsCFSRegistered = ZBool.True;

			this.JobToBeIncluded.JH_ParentID = shipment.PK;
			this.JobToBeExcluded.JH_ParentID = cFSShipment.PK;

			Factory.Save();

			TestARFilterBizO.SetParent(ParentBatchHeader);
			ParentBatchHeader.JobTypeList[ParentBatchHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.FWD)].Value = true;

			AssertJobTypeFilterResult();
		}

		protected void AssertJobTypeFilterResult()
		{
			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 1 matching transaction", 1, transactions.Count);
			Assert(transactions.Contains(this.InvoiceTobeIncluded));
			Assert(!transactions.Contains(this.InvoiceTobeExcluded));
		}

		void SetInvoicesToTest(string tableCode)
		{
			InvoiceTobeIncluded = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			InvoiceTobeIncluded.AH_ConsolidatedInvoiceRef = "Test001";
			InvoiceTobeIncluded.AH_OH = TestOrg.PK;
			InvoiceTobeIncluded.AH_RX_NKTransactionCurrency = ParentBatchHeader.AH_RX_NKTransactionCurrency;

			InvoiceTobeExcluded = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			InvoiceTobeExcluded.AH_ConsolidatedInvoiceRef = "Test002";
			InvoiceTobeExcluded.AH_OH = TestOrg.PK;
			InvoiceTobeExcluded.AH_RX_NKTransactionCurrency = ParentBatchHeader.AH_RX_NKTransactionCurrency;

			JobToBeIncluded = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobToBeIncluded.JH_ParentTableCode = tableCode;

			JobToBeExcluded = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobToBeExcluded.JH_ParentTableCode = tableCode;

			InvoiceTobeIncluded.AH_JH = JobToBeIncluded.PK;
			InvoiceTobeExcluded.AH_JH = JobToBeExcluded.PK;

			((ModuleNkFilter)TestARFilterBizO["Currency"]).Property = ParentBatchHeader.AH_RX_NKTransactionCurrency;
		}

		public void TestFilterByJobType_CFSShipmentType()
		{
			Factory.Save();

			SetInvoicesToTest(JobShipmentSchema.Constants.Prefix);

			ForwardingShipment shipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			shipment.JS_IsForwardRegistered = ZBool.True;

			ForwardingShipment cFSShipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			cFSShipment.JS_IsForwardRegistered = ZBool.False;
			cFSShipment.JS_IsCFSRegistered = ZBool.True;

			JobToBeExcluded.JH_ParentID = shipment.PK;
			JobToBeIncluded.JH_ParentID = cFSShipment.PK;

			Factory.Save();

			TestARFilterBizO.SetParent(ParentBatchHeader);
			ParentBatchHeader.JobTypeList[ParentBatchHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.CFS)].Value = true;

			AssertJobTypeFilterResult();
		}

		public void TestFilterJobTypeByBrokerageType()
		{
			Factory.Save();

			SetInvoicesToTest(JobShipmentSchema.Constants.Prefix);

			JobToBeExcluded.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			JobToBeIncluded.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

			Factory.Save();

			TestARFilterBizO.SetParent(ParentBatchHeader);
			ParentBatchHeader.JobTypeList[ParentBatchHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.CUS)].Value = true;

			AssertJobTypeFilterResult();
		}

		public void TestFilterJobTypeByLocalTransport()
		{
			Factory.Save();

			SetInvoicesToTest(JobCartageSchema.Constants.Prefix);

			JobToBeExcluded.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			JobToBeIncluded.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;

			Factory.Save();

			TestARFilterBizO.SetParent(ParentBatchHeader);
			ParentBatchHeader.JobTypeList[ParentBatchHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.TPT)].Value = true;

			AssertJobTypeFilterResult();
		}

		public void TestFilterJobTypeByISF()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();

			SetInvoicesToTest(CusISFHeaderSchema.Constants.Prefix);

			JobToBeExcluded.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			JobToBeIncluded.JH_ParentTableCode = CusISFHeaderSchema.Constants.Prefix;

			Factory.Save();

			TestARFilterBizO.SetParent(ParentBatchHeader);
			ParentBatchHeader.JobTypeList[ParentBatchHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.ISF)].Value = true;

			AssertJobTypeFilterResult();
		}

		public void TestFilterByJobType_MiscellaneousType()
		{
			Factory.Save();

			InvoiceTobeIncluded = Factory.NewWithValidTestData<ARInvoice>();
			InvoiceTobeIncluded.AH_OH = TestOrg.PK;
			InvoiceTobeIncluded.AH_RX_NKTransactionCurrency = ParentBatchHeader.AH_RX_NKTransactionCurrency;
			InvoiceTobeExcluded = Factory.NewWithValidTestData<ARInvoice>();
			InvoiceTobeExcluded.AH_OH = TestOrg.PK;
			InvoiceTobeExcluded.AH_RX_NKTransactionCurrency = ParentBatchHeader.AH_RX_NKTransactionCurrency;

			JobToBeExcluded = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			this.JobToBeExcluded.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			this.InvoiceTobeExcluded.AH_JH = this.JobToBeExcluded.PK;

			ForwardingShipment shipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			shipment.JS_IsForwardRegistered = ZBool.True;

			JobToBeExcluded.JH_ParentID = shipment.PK;

			Factory.Save();

			TestARFilterBizO.SetParent(ParentBatchHeader);
			ParentBatchHeader.JobTypeList[ParentBatchHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;

			AssertJobTypeFilterResult();
		}

		#endregion

		#region Parent Management

		public void TestSetParentAndLoad()
		{
			InvoiceBatchHeader parent = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;
			parent.AH_RX_NKTransactionCurrency = TestObjectCreator.GBP.RX_Code;
			Factory.Save();

			BusinessObjectFactory readOnlyFactory = new BusinessObjectFactory();
			InvoiceBatchHeader batchHeaderRetrieved = readOnlyFactory.Load(typeof(InvoiceBatchHeader), parent.PK) as InvoiceBatchHeader;

			AssertEquals(TestObjectCreator.GBP.RX_Code, batchHeaderRetrieved.AH_RX_NKTransactionCurrency);
		}

		#endregion

		#region Invoice Type

		public void TestGetInvoiceModuleType()
		{
			InvoiceTypeModuleList invoiceTypeModuleList = new InvoiceTypeModuleList();

			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes.AddNew();
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes.AddNew();

			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[0].PI_Module = InvoiceTypeModuleList.Codes.CFS;
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[1].PI_Module = InvoiceTypeModuleList.Codes.FWD;

			Factory.Save();

			ModuleGuidFilter orgFilter = (ModuleGuidFilter)TestARFilterBizO["Organisation"];
			if (orgFilter != null)
			{
				orgFilter.IsActive = true;
				orgFilter.Property = TestObjectCreator.AALSHI.PK;

				AssertEquals(6, TestARFilterBizO.JobTypeList_ForTestOnly.Count);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region Lookups

		public void TestInvoiceTypeList()
		{
			AssertEquals("InvoiceTypeList count", 14, TestARFilterBizO.InvoiceTypeList_ForTestOnly.Count);
			Assert("InvoiceTypeList item ALL", TestARFilterBizO.InvoiceTypeList_ForTestOnly.IndexOf(new CodeDescriptionPair("ALL", "All Invoices")) > -1);
			Assert("InvoiceTypeList item JDB", TestARFilterBizO.InvoiceTypeList_ForTestOnly.IndexOf(new CodeDescriptionPair("JDB", "Job Related Disbursement")) > -1);
			Assert("InvoiceTypeList item FIN", TestARFilterBizO.InvoiceTypeList_ForTestOnly.IndexOfCode("DES") > -1);
			Assert("InvoiceTypeList item FIN", TestARFilterBizO.InvoiceTypeList_ForTestOnly.IndexOfCode("FIN") > -1);
			Assert("InvoiceTypeList item CUR", TestARFilterBizO.InvoiceTypeList_ForTestOnly.IndexOfCode("CUR") > -1);
			Assert("InvoiceTypeList item FRT", TestARFilterBizO.InvoiceTypeList_ForTestOnly.IndexOfCode("FRT") > -1);
			Assert("InvoiceTypeList item ITC", TestARFilterBizO.InvoiceTypeList_ForTestOnly.IndexOfCode("ITC") > -1);
			Assert("InvoiceTypeList item NLD", TestARFilterBizO.InvoiceTypeList_ForTestOnly.IndexOf(new CodeDescriptionPair("NJD", "Non Job Related Disbursement")) > -1);
			Assert("InvoiceTypeList item MSC", TestARFilterBizO.InvoiceTypeList_ForTestOnly.IndexOf(new CodeDescriptionPair("MSC", "Non Job Related Miscellaneous")) > -1);
			foreach (string code in InvoiceTypeCalculationProvider.DisbursementInvoiceTypes)
			{
				AssertEquals("Should not contain standard DisbursementInvoiceTypes", false, TestARFilterBizO.InvoiceTypeList_ForTestOnly.ContainsCode(code));
			}
			Assert("List should not contain Self Billing Types", !TestARFilterBizO.InvoiceTypeList_ForTestOnly.ContainsCode(InvoiceTypesList.Codes.SelfBillingInvoice));
			Assert("List should not contain Self Billing Types", !TestARFilterBizO.InvoiceTypeList_ForTestOnly.ContainsCode(InvoiceTypesList.Codes.SelfBillingInvoice_Batching));
		}

		public void TestShipmentTypeList()
		{
			GlbCompany.CurrentCompany.SetCountry("LB");
			AssertEquals("ShipmentTypeList count", 3, TestARFilterBizO.ShipmentType_List_ForTestOnly.Count);
			AssertEquals("ShipmentTypeList item Import", 0, TestARFilterBizO.ShipmentType_List_ForTestOnly.IndexOf(new CodeDescriptionPair("Import", "Shipment Destination LB")));
			AssertEquals("ShipmentTypeList item Export", 1, TestARFilterBizO.ShipmentType_List_ForTestOnly.IndexOf(new CodeDescriptionPair("Export", "Shipment Origin LB")));
			AssertEquals("ShipmentTypeList item Transhipment", 2, TestARFilterBizO.ShipmentType_List_ForTestOnly.IndexOf(new CodeDescriptionPair("Cross-Trade", "Shipment Origin and Destination not LB")));
		}

		#endregion

		#region Invoice Type

		public void TestFilterByInvoiceType()
		{
			Factory.Save();

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			aRInv.AH_OH = TestOrg.PK;

			ARCreditNote aRCrd = Factory.NewWithValidTestData<ARCreditNote>();
			aRCrd.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			aRCrd.AH_OH = TestOrg.PK;

			ARInvoice aRInv2 = Factory.NewWithValidTestData<ARInvoice>();
			aRInv2.AH_OH = TestOrg.PK;
			aRInv2.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;

			ARInvoice aRInv2_ = Factory.NewWithValidTestData<ARInvoice>();
			aRInv2_.AH_OH = TestOrg.PK;
			aRInv2_.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice_Batching;

			ARInvoice aRInv3 = Factory.NewWithValidTestData<ARInvoice>();
			aRInv3.AH_OH = TestOrg.PK;
			aRInv3.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			aRInv3.AH_ConsolidatedInvoiceRef = "S00024561";

			ARInvoice aRInv3_ = Factory.NewWithValidTestData<ARInvoice>();
			aRInv3_.AH_OH = TestOrg.PK;
			aRInv3_.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching;
			aRInv3_.AH_ConsolidatedInvoiceRef = "S00024561";

			Factory.Save();

			ModuleTextFilter invoiceTypeFilter = (ModuleTextFilter)TestARFilterBizO["Invoice Type"];
			invoiceTypeFilter.IsActive = true;

			invoiceTypeFilter.Property = InvoiceBatchHeaderFilterBusinessObject.InvoiceTypesFilterAdditional.Codes.AllInvoices;
			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 6 matching transactions", 6, transactions.Count);
			Assert("Collection should contain ARInv", transactions.Contains(aRInv));
			Assert("Collection should contain ARCrd", transactions.Contains(aRCrd));
			Assert("Collection should contain ARInv2", transactions.Contains(aRInv2));
			Assert("Collection should contain ARInv2_", transactions.Contains(aRInv2_));
			Assert("Collection should contain ARInv3", transactions.Contains(aRInv3));
			Assert("Collection should contain ARInv3_", transactions.Contains(aRInv3_));

			invoiceTypeFilter.Property = InvoiceTypesList.Codes.FinalInvoice;
			transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 2 matching transactions", 2, transactions.Count);
			Assert("Collection should contain ARInv", transactions.Contains(aRInv));
			Assert("Collection should contain ARCrd", transactions.Contains(aRCrd));

			invoiceTypeFilter.Property = InvoiceBatchHeaderFilterBusinessObject.InvoiceTypesFilterAdditional.Codes.NonJobRelatedDisbursementInvoice;
			transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 1 matching transactions", 1, transactions.Count);
			Assert("Collection should contain ARInv2", transactions.Contains(aRInv2));

			invoiceTypeFilter.Property = InvoiceBatchHeaderFilterBusinessObject.InvoiceTypesFilterAdditional.Codes.JobRelatedDisbursementInvoice;
			transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 1 matching transactions", 1, transactions.Count);
			Assert("Collection should contain ARInv3", transactions.Contains(aRInv3));
		}

		#endregion

		#region Shipment Type

		public void TestFilterByShipmentType()
		{
			Factory.Save();

			InvoiceBatchHeaderFilterBusinessObject filter = TestARFilterBizO; // to initialise the currency filter
			SetInvoicesToTest(JobShipmentSchema.Constants.Prefix);

			GlbCompany.CurrentCompany.SetCountry("US");

			ForwardingShipment shipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "AUSYD";

			ForwardingShipment shipment2 = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USCHI";

			this.JobToBeIncluded.JH_ParentID = shipment.PK;
			this.JobToBeExcluded.JH_ParentID = shipment2.PK;

			Factory.Save();

			ModuleTextFilter shipmentTypeFilter = (ModuleTextFilter)TestARFilterBizO["Shipment Type"];
			shipmentTypeFilter.IsActive = true;

			shipmentTypeFilter.Property = InvoiceBatchHeaderFilterBusinessObject.ShipmentTypeList.Codes.Transhipment;
			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 0 matching transactions", 0, transactions.Count);

			GlbCompany.CurrentCompany.SetCountry("LB");

			transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 2 matching transaction", 2, transactions.Count);
			Assert(transactions.Contains(this.InvoiceTobeIncluded));
			Assert(transactions.Contains(this.InvoiceTobeExcluded));

			GlbCompany.CurrentCompany.SetCountry("US");
			shipmentTypeFilter.Property = InvoiceBatchHeaderFilterBusinessObject.ShipmentTypeList.Codes.Import;
			transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 1 matching transaction", 1, transactions.Count);
			Assert(!transactions.Contains(this.InvoiceTobeIncluded));
			Assert(transactions.Contains(this.InvoiceTobeExcluded));

			shipmentTypeFilter.Property = InvoiceBatchHeaderFilterBusinessObject.ShipmentTypeList.Codes.Export;
			transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 1 matching transaction", 1, transactions.Count);
			Assert(transactions.Contains(this.InvoiceTobeIncluded));
			Assert(!transactions.Contains(this.InvoiceTobeExcluded));
		}

		#endregion

		#region Service Level

		public void TestFilterByServiceLevel()
		{
			Factory.Save();

			SetInvoicesToTest(JobShipmentSchema.Constants.Prefix);

			ForwardingShipment shipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			shipment.JS_RS_NKServiceLevel = "DIR";

			ForwardingShipment shipment2 = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			shipment2.JS_RS_NKServiceLevel = "D2D";

			this.JobToBeIncluded.JH_ParentID = shipment.PK;
			this.JobToBeExcluded.JH_ParentID = shipment2.PK;

			Factory.Save();

			TestARFilterBizO.SetParent(ParentBatchHeader);
			ParentBatchHeader.JobTypeList[ParentBatchHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.FWD)].Value = true;

			ModuleNkFilter serviceLevelFilter = (ModuleNkFilter)TestARFilterBizO["Shipment Service Level"];
			serviceLevelFilter.IsActive = true;
			serviceLevelFilter.Property = "STD";

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 0 matching transactions", 0, transactions.Count);

			shipment.JS_RS_NKServiceLevel = "STD";
			shipment2.JS_RS_NKServiceLevel = "D2D";

			Factory.Save();

			ParentBatchHeader.JobTypeList[ParentBatchHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.FWD)].Value = true;
			serviceLevelFilter.Property = "STD";

			transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 1 matching transaction", 1, transactions.Count);
			Assert(transactions.Contains(this.InvoiceTobeIncluded));
			Assert(!transactions.Contains(this.InvoiceTobeExcluded));
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new InvoiceBatchHeaderFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(JobCharge.Schema.TableName);
			TestCaseHelper.ClearTable(AccTransactionLines.Schema.TableName);
			TestCaseHelper.ClearTable(AccTransactionMatchLink.Schema.TableName);
			TestCaseHelper.ClearTable(AccTransactionHeader.Schema.TableName);
			ParentBatchHeader = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			ParentBatchHeader.AH_RX_NKTransactionCurrency = TestObjectCreator.GBP.RX_Code;
		}

		protected OrgHeader TestOrg
		{
			get
			{
				if (fTestOrg == null)
				{
					fTestOrg = Factory.NewWithValidTestData<OrgHeader>();
					OrgInvoiceType invoiceType = fTestOrg.CompanyData.InvoiceTypes.AddNew();
					invoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;

					invoiceType = fTestOrg.CompanyData.InvoiceTypes.AddNew();
					invoiceType.PI_Module = JobInvoicingConsumerTypes.CFSShipment.Code;

					invoiceType = fTestOrg.CompanyData.InvoiceTypes.AddNew();
					invoiceType.PI_Module = JobInvoicingConsumerTypes.Brokerage.Code;

					invoiceType = fTestOrg.CompanyData.InvoiceTypes.AddNew();
					invoiceType.PI_Module = "MSC";

					invoiceType = fTestOrg.CompanyData.InvoiceTypes.AddNew();
					invoiceType.PI_Module = JobInvoicingConsumerTypes.LocalCartage.Code;

					invoiceType = fTestOrg.CompanyData.InvoiceTypes.AddNew();
					invoiceType.PI_Module = JobInvoicingConsumerTypes.ImporterSecurityFiling.Code;
				}
				return fTestOrg;
			}
		}

		protected OrgHeader fTestOrg;

		protected InvoiceBatchHeaderFilterBusinessObject TestARFilterBizO
		{
			get
			{
				if (fTestARFilterBizO == null)
				{
					fTestARFilterBizO = (InvoiceBatchHeaderFilterBusinessObject)GetNewFilterStripBusinessObject();

					ModuleGuidFilter orgFilter = (ModuleGuidFilter)fTestARFilterBizO["Organisation"];
					if (orgFilter != null)
					{
						orgFilter.IsActive = true;
						orgFilter.Property = TestOrg.PK;
					}

					ModuleNkFilter currencyFilter = (ModuleNkFilter)fTestARFilterBizO["Currency"];
					currencyFilter.IsActive = true;
					currencyFilter.Property = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				}

				return fTestARFilterBizO;
			}
		}
		InvoiceBatchHeaderFilterBusinessObject fTestARFilterBizO;
		protected ARInvoice InvoiceTobeIncluded;
		protected ARInvoice InvoiceTobeExcluded;
		JobHeader JobToBeIncluded;
		JobHeader JobToBeExcluded;
		InvoiceBatchHeader ParentBatchHeader;

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		#endregion
	}
}
