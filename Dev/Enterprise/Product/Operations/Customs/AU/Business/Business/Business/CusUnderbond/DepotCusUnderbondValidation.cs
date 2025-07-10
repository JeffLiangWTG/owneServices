using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DepotCusUnderbondValidation : CusUnderbondValidation
	{
		public DepotCusUnderbondValidation(CusUnderbond underbond)
			: base(underbond)
		{
		}

		protected override void CheckC4_DestinationPremiseID()
		{
			MandatoryValidation.CheckEntered(Parent.C4_DestinationPremiseIDInfo);
		}
	}
}
