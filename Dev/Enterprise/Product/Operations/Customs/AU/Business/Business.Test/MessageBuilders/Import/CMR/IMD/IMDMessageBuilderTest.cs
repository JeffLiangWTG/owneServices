using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class IMDMessageBuilderTest : BaseImportMessageBuilderAbstractTest
	{
		public void TestTAndIWhenMultipleCurrenciesAreInvolved()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_ExportDate = new ZDateTime(2010, 08, 18);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 28712.80m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var exchangeRateQuery = new ZQuery(RefExchangeRateSchema.RE_ExRateType, "CUS");
			exchangeRateQuery.AddToFilter(RefExchangeRateSchema.RE_StartDate, declaration.JE_ExportDate);
			exchangeRateQuery.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, declaration.JE_ExportDate);
			var rates = invoice.Invoice_Currency.ExchangeRates.Find(exchangeRateQuery);
			RefExchangeRate rate;
			if (!rates.Any())
			{
				rate = invoice.Invoice_Currency.ExchangeRates.AddNew();
				rate.RE_ExpiryDate = declaration.JE_ExportDate;
				rate.RE_ExRateType = "CUS";
				rate.RE_StartDate = declaration.JE_ExportDate;
			}
			else
			{
				rate = rates.ElementAt(0);
			}
			rate.RE_SellRate = 0.8981m;

			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 28712.80m;
			line.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 2244.45m, Core.Constants.CurrencyCodes.UnitedStates);
			line.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1029.57m, Core.Constants.CurrencyCodes.Australia);
			line.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 132.80m, Core.Constants.CurrencyCodes.UnitedStates);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];

			var builder = new IMDMessageBuilder(entry, CMRMessageTypes.LodgeWithoutPay);
			builder.GenerateMessageText();

			MOASegment headerTILV = null, lineTILV = null, headerOFT = null;

			foreach (MOASegment segment in builder.cUSDEC.Group30[0].Group33[0].MOA)
			{
				if (segment.MonetaryAmount.MonetaryAmountTypeCodeQualifier == MonetaryAmountTypeCodeQualifierList.InsuranceAndTransportChargesCustoms)
				{
					lineTILV = segment;
					break;
				}
			}

			foreach (MOASegment segment in builder.cUSDEC.Group8[0].MOA)
			{
				if (segment.MonetaryAmount.MonetaryAmountTypeCodeQualifier == MonetaryAmountTypeCodeQualifierList.InsuranceAndTransportChargesCustoms)
				{
					headerTILV = segment;
				}
				else if (segment.MonetaryAmount.MonetaryAmountTypeCodeQualifier == MonetaryAmountTypeCodeQualifierList.InternationalFreight)
				{
					headerOFT = segment;
				}

				if (headerTILV != null && headerOFT != null)
				{
					break;
				}
			}

			AssertNotNull(headerOFT);
			AssertNotNull(headerTILV);
			AssertNotNull(lineTILV);

			AssertEquals("Header OFT", "3528.68", headerOFT.MonetaryAmount.MonetaryAmountValue);
			AssertEquals("Line and Header TILV's are the same", headerTILV.MonetaryAmount.MonetaryAmountValue, lineTILV.MonetaryAmount.MonetaryAmountValue);
			AssertEquals(Core.Constants.CurrencyCodes.Australia, headerTILV.MonetaryAmount.CurrencyIdentificationCode);
			AssertEquals(Core.Constants.CurrencyCodes.Australia, lineTILV.MonetaryAmount.CurrencyIdentificationCode);
		}

		public void TestIncotermForN30EntryWhenThingsAreNormalised()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;
			line.Charges.AddNew("OFT", 150m, "AUD");

			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			line.JI_CL = entryLine.PK;

			AssertEquals("EntryHeader is N30", true, entryHeader.IsCMRNature30);
			AssertEquals("Normalised", true, entryHeader.ShouldEntryBeNormalised);

			var builder = new IMDMessageBuilder(entryHeader, CMRMessageTypes.PreLodge);
			var message = builder.PopulateMessagesReturningResult();
			var messageText = message.EM_MessageText;
			AssertEquals("Incoterm should not be there", false, messageText.Contains("RFF+APH:FOB'"));
		}

		public void TestPopulateLinesForPostLodgeEntriesWhenGeneratingMessagesForAmendmentDetection()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			Factory.Save();
			CusEntryHeaderToTestWith.CH_HighestLineNumber = 1;
			var entryLine = CusEntryHeaderToTestWith.MergedLines[0];
			AssertEquals("Line Action Code for line should be Amend", LineAction.Amend, entryLine.ActionCodeForMessage);

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.OriginalForAmendmentDetection);
			builder.PopulateMessages();
			Factory.Save();
			var messageText = CusEntryHeaderToTestWith.Messages[0].EM_MessageText;
			AssertEquals("Action Code in CST segment should always be Insert for Amendment Detection", true, messageText.Contains("CST+1+I::95+N10::95"));
		}

		public override void TestBGMSegment()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithoutPay);
			builder.PopulateMessages();
			Factory.Save();
			var messageText = CusEntryHeaderToTestWith.Messages[0].EM_MessageText;
			Assert(messageText.Contains("BGM+929:::IMD+" + CusEntryHeaderToTestWith.CH_BGMReference + "/DAT1:1+9'"));
		}

		public void TestDoNotPopulateTDTIfAirLinePrefixIsEmpty()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForAir();
			CusEntryHeaderToTestWith.Declaration.JE_VoyageFlightNo = "";
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithoutPay);
			builder.PopulateMessages();
			var messageText = CusEntryHeaderToTestWith.Messages[0].EM_MessageText;
			AssertEquals("Empty AirLine Prefix should not populate TDT", false, messageText.Contains("TDT+20+"));

			CusEntryHeaderToTestWith = GetCusEntryHeaderForAir();
			CusEntryHeaderToTestWith.Declaration.JE_VoyageFlightNo = "QF2";
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithoutPay);
			builder.PopulateMessages();
			messageText = CusEntryHeaderToTestWith.Messages[0].EM_MessageText;
			AssertEquals("AirLine Prefix TDT", true, messageText.Contains("TDT+20++A++QF::3'"));
		}

		public void TestTILVWithEmptyAmount()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForAir();
			var invoiceLine = CusEntryHeaderToTestWith.MergedLines[0].InvoiceLines[0];
			invoiceLine.AddInfo.ZA_TILV = "0.00AUD";

			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithoutPay);
			builder.PopulateMessages();
			var messageText = CusEntryHeaderToTestWith.Messages[0].EM_MessageText;
			AssertEquals("TILV overriden with an empty amount", true, messageText.Contains("MOA+68:"));

			invoiceLine.AddInfo.ZA_TILV = "";
			CusEntryHeaderToTestWith.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithoutPay);
			builder.PopulateMessages();
			messageText = CusEntryHeaderToTestWith.Messages[1].EM_MessageText;
			AssertEquals("TILV overrides cleared", false, messageText.Contains("MOA+68:"));
		}

		public void TestReSendingOrigianlThatWasRejected()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var key = ((ICPQAHeaderAttachee)CusEntryHeaderToTestWith).LodgementQuestionKey;
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithoutPay);
			var result = builder.MessageText;
			AssertEquals("CST Line Referece - insert", true, result.Contains("CST+1+I"));
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var reloadedEntryHeader = factory2.Load<CusEntryHeader>(CusEntryHeaderToTestWith.PK);
			key = ((ICPQAHeaderAttachee)reloadedEntryHeader).LodgementQuestionKey;
			var builder2 = GetCreateMessageBuilder(reloadedEntryHeader, CMRMessageTypes.LodgeWithoutPay);
			result = builder2.MessageText;
			AssertEquals("CST Line Referece - insert", true, result.Contains("CST+1+I"));
		}

		public void TestCSTSegment()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateCST();
			ZString expectedSection = "CST++N10::95'";
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("CST Segment", expectedSection, result);

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			invoiceLine.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateCST();
			expectedSection = "CST++N20::95'";
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("CST Segment", expectedSection, result);
		}

		public void TestEmptyInvoiceTotalForHeader()
		{
			var entryHeader = GetCusEntryHeaderForSea();
			invoiceHeader.JZ_InvoiceAmount = 0m;
			invoiceLine.JI_LinePrice = 0m;
			var builder = GetCreateMessageBuilder(entryHeader, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup8();
			ZString expectedSection = @"MOA+39:0.00:" + invoiceHeader.Invoice_Currency.RX_Code + "'";
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			Assert("MOA Segment", result.IndexOf(expectedSection) >= 0);
		}

		public void TestEmptyInvoiceTotalForLine()
		{
			var entryHeader = GetCusEntryHeaderForSea();
			invoiceHeader.JZ_InvoiceAmount = 0m;
			invoiceLine.JI_LinePrice = 0m;
			var builder = GetCreateMessageBuilder(entryHeader, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup30();
			ZString expectedSection = @"MOA+38:0.00:" + invoiceHeader.Invoice_Currency.RX_Code + "'";
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			Assert("MOA Segment", result.IndexOf(expectedSection) >= 0);
		}

		public void TestTransportAndInsuranceForLine()
		{
			var entryHeader = GetCusEntryHeaderForSea();
			invoiceHeader.Charges.RemoveAndDeleteAll();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;//OFT is included while ONS not : CFR
			Customs.Business.BaseJobComInvHeaderCharge oFT = invoiceLine.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, "AUD");
			oFT.J7_IsIncludedInITOT = true;
			invoiceLine.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 10m, "AUD");
			entryHeader.ResetTotalsAndCachedValues();//To refresh calculation
			invoiceLine.CusEntryLine.ResetTotalsAndCachedValues();

			var builder = GetCreateMessageBuilder(entryHeader, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup30();
			ZString expectedSection = @"MOA+68:110.00:AUD'";
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			Assert("MOA Segment for line transport and insurance", result.IndexOf(expectedSection) >= 0);
		}

		public void TestTransportAndInsuranceSentInItsOwnCurrencyForHeader()
		{
			var futureDate = ZDateTime.Today.AddDays(1);
			var testHelper = new ZTestHelper(Factory);
			testHelper.SetExchangeRate(futureDate, futureDate.AddDays(1), 0.7629m, testHelper.USDCurrency);

			var entryHeader = GetCusEntryHeaderForSea();
			invoiceHeader.Charges.RemoveAndDeleteAll();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoiceLine.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, "USD");
			invoiceLine.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 10m, "USD");
			entryHeader.ResetTotalsAndCachedValues();
			invoiceLine.CusEntryLine.ResetTotalsAndCachedValues();

			var builder = GetCreateMessageBuilder(entryHeader, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup8();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Transport and Insurance populated in the same currency", true, result.Contains("MOA+68:110.00:USD'"));
		}

		public void TestEQDSegment()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForMail();
			AddPacksToDeclaration();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateEQD();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertEquals("Has the first parcel post number", true, result.Contains("EQD+AH+AQT1HBL::95'"));
			AssertEquals("Has the second parcel post number", true, result.Contains("EQD+AH+AQT2HBL::95'"));
			AssertEquals("Has the third parcel post number", true, result.Contains("EQD+AH+HOUSE BIL::95'"));
		}

		public void TestEQDForNature30()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForMail();
			AddPacksToDeclaration();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateEQD();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertEquals("Has the first parcel post number", true, result.Contains("EQD+AH+AQT1HBL::95'"));
			AssertEquals("Has the second parcel post number", true, result.Contains("EQD+AH+AQT2HBL::95'"));
			AssertEquals("Has the third parcel post number", true, result.Contains("EQD+AH+HOUSE BIL::95'"));

			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateEQD();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertEquals("First parcel post number not included", false, result.Contains("EQD+AH+AQT1HBL::95'"));
			AssertEquals("Second parcel post number not included", false, result.Contains("EQD+AH+AQT2HBL::95'"));
			AssertEquals("Third parcel post number not included", false, result.Contains("EQD+AH+HOUSE BILL 3::95'"));
		}

		public void TestMEASegment()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateMEA();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Gross Weight", true, result.Contains("MEA+AAE+G+KG:123.45000'"));

			invoiceHeader.JZ_Weight = 0M;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateMEA();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Gross Weight", false, result.Contains("MEA+AAE+G"));
		}

		public void TestMEASegmentForEXW()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateMEA();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Gross Weight", true, result.Contains("MEA+AAE+G+KG:123.45000'"));

			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateMEA();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Gross Weight", false, result.Contains("MEA+AAE+G+KG:123.45000'"));
		}

		public void TestDTMSegmentWithEffectiveDutyDate()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateDTM();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Effective Duty Date", false, result.Contains("DTM+7'"));

			invoiceHeader.AddInfo.ZA_EFD = "050505";
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateDTM();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Effective Duty Date", true, result.Contains("DTM+7:20050505:102'"));
		}

		public void TestDTMSegmentWithDateOfValuation()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.JE_ExportDate = new ZDateTime(2005, 2, 15);
			testDec.JE_DateOfArrival = new ZDateTime(2005, 2, 16);

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateDTM();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("DTM Segment has date of valuation", true, result.Contains("DTM+260:20050215:102'"));
		}

		public void TestDTMSegmentWithWeeklySettlement()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.SettlementPeriodStartDate = new ZDateTime(2007, 10, 01);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			testDec.JE_SettlementPeriodType = "SM";

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateDTM();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("DTM Segment contains settlement period start date", true, result.Contains("DTM+165:20071001:102'"));
			AssertEquals("DTM Segment contains settlement period end date", true, result.Contains("DTM+166:20071031:102'"));

			testDec.JE_SettlementPeriodType = ZString.Empty;

			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateDTM();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("DTM Segment does not contains settlement period start date", false, result.Contains("DTM+165:"));
			AssertEquals("DTM Segment does not contains settlement period end date", false, result.Contains("DTM+166:"));
		}

		public void TestRFFForHeaderValuationAdvice()
		{
			var entryHeader = GetCusEntryHeaderForSea();
			invoiceHeader.AddInfo.ZA_VAN = "279230";
			var builder = GetCreateMessageBuilder(entryHeader, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString expectedSection = @"RFF+ABA:279230'";
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("RFF Segment", true, result.Contains(expectedSection));
		}

		public void TestRFFForITOTIncoterm()
		{
			var entryHeader = GetCusEntryHeaderForSea();
			invoiceHeader.Charges.RemoveAndDeleteAll();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;//OFT is included while ONS not : CFR
			Customs.Business.BaseJobComInvHeaderCharge oFT = invoiceHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, "AUD");
			oFT.J7_IsIncludedInITOT = true;
			invoiceHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 10m, "AUD");

			var builder = GetCreateMessageBuilder(entryHeader, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString expectedSection = @"RFF+APH:CFR'";
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			Assert("RFF Segment", result.IndexOf(expectedSection) >= 0);
		}

		public override void TestLocationsSegment()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateLocations();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Port of destination", true, result.Contains("LOC+8+AUMEL::6'"));
			AssertEquals("Port of discharge", true, result.Contains("LOC+12+AUSYD::6'"));
			AssertEquals("Port of loading", true, result.Contains("LOC+9+SGSIN::6'"));
			AssertEquals("First arrival port code", true, result.Contains("LOC+79+AUSYD::6'"));

			testDec.AddInfo.ZA_AQISInspectLocation_Hidden = "Inspection Location";
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateLocations();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Port of destination", true, result.Contains("LOC+8+AUMEL::6'"));
			AssertEquals("Port of discharge", true, result.Contains("LOC+12+AUSYD::6'"));
			AssertEquals("Port of loading", true, result.Contains("LOC+9+SGSIN::6'"));
			AssertEquals("First arrival port code", true, result.Contains("LOC+79+AUSYD::6'"));
			AssertEquals("AQIS Inspection Location", true, result.Contains("LOC+90+:::INSPECTION LOCATION'"));
		}

		public void TestWarehouseIDInLocationSegmentForNature20()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			invoiceLine.JI_IsPackToBondForLine = false;
			CheckWarehouseID(CusEntryHeaderToTestWith, false);

			invoiceLine.JI_IsPackToBondForLine = true;
			CheckWarehouseID(CusEntryHeaderToTestWith, true);
		}

		public void TestWarehouseIDInLocationSegmentForNature20OnLineLevel()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			var creator = new MergedDeclarationCreator2Line(Factory, ZDateTime.Now);
			creator.InvoiceLine1.JI_IsPackToBondForLine = false;
			creator.InvoiceLine2.JI_IsPackToBondForLine = true;
			creator.InvoiceLine2.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).MainAddress.PK;
			creator.InvoiceLine2.AddInfo.WarehouseAddress.LocalControlledPremisesID = "WAREHOUSEID";
			CheckWarehouseID(creator.Entry1, true);
		}

		public void TestWarehouseIDInLocationSegmentForNature30OnLineLevel()
		{
			var creator = new MergedDeclarationCreator(Factory);
			creator.Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			creator.InvoiceLine1.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).MainAddress.PK;
			creator.InvoiceLine1.AddInfo.WarehouseAddress.LocalControlledPremisesID = "WAREHOUSEID";
			CheckWarehouseID(creator.Entry1, true);
		}

		public override void TestGISSegment()
		{
			Env.Registry.RequestOfficialCustomsPaymentReceipt = true;

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithPay);
			builder.cUSDEC = new CUSDECMessage();

			builder.PopulateGIS();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("EFT Payment Approved", true, result.Contains("GIS+EPA:109:95'"));
			AssertEquals("EFT Payment Indicator", true, result.Contains("GIS+Y:153:95'"));
			AssertEquals("Total Liabilities breakdown Indicator", true, result.Contains("GIS+TLB:109:95'"));
			AssertEquals("Line Liabilities breakdown Indicator", true, result.Contains("GIS+LLB:109:95'"));
			AssertEquals("Request Official receipt", true, result.Contains("GIS+POR:109:95'"));

			builder = new IMDMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithPay);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGIS();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("EFT Payment Approved", true, result.Contains("GIS+EPA:109:95'"));
			AssertEquals("EFT Payment Indicator", true, result.Contains("GIS+Y:153:95'"));
			AssertEquals("Total Liabilities breakdown Indicator", true, result.Contains("GIS+TLB:109:95'"));
			AssertEquals("Line Liabilities breakdown Indicator", true, result.Contains("GIS+LLB:109:95'"));
			AssertEquals("Request Official receipt", true, result.Contains("GIS+POR:109:95'"));
		}

		public void TestGISSegmentWithPORTurnedOffInRegistry()
		{
			Env.Registry.RequestOfficialCustomsPaymentReceipt = false;

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithPay);
			builder.cUSDEC = new CUSDECMessage();

			builder.PopulateGIS();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("EFT Payment Approved", true, result.Contains("GIS+EPA:109:95'"));
			AssertEquals("EFT Payment Indicator", true, result.Contains("GIS+Y:153:95'"));
			AssertEquals("Total Liabilities breakdown Indicator", true, result.Contains("GIS+TLB:109:95'"));
			AssertEquals("Line Liabilities breakdown Indicator", true, result.Contains("GIS+LLB:109:95'"));
			AssertEquals("No Request Official receipt", false, result.Contains("GIS+POR:109:95'"));
		}

		public void TestPreLodgeDeclaration()
		{
			Env.Registry.RequestOfficialCustomsPaymentReceipt = true;

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			var builder = new IMDMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGIS();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("GIS Segment with Pre Lodge Indicator", true, result.Contains("GIS+PRE:109:95'"));
			AssertEquals("NO GIS Segment with Request for Official Receipt, is pre-lodge", false, result.Contains("GIS+POR:109:95'"));
		}

		public void TestWeeklySettlementIndicator()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			testDec.JE_SettlementPeriodType = "SQ";
			testDec.NilReturnInd = true;

			var builder = new IMDMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGIS();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Weekly settlement indicator", true, result.Contains("GIS+SQ:109:95'"));
			AssertEquals("Nil return indicator", true, result.Contains("GIS+NRT:109:95'"));

			testDec.NilReturnInd = false;

			builder = new IMDMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGIS();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Weekly settlement indicator", true, result.Contains("GIS+SQ:109:95'"));
			AssertEquals("Nil return indicator", false, result.Contains("GIS+NRT:109:95'"));

			testDec.JE_SettlementPeriodType = ZString.Empty;

			builder = new IMDMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGIS();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Weekly settlement indicator", false, result.Contains("GIS+SQ:109:95'"));
			AssertEquals("Nil return indicator", false, result.Contains("GIS+NRT:109:95'"));
		}

		public override void TestFTXSegment()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Delivery Name", true, result.Contains("FTX+DEL+++NAME OF IMPORTER'"));

			testDec.AddInfo.ZA_HART_Hidden = "Origin";
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Delivery Name", true, result.Contains("FTX+DEL+++NAME OF IMPORTER"));
			AssertEquals("Header Amber Reason Type", true, result.Contains("FTX+ABA++ORIGIN::95'"));

			testDec.JE_AmberStatement = "AMBER STATEMENT FOR THE DECLARATION";
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Delivery Name", true, result.Contains("FTX+DEL+++NAME OF IMPORTER"));
			AssertEquals("Header Amber Reason Type and Statement", true, result.Contains("FTX+ABA++ORIGIN::95+AMBER STATEMENT FOR THE DECLARATION'"));

			testDec.JE_PaidUnderProtestStatement = "PAID UNDER PROTEST STATEMENT FOR THE DECLARATION";
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Delivery Name", true, result.Contains("FTX+DEL+++NAME OF IMPORTER"));
			AssertEquals("Header Amber Reason Type and Statement", true, result.Contains("FTX+ABA++ORIGIN::95+AMBER STATEMENT FOR THE DECLARATION'"));
			AssertEquals("Paid Under Protest Statement", false, result.Contains("FTX+ACE+++PAID UNDER PROTEST STATEMENT FOR THE DECLARATION'"));

			var invoiceLine = CusEntryHeaderToTestWith.MergedLines[0].RandomLine;
			invoiceLine.AddInfo.ZA_PUP = "Y";
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Paid Under Protest Statement", true, result.Contains("FTX+ACE+++PAID UNDER PROTEST STATEMENT FOR THE DECLARATION'"));
		}

		public void TestAmberReasonLength()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.AddInfo.ZA_HART_Hidden = "Origin";
			testDec.JE_AmberStatement = longNoteValue;

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Header Amber Reason Type and Statement", true, result.Contains("FTX+ABA++ORIGIN::95+"
				+ "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
				"1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
				"12345678901234567890123456789012345678901234567890"));
		}

		public void TestAmberReasonWithEmptyHART()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.JE_AmberStatement = longNoteValue;

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Header Amber Reason Type and Statement", true, result.Contains("FTX+ABA+++"
				+ "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
				"1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
				"12345678901234567890123456789012345678901234567890"));
		}

		public void TestAmberStatementWithTwoSegments()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.AddInfo.ZA_HART_Hidden = "Origin";
			testDec.JE_AmberStatement = ZString.Replicate('1', 2560);

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Amber Statement", true, result.Contains("FTX+ABA++ORIGIN::95+" +
				"11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111:" +
				"11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111:" +
				"11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111:" +
				"11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111:" +
				"11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111'"));
		}

		public void TestPaidUnderProtestStatementLength()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.JE_PaidUnderProtestStatement = longNoteValue;
			var invoiceLine = CusEntryHeaderToTestWith.MergedLines[0].RandomLine;
			invoiceLine.AddInfo.ZA_PUP = "Y";

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Paid Under Protest Statement", true, result.Contains("FTX+ACE+++"
				+ "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
				"1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
				"12345678901234567890123456789012345678901234567890"));
		}

		public void TestPaidUnderProtestStatementWithTwoSegments()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.JE_PaidUnderProtestStatement = ZString.Replicate('1', 4000);
			var invoiceLine = CusEntryHeaderToTestWith.MergedLines[0].RandomLine;
			invoiceLine.AddInfo.ZA_PUP = "Y";

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Paid Under Protest Statement", true, result.Contains("FTX+ACE+++" +
					"11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111:" +
					"11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111:" +
					"11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111:" +
					"11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111:" +
					"11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111'" +
					"FTX+ACE+++11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111:" +
					"11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111:" +
					"11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111'"));
		}

		public void TestAgentReference()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.JE_AgentsReference = "Test";

			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.FAR;
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Agent Reference", true, result.Contains("RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'"));

			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.NSR;
			testDec.JE_AgentsReference = ZString.Empty;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Agent Reference", false, result.Contains("RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'"));

			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.NSR;
			testDec.JE_AgentsReference = "Test";
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Agent Reference", true, result.Contains("RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'"));

			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.PAR;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Agent Reference", true, result.Contains("RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'"));

			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.PAR;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Agent Reference", true, result.Contains("RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'"));

			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.PAR;
			testDec.JE_AgentsReference = ZString.Empty;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Agent Reference", false, result.Contains("RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'"));

			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.DEF;
			testDec.JE_AgentsReference = "Test";
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Agent Reference", true, result.Contains("RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'"));
		}

		public override void TestSegmentGroup1()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Importer Reference", true, result.Contains("RFF+ABQ:<<OWNER REFERENCE PLACE HOLDER>>'"));
			AssertEquals("Invoice Term Type", true, result.Contains("RFF+APH:FOB'"));

			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Importer Reference", true, result.Contains("RFF+ABQ:<<OWNER REFERENCE PLACE HOLDER>>'"));
			AssertEquals("Invoice Term Type", true, result.Contains("RFF+APH:FOB'"));

			testDec.AddInfo.ZA_FPUP_Hidden = "777777777";
			testDec.AddInfo.ZA_UPE_Hidden = "999999999";
			testDec.AddInfo.ZA_UPEIndicator_Hidden = true;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Importer Reference", true, result.Contains("RFF+ABQ:<<OWNER REFERENCE PLACE HOLDER>>'"));
			AssertEquals("Invoice Term Type", true, result.Contains("RFF+APH:FOB'"));
			AssertEquals("Unaccompanied Personal Effects Dec ID", false, result.Contains("RFF+ACD:999999999'"));
			AssertEquals("Unaccompanied Personal Effects Indicator", true, result.Contains("RFF+HDP:UPE"));
			AssertEquals("First Paid Under Protest Dec ID", false, result.Contains("RFF+ACE:777777777'"));

			invoiceLine.AddInfo.ZA_PUP = "Y";
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("First Paid Under Protest Dec ID should be built when there is at least one invoice line with PUP indicator", true, result.Contains("RFF+ACE:777777777'"));

			invoiceHeader.AddInfo.ZA_VAN = "VAN HIDDEN";
			invoiceHeader.AddInfo.ZA_DrawbackID = "123456789";
			testDec.AddInfo.ZA_FPUP_Hidden = "777777777";
			testDec.AddInfo.ZA_UPE_Hidden = "999999999";
			testDec.AddInfo.ZA_UPEIndicator_Hidden = true;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Importer Reference", true, result.Contains("RFF+ABQ:<<OWNER REFERENCE PLACE HOLDER>>'"));
			AssertEquals("Invoice Term Type", true, result.Contains("RFF+APH:FOB'"));
			AssertEquals("Unaccompanied Personal Effects Dec ID", false, result.Contains("RFF+ACD:999999999'"));
			AssertEquals("Unaccompanied Personal Effects Indicator", true, result.Contains("RFF+HDP:UPE"));
			AssertEquals("First Paid Under Protest Dec ID", true, result.Contains("RFF+ACE:777777777'"));
			AssertEquals("Valudation Basis", true, result.Contains("RFF+ABA:VAN HIDDEN'"));
			AssertEquals("DrawbackID", true, result.Contains("RFF+RF:123456789'"));
		}

		public void TestMultipleAQISConcernTypes()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.AddInfo.ZA_AQISConcern_Hidden = "CON,CON1,CON2,CON3";

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("AQIS Concern Type", true, result.Contains("RFF+AHT:CON'"));
			AssertEquals("AQIS Concern Type", true, result.Contains("RFF+AHT:CON1'"));
			AssertEquals("AQIS Concern Type", true, result.Contains("RFF+AHT:CON2'"));
			AssertEquals("AQIS Concern Type", true, result.Contains("RFF+AHT:CON3'"));

			var concernType = testDec.AQISConcernTypes.AddNew();
			concernType.Code = "CON4";
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("AQIS Concern Type", true, result.Contains("RFF+AHT:CON'"));
			AssertEquals("AQIS Concern Type", true, result.Contains("RFF+AHT:CON1'"));
			AssertEquals("AQIS Concern Type", true, result.Contains("RFF+AHT:CON2'"));
			AssertEquals("AQIS Concern Type", true, result.Contains("RFF+AHT:CON3'"));
			AssertEquals("AQIS Concern Type", true, result.Contains("RFF+AHT:CON4'"));
		}

		public void TestConcernTypeIsSorted()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.AddInfo.ZA_AQISConcern_Hidden = "CON2,CON1,CON0,CON4";

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("AQIS Concern Types are sorted", true, result.Contains("RFF+AHT:CON0'RFF+AHT:CON1'RFF+AHT:CON2'RFF+AHT:CON4'"));
		}

		public void TestNoGroup2Or3WhenBothAreEmpty()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			invoiceHeader.JZ_Nature10PackCount = 0;
			testDec.Notes.RemoveAndDeleteAll();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertEquals("No PAC Segment", false, result.Contains("PAC+"));
			AssertEquals("No PCI Segment", false, result.Contains("PCI+"));
		}

		public void TestSegmentGroup1ForOther()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.JE_TransportMode = Core.Constants.TransportModes.Other;
			testDec.JE_TotalNoOfPacks = 10;
			testDec.AddInfo.ZA_CustomsReceipt_Hidden = "RECEIPT ID";
			var note = testDec.Notes.AddNew();
			note.ST_NoteText = "MARKS AND NUMBERS FOR THE DECLARATION";
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertEquals("Customs Receipt For Goods Id", true, result.Contains("RFF+REN:RECEIPT ID'"));
			AssertEquals("Group 2 and 3", true, result.Contains("PAC+10'PCI+23+MARKS AND NUMBERS FOR THE DECLARATI:ON'"));
		}

		public void TestSegmentGroup2()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Package Count", false, result.Contains("PAC+987'"));

			testDec.JE_TransportMode = Core.Constants.TransportModes.Other;
			testDec.JE_TotalNoOfPacks = 10;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Package Count", true, result.Contains("PAC+10'"));
		}

		public void TestSegmentGroup3()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Marks And Numbers - not in message as mode is not other", false, result.Contains("PCI+23"));

			testDec.JE_TransportMode = Core.Constants.TransportModes.Other;
			testDec.JE_TotalNoOfPacks = 10;
			var note = testDec.Notes.AddNew();
			note.ST_NoteText = "MARKS AND NUMBERS FOR THE DECLARATION";
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Marks And Numbers", true, result.Contains("PAC+10'PCI+23+MARKS AND NUMBERS FOR THE DECLARATI:ON'"));

			invoiceHeader.JZ_Nature10PackCount = 0;
			note.ST_NoteText = longNoteValue;

			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Marks and Numbers", true, result.Contains("PAC+10'PCI+23+12345678901234567890123456789012345:" +
				"67890123456789012345678901234567890:" +
				"12345678901234567890123456789012345:" +
				"67890123456789012345678901234567890:" +
				"12345678901234567890123456789012345:" +
				"67890123456789012345678901234567890:" +
				"12345678901234567890123456789012345:" +
				"67890'"));
		}

		public void TestSegmentGroup3ForNature30WithDeclarationPackages()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			var note = testDec.Notes.AddNew();
			note.ST_NoteText = "MARKS AND NUMBERS FOR THE DECLARATION";
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Marks And Numbers", true, result.Contains("PAC+123'PCI+28+MARKS AND NUMBERS FOR THE DECLARATI:ON'"));

			invoiceHeader.JZ_Nature10PackCount = 987;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Marks And Numbers", true, result.Contains("PAC+123'PCI+28+MARKS AND NUMBERS FOR THE DECLARATI:ON'"));

			note.ST_NoteText = longNoteValue;

			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Marks and Numbers", true, result.Contains("PAC+123'PCI+28+12345678901234567890123456789012345:" +
				"67890123456789012345678901234567890:" +
				"12345678901234567890123456789012345:" +
				"67890123456789012345678901234567890:" +
				"12345678901234567890123456789012345:" +
				"67890123456789012345678901234567890:" +
				"12345678901234567890123456789012345:" +
				"67890'"));

			testDec.JE_TotalNoOfPacks = 0;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Marks and Numbers", true, result.Contains("PAC++1'PCI+28+12345678901234567890123456789012345:" +
				"67890123456789012345678901234567890:" +
				"12345678901234567890123456789012345:" +
				"67890123456789012345678901234567890:" +
				"12345678901234567890123456789012345:" +
				"67890123456789012345678901234567890:" +
				"12345678901234567890123456789012345:" +
				"67890'"));
		}

		public void TestSegmentGroup3ForNature30EntryHeaderPacakges()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			CusEntryHeaderToTestWith.WarehouseNumberOfPacks = 250;
			var note = testDec.Notes.AddNew();
			note.ST_NoteText = "MARKS AND NUMBERS FOR THE DECLARATION";
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Marks And Numbers", true, result.Contains("PAC+250'PCI+28+MARKS AND NUMBERS FOR THE DECLARATI:ON'"));

			invoiceHeader.JZ_Nature10PackCount = 987;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Marks And Numbers", true, result.Contains("PAC+250'PCI+28+MARKS AND NUMBERS FOR THE DECLARATI:ON'"));

			note.ST_NoteText = longNoteValue;

			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Marks and Numbers", true, result.Contains("PAC+250'PCI+28+12345678901234567890123456789012345:" +
				"67890123456789012345678901234567890:" +
				"12345678901234567890123456789012345:" +
				"67890123456789012345678901234567890:" +
				"12345678901234567890123456789012345:" +
				"67890123456789012345678901234567890:" +
				"12345678901234567890123456789012345:" +
				"67890'"));

			CusEntryHeaderToTestWith.WarehouseNumberOfPacks = 0;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Marks and Numbers", true, result.Contains("PAC+123'PCI+28+12345678901234567890123456789012345:" +
				"67890123456789012345678901234567890:" +
				"12345678901234567890123456789012345:" +
				"67890123456789012345678901234567890:" +
				"12345678901234567890123456789012345:" +
				"67890123456789012345678901234567890:" +
				"12345678901234567890123456789012345:" +
				"67890'"));

			testDec.JE_TotalNoOfPacks = 0;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Marks and Numbers", true, result.Contains("PAC++1'PCI+28+12345678901234567890123456789012345:" +
				"67890123456789012345678901234567890:" +
				"12345678901234567890123456789012345:" +
				"67890123456789012345678901234567890:" +
				"12345678901234567890123456789012345:" +
				"67890123456789012345678901234567890:" +
				"12345678901234567890123456789012345:" +
				"67890'"));
		}

		public void TestAcknowledgeQuestions()
		{
			var question = CMRLodgementQuestion.New(Factory);
			question.CQ_LodgementQuestionIdentifier = 1;
			question.CQ_LodgementQuestionStartDate = ZDateTime.Today;
			question.CQ_LodgementQuestionText = "This is a question";
			question.CQ_LodgementQuestionType = CMRCusEntryCPDec.LodgementQuestionTypes.GeneralLodgementQuestion;

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			var acknowledgeQuestion = CusEntryHeaderToTestWith.Questions.AddNew();
			acknowledgeQuestion.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			acknowledgeQuestion.ON_CPDecNum = question.CQ_LodgementQuestionIdentifier;
			acknowledgeQuestion.ON_CPDecStartDate = question.CQ_LodgementQuestionStartDate;

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Acknowledge Questions", true, result.Contains("RFF+AMG:0001'"));

			acknowledgeQuestion.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("As brokers answered No to this question", false, result.Contains("RFF+AMG:0001'"));
		}

		public void TestAcknowledgeQuestionsWhenGeneratingForAmendmentDetection()
		{
			var question = CMRLodgementQuestion.New(Factory);
			question.CQ_LodgementQuestionIdentifier = 1;
			question.CQ_LodgementQuestionStartDate = ZDateTime.Today;
			question.CQ_LodgementQuestionText = "This is a question";
			question.CQ_LodgementQuestionType = CMRCusEntryCPDec.LodgementQuestionTypes.GeneralLodgementQuestion;

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			var acknowledgeQuestion = CusEntryHeaderToTestWith.Questions.AddNew();
			acknowledgeQuestion.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			acknowledgeQuestion.ON_CPDecNum = question.CQ_LodgementQuestionIdentifier;
			acknowledgeQuestion.ON_CPDecStartDate = question.CQ_LodgementQuestionStartDate;

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Acknowledge Questions should be there for normal message types", true, result.Contains("RFF+AMG:0001'"));

			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.OriginalForAmendmentDetection);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Acknowledge Questions should not be there for original messages for amendment detection", false, result.Contains("RFF+AMG:0001'"));
		}

		public void TestYesAnswerToLodgementQuestion()
		{
			var question = CMRLodgementQuestion.New(Factory);
			question.CQ_LodgementQuestionIdentifier = 402;
			question.CQ_LodgementQuestionStartDate = ZDateTime.Today;
			question.CQ_LodgementQuestionText = "This is a question";

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			var acknowledgeQuestion = CusEntryHeaderToTestWith.Questions.AddNew();
			acknowledgeQuestion.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			acknowledgeQuestion.ON_CPDecNum = question.CQ_LodgementQuestionIdentifier;
			acknowledgeQuestion.ON_CPDecStartDate = question.CQ_LodgementQuestionStartDate;

			AssertEquals("IsNot ack", false, acknowledgeQuestion.IsAcknowledge);

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Acknowledge Questions", true, result.Contains("RFF+ADP:0402::Y'"));
		}

		public void TestNoAnswerToLodgementQuestion()
		{
			var question = CMRLodgementQuestion.New(Factory);
			question.CQ_LodgementQuestionIdentifier = 402;
			question.CQ_LodgementQuestionStartDate = ZDateTime.Today;
			question.CQ_LodgementQuestionText = "This is a question";

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			var acknowledgeQuestion = CusEntryHeaderToTestWith.Questions.AddNew();
			acknowledgeQuestion.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			acknowledgeQuestion.ON_CPDecNum = question.CQ_LodgementQuestionIdentifier;
			acknowledgeQuestion.ON_CPDecStartDate = question.CQ_LodgementQuestionStartDate;

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Acknowledge Questions", true, result.Contains("RFF+ADP:0402::N'"));
		}

		public override void TestSegmentGroup4()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup4();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Segment Group 4", true, result.Contains("TDT+20+QF1234+S+++++9203473::11'"));

			CusEntryHeaderToTestWith = GetCusEntryHeaderForAir();
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup4();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Segment Group 4", true, result.Contains("TDT+20++A++QF::3'"));

			CusEntryHeaderToTestWith = GetCusEntryHeaderForMail();
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup4();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Segment Group 4", true, result.Contains("TDT+20++P'"));
		}

		public void TestUPEMessage()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForAir();
			AUCustomsDataRegistry.Instance.UPEImplementationDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-1).ToDateTime());
			var importer = GetImporterOrg();
			testDec.JE_OH_Importer = importer.PK;

			testDec.AddInfo.ZA_UPEIndicator_Hidden = true;
			testDec.AddInfo.ZA_UPEImporterPassportNumber_Hidden = "7932374";
			testDec.AddInfo.ZA_UPEImporterPassportCountry_Hidden = "AU";
			testDec.AddInfo.ZA_UPEImporterSex_Hidden = "M";
			testDec.AddInfo.ZA_UPEImporterDOB_Hidden = new ZDateTime(1969, 7, 20);
			testDec.AddInfo.ZA_UPEChildrenCount_Hidden = 3;
			testDec.AddInfo.ZA_UPESpousePassportNumber_Hidden = "8234827";
			testDec.AddInfo.ZA_UPESpousePassportCountry_Hidden = "AU";
			testDec.AddInfo.ZA_UPESpouseName_Hidden = "Robyn Jones";

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			builder.PopulateGroup5();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertEquals("Segment Group 1", true, result.Contains("RFF+HDP:UPE"));
			AssertEquals("Segment Group 5", true, result.Contains("DOC+39:53+7932374:M::AU++3'DTM+329:19690720'DOC+39:53S+8234827::ROBYN JONES:AU"));
		}

		public override void TestSegmentGroup6()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
			brokerLicence.XZ_RefNumber = "54321";
			brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;
			testDec.Importer.LocalBusinessRegNo = "12345678901CAC";
			testDec.Importer.CustomsClientID = "12345678987";
			Env.Registry.AUCustoms.SetLocalCustomsBranchIdentifierForBranch(GlbBranch.CurrentBranch.PK.ToGuid(), "AA33HF");

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertContains("Nominee Broker Licence Number", "NAD+CB+54321::95'", result);
			AssertContains("Importer ABN", "NAD+AT+12345678901::95'", result);
			AssertNotContains("Importer ID not in message as ABN is entered", "NAD+IM+", result);
			AssertContains("Branch ID", "NAD+VT+AA33HF::95'", result);
			AssertContains("Importer CAC", "NAD+WP+CAC::95'", result);

			testDec.Importer.LocalBusinessRegNo = ZString.Empty;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertContains("Nominee Broker Licence Number", "NAD+CB+54321::95'", result);
			AssertNotContains("Importer ABN", "NAD+AT+", result);
			AssertContains("Importer ID", "NAD+IM+12345678987::95'", result);
			AssertContains("Branch ID", "NAD+VT+AA33HF::95'", result);
			AssertNotContains("Importer CAC", "NAD+WP+", result);

			testDec.Importer.LocalBusinessRegNo = ZString.Empty;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertContains("Nominee Broker Licence Number", "NAD+CB+54321::95'", result);
			AssertNotContains("Importer ABN", "NAD+AT+", result);
			AssertContains("Importer ID", "NAD+IM+12345678987::95'", result);
			AssertContains("Branch ID", "NAD+VT+AA33HF::95'", result);
			AssertNotContains("Importer CAC", "NAD+WP+", result);

			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);
			testDec.Importer.LocalBusinessRegNo = "12345678901";
			testDec.Importer.SetCustomsCode(OrgCusCode.CodeTypes.CreditAgencyCode, au, "954");
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertContains("Nominee Broker Licence Number", "NAD+CB+54321::95'", result);
			AssertContains("Importer ABN", "NAD+AT+12345678901::95'", result);
			AssertNotContains("Importer ID not in message as ABN is entered", "NAD+IM+", result);
			AssertContains("Branch ID", "NAD+VT+AA33HF::95'", result);
			AssertContains("Importer CAC", "NAD+WP+954::95'", result);
		}

		public void TestNomineeBrokerLicenceNumberInSegmentGroup6()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
			brokerLicence.XZ_RefNumber = "54321";
			brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Nominee Broker Licence Number", true, result.Contains("NAD+CB+54321::95'"));
		}

		public void TestPopulateBrokerLicenceForPreLodge()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Nominee Broker Licence Number", false, result.Contains("NAD+CB+"));

			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Nominee Broker Licence Number", false, result.Contains("NAD+CB+"));

			Env.Registry.AUCustoms.PreLodgementLicenceCode = "12345";
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Nominee Broker Licence Number", true, result.Contains("NAD+CB+12345::95'"));

			Env.Registry.AUCustoms.PreLodgementLicenceCode = ZString.Empty;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Nominee Broker Licence Number", false, result.Contains("NAD+CB+12345::95'"));

			var brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
			brokerLicence.XZ_RefNumber = "54321";
			brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Nominee Broker Licence Number", true, result.Contains("NAD+CB+54321::95'"));

			Env.Registry.AUCustoms.PreLodgementLicenceCode = "12345";
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			AssertEquals("Nominee Broker Licence Number", true, result.Contains("NAD+CB+54321::95'"));
		}

		public void TestBrokerLicenceWithIsImporterFlag()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
			brokerLicence.XZ_RefNumber = "54321";
			brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;

			var importer = GetImporterOrg();
			importer.LocalBusinessRegNo = GlbCompany.CurrentCompany.GC_BusinessRegNo;
			testDec.JE_OH_Importer = importer.PK;
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Nominee Broker Licence Number is not included", false, result.Contains("NAD+CB+54321::95'"));

			importer.LocalBusinessRegNo = "123456789";
			testDec.JE_OH_Importer = importer.PK;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Nominee Broker Licence Number is included", true, result.Contains("NAD+CB+54321::95'"));

			var aBN = GlbCompany.CurrentCompany.GC_BusinessRegNo;
			importer.LocalBusinessRegNo = ZString.Empty;
			testDec.JE_OH_Importer = importer.PK;
			GlbCompany.CurrentCompany.GC_BusinessRegNo = ZString.Empty;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Nominee Broker Licence Number is included", true, result.Contains("NAD+CB+54321::95'"));

			GlbCompany.CurrentCompany.GC_BusinessRegNo = aBN;
		}

		public void TestBrokerLicenceWithIsSoleTraderFlag()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
			brokerLicence.XZ_RefNumber = "54321";
			brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Nominee Broker Licence Number is included", true, result.Contains("NAD+CB+54321::95'"));

			AUCustomsDataRegistry.Instance.SoleTrader.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Nominee Broker Licence Number is not included", false, result.Contains("NAD+CB+54321::95'"));
		}

		public void TestDontGenerateHeaderLevelChargesForNature30()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			CusEntryHeaderToTestWith.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;

			invoiceHeader.JZ_IncoTerm = "EXW";

			invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");
			invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 10m, "AUD");
			invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.Commission, 20m, "AUD");
			invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.Discount, 30m, "AUD");
			invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight, 40m, "AUD");
			invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.LandingCharges, 50m, "AUD");
			invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.PackingCost, 60m, "AUD");
			invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge, 70m, "AUD");
			invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, 80m, "AUD");

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup8();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertEquals("OverseasFreight should not be part of message", false, result.Contains("MOA+313'"));
			AssertEquals("Discount should not be part of message", false, result.Contains("MOA+52'"));
			AssertEquals("OverseasInsurance should not be part of message", false, result.Contains("MOA+71'"));
			AssertEquals("LandingCharges should not be part of message", false, result.Contains("MOA+78'"));
			AssertEquals("Deduction should not be part of message", false, result.Contains("MOA+103'"));
			AssertEquals("Addition should not be part of message", false, result.Contains("MOA+105'"));
			AssertEquals("PackingCosts should not be part of message", false, result.Contains("MOA+107'"));
			AssertEquals("Commission should not be part of message", false, result.Contains("MOA+265'"));
			AssertEquals("ForeignInlandFreight should not be part of message", false, result.Contains("MOA+291'"));
		}

		public void TestSegmentGroup8Result()
		{
			//These are the charges on the invoice header

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup8();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertEquals("FOB Amount", true, result.Contains("MOA+63:997.65:USD'"));
			AssertEquals("CIF Amount", true, result.Contains("MOA+141:997.65:USD'"));
			AssertEquals("Price", true, result.Contains("MOA+39:997.65:USD'"));
		}

		public void TestAQISServicePaymentAmount()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			CusEntryHeaderToTestWith.Charges.AddNew(CusEntryChargeTypeList.Codes.AQISServicePaymentAmount, 454.45M);

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup8();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			ZString expectedSection =
				"MOA+63:997.65:USD'" +
				"MOA+141:997.65:USD'" +
				"MOA+39:997.65:USD'";
			AssertMultilineEquals("Segment Group 8", expectedSection.Replace("\r\n", ""), result, '\'');
		}

		public void TestCustomsValueForEXW()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup8();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Customs Value Not Included", false, result.Contains("MOA+40:1046.35:AUD'"));

			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup8();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Customs Value", true, result.Contains("MOA+40:1046.35:AUD'"));
		}

		public void TestGroup10IsEmpty()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.Bills.RemoveAndDeleteAll();
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup10();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Group 10 is empty", false, result.Contains("DMS+1'"));
		}

		public void TestGroup10IsEmptyForNature30()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AddContainersAndPacksToDeclaration();
			CusEntryHeaderToTestWith = testDec.CustomsEntryHeaders[0];

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup10();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Group 10 is empty", false, result.Contains("DMS+1'"));
		}

		public void TestSeaGroup10ForMultiHouseBills()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			AddContainersAndPacksToDeclaration();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup10();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertEquals("'OCLU10000020' Details", true, result.Contains("PAC+++FCL:67:95'PAC+100+1'PAC+1+3'PCI+1'RFF+AAQ:OCLU10000020'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
			AssertEquals("'OCLU10000031' Details For First House Bill", true, result.Contains("PAC+++FCL:67:95'PAC+200+1'PAC+2+3'PCI+1'RFF+AAQ:OCLU10000031'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
			AssertEquals("'OCLU10000031' Details For Second House Bill", true, result.Contains("PAC+++FCL:67:95'PAC+300+1'PAC+3+3'PCI+1'RFF+AAQ:OCLU10000031'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT2HBL'"));
			AssertEquals("'OCLU10000042' Details", true, result.Contains("PAC+++FCL:67:95'PAC+400+1'PAC+4+3'PCI+1'RFF+AAQ:OCLU10000042'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));

			AssertEquals("Has Line One", true, result.Contains("LIN+1+I'"));
			AssertEquals("Has Line Two", true, result.Contains("LIN+2+I'"));
			AssertEquals("Has Line Three", true, result.Contains("LIN+3+I'"));
			AssertEquals("Has Line Four", true, result.Contains("LIN+4+I'"));
		}

		public void TestSeaGroup10ForMixtureOfSituations()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			AddContainersAndPacksToDeclaration();

			testDec.JE_ContainerMode = Core.Constants.ContainerModes.FCLMixedShipper;
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			packageForContainer1.CW_OuterPacks = 0;
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCLMixedShipper;
			packageForContainer2.CW_PackQty = 0;
			packageForContainer2SecondHBL.CW_InBondPackQty = 120;
			packageForContainer3.CW_OuterPacks = 0;
			packageForContainer3.CW_InBondPackQty = 300;
			packageForContainer3.CW_PackQty = 300;

			var container4 = testDec.CusContainers.AddNew();
			container4.CO_ContainerNumber = "AAAA1234566";
			container4.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			var package5 = testDec.Packages.AddNew();
			package5.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			package5.CW_InBondPackQty = 500;
			package5.CW_PackQty = 500;
			package5.CW_OuterPacks = 5;

			var container5 = testDec.CusContainers.AddNew();
			container5.CO_ContainerNumber = "CONTAINER5";
			container5.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			var package6 = testDec.Packages.AddNew();
			package6.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			package6.CW_ContainerNoOrEquipmentNo = container5.CO_ContainerNumber;
			package6.CW_PackQty = 600;
			package6.CW_InBondPackQty = 600;
			package6.CW_OuterPacks = 5;

			var container6 = testDec.CusContainers.AddNew();
			container6.CO_ContainerNumber = "CONTAINER6";
			container6.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			var package7 = testDec.Packages.AddNew();
			package7.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			package7.CW_ContainerNoOrEquipmentNo = container6.CO_ContainerNumber;
			package7.CW_PackQty = 800;
			package7.CW_InBondPackQty = 700;
			package7.CW_OuterPacks = 1;

			testDec.DoMerge();
			Factory.Save();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup10();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertEquals("'OCLU10000020' Details", true, result.Contains("PAC+++LCL:67:95'PAC+100+1'PCI+1'RFF+AAQ:OCLU10000020'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
			AssertEquals("'OCLU10000031' Details For First House Bill", true, result.Contains("PAC+++FCX:67:95'PAC+2+3'PCI+1'RFF+AAQ:OCLU10000031'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
			AssertEquals("'OCLU10000031' Details For Second House Bill", true, result.Contains("PAC+++FCX:67:95'PAC+300+1'PAC+120+2'PAC+3+3'PCI+1'RFF+AAQ:OCLU10000031'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT2HBL'"));
			AssertEquals("'OCLU10000042' Details", true, result.Contains("PAC+++FCL:67:95'PAC+300+1'PAC+300+2'PCI+1'RFF+AAQ:OCLU10000042'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
			AssertEquals("AAAA1234566 not included as no pivot for it", false, result.Contains("PAC+++FCL:67:95'PCI+1'RFF+AAQ:AAAA1234566'PCI+1'RFF+MB:AQT1'"));
			AssertEquals("Details For pivot with no container", true, result.Contains("PAC+++FCX:67:95'PAC+500+1'PAC+500+2'PAC+5+3'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
			AssertEquals("'CONTAINER5' Details", true, result.Contains("PAC+++FCL:67:95'PAC+600+1'PAC+600+2'PAC+5+3'PCI+1'RFF+AAQ:CONTAINER5'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
			AssertEquals("'CONTAINER6' Details", true, result.Contains("PAC+++FCL:67:95'PAC+800+1'PAC+700+2'PAC+1+3'PCI+1'RFF+AAQ:CONTAINER6'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));

			AssertEquals("Has Line One", true, result.Contains("LIN+1+I'"));
			AssertEquals("Has Line Two", true, result.Contains("LIN+2+I'"));
			AssertEquals("Has Line Three", true, result.Contains("LIN+3+I'"));
			AssertEquals("Has Line Four", true, result.Contains("LIN+4+I'"));
			AssertEquals("Has Line Five", true, result.Contains("LIN+5+I'"));
			AssertEquals("Has Line Six", true, result.Contains("LIN+6+I'"));
			AssertEquals("Has Line Seven", true, result.Contains("LIN+7+I'"));
		}

		public void TestAirGroup10WithMultiHouseBills()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForAir();
			AddPacksToDeclaration();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup10();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertEquals("Details For House Bill One", true, result.Contains("PAC+100+1'PAC+1+3'PCI+1'RFF+MWB:AQT1'PCI+1'RFF+HWB:AQT1HBL'"));
			AssertEquals("Details For House Bill Two", true, result.Contains("PAC+200+1'PAC+2+3'PCI+1'RFF+MWB:AQT1'PCI+1'RFF+HWB:AQT2HBL'"));
			AssertEquals("Details For House Bill Three", true, result.Contains("PAC+2+1'PAC+2+2'PCI+1'RFF+MWB:AQT1'PCI+1'RFF+HWB:HOUSE BILL 3'"));

			AssertEquals("Has Line One", true, result.Contains("LIN+1+I'"));
			AssertEquals("Has Line Two", true, result.Contains("LIN+2+I'"));
			AssertEquals("Has Line Three", true, result.Contains("LIN+3+I'"));

			AssertEquals("Does not have cargo type", false, result.Contains("PAC+++AIR:67:95'"));
		}

		public void TestPostGroup10WithMultiHouseBills()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForMail();
			AddPacksToDeclaration();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup10();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertEquals("Details For Packs", true, result.Contains("LIN+1+I'PAC+302+1'PAC+2+2'PAC+3+3'"));
			AssertEquals("Does not have cargo type", false, result.Contains("PAC+++MAI:67:95'"));
		}

		public void TestMultipleEntryHeaders()
		{
			var testDec = SetUpDeclarationForMultipleEntryHeaders();
			testDec.DoMerge();

			AssertEquals("Two entry headers", 2, testDec.CustomsEntryHeaders.Count);

			var entryHeader1 = testDec.CustomsEntryHeaders[0];
			var entryHeader2 = testDec.CustomsEntryHeaders[1];

			var builder = GetCreateMessageBuilder(entryHeader2, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup10();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertHouseBillsForMultipleEntryHeaders(result, entryHeader2.RandomHeader.JZ_CU_RelatedHouseBill == houseBill1.PK);
		}

		public void TestThreeEntryHeadersTwoWithSameHouseBillAndOneDifferent()
		{
			var testDec = SetUpDeclarationForMultipleEntryHeaders();

			var invoiceHeaderWithEffectiveDuty = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			PopulateWithValidTestData(invoiceHeaderWithEffectiveDuty, true);
			invoiceHeader.AddInfo.ZA_EFD = "050505";
			var invoiceLine1 = invoiceHeaderWithEffectiveDuty.JobComInvoiceLines.AddNew();
			PopulateWithValidTestData(invoiceLine1);
			var houseBill3 = testDec.Bills.AddNew();
			houseBill3.CU_HouseBill = "House Bill 3";
			houseBill3.CU_MasterBill = "AQT1";
			packageForHouseBill3 = testDec.Packages.AddNew();
			packageForHouseBill3.CW_HouseBill = houseBill3.CU_BillUniqueCode;
			packageForHouseBill3.CW_InBondPackQty = 2;
			invoiceHeaderWithEffectiveDuty.JZ_CU_RelatedHouseBill = houseBill3.PK;

			testDec.DoMerge();

			AssertEquals("Two entry headers", 2, testDec.CustomsEntryHeaders.Count);

			var builder = GetCreateMessageBuilder(testDec.CustomsEntryHeaders[0], CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup10();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertHouseBillsForThreeEntryHeadersTest(result, testDec.CustomsEntryHeaders[0].InvoiceHeaders.Length == 2);

			builder = GetCreateMessageBuilder(testDec.CustomsEntryHeaders[1], CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup10();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertHouseBillsForThreeEntryHeadersTest(result, testDec.CustomsEntryHeaders[1].InvoiceHeaders.Length == 2);
		}

		public override void TestSegmentGroup30()
		{
			TestCaseHelper.ClearTable(CMRTariffRatePeriodSnapshot.Schema.TableName);

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var supplier = OrgHeader.LoadFromCode(Factory, "ABABEU");
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, "AAA3336347E");
			invoiceHeader.JZ_OH_Supplier = supplier.PK;

			invoiceLine.JI_Tariff = "00000000 25";
			invoiceLine.JI_Description = "DESCRIPTION ON LINE";
			invoiceLine.AddInfo.ZA_ORG = "IT";
			invoiceLine.AddInfo.ZA_DCX = "NZ";
			invoiceLine.AddInfo.ZA_FOD = "300305";
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_CustomsQuantity = 123M;
			invoiceLine.AddInfo.ZA_QT2 = 543M;
			invoiceLine.AddInfo.ZA_UQ2 = "LA";
			invoiceLine.AddInfo.ZA_ISS = 54.35M;
			invoiceLine.AddInfo.ZA_LCP = 12.25M;

			var package = invoiceLine.AQISPackages.AddNew();
			package.Number = 100;
			package.Type = "One";

			invoiceLine.JI_LinePrice = 1500.00M;
			invoiceLine.AddInfo.ZA_WRN = "9B50480001C";
			invoiceLine.AddInfo.ZA_WRL = 45;

			var pAndPBizObj = invoiceLine.AQISPremisesIdAndProcessingTypes.AddNew();
			pAndPBizObj.ProcessingType = "RPT";
			pAndPBizObj.PremisesId = "Test ID";

			invoiceLine.AddInfo.ZA_WETQ = "Y";
			invoiceLine.AddInfo.ZA_LCTQ = "Y";
			invoiceLine.AddInfo.ZA_MLPI = "Y";
			invoiceLine.AddInfo.ZA_LCTI = "Y";
			invoiceLine.AddInfo.ZA_PUP = "Y";
			invoiceLine.AddInfo.ZA_REL_Hidden = "Y";
			invoiceLine.AddInfo.ZA_ODF = 123.23M;
			invoiceLine.AddInfo.ZA_DRE = 0.5M;

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup30();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			ZString expectedResult =
@"CST+1+I::95+N10::95'
FTX+AAA+++ENTRY=9B50480001C, LINE=45 ?: DESCRIPTION ON LINE'
LOC+27+IT::5'
LOC+35+NZ::5'
DTM+4:20050330:102'
MEA+AAA++NO:123.00000'
MEA+AAA++LA:543.00000'
MEA+ABX++LCO:12.25'
NAD+SU+AAA3336347E::95'
PAC+100++ONE:185:194'
MOA+38:1500.00:USD'
MOA+5:456.78:USD'
RFF+ABD:00000000'
RFF+AED:25'
RFF+ACD:9B504800'
RFF+AFV:UT'
DOC+1'
LOC+90+TEST ID::194:RPT'
GIS+WET:109:95'
GIS+LCT:109:95'
GIS+LCQ:109:95'
GIS+MLP:109:95'
GIS+PUP:109:95'
GIS+REL:109:95'
TAX+1+ADD+++:::0.5000'
TAX+9+CUD+++:::123.2300'";

			AssertMultilineEquals("Segment Group 30", expectedResult.Replace("\r\n", ""), result, '\'');
		}

		public void TestSegmentGroup30_SupplierFromInvoiceLine()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_Code = "SUP1";
			supplier1.CustomsClientID = "AAA3336347E";
			invoiceHeader.JZ_OH_Supplier = supplier1.PK;
			invoiceLine.JI_Tariff = "00000000 25";
			invoiceLine.JI_Description = "Line 1";
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_CustomsQuantity = 123M;
			invoiceLine.JI_LinePrice = 1500.00M;

			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			supplier2.OH_Code = "SUP2";
			supplier2.CustomsClientID = "BBB1234567Y";
			var line2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			line2.JI_OH_Supplier = supplier2.PK;
			line2.JI_Tariff = "00000000 25";
			line2.JI_Description = "Line 2";
			line2.JI_CustomsUnitQty = "NO";
			line2.JI_CustomsQuantity = 124M;
			line2.JI_LinePrice = 900.00M;

			testDec.DoMerge();
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup30();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertContains("Supplier1 on line1 is included", "LOC+27+JP::5'MEA+AAA++NO:123.00000'NAD+SU+AAA3336347E::95'", result);
			AssertContains("Supplier2 on line2 is included", "LOC+27+JP::5'MEA+AAA++NO:124.00000'NAD+SU+BBB1234567Y::95'", result);
		}

		public void TestTrustedTraderIdentification()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
			brokerLicence.XZ_RefNumber = "54321";
			brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;

			invoiceHeader.Supplier.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.TraderIdentificationNumber, "SUPTIN1234567890", Core.Constants.CountryCodes.Australia);

			invoiceLine.JI_Tariff = "00000000 25";
			invoiceLine.JI_Description = "DESCRIPTION ON LINE";
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_CustomsQuantity = 123M;

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup30();
			ZString generatedMessage = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertContains("Generated message has Supplier TIN code.", "NAD+AU+SUPTIN1234567890::95'", generatedMessage);
		}

		public void TestRELInGroup40()
		{
			RunRELinGroup40Check(ZString.Empty, ZString.Empty, false);
			RunRELinGroup40Check(ZString.Empty, CMRRelatedTransaction.Yes.Code, true);
			RunRELinGroup40Check(ZString.Empty, CMRRelatedTransaction.No.Code, false);
			RunRELinGroup40Check(ZString.Empty, CMRRelatedTransaction.Default.Code, false);

			RunRELinGroup40Check(CMRRelatedTransaction.Yes.Code, ZString.Empty, true);
			RunRELinGroup40Check(CMRRelatedTransaction.Yes.Code, CMRRelatedTransaction.Yes.Code, true);
			RunRELinGroup40Check(CMRRelatedTransaction.Yes.Code, CMRRelatedTransaction.No.Code, false);
			RunRELinGroup40Check(CMRRelatedTransaction.Yes.Code, CMRRelatedTransaction.Default.Code, true);

			RunRELinGroup40Check(CMRRelatedTransaction.No.Code, ZString.Empty, false);
			RunRELinGroup40Check(CMRRelatedTransaction.No.Code, CMRRelatedTransaction.Yes.Code, true);
			RunRELinGroup40Check(CMRRelatedTransaction.No.Code, CMRRelatedTransaction.No.Code, false);
			RunRELinGroup40Check(CMRRelatedTransaction.No.Code, CMRRelatedTransaction.Default.Code, false);
		}

		public void TestPopulateWithADeletedLine()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			CusEntryHeaderToTestWith.AddInfo.ZA_IsPAYRECAck_Hidden = true;
			AssertEquals("Customs charge is paid", true, CusEntryHeaderToTestWith.IsCustomsChargePaid);
			var entryLine = testDec.CustomsEntryHeaders[0].AllEntryLines[0];
			entryLine.RefundReasonCode = "126A";
			entryLine.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.DeletePending;
			var deletedLine = new DeletedLineAmendment(entryLine, CusEntryHeaderToTestWith);
			var group30 = new SegmentGroup30();
			new IMDMessageLine(deletedLine, group30, false, true).Populate(1, LineAction.Delete);
			ZString result = group30.ToString(new Edifact.UNOCCMRCharacterSet());
			ZString expectedResult = "CST+1+D::95'RFF+ABE:126A'";
			AssertEquals("Deleted line", expectedResult, result);
		}

		public void TestWithdrawalMessage()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			CusEntryHeaderToTestWith.EntryNumber = "AAAAXTETA";

			var brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
			brokerLicence.XZ_RefNumber = "54321";
			brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;

			builder.AmendmentWithdrawalReason = new CMRAmendmentWithdrawalReason();
			builder.AmendmentWithdrawalReason.ReasonText = "TEST THE WITHDRAWAL";
			var result = builder.MessageText;
			AssertEquals("Change reason", true, result.Contains("FTX+CHG+++TEST THE WITHDRAWAL'"));
			AssertEquals("Entry number", true, result.Contains("RFF+ABT:AAAAXTETA'"));
			AssertEquals("Broker reference", true, result.Contains("NAD+CB+54321::95'"));
		}

		public void TestWithdrawalMessageWithNewLineCharacter()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			CusEntryHeaderToTestWith.EntryNumber = "AAAAXTETA";

			var brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
			brokerLicence.XZ_RefNumber = "54321";
			brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;

			builder.AmendmentWithdrawalReason = new CMRAmendmentWithdrawalReason();
			builder.AmendmentWithdrawalReason.ReasonText = System.Environment.NewLine + "TEST THE WITHDRAWAL";
			var result = builder.MessageText;
			AssertEquals("Change reason" + System.Environment.NewLine + result, true, result.Contains("FTX+CHG+++TEST THE WITHDRAWAL'"));
		}

		public void TestSecondUnitOfQuantityAffectsMergeResult()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForAir();
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			PopulateWithValidTestData(invoiceLine2);

			invoiceLine.AddInfo.ZA_ADJ = ZString.Empty;
			invoiceLine.AddInfo.ZA_QT2 = 15m;
			invoiceLine.AddInfo.ZA_UQ2 = "KG";

			invoiceLine2.AddInfo.ZA_ADJ = ZString.Empty;
			invoiceLine2.AddInfo.ZA_QT2 = 10m;
			invoiceLine2.AddInfo.ZA_UQ2 = "KG";

			testDec.DoMerge();

			AssertEquals("One Entry Header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.PopulateMessages();
			var expected = "MEA+AAA++CU:10.00000'MEA+AAA++KG:25.00000'";
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("UQ 2", true, result.Contains(expected));
		}

		public void TestDoNotGenerateNADIfBrokerPasswordIsEmpty()
		{
			GlbStaff.CurrentUser.Certificates.DeleteAll();
			CusEntryHeaderToTestWith = GetCusEntryHeaderForAir();
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			testDec.DoMerge();

			AssertEquals("One Entry Header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.PopulateMessages();
			var expected = "NAD+CB+::95'";
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("No NAD without broker's password", false, result.Contains(expected));

			var brokerLicenece = GlbStaff.CurrentUser.Certificates.AddNew();
			brokerLicenece.XZ_RefNumber = "54321";
			brokerLicenece.XZ_Type = CertificateTypePairList.Codes.BR1;

			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.PopulateMessages();
			expected = "NAD+CB+:54321:95'";
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("NAD with broker's password", false, result.Contains(expected));
		}

		public void TestSOFAMessage()
		{
			AUCustomsDataRegistry.Instance.UPEImplementationDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-1).ToDateTime());
			var importer = GetImporterOrg();

			testDec = GetBasicJobDeclaration();
			var flightNo = testDec.JE_VoyageFlightNo;
			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			testDec.JE_VoyageFlightNo = flightNo;
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			testDec.JE_OH_Importer = importer.PK;
			testDec.AddInfo.ZA_SOFAIndicator_Hidden = true;

			var invoiceHeader = testDec.Invoices[0];
			invoiceHeader.JZ_InvoiceNumber = "TestInv";
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_IsPackToBondForLine = false;

			testDec.DoMerge();
			CusEntryHeaderToTestWith = testDec.CustomsEntryHeaders[0];
			AssertEquals("IsSOFA Declaration", true, testDec.IsSOFADeclaration);

			var key = ((ICPQAHeaderAttachee)CusEntryHeaderToTestWith).LodgementQuestionKey;
			key.IsSOFADeclaration = testDec.IsSOFADeclaration;
			AssertEquals("IsSOFA Declaration", true, key.IsSOFADeclaration);

			var question = Factory.New<CMRLodgementQuestion>();
			question.CQ_LodgementQuestionIdentifier = 533;
			question.CQ_LodgementQuestionStartDate = ZDateTime.Today;
			question.CQ_LodgementQuestionEndDate = ZDateTime.Today;

			testHolder = Factory.New<DummyHolder>();
			testHolder.CachedQuestionsExposed = new CachedAnsweredQuestions();

			testGenerator = new CMRLodgementQuestionGenerator(testHolder);
			testHolder.HeadersExposed = new ICPQAHeaderAttachee[] { CusEntryHeaderToTestWith };
			testGenerator.GenerateQuestions(false);
			AssertEquals("Q533 appears for SOFA declaration", true, CusEntryHeaderToTestWith.Questions.HasQuestionWithID(533));
			var questionCount = CusEntryHeaderToTestWith.Questions.Count;
			var questionIndex = 0;
			while (questionCount > 0)
			{
				CusEntryHeaderToTestWith.Questions[questionIndex].ON_Answer = true;
				CusEntryHeaderToTestWith.Questions[questionIndex].ON_AnswerCode = "Y";
				questionIndex++;
				questionCount--;
			}

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertEquals("Segment Group 1 builds SOFA Header Document segment", true, result.Contains("RFF+HDP:SOFA"));
			AssertEquals("Message builds SOFA Lodgement Question with correct AMG prefix", true, result.Contains("RFF+AMG:0533"));
		}

		public void TestGroup10LINForChangedMessage()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			AddContainersAndPacksToDeclaration();
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testDec.DoMerge();
			AssertEquals("One Entry Header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			Factory.Save();

			packageForContainer1.CW_PackQty = 200;
			testDec.DoMerge();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.Amendment);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			builder.PopulateMessages();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			Assert("LIN Line", result.Contains("LIN+1+I'"));
			Assert("Changed line Details", result.Contains("PAC+++FCL:67:95'PAC+200+1'PAC+1+3'PCI+1'RFF+AAQ:OCLU10000020'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
		}

		public void TestGroup10LINForDeletedLine()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			AddContainersAndPacksToDeclaration();
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testDec.DoMerge();
			AssertEquals("One Entry Header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			Factory.Save();

			CusEntryHeaderToTestWith.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;

			var packToDelete = packageForContainer1;
			ZInt lineNumberOfDeletedLine = packToDelete.PackingGroup.CR_HouseContainerNumber;
			testDec.Packages.RemoveAndDelete(packToDelete);
			CusEntryHeaderToTestWith.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			testDec.DoMerge();
			Factory.Save();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.Amendment);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			builder.PopulateMessages();
			var expected = "LIN+" + lineNumberOfDeletedLine.ToString() + "+D'";
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("LIN Line", true, result.Contains(expected));
			AssertEquals("Other info for deleted line is not included", false, result.Contains("PAC+++FCL:67:95'PAC+100+1'PAC+1+3'PCI+1'RFF+AAQ:OCLU10000020'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
		}

		public void TestGroup10LINForNewLine()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			AddContainersAndPacksToDeclaration();
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testDec.DoMerge();
			AssertEquals("One Entry Header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			Factory.Save();
			CusEntryHeaderToTestWith.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;

			var pack4 = testDec.Packages.AddNew();
			pack4.CW_HouseBill = houseBill2.CU_BillUniqueCode;
			pack4.CW_ContainerNoOrEquipmentNo = container3.CO_ContainerNumber;
			pack4.CW_PackQty = 400;
			pack4.CW_OuterPacks = 4;
			CusEntryHeaderToTestWith.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			testDec.DoMerge();
			Factory.Save();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.Amendment);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			builder.PopulateMessages();
			var expected = "LIN+" + pack4.PackingGroup.CR_HouseContainerNumber + "+I'";
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("LIN Line", true, result.Contains(expected));
			AssertEquals("Details for new line", true, result.Contains("PAC+++FCL:67:95'PAC+400+1'PAC+4+3'PCI+1'RFF+AAQ:OCLU10000042'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT2HBL'"));
		}

		public void TestGroup10LINForNoLineChange()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			AddContainersAndPacksToDeclaration();
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			testDec.DoMerge();
			AssertEquals("One Entry Header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			Factory.Save();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.Amendment);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			builder.PopulateMessages();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			Assert("LIN Line", result.Contains("LIN+4+I'"));
		}

		public void TestRefundReasonCodeForPendingDeletionLine()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForAir();
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			invoiceLine.AddInfo.ZA_ADJ = ZString.Empty;
			invoiceLine.AddInfo.ZA_QT2 = 15.00m;
			invoiceLine.AddInfo.ZA_UQ2 = "KG";

			var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line2.AddInfo.ZA_QT2 = 15m;
			line2.AddInfo.ZA_UQ2 = "KG";
			testDec.DoMerge();
			AssertEquals("One Entry Header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines", 2, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			testDec.CustomsEntryHeaders[0].CH_HighestLineNumber = (short)2;
			testDec.CustomsEntryHeaders[0].EntryNumber = "AAA";
			Factory.Save();

			//Delete Line2
			invoiceHeader.JobComInvoiceLines.RemoveAndDelete(line2);
			testDec.DoMerge();
			AssertEquals("Pending entry line should have one item", 1, testDec.CustomsEntryHeaders[0].PendingDeletionEntryLines.Count);
			((CusEntryLine)testDec.CustomsEntryHeaders[0].PendingDeletionEntryLines[0]).RefundReasonCode = "E";

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.Amendment);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			builder.PopulateMessages();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			var expected = "CST+2+D::95'RFF+ABE:E'";
			AssertEquals("CST Line for the deleted line with user-entered refund reason code", true, result.Contains(expected));
		}

		public void TestGroup30CSTForChangedMessage()
		{
			var tariffRate = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffRate.TT_PreferenceSchemeType = "GEN";
			tariffRate.TT_TariffClassificationNumber = "44109000";
			tariffRate.TT_RateNumber = "001";
			tariffRate.TT_StartDate = new ZDateTime(2005, 1, 1);

			var schemeForJP = CMRPreferenceSchemePeriodCountry.New(Factory);
			schemeForJP.PC_PreferenceSchemePeriodSnapshotSchemeType = "GEN";
			schemeForJP.PC_CountryCode = "JP";

			TestCaseHelper.ClearTable(CMRTariffRatePeriodSnapshot.Schema.TableName);

			CusEntryHeaderToTestWith = GetCusEntryHeaderForAir();
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			invoiceLine.AddInfo.ZA_ADJ = ZString.Empty;
			invoiceLine.AddInfo.ZA_QT2 = 15.00m;
			invoiceLine.AddInfo.ZA_UQ2 = "KG";

			testDec.DoMerge();
			testDec.CustomsEntryHeaders[0].CH_HighestLineNumber = 1;
			Factory.Save();
			AssertEquals("One Entry Header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.Amendment);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			builder.PopulateMessages();
			var expected = "CST+1+A::95+N10::95'FTX+AAA+++DESCRIPTION OF THE GOODS FOR LINE 1'LOC+27+JP::5'" +
				"MEA+AAA++CU:5.00000'MEA+AAA++KG:15.00000'NAD+SU+AAA3336347E::95'MOA+38:997.65:USD'RFF+ABD:44109000'RFF+AED:25'RFF+AFV:UT'";
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("CST Line", true, result.Contains(expected));

			Factory.Save();
			invoiceLine.JI_Description = "NEW DESCRIPTION";
			testDec.DoMerge();
			testDec.CustomsEntryHeaders[0].CH_HighestLineNumber = 1;

			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.Amendment);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			builder.PopulateMessages();
			expected = "CST+1+A::95+N10::95'FTX+AAA+++NEW DESCRIPTION'LOC+27+JP::5'" +
				"MEA+AAA++CU:5.00000'MEA+AAA++KG:15.00000'NAD+SU+AAA3336347E::95'MOA+38:997.65:USD'RFF+ABD:44109000'RFF+AED:25'RFF+AFV:UT'";
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("CST Line", true, result.Contains(expected));
		}

		public void TestGroup30CSTForDeletedLine()
		{
			TestCaseHelper.ClearTable(CMRTariffRatePeriodSnapshot.Schema.TableName);

			CusEntryHeaderToTestWith = GetCusEntryHeaderForAir();
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			invoiceLine.AddInfo.ZA_ADJ = ZString.Empty;
			invoiceLine.AddInfo.ZA_QT2 = 15m;
			invoiceLine.AddInfo.ZA_UQ2 = "KG";

			var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line2.AddInfo.ZA_QT2 = 15m;
			line2.AddInfo.ZA_UQ2 = "KG";

			testDec.DoMerge();
			testDec.CustomsEntryHeaders[0].CH_HighestLineNumber = 2;
			testDec.CustomsEntryHeaders[0].EntryNumber = "AAA";
			AssertEquals("One Entry Header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("Two entry lines", 2, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			Factory.Save();
			invoiceHeader.JobComInvoiceLines.RemoveAndDelete(line2);
			testDec.DoMerge();
			AssertEquals("Should still be two entry lines", 2, testDec.CustomsEntryHeaders[0].AllEntryLines.Count);
			var entryLine2 = testDec.CustomsEntryHeaders[0].AllEntryLines[1];
			AssertEquals("EntryLine2 should not be deleted", false, entryLine2.IsDeleted);
			AssertEquals("EntryLine2 status should be changed", Customs.Business.EntryLineStatusList.Codes.DeletePending, entryLine2.CL_CustomsPostedStatus);

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.Amendment);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			builder.PopulateMessages();
			var expected = "CST+1+A::95+N10::95'FTX+AAA+++DESCRIPTION OF THE GOODS FOR LINE 1'LOC+27+JP::5'" +
				"MEA+AAA++CU:5.00000'MEA+AAA++KG:15.00000'NAD+SU+AAA3336347E::95'MOA+38:997.65:USD'RFF+ABD:44109000'RFF+AED:25'RFF+AFV:UT'";
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			Assert("CST Line 1", result.Contains(expected));
			expected = "CST+2+D::95'UNS+S'";
			Assert("CST Line 2", result.Contains(expected));

			entryLine2.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Deleted;
			testDec.DoMerge();
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.Amendment);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			builder.PopulateMessages();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			Assert("No CST Line ", !result.Contains("CST+2+"));

			entryLine2.RefundReasonCode = "126A";
			testDec.DoMerge();
			builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.Amendment);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			builder.PopulateMessages();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			expected = "CST+2+D::95'RFF+ABE:126A'UNS+S'";
			Assert("CST Line 2", result.Contains(expected));
		}

		public void TestGroup30CSTForNewLine()
		{
			TestCaseHelper.ClearTable(CMRTariffRatePeriodSnapshot.Schema.TableName);

			CusEntryHeaderToTestWith = GetCusEntryHeaderForAir();
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			invoiceLine.AddInfo.ZA_ADJ = ZString.Empty;
			invoiceLine.AddInfo.ZA_QT2 = 15m;
			invoiceLine.AddInfo.ZA_UQ2 = "KG";

			testDec.DoMerge();
			AssertEquals("One Entry Header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			var entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.EntryNumber = "AAA";
			entryHeader.CH_HighestLineNumber = (short)1;
			Factory.Save();

			var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line2.AddInfo.ZA_QT2 = 15m;
			line2.AddInfo.ZA_UQ2 = "KG";
			testDec.DoMerge();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.Amendment);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			builder.PopulateMessages();
			var expected = "CST+1+A::95+N10::95'FTX+AAA+++DESCRIPTION OF THE GOODS FOR LINE 1'LOC+27+JP::5'" +
				"MEA+AAA++CU:5.00000'MEA+AAA++KG:15.00000'NAD+SU+AAA3336347E::95'MOA+38:997.65:USD'RFF+ABD:44109000'RFF+AED:25'RFF+AFV:UT'";
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("CST Line", true, result.Contains(expected));

			expected = "CST+2+I::95+N10::95'LOC+27+JP::5'MEA+AAA++KG:15.00000'NAD+SU+AAA3336347E::95'MOA+38:0.00:USD'";
			AssertEquals("CST Line", true, result.Contains(expected));
		}

		public void TestGroup30CSTForNoLineChange()
		{
			TestCaseHelper.ClearTable(CMRTariffRatePeriodSnapshot.Schema.TableName);

			CusEntryHeaderToTestWith = GetCusEntryHeaderForAir();
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			invoiceLine.JI_CustomsQuantity = 5m;
			invoiceLine.JI_CustomsUnitQty = "CU";
			invoiceLine.AddInfo.ZA_ADJ = ZString.Empty;
			invoiceLine.AddInfo.ZA_QT2 = 15m;
			invoiceLine.AddInfo.ZA_UQ2 = "KG";

			testDec.DoMerge();
			testDec.CustomsEntryHeaders[0].CH_HighestLineNumber = 1;
			AssertEquals("One Entry Header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("One entry line", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			Factory.Save();

			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.Amendment);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			builder.PopulateMessages();
			var expected = "CST+1+A::95+N10::95'FTX+AAA+++DESCRIPTION OF THE GOODS FOR LINE 1'LOC+27+JP::5'" +
				"MEA+AAA++CU:5.00000'MEA+AAA++KG:15.00000'NAD+SU+AAA3336347E::95'MOA+38:997.65:USD'RFF+ABD:44109000'RFF+AED:25'RFF+AFV:UT'";
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("CST Line", true, result.Contains(expected));
		}

		protected override BaseImportMessageBuilder GetMessageBuilderToTest(CMRMessageTypes messageType) => GetCreateMessageBuilder(CusEntryHeaderToTestWith, messageType);

		IMDMessageBuilder GetCreateMessageBuilder(CusEntryHeader entryHeader, CMRMessageTypes messageType)
		{
			var builder = new IMDMessageBuilder(entryHeader, messageType);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			return builder;
		}

		void CheckWarehouseID(CusEntryHeader entry, bool shouldBePresent)
		{
			var builder = GetCreateMessageBuilder(entry, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateLocations();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Warehouse ID", shouldBePresent, result.Contains("LOC+18+WAREHOUSEID::95'"));
		}

		void AssertHouseBillsForMultipleEntryHeaders(ZString result, bool isHouseBillOne)
		{
			if (isHouseBillOne)
			{
				AssertEquals("'OCLU10000020' Details", true, result.Contains("PAC+++FCL:67:95'PAC+100+1'PAC+1+3'PCI+1'RFF+AAQ:OCLU10000020'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
				AssertEquals("'OCLU10000031' Details For First House Bill", true, result.Contains("PAC+++FCL:67:95'PAC+200+1'PAC+2+3'PCI+1'RFF+AAQ:OCLU10000031'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
				AssertEquals("'OCLU10000042' Details", true, result.Contains("PAC+++FCL:67:95'PAC+400+1'PAC+4+3'PCI+1'RFF+AAQ:OCLU10000042'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
				AssertEquals("No house bill 2", false, result.Contains("RFF+BH:AQT2HBL'"));
			}
			else
			{
				AssertEquals("'OCLU10000031' Details For Second House Bill", true, result.Contains("PAC+++FCL:67:95'PAC+300+1'PAC+3+3'PCI+1'RFF+AAQ:OCLU10000031'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT2HBL'"));
				AssertEquals("No house bill 1", false, result.Contains("RFF+BH:AQT1HBL'"));
			}
		}

		void AssertHouseBillsForThreeEntryHeadersTest(ZString result, bool isEntryWithTwoHouseBills)
		{
			if (isEntryWithTwoHouseBills)
			{
				AssertEquals("'OCLU10000020' Details", true, result.Contains("PAC+++FCL:67:95'PAC+100+1'PAC+1+3'PCI+1'RFF+AAQ:OCLU10000020'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
				AssertEquals("'OCLU10000031' Details For First House Bill", true, result.Contains("PAC+++FCL:67:95'PAC+200+1'PAC+2+3'PCI+1'RFF+AAQ:OCLU10000031'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
				AssertEquals("'OCLU10000042' Details", true, result.Contains("PAC+++FCL:67:95'PAC+400+1'PAC+4+3'PCI+1'RFF+AAQ:OCLU10000042'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT1HBL'"));
				AssertEquals("No house bill 2", false, result.Contains("RFF+BH:AQT2HBL'"));
				AssertEquals("House Bill Three", true, result.Contains("PAC+2+2'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:HOUSE BILL 3'"));
			}
			else
			{
				AssertEquals("'OCLU10000031' Details For Second House Bill", true, result.Contains("PAC+++FCL:67:95'PAC+300+1'PAC+3+3'PCI+1'RFF+AAQ:OCLU10000031'PCI+1'RFF+MB:AQT1'PCI+1'RFF+BH:AQT2HBL'"));
				AssertEquals("No house bill 1", false, result.Contains("RFF+BH:AQT1HBL'"));
				AssertEquals("No house Bill 3", false, result.Contains("RFF+HWB:HOUSE BILL 3'"));
			}
		}

		JobDeclaration SetUpDeclarationForMultipleEntryHeaders()
		{
			var testDec = GetBasicJobDeclaration();
			testDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, SQLComparisonOperator.Equal, "9203473"));
			testDec.JE_VesselName = vessel.RV_Code;

			AddContainersAndPacksToDeclaration();

			var invoiceHeaderWithEffectiveDuty = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			PopulateWithValidTestData(invoiceHeaderWithEffectiveDuty, true);
			invoiceHeader.AddInfo.ZA_EFD = "050505";
			var invoiceLine1 = invoiceHeaderWithEffectiveDuty.JobComInvoiceLines.AddNew();
			PopulateWithValidTestData(invoiceLine1);
			invoiceHeaderWithEffectiveDuty.JZ_CU_RelatedHouseBill = houseBill1.PK;

			invoiceHeader.JZ_CU_RelatedHouseBill = houseBill2.PK;

			return testDec;
		}

		void RunRELinGroup40Check(ZString headerREL, ZString lineREL, bool expectedResult)
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			invoiceHeader.AddInfo.ZA_HeaderREL_Hidden = headerREL;
			invoiceLine.AddInfo.ZA_REL_Hidden = lineREL;
			var builder = GetCreateMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup30();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Related Transaction: " + headerREL + ":" + lineREL, expectedResult, result.Contains("GIS+REL:109:95'"));
		}

		void AddPacksToDeclaration()
		{
			packageForHouseBill1 = testDec.Packages.AddNew();
			packageForHouseBill1.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			packageForHouseBill1.CW_PackQty = 100;
			packageForHouseBill1.CW_OuterPacks = 1;

			packageForHouseBill2 = testDec.Packages.AddNew();
			packageForHouseBill2.CW_HouseBill = houseBill2.CU_BillUniqueCode;
			packageForHouseBill2.CW_PackQty = 200;
			packageForHouseBill2.CW_OuterPacks = 2;

			var houseBill3 = testDec.Bills.AddNew();
			houseBill3.CU_HouseBill = "House Bill 3";
			houseBill3.CU_MasterBill = "AQT1";
			packageForHouseBill3 = testDec.Packages.AddNew();
			packageForHouseBill3.CW_HouseBill = houseBill3.CU_BillUniqueCode;
			packageForHouseBill3.CW_InBondPackQty = 2;
			packageForHouseBill3.CW_PackQty = 2;

			testDec.DoMerge();
		}

		Package packageForHouseBill1;
		Package packageForHouseBill2;
		Package packageForHouseBill3;
		CMRLodgementQuestionGenerator testGenerator;
		DummyHolder testHolder;
	}
}
