using Enterprise.Client.AUS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.AUS.Modules
{
	public partial class OriginPreferenceMappingFilterControl : ZFilterStripControl
	{
		void InitializeComponent()
		{
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Lookups+Organisations";
			zOrganisationFindBoxColumnStyleInfo1.Caption = "Importer";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "T7_OH_Importer";
			zOrganisationFindBoxColumnStyleInfo2.BindToList = "Lookups+Organisations";
			zOrganisationFindBoxColumnStyleInfo2.Caption = "Supplier";
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "T7_OH_Supplier";
			zTextBoxColumnStyleInfo1.Caption = "Origin";
			zTextBoxColumnStyleInfo1.ColumnName = "T7_RN_NKOrigin";
			zTextBoxColumnStyleInfo2.Caption = "Preference Origin";
			zTextBoxColumnStyleInfo2.ColumnName = "T7_RN_NKPreferenceOrigin";
			zTextBoxColumnStyleInfo3.Caption = "Preference Scheme Type";
			zTextBoxColumnStyleInfo3.ColumnName = "T7_PreferenceSchemeType";
			zTextBoxColumnStyleInfo4.Caption = "Preference Rule Type";
			zTextBoxColumnStyleInfo4.ColumnName = "T7_PreferenceRuleType";
			this.FilteredGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 388, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.AUS.Business.ClientAUSOriginPreferenceMapping);
			// 
			// OriginPreferenceMappingFilterControl
			// 
			this.Name = "OriginPreferenceMappingFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 408, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
