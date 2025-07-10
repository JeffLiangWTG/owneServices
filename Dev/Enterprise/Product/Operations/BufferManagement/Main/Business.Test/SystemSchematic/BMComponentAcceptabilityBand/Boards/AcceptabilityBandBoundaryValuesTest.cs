using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class AcceptabilityBandBoundaryValuesTest : TestCase
	{
		public void TestEquals()
		{
			var values1 = new AcceptabilityBandBoundaryValues(1, 2, 3, 4, 5, 6);
			var values2 = new AcceptabilityBandBoundaryValues(1, 2, 3, 4, 5, 6);
			var values3 = new AcceptabilityBandBoundaryValues(1, 1, 3, 4, 5, 6);

			AssertEquals(values1, values2);
			AssertNotEquals(values1, values3);
		}
	}
}
