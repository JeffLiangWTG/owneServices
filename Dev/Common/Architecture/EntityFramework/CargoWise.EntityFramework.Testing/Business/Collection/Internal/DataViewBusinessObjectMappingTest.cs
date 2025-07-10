using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DataViewBusinessObjectMappingTest : TestCase
	{
		public void TestIt()
		{
			for (int i = 1000; i <= 1999; i++)
			{
				Mapping[i] = i;
			}
			AssertEquals(-1, Mapping[999]);
			AssertEquals(1000, Mapping[1000]);
			AssertEquals(1500, Mapping[1500]);
			AssertEquals(1999, Mapping[1999]);
			AssertEquals(-1, Mapping[2000]);

			for (int i = 2000; i <= 2999; i++)
			{
				Mapping[i] = i;
			}
			for (int i = 1; i <= 999; i++)
			{
				Mapping[i] = i;
			}

			for (int i = 1; i <= 2999; i++)
			{
				AssertEquals(i, Mapping[i]);
			}
			AssertEquals(-1, Mapping[0]);
			AssertEquals(-1, Mapping[3000]);
		}

		readonly DataViewBusinessObjectMapping Mapping = new DataViewBusinessObjectMapping();
	}
}
