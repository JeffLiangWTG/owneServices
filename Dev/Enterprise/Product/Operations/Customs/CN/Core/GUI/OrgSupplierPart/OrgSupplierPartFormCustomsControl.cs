using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;

namespace Enterprise.Customs.CN.GUI
{
	public partial class OrgSupplierPartFormCustomsControl : Customs.GUI.OrgSupplierPartFormCustomsControlGlobal
	{
		protected override string TariffColumnNameCore => CusClassPartPivot.Schema.CI_TariffNum;
		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.China;
		protected override ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.China;
		protected override string UniversalTariffType => Universal.Constants.TariffTypes.HarmonizedSystem;

		protected new CusClassPartPivot currentPartPivot => base.currentPartPivot as CusClassPartPivot;

		public OrgSupplierPartFormCustomsControl()
		{
			InitializeComponent();

			BindingSource.SetBindingMember(CIQIngredientTextBox, "PivotsForBinding.CIQIngredient");
			CNC_CargoAttributesTextBox.SetBindingMember("PivotsForBinding.CargoAttributesAsString", x => currentPartPivot?.CargoAttributes);

			TariffCodeFindBox.GetCountryCode = GetCustomsCountryCode;
			TariffCodeFindBox.GetDataGrouping = GetDataGroupingForUniversalTariff;
			TariffCodeFindBox.TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;
			TariffCodeFindBox.PartialDescriptionMinLengthForSearch = 2;
			PivotGrid.RemoveFromAvailableColumns(CusClassPartPivot.Schema.CI_CC);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			CNC_NonDangerousChemicalFlagCheckBox.DataBindings.RemoveBinding(IsVisibleForBindingConst);

			if (dataSource != null)
			{
				CNC_NonDangerousChemicalFlagCheckBox.DataBindings.Add(new KBinding(IsVisibleForBindingConst, BindingSource.DataSource, "PivotsForBinding.NonDangerousChemicalFlagVisible", false, DataSourceUpdateMode.Never));
			}
		}
		const string IsVisibleForBindingConst = "IsVisibleForBinding";

		protected override void InitializeForm()
		{
			base.InitializeForm();
			var tariffColumn = PivotGrid.GetColumnStyle(TariffColumnName) as Universal.GUI.TariffColumnStyleInfo;
			if (tariffColumn != null)
			{
				tariffColumn.PartialDescriptionMinLengthForSearch = 2;
			}
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();

			if (currentPartPivot != null)
			{
				var isImport = currentPartPivot.CI_ChildType == ClassificationTypeList.Codes.HTI;
				var isExport = currentPartPivot.CI_ChildType == ClassificationTypeList.Codes.HTE;

				CNC_OriginDistrictCodeFindBox.Visible = CNC_OriginRegionCodeFindBox.Visible = !isImport;
				CI_RW_NKOriginStateDropEdit.Visible = CNC_OriginStateCodeFindBox.Visible = CNC_DestinationDistrictCodeFindBox.Visible = CNC_DestinationRegionCodeFindBox.Visible = !isExport;
			}
		}

		void GoodsSpecModelButton_Click(object sender, System.EventArgs e)
		{
			if (currentPartPivot != null)
			{
				AdditionalInformationForm.ShowDialog(currentPartPivot, currentPartPivot.CNC_NameOfGoodsInfo, currentPartPivot.CNC_GoodsSpecModelInfo, currentPartPivot.IsEnteringOrExiting);
			}
		}
	}
}
