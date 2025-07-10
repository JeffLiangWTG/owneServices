using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	class ModeOfRepresentationCalculatorTest : TestCaseWithFactory
	{
		public void TestGetModeOfRepresentation()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			var declarant = Factory.New<OrgHeader>();
			var principal = Factory.New<OrgHeader>();

			var calculator = nctsHeader.ModeOfRepresentationCalculator;

			nctsHeader.Declarant.E2_OA_Address = declarant.MainAddress.PK;
			nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;

			nctsHeader.Declarant.Address.OA_OH = Factory.New<OrgHeader>().PK;
			nctsHeader.Principal.Address.OA_OH = Factory.New<OrgHeader>().PK;

			var cusAccount1 = Factory.New<OrgCusAccount>();
			cusAccount1.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAccount1.CZ_OH = nctsHeader.Declarant.OrganisationPK;
			cusAccount1.CZ_Code = OrgCusAccountCodeList.Codes.DTA;
			cusAccount1.CZ_Account = "123";

			var cusAccount2 = Factory.New<OrgCusAccount>();
			cusAccount2.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAccount2.CZ_OH = nctsHeader.Declarant.OrganisationPK;
			cusAccount2.CZ_Code = OrgCusAccountCodeList.Codes.DTA;
			cusAccount2.CZ_Account = "456";

			var cusAccount3 = Factory.New<OrgCusAccount>();
			cusAccount3.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAccount3.CZ_OH = nctsHeader.Principal.OrganisationPK;
			cusAccount3.CZ_Code = OrgCusAccountCodeList.Codes.DTA;
			cusAccount3.CZ_Account = "ABC";

			var cusAccount4 = Factory.New<OrgCusAccount>();
			cusAccount4.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAccount4.CZ_OH = nctsHeader.Principal.OrganisationPK;
			cusAccount4.CZ_Code = OrgCusAccountCodeList.Codes.DTA;
			cusAccount4.CZ_Account = "DEF";

			AssertEquals(EU.NCTS.Business.NctsConstants.ModeOfRepresentation.Codes.Two, calculator.GetModeOfRepresentation());

			var cusAccount5 = Factory.New<OrgCusAccount>();
			cusAccount5.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAccount5.CZ_OH = nctsHeader.Principal.OrganisationPK;
			cusAccount5.CZ_Code = OrgCusAccountCodeList.Codes.DTA;
			cusAccount5.CZ_Account = "123";

			AssertEquals(EU.NCTS.Business.NctsConstants.ModeOfRepresentation.Codes.One, calculator.GetModeOfRepresentation());

			cusAccount1.CZ_Account = "789";
			AssertEquals(EU.NCTS.Business.NctsConstants.ModeOfRepresentation.Codes.Two, calculator.GetModeOfRepresentation());
		}
	}
}
