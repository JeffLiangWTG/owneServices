using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public class InvoiceDetailsLayoutBuilder<TEUInvoice> : ColumnLayoutBuilder<TEUInvoice, EU.GUI.InvoiceDetailsControlBag> where TEUInvoice : Business.Declaration.JobComInvoiceHeader
{
	public override EU.GUI.InvoiceDetailsControlBag CommonBag => EU.GUI.InvoiceDetailsControlBag.Instance;

	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();
		SetVisibility(CommercialInvoiceDetailsControlBag.IncoTermPlaceTextBox, invoice => !IsUCC6Other(invoice), invoice => invoice.JZ_IncoTermInfo);
		SetVisibility(CommonBag.IncoTermsAgreedPlaceLongTextControl, invoice => IsUCC6Other(invoice), invoice => invoice.JZ_IncoTermInfo);
	}

	protected override void SetDefaultCaptions()
	{
		base.SetDefaultCaptions();
		SetCaption(CommercialInvoiceDetailsControlBag.InvoiceCurrExRateCalcEdit, x => Res.GetData("43436AB8-E4FE-4E23-B888-6142FD7A83C4", "Exch. Rate", "Exchange Rate", "[UCC 4/15] Exchange Rate"));
		SetCaption(CommercialInvoiceDetailsControlBag.IncoTermsUserControl, x => Res.GetData("43436AB8-E4FE-4E23-B888-6142FD7A83C9", "[UCC 4/1] Incoterm"));
	}

	CommercialInvoiceDetailsControlBag CommercialInvoiceDetailsControlBag => CommercialInvoiceDetailsControlBag.Instance;

	bool IsUCC6Other(TEUInvoice invoice) => invoice.JobDeclaration.IsUCC6 && invoice.JZ_IncoTerm == Core.Constants.IncoTerms.Other;
}
