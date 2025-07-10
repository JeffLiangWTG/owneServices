using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsRuleG0123_1ValidationTest : TestCaseWithFactory
	{
		public void TestHeaderConsignorWithPrinciple()
		{
			const string expectedErrorMessage = "[G0123-1] The Consignor in the Header must be entered only if different from the Principal.";
			var orgHeader1 = NCTSTestHelper.CreateOrgHeaderForTest(Factory);
			var orgHeader2 = NCTSTestHelper.CreateOrgHeaderForTest(Factory);

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0123_1Active)))
			{
				CombineAssertions("When G0123-1 rule enabled and Phase 5", () =>
				{
					AssertNoMessageError("When Consignor not filled", nctsHeader.Consignor.OrganisationPKInfo, expectedErrorMessage);

					nctsHeader.Principal.OrganisationPK = orgHeader1.PK;
					nctsHeader.Consignor.OrganisationPK = orgHeader1.PK;
					AssertHasMessageError("When same Organization entered in Consignor and Principal", nctsHeader.Consignor.OrganisationPKInfo, expectedErrorMessage);

					nctsHeader.Consignor.OrganisationPK = orgHeader2.PK;
					AssertNoMessageError("When different Organization entered in Consignor and Principal", nctsHeader.Consignor.OrganisationPKInfo, expectedErrorMessage);
				});
			}

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0123_1Active)))
			{
				nctsHeader.Consignor.OrganisationPK = orgHeader1.PK;
				AssertNoMessageError("When G0123-1 rule disabled, Phase 5, and same Organization entered in Consignor and Principal", nctsHeader.Consignor.OrganisationPKInfo, expectedErrorMessage);
			}

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0123_1Active)))
			{
				nctsHeader.Consignor.Validation.ValidateOrganisationPK();
				AssertNoMessageError("When G0123-1 rule enabled, Phase 4, and same Organization entered in Consignor and Principal", nctsHeader.Consignor.OrganisationPKInfo, expectedErrorMessage);
			}
		}

		public void TestHeaderConsignorWithPrincipleEorAndTcuCode()
		{
			const string expectedErrorMessage = "[G0123-1] Consignor EOR/TCU code is the same as Principal EOR/TCU code. The Consignor in the Header must be entered only if different from the Principal.";
			var principal = NCTSTestHelper.CreateOrgHeaderForTest(Factory);
			var consignor = NCTSTestHelper.CreateOrgHeaderForTest(Factory);
			nctsHeader.Principal.OrganisationPK = principal.PK;
			nctsHeader.Consignor.OrganisationPK = consignor.PK;

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0123_1Active)))
			{
				CombineAssertions("When G0123-1 rule enabled and Phase 5", () =>
				{
					AssertNoMessageError("When EOR/TCU code not present", nctsHeader.Consignor.OrganisationPKInfo, expectedErrorMessage);

					var principalCusCode = principal.CustomsCodes.AddNew("EOR", "123");
					var consignorCusCode = consignor.CustomsCodes.AddNew("EOR", "123");
					nctsHeader.Consignor.Validation.ValidateOrganisationPK();
					AssertHasMessageError("When same EOR code present in Consignor and Principal", nctsHeader.Consignor.OrganisationPKInfo, expectedErrorMessage);

					principalCusCode.OK_CodeType = "TCU";
					consignorCusCode.OK_CodeType = "TCU";
					nctsHeader.Consignor.Validation.ValidateOrganisationPK();
					AssertHasMessageError("When same TCU code present in Consignor and Principal", nctsHeader.Consignor.OrganisationPKInfo, expectedErrorMessage);

					consignorCusCode.OK_CustomsRegNo = "111";
					nctsHeader.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("When different TCU code present in Consignor and Principal", nctsHeader.Consignor.OrganisationPKInfo, expectedErrorMessage);

					principalCusCode.OK_CodeType = "EOR";
					consignorCusCode.OK_CodeType = "EOR";
					nctsHeader.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("When different EOR code present in Consignor and Principal", nctsHeader.Consignor.OrganisationPKInfo, expectedErrorMessage);
				});
			}

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0123_1Active)))
			{
				consignor.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("EOR", "123", GlbCompany.CurrentCompany.Country.Code);
				nctsHeader.Consignor.Validation.ValidateOrganisationPK();
				AssertNoMessageError("When G0123-1 rule disabled, Phase 5, and same EOR code present in Consignor and Principal", nctsHeader.Consignor.OrganisationPKInfo, expectedErrorMessage);
			}

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0123_1Active)))
			{
				nctsHeader.Consignor.Validation.ValidateOrganisationPK();
				AssertNoMessageError("When G0123-1 rule enabled, Phase 4, and same EOR code present in Consignor and Principal", nctsHeader.Consignor.OrganisationPKInfo, expectedErrorMessage);
			}
		}

		public void TestHouseConsignorWithPrinciple()
		{
			const string expectedErrorMessage = "[G0123-1] The Consignor in the House must be entered only if different from the Principal.";
			var orgHeader1 = NCTSTestHelper.CreateOrgHeaderForTest(Factory);
			var orgHeader2 = NCTSTestHelper.CreateOrgHeaderForTest(Factory);

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0123_1Active)))
			{
				CombineAssertions("When G0123-1 rule enabled and Phase 5", () =>
				{
					AssertNoMessageError("When Consignor not filled", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);

					nctsHeader.Principal.OrganisationPK = orgHeader1.PK;
					nctsBill.Consignor.OrganisationPK = orgHeader1.PK;
					AssertHasMessageError("When same Organization entered in Consignor and Principal", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);

					nctsBill.Consignor.OrganisationPK = orgHeader2.PK;
					AssertNoMessageError("When different Organization entered in Consignor and Principal", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);
				});
			}

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0123_1Active)))
			{
				nctsBill.Consignor.Validation.ValidateOrganisationPK();
				AssertNoMessageError("When G0123-1 rule disabled, Phase 5, and same Organization entered in Consignor and Principal", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);
			}

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0123_1Active)))
			{
				nctsBill.Consignor.Validation.ValidateOrganisationPK();
				AssertNoMessageError("When G0123-1 rule enabled, Phase 4, and same Organization entered in Consignor and Principal", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);
			}
		}

		public void TestHouseConsignorWithPrincipleEorAndTcuCode()
		{
			const string expectedErrorMessage = "[G0123-1] Consignor EOR/TCU code is the same as Principal EOR/TCU code. The Consignor in the House must be entered only if different from the Principal.";
			var principal = NCTSTestHelper.CreateOrgHeaderForTest(Factory);
			var consignor = NCTSTestHelper.CreateOrgHeaderForTest(Factory);
			nctsHeader.Principal.OrganisationPK = principal.PK;
			nctsBill.Consignor.OrganisationPK = consignor.PK;

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0123_1Active)))
			{
				CombineAssertions("When G0123-1 rule enabled and Phase 5", () =>
				{
					nctsBill.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("When EOR/TCU code not present", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);

					var principalCusCode = principal.CustomsCodes.AddNew("EOR", "123");
					var consignorCusCode = consignor.CustomsCodes.AddNew("EOR", "123");
					nctsBill.Consignor.Validation.ValidateOrganisationPK();
					AssertHasMessageError("When same EOR code present in Consignor and Principal", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);

					principalCusCode.OK_CodeType = "TCU";
					consignorCusCode.OK_CodeType = "TCU";
					nctsBill.Consignor.Validation.ValidateOrganisationPK();
					AssertHasMessageError("When same TCU code present in Consignor and Principal", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);

					consignorCusCode.OK_CustomsRegNo = "111";
					nctsBill.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("When different TCU code present in Consignor and Principal", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);

					principalCusCode.OK_CodeType = "EOR";
					consignorCusCode.OK_CodeType = "EOR";
					nctsBill.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("When different EOR code present in Consignor and Principal", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);
				});
			}

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0123_1Active)))
			{
				consignor.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("EOR", "123", GlbCompany.CurrentCompany.Country.Code);
				nctsBill.Consignor.Validation.ValidateOrganisationPK();
				AssertNoMessageError("When G0123-1 rule disabled, Phase 5, and same EOR code present in Consignor and Principal", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);
			}

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleG0123_1Active)))
			{
				nctsBill.Consignor.Validation.ValidateOrganisationPK();
				AssertNoMessageError("When G0123-1 rule enabled, Phase 4, and same EOR code present in Consignor and Principal", nctsBill.Consignor.OrganisationPKInfo, expectedErrorMessage);
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
