
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTariffRatePeriodCharacteristic : AutoCMRTariffRatePeriodCharacteristic, IGSTExempt
	{
		public CMRTariffRatePeriodCharacteristic(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRTariffRatePeriodCharacteristic New(BusinessObjectFactory factory)
		{
			return factory.New<CMRTariffRatePeriodCharacteristic>();
		}

		#region IGSTExempt Members

		ZShort IGSTExempt.CharacteristicCode
		{
			get { return TH_CharacteristicCode; }
		}

		ZString IGSTExempt.RateCode
		{
			get { return TH_TariffRatePeriodSnapshotTariffClassificationNumber; }
		}

		ZString IGSTExempt.RateNumber
		{
			get { return TH_TariffRatePeriodSnapshotRateNumber; }
		}

		ZString IGSTExempt.PrefScheme
		{
			get { return TH_TariffRatePeriodSnapshotPreferenceSchemeType; }
		}

		#endregion
	}
}
