using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.EU.EMCS.DataTransfer
{
	public class EMCSUniversalDataObjectReaderHelper : UniversalDataObjectReaderHelper
	{
		public EMCSUniversalDataObjectReaderHelper(UniversalObjectFactory factory, ZString targetCountryCode, ZString sourceCountryCode, string dataProviderForCodeMapping = null)
			: base(factory, targetCountryCode, sourceCountryCode, dataProviderForCodeMapping)
		{
		}

		protected override IUniversalCustomsDataObjectProvider GetUniversalCustomsDataObjectProvider(ZString countryCode)
		{
			return new EMCSUniversalCustomsDataObjectProvider();
		}
	}
}
