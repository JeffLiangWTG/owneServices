using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestsSubclassesOf(typeof(TemporaryStorageBillConfiguration))]
	public abstract class TemporaryStorageBillConfigurationAbstractTest<T> : TestCaseWithFactory where T : TemporaryStorageBillConfiguration, new()
	{
		public abstract void TestGetValidationDecider();

		protected override void SetUp()
		{
			base.SetUp();
			configuration = new T();
		}
		protected T configuration;
	}

	[TestedType(typeof(TemporaryStorageBillConfiguration))]
	sealed class TemporaryStorageBillConfigurationTest : TemporaryStorageBillConfigurationAbstractTest<TemporaryStorageBillConfiguration>
	{
		[ExpectNoExceptions]
		public override void TestGetValidationDecider()
		{
			NUnit.Framework.Assert.That(configuration.GetValidationDecider(), NUnit.Framework.Is.TypeOf<TemporaryStorageBillValidationDecider>());
		}
	}
}
