namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobDeclarationMessageSendingConfiguration
	{
		public bool ShouldCheckCanSend => ShouldCheckCanSendCore;

		protected virtual bool ShouldCheckCanSendCore => true;
	}
}
