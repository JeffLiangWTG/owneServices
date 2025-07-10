using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RFPMessageManagerTest : TestCaseWithFactory
	{
		public void TestEntryGetStatus()
		{
			quarantineHeader.RequestForPermitStatus = "123";
			AssertEquals("GetEntryStatus()", "123", messageManager.EntryStatus);
		}

		public void TestMessageGetStatus()
		{
			quarantineHeader.Declaration.JE_MessageStatus = "123";
			AssertEquals("GetMessageStatus()", "123", messageManager.MessageStatus);
		}

		public void TestGetBusinessObjectInNewFactory()
		{
			quarantineHeader.Factory.Save();
			var newHeader = (QuarantineExDocHeader)messageManager.GetBusinessObjectInNewFactory(quarantineHeader);
			AssertEquals("PK", quarantineHeader.PK, newHeader.PK);
			AssertEquals("Factories Different", true, quarantineHeader.Factory != newHeader.Factory);
		}

		public void TestValidCanSendOrderStatusCodes()
		{
			AssertEquals("Can send order code count", 2, messageManager.ValidCanSendOrderStatusCodes.Count);
		}

		public void TestValidCanSendCertRequestStatusCodes()
		{
			AssertEquals("Can send certificate request code count", 1, messageManager.ValidCanSendCertRequestStatusCodes.Count);
			AssertEquals("ValidCanSendCertRequestStatusCode", ZString.Empty, messageManager.ValidCanSendCertRequestStatusCodes[0]);
		}

		public void TestValidCanSendOriginalStatusCodes()
		{
			AssertEquals("Can send original code count", 4, messageManager.ValidCanSendOriginalStatusCodes.Count);
		}

		public void TestValidCanSendAmendmentStatusCodes()
		{
			AssertEquals("Can send amendment code count", 8, messageManager.ValidCanSendAmendmentStatusCodes.Count);
		}

		public void TestValidCanSendWithdrawalStatusCodes()
		{
			AssertEquals("Can send withdrawal code count", 6, messageManager.ValidCanSendWithdrawalStatusCodes.Count);
		}

		public void TestValidCanSendTransferStatusCode()
		{
			AssertEquals("Can send transfer code count", 11, messageManager.ValidCanSendTransferStatusCode.Count);
		}

		public void TestValidCanSendTransferAcceptanceStatusCode()
		{
			AssertEquals("Can send transfer acceptance code count", 7, messageManager.ValidCanSendTransferInStatusCode.Count);
		}

		public void TestValidCanSendCopyStatusCodes()
		{
			AssertEquals("Can send copy code count", 9, messageManager.ValidCanSendCopyStatusCode.Count);
		}

		public void TestBusinessObject()
		{
			AssertEquals(quarantineHeader, messageManager.BusinessObject);
		}

		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "Request For Permit", messageManager.MessageFriendlyName);
		}

		public void TestGenerateOriginalMessages()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			var result = messageManager.GenerateOriginalMessages(quarantineHeader);
			AssertEquals("Length", 1, result.Length);
			AssertType<RFPMessage>(result[0]);
			AssertEquals("Messages count", 1, quarantineHeader.Messages.Count);
		}

		public void TestGenerateAmendmentMessages()
		{
			messageManager = new RFPMessageManagerForTest(quarantineHeader, EXDOCMessageTypeCodes.Codes.RPL);
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			quarantineHeader.ManualAmendmentReasonForMessaging = "Vendor Testing";
			var result = messageManager.GenerateAmendmentMessages(quarantineHeader);
			AssertEquals("AmendmentResponseStatus", RFPMessage.Status.AwaitingResponse, quarantineHeader.AddInfo.ZH_AmendmentResponseStatus);
			AssertEquals("Length", 1, result.Length);
			AssertType<RFPMessage>(result[0]);
			AssertEquals("Messages count", 1, quarantineHeader.Messages.Count);
		}

		public void TestGenerateWithdrawalMessages()
		{
			messageManager = new RFPMessageManagerForTest(quarantineHeader, EXDOCMessageTypeCodes.Codes.CAN);
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			var result = messageManager.GenerateWithdrawalMessages(quarantineHeader);
			AssertEquals("Length", 1, result.Length);
			AssertType<RFPMessage>(result[0]);
			AssertEquals("Messages count", 1, quarantineHeader.Messages.Count);
		}

		public void TestCanSendXUSMessage()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			Assert("Pre-condition", messageManager.CanSendOrder);
			quarantineHeader.Declaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;

			var outGoingMessage = quarantineHeader.Messages.AddNew();
			outGoingMessage.EM_ReceiveTransmit = "TRX";
			outGoingMessage.EM_MessageNum = "00000000000000000001";
			outGoingMessage.EM_MessageType = "XUS";
			outGoingMessage.EM_Status = "HQU";

			Assert("Cannot send Order", !messageManager.CanSendOrder);
			AssertEquals("Unable to send message until a response to the current message is received.", messageManager.Notification);

			outGoingMessage.EM_Status = "FAL";
			Assert("Can send Order", messageManager.CanSendOrder);
		}

		public void TestCanSendOriginal()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			Assert("Pre-condition", messageManager.CanSendOriginal);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
			Assert("Can send original", messageManager.CanSendOriginal);
			AssertEquals("Original on order", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
			Assert("Cannot send original", !messageManager.CanSendOriginal);
			AssertEquals("Original on cancellation", "Message cannot be sent when status is CANC - Cancelled.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			Assert("Cannot send original", !messageManager.CanSendOriginal);
			AssertEquals("No original on completed", "Message cannot be sent when status is COMP - Completed.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate;
			Assert("Cannot send original", !messageManager.CanSendOriginal);
			AssertEquals("No original on emergency health certificate", "Message cannot be sent when status is EMHC - Emergency Health Certificate.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal;
			Assert("Cannot send original", !messageManager.CanSendOriginal);
			AssertEquals("No original on final", "Message cannot be sent when status is FINL - Final.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady;
			Assert("Cannot send original", !messageManager.CanSendOriginal);
			AssertEquals("No original on health certificate", "Message cannot be sent when status is HCRD - Health Certificate Ready.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
			Assert("Can send original", messageManager.CanSendOriginal);
			AssertEquals("No original on inital", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected;
			Assert("Can send original", messageManager.CanSendOriginal);
			AssertEquals("No original on inspected", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.SuspSuspended;
			Assert("Cannot send original", !messageManager.CanSendOriginal);
			AssertEquals("No original on suspended", "Message cannot be sent when status is SUSP - Suspended.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
			quarantineHeader.Declaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			Assert("Cannot send Original", !messageManager.CanSendOriginal);
			AssertEquals("No original when already awaiting a response.", "Unable to send message until a response to the current message is received.", messageManager.Notification);
		}

		public void TestCanSendTransfer()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			Assert("Pre-condition", !messageManager.CanSendTransfer);
			quarantineHeader.QH_TransfereeEDIUserIdentifier = "21";
			quarantineHeader.QH_TransfereeExporterNumber = "99999";
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
			Assert("Cannot send transfer", !messageManager.CanSendTransfer);
			AssertEquals("No Transfer on cancellation", "Message cannot be sent when status is CANC - Cancelled.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			Assert("Can send transfer", messageManager.CanSendTransfer);
			AssertEquals("Transfer on completed", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate;
			Assert("Can send transfer", messageManager.CanSendTransfer);
			AssertEquals("Transfer on emergency health certificate", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal;
			Assert("Can send transfer", messageManager.CanSendTransfer);
			AssertEquals("Transfer on Final", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady;
			Assert("Can send transfer", messageManager.CanSendTransfer);
			AssertEquals("Transfer on Health Cert Ready", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
			Assert("Can send transfer", messageManager.CanSendTransfer);
			AssertEquals("Transfer on inital", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected;
			Assert("Can send transfer", messageManager.CanSendTransfer);
			AssertEquals("Transfer on Inspected", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
			Assert("Can send transfer", messageManager.CanSendTransfer);
			AssertEquals("Transfer on Order", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.ReviewInReview;
			Assert("Can send transfer", messageManager.CanSendTransfer);
			AssertEquals("Transfer on Review", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.SuspSuspended;
			Assert("Can send transfer", messageManager.CanSendTransfer);
			AssertEquals("Transfer on suspended", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected;
			quarantineHeader.Declaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			Assert("Cannot send transfer", !messageManager.CanSendTransfer);
			AssertEquals("Awaiting Response", "Unable to send message until a response to the current message is received.", messageManager.Notification);
			quarantineHeader.Declaration.JE_MessageStatus = ZString.Empty;
			quarantineHeader.QH_TransfereeEDIUserIdentifier = ZString.Empty;
			quarantineHeader.QH_TransfereeExporterNumber = ZString.Empty;
			Assert("The edi user/exporter number not set", !messageManager.CanSendTransfer);
			AssertEquals("No Transfer EDI User and Exporter", "Transfer to EDI user and Transfer to Exporter must be entered before a Transfer message can be sent.", messageManager.Notification);
		}

		public void TestCanSendForward()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			Assert("Pre-condition", !messageManager.CanSendForward);
			quarantineHeader.QH_ForwardeeEDIUserIdentifier = "21";
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
			Assert("Cannot send Forward", !messageManager.CanSendForward);
			AssertEquals("No Forward on cancellation", "Message cannot be sent when status is CANC - Cancelled.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			Assert("Can send Forward", messageManager.CanSendForward);
			AssertEquals("Forward on completed", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate;
			Assert("Can send Forward", messageManager.CanSendForward);
			AssertEquals("Forward on emergency health certificate", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal;
			Assert("Can send Forward", messageManager.CanSendForward);
			AssertEquals("Forward on Final", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady;
			Assert("Can send Forward", messageManager.CanSendForward);
			AssertEquals("Forward on Health Cert Ready", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
			Assert("Can send Forward", messageManager.CanSendForward);
			AssertEquals("Forward on inital", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected;
			Assert("Can send Forward", messageManager.CanSendForward);
			AssertEquals("Forward on Inspected", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
			Assert("Can send Forward", messageManager.CanSendForward);
			AssertEquals("Forward on Order", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.ReviewInReview;
			Assert("Can send Forward", messageManager.CanSendForward);
			AssertEquals("Forward on Review", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.SuspSuspended;
			Assert("Can send Forward", messageManager.CanSendForward);
			AssertEquals("Forward on suspended", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected;
			quarantineHeader.Declaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			Assert("Cannot send Forward", !messageManager.CanSendForward);
			AssertEquals("Awaiting Response", "Unable to send message until a response to the current message is received.", messageManager.Notification);
			quarantineHeader.Declaration.JE_MessageStatus = ZString.Empty;
			quarantineHeader.QH_ForwardeeEDIUserIdentifier = ZString.Empty;
			Assert("The edi user/exporter number not set", !messageManager.CanSendForward);
			AssertEquals("No Forward EDI User and Exporter", "Forward to EDI user must be entered before a Forward message can be sent.", messageManager.Notification);
		}

		public void TestCanSendWithdrawal()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			Assert("Pre-condition", !messageManager.CanSendWithdrawal);
			AssertEquals("No Withdrawl when no status", "Message cannot be sent when no successful lodgement message has been received.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal;
			Assert("Can send withdrawl", messageManager.CanSendWithdrawal);
			AssertEquals("Withdrawl when Final", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
			Assert("Cannot send withdrawl", !messageManager.CanSendWithdrawal);
			AssertEquals("No Withdrawl when cancelled", "Message cannot be sent when status is CANC - Cancelled.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			Assert("Can send withdrawl", messageManager.CanSendWithdrawal);
			AssertEquals("Withdrawl when Completed", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate;
			Assert("Can send withdrawl", messageManager.CanSendWithdrawal);
			AssertEquals("Withdrawl when Emergency Health Certificate", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady;
			Assert("Cannot send withdrawl", !messageManager.CanSendWithdrawal);
			AssertEquals("No Withdrawl when Health Cert Rdy", "Message cannot be sent when status is HCRD - Health Certificate Ready.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
			Assert("Can send withdrawl", messageManager.CanSendWithdrawal);
			AssertEquals("Withdrawl when Inital", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected;
			Assert("Cannot send withdrawl", !messageManager.CanSendWithdrawal);
			AssertEquals("Withdrawl when Inspected", "Message cannot be sent when status is INSP - Inspected.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
			Assert("Can send withdrawl", messageManager.CanSendWithdrawal);
			AssertEquals("Withdrawl when Order", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.SuspSuspended;
			Assert("Can send withdrawl", messageManager.CanSendWithdrawal);
			AssertEquals("Withdrawl when Suspended", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
			quarantineHeader.Declaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			Assert("Cannot send transfer", !messageManager.CanSendWithdrawal);
			AssertEquals("Awaiting Response", "Unable to send message until a response to the current message is received.", messageManager.Notification);
		}

		public void TestCanSendWhenUnderReview()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			Assert("Pre-condition", messageManager.CanSendOriginal);

			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
			Assert("Can send original - not under review", messageManager.CanSendOriginal);

			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.ReviewInReview;
			Assert("Can send original (with warning) for RFS type in review status", messageManager.CanSendOriginal);

			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
			Assert("Cannot send original", !messageManager.CanSendOriginal);
			AssertEquals("Original on cancellation", "Message cannot be sent when status is CANC - Cancelled.", messageManager.Notification);

			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.ReviewInReview;
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			messageInitiator.AnswerToContinueWithAction = false;
			quarantineHeader.Declaration.MessageInitiator = messageInitiator;
			Assert("Cannot send original - user has cancelled out of send warning", !messageManager.CanSendOriginal);

			messageInitiator.AnswerToContinueWithAction = true;
			Assert("Can send original when accepting warning", messageManager.CanSendOriginal);

			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.ReviewInReview;
			Assert("Cannot send certificate when under reveiw", !messageManager.CanSendCertificateRequest);
			Assert("Cannot request re-issue of certificate when under reveiw", !messageManager.CanSendRequestReissueCertificate);
			Assert("Cannot cancel EDN when under reveiw", !messageManager.CanSendTransferOrCancelEDN);
		}

		public void TestCanSendNEXDOCCancellation()
		{
			AssertEquals("CanSendNEXDOCCancellation should be false without NEXDOCCancellationReason", false, messageManager.CanSendNEXDOCCancellation);
			AssertEquals("Notification text", "Please input a NEXDOC Cancellation Reason in Notes.", messageManager.Notification);

			messageManager.Notification = ZString.Empty;
			quarantineHeader.Declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.NEXDOCCancellationReason.Description, "Cancellation Reason");
			AssertEquals("CanSendNEXDOCCancellation should be true with NEXDOCCancellationReason", true, messageManager.CanSendNEXDOCCancellation);
			AssertEquals("Notification text should be empty", string.Empty, messageManager.Notification);
		}

		public void TestIsWaitingForResponse()
		{
			quarantineHeader.Declaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			Assert("Awaiting a response is true", messageManager.IsWaitingForResponse);
			quarantineHeader.Declaration.JE_MessageStatus = Messaging.Integration.EDIMessageStatusList.Codes.Acknowledged;
			Assert("Awaiting a response is false", !messageManager.IsWaitingForResponse);
			quarantineHeader.Declaration.JE_MessageStatus = Messaging.Integration.EDIMessageStatusList.Codes.Rejected;
			Assert("Awaiting a response is false", !messageManager.IsWaitingForResponse);
		}

		public void TestCanSendOrder()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			Assert("Pre-condition", messageManager.CanSendOrder);
			AssertEquals("Order when no status", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
			Assert("Can send order", messageManager.CanSendOrder);
			AssertEquals("Order when Order", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
			Assert("Cannot send order", !messageManager.CanSendOrder);
			AssertEquals("No Order when Cancelled", "Message cannot be sent when status is CANC - Cancelled.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			Assert("Cannot send order", !messageManager.CanSendOrder);
			AssertEquals("No Order when Completed", "Message cannot be sent when status is COMP - Completed.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate;
			Assert("Cannot send order", !messageManager.CanSendOrder);
			AssertEquals("No Order when Emergency Health Certificate", "Message cannot be sent when status is EMHC - Emergency Health Certificate.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal;
			Assert("Cannot send order", !messageManager.CanSendOrder);
			AssertEquals("No Order when Final", "Message cannot be sent when status is FINL - Final.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady;
			Assert("Cannot send order", !messageManager.CanSendOrder);
			AssertEquals("No Order when Health Certificate Ready", "Message cannot be sent when status is HCRD - Health Certificate Ready.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
			Assert("Cannot send order", !messageManager.CanSendOrder);
			AssertEquals("No Order when Inital", "Message cannot be sent when status is INIT - Initial.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected;
			Assert("Cannot send order", !messageManager.CanSendOrder);
			AssertEquals("No Order when Inspected", "Message cannot be sent when status is INSP - Inspected.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.SuspSuspended;
			Assert("Cannot send order", !messageManager.CanSendOrder);
			AssertEquals("No Order when Suspended", "Message cannot be sent when status is SUSP - Suspended.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
			quarantineHeader.Declaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			Assert("Cannot send transfer", !messageManager.CanSendOrder);
			AssertEquals("Awaiting Response", "Unable to send message until a response to the current message is received.", messageManager.Notification);
		}

		public void TestCanSendTransferAcceptance()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			Assert("Pre-condition", !messageManager.CanSendTransferIn);
			AssertEquals("No Transfer Acceptance when no status", "Message cannot be sent when no Transfer message has been received.", messageManager.Notification);
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "TESTSUP";
			var exportNum = supplier.CustomsCodes.AddNew();
			exportNum.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber;
			exportNum.OK_RN_NKCodeCountry = "AU";
			exportNum.OK_OH = supplier.PK;
			exportNum.OK_CustomsRegNo = "12345";
			quarantineHeader.Declaration.JE_OH_Supplier = supplier.PK;
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
			Assert("Cannot send transfer acceptance/rejection", !messageManager.CanSendTransferIn);
			AssertEquals("No Transfer Acceptance when Cancelled", "Message cannot be sent when status is CANC - Cancelled.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			Assert("Can send transfer acceptance/rejection", messageManager.CanSendTransferIn);
			AssertEquals("Transfer Acceptance when Completed", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate;
			Assert("Cannot send transfer acceptance/rejection", !messageManager.CanSendTransferIn);
			AssertEquals("No Transfer Acceptance when Emergency Health Certificate", "Message cannot be sent when status is EMHC - Emergency Health Certificate.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal;
			Assert("Can send transfer acceptance/rejection", messageManager.CanSendTransferIn);
			AssertEquals("Transfer Acceptance when Final", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady;
			Assert("Can send transfer acceptance/rejection", messageManager.CanSendTransferIn);
			AssertEquals("Transfer Acceptance when Health Cert Ready", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
			Assert("Can send transfer acceptance/rejection", messageManager.CanSendTransferIn);
			AssertEquals("Transfer Acceptance when Intial", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected;
			Assert("Can send transfer acceptance/rejection", messageManager.CanSendTransferIn);
			AssertEquals("Transfer Acceptance when Inspected", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
			Assert("Can send transfer acceptance/rejection", messageManager.CanSendTransferIn);
			AssertEquals("Transfer Acceptance when Order", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.SuspSuspended;
			Assert("Cannot send transfer acceptance/rejection", !messageManager.CanSendTransferIn);
			AssertEquals("No Transfer Acceptance when Suspended", "Message cannot be sent when status is SUSP - Suspended.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
			quarantineHeader.Declaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			Assert("Cannot send transfer acceptance/rejection", !messageManager.CanSendTransferIn);
			AssertEquals("Awaiting Response", "Unable to send message until a response to the current message is received.", messageManager.Notification);
			quarantineHeader.Declaration.JE_MessageStatus = ZString.Empty;
			quarantineHeader.Declaration.JE_OH_Supplier = ZGuid.Empty;
			Assert("No supplier set cannot send", !messageManager.CanSendTransferIn);
			AssertEquals("NO Exdoc Exporter Number for supplier", "Supplier must be entered and have an EXDOC Exporter Number.", messageManager.Notification);
			var nonExdocSupplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "TESTSUP2";
			quarantineHeader.Declaration.JE_OH_Supplier = nonExdocSupplier.PK;
			Assert("Non Exdoc supplier set cannot send", !messageManager.CanSendTransferIn);
			AssertEquals("NO Exdoc Exporter Number for supplier", "Supplier must be entered and have an EXDOC Exporter Number.", messageManager.Notification);
		}

		public void TestCanSendCopy()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			Assert("Pre-condition", !messageManager.CanSendCopy);
			AssertEquals("No copy on blank status", "A copy request message cannot be sent when the RFP has not been reported to Quarantine.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
			Assert("Cannot send copy", !messageManager.CanSendCopy);
			AssertEquals("No copy on cancellation", "A copy request message cannot be sent when the RFP has not been reported to Quarantine.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			Assert("Can send copy", messageManager.CanSendCopy);
			AssertEquals("Copy on completed", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate;
			Assert("Can send copy", messageManager.CanSendCopy);
			AssertEquals("Copy on emergency health certificate", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal;
			Assert("Can send copy", messageManager.CanSendCopy);
			AssertEquals("Copy on Final", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady;
			Assert("Can send copy", messageManager.CanSendCopy);
			AssertEquals("Copy on Health certificate ready", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
			Assert("Can send copy", messageManager.CanSendCopy);
			AssertEquals("Copy on Intial", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected;
			Assert("Can send copy", messageManager.CanSendCopy);
			AssertEquals("Copy on Inspected", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
			Assert("Can send copy", messageManager.CanSendCopy);
			AssertEquals("Copy on Order", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.SuspSuspended;
			Assert("Can send copy", messageManager.CanSendCopy);
			AssertEquals("Copy on Suspended", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
			quarantineHeader.Declaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			Assert("Cannot send copy", !messageManager.CanSendCopy);
			AssertEquals("No copy when already awaiting a response.", "Unable to send message until a response to the current message is received.", messageManager.Notification);
		}

		public void TestCanSendAmmendment()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			quarantineHeader.RequestForPermitStatus = ZString.Empty;
			Assert("Pre-condition", !messageManager.CanSendAmmendment);
			AssertEquals("The notification message should indicate there is nothing to ammend", "Message cannot be sent when no successful lodgement message has been received.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
			Assert("Cannot send ammendment", !messageManager.CanSendAmmendment);
			AssertEquals("No ammendment on cancellation", "Message cannot be sent when status is CANC - Cancelled.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			Assert("Can send ammendment", messageManager.CanSendAmmendment);
			AssertEquals("Ammendment on completed", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate;
			Assert("Can send ammendment", messageManager.CanSendAmmendment);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal;
			Assert("Can send ammendment", messageManager.CanSendAmmendment);
			AssertEquals("Ammendment on Final", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady;
			Assert("Can send ammendment", messageManager.CanSendAmmendment);
			AssertEquals("Ammendment on Health Certificate Ready", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial;
			Assert("Can send ammendment", messageManager.CanSendAmmendment);
			AssertEquals("Ammendment on Initial", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected;
			Assert("Can send ammendment", messageManager.CanSendAmmendment);
			AssertEquals("Ammendment on Inspected", ZString.Empty, messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;
			Assert("Cannot send ammendment", !messageManager.CanSendAmmendment);
			AssertEquals("No ammendment on Order", "Message cannot be sent when status is ORDR - Order.", messageManager.Notification);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.SuspSuspended;
			Assert("Can send ammendment", messageManager.CanSendAmmendment);
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal;
			quarantineHeader.Declaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			Assert("Cannot send ammendment", !messageManager.CanSendAmmendment);
			AssertEquals("Awaiting Response", "Unable to send message until a response to the current message is received.", messageManager.Notification);
		}

		public void TestCanSendRequestReplacementCertificate()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;

			var description = PredefinedNoteTypes.Instance.EXDOCAmendmentReason.Description;
			var notification = $"Message cannot be sent until an amendment reason is entered. You can add it under Declaration - Notes and use '{description}' as Description.";

			Assert("CanSendRequestReplacementCertificate", !messageManager.CanSendRequestReplacementCertificate);
			AssertEquals("Cannot send Replacement Certificate Request message without any reason.", notification, messageManager.Notification);

			quarantineHeader.Declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.EXDOCAmendmentReason.Description, "Teldrassil");

			Assert("CanSendRequestReplacementCertificate", messageManager.CanSendRequestReplacementCertificate);
			AssertEquals("Replacement Certificate Request with reason.", ZString.Empty, messageManager.Notification);

			var validCodes = new string[]
			{
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CtrdCertificateReady,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.SuspSuspended,
			};

			AssertCanSendForEntryStatus(validCodes, () => messageManager.CanSendRequestReplacementCertificate);
		}

		public void TestCanSendRequestReissueCertificate()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			quarantineHeader.RequestForPermitStatus = ZString.Empty;
			Assert("Pre-condition", !messageManager.CanSendRequestReissueCertificate);
			AssertEquals("Message cannot be sent when no successful lodgement message has been received.", messageManager.Notification);

			var validCodes = new string[]
			{
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.SuspSuspended
			};

			AssertCanSendForEntryStatus(validCodes, () => messageManager.CanSendRequestReissueCertificate);
		}

		public void TestCanSendTransferOrCancelEDN()
		{
			var declaration = quarantineHeader.Declaration;

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			quarantineHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;

			declaration.DeclarationNumber = string.Empty;

			Assert("Pre-Condition.", declaration.CustomsAuthorityNumber.IsEmpty);
			Assert("Should be false as there is not a valid export declaration number.", !messageManager.CanSendTransferOrCancelEDN);
			AssertEquals("Message cannot be sent until the declaration has an Export Declaration Number.", messageManager.Notification);

			declaration.DeclarationNumber = "123";

			Assert("Pre-Condition.", !declaration.CustomsAuthorityNumber.IsEmpty);
			Assert("Should be true as there is a valid export declaration number.", messageManager.CanSendTransferOrCancelEDN);
			AssertEquals("Can Send Transfer Or Cancel EDN with a valid export declaration number.", ZString.Empty, messageManager.Notification);

			var validCodes = new string[]
			{
				EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted,
				EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CtrdCertificateReady,
				EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal,
				EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady,
				EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial,
				EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected,
				EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder
			};

			AssertCanSendForEntryStatus(validCodes, () => messageManager.CanSendTransferOrCancelEDN);
		}

		public void TestCanSendCertificateRequest()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			Assert("CanSendCertificateRequest", messageManager.CanSendCertificateRequest);
			AssertEquals("Certificate Request when no status", ZString.Empty, messageManager.Notification);

			foreach (ICodeDescription status in new EXDOCComplianceStatusCodesForCusEntryNumber())
			{
				quarantineHeader.RequestForPermitStatus = status.Code;
				Assert("Cannot send Certificate Request", !messageManager.CanSendCertificateRequest);
				AssertEquals("Cannot send Certificate Request message when status is setup", "A Certificate Request must be separate job to any RFP, i.e. an RFP must not have been requested on this job.", messageManager.Notification);
			}
		}

		public void TestGenerateMessage()
		{
			quarantineHeader.QH_ProduceType = ZString.Empty;
			AssertEquals("Empty produce type", 0, messageManager.GenerateOriginalMessages(quarantineHeader).Length);
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			AssertEquals("Dairy produce type", 1, messageManager.GenerateOriginalMessages(quarantineHeader).Length);
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			AssertEquals("Eggs produce type", 1, messageManager.GenerateOriginalMessages(quarantineHeader).Length);
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			AssertEquals("Fish produce type", 1, messageManager.GenerateOriginalMessages(quarantineHeader).Length);
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			AssertEquals("Grains and plants produce type", 1, messageManager.GenerateOriginalMessages(quarantineHeader).Length);
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			AssertEquals("Horticulture produce type", 1, messageManager.GenerateOriginalMessages(quarantineHeader).Length);
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
			AssertEquals("Inedible Meat produce type", 1, messageManager.GenerateOriginalMessages(quarantineHeader).Length);
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			AssertEquals("Meat produce type", 1, messageManager.GenerateOriginalMessages(quarantineHeader).Length);
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			AssertEquals("Skins and hides produce type", 1, messageManager.GenerateOriginalMessages(quarantineHeader).Length);
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			AssertEquals("Wool produce type", 1, messageManager.GenerateOriginalMessages(quarantineHeader).Length);
			messageManager = new RFPMessageManagerForTest(quarantineHeader, EXDOCMessageTypeCodes.Codes.CRQ);
			AssertEquals("Certificate request message", 1, messageManager.GenerateOriginalMessages(quarantineHeader).Length);
		}

		public void TestGenerateAmendmentMessage()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			quarantineHeader.ManualAmendmentReasonForMessaging = "Vendor Testing";
			messageManager = new RFPMessageManagerForTest(quarantineHeader, EXDOCMessageTypeCodes.Codes.RPL);
			messageManager.GenerateAmendmentMessages(quarantineHeader);
			AssertEquals("AmendmentResponseStatus", RFPMessage.Status.AwaitingResponse, quarantineHeader.AddInfo.ZH_AmendmentResponseStatus);
		}

		public void TestProduceTypeNotification()
		{
			Assert("Cannot send message", !messageManager.CanSendOrder);
			AssertEquals("Need to enter a produce type", "Message cannot be sent until a produce type is entered.", messageManager.Notification);
		}

		public void TestCanSendReadRex()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			AssertCanSendForEntryStatus(new EXDOCComplianceStatusCodesForCusEntryNumber().GetAllCodes(), () => messageManager.CanSendReadREX);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			quarantineHeader = helper.Header1.QuarantineExDocHeader;
			messageManager = new RFPMessageManagerForTest(quarantineHeader, EXDOCMessageTypeCodes.Codes.LDG);
		}

		QuarantineExDocHeader quarantineHeader;
		RFPMessageManagerForTest messageManager;

		void AssertCanSendForEntryStatus(string[] validCodes, Func<bool> canSendFunc)
		{
			CombineAssertions(() =>
			{
				var complianceCodeList = new EXDOCComplianceStatusCodesForCusEntryNumber();
				foreach (var code in complianceCodeList.GetAllCodes())
				{
					quarantineHeader.RequestForPermitStatus = code;

					var canSend = validCodes.Any(c => c == code);
					AssertEquals($"Should be {canSend} when Entry Status is {code}.", canSend, canSendFunc.Invoke());

					var notification = canSend ? ZString.Empty : ZString.Format("Message cannot be sent when status is {0}.", complianceCodeList.GetDescriptionFromCode(code));
					AssertEquals(notification, messageManager.Notification);
				}
			});
		}

		sealed class RFPMessageManagerForTest : RFPMessageManager
		{
			public RFPMessageManagerForTest(QuarantineExDocHeader exDocHeader, string messageType) : base(exDocHeader, messageType)
			{
			}

			internal new BusinessObject GetBusinessObjectInNewFactory(BusinessObject businessObject) => base.GetBusinessObjectInNewFactory(businessObject);
		}
	}
}
