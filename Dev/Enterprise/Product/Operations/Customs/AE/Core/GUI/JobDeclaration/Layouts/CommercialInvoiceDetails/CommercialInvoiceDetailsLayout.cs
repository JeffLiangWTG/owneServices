using Enterprise.Customs.AE.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI;

public sealed class CommercialInvoiceDetailsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public CommercialInvoiceDetailsLayout()
	{
		Layout = CreateCommercialInvoiceDetailsLayout();
	}

	PanelLayout CreateCommercialInvoiceDetailsLayout()
	{
		var builder = new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
		var commonBag = builder.CommonBag;
		var aeBag = CommercialInvoiceDetailsControlBag.Instance;
		builder.AddControlBag(aeBag);

		builder.AddColumn();
		builder.Add(commonBag.InvoiceNumberTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.GroupInvoiceDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.IncoTermsUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.IncoTermPlaceTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(aeBag.PaymentMethodDropEdit, ControlWidthClass.Auto);
		builder.Add(aeBag.ValuationCodeDropEdit, ControlWidthClass.Auto);
		builder.Add(aeBag.TotNoOfInvPagesCalcEdit, ControlWidthClass.Auto);
		builder.Add(aeBag.InvoiceTypeDropEdit, ControlWidthClass.Auto);
		builder.Add(aeBag.AttestationNoTextBox, ControlWidthClass.Auto);

		return builder.Build();
	}
}
