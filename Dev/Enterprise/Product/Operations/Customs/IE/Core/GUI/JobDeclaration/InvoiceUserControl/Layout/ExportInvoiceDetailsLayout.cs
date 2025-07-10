using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public sealed class ExportInvoiceDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();
		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
			var commonBag = builder.CommonBag;
			var euBag = InvoiceDetailsControlBag.Instance;

			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(commonBag.InvoiceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.IncoTermsUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.IncoTermPlaceTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.AdditionalTermsTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ValuationCodeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.TransportChargesMethodOfPaymentDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.UCRTextBox, ControlWidthClass.Auto);

			builder.SetVisibility(commonBag.IncoTermPlaceTextBox, i => i.IsIncoTermPlaceVisible, invoice => invoice.ZG_AgreedPlaceCodeInfo);
			return builder.Build();
		}
	}
}
