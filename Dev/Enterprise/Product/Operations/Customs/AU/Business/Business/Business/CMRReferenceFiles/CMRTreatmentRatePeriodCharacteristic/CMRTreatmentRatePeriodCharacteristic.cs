
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTreatmentRatePeriodCharacteristic : AutoCMRTreatmentRatePeriodCharacteristic, IGSTExempt
	{
		public CMRTreatmentRatePeriodCharacteristic(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRTreatmentRatePeriodCharacteristic New(BusinessObjectFactory factory)
		{
			return factory.New<CMRTreatmentRatePeriodCharacteristic>();
		}

		#region IGSTExempt Members

		ZShort IGSTExempt.CharacteristicCode
		{
			get { return TR_CharacteristicCode; }
		}

		ZString IGSTExempt.RateCode
		{
			get { return TR_TreatmentRatePeriodSnapshotCode; }
		}

		ZString IGSTExempt.RateNumber
		{
			get { return TR_TreatmentRatePeriodSnapshotRateNumber; }
		}

		ZString IGSTExempt.PrefScheme
		{
			get { return TR_TreatmentRatePeriodSnapshotPreferenceSchemeType; }
		}

		#endregion
	}
}
