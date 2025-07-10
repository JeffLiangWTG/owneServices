using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Messaging.Testing
{
	sealed class EDIMessageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEM_MessageText()
		{
			var message = Factory.New<TestEdiMessage>();
			AssertEquals("no errors", false, message.EM_MessageTextInfo.HasNotifications());

			message.EM_MessageText = "This \r is \n is \t a test ?' message'. This" + '\x1f' + "format" + '\x1d' + "I don't" + '\x1c' + " understand?";
			AssertEquals("no errors", false, message.EM_MessageTextInfo.HasNotifications());

			message.EM_MessageText = message.EM_MessageText + '\u03C0';
			AssertEquals("invalid", true, message.EM_MessageTextInfo.HasError(EnglishCharactersValidation.GetNotificationMessage(message.EM_MessageTextInfo)));
		}

		public void TestSizeCheckValidationIsSuppressedAsMessagesTendToBig()
		{
			var reallyBigString = new string('*', 5000000);

			var message = Factory.NewWithValidTestData<TestEdiMessage>();
			message.EM_MessageData = ZBlob.FromUTF8(reallyBigString);
			AssertNoErrors(message.EM_MessageDataInfo);
		}
	}
}
