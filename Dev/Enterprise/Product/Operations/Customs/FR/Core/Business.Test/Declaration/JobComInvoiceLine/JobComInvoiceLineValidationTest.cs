using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Messaging.MessageBuilders;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public sealed class JobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParent()
		{
			JobComInvoiceLine parent = Factory.New<JobComInvoiceLine>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestCheckRuleNAT_235()
		{
			var messageError = "[NAT_235] Add. Information starting with 'K' does not allow supplementary quantities.";

			var declaration = Factory.New<JobDeclaration>();
			var additionalInfoInDeclarationLevel = declaration.AdditionalInfos.AddNew();

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var additionalInfoInEntryInstructionLevel = entryInstruction.AdditionalInfos.AddNew();

			var invoiceHeader = declaration.Invoices.AddNew();
			var additionalInfoInInvoiceHeaderLevel = invoiceHeader.AdditionalInfos.AddNew();

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var additionalInfoInInvoiceLineLevel = invoiceLine.AdditionalInfos.AddNew();

			using (var context = new ImportInvoiceLineValidationTestContext(invoiceLine))
			{
				context.EnableRule(x => x.IsRuleNAT_235Active);
				CombineAssertions("When RuleNAT_235 is enabled:", () =>
				{
					additionalInfoInDeclarationLevel.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.TariffBypassNeedingMotivation;
					invoiceLine.JI_CustomsSecondQuantity = 1;
					invoiceLine.JI_CustomsThirdQuantity = 1;
					AssertHasMessageError("When K0001 is entered at Declaration level and JI_CustomsSecondQuantity is not empty, there should be message error.", invoiceLine.JI_CustomsSecondQuantityInfo, messageError);
					AssertHasMessageError("When K0001 is entered at Declaration level and JI_CustomsThirdQuantity is not empty, there should be message error.", invoiceLine.JI_CustomsThirdQuantityInfo, messageError);
					invoiceLine.JI_CustomsSecondQuantity = 0;
					invoiceLine.JI_CustomsThirdQuantity = 0;
					AssertNoMessageError("When JI_CustomsSecondQuantity is empty, there should be no message error.", invoiceLine.JI_CustomsSecondQuantityInfo, messageError);
					AssertNoMessageError("When JI_CustomsThirdQuantity is empty, there should be no message error.", invoiceLine.JI_CustomsThirdQuantityInfo, messageError);

					additionalInfoInDeclarationLevel.CSI_Code = ZString.Empty;
					additionalInfoInEntryInstructionLevel.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.TariffBypassNeedingMotivation;
					invoiceLine.JI_CustomsSecondQuantity = 1;
					invoiceLine.JI_CustomsThirdQuantity = 1;
					AssertHasMessageError("When K0001 is entered at EntryInstruction level and JI_CustomsSecondQuantity is not empty, there should be message error.", invoiceLine.JI_CustomsSecondQuantityInfo, messageError);
					AssertHasMessageError("When K0001 is entered at EntryInstruction level and JI_CustomsThirdQuantity is not empty, there should be message error.", invoiceLine.JI_CustomsThirdQuantityInfo, messageError);
					invoiceLine.JI_CustomsSecondQuantity = 0;
					invoiceLine.JI_CustomsThirdQuantity = 0;
					AssertNoMessageError("When JI_CustomsSecondQuantity is empty, there should be no message error.", invoiceLine.JI_CustomsSecondQuantityInfo, messageError);
					AssertNoMessageError("When JI_CustomsThirdQuantity is empty, there should be no message error.", invoiceLine.JI_CustomsThirdQuantityInfo, messageError);

					additionalInfoInEntryInstructionLevel.CSI_Code = ZString.Empty;
					additionalInfoInInvoiceHeaderLevel.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.TariffBypassNeedingMotivation;
					invoiceLine.JI_CustomsSecondQuantity = 1;
					invoiceLine.JI_CustomsThirdQuantity = 1;
					AssertHasMessageError("When K0001 is entered at InvoiceHeader level and JI_CustomsSecondQuantity is not empty, there should be message error.", invoiceLine.JI_CustomsSecondQuantityInfo, messageError);
					AssertHasMessageError("When K0001 is entered at InvoiceHeader level and JI_CustomsThirdQuantity is not empty, there should be message error.", invoiceLine.JI_CustomsThirdQuantityInfo, messageError);
					invoiceLine.JI_CustomsSecondQuantity = 0;
					invoiceLine.JI_CustomsThirdQuantity = 0;
					AssertNoMessageError("When JI_CustomsSecondQuantity is empty, there should be no message error.", invoiceLine.JI_CustomsSecondQuantityInfo, messageError);
					AssertNoMessageError("When JI_CustomsThirdQuantity is empty, there should be no message error.", invoiceLine.JI_CustomsThirdQuantityInfo, messageError);

					additionalInfoInInvoiceHeaderLevel.CSI_Code = ZString.Empty;
					additionalInfoInInvoiceLineLevel.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.TariffBypassNeedingMotivation;
					invoiceLine.JI_CustomsSecondQuantity = 1;
					invoiceLine.JI_CustomsThirdQuantity = 1;
					AssertHasMessageError("When K0001 is entered at InvoiceLine level and JI_CustomsSecondQuantity is not empty, there should be message error.", invoiceLine.JI_CustomsSecondQuantityInfo, messageError);
					AssertHasMessageError("When K0001 is entered at InvoiceLine level and JI_CustomsThirdQuantity is not empty, there should be message error.", invoiceLine.JI_CustomsThirdQuantityInfo, messageError);
					invoiceLine.JI_CustomsSecondQuantity = 0;
					invoiceLine.JI_CustomsThirdQuantity = 0;
					AssertNoMessageError("When JI_CustomsSecondQuantity is empty, there should be no message error.", invoiceLine.JI_CustomsSecondQuantityInfo, messageError);
					AssertNoMessageError("When JI_CustomsThirdQuantity is empty, there should be no message error.", invoiceLine.JI_CustomsThirdQuantityInfo, messageError);
					additionalInfoInInvoiceLineLevel.CSI_Code = ZString.Empty;
				});

				context.DisableRule(x => x.IsRuleNAT_235Active);
				CombineAssertions("When RuleNAT_088Bis is disabled:", () =>
				{
					additionalInfoInDeclarationLevel.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.TariffBypassNeedingMotivation;
					invoiceLine.JI_CustomsSecondQuantity = 1;
					invoiceLine.JI_CustomsThirdQuantity = 1;
					AssertNoMessageError("When JI_CustomsSecondQuantity is not empty, there should be no message error.", invoiceLine.JI_CustomsSecondQuantityInfo, messageError);
					AssertNoMessageError("When JI_CustomsThirdQuantity is not empty, there should be no message error.", invoiceLine.JI_CustomsThirdQuantityInfo, messageError);

					additionalInfoInDeclarationLevel.CSI_Code = ZString.Empty;
					additionalInfoInEntryInstructionLevel.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.TariffBypassNeedingMotivation;
					invoiceLine.JI_CustomsSecondQuantity = 1;
					invoiceLine.JI_CustomsThirdQuantity = 1;
					AssertNoMessageError("When JI_CustomsSecondQuantity is not empty, there should be no message error.", invoiceLine.JI_CustomsSecondQuantityInfo, messageError);
					AssertNoMessageError("When JI_CustomsThirdQuantity is not empty, there should be no message error.", invoiceLine.JI_CustomsThirdQuantityInfo, messageError);

					additionalInfoInEntryInstructionLevel.CSI_Code = ZString.Empty;
					additionalInfoInInvoiceHeaderLevel.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.TariffBypassNeedingMotivation;
					invoiceLine.JI_CustomsSecondQuantity = 1;
					invoiceLine.JI_CustomsThirdQuantity = 1;
					AssertNoMessageError("When JI_CustomsSecondQuantity is not empty, there should be no message error.", invoiceLine.JI_CustomsSecondQuantityInfo, messageError);
					AssertNoMessageError("When JI_CustomsThirdQuantity is not empty, there should be no message error.", invoiceLine.JI_CustomsThirdQuantityInfo, messageError);

					additionalInfoInInvoiceHeaderLevel.CSI_Code = ZString.Empty;
					additionalInfoInInvoiceLineLevel.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.TariffBypassNeedingMotivation;
					invoiceLine.JI_CustomsSecondQuantity = 1;
					invoiceLine.JI_CustomsThirdQuantity = 1;
					AssertNoMessageError("When JI_CustomsSecondQuantity is not empty, there should be no message error.", invoiceLine.JI_CustomsSecondQuantityInfo, messageError);
					AssertNoMessageError("When JI_CustomsThirdQuantity is not empty, there should be no message error.", invoiceLine.JI_CustomsThirdQuantityInfo, messageError);
				});
			}
		}

		public void TestCheckJI_Tariff_AdditionalCodesListContainsNonVATCodes()
		{
			SetUpReferenceData();

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = "IMP";
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "123456789";

			invoiceLine2.JI_ZZF_NKTaxType = "IT3";
			invoiceLine2.JI_SupplementaryCode1 = "";
			invoiceLine2.Validation.ValidateJI_Tariff();
			AssertEquals("Additional code list is empty", 0, invoiceLine2.Lookups.AdditionalCodesList.Count);
			AssertNoMessageErrorContaining("Additional code list is empty", invoiceLine2.JI_TariffInfo, "Supplementary Codes exist for this tariff.");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_RegionOrTerritoryOfDestination = FRDomesticOverseasTerritories.Codes.CONTI;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RN_NKDefaultOrigin = "FR";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "99999999";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			var taxOrFeeDetailEntityList = invoiceLine.Lookups.TaxOrFeeDetailEntities;

			AssertContainsExactElementsInAnyOrder("CONTI region declaration should contain METRO codes", new ZString[] { "Q003", "Q004" }, invoiceLine.Lookups.AdditionalCodesList.GetAllCodes());

			invoiceLine.JI_SupplementaryCode1 = "";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("No VAT selected and additional code list contains METRO codes but SupplementaryCodes is empty", invoiceLine.JI_TariffInfo, "Supplementary Codes exist for this tariff.");

			invoiceLine.JI_ZZF_NKTaxType = ZString.Empty;
			invoiceLine.JI_SupplementaryCode1 = "Q003";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining("No VAT selected but additional code list contains METRO codes and SupplementaryCodes is not empty", invoiceLine.JI_TariffInfo, "Supplementary Codes exist for this tariff.");

			var taxOrFeeDetailEntity = taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT1");
			invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
			invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
			invoiceLine.JI_SupplementaryCode1 = "";
			invoiceLine.JI_SupplementaryCode2 = "";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("Selected VAT has additional code and additional code list contains METRO codes but SupplementaryCodes is empty", invoiceLine.JI_TariffInfo, "Supplementary Codes exist for this tariff.");

			invoiceLine.JI_SupplementaryCode1 = "Q004";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining("Selected VAT has additional code and additional code list contains METRO codes and SupplementaryCodes is not empty", invoiceLine.JI_TariffInfo, "Supplementary Codes exist for this tariff.");

			taxOrFeeDetailEntity = taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT2");
			invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
			invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
			invoiceLine.JI_SupplementaryCode1 = "";
			invoiceLine.JI_SupplementaryCode2 = "";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("Selected VAT has no additional code and additional code list contains METRO codes but SupplementaryCodes is empty", invoiceLine.JI_TariffInfo, "Supplementary Codes exist for this tariff.");

			invoiceLine.JI_SupplementaryCode1 = "Q004";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining("Selected VAT has no additional code and additional code list contains METRO codes and SupplementaryCodes is not empty", invoiceLine.JI_TariffInfo, "Supplementary Codes exist for this tariff.");

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

				var metroTradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.France, FRLocalGroups.Codes.METRO);
				var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.France, "DTY");
				var rateCode = helper.CreateCusRateCode(Factory, "A00", rateType.PK);
				var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode.PK, ZDateTime.BrettsBirthday, new ZDateTime(2079, 6, 6), "VDF * 10%");
				helper.CreateCusApplicability(rate1, asiaTradeGroup, ZDateTime.BrettsBirthday, new ZDateTime(2079, 6, 6), "Q003", secondTradeGroup: metroTradeGroup);
				helper.CreateCusApplicability(rate1, asiaTradeGroup, ZDateTime.BrettsBirthday, new ZDateTime(2079, 6, 6), "Q004", secondTradeGroup: metroTradeGroup);

				Factory.Save();
			}
		}

		public void TestCheckJI_Tariff_AdditionalCodesListIsFullOfVATCodes()
		{
			SetUpReferenceData();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "99999999";
			var taxOrFeeDetailEntityList = invoiceLine.Lookups.TaxOrFeeDetailEntities;

			invoiceLine.JI_ZZF_NKTaxType = "IT1";
			invoiceLine.JI_SupplementaryCode1 = "";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertContainsExactElementsInAnyOrder("AdditionalCodesList is full of VAT Codes", new ZString[] { "VAT1" }, invoiceLine.Lookups.AdditionalCodesList.GetAllCodes());
			AssertHasMessageErrorContaining("VAT selected but SupplementaryCodes is not empty", invoiceLine.JI_TariffInfo, "Supplementary Codes exist for this tariff.");

			invoiceLine.JI_ZZF_NKTaxType = ZString.Empty;
			invoiceLine.JI_SupplementaryCode1 = "";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining("No VAT selected and SupplementaryCodes is empty", invoiceLine.JI_TariffInfo, "Supplementary Codes exist for this tariff.");

			var taxOrFeeDetailEntity = taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT1");
			invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
			invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
			invoiceLine.JI_SupplementaryCode1 = "";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("Selected VAT has additional code but SupplementaryCodes is not empty", invoiceLine.JI_TariffInfo, "Supplementary Codes exist for this tariff.");

			invoiceLine.JI_SupplementaryCode1 = "Add1";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining("SupplementaryCodes is not empty and SupplementaryCodes is not empty", invoiceLine.JI_TariffInfo, "Supplementary Codes exist for this tariff.");

			invoiceLine.JI_SupplementaryCode1 = "";
			taxOrFeeDetailEntity = taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT2");
			invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
			invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
			invoiceLine.JI_SupplementaryCode1 = "";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining("Selected VAT has no additional code and SupplementaryCodes is not empty", invoiceLine.JI_TariffInfo, "Supplementary Codes exist for this tariff.");

			void SetUpReferenceData()
			{
				var startDate = ZDateTime.Today.AddDays(-2);
				var endDate = ZDateTime.Today.AddDays(2);
				var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var helper = new UniversalReferenceTestDataHelper(Factory);

				helper.CreateNewOrGetExistingDataGrouping(currentCountry).ZZZ_ZZZ_Grouping = helper.CreateNewOrGetExistingDataGrouping("EUN").PK;

				var impTariffTypePK = helper.CreateNewOrGetExistingTariffType("EUN", "IMP").PK;
				Factory.Save();
				var tariff = helper.CreateTariff(currentCountry, impTariffTypePK, "99999999", ZDateTime.BrettsBirthday,
					endDate, "Alpha Bravo");

				helper.CreateTaxOrFee("VT1", 0.02m, currentCountry, description: "VAT One");
				helper.CreateTaxOrFee("VT2", 0.01m, currentCountry, description: "VAT Two");

				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT1", startDate: startDate,
					endDate: endDate, additionalCode: "VAT1");
				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT2", startDate: startDate,
					endDate: endDate, additionalCode: "");

				helper.CreateCusCodeType("ADDIN", "Additional Code");
				helper.CreateCusCodeList(currentCountry, "ADDIN", "VAT1", "Test Additional Code 1",
					startDate: startDate, endDate: endDate);

				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "IT1", additionalCode: "VAT1", startDate, endDate);
				helper.CreateTaxOrFee("IT1", 0.02m, currentCountry);

				Factory.Save();
			}
		}

		public void TestCheckJI_CustomsThirdQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsSecondQuantity = 0;
			invoiceLine.JI_CustomsThirdQuantity = 0;
			AssertNoMessageError(invoiceLine.JI_CustomsThirdQuantityInfo, "A second QTY must be entered before entering a third QTY value.");

			invoiceLine.JI_CustomsThirdQuantity = 1;
			AssertHasMessageError(invoiceLine.JI_CustomsThirdQuantityInfo, "A second QTY must be entered before entering a third QTY value.");

			invoiceLine.JI_CustomsSecondQuantity = 1;
			invoiceLine.Validation.ValidateJI_CustomsThirdQuantity();
			AssertNoMessageError(invoiceLine.JI_CustomsThirdQuantityInfo, "A second QTY must be entered before entering a third QTY value.");
		}

		public void TestJI_Tariff()
		{
			var impError = "Tariff should contain 10 numbers";
			var expError = "Tariff should contain at least 8 numbers";

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			invoiceLine.JI_FormattedTariff = "1234.5678.0";
			AssertHasMessageError(invoiceLine.JI_FormattedTariffInfo, impError);
			invoiceLine.JI_FormattedTariff = "1234.5678.00";
			AssertNoMessageError(invoiceLine.JI_FormattedTariffInfo, impError);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			invoiceLine.JI_FormattedTariff = "1234.567";
			AssertHasMessageError(invoiceLine.JI_FormattedTariffInfo, expError);
			invoiceLine.JI_FormattedTariff = "1234.5678";
			AssertNoMessageError(invoiceLine.JI_FormattedTariffInfo, expError);
			invoiceLine.JI_FormattedTariff = "1234.5678.00";
			AssertNoMessageError(invoiceLine.JI_FormattedTariffInfo, expError);
		}

		public void TestCheckJI_TariffIfNoApplicableRates()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var frenchCountryCode = CountryCodes.France;
			var testHelper = new UniversalReferenceTestDataHelper(Factory);

			var tradeGroupStandard = testHelper.CreateTradeGroup(frenchCountryCode, "STANDARD", date1, date4);
			testHelper.AddCountry(tradeGroupStandard, CountryCodes.Australia, date1, date4);

			var tariffType = testHelper.CreateNewOrGetExistingTariffType(frenchCountryCode, Universal.Constants.TariffTypes.Import);
			Factory.Save();
			var cusTariff = testHelper.CreateTariff(frenchCountryCode, tariffType.PK, "1234567890", date1, date4, "dummy Description 0");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_CountryOfOrigin = CountryCodes.Australia;
			var effectiveDate = invoiceLine.EffectiveAssessmentDate;

			invoiceLine.JI_PrimaryPreference = "ZRO";
			AssertNoMessageErrorContaining("no NationalRateSelectionCriteria", invoiceLine.JI_TariffInfo, $"There is no applicable Miscellaneous rate for the Tariff '1234567890' and Country Of Origin 'AU' as at {effectiveDate} in combination with other data entered on the form.");

			var mscRateType = testHelper.CreateNewOrGetExistingRateType(frenchCountryCode, "MSC", "Miscellaneous");
			var rateCode11 = testHelper.LoadOrCreateNewCusRateCode(Factory, "R732", mscRateType.PK);
			var rateCode12 = testHelper.LoadOrCreateNewCusRateCode(Factory, "V360", mscRateType.PK);
			var preferenceSTD = testHelper.CreatePreferenceForCountry("STD", "Standard", frenchCountryCode);
			var preferenceTWO = testHelper.CreatePreferenceForCountry("TWO", "TWO Matched", frenchCountryCode);
			var preferenceZRO = testHelper.CreatePreferenceForCountry("ZRO", "ZERO Matched", frenchCountryCode);

			var testRate1 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date4, "0", preferencePk: preferenceSTD.PK, dataGrouping: frenchCountryCode);
			testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4);
			var testRate2 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date4, "0", preferencePk: preferenceTWO.PK, dataGrouping: frenchCountryCode);
			testHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4);
			var testRate3 = testHelper.CreateRate(cusTariff, rateCode11.PK, date1, date4, "0", preferencePk: preferenceTWO.PK, dataGrouping: frenchCountryCode);
			testHelper.CreateCusApplicability(testRate3, tradeGroupStandard, date1, date4);
			var testRate4 = testHelper.CreateRate(cusTariff, rateCode12.PK, date1, date4, "0", preferencePk: preferenceZRO.PK, dataGrouping: frenchCountryCode);
			testHelper.CreateCusApplicability(testRate4, tradeGroupStandard, date1, date4, "", "ORD11");
			Factory.Save();

			var declaration2 = Factory.New<JobDeclaration>();
			var invoiceHeader2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			declaration2.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLine2.JI_Tariff = "1234567890";
			invoiceLine2.JI_CountryOfOrigin = CountryCodes.Australia;
			var effectiveDate2 = invoiceLine2.EffectiveAssessmentDate;

			invoiceLine2.JI_PrimaryPreference = "ZRO";
			AssertHasMessageErrorContaining("has NationalRateSelectionCriteria", invoiceLine2.JI_TariffInfo, $"There is no applicable Miscellaneous rate for the Tariff '1234567890' and Country Of Origin 'AU' as at {effectiveDate2} in combination with other data entered on the form.");
		}

		public void TestJI_CEI()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_SubStyle = "A";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_CEI = cei.PK;
			AssertNoMessageErrorContaining(invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestJI_Procedure()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "IM", "40", "00", "000", "One", "IMP", group: "40");
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, "IM", "40", "71", "000", "Two", "IMP", group: "40P");
			var procedure3 = helper.CreateRefCusProcedure(currentCountry, "IM", "51", "00", "000", "Three", "IMP", group: "51P");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_SubStyle = "A";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = cei.PK;

			AssertEquals("There should be 3 selectable procedures.", 3, invoiceLine.Lookups.CPCList.Count);

			cei.CEI_Style = "40";
			invoiceLine.JI_Procedure = "5100000";
			AssertHasMessageErrorContaining("Selected procedure group doesn't match selected entry instruction style.", invoiceLine.JI_ProcedureInfo, "The procedure you have selected is not matching the entry instruction");
			invoiceLine.JI_Procedure = "4071000";
			AssertHasMessageErrorContaining("Selected procedure group doesn't match selected entry instruction style.", invoiceLine.JI_ProcedureInfo, "The procedure you have selected is not matching the entry instruction");
			invoiceLine.JI_Procedure = "4000000";
			AssertNoMessageErrorContaining("Selected procedure group doesn't match selected entry instruction style.", invoiceLine.JI_ProcedureInfo, "The procedure you have selected is not matching the entry instruction");
		}

		public void TestCheckJI_Procedure_CheckIfEmptyOrInListOfDeltaIE()
		{
			var declaration = Factory.New<JobDeclaration>();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateRefCusProcedure(declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure), "IM", "40", "00", "000", "One", "IMP", group: "H1");

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_SubStyle = "A";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = cei.PK;
			cei.CEI_Style = "H1";

			var message = "You have not entered a " + invoiceLine.JI_ProcedureInfo.HumanReadableName + ".";
			var message2 = "The code you have selected is not in the list.";

			invoiceLine.JI_FormattedProcedure = "";
			AssertHasMessageErrorContaining("When CPC is empty, a message error should be displayed", invoiceLine.JI_ProcedureInfo, message);
			invoiceLine.JI_FormattedProcedure = "4000000";
			AssertNoMessageErrorContaining("When CPC is not empty, a message error should not be displayed", invoiceLine.JI_ProcedureInfo, message);
			invoiceLine.JI_FormattedProcedure = "ABC";
			AssertHasMessageErrorContaining("When CPC is not in the list, a message error should be displayed", invoiceLine.JI_ProcedureInfo, message2);
		}

		public void TestCHC_NumberOfPack()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "BK", "Bulk", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackQty = 2;
			package1.CW_PackType = "BK";
			package1.CW_MarksAndNos = "AAAAAAAAAAAA";

			var package2 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package2.CW_PackQty = 2;
			package2.CW_PackType = "BK";
			package2.CW_MarksAndNos = "AAAAAAAAAAAA";

			var packing1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 0;

			var packing2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0];
			packing2.IsLinked = true;
			packing2.PackQty = 0;

			var invoiceLine1PackagePivot = invoiceLine1.PackagesPivot.Cast<InvoiceLinePackagePivot>().FirstOrDefault();

			var entryLine = Factory.New<CusEntryLine>();

			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			AssertEquals(2, entryLine.PackagingDetails.Count());

			entryLine.CL_LineNumber = 1;
			invoiceLine1.PackagesPivot.RunPreSaveValidation();
			AssertNoMessageErrorContaining("Bulk type of packs always allow 0 as pack number whatever the entry line it eventually applies to", invoiceLine1PackagePivot.CHC_NumberOfPacksInfo, "You have not entered");

			package1.CW_PackType = "CT";
			invoiceLine1.PackagesPivot.RunPreSaveValidation();
			AssertHasMessageErrorContaining("Non bulk type of packs don't allow 0 as pack number when the entry line it eventually applies to is the first of its entry and shows a package count of 0", invoiceLine1PackagePivot.CHC_NumberOfPacksInfo, "You have not entered");

			entryLine.CL_LineNumber = 2;
			invoiceLine1.PackagesPivot.RunPreSaveValidation();
			AssertNoMessageErrorContaining("Non bulk type of packs allow 0 as pack number when the entry line it eventually applies to is not the first of its entry ", invoiceLine1PackagePivot.CHC_NumberOfPacksInfo, "You have not entered");

			entryLine.CL_LineNumber = 1;
			packing2.PackQty = 2;
			invoiceLine1.PackagesPivot.RunPreSaveValidation();
			AssertNoMessageErrorContaining("Non bulk type of packs allow 0 as pack number when the entry line it eventually applies to is the first of its entry but shows a package count > 0 ", invoiceLine1PackagePivot.CHC_NumberOfPacksInfo, "You have not entered");
		}

		public void TestJI_Weight()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_Weight = 125.253m;
			AssertNoMessageErrorContaining(invoiceLine.JI_WeightInfo, MessageBuilderHelper.MessageGrossWeightGreaterThanZero);

			invoiceLine.JI_Weight = ZDecimal.Zero;
			AssertHasMessageErrorContaining(invoiceLine.JI_WeightInfo, MessageBuilderHelper.MessageGrossWeightGreaterThanZero);

			invoiceLine.JI_Weight = -1m;
			AssertHasMessageErrorContaining(invoiceLine.JI_WeightInfo, MessageBuilderHelper.MessageGrossWeightGreaterThanZero);
		}

		public void TestJI_Description()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_Description = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Description = "XXX";
			AssertNoMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestJI_PrimaryPreference()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_PrimaryPreference = "XXX";
			AssertNoMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_PrimaryPreference = "XXX";
			AssertNoMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestJI_ValuationCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				invoiceLine.JI_ValuationCode = ZString.Empty;
				AssertNoMessageErrorContaining("When declaration is UCC6 and export, JI_ValuationCode is not mandatory", invoiceLine.JI_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				invoiceLine.JI_ValuationCode = ZString.Empty;
				AssertNoMessageErrorContaining("When declaration is UCC6 and import, JI_ValuationCode is not mandatory", invoiceLine.JI_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				invoiceLine.JI_ValuationCode = ZString.Empty;
				AssertNoMessageErrorContaining("When declaration is not UCC6 and  is export, JI_ValuationCode is not mandatory", invoiceLine.JI_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				invoiceLine.JI_ValuationCode = ZString.Empty;
				AssertHasMessageErrorContaining("When declaration is not UCC6 and is import, JI_ValuationCode is mandatory", invoiceLine.JI_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_ValuationCode = "X";
				AssertNoMessageErrorContaining(invoiceLine.JI_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestJI_CustomsSecondUnitQty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_CustomsSecondQuantity = 0;
			invoiceLine.JI_CustomsSecondUnitQty = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsSecondUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_CustomsSecondQuantity = 1;
			invoiceLine.JI_CustomsSecondUnitQty = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsSecondUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestJI_CustomsThirdUnitQty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_CustomsThirdQuantity = 0;
			invoiceLine.JI_CustomsThirdUnitQty = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsThirdUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_CustomsThirdQuantity = 1;
			invoiceLine.JI_CustomsThirdUnitQty = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsThirdUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestJI_CustomsQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_Weight = 10;
			invoiceLine.JI_WeightUQ = "KG";

			invoiceLine.JI_CustomsQuantity = 12;
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, "Customs Quantity[38] can not be greater than GWT[35]");

			invoiceLine.JI_CustomsQuantity = 9;
			AssertNoMessageError(invoiceLine.JI_CustomsQuantityInfo, "Customs Quantity[38] can not be greater than GWT[35]");

			invoiceLine.JI_CustomsQuantity = 10;
			AssertNoMessageError(invoiceLine.JI_CustomsQuantityInfo, "Customs Quantity[38] can not be greater than GWT[35]");

			invoiceLine.JI_Weight = 10;
			invoiceLine.JI_WeightUQ = "G";

			invoiceLine.JI_CustomsQuantity = 9;
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, "Customs Quantity[38] can not be greater than GWT[35]");

			invoiceLine.JI_CustomsQuantity = 125.253m;
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, MessageBuilderHelper.MessageCustomsQuantityGreaterThanZero);

			invoiceLine.JI_CustomsQuantity = ZDecimal.Zero;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, MessageBuilderHelper.MessageCustomsQuantityGreaterThanZero);

			invoiceLine.JI_CustomsQuantity = -1m;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, MessageBuilderHelper.MessageCustomsQuantityGreaterThanZero);
		}

		public void TestCheckCPCAgainstEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Procedure = "40";
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var context = new ImportInvoiceLineValidationTestContext(invoiceLine);
			context.EnableRule(x => x.ShouldValidateCPCAgainstEntryInstruction);

			invoiceLine.JI_Procedure = ZString.Empty;
			AssertNoMessageErrorContaining("Empty JI_Procedure should not trigger validation error.", invoiceLine.JI_ProcedureInfo, "Invoice line Requested Procedure");

			invoiceLine.JI_Procedure = "4";
			AssertHasMessageErrorContaining("Single-character JI_Procedure should trigger validation error.", invoiceLine.JI_ProcedureInfo, "Invoice line Requested Procedure");

			invoiceLine.JI_Procedure = "4207F47";
			AssertHasMessageErrorContaining("Prefix does not match entry instruction CPC – should trigger validation error.", invoiceLine.JI_ProcedureInfo, "Invoice line Requested Procedure");

			invoiceLine.JI_Procedure = "407F47";
			AssertNoMessageErrorContaining("Prefix matches entry instruction CPC – should not trigger validation error.", invoiceLine.JI_ProcedureInfo, "Invoice line Requested Procedure");

			context.DisableRule(x => x.ShouldValidateCPCAgainstEntryInstruction);
			invoiceLine.JI_Procedure = "4207F47";
			AssertNoMessageErrorContaining("ShouldValidateCPCAgainstEntryInstruction is disabled - should not trigger validation error.", invoiceLine.JI_ProcedureInfo, "Invoice line Requested Procedure");
		}

		public void TestCheckJI_ProcedureForRule_C0834_N02()
		{
			var message = "[C0834_N02] For Concession 1DP, a fiscal reference of any type but FR5 must be served.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var context = new ImportInvoiceLineValidationTestContext(invoiceLine);
			context.EnableRule(x => x.IsRuleC0834_N02Active);

			invoiceLine.JI_Procedure = "40001DP";
			AssertHasMessageError(invoiceLine.JI_ProcedureInfo, message);

			var fiscalReference = invoiceLine.FiscalReferences.AddNew();
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError(invoiceLine.JI_ProcedureInfo, message);

			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError(invoiceLine.JI_ProcedureInfo, message);

			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, message);

			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, message);

			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR2_Customer;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, message);

			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, message);

			context.DisableRule(x => x.IsRuleC0834_N02Active);
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, message);

			invoiceLine.FiscalReferences.RemoveAndDeleteAll();

			declaration = invoiceLine.Declaration;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			context.EnableRule(x => x.IsRuleC0834_N02Active);
			fiscalReference = entryInstruction.FiscalReferences.AddNew();
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError(invoiceLine.JI_ProcedureInfo, message);

			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError(invoiceLine.JI_ProcedureInfo, message);

			context.DisableRule(x => x.IsRuleC0834_N02Active);
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, message);

			context.EnableRule(x => x.IsRuleC0834_N02Active);
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, message);

			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, message);

			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR2_Customer;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, message);

			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, message);
		}

		public void TestCheckJI_LinePriceForRuleNAT_240()
		{
			var message = "[NAT_240] Invoice Line price must be equal to 0 EUR When Additional Code 0097 (free goods) is selected.";

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var context = new ImportInvoiceLineValidationTestContext(invoiceLine);
			context.EnableRule(x => x.IsRuleNAT_240Active);

			invoiceLine.JI_LinePrice = 10.00;
			invoiceLine.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.FreeGoodsSupplementaryCode;
			invoiceLine.Validation.ValidateJI_LinePrice();
			Assert("Prerequisite: HasFreeGoods should be true when SupplementaryCode equal to 0097.", invoiceLine.HasFreeGoods);
			AssertHasMessageError("A message error is expected as InvoiceLine price must be 0 when it has supplementary code as 0097.", invoiceLine.JI_LinePriceInfo, message);

			context.DisableRule(x => x.IsRuleNAT_240Active);
			invoiceLine.Validation.ValidateJI_LinePrice();
			AssertNoMessageError("No message is expected as Rule NAT_240Active is disabled.", invoiceLine.JI_LinePriceInfo, message);

			context.EnableRule(x => x.IsRuleNAT_240Active);
			invoiceLine.JI_LinePrice = 0.00;
			invoiceLine.Validation.ValidateJI_LinePrice();
			AssertNoMessageError("No message error is expected as InvoiceLine price is equal to 0.", invoiceLine.JI_LinePriceInfo, message);

			invoiceLine.JI_LinePrice = 1.00;
			invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
			invoiceLine.Validation.ValidateJI_LinePrice();
			Assert("Prerequisite: HasFreeGoods should be false when there is no SupplementaryCode equal to 0097.", !invoiceLine.HasFreeGoods);
			AssertNoMessageError("No message error is expected as supplementary code is not 0097.", invoiceLine.JI_LinePriceInfo, message);
		}

		public void TestCheckJI_CountryOfOriginForRuleC0710_N01()
		{
			var message = "[C0710_N01] The Country of Origin is mandatory.";

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var context = new ImportInvoiceLineValidationTestContext(invoiceLine);
			var subTypeToConsider = new[] { EntrySubStyleList.Codes.NormalDeclaration, EntrySubStyleList.Codes.SimplifiedDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC, EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationOfSimplifiedDeclarationsCoveredByCAndF, EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationUnderTheProcedureCoveredUnderArticle182OfTheCode, EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF, EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic };

			foreach (var subStyle in subTypeToConsider)
			{
				context.EnableRule(x => x.IsRuleC0710_N01Active);
				declaration.JE_CustomsOffice = "FR0040";
				entryInstruction.CEI_SubStyle = subStyle;
				entryInstruction.CEI_Procedure = ZString.Empty;
				invoiceLine.JI_CountryOfOrigin = ZString.Empty;
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertHasMessageError($"A message error is expected as CEI_SubStyle is {subStyle} and CEI_Procedure is not 71 and JE_CustomsOffice starts with FR.", invoiceLine.JI_CountryOfOriginInfo, message);

				declaration.JE_CustomsOffice = "FR0040";
				entryInstruction.CEI_SubStyle = subStyle;
				entryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
				invoiceLine.JI_CountryOfOrigin = CountryCodes.China;
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertNoMessageError("A message error is not expected as JI_CountryOfOrigin is entered.", invoiceLine.JI_CountryOfOriginInfo, message);

				declaration.JE_CustomsOffice = "FR0040";
				entryInstruction.CEI_SubStyle = subStyle;
				entryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
				invoiceLine.JI_CountryOfOrigin = ZString.Empty;
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertNoMessageError("A message error is not expected as CEI_Procedure is 71.", invoiceLine.JI_CountryOfOriginInfo, message);

				declaration.JE_CustomsOffice = "ES2819";
				entryInstruction.CEI_SubStyle = subStyle;
				entryInstruction.CEI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration;
				invoiceLine.JI_CountryOfOrigin = ZString.Empty;
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertNoMessageError("A message error is not expected as JE_CustomsOffice does not start with FR.", invoiceLine.JI_CountryOfOriginInfo, message);

				context.DisableRule(x => x.IsRuleC0710_N01Active);
				declaration.JE_CustomsOffice = "FR0040";
				entryInstruction.CEI_SubStyle = subStyle;
				entryInstruction.CEI_Procedure = ZString.Empty;
				invoiceLine.JI_CountryOfOrigin = ZString.Empty;
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertNoMessageError("A message error is not expected as rule C0710_N01 is disabled.", invoiceLine.JI_CountryOfOriginInfo, message);
			}

			context.EnableRule(x => x.IsRuleC0710_N01Active);
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
			entryInstruction.CEI_Procedure = ZString.Empty;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertNoMessageError("A message error is not expected as CEI_SubStyle is not in the list to consider.", invoiceLine.JI_CountryOfOriginInfo, message);
		}

		public void TestCheckJI_CountryOfOriginForRuleNAT_105()
		{
			var message = "[NAT_105] Country of Origin can't be an EU one if declaration type is 'IM'.";

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var context = new ImportInvoiceLineValidationTestContext(invoiceLine);

			context.EnableRule(x => x.IsRuleNAT_105Active);
			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
			invoiceLine.JI_CountryOfOrigin = CountryCodes.Spain;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertHasMessageError($"A message error is expected as JE_EntryStyle is IM and JI_CountryOfOrigin is a EU member.", invoiceLine.JI_CountryOfOriginInfo, message);

			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromEFTAMember;
			invoiceLine.JI_CountryOfOrigin = CountryCodes.Spain;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertNoMessageError($"A message error is not expected as JE_EntryStyle not IM.", invoiceLine.JI_CountryOfOriginInfo, message);

			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
			invoiceLine.JI_CountryOfOrigin = CountryCodes.China;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertNoMessageError($"A message error is not expected as JI_CountryOfOrigin is not a EU member.", invoiceLine.JI_CountryOfOriginInfo, message);

			context.DisableRule(x => x.IsRuleNAT_105Active);
			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
			invoiceLine.JI_CountryOfOrigin = CountryCodes.Spain;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertNoMessageError($"A message error is not expected as rule NAT_105 is disabled.", invoiceLine.JI_CountryOfOriginInfo, message);
		}

		public void TestCheckJI_CountryOfOriginForRuleNAT_254()
		{
			var message = "[NAT_254] Country of Origin must either be TR, AD or SM for Pref. Code '400' and '420'.";

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var context = new ImportInvoiceLineValidationTestContext(invoiceLine);

			context.EnableRule(x => x.IsRuleNAT_254Active);
			invoiceLine.JI_PrimaryPreference = Core.Constants.Customs.Universal.RefCusPreference.Codes._400;
			invoiceLine.JI_CountryOfOrigin = CountryCodes.Spain;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertHasMessageError("A message error is expected as JI_PrimaryPreference is 400 and JI_CountryOfOrigin is not TR / AD / SM.", invoiceLine.JI_CountryOfOriginInfo, message);

			invoiceLine.JI_PrimaryPreference = Core.Constants.Customs.Universal.RefCusPreference.Codes._420;
			invoiceLine.JI_CountryOfOrigin = CountryCodes.Spain;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertHasMessageError("A message error is expected as JI_PrimaryPreference is 420 and JI_CountryOfOrigin is not TR / AD / SM.", invoiceLine.JI_CountryOfOriginInfo, message);

			invoiceLine.JI_PrimaryPreference = Core.Constants.Customs.Universal.RefCusPreference.Codes._420;
			invoiceLine.JI_CountryOfOrigin = CountryCodes.Turkey;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertNoMessageError("A message error is not expected as JI_CountryOfOrigin is TR.", invoiceLine.JI_CountryOfOriginInfo, message);

			invoiceLine.JI_PrimaryPreference = Core.Constants.Customs.Universal.RefCusPreference.Codes._420;
			invoiceLine.JI_CountryOfOrigin = CountryCodes.Andorra;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertNoMessageError("A message error is not expected as JI_CountryOfOrigin is AD.", invoiceLine.JI_CountryOfOriginInfo, message);

			invoiceLine.JI_PrimaryPreference = Core.Constants.Customs.Universal.RefCusPreference.Codes._420;
			invoiceLine.JI_CountryOfOrigin = CountryCodes.SanMarino;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertNoMessageError("A message error is not expected as JI_CountryOfOrigin is SM.", invoiceLine.JI_CountryOfOriginInfo, message);

			invoiceLine.JI_PrimaryPreference = Core.Constants.Customs.Universal.RefCusPreference.Codes._100;
			invoiceLine.JI_CountryOfOrigin = CountryCodes.Turkey;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertNoMessageError("A message error is not expected as JI_PrimaryPreference is not 400 or 420.", invoiceLine.JI_CountryOfOriginInfo, message);

			context.DisableRule(x => x.IsRuleNAT_254Active);
			invoiceLine.JI_PrimaryPreference = Core.Constants.Customs.Universal.RefCusPreference.Codes._400;
			invoiceLine.JI_CountryOfOrigin = CountryCodes.Spain;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertNoMessageError("A message error is not expected as rule NAT_254 is disabled.", invoiceLine.JI_CountryOfOriginInfo, message);
		}

		public void TestCheckJI_ProcedureRuleNat_030()
		{
			var messageError = "[NAT_030] Fiscal reference FR5 must be provided when using concession F48.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var context = new ImportInvoiceLineValidationTestContext(invoiceLine);
			context.EnableRule(x => x.IsRuleNAT_030Active);

			invoiceLine.JI_Procedure = "1234F48";
			AssertHasMessageError("No Fiscal reference FR5 provided when JI_Procedure is F48. An error message should show.", invoiceLine.JI_ProcedureInfo, messageError);

			context.DisableRule(x => x.IsRuleNAT_030Active);
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("RuleNAT_30 is disabled. No error message should show.", invoiceLine.JI_ProcedureInfo, messageError);

			context.EnableRule(x => x.IsRuleNAT_030Active);
			var cusFiscalReference2 = invoiceLine.FiscalReferences.AddNew();
			cusFiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("Fiscal reference FR5 provided. No error message should show.", invoiceLine.JI_ProcedureInfo, messageError);

			cusFiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError("Fiscal reference provided without FR5. An error message should show.", invoiceLine.JI_ProcedureInfo, messageError);

			invoiceLine.JI_Procedure = "1234F49";
			AssertNoMessageError("JI_Procedure is other than F48. No error message should show.", invoiceLine.JI_ProcedureInfo, messageError);

			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = cusEntryInstruction.PK;
			invoiceLine.JI_Procedure = "1234F48";

			var cusFiscalReference = cusEntryInstruction.FiscalReferences.AddNew();
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("Fiscal reference FR5 provided at EntryInstruction level when JI_Procedure is F48. No error message should show.", invoiceLine.JI_ProcedureInfo, messageError);

			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError("Fiscal reference provided without FR5. An error message should show.", invoiceLine.JI_ProcedureInfo, messageError);

			invoiceLine.JI_Procedure = "1234F49";
			AssertNoMessageError("JI_Procedure is other than F48. No error message should show.", invoiceLine.JI_ProcedureInfo, messageError);

			context.DisableRule(x => x.IsRuleNAT_030Active);
			invoiceLine.JI_Procedure = "1234F48";
			AssertNoMessageError("RuleNAT_30 is disabled. No error message should show.", invoiceLine.JI_ProcedureInfo, messageError);
		}
	}
}
