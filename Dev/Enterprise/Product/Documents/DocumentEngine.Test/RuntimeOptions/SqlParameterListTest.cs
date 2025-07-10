using System.Collections;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class SqlParameterListTest : TestCase
	{
		public SqlParameterListTest()
			: base()
		{
		}

		SqlParameter GetNewSqlParameter()
		{
			return new SqlParameter();
		}

		[ExpectNoExceptions()]
		public void TestEmptyConstructor()
		{
			SqlParameterList testSqlParameterList = new SqlParameterList();
		}

		public void TestConstructorWithSqlParameterList()
		{
			SqlParameterList testSqlParameterList1 = new SqlParameterList();
			testSqlParameterList1.Add(GetNewSqlParameter());
			testSqlParameterList1.Add(GetNewSqlParameter());
			testSqlParameterList1.Add(GetNewSqlParameter());

			SqlParameterList testSqlParameterList2 = new SqlParameterList(testSqlParameterList1);
			AssertEquals(3, testSqlParameterList2.Count);
		}

		public void TestConstructorWithArray()
		{
			SqlParameterList testSqlParameterList = new SqlParameterList(new SqlParameter[] { GetNewSqlParameter(), GetNewSqlParameter() });
			AssertEquals(2, testSqlParameterList.Count);
		}

		public void TestIndexerGet()
		{
			SqlParameterList testSqlParameterList = new SqlParameterList();
			testSqlParameterList.Add(GetNewSqlParameter());
			AssertNotNull(testSqlParameterList[0]);
		}

		public void TestIndexerSet()
		{
			SqlParameterList testSqlParameterList = new SqlParameterList();
			SqlParameter testSqlParameter = GetNewSqlParameter();
			testSqlParameterList.Add(GetNewSqlParameter());
			testSqlParameterList[0] = testSqlParameter;
			AssertSame(testSqlParameter, testSqlParameterList[0]);
		}

		public void TestAdd()
		{
			SqlParameterList testSqlParameterList = new SqlParameterList();
			AssertEquals(0, testSqlParameterList.Count);
			testSqlParameterList.Add(GetNewSqlParameter());
			testSqlParameterList.Add(GetNewSqlParameter());
			testSqlParameterList.Add(GetNewSqlParameter());
			AssertEquals(3, testSqlParameterList.Count);
		}

		public void TestAddRangeSqlParameterList()
		{
			SqlParameterList testSqlParameterList1 = new SqlParameterList();
			testSqlParameterList1.Add(GetNewSqlParameter());
			testSqlParameterList1.Add(GetNewSqlParameter());
			testSqlParameterList1.Add(GetNewSqlParameter());

			SqlParameterList testSqlParameterList = new SqlParameterList();
			AssertEquals(0, testSqlParameterList.Count);
			testSqlParameterList.AddRange(testSqlParameterList1);
			AssertEquals(3, testSqlParameterList.Count);
		}

		public void TestAddRangeArray()
		{
			SqlParameterList testSqlParameterList = new SqlParameterList();
			AssertEquals(0, testSqlParameterList.Count);
			testSqlParameterList.AddRange(new SqlParameter[] { GetNewSqlParameter(), GetNewSqlParameter(), GetNewSqlParameter(), GetNewSqlParameter() });
			AssertEquals(4, testSqlParameterList.Count);
		}

		public void TestContaines()
		{
			SqlParameterList testSqlParameterList = new SqlParameterList();
			SqlParameter testSqlParameter = GetNewSqlParameter();
			testSqlParameterList.Add(testSqlParameter);
			Assert(testSqlParameterList.Contains(testSqlParameter));
		}

		public void TestCopyTo()
		{
			SqlParameterList testSqlParameterList = new SqlParameterList(new SqlParameter[] { GetNewSqlParameter(), GetNewSqlParameter() });
			SqlParameter[] sqlParameterList = new SqlParameter[3] { null, null, null };
			testSqlParameterList.CopyTo(sqlParameterList, 1);
			AssertNull(sqlParameterList[0]);
			AssertNotNull(sqlParameterList[1]);
			AssertNotNull(sqlParameterList[2]);
		}

		public void TestToArray()
		{
			SqlParameterList testSqlParameterList = new SqlParameterList(new SqlParameter[] { GetNewSqlParameter(), GetNewSqlParameter() });
			SqlParameter[] sqlParameterList = testSqlParameterList.ToArray();
			AssertEquals(2, sqlParameterList.Length);
		}

		public void TestIndexOf()
		{
			SqlParameterList testSqlParameterList = new SqlParameterList();
			testSqlParameterList.Add(GetNewSqlParameter());
			testSqlParameterList.Add(GetNewSqlParameter());
			SqlParameter testSqlParameter = GetNewSqlParameter();
			testSqlParameterList.Add(testSqlParameter);
			AssertEquals(2, testSqlParameterList.IndexOf(testSqlParameter));
		}

		public void TestInsert()
		{
			SqlParameterList testSqlParameterList = new SqlParameterList();
			testSqlParameterList.Add(GetNewSqlParameter());
			testSqlParameterList.Add(GetNewSqlParameter());
			SqlParameter testSqlParameter = GetNewSqlParameter();
			testSqlParameterList.Insert(1, testSqlParameter);
			AssertEquals(1, testSqlParameterList.IndexOf(testSqlParameter));
		}

		public void TestRemove()
		{
			SqlParameterList testSqlParameterList = new SqlParameterList();
			SqlParameter testSqlParameter = GetNewSqlParameter();
			testSqlParameterList.Add(testSqlParameter);
			Assert(testSqlParameterList.Contains(testSqlParameter));
			testSqlParameterList.Remove(testSqlParameter);
			Assert(!testSqlParameterList.Contains(testSqlParameter));
		}

		public void TestCount()
		{
			SqlParameterList testSqlParameterList = new SqlParameterList();
			AssertEquals(0, testSqlParameterList.Count);
			testSqlParameterList.Add(GetNewSqlParameter());
			testSqlParameterList.Add(GetNewSqlParameter());
			testSqlParameterList.Add(GetNewSqlParameter());
			AssertEquals(3, testSqlParameterList.Count);
		}

		public void TestGetEnumerator()
		{
			SqlParameterList testSqlParameterList = new SqlParameterList(new SqlParameter[] { GetNewSqlParameter(), GetNewSqlParameter() });
			AssertNotNull(testSqlParameterList.GetEnumerator());
		}

		public void TestCurrent()
		{
			SqlParameterList testSqlParameterList = new SqlParameterList(new SqlParameter[] { GetNewSqlParameter(), GetNewSqlParameter() });
			IEnumerator @enum = testSqlParameterList.GetEnumerator();
			@enum.MoveNext();
			AssertNotNull(@enum.Current);
		}

		public void TestMoveNext()
		{
			SqlParameterList testSqlParameterList = new SqlParameterList(new SqlParameter[] { GetNewSqlParameter(), GetNewSqlParameter() });
			IEnumerator @enum = testSqlParameterList.GetEnumerator();
			Assert(@enum.MoveNext());
			Assert(@enum.MoveNext());
			Assert(!@enum.MoveNext());
		}

		public void TestReset()
		{
			SqlParameterList testSqlParameterList = new SqlParameterList(new SqlParameter[] { GetNewSqlParameter(), GetNewSqlParameter() });
			IEnumerator @enum = testSqlParameterList.GetEnumerator();
			Assert(@enum.MoveNext());
			Assert(@enum.MoveNext());
			Assert(!@enum.MoveNext());
			@enum.Reset();
			Assert(@enum.MoveNext());
		}
	}
}
