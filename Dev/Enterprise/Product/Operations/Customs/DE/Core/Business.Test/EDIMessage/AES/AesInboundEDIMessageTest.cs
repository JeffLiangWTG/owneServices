using CargoWise.Customs.DE.MessageContracts.AES;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(AesInboundEDIMessage<IEXPNOT>))]
	class AesInboundEDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsAesSystem, message.EM_ApplicationCode);
				AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.AES, message.EM_MessageType);
			});
		}

		public void TestEM_LinkedObject_CusEntryHeader()
		{
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			message.EM_LinkTable = cusEntryHeader.TableName;
			message.EM_LinkUniqueID = cusEntryHeader.PK;
			AssertEquals(cusEntryHeader, message.EM_LinkedObject);
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<AesInboundEDIMessage<IEXPNOT>>();
		}
		protected AesInboundEDIMessage<IEXPNOT> message;
	}
}
