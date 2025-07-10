namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class BulkAddDiscountForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.NewDiscountsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ValidateAndSaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NewDiscountsGrid)).BeginInit();
			this.NewDiscountsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 205, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.BulkAddDiscountBizO);
			// 
			// NewDiscountsGrid
			// 
			this.NewDiscountsGrid.AllowNavigation = false;
			this.NewDiscountsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.NewDiscountsGrid, "NewDiscountCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscountBizO)(null)).NewDiscountCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscountBizO)(null)).NewDiscountCollection)).SyncRoot)).SystemCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscountBizO)(null)).NewDiscountCollection)).SyncRoot)).SubCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscountBizO)(null)).NewDiscountCollection)).SyncRoot)).DiscountType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscountBizO)(null)).NewDiscountCollection)).SyncRoot)).ModuleCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscountBizO)(null)).NewDiscountCollection)).SyncRoot)).BreakUnits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscountBizO)(null)).NewDiscountCollection)).SyncRoot)).BreakAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscountBizO)(null)).NewDiscountCollection)).SyncRoot)).Units)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscountBizO)(null)).NewDiscountCollection)).SyncRoot)).Discount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscountBizO)(null)).NewDiscountCollection)).SyncRoot)).StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscountBizO)(null)).NewDiscountCollection)).SyncRoot)).EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscountBizO)(null)).NewDiscountCollection)).SyncRoot)).Duration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscountBizO)(null)).NewDiscountCollection)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.BulkAddDiscountBizO)(null)).NewDiscountCollection)).SyncRoot)).Comment)));
			this.NewDiscountsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = "System";
			zDropEditColumnStyleInfo1.ColumnName = "SystemCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.Caption = "Category";
			zDropEditColumnStyleInfo2.ColumnName = "SubCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.Caption = "Type";
			zDropEditColumnStyleInfo3.ColumnName = "DiscountType";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.Caption = "Module Code";
			zDropEditColumnStyleInfo4.ColumnName = "ModuleCode";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.Caption = "Break Units";
			zDropEditColumnStyleInfo5.ColumnName = "BreakUnits";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Break/Min. Spend";
			zCalcEditColumnStyleInfo1.ColumnName = "BreakAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Units";
			zCalcEditColumnStyleInfo2.ColumnName = "Units";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Discount %";
			zCalcEditColumnStyleInfo3.ColumnName = "Discount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.Caption = "Start Date";
			zDateEditColumnStyleInfo1.ColumnName = "StartDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.Caption = "End Date";
			zDateEditColumnStyleInfo2.ColumnName = "EndDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = "Duration (Mths)";
			zCalcEditColumnStyleInfo4.ColumnName = "Duration";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.Caption = "Additional Description";
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.Caption = "Comment";
			zTextBoxColumnStyleInfo2.ColumnName = "Comment";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.NewDiscountsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.NewDiscountsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.NewDiscountsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.NewDiscountsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.NewDiscountsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.NewDiscountsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.NewDiscountsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.NewDiscountsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.NewDiscountsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.NewDiscountsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.NewDiscountsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.NewDiscountsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.NewDiscountsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.NewDiscountsGrid.CopySelectedRowsAllowed = true;
			this.NewDiscountsGrid.GridId = "7ceaab3d-975c-4f04-8168-9fc37d1d89c6";
			this.NewDiscountsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NewDiscountsGrid.LayoutKey = "NewDiscountsGrid";
			this.NewDiscountsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.NewDiscountsGrid.Name = "NewDiscountsGrid";
			this.NewDiscountsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1069, 157, true);
			this.NewDiscountsGrid.TabIndex = 1;
			// 
			// ValidateAndSaveButton
			// 
			this.ValidateAndSaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ValidateAndSaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(891, 173, true);
			this.ValidateAndSaveButton.Name = "ValidateAndSaveButton";
			this.ValidateAndSaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 23, true);
			this.ValidateAndSaveButton.TabIndex = 2;
			this.ValidateAndSaveButton.Text = "Validate and Save";
			this.ValidateAndSaveButton.UseVisualStyleBackColor = true;
			this.ValidateAndSaveButton.Click += new System.EventHandler(this.ValidateAndSaveButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1010, 173, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.Text = "Close";
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// BulkAddDiscountForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 229, true);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ValidateAndSaveButton);
			this.Controls.Add(this.NewDiscountsGrid);
			this.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.BulkAddDiscountBizO);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 200, true);
			this.Name = "BulkAddDiscountForm";
			this.Text = "Bulk Add Discounts";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.NewDiscountsGrid, 0);
			this.Controls.SetChildIndex(this.ValidateAndSaveButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NewDiscountsGrid)).EndInit();
			this.NewDiscountsGrid.ResumeLayout(false);
			this.NewDiscountsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid NewDiscountsGrid;
		private ZArchitecture.GUI.ZButton ValidateAndSaveButton;
		private ZArchitecture.GUI.ZButton CloseButton;
	}
}