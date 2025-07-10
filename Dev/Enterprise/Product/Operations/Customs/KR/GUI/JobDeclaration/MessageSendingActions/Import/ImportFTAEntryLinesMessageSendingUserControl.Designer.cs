namespace Enterprise.Customs.KR.GUI
{
	partial class ImportFTAEntryLinesMessageSendingUserControl
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
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.FTAEntryLinesGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FTAEntryLinesGrid)).BeginInit();
            this.FTAEntryLinesGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.FTAMessageSendingObject);
            // 
            // FTAEntryLinesGrid
            // 
            this.FTAEntryLinesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.FTAEntryLinesGrid, "FTALines");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).EntryLineNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).FTASequenceNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).FormattedHSCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).Preference)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).TotalNetWeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).SplitOrder)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).NetWeightInKG)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).COONo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).COOIssuedDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).CountryOfOrigin)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).TariffRate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).COOProductType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).ThirdCountryAdditionalInvoiceIssued)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).ThirdCountry)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).COOExporterNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).AssociatedCOOIssuingCountryCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).COOIssuingAgencyType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).COOAgencyName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).COOSupportingDocType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(null)).FTALines)).SyncRoot)).COOIssuerType)));
            this.FTAEntryLinesGrid.CaptionVisible = false;
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "EntryLineNo";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(91);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.ColumnName = "FTASequenceNumber";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zTextBoxColumnStyleInfo1.ColumnName = "FormattedHSCode";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(67);
            zTextBoxColumnStyleInfo2.ColumnName = "Preference";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.ColumnName = "TotalNetWeight";
            zCalcEditColumnStyleInfo3.Decimals = 3;
            zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo4.ColumnName = "SplitOrder";
            zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
            zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("c4c9ec10-0237-40cf-be6b-8cff2d8a3c11", "C/O Net Weight");
            zCalcEditColumnStyleInfo5.ColumnName = "NetWeightInKG";
            zCalcEditColumnStyleInfo5.Decimals = 3;
            zCalcEditColumnStyleInfo5.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(155);
            zTextBoxColumnStyleInfo3.ColumnName = "COONo";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zDateEditColumnStyleInfo1.ColumnName = "COOIssuedDate";
            zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(111);
            zTextBoxColumnStyleInfo4.ColumnName = "CountryOfOrigin";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.IsVisible = false;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(116);
            zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo6.ColumnName = "TariffRate";
            zCalcEditColumnStyleInfo6.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo6.IsVisible = false;
            zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zTextBoxColumnStyleInfo5.ColumnName = "COOProductType";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.IsVisible = false;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(138);
            zTextBoxColumnStyleInfo6.ColumnName = "ThirdCountryAdditionalInvoiceIssued";
            zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo6.IsVisible = false;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(211);
            zTextBoxColumnStyleInfo7.ColumnName = "ThirdCountry";
            zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo7.IsVisible = false;
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
            zTextBoxColumnStyleInfo8.ColumnName = "COOExporterNumber";
            zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo8.IsVisible = false;
            zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(143);
            zTextBoxColumnStyleInfo9.ColumnName = "AssociatedCOOIssuingCountryCode";
            zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo9.IsVisible = false;
            zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(175);
            zTextBoxColumnStyleInfo10.ColumnName = "COOIssuingAgencyType";
            zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo10.IsVisible = false;
            zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(151);
            zTextBoxColumnStyleInfo11.ColumnName = "COOAgencyName";
            zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo11.IsVisible = false;
            zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(116);
            zTextBoxColumnStyleInfo12.ColumnName = "COOSupportingDocType";
            zTextBoxColumnStyleInfo12.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo12.IsVisible = false;
            zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(151);
            zTextBoxColumnStyleInfo13.ColumnName = "COOIssuerType";
            zTextBoxColumnStyleInfo13.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo13.IsVisible = false;
            zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(121);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
            this.FTAEntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
            this.FTAEntryLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FTAEntryLinesGrid.GridId = "94066131-796E-4748-B4D8-BB44380CF6EB";
            this.FTAEntryLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.FTAEntryLinesGrid.LayoutKey = "FTAEntryLinesGrid";
            this.FTAEntryLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.FTAEntryLinesGrid.Name = "FTAEntryLinesGrid";
            this.FTAEntryLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 120, true);
            this.FTAEntryLinesGrid.TabIndex = 0;
            // 
            // ImportFTAEntryLinesMessageSendingUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.FTAEntryLinesGrid);
            this.Name = "ImportFTAEntryLinesMessageSendingUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 120, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FTAEntryLinesGrid)).EndInit();
            this.FTAEntryLinesGrid.ResumeLayout(false);
            this.FTAEntryLinesGrid.PerformLayout();
			this.CaptionRenderingEnabled = true;
			this.ResumeLayout(false);
            this.PerformLayout();

		}

		public ZArchitecture.ZGrid FTAEntryLinesGrid;

		#endregion
	}
}
