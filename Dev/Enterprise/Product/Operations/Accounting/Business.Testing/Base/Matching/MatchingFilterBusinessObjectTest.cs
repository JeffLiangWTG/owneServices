using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	public abstract class MatchingFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters

		public void TestTransactionNumberFilter()
		{
			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			var inv2 = Factory.NewWithValidTestData<ARInvoice>();

			inv1.IsManuallySetTransactionNumber_ForTestOnly = true;
			inv2.IsManuallySetTransactionNumber_ForTestOnly = true;

			inv1.AH_TransactionNum = "1";
			inv2.AH_TransactionNum = "2";

			Factory.Save();

			var filter = (ModuleNumberFilter)FilterBizO[MatchingFilterBusinessObject.TransactionNumber];

			filter.Property = "1";
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			filter.Property = "2";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = "NonExistent";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		public void TestInvoiceTransactionReferenceNumberFilter()
		{
			var invoice1 = Factory.NewWithValidTestData<ARInvoice>();

			var headerReference1 = Factory.New<AccTransactionHeaderReference>();
			headerReference1.AH1_AH = invoice1.PK;
			headerReference1.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ITR;
			headerReference1.AH1_Reference = "00001000";

			var invoice2 = Factory.NewWithValidTestData<ARInvoice>();

			var headerReference2 = Factory.New<AccTransactionHeaderReference>();
			headerReference2.AH1_AH = invoice2.PK;
			headerReference2.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ITR;
			headerReference2.AH1_Reference = "00001001";

			Factory.Save();

			var collectionReferenceNumberFilter = (ModuleTextFilter)FilterBizO[MatchingFilterBusinessObject.InvoiceTransactionReference];
			collectionReferenceNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			collectionReferenceNumberFilter.Property = "00001000";
			collectionReferenceNumberFilter.IsActive = true;

			AssertEquals(AccTransactionHeaderReferenceSchema.AH1_Reference.MaxLength, collectionReferenceNumberFilter.MaxLength);

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, FilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain one invoice", 1, testTransactions.Count);
			Assert("Collection contains invoice1", testTransactions.Contains(invoice1.PK));
			Assert("Collection doesn't contains invoice2", !testTransactions.Contains(invoice2.PK));
		}

		public void TestComplianceNumberFilter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var vat3 = TestObjectCreator.CreateTaxRate("VAT3", "", 3);
				vat3.AT_PostingGroupId = 0;

				var ac1 = TestObjectCreator.CreateChargeCode("AC1");
				ac1.AC_AT_GSTRate = vat3.PK;
				ac1.AC_Desc = "desc";

				var testInv = new Invoice[4];
				for (int i = 0; i < testInv.Length; i++)
				{
					testInv[i] = Factory.NewWithValidTestData<ARInvoice>();
					var line = (InvoiceLine)testInv[i].Lines.AddNew();
					line.AL_JH = TestObjectCreator.Job1.PK;
					line.AL_AC = ac1.PK;
					line.AL_AT = vat3.PK;
					var charge = TestObjectCreator.CreateJobCharge(line, TestObjectCreator.Job1, ac1);
				}
				testInv[1].AH_TransactionReference = "00001014";
				var complianceHeader3 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(testInv[2].AH_Ledger, "desc", "", "TXI", "lineDesc", testInv[2].Lines[0]);
				var complianceHeader4 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(testInv[3].AH_Ledger, "desc", "00001015", "TXI", "lineDesc", testInv[3].Lines[0]);
				Factory.Save();

				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				var filter = ((ModuleNumberFilter)FilterBizO["Compliance #"]);
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "00001014";
				filter.IsActive = true;

				var testTransactions = new TransactionHeaderCollection(Factory, FilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("There should be 1 Invoice in the collection", 1, testTransactions.Count);
				Assert("Should not contain 1st invoice", !testTransactions.Contains(testInv[0]));
				Assert("Should contain 2nd invoice", testTransactions.Contains(testInv[1]));
				Assert("Should not contain 3rd invoice", !testTransactions.Contains(testInv[2]));
				Assert("Should not contain 4th invoice", !testTransactions.Contains(testInv[3]));

				filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				testTransactions = new TransactionHeaderCollection(Factory, FilterBizO.Filter);
				testTransactions.Load();

				AssertEquals("There should be 3 Invoices in the collection", 3, testTransactions.Count);
				Assert("Should contain 1st invoice", testTransactions.Contains(testInv[0]));
				Assert("Should not contain 2nd invoice", !testTransactions.Contains(testInv[1]));
				Assert("Should contain 3rd invoice", testTransactions.Contains(testInv[2]));
				Assert("Should contain 4th invoice", testTransactions.Contains(testInv[3]));

				filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
				testTransactions = new TransactionHeaderCollection(Factory, FilterBizO.Filter);
				testTransactions.Load();

				AssertEquals("There should be 1 Invoice in the collection", 1, testTransactions.Count);
				Assert("Should not contain 1st invoice", !testTransactions.Contains(testInv[0]));
				Assert("Should contain 2nd invoice", testTransactions.Contains(testInv[1]));
				Assert("Should not contain 3rd invoice", !testTransactions.Contains(testInv[2]));
				Assert("Should not contain 4th invoice", !testTransactions.Contains(testInv[3]));

				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				FilterBizO = null;
				FilterBizO = (MatchingFilterBusinessObject)GetNewFilterStripBusinessObject();
				filter = ((ModuleNumberFilter)FilterBizO["Compliance #"]);
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "00001014";
				filter.IsActive = true;
				testTransactions = new TransactionHeaderCollection(Factory, FilterBizO.Filter);
				testTransactions.Load();

				AssertEquals("There should be 1 Invoice in the collection", 1, testTransactions.Count);
				Assert("Should not contain 1st invoice", !testTransactions.Contains(testInv[0]));
				Assert("Should contain 2nd invoice", testTransactions.Contains(testInv[1]));
				Assert("Should not contain 3rd invoice", !testTransactions.Contains(testInv[2]));
				Assert("Should not contain 4th invoice", !testTransactions.Contains(testInv[3]));

				filter.Property = "00001015";
				testTransactions = new TransactionHeaderCollection(Factory, FilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("There should be 1 Invoice in the collection", 1, testTransactions.Count);
				Assert("Should not contain 1st invoice", !testTransactions.Contains(testInv[0]));
				Assert("Should not contain 2nd invoice", !testTransactions.Contains(testInv[1]));
				Assert("Should not contain 3rd invoice", !testTransactions.Contains(testInv[2]));
				Assert("Should contain 4th invoice", testTransactions.Contains(testInv[3]));

				filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				filter.Property = "00001015";
				testTransactions = new TransactionHeaderCollection(Factory, FilterBizO.Filter);
				testTransactions.Load();

				AssertEquals("There should be 3 Invoices in the collection", 3, testTransactions.Count);
				Assert("Should contain 1st invoice", testTransactions.Contains(testInv[0]));
				Assert("Should contain 2nd invoice", testTransactions.Contains(testInv[1]));
				Assert("Should contain 3rd invoice", testTransactions.Contains(testInv[2]));
				Assert("Should not contain 4th invoice", !testTransactions.Contains(testInv[3]));

				filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
				filter.Property = "0000101";
				testTransactions = new TransactionHeaderCollection(Factory, FilterBizO.Filter);
				testTransactions.Load();

				AssertEquals("There should be 2 Invoices in the collection", 2, testTransactions.Count);
				Assert("Should contain 1st invoice", testTransactions.Contains(testInv[0]));
				Assert("Should not contain 2nd invoice", !testTransactions.Contains(testInv[1]));
				Assert("Should contain 3rd invoice", testTransactions.Contains(testInv[2]));
				Assert("Should not contain 4th invoice", !testTransactions.Contains(testInv[3]));

				filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
				filter.Property = "0000101";
				testTransactions = new TransactionHeaderCollection(Factory, FilterBizO.Filter);
				testTransactions.Load();

				AssertEquals("There should be 2 Invoices in the collection", 2, testTransactions.Count);
				Assert("Should contain 1st invoice", testTransactions.Contains(testInv[0]));
				Assert("Should not contain 2nd invoice", !testTransactions.Contains(testInv[1]));
				Assert("Should contain 3rd invoice", testTransactions.Contains(testInv[2]));
				Assert("Should not contain 4th invoice", !testTransactions.Contains(testInv[3]));

				filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				testTransactions = new TransactionHeaderCollection(Factory, FilterBizO.Filter);
				testTransactions.Load();

				AssertEquals("There should be 2 Invoices in the collection", 2, testTransactions.Count);
				Assert("Should contain 1st invoice", testTransactions.Contains(testInv[0]));
				Assert("Should not contain 2nd invoice", !testTransactions.Contains(testInv[1]));
				Assert("Should contain 3rd invoice", testTransactions.Contains(testInv[2]));
				Assert("Should not contain 4th invoice", !testTransactions.Contains(testInv[3]));

				filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
				testTransactions = new TransactionHeaderCollection(Factory, FilterBizO.Filter);
				testTransactions.Load();

				AssertEquals("There should be 2 Invoices in the collection", 2, testTransactions.Count);
				Assert("Should not contain 1st invoice", !testTransactions.Contains(testInv[0]));
				Assert("Should contain 2nd invoice", testTransactions.Contains(testInv[1]));
				Assert("Should not contain 3rd invoice", !testTransactions.Contains(testInv[2]));
				Assert("Should contain 4th invoice", testTransactions.Contains(testInv[3]));
			}
		}

		public void TestChequeReferenceNumberFilter()
		{
			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			var inv2 = Factory.NewWithValidTestData<ARInvoice>();

			inv1.AH_ChequeOrReference = "1";
			inv2.AH_ChequeOrReference = "2";

			Factory.Save();

			var filter = (ModuleNumberFilter)FilterBizO[MatchingFilterBusinessObject.ChequeReferenceNumber];

			filter.Property = "1";
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			filter.Property = "2";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = "3";
			FilterCollection.Load(FilterBizO.Filter);

			filter.Property = "NonExistent";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		public void TestJobInvoiceNumberFilter()
		{
			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			var inv2 = Factory.NewWithValidTestData<ARInvoice>();

			inv1.AH_ConsolidatedInvoiceRef = "1";
			inv2.AH_ConsolidatedInvoiceRef = "2";

			Factory.Save();

			var filter = (ModuleNumberFilter)FilterBizO[MatchingFilterBusinessObject.JobInvoiceNumber];

			filter.Property = "1";
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			filter.Property = "2";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = "NonExistent";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		public void TestInvoiceRemittanceReferenceNumberFilter()
		{
			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			var inv2 = Factory.NewWithValidTestData<ARInvoice>();

			var ref1 = Factory.NewWithValidTestData<AccTransactionHeaderReference>();
			var ref2 = Factory.NewWithValidTestData<AccTransactionHeaderReference>();

			ref1.AH1_AH = inv1.PK;
			ref2.AH1_AH = inv2.PK;

			ref1.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRR;
			ref2.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRR;

			ref1.AH1_Reference = "1";
			ref2.AH1_Reference = "2";

			Factory.Save();

			var filter = (ModuleNumberFilter)FilterBizO[MatchingFilterBusinessObject.InvoiceRemittanceReference];

			filter.Property = "1";
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			filter.Property = "2";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = "NonExistent";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		public void TestHouseBillNumberFilter()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var shipment2 = TestObjectCreator.CreateShipment("S0002");

			shipment1.JS_HouseBill = "1";
			shipment2.JS_HouseBill = "2";

			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();

			declaration1.JE_HouseBill = "3";
			declaration2.JE_HouseBill = "4";

			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var job4 = Factory.NewJobWithValidTestDataForTesting<Job>();

			job1.JH_ParentID = shipment1.PK;
			job2.JH_ParentID = shipment2.PK;
			job3.JH_ParentID = declaration1.PK;
			job4.JH_ParentID = declaration2.PK;

			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			var inv2 = Factory.NewWithValidTestData<ARInvoice>();
			var inv3 = Factory.NewWithValidTestData<ARInvoice>();
			var inv4 = Factory.NewWithValidTestData<ARInvoice>();

			inv1.AH_JH = shipment1.Job.PK;
			inv2.AH_JH = shipment2.Job.PK;
			inv3.AH_JH = declaration1.Job.PK;
			inv4.AH_JH = declaration2.Job.PK;

			Factory.Save();

			var filter = (ModuleNumberFilter)FilterBizO[MatchingFilterBusinessObject.HouseBillNumber];

			filter.Property = "1";
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			filter.Property = "2";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = "3";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv3.PK));

			filter.Property = "4";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv4.PK));

			filter.Property = "NonExistent";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		public void TestFlightVoyageNumberAndVesselFilter()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();

			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();

			vessel1.RV_Name = "V1";
			vessel2.RV_Name = "V2";

			var transport1 = consol1.Transports[0];
			var transport2 = consol2.Transports[0];

			transport1.JW_Vessel = vessel1.RV_FK;
			transport2.JW_Vessel = vessel2.RV_FK;

			transport1.JW_VoyageFlight = "1";
			transport2.JW_VoyageFlight = "2";

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			var jobConShipLink1 = Factory.NewWithValidTestData<JobConShipLink>();
			var jobConShipLink2 = Factory.NewWithValidTestData<JobConShipLink>();

			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();

			jobConShipLink1.JN_JK = consol1.PK;
			jobConShipLink2.JN_JK = consol2.PK;

			jobConShipLink1.JN_JS = shipment1.PK;
			jobConShipLink2.JN_JS = shipment2.PK;

			job1.JH_ParentID = shipment1.PK;
			job2.JH_ParentID = shipment2.PK;

			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			var inv2 = Factory.NewWithValidTestData<ARInvoice>();

			inv1.AH_JH = job1.PK;
			inv2.AH_JH = job2.PK;

			var jobSailing1 = Factory.NewWithValidTestData<JobSailing>();
			var jobSailing2 = Factory.NewWithValidTestData<JobSailing>();

			var jobVoyage1 = Factory.NewWithValidTestData<JobVoyage>();
			var jobVoyage2 = Factory.NewWithValidTestData<JobVoyage>();

			var jobVoyDestination1 = Factory.NewWithValidTestData<VoyageDestination>();
			var jobVoyDestination2 = Factory.NewWithValidTestData<VoyageDestination>();

			shipment1.JS_JX = jobSailing1.PK;
			shipment2.JS_JX = jobSailing2.PK;

			jobSailing1.JX_JB = jobVoyDestination1.PK;
			jobSailing2.JX_JB = jobVoyDestination2.PK;

			jobVoyDestination1.JB_JV = jobVoyage1.PK;
			jobVoyDestination2.JB_JV = jobVoyage2.PK;

			jobVoyage1.JV_VoyageFlight = "3";
			jobVoyage2.JV_VoyageFlight = "4";

			Factory.Save();

			var filter = (ModuleTextAndNkFilter)FilterBizO[MatchingFilterBusinessObject.FlightVoyageNumberAndVessel];

			filter.Property = "3";
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			filter.Property = "4";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			shipment1.JS_IsShipping = true;
			shipment2.JS_IsShipping = true;

			Factory.Save();

			filter.Property = "";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = "1";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			filter.Property = "2";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = "";
			filter.NkProperty = "V1";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			filter.NkProperty = "V2";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			filter.NkProperty = "Empty";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		public void TestFlightVoyageNumberAndVesselFilter_RoutingLinkedFlagIsUnticked()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VSL1";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var consolTransport = consol.Transports[0];
			consolTransport.JW_Vessel = vessel.RV_FK;
			consolTransport.JW_VoyageFlight = "VOY1";
			consolTransport.JW_IsLinked = false;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var jobConShipLink = Factory.NewWithValidTestData<JobConShipLink>();
			jobConShipLink.JN_JK = consol.PK;
			jobConShipLink.JN_JS = shipment.PK;

			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentID = shipment.PK;

			var invoiceAP = Factory.NewWithValidTestData<APInvoice>();
			var invoiceAR = Factory.NewWithValidTestData<ARInvoice>();

			Factory.Save();

			invoiceAP.AH_JH = job1.PK;
			invoiceAR.AH_JH = job1.PK;
			Factory.Save();

			var filter = (ModuleTextAndNkFilter)FilterBizO[MatchingFilterBusinessObject.FlightVoyageNumberAndVessel];
			filter.IsActive = true;

			filter.Property = "";
			filter.NkProperty = "VSL1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert("Should contain the A/R invoice", FilterCollection.Contains(invoiceAR.PK));
			Assert("Should contain the A/P invoice", FilterCollection.Contains(invoiceAP.PK));

			filter.Property = "VOY1";
			filter.NkProperty = "";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert("Should contain the A/R invoice", FilterCollection.Contains(invoiceAR.PK));
			Assert("Should contain the A/P invoice", FilterCollection.Contains(invoiceAP.PK));

			filter.Property = "VOY1";
			filter.NkProperty = "VSL1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert("Should contain the A/R invoice", FilterCollection.Contains(invoiceAR.PK));
			Assert("Should contain the A/P invoice", FilterCollection.Contains(invoiceAP.PK));

			filter.Property = "INVALID";
			filter.NkProperty = "INVALID";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(0, FilterCollection.Count);
			Assert("Should not contain the A/R invoice", !FilterCollection.Contains(invoiceAR.PK));
			Assert("Should not contain the A/P invoice", !FilterCollection.Contains(invoiceAP.PK));
		}

		public void TestMasterBillNumberFilter()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();

			consol1.JK_MasterBillNum = "1";
			consol2.JK_MasterBillNum = "2";

			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment1.JS_IsShipping = false;
			shipment1.JS_IsShipping = false;

			var jobConShipLink1 = Factory.NewWithValidTestData<JobConShipLink>();
			var jobConShipLink2 = Factory.NewWithValidTestData<JobConShipLink>();

			jobConShipLink1.JN_JK = consol1.PK;
			jobConShipLink1.JN_JS = shipment1.PK;

			jobConShipLink2.JN_JK = consol2.PK;
			jobConShipLink2.JN_JS = shipment2.PK;

			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();

			job1.JH_ParentID = shipment1.PK;
			job2.JH_ParentID = shipment2.PK;

			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			var inv2 = Factory.NewWithValidTestData<ARInvoice>();

			inv1.AH_JH = job1.PK;
			inv2.AH_JH = job2.PK;

			Factory.Save();

			var filter = (ModuleNumberFilter)FilterBizO[MatchingFilterBusinessObject.MasterBillNumber];

			filter.Property = "1";
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			filter.Property = "2";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = "NonExistent";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		public void TestAllNumbersFilter()
		{
			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			inv1.IsManuallySetTransactionNumber_ForTestOnly = true;
			inv1.AH_TransactionNum = "1";

			var inv2 = Factory.NewWithValidTestData<ARInvoice>();
			inv2.AH_TransactionReference = "1";

			var inv3 = Factory.NewWithValidTestData<ARInvoice>();
			inv3.AH_ChequeOrReference = "2";

			var inv4 = Factory.NewWithValidTestData<ARInvoice>();
			inv4.AH_ConsolidatedInvoiceRef = "2";

			var inv5 = Factory.NewWithValidTestData<ARInvoice>();
			var ref1 = Factory.NewWithValidTestData<AccTransactionHeaderReference>();
			ref1.AH1_AH = inv5.PK;
			ref1.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRR;
			ref1.AH1_Reference = "3";

			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			shipment1.JS_HouseBill = "3";
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentID = shipment1.PK;
			var inv6 = Factory.NewWithValidTestData<ARInvoice>();
			inv6.AH_JH = shipment1.Job.PK;

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			var transport1 = consol1.Transports[0];
			transport1.JW_Vessel = vessel1.RV_FK;
			transport1.JW_VoyageFlight = "4";
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_IsShipping = true;
			var jobConShipLink1 = Factory.NewWithValidTestData<JobConShipLink>();
			jobConShipLink1.JN_JK = consol1.PK;
			jobConShipLink1.JN_JS = shipment2.PK;
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentID = shipment2.PK;
			var inv7 = Factory.NewWithValidTestData<ARInvoice>();
			inv7.AH_JH = job2.PK;

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_MasterBillNum = "4";
			var jobConShipLink2 = Factory.NewWithValidTestData<JobConShipLink>();
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_IsShipping = false;
			jobConShipLink2.JN_JK = consol2.PK;
			jobConShipLink2.JN_JS = shipment3.PK;
			var job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_ParentID = shipment3.PK;
			var inv8 = Factory.NewWithValidTestData<ARInvoice>();
			inv8.AH_JH = job3.PK;

			var inv9 = Factory.NewWithValidTestData<ARInvoice>();
			inv9.AH_ReceiptBatchNo = "5";

			var inv10 = Factory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(inv10, TestObjectCreator.AUD, 1.0m, 5.0m, 0.0m, 0.0m);

			Factory.Save();

			var filter = (ModuleNumberFilter)FilterBizO[MatchingFilterBusinessObject.AllNumbers];

			filter.Property = "1";
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			filter.Property = "2";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv3.PK));
			Assert(FilterCollection.Contains(inv4.PK));

			filter.Property = "3";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv5.PK));
			Assert(FilterCollection.Contains(inv6.PK));

			filter.Property = "4";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv7.PK));
			Assert(FilterCollection.Contains(inv8.PK));

			filter.Property = "5";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv9.PK));
			Assert(FilterCollection.Contains(inv10.PK));

			filter.Property = "NonExistent";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		public void TestAllNumbersFilterWhenExceedMaxLength()
		{
			var allNumbersFilterMembersMaxLengths = new int[]
			{
				AccTransactionHeaderSchema.AH_ChequeOrReference.MaxLength,
				AccTransactionHeaderSchema.AH_TransactionNum.MaxLength,
				AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef.MaxLength,
				JobDeclarationSchema.JE_HouseBill.MaxLength,
				JobConsolSchema.JK_MasterBillNum.MaxLength,
				AccTransactionHeaderSchema.AH_ReceiptBatchNo.MaxLength,
				AccTransactionHeaderSchema.AH_OutstandingAmount.MaxLength,
				AccTransactionHeaderSchema.AH_TransactionReference.MaxLength,
				AccTransactionHeaderReferenceSchema.AH1_Reference.MaxLength,
				JobVoyageSchema.JV_VoyageFlight.MaxLength
			};

			var maxLength = allNumbersFilterMembersMaxLengths.MaxBy(x => x);
			var filter = (ModuleNumberFilter)FilterBizO[MatchingFilterBusinessObject.AllNumbers];

			filter.Property = new string('1', maxLength + 1);
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			AssertNoExceptionThrown(() => FilterCollection.Load(FilterBizO.Filter));
		}

		public void TestTransactionDateFilter()
		{
			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			var inv2 = Factory.NewWithValidTestData<ARInvoice>();

			inv1.AH_InvoiceDate = new ZDate(2000, 1, 1);
			inv2.AH_InvoiceDate = new ZDate(2005, 1, 1);

			Factory.Save();

			var filter = (ModuleDateFilter)FilterBizO[MatchingFilterBusinessObject.TransactionDate];

			filter.IsActive = true;
			filter.Property1 = new ZDateTime(1999, 1, 1);
			filter.Property2 = new ZDateTime(2001, 1, 1);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			filter.Property1 = new ZDateTime(2004, 1, 1);
			filter.Property2 = new ZDateTime(2006, 1, 1);
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property1 = new ZDateTime(1999, 1, 1);
			filter.Property2 = new ZDateTime(2016, 1, 1);
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));
			Assert(FilterCollection.Contains(inv2.PK));
		}

		public void TestDueDateFilter()
		{
			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			var inv2 = Factory.NewWithValidTestData<ARInvoice>();

			inv1.AH_DueDate = new ZDate(2000, 1, 1);
			inv2.AH_DueDate = new ZDate(2005, 1, 1);

			Factory.Save();

			var filter = (ModuleDateFilter)FilterBizO[MatchingFilterBusinessObject.DueDate];

			filter.IsActive = true;
			filter.Property1 = new ZDateTime(1999, 1, 1);
			filter.Property2 = new ZDateTime(2001, 1, 1);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			filter.Property1 = new ZDateTime(2004, 1, 1);
			filter.Property2 = new ZDateTime(2006, 1, 1);
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property1 = new ZDateTime(1999, 1, 1);
			filter.Property2 = new ZDateTime(2016, 1, 1);
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));
			Assert(FilterCollection.Contains(inv2.PK));
		}

		public void TestBranchFilter()
		{
			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			var inv2 = Factory.NewWithValidTestData<ARInvoice>();

			var branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("GHI", GlbCompany.CurrentCompany);

			inv1.AH_GB = branch1.PK;
			inv2.AH_GB = branch2.PK;

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterBizO[MatchingFilterBusinessObject.Branch];

			filter.Property = ZGuid.Empty;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = branch1.PK;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			filter.Property = branch2.PK;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = branch3.PK;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		public void TestDepartmentFilter()
		{
			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			var inv2 = Factory.NewWithValidTestData<ARInvoice>();

			var department1 = TestObjectCreator.CreateDepartment("ABC");
			var department2 = TestObjectCreator.CreateDepartment("DEF");
			var department3 = TestObjectCreator.CreateDepartment("GHI");

			inv1.AH_GE = department1.PK;
			inv2.AH_GE = department2.PK;

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterBizO[MatchingFilterBusinessObject.Department];

			filter.Property = ZGuid.Empty;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = department1.PK;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			filter.Property = department2.PK;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = department3.PK;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		public void TestCurrencyFilter()
		{
			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			var inv2 = Factory.NewWithValidTestData<ARInvoice>();

			inv1.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			inv2.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			Factory.Save();

			var filter = (ModuleNkFilter)FilterBizO[MatchingFilterBusinessObject.Currency];

			filter.Property = ZString.Empty;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = Core.Constants.CurrencyCodes.Australia;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			filter.Property = Core.Constants.CurrencyCodes.UnitedStates;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = Core.Constants.CurrencyCodes.Canada;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		public void TestLedgerTransactionTypeFilter()
		{
			var aRInv = Factory.NewWithValidTestData<ARInvoice>();
			var aPInv = Factory.NewWithValidTestData<APInvoice>();

			var aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			var aPCreditNote = Factory.NewWithValidTestData<APCreditNote>();

			var aRPayment = Factory.NewWithValidTestData<ARPayment>();
			var aPPayment = Factory.NewWithValidTestData<APPayment>();

			Factory.Save();

			var filter = (DependentListFilter)FilterBizO[MatchingFilterBusinessObject.LedgerTransactionType];

			filter.Property1 = ZString.Empty;
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(6, FilterCollection.Count);
			Assert(FilterCollection.Contains(aRInv.PK));
			Assert(FilterCollection.Contains(aPInv.PK));
			Assert(FilterCollection.Contains(aRCreditNote.PK));
			Assert(FilterCollection.Contains(aPCreditNote.PK));
			Assert(FilterCollection.Contains(aRPayment.PK));
			Assert(FilterCollection.Contains(aPPayment.PK));

			filter.Property1 = LedgerTypes.AccountsReceivable;
			filter.Property2 = ZString.Empty;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(3, FilterCollection.Count);
			Assert(FilterCollection.Contains(aRInv.PK));
			Assert(FilterCollection.Contains(aRCreditNote.PK));
			Assert(FilterCollection.Contains(aRPayment.PK));

			filter.Property1 = LedgerTypes.AccountsPayable;
			filter.Property2 = ZString.Empty;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(3, FilterCollection.Count);
			Assert(FilterCollection.Contains(aPInv.PK));
			Assert(FilterCollection.Contains(aPCreditNote.PK));
			Assert(FilterCollection.Contains(aPPayment.PK));

			filter.Property1 = ZString.Empty;
			filter.Property2 = TransactionTypes.Invoice;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(aRInv.PK));
			Assert(FilterCollection.Contains(aPInv.PK));

			filter.Property1 = ZString.Empty;
			filter.Property2 = TransactionTypes.CreditNote;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(aRCreditNote.PK));
			Assert(FilterCollection.Contains(aPCreditNote.PK));

			filter.Property1 = ZString.Empty;
			filter.Property2 = TransactionTypes.Payment;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(aRPayment.PK));
			Assert(FilterCollection.Contains(aPPayment.PK));

			filter.Property1 = LedgerTypes.AccountsReceivable;
			filter.Property2 = TransactionTypes.Invoice;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(aRInv.PK));

			filter.Property1 = LedgerTypes.AccountsPayable;
			filter.Property2 = TransactionTypes.Invoice;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(aPInv.PK));

			filter.Property1 = LedgerTypes.AccountsReceivable;
			filter.Property2 = TransactionTypes.CreditNote;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(aRCreditNote.PK));

			filter.Property1 = LedgerTypes.AccountsPayable;
			filter.Property2 = TransactionTypes.CreditNote;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(aPCreditNote.PK));

			filter.Property1 = LedgerTypes.AccountsReceivable;
			filter.Property2 = TransactionTypes.Payment;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(aRPayment.PK));

			filter.Property1 = LedgerTypes.AccountsPayable;
			filter.Property2 = TransactionTypes.Payment;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(aPPayment.PK));

			filter.Property1 = "NonExistent";
			filter.Property2 = "NonExistent";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		public void TestDescriptionFilter()
		{
			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			var inv2 = Factory.NewWithValidTestData<ARInvoice>();

			inv1.AH_Desc = "inv1";
			inv2.AH_Desc = "inv2";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBizO[MatchingFilterBusinessObject.Description];

			filter.Property = ZString.Empty;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = "inv1";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			filter.Property = "inv2";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = "NonExistent";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		public void TestInvoiceBatchNumberFilter()
		{
			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			var inv2 = Factory.NewWithValidTestData<ARInvoice>();

			inv1.AH_ReceiptBatchNo = "ABC";
			inv2.AH_ReceiptBatchNo = "DEF";

			Factory.Save();

			var filter = (ModuleNumberFilter)FilterBizO[MatchingFilterBusinessObject.InvoiceBatchNumber];

			filter.Property = ZString.Empty;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = "ABC";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			filter.Property = "DEF";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = "NonExistent";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		public void TestOutstandingAmountNumberFilter()
		{
			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			var inv2 = Factory.NewWithValidTestData<ARInvoice>();

			TestObjectCreator.CreateInvoiceLine(inv1, TestObjectCreator.AUD, 1.0m, 10.0m, 0.0m, 0.0m);
			TestObjectCreator.CreateInvoiceLine(inv2, TestObjectCreator.AUD, 1.0m, 20.0m, 0.0m, 0.0m);

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBizO[MatchingFilterBusinessObject.OutstandingAmount];

			filter.Property = "10";
			filter.IsActive = true;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			filter.Property = "20";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property = "NonExistent";
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		public void TestDispursementFlagFilter()
		{
			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			var inv2 = Factory.NewWithValidTestData<ARInvoice>();

			inv1.IsDisbursementOrFinal = true;
			inv2.IsDisbursementOrFinal = false;

			Factory.Save();

			var filter = (ModuleFlagsFilter)FilterBizO[MatchingFilterBusinessObject.DisbursementInvoice];

			filter.Property0 = false;
			filter.IsActive = true;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));
			Assert(FilterCollection.Contains(inv2.PK));

			filter.Property0 = true;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));
		}

		[SuspendCriticalValidation]
		public void TestDebtorAndAddressFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsDebtor = true;

			var mainAddress = TestObjectCreator.CreateAddress(org1, OrgAddressType.Office, isMain: true);
			var address1 = TestObjectCreator.CreateAddress(org1, OrgAddressType.Receivables, isMain: true);
			var address2 = TestObjectCreator.CreateAddress(org1, OrgAddressType.Receivables, isMain: false);

			Factory.Save();

			var job1 = TestObjectCreator.CreateJob(org1, 0, TestObjectCreator.Agent, 0);
			var job2 = TestObjectCreator.CreateJob(org1, 0, TestObjectCreator.Agent, 0);
			var job3 = TestObjectCreator.CreateJob(org1, 0, TestObjectCreator.Agent, 0);
			var job4 = TestObjectCreator.CreateJob(org1, 0, TestObjectCreator.Agent, 0);
			var job5 = TestObjectCreator.CreateJob(org1, 0, TestObjectCreator.Agent, 0);

			var inv1 = TestObjectCreator.CreateARInvoice<ARInvoice>("1", TestObjectCreator.AUD, 1m, org1);
			var inv2 = TestObjectCreator.CreateARInvoice<ARInvoice>("2", TestObjectCreator.AUD, 1m, org1);
			var inv3 = TestObjectCreator.CreateARInvoice<ARInvoice>("3", TestObjectCreator.AUD, 1m, org1);
			var inv4 = TestObjectCreator.CreateARInvoice<ARInvoice>("4", TestObjectCreator.AUD, 1m, org1);
			var inv5 = TestObjectCreator.CreateARInvoice<ARInvoice>("5", TestObjectCreator.AUD, 1m, org1);

			inv1.AH_OA_InvoiceAddressOverride = mainAddress.PK;
			inv2.AH_OA_InvoiceAddressOverride = address1.PK;
			inv3.AH_OA_InvoiceAddressOverride = address2.PK;
			inv4.AH_OA_InvoiceAddressOverride = ZGuid.Empty;
			inv5.AH_OA_InvoiceAddressOverride = ZGuid.Empty;

			TestObjectCreator.CreateInvoiceLine(inv1, 110m, inv1.TransactionCurrency, 1m);
			TestObjectCreator.CreateInvoiceLine(inv2, 110m, inv2.TransactionCurrency, 1m);
			TestObjectCreator.CreateInvoiceLine(inv3, 110m, inv3.TransactionCurrency, 1m);
			TestObjectCreator.CreateInvoiceLine(inv4, 110m, inv4.TransactionCurrency, 1m);
			TestObjectCreator.CreateInvoiceLine(inv5, 110m, inv5.TransactionCurrency, 1m);

			job1.JH_OA_LocalChargesAddr = ZGuid.Empty;
			job2.JH_OA_LocalChargesAddr = ZGuid.Empty;
			job3.JH_OA_LocalChargesAddr = address1.PK;
			job4.JH_OA_LocalChargesAddr = ZGuid.Empty;
			job5.JH_OA_LocalChargesAddr = address1.PK;

			job1.JH_OA_AgentCollectAddr = ZGuid.Empty;
			job2.JH_OA_AgentCollectAddr = ZGuid.Empty;
			job3.JH_OA_AgentCollectAddr = ZGuid.Empty;
			job4.JH_OA_AgentCollectAddr = address1.PK;
			job5.JH_OA_AgentCollectAddr = ZGuid.Empty;

			inv1.AH_JH = job1.PK;
			inv2.AH_JH = job2.PK;
			inv3.AH_JH = job3.PK;
			inv4.AH_JH = job4.PK;
			inv5.AH_JH = job5.PK;

			Factory.Save();

			var filter = (OrgWithAddressFilter)FilterBizO[MatchingFilterBusinessObject.DebtorAndAddress];

			filter.Organization = org1.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(5, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));
			Assert(FilterCollection.Contains(inv2.PK));
			Assert(FilterCollection.Contains(inv3.PK));
			Assert(FilterCollection.Contains(inv4.PK));
			Assert(FilterCollection.Contains(inv5.PK));

			filter.Organization = org1.PK;
			filter.Address = address1.PK;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(3, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));
			Assert(FilterCollection.Contains(inv4.PK));
			Assert(FilterCollection.Contains(inv5.PK));

			filter.Organization = org1.PK;
			filter.Address = address2.PK;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv3.PK));

			filter.Address = mainAddress.PK;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			job4.JH_OA_AgentCollectAddr = ZGuid.Empty;
			Factory.Save();

			filter.Address = address1.PK;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(3, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));
			Assert(FilterCollection.Contains(inv4.PK));
			Assert(FilterCollection.Contains(inv5.PK));

			job5.JH_OA_LocalChargesAddr = ZGuid.Empty;
			Factory.Save();

			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(3, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));
			Assert(FilterCollection.Contains(inv4.PK));
			Assert(FilterCollection.Contains(inv5.PK));

			address1.IsCancelled = true;
			Factory.Save();

			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			job5.JH_OA_LocalChargesAddr = address1.PK;
			Factory.Save();

			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));
			Assert(FilterCollection.Contains(inv5.PK));

			inv5.AH_FullyPaidDate = ZDateTime.Now;
			Factory.Save();

			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(!FilterCollection.Contains(inv5.PK));
		}

		public void TestDebtorAndAddressFilterWithOrWithoutInvoiceAddress()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsDebtor = true;
			org1.OH_IsCreditor = true;

			var mainAddress = TestObjectCreator.CreateAddress(org1, OrgAddressType.Office, true);
			var address1 = TestObjectCreator.CreateAddress(org1, OrgAddressType.Receivables, true);
			var address2 = TestObjectCreator.CreateAddress(org1, OrgAddressType.Receivables, false);

			Factory.Save();

			var job1 = TestObjectCreator.CreateJob(org1, 0, TestObjectCreator.Agent, 0);
			var job2 = TestObjectCreator.CreateJob(org1, 0, TestObjectCreator.Agent, 0);
			var job3 = TestObjectCreator.CreateJob(org1, 0, TestObjectCreator.Agent, 0);
			var job4 = TestObjectCreator.CreateJob(org1, 0, TestObjectCreator.Agent, 0);
			var job5 = TestObjectCreator.CreateJob(org1, 0, TestObjectCreator.Agent, 0);

			var inv1 = TestObjectCreator.CreateARInvoice<ARInvoice>("1", TestObjectCreator.AUD, 1m, org1);
			var inv2 = TestObjectCreator.CreateARInvoice<ARInvoice>("2", TestObjectCreator.AUD, 1m, org1);
			var inv3 = TestObjectCreator.CreateARInvoice<ARInvoice>("3", TestObjectCreator.AUD, 1m, org1);
			var inv4 = TestObjectCreator.CreateARInvoice<ARInvoice>("4", TestObjectCreator.AUD, 1m, org1);
			var inv5 = TestObjectCreator.CreateARInvoice<ARInvoice>("5", TestObjectCreator.AUD, 1m, org1);

			inv1.AH_OA_InvoiceAddressOverride = address1.PK;
			inv2.AH_OA_InvoiceAddressOverride = ZGuid.Empty;
			inv3.AH_OA_InvoiceAddressOverride = ZGuid.Empty;
			inv4.AH_OA_InvoiceAddressOverride = address2.PK;
			inv5.AH_OA_InvoiceAddressOverride = ZGuid.Empty;

			TestObjectCreator.CreateInvoiceLine(inv1, 110m, inv1.TransactionCurrency, 1m);
			TestObjectCreator.CreateInvoiceLine(inv2, 110m, inv2.TransactionCurrency, 1m);
			TestObjectCreator.CreateInvoiceLine(inv3, 110m, inv3.TransactionCurrency, 1m);
			TestObjectCreator.CreateInvoiceLine(inv4, 110m, inv4.TransactionCurrency, 1m);
			TestObjectCreator.CreateInvoiceLine(inv5, 110m, inv5.TransactionCurrency, 1m);

			inv1.AH_JH = job1.PK;
			inv2.AH_JH = job2.PK;
			inv3.AH_JH = job3.PK;
			inv4.AH_JH = job4.PK;
			inv5.AH_JH = job5.PK;

			job2.JH_OA_AgentCollectAddr = address1.PK;
			job5.JH_OA_AgentCollectAddr = ZGuid.Empty;

			job2.JH_OA_LocalChargesAddr = ZGuid.Empty;
			job3.JH_OA_LocalChargesAddr = address1.PK;
			job4.JH_OA_LocalChargesAddr = address1.PK;
			job5.JH_OA_LocalChargesAddr = mainAddress.PK;

			Factory.Save();

			FilterBizO.PrimaryOrganization = org1.PK;
			FilterCollection.Load(FilterBizO.Filter);
			AssertEquals(5, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));
			Assert(FilterCollection.Contains(inv2.PK));
			Assert(FilterCollection.Contains(inv3.PK));
			Assert(FilterCollection.Contains(inv4.PK));
			Assert(FilterCollection.Contains(inv5.PK));

			var filter = (OrgWithAddressFilter)FilterBizO[MatchingFilterBusinessObject.DebtorAndAddress];

			filter.IsActive = true;
			filter.Organization = org1.PK;
			filter.Address = address1.PK;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(3, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));
			Assert(FilterCollection.Contains(inv2.PK));
			Assert(FilterCollection.Contains(inv3.PK));

			filter.Organization = org1.PK;
			filter.Address = address2.PK;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv4.PK));

			filter.Organization = org1.PK;
			filter.Address = mainAddress.PK;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv5.PK));
		}

		[SuspendCriticalValidation]
		public void TestCreditorAndAddressFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsCreditor = true;

			var mainAddress = TestObjectCreator.CreateAddress(org1, OrgAddressType.Office, isMain: true);
			var address1 = TestObjectCreator.CreateAddress(org1, OrgAddressType.Payables, isMain: true);
			var address2 = TestObjectCreator.CreateAddress(org1, OrgAddressType.Payables, isMain: false);

			Factory.Save();

			var job1 = TestObjectCreator.CreateJob(org1, 0, TestObjectCreator.Agent, 0);
			var job2 = TestObjectCreator.CreateJob(org1, 0, TestObjectCreator.Agent, 0);
			var job3 = TestObjectCreator.CreateJob(org1, 0, TestObjectCreator.Agent, 0);
			var job4 = TestObjectCreator.CreateJob(org1, 0, TestObjectCreator.Agent, 0);
			var job5 = TestObjectCreator.CreateJob(org1, 0, TestObjectCreator.Agent, 0);

			var inv1 = TestObjectCreator.CreateAPInvoice<APInvoice>("1", TestObjectCreator.AUD, 1m, 1m, 1m, 1m, 1m, 1m, 1m, org1);
			var inv2 = TestObjectCreator.CreateAPInvoice<APInvoice>("2", TestObjectCreator.AUD, 1m, 1m, 1m, 1m, 1m, 1m, 1m, org1);
			var inv3 = TestObjectCreator.CreateAPInvoice<APInvoice>("3", TestObjectCreator.AUD, 1m, 1m, 1m, 1m, 1m, 1m, 1m, org1);
			var inv4 = TestObjectCreator.CreateAPInvoice<APInvoice>("4", TestObjectCreator.AUD, 1m, 1m, 1m, 1m, 1m, 1m, 1m, org1);
			var inv5 = TestObjectCreator.CreateAPInvoice<APInvoice>("5", TestObjectCreator.AUD, 1m, 1m, 1m, 1m, 1m, 1m, 1m, org1);

			inv1.AH_OA_InvoiceAddressOverride = mainAddress.PK;
			inv2.AH_OA_InvoiceAddressOverride = address1.PK;
			inv3.AH_OA_InvoiceAddressOverride = address2.PK;
			inv4.AH_OA_InvoiceAddressOverride = ZGuid.Empty;
			inv5.AH_OA_InvoiceAddressOverride = ZGuid.Empty;

			TestObjectCreator.CreateInvoiceLine(inv1, 110m, inv1.TransactionCurrency, 1m);
			TestObjectCreator.CreateInvoiceLine(inv2, 110m, inv2.TransactionCurrency, 1m);
			TestObjectCreator.CreateInvoiceLine(inv3, 110m, inv3.TransactionCurrency, 1m);
			TestObjectCreator.CreateInvoiceLine(inv4, 110m, inv4.TransactionCurrency, 1m);
			TestObjectCreator.CreateInvoiceLine(inv5, 110m, inv5.TransactionCurrency, 1m);

			job1.JH_OA_LocalChargesAddr = ZGuid.Empty;
			job2.JH_OA_LocalChargesAddr = ZGuid.Empty;
			job3.JH_OA_LocalChargesAddr = address1.PK;
			job4.JH_OA_LocalChargesAddr = ZGuid.Empty;
			job5.JH_OA_LocalChargesAddr = address1.PK;

			job1.JH_OA_AgentCollectAddr = ZGuid.Empty;
			job2.JH_OA_AgentCollectAddr = ZGuid.Empty;
			job3.JH_OA_AgentCollectAddr = ZGuid.Empty;
			job4.JH_OA_AgentCollectAddr = address1.PK;
			job5.JH_OA_AgentCollectAddr = ZGuid.Empty;

			inv1.AH_JH = job1.PK;
			inv2.AH_JH = job2.PK;
			inv3.AH_JH = job3.PK;
			inv4.AH_JH = job4.PK;
			inv5.AH_JH = job5.PK;

			Factory.Save();

			var filter = (OrgWithAddressFilter)FilterBizO[MatchingFilterBusinessObject.CreditorAndAddress];

			filter.Organization = org1.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(5, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));
			Assert(FilterCollection.Contains(inv2.PK));
			Assert(FilterCollection.Contains(inv3.PK));
			Assert(FilterCollection.Contains(inv4.PK));
			Assert(FilterCollection.Contains(inv5.PK));

			filter.Organization = org1.PK;
			filter.Address = address1.PK;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(3, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));
			Assert(FilterCollection.Contains(inv4.PK));
			Assert(FilterCollection.Contains(inv5.PK));

			filter.Organization = org1.PK;
			filter.Address = address2.PK;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv3.PK));

			filter.Address = mainAddress.PK;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			job4.JH_OA_AgentCollectAddr = ZGuid.Empty;
			Factory.Save();

			filter.Address = address1.PK;
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(3, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));
			Assert(FilterCollection.Contains(inv4.PK));
			Assert(FilterCollection.Contains(inv5.PK));

			job5.JH_OA_LocalChargesAddr = ZGuid.Empty;
			Factory.Save();

			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(3, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));
			Assert(FilterCollection.Contains(inv4.PK));
			Assert(FilterCollection.Contains(inv5.PK));

			address1.IsCancelled = true;
			Factory.Save();

			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			job5.JH_OA_LocalChargesAddr = address1.PK;
			Factory.Save();

			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv2.PK));

			inv5.AH_FullyPaidDate = ZDateTime.Now;
			Factory.Save();

			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(!FilterCollection.Contains(inv5.PK));
		}

		public abstract void TestDisbursementRelatingToFilter();

		#region Manual Filter Setters

		public void TestLedgerTransactionTypeFilterSetter()
		{
			var aRInv = Factory.NewWithValidTestData<ARInvoice>();
			var aPInv = Factory.NewWithValidTestData<APInvoice>();

			var aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			var aPCreditNote = Factory.NewWithValidTestData<APCreditNote>();

			var aRPayment = Factory.NewWithValidTestData<ARPayment>();
			var aPPayment = Factory.NewWithValidTestData<APPayment>();

			Factory.Save();

			FilterBizO.SetLedgerTransactionTypeFilter(ZString.Empty, ZString.Empty);
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(6, FilterCollection.Count);
			Assert(FilterCollection.Contains(aRInv.PK));
			Assert(FilterCollection.Contains(aPInv.PK));
			Assert(FilterCollection.Contains(aRCreditNote.PK));
			Assert(FilterCollection.Contains(aPCreditNote.PK));
			Assert(FilterCollection.Contains(aRPayment.PK));
			Assert(FilterCollection.Contains(aPPayment.PK));

			FilterBizO.SetLedgerTransactionTypeFilter(LedgerTypes.AccountsReceivable, ZString.Empty);
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(3, FilterCollection.Count);
			Assert(FilterCollection.Contains(aRInv.PK));
			Assert(FilterCollection.Contains(aRCreditNote.PK));
			Assert(FilterCollection.Contains(aRPayment.PK));

			FilterBizO.SetLedgerTransactionTypeFilter(LedgerTypes.AccountsPayable, ZString.Empty);
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(3, FilterCollection.Count);
			Assert(FilterCollection.Contains(aPInv.PK));
			Assert(FilterCollection.Contains(aPCreditNote.PK));
			Assert(FilterCollection.Contains(aPPayment.PK));

			FilterBizO.SetLedgerTransactionTypeFilter(ZString.Empty, TransactionTypes.Invoice);
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(aRInv.PK));
			Assert(FilterCollection.Contains(aPInv.PK));

			FilterBizO.SetLedgerTransactionTypeFilter(ZString.Empty, TransactionTypes.CreditNote);
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(aRCreditNote.PK));
			Assert(FilterCollection.Contains(aPCreditNote.PK));

			FilterBizO.SetLedgerTransactionTypeFilter(ZString.Empty, TransactionTypes.Payment);
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(aRPayment.PK));
			Assert(FilterCollection.Contains(aPPayment.PK));

			FilterBizO.SetLedgerTransactionTypeFilter(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(aRInv.PK));

			FilterBizO.SetLedgerTransactionTypeFilter(LedgerTypes.AccountsPayable, TransactionTypes.Invoice);
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(aPInv.PK));

			FilterBizO.SetLedgerTransactionTypeFilter(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote);
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(aRCreditNote.PK));

			FilterBizO.SetLedgerTransactionTypeFilter(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote);
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(aPCreditNote.PK));

			FilterBizO.SetLedgerTransactionTypeFilter(LedgerTypes.AccountsReceivable, TransactionTypes.Payment);
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(aRPayment.PK));

			FilterBizO.SetLedgerTransactionTypeFilter(LedgerTypes.AccountsPayable, TransactionTypes.Payment);
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(aPPayment.PK));

			FilterBizO.SetLedgerTransactionTypeFilter("NonExistent", "NonExistent");
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		public void TestAllNumbersFilterSetter()
		{
			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			inv1.IsManuallySetTransactionNumber_ForTestOnly = true;
			inv1.AH_TransactionNum = "1";

			var inv2 = Factory.NewWithValidTestData<ARInvoice>();
			inv2.AH_TransactionReference = "1";

			var inv3 = Factory.NewWithValidTestData<ARInvoice>();
			inv3.AH_ChequeOrReference = "2";

			var inv4 = Factory.NewWithValidTestData<ARInvoice>();
			inv4.AH_ConsolidatedInvoiceRef = "2";

			var inv5 = Factory.NewWithValidTestData<ARInvoice>();
			var ref1 = Factory.NewWithValidTestData<AccTransactionHeaderReference>();
			ref1.AH1_AH = inv5.PK;
			ref1.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRR;
			ref1.AH1_Reference = "3";

			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			shipment1.JS_HouseBill = "3";
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentID = shipment1.PK;
			var inv6 = Factory.NewWithValidTestData<ARInvoice>();
			inv6.AH_JH = shipment1.Job.PK;

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			var transport1 = consol1.Transports[0];
			transport1.JW_Vessel = vessel1.RV_FK;
			transport1.JW_VoyageFlight = "4";
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_IsShipping = true;
			var jobConShipLink1 = Factory.NewWithValidTestData<JobConShipLink>();
			jobConShipLink1.JN_JK = consol1.PK;
			jobConShipLink1.JN_JS = shipment2.PK;
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentID = shipment2.PK;
			var inv7 = Factory.NewWithValidTestData<ARInvoice>();
			inv7.AH_JH = job2.PK;

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_MasterBillNum = "4";
			var jobConShipLink2 = Factory.NewWithValidTestData<JobConShipLink>();
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_IsShipping = false;
			jobConShipLink2.JN_JK = consol2.PK;
			jobConShipLink2.JN_JS = shipment3.PK;
			var job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_ParentID = shipment3.PK;
			var inv8 = Factory.NewWithValidTestData<ARInvoice>();
			inv8.AH_JH = job3.PK;

			var inv9 = Factory.NewWithValidTestData<ARInvoice>();
			inv9.AH_ReceiptBatchNo = "5";

			var inv10 = Factory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(inv10, TestObjectCreator.AUD, 1.0m, 5.0m, 0.0m, 0.0m);

			Factory.Save();

			FilterBizO.SetAllNumbersFilter("1");
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv1.PK));

			FilterBizO.SetAllNumbersFilter("2");
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv3.PK));
			Assert(FilterCollection.Contains(inv4.PK));

			FilterBizO.SetAllNumbersFilter("3");
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv5.PK));
			Assert(FilterCollection.Contains(inv6.PK));

			FilterBizO.SetAllNumbersFilter("4");
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv7.PK));
			Assert(FilterCollection.Contains(inv8.PK));

			FilterBizO.SetAllNumbersFilter("5");
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(inv9.PK));
			Assert(FilterCollection.Contains(inv10.PK));

			FilterBizO.SetAllNumbersFilter("NonExistent");
			FilterCollection.Load(FilterBizO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		#endregion

		#region Clear Filters

		public void TestClearFilters()
		{
			var filter1 = (ModuleNumberFilter)FilterBizO[MatchingFilterBusinessObject.AllNumbers];
			var filter2 = (ModuleTextAndNkFilter)FilterBizO[MatchingFilterBusinessObject.FlightVoyageNumberAndVessel];
			var filter3 = (OrgWithAddressFilter)FilterBizO[MatchingFilterBusinessObject.CreditorAndAddress];
			var filter4 = (DependentListFilter)FilterBizO[MatchingFilterBusinessObject.LedgerTransactionType];

			filter1.IsActive = true;
			filter2.IsActive = true;
			filter3.IsActive = true;
			filter4.IsActive = true;

			filter1.Property = "1";
			filter2.Property = "2";
			filter2.NkProperty = "3";
			filter3.Property = "4";
			filter4.Property1 = "5";
			filter4.Property2 = "6";

			Assert(!filter1.IsEmpty);
			Assert(!filter2.IsEmpty);
			Assert(!filter3.IsEmpty);
			Assert(!filter4.IsEmpty);

			FilterBizO.ClearFilters();

			Assert(filter1.IsEmpty);
			Assert(filter2.IsEmpty);
			Assert(filter3.IsEmpty);
			Assert(filter4.IsEmpty);
		}

		#endregion

		#endregion

		#region DB Reloading

		public void TestFilterForDBReload()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			org1.CompanyData.OB_IsCreditor = true;
			org1.CompanyData.OB_IsDebtor = true;

			var aRInv1 = Factory.NewWithValidTestData<ARInvoice>();
			var aRInv2 = Factory.NewWithValidTestData<ARInvoice>();
			var aRInv3 = Factory.NewWithValidTestData<ARInvoice>();
			var aPInv1 = Factory.NewWithValidTestData<APInvoice>();
			var aPInv2 = Factory.NewWithValidTestData<APInvoice>();

			aRInv1.AH_OH = org1.PK;
			aPInv1.AH_OH = org1.PK;
			aRInv2.AH_OH = org2.PK;
			aPInv2.AH_OH = org2.PK;
			aRInv3.AH_OH = org3.PK;

			TestObjectCreator.CreateInvoiceLine(aRInv1, aRInv1.TransactionCurrency, 1m, 1m);
			TestObjectCreator.CreateInvoiceLine(aPInv1, aPInv1.TransactionCurrency, 1m, 1m);
			TestObjectCreator.CreateInvoiceLine(aRInv2, aRInv2.TransactionCurrency, 1m, 1m);
			TestObjectCreator.CreateInvoiceLine(aPInv2, aPInv2.TransactionCurrency, 1m, 1m);
			TestObjectCreator.CreateInvoiceLine(aRInv3, aRInv3.TransactionCurrency, 1m, 1m);

			Factory.Save();

			FilterBizO.PrimaryOrganization = org1.PK;

			var orgLedgerFilter = FilterBizO.SettlementOrgInfos.AddNew();
			orgLedgerFilter.Organization = org2.PK;

			var primaryOrgLedgerFilter = FilterBizO.SettlementOrgInfos[0];

			FilterBizO.IncludeAllAP = false;
			FilterBizO.IncludeAllAR = false;

			FilterCollection.Load(FilterBizO.FilterForDBReload);
			AssertEquals(0, FilterCollection.Count);

			orgLedgerFilter.APLedger = true;

			FilterCollection.Load(FilterBizO.FilterForDBReload);
			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(aPInv2.PK));

			orgLedgerFilter.ARLedger = true;

			FilterCollection.Load(FilterBizO.FilterForDBReload);
			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(aRInv2.PK));
			Assert(FilterCollection.Contains(aPInv2.PK));

			primaryOrgLedgerFilter.APLedger = true;

			FilterCollection.Load(FilterBizO.FilterForDBReload);
			AssertEquals(3, FilterCollection.Count);
			Assert(FilterCollection.Contains(aPInv1.PK));
			Assert(FilterCollection.Contains(aRInv2.PK));
			Assert(FilterCollection.Contains(aPInv2.PK));

			orgLedgerFilter.ARLedger = false;
			orgLedgerFilter.APLedger = false;

			primaryOrgLedgerFilter.ARLedger = false;
			primaryOrgLedgerFilter.APLedger = false;

			FilterCollection.Load(FilterBizO.FilterForDBReload);
			AssertEquals(0, FilterCollection.Count);
		}

		public void TestFilterForDBReloadMaximumRows()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsDebtor = true;
			org1.OH_IsCreditor = true;

			Factory.Save();

			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			var inv2 = Factory.NewWithValidTestData<ARInvoice>();
			var inv3 = Factory.NewWithValidTestData<ARInvoice>();

			inv1.AH_OH = org1.PK;
			inv2.AH_OH = org1.PK;
			inv3.AH_OH = org1.PK;

			TestObjectCreator.CreateInvoiceLine(inv1, inv1.TransactionCurrency, 1m, 1m);
			TestObjectCreator.CreateInvoiceLine(inv2, inv2.TransactionCurrency, 1m, 1m);
			TestObjectCreator.CreateInvoiceLine(inv3, inv3.TransactionCurrency, 1m, 1m);

			Factory.Save();

			FilterBizO.PrimaryOrganization = org1.PK;

			AccountingConfigurationRegistry.Instance.MaximumResultsInMatchingSearch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			FilterCollection.Load(FilterBizO.FilterForDBReload);

			AssertEquals(1, FilterCollection.Count);

			AccountingConfigurationRegistry.Instance.MaximumResultsInMatchingSearch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			FilterCollection.Load(FilterBizO.FilterForDBReload);

			AssertEquals(2, FilterCollection.Count);

			AccountingConfigurationRegistry.Instance.MaximumResultsInMatchingSearch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);
			FilterCollection.Load(FilterBizO.FilterForDBReload);

			AssertEquals(3, FilterCollection.Count);

			AccountingConfigurationRegistry.Instance.MaximumResultsInMatchingSearch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000);
			FilterCollection.Load(FilterBizO.FilterForDBReload);

			AssertEquals(3, FilterCollection.Count);
		}

		public void TestDBReloadRequired()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsDebtor = true;
			org1.OH_IsCreditor = true;

			Factory.Save();
			Assert(!FilterBizO.IsDBReloadRequired);

			FilterBizO.PrimaryOrganization = org1.PK;
			Assert(FilterBizO.IsDBReloadRequired);
		}

		public void TestDBReloadDoesNotLoadNonARAPTransactions()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsDebtor = true;
			org1.OH_IsCreditor = true;

			Factory.Save();

			var openingRec = Factory.NewWithValidTestData<OpeningReceipt>();
			openingRec.AH_OH = org1.PK;
			openingRec.AH_OSTotal = 90M;
			openingRec.AH_InvoiceAmount = 90M;
			openingRec.AH_OutstandingAmount = 90M;

			var aPInv1 = Factory.NewWithValidTestData<APInvoice>();
			var aRInv1 = Factory.NewWithValidTestData<ARInvoice>();

			aPInv1.AH_OH = org1.PK;
			aRInv1.AH_OH = org1.PK;

			TestObjectCreator.CreateInvoiceLine(aPInv1, aPInv1.TransactionCurrency, 1m, 100m);
			TestObjectCreator.CreateInvoiceLine(aRInv1, aRInv1.TransactionCurrency, 1m, 110m);

			Factory.Save();

			FilterBizO.PrimaryOrganization = org1.PK;
			FilterCollection.Load(FilterBizO.FilterForDBReload);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(aPInv1.PK));
			Assert(FilterCollection.Contains(aRInv1.PK));
		}

		public void TestDBReloadDoesNotCauseStackOverFlow()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsDebtor = true;
			org1.OH_IsCreditor = true;

			Factory.Save();

			for (int i = 0; i < 600; i++)
			{
				var testOrg = Factory.NewWithValidTestData<OrgHeader>();
				testOrg.OH_IsDebtor = true;
				testOrg.OH_IsCreditor = true;

				var relatedParty = testOrg.AllRelatedParties.AddNew();
				relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
				relatedParty.PR_OH_Parent = testOrg.PK;
				relatedParty.PR_OH_RelatedParty = org1.PK;
			}

			Factory.Save();

			var openingRec = Factory.NewWithValidTestData<OpeningReceipt>();
			openingRec.AH_OH = org1.PK;
			openingRec.AH_OSTotal = 90M;
			openingRec.AH_InvoiceAmount = 90M;
			openingRec.AH_OutstandingAmount = 90M;

			var aPInv1 = Factory.NewWithValidTestData<APInvoice>();
			var aRInv1 = Factory.NewWithValidTestData<ARInvoice>();

			aPInv1.AH_OH = org1.PK;
			aRInv1.AH_OH = org1.PK;

			TestObjectCreator.CreateInvoiceLine(aPInv1, aPInv1.TransactionCurrency, 1m, 100m);
			TestObjectCreator.CreateInvoiceLine(aRInv1, aRInv1.TransactionCurrency, 1m, 110m);

			Factory.Save();

			FilterBizO.PrimaryOrganization = org1.PK;
			FilterCollection.Load(FilterBizO.FilterForDBReload);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(aPInv1.PK));
			Assert(FilterCollection.Contains(aRInv1.PK));
		}

		#endregion

		#region Option Recompile

		public void TestFilterGetOptionRecompile()
		{
			Assert(FilterBizO.FilterForDBReload.AddOptionRecompileConditionally);
		}

		#endregion

		#region Descriptions

		public void TestNumberFilterDescriptions()
		{
			var moduleFiltersCollection = FilterBizO.GetModuleFiltersCore_ForTestOnly();

			var allNumbersFilter = moduleFiltersCollection[MatchingFilterBusinessObject.AllNumbers];
			AssertEquals(MatchingFilterBusinessObject.AllNumbers, allNumbersFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.AllNumbers, allNumbersFilter.MultilingualDescription);

			var transactionNumberFilter = moduleFiltersCollection[MatchingFilterBusinessObject.TransactionNumber];
			AssertEquals(MatchingFilterBusinessObject.TransactionNumber, transactionNumberFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.TransactionNumber, transactionNumberFilter.MultilingualDescription);

			var invoiceTransactionReferenceFilter = moduleFiltersCollection[MatchingFilterBusinessObject.InvoiceTransactionReference];
			AssertEquals(MatchingFilterBusinessObject.InvoiceTransactionReference, invoiceTransactionReferenceFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.InvoiceTransactionReference, invoiceTransactionReferenceFilter.MultilingualDescription);

			var jobInvoiceNumberFilter = moduleFiltersCollection[MatchingFilterBusinessObject.JobInvoiceNumber];
			AssertEquals(MatchingFilterBusinessObject.JobInvoiceNumber, jobInvoiceNumberFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.JobInvoiceNumber, jobInvoiceNumberFilter.MultilingualDescription);

			var houseBillNumberFilter = moduleFiltersCollection[MatchingFilterBusinessObject.HouseBillNumber];
			AssertEquals(MatchingFilterBusinessObject.HouseBillNumber, houseBillNumberFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.HouseBillNumber, houseBillNumberFilter.MultilingualDescription);

			var chequeReferenceNumberFilter = moduleFiltersCollection[MatchingFilterBusinessObject.ChequeReferenceNumber];
			AssertEquals(MatchingFilterBusinessObject.ChequeReferenceNumber, chequeReferenceNumberFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.ChequeReferenceNumber, chequeReferenceNumberFilter.MultilingualDescription);

			var invoiceRemittanceReferenceFilter = moduleFiltersCollection[MatchingFilterBusinessObject.InvoiceRemittanceReference];
			AssertEquals(MatchingFilterBusinessObject.InvoiceRemittanceReference, invoiceRemittanceReferenceFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.InvoiceRemittanceReference, invoiceRemittanceReferenceFilter.MultilingualDescription);

			var invoiceBatchNumberFilter = moduleFiltersCollection[MatchingFilterBusinessObject.InvoiceBatchNumber];
			AssertEquals(MatchingFilterBusinessObject.InvoiceBatchNumber, invoiceBatchNumberFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.InvoiceBatchNumber, invoiceBatchNumberFilter.MultilingualDescription);

			var outstandingAmountFilter = moduleFiltersCollection[MatchingFilterBusinessObject.OutstandingAmount];
			AssertEquals(MatchingFilterBusinessObject.OutstandingAmount, outstandingAmountFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.OutstandingAmount, outstandingAmountFilter.MultilingualDescription);
		}

		public void TestDateFilterDescriptions()
		{
			var moduleFiltersCollection = FilterBizO.GetModuleFiltersCore_ForTestOnly();

			var dueDateFilter = (ModuleDateFilter)moduleFiltersCollection[MatchingFilterBusinessObject.DueDate];
			AssertEquals(MatchingFilterBusinessObject.DueDate, dueDateFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.DueDate,
				dueDateFilter.MultilingualDescription
			);

			var transactionDateFilter = moduleFiltersCollection[MatchingFilterBusinessObject.TransactionDate];
			AssertEquals(MatchingFilterBusinessObject.TransactionDate, transactionDateFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.TransactionDate,
				transactionDateFilter.MultilingualDescription
			);
		}

		public void TestGuidFilterDescriptions()
		{
			var moduleFiltersCollection = FilterBizO.GetModuleFiltersCore_ForTestOnly();

			var branchFilter = moduleFiltersCollection[MatchingFilterBusinessObject.Branch];
			AssertEquals(MatchingFilterBusinessObject.Branch, branchFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.Branch,
				branchFilter.MultilingualDescription
			);

			var departmentFilter = moduleFiltersCollection[MatchingFilterBusinessObject.Department];
			AssertEquals(MatchingFilterBusinessObject.Department, departmentFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.Department,
				departmentFilter.MultilingualDescription
			);
		}

		public void TestNkFilterDescriptions()
		{
			var moduleFiltersCollection = FilterBizO.GetModuleFiltersCore_ForTestOnly();

			var currencyFilter = moduleFiltersCollection[MatchingFilterBusinessObject.Currency];
			AssertEquals(MatchingFilterBusinessObject.Currency, currencyFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.Currency,
				currencyFilter.MultilingualDescription
			);
		}

		public void TestTextNkFilterDescriptions()
		{
			var moduleFiltersCollection = FilterBizO.GetModuleFiltersCore_ForTestOnly();

			var flightVoyageVesselNumberFilter = moduleFiltersCollection[MatchingFilterBusinessObject.FlightVoyageNumberAndVessel];
			AssertEquals(MatchingFilterBusinessObject.FlightVoyageNumberAndVessel, flightVoyageVesselNumberFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.FlightVoyageNumberAndVessel,
				flightVoyageVesselNumberFilter.MultilingualDescription
			);
		}

		public void TestTextFilterDescriptions()
		{
			var moduleFiltersCollection = FilterBizO.GetModuleFiltersCore_ForTestOnly();

			var descriptionFilter = moduleFiltersCollection[MatchingFilterBusinessObject.Description];
			AssertEquals(MatchingFilterBusinessObject.Description, descriptionFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.Description,
				descriptionFilter.MultilingualDescription
			);
		}

		public void TestMiscFilterDescriptions()
		{
			var moduleFiltersCollection = FilterBizO.GetModuleFiltersCore_ForTestOnly();

			var ledgerTransactionTypeFilter = (DependentListFilter)moduleFiltersCollection[MatchingFilterBusinessObject.LedgerTransactionType];
			AssertEquals(MatchingFilterBusinessObject.LedgerTransactionType, ledgerTransactionTypeFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.LedgerTransactionType,
				ledgerTransactionTypeFilter.MultilingualDescription
			);

			var defaultLedgerTypesList = (CodeDescriptionPairList)ledgerTransactionTypeFilter.List1;
			var defaultTransactionTypesList = (CodeDescriptionPairList)ledgerTransactionTypeFilter.List2;

			AssertCorrectLedgerTypesList(defaultLedgerTypesList);
			AssertCorrectTransactionTypesList(defaultTransactionTypesList);

			var debtorAndAddressFilter = moduleFiltersCollection[MatchingFilterBusinessObject.DebtorAndAddress];
			AssertEquals(MatchingFilterBusinessObject.DebtorAndAddress, debtorAndAddressFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.DebtorAndAddress,
				debtorAndAddressFilter.MultilingualDescription
			);

			var creditorAndAddress = moduleFiltersCollection[MatchingFilterBusinessObject.CreditorAndAddress];
			AssertEquals(MatchingFilterBusinessObject.CreditorAndAddress, creditorAndAddress.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.CreditorAndAddress,
				creditorAndAddress.MultilingualDescription
			);
		}

		public void TestFlagFilterDescriptions()
		{
			var moduleFiltersCollection = FilterBizO.GetModuleFiltersCore_ForTestOnly();

			var disbursementInvoiceFilter = (ModuleFlagsFilter)moduleFiltersCollection[MatchingFilterBusinessObject.DisbursementInvoice];
			AssertEquals(MatchingFilterBusinessObject.DisbursementInvoice, disbursementInvoiceFilter.Description);
			AssertEquals((NoResString)MatchingFilterBusinessObject.DisbursementInvoice,
				disbursementInvoiceFilter.MultilingualDescription
			);
			AssertEquals((NoResString)"Only Display Disbursement Invoices",
				disbursementInvoiceFilter.FlagNames[0]
			);
		}

		#endregion

		#region Lists

		public void TestTransactionTypesListForReceivables()
		{
			var transactionTypesList = (CodeDescriptionPairList)FilterBizO.GetTransactionTypesList(LedgerTypes.AccountsReceivable);
			AssertCorrectTransactionTypesList(transactionTypesList);
		}

		public void TestTransactionTypesListForPayables()
		{
			var transactionTypesList = (CodeDescriptionPairList)FilterBizO.GetTransactionTypesList(LedgerTypes.AccountsPayable);
			AssertCorrectTransactionTypesList(transactionTypesList);
		}

		public void TestTransactionTypesListForBothPayablesAndReceivables()
		{
			var transactionTypesList = (CodeDescriptionPairList)FilterBizO.GetTransactionTypesList("");
			AssertCorrectTransactionTypesList(transactionTypesList);
		}

		public void TestTransactionTypesListForInvalidLedger()
		{
			var transactionTypesList = (CodeDescriptionPairList)FilterBizO.GetTransactionTypesList("NonExistent");
			AssertEquals(0, transactionTypesList.Count);
		}

		public void TestLedgerTypesList()
		{
			var ledgerTypesList = FilterBizO.LedgerTypesList;
			AssertCorrectLedgerTypesList(ledgerTypesList);
		}

		#endregion

		#region Helper Assertions

		void AssertCorrectLedgerTypesList(CodeDescriptionPairList ledgerTypesList)
		{
			AssertEquals("Must contain 3 types", 3, ledgerTypesList.Count);
			Assert("Must contain AR", ledgerTypesList.ContainsCode(LedgerTypes.AccountsReceivable));
			Assert("Must contain AP", ledgerTypesList.ContainsCode(LedgerTypes.AccountsPayable));
			AssertEquals("Incorrect AR type description", "Accounts Receivable", ledgerTypesList.GetDescriptionFromCode(LedgerTypes.AccountsReceivable));
			AssertEquals("Incorrect AP type description", "Accounts Payable", ledgerTypesList.GetDescriptionFromCode(LedgerTypes.AccountsPayable));
			AssertEquals("Must contain Both Ledger Types", "Both", ledgerTypesList.GetDescriptionFromCode(ZString.Empty));
		}

		void AssertCorrectTransactionTypesList(CodeDescriptionPairList transactionTypesList)
		{
			AssertEquals("Must contain 12 types", 12, transactionTypesList.Count);
			Assert("Must contain ADJ", transactionTypesList.ContainsCode(TransactionTypes.AdjustmentNote));
			Assert("Must contain CTR", transactionTypesList.ContainsCode(TransactionTypes.Contra));
			Assert("Must contain CRD", transactionTypesList.ContainsCode(TransactionTypes.CreditNote));
			Assert("Must contain DSC", transactionTypesList.ContainsCode(TransactionTypes.Discount));
			Assert("Must contain EXX", transactionTypesList.ContainsCode(TransactionTypes.ExchangeDifference));
			Assert("Must contain INV", transactionTypesList.ContainsCode(TransactionTypes.Invoice));
			Assert("Must contain JNL", transactionTypesList.ContainsCode(TransactionTypes.Journal));
			Assert("Must contain OVP", transactionTypesList.ContainsCode(TransactionTypes.Overpayment));
			Assert("Must contain PAY", transactionTypesList.ContainsCode(TransactionTypes.Payment));
			Assert("Must contain REC", transactionTypesList.ContainsCode(TransactionTypes.Receipt));
			Assert("Must contain TRF", transactionTypesList.ContainsCode(TransactionTypes.Transfer));
			AssertEquals(
				"Incorrect Adjustment Note description",
				"Adjustment Note",
				transactionTypesList.GetDescriptionFromCode(TransactionTypes.AdjustmentNote)
			);
			AssertEquals(
				"Incorrect Contra type description",
				"Contra",
				transactionTypesList.GetDescriptionFromCode(TransactionTypes.Contra)
			);
			AssertEquals(
				"Incorrect Credit Note type description",
				"Credit Note",
				transactionTypesList.GetDescriptionFromCode(TransactionTypes.CreditNote)
			);
			AssertEquals(
				"Incorrect Discount type description",
				"Discount",
				transactionTypesList.GetDescriptionFromCode(TransactionTypes.Discount)
			);
			AssertEquals(
				"Incorrect Exchange Difference type description",
				"Exchange Difference",
				transactionTypesList.GetDescriptionFromCode(TransactionTypes.ExchangeDifference)
			);
			AssertEquals(
				"Incorrect Invoice type description",
				"Invoice",
				transactionTypesList.GetDescriptionFromCode(TransactionTypes.Invoice)
			);
			AssertEquals(
				"Incorrect Journal type description",
				"Journal",
				transactionTypesList.GetDescriptionFromCode(TransactionTypes.Journal)
			);
			AssertEquals(
				"Incorrect Overpayment type description",
				"Overpayment",
				transactionTypesList.GetDescriptionFromCode(TransactionTypes.Overpayment)
			);
			AssertEquals(
				"Incorrect Payment type description",
				"Payment",
				transactionTypesList.GetDescriptionFromCode(TransactionTypes.Payment)
			);
			AssertEquals(
				"Incorrect Receipt type description",
				"Receipt",
				transactionTypesList.GetDescriptionFromCode(TransactionTypes.Receipt)
			);
			AssertEquals(
				"Incorrect Transfer type description",
				"Transfer",
				transactionTypesList.GetDescriptionFromCode(TransactionTypes.Transfer)
			);
			AssertEquals(
				"Incorrect All Transaction Types type description",
				"All Transaction Types",
				transactionTypesList.GetDescriptionFromCode(ZString.Empty)
			);
		}

		#endregion

		#region Constants

		public void TestConstants()
		{
			AssertEquals("All Numbers", MatchingFilterBusinessObject.AllNumbers);
			AssertEquals("Transaction #", MatchingFilterBusinessObject.TransactionNumber);
			AssertEquals("Invoice Transaction Reference", MatchingFilterBusinessObject.InvoiceTransactionReference);
			AssertEquals("Invoice Remittance Reference", MatchingFilterBusinessObject.InvoiceRemittanceReference);
			AssertEquals("Job Invoice #", MatchingFilterBusinessObject.JobInvoiceNumber);
			AssertEquals("House Bill #", MatchingFilterBusinessObject.HouseBillNumber);
			AssertEquals("Cheque/Reference #", MatchingFilterBusinessObject.ChequeReferenceNumber);
			AssertEquals("Master Bill #", MatchingFilterBusinessObject.MasterBillNumber);
			AssertEquals("Invoice Batch #", MatchingFilterBusinessObject.InvoiceBatchNumber);
			AssertEquals("Outstanding Amount", MatchingFilterBusinessObject.OutstandingAmount);
			AssertEquals("Due Date", MatchingFilterBusinessObject.DueDate);
			AssertEquals("Transaction Date", MatchingFilterBusinessObject.TransactionDate);
			AssertEquals("Branch", MatchingFilterBusinessObject.Branch);
			AssertEquals("Department", MatchingFilterBusinessObject.Department);
			AssertEquals("Currency", MatchingFilterBusinessObject.Currency);
			AssertEquals("Flight/Voyage # and Vessel", MatchingFilterBusinessObject.FlightVoyageNumberAndVessel);
			AssertEquals("Description", MatchingFilterBusinessObject.Description);
			AssertEquals("Ledger/Transaction Type", MatchingFilterBusinessObject.LedgerTransactionType);
			AssertEquals("Debtor and Address", MatchingFilterBusinessObject.DebtorAndAddress);
			AssertEquals("Creditor and Address", MatchingFilterBusinessObject.CreditorAndAddress);
			AssertEquals("Disbursement Invoice", MatchingFilterBusinessObject.DisbursementInvoice);
			AssertEquals("Disbursement Relating To", MatchingFilterBusinessObject.DisbursementRelatingTo);
		}

		#endregion

		#region Setup

		protected MatchingFilterBusinessObject FilterBizO;
		protected TransactionHeaderCollection FilterCollection;

		protected override void SetUp()
		{
			base.SetUp();
			FilterCollection = new TransactionHeaderCollection(Factory);
			FilterBizO = (MatchingFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		#endregion

		#region Test Object Creator

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
			}
		}
		TestObjectCreator fTestObjectCreator;

		#endregion
	}
}
