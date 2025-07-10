using System;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CADMessage))]
	sealed class CADMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBatchNumber()
		{
			message.EM_MessageText = "<ApplicationReferenceID>00001001</ApplicationReferenceID>'";
			AssertEquals("BatchNumber", "00001", message.BatchNumber);

			message.EM_MessageText = "00001001'";
			AssertEquals("BatchNumber", "", message.BatchNumber);

			message.EM_MessageText = "<ApplicationReferenceID>12345678'";
			AssertEquals("BatchNumber", "12345", message.BatchNumber);
		}

		public void TestGetLatestCADResponseMessage()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var message1 = Factory.New<CADMessage>();
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message1.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			message1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message1.EM_Status = EDIMessageStatusList.Codes.Received;
			message1.EM_MessageSubType = MessageStatusList.Codes.ClearOriginal;
			message1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-5);
			var message2 = Factory.New<B3Message>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message2.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			message2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message2.EM_Status = EDIMessageStatusList.Codes.Received;
			message2.EM_MessageSubType = MessageStatusList.Codes.ClearOriginal;
			message2.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-4);
			var message3 = Factory.New<CADMessage>();
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message3.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			message3.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message3.EM_Status = EDIMessageStatusList.Codes.Received;
			message3.EM_MessageSubType = MessageStatusList.Codes.ClearOriginal;
			message3.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-3);
			var message4 = Factory.New<CADMessage>();
			message4.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message4.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			message4.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message4.EM_Status = EDIMessageStatusList.Codes.Error;
			message4.EM_MessageSubType = MessageStatusList.Codes.ClearOriginal;
			message4.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-2);
			var message5 = Factory.New<CADMessage>();
			message5.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message5.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			message5.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message5.EM_Status = EDIMessageStatusList.Codes.Received;
			message5.EM_MessageSubType = MessageStatusList.Codes.AwaitingOriginal;
			message5.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			entryHeader.Messages.Add(message1);
			entryHeader.Messages.Add(message2);
			entryHeader.Messages.Add(message3);
			entryHeader.Messages.Add(message4);
			entryHeader.Messages.Add(message5);
			AssertEquals(message1, CADMessage.GetLatestCADResponseMessage(entryHeader));

			AssertExceptionThrown<ArgumentNullException>(() => CADMessage.GetLatestCADResponseMessage(null));

			message5.EM_MessageSubType = MessageStatusList.Codes.ClearDelete;
			AssertEquals(message1, CADMessage.GetLatestCADResponseMessage(entryHeader));

			message5.EM_MessageSubType = MessageStatusList.Codes.ClearReplace;
			AssertEquals(message1, CADMessage.GetLatestCADResponseMessage(entryHeader));

			message5.EM_MessageSubType = MessageStatusList.Codes.ClearChange;
			AssertEquals(message5, CADMessage.GetLatestCADResponseMessage(entryHeader));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			message = (CADMessage)GetNewBusinessObject();
			message.EM_MessageText = "Message Number = " + EDIMessage.MessageNumberPlaceHolder;
		}

		CADMessage message;

		#endregion
	}
}
