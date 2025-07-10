using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business
{
	public static class FRPermitHelper
	{
		public static ZString GetPermitAppIdForMessage(EDIMessage message) => message?.EM_MessageNum ?? ZString.Empty;
	}
}
