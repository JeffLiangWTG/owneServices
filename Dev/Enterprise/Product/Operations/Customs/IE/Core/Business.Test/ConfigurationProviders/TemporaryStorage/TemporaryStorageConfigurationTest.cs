using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageConfiguration))]
	sealed class TemporaryStorageConfigurationTest : TemporaryStorageConfigurationAbstractTest<TemporaryStorageConfiguration>
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
			AssertType<TemporaryStorageHeaderValidationDecider>(configuration.GetValidationDecider());
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
