using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using GuaranteeCodes = Enterprise.Customs.EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes;
using GuaranteeTypeDescriptions = Enterprise.Customs.EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Descriptions;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsGuarantee))]
	sealed class NctsGuaranteePhase5ValidationTest : TestCaseWithFactory
	{
		public void TestGuaranteeOverbooked_TR0093()
		{
			using var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory);

			const string warning =
				"[TR0093] Balance used -1.0000 EUR exceeds Guarantee Amount.";

			var org = Factory.NewWithValidTestData<OrgHeader>();

			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_OH_PermitHolder = org.PK;
			guaranteeHeader.CPH_UnitOfMeasure = "EUR";
			guaranteeHeader.CPH_Number = "GUA1";
			guaranteeHeader.CPH_Type = "TRA";
			guaranteeHeader.CPH_RN_NKCountryCode = CountryCodes.Germany;
			guaranteeHeader.CPH_Balance = 50m;

			guaranteeHeader.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 50.0m, 0, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);

			guaranteeHeader.AddTransaction("123",
				"NCTS write-off",
				string.Empty,
				ZString.Empty,
				-40,
				0,
				status: PermitTransactionStatusList.Codes.Confirmed);

			Factory.Save();

			guarantee.PW_BondNumber = "GUA1";
			guarantee.PW_CPH_Guarantee = guaranteeHeader.PK;
			header.Principal.E2_OA_Address = org.MainAddress.PK;

			deciderTestContext.DisableRule(c => c.IsRuleTR0093Active);

			guarantee.PW_BondAmount = 11m;
			guarantee.Validation.ValidateAll();
			AssertNoRowWarningContaining(guarantee, warning);

			guarantee.PW_BondAmount = 0;
			guarantee.Validation.ValidateAll();
			AssertNoRowWarningContaining(guarantee, warning);

			deciderTestContext.EnableRule(c => c.IsRuleTR0093Active);

			guarantee.PW_BondAmount = 11;
			guarantee.Validation.ValidateAll();
			AssertHasRowWarning(guarantee, warning);

			guarantee.PW_BondAmount = 10;
			guarantee.Validation.ValidateAll();
			AssertNoRowWarningContaining(guarantee, warning);
		}

		public void TestCheckPW_BondNumber_C0085_WhenActive()
		{
			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				deciderTestContext.EnableRule(c => c.IsRuleC0085Active);
				deciderTestContext.DisableRule(c => c.IsRuleC0085_1Active);

				AssertCheckPW_BondNumber_C0085_or_C0085_1WhenActive(new[]
				{
					GuaranteeCodes.GuaranteeWaiver,
					GuaranteeCodes.ComprehensiveGuarantee,
					GuaranteeCodes.IndividualGuaranteeByGuarantor,
					GuaranteeCodes.CashDepositGuarantee,
					GuaranteeCodes.FlatRateVoucher,
					GuaranteeCodes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur,
					GuaranteeCodes.IndividualGuaranteeWithMultipleUsage,
				},
				"[C0085]");
			}
		}

		public void TestCheckPW_BondNumber_C0085_1WhenActive()
		{
			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				deciderTestContext.DisableRule(c => c.IsRuleC0085Active);
				deciderTestContext.EnableRule(c => c.IsRuleC0085_1Active);

				AssertCheckPW_BondNumber_C0085_or_C0085_1WhenActive(new[]
				{
					GuaranteeCodes.GuaranteeWaiver,
					GuaranteeCodes.ComprehensiveGuarantee,
					GuaranteeCodes.IndividualGuaranteeByGuarantor,
					GuaranteeCodes.FlatRateVoucher,
					GuaranteeCodes.IndividualGuaranteeWithMultipleUsage,
				},
				"[C0085-1]");
			}
		}

		void AssertCheckPW_BondNumber_C0085_or_C0085_1WhenActive(string[] guaranteeTypeWithReference, string messagePrefix)
		{
			var errorMessage = $"{messagePrefix} You have not entered a Guarantee Reference Number (GRN).";
			var allCodes = new EUNctsGuaranteeTypeList().GetAllCodes();
			var guaranteeTypeNotWithReference = allCodes.Except(guaranteeTypeWithReference.Cast<string>());

			CombineAssertions(() =>
			{
				guaranteeTypeWithReference.ForEach(code => AssertGuaranteeTypeWithReference(guarantee, code));
				guaranteeTypeNotWithReference.ForEach(code => AssertGuaranteeTypeNotWithReference(guarantee, code));
			});

			void AssertGuaranteeTypeWithReference(NctsGuarantee guarantee, ZString bondType)
			{
				guarantee.PW_BondType = bondType;
				guarantee.PW_BondNumber = ZString.Empty;
				AssertHasMessageError($"PW_BondType is {bondType} and PW_BondNumber is empty", guarantee.PW_BondNumberInfo, errorMessage);

				guarantee.PW_BondNumber = "abc";
				AssertNoMessageError($"PW_BondType is {bondType} and PW_BondNumber is not empty", guarantee.PW_BondNumberInfo, errorMessage);
			}

			void AssertGuaranteeTypeNotWithReference(NctsGuarantee guarantee, ZString bondType)
			{
				guarantee.PW_BondType = bondType;
				guarantee.PW_BondNumber = ZString.Empty;
				AssertNoMessageError($"PW_BondType is {bondType} and PW_BondNumber is empty", guarantee.PW_BondNumberInfo, errorMessage);

				guarantee.PW_BondNumber = "abc";
				AssertNoMessageError($"PW_BondType is {bondType} and PW_BondNumber is not empty", guarantee.PW_BondNumberInfo, errorMessage);
			}
		}

		public void TestCheckPW_BondNumber_C0085_C0085_1WhenNotActive()
		{
			const string errorMessage = "[C0085] You have not entered a Guarantee Reference Number (GRN).";
			const string errorMessage_1 = "[C0085_1] You have not entered a Guarantee Reference Number (GRN).";
			var guaranteeTypeWithReference = new string[]
			{
				GuaranteeCodes.GuaranteeWaiver,
				GuaranteeCodes.ComprehensiveGuarantee,
				GuaranteeCodes.IndividualGuaranteeByGuarantor,
				GuaranteeCodes.CashDepositGuarantee,
				GuaranteeCodes.FlatRateVoucher,
				GuaranteeCodes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur,
				GuaranteeCodes.IndividualGuaranteeWithMultipleUsage,
			};

			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				deciderTestContext.DisableRule(c => c.IsRuleC0085Active);
				deciderTestContext.DisableRule(c => c.IsRuleC0085_1Active);
				var actions = guaranteeTypeWithReference.SelectMany(GetTestCase).ToArray();
				foreach (var testCase in actions)
				{
					testCase?.Invoke();
					Assert($"Expected notifications would not contain {errorMessage}", !guarantee.PW_BondNumberInfo.Notifications.Any(x => x.Message == errorMessage));
					Assert($"Expected notifications would not contain {errorMessage_1}", !guarantee.PW_BondNumberInfo.Notifications.Any(x => x.Message == errorMessage_1));
				}
			}

			IEnumerable<Action> GetTestCase(string bondType)
			{
				yield return () =>
				{
					guarantee.PW_BondType = bondType;
					guarantee.PW_BondNumber = ZString.Empty;
				};
			}
		}

		public void TestCheckPW_BondNumber_C0086()
		{
			var c0086BondTypeList = new ZString[]
			{
				GuaranteeCodes.GuaranteeWaiver
				, GuaranteeCodes.ComprehensiveGuarantee
				, GuaranteeCodes.IndividualGuaranteeByGuarantor
				, GuaranteeCodes.FlatRateVoucher
				, GuaranteeCodes.IndividualGuaranteeWithMultipleUsage
			};
			var messageError = "[C0086] You have not entered a Guarantee Reference Number (GRN).";

			CombineAssertions(() =>
			{
				foreach (var bondType in c0086BondTypeList)
				{
					guarantee.PW_BondNumber2 = "ref";
					guarantee.PW_BondType = bondType;
					guarantee.PW_BondNumber = ZString.Empty;
					AssertHasMessageErrorContaining($"Empty BondNumber with Bond Type {bondType}", guarantee.PW_BondNumberInfo, messageError);

					guarantee.PW_BondNumber = "123";
					AssertNoMessageErrorContaining($"Not Empty BondNumber Bond Type {bondType}", guarantee.PW_BondNumberInfo, messageError);
				}

				guarantee.PW_BondType = GuaranteeCodes.GuaranteeWaived;
				guarantee.PW_BondNumber = ZString.Empty;
				AssertNoMessageErrorContaining($"Not Empty BondNumber Bond Type outside c0086 scope", guarantee.PW_BondNumberInfo, messageError);
			});
		}

		public void TestCheckPW_BondNumber_TR0019()
		{
			Check_TR0019_ForProperty(
				guarantee => guarantee.PW_BondNumberInfo,
				$"Duplicate Guarantee Ref. No. for departure ncts movements when TR0019 is active",
				"[TR0019] Duplicate Guarantee Ref. No. is entered.");
		}

		public void TestCheckPW_BondNumber2_TR0019()
		{
			Check_TR0019_ForProperty(
				guarantee => guarantee.PW_BondNumber2Info,
				$"Duplicate Other Guarantee Ref. No. for departure ncts movements when TR0019 is active",
				"[TR0019] Duplicate Other Guarantee Ref. No. is entered.");
		}

		public void TestCheckPW_BondNumber_WhenR0318Active()
		{
			var errorMessageNotFlatRateVoucher = "[R0318] Since Guarantee Type is other than \"4\" the GRN should be declared as alphanumeric data 17 characters long.";
			var errorMessageFlatRateVoucher = "[R0318] Since Guarantee Type is \"4\" the GRN should be declared as alphanumeric data 24 characters long.";

			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				deciderTestContext.EnableRule(r => r.IsRuleR0318Active);
				CombineAssertions(() =>
				{
					guarantee.PW_BondType = GuaranteeCodes.FlatRateVoucher;
					guarantee.PW_BondNumber = "1234567890123456789012345";
					AssertHasMessageError("BondType equal to 4, BondNumber length not equal to 24.", guarantee.PW_BondNumberInfo, errorMessageFlatRateVoucher);

					guarantee.PW_BondNumber = "12345678901234567890123!";
					AssertHasMessageError("BondType equal to 4, BondNumber is not Alphanumeric Data.", guarantee.PW_BondNumberInfo, errorMessageFlatRateVoucher);

					guarantee.PW_BondNumber = "123456789012345678901234";
					AssertNoMessageError("BondType equal to 4, BondNumber is Alphanumeric Data and length equal to 24.", guarantee.PW_BondNumberInfo, errorMessageFlatRateVoucher);

					guarantee.PW_BondType = GuaranteeCodes.GuaranteeWaiver;
					guarantee.PW_BondNumber = "12345678901234567f8";
					AssertHasMessageError("BondType not equal to 4, BondNumber length not equal to 17.", guarantee.PW_BondNumberInfo, errorMessageNotFlatRateVoucher);

					guarantee.PW_BondNumber = "1234567890123456!";
					AssertHasMessageError("BondType not equal to 4, BondNumber is not Alphanumeric Data.", guarantee.PW_BondNumberInfo, errorMessageNotFlatRateVoucher);

					guarantee.PW_BondNumber = "12345678901234567";
					AssertNoMessageError("BondType not equal to 4, BondNumber is Alphanumeric Data and length equal to 17.", guarantee.PW_BondNumberInfo, errorMessageNotFlatRateVoucher);
				});
			}
		}

		public void TestCheckPW_BondNumber_WhenR0318NotActive()
		{
			var errorMessageFlatRateVoucher = "[R0318] Since Guarantee Type is \"4\" the GRN should be declared as alphanumeric data 24 characters long.";

			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				deciderTestContext.DisableRule(r => r.IsRuleR0318Active);
				guarantee.PW_BondType = GuaranteeCodes.FlatRateVoucher;
				guarantee.PW_BondNumber = "1234567890123456789012345";
				Assert($"Expected notifications would not contain {errorMessageFlatRateVoucher}", !guarantee.PW_BondNumberInfo.Notifications.Any(x => x.Message == errorMessageFlatRateVoucher));
			}
		}

		public void TestCheckPW_BondNumber_WhenIsRuleR0900_1Active()
		{
			const string messageError = "[R0900-1] Only one Guarantee must be declared when Declaration Type = TIR";

			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				var departureMovementHeader = header.MovementHeader;
				var guarantee1 = departureMovementHeader.Guarantees.AddNew();
				guarantee1.PW_BondNumber = "2222222";
				CombineAssertions(() =>
				{
					deciderTestContext.EnableRule(c => c.IsRuleR0900_1Active);
					departureMovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
					guarantee.PW_BondNumber = "1111111";
					guarantee.Validation.ValidatePW_BondNumber();
					AssertNoMessageError("When EnableRule IsRuleR0900_1Active and the declaration type is not TIR, no error", guarantee.PW_BondNumberInfo, messageError);

					departureMovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
					guarantee.Validation.ValidatePW_BondNumber();
					AssertHasMessageError("When EnableRule IsRuleR0900_1Active and the declaration type is TIR, error", guarantee.PW_BondNumberInfo, messageError);

					guarantee1.Delete();
					guarantee.Validation.ValidatePW_BondNumber();
					AssertNoMessageError("When EnableRule IsRuleR0900_1Active and the declaration type is not TIR and only one guarantee, no error", guarantee.PW_BondNumberInfo, messageError);

					deciderTestContext.DisableRule(c => c.IsRuleR0900_1Active);
					guarantee1 = header.MovementHeader.Guarantees.AddNew();
					guarantee1.PW_BondNumber = "2222222";
					guarantee.Validation.ValidatePW_BondNumber();
					AssertNoMessageError("When DisableRule IsRuleR0900_1Active and the declaration type is TIR, error", guarantee.PW_BondNumberInfo, messageError);
				});
			}
		}

		public void TestCheckPW_BondAmountValidationAtUILevel()
		{
			CombineAssertions(() =>
			{
				guarantee.PW_BondAmount = 0.1;
				guarantee.Validation.ValidatePW_BondAmount();
				AssertNoErrors(guarantee.PW_BondAmountInfo);

				guarantee.PW_BondAmount = -1;
				guarantee.Validation.ValidatePW_BondAmount();
				AssertHasErrorContaining(guarantee.PW_BondAmountInfo, MandatoryValidation.ValueCannotBeNegative);
			});
		}

		public void TestCheckPW_BondAmount_WhenIsRuleTR0055Active()
		{
			const string messageError = "[TR0055] For each guarantee of type 4, a maximum of 10,000€ can be covered";

			var guarantee1 = header.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondNumber = "2222222";
			guarantee1.PW_BondType = GuaranteeCodes.FlatRateVoucher;
			guarantee1.PW_BondAmount = 500;
			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				CombineAssertions(() =>
				{
					guarantee.PW_BondNumber = "1111111";
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0055Active));
					guarantee.PW_BondType = GuaranteeCodes.FlatRateVoucher;
					guarantee.PW_BondAmount = 9999;
					guarantee.Validation.ValidatePW_BondAmount();
					AssertNoMessageError("When EnableRule IsRuleTR0055Active and the Guarantee type is 4 and liability amount < 10000 €, no error", guarantee.PW_BondAmountInfo, messageError);

					guarantee.PW_BondAmount = 10001;
					guarantee.Validation.ValidatePW_BondAmount();
					AssertHasMessageError("When EnableRule IsRuleTR0055Active and the Guarantee type 4 and liability amount > 10000 €, error", guarantee.PW_BondAmountInfo, messageError);
					guarantee1.Validation.ValidatePW_BondAmount();
					AssertNoMessageError("Try another guarantee that does not meet the conditions when EnableRule IsRuleTR0055Active and the Guarantee type 4 and liability amount < 10000 €, no error", guarantee1.PW_BondAmountInfo, messageError);

					guarantee.PW_BondType = GuaranteeCodes.CashDepositGuarantee;
					guarantee.Validation.ValidatePW_BondAmount();
					AssertNoMessageError("When EnableRule IsRuleTR0055Active and the Guarantee type is not 4 and liability amount > 10000 €, no error", guarantee.PW_BondAmountInfo, messageError);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0055Active));
					guarantee.PW_BondType = GuaranteeCodes.FlatRateVoucher;
					guarantee.Validation.ValidatePW_BondAmount();
					AssertNoMessageError("When DisableRule IsRuleTR0055Active and the Guarantee type 4 and liability amount > 10000 €, no error", guarantee.PW_BondAmountInfo, messageError);
				});
			}
		}

		public void TestCheckPW_BondAmount_WhenIsRuleNR0064Active()
		{
			const string message = "[NR0064] You have not entered a Liability Amount.";

			using (var ruleTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				CombineAssertions(() =>
				{
					ruleTestContext.EnableRule(c => c.IsRuleNR0064Active);

					guarantee.PW_BondAmount = ZDecimal.Zero;

					foreach (var guaranteeType in new ZString[] { GuaranteeCodes.GuaranteeWaiver, GuaranteeCodes.ComprehensiveGuarantee, GuaranteeCodes.IndividualGuaranteeByGuarantor, GuaranteeCodes.CashDepositGuarantee, GuaranteeCodes.FlatRateVoucher })
					{
						guarantee.PW_BondType = guaranteeType;
						guarantee.Validation.ValidatePW_BondAmount();
						AssertHasMessageError($"Liability Amount is 0, Guarantee Type is {guaranteeType}, has message error", guarantee.PW_BondAmountInfo, message);
					}

					guarantee.PW_BondType = GuaranteeCodes.GuaranteeNotRequiredForCertainPublicBodies;
					guarantee.PW_BondAmount = ZDecimal.Zero;
					guarantee.Validation.ValidatePW_BondAmount();
					AssertNoMessageError("Guarantee Type is 8 and Liability Amount = 0, no message error", guarantee.PW_BondAmountInfo, message);

					guarantee.PW_BondAmount = 1.1234M;
					guarantee.PW_BondType = GuaranteeCodes.ComprehensiveGuarantee;
					guarantee.Validation.ValidatePW_BondAmount();
					AssertNoMessageError("Guarantee Type is 1 and Liability Amount > 0, no message error", guarantee.PW_BondAmountInfo, message);

					ruleTestContext.DisableRule(c => c.IsRuleNR0064Active);
					guarantee.PW_BondAmount = ZDecimal.Zero;
					guarantee.PW_BondType = GuaranteeCodes.ComprehensiveGuarantee;
					guarantee.Validation.ValidatePW_BondAmount();
					AssertNoMessageError("IsRuleNR0064Active disabled, no message error", guarantee.PW_BondAmountInfo, message);
				});
			}
		}

		public void TestCheckPW_Password_WhenIsRuleTR0056Active()
		{
			const string messageError = "[TR0056] You have not entered an Access Code (GAC)";

			var guarantee1 = header.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondNumber = "2222222";
			guarantee1.PW_BondType = GuaranteeCodes.FlatRateVoucher;
			guarantee1.PW_Password = "bbb";
			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				CombineAssertions(() =>
				{
					guarantee.PW_BondNumber = "1111111";
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0056Active));
					guarantee.PW_BondType = GuaranteeCodes.FlatRateVoucher;
					guarantee.PW_Password = "aaa";
					guarantee.Validation.ValidatePW_Password();
					AssertNoMessageError("When EnableRule IsRuleTR0056Active and the Guarantee type is 4 and Access Code (GAC) is not empty, no error", guarantee.PW_PasswordInfo, messageError);

					guarantee.PW_Password = ZString.Empty;
					guarantee.Validation.ValidatePW_Password();
					AssertHasMessageError("When EnableRule IsRuleTR0056Active and the Guarantee type 4 and Access Code (GAC) is empty, error", guarantee.PW_PasswordInfo, messageError);
					guarantee1.Validation.ValidatePW_Password();
					AssertNoMessageError("Try another warranty that does not meet the conditions when EnableRule IsRuleTR0056Active and the Guarantee type 4 and Access Code (GAC) is empty, no error", guarantee1.PW_PasswordInfo, messageError);

					guarantee.PW_BondType = GuaranteeCodes.CashDepositGuarantee;
					guarantee.Validation.ValidatePW_Password();
					AssertNoMessageError("When EnableRule IsRuleTR0056Active and the Guarantee type is not 4 and Access Code (GAC) is empty, no error", guarantee.PW_PasswordInfo, messageError);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0056Active));
					guarantee.PW_BondType = GuaranteeCodes.FlatRateVoucher;
					guarantee.Validation.ValidatePW_Password();
					AssertNoMessageError("When DisableRule IsRuleTR0056Active and the Guarantee type 4 and Access Code (GAC) is empty, no error", guarantee.PW_PasswordInfo, messageError);
				});
			}
		}

		public void TestCheckPW_BondTypeRulePR16()
		{
			const string messageError = "[RP16] When Authorization Code is ACR and Declaration Type is not equal to TIR, Then Guarantee Type (0 or 1 or 2 or 4 or 9) is required.";

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				CombineAssertions(() =>
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleRP16Active));
					var movementHeader = header.MovementHeader;
					movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
					guarantee.Validation.ValidatePW_BondType();
					AssertNoError("When the declaration type is TIR, no error", guarantee.PW_BondTypeInfo, messageError);

					movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
					var cusAuthorizationUsage = movementHeader.CusAuthorizationUsages.AddNew();
					guarantee.Validation.ValidatePW_BondType();
					AssertNoError("When there is no CusAuthorizationUsages' code equal to ACR", guarantee.PW_BondTypeInfo, messageError);

					cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
					guarantee.PW_BondType = GuaranteeCodes.GuaranteeWaiver;
					AssertNoError("When the guarantee type is in {0,1,2,4,9}", guarantee.PW_BondTypeInfo, messageError);

					guarantee.PW_BondType = GuaranteeCodes.GuaranteeWaived;
					AssertHasError("When the guarantee type is not in {0,1,2,4,9}", guarantee.PW_BondTypeInfo, messageError);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleRP16Active));
					guarantee.PW_BondType = GuaranteeCodes.GuaranteeWaived;
					AssertNoError("When the RuleRP16 is not Active", guarantee.PW_BondTypeInfo, messageError);
				});
			}
		}

		public void TestCheckPW_BondTypeRuleR0900()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGroup = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland, parent: euGroup);

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "CL010");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL229, "CL229");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL230, "CL230");

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "PL", "PL 2", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL229, GuaranteeCodes.ComprehensiveGuarantee, GuaranteeTypeDescriptions.ComprehensiveGuarantee, yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL230, GuaranteeCodes.GuaranteeWaiver, GuaranteeTypeDescriptions.GuaranteeWaiver, yesterday, tomorrow);
			Factory.Save();

			var movementHeader = header.MovementHeader;
			var customsOfficeTXT = movementHeader.CustomsOffices.AddNew();
			customsOfficeTXT.CY_Data = "PL00110";
			customsOfficeTXT.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;

			const string cl230ErrorMessage = "[R0900] A Guarantee of Type from CL230 list must be declared when Country of Customs Office of Departure is either from CL010 list or 'SM' or 'AD'.";
			const string cl229ErrorMessage = "[R0900] A Guarantee of Type from CL229 list must be declared when Country of Customs Office of Departure is not in (set CL010 i.e. Country Code Community, 'SM', 'AD').";
			const string tirErrorMessage = "[R0900] A TIR declaration requires a guarantee of type B (TIR).";

			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				CombineAssertions(() =>
				{
					deciderTestContext.EnableRule(x => x.IsRuleR0900Active);

					var guarantee = movementHeader.Guarantees.AddNew();

					movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
					guarantee.PW_BondType = GuaranteeCodes.GuaranteeWaiver;
					AssertHasMessageError("Declaration type = TIR, Guarantee Type <> B", guarantee.PW_BondTypeInfo, tirErrorMessage);

					guarantee.PW_BondType = GuaranteeCodes.MovementsCarriedUnderTheTirConvention;
					AssertNoMessageError("Declaration type = TIR, Guarantee Type = B", guarantee.PW_BondTypeInfo, tirErrorMessage);

					movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
					guarantee.Validation.ValidatePW_BondType();
					AssertHasMessageError("Declaration type <> TIR, A Guarantee of Type from CL230 list must be checked for Country CL010", guarantee.PW_BondTypeInfo, cl230ErrorMessage);

					customsOfficeTXT.CY_Data = "AD00110";
					guarantee.Validation.ValidatePW_BondType();
					AssertHasMessageError("Declaration type <> TIR, A Guarantee of Type from CL230 list must be checked for Country Andorra", guarantee.PW_BondTypeInfo, cl230ErrorMessage);

					customsOfficeTXT.CY_Data = "SM00110";
					guarantee.Validation.ValidatePW_BondType();
					AssertHasMessageError("Declaration type <> TIR, A Guarantee of Type from CL230 list must be checked for Country San Marino", guarantee.PW_BondTypeInfo, cl230ErrorMessage);

					customsOfficeTXT.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
					guarantee.Validation.ValidatePW_BondType();
					AssertNoMessageError("Declaration type <> TIR, R0900 should only be checked for Departure", guarantee.PW_BondTypeInfo, cl230ErrorMessage);

					customsOfficeTXT.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
					guarantee.PW_BondType = GuaranteeCodes.GuaranteeWaiver;
					AssertNoMessageError("Declaration type <> TIR, Has guarantee of type from CL230", guarantee.PW_BondTypeInfo, cl230ErrorMessage);

					customsOfficeTXT.CY_Data = "GB00110";
					guarantee.PW_BondType = GuaranteeCodes.MovementsCarriedUnderTheTirConvention;
					AssertHasMessageError("Declaration type <> TIR, A Guarantee of Type from CL229 list must be checked for Country other than CL010, AD or SM", guarantee.PW_BondTypeInfo, cl229ErrorMessage);

					guarantee.PW_BondType = GuaranteeCodes.ComprehensiveGuarantee;
					AssertNoMessageError("Has guarantee of type from CL229", guarantee.PW_BondTypeInfo, cl229ErrorMessage);

					deciderTestContext.DisableRule(x => x.IsRuleR0900Active);

					movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
					guarantee.PW_BondType = GuaranteeCodes.GuaranteeWaiver;
					AssertNoMessageError("Rule R0900 inactive, Declaration type = TIR, Guarantee Type <> B", guarantee.PW_BondTypeInfo, tirErrorMessage);

					movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
					customsOfficeTXT.CY_Data = "PL00110";
					guarantee.PW_BondType = GuaranteeCodes.MovementsCarriedUnderTheTirConvention;
					AssertNoMessageError("Rule R0900 inactive, Declaration type <> TIR, A Guarantee of Type not from CL230 for Country CL010, AD or SM", guarantee.PW_BondTypeInfo, cl230ErrorMessage);

					customsOfficeTXT.CY_Data = "GB00110";
					guarantee.Validation.ValidatePW_BondType();
					AssertNoMessageError("Rule R0900 inactive, Declaration type <> TIR, A Guarantee of Type not from CL229 list for Country other than CL010, AD or SM", guarantee.PW_BondTypeInfo, cl229ErrorMessage);
				});
			}
		}

		public void TestCheckPW_BondAmountRulePR16()
		{
			const string messageWarning = "[RP16] You have not entered Liability Amount (Guarantee Amount).";

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				CombineAssertions(() =>
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleRP16Active));
					var movementHeader = header.MovementHeader;
					movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
					guarantee.PW_BondAmount = ZDecimal.Zero;
					AssertNoWarning("When the declaration type is TIR, no error", guarantee.PW_BondAmountInfo, messageWarning);

					movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
					var cusAuthorizationUsage = movementHeader.CusAuthorizationUsages.AddNew();
					guarantee.PW_BondAmount = ZDecimal.Zero;
					AssertNoWarning("When there is no CusAuthorizationUsages' code equal to ACR", guarantee.PW_BondAmountInfo, messageWarning);

					cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
					guarantee.PW_BondAmount = 5000m;
					AssertNoWarning("When the Liability Amount is not empty", guarantee.PW_BondAmountInfo, messageWarning);

					guarantee.PW_BondAmount = ZDecimal.Zero;
					AssertHasWarning("When the Liability Amount is empty", guarantee.PW_BondAmountInfo, messageWarning);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleRP16Active));
					guarantee.PW_BondAmount = ZDecimal.Zero;
					AssertNoWarning("When the RuleRP16 is not Active", guarantee.PW_BondAmountInfo, messageWarning);
				});
			}
		}

		public void TestCheckPW_Password_C0086()
		{
			var c0086BondTypeList = new ZString[]
			{
				GuaranteeCodes.GuaranteeWaiver
				, GuaranteeCodes.ComprehensiveGuarantee
				, GuaranteeCodes.IndividualGuaranteeByGuarantor
				, GuaranteeCodes.FlatRateVoucher
				, GuaranteeCodes.IndividualGuaranteeWithMultipleUsage
			};
			var messageError = "[C0086] You have not entered a Access code (GAC).";

			CombineAssertions(() =>
			{
				foreach (var bondType in c0086BondTypeList)
				{
					guarantee.PW_BondNumber2 = "ref";
					guarantee.PW_BondType = bondType;
					guarantee.PW_Password = ZString.Empty;
					AssertHasMessageErrorContaining($"Empty BondNumber with Bond Type {bondType}", guarantee.PW_PasswordInfo, messageError);

					guarantee.PW_Password = "123";
					AssertNoMessageErrorContaining($"Not Empty BondNumber Bond Type {bondType}", guarantee.PW_PasswordInfo, messageError);
				}

				guarantee.PW_BondType = GuaranteeCodes.GuaranteeWaived;
				guarantee.PW_Password = ZString.Empty;
				AssertNoMessageErrorContaining($"Not Empty BondNumber Bond Type outside c0086 scope", guarantee.PW_PasswordInfo, messageError);
			});
		}

		public void TestCheckPW_Password_C0086_1_1() => CombineAssertions(() =>
		{
			const string messageError = "[C0086-1] You have not entered a Access code (GAC).";

			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				guarantee.PW_BondNumber2 = "ref";
				deciderTestContext.DisableRule(r => r.IsRuleC0086Active);
				deciderTestContext.EnableRule(r => r.IsRuleC0086_1Active);

				guarantee.PW_BondType = GuaranteeCodes.ComprehensiveGuarantee;
				guarantee.PW_Password = ZString.Empty;
				AssertHasMessageErrorContaining($"Empty Password with Bond Type ComprehensiveGuarantee", guarantee.PW_PasswordInfo, messageError);

				guarantee.PW_BondType = GuaranteeCodes.IndividualGuaranteeByGuarantor;
				guarantee.PW_Password = ZString.Empty;
				AssertHasMessageErrorContaining($"Empty Password with Bond Type ComprehensiveGuarantee", guarantee.PW_PasswordInfo, messageError);

				guarantee.PW_Password = "123";
				AssertNoMessageErrorContaining($"Password with Bond Type ComprehensiveGuarantee", guarantee.PW_PasswordInfo, messageError);

				guarantee.PW_BondType = GuaranteeCodes.FlatRateVoucher;
				guarantee.PW_Password = ZString.Empty;
				AssertNoMessageErrorContaining($"Empty Password with Bond Type FlatRateVoucher", guarantee.PW_PasswordInfo, messageError);

				deciderTestContext.EnableRule(r => r.IsRuleC0086Active);
				deciderTestContext.DisableRule(r => r.IsRuleC0086_1Active);

				guarantee.PW_BondType = GuaranteeCodes.ComprehensiveGuarantee;
				guarantee.PW_Password = ZString.Empty;
				AssertNoMessageErrorContaining($"Empty Password with Bond Type ComprehensiveGuarantee", guarantee.PW_PasswordInfo, messageError);

				guarantee.PW_BondType = GuaranteeCodes.IndividualGuaranteeByGuarantor;
				guarantee.PW_Password = ZString.Empty;
				AssertNoMessageErrorContaining($"Empty Password with Bond Type ComprehensiveGuarantee", guarantee.PW_PasswordInfo, messageError);
			}
		});

		public void TestCheckPW_Password_C0086_1_2() => CombineAssertions(() =>
		{
			const string messageError = "[C0086-1] Access code (GAC) must not be entered if Type is not 1 or 2.";

			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				guarantee.PW_BondNumber2 = "ref";
				deciderTestContext.DisableRule(r => r.IsRuleC0086Active);
				deciderTestContext.EnableRule(r => r.IsRuleC0086_1Active);

				guarantee.PW_BondType = GuaranteeCodes.ComprehensiveGuarantee;
				guarantee.PW_Password = "123";
				AssertNoMessageErrorContaining($"Password with Bond Type ComprehensiveGuarantee", guarantee.PW_PasswordInfo, messageError);

				guarantee.PW_BondType = GuaranteeCodes.IndividualGuaranteeByGuarantor;
				guarantee.PW_Password = "123";
				AssertNoMessageErrorContaining($"Password with Bond Type IndividualGuaranteeByGuarantor", guarantee.PW_PasswordInfo, messageError);

				guarantee.PW_BondType = GuaranteeCodes.IndividualGuaranteeByGuarantor;
				guarantee.PW_Password = ZString.Empty;
				AssertNoMessageErrorContaining($"Empty Password with Bond Type IndividualGuaranteeByGuarantor", guarantee.PW_PasswordInfo, messageError);

				guarantee.PW_BondType = GuaranteeCodes.FlatRateVoucher;
				guarantee.PW_Password = "123";
				AssertHasMessageErrorContaining($"Password with Bond Type FlatRateVoucher", guarantee.PW_PasswordInfo, messageError);

				deciderTestContext.EnableRule(r => r.IsRuleC0086Active);
				deciderTestContext.DisableRule(r => r.IsRuleC0086_1Active);

				guarantee.PW_BondType = GuaranteeCodes.ComprehensiveGuarantee;
				guarantee.PW_Password = "123";
				AssertNoMessageErrorContaining($"Password with Bond Type ComprehensiveGuarantee", guarantee.PW_PasswordInfo, messageError);

				guarantee.PW_BondType = GuaranteeCodes.IndividualGuaranteeByGuarantor;
				guarantee.PW_Password = "123";
				AssertNoMessageErrorContaining($"Password with Bond Type ComprehensiveGuarantee", guarantee.PW_PasswordInfo, messageError);
			}
		});

		public void TestCheckPW_BondNumber2_C0130_WhenActive()
		{
			const string errorMessage = "[C0130] You have not entered an Other Guarantee Reference Number.";
			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
				{
					deciderTestContext.EnableRule(c => c.IsRuleC0130Active);
					guarantee.PW_BondType = GuaranteeCodes.GuaranteeNotRequiredForCertainPublicBodies;
					guarantee.Validation.ValidatePW_BondNumber2();
					AssertHasMessageError("Guarantee's PW_BondType is 8 and PW_BondNumber2 is Empty", guarantee.PW_BondNumber2Info, errorMessage);

					guarantee.PW_BondNumber2 = "aaa";
					AssertNoMessageError("Guarantee's PW_BondType is 8 but PW_BondNumber2 is not Empty", guarantee.PW_BondNumber2Info, errorMessage);

					guarantee.PW_BondType = GuaranteeCodes.CashDepositGuarantee;
					guarantee.Validation.ValidatePW_BondNumber2();
					AssertNoMessageError("Guarantee's PW_BondType is 3 but PW_BondNumber2 is not Empty", guarantee.PW_BondNumber2Info, errorMessage);

					guarantee.PW_BondNumber2 = ZString.Empty;
					AssertNoMessageError("Guarantee's PW_BondType is 3 and PW_BondNumber2 is Empty", guarantee.PW_BondNumber2Info, errorMessage);
				}
			});
		}

		public void TestCheckPW_BondNumber2_C0130_WhenNotActive()
		{
			const string errorMessage = "[C0130] You have not entered a Other Guarantee Reference Number.";
			var allCodes = new EUNctsGuaranteeTypeList().GetAllCodes();

			var guaranteeTypeWithReference = new string[]
			{
				GuaranteeCodes.GuaranteeWaiver,
				GuaranteeCodes.ComprehensiveGuarantee,
				GuaranteeCodes.IndividualGuaranteeByGuarantor,
				GuaranteeCodes.CashDepositGuarantee,
				GuaranteeCodes.FlatRateVoucher,
				GuaranteeCodes.GuaranteeWaiverSecuredAmountNotGreaterThan500Eur,
				GuaranteeCodes.IndividualGuaranteeWithMultipleUsage,
			};

			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				deciderTestContext.DisableRule(c => c.IsRuleC0130Active);
				var actions = guaranteeTypeWithReference.SelectMany(GetTestCase).ToArray();
				foreach (var testCase in actions)
				{
					testCase?.Invoke();
					Assert($"Expected notifications would not contain {errorMessage}", !guarantee.PW_BondNumberInfo.Notifications.Any(x => x.Message == errorMessage));
				}
			}

			IEnumerable<Action> GetTestCase(string bondType)
			{
				yield return () =>
				{
					guarantee.PW_BondType = bondType;
					guarantee.PW_BondNumber2 = ZString.Empty;
				};

				yield return () =>
				{
					guarantee.PW_BondType = bondType;
					guarantee.PW_BondNumber2 = "abc";
				};
			}
		}

		public void TestCheckPW_BondNumber2_IsEmptyAndReadOnly()
		{
			var allCodes = new EUNctsGuaranteeTypeList().GetAllCodes();
			var codesRequiringBondNumber2 = allCodes.Where(code => code == GuaranteeCodes.GuaranteeNotRequiredForCertainPublicBodies || code == GuaranteeCodes.CashDepositGuarantee);
			var codesNotRequiringBondNumber2 = allCodes.Except(codesRequiringBondNumber2);
			CombineAssertions(() =>
			{
				codesRequiringBondNumber2.ForEach(code => AssertBondNumber2Editable(guarantee, code));
				codesNotRequiringBondNumber2.ForEach(code => AssertBondNumber2ReadOnly(guarantee, code));
			});

			void AssertBondNumber2Editable(NctsGuarantee guarantee, string bondType)
			{
				guarantee.PW_BondNumber2 = "ref";
				guarantee.PW_BondType = bondType;
				AssertEquals($"PW_BondType {bondType}, value", "ref", guarantee.PW_BondNumber2);
				AssertEquals($"PW_BondType {bondType}, read-only", false, guarantee.PW_BondNumber2Info.ReadOnly);
			}

			void AssertBondNumber2ReadOnly(NctsGuarantee guarantee, string bondType)
			{
				guarantee.PW_BondNumber2 = "ref";
				guarantee.PW_BondType = bondType;
				AssertEquals($"PW_BondType {bondType}, value", ZString.Empty, guarantee.PW_BondNumber2);
				AssertEquals($"PW_BondType {bondType}, read-only", true, guarantee.PW_BondNumber2Info.ReadOnly);
			}
		}

		public void TestValidateGuaranteesRuleNR0005()
		{
			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				deciderTestContext.EnableRule(r => r.IsRuleNR0005Active);
				NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				const string message = "[NR0005] The entered access code must be linked to an active contact name of the representative.";
				var movementHeader = nctsHeader.MovementHeader;
				var principalOrg = Factory.New<OrgHeader>();
				var representativeOrg = Factory.New<OrgHeader>();
				var representativeContact = representativeOrg.ContactsActive.AddNew();
				representativeContact.OC_ContactName = "Rep John Smith";
				var nctsGuarantee = movementHeader.Guarantees.AddNew();
				nctsGuarantee.PW_BondNumber = "GUA1";
				nctsGuarantee.PW_Password = "1111";
				nctsHeader.Principal.E2_OA_Address = principalOrg.MainAddress.PK;
				nctsHeader.Principal.E2_Contact = "John Smith";
				var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
				guaranteeHeader.CPH_OH_PermitHolder = principalOrg.PK;
				guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;

				foreach (var subtype in ValidationExtendMethods.GuaranteeTypesApplicable)
				{
					nctsGuarantee.PW_BondType = subtype;
					guaranteeHeader.CPH_SubType = subtype;
					guaranteeHeader.CPH_Number = "XYZ";
					guaranteeHeader.MainAccessCode = ZString.Empty;
					guaranteeHeader.MainAccessPersonName = "";
					movementHeader.Representative.E2_OA_Address = ZGuid.Empty;
					representativeContact.OC_IsActive = true;
					CombineAssertions($"GuaranteeType: {subtype}", () =>
					{
						nctsGuarantee.Validation.ValidateAll();
						AssertNoRowMessageError("There is no valid CusGuaranteeHeader to check", nctsGuarantee, message);

						guaranteeHeader.CPH_Number = "GUA1";
						nctsGuarantee.Validation.ValidateAll();
						AssertNoRowMessageError("MainAccessCode is empty, Representative not filled", nctsGuarantee, message);
						movementHeader.Representative.E2_OA_Address = representativeOrg.MainAddress.PK;
						nctsGuarantee.Validation.ValidateAll();
						AssertHasRowMessageError("MainAccessCode is empty, Representative is filled", nctsGuarantee, message);

						guaranteeHeader.MainAccessCode = "1234";
						guaranteeHeader.MainAccessPersonName = "Not John Smith";
						movementHeader.Representative.E2_OA_Address = ZGuid.Empty;
						nctsGuarantee.Validation.ValidateAll();
						AssertNoRowMessageError("OC_ContactName and MainAccessCodeRule.CPR_Description are not equal, Representative is not filled", nctsGuarantee, message);
						movementHeader.Representative.E2_OA_Address = representativeOrg.MainAddress.PK;
						nctsGuarantee.Validation.ValidateAll();
						AssertHasRowMessageError("OC_ContactName and MainAccessCodeRule.CPR_Description are not equal, Representative is filled", nctsGuarantee, message);

						guaranteeHeader.MainAccessPersonName = "Rep John Smith";
						movementHeader.Representative.E2_OA_Address = ZGuid.Empty;
						nctsGuarantee.Validation.ValidateAll();
						AssertNoRowMessageError("OC_ContactName and MainAccessCodeRule.CPR_Description are equal, Representative is not filled", nctsGuarantee, message);
						movementHeader.Representative.E2_OA_Address = representativeOrg.MainAddress.PK;
						nctsGuarantee.Validation.ValidateAll();
						AssertHasRowMessageError("OC_ContactName and MainAccessCodeRule.CPR_Description are equal, Representative is filled, Access code not equal", nctsGuarantee, message);
						guaranteeHeader.MainAccessCode = "1111";
						nctsGuarantee.Validation.ValidateAll();
						AssertNoRowMessageError("OC_ContactName and MainAccessCodeRule.CPR_Description are equal, Representative is filled, Access code is equal", nctsGuarantee, message);
						representativeContact.OC_IsActive = false;
						nctsGuarantee.Validation.ValidateAll();
						AssertHasRowMessageError("Not active Contact/OC_ContactName and MainAccessCodeRule.CPR_Description are equal, Representative is filled, Access code is equal", nctsGuarantee, message);

						var additionalAccessCode = guaranteeHeader.AdditionalAccessCodes.AddNew();
						additionalAccessCode.CPR_ValueFrom = "1234";
						additionalAccessCode.CPR_Description = "Not John Smith";
						guaranteeHeader.MainAccessPersonName = "Not John Smith";
						movementHeader.Representative.E2_OA_Address = ZGuid.Empty;
						nctsGuarantee.Validation.ValidateAll();
						AssertNoRowMessageError("OC_ContactName and MainAccessCode/AdditionalAccessCodes CPR_Description are not equal, Representative is not filled", nctsGuarantee, message);
						movementHeader.Representative.E2_OA_Address = representativeOrg.MainAddress.PK;
						nctsGuarantee.Validation.ValidateAll();
						AssertHasRowMessageError("OC_ContactName and MainAccessCode/AdditionalAccessCodes CPR_Description are not equal, Representative is filled", nctsGuarantee, message);

						representativeContact.OC_IsActive = true;
						additionalAccessCode.CPR_Description = "Rep John Smith";
						movementHeader.Representative.E2_OA_Address = ZGuid.Empty;
						nctsGuarantee.Validation.ValidateAll();
						AssertNoRowMessageError("OC_ContactName and AdditionalAccessCodes.CPR_Description are equal, Representative is not filled", nctsGuarantee, message);
						movementHeader.Representative.E2_OA_Address = representativeOrg.MainAddress.PK;
						nctsGuarantee.Validation.ValidateAll();
						AssertHasRowMessageError("OC_ContactName and AdditionalAccessCodes.CPR_Description are equal, Representative is filled, Access code not equal", nctsGuarantee, message);
						additionalAccessCode.CPR_ValueFrom = "1111";
						nctsGuarantee.Validation.ValidateAll();
						AssertNoRowMessageError("OC_ContactName and AdditionalAccessCodes.CPR_Description are equal, Representative is filled, Access code is equal", nctsGuarantee, message);
						representativeContact.OC_IsActive = false;
						nctsGuarantee.Validation.ValidateAll();
						AssertHasRowMessageError("Not active Contact/OC_ContactName and AdditionalAccessCodes.CPR_Description are equal, Representative is filled, Access code is equal", nctsGuarantee, message);
					});
				}
			}
		}

		public void TestValidateGuaranteesRuleNR0005_WhenNotActive()
		{
			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				deciderTestContext.DisableRule(r => r.IsRuleNR0005Active);
				NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				const string message = "[NR0005] The entered access code must be linked to an active contact name of the representative.";
				var movementHeader = nctsHeader.MovementHeader;
				var principalOrg = Factory.New<OrgHeader>();
				var representativeOrg = Factory.New<OrgHeader>();
				var representativeContact = representativeOrg.ContactsActive.AddNew();
				representativeContact.OC_ContactName = "Rep John Smith";
				var nctsGuarantee = movementHeader.Guarantees.AddNew();
				nctsGuarantee.PW_BondNumber = "GUA1";
				nctsGuarantee.PW_Password = "1111";
				nctsHeader.Principal.E2_OA_Address = principalOrg.MainAddress.PK;
				nctsHeader.Principal.E2_Contact = "John Smith";
				var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
				guaranteeHeader.CPH_OH_PermitHolder = principalOrg.PK;
				guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;

				foreach (var subtype in ValidationExtendMethods.GuaranteeTypesApplicable)
				{
					nctsGuarantee.PW_BondType = subtype;
					guaranteeHeader.CPH_SubType = subtype;
					guaranteeHeader.CPH_Number = "XYZ";
					guaranteeHeader.MainAccessCode = ZString.Empty;
					guaranteeHeader.MainAccessPersonName = "";
					movementHeader.Representative.E2_OA_Address = ZGuid.Empty;
					representativeContact.OC_IsActive = true;
					CombineAssertions($"GuaranteeType: {subtype}", () =>
					{
						movementHeader.Representative.E2_OA_Address = representativeOrg.MainAddress.PK;
						nctsGuarantee.Validation.ValidateAll();
						Assertion.AssertCollectionNotContains($"Expected notifications would not contain {message}", nctsGuarantee.Notifications.Select(e => e.Message), x => x.Contains(message));

						guaranteeHeader.MainAccessCode = "1234";
						guaranteeHeader.MainAccessPersonName = "Not John Smith";
						movementHeader.Representative.E2_OA_Address = representativeOrg.MainAddress.PK;
						nctsGuarantee.Validation.ValidateAll();
						Assertion.AssertCollectionNotContains($"Expected notifications would not contain {message}", nctsGuarantee.Notifications.Select(e => e.Message), x => x.Contains(message));

						guaranteeHeader.MainAccessPersonName = "Rep John Smith";
						movementHeader.Representative.E2_OA_Address = representativeOrg.MainAddress.PK;
						nctsGuarantee.Validation.ValidateAll();
						Assertion.AssertCollectionNotContains($"Expected notifications would not contain {message}", nctsGuarantee.Notifications.Select(e => e.Message), x => x.Contains(message));

						representativeContact.OC_IsActive = false;
						nctsGuarantee.Validation.ValidateAll();
						Assertion.AssertCollectionNotContains($"Expected notifications would not contain {message}", nctsGuarantee.Notifications.Select(e => e.Message), x => x.Contains(message));

						var additionalAccessCode = guaranteeHeader.AdditionalAccessCodes.AddNew();
						additionalAccessCode.CPR_ValueFrom = "1234";
						additionalAccessCode.CPR_Description = "Not John Smith";
						guaranteeHeader.MainAccessPersonName = "Not John Smith";
						movementHeader.Representative.E2_OA_Address = representativeOrg.MainAddress.PK;
						nctsGuarantee.Validation.ValidateAll();
						Assertion.AssertCollectionNotContains($"Expected notifications would not contain {message}", nctsGuarantee.Notifications.Select(e => e.Message), x => x.Contains(message));

						representativeContact.OC_IsActive = true;
						additionalAccessCode.CPR_Description = "Rep John Smith";
						movementHeader.Representative.E2_OA_Address = representativeOrg.MainAddress.PK;
						nctsGuarantee.Validation.ValidateAll();
						Assertion.AssertCollectionNotContains($"Expected notifications would not contain {message}", nctsGuarantee.Notifications.Select(e => e.Message), x => x.Contains(message));

						representativeContact.OC_IsActive = false;
						nctsGuarantee.Validation.ValidateAll();
						Assertion.AssertCollectionNotContains($"Expected notifications would not contain {message}", nctsGuarantee.Notifications.Select(e => e.Message), x => x.Contains(message));
					});
				}
			}
		}

		public void TestCheckPW_BondFiledPort_RuleNR0014()
		{
			const string expectedMessageError = "[NR0014] You have not entered";
			var targetPropertyInfo = guarantee.PW_BondFiledPortInfo;

			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				deciderTestContext.EnableRule(r => r.IsRuleNR0014Active);
				CombineAssertions("When Rule is enabled", () =>
				{
					guarantee.PW_BondFiledPort = ZString.Empty;
					guarantee.Validation.ValidatePW_BondFiledPort();
					AssertHasMessageErrorContaining("When PW_BondFiledPort is empty", targetPropertyInfo, expectedMessageError);

					guarantee.PW_BondFiledPort = "IT12211";
					guarantee.Validation.ValidatePW_BondFiledPort();
					AssertNoMessageErrorContaining("When PW_BondFiledPort is not empty", targetPropertyInfo, expectedMessageError);
				});

				deciderTestContext.DisableRule(r => r.IsRuleNR0014Active);
				guarantee.PW_BondFiledPort = ZString.Empty;
				guarantee.Validation.ValidatePW_BondFiledPort();
				AssertNoMessageErrorContaining("When PW_BondFiledPort is empty but the rule is disabled", targetPropertyInfo, expectedMessageError);
			}
		}

		public void TestCheckPW_BondFiledPort_NoErrorMessageWhenRuleC0085_2Applicable() => CombineAssertions(() =>
		{
			using (var context = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				guarantee.PW_BondType = "8";

				context.DisableRule(r => r.IsRuleC0085_2Active);
				guarantee.PW_BondFiledPort = "IT304199";
				AssertHasMessageErrorContaining("When RuleC0085_2 disabled", guarantee.PW_BondFiledPortInfo, ListValidation.InvalidCodeMessageError.ToString());

				context.EnableRule(r => r.IsRuleC0085_2Active);
				guarantee.Validation.ValidatePW_BondFiledPort();
				AssertNoMessageErrorContaining("When RuleC0085_2 Active", guarantee.PW_BondFiledPortInfo, ListValidation.InvalidCodeMessageError.ToString());
			}
		});

		public void TestCheckPW_RX_NKCurrency_RuleB1898_1()
		{
			const string expectedErrorMessage = "[B1898-1] You have not entered a Currency.";

			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				UniversalReferenceTestDataHelper.RunAssertionsInPhase5TransitionPeriod(() =>
				{
					deciderTestContext.EnableRule(r => r.IsRuleB1898_1ActiveForPW_RX_NKCurrency);
					guarantee.PW_BondAmount = 4000m;
					guarantee.PW_RX_NKCurrency = String.Empty;
					AssertHasMessageError("When Bond Amount > 0 & Currency is Empty and rule is enabled.", guarantee.PW_RX_NKCurrencyInfo, expectedErrorMessage);

					guarantee.PW_BondAmount = 4000m;
					guarantee.PW_RX_NKCurrency = "EUR";
					AssertNoMessageError("When Bond Amount > 0 & Currency has value and rule is enabled.", guarantee.PW_RX_NKCurrencyInfo, expectedErrorMessage);

					deciderTestContext.DisableRule(r => r.IsRuleB1898_1ActiveForPW_RX_NKCurrency);
					guarantee.PW_BondAmount = 4000m;
					guarantee.PW_RX_NKCurrency = String.Empty;
					AssertNoMessageError("When Bond Amount > 0 & Currency is Empty and rule is disabled.", guarantee.PW_RX_NKCurrencyInfo, expectedErrorMessage);

					guarantee.PW_BondAmount = 4000m;
					guarantee.PW_RX_NKCurrency = "EUR";
					AssertNoMessageError("When Bond Amount > 0 & Currency has value and rule is disabled.", guarantee.PW_RX_NKCurrencyInfo, expectedErrorMessage);
				});

				UniversalReferenceTestDataHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
				{
					deciderTestContext.EnableRule(c => c.IsRuleB1898_1ActiveForPW_RX_NKCurrency);
					guarantee.PW_BondAmount = 4000m;
					guarantee.PW_RX_NKCurrency = String.Empty;
					AssertNoMessageError("Outside Phase5 Transition Period - When Bond Amount > 0 & Currency is Empty.", guarantee.PW_RX_NKCurrencyInfo, expectedErrorMessage);
				});
			}
		}

		public void TestCheckPW_RX_NKCurrency_TR0089() => CombineAssertions(() =>
		{
			const string currency = "YSN";
			const string otherCurrency = "SSS";
			var message = $"[TR0089] The current Exchange Rate is missing in CW1 for Currency of Guarantee: {currency}. Please maintain the current Rate in Maintain>Reference Files>Exchange Rates.";

			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			exchangeRate.RE_ExRateType = ExchangeRateTypes.Code.CustomsRate;
			exchangeRate.RE_RX_NKExCurrency = otherCurrency;

			using (var testContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory, typeof(IRuleTR0089Decider)))
			{
				testContext.EnableRuleDecider<IRuleTR0089Decider>(x => x.IsActive);

				guarantee.PW_RX_NKCurrency = currency;
				exchangeRate.RE_RX_NKExCurrency = currency;
				header.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
				guarantee.Validation.ValidatePW_RX_NKCurrency();
				AssertNoMessageError("Specific ExchangeRate exists, no message error", guarantee.PW_RX_NKCurrencyInfo, message);

				header.EffectiveMessageStatus = "XXX";
				guarantee.Validation.ValidatePW_RX_NKCurrency();
				AssertNoMessageError($"EffectiveMessageStatus is {header.EffectiveMessageStatus} and Specific ExchangeRate exists", guarantee.PW_RX_NKCurrencyInfo, message);

				exchangeRate.RE_RX_NKExCurrency = otherCurrency;
				foreach (var status in new[] { string.Empty, NctsMessageStatusList.Codes.DepartureDeclarationNotSent, NctsMessageStatusList.Codes.Rejected, LogicalStatusList.Codes.Invalid, LogicalStatusList.Codes.Failed, LogicalStatusList.Codes.Error })
				{
					header.EffectiveMessageStatus = status;
					guarantee.Validation.ValidatePW_RX_NKCurrency();
					AssertHasMessageError($"EffectiveMessageStatus is {header.EffectiveMessageStatus} and Specific ExchangeRate not exist", guarantee.PW_RX_NKCurrencyInfo, message);
				}

				guarantee.PW_RX_NKCurrency = "EUR";
				AssertNoMessageError($"Local currrency", guarantee.PW_RX_NKCurrencyInfo, message);

				testContext.DisableRuleDecider<IRuleTR0089Decider>(x => x.IsActive);
				guarantee.PW_RX_NKCurrency = currency;
				AssertNoMessageError($"TR0089 disabled", guarantee.PW_RX_NKCurrencyInfo, message);
			}
		});

		public void TestCheckPW_BondAmount_PermitRuleNotFound()
		{
			NCTSTestHelper.AssertCheckPW_BondAmount_PermitRuleNotFound(Factory, CusInBondApplicationCodeList.Codes.NCTS5);
		}

		public void TestCheckPW_BondAmount_NR0067() => CombineAssertions(() =>
		{
			const string errorMessage = "[NR0067] You have not entered a Liability Amount.";

			var bondTypesWithMessageExpected = new[] { GuaranteeCodes.GuaranteeWaiver, GuaranteeCodes.ComprehensiveGuarantee, GuaranteeCodes.IndividualGuaranteeByGuarantor, GuaranteeCodes.FlatRateVoucher, GuaranteeCodes.IndividualGuaranteeWithMultipleUsage };
			var bondTypesWithMessageNotExpected = new EUNctsGuaranteeTypeList().GetAllCodes().Except(bondTypesWithMessageExpected);

			using (var testContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory, typeof(IRuleNR0067Decider)))
			{
				testContext.EnableRuleDecider<IRuleNR0067Decider>(x => x.IsActive);
				guarantee.PW_BondAmount = 12.3;
				AssertNoMessageError(guarantee.PW_BondAmountInfo, errorMessage);

				guarantee.PW_BondAmount = ZDecimal.Zero;
				foreach (var bondType in bondTypesWithMessageNotExpected)
				{
					guarantee.PW_BondType = bondType;
					guarantee.Validation.ValidatePW_BondAmount();
					AssertNoMessageError($"BondType={bondType}", guarantee.PW_BondAmountInfo, errorMessage);
				}
				foreach (var bondType in bondTypesWithMessageExpected)
				{
					guarantee.PW_BondType = bondType;
					guarantee.Validation.ValidatePW_BondAmount();
					AssertHasMessageError($"BondType={bondType}", guarantee.PW_BondAmountInfo, errorMessage);
				}

				testContext.DisableRuleDecider<IRuleNR0067Decider>(x => x.IsActive);
				guarantee.Validation.ValidatePW_BondAmount();
				AssertNoMessageError("NR0067 inactive", guarantee.PW_BondAmountInfo, errorMessage);
			}
		});

		public void TestCheckPW_RX_NKCurrency_B2101()
		{
			const string expectedErrorMessage = "[B2101] You have not entered a Currency";

			var currencyInfo = guarantee.PW_RX_NKCurrencyInfo;
			using (var ruleTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
				{
					ruleTestContext.ClearCachedValidationDecider(guarantee);
					ruleTestContext.DisableRule(x => x.IsRuleB2101Active);
					guarantee.PW_RX_NKCurrency = ZString.Empty;
					AssertNoMessageError("When rule B2101 is disabled", currencyInfo, expectedErrorMessage);

					CombineAssertions("When rule B2101 is enabled and not in TP", () =>
					{
						ruleTestContext.EnableRule(x => x.IsRuleB2101Active);
						guarantee.Validation.ValidatePW_RX_NKCurrency();
						AssertHasMessageError("Currency is Empty", currencyInfo, expectedErrorMessage);

						guarantee.PW_RX_NKCurrency = "INR";
						AssertNoMessageError("Currency is filled", currencyInfo, expectedErrorMessage);
					});
				}

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					guarantee.PW_RX_NKCurrency = ZString.Empty;
					AssertNoMessageError("When in TP", currencyInfo, expectedErrorMessage);
				}
			}
		}

		public void TestCheckPW_RX_NKCurrency_WhenIsRuleNR0065Active()
		{
			const string message = "[NR0065] You have not entered a Currency.";

			using (var ruleTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				CombineAssertions(() =>
				{
					ruleTestContext.EnableRule(c => c.IsRuleNR0065Active);
					guarantee.PW_RX_NKCurrency = "";
					guarantee.PW_BondAmount = 1.1234M;

					foreach (var guaranteeType in new ZString[] { GuaranteeCodes.GuaranteeWaiver, GuaranteeCodes.ComprehensiveGuarantee, GuaranteeCodes.IndividualGuaranteeByGuarantor, GuaranteeCodes.CashDepositGuarantee, GuaranteeCodes.FlatRateVoucher })
					{
						guarantee.PW_BondType = guaranteeType;
						guarantee.Validation.ValidatePW_RX_NKCurrency();
						AssertHasMessageError($"Currency is blank, Guarantee Type is {guaranteeType} and Liability Amount > 0, has message error", guarantee.PW_RX_NKCurrencyInfo, message);
					}

					guarantee.PW_BondType = GuaranteeCodes.GuaranteeNotRequiredForCertainPublicBodies;
					guarantee.Validation.ValidatePW_RX_NKCurrency();
					AssertNoMessageError("Guarantee Type is 8, no message error", guarantee.PW_RX_NKCurrencyInfo, message);

					guarantee.PW_BondType = GuaranteeCodes.GuaranteeWaiver;
					guarantee.PW_BondAmount = ZDecimal.Zero;
					guarantee.Validation.ValidatePW_RX_NKCurrency();
					AssertNoMessageError("Guarantee Type is 0 and Liability Amount = 0, no message error", guarantee.PW_RX_NKCurrencyInfo, message);

					guarantee.PW_RX_NKCurrency = "YSN";
					guarantee.PW_BondAmount = 1.1234M;
					guarantee.PW_BondType = GuaranteeCodes.ComprehensiveGuarantee;
					guarantee.Validation.ValidatePW_RX_NKCurrency();
					AssertNoMessageError("Currency is not blank, no message error", guarantee.PW_RX_NKCurrencyInfo, message);

					ruleTestContext.DisableRule(c => c.IsRuleNR0065Active);
					guarantee.PW_RX_NKCurrency = "";
					guarantee.PW_BondAmount = 1.1234M;
					guarantee.PW_BondType = GuaranteeCodes.ComprehensiveGuarantee;
					guarantee.Validation.ValidatePW_RX_NKCurrency();
					AssertNoMessageError("IsRuleNR0065Active disabled, no message error", guarantee.PW_RX_NKCurrencyInfo, message);
				});
			}
		}

		public void TestCheckPW_BondType()
		{
			guarantee.PW_BondType = "";

			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining($"When {nameof(guarantee.PW_BondType)} is empty", guarantee.PW_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
				guarantee.PW_BondType = "B";

				AssertNoMessageErrorContaining($"When {nameof(guarantee.PW_BondType)} is not empty", guarantee.PW_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckPW_BondType_R0900_2()
		{
			const string messageError = "[R0900-2] Guarantee Type B must be declared when Declaration Type = TIR";

			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				var guarantee1 = header.MovementHeader.Guarantees.AddNew();
				header.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
				guarantee1.PW_BondType = "B";
				CombineAssertions(() =>
				{
					deciderTestContext.EnableRule(c => c.IsRuleR0900_2Active);
					guarantee1.Validation.ValidatePW_BondType();
					AssertNoMessageError("When EnableRule IsRuleR0900_2Active, Guarantee Type is B and the declaration type is TIR, no error", guarantee1.PW_BondTypeInfo, messageError);

					guarantee1.PW_BondType = "A";
					guarantee1.Validation.ValidatePW_BondType();
					AssertHasMessageErrorContaining("When EnableRule IsRuleR0900_2Active, Guarantee Type is not B and the declaration type is TIR, error", guarantee1.PW_BondTypeInfo, messageError);

					deciderTestContext.DisableRule(c => c.IsRuleR0900_2Active);
					guarantee1.Validation.ValidatePW_BondType();
					AssertNoMessageError("When DisableRule IsRuleR0900_2Active, Guarantee Type is not B and the declaration type is TIR, no error", guarantee1.PW_BondTypeInfo, messageError);
				});
			}
		}

		public void TestCheckPW_BondType_R0900_3()
		{
			const string messageError = "[R0900-3] Guarantee Type B must be only declared if Declaration Type = TIR";

			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				var guarantee1 = header.MovementHeader.Guarantees.AddNew();
				header.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
				guarantee1.PW_BondType = "B";
				CombineAssertions(() =>
				{
					deciderTestContext.EnableRule(c => c.IsRuleR0900_3Active);
					guarantee1.Validation.ValidatePW_BondType();
					AssertNoMessageError("When EnableRule IsRuleR0900_3Active, Guarantee Type is B and the declaration type is TIR, no error", guarantee1.PW_BondTypeInfo, messageError);

					header.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
					guarantee1.Validation.ValidatePW_BondType();
					AssertHasMessageError("When EnableRule IsRuleR0900_3Active, Guarantee Type is B and the declaration type is not TIR, error", guarantee1.PW_BondTypeInfo, messageError);

					deciderTestContext.DisableRule(c => c.IsRuleR0900_3Active);
					guarantee1.Validation.ValidatePW_BondType();
					AssertNoMessageError("When DisableRule IsRuleR0900_3Active, Guarantee Type is B and the declaration type is not TIR, no error", guarantee1.PW_BondTypeInfo, messageError);
				});
			}
		}

		public void TestCheckPW_Bond_WhenTR0065NotActive()
		{
			const string grnNotValidMessage = "The GRN is not a valid format.";
			const string grnCheckDigitNotValidMessage = "The GRN check digit is not valid.";

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
				{
					deciderTestContext.DisableRule(r => r.IsRuleTR0065Active);
					guarantee.PW_BondNumber = "19XX0040191000015";
					Assert($"Expected notifications would not contain {grnNotValidMessage}", !guarantee.PW_BondNumberInfo.Notifications.Any(x => x.Message == grnNotValidMessage));

					guarantee.PW_BondNumber = "30CH0040191000017";
					Assert($"Expected notifications would not contain {grnCheckDigitNotValidMessage}", !guarantee.PW_BondNumberInfo.Notifications.Any(x => x.Message == grnCheckDigitNotValidMessage));
				}
			});
		}

		public void TestCheckPW_Bond_WhenTR0065Active()
		{
			const string grnNotValidMessage = "The GRN is not a valid format.";
			const string grnCheckDigitNotValidMessage = "The GRN check digit is not valid.";

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
				{
					deciderTestContext.EnableRule(r => r.IsRuleTR0065Active);
					guarantee.PW_BondNumber = "19XX0040191000015";
					AssertHasMessageErrorContaining("Third and fourth digit not a valid country", guarantee.PW_BondNumberInfo, grnNotValidMessage);

					guarantee.PW_BondNumber = "30CH0040191000017";
					AssertHasMessageErrorContaining("Check digit not valid", guarantee.PW_BondNumberInfo, grnCheckDigitNotValidMessage);

					guarantee.PW_BondNumber = "08CH1234GE0000335";
					AssertNoMessageErrorContaining("Valid GDNR", guarantee.PW_BondNumberInfo, grnCheckDigitNotValidMessage);
				}
			});
		}

		public void TestCheckPW_Status()
		{
			const string messageError = "Since creation of this Declaration one or more Exchange Rate(s) of Currencies in Goods Items might have changed. Please calculate Liability Amount with untick and tick on checkbox \"Override\" again.";

			guarantee.PW_Status = NctsGuarantee.DirtyStatus;
			AssertHasRowMessageError(guarantee, messageError);

			guarantee.PW_Status = string.Empty;
			AssertNoRowMessageError(guarantee, messageError);

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			guarantee.PW_Status = NctsGuarantee.DirtyStatus;
			AssertNoRowMessageError(guarantee, messageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			guarantee = header.MovementHeader.Guarantees.AddNew();
		}

		NctsHeader header;
		NctsGuarantee guarantee;

		void Check_TR0019_ForProperty(Func<NctsGuarantee, ZPropertyInfo> getTestPropertyInfo, string assertDescription, string errorMessage)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			NctsGuarantee AddGuarantee(string refNo, string otherRefNo)
			{
				var result = nctsHeader.MovementHeader.Guarantees.AddNew();
				result.PW_BondType = GuaranteeCodes.CashDepositGuarantee;
				result.PW_BondNumber = refNo;
				result.PW_BondNumber2 = otherRefNo;
				return result;
			}

			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				const string duplicateRefNo = "12345678901234567";
				const string duplicateOtherRefNo = "76543210987654321";

				var guarantee1 = AddGuarantee(duplicateRefNo, duplicateOtherRefNo);
				var guarantee2 = AddGuarantee("00045678901234567", "00003210987654321");
				var guarantee3 = AddGuarantee(duplicateRefNo, duplicateOtherRefNo);

				foreach (var movementTypeCode in new[] { NctsMovementType.Codes.Departure, NctsMovementType.Codes.DepartureAndArrival, NctsMovementType.Codes.Arrival })
				{
					nctsHeader.BH_HeaderType = movementTypeCode;

					deciderTestContext.EnableRule(r => r.IsRuleTR0019Active);
					guarantee2.Validation.ValidateAll();
					guarantee3.Validation.ValidateAll();
					AssertNoMessageError(assertDescription, getTestPropertyInfo(guarantee2), errorMessage);
					if (nctsHeader.IsDepartureMovement)
					{
						AssertHasMessageError(assertDescription, getTestPropertyInfo(guarantee3), errorMessage);
					}
					else
					{
						AssertNoMessageError(assertDescription, getTestPropertyInfo(guarantee3), errorMessage);
					}

					deciderTestContext.DisableRule(r => r.IsRuleTR0019Active);
					guarantee2.Validation.ValidateAll();
					guarantee3.Validation.ValidateAll();
					AssertNoMessageError(assertDescription, getTestPropertyInfo(guarantee2), errorMessage);
					AssertNoMessageError(assertDescription, getTestPropertyInfo(guarantee3), errorMessage);
				}
			}
		}

		public void TestGuarantee_TR0096RuleEnabled() => CombineAssertions(() =>
		{
			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				deciderTestContext.EnableRule(c => c.IsRuleTR0096Active);
				var errorMessage = header.Configuration.ValidationRuleConfiguration.Messages.TR0096Message;

				guarantee.Validation.ValidateAll();
				AssertNoRowMessageError("expected no error, because no Goods Item exist yet", guarantee, errorMessage);

				var bill = header.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();
				guarantee.Validation.ValidateAll();
				AssertHasRowMessageError("expected error, because no DTY record exists for the Goods Item and guarantee is not overriden", guarantee, errorMessage);

				guarantee.PW_Override = true;
				guarantee.Validation.ValidateAll();
				AssertNoRowMessageError("expected no error, because the guarantee calculation is overriden now", guarantee, errorMessage);

				guarantee.PW_Override = false;
				var fee = goodsItem.Fees.AddNew();
				fee.BFE_ChargeType = NctsCommonCargoDesc.ChargeType.Duty;
				guarantee.Validation.ValidateAll();
				AssertNoRowMessageError("expected no error, because a DTY record exists in the Goods Item Fees", guarantee, errorMessage);
			}
		});

		public void TestGuarantee_TR0096RuleDisabled()
		{
			using (var deciderTestContext = new NctsGuaranteeValidationDeciderTestContext<INctsGuaranteeDeparturePhase5ValidationDecider>(Factory))
			{
				deciderTestContext.DisableRule(c => c.IsRuleTR0096Active);
				var errorMessage = header.Configuration.ValidationRuleConfiguration.Messages.TR0096Message;

				var bill = header.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();
				guarantee.Validation.ValidateAll();
				AssertNoRowMessageError("expected no error, because the RuleTR0096 is disabled", guarantee, errorMessage);
			}
		}

		IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isTransitionPeriodActive);
	}
}
