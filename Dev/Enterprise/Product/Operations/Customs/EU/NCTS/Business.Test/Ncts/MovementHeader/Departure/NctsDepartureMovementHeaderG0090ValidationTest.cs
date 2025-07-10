using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using OrgCusCode = Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsDepartureMovementHeaderG0090ValidationTest : TestCaseWithFactory
	{
		public void TestValidConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NctsDepartureMovementHeaderG0090Validation(movementHeader: null));
		}

		public void TestPrincipalAndCarrier_SameEoriAndOrg()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddressHeader1 = orgHeader1.Addresses.AddNew();

			nctsHeader.Principal.E2_OA_Address = orgAddressHeader1.PK;
			departureMovementHeader.Carrier.E2_OA_Address = orgAddressHeader1.PK;

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0090Active)))
			{
				CombineAssertions("When RuleG0090 is Active", () =>
				{
					departureMovementHeader.Carrier.Validation.ValidateOrganisationPK();
					AssertHasWarningContaining("Phase5, same Eori & Org",
						departureMovementHeader.Carrier.OrganisationPKInfo, ExpectedErrorMessage);

					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					departureMovementHeader.Carrier.Validation.ValidateOrganisationPK();
					AssertNoWarningContaining("Phase4, same Eori & Org",
						departureMovementHeader.Carrier.OrganisationPKInfo, ExpectedErrorMessage);
				});
			}

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0090Active)))
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				departureMovementHeader.Carrier.Validation.ValidateOrganisationPK();
				AssertNoWarningContaining("When RuleG0090 is Disabled, Phase5, Principal and Carrier have same Eori & Org",
					departureMovementHeader.Carrier.OrganisationPKInfo, ExpectedErrorMessage);
			}
		}

		public void TestPrincipalAndCarrier_SameEoriDifferentOrg()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddressHeader1 = orgHeader1.Addresses.AddNew();

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddressHeader2 = orgHeader2.Addresses.AddNew();

			orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Italy);
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Italy);

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0090Active)))
			{
				nctsHeader.Principal.E2_OA_Address = orgAddressHeader1.PK;
				departureMovementHeader.Carrier.E2_OA_Address = orgAddressHeader2.PK;

				departureMovementHeader.Carrier.Validation.ValidateOrganisationPK();
				AssertHasWarningContaining("When RuleG0090 is Active, Principal and Carrier have same Eori but different Org",
					departureMovementHeader.Carrier.OrganisationPKInfo, ExpectedErrorMessage);
			}

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0090Active)))
			{
				departureMovementHeader.Carrier.Validation.ValidateOrganisationPK();
				AssertNoWarningContaining("When RuleG0090 is Disabled, Principal and Carrier have same Eori but different Org",
					departureMovementHeader.Carrier.OrganisationPKInfo, ExpectedErrorMessage);
			}
		}

		public void TestPrincipalAndCarrier_DifferentOrgsDifferentEoriSameTcu()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddressHeader1 = orgHeader1.Addresses.AddNew();

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddressHeader2 = orgHeader2.Addresses.AddNew();

			orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321", Core.Constants.CountryCodes.Germany);
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Germany);

			orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0090Active)))
			{
				nctsHeader.Principal.E2_OA_Address = orgAddressHeader1.PK;
				departureMovementHeader.Carrier.E2_OA_Address = orgAddressHeader2.PK;

				departureMovementHeader.Carrier.Validation.ValidateOrganisationPK();
				AssertHasWarningContaining("When RuleG0090 is Active, Principal and Carrier have different Eori and Org, but same TCU",
					departureMovementHeader.Carrier.OrganisationPKInfo, ExpectedErrorMessage);
			}

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0090Active)))
			{
				departureMovementHeader.Carrier.Validation.ValidateOrganisationPK();
				AssertNoWarningContaining("When RuleG0090 is Disabled, Principal and Carrier have different Eori and Org, but same TCU",
					departureMovementHeader.Carrier.OrganisationPKInfo, ExpectedErrorMessage);
			}
		}

		public void TestPrincipalAndCarrier_DifferentOrgsSameEoriDifferentTcu()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddressHeader1 = orgHeader1.Addresses.AddNew();

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddressHeader2 = orgHeader2.Addresses.AddNew();

			orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Germany);
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Germany);

			orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456654", Core.Constants.CountryCodes.UnitedStates);

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0090Active)))
			{
				nctsHeader.Principal.E2_OA_Address = orgAddressHeader1.PK;
				departureMovementHeader.Carrier.E2_OA_Address = orgAddressHeader2.PK;

				departureMovementHeader.Carrier.Validation.ValidateOrganisationPK();
				AssertHasWarningContaining("When RuleG0090 is Active, Principal and Carrier have same Eori, but different TCU and Org",
					departureMovementHeader.Carrier.OrganisationPKInfo, ExpectedErrorMessage);
			}

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0090Active)))
			{
				departureMovementHeader.Carrier.Validation.ValidateOrganisationPK();
				AssertNoWarningContaining("When RuleG0090 is Disabled, Principal and Carrier have same Eori, but different TCU and Org",
					departureMovementHeader.Carrier.OrganisationPKInfo, ExpectedErrorMessage);
			}
		}

		public void TestPrincipalAndCarrier_MultipleTcu()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddressHeader1 = orgHeader1.Addresses.AddNew();

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddressHeader2 = orgHeader2.Addresses.AddNew();

			orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456654", Core.Constants.CountryCodes.UnitedStates);

			orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.Italy);
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.Italy);

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0090Active)))
			{
				nctsHeader.Principal.E2_OA_Address = orgAddressHeader1.PK;
				departureMovementHeader.Carrier.E2_OA_Address = orgAddressHeader2.PK;

				departureMovementHeader.Carrier.Validation.ValidateOrganisationPK();
				AssertHasWarningContaining("When RuleG0090 is Active, Multiple TCUs with one matching",
					departureMovementHeader.Carrier.OrganisationPKInfo, ExpectedErrorMessage);
			}

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0090Active)))
			{
				departureMovementHeader.Carrier.Validation.ValidateOrganisationPK();
				AssertNoWarningContaining("When RuleG0090 is Disabled, Multiple TCUs with one matching",
					departureMovementHeader.Carrier.OrganisationPKInfo, ExpectedErrorMessage);
			}
		}

		public void TestPrincipalAndCarrier_RandomScenarios()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddressHeader1 = orgHeader1.Addresses.AddNew();

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddressHeader2 = orgHeader2.Addresses.AddNew();

			orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.UnitedStates);
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456654", Core.Constants.CountryCodes.UnitedStates);

			orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456798", Core.Constants.CountryCodes.Italy);
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.Italy);

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0090Active)))
			{
				CombineAssertions("When RuleG0090 is Active", () =>
				{
					departureMovementHeader.Carrier.Validation.ValidateOrganisationPK();
					AssertNoWarningContaining("Null Orgs",
						departureMovementHeader.Carrier.OrganisationPKInfo, ExpectedErrorMessage);

					nctsHeader.Principal.E2_OA_Address = orgAddressHeader1.PK;
					departureMovementHeader.Carrier.E2_OA_Address = orgAddressHeader2.PK;
					departureMovementHeader.Carrier.Validation.ValidateOrganisationPK();
					AssertNoWarningContaining("Different Orgs, Eori and TCU",
						departureMovementHeader.Carrier.OrganisationPKInfo, ExpectedErrorMessage);

					orgHeader1.CustomsCodes.DeleteAll();
					orgHeader2.CustomsCodes.DeleteAll();

					orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.UnitedStates);
					orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Italy);
					departureMovementHeader.Carrier.Validation.ValidateOrganisationPK();
					AssertNoWarningContaining("Same Eori with different Country codes",
						departureMovementHeader.Carrier.OrganisationPKInfo, ExpectedErrorMessage);

					orgHeader1.CustomsCodes.DeleteAll();
					orgHeader2.CustomsCodes.DeleteAll();

					orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.Italy);
					orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.Poland);
					departureMovementHeader.Carrier.Validation.ValidateOrganisationPK();
					AssertNoWarningContaining("Same TCU with different Country codes",
						departureMovementHeader.Carrier.OrganisationPKInfo, ExpectedErrorMessage);

					orgHeader1.CustomsCodes.DeleteAll();
					orgHeader2.CustomsCodes.DeleteAll();

					orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Italy);
					orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Italy);
					orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.Italy);
					orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.Italy);
					departureMovementHeader.Carrier.Validation.ValidateOrganisationPK();
					AssertHasWarningContaining("Same TCU, Eori and Country codes",
						departureMovementHeader.Carrier.OrganisationPKInfo, ExpectedErrorMessage);
				});
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovementHeader = nctsHeader.MovementHeader;
		}

		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovementHeader;

		const string ExpectedErrorMessage =
			"[G0090] Carrier EOR/TCU code used is the same as Principal EOR/TCU code used: Carrier data will not be reported in the Message.";

		#endregion
	}
}
