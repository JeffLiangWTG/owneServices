using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(ESDocC10Header))]
	public class ESDocC10HeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertNoExceptionThrown(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				ESDocC10Header.New(entryHeader, declaration);
			});
		}

		public void TestSheetName()
		{
			CombineAssertions(() =>
			{
				AssertEquals("When no mrn and no CH_BGMReference", ZString.Empty, wrapper.SheetName);

				entryHeader.CH_BGMReference = "Reference";
				wrapper = ESDocC10Header.New(entryHeader, declaration);
				AssertEquals("When no mrn and CH_BGMReference is not empty", "Reference", wrapper.SheetName);

				entryHeader.MovementReferenceNumberSetter("MRNCode");
				wrapper = ESDocC10Header.New(entryHeader, declaration);
				AssertEquals("When mrn is not empty", "MRNCode", wrapper.SheetName);
			});
		}

		public void TestTitle()
		{
			AssertEquals("BASE DEL IMPUESTO DEL VALOR AÑADIDO", wrapper.Title);
		}

		public void TestCityDate()
		{
			AssertEquals(" " + ZDateTime.Today.ToCustomsFormatDateStringddMMyyyyWithDash(), wrapper.CityDate);
			var cityAtrribute = customsOffice.Attributes.AddNew();
			cityAtrribute.ZZE_ZXE_NKName = "CITY";
			cityAtrribute.ZZE_Value = "TEST CITY";
			cityAtrribute.ZZE_ZZD_CodeList = customsOffice.PK;
			AssertEquals(false, wrapper.CityDate.Contains("TEST CITY"));
		}

		public void TestDeclarantFullName()
		{
			AssertContains("EDI CUSTOMS BROKERS", wrapper.DeclarantFullName);
			OrgHeader declarant = Factory.New<OrgHeader>();
			declarant.OH_FullName = "Declarant name test";
			OrgAddress declarantAddress = Factory.New<OrgAddress>();
			declarant.Addresses.Add(declarantAddress);
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			AssertContains("Declarant name test", wrapper.DeclarantFullName);
		}

		public void TestUserName()
		{
			AssertEquals(GlbStaff.CurrentUser.GS_FullName, wrapper.UserName);
			GlbStaff.CurrentUser.GS_FullName = "Test User";
			AssertEquals("Test User", wrapper.UserName);
		}

		public void TestReference()
		{
			AssertEquals(false, wrapper.HeaderGrid.Contains("Expediente"));
			entryHeader.CH_BGMReference = "TESTRFERENCE";
			AssertEquals(true, wrapper.HeaderGrid.Contains("TESTRFERENCE"));
		}

		public void TestMRN()
		{
			AssertEquals(false, wrapper.HeaderGrid.Contains("Num. Registro"));
			entryHeader.MovementReferenceNumber = "MRNTEST";
			AssertEquals(true, wrapper.HeaderGrid.Contains("MRNTEST"));
		}

		public void TestOwnerRef()
		{
			AssertEquals(false, wrapper.HeaderGrid.Contains("Referencia"));
			declaration.JE_OwnerRef = "OwnerRefTEST";
			AssertEquals(true, wrapper.HeaderGrid.Contains("OwnerRefTEST"));
		}

		public void TestImporterName()
		{
			AssertEquals(false, wrapper.HeaderGrid.Contains("Importador"));
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "Test Importer";
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals(true, wrapper.HeaderGrid.Contains("Test Importer"));
		}

		public void TestCustomsOffice()
		{
			AssertEquals(true, wrapper.HeaderGrid.Contains("ES137100-MADRID CUSTOM"));
		}

		public void TestTotalPackages()
		{
			AssertEquals(false, wrapper.HeaderGrid.Contains("Número Bultos"));
			var cw = declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			cw.CW_PackQty = 1;
			var pack1 = (Customs.Business.InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(cw);
			pack1.CHC_NumberOfPacks = 5;
			AssertEquals(false, wrapper.HeaderGrid.Contains("Número Bultos"));
			var wrapper2 = ESDocC10Header.New(entryHeader, declaration);
			AssertEquals(true, wrapper2.HeaderGrid.Contains("Número Bultos .... : 5"));
		}

		public void TestCustomsValue()
		{
			AssertEquals(false, wrapper.HeaderGrid.Contains("Valor en Aduana"));
			entryLine.CL_CustomsValue = 50;
			invoiceLine.JI_LinePrice = 20;
			invoiceLine.Charges.AddNew(ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing, 40m, declaration.LocalCurrencyCode);
			invoiceLine.Charges.AddNew(ESCustomsChargeTypeList.Codes.InvoicedExportedGoodsValueForOutwardProcessing, 60m, declaration.LocalCurrencyCode);
			AssertEquals(false, wrapper.HeaderGrid.Contains("Valor en Aduana"));
			var wrapper2 = ESDocC10Header.New(entryHeader, declaration);
			AssertEquals(true, wrapper2.HeaderGrid.Contains("Valor en Aduana.........................            60,00"));
		}

		public void TestTransformedRPP()
		{
			AssertEquals(false, wrapper.HeaderGrid.Contains("Valor Prod. Transformados RPP (7009)"));
			var doc = invoiceLine.SupportingDocuments.AddNew();
			doc.CSI_Code = SupportingDocumentType.TransformedRPP;
			doc.CSI_ReferenceNumber = "40";
			AssertEquals(false, wrapper.HeaderGrid.Contains("Valor Prod. Transformados RPP (7009)"));
			var wrapper2 = ESDocC10Header.New(entryHeader, declaration);
			AssertEquals(true, wrapper2.HeaderGrid.Contains("Valor Prod. Transformados RPP (7009)....           -40,00"));
		}

		public void TestDeductionsValue()
		{
			AssertEquals(false, wrapper.HeaderGrid.Contains("Deducciones (DV1)"));
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_IsDutiable = false;
			charge.J7_IsIncludedInITOT = true;
			charge.J7_IsGSTApplicable = true;
			charge.J7_RX_NKCurrency = "EUR";
			charge.J7_Amount = 25;
			AssertEquals(false, wrapper.HeaderGrid.Contains("Deducciones (DV1)"));
			var wrapper2 = ESDocC10Header.New(entryHeader, declaration);
			AssertEquals(true, wrapper2.HeaderGrid.Contains("Deducciones (DV1).......................            25,00"));
		}

		public void TestAdditionDeducibleValue()
		{
			AssertEquals(false, wrapper.HeaderGrid.Contains("Adiciones (DV1) deducibles"));
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_IsDutiable = true;
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsGSTApplicable = false;
			charge.J7_RX_NKCurrency = "EUR";
			charge.J7_Amount = 26;
			AssertEquals(false, wrapper.HeaderGrid.Contains("Adiciones (DV1) deducibles"));
			var wrapper2 = ESDocC10Header.New(entryHeader, declaration);
			AssertEquals(true, wrapper2.HeaderGrid.Contains("Adiciones (DV1) deducibles..............           -26,00"));
		}

		public void TestREARebateValue()
		{
			CombineAssertions(() =>
			{
				AssertEquals("When no supporting documents with code 7003 are declared REA Rebate line is not shown in document", false, wrapper.HeaderGrid.Contains("Ayuda P"));
				var doc = invoiceLine.SupportingDocuments.AddNew();
				doc.CSI_Code = SupportingDocumentType.REARebate;
				doc.CSI_ReferenceNumber = "1721,69";
				AssertEquals("REA Rebate line is not shown in document because the value is cached", false, wrapper.HeaderGrid.Contains("Ayuda P"));
				invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCode.REA;
				invoiceLine.ZG_REAProductCode = "T001";
				invoiceLine.JI_Tariff = "2208905400";
				invoiceLine.ZG_IsREADirectConsumption = true;
				invoiceLine.JI_CustomsQuantity = 20255.2;
				invoiceLine.JI_CustomsUnitQty = "KGM";
				var wrapper2 = ESDocC10Header.New(entryHeader, declaration);
				AssertEquals("REA Rebate line is shown with all data included", true, wrapper2.HeaderGrid.Contains("Ayuda P1: 85.0000 * [TNE], 20255,20 KN..         -1721,69"));

				var entryLine2 = entryHeader.MergedLines.AddNew();
				entryLine2.CL_LineNumber = 2;
				var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine2.JI_CL = entryLine2.PK;
				invoiceLine2.JI_PrimaryPreference = PrimaryPreferenceCode.REA;
				invoiceLine2.ZG_REAProductCode = "T001";
				invoiceLine2.JI_Tariff = "2208905400";
				invoiceLine2.ZG_IsREADirectConsumption = true;
				invoiceLine2.JI_CustomsQuantity = 255.2;
				invoiceLine2.JI_CustomsUnitQty = "KGM";
				var doc2 = invoiceLine2.SupportingDocuments.AddNew();
				doc2.CSI_Code = SupportingDocumentType.REARebate;
				doc2.CSI_ReferenceNumber = "11,33";
				var doc3 = invoiceLine2.SupportingDocuments.AddNew();
				doc3.CSI_Code = SupportingDocumentType.REARebate;
				doc3.CSI_ReferenceNumber = "10,36";
				var wrapper3 = ESDocC10Header.New(entryHeader, declaration);
				AssertEquals("REA Rebate line1 is shown with all data included in entryLine1", true, wrapper3.HeaderGrid.Contains("Ayuda P1: 85.0000 * [TNE], 20255,20 KN..         -1721,69"));
				AssertEquals("REA Rebate line2 is shown with all data included in entryLine2", true, wrapper3.HeaderGrid.Contains("Ayuda P2: 85.0000 * [TNE], 255,20 KN....           -21,69"));

				invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCode.REA;
				invoiceLine.ZG_REAProductCode = ZString.Empty;
				invoiceLine.JI_Tariff = "22089054001";
				invoiceLine.ZG_IsREADirectConsumption = true;
				invoiceLine.JI_CustomsQuantity = 20255.2;
				invoiceLine.JI_CustomsUnitQty = "KGM";

				invoiceLine2.JI_PrimaryPreference = PrimaryPreferenceCode.REA;
				invoiceLine2.ZG_REAProductCode = "T001";
				invoiceLine2.JI_Tariff = "2208905400";
				invoiceLine2.ZG_IsREADirectConsumption = true;
				invoiceLine2.JI_CustomsQuantity = 0;
				invoiceLine2.JI_CustomsUnitQty = "KGM";

				var wrapper4 = ESDocC10Header.New(entryHeader, declaration);
				AssertEquals("REA Rebate line1 is shown with only Ayuda1 as the text", true, wrapper4.HeaderGrid.Contains("Ayuda P1................................         -1721,69"));
				AssertEquals("REA Rebate line2 is shown with only Ayuda2: 85.0000 * [TNE] as the text", true, wrapper4.HeaderGrid.Contains("Ayuda P2: 85.0000 * [TNE]...............           -21,69"));
			});
		}

		void AddFeeType(string type)
		{
			var fee = entryLine.Fees.AddNew();
			fee.G4_Type = type;
			fee.G4_Amount = "4";
		}

		public void TestFeeTypeA00()
		{
			AssertEquals(false, wrapper.HeaderGrid.Contains("Derechos Arancelarios"));
			AddFeeType("A00");
			AddFeeType("A10");
			AssertEquals(true, wrapper.HeaderGrid.Contains("Derechos Arancelarios...................             8,00"));
		}

		public void TestAntidumpingFee()
		{
			AssertEquals(false, wrapper.HeaderGrid.Contains("Derechos Antidumping"));
			AddFeeType("A30");
			AddFeeType("A35");
			AddFeeType("A40");
			AddFeeType("A45");
			AssertEquals(true, wrapper.HeaderGrid.Contains("Derechos Antidumping....................            16,00"));
		}

		public void TestFeeTypeA20()
		{
			AssertEquals(false, wrapper.HeaderGrid.Contains("Derechos Adicionales"));
			AddFeeType("A20");
			AssertEquals(true, wrapper.HeaderGrid.Contains("Derechos Adicionales....................             4,00"));
		}

		public void TestSpecialTaxesFee()
		{
			AssertEquals(false, wrapper.HeaderGrid.Contains("Impuestos Especiales"));
			AddFeeType("0XX");
			AddFeeType("5XX");
			AssertEquals(false, wrapper.HeaderGrid.Contains("Impuestos Especiales"));
			var wrapper2 = ESDocC10Header.New(entryHeader, declaration);
			AssertEquals(true, wrapper2.HeaderGrid.Contains("Impuestos Especiales....................             8,00"));
		}

		public void TestWaitressFee()
		{
			AssertEquals(false, wrapper.HeaderGrid.Contains("Mozos (Tarifa general)"));
			AddFeeType("131");
			AddFeeType("132");
			AssertEquals(true, wrapper.HeaderGrid.Contains("Mozos (Tarifa general)..................             8,00"));
		}

		public void TestFeeTypeE00()
		{
			AssertEquals(false, wrapper.HeaderGrid.Contains("Derechos percibidos en nombre de otros p"));
			AddFeeType("E00");
			AssertEquals(true, wrapper.HeaderGrid.Contains("Derechos percibidos en nombre de otros p             4,00"));
		}

		public void TestFeeType117()
		{
			AssertEquals(false, wrapper.HeaderGrid.Contains("Almacenaje"));
			AddFeeType("117");
			AssertEquals(true, wrapper.HeaderGrid.Contains("Almacenaje..............................             4,00"));
		}

		[TestDate(2025, 1, 1)]
		public void TestAccesoryCharges()
		{
			var eur = RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.EuropeanUnion);
			eur.ExchangeRates.DeleteAll();

			var thisMonthEurRate = eur.ExchangeRates.AddNew();
			thisMonthEurRate.RE_ExRateType = "CUS";
			thisMonthEurRate.RE_StartDate = new ZDateTime(2024, 3, 1, 0, 0, 1);
			thisMonthEurRate.RE_ExpiryDate = new ZDateTime(2025, 3, 1, 0, 0, 1);
			thisMonthEurRate.RE_SellRate = 0.8;
			thisMonthEurRate.RE_RX_NKExCurrency = CurrencyCodes.UnitedStates;
			Factory.Save();

			AssertEquals(expected: false, wrapper.HeaderGrid.Contains("GASTOS ACCESORIOS"));
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_IsDutiable = false;
			charge.J7_IsIncludedInITOT = false;
			charge.J7_IsGSTApplicable = true;
			charge.J7_RX_NKCurrency = "EUR";
			charge.J7_Amount = 28;
			charge.J7_ChargeType = "T3";
			charge.J7_ChargeDescription = "Test charge";
			AssertEquals(expected: false, wrapper.HeaderGrid.Contains("GASTOS ACCESORIOS"));

			var headerCharge = invoice.Charges.AddNew();
			headerCharge.J7_IsDutiable = false;
			headerCharge.J7_IsIncludedInITOT = false;
			headerCharge.J7_IsGSTApplicable = true;
			headerCharge.J7_RX_NKCurrency = "EUR";
			headerCharge.J7_Amount = 34;
			headerCharge.J7_ChargeType = "T3";
			headerCharge.J7_ChargeDescription = "Test charge";

			var charge2 = invoiceLine.ApportionedCharges.AddNew();
			charge2.J7_IsDutiable = false;
			charge2.J7_IsIncludedInITOT = false;
			charge2.J7_IsGSTApplicable = true;
			charge2.J7_RX_NKCurrency = "EUR";
			charge2.J7_Amount = 32;
			charge2.J7_ChargeType = "T3";
			charge2.J7_ChargeDescription = "Test apportioned charge";

			var charge3 = invoiceLine.ApportionedCharges.AddNew();
			charge3.J7_IsDutiable = false;
			charge3.J7_IsIncludedInITOT = false;
			charge3.J7_IsGSTApplicable = true;
			charge3.J7_RX_NKCurrency = "USD";
			charge3.J7_Amount = 25;
			charge3.J7_ChargeType = "T3";
			charge3.J7_ChargeDescription = "Test apportioned charge USD";
			AssertEquals(expected: false, wrapper.HeaderGrid.Contains("GASTOS ACCESORIOS"));

			var wrapper2 = ESDocC10Header.New(entryHeader, declaration);
			AssertEquals(expected: true, wrapper2.HeaderGrid.Contains("GASTOS ACCESORIOS"));
			AssertEquals(expected: true, wrapper2.HeaderGrid.Contains("..................            91,25"));
			AssertEquals(expected: false, wrapper2.HeaderGrid.Contains("..................            28,00"));
			AssertEquals(expected: false, wrapper2.HeaderGrid.Contains("..................            32,00"));
			AssertEquals(expected: false, wrapper2.HeaderGrid.Contains("..................            34,00"));
			AssertEquals(expected: false, wrapper2.HeaderGrid.Contains("..................            25,00"));
		}

		public void TestDocumentSupporter()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();

				var wrapperDec = ESDocC10Header.New(entryHeader, declaration);
				AssertType<JobDeclarationDocumentSupporter>(wrapperDec.DocumentSupporter);

				var wrapperEntry = ESDocC10Header.New(entryHeader, entryHeader);
				AssertType<CusEntryHeaderDocumentSupporter>(wrapperEntry.DocumentSupporter);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Spain);
			SetUpRefData();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsOffice = "ES137100";
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_EntryType = "REG";
			cusEntryNumber.CE_ParentID = entryHeader.PK;
			cusEntryNumber.CE_ParentTable = entryHeader.TableName;
			cusEntryNumber.CE_Category = "CUS";
			cusEntryNumber.CE_EntryNum = "4 T-2343G";
			cusEntryNumber.CE_IssueDate = new ZDateTime(2020, 01, 01);
			entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Spain;
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			wrapper = ESDocC10Header.New(entryHeader, declaration);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return ESDocC10Header.New(entryHeader, declaration);
		}

		void SetUpRefData()
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var esCode = Core.Constants.CountryCodes.Spain;

			refDataHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			customsOffice = refDataHelper.CreateCusCodeList(esCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ES137100", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			customsOffice.ZZD_Description = "MADRID CUSTOM";

			var impTariffType = refDataHelper.CreateNewOrGetExistingTariffType(esCode, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			var tariff = refDataHelper.LoadOrCreateNewTariff(esCode, impTariffType.PK, "2208905400", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var reaRateType = refDataHelper.CreateNewOrGetExistingRateType(esCode, UniversalReferenceConstants.RateTypeList.REA, "REA - AY Tax Rebate");
			var rateCodeAYD = refDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RateCodeCodeList.AYD, reaRateType.PK);
			var rateAYD = refDataHelper.CreateRate(tariff, rateCodeAYD.PK, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), rateFormula: "85.0000 * [TNE]");
			refDataHelper.CreateCusApplicability(rateAYD, null, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "T001");

			refDataHelper.CreateCusMapType("CUSUQ", MapDirectionList.Codes.BTH, "Description", false);
			refDataHelper.CreateCusMap("CUSUQ", "KGM", "KN", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), esCode);

			Factory.Save();
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		ESDocC10Header wrapper;
		Universal.RefCusCodeList customsOffice;
	}
}
