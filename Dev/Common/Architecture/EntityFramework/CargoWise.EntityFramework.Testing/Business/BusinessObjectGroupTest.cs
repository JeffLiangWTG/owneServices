using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectGroupTest : TestCaseWithFactory
	{
		public void TestGroupByColumn()
		{
			var dummies = PrepareTestData();
			var groups = BusinessObjectGroup<DummyBusinessObject>.GroupByColumn(DummyBizoSchema.Z0_Number, dummies).OrderBy(group => group.GroupKey).ToArray();

			AssertEquals(3, groups.Length);

			AssertEquals(1, groups[0].GroupKey);
			AssertEquals(4, groups[0].Elements.Count());
			AssertEquals(1, groups[0].Elements.ElementAt(0).Z0_AnotherNumber);
			AssertEquals(4, groups[0].Elements.ElementAt(1).Z0_AnotherNumber);
			AssertEquals(6, groups[0].Elements.ElementAt(2).Z0_AnotherNumber);
			AssertEquals(8, groups[0].Elements.ElementAt(3).Z0_AnotherNumber);

			AssertEquals(2, groups[1].GroupKey);
			AssertEquals(3, groups[1].Elements.Count());
			AssertEquals(2, groups[1].Elements.ElementAt(0).Z0_AnotherNumber);
			AssertEquals(5, groups[1].Elements.ElementAt(1).Z0_AnotherNumber);
			AssertEquals(7, groups[1].Elements.ElementAt(2).Z0_AnotherNumber);

			AssertEquals(3, groups[2].GroupKey);
			AssertEquals(1, groups[2].Elements.Count());
			AssertEquals(3, groups[2].Elements.ElementAt(0).Z0_AnotherNumber);
		}

		public void TestGroupByColumnAndJoinAllValues()
		{
			var dummies = PrepareTestData();
			var groups = BusinessObjectGroup<DummyBusinessObject>.GroupByColumnAndJoinAllValues(
				DummyBizoSchema.Z0_Number,
				new SchemaColumn[]
				{
					DummyBizoSchema.Z0_Code,
					DummyBizoSchema.Z0_Description,
					DummyBizoSchema.Z0_Bool
				},
				dummies)
				.OrderBy(
					group =>
					{
						var key = (IList)group.GroupKey;
						if (key.Count == 1)
						{
							return (ZInt)key[0];
						}
						return (ZInt)100;
					})
				.ToArray();

			AssertEquals(4, groups.Length); // (1):(6,8), (2):(7), (3):(3), (1,2):(1,4)

			AssertEquals(1, ((IList)groups[0].GroupKey).Count);
			AssertEquals(1, ((IList)groups[0].GroupKey)[0]);
			AssertEquals(2, groups[0].Elements.Count());
			AssertEquals(6, groups[0].Elements.ElementAt(0).Z0_AnotherNumber);
			AssertEquals(8, groups[0].Elements.ElementAt(1).Z0_AnotherNumber);

			AssertEquals(1, ((IList)groups[1].GroupKey).Count);
			AssertEquals(2, ((IList)groups[1].GroupKey)[0]);
			AssertEquals(1, groups[1].Elements.Count());
			AssertEquals(7, groups[1].Elements.ElementAt(0).Z0_AnotherNumber);

			AssertEquals(1, ((IList)groups[2].GroupKey).Count);
			AssertEquals(3, ((IList)groups[2].GroupKey)[0]);
			AssertEquals(1, groups[2].Elements.Count());
			AssertEquals(3, groups[2].Elements.ElementAt(0).Z0_AnotherNumber);

			AssertEquals(2, ((IList)groups[3].GroupKey).Count);
			AssertEquals(1, ((IList)groups[3].GroupKey)[0]);
			AssertEquals(2, ((IList)groups[3].GroupKey)[1]);
			AssertEquals(2, groups[3].Elements.Count());
			AssertEquals(1, groups[3].Elements.ElementAt(0).Z0_AnotherNumber);
			AssertEquals(4, groups[3].Elements.ElementAt(1).Z0_AnotherNumber);
		}

		#region Implementation

		IEnumerable<DummyBusinessObject> PrepareTestData()
		{
			yield return CreateDummy("AAA", "Aaaa", true, 1, 1); // Group (1,2)
			yield return CreateDummy("AAA", "Aaaa", true, 2, 2); // Group (1,2), ignored as duplicate
			yield return CreateDummy("AAA", "Aaaa", false, 3, 3); // Group (3)
			yield return CreateDummy("BBB", "Bbbb", true, 1, 4); // Group (1,2)
			yield return CreateDummy("BBB", "Bbbb", true, 2, 5); // Group (1,2), ignored as duplicate
			yield return CreateDummy("CCC", "Cccc", true, 1, 6); // Group (1)
			yield return CreateDummy("CCC", "Xxxx", false, 2, 7); // Group (2)
			yield return CreateDummy("DDD", "Dddd", true, 1, 8); // Group (1)
		}

		DummyBusinessObject CreateDummy(ZString code, ZString description, ZBool flag, ZInt number, ZInt anotherNumber)
		{
			var dummy = Factory.New<DummyBusinessObject>();

			dummy.Z0_Code = code;
			dummy.Z0_Description = description;
			dummy.Z0_Bool = flag;
			dummy.Z0_Number = number;
			dummy.Z0_AnotherNumber = anotherNumber;

			return dummy;
		}

		#endregion
	}
}
