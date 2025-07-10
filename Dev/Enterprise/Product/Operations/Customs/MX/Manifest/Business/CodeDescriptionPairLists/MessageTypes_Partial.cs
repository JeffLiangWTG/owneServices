using CargoWise.Types;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public partial class MessageTypes
	{
		public static bool ShouldLinkRequestMessageByTrackingId(ZString messageType)
		{
			return messageType == Codes.MXF || messageType == Codes.MXG || messageType == MXMessageConstants.XER;
		}
	}
}
