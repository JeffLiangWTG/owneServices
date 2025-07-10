
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class InvoiceChargeValidation : Customs.Business.BaseInvoiceChargeValidation
	{
		public InvoiceChargeValidation(InvoiceCharge invoiceCharge)
			: base(invoiceCharge)
		{
		}

		public InvoiceCharge InvoiceCharge
		{
			get { return Parent; }
		}

		protected new InvoiceCharge Parent
		{
			get { return (InvoiceCharge)base.Parent; }
		}

		protected override Customs.Business.ExternalMessageValidation GetNewExternalMessageValidation()
		{
			return new ExternalMessageValidation(Parent);
		}

		protected override bool IsCIFComponentUsed
		{
			get { return true; }
		}

		protected override void CheckJ7_Amount()
		{
			base.CheckJ7_Amount();

			if ((double)(InvoiceCharge.J7_Amount * InvoiceCharge.J7_ExchangeRate) > 99999.00 && InvoiceCharge.J7_ChargeType == CAChargeTypeList.Codes.OverseasFreight)
			{
				InvoiceCharge.J7_AmountInfo.AddMessageError(Res.GetString("6d2ac628-827d-4fb6-8f4e-4cd9ddfbfdba", "Amount for overseas freight may not exceed CAD 99,999"));
			}
		}

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();
			ValidateJ7_Amount();
		}
	}
}
