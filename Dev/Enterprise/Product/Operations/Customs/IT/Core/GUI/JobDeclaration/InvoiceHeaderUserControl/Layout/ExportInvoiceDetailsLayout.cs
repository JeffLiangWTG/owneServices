using Enterprise.Customs.GUI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public sealed class ExportInvoiceDetailsLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateLayout());
	PanelLayout layout;

	static PanelLayout CreateLayout()
	{
		var builder = new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.InvoiceDetailsControlBag.Instance;
		var itBag = InvoiceDetailsControlBag.Instance;

		builder.AddControlBag(itBag);
		builder.AddControlBag(euBag);
		builder.AddColumn();

		builder.Add(commonBag.InvoiceNumberTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceDateEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.IncoTermsUserControl, ControlWidthClass.Auto);
		builder.Add(euBag.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
		builder.Add(itBag.AgreedPlaceCodeDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.IncoTermPlaceTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.AdditionalTermsTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ValuationCodeDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.TransportChargesMethodOfPaymentDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);
		builder.SetCaption(commonBag.IncoTermPlaceTextBox, _ => InvoiceHeaderDetailsCaptions.Instance.IncoTermPlaceCaption);
		builder.SetCaption(euBag.AgreedPlaceCodeFindBox, _ => InvoiceHeaderDetailsCaptions.Instance.IncotermPlaceCodeCaption);
		builder.SetCaption(commonBag.AdditionalTermsTextBox, _ => InvoiceHeaderDetailsCaptions.Instance.AdditionalTermsCaption);
		builder.SetCaption(itBag.AgreedPlaceCodeDropEdit, _ => InvoiceHeaderDetailsCaptions.Instance.AgreedPlaceCodeCaption);
		builder.SetCaption(commonBag.InvoiceAmountConvertToLocalCurrencyControl, _ => InvoiceHeaderDetailsCaptions.Instance.InvoiceAmountConvertToLocalCurrencyCaption);
		builder.SetCaption(commonBag.ValuationCodeDropEdit, _ => InvoiceHeaderDetailsCaptions.Instance.ValuationCodeCaption);
		builder.SetCaption(euBag.TransportChargesMethodOfPaymentDropEdit, _ => InvoiceHeaderDetailsCaptions.Instance.TransportChargesMethodOfPaymentCaption);
		builder.SetCaption(commonBag.InvoiceCurrExRateCalcEdit, _ => InvoiceHeaderDetailsCaptions.Instance.InvoiceCurrExRateCaption);

		builder.SetVisibility(euBag.AgreedPlaceCodeFindBox, i => i.AgreedPlaceCodeSupportAndVisible, d => d.JZ_IncoTermInfo);
		builder.SetVisibility(itBag.AgreedPlaceCodeDropEdit, i => !i.JobDeclaration.IsUCC6, d => d.JobDeclaration.JE_MessageTypeInfo, d => d.JobDeclaration.MessageVersionInfo);
		builder.SetVisibility(commonBag.AdditionalTermsTextBox, i => i.AdditionalTermsSupport, d => d.JZ_IncoTermInfo, d => d.JobDeclaration.JE_MessageTypeInfo, d => d.JobDeclaration.MessageVersionInfo);

		return builder.Build();
	}
}
