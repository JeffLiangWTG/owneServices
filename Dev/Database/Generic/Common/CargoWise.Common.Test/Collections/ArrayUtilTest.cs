using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class ArrayUtilTest : TestCase
	{
		public void TestArrayEquals()
		{
			AssertEquals(true, ArrayUtil.ArrayEquals<object>(null, null));
			AssertEquals(false, ArrayUtil.ArrayEquals(null, System.Array.Empty<int>()));
			AssertEquals(false, ArrayUtil.ArrayEquals(System.Array.Empty<int>(), null));
			AssertEquals(true, ArrayUtil.ArrayEquals(System.Array.Empty<int>(), System.Array.Empty<int>()));
			AssertEquals(false, ArrayUtil.ArrayEquals(new int[] { 1 }, System.Array.Empty<int>()));
			AssertEquals(false, ArrayUtil.ArrayEquals(System.Array.Empty<int>(), new int[] { 1 }));
			AssertEquals(true, ArrayUtil.ArrayEquals(new int[] { 1, 2 }, new int[] { 1, 2 }));
			AssertEquals(false, ArrayUtil.ArrayEquals(new int[] { 1, 5 }, new int[] { 1, 2 }));
		}

		public void TestCombine()
		{
			string[] ar1 = new string[] { "1", "2" };
			string[] ar2 = new string[] { "3", "4" };
			string[] result = ArrayUtil.Combine(ar1, ar2);
			AssertEquals("1", result[0]);
			AssertEquals("2", result[1]);
			AssertEquals("3", result[2]);
			AssertEquals("4", result[3]);
		}
	}
}
