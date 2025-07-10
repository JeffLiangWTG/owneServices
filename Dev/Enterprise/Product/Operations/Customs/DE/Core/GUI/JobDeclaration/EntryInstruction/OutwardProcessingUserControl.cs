using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.GUI;
using UniversalReferenceConstants = Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.GUI
{
	public partial class OutwardProcessingUserControl : ZUserControl
	{
		public OutwardProcessingUserControl()
		{
			InitializeComponent();
			InitializeGrid();
		}

		void InitializeGrid()
		{
			ProductGrid.ColumnStyles.Insert(0, new Universal.GUI.TariffColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				GetCountryCode = () => Core.Constants.CountryCodes.Germany,
				GetDataGrouping = () => Core.Constants.CountryCodes.Germany,
				TariffType = UniversalReferenceConstants.CusTariffTypes.ExportTariff,
				GetEffectiveDate = () => ZDateTime.Today,
				CaptionResourceString = Res.GetData("c15c611b-b833-46dc-9c0e-116b71a9e128", "Commodity"),
				ColumnName = ProductSupportingInfo.Schema.FormattedTariff,
				IsMandatory = true
			});
		}
	}
}
