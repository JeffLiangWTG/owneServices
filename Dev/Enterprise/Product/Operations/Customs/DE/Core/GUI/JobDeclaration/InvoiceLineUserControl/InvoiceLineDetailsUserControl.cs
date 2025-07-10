using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class InvoiceLineDetailsUserControl : ZUserControl
	{
		public InvoiceLineDetailsUserControl()
		{
			InitializeComponent();

			//Initialize bindings
			BindingSource.SetBindingMember(DescriptionLongTextControl, AutoJobComInvoiceLine.Schema.JI_Description);

			InitializeFixedMaxLengthWithDescriptionTariffFindBox();
		}

		void InitializeFixedMaxLengthWithDescriptionTariffFindBox()
		{
			FixedMaxLengthWithDescriptionTariffFindBox.GetEffectiveDate = () => GetInvoiceLine(FixedMaxLengthWithDescriptionTariffFindBox)?.EffectiveAssessmentDate ?? ZDateTime.Today;
			FixedMaxLengthWithDescriptionTariffFindBox.GetTariffType = () => GetInvoiceLine(FixedMaxLengthWithDescriptionTariffFindBox)?.UniversalTariffType ?? ZString.Empty;
			FixedMaxLengthWithDescriptionTariffFindBox.GetDataGrouping = () => GetInvoiceLine(FixedMaxLengthWithDescriptionTariffFindBox)?.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff) ?? ZString.Empty;
		}

		BaseJobComInvoiceLine GetInvoiceLine(Universal.GUI.TariffFindBox tariffFindBox) => ((DynamicLayoutPanel)tariffFindBox.Parent).CurrentDataItem as BaseJobComInvoiceLine;
	}
}
