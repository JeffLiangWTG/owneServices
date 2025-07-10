using System;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	class MessageAcceptedMessageProcessorTestCase : TestCaseWithFactory
	{
		public void TestGetEmailRegistryValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var processor = new MessageAcceptedMessageProcessorForTesting(logger, universalEvent, message, entry);
			AssertEquals("newGroupMM", newGroupNN.PK, processor.NotifyEmailGroup_Exposed);
			AssertEquals("email Mode", Core.Constants.EmailTo.StaffMemberAndNominatedGroup, processor.NotifyEmailMode_Exposed);
		}

		public void TestUpdateEntryStatusAfterAcceptedMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00229998";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "";
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingChange;
			entryHeader.CH_BGMReference = "10207003504232";
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

			var message = Factory.New<UniversalEventMessage>();
			message.EM_MessageData = Encoding.ASCII.GetBytes(messageAcceptedJob);

			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new MessageAcceptedMessageProcessorForTesting(logger, universalEvent, message, entryHeader);

			AssertEquals(string.Empty, entryHeader.CH_EntryStatus);
			processor.Process();

			AssertEquals("Message Status", MessageStatusList.Codes.ClearChange, entryHeader.CH_Status);
			AssertEquals("Entry Status", string.Empty, entryHeader.CH_EntryStatus);
		}
		public string messageAcceptedJob = @"<UniversalEvent xmlns:s0=""http://www.cargowise.com/Schemas/Universal/2011/11""><Event><DataContext><DataTargetCollection><DataTarget><Type>CAIntegratedImportDeclaration</Type><Key>10207003504232</Key></DataTarget></DataTargetCollection><RecipientRoleCollection><RecipientRole><Code>CD4</Code><Description>CA Customs IID/D4 Status Notice</Description></RecipientRole></RecipientRoleCollection></DataContext><EventTime>2024-08-9T10:12:00</EventTime><EventType>MAA</EventType><EventReference>IID Accepted</EventReference><ContextCollection><Context><Type>InterchangeNumber</Type><Value>145</Value></Context><Context><Type>MessageNumber</Type><Value>1</Value></Context><Context><Type>IsTest</Type><Value>Y</Value></Context><Context><Type>OrganizationReference</Type><Value>B00229998</Value></Context><Context><Type>SentRawMessage</Type><Value>UNH+20240809000001+GOVCBR:D:13A:UN:IID+4.02'BGM+929+10207003504232+4'DTM+132:202408071807:203'MOA+134:15000:CAD'RFF+ABO:B00229998'GOR++5'LOC+23+0453+9453:::CUSTOMS GENERIC - WINDSOR - AMBASSA'NAD+IM+836006668RM0001++OFFICAL CUSTOMS NAME+999 Howerd AVE+WINDSOR+ON+N9E3N8+CA'CTA+IC+:PGA Contact'COM+cedomir.bekic@gmail.com:EM'COM+15198177045:TE'COM+15198177666:FX'NAD+CB+10207++CANADIAN CUSTOMS BROKER+2701 LOMBARDY CRES WHSE+LA SALLE+ON+N9H2L7+CA'CTA+IC+:Craig Seelig'COM+craig.seelig@wisetechglobal.com:EM'COM+12155551234:TE'COM+12153338888:FX'UNS+D'SEQ+1'RFF+CN:8083HSE080424Z'NAD+VN+++ACE TEST HK+174 GLOUCESTER ROAD WAN CHAI DISTRI:CT+HONG KONG+++HK'CTA+IC+:Ian Chen'COM+ian.chen@fisherman.com:EM'COM+61888888999:TE'NAD+UC+836006668RM0001++WINDSOR IMPORTER TEST.+3801 HOWARD AVE+WINDSOR+ON+N9E3N8+CA'CTA+IC+:PGA Contact'COM+cedomir.bekic@gmail.com:EM'COM+15198177045:TE'COM+15198177666:FX'LOC+35+HK'LOC+277+HK'DTM+757:20240806:102'DOC+380+INV080724Z'SEQ+1'DTM+3:20240806:102'MOA+39:5000:CAD'MEA+AAE+AAB+KGM:100'QTY+47:300:EA'LIN+1'LOC+27+HK'PAC+5+4+PK'SEQ+1'GID+1'IMD++8+:::STYRENE-ACRYLONITRILE (SAN) COPOLYMERS (POLYMERS OF STYRENE, IN PRIMARY FORMS.)'MOA+146:16.67:CAD'MOA+66:5000:CAD'TCC+++3903200000:HS'CNT+51:300:EA'LOC+27+HK'SEQ+1'NAD+MF+++ACE TEST HK+174 GLOUCESTER ROAD WAN CHAI DISTRI:CT+HONG KONG+++HK'CTA+IC'COM+2252123456789:TE'SEQ+1'RFF+CN:8083HSE080724X'NAD+VN+++ACE TEST HK+174 GLOUCESTER ROAD WAN CHAI DISTRI:CT+HONG KONG+++HK'CTA+IC+:Ian Chen'COM+ian.chen@fisherman.com:EM'COM+61888888999:TE'NAD+UC+836006668RM0001++WINDSOR IMPORTER TEST.+3801 HOWARD AVE+WINDSOR+ON+N9E3N8+CA'CTA+IC+:PGA Contact'COM+cedomir.bekic@gmail.com:EM'COM+15198177045:TE'COM+15198177666:FX'LOC+35+HK'LOC+277+HK'DTM+757:20240806:102'DOC+380+INV080724X'SEQ+1'DTM+3:20240806:102'MOA+39:5000:CAD'MEA+AAE+AAB+KGM:100'QTY+47:100:EA'LIN+2'LOC+27+HK'PAC+5+4+PK'SEQ+1'GID+1'IMD++8+:::GOODS FOR X'MOA+146:50:CAD'MOA+66:5000:CAD'TCC+++3903200000:HS'CNT+51:100:EA'LOC+27+HK'SEQ+1'NAD+MF+++ACE TEST HK+174 GLOUCESTER ROAD WAN CHAI DISTRI:CT+HONG KONG+++HK'CTA+IC'COM+2252123456789:TE'SEQ+1'RFF+CN:8083HSE080724Y'NAD+VN+++ACE TEST HK+174 GLOUCESTER ROAD WAN CHAI DISTRI:CT+HONG KONG+++HK'CTA+IC+:Ian Chen'COM+ian.chen@fisherman.com:EM'COM+61888888999:TE'NAD+UC+836006668RM0001++WINDSOR IMPORTER TEST.+3801 HOWARD AVE+WINDSOR+ON+N9E3N8+CA'CTA+IC+:PGA Contact'COM+cedomir.bekic@gmail.com:EM'COM+15198177045:TE'COM+15198177666:FX'LOC+35+HK'LOC+277+HK'DTM+757:20240806:102'DOC+380+INV080724Y'SEQ+1'DTM+3:20240806:102'MOA+39:5000:CAD'MEA+AAE+AAB+KGM:100'QTY+47:200:EA'LIN+3'LOC+27+HK'PAC+5+4+PK'SEQ+1'GID+1'IMD++8+:::GOODS FOR Y'MOA+146:25:CAD'MOA+66:5000:CAD'TCC+++3903200000:HS'CNT+51:200:EA'LOC+27+HK'SEQ+1'NAD+MF+++ACE TEST HK+174 GLOUCESTER ROAD WAN CHAI DISTRI:CT+HONG KONG+++HK'CTA+IC'COM+2252123456789:TE'HYN+3'UNS+S'UNT+126+20240809000001'</Value></Context><Context><Type>ResponseRawMessage</Type><Value>UNB+UNOC:3+RCCECECPW+YUSAIRXPN+240809:1013+145'UNG+GOVCBR+IIDT+U10207V2+20240809:1013+127+UN+D:13A'UNH+1+GOVCBR:D:13A:UN'BGM+312+10207003504232'DTM+9:202408091012:203'RFF+AGO:B00229998'UNS+S'HYN+3'UNS+S'UNT+8+1'UNE+1+127'UNZ+1+145'</Value></Context></ContextCollection></Event></UniversalEvent>";

		public void TestGetAssociatedBusinessObjectDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var expectedAssociatedBusinessObjectDescription = "A 'Message Accepted and Passed Application Edits' response has been received from the CBSA for a(n) Test IID Declaration.";
			var house = Factory.New<CusCAeMHHouse>();
			var message = Factory.New<UniversalEventMessage>();
			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new MessageAcceptedMessageProcessorForTesting(logger, universalEvent, message, entry);
			AssertEquals("GetAssociatedBusinessObjectDescription", expectedAssociatedBusinessObjectDescription, processor.AssociatedBusinessObjectDescription_Exposed);
		}

		protected class MessageAcceptedMessageProcessorForTesting : MessageAcceptedMessageProcessor
		{
			public MessageAcceptedMessageProcessorForTesting(IXmlSessionTracker logger, UniversalEvent universalEvent, UniversalEventMessage message, CusEntryHeader entry)
				: base(logger, universalEvent, message, entry)
			{ }

			public ZGuid NotifyEmailGroup_Exposed => base.NotifyEmailGroup;

			public ZString NotifyEmailMode_Exposed => base.NotifyEmailMode;

			public ZString AssociatedBusinessObjectDescription_Exposed => base.GetAssociatedBusinessObjectDescription();
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			message = Factory.New<UniversalEventMessage>();
			universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();

			newGroupNN = Factory.New<GlbGroup>();
			newGroupNN.GG_Code = "NN1";

			CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgements.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgementsToGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newGroupNN.PK.ToGuid());
		}

		GlbGroup newGroupNN;
		IXmlSessionTracker logger;
		UniversalEventMessage message;
		UniversalEvent universalEvent;
	}
}
