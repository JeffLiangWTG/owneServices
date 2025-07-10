using CargoWise.Common;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.GUI
{
	public partial class OrgSupplierPartFormCustomsControlGlobal : Customs.GUI.OrgSupplierPartFormCustomsControlGlobal
	{
		public OrgSupplierPartFormCustomsControlGlobal()
		{
			InitializeComponent();

			if (!this.IsDesignMode() && Registry.BRCustomsDataRegistry.Instance.EnableCatalogModule.Value)
			{
				CreateNewGuidFindBoxColumn(CusClassPartPivot.Schema.CI_CGC_Catalog, 120, ModuleIDs.Customs.GoodsCatalog);
			}
		}

		protected override string TariffColumnNameCore => CusClassPartPivotSchema.Constants.CI_TariffNum;
		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.Brazil;
		protected override ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.Brazil;

		protected override void ChangeControlsVisibility()
		{
			if (currentPartPivot != null)
			{
				NveTabPage.TabVisible = currentPartPivot.IsImportClassification;
				AttributesNcmTabPage.TabVisible = currentPartPivot.IsExportClassification;
				AdditionalTariffsTabPage.TabVisible = currentPartPivot.IsImportClassification;
			}
		}

		protected ZGuidFindBoxColumnStyleInfo CreateNewGuidFindBoxColumn(ZString column, ZInt length, ModuleIdentifier module, ResourceStringData groupName = null)
		{
			var zGuidFindBoxColumnStyleInfo = new ZGuidFindBoxColumnStyleInfo();
			zGuidFindBoxColumnStyleInfo.ColumnName = column;
			zGuidFindBoxColumnStyleInfo.ModuleID = module;
			zGuidFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(length);
			zGuidFindBoxColumnStyleInfo.GroupName = groupName;
			PivotGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo);
			return zGuidFindBoxColumnStyleInfo;
		}
	}
}
