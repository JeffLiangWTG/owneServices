using System;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class MessagingProvider : ASYCUDA.Business.MessagingProvider
	{
		public override ASYCUDA.Business.MessageStatusProvider MessageStatusProvider => new MessageStatusProvider();

		public override Type GetAsycudaEDIMessageType() => typeof(MXMessage);
	}
}

