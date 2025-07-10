using Enterprise.Customs.GUI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public sealed class MiscellaneousInvoiceDetailsLayout : IPanelLayoutProvider
{
	#region IPanelLayoutProvider

	PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateLayout());
	PanelLayout layout;

	#endregion

	PanelLayout CreateLayout()
	{
		var builder = new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
		var commonBag = builder.CommonBag;
		var itBag = InvoiceDetailsControlBag.Instance;

		builder.AddControlBag(itBag);
		builder.AddColumn();

		builder.Add(commonBag.InvoiceNumberTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceDateEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.IncoTermsUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.IncoTermPlaceTextBox, ControlWidthClass.Auto);
		builder.Add(itBag.AgreedPlaceCodeDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.ValuationCodeDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);

		builder.SetCaption(commonBag.IncoTermPlaceTextBox, _ => InvoiceHeaderDetailsCaptions.Instance.IncoTermPlaceCaption);
		builder.SetCaption(itBag.AgreedPlaceCodeDropEdit, _ => InvoiceHeaderDetailsCaptions.Instance.AgreedPlaceCodeCaption);
		builder.SetCaption(commonBag.InvoiceAmountConvertToLocalCurrencyControl, _ => InvoiceHeaderDetailsCaptions.Instance.InvoiceAmountConvertToLocalCurrencyCaption);
		builder.SetCaption(commonBag.ValuationCodeDropEdit, _ => InvoiceHeaderDetailsCaptions.Instance.ValuationCodeCaption);
		builder.SetCaption(commonBag.InvoiceCurrExRateCalcEdit, _ => InvoiceHeaderDetailsCaptions.Instance.InvoiceCurrExRateCaption);

		return builder.Build();
	}
}
