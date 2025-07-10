using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRTariffRatePeriodCharacteristicCollection))]
	sealed class CMRTariffRatePeriodCharacteristicCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIsThisGSTExempt()
		{
			var testCollection = new CMRTariffRatePeriodCharacteristicCollection(Factory);
			var testTariff = testCollection.AddNew();
			testTariff.TH_CharacteristicCode = ZShort.Parse(CharacteristicCodeList.Codes.NonTaxableImports);
			testTariff.TH_TariffRatePeriodSnapshotPreferenceSchemeType = "AAA";
			testTariff.TH_TariffRatePeriodSnapshotRateNumber = "111";
			testTariff.TH_TariffRatePeriodSnapshotTariffClassificationNumber = "0000";

			AssertEquals("IsThisGSTExempt", true, testCollection.IsThisGSTExempt("0000", "111", "AAA"));
			AssertEquals("IsThisGSTExempt", false, testCollection.IsThisGSTExempt("0000", "111", ""));
			AssertEquals("IsThisGSTExempt", false, testCollection.IsThisGSTExempt("0001", "111", "AAA"));
			AssertEquals("IsThisGSTExempt", false, testCollection.IsThisGSTExempt("0000", "110", "AAA"));
			AssertEquals("IsThisGSTExempt", false, testCollection.IsThisGSTExempt("0000", "111", "AAB"));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CMRTariffRatePeriodCharacteristicCollection(Factory);
	}
}
