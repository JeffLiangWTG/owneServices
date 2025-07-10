using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static Enterprise.Core.Constants.CountryCodes;
using static Enterprise.Customs.EU.Business.OfficeCodes_NCTS.Codes;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes;
using static Enterprise.Customs.Universal.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsEuOfficeCodePhase5DepartureValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB1836_WarningMessage()
		{
			var movementHeader = header.MovementHeader;
			var customsOffice = movementHeader.CustomsOffices.AddNew();

			const string warningMessage = "[B1836] Customs Office with Purpose='TRA' is not required for a 'TIR' Declaration.";

			using (var ruleContext = new NctsEuOfficeCodeValidationDeciderTestContext<INctsEuOfficeCodeDeparturePhase5ValidationDecider>(Factory))
			{
				ruleContext.EnableRule(c => c.IsRuleB1836Active);
				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2F;
					customsOffice.CY_Code = NCTSOfficeOfTransit;
					AssertNoWarning("Declaration type <> TIR.", customsOffice.CY_CodeInfo, warningMessage);

					movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
					customsOffice.CY_Code = NCTSOfficeOfDeparture;
					AssertNoWarning("Customs Office with Purpose <> TRA.", customsOffice.CY_CodeInfo, warningMessage);

					customsOffice.CY_Code = NCTSOfficeOfTransit;
					AssertHasWarning("Declaration type = TIR and Customs Office with Purpose = TRA.", customsOffice.CY_CodeInfo, warningMessage);
				}

				ruleContext.DisableRule(c => c.IsRuleB1836Active);

				customsOffice.Validation.ValidateCY_Code();
				AssertNoWarning("The rule B1836 is inactive.", customsOffice.CY_CodeInfo, warningMessage);
			}
		}

		public void TestCheckB1836_ErrorMessage()
		{
			using (var ruleContext = new NctsEuOfficeCodeValidationDeciderTestContext<INctsEuOfficeCodeDeparturePhase5ValidationDecider>(Factory))
			{
				ruleContext.EnableRule(c => c.IsRuleB1836Active);
				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					UniversalReferenceTestDataHelper.EnsureCountriesAreInRefCusCodeList(Factory, Code_CL112, (Poland, nameof(Poland)));
					Factory.Save();

					var movementHeader = header.MovementHeader;
					movementHeader.CustomsOffices.RemoveAndDeleteAll();
					var departureOffice = movementHeader.CustomsOffices.AddNew(NCTSOfficeOfDeparture, Australia);
					var destinationOffice = movementHeader.CustomsOffices.AddNew(NCTSOfficeOfDestination, Australia);
					var customsOffice = movementHeader.CustomsOffices.AddNew();

					const string errorMessage = "[B1836] Customs Office with Purpose='TRA' is required.";

					const string officeCodeThatStartsFromPL = "PL000001";
					movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2F;
					customsOffice.CY_Code = ZString.Empty;

					foreach (var testOffice in new[] { destinationOffice, departureOffice })
					{
						var officeCode = testOffice.CY_Code;
						var officeCodeInfo = testOffice.CY_CodeInfo;

						testOffice.CY_Data = ZString.Empty;
						testOffice.Validation.ValidateCY_Code();
						AssertNoMessageError($"Office with code {officeCode} is not in CL112.", officeCodeInfo, errorMessage);

						testOffice.CY_Data = officeCodeThatStartsFromPL;
						testOffice.Validation.ValidateCY_Code();
						AssertHasMessageError($"Office with code {officeCode} is in CL112.", officeCodeInfo, errorMessage);

						customsOffice.CY_Code = NCTSOfficeOfTransit;
						testOffice.Validation.ValidateCY_Code();
						AssertNoMessageError($"Office with code {officeCode} is in CL112 but Office with code TRA is provided.", officeCodeInfo, errorMessage);

						customsOffice.CY_Code = ZString.Empty;
						movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
						testOffice.Validation.ValidateCY_Code();
						AssertNoMessageError($"Office with code {officeCode} is in CL112 but Declaration type is TIR.", officeCodeInfo, errorMessage);

						ruleContext.DisableRule(c => c.IsRuleB1836Active);

						movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2F;
						testOffice.Validation.ValidateCY_Code();
						AssertNoMessageError($"Office with code {officeCode}: The rule B1836 is inactive.", officeCodeInfo, errorMessage);

						ruleContext.EnableRule(c => c.IsRuleB1836Active);
					}
				}
			}
		}

		public void TestCheckCY_Data_RuleC0030_1()
		{
			const string expectedErrorMessage = "[C0030-1] Customs Office of Transit cannot be used for Declaration TIR or T2SM";
			using (var ruleContext = new NctsEuOfficeCodeValidationDeciderTestContext<INctsEuOfficeCodeDeparturePhase5ValidationDecider>(Factory))
			{
				ruleContext.EnableRule(c => c.IsRuleC0030_1Active);

				var movementHeader = header.MovementHeader;
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				movementHeader.CustomsOffices.RemoveAll();

				var transitOffice = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
				var cY_DataInfo = transitOffice.CY_DataInfo;

				CombineAssertions(() =>
				{
					movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
					transitOffice.CY_Data = "IT275100";
					AssertHasMessageErrorContaining($"Declaration Type is {movementHeader.BM_InBondEntryType} and CustomsOfficeOfTransit is filled", cY_DataInfo, expectedErrorMessage);

					movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2SM;
					AssertHasMessageErrorContaining($"Declaration Type is {movementHeader.BM_InBondEntryType} and CustomsOfficeOfTransit is filled", cY_DataInfo, expectedErrorMessage);

					transitOffice.CY_Data = ZString.Empty;

					movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
					AssertNoMessageErrorContaining($"Declaration Type is {movementHeader.BM_InBondEntryType} and CustomsOfficeOfTransit is NOT filled", cY_DataInfo, expectedErrorMessage);

					movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2SM;
					AssertNoMessageErrorContaining($"Declaration Type is {movementHeader.BM_InBondEntryType} and CustomsOfficeOfTransit is NOT filled", cY_DataInfo, expectedErrorMessage);

					movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
					transitOffice.CY_Data = "IT275100";
					AssertNoMessageErrorContaining($"Declaration Type is NOT IN (TIR, T2SM) {movementHeader.BM_InBondEntryType} and CustomsOfficeOfTransit is filled", cY_DataInfo, expectedErrorMessage);

					ruleContext.DisableRule(c => c.IsRuleC0030_1Active);

					movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
					transitOffice.CY_Data = "IT275100";
					AssertNoMessageErrorContaining($"Declaration Type is {movementHeader.BM_InBondEntryType} and CustomsOfficeOfTransit is filled", cY_DataInfo, expectedErrorMessage);
				});
			}
		}

		public void TestCheckRuleG0587WithSingleOffice()
		{
			var factory = Factory;
			UniversalReferenceTestDataHelper.EnsureCountriesAreInRefCusCodeList(factory, Code_CL247, (Poland, nameof(Poland)));
			factory.Save();
			var countryOfRouting = header.CountriesOfRouting.AddNew();
			countryOfRouting.CY_Data = Poland;

			var movementHeader = header.MovementHeader;
			movementHeader.CustomsOffices.RemoveAndDeleteAll();
			var departureOffice = movementHeader.CustomsOffices.AddNew(NCTSOfficeOfDeparture, Australia);
			const string errorMessage = "[G0587] You have not entered The Customs Office of Exit for Transit, Purpose Code TXT.";

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				CombineAssertions(() =>
				{
					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleG0587Active));
					departureOffice.Validation.ValidateCY_Code();
					AssertNoMessageError("No TXT in the purpose of customs office and countryOfRouting is in CL247", departureOffice.CY_CodeInfo, errorMessage);

					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleG0587Active));
					departureOffice.Validation.ValidateCY_Code();
					AssertHasMessageError("No TXT in the purpose of customs office and countryOfRouting is in CL247", departureOffice.CY_CodeInfo, errorMessage);

					departureOffice.CY_Code = NCTSOfficeOfExitForTransit;
					departureOffice.Validation.ValidateCY_Code();
					AssertNoMessageError("TXT in the purpose of customs office and countryOfRouting is in CL247", departureOffice.CY_CodeInfo, errorMessage);

					departureOffice.CY_Code = NCTSOfficeOfDeparture;
					countryOfRouting.CY_Data = Australia;
					departureOffice.Validation.ValidateCY_Code();
					AssertNoMessageError("No TXT in the purpose of customs office but countryOfRouting is not in CL247", departureOffice.CY_CodeInfo, errorMessage);
				});
			}
		}

		public void TestCheckRuleG0587WithMultipleOffice()
		{
			var factory = Factory;
			UniversalReferenceTestDataHelper.EnsureCountriesAreInRefCusCodeList(factory, Code_CL247, (Poland, nameof(Poland)));
			factory.Save();
			var countryOfRouting = header.CountriesOfRouting.AddNew();
			countryOfRouting.CY_Data = Poland;

			var movementHeader = header.MovementHeader;
			movementHeader.CustomsOffices.RemoveAndDeleteAll();
			var departureOffice = movementHeader.CustomsOffices.AddNew(NCTSOfficeOfDeparture, Australia);
			const string errorMessage = "[G0587] You have not entered The Customs Office of Exit for Transit, Purpose Code TXT.";

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				CombineAssertions(() =>
				{
					var customsOffice = movementHeader.CustomsOffices.AddNew();
					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleG0587Active));
					customsOffice.Validation.ValidateCY_Code();
					AssertNoMessageError("No TXT in the purpose of customs office and countryOfRouting is in CL247", customsOffice.CY_CodeInfo, errorMessage);

					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleG0587Active));
					customsOffice.Validation.ValidateCY_Code();
					AssertHasMessageError("No TXT in the purpose of customs office and countryOfRouting is in CL247", customsOffice.CY_CodeInfo, errorMessage);

					departureOffice.CY_Code = NCTSOfficeOfExitForTransit;
					customsOffice.Validation.ValidateCY_Code();
					AssertNoMessageError("TXT in the purpose of customs office and countryOfRouting is in CL247", customsOffice.CY_CodeInfo, errorMessage);

					departureOffice.CY_Code = NCTSOfficeOfDeparture;
					countryOfRouting.CY_Data = Australia;
					customsOffice.Validation.ValidateCY_Code();
					AssertNoMessageError("No TXT in the purpose of customs office but countryOfRouting is not in CL247", customsOffice.CY_CodeInfo, errorMessage);
				});
			}
		}

		[TestDate(2025, 2, 8, 10, 45, 31)]
		public void TestCheckCY_Date_RuleB1904DisablesR0005()
		{
			const string r0005ErrorMessage = "The Estimated Arrival Date Time for Customs Office of Transit can't be earlier or equal to current date time.";
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			using (var ruleContext = new NctsEuOfficeCodeValidationDeciderTestContext<INctsEuOfficeCodeDeparturePhase5ValidationDecider>(Factory))
			{
				ruleContext.EnableRule(x => x.IsRuleR0005Active);

				var movementHeader = header.MovementHeader;
				UniversalReferenceTestDataHelper.RunAssertionsInPhase5TransitionPeriod(() =>
				{
					ruleContext.DisableRule(x => x.IsRuleB1904Active);
					var customsOffice = movementHeader.CustomsOffices.AddNew();
					customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;

					customsOffice.CY_Date = new ZDateTime(2025, 2, 8, 10, 45, 30);
					AssertHasMessageError("B1904 is disabled in Phase5 Transition Period, R0005 is applied", customsOffice.CY_DateInfo, r0005ErrorMessage);

					ruleContext.EnableRule(x => x.IsRuleB1904Active);
					customsOffice.Validation.ValidateCY_Date();
					AssertNoMessageError("Rule B1904 active in Phase5 Transition Period, R0005 is not applied", customsOffice.CY_DateInfo, r0005ErrorMessage);
				});

				UniversalReferenceTestDataHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
				{
					ruleContext.DisableRule(x => x.IsRuleB1904Active);
					var customsOffice = movementHeader.CustomsOffices.AddNew();
					customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;

					customsOffice.CY_Date = new ZDateTime(2025, 2, 8, 10, 45, 30);
					AssertHasMessageError("Rule B1904 is disabled outside Phase5 Transition Period, R0005 is applied", customsOffice.CY_DateInfo, r0005ErrorMessage);

					ruleContext.EnableRule(x => x.IsRuleB1904Active);
					customsOffice.Validation.ValidateCY_Date();
					AssertHasMessageError("Rule B1904 active outside Phase5 Transition Period, R0005 is applied", customsOffice.CY_DateInfo, r0005ErrorMessage);
				});

				ruleContext.DisableRule(x => x.IsRuleR0005Active);

				UniversalReferenceTestDataHelper.RunAssertionsInPhase5TransitionPeriod(() =>
				{
					ruleContext.DisableRule(x => x.IsRuleB1904Active);
					var customsOffice = movementHeader.CustomsOffices.AddNew();
					customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;

					customsOffice.CY_Date = new ZDateTime(2025, 2, 8, 10, 45, 30);
					AssertNoMessageError("Rule R0005 is disabled, B1904 is disabled in Phase5 Transition Period, R0005 is applied", customsOffice.CY_DateInfo, r0005ErrorMessage);
				});

				UniversalReferenceTestDataHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
				{
					ruleContext.DisableRule(x => x.IsRuleB1904Active);
					var customsOffice = movementHeader.CustomsOffices.AddNew();
					customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;

					customsOffice.CY_Date = new ZDateTime(2025, 2, 8, 10, 45, 30);
					AssertNoMessageError("Rule R0005 is disabled, Rule B1904 is disabled outside Phase5 Transition Period, R0005 is applied", customsOffice.CY_DateInfo, r0005ErrorMessage);

					ruleContext.EnableRule(x => x.IsRuleB1904Active);
					customsOffice.Validation.ValidateCY_Date();
					AssertNoMessageError("Rule R0005 is disabled, Rule B1904 active outside Phase5 Transition Period, R0005 is applied", customsOffice.CY_DateInfo, r0005ErrorMessage);
				});
			}
		}

		public void TestCheckConditionTXTC0030Condition() => CombineAssertions(() =>
		{
			const string messageError = "[C0030] You have not entered a Customs Office Of Transit Declared (TRA).";

			using (var ruleContext = new NctsEuOfficeCodeValidationDeciderTestContext<INctsEuOfficeCodeDeparturePhase5ValidationDecider>(Factory))
			{
				ruleContext.DisableRule(c => c.IsRuleB1836Active);
				ruleContext.EnableRule(c => c.IsRuleC0030Active);

				var movementHeader = header.MovementHeader;
				movementHeader.CustomsOffices.RemoveAndDeleteAll();

				var officeTXT = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
				officeTXT.CY_Data = "LV000001";
				AssertHasMessageError("HasExitForTransitOffice HasNoTransitOffice", officeTXT.CY_DataInfo, messageError);

				ruleContext.DisableRule(c => c.IsRuleC0030Active);
				officeTXT.CY_Data = "LV000001";
				AssertNoMessageError("HasExitForTransitOffice HasNoTransitOffice RuleDisabled", officeTXT.CY_DataInfo, messageError);

				ruleContext.EnableRule(c => c.IsRuleC0030Active);
				movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
				officeTXT.CY_Data = "LV000001";
				AssertNoMessageError("HasExitForTransitOffice HasTransitOffice", officeTXT.CY_DataInfo, messageError);
			}
		});

		[TestDate(2025, 2, 8, 10, 45, 31)]
		public void TestCheckCY_Date_R0005()
		{
			var messageError = "The Estimated Arrival Date Time for Customs Office of Transit can't be earlier or equal to current date time.";

			using (var ruleContext = new NctsEuOfficeCodeValidationDeciderTestContext<INctsEuOfficeCodeDeparturePhase5ValidationDecider>(Factory))
			{
				CombineAssertions(() =>
				{
					ruleContext.EnableRule(x => x.IsRuleR0005Active);
					var customsOffice = header.MovementHeader.CustomsOffices.AddNew();
					customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
					customsOffice.CY_Date = new ZDateTime(2025, 2, 8, 10, 45, 31);
					AssertHasMessageError("Rule R0005 active, Date not in future (present), TRA, Phase Status is empty", customsOffice.CY_DateInfo, messageError);

					customsOffice.CY_Date = new ZDateTime(2025, 2, 8, 10, 45, 30);
					AssertHasMessageError("Rule R0005 active, Date not in future (past), TRA, Phase Status is empty", customsOffice.CY_DateInfo, messageError);

					header.MovementHeader.BM_Phase = "013";
					customsOffice.Validation.ValidateCY_Date();
					AssertNoMessageError("Rule R0005 active, Date not in future (past), TRA, Phase Status is not 015", customsOffice.CY_DateInfo, messageError);

					header.MovementHeader.BM_Phase = "015";
					customsOffice.Validation.ValidateCY_Date();
					AssertHasMessageError("Rule R0005 active, Date not in future (past), TRA, Phase Status is 015", customsOffice.CY_DateInfo, messageError);

					ruleContext.DisableRule(x => x.IsRuleR0005Active);
					customsOffice.Validation.ValidateCY_Date();
					AssertNoMessageError("Rule R0005 inactive, Date not in future, TRA", customsOffice.CY_DateInfo, messageError);

					ruleContext.EnableRule(x => x.IsRuleR0005Active);
					customsOffice.CY_Date = new ZDateTime(2025, 2, 8, 10, 45, 32);
					AssertNoMessageError("Rule R0005 active, Future Date, TRA", customsOffice.CY_DateInfo, messageError);

					customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit;
					customsOffice.CY_Date = new ZDateTime(2025, 2, 8, 10, 45, 30);
					AssertNoMessageError("Rule R0005 active, Date not in future, TXT", customsOffice.CY_DateInfo, messageError);
				});
			}
		}

		[TestDate(2022, 07, 01)]
		public void TestCheckCY_Date_C0598() => CombineAssertions(() =>
		{
			var expectedError = "[C0598] You have not entered a Time (Arrival Date and Time Estimated).";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var tradeGroup = helper.CreateTradeGroup("EUN", "EUC", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Spain, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			tradeGroup = helper.CreateTradeGroup("EUN", "EUCTP", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Turkey, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));

			helper.CreateNewOrGetExistingDataGrouping("EUN");
			var tradeGroupEUSEC = helper.CreateTradeGroup("EUN", "EUSEC", new ZDate(2022, 01, 01), new ZDate(2022, 12, 31));
			helper.AddCountry(tradeGroupEUSEC, Core.Constants.CountryCodes.Austria, new ZDate(2022, 01, 01), new ZDate(2022, 12, 31));
			Factory.Save();

			using (var ruleContext = new NctsEuOfficeCodeValidationDeciderTestContext<INctsEuOfficeCodeDeparturePhase5ValidationDecider>(Factory))
			{
				var nctsDeparture = header.MovementHeader;
				var officeCode = nctsDeparture.CustomsOffices.AddNew();
				officeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
				officeCode.CY_Data = "AT00110";
				nctsDeparture.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				var validation = officeCode.Validation;

				ruleContext.EnableRule(c => c.IsRuleC0598Active);

				officeCode.CY_Date = new ZDateTime(2023, 1, 18);
				AssertNoError("No error as CY_Date is not empty", officeCode.CY_DateInfo, expectedError);

				officeCode.CY_Date = ZDateTime.Empty;
				AssertHasError("C0598 error when CY_Date is empty", officeCode.CY_DateInfo, expectedError);

				ruleContext.DisableRule(c => c.IsRuleC0598Active);

				officeCode.CY_Date = ZDateTime.Empty;
				validation.ValidateCY_Date();
				AssertNoError("Rule C0598 is disabled", officeCode.CY_DateInfo, expectedError);

				ruleContext.EnableRule(c => c.IsRuleC0598Active);

				officeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				validation.ValidateCY_Date();
				AssertNoError("No error as Purpose is not TRA", officeCode.CY_DateInfo, expectedError);

				officeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
				nctsDeparture.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
				validation.ValidateCY_Date();
				AssertNoError("No error as Security Type is not ENT or BTH", officeCode.CY_DateInfo, expectedError);

				nctsDeparture.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				officeCode.CY_Data = "CY00110";
				validation.ValidateCY_Date();
				AssertNoError("No error as office not in Country Customs Security Agreement Area", officeCode.CY_DateInfo, expectedError);

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					ruleContext.EnableRule(c => c.IsRuleB1831Active);
					officeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
					officeCode.CY_Data = "AT00110";
					officeCode.CY_Date = ZDateTime.Empty;
					AssertNoError("No error when TP is ON, RuleB1831 is Active and CY_Date is empty", officeCode.CY_DateInfo, expectedError);
				}
			}
		});

		[TestDate(2022, 07, 01)]
		public void TestCheckCY_Date_B1831()
		{
			var expectedMessageError = $"{ValidationRuleCodeConstants.B1831.GetRuleCodeMessagePrefix()} Time can't be empty for Customs Office with Purpose='TRA'.";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "Country Customs Security Agreement Area");
			helper.CreateCusCodeList("EUN", EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "AT", "Austria", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
			helper.CreateCusCodeList("EUN", UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, "CY", "Cyprus", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
			Factory.Save();

			using (var ruleContext = new NctsEuOfficeCodeValidationDeciderTestContext<INctsEuOfficeCodeDeparturePhase5ValidationDecider>(Factory))
			{
				var nctsDeparture = header.MovementHeader;
				nctsDeparture.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;

				nctsDeparture.CustomsOffices.RemoveAndDeleteAll();

				var officeCodeOfTransit = nctsDeparture.CustomsOffices.AddNew();
				officeCodeOfTransit.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
				officeCodeOfTransit.CY_Data = "AT00110";

				var officeCodeOfDeparture = nctsDeparture.CustomsOffices.AddNew();
				officeCodeOfDeparture.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				officeCodeOfDeparture.CY_Data = "PL00110";

				var validation = officeCodeOfTransit.Validation;

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					ruleContext.EnableRule(c => c.IsRuleB1831Active);

					CombineAssertions("When TP: ON, NCTS5, Movement: D, Security: ENT and RuleB1831: Active", () =>
					{
						officeCodeOfTransit.CY_Date = new ZDateTime(2023, 1, 18);
						AssertNoMessageError("Transit.CY_Data is in CL010 list, Departure.CY_Data is not in CL010 list, CY_Date: not empty",
							officeCodeOfTransit.CY_DateInfo, expectedMessageError);

						officeCodeOfTransit.CY_Date = ZDateTime.Empty;
						AssertHasMessageError("Transit.CY_Data is in CL010 list, Departure.CY_Data is not in CL010 list, CY_Date: empty",
							officeCodeOfTransit.CY_DateInfo, expectedMessageError);

						officeCodeOfTransit.CY_Data = "CY00110";
						validation.ValidateCY_Date();
						AssertNoMessageError("Transit.CY_Data is not in CL010 list, Departure.CY_Data is not in CL010 list, CY_Date: empty",
							officeCodeOfTransit.CY_DateInfo, expectedMessageError);

						officeCodeOfTransit.CY_Data = "AT00110";
						officeCodeOfDeparture.CY_Data = "AT00110";
						validation.ValidateCY_Date();
						AssertNoMessageError("Transit.CY_Data is in CL010 list, Departure.CY_Data is in CL010 list, CY_Date: empty",
							officeCodeOfTransit.CY_DateInfo, expectedMessageError);

						nctsDeparture.CustomsOffices.Remove(officeCodeOfDeparture);
						validation.ValidateCY_Date();
						AssertNoMessageError("Transit.CY_Data is in CL010 list, Departure row is not added, CY_Date: empty",
							officeCodeOfTransit.CY_DateInfo, expectedMessageError);
						nctsDeparture.CustomsOffices.Add(officeCodeOfDeparture);
					});

					officeCodeOfTransit.CY_Data = "AT00110";
					officeCodeOfDeparture.CY_Data = "PL00110";
					CombineAssertions("When TP: ON, NCTS5, Movement: D, RuleB1831: Active, Transit.CY_Data is in CL010 list, Departure.CY_Data is not in CL010 list, CY_Date: empty", () =>
					{
						nctsDeparture.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
						validation.ValidateCY_Date();
						AssertHasMessageError("Security: EXI", officeCodeOfTransit.CY_DateInfo, expectedMessageError);

						nctsDeparture.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
						validation.ValidateCY_Date();
						AssertHasMessageError("Security: BTH", officeCodeOfTransit.CY_DateInfo, expectedMessageError);
					});

					CombineAssertions("When Transit.CY_Data is in CL010 list, Departure.CY_Data is not in CL010 list, CY_Date: empty", () =>
					{
						ruleContext.DisableRule(c => c.IsRuleB1831Active);
						officeCodeOfTransit.CY_Data = "AT00110";
						officeCodeOfDeparture.CY_Data = "PL00110";
						validation.ValidateCY_Date();
						AssertNoMessageError("NCTS5, Movement: D, RuleB1831: Disabled", officeCodeOfTransit.CY_DateInfo, expectedMessageError);
					});
				}

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
				{
					officeCodeOfTransit.CY_Data = "AT00110";
					officeCodeOfDeparture.CY_Data = "PL00110";
					validation.ValidateCY_Date();
					AssertNoMessageError("NCTS5, Movement: D, TP: OFF, RuleB1831: Active, Transit.CY_Data is in CL010 list, Departure.CY_Data is not in CL010 list, CY_Date: empty",
						officeCodeOfTransit.CY_DateInfo, expectedMessageError);
				}
			}
		}

		public void TestCheckCY_Code_R0003()
		{
			const string messageError = "[R0003] The entered Customs Office of Transit is already present.";

			var movementHeader = header.MovementHeader;
			movementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				CombineAssertions(() =>
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleR0003Active));
					var officeTRA1 = movementHeader.CustomsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
					officeTRA1.CY_Data = "123";
					var officeTRA2 = movementHeader.CustomsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
					officeTRA2.CY_Data = officeTRA1.CY_Data;
					officeTRA2.Validation.ValidateCY_Code();
					AssertHasMessageError("Customs Office 'TRA' same Data, Rule R0003 active", officeTRA2.CY_CodeInfo, messageError);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleR0003Active));
					officeTRA2.Validation.ValidateCY_Code();
					AssertNoMessageError("Customs Office 'TRA' same Data, Rule R0003 inactive", officeTRA2.CY_CodeInfo, messageError);

					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleR0003Active));
					var officeDES = movementHeader.CustomsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
					AssertNoMessageError("Customs Office 'DES', Rule R0003 active", officeDES.CY_CodeInfo, messageError);

					officeTRA1.CY_Data = "987";
					officeTRA2.CY_Data = "123";
					AssertNoMessageError("Customs Office 'TRA' different Data, Rule R0003 active", officeTRA2.CY_CodeInfo, messageError);
				});
			}
		}

		public void TestCheckCY_Code_RuleC0587_1()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			var tradeGroupEUSEC = helper.CreateTradeGroup("EUN", "EUSEC", ZDateTime.Today, ZDateTime.Today);
			helper.AddCountry(tradeGroupEUSEC, Core.Constants.CountryCodes.Austria, ZDate.Today, ZDate.Today);

			Factory.Save();

			var expectedErrorMessage = "[C0587-1] A Customs Office of Exit for Transit (Purpose = TXT) must not be present";
			var movementHeader = header.MovementHeader;

			var officeToValidate = movementHeader.CustomsOffices.AddNew();
			var officeWithTRACode = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			officeWithTRACode.CY_Data = "AT275100";
			officeToValidate.CY_Data = "CY110123";

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleC0587_1Active));

				CombineAssertions(() =>
				{
					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
					officeToValidate.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit;
					AssertHasMessageErrorContaining("Security type is BTH", officeToValidate.CY_CodeInfo,
						expectedErrorMessage);

					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
					officeToValidate.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit;
					AssertHasMessageErrorContaining("Security type is EXI", officeToValidate.CY_CodeInfo,
						expectedErrorMessage);

					officeToValidate.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
					AssertNoErrorContaining("Office code is not equal to TXT", officeToValidate.CY_CodeInfo,
						expectedErrorMessage);

					officeWithTRACode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit;
					officeToValidate.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit;
					AssertNoErrorContaining("No custom office with code = TRA", officeToValidate.CY_CodeInfo,
						expectedErrorMessage);

					officeWithTRACode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
					officeToValidate.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit;
					AssertNoErrorContaining("Security type is not EXI or BTH", officeToValidate.CY_CodeInfo,
						expectedErrorMessage);

					officeWithTRACode.CY_Data = "AD275100";
					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
					officeToValidate.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit;
					AssertNoErrorContaining("No transit office in security arrangement area (CL147)",
						officeToValidate.CY_CodeInfo, expectedErrorMessage);
				});

				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleC0587_1Active));

				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
				officeWithTRACode.CY_Data = "AT275100";
				officeToValidate.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit;
				AssertNoErrorContaining("Rule is inactive", officeToValidate.CY_CodeInfo,
					expectedErrorMessage);
			}
		}

		public void TestCheckCY_Code_RuleC0587_2()
		{
			const string warningMessage = "[C0587-2] Customs Office of Exit (TXT) will not be declared if the Security value is not EXI or if declaration type is TIR";

			CombineAssertions(() =>
			{
				using var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory);

				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleC0587_2Active));

				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				var departureMovement = nctsHeader.MovementHeader;
				var office = departureMovement.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);

				departureMovement.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
				departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
				office.Validation.ValidateCY_Code();
				AssertHasWarningContaining("TXT code, TIR entryType and EXI security", office.CY_CodeInfo, warningMessage);

				departureMovement.BM_InBondEntryType = "JPB";
				departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
				office.Validation.ValidateCY_Code();
				AssertNoWarningContaining("TXT code, not TIR entryType and EXI security", office.CY_CodeInfo, warningMessage);

				departureMovement.BM_TypeOfSecurity = "JPB";
				office.Validation.ValidateCY_Code();
				AssertHasWarningContaining("TXT code, not TIR entryType and not EXI security", office.CY_CodeInfo, warningMessage);

				office.CY_Code = "JPB";
				AssertNoWarningContaining("not TXT code", office.CY_CodeInfo, warningMessage);

				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleC0587_2Active));
				office.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit;
				AssertNoWarningContaining("Rule disabled", office.CY_CodeInfo, warningMessage);
			});
		}

		public void TestCheckRuleC0030_CL112CountryCodes()
		{
			var movementHeader = header.MovementHeader;

			using (var ruleContext = new NctsEuOfficeCodeValidationDeciderTestContext<INctsEuOfficeCodeDeparturePhase5ValidationDecider>(Factory))
			{
				ruleContext.EnableRule(c => c.IsRuleC0030Active);
				ruleContext.DisableRule(c => c.IsRuleB1836Active);

				var errorMessage = "[C0030] You have not entered a Customs Office Of Transit Declared (TRA).";

				var helper = new UniversalReferenceTestDataHelper(Factory);
				var euGroup = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN);
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
				helper.CreateNewOrGetExistingCusCodeType(EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "CL112 Desc.");
				helper.CreateCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "DE", "DE CL112 Code", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				Factory.Save();

				var customsOffice = movementHeader.CustomsOffices.AddNew();
				var customsOffice2 = movementHeader.CustomsOffices.AddNew();

				customsOffice.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				customsOffice2.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
				CombineAssertions(() =>
				{
					customsOffice.CY_Data = "DE123456";
					customsOffice.Validation.ValidateCY_Data();
					AssertHasMessageError("Having one of the customs office with a CL112 country code should evoke the C0030 message error", customsOffice.CY_DataInfo, errorMessage);

					customsOffice.CY_Data = "AD123456";
					customsOffice.Validation.ValidateCY_Data();
					AssertHasMessageError("Having one of the customs offce coutnry code set to AD should evoke the C0030 message error", customsOffice.CY_DataInfo, errorMessage);

					customsOffice.CY_Data = "FR123456";
					AssertNoMessageError("Having a non CL112 Coutnry Code in a customs office should not evoke the C0030 message error", customsOffice.CY_DataInfo, errorMessage);

					ruleContext.DisableRule(c => c.IsRuleC0030Active);
					customsOffice.CY_Data = "DE123456";
					AssertNoMessageError("The C0030 Rule Code message error should not appear when deactivated", customsOffice.CY_DataInfo, errorMessage);
				});
			}
		}

		public void TestCheckRuleR0103()
		{
			var messageError = header.Configuration.ValidationRuleConfiguration.Messages.R0103Message;

			using (var ruleContext = new NctsEuOfficeCodeValidationDeciderTestContext<INctsEuOfficeCodeDeparturePhase5ValidationDecider>(Factory))
			{
				ruleContext.EnableRule(x => x.IsRuleR0103Active);

				var movementHeader = DepartureMovementPhase5ForTest();

				var customsOfficeTRA = movementHeader.CustomsOffices.AddNew();
				customsOfficeTRA.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;

				var customsOfficeTXT = movementHeader.CustomsOffices.AddNew();
				customsOfficeTXT.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit;

				CombineAssertions(() =>
				{
					movementHeader.DestinationCustomsOfficeCodeForDeparture = "PL301050";
					customsOfficeTRA.CY_Data = "PL301050";
					customsOfficeTXT.CY_Data = "PL301050";
					AssertHasMessageError("ExitForTransit == Transit AND ExitForTransit == Destination", customsOfficeTXT.CY_DataInfo, messageError);

					customsOfficeTRA.CY_Data = "PL301060";
					customsOfficeTXT.CY_Data = "PL301060";
					AssertHasMessageError("ExitForTransit == Transit AND ExitForTransit != Destination", customsOfficeTXT.CY_DataInfo, messageError);

					movementHeader.DestinationCustomsOfficeCodeForDeparture = "PL301070";
					customsOfficeTXT.CY_Data = "PL301070";
					AssertHasMessageError("ExitForTransit != Transit AND ExitForTransit == Destination", customsOfficeTXT.CY_DataInfo, messageError);

					customsOfficeTXT.CY_Data = "PL301080";
					AssertNoMessageError("ExitForTransit != Transit AND ExitForTransit != Destination", customsOfficeTXT.CY_DataInfo, messageError);

					ruleContext.DisableRule(x => x.IsRuleR0103Active);

					movementHeader.DestinationCustomsOfficeCodeForDeparture = "PL301050";
					customsOfficeTRA.CY_Data = "PL301050";
					customsOfficeTXT.CY_Data = "PL301050";
					AssertNoMessageError("ExitForTransit == Transit AND ExitForTransit == Destination, rule is disabled", customsOfficeTXT.CY_DataInfo, messageError);
				});
			}
		}

		public void TestCheckCustomOffices_R0006_TransitBasedOnDestination()
		{
			var (_, _, cl112Code1, cl112Code2, nonCl112Code) = SetupCodesForR0006Rule();

			var testCases = new[] {
				(departure: Estonia, destination: cl112Code1, transit: nonCl112Code,
				 errorExpected: true, message: "destination office in CL112, no transit office from CL112"),
				(departure: Estonia, destination: nonCl112Code, transit: nonCl112Code,
				 errorExpected: false, message: "destination office in not CL112, no transit office from CL112"),
				(departure: Estonia, destination: cl112Code1, transit: cl112Code1,
				 errorExpected: false, message: "destination office in CL112, transit office from CL112 is present, transit CL112 office is in same country as destination office"),
				(departure: Estonia, destination: cl112Code1, transit: cl112Code2,
				 errorExpected: true, message: "destination office in CL112, transit office from CL112 is present, transit CL112 office is in another country than destination office")
			};

			AssertRuleR0006ComplianceForCustomsOffices("[R0006] At least one Customs Office of Transit must be from the same country as that of Destination Customs Office.", testCases);
		}

		public void TestCheckCustomOffices_R0006_TransitBasedOnDepartureAndDestination()
		{
			var (cl010Code, nonCl010Code, cl112Code, _, nonCl112Code) = SetupCodesForR0006Rule();

			var testCases = new[] {
				(departure: cl112Code, destination: cl010Code, transit: nonCl010Code,
				 errorExpected: true, message: "departure office in CL112, destination office in CL010, no transit office from CL010"),
				(departure: nonCl112Code, destination: cl010Code, transit: nonCl010Code,
				 errorExpected: false, message: "departure office not in CL112, destination office in CL010, no transit office from CL010"),
				(departure: nonCl112Code, destination: nonCl010Code, transit: nonCl010Code,
				 errorExpected: false, message: "departure office in CL112, destination office not in CL010, no transit office from CL010"),
				(departure: nonCl112Code, destination: cl010Code, transit: cl010Code,
				 errorExpected: false, message: "departure office in CL112, destination office in CL010, transit office from CL010")
			};

			AssertRuleR0006ComplianceForCustomsOffices("[R0006] At least one Customs Office of Transit must be declared belonging to CL010 (Country Codes Community) set of countries.", testCases);
		}

		void AssertRuleR0006ComplianceForCustomsOffices(string errorMessage, params (string departure, string destination, string transit, bool errorExpected, string message)[] testData)
		{
			using var ruleContext = new NctsEuOfficeCodeValidationDeciderTestContext<INctsEuOfficeCodeDeparturePhase5ValidationDecider>(Factory);
			var movementHeader = header.MovementHeader;

			movementHeader.CustomsOffices.RemoveAndDeleteAll();
			var officeDeparture = movementHeader.CustomsOffices.AddNew(NCTSOfficeOfDeparture);
			var officeDestination = movementHeader.CustomsOffices.AddNew(NCTSOfficeOfDestination);
			movementHeader.CustomsOffices.AddNew(NCTSOfficeOfTransit, Australia);
			var officeTransit = movementHeader.CustomsOffices.AddNew(NCTSOfficeOfTransit);

			CombineAssertions(() =>
			{
				AssertR0006MessageError(ruleEnabled: true, inTransitPeriod: false);
				AssertR0006MessageError(ruleEnabled: true, inTransitPeriod: true);
				AssertR0006MessageError(ruleEnabled: false, inTransitPeriod: false);
			});

			void AssertR0006MessageError(bool ruleEnabled, bool inTransitPeriod)
			{
				var msgPrefix = (ruleEnabled ? "Rule R0006 active" : "Rule R0006 not active") + ", " + (inTransitPeriod ? "in NCTS transition period" : "outside of NCTS transition period") + ", ";

				if (ruleEnabled)
				{
					ruleContext.EnableRule(c => c.IsRuleR0006Active);
				}
				else
				{
					ruleContext.DisableRule(c => c.IsRuleR0006Active);
				}

				using var customsFunctionalityDisposable = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, inTransitPeriod);
				foreach (var (departure, destination, transit, errorExpected, message) in testData)
				{
					officeDeparture.CY_Data = departure + "000";
					officeDestination.CY_Data = destination + "000";
					officeTransit.CY_Data = transit + "000";
					officeDestination.Validation.ValidateCY_Code();

					if (ruleEnabled && !inTransitPeriod && errorExpected)
					{
						AssertHasMessageError(msgPrefix + message, officeDestination.CY_CodeInfo, errorMessage);
					}
					else
					{
						AssertNoMessageError(msgPrefix + message, officeDestination.CY_CodeInfo, errorMessage);
					}
				}
			}
		}

		(string cl010Code, string nonCl010Code, string cl112Code1, string cl112Code2, string nonCl112Code) SetupCodesForR0006Rule()
		{
			var factory = Factory;
			UniversalReferenceTestDataHelper.EnsureCountriesAreInRefCusCodeList(factory, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, (Estonia, "Estonia"), (Romania, "Romania"), (Lithuania, "Lithuania"));
			UniversalReferenceTestDataHelper.EnsureCountriesAreInRefCusCodeList(factory, Code_CL112, (Switzerland, "Switzerland"), (Norway, "Norway"), (Serbia, "Serbia"));
			factory.Save();

			return (cl010Code: Estonia, nonCl010Code: Poland, cl112Code1: Switzerland, cl112Code2: Norway, nonCl112Code: Spain);
		}

		NctsDepartureMovementHeader DepartureMovementPhase5ForTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeader.MovementHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
		}
		NctsHeader header;

		IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
			Constants.FunctionalityTypes.NCTSTransitionPeriod,
			RefDataGroupingCodes.EuropeanUnionEUN,
			ZDateTime.Today, isTransitionPeriodActive);
	}
}
