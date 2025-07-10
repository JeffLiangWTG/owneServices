namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	partial class OverrideInvoiceLineSequenceForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.InvoicesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// InvoicesGrid
			//
			zTextBoxColumnStyleInfo1.ColumnName = "AL_Sequence";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AL_GB";
			zGuidFindBoxColumnStyleInfo2.ColumnName = "AL_GE";
			zGuidFindBoxColumnStyleInfo3.ColumnName = "AL_JH";
			zGuidFindBoxColumnStyleInfo4.ColumnName = "AL_AT";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OverrideInvoiceLineDescriptionForm|cfc17d86-95e5-40ea-826f-8395f5f40fbd", "Local Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "AL_LocalExTaxAmount";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OverrideInvoiceLineDescriptionForm|4116f1db-e7a4-4e87-80c7-ab6e3930032b", "GST Local");
			zCalcEditColumnStyleInfo2.ColumnName = "AL_LocalGSTAmount";
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OverrideInvoiceLineDescriptionForm|b77c78f4-d0a7-4909-b1e7-c1a81387ed98", "Local Tax");
			zCalcEditColumnStyleInfo3.ColumnName = "AL_LocalTaxAmount";
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OverrideInvoiceLineDescriptionForm|8d4b5f3b-10bc-446a-ac23-8e765bfa56e5", "Local Total");
			zCalcEditColumnStyleInfo4.ColumnName = "AL_LocalTotalAmount";
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OverrideInvoiceLineDescriptionForm|8beb10d6-f25f-4781-916b-f76dfca06616", "Amount");
			zCalcEditColumnStyleInfo5.ColumnName = "AL_OSExTaxAmount";
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OverrideInvoiceLineDescriptionForm|ce9ee2a4-15a8-40ce-a437-870863e71569", "GST Amount");
			zCalcEditColumnStyleInfo6.ColumnName = "AL_OSGSTAmount";
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OverrideInvoiceLineDescriptionForm|d85ab6b3-5234-4e4d-bd32-b778e7787e87", "Tax");
			zCalcEditColumnStyleInfo7.ColumnName = "AL_OSTaxAmount";
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OverrideInvoiceLineDescriptionForm|c96cce92-4428-4501-9719-c7e00034fa71", "Total");
			zCalcEditColumnStyleInfo8.ColumnName = "AL_OverseasTotal";
			zTextBoxColumnStyleInfo2.ColumnName = "AL_RX_NKTransactionCurrency";
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "AL_ExchangeRate";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OverrideInvoiceLineDescriptionForm|013c56ae-f769-4514-b5fd-a39a10a281f5", "Charge Type");
			zTextBoxColumnStyleInfo3.ColumnName = "ChargeTypeWithOverride";
			zGuidFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OverrideInvoiceLineDescriptionForm|4b93a4f0-1428-4e42-a769-1480dcc1dbcf", "Charges");
			zGuidFindBoxColumnStyleInfo5.ColumnName = "GenericCharge";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OverrideInvoiceLineDescriptionForm|63861230-a9de-4c0a-8050-7e5771944c11", "Job Local Reference");
			zTextBoxColumnStyleInfo4.ColumnName = "JobLocalReference";
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.InvoicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.InvoicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.InvoicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.InvoicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 122, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 158, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceLineOverrideForEditingSequenceAdaptor);
			// 
			// OverrideInvoiceLineSequenceForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ECF0C3AB-1CEC-47D5-93E6-FF22E91B14C9", "Override Line Charge Sequence");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 182, true);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceLineOverrideForEditingSequenceAdaptor);
			this.Name = "OverrideInvoiceLineSequenceForm";
			((System.ComponentModel.ISupportInitialize)(this.InvoicesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
