using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class ImportInvoiceLineTemplate : ZUserControl
	{
		public ImportInvoiceLineTemplate()
		{
			InitializeComponent();
			InitializeTariffFindBox();
		}

		void InitializeTariffFindBox()
		{
			TariffFindBox.NeedLoadParentDataGroup = false;
			TariffFindBox.NeedLoadNomenclatureWhenTariffNotFound = true;
			TariffFindBox.GetTariffType = () => Universal.Constants.TariffTypes.Import;
			TariffFindBox.GetDataGrouping = GetDataGroupingForUniversalTariff;
			TariffFindBox.GetCountryCode = () => Core.Constants.CountryCodes.Japan;
			TariffFindBox.GetEffectiveDate = () => GetInvoiceLine(TariffFindBox)?.EffectiveAssessmentDate ?? ZDateTime.Today;
			TariffFindBox.SelectNomenclatureModes = new System.Collections.Generic.List<SelectionStyle> { SelectionStyle.Tariff, SelectionStyle.Subheading };
		}

		BaseJobComInvoiceLine GetInvoiceLine(Universal.GUI.TariffFindBox tariffFindBox) => ((DynamicLayoutPanel)tariffFindBox.Parent).CurrentDataItem as JobComInvoiceLine;

		protected ZString GetDataGroupingForUniversalTariff()
		{
			var currentInvoiceLine = GetInvoiceLine(TariffFindBox);
			return currentInvoiceLine?.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff).ToString() ?? string.Empty;
		}
	}
}
