using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestsSubclassesOf(typeof(EMCSJobDeclarationMessageSendingConfiguration))]
	public abstract class EMCSJobDeclarationMessageSendingConfigurationAbstractTest<T> : TestCaseWithFactory
		where T : EMCSJobDeclarationMessageSendingConfiguration, new()
	{
		protected virtual bool ShouldCheckCanSend_Expected => true;

		public void TestShouldCheckCanSend()
		{
			AssertEquals(ShouldCheckCanSend_Expected, Configuration.ShouldCheckCanSend);
		}

		protected T Configuration => configuration ??= new T();
		T configuration;
	}

	[TestedType(typeof(EMCSJobDeclarationMessageSendingConfiguration))]
	sealed class EMCSJobDeclarationMessageSendingConfigurationBaseOnlyTest : EMCSJobDeclarationMessageSendingConfigurationAbstractTest<EMCSJobDeclarationMessageSendingConfiguration>
	{
	}
}
