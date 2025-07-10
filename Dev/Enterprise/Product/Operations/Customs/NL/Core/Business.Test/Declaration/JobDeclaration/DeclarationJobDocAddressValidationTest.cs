using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

class DeclarationJobDocAddressValidationTest : TestCaseWithFactory
{
	public void TestCheckOrgainisationPK_CarrierEUBorder_EoriAndTCUNumber()
	{
		var jobDocAddress = Factory.New<JobDocAddress>();
		jobDocAddress.E2_OA_Address = carrierAddress.PK;
		var propertyInfo = declaration.CarrierEUBorderDocAddress.OrganisationPKInfo;

		CombineAssertions(() =>
		{
			declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(propertyInfo, "[13 12 000 000] The carrier cannot be empty when it is a security declaration (EXS) .");

			declaration.ZG_TypeOfSecurity = ExportSecurityTypeList.Codes.EXS;
			declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(propertyInfo, "[13 12 000 000] The carrier cannot be empty when it is a security declaration (EXS) .");

			declaration.CarrierEUBorderDocAddress.E2_OA_Address = carrierAddress.PK;
			declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(propertyInfo, "EORI or TCU reference is required for Carrier EU Border.");

			var eori = carrier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Netherlands);
			declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrors(propertyInfo);
		});
	}

	public void TestCheckOrgainisationPK_CarrierEUBorderForUC9011()
	{
		var messageError = "[C9011] If security is not 0 THEN carrier is required as organization";
		var propertyInfo = declaration.CarrierEUBorderDocAddress.OrganisationPKInfo;

		CombineAssertions(() =>
		{
			foreach(var typeOfSecurity in new ExportSecurityTypeList())
			{
				declaration.CarrierEUBorderDocAddress.E2_OA_Address = ZGuid.Empty;
				declaration.ZG_TypeOfSecurity = typeOfSecurity.ToString();
				declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
				switch (declaration.ZG_TypeOfSecurity)
				{
					case ExportSecurityTypeList.Codes.NotUsed:
						AssertNoMessageErrorContaining($"No message error - Security {declaration.ZG_TypeOfSecurity}", propertyInfo, messageError);
						break;
					default:
						AssertHasMessageErrorContaining($"Message error - Security {declaration.ZG_TypeOfSecurity}", propertyInfo, messageError);
						declaration.CarrierEUBorderDocAddress.E2_OA_Address = carrierAddress.PK;
						declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining($"No message error - Security {declaration.ZG_TypeOfSecurity}", propertyInfo, messageError);
						break;
				}
			}
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		carrier = Factory.New<OrgHeader>();
		carrierAddress = carrier.MainAddress;
		carrierAddress.Address1 = "Street";
		carrierAddress.City = "City";
		carrierAddress.Postcode = "1234AB";
		carrierAddress.OA_RN_NKCountryCode = "NL";
	}

	JobDeclaration declaration;
	OrgAddress carrierAddress;
	OrgHeader carrier;
}
