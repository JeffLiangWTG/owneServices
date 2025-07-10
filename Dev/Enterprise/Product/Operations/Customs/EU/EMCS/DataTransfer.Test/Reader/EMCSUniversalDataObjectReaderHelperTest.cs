using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.EU.EMCS.DataTransfer.Testing
{
	class EMCSUniversalDataObjectReaderHelperTest : OrganizationAddressTestHelper
	{
		public void TestGetUniversalCustomsDataObjectProvider()
		{
			var helper = new EMCSUniversalDataObjectReaderHelperForTest(Factory, "UK", "ZA");
			AssertType<EMCSUniversalCustomsDataObjectProvider>(helper.GetUniversalCustomsDataObjectProvider_Exposed("UK"));
		}
	}

	class EMCSUniversalDataObjectReaderHelperForTest : EMCSUniversalDataObjectReaderHelper
	{
		public EMCSUniversalDataObjectReaderHelperForTest(UniversalObjectFactory factory, ZString targetCountryCode, ZString sourceCountryCode, string dataProviderForCodeMapping = null) : base(factory, targetCountryCode, sourceCountryCode, dataProviderForCodeMapping)
		{
		}

		public IUniversalCustomsDataObjectProvider GetUniversalCustomsDataObjectProvider_Exposed(ZString countryCode)
		{
			return base.GetUniversalCustomsDataObjectProvider(countryCode);
		}
	}
}
