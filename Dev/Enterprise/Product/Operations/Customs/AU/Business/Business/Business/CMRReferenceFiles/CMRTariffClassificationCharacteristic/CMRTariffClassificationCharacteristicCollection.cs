
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTariffClassificationCharacteristicCollection : BusinessObjectCollection<CMRTariffClassificationCharacteristic>
	{
		public CMRTariffClassificationCharacteristicCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CMRTariffClassificationCharacteristicCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
