using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestsSubclassesOf(typeof(TemporaryStorageMessageSendingConfiguration))]
	public abstract class TemporaryStorageMessageSendingConfigurationAbstractTest<T> : TestCaseWithFactory where T : TemporaryStorageMessageSendingConfiguration, new()
	{
		public abstract void TestGetNewMessageSendingObjectParent();

		public abstract void TestGetNewMessageSendingObject();

		protected override void SetUp()
		{
			base.SetUp();
			configuration = new T();
		}
		protected T configuration;
	}

	[TestedType(typeof(TemporaryStorageMessageSendingConfiguration))]
	sealed class TemporaryStorageMessageSendingConfigurationBaseOnlyTest : TemporaryStorageMessageSendingConfigurationAbstractTest<TemporaryStorageMessageSendingConfiguration>
	{
		[ExpectNoExceptions]
		public override void TestGetNewMessageSendingObjectParent()
		{
			NUnit.Framework.Assert.That(configuration.GetNewMessageSendingObjectParent(Factory.New<TemporaryStorageHeader>()), NUnit.Framework.Is.TypeOf<TemporaryStorageMessageSendingObjectParent<TemporaryStorageMessageSendingObject, TemporaryStorageHeader>>());
		}

		[ExpectNoExceptions]
		public override void TestGetNewMessageSendingObject()
		{
			NUnit.Framework.Assert.That(configuration.GetNewMessageSendingObject(Factory.New<TemporaryStorageHeader>()), NUnit.Framework.Is.TypeOf<TemporaryStorageMessageSendingObject>());
		}
	}
}
