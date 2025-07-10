using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRIMDRMessage))]
	sealed class CMRIMDRMessageTest : CMRImportDeclarationMessageTest
	{
		public void TestExchangeRates()
		{
			var testDec = JobDeclaration.New(Factory);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var response = Factory.New<CMRIMDRMessage>();
			response.EM_MessageText = TestMessages.IMDRHavingCIFAndFOBValidationError;
			response.EM_LinkedObject = entryHeader;
			AssertNotNull("CUSRES", response.CUSRESForTesting);

			ZString report = response.GetErrorsSection("CLR");
			AssertEquals("It should contain advice on exchange rates", true, report.Contains(CMRIMDRMessage.ExchangeRateMessageAdvice));
		}

		public void TestGetGSTRelatedErrorIfAny()
		{
			var testDec = JobDeclaration.New(Factory);
			var importer = OrgHeader.New(Factory);
			importer.MiscServ.OM_IMIsGSTDeferred = true;
			testDec.JE_OH_Importer = importer.PK;

			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var response = Factory.New<CMRIMDRMessage>();
			response.EM_MessageText = TestMessages.IMDRMessageTextHavingGSTPayable;
			response.EM_LinkedObject = entryHeader;
			AssertNotNull("CUSRES", response.CUSRESForTesting);

			ZString report = response.GetReport();
			AssertEquals("Report should advise users of the discrepancy", true, report.Contains("GST-Deferred Configuration Discrepancy: The message from Customs has GST PAYABLE amounts, not deferred amounts."));
			AssertEquals("Report should advise users of the discrepancy", false, report.Contains("GST-Deferred Configuration Discrepancy: The message from Customs has GST DEFERRED amounts, not GST payable amounts."));

			importer.MiscServ.OM_IMIsGSTDeferred = false;
			response = Factory.New<CMRIMDRMessage>();
			response.EM_MessageText = TestMessages.IMDRMessageTextHavingGSTDeferred;
			response.EM_LinkedObject = entryHeader;
			AssertNotNull("CUSRES", response.CUSRESForTesting);

			report = response.GetReport();
			AssertEquals("Report should advise users of the discrepancy", false, report.Contains("GST-Deferred Configuration Discrepancy: The message from Customs has GST PAYABLE amounts, not deferred amounts."));
			AssertEquals("Report should advise users of the discrepancy", true, report.Contains("GST-Deferred Configuration Discrepancy: The message from Customs has GST DEFERRED amounts, not GST payable amounts."));
		}

		public void TestIOutstandingPaymentInfoProvider()
		{
			var message = Factory.New<CMRIMDRMessage>();
			message.EM_MessageText = TestMessages.IMDRMessageTextHavingGSTPayable;
			var provider = (IOutstandingPaymentInfoProvider)message;
			AssertType<SegmentGroup5MessageSection>(provider.Group5Section);
			AssertType<GISSegmentMessageSection>(provider.GISSection);
		}

		#region Test CP Dec Message Advice

		[TestDate(2005, 1, 2)]
		public void TestMessageAdviceWhenTheCPDecQuestionGeneratedOnOldVersion()
		{
			var updateLog = new CMRReferenceFileUpdateLog(Factory);
			updateLog.LogUpdateSuccessForTesting(new ZDateTime(2005, 1, 2));
			updateLog.LoadLastSuccessfulUpdate();
			AssertEquals("PreCondition:Last update log", new ZDateTime(2005, 1, 2), updateLog.SuccessfulUpdateFileTimeStamp.ToZDateTime());

			var message = Factory.New<CMRIMDRMessage>();
			message.EM_MessageText = CPDecErrorResponse;

			var declaration = JobDeclaration.New(Factory);
			declaration.AddInfo.ZA_CPQuestionGenDate_Hidden = new ZDateTime(2005, 1, 1);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00149151/6";
			message.EM_LinkedObject = entryHeader;

			ZString report = message.GetReport();

			var adviceMessage = new ZStringBuilder();
			adviceMessage.Append("CP Dec questions for the job were generated on the version of reference files updated on ");
			adviceMessage.Append(new ZDateTime(2005, 1, 1).ToShortDateString());
			adviceMessage.Append(". But the last successful update of the reference files was done on ");
			adviceMessage.Append(new ZDateTime(2005, 1, 2).ToShortDateString());
			adviceMessage.Append(".\r\nCustoms might have added more mandatory questions for the tariff in the meantime.");
			adviceMessage.Append(" Please open the job and regenerate Declaration questions by clicking Brokerage > Regenerate Declaration Questions");

			AssertContains("Report should include an advice on reference file", adviceMessage.ToString(), report);
		}

		const string CPDecErrorResponse = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+6BI9 CJ26 G06:1+11'NAD+MR+AAA374M::95'RFF+ABO:B00149151/6/CMT1::1'ERP+::1'ERC+ID1005::95'FTX+AAO+++CP risk associated qstn=000000000000261 REQUIRED FOR line=000000000000001 CP risk associated qstn=000000000000261,line=000000000000001'CNT+55:1'UNT+9+000001'";

		#endregion

		#region Test Report

		public void TestGetReportForCCFErrorResponse()
		{
			ZString cCFErrorResponse =
				"UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+CCF_AAA374M_169177_FID_1:1+11'NAD+MR+AAA374M:110:95'RFF+ACW:IMD'RFF+AFM:9'" +
				"RFF+ABO:B00122382/1/1::1'DTM+310:20050410232402:204'ERP+1'ERC+CCFERROR:80:95'ERC+582:6:95'FTX+AAO+++The length of " +
				"MASTERAIRWAYBILL is 10 characters which does not match the defined field length of exactly 11 characters'CNT+55:1'" +
				"UNT+13+000001'";

			CMRIMDRMessage message = Factory.New<CMRIMDRMessage>();
			message.EM_MessageText = cCFErrorResponse;
			ZString report = message.GetReport();
			AssertEquals("Report", true, report.Contains("Status: ORIGINAL REJECTED"));
			AssertEquals("Report", true, report.Contains("Errors:"));
			AssertEquals("Report", true, report.Contains("The length of MASTERAIRWAYBILL is 10 characters which does not match the defined field length of exactly 11 characters"));
		}

		public void TestGetReportForHeaderErrorResponse()
		{
			ZString headerErrorResponse =
				"UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+2475 C0I3 DHAF:1+11'NAD+MR+AAA374M::95'RFF+ABO:B00122382/1/1::2'ERP+::0'" +
				"ERC+ID0152::95'FTX+AAO+++LOADING PORT CODE=CNXNG DOES NOT EXIST LOADING PORT CODE=CNXNG'CNT+55:1'UNT+9+000001'";

			CMRIMDRMessage message = Factory.New<CMRIMDRMessage>();
			message.EM_MessageText = headerErrorResponse;
			ZString report = message.GetReport();
			AssertEquals("Report", true, report.Contains("Status: TRANSACTION REJECTED"));
			AssertEquals("Report", true, report.Contains("Errors:"));
			AssertEquals("Report", true, report.Contains("LOADING PORT CODE=CNXNG DOES NOT EXIST LOADING PORT CODE=CNXNG"));
		}

		public void TestGetReportForLineErrorResponse()
		{
			ZString lineErrorResponse =
				"UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+D577 2I43 9F5:1+11'NAD+MR+AAA374M::95'RFF+ABO:B00122382/1/1::1'ERP+::7'" +
				"ERC+ID0272::95'FTX+AAO+++DUMPING SPECIFICATION NUMBER OR DUMPING EXEMPTION TYPE REQUIRED'ERP+::16'ERC+ID0237::95'" +
				"FTX+AAO+++TARIFF RATE NUMBER IS REQUIRED'CNT+55:2'UNT+12+000001'";

			CMRIMDRMessage message = Factory.New<CMRIMDRMessage>();
			message.EM_MessageText = lineErrorResponse;
			ZString report = message.GetReport();
			AssertEquals("Report", true, report.Contains("Status: TRANSACTION REJECTED"));
			AssertEquals("Report", true, report.Contains("Errors:"));
			AssertEquals("Report", true, report.Contains("DUMPING SPECIFICATION NUMBER OR DUMPING EXEMPTION TYPE REQUIRED (LINE 7)"));
			AssertEquals("Report", true, report.Contains("TARIFF RATE NUMBER IS REQUIRED (LINE 16)"));
		}

		public void TestGetReportForHeaderAndLineErrorResponse()
		{
			ZString headerAndLineErrorResponse =
				"UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+D577 2I43 9F5:1+11'NAD+MR+AAA374M::95'RFF+ABO:B00122382/1/1::1'ERP+::0'" +
				"ERC+ID0152::95'FTX+AAO+++LOADING PORT CODE=CNXNG DOES NOT EXIST LOADING PORT CODE=CNXNG'ERP+::7'ERC+ID0272::95'" +
				"FTX+AAO+++DUMPING SPECIFICATION NUMBER OR DUMPING EXEMPTION TYPE REQUIRED'ERP+::16'ERC+ID0237::95'" +
				"FTX+AAO+++TARIFF RATE NUMBER IS REQUIRED'CNT+55:2'UNT+12+000001'";

			CMRIMDRMessage message = Factory.New<CMRIMDRMessage>();
			message.EM_MessageText = headerAndLineErrorResponse;
			ZString report = message.GetReport();
			AssertEquals("Report", true, report.Contains("Status: TRANSACTION REJECTED"));
			AssertEquals("Report", true, report.Contains("Errors:"));
			AssertEquals("Report", true, report.Contains("LOADING PORT CODE=CNXNG DOES NOT EXIST LOADING PORT CODE=CNXNG"));
			AssertEquals("Report", true, report.Contains("DUMPING SPECIFICATION NUMBER OR DUMPING EXEMPTION TYPE REQUIRED (LINE 7)"));
			AssertEquals("Report", true, report.Contains("TARIFF RATE NUMBER IS REQUIRED (LINE 16)"));
		}

		public void TestGetReportForAdviceErrorsAndSecurityAmount()
		{
			ZString response =
				"UNH+000001+CUSRES:D:99B:UN'" +
				"BGM+961:::IMDR+AJ3D AI8G 755:1+11'FTX+AHN+++HELD:HELD'GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA447Y::95'" +
				"NAD+VT+AA33JL::95'NAD+CB+54333::95'NAD+IM++PORTER DATA MANAGEMENT PTY LTD'NAD+CB++SOFTWARE CRAFT PTY LTD'RFF+ABO:B00122382/1/1::1'" +
				"RFF+ABT:AAAAT33FA::1'RFF+ABQ:433407'RFF+ADU:B00122382'RFF+AAE:N10'ERP+::0'ERC+ID0060::95'FTX+AAO+++INVOICE TOTAL AMOUNT IS LESS THAN FREE ON BOARD AMOUNT'" +
				"TAX+3'MOA+39:0000000002534.21'TAX+3'MOA+40:0000000002534.21'TAX+3'MOA+369:0000000000266.09'TAX+3'MOA+68:0000000000126.71'TAX+3'" +
				"MOA+292:0000000009450.00'TAX+3'MOA+Z01:0000000009876.54'TAX+3'" +
				"MOA+128:0000000000297.84'TAX+3'MOA+26:0000000000002.50'TAX+3'MOA+23:0000000000029.25'DOC+1+1'FTX+AHN+++HELD:HELD'CST+1+N10::95'" +
				"FTX+AAF+++FREE'TAX+1'GIS+LAQ:109:95'TAX+1'MOA+40:0000000002534.21'TAX+1'MOA+369:0000000000266.09'TAX+1'MOA+68:0000000000126.71'" +
				"ERP+::2'ERC+1::95'FTX+ABS+++IFIS (IMPORTED FOOD INSPECTION SCHEME) CLEARANCE REQUIRED'ERP+::3'ERC+1::95'FTX+ABS+++IMPORTED FOOD " +
				"CONTROL ACT 1992. IFIS (IMPORTED FOOD INSPECTION SCHEME) PERMISSION TO DELIVER TO THE IMPORTER REQUIRED'CNT+5:1'UNT+56+000001'" +
				"UNZ+1+00000000003899'";

			CMRIMDRMessage message = Factory.New<CMRIMDRMessage>();
			message.EM_MessageText = response;
			ZString report = message.GetReport();
			Assert("Report", report.Contains("Status: HELD"));
			Assert("Report", report.Contains("Status Description: HELD"));
			Assert("Report", report.Contains("Warnings:"));
			Assert("Report", report.Contains("INVOICE TOTAL AMOUNT IS LESS THAN FREE ON BOARD AMOUNT"));
			Assert("Report", report.Contains("Message Advice:"));
			Assert("Report", report.Contains("IFIS (IMPORTED FOOD INSPECTION SCHEME) CLEARANCE REQUIRED (LINE 1)"));
			Assert("Report", report.Contains("IMPORTED FOOD CONTROL ACT 1992. IFIS (IMPORTED FOOD INSPECTION SCHEME) PERMISSION TO DELIVER TO THE IMPORTER REQUIRED (LINE 1)"));
			Assert("Report", report.Contains("Total Security Concession Amount: 9450.00"));
			Assert("Report", report.Contains("Total Security Uncollected Amount: 9876.54"));
		}

		public void TestGetReportWithOneMessageAdviceOnMultipleLines()
		{
			ZString response = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+AJ3D AI8G 755:1+11'FTX+AHN+++HELD:HELD'" +
				"GIS+N:117:95'GIS+TLB:109:95'GIS+LLB:109:95'NAD+MR+AAA447Y::95'NAD+VT+AA33JL::95'NAD+CB+54333::95'" +
				"NAD+IM++PORTER DATA MANAGEMENT PTY LTD'NAD+CB++SOFTWARE CRAFT PTY LTD'RFF+ABO:B00122382/1/1::1'" +
				"RFF+ABT:AAAAT33FA::1'RFF+ABQ:433407'RFF+ADU:B00122382'RFF+AAE:N10'ERP+::0'ERC+ID0060::95'" +
				"FTX+AAO+++INVOICE TOTAL AMOUNT IS LESS THAN FREE ON BOARD AMOUNT'TAX+3'MOA+39:0000000002534.21'TAX+3'" +
				"MOA+40:0000000002534.21'TAX+3'MOA+369:0000000000266.09'TAX+3'MOA+68:0000000000126.71'TAX+3'" +
				"MOA+128:0000000000297.84'TAX+3'MOA+26:0000000000002.50'TAX+3'MOA+23:0000000000029.25'DOC+1+1'" +
				"FTX+AHN+++HELD:HELD'CST+1+N10::95'FTX+AAF+++FREE'TAX+1'GIS+LAQ:109:95'TAX+1'MOA+40:0000000002534.21'TAX+1'" +
				"MOA+369:0000000000266.09'TAX+1'MOA+68:0000000000126.71'ERP+::5'ERC+1::95'ERC+2::95'FTX+ABS+++QUARANTINE CLEARANCE REQUIRED'" +
				"CNT+5:1'UNT+53+000001'UNZ+1+00000000003899'";

			CMRIMDRMessage message = Factory.New<CMRIMDRMessage>();
			message.EM_MessageText = response;
			ZString report = message.GetReport();
			AssertEquals("Report", true, report.Contains("Status: HELD"));
			AssertEquals("Report", true, report.Contains("Status Description: HELD"));
			AssertEquals("Report", true, report.Contains("Warnings:"));
			AssertEquals("Report", true, report.Contains("INVOICE TOTAL AMOUNT IS LESS THAN FREE ON BOARD AMOUNT"));
			AssertEquals("Report", true, report.Contains("Message Advice:"));
			AssertEquals("Report", true, report.Contains("QUARANTINE CLEARANCE REQUIRED (LINE 1)"));
			AssertEquals("Report", true, report.Contains("QUARANTINE CLEARANCE REQUIRED (LINE 2)"));
		}

		#endregion

		#region Implemenation

		protected override EDIMessage GetEDIMessage(string reference)
		{
			CMRIMDRMessageTestClass message = Factory.New<CMRIMDRMessageTestClass>();
			message.EM_MessageText = CMRImportDeclarationTestData.IMDRClear.Replace("B00122382", reference);
			return message;
		}

		protected override BusinessObject GetWrappedObject(string reference)
		{
			CMRIMDRMessageTestClass message = (CMRIMDRMessageTestClass)GetEDIMessage(reference);
			return message.GetWrappedObjectTestMethod();
		}

		#endregion
	}
}
