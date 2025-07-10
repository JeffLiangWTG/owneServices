using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public class JobComInvoiceLineValidation : AutoILJobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_InvoiceQuantityInfo);
		}

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_InvoiceUQInfo);
		}

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CustomsQuantityInfo);
		}

		protected override void CheckJI_CustomsUnitQty()
		{
			base.CheckJI_CustomsUnitQty();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_CustomsUnitQtyInfo);
		}

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_LinePriceInfo);
		}

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CountryOfOriginInfo);
		}

		protected override void CheckJI_WeightUQ()
		{
			base.CheckJI_WeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_WeightUQInfo);
		}

		protected override void CheckJI_CustomsSecondUnitQty()
		{
			base.CheckJI_CustomsSecondUnitQty();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsSecondUnitQtyInfo);
		}

		protected override void CheckJI_CustomsThirdUnitQty()
		{
			base.CheckJI_CustomsThirdUnitQty();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsThirdUnitQtyInfo);
		}

		protected override void CheckJI_BondedWhsUnitQty()
		{
			base.CheckJI_BondedWhsUnitQty();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_BondedWhsUnitQtyInfo);
		}

		protected override void CheckJI_ZZF_NKTaxType()
		{
			base.CheckJI_ZZF_NKTaxType();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_ZZF_NKTaxTypeInfo);
		}
	}
}
