namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsDepartureHeaderContainerPhase4Validation : EU.NCTS.Business.NctsDepartureHeaderContainerPhase4Validation
	{
		public NctsDepartureHeaderContainerPhase4Validation(NctsDepartureHeaderContainer parent) : base(parent)
		{
		}

		new NctsDepartureHeaderContainer Parent => (NctsDepartureHeaderContainer)base.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			Parent.CheckContainerHasBeenAssigned();
		}
	}
}
