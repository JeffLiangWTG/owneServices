using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(AISOutboundEDIMessage))]
	sealed class AISOutboundEDIMessageTest : Enterprise.Messaging.Testing.EDIMessageTest
	{
		public void TestIMessageTypeProviderMembers()
		{
			var message = Factory.New<AISOutboundEDIMessage>();
			message.EM_MessageType = "515";
			EU.Business.CusTempStorage.IMessageTypeProvider provider = message;
			AssertEquals("MessageType", "515", provider.MessageType);
		}

		public void TestSetDefaultValues()
		{
			var message = Factory.New<AISOutboundEDIMessage>();
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.IECustomsImport, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", Enterprise.Messaging.Business.EDIInterchange.Direction.Transmit, message.EM_ReceiveTransmit);
			});
		}

		public void TestGetMessageReferenceNumber()
		{
			var message1 = Factory.New<AISOutboundEDIMessage>();
			Factory.Save();
			AssertEquals("Should have created MessageNumber(001) from IEMessageControlNumber sequense", "IEI00000000000001", message1.EM_MessageNum);

			var message2 = Factory.New<AISOutboundEDIMessage>();
			Factory.Save();
			AssertEquals("Should have created MessageNumber(002) from IEMessageControlNumber sequense", "IEI00000000000002", message2.EM_MessageNum);
		}

		[TestDate(2024, 02, 04)]
		public void TestSendersReferencePlaceHolderReplacement()
		{
			Env.NumberFountains.GetIENumberFountain("IEAISRFApplicationReferenceId").SetNext(Factory, 3);
			Env.NumberFountains.GetIENumberFountain("IEAISRDApplicationReferenceId").SetNext(Factory, 5);
			var declaration = Factory.New<JobDeclaration>();

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var message1 = Factory.New<AISOutboundEDIMessage>();
			message1.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIInterchange.Direction.Transmit;
			message1.EM_MessageType = AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtF15;
			message1.EM_MessageText = $"<ApplicationReferenceId>{AISOutboundEDIMessage.RF415ApplicationReferenceIdPlaceHolder}</ApplicationReferenceId>";
			entryHeader1.Messages.Add(message1);

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var message2 = Factory.New<AISOutboundEDIMessage>();
			message2.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIInterchange.Direction.Transmit;
			message2.EM_MessageType = AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtR15;
			message2.EM_MessageText = $"<ApplicationReferenceId>{AISOutboundEDIMessage.RD415ApplicationReferenceIdPlaceHolder}</ApplicationReferenceId>";
			entryHeader2.Messages.Add(message2);

			var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
			var message3 = Factory.New<AISOutboundEDIMessage>();
			message3.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIInterchange.Direction.Transmit;
			message3.EM_MessageType = AISOutgoingMessageTypeList.Codes.CustomsDeclaration;
			message3.EM_MessageText = $"<CustomsDeclaration>{AISOutboundEDIMessage.LRNPlaceHolder}</CustomsDeclaration>";
			entryHeader3.Messages.Add(message3);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("<ApplicationReferenceId>0000000000000000000003</ApplicationReferenceId>", message1.EM_MessageText);
				AssertEquals("<ApplicationReferenceId>000000000000000005</ApplicationReferenceId>", message2.EM_MessageText);
				AssertEquals("<CustomsDeclaration>EDIDATBNE2400000001V01</CustomsDeclaration>", message3.EM_MessageText);
			});
		}
	}
}
