using Enterprise.Client.AUS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.AUS.Modules
{
	public partial class ProductImportRegistryControl : ZFilterStripControl
	{
		void InitializeComponent()
		{
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Lookups+Organisations";
			zOrganisationFindBoxColumnStyleInfo1.Caption = "Importer";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "T6_OH_Importer";
			zOrganisationFindBoxColumnStyleInfo2.BindToList = "Lookups+Organisations";
			zOrganisationFindBoxColumnStyleInfo2.Caption = "Supplier";
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "T6_OH_Supplier";
			zTextBoxColumnStyleInfo1.Caption = "Directory To Store Files";
			zTextBoxColumnStyleInfo1.ColumnName = "T6_DirectoryToStoreFiles";
			zTextBoxColumnStyleInfo2.Caption = "All Products File Name";
			zTextBoxColumnStyleInfo2.ColumnName = "T6_AllProductsFileName";
			zTextBoxColumnStyleInfo3.Caption = "Client Invoicing File Name";
			zTextBoxColumnStyleInfo3.ColumnName = "T6_ClientInvoicingFileName";
			zTextBoxColumnStyleInfo4.Caption = "Product Update File Name";
			zTextBoxColumnStyleInfo4.ColumnName = "T6_ProductUpdateFileName";
			zTextBoxColumnStyleInfo5.Caption = "Email";
			zTextBoxColumnStyleInfo5.ColumnName = "T6_Email";
			zTextBoxColumnStyleInfo6.Caption = "Back Up File Name";
			zTextBoxColumnStyleInfo6.ColumnName = "T6_BackUpFileName";
			zTextBoxColumnStyleInfo7.Caption = "Directory For Imported Parts";
			zTextBoxColumnStyleInfo7.ColumnName = "T6_DirectoryImportedParts";
			zTextBoxColumnStyleInfo8.Caption = "Directory For Rejected Parts";
			zTextBoxColumnStyleInfo8.ColumnName = "T6_DirectoryRejectedParts";
			this.FilteredGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 413, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.AUS.Business.ClientAUSProductImportRegistry);
			// 
			// ProductImportRegistryControl
			// 
			this.Name = "ProductImportRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
