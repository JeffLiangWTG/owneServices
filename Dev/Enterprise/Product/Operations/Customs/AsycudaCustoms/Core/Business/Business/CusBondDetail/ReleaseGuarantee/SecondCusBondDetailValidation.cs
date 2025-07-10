using CargoWise.EntityFramework;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class SecondCusBondDetailValidation : CommonCusBondDetailValidation
	{
		public SecondCusBondDetailValidation(SecondCusBondDetail parent) : base(parent)
		{
		}

		protected override void CheckPW_BondAmount()
		{
			base.CheckPW_BondAmount();
			MandatoryValidation.CheckEntered(Parent.PW_BondAmountInfo);
		}

		protected override void CheckPW_BondNumber2()
		{
			base.CheckPW_BondNumber2();
			MandatoryValidation.CheckEntered(Parent.PW_BondNumber2Info);
		}

		protected override void CheckPW_BondEffectiveDate()
		{
			base.CheckPW_BondEffectiveDate();
			MandatoryValidation.CheckEntered(Parent.PW_BondEffectiveDateInfo);
		}

		protected new SecondCusBondDetail Parent => (SecondCusBondDetail)base.Parent;
	}
}
