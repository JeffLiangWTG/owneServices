using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class InvoiceDetailsLayoutBuilder<TEUInvoice> : ColumnLayoutBuilder<TEUInvoice, InvoiceDetailsControlBag> where TEUInvoice : Business.Declaration.JobComInvoiceHeader
	{
		public override InvoiceDetailsControlBag CommonBag => InvoiceDetailsControlBag.Instance;

		public CommercialInvoiceDetailsControlBag CommercialInvoiceDetailsControlBag => CommercialInvoiceDetailsControlBag.Instance;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(CommonBag.AgreedPlaceCodeFindBox, invoice => invoice.AgreedPlaceCodeSupportAndVisible, invoice => invoice.JZ_IncoTermInfo);
			SetVisibility(CommercialInvoiceDetailsControlBag.IncoTermPlaceTextBox, invoice => invoice.JobDeclaration.Configuration.InvoiceHeaderConfiguration.AgreedPlaceCodeSupport(invoice.JobDeclaration) && !IsUCC6OtherIncoTerm(invoice), invoice => invoice.JZ_IncoTermInfo);
			SetVisibility(CommonBag.IncoTermsAgreedPlaceLongTextControl, invoice => invoice.JobDeclaration.Configuration.InvoiceHeaderConfiguration.AgreedPlaceCodeSupport(invoice.JobDeclaration) && IsUCC6OtherIncoTerm(invoice), invoice => invoice.JZ_IncoTermInfo);
		}

		static bool IsUCC6OtherIncoTerm(TEUInvoice invoice) => invoice.JobDeclaration.IsUCC6 && invoice.JZ_IncoTerm == Core.Constants.IncoTerms.Other;
	}
}
