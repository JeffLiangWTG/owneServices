using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.CA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CA.Business.Testing
{
	public sealed class DutyAndTaxManagerTest : TestCaseWithFactory
	{
		public void TestGetDutyTypeCode()
		{
			AssertEquals(DutyAndTaxManager.CombinedDuty.Classification, DutyAndTaxManager.CombinedDuty.GetDutyTypeCode(DutyAndTaxManager.CombinedDuty.Type.Classification));
			AssertEquals(DutyAndTaxManager.CombinedDuty.Excise, DutyAndTaxManager.CombinedDuty.GetDutyTypeCode(DutyAndTaxManager.CombinedDuty.Type.Excise));
			AssertEquals(DutyAndTaxManager.CombinedDuty.Tariff, DutyAndTaxManager.CombinedDuty.GetDutyTypeCode(DutyAndTaxManager.CombinedDuty.Type.Tariff));
			AssertEquals(ZString.Empty, DutyAndTaxManager.CombinedDuty.GetDutyTypeCode(DutyAndTaxManager.CombinedDuty.Type.Empty));
		}

		public void TestGetRateViewDependsOnC1_CodeForSURTax()
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
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.Declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			invoiceLine.JI_Tariff = "00000001";
			invoiceLine.DutyAndTaxManager.AddSIMADutyIfNotExists();
			var surTax = invoiceLine.DutiesAndTaxes.FirstOrDefault(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SUR);
			var rateCode = surTax.C1_Code;
			var rate = surTax.C1_Rate;

			switch (rateCode)
			{
				case "C001": rateCode = "C002";
					break;
				case "C002": rateCode = "C003";
					break;
				case "C003": rateCode = "C001";
					break;
				default: break;
			}

			surTax.C1_Code = rateCode;
			invoiceLine.DutyAndTaxManager.AddSIMADutyIfNotExists();
			AssertEquals(false, surTax.C1_Rate == rate);

			surTax.C1_Code = "ABCD";
			surTax.C1_Rate = 0m;
			invoiceLine.DutyAndTaxManager.AddSIMADutyIfNotExists();
			AssertEquals("ABCD", surTax.C1_Code);
			AssertEquals(0m, surTax.C1_Rate);

			surTax.C1_Code = "C001";
			invoiceLine.DutyAndTaxManager.AddSIMADutyIfNotExists();
			AssertEquals("C001", surTax.C1_Code);
			AssertEquals(40m, surTax.C1_Rate);
		}

		public void TestPopulateGSTWithoutCheckRefNumbers_UpdateGSTRateFromSRDB()
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = "1123456789";
			classHeader.ZA_EffectiveDate = ZDateTime.Today.AddYears(-1);
			classHeader.ZA_ExpiryDate = ZDateTime.Today.AddYears(1);

			invoiceLine.JI_Tariff = classHeader.ZA_ClassificationNumber;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Bag;
			invoiceLine.JI_CustomsQuantity = 1000;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			var gstTaxes = invoiceLine.DutiesAndTaxes.Find(tax => tax.C1_TaxType == DutyAndTaxTypes.Codes.GST);
			AssertEquals("One GST row", 1, gstTaxes.Count(tax => !tax.C1_Override));
			var gstTax = gstTaxes.FirstOrDefault();
			AssertEquals(5m, gstTax.C1_Rate);
			AssertEquals("V", gstTax.C1_RateType);
		}

		public void TestDefualtDutyAndTax()
		{
			PrepareGlobalTariffData(Factory, "00000001");
			var classHeader = GetClassHeader("00000001");
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			invoiceLine.CA_TreatmentCode = "01";
			invoiceLine.JI_Tariff = "00000001";
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			var dutyAndTaxes = invoiceLine.DutiesAndTaxes;
			AssertEquals(3, dutyAndTaxes.Count);
			AssertTax(dutyAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty), false, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 20m, ZString.Empty, 0m);
			AssertTax(dutyAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST), false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5m, ZString.Empty, 0m);
			AssertTax(dutyAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax), false, DutyAndTaxTypes.Codes.ExciseTax, ZString.Empty, false, ZString.Empty, 0m, ZString.Empty, 0m);

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			dutyAndTaxes = invoiceLine.DutiesAndTaxes;
			var exciseTax = dutyAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax);
			AssertTax(exciseTax, false, DutyAndTaxTypes.Codes.ExciseTax, ZString.Empty, false, ZString.Empty, 0m, ZString.Empty, 0m);
			AssertTax(dutyAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST), false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5m, ZString.Empty, 0m);

			exciseTax.C1_Code = "E90";
			AssertTax(exciseTax, false, DutyAndTaxTypes.Codes.ExciseTax, "E90", true, RateTypes.Codes.AdValorem, 40m, ZString.Empty, 0m);

			exciseTax.C1_Code = "E91";
			AssertTax(exciseTax, false, DutyAndTaxTypes.Codes.ExciseTax, "E91", true, RateTypes.Codes.Specific, 0.5m, "KGM", 0m);

			invoiceLine.CA_TreatmentCode = "02";
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertTax(dutyAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax && x.C1_Override), false, DutyAndTaxTypes.Codes.ExciseTax, "E91", true, RateTypes.Codes.Specific, 0.5m, "KGM", 0m);
			AssertTax(dutyAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST), false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5m, ZString.Empty, 0m);

			exciseTax.C1_Override = false;
			exciseTax.C1_Code = "C00";
			AssertTax(exciseTax, false, DutyAndTaxTypes.Codes.ExciseTax, "C00", false, ZString.Empty, 0m, ZString.Empty, 0m);

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertTax(dutyAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax), false, DutyAndTaxTypes.Codes.ExciseTax, "C00", false, ZString.Empty, 0m, ZString.Empty, 0m);
			AssertTax(dutyAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST), false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5m, ZString.Empty, 0m);
		}

		public void TestSURTaxCalculatedOnExchangeRate()
		{
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			exchangeRate1.RE_StartDate = new ZDateTime(2024, 11, 29, 00, 00, 00);
			exchangeRate1.RE_ExpiryDate = new ZDateTime(2024, 12, 05, 23, 59, 00);
			exchangeRate1.RE_SellRate = 1.4010m;
			exchangeRate1.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			exchangeRate1.RE_GC = Env.CurrentCompany.PK;

			var exchangeRate2 = Factory.New<RefExchangeRate>();
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			exchangeRate2.RE_StartDate = new ZDateTime(2024, 12, 06, 00, 00, 00);
			exchangeRate2.RE_ExpiryDate = new ZDateTime(2024, 12, 10, 23, 59, 00);
			exchangeRate2.RE_SellRate = 1.40380m;
			exchangeRate2.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			exchangeRate2.RE_GC = Env.CurrentCompany.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.ExWorks;
			invoice.JZ_InvoiceAmount = 407322.40m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceDate = new ZDateTime(2024, 11, 30);
			invoice.JZ_ValuationDateOverride = new ZDateTime(2024, 12, 07);
			invoice.JZ_InvoiceCurrLandedCostExRate = 1.40380m;
			invoice.JZ_Weight = 180871.60m;
			invoice.JZ_NetWeight = 180831.60m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7304290075";
			invoiceLine.JI_CustomsQuantity = 180832.0m;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Kilogram;
			invoiceLine.JI_CustomsSecondQuantity = 180.832m;
			invoiceLine.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.MetricTon;
			invoiceLine.CA_TreatmentCode = "02";
			invoiceLine.CA_ValueForDutyCode = "13";

			invoiceLine.JI_InvoiceQuantity = 364.000000m;
			invoiceLine.JI_InvoiceUQ = "EA";
			invoiceLine.JI_LinePrice = 407322.40m;
			invoiceLine.JI_Weight = 180871.600m;
			invoiceLine.JI_WeightUQ = CustomsUnitOfMeasureList.Codes.Keg;
			invoiceLine.JI_NetWeight = 180831.600m;
			invoiceLine.JI_NetWeightUQ = CustomsUnitOfMeasureList.Codes.Keg;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.HongKong;

			var add = invoiceLine.DutiesAndTaxes.AddNew();
			add.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			add.C1_ExemptCode = SIMACodes.Codes.C51;
			add.C1_Override = true;
			add.C1_RateType = RateTypes.Codes.Specific;
			add.C1_Rate = 166.90m;
			add.C1_UnitOfMeasure = CustomsUnitOfMeasureList.Codes.MetricTon;
			add.C1_NormalValuePerUnit = 2248.75m;
			add.C1_NormalValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			var cvd = invoiceLine.DutiesAndTaxes.AddNew();
			cvd.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			cvd.C1_ExemptCode = SIMACodes.Codes.C51;
			cvd.C1_Override = true;
			cvd.C1_RateType = RateTypes.Codes.Specific;
			cvd.C1_Rate = 12.36368m;
			cvd.C1_UnitOfMeasure = CustomsUnitOfMeasureList.Codes.MetricTon;
			cvd.C1_ForeignRate = 36.08m;
			cvd.C1_ForeignCurrency = Core.Constants.CurrencyCodes.China;

			var gst = invoiceLine.DutiesAndTaxes.AddNew();
			gst.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			gst.C1_Code = "001";
			gst.C1_RateType = RateTypes.Codes.AdValorem;
			gst.C1_Rate = 5m;
			gst.C1_Amount = 35833.50m;

			var surTax = invoiceLine.DutiesAndTaxes.AddNew();
			surTax.C1_ExemptCode = SIMACodes.Codes.C51;
			surTax.C1_Code = "2418B";
			surTax.C1_RateType = RateTypes.Codes.AdValorem;
			surTax.C1_Rate = 25m;
			surTax.C1_Amount = 142664.67m;

			AssertEquals(4, invoiceLine.DutiesAndTaxes.Count);
			Factory.Save();
			AssertEquals("VFD", 571799.19m, invoiceLine.CA_CustomsValue);
		}

		public void TestB2InvoiceLine_EditableForTypeFB3()
		{
			#region interchange

			const string interchangeText = @"UNB+UNOA:3+YUSAIRXPN+INETCECPT+120529:0443+8228
UNG+CUSDEC+U10207V1+KI+120529:0443+1232+UN+S:99B+10207YUSENT
UNH+1088+CUSDEC:S:99B:UN
BGM+:::AB+659+9
LOC+41+495
LOC+11+495
RFF+TN:000002752
RFF+ARA:842957342RM0001
TDT+11++9++9165
DOC+785+8010925243
DTM+204:20120521:102
MOA+43:55140'UNS+D
DMS+1'MOA+64:125
NAD+SE++P.DON INTERNATIONAL TRANSPORT ++++PR
DOC+935
DTM+129:20120315:102
LOC+27+JP+UIL+3901
PAT+1+CONSIGN:::02'MOA+6::USD
CST+1+POS+1+2402100010+23
MOA+40:109044
MOA+43:108117
MOA+125:128026
RFF+LI:1:1
MOA+38:100000
TAX+1+EXC++0.067
MOA+161:2010
TAX+7+VAT++5.0
MOA+1:6401
GIR+1+1
MEA+AAR++MIL:5000
MEA+AAA++KGM:50
TAX+5+++18.50
MOA+155:9250
GIR+1+1
MEA+AAR++NMB:300000
TAX+5+++8.0
MOA+155:8649
CST+2+POS+1+7326909091+13
MOA+40:5452177
MOA+43:5405833
MOA+125:5807212
RFF+LI:1:2
MOA+38:5000000
TAX+1+ADD++51
MOA+46:50000
TAX+7+VAT++5.0
MOA+1:290361
GIR+1+2
TAX+5+++6.5
MOA+155:351379
UNS+S
TAX+5+:::K90
MOA+155:369278
TAX+1+:::K90
MOA+105:50000
TAX+3+:::K90
MOA+4:2010
TAX+7+:::K90
MOA+1:296762
TAX+4+:::K90
MOA+176:718050
UNT+63+1088
UNE+1+1232
UNZ+1+8228
";

			const string interchangeText2 = @"UNB+UNOA:3+YUSAIRXPN+INETCECPT+120529:0443+8229
UNG+CUSDEC+U10207V1+KI+120529:0443+1232+UN+S:99B+10207YUSENT
UNH+1088+CUSDEC:S:99B:UN
BGM+:::AB+659+9
LOC+41+495
LOC+11+495
RFF+TN:000002752
RFF+ARA:842957342RM0001
TDT+11++9++9165
DOC+785+8010925243
DTM+204:20120521:102
MOA+43:55140'UNS+D
DMS+1'MOA+64:125
NAD+SE++P.DON INTERNATIONAL TRANSPORT ++++PR
DOC+935
DTM+129:20120315:102
LOC+27+JP+UIL+3901
PAT+1+CONSIGN:::02'MOA+6::USD
CST+1+POS+1+2402100010+23
MOA+40:109044
MOA+43:108117
MOA+125:128026
RFF+LI:1:1
MOA+38:100000
TAX+1+EXC++0.067
MOA+161:2010
TAX+7+VAT++5.0
MOA+1:6401
GIR+1+1
MEA+AAR++MIL:5000
MEA+AAA++KGM:50
TAX+5+++18.50
MOA+155:9250
GIR+1+1
MEA+AAR++NMB:300000
TAX+5+++8.0
MOA+155:8649
CST+2+POS+1+7326909091+13
MOA+40:5452177
MOA+43:5405833
MOA+125:5807212
RFF+LI:1:2
MOA+38:5000000
TAX+1+ADD++51
MOA+46:50000
TAX+7+VAT++5.0
MOA+1:290361
GIR+1+2
TAX+5+++6.5
MOA+155:351379
UNS+S
TAX+5+:::K90
MOA+155:369278
TAX+1+:::K90
MOA+105:50000
TAX+3+:::K90
MOA+4:2010
TAX+7+:::K90
MOA+1:296762
TAX+4+:::K90
MOA+176:718050
UNT+63+1088
UNE+1+1232
UNZ+1+8228
";
			#endregion

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "40000";
			declaration.TransactionNumber.SequentialNumber = "04228";
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var b3Header = declaration.B3EntryHeader;

			var message = B3AsLodgedDocumentWrapperTest.CreateMessageFromInterchangeString(Factory, interchangeText);
			message.EM_SystemCreateTimeUtc = new ZDateTime(2021, 07, 03);
			message.EM_Status = Enterprise.Messaging.Business.EDIMessage.Status.Sent;
			b3Header.Messages.Add(message);

			var acceptedResponse = Factory.New<B3Message>();
			acceptedResponse.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			acceptedResponse.EM_MessageSubType = B3EntryStatusList.Codes.Accepted;
			acceptedResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			acceptedResponse.EM_SystemCreateTimeUtc = message.EM_SystemCreateTimeUtc.AddHours(1);
			b3Header.Messages.Add(acceptedResponse);

			var lvsdeclaration = Factory.New<JobDeclaration>();
			lvsdeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			lvsdeclaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			lvsdeclaration.TransactionNumber.AccountSecurityCode = "40000";
			lvsdeclaration.TransactionNumber.SequentialNumber = "04229";
			lvsdeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			lvsdeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			lvsdeclaration.DoMerge();
			var lvsB3Header = lvsdeclaration.B3EntryHeader;

			var lvsMessage = B3AsLodgedDocumentWrapperTest.CreateMessageFromInterchangeString(Factory, interchangeText2);
			lvsMessage.EM_SystemCreateTimeUtc = new ZDateTime(2021, 07, 03);
			lvsMessage.EM_Status = Enterprise.Messaging.Business.EDIMessage.Status.Sent;
			lvsB3Header.Messages.Add(lvsMessage);

			var lvsAcceptedResponse = Factory.New<B3Message>();
			lvsAcceptedResponse.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			lvsAcceptedResponse.EM_MessageSubType = B3EntryStatusList.Codes.Accepted;
			lvsAcceptedResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			lvsAcceptedResponse.EM_SystemCreateTimeUtc = message.EM_SystemCreateTimeUtc.AddHours(1);
			lvsB3Header.Messages.Add(lvsAcceptedResponse);

			Factory.Save();

			var b2Declaration = Factory.New<JobDeclaration>();
			b2Declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2Declaration.CA_OriginalTransactionNo = declaration.DeclarationNumber;

			var linesWrapper = b2Declaration.GetOriginalB3Lines();
			linesWrapper[0].IsSelected = true;
			b2Declaration.SeedingB2(linesWrapper);

			var accountedInvoices = b2Declaration.B2AsAccountedForInvoices;
			AssertEquals("Invoice should be seeded from B3", 1, accountedInvoices.Count);
			var accountedLines = accountedInvoices[0].AsAccountForFilteredInvoiceLines;
			AssertEquals("Invoice line should be seeded from B3", 1, accountedLines.Count);
			AssertEquals("Line should be read only", true, accountedLines[0].ReadOnly);

			var b2LVSDeclaration = Factory.New<JobDeclaration>();
			b2LVSDeclaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2LVSDeclaration.CA_OriginalTransactionNo = lvsdeclaration.DeclarationNumber;

			var lvsLinesWrapper = b2LVSDeclaration.GetOriginalB3Lines();
			lvsLinesWrapper[0].IsSelected = true;
			b2LVSDeclaration.SeedingB2(lvsLinesWrapper);

			var accountedLVSInvoices = b2LVSDeclaration.B2AsAccountedForInvoices;
			AssertEquals("Invoice should be seeded from B3", 1, accountedLVSInvoices.Count);
			var accountedLVSLines = accountedLVSInvoices[0].AsAccountForFilteredInvoiceLines;
			AssertEquals("Invoice line should be seeded from B3", 1, accountedLVSLines.Count);
			AssertEquals("Line should not be read only", false, accountedLVSLines[0].ReadOnly);
		}

		public void TestRefreshAggregatedFieldsCalculatorAfterApportion()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_TotalWeight = 123.45;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.CA_MergeBy = B3MergeByList.Codes.ClassificationOrTariffOverMultipleInvoices;

			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();

			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "KJ";
			invoice.JZ_InvoiceAmount = 123.00m;
			invoice.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsComputedValue;
			invoice.CA_RN_NKExport = "US";
			invoice.JZ_RX_NKInvoice_Currency = "CA";
			invoice.JZ_InvoiceCurrExRate = 1.0m;

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			line1.CA_PageNumber = 1;
			line1.JI_CustomsQuantity = 1m;
			line1.JI_LinePrice = 123.00m;
			line1.CA_TreatmentCode = "02";

			var line1GST = line1.DutiesAndTaxes.AddNew();
			line1GST.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			line1GST.C1_Code = "001";
			line1GST.C1_RateType = RateTypes.Codes.AdValorem;
			line1GST.C1_Rate = 5;
			line1GST.C1_Amount = 6.15m;

			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LineNo = 2;
			line2.CA_PageNumber = 1;
			line2.JI_InvoiceQuantity = 1m;
			line2.JI_CustomsQuantity = 2m;
			line2.JI_LinePrice = 450.90m;
			line2.UnitPrice = 450.90m;
			line2.CA_TreatmentCode = "02";

			var line2GST = line2.DutiesAndTaxes.AddNew();
			line2GST.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			line2GST.C1_Code = "001";
			line2GST.C1_RateType = RateTypes.Codes.AdValorem;
			line2GST.C1_Rate = 5;
			line2GST.C1_Amount = 22.11m;

			var line3 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_LineNo = 3;
			line3.CA_PageNumber = 1;
			line3.JI_CustomsQuantity = 1m;
			line3.JI_LinePrice = 450.90m;
			line3.CA_TreatmentCode = "02";

			var line3GST = line3.DutiesAndTaxes.AddNew();
			line3GST.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			line3GST.C1_Code = "001";
			line3GST.C1_RateType = RateTypes.Codes.AdValorem;
			line3GST.C1_Rate = 5;
			line3GST.C1_Amount = 22.33m;

			line1.JI_AddInfo = "B3SubHeaderNumber=1*CalculationMethod=N*CFIAInd=Y*CustomsValue=123*CVforCurrConv=123*ECCCInd=Y*GSTAmount=15.15*GSTRateDescription=5V*OGDStatus=NOT*PageNumber=1*RN_NKSource=US*StateOfSource=NY*ValueForTax=123";
			line2.JI_AddInfo = "B3SubHeaderNumber=1*CalculationMethod=N*CFIAInd=Y*CustomsValue=450.9*CVforCurrConv=450.9*ECCCInd=Y*GSTAmount=44.44*GSTRateDescription=5V*OGDStatus=NOT*PageNumber=1*RN_NKSource=US*StateOfSource=NY*ValueForTax=450.9";

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var afterapportion = line2.DutiesAndTaxes[0];
			AssertEquals("GST amount on Line2", 22.11m, afterapportion.C1_Amount);

			var entryHeader = declaration.ReleaseEntryHeader;
			var lineNo1 = entryHeader.InvoiceLines.First();
			AssertEquals("GST amount 1", 6.15m, lineNo1.JI_Calc_GSTVATAmount);

			var lineNo2 = (JobComInvoiceLine)entryHeader.InvoiceLines.First(x => x.JI_LineNo == 2);
			AssertEquals("GST amount 2", 22.11m, lineNo2.JI_Calc_GSTVATAmount);

			var lineNo3 = (JobComInvoiceLine)entryHeader.InvoiceLines.First(x => x.JI_LineNo == 3);
			AssertEquals("GST amount 3", 22.33m, lineNo3.JI_Calc_GSTVATAmount);
		}

		public void TestRecalculateSIMAAmountWhenQuantityChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "CA-1482";

			var line = invoice.JobComInvoiceLines.AddNew();
			var cvd = line.DutiesAndTaxes.AddNew();
			cvd.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			cvd.C1_ExemptCode = SIMACodes.Codes.C51;
			cvd.C1_Override = true;
			cvd.C1_RateType = RateTypes.Codes.Specific;
			cvd.C1_Rate = 2;
			cvd.C1_UnitOfMeasure = "NMB";

			var add = line.DutiesAndTaxes.AddNew();
			add.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			add.C1_ExemptCode = SIMACodes.Codes.C51;
			add.C1_Override = true;
			add.C1_RateType = RateTypes.Codes.Specific;
			add.C1_Rate = 5;
			add.C1_UnitOfMeasure = "BG";

			var gst = line.DutiesAndTaxes.AddNew();
			gst.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			gst.C1_ExemptCode = SIMACodes.Codes.C51;
			gst.C1_Override = true;
			gst.C1_RateType = RateTypes.Codes.Specific;
			gst.C1_Rate = 3;
			gst.C1_UnitOfMeasure = "BG";

			line.JI_CustomsSecondUnitQty = "NMB";
			line.JI_CustomsSecondQuantity = 2;
			line.JI_CustomsThirdUnitQty = "BG";
			line.JI_CustomsThirdQuantity = 2;

			AssertEquals(cvd.C1_Amount, 4m);
			AssertEquals(add.C1_Amount, 10m);
			AssertEquals(gst.C1_Amount, 0m);

			line.JI_CustomsSecondQuantity = 3;
			line.JI_CustomsThirdQuantity = 3;

			AssertEquals(cvd.C1_Amount, 6m);
			AssertEquals(add.C1_Amount, 15m);
			AssertEquals(gst.C1_Amount, 0m);

			add.C1_ExemptCode = SIMACodes.Codes.C50;
			AssertEquals(cvd.C1_Amount, 0m);
			AssertEquals(add.C1_Amount, 0m);

			line.JI_CustomsSecondQuantity = 4;
			line.JI_CustomsThirdQuantity = 4;

			AssertEquals(cvd.C1_Amount, 0m);
			AssertEquals(add.C1_Amount, 0m);
		}

		public void TestRefreshCustomsValueAfterCalculated()
		{
			var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();
			var helper = new DeclarationTestHelper(factory, true);
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "CA-1482";
			invoice.CA_RN_NKExport = Enterprise.Core.Constants.CountryCodes.Japan;
			invoice.JZ_RX_NKInvoice_Currency = helper.CAD.RX_Code;
			invoice.JZ_InvoiceCurrExRate = 1.6427m;
			invoice.IsJZ_InvoiceCurrExRateUserEnterable = true;

			var line = invoice.JobComInvoiceLines.AddNew();
			line.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 108.05m);

			line = invoice.JobComInvoiceLines.AddNew();
			line.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 54.82m);

			line = invoice.JobComInvoiceLines.AddNew();
			line.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 50.01m);

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			AssertEquals("MergedLines.Count", 1, entryHeader.MergedLines.Count);

			IClassificationLine1 classLine1 = entryHeader.MergedLines[0];
			AssertEquals("classLine1.ValueForCurrency", 155.52m, classLine1.ValueForCurrency);
			AssertEquals("classLine1.ValueForDuty", 255.47m, classLine1.ValueForDuty);
		}

		public void TestClearCachedValues()
		{
			invoiceLine.JI_Tariff = classificationNumber3;
			invoiceLine.JI_CustomsQuantity = 1000;
			invoiceLine.JI_CustomsUnitQty = customsUnits1;
			var dty1 = invoiceLine.DutiesAndTaxes.AddNew("DTY");
			dty1.C1_Override = true;
			dty1.C1_Amount = 15m;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals(1015m, invoiceLine.DutyAndTaxManager.NormalDutyPaidValue);
			AssertEquals(1015m, invoiceLine.DutyAndTaxManager.NormalValueForTax);
			AssertEquals(1015m, invoiceLine.DutyAndTaxManager.CalculatedValueForTax);

			dty1.C1_Amount = 0m;
			invoiceLine.DutyAndTaxManager.ClearCachedValues();
			AssertEquals(1000m, invoiceLine.DutyAndTaxManager.NormalDutyPaidValue);
			AssertEquals(1000m, invoiceLine.DutyAndTaxManager.NormalValueForTax);
			AssertEquals(1000m, invoiceLine.DutyAndTaxManager.CalculatedValueForTax);
		}

		public void TestAddOrUpdateTaxCodeWhenEnterProductCode()
		{
			PrepareGlobalTariffData(Factory, "2234567890");
			var classHeader = GetClassHeader("2234567890");
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "APART";
			part.OP_Desc = "A typical part";
			var relationImporter = part.RelatedOrganisations.AddNew();
			relationImporter.OU_OH = importer.PK;
			relationImporter.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var relationSupplier = part.RelatedOrganisations.AddNew();
			relationSupplier.OU_OH = supplier.PK;
			relationSupplier.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals(0, invoiceLine.DutiesAndTaxes.Count(x => DutyAndTaxTypes.IsSIMATaxCode(x.C1_TaxType)));

			var importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_TariffNum = classificationNumber1;
			importPivot.CCA_GSTStatusCode = "01";
			importPivot.CCA_ETExemption = "03";
			importPivot.CCA_ETRateCode = "E91";
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_Tariff = "2234567890";
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_CustomsUnitQty = customsUnits1;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertExemptionCode(DutyAndTaxTypes.Codes.GST, "01");
			AssertExemptionCode(DutyAndTaxTypes.Codes.ExciseTax, "03");
			var dutyAndTaxs = new List<DutyAndTax>(invoiceLine.DutiesAndTaxes.Find(tax => tax.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax));
			AssertEquals("1 occurance of this tax/duty type", 1, dutyAndTaxs.Count);
			AssertEquals("Exemption code should be correct", "E91", dutyAndTaxs[0].C1_Code);
		}

		public void TestTotalValueApportionDividedByZero()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "CA-1482";

			var line = invoice.JobComInvoiceLines.AddNew();
			line.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
			JobComInvoiceLineTestHelper.FillInvoiceLine(line, 7386.5m);

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_JE = declaration.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_CustomsValue = 546m;
			declaration.ResumeApportionment();

			AssertEquals(0, entryHeader.InvoiceLines.Count());
			AssertNoExceptionThrown("Attempted to divide by zero.", () => entryLine.ApportionRoundingAmountsOverLines());
		}

		public void TestExciseTaxesTotalAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "CA-1482";
			invoice.JZ_RX_NKInvoice_Currency = "CAD";
			invoice.JZ_InvoiceCurrExRate = 1m;

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			line1.CA_CVforCurrConv = 100m;
			var escise1 = line1.DutiesAndTaxes.AddNew();
			escise1.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			escise1.C1_Override = true;
			escise1.C1_Amount = 10m;
			var escise2 = line1.DutiesAndTaxes.AddNew();
			escise2.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			escise2.C1_Override = true;
			escise2.C1_Amount = 200m;

			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LineNo = 2;
			line2.CA_CVforCurrConv = 50m;
			var escise3 = line2.DutiesAndTaxes.AddNew();
			escise3.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			escise3.C1_Override = true;
			escise3.C1_Amount = 5m;
			var escise4 = line2.DutiesAndTaxes.AddNew();
			escise4.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			escise4.C1_Override = true;
			escise4.C1_Amount = 40m;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals(210m, line1.DutyAndTaxManager.ExciseTaxesTotalAmount);
			AssertEquals(45m, line2.DutyAndTaxManager.ExciseTaxesTotalAmount);
		}

		public void TestNormalValueForTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "CA-1482";
			invoice.JZ_RX_NKInvoice_Currency = "CAD";
			invoice.JZ_InvoiceCurrExRate = 1m;

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			line1.CA_CVforCurrConv = 100m;
			var duty1 = line1.DutiesAndTaxes.AddNew();
			duty1.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			duty1.C1_Override = true;
			duty1.C1_Amount = 23m;
			var sima1 = line1.DutiesAndTaxes.AddNew();
			sima1.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			sima1.C1_ExemptCode = SIMACodes.Codes.C51;
			sima1.C1_Override = true;
			sima1.C1_Amount = 33m;
			var escise1 = line1.DutiesAndTaxes.AddNew();
			escise1.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			escise1.C1_Override = true;
			escise1.C1_Amount = 10m;

			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LineNo = 2;
			line2.CA_CVforCurrConv = 50m;
			var duty2 = line2.DutiesAndTaxes.AddNew();
			duty2.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			duty2.C1_Override = true;
			duty2.C1_Amount = 47m;
			var sima2 = line2.DutiesAndTaxes.AddNew();
			sima2.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			sima2.C1_ExemptCode = SIMACodes.Codes.C51;
			sima2.C1_Override = true;
			sima2.C1_Amount = 6m;
			var escise2 = line2.DutiesAndTaxes.AddNew();
			escise2.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			escise2.C1_Override = true;
			escise2.C1_Amount = 5m;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals(66m, line1.DutyAndTaxManager.NormalValueForTax);
			AssertEquals(58m, line2.DutyAndTaxManager.NormalValueForTax);
		}

		public void TestGSTTaxesTotalAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "CA-1482";
			invoice.JZ_RX_NKInvoice_Currency = "CAD";
			invoice.JZ_InvoiceCurrExRate = 1m;

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			line1.CA_CVforCurrConv = 100m;
			var gst1 = line1.DutiesAndTaxes.AddNew();
			gst1.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			gst1.C1_Override = true;
			gst1.C1_Amount = 10m;
			var gst2 = line1.DutiesAndTaxes.AddNew();
			gst2.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			gst2.C1_Override = true;
			gst2.C1_Amount = 200m;

			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LineNo = 2;
			line2.CA_CVforCurrConv = 50m;
			var gst3 = line2.DutiesAndTaxes.AddNew();
			gst3.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			gst3.C1_Override = true;
			gst3.C1_Amount = 5m;
			var gst4 = line2.DutiesAndTaxes.AddNew();
			gst4.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			gst4.C1_Override = true;
			gst4.C1_Amount = 40m;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals(210m, line1.DutyAndTaxManager.GSTTaxesTotalAmount);
			AssertEquals(45m, line2.DutyAndTaxManager.GSTTaxesTotalAmount);
		}

		public void TestSIMAAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "CA-1482";
			invoice.JZ_RX_NKInvoice_Currency = "CAD";
			invoice.JZ_InvoiceCurrExRate = 1m;

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			line1.CA_PageNumber = 1;
			line1.CA_CVforCurrConv = 100m;
			line1.JI_Tariff = classificationNumber1;
			line1.CA_99TariffCode = tariffCode1;
			line1.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			line1.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsPaidPayableWithoutAdjustments;
			line1.JI_LinePrice = 10m;
			line1.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			line1.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;

			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LineNo = 2;
			line2.CA_PageNumber = 1;
			line2.CA_CVforCurrConv = 50m;
			line2.JI_Tariff = classificationNumber1;
			line2.CA_99TariffCode = tariffCode1;
			line2.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			line2.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsPaidPayableWithoutAdjustments;
			line2.JI_LinePrice = 20m;
			line2.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			line2.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			AssertEquals(1, entryHeader.AllEntryLines.Count);
			var entryLine = entryHeader.AllEntryLines[0];
			AssertEquals(0m, ((IClassificationLine1)entryLine).SIMAAssessment);
		}

		public void TestPivotDutyRateForCurrentCountry()
		{
			Factory.ClearCachedValue<CACClassHeader>(string.Format("CACClassHeader_{0}_{1}_{2}", currentDate.ToShortDateString(), classificationNumber1, false));
			PrepareRefFiles();
			Factory.Save();
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = classificationNumber5;
			// default MFN
			AssertEquals("8%", pivot.DutyRateForCurrentCountry);

			pivot.CCA_TreatmentCode = TariffTreatmentCodes.Codes.General;
			pivot.CI_TariffNum = classificationNumber5;
			// single AdValorem
			AssertEquals("123%", pivot.DutyRateForCurrentCountry);

			pivot.CCA_TreatmentCode = TariffTreatmentCodes.Codes.Chile;
			pivot.CI_TariffNum = classificationNumber1;
			// no rate defaults to general rate of duty
			AssertEquals("20%", pivot.DutyRateForCurrentCountry);

			pivot.CCA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			pivot.CI_TariffNum = classificationNumber1;
			Factory.ClearCachedValue<CACRate>(string.Format("CACRate|{0}|{1}|{2}|{3}", classHeader1.ZA_ClassificationNumber, TariffTreatmentCodes.Codes.UnitedStates, currentDate.ToShortDateString(), "CLS"));
			// AdValorem + specific
			AssertEquals("70¢/KGM + 19%", pivot.DutyRateForCurrentCountry);

			pivot.CCA_TreatmentCode = TariffTreatmentCodes.Codes.NewZealand;
			pivot.CI_TariffNum = classificationNumber1;
			Factory.ClearCachedValue<CACRate>(string.Format("CACRate|{0}|{1}|{2}|{3}", classificationNumber1, TariffTreatmentCodes.Codes.NewZealand, currentDate.ToShortDateString(), "CLS"));
			// AdValorem + specific + max + min
			AssertEquals("2.34¢/KGM + 5% But not less than 4.74¢/KGM or not more than 9.48¢/KGM", pivot.DutyRateForCurrentCountry);

			pivot.CCA_TreatmentCode = TariffTreatmentCodes.Codes.General;
			pivot.CI_TariffNum = classificationNumber4;
			Factory.ClearCachedValue<CACRate>(string.Format("CACRate|{0}|{1}|{2}|{3}", classificationNumber4, TariffTreatmentCodes.Codes.General, currentDate.ToShortDateString(), "CLS"));
			// free
			AssertEquals("Free", pivot.DutyRateForCurrentCountry);

			pivot.CCA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			pivot.CI_TariffNum = classificationNumber2;
			Factory.ClearCachedValue<CACRate>(string.Format("CACRate|{0}|{1}|{2}|{3}", classificationNumber2, TariffTreatmentCodes.Codes.MostFavouredNation, currentDate.ToShortDateString(), "CLS"));
			// excise duty
			AssertEquals("20% + Free + Manual + Manual", pivot.DutyRateForCurrentCountry);

			pivot.CCA_TreatmentCode = TariffTreatmentCodes.Codes.Chile;
			pivot.CI_TariffNum = classificationNumber1;
			pivot.CCA_99TariffCode = tariffCode1;
			Factory.ClearCachedValue<CACRate>(string.Format("CACRate|{0}|{1}|{2}|{3}", classificationNumber1, TariffTreatmentCodes.Codes.Chile, currentDate.ToShortDateString(), "CLS"));
			// tariff override duty, no rate so deafult
			AssertEquals("20%", pivot.DutyRateForCurrentCountry);

			pivot.CCA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			pivot.CI_TariffNum = classificationNumber1;
			pivot.CCA_99TariffCode = tariffCode1;
			Factory.ClearCachedValue<CACRate>(string.Format("CACRate|{0}|{1}|{2}|{3}", classificationNumber1, TariffTreatmentCodes.Codes.UnitedStates, currentDate.ToShortDateString(), "CLS"));
			// tariff override duty, free
			AssertEquals("Free", pivot.DutyRateForCurrentCountry);

			pivot.CCA_TreatmentCode = TariffTreatmentCodes.Codes.NewZealand;
			pivot.CI_TariffNum = classificationNumber1;
			pivot.CCA_99TariffCode = tariffCode4;
			Factory.ClearCachedValue<CACRate>(string.Format("CACRate|{0}|{1}|{2}|{3}", classificationNumber1, TariffTreatmentCodes.Codes.NewZealand, currentDate.ToShortDateString(), "CLS"));
			// tariff override duty
			AssertEquals("22%", pivot.DutyRateForCurrentCountry);
		}

		public void TestPivotTaxRateForCurrentCountry()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType3 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.ExciseTax);
			var rateCode5 = universalHelper.LoadOrCreateNewCusRateCode(Factory, "BB2", rateType3.PK);
			var preference1 = universalHelper.CreatePreferenceForCountry("01", "Preference 01", Core.Constants.CountryCodes.Canada);
			var preference2 = universalHelper.CreatePreferenceForCountry("02", "Preference 02", Core.Constants.CountryCodes.Canada);
			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "3234567890", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var taxRate7 = universalHelper.CreateRate(tariff1, rateCode5.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "22*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			Factory.Save();
			PrepareRefFiles();
			var classHeader = GetClassHeader("3234567890");
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "3234567890";

			// GST and excise
			AssertEquals("EXT: 22% + GST: 5%", pivot.TaxRateForCurrentCountry);

			pivot.CCA_ETExemption = "88";
			// GST but excise exempt
			AssertEquals("EXT: Exempt + GST: 5%", pivot.TaxRateForCurrentCountry);

			pivot.CCA_GSTStatusCode = "48";
			pivot.CCA_ETExemption = ZString.Empty;
			// GST exempt
			AssertEquals("EXT: 22% + GST: Exempt", pivot.TaxRateForCurrentCountry);
		}

		public void TestRemissionGiftsUpTo60()
		{
			var classHeader = GetClassHeader("0000000890");
			PrepareGlobalTariffData(Factory, "0000000890");
			PrepareRefFiles();
			invoiceLine.JI_Tariff = "0000000890";
			invoiceLine.CA_99TariffCode = string.Empty;
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_CustomsUnitQty = customsUnits1;
			invoiceLine.CA_CustomsValue = 100;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty), false, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 20m, ZString.Empty, 20m);
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax), false, DutyAndTaxTypes.Codes.ExciseTax, "", false, "", 0m, "", 0m);
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST), false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5m, ZString.Empty, 6.0m);
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.GiftsUpTo60;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty), false, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 20m, ZString.Empty, 8m);
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax), false, DutyAndTaxTypes.Codes.ExciseTax, "", false, "", 0m, "", 0m);
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST), false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5m, ZString.Empty, 2.4m);
		}
		#region TestPartCodeDefaults

		public void TestPartCodeDefaults()
		{
			var classHeader = GetClassHeader("3234567890");
			PrepareGlobalTariffData(Factory, "3234567890");
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "APART";
			part.OP_Desc = "A typical part";
			var relationImporter = part.RelatedOrganisations.AddNew();
			relationImporter.OU_OH = importer.PK;
			relationImporter.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var relationSupplier = part.RelatedOrganisations.AddNew();
			relationSupplier.OU_OH = supplier.PK;
			relationSupplier.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			var importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_TariffNum = classificationNumber1;
			importPivot.CCA_GSTStatusCode = "01";
			importPivot.CCA_ETExemption = "03";

			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_Tariff = "3234567890";
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_CustomsUnitQty = customsUnits1;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertExemptionCode(DutyAndTaxTypes.Codes.GST, "01");
			AssertExemptionCode(DutyAndTaxTypes.Codes.ExciseTax, "03");
		}

		void AssertExemptionCode(ZString type, ZString expectedValue)
		{
			var dutyAndTaxs = new List<DutyAndTax>(invoiceLine.DutiesAndTaxes.Find(tax => tax.C1_TaxType == type));
			AssertEquals("1 occurance of this tax/duty type", 1, dutyAndTaxs.Count);
			AssertEquals("Exemption code should be correct", expectedValue, dutyAndTaxs[0].C1_ExemptCode);
		}

		#endregion

		#region TestPopulateDutiesAndTaxes

		public void TestPopulateDutiesAndTaxes()
		{
			using (SetReciprocalFlagForCurrentCompany(true))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("ValueForCurrencyConversion", 50m, invoiceLine.DutyAndTaxManager.GetValueForCurrencyConversion());
				invoiceLine.CA_ADJCode = AmountTypes.Codes.Percent;
				AssertEquals("ValueForCurrencyConversion", 55m, invoiceLine.DutyAndTaxManager.GetValueForCurrencyConversion());
				invoiceLine.CA_ADJCode = AmountTypes.Codes.Dollar;
				AssertEquals("ValueForCurrencyConversion", 60m, invoiceLine.DutyAndTaxManager.GetValueForCurrencyConversion());

				AssertEquals("CustomsValueForDuty", 200m, invoiceLine.DutyAndTaxManager.GetCustomsValueForDuty());
				invoiceLine.CA_CVforCurrConvOvr = false;
				AssertEquals("CustomsValueForDuty", 600m, invoiceLine.DutyAndTaxManager.GetCustomsValueForDuty());
				invoiceLine.CA_CustomsValueOvr = false;
			}
			using (SetReciprocalFlagForCurrentCompany(false))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("CustomsValueForDuty is the same now that recirocal flag is not from company licence", 600m, invoiceLine.DutyAndTaxManager.GetCustomsValueForDuty());
			}

			using (SetReciprocalFlagForCurrentCompany(true))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				TestRemoveNotOverriddenDutiesAndTaxes();
				TestRemoveNotOverriddenDutiesForWarehouseDeclarations();
				TestRemoveNotOverriddenCasualImportProvincialTaxesForNormalBills();
				TestPopulateWithoutRefFiles();
				PrepareRefFiles();
				TestPopulateClassificationDutyRates();
				TestPopulateNormalDutiesFromTariffCode();
				TestPopulateExciseDutyRates();
				TestPopulateNormalTaxes();
				TestPopulateNormalTaxesWithInvalidClassHeader();
				TestRemissionCalculations();
				TestRemissionCalculationsFixRounding();
				TestPopulateCasualImportProvincialTaxes();
				//TODO INC: Fix in WI00208342 TestDutyPaidCalculations();
				TestDutyPaidCalculationsForLVS();
				TestPopulateNormalTaxesFallbackToLegacyClassHeader();
			}
		}

		#region TestRemoveNotOverriddenDutiesAndTaxes

		void TestRemoveNotOverriddenDutiesAndTaxes()
		{
			var tax1 = invoiceLine.DutiesAndTaxes.AddNew();
			tax1.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;

			var tax2 = invoiceLine.DutiesAndTaxes.AddNew();
			tax2.C1_Override = true;
			tax2.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("Should not delete taxes", true, invoiceLine.DutiesAndTaxes.Contains(tax1));
			AssertEquals("Should not delete overridden duties", true, invoiceLine.DutiesAndTaxes.Contains(tax2));

			tax1.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			tax2.C1_Override = false;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("Should not delete taxes", true, invoiceLine.DutiesAndTaxes.Contains(tax1));
			AssertEquals("Should delete not overridden duties", false, invoiceLine.DutiesAndTaxes.Contains(tax2));

			invoiceLine.DutiesAndTaxes.DeleteAll();
		}

		void TestRemoveNotOverriddenDutiesForWarehouseDeclarations()
		{
			Factory.ClearCachedValue<CACClassHeader>(string.Format("CACClassHeader_{0}_{1}_{2}", currentDate.ToShortDateString(), classificationNumber1, false));

			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = classificationNumber1;
			classHeader.ZA_EffectiveDate = ZDateTime.Today.AddYears(-1);
			classHeader.ZA_ExpiryDate = ZDateTime.Today.AddYears(1);

			invoiceLine.Declaration.JE_MessageSubType = B3EntryTypeList.Codes.Supplementary;
			var tax1 = invoiceLine.DutiesAndTaxes.AddNew();
			tax1.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			tax1.C1_UnitOfMeasure = "KGM";
			tax1.C1_Override = false;

			var tax2 = invoiceLine.DutiesAndTaxes.AddNew();
			tax2.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			tax2.C1_PreviousTranLine = 123;
			tax2.C1_Override = false;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals("Should delete duties", false, invoiceLine.DutiesAndTaxes.Contains(tax1));
			AssertEquals("Should not delete duties with previous transaction details", true, invoiceLine.DutiesAndTaxes.Contains(tax2));

			invoiceLine.Declaration.JE_MessageSubType = B3EntryTypeList.Codes.Postal;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals("Should delete duties with previous transaction details", false, invoiceLine.DutiesAndTaxes.Contains(tax2));

			classHeader.Delete();
			Factory.ClearCachedValue<CACClassHeader>(string.Format("CACClassHeader_{0}_{1}_{2}", currentDate.ToShortDateString(), classificationNumber1, false));
		}

		#endregion

		#region TestRemoveNotOverriddenCasualImportProvincialTaxesForNormalBills

		void TestRemoveNotOverriddenCasualImportProvincialTaxesForNormalBills()
		{
			var tax1 = invoiceLine.DutiesAndTaxes.AddNew();
			tax1.C1_TaxType = DutyAndTaxTypes.Codes.CPT;
			tax1.C1_Override = false;

			var tax2 = invoiceLine.DutiesAndTaxes.AddNew();
			tax2.C1_TaxType = DutyAndTaxTypes.Codes.CPT;
			tax2.C1_Override = true;

			var tax3 = invoiceLine.DutiesAndTaxes.AddNew();
			tax3.C1_TaxType = DutyAndTaxTypes.Codes.CTA;
			tax3.C1_Override = false;

			var tax4 = invoiceLine.DutiesAndTaxes.AddNew();
			tax4.C1_TaxType = DutyAndTaxTypes.Codes.CTA;
			tax4.C1_Override = true;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("Should delete taxes", false, invoiceLine.DutiesAndTaxes.Contains(tax1));
			AssertEquals("Should not delete overridden duties", true, invoiceLine.DutiesAndTaxes.Contains(tax2));
			AssertEquals("Should delete taxes", false, invoiceLine.DutiesAndTaxes.Contains(tax3));
			AssertEquals("Should not delete overridden duties", true, invoiceLine.DutiesAndTaxes.Contains(tax4));

			invoiceLine.DutiesAndTaxes.DeleteAll();
		}

		#endregion

		#region TestPopulateWithoutRefFiles

		void TestPopulateWithoutRefFiles()
		{
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("No duty should be populated when ref files empty", 0, invoiceLine.DutiesAndTaxes.Count);
			AssertHasMessageErrorContaining(invoiceLine.CA_CustomsValueInfo, "No details were found for the Classification Number.");
			AssertHasMessageErrorContaining(invoiceLine.CA_CustomsValueInfo, "No details were found for the Tariff Number.");
		}

		#endregion

		#region TestPopulateClassificationDutyRates

		void TestPopulateClassificationDutyRates()
		{
			AssertEquals("Pre-Condition: No duty should be populated", 0, invoiceLine.DutiesAndTaxes.Count);

			//ClassificationNumber is blank
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.CA_99TariffCode = ZString.Empty;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("DutiesAndTaxes count", 0, invoiceLine.DutiesAndTaxes.Count);
			AssertNoMessageErrorContaining(invoiceLine.CA_CustomsValueInfo, "No details were found for the Classification Number.");

			//No eligible Class CACRate row is found
			Factory.ClearCachedValue<CACClassHeader>(string.Format("CACClassHeader_{0}_{1}_{2}", currentDate.ToShortDateString(), classificationNumber1, false));
			invoiceLine.JI_Tariff = classificationNumber1;
			invoiceLine.JI_CustomsUnitQty = customsUnits1;
			invoiceHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.General;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertSIMADutyAndGeneralTaxPopulatedOnly();

			////No eligible Class CACRateLine rows found
			invoiceHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.Chile;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertSIMADutyAndGeneralTaxPopulatedOnly();

			//Free CACRateHeader row found
			invoiceLine.JI_Tariff = classificationNumber4;
			invoiceLine.JI_CustomsUnitQty = customsUnits2;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("DutiesAndTaxes count", 2, invoiceLine.DutiesAndTaxes.Count);
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty), false, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Free, 0, ZString.Empty, 0);
			AssertEquals("Free Classification duty rate.", invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty).AmountDescription);

			//CACRateLines found. Ad Valorem + Specific Rate Case
			invoiceLine.JI_Tariff = classificationNumber1;
			invoiceLine.JI_CustomsUnitQty = customsUnits1;
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.CA_CustomsValueOvr = true;
			invoiceLine.CA_CustomsValue = 1000;
			invoiceHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			Factory.Save();
			Factory.ClearCachedValue<CACRate>(string.Format("CACRate|{0}|{1}|{2}|{3}", classificationNumber1, TariffTreatmentCodes.Codes.UnitedStates, currentDate.ToShortDateString(), "CLS"));
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("DutiesAndTaxes count", 4, invoiceLine.DutiesAndTaxes.Count);
			AssertCount("SIMA duties count", DutyAndTaxTypes.Codes.SUR, 1);
			AssertCount("Duties count", DutyAndTaxTypes.Codes.CustomsDuty, 2);
			AssertCount("GST count", DutyAndTaxTypes.Codes.GST, 1);

			var customsDuties = invoiceLine.DutiesAndTaxes.Where(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty).ToList();
			AssertTax(customsDuties[0], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.7, ZString.Empty, 7);
			AssertTax(customsDuties[1], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 19, ZString.Empty, 190);

			AssertEquals("SUR Total Amount", 6m, invoiceLine.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.SUR));
			//NormalDutyPaidValue, Total Amount, Rate
			AssertEquals("NormalValueForTax", 1203m, invoiceLine.DutyAndTaxManager.NormalValueForTax);
			AssertEquals("CustomsDuty Total Amount", 197m, invoiceLine.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.CustomsDuty));
			AssertEquals("CustomsDuty Rate", 0.7m, invoiceLine.DutyAndTaxManager.GetRate(DutyAndTaxTypes.Codes.CustomsDuty));
			AssertEquals("CustomsDuty RateType", RateTypes.Codes.Specific, invoiceLine.DutyAndTaxManager.GetRateType(DutyAndTaxTypes.Codes.CustomsDuty));

			//CACRateLines found. Most Complex Case
			invoiceHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.NewZealand;
			invoiceLine.DutiesAndTaxes[0].C1_ExemptCode = SIMACodes.Codes.C31;
			invoiceLine.DutiesAndTaxes[0].C1_Amount = 72;

			//Regular between min/max
			invoiceLine.JI_CustomsQuantity = 1000;
			Factory.ClearCachedValue<CACRate>(string.Format("CACRate|{0}|{1}|{2}|{3}", classificationNumber1, TariffTreatmentCodes.Codes.NewZealand, currentDate.ToShortDateString(), "CLS"));
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("DutiesAndTaxes count", 4, invoiceLine.DutiesAndTaxes.Count);
			AssertCount("SIMA duties count", DutyAndTaxTypes.Codes.SUR, 1);
			AssertCount("Duties count", DutyAndTaxTypes.Codes.CustomsDuty, 2);
			AssertCount("GST count", DutyAndTaxTypes.Codes.GST, 1);
			customsDuties = invoiceLine.DutiesAndTaxes.Where(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty).ToList();
			AssertTax(customsDuties[0], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.0234, ZString.Empty, 23.4);
			AssertTax(customsDuties[1], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 5, ZString.Empty, 50);

			const string expectedDescription = "Classification duty rate. The quantity of the first unit of measure (1000KGM) is multiplied by the Specific rate (2.34¢/Kilogram)."
											   + " The customs value ($1000) is multiplied by the Ad Valorem rate (5%). The results are added. But not less than 4.74¢/Kilogram or not more than 9.48¢/Kilogram.";
			customsDuties = invoiceLine.DutiesAndTaxes.Where(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty).ToList();
			AssertEquals("AmountDescription", expectedDescription, customsDuties[0].AmountDescription);
			AssertEquals("AmountDescription", expectedDescription, customsDuties[1].AmountDescription);

			customsDuties[0].AmountDescription = customsDuties[1].AmountDescription = ZString.Empty;
			invoiceLine.DutyAndTaxManager.UpdateDutiesAmountDescriptions();

			customsDuties = invoiceLine.DutiesAndTaxes.Where(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty).ToList();
			AssertEquals("AmountDescription", expectedDescription, customsDuties[0].AmountDescription);
			AssertEquals("AmountDescription", expectedDescription, customsDuties[1].AmountDescription);

			//Regular greater than max
			invoiceLine.JI_CustomsQuantity = 500;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("DutiesAndTaxes count", 3, invoiceLine.DutiesAndTaxes.Count);
			AssertCount("SIMA duties count", DutyAndTaxTypes.Codes.SUR, 1);
			AssertCount("Duties count", DutyAndTaxTypes.Codes.CustomsDuty, 1);
			AssertCount("GST count", DutyAndTaxTypes.Codes.GST, 1);
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty), true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.0948, ZString.Empty, 47.4);

			//Regular less than min
			invoiceLine.JI_CustomsQuantity = 10000;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("DutiesAndTaxes count", 3, invoiceLine.DutiesAndTaxes.Count);
			AssertCount("SIMA duties count", DutyAndTaxTypes.Codes.SUR, 1);
			AssertCount("Duties count", DutyAndTaxTypes.Codes.CustomsDuty, 1);
			AssertCount("GST count", DutyAndTaxTypes.Codes.GST, 1);
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty), true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.0474, ZString.Empty, 474);

			//NormalDutyPaidValue, Total Amount, Rate
			AssertEquals("NormalValueForTax", 1480m, invoiceLine.DutyAndTaxManager.NormalValueForTax);
			AssertEquals("CustomsDuty Total Amount", 474m, invoiceLine.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.CustomsDuty));
			AssertEquals("CustomsDuty Rate", 0.0474m, invoiceLine.DutyAndTaxManager.GetRate(DutyAndTaxTypes.Codes.CustomsDuty));

			//No eligible Class CACRate row is found so existing general rate row is used
			invoiceLine.JI_Tariff = classificationNumber5;
			invoiceLine.JI_CustomsUnitQty = customsUnits1;
			invoiceHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.Australia;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("DutiesAndTaxes count", 2, invoiceLine.DutiesAndTaxes.Count);
			AssertCount("Duties count", DutyAndTaxTypes.Codes.CustomsDuty, 1);
			AssertCount("GST should be populated", DutyAndTaxTypes.Codes.GST, 1);
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty), true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 123, ZString.Empty, 1230);

			invoiceLine.DutiesAndTaxes.DeleteAll();
		}

		void AssertSIMADutyAndGeneralTaxPopulatedOnly()
		{
			AssertEquals("DutiesAndTaxes count", 3, invoiceLine.DutiesAndTaxes.Count);

			AssertCount("SIMA duties count", DutyAndTaxTypes.Codes.SUR, 1);
			var surTax = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SUR);
			AssertTax(surTax, false, DutyAndTaxTypes.Codes.SUR, ZString.Empty, false, RateTypes.Codes.AdValorem, 10m, ZString.Empty, 60.0m);

			AssertCount("General duty should be populated", DutyAndTaxTypes.Codes.CustomsDuty, 1);
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty), false, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 20, ZString.Empty, 120);
			AssertEquals("AmountDescription", "General Classification duty rate. The customs value ($600) is multiplied by the Ad Valorem rate (20%).", invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty).AmountDescription);
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST), false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5, ZString.Empty, 39);
		}

		#endregion

		#region TestPopulateNormalDutiesFromTariffCode

		void TestPopulateNormalDutiesFromTariffCode()
		{
			//Tariff Code is blank
			invoiceLine.JI_Tariff = classificationNumber1;
			invoiceLine.CA_99TariffCode = ZString.Empty;
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("Class duty should be calculated if tariff code empty", 3, invoiceLine.DutiesAndTaxes.Count);
			const string errorMessage = "No details were found for the Tariff Number.";
			AssertNoMessageErrorContaining(invoiceLine.CA_CustomsValueInfo, errorMessage);

			//Free CACTariffHeader row found
			invoiceLine.CA_99TariffCode = tariffCode3;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("DutiesAndTaxes count", 2, invoiceLine.DutiesAndTaxes.Count);
			AssertNoMessageErrorContaining(invoiceLine.CA_CustomsValueInfo, errorMessage);

			//EffectiveDutyDate is not within the RateEffectiveDate and RateExpiryDate
			invoiceLine.CA_99TariffCode = tariffCode2;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("DutiesAndTaxes count", 2, invoiceLine.DutiesAndTaxes.Count);
			AssertNoMessageErrorContaining(invoiceLine.CA_CustomsValueInfo, errorMessage);

			//No eligible tariff CACRate row found
			invoiceLine.CA_99TariffCode = tariffCode1;
			invoiceHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.Chile;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("Class duty should be calculated if tariff rate not found", 3, invoiceLine.DutiesAndTaxes.Count);
			AssertHasMessageErrorContaining(invoiceLine.CA_CustomsValueInfo, errorMessage);

			//CACRates found
			invoiceLine.CA_99TariffCode = tariffCode1;
			invoiceHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.NewZealand;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("DutiesAndTaxes count", 3, invoiceLine.DutiesAndTaxes.Count);
			AssertCount("SIMA duties count", DutyAndTaxTypes.Codes.SUR, 1);
			AssertCount("CustomsDuty count", DutyAndTaxTypes.Codes.CustomsDuty, 1);
			AssertCount("GST count", DutyAndTaxTypes.Codes.GST, 1);
			var dty = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty);
			AssertTax(dty, true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.352, ZString.Empty, 3.52);
			AssertEquals("AmountDescription", "Tariff duty rate. The quantity of the first unit of measure (10) is multiplied by the Specific rate (35.2¢/Piece).", dty.AmountDescription);
		}

		#endregion

		#region TestPopulateExciseDutyRates

		void TestPopulateExciseDutyRates()
		{
			invoiceLine.CA_99TariffCode = ZString.Empty;
			invoiceLine.JI_Tariff = classificationNumber2;
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_CustomsUnitQty = customsUnits1;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("DutiesAndTaxes count", 5, invoiceLine.DutiesAndTaxes.Count);
			AssertCount("Duties count", DutyAndTaxTypes.Codes.CustomsDuty, 4);
			var duties = invoiceLine.DutiesAndTaxes.Where(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty).ToList();
			AssertTax(duties[0], false, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 20, ZString.Empty, 200); //General
			AssertTax(duties[1], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Free, 0, customsUnits1, 0);
			AssertTax(duties[2], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AcceptX, 0, customsUnits1, 0);
			AssertTax(duties[3], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AcceptT, 0, customsUnits1, 0);

			AssertHasWarning(duties[0].C1_RateInfo, "No duty rate found so the default general rate of duty has been used");
			AssertHasMessageErrorContaining(duties[2].C1_AmountInfo, "Type X specified so you must override and enter the Rate and Amount manually");
			AssertHasMessageErrorContaining(duties[3].C1_AmountInfo, "Type T specified so you must override and enter the Rate and Amount manually");
			AssertEquals("Excise duty rate. Free. Accept (X), you must override. Accept (T), you must override. The results are added.", duties[2].AmountDescription);

			//NormalDutyPaidValue, Total Amount, Exempt Code, Rate
			var duty = invoiceLine.DutiesAndTaxes[0];
			duty.C1_Override = true;
			duty.C1_ExemptCode = SIMACodes.Codes.C52;
			duty.C1_Amount = 180;
			var tax = invoiceLine.DutiesAndTaxes.AddNew();
			tax.C1_Override = true;
			tax.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			tax.C1_ExemptCode = SIMACodes.Codes.C51;
			tax.C1_Amount = 123;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("SIMADuty Total Amount", 123m, invoiceLine.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.SIMADuty));
			AssertEquals("SIMADuty Exempt Code", SIMACodes.Codes.C51, invoiceLine.DutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.SIMADuty));

			AssertEquals("NormalValueForTax", 1323m, invoiceLine.DutyAndTaxManager.NormalValueForTax);
			AssertEquals("CustomsDuty Total Amount", 200m, invoiceLine.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.CustomsDuty));
			AssertEquals("CustomsDuty Rate", 20m, invoiceLine.DutyAndTaxManager.GetRate(DutyAndTaxTypes.Codes.CustomsDuty));

			var overridedDutyAndTax = invoiceLine.DutiesAndTaxes.Where(x => x.C1_Override);
			overridedDutyAndTax.ForEach(x => invoiceLine.DutiesAndTaxes.Delete(x));
		}

		#endregion

		#region TestPopulateNormalTaxes

		void TestPopulateNormalTaxes()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType3 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.ExciseTax);
			var rateCode6 = universalHelper.LoadOrCreateNewCusRateCode(Factory, "BB2", rateType3.PK);
			var taxRate8 = universalHelper.CreateRate(countervailingRelTariff, rateCode6.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "22*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);

			CACustomsDataRegistry.Instance.DefaultToThisExciseTaxRateCodeWhenApplicable.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "BB2");
			invoiceLine.JI_Tariff = classificationNumber3;
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_CustomsUnitQty = customsUnits1;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			var duty = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty);
			AssertTax(duty, false, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 20, ZString.Empty, 200);

			invoiceLine.DutiesAndTaxes[0].C1_ExemptCode = SIMACodes.Codes.C51;
			invoiceLine.Factory.Save();
			AssertEquals("NormalValueForTax", 1464m, invoiceLine.DutyAndTaxManager.NormalValueForTax);
			AssertEquals("CalculatedValueForTax:", 1464m, invoiceLine.CA_ValueForTax);

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var declarationInNewfactory = newFactory.Load<JobDeclaration>(invoiceLine.Declaration.PK);
			AssertEquals("NormalValueForTax not calculated at this time", 464m, declarationInNewfactory.InvoiceLines[0].DutyAndTaxManager.NormalValueForTax);
			AssertEquals("CalculatedValueForTax:", 1464m, declarationInNewfactory.InvoiceLines[0].CA_ValueForTax);

			//AssertEquals("Duty should be reused", duty, invoiceLine.DutiesAndTaxes[1]);
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CVD), false, DutyAndTaxTypes.Codes.CVD, ZString.Empty, false, RateTypes.Codes.Specific, 200m, "KGM", 2000);
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST), false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5, ZString.Empty, 0);
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty), false, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 20, ZString.Empty, 200); //General
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax), false, DutyAndTaxTypes.Codes.ExciseTax, "BB2", false, RateTypes.Codes.AdValorem, 22, customsUnits1, 264m);

			//Update data when override canceled
			var exices = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax);
			exices.C1_Override = true;
			exices.C1_Rate = 12;
			exices.C1_Amount = 220;
			exices.C1_Override = false;
			AssertTax(exices, false, DutyAndTaxTypes.Codes.ExciseTax, "BB2", false, RateTypes.Codes.AdValorem, 22, customsUnits1, 264m);

			//Clear amount when rate type is 'E'
			exices = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax);
			exices.C1_Code = "BB1";
			AssertTax(exices, false, DutyAndTaxTypes.Codes.ExciseTax, "BB1", false, RateTypes.Codes.Exempt, 0, customsUnits1, 0);

			//If code not found during update
			exices.C1_ExemptCode = ZString.Empty;
			exices.C1_Override = true;
			exices.C1_Code = "BB3";
			exices.C1_Amount = 303.6;
			exices.C1_Override = false;
			exices.AddInfoValidation.ValidateC1_Code();
			AssertHasMessageErrorContaining(exices.C1_CodeInfo, "No details were found for current tax so you have to override this line");
			AssertEquals("Amount should be zero because rate not found", 0m, exices.C1_Amount);

			//should revert back to nornmal now for rest of test
			exices.C1_Code = "BB2";
			AssertTax(exices, false, DutyAndTaxTypes.Codes.ExciseTax, "BB2", true, RateTypes.Codes.AdValorem, 2200, "", 26400);

			//NormalValueForTax, Total Amount, Exempt Code, Rate

			AssertEquals("NormalValueForTax", 1464m, invoiceLine.DutyAndTaxManager.NormalValueForTax);

			AssertEquals("CustomsDuty Total Amount", 200m, invoiceLine.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.CustomsDuty));
			AssertEquals("CustomsDuty Rate", 20m, invoiceLine.DutyAndTaxManager.GetRate(DutyAndTaxTypes.Codes.CustomsDuty));
			AssertEquals("CustomsDuty RateType", RateTypes.Codes.AdValorem, invoiceLine.DutyAndTaxManager.GetRateType(DutyAndTaxTypes.Codes.CustomsDuty));

			AssertEquals("SIMADuty Total Amount", 2000m, invoiceLine.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.SIMADuty));
			AssertEquals("SIMADuty Exempt Code", ZString.Empty, invoiceLine.DutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.SIMADuty));

			AssertEquals("GST Total Amount", 0m, invoiceLine.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.GST));
			invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST).C1_ExemptCode = GSTStatusCodes.Codes.C81;
			AssertEquals("GST Exempt Code", GSTStatusCodes.Codes.C81, invoiceLine.DutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.GST));
			AssertEquals("GST Rate", 5m, invoiceLine.DutyAndTaxManager.GetRate(DutyAndTaxTypes.Codes.GST));
			AssertEquals("GST RateType", RateTypes.Codes.AdValorem, invoiceLine.DutyAndTaxManager.GetRateType(DutyAndTaxTypes.Codes.GST));
			AssertEquals("GST AmountDescription", "The normal value for tax ($1464) is multiplied by the Ad Valorem rate (5%).", invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST).AmountDescription);

			AssertEquals("ExciseTax Total Amount", 26400m, invoiceLine.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.ExciseTax));
			invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax).C1_Override = false;
			invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax).C1_ExemptCode = ExciseTaxExemptionCodes.Codes.C99;
			AssertEquals("ExciseTax Exempt Code", ExciseTaxExemptionCodes.Codes.C99, invoiceLine.DutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.ExciseTax));
			AssertEquals("ExciseTax Rate", 22m, invoiceLine.DutyAndTaxManager.GetRate(DutyAndTaxTypes.Codes.ExciseTax));
			AssertEquals("ExciseTax RateType", RateTypes.Codes.AdValorem, invoiceLine.DutyAndTaxManager.GetRateType(DutyAndTaxTypes.Codes.ExciseTax));
			AssertEquals("ExciseTax Total Amount should now be zero", 0m, invoiceLine.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.ExciseTax));
		}

		#endregion

		#region TestPopulateNormalTaxesWithInvalidClassHeader

		void TestPopulateNormalTaxesWithInvalidClassHeader()
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = "1123456789";
			classHeader.ZA_EffectiveDate = ZDateTime.Today.AddYears(-1);
			classHeader.ZA_ExpiryDate = ZDateTime.Today.AddYears(1);

			var refNumHeader = Factory.New<CACTaxRefNumHeader>();
			refNumHeader.ZD_ZA_ClassNumber = classHeader.PK;
			refNumHeader.ZD_EffectiveDate = ZDateTime.Today.AddYears(-1);
			refNumHeader.ZD_ExpiryDate = ZDateTime.Today.AddYears(1);

			var refNum1 = refNumHeader.RefNumbers.AddNew();
			refNum1.ZE_GSTRefNumber = "AA3";

			invoiceLine.JI_Tariff = classHeader.ZA_ClassificationNumber;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Bag;
			invoiceLine.JI_CustomsQuantity = 1000;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals("One GST row", 1, invoiceLine.DutiesAndTaxes.Find(tax => tax.C1_TaxType == DutyAndTaxTypes.Codes.GST).Count(tax => !tax.C1_Override));
			AssertEquals("One Duty row", 1, invoiceLine.DutiesAndTaxes.Find(tax => tax.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty).Count(tax => !tax.C1_Override));
			AssertEquals("One EXS row", 1, invoiceLine.DutiesAndTaxes.Find(tax => tax.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax).Count(tax => !tax.C1_Override));

			Factory.ClearCachedValue<CACClassHeader>(string.Format("CACClassHeader_{0}_{1}_{2}", currentDate.ToShortDateString(), classHeader.ZA_ClassificationNumber, false));
			classHeader.ZA_EffectiveDate = ZDateTime.Today.AddYears(10);
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals("Should be deleted", 0, invoiceLine.DutiesAndTaxes.Find(tax => tax.C1_TaxType == DutyAndTaxTypes.Codes.GST).Count(tax => !tax.C1_Override));
			AssertEquals("Should be deleted", 0, invoiceLine.DutiesAndTaxes.Find(tax => tax.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty).Count(tax => !tax.C1_Override));
			AssertEquals("Should be deleted", 0, invoiceLine.DutiesAndTaxes.Find(tax => tax.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax).Count(tax => !tax.C1_Override));
		}

		void TestPopulateNormalTaxesFallbackToLegacyClassHeader()
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = "1123456789";
			classHeader.ZA_EffectiveDate = new ZDateTime(2024, 10, 04);
			classHeader.ZA_ExpiryDate = new ZDateTime(2079, 06, 06);
			classHeader.ZA_AreaCode = "XXX";

			var legacyClassHeader = Factory.New<CACClassHeader>();
			legacyClassHeader.ZA_ClassificationNumber = "1123456789";
			legacyClassHeader.ZA_EffectiveDate = new ZDateTime(2023, 10, 03);
			legacyClassHeader.ZA_ExpiryDate = new ZDateTime(2024, 10, 03, 23, 59, 00);

			var refNumHeader = Factory.New<CACTaxRefNumHeader>();
			refNumHeader.ZD_ZA_ClassNumber = legacyClassHeader.PK;
			refNumHeader.ZD_EffectiveDate = new ZDateTime(2023, 10, 03);
			refNumHeader.ZD_ExpiryDate = new ZDateTime(2079, 06, 06);

			var refNum1 = refNumHeader.RefNumbers.AddNew();
			refNum1.ZE_GSTRefNumber = "AA3";

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 12, 04);
			invoiceLine.JI_Tariff = classHeader.ZA_ClassificationNumber;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Bag;
			invoiceLine.JI_CustomsQuantity = 1000;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals("One GST row", 1, invoiceLine.DutiesAndTaxes.Find(tax => tax.C1_TaxType == DutyAndTaxTypes.Codes.GST).Count(tax => !tax.C1_Override));
			AssertEquals("One Duty row", 1, invoiceLine.DutiesAndTaxes.Find(tax => tax.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty).Count(tax => !tax.C1_Override));

			declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
		}

		#endregion

		#region TestRemissionCalculations

		void TestRemissionCalculationsFixRounding()
		{
			invoiceLine.CA_CustomsValue = 252.7555m;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.OneSixtiethRemission;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertTax(invoiceLine.DutiesAndTaxes[3], false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5, ZString.Empty, 0.76);
		}

		void TestRemissionCalculations()
		{
			invoiceLine.DutiesAndTaxes.DeleteAll();
			var tax = invoiceLine.DutiesAndTaxes.AddNew();
			tax.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			tax.C1_Override = true;
			tax.C1_ExemptCode = SIMACodes.Codes.C31;
			tax.C1_Rate = 200m;
			tax.C1_RateType = RateTypes.Codes.Specific;
			tax.C1_UnitOfMeasure = "KGM";
			tax.C1_Amount = 180;

			invoiceLine.JI_Tariff = classificationNumber3;
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_CustomsUnitQty = customsUnits1;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.OneSixtiethRemission;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertRemissionAmounts(2000m, 10, 35.2m, 9.76m, 3904m, true);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertRemissionAmounts(2000m, 0, 660m, 183m, 3660m, true);

			invoiceLine.CA_AuthorityNumber = "85-2955";
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertRemissionAmounts(2000m, 0, 0, 0, 3000m, true);

			invoiceLine.DutiesAndTaxes.DeleteAll();
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.DutyDeferral;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertRemissionAmounts(2000m, 200m, 264m, 73.2m, 1464m, false);

			var childLine = Factory.Load<JobComInvoiceLine>(new ZQuery(JobComInvoiceLineSchema.JI_ParentID, invoiceLine.PK)).First();
			childLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals(4, childLine.DutiesAndTaxes.Count);
			childLine.DutiesAndTaxes.ApplySort(DutyAndTax.Schema.C1_TaxType, ListSortDirection.Ascending);
			AssertTax(childLine.DutiesAndTaxes[0], false, DutyAndTaxTypes.Codes.CVD, ZString.Empty, false, RateTypes.Codes.Specific, 200, "KGM", 0m);
			AssertTax(childLine.DutiesAndTaxes[1], false, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 20, ZString.Empty, 0m);
			AssertTax(childLine.DutiesAndTaxes[2], false, DutyAndTaxTypes.Codes.ExciseTax, "BB2", false, RateTypes.Codes.AdValorem, 22, customsUnits1, 0m);
			AssertTax(childLine.DutiesAndTaxes[3], false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5, ZString.Empty, 2.93m);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			invoiceLine.DutiesAndTaxes.DeleteAll();
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RepairsRemission;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertRemissionAmounts(0, 0, 0, 0, 1000m, false);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			invoiceLine.DutiesAndTaxes.DeleteAll();
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.SpiritsRemission;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			var exciseDuty = invoiceLine.DutiesAndTaxes.FirstOrDefault(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax);
			AssertEquals("94", exciseDuty.C1_ExemptCode);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.GiftsUpTo60;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertRemissionAmounts(2000m, 188m, 248.16m, 68.81m, 1436.16m, false);
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.WarrantyRepairsRemission;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertRemissionAmounts(0, 0, 0, 0, 1000m, false);
		}

		void AssertRemissionAmounts(decimal simaAmount, decimal dutyAmount, decimal exciseTaxAmount, decimal gstAmount, decimal valueForTax, bool simaDutyOverride)
		{
			ActiveBusinessObjectCollection.RefreshAll(Factory);
			AssertEquals("DutiesAndTaxes count", 4, invoiceLine.DutiesAndTaxes.Count);
			invoiceLine.DutiesAndTaxes.ApplySort(DutyAndTax.Schema.C1_TaxType, ListSortDirection.Ascending);
			AssertTax(invoiceLine.DutiesAndTaxes[0], false, DutyAndTaxTypes.Codes.CVD, ZString.Empty, simaDutyOverride, RateTypes.Codes.Specific, 200, "KGM", simaAmount);
			AssertTax(invoiceLine.DutiesAndTaxes[1], false, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 20, ZString.Empty, dutyAmount);
			AssertTax(invoiceLine.DutiesAndTaxes[2], false, DutyAndTaxTypes.Codes.ExciseTax, "BB2", false, RateTypes.Codes.AdValorem, 22, customsUnits1, exciseTaxAmount);
			AssertTax(invoiceLine.DutiesAndTaxes[3], false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5, ZString.Empty, gstAmount);

			AssertEquals("NormalValueForTax", valueForTax, invoiceLine.DutyAndTaxManager.NormalValueForTax);
		}

		#endregion

		#region TestPopulateCasualImportProvincialTaxes

		void TestPopulateCasualImportProvincialTaxes()
		{
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.WarrantyRepairsRemission;
			invoiceLine.DutiesAndTaxes.DeleteAll();
			invoiceLine.CA_IsCasualImport = true;

			invoiceLine.CA_CustomsValueOvr = true;
			invoiceLine.CA_CustomsValue = 100m;
			invoiceLine.JI_CustomsQuantity = 2;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Number;
			invoiceLine.JI_CustomsSecondQuantity = 1;
			invoiceLine.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;

			declaration.JE_CustomsOffice = portCode1;
			invoiceLine.CA_CasualImportDestinationProvince = provinceCodeNT;
			invoiceLine.CA_CasualImportCommodity = commodityCodeCigars;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 0);
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 0);

			declaration.JE_CustomsOffice = portCode2;
			invoiceLine.CA_CasualImportDestinationProvince = provinceCodeNL;
			invoiceLine.CA_CasualImportCommodity = commodityCodeCigars;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 1);
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 0);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT), false, DutyAndTaxTypes.Codes.CPT, ZString.Empty, false, RateTypes.Codes.AdValorem, 15, ZString.Empty, 15);
			var hstApplied = invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.GST);
			AssertEquals("GST have Exemption Code 99 because HST is applied.", ExciseTaxExemptionCodes.Codes.C99, hstApplied.C1_ExemptCode);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				invoiceLine.CA_CalculationMethod = ZString.Empty;
				invoiceLine.CA_CasualImportDestinationProvince = CanadianProvinceList.Codes.YukonTerritory;
				invoiceLine.CA_CasualImportCommodity = commodityCodeCigars;
				invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
				AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 1);
				AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 0);
				AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT), false, DutyAndTaxTypes.Codes.CPT, ZString.Empty, false, RateTypes.Codes.AdValorem, 16, ZString.Empty, 23.42);
				hstApplied = invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.GST);
				AssertEquals("GST have Exemption Code 99 because HST is applied.", ExciseTaxExemptionCodes.Codes.C99, hstApplied.C1_ExemptCode);
				AssertEquals(0m, hstApplied.C1_Amount);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				hstApplied.C1_ExemptCode = ZString.Empty;
				invoiceLine.CA_CalculationMethod = ZString.Empty;
				invoiceLine.CA_CasualImportDestinationProvince = CanadianProvinceList.Codes.YukonTerritory;
				invoiceLine.CA_CasualImportCommodity = commodityCodeCigars;
				invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
				AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 1);
				AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 0);
				AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT), false, DutyAndTaxTypes.Codes.CPT, ZString.Empty, false, RateTypes.Codes.AdValorem, 11, ZString.Empty, 16.10);
				hstApplied = invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.GST);
				AssertEquals("GST does not have Exemption Code 99 when CAD is enabled.", ZString.Empty, hstApplied.C1_ExemptCode);
				AssertEquals(7.32m, hstApplied.C1_Amount);
			}

			declaration.JE_CustomsOffice = portCode3;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.WarrantyRepairsRemission;
			invoiceLine.CA_CasualImportDestinationProvince = provinceCodeAB;
			invoiceLine.CA_CasualImportCommodity = commodityCodeTabSticks;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 0);
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 1);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA), false, DutyAndTaxTypes.Codes.CTA, ZString.Empty, false, RateTypes.Codes.Specific, 0.25, CustomsUnitOfMeasureList.Codes.Number, 0.5);

			declaration.JE_CustomsOffice = portCode4;
			invoiceLine.CA_CasualImportDestinationProvince = provinceCodeNL;
			invoiceLine.CA_CasualImportCommodity = commodityCodeSpirits;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 1);
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 1);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT), false, DutyAndTaxTypes.Codes.CPT, ZString.Empty, false, RateTypes.Codes.AdValorem, 15, ZString.Empty, 15);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA), false, DutyAndTaxTypes.Codes.CTA, ZString.Empty, false, RateTypes.Codes.Specific, 0.15, CustomsUnitOfMeasureList.Codes.Ounce, 0.05);

			declaration.JE_CustomsOffice = portCode5;
			invoiceLine.CA_CasualImportDestinationProvince = provinceCodeMB;
			invoiceLine.CA_CasualImportCommodity = commodityCodeCigarettes;
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 1);
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 1);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT), false, DutyAndTaxTypes.Codes.CPT, ZString.Empty, false, RateTypes.Codes.AdValorem, 7, ZString.Empty, 7.41);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA), false, DutyAndTaxTypes.Codes.CTA, ZString.Empty, false, RateTypes.Codes.Specific, 0.295, CustomsUnitOfMeasureList.Codes.Number, 5.9);
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 1);
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 0);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT), false, DutyAndTaxTypes.Codes.CPT, ZString.Empty, false, RateTypes.Codes.AdValorem, 7, ZString.Empty, 7);

			invoiceLine.CA_CasualImportDestinationProvince = provinceCodeON;
			invoiceLine.CA_CasualImportCommodity = "";
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 1);
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 0);
			AssertEquals("GST Tax has an Exemption Code 99 because HST is applied.", ExciseTaxExemptionCodes.Codes.C99, invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.GST).C1_ExemptCode);
			AssertEquals("0 GST Amount because of the exempt Code 99", 0m, invoiceLine.JI_Calc_GSTVATAmountIncludingWHEstimate);

			declaration.JE_CustomsOffice = portCode6;
			invoiceLine.CA_CasualImportDestinationProvince = provinceCodeBC;
			invoiceLine.CA_CasualImportCommodity = commodityCodeCider;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 1);
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 1);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT), false, DutyAndTaxTypes.Codes.CPT, ZString.Empty, false, RateTypes.Codes.AdValorem, 10, ZString.Empty, 10); // BC has both PSTA 10% and PST 7%. PSTA has a priority in case commodity entered.
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA), false, DutyAndTaxTypes.Codes.CTA, ZString.Empty, false, RateTypes.Codes.AdValorem, 70, ZString.Empty, 70);

			declaration.JE_CustomsOffice = portCode7;
			invoiceLine.CA_CasualImportDestinationProvince = provinceCodeAB;
			invoiceLine.CA_CasualImportCommodity = commodityCodeCigars;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 1);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA), false, DutyAndTaxTypes.Codes.CTA, ZString.Empty, false, RateTypes.Codes.AdValorem, 129, ZString.Empty, 129);

			declaration.JE_CustomsOffice = portCode9;
			invoiceLine.CA_CasualImportDestinationProvince = provinceCodeAB;
			invoiceLine.CA_CasualImportCommodity = commodityCodeSparklingWine;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 0);
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 1);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA && dutyAndTax.C1_RateType == RateTypes.Codes.Specific), false, DutyAndTaxTypes.Codes.CTA, ZString.Empty, false, RateTypes.Codes.Specific, 0.15, CustomsUnitOfMeasureList.Codes.Ounce, 1.06);

			declaration.JE_CustomsOffice = portCode8;
			invoiceLine.CA_CasualImportDestinationProvince = provinceCodeQC;
			invoiceLine.CA_CasualImportCommodity = commodityCodeSparklingWine;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 1);
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 2);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT), false, DutyAndTaxTypes.Codes.CPT, ZString.Empty, false, RateTypes.Codes.AdValorem, 9.975, ZString.Empty, 9.98); // QC has PST 9.975%  and GST 5%
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA && dutyAndTax.C1_RateType == RateTypes.Codes.AdValorem), false, DutyAndTaxTypes.Codes.CTA, ZString.Empty, false, RateTypes.Codes.AdValorem, 72, ZString.Empty, 72);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA && dutyAndTax.C1_RateType == RateTypes.Codes.Specific), false, DutyAndTaxTypes.Codes.CTA, ZString.Empty, false, RateTypes.Codes.Specific, 1.12, CustomsUnitOfMeasureList.Codes.Litre, 0.22);

			var ctaTaxV = invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA && dutyAndTax.C1_RateType == RateTypes.Codes.AdValorem);
			ctaTaxV.C1_Override = true;
			ctaTaxV.C1_Rate = 0;
			ctaTaxV.C1_Amount = 11;
			var ctaTaxS = invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA && dutyAndTax.C1_RateType == RateTypes.Codes.Specific);
			ctaTaxS.C1_Override = true;
			ctaTaxS.C1_Rate = 0;
			ctaTaxS.C1_Amount = 22;
			var gstTax = invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.GST);
			var gstTaxCode = gstTax.C1_Code;
			var gstTaxRate = gstTax.C1_Rate;
			var gstTaxRateType = gstTax.C1_RateType;
			var gstTaxUOM = gstTax.C1_UnitOfMeasure;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 1);
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 2);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT), false, DutyAndTaxTypes.Codes.CPT, ZString.Empty, false, RateTypes.Codes.AdValorem, 9.975, ZString.Empty, 9.98);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA && dutyAndTax.C1_RateType == RateTypes.Codes.AdValorem), false, DutyAndTaxTypes.Codes.CTA, ZString.Empty, true, RateTypes.Codes.AdValorem, 0, ZString.Empty, 11);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA && dutyAndTax.C1_RateType == RateTypes.Codes.Specific), false, DutyAndTaxTypes.Codes.CTA, ZString.Empty, true, RateTypes.Codes.Specific, 0, CustomsUnitOfMeasureList.Codes.Litre, 22);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.GST), false, DutyAndTaxTypes.Codes.GST, gstTaxCode, false, gstTaxRateType, gstTaxRate, ZString.Empty, 0);

			invoiceLine.DutiesAndTaxes.DeleteAll();
			declaration.JE_CustomsOffice = portCode10;
			invoiceLine.CA_CasualImportDestinationProvince = provinceCodeQC;
			invoiceLine.CA_CasualImportCommodity = commodityCodeTabSticks;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Number;
			invoiceLine.JI_CustomsSecondQuantity = 1;
			invoiceLine.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.Gram;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 1);
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 1);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT), false, DutyAndTaxTypes.Codes.CPT, ZString.Empty, false, RateTypes.Codes.AdValorem, 9.975, ZString.Empty, 9.98);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA), false, DutyAndTaxTypes.Codes.CTA, ZString.Empty, false, RateTypes.Codes.Specific, 0.149, CustomsUnitOfMeasureList.Codes.Number, 2.98);

			invoiceLine.JI_CustomsSecondQuantity = 30;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 1);
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 1);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT), false, DutyAndTaxTypes.Codes.CPT, ZString.Empty, false, RateTypes.Codes.AdValorem, 9.975, ZString.Empty, 9.98);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA), false, DutyAndTaxTypes.Codes.CTA, ZString.Empty, false, RateTypes.Codes.Specific, 0.2292, CustomsUnitOfMeasureList.Codes.Gram, 6.88);

			declaration.JE_CustomsOffice = portCode11;
			invoiceLine.CA_CasualImportDestinationProvince = provinceCodeSK;
			invoiceLine.CA_CasualImportCommodity = commodityCodeCigars;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 1);
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 1);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT), false, DutyAndTaxTypes.Codes.CPT, ZString.Empty, false, RateTypes.Codes.AdValorem, 6, ZString.Empty, 12.0);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA), false, DutyAndTaxTypes.Codes.CTA, ZString.Empty, false, RateTypes.Codes.AdValorem, 100, ZString.Empty, 100);

			invoiceLine.JI_CustomsQuantity = 2;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 1);
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 1);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT), false, DutyAndTaxTypes.Codes.CPT, ZString.Empty, false, RateTypes.Codes.AdValorem, 6, ZString.Empty, 6.60);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA), false, DutyAndTaxTypes.Codes.CTA, ZString.Empty, false, RateTypes.Codes.Specific, 5, CustomsUnitOfMeasureList.Codes.Number, 10);

			invoiceLine.JI_CustomsQuantity = 1000;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 1);
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 1);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT), false, DutyAndTaxTypes.Codes.CPT, ZString.Empty, false, RateTypes.Codes.AdValorem, 6, ZString.Empty, 27.00);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA), false, DutyAndTaxTypes.Codes.CTA, ZString.Empty, false, RateTypes.Codes.Specific, 0.35, CustomsUnitOfMeasureList.Codes.Number, 350);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;
			declaration.JE_CustomsOffice = portCode8;
			invoiceLine.CA_CasualImportDestinationProvince = provinceCodeQC;
			invoiceLine.CA_CasualImportCommodity = commodityCodeSparklingWine;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 1);
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 2);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT), false, DutyAndTaxTypes.Codes.CPT, ZString.Empty, false, RateTypes.Codes.AdValorem, 9.975, ZString.Empty, 9.98);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA && dutyAndTax.C1_RateType == RateTypes.Codes.AdValorem), false, DutyAndTaxTypes.Codes.CTA, ZString.Empty, false, RateTypes.Codes.AdValorem, 72, ZString.Empty, 72);
			AssertTax(invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA && dutyAndTax.C1_RateType == RateTypes.Codes.Specific), false, DutyAndTaxTypes.Codes.CTA, ZString.Empty, false, RateTypes.Codes.Specific, 1.12, CustomsUnitOfMeasureList.Codes.Litre, 0.22);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			invoiceLine.DutiesAndTaxes.DeleteAll();
			invoiceLine.CA_IsCasualImport = false;
		}

		public void TestPopulateCasualImportProvincialTaxesForAlcohol()
		{
			invoiceLine.DutiesAndTaxes.DeleteAll();
			invoiceLine.CA_IsCasualImport = true;

			invoiceLine.CA_CustomsValueOvr = true;
			invoiceLine.CA_CustomsValue = 100m;
			invoiceLine.JI_CustomsQuantity = 2;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Number;
			invoiceLine.JI_CustomsSecondQuantity = 1;
			invoiceLine.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;

			declaration.JE_CustomsOffice = portCode5;
			invoiceLine.CA_CasualImportDestinationProvince = provinceCodeMB;
			invoiceLine.CA_CasualImportCommodity = commodityCodeSparklingWine;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CTA Taxes count", DutyAndTaxTypes.Codes.CTA, 1);
		}
		#endregion

		#region TestUpdatePSTTaxRateNotExempt

		public void TestUpdatePSTTaxRateNotExempt()
		{
			invoiceLine.DutiesAndTaxes.DeleteAll();
			invoiceLine.CA_IsCasualImport = true;
			invoiceLine.CA_CustomsValueOvr = true;
			invoiceLine.CA_CustomsValue = 100m;
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Number;
			invoiceLine.JI_CustomsSecondQuantity = 1;
			invoiceLine.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;

			declaration.JE_CustomsOffice = portCode5;
			invoiceLine.CA_CasualImportDestinationProvince = provinceCodeMB;
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertCount("CPT Taxes count", DutyAndTaxTypes.Codes.CPT, 1);

			var pstTax = invoiceLine.DutiesAndTaxes.FirstOrDefault(dutyAndTax => dutyAndTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT);
			AssertTax(pstTax, false, DutyAndTaxTypes.Codes.CPT, ZString.Empty, false, RateTypes.Codes.AdValorem, 7, ZString.Empty, 7);
		}

		#endregion

		#region TestDutyPaidCalculations

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TODO: This method is not yet completed")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1144:Do Not Use Private Test Method", Justification = "WI00649448")]
		void TestDutyPaidCalculations()
		{
			invoiceHeader.JZ_InvoiceCurrExRate = 1;
			invoiceHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			invoiceLine.DutiesAndTaxes.DeleteAll();
			invoiceLine.DutiesAndTaxes.RemoveSort();
			invoiceLine.CA_CustomsValueOvr = false;
			invoiceLine.CA_ADJValue = 0;
			invoiceLine.JI_Tariff = classificationNumber1;
			invoiceLine.JI_CustomsUnitQty = customsUnits1;

			//Ad Valorem + Specific Rate
			invoiceLine.JI_CustomsQuantity = 1000;
			invoiceLine.JI_LinePrice = 1890;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals("Pre-Condition: Customs Value", 1890m, invoiceLine.CA_CustomsValue);
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("Customs Value", 1000m, invoiceLine.CA_CustomsValue);
			AssertEquals("Customs Value for Currency Conversion", 1000m, invoiceLine.CA_CVforCurrConv);
			AssertEquals("DutiesAndTaxes count", 3, invoiceLine.DutiesAndTaxes.Count);
			AssertCount("SIMA duties count", DutyAndTaxTypes.Codes.SUR, 1);
			AssertCount("Duties count", DutyAndTaxTypes.Codes.CustomsDuty, 2);
			AssertTax(invoiceLine.DutiesAndTaxes[1], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.7, ZString.Empty, 700);
			AssertTax(invoiceLine.DutiesAndTaxes[2], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 19, ZString.Empty, 190);

			//NormalDutyPaidValue, Total Amount, Rate
			AssertEquals("NormalValueForTax", 1890m, invoiceLine.DutyAndTaxManager.NormalValueForTax);
			AssertEquals("CustomsDuty Total Amount", 890m, invoiceLine.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.CustomsDuty));
			AssertEquals("CustomsDuty Rate", 0.7m, invoiceLine.DutyAndTaxManager.GetRate(DutyAndTaxTypes.Codes.CustomsDuty));

			//Regular between min/max
			invoiceHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.NewZealand;
			invoiceLine.DutiesAndTaxes[0].C1_ExemptCode = SIMACodes.Codes.C31;
			invoiceLine.DutiesAndTaxes[0].C1_Amount = 72;

			invoiceLine.JI_CustomsQuantity = 1000;
			invoiceLine.JI_LinePrice = 1145.4m;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals("Pre-Condition: Customs Value", 1145.4m, invoiceLine.CA_CustomsValue);
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("Customs Value", 1000m, invoiceLine.CA_CustomsValue);
			AssertEquals("DutiesAndTaxes count", 3, invoiceLine.DutiesAndTaxes.Count);
			AssertCount("SIMA duties count", DutyAndTaxTypes.Codes.SUR, 1);
			AssertCount("Duties count", DutyAndTaxTypes.Codes.CustomsDuty, 2);
			AssertTax(invoiceLine.DutiesAndTaxes[1], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.0234, ZString.Empty, 23.4);
			AssertTax(invoiceLine.DutiesAndTaxes[2], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 5, ZString.Empty, 50);

			const string expectedDescription = "Classification duty rate. The quantity of the first unit of measure (1000kgm) is multiplied by the Specific rate (2.34¢/Kilogram)."
											   + " The customs value ($1000) is multiplied by the Ad Valorem rate (5%). The results are added. But not less than 4.74¢/Kilogram or not more than 9.48¢/Kilogram.";
			AssertEquals("AmountDescription", expectedDescription, invoiceLine.DutiesAndTaxes[1].AmountDescription);
			AssertEquals("AmountDescription", expectedDescription, invoiceLine.DutiesAndTaxes[2].AmountDescription);

			invoiceLine.DutiesAndTaxes[1].AmountDescription = invoiceLine.DutiesAndTaxes[2].AmountDescription = ZString.Empty;
			invoiceLine.DutyAndTaxManager.UpdateDutiesAmountDescriptions();

			AssertEquals("AmountDescription", expectedDescription, invoiceLine.DutiesAndTaxes[1].AmountDescription);
			AssertEquals("AmountDescription", expectedDescription, invoiceLine.DutiesAndTaxes[2].AmountDescription);

			//Regular greater than max
			invoiceLine.JI_CustomsQuantity = 500;
			invoiceLine.JI_LinePrice = 1119.4;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals("Pre-Condition: Customs Value", 1119.4m, invoiceLine.CA_CustomsValue);
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("Customs Value", 1000m, invoiceLine.CA_CustomsValue);
			AssertEquals("DutiesAndTaxes count", 2, invoiceLine.DutiesAndTaxes.Count);
			AssertCount("SIMA duties count", DutyAndTaxTypes.Codes.SUR, 1);
			AssertCount("Duties count", DutyAndTaxTypes.Codes.CustomsDuty, 1);
			AssertTax(invoiceLine.DutiesAndTaxes[1], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.0948, ZString.Empty, 47.4);

			//Regular less than min
			invoiceLine.JI_CustomsQuantity = 10000;
			invoiceLine.JI_LinePrice = 1546;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals("Pre-Condition: Customs Value", 1546m, invoiceLine.CA_CustomsValue);
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("Customs Value", 1000m, invoiceLine.CA_CustomsValue);
			AssertEquals("DutiesAndTaxes count", 2, invoiceLine.DutiesAndTaxes.Count);
			AssertCount("SIMA duties count", DutyAndTaxTypes.Codes.SUR, 1);
			AssertCount("Duties count", DutyAndTaxTypes.Codes.CustomsDuty, 1);
			AssertTax(invoiceLine.DutiesAndTaxes[1], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.0474, ZString.Empty, 474);

			AssertEquals("NormalValueForTax", 1546m, invoiceLine.DutyAndTaxManager.NormalValueForTax);
			AssertEquals("CustomsDuty Total Amount", 474m, invoiceLine.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.CustomsDuty));
			AssertEquals("CustomsDuty Rate", 0.0474m, invoiceLine.DutyAndTaxManager.GetRate(DutyAndTaxTypes.Codes.CustomsDuty));

			//Classification regular less than min + excise regular greater than max
			var exciseRate = classHeader1.ExciseDutyRates.AddNew();
			exciseRate.ZB_EffectiveDate = effectiveDate;
			exciseRate.ZB_ExpiryDate = expiryDate;
			exciseRate.ZB_UnitOfMeasure = customsUnits3;

			var rate = exciseRate.Rates.AddNew();
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Specific, 0, 0, 6.03);
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AdValorem, 0, 165, 0);

			invoiceLine.JI_LinePrice = 1907.8;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals("Pre-Condition: Customs Value", 1907.8m, invoiceLine.CA_CustomsValue);
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("Customs Value", 1000m, invoiceLine.CA_CustomsValue);
			AssertEquals("DutiesAndTaxes count", 3, invoiceLine.DutiesAndTaxes.Count);
			AssertCount("SIMA duties count", DutyAndTaxTypes.Codes.SUR, 1);
			AssertCount("Duties count", DutyAndTaxTypes.Codes.CustomsDuty, 2);
			AssertTax(invoiceLine.DutiesAndTaxes[1], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.0474, ZString.Empty, 474);
			AssertTax(invoiceLine.DutiesAndTaxes[2], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 6.03, customsUnits3, 361.80);

			//TODO: Find out how to make calculated customs value invalid.
			//Unable to calculate
			//invoiceLine.JI_LinePrice = -100;
			//invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			//AssertEquals("Customs Value", ZDecimal.Zero, invoiceLine.CA_CustomsValue);
			//AssertHasMessageErrorContaining(invoiceLine.CA_CustomsValueInfo, "The system was unable to calculate Customs Value without duty, so you have to override it.");

			//General classification rate
			invoiceLine.CA_99TariffCode = ZString.Empty;
			invoiceLine.JI_Tariff = classificationNumber2;
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_CustomsUnitQty = customsUnits1;

			invoiceLine.JI_LinePrice = 1272;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals("Pre-Condition: Customs Value", 1272m, invoiceLine.CA_CustomsValue);
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertEquals("DutiesAndTaxes count", 5, invoiceLine.DutiesAndTaxes.Count);
			AssertCount("SIMA duties count", DutyAndTaxTypes.Codes.SUR, 1);
			AssertCount("Duties count", DutyAndTaxTypes.Codes.CustomsDuty, 4);
			AssertTax(invoiceLine.DutiesAndTaxes[1], false, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 20, ZString.Empty, 200); //General
			AssertTax(invoiceLine.DutiesAndTaxes[2], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Free, 0, customsUnits1, 0);
			AssertTax(invoiceLine.DutiesAndTaxes[3], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AcceptX, 0, customsUnits1, 0);
			AssertTax(invoiceLine.DutiesAndTaxes[4], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AcceptT, 0, customsUnits1, 0);
		}

		#endregion

		#region TestDutyPaidCalculationsForLVS

		void TestDutyPaidCalculationsForLVS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "LB001C";
			invoice1.JZ_InvoiceAmount = 100m;
			invoice1.JZ_RX_NKInvoice_Currency = Enterprise.Core.Constants.CurrencyCodes.Canada;
			invoice1.JZ_InvoiceCurrLandedCostExRate = 1m;
			invoice1.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			invoice1.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.DeliveredDutyPaid;

			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
			invoiceLine1.JI_Tariff = "7326909099";
			invoiceLine1.JI_LinePrice = 100m;

			var dutyAndTex = invoiceLine1.DutiesAndTaxes.AddNew();
			dutyAndTex.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			invoiceLine1.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals("Customs Duty Exists", true, invoiceLine1.DutiesAndTaxes.Contains(dutyAndTex));
		}

		#endregion

		#region PrepareRefFiles

		void PrepareGlobalTariffData(BusinessObjectFactory factory, string tariffNumber)
		{
			var universalHelper = new UniversalReferenceTestDataHelper(factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType1 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Excise);
			var rateCode1 = universalHelper.LoadOrCreateNewCusRateCode(factory, DutyAndTaxTypes.Codes.ExciseTax, rateType1.PK);
			var rateType2 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Duty);
			var rateCode2 = universalHelper.LoadOrCreateNewCusRateCode(factory, DutyAndTaxTypes.Codes.CustomsDuty, rateType2.PK);
			var rateType3 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.ExciseTax);
			var rateCode3 = universalHelper.LoadOrCreateNewCusRateCode(factory, "E90", rateType3.PK);
			var rateCode4 = universalHelper.LoadOrCreateNewCusRateCode(factory, "E91", rateType3.PK);
			var rateCode5 = universalHelper.LoadOrCreateNewCusRateCode(factory, "BB1", rateType3.PK);
			var rateCode6 = universalHelper.LoadOrCreateNewCusRateCode(factory, "BB2", rateType3.PK);
			var preference1 = universalHelper.CreatePreferenceForCountry("01", "Preference 01", Core.Constants.CountryCodes.Canada);
			var preference2 = universalHelper.CreatePreferenceForCountry("02", "Preference 02", Core.Constants.CountryCodes.Canada);
			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, tariffNumber, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var taxRate1 = universalHelper.CreateRate(tariff1, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.1*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate2 = universalHelper.CreateRate(tariff1, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.2*[MIL]", preference1.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate3 = universalHelper.CreateRate(tariff1, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.3*VFD", preference2.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate4 = universalHelper.CreateRate(tariff1, rateCode3.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.4*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate5 = universalHelper.CreateRate(tariff1, rateCode4.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.5*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate6 = universalHelper.CreateRate(tariff1, rateCode4.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-10), rateFormula: "0.6*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate7 = universalHelper.CreateRate(tariff1, rateCode5.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "22*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate8 = universalHelper.CreateRate(tariff1, rateCode6.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "22*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			factory.Save();
		}

		void PrepareRefFiles()
		{
			#region Populate Class Header

			#region Populate Class Rates

			#region Class Rate 1

			classHeader1 = GetClassHeader(classificationNumber1);

			var classRate = classHeader1.ClassRates.AddNew();
			classRate.ZB_EffectiveDate = effectiveDate;
			classRate.ZB_ExpiryDate = expiryDate;
			classRate.ZB_UnitOfMeasure = customsUnits1;

			var rate = classRate.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Specific, 0, 0.7, 0);
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AdValorem, 0, 19, 0);

			rate = classRate.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.NewZealand;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Specific, 0, 0.0234, 0);
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AdValorem, 0, 5, 0);
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Specific, 0, 0, 0.0948);
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Specific, 0.04740, 0, 0);

			rate = classRate.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.Chile;

			#endregion

			#region Class Rate 2

			var classHeader = GetClassHeader(classificationNumber4);

			classRate = classHeader.ClassRates.AddNew();
			classRate.ZB_EffectiveDate = effectiveDate;
			classRate.ZB_ExpiryDate = expiryDate;
			classRate.ZB_UnitOfMeasure = customsUnits2;
			classRate.ZB_FreeInd = true;

			#endregion

			#region Class Rate 3

			classHeader = GetClassHeader(classificationNumber5);

			classRate = classHeader.ClassRates.AddNew();
			classRate.ZB_EffectiveDate = effectiveDate;
			classRate.ZB_ExpiryDate = expiryDate;
			classRate.ZB_UnitOfMeasure = customsUnits1;

			rate = classRate.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.General;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AdValorem, 0, 123, 0);

			rate = classRate.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AdValorem, 0, 8, 0);

			#endregion

			#endregion

			#region Populate Excise Duty Rates

			#region Excise Duty Rate 1

			classHeader = GetClassHeader(classificationNumber2);

			var exciseRate = classHeader.ExciseDutyRates.AddNew();
			exciseRate.ZB_EffectiveDate = effectiveDate;
			exciseRate.ZB_ExpiryDate = expiryDate;
			exciseRate.ZB_UnitOfMeasure = customsUnits1;

			rate = exciseRate.Rates.AddNew();
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Free, 0, 0, 0);
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AcceptX, 0, 0, 0);
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AcceptT, 0, 0, 0);

			#endregion

			#region Excise Duty Rate 3

			exciseRate = classHeader.ExciseDutyRates.AddNew();
			exciseRate.ZB_EffectiveDate = effectiveDate;
			exciseRate.ZB_ExpiryDate = effectiveDate;
			exciseRate.ZB_UnitOfMeasure = customsUnits3;

			rate = exciseRate.Rates.AddNew();
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Specific, 6.03, 0, 0);
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AdValorem, 0, 165, 0);

			#endregion

			#endregion

			#region Populate Ref Numbers

			classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = classificationNumber3;
			classHeader.ZA_EffectiveDate = effectiveDate;
			classHeader.ZA_ExpiryDate = expiryDate;
			classHeader.ZA_AreaCode = "900";

			var refNumHeader = classHeader.RefNumbers.AddNew();
			refNumHeader.ZD_EffectiveDate = effectiveDate;
			refNumHeader.ZD_ExpiryDate = expiryDate;
			FillRefNumber(refNumHeader.RefNumbers.AddNew(), "001", "BB2");
			FillRefNumber(refNumHeader.RefNumbers.AddNew(), "AA1", "BB1");
			FillRefNumber(refNumHeader.RefNumbers.AddNew(), "AA3", "BB3");

			var exciseRates = new CACTaxRateCollection(Factory, CACTaxRate.TaxType.Excise);
			exciseRates.Load();
			FillTaxRate(exciseRates.AddNew(), refNumHeader.RefNumbers[1].ZE_ExciseTaxRefNumber, customsUnits1, "Excise TAX 1", RateTypes.Codes.Exempt, 0);
			FillTaxRate(exciseRates.AddNew(), refNumHeader.RefNumbers[0].ZE_ExciseTaxRefNumber, customsUnits1, "Excise TAX 2", RateTypes.Codes.AdValorem, 22);
			FillTaxRate(exciseRates.AddNew(), refNumHeader.RefNumbers[2].ZE_ExciseTaxRefNumber, customsUnits3, "Excise TAX 3", RateTypes.Codes.AcceptT, 12, true);

			#endregion

			#endregion

			#region  Populate Tariff Header

			#region Tariff 1

			var tariffHeader = Factory.New<CACTariffHeader>();
			tariffHeader.ZF_TariffCode = tariffCode1;
			tariffHeader.ZF_AuthEffectiveDate = effectiveDate;
			tariffHeader.ZF_AuthExpiryDate = currentDate;
			tariffHeader.ZF_RateEffectiveDate = effectiveDate;
			tariffHeader.ZF_RateExpiryDate = currentDate;

			rate = tariffHeader.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Free, 0, 0, 0);

			rate = tariffHeader.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.NewZealand;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Specific, 0, 0.352, 0);

			#endregion

			#region Tariff 2

			tariffHeader = Factory.New<CACTariffHeader>();
			tariffHeader.ZF_TariffCode = tariffCode2;
			tariffHeader.ZF_AuthEffectiveDate = effectiveDate;
			tariffHeader.ZF_AuthExpiryDate = expiryDate;
			tariffHeader.ZF_RateEffectiveDate = effectiveDate;
			tariffHeader.ZF_RateExpiryDate = effectiveDate;

			rate = tariffHeader.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.NewZealand;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Specific, 0, 0.66, 0);
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AdValorem, 0, 20, 0);

			#endregion

			#region Tariff 3

			tariffHeader = Factory.New<CACTariffHeader>();
			tariffHeader.ZF_TariffCode = tariffCode3;
			tariffHeader.ZF_AuthEffectiveDate = effectiveDate;
			tariffHeader.ZF_AuthExpiryDate = expiryDate;
			tariffHeader.ZF_FreeInd = true;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Free, 0, 0, 0);

			#endregion

			#region Tariff 4

			tariffHeader = Factory.New<CACTariffHeader>();
			tariffHeader.ZF_TariffCode = tariffCode4;
			tariffHeader.ZF_AuthEffectiveDate = effectiveDate;
			tariffHeader.ZF_AuthExpiryDate = expiryDate;
			tariffHeader.ZF_RateEffectiveDate = effectiveDate;
			tariffHeader.ZF_RateExpiryDate = expiryDate;

			rate = tariffHeader.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.NewZealand;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AdValorem, 0, 22, 0);

			#endregion

			#endregion
		}

		#endregion

		#endregion

		#region TestResetSIMADuty

		public void TestDefaultSIMADutyForAllTradeGroupOnInvoiceLine()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);

			var allTradeGroup = universalHelper.CreateTradeGroup(Core.Constants.CountryCodes.Canada, "All Countries", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddCountry(allTradeGroup, Core.Constants.CountryCodes.China, ZDate.Today.AddYears(-1), ZDateTime.MaxSmallDateTime.Date);
			universalHelper.AddCountry(allTradeGroup, Core.Constants.CountryCodes.HongKong, ZDate.Today.AddYears(-1), ZDateTime.MaxSmallDateTime.Date);
			universalHelper.AddCountry(allTradeGroup, Core.Constants.CountryCodes.UnitedStates, ZDate.Today.AddYears(-1), ZDateTime.MaxSmallDateTime.Date);

			var usTradeGroup = universalHelper.CreateTradeGroup(Core.Constants.CountryCodes.Canada, "US", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddCountry(usTradeGroup, Core.Constants.CountryCodes.UnitedStates, ZDate.Today.AddYears(-1), ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var simaTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var antiDumpingRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.AntiDumping);
			var antiDumpingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ADD, antiDumpingRateType.PK);
			var surTaxRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.SUR, antiDumpingRateType.PK);
			var countervailingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CVD, antiDumpingRateType.PK);
			Factory.Save();

			var surtaxTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "2345678901", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(surtaxTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM");
			var surTaxRate = universalHelper.CreateRate(surtaxTariff, surTaxRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "0.1 * VFD");
			var surTaxApplicability = universalHelper.CreateCusApplicability(surTaxRate, allTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateExcludedTradeGroup(usTradeGroup, surTaxApplicability);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2345678901";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.CA_CustomsValueOvr = true;
			invoiceLine.CA_CustomsValue = 2500m;
			AssertEquals(1, invoiceLine.DutiesAndTaxes.Count);
			var surTax = invoiceLine.DutiesAndTaxes[0];
			surTax.C1_ExemptCode = SIMACodes.Codes.C51;
			AssertTax(surTax, false, DutyAndTaxTypes.Codes.SUR, "", false, RateTypes.Codes.AdValorem, 10m, "", 250m);
			surTax.C1_ExemptCode = SIMACodes.Codes.C50;
			AssertTax(surTax, false, DutyAndTaxTypes.Codes.SUR, "", false, RateTypes.Codes.AdValorem, 10m, "", 0m);
			Factory.Save();
			AssertTax(surTax, false, DutyAndTaxTypes.Codes.SUR, "", false, RateTypes.Codes.Exempt, 10m, "", 0m);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			AssertEquals(1, invoiceLine.DutiesAndTaxes.Count);
			surTax = invoiceLine.DutiesAndTaxes[0];
			surTax.C1_ExemptCode = SIMACodes.Codes.C51;
			AssertTax(surTax, false, DutyAndTaxTypes.Codes.SUR, "", false, RateTypes.Codes.AdValorem, 10m, "", 250m);
			surTax.C1_ExemptCode = SIMACodes.Codes.C50;
			AssertTax(surTax, false, DutyAndTaxTypes.Codes.SUR, "", false, RateTypes.Codes.AdValorem, 10m, "", 0m);
			Factory.Save();
			AssertTax(surTax, false, DutyAndTaxTypes.Codes.SUR, "", false, RateTypes.Codes.Exempt, 10m, "", 0m);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(0, invoiceLine.DutiesAndTaxes.Count);
		}

		[TestDate(2021, 11, 20)]
		public void TestResetSIMADutyOnInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "CNY";
			invoice.JZ_ValuationDateOverride = new ZDate(2020, 11, 21);
			invoice.JZ_InvoiceDate = new ZDate(2020, 11, 7);
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CVforCurrConvOvr = true;
			invoiceLine.CA_CVforCurrConv = 2500m;
			invoiceLine.JI_Tariff = classificationNumber1;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.CA_CustomsValueOvr = true;
			invoiceLine.CA_CustomsValue = 2500m;
			AssertEquals(1, invoiceLine.DutiesAndTaxes.Count);
			var surTax = invoiceLine.DutiesAndTaxes[0];
			surTax.C1_ExemptCode = SIMACodes.Codes.C51;
			AssertTax(surTax, false, DutyAndTaxTypes.Codes.SUR, "", false, RateTypes.Codes.AdValorem, 10m, "", 250m);
			surTax.C1_Override = true;
			surTax.C1_RateType = RateTypes.Codes.Specific;
			surTax.C1_Rate = 15m;
			surTax.C1_UnitOfMeasure = "KGM";
			surTax.C1_Amount = 100m;
			AssertTax(surTax, false, DutyAndTaxTypes.Codes.SUR, "", true, RateTypes.Codes.Specific, 15m, "KGM", 100m);
			AssertEquals(SIMACodes.Codes.C51, surTax.C1_ExemptCode);
			surTax.C1_Code = "ABCD";
			surTax.C1_Override = false;
			AssertTax(surTax, false, DutyAndTaxTypes.Codes.SUR, "ABCD", false, RateTypes.Codes.AdValorem, 10m, "", 417.5m);
			AssertEquals(SIMACodes.Codes.C51, surTax.C1_ExemptCode);
		}

		public void TestExemptSIMADutyOnInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = classificationNumber1;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.CA_CustomsValueOvr = true;
			invoiceLine.CA_CustomsValue = 2500m;
			AssertEquals(1, invoiceLine.DutiesAndTaxes.Count);
			var surTax = invoiceLine.DutiesAndTaxes[0];
			surTax.C1_ExemptCode = SIMACodes.Codes.C51;
			AssertTax(surTax, false, DutyAndTaxTypes.Codes.SUR, "", false, RateTypes.Codes.AdValorem, 10m, "", 250m);
			surTax.C1_ExemptCode = SIMACodes.Codes.C50;
			AssertTax(surTax, false, DutyAndTaxTypes.Codes.SUR, "", false, RateTypes.Codes.AdValorem, 10m, "", 0m);
			Factory.Save();
			AssertTax(surTax, false, DutyAndTaxTypes.Codes.SUR, "", false, RateTypes.Codes.Exempt, 10m, "", 0m);
		}

		public void TestAmountDescriptionSIMADutyOnInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = classificationNumber1;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.CA_CustomsValueOvr = true;
			invoiceLine.CA_CustomsValue = 2500m;
			AssertEquals(1, invoiceLine.DutiesAndTaxes.Count);
			var surTax = invoiceLine.DutiesAndTaxes[0];
			surTax.C1_ExemptCode = SIMACodes.Codes.C51;
			AssertTax(surTax, false, DutyAndTaxTypes.Codes.SUR, "", false, RateTypes.Codes.AdValorem, 10m, "", 250m);
			AssertEquals("The amount description should display 2500", "The customs value ($2500) is multiplied by the Ad Valorem rate (10%).", surTax.AmountDescription);
			invoiceLine.CA_CustomsValue = 5000m;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals("The amount description should be updated", "The customs value ($5000) is multiplied by the Ad Valorem rate (10%).", surTax.AmountDescription);
		}

		public void TestResetSIMADutyOnPivot()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "0123456789";
			pivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.China;
			AssertEquals(1, pivot.DutiesAndTaxes.Count);
			var addTax = pivot.DutiesAndTaxes[0];
			addTax.C1_ExemptCode = SIMACodes.Codes.C51;
			AssertTax(addTax, false, DutyAndTaxTypes.Codes.ADD, "", false, RateTypes.Codes.Specific, 100.5m, "NMB", 0m);
			addTax.C1_Override = true;
			addTax.C1_RateType = RateTypes.Codes.AdValorem;
			addTax.C1_Rate = 10m;
			addTax.C1_UnitOfMeasure = "KGM";
			addTax.C1_NormalValuePerUnit = 100m;
			addTax.C1_NormalValueCurrency = "CNY";
			addTax.C1_ForeignRate = 11m;
			addTax.C1_ForeignCurrency = "CNY";
			AssertTax(addTax, false, DutyAndTaxTypes.Codes.ADD, "", true, RateTypes.Codes.AdValorem, 10m, "KGM", 0m);
			AssertEquals(100m, addTax.C1_NormalValuePerUnit);
			AssertEquals("CNY", addTax.C1_NormalValueCurrency);
			AssertEquals(11m, addTax.C1_ForeignRate);
			AssertEquals("CNY", addTax.C1_ForeignCurrency);
			addTax.C1_Override = false;
			AssertTax(addTax, false, DutyAndTaxTypes.Codes.ADD, "", false, RateTypes.Codes.Specific, 100.5m, "NMB", 0m);
			AssertEquals(0m, addTax.C1_NormalValuePerUnit);
			AssertEquals(ZString.Empty, addTax.C1_NormalValueCurrency);
			AssertEquals(0m, addTax.C1_ForeignRate);
			AssertEquals(ZString.Empty, addTax.C1_ForeignCurrency);
		}

		[TestDate(2021, 09, 16)]
		public void TestSIMADutiesBasedOnInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "CNY";
			invoice.JZ_ValuationDateOverride = new ZDate(2020, 11, 21);
			invoice.JZ_InvoiceDate = new ZDate(2020, 11, 7);
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = classificationNumber1;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.CA_CustomsValueOvr = true;
			invoiceLine.CA_CustomsValue = 2500m;
			invoiceLine.CA_CVforCurrConvOvr = true;
			invoiceLine.CA_CVforCurrConv = 2500m;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			AssertEquals(1, invoiceLine.DutiesAndTaxes.Count);
			var addTax = invoiceLine.DutiesAndTaxes[0];
			AssertTax(addTax, false, DutyAndTaxTypes.Codes.SUR, "", false, RateTypes.Codes.AdValorem, 10m, "", 417.5m);

			invoiceLine.JI_Tariff = classificationNumber6;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals(1, invoiceLine.DutiesAndTaxes.Count);
			addTax = invoiceLine.DutiesAndTaxes[0];
			AssertTax(addTax, false, DutyAndTaxTypes.Codes.SUR, "ADCode", false, RateTypes.Codes.AdValorem, 10m, "", 417.5m);
		}

		#endregion

		#region TestRemissionConfigs

		public void TestRemissionConfigs_DTY()
		{
			Factory.ClearCachedValue<CACClassHeader>(string.Format("CACClassHeader_{0}_{1}_{2}", currentDate.ToShortDateString(), classificationNumber1, false));
			PrepareRefFiles();
			invoiceLine.CA_99TariffCode = tariffCode1;
			invoiceHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.NewZealand;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertCount("CustomsDuty count", DutyAndTaxTypes.Codes.CustomsDuty, 1);
			AssertTax(invoiceLine.DutiesAndTaxes[0], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.352, ZString.Empty, 3.52);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
			var rulingconfig1 = invoiceLine.RulingConfigurations.AddNew();
			rulingconfig1.ZZY_Category = RefCusRulingConfigCategories.Codes.DTY;
			rulingconfig1.ZZY_Type = RefCusRulingConfigTypes.Codes.AcceptAmount;
			rulingconfig1.ZZY_Value = "5.1";

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertCount("CustomsDuty count", DutyAndTaxTypes.Codes.CustomsDuty, 1);
			AssertTax(invoiceLine.DutiesAndTaxes[0], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.352, ZString.Empty, 5.1);
			AssertEquals("AmountDescription", "Remission configuration duty amount. 5.1 for Accept Amount.", invoiceLine.DutiesAndTaxes[0].AmountDescription);
			AssertEquals("C1_OriginalAmount", 5.1m, invoiceLine.DutiesAndTaxes[0].C1_OriginalAmount);

			rulingconfig1.ZZY_Type = RefCusRulingConfigTypes.Codes.Maximum;
			rulingconfig1.ZZY_Value = "3.2";

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertCount("CustomsDuty count", DutyAndTaxTypes.Codes.CustomsDuty, 1);
			AssertTax(invoiceLine.DutiesAndTaxes[0], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.352, ZString.Empty, 3.2);
			AssertEquals("AmountDescription", "Remission configuration duty amount. Not more than 3.2.", invoiceLine.DutiesAndTaxes[0].AmountDescription);
			AssertEquals("C1_OriginalAmount", 3.52m, invoiceLine.DutiesAndTaxes[0].C1_OriginalAmount);

			rulingconfig1.ZZY_Value = string.Empty;
			rulingconfig1.ZZY_Rate = 0.2m;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertCount("CustomsDuty count", DutyAndTaxTypes.Codes.CustomsDuty, 1);
			AssertTax(invoiceLine.DutiesAndTaxes[0], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.352, ZString.Empty, 2);
			AssertEquals("AmountDescription", "Remission configuration duty amount. Not more than 2.0.", invoiceLine.DutiesAndTaxes[0].AmountDescription);
			AssertEquals("C1_OriginalAmount", 3.52m, invoiceLine.DutiesAndTaxes[0].C1_OriginalAmount);

			rulingconfig1.ZZY_Type = RefCusRulingConfigTypes.Codes.Minimum;
			rulingconfig1.ZZY_Rate = 0m;
			rulingconfig1.ZZY_Value = "4.3";

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertCount("CustomsDuty count", DutyAndTaxTypes.Codes.CustomsDuty, 1);
			AssertTax(invoiceLine.DutiesAndTaxes[0], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.352, ZString.Empty, 4.3);
			AssertEquals("AmountDescription", "Remission configuration duty amount. Not less than 4.3.", invoiceLine.DutiesAndTaxes[0].AmountDescription);
			AssertEquals("C1_OriginalAmount", 3.52m, invoiceLine.DutiesAndTaxes[0].C1_OriginalAmount);

			rulingconfig1.ZZY_Value = string.Empty;
			rulingconfig1.ZZY_Rate = 0.56m;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertCount("CustomsDuty count", DutyAndTaxTypes.Codes.CustomsDuty, 1);
			AssertTax(invoiceLine.DutiesAndTaxes[0], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.352, ZString.Empty, 5.6);
			AssertEquals("AmountDescription", "Remission configuration duty amount. Not less than 5.60.", invoiceLine.DutiesAndTaxes[0].AmountDescription);

			rulingconfig1.ZZY_Type = RefCusRulingConfigTypes.Codes.Specific;
			rulingconfig1.ZZY_Rate = 0.2m;
			rulingconfig1.ZZY_Value = string.Empty;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertCount("CustomsDuty count", DutyAndTaxTypes.Codes.CustomsDuty, 1);
			AssertTax(invoiceLine.DutiesAndTaxes[0], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.2, ZString.Empty, 2);
			AssertEquals("AmountDescription", "Remission configuration duty rate. 0.2% for Specific Rate.", invoiceLine.DutiesAndTaxes[0].AmountDescription);
			AssertEquals("C1_OriginalAmount", 2.0m, invoiceLine.DutiesAndTaxes[0].C1_OriginalAmount);

			rulingconfig1.ZZY_Type = RefCusRulingConfigTypes.Codes.ExemptCode;
			rulingconfig1.ZZY_Rate = 0;
			rulingconfig1.ZZY_Value = "001";

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertCount("CustomsDuty count", DutyAndTaxTypes.Codes.CustomsDuty, 1);
			AssertTax(invoiceLine.DutiesAndTaxes[0], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.352, ZString.Empty, 0);
			AssertEquals("AmountDescription", "Remission configuration duty amount. 0 for Exempt Code: 001.", invoiceLine.DutiesAndTaxes[0].AmountDescription);
			AssertEquals("C1_ExemptCode", "001", invoiceLine.DutiesAndTaxes[0].C1_ExemptCode);
			AssertEquals("C1_OriginalAmount", 0m, invoiceLine.DutiesAndTaxes[0].C1_OriginalAmount);

			rulingconfig1.ZZY_Type = RefCusRulingConfigTypes.Codes.AcceptRate;
			rulingconfig1.ZZY_Rate = 0m;

			var rulingconfig2 = invoiceLine.RulingConfigurations.AddNew();
			rulingconfig2.ZZY_Category = RefCusRulingConfigCategories.Codes.DTY;
			rulingconfig2.ZZY_Type = RefCusRulingConfigTypes.Codes.Specific;
			rulingconfig2.ZZY_Rate = 0.45m;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertCount("CustomsDuty count", DutyAndTaxTypes.Codes.CustomsDuty, 1);
			var duty = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty);
			AssertTax(duty, true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.Specific, 0.45, ZString.Empty, 4.5);
			AssertEquals("AmountDescription", "Remission configuration duty rate. 0.45% for Specific Rate.", duty.AmountDescription);
			AssertEquals("C1_OriginalAmount", 4.5m, duty.C1_OriginalAmount);

			duty = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty.C1_Rate = 10m;
			duty.C1_Amount = 100.11m;
			AssertTax(duty, true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, true, RateTypes.Codes.Specific, 10m, ZString.Empty, 100.11m);
			AssertEquals("C1_OriginalAmount", 100.11m, duty.C1_OriginalAmount);
		}

		public void TestRemissionConfigs_EXD()
		{
			Factory.ClearCachedValue<CACClassHeader>(string.Format("CACClassHeader_{0}_{1}_{2}", currentDate.ToShortDateString(), classificationNumber1, false));
			var classHeader = GetClassHeader(classificationNumber1);
			var classRate = classHeader.ClassRates.AddNew();
			classRate.ZB_EffectiveDate = effectiveDate;
			classRate.ZB_ExpiryDate = expiryDate;
			classRate.ZB_UnitOfMeasure = CustomsUnitOfMeasureList.Codes.Kilogram;
			var rate = classRate.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.General;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AdValorem, 0, 1.8, 0);

			var exciseRate = classHeader.ExciseDutyRates.AddNew();
			exciseRate.ZB_EffectiveDate = effectiveDate;
			exciseRate.ZB_ExpiryDate = expiryDate;
			exciseRate.ZB_UnitOfMeasure = CustomsUnitOfMeasureList.Codes.Kilogram;
			rate = exciseRate.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.General;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AdValorem, 0, 1.9, 0);

			Factory.Save();

			invoiceLine.JI_Tariff = classificationNumber1;
			invoiceHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.NewZealand;
			Factory.ClearCachedValue<CACRate>(string.Format("CACRate|{0}|{1}|{2}|{3}", classificationNumber1, TariffTreatmentCodes.Codes.General, currentDate.ToShortDateString(), "CLS"));
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			var taxes = new List<DutyAndTax>(invoiceLine.DutiesAndTaxes.Find(tax => tax.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty));
			var eXDTaxes = taxes.FindAll(tax => tax.IsEXDDuty);
			var dTYTaxes = taxes.FindAll(tax => !tax.IsEXDDuty);

			AssertEquals("Expect count of taxes is 2", 2, taxes.Count);
			AssertEquals("Expect count of eXDTaxes is 1", 1, eXDTaxes.Count);
			AssertEquals("Expect count of dTYTaxes is 1", 1, dTYTaxes.Count);
			AssertTax(eXDTaxes[0], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 1.9, CustomsUnitOfMeasureList.Codes.Kilogram, 19);
			AssertTax(dTYTaxes[0], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 1.8, "", 18);

			var rulingconfig1 = invoiceLine.RulingConfigurations.AddNew();
			rulingconfig1.ZZY_Category = RefCusRulingConfigCategories.Codes.EXD;
			rulingconfig1.ZZY_Type = RefCusRulingConfigTypes.Codes.Maximum;
			rulingconfig1.ZZY_Value = "3.2";
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals("Expect count of taxes is 2", 2, taxes.Count);
			AssertEquals("Expect count of eXDTaxes is 1", 1, eXDTaxes.Count);
			AssertEquals("Expect count of dTYTaxes is 1", 1, dTYTaxes.Count);
			AssertTax(eXDTaxes[0], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 1.9, CustomsUnitOfMeasureList.Codes.Kilogram, 3.2);
			AssertTax(dTYTaxes[0], true, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 1.8, "", 18);
			AssertEquals("AmountDescription", "Remission configuration duty amount. Not more than 3.2.", eXDTaxes[0].AmountDescription);
			AssertEquals("C1_OriginalAmount", 19m, eXDTaxes[0].C1_OriginalAmount);
		}

		public void TestRemissionConfigs_SIMDuty()
		{
			PrepareRefFiles();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "CNY";
			invoiceHeader.JZ_ValuationDateOverride = new ZDate(2020, 11, 21);
			invoiceHeader.JZ_InvoiceDate = new ZDate(2020, 11, 7);
			invoiceLine.CA_CVforCurrConvOvr = true;
			invoiceLine.CA_CVforCurrConv = 2500m;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.JI_Tariff = classificationNumber1;
			invoiceLine.JI_CustomsUnitQty = customsUnits1;
			invoiceHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.General;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertTax(invoiceLine.DutiesAndTaxes[0], false, DutyAndTaxTypes.Codes.SUR, ZString.Empty, false, RateTypes.Codes.AdValorem, 10.0, ZString.Empty, 417.5m);
			AssertEquals("AmountDescription", "The customs value ($4175) is multiplied by the Ad Valorem rate (10%).", invoiceLine.DutiesAndTaxes[0].AmountDescription);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
			var rulingconfig = invoiceLine.RulingConfigurations.AddNew();
			rulingconfig.ZZY_Category = RefCusRulingConfigCategories.Codes.SIM;
			rulingconfig.ZZY_Type = RefCusRulingConfigTypes.Codes.AcceptAmount;
			rulingconfig.ZZY_Value = "150";

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertTax(invoiceLine.DutiesAndTaxes[0], false, DutyAndTaxTypes.Codes.SUR, ZString.Empty, false, RateTypes.Codes.AdValorem, 10.0, ZString.Empty, 150m);
			AssertEquals("AmountDescription", "Remission configuration duty amount. 150 for Accept Amount.", invoiceLine.DutiesAndTaxes[0].AmountDescription);
			AssertEquals("C1_OriginalAmount", 150m, invoiceLine.DutiesAndTaxes[0].C1_OriginalAmount);

			rulingconfig.ZZY_Type = RefCusRulingConfigTypes.Codes.Maximum;
			rulingconfig.ZZY_Value = "80";

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertTax(invoiceLine.DutiesAndTaxes[0], false, DutyAndTaxTypes.Codes.SUR, ZString.Empty, false, RateTypes.Codes.AdValorem, 10m, ZString.Empty, 80m);
			AssertEquals("AmountDescription", "Remission configuration duty amount. Not more than 80.", invoiceLine.DutiesAndTaxes[0].AmountDescription);
			AssertEquals("C1_OriginalAmount", 417.5m, invoiceLine.DutiesAndTaxes[0].C1_OriginalAmount);

			rulingconfig.ZZY_Type = RefCusRulingConfigTypes.Codes.Minimum;
			rulingconfig.ZZY_Value = "300";
			rulingconfig.ZZY_Rate = 0;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertTax(invoiceLine.DutiesAndTaxes[0], false, DutyAndTaxTypes.Codes.SUR, ZString.Empty, false, RateTypes.Codes.AdValorem, 10m, ZString.Empty, 417.5m);
			AssertEquals("AmountDescription", "The customs value ($4175) is multiplied by the Ad Valorem rate (10%).", invoiceLine.DutiesAndTaxes[0].AmountDescription);
			AssertEquals("C1_OriginalAmount", 417.5m, invoiceLine.DutiesAndTaxes[0].C1_OriginalAmount);

			rulingconfig.ZZY_Type = RefCusRulingConfigTypes.Codes.AdValorem;
			rulingconfig.ZZY_Rate = 20m;
			rulingconfig.ZZY_Value = string.Empty;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertTax(invoiceLine.DutiesAndTaxes[0], false, DutyAndTaxTypes.Codes.SUR, ZString.Empty, false, RateTypes.Codes.AdValorem, 20m, ZString.Empty, 200);
			AssertEquals("AmountDescription", "Remission configuration duty rate. 20% for Ad-Valorem rate for Remission.", invoiceLine.DutiesAndTaxes[0].AmountDescription);
			AssertEquals("C1_OriginalAmount", 200m, invoiceLine.DutiesAndTaxes[0].C1_OriginalAmount);

			rulingconfig.ZZY_Type = RefCusRulingConfigTypes.Codes.ExemptCode;
			rulingconfig.ZZY_Value = "002";
			rulingconfig.ZZY_Rate = 0;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertTax(invoiceLine.DutiesAndTaxes[0], false, DutyAndTaxTypes.Codes.SUR, ZString.Empty, false, RateTypes.Codes.AdValorem, 10m, ZString.Empty, 0m);
			AssertEquals("AmountDescription", "Remission configuration duty amount. 0 for Exempt Code: 002.", invoiceLine.DutiesAndTaxes[0].AmountDescription);
			AssertEquals("C1_ExemptCode", "002", invoiceLine.DutiesAndTaxes[0].C1_ExemptCode);
			AssertEquals("C1_OriginalAmount", 0m, invoiceLine.DutiesAndTaxes[0].C1_OriginalAmount);
		}

		public void TestRemissionConfigs_EXC()
		{
			PrepareGlobalTariffData(Factory, "423456789");
			var classHeader = GetClassHeader("423456789");
			PrepareRefFiles();
			invoiceLine.JI_Tariff = "423456789";
			invoiceLine.CA_99TariffCode = string.Empty;
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_CustomsUnitQty = customsUnits1;
			CACustomsDataRegistry.Instance.DefaultToThisExciseTaxRateCodeWhenApplicable.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "BB2");

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			var exsTax = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax);
			AssertTax(exsTax, false, DutyAndTaxTypes.Codes.ExciseTax, "BB2", false, RateTypes.Codes.AdValorem, 22m, "KGM", 264m);
			AssertEquals("AmountDescription", "The normal duty paid value ($1200) is multiplied by the Ad Valorem rate (22%).", exsTax.AmountDescription);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
			var rulingconfig = invoiceLine.RulingConfigurations.AddNew();
			rulingconfig.ZZY_Category = RefCusRulingConfigCategories.Codes.EXC;
			rulingconfig.ZZY_Type = RefCusRulingConfigTypes.Codes.AcceptAmount;
			rulingconfig.ZZY_Value = "150";

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			exsTax = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax);
			AssertTax(exsTax, false, DutyAndTaxTypes.Codes.ExciseTax, "BB2", false, RateTypes.Codes.AdValorem, 22m, customsUnits1, 150m);
			AssertEquals("AmountDescription", "Remission configuration duty amount. 150 for Accept Amount.", exsTax.AmountDescription);
			AssertEquals("C1_OriginalAmount", 264m, exsTax.C1_OriginalAmount);

			rulingconfig.ZZY_Type = RefCusRulingConfigTypes.Codes.Maximum;
			rulingconfig.ZZY_Value = "120";

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			exsTax = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax);
			AssertTax(exsTax, false, DutyAndTaxTypes.Codes.ExciseTax, "BB2", false, RateTypes.Codes.AdValorem, 22m, customsUnits1, 120m);
			AssertEquals("AmountDescription", "Remission configuration duty amount. Not more than 120.", exsTax.AmountDescription);
			AssertEquals("C1_OriginalAmount", 264m, exsTax.C1_OriginalAmount);

			rulingconfig.ZZY_Type = RefCusRulingConfigTypes.Codes.Minimum;
			rulingconfig.ZZY_Value = "300";
			rulingconfig.ZZY_Rate = 0;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			exsTax = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax);
			AssertTax(exsTax, false, DutyAndTaxTypes.Codes.ExciseTax, "BB2", false, RateTypes.Codes.AdValorem, 22m, customsUnits1, 300m);
			AssertEquals("AmountDescription", "Remission configuration duty amount. Not less than 300.", exsTax.AmountDescription);
			AssertEquals("C1_OriginalAmount", 264m, exsTax.C1_OriginalAmount);

			rulingconfig.ZZY_Type = RefCusRulingConfigTypes.Codes.AdValorem;
			rulingconfig.ZZY_Rate = 20m;
			rulingconfig.ZZY_Value = string.Empty;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			exsTax = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax);
			AssertTax(exsTax, false, DutyAndTaxTypes.Codes.ExciseTax, "BB2", false, RateTypes.Codes.AdValorem, 20m, customsUnits1, 240m);
			AssertEquals("AmountDescription", "Remission configuration duty rate. 20% for Ad-Valorem rate for Remission.", exsTax.AmountDescription);
			AssertEquals("C1_OriginalAmount", 264m, exsTax.C1_OriginalAmount);

			rulingconfig.ZZY_Type = RefCusRulingConfigTypes.Codes.ExemptCode;
			rulingconfig.ZZY_Value = "002";
			rulingconfig.ZZY_Rate = 0;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			exsTax = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax);
			AssertTax(exsTax, false, DutyAndTaxTypes.Codes.ExciseTax, "BB2", false, RateTypes.Codes.AdValorem, 22m, customsUnits1, 0m);
			AssertEquals("AmountDescription", "Remission configuration duty amount. 0 for Exempt Code: 002.", exsTax.AmountDescription);
			AssertEquals("C1_ExemptCode", "002", exsTax.C1_ExemptCode);
			AssertEquals("C1_OriginalAmount", 0m, exsTax.C1_OriginalAmount);
		}

		public void TestRemissionConfigs_GST()
		{
			var classHeader = GetClassHeader("3234567890");
			PrepareGlobalTariffData(Factory, "3234567890");
			PrepareRefFiles();
			invoiceLine.JI_Tariff = "3234567890";
			invoiceLine.CA_99TariffCode = string.Empty;
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_CustomsUnitQty = customsUnits1;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST), false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5m, ZString.Empty, 60m);
			AssertEquals("AmountDescription", "The normal value for tax ($1200) is multiplied by the Ad Valorem rate (5%).", invoiceLine.DutiesAndTaxes[2].AmountDescription);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
			var rulingconfig = invoiceLine.RulingConfigurations.AddNew();
			rulingconfig.ZZY_Category = RefCusRulingConfigCategories.Codes.GST;
			rulingconfig.ZZY_Type = RefCusRulingConfigTypes.Codes.AcceptAmount;
			rulingconfig.ZZY_Value = "150";

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST), false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5m, ZString.Empty, 150m);
			AssertEquals("AmountDescription", "Remission configuration duty amount. 150 for Accept Amount.", invoiceLine.DutiesAndTaxes[2].AmountDescription);
			AssertEquals("C1_OriginalAmount", 60m, invoiceLine.DutiesAndTaxes[2].C1_OriginalAmount);

			rulingconfig.ZZY_Type = RefCusRulingConfigTypes.Codes.Maximum;
			rulingconfig.ZZY_Value = "50";

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST), false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5m, ZString.Empty, 50m);
			AssertEquals("AmountDescription", "Remission configuration duty amount. Not more than 50.", invoiceLine.DutiesAndTaxes[2].AmountDescription);
			AssertEquals("C1_OriginalAmount", 60m, invoiceLine.DutiesAndTaxes[2].C1_OriginalAmount);

			rulingconfig.ZZY_Type = RefCusRulingConfigTypes.Codes.Minimum;
			rulingconfig.ZZY_Value = "300";
			rulingconfig.ZZY_Rate = 0;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST), false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5m, ZString.Empty, 300m);
			AssertEquals("AmountDescription", "Remission configuration duty amount. Not less than 300.", invoiceLine.DutiesAndTaxes[2].AmountDescription);
			AssertEquals("C1_OriginalAmount", 60m, invoiceLine.DutiesAndTaxes[2].C1_OriginalAmount);

			rulingconfig.ZZY_Type = RefCusRulingConfigTypes.Codes.AdValorem;
			rulingconfig.ZZY_Rate = 20m;
			rulingconfig.ZZY_Value = string.Empty;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST), false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 20m, ZString.Empty, 240m);
			AssertEquals("AmountDescription", "Remission configuration duty rate. 20% for Ad-Valorem rate for Remission.", invoiceLine.DutiesAndTaxes[2].AmountDescription);
			AssertEquals("C1_OriginalAmount", 60m, invoiceLine.DutiesAndTaxes[2].C1_OriginalAmount);

			rulingconfig.ZZY_Type = RefCusRulingConfigTypes.Codes.ExemptCode;
			rulingconfig.ZZY_Value = "002";
			rulingconfig.ZZY_Rate = 0;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertTax(invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST), false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5m, ZString.Empty, 0m);
			AssertEquals("AmountDescription", "Remission configuration duty amount. 0 for Exempt Code: 002.", invoiceLine.DutiesAndTaxes[2].AmountDescription);
			AssertEquals("C1_ExemptCode", "002", invoiceLine.DutiesAndTaxes[2].C1_ExemptCode);
			AssertEquals("C1_OriginalAmount", 60m, invoiceLine.DutiesAndTaxes[2].C1_OriginalAmount);
		}

		public void TestCalculateGSTWithDutyRemissionConfigs()
		{
			var classHeader = GetClassHeader("4234567890");
			PrepareGlobalTariffData(Factory, "4234567890");
			PrepareRefFiles();
			invoiceLine.JI_Tariff = "4234567890";
			invoiceLine.CA_99TariffCode = string.Empty;
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_CustomsUnitQty = customsUnits1;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();

			var gstRate = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST);
			AssertTax(gstRate, false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5m, ZString.Empty, 60m);
			AssertEquals("AmountDescription", "The normal value for tax ($1200) is multiplied by the Ad Valorem rate (5%).", gstRate.AmountDescription);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
			var rulingconfig = invoiceLine.RulingConfigurations.AddNew();
			rulingconfig.ZZY_Category = RefCusRulingConfigCategories.Codes.DTY;
			rulingconfig.ZZY_Type = RefCusRulingConfigTypes.Codes.AdValorem;
			rulingconfig.ZZY_Rate = 10m;

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			gstRate = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST);
			AssertTax(gstRate, false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5m, ZString.Empty, 55m);
			AssertEquals("AmountDescription", "The normal value for tax ($1100) is multiplied by the Ad Valorem rate (5%).", gstRate.AmountDescription);
			AssertEquals("C1_OriginalAmount", 100m, invoiceLine.DutiesAndTaxes[0].C1_OriginalAmount);
			AssertEquals("C1_OriginalAmount", 60m, gstRate.C1_OriginalAmount);
		}

		#endregion

		public void TestDDPDeductDutyOnly()
		{
			PrepareRefFiles();
			invoiceHeader.JZ_InvoiceNumber = "1";
			invoiceHeader.JZ_IncoTerm = "DDP";
			invoiceHeader.JZ_InvoiceAmount = 50;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "CAD";
			invoiceHeader.JZ_InvoiceCurrExRate = 1;
			invoiceHeader.CA_TreatmentCode = "02";

			invoiceLine.JI_FormattedTariff = classificationNumber3;
			invoiceLine.CA_99TariffCode = string.Empty;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.CA_CustomsValueOvr = false;
			invoiceLine.CA_ADJValue = 0;
			invoiceLine.CA_CVforCurrConv = 0;
			invoiceLine.CA_CVforCurrConvOvr = false;
			invoiceLine.CA_CustomsValue = 0;
			invoiceLine.JI_NetWeight = 1;
			invoiceLine.JI_CustomsSecondQuantity = 0;
			invoiceLine.JI_CustomsThirdQuantity = 0;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;

			invoiceHeader.CA_DDPDeductDutyOnly = false;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals("PreCondition: Flag not checked, CA_DDPDeductDutyOnly indicator is false", false, invoiceHeader.CA_DDPDeductDutyOnly);
			AssertEquals("Customs Value = Invoice value with GST and Duty deducted", 39.68m, invoiceLine.CA_CustomsValue);

			invoiceHeader.CA_DDPDeductDutyOnly = true;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals("PreCondition: flag checked, CA_DDPDeductDutyOnly indicator is true", true, invoiceHeader.CA_DDPDeductDutyOnly);
			AssertEquals("Customs Value = Invoice value with Duty deducted only", 41.67m, invoiceLine.CA_CustomsValue);

			invoiceHeader.JZ_IncoTerm = "";
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals("PreCondition: flag unchecked, CA_DDPDeductDutyOnly indicator is false", false, invoiceHeader.CA_DDPDeductDutyOnly);
			AssertEquals("Customs value calculated using standard method", 50m, invoiceLine.CA_CustomsValue);
		}

		public void TestDDPCalculationWithCasualEntries()
		{
			Factory.ClearCachedValue<CACClassHeader>(string.Format("CACClassHeader_{0}_{1}_{2}", currentDate.ToShortDateString(), classificationNumber1, false));
			classHeader1 = GetClassHeader(classificationNumber1);

			var classRate = classHeader1.ClassRates.AddNew();
			classRate.ZB_EffectiveDate = effectiveDate;
			classRate.ZB_ExpiryDate = expiryDate;
			classRate.ZB_UnitOfMeasure = customsUnits1;

			var rate = classRate.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AdValorem, 0, 19, 0);

			var refNumHeader = Factory.New<CACTaxRefNumHeader>();
			refNumHeader.ZD_ZA_ClassNumber = classHeader1.PK;
			refNumHeader.ZD_EffectiveDate = ZDateTime.Today.AddYears(-1);
			refNumHeader.ZD_ExpiryDate = ZDateTime.Today.AddYears(1);

			var refNum1 = refNumHeader.RefNumbers.AddNew();
			refNum1.ZE_ExciseTaxRefNumber = "001";
			Factory.Save();

			Factory.ClearCachedValue<CACRate>(string.Format("CACRate|{0}|{1}|{2}|{3}", classHeader1.ZA_ClassificationNumber, TariffTreatmentCodes.Codes.UnitedStates, currentDate.ToShortDateString(), "CLS"));

			invoiceHeader.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.DeliveredDutyPaid;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "CAD";
			invoiceHeader.CA_DDPDeductDutyOnly = false;
			invoiceHeader.CA_IsCasualImport = true;
			invoiceHeader.CA_CasualImportDestinationProvince = provinceCodeNL;
			invoiceLine.CA_CVforCurrConvOvr = false;
			invoiceLine.CA_CustomsValueOvr = false;
			invoiceLine.CA_99TariffCode = ZString.Empty;
			invoiceLine.JI_Tariff = classificationNumber1;
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			var dutiesAndTaxes = invoiceLine.DutiesAndTaxes;
			AssertTax(dutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty), true, DutyAndTaxTypes.Codes.CustomsDuty, "", false, RateTypes.Codes.AdValorem, 19m, "", 138.84m);
			AssertTax(dutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST), false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5m, "", 0m);
			AssertTax(dutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CPT), false, DutyAndTaxTypes.Codes.CPT, "", false, RateTypes.Codes.AdValorem, 15m, "", 130.44m);

			invoiceHeader.CA_DDPDeductDutyOnly = true;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			dutiesAndTaxes = invoiceLine.DutiesAndTaxes;
			AssertTax(dutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty), true, DutyAndTaxTypes.Codes.CustomsDuty, "", false, RateTypes.Codes.AdValorem, 19m, "", 159.66m);
			AssertTax(dutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST), false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5m, "", 0m);
			AssertTax(dutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CPT), false, DutyAndTaxTypes.Codes.CPT, "", false, RateTypes.Codes.AdValorem, 15m, "", 150m);
		}

		public void TestDDPCalculationWithSIMADuty()
		{
			#region Prepare Tariff Data

			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = "7210490070";
			classHeader.ZA_EffectiveDate = effectiveDate;
			classHeader.ZA_ExpiryDate = expiryDate;
			classHeader.ZA_AreaCode = "900";

			var classRate = classHeader.ClassRates.AddNew();
			classRate.ZB_EffectiveDate = effectiveDate;
			classRate.ZB_ExpiryDate = expiryDate;
			classRate.ZB_RateType = CACRateHeader.RateType.ClassificationRate;
			var rate = classRate.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Free, 0m, 0m, 0m);

			var refNumHeader = classHeader.RefNumbers.AddNew();
			refNumHeader.ZD_EffectiveDate = effectiveDate;
			refNumHeader.ZD_ExpiryDate = expiryDate;
			FillRefNumber(refNumHeader.RefNumbers.AddNew(), "001", ZString.Empty);

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var taiwanTradeGroup = universalHelper.CreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.Taiwan, effectiveDate, ZDateTime.MaxSmallDateTime);
			universalHelper.AddCountry(taiwanTradeGroup, Core.Constants.CountryCodes.Taiwan, effectiveDate.Date, ZDateTime.MaxSmallDateTime.Date);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var simaTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var antiDumpingRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.AntiDumping);
			var antiDumpingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ADD, antiDumpingRateType.PK);

			var antiDumpingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "COR2018", effectiveDate, ZDateTime.MaxSmallDateTime, relatedTariffCode: "7210490070");
			var antiDumpingRelationShip = universalHelper.CreateTariffRelationship(antiDumpingTariff.PK, harmonizedTariffType.PK, "7210490070");
			var antiDumpingRate = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, effectiveDate, ZDateTime.MaxSmallDateTime, rateFormula: "0.332 * VFD");
			var antiDumpingApplicability = universalHelper.CreateCusApplicability(antiDumpingRate, taiwanTradeGroup, effectiveDate, ZDateTime.MaxSmallDateTime);

			Factory.Save();

			#endregion

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var importer = Factory.New<OrgHeader>();
				importer.FillWithValidTestData();
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;

				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "P030094";
				invoice.JZ_InvoiceAmount = 165259.10m;
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
				invoice.JZ_ValuationDateOverride = ZDateTime.Today;
				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
				invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Taiwan;
				invoice.CA_RN_NKExport = Core.Constants.CountryCodes.Taiwan;

				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "7210490070";
				invoiceLine.JI_CustomsQuantity = 100m;
				invoiceLine.JI_CustomsUnitQty = "KGM";
				invoiceLine.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
				invoiceLine.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsPaidPayableWithAdjustments;
				invoiceLine.CA_SIMADumpingNum = "COR2018";
				invoiceLine.JI_LinePrice = 165259.10m;

				var antiDumpingDuty = invoiceLine.DutiesAndTaxes.FirstOrDefault(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ADD);
				antiDumpingDuty.C1_ExemptCode = SIMACodes.Codes.C51;

				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("VFD", 118160.37m, invoiceLine.CA_CustomsValue);
				AssertEquals("VFT", 157389.61m, invoiceLine.CA_ValueForTax);
				AssertEquals("SIMA", 39229.24m, invoiceLine.JI_Calc_SIMADutyAmount);
				AssertEquals("GST", 7869.48m, invoiceLine.JI_Calc_GSTVATAmountIncludingWHEstimate);

				antiDumpingDuty.C1_ExemptCode = SIMACodes.Codes.C10;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("VFD", 157389.62m, invoiceLine.CA_CustomsValue);
				AssertEquals("VFT", 157389.62m, invoiceLine.CA_ValueForTax);
				AssertEquals("SIMA", 0m, invoiceLine.JI_Calc_SIMADutyAmount);
				AssertEquals("GST", 7869.48m, invoiceLine.JI_Calc_GSTVATAmountIncludingWHEstimate);

				antiDumpingDuty.C1_ExemptCode = SIMACodes.Codes.C51;
				antiDumpingDuty.C1_Override = true;
				antiDumpingDuty.C1_RateType = RateTypes.Codes.AdValorem;
				antiDumpingDuty.C1_Rate = 3.2m;
				antiDumpingDuty.C1_Amount = 100m;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("VFD", 157289.62m, invoiceLine.CA_CustomsValue);
				AssertEquals("VFT", 157389.62m, invoiceLine.CA_ValueForTax);
				AssertEquals("SIMA", 100m, invoiceLine.JI_Calc_SIMADutyAmount);
				AssertEquals("GST", 7869.48m, invoiceLine.JI_Calc_GSTVATAmountIncludingWHEstimate);
			}
		}

		public void TestPopulateTaxRateC1_ExemptCodeWhenOverride()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			declaration.JE_OH_Importer = importer.PK;
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "APART";
			var relationImporter = part.RelatedOrganisations.AddNew();
			relationImporter.OU_OH = importer.PK;
			relationImporter.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var importPivot = part.PivotsForBinding.AddNew();
			importPivot.CCA_GSTStatusCode = "56";
			PrepareRefFiles();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_Tariff = classificationNumber3;

			var tax1 = invoiceLine.DutiesAndTaxes.AddNew();
			tax1.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			tax1.C1_Override = false;
			tax1.C1_ExemptCode = "";
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals("56", tax1.C1_ExemptCode);

			tax1.C1_Override = true;
			tax1.C1_ExemptCode = "";
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals(ZString.Empty, tax1.C1_ExemptCode);
		}

		#region Implementation

		void AssertCount(ZString message, ZString taxType, int expectedCount)
		{
			var taxes = new List<DutyAndTax>(invoiceLine.DutiesAndTaxes.Find(tax => tax.C1_TaxType == taxType));
			AssertEquals(message, expectedCount, taxes.Count);
		}

		void AssertTax(DutyAndTax tax, bool isInRefFiles, ZString taxType, ZString code, bool ovr, ZString rateType, ZDecimal rate, ZString unitOfMeasure, ZDecimal amount)
		{
			CombineAssertions(() =>
			{
				AssertEquals("IsInRefFiles", isInRefFiles, tax.IsInRefFiles);
				AssertEquals("C1_TaxType", taxType, tax.C1_TaxType);
				AssertEquals("C1_Code", code, tax.C1_Code);
				AssertEquals("C1_Override", ovr, tax.C1_Override);
				AssertEquals("C1_RateType", rateType, tax.C1_RateType);
				AssertEquals("C1_Rate", rate, tax.C1_Rate);
				AssertEquals("C1_UnitOfMeasure", unitOfMeasure, tax.C1_UnitOfMeasure);
				AssertEquals("C1_Amount", amount, tax.C1_Amount);
			});
		}

		CACClassHeader GetClassHeader(string classificationNumber)
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = classificationNumber;
			classHeader.ZA_EffectiveDate = effectiveDate;
			classHeader.ZA_ExpiryDate = expiryDate;
			classHeader.ZA_AreaCode = "900";
			return classHeader;
		}

		void FillRateLine(CACRateLine rateLine, ZString rateType, ZDecimal min, ZDecimal regular, ZDecimal max)
		{
			rateLine.ZR_DutyRateMax = max;
			rateLine.ZR_DutyRateMin = min;
			rateLine.ZR_DutyRateRegular = regular;
			rateLine.ZR_DutyRateType = rateType;
		}

		void FillRefNumber(CACTaxRefNumber refNumber, ZString gstRefNum, ZString exciseRefNum)
		{
			refNumber.ZE_GSTRefNumber = gstRefNum;
			refNumber.ZE_ExciseTaxRefNumber = exciseRefNum;
		}

		void FillTaxRate(CACTaxRate taxRate, ZString refNumber, ZString unitOfMeasure, ZString title, ZString rateType, ZDecimal rate, bool inactive = false)
		{
			taxRate.ZH_TaxRefNumber = refNumber;
			taxRate.ZH_UnitOfMeasure = unitOfMeasure;
			taxRate.ZH_Title = title;
			taxRate.ZH_RateType = rateType;
			taxRate.ZH_Rate = rate;
			taxRate.ZH_EffectiveDate = effectiveDate;
			taxRate.ZH_ExpiryDate = expiryDate;
			taxRate.ZH_Inactive = inactive;
		}

		public static IDisposable SetReciprocalFlagForCurrentCompany(bool isReciprocal)
		{
			return new ReciprocalSetter(isReciprocal);
		}

		class ReciprocalSetter : IDisposable
		{
			public ReciprocalSetter(bool isReciprocal)
			{
				originalReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
				GlbCompany.CurrentCompany.GC_IsReciprocal = isReciprocal;

				((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
				try
				{
					GlbCompany.CurrentCompany.Factory.Save();
				}
				finally
				{
					((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = false;
				}
			}

			readonly bool originalReciprocal;

			#region IDisposable Members

			void IDisposable.Dispose()
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = originalReciprocal;
				((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
				try
				{
					GlbCompany.CurrentCompany.Factory.Save();
				}
				finally
				{
					((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = false;
				}
			}

			#endregion
		}

		public GlbBranch CABranch
		{
			get
			{
				if (cABranch == null)
				{
					cABranch = Factory.New<GlbBranch>();
					cABranch.GB_Code = "CAX";
					cABranch.GB_GC = Env.CurrentBranch.CompanyPK;
					cABranch.GB_OH_OrgProxy = Env.CurrentBranch.OrganisationPK;
					cABranch.GB_RL_NKHomePort = "CATOR";
					Factory.Save();
				}
				return cABranch;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var startDate = new ZDateTime(2020, 11, 20);
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.CreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, startDate, ZDateTime.MaxSmallDateTime);
			universalHelper.AddCountry(chinaTradeGroup, Core.Constants.CountryCodes.China, startDate.Date, ZDateTime.MaxSmallDateTime.Date);

			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var simaTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var antiDumpingRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.AntiDumping);
			var antiDumpingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ADD, antiDumpingRateType.PK);
			var surTaxRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.SUR, antiDumpingRateType.PK);
			var countervailingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CVD, antiDumpingRateType.PK);

			var surtaxTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", startDate, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(surtaxTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM");
			var surTaxRate = universalHelper.CreateRate(surtaxTariff, surTaxRateCode.PK, startDate, ZDateTime.MaxSmallDateTime, rateFormula: "0.1 * VFD");
			var surTaxApplicability = universalHelper.CreateCusApplicability(surTaxRate, chinaTradeGroup, startDate, ZDateTime.MaxSmallDateTime);
			var antiDumpingRelTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0123456789", startDate, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(antiDumpingRelTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");
			var antiDumpingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1407", startDate, ZDateTime.MaxSmallDateTime, relatedTariffCode: "0123456789");
			var antiDumpingRelationShip = universalHelper.CreateTariffRelationship(antiDumpingTariff.PK, harmonizedTariffType.PK, "0123456789");
			var antiDumpingRate = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, startDate, ZDateTime.MaxSmallDateTime, rateFormula: "100.5 * [NMB]");
			var antiDumpingApplicability = universalHelper.CreateCusApplicability(antiDumpingRate, chinaTradeGroup, startDate, ZDateTime.MaxSmallDateTime);
			countervailingRelTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0789456123", startDate, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(countervailingRelTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "TNE");
			var countervailingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1408", startDate, ZDateTime.MaxSmallDateTime, relatedTariffCode: "0789456123");
			var countervailingRelationShip = universalHelper.CreateTariffRelationship(countervailingTariff.PK, harmonizedTariffType.PK, "0789456123");
			var countervailingRate = universalHelper.CreateRate(countervailingTariff, countervailingRateCode.PK, startDate, ZDateTime.MaxSmallDateTime, rateFormula: "200 * [KGM]");
			var countervailingApplicability = universalHelper.CreateCusApplicability(countervailingRate, chinaTradeGroup, startDate, ZDateTime.MaxSmallDateTime);

			var surtaxTariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "8954671230", startDate, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(surtaxTariff1, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM");
			var surTaxRate1 = universalHelper.CreateRate(surtaxTariff1, surTaxRateCode.PK, startDate, ZDateTime.MaxSmallDateTime, rateFormula: "0.1 * VFD");
			var surTaxApplicability1 = universalHelper.CreateCusApplicability(surTaxRate1, chinaTradeGroup, startDate, ZDateTime.MaxSmallDateTime, "ADCode");

			commodityCodeCigars = "Cigars";
			commodityCodeTabSticks = "TabSticks";
			commodityCodeSpirits = "Spirits";
			commodityCodeCigarettes = "Cigarettes";
			commodityCodeCider = "Cider";
			commodityCodeSparklingWine = "SparklingWine";

			universalHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity,
				commodityCodeCigars, commodityCodeCigars, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Names.Type, CasualImportConstants.CasualImpCommodityType.Tobacco);
			universalHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity,
				commodityCodeTabSticks, commodityCodeTabSticks, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Names.Type, CasualImportConstants.CasualImpCommodityType.Tobacco);
			universalHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity,
				commodityCodeSpirits, commodityCodeSpirits, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Names.Type, CasualImportConstants.CasualImpCommodityType.Alcohol);
			universalHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity,
				commodityCodeCigarettes, commodityCodeCigarettes, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Names.Type, CasualImportConstants.CasualImpCommodityType.Tobacco);
			universalHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity,
				commodityCodeCider, commodityCodeCider, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Names.Type, CasualImportConstants.CasualImpCommodityType.Alcohol);
			universalHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity,
				commodityCodeSparklingWine, commodityCodeSparklingWine, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Names.Type, CasualImportConstants.CasualImpCommodityType.Alcohol);

			portCode1 = "0512";
			portCode2 = "0900";
			portCode3 = "0701";
			portCode4 = "0900";
			portCode5 = "0502";
			portCode6 = "0801";
			portCode7 = "0701";
			portCode8 = "0301";
			portCode9 = "0701";
			portCode10 = "0301";
			portCode11 = "0605";
			provinceCodeNT = "NT";
			provinceCodeAB = "AB";
			provinceCodeNL = "NL";
			provinceCodeMB = "MB";
			provinceCodeBC = "BC";
			provinceCodeQC = "QC";
			provinceCodeSK = "SK";
			provinceCodeON = "ON";
			universalHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, portCode1, "Inuvik", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, provinceCodeNT);
			var officeCode2 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, portCode2, "Clarenville", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, provinceCodeNL);
			var officeCode3 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, portCode3, "Calgary", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode3.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, provinceCodeAB);
			var officeCode4 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, portCode5, "Emerson", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode4.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, provinceCodeMB);
			var officeCode5 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, portCode6, "Cranbrook", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode5.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, provinceCodeBC);
			var officeCode6 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, portCode8, "Chicoutimi", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode6.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, provinceCodeQC);
			var officeCode7 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, portCode11, "Saskatoon", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(officeCode7.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, provinceCodeSK);

			universalHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "CAGSTRateCodes", Core.Constants.CountryCodes.Canada);
			universalHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_Rate, "DESC", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, Core.Constants.CountryCodes.Canada);
			universalHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_RateType, "DESC", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, Core.Constants.CountryCodes.Canada);
			var gst1 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "001", "001 DESC", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(gst1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_Rate, "5.00");
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(gst1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_RateType, RateTypes.Codes.AdValorem);
			var gst2 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "AA1", "AA1 DESC", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(gst2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_Rate, "12.00");
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(gst2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_RateType, RateTypes.Codes.Specific);
			var gst3 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "AA3", "AA3 DESC", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(gst3.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_Rate, "18.00");
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(gst3.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_RateType, RateTypes.Codes.AcceptX);

			Factory.Save();

			userContextChange = Env.SetTemporaryUserContext(Env.CurrentUser.PK, CABranch.PK.ToGuid(), Env.CurrentDepartment.PK);
			GlbCompany.CurrentCompany.SetCountry("CA");
			CACustomsDataRegistry.Instance.DefaultExciseTaxFromCustomsTariff.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			currentDate = ZDateTime.Today;
			effectiveDate = currentDate.AddDays(-1);
			expiryDate = currentDate.AddDays(+1);
			customsUnits1 = CustomsUnitOfMeasureList.Codes.Kilogram;
			customsUnits2 = CustomsUnitOfMeasureList.Codes.Gram;
			customsUnits3 = CustomsUnitOfMeasureList.Codes.Litre;
			classificationNumber1 = "1234567890";
			classificationNumber2 = "0987654321";
			classificationNumber3 = "0789456123";
			classificationNumber4 = "4567891230";
			classificationNumber5 = "1223123400";
			classificationNumber6 = "8954671230";
			tariffCode1 = "4901";
			tariffCode2 = "4902";
			tariffCode3 = "4903";
			tariffCode4 = "9905";

			CACustomsDataRegistry.Instance.DefaultGeneralRateOfDuty.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DecimalEffectiveDate { NewValue = 20, PreviousValue = 20, EffectiveDate = effectiveDate });

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			invoiceHeader.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			invoiceHeader.CA_TimeLimit = 3;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "CAD";
			invoiceHeader.JZ_InvoiceCurrExRate = 10;

			invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = classificationNumber1;
			invoiceLine.CA_99TariffCode = tariffCode1;
			invoiceLine.JI_LinePrice = 50;
			invoiceLine.CA_ADJValue = 10;
			invoiceLine.CA_CVforCurrConv = 20;
			invoiceLine.CA_CVforCurrConvOvr = true;
			invoiceLine.CA_CustomsValue = 1000;
			invoiceLine.CA_CustomsValueOvr = true;
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_CustomsUnitQty = customsUnits3;
			invoiceLine.JI_NetWeight = 1;
			invoiceLine.JI_CustomsSecondQuantity = 7;
			invoiceLine.JI_CustomsSecondUnitQty = customsUnits2;
			invoiceLine.JI_CustomsThirdQuantity = 60;
			invoiceLine.JI_CustomsThirdUnitQty = customsUnits3;

			var cacCasualImpRatesAB = CACCasualImpRates.Load(Factory, CanadianProvinceList.Codes.YukonTerritory, AdValoremBasisTypes.Codes.HST);
			if (cacCasualImpRatesAB == null)
			{
				cacCasualImpRatesAB = Factory.New<CACCasualImpRates>();
				cacCasualImpRatesAB.IR_Province = CanadianProvinceList.Codes.YukonTerritory;
				cacCasualImpRatesAB.IR_Commodity = CasualImportConstants.CasualImpRatesCommodityType.CommodityTypeHSTCode;
			}
			cacCasualImpRatesAB.IR_ProcessingType = CasualImportConstants.CasualImpRatesProcessingType.ProcessingTypeHSTCode;
			cacCasualImpRatesAB.IR_AdValoremBasis = AdValoremBasisTypes.Codes.VFT;
			cacCasualImpRatesAB.IR_RateType1 = RateTypes.Codes.AdValorem;
			cacCasualImpRatesAB.IR_RegularRate1 = 16m;
			cacCasualImpRatesAB.IR_EffectiveDateFrom = ZDateTime.Now.AddDays(-10);
			cacCasualImpRatesAB.IR_EffectiveDateTo = ZDateTime.Now.AddDays(10);
		}

		protected override void TearDown()
		{
			userContextChange.Dispose();
			base.TearDown();
		}

		GlbBranch cABranch;
		ZDateTime effectiveDate;
		ZDateTime expiryDate;
		ZDateTime currentDate;
		string customsUnits1;
		string customsUnits2;
		string customsUnits3;
		string classificationNumber1;
		string classificationNumber2;
		string classificationNumber3;
		string classificationNumber4;
		string classificationNumber5;
		string classificationNumber6;
		string tariffCode1;
		string tariffCode2;
		string tariffCode3;
		string tariffCode4;
		string portCode1;
		string portCode2;
		string portCode3;
		string portCode4;
		string portCode5;
		string portCode6;
		string portCode7;
		string portCode8;
		string portCode9;
		string portCode10;
		string portCode11;
		string provinceCodeNT;
		string provinceCodeNL;
		string provinceCodeMB;
		string provinceCodeON;
		string provinceCodeBC;
		string provinceCodeAB;
		string provinceCodeQC;
		string provinceCodeSK;
		string commodityCodeCigars;
		string commodityCodeTabSticks;
		string commodityCodeSpirits;
		string commodityCodeCigarettes;
		string commodityCodeCider;
		string commodityCodeSparklingWine;
		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CACClassHeader classHeader1;
		IDisposable userContextChange;
		TariffView countervailingRelTariff;

		#endregion
	}
}
