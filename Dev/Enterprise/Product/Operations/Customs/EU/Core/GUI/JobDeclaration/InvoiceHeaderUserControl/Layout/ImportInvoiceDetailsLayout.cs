using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	sealed class ImportInvoiceDetailsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public ImportInvoiceDetailsLayout()
		{
			Layout = CreateInvoiceDetailsLayout();
		}

		static PanelLayout CreateInvoiceDetailsLayout()
		{
			var builder = new InvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();

			var customsCommonBag = CommercialInvoiceDetailsControlBag.Instance;
			var euBag = InvoiceDetailsControlBag.Instance;

			builder.AddControlBag(customsCommonBag);
			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(customsCommonBag.InvoiceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(customsCommonBag.InvoiceDateEdit, ControlWidthClass.Auto);
			builder.Add(customsCommonBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
			builder.Add(customsCommonBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
			builder.Add(customsCommonBag.IncoTermsUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
			builder.Add(customsCommonBag.IncoTermPlaceTextBox, ControlWidthClass.Auto);
			builder.Add(euBag.IncoTermsAgreedPlaceLongTextControl, ControlWidthClass.Auto);
			builder.Add(customsCommonBag.ValuationCodeDropEdit, ControlWidthClass.Auto);
			builder.Add(customsCommonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(customsCommonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(customsCommonBag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
			builder.Add(customsCommonBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
