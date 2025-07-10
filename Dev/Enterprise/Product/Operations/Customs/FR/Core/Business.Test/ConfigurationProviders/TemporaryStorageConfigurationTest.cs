using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.CusTempStorage;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Testing.ConfigurationProviders
{
	[TestedType(typeof(TemporaryStorageConfiguration))]
	class TemporaryStorageConfigurationTest : EU.Business.CusTempStorage.Testing.TemporaryStorageConfigurationAbstractTest<TemporaryStorageConfiguration>
	{
		public override void TestPreviousDocumentConfiguration()
		{
			AssertType<TemporaryStoragePreviousDocumentConfiguration>(configuration.PreviousDocumentConfiguration);
		}

		public override void TestSupportingDocumentConfiguration()
		{
			AssertType<TemporaryStorageSupportingDocumentConfiguration>(configuration.SupportingDocumentConfiguration);
		}

		public override void TestTemporaryStorageHeaderValidationDecider()
		{
			AssertType<FRTemporaryStorageHeaderValidationDecider>(configuration.GetValidationDecider());
		}

		public override void TestBillConfiguration()
		{
			AssertType<TemporaryStorageBillConfiguration>(configuration.BillConfiguration);
		}

		public override void TestPackedItemConfiguration()
		{
			AssertType<TemporaryStoragePackedItemConfiguration>(configuration.PackedItemConfiguration);
		}

		protected override bool ExpectedSupportLRNGeneration => true;

		protected override bool ExpectedSupportAgentDefaulting => true;

		protected override string TemporaryStorageHeaderDocumentWrapperClass => "Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage.TemporaryStorageHeaderWrapper";
	}
}
