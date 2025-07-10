using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUCustomsEntryPrintTest : CMRTestCase
	{
		public void TestBreakBulkAppearOnEntryPrintWhereNoContainerIsLinked()
		{
			var creator = new MergedDeclarationCreator(Factory);

			creator.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			creator.Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			creator.Declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;

			creator.Declaration.JE_MasterBill = "1";
			creator.Declaration.JE_TotalNoOfPacksPackType = "PCE";
			creator.Declaration.JE_TotalNoOfPacks = 12;
			var packGroup = (PackingGroup)creator.Declaration.PrimaryMasterBill.PackingGroups[0];

			ZString packageDetailsPrinted = new AUCustomsEntryPrint(creator.Declaration.CustomsEntryHeaders[0], true).GetPackingLine(packGroup).Text;
			AssertEquals(true, packageDetailsPrinted.Contains("(BBK) / 12 PCE"));

			creator.Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			packageDetailsPrinted = new AUCustomsEntryPrint(creator.Declaration.CustomsEntryHeaders[0], true).GetPackingLine(packGroup).Text;
			AssertEquals(false, packageDetailsPrinted.Contains("(BBK) / 12 PCE"));
			AssertEquals(true, packageDetailsPrinted.Contains("12 PCE"));
		}

		public void TestWarehouseAddress()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			var w1 = OrgHeader.New(Factory).MainAddress;
			w1.LocalControlledPremisesID = "ccp1";
			var w2 = OrgHeader.New(Factory).MainAddress;
			w2.LocalControlledPremisesID = "ccp2";
			var creator = new MergedDeclarationCreator2Line(Factory, ZDateTime.Now);
			creator.InvoiceLine2.JI_IsPackToBondForLine = true;
			creator.InvoiceLine2.AddInfo.ZA_OA_WarehouseAddress_Hidden = w2.PK;
			AssertEquals("From second line on combined n10/20", w2, new AUCustomsEntryPrint(creator.Entry1, false).WarehouseAddress);

			creator.Declaration.WarehouseDocAddress.E2_OA_Address = w1.PK;
			creator.InvoiceLine1.JI_IsPackToBondForLine = true;
			creator.Declaration.DoMerge();
			var entry1 = creator.InvoiceLine1.CusEntryLine.Header;
			var entry2 = creator.InvoiceLine2.CusEntryLine.Header;
			AssertEquals("Entry1 whs", w1, new AUCustomsEntryPrint(entry1, false).WarehouseAddress);
			AssertEquals("Entry2 whs", w2, new AUCustomsEntryPrint(entry2, false).WarehouseAddress);
		}

		public void TestCustomsDate()
		{
			var fTestEntryPrint = new AUCustomsEntryPrint(Factory.New<CusEntryHeader>(), false);
			var testDate = new ZDateTime(2004, 04, 01);

			AssertEquals("Format of Entry printed date does not match required COMPILE print format", "01APR04", fTestEntryPrint.CustomsDate(testDate));
		}

		public void TestCustomsTime()
		{
			var fTestEntryPrint = new AUCustomsEntryPrint(Factory.New<CusEntryHeader>(), false);
			var testTime = new ZDateTime(2004, 04, 01, 16, 35, 30);
			AssertEquals("Format of Entry printed date/time does not match required COMPILE print format", "01APR04 16:35 HRS", fTestEntryPrint.CustomsTime(testTime));
		}

		public void TestCustomsPackagesInWords()
		{
			AUCustomsEntryPrint fTestEntryPrint = new AUCustomsEntryPrint(Factory.New<CusEntryHeader>(), false);
			int test1 = 1;
			string expectedResult1 = "(ONE)";
			int test2 = 111;
			string expectedResult2 = "(ONE ONE ONE)";
			int test3 = 1234567890;
			string expectedResult3 = "(ONE TWO THREE FOUR FIVE SIX SEVEN EIGHT NINE ZERO)";

			AssertEquals("", expectedResult1, fTestEntryPrint.CustomsPackagesInWords(test1));
			AssertEquals("Entry print must format printed numbers in words in this manner - as distinct from 'ONE HUNDRED AND ELEVEN' for example, to mimic required COMPILE print format", expectedResult2, fTestEntryPrint.CustomsPackagesInWords(test2));
			AssertEquals("Entry print must format printed numbers in words in this expected manner", expectedResult3, fTestEntryPrint.CustomsPackagesInWords(test3));
		}

		public void TestStupidNegativeNumber()
		{
			AUCustomsEntryPrint fTestEntryPrint = new AUCustomsEntryPrint(Factory.New<CusEntryHeader>(), false);
			AssertEquals("(NEGATIVE)", fTestEntryPrint.CustomsPackagesInWords(-1));
		}

		public void TestGetEntryStatus()
		{
			CusEntryHeader entryHeader = GetCusEntryHeaderForAir();
			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrint(entryHeader, false);
			entryHeader.Messages.RemoveAndDeleteAll();
			EDIMessage message = entryHeader.Messages.AddNew();

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.NotPaid;
			TextLayout result = entryPrint.GetMsgStatus();
			AssertEquals("LODGE WITHOUT PAY PENDING", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayAckPending;
			result = entryPrint.GetMsgStatus();
			AssertEquals("LODGE WITH PAY PENDING", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingPreLodge.Code;
			result = entryPrint.GetMsgStatus();
			AssertEquals("PRE-LODGE PENDING", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			result = entryPrint.GetMsgStatus();
			AssertEquals("AMENDMENT PENDING", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingWithdrawal.Code;
			result = entryPrint.GetMsgStatus();
			AssertEquals("WITHDRAWAL PENDING", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingSAC.Code;
			result = entryPrint.GetMsgStatus();
			AssertEquals("SAC PENDING", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.FailSAC.Code;
			result = entryPrint.GetMsgStatus();
			AssertEquals("SAC FAILED", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.FailFormalLodge.Code;
			result = entryPrint.GetMsgStatus();
			AssertEquals("LODGE FAILED", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.FailPreLodge.Code;
			result = entryPrint.GetMsgStatus();
			AssertEquals("PRE-LODGE FAILED", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.FailAmendment.Code;
			result = entryPrint.GetMsgStatus();
			AssertEquals("AMENDMENT FAILED", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.FailWithdrawal.Code;
			result = entryPrint.GetMsgStatus();
			AssertEquals("WITHDRAWAL FAILED", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			result = entryPrint.GetMsgStatus();
			AssertEquals("AMENDMENT", result.Text);

			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayPending;
			result = entryPrint.GetMsgStatus();
			AssertEquals("AMENDMENT", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.ClearSAC.Code;
			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.RefundRejected;
			result = entryPrint.GetMsgStatus();
			AssertEquals("SAC", result.Text);

			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayPending;
			result = entryPrint.GetMsgStatus();
			AssertEquals("SAC", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.RefundRejected;
			result = entryPrint.GetMsgStatus();
			AssertEquals("LODGE", result.Text);

			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayPending;
			result = entryPrint.GetMsgStatus();
			AssertEquals("LODGE", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;
			result = entryPrint.GetMsgStatus();
			AssertEquals("PRE-LODGE", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingPayment.Code;
			result = entryPrint.GetMsgStatus();
			AssertEquals("LODGE PAYMENT PENDING", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.FailPayment.Code;
			result = entryPrint.GetMsgStatus();
			AssertEquals("LODGE PAYMENT FAILED", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.ClearPayment.Code;
			result = entryPrint.GetMsgStatus();
			AssertEquals("LODGE PAYMENT", result.Text);

			entryHeader.Declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;

			result = entryPrint.GetMsgStatus();
			AssertEquals("SAC PAYMENT", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingPayment.Code;
			result = entryPrint.GetMsgStatus();
			AssertEquals("SAC PAYMENT PENDING", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.FailPayment.Code;
			result = entryPrint.GetMsgStatus();
			AssertEquals("SAC PAYMENT FAILED", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
			result = entryPrint.GetMsgStatus();
			AssertEquals("WITHDRAWAL", result.Text);

			entryHeader.CH_Status = CustomsEntryStatus.HoldAwaiting.Code;
			result = entryPrint.GetMsgStatus();
			AssertEquals("NONE", result.Text);
		}

		[ExpectNoExceptions()]
		public void TestConstructor()
		{
			_ = new AUCustomsEntryPrint(Factory.New<CusEntryHeader>(), false);
		}

		public void TestEntryPrintWithNoData()
		{
			var entryPrint = new AUCustomsEntryPrint(Factory.New<CusEntryHeader>(), false);
			AssertNotNull("Even with invalid data we should get something back", entryPrint.EntryPrint);
		}

		public void TestHeaderAddInfoGetsDisplayedOnLine()
		{
			var testDec = JobDeclarationTest.CreateSendableDeclaration(Factory);
			testDec.JE_EntrySubmittedDate = new ZDateTime(2003, 3, 1);
			var invoiceLine = testDec.FilteredInvoiceLines[0];
			invoiceLine.AddInfo.ZA_GSTE = ZString.Empty;
			testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].AddInfo.ZA_GSTE = "1234";
			testDec.DoMerge();

			var entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);

			Assert("Entry Print Contains GSTE", entryPrint.EntryPrint.Contains("GSTE=1234"));
		}

		[ExpectNoExceptions]
		public void TestDoesntBlowUpIfInvoiceLinesDeleted()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ExportDate = new ZDateTime(2005, 8, 16);
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 8, 28);
			testDec.JE_DeclarationReference = "B00148999";
			JobComInvoiceHeader inv1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			inv1.JZ_InvoiceNumber = "1";
			JobComInvoiceLine invLine1 = inv1.JobComInvoiceLines.AddNew();

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			testDec.DoMerge();

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.MergedLines[0].InvoiceLines[0].Delete();
			AssertEquals("The merged lines should have no invoice lines on them to reproduce the problem", 0, entryHeader.MergedLines[0].InvoiceLines.Count);

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(entryHeader, false);
			entryPrint.Generate();
		}

		public void TestCorrectExchangeRatePrinted()
		{
			TestCaseHelper.ClearTable(RefExchangeRateSchema.Constants.TableName);
			SaveExchangeRate("USD", new ZDateTime(2005, 8, 17), 0.8m);
			SaveExchangeRate("USD", new ZDateTime(2005, 8, 18), 0.9m);
			SaveExchangeRate("IDR", new ZDateTime(2005, 8, 20), 10035m);

			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ExportDate = new ZDateTime(2005, 8, 16);
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 8, 28);
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var inv1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			inv1.JZ_InvoiceNumber = "1";
			var inv2 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			inv2.JZ_InvoiceNumber = "2";
			var inv3 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			inv3.JZ_InvoiceNumber = "3";
			var inv4 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			inv4.JZ_InvoiceNumber = "4";
			var invLine1 = inv1.JobComInvoiceLines.AddNew();
			var invLine2 = inv2.JobComInvoiceLines.AddNew();
			var invLine3 = inv3.JobComInvoiceLines.AddNew();
			var invLine4 = inv4.JobComInvoiceLines.AddNew();

			inv1.JZ_RX_NKInvoice_Currency = "USD";
			inv1.JZ_ValuationDateOverride = new ZDateTime(2005, 8, 17);
			inv2.JZ_RX_NKInvoice_Currency = "USD";
			inv2.JZ_ValuationDateOverride = new ZDateTime(2005, 8, 18);
			inv3.JZ_RX_NKInvoice_Currency = "USD";
			inv3.JZ_ValuationDateOverride = new ZDateTime(2005, 8, 19);
			inv4.JZ_RX_NKInvoice_Currency = "IDR";
			inv4.JZ_ValuationDateOverride = new ZDateTime(2005, 8, 20);

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			testDec.DoMerge();

			var entryHeader1 = testDec.CustomsEntryHeaders[0];
			entryHeader1.CH_BGMReference = "B00001000/1";
			var entryPrint1 = new AUCustomsEntryPrintForTest(entryHeader1, false);

			var entryHeader2 = testDec.CustomsEntryHeaders[1];
			entryHeader2.CH_BGMReference = "B00001000/1";
			var entryPrint2 = new AUCustomsEntryPrintForTest(entryHeader2, false);

			var entryHeader3 = testDec.CustomsEntryHeaders[2];
			entryHeader3.CH_BGMReference = "B00001000/1";
			var entryPrint3 = new AUCustomsEntryPrintForTest(entryHeader3, false);

			var entryHeader4 = testDec.CustomsEntryHeaders[3];
			entryHeader4.CH_BGMReference = "B00001000/1";
			var entryPrint4 = new AUCustomsEntryPrintForTest(entryHeader4, false);

			ZString print1 = entryPrint1.EntryPrint;
			ZString print2 = entryPrint2.EntryPrint;
			ZString print3 = entryPrint3.EntryPrint;
			ZString print4 = entryPrint4.EntryPrint;
			Assert("1st Entry Print Contains 1st Rate", print1.Contains("CRNCYS 1 USD @ 0.8 (17AUG05)"));
			Assert("2nd Entry Print Contains 2nd Rate", print2.Contains("CRNCYS 1 USD @ 0.9 (18AUG05)"));
			Assert("3rd Entry Print also Contains 2nd Rate", print3.Contains("CRNCYS 1 USD @ 0.9 (18AUG05)"));
			Assert("4th Entry Print also Contains 3rd Rate", print4.Contains("CRNCYS 1 IDR @ 10035 (20AUG05)"));
		}

		public void TestLargeCurrencyValueWithDecimalPlacesTruncatedByCharges()
		{
			TestCaseHelper.ClearTable(RefExchangeRateSchema.Constants.TableName);
			SaveExchangeRate("VND", new ZDateTime(2016, 2, 1), 15830.9638m);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = new ZDateTime(2016, 2, 1);
			declaration.JE_DateOfFirstArrival = new ZDateTime(2016, 2, 1);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";
			invoiceHeader.JZ_RX_NKInvoice_Currency = "VND";
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2016, 2, 1);

			declaration.JobComInvoiceGroupHeaders[0].JZ_GroupInvoice = true;
			declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 3105300m, "VND");
			declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 31053m, "VND");

			invoiceHeader.JZ_OH_Supplier = ZGuid.NewZGuid();
			invoiceHeader.JZ_AddInfo = "ORG=FR*PackCountForNature10_Hidden=1*ValuationBasis_Hidden=RT";
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostFreightWithAmpersand;
			invoiceHeader.JZ_InvoiceAmount = 3105300m;
			invoiceHeader.JZ_InvoiceCurrExRate = 1.000000000m;
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2016, 2, 1);
			invoiceHeader.JZ_InvoiceNumber = "4X00092";
			invoiceHeader.JZ_PaymentExRate = 1.000000000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "VND";
			invoiceHeader.JZ_Weight = 6.000m;
			invoiceHeader.JZ_WeightUQ = "KG";
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2016, 2, 1);

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_AddInfo = "ORG=FR*ValuationBasis_Hidden=TV";
			invoiceLine.JI_CustomsQuantity = 2.0000m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_Description = "CABLES FOR ELECTRONIC EQUIPMENT (INCLUDING RADIO & T.V. HOOK UP WIRES)";
			invoiceLine.JI_InvoiceQuantity = 2.00000m;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_LineNo = (short)1;
			invoiceLine.JI_LinePrice = 3105300m;
			invoiceLine.JI_CountryOfOrigin = "FR";
			invoiceLine.JI_Tariff = "8544.41.90 23";
			invoiceLine.JI_Weight = 2.000m;
			invoiceLine.JI_WeightUQ = "KG";

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();

			var customHeader = declaration.CustomsEntryHeaders[0];
			customHeader.CH_BGMReference = "B00001000/1";
			var entryPrint = new AUCustomsEntryPrintForTest(customHeader, false);

			AssertEquals(
				"Expecting large currency with 4 DPs displayed without truncation",
				true,
				entryPrint.EntryPrint.Contains("CRNCYS 1 VND @ 15830.9638 (01FEB16)"));
		}

		[TestDate(2008, 6, 18)]
		public void TestInvoiceLineGeneralPreferenceRateSpecialCase()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header.JZ_InvoiceNumber = "1";
			header.AddInfo.ZA_POC = "NZ";
			header.AddInfo.ZA_PRT = "P50";
			header.AddInfo.ZA_PST = "XYZ";

			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line3 = header.JobComInvoiceLines.AddNew();

			line1.AddInfo.ZA_PST = AUAddInfo.GeneralPreferenceRate;
			line2.AddInfo.ZA_PST = "OTH";

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "B00001000/1";
			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(entryHeader, false);

			ZString print = entryPrint.EntryPrint;

			Assert(print.Contains("ADD INFO: PST=GEN"));
			Assert(print.Contains("ADD INFO: POC=NZ*PRT=P50*PST=OTH"));
			Assert(print.Contains("ADD INFO: POC=NZ*PRT=P50*PST=XYZ"));
		}

		[TestDate(2008, 6, 18)]
		public void TestInvoiceHeaderGeneralPreferenceRateSpecialCase()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header.JZ_InvoiceNumber = "1";
			header.AddInfo.ZA_PST = AUAddInfo.GeneralPreferenceRate;

			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line3 = header.JobComInvoiceLines.AddNew();

			line2.AddInfo.ZA_PST = "OTH";
			line2.AddInfo.ZA_POC = "NZ";
			line2.AddInfo.ZA_PRT = "P50";
			line3.AddInfo.ZA_PST = AUAddInfo.GeneralPreferenceRate;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "B00001000/1";
			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(entryHeader, false);

			ZString print = entryPrint.EntryPrint;

			Assert(print.Contains("ADD INFO: PST=GEN"));
			Assert(print.Contains("ADD INFO: POC=NZ*PRT=P50*PST=OTH"));
		}

		[TestDate(2005, 3, 3)]
		public void TestTwoPageEntriesForCMR_Landscape()
		{
			JobDeclaration testDec = SetupCMRDataForTwoPageEntry();

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testDec.JE_MergeBy = "NON";
			testDec.DoMerge();
			testDec.Consignee.MiscServ.OM_IMIsGSTDeferred = ZBool.True;
			CusEntryLine mergedLine1 = testDec.CustomsEntryHeaders[0].MergedLines[0];
			mergedLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.SecurityConcession, 1234.51m);
			mergedLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.WetAmount, 1234.52m);
			mergedLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.LCTAmount, 1234.53m);
			mergedLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.CountervailingDuty, 1234.54m);
			mergedLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DumpingDuty, 1234.55m);
			mergedLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.SecurityLiability, 1234.56m);
			testDec.CustomsEntryHeaders[0].AQISServicePaymentAmount = 33m;
			testDec.CustomsEntryHeaders[0].CH_TotalPaid += 33m;

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);
			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 1
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF23
                                         BOX NO  :
O/REF   : PRO040263                      A/REF   : B00148206/1                                FOLIO    :

                                         FOB  (1) :       44796.46 = $A      44796.46         LOAD  PT : KANSAS CITY
                                         CIF  (1) :       45177.59 = $A      45177.59         FIRST PT : SYDNEY             03MAR05
                                         GRWT (KG):       17426.00 = KG      17426.00         DSCH  PT : SYDNEY             03MAR05
                                         T & I    :         381.13 = $A        381.13         DEST  PT : SYDNEY
ITERMS  : FOB
                                                                                              TOTAL CUSTOMS VALUE : $A 1157377.04
VALUATION DATE : 02MAR05                 ITOT (1) :  44796.46 = $A      44796.46
                                         OSEA (1) :    350.00 = $A        350.00              FACTOR              : 1.00000000
CRNCYS 1 AUD @ 1.0000                    ONS  (1) :     31.13 = $A         31.13
       2 USD @ 1.39 (02MAR05)                                                                 CALCULATION DATE    : 01APR05

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

001 34029000     19        US            4623.88 L         18053.13       18053.13            0.00       1805.90
    (US,US,PS)                                                               UT/SG            FREE
    ADD INFO: DMP=1234.55*ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=    20528.16      T&I=        5.94      WET=     1234.52      LCT=     1234.53      CVD=     1234.54      DMP=     1234.55
    Security=     1234.51      Security Uncollected=     1234.56

002 34039990     37        US             204.69 L          1072.15        1072.15           11.15        108.36
    (GEN)                                                                    UT/SG           0.00%+0.05449/L
    ADD INFO: ORG=US*PST=GEN*RNO=001
    LUBRICATING PREPARATIONS (INCLUDING CUTTING-OIL PREPARATIONS, BOLT OR NUT RELEAS
    VOTI=     1083.65      T&I=        0.35

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                               1777.84   *
                                                                                      * COUNTERVAILING DUTY                1234.54   *
                                                                                      * DUMPING DUTY                    1235802.44   *
TOTAL DEFERRED GST  FOR ENTRY  =   $4377.60                                           * GST                                   0.00   *
                                                                                      * WET                                1234.52   *
                                                                                      * LCT                                1234.53   *
                                                                                      * QUARANTINE SERVICES FEE              33.00   *
                                                                                      * OTHER CHARGES                        44.10   *
Delivery: ABI GAS & TOOLS                                                             *                                              *
          171 ABBOTSFORD ROAD                                                         * TOTAL AMOUNT PAYABLE ***        1242031.43 ***
          MAYNE QLD AUSTRALIA                                                         ************************************************
                                                                                      * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************

ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 2
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF23
                                         BOX NO  :
O/REF   : PRO040263                      A/REF   : B00148206/1                                FOLIO    :

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

003 34039990     37        US             204.77 L          1065.06        1065.06           11.15        107.65
    (GEN)                                                                    UT/SG           0.00%+0.05449/L
    ADD INFO: ORG=US*PST=GEN*RNO=001
    LUBRICATING PREPARATIONS (INCLUDING CUTTING-OIL PREPARATIONS, BOLT OR NUT RELEAS
    VOTI=     1076.56      T&I=        0.35

004 39072000     37   505  US            6179.74 KG        17384.86       17384.86          521.54       1791.21    TC 9604601
    (GEN)                                                                    UT/SG           3.00%
    ADD INFO: ORG=US*PST=GEN*RNO=001
    OTHER POLYETHERS EXCL POLYETHYLENE OXIDE,PROPYLENE OXIDE BASED POLYOLS
    VOTI=    17912.12      T&I=        5.72

005 29221900     40        US             740.26 KG         1578.21     1114158.79         1234.00          0.00
    (GEN)                                                                    UT/TV+ADJ       0.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ADJ=1546487USD*AMB=COPQTVD*CL2=12345678*DCX=AD*DMP=1234567.89*DRE=1.2455*DSN=123456*DTY=1234*DXP=1234753AUD*DXT=C*ELA=A
    AA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*FOD=051013*GSTE=404*ISC=1234*LCP=1213.00
    *LCTE=404*LCTI=Y*LCTQ=Y*MD2=123456*MLPI=Y*ODF=13213*ORG=US*PRI=TC:12345678*PST=GEN*PUP=Y*QT2=740.26*RNO=001*SCN=SN1234567*STD=123
    1*TAN=1233*TC2=8840003*TCI=AD:12345678*TFQ=1312*TI2=AD:13213131*TR2=131*TRN=131*UQ2=KG*VAN=1234*VID=ADFFSDAFASDFASDADFSAFSDFSDFSD
    FSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFAS
    DFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*WET=1313*WETE=404*WETQ=Y*WMC=13213213256
    N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)
    VOTI=  2350327.59      T&I=      366.91      DMP=  1234567.89

006 29221900     41        US            1469.64 KG         1694.35        1694.35            0.00        169.49
    (GEN)                                                                    UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*PST=GEN*RNO=001
    N,N-DIETHYLAMINOETHANOL (CAS 100-37-8) VERY LONG DESCRIPTION LINE THAT SHOULD MAKE THE SYSTEM PRINT AT LEAST TWO LINES 1234567890
    1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 12345678
    90 1234567890 1234567890 1234567890 1234567890
    VOTI=     1694.91      T&I=        0.56

007 34021300     53        US            1741.79 KG         2326.86        2326.86            0.00        232.76
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=     2327.63      T&I=        0.77

008 34021300     53        US             870.89 KG         1163.43        1163.43            0.00        116.38
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=     1163.81      T&I=        0.38

ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 3
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF23
                                         BOX NO  :
O/REF   : PRO040263                      A/REF   : B00148206/1                                FOLIO    :

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

009 34021300     53        US             213.18 KG          458.41         458.41            0.00         45.85
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=      458.56      T&I=        0.15

TOTAL NUMBER OF PACKAGES:         0   (ZERO)

                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print Landscape", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 3, 3)]
		public void TestTwoPageEntriesForCMR_Portrait()
		{
			AUCustomsDataRegistry.Instance.IncludeAQISServicePaymentsInTotalPayableOnEntryPrint.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			JobDeclaration testDec = SetupCMRDataForTwoPageEntry();

			for (int i = 0; i < 5; i++)
			{
				JobComInvoiceLine testLine10 = testDec.Invoices[0].JobComInvoiceLines.AddNew();
				testLine10.JI_Tariff = "2922.19.00 40";
				testLine10.JI_CustomsQuantity = 740.2600m;
				testLine10.JI_CustomsUnitQty = "KG";
				testLine10.JI_AddInfo = "WETE=404*DTY=1234.00*GSTE=404*ADJ=1546487USD*WMC=13213213256*TI2=AD:13213131*DCX=AD*ORG=US*TCI=AD:12345678*DMP=1234567.89*STD=1231.00*PUP=Y*TC2=8840003*TRN=131*QT2=131131.00*WET=1313.00*TFQ=1312*VID=ADFFSDAFASDFASDADFSAFSDFSDFSDFSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFASDFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*LCTE=404*FOD=051013*AMB=COPQTVD*TAN=1233*CL2=12345678*PRI=TC:12345678*DXP=1234753*PST=GEN*LCTI=Y*LCTQ=Y*DRE=1.2455*ISC=1234*UQ2=KG*DXT=C*DSN=123456*VAN=1234*LCP=1213.00*ODF=13213.00*MLPI=Y*MD2=123456*SCN=SN1234567*TR2=131*ELA=AAA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*RNO=001*WETQ=Y";
				testLine10.JI_Description = "N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)";
				testLine10.JI_InvoiceQuantity = 740.26000m;
				testLine10.JI_InvoiceUQ = "KG";
				testLine10.JI_LineNo = (short)1;
				testLine10.JI_LinePrice = 1578.21m;
				testLine10.JI_Weight = 740.26000m;
				testLine10.JI_WeightUQ = "KG";
			}

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testDec.JE_MergeBy = "NON";
			testDec.DoMerge();
			testDec.CustomsEntryHeaders[0].AQISServicePaymentAmount = 33m;
			testDec.CustomsEntryHeaders[0].CH_TotalPaid += 33m;

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], true);
			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 1
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:

OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF23
                                         BOX NO  :
O/REF   : PRO040263                      A/REF   : B00148206/1                                FOLIO    :

                                         FOB  (1) :       52687.51 = $A      52687.51         LOAD  PT : KANSAS CITY
                                         CIF  (1) :       53068.64 = $A      53068.64         FIRST PT : SYDNEY             03MAR05
                                         GRWT (KG):       17426.00 = KG      17426.00         DSCH  PT : SYDNEY             03MAR05
                                         T & I    :         381.13 = $A        381.13         DEST  PT : SYDNEY

                                                                                              TOTAL CUSTOMS VALUE : $A 6728170.99
ITERMS  : FOB

VALUATION DATE : 02MAR05                 ITOT (1) :  52687.51 = $A      52687.51
                                         OSEA (1) :    350.00 = $A        350.00              FACTOR              : 1.00000000
CRNCYS 1 AUD @ 1.0000                    ONS  (1) :     31.13 = $A         31.13
       2 USD @ 1.39 (02MAR05)                                                                 CALCULATION DATE    : 01APR05

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

001 34029000     19        US            4623.88 L         18053.13       18053.13            0.00       1805.41
    (US,US,PS)                                                               UT/SG            FREE
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=    18054.15      T&I=        1.02

002 34039990     37        US             204.69 L          1072.15        1072.15           11.15        108.33
    (GEN)                                                                    UT/SG           0.00%+0.05449/L
    ADD INFO: ORG=US*PST=GEN*RNO=001
    LUBRICATING PREPARATIONS (INCLUDING CUTTING-OIL PREPARATIONS, BOLT OR NUT RELEAS
    VOTI=     1083.36      T&I=        0.06

003 34039990     37        US             204.77 L          1065.06        1065.06           11.15        107.62
    (GEN)                                                                    UT/SG           0.00%+0.05449/L
    ADD INFO: ORG=US*PST=GEN*RNO=001
    LUBRICATING PREPARATIONS (INCLUDING CUTTING-OIL PREPARATIONS, BOLT OR NUT RELEAS
    VOTI=     1076.27      T&I=        0.06

004 39072000     37   505  US            6179.74 KG        17384.86       17384.86          521.54       1790.73    TC 9604601
    (GEN)                                                                    UT/SG           3.00%
    ADD INFO: ORG=US*PST=GEN*RNO=001
    OTHER POLYETHERS EXCL POLYETHYLENE OXIDE,PROPYLENE OXIDE BASED POLYOLS
    VOTI=    17907.38      T&I=        0.98

005 29221900     40        US             740.26 KG         1578.21     1114158.79         1234.00          0.00
    (GEN)                                                                    UT/TV+ADJ       0.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ADJ=1546487USD*AMB=COPQTVD*CL2=12345678*DCX=AD*DMP=1234567.89*DRE=1.2455*DSN=123456*DTY=1234*DXP=1234753AUD*DXT=C*ELA=A
    AA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*FOD=051013*GSTE=404*ISC=1234*LCP=1213.00
    *LCTE=404*LCTI=Y*LCTQ=Y*MD2=123456*MLPI=Y*ODF=13213*ORG=US*PRI=TC:12345678*PST=GEN*PUP=Y*QT2=740.26*RNO=001*SCN=SN1234567*STD=123
    1*TAN=1233*TC2=8840003*TCI=AD:12345678*TFQ=1312*TI2=AD:13213131*TR2=131*TRN=131*UQ2=KG*VAN=1234*VID=ADFFSDAFASDFASDADFSAFSDFSDFSD
    FSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFAS
    DFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*WET=1313*WETE=404*WETQ=Y*WMC=13213213256
    N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)
    VOTI=  2350023.79      T&I=       63.11      DMP=  1234567.89

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                               7947.84   *
                                                                                      * DUMPING DUTY                    7407407.34   *
                                                                                      * GST                                4376.41   *
                                                                                      * OTHER CHARGES                        44.10   *
Delivery: ABI GAS & TOOLS                                                             *                                              *
          171 ABBOTSFORD ROAD                                                         * TOTAL AMOUNT PAYABLE ***        7427161.69 ***
          MAYNE QLD AUSTRALIA                                                         * QUARANTINE SERVICES FEE              33.00   *
                                                                                      ************************************************
                                                                                      * OFFICIAL USE ONLY                            *
I ......................................................                              *                                              *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * ................................     /  /    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              * SIGNATURE OF AUTHORISING OFFICER     DATE    *
                                                                                      ************************************************
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     * WARRANTED AND RECEIPTED:                     *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      *                                              *
                                                                                      ************************************************

ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 2
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:

OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF23
                                         BOX NO  :
O/REF   : PRO040263                      A/REF   : B00148206/1                                FOLIO    :

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

006 29221900     40        US             740.26 KG         1578.21     1114158.79         1234.00          0.00
    (GEN)                                                                    UT/TV+ADJ       0.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ADJ=1546487USD*AMB=COPQTVD*CL2=12345678*DCX=AD*DMP=1234567.89*DRE=1.2455*DSN=123456*DTY=1234*DXP=1234753AUD*DXT=C*ELA=A
    AA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*FOD=051013*GSTE=404*ISC=1234*LCP=1213.00
    *LCTE=404*LCTI=Y*LCTQ=Y*MD2=123456*MLPI=Y*ODF=13213*ORG=US*PRI=TC:12345678*PST=GEN*PUP=Y*QT2=740.26*RNO=001*SCN=SN1234567*STD=123
    1*TAN=1233*TC2=8840003*TCI=AD:12345678*TFQ=1312*TI2=AD:13213131*TR2=131*TRN=131*UQ2=KG*VAN=1234*VID=ADFFSDAFASDFASDADFSAFSDFSDFSD
    FSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFAS
    DFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*WET=1313*WETE=404*WETQ=Y*WMC=13213213256
    N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)
    VOTI=  2350023.79      T&I=       63.11      DMP=  1234567.89

007 29221900     40        US             740.26 KG         1578.21     1114158.79         1234.00          0.00
    (GEN)                                                                    UT/TV+ADJ       0.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ADJ=1546487USD*AMB=COPQTVD*CL2=12345678*DCX=AD*DMP=1234567.89*DRE=1.2455*DSN=123456*DTY=1234*DXP=1234753AUD*DXT=C*ELA=A
    AA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*FOD=051013*GSTE=404*ISC=1234*LCP=1213.00
    *LCTE=404*LCTI=Y*LCTQ=Y*MD2=123456*MLPI=Y*ODF=13213*ORG=US*PRI=TC:12345678*PST=GEN*PUP=Y*QT2=740.26*RNO=001*SCN=SN1234567*STD=123
    1*TAN=1233*TC2=8840003*TCI=AD:12345678*TFQ=1312*TI2=AD:13213131*TR2=131*TRN=131*UQ2=KG*VAN=1234*VID=ADFFSDAFASDFASDADFSAFSDFSDFSD
    FSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFAS
    DFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*WET=1313*WETE=404*WETQ=Y*WMC=13213213256
    N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)
    VOTI=  2350023.79      T&I=       63.11      DMP=  1234567.89

008 29221900     40        US             740.26 KG         1578.21     1114158.79         1234.00          0.00
    (GEN)                                                                    UT/TV+ADJ       0.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ADJ=1546487USD*AMB=COPQTVD*CL2=12345678*DCX=AD*DMP=1234567.89*DRE=1.2455*DSN=123456*DTY=1234*DXP=1234753AUD*DXT=C*ELA=A
    AA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*FOD=051013*GSTE=404*ISC=1234*LCP=1213.00
    *LCTE=404*LCTI=Y*LCTQ=Y*MD2=123456*MLPI=Y*ODF=13213*ORG=US*PRI=TC:12345678*PST=GEN*PUP=Y*QT2=740.26*RNO=001*SCN=SN1234567*STD=123
    1*TAN=1233*TC2=8840003*TCI=AD:12345678*TFQ=1312*TI2=AD:13213131*TR2=131*TRN=131*UQ2=KG*VAN=1234*VID=ADFFSDAFASDFASDADFSAFSDFSDFSD
    FSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFAS
    DFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*WET=1313*WETE=404*WETQ=Y*WMC=13213213256
    N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)
    VOTI=  2350023.79      T&I=       63.11      DMP=  1234567.89

009 29221900     40        US             740.26 KG         1578.21     1114158.79         1234.00          0.00
    (GEN)                                                                    UT/TV+ADJ       0.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ADJ=1546487USD*AMB=COPQTVD*CL2=12345678*DCX=AD*DMP=1234567.89*DRE=1.2455*DSN=123456*DTY=1234*DXP=1234753AUD*DXT=C*ELA=A
    AA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*FOD=051013*GSTE=404*ISC=1234*LCP=1213.00
    *LCTE=404*LCTI=Y*LCTQ=Y*MD2=123456*MLPI=Y*ODF=13213*ORG=US*PRI=TC:12345678*PST=GEN*PUP=Y*QT2=740.26*RNO=001*SCN=SN1234567*STD=123
    1*TAN=1233*TC2=8840003*TCI=AD:12345678*TFQ=1312*TI2=AD:13213131*TR2=131*TRN=131*UQ2=KG*VAN=1234*VID=ADFFSDAFASDFASDADFSAFSDFSDFSD
    FSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFAS
    DFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*WET=1313*WETE=404*WETQ=Y*WMC=13213213256
    N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)
    VOTI=  2350023.79      T&I=       63.11      DMP=  1234567.89

010 29221900     40        US             740.26 KG         1578.21     1114158.79         1234.00          0.00
    (GEN)                                                                    UT/TV+ADJ       0.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ADJ=1546487USD*AMB=COPQTVD*CL2=12345678*DCX=AD*DMP=1234567.89*DRE=1.2455*DSN=123456*DTY=1234*DXP=1234753AUD*DXT=C*ELA=A
    AA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*FOD=051013*GSTE=404*ISC=1234*LCP=1213.00
    *LCTE=404*LCTI=Y*LCTQ=Y*MD2=123456*MLPI=Y*ODF=13213*ORG=US*PRI=TC:12345678*PST=GEN*PUP=Y*QT2=740.26*RNO=001*SCN=SN1234567*STD=123
    1*TAN=1233*TC2=8840003*TCI=AD:12345678*TFQ=1312*TI2=AD:13213131*TR2=131*TRN=131*UQ2=KG*VAN=1234*VID=ADFFSDAFASDFASDADFSAFSDFSDFSD
    FSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFAS
    DFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*WET=1313*WETE=404*WETQ=Y*WMC=13213213256
    N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)
    VOTI=  2350023.81      T&I=       63.13      DMP=  1234567.89

011 29221900     41        US            1469.64 KG         1694.35        1694.35            0.00        169.44
    (GEN)                                                                    UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*PST=GEN*RNO=001
    N,N-DIETHYLAMINOETHANOL (CAS 100-37-8) VERY LONG DESCRIPTION LINE THAT SHOULD MAKE THE SYSTEM PRINT AT LEAST TWO LINES 1234567890
    1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 12345678
    90 1234567890 1234567890 1234567890 1234567890
    VOTI=     1694.45      T&I=        0.10

012 34021300     53        US            1741.79 KG         2326.86        2326.86            0.00        232.69
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=     2326.99      T&I=        0.13

ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 3
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:

OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF23
                                         BOX NO  :
O/REF   : PRO040263                      A/REF   : B00148206/1                                FOLIO    :

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

013 34021300     53        US             870.89 KG         1163.43        1163.43            0.00        116.35
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=     1163.50      T&I=        0.07

014 34021300     53        US             213.18 KG          458.41         458.41            0.00         45.84
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=      458.44      T&I=        0.03

TOTAL NUMBER OF PACKAGES:         0   (ZERO)

                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print Portrait", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestEntryPrintWithLodgedPendingPayStatusCMRForLandscape()
		{
			JobDeclaration testDec = SetupCMRDataForTwoPageEntry();

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testDec.JE_MergeBy = "NON";
			testDec.DoMerge();

			testDec.JE_EntrySubmittedDate = new ZDateTime(2005, 3, 3);
			testDec.CustomsEntryHeaders[0].AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayPending;
			testDec.CustomsEntryHeaders[0].CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			testDec.CustomsEntryHeaders[0].CH_EntryStatus = CMRImportEntryAdvice.Finalised.Code;

			testDec.DoMerge();// again to recalc with new effective duty date

			EDIMessage message = testDec.CustomsEntryHeaders[0].Messages.AddNew();
			message.EM_SystemCreateTimeUtc = new ZDateTime(2005, 12, 25, 11, 30, 35);
			StmALog addLogForWorkComplete = testDec.Logs.AddNew(Events.DeclarationWorkComplete, "work complete");
			StmALog addLogForImpedement = testDec.CustomsEntryHeaders[0].Logs.AddNew(Events.CustomsImpedimentReceived, "quarantine impedement");
			StmALog addLogForAmendmentQueued = testDec.Logs.AddNew(Events.DeclarationAmendmentQueued, "xxx");

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);
			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 1
Finalized - Paid:work complete           X  X    X    *     AUSTRALIAN CUSTOMS     *
Impediment(s):quarantine                 X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
impedement(Amendment detected, NOT       X  X    X    ******************************
Lodged.)                                 X  XXXXXX
Last Msg: LODGE(25DEC05 11:30UTC)                       PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF23
                                         BOX NO  :
O/REF   : PRO040263                      A/REF   : B00148206/1                                FOLIO    :

                                         FOB  (1) :       44796.46 = $A      44796.46         LOAD  PT : KANSAS CITY
                                         CIF  (1) :       45177.59 = $A      45177.59         FIRST PT : SYDNEY             03MAR05
                                         GRWT (KG):       17426.00 = KG      17426.00         DSCH  PT : SYDNEY             03MAR05
                                         T & I    :         381.13 = $A        381.13         DEST  PT : SYDNEY
ITERMS  : FOB
                                                                                              TOTAL CUSTOMS VALUE : $A 1157377.04
VALUATION DATE : 02MAR05                 ITOT (1) :  44796.46 = $A      44796.46
                                         OSEA (1) :    350.00 = $A        350.00              FACTOR              : 1.00000000
CRNCYS 1 AUD @ 1.0000                    ONS  (1) :     31.13 = $A         31.13
       2 USD @ 1.39 (02MAR05)                                                                 CALCULATION DATE    : 03MAR05

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

001 34029000     19        US            4623.88 L         18053.13       18053.13            0.00       1805.90
    (US,US,PS)                                                               UT/SG            FREE
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=    18059.07      T&I=        5.94

002 34039990     37        US             204.69 L          1072.15        1072.15           11.15        108.36
    (GEN)                                                                    UT/SG           0.00%+0.05449/L
    ADD INFO: ORG=US*PST=GEN*RNO=001
    LUBRICATING PREPARATIONS (INCLUDING CUTTING-OIL PREPARATIONS, BOLT OR NUT RELEAS
    VOTI=     1083.65      T&I=        0.35

003 34039990     37        US             204.77 L          1065.06        1065.06           11.15        107.65
    (GEN)                                                                    UT/SG           0.00%+0.05449/L
    ADD INFO: ORG=US*PST=GEN*RNO=001
    LUBRICATING PREPARATIONS (INCLUDING CUTTING-OIL PREPARATIONS, BOLT OR NUT RELEAS
    VOTI=     1076.56      T&I=        0.35

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                               1777.84   *
                                                                                      * DUMPING DUTY                    1234567.89   *
                                                                                      * GST                                4377.60   *
                                                                                      * OTHER CHARGES                        44.10   *
Delivery: ABI GAS & TOOLS                                                             *                                              *
          171 ABBOTSFORD ROAD                                                         * TOTAL AMOUNT PAYABLE ***        1241998.43 ***
          MAYNE QLD AUSTRALIA                                                         ************************************************
                                                                                      * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************

ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 2
Finalized - Paid:work complete           X  X    X    *     AUSTRALIAN CUSTOMS     *
Impediment(s):quarantine                 X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
impedement(Amendment detected, NOT       X  X    X    ******************************
Lodged.)                                 X  XXXXXX
Last Msg: LODGE(25DEC05 11:30UTC)                       PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF23
                                         BOX NO  :
O/REF   : PRO040263                      A/REF   : B00148206/1                                FOLIO    :

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

004 39072000     37   505  US            6179.74 KG        17384.86       17384.86          521.54       1791.21    TC 9604601
    (GEN)                                                                    UT/SG           3.00%
    ADD INFO: ORG=US*PST=GEN*RNO=001
    OTHER POLYETHERS EXCL POLYETHYLENE OXIDE,PROPYLENE OXIDE BASED POLYOLS
    VOTI=    17912.12      T&I=        5.72

005 29221900     40        US             740.26 KG         1578.21     1114158.79         1234.00          0.00
    (GEN)                                                                    UT/TV+ADJ       0.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ADJ=1546487USD*AMB=COPQTVD*CL2=12345678*DCX=AD*DMP=1234567.89*DRE=1.2455*DSN=123456*DTY=1234*DXP=1234753AUD*DXT=C*ELA=A
    AA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*FOD=051013*GSTE=404*ISC=1234*LCP=1213.00
    *LCTE=404*LCTI=Y*LCTQ=Y*MD2=123456*MLPI=Y*ODF=13213*ORG=US*PRI=TC:12345678*PST=GEN*PUP=Y*QT2=740.26*RNO=001*SCN=SN1234567*STD=123
    1*TAN=1233*TC2=8840003*TCI=AD:12345678*TFQ=1312*TI2=AD:13213131*TR2=131*TRN=131*UQ2=KG*VAN=1234*VID=ADFFSDAFASDFASDADFSAFSDFSDFSD
    FSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFAS
    DFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*WET=1313*WETE=404*WETQ=Y*WMC=13213213256
    N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)
    VOTI=  2350327.59      T&I=      366.91      DMP=  1234567.89

006 29221900     41        US            1469.64 KG         1694.35        1694.35            0.00        169.49
    (GEN)                                                                    UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*PST=GEN*RNO=001
    N,N-DIETHYLAMINOETHANOL (CAS 100-37-8) VERY LONG DESCRIPTION LINE THAT SHOULD MAKE THE SYSTEM PRINT AT LEAST TWO LINES 1234567890
    1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 12345678
    90 1234567890 1234567890 1234567890 1234567890
    VOTI=     1694.91      T&I=        0.56

007 34021300     53        US            1741.79 KG         2326.86        2326.86            0.00        232.76
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=     2327.63      T&I=        0.77

008 34021300     53        US             870.89 KG         1163.43        1163.43            0.00        116.38
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=     1163.81      T&I=        0.38

009 34021300     53        US             213.18 KG          458.41         458.41            0.00         45.85
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=      458.56      T&I=        0.15

TOTAL NUMBER OF PACKAGES:         0   (ZERO)

                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print Landscape", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestEntryPrintWithAmendmentLodgedPendingPayStatusCMRForPortrait()
		{
			JobDeclaration testDec = SetupCMRDataForTwoPageEntry();
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testDec.JE_MergeBy = "NON";
			testDec.DoMerge();

			testDec.JE_EntrySubmittedDate = new ZDateTime(2005, 3, 3);
			testDec.CustomsEntryHeaders[0].AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayPending;
			testDec.CustomsEntryHeaders[0].CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			testDec.CustomsEntryHeaders[0].CH_EntryStatus = CMRImportEntryAdvice.Clear.Code;

			testDec.DoMerge();// again to recalc with new effective duty date

			EDIMessage message = testDec.CustomsEntryHeaders[0].Messages.AddNew();
			message.EM_SystemCreateTimeUtc = new ZDateTime(2005, 12, 25, 11, 30, 35);

			StmALog addLogForCIAmendment = testDec.Logs.AddNew(Events.ManualMatchDone, "Reply to CI Amendment Received");

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], true);
			ZString expectedResult =
				@"ENTRY PRINT                              X  XXXXXX    ******************************          PAGE 1
Clear - For Payment, refer to            X  X    X    *     AUSTRALIAN CUSTOMS     *
'Entries'(Amended through CI, Details    X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
may not match ICS.)                      X  X    X    ******************************
Last Msg: AMENDMENT(25DEC05 11:30UTC)    X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:

OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF23
                                         BOX NO  :
O/REF   : PRO040263                      A/REF   : B00148206/1                                FOLIO    :

                                         FOB  (1) :       44796.46 = $A      44796.46         LOAD  PT : KANSAS CITY
                                         CIF  (1) :       45177.59 = $A      45177.59         FIRST PT : SYDNEY             03MAR05
                                         GRWT (KG):       17426.00 = KG      17426.00         DSCH  PT : SYDNEY             03MAR05
                                         T & I    :         381.13 = $A        381.13         DEST  PT : SYDNEY

                                                                                              TOTAL CUSTOMS VALUE : $A 1157377.04
ITERMS  : FOB

VALUATION DATE : 02MAR05                 ITOT (1) :  44796.46 = $A      44796.46
                                         OSEA (1) :    350.00 = $A        350.00              FACTOR              : 1.00000000
CRNCYS 1 AUD @ 1.0000                    ONS  (1) :     31.13 = $A         31.13
       2 USD @ 1.39 (02MAR05)                                                                 CALCULATION DATE    : 03MAR05

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

001 34029000     19        US            4623.88 L         18053.13       18053.13            0.00       1805.90
    (US,US,PS)                                                               UT/SG            FREE
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=    18059.07      T&I=        5.94

002 34039990     37        US             204.69 L          1072.15        1072.15           11.15        108.36
    (GEN)                                                                    UT/SG           0.00%+0.05449/L
    ADD INFO: ORG=US*PST=GEN*RNO=001
    LUBRICATING PREPARATIONS (INCLUDING CUTTING-OIL PREPARATIONS, BOLT OR NUT RELEAS
    VOTI=     1083.65      T&I=        0.35

003 34039990     37        US             204.77 L          1065.06        1065.06           11.15        107.65
    (GEN)                                                                    UT/SG           0.00%+0.05449/L
    ADD INFO: ORG=US*PST=GEN*RNO=001
    LUBRICATING PREPARATIONS (INCLUDING CUTTING-OIL PREPARATIONS, BOLT OR NUT RELEAS
    VOTI=     1076.56      T&I=        0.35

004 39072000     37   505  US            6179.74 KG        17384.86       17384.86          521.54       1791.21    TC 9604601
    (GEN)                                                                    UT/SG           3.00%
    ADD INFO: ORG=US*PST=GEN*RNO=001
    OTHER POLYETHERS EXCL POLYETHYLENE OXIDE,PROPYLENE OXIDE BASED POLYOLS
    VOTI=    17912.12      T&I=        5.72

005 29221900     40        US             740.26 KG         1578.21     1114158.79         1234.00          0.00
    (GEN)                                                                    UT/TV+ADJ       0.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ADJ=1546487USD*AMB=COPQTVD*CL2=12345678*DCX=AD*DMP=1234567.89*DRE=1.2455*DSN=123456*DTY=1234*DXP=1234753AUD*DXT=C*ELA=A
    AA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*FOD=051013*GSTE=404*ISC=1234*LCP=1213.00
    *LCTE=404*LCTI=Y*LCTQ=Y*MD2=123456*MLPI=Y*ODF=13213*ORG=US*PRI=TC:12345678*PST=GEN*PUP=Y*QT2=740.26*RNO=001*SCN=SN1234567*STD=123
    1*TAN=1233*TC2=8840003*TCI=AD:12345678*TFQ=1312*TI2=AD:13213131*TR2=131*TRN=131*UQ2=KG*VAN=1234*VID=ADFFSDAFASDFASDADFSAFSDFSDFSD
    FSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFAS
    DFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*WET=1313*WETE=404*WETQ=Y*WMC=13213213256
    N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)
    VOTI=  2350327.59      T&I=      366.91      DMP=  1234567.89

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                               1777.84   *
                                                                                      * DUMPING DUTY                    1234567.89   *
                                                                                      * GST                                4377.60   *
                                                                                      * OTHER CHARGES                        44.10   *
Delivery: ABI GAS & TOOLS                                                             *                                              *
          171 ABBOTSFORD ROAD                                                         * TOTAL AMOUNT PAYABLE ***        1241998.43 ***
          MAYNE QLD AUSTRALIA                                                         ************************************************
                                                                                      * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************

ENTRY PRINT                              X  XXXXXX    ******************************          PAGE 2
Clear - For Payment, refer to            X  X    X    *     AUSTRALIAN CUSTOMS     *
'Entries'(Amended through CI, Details    X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
may not match ICS.)                      X  X    X    ******************************
Last Msg: AMENDMENT(25DEC05 11:30UTC)    X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:

OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF23
                                         BOX NO  :
O/REF   : PRO040263                      A/REF   : B00148206/1                                FOLIO    :

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

006 29221900     41        US            1469.64 KG         1694.35        1694.35            0.00        169.49
    (GEN)                                                                    UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*PST=GEN*RNO=001
    N,N-DIETHYLAMINOETHANOL (CAS 100-37-8) VERY LONG DESCRIPTION LINE THAT SHOULD MAKE THE SYSTEM PRINT AT LEAST TWO LINES 1234567890
    1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 12345678
    90 1234567890 1234567890 1234567890 1234567890
    VOTI=     1694.91      T&I=        0.56

007 34021300     53        US            1741.79 KG         2326.86        2326.86            0.00        232.76
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=     2327.63      T&I=        0.77

008 34021300     53        US             870.89 KG         1163.43        1163.43            0.00        116.38
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=     1163.81      T&I=        0.38

009 34021300     53        US             213.18 KG          458.41         458.41            0.00         45.85
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=      458.56      T&I=        0.15

TOTAL NUMBER OF PACKAGES:         0   (ZERO)

                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print Portrait", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestImportABNAndClientCode()
		{
			Env.Registry.BrokerageID = "00079C";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";

			CusEntryHeader entryHeader = GetCusEntryHeaderForSea();
			entryHeader.Declaration.Importer.CustomsClientID = "41 005 463 383/001";
			entryHeader.Declaration.Importer.LocalBusinessRegNo = "41 005 463 383/001";

			AddContainersAndPacksToDeclaration();
			packageForContainer1.CW_MarksAndNos = "CONTAINER 1 MARKS AND NUMBERS";
			packageForContainer2.CW_MarksAndNos = "CONTAINER 2 MARKS AND NUMBERS";
			packageForContainer2SecondHBL.CW_MarksAndNos = "CONTAINER 3 MARKS AND NUMBERS" + longNoteValue;

			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCLMixedShipper;
			container3.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			packageForContainer1.PackingGroup.CR_CargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			packageForContainer2.PackingGroup.CR_CargoStatus = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			packageForContainer3.PackingGroup.CR_CargoStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;

			testDec.DoMerge();
			entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "B00001000/1";

			AssertEquals("AQIS Processing charge is calculated", 7m, entryHeader.AQISProcessingCharge);
			AssertEquals("AQIS Container Charges", 33.75m, entryHeader.AQISContainerCharges);

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);

			AssertNotNull("EntryPrint Exposed", entryPrint.EntryPrint);
			Assert("EntryPrint Generated", entryPrint.Generated);

			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 1
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : SEA
(41005463383/001) (41005463383/001)                (00079C)
                                                                          (A2M139)            SHIP     : BAI YUN HE
                                         BOX NO  :                                                       (9203473)
O/REF   : OWNERS REF                     A/REF   : B00001000/1                                SHIP VOY : QF1234

                                         FOB  (1) :         997.65 = $A        717.73         LOAD  PT : SINGAPORE
                                         CIF  (1) :         997.65 = $A        717.73         FIRST PT : SYDNEY             01JAN10
                                         GRWT (KG):         123.45 = KG        123.45         DSCH  PT : SYDNEY             01JAN10
                                         T & I    :           0.00 = $A          0.00         DEST  PT : MELBOURNE
ITERMS  : FOB
                                                                                              TOTAL CUSTOMS VALUE : $A 1046.35
VALUATION DATE : 01NOV05                 ITOT (1) :    997.65 = $A        717.73
                                                                                              FACTOR              : 0.71942446
CRNCYS 1 USD @ 1.39 (01NOV05)
       2 AUD @ 1.0000                                                                         CALCULATION DATE    :
       3 ZAR @ 1.79 (01NOV05)

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

001 44109000     25        JP                  5 CU          997.65        1046.35           52.31        109.86
    (GEN)                                                                    UT/UT+ADJ       5.00%
    SUPPLIER: ABA BEUL (AAA3336347E)
    ADD INFO: ADJ=456.78USD*EFD=070604*ORG=JP*PST=GEN*RNO=001
    Description of the goods for line 1
    VOTI=     1098.66      T&I=        0.00

TOTAL NUMBER OF PACKAGES:      1000   (ONE ZERO ZERO ZERO)

CONTAINER NO. / NO. OF PACKAGES / MASTER BILL / HOUSE BILL

(FCL)OCLU10000020 / 100 PCE / AQT1 / AQT1HBL
    PACKAGE MARKS: CONTAINER 1 MARKS AND NUMBERS
(FCX)OCLU10000031 / 200 PCE / AQT1 / AQT1HBL
    PACKAGE MARKS: CONTAINER 2 MARKS AND NUMBERS
(FCX)OCLU10000031 / 300 PCE / AQT1 / AQT2HBL
    PACKAGE MARKS: CONTAINER 3 MARKS AND NUMBERS1234567890123456789012345678901234567890123456789012345678901234567890123456789012345
    678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234
    567890123456789012345678901234567890
                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                                 52.31   *
                                                                                      * GST                                 109.86   *
                                                                                      * OTHER CHARGES                        91.00   *
Delivery: Name of Importer                                                            *                                              *
          12345678901234567890123456789012345678901234567890                          * TOTAL AMOUNT PAYABLE ***            253.17 ***
          12345678901234567890123456789012345678901234567890                          ************************************************
          1234567890123456789012345 NSW 1234567890 AUSTRALIA                          * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************

ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 2
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : SEA
(41005463383/001) (41005463383/001)                (00079C)
                                                                          (A2M139)            SHIP     : BAI YUN HE
                                         BOX NO  :                                                       (9203473)
O/REF   : OWNERS REF                     A/REF   : B00001000/1                                SHIP VOY : QF1234

CONTAINER NO. / NO. OF PACKAGES / MASTER BILL / HOUSE BILL

(LCL)OCLU10000042 / 400 PCE / AQT1 / AQT1HBL

                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print", expectedResult, entryPrint.EntryPrint);
		}

		public void TestSupplierCIDCode()
		{
			var declaration = GetBasicJobDeclaration();
			var supplier = GetSupplierOrg();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.DoMerge();
			var entryPrint = new AUCustomsEntryPrintForTest(declaration.CustomsEntryHeaders[0], false);
			AssertContains("SUPPLIER: ABA BEUL (AAA3336347E)", entryPrint.EntryPrint);

			var newAddress = Factory.New<OrgAddress>();
			var cusCode1 = supplier.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia);
			cusCode1.OK_OA_PremisesAddress = newAddress.PK;
			var cusCode2 = supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "123456", Core.Constants.CountryCodes.Australia);
			cusCode2.OK_OA_PremisesAddress = supplier.MainAddress.PK;
			entryPrint = new AUCustomsEntryPrintForTest(declaration.CustomsEntryHeaders[0], false);
			AssertNotContains("SUPPLIER: ABA BEUL (AAA3336347E)", entryPrint.EntryPrint);
			AssertContains("SUPPLIER: ABA BEUL (123456)", entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestEntryPrintCMRNature10Sea()
		{
			Env.Registry.BrokerageID = "00079C";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";

			CusEntryHeader entryHeader = GetCusEntryHeaderForSea();

			AddContainersAndPacksToDeclaration();
			packageForContainer1.CW_MarksAndNos = "CONTAINER 1 MARKS AND NUMBERS";
			packageForContainer2.CW_MarksAndNos = "CONTAINER 2 MARKS AND NUMBERS";
			packageForContainer2SecondHBL.CW_MarksAndNos = "CONTAINER 3 MARKS AND NUMBERS" + longNoteValue;
			packageForContainer3.CW_MarksAndNos = "CONTAINER 4 MARKS AND NUMBERS";

			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCLMixedShipper;
			container3.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			InvoiceCharge aDDCharge = invoiceHeader.Charges.AddNew();
			aDDCharge.J7_ChargeType = AUChargeCodeList.Codes.AdditionCharge;
			aDDCharge.J7_Amount = 100.00M;
			aDDCharge.J7_RX_NKCurrency = AUDCurrency.RX_Code;
			aDDCharge.J7_IsIncludedInITOT = false;
			aDDCharge.J7_IsDutiable = true;
			aDDCharge.J7_IsGSTApplicable = true;

			InvoiceCharge dEDCharge = invoiceHeader.Charges.AddNew();
			dEDCharge.J7_ChargeType = AUChargeCodeList.Codes.DeductionCharge;
			dEDCharge.J7_Amount = 25.00M;
			dEDCharge.J7_RX_NKCurrency = AUDCurrency.RX_Code;
			dEDCharge.J7_IsIncludedInITOT = true;
			dEDCharge.J7_IsDutiable = false;
			dEDCharge.J7_IsGSTApplicable = false;

			testDec.DoMerge();
			entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "B00001000/1";

			AssertEquals("AQIS Processing charge is calculated", 7m, entryHeader.AQISProcessingCharge);
			AssertEquals("AQIS Container Charges", 33.75m, entryHeader.AQISContainerCharges);

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);

			AssertNotNull("EntryPrint Exposed", entryPrint.EntryPrint);
			Assert("EntryPrint Generated", entryPrint.Generated);

			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 1
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : SEA
(55006646046) (12345678901/234)                    (00079C)
                                                                          (A2M139)            SHIP     : BAI YUN HE
                                         BOX NO  :                                                       (9203473)
O/REF   : OWNERS REF                     A/REF   : B00001000/1                                SHIP VOY : QF1234

                                         FOB  (1) :        1101.90 = $A        792.73         LOAD  PT : SINGAPORE
                                         CIF  (1) :        1101.90 = $A        792.73         FIRST PT : SYDNEY             01JAN10
                                         GRWT (KG):         123.45 = KG        123.45         DSCH  PT : SYDNEY             01JAN10
                                         T & I    :           0.00 = $A          0.00         DEST  PT : MELBOURNE
ITERMS  : FOB
                                                                                              TOTAL CUSTOMS VALUE : $A 1121.35
VALUATION DATE : 01NOV05                 ITOT (1) :    997.65 = $A        717.73
                                         ADD  (2) :    100.00 = $A        100.00              FACTOR              : 0.79460113
CRNCYS 1 USD @ 1.39 (01NOV05)            DED  (2) :     25.00 = $A         25.00
       2 AUD @ 1.0000                                                                         CALCULATION DATE    :
       3 ZAR @ 1.79 (01NOV05)

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

001 44109000     25        JP                  5 CU          997.65        1121.35           56.06        117.74
    (GEN)                                                                    UT/UT+ADJ       5.00%
    SUPPLIER: ABA BEUL (AAA3336347E)
    ADD INFO: ADJ=456.78USD*EFD=070604*ORG=JP*PST=GEN*RNO=001
    Description of the goods for line 1
    VOTI=     1177.41      T&I=        0.00

TOTAL NUMBER OF PACKAGES:      1000   (ONE ZERO ZERO ZERO)

CONTAINER NO. / NO. OF PACKAGES / MASTER BILL / HOUSE BILL

(FCL)OCLU10000020 / 100 PCE / AQT1 / AQT1HBL
    PACKAGE MARKS: CONTAINER 1 MARKS AND NUMBERS
(FCX)OCLU10000031 / 200 PCE / AQT1 / AQT1HBL
    PACKAGE MARKS: CONTAINER 2 MARKS AND NUMBERS
(FCX)OCLU10000031 / 300 PCE / AQT1 / AQT2HBL
    PACKAGE MARKS: CONTAINER 3 MARKS AND NUMBERS1234567890123456789012345678901234567890123456789012345678901234567890123456789012345
    678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234
    567890123456789012345678901234567890

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                                 56.06   *
                                                                                      * GST                                 117.74   *
                                                                                      * OTHER CHARGES                        91.00   *
Delivery: Name of Importer                                                            *                                              *
          12345678901234567890123456789012345678901234567890                          * TOTAL AMOUNT PAYABLE ***            264.80 ***
          12345678901234567890123456789012345678901234567890                          ************************************************
          1234567890123456789012345 NSW 1234567890 AUSTRALIA                          * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************

ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 2
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : SEA
(55006646046) (12345678901/234)                    (00079C)
                                                                          (A2M139)            SHIP     : BAI YUN HE
                                         BOX NO  :                                                       (9203473)
O/REF   : OWNERS REF                     A/REF   : B00001000/1                                SHIP VOY : QF1234

CONTAINER NO. / NO. OF PACKAGES / MASTER BILL / HOUSE BILL

(LCL)OCLU10000042 / 400 PCE / AQT1 / AQT1HBL
    PACKAGE MARKS: CONTAINER 4 MARKS AND NUMBERS

                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestEntryPrintCMRNature10SeaWithRelatedTransaction()
		{
			Env.Registry.BrokerageID = "00079C";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";
			OrgHeader importer = GetImporterOrg();
			OrgHeader supplier = GetSupplierOrg();

			CusEntryHeader entryHeader = GetCusEntryHeaderForSea();
			AddContainersAndPacksToDeclaration();
			invoiceHeader.AddInfo.ZA_HeaderREL_Hidden = "Y";
			packageForContainer1.CW_MarksAndNos = "CONTAINER 1 MARKS AND NUMBERS";
			packageForContainer2.CW_MarksAndNos = "CONTAINER 2 MARKS AND NUMBERS";
			packageForContainer2SecondHBL.CW_MarksAndNos = "CONTAINER 3 MARKS AND NUMBERS" + longNoteValue;
			packageForContainer3.CW_MarksAndNos = "CONTAINER 4 MARKS AND NUMBERS";

			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCLMixedShipper;
			container3.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			testDec.DoMerge();
			entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "B00001000/1";

			AssertEquals("AQIS Processing charge is calculated", 7m, entryHeader.AQISProcessingCharge);
			AssertEquals("AQIS Container Charges", 33.75m, entryHeader.AQISContainerCharges);

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);

			AssertNotNull("EntryPrint Exposed", entryPrint.EntryPrint);
			Assert("EntryPrint Generated", entryPrint.Generated);

			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 1
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : SEA
(55006646046) (12345678901/234)                    (00079C)
                                                                          (A2M139)            SHIP     : BAI YUN HE
                                         BOX NO  :                                                       (9203473)
O/REF   : OWNERS REF                     A/REF   : B00001000/1                                SHIP VOY : QF1234

                                         FOB  (1) :         997.65 = $A        717.73         LOAD  PT : SINGAPORE
                                         CIF  (1) :         997.65 = $A        717.73         FIRST PT : SYDNEY             01JAN10
                                         GRWT (KG):         123.45 = KG        123.45         DSCH  PT : SYDNEY             01JAN10
                                         T & I    :           0.00 = $A          0.00         DEST  PT : MELBOURNE
ITERMS  : FOB
                                                                                              TOTAL CUSTOMS VALUE : $A 1046.35
VALUATION DATE : 01NOV05                 ITOT (1) :    997.65 = $A        717.73
                                                                                              FACTOR              : 0.71942446
CRNCYS 1 USD @ 1.39 (01NOV05)
       2 AUD @ 1.0000                                                                         CALCULATION DATE    :
       3 ZAR @ 1.79 (01NOV05)

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

001 44109000     25        JP                  5 CU          997.65        1046.35           52.31        109.86
    (GEN)                                                                    RT/UT+ADJ       5.00%
    SUPPLIER: ABA BEUL (AAA3336347E)
    ADD INFO: ADJ=456.78USD*EFD=070604*ORG=JP*PST=GEN*RNO=001
    Description of the goods for line 1
    VOTI=     1098.66      T&I=        0.00

TOTAL NUMBER OF PACKAGES:      1000   (ONE ZERO ZERO ZERO)

CONTAINER NO. / NO. OF PACKAGES / MASTER BILL / HOUSE BILL

(FCL)OCLU10000020 / 100 PCE / AQT1 / AQT1HBL
    PACKAGE MARKS: CONTAINER 1 MARKS AND NUMBERS
(FCX)OCLU10000031 / 200 PCE / AQT1 / AQT1HBL
    PACKAGE MARKS: CONTAINER 2 MARKS AND NUMBERS
(FCX)OCLU10000031 / 300 PCE / AQT1 / AQT2HBL
    PACKAGE MARKS: CONTAINER 3 MARKS AND NUMBERS1234567890123456789012345678901234567890123456789012345678901234567890123456789012345
    678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234
    567890123456789012345678901234567890

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                                 52.31   *
                                                                                      * GST                                 109.86   *
                                                                                      * OTHER CHARGES                        91.00   *
Delivery: Name of Importer                                                            *                                              *
          12345678901234567890123456789012345678901234567890                          * TOTAL AMOUNT PAYABLE ***            253.17 ***
          12345678901234567890123456789012345678901234567890                          ************************************************
          1234567890123456789012345 NSW 1234567890 AUSTRALIA                          * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************

ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 2
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : SEA
(55006646046) (12345678901/234)                    (00079C)
                                                                          (A2M139)            SHIP     : BAI YUN HE
                                         BOX NO  :                                                       (9203473)
O/REF   : OWNERS REF                     A/REF   : B00001000/1                                SHIP VOY : QF1234

CONTAINER NO. / NO. OF PACKAGES / MASTER BILL / HOUSE BILL

(LCL)OCLU10000042 / 400 PCE / AQT1 / AQT1HBL
    PACKAGE MARKS: CONTAINER 4 MARKS AND NUMBERS

                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestEntryPrintCMRNature10Air()
		{
			Env.Registry.BrokerageID = "00079C";
			//Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";
			OrgHeader importer = GetImporterOrg();
			OrgHeader supplier = GetSupplierOrg();

			CusEntryHeader entryHeader = GetCusEntryHeaderForAir();

			SetupPackages();
			testDec.PrimaryHouseBill.CU_fPartShipConsignmentReference = "X342089423";
			testDec.DoMerge();

			entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "B00001000/1";

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);

			AssertNotNull("EntryPrint Exposed", entryPrint.EntryPrint);
			Assert("EntryPrint Generated", entryPrint.Generated);

			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 1
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (12345678901/234)                    (00079C)
                                                                          (A2M139)            AIRCR    : QF1234
                                         BOX NO  :
O/REF   : OWNERS REF                     A/REF   : B00001000/1                                FOLIO    :

                                         FOB  (1) :         997.65 = $A        717.73         LOAD  PT : SINGAPORE
                                         CIF  (1) :         997.65 = $A        717.73         FIRST PT : SYDNEY             01JAN10
                                         GRWT (KG):         123.45 = KG        123.45         DSCH  PT : SYDNEY             01JAN10
                                         T & I    :           0.00 = $A          0.00         DEST  PT : MELBOURNE
ITERMS  : FOB
                                                                                              TOTAL CUSTOMS VALUE : $A 1046.35
VALUATION DATE : 01NOV05                 ITOT (1) :    997.65 = $A        717.73
                                                                                              FACTOR              : 0.71942446
CRNCYS 1 USD @ 1.39 (01NOV05)
       2 AUD @ 1.0000                                                                         CALCULATION DATE    :
       3 ZAR @ 1.79 (01NOV05)

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

001 44109000     25        JP                  5 CU          997.65        1046.35           52.31        109.86
    (GEN)                                                                    UT/UT+ADJ       5.00%
    SUPPLIER: ABA BEUL (AAA3336347E)
    ADD INFO: ADJ=456.78USD*EFD=070604*ORG=JP*PST=GEN*RNO=001
    Description of the goods for line 1
    VOTI=     1098.66      T&I=        0.00

TOTAL NUMBER OF PACKAGES:      1000   (ONE ZERO ZERO ZERO)

NO. OF PACKAGES / MASTER BILL / HOUSE BILL / CONSIGN. REF. NO.

400 PCE / AQT1 / AQT1HBL / X342089423
    PACKAGE MARKS: PACKING LINE 1 MARKS AND NUMBERS1234567890123456789012345678901234567890123456789012345678901234567890123456789012
    345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901
    234567890123456789012345678901234567890
600 PCE / AQT1 / AQT2HBL /
    PACKAGE MARKS: PACKING LINE 2 MARKS AND NUMBERS

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                                 52.31   *
                                                                                      * GST                                 109.86   *
                                                                                      * OTHER CHARGES                        44.85   *
Delivery: Name of Importer                                                            *                                              *
          12345678901234567890123456789012345678901234567890                          * TOTAL AMOUNT PAYABLE ***            207.02 ***
          12345678901234567890123456789012345678901234567890                          ************************************************
          1234567890123456789012345 NSW 1234567890 AUSTRALIA                          * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************
                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestEntryPrintCMRNature10Mail()
		{
			Env.Registry.BrokerageID = "00079C";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";
			OrgHeader importer = GetImporterOrg();
			OrgHeader supplier = GetSupplierOrg();

			CusEntryHeader entryHeader = GetCusEntryHeaderForAir();
			testDec.JE_TransportMode = Core.Constants.TransportModes.Mail;
			SetupPackages();
			testDec.DoMerge();

			entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "B00001000/1";

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);

			AssertNotNull("EntryPrint Exposed", entryPrint.EntryPrint);
			Assert("EntryPrint Generated", entryPrint.Generated);

			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 1
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : POST
(55006646046) (12345678901/234)                    (00079C)
                                                                          (A2M139)            PPC'S    : AQT1HBL,AQT2HBL
                                         BOX NO  :
O/REF   : OWNERS REF                     A/REF   : B00001000/1

                                         FOB  (1) :         997.65 = $A        717.73         LOAD  PT : SINGAPORE
                                         CIF  (1) :         997.65 = $A        717.73         ARRIVAL  : NSW PARCELS POST   01JAN10
                                         GRWT (KG):         123.45 = KG        123.45         DEST  PT : MELBOURNE
                                         T & I    :           0.00 = $A          0.00
ITERMS  : FOB
                                                                                              TOTAL CUSTOMS VALUE : $A 1046.35
VALUATION DATE : 01NOV05                 ITOT (1) :    997.65 = $A        717.73
                                                                                              FACTOR              : 0.71942446
CRNCYS 1 USD @ 1.39 (01NOV05)
       2 AUD @ 1.0000                                                                         CALCULATION DATE    :
       3 ZAR @ 1.79 (01NOV05)

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

001 44109000     25        JP                  5 CU          997.65        1046.35           52.31        109.86
    (GEN)                                                                    UT/UT+ADJ       5.00%
    SUPPLIER: ABA BEUL (AAA3336347E)
    ADD INFO: ADJ=456.78USD*EFD=070604*ORG=JP*PST=GEN*RNO=001
    Description of the goods for line 1
    VOTI=     1098.66      T&I=        0.00

TOTAL NUMBER OF PACKAGES:      1000   (ONE ZERO ZERO ZERO)

NO. OF PACKAGES / PARCEL POST NUMBER

400 PCE / AQT1HBL
    PACKAGE MARKS: PACKING LINE 1 MARKS AND NUMBERS1234567890123456789012345678901234567890123456789012345678901234567890123456789012
    345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901
    234567890123456789012345678901234567890
600 PCE / AQT2HBL
    PACKAGE MARKS: PACKING LINE 2 MARKS AND NUMBERS

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                                 52.31   *
                                                                                      * GST                                 109.86   *
                                                                                      * OTHER CHARGES                        30.85   *
Delivery: Name of Importer                                                            *                                              *
          12345678901234567890123456789012345678901234567890                          * TOTAL AMOUNT PAYABLE ***            193.02 ***
          12345678901234567890123456789012345678901234567890                          ************************************************
          1234567890123456789012345 NSW 1234567890 AUSTRALIA                          * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************
                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestEntryPrintCMRNature20()
		{
			Env.Registry.BrokerageID = "00079C";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";
			OrgHeader importer = GetImporterOrg();
			OrgHeader supplier = GetSupplierOrg();
			OrgHeader warehouseOrg = GetWarehouseOrg();

			CusEntryHeader entryHeader = GetCusEntryHeaderForAir();
			InvoiceCharge oFTCharge = invoiceHeader.Charges.AddNew();
			oFTCharge.J7_ChargeType = AUChargeCodeList.Codes.OverseasFreight;
			oFTCharge.J7_Amount = 100.00M;
			oFTCharge.J7_RX_NKCurrency = AUDCurrency.RX_Code;
			oFTCharge.J7_IsIncludedInITOT = false;

			InvoiceCharge oNSCharge = invoiceHeader.Charges.AddNew();
			oNSCharge.J7_ChargeType = AUChargeCodeList.Codes.OverseasInsurance;
			oNSCharge.J7_Amount = 50.00M;
			oNSCharge.J7_RX_NKCurrency = AUDCurrency.RX_Code;
			oNSCharge.J7_IsIncludedInITOT = false;

			invoiceLine.JI_IsPackToBondForLine = true;
			invoiceHeader.AddInfo.ZA_POC = "US";
			invoiceHeader.AddInfo.ZA_PST = "US";
			invoiceHeader.AddInfo.ZA_PRT = "PS";

			packageForContainer1 = testDec.Packages.AddNew();
			packageForContainer1.CW_HouseBill = testDec.PrimaryMasterBill.CU_BillUniqueCode;
			packageForContainer1.CW_PackQty = 900;
			packageForContainer1.CW_InBondPackQty = 900;
			packageForContainer1.CW_OuterPacks = 1;
			packageForContainer1.PackingGroup.CR_HouseContainerNumber = 1;
			packageForContainer1.CW_MarksAndNos = "PACKING LINE 1 MARKS AND NUMBERS";

			packageForContainer2 = testDec.Packages.AddNew();
			packageForContainer2.CW_HouseBill = testDec.PrimaryMasterBill.CU_BillUniqueCode;
			packageForContainer2.CW_PackQty = 600;
			packageForContainer2.CW_InBondPackQty = 600;
			packageForContainer2.CW_OuterPacks = 2;
			packageForContainer2.PackingGroup.CR_HouseContainerNumber = 2;
			packageForContainer2.CW_MarksAndNos = "PACKING LINE 2 MARKS AND NUMBERS";

			testDec.DoMerge();
			entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "B00001000/1";

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);

			AssertNotNull("EntryPrint Exposed", entryPrint.EntryPrint);
			Assert("EntryPrint Generated", entryPrint.Generated);

			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   XXXXX  XXXXXX    *************************           PAGE 1
Not Sent                                     X  X    X    *   AUSTRALIAN CUSTOMS  *
Last Msg: NONE                           XXXXX  X    X    * ENTRY FOR WAREHOUSING *           ENTRY NO.   PRINT
                                         X      X    X    *************************
                                         XXXXX  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (12345678901/234)                    (00079C)
                                                                          (A2M139)            AIRCR    : QF1234
                                         BOX NO  :
O/REF   : OWNERS REF                     A/REF   : B00001000/1                                FOLIO    :

WAREHOUSE: PJG WAREHOUSING & DISTRIBUTIONFOB  (1) :         997.65 = $A        717.73         LOAD  PT : SINGAPORE
           (WAREHOUSEID)                 CIF  (2) :         867.73 = $A        867.73         FIRST PT : SYDNEY             01JAN10
                                         GRWT (KG):         123.45 = KG        123.45         DSCH  PT : SYDNEY             01JAN10
                                         T & I    :         150.00 = $A        150.00         DEST  PT : MELBOURNE
ITERMS  : FOB
                                                                                              TOTAL CUSTOMS VALUE : $A 1046.35
VALUATION DATE : 01NOV05                 ITOT (1) :    997.65 = $A        717.73
                                         OSEA (2) :    100.00 = $A        100.00              FACTOR              : 0.71942446
CRNCYS 1 USD @ 1.39 (01NOV05)            ONS  (2) :     50.00 = $A         50.00
       2 AUD @ 1.0000                                                                         CALCULATION DATE    :
       3 ZAR @ 1.79 (01NOV05)

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                  WAREHOUSE UV

001 44109000     25        JP                  5 CU          997.65        1046.35            0.00          0.00
    (GEN)                                                                    UT/UT+ADJ        FREE                      209.2700
    SUPPLIER: ABA BEUL (AAA3336347E)
    ADD INFO: ADJ=456.78USD*EFD=070604*ORG=JP*PST=GEN*RNO=001
    Description of the goods for line 1
    VOTI=     1196.35      T&I=      150.00      T&I UV=       30.00

TOTAL NUMBER OF WAREHOUSE PACKAGES:      1500   (ONE FIVE ZERO ZERO)

NO. OF PACKAGES / MASTER BILL / HOUSE BILL

1500 PCE / AQT1 /
    PACKAGE MARKS: PACKING LINE 1 MARKS AND NUMBERS PACKING LINE 2 MARKS AND NUMBERS

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * GST                                   0.00   *
                                                                                      * OTHER CHARGES                        44.10   *
Delivery: Name of Importer                                                            *                                              *
          12345678901234567890123456789012345678901234567890                          * TOTAL AMOUNT PAYABLE ***             44.10 ***
          12345678901234567890123456789012345678901234567890                          ************************************************
          1234567890123456789012345 NSW 1234567890 AUSTRALIA                          * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************
                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestEntryPrintCMRNature1020()
		{
			Env.Registry.BrokerageID = "00079C";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";
			OrgHeader importer = GetImporterOrg();
			OrgHeader supplier = GetSupplierOrg();
			OrgHeader warehouseOrg = GetWarehouseOrg();

			CusEntryHeader entryHeader = GetCusEntryHeaderForAir();
			InvoiceCharge oFTCharge = invoiceHeader.Charges.AddNew();
			oFTCharge.J7_ChargeType = AUChargeCodeList.Codes.OverseasFreight;
			oFTCharge.J7_Amount = 100.00M;
			oFTCharge.J7_RX_NKCurrency = AUDCurrency.RX_Code;
			oFTCharge.J7_IsIncludedInITOT = false;

			InvoiceCharge oNSCharge = invoiceHeader.Charges.AddNew();
			oNSCharge.J7_ChargeType = AUChargeCodeList.Codes.OverseasInsurance;
			oNSCharge.J7_Amount = 50.00M;
			oNSCharge.J7_RX_NKCurrency = AUDCurrency.RX_Code;
			oNSCharge.J7_IsIncludedInITOT = false;

			invoiceLine.JI_IsPackToBondForLine = true;
			invoiceLine.AddInfo.ZA_WRQ = 3m;
			invoiceLine.AddInfo.ZA_WRU = "NO";
			invoiceHeader.AddInfo.ZA_POC = "US";
			invoiceHeader.AddInfo.ZA_PST = "US";
			invoiceHeader.AddInfo.ZA_PRT = "PS";

			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			PopulateWithValidTestData(invoiceLine2);
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_LinePrice = 850.65M;
			invoiceLine2.AddInfo.ZA_VALB_Hidden = "TV";
			invoiceLine2.JI_CustomsQuantity = 654321.12345m;

			packageForContainer1 = testDec.Packages.AddNew();
			packageForContainer1.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			packageForContainer1.CW_PackQty = 400;
			packageForContainer1.CW_InBondPackQty = 100;
			packageForContainer1.CW_OuterPacks = 1;
			packageForContainer1.PackingGroup.CR_HouseContainerNumber = 1;
			packageForContainer1.CW_MarksAndNos = "PACKING LINE 1 MARKS AND NUMBERS";

			packageForContainer2 = testDec.Packages.AddNew();
			packageForContainer2.CW_HouseBill = houseBill2.CU_BillUniqueCode;
			packageForContainer2.CW_PackQty = 600;
			packageForContainer2.CW_InBondPackQty = 400;
			packageForContainer2.CW_OuterPacks = 2;
			packageForContainer2.PackingGroup.CR_HouseContainerNumber = 2;
			packageForContainer2.CW_MarksAndNos = "PACKING LINE 2 MARKS AND NUMBERS";

			testDec.DoMerge();
			entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "B00001000/1";
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayPending;

			CusEntryLine entryLine2 = invoiceLine2.CusEntryLine;
			entryLine2.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATDeferred, entryLine2.GSTVATAmount);
			entryLine2.Fees.Remove(entryLine2.Fees.GetElementWithThisCode(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount));

			EDIMessage message = entryHeader.Messages.AddNew();
			message.EM_SystemCreateTimeUtc = new ZDateTime(2005, 12, 25, 11, 30, 35);
			StmALog addLogForImpedement = entryHeader.Logs.AddNew(Events.CustomsImpedimentReceived, "quarantine impedement");

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);

			AssertNotNull("EntryPrint Exposed", entryPrint.EntryPrint);
			Assert("EntryPrint Generated", entryPrint.Generated);

			ZString expectedResult =
				@"ENTRY PRINT                    X  XXXXXX    XXXXX  XXXXXX ******************************      PAGE 1
Clear - For Payment, refer to  X  X    X        X  X    X *     AUSTRALIAN CUSTOMS     *
'Entries':                     X  X    X  / XXXXX  X    X * ENTRY FOR HOME CONSUMPTION/*      ENTRY NO.   PRINT
Impediment(s):quarantine       X  X    X    X      X    X *    ENTRY FOR WAREHOUSING   *
impedement                     X  XXXXXX    XXXXX  XXXXXX ******************************
Last Msg: LODGE(25DEC05                                 PREPARED 17FEB05 17:47 HRS
11:30UTC)                                                                                                COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (12345678901/234)                    (00079C)
                                                                          (A2M139)            AIRCR    : QF1234
                                         BOX NO  :
O/REF   : OWNERS REF                     A/REF   : B00001000/1                                FOLIO    :

WAREHOUSE: PJG WAREHOUSING & DISTRIBUTIONFOB  (1) :        1848.30 = $A       1329.71         LOAD  PT : SINGAPORE
           (WAREHOUSEID)                 CIF  (2) :        1479.71 = $A       1479.71         FIRST PT : SYDNEY             01JAN10
                                         GRWT (KG):         123.45 = KG        123.45         DSCH  PT : SYDNEY             01JAN10
                                         T & I    :         150.00 = $A        150.00         DEST  PT : MELBOURNE
ITERMS  : FOB
                                                                                              TOTAL CUSTOMS VALUE : $A 1986.95
VALUATION DATE : 01NOV05                 ITOT (1) :   1848.30 = $A       1329.71
                                         OSEA (2) :    100.00 = $A        100.00              FACTOR              : 0.71942446
CRNCYS 1 USD @ 1.39 (01NOV05)            ONS  (2) :     50.00 = $A         50.00
       2 AUD @ 1.0000                                                                         CALCULATION DATE    :
       3 ZAR @ 1.79 (01NOV05)

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                  WAREHOUSE UV

001 44109000     25        JP                  5 CU          997.65        1046.35            0.00          0.00
    (GEN)                                                                    UT/UT+ADJ        FREE                      348.7833
    SUPPLIER: ABA BEUL (AAA3336347E)
    ADD INFO: ADJ=456.78USD*EFD=070604*ORG=JP*PST=GEN*RNO=001*WRQ=3*WRU=NO
    Description of the goods for line 1
    VOTI=     1125.34      T&I=       78.99      T&I UV=       26.33

002 44109000     25        JP         654321.123 CU          850.65         940.60           47.03        105.86
    (GEN)                                                                    UT/TV+ADJ       5.00%
    SUPPLIER: ABA BEUL (AAA3336347E)
    ADD INFO: ADJ=456.78USD*EFD=070604*ORG=JP*PST=GEN*RNO=001
    Description of the goods for line 1
    VOTI=     1058.64      T&I=       71.01

TOTAL NUMBER OF PACKAGES:      1000   (ONE ZERO ZERO ZERO)
TOTAL NUMBER OF WAREHOUSE PACKAGES:       500   (FIVE ZERO ZERO)

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                                 47.03   *
TOTAL DEFERRED GST  FOR ENTRY  =   $105.86                                            * GST                                   0.00   *
                                                                                      * OTHER CHARGES                     98193.01   *
Delivery: Name of Importer                                                            *                                              *
          12345678901234567890123456789012345678901234567890                          * TOTAL AMOUNT PAYABLE ***          98345.90 ***
          12345678901234567890123456789012345678901234567890                          ************************************************
          1234567890123456789012345 NSW 1234567890 AUSTRALIA                          * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************

ENTRY PRINT                    X  XXXXXX    XXXXX  XXXXXX ******************************      PAGE 2
Clear - For Payment, refer to  X  X    X        X  X    X *     AUSTRALIAN CUSTOMS     *
'Entries':                     X  X    X  / XXXXX  X    X * ENTRY FOR HOME CONSUMPTION/*      ENTRY NO.   PRINT
Impediment(s):quarantine       X  X    X    X      X    X *    ENTRY FOR WAREHOUSING   *
impedement                     X  XXXXXX    XXXXX  XXXXXX ******************************
Last Msg: LODGE(25DEC05                                 PREPARED 17FEB05 17:47 HRS
11:30UTC)                                                                                                COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (12345678901/234)                    (00079C)
                                                                          (A2M139)            AIRCR    : QF1234
                                         BOX NO  :
O/REF   : OWNERS REF                     A/REF   : B00001000/1                                FOLIO    :

NO. OF PACKAGES / MASTER BILL / HOUSE BILL

400 PCE / AQT1 / AQT1HBL
    PACKAGE MARKS: PACKING LINE 1 MARKS AND NUMBERS
600 PCE / AQT1 / AQT2HBL
    PACKAGE MARKS: PACKING LINE 2 MARKS AND NUMBERS

                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestEntryPrintCMRNature30()
		{
			Env.Registry.BrokerageID = "00079C";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";
			OrgHeader importer = GetImporterOrg();
			OrgHeader supplier = GetSupplierOrg();
			OrgHeader warehouseOrg = GetWarehouseOrg();

			CusEntryHeader entryHeader = GetCusEntryHeaderForSea();
			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			invoiceHeader.AddInfo.ZA_POC = "US";
			invoiceHeader.AddInfo.ZA_PST = "US";
			invoiceHeader.AddInfo.ZA_PRT = "PS";
			testDec.JE_TotalNoOfPacks = 252;
			testDec.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Piece;
			testDec.JE_MarksAndNumbers = "Marks and numbers for this job \r\n more mareks and number lines";
			invoiceLine.JI_Weight = 123.45m;
			invoiceLine.JI_WeightUQ = "KG";
			CusContainer container1 = testDec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OCLU10000001";
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			CusContainer container2 = testDec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OCLU10000002";
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Bulk;

			CusContainer container3 = testDec.CusContainers.AddNew();
			container3.CO_ContainerNumber = "OCLU10000003";
			container3.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BreakBulk;

			CusContainer container4 = testDec.CusContainers.AddNew();
			container4.CO_ContainerNumber = "OCLU10000004";
			container4.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BuyersConsol;

			CusContainer container5 = testDec.CusContainers.AddNew();
			container5.CO_ContainerNumber = "OCLU10000005";
			container5.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Combination;

			CusContainer container6 = testDec.CusContainers.AddNew();
			container6.CO_ContainerNumber = "OCLU10000006";
			container6.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			CusContainer container7 = testDec.CusContainers.AddNew();
			container7.CO_ContainerNumber = "OCLU10000007";
			container7.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			testDec.DoMerge();
			entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "B00001000/1";

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);

			AssertNotNull("EntryPrint Exposed", entryPrint.EntryPrint);
			Assert("EntryPrint Generated", entryPrint.Generated);

			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   XXXXX  XXXXXX    ************************            PAGE 1
Not Sent                                     X  X    X    *  AUSTRALIAN CUSTOMS  *
Last Msg: NONE                           XXXXX  X    X    * EX-WAREHOUSING ENTRY *            ENTRY NO.   PRINT
                                             X  X    X    ************************
                                         XXXXX  XXXXXX                                        DESTINATION PORT: MELBOURNE
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : SEA
(55006646046) (12345678901/234)                    (00079C)
                                                                          (A2M139)            SHIP     : BAI YUN HE
                                         BOX NO  :                                                       (9203473)
O/REF   : OWNERS REF                     A/REF   : B00001000/1                                SHIP VOY : QF1234

WAREHOUSE: PJG WAREHOUSING & DISTRIBUTION
           (WAREHOUSEID)

                                         T & I    :           0.00 = $A          0.00

                                                                                              TOTAL CUSTOMS VALUE : $A 1326.27
VALUATION DATE : 01NOV05                 ITOT (1) :    997.65 = $A        997.65
                                                                                              FACTOR              : 1.00000000
CRNCYS 1 AUD @ 1.0000
       2 ZAR @ 1.79 (01NOV05)                                                                 CALCULATION DATE    :
       3 USD @ 1.39 (01NOV05)

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                  WAREHOUSE UV

001 44109000     25        JP                  5 CU          997.65        1326.27           66.31        139.25
    (GEN)                                                                    UT/UT+ADJ       5.00%                      265.2540
    ADD INFO: ADJ=456.78USD*EFD=070604*ORG=JP*PST=GEN*RNO=001
    Description of the goods for line 1
    VOTI=     1392.58      T&I=        0.00

TOTAL NUMBER OF PACKAGES:       252   (TWO FIVE TWO)

AQT1,AQT1/M   AQT1HBL,AQT2HBL/H

CONTAINER NOS:   (FCL)OCLU10000001  (BLK)OCLU10000002  (BBK)OCLU10000003  (BCN)OCLU10000004  (COM)OCLU10000005  (FCL)OCLU10000006
                 (LCL)OCLU10000007

MARKS AND NOS: Marks and numbers for this job  more mareks and number lines

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                                 66.31   *
                                                                                      * GST                                 139.25   *
                                                                                      * OTHER CHARGES                        23.95   *
Delivery: Name of Importer                                                            *                                              *
          12345678901234567890123456789012345678901234567890                          * TOTAL AMOUNT PAYABLE ***            229.51 ***
          12345678901234567890123456789012345678901234567890                          ************************************************
          1234567890123456789012345 NSW 1234567890 AUSTRALIA                          * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************
                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestEntryPrintCMRNature30WithPacksOnEntryHeader()
		{
			Env.Registry.BrokerageID = "00079C";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";
			OrgHeader importer = GetImporterOrg();
			OrgHeader supplier = GetSupplierOrg();
			OrgHeader warehouseOrg = GetWarehouseOrg();

			CusEntryHeader entryHeader = GetCusEntryHeaderForSea();
			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			invoiceHeader.AddInfo.ZA_POC = "US";
			invoiceHeader.AddInfo.ZA_PST = "US";
			invoiceHeader.AddInfo.ZA_PRT = "PS";
			testDec.JE_TotalNoOfPacks = 252;
			testDec.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Piece;
			testDec.JE_MarksAndNumbers = "Marks and numbers for this job \r\n more mareks and number lines";
			invoiceLine.JI_Weight = 123.45m;
			invoiceLine.JI_WeightUQ = "KG";

			CusContainer container1 = testDec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OCLU10000001";
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			CusContainer container2 = testDec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OCLU10000002";
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Bulk;

			CusContainer container3 = testDec.CusContainers.AddNew();
			container3.CO_ContainerNumber = "OCLU10000003";
			container3.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BreakBulk;

			CusContainer container4 = testDec.CusContainers.AddNew();
			container4.CO_ContainerNumber = "OCLU10000004";
			container4.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BuyersConsol;

			CusContainer container5 = testDec.CusContainers.AddNew();
			container5.CO_ContainerNumber = "OCLU10000005";
			container5.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Combination;

			CusContainer container6 = testDec.CusContainers.AddNew();
			container6.CO_ContainerNumber = "OCLU10000006";
			container6.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			CusContainer container7 = testDec.CusContainers.AddNew();
			container7.CO_ContainerNumber = "OCLU10000007";
			container7.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			testDec.DoMerge();
			entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "B00001000/1";
			entryHeader.WarehouseNumberOfPacks = 152;

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);

			AssertNotNull("EntryPrint Exposed", entryPrint.EntryPrint);
			Assert("EntryPrint Generated", entryPrint.Generated);

			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   XXXXX  XXXXXX    ************************            PAGE 1
Not Sent                                     X  X    X    *  AUSTRALIAN CUSTOMS  *
Last Msg: NONE                           XXXXX  X    X    * EX-WAREHOUSING ENTRY *            ENTRY NO.   PRINT
                                             X  X    X    ************************
                                         XXXXX  XXXXXX                                        DESTINATION PORT: MELBOURNE
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : SEA
(55006646046) (12345678901/234)                    (00079C)
                                                                          (A2M139)            SHIP     : BAI YUN HE
                                         BOX NO  :                                                       (9203473)
O/REF   : OWNERS REF                     A/REF   : B00001000/1                                SHIP VOY : QF1234

WAREHOUSE: PJG WAREHOUSING & DISTRIBUTION
           (WAREHOUSEID)

                                         T & I    :           0.00 = $A          0.00

                                                                                              TOTAL CUSTOMS VALUE : $A 1326.27
VALUATION DATE : 01NOV05                 ITOT (1) :    997.65 = $A        997.65
                                                                                              FACTOR              : 1.00000000
CRNCYS 1 AUD @ 1.0000
       2 ZAR @ 1.79 (01NOV05)                                                                 CALCULATION DATE    :
       3 USD @ 1.39 (01NOV05)

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                  WAREHOUSE UV

001 44109000     25        JP                  5 CU          997.65        1326.27           66.31        139.25
    (GEN)                                                                    UT/UT+ADJ       5.00%                      265.2540
    ADD INFO: ADJ=456.78USD*EFD=070604*ORG=JP*PST=GEN*RNO=001
    Description of the goods for line 1
    VOTI=     1392.58      T&I=        0.00

TOTAL NUMBER OF PACKAGES:       152   (ONE FIVE TWO)

AQT1,AQT1/M   AQT1HBL,AQT2HBL/H

CONTAINER NOS:   (FCL)OCLU10000001  (BLK)OCLU10000002  (BBK)OCLU10000003  (BCN)OCLU10000004  (COM)OCLU10000005  (FCL)OCLU10000006
                 (LCL)OCLU10000007

MARKS AND NOS: Marks and numbers for this job  more mareks and number lines

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                                 66.31   *
                                                                                      * GST                                 139.25   *
                                                                                      * OTHER CHARGES                        23.95   *
Delivery: Name of Importer                                                            *                                              *
          12345678901234567890123456789012345678901234567890                          * TOTAL AMOUNT PAYABLE ***            229.51 ***
          12345678901234567890123456789012345678901234567890                          ************************************************
          1234567890123456789012345 NSW 1234567890 AUSTRALIA                          * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************
                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestEntryPrintCMRNature30WarehouseOnLines()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			Env.Registry.BrokerageID = "00079C";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";
			OrgHeader importer = GetImporterOrg();
			OrgHeader supplier = GetSupplierOrg();
			OrgHeader warehouseOrg = GetWarehouseOrg();

			CusEntryHeader entryHeader = GetCusEntryHeaderForSea();
			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			OrgAddress otherWarehouseAddress = OrgHeader.New(Factory).MainAddress;
			otherWarehouseAddress.Header.OH_FullName = "AnotherWarehouse";
			otherWarehouseAddress.LocalControlledPremisesID = "123CCP";
			invoiceHeader.JobComInvoiceLines[0].AddInfo.ZA_OA_WarehouseAddress_Hidden = otherWarehouseAddress.PK;
			invoiceHeader.AddInfo.ZA_POC = "US";
			invoiceHeader.AddInfo.ZA_PST = "US";
			invoiceHeader.AddInfo.ZA_PRT = "PS";
			invoiceLine.JI_Weight = 123.45m;
			invoiceLine.JI_WeightUQ = "KG";
			testDec.JE_TotalNoOfPacks = 252;
			testDec.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Piece;
			testDec.JE_MarksAndNumbers = "Marks and numbers for this job \r\n more mareks and number lines";

			CusContainer container1 = testDec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OCLU10000001";
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			CusContainer container2 = testDec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OCLU10000002";
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Bulk;

			CusContainer container3 = testDec.CusContainers.AddNew();
			container3.CO_ContainerNumber = "OCLU10000003";
			container3.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BreakBulk;

			CusContainer container4 = testDec.CusContainers.AddNew();
			container4.CO_ContainerNumber = "OCLU10000004";
			container4.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BuyersConsol;

			CusContainer container5 = testDec.CusContainers.AddNew();
			container5.CO_ContainerNumber = "OCLU10000005";
			container5.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Combination;

			CusContainer container6 = testDec.CusContainers.AddNew();
			container6.CO_ContainerNumber = "OCLU10000006";
			container6.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			CusContainer container7 = testDec.CusContainers.AddNew();
			container7.CO_ContainerNumber = "OCLU10000007";
			container7.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			testDec.DoMerge();
			entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "B00001000/1";
			invoiceLine.CusEntryLine.CL_FlatAmountUQ = "NO";
			invoiceLine.CusEntryLine.CL_FlatAmount = 12000.00m;
			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);

			AssertNotNull("EntryPrint Exposed", entryPrint.EntryPrint);
			Assert("EntryPrint Generated", entryPrint.Generated);

			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   XXXXX  XXXXXX    ************************            PAGE 1
Not Sent                                     X  X    X    *  AUSTRALIAN CUSTOMS  *
Last Msg: NONE                           XXXXX  X    X    * EX-WAREHOUSING ENTRY *            ENTRY NO.   PRINT
                                             X  X    X    ************************
                                         XXXXX  XXXXXX                                        DESTINATION PORT: MELBOURNE
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : SEA
(55006646046) (12345678901/234)                    (00079C)
                                                                          (A2M139)            SHIP     : BAI YUN HE
                                         BOX NO  :                                                       (9203473)
O/REF   : OWNERS REF                     A/REF   : B00001000/1                                SHIP VOY : QF1234

WAREHOUSE: ANOTHERWAREHOUSE
           (123CCP)

                                         T & I    :           0.00 = $A          0.00

                                                                                              TOTAL CUSTOMS VALUE : $A 1326.27
VALUATION DATE : 01NOV05                 ITOT (1) :    997.65 = $A        997.65
                                                                                              FACTOR              : 1.00000000
CRNCYS 1 AUD @ 1.0000
       2 ZAR @ 1.79 (01NOV05)                                                                 CALCULATION DATE    :
       3 USD @ 1.39 (01NOV05)

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                  WAREHOUSE UV

001 44109000     25        JP                  5 CU          997.65        1326.27           66.31        139.25
    (GEN)                                                                    UT/UT+ADJ       5.00%+12000.00000/NO       265.2540
    ADD INFO: ADJ=456.78USD*EFD=070604*ORG=JP*PST=GEN*RNO=001
    Description of the goods for line 1
    VOTI=     1392.58      T&I=        0.00

TOTAL NUMBER OF PACKAGES:       252   (TWO FIVE TWO)

AQT1,AQT1/M   AQT1HBL,AQT2HBL/H

CONTAINER NOS:   (FCL)OCLU10000001  (BLK)OCLU10000002  (BBK)OCLU10000003  (BCN)OCLU10000004  (COM)OCLU10000005  (FCL)OCLU10000006
                 (LCL)OCLU10000007

MARKS AND NOS: Marks and numbers for this job  more mareks and number lines

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                                 66.31   *
                                                                                      * GST                                 139.25   *
                                                                                      * OTHER CHARGES                        23.95   *
Delivery: Name of Importer                                                            *                                              *
          12345678901234567890123456789012345678901234567890                          * TOTAL AMOUNT PAYABLE ***            229.51 ***
          12345678901234567890123456789012345678901234567890                          ************************************************
          1234567890123456789012345 NSW 1234567890 AUSTRALIA                          * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************
                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 3, 3)]
		public void TestTwoPageEntriesForCMR_LandscapeWithMultipleCurrencies()
		{
			JobDeclaration testDec = SetupCMRDataForTwoPageEntryWithMultipleCurrencies();

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testDec.JE_MergeBy = "NON";
			testDec.DoMerge();
			testDec.Consignee.MiscServ.OM_IMIsGSTDeferred = ZBool.True;
			CusEntryLine mergedLine1 = testDec.CustomsEntryHeaders[0].MergedLines[0];
			mergedLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.SecurityConcession, 1234.51m);
			mergedLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.WetAmount, 1234.52m);
			mergedLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.LCTAmount, 1234.53m);
			mergedLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.CountervailingDuty, 1234.54m);
			mergedLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DumpingDuty, 1234.55m);
			testDec.CustomsEntryHeaders[0].AQISServicePaymentAmount = 33m;
			testDec.CustomsEntryHeaders[0].CH_TotalPaid += 33m;

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);
			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 1
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF23
                                         BOX NO  :
O/REF   : PRO040263                      A/REF   : B00148206/1                                FOLIO    :

                                         FOB  (1) :       67826.42 = $A      67826.42         LOAD  PT : KANSAS CITY
                                         CIF  (1) :       68207.55 = $A      68207.55         FIRST PT : SYDNEY             03MAR05
                                         GRWT (KG):       17426.00 = KG      17426.00         DSCH  PT : SYDNEY             03MAR05
                                         T & I    :         381.13 = $A        381.13         DEST  PT : SYDNEY
ITERMS  : FOB
                                                                                              TOTAL CUSTOMS VALUE : $A 1180407.00
VALUATION DATE : 02MAR05                 ITOT (1) :  67826.42 = $A      67826.42
                                         OSEA (1) :    350.00 = $A        350.00              FACTOR              : 1.00000000
CRNCYS 1 AUD @ 1.0000                    ONS  (1) :     31.13 = $A         31.13
       2 HKD @ 0.62 (02MAR05)                                                                 CALCULATION DATE    : 01APR05
       3 USD @ 1.39 (02MAR05)

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

001 34029000     19        US            4623.88 L         29117.95       29117.95            0.00       2912.73
    (US,US,PS)                                                               UT/SG            FREE
    ADD INFO: DMP=1234.55*ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=    31596.44      T&I=        9.40      WET=     1234.52      LCT=     1234.53      CVD=     1234.54      DMP=     1234.55
    Security=     1234.51

002 34039990     37        US             204.69 L          1729.27        1729.27           11.15        174.09
    (GEN)                                                                    UT/SG           0.00%+0.05449/L
    ADD INFO: ORG=US*PST=GEN*RNO=001
    LUBRICATING PREPARATIONS (INCLUDING CUTTING-OIL PREPARATIONS, BOLT OR NUT RELEAS
    VOTI=     1740.98      T&I=        0.56

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                               2097.50   *
                                                                                      * COUNTERVAILING DUTY                1234.54   *
                                                                                      * DUMPING DUTY                    1235802.44   *
TOTAL DEFERRED GST  FOR ENTRY  =   $6713.28                                           * GST                                   0.00   *
                                                                                      * WET                                1234.52   *
                                                                                      * LCT                                1234.53   *
                                                                                      * QUARANTINE SERVICES FEE              33.00   *
                                                                                      * OTHER CHARGES                        44.10   *
Delivery: ABI GAS & TOOLS                                                             *                                              *
          171 ABBOTSFORD ROAD                                                         * TOTAL AMOUNT PAYABLE ***        1244686.77 ***
          MAYNE QLD AUSTRALIA                                                         ************************************************
                                                                                      * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************

ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 2
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF23
                                         BOX NO  :
O/REF   : PRO040263                      A/REF   : B00148206/1                                FOLIO    :

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

003 34039990     37        US             204.77 L          1717.84        1717.84           11.15        172.95
    (GEN)                                                                    UT/SG           0.00%+0.05449/L
    ADD INFO: ORG=US*PST=GEN*RNO=001
    LUBRICATING PREPARATIONS (INCLUDING CUTTING-OIL PREPARATIONS, BOLT OR NUT RELEAS
    VOTI=     1729.54      T&I=        0.55

004 39072000     37   505  US            6179.74 KG        28040.10       28040.10          841.20       2889.03    TC 9604601
    (GEN)                                                                    UT/SG           3.00%
    ADD INFO: ORG=US*PST=GEN*RNO=001
    OTHER POLYETHERS EXCL POLYETHYLENE OXIDE,PROPYLENE OXIDE BASED POLYOLS
    VOTI=    28890.35      T&I=        9.05

005 29221900     40        US             740.26 KG         1578.21     1114158.79         1234.00          0.00
    (GEN)          Quarantine Permits: permit1,permit2                       UT/TV+ADJ       0.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ADJ=1546487USD*AMB=COPQTVD*CL2=12345678*DCX=AD*DMP=1234567.89*DRE=1.2455*DSN=123456*DTY=1234*DXP=1234753AUD*DXT=C*ELA=A
    AA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*FOD=051013*GSTE=404*ISC=1234*LCP=1213.00
    *LCTE=404*LCTI=Y*LCTQ=Y*MD2=123456*MLPI=Y*ODF=13213*ORG=US*PRI=TC:12345678*PST=GEN*PUP=Y*QT2=740.26*RNO=001*SCN=SN1234567*STD=123
    1*TAN=1233*TC2=8840003*TCI=AD:12345678*TFQ=1312*TI2=AD:13213131*TR2=131*TRN=131*UQ2=KG*VAN=1234*VID=ADFFSDAFASDFASDADFSAFSDFSDFSD
    FSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFAS
    DFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*WET=1313*WETE=404*WETQ=Y*WMC=13213213256
    N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)
    VOTI=  2350320.42      T&I=      359.74      DMP=  1234567.89

006 29221900     41        US            1469.64 KG         1694.35        1694.35            0.00        169.49
    (GEN)                                                                    UT/TV            FREE
    Quarantine Permits: 1111111111,2222222222,3333333333,444444444
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*PST=GEN*RNO=001
    N,N-DIETHYLAMINOETHANOL (CAS 100-37-8) VERY LONG DESCRIPTION LINE THAT SHOULD MAKE THE SYSTEM PRINT AT LEAST TWO LINES 1234567890
    1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 12345678
    90 1234567890 1234567890 1234567890 1234567890
    VOTI=     1694.90      T&I=        0.55

007 34021300     53        US            1741.79 KG         2326.86        2326.86            0.00        232.76
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=     2327.61      T&I=        0.75

008 34021300     53        US             870.89 KG         1163.43        1163.43            0.00        116.38
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=     1163.81      T&I=        0.38

ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 3
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF23
                                         BOX NO  :
O/REF   : PRO040263                      A/REF   : B00148206/1                                FOLIO    :

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

009 34021300     53        US             213.18 KG          458.41         458.41            0.00         45.85
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=      458.56      T&I=        0.15

TOTAL NUMBER OF PACKAGES:         0   (ZERO)

                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print Landscape", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 3, 3)]
		public void TestTwoPageEntriesForCMR_PortraitWithMultipleCurrencies()
		{
			AUCustomsDataRegistry.Instance.IncludeAQISServicePaymentsInTotalPayableOnEntryPrint.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			JobDeclaration testDec = SetupCMRDataForTwoPageEntryWithMultipleCurrencies();

			for (int i = 0; i < 5; i++)
			{
				JobComInvoiceLine testLine10 = testDec.Invoices[0].JobComInvoiceLines.AddNew();
				testLine10.JI_Tariff = "2922.19.00 40";
				testLine10.JI_CustomsQuantity = 740.2600m;
				testLine10.JI_CustomsUnitQty = "KG";
				testLine10.JI_AddInfo = "WETE=404*DTY=1234.00*GSTE=404*ADJ=1546487USD*WMC=13213213256*TI2=AD:13213131*DCX=AD*ORG=US*TCI=AD:12345678*DMP=1234567.89*STD=1231.00*PUP=Y*TC2=8840003*TRN=131*QT2=131131.00*WET=1313.00*TFQ=1312*VID=ADFFSDAFASDFASDADFSAFSDFSDFSDFSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFASDFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*LCTE=404*FOD=051013*AMB=COPQTVD*TAN=1233*CL2=12345678*PRI=TC:12345678*DXP=1234753*PST=GEN*LCTI=Y*LCTQ=Y*DRE=1.2455*ISC=1234*UQ2=KG*DXT=C*DSN=123456*VAN=1234*LCP=1213.00*ODF=13213.00*MLPI=Y*MD2=123456*SCN=SN1234567*TR2=131*ELA=AAA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*RNO=001*WETQ=Y";
				testLine10.JI_Description = "N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)";
				testLine10.JI_InvoiceQuantity = 740.26000m;
				testLine10.JI_InvoiceUQ = "KG";
				testLine10.JI_LineNo = (short)1;
				testLine10.JI_LinePrice = 1578.21m;
				testLine10.JI_Weight = 740.26000m;
				testLine10.JI_WeightUQ = "KG";
			}

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testDec.JE_MergeBy = "NON";
			testDec.DoMerge();
			testDec.CustomsEntryHeaders[0].AQISServicePaymentAmount = 33m;
			testDec.CustomsEntryHeaders[0].CH_TotalPaid += 33m;

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], true);
			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 1
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:

OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF23
                                         BOX NO  :
O/REF   : PRO040263                      A/REF   : B00148206/1                                FOLIO    :

                                         FOB  (1) :       75717.47 = $A      75717.47         LOAD  PT : KANSAS CITY
                                         CIF  (1) :       76098.60 = $A      76098.60         FIRST PT : SYDNEY             03MAR05
                                         GRWT (KG):       17426.00 = KG      17426.00         DSCH  PT : SYDNEY             03MAR05
                                         T & I    :         381.13 = $A        381.13         DEST  PT : SYDNEY

                                                                                              TOTAL CUSTOMS VALUE : $A 6751200.95
ITERMS  : FOB

VALUATION DATE : 02MAR05                 ITOT (1) :  75717.47 = $A      75717.47
                                         OSEA (1) :    350.00 = $A        350.00              FACTOR              : 1.00000000
CRNCYS 1 AUD @ 1.0000                    ONS  (1) :     31.13 = $A         31.13
       2 HKD @ 0.62 (02MAR05)                                                                 CALCULATION DATE    : 01APR05
       3 USD @ 1.39 (02MAR05)

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

001 34029000     19        US            4623.88 L         29117.95       29117.95            0.00       2911.95
    (US,US,PS)                                                               UT/SG            FREE
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=    29119.59      T&I=        1.64

002 34039990     37        US             204.69 L          1729.27        1729.27           11.15        174.05
    (GEN)                                                                    UT/SG           0.00%+0.05449/L
    ADD INFO: ORG=US*PST=GEN*RNO=001
    LUBRICATING PREPARATIONS (INCLUDING CUTTING-OIL PREPARATIONS, BOLT OR NUT RELEAS
    VOTI=     1740.52      T&I=        0.10

003 34039990     37        US             204.77 L          1717.84        1717.84           11.15        172.90
    (GEN)                                                                    UT/SG           0.00%+0.05449/L
    ADD INFO: ORG=US*PST=GEN*RNO=001
    LUBRICATING PREPARATIONS (INCLUDING CUTTING-OIL PREPARATIONS, BOLT OR NUT RELEAS
    VOTI=     1729.09      T&I=        0.10

004 39072000     37   505  US            6179.74 KG        28040.10       28040.10          841.20       2888.28    TC 9604601
    (GEN)                                                                    UT/SG           3.00%
    ADD INFO: ORG=US*PST=GEN*RNO=001
    OTHER POLYETHERS EXCL POLYETHYLENE OXIDE,PROPYLENE OXIDE BASED POLYOLS
    VOTI=    28882.88      T&I=        1.58

005 29221900     40        US             740.26 KG         1578.21     1114158.79         1234.00          0.00
    (GEN)                                                                    UT/TV+ADJ       0.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ADJ=1546487USD*AMB=COPQTVD*CL2=12345678*DCX=AD*DMP=1234567.89*DRE=1.2455*DSN=123456*DTY=1234*DXP=1234753AUD*DXT=C*ELA=A
    AA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*FOD=051013*GSTE=404*ISC=1234*LCP=1213.00
    *LCTE=404*LCTI=Y*LCTQ=Y*MD2=123456*MLPI=Y*ODF=13213*ORG=US*PRI=TC:12345678*PST=GEN*PUP=Y*QT2=740.26*RNO=001*SCN=SN1234567*STD=123
    1*TAN=1233*TC2=8840003*TCI=AD:12345678*TFQ=1312*TI2=AD:13213131*TR2=131*TRN=131*UQ2=KG*VAN=1234*VID=ADFFSDAFASDFASDADFSAFSDFSDFSD
    FSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFAS
    DFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*WET=1313*WETE=404*WETQ=Y*WMC=13213213256
    N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)
    VOTI=  2350023.58      T&I=       62.90      DMP=  1234567.89

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                               8267.50   *
                                                                                      * DUMPING DUTY                    7407407.34   *
                                                                                      * GST                                6711.50   *
                                                                                      * OTHER CHARGES                        44.10   *
Delivery: ABI GAS & TOOLS                                                             *                                              *
          171 ABBOTSFORD ROAD                                                         * TOTAL AMOUNT PAYABLE ***        7429816.44 ***
          MAYNE QLD AUSTRALIA                                                         * QUARANTINE SERVICES FEE              33.00   *
                                                                                      ************************************************
                                                                                      * OFFICIAL USE ONLY                            *
I ......................................................                              *                                              *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * ................................     /  /    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              * SIGNATURE OF AUTHORISING OFFICER     DATE    *
                                                                                      ************************************************
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     * WARRANTED AND RECEIPTED:                     *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      *                                              *
                                                                                      ************************************************

ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 2
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:

OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF23
                                         BOX NO  :
O/REF   : PRO040263                      A/REF   : B00148206/1                                FOLIO    :

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

006 29221900     40        US             740.26 KG         1578.21     1114158.79         1234.00          0.00
    (GEN)                                                                    UT/TV+ADJ       0.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ADJ=1546487USD*AMB=COPQTVD*CL2=12345678*DCX=AD*DMP=1234567.89*DRE=1.2455*DSN=123456*DTY=1234*DXP=1234753AUD*DXT=C*ELA=A
    AA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*FOD=051013*GSTE=404*ISC=1234*LCP=1213.00
    *LCTE=404*LCTI=Y*LCTQ=Y*MD2=123456*MLPI=Y*ODF=13213*ORG=US*PRI=TC:12345678*PST=GEN*PUP=Y*QT2=740.26*RNO=001*SCN=SN1234567*STD=123
    1*TAN=1233*TC2=8840003*TCI=AD:12345678*TFQ=1312*TI2=AD:13213131*TR2=131*TRN=131*UQ2=KG*VAN=1234*VID=ADFFSDAFASDFASDADFSAFSDFSDFSD
    FSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFAS
    DFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*WET=1313*WETE=404*WETQ=Y*WMC=13213213256
    N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)
    VOTI=  2350023.58      T&I=       62.90      DMP=  1234567.89

007 29221900     40        US             740.26 KG         1578.21     1114158.79         1234.00          0.00
    (GEN)                                                                    UT/TV+ADJ       0.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ADJ=1546487USD*AMB=COPQTVD*CL2=12345678*DCX=AD*DMP=1234567.89*DRE=1.2455*DSN=123456*DTY=1234*DXP=1234753AUD*DXT=C*ELA=A
    AA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*FOD=051013*GSTE=404*ISC=1234*LCP=1213.00
    *LCTE=404*LCTI=Y*LCTQ=Y*MD2=123456*MLPI=Y*ODF=13213*ORG=US*PRI=TC:12345678*PST=GEN*PUP=Y*QT2=740.26*RNO=001*SCN=SN1234567*STD=123
    1*TAN=1233*TC2=8840003*TCI=AD:12345678*TFQ=1312*TI2=AD:13213131*TR2=131*TRN=131*UQ2=KG*VAN=1234*VID=ADFFSDAFASDFASDADFSAFSDFSDFSD
    FSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFAS
    DFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*WET=1313*WETE=404*WETQ=Y*WMC=13213213256
    N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)
    VOTI=  2350023.58      T&I=       62.90      DMP=  1234567.89

008 29221900     40        US             740.26 KG         1578.21     1114158.79         1234.00          0.00
    (GEN)                                                                    UT/TV+ADJ       0.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ADJ=1546487USD*AMB=COPQTVD*CL2=12345678*DCX=AD*DMP=1234567.89*DRE=1.2455*DSN=123456*DTY=1234*DXP=1234753AUD*DXT=C*ELA=A
    AA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*FOD=051013*GSTE=404*ISC=1234*LCP=1213.00
    *LCTE=404*LCTI=Y*LCTQ=Y*MD2=123456*MLPI=Y*ODF=13213*ORG=US*PRI=TC:12345678*PST=GEN*PUP=Y*QT2=740.26*RNO=001*SCN=SN1234567*STD=123
    1*TAN=1233*TC2=8840003*TCI=AD:12345678*TFQ=1312*TI2=AD:13213131*TR2=131*TRN=131*UQ2=KG*VAN=1234*VID=ADFFSDAFASDFASDADFSAFSDFSDFSD
    FSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFAS
    DFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*WET=1313*WETE=404*WETQ=Y*WMC=13213213256
    N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)
    VOTI=  2350023.58      T&I=       62.90      DMP=  1234567.89

009 29221900     40        US             740.26 KG         1578.21     1114158.79         1234.00          0.00
    (GEN)                                                                    UT/TV+ADJ       0.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ADJ=1546487USD*AMB=COPQTVD*CL2=12345678*DCX=AD*DMP=1234567.89*DRE=1.2455*DSN=123456*DTY=1234*DXP=1234753AUD*DXT=C*ELA=A
    AA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*FOD=051013*GSTE=404*ISC=1234*LCP=1213.00
    *LCTE=404*LCTI=Y*LCTQ=Y*MD2=123456*MLPI=Y*ODF=13213*ORG=US*PRI=TC:12345678*PST=GEN*PUP=Y*QT2=740.26*RNO=001*SCN=SN1234567*STD=123
    1*TAN=1233*TC2=8840003*TCI=AD:12345678*TFQ=1312*TI2=AD:13213131*TR2=131*TRN=131*UQ2=KG*VAN=1234*VID=ADFFSDAFASDFASDADFSAFSDFSDFSD
    FSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFAS
    DFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*WET=1313*WETE=404*WETQ=Y*WMC=13213213256
    N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)
    VOTI=  2350023.58      T&I=       62.90      DMP=  1234567.89

010 29221900     40        US             740.26 KG         1578.21     1114158.79         1234.00          0.00
    (GEN)          Quarantine Permits: permit1,permit2                       UT/TV+ADJ       0.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ADJ=1546487USD*AMB=COPQTVD*CL2=12345678*DCX=AD*DMP=1234567.89*DRE=1.2455*DSN=123456*DTY=1234*DXP=1234753AUD*DXT=C*ELA=A
    AA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*FOD=051013*GSTE=404*ISC=1234*LCP=1213.00
    *LCTE=404*LCTI=Y*LCTQ=Y*MD2=123456*MLPI=Y*ODF=13213*ORG=US*PRI=TC:12345678*PST=GEN*PUP=Y*QT2=740.26*RNO=001*SCN=SN1234567*STD=123
    1*TAN=1233*TC2=8840003*TCI=AD:12345678*TFQ=1312*TI2=AD:13213131*TR2=131*TRN=131*UQ2=KG*VAN=1234*VID=ADFFSDAFASDFASDADFSAFSDFSDFSD
    FSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFAS
    DFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*WET=1313*WETE=404*WETQ=Y*WMC=13213213256
    N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)
    VOTI=  2350023.56      T&I=       62.88      DMP=  1234567.89

011 29221900     41        US            1469.64 KG         1694.35        1694.35            0.00        169.44
    (GEN)                                                                    UT/TV            FREE
    Quarantine Permits: 1111111111,2222222222,3333333333,444444444
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*PST=GEN*RNO=001
    N,N-DIETHYLAMINOETHANOL (CAS 100-37-8) VERY LONG DESCRIPTION LINE THAT SHOULD MAKE THE SYSTEM PRINT AT LEAST TWO LINES 1234567890
    1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 12345678
    90 1234567890 1234567890 1234567890 1234567890
    VOTI=     1694.45      T&I=        0.10

012 34021300     53        US            1741.79 KG         2326.86        2326.86            0.00        232.69
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=     2326.99      T&I=        0.13

ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 3
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:

OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF23
                                         BOX NO  :
O/REF   : PRO040263                      A/REF   : B00148206/1                                FOLIO    :

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

013 34021300     53        US             870.89 KG         1163.43        1163.43            0.00        116.35
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=     1163.50      T&I=        0.07

014 34021300     53        US             213.18 KG          458.41         458.41            0.00         45.84
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=      458.44      T&I=        0.03

TOTAL NUMBER OF PACKAGES:         0   (ZERO)

                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print Portrait", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestEntryPrintCMRSAC()
		{
			Env.Registry.BrokerageID = "00079C";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";

			var entryHeader = GetCusEntryHeaderForSea(true, false);
			testDec.JE_TotalNoOfPacks = 456;
			container1 = testDec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OCLU10000020";
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			testDec.DoMerge();
			entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "B00001000/1";

			var orgMessage = Factory.New<CMRIMDMessage>();
			orgMessage.EM_MessageText = CMRImportDeclarationTestData.IMD; //IMD+B00122382/1/NAD+VT+AA33HF
			orgMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			orgMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			entryHeader.Messages.Add(orgMessage);
			entryHeader.EntryNumber = "AAAA7GW6R";

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);
			AssertNotNull("EntryPrint Exposed", entryPrint.EntryPrint);
			Assert("EntryPrint Generated", entryPrint.Generated);

			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 1
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE(UTC)                      X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO. AAAA7GW6R  PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : SEA
(55006646046) (12345678901/234)                    (00079C)
                                                                          (AA33HF)            SHIP     : BAI YUN HE
                                         BOX NO  :                                                       (9203473)
O/REF   : OWNERS REF                     A/REF   : B00001000/1                                SHIP VOY : QF1234

                                         FOB  (1) :         987.65 = $A        710.54         LOAD  PT : SINGAPORE
                                         CIF  (1) :         987.65 = $A        710.54         FIRST PT : SYDNEY             01JAN10
                                         GRWT (KG):         123.45 = KG        123.45         DSCH  PT : SYDNEY             01JAN10
                                         T & I    :           0.00 = $A          0.00         DEST  PT : MELBOURNE
ITERMS  : FOB
                                                                                              TOTAL CUSTOMS VALUE : $A 710.54
VALUATION DATE : 01NOV05                 ITOT (1) :    987.65 = $A        710.54
                                                                                              FACTOR              : 0.71942446
CRNCYS 1 USD @ 1.39 (01NOV05)
       2 AUD @ 1.0000                                                                         CALCULATION DATE    :
       3 ZAR @ 1.79 (01NOV05)

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

001                        JP                  0             987.65         710.54            0.00          0.00
                                                                             UT/TV            FREE
    SUPPLIER: ABA BEUL (AAA3336347E)
    ADD INFO: EFD=070604*ORG=JP
    VOTI=      710.54      T&I=        0.00

TOTAL NUMBER OF PACKAGES:       456   (FOUR FIVE SIX)

AQT1/M   AQT1HBL/H

CONTAINER NOS:   (LCL)OCLU10000020

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * GST                                   0.00   *
                                                                                      * OTHER CHARGES                         0.00   *
Delivery: Name of Importer                                                            *                                              *
          12345678901234567890123456789012345678901234567890                          * TOTAL AMOUNT PAYABLE ***              0.00 ***
          12345678901234567890123456789012345678901234567890                          ************************************************
          1234567890123456789012345 NSW 1234567890 AUSTRALIA                          * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************
                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestEntryPrintCMRSACwithLines()
		{
			Env.Registry.BrokerageID = "00079C";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";

			CusEntryHeader entryHeader = GetCusEntryHeaderForSea(false, true);
			testDec.JE_TotalNoOfPacks = 456;
			container1 = testDec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OCLU10000020";
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			testDec.DoMerge();
			entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "B00001000/1";

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);
			AssertNotNull("EntryPrint Exposed", entryPrint.EntryPrint);
			Assert("EntryPrint Generated", entryPrint.Generated);

			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 1
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : SEA
(55006646046) (12345678901/234)                    (00079C)
                                                                          (A2M139)            SHIP     : BAI YUN HE
                                         BOX NO  :                                                       (9203473)
O/REF   : OWNERS REF                     A/REF   : B00001000/1                                SHIP VOY : QF1234

                                         FOB  (1) :         997.65 = $A        717.73         LOAD  PT : SINGAPORE
                                         CIF  (1) :         997.65 = $A        717.73         FIRST PT : SYDNEY             01JAN10
                                         GRWT (KG):         123.45 = KG        123.45         DSCH  PT : SYDNEY             01JAN10
                                         T & I    :           0.00 = $A          0.00         DEST  PT : MELBOURNE
ITERMS  : FOB
                                                                                              TOTAL CUSTOMS VALUE : $A 1046.35
VALUATION DATE : 01NOV05                 ITOT (1) :    997.65 = $A        717.73
                                                                                              FACTOR              : 0.71942446
CRNCYS 1 USD @ 1.39 (01NOV05)
       2 AUD @ 1.0000                                                                         CALCULATION DATE    :
       3 ZAR @ 1.79 (01NOV05)

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

001 44109000     25        JP                  5 CU          997.65        1046.35           52.31        109.86
    (GEN)                                                                    UT/UT+ADJ       5.00%
    SUPPLIER: ABA BEUL (AAA3336347E)
    ADD INFO: ADJ=456.78USD*EFD=070604*ORG=JP*PST=GEN*RNO=001
    Description of the goods for line 1
    VOTI=     1098.66      T&I=        0.00

TOTAL NUMBER OF PACKAGES:       456   (FOUR FIVE SIX)

AQT1/M   AQT1HBL/H

CONTAINER NOS:   (LCL)OCLU10000020

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                                 52.31   *
                                                                                      * GST                                 109.86   *
                                                                                      * OTHER CHARGES                        61.00   *
Delivery: Name of Importer                                                            *                                              *
          12345678901234567890123456789012345678901234567890                          * TOTAL AMOUNT PAYABLE ***            223.17 ***
          12345678901234567890123456789012345678901234567890                          ************************************************
          1234567890123456789012345 NSW 1234567890 AUSTRALIA                          * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************
                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestEntryPrintWithAmberAndPupStatement()
		{
			Env.Registry.BrokerageID = "00079C";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";
			GetCusEntryHeaderForSea(true, true);

			testDec.AddInfo.ZA_HART_Hidden = "QUANTITY";
			testDec.JE_AmberStatement = "AmberStatement. AmberStatement.";
			testDec.JE_PaidUnderProtestStatement = "PaidUnderProtest Statement.";
			testDec.DoMerge();

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);

			AssertNotNull("EntryPrint Exposed", entryPrint.EntryPrint);
			Assert("EntryPrint Generated", entryPrint.Generated);

			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 1
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : SEA
(55006646046) (12345678901/234)                    (00079C)
                                                                          (A2M139)            SHIP     : BAI YUN HE
                                         BOX NO  :                                                       (9203473)
O/REF   : OWNERS REF                     A/REF   : B9999999/1                                 SHIP VOY : QF1234

                                         FOB  (1) :         987.65 = $A        710.54         LOAD  PT : SINGAPORE
                                         CIF  (1) :         987.65 = $A        710.54         FIRST PT : SYDNEY             01JAN10
                                         GRWT (KG):         123.45 = KG        123.45         DSCH  PT : SYDNEY             01JAN10
                                         T & I    :           0.00 = $A          0.00         DEST  PT : MELBOURNE
ITERMS  : FOB
                                                                                              TOTAL CUSTOMS VALUE : $A 710.54
VALUATION DATE : 01NOV05                 ITOT (1) :    987.65 = $A        710.54
                                                                                              FACTOR              : 0.71942446
CRNCYS 1 USD @ 1.39 (01NOV05)
       2 AUD @ 1.0000                                                                         CALCULATION DATE    :
       3 ZAR @ 1.79 (01NOV05)

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

001                        JP                  0             987.65         710.54            0.00          0.00
                                                                             UT/TV            FREE
    SUPPLIER: ABA BEUL (AAA3336347E)
    ADD INFO: EFD=070604*ORG=JP
    VOTI=      710.54      T&I=        0.00

TOTAL NUMBER OF PACKAGES:       123   (ONE TWO THREE)

AQT1/M   AQT1HBL/H

AMBER REASON AND STATEMENT:

REASON: QUANTITY
STATEMENT: AmberStatement. AmberStatement.

PAID UNDER PROTEST STATEMENT:

PaidUnderProtest Statement.

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * GST                                   0.00   *
                                                                                      * OTHER CHARGES                         0.00   *
Delivery: Name of Importer                                                            *                                              *
          12345678901234567890123456789012345678901234567890                          * TOTAL AMOUNT PAYABLE ***              0.00 ***
          12345678901234567890123456789012345678901234567890                          ************************************************
          1234567890123456789012345 NSW 1234567890 AUSTRALIA                          * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************
                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestEntryPrintWithAmberAndPupStatementPageOverload()
		{
			Env.Registry.BrokerageID = "00079C";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";
			GetCusEntryHeaderForSea(true, true);

			container1 = testDec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OCLU10000020";
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			testDec.AddInfo.ZA_HART_Hidden = "QUANTITY";
			testDec.JE_AmberStatement = @"AmberStatement. AmberStatement. 
AmberStatement. AmberStatement. 
AmberStatement. 
AmberStatement. AmberStatement. 
AmberStatement. AmberStatement.
AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement.
AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement.";
			testDec.JE_PaidUnderProtestStatement = @"PaidUnderProtest Statement. 
PaidUnderProtest Statement. 
PaidUnderProtest Statement. 
PaidUnderProtest Statement. 
PaidUnderProtest Statement.
PaidUnderProtest Statement. PaidUnderProtest Statement. PaidUnderProtest Statement. PaidUnderProtest Statement. PaidUnderProtest Statement.
PaidUnderProtest Statement. PaidUnderProtest Statement. PaidUnderProtest Statement. PaidUnderProtest Statement. PaidUnderProtest Statement.";
			testDec.DoMerge();

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);

			AssertNotNull("EntryPrint Exposed", entryPrint.EntryPrint);
			Assert("EntryPrint Generated", entryPrint.Generated);

			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 1
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : SEA
(55006646046) (12345678901/234)                    (00079C)
                                                                          (A2M139)            SHIP     : BAI YUN HE
                                         BOX NO  :                                                       (9203473)
O/REF   : OWNERS REF                     A/REF   : B9999999/1                                 SHIP VOY : QF1234

                                         FOB  (1) :         987.65 = $A        710.54         LOAD  PT : SINGAPORE
                                         CIF  (1) :         987.65 = $A        710.54         FIRST PT : SYDNEY             01JAN10
                                         GRWT (KG):         123.45 = KG        123.45         DSCH  PT : SYDNEY             01JAN10
                                         T & I    :           0.00 = $A          0.00         DEST  PT : MELBOURNE
ITERMS  : FOB
                                                                                              TOTAL CUSTOMS VALUE : $A 710.54
VALUATION DATE : 01NOV05                 ITOT (1) :    987.65 = $A        710.54
                                                                                              FACTOR              : 0.71942446
CRNCYS 1 USD @ 1.39 (01NOV05)
       2 AUD @ 1.0000                                                                         CALCULATION DATE    :
       3 ZAR @ 1.79 (01NOV05)

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

001                        JP                  0             987.65         710.54            0.00          0.00
                                                                             UT/TV            FREE
    SUPPLIER: ABA BEUL (AAA3336347E)
    ADD INFO: EFD=070604*ORG=JP
    VOTI=      710.54      T&I=        0.00

TOTAL NUMBER OF PACKAGES:       123   (ONE TWO THREE)

AQT1/M   AQT1HBL/H

CONTAINER NOS:   (FCL)OCLU10000020

AMBER REASON AND STATEMENT:

REASON: QUANTITY
STATEMENT: AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement.
AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement.
AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement. AmberStatement.
AmberStatement. AmberStatement. AmberStatement. AmberStatement.

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * GST                                   0.00   *
                                                                                      * OTHER CHARGES                         0.00   *
Delivery: Name of Importer                                                            *                                              *
          12345678901234567890123456789012345678901234567890                          * TOTAL AMOUNT PAYABLE ***              0.00 ***
          12345678901234567890123456789012345678901234567890                          ************************************************
          1234567890123456789012345 NSW 1234567890 AUSTRALIA                          * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************

ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 2
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : SEA
(55006646046) (12345678901/234)                    (00079C)
                                                                          (A2M139)            SHIP     : BAI YUN HE
                                         BOX NO  :                                                       (9203473)
O/REF   : OWNERS REF                     A/REF   : B9999999/1                                 SHIP VOY : QF1234

PAID UNDER PROTEST STATEMENT:

PaidUnderProtest Statement. PaidUnderProtest Statement. PaidUnderProtest Statement. PaidUnderProtest Statement. PaidUnderProtest
Statement. PaidUnderProtest Statement. PaidUnderProtest Statement. PaidUnderProtest Statement. PaidUnderProtest Statement.
PaidUnderProtest Statement. PaidUnderProtest Statement. PaidUnderProtest Statement. PaidUnderProtest Statement. PaidUnderProtest
Statement. PaidUnderProtest Statement.

                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestEntryPrintSACWithTooManyContainers()
		{
			GetCusEntryHeaderForSea(true, true);
			for (int i = 0; i < 100; i++)
			{
				container1 = testDec.CusContainers.AddNew();
				container1.CO_ContainerNumber = "OCLU100000" + i.ToString();
				container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			}
			testDec.DoMerge();

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);

			AssertNotNull("EntryPrint Exposed", entryPrint.EntryPrint);
			Assert("EntryPrint Generated", entryPrint.Generated);

			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 1
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : SEA
(55006646046) (12345678901/234)                    ()
                                                                          ()                  SHIP     : BAI YUN HE
                                         BOX NO  :                                                       (9203473)
O/REF   : OWNERS REF                     A/REF   : B9999999/1                                 SHIP VOY : QF1234

                                         FOB  (1) :         987.65 = $A        710.54         LOAD  PT : SINGAPORE
                                         CIF  (1) :         987.65 = $A        710.54         FIRST PT : SYDNEY             01JAN10
                                         GRWT (KG):         123.45 = KG        123.45         DSCH  PT : SYDNEY             01JAN10
                                         T & I    :           0.00 = $A          0.00         DEST  PT : MELBOURNE
ITERMS  : FOB
                                                                                              TOTAL CUSTOMS VALUE : $A 710.54
VALUATION DATE : 01NOV05                 ITOT (1) :    987.65 = $A        710.54
                                                                                              FACTOR              : 0.71942446
CRNCYS 1 USD @ 1.39 (01NOV05)
       2 AUD @ 1.0000                                                                         CALCULATION DATE    :
       3 ZAR @ 1.79 (01NOV05)

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

001                        JP                  0             987.65         710.54            0.00          0.00
                                                                             UT/TV            FREE
    SUPPLIER: ABA BEUL (AAA3336347E)
    ADD INFO: EFD=070604*ORG=JP
    VOTI=      710.54      T&I=        0.00

TOTAL NUMBER OF PACKAGES:       123   (ONE TWO THREE)

AQT1/M   AQT1HBL/H

CONTAINER NOS:   (FCL)OCLU1000000  (FCL)OCLU1000001  (FCL)OCLU1000002  (FCL)OCLU1000003  (FCL)OCLU1000004  (FCL)OCLU1000005
                 (FCL)OCLU1000006  (FCL)OCLU1000007  (FCL)OCLU1000008  (FCL)OCLU1000009  (FCL)OCLU10000010  (FCL)OCLU10000011
                 (FCL)OCLU10000012  (FCL)OCLU10000013  (FCL)OCLU10000014  (FCL)OCLU10000015  (FCL)OCLU10000016  (FCL)OCLU10000017
                 (FCL)OCLU10000018  (FCL)OCLU10000019  (FCL)OCLU10000020  (FCL)OCLU10000021  (FCL)OCLU10000022  (FCL)OCLU10000023
                 (FCL)OCLU10000024  (FCL)OCLU10000025  (FCL)OCLU10000026  (FCL)OCLU10000027  (FCL)OCLU10000028  (FCL)OCLU10000029
                 (FCL)OCLU10000030  (FCL)OCLU10000031  (FCL)OCLU10000032  (FCL)OCLU10000033  (FCL)OCLU10000034  (FCL)OCLU10000035
                 (FCL)OCLU10000036  (FCL)OCLU10000037  (FCL)OCLU10000038  (FCL)OCLU10000039  (FCL)OCLU10000040  (FCL)OCLU10000041
                 (FCL)OCLU10000042  (FCL)OCLU10000043  (FCL)OCLU10000044  (FCL)OCLU10000045  (FCL)OCLU10000046  (FCL)OCLU10000047
                 (FCL)OCLU10000048  (FCL)OCLU10000049  (FCL)OCLU10000050  (FCL)OCLU10000051  (FCL)OCLU10000052  (FCL)OCLU10000053
                 (FCL)OCLU10000054  (FCL)OCLU10000055  (FCL)OCLU10000056  (FCL)OCLU10000057  (FCL)OCLU10000058  (FCL)OCLU10000059

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * GST                                   0.00   *
                                                                                      * OTHER CHARGES                         0.00   *
Delivery: Name of Importer                                                            *                                              *
          12345678901234567890123456789012345678901234567890                          * TOTAL AMOUNT PAYABLE ***              0.00 ***
          12345678901234567890123456789012345678901234567890                          ************************************************
          1234567890123456789012345 NSW 1234567890 AUSTRALIA                          * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************

ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 2
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : SEA
(55006646046) (12345678901/234)                    ()
                                                                          ()                  SHIP     : BAI YUN HE
                                         BOX NO  :                                                       (9203473)
O/REF   : OWNERS REF                     A/REF   : B9999999/1                                 SHIP VOY : QF1234

CONTAINER NOS:   (FCL)OCLU10000060  (FCL)OCLU10000061  (FCL)OCLU10000062  (FCL)OCLU10000063  (FCL)OCLU10000064  (FCL)OCLU10000065
                 (FCL)OCLU10000066  (FCL)OCLU10000067  (FCL)OCLU10000068  (FCL)OCLU10000069  (FCL)OCLU10000070  (FCL)OCLU10000071
                 (FCL)OCLU10000072  (FCL)OCLU10000073  (FCL)OCLU10000074  (FCL)OCLU10000075  (FCL)OCLU10000076  (FCL)OCLU10000077
                 (FCL)OCLU10000078  (FCL)OCLU10000079  (FCL)OCLU10000080  (FCL)OCLU10000081  (FCL)OCLU10000082  (FCL)OCLU10000083
                 (FCL)OCLU10000084  (FCL)OCLU10000085  (FCL)OCLU10000086  (FCL)OCLU10000087  (FCL)OCLU10000088  (FCL)OCLU10000089
                 (FCL)OCLU10000090  (FCL)OCLU10000091  (FCL)OCLU10000092  (FCL)OCLU10000093  (FCL)OCLU10000094  (FCL)OCLU10000095
                 (FCL)OCLU10000096  (FCL)OCLU10000097  (FCL)OCLU10000098  (FCL)OCLU10000099

                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestEntryPrintCMRNature10AirInvalidWeightUnits()
		{
			Env.Registry.BrokerageID = "00079C";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";
			OrgHeader importer = GetImporterOrg();
			OrgHeader supplier = GetSupplierOrg();

			CusEntryHeader entryHeader = GetCusEntryHeaderForAir();
			invoiceHeader.JZ_Weight = 0m;
			invoiceHeader.JZ_WeightUQ = ZString.Empty;
			testDec.JE_TotalWeightUnit = "CS";

			SetupPackages();

			testDec.DoMerge();

			entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "B00001000/1";

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);

			AssertNotNull("EntryPrint Exposed", entryPrint.EntryPrint);
			Assert("EntryPrint Generated", entryPrint.Generated);

			ZString expectedResult =
				@"ENTRY PRINT - ESTIMATE                   X  XXXXXX    ******************************          PAGE 1
Not Sent                                 X  X    X    *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                           X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
                                         X  X    X    ******************************
                                         X  XXXXXX
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : Name of Importer               AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (12345678901/234)                    (00079C)
                                                                          (A2M139)            AIRCR    : QF1234
                                         BOX NO  :
O/REF   : OWNERS REF                     A/REF   : B00001000/1                                FOLIO    :

                                         FOB  (1) :         997.65 = $A        717.73         LOAD  PT : SINGAPORE
                                         CIF  (1) :         997.65 = $A        717.73         FIRST PT : SYDNEY             01JAN10
                                         GRWT (CS):           0.00 = KG          0.00         DSCH  PT : SYDNEY             01JAN10
                                         T & I    :           0.00 = $A          0.00         DEST  PT : MELBOURNE
ITERMS  : FOB
                                                                                              TOTAL CUSTOMS VALUE : $A 1046.35
VALUATION DATE : 01NOV05                 ITOT (1) :    997.65 = $A        717.73
                                                                                              FACTOR              : 0.71942446
CRNCYS 1 USD @ 1.39 (01NOV05)
       2 AUD @ 1.0000                                                                         CALCULATION DATE    :
       3 ZAR @ 1.79 (01NOV05)

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

001 44109000     25        JP                  5 CU          997.65        1046.35           52.31        109.86
    (GEN)                                                                    UT/UT+ADJ       5.00%
    SUPPLIER: ABA BEUL (AAA3336347E)
    ADD INFO: ADJ=456.78USD*EFD=070604*ORG=JP*PST=GEN*RNO=001
    Description of the goods for line 1
    VOTI=     1098.66      T&I=        0.00

TOTAL NUMBER OF PACKAGES:      1000   (ONE ZERO ZERO ZERO)

NO. OF PACKAGES / MASTER BILL / HOUSE BILL

400 PCE / AQT1 / AQT1HBL
    PACKAGE MARKS: PACKING LINE 1 MARKS AND NUMBERS1234567890123456789012345678901234567890123456789012345678901234567890123456789012
    345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901
    234567890123456789012345678901234567890
600 PCE / AQT1 / AQT2HBL
    PACKAGE MARKS: PACKING LINE 2 MARKS AND NUMBERS

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                                 52.31   *
                                                                                      * GST                                 109.86   *
                                                                                      * OTHER CHARGES                        44.85   *
Delivery: Name of Importer                                                            *                                              *
          12345678901234567890123456789012345678901234567890                          * TOTAL AMOUNT PAYABLE ***            207.02 ***
          12345678901234567890123456789012345678901234567890                          ************************************************
          1234567890123456789012345 NSW 1234567890 AUSTRALIA                          * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************
                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2005, 11, 1)]
		public void TestEntryPrintWithFailedAmendmentCMRForLandscape()
		{
			JobDeclaration testDec = SetupCMRDataForTwoPageEntry();

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testDec.JE_MergeBy = "NON";
			testDec.DoMerge();

			testDec.JE_EntrySubmittedDate = new ZDateTime(2005, 3, 3);
			testDec.CustomsEntryHeaders[0].AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayPending;
			testDec.CustomsEntryHeaders[0].CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			testDec.CustomsEntryHeaders[0].CH_EntryStatus = CMRImportEntryAdvice.Finalised.Code;

			testDec.DoMerge();// again to recalc with new effective duty date

			EDIMessage message = testDec.CustomsEntryHeaders[0].Messages.AddNew();
			message.EM_SystemCreateTimeUtc = new ZDateTime(2005, 12, 25, 11, 30, 35);
			StmALog addLogForWorkComplete = testDec.Logs.AddNew(Events.DeclarationWorkComplete, "work complete");
			StmALog addLogForImpedement = testDec.CustomsEntryHeaders[0].Logs.AddNew(Events.CustomsImpedimentReceived, "quarantine impedement");
			StmALog addLogForAmendmentQueued = testDec.Logs.AddNew(Events.DeclarationAmendmentRejected, "xxx");

			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], false);
			ZString expectedResult =
				@"ENTRY PRINT                              X  XXXXXX    ******************************          PAGE 1
Finalized - Paid:work complete           X  X    X    *     AUSTRALIAN CUSTOMS     *
Impediment(s):quarantine                 X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
impedement(Amendment Failed, Details     X  X    X    ******************************
may not match ICS.)                      X  XXXXXX
Last Msg: LODGE(25DEC05 11:30UTC)                       PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF23
                                         BOX NO  :
O/REF   : PRO040263                      A/REF   : B00148206/1                                FOLIO    :

                                         FOB  (1) :       44796.46 = $A      44796.46         LOAD  PT : KANSAS CITY
                                         CIF  (1) :       45177.59 = $A      45177.59         FIRST PT : SYDNEY             03MAR05
                                         GRWT (KG):       17426.00 = KG      17426.00         DSCH  PT : SYDNEY             03MAR05
                                         T & I    :         381.13 = $A        381.13         DEST  PT : SYDNEY
ITERMS  : FOB
                                                                                              TOTAL CUSTOMS VALUE : $A 1157377.04
VALUATION DATE : 02MAR05                 ITOT (1) :  44796.46 = $A      44796.46
                                         OSEA (1) :    350.00 = $A        350.00              FACTOR              : 1.00000000
CRNCYS 1 AUD @ 1.0000                    ONS  (1) :     31.13 = $A         31.13
       2 USD @ 1.39 (02MAR05)                                                                 CALCULATION DATE    : 03MAR05

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

001 34029000     19        US            4623.88 L         18053.13       18053.13            0.00       1805.90
    (US,US,PS)                                                               UT/SG            FREE
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=    18059.07      T&I=        5.94

002 34039990     37        US             204.69 L          1072.15        1072.15           11.15        108.36
    (GEN)                                                                    UT/SG           0.00%+0.05449/L
    ADD INFO: ORG=US*PST=GEN*RNO=001
    LUBRICATING PREPARATIONS (INCLUDING CUTTING-OIL PREPARATIONS, BOLT OR NUT RELEAS
    VOTI=     1083.65      T&I=        0.35

003 34039990     37        US             204.77 L          1065.06        1065.06           11.15        107.65
    (GEN)                                                                    UT/SG           0.00%+0.05449/L
    ADD INFO: ORG=US*PST=GEN*RNO=001
    LUBRICATING PREPARATIONS (INCLUDING CUTTING-OIL PREPARATIONS, BOLT OR NUT RELEAS
    VOTI=     1076.56      T&I=        0.35

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                               1777.84   *
                                                                                      * DUMPING DUTY                    1234567.89   *
                                                                                      * GST                                4377.60   *
                                                                                      * OTHER CHARGES                        44.10   *
Delivery: ABI GAS & TOOLS                                                             *                                              *
          171 ABBOTSFORD ROAD                                                         * TOTAL AMOUNT PAYABLE ***        1241998.43 ***
          MAYNE QLD AUSTRALIA                                                         ************************************************
                                                                                      * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************

ENTRY PRINT                              X  XXXXXX    ******************************          PAGE 2
Finalized - Paid:work complete           X  X    X    *     AUSTRALIAN CUSTOMS     *
Impediment(s):quarantine                 X  X    X    * ENTRY FOR HOME CONSUMPTION *          ENTRY NO.   PRINT
impedement(Amendment Failed, Details     X  X    X    ******************************
may not match ICS.)                      X  XXXXXX
Last Msg: LODGE(25DEC05 11:30UTC)                       PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:
OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF23
                                         BOX NO  :
O/REF   : PRO040263                      A/REF   : B00148206/1                                FOLIO    :

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                     NO.

004 39072000     37   505  US            6179.74 KG        17384.86       17384.86          521.54       1791.21    TC 9604601
    (GEN)                                                                    UT/SG           3.00%
    ADD INFO: ORG=US*PST=GEN*RNO=001
    OTHER POLYETHERS EXCL POLYETHYLENE OXIDE,PROPYLENE OXIDE BASED POLYOLS
    VOTI=    17912.12      T&I=        5.72

005 29221900     40        US             740.26 KG         1578.21     1114158.79         1234.00          0.00
    (GEN)                                                                    UT/TV+ADJ       0.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ADJ=1546487USD*AMB=COPQTVD*CL2=12345678*DCX=AD*DMP=1234567.89*DRE=1.2455*DSN=123456*DTY=1234*DXP=1234753AUD*DXT=C*ELA=A
    AA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*FOD=051013*GSTE=404*ISC=1234*LCP=1213.00
    *LCTE=404*LCTI=Y*LCTQ=Y*MD2=123456*MLPI=Y*ODF=13213*ORG=US*PRI=TC:12345678*PST=GEN*PUP=Y*QT2=740.26*RNO=001*SCN=SN1234567*STD=123
    1*TAN=1233*TC2=8840003*TCI=AD:12345678*TFQ=1312*TI2=AD:13213131*TR2=131*TRN=131*UQ2=KG*VAN=1234*VID=ADFFSDAFASDFASDADFSAFSDFSDFSD
    FSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFAS
    DFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*WET=1313*WETE=404*WETQ=Y*WMC=13213213256
    N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)
    VOTI=  2350327.59      T&I=      366.91      DMP=  1234567.89

006 29221900     41        US            1469.64 KG         1694.35        1694.35            0.00        169.49
    (GEN)                                                                    UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*PST=GEN*RNO=001
    N,N-DIETHYLAMINOETHANOL (CAS 100-37-8) VERY LONG DESCRIPTION LINE THAT SHOULD MAKE THE SYSTEM PRINT AT LEAST TWO LINES 1234567890
    1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 12345678
    90 1234567890 1234567890 1234567890 1234567890
    VOTI=     1694.91      T&I=        0.56

007 34021300     53        US            1741.79 KG         2326.86        2326.86            0.00        232.76
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=     2327.63      T&I=        0.77

008 34021300     53        US             870.89 KG         1163.43        1163.43            0.00        116.38
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=     1163.81      T&I=        0.38

009 34021300     53        US             213.18 KG          458.41         458.41            0.00         45.85
    (US,US,PS)                                                               UT/TV            FREE
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*POC=US*PRT=PS*PST=US*RNO=001
    NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM
    VOTI=      458.56      T&I=        0.15

TOTAL NUMBER OF PACKAGES:         0   (ZERO)

                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print Landscape", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2013, 2, 26)]
		public void TestCombinedN1020WithEstimates()
		{
			Env.Registry.BrokerageID = "00079C";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("LNT", new ZDecimal(60316), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(2013, 7, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("LNT", new ZDecimal(59133), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(1970, 7, 1), new ZDateTime(2013, 6, 30, 23, 59, 59));
			helper.CreateTaxOrFee("LCT", new ZDecimal(0.33), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(1970, 7, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("LFT", new ZDecimal(75526), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(1970, 7, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var importer = OrgHeader.LoadFromCode(Factory, "ABIGAS");
			var supplier = OrgHeader.LoadFromCode(Factory, "ABABEU");
			var warehouseOrg = Factory.New<OrgHeader>();
			warehouseOrg.OH_FullName = "WAREHOUSE ORGANISATION";
			warehouseOrg.Addresses[0].LocalControlledPremisesID = "1399K";
			importer.PrimaryRegistrationNumber.Number = "90093519530";
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "1234567H");
			importer.CustomsClientID = "55006646046";
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "1234567V");
			supplier.CustomsClientID = "12090995252";
			var testDec = Factory.New<JobDeclaration>();
			testDec.DisableDefaultPackingInformation = true;
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.AIR;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_DateAtFinalDestination = new ZDateTime(2013, 2, 26);
			testDec.JE_DateAtOrigin = new ZDateTime(2013, 2, 25);
			testDec.JE_DateOfArrival = new ZDateTime(2013, 2, 26);
			testDec.JE_DateOfFirstArrival = new ZDateTime(2013, 2, 26);
			testDec.JE_DeclarationReference = "B00148206";
			testDec.JE_AgentsReference = "N10/20 COMBINDED TES";
			testDec.JE_EntrySubmittedDate = new ZDateTime(2013, 2, 26);
			testDec.JE_ExportDate = new ZDateTime(2013, 2, 25);
			testDec.JE_GB = GlbBranch.CurrentBranch.PK;
			testDec.JE_GoodsDescription = "N10/20 COMBINDED TEST";
			testDec.JE_MasterBill = "08111111111";
			testDec.JE_HouseBill = "321632";
			testDec.JE_MergeBy = "NON";
			testDec.JE_MessageSubType = "FRM";
			testDec.JE_OH_Importer = importer.PK;
			testDec.JE_OH_Supplier = supplier.PK;
			testDec.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = true;
			testDec.WarehouseDocAddress.E2_OA_Address = warehouseOrg.Addresses[0].PK;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_OwnerRef = "23464236234";
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			testDec.JE_RL_NKFinalDestination = "AUMEL";
			testDec.JE_RL_NKOrigin = "USLAX";
			testDec.JE_RL_NKPortOfArrival = "AUSYD";
			testDec.JE_RL_NKPortOfFirstArrival = "AUSYD";
			testDec.JE_RL_NKPortOfLoading = "USLAX";
			testDec.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testDec.JE_SystemCreateTimeUtc = new ZDateTime(2013, 2, 26);
			testDec.JE_SystemCreateUser = "C";
			testDec.JE_SystemLastEditTimeUtc = new ZDateTime(2013, 2, 26);
			testDec.JE_SystemLastEditUser = "C";
			testDec.JE_TotalNoOfPacks = 1000;
			testDec.JE_TotalNoOfPacksPackType = "PKG";
			testDec.JE_TotalVolume = 2;
			testDec.JE_TotalVolumeUnit = "M3";
			testDec.JE_TotalWeight = 5000m;
			testDec.JE_TotalWeightUnit = "KG";
			testDec.JE_TotalNoOfPieces = 1;
			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			testDec.JE_VoyageFlightNo = "QF1";

			var packs = testDec.Packages.AddNew();
			packs.CW_HouseBill = testDec.Bills[0].CU_BillUniqueCode;
			packs.CW_PackQty = 1000;
			packs.CW_InBondPackQty = 500;
			packs.CW_OuterPacks = 1;
			packs.PackingGroup.CR_HouseContainerNumber = 1;
			packs.CW_MarksAndNos = "PACKING LINE 1 MARKS AND NUMBERS";

			testDec.JobComInvoiceGroupHeaders[0].JZ_GroupInvoice = true;
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 200m, "AUD");
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100m, "AUD");

			var testInvoiceHeader = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testInvoiceHeader.JZ_OH_Supplier = supplier.PK;
			testInvoiceHeader.AddInfo.ZA_ORG = "US";
			testInvoiceHeader.AddInfo.ZA_PST = "GEN";
			testInvoiceHeader.AddInfo.ZA_VALB_Hidden = "TV";
			testInvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testInvoiceHeader.JZ_InvoiceAmount = 260000m;
			testInvoiceHeader.JZ_InvoiceCurrExRate = 1.000000000m;
			testInvoiceHeader.JZ_InvoiceDate = new ZDateTime(2013, 2, 21);
			testInvoiceHeader.JZ_InvoiceNumber = "111321P";
			testInvoiceHeader.JZ_PaymentExRate = 1.000000000m;
			testInvoiceHeader.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;
			testInvoiceHeader.JZ_WeightUQ = "KG";
			testInvoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 2, 26);

			var testLine1 = testInvoiceHeader.JobComInvoiceLines.AddNew();
			testLine1.JI_Tariff = "4409.10.00 01";
			testLine1.JI_CustomsQuantity = 500m;
			testLine1.JI_CustomsUnitQty = "CU";
			testLine1.JI_AddInfo = "RNO=001";
			testLine1.JI_IsPackToBondForLine = true;
			testLine1.JI_Description = "CONIFEROUS";
			testLine1.JI_InvoiceQuantity = 1m;
			testLine1.JI_InvoiceUQ = "NO";
			testLine1.JI_LineNo = (short)1;
			testLine1.JI_LinePrice = 60000m;
			testLine1.JI_Weight = 1000m;
			testLine1.JI_WeightUQ = "KG";

			var testLine2 = testInvoiceHeader.JobComInvoiceLines.AddNew();
			testLine2.JI_Tariff = "4409.10.00 01";
			testLine2.JI_CustomsQuantity = 500m;
			testLine2.JI_CustomsUnitQty = "CU";
			testLine2.JI_AddInfo = "RNO=001";
			testLine2.JI_Description = "CONIFEROUS";
			testLine2.JI_InvoiceQuantity = 1m;
			testLine2.JI_InvoiceUQ = "NO";
			testLine2.JI_LineNo = (short)2;
			testLine2.JI_LinePrice = 60000m;
			testLine2.JI_Weight = 1000m;
			testLine2.JI_WeightUQ = "KG";

			var testLine3 = testInvoiceHeader.JobComInvoiceLines.AddNew();
			testLine3.JI_Tariff = "2204.21.20 71";
			testLine3.JI_CustomsQuantity = 50m;
			testLine3.JI_CustomsUnitQty = "L";
			testLine3.JI_AddInfo = "RNO=001";
			testLine3.JI_IsPackToBondForLine = true;
			testLine3.JI_Description = "GRAPE WINE AS DEFINED IN ADDITIONAL NOTE 3 TO THIS CHAPTER";
			testLine3.JI_InvoiceQuantity = 1m;
			testLine3.JI_InvoiceUQ = "NO";
			testLine3.JI_LineNo = (short)3;
			testLine3.JI_LinePrice = 10000m;
			testLine3.JI_WeightUQ = "KG";

			var testLine4 = testInvoiceHeader.JobComInvoiceLines.AddNew();
			testLine4.JI_Tariff = "2204.21.20 71";
			testLine4.JI_CustomsQuantity = 50m;
			testLine4.JI_CustomsUnitQty = "L";
			testLine4.JI_AddInfo = "RNO=001";
			testLine4.JI_Description = "GRAPE WINE AS DEFINED IN ADDITIONAL NOTE 3 TO THIS CHAPTER";
			testLine4.JI_InvoiceQuantity = 1m;
			testLine4.JI_InvoiceUQ = "NO";
			testLine4.JI_LineNo = (short)4;
			testLine4.JI_LinePrice = 10000m;
			testLine4.JI_WeightUQ = "KG";

			var testLine5 = testInvoiceHeader.JobComInvoiceLines.AddNew();
			testLine5.JI_Tariff = "8703.24.11 11";
			testLine5.JI_CustomsQuantity = 1m;
			testLine5.JI_CustomsUnitQty = "NO";
			testLine5.JI_AddInfo = "RNO=001";
			testLine5.JI_IsPackToBondForLine = true;
			testLine5.JI_Description = "LESS THAN 5 YEARS OF AGE";
			testLine5.JI_InvoiceQuantity = 1m;
			testLine5.JI_InvoiceUQ = "NO";
			testLine5.JI_LineNo = (short)5;
			testLine5.JI_LinePrice = 60000m;
			testLine5.JI_WeightUQ = "KG";

			var testLine6 = testInvoiceHeader.JobComInvoiceLines.AddNew();
			testLine6.JI_Tariff = "8703.24.11 11";
			testLine6.JI_CustomsQuantity = 1m;
			testLine6.JI_CustomsUnitQty = "NO";
			testLine6.JI_AddInfo = "LCTI=Y*RNO=001";
			testLine6.JI_Description = "LESS THAN 5 YEARS OF AGE";
			testLine6.JI_InvoiceQuantity = 1m;
			testLine6.JI_InvoiceUQ = "NO";
			testLine6.JI_LineNo = (short)6;
			testLine6.JI_LinePrice = 60000m;
			testLine6.JI_WeightUQ = "KG";

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testDec.DoMerge();

			var entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], true);
			ZString expectedResult = @"ENTRY PRINT - ESTIMATE         X  XXXXXX    XXXXX  XXXXXX ******************************      PAGE 1
Not Sent                       X  X    X        X  X    X *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                 X  X    X  / XXXXX  X    X * ENTRY FOR HOME CONSUMPTION/*      ENTRY NO.   PRINT
                               X  X    X    X      X    X *    ENTRY FOR WAREHOUSING   *
                               X  XXXXXX    XXXXX  XXXXXX ******************************
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:

OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF1
                                         BOX NO  :
O/REF   : 23464236234                    A/REF   : N10/20 COMBINDED TES                       FOLIO    :

WAREHOUSE: WAREHOUSE ORGANISATION        FOB  (1) :      260000.00 = $A        260000         LOAD  PT : LOS ANGELES
           (1399K)                       CIF  (1) :      260300.00 = $A     260300.00         FIRST PT : SYDNEY             26FEB13
                                         GRWT (KG):        5000.00 = KG       5000.00         DSCH  PT : SYDNEY             26FEB13
                                         T & I    :         300.00 = $A        300.00         DEST  PT : MELBOURNE

                                                                                              TOTAL CUSTOMS VALUE : $A 260000.00
ITERMS  : FOB

VALUATION DATE : 26FEB13                 ITOT (1) : 260000.00 = $A     260000.00
                                         OSEA (1) :    200.00 = $A        200.00              FACTOR              : 1.00000000
CRNCYS 1 AUD @ 1.0000                    ONS  (1) :    100.00 = $A        100.00
                                                                                              CALCULATION DATE    : 26FEB13

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                  WAREHOUSE UV

001 44091000     01        US                500 CU        60000.00       60000.00         3000.00       6306.92    *ESTIMATE*
    (GEN)                                                                    UT/TV           5.00%                      120.0000
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*PST=GEN*RNO=001
    CONIFEROUS
    VOTI=    63069.23      T&I=       69.23      T&I UV=        0.13

002 44091000     01        US                500 CU        60000.00       60000.00         3000.00       6306.92
    (GEN)                                                                    UT/TV           5.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*PST=GEN*RNO=001
    CONIFEROUS
    VOTI=    63069.23      T&I=       69.23

003 22042120     71        US                 50 L         10000.00       10000.00          500.00       1355.98    *ESTIMATE*
    (GEN)                                                                    UT/TV           5.00%                      200.0000
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*PST=GEN*RNO=001
    GRAPE WINE AS DEFINED IN ADDITIONAL NOTE 3 TO THIS CHAPTER
    VOTI=    10511.54      T&I=       11.54      T&I UV=        0.23      WET=     3048.34

004 22042120     71        US                 50 L         10000.00       10000.00          500.00       1355.98
    (GEN)                                                                    UT/TV           5.00%
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*PST=GEN*RNO=001
    GRAPE WINE AS DEFINED IN ADDITIONAL NOTE 3 TO THIS CHAPTER
    VOTI=    10511.54      T&I=       11.54      WET=     3048.34

005 87032411     11        US                  1 NO        60000.00       60000.00        18000.00       7806.92    *ESTIMATE*
    (GEN)                                                                    UT/TV           10.00%+12000.00000/NO    60000.0000
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*PST=GEN*RNO=001
    LESS THAN 5 YEARS OF AGE
    VOTI=    78069.23      T&I=       69.23      T&I UV=       69.23

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                              21500.00   *
                                                                                      * GST                               15469.82   *
                                                                                      * WET                                3048.34   *
                                                                                      * LCT                                8022.94   *
                                                                                      * OTHER CHARGES                       808.20   *
Delivery: ABI GAS & TOOLS                                                             *                                              *
          171 ABBOTSFORD ROAD                                                         * TOTAL AMOUNT PAYABLE ***          48849.30 ***
          MAYNE QLD AUSTRALIA                                                         ************************************************
                                                                                      * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************

ENTRY PRINT - ESTIMATE         X  XXXXXX    XXXXX  XXXXXX ******************************      PAGE 2
Not Sent                       X  X    X        X  X    X *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                 X  X    X  / XXXXX  X    X * ENTRY FOR HOME CONSUMPTION/*      ENTRY NO.   PRINT
                               X  X    X    X      X    X *    ENTRY FOR WAREHOUSING   *
                               X  XXXXXX    XXXXX  XXXXXX ******************************
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:

OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF1
                                         BOX NO  :
O/REF   : 23464236234                    A/REF   : N10/20 COMBINDED TES                       FOLIO    :

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                  WAREHOUSE UV

006 87032411     11        US                  1 NO        60000.00       60000.00        18000.00       7806.92
    (GEN)                                                                    UT/TV           10.00%+12000.00000/NO
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: LCTI=Y*ORG=US*PST=GEN*RNO=001
    LESS THAN 5 YEARS OF AGE
    VOTI=    78069.23      T&I=       69.23      LCT=     8022.94

TOTAL NUMBER OF PACKAGES:      1000   (ONE ZERO ZERO ZERO)
TOTAL NUMBER OF WAREHOUSE PACKAGES:       500   (FIVE ZERO ZERO)

NO. OF PACKAGES / MASTER BILL / HOUSE BILL

1000 PKG / 08111111111 /
    PACKAGE MARKS: PACKING LINE 1 MARKS AND NUMBERS

                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print with Estimates", expectedResult, entryPrint.EntryPrint);
		}

		[TestDate(2013, 2, 26)]
		public void TestDeferredN1020WithFlatDutyAndEstimates()
		{
			Env.Registry.BrokerageID = "00079C";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";
			var importer = OrgHeader.LoadFromCode(Factory, "ABIGAS");
			var supplier = OrgHeader.LoadFromCode(Factory, "ABABEU");
			var warehouseOrg = Factory.New<OrgHeader>();
			warehouseOrg.OH_FullName = "WAREHOUSE ORGANISATION";
			warehouseOrg.Addresses[0].LocalControlledPremisesID = "1399K";
			importer.PrimaryRegistrationNumber.Number = "90093519530";
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "1234567H");
			importer.CustomsClientID = "55006646046";
			supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "1234567V");
			supplier.CustomsClientID = "12090995252";
			var testDec = Factory.New<JobDeclaration>();
			testDec.DisableDefaultPackingInformation = true;
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.AIR;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_DateAtFinalDestination = new ZDateTime(2013, 2, 26);
			testDec.JE_DateAtOrigin = new ZDateTime(2013, 2, 25);
			testDec.JE_DateOfArrival = new ZDateTime(2013, 2, 26);
			testDec.JE_DateOfFirstArrival = new ZDateTime(2013, 2, 26);
			testDec.JE_DeclarationReference = "B00148206";
			testDec.JE_AgentsReference = "N10/20 COMBINDED TES";
			testDec.JE_EntrySubmittedDate = new ZDateTime(2013, 2, 26);
			testDec.JE_ExportDate = new ZDateTime(2013, 2, 25);
			testDec.JE_GB = GlbBranch.CurrentBranch.PK;
			testDec.JE_GoodsDescription = "N10/20 COMBINDED TEST";
			testDec.JE_MasterBill = "08111111111";
			testDec.JE_HouseBill = "321632";
			testDec.JE_MergeBy = "NON";
			testDec.JE_MessageSubType = "FRM";
			testDec.JE_OH_Importer = importer.PK;
			testDec.JE_OH_Supplier = supplier.PK;
			testDec.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = true;
			testDec.Consignee.MiscServ.OM_IMIsGSTDeferred = ZBool.True;
			testDec.WarehouseDocAddress.E2_OA_Address = warehouseOrg.Addresses[0].PK;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_OwnerRef = "23464236234";
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			testDec.JE_RL_NKFinalDestination = "AUMEL";
			testDec.JE_RL_NKOrigin = "USLAX";
			testDec.JE_RL_NKPortOfArrival = "AUSYD";
			testDec.JE_RL_NKPortOfFirstArrival = "AUSYD";
			testDec.JE_RL_NKPortOfLoading = "USLAX";
			testDec.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testDec.JE_SystemCreateTimeUtc = new ZDateTime(2013, 2, 26);
			testDec.JE_SystemCreateUser = "C";
			testDec.JE_SystemLastEditTimeUtc = new ZDateTime(2013, 2, 26);
			testDec.JE_SystemLastEditUser = "C";
			testDec.JE_TotalNoOfPacks = 1000;
			testDec.JE_TotalNoOfPacksPackType = "PKG";
			testDec.JE_TotalVolume = 2;
			testDec.JE_TotalVolumeUnit = "M3";
			testDec.JE_TotalWeight = 5000m;
			testDec.JE_TotalWeightUnit = "KG";
			testDec.JE_TotalNoOfPieces = 1;
			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			testDec.JE_VoyageFlightNo = "QF1";

			var packs = testDec.Packages.AddNew();
			packs.CW_HouseBill = testDec.Bills[0].CU_BillUniqueCode;
			packs.CW_PackQty = 1000;
			packs.CW_InBondPackQty = 500;
			packs.CW_OuterPacks = 1;
			packs.PackingGroup.CR_HouseContainerNumber = 1;
			packs.CW_MarksAndNos = "PACKING LINE 1 MARKS AND NUMBERS";

			testDec.JobComInvoiceGroupHeaders[0].JZ_GroupInvoice = true;
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 200m, "AUD");
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100m, "AUD");

			var testInvoiceHeader = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testInvoiceHeader.JZ_OH_Supplier = supplier.PK;
			testInvoiceHeader.AddInfo.ZA_ORG = "US";
			testInvoiceHeader.AddInfo.ZA_PST = "GEN";
			testInvoiceHeader.AddInfo.ZA_VALB_Hidden = "TV";
			testInvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testInvoiceHeader.JZ_InvoiceAmount = 260000m;
			testInvoiceHeader.JZ_InvoiceCurrExRate = 1.000000000m;
			testInvoiceHeader.JZ_InvoiceDate = new ZDateTime(2013, 2, 21);
			testInvoiceHeader.JZ_InvoiceNumber = "111321P";
			testInvoiceHeader.JZ_PaymentExRate = 1.000000000m;
			testInvoiceHeader.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;
			testInvoiceHeader.JZ_WeightUQ = "KG";
			testInvoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 2, 26);

			var charge = testInvoiceHeader.Charges.AddNew();
			charge.J7_ChargeType = AUChargeCodeList.Codes.OverseasFreight;
			charge.J7_Amount = 200M;
			charge.J7_RX_NKCurrency = AUDCurrency.RX_Code;
			charge.J7_IsIncludedInITOT = false;

			charge = testInvoiceHeader.Charges.AddNew();
			charge.J7_ChargeType = AUChargeCodeList.Codes.OverseasInsurance;
			charge.J7_Amount = 100M;
			charge.J7_RX_NKCurrency = AUDCurrency.RX_Code;
			charge.J7_IsIncludedInITOT = false;

			var testLine1 = testInvoiceHeader.JobComInvoiceLines.AddNew();
			testLine1.JI_Tariff = "3817.00.19 36";
			testLine1.JI_CustomsQuantity = 50m;
			testLine1.JI_CustomsUnitQty = "L";
			testLine1.JI_AddInfo = "RNO=001";
			testLine1.JI_IsPackToBondForLine = true;
			testLine1.JI_Description = "FLAT DUTY RATE ON THIS LINE";
			testLine1.JI_InvoiceQuantity = 1m;
			testLine1.JI_InvoiceUQ = "NO";
			testLine1.JI_LineNo = (short)1;
			testLine1.JI_LinePrice = 10000m;
			testLine1.JI_Weight = 100m;
			testLine1.JI_WeightUQ = "KG";

			var testLine2 = testInvoiceHeader.JobComInvoiceLines.AddNew();
			testLine2.JI_Tariff = "3817.00.19 36";
			testLine2.JI_CustomsQuantity = 50m;
			testLine2.JI_CustomsUnitQty = "L";
			testLine2.JI_AddInfo = "RNO=001";
			testLine2.JI_Description = "FLAT DUTY RATE ON THIS LINE";
			testLine2.JI_InvoiceQuantity = 1m;
			testLine2.JI_InvoiceUQ = "NO";
			testLine2.JI_LineNo = (short)2;
			testLine2.JI_LinePrice = 10000m;
			testLine2.JI_Weight = 100m;
			testLine2.JI_WeightUQ = "KG";

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testDec.DoMerge();

			var entryPrint = new AUCustomsEntryPrintForTest(testDec.CustomsEntryHeaders[0], true);
			ZString expectedResult = @"ENTRY PRINT - ESTIMATE         X  XXXXXX    XXXXX  XXXXXX ******************************      PAGE 1
Not Sent                       X  X    X        X  X    X *     AUSTRALIAN CUSTOMS     *
Last Msg: NONE                 X  X    X  / XXXXX  X    X * ENTRY FOR HOME CONSUMPTION/*      ENTRY NO.   PRINT
                               X  X    X    X      X    X *    ENTRY FOR WAREHOUSING   *
                               X  XXXXXX    XXXXX  XXXXXX ******************************
                                                        PREPARED 17FEB05 17:47 HRS
                                                                                                         COPYNUM:

OWNER   : ABI GAS & TOOLS                AGENCY  : Eagle Datamation International             MODE     : AIR
(55006646046) (90093519530)                        (00079C)
                                                                          (A2M139)            AIRCR    : QF1
                                         BOX NO  :
O/REF   : 23464236234                    A/REF   : N10/20 COMBINDED TES                       FOLIO    :

WAREHOUSE: WAREHOUSE ORGANISATION        FOB  (1) :       20000.00 = $A         20000         LOAD  PT : LOS ANGELES
           (1399K)                       CIF  (1) :       20300.00 = $A      20300.00         FIRST PT : SYDNEY             26FEB13
                                         GRWT (KG):        5000.00 = KG       5000.00         DSCH  PT : SYDNEY             26FEB13
                                         T & I    :         300.00 = $A        300.00         DEST  PT : MELBOURNE

                                                                                              TOTAL CUSTOMS VALUE : $A 20000.00
ITERMS  : FOB

VALUATION DATE : 26FEB13                 ITOT (1) :  20000.00 = $A      20000.00
                                         OSEA (1) :    200.00 = $A        200.00              FACTOR              : 1.00000000
CRNCYS 1 AUD @ 1.0000                    ONS  (1) :    100.00 = $A        100.00
                                                                                              CALCULATION DATE    : 26FEB13

LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT
NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                  WAREHOUSE UV

001 38170019     36        US                 50 L         10000.00       10000.00          519.07       1066.90    *ESTIMATE*
    (GEN)                                                                    UT/TV           5.00%+0.38143/L            200.0000
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*PST=GEN*RNO=001
    FLAT DUTY RATE ON THIS LINE
    VOTI=    10669.07      T&I=      150.00      T&I UV=        3.00

002 38170019     36        US                 50 L         10000.00       10000.00          519.07       1066.90
    (GEN)                                                                    UT/TV           5.00%+0.38143/L
    SUPPLIER: ABA BEUL (12090995252)
    ADD INFO: ORG=US*PST=GEN*RNO=001
    FLAT DUTY RATE ON THIS LINE
    VOTI=    10669.07      T&I=      150.00

TOTAL NUMBER OF PACKAGES:      1000   (ONE ZERO ZERO ZERO)
TOTAL NUMBER OF WAREHOUSE PACKAGES:       500   (FIVE ZERO ZERO)

NO. OF PACKAGES / MASTER BILL / HOUSE BILL

1000 PKG / 08111111111 /
    PACKAGE MARKS: PACKING LINE 1 MARKS AND NUMBERS

                                                                                      **************  E F T    O N L Y  **************
                                                                                      *                                              *
                                                                                      *                                              *
                                                                                      * DUTY                                519.07   *
TOTAL DEFERRED GST  FOR ENTRY  =   $1066.90                                           * GST                                   0.00   *
                                                                                      * OTHER CHARGES                        88.20   *
Delivery: ABI GAS & TOOLS                                                             *                                              *
          171 ABBOTSFORD ROAD                                                         * TOTAL AMOUNT PAYABLE ***            607.27 ***
          MAYNE QLD AUSTRALIA                                                         ************************************************
                                                                                      * OFFICIAL USE ONLY                            *
                                                                                      *                                              *
I ......................................................                              * ................................     /  /    *
BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.             * SIGNATURE OF AUTHORISING OFFICER     DATE    *
(SIGNED)    /  /    ........................ AGENT/OWNER                              ************************************************
                                                                                      * WARRANTED AND RECEIPTED:                     *
ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com     *                                              *
The Entry print will reflect the message details, once a successful message           *                                              *
response has been received from Government Customs.                                   *                                              *
                                                                                      ************************************************
                                                ***  END OF ENTRY  ***";
			AssertMultilineASCIIEquals("Entry Print with Estimates", expectedResult, entryPrint.EntryPrint);
		}

		protected override void SetUp()
		{
			base.SetUp();
			useCustomsReferenceDataRegItem = AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tax1 = helper.CreateTaxOrFee("DAN", 30.10M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			tax1.ZZF_Threshold = 10000m;
			var tax2 = helper.CreateTaxOrFee("DNW", 23.20M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			var tax3 = helper.CreateTaxOrFee("DPN", 30.10M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			tax3.ZZF_Threshold = 10000m;
			var tax4 = helper.CreateTaxOrFee("DSN", 49.50M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			tax4.ZZF_Threshold = 10000m;

			var tax5 = helper.CreateTaxOrFee("DAH", 152.0M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FAT", new ZDateTime(2016, 1, 1), new ZDateTime(2079, 6, 6));
			tax5.ZZF_Threshold = 10000m;
			var tax6 = helper.CreateTaxOrFee("DSH", 152.0M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FAT", new ZDateTime(2016, 1, 1), new ZDateTime(2079, 6, 6));
			tax6.ZZF_Threshold = 10000m;
			var tax7 = helper.CreateTaxOrFee("DPH", 152.0M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FAT", new ZDateTime(2016, 1, 1), new ZDateTime(2079, 6, 6));
			tax7.ZZF_Threshold = 10000m;
			var tax8 = helper.CreateTaxOrFee("DAH", 122.10M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FAT", new ZDateTime(2014, 1, 1), new ZDateTime(2015, 12, 31, 23, 59, 59));
			tax8.ZZF_Threshold = 10000m;
			var tax9 = helper.CreateTaxOrFee("DSH", 152.60M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FAT", new ZDateTime(2014, 1, 1), new ZDateTime(2015, 12, 31, 23, 59, 59));
			tax9.ZZF_Threshold = 10000m;
			var tax10 = helper.CreateTaxOrFee("DPH", 122.10M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FAT", new ZDateTime(2014, 1, 1), new ZDateTime(2015, 12, 31, 23, 59, 59));
			tax10.ZZF_Threshold = 10000m;

			helper.CreateTaxOrFee("Q1A", 14M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2A", 14M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q1S", 7M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2S", 7M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));

			helper.CreateTaxOrFee("Q1F", 15M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2F", 15M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q1X", 15M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2X", 15M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q1L", 3.75M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2L", 3.75M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));

			Factory.Save();

			TaxOrFeeTestHelper.SetUp();
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "2922190040", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "2922190041", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff2, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");
			var tariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "3402130053", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff3, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");
			var tariff4 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "3402900019", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff4, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "L");
			var tariff5 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "3403999037", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff5, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "L");
			var tariff6 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "3907200037", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff6, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "L");
			var tariff7 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "4409100001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff7, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "CU");
			var tariff8 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "4410900025", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff8, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "CU");
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			useCustomsReferenceDataRegItem?.Dispose();
		}

		IDisposable useCustomsReferenceDataRegItem;

		void SaveExchangeRate(ZString currency, ZDateTime date, ZDecimal sellRate)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			RefExchangeRate exRate = RefExchangeRate.New(factory);
			exRate.RE_ExpiryDate = date.Add(RefExchangeRate.EndOfDay);
			exRate.RE_ExRateType = "CUS";
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exRate.RE_RX_NKExCurrency = currency;
			exRate.RE_SellRate = sellRate;
			exRate.RE_StartDate = date;
			factory.Save();
		}

		JobDeclaration SetupCMRDataForTwoPageEntry()
		{
			Env.Registry.BrokerageID = "00079C";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";
			OrgHeader importer = OrgHeader.LoadFromCode(Factory, "ABIGAS");
			OrgHeader supplier1 = OrgHeader.LoadFromCode(Factory, "ABABEU");
			OrgHeader supplier2 = OrgHeader.LoadFromCode(Factory, "AFSSHU");

			importer.PrimaryRegistrationNumber.Number = "90093519530";
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "1234567H");
			importer.CustomsClientID = "55006646046";
			supplier1.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "1234567V");
			supplier1.CustomsClientID = "12090995252";
			supplier2.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "2345678P");
			supplier2.CustomsClientID = "";
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.DisableDefaultPackingInformation = true;
			testDec.JE_AddInfo = "ForcePrimeEnclosureIfMultipleEntries_Hidden=Y*NumberOfEntryPrints_Hidden=2*EFTReceiptPrinter_Hidden=06976*PrinterNumber_Hidden=06976*ClearanceAdvicePrinter_Hidden=06976";
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.AIR;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_DateAtFinalDestination = new ZDateTime(2005, 3, 3);
			testDec.JE_DateAtOrigin = new ZDateTime(2005, 3, 2);
			testDec.JE_DateOfArrival = new ZDateTime(2005, 3, 3);
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 3, 3);
			testDec.JE_DeclarationReference = "B00148206";
			testDec.JE_EntrySubmittedDate = new ZDateTime(2005, 4, 1);
			testDec.JE_ExportDate = new ZDateTime(2005, 7, 7);
			testDec.JE_ExportGoodsType = "OT";
			testDec.JE_GB = GlbBranch.CurrentBranch.PK;
			testDec.JE_GoodsDescription = "PARTS FOR CRANE";
			testDec.JE_LandedCostByWeight = 100;
			testDec.JE_MasterBill = "08165154784";
			testDec.JE_MergeBy = "NON";
			testDec.JE_MessageSubType = "FRM";
			testDec.JE_OH_Importer = importer.PK;
			testDec.JE_OH_Supplier = supplier1.PK;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_OwnerRef = "PRO040263";
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			testDec.JE_RL_NKFinalDestination = "AUSYD";
			testDec.JE_RL_NKOrigin = "USKCK";
			testDec.JE_RL_NKPortOfArrival = "AUSYD";
			testDec.JE_RL_NKPortOfFirstArrival = "AUSYD";
			testDec.JE_RL_NKPortOfLoading = "USKCK";
			testDec.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testDec.JE_SystemCreateTimeUtc = new ZDateTime(2005, 4, 1);
			testDec.JE_SystemCreateUser = "C";
			testDec.JE_SystemLastEditTimeUtc = new ZDateTime(2005, 4, 1);
			testDec.JE_SystemLastEditUser = "C";
			testDec.JE_TotalNoOfPacks = 1;
			testDec.JE_TotalNoOfPacksPackType = "PCE";
			testDec.JE_TotalVolumeUnit = "M3";
			testDec.JE_TotalWeight = 13.000m;
			testDec.JE_TotalWeightUnit = "KG";
			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			testDec.JE_VoyageFlightNo = "QF23";

			testDec.JobComInvoiceGroupHeaders[0].JZ_GroupInvoice = true;
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 350m, "AUD");
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 31.13m, "AUD");

			JobComInvoiceHeader testHeader1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader1.JZ_OH_Supplier = supplier1.PK;
			testHeader1.AddInfo.ZA_ORG = "US";
			testHeader1.AddInfo.ZA_PST = "GEN";
			testHeader1.AddInfo.ZA_VALB_Hidden = "TV";
			testHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testHeader1.JZ_InvoiceAmount = 7221.26m;
			testHeader1.JZ_InvoiceCurrExRate = 1.000000000m;
			testHeader1.JZ_InvoiceDate = new ZDateTime(2004, 10, 11);
			testHeader1.JZ_InvoiceNumber = "4X00092";
			testHeader1.JZ_PaymentExRate = 1.000000000m;
			testHeader1.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;
			testHeader1.JZ_Weight = 10426.00m;
			testHeader1.JZ_WeightUQ = "KG";
			testHeader1.JZ_ValuationDateOverride = new ZDateTime(2005, 3, 2);

			JobComInvoiceHeader testHeader2 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader2.JZ_OH_Supplier = supplier2.PK;
			testHeader2.AddInfo.ZA_ORG = "US";
			testHeader2.AddInfo.ZA_PST = "GEN";
			testHeader2.AddInfo.ZA_VALB_Hidden = "TV";
			testHeader2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testHeader2.JZ_InvoiceAmount = 37575.20m;
			testHeader2.JZ_InvoiceCurrExRate = 1.000000000m;
			testHeader2.JZ_InvoiceDate = new ZDateTime(2004, 10, 11);
			testHeader2.JZ_InvoiceNumber = "30/09/04";
			testHeader2.JZ_PaymentExRate = 1.000000000m;
			testHeader2.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;
			testHeader2.JZ_Weight = 7000.00m;
			testHeader2.JZ_WeightUQ = "KG";
			testHeader2.JZ_ValuationDateOverride = new ZDateTime(2005, 3, 2);

			JobComInvoiceLine testLine1 = testHeader1.JobComInvoiceLines.AddNew();
			testLine1.JI_Tariff = "2922.19.00 40";
			testLine1.JI_CustomsQuantity = 740.2600m;
			testLine1.JI_CustomsUnitQty = "KG";
			testLine1.JI_AddInfo = "WETE=404*DTY=1234.00*GSTE=404*ADJ=1546487USD*WMC=13213213256*TI2=AD:13213131*DCX=AD*ORG=US*TCI=AD:12345678*DMP=1234567.89*STD=1231.00*PUP=Y*TC2=8840003*TRN=131*QT2=131131.00*WET=1313.00*TFQ=1312*VID=ADFFSDAFASDFASDADFSAFSDFSDFSDFSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFASDFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*LCTE=404*FOD=051013*AMB=COPQTVD*TAN=1233*CL2=12345678*PRI=TC:12345678*DXP=1234753*PST=GEN*LCTI=Y*LCTQ=Y*DRE=1.2455*ISC=1234*UQ2=KG*DXT=C*DSN=123456*VAN=1234*LCP=1213.00*ODF=13213.00*MLPI=Y*MD2=123456*SCN=SN1234567*TR2=131*ELA=AAA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*RNO=001*WETQ=Y";
			testLine1.JI_Description = "N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)";
			testLine1.JI_InvoiceQuantity = 740.26000m;
			testLine1.JI_InvoiceUQ = "KG";
			testLine1.JI_LineNo = (short)1;
			testLine1.JI_LinePrice = 1578.21m;
			testLine1.JI_Weight = 740.26000m;
			testLine1.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine2 = testHeader1.JobComInvoiceLines.AddNew();
			testLine2.JI_Tariff = "2922.19.00 41";
			testLine2.JI_CustomsQuantity = 1469.64000m;
			testLine2.JI_CustomsUnitQty = "KG";
			testLine2.JI_AddInfo = "PST=GEN*RNO=001";
			testLine2.JI_Description = "N,N-DIETHYLAMINOETHANOL (CAS 100-37-8) VERY LONG DESCRIPTION LINE THAT SHOULD MAKE THE SYSTEM PRINT AT LEAST TWO LINES 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890";
			testLine2.JI_InvoiceQuantity = 1469.64000m;
			testLine2.JI_InvoiceUQ = "KG";
			testLine2.JI_LineNo = (short)2;
			testLine2.JI_LinePrice = 1694.35m;
			testLine2.JI_Weight = 1469.64000m;
			testLine2.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine3 = testHeader1.JobComInvoiceLines.AddNew();
			testLine3.JI_CustomsQuantity = 1741.79000m;
			testLine3.JI_CustomsUnitQty = "KG";
			testLine3.JI_AddInfo = "PRT=PS*PST=US*POC=US*RNO=001";
			testLine3.JI_Description = "NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM";
			testLine3.JI_InvoiceQuantity = 1741.79000m;
			testLine3.JI_InvoiceUQ = "KG";
			testLine3.JI_LineNo = (short)3;
			testLine3.JI_LinePrice = 2326.86m;
			testLine3.JI_Tariff = "3402.13.00 53";
			testLine3.JI_Weight = 1741.79000m;
			testLine3.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine4 = testHeader1.JobComInvoiceLines.AddNew();
			testLine4.JI_Tariff = "3402.13.00 53";
			testLine4.JI_CustomsQuantity = 870.89000m;
			testLine4.JI_CustomsUnitQty = "KG";
			testLine4.JI_AddInfo = "PRT=PS*PST=US*POC=US*RNO=001";
			testLine4.JI_Description = "NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM";
			testLine4.JI_InvoiceQuantity = 870.89000m;
			testLine4.JI_InvoiceUQ = "KG";
			testLine4.JI_LineNo = (short)4;
			testLine4.JI_LinePrice = 1163.43m;
			testLine4.JI_Weight = 870.89000m;
			testLine4.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine5 = testHeader1.JobComInvoiceLines.AddNew();
			testLine5.JI_Tariff = "3402.13.00 53";
			testLine5.JI_CustomsQuantity = 213.18000m;
			testLine5.JI_CustomsUnitQty = "KG";
			testLine5.JI_AddInfo = "PRT=PS*PST=US*POC=US*RNO=001";
			testLine5.JI_Description = "NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM";
			testLine5.JI_InvoiceQuantity = 213.18000m;
			testLine5.JI_InvoiceUQ = "KG";
			testLine5.JI_LineNo = (short)5;
			testLine5.JI_LinePrice = 458.41m;
			testLine5.JI_Weight = 213.18000m;
			testLine5.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine6 = testHeader2.JobComInvoiceLines.AddNew();
			testLine6.JI_Tariff = "3402.90.00 19";
			testLine6.JI_CustomsQuantity = 4623.88000m;
			testLine6.JI_CustomsUnitQty = "L";
			testLine6.JI_AddInfo = "PRT=PS*PST=US*POC=US*RNO=001*VALB_Hidden=SG";
			testLine6.JI_Description = "NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM";
			testLine6.JI_InvoiceQuantity = 4623.88000m;
			testLine6.JI_InvoiceUQ = "L";
			testLine6.JI_LineNo = (short)1;
			testLine6.JI_LinePrice = 18053.13m;
			testLine6.JI_Weight = 4623.88000m;
			testLine6.JI_WeightUQ = "L";

			JobComInvoiceLine testLine7 = testHeader2.JobComInvoiceLines.AddNew();
			testLine7.JI_Tariff = "3403.99.90 37";
			testLine7.JI_CustomsQuantity = 204.69000m;
			testLine7.JI_CustomsUnitQty = "L";
			testLine7.JI_AddInfo = "PST=GEN*RNO=001*VALB_Hidden=SG";
			testLine7.JI_Description = "LUBRICATING PREPARATIONS (INCLUDING CUTTING-OIL PREPARATIONS, BOLT OR NUT RELEAS";
			testLine7.JI_InvoiceQuantity = 204.69000m;
			testLine7.JI_InvoiceUQ = "L";
			testLine7.JI_LineNo = (short)2;
			testLine7.JI_LinePrice = 1072.15m;
			testLine7.JI_Weight = 204.69000m;
			testLine7.JI_WeightUQ = "L";

			JobComInvoiceLine testLine8 = testHeader2.JobComInvoiceLines.AddNew();
			testLine8.JI_Tariff = "3403.99.90 37";
			testLine8.JI_CustomsQuantity = 204.77000m;
			testLine8.JI_CustomsUnitQty = "L";
			testLine8.JI_AddInfo = "PST=GEN*RNO=001*VALB_Hidden=SG";
			testLine8.JI_Description = "LUBRICATING PREPARATIONS (INCLUDING CUTTING-OIL PREPARATIONS, BOLT OR NUT RELEAS";
			testLine8.JI_InvoiceQuantity = 204.77000m;
			testLine8.JI_InvoiceUQ = "L";
			testLine8.JI_LineNo = (short)3;
			testLine8.JI_LinePrice = 1065.06m;
			testLine8.JI_Weight = 204.77000m;
			testLine8.JI_WeightUQ = "L";

			JobComInvoiceLine testLine9 = testHeader2.JobComInvoiceLines.AddNew();
			testLine9.JI_Tariff = "3907.20.00 37";
			testLine9.JI_CustomsQuantity = 6179.74000m;
			testLine9.JI_CustomsUnitQty = "KG";
			testLine9.JI_AddInfo = "PST=GEN*RNO=001*TreatmentCode_Hidden=505*InstrumentType_Hidden=TC*InstrumentCode_Hidden=9604601*VALB_Hidden=SG";
			testLine9.JI_Description = "OTHER POLYETHERS EXCL POLYETHYLENE OXIDE,PROPYLENE OXIDE BASED POLYOLS";
			testLine9.JI_InvoiceQuantity = 6179.74000m;
			testLine9.JI_InvoiceUQ = "KG";
			testLine9.JI_LineNo = (short)4;
			testLine9.JI_LinePrice = 17384.86m;
			testLine9.JI_Weight = 6179.74000m;
			testLine9.JI_WeightUQ = "KG";

			return testDec;
		}

		void SetupPackages()
		{
			packageForContainer1 = testDec.Packages.AddNew();
			packageForContainer1.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			packageForContainer1.CW_PackQty = 400;
			packageForContainer1.CW_OuterPacks = 1;
			packageForContainer1.PackingGroup.CR_HouseContainerNumber = 1;
			packageForContainer1.CW_MarksAndNos = "PACKING LINE 1 MARKS AND NUMBERS" + longNoteValue;

			packageForContainer2 = testDec.Packages.AddNew();
			packageForContainer2.CW_HouseBill = houseBill2.CU_BillUniqueCode;
			packageForContainer2.CW_PackQty = 600;
			packageForContainer2.CW_OuterPacks = 2;
			packageForContainer2.PackingGroup.CR_HouseContainerNumber = 2;
			packageForContainer2.CW_MarksAndNos = "PACKING LINE 2 MARKS AND NUMBERS";
		}

		JobDeclaration SetupCMRDataForTwoPageEntryWithMultipleCurrencies()
		{
			Env.Registry.BrokerageID = "00079C";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "A2M139";
			OrgHeader importer = OrgHeader.LoadFromCode(Factory, "ABIGAS");
			OrgHeader supplier1 = OrgHeader.LoadFromCode(Factory, "ABABEU");
			OrgHeader supplier2 = OrgHeader.LoadFromCode(Factory, "AFSSHU");

			importer.PrimaryRegistrationNumber.Number = "90093519530";
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "1234567H");
			importer.CustomsClientID = "55006646046";
			supplier1.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "1234567V");
			supplier1.CustomsClientID = "12090995252";
			supplier2.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "2345678P");
			supplier2.CustomsClientID = "";
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.DisableDefaultPackingInformation = true;
			testDec.JE_AddInfo = "ForcePrimeEnclosureIfMultipleEntries_Hidden=Y*NumberOfEntryPrints_Hidden=2*EFTReceiptPrinter_Hidden=06976*PrinterNumber_Hidden=06976*ClearanceAdvicePrinter_Hidden=06976";
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.AIR;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_DateAtFinalDestination = new ZDateTime(2005, 3, 3);
			testDec.JE_DateAtOrigin = new ZDateTime(2005, 3, 2);
			testDec.JE_DateOfArrival = new ZDateTime(2005, 3, 3);
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 3, 3);
			testDec.JE_DeclarationReference = "B00148206";
			testDec.JE_EntrySubmittedDate = new ZDateTime(2005, 4, 1);
			testDec.JE_ExportDate = new ZDateTime(2005, 7, 7);
			testDec.JE_ExportGoodsType = "OT";
			testDec.JE_GB = GlbBranch.CurrentBranch.PK;
			testDec.JE_GoodsDescription = "PARTS FOR CRANE";
			testDec.JE_LandedCostByWeight = 100;
			testDec.JE_MasterBill = "08165154784";
			testDec.JE_MergeBy = "NON";
			testDec.JE_MessageSubType = "FRM";
			testDec.JE_OH_Importer = importer.PK;
			testDec.JE_OH_Supplier = supplier1.PK;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_OwnerRef = "PRO040263";
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			testDec.JE_RL_NKFinalDestination = "AUSYD";
			testDec.JE_RL_NKOrigin = "USKCK";
			testDec.JE_RL_NKPortOfArrival = "AUSYD";
			testDec.JE_RL_NKPortOfFirstArrival = "AUSYD";
			testDec.JE_RL_NKPortOfLoading = "USKCK";
			testDec.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testDec.JE_SystemCreateTimeUtc = new ZDateTime(2005, 4, 1);
			testDec.JE_SystemCreateUser = "C";
			testDec.JE_SystemLastEditTimeUtc = new ZDateTime(2005, 4, 1);
			testDec.JE_SystemLastEditUser = "C";
			testDec.JE_TotalNoOfPacks = 1;
			testDec.JE_TotalNoOfPacksPackType = "PCE";
			testDec.JE_TotalVolumeUnit = "M3";
			testDec.JE_TotalWeight = 13.000m;
			testDec.JE_TotalWeightUnit = "KG";
			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			testDec.JE_VoyageFlightNo = "QF23";

			testDec.JobComInvoiceGroupHeaders[0].JZ_GroupInvoice = true;
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 350m, "AUD");
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 31.13m, "AUD");

			JobComInvoiceHeader testHeader1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader1.JZ_OH_Supplier = supplier1.PK;
			testHeader1.AddInfo.ZA_ORG = "US";
			testHeader1.AddInfo.ZA_VALB_Hidden = "TV";
			testHeader1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testHeader1.JZ_InvoiceAmount = 7221.26m;
			testHeader1.JZ_InvoiceCurrExRate = 1.000000000m;
			testHeader1.JZ_InvoiceDate = new ZDateTime(2004, 10, 11);
			testHeader1.JZ_InvoiceNumber = "4X00092";
			testHeader1.JZ_PaymentExRate = 1.000000000m;
			testHeader1.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;
			testHeader1.JZ_Weight = 10426.00m;
			testHeader1.JZ_WeightUQ = "KG";
			testHeader1.JZ_ValuationDateOverride = new ZDateTime(2005, 3, 2);
			testHeader1.AddInfo.ZA_PRT = "PS";
			testHeader1.AddInfo.ZA_PST = "US";
			testHeader1.AddInfo.ZA_POC = "US";

			JobComInvoiceHeader testHeader2 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader2.JZ_OH_Supplier = supplier2.PK;
			testHeader2.AddInfo.ZA_ORG = "US";
			testHeader2.AddInfo.ZA_PST = "GEN";
			testHeader2.AddInfo.ZA_VALB_Hidden = "TV";
			testHeader2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testHeader2.JZ_InvoiceAmount = 37575.20m;
			testHeader2.JZ_InvoiceCurrExRate = 1.000000000m;
			testHeader2.JZ_InvoiceDate = new ZDateTime(2004, 10, 11);
			testHeader2.JZ_InvoiceNumber = "30/09/04";
			testHeader2.JZ_PaymentExRate = 1.000000000m;
			testHeader2.JZ_RX_NKInvoice_Currency = HKDCurrency.RX_Code;
			testHeader2.JZ_Weight = 7000.00m;
			testHeader2.JZ_WeightUQ = "KG";
			testHeader2.JZ_ValuationDateOverride = new ZDateTime(2005, 3, 2);

			JobComInvoiceLine testLine1 = testHeader1.JobComInvoiceLines.AddNew();
			testLine1.JI_Tariff = "2922.19.00 40";
			testLine1.JI_CustomsQuantity = 740.2600m;
			testLine1.JI_CustomsUnitQty = "KG";
			testLine1.JI_AddInfo = "WETE=404*DTY=1234.00*GSTE=404*ADJ=1546487USD*WMC=13213213256*TI2=AD:13213131*DCX=AD*ORG=US*TCI=AD:12345678*DMP=1234567.89*STD=1231.00*PUP=Y*TC2=8840003*TRN=131*QT2=131131.00*WET=1313.00*TFQ=1312*VID=ADFFSDAFASDFASDADFSAFSDFSDFSDFSDFSDAFDSAFSDFSADFDSFSADFSADFSADFSADFSADFSDFASDFSDAFSADFASDFSDAFASDFDSFSAFDSFASDFSDAFSADFSDAFASDFSADFASDFSADFDSFAFSADFASFDDSAFASDFSADFSDAFSADFASFSADFSADFSADFASDFSDAFSDAFASDFSDAFASDF*LCTE=404*FOD=051013*AMB=COPQTVD*TAN=1233*CL2=12345678*PRI=TC:12345678*DXP=1234753*PST=GEN*LCTI=Y*LCTQ=Y*DRE=1.2455*ISC=1234*UQ2=KG*DXT=C*DSN=123456*VAN=1234*LCP=1213.00*ODF=13213.00*MLPI=Y*MD2=123456*SCN=SN1234567*TR2=131*ELA=AAA11111,BBB22222,CCC33333,DDD44444,EEE55555,FFF66666,GGG77777,HHH88888,III99999,JJJ00000*RNO=001*WETQ=Y*AQISPermitIds_Hidden=permit1,permit2";
			testLine1.JI_Description = "N,N-DIMETHYLAMINOETHAN-2-OL (CAS 108-01-0)";
			testLine1.JI_InvoiceQuantity = 740.26000m;
			testLine1.JI_InvoiceUQ = "KG";
			testLine1.JI_LineNo = (short)1;
			testLine1.JI_LinePrice = 1578.21m;
			testLine1.JI_Weight = 740.26000m;
			testLine1.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine2 = testHeader1.JobComInvoiceLines.AddNew();
			testLine2.JI_Tariff = "2922.19.00 41";
			testLine2.JI_CustomsQuantity = 1469.64000m;
			testLine2.JI_CustomsUnitQty = "KG";
			testLine2.JI_AddInfo = "PST=GEN*RNO=001*AQISPermitIds_Hidden=1111111111,2222222222,3333333333,444444444";
			testLine2.JI_Description = "N,N-DIETHYLAMINOETHANOL (CAS 100-37-8) VERY LONG DESCRIPTION LINE THAT SHOULD MAKE THE SYSTEM PRINT AT LEAST TWO LINES 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890";
			testLine2.JI_InvoiceQuantity = 1469.64000m;
			testLine2.JI_InvoiceUQ = "KG";
			testLine2.JI_LineNo = (short)2;
			testLine2.JI_LinePrice = 1694.35m;
			testLine2.JI_Weight = 1469.64000m;
			testLine2.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine3 = testHeader1.JobComInvoiceLines.AddNew();
			testLine3.JI_CustomsQuantity = 1741.79000m;
			testLine3.JI_CustomsUnitQty = "KG";
			testLine3.JI_AddInfo = "RNO=001";
			testLine3.JI_Description = "NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM";
			testLine3.JI_InvoiceQuantity = 1741.79000m;
			testLine3.JI_InvoiceUQ = "KG";
			testLine3.JI_LineNo = (short)3;
			testLine3.JI_LinePrice = 2326.86m;
			testLine3.JI_Tariff = "3402.13.00 53";
			testLine3.JI_Weight = 1741.79000m;
			testLine3.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine4 = testHeader1.JobComInvoiceLines.AddNew();
			testLine4.JI_Tariff = "3402.13.00 53";
			testLine4.JI_CustomsQuantity = 870.89000m;
			testLine4.JI_CustomsUnitQty = "KG";
			testLine4.JI_AddInfo = "PRT=PS*PST=US*POC=US*RNO=001";
			testLine4.JI_Description = "NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM";
			testLine4.JI_InvoiceQuantity = 870.89000m;
			testLine4.JI_InvoiceUQ = "KG";
			testLine4.JI_LineNo = (short)4;
			testLine4.JI_LinePrice = 1163.43m;
			testLine4.JI_Weight = 870.89000m;
			testLine4.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine5 = testHeader1.JobComInvoiceLines.AddNew();
			testLine5.JI_Tariff = "3402.13.00 53";
			testLine5.JI_CustomsQuantity = 213.18000m;
			testLine5.JI_CustomsUnitQty = "KG";
			testLine5.JI_AddInfo = "PRT=PS*PST=US*POC=US*RNO=001";
			testLine5.JI_Description = "NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM";
			testLine5.JI_InvoiceQuantity = 213.18000m;
			testLine5.JI_InvoiceUQ = "KG";
			testLine5.JI_LineNo = (short)5;
			testLine5.JI_LinePrice = 458.41m;
			testLine5.JI_Weight = 213.18000m;
			testLine5.JI_WeightUQ = "KG";

			JobComInvoiceLine testLine6 = testHeader2.JobComInvoiceLines.AddNew();
			testLine6.JI_Tariff = "3402.90.00 19";
			testLine6.JI_CustomsQuantity = 4623.88000m;
			testLine6.JI_CustomsUnitQty = "L";
			testLine6.JI_AddInfo = "PRT=PS*PST=US*POC=US*RNO=001*VALB_Hidden=SG";
			testLine6.JI_Description = "NON-IONIC EXCL GOODS IN LIQUID FORM IN PACKS NOT EXCEEDING 10 L OR IN OTHER FORM";
			testLine6.JI_InvoiceQuantity = 4623.88000m;
			testLine6.JI_InvoiceUQ = "L";
			testLine6.JI_LineNo = (short)1;
			testLine6.JI_LinePrice = 18053.13m;
			testLine6.JI_Weight = 4623.88000m;
			testLine6.JI_WeightUQ = "L";

			JobComInvoiceLine testLine7 = testHeader2.JobComInvoiceLines.AddNew();
			testLine7.JI_Tariff = "3403.99.90 37";
			testLine7.JI_CustomsQuantity = 204.69000m;
			testLine7.JI_CustomsUnitQty = "L";
			testLine7.JI_AddInfo = "PST=GEN*RNO=001*VALB_Hidden=SG";
			testLine7.JI_Description = "LUBRICATING PREPARATIONS (INCLUDING CUTTING-OIL PREPARATIONS, BOLT OR NUT RELEAS";
			testLine7.JI_InvoiceQuantity = 204.69000m;
			testLine7.JI_InvoiceUQ = "L";
			testLine7.JI_LineNo = (short)2;
			testLine7.JI_LinePrice = 1072.15m;
			testLine7.JI_Weight = 204.69000m;
			testLine7.JI_WeightUQ = "L";

			JobComInvoiceLine testLine8 = testHeader2.JobComInvoiceLines.AddNew();
			testLine8.JI_Tariff = "3403.99.90 37";
			testLine8.JI_CustomsQuantity = 204.77000m;
			testLine8.JI_CustomsUnitQty = "L";
			testLine8.JI_AddInfo = "PST=GEN*RNO=001*VALB_Hidden=SG";
			testLine8.JI_Description = "LUBRICATING PREPARATIONS (INCLUDING CUTTING-OIL PREPARATIONS, BOLT OR NUT RELEAS";
			testLine8.JI_InvoiceQuantity = 204.77000m;
			testLine8.JI_InvoiceUQ = "L";
			testLine8.JI_LineNo = (short)3;
			testLine8.JI_LinePrice = 1065.06m;
			testLine8.JI_Weight = 204.77000m;
			testLine8.JI_WeightUQ = "L";

			JobComInvoiceLine testLine9 = testHeader2.JobComInvoiceLines.AddNew();
			testLine9.JI_Tariff = "3907.20.00 37";
			testLine9.JI_CustomsQuantity = 6179.74000m;
			testLine9.JI_CustomsUnitQty = "KG";
			testLine9.JI_AddInfo = "PST=GEN*RNO=001*TreatmentCode_Hidden=505*InstrumentType_Hidden=TC*InstrumentCode_Hidden=9604601*VALB_Hidden=SG";
			testLine9.JI_Description = "OTHER POLYETHERS EXCL POLYETHYLENE OXIDE,PROPYLENE OXIDE BASED POLYOLS";
			testLine9.JI_InvoiceQuantity = 6179.74000m;
			testLine9.JI_InvoiceUQ = "KG";
			testLine9.JI_LineNo = (short)4;
			testLine9.JI_LinePrice = 17384.86m;
			testLine9.JI_Weight = 6179.74000m;
			testLine9.JI_WeightUQ = "KG";

			return testDec;
		}

		sealed class AUCustomsEntryPrintForTest : AUCustomsEntryPrint
		{
			public AUCustomsEntryPrintForTest(CusEntryHeader entryHeader, bool isPortrait)
				: base(entryHeader, isPortrait)
			{
			}

			protected override ZDateTime CurrentDateTime => new ZDateTime(2005, 2, 17, 17, 47, 0);
		}
	}
}
