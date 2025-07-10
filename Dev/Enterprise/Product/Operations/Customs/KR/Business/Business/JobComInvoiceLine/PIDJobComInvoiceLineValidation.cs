using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class PIDJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public PIDJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckJI_Description()
		{
		}

		protected override void CheckJI_NDescription()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_NDescriptionInfo);
		}

		protected override void CheckJI_InvoiceQuantity()
		{
			TypeValidation.CheckValidDecimal(Parent.JI_InvoiceQuantityInfo, 10, 0);
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_InvoiceQuantityInfo);
		}

		protected override void CheckJI_InvoiceUQ()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_InvoiceUQInfo);
		}

		protected override void CheckJI_CustomsQuantity()
		{
			TypeValidation.CheckValidDecimal(Parent.JI_CustomsQuantityInfo, 2, 0);
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_CustomsQuantityInfo);
		}

		protected override void CheckJI_LinePrice()
		{
			TypeValidation.CheckValidDecimal(Parent.JI_LinePriceInfo, 16, 2);
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_LinePriceInfo);
		}

		protected override void CheckJI_ProductTypeCode()
		{
			base.CheckJI_ProductTypeCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_ProductTypeCodeInfo);
		}

		protected override bool IsTariffMandatory => false;
	}
}
