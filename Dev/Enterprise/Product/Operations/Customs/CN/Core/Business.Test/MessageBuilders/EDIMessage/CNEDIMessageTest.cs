using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNEDIMessage))]
	class CNEDIMessageTest : EDIMessageTest
	{
		public void TestMessageNum()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var message1 = Factory.New<CNEDIMessage>();
			message1.EM_LinkedObject = entry;
			Factory.Save();
			AssertEquals("00000000000000000001", message1.EM_MessageNum);

			var message2 = Factory.New<CNEDIMessage>();
			message2.EM_LinkedObject = entry;
			Factory.Save();
			AssertEquals("00000000000000000002", message2.EM_MessageNum);
		}

		public void TestMessageDefaults()
		{
			var message = Factory.New<CNEDIMessage>();
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.CNCustomsSingleWindow, message.EM_ApplicationCode);
		}
	}
}
