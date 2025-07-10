using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AURollAddInfoValidation : AutoAURollAddInfoValidation
	{
		public AURollAddInfoValidation(AutoAURollAddInfo parent) : base(parent)
		{
		}

		protected override void CheckZA_Roll()
		{
			base.CheckZA_Roll();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZA_RollInfo, Parent.Lookups.ClientRolls);
		}
	}
}
