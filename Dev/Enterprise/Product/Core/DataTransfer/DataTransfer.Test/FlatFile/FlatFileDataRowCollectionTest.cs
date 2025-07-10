using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	public class FlatFileDataRowCollectionTest : TestCase
	{
		public void TestToArray()
		{
			FlatFileDataRowCollection test = new FlatFileDataRowCollection();
			test.InsertBlankLine();
			test.InsertBlankLine();
			test.InsertBlankLine();

			FlatFileDataRow[] result = test.ToArray();

			foreach (FlatFileDataRow row in result)
			{
				AssertNotNull(row);
				AssertEquals(typeof(FlatFileDataRow), row.GetType());
			}

			AssertEquals(3, result.Length);
		}

		public void TestInsertEmptyRow()
		{
			FlatFileDataRow row = new FlatFileDataRow(0);
			FlatFileDataRowCollection collection = new FlatFileDataRowCollection();
			collection.Insert(0, row);
			AssertEquals("Empty Row was allowed to be part of collection", 0, collection.Count);
		}

		public void TestInsertRow()
		{
			FlatFileDataRow row1 = new FlatFileDataRow(1);
			row1[0] = "hunters";

			FlatFileDataRow row2 = new FlatFileDataRow(1);
			row2[0] = "hair";

			FlatFileDataRow row3 = new FlatFileDataRow(1);
			row3[0] = "is";

			FlatFileDataRow row4 = new FlatFileDataRow(1);
			row4[0] = "gone";

			FlatFileDataRowCollection collection = new FlatFileDataRowCollection();

			collection.Add(row1);
			collection.Add(row2);
			collection.Add(row4);

			collection.Insert(2, row3);

			AssertEquals("is", collection[2][0]);
		}

		public void TestAdd_Collection_Empty()
		{
			FlatFileDataRowCollection newCollection = new FlatFileDataRowCollection();
			AssertEquals("Collection should have no elements", 0, newCollection.Count);
			Collection.Add(newCollection);
			AssertEquals("Collection should have no elements", 0, Collection.Count);
		}

		public void TestAdd_Collection_NotEmpty()
		{
			FlatFileDataRowCollection newCollection = new FlatFileDataRowCollection();
			FlatFileDataRow row = new FlatFileDataRow(1);
			row[0] = "haha";
			newCollection.Add(row);

			newCollection.InsertBlankLine();

			row = new FlatFileDataRow(1);
			row[0] = "haha";
			newCollection.Add(row);

			AssertEquals("Collection should have 3 element", 3, newCollection.Count);

			Collection.Add(newCollection);
			AssertEquals("Collection should have 3 elements", 3, Collection.Count);

			Collection.Add(newCollection);
			AssertEquals("Collection should have 6 elements", 6, Collection.Count);
		}

		public void TestAdd_NotEmpty()
		{
			FlatFileDataRow row = new FlatFileDataRow(1);
			row.SetField(0, "asl");

			Collection.Add(row);
			AssertEquals("Item was not added to base Collection", 1, Collection.Count);
		}

		public void TestAdd_Empty()
		{
			Collection.Add(new FlatFileDataRow(0));
			AssertEquals("Item was not added to base Collection", 0, Collection.Count);
		}

		public void TestIndexer()
		{
			FlatFileDataRow row = new FlatFileDataRow(1);
			row.SetField(0, "row");
			Collection.Add(row);
			AssertEquals("Incorrect object type returned from indexer", typeof(FlatFileDataRow), Collection[0].GetType());
		}

		public void TestInsertBlankLine()
		{
			Collection.InsertBlankLine();
			AssertEquals(1, Collection.Count);
			AssertEquals(0, Collection[0].FieldCount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Collection = new FlatFileDataRowCollection();
		}

		FlatFileDataRowCollection Collection;
	}
}
