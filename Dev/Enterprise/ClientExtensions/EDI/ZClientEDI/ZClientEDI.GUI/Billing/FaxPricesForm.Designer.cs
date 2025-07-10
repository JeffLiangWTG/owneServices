using Enterprise.Client.EDI.Billing.Business;
namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class FaxPricesForm
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
			if (disposing)
			{
				if (BulkClientFaxPriceUpdater != null)
				{
					BulkClientFaxPriceUpdater.HasChangesChanged -= BulkClientFaxPriceUpdater_HasChangesChanged;
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		BulkClientFaxPriceUpdater BulkClientFaxPriceUpdater
		{
			get { return Updater as BulkClientFaxPriceUpdater; }
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.MonthAndYearPeriodDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ClientFaxPriceGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SaveChangesWarningLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			this.MonthAndYearPeriodDateEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ClientFaxPriceGrid)).BeginInit();
			this.ClientFaxPriceGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 381, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.BulkClientFaxPriceUpdater);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 352, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.PostingButtonsUserControl.TabIndex = 4;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 15, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 14, true);
			this.zLabel1.TabIndex = 3;
			this.zLabel1.Text = "Monthly Period:";
			// 
			// MonthAndYearPeriodDateEdit
			// 
			this.MonthAndYearPeriodDateEdit.AllowDrop = true;
			this.MonthAndYearPeriodDateEdit.AutoCompleteMonthThreshold = 1;
			this.MonthAndYearPeriodDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.MonthAndYearPeriodDateEdit, "MonthAndYearPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Billing.Business.BulkClientFaxPriceUpdater)(null)).MonthAndYearPeriod)));
			this.MonthAndYearPeriodDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 12, true);
			this.MonthAndYearPeriodDateEdit.Name = "MonthAndYearPeriodDateEdit";
			this.MonthAndYearPeriodDateEdit.TabIndex = 1;
			// 
			// ClientFaxPriceGrid
			// 
			this.ClientFaxPriceGrid.AllowNavigation = false;
			this.ClientFaxPriceGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ClientFaxPriceGrid, "ClientFaxPriceList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.BulkClientFaxPriceUpdater)(null)).ClientFaxPriceList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientFaxPrice)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.BulkClientFaxPriceUpdater)(null)).ClientFaxPriceList)).SyncRoot)).CFP_RX_NKCurrencyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.ClientFaxPrice)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.BulkClientFaxPriceUpdater)(null)).ClientFaxPriceList)).SyncRoot)).CFP_PageRate)));
			this.ClientFaxPriceGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.Caption = "Currency";
			zCodeFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CFP_RX_NKCurrencyCode";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Page Rate";
			zCalcEditColumnStyleInfo1.ColumnName = "CFP_PageRate";
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ClientFaxPriceGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ClientFaxPriceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ClientFaxPriceGrid.CopySelectedRowsAllowed = true;
			this.ClientFaxPriceGrid.GridId = "0c674b38-17ba-4b88-b59c-5a5b4ae27248";
			this.ClientFaxPriceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ClientFaxPriceGrid.LayoutKey = "ClientFaxPriceGrid";
			this.ClientFaxPriceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 51, true);
			this.ClientFaxPriceGrid.Name = "ClientFaxPriceGrid";
			this.ClientFaxPriceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 295, true);
			this.ClientFaxPriceGrid.TabIndex = 2;
			// 
			// SaveChangesWarningLabel
			// 
			this.SaveChangesWarningLabel.AutoSize = true;
			this.SaveChangesWarningLabel.ForeColor = System.Drawing.Color.Red;
			this.SaveChangesWarningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 35, true);
			this.SaveChangesWarningLabel.Name = "SaveChangesWarningLabel";
			this.SaveChangesWarningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 14, true);
			this.SaveChangesWarningLabel.TabIndex = 5;
			this.SaveChangesWarningLabel.Text = "Save your changes before changing the date";
			this.SaveChangesWarningLabel.Visible = false;
			// 
			// FaxPricesForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 405, true);
			this.Controls.Add(this.SaveChangesWarningLabel);
			this.Controls.Add(this.MonthAndYearPeriodDateEdit);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.ClientFaxPriceGrid);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.BulkClientFaxPriceUpdater);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 400, true);
			this.Name = "FaxPricesForm";
			this.Text = "Fax Prices";
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.ClientFaxPriceGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.MonthAndYearPeriodDateEdit, 0);
			this.Controls.SetChildIndex(this.SaveChangesWarningLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.MonthAndYearPeriodDateEdit.ResumeLayout(true);
			this.MonthAndYearPeriodDateEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ClientFaxPriceGrid)).EndInit();
			this.ClientFaxPriceGrid.ResumeLayout(false);
			this.ClientFaxPriceGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZArchitecture.ZGrid ClientFaxPriceGrid;
		private ZArchitecture.ZLabel zLabel1;
		public ZArchitecture.GUI.ZDateEdit MonthAndYearPeriodDateEdit;
		public ZArchitecture.ZLabel SaveChangesWarningLabel;
	}
}
