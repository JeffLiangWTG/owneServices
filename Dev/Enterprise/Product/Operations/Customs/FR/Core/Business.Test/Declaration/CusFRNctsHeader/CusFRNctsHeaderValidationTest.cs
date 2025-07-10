using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class CusFRNctsHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDetailedDepartureStatusCode()
		{
			nctsHeader.DetailedDepartureStatusCode = "ZZZ";
			AssertHasMessageErrorContaining(nctsHeader.DetailedDepartureStatusCodeInfo, ListValidation.InvalidCodeMessageError);
			nctsHeader.DetailedDepartureStatusCode = NctsDetailedStatusList.Codes.AmendmentAccepted;
			AssertNoMessageErrorContaining(nctsHeader.DetailedDepartureStatusCodeInfo, ListValidation.InvalidCodeMessageError);
			nctsHeader.DetailedDepartureStatusCode = "";
			AssertNoMessageErrorContaining(nctsHeader.DetailedDepartureStatusCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCFN_NatureOfSeals()
		{
			AssertNoMessageErrorContaining(nctsHeader.FRNctsHeader.CFN_NatureOfSealsInfo, ListValidation.InvalidCodeMessageError);
			nctsHeader.FRNctsHeader.CFN_NatureOfSeals = "A";
			AssertHasMessageErrorContaining(nctsHeader.FRNctsHeader.CFN_NatureOfSealsInfo, ListValidation.InvalidCodeMessageError);
			nctsHeader.FRNctsHeader.CFN_NatureOfSeals = "";
			AssertHasMessageErrorContaining(nctsHeader.FRNctsHeader.CFN_NatureOfSealsInfo, "You have not entered a Seals Nature");

			nctsHeader.FRNctsHeader.CFN_NatureOfSeals = NatureOfSealsList.Codes.NS1;
			AssertNoMessageErrorContaining(nctsHeader.FRNctsHeader.CFN_NatureOfSealsInfo, "You have not entered a Seals Nature");
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		}
		NctsHeader nctsHeader;
	}
}
