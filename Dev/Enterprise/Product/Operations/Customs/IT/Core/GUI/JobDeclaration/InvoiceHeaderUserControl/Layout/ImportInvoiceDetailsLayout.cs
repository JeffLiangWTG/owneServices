using Enterprise.Customs.GUI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public sealed class ImportInvoiceDetailsLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => layout ??= CreateLayout();
	PanelLayout layout;

	static PanelLayout CreateLayout()
	{
		var builder = new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.InvoiceDetailsControlBag.Instance;

		builder.AddControlBag(euBag);
		builder.AddColumn();

		builder.Add(commonBag.InvoiceNumberTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceDateEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.IncoTermsUserControl, ControlWidthClass.Auto);
		builder.Add(euBag.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.IncoTermPlaceTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ValuationCodeDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);

		builder.SetCaption(commonBag.IncoTermsUserControl, invoice => InvoiceHeaderDetailsCaptions.Instance.IncoTermsCaption);
		builder.SetCaption(commonBag.IncoTermPlaceTextBox, invoice => InvoiceHeaderDetailsCaptions.Instance.IncoTermPlaceCaption);

		return builder.Build();
	}
}
