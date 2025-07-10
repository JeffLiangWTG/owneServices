using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(BREDIInterchange))]
	class BREDIInterchangeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var interchange = Factory.New<BREDIInterchange>();
			AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.BRCustoms, interchange.EI_ApplicationCode);
		}

		public void TestShouldSendViaeHub()
		{
			CombineAssertions(() =>
			{
				var interchange = Factory.New<BREDIInterchange>();
				AssertEquals("EI_ApplicationCode", "BRC", interchange.EI_ApplicationCode);
				AssertEquals("ShouldSendViaEHub - eHub", true, interchange.ShouldSendViaEHub);

				interchange.EI_TransportType = EDIInterchange.TransportType.xT;
				AssertEquals("ShouldSendViaEHub - DirectxT", false, interchange.ShouldSendViaEHub);
			});
		}

		public void TestNewAndLoadType()
		{
			var interchange = Factory.NewWithValidTestData<BREDIInterchange>();
			interchange.EI_From = "BRCustoms.TEST";
			interchange.EI_To = "TEST";
			interchange.EI_ApplicationCode = "BRC";
			interchange.EI_InterchangeType = "BRC";
			interchange.EI_InterchangeNum = "XXXX.ABCD";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = ZString.Empty;
			interchange.EI_InterchangeNum = "1234567890123456789012345678901234567890123456789012345678901234";
			AssertEquals("BRCInterchange", typeof(BREDIInterchange), interchange.GetType());
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			interchange = newFactory.Load<BREDIInterchange>(interchange.PK);
			AssertEquals("BRCInterchange", typeof(BREDIInterchange), interchange.GetType());
		}
		public void TestReplaceUniqueBatchNumbers()
		{
			var expectedMessageText = "<a>Hello World 000001 Hello World</a>";
			var messageText = "<a>Hello World " + EDIMessage.UniqueBatchNumberPlaceHolderHtml + " Hello World</a>";
			var interchange = Factory.NewWithValidTestData<BREDIInterchange>();
			interchange.EI_InterchangeType = MessageTypeList.Codes.LIC;
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			interchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			interchange.EI_BodyText = messageText;

			var message1 = Factory.New<BREDIMessage>();
			message1.EM_MessageType = MessageTypeList.Codes.LIC;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			message1.EM_MessageText = messageText;

			var message2 = Factory.New<BREDIMessage>();
			message2.EM_MessageType = MessageTypeList.Codes.LIC;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			message2.EM_MessageText = messageText;

			interchange.ContainedMessages.Add(message1);
			interchange.ContainedMessages.Add(message2);

			Factory.Save();

			AssertEquals("UniqueBatchNumberPlaceHolder replaced", expectedMessageText, interchange.EI_BodyText);
			AssertEquals("UniqueBatchNumberPlaceHolder replaced", expectedMessageText, message1.EM_MessageText);
			AssertEquals("UniqueBatchNumberPlaceHolder replaced", expectedMessageText, message2.EM_MessageText);
			AssertEquals("EM_ApplicationReference set", "000001", message1.EM_ApplicationReference);
			AssertEquals("EM_ApplicationReference set", "000001", message2.EM_ApplicationReference);
		}

		public void TestGetMessageData()
		{
			var licenseKey = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID;

			using (BRCustomsDataRegistry.Instance.XTEndPointAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://xttest-customs-brpushnotification.wisegrid.net/BRC"))
			{
				var interchange = Factory.NewWithValidTestData<BREDIInterchange>();
				interchange.EI_InterchangeType = MessageTypeList.Codes.LIC;
				interchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
				interchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
				interchange.EI_BodyText = JsonSubscrptionMessage;

				using (var reader = (interchange as IMessageDataProvider).GetMessageData())
				{
					AssertEquals("PLACE HOLDER not replaced", JsonSubscrptionMessage, ReadAsString(reader));
				}

				interchange.EI_InterchangeType = MessageTypeList.Codes.SUB;

				using (var reader = (interchange as IMessageDataProvider).GetMessageData())
				{
					AssertEquals("PLACE HOLDER replaced", GetExpectedSubscrptionMessage(licenseKey), ReadAsString(reader));
				}
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<BREDIInterchange>();
		}

		const string JsonSubscrptionMessage = @"{ ""id"": 1, ""evento"": ""duex-historico"", ""endpoint"": ""<<ENDPOINT PLACE HOLDER>>"" }";

		string GetExpectedSubscrptionMessage(string licenseKey) => $@"{{ ""id"": 1, ""evento"": ""duex-historico"", ""endpoint"": ""https://xttest-customs-brpushnotification.wisegrid.net/BRC/{licenseKey}"" }}";
		string ReadAsString(BinaryReader reader)
		{
			var bytes = new byte[reader.BaseStream.Length];
			reader.BaseStream.Read(bytes, 0, bytes.Length);
			return Encoding.UTF8.GetString(bytes);
		}
	}
}
