using System;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SACMessageBuilderTest : BaseImportMessageBuilderAbstractTest
	{
		public void TestNADForImporterThatDoesNotHaveABNOrCCID()
		{
			OrgHeader importer = OrgHeader.New(Factory);
			importer.OH_FullName = "FRED BLOGGS";

			importer.MainAddress.OA_Address1 = "PO BOX 73";
			importer.MainAddress.OA_City = "PENSHURST";
			importer.MainAddress.OA_State = "NSW";
			importer.MainAddress.OA_PostCode = "2222";
			importer.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_OH_Importer = importer.PK;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			SACMessageBuilder builder = GetCreateBuilder(entryHeader, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateNADForImporter(importer);
			ZString result = builder.MessageText;
			ZString expectedAddress = "NAD+IM++PENSHURST+FRED BLOGGS+PO BOX 73++:::NSW+2222+AU'";
			AssertEquals("Message generated", expectedAddress, result);

			importer.LocalBusinessRegNo = "12345678901";
			builder = GetCreateBuilder(entryHeader, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateNADForImporter(importer);
			result = builder.MessageText;
			AssertEquals("Importer ABN", true, result.Contains("NAD+AT+12345678901::95'"));
			AssertEquals("Importer ABN is there so Address is not necessary", false, result.Contains(expectedAddress));

			importer.LocalBusinessRegNo = "";
			importer.CustomsClientID = "123456";
			var cusCode = importer.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia);
			builder = GetCreateBuilder(entryHeader, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateNADForImporter(importer);
			result = builder.MessageText;
			AssertEquals("Importer CCIDs", true, result.Contains("NAD+IM+123456::95'"));
			AssertEquals("Importer CCIDs is there so Address is not necessary", false, result.Contains(expectedAddress));
		}

		public void TestMessageSpecificContainerTypeCodesIsUsed()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			CusEntryHeaderToTestWith.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusEntryHeaderToTestWith.Declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;

			CusContainer container = CusEntryHeaderToTestWith.Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BreakBulk;
			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.MessageText;

			AssertEquals("Container Type was expected to be in" + System.Environment.NewLine + result, true, result.Contains("'PAC+++B/B:67:95'"));
		}

		public void TestGroup1WithConsignmentReferenceNumber()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForAir();
			CusEntryHeaderToTestWith.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusEntryHeaderToTestWith.Declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			CusEntryHeaderToTestWith.Declaration.JE_PartShipConsignmentReference = "X423809";

			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.MessageText;

			AssertContains("RFF+CNR:X423809", result);
		}

		public void TestSortQuestionsBeforeGeneratingMessages()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			CusEntryHeaderToTestWith.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusEntryHeaderToTestWith.Declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;

			CMRCusEntryCPDec question = CusEntryHeaderToTestWith.Questions.AddNew();
			question.ON_CPDecNum = 11;
			question.ON_AnswerCode = "Y";

			CMRCusEntryCPDec question2 = CusEntryHeaderToTestWith.Questions.AddNew();
			question2.ON_CPDecNum = 1;
			question2.ON_AnswerCode = "Y";

			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.MessageText;

			AssertEquals("Messages should be built after sorting", true, result.Contains("RFF+AMG:0001'RFF+AMG:0011'"));
		}

		[TestDate(2017, 4, 11)]
		public void TestPopulateGroup6_Vendor()
		{
			using (AUCustomsDataRegistry.Instance.ICSReleaseEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2017, 4, 1)))
			{
				var supplier = Factory.New<OrgHeader>();
				supplier.OH_Code = "Test 1";
				supplier.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "12345678901");

				var testDec = JobDeclaration.New(Factory);
				testDec.JE_ExportDate = ZDateTime.Today;
				testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				testDec.JE_MessageSubType = "SWL";
				testDec.JE_OH_Supplier = supplier.PK;
				var invoice = testDec.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 150m;
				invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 150m;

				var entryHeader = testDec.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;

				invoiceLine.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 10m, "USD");
				entryLine.CL_CustomsValue = 150m;

				var builder = GetCreateBuilder(entryHeader, CMRMessageTypes.LodgeWithoutPay);
				builder.cUSDEC = new CUSDECMessage();
				builder.PopulateGroup6();
				var result = builder.MessageText;

				AssertEquals("Vendor is sent in the message", true, result.Contains("NAD+VN+12345678901::95'"));
			}
		}

		public void TestPopulateGroup8()
		{
			ZDateTime futureDate = ZDateTime.Today.AddDays(1);
			var testHelper = new ZTestHelper(Factory);
			testHelper.SetExchangeRate(futureDate, futureDate.AddDays(1), 0.5m, testHelper.USDCurrency);

			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_ExportDate = ZDateTime.Today;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = "SWL";
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 150m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 150m;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			invoiceLine.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 10m, "USD");
			entryLine.CL_CustomsValue = 150m;

			CMRCusEntryCPDec cPDec = entryHeader.Questions.AddNew();
			cPDec.ON_CPDecNum = 18;
			cPDec.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;

			SACMessageBuilder builder = GetCreateBuilder(entryHeader, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup8();
			ZString result = builder.MessageText;

			AssertEquals("T&I is sent in the message", true, result.Contains("MOA+68:20.00:AUD"));
			AssertEquals("Customs Value is sent in the message", true, result.Contains("MOA+40:150.00:AUD"));
		}

		public void TestForSACWithLinesGroup30IsGenerated()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 150m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 150m;

			var merger = new LineMerger(testDec);
			merger.DoMerge();

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];

			SACMessageBuilder builder = GetCreateBuilder(entryHeader, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup30();
			ZString result = builder.MessageText;
			AssertEquals("Group30 is not populated" + System.Environment.NewLine + result, true, result.Contains("CST+1+I::95'MOA+40:150.00:AUD'"));
		}

		public void TestForSACWithoutLinesGroup30IsNotGenerated()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 150m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 150m;

			var merger = new LineMerger(testDec);
			merger.DoMerge();

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];

			SACMessageBuilder builder = GetCreateBuilder(entryHeader, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup30();
			ZString result = builder.MessageText;
			AssertEquals("Group30 is populated" + System.Environment.NewLine + result, true, result.IsEmpty);
		}

		public void TestValuationDate()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			CusEntryHeaderToTestWith.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeaderToTestWith.Declaration.JE_MessageSubType = "SWL";
			CMRCusEntryCPDec cPDec = CusEntryHeaderToTestWith.Questions.AddNew();
			cPDec.ON_CPDecNum = 18;
			cPDec.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;

			CusEntryHeaderToTestWith.Declaration.JE_ExportDate = new ZDateTime(2005, 8, 16);

			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateDTM();
			ZString result = builder.MessageText;

			AssertEquals("Date of valuation is necessary", true, result.Contains("DTM+260:20050816:102"));
		}

		public void TestPopulateLocations()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = "SWL";
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			invoice.AddInfo.ZA_ORG = "NZ";
			testDec.AddInfo.ZA_AQISInspectLocation_Hidden = "TEST";

			SACMessageBuilder builder = GetCreateBuilder(entryHeader, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateLocations();

			ZString result = builder.MessageText;
			AssertEquals("Message contains LOC segments with Goods Origin", true, result.Contains("LOC+27+NZ::5"));
			AssertEquals("Message contains LOC segments with AQISInspectionLocation", true, result.Contains("LOC+90+:::TEST"));
		}

		public void TestAcknowledgeQuestions()
		{
			CMRLodgementQuestion question = CMRLodgementQuestion.New(Factory);
			question.CQ_LodgementQuestionIdentifier = 2;
			question.CQ_LodgementQuestionStartDate = ZDateTime.Today;
			question.CQ_LodgementQuestionText = "This is a question";

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			CMRCusEntryCPDec acknowledgeQuestion = CusEntryHeaderToTestWith.Questions.AddNew();
			acknowledgeQuestion.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			acknowledgeQuestion.ON_CPDecNum = question.CQ_LodgementQuestionIdentifier;
			acknowledgeQuestion.ON_CPDecStartDate = question.CQ_LodgementQuestionStartDate;

			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Acknowledge Questions", true, result.Contains("RFF+AMG:0002'"));
		}

		public void TestYesAnswerToLodgementQuestion()
		{
			CMRLodgementQuestion question = CMRLodgementQuestion.New(Factory);
			question.CQ_LodgementQuestionIdentifier = 400;
			question.CQ_LodgementQuestionStartDate = ZDateTime.Today;
			question.CQ_LodgementQuestionText = "This is a question";

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			CMRCusEntryCPDec qA = CusEntryHeaderToTestWith.Questions.AddNew();
			qA.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			qA.ON_CPDecNum = question.CQ_LodgementQuestionIdentifier;
			qA.ON_CPDecStartDate = question.CQ_LodgementQuestionStartDate;

			AssertEquals("This Q is not an Acknowledge", false, qA.IsAcknowledge);

			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			builder.PopulateQuestions();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Normal answer", true, result.Contains("FTX+ACD+++0400:Y:THIS IS THE REFERAL REASON'"));
		}

		public void TestNoAnswerToLodgementQuestion()
		{
			CMRLodgementQuestion question = CMRLodgementQuestion.New(Factory);
			question.CQ_LodgementQuestionIdentifier = 400;
			question.CQ_LodgementQuestionStartDate = ZDateTime.Today;
			question.CQ_LodgementQuestionText = "This is a question";

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			CMRCusEntryCPDec acknowledgeQuestion = CusEntryHeaderToTestWith.Questions.AddNew();
			acknowledgeQuestion.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			acknowledgeQuestion.ON_CPDecNum = question.CQ_LodgementQuestionIdentifier;
			acknowledgeQuestion.ON_CPDecStartDate = question.CQ_LodgementQuestionStartDate;

			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			builder.PopulateQuestions();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Normal answer", true, result.Contains("FTX+ACD+++0400:N:THIS IS THE REFERAL REASON'"));
		}

		public void TestNullAnswerToLodgementQuestion()
		{
			CMRLodgementQuestion question = CMRLodgementQuestion.New(Factory);
			question.CQ_LodgementQuestionIdentifier = 400;
			question.CQ_LodgementQuestionStartDate = ZDateTime.Today;
			question.CQ_LodgementQuestionText = "This is a question";

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			CMRCusEntryCPDec acknowledgeQuestion = CusEntryHeaderToTestWith.Questions.AddNew();
			acknowledgeQuestion.ON_CPDecNum = question.CQ_LodgementQuestionIdentifier;
			acknowledgeQuestion.ON_CPDecStartDate = question.CQ_LodgementQuestionStartDate;

			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			builder.PopulateQuestions();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Message should not contain question 400", false, result.Contains("FTX+ACD+++0400"));
		}

		public void TestQuestionsArePopulatedForWithdrawalForSAC()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			CusEntryHeaderToTestWith.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusEntryHeaderToTestWith.Declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			CusContainer container = CusEntryHeaderToTestWith.Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BreakBulk;
			CMRCusEntryCPDec cPDec14 = CusEntryHeaderToTestWith.Questions.AddNew();
			cPDec14.ON_CPDecNum = 14;
			cPDec14.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			CMRCusEntryCPDec cPDec12 = CusEntryHeaderToTestWith.Questions.AddNew();
			cPDec12.ON_CPDecNum = 12;
			cPDec12.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;

			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
			builder.PopulateHeaderGroupsForWithdrawal();
			ZString result = builder.MessageText;

			AssertEquals("FTX was expected to be in" + System.Environment.NewLine + result, true, result.Contains("'FTX+ACD+++0014:N'"));
			AssertEquals("RFF was expected to be in" + System.Environment.NewLine + result, true, result.Contains("'RFF+AMG:0012'"));
		}

		public void TestQuestionsArePopulatedForWithdrawalForSWL()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			CusEntryHeaderToTestWith.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusEntryHeaderToTestWith.Declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			CusContainer container = CusEntryHeaderToTestWith.Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BreakBulk;
			CMRCusEntryCPDec cPDec14 = CusEntryHeaderToTestWith.Questions.AddNew();
			cPDec14.ON_CPDecNum = 14;
			cPDec14.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			CMRCusEntryCPDec cPDec12 = CusEntryHeaderToTestWith.Questions.AddNew();
			cPDec12.ON_CPDecNum = 12;
			cPDec12.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			CMRCusEntryCPDec cPDec10 = CusEntryHeaderToTestWith.Questions.AddNew();
			cPDec10.ON_CPDecNum = 10;
			cPDec10.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			CMRCusEntryCPDec cPDec15 = CusEntryHeaderToTestWith.Questions.AddNew();
			cPDec15.ON_CPDecNum = 15;
			cPDec15.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			CusEntryHeaderToTestWith.Questions.Sort(CusEntryCPDecSchema.ON_CPDecNum.Name, System.ComponentModel.ListSortDirection.Descending);

			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.Withdrawal);
			builder.cUSDEC = new CUSDECMessage();
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
			builder.PopulateHeaderGroupsForWithdrawal();
			ZString result = builder.MessageText;

			AssertEquals("FTX was expected to be in sorted order" + System.Environment.NewLine + result, true, result.Contains("'FTX+ACD+++0014:N'FTX+ACD+++0015:Y'"));
			AssertEquals("RFF was expected to be in" + System.Environment.NewLine + result, true, result.Contains("'RFF+AMG:0012'"));
			AssertEquals("RFF was expected to be in" + System.Environment.NewLine + result, true, result.Contains("'RFF+AMG:0010'"));
		}

		public void TestGISLLBIsGeneratedForSAC()
		{
			Env.Registry.RequestOfficialCustomsPaymentReceipt = true;
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGIS();

			ZString result = builder.MessageText;
			AssertEquals("Message does not contains GIS+LLB. This would be a control error", false, result.Contains("GIS+LLB:109:95"));
			AssertEquals("Message does not contains GIS+POR, is pre-lodge.", false, result.Contains("GIS+POR:109:95"));
		}

		public override void TestLocationsSegment()
		{
			Env.Registry.RequestOfficialCustomsPaymentReceipt = true;
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateLocations();
			ZString expectedSection =
				@"LOC+8+AUMEL::6'
LOC+12+AUSYD::6'";
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertMultilineEquals("LOC Segment", expectedSection.Replace("\r\n", ""), result, '\'');
		}

		public override void TestGISSegment()
		{
			Env.Registry.RequestOfficialCustomsPaymentReceipt = true;
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGIS();
			ZString expectedSection =
				@"GIS+EPA:109:95'
GIS+Y:153:95'
GIS+POR:109:95'";
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertMultilineEquals("GIS Segment", expectedSection.Replace("\r\n", ""), result, '\'');
		}

		public void TestGISSegmentWithPORTurnedOffInRegistry()
		{
			Env.Registry.RequestOfficialCustomsPaymentReceipt = false;

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGIS();
			ZString expectedSection =
				@"GIS+EPA:109:95'
GIS+Y:153:95'";
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertMultilineEquals("GIS Segment", expectedSection.Replace("\r\n", ""), result, '\'');
		}

		public override void TestSegmentGroup1()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			CusEntryHeaderToTestWith.EntryNumber = "ABC0987";

			AddContainersAndPacksToDeclaration();
			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertEquals("Entry Number as this is Create", false, result.Contains("RFF+ABT:ABC0987'"));
			AssertEquals("Owners Ref", true, result.Contains("RFF+ABQ:<<OWNER REFERENCE PLACE HOLDER>>'"));
			AssertEquals("Master Bill Number", true, result.Contains("RFF+MB:AQT1'"));
			AssertEquals("House Bill Number", true, result.Contains("RFF+BH:AQT1HBL'"));
			AssertEquals("House Bill Number", true, result.Contains("RFF+BH:AQT2HBL'"));

			AssertEquals("Container One", true, result.Contains("RFF+AAQ:OCLU10000020'"));
			AssertEquals("Container Two", true, result.Contains("RFF+AAQ:OCLU10000031'"));
			AssertEquals("Container Two", true, result.Contains("RFF+AAQ:OCLU10000042'"));

			builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.Amendment);
			builder.cUSDEC = new CUSDECMessage();
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Entry Number as this is Amendment", true, result.Contains("RFF+ABT:ABC0987'"));
		}

		public override void TestFTXSegment()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("FTX Segment", true, result.Contains("FTX+DEL+++NAME OF IMPORTER'FTX+AAA'"));
		}

		public override void TestSegmentGroup4()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			ZString expectedSection = "TDT+20+QF1234+S+++++9203473::11'";
			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup4();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Segment Group 4", expectedSection, result);

			CusEntryHeaderToTestWith = GetCusEntryHeaderForAir();
			expectedSection = "TDT+20++A'";
			builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup4();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Segment Group 4", expectedSection, result);

			CusEntryHeaderToTestWith = GetCusEntryHeaderForMail();
			expectedSection = "TDT+20++P'";
			builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup4();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Segment Group 4", expectedSection, result);
		}

		public override void TestSegmentGroup6()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			GenRegCertAccredMaintList brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
			brokerLicence.XZ_RefNumber = "54321";
			brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;
			testDec.Importer.LocalBusinessRegNo = "12345678901CAC";
			testDec.Importer.CustomsClientID = "12345678901";
			Env.Registry.AUCustoms.SetLocalCustomsBranchIdentifierForBranch(GlbBranch.CurrentBranch.PK.ToGuid(), "AA33HF");

			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Importer ABN", true, result.Contains("NAD+AT+12345678901::95'"));
			AssertEquals("Importer ID not in message as ABN is entered", false, result.Contains("NAD+IM+12345678901::95'"));
			AssertEquals("Branch ID", true, result.Contains("NAD+VT+AA33HF::95'"));
			AssertEquals("Importer CAC", true, result.Contains("NAD+WP+CAC::95'"));
			AssertEquals("Doesn't have CTA", false, result.Contains("CTA+IC'"));
			AssertEquals("Doesn't have Communication Contact", false, result.Contains("COM+'"));
			AssertEquals("There are 4 SegmentGroup6", 4, builder.cUSDEC.Group6.Count);

			testDec.Importer.LocalBusinessRegNo = ZString.Empty;
			builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Importer ABN", false, result.Contains("NAD+AT+12345678901::95'"));
			AssertEquals("Importer ID", true, result.Contains("NAD+IM+12345678901::95'"));
			AssertEquals("Branch ID", true, result.Contains("NAD+VT+AA33HF::95'"));
			AssertEquals("Importer CAC", false, result.Contains("NAD+WP+CAC::95'"));
			AssertEquals("Doesn't have CTA", false, result.Contains("CTA+IC'"));
			AssertEquals("Doesn't have Communication Contact", false, result.Contains("COM+'"));
			AssertEquals("There are 3 SegmentGroup6", 3, builder.cUSDEC.Group6.Count);

			AUCustomsDataRegistry.Instance.LocalContactPhoneNumber.SetValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, "0292001234");
			Env.Registry.AUCustoms.SetLocalCustomsBranchIdentifierForBranch(GlbBranch.CurrentBranch.PK.ToGuid(), string.Empty);
			builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("CTA and COM appear after NAD+VT", true, result.Contains("NAD+IM+12345678901::95'NAD+VT'CTA+IC'COM+0292001234:TE'NAD+DP"));
			AssertEquals("There are 3 SegmentGroup6", 3, builder.cUSDEC.Group6.Count);
		}

		public void TestTrustedTraderIdentification()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			GenRegCertAccredMaintList brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
			brokerLicence.XZ_RefNumber = "54321";
			brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;
			testDec.Importer.LocalBusinessRegNo = "12345678901CAC";
			testDec.Importer.CustomsClientID = "12345678901";
			Env.Registry.AUCustoms.SetLocalCustomsBranchIdentifierForBranch(GlbBranch.CurrentBranch.PK.ToGuid(), "AA33HF");

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUP001";
			supplier.CustomsClientID = "9876543210";
			supplier.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.TraderIdentificationNumber, "SUPTIN1234567890", Core.Constants.CountryCodes.Australia);
			testDec.JE_OH_Supplier = supplier.PK;

			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			ZString generatedMessage = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertContains("Generated message has Supplier TIN code.", "NAD+AU+SUPTIN1234567890::95'", generatedMessage);
		}

		public override void TestBGMSegment()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			builder.PopulateMessages();
			Factory.Save();
			ZString messageText = CusEntryHeaderToTestWith.Messages[0].EM_MessageText;
			Assert(messageText.Contains("BGM+929:::SAC+" + CusEntryHeaderToTestWith.CH_BGMReference + "/DAT1:1+9'"));
		}

		public override void TestSegmentGroup30()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			ZString expectedSection =
				@"CST+1+I::95'
FTX+AAA+++DESCRIPTION OF THE GOODS FOR LINE 1'
MEA+AAA++CU:5.00000'
MOA+40:1046.35:AUD'
RFF+ABD:44109000'
RFF+AED:25'";

			CusEntryHeaderToTestWith.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeaderToTestWith.Declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			SACMessageBuilder builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup30();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertMultilineEquals("Segment Group 30", expectedSection.Replace("\r\n", ""), result, '\'');

			CusEntryHeaderToTestWith.Declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			builder = GetCreateBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup30();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Segment Group 30", "", result);
		}

		protected override BaseImportMessageBuilder GetMessageBuilderToTest(CMRMessageTypes messageType) => GetCreateBuilder(CusEntryHeaderToTestWith, messageType);

		SACMessageBuilder GetCreateBuilder(CusEntryHeader entryHeader, CMRMessageTypes messageType)
		{
			SACMessageBuilder builder = new SACMessageBuilder(entryHeader, messageType);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			return builder;
		}
	}
}
