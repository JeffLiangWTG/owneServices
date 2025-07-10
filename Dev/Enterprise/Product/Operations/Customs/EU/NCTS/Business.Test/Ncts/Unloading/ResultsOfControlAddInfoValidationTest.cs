using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class UnloadedMeansOfTransportAtDepartureNationalityValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUnloadedMeansOfTransportAtDepartureNationality()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			ValidationTestHelper.AssertInvalidCodeMessageError(header.UnloadedMeansOfTransportAtDepartureNationalityInfo, "X~", header.Lookups.Countries[0].Code);
		}
	}
}
