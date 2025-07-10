using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZSqlParameterCollectionTest : TestCase
	{
		public void TestConstructorWithZSqlParameterCollection()
		{
			ZSqlParameterCollection collection1 = new ZSqlParameterCollection();
			collection1.Add(GetNewSqlParameter("@test"));
			collection1.Add(GetNewSqlParameter("@test2"));
			collection1.Add(GetNewSqlParameter("@test3"));

			ZSqlParameterCollection collection2 = new ZSqlParameterCollection(collection1);
			AssertEquals(3, collection2.Count);
		}

		public void TestIndexerGet()
		{
			collection = new ZSqlParameterCollection();
			collection.Add(ZSqlParameter.New("@foo", new ZString("hat"), DummyBizoSchema.Z0_Description));
			AssertNotNull(collection["@foo"]);
		}

		public void TestAddFieldNameValue()
		{
			collection = new ZSqlParameterCollection();
			AssertEquals(0, collection.Count);

			collection.Add(ZSqlParameter.New("@foo", "hat", DummyBizoSchema.Z0_Description));
			collection.Add(ZSqlParameter.New("@foobar", new ZString("hat"), DummyBizoSchema.Z0_Description));
			AssertEquals(2, collection.Count);
		}

		[ExpectException(typeof(DuplicateParameterArgumentException))]
		public void TestAddDuplicateNamedParameter()
		{
			ZSqlParameterCollection collection = new ZSqlParameterCollection();
			collection.Add("@test", "testval", DummyBizoSchema.Z0_Description);
			collection.Add("@test", "testval2", DummyBizoSchema.Z0_Description);
		}

		[ExpectException(typeof(DuplicateParameterArgumentException))]
		public void TestAddDuplicateParameterRange()
		{
			collection = new ZSqlParameterCollection();
			collection.Add("@test", "testval", DummyBizoSchema.Z0_Description);
			collection.Add("@test", "testval2", DummyBizoSchema.Z0_Description);

			collection.AddRange(collection);
		}

		public void TestRemoveFieldNameValue()
		{
			collection = new ZSqlParameterCollection();
			AssertEquals(0, collection.Count);

			collection.Add(ZSqlParameter.New("@foo", "hat", DummyBizoSchema.Z0_Description));
			collection.Add(ZSqlParameter.New("@foobar", new ZString("hat"), DummyBizoSchema.Z0_Description));
			collection.Remove("@foo");
			AssertEquals(1, collection.Count);
		}

		public void TestAddZSqlParameter()
		{
			collection = new ZSqlParameterCollection();
			AssertEquals(0, collection.Count);

			collection.Add(GetNewSqlParameter("@test"));
			collection.Add(GetNewSqlParameter("@test2"));
			collection.Add(GetNewSqlParameter("@test3"));
			AssertEquals(3, collection.Count);
		}

		public void TestAddRangeZSqlParameterCollection()
		{
			ZSqlParameterCollection collection1 = new ZSqlParameterCollection();
			collection1.Add(GetNewSqlParameter("@test"));
			collection1.Add(GetNewSqlParameter("@test2"));
			collection1.Add(GetNewSqlParameter("@test3"));

			ZSqlParameterCollection collection2 = new ZSqlParameterCollection();
			AssertEquals(0, collection2.Count);

			collection2.AddRange(collection1);
			AssertEquals(3, collection2.Count);
		}

		public void TestAddRangeArray()
		{
			collection = new ZSqlParameterCollection();
			AssertEquals(0, collection.Count);

			collection.AddRange(new ZSqlParameter[] { GetNewSqlParameter("@test"), GetNewSqlParameter("@test2"), GetNewSqlParameter("@test3"), GetNewSqlParameter("@test4") });
			AssertEquals(4, collection.Count);
		}

		public void TestContaines()
		{
			collection = new ZSqlParameterCollection();
			ZSqlParameter param = GetNewSqlParameter("@test");
			collection.Add(param);
			Assert(collection.Contains(param));
		}

		public void TestCopyTo()
		{
			ZSqlParameter[] array = new ZSqlParameter[] { null, null, null };
			collection.CopyTo(array, 1);
			AssertNull(array[0]);
			AssertNotNull(array[1]);
			AssertNotNull(array[2]);
		}

		public void TestToArray()
		{
			ZSqlParameter[] array = collection.ToArray();
			AssertEquals(2, array.Length);
		}

		public void TestRemove()
		{
			collection = new ZSqlParameterCollection();
			ZSqlParameter param = GetNewSqlParameter("@test");

			collection.Add(param);
			Assert(collection.Contains(param));

			collection.Remove(param);
			Assert(!collection.Contains(param));
		}

		public void TestCount()
		{
			collection = new ZSqlParameterCollection();
			AssertEquals(0, collection.Count);

			collection.Add(GetNewSqlParameter("@test"));
			collection.Add(GetNewSqlParameter("@test2"));
			collection.Add(GetNewSqlParameter("@test3"));
			AssertEquals(3, collection.Count);
		}

		public void TestGetEnumerator()
		{
			int i = 0;
			foreach (ZSqlParameter param in collection)
			{
				AssertEquals(collection[i++], param);
			}

			AssertEquals(2, i);
		}

		// Temporarily disabled due to memory leaks with cached ZQueries (see W00042289). Brett will look at this later and decide what to do with it.
		//public void TestParent()
		//{
		//    ZQuery filter = new ZQuery();
		//    filter.DefaultJoinCondition = JoinCondition.And;
		//    ZSqlParameterCollection parameters = new ZSqlParameterCollection();
		//    ZSqlParameter parameter = ZSqlParameter.New("@MyParam", "Code", DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal);
		//    parameters.Add(parameter);
		//    AssertEquals(parameters, parameter.Parents[0]);
		//}

		ZSqlParameter GetNewSqlParameter(string name)
		{
			return ZSqlParameter.New(name, new ZString("pot"), DummyBizoSchema.Z0_Description);
		}

		protected override void SetUp()
		{
			base.SetUp();
			collection = new ZSqlParameterCollection(new ZSqlParameter[] { GetNewSqlParameter("@test"), GetNewSqlParameter("@test2") });
			AssertEquals(2, collection.Count);
		}

		ZSqlParameterCollection collection;
	}
}
