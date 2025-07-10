using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class FRNctsDepartureHeaderContainerPhase5Validation : NctsDepartureHeaderContainerPhase5Validation
	{
		public FRNctsDepartureHeaderContainerPhase5Validation(NctsDepartureHeaderContainer parent) : base(parent)
		{
		}

		protected override void CheckBC_RC()
		{
			base.CheckBC_RC();
			Parent.CheckInvalidRC();
		}

		protected override void CheckBC_Mode()
		{
			base.CheckBC_Mode();
			Parent.CheckInvalidMode();
		}

		protected override void CheckBC_ContainerNum()
		{
			base.CheckBC_ContainerNum();
			Parent.CheckMandatoryContainerNum();
		}
	}
}
