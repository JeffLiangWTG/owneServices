using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.GUI.Testing
{
	sealed class CheckoutFormTest : TestCaseWithFactory
	{
		public void TestFormCaption()
		{
			using (var form = new CheckoutForm(checkout))
			{
				AssertEquals("Operations", form.FormCaption);
			}
		}

		public void TestPrint()
		{
			using (var form = new CheckoutForm(checkout))
			{
				form.Show();
				form.PrintButton.PerformClick();
				AssertEquals("Error Please fix the errors before printing.", UnitTestUserNotification.Instance.LastMessage.ToString());
				TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
				var cusMAWB = Factory.New<CusMAWB>();
				var callout = Factory.New<Callout>();
				callout.CS_CM = Factory.New(typeof(CusMAWB)).PK;
				CreateJobRelatedWaybill(callout.PK, "99999", "", JobRelatedWayBill.Constants.RelatedWayBillType.Parent);
				Factory.Save();
				checkout.TrackingNumber = "99999";
				checkout.LabelPrinter = Printer.PK;
				checkout.InvoicePrinter = Printer.PK;
				form.TrackingNumberTextBox.Focus();
				KeySender.PostKeyDown(form.TrackingNumberTextBox, Keys.Enter);
				Application.DoEvents();
				AssertEquals("1 print job is created", 1, PrintJobs.Length);
			}
		}

		public void Test_FormState()
		{
			checkout.LabelPrinter = Printer.PK;
			using (var form = new CheckoutForm(checkout))
			{
				form.Show();
				form.Close();
			}

			checkout.LabelPrinter = ZGuid.Empty;
			using (var form = new CheckoutForm(checkout))
			{
				form.Show();
				AssertEquals(Printer.PK, checkout.LabelPrinter);
			}
		}

		void CreateJobRelatedWaybill(ZGuid parentID, string longTrackingNumber, string shortTrackingNumber, string waybillType)
		{
			var jobRelatedWayBill = Factory.New<JobRelatedWayBill>();
			jobRelatedWayBill.EB_ParentID = parentID;
			jobRelatedWayBill.EB_ParentTableCode = CusHAWBSchema.Constants.Prefix;
			jobRelatedWayBill.EB_WaybillNumber = longTrackingNumber;
			jobRelatedWayBill.EB_WaybillShortNumber = shortTrackingNumber;
			jobRelatedWayBill.EB_WaybillType = waybillType;
		}

		StmPrintJob[] PrintJobs => Factory.Load<StmPrintJob>(PrintJobsFilter);

		ZQuery PrintJobsFilter => new ZQuery(StmPrintJobSchema.SP_SQ, Printer.PK);

		StmPrintQueue Printer
		{
			get
			{
				if (fPrinter == null)
				{
					fPrinter = Factory.New<StmPrintQueue>();
					Printer.SQ_ServerName = System.Environment.MachineName;
					Printer.SQ_DisplayName = "CheckoutPrinterForm_DisplayName";
					Printer.SQ_QueueName = "CheckoutPrinterForm_QueueName";
					Factory.Save();
				}

				return fPrinter;
			}
		}

		StmPrintQueue fPrinter;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			checkout = new Checkout(Factory);
		}

		Checkout checkout;
	}
}
