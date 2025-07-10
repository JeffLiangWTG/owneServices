using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class HashCodeHelperTest : TestCase
	{
		public void TestHashCode()
		{
			var objs = new object[] { "Apple", "Orange", null, 99 };
			var hashCode = HashCodeHelper.GetCompositeHashCode(objs);
			AssertEquals(1304247792, hashCode);
			hashCode = HashCodeHelper.GetCompositeHashCode(37, objs);
			AssertEquals(1304491132, hashCode);
			hashCode = HashCodeHelper.GetCompositeHashCode(37, 100);
			AssertEquals(951, hashCode);
		}
	}
}