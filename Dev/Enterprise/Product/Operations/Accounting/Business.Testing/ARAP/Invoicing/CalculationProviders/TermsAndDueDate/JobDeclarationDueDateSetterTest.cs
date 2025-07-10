using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	public class JobDeclarationDueDateSetterTest : TestCaseWithFactory
	{
		public void TestShipmentDateImport()
		{
			ZDateTime expectedDate = new ZDateTime(2004, 9, 13);

			TestDeclaration.JE_MessageType = "IMP";
			TestDeclaration.JE_RL_NKPortOfLoading = USLAX.Code;
			TestDeclaration.JE_RL_NKPortOfArrival = CNSHA.Code;

			Transport transport = TestDeclaration.Transports.AddNew();
			transport.JW_RL_NKLoadPort = USLAX.Code;
			transport.JW_RL_NKDiscPort = CNSHA.Code;
			transport.JW_ATA = expectedDate;

			TestDeclaration.JE_RL_NKOrigin = USLAX.Code;
			TestDeclaration.JE_RL_NKFinalDestination = CNSHA.Code;

			InvoiceAndDueDateCalculatorForTest testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestDeclaration, Invoice.AH_InvoiceDate, new InvoiceTerm(InvoiceTermsList.FromShipmentDate.Code, "", 0));

			ZDateTime shipmentDate = testDateSetter.GetDateByInvoiceTerm();

			AssertEquals("Shipment Date is incorrect.", expectedDate, shipmentDate);
		}

		public void TestShipmentDateImportWithETA()
		{
			ZDateTime expectedDate = new ZDateTime(2004, 9, 13);

			TestDeclaration.JE_MessageType = "IMP";
			TestDeclaration.JE_RL_NKPortOfLoading = USLAX.Code;
			TestDeclaration.JE_RL_NKPortOfArrival = CNSHA.Code;

			Transport transport = TestDeclaration.Transports.AddNew();
			transport.JW_RL_NKLoadPort = USLAX.Code;
			transport.JW_RL_NKDiscPort = CNSHA.Code;
			transport.JW_ETA = expectedDate;

			TestDeclaration.JE_RL_NKOrigin = USLAX.Code;
			TestDeclaration.JE_RL_NKFinalDestination = CNSHA.Code;

			InvoiceAndDueDateCalculatorForTest testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestDeclaration, Invoice.AH_InvoiceDate, new InvoiceTerm(InvoiceTermsList.FromShipmentDate.Code, "", 0));

			AssertEquals("Shipment Date is incorrect.", expectedDate, testDateSetter.GetDateByInvoiceTerm());

			//Fallback to shipment dates if consol dates are empty
			transport.JW_ETA = ZDateTime.Empty;
			AssertEquals("Shipment Date should be InvoiceDate", Invoice.AH_InvoiceDate, testDateSetter.GetDateByInvoiceTerm());
		}

		public void TestShipmentDateExport()
		{
			ZDateTime expectedDate = new ZDateTime(2004, 9, 13);

			TestDeclaration.JE_RL_NKPortOfLoading = CNSHA.Code;
			TestDeclaration.JE_RL_NKPortOfArrival = USLAX.Code;

			Transport transport = TestDeclaration.Transports.AddNew();
			transport.JW_RL_NKLoadPort = USLAX.Code;
			transport.JW_RL_NKDiscPort = CNSHA.Code;
			transport.JW_ATD = expectedDate;

			TestDeclaration.JE_RL_NKOrigin = CNSHA.Code;
			TestDeclaration.JE_RL_NKFinalDestination = USLAX.Code;

			InvoiceAndDueDateCalculatorForTest testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestDeclaration, Invoice.AH_InvoiceDate, new InvoiceTerm(InvoiceTermsList.FromShipmentDate.Code, "", 0));

			ZDateTime shipmentDate = testDateSetter.GetDateByInvoiceTerm();

			AssertEquals("Shipment Date is incorrect.", expectedDate, shipmentDate);
		}

		public void TestShipmentDateExportWithETA()
		{
			ZDateTime expectedDate = new ZDateTime(2004, 9, 13);

			TestDeclaration.JE_RL_NKPortOfLoading = CNSHA.Code;
			TestDeclaration.JE_RL_NKPortOfArrival = USLAX.Code;

			Transport transport = TestDeclaration.Transports.AddNew();
			transport.JW_RL_NKLoadPort = USLAX.Code;
			transport.JW_RL_NKDiscPort = CNSHA.Code;
			transport.JW_ETD = expectedDate;

			TestDeclaration.JE_RL_NKOrigin = CNSHA.Code;
			TestDeclaration.JE_RL_NKFinalDestination = USLAX.Code;

			InvoiceAndDueDateCalculatorForTest testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestDeclaration, Invoice.AH_InvoiceDate, new InvoiceTerm(InvoiceTermsList.FromShipmentDate.Code, "", 0));

			AssertEquals("Shipment Date is incorrect.", expectedDate, testDateSetter.GetDateByInvoiceTerm());

			//Fallback to shipment dates if consol dates are empty
			transport.JW_ETD = ZDateTime.Empty;
			AssertEquals("Shipment Date should be InvoiceDate", Invoice.AH_InvoiceDate, testDateSetter.GetDateByInvoiceTerm());
		}

		[TestDate(2004, 12, 15)]
		public void TestInvoiceDateWithEmptyShipmentDate()
		{
			InvoiceTerm term = new InvoiceTerm("SHP", "", 21);

			TestDeclaration.JE_RL_NKOrigin = CNSHA.Code;
			TestDeclaration.JE_RL_NKFinalDestination = USLAX.Code;

			InvoiceAndDueDateCalculatorForTest testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestDeclaration, Invoice.AH_InvoiceDate, term);

			AssertEquals(ZDateTime.Today, testDateSetter.InvoiceDate);
		}

		[ExpectNoExceptions]
		[TestDate(2004, 12, 15)]
		public void TestIfDeclarationIsNullthrowsNoException()
		{
			InvoiceTerm term = new InvoiceTerm("SHP", "", 21);

			InvoiceAndDueDateCalculatorForTest testDateSetter = new InvoiceAndDueDateCalculatorForTest(null, Invoice.AH_InvoiceDate, term);

			testDateSetter.GetDateByInvoiceTerm();
		}

		#region Implementation

		BaseJobDeclaration TestDeclaration;
		RefUNLOCO CNSHA;
		RefUNLOCO USLAX;
		Job TestJob;
		ARInvoice Invoice;

		protected override void SetUp()
		{
			base.SetUp();

			USLAX = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "USLAX") as RefUNLOCO;
			CNSHA = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "CNSHA") as RefUNLOCO;
			Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUSYD");

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = CNSHA.Code;

			TestJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			TestDeclaration = GetDeclaration(TestJob);
			Invoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
		}

		protected BaseJobDeclaration GetDeclaration(Job job)
		{
			BaseJobDeclaration result = Factory.New<BaseJobDeclaration>();
			job.JH_ParentID = result.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

			return result;
		}

		#endregion
	}
}
