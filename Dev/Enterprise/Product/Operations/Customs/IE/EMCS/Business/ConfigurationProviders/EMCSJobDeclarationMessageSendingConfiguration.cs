namespace Enterprise.Customs.IE.EMCS.Business
{
	sealed class EMCSJobDeclarationMessageSendingConfiguration : EU.EMCS.Business.EMCSJobDeclarationMessageSendingConfiguration
	{
		protected override bool ShouldCheckCanSendCore => false;
	}
}
