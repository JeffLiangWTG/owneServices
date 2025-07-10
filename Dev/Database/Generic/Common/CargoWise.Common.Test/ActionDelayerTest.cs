using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class ActionDelayerTest : TestCase
	{
		const int bigNum = 1000;
		public void TestDelay()
		{
			var delayer = new ActionDelayer();
			int i = 0;
			using (delayer.TemporaryChangeToDelayStrategy())
			{
				for (int j = 0; j < bigNum; j++)
				{
					delayer.Do(() => i++);
				}

				AssertEquals(0, i);
			}

			AssertEquals(bigNum, i);
		}

		public void TestDelay_MultiAndReusable()
		{
			var delayer = new ActionDelayer();
			int i = 0;
			using (delayer.TemporaryChangeToDelayStrategy())
			{
				using (delayer.TemporaryChangeToDelayStrategy())
				using (delayer.TemporaryChangeToDelayStrategy())
				{
					for (int j = 0; j < bigNum; j++)
					{
						delayer.Do(() => i++);
					}

					AssertEquals(0, i);
				}

				AssertEquals(0, i);
			}

			AssertEquals(bigNum, i);
		}

		public void TestDelay_RunThemWhenYouWantTo()
		{
			var delayer = new ActionDelayer();
			int i = 0;
			using (delayer.TemporaryChangeToDelayStrategy())
			{
				for (int j = 0; j < bigNum; j++)
				{
					delayer.Do(() => i++);
				}

				Assert(delayer.RunAllDelayed());
				AssertEquals(bigNum, i);
			}

			Assert(!delayer.RunAllDelayed());
			AssertEquals(bigNum, i);
		}

		public void TestDelay_ManualDisposeShouldRun()
		{
			var delayer = new ActionDelayer();
			int i = 0;
			delayer.SetDelayStrategy();
			for (int j = 0; j < bigNum; j++)
			{
				delayer.Do(() => i++);
			}
			AssertEquals(0, i);
			delayer.Dispose();
			AssertEquals(bigNum, i);

			delayer.Do(() => i++);
			AssertEquals("Should be back in Invoke strategy", bigNum + 1, i);
		}

		public void TestDelay_ManualDisposeShouldNotRun()
		{
			var delayer = new ActionDelayer();
			int i = 0;
			delayer.SetDelayStrategy();
			for (int j = 0; j < bigNum; j++)
			{
				delayer.Do(() => i++);
			}
			AssertEquals(0, i);
			delayer.MarkDelayActionsAsUnsafe();
			delayer.Dispose();
			AssertEquals(0, i);

			delayer.Do(() => i++);
			AssertEquals("Should be back in Invoke strategy", 1, i);
			Assert("Action queue should be empty", !delayer.RunAllDelayed());
		}

		public void TestDelay_Ignorable()
		{
			var delayer = new ActionDelayer();
			int i = 0;
			for (int j = 0; j < bigNum; j++)
			{
				delayer.Do(() => i++);
			}

			AssertEquals(bigNum, i);
		}

		public void TestDelay_Recursive()
		{
			var delayer = new ActionDelayer();
			int i = 0;
			using (delayer.TemporaryChangeToDelayStrategy())
			{
				for (int j = 0; j < bigNum; j++)
				{
					delayer.Do(() =>
					{
						i++;
						delayer.Do(() => i++);
					});
				}
			}

			AssertEquals(bigNum * 2, i);
		}
	}
}
