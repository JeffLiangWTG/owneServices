using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportInvoiceApportionChargeValidation : EU.Business.Declaration.InvoiceApportionChargeValidation
	{
		public ImportInvoiceApportionChargeValidation(InvoiceApportionCharge invoiceApportionCharge) : base(invoiceApportionCharge)
		{
		}

		protected new InvoiceApportionCharge Parent => (InvoiceApportionCharge)base.Parent;

		protected override void ValidateExcludedCharges()
		{
			var parent = Parent;
			if (parent.Invoice is JobComInvoiceHeader invoice
				&& invoice.IncoTerm is ZString incoTerm
				&& !incoTerm.IsEmpty
				&& invoice.IncoTermAndChargeFactory is ImportIncoTermAndCustomsChargeFactory incoTermAndChargeFactory
				&& parent.ChargeCode is Customs.Common.ICustomsChargeCode chargeCode
				&& !incoTermAndChargeFactory.CanThisIncoTermHaveThisChargeForValidation(incoTerm, chargeCode)
				&& parent.J7_IsIncludedInITOT)
			{
				parent.J7_IsIncludedInITOTInfo.AddMessageError(Res.GetString("38704EBB-733C-4EB6-B716-27FA9920CDE4", "{0} cannot include this charge in lines.", parent.Invoice.JZ_IncoTerm));
			}
		}
	}
}
