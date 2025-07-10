using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	sealed class EnumerablesExtensionsTest : TestCase
	{
		class A
		{
			public int i;
		}

		public void TestEqualIgnoringOrder()
		{
			var list1 = new List<A>
					{
						new A { i = 1 },
						null,
						new A { i = 1 },
						new A { i = 2 },
						new A { i = 3 },
						null,
						new A { i = 4 },
						new A { i = 5 },
					};

			var list2 = new List<A>
					{
						new A { i = 1 },
						new A { i = 2 },
						null,
						null,
						new A { i = 3 },
						new A { i = 5 },
						new A { i = 4 },
						new A { i = 1 },
					};

			Assert(!((IList<A>)null).EqualIgnoringOrder(null));
			Assert(!((IList<A>)null).EqualIgnoringOrder(list2));
			Assert(!list1.EqualIgnoringOrder(null));

			Assert(!list1.EqualIgnoringOrder(list2));
			Assert(list1.EqualIgnoringOrder(list2, new LambdaComparer<A>((x, y) => x.i.Equals(y.i), x => x.i.GetHashCode())));

			list1.RemoveAt(0);
			Assert(!list1.EqualIgnoringOrder(list2, new LambdaComparer<A>((x, y) => x.i.Equals(y.i), x => x.i.GetHashCode())));

			list2.RemoveAt(7);
			Assert(list1.EqualIgnoringOrder(list2, new LambdaComparer<A>((x, y) => x.i.Equals(y.i), x => x.i.GetHashCode())));
			Assert(!list1.EqualIgnoringOrder(list2, new LambdaComparer<A>((x, y) => x.i.Equals(y.i), x => x.i.GetHashCode()), false));

			var list3 = new List<int> { 4, 5, 6, 2, 3, 2, 2 };
			var list4 = new List<int> { 2, 4, 5, 6, 2, 3, 2 };

			Assert(list3.EqualIgnoringOrder(list4));

			list4.Add(2);

			Assert(!list3.EqualIgnoringOrder(list4));
		}

		public void TestLambdaNotNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new LambdaComparer<A>((x, y) => x.Equals(y), null));
			AssertExceptionThrown(typeof(ArgumentNullException), () => new LambdaComparer<A>(null, x => x.GetHashCode()));
		}
	}
}
