using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(AISUCC5OutboundEDIMessage))]
	sealed class AISUCC5OutboundEDIMessageTest : Enterprise.Messaging.Testing.EDIMessageTest
	{
		public void TestIMessageTypeProviderMembers()
		{
			var message = Factory.New<AISUCC5OutboundEDIMessage>();
			message.EM_MessageType = "515";
			EU.Business.CusTempStorage.IMessageTypeProvider provider = message;
			AssertEquals("MessageType", "515", provider.MessageType);
		}

		public void TestSetDefaultValues()
		{
			var message = Factory.New<AISUCC5OutboundEDIMessage>();
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.IECustomsUCC5Import, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", Enterprise.Messaging.Business.EDIInterchange.Direction.Transmit, message.EM_ReceiveTransmit);
			});
		}

		public void TestGetMessageReferenceNumber()
		{
			var message1 = Factory.New<AISUCC5OutboundEDIMessage>();
			Factory.Save();
			AssertEquals("Should have created MessageNumber(001) from IEMessageControlNumber sequense", "IE500000000000001", message1.EM_MessageNum);

			var message2 = Factory.New<AISUCC5OutboundEDIMessage>();
			Factory.Save();
			AssertEquals("Should have created MessageNumber(002) from IEMessageControlNumber sequense", "IE500000000000002", message2.EM_MessageNum);
		}

		[TestDate(2024, 02, 04)]
		public void TestSendersReferencePlaceHolderReplacement()
		{
			Env.NumberFountains.GetIENumberFountain("IEAISRFApplicationReferenceId").SetNext(Factory, 3);
			var declaration = Factory.New<JobDeclaration>();

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var message1 = Factory.New<AISUCC5OutboundEDIMessage>();
			message1.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIInterchange.Direction.Transmit;
			message1.EM_MessageType = AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtF15;
			message1.EM_MessageText = $"<ApplicationReferenceId>{AISCommonOutboundEDIMessage.RF415ApplicationReferenceIdPlaceHolder}</ApplicationReferenceId>";
			entryHeader1.Messages.Add(message1);

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var message2 = Factory.New<AISUCC5OutboundEDIMessage>();
			message2.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIInterchange.Direction.Transmit;
			message2.EM_MessageType = AISOutgoingMessageTypeList.Codes.CustomsDeclaration;
			message2.EM_MessageText = $"<CustomsDeclaration>{AISCommonOutboundEDIMessage.LRNPlaceHolder}</CustomsDeclaration>";
			entryHeader2.Messages.Add(message2);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("<ApplicationReferenceId>0000000000000000000003</ApplicationReferenceId>", message1.EM_MessageText);
				AssertEquals("<CustomsDeclaration>EDIDATBNE2400000001V01</CustomsDeclaration>", message2.EM_MessageText);
			});
		}
	}
}
