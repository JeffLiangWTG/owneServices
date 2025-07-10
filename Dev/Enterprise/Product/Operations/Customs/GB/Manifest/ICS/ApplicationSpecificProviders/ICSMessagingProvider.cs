using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.GB.ICS
{
	public class ICSMessagingProvider : MessagingProvider
	{
		public override MessageStatusProvider MessageStatusProvider => new ICSMessageStatusProvider();
	}
}
