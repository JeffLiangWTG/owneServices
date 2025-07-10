using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(EntryPaymentStatusCalculator))]
	sealed class EntryPaymentStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAutoDeriveStatusOnFactorySaving()
		{
			AssertEquals(false, calculator.AutoDeriveStatusOnFactorySavingForTest);
		}

		public void TestUpdateStatusWhenPAYINVComesBack()
		{
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			CMRPAYSTDMessage payMessage = Factory.New<CMRPAYSTDMessage>();
			payMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			payMessage.EM_MessageText = TestMessages.PAYSTDMessage;
			payMessage.EM_LinkedObject = entryHeader;
			entryHeader.Messages.Add(payMessage);

			EntryPaymentStatusCalculatorForTest calculator = new EntryPaymentStatusCalculatorForTest(entryHeader);
			calculator.DeriveStatusForTest();

			CMRPAYINVMessage payInvMessage = Factory.New<CMRPAYINVMessage>();
			payInvMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			payInvMessage.EM_MessageText = TestMessages.PAYINVMessage;
			payInvMessage.EM_Status = "REJ";
			payInvMessage.EM_LinkedObject = entryHeader;
			entryHeader.Messages.Add(payInvMessage);

			calculator = new EntryPaymentStatusCalculatorForTest(entryHeader);
			calculator.DeriveStatusForTest();

			AssertEquals("Entry Header payment status should be 'PayRejected", CMREntryPaymentStatusList.Codes.PayRejected, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);
		}

		public void TestUpdateStatusWhenPAYEXCComesBack()
		{
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			CMRIMDMessage iMDMessage = Factory.New<CMRIMDMessage>();
			iMDMessage.EM_MessageText = TestMessages.IMDWithPaymentText;
			iMDMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			entryHeader.Messages.Add(iMDMessage);
			Factory.Save();
			AssertEquals("PreCondition:Payment status should not be PayRejected", false, entryHeader.AddInfo.ZA_PaymentStatus_Hidden == CMREntryPaymentStatusList.Codes.PayRejected);

			CMRPAYEXCMessage pAYEXCMessage = Factory.New<CMRPAYEXCMessage>();
			pAYEXCMessage.EM_LinkedObject = entryHeader;
			pAYEXCMessage.EM_ReceiveTransmit = "RCV";
			pAYEXCMessage.EM_Status = "REJ";
			entryHeader.Messages.Add(pAYEXCMessage);
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.PayRejected, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);
		}

		public void TestUpdateStatusForIMDOrSACWithPaymentIncludedAndPayrecIsPending()
		{
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			Factory.Save();

			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.NotPaid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);

			CMRIMDMessage iMDMessage = Factory.New<CMRIMDMessage>();
			iMDMessage.EM_MessageText = TestMessages.IMDWithPaymentText;
			iMDMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			entryHeader.Messages.Add(iMDMessage);
			Factory.Save();

			CMRIMDRMessage iMDRMessage = Factory.New<CMRIMDRMessage>();
			iMDRMessage.EM_MessageText = TestMessages.IMDRMessageText;
			iMDRMessage.EM_LinkedObject = entryHeader;
			iMDRMessage.EM_ReceiveTransmit = "RCV";
			entryHeader.Messages.Add(iMDRMessage);
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.PayAckPending, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);

			CMRPAYRECMessage pAYRECMessage = Factory.New<CMRPAYRECMessage>();
			pAYRECMessage.EM_MessageText = TestMessages.PAYRECMessageText;
			pAYRECMessage.EM_LinkedObject = entryHeader;
			pAYRECMessage.EM_ReceiveTransmit = "RCV";
			entryHeader.Messages.Add(pAYRECMessage);
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.Paid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);
		}

		public void TestUpdateStatusForIMDWithoutPaymentWhenDutyDeferred()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AUT";
			org.AUIsDutyDeferred = true;
			testDec.JE_OH_Importer = org.PK;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.NotPaid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);

			CMRIMDMessage iMDMessage = Factory.New<CMRIMDMessage>();
			iMDMessage.EM_MessageText = TestMessages.IMDWithPaymentText;
			iMDMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			entryHeader.Messages.Add(iMDMessage);
			Factory.Save();

			CMRIMDRMessage iMDRMessage = Factory.New<CMRIMDRMessage>();
			iMDRMessage.EM_MessageText = TestMessages.IMDRWithNoOutstandingAmount;
			iMDRMessage.EM_LinkedObject = entryHeader;
			iMDRMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entryHeader.Messages.Add(iMDRMessage);
			entryHeader.Charges[CusEntryChargeTypeList.Codes.DutyDeferredAmount].C1_ChargeAmount = 100;
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.Paid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);
		}

		public void TestUpdateStatusForIMDWithoutPaymentWhenDutyDeferred_ConsolidatedDeclaration()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AUT";
			org.AUIsDutyDeferred = true;

			var consolidatedDeclaration = Customs.Business.Testing.ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			var leadDec = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			leadDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			leadDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			leadDec.JE_OH_Importer = org.PK;
			var memberDeclaration = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			memberDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			memberDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = leadDec.EntryHeader;
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.NotPaid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);

			var imdMessage = Factory.New<CMRIMDMessage>();
			imdMessage.EM_MessageText = TestMessages.IMDWithPaymentText;
			imdMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			consolidatedDeclaration.Messages.Add(imdMessage);
			Factory.Save();

			var imdrMessage = Factory.New<CMRIMDRMessage>();
			imdrMessage.EM_MessageText = TestMessages.IMDRWithNoOutstandingAmount;
			imdrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			consolidatedDeclaration.Messages.Add(imdrMessage);

			entryHeader.Charges[CusEntryChargeTypeList.Codes.DutyDeferredAmount].C1_ChargeAmount = 100;
			Factory.Save();
			AssertEquals("Lead Entry Header payment status unchanged", "", entryHeader.AddInfo.ZA_PaymentStatus_Hidden);
			AssertEquals("Member Entry Header payment status unchanged", "", memberDeclaration.EntryHeader.AddInfo.ZA_PaymentStatus_Hidden);

			entryHeader.DeriveConsolidatedStatus();
			AssertEquals("Lead Entry Header payment status updated", CMREntryPaymentStatusList.Codes.Paid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);
			AssertEquals("Member Entry Header payment status unchanged", "", memberDeclaration.EntryHeader.AddInfo.ZA_PaymentStatus_Hidden);
		}

		public void TestUpdateStatusForIMDOnPreLodgementWhenDutyDeferred()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AUT";
			org.AUIsDutyDeferred = true;
			testDec.JE_OH_Importer = org.PK;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.NotPaid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);

			CMRIMDMessage iMDMessage = Factory.New<CMRIMDMessage>();
			iMDMessage.EM_MessageText = TestMessages.IMDPreLodgementText;
			iMDMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			entryHeader.Messages.Add(iMDMessage);
			Factory.Save();

			CMRIMDRMessage iMDRMessage = Factory.New<CMRIMDRMessage>();
			iMDRMessage.EM_MessageText = TestMessages.IMDRWithNoOutstandingAmountPreLodge;
			iMDRMessage.EM_LinkedObject = entryHeader;
			iMDRMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entryHeader.Messages.Add(iMDRMessage);
			entryHeader.Charges[CusEntryChargeTypeList.Codes.DutyDeferredAmount].C1_ChargeAmount = 100;
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.PayPending, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);
		}

		public void TestDontUpdateToPayPendingWhenLastIMDOrSACIncludePaymentAsPAYRECarrivesEarlier()
		{
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			var deminimus = UniversalReferenceHelper.GetDeminimus(Factory);
			entryLine.CL_CustomsValue = deminimus + 1;
			Factory.Save();

			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.NotPaid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);

			CMRIMDMessage iMDMessage = Factory.New<CMRIMDMessage>();
			iMDMessage.EM_MessageText = TestMessages.IMDWithPaymentText;
			iMDMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			entryHeader.Messages.Add(iMDMessage);
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.NotPaid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);

			CMRPAYRECMessage pAYRECMessage = Factory.New<CMRPAYRECMessage>();
			pAYRECMessage.EM_MessageText = TestMessages.PAYRECMessageText;
			pAYRECMessage.EM_LinkedObject = entryHeader;
			pAYRECMessage.EM_ReceiveTransmit = "RCV";
			entryHeader.Messages.Add(pAYRECMessage);
			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			AssertEquals("PreCondition:Paid", true, entryHeader.IsCustomsChargePaid);
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.Paid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);

			CMRIMDRMessage iMDRMessage = Factory.New<CMRIMDRMessage>();
			iMDRMessage.EM_MessageText = TestMessages.IMDRMessageText;
			iMDRMessage.EM_LinkedObject = entryHeader;
			iMDRMessage.EM_ReceiveTransmit = "RCV";
			entryHeader.Messages.Add(iMDRMessage);
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.Paid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);
		}

		public void TestEndToEnd()
		{
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			var deminimus = UniversalReferenceHelper.GetDeminimus(Factory);
			entryLine.CL_CustomsValue = deminimus + 1;
			Factory.Save();

			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.NotPaid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);

			CMRIMDMessage message = Factory.New<CMRIMDMessage>();
			message.EM_MessageText = TestMessages.IMDWithPaymentText;
			message.EM_LinkedObject = entryHeader;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.NotPaid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);

			CMRIMDRMessage iMDRMessage = Factory.New<CMRIMDRMessage>();
			iMDRMessage.EM_MessageText = TestMessages.IMDRMessageText;
			iMDRMessage.EM_LinkedObject = entryHeader;
			iMDRMessage.EM_ReceiveTransmit = "RCV";
			entryHeader.Messages.Add(iMDRMessage);
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.PayAckPending, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);

			var mock = Factory.NewMoq<CMRPAYRECMessage>();
			mock.Setup(m => m.EM_DateTimeInterchangeSent).Returns(ZDateTime.Now);
			CMRPAYRECMessage pAYRECMessage = mock.Object;
			pAYRECMessage.EM_MessageText = TestMessages.PAYRECMessageText;
			pAYRECMessage.EM_LinkedObject = entryHeader;
			pAYRECMessage.EM_ReceiveTransmit = "RCV";

			entryHeader.Messages.Add(pAYRECMessage);
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.Paid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);

			CMRIMDMessage iMDMessage = Factory.New<CMRIMDMessage>();
			iMDMessage.EM_MessageText = TestMessages.IMDMessageText;
			iMDMessage.EM_LinkedObject = entryHeader;
			iMDMessage.EM_ReceiveTransmit = "TRX";
			entryHeader.Messages.Add(iMDMessage);
			Factory.Save();
			AssertEquals("Entry header payment status stays as Paid as this is transmit message", CMREntryPaymentStatusList.Codes.Paid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);
		}

		public void TestGetStatusFromInboundMessage()
		{
			CMRCUSRESMessage message = Factory.New<CMRCUSRESMessage>();

			message.EM_MessageType = CMRMessage.CMRMessageTypes.PAYINV;
			AssertEquals("Status on PAYINV", CMREntryPaymentStatusList.Codes.PayRejected, calculator.GetStatusFromInboundMessageForTest(message));

			message.EM_MessageType = CMRMessage.CMRMessageTypes.PAYOUT;
			AssertEquals("Status on PAYOUT", CMREntryPaymentStatusList.Codes.PayRejected, calculator.GetStatusFromInboundMessageForTest(message));

			message.EM_MessageType = CMRMessage.CMRMessageTypes.REFACC;
			AssertEquals("Status on REFACC", CMREntryPaymentStatusList.Codes.Refunded, calculator.GetStatusFromInboundMessageForTest(message));

			message.EM_MessageType = CMRMessage.CMRMessageTypes.REFREJ;
			AssertEquals("Status on REFREJ", CMREntryPaymentStatusList.Codes.RefundRejected, calculator.GetStatusFromInboundMessageForTest(message));
		}

		public void TestStatusForPAYREC()
		{
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			Factory.Save();

			CMRPAYRECMessage aQISPayrec = Factory.New<CMRPAYRECMessage>();
			aQISPayrec.EM_MessageType = CMRMessage.CMRMessageTypes.PAYREC;
			aQISPayrec.EM_MessageText = TestMessages.AQISServiceAmountPAYRECText;
			aQISPayrec.EM_LinkedObject = entryHeader;
			aQISPayrec.EM_ReceiveTransmit = "RCV";
			entryHeader.Messages.Add(aQISPayrec);

			AssertEquals("Customs charges is not paid", false, entryHeader.IsCustomsChargePaid);
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.NotPaid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);

			CMRPAYRECMessage customsChargePAYREC = Factory.New<CMRPAYRECMessage>();
			customsChargePAYREC.EM_MessageText = TestMessages.PAYRECMessageText;
			customsChargePAYREC.EM_LinkedObject = entryHeader;
			customsChargePAYREC.EM_ReceiveTransmit = "RCV";
			entryHeader.Messages.Add(customsChargePAYREC);
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.Paid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);

			CMRPAYRECMessage aQISServiceAmount = Factory.New<CMRPAYRECMessage>();
			aQISServiceAmount.EM_MessageType = CMRMessage.CMRMessageTypes.PAYREC;
			aQISServiceAmount.EM_MessageText = TestMessages.AQISServiceAmountPAYRECText;
			aQISServiceAmount.EM_LinkedObject = entryHeader;
			aQISServiceAmount.EM_ReceiveTransmit = "RCV";
			entryHeader.Messages.Add(aQISServiceAmount);

			AssertEquals("Customs charges is not paid", true, entryHeader.IsCustomsChargePaid);
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.Paid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);
		}

		public void TestGetResponseStatusFromOutgoing()
		{
			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			EDIMessage incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			AssertEquals("Status on Rejection", "", calculator.GetRejectedResponseStatusTest(message, incomingMessage));
			AssertEquals("Status on AwaitingResponse", "", calculator.GetAwaitingResponseStatusForTest(message));
			AssertEquals("Status on GetAcceptedResponseStatus", "", calculator.GetAcceptedResponseStatusForTest(message));
		}

		public void TestGetStatusFromInboundMessageForIMDR()
		{
			CMRIMDMessage outgoingIMD = Factory.New<CMRIMDMessage>();
			outgoingIMD.EM_MessageText = TestMessages.IMDMessageText;
			outgoingIMD.EM_LinkedObject = entryHeader;
			entryHeader.Messages.Add(outgoingIMD);

			CMRIMDRMessage message = Factory.New<CMRIMDRMessage>();
			message.EM_MessageText = TestMessages.IMDRMessageText;
			entryHeader.Messages.Add(message);

			AssertEquals("Status on IMDR Positive", CMREntryPaymentStatusList.Codes.PayPending, calculator.GetStatusFromInboundMessageForTest(message));
		}

		public void TestGetStatusFromInboundMessageSACR()
		{
			CMRIMDMessage outgoingIMD = Factory.New<CMRIMDMessage>();
			outgoingIMD.EM_MessageText = TestMessages.IMDMessageText;
			outgoingIMD.EM_LinkedObject = entryHeader;
			entryHeader.Messages.Add(outgoingIMD);

			CMRSACRMessage message = Factory.New<CMRSACRMessage>();
			message.EM_MessageText = TestMessages.SACRMessageTextPositive;
			entryHeader.Messages.Add(message);

			AssertEquals("Status on SACR Positive", CMREntryPaymentStatusList.Codes.PayPending, calculator.GetStatusFromInboundMessageForTest(message));
		}

		public void TestStatusList()
		{
			AssertEquals("Status list", typeof(CMREntryPaymentStatusList), calculator.StatusListForTest.GetType());
		}

		public void TestStatusInfo()
		{
			AssertEquals("StatusInfo", entryHeader.AddInfo.ZA_PaymentStatus_HiddenInfo, calculator.StatusInfoForTest);
		}

		public void TestInterestedMessageTypes()
		{
			AssertEquals("7 message types interested", 9, calculator.InterestedMessageTypesForTest.Length);
			AssertEquals("PAYREC", CMRMessage.CMRMessageTypes.PAYREC, calculator.InterestedMessageTypesForTest[0]);
			AssertEquals("PAYINV", CMRMessage.CMRMessageTypes.PAYINV, calculator.InterestedMessageTypesForTest[1]);
			AssertEquals("PAYOUT", CMRMessage.CMRMessageTypes.PAYOUT, calculator.InterestedMessageTypesForTest[2]);
			AssertEquals("IMD", CMRMessage.CMRMessageTypes.IMD, calculator.InterestedMessageTypesForTest[3]);
			AssertEquals("SAC", CMRMessage.CMRMessageTypes.SAC, calculator.InterestedMessageTypesForTest[4]);
			AssertEquals("REFACC", CMRMessage.CMRMessageTypes.REFACC, calculator.InterestedMessageTypesForTest[5]);
			AssertEquals("REFREJ", CMRMessage.CMRMessageTypes.REFREJ, calculator.InterestedMessageTypesForTest[6]);
			AssertEquals("PAYSTD", CMRMessage.CMRMessageTypes.PAYSTD, calculator.InterestedMessageTypesForTest[7]);
			AssertEquals("PAYEXC", CMRMessage.CMRMessageTypes.PAYEXC, calculator.InterestedMessageTypesForTest[8]);
		}

		public void TestNoAmountDueForCustomsValueLessThanThreshold()
		{
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			var deminimus = UniversalReferenceHelper.GetDeminimus(Factory);
			entryLine.CL_CustomsValue = deminimus;
			AssertEquals("Is not subject to duty and tax", false, entryHeader.IsSubjectToDutyAndTax);

			Factory.Save();

			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.NotPaid, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);

			CMRIMDMessage message = Factory.New<CMRIMDMessage>();
			message.EM_MessageText = TestMessages.IMDWithPaymentText;
			message.EM_LinkedObject = entryHeader;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			entryHeader.Messages.Add(message);
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.NoAmountDue, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);

			CMRIMDRMessage iMDRMessage = Factory.New<CMRIMDRMessage>();
			iMDRMessage.EM_MessageText = TestMessages.IMDRWithNoOutstandingAmount;
			iMDRMessage.EM_LinkedObject = entryHeader;
			iMDRMessage.EM_ReceiveTransmit = "RCV";
			entryHeader.Messages.Add(iMDRMessage);
			Factory.Save();
			AssertEquals("Entry header payment status", CMREntryPaymentStatusList.Codes.NoAmountDue, entryHeader.AddInfo.ZA_PaymentStatus_Hidden);
		}

		JobDeclaration testDec;
		CusEntryHeader entryHeader;
		EntryPaymentStatusCalculatorForTest calculator;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);

			entryHeader = testDec.CustomsEntryHeaders.AddNew();
			calculator = new EntryPaymentStatusCalculatorForTest(entryHeader);

			TaxOrFeeTestHelper.SetUp();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			SetUp();
			return new EntryPaymentStatusCalculator(entryHeader);
		}

		sealed class EntryPaymentStatusCalculatorForTest : EntryPaymentStatusCalculator
		{
			public EntryPaymentStatusCalculatorForTest(CusEntryHeader entryHeader)
			: base(entryHeader)
			{
			}

			public void DeriveStatusForTest()
			{
				DeriveStatus();
			}

			public ZString GetStatusFromInboundMessageForTest(EDIMessage incomingMessage) => GetStatusFromInboundMessage(incomingMessage);

			public ZString[] InterestedMessageTypesForTest => InterestedMessageTypes;

			public ZString GetAwaitingResponseStatusForTest(EDIMessage outgoingMessage) => GetAwaitingResponseStatus(outgoingMessage);

			public ZString GetRejectedResponseStatusTest(EDIMessage outgoingMessage, EDIMessage incomingMessage)
				=> GetRejectedResponseStatus(outgoingMessage, incomingMessage);

			public ZString GetAcceptedResponseStatusForTest(EDIMessage outgoingMessage) => GetAcceptedResponseStatus(outgoingMessage);

			public ZPropertyInfo StatusInfoForTest => StatusInfo;

			public CodeDescriptionPairList StatusListForTest => StatusList;

			public bool AutoDeriveStatusOnFactorySavingForTest => AutoDeriveStatusOnFactorySaving;

			protected override bool AutoDeriveStatusOnFactorySaving => false;
		}
	}
}
