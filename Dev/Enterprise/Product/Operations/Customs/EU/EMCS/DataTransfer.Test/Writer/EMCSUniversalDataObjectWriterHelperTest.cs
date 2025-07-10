using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;

namespace Enterprise.Customs.EU.EMCS.DataTransfer.Testing
{
	class EMCSUniversalDataObjectWriterHelperTest : TestCaseWithFactory
	{
		public void TestGetUniversalCustomsDataObjectProvider()
		{
			var helper = new EMCSUniversalDataObjectWriterHelperForTest(Factory, ZString.Empty);
			AssertType<EMCSUniversalCustomsDataObjectProvider>(helper.GetUniversalCustomsDataObjectProvider_Exposed(ZString.Empty));
		}
	}

	class EMCSUniversalDataObjectWriterHelperForTest : EMCSUniversalDataObjectWriterHelper
	{
		public EMCSUniversalDataObjectWriterHelperForTest(BusinessObjectFactory factory, ZString countryCode) : base(factory, countryCode)
		{
		}

		public IUniversalCustomsDataObjectProvider GetUniversalCustomsDataObjectProvider_Exposed(ZString countryCode)
		{
			return base.GetUniversalCustomsDataObjectProvider(countryCode);
		}
	}
}
