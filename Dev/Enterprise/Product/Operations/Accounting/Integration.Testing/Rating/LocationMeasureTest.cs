using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Integration.Testing
{
	public class LocationMeasureTest : TestCase
	{
		public void TestLocationMeasure()
		{
			LocationMeasure m1 = new LocationMeasure(ZGuid.NewZGuid(), "AAA", "Desc 1");
			LocationMeasure m2 = new LocationMeasure(ZGuid.NewZGuid(), "BBB", "Desc 2");
			LocationMeasure m3 = new LocationMeasure(m1.PK, "CCC", "Desc 3");

			Assert(!m1.Equals(m2));
			Assert(m1.Equals(m3));

			AssertNotEquals(0, m1.CompareTo(m2));
			AssertEquals(0, m1.CompareTo(m3));
		}
	}
}
