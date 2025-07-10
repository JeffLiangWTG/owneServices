using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.KR.Messaging.Constants;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class EXPJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJI_Tariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "8429521022", new ZDateTime(2015, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "Test Tariff");

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_AdValoremTariff = "8429521022";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Tariff = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff may not be empty");

			invoiceLine.JI_Tariff = "8429521021";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "The Tariff Code entered is not valid for the current context.");

			invoiceLine.JI_Tariff = "8429521022";
			AssertNoMessageErrors(invoiceLine.JI_TariffInfo);
		}

		public void TestCheckJI_Model()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Model = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Model = "Test_Model";
			AssertNoMessageErrorContaining(invoiceLine.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_BrandName()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_BrandName = "";
			AssertNoMessageErrorContaining(invoiceLine.JI_BrandNameInfo, "Brand Name should only contain alphanumeric characters.");

			invoiceLine.JI_BrandName = "CHRISTIAN DIOR";
			AssertHasMessageErrorContaining(invoiceLine.JI_BrandNameInfo, "Brand Name should only contain alphanumeric characters.");

			invoiceLine.JI_BrandName = @"CHRIST
IANDIOR";
			AssertHasMessageErrorContaining(invoiceLine.JI_BrandNameInfo, "Brand Name should only contain alphanumeric characters.");

			invoiceLine.JI_BrandName = "CHRISTIAN\tDIOR";
			AssertHasMessageErrorContaining(invoiceLine.JI_BrandNameInfo, "Brand Name should only contain alphanumeric characters.");

			invoiceLine.JI_BrandName = "CHRISTIAN@DIOR";
			AssertHasMessageErrorContaining(invoiceLine.JI_BrandNameInfo, "Brand Name should only contain alphanumeric characters.");

			invoiceLine.JI_BrandName = "CHRISTIANDIOR";
			AssertNoMessageErrorContaining(invoiceLine.JI_BrandNameInfo, "Brand Name should only contain alphanumeric characters.");

			invoiceLine.JI_BrandName = "christiandior";
			AssertNoMessageErrorContaining(invoiceLine.JI_BrandNameInfo, "Brand Name should only contain alphanumeric characters.");

			invoiceLine.JI_BrandName = "CHRISTIANDIORV1";
			AssertNoMessageErrorContaining(invoiceLine.JI_BrandNameInfo, "Brand Name should only contain alphanumeric characters.");

			invoiceLine.JI_BrandName = "1234567890";
			AssertNoMessageErrorContaining(invoiceLine.JI_BrandNameInfo, "Brand Name should only contain alphanumeric characters.");
		}

		public void TestCheckJI_CountryOfOrigin()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_CountryOfOrigin = "12";
			AssertHasMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_CountryOfOrigin = "KR";
			AssertNoMessageErrors(invoiceLine.JI_CountryOfOriginInfo);
		}

		public void TestCheckJI_NetWeight()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_WeightUQ = "";
			invoiceLine.JI_Weight = 0;
			invoiceLine.JI_NetWeightUQ = "";
			invoiceLine.JI_NetWeight = 0;
			AssertNoErrors(invoiceLine.JI_NetWeightInfo);

			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.JI_Weight = 2;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_NetWeight = 1;
			AssertNoErrors(invoiceLine.JI_NetWeightInfo);

			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.JI_Weight = 2;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_NetWeight = 3;
			AssertHasWarningContaining(invoiceLine.JI_NetWeightInfo, "Net Weight should be less than Gross Weight.");

			invoiceLine.JI_NetWeight = -1;
			AssertHasMessageErrorContaining(invoiceLine.JI_NetWeightInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckJI_CustomsQuantity()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = "";
			invoiceLine.JI_CustomsQuantity = 10;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, "There is no customs unit associated with the tariff.");

			invoiceLine.JI_CustomsUnitQty = "BG";
			invoiceLine.JI_CustomsQuantity = 10;
			AssertNoMessageErrors(invoiceLine.JI_CustomsQuantityInfo);

			invoiceLine.JI_CustomsQuantity = 0;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeZero);

			invoiceLine.JI_CustomsQuantity = -1;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckJI_PreviousEntryNumber()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			declaration.JE_ExportGoodsType = "72";
			invoiceLine.JI_PreviousEntryNumber = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_PreviousEntryNumber = "123456-78-901234";
			AssertNoMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ExportGoodsType = "84";
			invoiceLine.JI_PreviousEntryNumber = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_PreviousEntryNumber = "123456-78-901234";
			AssertNoMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ExportGoodsType = "86";
			invoiceLine.JI_PreviousEntryNumber = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_PreviousEntryNumber = "123456-78-901234";
			AssertNoMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ExportGoodsType = "89";
			invoiceLine.JI_PreviousEntryNumber = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_PreviousEntryNumber = "123456-78-901234";
			AssertNoMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ExportGoodsType = "93";
			invoiceLine.JI_PreviousEntryNumber = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_PreviousEntryNumber = "123456-78-901234";
			AssertNoMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ExportGoodsType = "94";
			invoiceLine.JI_PreviousEntryLineNumber = 0;
			invoiceLine.JI_PreviousEntryNumber = "";
			AssertNoMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			invoiceLine.JI_PreviousEntryNumber = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_PreviousEntryNumber = "123456-78-901234";
			AssertNoMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ExportGoodsType = "100";
			invoiceLine.JI_PreviousEntryNumber = "";
			AssertNoMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, "For the selected transaction type, an import declaration number/entry line number is not relevant.");
			invoiceLine.JI_PreviousEntryNumber = "123456-78-901234";
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, "For the selected transaction type, an import declaration number/entry line number is not relevant.");
		}

		public void TestCheckJI_PreviousEntryLineNumber()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			declaration.JE_ExportGoodsType = "72";
			invoiceLine.JI_PreviousEntryLineNumber = 0;
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryLineNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			AssertNoMessageErrors(invoiceLine.JI_PreviousEntryLineNumberInfo);

			declaration.JE_ExportGoodsType = "84";
			invoiceLine.JI_PreviousEntryLineNumber = 0;
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryLineNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			AssertNoMessageErrors(invoiceLine.JI_PreviousEntryLineNumberInfo);

			declaration.JE_ExportGoodsType = "86";
			invoiceLine.JI_PreviousEntryLineNumber = 0;
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryLineNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			AssertNoMessageErrors(invoiceLine.JI_PreviousEntryLineNumberInfo);

			declaration.JE_ExportGoodsType = "89";
			invoiceLine.JI_PreviousEntryLineNumber = 0;
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryLineNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			AssertNoMessageErrors(invoiceLine.JI_PreviousEntryLineNumberInfo);

			declaration.JE_ExportGoodsType = "93";
			invoiceLine.JI_PreviousEntryLineNumber = 0;
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryLineNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			AssertNoMessageErrors(invoiceLine.JI_PreviousEntryLineNumberInfo);

			declaration.JE_ExportGoodsType = "94";
			invoiceLine.JI_PreviousEntryNumber = "";
			invoiceLine.JI_PreviousEntryLineNumber = 0;
			AssertNoMessageErrors(invoiceLine.JI_PreviousEntryLineNumberInfo);
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryLineNumberInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.JI_PreviousEntryNumber = "123456-78-901234";
			invoiceLine.JI_PreviousEntryLineNumber = 0;
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryLineNumberInfo, MandatoryValidation.ValueCannotBeZero);
			invoiceLine.JI_PreviousEntryLineNumber = -1;
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryLineNumberInfo, MandatoryValidation.ValueCannotBeNegative);
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			AssertNoMessageErrors(invoiceLine.JI_PreviousEntryLineNumberInfo);

			declaration.JE_ExportGoodsType = "100";
			invoiceLine.JI_PreviousEntryLineNumber = 0;
			AssertNoMessageErrors(invoiceLine.JI_PreviousEntryLineNumberInfo);
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryLineNumberInfo, "For the selected transaction type, an import declaration number/entry line number is not relevant.");
		}

		public void TestCheckJI_Calc_FOB()
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var validation = (EXPJobComInvoiceLineValidation)invoiceLine.Validation;

			invoiceLine.JI_LinePrice = -1m;
			AssertEquals(-1m, invoiceLine.JI_Calc_FOB);
			validation.ValidateJI_Calc_FOB();
			AssertHasMessageErrorContaining(invoiceLine.JI_Calc_FOBInfo, "The FOB value is calculated as a negative value. Please check Line Price, Currency and its Exchange Rate and deduction charges.");

			invoiceLine.JI_LinePrice = 1m;
			AssertEquals(1m, invoiceLine.JI_Calc_FOB);
			validation.ValidateJI_Calc_FOB();
			AssertNoMessageErrors(invoiceLine.JI_Calc_FOBInfo);
		}

		public void TestCheckUnitPrice()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.UnitPrice = -1m;
			invoiceLine.Validation.ValidateUnitPrice();
			AssertHasMessageErrorContaining(invoiceLine.UnitPriceInfo, "Please enter a 'Unit Price' greater than or equal to 0.");

			invoiceLine.UnitPrice = ZDecimal.Zero;
			invoiceLine.Validation.ValidateUnitPrice();
			AssertNoMessageErrors(invoiceLine.UnitPriceInfo);
		}

		public void TestCheckAmount()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = -1m;
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, "Please enter a 'Price' greater than or equal to 0.");

			invoiceLine.JI_LinePrice = ZDecimal.Zero;
			AssertNoMessageErrors(invoiceLine.JI_LinePriceInfo);
		}

		public void TestCheckFTAType()
		{
			SetCusCodeList();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PrimaryPreference = "123";
			AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_PrimaryPreference = "101";
			AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Chile;
			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.JI_PrimaryPreferenceInfo);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.KoreaSouth;
			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.JI_PrimaryPreferenceInfo);

			invoiceLine.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.Y;
			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_PrimaryPreference = "123";
			AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_PrimaryPreference = "101";
			AssertNoMessageErrors(invoiceLine.JI_PrimaryPreferenceInfo);

			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Singapore;
			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.KoreaNorth;
			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.JI_PrimaryPreferenceInfo);
		}

		public void TestCheckCertificateOfOriginIssueStatus()
		{
			SetCusCodeList();

			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.E;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.KoreaSouth;

			var validation = (EXPJobComInvoiceLineValidation)invoiceLine.Validation;
			validation.ValidateCertificateOfOriginIssueStatus();
			AssertNoMessageErrors(invoiceLine.CertificateOfOriginIssueStatusInfo);

			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.M;
			validation.ValidateCertificateOfOriginIssueStatus();
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginIssueStatusInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.CertificateOfOriginIssueStatus = "B";
			AssertNoMessageErrorContaining(invoiceLine.CertificateOfOriginIssueStatusInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginIssueStatusInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.CertificateOfOriginIssueStatus = "";
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginIssueStatusInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(invoiceLine.CertificateOfOriginIssueStatusInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.Y;
			AssertNoMessageErrors(invoiceLine.CertificateOfOriginIssueStatusInfo);

			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Afghanistan;
			invoiceLine.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.Y;
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginIssueStatusInfo, "It can be ‘Y’ only for a destination country with which Country of Origin Korea has an FTA relationship.");

			invoiceLine.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.N;
			AssertNoMessageErrors(invoiceLine.CertificateOfOriginIssueStatusInfo);

			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Chile;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.Y;
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginIssueStatusInfo, "It can be ‘Y’ only for a destination country with which Country of Origin Korea has an FTA relationship.");

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.KoreaSouth;
			validation.ValidateCertificateOfOriginIssueStatus();
			AssertNoMessageErrors(invoiceLine.CertificateOfOriginIssueStatusInfo);

			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Brunei;
			invoiceLine.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.Y;
			AssertNoMessageErrors(invoiceLine.CertificateOfOriginIssueStatusInfo);

			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.KoreaNorth;
			invoiceLine.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.Y;
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginIssueStatusInfo, "It can be ‘Y’ only for a destination country with which Country of Origin Korea has an FTA relationship.");

			invoiceLine.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.N;
			AssertNoMessageErrors(invoiceLine.CertificateOfOriginIssueStatusInfo);
		}

		void SetCusCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tradeGroup1 = helper.CreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.CountryCodes.Chile, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.Chile);
			var codeListAndAttributeNames1 = helper.CreateCusCodeListWithAttributeNames(Core.Constants.CountryCodes.KoreaSouth,
												KR.Messaging.Constants.ZZ.NKCodeType.EXFTA, "101", "한-칠레",
												ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1),
												new[] { new KeyValuePair<string, string>(KR.Messaging.Constants.ZZ.CodeListAttributeNames.FTATradeGroup, Core.Constants.CountryCodes.Chile) });
			helper.CreateCusCodeListAttribute(codeListAndAttributeNames1.PK, KR.Messaging.Constants.ZZ.CodeListAttributeNames.FTATradeGroup, Core.Constants.CountryCodes.Chile);

			var tradeGroup2 = helper.CreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.CountryCodes.Singapore, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.AddCountry(tradeGroup2, Core.Constants.CountryCodes.Singapore);
			var codeListAndAttributeNames2 = helper.CreateCusCodeListWithAttributeNames(Core.Constants.CountryCodes.KoreaSouth,
												KR.Messaging.Constants.ZZ.NKCodeType.EXFTA, "102", "한-싱가포르",
												ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1),
												new[] { new KeyValuePair<string, string>(KR.Messaging.Constants.ZZ.CodeListAttributeNames.FTATradeGroup, Core.Constants.CountryCodes.Singapore) });
			helper.CreateCusCodeListAttribute(codeListAndAttributeNames2.PK, KR.Messaging.Constants.ZZ.CodeListAttributeNames.FTATradeGroup, Core.Constants.CountryCodes.Singapore);

			var tradeGroup3 = helper.CreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, "ASEAN", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.AddCountry(tradeGroup3, Core.Constants.CountryCodes.Brunei);
			var codeListAndAttributeNames3 = helper.CreateCusCodeListWithAttributeNames(Core.Constants.CountryCodes.KoreaSouth,
												KR.Messaging.Constants.ZZ.NKCodeType.EXFTA, "104", "한-아세안",
												ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1),
												new[] { new KeyValuePair<string, string>(KR.Messaging.Constants.ZZ.CodeListAttributeNames.FTATradeGroup, "ASEAN") });
			helper.CreateCusCodeListAttribute(codeListAndAttributeNames3.PK, KR.Messaging.Constants.ZZ.CodeListAttributeNames.FTATradeGroup, "ASEAN");

			var tradeGroup4 = helper.CreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.CountryCodes.KoreaNorth, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.AddCountry(tradeGroup4, Core.Constants.CountryCodes.KoreaNorth);
			var codeListAndAttributeNames4 = helper.CreateCusCodeListWithAttributeNames(Core.Constants.CountryCodes.KoreaSouth,
												"EXTPF", "201", "남북교역",
												ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1),
												new[] { new KeyValuePair<string, string>(KR.Messaging.Constants.ZZ.CodeListAttributeNames.TradePreferenceTradeGroup, Core.Constants.CountryCodes.KoreaNorth) });
			helper.CreateCusCodeListAttribute(codeListAndAttributeNames4.PK, KR.Messaging.Constants.ZZ.CodeListAttributeNames.TradePreferenceTradeGroup, Core.Constants.CountryCodes.KoreaNorth);

			var tradeGroup5 = helper.CreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.CountryCodes.Afghanistan, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.AddCountry(tradeGroup5, Core.Constants.CountryCodes.Afghanistan);
			Factory.Save();
		}

		public void TestCheckJI_LineNo()
		{
			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.M;
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.Validation.ValidateAll();

			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.ContainersPivot.AddNew();
			invoiceLine2.Validation.ValidateAll();
			AssertNoMessageErrors(invoiceLine1.JI_LineNoInfo);
			AssertNoMessageErrors(invoiceLine2.JI_LineNoInfo);

			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
			invoiceLine1.Validation.ValidateAll();
			invoiceLine2.Validation.ValidateAll();
			AssertHasMessageErrorContaining(invoiceLine1.JI_LineNoInfo, "If the 'Declaration Type' is 'B', at least one container must be checked.");
			AssertNoMessageErrors(invoiceLine2.JI_LineNoInfo);
		}

		public void TestCheckJI_InvoiceQuantity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "2402200000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateTariffAttribute(ZZ.TariffAttributes.InvoiceQuantityInCU1, YesNo.Yes, tariff);
			Factory.Save();

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, MandatoryValidation.ValueCannotBeZero);

			invoiceLine.JI_InvoiceQuantity = -1;
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_InvoiceQuantity = 0;
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, MandatoryValidation.ValueCannotBeZero);

			invoiceLine.JI_InvoiceQuantity = -1;
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.JI_Tariff = "2402200000";
			invoiceLine.JI_InvoiceQuantity = 0;
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, MandatoryValidation.ValueCannotBeZero);

			invoiceLine.JI_InvoiceQuantity = -1;
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.VehicleNumbers.AddNew();
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, "If there are vehicles in the invoice line, The quantity must be equal to vehicles count.");

			invoiceLine.JI_InvoiceQuantity = 1;
			AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, "If there are vehicles in the invoice line, The quantity must be equal to vehicles count.");
		}

		public void TestCheckJI_InvoiceUQ()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "2402200000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateTariffAttribute(ZZ.TariffAttributes.InvoiceQuantityInCU1, YesNo.Yes, tariff);
			Factory.Save();

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Tariff = "2402200000";
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_InvoiceUQ = "XX";
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_InvoiceUQ = "U";
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertNoMessageErrors(invoiceLine.JI_InvoiceUQInfo);
		}

		public void TestCheckPRA_ReferenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CountryOfOrigin = "KR";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CountryOfOrigin = "US";
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_CountryOfOrigin = "AU";
			Factory.Save();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(3, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			invoiceLine1.PRA_ReferenceNumber = ZString.Empty;
			invoiceLine2.PRA_ReferenceNumber = ZString.Empty;
			invoiceLine3.PRA_ReferenceNumber = ZString.Empty;
			AssertNoMessageErrors(invoiceLine1.PRA_ReferenceNumberInfo);
			AssertNoMessageErrors(invoiceLine2.PRA_ReferenceNumberInfo);
			AssertNoMessageErrors(invoiceLine3.PRA_ReferenceNumberInfo);

			invoiceLine1.PRA_ReferenceNumber = "철강수출번호1";
			invoiceLine2.PRA_ReferenceNumber = "철강수출번호1";
			invoiceLine3.PRA_ReferenceNumber = "철강수출번호1";
			AssertNoMessageErrorContaining(invoiceLine1.PRA_ReferenceNumberInfo, "The same Steel Pre-Approval number cannot be applied to different entry lines. You need to get another pre-approval number from the Korean Steel Association.");
			AssertHasMessageErrorContaining(invoiceLine2.PRA_ReferenceNumberInfo, "The same Steel Pre-Approval number cannot be applied to different entry lines. You need to get another pre-approval number from the Korean Steel Association.");
			AssertHasMessageErrorContaining(invoiceLine3.PRA_ReferenceNumberInfo, "The same Steel Pre-Approval number cannot be applied to different entry lines. You need to get another pre-approval number from the Korean Steel Association.");

			invoiceLine2.PRA_ReferenceNumber = "철강수출번호2";
			invoiceLine3.PRA_ReferenceNumber = "철강수출번호3";
			AssertNoMessageErrorContaining(invoiceLine1.PRA_ReferenceNumberInfo, "The same Steel Pre-Approval number cannot be applied to different entry lines. You need to get another pre-approval number from the Korean Steel Association.");
			AssertNoMessageErrorContaining(invoiceLine2.PRA_ReferenceNumberInfo, "The same Steel Pre-Approval number cannot be applied to different entry lines. You need to get another pre-approval number from the Korean Steel Association.");
			AssertNoMessageErrorContaining(invoiceLine3.PRA_ReferenceNumberInfo, "The same Steel Pre-Approval number cannot be applied to different entry lines. You need to get another pre-approval number from the Korean Steel Association.");
		}

		public void TestPRA_ReferenceNumberMandatory()
		{
			var conditionHelper = new TestRefConditionSetupHelper(Factory);
			var tariffHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = tariffHelper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, TariffTypes.HarmonizedSystem);

			tariffHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "7207190001", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			tariffHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "6344650002", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			tariffHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0012340001", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			conditionHelper.Setup();
			var tradeGroup = conditionHelper.GenerateTradeGroup(Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			conditionHelper.GenerateTradeGroupCountry(tradeGroup, Core.Constants.CountryCodes.UnitedStates, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(1));
			tradeGroup = conditionHelper.GenerateTradeGroup(Core.Constants.CountryCodes.EuropeanUnion, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			conditionHelper.GenerateTradeGroupCountry(tradeGroup, Core.Constants.CountryCodes.Germany, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(1));

			var nomenclature1 = conditionHelper.GenerateNomenclature("7207", "12.34..56.7.8", ZDateTime.Today.AddDays(-2), ZDateTime.MaxSmallDateTime);
			var nomenclature2 = conditionHelper.GenerateNomenclature("634465", "12.63..44.6.5", ZDateTime.Today.AddDays(-2), ZDateTime.MaxSmallDateTime);
			var nomenclature3 = conditionHelper.GenerateNomenclature("123456789", "12.23..34.5.6", ZDateTime.Today.AddDays(-2), ZDateTime.MaxSmallDateTime);

			conditionHelper.SetCondition(nomenclature1, "US", ZDateTime.Today.AddDays(-2), ZDateTime.MaxSmallDateTime);
			conditionHelper.SetCondition(nomenclature2, "US", ZDateTime.Today.AddDays(-2), ZDateTime.MaxSmallDateTime);
			conditionHelper.SetCondition(nomenclature2, "EU", ZDateTime.Today.AddDays(-2), ZDateTime.MaxSmallDateTime);
			conditionHelper.SetCondition(nomenclature3, "GB", ZDateTime.Today.AddDays(-2), ZDateTime.MaxSmallDateTime);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "0012340001";
			declaration.JE_GoodsDestination = "US";
			ValidateAll();
			AssertNoErrors();

			invoiceLine.JI_Tariff = "7207190001";
			ValidateAll();
			AssertMessageNotEnteredErrors();

			declaration.JE_GoodsDestination = "DE";
			ValidateAll();
			AssertNoErrors();

			invoiceLine.JI_Tariff = "6344650002";
			ValidateAll();
			AssertMessageNotEnteredErrors();

			invoiceLine.PRA_ReferenceNumber = "KR00101010000";
			invoiceLine.PRA_DateOfIssue = ZDateTime.Today;
			invoiceLine.PRA_DateOfExpiry = ZDateTime.Today;
			AssertNoErrors();

			declaration.JE_GoodsDestination = "JP";
			ValidateAll();
			AssertMessageEnteredErrors();

			invoiceLine.PRA_ReferenceNumber = ZString.Empty;
			invoiceLine.PRA_DateOfIssue = ZDateTime.Empty;
			invoiceLine.PRA_DateOfExpiry = ZDateTime.Empty;
			AssertNoErrors();

			void ValidateAll()
			{
				((EXPJobComInvoiceLineValidation)invoiceLine.Validation).ValidatePRA_ReferenceNumber();
				((EXPJobComInvoiceLineValidation)invoiceLine.Validation).ValidatePRA_DateOfIssue();
				((EXPJobComInvoiceLineValidation)invoiceLine.Validation).ValidatePRA_DateOfExpiry();
			}

			void AssertNoErrors()
			{
				AssertNoMessageErrors(invoiceLine.PRA_ReferenceNumberInfo);
				AssertNoMessageErrors(invoiceLine.PRA_DateOfIssueInfo);
				AssertNoMessageErrors(invoiceLine.PRA_DateOfExpiryInfo);
			}

			void AssertMessageNotEnteredErrors()
			{
				AssertHasMessageErrorContaining(invoiceLine.PRA_ReferenceNumberInfo, "The entered tariff requires a pre-approval from the Korean Steel Association.");
				AssertHasMessageErrorContaining(invoiceLine.PRA_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(invoiceLine.PRA_DateOfExpiryInfo, MandatoryValidation.YouHaveNotEntered);
			}

			void AssertMessageEnteredErrors()
			{
				AssertHasMessageErrorContaining(invoiceLine.PRA_ReferenceNumberInfo, MandatoryValidation.DoNotEntered);
				AssertHasMessageErrorContaining(invoiceLine.PRA_DateOfIssueInfo, MandatoryValidation.DoNotEntered);
				AssertHasMessageErrorContaining(invoiceLine.PRA_DateOfExpiryInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckNoOfPacks()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_NoOfPacks = -1;
			AssertHasMessageErrorContaining(invoiceLine.JI_NoOfPacksInfo, "Please enter a 'Packages' greater than or equal to 0.");

			invoiceLine.JI_NoOfPacks = ZInt.Zero;
			AssertNoMessageErrors(invoiceLine.JI_NoOfPacksInfo);
		}

		public void TestCheckJI_NoOfPacks()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_PackType = "";
			invoiceLine.JI_NoOfPacks = 10;
			AssertHasMessageErrorContaining(invoiceLine.JI_NoOfPacksInfo, "Pack Qty should be zero When Pack Type is not entered.");

			invoiceLine.JI_NoOfPacks = 0;
			AssertNoMessageErrorContaining(invoiceLine.JI_NoOfPacksInfo, "Pack Qty should be zero When Pack Type is not entered.");

			invoiceLine.JI_NoOfPacks = -1;
			AssertHasMessageErrorContaining(invoiceLine.JI_NoOfPacksInfo, "Pack Qty should be zero When Pack Type is not entered.");
			AssertHasMessageErrorContaining(invoiceLine.JI_NoOfPacksInfo, "Please enter a 'Packages' greater than or equal to 0.");

			invoiceLine.JI_PackType = "BG";
			invoiceLine.JI_NoOfPacks = -10;
			AssertHasMessageErrorContaining(invoiceLine.JI_NoOfPacksInfo, "Please enter a 'Packages' greater than or equal to 0.");
			AssertNoMessageErrorContaining(invoiceLine.JI_NoOfPacksInfo, "Pack Qty should be zero When Pack Type is not entered.");

			invoiceLine.JI_NoOfPacks = 0;
			AssertNoMessageErrors(invoiceLine.JI_NoOfPacksInfo);

			invoiceLine.JI_NoOfPacks = 10;
			AssertNoMessageErrors(invoiceLine.JI_NoOfPacksInfo);
		}

		public void TestCheckJI_PackType()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_NoOfPacks = 10;
			invoiceLine.JI_PackType = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_PackTypeInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_PackType = "12";
			AssertHasMessageErrorContaining(invoiceLine.JI_PackTypeInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_PackType = "BG";
			AssertNoMessageErrorContaining(invoiceLine.JI_PackTypeInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_NoOfPacks = 0;
			invoiceLine.JI_PackType = "BG";
			AssertHasMessageErrorContaining(invoiceLine.JI_PackTypeInfo, "Pack type should not be entered if Pack Qty is zero.");

			invoiceLine.JI_PackType = "";
			AssertNoMessageErrorContaining(invoiceLine.JI_PackTypeInfo, "Pack type should not be entered if Pack Qty is zero.");
		}

		public void TestCehckJI_SkipManifestReport()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "8608------";
			invoiceLine.JI_SkipManifestReport = "";
			AssertNoMessageErrorContaining(invoiceLine.JI_SkipManifestReportInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.JI_SkipManifestReport = "Y";
			AssertHasMessageErrorContaining(invoiceLine.JI_SkipManifestReportInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.JI_SkipManifestReport = "N";
			AssertHasMessageErrorContaining(invoiceLine.JI_SkipManifestReportInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.JI_Tariff = "8609------";
			invoiceLine.JI_SkipManifestReport = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_SkipManifestReportInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_SkipManifestReport = "X";
			AssertHasMessageErrorContaining(invoiceLine.JI_SkipManifestReportInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_SkipManifestReport = "Y";
			AssertNoMessageErrorContaining(invoiceLine.JI_SkipManifestReportInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_SkipManifestReport = "N";
			AssertNoMessageErrorContaining(invoiceLine.JI_SkipManifestReportInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_COOLabelLocation()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_COOLabelLocation = "";
			AssertNoMessageErrors(invoiceLine.JI_COOLabelLocationInfo);

			invoiceLine.JI_COOLabelLocation = "B";
			AssertNoMessageErrors(invoiceLine.JI_COOLabelLocationInfo);

			invoiceLine.JI_COOLabelLocation = "E";
			AssertNoMessageErrors(invoiceLine.JI_COOLabelLocationInfo);

			invoiceLine.JI_COOLabelLocation = "G";
			AssertNoMessageErrors(invoiceLine.JI_COOLabelLocationInfo);

			invoiceLine.JI_COOLabelLocation = "N";
			AssertNoMessageErrors(invoiceLine.JI_COOLabelLocationInfo);

			invoiceLine.JI_COOLabelLocation = "S";
			AssertNoMessageErrors(invoiceLine.JI_COOLabelLocationInfo);

			invoiceLine.JI_COOLabelLocation = "Y";
			AssertNoMessageErrors(invoiceLine.JI_COOLabelLocationInfo);

			invoiceLine.JI_COOLabelLocation = "X";
			AssertHasMessageErrorContaining(invoiceLine.JI_COOLabelLocationInfo, ListValidation.InvalidCodeMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";

			Factory.Save();
		}
		JobDeclaration declaration;
	}
}
