using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(DummyRegistryBusinessObject))]
	public class DeclarationValidationTestHelper : ValidationTest
	{
		public static void AssertEORI(JobDocAddress address, OrgHeader orgHeader)
		{
			string error = "[NAT_020] The selected organization does not have an EORI.";
			var orgAddress = orgHeader.Addresses.AddNew();

			address.E2_OA_Address = orgAddress.PK;
			address.Validation.ValidateOrganisationPK();
			AssertHasMessageError(address.OrganisationPKInfo, error);

			var eori = orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.France);
			address.Validation.ValidateOrganisationPK();
			AssertNoMessageError(address.OrganisationPKInfo, error);
		}

		public static void AssertEORI(JobDeclaration declaration, OrgAddress address, ZPropertyInfo info, Action invokeValidation, string error)
		{
			using (var context = new Business.Testing.ConfigurationProviders.DeclarationValidationDeciderTestContext(declaration))
			{
				context.EnableRule(x => x.IsRuleNAT_020Active);
				invokeValidation();
				AssertHasMessageError(info, error);

				var eori = address.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.France);
				invokeValidation();
				AssertNoMessageError(info, error);

				context.DisableRule(x => x.IsRuleNAT_020Active);
				invokeValidation();
				AssertNoMessageError(info, error);
			}
		}

		public static void AssertEORIOrFullAddress(JobDocAddress address, OrgHeader orgHeader)
		{
			string error = "Please enter an organization with an EORI or enter full address details.";

			var orgAddress = orgHeader.Addresses.AddNew();

			address.E2_OA_Address = orgAddress.PK;
			address.Validation.ValidateOrganisationPK();
			AssertHasMessageError(address.OrganisationPKInfo, error);

			var eori = orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.France);
			address.Validation.ValidateOrganisationPK();
			AssertNoMessageError(address.OrganisationPKInfo, error);

			orgHeader.CustomsCodes.Remove(eori);

			orgHeader.OH_FullName = "FullName";
			address.Validation.ValidateOrganisationPK();
			AssertHasMessageError(address.OrganisationPKInfo, error);

			orgHeader.OH_RL_NKClosestPort = Core.Constants.CountryCodes.France;
			address.Validation.ValidateOrganisationPK();
			AssertHasMessageError(address.OrganisationPKInfo, error);

			orgAddress.OA_Address1 = "Address";
			address.Validation.ValidateOrganisationPK();
			AssertHasMessageError(address.OrganisationPKInfo, error);

			orgAddress.OA_PostCode = "PostCode";
			address.Validation.ValidateOrganisationPK();
			AssertHasMessageError(address.OrganisationPKInfo, error);

			orgAddress.OA_City = "City";
			address.Validation.ValidateOrganisationPK();
			AssertNoMessageError(address.OrganisationPKInfo, error);
		}

		public static void AssertEORIOrFullAddress(OrgAddress address, ZPropertyInfo info, Action invokeValidation)
		{
			string error = "Please enter an organization with an EORI or enter full address details.";

			invokeValidation();
			AssertHasMessageError(info, error);

			var eori = address.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.France);
			invokeValidation();
			AssertNoMessageError(info, error);

			address.Header.CustomsCodes.Remove(eori);

			address.Header.OH_FullName = "FullName";
			invokeValidation();
			AssertHasMessageError(info, error);

			address.Header.OH_RL_NKClosestPort = Core.Constants.CountryCodes.France;
			invokeValidation();
			AssertHasMessageError(info, error);

			address.OA_Address1 = "Address";
			invokeValidation();
			AssertHasMessageError(info, error);

			address.OA_PostCode = "PostCode";
			invokeValidation();
			AssertHasMessageError(info, error);

			address.OA_City = "City";
			invokeValidation();
			AssertNoMessageError(info, error);
		}
	}
}
