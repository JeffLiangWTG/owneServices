using System.Collections;
using System.Data;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZDataRowDictionaryTest : TestCase
	{
		public void TestAdd()
		{
			AssertEquals("Precondition - the dictionary should be empty.", 0, Dictionary.Count);

			DataRow row = new DataTable().NewRow();
			Dictionary.Add("row", row);

			AssertEquals(1, Dictionary.Count);
			AssertEquals(row, Dictionary["row"]);
		}

		public void TestIndexer()
		{
			DataRow row1 = new DataTable().NewRow();
			DataRow row2 = new DataTable().NewRow();

			Dictionary.Add("row1", row1);
			Dictionary.Add("row2", row2);

			AssertEquals(row1, Dictionary["row1"]);
			AssertEquals(row2, Dictionary["row2"]);
		}

		public void TestEnumerating()
		{
			DataRow row1 = new DataTable().NewRow();
			DataRow row2 = new DataTable().NewRow();

			Dictionary.Add("row1", row1);
			Dictionary.Add("row2", row2);

			bool row1Found = false;
			bool row2Found = false;
			int iterations = 0;

			foreach (DataRow row in Dictionary.Values)
			{
				if (row == row1)
				{
					row1Found = true;
				}
				else if (row == row2)
				{
					row2Found = true;
				}

				iterations++;
			}

			AssertEquals("Should have found Row 1.", true, row1Found);
			AssertEquals("Should have found Row 2.", true, row2Found);
			AssertEquals("Should have found 2 rows while enumerating.", 2, iterations);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Dictionary = new ZDataRowDictionary();
		}

		ZDataRowDictionary Dictionary;

		#endregion
	}

	sealed class ZDataRowCollectionTest : TestCase
	{
		DataRow GetNewDataRow()
		{
			return new DataTable().NewRow();
		}

		[ExpectNoExceptions()]
		public void TestEmptyConstructor()
		{
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection();
		}

		public void TestConstructorWithZDataRowCollection()
		{
			ZDataRowCollection testZDataRowCollection1 = new ZDataRowCollection();
			testZDataRowCollection1.Add(GetNewDataRow());
			testZDataRowCollection1.Add(GetNewDataRow());
			testZDataRowCollection1.Add(GetNewDataRow());

			ZDataRowCollection testZDataRowCollection2 = new ZDataRowCollection(testZDataRowCollection1);
			AssertEquals(3, testZDataRowCollection2.Count);
		}

		public void TestConstructorWithArray()
		{
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection(new DataRow[] { GetNewDataRow(), GetNewDataRow() });
			AssertEquals(2, testZDataRowCollection.Count);
		}

		public void TestConstructorWithDataRowCollection()
		{
			DataTable table = new DataTable();
			table.Rows.Add(table.NewRow());
			table.Rows.Add(table.NewRow());
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection(table.Rows);
			AssertEquals(2, testZDataRowCollection.Count);
		}

		public void TestIndexerGet()
		{
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection();
			testZDataRowCollection.Add(GetNewDataRow());
			AssertNotNull(testZDataRowCollection[0]);
		}

		public void TestIndexerSet()
		{
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection();
			DataRow testDataRow = GetNewDataRow();
			testZDataRowCollection.Add(GetNewDataRow());
			testZDataRowCollection[0] = testDataRow;
			AssertSame(testDataRow, testZDataRowCollection[0]);
		}

		public void TestAdd()
		{
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection();
			AssertEquals(0, testZDataRowCollection.Count);
			testZDataRowCollection.Add(GetNewDataRow());
			testZDataRowCollection.Add(GetNewDataRow());
			testZDataRowCollection.Add(GetNewDataRow());
			AssertEquals(3, testZDataRowCollection.Count);
		}

		public void TestAddRangeZDataRowCollection()
		{
			ZDataRowCollection testZDataRowCollection1 = new ZDataRowCollection();
			testZDataRowCollection1.Add(GetNewDataRow());
			testZDataRowCollection1.Add(GetNewDataRow());
			testZDataRowCollection1.Add(GetNewDataRow());

			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection();
			AssertEquals(0, testZDataRowCollection.Count);
			testZDataRowCollection.AddRange(testZDataRowCollection1);
			AssertEquals(3, testZDataRowCollection.Count);
		}

		public void TestAddRangeArray()
		{
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection();
			AssertEquals(0, testZDataRowCollection.Count);
			testZDataRowCollection.AddRange(new DataRow[] { GetNewDataRow(), GetNewDataRow(), GetNewDataRow(), GetNewDataRow() });
			AssertEquals(4, testZDataRowCollection.Count);
		}

		public void TestAddRangeDataRowCollection()
		{
			DataTable table = new DataTable();
			table.Rows.Add(table.NewRow());
			table.Rows.Add(table.NewRow());
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection();
			testZDataRowCollection.AddRange(table.Rows);
			AssertEquals(2, testZDataRowCollection.Count);
		}

		public void TestContaines()
		{
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection();
			DataRow testDataRow = GetNewDataRow();
			testZDataRowCollection.Add(testDataRow);
			Assert(testZDataRowCollection.Contains(testDataRow));
		}

		public void TestCopyTo()
		{
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection(new DataRow[] { GetNewDataRow(), GetNewDataRow() });
			DataRow[] zDataRowCollection = new DataRow[3] { null, null, null };
			testZDataRowCollection.CopyTo(zDataRowCollection, 1);
			AssertNull(zDataRowCollection[0]);
			AssertNotNull(zDataRowCollection[1]);
			AssertNotNull(zDataRowCollection[2]);
		}

		public void TestToArray()
		{
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection(new DataRow[] { GetNewDataRow(), GetNewDataRow() });
			DataRow[] zDataRowCollection = testZDataRowCollection.ToArray();
			AssertEquals(2, zDataRowCollection.Length);
		}

		public void TestIndexOf()
		{
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection();
			testZDataRowCollection.Add(GetNewDataRow());
			testZDataRowCollection.Add(GetNewDataRow());
			DataRow testDataRow = GetNewDataRow();
			testZDataRowCollection.Add(testDataRow);
			AssertEquals(2, testZDataRowCollection.IndexOf(testDataRow));
		}

		public void TestInsert()
		{
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection();
			testZDataRowCollection.Add(GetNewDataRow());
			testZDataRowCollection.Add(GetNewDataRow());
			DataRow testDataRow = GetNewDataRow();
			testZDataRowCollection.Insert(1, testDataRow);
			AssertEquals(1, testZDataRowCollection.IndexOf(testDataRow));
		}

		public void TestRemove()
		{
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection();
			DataRow testDataRow = GetNewDataRow();
			testZDataRowCollection.Add(testDataRow);
			Assert(testZDataRowCollection.Contains(testDataRow));
			testZDataRowCollection.Remove(testDataRow);
			Assert(!testZDataRowCollection.Contains(testDataRow));
		}

		public void TestCount()
		{
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection();
			AssertEquals(0, testZDataRowCollection.Count);
			testZDataRowCollection.Add(GetNewDataRow());
			testZDataRowCollection.Add(GetNewDataRow());
			testZDataRowCollection.Add(GetNewDataRow());
			AssertEquals(3, testZDataRowCollection.Count);
		}

		public void TestGetEnumerator()
		{
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection(new DataRow[] { GetNewDataRow(), GetNewDataRow() });
			AssertNotNull(testZDataRowCollection.GetEnumerator());
		}

		public void TestCurrent()
		{
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection(new DataRow[] { GetNewDataRow(), GetNewDataRow() });
			IEnumerator @enum = testZDataRowCollection.GetEnumerator();
			@enum.MoveNext();
			AssertNotNull(@enum.Current);
		}

		public void TestMoveNext()
		{
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection(new DataRow[] { GetNewDataRow(), GetNewDataRow() });
			IEnumerator @enum = testZDataRowCollection.GetEnumerator();
			Assert(@enum.MoveNext());
			Assert(@enum.MoveNext());
			Assert(!@enum.MoveNext());
		}

		public void TestReset()
		{
			ZDataRowCollection testZDataRowCollection = new ZDataRowCollection(new DataRow[] { GetNewDataRow(), GetNewDataRow() });
			IEnumerator @enum = testZDataRowCollection.GetEnumerator();
			Assert(@enum.MoveNext());
			Assert(@enum.MoveNext());
			Assert(!@enum.MoveNext());
			@enum.Reset();
			Assert(@enum.MoveNext());
		}
	}
}
