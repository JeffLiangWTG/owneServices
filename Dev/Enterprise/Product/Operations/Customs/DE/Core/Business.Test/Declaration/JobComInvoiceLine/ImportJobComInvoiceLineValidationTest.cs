using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class ImportJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJI_CustomsValue()
		{
			const string messageError = "Customs Value cannot be smaller than zero. Please check Line Price and/or deduction Charges.";

			var header = declaration.Invoices.AddNew();
			header.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var line = header.InvoiceLines.AddNew();
			line.JI_LinePrice = 1000m;
			var chargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;

			var overseasFreight = line.Charges.AddNew();
			overseasFreight.J7_ChargeType = chargeFactory.FreightAfterEUBorderCode;
			overseasFreight.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			overseasFreight.J7_Amount = 100m;
			overseasFreight.J7_IsDutiable = false;

			CombineAssertions(() =>
			{
				AssertEquals("Customs value = 1000-100 = $900", 900m, line.JI_CustomsValue);
				line.Validation.ValidateAll();
				AssertNoRowMessageError("Customs value > 0 is valid", line, messageError);

				overseasFreight.J7_Amount = 2000m;
				AssertEquals("Customs value = 1000-2000 = $-1000", -1000m, line.JI_CustomsValue);
				line.Validation.ValidateAll();
				AssertHasRowMessageError("Customs value < 0 is invalid", line, messageError);

				overseasFreight.J7_Amount = 1000m;
				AssertEquals("Customs value = 1000-1000 = $0", 0m, line.JI_CustomsValue);
				line.Validation.ValidateAll();
				AssertNoRowMessageError("Customs value = 0 is valid", line, messageError);
			});
		}

		public void TestCheckJI_CustomsThirdQuantity()
		{
			const string message = "Third Qty (Customs Qty) should be greater than zero.";
			CombineAssertions(() =>
			{
				foreach (var concessionCode in GetConcessionCodes())
				{
					AssertJI_CustomsThirdQuantityInfoHasError(concessionCode);
				}

				invoiceLine.JI_FormattedProcedure = "4000C22";
				invoiceLine.JI_CustomsThirdQuantity = 0;
				AssertNoMessageError(invoiceLine.JI_CustomsThirdQuantityInfo, message);
			});

			IEnumerable<string> GetConcessionCodes()
			{
				yield return CustomsProcedureCodeList.Import.Concession._E01;
				yield return CustomsProcedureCodeList.Import.Concession._E02;
				yield return CustomsProcedureCodeList.Import.Concession._8E3;
				yield return CustomsProcedureCodeList.Import.Concession._8E6;
				yield return CustomsProcedureCodeList.Import.Concession._8E8;
				yield return CustomsProcedureCodeList.Import.Concession._8E9;
			}

			void AssertJI_CustomsThirdQuantityInfoHasError(string concession)
			{
				invoiceLine.JI_FormattedProcedure = $"4000{concession}";
				invoiceLine.JI_CustomsThirdQuantity = 0;
				AssertHasMessageError(invoiceLine.JI_CustomsThirdQuantityInfo, message);
			}
		}

		public void TestCheckJI_ExtraInfoForClassification()
		{
			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateJI_ExtraInfoForClassification();
				AssertNoMessageErrorContaining("No Identification Means", invoiceLine.JI_ExtraInfoForClassificationInfo, MandatoryValidation.YouHaveNotEntered);
				invoiceLine.ZG_IdentificationMeansType = IdentificationMeansTypeList.Codes.D;
				invoiceLine.Validation.ValidateJI_ExtraInfoForClassification();
				AssertNoMessageErrorContaining("No Entry Instruction", invoiceLine.JI_ExtraInfoForClassificationInfo, MandatoryValidation.YouHaveNotEntered);
				var instruction = Factory.CreateInwardProcessingInstruction();
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.Validation.ValidateJI_ExtraInfoForClassification();
				AssertHasMessageErrorContaining("Requirements met", invoiceLine.JI_ExtraInfoForClassificationInfo, MandatoryValidation.YouHaveNotEntered);
				invoiceLine.JI_ExtraInfoForClassification = "EXTRA INFO";
				AssertNoMessageErrorContaining("Entered", invoiceLine.JI_ExtraInfoForClassificationInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestMaxSpecialLinesAllowed()
		{
			var taxes = new List<JobComInvoiceLineTax>();
			for (var i = 0; i < 9; i++)
			{
				taxes.Add(invoiceLine.Taxes.AddNew());
			}

			AssertEquals(9, taxes.Count);
			AssertNoRowError(taxes[8], "You are only allowed a maximum of 9 Special Cases per Invoice Line.");

			taxes.Add(invoiceLine.Taxes.AddNew());
			AssertEquals(10, taxes.Count);
			AssertHasRowError(taxes[9], "You are only allowed a maximum of 9 Special Cases per Invoice Line.");
		}

		public void TestCheckJI_BondedWhsQuantity()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			AssertEquals("Pre-condition IsEarlyClearanceFlagApplicable", true, entryInstruction.IsEarlyClearanceFlagApplicable);
			AssertEquals("Pre-condition IsOutOfWarehouseWarehousing", false, invoiceLine.IsOutOfWarehouseWarehousing);

			var info = invoiceLine.JI_BondedWhsQuantityInfo;
			VoidParameterlessDelegate assertions = () =>
			{
				invoiceLine.JI_BondedWhsUnitQty = "CTH";
				invoiceLine.JI_BondedWhsQuantity = -1m;
				AssertHasErrorContaining(info, MandatoryValidation.ValueCannotBeNegative);
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageError(info, "A maximum of 12 numbers (including decimals) can be entered.");

				invoiceLine.JI_BondedWhsQuantity = ZDecimal.Zero;
				AssertNoErrorContaining(info, MandatoryValidation.ValueCannotBeNegative);
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageError(info, "A maximum of 12 numbers (including decimals) can be entered.");

				invoiceLine.JI_BondedWhsQuantity = 1m;
				AssertNoErrorContaining(info, MandatoryValidation.ValueCannotBeNegative);
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageError(info, "A maximum of 12 numbers (including decimals) can be entered.");

				invoiceLine.JI_BondedWhsQuantity = 123456789.012;
				AssertNoErrorContaining(info, MandatoryValidation.ValueCannotBeNegative);
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageError(info, "A maximum of 12 numbers (including decimals) can be entered.");

				invoiceLine.JI_BondedWhsQuantity = 1234567890.123;
				AssertNoErrorContaining(info, MandatoryValidation.ValueCannotBeNegative);
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageError(info, "A maximum of 12 numbers (including decimals) can be entered.");
			};

			CombineAssertions("When AdditionalImportFieldsExist is true", assertions);

			entryInstruction.CEI_Style = "ZZ";
			invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;
			AssertEquals("Pre-condition IsOutOfWarehouseWarehousing", true, invoiceLine.IsOutOfWarehouseWarehousing);

			CombineAssertions("When AdditionalImportFieldsExist is false and IsOutOfWarehouseWarehousing is true", assertions);
			invoiceLine.JI_Procedure = "";

			AssertEquals("Pre-condition IsOutOfWarehouseWarehousing", false, invoiceLine.IsOutOfWarehouseWarehousing);
			AssertEquals("Pre-condition IsEarlyClearanceFlagApplicable", false, entryInstruction.IsEarlyClearanceFlagApplicable);

			CombineAssertions("When AdditionalImportFieldsExist and IsOutOfWarehouseWarehousing is false", () =>
			{
				invoiceLine.JI_BondedWhsQuantity = -1m;
				AssertNoNotifications(info);

				invoiceLine.JI_BondedWhsQuantity = ZDecimal.Zero;
				AssertNoNotifications(info);

				invoiceLine.JI_BondedWhsQuantity = 1m;
				AssertNoNotifications(info);

				invoiceLine.JI_BondedWhsQuantity = 123456789.012;
				AssertNoNotifications(info);

				invoiceLine.JI_BondedWhsQuantity = 1234567890.123;
				AssertNoNotifications(info);
			});
		}

		public void TestCheckJI_BondedWhsQuantity_MustBeInteger()
		{
			const string messageError = "Unit of quantity 'NAR', 'NARB', 'NCL' or 'NPR' requires an Integer value.";
			var propertyInfo = invoiceLine.JI_BondedWhsQuantityInfo;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			CombineAssertions(() =>
			{
				foreach (var integerRequiredUnit in new[]
				{
					Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems,
					Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfCells,
					Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfPairs,
					Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItemsPerFlask
				})
				{
					invoiceLine.JI_BondedWhsUnitQty = integerRequiredUnit;
					invoiceLine.JI_BondedWhsQuantity = 1.1m;
					AssertHasMessageError($"JI_BondedWhsUnitQty = {integerRequiredUnit}, JI_BondedWhsQuantity not integer", propertyInfo, messageError);

					invoiceLine.JI_BondedWhsQuantity = 1m;
					AssertNoMessageError($"JI_BondedWhsUnitQty = {integerRequiredUnit}, JI_BondedWhsQuantity is integer", propertyInfo, messageError);
				}

				invoiceLine.JI_BondedWhsUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
				invoiceLine.JI_BondedWhsQuantity = 1.1m;
				AssertNoMessageError("JI_BondedWhsUnitQty = KGM, JI_BondedWhsQuantity is not integer", propertyInfo, messageError);
			});
		}

		public void TestCheckJI_PreviousEntryLineNumber()
		{
			const string notification = "You have not entered a Previous Entry Line.";
			invoiceLine.JI_PreviousEntryNumber = "1111";
			invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, notification);
			invoiceLine.JI_PreviousEntryNumber = ZString.Empty;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_PreviousEntryLineNumberInfo, notification);
			invoiceLine.JI_PreviousEntryNumber = "1111";
			invoiceLine.JI_Procedure = "";
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_PreviousEntryLineNumberInfo, notification);
		}

		public void TestCheckJI_PreviousEntryNumber()
		{
			const string notification = "You have not entered a Previous Entry Number.";
			invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;
			invoiceLine.JI_BondedWhsQuantity = 1M;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_PreviousEntryNumberInfo, notification);
			invoiceLine.JI_BondedWhsQuantity = 0M;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_PreviousEntryNumberInfo, notification);
			invoiceLine.JI_BondedWhsQuantity = 1M;
			invoiceLine.JI_Procedure = "";
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_PreviousEntryNumberInfo, notification);
		}

		public void TestCheckJI_BondedWhsUnitQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany");
			helper.CreateCusCodeType("CUSUQ", "Customs Unit Quantity");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, "CUSUQ", "NAR", "Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			AssertEquals("Pre-condition IsEarlyClearanceFlagApplicable", true, entryInstruction.IsEarlyClearanceFlagApplicable);
			AssertEquals("Pre-condition IsOutOfWarehouseWarehousing", false, invoiceLine.IsOutOfWarehouseWarehousing);

			var info = invoiceLine.JI_BondedWhsUnitQtyInfo;
			VoidParameterlessDelegate assertions = () =>
			{
				invoiceLine.JI_BondedWhsQuantity = ZDecimal.Zero;
				invoiceLine.JI_BondedWhsUnitQty = ZString.Empty;
				invoiceLine.Validation.ValidateJI_BondedWhsUnitQty();
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

				invoiceLine.JI_BondedWhsQuantity = 10m;
				invoiceLine.JI_BondedWhsUnitQty = ZString.Empty;
				invoiceLine.Validation.ValidateJI_BondedWhsUnitQty();
				AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

				invoiceLine.JI_BondedWhsUnitQty = "NAR";
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

				invoiceLine.JI_BondedWhsUnitQty = "ZZZ";
				AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
			};

			CombineAssertions("When AdditionalImportFieldsExist is true", assertions);

			entryInstruction.CEI_Style = "ZZ";
			invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;
			AssertEquals("Pre-condition IsOutOfWarehouseWarehousing", true, invoiceLine.IsOutOfWarehouseWarehousing);

			CombineAssertions("When AdditionalImportFieldsExist is false and IsOutOfWarehouseWarehousing is true", assertions);
			invoiceLine.JI_Procedure = "";

			AssertEquals("Pre-condition IsOutOfWarehouseWarehousing", false, invoiceLine.IsOutOfWarehouseWarehousing);
			AssertEquals("Pre-condition IsEarlyClearanceFlagApplicable", false, entryInstruction.IsEarlyClearanceFlagApplicable);

			CombineAssertions("When AdditionalImportFieldsExist and IsOutOfWarehouseWarehousing is false", () =>
			{
				invoiceLine.JI_BondedWhsQuantity = ZDecimal.Zero;
				invoiceLine.JI_BondedWhsUnitQty = ZString.Empty;
				invoiceLine.Validation.ValidateJI_BondedWhsUnitQty();
				AssertNoNotifications(info);

				invoiceLine.JI_BondedWhsQuantity = 10m;
				invoiceLine.JI_BondedWhsUnitQty = ZString.Empty;
				AssertNoNotifications(info);

				invoiceLine.JI_BondedWhsUnitQty = "NAR";
				AssertNoNotifications(info);

				invoiceLine.JI_BondedWhsUnitQty = "ZZZ";
				AssertNoNotifications(info);
			});
		}

		public void TestCheckJI_ValuationCode()
		{
			invoiceLine.JI_ValuationCode = "X";
			AssertEquals("No List validation", false, invoiceLine.JI_ValuationCodeInfo.Notifications.Any(x => x.Message.Contains(ListValidation.InvalidCodeMessageError.ToString())));
		}

		public void TestCheckJI_CustomsSecondQuantity()
		{
			const string errorMessage = "the maximum value allowed for [41] Supp. Qty is 999,999,999.999.";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_CustomsSecondQuantity = 1234567890.123m;
			AssertNoErrorContaining(invoiceLine.JI_CustomsSecondQuantityInfo, errorMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.Validation.ValidateJI_CustomsSecondQuantity();
			AssertHasErrorContaining(invoiceLine.JI_CustomsSecondQuantityInfo, errorMessage);

			invoiceLine.JI_CustomsSecondQuantity = 123456789.123m;
			AssertNoErrorContaining(invoiceLine.JI_CustomsSecondQuantityInfo, errorMessage);
		}

		public void TestCheckJI_LineNo()
		{
			var ctiCusCode = invoiceLine.ContentInformationTypes.AddNew();
			ctiCusCode.CY_Code = "01";
			invoiceLine.Validation.ValidateAll();
			AssertNoMessageError(invoiceLine.JI_LineNoInfo, "The maximum number of the Content Information grid records is 3.");

			ctiCusCode = invoiceLine.ContentInformationTypes.AddNew();
			ctiCusCode.CY_Code = "01";
			invoiceLine.Validation.ValidateAll();
			AssertNoMessageError(invoiceLine.JI_LineNoInfo, "The maximum number of the Content Information grid records is 3.");

			ctiCusCode = invoiceLine.ContentInformationTypes.AddNew();
			ctiCusCode.CY_Code = "01";
			invoiceLine.Validation.ValidateAll();
			AssertNoMessageError(invoiceLine.JI_LineNoInfo, "The maximum number of the Content Information grid records is 3.");

			ctiCusCode = invoiceLine.ContentInformationTypes.AddNew();
			ctiCusCode.CY_Code = "01";
			invoiceLine.Validation.ValidateAll();
			AssertHasMessageError(invoiceLine.JI_LineNoInfo, "The maximum number of the Content Information grid records is 3.");
		}

		public void TestCheckJI_Tariff()
		{
			var info = invoiceLine.JI_TariffInfo;
			var msgError = "The Tariff Code entered is not valid for the current context.";

			invoiceLine.JI_Tariff = ZString.Empty;
			AssertNoMessageErrorContaining(info, msgError);

			invoiceLine.JI_Tariff = "20064000004";
			AssertHasMessageErrorContaining(info, msgError);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeExport = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Export);
			Factory.Save();
			var tariffExport = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeExport.PK, "08091998", ZDateTime.BrettsBirthday, ZDateTime.Today);
			invoiceLine.JI_Tariff = tariffExport.ZZ1_TariffCode;
			AssertHasMessageErrorContaining(info, msgError);

			var tariffTypeImport = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Import);
			Factory.Save();
			var tariffImport = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeImport.PK, "1006400000", ZDateTime.BrettsBirthday, ZDateTime.Today);
			invoiceLine.JI_Tariff = tariffImport.ZZ1_TariffCode;
			AssertHasMessageErrorContaining(info, msgError);

			var tariffNational = helper.CreateTariffNationalCode(Core.Constants.CountryCodes.Germany, tariffImport.PK, "4", ZDateTime.BrettsBirthday, ZDateTime.Today, ZDate.BrettsBirthday);
			invoiceLine.JI_Tariff = tariffNational.ZZ1_TariffCode;
			AssertNoMessageErrorContaining(info, msgError);

			var tradeGroupStandard = helper.CreateTradeGroup(Core.Constants.CountryCodes.Germany, "STANDARD", ZDateTime.BrettsBirthday, ZDateTime.Today);

			var addRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Germany, Constants.RateTypes.AntiDumping, "Anti-Dumping");
			var rateCode21 = helper.LoadOrCreateNewCusRateCode(Factory, "AD1", addRateType.PK);
			var testRate5 = helper.CreateRate(tariffNational, rateCode21.PK, ZDateTime.BrettsBirthday, ZDateTime.Today, dataGrouping: Core.Constants.CountryCodes.Germany);
			helper.CreateCusApplicability(testRate5, tradeGroupStandard, ZDateTime.BrettsBirthday, ZDateTime.Today, additionalCode: "111");
			Factory.Save();

			invoiceLine.JI_CountryOfOrigin = "CN";

			AssertHasWarningContaining(info, "There is no applicable");
		}

		[TestDate(2025, 2, 15)]
		public void TestCheckJI_Tariff_InwardProcessing_AVABR()
		{
			var info = invoiceLine.JI_TariffInfo;
			var msgError = "The Customs Control condition is not satisfied";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Constants.TariffTypes.Import);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffType.PK, "1006400000", ZDateTime.BrettsBirthday, ZDateTime.Today);
			var tariffNational = helper.CreateTariffNationalCode(Core.Constants.CountryCodes.Germany, tariff.PK, "4", ZDateTime.BrettsBirthday, ZDateTime.Today, ZDate.BrettsBirthday);

			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber);
			var refConditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "724");
			var refCondition = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.Germany, refConditionType.PK, tariff.PK, ZString.Empty, true, false, ZDateTime.BrettsBirthday, ZDateTime.Today);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, refCondition.PK, "Y160");
			Factory.Save();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZA;
			invoiceLine.JI_Tariff = tariffNational.ZZ1_TariffCode;
			AssertHasMessageErrorContaining(info, msgError);

			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining(info, msgError);
		}

		public void TestCheckJI_Tariff_AWarningWhereValidRatesExist()
		{
			var startDate = new ZDate(2010, 12, 10);
			var endDate = new ZDate(2079, 06, 06);
			var testHelper = new UniversalReferenceTestDataHelper(Factory);

			var tradeGroupStandard = testHelper.CreateTradeGroup(Core.Constants.CountryCodes.Germany, "STANDARD", startDate, endDate);
			testHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, startDate, endDate);
			Factory.Save();

			var hsnTariffType = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Import);
			Factory.Save();
			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Germany, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = testHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
			Factory.Save();
			var preferenceSTD = testHelper.CreatePreferenceForCountry("STD", "Standard", Core.Constants.CountryCodes.Germany);
			Factory.Save();

			var cusTariff = testHelper.CreateTariff(Core.Constants.CountryCodes.Germany, hsnTariffType.PK, "1006400000", startDate, endDate, "dummy Description 0");
			var tariffNational = testHelper.CreateTariffNationalCode(Core.Constants.CountryCodes.Germany, cusTariff.PK, "4", ZDateTime.BrettsBirthday, ZDateTime.Today, ZDate.BrettsBirthday);
			Factory.Save();

			var testRate = testHelper.CreateRate(cusTariff, rateCode.PK, startDate, endDate, "0", preferencePk: preferenceSTD.PK);
			testHelper.CreateCusApplicability(testRate, tradeGroupStandard, startDate, endDate, orderNumber: "Order1");
			Factory.Save();

			const string ratesExistWhere = "rates exist where";
			CombineAssertions(() =>
			{
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
				invoiceLine.JI_Tariff = tariffNational.ZZ1_TariffCode;
				AssertHasWarningContaining("There is no applicable Duty rate for the Tariff '10064000004' and Country Of Origin 'AU'", invoiceLine.JI_TariffInfo, ratesExistWhere);

				invoiceLine.JI_PrimaryPreference = "STD";
				invoiceLine.JI_ConcessionOrder = "Order1";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoWarningContaining("A applicable Duty rate exists", invoiceLine.JI_TariffInfo, ratesExistWhere);
			});
		}

		public void TestCheckJI_Weight()
		{
			var info = invoiceLine.JI_WeightInfo;
			var messageError = "You have not entered a valid Gross Weight.";
			invoiceLine.Validation.ValidateJI_Weight();
			AssertHasMessageError(info, messageError);

			invoiceLine.JI_Weight = 1;
			invoiceLine.Validation.ValidateJI_Weight();
			AssertNoMessageError(info, messageError);

			invoiceHeader.JZ_Weight = 0;
			invoiceLine.JI_Weight = 0;
			invoiceLine.Validation.ValidateJI_Weight();
			AssertHasMessageError(info, messageError);

			invoiceHeader.JZ_Weight = 1;
			invoiceLine.Validation.ValidateJI_Weight();
			AssertNoMessageError(info, messageError);

			invoiceHeader.JZ_Weight = -1;
			invoiceLine.JI_Weight = -1;
			invoiceLine.Validation.ValidateJI_Weight();
			AssertHasMessageError(info, messageError);
		}

		public void TestCheckJI_Procedure_SpecificRate()
		{
			const string messageError = "This CPC requires a Charge Code of Type SRC or SRN or SRS.";
			invoiceLine.JI_Procedure = "4900C22";
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, messageError);

			invoiceLine.JI_Procedure = "40718E6";
			AssertHasMessageError(invoiceLine.JI_ProcedureInfo, messageError);

			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = ImportChargeCodeList.Codes.SRC;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, messageError);
		}

		public void TestCheckJI_ProcedureFirstTwoDigitsEqualCEI_Procedure()
		{
			const string messageError = "Current Procedure must match Procedure on Entry Instruction.";

			CombineAssertions(() =>
			{
				entryInstruction.CEI_Procedure = "40";
				invoiceLine.JI_Procedure = "4100";
				AssertHasMessageError("No match", invoiceLine.JI_ProcedureInfo, messageError);

				invoiceLine.JI_Procedure = "4000";
				AssertNoMessageError("Match", invoiceLine.JI_ProcedureInfo, messageError);
			});
		}

		public void TestCheckJI_ProcedureRequiringTransactionNature32()
		{
			const string message = "Concession Codes 'E01' and 'E02' require [24] Transaction Nature to be '32'.";

			CombineAssertions(() =>
			{
				invoiceHeader.JZ_ValuationCode = "31";

				invoiceLine.JI_Procedure = "4000E01";
				AssertHasMessageError("JZ_ValuationCode <> 32, concession E01", invoiceLine.JI_ProcedureInfo, message);

				invoiceLine.JI_Procedure = "4000E02";
				AssertHasMessageError("JZ_ValuationCode <> 32, concession E02", invoiceLine.JI_ProcedureInfo, message);

				invoiceHeader.JZ_ValuationCode = "32";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("JZ_ValuationCode = 32, concession E02", invoiceLine.JI_ProcedureInfo, message);

				invoiceHeader.JZ_ValuationCode = "31";
				invoiceLine.JI_Procedure = "40008E2";
				AssertNoMessageError("JZ_ValuationCode <> 32, concession 8E2", invoiceLine.JI_ProcedureInfo, message);
			});
		}

		public void TestCheckJI_ProcedureRequiringTransactionNatureOtherThan32()
		{
			const string message = "Concession Code '8E2' requires a [24] Transaction Nature other than '32'.";

			CombineAssertions(() =>
			{
				invoiceHeader.JZ_ValuationCode = "32";

				invoiceLine.JI_Procedure = "4000E01";
				AssertNoMessageError("JZ_ValuationCode = 32, concession E01", invoiceLine.JI_ProcedureInfo, message);

				invoiceLine.JI_Procedure = "4000E02";
				AssertNoMessageError("JZ_ValuationCode = 32, concession E02", invoiceLine.JI_ProcedureInfo, message);

				invoiceLine.JI_Procedure = "40008E2";
				AssertHasMessageError("JZ_ValuationCode = 32, concession E8E2", invoiceLine.JI_ProcedureInfo, message);

				invoiceHeader.JZ_ValuationCode = "31";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("JZ_ValuationCode <> 32, concession E8E2", invoiceLine.JI_ProcedureInfo, message);
			});
		}

		public void TestCheck_JI_RN_NKCountryOfExport()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_RN_NKCountryOfExportInfo, "RQ", Core.Constants.CountryCodes.France);
		}

		public void TestCheck_JI_RN_NKCountryOfExport_Mandatory()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
				invoiceLine.Validation.ValidateJI_RN_NKCountryOfExport();
				AssertNoMessageErrorContaining(invoiceLine.JI_RN_NKCountryOfExportInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
				invoiceLine.Validation.ValidateJI_RN_NKCountryOfExport();
				AssertHasMessageErrorContaining(invoiceLine.JI_RN_NKCountryOfExportInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Russia;
				invoiceLine.Validation.ValidateJI_RN_NKCountryOfExport();
				AssertNoMessageErrorContaining(invoiceLine.JI_RN_NKCountryOfExportInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheck_JI_CustomDate1()
		{
			const string message = "Decisive Date cannot be in future.";
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
				invoiceLine.JI_CustomDate1 = ZDate.Today.AddDays(10);
				AssertHasMessageErrorContaining("Must be not in future", invoiceLine.JI_CustomDate1Info, message);

				invoiceLine.JI_CustomDate1 = ZDate.Today;
				AssertNoMessageErrorContaining("Current date", invoiceLine.JI_CustomDate1Info, message);
			});
		}

		public void TestCheck_JI_CustomDate1_Mandatory()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
				invoiceLine.Validation.ValidateJI_CustomDate1();
				AssertNoMessageErrorContaining("CEI_Style 'AAV'", invoiceLine.JI_CustomDate1Info, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
				invoiceLine.Validation.ValidateJI_CustomDate1();
				AssertHasMessageErrorContaining("CEI_Style 'LUZ', JI_CustomDate1 empty", invoiceLine.JI_CustomDate1Info, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_CustomDate1 = ZDate.Today;
				AssertNoMessageErrorContaining("CEI_Style 'LUZ', JI_CustomDate1 set", invoiceLine.JI_CustomDate1Info, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckAirFreightCostsExists_JE_IsHighValueOvrd()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			invoiceHeader.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._1;

			CombineAssertions(() =>
			{
				invoiceLine.Validation.CheckAirFreightCostsExists();
				AssertNoRowMessageError("Not AIR", invoiceLine, MissingAirFreightCostsRowMessageError);

				declaration.ZG_IsHighValueOvrd = true;
				ClearInvoiceLineRowNotificationsAndValidate(() => invoiceLine.Validation.CheckAirFreightCostsExists());
				AssertHasRowMessageError("AIR", invoiceLine, MissingAirFreightCostsRowMessageError);
			});
		}

		public void TestCheckAirFreightCostsExists_JE_TransportMode()
		{
			declaration.ZG_IsHighValueOvrd = true;
			invoiceHeader.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._1;

			CombineAssertions(() =>
			{
				invoiceLine.Validation.CheckAirFreightCostsExists();
				AssertNoRowMessageError("Not AIR", invoiceLine, MissingAirFreightCostsRowMessageError);

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				ClearInvoiceLineRowNotificationsAndValidate(() => invoiceLine.Validation.CheckAirFreightCostsExists());
				AssertHasRowMessageError("AIR", invoiceLine, MissingAirFreightCostsRowMessageError);
			});
		}

		public void TestCheckAirFreightCostsExists_ZG_AgreedPlaceCode()
		{
			declaration.ZG_IsHighValueOvrd = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			CombineAssertions(() =>
			{
				invoiceLine.Validation.CheckAirFreightCostsExists();
				AssertNoRowMessageError("Not AIR", invoiceLine, MissingAirFreightCostsRowMessageError);

				invoiceHeader.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._1;
				ClearInvoiceLineRowNotificationsAndValidate(() => invoiceLine.Validation.CheckAirFreightCostsExists());
				AssertHasRowMessageError("AIR", invoiceLine, MissingAirFreightCostsRowMessageError);
			});
		}

		public void TestCheckAirFreightCostsExists_Charges()
		{
			declaration.ZG_IsHighValueOvrd = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			invoiceHeader.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._1;

			CombineAssertions(() =>
			{
				invoiceLine.Validation.CheckAirFreightCostsExists();
				AssertHasRowMessageError("No Charges", invoiceLine, MissingAirFreightCostsRowMessageError);

				var charge = invoiceLine.Charges.AddNew();
				charge.J7_ChargeType = ImportChargeCodeList.Codes.INP;
				ClearInvoiceLineRowNotificationsAndValidate(() => invoiceLine.Validation.CheckAirFreightCostsExists());
				AssertHasRowMessageError("No charge for AirFreightCosts'", invoiceLine, MissingAirFreightCostsRowMessageError);

				charge.J7_ChargeType = ImportChargeCodeList.Codes._010;
				ClearInvoiceLineRowNotificationsAndValidate(() => invoiceLine.Validation.CheckAirFreightCostsExists());
				AssertNoRowMessageError("Has charge for AirFreightCosts'", invoiceLine, MissingAirFreightCostsRowMessageError);
			});
		}

		public void TestCheckAirFreightCostsExists_ApportionedCharges()
		{
			declaration.ZG_IsHighValueOvrd = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			invoiceHeader.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._1;

			CombineAssertions(() =>
			{
				invoiceLine.Validation.CheckAirFreightCostsExists();
				AssertHasRowMessageError("No ApportionedCharges", invoiceLine, MissingAirFreightCostsRowMessageError);

				var apportionedCharge = invoiceLine.ApportionedCharges.AddNew();
				apportionedCharge.J7_ChargeType = ImportChargeCodeList.Codes.INP;
				ClearInvoiceLineRowNotificationsAndValidate(() => invoiceLine.Validation.CheckAirFreightCostsExists());
				AssertHasRowMessageError("No ApportionedCharges for AirFreightCosts'", invoiceLine, MissingAirFreightCostsRowMessageError);

				apportionedCharge.J7_ChargeType = ImportChargeCodeList.Codes._014;
				ClearInvoiceLineRowNotificationsAndValidate(() => invoiceLine.Validation.CheckAirFreightCostsExists());
				AssertNoRowMessageError("Has ApportionedCharges for AirFreightCosts'", invoiceLine, MissingAirFreightCostsRowMessageError);
			});
		}

		public void TestValidateForRowNotification_PackagingDetails_NotMerged()
		{
			const string errorMessage = "This line has no packaging details";

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2_1 = invoiceHeader2.InvoiceLines.AddNew();
			var invoiceLine2_2 = invoiceHeader2.InvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateJI_Tariff();
				//JI_LinNo = 1, no PackingDetails
				AssertHasRowMessageErrorContaining(invoiceLine, errorMessage);

				invoiceLine2.Validation.ValidateJI_Tariff();
				//JI_LinNo = 2, no PackingDetails
				AssertNoRowMessageErrorContaining(invoiceLine2, errorMessage);

				invoiceLine2_1.Validation.ValidateJI_Tariff();
				//JI_LinNo = 1, no PackingDetails
				AssertHasRowMessageErrorContaining(invoiceLine2_1, errorMessage);

				invoiceLine2_2.Validation.ValidateJI_Tariff();
				//JI_LinNo = 2, no PackingDetails
				AssertNoRowMessageErrorContaining(invoiceLine2_2, errorMessage);

				declaration.Packages.AddNew();
				var baseCusLinkPackage1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().First();
				baseCusLinkPackage1.IsLinked = true;
				declaration.Packages.AddNew();
				var baseCusLinkPackage2 = invoiceLine2_1.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().Skip(1).First();
				baseCusLinkPackage2.IsLinked = true;
				invoiceLine.Validation.ValidateJI_Tariff();
				//JI_LinNo = 1, has PackingDetails
				AssertNoRowMessageErrorContaining(invoiceLine, errorMessage);

				invoiceLine2_1.Validation.ValidateJI_Tariff();
				//JI_LinNo = 1, has PackingDetails
				AssertNoRowMessageErrorContaining(invoiceLine2_1, errorMessage);
			});
		}

		public void TestValidateRowNotification_PackagingDetails_InwardProcessingAVABR()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			const string errorMessage = "This line has no packaging details";

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			instruction.CEI_JE = declaration.PK;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoRowMessageErrorContaining(invoiceLine, errorMessage);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasRowMessageErrorContaining(invoiceLine, errorMessage);
		}

		public void TestValidateForRowNotification_PackagingDetails_Merged()
		{
			const string errorMessage = "This line has no packaging details";

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2_1 = invoiceHeader2.InvoiceLines.AddNew();
			var invoiceLine2_2 = invoiceHeader2.InvoiceLines.AddNew();
			var invoiceLine2_3 = invoiceHeader2.InvoiceLines.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "10";

			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			invoiceLine.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine1.PK;
			invoiceLine2_3.JI_CL = entryLine1.PK;

			invoiceLine3.JI_CL = entryLine2.PK;
			invoiceLine2_1.JI_CL = entryLine2.PK;
			invoiceLine2_2.JI_CL = entryLine2.PK;

			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateJI_Tariff();
				//JI_Calc_MergedLineNumber = 1, All InvoiceLines merged into entryLine #1 have no PackingDetails
				AssertHasRowMessageErrorContaining(invoiceLine, errorMessage);

				invoiceLine2.Validation.ValidateJI_Tariff();
				//JI_Calc_MergedLineNumber = 1, All InvoiceLines merged into entryLine #1 have no PackingDetails
				AssertHasRowMessageErrorContaining(invoiceLine2, errorMessage);

				invoiceLine2_3.Validation.ValidateJI_Tariff();
				//JI_Calc_MergedLineNumber = 1, All InvoiceLines merged into entryLine #1 have no PackingDetails
				AssertHasRowMessageErrorContaining(invoiceLine2_3, errorMessage);

				invoiceLine3.Validation.ValidateJI_Tariff();
				//JI_Calc_MergedLineNumber = 2, All InvoiceLines merged into entryLine #2 have no PackingDetails
				AssertNoRowMessageErrorContaining(invoiceLine3, errorMessage);

				invoiceLine2_1.Validation.ValidateJI_Tariff();
				//JI_Calc_MergedLineNumber = 2, All InvoiceLines merged into entryLine #2 have no PackingDetails
				AssertNoRowMessageErrorContaining(invoiceLine2_1, errorMessage);

				invoiceLine2_2.Validation.ValidateJI_Tariff();
				//JI_Calc_MergedLineNumber = 2, All InvoiceLines merged into entryLine #2 have no PackingDetails
				AssertNoRowMessageErrorContaining(invoiceLine2_2, errorMessage);

				declaration.Packages.AddNew();
				var baseCusLinkPackage = invoiceLine2_3.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().Single();
				baseCusLinkPackage.IsLinked = true;
				invoiceLine.Validation.ValidateJI_Tariff();
				//JI_Calc_MergedLineNumber = 1, At least one InvoiceLine merged into entryLine #1 has PackingDetails
				AssertNoRowMessageErrorContaining(invoiceLine, errorMessage);

				invoiceLine2.Validation.ValidateJI_Tariff();
				//JI_Calc_MergedLineNumber = 1, At least one InvoiceLine merged into entryLine #1 has PackingDetails
				AssertNoRowMessageErrorContaining(invoiceLine2, errorMessage);

				invoiceLine2_3.Validation.ValidateJI_Tariff();
				//JI_Calc_MergedLineNumber = 1, At least one InvoiceLine merged into entryLine #1 has PackingDetails
				AssertNoRowMessageErrorContaining(invoiceLine2_3, errorMessage);
			});
		}

		public void TestCheckSpecialCasesExists_CEI_Style()
		{
			invoiceLine.JI_Procedure = "40545F1";
			CombineAssertions(() =>
			{
				invoiceLine.Validation.CheckSpecialCasesExists();
				AssertNoRowMessageError("Not EZA", invoiceLine, MissingSpecialCasesRowMessageError);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZA;
				ClearInvoiceLineRowNotificationsAndValidate(() => invoiceLine.Validation.CheckSpecialCasesExists());
				AssertHasRowMessageError("EZA, no special case", invoiceLine, MissingSpecialCasesRowMessageError);
			});
		}

		public void TestCheckSpecialCasesExists_JI_Procedure()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZA;
			CombineAssertions(() =>
			{
				invoiceLine.Validation.CheckSpecialCasesExists();
				AssertNoRowMessageError("Not '4054' or '4254'", invoiceLine, MissingSpecialCasesRowMessageError);

				invoiceLine.JI_Procedure = "40545F1";
				ClearInvoiceLineRowNotificationsAndValidate(() => invoiceLine.Validation.CheckSpecialCasesExists());
				AssertHasRowMessageError("Procedure '4054'", invoiceLine, MissingSpecialCasesRowMessageError);

				invoiceLine.JI_Procedure = "4254F06";
				ClearInvoiceLineRowNotificationsAndValidate(() => invoiceLine.Validation.CheckSpecialCasesExists());
				AssertHasRowMessageError("Procedure '4254'", invoiceLine, MissingSpecialCasesRowMessageError);
			});
		}

		public void TestCheckSpecialCasesExists()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZA;
			invoiceLine.JI_Procedure = "40545F1";
			CombineAssertions(() =>
			{
				invoiceLine.Validation.CheckSpecialCasesExists();
				AssertHasRowMessageError("no special case", invoiceLine, MissingSpecialCasesRowMessageError);

				var specialCase = invoiceLine.Taxes.AddNew();
				specialCase.JLT_Type = SpecialCaseGroupList.Codes._01;
				ClearInvoiceLineRowNotificationsAndValidate(() => invoiceLine.Validation.CheckSpecialCasesExists());
				AssertNoRowMessageError("specialCase with group '01' exists and no Message Error", invoiceLine, MissingSpecialCasesRowMessageError);

				specialCase.JLT_Type = SpecialCaseGroupList.Codes._37;
				ClearInvoiceLineRowNotificationsAndValidate(() => invoiceLine.Validation.CheckSpecialCasesExists());
				AssertHasRowMessageError("specialCase with group '37' exists and Message Error", invoiceLine, MissingSpecialCasesRowMessageError);
			});
		}

		public void TestCheckJI_CustomsFourthQuantity_NumberOfItems()
		{
			AssertCustomsFourthQuantityRequiresInteger(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems);
		}

		public void TestCheckJI_CustomsFourthQuantity_NumberOfCells()
		{
			AssertCustomsFourthQuantityRequiresInteger(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfCells);
		}

		public void TestCheckJI_CustomsFourthQuantity_NumberOfPairs()
		{
			AssertCustomsFourthQuantityRequiresInteger(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfPairs);
		}

		public void TestCheckJI_CustomsFourthQuantity_DecimalType()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_CustomsFourthUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfPairs;
				invoiceLine.JI_CustomsFourthQuantity = 123456789.123m;
				AssertHasMessageError("Integer Type", invoiceLine.JI_CustomsFourthQuantityInfo, FourthQtyIntegerMessageError);
				invoiceLine.JI_CustomsFourthUnitQty = "ABC";
				invoiceLine.Validation.ValidateJI_CustomsFourthQuantity();
				AssertNoMessageError("Decimal Type", invoiceLine.JI_CustomsFourthQuantityInfo, FourthQtyIntegerMessageError);
			});
		}

		public void TestCheckJI_CustomsFourthQuantityIsValidZDecimal()
		{
			const string expectedErrorMessage = "is too large, the maximum value allowed for";
			CombineAssertions(() =>
			{
				invoiceLine.JI_CustomsFourthQuantity = 1234567890123.1234m;
				AssertHasErrorContaining("Large", invoiceLine.JI_CustomsFourthQuantityInfo, expectedErrorMessage);
				invoiceLine.JI_CustomsFourthQuantity = 123456789.123m;
				AssertNoErrorContaining("Acceptable", invoiceLine.JI_CustomsFourthQuantityInfo, expectedErrorMessage);
			});
		}

		public void TestCheckJI_CustomsFourthUnitQty_Mandatory()
		{
			invoiceLine.JI_CustomsFourthQuantity = 1;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_CustomsFourthUnitQtyInfo);
		}

		public void TestCheckJI_CustomsFourthUnitQty_List()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CustomsFourthUnitQtyInfo, "XXX", "ABC");
		}

		public void TestCheckInwardProcessedProducts()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EAV;
			entryInstruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
			CombineAssertions(() =>
			{
				invoiceLine.Validation.CheckInwardProcessingProducts();
				AssertNoRowMessageError("CEI_SimplifiedGrantAuthorization isn't 'J'", invoiceLine, MissingInwardProcessedProductsRowMessageError);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZA;
				entryInstruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
				invoiceLine.Validation.CheckInwardProcessingProducts();
				AssertNoRowMessageError("EnabledInwardProcessing is false", invoiceLine, MissingInwardProcessedProductsRowMessageError);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EAV;
				entryInstruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
				invoiceLine.Validation.CheckInwardProcessingProducts();
				AssertHasRowMessageError("CEI_SimplifiedGrantAuthorization is 'J'", invoiceLine, MissingInwardProcessedProductsRowMessageError);

				invoiceLine.InwardProcessingProducts.AddNew();
				ClearInvoiceLineRowNotificationsAndValidate(() => invoiceLine.Validation.CheckInwardProcessingProducts());
				AssertNoRowMessageError("Has Processed Product", invoiceLine, MissingInwardProcessedProductsRowMessageError);
			});
		}

		public void TestCheckJI_BondedWHSOrderLineNumber()
		{
			string message = "You have not entered a Warehouse Order Line.";
			CombineAssertions(() =>
			{
				invoiceLine.JI_BondedWHSOrderNumber = "12345";
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_BondedWHSOrderLineNumberInfo, message);

				invoiceLine.JI_BondedWHSOrderNumber = ZString.Empty;
				ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_BondedWHSOrderLineNumberInfo, message);
			});
		}

		public void TestValidateAll()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZA;
			declaration.ZG_IsHighValueOvrd = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			invoiceHeader.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._1;
			invoiceLine.JI_Procedure = "40545F01";
			invoiceLine.Validation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertHasRowMessageError("CheckSpecialCasesExists", invoiceLine, MissingSpecialCasesRowMessageError);
				AssertHasRowMessageError("CheckAirFreightCostsExists", invoiceLine, MissingAirFreightCostsRowMessageError);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EAV;
				entryInstruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
				invoiceLine.Validation.ValidateAll();
				AssertHasRowMessageError("CheckInwardProcessingProducts", invoiceLine, MissingInwardProcessedProductsRowMessageError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var procedure = Factory.NewWithValidTestData<RefCusProcedure>();
			isOutOfWarehouseWarehousingProcedureCode = "4071";
			Factory.NewWithValidTestData<RefDataGrouping>().ZZZ_DataGrouping = "DE";
			procedure.ZZ6_ZZZ_NKDataGrouping = "DE";
			procedure.ZZ6_ShipmentType = "IMP";
			procedure.ZZ6_ProcedureCode = isOutOfWarehouseWarehousingProcedureCode.Left(2);
			procedure.ZZ6_Concession = isOutOfWarehouseWarehousingProcedureCode.PadRight(7).Right(3);
			procedure.ZZ6_OutOfWarehouse = "Y";
			procedure.ZZ6_PreviousProcedureCode = "71";
			Factory.Save();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryInstruction entryInstruction;
		ZString isOutOfWarehouseWarehousingProcedureCode;

		const string MissingAirFreightCostsRowMessageError = "You have not entered Air Freight Costs with charge code '010/014' – Or use 'Calculate Freight' Button on Invoice Header Level.";

		const string MissingSpecialCasesRowMessageError = "The CPC of this Invoice Line requires Special Cases with Group '01'-'12' or Group '20'.";

		const string FourthQtyIntegerMessageError = "Only integer values are allowed for this Fourth Qty Unit";

		const string MissingInwardProcessedProductsRowMessageError = "You should enter at least one Processed Product on the 'Inward Processing' tab.";

		void ClearInvoiceLineRowNotificationsAndValidate(Action validationMethodToRun)
		{
			invoiceLine.ClearRowNotifications();
			validationMethodToRun.Invoke();
		}

		void AssertCustomsFourthQuantityRequiresInteger(ZString unitQty)
		{
			invoiceLine.JI_CustomsFourthUnitQty = unitQty;
			CombineAssertions(() =>
			{
				invoiceLine.JI_CustomsFourthQuantity = 123456789.123m;
				AssertHasMessageError("Decimal " + unitQty, invoiceLine.JI_CustomsFourthQuantityInfo, FourthQtyIntegerMessageError);

				invoiceLine.JI_CustomsFourthQuantity = 123456789m;
				AssertNoMessageError("Integer " + unitQty, invoiceLine.JI_CustomsFourthQuantityInfo, FourthQtyIntegerMessageError);
			});
		}
	}
}
