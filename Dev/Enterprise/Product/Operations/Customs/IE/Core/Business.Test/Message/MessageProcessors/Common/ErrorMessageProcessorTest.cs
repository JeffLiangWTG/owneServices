using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Testing
{
	class ErrorMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "Error Message", new ErrorMessageProcessor(new LoggingInformation()).MessageFriendlyName);
		}

		public void TestProcess_NoOriginalMatch_TransactionID()
		{
			var staff1 = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			var staff2 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "!2@", "Staff 2", "staff2@where.com");
			var (entryHeader, outgoingMessage, incomingMessage, processor) = CreateOriginalExportData();
			outgoingMessage.EM_SystemCreateUser = staff1.GS_Code;
			incomingMessage.EM_ApplicationReference = "HELLOWORLD123";
			incomingMessage.EM_MessageType = AESOutgoingMessageTypeList.Codes.ExportOriginal;
			entryHeader.Declaration.JE_GS_NKCusAgent = staff2.GS_Code;
			Factory.Save();
			using (Factory.AddDisposableService())
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				processor.ProcessMessage(incomingMessage);

				CombineAssertions("Result for processing ERROR message with no original match.", () =>
				{
					AssertEquals("CusEntryHeader.CH_Status", "FAL", entryHeader.CH_Status);
					var messageTypeDesc = incomingMessage.MessageTypeWithDescription;
					MessageProcessorNotificationTestHelper.AssertEmail($"{messageTypeDesc} Response (Failure) for {entryHeader.Declaration.JE_DeclarationReference}", new[] { $@"Submission was unsuccessful<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Transaction ID</td><td>HELLOWORLD123</td></tr><tr><td>Message Type</td><td>{messageTypeDesc}</td></tr><tr><td>Message Number</td><td>&nbsp;</td></tr></table><br />Failure Notification for 480036." }, new[] { staff2.GS_EmailAddress.ToString() });
				});
			}
		}

		public void TestProcess_NoOriginalMatch_ApplicationCode()
		{
			var staff1 = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			var staff2 = MessageProcessorNotificationTestHelper.CreateStaff(Factory, "!2@", "Staff 2", "staff2@where.com");
			var (entryHeader, outgoingMessage, incomingMessage, processor) = CreateOriginalExportData();
			outgoingMessage.EM_SystemCreateUser = staff1.GS_Code;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsImport;
			entryHeader.Declaration.JE_GS_NKCusAgent = staff2.GS_Code;
			Factory.Save();
			using (Factory.AddDisposableService())
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				processor.ProcessMessage(incomingMessage);

				CombineAssertions("Result for processing ERROR message with no original match.", () =>
				{
					AssertEquals("CusEntryHeader.CH_Status", "FAL", entryHeader.CH_Status);
					var messageTypeDesc = incomingMessage.MessageTypeWithDescription;
					MessageProcessorNotificationTestHelper.AssertEmail($"{messageTypeDesc} Response (Failure) for {entryHeader.Declaration.JE_DeclarationReference}", new[] { $@"Submission was unsuccessful<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Transaction ID</td><td>{transactionId}</td></tr><tr><td>Message Type</td><td>{messageTypeDesc}</td></tr><tr><td>Message Number</td><td>&nbsp;</td></tr></table><br />Failure Notification for 480036." }, new[] { staff2.GS_EmailAddress.ToString() });
				});
			}
		}

		public void TestProcess_NoLinkObject()
		{
			var staff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			var (entryHeader, outgoingMessage, incomingMessage, processor) = CreateOriginalExportData();
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			incomingMessage.EM_LinkedObject = null;
			Factory.Save();
			using (Factory.AddDisposableService())
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				processor.ProcessMessage(incomingMessage);

				CombineAssertions("Result for processing ERROR message.", () =>
				{
					AssertEquals("CusEntryHeader.CH_Status", "SNT", entryHeader.CH_Status);
					AssertEquals("No email", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				});
			}
		}

		public void TestProcessError_Export()
		{
			var staff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			var emailGroup = EUCustomsDataRegistry.Instance.SendExportAcknowledgements.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			emailGroup.SendMode = Core.Constants.EmailTo.NominatedGroup;
			emailGroup.SendGroupPK = MessageProcessorNotificationTestHelper.SetupStaffGroup(Factory).PK;

			var (entryHeader, outgoingMessage, incomingMessage, processor) = CreateOriginalExportData();
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;

			Factory.Save();
			using (EUCustomsDataRegistry.Instance.SendExportAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, emailGroup))
			using (Factory.AddDisposableService())
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				processor.ProcessMessage(incomingMessage);

				CombineAssertions("Result for processing ERROR message.", () =>
				{
					AssertEquals("CusEntryHeader.CH_Status", "FAL", entryHeader.CH_Status);
					var messageTypeDesc = outgoingMessage.MessageTypeWithDescription;
					MessageProcessorNotificationTestHelper.AssertEmail($"{messageTypeDesc} Response (Failure) for {entryHeader.Declaration.JE_DeclarationReference}", new[] { $@"Submission was unsuccessful<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Transaction ID</td><td>{transactionId}</td></tr><tr><td>Message Type</td><td>{messageTypeDesc}</td></tr><tr><td>Message Number</td><td>{outgoingMessage.EM_MessageNum}</td></tr></table><br />Failure Notification for 480036." }, new[] { "staff3@where.com", "staff4@where.com", "staff5@where.com" });
				});
			}
		}

		public void TestProcessError_ImportUCC5()
		{
			var staff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			var emailGroup = IECustomsDataRegistry.Instance.SendImportAcknowledgements.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			emailGroup.SendMode = Core.Constants.EmailTo.NominatedGroup;
			emailGroup.SendGroupPK = MessageProcessorNotificationTestHelper.SetupStaffGroup(Factory).PK;

			var (entryHeader, outgoingMessage, incomingMessage, processor) = CreateOriginalImportUCC5Data();
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;

			Factory.Save();
			using (IECustomsDataRegistry.Instance.SendImportAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, emailGroup))
			using (Factory.AddDisposableService())
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				processor.ProcessMessage(incomingMessage);

				CombineAssertions("Result for processing ERROR message.", () =>
				{
					AssertEquals("CusEntryHeader.CH_Status", "FAL", entryHeader.CH_Status);
					var messageTypeDesc = outgoingMessage.MessageTypeWithDescription;
					MessageProcessorNotificationTestHelper.AssertEmail($"{messageTypeDesc} Response (Failure) for {entryHeader.Declaration.JE_DeclarationReference}", new[] { $@"Submission was unsuccessful<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Transaction ID</td><td>{transactionId}</td></tr><tr><td>Message Type</td><td>{messageTypeDesc}</td></tr><tr><td>Message Number</td><td>{outgoingMessage.EM_MessageNum}</td></tr></table><br />Failure Notification for 480036." }, new[] { "staff3@where.com", "staff4@where.com", "staff5@where.com" });
				});
			}
		}

		const string transactionId = "TRS0000001";

		(CusEntryHeader entryHeader, AESOutboundEDIMessage outgoingMessage, AESInboundEDIMessage incomingMessage, ErrorMessageProcessor processor) CreateOriginalExportData(LoggingInformation logger = null)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B012345678";
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_Status = LogicalStatusList.Codes.Sent;
			var outgoingMessage = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
			outgoingMessage.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			outgoingMessage.EM_ApplicationReference = transactionId;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_LinkedObject = entryHeader;
			var incomingMessage = Factory.New<AESInboundEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			incomingMessage.EM_MessageSubType = EDIMessage.Status.Error;
			incomingMessage.EM_MessageText = "Failure Notification for 480036.";
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
			incomingMessage.EM_GB = outgoingMessage.EM_GB;
			incomingMessage.EM_ApplicationReference = transactionId;
			incomingMessage.EM_LinkedObject = entryHeader;
			return (entryHeader, outgoingMessage, incomingMessage, new ErrorMessageProcessor(logger ?? new LoggingInformation()));
		}

		(CusEntryHeader entryHeader, AISUCC5OutboundEDIMessage outgoingMessage, AISUCC5InboundEDIMessage incomingMessage, ErrorMessageProcessor processor) CreateOriginalImportUCC5Data()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			declaration.JE_DeclarationReference = "B012345678";
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_Status = LogicalStatusList.Codes.Sent;
			var outgoingMessage = Factory.New<AISUCC5OutboundEDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsUCC5Import;
			outgoingMessage.EM_MessageType = AISOutgoingMessageTypeList.Codes.CustomsDeclaration;
			outgoingMessage.EM_ApplicationReference = transactionId;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = LogicalStatusList.Codes.Sent;
			outgoingMessage.EM_LinkedObject = entryHeader;
			var incomingMessage = Factory.New<AISUCC5InboundEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsUCC5Import;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageType = AISOutgoingMessageTypeList.Codes.CustomsDeclaration;
			incomingMessage.EM_MessageSubType = EDIMessage.Status.Error;
			incomingMessage.EM_MessageText = "Failure Notification for 480036.";
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
			incomingMessage.EM_GB = outgoingMessage.EM_GB;
			incomingMessage.EM_ApplicationReference = transactionId;
			incomingMessage.EM_LinkedObject = entryHeader;
			return (entryHeader, outgoingMessage, incomingMessage, new ErrorMessageProcessor(new LoggingInformation()));
		}
	}
}
