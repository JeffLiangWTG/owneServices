using NUnit.Framework;

namespace CargoWise.IO.Testing
{
	public class PdhWrapperTest : TestCase
	{
		public void TestGetCounter()
		{
			string[] pdhCounters = new string[] { @"\Objects(_Total)\Processes", @"\Process(_Total)\Thread Count", @"\Process(_Total)\Handle Count" };

			foreach (string pdhCounter in pdhCounters)
			{
				double count = PdhWrapper.GetCounter(pdhCounter);
				Assert("PDH counter must be > 0", count > 0);
			}

			AssertExceptionThrown(typeof(PdhException),
				"Unable to add a performance counter.\r\nError Code: CStatusNoObject",
				delegate
				{ double invalidCount = PdhWrapper.GetCounter(@"\Lorem ipsum dolor sit amet(_Total)\Handle Count"); });
		}
	}
}
