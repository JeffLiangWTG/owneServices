namespace Enterprise.Customs.BR.GUI
{
	public partial class NFEImportForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.NFEGrid = new Enterprise.ZArchitecture.ZGrid();
			this.FileContentGroupBox.SuspendLayout();
			this.LogDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NFEGrid)).BeginInit();
			this.NFEGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// FileNameTextBox
			// 
			this.FileNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 15, true);
			// 
			// BrowseButton
			// 
			this.BrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(599, 8, true);
			// 
			// ImportButton
			// 
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(516, 519, true);
			// 
			// CloseButton
			// 
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(599, 519, true);
			// 
			// FileContentGroupBox
			// 
			this.FileContentGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("ABCB592E-60A9-4078-A5A1-1D718CC2FBFD", "NF-e");
			this.FileContentGroupBox.Controls.Add(this.NFEGrid);
			this.FileContentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(662, 328, true);
			// 
			// LogDetailsGroupBox
			// 
			this.LogDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 370, true);
			this.LogDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 139, true);
			// 
			// LogListBox
			// 
			this.LogDetailsListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(657, 102, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 548, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 24, true);
			// 
			// NFEGrid
			// 
			this.NFEGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NFEGrid, "NFEImportObjectCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.NFEImportObjectParent)(null)).NFEImportObjectCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.NFEImportObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.NFEImportObjectParent)(null)).NFEImportObjectCollection)).SyncRoot)).NfeKey)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.NFEImportObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.NFEImportObjectParent)(null)).NFEImportObjectCollection)).SyncRoot)).NfeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.NFEImportObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.NFEImportObjectParent)(null)).NFEImportObjectCollection)).SyncRoot)).NfeSerie)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.BR.Business.NFEImportObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.NFEImportObjectParent)(null)).NFEImportObjectCollection)).SyncRoot)).NfeDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.NFEImportObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.NFEImportObjectParent)(null)).NFEImportObjectCollection)).SyncRoot)).Incoterm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.BR.Business.NFEImportObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.NFEImportObjectParent)(null)).NFEImportObjectCollection)).SyncRoot)).ExchangeRateDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.NFEImportObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.NFEImportObjectParent)(null)).NFEImportObjectCollection)).SyncRoot)).CurrencyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.NFEImportObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.NFEImportObjectParent)(null)).NFEImportObjectCollection)).SyncRoot)).ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.NFEImportObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.NFEImportObjectParent)(null)).NFEImportObjectCollection)).SyncRoot)).ExchangeRateBuy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.NFEImportObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.NFEImportObjectParent)(null)).NFEImportObjectCollection)).SyncRoot)).ExchangeRateSell)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.BR.Business.NFEImportObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.NFEImportObjectParent)(null)).NFEImportObjectCollection)).SyncRoot)).InvoiceHeaderPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.BR.Business.NFEImportObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.NFEImportObjectParent)(null)).NFEImportObjectCollection)).SyncRoot)).EntryInstructionPK)));
			this.NFEGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "NfeKey";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.ColumnName = "NfeNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.ColumnName = "NfeSerie";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "NfeDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "Incoterm";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "ExchangeRateDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CurrencyCode";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ExchangeRate";
			zCalcEditColumnStyleInfo1.Decimals = 9;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "ExchangeRateBuy";
			zCalcEditColumnStyleInfo2.Decimals = 9;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "ExchangeRateSell";
			zCalcEditColumnStyleInfo3.Decimals = 9;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidDropEditColumnStyleInfo1.ColumnName = "InvoiceHeaderPK";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidDropEditColumnStyleInfo2.ColumnName = "EntryInstructionPK";
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.NFEGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.NFEGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.NFEGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.NFEGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.NFEGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.NFEGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.NFEGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.NFEGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.NFEGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.NFEGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.NFEGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.NFEGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.NFEGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NFEGrid.GridId = "8afa1417-b14c-4534-adff-0510fe9ed258";
			this.NFEGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NFEGrid.LayoutKey = "NFEGrid";
			this.NFEGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.NFEGrid.Name = "NFEGrid";
			this.NFEGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 313, true);
			this.NFEGrid.TabIndex = 0;
			// 
			// NFEImportForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 572, true);
			this.Name = "NFEImportForm";
			this.FileContentGroupBox.ResumeLayout(false);
			this.FileContentGroupBox.PerformLayout();
			this.LogDetailsGroupBox.ResumeLayout(false);
			this.LogDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NFEGrid)).EndInit();
			this.NFEGrid.ResumeLayout(false);
			this.NFEGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid NFEGrid;
	}
}
