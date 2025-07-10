using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsArrivalHeaderContainerValidation : Customs.Business.CusInBondContainerValidation
	{
		public NctsArrivalHeaderContainerValidation(NctsArrivalHeaderContainer parent) : base(parent)
		{
		}

		public new NctsArrivalHeaderContainer Parent => (NctsArrivalHeaderContainer)base.Parent;

		protected NctsHeader NctsArrival => Parent.NctsArrival;

		protected ValidationRuleConfiguration ValidationRuleConfiguration => NctsArrival?.Configuration.ValidationRuleConfiguration ?? new ValidationRuleConfiguration();

		protected override void CheckBC_ContainerNum()
		{
			base.CheckBC_ContainerNum();

			var parent = Parent;
			if (parent.IsContainerised)
			{
				ContainerNumberValidation.WarnIfInvalid(parent.BC_ContainerNumInfo);
			}
		}

		protected override void CheckBC_UnloadedState()
		{
			base.CheckBC_UnloadedState();
			if (!NctsHelper.UnloadedStateInitiallyNew(Parent.BC_UnloadedStateInfo))
			{
				ListValidation.ErrorIfInvalidCode(Parent.BC_UnloadedStateInfo);
			}
		}
	}
}
