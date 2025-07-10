using CargoWise.Types;

namespace Enterprise.Customs.Common.US.AMS
{
	public partial class MessageStatusListSTW
	{
		public static bool IsAccepted(ZString messagStatus)
		{
			return messagStatus == MessageStatusListSTW.Codes.Acceptance || messagStatus == MessageStatusListSTW.Codes.AcceptanceWithWarnings;
		}
	}
}
