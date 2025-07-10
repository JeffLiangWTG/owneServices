using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
#if WINZOR
using static System.Windows.Forms.DataGrid;
#endif

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(APDraftInvoicesGrid))]
	public class APDraftInvoicesGridTest : TestCaseWithFactory
	{
		public void TestAPDraftInvoicesGrid_ShouldSelectWholeRowOnClick()
		{
			var (form, grid) = PrepareTestData();

			using (form)
			using (grid)
			{
				AssertEquals("Should select whole row on click", true, grid.IsWholeRowSelectedOnClick);
			}
		}

		public void TestOpenDraftInvoiceInGlowPortal_ByDoubleClickTheRow()
		{
			var (form, grid) = PrepareTestData();

			using (form)
			using (grid)
			{
				grid.CurrentCell = new DataGridCell(0, 1);
				WebUrlLauncher.ClearLastUrlLaunched();
				grid.PerformDoubleClickForTest();
				AssertEquals("Should not launch any Url", string.Empty, WebUrlLauncher.LastUrlLaunched);

				grid.CurrentCell = new DataGridCell(0, 0);
				grid.PerformDoubleClickForTest();
				AssertEquals("Should select the first row", 0, grid.CurrentRowIndex);
				AssertLaunchedUrl(grid.GetFirstSelectedRow().PK);
			}
		}

		public void TestOpenDraftInvoiceInGlowPortal_ByClickingLinkedCell()
		{
			var (form, grid) = PrepareTestData();

			using (form)
			using (grid)
			{
				var transactionNumCellBounds = grid.GetCellBounds(0, 0);
				var glowLinkCellBounds = grid.GetCellBounds(0, 1);
				var e = new MouseEventArgs(MouseButtons.Left, 1, transactionNumCellBounds.X, transactionNumCellBounds.Y, 0);
				WebUrlLauncher.ClearLastUrlLaunched();
				grid.PerformMouseDownForTest(e);
				AssertEquals("Should not launch any Url", string.Empty, WebUrlLauncher.LastUrlLaunched);

				e = new MouseEventArgs(MouseButtons.Left, 1, glowLinkCellBounds.X, glowLinkCellBounds.Y, 0);
				grid.PerformMouseDownForTest(e, column: 1);
				AssertEquals("Should select the first row", 0, grid.CurrentRowIndex);
				AssertLaunchedUrl(((AccDraftInvoiceHeader)grid.ListManager.List[0]).PK);
			}
		}

		public void TestOpenDraftInvoiceInGlowPortal_ByMenuItem()
		{
			var (form, grid) = PrepareTestData();

			using (form)
			using (grid)
			{
				grid.Select(0);
				var openLinkMenuItem = grid.ContextMenu.MenuItems.FindByText("Open in Portal");
				WebUrlLauncher.ClearLastUrlLaunched();
				openLinkMenuItem.PerformClick();
				AssertLaunchedUrl(grid.GetFirstSelectedRow().PK);
			}
		}

#if !WINZOR
		public void TestCursorStyle_WhenHoveringAboveTheLinkedCell()
		{
			var (form, grid) = PrepareTestData();

			using (form)
			using (grid)
			{
				var transactionNumCellBounds = grid.GetCellBounds(0, 0);
				var glowLinkCellBounds = grid.GetCellBounds(0, 1);

				grid.MousePositionForTesting = new Point(transactionNumCellBounds.X, transactionNumCellBounds.Y);
				grid.OnMouseHoverForTest(EventArgs.Empty);
				AssertEquals("Cursor should be default", Cursors.Default, grid.Cursor);

				grid.MousePositionForTesting = new Point(glowLinkCellBounds.X, glowLinkCellBounds.Y);
				grid.OnMouseHoverForTest(EventArgs.Empty);
				AssertEquals("Cursor should be hand", Cursors.Hand, grid.Cursor);
			}
		}
#endif

		void AssertLaunchedUrl(ZGuid draftInvoicePK)
		{
			var uri = new Uri(WebUrlLauncher.LastUrlLaunched, UriKind.Absolute);

			CombineAssertions("Launched uri is correct.", () =>
			{
				AssertEquals("/Goto/OpenDraftInvoiceProxy_51D729D30A234F0281658C4120CAC554", uri.AbsolutePath);
				AssertEquals(string.Empty, uri.Fragment);
				AssertContains("entityPK=" + draftInvoicePK.ToString(), uri.Query);
			});
		}

		(ZForm form, APDraftInvoicesGridForTestOnly grid) PrepareTestData()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var shipment = TestObjectCreator.CreateShipment("S0001");

			var draftInvoice1 = TestObjectCreator.CreateDraftInvoice("INV-0001", "ADI-IREF-001", testObjectCreator.Creditor1.PK, 100M, 0M, "AUD");
			var cluster1 = TestObjectCreator.AddClusterToDraftTransaction(draftInvoice1, 100M);
			var draftInvoiceJob1 = TestObjectCreator.AddJobToTheCluster(cluster1, shipment);

			var draftInvoice2 = TestObjectCreator.CreateDraftInvoice("INV-0002", "ADI-IREF-002", testObjectCreator.Creditor1.PK, 200M, 0M, "AUD");
			var cluster2 = TestObjectCreator.AddClusterToDraftTransaction(draftInvoice2, 100M);
			var draftInvoiceJob2 = TestObjectCreator.AddJobToTheCluster(cluster2, shipment);

			Factory.Save();

			var filter = new JobDraftInvoicePrintingFilter(shipment);

			var form = new ZForm(filter);
			var grid = new APDraftInvoicesGridForTestOnly { Location = new Point(0, 0), Size = new Size(800, 800) };
			grid.RowHeaderWidth = 50;

			form.Controls.Add(grid);
			form.Show();

			var columnStyleInfo1 = new ZTextBoxColumnStyleInfo { ColumnName = "AIH_TransactionNumber" };
			var columnStyleInfo2 = new APDraftInvoiceLinkColumnStyleInfo { ColumnName = "OpenInPortal" };
			grid.ColumnStyles.Add(columnStyleInfo1);
			grid.ColumnStyles.Add(columnStyleInfo2);
			grid.SetDataBinding(filter, "Transactions", filter.Transactions.GetType().Name);

			return (form, grid);
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		class APDraftInvoicesGridForTestOnly : APDraftInvoicesGrid
		{
			public void OnMouseHoverForTest(EventArgs e)
			{
				OnMouseHover(e);
			}

			protected override Point GetMousePosition()
			{
				return MousePositionForTesting;
			}
		}
	}
}
