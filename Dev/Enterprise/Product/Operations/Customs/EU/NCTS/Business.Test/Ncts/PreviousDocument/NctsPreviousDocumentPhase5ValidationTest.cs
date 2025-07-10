using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsPreviousDocumentPhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ItemNumber()
		{
			CusSupportingInfoTestHelper.AssertPropertyIsMandatoryWhenHasAttributeWithValueY(previousDocument.CSI_ItemNumberInfo, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, UniversalReferenceConstants.RefCusCodeListAttributeTypes.ItemNumber);
		}

		public void TestCheckCSI_ItemNumber_NotAnyAttribute()
		{
			CusSupportingInfoTestHelper.AssertPropertyIsNotMandatoryWhenHasNotAnyAttribute(previousDocument.CSI_ItemNumberInfo, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, UniversalReferenceConstants.RefCusCodeListAttributeTypes.ItemNumber);
		}

		public void TestCSI_Code()
		{
			NctsPreviousDocumentTestHelper.SetupCusCodes(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS,
				UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(previousDocument.CSI_CodeInfo, "BAD", "DC40N_1", "The code you have selected is not in the list.");
		}

		public void TestCheckCSI_ReferenceNumberEmptyWithRule()
		{
			previousDocument.CSI_Code = "aa";
			var warningMessage = "[G0321] You have not entered a Reference Number - '0' will be used.";

			CombineAssertions(() =>
			{
				using var ruleTestContext = new NctsPreviousDocumentValidationDeciderTestContext<INctsPreviousDocumentPhase5ValidationDecider>(Factory);
				ruleTestContext.EnableRule(x => x.IsRuleG0321Active);
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertHasWarning("The rule is active and CSI_ReferenceNumber is empty and CSI_Code is not empty", previousDocument.CSI_ReferenceNumberInfo, warningMessage);

				previousDocument.CSI_Code = ZString.Empty;
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoWarning("The rule is active and CSI_ReferenceNumber is empty and CSI_Code is empty", previousDocument.CSI_ReferenceNumberInfo, warningMessage);

				previousDocument.CSI_Code = "aa";
				previousDocument.CSI_ReferenceNumber = "ss";
				AssertNoWarning("The CSI_ReferenceNumber is not empty", previousDocument.CSI_ReferenceNumberInfo, warningMessage);

				ruleTestContext.DisableRule(x => x.IsRuleG0321Active);
				previousDocument.CSI_ReferenceNumber = ZString.Empty;
				AssertNoWarning("The rule is inactive", previousDocument.CSI_ReferenceNumberInfo, warningMessage);

				previousDocument.CSI_Code = "aa";
				previousDocument.CSI_ReferenceNumber = ZString.Empty;
				ruleTestContext.VerifyRule(x => x.IsRuleG0321Active);
				AssertNoWarning("The rule is inactive", previousDocument.CSI_ReferenceNumberInfo, warningMessage);
			});
		}

		public void TestMaxCountValidation_NctsHeader_TR0030_1() => CombineAssertions(() =>
		{
			const string message = "[TR0030-1] The maximum number of 9 Previous Documents has been exceeded.";

			using var validationDeciderTestContext = new NctsPreviousDocumentValidationDeciderTestContext<INctsPreviousDocumentDeparturePhase5ValidationDecider>(Factory);
			using (TemporarilySetTransitionPeriod(true))
			{
				AddPreviousDocuments(goodsItem.PreviousDocuments, "N1", "N2", "N3", "N4", "N5", "N6", "N7", "N8", "N9", "N10");
				AssertMessageError("TR0030_1 inactive", false, "N10");

				validationDeciderTestContext.EnableRule(decider => decider.IsRuleTR0030_1Active);

				goodsItem.PreviousDocuments.RemoveAll();
				AddPreviousDocuments(goodsItem.PreviousDocuments, "N1", "N2", "N3", "N4", "N5", "N6", "N7", "N8", "N9");
				AssertMessageError("9 NCTS documents", false, "N9");
				AddPreviousDocuments(goodsItem.PreviousDocuments, "N10");
				AssertMessageError("10 NCTS documents", true, "N10");
				AddPreviousDocuments(goodsItem.PreviousDocuments, "X11");
				AssertMessageError("10 NCTS documents (added document is not NCTS)", false, "X11");

				goodsItem.PreviousDocuments.RemoveAll();
				AddPreviousDocuments(goodsItem.PreviousDocuments, "X", "N1", "N2", "N3", "N4", "N5", "N6", "N7", "N8", "N9");
				AssertMessageError("9 NCTS documents and 1 other document", false, "N9");

				goodsItem.PreviousDocuments.RemoveAll();
				AddPreviousDocuments(header.PreviousDocuments, "N1", "X");
				AddPreviousDocuments(goodsItem.PreviousDocuments, "N2", "N3", "N4", "N5", "N6", "N7", "N8", "N9");
				AssertMessageError("9 NCTS documents (1 at header-level)", false, "N9");
				AddPreviousDocuments(goodsItem.PreviousDocuments, "N10");
				AssertMessageError("10 NCTS documents, (1 at header level)", true, "N10");

				goodsItem.PreviousDocuments.RemoveAll();
				AddPreviousDocuments(bill.PreviousDocuments, "N2", "X");
				AddPreviousDocuments(goodsItem.PreviousDocuments, "N3", "N4", "N5", "N6", "N7", "N8", "N9");
				AssertMessageError("9 NCTS documents (1 at header level, 1 at bill level)", false, "N9");
				AddPreviousDocuments(goodsItem.PreviousDocuments, "N10");
				AssertMessageError("10 NCTS documents (1 at header level, 1 at bill level)", true, "N10");

				header.PreviousDocuments.RemoveAll();
				goodsItem.PreviousDocuments.RemoveAll();
				AddPreviousDocuments(goodsItem.PreviousDocuments, "N2", "N3", "N4", "N5", "N6", "N7", "N8", "N9");
				AssertMessageError("9 NCTS documents (1 at bill level)", false, "N9");
				AddPreviousDocuments(goodsItem.PreviousDocuments, "N10");
				AssertMessageError("10 NCTS documents (1 at bill level)", true, "N10");
				validationDeciderTestContext.DisableRule(decider => decider.IsRuleTR0030_1Active);
				AssertMessageError("10 NCTS documents (1 at bill level), but rule TR0030_1 is not active", false, "N10");
			}
			using (TemporarilySetTransitionPeriod(false))
			{
				validationDeciderTestContext.EnableRule(decider => decider.IsRuleTR0030_1Active);
				goodsItem.PreviousDocuments.RemoveAll();
				AddPreviousDocuments(goodsItem.PreviousDocuments, "N1", "N2", "N3", "N4", "N5", "N6", "N7", "N8", "N9", "N10");
				AssertMessageError("Not in transition period", false, "N10");
			}

			void AddPreviousDocuments(ICusSupportingInfoCollection<CusSupportingInfo> collection, params string[] codes)
			{
				foreach (var code in codes)
				{
					collection.AddNew().CSI_Code = code;
				}
			}

			void AssertMessageError(string assertionMessage, bool expected, string code)
			{
				var previousDocument = goodsItem.PreviousDocuments.First(x => x.CSI_Code == code);
				previousDocument.Validation.ValidateAll();

				if (expected)
				{
					AssertHasRowMessageError(assertionMessage, previousDocument, message);
				}
				else
				{
					AssertNoRowMessageError(assertionMessage, previousDocument, message);
				}
			}
		});

		public void TestCheckCSI_ReferenceNumber2_MaxLength()
		{
			CombineAssertions(() =>
			{
				UniversalValidationHelperTest.AssertMaxLengthDuringTransitionPeriod(previousDocument.CSI_ReferenceNumber2Info, 26, NctsConstants.ValidationRuleMessagePrefixes.E1117);
			});
		}

		public void TestCheckCSI_UnitOfQuantity_Mandatory_C0298() => CombineAssertions(() =>
		{
			using var validationDeciderTestContext = new NctsPreviousDocumentValidationDeciderTestContext<INctsPreviousDocumentDeparturePhase5ValidationDecider>(Factory);
			validationDeciderTestContext.EnableRule(decider => decider.IsRuleC0298Active);

			const string error = "[C0298] You have not entered a Unit Qty.";

			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertNoMessageError("CSI_Quantity is 0 and CSI_UnitOfQuantity is empty", previousDocument.CSI_UnitOfQuantityInfo, error);

			previousDocument.CSI_Quantity = 10;
			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertHasMessageError("CSI_Quantity > 0 and CSI_UnitOfQuantity is empty", previousDocument.CSI_UnitOfQuantityInfo, error);

			previousDocument.CSI_UnitOfQuantity = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
			AssertNoMessageError("CSI_Quantity > 0 and CSI_UnitOfQuantity is not empty", previousDocument.CSI_UnitOfQuantityInfo, error);

			previousDocument.CSI_Quantity = 0;
			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertNoMessageError("CSI_Quantity is 0 and CSI_UnitOfQuantity is not empty", previousDocument.CSI_UnitOfQuantityInfo, error);

			validationDeciderTestContext.DisableRule(decider => decider.IsRuleC0298Active);
			previousDocument.CSI_UnitOfQuantity = ZString.Empty;
			previousDocument.CSI_Quantity = 10;
			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertNoMessageError("CSI_Quantity > 0 and CSI_UnitOfQuantity is empty", previousDocument.CSI_UnitOfQuantityInfo, error);
		});

		public void TestCheckCSI_ReferenceNumber()
		{
			using var ruleTestContext = new NctsPreviousDocumentValidationDeciderTestContext<INctsPreviousDocumentPhase5ValidationDecider>(Factory);
			CusSupportingInfoTestHelper.AssertPropertyIsMandatoryWhenHasAttributeWithValueYAndRule(
				previousDocument.CSI_ReferenceNumberInfo,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS,
				EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item,
				UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference,
				() => ruleTestContext.EnableRule(rule => rule.IsRuleG0321Active),
				() => ruleTestContext.DisableRule(rule => rule.IsRuleG0321Active));
		}

		public void TestCheckCSI_ReferenceNumber_NotAnyAttribute()
		{
			CusSupportingInfoTestHelper.AssertPropertyIsNotMandatoryWhenHasNotAnyAttribute(previousDocument.CSI_ReferenceNumberInfo, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference);
		}

		public void TestCheckCSI_ReferenceNumber_RuleNR0008() => CombineAssertions(() =>
		{
			const string messageError = "[NR0008]: MRN does not contain a valid country/region code";
			const string validMRN = "22DE587500028223M0";
			const string invalidMRN = "11ZZ11111111111110";
			var targetInfo = previousDocument.CSI_ReferenceNumberInfo;
			
			using var validationDeciderTestContext = new NctsPreviousDocumentValidationDeciderTestContext<INctsPreviousDocumentDeparturePhase5ValidationDecider>(Factory);
			validationDeciderTestContext.EnableRule(decider => decider.IsRuleNR0008Active);

			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
			previousDocument.CSI_ReferenceNumber = invalidMRN;
			AssertHasMessageError("Code 'N830', invalid MRN, Rule active", targetInfo, messageError);

			previousDocument.CSI_ReferenceNumber = validMRN;
			AssertNoMessageError("Code 'N830', valid MRN, Rule active", targetInfo, messageError);

			validationDeciderTestContext.DisableRule(decider => decider.IsRuleNR0008Active);
			previousDocument.CSI_ReferenceNumber = invalidMRN;
			AssertNoMessageError("Code 'N830', invalid MRN, Rule not active", targetInfo, messageError);

			validationDeciderTestContext.EnableRule(decider => decider.IsRuleNR0008Active);
			previousDocument.CSI_Code = "N337";
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError("Code not 'N830', invalid MRN, Rule active", targetInfo, messageError);
		});

		public void TestCheckCSI_ReferenceNumber_RuleNR0066()
		{
			using var validationDeciderTestContext = new NctsPreviousDocumentValidationDeciderTestContext<INctsPreviousDocumentDeparturePhase5ValidationDecider>(Factory);
			const string errorMessage = "[NR0066] For Type N830, Goods Item Identifier cannot be zero.";

			using (TemporarilySetTransitionPeriod(true))
			{
				CombineAssertions("In Transition Period", () =>
				{
					validationDeciderTestContext.EnableRule(decider => decider.IsRuleNR0066Active);
					previousDocument.CSI_Code = string.Empty;
					previousDocument.CSI_ItemNumber = 0;
					AssertNoMessageError("CusCode empty and ItemNumber is not zero, Rule active", previousDocument.CSI_ItemNumberInfo, errorMessage);

					previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
					previousDocument.CSI_ItemNumber = 1;
					AssertNoMessageError("CusCode N830 and ItemNumber is not zero, Rule active", previousDocument.CSI_ItemNumberInfo, errorMessage);

					previousDocument.CSI_ItemNumber = 0;
					AssertHasMessageError("CusCode N830 and ItemNumber is zero, Rule active", previousDocument.CSI_ItemNumberInfo, errorMessage);

					previousDocument.CSI_Code = "$X%";
					previousDocument.CSI_ItemNumber = 0;
					AssertNoMessageError("CusCode is not N830, Rule active", previousDocument.CSI_ItemNumberInfo, errorMessage);

					validationDeciderTestContext.DisableRule(decider => decider.IsRuleNR0066Active);
					previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
					previousDocument.CSI_ItemNumber = 0;
					AssertNoMessageError("CusCode N830 and ItemNumber is not zero, Rule not active", previousDocument.CSI_ItemNumberInfo, errorMessage);
				});

				using (TemporarilySetTransitionPeriod(false))
				{
					validationDeciderTestContext.EnableRule(decider => decider.IsRuleNR0066Active);
					previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
					previousDocument.CSI_ItemNumber = 0;
					AssertNoMessageError("Outside Transition Period - CusCode N830 and ItemNumber is zero", previousDocument.CSI_ItemNumberInfo, errorMessage);
				}
			}
		}

		public void TestValidateCSI_ItemNumberRuleG0058_1()
		{
			using var validationDeciderTestContext = new NctsPreviousDocumentValidationDeciderTestContext<INctsPreviousDocumentDeparturePhase5ValidationDecider>(Factory);
			validationDeciderTestContext.EnableRule(decider => decider.IsRuleG0058_1Active);

			CombineAssertions("Rule G0058 Active", () =>
			{
				AssertHasMessageErrorForNegativeAndZeroItemNumberWithCode(code: "C651");
				AssertHasMessageErrorForNegativeAndZeroItemNumberWithCode(code: "C658");
				AssertNoMessageErrorForNegativeAndZeroItemNumberWithCode(code: "N380");
			});

			validationDeciderTestContext.DisableRule(decider => decider.IsRuleG0058_1Active);
			CombineAssertions("Rule G0058 Inactive", () =>
			{
				AssertNoMessageErrorForNegativeAndZeroItemNumberWithCode(code: "C651");
				AssertNoMessageErrorForNegativeAndZeroItemNumberWithCode(code: "C658");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			bill = header.Bills.AddNew();
			goodsItem = bill.GoodsItems.AddNew();
			previousDocument = goodsItem.PreviousDocuments.AddNew();
		}

		IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isTransitionPeriodActive);

		void AssertHasMessageErrorForNegativeAndZeroItemNumberWithCode(string code)
		{
			previousDocument.CSI_Code = code;
			previousDocument.CSI_ItemNumber = -4;
			AssertHasMessageError($"When Code = {code} and Item number = -4", previousDocument.CSI_ItemNumberInfo, ItemNumberRuleG0058ExpectedMessage);

			previousDocument.CSI_ItemNumber = 5;
			AssertNoMessageError($"When Code = {code} and Item number = 5", previousDocument.CSI_ItemNumberInfo, ItemNumberRuleG0058ExpectedMessage);

			previousDocument.CSI_ItemNumber = 0;
			AssertHasMessageError($"When Code = {code} and Item number = 0", previousDocument.CSI_ItemNumberInfo, ItemNumberRuleG0058ExpectedMessage);
		}

		void AssertNoMessageErrorForNegativeAndZeroItemNumberWithCode(string code)
		{
			previousDocument.CSI_Code = code;
			previousDocument.CSI_ItemNumber = -4;
			AssertNoMessageError($"When Code = {code} and Item number = -4", previousDocument.CSI_ItemNumberInfo, ItemNumberRuleG0058ExpectedMessage);

			previousDocument.CSI_ItemNumber = 5;
			AssertNoMessageError($"When Code = {code} and Item number = 5", previousDocument.CSI_ItemNumberInfo, ItemNumberRuleG0058ExpectedMessage);

			previousDocument.CSI_ItemNumber = 0;
			AssertNoMessageError($"When Code = {code} and Item number = 0", previousDocument.CSI_ItemNumberInfo, ItemNumberRuleG0058ExpectedMessage);
		}

		NctsHeader header;
		NctsBill bill;
		NctsDepartureCargoDesc goodsItem;
		NctsPreviousDocument previousDocument;

		const string ItemNumberRuleG0058ExpectedMessage = "[G0058-1] 'Goods Item Identifier' field must be filled with the Unique Body Reference (UBR).";
	}
}
