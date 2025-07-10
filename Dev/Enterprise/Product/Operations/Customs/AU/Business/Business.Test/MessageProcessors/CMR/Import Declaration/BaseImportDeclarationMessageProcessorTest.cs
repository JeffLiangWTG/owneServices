using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class BaseImportDeclarationMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestSettingEntryStatus()
		{
			EDIMessage iMDRMessage = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage.EM_MessageText = CMRImportDeclarationTestData.IMRHeld;
			new IMDRMessageProcessor(logger).ProcessMessage(iMDRMessage);
			AssertEquals("Entry Header Entry Status", CMRImportEntryAdvice.Held.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration Entry Status", CMRImportEntryAdvice.Held.Code, declaration.JE_EntryStatus);

			EDIMessage sAMMessage = entryHeader.Messages.AddNew(typeof(CMRSAMMessage));
			sAMMessage.EM_MessageText = CMRImportDeclarationTestData.SAM;
			new SAMMessageProcessor(logger).ProcessMessage(sAMMessage);
			AssertEquals("Entry Header Entry Status", CMRImportEntryAdvice.Clear.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration Entry Status", CMRImportEntryAdvice.Clear.Code, declaration.JE_EntryStatus);

			EDIMessage aTDMessage = entryHeader.Messages.AddNew(typeof(CMRATDMessage));
			aTDMessage.EM_MessageText = CMRImportDeclarationTestData.ATD;
			new ATDMessageProcessor(logger).ProcessMessage(aTDMessage);
			AssertEquals("Entry Header Entry Status", CMRImportEntryAdvice.ATDReceived.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration Entry Status", CMRImportEntryAdvice.ATDReceived.Code, declaration.JE_EntryStatus);

			EDIMessage iMDRMessage2 = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage2.EM_MessageText = CMRImportDeclarationTestData.IMDRFinalised;
			new IMDRMessageProcessor(logger).ProcessMessage(iMDRMessage2);
			AssertEquals("Entry Header Entry Status should not change", CMRImportEntryAdvice.ATDReceived.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration Entry Status should not change", CMRImportEntryAdvice.ATDReceived.Code, declaration.JE_EntryStatus);

			EDIMessage iMDRMessage3 = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage3.EM_MessageText = CMRImportDeclarationTestData.IMRHeld;
			new IMDRMessageProcessor(logger).ProcessMessage(iMDRMessage3);
			AssertEquals("Entry Header Entry Status does change", CMRImportEntryAdvice.Held.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration Entry Status does change", CMRImportEntryAdvice.Held.Code, declaration.JE_EntryStatus);
		}

		public void TestSettingEntryStatusWithMultiHeaders()
		{
			EDIMessage iMDRMessageForHeader1 = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessageForHeader1.EM_MessageText = CMRImportDeclarationTestData.IMRHeld;
			new IMDRMessageProcessor(logger).ProcessMessage(iMDRMessageForHeader1);
			AssertEquals("Entry Header Entry Status", CMRImportEntryAdvice.Held.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration Entry Status", CMRImportEntryAdvice.Held.Code, declaration.JE_EntryStatus);

			CusEntryHeader entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_BGMReference = "B00122382/2";
			EDIMessage aTDMessageForHeader2 = entryHeader2.Messages.AddNew(typeof(CMRATDMessage));
			aTDMessageForHeader2.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::ATD+13B9 DAI3 I355:1+11'DTM+58:20050204:102'DTM+138:20050204:102'FTX+AHN+++FINALISED:FINALISED'" +
				"TDT+20++6'LOC+12+AUSYD::6'EQD+AH+N123::95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95+EAGLE DATAMATION INTERNATIONAL PTY:LTD'" +
				"NAD+IM++EAGLE DATAMATION NB'RFF+ABO:B00122382/2/1::1'RFF+ABT:AAAANNPR6::1'RFF+ABQ:SIMPLE MAIL'RFF+AIA:AAAANNPTY'RFF+AAE:N10'" +
				"DOC+1+1'PAC+150+1'CST+1'TAX+1'MOA+68:0.0000'MOA+40:15.0000'MEA+AAA+::WAR+NO:100.00000'RFF+ABD:96092000'RFF+AED:17'CNT+5:1'CNT+2:1'CNT+3:0'" +
				"UNT+30+000001'UNZ+1+00000000273283'";
			new ATDMessageProcessor(logger).ProcessMessage(aTDMessageForHeader2);
			AssertEquals("Entry Header Entry Status", CMRImportEntryAdvice.Held.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Entry Header 2 Entry Status", CMRImportEntryAdvice.ATDReceived.Code, entryHeader2.CH_EntryStatus);
			AssertEquals("Declaration Entry Status", CMRImportEntryAdvice.MultiStatus.Code, declaration.JE_EntryStatus);
		}

		public void TestEntryNumberNotPopulatedIfRejected()
		{
			EDIMessage message = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			message.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+H265 6D20 6F:1+11'NAD+MR+FGG369J::95'RFF+ABO:B00122382/1/MEL1::1'ERP+::1'"
				+ "ERC+ID0244::95'FTX+AAO+++PREFERENCE SCHEME DOES NOT EXIST FOR TARIFF CLASSIFICATION NUMBER'CNT+55:1'UNT+9+000001'UNZ+1+00000000002179'";
			new IMDRMessageProcessor(logger).ProcessMessage(message);
			AssertEquals("Entry number is not populated", ZString.Empty, declaration.CustomsEntryHeaders[0].EntryNumber);
		}

		public void TestEntryHeaderNumberIsPopulated()
		{
			declaration.CustomsEntryHeaders[0].EntryNumber = "";
			EDIMessage message = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			message.EM_MessageText = CMRImportDeclarationTestData.IMDRClear;
			new IMDRMessageProcessor(logger).ProcessMessage(message);
			AssertEquals("Entry number on invoice header is populated", "AAAA6RHTE", declaration.CustomsEntryHeaders[0].EntryNumber);
		}

		public void TestEntryHeaderNumberIsPopulatedWithATD()
		{
			declaration.CustomsEntryHeaders[0].EntryNumber = "";
			EDIMessage message = entryHeader.Messages.AddNew(typeof(CMRATDMessage));
			message.EM_MessageText = CMRImportDeclarationTestData.ATD;
			new ATDMessageProcessor(logger).ProcessMessage(message);
			AssertEquals("Entry number on invoice header is populated", "AAAA6RHTE", declaration.CustomsEntryHeaders[0].EntryNumber);
		}

		public void TestSettingEntryNumberForPreLodgement()
		{
			entryHeader.EntryNumber = "";
			EDIMessage iMDRMessage = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage.EM_MessageText = CMRImportDeclarationTestData.PreLodgementProcessing;
			new IMDRMessageProcessor(logger).ProcessMessage(iMDRMessage);
			AssertEquals("Entry Header Number not saved", ZString.Empty, entryHeader.EntryNumber);
		}

		public void TestSettingEntryNumberForLodgement()
		{
			entryHeader.EntryNumber = "";
			EDIMessage iMDRMessage = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage.EM_MessageText = CMRImportDeclarationTestData.IMRHeld;
			new IMDRMessageProcessor(logger).ProcessMessage(iMDRMessage);
			AssertEquals("Entry Header Number", "AAAA6RHTE", entryHeader.EntryNumber);
		}

		public void TestSubjectToRedLineFromHeldToClear()
		{
			entryHeader.EntryNumber = "";
			EDIMessage iMDRMessage = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage.EM_MessageText = CMRImportDeclarationTestData.IMRHeld;
			new IMDRMessageProcessor(logger).ProcessMessage(iMDRMessage);
			AssertEquals("Is Subject To Red Line", true, entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden);

			EDIMessage sAMMessage = entryHeader.Messages.AddNew(typeof(CMRSAMMessage));
			sAMMessage.EM_MessageText = CMRImportDeclarationTestData.SAM;
			new SAMMessageProcessor(logger).ProcessMessage(sAMMessage);
			AssertEquals("Is Subject To Red Line", false, entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden);
		}

		public void TestSubjectToRedLineFromHeldToFinalised()
		{
			entryHeader.EntryNumber = "";
			EDIMessage iMDRMessage = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage.EM_MessageText = CMRImportDeclarationTestData.IMRHeld;
			new IMDRMessageProcessor(logger).ProcessMessage(iMDRMessage);
			AssertEquals("Is Subject To Red Line", true, entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden);

			EDIMessage aTDMessage = entryHeader.Messages.AddNew(typeof(CMRATDMessage));
			aTDMessage.EM_MessageText = CMRImportDeclarationTestData.ATD;
			new ATDMessageProcessor(logger).ProcessMessage(aTDMessage);
			AssertEquals("Is Subject To Red Line", false, entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden);
		}

		public void TestSubjectToRedLineFromHeldToHeld()
		{
			entryHeader.EntryNumber = "";
			EDIMessage iMDRMessage = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage.EM_MessageText = CMRImportDeclarationTestData.IMRHeld;
			new IMDRMessageProcessor(logger).ProcessMessage(iMDRMessage);
			AssertEquals("Is Subject To Red Line", true, entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden);

			EDIMessage iMDRMessage1 = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage1.EM_MessageText = CMRImportDeclarationTestData.IMRHeld.Replace("ERC+ID5310::95'FTX+AAO+++SUBJECT TO RED LINE PROCESSING. DOCUMENTS MUST BE SUBMITTED PURSUANT TO S71DA OF THE CUSTOMS ACT'", "");
			new IMDRMessageProcessor(logger).ProcessMessage(iMDRMessage1);
			AssertEquals("Is Subject To Red Line", false, entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden);
		}

		public void TestSubjectToRedLineFromClearToWithdrawn()
		{
			entryHeader.EntryNumber = "";
			EDIMessage iMDRMessage = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage.EM_MessageText = CMRImportDeclarationTestData.IMRWithdrawnHeld;
			new IMDRMessageProcessor(logger).ProcessMessage(iMDRMessage);
			AssertEquals("Is Subject To Red Line", true, entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden);

			EDIMessage iMDRMessage1 = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage1.EM_MessageText = CMRImportDeclarationTestData.IMRHeld.Replace("ERC+ID5310::95'FTX+AAO+++SUBJECT TO RED LINE PROCESSING. DOCUMENTS MUST BE SUBMITTED PURSUANT TO S71DA OF THE CUSTOMS ACT'", "");
			new IMDRMessageProcessor(logger).ProcessMessage(iMDRMessage1);
			AssertEquals("Is Subject To Red Line", false, entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden);
		}

		public void TestIsNotSubjectToRedLine()
		{
			EDIMessage iMDRMessage = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage.EM_MessageText = CMRImportDeclarationTestData.IMRHeld.Replace("ERC+ID5310::95'FTX+AAO+++SUBJECT TO RED LINE PROCESSING. DOCUMENTS MUST BE SUBMITTED PURSUANT TO S71DA OF THE CUSTOMS ACT'", "");
			new IMDRMessageProcessor(logger).ProcessMessage(iMDRMessage);
			AssertEquals("Is Subject To Red Line", false, entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden);
		}

		public void TestRejectedMessageDoesNotResetRedLine()
		{
			entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden = true;
			EDIMessage iMDRMessage = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage.EM_MessageText = CMRImportDeclarationTestData.IMDRTransactionRejected;
			new IMDRMessageProcessor(logger).ProcessMessage(iMDRMessage);
			AssertEquals("Is Subject To Red Line", true, entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden);
			EDIMessage iMDRMessage2 = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage2.EM_MessageText = CMRImportDeclarationTestData.IMDRClear;
			new IMDRMessageProcessor(logger).ProcessMessage(iMDRMessage2);
			AssertEquals("Is Subject To Red Line", false, entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden);
		}

		public void TestAcknowledgementEmailGroup()
		{
			Env.Registry.AUCustoms.EdificeSendAcknowledgementsToGroup = Guid.NewGuid();
			BaseImportDeclarationMessageProcessorTestClass testClass = new BaseImportDeclarationMessageProcessorTestClass();
			AssertEquals("AcknowledgementEmailGroup", Env.Registry.AUCustoms.EdificeSendAcknowledgementsToGroup, testClass.AcknowledgementEmailGroupExposed);
		}

		public void TestAcknowledgementEmailMode()
		{
			Env.Registry.AUCustoms.EdificeSendAcknowledgements = "ENG";
			BaseImportDeclarationMessageProcessorTestClass testClass = new BaseImportDeclarationMessageProcessorTestClass();
			AssertEquals("AcknowledgementEmailMode", Env.Registry.AUCustoms.EdificeSendAcknowledgements, testClass.AcknowledgementEmailModeExposed);
		}

		public void TestImpedimentEmailGroup()
		{
			Env.Registry.AUCustoms.EdificeSendImpedimentsToGroup = Guid.NewGuid();
			BaseImportDeclarationMessageProcessorTestClass testClass = new BaseImportDeclarationMessageProcessorTestClass();
			AssertEquals("ImpedimentEmailGroup", Env.Registry.AUCustoms.EdificeSendImpedimentsToGroup, testClass.ImpedimentEmailGroupExposed);
		}

		public void TestImpedimentEmailMode()
		{
			Env.Registry.AUCustoms.EdificeSendImpediments = "NOE";
			BaseImportDeclarationMessageProcessorTestClass testClass = new BaseImportDeclarationMessageProcessorTestClass();
			AssertEquals("ImpedimentEmailMode", Env.Registry.AUCustoms.EdificeSendImpediments, testClass.ImpedimentEmailModeExposed);
		}

		public void TestErrorEmailGroup()
		{
			Env.Registry.AUCustoms.EdificeSendErrorsToGroup = Guid.NewGuid();
			BaseImportDeclarationMessageProcessorTestClass testClass = new BaseImportDeclarationMessageProcessorTestClass();
			AssertEquals("ErrorEmailGroup", Env.Registry.AUCustoms.EdificeSendErrorsToGroup, testClass.ErrorEmailGroupExposed);
		}

		public void TestErrorEmailMode()
		{
			Env.Registry.AUCustoms.EdificeSendErrors = "ENG";
			BaseImportDeclarationMessageProcessorTestClass testClass = new BaseImportDeclarationMessageProcessorTestClass();
			AssertEquals("ErrorEmailMode", Env.Registry.AUCustoms.EdificeSendErrors, testClass.ErrorEmailModeExposed);
		}

		protected override void SetUp()
		{
			base.SetUp();
			entryHeader = new CMRImportDeclarationTestData().GetCusEntryHeaderWithOneLine(Factory);
			entryLine1 = entryHeader.MergedLines.FindByLineNumber(1);
			declaration = entryHeader.Declaration;
			logger = new LoggingInformation();
		}

		protected JobDeclaration declaration;
		protected CusEntryHeader entryHeader;
		protected CusEntryLine entryLine1;
	}
}
