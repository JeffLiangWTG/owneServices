using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave.Testing
{
	class LogSubscriberToAbortLogGroupProcessingSilentlyExceptionTest : TestCase
	{
		public void TestContructor()
		{
			var expectedMessage = "Some Message";
			var expectedEmailDef1 = new Mock<IEmailCreator>();
			var expectedEmailDef2 = new Mock<IEmailCreator>();
			var exception = new LogSubscriberToAbortLogGroupProcessingSilentlyException(expectedMessage, expectedEmailDef1.Object, expectedEmailDef2.Object);
			AssertEquals("Message", expectedMessage, exception.Message);
			AssertCollectionContains("Email1", expectedEmailDef1.Object, exception.Emails);
			AssertCollectionContains("Email2", expectedEmailDef2.Object, exception.Emails);
		}
	}
}
