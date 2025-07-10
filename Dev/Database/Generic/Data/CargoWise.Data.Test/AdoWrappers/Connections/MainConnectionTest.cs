using System;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class MainConnectionTest : TransactionedTestCase
	{
		public void TestMainConnectionDoesNotResolve()
		{
			DbEnv.SetDbEnvironment(new BaseDbEnvironment());

			using var connectionToNonExistantServer = new MainConnectionForTest(new TimeSpan(0));

			AssertExceptionThrown<LoginException>(() =>
			{
				using var cmd = connectionToNonExistantServer.Command("GIMME AN ERROR");
			});
		}

		IDbEnvironment originalDbEnvironment;
		protected override void SetUp()
		{
			base.SetUp();
			originalDbEnvironment = DbEnv.Instance;
		}

		protected override void TearDown()
		{
			DbEnv.SetDbEnvironment(originalDbEnvironment);
			base.TearDown();
		}

		public void TestReconnectQuestionIsNotAskedIndefinantly()
		{
			DbEnv.SetDbEnvironment(new BaseDbEnvironment());
			var mainConnection = new MainConnectionForTest(TimeSpan.FromMilliseconds(500));

			Assert("1st Attempt", mainConnection.ConfirmReopenConnection_Exposed());
			Assert("2nd Attempt", mainConnection.ConfirmReopenConnection_Exposed());
			Assert("3rd Attempt", mainConnection.ConfirmReopenConnection_Exposed());

			Assert("Final attempt", !mainConnection.ConfirmReopenConnection_Exposed());
		}

		[SnailTest]
		public void TestReconnectQuestionConsidersTime()
		{
			DbEnv.SetDbEnvironment(new BaseDbEnvironment());
			var mainConnection = new MainConnectionForTest(TimeSpan.FromMilliseconds(500));

			for (int i = 0; i < 5; i++)
			{
				Assert(mainConnection.ConfirmReopenConnection_Exposed());
				Thread.Sleep(200);
			}
		}
	}
}
