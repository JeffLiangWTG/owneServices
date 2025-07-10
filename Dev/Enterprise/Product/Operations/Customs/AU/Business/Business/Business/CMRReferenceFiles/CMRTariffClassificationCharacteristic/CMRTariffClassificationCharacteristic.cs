using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTariffClassificationCharacteristic : AutoCMRTariffClassificationCharacteristic
	{
		public CMRTariffClassificationCharacteristic(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRTariffClassificationCharacteristic New(BusinessObjectFactory factory)
		{
			return factory.New<CMRTariffClassificationCharacteristic>();
		}

		public static bool IsTariffClassificationCharacteristic(BusinessObjectFactory factory, ZString tariff, params short[] characteristicCodes)
		{
			if (!tariff.IsEmpty)
			{
				var tariffNumber = tariff.Left(8);
				var tariffCharacteristicSearchQuery = new ZQuery(CMRTariffClassificationCharacteristicSchema.TC_TariffClassificationSnapshotTariffClassificationNumber, tariffNumber);
				tariffCharacteristicSearchQuery.AddToFilter(CMRTariffClassificationCharacteristicSchema.TC_CharacteristicCode, characteristicCodes);
				return factory.LoadTop1<CMRTariffClassificationCharacteristic>(tariffCharacteristicSearchQuery) != null;
			}
			return false;
		}

		public static bool IsTariffExciseEquivalentGoods(BusinessObjectFactory factory, ZString tariffNumber)
		{
			return factory.GetCachedValue("CMRTariffClassificationCharacteristic|IsTariffExciseEquivalentGoods|" + tariffNumber, () =>
			{
				short[] exciseEquivalentCharacteristicCodes =
				{
					ExciseEquivalentClassificationLaserInterestedCharacteristicCode,
					GoodsThatArePetroleumAndFuelRelatedCharacteristicCode,
					GoodsThatAreAlcoholRelatedCharacteristicCode,
					GoodsThatAreTobaccoRelatedCharacteristicCode
				};

				return IsTariffClassificationCharacteristic(factory, tariffNumber, exciseEquivalentCharacteristicCodes);
			});
		}

		const short ExciseEquivalentClassificationLaserInterestedCharacteristicCode = 29;
		const short GoodsThatArePetroleumAndFuelRelatedCharacteristicCode = 34;
		const short GoodsThatAreAlcoholRelatedCharacteristicCode = 35;
		const short GoodsThatAreTobaccoRelatedCharacteristicCode = 36;
	}
}
