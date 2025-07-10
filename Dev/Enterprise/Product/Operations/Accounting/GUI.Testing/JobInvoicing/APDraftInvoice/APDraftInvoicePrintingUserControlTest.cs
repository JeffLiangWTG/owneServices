using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class APDraftInvoicePrintingUserControlTest : TestCaseWithFactory
	{
		public void TestGridLoading()
		{
			using (var form = new ZForm())
			using (var control = new APDraftInvoicePrintingUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var draftInvoice = TestObjectCreator.CreateDraftInvoice("INV-0001", "ADI-IREF-001", testObjectCreator.Creditor1.PK, 200M, 0M, "AUD");
				var cluster = TestObjectCreator.AddClusterToDraftTransaction(draftInvoice, 200M);
				var draftInvoiceJob = TestObjectCreator.AddJobToTheCluster(cluster, shipment);

				Factory.Save();

				var filter = new JobDraftInvoicePrintingFilter(shipment);
				control.Bind(filter);

				control.DraftInvoicesGrid.SelectAllElements();
				Application.DoEvents();

				Assert(control.DraftInvoicesGrid.Columns.Contains("PostingStatus"));
				AssertEquals("SelectedRowCount", 1, control.DraftInvoicesGrid.VisibleRowCount);
			}
		}

		public void TestInvoiceFilterObjectIsNotInitialised()
		{
			using (var control = new APDraftInvoicePrintingUserControl())
			{
				var findButton = control.Controls.Find("FindButton", true).Single() as ZButton;
				var clearButton = control.Controls.Find("ClearButton", true).Single() as ZButton;
				var uploadInvoiceButton = control.Controls.Find("UploadInvoiceButton", true).Single() as ZButton;

				AssertNoExceptionThrown("A null InvoiceFilterObject should be handled in FindButton_Click()", findButton.PerformClick);
				AssertNoExceptionThrown("A null InvoiceFilterObject should be handled in ClearButton_Click()", clearButton.PerformClick);
				AssertNoExceptionThrown("A null InvoiceFilterObject should be handled in UploadInvoiceButton_Click()", uploadInvoiceButton.PerformClick);
			}
		}

		public void TestUploadInvoiceButtonClick()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (var form = new ZForm())
			using (var control = new APDraftInvoicePrintingUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var draftInvoice = TestObjectCreator.CreateDraftInvoice("INV-0001", "ADI-IREF-001", testObjectCreator.Creditor1.PK, 200M, 0M, "AUD");
				var cluster = TestObjectCreator.AddClusterToDraftTransaction(draftInvoice, 200M);
				var draftInvoiceJob = TestObjectCreator.AddJobToTheCluster(cluster, shipment);

				Factory.Save();

				var filter = new JobDraftInvoicePrintingFilter(shipment);
				control.Bind(filter);

				var uploadInvoiceButton = control.Controls.Find("UploadInvoiceButton", true).Single() as ZButton;
				AssertNotNull("UploadInvoiceButton should be available", uploadInvoiceButton);
				uploadInvoiceButton.PerformClick();

				var uri = new Uri(WebUrlLauncher.LastUrlLaunched, UriKind.Absolute);
				AssertEquals("/Goto/UploadInvoice_33c172f64d7245f89c4da047f5a18fcc", uri.AbsolutePath);
				AssertEquals(string.Empty, uri.Fragment);
				AssertContains("parentTableCode=JS", uri.Query);
				AssertContains($"parentID={shipment.PK.ToString()}", uri.Query);
			}
		}

		public void TestPostingStatusColumnExistForGrid()
		{
			using (var control = new APDraftInvoicePrintingUserControl())
			{
				AssertNotNull($"PostingStatus column should be available in grid", control.DraftInvoicesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == "PostingStatus"));
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
