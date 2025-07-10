using System.Threading;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	sealed class SingletonApplicationRunnerTest : TestCase
	{
		public void TestRunWithDifferentConfigNameButSameServiceURL()
		{
			var readyHandle1 = new EventWaitHandle(false, EventResetMode.ManualReset);
			var readyHandle2 = new EventWaitHandle(false, EventResetMode.ManualReset);
			var readyHandle3 = new EventWaitHandle(false, EventResetMode.ManualReset);

			var runner1 = new SingletonApplicationRunnerForTest("https://test1.com");
			var runner2 = new SingletonApplicationRunnerForTest("http://test1.com");

			var thread1 = new Thread(() =>
			{
				runner1.Run(() =>
				{
					readyHandle2.Set();
					readyHandle1.WaitOne();
				}, "ConfigTest1");
			});
			thread1.Start();
			readyHandle2.WaitOne();

			var thread2 = new Thread(() =>
			{
				runner2.Run(() => { }, "ConfigTest2");
				readyHandle3.Set();
			});
			thread2.Start();
			readyHandle3.WaitOne();
			readyHandle1.Set();

			thread1.Join();
			thread2.Join();

			AssertEquals("Another instance of the CargoWise One WebPrint Client is already running for this web url.", runner2.AnotherCopyRunningMessage);
		}

		class SingletonApplicationRunnerForTest : SingletonApplicationRunner
		{
			public SingletonApplicationRunnerForTest(string webServiceUrl)
			{
				WebServiceUrl = webServiceUrl;
			}

			readonly string WebServiceUrl;

			protected override string GetWebServiceUrl(string configName) => WebServiceUrl;
		}
	}
}
