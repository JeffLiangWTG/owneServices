using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	class UberWatchTest : TestCase
	{
		public void TestRollOver()
		{
			int tickCount = int.MaxValue;
			int TickCountFunc()
			{
				return tickCount;
			}

			var uberWatch = new UberWatch(TickCountFunc);
			uberWatch.Start();
			AssertEquals(0, uberWatch.ElapsedMilliseconds);
			tickCount = unchecked(tickCount + 50);
			AssertEquals(50, uberWatch.ElapsedMilliseconds);
			tickCount = unchecked((int)uint.MaxValue);
			uberWatch.Restart();
			AssertEquals(0, uberWatch.ElapsedMilliseconds);
			tickCount = unchecked(tickCount + 50);
			AssertEquals(50, uberWatch.ElapsedMilliseconds);
		}
	}
}