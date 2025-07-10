namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class AddPriceCurrencyForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.rateGrid = new Enterprise.ZArchitecture.ZGrid();
			this.roundingGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.rateGrid)).BeginInit();
			this.rateGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.roundingGrid)).BeginInit();
			this.roundingGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 332, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.GUI.PriceCurrencyUpdater);
			// 
			// rateGrid
			// 
			this.rateGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.rateGrid, "Rates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.GUI.PriceCurrencyUpdater)(null)).Rates)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.GUI.PriceExchangeRate)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.GUI.PriceCurrencyUpdater)(null)).Rates)).SyncRoot)).CurrencyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.GUI.PriceExchangeRate)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.GUI.PriceCurrencyUpdater)(null)).Rates)).SyncRoot)).Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.GUI.PriceExchangeRate)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.GUI.PriceCurrencyUpdater)(null)).Rates)).SyncRoot)).Uplift)));
			this.rateGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Currency";
			zTextBoxColumnStyleInfo1.ColumnName = "CurrencyCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Exchange Rate";
			zCalcEditColumnStyleInfo1.ColumnName = "Rate";
			zCalcEditColumnStyleInfo1.Decimals = 9;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Uplift %";
			zCalcEditColumnStyleInfo2.ColumnName = "Uplift";
			zCalcEditColumnStyleInfo2.Decimals = 1;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.rateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.rateGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.rateGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.rateGrid.CopySelectedRowsAllowed = true;
			this.rateGrid.GridId = "fa40d8a1-f579-4dcc-9a3a-57b8127efc92";
			this.rateGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.rateGrid.LayoutKey = "rateGrid";
			this.rateGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 14, true);
			this.rateGrid.Name = "rateGrid";
			this.rateGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 122, true);
			this.rateGrid.TabIndex = 1;
			// 
			// roundingGrid
			// 
			this.roundingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.roundingGrid, "Rounding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.GUI.PriceCurrencyUpdater)(null)).Rounding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.PriceRounding)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.GUI.PriceCurrencyUpdater)(null)).Rounding)).SyncRoot)).PriceBreak)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.PriceRounding)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.GUI.PriceCurrencyUpdater)(null)).Rounding)).SyncRoot)).RoundingScale)));
			this.roundingGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Price Less Than";
			zCalcEditColumnStyleInfo3.ColumnName = "PriceBreak";
			zCalcEditColumnStyleInfo3.Decimals = 1;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = "Round To Nearest";
			zCalcEditColumnStyleInfo4.ColumnName = "RoundingScale";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.roundingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.roundingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.roundingGrid.CopySelectedRowsAllowed = true;
			this.roundingGrid.GridId = "50d625d8-67dd-4ca2-8ec4-be5abbeca8b1";
			this.roundingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.roundingGrid.LayoutKey = "roundingGrid";
			this.roundingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 161, true);
			this.roundingGrid.Name = "roundingGrid";
			this.roundingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 98, true);
			this.roundingGrid.TabIndex = 2;
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 265, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 35, true);
			this.zLabel1.TabIndex = 3;
			this.zLabel1.Text = "The row with zero price break is the default when no other break applies.";
			// 
			// okButton
			// 
			this.okButton.Click += OkButton_Click;
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 306, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 23, true);
			this.okButton.TabIndex = 4;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 306, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 23, true);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// AddPriceCurrencyForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 356, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.roundingGrid);
			this.Controls.Add(this.rateGrid);
			this.DataSourceType = typeof(Enterprise.Client.EDI.Billing.GUI.PriceCurrencyUpdater);
			this.Name = "AddPriceCurrencyForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Add Currency";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.rateGrid, 0);
			this.Controls.SetChildIndex(this.roundingGrid, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.rateGrid)).EndInit();
			this.rateGrid.ResumeLayout(false);
			this.rateGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.roundingGrid)).EndInit();
			this.roundingGrid.ResumeLayout(false);
			this.roundingGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid rateGrid;
		private ZArchitecture.ZGrid roundingGrid;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.GUI.ZButton okButton;
		private ZArchitecture.GUI.ZButton cancelButton;
	}
}