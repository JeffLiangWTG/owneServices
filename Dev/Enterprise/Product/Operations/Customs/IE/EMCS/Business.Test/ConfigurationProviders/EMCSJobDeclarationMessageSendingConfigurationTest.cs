using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSJobDeclarationMessageSendingConfiguration))]
	sealed class EMCSJobDeclarationMessageSendingConfigurationTest : EU.EMCS.Business.Testing.EMCSJobDeclarationMessageSendingConfigurationAbstractTest<EMCSJobDeclarationMessageSendingConfiguration>
	{
		protected override bool ShouldCheckCanSend_Expected => false;
	}
}
