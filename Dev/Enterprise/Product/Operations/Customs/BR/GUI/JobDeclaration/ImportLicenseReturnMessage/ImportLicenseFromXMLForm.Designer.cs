namespace Enterprise.Customs.BR.GUI
{
	public partial class ImportLicenseFromXMLForm
	{
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ImportLicensesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ImportLicensesGrid)).BeginInit();
			this.ImportLicensesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 567, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.IXmlLoadingObjectParent<Enterprise.Customs.BR.Business.ImportLicenseLoadingObjectCollection>);
			// 
			// LogDetailsGroupBox
			// 
			this.LogDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 158, true);
			// 
			// FileContentGroupBox
			//
			this.FileContentGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("2E2E4845-DD8D-4255-B288-5B8C6DEBA9B8", "Import License");
			this.FileContentGroupBox.Controls.Add(this.ImportLicensesGrid);
			this.FileContentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 328, true);
			// 
			// ImportButton
			// 
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 542, true);
			// 
			// CloseButton
			// 
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(633, 542, true);
			// 
			// ImportLicensesGrid
			// 
			this.ImportLicensesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ImportLicensesGrid, "Collection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IXmlLoadingObjectParent<Enterprise.Customs.BR.Business.ImportLicenseLoadingObjectCollection>)(null)).Collection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ImportLicenseLoadingObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IXmlLoadingObjectParent<Enterprise.Customs.BR.Business.ImportLicenseLoadingObjectCollection>)(null)).Collection)).SyncRoot)).ImportLicenseNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.BR.Business.ImportLicenseLoadingObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IXmlLoadingObjectParent<Enterprise.Customs.BR.Business.ImportLicenseLoadingObjectCollection>)(null)).Collection)).SyncRoot)).RegistrationDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.BR.Business.ImportLicenseLoadingObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IXmlLoadingObjectParent<Enterprise.Customs.BR.Business.ImportLicenseLoadingObjectCollection>)(null)).Collection)).SyncRoot)).InvoiceHeaderPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ImportLicenseLoadingObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IXmlLoadingObjectParent<Enterprise.Customs.BR.Business.ImportLicenseLoadingObjectCollection>)(null)).Collection)).SyncRoot)).ImportLicenseType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.BR.Business.ImportLicenseLoadingObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IXmlLoadingObjectParent<Enterprise.Customs.BR.Business.ImportLicenseLoadingObjectCollection>)(null)).Collection)).SyncRoot)).ImportLicenseAuthorizationDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ImportLicenseLoadingObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IXmlLoadingObjectParent<Enterprise.Customs.BR.Business.ImportLicenseLoadingObjectCollection>)(null)).Collection)).SyncRoot)).ImportLicenseFeeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ImportLicenseLoadingObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IXmlLoadingObjectParent<Enterprise.Customs.BR.Business.ImportLicenseLoadingObjectCollection>)(null)).Collection)).SyncRoot)).Incoterm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ImportLicenseLoadingObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IXmlLoadingObjectParent<Enterprise.Customs.BR.Business.ImportLicenseLoadingObjectCollection>)(null)).Collection)).SyncRoot)).Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.ImportLicenseLoadingObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IXmlLoadingObjectParent<Enterprise.Customs.BR.Business.ImportLicenseLoadingObjectCollection>)(null)).Collection)).SyncRoot)).VMLE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.ImportLicenseLoadingObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IXmlLoadingObjectParent<Enterprise.Customs.BR.Business.ImportLicenseLoadingObjectCollection>)(null)).Collection)).SyncRoot)).VMCV)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.ImportLicenseLoadingObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IXmlLoadingObjectParent<Enterprise.Customs.BR.Business.ImportLicenseLoadingObjectCollection>)(null)).Collection)).SyncRoot)).NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ImportLicenseLoadingObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.IXmlLoadingObjectParent<Enterprise.Customs.BR.Business.ImportLicenseLoadingObjectCollection>)(null)).Collection)).SyncRoot)).UQ)));
			this.ImportLicensesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ImportLicenseNo";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zDateEditColumnStyleInfo1.ColumnName = "RegistrationDate";
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidDropEditColumnStyleInfo1.ColumnName = "InvoiceHeaderPK";
			zGuidDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo1.ColumnName = "ImportLicenseType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo2.ColumnName = "ImportLicenseAuthorizationDate";
			zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo2.ColumnName = "ImportLicenseFeeType";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.ColumnName = "Incoterm";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.ColumnName = "Currency";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "VMLE";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "VMCV";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "NetWeight";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo4.ColumnName = "UQ";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.ImportLicensesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ImportLicensesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ImportLicensesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.ImportLicensesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ImportLicensesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ImportLicensesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ImportLicensesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ImportLicensesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ImportLicensesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ImportLicensesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ImportLicensesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ImportLicensesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ImportLicensesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImportLicensesGrid.GridId = "7985067F-A5CB-4E6E-AF42-19DBC7EB634E";
			this.ImportLicensesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ImportLicensesGrid.LayoutKey = "ImportLicensesGrid";
			this.ImportLicensesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ImportLicensesGrid.Name = "ImportLicensesGrid";
			this.ImportLicensesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 332, true);
			this.ImportLicensesGrid.TabIndex = 8;
			// 
			// ImportLicenseFromXMLForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("91127c86-bcb7-45f8-9998-866345016bda", "Load Import License(s)");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 591, true);
			this.DataSourceType = typeof(Enterprise.Customs.BR.Business.IXmlLoadingObjectParent<Enterprise.Customs.BR.Business.ImportLicenseLoadingObjectCollection>);
			this.Name = "ImportLicenseFromXMLForm";
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ImportLicensesGrid)).EndInit();
			this.ImportLicensesGrid.ResumeLayout(false);
			this.ImportLicensesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.ZGrid ImportLicensesGrid;
	}
}
