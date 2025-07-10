using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRTreatmentRatePeriodCharacteristicCollection))]
	sealed class CMRTreatmentRatePeriodCharacteristicCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIsThisGSTExempt()
		{
			var testCollection = new CMRTreatmentRatePeriodCharacteristicCollection(Factory);
			var testTreatment = testCollection.AddNew();
			testTreatment.TR_CharacteristicCode = ZShort.Parse(CharacteristicCodeList.Codes.NonTaxableImports);
			testTreatment.TR_TreatmentRatePeriodSnapshotPreferenceSchemeType = "AAA";
			testTreatment.TR_TreatmentRatePeriodSnapshotRateNumber = "111";
			testTreatment.TR_TreatmentRatePeriodSnapshotCode = "000";

			AssertEquals("IsThisGSTExempt", true, testCollection.IsThisGSTExempt("000", "111", "AAA"));
			AssertEquals("IsThisGSTExempt", false, testCollection.IsThisGSTExempt("000", "111", ""));
			AssertEquals("IsThisGSTExempt", false, testCollection.IsThisGSTExempt("001", "111", "AAA"));
			AssertEquals("IsThisGSTExempt", false, testCollection.IsThisGSTExempt("000", "110", "AAA"));
			AssertEquals("IsThisGSTExempt", false, testCollection.IsThisGSTExempt("000", "111", "AAB"));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CMRTreatmentRatePeriodCharacteristicCollection(Factory);
	}
}
