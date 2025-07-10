using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class AddInfoJobComInvoiceHeaderValidation : EU.Business.Declaration.AddInfoJobComInvoiceHeaderValidation
	{
		public AddInfoJobComInvoiceHeaderValidation(EU.Business.Declaration.AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		JobComInvoiceHeader InvoiceHeader => Parent.Parent as JobComInvoiceHeader;

		protected override void CheckZG_AgreedPlaceCode()
		{
			base.CheckZG_AgreedPlaceCode();

			var invoice = InvoiceHeader;
			if (invoice.IsImport && !invoice.IsStockMovement)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZG_AgreedPlaceCodeInfo);
			}
		}

		protected override void CheckZG_TransportChargesMethodOfPayment()
		{
			base.CheckZG_TransportChargesMethodOfPayment();
			if (InvoiceHeader.IsExport)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.ZG_TransportChargesMethodOfPaymentInfo);
			}
		}

		protected override void CheckZG_IncoTermDescription()
		{
			base.CheckZG_IncoTermDescription();

			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(InvoiceHeader.ZG_IncoTermDescriptionInfo, InvoiceHeader.JZ_IncoTermInfo, (ZString)Core.Constants.IncoTerms.Other);
		}
	}
}
