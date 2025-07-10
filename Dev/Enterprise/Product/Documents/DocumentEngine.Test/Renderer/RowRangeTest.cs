using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class RowRangeTest : TestCase
	{
		public RowRangeTest() : base()
		{
		}

		public void TestConstructor()
		{
			RowRange testRange = new RowRange(1, 33);
			AssertEquals(1, testRange.Start);
			AssertEquals(33, testRange.End);
		}
	}
}
