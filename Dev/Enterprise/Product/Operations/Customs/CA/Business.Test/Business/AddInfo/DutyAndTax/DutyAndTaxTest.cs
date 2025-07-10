using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DutyAndTax))]
	sealed class DutyAndTaxTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<DutyAndTax>
	{
		public void TestIDutyAndTaxDataForCalculationDutyType()
		{
			var dutyOrTax = Factory.New<DutyAndTax>();
			AssertEquals(ZString.Empty, ((IDutyAndTaxDataForCalculation)dutyOrTax).DutyType);
			dutyOrTax.C1_DutyType = DutyAndTaxManager.CombinedDuty.Excise;
			AssertEquals(DutyAndTaxManager.CombinedDuty.Excise, ((IDutyAndTaxDataForCalculation)dutyOrTax).DutyType);
		}

		public void TestIsEXDDuty()
		{
			var dutyOrTax = Factory.New<DutyAndTax>();
			Assert(!dutyOrTax.IsEXDDuty);
			dutyOrTax.C1_DutyType = DutyAndTaxManager.CombinedDuty.Excise;
			Assert(dutyOrTax.IsEXDDuty);
		}

		public void TestDefaultLogicForSetC1_Code()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);
			var refCusRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, DutyAndTaxTypes.Codes.ADD);
			var refCusRateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.SUR, refCusRateType.PK);
			var harmonizedTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "00000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate1 = helper.CreateRate(tariff1, refCusRateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.4*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var rate2 = helper.CreateRate(tariff1, refCusRateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.5*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			var rate3 = helper.CreateRate(tariff1, refCusRateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0", dataGrouping: Core.Constants.CountryCodes.Canada);
			var tradeGroupView1 = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.UnitedStates);
			var tradeGroupView2 = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.Australia);
			_ = helper.AddNewOrExistingCountry(tradeGroupView1, Core.Constants.CountryCodes.UnitedStates);
			_ = helper.AddNewOrExistingCountry(tradeGroupView2, Core.Constants.CountryCodes.Australia);
			_ = helper.CreateCusApplicability(rate1.PK, tradeGroupView1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "C001");
			_ = helper.CreateCusApplicability(rate2.PK, tradeGroupView1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "C002");
			_ = helper.CreateCusApplicability(rate3.PK, tradeGroupView1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "C003");
			_ = helper.CreateCusApplicability(rate3.PK, tradeGroupView2, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "C004");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.Declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			invoiceLine.JI_Tariff = "00000001";
			var tax = invoiceLine.DutiesAndTaxes.AddNew();
			AssertEquals("", tax.C1_TaxType);
			AssertEquals("", tax.C1_Code);
			AssertEquals(0m, tax.C1_Rate);
			AssertEquals(false, tax.C1_Override);
			AssertEquals("", tax.C1_RateType);
			AssertEquals(0m, tax.C1_Amount);

			tax.C1_Code = "C002";
			AssertEquals("", tax.C1_TaxType);
			AssertEquals("C002", tax.C1_Code);
			AssertEquals(0m, tax.C1_Rate);
			AssertEquals(false, tax.C1_Override);
			AssertEquals("", tax.C1_RateType);
			AssertEquals(0m, tax.C1_Amount);

			tax.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			tax.C1_Code = "C001";
			AssertEquals("SUR", tax.C1_TaxType);
			AssertEquals("C001", tax.C1_Code);
			AssertEquals(40m, tax.C1_Rate);
			AssertEquals(true, tax.C1_Override);
			AssertEquals(RateTypes.Codes.AdValorem, tax.C1_RateType);
			AssertEquals(0m, tax.C1_Amount);

			tax.C1_Code = "C002";
			AssertEquals("SUR", tax.C1_TaxType);
			AssertEquals("C002", tax.C1_Code);
			AssertEquals(0.5m, tax.C1_Rate);
			AssertEquals(true, tax.C1_Override);
			AssertEquals(RateTypes.Codes.Specific, tax.C1_RateType);
			AssertEquals(0m, tax.C1_Amount);

			tax.C1_Code = "C004";
			AssertEquals("SUR", tax.C1_TaxType);
			AssertEquals("C004", tax.C1_Code);
			AssertEquals(0.5m, tax.C1_Rate);
			AssertEquals(true, tax.C1_Override);
			AssertEquals(RateTypes.Codes.Specific, tax.C1_RateType);
			AssertEquals(0m, tax.C1_Amount);

			tax.C1_Override = false;
			tax.C1_Amount = 12m;
			AssertEquals(false, tax.C1_Override);
			AssertEquals(12m, tax.C1_Amount);

			tax.C1_Code = "C003";
			AssertEquals("SUR", tax.C1_TaxType);
			AssertEquals("C003", tax.C1_Code);
			AssertEquals(0m, tax.C1_Rate);
			AssertEquals(true, tax.C1_Override);
			AssertEquals(RateTypes.Codes.AcceptT, tax.C1_RateType);
			AssertEquals(0m, tax.C1_Amount);
		}

		public void TestUpdateRateTypeForCIGARS()
		{
			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			dutyOrTax.C1_Code = "E01";
			AssertEquals(true, dutyOrTax.C1_Override);
			AssertEquals(RateTypes.Codes.AcceptT, dutyOrTax.C1_RateType);
			dutyOrTax.C1_RateType = RateTypes.Codes.Specific;
			dutyOrTax.C1_Override = false;
			dutyOrTax.C1_Override = true;
			AssertEquals(RateTypes.Codes.AcceptT, dutyOrTax.C1_RateType);

			dutyOrTax.C1_RateType = RateTypes.Codes.Specific;
			dutyOrTax.C1_Code = "E07";
			AssertEquals(RateTypes.Codes.AcceptT, dutyOrTax.C1_RateType);
			dutyOrTax.C1_RateType = RateTypes.Codes.Specific;
			dutyOrTax.C1_Override = false;
			dutyOrTax.C1_Override = true;
			AssertEquals(RateTypes.Codes.AcceptT, dutyOrTax.C1_RateType);

			dutyOrTax.C1_RateType = RateTypes.Codes.Specific;
			dutyOrTax.C1_Code = "E37";
			AssertEquals(RateTypes.Codes.AcceptT, dutyOrTax.C1_RateType);
			dutyOrTax.C1_RateType = RateTypes.Codes.Specific;
			dutyOrTax.C1_Override = false;
			dutyOrTax.C1_Override = true;
			AssertEquals(RateTypes.Codes.AcceptT, dutyOrTax.C1_RateType);

			dutyOrTax.C1_RateType = RateTypes.Codes.AcceptX;
			dutyOrTax.C1_Code = "E01";
			AssertEquals(RateTypes.Codes.AcceptX, dutyOrTax.C1_RateType);
			dutyOrTax.C1_Code = "E07";
			AssertEquals(RateTypes.Codes.AcceptX, dutyOrTax.C1_RateType);
			dutyOrTax.C1_Code = "E37";
			AssertEquals(RateTypes.Codes.AcceptX, dutyOrTax.C1_RateType);
		}

		public void TestExciseOfLuxuryLine()
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = JobComInvoiceLine.LuxuryTaxTariffCode;
			classHeader.ZA_EffectiveDate = ZDateTime.Today.AddDays(-1);
			classHeader.ZA_ExpiryDate = ZDateTime.Today.AddDays(1);
			var refNumHeader = Factory.New<CACTaxRefNumHeader>();
			refNumHeader.ZD_ZA_ClassNumber = classHeader.PK;
			refNumHeader.ZD_EffectiveDate = ZDateTime.Today.AddDays(-1);
			refNumHeader.ZD_ExpiryDate = ZDateTime.Today.AddDays(1);

			var refNum1 = refNumHeader.RefNumbers.AddNew();
			refNum1.ZE_ExciseTaxRefNumber = "XXX";
			var refNum2 = refNumHeader.RefNumbers.AddNew();
			refNum2.ZE_ExciseTaxRefNumber = "YYY";

			var rate1 = Factory.New<CACTaxRate>();
			rate1.ZH_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			rate1.ZH_EffectiveDate = ZDateTime.Today.AddDays(-1);
			rate1.ZH_ExpiryDate = ZDateTime.Today.AddDays(1);
			rate1.ZH_TaxRefNumber = "XXX";
			rate1.ZH_RateType = RateTypes.Codes.AcceptX;
			rate1.ZH_Rate = 10;

			var rate2 = Factory.New<CACTaxRate>();
			rate2.ZH_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			rate2.ZH_EffectiveDate = ZDateTime.Today.AddDays(-1);
			rate2.ZH_ExpiryDate = ZDateTime.Today.AddDays(1);
			rate2.ZH_TaxRefNumber = "YYY";
			rate2.ZH_RateType = RateTypes.Codes.AcceptX;
			rate2.ZH_Rate = 100;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 1000000m;
			line.JI_CustomsQuantity = 1000m;
			line.ShouldDeleteLuxuryTaxInvoiceLine += () => true;
			line.CA_ApplyLuxuryTax = true;
			var luxuryTaxLine = line.LuxuryTaxInvoiceLine;
			AssertEquals("EXS should be created", 1, luxuryTaxLine.DutiesAndTaxes.Count);
			var exs = luxuryTaxLine.DutiesAndTaxes[0];
			AssertEquals(DutyAndTaxTypes.Codes.ExciseTax, exs.C1_TaxType);
			exs.C1_Code = rate1.ZH_TaxRefNumber;
			AssertEquals(10m, exs.C1_Rate);
			exs.C1_Code = rate2.ZH_TaxRefNumber;
			AssertEquals(100m, exs.C1_Rate);
		}

		[TestDate(2024, 07, 22)]
		public void TestCalculateCVDRateWithNoRounding()
		{
			var rate = Factory.New<RefExchangeRate>();
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.RE_StartDate = ZDateTime.Today;
			rate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			rate.RE_SellRate = 0.1904m;
			rate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.China;
			rate.RE_GC = Env.CurrentCompany.PK;

			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			dutyOrTax.C1_RateType = RateTypes.Codes.Specific;
			dutyOrTax.C1_ForeignCurrency = Core.Constants.CurrencyCodes.China;
			dutyOrTax.C1_ForeignRate = 1.25m;

			dutyOrTax.CalculateRateFromForeighRate();
			AssertEquals(0.238m, dutyOrTax.C1_Rate);
		}

		public void TestCalculateADDUsingInvoiceDateToGetExchangeRate()
		{
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			exchangeRate1.RE_StartDate = new ZDateTime(2023, 07, 03, 00, 00, 00);
			exchangeRate1.RE_ExpiryDate = new ZDateTime(2023, 07, 03, 23, 59, 00);
			exchangeRate1.RE_SellRate = 1.3m;
			exchangeRate1.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			exchangeRate1.RE_GC = Env.CurrentCompany.PK;

			var exchangeRate2 = Factory.New<RefExchangeRate>();
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			exchangeRate2.RE_StartDate = new ZDateTime(2023, 07, 05, 00, 00, 00);
			exchangeRate2.RE_ExpiryDate = new ZDateTime(2023, 07, 05, 23, 59, 00);
			exchangeRate2.RE_SellRate = 1.4m;
			exchangeRate2.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			exchangeRate2.RE_GC = Env.CurrentCompany.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceDate = new ZDateTime(2023, 07, 03);
			invoice.JZ_ValuationDateOverride = new ZDateTime(2023, 07, 05);
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsSecondQuantity = 120m;
			invoiceLine.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.Number;

			var duty = invoiceLine.DutiesAndTaxes.AddNew();
			duty.C1_Override = true;
			duty.C1_NormalValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			duty.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			duty.C1_RateType = RateTypes.Codes.Specific;
			duty.C1_UnitOfMeasure = CustomsUnitOfMeasureList.Codes.Number;
			duty.C1_NormalValuePerUnit = 100m;

			AssertEquals(2600m, duty.C1_Amount);
		}

		public void TestResetPreviousTransactionDetails()
		{
			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			dutyOrTax.C1_PreviousTranNumber = "ABC";
			dutyOrTax.C1_PreviousTranLine = 123;
			AssertEquals("ABC", dutyOrTax.C1_PreviousTranNumber);
			AssertEquals(123, dutyOrTax.C1_PreviousTranLine);

			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			AssertEquals("ABC", dutyOrTax.C1_PreviousTranNumber);
			AssertEquals(123, dutyOrTax.C1_PreviousTranLine);

			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			AssertEquals(ZString.Empty, dutyOrTax.C1_PreviousTranNumber);
			AssertEquals(ZInt.Zero, dutyOrTax.C1_PreviousTranLine);
		}

		public void TestResetAmountOnExemptCodeChanged()
		{
			var dutyOrTax = Factory.New<DutyAndTax>();
			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			dutyOrTax.C1_Amount = 20;
			dutyOrTax.C1_Override = false;
			dutyOrTax.C1_ExemptCode = "22";
			AssertEquals("C1_Amount", 20m, dutyOrTax.C1_Amount);

			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			dutyOrTax.C1_Override = true;
			dutyOrTax.C1_ExemptCode = "23";
			AssertEquals("C1_Amount", 20m, dutyOrTax.C1_Amount);

			dutyOrTax.C1_Override = false;
			dutyOrTax.C1_ExemptCode = "22";
			AssertEquals("C1_Amount", 0m, dutyOrTax.C1_Amount);

			dutyOrTax.C1_Amount = 20;
			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			dutyOrTax.C1_ExemptCode = "23";
			AssertEquals("C1_Amount", 0m, dutyOrTax.C1_Amount);

			dutyOrTax.C1_Amount = 20;
			dutyOrTax.C1_ExemptCode = ZString.Empty;
			AssertEquals("C1_Amount", 20m, dutyOrTax.C1_Amount);
		}

		public void TestSettingRateType()
		{
			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			dutyOrTax.C1_Amount = 20;
			dutyOrTax.C1_Override = true;
			dutyOrTax.C1_RateType = RateTypes.Codes.Specific;
			AssertEquals("C1_Amount not changed", 20m, dutyOrTax.C1_Amount);
			dutyOrTax.C1_RateType = RateTypes.Codes.Free;
			AssertEquals("C1_Amount now zero", 0m, dutyOrTax.C1_Amount);
			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			dutyOrTax.C1_Amount = 20;
			dutyOrTax.C1_RateType = RateTypes.Codes.Specific;
			AssertEquals("C1_Amount not changed", 20m, dutyOrTax.C1_Amount);
			dutyOrTax.C1_RateType = RateTypes.Codes.Exempt;
			AssertEquals("C1_Amount now zero", 0m, dutyOrTax.C1_Amount);
		}

		public void TestCAGSTRateCode()
		{
			JobComInvoiceLineTestHelper.CreateCAGSTRateCode(Factory, 5);
			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			dutyOrTax.C1_Code = "001";
			var gstRateCode = dutyOrTax.CAGSTRateCode;
			AssertNotNull(gstRateCode);
			AssertEquals("001", gstRateCode.ZZD_Code);
			AssertEquals("5.00", gstRateCode.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_Rate));
			AssertEquals(RateTypes.Codes.AdValorem, gstRateCode.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_RateType));
		}

		public void TestDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var refCusRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, DutyAndTaxTypes.Codes.ExciseTax);
			var refCusRateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "C01", refCusRateType.PK, description: "C01 Excise");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "CAGSTRateCodes", Core.Constants.CountryCodes.Canada);
			var gst1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "AA2", "GST", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			AssertEquals("CustomsDuty Description", "Customs Duty", dutyOrTax.Description);

			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			AssertEquals("ADD Description", "Anti Dumping", dutyOrTax.Description);

			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			AssertEquals("CVD Description", "Countervailing", dutyOrTax.Description);

			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			AssertEquals("SUR Description", "Surtax", dutyOrTax.Description);

			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			AssertEquals("ExciseTax Description", string.Empty, dutyOrTax.Description);
			dutyOrTax.C1_Code = refCusRateCode1.ZY1_RateCode;
			AssertEquals("ExciseTax Description", refCusRateCode1.ZY1_Description, dutyOrTax.Description);

			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			AssertEquals("GST Description", string.Empty, dutyOrTax.Description);
			dutyOrTax.C1_Code = gst1.ZZD_Code;
			AssertEquals("GST Description", gst1.ZZD_Description, dutyOrTax.Description);

			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.CPT;
			AssertEquals("CPT Description", DutyAndTaxTypes.Descriptions.CPT, dutyOrTax.Description);

			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.CTA;
			AssertEquals("CTA Description", DutyAndTaxTypes.Descriptions.CTA, dutyOrTax.Description);
		}

		public void TestQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_CustomsUnitQty = UnitOfWeightList.Codes.Kilogram;
			invoiceLine.JI_CustomsSecondQuantity = 20;
			invoiceLine.JI_CustomsSecondUnitQty = UnitOfWeightList.Codes.Kilogram;
			invoiceLine.JI_CustomsThirdQuantity = 30;
			invoiceLine.JI_CustomsThirdUnitQty = CustomsUnitOfMeasureList.Codes.Litre;
			dutyOrTax.Parent = invoiceLine;
			CACustomsDataRegistry.Instance.DefaultExciseDutyQuantityToFirstCustomsQuantity.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, false);
			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;

			AssertEquals("Quantity shouldn't be calculated if rate type isn't specific", ZDecimal.Zero, dutyOrTax.Quantity);

			dutyOrTax.C1_RateType = RateTypes.Codes.Specific;
			AssertEquals("Class/Tariff duties should use 1st quantity", invoiceLine.JI_CustomsQuantity, dutyOrTax.Quantity);

			dutyOrTax.C1_UnitOfMeasure = invoiceLine.JI_CustomsUnitQty;
			AssertEquals("Excise duty should use 2nd quantity even if it matches 1st", invoiceLine.JI_CustomsSecondQuantity, dutyOrTax.Quantity);

			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			dutyOrTax.C1_UnitOfMeasure = invoiceLine.JI_CustomsUnitQty;
			AssertEquals("Tax quantity should match 1st line quantity", invoiceLine.JI_CustomsQuantity, dutyOrTax.Quantity);

			invoiceLine.JI_CustomsSecondUnitQty = UnitOfWeightList.Codes.Gram;
			dutyOrTax.C1_UnitOfMeasure = invoiceLine.JI_CustomsSecondUnitQty;
			AssertEquals("Tax quantity should match 2st line quantity", invoiceLine.JI_CustomsSecondQuantity, dutyOrTax.Quantity);

			dutyOrTax.C1_UnitOfMeasure = invoiceLine.JI_CustomsThirdUnitQty;
			AssertEquals("Tax quantity should match 3st line quantity", invoiceLine.JI_CustomsThirdQuantity, dutyOrTax.Quantity);

			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			AssertEquals("ADD quantity", 30m, dutyOrTax.Quantity);

			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			AssertEquals("CVD quantity", 30m, dutyOrTax.Quantity);

			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			AssertEquals("SUR quantity", 30m, dutyOrTax.Quantity);

			var qty = 20000m;
			dutyOrTax.Quantity = qty;
			AssertEquals("Tax quantity should match 2st line quantity", qty, dutyOrTax.Quantity);

			dutyOrTax.Quantity = ZDecimal.Zero;
			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			dutyOrTax.C1_RateType = RateTypes.Codes.Specific;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Number;
			invoiceLine.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.Kilogram;
			invoiceLine.JI_CustomsThirdUnitQty = CustomsUnitOfMeasureList.Codes.Litre;
			dutyOrTax.C1_UnitOfMeasure = invoiceLine.JI_CustomsUnitQty;
			AssertEquals("Tax quantity should match nothing and be zero", 0m, dutyOrTax.Quantity);

			CACustomsDataRegistry.Instance.DefaultExciseDutyQuantityToFirstCustomsQuantity.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, true);
			dutyOrTax.C1_UnitOfMeasure = invoiceLine.JI_CustomsSecondUnitQty;
			AssertEquals("Tax quantity should match 2st line quantity", invoiceLine.JI_CustomsSecondQuantity, dutyOrTax.Quantity);
			dutyOrTax.C1_UnitOfMeasure = invoiceLine.JI_CustomsThirdUnitQty;
			AssertEquals("Tax quantity should match 3st line quantity", invoiceLine.JI_CustomsThirdQuantity, dutyOrTax.Quantity);
			dutyOrTax.C1_UnitOfMeasure = invoiceLine.JI_CustomsUnitQty;
			AssertEquals("Tax quantity should match 1st line quantity", invoiceLine.JI_CustomsQuantity, dutyOrTax.Quantity);
			dutyOrTax.C1_UnitOfMeasure = CustomsUnitOfMeasureList.Codes.Watt;
			AssertEquals("Tax quantity should match nothing and be zero", 0m, dutyOrTax.Quantity);
		}

		public void TestIsTax()
		{
			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			AssertEquals("ExciseTax is tax", true, dutyOrTax.IsTax);
			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			AssertEquals("GST is tax", true, dutyOrTax.IsTax);
			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			AssertEquals("CustomsDuty is tax", false, dutyOrTax.IsTax);
			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			AssertEquals("ADD is tax", false, dutyOrTax.IsTax);
			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			AssertEquals("CVD is tax", false, dutyOrTax.IsTax);
			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			AssertEquals("SUR is tax", false, dutyOrTax.IsTax);
		}

		public void TestC1_ExemptCode()
		{
			dutyOrTax.C1_TaxType = string.Empty;
			Assert(dutyOrTax.C1_ExemptCodeInfo.ReadOnly);

			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			Assert(!dutyOrTax.C1_ExemptCodeInfo.ReadOnly);
		}

		public void TestC1_ExemptCode_SURShouldBeOutOfSync()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var dutyOrTax1 = invoiceLine.DutiesAndTaxes.AddNew();
			dutyOrTax1.C1_Override = true;
			dutyOrTax1.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			var dutyOrTax2 = invoiceLine.DutiesAndTaxes.AddNew();
			dutyOrTax2.C1_Override = true;
			dutyOrTax2.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			var dutyOrTax3 = invoiceLine.DutiesAndTaxes.AddNew();
			dutyOrTax3.C1_Override = true;
			dutyOrTax3.C1_TaxType = DutyAndTaxTypes.Codes.SUR;

			dutyOrTax1.C1_ExemptCode = "22";
			AssertEquals("CVD_ExemptCode", "22", dutyOrTax2.C1_ExemptCode);
			AssertNotEquals("SUR_ExemptCode", "22", dutyOrTax3.C1_ExemptCode);

			dutyOrTax3.C1_ExemptCode = "23";
			AssertNotEquals("ADD_ExemptCode", "23", dutyOrTax1.C1_ExemptCode);
			AssertNotEquals("CVD_ExemptCode", "23", dutyOrTax2.C1_ExemptCode);
			AssertEquals("SUR_ExemptCode", "23", dutyOrTax3.C1_ExemptCode);
		}

		public void TestHasChangesChanged()
		{
			var countOfRun = 0;
			dutyOrTax.HasChangesChanged += (x, y) => countOfRun++;
			dutyOrTax.B7_ParentID = ZGuid.NewZGuid();
			dutyOrTax.B7_ParentTableCode = "TC";
			dutyOrTax.B7_AddInfoData = "AddInfoData";
			AssertEquals("CusAddInfo properties don't run HasChangesChanged", 0, countOfRun);

			dutyOrTax.C1_Amount = 10;
			dutyOrTax.C1_Code = "AAA";
			dutyOrTax.C1_ExemptCode = "22";
			dutyOrTax.C1_Override = true;
			dutyOrTax.C1_PreviousTranLine = 1;
			dutyOrTax.C1_PreviousTranNumber = "12356";
			dutyOrTax.C1_Rate = 10;
			dutyOrTax.C1_RateType = "V";
			dutyOrTax.C1_TaxType = "GST";
			dutyOrTax.C1_UnitOfMeasure = "KGM";
			AssertEquals("CADutyAndTaxAddInfo properties should run HasChangesChanged", 16, countOfRun);
		}

		public void TestReadOnly()
		{
			var matrixOfExpectedReadOnlyValues =
				new Dictionary<ZPropertyInfo, bool[]>
					{
						//			   \	Tax Type				|	CustomsDuty	|	ADD			|	ExciseTax	|	GST			|	CPT			|	CTA			|	SIM			|	CVD			|	SAF			|
						//	Property	\							|	NotOvr	Ovr	|	NotOvr	Ovr	|	NotOvr	Ovr	|	NotOvr	Ovr	|	NotOvr	Ovr	|	NotOvr	Ovr	|	NotOvr	Ovr	|	NotOvr	Ovr	|	NotOvr	Ovr	|
						{ dutyOrTax.C1_OverrideInfo,                new[] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false } },
						{ dutyOrTax.C1_CodeInfo,                    new[] { true, false, false, false, false, false, true, false, true, true, true, true, true, true, false, false, false, false } },
						{ dutyOrTax.DescriptionInfo,                new[] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true } },
						{ dutyOrTax.C1_ExemptCodeInfo,              new[] { true, true, false, false, false, false, false, false, false, false, true, true, false, false, false, false, false, false } },
						{ dutyOrTax.C1_RateTypeInfo,                new[] { true, false, true, false, true, false, true, false, true, false, true, false, true, true, true, false, true, false } },
						{ dutyOrTax.C1_RateInfo,                    new[] { true, false, true, false, true, false, true, false, true, false, true, false, true, true, true, false, true, false } },
						{ dutyOrTax.QuantityInfo,                   new[] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true } },
						{ dutyOrTax.C1_UnitOfMeasureInfo,           new[] { true, false, true, false, true, false, true, false, true, false, true, false, true, true, true, false, true, false } },
						{ dutyOrTax.C1_AmountInfo,                  new[] { true, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false } },
						{ dutyOrTax.C1_PreviousTranLineInfo,        new[] { true, false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true } },
						{ dutyOrTax.C1_PreviousTranNumberInfo,      new[] { true, false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true } },
						{ dutyOrTax.C1_TaxTypeInfo,                 new[] { true, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false } },
						{ dutyOrTax.C1_NormalValuePerUnitInfo,      new[] { true, true, true, false, true, true, true, true, true, true, true, true, true, true, true, true, true, true } },
						{ dutyOrTax.C1_NormalValueCurrencyInfo,     new[] { true, true, true, false, true, true, true, true, true, true, true, true, true, true, true, true, true, true } },
						{ dutyOrTax.C1_ForeignRateInfo,             new[] { true, true, true, false, true, true, true, true, true, true, true, true, true, true, true, false, true, true } },
						{ dutyOrTax.C1_ForeignCurrencyInfo,         new[] { true, true, true, false, true, true, true, true, true, true, true, true, true, true, true, false, true, true } }
					};

			var types = new[] { DutyAndTaxTypes.Codes.CustomsDuty, DutyAndTaxTypes.Codes.ADD, DutyAndTaxTypes.Codes.ExciseTax, DutyAndTaxTypes.Codes.GST, DutyAndTaxTypes.Codes.CPT, DutyAndTaxTypes.Codes.CTA, DutyAndTaxTypes.Codes.SIMADuty, DutyAndTaxTypes.Codes.CVD, DutyAndTaxTypes.Codes.SAF };
			foreach (var line in matrixOfExpectedReadOnlyValues)
			{
				for (var i = 0; i < types.Length; i++)
				{
					dutyOrTax.C1_TaxType = types[i];
					dutyOrTax.C1_Override = false;
					AssertReadOnly(line, (i * 2));
					dutyOrTax.C1_Override = true;
					AssertReadOnly(line, (i * 2) + 1);
				}
			}
		}

		void AssertReadOnly(KeyValuePair<ZPropertyInfo, bool[]> line, int expectedValueIndex)
		{
			AssertEquals(string.Format("{0} ReadOnly when C1_TaxType = '{1}' and C1_Override ='{2}'", line.Key.Name, dutyOrTax.C1_TaxType, dutyOrTax.C1_Override), line.Value[expectedValueIndex], line.Key.ReadOnly);
		}

		public void TestC1_Code_ReadOnly()
		{
			var classHeader1 = Factory.New<CACClassHeader>();
			classHeader1.ZA_ClassificationNumber = "0123456789";
			classHeader1.ZA_EffectiveDate = ZDateTime.Today.AddYears(-1);
			classHeader1.ZA_ExpiryDate = ZDateTime.Today.AddYears(1);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			var invoiceLine1 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = classHeader1.ZA_ClassificationNumber;

			var data1 = Factory.New<DutyAndTax>();
			data1.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			data1.Parent = invoiceLine1;
			data1.C1_Override = false;

			var data2 = Factory.New<DutyAndTax>();
			data2.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			data2.Parent = invoiceLine1;
			data2.C1_Override = false;

			var data3 = Factory.New<DutyAndTax>();
			data3.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			data3.Parent = invoiceLine1;
			data3.C1_Override = false;

			AssertEquals(false, data1.C1_CodeInfo.ReadOnly);
			AssertEquals(false, data2.C1_CodeInfo.ReadOnly);
			AssertEquals(false, data3.C1_CodeInfo.ReadOnly);

			data1.C1_Override = true;
			data2.C1_Override = true;
			data3.C1_Override = true;

			AssertEquals(false, data1.C1_CodeInfo.ReadOnly);
			AssertEquals(false, data2.C1_CodeInfo.ReadOnly);
			AssertEquals(false, data3.C1_CodeInfo.ReadOnly);
		}

		public void TestC1_Code_ReadOnly_IsCigars()
		{
			var classHeader1 = Factory.New<CACClassHeader>();
			classHeader1.ZA_ClassificationNumber = "0123456789";
			classHeader1.ZA_EffectiveDate = ZDateTime.Today.AddYears(-1);
			classHeader1.ZA_ExpiryDate = ZDateTime.Today.AddYears(1);

			var refNumHeader1 = Factory.New<CACTaxRefNumHeader>();
			refNumHeader1.ZD_ZA_ClassNumber = classHeader1.PK;
			refNumHeader1.ZD_EffectiveDate = ZDateTime.Today.AddYears(-1);
			refNumHeader1.ZD_ExpiryDate = ZDateTime.Today.AddYears(1);

			var refNum1 = refNumHeader1.RefNumbers.AddNew();
			refNum1.ZE_ExciseTaxRefNumber = "XXX";

			var classHeader2 = Factory.New<CACClassHeader>();
			classHeader2.ZA_ClassificationNumber = "0987654321";
			classHeader2.ZA_EffectiveDate = ZDateTime.Today.AddYears(-1);
			classHeader2.ZA_ExpiryDate = ZDateTime.Today.AddYears(1);

			var refNumHeader2 = Factory.New<CACTaxRefNumHeader>();
			refNumHeader2.ZD_ZA_ClassNumber = classHeader2.PK;
			refNumHeader2.ZD_EffectiveDate = ZDateTime.Today.AddYears(-1);
			refNumHeader2.ZD_ExpiryDate = ZDateTime.Today.AddYears(1);

			var refNum2 = refNumHeader2.RefNumbers.AddNew();
			refNum2.ZE_ExciseTaxRefNumber = "E01";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			var invoiceLine1 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = classHeader1.ZA_ClassificationNumber;
			var data1 = Factory.New<DutyAndTax>();
			data1.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			data1.Parent = invoiceLine1;
			data1.C1_Override = true;

			var invoiceLine2 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = classHeader2.ZA_ClassificationNumber;
			var data2 = Factory.New<DutyAndTax>();
			data2.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			data2.Parent = invoiceLine2;
			data2.C1_Override = true;

			var data3 = Factory.New<DutyAndTax>();
			data3.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			data3.Parent = invoiceLine2;
			data3.C1_Override = true;

			var data4 = Factory.New<DutyAndTax>();
			data4.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			data4.Parent = invoiceLine2;
			data4.C1_Override = true;

			var data5 = Factory.New<DutyAndTax>();
			data5.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			data5.Parent = invoiceLine2;
			data5.C1_Override = false;

			using (CACustomsDataRegistry.Instance.DefaultExciseTaxFromCustomsTariff.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				Assert("C1_Code should not be readonly", !data1.C1_CodeInfo.ReadOnly);
				Assert("C1_Code should not be readonly", !data2.C1_CodeInfo.ReadOnly);
				Assert("C1_Code should not be readonly", !data3.C1_CodeInfo.ReadOnly);
				Assert("C1_Code should not be readonly", !data4.C1_CodeInfo.ReadOnly);
				Assert("C1_Code should not be readonly", !data5.C1_CodeInfo.ReadOnly);
			}
			using (CACustomsDataRegistry.Instance.DefaultExciseTaxFromCustomsTariff.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				Assert("C1_Code should not be readonly", !data1.C1_CodeInfo.ReadOnly);
				Assert("C1_Code should not be readonly", !data2.C1_CodeInfo.ReadOnly);
				Assert("C1_Code should not be readonly", !data3.C1_CodeInfo.ReadOnly);
				Assert("C1_Code should not be readonly", !data4.C1_CodeInfo.ReadOnly);
				Assert("C1_Code should not be readonly", !data5.C1_CodeInfo.ReadOnly);
			}
		}

		public void TestSettingC1_Rate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_CustomsUnitQty = UnitOfWeightList.Codes.Kilogram;
			dutyOrTax.Parent = invoiceLine;
			CACustomsDataRegistry.Instance.DefaultExciseDutyQuantityToFirstCustomsQuantity.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, false);
			dutyOrTax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			dutyOrTax.C1_RateType = RateTypes.Codes.Specific;

			AssertEquals("Precondition:Quantity", 10m, dutyOrTax.Quantity);
			dutyOrTax.C1_Override = true;
			dutyOrTax.C1_Rate = 5m;
			AssertEquals("C1_Amount should be set", 50m, dutyOrTax.C1_Amount);
			dutyOrTax.C1_RateType = RateTypes.Codes.AdValorem;
			dutyOrTax.C1_Rate = 6m;
			AssertEquals("C1_Amount should be set to zero", 0m, dutyOrTax.C1_Amount);
			dutyOrTax.C1_Rate = 0m;
			AssertEquals("C1_Amount should be set to zero", 0m, dutyOrTax.C1_Amount);
			dutyOrTax.C1_RateType = RateTypes.Codes.Specific;
			dutyOrTax.C1_Rate = 5m;
			AssertEquals("C1_Amount should be set", 50m, dutyOrTax.C1_Amount);
			dutyOrTax.C1_Rate = 0m;
			AssertEquals("C1_Amount should be set to zero", 0m, dutyOrTax.C1_Amount);
		}

		public void TestSettingC1_OriginalAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var dutyAndTax = Factory.New<DutyAndTax>();
			dutyAndTax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			dutyAndTax.Parent = invoiceLine;
			dutyAndTax.C1_Amount = 10m;
			dutyAndTax.C1_OriginalAmount = 15m;

			AssertEquals(10m, dutyAndTax.C1_Amount);
			AssertEquals(15m, dutyAndTax.C1_OriginalAmount);

			dutyAndTax.C1_Override = true;

			AssertEquals(10m, dutyAndTax.C1_Amount);
			AssertEquals(10m, dutyAndTax.C1_OriginalAmount);

			dutyAndTax.C1_Amount = 20m;
			AssertEquals(20m, dutyAndTax.C1_Amount);
			AssertEquals(20m, dutyAndTax.C1_OriginalAmount);
		}

		public void TestIsWarehouseOrSupplementaryEntryHasValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			dutyOrTax.Parent = invoiceLine;
			dutyOrTax.C1_PreviousTranNumber = ZString.Empty;
			dutyOrTax.C1_PreviousTranLine = ZInt.Zero;
			AssertEquals(false, dutyOrTax.IsWarehouseOrSupplementaryEntryHasValues);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Supplementary;
			AssertEquals(false, dutyOrTax.IsWarehouseOrSupplementaryEntryHasValues);

			dutyOrTax.C1_PreviousTranNumber = "ABC";
			AssertEquals(true, dutyOrTax.IsWarehouseOrSupplementaryEntryHasValues);

			dutyOrTax.C1_PreviousTranNumber = ZString.Empty;
			dutyOrTax.C1_PreviousTranLine = 123;
			AssertEquals(true, dutyOrTax.IsWarehouseOrSupplementaryEntryHasValues);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Postal;
			AssertEquals(false, dutyOrTax.IsWarehouseOrSupplementaryEntryHasValues);
		}

		public void TestSynchroniser()
		{
			AssertSynchroniser(JobMessageTypeList.Codes.B2Adjustments);
			AssertSynchroniser(JobMessageTypeList.Codes.XTypeEntry);
		}

		void AssertSynchroniser(ZString messageType)
		{
			var factory = new BusinessObjectFactory();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var invoice = declaration.B2AsAccountedForInvoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			var asClaimedInvoice = invoice.CorrespondingAsClaimedForInvoice;
			AssertNotNull(messageType, asClaimedInvoice);

			var line = invoice.AsAccountForFilteredInvoiceLines.AddNew();
			line.CA_OriginalLineNo = "1";
			var asClaimedLine = line.CorrespondingAsClaimedForInvoiceLine;
			AssertNotNull(messageType, asClaimedLine);

			var dutyAndTax = line.DutiesAndTaxes.AddNew();
			dutyAndTax.C1_Override = true;
			AssertEquals(1, asClaimedLine.DutiesAndTaxes.Count);
			dutyAndTax.C1_TaxType = "DTY";
			var asClaimedDutyAndTax = asClaimedLine.DutiesAndTaxes[0];
			AssertEquals(messageType, "DTY", asClaimedDutyAndTax.C1_TaxType);

			AssertNotEquals(5m, asClaimedDutyAndTax.C1_Rate);
			dutyAndTax.C1_Rate = 5;
			AssertEquals(messageType, 5m, asClaimedDutyAndTax.C1_Rate);
			factory.Save();

			var factory1 = new BusinessObjectFactory();
			var duty = factory1.Load<DutyAndTax>(dutyAndTax.PK);
			Assert(messageType, !duty.ShouldSynchronise);
		}

		#region Implementation

		protected override IEnumerable<DutyAndTax> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			yield return invoiceLine.DutiesAndTaxes.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			yield return new DutyAndTaxCollection(entryLine).AddNew();

			var classification = factory.NewWithValidTestData<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			classification.CC_TariffNum = "12345678";
			yield return new DutyAndTaxCollection(classification).AddNew();

			var product = factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			pivot.CI_TariffNum = "1111.11.11";
			yield return new DutyAndTaxCollection(pivot).AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			dutyOrTax = invoiceLine.DutiesAndTaxes.AddNew();
			dutyOrTax.Parent = invoiceLine;
		}

		DutyAndTax dutyOrTax;

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var dutyOrTax = Factory.New<DutyAndTax>();
			dutyOrTax.B7_ParentTableCode = "JI";
			dutyOrTax.Parent = (IDutyAndTaxData)Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
			return dutyOrTax;
		}

		#endregion
	}
}
