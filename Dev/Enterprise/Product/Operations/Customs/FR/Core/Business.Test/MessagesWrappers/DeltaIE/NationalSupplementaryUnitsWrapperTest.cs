using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class NationalSupplementaryUnitsWrapperTest : DataProviderTestCase<NationalSupplementaryUnitWrapper>
	{
		public void TestNationalMeasurementUnitAndQualifier()
		{
			AssertEquals("NationalMeasurementUnitAndQualifier should equal entry line third quantity unit.", "TNE1", Provider.NationalMeasurementUnitAndQualifier);
		}

		public void TestNationalSupplementaryUnits()
		{
			AssertEquals("NationalSupplementaryUnits should equal entry line the sum of all third quantities.", 22d, Provider.NationalSupplementaryUnits);
		}

		protected override NationalSupplementaryUnitWrapper GetProvider()
		{
			return NationalSupplementaryUnitWrapper.New(22d, "TNE1");
		}
	}
}
