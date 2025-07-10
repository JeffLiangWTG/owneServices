namespace Enterprise.Accounting.Module
{
	public partial class AccCollectionOrderFilterControl
	{
		#region Component Designer generated code

		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			zTextBoxColumnStyleInfo1.ColumnName = "ACO_BatchNumber";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("359003A7-BB9B-471C-B999-B90A2D61BEFE", "Batch Number");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "ACO_Calc_AB_Code";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("8A09D2C6-B1DE-4C68-BDAD-EDB39889F492", "Bank Account");
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.ColumnName = "ACO_OrderNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "ACO_OH_Debtor";
			zTextBoxColumnStyleInfo4.ColumnName = "ACO_RX_NKCurrency";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "ACO_Amount";
			zTextBoxColumnStyleInfo5.ColumnName = "ACO_CollectionDate";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("4c447611-96a7-4e1c-80ea-46c2f6a1b373", "Bank Name");
			zTextBoxColumnStyleInfo7.ColumnName = "CollectionRequestBankName";
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("7194acfa-c836-4fd1-bbee-39d5ac4ad295", "Bank Country/Region");
			zTextBoxColumnStyleInfo9.ColumnName = "CollectionRequestBankCountry";
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("8730c27e-2b45-4871-92b1-d1399a970755", "Bank/Branch Code");
			zTextBoxColumnStyleInfo8.ColumnName = "CollectionRequestBankBsb";
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ce55b8ff-94e3-40c8-a765-f566b44ca2fa", "Account Name");
			zTextBoxColumnStyleInfo11.ColumnName = "CollectionRequestAccountName";
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("d2814f80-5f8f-4a6d-b312-8ccee091b57a", "Account Num.");
			zTextBoxColumnStyleInfo12.ColumnName = "CollectionRequestAccountNumber";
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("f1054b94-f4e6-4a85-8f2d-b925e96e1b1d", "Account Currency");
			zTextBoxColumnStyleInfo13.ColumnName = "CollectionRequestAccountCurrency";
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ccb6a63c-b048-417b-8f49-cdd51bb96d6a", "Bank Swift");
			zTextBoxColumnStyleInfo10.ColumnName = "CollectionRequestBankSwift";
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("4119a9df-5568-4384-9b64-b7885be3ad28", "IBAN Num.");
			zTextBoxColumnStyleInfo14.ColumnName = "CollectionRequestIBANNumber";
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("42296a1e-9772-4b22-95ba-a93b5dddafd6", "Batch Type");
			zTextBoxColumnStyleInfo15.ColumnName = "ACO_BatchType";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo16.ColumnName = "ACO_DepositedDate";
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("912DA237-6D5E-4586-9AC6-ECF4CA9127BC", "Canceled/Rejected");
			zCheckBoxColumnStyleInfo2.ColumnName = "ACO_IsCancelled";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("80d9b223-002f-4536-8f51-2ea6ad823410", "Rejected Reason");
			zTextBoxColumnStyleInfo6.ColumnName = "ACO_CancelledReason";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.CopySelectedRowsAllowed = true;
			this.grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 54, true);
			this.grid.ReadOnly = true;
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 474, true);
			this.grid.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AccCollectionOrderFilterBusinessObject);
			// 
			// CollectionOrdersDisplayGrid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// 
			// AccCollectionOrderFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "AccCollectionOrderFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 528, true);
			this.Controls.SetChildIndex(this.FilterStripsPanel, 0);
			this.Controls.SetChildIndex(this.AddStripButton, 0);
			this.Controls.SetChildIndex(this.ToolStripRecordsFoundLabel, 0);
			this.Controls.SetChildIndex(this.grid, 0);

			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
