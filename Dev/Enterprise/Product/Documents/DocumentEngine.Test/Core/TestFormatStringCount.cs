using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class TestFormatStringCount : TestFormatStringFunction
	{
		public override void TestMatch()
		{
			var count = new FormatStringCount();

			AssertEquals(true, count.Match("Count"));
			AssertEquals(true, count.Match("Count()"));
			AssertEquals(true, count.Match("COUNT"));
			AssertEquals(true, count.Match("cOUnt()"));
			AssertEquals(false, count.Match(" Count "));
		}

		public override void TestExecute()
		{
			var count = new FormatStringCount();

			AssertEquals(ZString.Empty, count.Execute(null));

			var list = new List<ObjectForTest>();
			list.Add(new ObjectForTest());
			list.Add(new ObjectForTest());

			AssertEquals(2, count.Execute(list));

			list.Add(new ObjectForTest());

			AssertEquals(3, count.Execute(list));
		}
	}
}
