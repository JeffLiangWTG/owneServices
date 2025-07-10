using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class ModeOfRepresentationCalculatorTest : TestCaseWithFactory
	{
		public void TestGetModeOfRepresentation()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var declarant = Factory.New<OrgHeader>();
			var principal = Factory.New<OrgHeader>();

			var calculator = nctsHeader.ModeOfRepresentationCalculator;
			AssertEquals(NctsConstants.ModeOfRepresentation.Codes.Blank, calculator.GetModeOfRepresentation());

			nctsHeader.Declarant.E2_OA_Address = declarant.MainAddress.PK;
			AssertEquals(NctsConstants.ModeOfRepresentation.Codes.Blank, calculator.GetModeOfRepresentation());

			nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;
			nctsHeader.Declarant.Address.OA_OH = ZGuid.Empty;
			nctsHeader.Principal.Address.OA_OH = ZGuid.Empty;
			AssertEquals(NctsConstants.ModeOfRepresentation.Codes.Blank, calculator.GetModeOfRepresentation());

			nctsHeader.Declarant.Address.OA_OH = Factory.New<OrgHeader>().PK;
			AssertEquals(NctsConstants.ModeOfRepresentation.Codes.Blank, calculator.GetModeOfRepresentation());

			nctsHeader.Principal.Address.OA_OH = Factory.New<OrgHeader>().PK;
			AssertEquals(NctsConstants.ModeOfRepresentation.Codes.Two, calculator.GetModeOfRepresentation());

			nctsHeader.Principal.Address.OA_OH = nctsHeader.Declarant.Address.OA_OH;
			AssertEquals(NctsConstants.ModeOfRepresentation.Codes.One, calculator.GetModeOfRepresentation());

			nctsHeader.Declarant.Address.OA_OH = Factory.New<OrgHeader>().PK;
			AssertEquals(NctsConstants.ModeOfRepresentation.Codes.Two, calculator.GetModeOfRepresentation());

			nctsHeader.Declarant.Address.OA_OH = ZGuid.Empty;
			AssertEquals(NctsConstants.ModeOfRepresentation.Codes.Blank, calculator.GetModeOfRepresentation());
		}
	}
}
