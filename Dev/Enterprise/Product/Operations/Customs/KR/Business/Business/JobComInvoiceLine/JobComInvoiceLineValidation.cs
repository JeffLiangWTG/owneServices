using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class JobComInvoiceLineValidation : AutoKRJobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCustomsUnitPrice();
		}

		public void ValidateCustomsUnitPrice()
		{
			ValidateCalculatedProperty(Parent.CustomsUnitPriceInfo);
		}

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();

			ListValidation.MessageErrorIfInvalidCode(Parent.JI_InvoiceUQInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		protected void CheckCustomsUnitPrice()
		{
			if (Parent.HasHSRequiringInvQuantityInCustomsUQ && Parent.JI_InvoiceUQ != Parent.JI_CustomsUnitQty)
			{
				Parent.CustomsUnitPriceInfo.AddWarning(Res.GetString("3FBAA5DB-CB43-4C51-9B8D-C534C909CF15", "Due to the Customs requirement, the Customs Quantity, UQ, and Customs Unit Price are to be sent to the Customs instead of the Invoice Quantity, UQ, and its Unit Price."));
			}
		}

		protected override void CheckJI_NetWeightUQ()
		{
			base.CheckJI_NetWeightUQ();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_NetWeightUQInfo);
		}

		protected override void CheckJI_WeightUQ()
		{
			base.CheckJI_WeightUQ();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_WeightUQInfo);
		}

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_DescriptionInfo);
		}
		protected override void CheckJI_ZZF_NKTaxType()
		{
			base.CheckJI_ZZF_NKTaxType();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_ZZF_NKTaxTypeInfo, Parent.Lookups.TaxOrFeeCodeList);
		}
	}
}
