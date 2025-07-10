using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class LVXJobNotificationCollectorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestNotifyUserOfAnInvalidOperation()
		{
			var currentMessage = string.Empty;
			var collector = new LVXJobNotificationCollector((logType, message, args) =>
			{
				currentMessage = string.Format(message, args);
			});
			collector.NotifyUserOfAnInvalidOperation("An Invalid Operation");
			AssertEquals("An Invalid Operation", currentMessage);
		}

		public void TestNotifyUserOfAnInvalidOperation_NullLog()
		{
			AssertNoExceptionThrown(() =>
			{
				new LVXJobNotificationCollector(null).NotifyUserOfAnInvalidOperation("An Invalid Operation");
			});
		}
	}
}
