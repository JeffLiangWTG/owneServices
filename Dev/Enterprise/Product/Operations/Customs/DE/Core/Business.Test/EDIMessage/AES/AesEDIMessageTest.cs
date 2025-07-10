using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(AesEDIMessage))]
	public class AesEDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsAesSystem, message.EM_ApplicationCode);
				AssertEquals("EM_MessageType", Messaging.EDIMessageTypeList.Codes.AES, message.EM_MessageType);
			});
		}

		public void TestMessageNum()
		{
			message.EM_LinkedObject = entry;
			Factory.Save();
			AssertEquals("EDIEDIDAT00000000000001", message.EM_MessageNum);

			var message2 = Factory.New<AesEDIMessage>();
			message2.EM_LinkedObject = entry;
			Factory.Save();
			AssertEquals("EDIEDIDAT00000000000002", message2.EM_MessageNum);
		}

		public void TestFillInPlaceHolders()
		{
			message.EM_MessageText = $"<InterchangeControlReference>{EDIInterchange.InterchangeNumberPlaceHolderHtml}</InterchangeControlReference><MessageIdentifier>{AesEDIMessage.SendersReferencePlaceHolderHtml}</MessageIdentifier>";
			message.EM_LinkedObject = entry;
			Factory.Save();
			AssertContains(@"<InterchangeControlReference>00000000000001</InterchangeControlReference><MessageIdentifier>EDIEDIDAT00000000000001</MessageIdentifier>", message.EM_MessageText);
		}

		public void TestEM_LinkedObject_CusEntryHeader()
		{
			message.EM_LinkTable = entry.TableName;
			message.EM_LinkUniqueID = entry.PK;
			AssertEquals(entry, message.EM_LinkedObject);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			message = Factory.New<AesEDIMessage>();
		}

		CusEntryHeader entry;
		AesEDIMessage message;
	}
}
