using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class TestFormatStringSum : TestFormatStringFunction
	{
		public override void TestMatch()
		{
			var sum = new FormatStringSum();

			AssertEquals(true, sum.Match("Sum()"));
			AssertEquals(true, sum.Match("Sum(a)"));
			AssertEquals(true, sum.Match("Sum(a.b.c)"));
			AssertEquals(true, sum.Match("sum(a.b.c)"));
			AssertEquals(true, sum.Match("SUM(someProperty)"));
			AssertEquals(false, sum.Match("Sum(someProperty"));
			AssertEquals(false, sum.Match(" Sum(someProperty)"));
		}

		public override void TestExecute()
		{
			var sum = new FormatStringSum();
			sum.Match("Sum(Property)");

			AssertEquals(ZString.Empty, sum.Execute(null));
			AssertEquals((ZInt)42, sum.Execute(GetListForTest<ZInt>(32, 10)));
			AssertEquals((ZShort)9, sum.Execute(GetListForTest<ZShort>(2, 3, 4)));
			AssertEquals((ZDecimal)12.321, sum.Execute(GetListForTest<ZDecimal>(0.001, 1, 1.1, 2.2, 3.02, 5)));
			AssertEquals((ZLong)32, sum.Execute(GetListForTest<ZLong>(22, 10)));

			sum.Match("Sum(Property.Property)");

			AssertEquals(ZString.Empty, sum.Execute(null));
			AssertEquals((ZInt)42, sum.Execute(GetListForTest2<ZInt>(32, 10)));
			AssertEquals((ZShort)9, sum.Execute(GetListForTest2<ZShort>(2, 3, 4)));
			AssertEquals((ZDecimal)12.321, sum.Execute(GetListForTest2<ZDecimal>(0.001, 1, 1.1, 2.2, 3.02, 5)));
			AssertEquals((ZLong)32, sum.Execute(GetListForTest2<ZLong>(22, 10)));
		}

		List<ObjectForTest<T>> GetListForTest<T>(params T[] values)
		{
			return values.Select(val => new ObjectForTest<T> { Property = val }).ToList();
		}

		List<ObjectForTest<ObjectForTest<T>>> GetListForTest2<T>(params T[] values)
		{
			return values.Select(val => new ObjectForTest<ObjectForTest<T>> { Property = new ObjectForTest<T> { Property = val } }).ToList();
		}
	}
}
