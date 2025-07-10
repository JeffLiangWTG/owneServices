
namespace Enterprise.Accounting.GUI.JobInvoicing
{
	partial class CashAdvanceRequestUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.headerGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MarkAsUnpaidButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MarkAsPaidButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.headerGrid = new Enterprise.ZArchitecture.ZGrid();
			this.lineGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.lineGrid = new Enterprise.ZArchitecture.ZGrid();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.printButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.headerGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.headerGrid)).BeginInit();
			this.headerGrid.SuspendLayout();
			this.lineGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.lineGrid)).BeginInit();
			this.lineGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeaderCollection);
			// 
			// headerGroupBox
			// 
			this.headerGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("15c5f718-eaae-4f29-836f-384a0b8e4fb9", "Advance Payment Requests");
			this.headerGroupBox.Controls.Add(this.printButton);
			this.headerGroupBox.Controls.Add(this.CancelButton);
			this.headerGroupBox.Controls.Add(this.MarkAsUnpaidButton);
			this.headerGroupBox.Controls.Add(this.MarkAsPaidButton);
			this.headerGroupBox.Controls.Add(this.headerGrid);
			this.headerGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.headerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.headerGroupBox.Name = "headerGroupBox";
			this.headerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1174, 202, true);
			this.headerGroupBox.TabIndex = 0;
			this.headerGroupBox.TabStop = false;
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CancelButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3f546378-78dd-4055-804f-ef47d23ccff5", "Cancel");
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 163, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 1;
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// MarkAsUnpaidButton
			// 
			this.MarkAsUnpaidButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.MarkAsUnpaidButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("836de9b4-dce9-49bd-b835-d5bb3184aa52", "Mark as Unpaid");
			this.MarkAsUnpaidButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 163, true);
			this.MarkAsUnpaidButton.Name = "MarkAsUnpaidButton";
			this.MarkAsUnpaidButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 23, true);
			this.MarkAsUnpaidButton.TabIndex = 2;
			this.MarkAsUnpaidButton.ToolTipCaption = null;
			this.MarkAsUnpaidButton.UseVisualStyleBackColor = true;
			this.MarkAsUnpaidButton.Click += new System.EventHandler(this.MarkAsUnpaidButton_Click);
			// 
			// MarkAsPaidButton
			// 
			this.MarkAsPaidButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.MarkAsPaidButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ec401d5c-7bde-46ff-81f6-bdbfc4d5f5fe", "Mark as Paid");
			this.MarkAsPaidButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 163, true);
			this.MarkAsPaidButton.Name = "MarkAsPaidButton";
			this.MarkAsPaidButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 23, true);
			this.MarkAsPaidButton.TabIndex = 1;
			this.MarkAsPaidButton.ToolTipCaption = null;
			this.MarkAsPaidButton.UseVisualStyleBackColor = true;
			this.MarkAsPaidButton.Click += new System.EventHandler(this.MarkAsPaidButton_Click);
			// 
			// headerGrid
			// 
			this.headerGrid.AllowNavigation = false;
			this.headerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.headerGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).CAH_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).OrganizationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).CAH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).CAH_OSAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).CAH_OSPaidAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).CAH_OSOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).OrganizationName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).CAH_RequestReferenceNumber)));
			this.headerGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("267033db-837b-4375-b513-b0a735cb319b", "Ledger");
			zTextBoxColumnStyleInfo1.ColumnName = "CAH_Ledger";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e4169cff-62e4-416d-9c5c-82cc9b4cce81", "Organization");
			zTextBoxColumnStyleInfo2.ColumnName = "OrganizationCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6a12741b-f63b-417a-9e62-76cfca0d0a62", "Currency");
			zTextBoxColumnStyleInfo3.ColumnName = "CAH_RX_NKTransactionCurrency";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c0a5c353-93f1-4e9e-9f99-45f91196089e", "OS Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "CAH_OSAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9746a918-cf6a-442b-be20-c59365c78c0b", "OS Paid Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "CAH_OSPaidAmount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("12819ae4-ed49-4b91-9d46-802638432efa", "OS Outstanding Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "CAH_OSOutstandingAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6b363540-211c-475d-9ba2-8660034042ec", "Status");
			zTextBoxColumnStyleInfo4.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("668d5868-2fd4-497a-ac29-a0be82e6c709", "Organization Name");
			zTextBoxColumnStyleInfo5.ColumnName = "OrganizationName";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DEE7651A-4E1B-45D0-8F05-F784213C1511", "Advance Payment Request ID");
			zTextBoxColumnStyleInfo6.ColumnName = "CAH_RequestReferenceNumber";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.headerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.headerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.headerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.headerGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.headerGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.headerGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.headerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.headerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.headerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.headerGrid.GridId = "fcbf0474-364e-4f4b-a013-6d999c02c977";
			this.headerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.headerGrid.LayoutKey = "headerGrid";
			this.headerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.headerGrid.Name = "headerGrid";
			this.headerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1168, 141, true);
			this.headerGrid.TabIndex = 0;
			// 
			// lineGroupBox
			// 
			this.lineGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9834d35f-932b-4df1-800d-78418899586d", "Detail");
			this.lineGroupBox.Controls.Add(this.lineGrid);
			this.lineGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lineGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.lineGroupBox.Name = "lineGroupBox";
			this.lineGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1174, 285, true);
			this.lineGroupBox.TabIndex = 0;
			this.lineGroupBox.TabStop = false;
			// 
			// lineGrid
			// 
			this.lineGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.lineGrid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).RelatedChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).OrganizationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).CAL_OSAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).CAL_OSPaidAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).CAL_OSOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).CAL_LocalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).CAL_LocalPaidAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).OrganizationName)));
			this.lineGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8c8cf133-4aca-4b3a-84b9-5bd9eb2759ae", "Charge Code");
			zTextBoxColumnStyleInfo7.ColumnName = "RelatedChargeCode";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a4b2f036-378b-4879-be81-3d09e9451c8d", "Organization");
			zTextBoxColumnStyleInfo8.ColumnName = "OrganizationCode";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3cc2ead1-9091-4a23-b5d6-dd910cf282c6", "Currency");
			zTextBoxColumnStyleInfo9.ColumnName = "Currency";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b7c5abc4-fe43-4224-9464-ab2bfd790745", "OS Amount");
			zCalcEditColumnStyleInfo4.ColumnName = "CAL_OSAmount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("55cfe18d-9c88-42bd-9254-1bba638c3594", "OS Paid Amount");
			zCalcEditColumnStyleInfo5.ColumnName = "CAL_OSPaidAmount";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c4ef2c64-23a8-4cd6-aaa3-24ee912cb257", "OS Outstanding Amount");
			zCalcEditColumnStyleInfo6.ColumnName = "CAL_OSOutstandingAmount";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0ef74534-262c-4925-9b23-aec1695ffe24", "Status");
			zTextBoxColumnStyleInfo10.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d7587f1b-3826-4c27-85ff-724a28764f52", "Local Amount");
			zCalcEditColumnStyleInfo7.ColumnName = "CAL_LocalAmount";
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("59e3de98-d14e-446b-86f3-cc3139e98945", "Local Paid Amount");
			zCalcEditColumnStyleInfo8.ColumnName = "CAL_LocalPaidAmount";
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5346a1d1-01c9-484a-bcbd-04e1c51f30e4", "Organization Name");
			zTextBoxColumnStyleInfo11.ColumnName = "OrganizationName";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.lineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.lineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.lineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.lineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.lineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.lineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.lineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.lineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.lineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.lineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.lineGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lineGrid.GridId = "071266ae-72c8-4ac4-8fd3-84ac60ab9e50";
			this.lineGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.lineGrid.LayoutKey = "lineGrid";
			this.lineGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.lineGrid.Name = "lineGrid";
			this.lineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1168, 266, true);
			this.lineGrid.TabIndex = 0;
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.AutoScroll = true;
			this.mainSplitContainer.Panel1.Controls.Add(this.headerGroupBox);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.AutoScroll = true;
			this.mainSplitContainer.Panel2.Controls.Add(this.lineGroupBox);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1174, 491, true);
			this.mainSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(202);
			this.mainSplitContainer.TabIndex = 1;
			// 
			// printButton
			// 
			this.printButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.printButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1084, 163, true);
			this.printButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ebccecb1-6b5c-4478-a7ca-9b77bdf82d41", "Print");
			this.printButton.Name = "printButton";
			this.printButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.printButton.TabIndex = 3;
			this.printButton.ToolTipCaption = null;
			this.printButton.UseVisualStyleBackColor = true;
			this.printButton.Click += new System.EventHandler(PrintRequests);
			// 
			// CashAdvanceRequestUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.mainSplitContainer);
			this.Name = "CashAdvanceRequestUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1174, 491, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.headerGroupBox.ResumeLayout(false);
			this.headerGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.headerGrid)).EndInit();
			this.headerGrid.ResumeLayout(false);
			this.headerGrid.PerformLayout();
			this.lineGroupBox.ResumeLayout(false);
			this.lineGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.lineGrid)).EndInit();
			this.lineGrid.ResumeLayout(false);
			this.lineGrid.PerformLayout();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGroupBox lineGroupBox;
		private ZArchitecture.GUI.ZGroupBox headerGroupBox;
		private ZArchitecture.ZGrid headerGrid;
		private ZArchitecture.ZGrid lineGrid;
		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
		private ZArchitecture.GUI.ZButton CancelButton;
		private ZArchitecture.GUI.ZButton MarkAsUnpaidButton;
		private ZArchitecture.GUI.ZButton MarkAsPaidButton;
		private ZArchitecture.GUI.ZButton printButton;
	}
}
