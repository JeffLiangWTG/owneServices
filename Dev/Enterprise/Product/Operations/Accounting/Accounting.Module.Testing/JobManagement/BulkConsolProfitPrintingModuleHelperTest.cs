using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.Testing
{
	public class BulkConsolProfitPrintingModuleHelperTest : TestCaseWithFactory
	{
		public void TestMenuItem()
		{
			IBulkJobProfitPrintingModuleHelper helper = new BulkConsolProfitPrintingModuleHelper();
			bool called = false;
			MenuItem menu = (MenuItem)helper.GetMenuItem(() => called = true);
			menu.PerformClick();
			Assert(called);
			AssertEquals(Constants.MenuNameConstants.PrintJobProfitDoc, menu.Text);
		}

		public void TestSecurity()
		{
			IBulkJobProfitPrintingModuleHelper helper = new BulkConsolProfitPrintingModuleHelper();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			MenuItem menu = (MenuItem)helper.GetMenuItem(() =>
					helper.PrintJobProfitDocument(Factory, new BusinessObject[] { consol }));
			Env.Security.ConsolBulkPrintJobProfitDoc.IsAllowed = false;
			menu.PerformClick();
			Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		public void TestPrinting()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();

			IBulkJobProfitPrintingModuleHelper helper = new BulkConsolProfitPrintingModuleHelperForTest();
			MenuItem menu = (MenuItem)helper.GetMenuItem(() =>
					helper.PrintJobProfitDocument(Factory, new BusinessObject[] { consol1, consol2 }));

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			menu.PerformClick();
			AssertEquals("Are you sure you want to print Job Profit Document for following job(s): " +
					consol1.JK_UniqueConsignRef + ", " +
					consol2.JK_UniqueConsignRef,
					UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNull("JobDocumentPrinter", ((BulkConsolProfitPrintingModuleHelperForTest)helper).JobDocumentPrinter);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			menu.PerformClick();
			JobDocumentPrintItem[] lastPrintedItems =
				((BulkConsolProfitPrintingModuleHelperForTest)helper).JobDocumentPrinter.LastPrintedItems;
			AssertNull("LastPrintedItems", lastPrintedItems);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			menu.PerformClick();
			lastPrintedItems =
				((BulkConsolProfitPrintingModuleHelperForTest)helper).JobDocumentPrinter.LastPrintedItems;
			AssertNotNull("LastPrintedItems", lastPrintedItems);
			AssertEquals(2, lastPrintedItems.Length);
			AssertEquals(consol1, ((ConsolJobDocumentPrintItem)lastPrintedItems[0]).Consol);
			AssertEquals(consol2, ((ConsolJobDocumentPrintItem)lastPrintedItems[1]).Consol);
		}

		class BulkConsolProfitPrintingModuleHelperForTest : BulkConsolProfitPrintingModuleHelper
		{
			protected override JobDocumentPrinter CreateJobDocumentPrinter(BusinessObjectFactory factory)
			{
				JobDocumentPrinter = new ConsolJobDocumentPrinterForTest(factory);
				return JobDocumentPrinter;
			}

			public ConsolJobDocumentPrinterForTest JobDocumentPrinter;
		}

		class ConsolJobDocumentPrinterForTest : ConsolJobDocumentPrinter
		{
			public ConsolJobDocumentPrinterForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override void PrintJobProfitDocuments(BusinessObjectFactory factory, params JobDocumentPrintItem[] jobDocumentPrinters)
			{
				LastPrintedItems = jobDocumentPrinters;
			}

			public JobDocumentPrintItem[] LastPrintedItems;
		}
	}
}
