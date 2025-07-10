using Enterprise.Accounting.Integration;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class ChargePosterBehavioursHelper
	{
		public static ChargePosterBehaviours GetChargePosterBehaviours(this NctsHeader nctsHeader)
		{
			// NCTS doesn't provide its own charges
			return ChargePosterBehaviours.None;
		}
	}
}
