using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CommonPreviousDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber()
		{
			using var ruleTestContext = new CommonPreviousDocumentValidationDeciderTestContext<ICommonPreviousDocumentValidationDecider>(Factory);
			CusSupportingInfoTestHelper.AssertPropertyIsMandatoryWhenHasAttributeWithValueYAndRule(
				previousDocument.CSI_ReferenceNumberInfo,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS,
				"House",
				"Reference",
				() => ruleTestContext.EnableRule(decider => decider.IsRuleG0321Active),
				() => ruleTestContext.DisableRule(decider => decider.IsRuleG0321Active));
		}

		public void TestCheckCSI_ReferenceNumber2()
		{
			AssertPropertyIsMandatoryWhenHasAttributeWithValueY(previousDocument.CSI_ReferenceNumber2Info, "Complement");
		}

		public void TestCheckCSI_ReferenceNumber2_MaxLength()
		{
			CombineAssertions(() =>
			{
				UniversalValidationHelperTest.AssertMaxLengthDuringTransitionPeriod(previousDocument.CSI_ReferenceNumber2Info, 26);
			});
		}

		public void TestCheckCSI_SubType()
		{
			previousDocument.Validation.ValidateCSI_SubType();
			AssertNoNotifications("CSI_SubType is not in GUI so shouldn't be validated", previousDocument.CSI_SubTypeInfo);
		}

		public void TestCheckCSI_ReferenceNumber_R0416()
		{
			var messageError = "[R0416] The 17th character of the house consignment level previous document's reference number must be one of these - A or B or E.";
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var nctsHeaderPreviousDocument = nctsHeader.PreviousDocuments.AddNew();
			var propertyInfo = previousDocument.CSI_ReferenceNumberInfo;

			CombineAssertions(() =>
			{
				using var testContext = new CommonPreviousDocumentValidationDeciderTestContext<ICommonPreviousDocumentDepartureValidationDecider>(Factory);
				testContext.EnableRule(decider => decider.IsRuleR0416Active);

				AssertNoMessageError("Empty Reference Number", propertyInfo, messageError);

				previousDocument.CSI_ReferenceNumber = "1234567890123456";
				AssertNoMessageError("16 character long", propertyInfo, messageError);

				previousDocument.CSI_ReferenceNumber = "12345678901234567";
				AssertHasMessageError("17th character is not A or B or E", propertyInfo, messageError);

				char[] abe = ['A', 'B', 'E'];
				foreach (var character in abe)
				{
					previousDocument.CSI_ReferenceNumber = $"1234567890123456{character}";
					AssertNoMessageError($"17th character is {character}", propertyInfo, messageError);
				}

				char[] characters = ['A', 'B', 'C', 'D', 'E', 'F', '7'];
				foreach (var character in characters)
				{
					nctsHeaderPreviousDocument.CSI_ReferenceNumber = $"1234567890123456{character}";
					AssertNoMessageError($"R0416 applies only to Bill. 17th character is {character}", nctsHeaderPreviousDocument.CSI_ReferenceNumberInfo, messageError);
				}

				testContext.DisableRule(decider => decider.IsRuleR0416Active);
				foreach (var character in characters)
				{
					previousDocument.CSI_ReferenceNumber = $"1234567890123456{character}";
					AssertNoMessageError($"Rule is not active: 17th character is {character}", propertyInfo, messageError);
				}
			});
		}

		public void TestCheckCSI_ReferenceNumber_RuleNR0008() => CombineAssertions(() =>
		{
			const string messageError = "[NR0008]: MRN does not contain a valid country/region code";
			const string validMRN = "22DE587500028223M0";
			const string invalidMRN = "11ZZ11111111111110";

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var targetInfo = previousDocument.CSI_ReferenceNumberInfo;

			using var ruleTestContext = new CommonPreviousDocumentValidationDeciderTestContext<ICommonPreviousDocumentDepartureValidationDecider>(Factory);
			ruleTestContext.EnableRule(decider => decider.IsRuleNR0008Active);
			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
			previousDocument.CSI_ReferenceNumber = invalidMRN;
			AssertHasMessageError("Code 'N830', invalid MRN, Rule active", targetInfo, messageError);

			previousDocument.CSI_ReferenceNumber = validMRN;
			AssertNoMessageError("Code 'N830', valid MRN, Rule active", targetInfo, messageError);

			ruleTestContext.DisableRule(decider => decider.IsRuleNR0008Active);
			previousDocument.CSI_ReferenceNumber = invalidMRN;
			AssertNoMessageError("Code 'N830', invalid MRN, Rule not active", targetInfo, messageError);

			ruleTestContext.EnableRule(decider => decider.IsRuleNR0008Active);
			previousDocument.CSI_Code = "N355";
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError("Code not 'N830', invalid MRN, Rule active", targetInfo, messageError);
		});

		public void TestCheckCSI_ReferenceNumberEmptyWithRule()
		{
			previousDocument.CSI_Code = "aa";
			var warningMessage = "[G0321] You have not entered a Reference Number - '0' will be used.";

			CombineAssertions(() =>
			{
				using var ruleTestContext = new CommonPreviousDocumentValidationDeciderTestContext<ICommonPreviousDocumentDepartureValidationDecider>(Factory);
				ruleTestContext.ClearCachedValidationDecider(previousDocument);
				ruleTestContext.EnableRule(decider => decider.IsRuleG0321Active);
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertHasWarning("The rule is active and CSI_ReferenceNumber is empty and CSI_Code is not empty", previousDocument.CSI_ReferenceNumberInfo, warningMessage);

				previousDocument.CSI_Code = ZString.Empty;
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoWarning("The rule is active and CSI_ReferenceNumber is empty and CSI_Code is empty", previousDocument.CSI_ReferenceNumberInfo, warningMessage);

				previousDocument.CSI_Code = "aa";
				previousDocument.CSI_ReferenceNumber = "ss";
				AssertNoWarning("The CSI_ReferenceNumber is not empty", previousDocument.CSI_ReferenceNumberInfo, warningMessage);

				ruleTestContext.DisableRule((decider => decider.IsRuleG0321Active));
				previousDocument.CSI_ReferenceNumber = ZString.Empty;
				AssertNoWarning("The rule is inactive", previousDocument.CSI_ReferenceNumberInfo, warningMessage);

				previousDocument.CSI_Code = "aa";
				previousDocument.CSI_ReferenceNumber = ZString.Empty;
				AssertNoWarning("The rule is inactive", previousDocument.CSI_ReferenceNumberInfo, warningMessage);
			});
		}

		public void TestCheckCSI_Code_RuleE1301_Header()
		{
			AssertCheckCSICodeRuleE1301(previousDocumentHeader);
		}

		public void TestCheckCSI_Code_RuleE1301_Bills()
		{
			AssertCheckCSICodeRuleE1301(previousDocument);
		}

		public void TestMaxCountValidation_NctsHeader_TR0030_1() => CombineAssertions(() =>
		{
			AddPreviousDocuments(nctsHeader.PreviousDocuments, "N1", "N2", "N3", "N4", "N5", "N6", "N7", "N8", "N9", "N10");
			AssertMessageError("TR0030_1 inactive", false, "N10");

			using var ruleTestContext = new CommonPreviousDocumentValidationDeciderTestContext<ICommonPreviousDocumentValidationDecider>(Factory);
			using (TemporarilySetTransitionPeriod(true))
			{
				AddPreviousDocuments(nctsHeader.PreviousDocuments, "N1", "N2", "N3", "N4", "N5", "N6", "N7", "N8", "N9", "N10");
				AssertMessageError("TR0030_1 inactive", false, "N10");

				ruleTestContext.EnableRule(decider => decider.IsRuleTR0030_1Active);
				nctsBill.PreviousDocuments.RemoveAll();

				nctsHeader.PreviousDocuments.RemoveAll();
				AddPreviousDocuments(nctsHeader.PreviousDocuments, "N1", "N2", "N3", "N4", "N5", "N6", "N7", "N8", "N9");
				AssertMessageError("9 NCTS documents", false, "N9");
				AddPreviousDocuments(nctsHeader.PreviousDocuments, "N10");
				AssertMessageError("10 NCTS documents", true, "N10");
				AddPreviousDocuments(nctsHeader.PreviousDocuments, "X11");
				AssertMessageError("10 NCTS documents (added document is not NCTS)", false, "X11");

				nctsHeader.PreviousDocuments.RemoveAll();
				AddPreviousDocuments(nctsHeader.PreviousDocuments, "X", "N1", "N2", "N3", "N4", "N5", "N6", "N7", "N8", "N9");
				AssertMessageError("9 NCTS documents and 1 other document", false, "N9");
				AddPreviousDocuments(nctsHeader.PreviousDocuments, "N10");
				AssertMessageError("10 NCTS documents and 1 other document", true, "N10");

				var nctsBill2 = nctsHeader.Bills.AddNew();

				nctsHeader.PreviousDocuments.RemoveAll();
				AddPreviousDocuments(nctsBill.PreviousDocuments, "N1a");
				AddPreviousDocuments(nctsBill2.PreviousDocuments, "N1", "N2", "X");
				AddPreviousDocuments(nctsHeader.PreviousDocuments, "N3", "N4", "N5", "N6", "N7", "N8", "N9");
				AssertMessageError("9 NCTS documents (max. 2 at bill level)", false, "N9");
				AddPreviousDocuments(nctsHeader.PreviousDocuments, "N10");
				AssertMessageError("10 NCTS documents (max. 2 at bill level)", true, "N10");

				var goodsItem1 = nctsBill.GoodsItems.AddNew();
				var goodsItem2 = nctsBill.GoodsItems.AddNew();

				nctsHeader.PreviousDocuments.RemoveAll();
				AddPreviousDocuments(goodsItem1.PreviousDocuments, "N3a");
				AddPreviousDocuments(goodsItem2.PreviousDocuments, "N3b", "N3c");
				AddPreviousDocuments(nctsHeader.PreviousDocuments, "N4", "N5", "N6", "N7", "N8", "N9");
				AssertMessageError("9 NCTS documents (max. 3 at bill level + goods item level)", false, "N9");
				AddPreviousDocuments(nctsHeader.PreviousDocuments, "N10");
				AssertMessageError("10 NCTS documents (max. 3 at bill level + goods item level)", true, "N10");
			}

			using (TemporarilySetTransitionPeriod(false))
			{
				nctsHeader.PreviousDocuments.RemoveAll();
				AddPreviousDocuments(nctsHeader.PreviousDocuments, "N1", "N2", "N3", "N4", "N5", "N6", "N7", "N8", "N9", "N10");
				AssertMessageError("Not in transition period", false, "N10");
			}

			void AssertMessageError(string assertionMessage, bool expected, string code)
			{
				AssertMessageErrorTR0030_1(assertionMessage, expected, nctsHeader.PreviousDocuments, code);
			}
		});

		public void TestMaxCountValidation_NctsBill_TR0030_1() => CombineAssertions(() =>
		{
			using var ruleTestContext = new CommonPreviousDocumentValidationDeciderTestContext<ICommonPreviousDocumentValidationDecider>(Factory);
			using (TemporarilySetTransitionPeriod(true))
			{
				AddPreviousDocuments(nctsBill.PreviousDocuments, "N1", "N2", "N3", "N4", "N5", "N6", "N7", "N8", "N9", "N10");
				AssertMessageError("TR0030_1 inactive", false, "N10");

				ruleTestContext.EnableRule(decider => decider.IsRuleTR0030_1Active);
				nctsHeader.PreviousDocuments.RemoveAll();

				nctsBill.PreviousDocuments.RemoveAll();
				AddPreviousDocuments(nctsBill.PreviousDocuments, "N1", "N2", "N3", "N4", "N5", "N6", "N7", "N8", "N9");
				AssertMessageError("9 NCTS documents", false, "N9");
				AddPreviousDocuments(nctsBill.PreviousDocuments, "N10");
				AssertMessageError("10 NCTS documents", true, "N10");
				AddPreviousDocuments(nctsBill.PreviousDocuments, "X11");
				AssertMessageError("10 NCTS documents (added document is not NCTS)", false, "X11");

				nctsBill.PreviousDocuments.RemoveAll();
				AddPreviousDocuments(nctsBill.PreviousDocuments, "X", "N1", "N2", "N3", "N4", "N5", "N6", "N7", "N8", "N9");
				AssertMessageError("9 NCTS documents and 1 other document", false, "N9");
				AddPreviousDocuments(nctsBill.PreviousDocuments, "N10");
				AssertMessageError("10 NCTS documents and 1 other document", true, "N10");

				nctsBill.PreviousDocuments.RemoveAll();
				AddPreviousDocuments(nctsHeader.PreviousDocuments, "N1", "X");
				AddPreviousDocuments(nctsBill.PreviousDocuments, "N2", "N3", "N4", "N5", "N6", "N7", "N8", "N9");
				AssertMessageError("9 NCTS documents (1 at header level)", false, "N9");
				AddPreviousDocuments(nctsBill.PreviousDocuments, "N10");
				AssertMessageError("10 NCTS documents ( 1 at header level)", true, "N10");

				var goodsItem1 = nctsBill.GoodsItems.AddNew();
				var goodsItem2 = nctsBill.GoodsItems.AddNew();

				nctsBill.PreviousDocuments.RemoveAll();
				nctsHeader.PreviousDocuments.RemoveAll();
				AddPreviousDocuments(goodsItem1.PreviousDocuments, "N1a", "X");
				AddPreviousDocuments(goodsItem2.PreviousDocuments, "N1", "N2", "X");
				AddPreviousDocuments(nctsBill.PreviousDocuments, "N3", "N4", "N5", "N6", "N7", "N8", "N9");
				AssertMessageError("9 NCTS documents (max. 2 at goods item level)", false, "N9");
				AddPreviousDocuments(nctsBill.PreviousDocuments, "N10");
				AssertMessageError("10 NCTS documents (max. 2 at goods item level)", true, "N10");

				nctsHeader.PreviousDocuments.RemoveAll();
				nctsBill.PreviousDocuments.RemoveAll();
				AddPreviousDocuments(nctsHeader.PreviousDocuments, "N3", "X");
				AddPreviousDocuments(nctsBill.PreviousDocuments, "N4", "N5", "N6", "N7", "N8", "N9");
				AssertMessageError("9 NCTS documents (max. 2 at goods item level, 1 at header level)", false, "N9");
				AddPreviousDocuments(nctsBill.PreviousDocuments, "N10");
				AssertMessageError("10 NCTS documents (max. 2 at goods item level, 1 at header level)", true, "N10");
			}
			using (TemporarilySetTransitionPeriod(false))
			{
				nctsBill.PreviousDocuments.RemoveAll();
				AddPreviousDocuments(nctsBill.PreviousDocuments, "N1", "N2", "N3", "N4", "N5", "N6", "N7", "N8", "N9", "N10");
				AssertMessageError("Not in transition period", false, "N10");
			}

			void AssertMessageError(string assertionMessage, bool expected, string code)
			{
				AssertMessageErrorTR0030_1(assertionMessage, expected, nctsBill.PreviousDocuments, code);
			}
		});

		public void TestCheckCSI_Code_BillsRuleG0026_1_WhenAdditionalDeclarationTypeIsA()
		{
			const string expectedError = "[G0026-1] House Consignment > Previous Documents > can be used only if Additional Declaration Type = 'A'.";
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var movementHeader = nctsHeader.MovementHeader;
			var currentPreviousDocument = previousDocument;

			using var ruleTestContext = new CommonPreviousDocumentValidationDeciderTestContext<ICommonPreviousDocumentDepartureValidationDecider>(Factory);
			CombineAssertions("RuleG0026_1 is Disabled", () =>
			{
				ruleTestContext.DisableRule(x => x.IsRuleG0026_1Active);
				movementHeader.BM_AdditionalDeclarationType = ZString.Empty;
				currentPreviousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;

				AssertNoMessageErrorContaining("Departure, code is N830, declaration type is Empty", currentPreviousDocument.CSI_CodeInfo, expectedError);
			});

			CombineAssertions("RuleG0026_1 is Active", () =>
			{
				ruleTestContext.EnableRule(x => x.IsRuleG0026_1Active);

				var headerArrival = Factory.New<NctsHeader>();
				headerArrival.SetMovementType(NctsMovementType.Codes.Arrival);
				var previousDocumentArrival = headerArrival.Bills.AddNew().PreviousDocuments.AddNew();

				previousDocumentArrival.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
				AssertNoMessageErrorContaining("Arrival, code is N830, declaration type is Empty", previousDocumentArrival.CSI_CodeInfo, expectedError);

				movementHeader.BM_AdditionalDeclarationType = ZString.Empty;
				currentPreviousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
				AssertHasMessageErrorContaining("Departure, code is N830, declaration type is Empty", currentPreviousDocument.CSI_CodeInfo, expectedError);

				movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
				currentPreviousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
				AssertNoMessageErrorContaining("Departure, code is N830, declaration type is A", currentPreviousDocument.CSI_CodeInfo, expectedError);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				movementHeader.BM_AdditionalDeclarationType = ZString.Empty;
				currentPreviousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
				AssertNoMessageErrorContaining("NCTS4, Departure, code is N830, declaration type is Empty", currentPreviousDocument.CSI_CodeInfo, expectedError);
			});
		}

		public void TestCheckCSI_Code_BillsRuleG0026_1_WhenDocTypeIsN830()
		{
			const string expectedError = "[G0026-1] House Consignment > Previous Document > Type > can only be N830.";
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			var currentPreviousDocument = previousDocument;

			using var ruleTestContext = new CommonPreviousDocumentValidationDeciderTestContext<ICommonPreviousDocumentDepartureValidationDecider>(Factory);
			CombineAssertions("RuleG0026_1 is Disabled", () =>
			{
				ruleTestContext.DisableRule(x => x.IsRuleG0026_1Active);
				currentPreviousDocument.CSI_Code = UniversalReferenceConstants.PreviousDocumentTypes.CommercialInvoice;
				AssertNoMessageErrorContaining("Departure, code is not N830, declaration type is A", currentPreviousDocument.CSI_CodeInfo, expectedError);
			});

			CombineAssertions("RuleG0026_1 is Active", () =>
			{
				ruleTestContext.EnableRule(x => x.IsRuleG0026_1Active);

				var headerArrival = Factory.New<NctsHeader>();
				headerArrival.SetMovementType(NctsMovementType.Codes.Arrival);
				var previousDocumentArrival = headerArrival.Bills.AddNew().PreviousDocuments.AddNew();
				headerArrival.ArrivalMovementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;

				previousDocumentArrival.CSI_Code = UniversalReferenceConstants.PreviousDocumentTypes.CommercialInvoice;
				AssertNoMessageErrorContaining("Arrival, code is not N830, declaration type is A", previousDocumentArrival.CSI_CodeInfo, expectedError);

				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				currentPreviousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
				AssertNoMessageErrorContaining("Departure, code is N830, declaration type is A", currentPreviousDocument.CSI_CodeInfo, expectedError);

				currentPreviousDocument.CSI_Code = UniversalReferenceConstants.PreviousDocumentTypes.CommercialInvoice;
				AssertHasMessageErrorContaining("Departure, code is not N830, declaration type is A", currentPreviousDocument.CSI_CodeInfo, expectedError);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

				currentPreviousDocument.CSI_Code = UniversalReferenceConstants.PreviousDocumentTypes.CommercialInvoice;
				AssertNoMessageErrorContaining("NCTS4, Departure, code is not N830, declaration type is A", currentPreviousDocument.CSI_CodeInfo, expectedError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			previousDocumentHeader = nctsHeader.PreviousDocuments.AddNew();
			nctsBill = nctsHeader.Bills.AddNew();
			previousDocument = nctsBill.PreviousDocuments.AddNew();
		}

		void AddPreviousDocuments(ICusSupportingInfoCollection<CusSupportingInfo> collection, params string[] codes)
		{
			foreach (var code in codes)
			{
				collection.AddNew().CSI_Code = code;
			}
		}

		void AssertMessageErrorTR0030_1(string assertionMessage, bool expected, ICusSupportingInfoCollection<CusSupportingInfo> collection, string code)
		{
			var previousDocument = collection.First(x => x.CSI_Code == code);
			previousDocument.Validation.ValidateAll();

			if (expected)
			{
				AssertHasRowMessageError(assertionMessage, previousDocument, messageTR0030_1);
			}
			else
			{
				AssertNoRowMessageError(assertionMessage, previousDocument, messageTR0030_1);
			}
		}

		void AssertPropertyIsMandatoryWhenHasAttributeWithValueY(ZPropertyInfo propertyInfo, string attributeName)
		{
			CusSupportingInfoTestHelper.AssertPropertyIsMandatoryWhenHasAttributeWithValueY(propertyInfo, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS, "House", attributeName);
		}

		void AssertCheckCSICodeRuleE1301(CommonPreviousDocument previousDocument)
		{
			const string error = "[E1301] In transition period, which is now, Previous Document must be empty";

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			using var ruleTestContext = new CommonPreviousDocumentValidationDeciderTestContext<ICommonPreviousDocumentDepartureValidationDecider>(Factory);
			{
				using (TemporarilySetTransitionPeriod(true))
				{
					ruleTestContext.EnableRule(x => x.IsRuleE1301Active);
					CombineAssertions("When TP: ON, NCTS5, RuleE1301: Active", () =>
					{
						previousDocument.CSI_Code = ZString.Empty;
						AssertNoMessageError("CSI_Code: Empty", previousDocument.CSI_CodeInfo, error);

						previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
						AssertHasMessageError("CSI_Code: N830", previousDocument.CSI_CodeInfo, error);
					});

					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					CombineAssertions("When TP: ON, NCTS4, RuleE1301: Active", () =>
					{
						previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
						AssertNoMessageError("CSI_Code: N830", previousDocument.CSI_CodeInfo, error);
					});

					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					ruleTestContext.DisableRule(x => x.IsRuleE1301Active);
					CombineAssertions("When TP: ON, NCTS5, RuleE1301: Inactive", () =>
					{
						previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
						AssertNoMessageError("CSI_Code: N830", previousDocument.CSI_CodeInfo, error);
					});
				}

				using (TemporarilySetTransitionPeriod(false))
				{
					ruleTestContext.EnableRule(x => x.IsRuleE1301Active);
					CombineAssertions("When TP: OFF, NCTS5, RuleE1301: Active", () =>
					{
						previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
						AssertNoMessageError("CSI_Code: N830", previousDocument.CSI_CodeInfo, error);
					});
				}
			}
		}

		IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isTransitionPeriodActive);

		CommonPreviousDocument previousDocument;
		CommonPreviousDocument previousDocumentHeader;
		NctsHeader nctsHeader;
		NctsBill nctsBill;

		const string messageTR0030_1 = "[TR0030-1] The maximum number of 9 Previous Documents has been exceeded.";
	}
}
