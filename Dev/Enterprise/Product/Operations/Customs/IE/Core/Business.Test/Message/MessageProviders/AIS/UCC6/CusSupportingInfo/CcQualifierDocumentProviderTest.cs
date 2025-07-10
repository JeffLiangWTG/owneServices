using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class CcQualifierDocumentProviderTest : DataProviderTestCase<CcQualifierDocumentProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CcQualifierDocumentProvider(null));
		}

		public void TestProviderInterface()
		{
			Assert(Provider is ICcQualifierDocument);
		}

		public void TestType()
		{
			AssertEquals("123", Provider.Type);
		}

		public void TestReference()
		{
			AssertEquals("REFNO1", Provider.Reference);
		}
		public void TestCcQualifier()
		{
			// node should not be populated
			AssertEquals(null, Provider.CcQualifier);
		}

		protected override CcQualifierDocumentProvider GetProvider() => new CcQualifierDocumentProvider(additionalInfo);

		protected override void SetUp()
		{
			additionalInfo = Factory.New<AdditionalInfo>();
			additionalInfo.CSI_Code = "123";
			additionalInfo.CSI_ReferenceNumber = "REFNO1";
		}
		AdditionalInfo additionalInfo;
	}
}
