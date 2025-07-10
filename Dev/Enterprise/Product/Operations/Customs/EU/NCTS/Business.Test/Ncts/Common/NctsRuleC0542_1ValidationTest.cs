using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsRuleC0542_1ValidationTest : TestCaseWithFactory
	{
		public void TestHeaderConsignorWithSecurityAndReducedDatasetFlag()
		{
			const string expectedErrorMessage = "[C0542-1] The Consignor in the Header must not be entered if Security is NON and Reduced Dataset Ind. is flagged.";
			var orgHeader = NCTSTestHelper.CreateOrgHeaderForTest(Factory);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_TypeOfSecurity = "NON";
			movementHeader.BM_ReducedDatasetIndicator = true;

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0542_1Active)))
			{
				CombineAssertions("When C0542-1 rule enabled and Phase 5", () =>
				{
					nctsHeader.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("When TypeOfSecurity = NON, Reduced Dataset flagged, and Consignor not entered", nctsHeader.Consignor.OrganisationPKInfo, expectedErrorMessage);

					nctsHeader.Consignor.OrganisationPK = orgHeader.PK;
					AssertHasMessageError("When TypeOfSecurity = NON, Reduced Dataset flagged, and Consignor entered", nctsHeader.Consignor.OrganisationPKInfo, expectedErrorMessage);

					movementHeader.BM_TypeOfSecurity = "ABC";
					nctsHeader.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("When TypeOfSecurity = ABC, Reduced Dataset flagged, and Consignor entered", nctsHeader.Consignor.OrganisationPKInfo, expectedErrorMessage);

					movementHeader.BM_TypeOfSecurity = "NON";
					movementHeader.BM_ReducedDatasetIndicator = false;
					nctsHeader.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("When TypeOfSecurity = NON, Reduced Dataset not flagged, and Consignor entered", nctsHeader.Consignor.OrganisationPKInfo, expectedErrorMessage);
				});
			}

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0542_1Active)))
			{
				nctsHeader.Consignor.Validation.ValidateOrganisationPK();
				AssertNoMessageError("When C0542-1 rule disabled, Phase 5, TypeOfSecurity = NON, Reduced Dataset flagged, and Consignor entered", nctsHeader.Consignor.OrganisationPKInfo, expectedErrorMessage);
			}

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0542_1Active)))
			{
				nctsHeader.Consignor.Validation.ValidateOrganisationPK();
				AssertNoMessageError("When C0542-1 rule enabled, Phase 4, TypeOfSecurity = NON, Reduced Dataset flagged, and Consignor entered", nctsHeader.Consignor.OrganisationPKInfo, expectedErrorMessage);
			}
		}

		public void TestHouseConsignorWithSecurityAndReducedDatasetFlag()
		{
			const string expectedErrorMessage = "[C0542-1] The Consignor in the House must not be entered if Security is NON and Reduced Dataset Ind. is flagged.";
			var orgHeader = NCTSTestHelper.CreateOrgHeaderForTest(Factory);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_TypeOfSecurity = "NON";
			movementHeader.BM_ReducedDatasetIndicator = true;

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0542_1Active)))
			{
				CombineAssertions("When C0542-1 rule enabled and Phase 5", () =>
				{
					nctsBill.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("When TypeOfSecurity = NON, Reduced Dataset flagged, and Consignor not entered", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);

					nctsBill.Consignor.OrganisationPK = orgHeader.PK;
					AssertHasMessageError("When TypeOfSecurity = NON, Reduced Dataset flagged, and Consignor entered", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);

					movementHeader.BM_TypeOfSecurity = "ABC";
					nctsBill.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("When TypeOfSecurity = ABC, Reduced Dataset flagged, and Consignor entered", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);

					movementHeader.BM_TypeOfSecurity = "NON";
					movementHeader.BM_ReducedDatasetIndicator = false;
					nctsBill.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("When TypeOfSecurity = NON, Reduced Dataset not flagged, and Consignor entered", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);
				});
			}

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0542_1Active)))
			{
				nctsBill.Consignor.Validation.ValidateOrganisationPK();
				AssertNoMessageError("When C0542-1 rule disabled, Phase 5, TypeOfSecurity = NON, Reduced Dataset flagged, and Consignor entered", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);
			}

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0542_1Active)))
			{
				nctsBill.Consignor.Validation.ValidateOrganisationPK();
				AssertNoMessageError("When C0542-1 rule enabled, Phase 4, TypeOfSecurity = NON, Reduced Dataset flagged, and Consignor entered", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);
			}
		}

		public void TestHouseConsignorWithSecurityReducedDatasetFlagAndHeaderConsignor()
		{
			const string expectedErrorMessage = "[C0542-1] When Security is different from 'NON' or Reduced Dataset Ind. is not flagged, the Consignor in the House must not be entered if the Consignor in the Consignment Header is entered.";
			var orgHeader = NCTSTestHelper.CreateOrgHeaderForTest(Factory);
			nctsHeader.Consignor.OrganisationPK = orgHeader.PK;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_TypeOfSecurity = "ABC";

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0542_1Active)))
			{
				CombineAssertions("When C0542-1 rule enabled and Phase 5", () =>
				{
					nctsBill.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("When Header Consignor filled, TypeOfSecurity = ABC, Reduced Dataset not flagged, and House Consignor not filled", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);

					nctsBill.Consignor.OrganisationPK = orgHeader.PK;
					AssertHasMessageError("When Header Consignor filled, TypeOfSecurity = ABC, Reduced Dataset not flagged, and House Consignor filled", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);

					movementHeader.BM_TypeOfSecurity = "NON";
					nctsBill.Consignor.Validation.ValidateOrganisationPK();
					AssertHasMessageError("When Header Consignor filled, TypeOfSecurity = NON, Reduced Dataset not flagged, and House Consignor filled", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);

					movementHeader.BM_TypeOfSecurity = "ABC";
					movementHeader.BM_ReducedDatasetIndicator = true;
					nctsBill.Consignor.Validation.ValidateOrganisationPK();
					AssertHasMessageError("When Header Consignor filled, TypeOfSecurity = ABC, Reduced Dataset flagged, and House Consignor filled", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);

					movementHeader.BM_TypeOfSecurity = "NON";
					nctsBill.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("When Header Consignor filled, TypeOfSecurity = NON, Reduced Dataset flagged, and House Consignor filled", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);

					movementHeader.BM_TypeOfSecurity = "ABC";
					movementHeader.BM_ReducedDatasetIndicator = false;
					nctsHeader.Consignor.OrganisationPK = ZGuid.Empty;
					nctsBill.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("When Header Consignor not filled, TypeOfSecurity = ABC, Reduced Dataset not flagged, and House Consignor filled", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);
				});
			}

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0542_1Active)))
			{
				CombineAssertions("When C0542-1 rule disabled, Phase 5,and Header and House Consignors filled", () =>
				{
					nctsHeader.Consignor.OrganisationPK = orgHeader.PK;
					nctsBill.Consignor.OrganisationPK = orgHeader.PK;
					AssertNoMessageError("TypeOfSecurity = ABC, Reduced Dataset not flagged", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);

					movementHeader.BM_TypeOfSecurity = "NON";
					nctsBill.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("TypeOfSecurity = NON, Reduced Dataset not flagged", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);

					movementHeader.BM_TypeOfSecurity = "ABC";
					movementHeader.BM_ReducedDatasetIndicator = true;
					nctsBill.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("TypeOfSecurity = ABC, Reduced Dataset flagged", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);
				});
			}

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0542_1Active)))
			{
				CombineAssertions("When C0542-1 rule enabled, Phase 4,and Header and House Consignors filled", () =>
				{
					nctsHeader.Consignor.OrganisationPK = orgHeader.PK;
					nctsBill.Consignor.OrganisationPK = orgHeader.PK;
					AssertNoMessageError("TypeOfSecurity = ABC, Reduced Dataset not flagged", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);

					movementHeader.BM_TypeOfSecurity = "NON";
					nctsBill.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("TypeOfSecurity = NON, Reduced Dataset not flagged", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);

					movementHeader.BM_TypeOfSecurity = "ABC";
					movementHeader.BM_ReducedDatasetIndicator = true;
					nctsBill.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("TypeOfSecurity = ABC, Reduced Dataset flagged", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsBill = nctsHeader.Bills.AddNew();
		}

		NctsHeader nctsHeader;
		NctsBill nctsBill;
	}
}
