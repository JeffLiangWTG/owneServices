using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Periodic_Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class PeriodicInvoiceControl
	{


		#region Component Designer generated code

		private AccountingOnFormFilterControl MiscInvoicesFilterControl;
		private PeriodicInvoiceMiscInvoicesFilterBusinessObject MiscInvoicesFilterBuisnessObject;
		private AccountingOnFormFilterControl JobsFilterControl;
		private PeriodicInvoiceBaseJobFilterBusinessObject JobsFilterBuisnessObject;
		public ZTemplateTabControl TabControl;
		public ZTabPage JobsTabPage;
		public ZTabPage MiscInvoicesTabPage;
		protected ZGroupBox TotalsGroupBox;
		protected ZGroupBox ExtraTaxGroupBox;
		protected ZCalcFindBox OSTaxAmountCalcFindBox;
		protected ZCalcFindBox OSExTaxAmountCalcFindBox;
		protected ZPanel JobsFilterPanel;
		protected ZPanel JobsGridPanel;
		protected ZPanel JobsNotificationPanel;
		protected ZPanel MiscInvoicesGridPanel;
		protected ZPanel MiscInvoicesNotificationPanel;
		protected ZArchitecture.ZLabel MiscInvoicesInfoLabel;
		protected ZPanel MiscInvoicesFilterPanel;
		protected ZCalcFindBox OSTotalAmountCalcFindBox;
		protected ZCalcFindBox OSExtraTaxAmount;
		protected ZCalcFindBox LocalExtraTaxAmount;
		protected ZCalcFindBox LocalTotalAmountCalcFindBox;
		protected ZCalcFindBox LocalTaxAmountCalcFindBox;
		protected ZCalcFindBox LocalExTaxAmountCalcFindBox;
		protected ZArchitecture.ZGrid JobsGrid;
		protected ZArchitecture.ZGrid MiscInvoicesGrid;
		protected ZArchitecture.ZLabel JobsInfoLabel;

		private void InitializeComponent()
		{
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TabControl = new ZTemplateTabControl();
			this.JobsTabPage = new ZTabPage();
			this.JobsFilterPanel = new ZPanel();
			this.JobsGridPanel = new ZPanel();
			this.JobsGrid = new ZArchitecture.ZGrid();
			this.JobsNotificationPanel = new ZPanel();
			this.JobsInfoLabel = new ZArchitecture.ZLabel();
			this.MiscInvoicesTabPage = new ZTabPage();
			this.MiscInvoicesGridPanel = new ZPanel();
			this.MiscInvoicesGrid = new ZArchitecture.ZGrid();
			this.MiscInvoicesNotificationPanel = new ZPanel();
			this.MiscInvoicesInfoLabel = new ZArchitecture.ZLabel();
			this.MiscInvoicesFilterPanel = new ZPanel();
			this.TotalsGroupBox = new ZGroupBox();
			this.LocalTotalAmountCalcFindBox = new ZCalcFindBox();
			this.LocalTaxAmountCalcFindBox = new ZCalcFindBox();
			this.LocalExTaxAmountCalcFindBox = new ZCalcFindBox();
			this.OSTotalAmountCalcFindBox = new ZCalcFindBox();
			this.OSTaxAmountCalcFindBox = new ZCalcFindBox();
			this.OSExTaxAmountCalcFindBox = new ZCalcFindBox();
			this.ExtraTaxGroupBox = new ZGroupBox();
			this.LocalExtraTaxAmount = new ZCalcFindBox();
			this.OSExtraTaxAmount = new ZCalcFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TabControl.SuspendLayout();
			this.JobsTabPage.SuspendLayout();
			this.JobsFilterPanel.SuspendLayout();
			this.JobsGridPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobsGrid)).BeginInit();
			this.JobsNotificationPanel.SuspendLayout();
			this.MiscInvoicesTabPage.SuspendLayout();
			this.MiscInvoicesGridPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MiscInvoicesGrid)).BeginInit();
			this.MiscInvoicesNotificationPanel.SuspendLayout();
			this.TotalsGroupBox.SuspendLayout();
			this.ExtraTaxGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(PeriodicInvoiceBase);
			// 
			// TabControl
			// 
			this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControl.Controls.Add(this.JobsTabPage);
			this.TabControl.Controls.Add(this.MiscInvoicesTabPage);
			this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 236, true);
			this.TabControl.TabIndex = 0;
			// 
			// JobsTabPage
			// 
			this.JobsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.JobsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|639612f7-4f16-45ff-8654-ea8ff0e6ee75", "Jobs");
			this.JobsTabPage.Controls.Add(this.JobsFilterPanel);
			this.JobsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.JobsTabPage.Name = "JobsTabPage";
			this.JobsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.JobsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 209, true);
			this.JobsTabPage.TabIndex = 0;
			// 
			// JobsFilterPanel
			// 
			this.JobsFilterPanel.Controls.Add(this.JobsGridPanel);
			this.JobsFilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobsFilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.JobsFilterPanel.Name = "JobsFilterPanel";
			this.JobsFilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 203, true);
			this.JobsFilterPanel.TabIndex = 0;
			// 
			// JobsGridPanel
			// 
			this.JobsGridPanel.Controls.Add(this.JobsGrid);
			this.JobsGridPanel.Controls.Add(this.JobsNotificationPanel);
			this.JobsGridPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.JobsGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 42, true);
			this.JobsGridPanel.Name = "JobsGridPanel";
			this.JobsGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 161, true);
			this.JobsGridPanel.TabIndex = 10;
			// 
			// JobsGrid
			// 
			this.JobsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.JobsGrid, "Jobs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).IncludeInThePeriodicInvoice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).JH_JobNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).JH_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).JH_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).JH_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).JH_A_JOP)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).LocalChargesPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).JH_GS_NKRepOps)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).JH_GS_NKRepSales)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).AgentCollectPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).RevenueRecognitionDates)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).JH_MasterBillNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).JH_HouseBillNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).JH_ConsolNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).JH_OSAmountForPeriodicBilling)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).JH_OSTaxAmountForPeriodicBilling)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).JH_LocalAmountForPeriodicBilling)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).JH_LocalTaxAmountForPeriodicBilling)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).OSDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).JH_OSExtraTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).LocalDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).JH_LocalExtraTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).ServiceDirection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).SecondaryLayoutWhenPrintedInPeriodicInvoice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).LayoutWhenPrintedInPeriodicInvoice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).ServiceLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).JH_GB_TaxBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PeriodicInvoiceSelectableJob)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).Jobs)).SyncRoot)).ChargeTaxBranches)));
			this.JobsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|3f118fcf-5c59-4cd1-8179-0aa0a29ae703", "Include");
			zCheckBoxColumnStyleInfo1.ColumnName = "IncludeInThePeriodicInvoice";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8f36945c-ec1f-4637-bd61-3215f21dfade", "Job Number");
			zTextBoxColumnStyleInfo1.ColumnName = "JH_JobNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4BA5CF0E-139B-4893-AF8A-9D6A5AE89294", "Order Reference");
			zTextBoxColumnStyleInfo18.ColumnName = "JH_JS_OrderReferences";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("95174B85-FB33-400B-983B-DBE4154D13ED", "Additional Reference");
			zTextBoxColumnStyleInfo19.ColumnName = "AdditionalReferenceAsString";
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("B32B4CA0-2870-4BC4-87DB-5A74BA487C69", "Vessel");
			zTextBoxColumnStyleInfo20.ColumnName = "JH_JS_JK_Vessel";
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo21.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8E64AF3E-480F-425C-96E5-E918010EF94F", "Voyage");
			zTextBoxColumnStyleInfo21.ColumnName = "JH_JS_JK_VoyageFlight";
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e96b27ee-7466-4000-a718-779c54b40c1d", "Job Type");
			zTextBoxColumnStyleInfo2.ColumnName = "JobType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8fac921d-d248-4cb9-98f5-a15b4fa31caa", "Status");
			zTextBoxColumnStyleInfo3.ColumnName = "JH_Status";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a10f1d7c-7856-4ae2-a0b1-73005c5f8d7e", "Branch");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JH_GB";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0ba62cd9-c817-42cb-894f-b2cf77c8a7d7", "Dept.");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "JH_GE";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2b7d8d77-c454-46f9-9f86-bd11ca512064", "JOP Date");
			zDateEditColumnStyleInfo1.ColumnName = "JH_A_JOP";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|b6bf8381-986f-493c-b2c6-ca3e40037cc7", "Local Client");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "LocalChargesPK";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5d277741-106d-4bf2-8f64-967b36e43c8a", "Ops.Rep");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JH_GS_NKRepOps";
			zCodeFindBoxColumnStyleInfo1.IsVisible = false;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("939e25c0-29b2-4cb7-bd27-295d8b94d114", "Sales Rep.");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "JH_GS_NKRepSales";
			zCodeFindBoxColumnStyleInfo2.IsVisible = false;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|6c02bde2-85e7-4c9d-b8af-f178d016f49a", "Overseas Agent");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "AgentCollectPK";
			zGuidFindBoxColumnStyleInfo4.IsVisible = false;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(89);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|7f1bc43c-ebcf-451f-b655-6495bed7af5e", "Job Revenue Recognition Date");
			zTextBoxColumnStyleInfo4.ColumnName = "RevenueRecognitionDates";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(159);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|728656f0-b12e-4ab4-bf85-66e3eab6e27c", "Master Bill Number");
			zTextBoxColumnStyleInfo5.ColumnName = "JH_MasterBillNo";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(99);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|821620ed-ac87-40d7-a1e8-2e1b95f7784f", "House Bill Number");
			zTextBoxColumnStyleInfo6.ColumnName = "JH_HouseBillNo";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|bed134a8-e53d-4166-aed1-3efb631cadf5", "Consol Number");
			zTextBoxColumnStyleInfo7.ColumnName = "JH_ConsolNo";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(83);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|bdbfe7e1-0493-48ff-afb8-50dfb2127b81", "Currency");
			zTextBoxColumnStyleInfo8.ColumnName = "Currency";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "OSDecimals";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|0d5d7e5a-42b9-48f8-9954-059369ce8c08", "OS Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "JH_OSAmountForPeriodicBilling";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = "OSDecimals";
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|593bc6de-bd05-49a4-a5e9-d553e395fd82", "OS Tax Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "JH_OSTaxAmountForPeriodicBilling";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(86);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = "LocalDecimals";
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|4dfc6de8-d0fa-4c60-8c9c-dbc44dcee3a7", "Local Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "JH_LocalAmountForPeriodicBilling";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = "LocalDecimals";
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|45f4a555-9f12-4780-b05c-15aa37c4af49", "Local Tax Amount");
			zCalcEditColumnStyleInfo4.ColumnName = "JH_LocalTaxAmountForPeriodicBilling";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = "OSDecimals";
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e2c10b47-4b5e-4466-9d14-f2438292b6ec", "Extra Tax Amount.");
			zCalcEditColumnStyleInfo5.ColumnName = "JH_OSExtraTaxAmount";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = "LocalDecimals";
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a7093045-92cc-42e5-a84c-9edc07a85fd0", "Local Extra Tax Amount.");
			zCalcEditColumnStyleInfo6.ColumnName = "JH_LocalExtraTaxAmount";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f5afa409-098e-4c3c-9403-836e4b89f65c", "Transport");
			zTextBoxColumnStyleInfo9.ColumnName = "TransportMode";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("303c560a-977a-417b-9dfc-f58c20f8b98c", "Direction");
			zTextBoxColumnStyleInfo10.ColumnName = "ServiceDirection";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ab13c9ed-1b46-4e59-8a04-c5e27e6d1340", "Sec. Layout");
			zTextBoxColumnStyleInfo11.ColumnName = "SecondaryLayoutWhenPrintedInPeriodicInvoice";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("34e7119c-9b93-4740-af5f-5f54120653c3", "Layout");
			zTextBoxColumnStyleInfo12.ColumnName = "LayoutWhenPrintedInPeriodicInvoice";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("B2810402-E1C7-448A-A802-D3FA2D76789F", "Service Level");
			zTextBoxColumnStyleInfo17.ColumnName = "ServiceLevel";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("485a35ef-7f3c-4694-8f8a-84935edf45de", "Job Tax Branch");
			zGuidFindBoxColumnStyleInfo5.ColumnName = "JH_GB_TaxBranch";
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo22.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("74076fab-f653-4dc6-ad22-2ac26382e9bf", "Tax Branch");
			zTextBoxColumnStyleInfo22.ColumnName = "ChargeTaxBranches";
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.JobsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.JobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.JobsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.JobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.JobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.JobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.JobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.JobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.JobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.JobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.JobsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.JobsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.JobsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.JobsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.JobsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.JobsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.JobsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.JobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.JobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.JobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.JobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.JobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.JobsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.JobsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.JobsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.JobsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.JobsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.JobsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.JobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.JobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.JobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.JobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.JobsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.JobsGrid.CopySelectedRowsAllowed = true;
			this.JobsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobsGrid.GridId = "C26900C8-EAD7-44B8-895D-0712DF0A7794";
			this.JobsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobsGrid.LayoutKey = "BatchInvoiceLinesGrid";
			this.JobsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.JobsGrid.Name = "JobsGrid";
			this.JobsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.JobsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 129, true);
			this.JobsGrid.TabIndex = 1;
			// 
			// JobsNotificationPanel
			// 
			this.JobsNotificationPanel.Controls.Add(this.JobsInfoLabel);
			this.JobsNotificationPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.JobsNotificationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobsNotificationPanel.Name = "JobsNotificationPanel";
			this.JobsNotificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 32, true);
			this.JobsNotificationPanel.TabIndex = 0;
			// 
			// JobsInfoLabel
			// 
			this.JobsInfoLabel.AutoSize = true;
			this.JobsInfoLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|8f7f99ff-ff2d-4051-9737-3804e120e5dc", "Please enter the filter criteria.");
			this.JobsInfoLabel.ForeColor = System.Drawing.SystemColors.InfoText;
			this.JobsInfoLabel.IsFontBold = true;
			this.JobsInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.JobsInfoLabel.Name = "JobsInfoLabel";
			this.JobsInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 13, true);
			this.JobsInfoLabel.TabIndex = 0;
			// 
			// MiscInvoicesTabPage
			// 
			this.MiscInvoicesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.MiscInvoicesTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|dae6e1bd-83c0-4573-b6bf-3fcd8a8cb265", "Misc Invoices");
			this.MiscInvoicesTabPage.Controls.Add(this.MiscInvoicesGridPanel);
			this.MiscInvoicesTabPage.Controls.Add(this.MiscInvoicesFilterPanel);
			this.MiscInvoicesTabPage.LicenceCheckpoint = null;
			this.MiscInvoicesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MiscInvoicesTabPage.Name = "MiscInvoicesTabPage";
			this.MiscInvoicesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MiscInvoicesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 209, true);
			this.MiscInvoicesTabPage.TabIndex = 1;
			// 
			// MiscInvoicesGridPanel
			// 
			this.MiscInvoicesGridPanel.Controls.Add(this.MiscInvoicesGrid);
			this.MiscInvoicesGridPanel.Controls.Add(this.MiscInvoicesNotificationPanel);
			this.MiscInvoicesGridPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.MiscInvoicesGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 30, true);
			this.MiscInvoicesGridPanel.Name = "MiscInvoicesGridPanel";
			this.MiscInvoicesGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 176, true);
			this.MiscInvoicesGridPanel.TabIndex = 11;
			// 
			// MiscInvoicesGrid
			// 
			this.MiscInvoicesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MiscInvoicesGrid, "MiscInvoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).MiscInvoices)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((InvoicingBase)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).MiscInvoices)).SyncRoot)).IncludeInThePeriodicInvoice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoicingBase)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).MiscInvoices)).SyncRoot)).AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoicingBase)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).MiscInvoices)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoicingBase)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).MiscInvoices)).SyncRoot)).AH_ConsolidatedInvoiceRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoicingBase)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).MiscInvoices)).SyncRoot)).AH_TransactionCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((InvoicingBase)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).MiscInvoices)).SyncRoot)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((InvoicingBase)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).MiscInvoices)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((InvoicingBase)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).MiscInvoices)).SyncRoot)).AH_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoicingBase)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).MiscInvoices)).SyncRoot)).AH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((InvoicingBase)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).MiscInvoices)).SyncRoot)).AH_OSTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((InvoicingBase)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).MiscInvoices)).SyncRoot)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((InvoicingBase)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).MiscInvoices)).SyncRoot)).AH_InvoiceAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((InvoicingBase)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).MiscInvoices)).SyncRoot)).AH_GSTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((InvoicingBase)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).MiscInvoices)).SyncRoot)).AH_OutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((InvoicingBase)(((System.Collections.IList)(((PeriodicInvoiceBase)(null)).MiscInvoices)).SyncRoot)).AH_ExchangeRate)));
			this.MiscInvoicesGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|7cf6930f-ecce-4248-86b5-93b441885431", "Include");
			zCheckBoxColumnStyleInfo2.ColumnName = "IncludeInThePeriodicInvoice";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.ColumnName = "AH_TransactionType";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.ColumnName = "AH_TransactionNum";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo15.ColumnName = "AH_ConsolidatedInvoiceRef";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo16.ColumnName = "AH_TransactionCategory";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo4.ColumnName = "AH_DueDate";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo3.ColumnName = "AH_RX_NKTransactionCurrency";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "AH_OSTotal";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AH_OH";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "AH_InvoiceAmount";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "AH_GSTAmount";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.ColumnName = "AH_OutstandingAmount";
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.ColumnName = "AH_ExchangeRate";
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.MiscInvoicesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.MiscInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.MiscInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.MiscInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.MiscInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.MiscInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MiscInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.MiscInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.MiscInvoicesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.MiscInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.MiscInvoicesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.MiscInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.MiscInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.MiscInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.MiscInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.MiscInvoicesGrid.CopySelectedRowsAllowed = true;
			this.MiscInvoicesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MiscInvoicesGrid.GridId = "ac4c4c7b-4733-4a47-a36d-74fe44a80fb5";
			this.MiscInvoicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MiscInvoicesGrid.LayoutKey = "BatchInvoiceLinesGrid";
			this.MiscInvoicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.MiscInvoicesGrid.Name = "MiscInvoicesGrid";
			this.MiscInvoicesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.MiscInvoicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 144, true);
			this.MiscInvoicesGrid.TabIndex = 1;
			// 
			// MiscInvoicesNotificationPanel
			// 
			this.MiscInvoicesNotificationPanel.BackColor = System.Drawing.SystemColors.Control;
			this.MiscInvoicesNotificationPanel.Controls.Add(this.MiscInvoicesInfoLabel);
			this.MiscInvoicesNotificationPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MiscInvoicesNotificationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MiscInvoicesNotificationPanel.Name = "MiscInvoicesNotificationPanel";
			this.MiscInvoicesNotificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 32, true);
			this.MiscInvoicesNotificationPanel.TabIndex = 0;
			// 
			// MiscInvoicesInfoLabel
			// 
			this.MiscInvoicesInfoLabel.AutoSize = true;
			this.MiscInvoicesInfoLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|c56258b9-72a8-452c-8009-b977d3fc4dcd", "Please enter the filter criteria.");
			this.MiscInvoicesInfoLabel.ForeColor = System.Drawing.SystemColors.InfoText;
			this.MiscInvoicesInfoLabel.IsFontBold = true;
			this.MiscInvoicesInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.MiscInvoicesInfoLabel.Name = "MiscInvoicesInfoLabel";
			this.MiscInvoicesInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.MiscInvoicesInfoLabel.TabIndex = 0;
			// 
			// MiscInvoicesFilterPanel
			// 
			this.MiscInvoicesFilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MiscInvoicesFilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MiscInvoicesFilterPanel.Name = "MiscInvoicesFilterPanel";
			this.MiscInvoicesFilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 203, true);
			this.MiscInvoicesFilterPanel.TabIndex = 0;
			// 
			// TotalsGroupBox
			// 
			this.TotalsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|bfd3878c-f8c7-42cd-ba86-4968bb677dfb", "Totals");
			this.TotalsGroupBox.Controls.Add(this.LocalTotalAmountCalcFindBox);
			this.TotalsGroupBox.Controls.Add(this.LocalTaxAmountCalcFindBox);
			this.TotalsGroupBox.Controls.Add(this.LocalExTaxAmountCalcFindBox);
			this.TotalsGroupBox.Controls.Add(this.OSTotalAmountCalcFindBox);
			this.TotalsGroupBox.Controls.Add(this.OSTaxAmountCalcFindBox);
			this.TotalsGroupBox.Controls.Add(this.OSExTaxAmountCalcFindBox);
			this.TotalsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.TotalsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 236, true);
			this.TotalsGroupBox.Name = "TotalsGroupBox";
			this.TotalsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 106, true);
			this.TotalsGroupBox.TabIndex = 1;
			this.TotalsGroupBox.TabStop = false;
			// 
			// LocalTotalAmountCalcFindBox
			// 
			this.LocalTotalAmountCalcFindBox.AllowDrop = true;
			this.LocalTotalAmountCalcFindBox.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.LocalTotalAmountCalcFindBox.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.LocalTotalAmountCalcFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LocalTotalAmountCalcFindBox.BindToAmount = "LocalTotalAmount";
			this.LocalTotalAmountCalcFindBox.BindToDecimalPlaces = "LocalCurrency_Decimals";
			this.LocalTotalAmountCalcFindBox.BindToUnit = "CurrencyReadonlyLocalNK";
			this.LocalTotalAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LocalTotalAmountCalcFindBox, false);
			this.LocalTotalAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(688, 70, true);
			this.LocalTotalAmountCalcFindBox.Name = "LocalTotalAmountCalcFindBox";
			this.LocalTotalAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.LocalTotalAmountCalcFindBox.TabIndex = 5;
			// 
			// LocalTaxAmountCalcFindBox
			// 
			this.LocalTaxAmountCalcFindBox.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.LocalTaxAmountCalcFindBox.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.LocalTaxAmountCalcFindBox.AllowDrop = true;
			this.LocalTaxAmountCalcFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LocalTaxAmountCalcFindBox.BindToAmount = "LocalTaxAmount";
			this.LocalTaxAmountCalcFindBox.BindToDecimalPlaces = "LocalCurrency_Decimals";
			this.LocalTaxAmountCalcFindBox.BindToUnit = "CurrencyReadonlyLocalNK";
			this.LocalTaxAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LocalTaxAmountCalcFindBox, false);
			this.LocalTaxAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(688, 44, true);
			this.LocalTaxAmountCalcFindBox.Name = "LocalTaxAmountCalcFindBox";
			this.LocalTaxAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.LocalTaxAmountCalcFindBox.TabIndex = 4;
			// 
			// LocalExTaxAmountCalcFindBox
			// 
			this.LocalExTaxAmountCalcFindBox.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.LocalExTaxAmountCalcFindBox.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.LocalExTaxAmountCalcFindBox.AllowDrop = true;
			this.LocalExTaxAmountCalcFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LocalExTaxAmountCalcFindBox.BindToAmount = "LocalExTaxAmount";
			this.LocalExTaxAmountCalcFindBox.BindToDecimalPlaces = "LocalCurrency_Decimals";
			this.LocalExTaxAmountCalcFindBox.BindToUnit = "CurrencyReadonlyLocalNK";
			this.LocalExTaxAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LocalExTaxAmountCalcFindBox, false);
			this.LocalExTaxAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(688, 18, true);
			this.LocalExTaxAmountCalcFindBox.Name = "LocalExTaxAmountCalcFindBox";
			this.LocalExTaxAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.LocalExTaxAmountCalcFindBox.TabIndex = 3;
			// 
			// OSTotalAmountCalcFindBox
			//
			this.OSTotalAmountCalcFindBox.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.OSTotalAmountCalcFindBox.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.OSTotalAmountCalcFindBox.AllowDrop = true;
			this.OSTotalAmountCalcFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OSTotalAmountCalcFindBox.BindToAmount = "OSTotalAmount";
			this.OSTotalAmountCalcFindBox.BindToDecimalPlaces = "CurrencyNK_Decimals";
			this.OSTotalAmountCalcFindBox.BindToUnit = "CurrencyReadonlyNK";
			this.OSTotalAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoiceControl|e6d735c1-49db-44ac-88c4-a27dee75b865", "Invoice Total");
			this.OSTotalAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OSTotalAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(523, 70, true);
			this.OSTotalAmountCalcFindBox.Name = "OSTotalAmountCalcFindBox";
			this.OSTotalAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.OSTotalAmountCalcFindBox.TabIndex = 2;
			// 
			// OSTaxAmountCalcFindBox
			// 
			this.OSTaxAmountCalcFindBox.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.OSTaxAmountCalcFindBox.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.OSTaxAmountCalcFindBox.AllowDrop = true;
			this.OSTaxAmountCalcFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OSTaxAmountCalcFindBox.BindToAmount = "OSTaxAmount";
			this.OSTaxAmountCalcFindBox.BindToDecimalPlaces = "CurrencyNK_Decimals";
			this.OSTaxAmountCalcFindBox.BindToUnit = "CurrencyReadonlyNK";
			this.OSTaxAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OSTaxAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(523, 44, true);
			this.OSTaxAmountCalcFindBox.Name = "OSTaxAmountCalcFindBox";
			this.OSTaxAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.OSTaxAmountCalcFindBox.TabIndex = 1;
			// 
			// OSExTaxAmountCalcFindBox
			// 
			this.OSExTaxAmountCalcFindBox.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.OSExTaxAmountCalcFindBox.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.OSExTaxAmountCalcFindBox.AllowDrop = true;
			this.OSExTaxAmountCalcFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OSExTaxAmountCalcFindBox.BindToAmount = "OSExTaxAmount";
			this.OSExTaxAmountCalcFindBox.BindToDecimalPlaces = "CurrencyNK_Decimals";
			this.OSExTaxAmountCalcFindBox.BindToUnit = "CurrencyReadonlyNK";
			this.OSExTaxAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OSExTaxAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(522, 18, true);
			this.OSExTaxAmountCalcFindBox.Name = "OSExTaxAmountCalcFindBox";
			this.OSExTaxAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.OSExTaxAmountCalcFindBox.TabIndex = 0;
			// 
			// ExtraTaxGroupBox
			// 
			this.ExtraTaxGroupBox.Controls.Add(this.LocalExtraTaxAmount);
			this.ExtraTaxGroupBox.Controls.Add(this.OSExtraTaxAmount);
			this.ExtraTaxGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ExtraTaxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 342, true);
			this.ExtraTaxGroupBox.Name = "ExtraTaxGroupBox";
			this.ExtraTaxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 55, true);
			this.ExtraTaxGroupBox.TabIndex = 1;
			this.ExtraTaxGroupBox.TabStop = false;
			// 
			// LocalExtraTaxAmount
			// 
			this.LocalExtraTaxAmount.AllowDrop = true;
			this.LocalExtraTaxAmount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LocalExtraTaxAmount.BindToAmount = "LocalExtraTaxAmount";
			this.LocalExtraTaxAmount.BindToDecimalPlaces = "LocalCurrency_Decimals";
			this.LocalExtraTaxAmount.BindToUnit = "CurrencyReadonlyLocalNK";
			this.LocalExtraTaxAmount.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LocalExtraTaxAmount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(688, 18, true);
			this.LocalExtraTaxAmount.Name = "LocalExtraTaxAmount";
			this.LocalExtraTaxAmount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.LocalExtraTaxAmount.TabIndex = 2;
			// 
			// OSExtraTaxAmount
			// 
			this.OSExtraTaxAmount.AllowDrop = true;
			this.OSExtraTaxAmount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OSExtraTaxAmount.BindToAmount = "OSExtraTaxAmount";
			this.OSExtraTaxAmount.BindToDecimalPlaces = "CurrencyNK_Decimals";
			this.OSExtraTaxAmount.BindToUnit = "CurrencyReadonlyNK";
			this.OSExtraTaxAmount.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OSExtraTaxAmount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(523, 18, true);
			this.OSExtraTaxAmount.Name = "OSExtraTaxAmount";
			this.OSExtraTaxAmount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.OSExtraTaxAmount.TabIndex = 2;
			// 
			// PeriodicInvoiceControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TabControl);
			this.Controls.Add(this.TotalsGroupBox);
			this.Controls.Add(this.ExtraTaxGroupBox);
			this.Name = "PeriodicInvoiceControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 397, true);
			this.BackColorChanged += new EventHandler(this.PeriodicInvoiceControl_BackColorChanged);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.JobsTabPage.ResumeLayout(false);
			this.JobsFilterPanel.ResumeLayout(false);
			this.JobsGridPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.JobsGrid)).EndInit();
			this.JobsNotificationPanel.ResumeLayout(false);
			this.JobsNotificationPanel.PerformLayout();
			this.MiscInvoicesTabPage.ResumeLayout(false);
			this.MiscInvoicesGridPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MiscInvoicesGrid)).EndInit();
			this.MiscInvoicesNotificationPanel.ResumeLayout(false);
			this.MiscInvoicesNotificationPanel.PerformLayout();
			this.TotalsGroupBox.ResumeLayout(false);
			this.ExtraTaxGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

	}
}