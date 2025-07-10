using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRTariffRatePeriodCharacteristic))]
	sealed class CMRTariffRatePeriodCharacteristicTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIGSTExempt()
		{
			var characteristic = CMRTariffRatePeriodCharacteristic.New(Factory);
			characteristic.TH_CharacteristicCode = (short)1;
			characteristic.TH_TariffRatePeriodSnapshotTariffClassificationNumber = "0000";
			characteristic.TH_TariffRatePeriodSnapshotRateNumber = "111";
			characteristic.TH_TariffRatePeriodSnapshotPreferenceSchemeType = "AAA";

			AssertEquals("IGSTExempt.CharacteristicCode", (short)1, ((IGSTExempt)characteristic).CharacteristicCode);
			AssertEquals("IGSTExempt.RateCode", "0000", ((IGSTExempt)characteristic).RateCode);
			AssertEquals("IGSTExempt.RateNumber", "111", ((IGSTExempt)characteristic).RateNumber);
			AssertEquals("IGSTExempt.PrefScheme", "AAA", ((IGSTExempt)characteristic).PrefScheme);
		}

		protected override BusinessObject GetNewBusinessObject() => CMRTariffRatePeriodCharacteristic.New(Factory);
	}
}
