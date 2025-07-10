using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SupplyChainSecurityOrganisationToUse))]
	sealed class SupplyChainSecurityOrganisationToUseTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestCodeAndDescriptionReadOnly()
		{
			var organisationToUse = new SupplyChainSecurityOrganisationToUse();
			AssertEquals(true, organisationToUse.CodeInfo.ReadOnly);
			AssertEquals(true, organisationToUse.DescriptionInfo.ReadOnly);
			AssertEquals(false, organisationToUse.ValidationCodeInfo.ReadOnly);
		}

		public void TestValidationCodeValidation()
		{
			BizObj.ValidationCode = "XYZ";
			AssertHasErrors("Invalid Validation Code", BizObj.ValidationCodeInfo);

			BizObj.ValidationCode = "";
			AssertHasErrors("Empty Validation Code", BizObj.ValidationCodeInfo);

			BizObj.ValidationCode = "YES";
			AssertNoErrors("Valid Code = YES", BizObj.ValidationCodeInfo);

			BizObj.ValidationCode = "NO";
			AssertNoErrors("Valid Code = NO", BizObj.ValidationCodeInfo);

			BizObj.ValidationCode = "WARN";
			AssertNoErrors("Valid Code = WARN", BizObj.ValidationCodeInfo);
		}

		public void TestValidationCodeValidationByCountry()
		{
			AssertValidationCodeByCountry(Core.Constants.CountryCodes.Australia);
			AssertValidationCodeByCountry(Core.Constants.CountryCodes.HongKong);
			AssertValidationCodeByCountry(Core.Constants.CountryCodes.EuropeanUnion);
			AssertValidationCodeByCountry(Core.Constants.CountryCodes.SouthAfrica);
			AssertValidationCodeByCountry(ZString.Empty);
		}

		void AssertValidationCodeByCountry(ZString countryCode)
		{
			var errorMessage = "Either Consignor or Local Client must be set to Yes/Warning.";

			var organisationToUseCollection = new SupplyChainSecurityOrganisationToUseCollection(countryCode);
			var validationCode1 = organisationToUseCollection.Add("CON", SupplyChainSecurityOrganisationTypes.GetDescription(SupplyChainSecurityOrganisationTypes.Consignor), SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes);
			var validationCode2 = organisationToUseCollection.Add("LOC", SupplyChainSecurityOrganisationTypes.GetDescription(SupplyChainSecurityOrganisationTypes.LocalClient), SupplyChainSecurityOrganisationToUse.ValidationCodes.No);
			var validationCode3 = organisationToUseCollection.Add("TRS", SupplyChainSecurityOrganisationTypes.GetDescription(SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany), SupplyChainSecurityOrganisationToUse.ValidationCodes.No);

			AssertNoErrors("Precondition: Validation Code = YES, should not have errors", validationCode1.ValidationCodeInfo);
			AssertNoErrors("Precondition: Validation Code = NO, should not have errors", validationCode2.ValidationCodeInfo);
			AssertNoErrors("Precondition: Validation Code = NO, should not have errors", validationCode3.ValidationCodeInfo);

			validationCode1.ValidationCode = "NO";
			organisationToUseCollection.RunPreSaveValidation();

			AssertHasErrors("Validation Code = NO", validationCode1.ValidationCodeInfo);
			AssertHasErrors("Validation Code = NO", validationCode2.ValidationCodeInfo);
			AssertHasError(validationCode1.ValidationCodeInfo, errorMessage);
			AssertHasError(validationCode2.ValidationCodeInfo, errorMessage);
			AssertNoErrors("Validation Code = NO", validationCode3.ValidationCodeInfo);

			validationCode1.ValidationCode = "WARN";
			organisationToUseCollection.RunPreSaveValidation();
			AssertNoErrors("Validation Code = WARN", validationCode1.ValidationCodeInfo);
			AssertNoErrors("Validation Code = NO", validationCode2.ValidationCodeInfo);
			AssertNoErrors("Validation Code = NO", validationCode3.ValidationCodeInfo);

			validationCode2.ValidationCode = "WARN";
			organisationToUseCollection.RunPreSaveValidation();
			AssertNoErrors("Validation Code = WARN", validationCode1.ValidationCodeInfo);
			AssertNoErrors("Validation Code = WARN", validationCode2.ValidationCodeInfo);
			AssertNoErrors("Validation Code = NO", validationCode3.ValidationCodeInfo);

			validationCode1.ValidationCode = "YES";
			validationCode2.ValidationCode = "YES";
			organisationToUseCollection.RunPreSaveValidation();
			AssertNoErrors("Validation Code = YES", validationCode1.ValidationCodeInfo);
			AssertNoErrors("Validation Code = YES", validationCode2.ValidationCodeInfo);
			AssertNoErrors("Validation Code = NO", validationCode3.ValidationCodeInfo);
		}

		#region Implementation

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			SupplyChainSecurityOrganisationToUse result = new SupplyChainSecurityOrganisationToUse();

			result.Code = "XYZ";
			result.Description = (NoResString)"XYZ Organisation";
			result.ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		new SupplyChainSecurityOrganisationToUse BizObj
		{
			get { return (SupplyChainSecurityOrganisationToUse)base.BizObj; }
		}

		#endregion
	}
}
