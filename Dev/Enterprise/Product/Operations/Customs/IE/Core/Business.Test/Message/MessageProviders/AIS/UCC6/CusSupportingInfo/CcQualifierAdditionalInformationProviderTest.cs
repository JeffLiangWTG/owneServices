using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class CcQualifierAdditionalInformationProviderTest : DataProviderTestCase<CcQualifierAdditionalInformationProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CcQualifierAdditionalInformationProvider(null));
		}

		public void TestProviderInterface()
		{
			Assert(Provider is ICcQualifierAdditionalInformation);
		}

		public void TestCode()
		{
			AssertEquals("123", Provider.Code);
		}

		public void TestText()
		{
			AssertEquals("REFNO1", Provider.Text);
		}

		public void TestCcQualifier()
		{
			// node should not be populated
			AssertEquals(null, Provider.CcQualifier);
		}

		protected override CcQualifierAdditionalInformationProvider GetProvider() => new CcQualifierAdditionalInformationProvider(additionalInfo);

		protected override void SetUp()
		{
			additionalInfo = Factory.New<AdditionalInfo>();
			additionalInfo.CSI_Code = "123";
			additionalInfo.CSI_Description = "REFNO1";
		}
		AdditionalInfo additionalInfo;
	}
}
