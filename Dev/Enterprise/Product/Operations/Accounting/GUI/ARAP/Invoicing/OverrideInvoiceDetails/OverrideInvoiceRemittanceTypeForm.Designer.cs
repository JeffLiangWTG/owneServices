namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	partial class OverrideInvoiceRemittanceTypeForm
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
		new void InitializeComponent()
		{
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo paymentReferenceCodeDropEditStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.InvoicesGrid)).BeginInit();
			this.InvoicesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// InvoicesGrid
			// 
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AH_OH";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OverrideInvoicePaymentReferenceCodeForm|A5AACAD5-DC77-4C04-B17E-5CC740064E47", "Client Number");
			zTextBoxColumnStyleInfo1.ColumnName = "OrgARClientNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OverrideInvoicePaymentReferenceCodeForm|CFCF72E3-C37D-446F-895C-E528AB474138", "Invoice Transaction Reference");
			zTextBoxColumnStyleInfo6.ColumnName = "InvoiceTransactionReference";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "AH_TransactionType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.ColumnName = "AH_TransactionCategory";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.ColumnName = "AH_TransactionNum";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AH_JH";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AH_RX_NKTransactionCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "AH_OSTotalAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "AH_Desc";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "AH_OSTaxAmount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "AH_OSExTaxAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			paymentReferenceCodeDropEditStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("EC6F954A-B7E7-4CF2-839E-D5F44FBA9AD2", "Invoice Remittance Type");
			paymentReferenceCodeDropEditStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			paymentReferenceCodeDropEditStyleInfo.ColumnName = "AH_InvoicePaymentReferenceCode";
			this.InvoicesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.InvoicesGrid.ColumnStyles.Add(paymentReferenceCodeDropEditStyleInfo);
			this.InvoicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.InvoicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 181, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 217, true);
			// 
			// OverrideInvoicePaymentReferenceCodeForm
			// 
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OverrideInvoicePaymentReferenceCodeForm|B9B0FF4B-E8AC-4363-8ADB-DF25FB6ECB8A", "Override Invoice Remittance Type");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 241, true);
			this.Name = "OverrideInvoicePaymentReferenceCodeForm";
			((System.ComponentModel.ISupportInitialize)(this.InvoicesGrid)).EndInit();
			this.InvoicesGrid.ResumeLayout(false);
			this.InvoicesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}