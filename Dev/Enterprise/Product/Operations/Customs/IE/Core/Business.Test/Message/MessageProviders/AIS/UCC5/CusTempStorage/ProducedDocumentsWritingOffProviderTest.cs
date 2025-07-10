using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class ProducedDocumentsWritingOffProviderTest : DataProviderTestCase<ProducedDocumentsWritingOffProvider>
	{
		public void TestProviderIsIProducedDocumentsWritingOff()
		{
			Assert(Provider is IIdType);
		}

		public void TestType()
		{
			AssertEquals("123", Provider.Type);
		}

		public void TestId()
		{
			AssertEquals("REFNO1", Provider.Id);
		}

		protected override ProducedDocumentsWritingOffProvider GetProvider() => ProducedDocumentsWritingOffProvider.New(additionalInfo);

		protected override void SetUp()
		{
			additionalInfo = Factory.New<AdditionalInfo>();
			additionalInfo.CSI_Code = "123";
			additionalInfo.CSI_ReferenceNumber = "REFNO1";
		}
		AdditionalInfo additionalInfo;
	}
}
