namespace Enterprise.Customs.GB.GVMS
{
	public class MessagingProvider : ASYCUDA.Business.MessagingProvider
	{
		public override ASYCUDA.Business.MessageStatusProvider MessageStatusProvider => new MessageStatusProvider();
	}
}
