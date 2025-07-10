using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSAddInfoCusContainerValidation : EUEMCSAddInfoValidation
	{
		public EMCSAddInfoCusContainerValidation(EMCSAddInfoCusContainer parent)
			: base(parent)
		{
		}

		public new EMCSAddInfoCusContainer Parent => (EMCSAddInfoCusContainer)base.Parent;

		protected override void CheckZG_UnitCode()
		{
			base.CheckZG_UnitCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZG_UnitCodeInfo);
		}
	}
}
