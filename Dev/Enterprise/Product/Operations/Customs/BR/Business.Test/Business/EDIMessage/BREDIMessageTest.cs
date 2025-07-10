using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(BREDIMessage))]
	public class BREDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<BREDIMessage>();
			AssertEquals(EDIInterchange.ApplicationCodes.BRCustoms, message.EM_ApplicationCode);
		}

		public void TestNewAndLoadType()
		{
			var testMessage = Factory.NewWithValidTestData<EDIMessage>();
			testMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			testMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.BRCustoms;
			testMessage.EM_Status = EDIInterchangeStatusList.Codes.Queued;
			testMessage.EM_MessageText = "BBB";
			testMessage.EM_MessageType = MessageTypeList.Codes.CDC;
			testMessage.EM_MessageNum = "1234567890123456789012345678901234A";
			AssertEquals("EDIMessage", typeof(EDIMessage), testMessage.GetType());
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			testMessage = newFactory.Load<BREDIMessage>(testMessage.PK);
			AssertEquals("BREDIMessage", typeof(BREDIMessage), testMessage.GetType());
		}

		public void TestMessageSubTypeDescription()
		{
			BREDIMessage message = Factory.New<BREDIMessage>();
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Original;
			AssertEquals("Original Description", EDIMessageSubTypeList.Descriptions.Original, message.EM_MessageSubTypeDescription);

			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Rectification;
			AssertEquals("Rectification Description", EDIMessageSubTypeList.Descriptions.Rectification, message.EM_MessageSubTypeDescription);
		}

		public void TestShouldNotReplaceUniqueBatchNumbers()
		{
			var messageText = "<a>Hello World " + EDIMessage.UniqueBatchNumberPlaceHolderHtml + " Hello World</a>";

			var message = Factory.New<BREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.LIC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			message.EM_MessageText = messageText;

			AssertEquals("UniqueBatchNumberPlaceHolder NOT replaced", messageText, message.EM_MessageText);
		}

		public void TestIsImportLicense()
		{
			var message = Factory.New<BREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.LIC;
			AssertEquals("IsImportLicense method should be true", true, message.IsImportLicense);

			message.EM_MessageType = MessageTypeList.Codes.CDC;
			AssertEquals("IsImportLicense method should be false", false, message.IsImportLicense);
		}

		public void TestIsProductMessage()
		{
			var message = Factory.New<BREDIMessage>();
			AssertEquals("IsImportLicense method should be true", false, message.IsProductMessage);

			message.EM_MessageType = MessageTypeList.Codes.CAT;
			AssertEquals("IsImportLicense method should be true", false, message.IsProductMessage);

			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Original;
			AssertEquals("IsImportLicense method should be false", true, message.IsProductMessage);
		}

		public void TestIsForeignOperatorMessage()
		{
			var message = Factory.New<BREDIMessage>();
			AssertEquals("IsImportLicense method should be true", false, message.IsForeignOperatorMessage);

			message.EM_MessageType = MessageTypeList.Codes.OPE;
			AssertEquals("IsImportLicense method should be true", false, message.IsForeignOperatorMessage);

			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Original;
			AssertEquals("IsImportLicense method should be false", true, message.IsForeignOperatorMessage);
		}

		public void TestPopulateMessageNumber()
		{
			var message = Factory.NewWithValidTestData<BREDIMessage>();
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			AssertEquals(string.Empty, message.EM_MessageNum);

			Factory.Save();
			AssertEquals("1", message.EM_MessageNum);

			message = Factory.NewWithValidTestData<BREDIMessage>();
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			message.EM_MessageNum = "10";

			Factory.Save();
			AssertEquals("10", message.EM_MessageNum);
		}

		public void TestIsProductLinkMessage()
		{
			var message = Factory.New<BREDIMessage>();
			AssertEquals("IsProductLinkMessage method should be true", false, message.IsProductLinkMessage);

			message.EM_MessageType = MessageTypeList.Codes.CAT;
			AssertEquals("IsProductLinkMessage method should be true", false, message.IsProductLinkMessage);

			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Original;
			AssertEquals("IsProductLinkMessage method should be true", false, message.IsProductLinkMessage);

			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Link;
			AssertEquals("IsProductLinkMessage method should be false", true, message.IsProductLinkMessage);
		}
	}
}
