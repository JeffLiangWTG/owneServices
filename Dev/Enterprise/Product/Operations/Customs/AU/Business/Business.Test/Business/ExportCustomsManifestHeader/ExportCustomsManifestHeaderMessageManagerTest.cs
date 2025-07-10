using System.Collections.Specialized;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ExportCustomsManifestHeaderMessageManagerTest : DeclarationsAndShipmentsCreatedCancelledTestCase
	{
		public void TestOriginalMessageBuilder()
		{
			Header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			AssertEquals("MessageBuilderType", typeof(EMMMessageBuilder), messageManager.GetOriginalManifestBuilder(Header).GetType());
			Header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			AssertEquals("MessageBuilderType", typeof(ESMMessageBuilder), messageManager.GetOriginalManifestBuilder(Header).GetType());
			Header.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			AssertEquals("MessageBuilderType", typeof(ESMMessageBuilder), messageManager.GetOriginalManifestBuilder(Header).GetType());
		}

		public void TestGetReplacementMessageBuilder()
		{
			Header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			AssertEquals("MessageBuilderType", typeof(EMMMessageBuilder), messageManager.GetReplacementManifestBuilder(Header).GetType());
			Header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			AssertEquals("MessageBuilderType", typeof(ESMMessageBuilder), messageManager.GetReplacementManifestBuilder(Header).GetType());
			Header.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			AssertEquals("MessageBuilderType", typeof(ESMMessageBuilder), messageManager.GetReplacementManifestBuilder(Header).GetType());
		}

		public void TestWithdrawalMessageBuilder()
		{
			Header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			AssertEquals("MessageBuilderType", typeof(EMMMessageBuilder), messageManager.GetWithdrawalManifestBuilder(Header).GetType());
			Header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			AssertEquals("MessageBuilderType", typeof(ESMMessageBuilder), messageManager.GetWithdrawalManifestBuilder(Header).GetType());
			Header.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			AssertEquals("MessageBuilderType", typeof(ESMMessageBuilder), messageManager.GetWithdrawalManifestBuilder(Header).GetType());
		}

		public void TestSendAManifestAmmendmentIfNeededIfNoMessagesSent()
		{
			Header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			var sender = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			var result = messageManager.SendAManifestAmendmentIfNeeded(sender);
			Assert(result);
			AssertEquals("MessageCount", 0, Header.Messages.Count);
		}

		public void TestSendAManifestAmmendmentIfNeededIfMessageSentAndNoChangesToMessage()
		{
			Header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			Header.ED_TransportMode = Core.Constants.TransportModes.Air;
			Header.Messages.AddNew(typeof(CMREMMMessage));
			Header.Messages[0].EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			Factory.Save();
			Header.ED_VesselName = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Code;
			var sender = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			AssertEquals("MessageCount", 1, Header.Messages.Count);
			var result = messageManager.SendAManifestAmendmentIfNeeded(sender);
			Assert(result);
			AssertEquals("MessageCount", 1, Header.Messages.Count);
		}

		public void TestSendAManifestAmmendmentIfNeededIfMessageSentAndChangesToMessageAnswerNoToSave()
		{
			Header.ED_CAN = "123456789";
			Header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			Header.ED_TransportMode = Core.Constants.TransportModes.Air;
			//				Header.Messages.AddNew(typeof(CMREMMMessage));
			//				Header.Messages[0].EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			//				Factory.Save();
			Header.ED_FlightNumber = "QF123";
			var sender = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			sender.AnswerToContinueWithAction = false;
			AssertEquals("MessageCount", 0, Header.Messages.Count);
			var result = messageManager.SendAManifestAmendmentIfNeeded(sender);
			Assert(!result);
			AssertEquals("MessageCount", 0, Header.Messages.Count);
		}

		public void TestSendAManifestAmmendmentIfNeededIfMessageSentAndChangesToMessageAnswerYesToSave()
		{
			Header.ED_CAN = "123456789";
			Header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			Header.ED_TransportMode = Core.Constants.TransportModes.Air;
			//				Header.Messages.AddNew(typeof(CMREMMMessage));
			//				Header.Messages[0].EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			//				Header.Messages[0].EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			//				Factory.Save();
			Header.ED_FlightNumber = "QF123";
			var sender = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			sender.AnswerToContinueWithAction = true;
			AssertEquals("MessageCount", 0, Header.Messages.Count);
			var result = messageManager.SendAManifestAmendmentIfNeeded(sender);
			Assert(result);
			AssertEquals("MessageCount", 1, Header.Messages.Count);
		}

		public void TestSendAManifestAmmendmentIfNeededIfMessageSentAndChangesToMessageAndWaitingForResponse()
		{
			//Header.ED_CAN = "123456789";
			Header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			Header.ED_TransportMode = Core.Constants.TransportModes.Air;
			Header.Messages.AddNew(typeof(CMREMMMessage));
			Header.Messages[0].EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			Header.Messages[0].EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Factory.Save();
			Header.ED_FlightNumber = "QF123";
			var sender = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			AssertEquals("MessageCount", 1, Header.Messages.Count);
			var result = messageManager.SendAManifestAmendmentIfNeeded(sender);
			Assert(!result);
			AssertEquals("MessageCount", 1, Header.Messages.Count);
		}

		public void TestWeCantSendASeaDepartureReportWithoutAnABNOrCCIDConfigured()
		{
			Header.ED_ManifestType = ManifestTypeList.Codes.DepartureReport;
			Header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(Enterprise.MasterFiles.Business.OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345");
			Assert("NoErrors", messageManager.GetCommonReasonForNotSendingADepartureReportMessage().Count == 0);
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(Enterprise.MasterFiles.Business.OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, ZString.Empty);
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(Enterprise.MasterFiles.Business.OrgCusCode.CodeTypes.CustomsClientCode, ZString.Empty);
			Assert("Errors", messageManager.GetCommonReasonForNotSendingADepartureReportMessage().Count > 0);
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(Enterprise.MasterFiles.Business.OrgCusCode.CodeTypes.CustomsClientCode, "12345");
			Assert("NoErrors", messageManager.GetCommonReasonForNotSendingADepartureReportMessage().Count == 0);
		}

		public void TestCantSendIfMissingCMRMailbox()
		{
			var oldRegNum = GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;
			try
			{
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "12345";
				Assert("NoErrors", messageManager.GetCommonReasonForNotSendingAMessage().Count == 0);
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
				Assert("Errors", messageManager.GetCommonReasonForNotSendingAMessage().Count > 0);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = oldRegNum;
			}
		}

		public void TestCantSendIfMissingCertificate()
		{
			Assert("NoErrors", messageManager.GetCommonReasonForNotSendingAMessage().Count == 0);
			Env.Registry.AUCCompanyCertificateData = System.Array.Empty<byte>();
			Env.Registry.AUCCompanyCertificatePassword = ZString.Empty;
			Assert("Errors", messageManager.GetCommonReasonForNotSendingAMessage().Count > 0);
		}

		public void TestCantSendIfMissingPassword()
		{
			Assert("NoErrors", messageManager.GetCommonReasonForNotSendingAMessage().Count == 0);
			Env.Registry.AUCCompanyCertificatePassword = ZString.Empty;
			Assert("Errors", messageManager.GetCommonReasonForNotSendingAMessage().Count > 0);
		}

		public void TestIsWaitingForManifestResponse()
		{
			Header.ED_ManifestType = "EMM";
			Assert(!Header.IsWaitingForManifestResponse);
			Header.Messages.AddNew(typeof(CMREMMMessage));
			Assert(Header.IsWaitingForManifestResponse);
		}

		public void TestIsWaitingForManifestResponseWithDepartureReportResponse()
		{
			Header.Messages.AddNew(typeof(CMRDEPARTMessage));
			Assert(!Header.IsWaitingForManifestResponse);
			Header.Messages.AddNew(typeof(CMREMMMessage));
			Header.ED_ManifestType = "EMM";
			Assert(Header.IsWaitingForManifestResponse);
		}

		public void TestIsWaitingForDepartureReportResponse()
		{
			var header = Factory.New<ExportCustomsManifestHeader>();
			Assert(!header.IsWaitingForDepartureReportResponse);
			header.Messages.AddNew(typeof(CMRDEPARTMessage));
			Assert(header.IsWaitingForDepartureReportResponse);
		}

		public void TestIsWaitingForDepartureReportResponseWithManifestResponse()
		{
			var header = Factory.New<ExportCustomsManifestHeader>();
			header.Messages.AddNew(typeof(CMREMMMessage));
			Assert(!header.IsWaitingForDepartureReportResponse);
			header.Messages.AddNew(typeof(CMRDEPARTMessage));
			Assert(header.IsWaitingForDepartureReportResponse);
		}

		public void TestIsManifestDeclaredAtCustoms()
		{
			var header = Factory.New<ExportCustomsManifestHeader>();
			Assert("!IsManifestDeclaredAtCustoms", !header.IsManifestDeclaredAtCustoms);
			header.ED_CAN = "AAAAAAAAA";
			Assert("IsManifestDeclaredAtCustoms", header.IsManifestDeclaredAtCustoms);
		}

		public void TestWeGetWarningWhenWithdrawingZeroLiner()
		{
			Header.Lines.AddNew();
			var result1 = messageManager.GetWarningsAboutWithdrawingTheManifest();
			Header.Lines.RemoveAll();
			var result2 = messageManager.GetWarningsAboutWithdrawingTheManifest();
			AssertEquals("WarningDifference", 1, result1.Count - result2.Count);
		}

		public void TestWeCantWithdrawTheManifestIfWeDontHaveACAN()
		{
			Header.Lines.AddNew();
			Header.HasChanges = true;
			var result1 = messageManager.GetAnyReasonsWeCantWithdrawTheManifest();
			Header.ED_CAN = "123456789";
			var result2 = messageManager.GetAnyReasonsWeCantWithdrawTheManifest();
			AssertEquals("ErrorDifference", 1, result1.Count - result2.Count);
		}

		public void TestWeCantDeclareTheManifestIfWeHaveACAN()
		{
			Header.Lines.AddNew();
			Header.ED_CAN = "123456789";
			var result1 = messageManager.GetAnyReasonsWeCantDeclareTheManifest();
			Header.ED_CAN = ZString.Empty;
			var result2 = messageManager.GetAnyReasonsWeCantDeclareTheManifest();
			AssertEquals("ErrorDifference", 1, result1.Count - result2.Count);
		}

		public void TestIsDepartureReportDeclaredAtCustoms()
		{
			Assert("NotDeclared", !Header.IsDepartureReportDeclaredAtCustoms);
			SimulateOutgoingMessage(Header, CMRMessage.CMRMessageTypes.DEPART, CMRMessage.MessageSubTypes.Original);
			Assert("NotDeclared", !Header.IsDepartureReportDeclaredAtCustoms);
			SimulateIncomingMessage(Header, CMRMessage.CMRMessageTypes.DEPART, CMRMessage.ManifestResponseSubTypes.Rejected);
			Assert("NotDeclared", !Header.IsDepartureReportDeclaredAtCustoms);
			SimulateOutgoingMessage(Header, CMRMessage.CMRMessageTypes.DEPART, CMRMessage.MessageSubTypes.Original);
			SimulateIncomingMessage(Header, CMRMessage.CMRMessageTypes.DEPART, CMRMessage.ManifestResponseSubTypes.Error);
			Assert("Declared", Header.IsDepartureReportDeclaredAtCustoms);
			SimulateOutgoingMessage(Header, CMRMessage.CMRMessageTypes.DEPART, CMRMessage.MessageSubTypes.Amendment);
			SimulateIncomingMessage(Header, CMRMessage.CMRMessageTypes.DEPART, CMRMessage.ManifestResponseSubTypes.Rejected);
			Assert("Declared", Header.IsDepartureReportDeclaredAtCustoms);
			SimulateOutgoingMessage(Header, CMRMessage.CMRMessageTypes.DEPART, CMRMessage.MessageSubTypes.Withdraw);
			Assert("Declared", Header.IsDepartureReportDeclaredAtCustoms);
			SimulateInterchangeRejection(Header);
			Assert("Declared", Header.IsDepartureReportDeclaredAtCustoms);
			SimulateOutgoingMessage(Header, CMRMessage.CMRMessageTypes.DEPART, CMRMessage.MessageSubTypes.Withdraw);
			SimulateIncomingMessage(Header, CMRMessage.CMRMessageTypes.DEPART, CMRMessage.ManifestResponseSubTypes.Rejected);
			Assert("Declared", Header.IsDepartureReportDeclaredAtCustoms);
			SimulateOutgoingMessage(Header, CMRMessage.CMRMessageTypes.DEPART, CMRMessage.MessageSubTypes.Withdraw);
			SimulateIncomingMessage(Header, CMRMessage.CMRMessageTypes.DEPART, CMRMessage.ManifestResponseSubTypes.Withdrawn);
			Assert("NotDeclared", !Header.IsDepartureReportDeclaredAtCustoms);
		}

		public void TestDeclareManifestMessageType()
		{
			var sender = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			var manager = new ExportCustomsManifestHeaderMessageManagerForTest(Header);
			manager.DeclareManifest(sender);
			AssertEquals(Common.MessageBuilders.MessageSubTypes.Create, ((ESMMessageBuilder)manager.BuilderDelegates[0](Header)).MessageSubType);
		}

		public void TestWithdrawManifestSendsTwoMessages()
		{
			Header.ED_CAN = "12345";
			Header.HasChanges = false;
			var sender = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			var manager = new ExportCustomsManifestHeaderMessageManagerForTest(Header);
			manager.WithdrawManifest(sender);

			AssertEquals("Number of Builders", 2, manager.BuilderDelegates.Length);

			var builder1 = (CMRManifestMessageBuilder)manager.BuilderDelegates[0](Header);
			var builder2 = (CMRManifestMessageBuilder)manager.BuilderDelegates[1](Header);
			AssertEquals("DontSendAnyLines", true, builder1.DontSendAnyLines);
			var message1 = builder1.PopulateMessagesReturningResult();
			var message2 = builder2.PopulateMessagesReturningResult();

			AssertEquals("EM_MessageSubType", CMRMessage.MessageSubTypes.Amendment, message1.EM_MessageSubType);
			AssertEquals("EM_MessageSubType", CMRMessage.MessageSubTypes.Withdraw, message2.EM_MessageSubType);

			AssertEquals("Status", EDIMessage.Status.Queued, message1.EM_Status);
			AssertEquals("Status", EDIMessage.Status.Pending, message2.EM_Status);
		}

		public void TestResetToOriginal()
		{
			Header.Messages.RemoveAndDeleteAll();
			var message1 = Header.Messages.AddNew();
			var message2 = Header.Messages.AddNew();
			message1.EM_Status = "ABC";
			message2.EM_Status = "DEF";
			var sender = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			var manager = new ExportCustomsManifestHeaderMessageManagerForTest(Header);
			manager.ResetToOriginal(sender);
			AssertEquals(EDIMessage.Status.Discarded, message1.EM_Status);
			AssertEquals(EDIMessage.Status.Discarded, message2.EM_Status);
			AssertEquals("Save should be enabled after resetting to original", true, Header.Messages.HasChanges);
		}

		public void TestShouldSendMessagesInTestMode()
		{
			Env.Registry.CMRTestMode = false;
			var manager = new ExportCustomsManifestHeaderMessageManagerForTest(Header);
			Assert(!manager.ShouldSendMessagesInTestMode);
			Env.Registry.CMRTestMode = true;
			Assert(manager.ShouldSendMessagesInTestMode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			messageManager = Factory.New<ExportCustomsManifestHeader>().MessageManager;
			preGST = GlbCompany.CurrentCompany.OrgProxy.LocalBusinessRegNo;
			preCCC = GlbCompany.CurrentCompany.OrgProxy.LocalCustomsClientCode;
			Customs.Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();
		}

		ZString preGST;
		ZString preCCC;
		ExportCustomsManifestHeaderMessageManager messageManager;

		protected override void TearDown()
		{
			base.TearDown();
			RestoreCodes();
			AssertEquals("GST", preGST, GlbCompany.CurrentCompany.OrgProxy.LocalBusinessRegNo);
			AssertEquals("CCC", preCCC, GlbCompany.CurrentCompany.OrgProxy.LocalCustomsClientCode);
		}

		ExportCustomsManifestHeader Header => messageManager.header;

		EDIMessage SimulateOutgoingMessage(ExportCustomsManifestHeader header, ZString messageType, ZString messageSubType)
		{
			var lastMessage = header.Messages.LastMessage;
			var lastMessageTime = lastMessage == null ? ZDateTime.Empty : Env.Time.GetLocalTimeFromUtc(lastMessage.EM_SystemCreateTimeUtc.ToDateTime());
			var message = header.Messages.AddNew(typeof(CMRMessage));
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_SystemCreateTimeUtc = lastMessageTime.IsEmpty ? ZDateTime.Now : lastMessageTime.AddMinutes(5);
			return message;
		}

		void SimulateIncomingMessage(ExportCustomsManifestHeader header, ZString messageType, ZString messageSubType)
		{
			var message = SimulateOutgoingMessage(header, messageType, messageSubType);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}

		void SimulateInterchangeRejection(ExportCustomsManifestHeader header)
		{
			header.Messages[header.Messages.Count - 1].EM_Status = EDIMessage.Status.Rejected;
		}

		void RestoreCodes()
		{
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, preGST);
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, preCCC);
		}

		sealed class ExportCustomsManifestHeaderMessageManagerForTest : ExportCustomsManifestHeaderMessageManager
		{
			public ExportCustomsManifestHeaderMessageManagerForTest(ExportCustomsManifestHeader header)
				: base(header)
			{
			}

			protected override bool SendMessage(Customs.Business.ISendsMessagesToCustoms sender, StringCollection errors, StringCollection warnings, MessageBuilderDelegate[] builderDelegates, string messageName, CancellationToken token)
			{
				Sender = sender;
				Errors = errors;
				Warnings = warnings;
				BuilderDelegates = builderDelegates;
				MessageName = messageName;
				return true;
			}

			internal new bool ShouldSendMessagesInTestMode => base.ShouldSendMessagesInTestMode;
			internal Customs.Business.ISendsMessagesToCustoms Sender;
			internal StringCollection Errors;
			internal StringCollection Warnings;
			internal MessageBuilderDelegate[] BuilderDelegates;
			internal string MessageName;
		}
	}
}
