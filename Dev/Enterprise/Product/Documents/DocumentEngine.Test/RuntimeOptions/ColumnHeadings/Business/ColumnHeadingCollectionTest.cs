using System;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(ColumnHeadingCollection))]
	sealed class ColumnHeadingCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestArrayConstructorAndContains()
		{
			AssertEquals("Collecton should have 2 columns", 2, Collection.Count);
			AssertEquals("Collection should contain Column 1", true, Collection.Contains("Column 1"));
			AssertEquals("Collection should contain Column 2", true, Collection.Contains("Column 2"));
		}

		readonly ColumnHeading[] headings = new ColumnHeading[]
		{
			new ColumnHeading("Column 1"),
			new ColumnHeading("Column 2")
		};

		ColumnHeadingCollection Collection
		{
			get
			{
				if (fCollection == null)
				{
					fCollection = new ColumnHeadingCollection(headings);
				}
				return fCollection;
			}
		}
		ColumnHeadingCollection fCollection;

		public void TestToArray()
		{
			ColumnHeading[] array = new ColumnHeadingCollection(headings).ToArray();
			AssertEquals("To array should be same lenght as array collection is based on", headings.Length, array.Length);
			for (int i = 0; i < headings.Length; i++)
			{
				AssertEquals("ToArray sould work", headings[i].DisplayLabel, array[i].DisplayLabel);
			}
		}

		public void TestClone()
		{
			ColumnHeadingCollection newCollection = Collection.Clone();
			foreach (ColumnHeading heading in Collection)
			{
				AssertEquals("New collection contains old collection headings", true, newCollection.Contains(heading.DisplayLabel));
			}
		}

		public void TestCloneReferences()
		{
			ColumnHeadingCollection collectionWithRefernces = Collection.Clone();
			collectionWithRefernces["Column 1"].ReferencedBy.Add(collectionWithRefernces["Column 2"]);
			collectionWithRefernces["Column 1"].ReferencedBy.Add(new ColumnHeading());

			ColumnHeadingCollection newCollection = collectionWithRefernces.Clone();
			foreach (ColumnHeading heading in collectionWithRefernces)
			{
				foreach (ColumnHeading refencingHeading in heading.ReferencedBy)
				{
					AssertEquals("New collection members' references are cloned", true, newCollection[heading.DisplayLabel].ReferencedBy.Contains(refencingHeading.DisplayLabel));
				}
			}
		}

		public void TestCloneVisible()
		{
			Collection["Column 1"].Hidden = true;

			ColumnHeadingCollection newCollection = Collection.CloneVisible();
			AssertEquals("New collection contains column 2", true, newCollection.Contains("Column 2"));
			AssertEquals("New collection does not contain column 1", false, newCollection.Contains("Column 1"));
		}

		public void TestStringIndexing()
		{
			AssertEquals("String indexer initialises correctly from array constructor ", "Column 1", Collection["Column 1"].DisplayLabel);
			AssertEquals("String indexer initialises correctly from array constructor ", "Column 2", Collection["Column 2"].DisplayLabel);

			Collection.Remove(Collection["Column 2"]);
			try
			{
				string name = Collection["Column 2"].DisplayLabel;
				Assert("Should have rased an exception", false);
			}
			catch (Exception ex)
			{
				AssertEquals("Should get out of range exception", "Index was out of range. Must be non-negative and less than the size of the collection.\r\nParameter name: index", ex.Message.Trim());
			}
			AssertEquals("Should not contain column 2", false, Collection.Contains("Column 2"));

			Collection.Add(new ColumnHeading("Column 3"));

			AssertEquals("string index maintained in an add", "Column 3", Collection["Column 3"].DisplayLabel);

			string displayLabel = Collection[0].DisplayLabel;
			Collection.RemoveAt(0);
			AssertEquals("string index maintained during remove at", false, Collection.Contains(displayLabel));
		}
	}
}
