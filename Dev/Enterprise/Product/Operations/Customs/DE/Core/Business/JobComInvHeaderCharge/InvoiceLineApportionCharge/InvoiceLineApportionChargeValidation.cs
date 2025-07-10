using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class InvoiceLineApportionChargeValidation : EU.Business.Declaration.InvoiceLineApportionChargeValidation
	{
		public InvoiceLineApportionChargeValidation(InvoiceLineApportionCharge invoiceLineApportionCharge)
			: base(invoiceLineApportionCharge)
		{
		}

		protected override void CheckJ7_IsGSTApplicable()
		{
			if (!Parent.IsFreightChargeToEUBorderByAirInsideEU())
			{
				base.CheckJ7_IsGSTApplicable();
			}
		}

		protected override void CheckJ7_Amount()
		{
			base.CheckJ7_Amount();
			var parent = Parent;
			if (parent.Parent?.JobDeclaration?.IsImport ?? ZBool.False)
			{
				if (parent.J7_Amount == 0)
				{
					parent.J7_AmountInfo.AddMessageError(Res.GetString("658F1B51-4BCE-484F-A2FF-635C5B57239F", "Amount cannot be zero."));
				}
			}
		}
	}
}
