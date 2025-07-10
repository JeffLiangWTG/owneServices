using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	public partial class JPManifestBillSpecificUserControl : ZUserControl
	{
		public JPManifestBillSpecificUserControl()
		{
			InitializeComponent();
			InitializeTariffFindBox();
		}

		void InitializeTariffFindBox()
		{
			TariffFindBox.NeedLoadParentDataGroup = false;
			TariffFindBox.NeedLoadNomenclatureWhenTariffNotFound = true;
			TariffFindBox.GetTariffType = () => Bill?.TariffType ?? ZString.Empty;
			TariffFindBox.GetDataGrouping = () => Core.Constants.CountryCodes.Japan;
			TariffFindBox.GetCountryCode = () => Core.Constants.CountryCodes.Japan;
			TariffFindBox.GetEffectiveDate = () => ZDateTime.Today;
			TariffFindBox.SelectNomenclatureModes = new List<SelectionStyle> { SelectionStyle.Tariff, SelectionStyle.Subheading, SelectionStyle.Heading };
		}

		AsycudaBill Bill => (TariffFindBox.Parent is DynamicLayoutPanel dynamicLayoutPanel && dynamicLayoutPanel.CurrentDataItem is AsycudaBill asycudaBill) ? asycudaBill : null;
	}
}
