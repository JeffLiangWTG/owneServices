using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsGuaranteeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPW_BondAmount()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var frGuaranteeHeader = CreateCusGuaranteeHeader(org, Core.Constants.CountryCodes.France, "20200715");
			var esGuaranteeHeader = CreateCusGuaranteeHeader(org, Core.Constants.CountryCodes.Spain, "21210716");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;

			var nctsGuarantee = nctsHeader.Guarantees.AddNew();
			AssertPW_BondAmountWithDifferentCphType(nctsGuarantee, frGuaranteeHeader, "COD");
			AssertPW_BondAmountWithDifferentCphType(nctsGuarantee, frGuaranteeHeader, "NCTS");
			AssertPW_BondAmountWithDifferentCphType(nctsGuarantee, esGuaranteeHeader, "COD");
			AssertPW_BondAmountWithDifferentCphType(nctsGuarantee, esGuaranteeHeader, "NCTS");
		}

		public void TestGuaranteeAmountValidation()
		{
			var guar = Factory.New<NctsGuarantee>();
			CombineAssertions(() =>
			{
				AssertGuaranteeLiabilityAmount(guar, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver, true);
				AssertGuaranteeLiabilityAmount(guar, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee, true);
				AssertGuaranteeLiabilityAmount(guar, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor, true);
				AssertGuaranteeLiabilityAmount(guar, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.FlatRateVoucher, true);
				AssertGuaranteeLiabilityAmount(guar, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage, true);
				AssertGuaranteeLiabilityAmount(guar, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeNotRequiredForCertainPublicBodies, false);
				AssertGuaranteeLiabilityAmount(guar, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeNotRequiredForTheJourneyBetweenOodepAndOotra, false);
				AssertGuaranteeLiabilityAmount(guar, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiverByAgreement, false);
				AssertGuaranteeLiabilityAmount(guar, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur, false);
				AssertGuaranteeLiabilityAmount(guar, EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.MovementsCarriedUnderTheTirConvention, false);
			});
		}

		public void TestRuleC086AndRuleC085()
		{
			var guar = Factory.New<NctsGuarantee>();
			AssertType(typeof(NctsGuaranteeValidation), guar.Validation);
			guar.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
			AssertHasMessageErrorContaining(guar.PW_BondTypeInfo, "C086");
			AssertHasMessageErrorContaining(guar.PW_PasswordInfo, "C086");
			AssertHasMessageErrorContaining(guar.PW_BondTypeInfo, "C085");
			AssertHasMessageErrorContaining(guar.PW_BondNumberInfo, "C085");
			guar.PW_BondNumber = "X";
			AssertHasMessageErrorContaining(guar.PW_BondTypeInfo, "C086");
			AssertHasMessageErrorContaining(guar.PW_PasswordInfo, "C086");
			AssertNoMessageErrorContaining(guar.PW_BondTypeInfo, "C085");
			AssertNoMessageErrorContaining(guar.PW_BondNumberInfo, "C085");
			guar.PW_BondNumber = "";
			guar.PW_Password = "Y";
			AssertNoMessageErrorContaining(guar.PW_BondTypeInfo, "C086");
			AssertNoMessageErrorContaining(guar.PW_PasswordInfo, "C086");
			AssertHasMessageErrorContaining(guar.PW_BondTypeInfo, "C085");
			AssertHasMessageErrorContaining(guar.PW_BondNumberInfo, "C085");
			guar.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee;
			AssertNoMessageErrorContaining(guar.PW_BondTypeInfo, "C086");
			AssertNoMessageErrorContaining(guar.PW_PasswordInfo, "C086");
			AssertNoMessageErrorContaining(guar.PW_BondTypeInfo, "C085");
			AssertNoMessageErrorContaining(guar.PW_BondNumberInfo, "C085");
		}

		public void TestGuaranteeCountValidation()
		{
			var nctsHeader = Factory.New<NctsHeaderForTest>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var guar1 = nctsHeader.Guarantees.AddNew();
			guar1.PW_BondType = "1";
			AssertNoRowMessageErrors(guar1);
			var guar2 = nctsHeader.Guarantees.AddNew();
			guar2.PW_BondType = "1";
			AssertHasRowMessageError(guar2, "The maximum number of guarantees allowed is 1.");
		}

		public void TestPwBondNumberValidationOnCountryValidity()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var nctsHeader1 = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader1.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader1.Principal.E2_OA_Address = org.MainAddress.PK;

			NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader1, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "GB123456", ZDateTime.Empty);
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader1, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "IT123456", ZDateTime.Empty);

			var nctsHeader2 = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader2.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader2.Principal.E2_OA_Address = org.MainAddress.PK;
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader1, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "ES123456", ZDateTime.Empty);

			var frGuaranteeHeader = CreateCusGuaranteeHeader(org, Core.Constants.CountryCodes.UnitedKingdom, "20200715");
			CreateCusGuaranteeHeader(org2, Core.Constants.CountryCodes.UnitedKingdom, "20200717");
			CreateCusGuaranteeHeader(org, Core.Constants.CountryCodes.Spain, "21210716");
			CreateCusGuaranteeHeader(org2, Core.Constants.CountryCodes.Spain, "21210717");

			var rules2 = frGuaranteeHeader.CusGuaranteeRules.AddNew();
			rules2.CPR_RuleCode = "INV";
			rules2.CPR_ValueFrom = Core.Constants.CountryCodes.Germany;
			var rules = frGuaranteeHeader.CusGuaranteeRules.AddNew();
			rules.CPR_RuleCode = "INV";
			rules.CPR_ValueFrom = Core.Constants.CountryCodes.UnitedKingdom;

			var guar1 = nctsHeader1.Guarantees.AddNew();
			guar1.PW_BondType = "4";
			guar1.PW_BondNumber = "20200715";
			AssertHasMessageError(guar1.PW_BondNumberInfo, "This guarantee is not valid in country United Kingdom");
			AssertNoMessageError(guar1.PW_BondNumberInfo, "This guarantee is not valid in country Italy");
			AssertNoMessageError(guar1.PW_BondNumberInfo, "This guarantee is not valid in country Germany");

			frGuaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;

			guar1.PW_BondNumber = "20200715";
			AssertNoMessageError(guar1.PW_BondNumberInfo, "This guarantee is not valid in country Italy");
			AssertHasMessageError(guar1.PW_BondNumberInfo, "This guarantee is not valid in country United Kingdom");

			var rules3 = frGuaranteeHeader.CusGuaranteeRules.AddNew();
			rules3.CPR_RuleCode = "INV";
			rules3.CPR_ValueFrom = Core.Constants.CountryCodes.Italy;
			guar1.PW_BondNumber = "20200715";
			AssertHasMessageError(guar1.PW_BondNumberInfo, "This guarantee is not valid in country Italy");
			AssertHasMessageError(guar1.PW_BondNumberInfo, "This guarantee is not valid in country United Kingdom");

			NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader1, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "DE123456", ZDateTime.Empty);
			guar1.PW_BondNumber = "20200715";
			AssertHasMessageError(guar1.PW_BondNumberInfo, "This guarantee is not valid in country Germany");
			AssertHasMessageErrorContaining(guar1.PW_BondNumberInfo, "This guarantee is not valid in country");

			guar1.PW_BondNumber = "21210716";
			AssertNoMessageErrorContaining(guar1.PW_BondNumberInfo, "This guarantee is not valid in country");

			guar1.PW_BondNumber = "21210717";
			AssertNoMessageErrorContaining(guar1.PW_BondNumberInfo, "This guarantee is not valid in country");

			var guar2 = nctsHeader2.Guarantees.AddNew();
			guar2.PW_BondType = "4";
			guar2.PW_BondNumber = "20200715";
			AssertNoMessageErrorContaining(guar2.PW_BondNumberInfo, "This guarantee is not valid in country");
		}

		public void TestEnsureGuaranteeExists()
		{
			const string guaranteeNotExist = "This guarantee does not exist, and so transactions will not be managed. It is recommended to create a guarantee record for the best functionality.";
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			var nctsGuarantee = nctsHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "20210101";
			AssertHasMessageError(nctsGuarantee.PW_BondNumberInfo, guaranteeNotExist);
		}

		public void TestEnsureGuaranteeExistsInTIRContext()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;

			var nctsGuarantee = nctsHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "20210101";
			AssertHasMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "This guarantee does not exist");

			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
			nctsGuarantee.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.MovementsCarriedUnderTheTirConvention;
			AssertNoMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "This guarantee does not exist");
		}

		public void TestEnsureGuaranteeOwnedByPrincipal()
		{
			const string guaranteeNotOwnedByPrincipal = "This guarantee is not owned by the selected principal.";
			var principal = Factory.New<OrgHeader>();
			var importer = Factory.New<OrgHeader>();

			var guarantee = Factory.New<CusGuaranteeHeader>();
			guarantee.CPH_Number = "20210101";
			guarantee.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guarantee.CPH_OH_PermitHolder = importer.PK;

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;
			nctsHeader.BH_OA_Importer = importer.PK;
			var nctsGuarantee = nctsHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondNumber = "20210101";
			AssertHasMessageError(nctsGuarantee.PW_BondNumberInfo, guaranteeNotOwnedByPrincipal);

			guarantee.CPH_OH_PermitHolder = principal.PK;
			nctsGuarantee.Validation.ValidatePW_BondNumber();
			AssertNoMessageError(nctsGuarantee.PW_BondNumberInfo, guaranteeNotOwnedByPrincipal);
		}

		public void TestPermitRuleNotFoundOnGuarantee()
		{
			NCTSTestHelper.AssertCheckPW_BondAmount_PermitRuleNotFound(Factory, CusInBondApplicationCodeList.Codes.NCTS4);
		}

		public void TestRuleTR0301()
		{
			var principal = Factory.New<OrgHeader>();
			var importer = Factory.New<OrgHeader>();

			var guarantee = Factory.New<CusGuaranteeHeader>();
			guarantee.CPH_Number = "20210101";
			guarantee.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guarantee.CPH_OH_PermitHolder = principal.PK;

			var guarantee2 = Factory.New<CusGuaranteeHeader>();
			guarantee2.CPH_Number = "123456789012345678901234";
			guarantee2.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guarantee2.CPH_OH_PermitHolder = principal.PK;

			var guarantee3 = Factory.New<CusGuaranteeHeader>();
			guarantee3.CPH_Number = "12345678901234567";
			guarantee3.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guarantee3.CPH_OH_PermitHolder = principal.PK;

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;
			nctsHeader.BH_OA_Importer = importer.PK;
			var nctsGuarantee = nctsHeader.Guarantees.AddNew();

			nctsGuarantee.PW_BondNumber = "20210101";
			nctsGuarantee.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee;

			nctsGuarantee.Validation.ValidatePW_BondNumber();
			AssertNoMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "characters long");

			nctsGuarantee.PW_BondNumber = ZString.Empty;
			nctsGuarantee.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.FlatRateVoucher;

			nctsGuarantee.Validation.ValidatePW_BondNumber();
			AssertNoMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "characters long");

			nctsGuarantee.PW_BondNumber = "20210101";
			nctsGuarantee.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.FlatRateVoucher;

			nctsGuarantee.Validation.ValidatePW_BondNumber();
			AssertHasMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "characters long");
			AssertHasMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "Guarantee reference must be 24 characters long");

			nctsGuarantee.PW_BondNumber = "123456789012345678901234";
			nctsGuarantee.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.FlatRateVoucher;

			nctsGuarantee.Validation.ValidatePW_BondNumber();
			AssertNoMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "Guarantee reference must be 24 characters long");

			nctsGuarantee.PW_BondNumber = ZString.Empty;
			nctsGuarantee.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;

			nctsGuarantee.Validation.ValidatePW_BondNumber();
			AssertNoMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "characters long");

			nctsGuarantee.PW_BondNumber = "20210101";
			nctsGuarantee.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;

			nctsGuarantee.Validation.ValidatePW_BondNumber();
			AssertHasMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "Guarantee reference must be 17 characters long");

			nctsGuarantee.PW_BondNumber = "12345678901234567";
			nctsGuarantee.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;

			nctsGuarantee.Validation.ValidatePW_BondNumber();
			AssertNoMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "Guarantee reference must be 17 characters long");
		}

		public void TestRuleTR0301NotApplyingForBTypeOfGuarantee()
		{
			var principal = Factory.New<OrgHeader>();
			var importer = Factory.New<OrgHeader>();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;
			nctsHeader.BH_OA_Importer = importer.PK;
			var nctsGuarantee = nctsHeader.Guarantees.AddNew();

			nctsGuarantee.PW_BondNumber = "20210101";
			nctsGuarantee.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
			AssertHasMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "characters long");

			nctsGuarantee.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.MovementsCarriedUnderTheTirConvention;
			AssertNoMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "characters long");
		}

		public void TestCheckPW_BondType()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			var nctsGuarantee = nctsHeader.Guarantees.AddNew();

			CombineAssertions(() =>
			{
				AssertType(typeof(NctsGuaranteeValidation), nctsGuarantee.Validation);
				nctsGuarantee.PW_BondType = "";

				AssertHasMessageErrorContaining($"When {nameof(nctsGuarantee.PW_BondType)} is empty", nctsGuarantee.PW_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
				nctsGuarantee.PW_BondType = "B";

				AssertNoMessageErrorContaining($"When {nameof(nctsGuarantee.PW_BondType)} is not empty", nctsGuarantee.PW_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckPW_SuretyCode()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;

			var nctsGuarantee = nctsHeader.Guarantees.AddNew();
			nctsGuarantee.PW_SuretyCode = LiabilityApplicablePercentageCodeList.Codes.FUL;
			AssertNoMessageErrorContaining(nctsGuarantee.PW_SuretyCodeInfo, ListValidation.InvalidCodeMessageError);

			nctsGuarantee.PW_SuretyCode = "ABC";
			AssertHasMessageErrorContaining(nctsGuarantee.PW_SuretyCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		CusGuaranteeHeader CreateCusGuaranteeHeader(OrgHeader org, string countryCode, string cphNumber)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				guaranteeHeader.CPH_Number = cphNumber;
				guaranteeHeader.CPH_OH_PermitHolder = org.PK;
				guaranteeHeader.CPH_Balance = 50m;
				guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
				guaranteeHeader.CPH_SubType = "4";

				var guaranteeLineTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
				guaranteeLineTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
				guaranteeLineTransaction.CPL_TranValue = 50m;
				guaranteeLineTransaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
				guaranteeLineTransaction.CPL_Reference = "XJ5 - 00003877";
				Factory.Save();
				return guaranteeHeader;
			}
		}

		void AssertPW_BondAmountWithDifferentCphType(NctsGuarantee nctsGuarantee, CusGuaranteeHeader guaranteeHeader, string cphType)
		{
			nctsGuarantee.PW_BondNumber = "";
			guaranteeHeader.CPH_Type = cphType;
			var cusGuarantee = nctsGuarantee.CusGuarantee;

			AssertNull(cusGuarantee);

			nctsGuarantee.PW_BondNumber = guaranteeHeader.CPH_Number;
			cusGuarantee = nctsGuarantee.CusGuarantee;

			if (guaranteeHeader.IsPermitGuaranteeType)
			{
				AssertNotNull(cusGuarantee);
				AssertEquals(guaranteeHeader.PK, cusGuarantee.PK);

				nctsGuarantee.PW_BondAmount = 140m;

				Assert(nctsGuarantee.PW_BondAmountInfo.HasMessageError(ZString.Format("This guarantee has a remaining balance of {0} but this movement would incur a liability of {1}", guaranteeHeader.CPH_Calc_TotalBalanceIncludingPending.Amount, nctsGuarantee.PW_BondAmount)));

				nctsGuarantee.PW_BondAmount = 50m;

				Assert(!nctsGuarantee.PW_BondAmountInfo.HasMessageError(ZString.Format("This guarantee has a remaining balance of {0} but this movement would incur a liability of {1}", guaranteeHeader.CPH_Calc_TotalBalanceIncludingPending.Amount, nctsGuarantee.PW_BondAmount)));
			}
			else
			{
				AssertNull(cusGuarantee);
			}
		}

		void AssertGuaranteeLiabilityAmount(NctsGuarantee guarantee, string guaranteeType, ZBool shouldRaiseMessageError)
		{
			guarantee.PW_BondType = guaranteeType;
			guarantee.PW_BondAmount = ZDecimal.Zero;

			if (shouldRaiseMessageError)
			{
				AssertHasMessageErrorContaining(guarantee.PW_BondAmountInfo, "Guarantee Liability Amount is required");
				guarantee.PW_BondAmount = 1234.56m;
				AssertNoMessageErrorContaining(guarantee.PW_BondAmountInfo, "Guarantee Liability Amount is required");
			}
			else
			{
				AssertNoMessageErrorContaining(guarantee.PW_BondAmountInfo, "Guarantee Liability Amount is required");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		}

		public sealed class NctsGuaranteeValidationTest_DoNotInheritTest : BusinessObjectValidationTestCase
		{
			public void TestApplyRuleC086()
			{
				var nctsGuarantee = Factory.New<NctsGuarantee>();
				var validation = new NctsGuaranteeValidationForTest(nctsGuarantee);
				AssertEquals("ApplyRuleC086 is applicable", true, validation.ApplyRuleC086Exposed);
			}

			class NctsGuaranteeValidationForTest : NctsGuaranteeValidation
			{
				public NctsGuaranteeValidationForTest(NctsGuarantee cusBondDetail) : base(cusBondDetail)
				{
				}

				public bool ApplyRuleC086Exposed => base.ApplyRuleC086;
			}
		}
	}
}
