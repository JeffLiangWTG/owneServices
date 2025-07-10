using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Manifest.Business
{
	public class AsycudaTaxValidation : ASYCUDA.Business.AsycudaTaxValidation
	{
		public AsycudaTaxValidation(AsycudaTax parent)
			: base(parent)
		{
		}

		protected override void CheckAET_ChargeType()
		{
			base.CheckAET_ChargeType();

			ListValidation.MessageErrorIfInvalidCode(Parent.AET_ChargeTypeInfo);

			var duplicatedTax = ((AsycudaBill)Parent.Bill)?.AsycudaTaxes.Find(t => t.PK != Parent.PK && t.AET_ChargeType == Parent.AET_ChargeType).Any() ?? false;
			if (duplicatedTax)
			{
				Parent.AET_ChargeTypeInfo.AddMessageError(Res.GetString("8B3B5DF3-862B-4FD7-BE97-6E92EC77F24A", "The Charge Code cannot be repeated."));
			}
		}

		protected override void CheckAET_MethodOfPayment()
		{
			base.CheckAET_MethodOfPayment();
			ListValidation.MessageErrorIfInvalidCode(Parent.AET_MethodOfPaymentInfo);
		}

		protected override void CheckAET_RX_NKCurrency()
		{
			base.CheckAET_RX_NKCurrency();
			ListValidation.MessageErrorIfInvalidCode(Parent.AET_RX_NKCurrencyInfo);
		}

		protected override void CheckAET_MethodOfCalculation()
		{
		}

		protected new AsycudaTax Parent => (AsycudaTax)base.Parent;
	}
}
