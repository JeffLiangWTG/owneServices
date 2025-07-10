using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRTreatmentRatePeriodCharacteristic))]
	sealed class CMRTreatmentRatePeriodCharacteristicTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIGSTExempt()
		{
			var characteristic = CMRTreatmentRatePeriodCharacteristic.New(Factory);
			characteristic.TR_CharacteristicCode = (short)1;
			characteristic.TR_TreatmentRatePeriodSnapshotCode = "000";
			characteristic.TR_TreatmentRatePeriodSnapshotRateNumber = "111";
			characteristic.TR_TreatmentRatePeriodSnapshotPreferenceSchemeType = "AAA";

			AssertEquals("IGSTExempt.CharacteristicCode", (short)1, ((IGSTExempt)characteristic).CharacteristicCode);
			AssertEquals("IGSTExempt.RateCode", "000", ((IGSTExempt)characteristic).RateCode);
			AssertEquals("IGSTExempt.RateNumber", "111", ((IGSTExempt)characteristic).RateNumber);
			AssertEquals("IGSTExempt.PrefScheme", "AAA", ((IGSTExempt)characteristic).PrefScheme);
		}

		protected override BusinessObject GetNewBusinessObject() => CMRTreatmentRatePeriodCharacteristic.New(Factory);
	}
}
