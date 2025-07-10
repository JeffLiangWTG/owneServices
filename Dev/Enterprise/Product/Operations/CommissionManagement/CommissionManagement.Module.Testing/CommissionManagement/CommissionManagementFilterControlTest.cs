using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.CommissionManagement.Business;
using Enterprise.CommissionManagement.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.CommissionManagement.Module
{
	class CommissionManagementFilterControlTest : TestCaseWithFactory
	{
		#region RecentCommissionsMatrix

		public void TestAddAndRemoveRecentCommissionsMatrix()
		{
			var collection = new ViewCommissionLineCollection(Factory);
			var filterStripBizObj = new CommissionManagementFilterBusinessObject();

			using (var form = new ZForm())
			using (var control = new CommissionManagementFilterControlForTesting(collection, filterStripBizObj))
			{
				form.Show();

				form.Controls.Add(control);

				var recentCommissionsMatrix = control.RecentCommissionsMatrixControlExposed;
				AssertNotNull(recentCommissionsMatrix);
				AssertCollectionContains("Matrix control should have been added to the parent form", recentCommissionsMatrix, form.Controls);

				control.Dispose();

				AssertEquals(true, recentCommissionsMatrix.IsDisposed);
				AssertCollectionNotContains("Matrix control should have been removed from the parent form", recentCommissionsMatrix, form.Controls);
			}
		}

		public void TestVerifyPositionRecentCommissionsMatrixPopUp()
		{
			using (var module = new CommissionManagementModuleForTest())
			using (var modulePopup = new EmbeddedModulePopup(module))
			using (var controlHelper = new CommissionManagementFilterControlForTesting(new ViewCommissionLineCollection(Factory), new CommissionManagementFilterBusinessObject()))
			{
				var control = module.EmbeddedControl;
				var recentCommissionsControl = modulePopup.Controls.Find("RecentCommissionsMatrixControl", true).First();
				modulePopup.Show();

				var controlToolStrip = control.Controls.Find("ToolStrip", true).First();
				AssertEquals(recentCommissionsControl.Height, controlToolStrip.Top + controlToolStrip.Height + controlToolStrip.Parent.Parent.Location.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(controlHelper.RecentCommissionsMatrixHeightAdjustmentExposed));
				AssertEquals(recentCommissionsControl.Location, ControlDpiScalingHelper.NewScaledPoint(810, 38));
			}
		}

		public void TestVerifyPositionRecentCommissionsMatrixNoPopUp()
		{
			using (var form = new ZForm())
			using (var control = new CommissionManagementFilterControlForTesting(new ViewCommissionLineCollection(Factory), new CommissionManagementFilterBusinessObject()))
			{
				form.Show();
				form.Controls.Add(control);

				AssertEquals(control.RecentCommissionsMatrixHeightAdjustmentExposed, 40);

				var controlToolStrip = control.ToolStripExposed;
				AssertEquals(control.RecentCommissionsMatrixControlExposed.Height, controlToolStrip.Top + controlToolStrip.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(control.RecentCommissionsMatrixHeightAdjustmentExposed));
#if WINZOR
				AssertEquals(control.RecentCommissionsMatrixControlExposed.Location, ControlDpiScalingHelper.NewScaledPoint(810, 41));
#else
				AssertEquals(control.RecentCommissionsMatrixControlExposed.Location, ControlDpiScalingHelper.NewScaledPoint(810, 65));
#endif
			}
		}

		#endregion

		#region Grid

		public void TestGrid_HideCompanyColumnIfOnlyShowCommissionsForCurrentLoginCompany()
		{
			var collection = new ViewCommissionLineCollection(Factory);
			var filterStripBizObj = new CommissionManagementFilterBusinessObject();

			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = new ZForm())
			using (var control = new CommissionManagementFilterControl(collection, filterStripBizObj))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(false, control.Grid.Columns.Any(x => x.ColumnName == ViewCommissionLineGrouping.Schema.CompanyPk));
			}

			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (var form = new ZForm())
			using (var control = new CommissionManagementFilterControl(collection, filterStripBizObj))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(true, control.Grid.Columns.Any(x => x.ColumnName == ViewCommissionLineGrouping.Schema.CompanyPk));
			}
		}

		public void TestExportAllColumnsToExcel()
		{
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			_ = new JobHeader.Loader(shipment).TryCreate();

			var existingInvoiceCommissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			existingInvoiceCommissionHeader.CH0_GC = GlbCompany.CurrentCompany.PK;
			existingInvoiceCommissionHeader.CH0_GroupingSourceID = invoice.PK;
			existingInvoiceCommissionHeader.CH0_GroupingSourceTableCode = invoice.TablePrefix;

			var existingInvoiceCommissionLine = existingInvoiceCommissionHeader.Lines.AddNew();
			existingInvoiceCommissionLine.CL0_GS_NKStaff = "ADL";
			existingInvoiceCommissionLine.CL0_CommissionType = CommissionTypes.Codes.FIX;
			existingInvoiceCommissionLine.CL0_EntityCommissionAmount = 100;
			Factory.Save();

			using (var form = new ZForm())
			using (var control = new CommissionManagementFilterControl(new ViewCommissionLineCollection(Factory), new CommissionManagementFilterBusinessObject()))
			using (var module = new CommissionManagementModuleForTest())
			{
				control.Grid.SetParentFilterGridModule(module);
				form.Controls.Add(control);
				form.Show();

				control.Grid.ContextMenu.MenuItems.FindByText("Export All Columns To Excel").PerformClick();
				AssertContains("There are no records to export.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region GUI states

		public void TestLoadMainSplitterGuiState_SplitContainerShouldNotSetForNegativeSplitterDistance()
		{
			var collection = new ViewCommissionLineCollection(Factory);
			var filterStripBizObj = new CommissionManagementFilterBusinessObject();

			using (var control = new CommissionManagementFilterControlForTesting(collection, filterStripBizObj))
			{
				control.LinesPreviewPaneExposed.SplitContainer.Panel1MinSize = 100;
				control.LinesPreviewPaneExposed.SplitContainer.Panel2MinSize = 50;
				control.LinesPreviewPaneExposed.SplitContainer.SplitterWidth = 10;
				control.CommissionLinesPreviewPaneGuiStateExposed.SplitterDistance = 110;
				control.LoadMainSplitterGuiStateExposed();

				AssertEquals("LinesPreviewPane.SplitContainer.SplitterDistance should be set to same value with CommissionLinesPreviewPaneGuiState.SplitterDistance when splitter between two panel.", 110, control.LinesPreviewPaneExposed.SplitContainer.SplitterDistance);

				control.LinesPreviewPaneExposed.SplitContainer.SplitterDistance = 0;
				AssertEquals("LinesPreviewPane.SplitContainer.SplitterDistance should be set to Panel1MinSize when value is less then Panel1MinSize.", 100, control.LinesPreviewPaneExposed.SplitContainer.SplitterDistance);

				control.LinesPreviewPaneExposed.SplitContainer.Panel1MinSize = 100;
				control.LinesPreviewPaneExposed.SplitContainer.Panel2MinSize = 500;
				control.LinesPreviewPaneExposed.SplitContainer.SplitterWidth = 10;
				control.CommissionLinesPreviewPaneGuiStateExposed.SplitterDistance = 110;
				control.LoadMainSplitterGuiStateExposed();

				AssertEquals("LinesPreviewPane.SplitContainer.SplitterDistance should not be set when Panel2MinSize + SplitterWidth is larger than Height.", 100, control.LinesPreviewPaneExposed.SplitContainer.SplitterDistance);
			}
		}

		#endregion

		#region Classes

		class CommissionManagementFilterControlForTesting : CommissionManagementFilterControl
		{
			public CommissionManagementFilterControlForTesting(ViewCommissionLineCollection collection, CommissionManagementFilterBusinessObject strip)
				: base(collection, strip)
			{
			}

			public RecentCommissionsMatrixControl RecentCommissionsMatrixControlExposed => RecentCommissionsMatrixControl;

			internal ZToolStrip ToolStripExposed => ToolStrip;

			internal int MainSplitterSplitPositionExposed => mainSplitter.SplitPosition;

			internal int RecentCommissionsMatrixHeightAdjustmentExposed => RecentCommissionsMatrixHeightAdjustment;

			internal CommissionLinesPreviewPaneGuiState CommissionLinesPreviewPaneGuiStateExposed => CommissionLinesPreviewPaneGuiState;

			internal void LoadMainSplitterGuiStateExposed() => LoadMainSplitterGuiState();

			internal CommissionLinesPreviewPane LinesPreviewPaneExposed => LinesPreviewPane;
		}

		class CommissionManagementModuleForTest : CommissionManagementModule
		{
			protected override BusinessObject GetFirstBizOInList()
			{
				return null;
			}
		}

		#endregion
	}
}
