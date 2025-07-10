using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business.Testing
{
	class ExtensionsTest : TestCaseWithFactory
	{
		public void TestJoinAsString()
		{
			AssertEquals("1,2,3", new[] { 1, 2, 3, 3 }.JoinAsString());
			AssertEquals("1;2;3", new[] { "1", "2", "3", "3" }.JoinAsString(";"));
			AssertEquals("1;2;3;3", new[] { 1, 2, 3, 3 }.JoinAsString(';', false));
		}

		public void TestDistinctSortAndJoinForDisplay()
		{
			var strings = new List<ZString> { "one", "two", "three", "three" };
			AssertEquals("default behavior", "one;three;two", strings.DistinctSortAndJoinForDisplay());
			AssertEquals("separator", "one,three,two", strings.DistinctSortAndJoinForDisplay(","));

			var dates = new List<ZDateTime> { new ZDateTime(2022, 02, 22), new ZDateTime(2022, 02, 01), ZDateTime.Empty, new ZDateTime(2022, 02, 15) };
			AssertEquals("filter empty elements and format", "20220201;20220215;20220222", dates.DistinctSortAndJoinForDisplay(format: x => x.ToString("yyyyMMdd")));
		}
	}
}
