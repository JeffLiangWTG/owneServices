using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestMessageSendingConfiguration))]
	public sealed class AsycudaManifestMessageSendingConfigurationTest : TestCaseWithFactory
	{
		public void TestGetNewMessageSendingObjectParent()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaManifestMessageSendingObjectParent>(configuration.GetNewMessageSendingObjectParent(header));
		}

		public void TestGetNewQueryMessageSendingObjectParent()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaManifestQueryMessageSendingObjectParent>(configuration.GetNewQueryMessageSendingObjectParent(header));
		}

		protected override void SetUp()
		{
			base.SetUp();
			configuration = new AsycudaManifestMessageSendingConfiguration();
		}

		AsycudaManifestMessageSendingConfiguration configuration;
	}
}
