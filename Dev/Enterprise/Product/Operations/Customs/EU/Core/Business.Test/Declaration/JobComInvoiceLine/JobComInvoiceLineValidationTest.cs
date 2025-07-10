using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class JobComInvoiceLineValidationBaseOnlyTest : BusinessObjectValidationTestCase
	{
		public void TestGetNotAllowCreateNewEntryLineErrorMessage()
		{
			const string errorMessage = "Entry Declared (BGM001) : Cannot add new Entry lines to a Declared or Canceled Entry. Adding a new Entry Instruction then performing 'Generate Entries (Merge)' could solve the problem.";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "BGM001";
			AssertEquals(errorMessage , invoiceLine.Validation.GetNotAllowCreateNewEntryLineErrorMessage(entryHeader));
		}

		public void TestCheckJI_ValuationCode_RuleC0627()
		{
			const string C0627ErrorMessageWhenSubStyleIsCOrF = "[C0627] This field must be empty in case of Declaration Sub Type C or F.";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;

			using (var context = new InvoiceLineValidationDeciderTestContext<IImportInvoiceLineValidationDecider>(declaration))
			{
				context.EnableRule(r => r.IsRuleC0627Active);
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
				invoiceLine1.JI_ValuationCode = "1";
				AssertHasMessageError("IsRuleC0627Active is true, Sub Style in C/F, JI_ValuationCode of invoice line should be empty", invoiceLine1.JI_ValuationCodeInfo, C0627ErrorMessageWhenSubStyleIsCOrF);

				context.DisableRule(r => r.IsRuleC0627Active);
				invoiceLine1.JI_ValuationCode = "1";
				AssertNoMessageError("IsRuleC0627Active is false, Rule C0627 does not apply", invoiceLine1.JI_ValuationCodeInfo, C0627ErrorMessageWhenSubStyleIsCOrF);

				context.EnableRule(r => r.IsRuleC0627Active);
				invoiceLine1.JI_ValuationCode = ZString.Empty;
				AssertNoMessageError(invoiceLine1.ZG_TransNatureInfo, C0627ErrorMessageWhenSubStyleIsCOrF);
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
				invoiceLine1.JI_ValuationCode = "1";
				AssertNoMessageError("Sub Style is not C/F", invoiceLine1.JI_ValuationCodeInfo, C0627ErrorMessageWhenSubStyleIsCOrF);
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
				invoiceLine1.JI_ValuationCode = "2";
				AssertHasMessageErrorContaining("IsRuleC0627Active is true, Sub Style in C/F, JI_ValuationCode of invoice line should be empty", invoiceLine1.JI_ValuationCodeInfo, C0627ErrorMessageWhenSubStyleIsCOrF);
			}
		}

		public void TestCheckRuleC0919()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var errorMessage = "[C0919] Country of origin is mandatory for the chosen Preference code.";

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				invoiceLine.JI_PrimaryPreference = "100";
				invoiceLine.JI_CountryOfOrigin = ZString.Empty;
				AssertNoMessageErrorContaining("No message error is expected due to Preference code for non UCC6 declaration.", invoiceLine.JI_CountryOfOriginInfo, errorMessage);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				invoiceLine.JI_PrimaryPreference = "100";
				invoiceLine.JI_CountryOfOrigin = ZString.Empty;
				AssertHasMessageErrorContaining("Message error is expected due to Preference code for UCC6 declaration.", invoiceLine.JI_CountryOfOriginInfo, errorMessage);
			}

			using (var context = new InvoiceLineValidationDeciderTestContext<IImportInvoiceLineValidationDecider>(declaration))
			{
				context.EnableRule(r => r.IsRuleC0919Active);
				CombineAssertions("When RuleC0919 is enabled", () =>
				{
					invoiceLine.JI_PrimaryPreference = "100";

					invoiceLine.JI_CountryOfOrigin = CountryCodes.Germany;
					AssertNoMessageErrorContaining("No message error is expected when Preference code starts with '1' and CountryOfOrigin has value.", invoiceLine.JI_CountryOfOriginInfo, errorMessage);

					invoiceLine.JI_CountryOfOrigin = ZString.Empty;
					AssertHasMessageErrorContaining("Country Of Origin is mandatory when RuleC0919 is active and Preference code starts with '1'.", invoiceLine.JI_CountryOfOriginInfo, errorMessage);

					invoiceLine.JI_PrimaryPreference = "200";
					invoiceLine.Validation.ValidateJI_CountryOfOrigin();
					AssertNoMessageErrorContaining("No message error is expected when Preference code does not starts with '1' or '4' and CountryOfOrigin has no value.", invoiceLine.JI_CountryOfOriginInfo, errorMessage);

					invoiceLine.JI_PrimaryPreference = "400";
					invoiceLine.Validation.ValidateJI_CountryOfOrigin();
					AssertHasMessageErrorContaining("Country Of Origin is mandatory when RuleC0919 is active and Preference code starts with '4'.", invoiceLine.JI_CountryOfOriginInfo, errorMessage);

					invoiceLine.JI_PrimaryPreference = ZString.Empty;
					invoiceLine.Validation.ValidateJI_CountryOfOrigin();
					AssertNoMessageErrorContaining("No message error is expected when Preference code does not starts with '1' or '4' and CountryOfOrigin has no value.", invoiceLine.JI_CountryOfOriginInfo, errorMessage);
				});

				context.DisableRule(r => r.IsRuleC0919Active);
				CombineAssertions("When RuleC0919 is disabled", () =>
				{
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					invoiceLine.JI_PrimaryPreference = "100";
					invoiceLine.JI_CountryOfOrigin = ZString.Empty;
					AssertNoMessageErrorContaining("Country Of Origin is not mandatory when RuleC0919 is disabled.", invoiceLine.JI_CountryOfOriginInfo, errorMessage);

					invoiceLine.JI_PrimaryPreference = "400";
					invoiceLine.Validation.ValidateJI_CountryOfOrigin();
					AssertNoMessageErrorContaining("Country Of Origin is not mandatory when RuleC0919 is disabled.", invoiceLine.JI_CountryOfOriginInfo, errorMessage);
				});
			}
		}

		public void TestCheckRuleC0710()
		{
			var subStylesOfInterest = new string[] { EntrySubStyleList.Codes.NormalDeclaration,
													EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA,
													EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationOfSimplifiedDeclarationsCoveredByCAndF,
													EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationUnderTheProcedureCoveredUnderArticle182OfTheCode,
													EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF,
													EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic,
													};

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			foreach (var subStyle in subStylesOfInterest)
			{
				AssertInfringementToRuleC0710(declaration, invoiceLine, instruction, subStyle, CountryCodes.China, CountryCodes.Colombia, 1m, false);
				AssertInfringementToRuleC0710(declaration, invoiceLine, instruction, subStyle, ZString.Empty, ZString.Empty, ZDecimal.Zero, true);
			}
			AssertInfringementToRuleC0710(declaration, invoiceLine, instruction, "X", ZString.Empty, ZString.Empty, ZDecimal.Zero, false);

			void AssertInfringementToRuleC0710(JobDeclaration dec, JobComInvoiceLine line, CusEntryInstruction entryInstruction, string subStyle, string countryOfOrigin, string countryOfSupply, decimal linePrice, bool shouldExpectRuleInfriged)
			{
				using (var context = new InvoiceLineValidationDeciderTestContext<IImportInvoiceLineValidationDecider>(declaration))
				{
					context.EnableRule(x => x.IsRuleC0710ActiveForJI_CountryOfOrigin);
					context.EnableRule(x => x.IsRuleC0710ActiveForJI_LinePrice);
					context.EnableRule(x => x.IsRuleC0710ActiveForZG_CountryOfSupply);

					entryInstruction.CEI_SubStyle = subStyle;
					invoiceLine.JI_CountryOfOrigin = countryOfOrigin;
					invoiceLine.ZG_CountryOfSupply = countryOfSupply;
					invoiceLine.JI_LinePrice = linePrice;

					if (shouldExpectRuleInfriged)
					{
						CombineAssertions($"When RuleC0710: Active, CEI_SubStyle: {subStyle}", () =>
						{
							AssertHasMessageError($"JI_CountryOfOrigin: {countryOfOrigin}", invoiceLine.JI_CountryOfOriginInfo, "[C0710] This field is mandatory for this declaration sub type.");
							AssertHasMessageError($"ZG_CountryOfSupply: {countryOfSupply}", invoiceLine.ZG_CountryOfSupplyInfo, "[C0710] This field is mandatory for this declaration sub type.");
							AssertHasMessageError($"JI_LinePrice: {linePrice}", invoiceLine.JI_LinePriceInfo, "[C0710] This field is mandatory for this declaration sub type.");
						});

						context.DisableRule(x => x.IsRuleC0710ActiveForJI_CountryOfOrigin);
						context.DisableRule(x => x.IsRuleC0710ActiveForJI_LinePrice);
						context.DisableRule(x => x.IsRuleC0710ActiveForZG_CountryOfSupply);

						invoiceLine.Validation.ValidateJI_CountryOfOrigin();
						invoiceLine.Validation.ValidateJI_LinePrice();
						invoiceLine.ZG_CountryOfSupply = countryOfSupply;
						CombineAssertions($"When RuleC0710: Disabled, CEI_SubStyle: {subStyle}", () =>
						{
							AssertNoMessageError($"JI_CountryOfOrigin: {countryOfOrigin}", invoiceLine.JI_CountryOfOriginInfo, "[C0710] This field is mandatory for this declaration sub type.");
							AssertNoMessageError($"ZG_CountryOfSupply: {countryOfSupply}", invoiceLine.ZG_CountryOfSupplyInfo, "[C0710] This field is mandatory for this declaration sub type.");
							AssertNoMessageError($"JI_LinePrice: {linePrice}", invoiceLine.JI_LinePriceInfo, "[C0710] This field is mandatory for this declaration sub type.");
						});
					}
					else
					{
						CombineAssertions($"RuleC0710: Active, CEI_SubStyle: {subStyle}", () =>
						{
							AssertNoMessageError($"JI_CountryOfOrigin: {countryOfOrigin}", invoiceLine.JI_CountryOfOriginInfo, "[C0710] This field is mandatory for this declaration sub type.");
							AssertNoMessageError($"ZG_CountryOfSupply: {countryOfSupply}", invoiceLine.ZG_CountryOfSupplyInfo, "[C0710] This field is mandatory for this declaration sub type.");
							AssertNoMessageError($"JI_LinePrice: {linePrice}", invoiceLine.JI_LinePriceInfo, "[C0710] This field is mandatory for this declaration sub type.");
						});
					}
				}
			}
		}

		public void TestCheckRelatedIndicators_RuleC0624()
		{
			const string C0624ErrorString = "[C0624] Value indicator should not be entered for the provided Declaration Type and Requested Procedure.";

			var testCase = new MultiFactorTestCase<JobDeclaration>(() =>
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				return declaration;
			});

			var isUCC = new ConfigurationPreq<JobDeclaration>(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration).Values(true);
			var isImport = new FieldPreq<JobDeclaration>(dec => dec.JE_MessageTypeInfo).Values(MessageTypeList.Codes.Import);
			var validCEI_Procedure = new FieldPreq<JobDeclaration>(dec => dec.CustomsEntryInstructions[0].CEI_ProcedureInfo).Values("53", "71");
			var validCEI_SubStyle = new FieldPreq<JobDeclaration>(dec => dec.CustomsEntryInstructions[0].CEI_SubStyleInfo).Values("C", "F").NotValues("D");

			var relatedIndicatorsToTest = new List<Func<JobDeclaration, ZPropertyInfo>>()
			{
				dec => dec.Invoices[0].InvoiceLines[0].RelatedIndicatorInfo,
				dec => dec.Invoices[0].InvoiceLines[0].ZG_RelatedIndicator2Info,
				dec => dec.Invoices[0].InvoiceLines[0].ZG_RelatedIndicator3Info,
				dec => dec.Invoices[0].InvoiceLines[0].ZG_RelatedIndicator4Info,
			};

			relatedIndicatorsToTest.ForEach(getTargetRelatedIndicator =>
			{
				var relatedIndicatorTicked = new FieldPreq<JobDeclaration>(getTargetRelatedIndicator).Values(true);
				testCase.SetUpCondition(isUCC && isImport && (validCEI_Procedure || validCEI_SubStyle) && relatedIndicatorTicked);
				testCase.SetUpProcessAction(dec => { dec.Invoices[0].InvoiceLines[0].Validation.ValidateJI_RelatedIndicator(); dec.Invoices[0].InvoiceLines[0].AddInfoValidation.ValidateAll(); });
				testCase.RunAssertion(dec => AssertHasMessageError(getTargetRelatedIndicator(dec), C0624ErrorString), dec => AssertNoMessageErrors(getTargetRelatedIndicator(dec)));
			});
		}

		public void TestValidateAllowMultipleRequestedProcedure()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_Procedure = "123456";
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_Procedure = "234567";
			const string messageError = "All invoice lines under the same entry must have the same Requested Procedure.";

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				invoiceLine2.Validation.ValidateJI_Procedure();
				AssertNoMessageError("IsUcc6AndImport and AllowMultipleRequestedProcedure", invoiceLine2.JI_ProcedureInfo, messageError);
			}

			using (var context = new InvoiceLineValidationDeciderTestContext<IImportInvoiceLineValidationDecider>(declaration))
			{
				context.DisableRule(r => r.AllowMultipleRequestedProcedure);
				invoiceLine2.Validation.ValidateJI_Procedure();
				AssertHasMessageError("IsUcc6AndImport and not AllowMultipleRequestedProcedure", invoiceLine2.JI_ProcedureInfo, messageError);
			}
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				invoiceLine2.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Import and IsNotUcc6 ", invoiceLine2.JI_ProcedureInfo, messageError);
			}
		}

		public void TestCheckRuleCD5161()
		{
			using (var context = new InvoiceLineValidationDeciderTestContext<IImportInvoiceLineValidationDecider>(declaration))
			{
				context.EnableRule(r => r.IsRuleCD5161ActiveForJI_CountryOfOrigin);
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				instruction.CEI_Style = "I1";
				invoiceLine.JI_PrimaryPreference = "475";
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();

				var countryOfOriginRequiredForPreferenceMsgError = $"[CD5161] Preferential Origin ({invoiceLine.JI_CountryOfOriginInfo.Description}) is required when the first digit of {invoiceLine.JI_PrimaryPreferenceInfo.Description} is ‘2’, ‘3’, ‘4’ or ‘5’.";
				AssertHasMessageError("PrimaryPreference starts with 4", invoiceLine.JI_CountryOfOriginInfo, countryOfOriginRequiredForPreferenceMsgError);

				invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.Netherlands;
				AssertNoMessageError(invoiceLine.ZG_CountryOfSupplyInfo, countryOfOriginRequiredForPreferenceMsgError);

				context.DisableRule(r => r.IsRuleCD5161ActiveForJI_CountryOfOrigin);
				//var invoice = declaration.Invoices.AddNew();
				//invoiceLine = invoice.JobComInvoiceLines.AddNew();
				//var instruction = declaration.CustomsEntryInstructions.AddNew();
				//invoiceLine.JI_CEI = instruction.PK;
				//instruction.CEI_Style = "I1";
				//invoiceLine.JI_PrimaryPreference = "475";
				//invoiceLine.Validation.ValidateJI_CountryOfOrigin();

				invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.Netherlands;
				AssertNoMessageErrors(invoiceLine.ZG_CountryOfSupplyInfo);
			}
		}

		public void TestCheckJI_CustomsQuantity()
		{
			var errorMessage = "[CD9102] Net Weight in KG is required.";
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					entryInstruction.CEI_Style = EUCommonConstants.ImportDeclarationTypeList.H1;
					invoiceLine.JI_CustomsQuantity = 0m;
					invoiceLine.Validation.ValidateJI_CustomsQuantity();
					AssertHasMessageError("CEI_Style is 'H1' and JI_CustomsQuantity is 0", invoiceLine.JI_CustomsQuantityInfo, errorMessage);

					entryInstruction.CEI_Style = EUCommonConstants.ImportDeclarationTypeList.H3;
					invoiceLine.Validation.ValidateJI_CustomsQuantity();
					AssertNoMessageError("CEI_Style is not 'H1' or 'H4'", invoiceLine.JI_CustomsQuantityInfo, errorMessage);

					entryInstruction.CEI_Style = EUCommonConstants.ImportDeclarationTypeList.H4;
					invoiceLine.Validation.ValidateJI_CustomsQuantity();
					AssertHasMessageError("CEI_Style is 'H4' and JI_CustomsQuantity is 0", invoiceLine.JI_CustomsQuantityInfo, errorMessage);

					invoiceLine.JI_CustomsQuantity = 10m;
					invoiceLine.Validation.ValidateJI_CustomsQuantity();
					AssertNoMessageError("CEI_Style is 'H1' or 'H4' and JI_CustomsQuantity is not 0", invoiceLine.JI_CustomsQuantityInfo, errorMessage);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					entryInstruction.CEI_Style = EUCommonConstants.ImportDeclarationTypeList.H1;
					invoiceLine.JI_CustomsQuantity = 0m;
					invoiceLine.Validation.ValidateJI_CustomsQuantity();
					AssertNoMessageError("Rule CD9102 is not applied when declaration is not UCC6", invoiceLine.JI_CustomsQuantityInfo, errorMessage);
				}
			});
		}

		public void TestCheckJI_CustomsThirdQuantity()
		{
			invoiceLine.JI_CustomsThirdUnitQty = "KGM";
			invoiceLine.JI_CustomsThirdQuantity = ZDecimal.Zero;
			AssertHasMessageError(invoiceLine.JI_CustomsThirdQuantityInfo, "You have not entered a [44] Third Qty.");

			invoiceLine.JI_CustomsThirdQuantity = 1m;
			AssertNoMessageError(invoiceLine.JI_CustomsThirdQuantityInfo, "You have not entered a [44] Third Qty.");

			invoiceLine.JI_CustomsThirdUnitQty = ZString.Empty;
			invoiceLine.JI_CustomsThirdQuantity = ZDecimal.Zero;
			AssertNoMessageErrors(invoiceLine.JI_CustomsThirdQuantityInfo);
		}

		public void TestCheckJI_Tariff_UniqueRateForRateType()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var latviaCountryCode = CountryCodes.Latvia;
			var testHelper = new UniversalReferenceTestDataHelper(Factory);

			var tradeGroupStandard = testHelper.CreateTradeGroup(latviaCountryCode, "STANDARD", date1, date4);
			testHelper.AddCountry(tradeGroupStandard, CountryCodes.Australia, date1, date4);

			var tariffType = testHelper.CreateNewOrGetExistingTariffType(latviaCountryCode, Constants.TariffTypes.Import);
			Factory.Save();
			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(latviaCountryCode, Constants.RateTypes.Duty, "Duty");
			var rateCode11 = testHelper.LoadOrCreateNewCusRateCode(Factory, "A00", dutyRateType.PK);
			var rateCode12 = testHelper.LoadOrCreateNewCusRateCode(Factory, "A20", dutyRateType.PK);
			var addRateType = testHelper.CreateNewOrGetExistingRateType(latviaCountryCode, Constants.RateTypes.AntiDumping, "Anti-Dumping");
			var rateCode21 = testHelper.LoadOrCreateNewCusRateCode(Factory, "AD1", addRateType.PK);
			var rateCode22 = testHelper.LoadOrCreateNewCusRateCode(Factory, "AD2", addRateType.PK);
			var cvdRateType = testHelper.CreateNewOrGetExistingRateType(latviaCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.CountervailingDuty, "CVD");
			var rateCode31 = testHelper.LoadOrCreateNewCusRateCode(Factory, "CV1", cvdRateType.PK);
			var rateCode32 = testHelper.LoadOrCreateNewCusRateCode(Factory, "CV2", cvdRateType.PK);
			var preferenceSTD = testHelper.CreatePreferenceForCountry("STD", "Standard", latviaCountryCode);
			var preferenceTWO = testHelper.CreatePreferenceForCountry("TWO", "TWO Matched", latviaCountryCode);
			var preferenceZRO = testHelper.CreatePreferenceForCountry("ZRO", "ZERO Matched", latviaCountryCode);

			var cusTariff = testHelper.CreateTariff(latviaCountryCode, tariffType.PK, "1234567890", date1, date4, "dummy Description 0");

			var testRate1 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date4, "0", preferencePk: preferenceSTD.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4);
			var testRate2 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date4, "0", preferencePk: preferenceTWO.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4);
			var testRate3 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date4, "0", preferencePk: preferenceTWO.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate3, tradeGroupStandard, date1, date4);
			var testRate4 = testHelper.CreateRate(cusTariff, rateCode12.PK, date1, date4, "0", preferencePk: preferenceSTD.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate4, tradeGroupStandard, date1, date4);
			var testRate5 = testHelper.CreateRate(cusTariff, rateCode21.PK, date1, date4, "0", preferencePk: preferenceSTD.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate5, tradeGroupStandard, date1, date4);
			var testRate6 = testHelper.CreateRate(cusTariff, rateCode31.PK, date1, date4, "0", preferencePk: preferenceSTD.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate6, tradeGroupStandard, date1, date4);
			var testRate7 = testHelper.CreateRate(cusTariff, rateCode21.PK, date1, date4, "0", preferencePk: preferenceTWO.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate7, tradeGroupStandard, date1, date4);
			var testRate8 = testHelper.CreateRate(cusTariff, rateCode21.PK, date1, date4, "0", preferencePk: preferenceTWO.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate8, tradeGroupStandard, date1, date4);
			var testRate9 = testHelper.CreateRate(cusTariff, rateCode31.PK, date1, date4, "0", preferencePk: preferenceTWO.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate9, tradeGroupStandard, date1, date4);
			var testRate10 = testHelper.CreateRate(cusTariff, rateCode31.PK, date1, date4, "0", preferencePk: preferenceTWO.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate10, tradeGroupStandard, date1, date4);
			var testRate11 = testHelper.CreateRate(cusTariff, rateCode31.PK, date1, date4, "0", preferencePk: preferenceZRO.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate11, tradeGroupStandard, date1, date4, "", "ORD11");
			Factory.Save();

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_CountryOfOrigin = CountryCodes.Australia;
			var effectiveDate = invoiceLine.EffectiveAssessmentDate;

			CombineAssertions(() =>
			{
				invoiceLine.JI_PrimaryPreference = "TWO";
				AssertHasMessageErrorContaining("More than one applicable Duty rate", invoiceLine.JI_TariffInfo, $"There is more than one applicable Duty rate with rate code A00 for the Tariff '1234567890' and Country Of Origin 'AU' as at {effectiveDate} in combination with other data entered on the form.");
				AssertHasMessageErrorContaining("More than one applicable Anti-Dumping rate", invoiceLine.JI_TariffInfo, $"There is more than one applicable Anti-Dumping rate with rate code AD1 for the Tariff '1234567890' and Country Of Origin 'AU' as at {effectiveDate} in combination with other data entered on the form.");
				AssertHasMessageErrorContaining("More than one applicable CVD rate", invoiceLine.JI_TariffInfo, $"There is more than one applicable CVD rate with rate code CV1 for the Tariff '1234567890' and Country Of Origin 'AU' as at {effectiveDate} in combination with other data entered on the form.");

				invoiceLine.JI_PrimaryPreference = "ZRO";
				AssertHasMessageErrorContaining("no applicable Duty rate", invoiceLine.JI_TariffInfo, $"There is no applicable Duty rate for the Tariff '1234567890' and Country Of Origin 'AU' as at {effectiveDate} in combination with other data entered on the form.");
				AssertHasMessageErrorContaining("has valid Duty rate", invoiceLine.JI_TariffInfo, "Valid Duty rates exist where\r\n1: Preference = STD\r\n2: Preference = TWO\r\n");
				AssertHasMessageErrorContaining("no applicable Anti-Dumping rate", invoiceLine.JI_TariffInfo, $"There is no applicable Anti-Dumping rate for the Tariff '1234567890' and Country Of Origin 'AU' as at {effectiveDate} in combination with other data entered on the form.");
				AssertHasMessageErrorContaining("has valid Anti-Dumping rate", invoiceLine.JI_TariffInfo, "Valid Anti-Dumping rates exist where\r\n1: Preference = STD\r\n2: Preference = TWO\r\n");
				AssertHasMessageErrorContaining("no applicable CVD rate", invoiceLine.JI_TariffInfo, $"There is no applicable CVD rate for the Tariff '1234567890' and Country Of Origin 'AU' as at {effectiveDate} in combination with other data entered on the form.");
				AssertHasMessageErrorContaining("has valid CVD rate", invoiceLine.JI_TariffInfo, "Valid CVD rates exist where\r\n1: Preference = ZRO and Quota = ORD11\r\n");

				invoiceLine.JI_ConcessionOrder = "ORD11";
				AssertNoMessageErrorContaining("has applicable CVD rate", invoiceLine.JI_TariffInfo, $"There is no applicable CVD rate for the Tariff '1234567890' and Country Of Origin 'AU' as at {effectiveDate} in combination with other data entered on the form.");
				AssertNoMessageErrorContaining("has applicable CVD rate", invoiceLine.JI_TariffInfo, "Valid CVD rates exist where\r\n1: Preference = ZRO and Quota = ORD11\r\n");

				invoiceLine.JI_ConcessionOrder = "";
				invoiceLine.JI_PrimaryPreference = "STD";
				AssertNoMessageErrors("Single Result For DUTY and Anti-Dumping and CVD rate", invoiceLine.JI_TariffInfo);

				invoiceLine.JI_CountryOfOrigin = CountryCodes.Canada;
				AssertNoMessageErrors("No applicable and valid Duty&ADD&CVD rate for CA", invoiceLine.JI_TariffInfo);
			});
		}

		public void TestCheckJI_Tariff_CheckWithSpecificRateCode()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var latviaCountryCode = CountryCodes.Latvia;
			var testHelper = new UniversalReferenceTestDataHelper(Factory);

			var tradeGroupStandard = testHelper.CreateTradeGroup(latviaCountryCode, "STANDARD", date1, date4);
			testHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, date1, date4);

			var tariffType = testHelper.CreateNewOrGetExistingTariffType(latviaCountryCode, Constants.TariffTypes.Import);
			Factory.Save();
			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(latviaCountryCode, Constants.RateTypes.Duty, "Duty");
			var addRateType = testHelper.CreateNewOrGetExistingRateType(latviaCountryCode, Constants.RateTypes.AntiDumping, "AntiDumping");
			var cvdRateType = testHelper.CreateNewOrGetExistingRateType(latviaCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.CountervailingDuty, "CountervailingDuty");
			var rateCode11 = testHelper.LoadOrCreateNewCusRateCode(Factory, "A00", dutyRateType.PK);
			var rateCode12 = testHelper.LoadOrCreateNewCusRateCode(Factory, "A20", dutyRateType.PK);
			var rateCode13 = testHelper.LoadOrCreateNewCusRateCode(Factory, "000", dutyRateType.PK);
			var preferenceSTD = testHelper.CreatePreferenceForCountry("STD", "Standard", latviaCountryCode);
			var preferenceTWO = testHelper.CreatePreferenceForCountry("TWO", "TWO Matched", latviaCountryCode);
			var preferenceZRO = testHelper.CreatePreferenceForCountry("ZRO", "ZERO Matched", latviaCountryCode);

			var cusTariff = testHelper.CreateTariff(latviaCountryCode, tariffType.PK, "1234567890", date1, date4, "dummy Description 0");

			var testRate1 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date4, "0", preferencePk: preferenceSTD.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4);
			var testRate2 = testHelper.CreateRate(cusTariff, rateCode12.PK, date1, date4, "0", preferencePk: preferenceSTD.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4);
			var testRate3 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date4, "0", preferencePk: preferenceTWO.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate3, tradeGroupStandard, date1, date4);
			var testRate4 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date4, "0", preferencePk: preferenceTWO.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate4, tradeGroupStandard, date1, date4);
			var testRate5 = testHelper.CreateRate(cusTariff, rateCode12.PK, date1, date4, "0", preferencePk: preferenceTWO.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate5, tradeGroupStandard, date1, date4);
			var testRate6 = testHelper.CreateRate(cusTariff, rateCode12.PK, date1, date4, "0", preferencePk: preferenceTWO.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate6, tradeGroupStandard, date1, date4);
			var testRate7 = testHelper.CreateRate(cusTariff, rateCode13.PK, date1, date4, "0", preferencePk: preferenceZRO.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate7, tradeGroupStandard, date1, date4);
			Factory.Save();

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_CountryOfOrigin = CountryCodes.Australia;

			CombineAssertions(() =>
			{
				invoiceLine.JI_PrimaryPreference = "STD";
				AssertNoMessageErrors("Single Result For DUTY rate with ratecode = A00/A20 and ADD&CVD rate", invoiceLine.JI_TariffInfo);
				invoiceLine.JI_PrimaryPreference = "TWO";
				AssertHasMessageErrorContaining("Has more than one RateCode A00 rate for preference = TWO", invoiceLine.JI_TariffInfo, $"There is more than one applicable Duty rate with rate code A20 for the Tariff '1234567890' and Country Of Origin 'AU' as at {invoiceLine.EffectiveAssessmentDate} in combination with other data entered on the form.");
				AssertHasMessageErrorContaining("Has more than one RateCode A20 rate for preference = TWO", invoiceLine.JI_TariffInfo, $"There is more than one applicable Duty rate with rate code A00 for the Tariff '1234567890' and Country Of Origin 'AU' as at {invoiceLine.EffectiveAssessmentDate} in combination with other data entered on the form.");
				invoiceLine.JI_PrimaryPreference = "ZRO";
				AssertHasMessageErrorContaining("Has no RateCode A00 rate for preference = ZRO", invoiceLine.JI_TariffInfo, "No DUTY rate with Rate Code A00 exists.");
			});
		}

		public void TestCheckJI_Tariff_SupplementaryCodeForMeursing()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var latviaCountryCode = CountryCodes.Latvia;
			var testHelper = new UniversalReferenceTestDataHelper(Factory);

			var tradeGroupStandard = testHelper.CreateTradeGroup(latviaCountryCode, "STANDARD", date1, date4);
			testHelper.AddCountry(tradeGroupStandard, CountryCodes.Australia, date1, date4);

			var tariffType = testHelper.CreateNewOrGetExistingTariffType(latviaCountryCode, Constants.TariffTypes.Import);
			Factory.Save();
			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(latviaCountryCode, Constants.RateTypes.Duty, "Duty");
			var addRateType = testHelper.CreateNewOrGetExistingRateType(latviaCountryCode, Constants.RateTypes.AntiDumping, "AntiDumping");
			var cvdRateType = testHelper.CreateNewOrGetExistingRateType(latviaCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.CountervailingDuty, "CountervailingDuty");
			var rateCode11 = testHelper.LoadOrCreateNewCusRateCode(Factory, "A00", dutyRateType.PK);
			var rateCode12 = testHelper.LoadOrCreateNewCusRateCode(Factory, "A20", dutyRateType.PK);
			var preferenceSTD = testHelper.CreatePreferenceForCountry("STD", "Standard", latviaCountryCode);
			var preferenceTWO = testHelper.CreatePreferenceForCountry("TWO", "TWO", latviaCountryCode);
			var preferenceTRD = testHelper.CreatePreferenceForCountry("TRD", "TRD", latviaCountryCode);
			var preferenceFou = testHelper.CreatePreferenceForCountry("Fou", "Fourth", latviaCountryCode);

			var cusTariff = testHelper.CreateTariff(latviaCountryCode, tariffType.PK, "1234567890", date1, date4, "dummy Description 0");

			var testRate1 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date4, rateFormula: "#ADFM(5)#", preferencePk: preferenceSTD.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, additionalCode: "7444");
			testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, additionalCode: "7454");
			testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, additionalCode: "8444");
			var testRate2 = testHelper.CreateRate(cusTariff, rateCode12.PK, date1, date4, rateFormula: "#ADFM(5)#", preferencePk: preferenceTRD.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4);
			var testRate3 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date4, rateFormula: "#EA(5)#", preferencePk: preferenceTWO.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate3, tradeGroupStandard, date1, date4, additionalCode: "7444");
			testHelper.CreateCusApplicability(testRate3, tradeGroupStandard, date1, date4, additionalCode: "7454");
			testHelper.CreateCusApplicability(testRate3, tradeGroupStandard, date1, date4, additionalCode: "8444");
			var testRate4 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date4, rateFormula: "0", preferencePk: preferenceTRD.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate4, tradeGroupStandard, date1, date4, additionalCode: "1");
			testHelper.CreateCusApplicability(testRate4, tradeGroupStandard, date1, date4, additionalCode: "2");
			testHelper.CreateCusApplicability(testRate4, tradeGroupStandard, date1, date4, additionalCode: "3");
			var testRate5 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date4, rateFormula: "0", preferencePk: preferenceFou.PK, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate5, tradeGroupStandard, date1, date4, additionalCode: "1");
			Factory.Save();

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_CountryOfOrigin = CountryCodes.Australia;

			CombineAssertions(() =>
			{
				var messageError = "Exactly one supplementary code matching pattern '7NNN' e.g. 7086, should exist for Meursing duty calculation purposes.";
				invoiceLine.JI_PrimaryPreference = "STD";
				AssertNoMessageErrorContaining("No error with no SupplementaryCode", invoiceLine.JI_TariffInfo, messageError);
				invoiceLine.JI_SupplementaryCode1 = "888";
				AssertNoMessageErrorContaining("Has error with preference='STD' and SupplementaryCode = 888 and ratecode = A00", invoiceLine.JI_TariffInfo, messageError);
				invoiceLine.JI_SupplementaryCode1 = "7444";
				AssertNoMessageErrorContaining("No error with preference='STD' and SupplementaryCode = 7444", invoiceLine.JI_TariffInfo, messageError);
				invoiceLine.JI_SupplementaryCode2 = "7454";
				AssertHasMessageErrorContaining("Has error with preference='STD' and two SupplementaryCodes start with 7 and ratecode = A00", invoiceLine.JI_TariffInfo, messageError);
				invoiceLine.JI_SupplementaryCode2 = "8444";
				AssertNoMessageErrorContaining("No error with preference='STD' and SupplementaryCode = 8444", invoiceLine.JI_TariffInfo, messageError);

				invoiceLine.JI_PrimaryPreference = "TWO";
				AssertNoMessageErrorContaining("No error with preference='TWO' and no SupplementaryCode", invoiceLine.JI_TariffInfo, messageError);
				invoiceLine.JI_SupplementaryCode1 = "888";
				AssertHasMessageErrorContaining("No error with preference='TWO' and SupplementaryCode = 888", invoiceLine.JI_TariffInfo, messageError);
				invoiceLine.JI_SupplementaryCode1 = "7444";
				AssertNoMessageErrorContaining("No error with preference='TWO' and SupplementaryCode = 7444", invoiceLine.JI_TariffInfo, messageError);
				invoiceLine.JI_SupplementaryCode2 = "7454";
				AssertHasMessageErrorContaining("Has error with preference='TWO' and two SupplementaryCodes start with 7", invoiceLine.JI_TariffInfo, messageError);
				invoiceLine.JI_SupplementaryCode2 = "8444";
				AssertNoMessageErrorContaining("No error with preference='TWO' and SupplementaryCode = 8444", invoiceLine.JI_TariffInfo, messageError);

				invoiceLine.JI_PrimaryPreference = "TRD";
				AssertNoMessageErrorContaining("No error with preference='TRD' and no SupplementaryCode", invoiceLine.JI_TariffInfo, messageError);
				invoiceLine.JI_SupplementaryCode1 = "888";
				AssertHasMessageErrorContaining("Has error with preference='TRD' and SupplementaryCode = 888 and ratecode = A20", invoiceLine.JI_TariffInfo, messageError);
				invoiceLine.JI_SupplementaryCode1 = "7444";
				AssertNoMessageErrorContaining("No error with preference='TRD' and SupplementaryCode = 7444", invoiceLine.JI_TariffInfo, messageError);
				invoiceLine.JI_SupplementaryCode2 = "7454";
				AssertHasMessageErrorContaining("Has error with preference='TRD' and two SupplementaryCodes start with 7 and ratecode = A20", invoiceLine.JI_TariffInfo, messageError);

				var messageError2 = "No supplementary code matching pattern '7NNN' should exist when Meursing is not applicable.";
				invoiceLine.JI_PrimaryPreference = "FOU";

				invoiceLine.JI_SupplementaryCode2 = ZString.Empty;
				invoiceLine.JI_SupplementaryCode1 = "1";
				AssertNoMessageErrorContaining("No error with preference='FOU' and no SupplementaryCode", invoiceLine.JI_TariffInfo, messageError);
				AssertNoMessageErrorContaining("No error with preference='FOU' and no SupplementaryCode", invoiceLine.JI_TariffInfo, messageError2);
				invoiceLine.JI_SupplementaryCode1 = "7444";
				AssertHasMessageErrorContaining("Has error with preference='TRD' and SupplementaryCode = 888 and ratecode = A20", invoiceLine.JI_TariffInfo, messageError2);
			});
		}

		public void TestCheckJI_Tariff_SupplementaryCodesExist()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date2 = new ZDate(2079, 06, 06);
			var testHelper = new UniversalReferenceTestDataHelper(Factory);
			var latviaCountryCode = CountryCodes.Latvia;

			var tradeGroupStandard = testHelper.CreateTradeGroup(latviaCountryCode, "STANDARD", date1, date2);
			testHelper.AddCountry(tradeGroupStandard, CountryCodes.Australia, date1, date2);
			var hsnTariffType = testHelper.CreateNewOrGetExistingTariffType(latviaCountryCode, Constants.TariffTypes.Import);
			Factory.Save();
			var cusTariff = testHelper.CreateTariff(latviaCountryCode, hsnTariffType.PK, "1234567890", date1, date2, "dummy Description 0");

			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(latviaCountryCode, Constants.RateTypes.Duty, "Duty");
			var rateCode11 = testHelper.LoadOrCreateNewCusRateCode(Factory, "A00", dutyRateType.PK);
			var testRate1 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date2, dataGrouping: latviaCountryCode);
			testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date2, additionalCode: "7444");
			Factory.Save();

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				invoiceLine.JI_Tariff = "TEST";
				AssertNoWarningContaining(invoiceLine.JI_TariffInfo, "Supplementary Codes exist for this tariff");

				invoiceLine.JI_Tariff = "1234567890";
				invoiceLine.JI_CountryOfOrigin = CountryCodes.Australia;
				AssertHasWarningContaining(invoiceLine.JI_TariffInfo, "Supplementary Codes exist for this tariff");

				invoiceLine.JI_SupplementaryCode1 = "7444";
				AssertNoWarningContaining(invoiceLine.JI_TariffInfo, "Supplementary Codes exist for this tariff");
			});
		}

		public void TestCheckRuleC0820_ForJI_Tariff()
		{
			var messageError = "[C0820] This field is mandatory for any case besides Additional procedure F15.";

			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				var invoice = declaration.Invoices.AddNew();

				var line = invoice.InvoiceLines.AddNew();
				line.JI_FormattedProcedure = "0000F16";
				line.Validation.ValidateJI_Tariff();
				AssertNoMessageErrorContaining("No validation run is expected for RuleC0820 for concession other than F15 when declaration is of export type.", line.JI_TariffInfo, messageError);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				line.Validation.ValidateJI_Tariff();
				AssertHasMessageError("A message error is expected for RuleC0820 for concession other than F15 when Tariff Code is not set.", line.JI_TariffInfo, messageError);

				line = invoice.InvoiceLines.AddNew();
				line.JI_Tariff = "01234567";
				line.JI_FormattedProcedure = "0000F16";
				line.Validation.ValidateJI_Tariff();
				AssertHasMessageError("A message error is expected for RuleC0820 for concession other than F15 when no Taric Code(JI_Tariff(9, 10)) value is set.", line.JI_TariffInfo, messageError);

				line = invoice.InvoiceLines.AddNew();
				line.JI_Tariff = "123456789";
				line.JI_FormattedProcedure = "0000F16";
				line.Validation.ValidateJI_Tariff();
				AssertHasMessageError("A message error is expected for RuleC0820 for concession other than F15 when Taric Code(JI_Tariff(9, 10)) length less than 2.", line.JI_TariffInfo, messageError);

				line = invoice.InvoiceLines.AddNew();
				line.JI_Tariff = "0123456789";
				line.JI_FormattedProcedure = "0000F16";
				line.Validation.ValidateJI_Tariff();
				AssertNoMessageErrorContaining("No message error is expected for RuleC0820 for concession other than F15 when Taric Code(JI_Tariff(9, 10)) has length at least 2.", line.JI_TariffInfo, messageError);

				line = invoice.InvoiceLines.AddNew();
				line.JI_FormattedProcedure = "0000F15";
				line.Validation.ValidateJI_Tariff();
				AssertNoMessageErrorContaining("No validation should run for RuleC0820 when concession is F15.", line.JI_TariffInfo, messageError);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var line = invoice.InvoiceLines.AddNew();
				line.JI_FormattedProcedure = "0000F16";
				line.Validation.ValidateJI_Tariff();
				AssertNoMessageErrorContaining("No validation should run for RuleC0820 for concession other than F15 when declaration is Non UCC6.", line.JI_TariffInfo, messageError);
			}

			using (var context = new InvoiceLineValidationDeciderTestContext<IImportInvoiceLineValidationDecider>(declaration))
			{
				context.DisableRule(x => x.IsRuleC0820ActiveForJI_Tariff);

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				var line = declaration
					.Invoices.AddNew()
					.InvoiceLines.AddNew();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				line.JI_FormattedProcedure = "0000F16";
				line.Validation.ValidateJI_Tariff();
				AssertNoMessageErrorContaining("No validation run is expected for RuleC0820 for concession other than F15 when Rule C0820 is disabled.", line.JI_TariffInfo, messageError);
			}
		}

		public void TestCheckRuleC0820_ForJI_SupplementaryCode1()
		{
			SetUpReferenceData();

			var messageError = "[C0820] This field is mandatory for any case besides Additional procedure F15.";

			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				var invoice = declaration.Invoices.AddNew();
				var line = invoice.InvoiceLines.AddNew();
				line.JI_FormattedProcedure = "0000F16";
				line.Validation.ValidateJI_SupplementaryCode1();
				AssertNoMessageErrorContaining("No validation should run for RuleC0820 for concession other than F15 when declaration is of export type.", line.JI_SupplementaryCode1Info, messageError);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				invoice.JZ_RN_NKDefaultOrigin = "FR";
				line.Validation.ValidateJI_SupplementaryCode1();
				AssertNoMessageErrorContaining("No validation should run for RuleC0820 for concession other than F15 when JI_SupplementaryCode1 list is empty.", line.JI_SupplementaryCode1Info, messageError);

				line.JI_Tariff = "99999999";
				line.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
				line.JI_FormattedProcedure = "0000F17";
				line.JI_SupplementaryCode1 = "";
				line.Validation.ValidateJI_SupplementaryCode1();
				AssertHasMessageError("A message error is expected for RuleC0820 for concession other than F15 when JI_SupplementaryCode1 is empty.", line.JI_SupplementaryCode1Info, messageError);

				line.JI_SupplementaryCode1 = "Q003";
				line.Validation.ValidateJI_SupplementaryCode1();
				AssertNoMessageError("No validation should run for RuleC0820 for concession other than F15 when JI_SupplementaryCode1 value is set.", line.JI_SupplementaryCode1Info, messageError);

				var line2 = invoice.InvoiceLines.AddNew();
				line2.JI_SupplementaryCode1 = "1234";
				line2.JI_FormattedProcedure = "0000F16";
				line.Validation.ValidateJI_SupplementaryCode1();
				AssertNoNotifications("No validation should run for RuleC0820 for concession other than F15 when JI_SupplementaryCode1 value is set.", line2.JI_SupplementaryCode1Info);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var line = invoice.InvoiceLines.AddNew();
				line.JI_FormattedProcedure = "0000F16";
				line.Validation.ValidateJI_SupplementaryCode1();
				AssertNoMessageErrorContaining("No validation should run for RuleC0820 for concession other than F15 when declaration is non UCC6.", line.JI_SupplementaryCode1Info, messageError);
			}

			using (var context = new InvoiceLineValidationDeciderTestContext<IImportInvoiceLineValidationDecider>(declaration))
			{
				context.DisableRule(x => x.IsRuleC0820ActiveForJI_SupplementaryCode1);

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				var line = declaration
					.Invoices.AddNew()
					.InvoiceLines.AddNew();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				line.JI_FormattedProcedure = "0000F16";
				line.Validation.ValidateJI_SupplementaryCode1();
				AssertNoMessageErrorContaining("No validation run is expected for RuleC0820 for concession other than F15 when Rule C0820 is disabled.", line.JI_SupplementaryCode1Info, messageError);
			}

			void SetUpReferenceData()
			{
				var startDate = ZDateTime.Today.AddDays(-2);
				var endDate = ZDateTime.Today.AddDays(2);
				var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingDataGrouping(currentCountry).ZZZ_ZZZ_Grouping = helper.CreateNewOrGetExistingDataGrouping("FR").PK;
				var impTariffTypePK = helper.CreateNewOrGetExistingTariffType("FR", "IMP").PK;
				Factory.Save();

				var tariff = helper.CreateTariff(currentCountry, impTariffTypePK, "99999999", ZDateTime.BrettsBirthday,
					endDate, "Alpha Bravo");

				helper.CreateTaxOrFee("VT1", 0.02m, currentCountry, description: "VAT One");
				helper.CreateTaxOrFee("VT2", 0.01m, currentCountry, description: "VAT Two");

				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT1", startDate: startDate,
					endDate: endDate, additionalCode: "Add1");
				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT2", startDate: startDate,
					endDate: endDate, additionalCode: "");

				helper.CreateCusCodeType("ADDIN", "Additional Code");
				helper.CreateCusCodeList(currentCountry, "ADDIN", "Add1", "Test Additional Code 1",
					startDate: startDate, endDate: endDate);

				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "IT1", additionalCode: "Add1", startDate, endDate);
				helper.CreateTaxOrFee("IT1", 0.02m, currentCountry);

				Factory.Save();

				var asiaTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.France, ZDateTime.BrettsBirthday, new ZDateTime(2079, 6, 6));
				helper.AddCountry(asiaTradeGroup, Core.Constants.CountryCodes.China);

				var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.France, "DTY");
				var rateCode = helper.CreateCusRateCode(Factory, "A00", rateType.PK);
				var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode.PK, ZDateTime.BrettsBirthday, new ZDateTime(2079, 6, 6), "VDF * 10%");
				helper.CreateCusApplicability(rate1, asiaTradeGroup, ZDateTime.BrettsBirthday, new ZDateTime(2079, 6, 6), "Q003");

				Factory.Save();
			}
		}

		public void TestCheckJI_CountryOfOrigin_ListValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Latvia).ZZZ_ZZZ_Grouping = eun.PK;
			var group1011 = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, "1011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(group1011, CountryCodes.France, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CountryOfOriginInfo, "ZZ", CountryCodes.France);
		}

		public void TestCheckJI_Procedure()
		{
			var latviaCountryCode = CountryCodes.Latvia;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(latviaCountryCode, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			helper.CreateRefCusProcedure(latviaCountryCode, "A", "22", "22", "222", "Two", "IMP", group: "IFD");
			helper.CreateRefCusProcedure(latviaCountryCode, "B", "33", "33", "333", "Three", "IMP", group: "ICR");
			helper.CreateRefCusProcedure(latviaCountryCode, "A", "44", "44", "444", "Four", "EXP", group: "EFD");
			helper.CreateRefCusProcedure(latviaCountryCode, "A", "11", "11", "555", "Five", "IMP", group: "IFD");

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var invLine = Factory.New<JobComInvLineForTest>();
			dec.Invoices.AddNew().InvoiceLines.Add(invLine);
			invLine.JI_JZ = dec.Invoices[0].PK;

			invLine.JI_RN_NKCountryOfExport = CountryCodes.Latvia;
			var addtionalProcedueCode = invLine.AdditionalProcedureCodes.AddNew();
			addtionalProcedueCode.CY_Code = "1111333";

			invLine.JI_Procedure = "2222222";
			AssertHasMessageError(invLine.JI_ProcedureInfo, "The first 4 characters of additional procedure codes should be all the same as the main procedure code's.");

			invLine.JI_Procedure = "1111111";
			AssertNoMessageError(invLine.JI_ProcedureInfo, "The first 4 characters of additional procedure codes should be all the same as the main procedure code's.");

			invLine.JI_Procedure = "1111333";
			AssertHasMessageError(invLine.JI_ProcedureInfo, "Procedure code should not be duplicated in Additional Procedure codes.");

			invLine.JI_Procedure = "1111111";
			AssertNoMessageError(invLine.JI_ProcedureInfo, "Procedure code should not be duplicated in Additional Procedure codes.");
		}

		public void TestCheckJI_Procedure_ValidateFiscalReferences_CannotBeEntered()
		{
			const string fiscalReferencesCannotBeEntered = "Fiscal Representation should only be entered when CPC starts with 42 or 63.";

			var invoiceLineConfigurationMock = new Mock<InvoiceLineConfiguration>();
			invoiceLineConfigurationMock.Protected()
				.Setup<ZBool>("FiscalReferencesSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(true);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInvoiceLineConfiguration", invoiceLineConfigurationMock.Object, null))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				CombineAssertions(() =>
				{
					AssertFiscalReference("No error message - CPC 40", instruction, false, invoiceLine, "4000000", false, false, fiscalReferencesCannotBeEntered);
					AssertFiscalReference("Error message - CPC 40 - fiscal reference on invoice line", instruction, false, invoiceLine, "4000000", true, true, fiscalReferencesCannotBeEntered);
				});
			}
		}

		public void TestCheckJI_Procedure_ValidateFiscalReferences_MustBeEntered_FiscalReferencesSupportedOnInvoiceLineOnly()
		{
			const string fiscalReferencesMustBeEntered = "When CPC starts with 42 or 63, at least one Fiscal Reference record must exist at the Line level.";

			var invoiceLineConfigurationMock = new Mock<InvoiceLineConfiguration>();
			invoiceLineConfigurationMock.Protected().Setup<ZBool>("FiscalReferencesSupportCore", ItExpr.IsAny<BusinessObject>()).Returns(true);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInvoiceLineConfiguration", invoiceLineConfigurationMock.Object, null))
			{
				CombineAssertions(() =>
				{
					var declaration = Factory.New<JobDeclaration>();
					var instruction = declaration.CustomsEntryInstructions.AddNew();
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					invoiceLine.JI_CEI = instruction.PK;

					AssertFiscalReference("No error message - CPC 40", instruction, false, invoiceLine, "4000000", false, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("Error message - CPC 42 - no fiscal reference", instruction, false, invoiceLine, "4200000", false, true, fiscalReferencesMustBeEntered);
					AssertFiscalReference("Error message - CPC 63 - no fiscal reference", instruction, false, invoiceLine, "6300000", false, true, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 42 - fiscal reference on invoice line", instruction, false, invoiceLine, "4200000", true, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 63 - fiscal reference on invoice line", instruction, false, invoiceLine, "6300000", true, false, fiscalReferencesMustBeEntered);
				});
			}
		}

		public void TestCheckJI_Procedure_ValidateFiscalReferences_MustBeEntered_FiscalReferencesSupportedOnInstructionOnly()
		{
			const string fiscalReferencesMustBeEntered = "When CPC starts with 42 or 63, at least one Fiscal Reference record must exist at the Instruction level.";

			var invoiceLineConfigurationMock = new Mock<InstructionConfiguration>();
			invoiceLineConfigurationMock.CallBase = true;
			invoiceLineConfigurationMock.Protected()
				.Setup<ZBool>("FiscalReferencesSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(true);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInstructionConfiguration", invoiceLineConfigurationMock.Object, null))
			{
				CombineAssertions(() =>
				{
					var declaration = Factory.New<JobDeclaration>();
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();

					AssertFiscalReference("No error message - CPC 40", null, false, invoiceLine, "4000000", false, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("Error message - CPC 42 - no instruction; no fiscal reference", null, false, invoiceLine, "4200000", false, true, fiscalReferencesMustBeEntered);
					AssertFiscalReference("Error message - CPC 63 - no instruction; no fiscal reference", null, false, invoiceLine, "6300000", false, true, fiscalReferencesMustBeEntered);

					var instruction = declaration.CustomsEntryInstructions.AddNew();
					invoiceLine.JI_CEI = instruction.PK;
					AssertFiscalReference("Error message - CPC 42 - instruction; no fiscal reference", instruction, false, invoiceLine, "4200000", false, true, fiscalReferencesMustBeEntered);
					AssertFiscalReference("Error message - CPC 63 - instruction; no fiscal reference", instruction, false, invoiceLine, "6300000", false, true, fiscalReferencesMustBeEntered);

					AssertFiscalReference("No error message - CPC 42 - fiscal reference on instruction", instruction, true, invoiceLine, "4200000", false, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 63 - fiscal reference on instruction", instruction, true, invoiceLine, "6300000", false, false, fiscalReferencesMustBeEntered);
				});
			}
		}

		public void TestCheckJI_Procedure_ValidateFiscalReferences_MustBeEntered_FiscalReferencesSupportedOnInvoiceLineAndInstruction()
		{
			const string fiscalReferencesMustBeEntered = "When CPC starts with 42 or 63, at least one Fiscal Reference record must exist at the Instruction or Line level.";

			var invoiceLineConfigurationMock1 = new Mock<InvoiceLineConfiguration>();
			invoiceLineConfigurationMock1.Protected()
				.Setup<ZBool>("FiscalReferencesSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(true);

			var instructionConfigurationMock1 = new Mock<InstructionConfiguration>();
			instructionConfigurationMock1.Protected()
				.Setup<ZBool>("FiscalReferencesSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(true);
			instructionConfigurationMock1.Protected()
				.Setup<ZBool>("FiscalReferencesSupportOnCPC42And63OnlyCore", ItExpr.IsAny<BusinessObject>())
				.Returns(true);

			var declarationConfigurationMock = new Mock<DeclarationConfiguration>();
			declarationConfigurationMock.CallBase = true;
			declarationConfigurationMock.Protected().Setup<InvoiceLineConfiguration>("GetNewInvoiceLineConfiguration")
				.Returns(invoiceLineConfigurationMock1.Object);
			declarationConfigurationMock.Protected().Setup<InstructionConfiguration>("GetNewInstructionConfiguration")
				.Returns(instructionConfigurationMock1.Object);

			var countryOrGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.ClearCachedValue<DeclarationConfiguration>($"DeclarationConfiguration_{countryOrGrouping}");

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject()).Returns(declarationConfigurationMock.Object);

			var declarationConfiguration = new KeyObjectHandleDictionaryObject
			{
				{ countryOrGrouping, objectHandleMock.Object }
			};

			using (ObjectFactory.Substitute(nameof(DeclarationConfiguration), declarationConfiguration))
			{
				CombineAssertions(() =>
				{
					var declaration = Factory.New<JobDeclaration>();
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();

					AssertFiscalReference("No error message - CPC 40", null, false, invoiceLine, "4000000", false, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("Error message - CPC 42 - no instruction; no fiscal reference", null, false, invoiceLine, "4200000", false, true, fiscalReferencesMustBeEntered);
					AssertFiscalReference("Error message - CPC 63 - no instruction; no fiscal reference", null, false, invoiceLine, "6300000", false, true, fiscalReferencesMustBeEntered);

					var instruction = declaration.CustomsEntryInstructions.AddNew();
					invoiceLine.JI_CEI = instruction.PK;
					AssertFiscalReference("Error message - CPC 42 - instruction; no fiscal reference", instruction, false, invoiceLine, "4200000", false, true, fiscalReferencesMustBeEntered);
					AssertFiscalReference("Error message - CPC 63 - instruction; no fiscal reference", instruction, false, invoiceLine, "6300000", false, true, fiscalReferencesMustBeEntered);

					AssertFiscalReference("No error message - CPC 42 - fiscal reference on instruction", instruction, true, invoiceLine, "4200000", false, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 63 - fiscal reference on instruction", instruction, true, invoiceLine, "6300000", false, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 42 - fiscal reference on invoice line", instruction, false, invoiceLine, "4200000", true, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 63 - fiscal reference on invoice line", instruction, false, invoiceLine, "6300000", true, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 42 - fiscal reference on instruction and invoice line", instruction, true, invoiceLine, "4200000", true, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 63 - fiscal reference on instruction and invoice line", instruction, true, invoiceLine, "6300000", true, false, fiscalReferencesMustBeEntered);
				});
			}

			var invoiceLineConfigurationMock2 = new Mock<InvoiceLineConfiguration>();
			invoiceLineConfigurationMock2.Protected()
				.Setup<ZBool>("FiscalReferencesSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(true);

			var instructionConfigurationMock2 = new Mock<InstructionConfiguration>();
			instructionConfigurationMock2.Protected()
				.Setup<ZBool>("FiscalReferencesSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(true);
			instructionConfigurationMock2.Protected()
				.Setup<ZBool>("FiscalReferencesSupportOnCPC42And63OnlyCore", ItExpr.IsAny<BusinessObject>())
				.Returns(false);

			var declarationConfigurationMock2 = new Mock<DeclarationConfiguration>();
			declarationConfigurationMock2.CallBase = true;
			declarationConfigurationMock2.Protected().Setup<InvoiceLineConfiguration>("GetNewInvoiceLineConfiguration")
				.Returns(invoiceLineConfigurationMock2.Object);
			declarationConfigurationMock2.Protected().Setup<InstructionConfiguration>("GetNewInstructionConfiguration")
				.Returns(instructionConfigurationMock2.Object);

			countryOrGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.ClearCachedValue<DeclarationConfiguration>($"DeclarationConfiguration_{countryOrGrouping}");

			var objectHandleMock2 = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject()).Returns(declarationConfigurationMock2.Object);

			var declarationConfiguration2 = new KeyObjectHandleDictionaryObject
			{
				{ countryOrGrouping, objectHandleMock.Object }
			};

			using (ObjectFactory.Substitute(nameof(DeclarationConfiguration), declarationConfiguration2))
			{
				CombineAssertions(() =>
				{
					var declaration = Factory.New<JobDeclaration>();
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();

					AssertFiscalReference("No error message - CPC 42 - no instruction; no fiscal reference", null, false, invoiceLine, "4200000", false, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 63 - no instruction; no fiscal reference", null, false, invoiceLine, "6300000", false, false, fiscalReferencesMustBeEntered);

					var instruction = declaration.CustomsEntryInstructions.AddNew();
					invoiceLine.JI_CEI = instruction.PK;
					AssertFiscalReference("No error message - CPC 42 - instruction; no fiscal reference", instruction, false, invoiceLine, "4200000", false, false, fiscalReferencesMustBeEntered);
					AssertFiscalReference("No error message - CPC 63 - instruction; no fiscal reference", instruction, false, invoiceLine, "6300000", false, false, fiscalReferencesMustBeEntered);
				});
			}
		}

		static void AssertFiscalReference(string scenarioName, CusEntryInstruction instruction, bool hasFiscalOnInstruction, JobComInvoiceLine invoiceLine, string cpc, bool hasFiscalOnItem, bool hasError, string errorMessage)
		{
			invoiceLine.JI_Procedure = cpc;

			if (instruction != null)
			{
				instruction.FiscalReferences.RemoveAndDeleteAll();
				if (hasFiscalOnInstruction)
				{
					var fiscalReference = instruction.FiscalReferences.AddNew();
					fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
					fiscalReference.CFR_Reference = "refInstruction";
				}
			}

			invoiceLine.FiscalReferences.RemoveAndDeleteAll();
			if (hasFiscalOnItem)
			{
				var fiscalReference = invoiceLine.FiscalReferences.AddNew();
				fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
				fiscalReference.CFR_Reference = "refInvoiceLine";
			}

			invoiceLine.Validation.ValidateJI_Procedure();
			if (hasError)
			{
				AssertHasMessageError(scenarioName, invoiceLine.JI_ProcedureInfo, errorMessage);
			}
			else
			{
				AssertNoMessageError(scenarioName, invoiceLine.JI_ProcedureInfo, errorMessage);
			}
		}

		public void TestParent()
		{
			AssertEquals(invoiceLine.Validation.Parent, invoiceLine);
		}

		public void TestEmptyPackaging()
		{
			declaration.JE_MasterBill = "M";
			var cw = declaration.Bills[0].PackingGroups[0].Packages.AddNew();
			cw.CW_PackQty = 1;

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasRowMessageErrorContaining(invoiceLine, "This line has no packaging details");

			var pack1 = (InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(cw);
			pack1.CHC_NumberOfPacks = 0;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoRowMessageErrorContaining(invoiceLine, "This line has no packaging details");
			pack1.Delete();
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasRowMessageErrorContaining(invoiceLine, "This line has no packaging details");

			pack1 = (InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(cw);
			pack1.CHC_NumberOfPacks = 1;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoRowMessageErrorContaining(invoiceLine, "This line has no packaging details");
			pack1.Delete();
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasRowMessageErrorContaining(invoiceLine, "This line has no packaging details");

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = "1.2.3";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasRowMessageErrorContaining(invoiceLine, "This line has no packaging details");
			AssertNoRowMessageErrorContaining(invoiceLine, "export sea");
			AssertNoRowMessageErrorContaining(invoiceLine, "consol");

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportModes.Sea;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasRowMessageErrorContaining(invoiceLine, "This line has no packaging details");
			AssertHasRowMessageErrorContaining(invoiceLine, "export sea");
			AssertNoRowMessageErrorContaining(invoiceLine, "consol");

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = "1.2.3";
			pack1 = (InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(cw);
			pack1.CHC_NumberOfPacks = 0;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoRowMessageErrorContaining(invoiceLine, "This line has no packaging details");
			pack1.Delete();
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasRowMessageErrorContaining(invoiceLine, "This line has no packaging details");

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var consol = Factory.New<Freight.Forwarding.Business.ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, declaration.CountryCode)).RL_Code;
			var shipment = consol.Shipments.AddNew();
			declaration.JE_JS = shipment.PK;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasRowMessageErrorContaining(invoiceLine, "This line has no packaging details");
			AssertHasRowMessageErrorContaining(invoiceLine, "export sea");
		}

		public void TestSupplementaryUnitsDemandNonZeroSupplementaryQuantityExceptForExcludedCpcsAndUnit119()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cpcOne = helper.CreateOrFindExistingRefCusProcedure(CountryCodes.Latvia, "00", "76", "54", "321", "Test - allows zero supp", "IMP", "IFD");
			var cpcTwo = helper.CreateOrFindExistingRefCusProcedure(CountryCodes.Latvia, "00", "12", "34", "567", "Test - normal, demands supp", "IMP", "IFD");
			cpcOne.Attributes.AddNew(AttributeNames.Codes.ZeroSupplementaryQty, "anything");
			Factory.Save();

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = "IFD";
			invoiceLine.JI_Procedure = cpcTwo.FullCodeCurrentPlusPreviousPlusConcession;
			invoiceLine.JI_Tariff = "1.2.345";
			invoiceLine.JI_CustomsSecondQuantity = 0;
			invoiceLine.JI_CustomsSecondUnitQty = "X";
			invoiceLine.Validation.ValidateAll();
			AssertHasMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, JobComInvoiceLineValidation.SupplementaryQtyShouldNotBeEmptyWhenWeHaveUnitsWarning);

			invoiceLine.JI_Procedure = cpcOne.FullCodeCurrentPlusPreviousPlusConcession;
			invoiceLine.Validation.ValidateAll();
			AssertNoMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, JobComInvoiceLineValidation.SupplementaryQtyShouldNotBeEmptyWhenWeHaveUnitsWarning);

			invoiceLine.JI_Procedure = cpcTwo.FullCodeCurrentPlusPreviousPlusConcession;
			invoiceLine.JI_CustomsSecondQuantity = 69;
			invoiceLine.Validation.ValidateAll();
			AssertNoMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, JobComInvoiceLineValidation.SupplementaryQtyShouldNotBeEmptyWhenWeHaveUnitsWarning);

			invoiceLine.JI_CustomsSecondQuantity = 0;
			AssertHasMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, JobComInvoiceLineValidation.SupplementaryQtyShouldNotBeEmptyWhenWeHaveUnitsWarning);
			invoiceLine.JI_Procedure = cpcOne.FullCodeCurrentPlusPreviousPlusConcession;
			AssertNoMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, JobComInvoiceLineValidation.SupplementaryQtyShouldNotBeEmptyWhenWeHaveUnitsWarning); // Changing CPC re-validates SuppQty field

			invoiceLine.JI_Procedure = cpcTwo.FullCodeCurrentPlusPreviousPlusConcession;
			invoiceLine.JI_CustomsSecondUnitQty = QuantityUnitList.WeightAcesulfamePotatssium119;
			invoiceLine.JI_CustomsSecondQuantity = 0;
			AssertNoMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, JobComInvoiceLineValidation.SupplementaryQtyShouldNotBeEmptyWhenWeHaveUnitsWarning);
		}

		public void TestSupplementaryUnitsRuleC0936()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cpcOne = helper.CreateOrFindExistingRefCusProcedure(CountryCodes.Latvia, "00", "12", "34", "567", "Test - normal, demands supp", "IMP", "IFD");
			var cpcTwo = helper.CreateOrFindExistingRefCusProcedure(CountryCodes.Latvia, "00", "12", "34", "F15", "Test - normal, demands supp", "IMP", "IFD");
			Factory.Save();

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_DeclarantType = "IFD";
			invoiceLine.JI_Procedure = cpcOne.FullCodeCurrentPlusPreviousPlusConcession;
			invoiceLine.JI_Tariff = "1.2.345";
			invoiceLine.JI_CustomsSecondQuantity = 0;
			invoiceLine.JI_CustomsSecondUnitQty = "X";
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
			invoiceLine.Validation.ValidateAll();
			AssertHasMessageError("Entry instruction substyle is F but declaration is not UCC6 => error message", invoiceLine.JI_CustomsSecondQuantityInfo, JobComInvoiceLineValidation.SupplementaryQtyShouldNotBeEmptyWhenWeHaveUnitsWarning);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
			invoiceLine.JI_Procedure = cpcTwo.FullCodeCurrentPlusPreviousPlusConcession;
			AssertHasMessageError("Concession is F15  but declaration is not UCC6 => error message", invoiceLine.JI_CustomsSecondQuantityInfo, JobComInvoiceLineValidation.SupplementaryQtyShouldNotBeEmptyWhenWeHaveUnitsWarning);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
			invoiceLine.JI_CustomsSecondUnitQty = "X";
			invoiceLine.JI_CustomsSecondQuantity = 0;
			invoiceLine.Validation.ValidateAll();
			AssertHasMessageError("Entry instruction substyle is C but declaration is not UCC6 and is not Import => error message", invoiceLine.JI_CustomsSecondQuantityInfo, JobComInvoiceLineValidation.SupplementaryQtyShouldNotBeEmptyWhenWeHaveUnitsWarning);

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				invoiceLine.JI_CustomsSecondUnitQty = "X";
				invoiceLine.JI_CustomsSecondQuantity = 0;
				invoiceLine.Validation.ValidateAll();
				AssertHasMessageError("Entry instruction substyle is C and declaration is UCC6 but declaration is not Import => error message", invoiceLine.JI_CustomsSecondQuantityInfo, JobComInvoiceLineValidation.SupplementaryQtyShouldNotBeEmptyWhenWeHaveUnitsWarning);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				invoiceLine.Validation.ValidateAll();
				AssertNoMessageError("Entry instruction substyle is C and declaration is UCC6 and Import => no error message", invoiceLine.JI_CustomsSecondQuantityInfo, JobComInvoiceLineValidation.SupplementaryQtyShouldNotBeEmptyWhenWeHaveUnitsWarning);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
				AssertNoMessageError("Entry instruction substyle is F and declaration is UCC6 and Import => no error message", invoiceLine.JI_CustomsSecondQuantityInfo, JobComInvoiceLineValidation.SupplementaryQtyShouldNotBeEmptyWhenWeHaveUnitsWarning);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
				invoiceLine.JI_Procedure = cpcTwo.FullCodeCurrentPlusPreviousPlusConcession;
				AssertNoMessageError("Entry instruction substyle is D and declaration is UCC6 and Import and Concession is F15 => no error message", invoiceLine.JI_CustomsSecondQuantityInfo, JobComInvoiceLineValidation.SupplementaryQtyShouldNotBeEmptyWhenWeHaveUnitsWarning);

				invoiceLine.JI_Procedure = cpcOne.FullCodeCurrentPlusPreviousPlusConcession;
				AssertHasMessageError("Declaration is UCC6 and Import but Concession is not F15 and Entry instruction substyle is not in F/C => error message", invoiceLine.JI_CustomsSecondQuantityInfo, JobComInvoiceLineValidation.SupplementaryQtyShouldNotBeEmptyWhenWeHaveUnitsWarning);
			}
		}

		public void TestCheckQuantityLengthValidation()
		{
			AssertCheckQuantityLengthValidation(invoiceLine.JI_CustomsQuantityInfo);
			AssertCheckQuantityLengthValidation(invoiceLine.JI_CustomsSecondQuantityInfo);
			AssertCheckQuantityLengthValidation(invoiceLine.JI_CustomsThirdQuantityInfo);
			AssertCheckQuantityLengthValidation(invoiceLine.JI_CustomsFourthQuantityInfo);
			AssertCheckQuantityLengthValidation(invoiceLine.JI_InvoiceQuantityInfo);

			void AssertCheckQuantityLengthValidation(ZPropertyInfo targetInfo)
			{
				const string expectedErrorMessage = "is too large, the maximum number of digits allowed for";
				CombineAssertions($"For Field : {targetInfo.HumanReadableName}", () =>
				{
					targetInfo.Value = new ZDecimal(1234567890123.1234m);
					AssertHasMessageErrorContaining("Large", targetInfo, expectedErrorMessage);

					targetInfo.Value = new ZDecimal(12345678901.123000m);
					AssertNoMessageErrorContaining("Acceptable", targetInfo, expectedErrorMessage);
				});
			}
		}

		public void TestCheckJI_WeightUQ()
		{
			invoiceLine.JI_WeightUQ = "";
			invoiceLine.Validation.ValidateJI_WeightUQ();
			AssertNoMessageErrorContaining(invoiceLine.JI_WeightUQInfo, "UQ");
			AssertNoMessageErrorContaining(invoiceLine.JI_WeightUQInfo, "Effective gross mass");
			invoiceLine.JI_Weight = 10m;
			invoiceLine.Validation.ValidateJI_WeightUQ();
			AssertHasMessageErrorContaining(invoiceLine.JI_WeightUQInfo, "UQ");
			AssertNoMessageErrorContaining(invoiceLine.JI_WeightUQInfo, "Effective gross mass");
			declaration.WarehouseDocAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			var entryHeader = Factory.New<CusEntryHeader>();
			declaration.CustomsEntryHeaders.Add(entryHeader);
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			AssertEquals("Pre Req", true, entryLine.EffectiveGrossWeightIsApplicable);
			invoiceLine.JI_Weight = 11m;
			invoiceLine.Validation.ValidateJI_WeightUQ();
			AssertHasMessageErrorContaining(invoiceLine.JI_WeightUQInfo, "UQ");
			AssertHasMessageErrorContaining(invoiceLine.JI_WeightUQInfo, "Effective gross mass");
			invoiceLine.JI_WeightUQ = "KG";
			AssertNoMessageErrorContaining(invoiceLine.JI_WeightUQInfo, "UQ");
			AssertNoMessageErrorContaining(invoiceLine.JI_WeightUQInfo, "Effective gross mass");
		}

		public void TestCheckJI_Weight_R0222()
		{
			var package = declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package.CW_PackQty = 3;
			package.CW_PackType = "PK";
			var package1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			package1.IsLinked = true;
			package1.PackQty = 0;
			using var context = new InvoiceLineValidationDeciderTestContext<IInvoiceLineValidationDecider>(declaration);

			var messageError = "[R0222] If package quantity = 0 then Gross Weight must be 0 as well.";
			CombineAssertions(() =>
			{
				context.EnableRule(r => r.IsRuleR0222Active);
				invoiceLine.JI_Weight = 4.3;
				AssertHasMessageError("R0222 rule is active, pack quantity is 0, GrossWeight isn't 0", invoiceLine.JI_WeightInfo, messageError);

				invoiceLine.JI_Weight = 0;
				AssertNoMessageError("R0222 rule is active, pack quantity is 0, GrossWeight is 0", invoiceLine.JI_WeightInfo, messageError);

				package.CW_PackType = "VG";
				invoiceLine.JI_Weight = 4.3;
				AssertNoMessageError("R0222 rule is active, pack quantity is 0, GrossWeight isn't 0, PackType is VG", invoiceLine.JI_WeightInfo, messageError);

				invoiceLine.JI_Weight = 0;
				AssertNoMessageError("R0222 rule is active, pack quantity is 0, GrossWeight is 0, PackType is VG", invoiceLine.JI_WeightInfo, messageError);

				context.DisableRule(r => r.IsRuleR0222Active);
				package.CW_PackType = "PK";
				invoiceLine.JI_Weight = 4.3;
				AssertNoMessageError("R0222 rule is inactive, pack quantity is 0, GrossWeight isn't 0", invoiceLine.JI_WeightInfo, messageError);
			});
		}

		public void TestCheckJI_WeightR0223()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			using var context = new InvoiceLineValidationDeciderTestContext<IInvoiceLineValidationDecider>(declaration);
			CombineAssertions(() =>
			{
				context.EnableRule(r => r.IsRuleR0223Active);
				invoiceLine.JI_NetWeight = 1;
				invoiceLine.JI_Weight = 0;
				AssertNoMessageError("Gross weight is 0", invoiceLine.JI_WeightInfo, "[R0223] Gross weight must be greater than or equal to net weight");

				invoiceLine.JI_NetWeight = 1;
				invoiceLine.JI_Weight = 1;
				AssertNoMessageError("Gross weight and net weight are equal", invoiceLine.JI_WeightInfo, "[R0223] Gross weight must be greater than or equal to net weight");

				invoiceLine.JI_NetWeight = 2;
				invoiceLine.JI_Weight = 1;
				AssertHasMessageError("Gross weight is less than net weight", invoiceLine.JI_WeightInfo, "[R0223] Gross weight must be greater than or equal to net weight");

				invoiceLine.JI_NetWeight = 1;
				invoiceLine.JI_WeightUQ = "G";
				invoiceLine.JI_Weight = 1;
				AssertHasMessageError("Gross weight has smaller units than net weight", invoiceLine.JI_WeightInfo, "[R0223] Gross weight must be greater than or equal to net weight");

				context.DisableRule(r => r.IsRuleR0223Active);
				invoiceLine.JI_NetWeight = 2;
				invoiceLine.JI_Weight = 1;
				AssertNoMessageError("Gross weight is less than net weight", invoiceLine.JI_WeightInfo, "[R0223] Gross weight must be greater than or equal to net weight");

				invoiceLine.JI_NetWeight = 1;
				invoiceLine.JI_WeightUQ = "G";
				invoiceLine.JI_Weight = 1;
				AssertNoMessageError("Gross weight has smaller units than net weight", invoiceLine.JI_WeightInfo, "[R0223] Gross weight must be greater than or equal to net weight");
			});
		}

		public void TestCheckJI_WeightR0224()
		{
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			using var context = new InvoiceLineValidationDeciderTestContext<IInvoiceLineValidationDecider>(declaration);
			CombineAssertions(() =>
			{
				context.EnableRule(r => r.IsRuleR0224Active);
				invoiceLine1.JI_NetWeight = 2;
				invoiceLine1.JI_Weight = 3;
				invoiceLine2.JI_NetWeight = 4;
				invoiceLine2.JI_Weight = 4;
				AssertNoMessageError("Total gross weight is greater than total net weight", invoiceLine1.JI_WeightInfo, "[R0224] Sum of gross weight must be greater than or equal to sum of net weight");
				AssertNoMessageError("Total gross weight is equal to total net weight", invoiceLine2.JI_WeightInfo, "[R0224] Sum of gross weight must be greater than or equal to sum of net weight");

				invoiceLine1.JI_NetWeight = 3;
				invoiceLine1.JI_Weight = 1;
				invoiceLine2.JI_NetWeight = 2;
				invoiceLine2.JI_Weight = 2;
				AssertHasMessageError("Total gross weight is less than total net weight", invoiceLine1.JI_WeightInfo, "[R0224] Sum of gross weight must be greater than or equal to sum of net weight");
				AssertNoMessageError("Total gross weight is equal to total net weight, even when across entry instructions sum of gross weight is less than sum of net weight", invoiceLine2.JI_WeightInfo, "[R0224] Sum of gross weight must be greater than or equal to sum of net weight");

				context.DisableRule(r => r.IsRuleR0224Active);
				invoiceLine1.JI_NetWeight = 3;
				invoiceLine1.JI_Weight = 1;
				invoiceLine2.JI_NetWeight = 2;
				invoiceLine2.JI_Weight = 2;
				AssertNoMessageError("Total gross weight is less than total net weight", invoiceLine1.JI_WeightInfo, "[R0224] Sum of gross weight must be greater than or equal to sum of net weight");
				AssertNoMessageError("Total gross weight is equal to total net weight, even when across entry instructions sum of gross weight is less than sum of net weight", invoiceLine2.JI_WeightInfo, "[R0224] Sum of gross weight must be greater than or equal to sum of net weight");
			});
		}

		public void TestCheckJI_ConcessionOrder_Length_Import()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				invoiceLine.JI_ConcessionOrder = "1";
				AssertHasMessageErrorContaining("JI_ConcessionOrder < 6 chars", invoiceLine.JI_ConcessionOrderInfo, JobComInvoiceLineValidation.QuotaNumberShouldBe6Characters);

				invoiceLine.JI_ConcessionOrder = "";
				AssertNoMessageErrorContaining("JI_ConcessionOrder empty", invoiceLine.JI_ConcessionOrderInfo, JobComInvoiceLineValidation.QuotaNumberShouldBe6Characters);

				invoiceLine.JI_ConcessionOrder = "1234567";
				AssertHasMessageErrorContaining("JI_ConcessionOrder > 6 chars", invoiceLine.JI_ConcessionOrderInfo, JobComInvoiceLineValidation.QuotaNumberShouldBe6Characters);

				invoiceLine.JI_ConcessionOrder = "123456";
				AssertNoMessageErrorContaining("JI_ConcessionOrder 6 chars", invoiceLine.JI_ConcessionOrderInfo, JobComInvoiceLineValidation.QuotaNumberShouldBe6Characters);

				invoiceLine.JI_ConcessionOrder = "12345A";
				AssertHasMessageErrorContaining("JI_ConcessionOrder 6 chars not numbers only", invoiceLine.JI_ConcessionOrderInfo, JobComInvoiceLineValidation.QuotaNumberShouldBe6Characters);
			});
		}

		public void TestCheckJI_ConcessionOrder_Length_Export()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			invoiceLine.JI_ConcessionOrder = "1";
			AssertNoNotifications("JI_ConcessionOrder < 6 chars, No notifications", invoiceLine.JI_ConcessionOrderInfo);
		}

		public void TestCheckJI_ConcessionOrder_TariffQuotaPrefCode_Import()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateJI_ConcessionOrder();
				AssertNoMessageErrorContaining("JI_PrimaryPreference empty, JI_ConcessionOrder not mandatory", invoiceLine.JI_ConcessionOrderInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_PrimaryPreference = "150";
				invoiceLine.Validation.ValidateJI_ConcessionOrder();
				AssertNoMessageErrorContaining("JI_PrimaryPreference 150, JI_ConcessionOrder not mandatory", invoiceLine.JI_ConcessionOrderInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_PrimaryPreference = "120";
				invoiceLine.Validation.ValidateJI_ConcessionOrder();
				AssertHasMessageErrorContaining("JI_PrimaryPreference 120, JI_ConcessionOrder mandatory", invoiceLine.JI_ConcessionOrderInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_ConcessionOrder = "123456";
				AssertNoMessageErrorContaining("Entered JI_ConcessionOrder", invoiceLine.JI_ConcessionOrderInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_ConcessionOrder_TariffQuotaPrefCode_Export()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			invoiceLine.JI_PrimaryPreference = "120";
			invoiceLine.Validation.ValidateJI_ConcessionOrder();
			AssertNoNotifications("JI_PrimaryPreference 120, JI_ConcessionOrder not mandatory", invoiceLine.JI_ConcessionOrderInfo);
		}

		public void TestCheckJI_TaxOrFeeDetail()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(CountryCodes.Latvia, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";

			var header = dec.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			line.JI_Procedure = "1111111";
			var taxOrFeeDetailEntity = new TaxOrFeeDetailEntity();
			taxOrFeeDetailEntity.VATCode = "RED";
			taxOrFeeDetailEntity.Description = "Reduced, V904, A505";
			taxOrFeeDetailEntity.AdditionalCode = "V904";
			taxOrFeeDetailEntity.Category = "A505";

			line.Lookups.TaxOrFeeDetailEntities.Add(taxOrFeeDetailEntity);
			line.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;

			procedure1.ZZ6_CalculateVAT = false;
			Factory.Save();
			line.Validation.ValidateJI_TaxOrFeeDetail();
			AssertHasWarning(line.JI_TaxOrFeeDetailInfo, "Procedure 1111111 indicates that VAT does not apply, but value RED in this field means that VAT is calculated by CW1; To disable the calculation set this field's value to blank.");

			procedure1.ZZ6_CalculateVAT = true;
			Factory.Save();
			line.Validation.ValidateJI_TaxOrFeeDetail();
			AssertNoWarning(line.JI_TaxOrFeeDetailInfo, "Procedure 1111111 indicates that VAT does not apply, but value RED in this field means that VAT is calculated by CW1; To disable the calculation set this field's value to blank.");
		}

		public void TestCheckJI_ZZF_NKTaxType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(CountryCodes.Latvia, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			procedure1.ZZ6_CalculateVAT = false;
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";

			var header = dec.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			line.JI_Procedure = "1111111";
			line.JI_ZZF_NKTaxType = "605";
			AssertHasWarning(line.JI_ZZF_NKTaxTypeInfo, "Procedure 1111111 indicates that VAT does not apply, and will not be calculated.");

			procedure1.ZZ6_CalculateVAT = true;
			Factory.Save();
			line.Validation.ValidateJI_ZZF_NKTaxType();
			AssertNoWarning(line.JI_ZZF_NKTaxTypeInfo, "Procedure 1111111 indicates that VAT does not apply, and will not be calculated.");
		}

		public void TestTariffInfoMessageError_NonUCCCompliant()
		{
			AssertEquals(false, declaration.IsUCCCompliant);

			var validation = new JobComInvoiceLineValidation(invoiceLine);
			validation.ValidateJI_Tariff();
			var msg = invoiceLine.GetMessageErrors().FirstOrDefault(x => x.Message.StartsWith("Message Error - Invoice Line: This line has no packaging details.")).Message;
			Assert(msg.Contains("Packages"));
		}

		public void TestCheckJI_Description()
		{
			invoiceLine.JI_Tariff = "";
			invoiceLine.JI_Description = "";
			AssertNoMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Tariff = "93011000";
			AssertHasMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Description = "ARTILLERY WEAPONS (FOR EXAMPLE, GUNS, HOWITZERS AND MORTARS)";
			AssertNoMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_LinePrice()
		{
			var message = "Price is required when Procedure is H1, H3 , H4, H5 or I1.";
			declaration.JE_MessageType = "IMP";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					instruction.CEI_Style = "H1";
					invoiceLine.Validation.ValidateJI_LinePrice();
					AssertHasMessageErrorContaining("IMP UCC6 declaration, JI_LinePrice empty, instruction's style is H1", invoiceLine.JI_LinePriceInfo, message);

					instruction.CEI_Style = "B1";
					invoiceLine.Validation.ValidateJI_LinePrice();
					AssertNoMessageErrorContaining("IMP UCC6 declaration, JI_LinePrice empty, instruction's style is B1", invoiceLine.JI_LinePriceInfo, message);

					instruction.CEI_Style = "H1";
					invoiceLine.JI_LinePrice = 1;
					AssertNoMessageErrorContaining("IMP UCC6 declaration, JI_LinePrice not empty, instruction's style is H1", invoiceLine.JI_LinePriceInfo, message);

					var dec = Factory.New<JobDeclaration>();
					dec.JE_MessageType = "EXP";

					var header = dec.Invoices.AddNew();
					var line = header.InvoiceLines.AddNew();
					instruction = dec.CustomsEntryInstructions.AddNew();
					line.JI_CEI = instruction.PK;
					instruction.CEI_Style = "H1";
					line.Validation.ValidateJI_LinePrice();
					AssertNoMessageErrorContaining("EXP UCC6 declaration, JI_LinePrice empty, instruction's style is H1", invoiceLine.JI_LinePriceInfo, message);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					instruction.CEI_Style = "H1";
					invoiceLine.Validation.ValidateJI_LinePrice();
					AssertNoMessageErrorContaining("IMP declaration but not UCC6, JI_LinePrice empty, instruction's style is H1", invoiceLine.JI_LinePriceInfo, message);
				}
			});
		}

		public void TestValidationDecider()
		{
			declaration.JE_MessageType = "IMP";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					AssertType<UCC6ImportInvoiceLineValidationDecider>("IMP UCC6 declaration", invoiceLine.Validation.ValidationDecider);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					AssertNull("IMP declaration but not UCC6", invoiceLine.Validation.ValidationDecider);
				}
			});
		}

		public void TestCheckMissingPreviousDocuments()
		{
			var invoiceLine = Factory.New<JobComInvLineForTest>();
			invoiceLine.JI_JZ = invoiceHeader.PK;

			void AssertCheckMissingPreviousDocuments(string previousDocumentParentObjectDescription, Action addPreviousDocumentAction)
			{
				CombineAssertions(() =>
				{
					invoiceLine.JI_LineNo = 1;
					AssertHasWarningContaining($"No Previous Document on {previousDocumentParentObjectDescription}", invoiceLine.JI_LineNoInfo, LinePreviousDocumentsError);

					using (invoiceLine.SuspendCheckMissingPreviousDocuments())
					{
						invoiceLine.Validation.ValidateJI_LineNo();
						AssertNoWarningContaining($"Has Previous Document on {previousDocumentParentObjectDescription}", invoiceLine.JI_LineNoInfo, LinePreviousDocumentsError);
					}

					addPreviousDocumentAction();
					invoiceLine.Validation.ValidateJI_LineNo();
					AssertNoWarningContaining($"Has Previous Document on {previousDocumentParentObjectDescription}", invoiceLine.JI_LineNoInfo, LinePreviousDocumentsError);
				});
			}

			AssertCheckMissingPreviousDocuments("Header", () => invoiceHeader.PreviousDocuments.AddNew());
			invoiceHeader.PreviousDocuments.RemoveAndDeleteAll();

			AssertCheckMissingPreviousDocuments("Declaration", () => declaration.PreviousDocuments.AddNew());
			declaration.PreviousDocuments.RemoveAndDeleteAll();

			AssertCheckMissingPreviousDocuments("Invoice Line", () => invoiceLine.PreviousDocuments.AddNew());
		}

		public void TestCheckMissingPreviousDocuments_MergedEntryLine()
		{
			CombineAssertions(() =>
			{
				var line1 = Factory.New<JobComInvLineForTest>();
				line1.JI_JZ = invoiceHeader.PK;
				line1.JI_LineNo = 1;
				line1.JI_Tariff = "1.2.345";
				var line2 = Factory.New<JobComInvLineForTest>();
				line2.JI_JZ = invoiceHeader.PK;
				line2.JI_LineNo = 2;
				line2.JI_Tariff = "1.2.345";

				invoiceHeader.InvoiceLines.Reload(true);
				var shutup = new SendsMessagesToCustomsShutterUpperer(false);
				declaration.DoMerge(shutup);
				line1.Validation.ValidateJI_LineNo();
				line2.Validation.ValidateJI_LineNo();
				AssertHasWarningContaining(line1.JI_LineNoInfo, MergedLinePreviousDocumentsError);
				AssertHasWarningContaining(line2.JI_LineNoInfo, MergedLinePreviousDocumentsError);

				using (line1.SuspendCheckMissingPreviousDocuments())
				{
					using (line2.SuspendCheckMissingPreviousDocuments())
					{
						line1.Validation.ValidateJI_LineNo();
						line2.Validation.ValidateJI_LineNo();
						AssertNoWarningContaining(line1.JI_LineNoInfo, MergedLinePreviousDocumentsError);
						AssertNoWarningContaining(line2.JI_LineNoInfo, MergedLinePreviousDocumentsError);
					}
				}

				invoiceHeader.PreviousDocuments.AddNew();
				line1.Validation.ValidateJI_LineNo();
				line2.Validation.ValidateJI_LineNo();
				AssertNoWarningContaining(line1.JI_LineNoInfo, MergedLinePreviousDocumentsError);
				AssertNoWarningContaining(line2.JI_LineNoInfo, MergedLinePreviousDocumentsError);
			});
		}

		public void TestCheckMissingPreviousDocuments_WithCEIStyle()
		{
			var invoiceLine = Factory.New<JobComInvLineForTest>();
			invoiceLine.JI_JZ = invoiceHeader.PK;
			invoiceHeader.InvoiceLines.Reload(true);

			CombineAssertions(() =>
			{
				var customsEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine.JI_Tariff = "1.2.345";
				var shutup = new SendsMessagesToCustomsShutterUpperer(false);
				declaration.DoMerge(shutup);

				customsEntryInstruction.CEI_Style = "XXX";
				var validation = new JobComInvLineValidationForTest(invoiceLine);
				validation.ValidateJI_LineNo();
				AssertHasWarningContaining(invoiceLine.JI_LineNoInfo, MergedLinePreviousDocumentsError);

				using (invoiceLine.SuspendCheckMissingPreviousDocuments())
				{
					validation.ValidateJI_LineNo();
					AssertNoWarningContaining(invoiceLine.JI_LineNoInfo, MergedLinePreviousDocumentsError);
				}

				customsEntryInstruction.CEI_Style = "LIN";
				validation.ValidateJI_LineNo();
				AssertNoWarningContaining(invoiceLine.JI_LineNoInfo, MergedLinePreviousDocumentsError);
			});
		}

		public void TestCheckJI_CountryOfOrigin_MandatoryValidation()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				invoiceLine.JI_CountryOfOrigin = CountryCodes.Germany;
				AssertNoMessageErrorContaining("Country Of Origin is mandatory for non UCC6 declaration.", invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_CountryOfOrigin = ZString.Empty;
				AssertHasMessageErrorContaining("No message error is expected when CountryOfOrigin has value.", invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			}
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				invoiceLine.JI_CountryOfOrigin = CountryCodes.Germany;
				AssertNoMessageErrorContaining("No message error is expected when CountryOfOrigin has value.", invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_CountryOfOrigin = ZString.Empty;
				AssertNoMessageErrorContaining("Country Of Origin is not strictly mandatory for UCC6 declaration (see rule C0170).", invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertHasMessageErrorContaining("Country Of Supply is mandatory for non UCC6 import declaration.", invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestCheckJI_CustomsSecondUnitQty_List()
		{
			SetupCustomsUQReferenceData();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CustomsSecondUnitQtyInfo, "~~", "ABC");
		}

		public void TestCheckJI_CustomsSecondUnitQty_Mandatory()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			CombineAssertions(() =>
			{
				invoiceLine.JI_CustomsSecondQuantity = 1;
				invoiceLine.Validation.ValidateJI_CustomsSecondUnitQty();
				AssertHasMessageErrorContaining("Quantity with no UQ", invoiceLine.JI_CustomsSecondUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_CustomsSecondQuantity = 0;
				invoiceLine.Validation.ValidateJI_CustomsSecondUnitQty();
				AssertNoMessageErrorContaining("No Quantity", invoiceLine.JI_CustomsSecondUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_CustomsThirdUnitQty_List()
		{
			SetupCustomsUQReferenceData();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CustomsSecondUnitQtyInfo, "~~", "ABC");
		}

		public void TestCheckJI_CustomsThirdUnitQty_Mandatory()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			CombineAssertions(() =>
			{
				invoiceLine.JI_CustomsThirdQuantity = 1;
				invoiceLine.Validation.ValidateJI_CustomsThirdUnitQty();
				AssertHasMessageErrorContaining("Quantity with no UQ", invoiceLine.JI_CustomsThirdUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_CustomsThirdQuantity = 0;
				invoiceLine.Validation.ValidateJI_CustomsThirdUnitQty();
				AssertNoMessageErrorContaining("No Quantity", invoiceLine.JI_CustomsThirdUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_CustomsFourthUnitQty_List()
		{
			SetupCustomsUQReferenceData();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CustomsFourthUnitQtyInfo, "~~", "ABC");
		}

		public void TestCheckJI_CustomsFourthUnitQty_Mandatory()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			CombineAssertions(() =>
			{
				invoiceLine.JI_CustomsFourthQuantity = 0;
				invoiceLine.Validation.ValidateJI_CustomsFourthUnitQty();
				AssertNoMessageErrorContaining("No Quantity", invoiceLine.JI_CustomsFourthUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_CustomsFourthQuantity = 1;
				invoiceLine.Validation.ValidateJI_CustomsFourthUnitQty();
				AssertHasMessageErrorContaining("Quantity with no UQ", invoiceLine.JI_CustomsFourthUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_CustomsFourthUnitQty = "MTQ";
				invoiceLine.Validation.ValidateJI_CustomsFourthUnitQty();
				AssertNoMessageErrorContaining("Quantity with UQ", invoiceLine.JI_CustomsFourthUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_CustomsFifthUnitQty_List()
		{
			SetupCustomsUQReferenceData();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CustomsFifthUnitQtyInfo, "~~", "ABC");
		}

		public void TestCheckJI_CustomsFifthUnitQty_Mandatory()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			CombineAssertions(() =>
			{
				invoiceLine.JI_CustomsFifthQuantity = 0;
				invoiceLine.Validation.ValidateJI_CustomsFifthUnitQty();
				AssertNoMessageErrorContaining("No Quantity", invoiceLine.JI_CustomsFifthUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_CustomsFifthQuantity = 1;
				invoiceLine.Validation.ValidateJI_CustomsFifthUnitQty();
				AssertHasMessageErrorContaining("Quantity with no UQ", invoiceLine.JI_CustomsFifthUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_CustomsFifthUnitQty = "MTQ";
				invoiceLine.Validation.ValidateJI_CustomsFifthUnitQty();
				AssertNoMessageErrorContaining("Quantity with UQ", invoiceLine.JI_CustomsFifthUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestAddNotAllowCreateNewEntryLineError()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_BGMReference = "BGM001";
			AssertNoRowError(invoiceLine, "Entry Declared (BGM001) : Cannot add new Entry lines to a Declared or Canceled Entry. Adding a new Entry Instruction then performing 'Generate Entries (Merge)' could solve the problem.");

			invoiceLine.Validation.AddNotAllowCreateNewEntryLineError(entryHeader);
			AssertEquals(true, invoiceLine.ShouldKeepNotAllowCreateNewEntryLineError);
			AssertHasRowError(invoiceLine, "Entry Declared (BGM001) : Cannot add new Entry lines to a Declared or Canceled Entry. Adding a new Entry Instruction then performing 'Generate Entries (Merge)' could solve the problem.");
		}

		public void TestValidateNotAllowCreateNewEntryLine()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			invoiceLine.JI_CEI = instruction.PK;
			entryHeader.CH_BGMReference = "BGM001";
			invoiceLine.Validation.ValidateNotAllowCreateNewEntryLine();
			AssertNoRowError(invoiceLine, "Entry Declared (BGM001) : Cannot add new Entry lines to a Declared or Canceled Entry. Adding a new Entry Instruction then performing 'Generate Entries (Merge)' could solve the problem.");

			invoiceLine.ShouldKeepNotAllowCreateNewEntryLineError = true;
			invoiceLine.Validation.ValidateNotAllowCreateNewEntryLine();
			AssertHasRowError(invoiceLine, "Entry Declared (BGM001) : Cannot add new Entry lines to a Declared or Canceled Entry. Adding a new Entry Instruction then performing 'Generate Entries (Merge)' could solve the problem.");

			invoiceLine.Validation.ValidateAll();
			AssertHasRowError(invoiceLine, "Entry Declared (BGM001) : Cannot add new Entry lines to a Declared or Canceled Entry. Adding a new Entry Instruction then performing 'Generate Entries (Merge)' could solve the problem.");
		}

		public void TestCheckJI_BondedWhsQuantity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France");
			var procedure = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "12", "34", "567", "One", EU.Business.MessageTypeList.Codes.Import, intoWarehouse: true, outOfWarehouse: false);

			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				declaration.SetSupportsBondedWarehousingForTesting(true);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();

				AssertEquals(false, invoiceLine.IsBondedWhsQuantityVisible);
				ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_BondedWhsQuantityInfo);
				AssertNoMessageErrorContaining(invoiceLine.JI_BondedWhsQuantityInfo, MandatoryValidation.ValueCannotBeNegative);

				invoiceLine.JI_Procedure = "1234567";
				AssertEquals(true, invoiceLine.IsBondedWhsQuantityVisible);
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_BondedWhsQuantityInfo);
				ValidationTestHelper.AssertValueCannotBeNegativeMessageError(invoiceLine.JI_BondedWhsQuantityInfo);

				invoiceLine.ComponentInventoryCollection.AddNew();
				ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_BondedWhsQuantityInfo);
			}
		}

		public void TestCheckJI_BondedWhsUnitQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France");
			var procedure = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "12", "34", "567", "One", EU.Business.MessageTypeList.Codes.Import, intoWarehouse: true, outOfWarehouse: false);
			Factory.Save();

			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				declaration.SetSupportsBondedWarehousingForTesting(true);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();

				AssertEquals(false, invoiceLine.IsBondedWhsQuantityVisible);
				ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_BondedWhsUnitQtyInfo);

				invoiceLine.JI_Procedure = "1234567";
				AssertEquals(true, invoiceLine.IsBondedWhsQuantityVisible);
				ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_BondedWhsUnitQtyInfo, "^_^", "ABC");

				invoiceLine.ComponentInventoryCollection.AddNew();
				ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_BondedWhsUnitQtyInfo, "^_^", "ABC");
				ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_BondedWhsUnitQtyInfo);
			}
		}

		public void TestAdditionalProcedureCodesCheckFirst4Characters() =>
			AssertEquals(true, new JobComInvLineValidationForTest(Factory.New<JobComInvoiceLine>()).AdditionalProcedureCodesCheckFirst4Characters_Exposed);

		public void TestCheckJI_InvoiceQuantity()
		{
			var invoiceLineMock = Factory.NewMoq<JobComInvoiceLine>();
			invoiceLineMock.Protected().Setup<(ZBool, BasePackage)>("ShouldCreateCusPackagePivotFromInvoiceQuantityForSinglePackageTypeCore").Returns((ZBool.True, Factory.New<BasePackage>()));

			invoiceLineMock.Setup(m => m.JI_InvoiceQuantity).Returns(5);
			invoiceLineMock.Object.Validation.ValidateJI_InvoiceQuantity();
			AssertNoWarnings("Value is less than max int32", invoiceLineMock.Object.JI_InvoiceQuantityInfo);

			invoiceLineMock.Setup(m => m.JI_InvoiceQuantity).Returns((ZDecimal)int.MaxValue + 10.0m);
			invoiceLineMock.Object.Validation.ValidateJI_InvoiceQuantity();
			AssertHasWarning("Value is more than max int32", invoiceLineMock.Object.JI_InvoiceQuantityInfo, "No package pivot could be created automatically because this value is too large.");

			invoiceLineMock.Setup(m => m.JI_InvoiceQuantity).Returns(5);
			invoiceLineMock.Object.Validation.ValidateJI_InvoiceQuantity();
			AssertNoWarnings("Value is less than max int32", invoiceLineMock.Object.JI_InvoiceQuantityInfo);

			invoiceLineMock.Protected().Setup<(ZBool, BasePackage)>("ShouldCreateCusPackagePivotFromInvoiceQuantityForSinglePackageTypeCore").Returns((ZBool.False, Factory.New<BasePackage>()));
			invoiceLineMock.Setup(m => m.JI_InvoiceQuantity).Returns((ZDecimal)int.MaxValue + 10.0m);
			invoiceLineMock.Object.Validation.ValidateJI_InvoiceQuantity();
			AssertNoWarnings("Should not create package pivot so no validation required", invoiceLineMock.Object.JI_InvoiceQuantityInfo);
		}

		public void TestCheckRuleC0002_Exporter()
		{
			var messageForInvoiceLine = "[C0002] In case data entered in Invoice line level, all related lines must have a value in this field.";

			var exporterForInvoice = Factory.New<OrgHeader>();
			var exporterForInvoiceLine = Factory.New<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();

			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();

			var invoiceheader1 = declaration.Invoices.AddNew();
			var invoiceheader2 = declaration.Invoices.AddNew();

			var invoiceLine1_1 = invoiceheader1.InvoiceLines.AddNew();
			var invoiceLine1_2 = invoiceheader1.InvoiceLines.AddNew();

			var invoiceLine2_1 = invoiceheader2.InvoiceLines.AddNew();
			var invoiceLine2_2 = invoiceheader2.InvoiceLines.AddNew();

			invoiceLine1_1.JI_CEI = instruction1.PK;
			invoiceLine1_2.JI_CEI = instruction2.PK;
			invoiceLine2_1.JI_CEI = instruction2.PK;
			invoiceLine2_2.JI_CEI = instruction1.PK;

			using (var context = new InvoiceLineValidationDeciderTestContext<IImportInvoiceLineValidationDecider>(declaration))
			{
				context.DisableRule(r => r.IsRuleC0002Active);

				invoiceheader1.JZ_OA_SupplierAddress = exporterForInvoice.MainAddress.PK;
				invoiceLine1_1.JI_OA_ExporterAddress = exporterForInvoiceLine.MainAddress.PK;
				ValidateAllRuleC0002(invoiceheader1, invoiceheader2);

				CombineAssertions("Rule C0002 is Disabled : invoiceLine1_1 has value for Exporter", () =>
				{
					AssertNoMessageErrorContaining("InvoiceLine 1 of Instruction 1", invoiceLine1_1.JI_OA_ExporterAddressInfo, messageForInvoiceLine);
					AssertNoMessageErrorContaining("InvoiceLine 2 of Instruction 1", invoiceLine2_2.JI_OA_ExporterAddressInfo, messageForInvoiceLine);

					AssertNoMessageErrorContaining("InvoiceLine 1 of Instruction 2", invoiceLine2_1.JI_OA_ExporterAddressInfo, messageForInvoiceLine);
					AssertNoMessageErrorContaining("InvoiceLine 2 of Instruction 2", invoiceLine1_2.JI_OA_ExporterAddressInfo, messageForInvoiceLine);
				});

				invoiceheader1.JZ_OA_SupplierAddress = ZGuid.Empty;
				invoiceLine1_1.JI_OA_ExporterAddress = ZGuid.Empty;

				context.EnableRule(r => r.IsRuleC0002Active);
				invoiceheader1.JZ_OA_SupplierAddress = exporterForInvoice.MainAddress.PK;
				invoiceLine1_1.JI_OA_ExporterAddress = exporterForInvoiceLine.MainAddress.PK;
				ValidateAllRuleC0002(invoiceheader1, invoiceheader2);
				CombineAssertions("Rule C0002 is Active : invoiceLine1_1 has value for Exporter", () =>
				{
					AssertNoMessageErrorContaining("InvoiceLine 1 of Instruction 1", invoiceLine1_1.JI_OA_ExporterAddressInfo, messageForInvoiceLine);
					AssertHasMessageErrorContaining("InvoiceLine 2 of Instruction 1", invoiceLine2_2.JI_OA_ExporterAddressInfo, messageForInvoiceLine);

					AssertNoMessageErrorContaining("InvoiceLine 1 of Instruction 2", invoiceLine2_1.JI_OA_ExporterAddressInfo, messageForInvoiceLine);
					AssertNoMessageErrorContaining("InvoiceLine 2 of Instruction 2", invoiceLine1_2.JI_OA_ExporterAddressInfo, messageForInvoiceLine);
				});

				context.DisableRule(r => r.IsRuleC0002Active);
				ValidateAllRuleC0002(invoiceheader1, invoiceheader2);
				CombineAssertions("Rule C0002 is Disable : invoiceLine1_1 has value for Exporter", () =>
				{
					AssertNoMessageErrorContaining("InvoiceLine 2 of Instruction 1", invoiceLine2_2.JI_OA_ExporterAddressInfo, messageForInvoiceLine);
				});

				context.EnableRule(r => r.IsRuleC0002Active);
				invoiceLine1_2.JI_OA_ExporterAddress = exporterForInvoiceLine.MainAddress.PK;
				invoiceLine2_2.JI_OA_ExporterAddress = exporterForInvoiceLine.MainAddress.PK;
				ValidateAllRuleC0002(invoiceheader1, invoiceheader2);
				CombineAssertions("Rule C0002 is Active : invoiceLine1_1 and invoiceLine1_2 and invoiceLine2_2 have value for Exporter", () =>
				{
					AssertNoMessageErrorContaining("InvoiceLine 1 of Instruction 1", invoiceLine1_1.JI_OA_ExporterAddressInfo, messageForInvoiceLine);
					AssertNoMessageErrorContaining("InvoiceLine 2 of Instruction 1", invoiceLine2_2.JI_OA_ExporterAddressInfo, messageForInvoiceLine);

					AssertHasMessageErrorContaining("InvoiceLine 1 of Instruction 2", invoiceLine2_1.JI_OA_ExporterAddressInfo, messageForInvoiceLine);
					AssertNoMessageErrorContaining("InvoiceLine 2 of Instruction 2", invoiceLine1_2.JI_OA_ExporterAddressInfo, messageForInvoiceLine);
				});

				context.DisableRule(r => r.IsRuleC0002Active);
				ValidateAllRuleC0002(invoiceheader1, invoiceheader2);
				CombineAssertions("Rule C0002 is Disable : invoiceLine1_1 and invoiceLine1_2 and invoiceLine2_2 have value for Exporter", () =>
				{
					AssertNoMessageErrorContaining("InvoiceLine 1 of Instruction 2", invoiceLine2_1.JI_OA_ExporterAddressInfo, messageForInvoiceLine);
				});

				context.EnableRule(r => r.IsRuleC0002Active);
				invoiceheader1.JZ_OA_SupplierAddress = ZGuid.Empty;
				invoiceLine1_2.JI_OA_ExporterAddress = exporterForInvoiceLine.MainAddress.PK;
				invoiceLine2_1.JI_OA_ExporterAddress = exporterForInvoiceLine.MainAddress.PK;
				ValidateAllRuleC0002(invoiceheader1, invoiceheader2);
				CombineAssertions("Rule C0002 is Active : all invoice lines have value for Exporter", () =>
				{
					AssertNoMessageErrorContaining("InvoiceLine 1 of Instruction 1", invoiceLine1_1.JI_OA_ExporterAddressInfo, messageForInvoiceLine);
					AssertNoMessageErrorContaining("InvoiceLine 2 of Instruction 1", invoiceLine2_2.JI_OA_ExporterAddressInfo, messageForInvoiceLine);

					AssertNoMessageErrorContaining("InvoiceLine 1 of Instruction 2", invoiceLine2_1.JI_OA_ExporterAddressInfo, messageForInvoiceLine);
					AssertNoMessageErrorContaining("InvoiceLine 2 of Instruction 2", invoiceLine1_2.JI_OA_ExporterAddressInfo, messageForInvoiceLine);
				});
			}
		}

		void ValidateAllRuleC0002(JobComInvoiceHeader invoice1, JobComInvoiceHeader invoice2)
		{
			invoice1.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Validation.ValidateAll());
			invoice2.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Validation.ValidateAll());
		}

		public void TestIsJIDescriptionMandatory() =>
			AssertEquals(true, new JobComInvLineValidationForTest(Factory.New<JobComInvoiceLine>()).IsJIDescriptionMandatory_Exposed);

		public void TestInventoryManagementSettingMatchesProcedure()
		{
			AssertEquals("InventoryManagementSettingMatchesProcedure should be always true in EU.", true, new JobComInvLineValidationForTest(Factory.New<JobComInvoiceLine>()).InventoryManagementSettingMatchesProcedureExposed);
		}

		public void TestCheckJI_Tariff_CheckClassConditions()
		{
			var countryCode = CountryCodes.Latvia;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsSecondQuantity = 2;
			invoiceLine.JI_CustomsSecondUnitQty = "NAR";

			var anotherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(anotherFactory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(countryCode, "TTT");
			var classType1 = helper.CreateOrGetExistingRefCusConditionType(countryCode, "CLASS", "TSTC1", "Test Class Condition Type 1");

			anotherFactory.Save();
			var tariff = helper.CreateTariff(countryCode, tariffType.PK, "Tariff1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var formulaValueType = helper.CreateOrGetExistingRefCusConditionValueType(countryCode, "FRMVT", afterCreate: t => t.ZX4_IsFormula = true);

			var testCondCtrl1 = helper.CreateOrGetExistingRefCusCondition(countryCode, classType1.PK, tariff.PK, "Comment", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(formulaValueType.PK, testCondCtrl1.PK, "[KGM]/[NAR] < 165.001 & [KGM]/[NAR] >= 60.000");

			anotherFactory.Save();
			tariff = Factory.Load<TariffView>(tariff.PK);
			var (notMetMessageForNonInformation, notMetMessageForInformation) = tariff.CheckConditionsAreMetForConditionClass(Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Class, invoiceLine.EvaluateConditionValue, null, invoiceLine.CalcDataForConditionFormula);
			AssertMultilineASCIIEquals("notMetMessageForNonInformation should not be empty", @"The Classification condition is not satisfied
    Test Class Condition Type 1:
        Comment: ([KGM]/[NAR] < 165.001 & [KGM]/[NAR] >= 60.000)
", notMetMessageForNonInformation);

			invoiceLine.JI_CustomsQuantity = 180m;
			(notMetMessageForNonInformation, notMetMessageForInformation) = tariff.CheckConditionsAreMetForConditionClass(Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Class, invoiceLine.EvaluateConditionValue, null, invoiceLine.CalcDataForConditionFormula);
			AssertMultilineASCIIEquals("notMetMessageForNonInformation should be empty", "", notMetMessageForNonInformation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceHeader invoiceHeader;

		const string LinePreviousDocumentsError = "This line has no previous documents. Without a previous document the entry may be rejected. Please enter one at this line or at its invoice or at declaration";
		const string MergedLinePreviousDocumentsError = "This invoice line’s merged entry line has no previous documents. Without a previous document the entry may be rejected. Add one to any of the entry line’s invoice line or at its invoice or at declaration";

		void SetupCustomsUQReferenceData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			Factory.Save();
		}

		class JobComInvLineValidationForTest : JobComInvoiceLineValidation
		{
			public JobComInvLineValidationForTest(JobComInvoiceLine invL) : base(invL)
			{
			}

			protected override IEnumerable<ZString> PreviousDocumentsCheckExceptedDeclarationTypes => new ZString[] { "LIN" };

			public ZBool AdditionalProcedureCodesCheckFirst4Characters_Exposed => AdditionalProcedureCodesCheckFirst4Characters;
			public ZBool IsJIDescriptionMandatory_Exposed => IsJIDescriptionMandatory;

			public ZBool InventoryManagementSettingMatchesProcedureExposed => InventoryManagementSettingMatchesProcedure;
		}
	}
}
