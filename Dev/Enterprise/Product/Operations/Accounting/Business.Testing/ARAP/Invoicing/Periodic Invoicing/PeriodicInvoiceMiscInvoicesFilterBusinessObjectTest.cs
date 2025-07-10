using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(PeriodicInvoiceMiscInvoicesFilterBusinessObject))]
	public class PeriodicInvoiceMiscInvoicesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestSetupFilter()
		{
			var periodicinvoice = new PeriodicInvoice(Factory);
			var filter = new PeriodicInvoiceMiscInvoicesFilterBusinessObject(periodicinvoice);
			periodicinvoice.CurrencyNK = TestObjectCreator.EUR.RX_Code;
			AssertEquals("Precondition: Currency filter value", "", ((ModuleNkFilter)filter["Currency"]).Property);

			filter.SetupFilter(periodicinvoice);
			AssertEquals("Currency filter value", TestObjectCreator.EUR.RX_Code, ((ModuleNkFilter)filter["Currency"]).Property);
		}

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

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 2 matching transactions", 2, transactions.Count);
			Assert("Collection should contain ARInv", transactions.Contains(aRInv));
			Assert("Collection should contain ARCrd", transactions.Contains(aRCrd));
		}

		#endregion

		void SetInvoicesToTest(string tableCode)
		{
			InvoiceTobeIncluded = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			InvoiceTobeIncluded.AH_ConsolidatedInvoiceRef = "Test001";
			InvoiceTobeIncluded.AH_OH = TestOrg.PK;

			InvoiceTobeExcluded = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			InvoiceTobeExcluded.AH_ConsolidatedInvoiceRef = "Test002";
			InvoiceTobeExcluded.AH_OH = TestOrg.PK;

			JobToBeIncluded = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobToBeIncluded.JH_ParentTableCode = tableCode;

			JobToBeExcluded = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobToBeExcluded.JH_ParentTableCode = tableCode;

			InvoiceTobeIncluded.AH_JH = JobToBeIncluded.PK;
			InvoiceTobeExcluded.AH_JH = JobToBeExcluded.PK;
		}

		public void TestSubGroup()
		{
			int count = 0;

			foreach (ModuleFilter filter in TestARFilterBizO.ModuleFilters)
			{
				count++;

				if (filter.Description == "Shipment Type" || filter.Description == "Shipment Service Level")
				{
					AssertNotEquals("Sub Group has been set on this filter", null, filter.SubGroup);
				}
				else
				{
					AssertEquals("Sub Group hasn not been set", null, filter.SubGroup);
				}
			}

			AssertEquals("Total count of filters", 12, count);
		}

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

			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[0].PI_Module = JobInvoicingConsumerTypes.CFSShipment.Code;
			TestObjectCreator.AALSHI.CompanyData.InvoiceTypes[1].PI_Module = JobInvoicingConsumerTypes.Shipment.Code;

			Factory.Save();

			ModuleGuidFilter orgFilter = (ModuleGuidFilter)TestARFilterBizO["Organisation"];
			if (orgFilter != null)
			{
				orgFilter.IsActive = true;
				orgFilter.Property = TestObjectCreator.AALSHI.PK;

				AssertEquals(5, TestARFilterBizO.JobTypeList_ForTestOnly.Count);
			}
			else
			{
				Assert(true);
			}
		}

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

		#endregion

		#region Lookups

		public void TestShipmentTypeList()
		{
			GlbCompany.CurrentCompany.SetCountry("LB");
			AssertEquals("ShipmentTypeList count", 3, TestARFilterBizO.ShipmentType_List_ForTestOnly.Count);
			AssertEquals("ShipmentTypeList item Import", 0, TestARFilterBizO.ShipmentType_List_ForTestOnly.IndexOf(new CodeDescriptionPair("Import", "Shipment Destination LB")));
			AssertEquals("ShipmentTypeList item Export", 1, TestARFilterBizO.ShipmentType_List_ForTestOnly.IndexOf(new CodeDescriptionPair("Export", "Shipment Origin LB")));
			AssertEquals("ShipmentTypeList item Transhipment", 2, TestARFilterBizO.ShipmentType_List_ForTestOnly.IndexOf(new CodeDescriptionPair("Cross-Trade", "Shipment Origin and Destination not LB")));
		}

		#endregion

		#region Transaction Type

		public void TestFilterByTransactionType()
		{
			Factory.Save();

			ARInvoice inv1 = TestObjectCreator.CreateARInvoice<ARInvoice>("ARInv1", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			inv1.AH_TransactionType = "INV";

			ARInvoice inv2 = TestObjectCreator.CreateARInvoice<ARInvoice>("ARInv2", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			inv2.AH_TransactionType = "CRD";

			ModuleTextFilter transactionTypeFilter = (ModuleTextFilter)TestARFilterBizO["Transaction Type"];
			transactionTypeFilter.IsActive = true;

			transactionTypeFilter.Property = TransactionTypes.Invoice;
			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 6 matching transactions", 1, transactions.Count);
			Assert("Collection should contain ARInv1", transactions.Contains(inv1));
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
			aRCrd.AH_TransactionCategory = "";
			aRCrd.AH_OH = TestOrg.PK;
			aRCrd.AH_JH = TestObjectCreator.Job1.PK;

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

			invoiceTypeFilter.Property = PeriodicInvoiceMiscInvoicesFilterBusinessObject.InvoiceTypesFilterAdditional.Codes.AllInvoices;
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

			AssertEquals("There should be 1 matching transactions", 1, transactions.Count);
			Assert("Collection should contain ARInv", transactions.Contains(aRInv));

			invoiceTypeFilter.Property = PeriodicInvoiceMiscInvoicesFilterBusinessObject.InvoiceTypesFilterAdditional.Codes.MiscellaneousInvoice;
			transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 1 matching transactions", 1, transactions.Count);
			Assert("Collection should contain ARCrd", transactions.Contains(aRCrd));

			invoiceTypeFilter.Property = PeriodicInvoiceMiscInvoicesFilterBusinessObject.InvoiceTypesFilterAdditional.Codes.NonJobRelatedDisbursementInvoice;
			transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 1 matching transactions", 1, transactions.Count);
			Assert("Collection should contain ARInv2", transactions.Contains(aRInv2));

			invoiceTypeFilter.Property = PeriodicInvoiceMiscInvoicesFilterBusinessObject.InvoiceTypesFilterAdditional.Codes.JobRelatedDisbursementInvoice;
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

			PeriodicInvoiceMiscInvoicesFilterBusinessObject filter = TestARFilterBizO; // to initialise the currency filter
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

			shipmentTypeFilter.Property = PeriodicInvoiceMiscInvoicesFilterBusinessObject.ShipmentTypeList.Codes.Transhipment;
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
			shipmentTypeFilter.Property = PeriodicInvoiceMiscInvoicesFilterBusinessObject.ShipmentTypeList.Codes.Import;
			transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 1 matching transaction", 1, transactions.Count);
			Assert(!transactions.Contains(this.InvoiceTobeIncluded));
			Assert(transactions.Contains(this.InvoiceTobeExcluded));

			shipmentTypeFilter.Property = PeriodicInvoiceMiscInvoicesFilterBusinessObject.ShipmentTypeList.Codes.Export;
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

			periodicInvoiceBase.JobTypeList.Where(x => x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)).ToList().ForEach(x => x.Value = true);

			ModuleNkFilter serviceLevelFilter = (ModuleNkFilter)TestARFilterBizO["Shipment Service Level"];
			serviceLevelFilter.IsActive = true;
			serviceLevelFilter.Property = "STD";

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 0 matching transactions", 0, transactions.Count);

			shipment.JS_RS_NKServiceLevel = "STD";
			shipment2.JS_RS_NKServiceLevel = "D2D";

			Factory.Save();

			periodicInvoiceBase.JobTypeList.Where(x => x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)).ToList().ForEach(x => x.Value = true);
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
			return new PeriodicInvoiceMiscInvoicesFilterBusinessObject(periodicInvoiceBase);
		}

		protected virtual PeriodicInvoiceBase CreatePeriodicInvoice()
		{
			return new PeriodicInvoice(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(JobCharge.Schema.TableName);
			TestCaseHelper.ClearTable(AccTransactionLines.Schema.TableName);
			TestCaseHelper.ClearTable(AccTransactionMatchLink.Schema.TableName);
			TestCaseHelper.ClearTable(AccTransactionHeader.Schema.TableName);
			periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = TestObjectCreator.GBP.RX_Code;
		}

		protected OrgHeader TestOrg
		{
			get
			{
				if (fTestOrg == null)
				{
					fTestOrg = Factory.NewWithValidTestData<OrgHeader>();
					OrgInvoiceType invoiceType = fTestOrg.CompanyData.InvoiceTypes.AddNew();
					invoiceType.PI_Module = JobInvoicingConsumerTypes.CFSShipment.Code;

					invoiceType = fTestOrg.CompanyData.InvoiceTypes.AddNew();
					invoiceType.PI_Module = JobInvoicingConsumerTypes.Brokerage.Code;

					invoiceType = fTestOrg.CompanyData.InvoiceTypes.AddNew();
					invoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;

					invoiceType = fTestOrg.CompanyData.InvoiceTypes.AddNew();
					invoiceType.PI_Module = "MSC";

					invoiceType = fTestOrg.CompanyData.InvoiceTypes.AddNew();
					invoiceType.PI_Module = JobInvoicingConsumerTypes.TransportBookingConsignment.Code;

					//invoiceType = fTestOrg.CompanyData.InvoiceTypes.AddNew();
					//invoiceType.PI_Module = JobInvoicingConsumerTypes.TransportConsignment.Code;

					invoiceType = fTestOrg.CompanyData.InvoiceTypes.AddNew();
					invoiceType.PI_Module = JobInvoicingConsumerTypes.LocalCartage.Code;
				}
				return fTestOrg;
			}
		}

		protected OrgHeader fTestOrg;

		protected PeriodicInvoiceMiscInvoicesFilterBusinessObject TestARFilterBizO
		{
			get
			{
				if (fTestARFilterBizO == null)
				{
					fTestARFilterBizO = (PeriodicInvoiceMiscInvoicesFilterBusinessObject)GetNewFilterStripBusinessObject();

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
		PeriodicInvoiceMiscInvoicesFilterBusinessObject fTestARFilterBizO;
		protected ARInvoice InvoiceTobeIncluded;
		protected ARInvoice InvoiceTobeExcluded;
		JobHeader JobToBeIncluded;
		JobHeader JobToBeExcluded;
		PeriodicInvoiceBase periodicInvoiceBase;

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
