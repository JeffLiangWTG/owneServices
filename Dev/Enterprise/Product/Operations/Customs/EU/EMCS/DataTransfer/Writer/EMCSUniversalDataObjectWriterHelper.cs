using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;

namespace Enterprise.Customs.EU.EMCS.DataTransfer
{
	public class EMCSUniversalDataObjectWriterHelper : UniversalDataObjectWriterHelper
	{
		public EMCSUniversalDataObjectWriterHelper(BusinessObjectFactory factory, ZString countryCode)
			: base(factory, countryCode)
		{
		}

		protected override IUniversalCustomsDataObjectProvider GetUniversalCustomsDataObjectProvider(ZString countryCode)
		{
			return new EMCSUniversalCustomsDataObjectProvider();
		}
	}
}
