using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(Checkout))]
	sealed class CheckoutTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRunPreSaveValidation()
		{
			checkout.TrackingNumber = "1";
			checkout.RunPreSaveValidation();
			AssertEquals(true, checkout.TrackingNumberInfo.HasError("Tracking number is not found."));
			AssertEquals(true, checkout.LabelPrinterInfo.HasErrors());
			AssertEquals(true, checkout.InvoicePrinterInfo.HasErrors());
		}

		public void TestValidateTrackingNumber()
		{
			checkout.TrackingNumber = "1";
			AssertEquals(true, checkout.TrackingNumberInfo.HasError("Tracking number is not found."));
			checkout.TrackingNumber = "123132";
			AssertEquals(true, checkout.TrackingNumberInfo.HasError("Tracking number is not found."));
			var callout = Factory.New<Callout>();
			callout.CS_CM = (Factory.New<CusMAWB>()).PK;
			CreateJobRelatedWaybill(callout.PK, "123456789012345678", "12345678901", JobRelatedWayBill.Constants.RelatedWayBillType.Parent);
			CreateJobRelatedWaybill(callout.PK, "1ZA123456789456132", "12345678902", JobRelatedWayBill.Constants.RelatedWayBillType.Child);
			CreateJobRelatedWaybill(callout.PK, "98765432101", "12345678903", JobRelatedWayBill.Constants.RelatedWayBillType.Child);
			Factory.Save();
			checkout.TrackingNumber = "1";
			AssertEquals(true, checkout.TrackingNumberInfo.HasError("Tracking number is not found."));
			checkout.TrackingNumber = "123132";
			AssertEquals(true, checkout.TrackingNumberInfo.HasError("Tracking number is not found."));
			checkout.TrackingNumber = "123456789012345678";
			AssertEquals(false, checkout.TrackingNumberInfo.HasErrors());
			checkout.TrackingNumber = "1ZA123456789456132";
			AssertEquals(false, checkout.TrackingNumberInfo.HasErrors());
			checkout.TrackingNumber = "98765432101";
			AssertEquals(false, checkout.TrackingNumberInfo.HasErrors());
		}

		public void TestLabelPrinterValidation()
		{
			SetupPrinters();
			checkout.LabelPrinter = ZGuid.Empty;
			Assert(checkout.LabelPrinterInfo.HasErrors());
			checkout.LabelPrinter = Printer.PK;
			Assert(!checkout.LabelPrinterInfo.HasErrors());
		}

		public void TestInvoicePrinterValidation()
		{
			SetupPrinters();
			checkout.InvoicePrinter = ZGuid.Empty;
			Assert(checkout.InvoicePrinterInfo.HasErrors());
			checkout.InvoicePrinter = Printer.PK;
			Assert(!checkout.InvoicePrinterInfo.HasErrors());
		}

		public void TestPrinterNames()
		{
			AssertNotNull(checkout.PrinterNames);
		}

		public void TestPrint_InvalidTrackingNumber()
		{
			SetupPrinters();
			checkout.Print();
			AssertEquals("No print job is created", 0, PrintJobs.Length);
		}

		public void TestPrint_LabelOnly()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			SetupPrinters();
			CreateSavedCallout("99999");
			checkout.TrackingNumber = "99999";
			checkout.LabelPrinter = Printer.PK;
			checkout.Print();
			AssertEquals("1 print job is created", 1, PrintJobs.Length);
		}

		public void TestPrint_MultipleCallouts()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			SetupPrinters();
			CreateSavedCallout("99999");
			CreateSavedCallout("99999");
			CreateSavedCallout("99999");
			checkout.TrackingNumber = "99999";
			checkout.LabelPrinter = Printer.PK;
			checkout.Print();
			AssertEquals("3 print jobs are created", 3, PrintJobs.Length);
		}

		public void TestPrint_IsCOD()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			SetupPrinters();
			var callout = Factory.New<Callout>();
			callout.CS_CM = (Factory.New<CusMAWB>()).PK;
			callout.CS_HAWB = "99999";
			CreateJobRelatedWaybill(callout.PK, "99999", string.Empty, JobRelatedWayBill.Constants.RelatedWayBillType.Parent);
			callout.Payment.IsCheque = true;
			Factory.Save();
			checkout.TrackingNumber = "99999";
			checkout.LabelPrinter = Printer.PK;
			checkout.InvoicePrinter = Printer.PK;
			checkout.Print();
			AssertEquals("2 print jobs are created", 2, PrintJobs.Length);
		}

		public void TestSaveFormState_SetFormState_LabelPrinter()
		{
			checkout.LabelPrinter = Printer.PK;
			checkout.SaveFormState();
			checkout.LabelPrinter = ZGuid.Empty;
			checkout.SetFormState();
			AssertEquals(Printer.PK, checkout.LabelPrinter);
		}

		public void TestSaveFormState_SetFormState_InvoicePrinter()
		{
			checkout.InvoicePrinter = Printer.PK;
			checkout.SaveFormState();
			checkout.InvoicePrinter = ZGuid.Empty;
			checkout.SetFormState();
			AssertEquals(Printer.PK, checkout.InvoicePrinter);
		}

		public void TestSaveFormState_SetFormState_InvalidPrinter()
		{
			checkout.InvoicePrinter = ZGuid.NewZGuid();
			checkout.SaveFormState();
			checkout.InvoicePrinter = ZGuid.Empty;
			checkout.SetFormState();
			AssertEquals(ZGuid.Empty, checkout.InvoicePrinter);
		}

		protected override BusinessObject GetNewBusinessObject() => new Checkout(Factory);

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			checkout = new Checkout(Factory);
		}

		Checkout checkout;

		void CreateJobRelatedWaybill(ZGuid parentID, string longTrackingNumber, string shortTrackingNumber, string waybillType)
		{
			var jobRelatedWayBill = Factory.New<JobRelatedWayBill>();
			jobRelatedWayBill.EB_ParentID = parentID;
			jobRelatedWayBill.EB_ParentTableCode = CusHAWBSchema.Constants.Prefix;
			jobRelatedWayBill.EB_WaybillNumber = longTrackingNumber;
			jobRelatedWayBill.EB_WaybillShortNumber = shortTrackingNumber;
			jobRelatedWayBill.EB_WaybillType = waybillType;
		}

		Callout CreateSavedCallout(string hAWBNumber)
		{
			var result = Factory.New<Callout>();
			result.CS_CM = (Factory.New<CusMAWB>()).PK;
			CreateJobRelatedWaybill(result.PK, hAWBNumber, string.Empty, JobRelatedWayBill.Constants.RelatedWayBillType.Parent);
			result.CS_HAWB = hAWBNumber;
			Factory.Save();
			return result;
		}

		void SetupPrinters()
		{
			AssertNotNull(Printer);
		}

		StmPrintJob[] PrintJobs => Factory.Load<StmPrintJob>(PrintJobsFilter);

		ZQuery PrintJobsFilter => new ZQuery(StmPrintJobSchema.SP_SQ, Printer.PK);

		StmPrintQueue printer;
		StmPrintQueue Printer
		{
			get
			{
				if (printer == null)
				{
					printer = Factory.New<StmPrintQueue>();
					Printer.SQ_ServerName = System.Environment.MachineName;
					Printer.SQ_DisplayName = "CheckoutPrinter_DisplayName";
					Printer.SQ_QueueName = "CheckoutPrinter_QueueName";
					Factory.Save();
				}

				return printer;
			}
		}
	}
}
