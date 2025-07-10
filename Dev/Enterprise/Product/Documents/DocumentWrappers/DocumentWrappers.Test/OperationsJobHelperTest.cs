using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.Accounting.Testing
{
	sealed class OperationsJobHelperTest : TestCaseWithFactory
	{
		#region TestOperationsJob

		public void TestOperationsJob()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var chargeCodePK = objectCreator.CC1.PK;

			Action<IJobInvoicingPlugIn> createNewJob = (IJobInvoicingPlugIn plugin) =>
			{
				objectCreator = new TestObjectCreator(Factory);
				Invoice = Factory.New<ARInvoice>();
				Line = (ARInvoiceLine)Invoice.Lines.AddNew();
				JobHeader jobHeader = objectCreator.CreateJob(plugin, false);
				Line.AL_AC = chargeCodePK;
				Line.AL_JH = jobHeader.PK;
				objectCreator.CreateCharge(Line);
			};

			Action<IJobInvoicingPlugIn, Type, string, string> assertOperationsJob = (parent, expectedWrapperType, expectedJobNumber, expectedJobDescription) =>
			{
				createNewJob(parent);
				InvoiceLineWrapper = RecreateTestingDocWrapper(Line, (line, factory) => DocARInvoiceLine.New(line, factory));
				AssertEquals(expectedWrapperType, InvoiceLineWrapper.OperationsJob.GetType());
				AssertEquals(expectedJobNumber, InvoiceLineWrapper.OperationsJob.JobNumber);
			};

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1000";
			assertOperationsJob(shipment, typeof(FreightWrapperFromShipment), "S1000", "SHIPMENT JOBS");

			var cfsShipment = Factory.New<CFSShipment>();
			cfsShipment.JS_UniqueConsignRef = "S2000";
			cfsShipment.JS_IsCFSRegistered = true;
			cfsShipment.JS_IsForwardRegistered = false;
			assertOperationsJob(cfsShipment, typeof(FreightWrapperFromCFSShipment), "S2000", "CTO / CFS JOBS");

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_UniqueConsignRef = "C1000";
			assertOperationsJob(loadList, typeof(FreightWrapperFromCFSLoadList), "C1000", "CTO / CFS JOBS");

			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "3333";
			assertOperationsJob(cartage, typeof(FreightWrapperFromCartage), "3333", "LOCAL CARTAGE");

			var consignment = Helper.CreateBookingConsignment();
			consignment.KM_JobID = "5555";
			assertOperationsJob(consignment, typeof(FreightWrapperFromDtbBookingConsignment), "5555", "CONSIGNMENT");

			var ltConsignment = new TransportConsignmentTestHelper(Factory).CreateConsignment("LT00001");
			assertOperationsJob(ltConsignment, typeof(FreightWrapperFromDtbConsignment), "LT00001", "LTCONSIGNMENT");

			var sundryChargers = Factory.NewWithValidTestData<SundryCharges>();
			sundryChargers.D4_JobNumber = "6666";
			assertOperationsJob(sundryChargers, typeof(FreightWrapperFromSundryCharges), "6666", "SUNDRY CHARGES");

			var voyageAccounting = Factory.NewWithValidTestData<VoyageAccount>();
			voyageAccounting.NA_JobNumber = "7777";
			assertOperationsJob(voyageAccounting, typeof(FreightWrapperFromVoyageAccount), "7777", "VOYAGE ACCOUNTING");

			var containerDetention = Factory.NewWithValidTestData<ContainerDetention>();
			containerDetention.Principal.OH_Code = "BLAHHHH"; // to satisfy db constraint
			containerDetention.NC_JobNumber = "8888";
			assertOperationsJob(containerDetention, typeof(FreightWrapperFromDetentionInvoice), "8888", "CONTAINER DETENTION");

			createNewJob(Factory.New<CFSContainer>());
			InvoiceLineWrapper = RecreateTestingDocWrapper(Line, (line, factory) => DocARInvoiceLine.New(line, Factory));
			AssertEquals("MISCELLANEOUS JOBS", InvoiceLineWrapper.JobTypeForPeriodicInvoice);

			var containerRegistrationJob = Factory.NewWithValidTestData<CFSContainer>();
			containerRegistrationJob.JC_ContainerJobID = "D000010001";
			assertOperationsJob(containerRegistrationJob, typeof(FreightWrapperFromCFSContainer), "D000010001", "MISCELLANEOUS JOBS");
		}

		#endregion

		#region TestIsLandConsignmentJob

		public void TestIsLandConsignmentJob()
		{
			var consignment = new TransportConsignmentTestHelper(Factory).CreateConsignment("LT00001");
			var jobHeader = new TestObjectCreator(Factory).CreateJob(consignment, false);
			var operationJobHelper = new OperationsJobHelper(DocJobHeader.New(jobHeader, Factory), null, null, Factory);
			AssertEquals(true, operationJobHelper.IsLandConsignmentJob);
		}

		#endregion

		#region TestOperationsJob_Warehouse

		public void TestOperationsJob_Warehouse()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "External Reference";
			helper.CreateAccountingDataWithNoCharge(receive);

			var job = new Job.Loader(receive).Load(true);
			var arLine1 = CreateARLine(job);
			arLine1.AL_AC = chargeCode.PK;

			var charge1 = objectCreator.CreateCharge(arLine1);
			var attrib1 = charge1.JobChargeAttributes.AddNew();
			attrib1.EC_Name = JobChargeAttribTypeList.Codes.DocketReference;
			attrib1.EC_Value = "External Reference";

			var arLine1Wrapper = DocARInvoiceLine.New(arLine1, Factory);
			AssertEquals(true, arLine1Wrapper.IsWarehouseJob);
			var arLine1ParentWrapper = arLine1Wrapper.OperationsJob as FreightWrapperFromWhsBO;
			AssertEquals(receive, arLine1ParentWrapper.WrappedObject);
			AssertEquals("WAREHOUSE JOBS", arLine1Wrapper.JobTypeForPeriodicInvoice);

			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			helper.CreateAccountingDataWithCharge(invoice);

			var arLine2 = CreateARLine(invoice.JobHeader);
			arLine2.AL_AC = chargeCode.PK;
			var charge2 = objectCreator.CreateCharge(arLine2);
			var arLine2Wrapper_1 = DocARInvoiceLine.New(arLine2, Factory);
			AssertEquals(true, arLine2Wrapper_1.IsWarehouseJob);
			var arLine2ParentWrapper_1 = arLine2Wrapper_1.OperationsJob as FreightWrapperFromWhsInvoice;
			AssertNotNull(arLine2ParentWrapper_1);
			AssertEquals("WAREHOUSE JOBS", arLine2Wrapper_1.JobTypeForPeriodicInvoice);

			Factory.ClearCachedValue<WhsDocket>(string.Format("DocketFromCharge-{0}", charge2.PK));

			var attrib2 = charge2.JobChargeAttributes.AddNew();
			attrib2.EC_Name = JobChargeAttribTypeList.Codes.DocketReference;
			attrib2.EC_Value = "External Reference";

			var arLine2Wrapper_2 = DocARInvoiceLine.New(arLine2, Factory);
			var arLine2ParentWrapper_2 = arLine2Wrapper_2.OperationsJob as FreightWrapperFromWhsBO;
			AssertEquals(true, arLine2Wrapper_2.IsWarehouseJob);
			AssertEquals(receive, arLine2ParentWrapper_2.WrappedObject);
			AssertEquals("WAREHOUSE JOBS", arLine2Wrapper_2.JobTypeForPeriodicInvoice);

			receive.WD_ExternalReference = "Other Reference";
			arLine2ParentWrapper_2 = arLine2Wrapper_2.OperationsJob as FreightWrapperFromWhsBO;
			AssertEquals("Warehouse job should be cached", true, arLine2Wrapper_2.IsWarehouseJob);
			AssertEquals("Warehouse job should be cached", receive, arLine2ParentWrapper_2.WrappedObject);
		}

		ARInvoiceLine CreateARLine(JobHeader job)
		{
			var invoice = Factory.New<ARInvoice>();
			var line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_JH = job.PK;
			return line;
		}

		#endregion

		#region TestIsContainerRegistrationJob

		public void TestIsContainerRegistrationJob()
		{
			var containerRegistrationJob = Factory.NewWithValidTestData<CFSContainer>();
			containerRegistrationJob.JC_ContainerJobID = "D000010001";
			var jobHeader = new TestObjectCreator(Factory).CreateJob(containerRegistrationJob, false);
			var operationJobHelper = new OperationsJobHelper(DocJobHeader.New(jobHeader, Factory), null, null, Factory);
			AssertEquals(true, operationJobHelper.IsContainerRegistrationJob);
		}

		#endregion

		#region Helper

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}
		TransportBookingConsignmentTestHelper helper;

		#endregion

		TResult RecreateTestingDocWrapper<T, TResult>(T bizo, Func<T, BusinessObjectFactory, TResult> createWrapper)
			where T : BusinessObject
			where TResult : DocumentWrapper
		{
			Factory.Save();
			bizo.Factory.Save();
			ReleaseFactory();
			return createWrapper(Factory.Load<T>(bizo.PK), Factory);
		}

		ARInvoice Invoice;
		ARInvoiceLine Line;
		DocARInvoiceLine InvoiceLineWrapper;
	}
}
