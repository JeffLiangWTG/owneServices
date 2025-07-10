using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(IMDStatusCalculator))]
	sealed class IMDStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetResponseStatusForSAC_NoWithdrawal()
		{
			CMRSACMessage message = Factory.New<CMRSACMessage>();
			message.EM_MessageText = "Message";

			CMRSACRMessage incomingMessage = Factory.New<CMRSACRMessage>();

			AssertEquals("IsSACMessage", true, calculator.IsSACMessage(message));

			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearSAC.Code, calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingSAC.Code, calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailSAC.Code, calculator.GetRejectedResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForSACWithdrawal()
		{
			var cMRSACMessageMock = Factory.NewMoq<CMRSACMessage>();
			CMRSACMessage message = cMRSACMessageMock.Object;
			cMRSACMessageMock.Setup(m => m.IsWithdrawalMessage).Returns(true);
			message.EM_MessageText = "Message";

			CMRSACRMessage incomingMessage = Factory.New<CMRSACRMessage>();

			AssertEquals("IsSACMessage", true, calculator.IsSACMessage(message));

			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearWithdrawal.Code, calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingWithdrawal.Code, calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailWithdrawal.Code, calculator.GetRejectedResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForPreLodge()
		{
			CMRIMDMessage message = Factory.New<CMRIMDMessage>();
			message.EM_MessageType = CMRMessage.CMRMessageTypes.IMD;
			message.EM_MessageText = ValidIMDPreLodgeMessageText;
			CMRIMDRMessage incomingMessage = Factory.New<CMRIMDRMessage>();

			AssertEquals("PreCondition:IsPreLodgeMessage", true, calculator.IsPreLodgeMessage(message));

			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearPreLodge.Code, calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingPreLodge.Code, calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailPreLodge.Code, calculator.GetRejectedResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForFormalLodge()
		{
			CMRIMDMessage message = Factory.New<CMRIMDMessage>();
			message.EM_MessageType = CMRMessage.CMRMessageTypes.IMD;
			message.EM_MessageText = ValidIMDFormalLodgeMessageText;

			CMRIMDRMessage incomingMessage = Factory.New<CMRIMDRMessage>();

			AssertEquals("PreCondition:IsFormalLodgeWithPayMessage", true, calculator.IsFormalLodge(message));
			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearFormalLodge.Code, calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingFormalLodge.Code, calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailFormalLodge.Code, calculator.GetRejectedResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForAmendment()
		{
			CMRIMDMessage message = Factory.New<CMRIMDMessage>();
			message.EM_MessageType = CMRMessage.CMRMessageTypes.IMD;
			message.EM_MessageText = ValidIMDAmendmentMessageText;
			CMRIMDRMessage incomingMessage = Factory.New<CMRIMDRMessage>();

			AssertEquals("PreCondition:IsAmendment", true, calculator.IsAmendment(message));
			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearAmendment.Code, calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingAmendment.Code, calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailAmendment.Code, calculator.GetRejectedResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForWithdrawal()
		{
			CMRIMDMessage message = Factory.New<CMRIMDMessage>();
			message.EM_MessageType = CMRMessage.CMRMessageTypes.IMD;
			message.EM_MessageText = ValidIMDWithdrawalMessageText;
			CMRIMDRMessage incomingMessage = Factory.New<CMRIMDRMessage>();

			AssertEquals("PreCondition:IsWithdrawal", true, calculator.IsWithdrawal(message));
			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearWithdrawal.Code, calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingWithdrawal.Code, calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailWithdrawal.Code, calculator.GetRejectedResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForPaymentMessage()
		{
			CMRIMDMessage message = Factory.New<CMRIMDMessage>();
			message.EM_MessageType = CMRMessage.CMRMessageTypes.PAYSTD;
			CMRIMDRMessage incomingMessage = Factory.New<CMRIMDRMessage>();

			AssertEquals("PreCondition:IsPayMessage", true, calculator.IsPaymentMessage(message));
			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearPayment.Code, calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingPayment.Code, calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailPayment.Code, calculator.GetRejectedResponseStatus(message, incomingMessage));
		}

		public void TestIsFormalLodgeWithoutPayMessage()
		{
			CMRIMDMessage message = Factory.New<CMRIMDMessage>();
			message.EM_MessageText = ValidIMDFormalLodgeMessageText;
			AssertEquals("IsFormalLodgeWithPayMessage", true, calculator.IsFormalLodge(message));
		}

		public void TestIsPreLodgeMessage()
		{
			CMRIMDMessage message = Factory.New<CMRIMDMessage>();
			message.EM_MessageType = CMRMessage.CMRMessageTypes.IMD;
			message.EM_MessageText = ValidIMDPreLodgeMessageText;
			AssertEquals("IsPrelodge Message", true, calculator.IsPreLodgeMessage(message));
		}

		public void TestIsPaymentMessage()
		{
			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageType = CMRMessage.CMRMessageTypes.PAYSTD;
			AssertEquals("IsPaymentMessage", true, calculator.IsPaymentMessage(message));
		}

		public void TestInterestedMessageTypes()
		{
			AssertEquals("Message types interested", 8, calculator.InterestedMessageTypes.Length);
			AssertEquals("ATD", CMRMessage.CMRMessageTypes.ATD, calculator.InterestedMessageTypes[0]);
			AssertEquals("PAYINV", CMRMessage.CMRMessageTypes.PAYINV, calculator.InterestedMessageTypes[1]);
			AssertEquals("PAYOUT", CMRMessage.CMRMessageTypes.PAYOUT, calculator.InterestedMessageTypes[2]);
			AssertEquals("IMD", CMRMessage.CMRMessageTypes.IMD, calculator.InterestedMessageTypes[3]);
			AssertEquals("PAYSTD", CMRMessage.CMRMessageTypes.PAYSTD, calculator.InterestedMessageTypes[4]);
			AssertEquals("SAC", CMRMessage.CMRMessageTypes.SAC, calculator.InterestedMessageTypes[5]);
			AssertEquals("PAYREC", CMRMessage.CMRMessageTypes.PAYREC, calculator.InterestedMessageTypes[6]);
			AssertEquals("PAYEXC", CMRMessage.CMRMessageTypes.PAYEXC, calculator.InterestedMessageTypes[7]);
		}

		public void TestStatusInfo()
		{
			AssertEquals("Status info", entryHeader.CH_StatusInfo, calculator.StatusInfo);
		}

		public void TestUpdateStatusWhenPAYEXCIsReceived()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_DeclarationReference = "B00122382";
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00122382/1";

			CMRPAYSTDMessage paymentMessage = Factory.New<CMRPAYSTDMessage>();
			paymentMessage.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			paymentMessage.EM_MessageText = TestMessages.PAYSTDMessage;

			entryHeader.Messages.Add(paymentMessage);
			entryHeader.CH_Status = CustomsEntryStatus.AwaitingPayment.Code;
			testDec.JE_MessageStatus = CustomsEntryStatus.AwaitingPayment.Code;

			CMRPAYEXCMessage paymentResponse = Factory.New<CMRPAYEXCMessage>();
			paymentResponse.EM_SystemCreateTimeUtc = ZDateTime.Today;
			paymentResponse.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Rejected;
			entryHeader.Messages.Add(paymentResponse);
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);

			calculator = new TestIMDStatusCalculatorClass(entryHeader);
			calculator.DeriveStatus();
			AssertEquals("Entry and Declaration status should be updated", CustomsEntryStatus.FailPayment.Code, entryHeader.CH_Status);
			AssertEquals("Entry and Declaration status should be updated", CustomsEntryStatus.FailPayment.Code, testDec.JE_MessageStatus);
		}

		JobDeclaration testDec;
		CusEntryHeader entryHeader;
		TestIMDStatusCalculatorClass calculator;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			entryHeader = testDec.CustomsEntryHeaders.AddNew();
			calculator = new TestIMDStatusCalculatorClass(entryHeader);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			SetUp();
			return new IMDStatusCalculator(entryHeader);
		}

		string ValidIMDPreLodgeMessageText => @"UNH+1+CUSDEC:D:99B:UN'
BGM+929:::IMD+B00148229/1/SYD7:7+9'
CST++N20::95'
LOC+8+AUSYD::6'
LOC+12+AUSYD::6'
LOC+9+SGSIN::6'
LOC+79+AUSYD::6'
DTM+178:20050610:102'
DTM+252:20050610:102'
DTM+260:20050610:102'
GIS+N:153:95'
GIS+PRE:109:95'
MEA+AAE+G+KG:150.00000'
FTX+DEL+++DELIVERY NAME'
RFF+ABQ:OWNERS REF'
RFF+ADU:B00148229'
RFF+APH:FOB'
TDT+20++A++QF::3'
NAD+AT+51006765546::95'
NAD+VT+AA33HF::95'
NAD+DP++CANBERRA++DELIVERY ADDRESS LINE 1::DELIVERY ADDRESS LINE 2++:::ACT+2601+AU'
NAD+CB+54321::95'
MOA+63:15000.00:AUD'
MOA+141:15000.00:AUD'
MOA+39:15000.00:AUD'
UNS+D'
DMS+1'
LIN+1+I'
PAC+120+1'
PCI+1'
RFF+MWB:08155555555'
PCI+1'
RFF+HWB:HOUSE BILL'
CST+1+I::95+N20::95'
FTX+AAA+++FLOOR TILES'
LOC+27+AU::5'
MEA+AAA++SM:150.00'
NAD+SU+66015286036::95'
MOA+38:15000.00:AUD'
RFF+ABD:39181000'
RFF+AED:23'
RFF+AFV:TV'
UNS+S'
UNT+44+1'".Replace("\r\n", "");

		string ValidIMDFormalLodgeMessageText => @"UNH+1+CUSDEC:D:99B:UN'
BGM+929:::IMD+B00148229/1/SYD7:7+9'
CST++N20::95'
LOC+8+AUSYD::6'
LOC+12+AUSYD::6'
LOC+9+SGSIN::6'
LOC+79+AUSYD::6'
DTM+178:20050610:102'
DTM+252:20050610:102'
DTM+260:20050610:102'
GIS+N:153:95'
MEA+AAE+G+KG:150.00000'
FTX+DEL+++DELIVERY NAME'
RFF+ABQ:OWNERS REF'
RFF+ADU:B00148229'
RFF+APH:FOB'
TDT+20++A++QF::3'
NAD+AT+51006765546::95'
NAD+VT+AA33HF::95'
NAD+DP++CANBERRA++DELIVERY ADDRESS LINE 1::DELIVERY ADDRESS LINE 2++:::ACT+2601+AU'
NAD+CB+54321::95'
MOA+63:15000.00:AUD'
MOA+141:15000.00:AUD'
MOA+39:15000.00:AUD'
UNS+D'
DMS+1'
LIN+1+I'
PAC+120+1'
PCI+1'
RFF+MWB:08155555555'
PCI+1'
RFF+HWB:HOUSE BILL'
CST+1+I::95+N20::95'
FTX+AAA+++FLOOR TILES'
LOC+27+AU::5'
MEA+AAA++SM:150.00'
NAD+SU+66015286036::95'
MOA+38:15000.00:AUD'
RFF+ABD:39181000'
RFF+AED:23'
RFF+AFV:TV'
UNS+S'
UNT+44+1'".Replace("\r\n", "");

		string ValidIMDAmendmentMessageText => @"UNH+1+CUSDEC:D:99B:UN'
BGM+929:::IMD+B00148229/1/SYD7:7+4'
CST++N20::95'
LOC+8+AUSYD::6'
LOC+12+AUSYD::6'
LOC+9+SGSIN::6'
LOC+79+AUSYD::6'
DTM+178:20050610:102'
DTM+252:20050610:102'
DTM+260:20050610:102'
GIS+N:153:95'
MEA+AAE+G+KG:150.00000'
FTX+DEL+++DELIVERY NAME'
RFF+ABQ:OWNERS REF'
RFF+ADU:B00148229'
RFF+APH:FOB'
TDT+20++A++QF::3'
NAD+AT+51006765546::95'
NAD+VT+AA33HF::95'
NAD+DP++CANBERRA++DELIVERY ADDRESS LINE 1::DELIVERY ADDRESS LINE 2++:::ACT+2601+AU'
NAD+CB+54321::95'
MOA+63:15000.00:AUD'
MOA+141:15000.00:AUD'
MOA+39:15000.00:AUD'
UNS+D'
DMS+1'
LIN+1+I'
PAC+120+1'
PCI+1'
RFF+MWB:08155555555'
PCI+1'
RFF+HWB:HOUSE BILL'
CST+1+I::95+N20::95'
FTX+AAA+++FLOOR TILES'
LOC+27+AU::5'
MEA+AAA++SM:150.00'
NAD+SU+66015286036::95'
MOA+38:15000.00:AUD'
RFF+ABD:39181000'
RFF+AED:23'
RFF+AFV:TV'
UNS+S'
UNT+44+1'".Replace("\r\n", "");

		string ValidIMDWithdrawalMessageText => @"UNH+1+CUSDEC:D:99B:UN'
BGM+929:::IMD+B00148229/1/SYD7:7+50'
CST++N20::95'
LOC+8+AUSYD::6'
LOC+12+AUSYD::6'
LOC+9+SGSIN::6'
LOC+79+AUSYD::6'
DTM+178:20050610:102'
DTM+252:20050610:102'
DTM+260:20050610:102'
GIS+N:153:95'
MEA+AAE+G+KG:150.00000'
FTX+DEL+++DELIVERY NAME'
RFF+ABQ:OWNERS REF'
RFF+ADU:B00148229'
RFF+APH:FOB'
TDT+20++A++QF::3'
NAD+AT+51006765546::95'
NAD+VT+AA33HF::95'
NAD+DP++CANBERRA++DELIVERY ADDRESS LINE 1::DELIVERY ADDRESS LINE 2++:::ACT+2601+AU'
NAD+CB+54321::95'
MOA+63:15000.00:AUD'
MOA+141:15000.00:AUD'
MOA+39:15000.00:AUD'
UNS+D'
DMS+1'
LIN+1+I'
PAC+120+1'
PCI+1'
RFF+MWB:08155555555'
PCI+1'
RFF+HWB:HOUSE BILL'
CST+1+I::95+N20::95'
FTX+AAA+++FLOOR TILES'
LOC+27+AU::5'
MEA+AAA++SM:150.00'
NAD+SU+66015286036::95'
MOA+38:15000.00:AUD'
RFF+ABD:39181000'
RFF+AED:23'
RFF+AFV:TV'
UNS+S'
UNT+44+1'".Replace("\r\n", "");

		sealed class TestIMDStatusCalculatorClass : IMDStatusCalculator
		{
			public TestIMDStatusCalculatorClass(CusEntryHeader entryHeader)
				: base(entryHeader)
			{
			}

			public bool UseInterestedMessageTypesExposed;

			public ZString[] InterestedMessageTypesExposed = System.Array.Empty<ZString>();

			public new void DeriveStatus() => base.DeriveStatus();

			protected internal override ZString[] InterestedMessageTypes
			{
				get
				{
					if (!UseInterestedMessageTypesExposed)
					{
						return base.InterestedMessageTypes;
					}
					return InterestedMessageTypesExposed;
				}
			}

			protected internal override ZString GetStatusFromInboundMessage(EDIMessage message) => message.EM_MessageText;

			protected internal override ZPropertyInfo StatusInfo => Parent.CH_StatusInfo;
		}
	}
}
