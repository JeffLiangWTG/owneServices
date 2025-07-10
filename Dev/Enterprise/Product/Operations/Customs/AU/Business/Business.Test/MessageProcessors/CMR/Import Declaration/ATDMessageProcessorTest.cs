using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ATDMessageProcessorTest : BaseImportDeclarationMessageProcessorTest
	{
		public void TestSettingEntryStatusForATDForOneEntryHeader()
		{
			EDIMessage iMDRMessage = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage.EM_MessageText = CMRImportDeclarationTestData.IMRHeld;
			new IMDRMessageProcessor(logger).ProcessMessage(iMDRMessage);
			AssertEquals("Entry Header Entry Status", CMRImportEntryAdvice.Held.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration Entry Status", CMRImportEntryAdvice.Held.Code, declaration.JE_EntryStatus);

			EDIMessage sAMMessage = entryHeader.Messages.AddNew(typeof(CMRSAMMessage));
			sAMMessage.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::SAM+4E0C B8IA6IB5:1+11'FTX+AHN+++CLEAR:CLEAR'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95+EAGLE DATAMATION INTERNATIONAL PTY:LIMIT'"
				+ "NAD+IM++ABX LOGISTICS (AUSTRALIA) PTY'RFF+ABO:B00122382/1/SYD1::2'RFF+ABT:AAAA6RHTE::2'RFF+ABQ:OWNERREF'RFF+ADU:B00148267'UNT+12+000001'";
			new SAMMessageProcessor(logger).ProcessMessage(sAMMessage);
			AssertEquals("Entry Header Entry Status", CMRImportEntryAdvice.Clear.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration Entry Status", CMRImportEntryAdvice.Clear.Code, declaration.JE_EntryStatus);

			EDIMessage aTDMessage = entryHeader.Messages.AddNew(typeof(CMRATDMessage));
			aTDMessage.EM_MessageText = CMRImportDeclarationTestData.ATD;
			new ATDMessageProcessor(logger).ProcessMessage(aTDMessage);
			AssertEquals("Entry Header Entry Status", CMRImportEntryAdvice.ATDReceived.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration Entry Status", CMRImportEntryAdvice.ATDReceived.Code, declaration.JE_EntryStatus);
		}

		public void TestSettingEntryStatusForATDForOneEntryHeader_ConsolidatedDeclaration()
		{
			GlbStaff.CurrentUser.GS_Code = User.ServiceUserCode;

			var consolidatedDeclaration = Customs.Business.Testing.ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			var leadDec = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			leadDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			leadDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var memberDeclaration = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			memberDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			memberDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = leadDec.EntryHeader;
			Factory.Save();

			var refNum = consolidatedDeclaration.CRD_JobReferenceNumber;
			entryHeader.CH_BGMReference = refNum;

			var imdrMessage = consolidatedDeclaration.Messages.AddNew(typeof(CMRIMDRMessage));
			imdrMessage.EM_MessageText = CMRImportDeclarationTestData.IMRHeld.Replace("B00122382/1", refNum);
			new IMDRMessageProcessor(logger).ProcessMessage(imdrMessage);
			AssertEquals("Entry Header IMDR Entry Status", CMRImportEntryAdvice.Held.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration IMDR Entry Status", CMRImportEntryAdvice.Held.Code, leadDec.JE_EntryStatus);

			var samMessage = consolidatedDeclaration.Messages.AddNew(typeof(CMRSAMMessage));
			samMessage.EM_MessageText = $"UNH+000001+CUSRES:D:99B:UN'BGM+961:::SAM+4E0C B8IA6IB5:1+11'FTX+AHN+++CLEAR:CLEAR'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95+EAGLE DATAMATION INTERNATIONAL PTY:LIMIT'"
				+ $"NAD+IM++ABX LOGISTICS (AUSTRALIA) PTY'RFF+ABO:{refNum}/SYD1::2'RFF+ABT:AAAA6RHTE::2'RFF+ABQ:OWNERREF'RFF+ADU:B00148267'UNT+12+000001'";
			new SAMMessageProcessor(logger).ProcessMessage(samMessage);
			AssertEquals("Entry Header SAM Entry Status", CMRImportEntryAdvice.Clear.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration SAM Entry Status", CMRImportEntryAdvice.Clear.Code, leadDec.JE_EntryStatus);

			var atdMessage = Factory.New<CMRATDMessage>();
			atdMessage.EM_MessageText = CMRImportDeclarationTestData.ATD.Replace("B00122382/1", refNum);

			new ATDMessageProcessor(logger).ProcessMessage(atdMessage);
			AssertEquals("Entry Header ATD Entry Status", CMRImportEntryAdvice.ATDReceived.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration ATD Entry Status", CMRImportEntryAdvice.ATDReceived.Code, leadDec.JE_EntryStatus);
			AssertEquals("Lead entry ATD Code", "AAAANNPTY", entryHeader.ATDSecurityCode);
			AssertEquals("Other entry ATD Code", "AAAANNPTY", memberDeclaration.EntryHeader.ATDSecurityCode);
		}

		public void TestSettingEntryStatusForATDWithMultiHeaders()
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

			EDIMessage aTDMessageForHeader1 = entryHeader.Messages.AddNew(typeof(CMRATDMessage));
			aTDMessageForHeader1.EM_MessageText = CMRImportDeclarationTestData.ATD;
			new ATDMessageProcessor(logger).ProcessMessage(aTDMessageForHeader1);
			AssertEquals("Entry Header Entry Status", CMRImportEntryAdvice.ATDReceived.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Entry Header 2 Entry Status", CMRImportEntryAdvice.ATDReceived.Code, entryHeader2.CH_EntryStatus);
			AssertEquals("Declaration Entry Status", CMRImportEntryAdvice.ATDReceived.Code, declaration.JE_EntryStatus);
		}

		public void TestConsolidatedATDMessageStatus()
		{
			GlbStaff.CurrentUser.GS_Code = User.ServiceUserCode;

			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 3);
			var leadDec = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			leadDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			leadDec.JE_MessageType = JobMessageTypeList.Codes.Import;

			var consolidatedDec1 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			consolidatedDec1.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			consolidatedDec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			consolidatedDec1.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;

			var consolidatedDec2 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[2];
			consolidatedDec2.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			consolidatedDec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			consolidatedDec2.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;

			var consolidatedDec3 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[3];
			consolidatedDec3.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			consolidatedDec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			consolidatedDec3.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			Factory.Save();

			var refNum = consolidatedDeclaration.CRD_JobReferenceNumber;
			var entryHeader = leadDec.EntryHeader;
			entryHeader.CH_BGMReference = refNum;

			var imdrMessage = consolidatedDeclaration.Messages.AddNew(typeof(CMRIMDRMessage));
			imdrMessage.EM_MessageText = CMRImportDeclarationTestData.IMRHeld.Replace("B00122382/1", refNum);
			new IMDRMessageProcessor(logger).ProcessMessage(imdrMessage);
			AssertEquals("Entry Header IMDR Entry Status", CMRImportEntryAdvice.Held.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration IMDR Entry Status", CMRImportEntryAdvice.Held.Code, leadDec.JE_EntryStatus);

			var samMessage = consolidatedDeclaration.Messages.AddNew(typeof(CMRSAMMessage));
			samMessage.EM_MessageText = $"UNH+000001+CUSRES:D:99B:UN'BGM+961:::SAM+4E0C B8IA6IB5:1+11'FTX+AHN+++CLEAR:CLEAR'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95+EAGLE DATAMATION INTERNATIONAL PTY:LIMIT'"
				+ $"NAD+IM++ABX LOGISTICS (AUSTRALIA) PTY'RFF+ABO:{refNum}/SYD1::2'RFF+ABT:AAAA6RHTE::2'RFF+ABQ:OWNERREF'RFF+ADU:B00148267'UNT+12+000001'";
			new SAMMessageProcessor(logger).ProcessMessage(samMessage);
			AssertEquals("Entry Header SAM Entry Status", CMRImportEntryAdvice.Clear.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration SAM Entry Status", CMRImportEntryAdvice.Clear.Code, leadDec.JE_EntryStatus);

			var atdResponseMessage = Factory.New<CMRATDMessage>();
			atdResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			atdResponseMessage.EM_MessageType = CMRMessage.CMRMessageTypes.ATD;
			atdResponseMessage.EM_MessageText = CMRImportDeclarationTestData.ATD.Replace("B00122382/1", refNum);
			new ATDMessageProcessor(logger).ProcessMessage(atdResponseMessage);
			Factory.Save();

			Assert("Consolidated Declaration has the ATD response message attached", consolidatedDeclaration.Messages.Contains(atdResponseMessage));

			AssertEquals("Consolidated lead declaration Entry Status", CMRImportEntryAdvice.ATDReceived.Code, leadDec.JE_EntryStatus);
			AssertEquals("Ensure the ‘Authority to Deal’ Message Status is synchronized across the consolidated entry and attached declarations - consolidatedDec1 Entry Status", CMRImportEntryAdvice.ATDReceived.Code, consolidatedDec1.JE_EntryStatus);
			AssertEquals("consolidatedDec2 Entry Status", CMRImportEntryAdvice.ATDReceived.Code, consolidatedDec2.JE_EntryStatus);
			AssertEquals("consolidatedDec3 Entry Status", CMRImportEntryAdvice.ATDReceived.Code, consolidatedDec3.JE_EntryStatus);

			AssertEquals("Lead entry has ATD Security Code from consolidated entry message", "AAAANNPTY", entryHeader.ATDSecurityCode);
			AssertEquals("consolidatedDec1 has the same ATD Code", "AAAANNPTY", consolidatedDec1.EntryHeader.ATDSecurityCode);
			AssertEquals("consolidatedDec2 has the same ATD Code", "AAAANNPTY", consolidatedDec2.EntryHeader.ATDSecurityCode);
			AssertEquals("consolidatedDec3 has the same ATD Code", "AAAANNPTY", consolidatedDec3.EntryHeader.ATDSecurityCode);
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.ATD;

		protected override ZString GetExpectedMessageName() => "Authority to Deal Message (ATD)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => new ATDMessageProcessor(logger);

		protected override Type IncomingMessageType => typeof(CMRATDMessage);
	}
}
