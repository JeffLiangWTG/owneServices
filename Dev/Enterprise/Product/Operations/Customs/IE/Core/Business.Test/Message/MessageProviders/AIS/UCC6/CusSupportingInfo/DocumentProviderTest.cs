using System;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class DocumentProviderTest : DataProviderTestCase<DocumentProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new DocumentProvider(null));
		}

		public void TestProviderInterface()
		{
			Assert(Provider is IDocument);
		}

		public void TestType()
		{
			AssertEquals("123", Provider.Type);
		}

		public void TestReference()
		{
			AssertEquals("REFNO1", Provider.Reference);
		}

		protected override DocumentProvider GetProvider() => new DocumentProvider(additionalInfo);

		protected override void SetUp()
		{
			additionalInfo = Factory.New<AdditionalInfo>();
			additionalInfo.CSI_Code = "123";
			additionalInfo.CSI_ReferenceNumber = "REFNO1";
		}
		AdditionalInfo additionalInfo;
	}
}
