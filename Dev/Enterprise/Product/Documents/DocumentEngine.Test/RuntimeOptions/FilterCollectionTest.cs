using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class FilterCollectionTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			fc = new CollectionOfIFilter();
		}

		CollectionOfIFilter fc;

		public void TestJsonConverter()
		{
			var textField = new TextField(Factory);
			textField.Value = "Test!";
			fc.Add(textField);

			var currentCompanyField = new CurrentCompanyField(Factory);
			currentCompanyField.FieldName = "MyCompanyField";
			currentCompanyField.DisplayName = "Json Test";
			fc.Add(currentCompanyField);

			var result = JsonConverterHelper.Serialize(fc);
			var deserialisedCollection = JsonConverterHelper.Deserialize<CollectionOfIFilter>(result);

			AssertEquals("Collection should have serialized filters", 2, deserialisedCollection.Count);
		}

		public void TestJsonConverterDoesNotConvertReadOnlyFilters()
		{
			var textField = new TextField(Factory);
			textField.Value = "Test!";
			textField.ReadOnly = true;
			fc.Add(textField);

			var result = JsonConverterHelper.Serialize(fc);
			var deserialisedCollection = JsonConverterHelper.Deserialize<CollectionOfIFilter>(result);

			AssertEquals("Collection should not have serialized readonly filter", 0, deserialisedCollection.Count);
		}

		public void TestWhereClauseWithNoFields()
		{
			AssertEquals("Should return empty where clause when no filter fields are present", fc.WhereClause(), "");
		}

		public void TestEnumerableAndLinqMatch()
		{
			var collection = new CollectionOfIFilter();
			collection.AddRange(Enumerable.Range(0, 10).Select(i => new DummyFilter("D" + i)));

			int count = 0;
			foreach (var filter in collection)
			{
				count++;
			}

			AssertEquals("The loop should match the linq count (check your GetEnumerator)", count, collection.OfType<IFilter>().Count());
		}

		public void TestWhereClauseWithOneField()
		{
			fc.Add(new DummyFilter("TRUE"));
			AssertEquals("Should get single child where clause", "(TRUE)", fc.WhereClause());
		}

		public void TestWhereClauseWithTwoFields()
		{
			fc.Add(new DummyFilter("TRUE"));
			fc.Add(new DummyFilter("FALSE"));
			AssertEquals("Should join where clauses together", "(TRUE) AND (FALSE)", fc.WhereClause());
		}

		public void TestDoesNotIncludeEmptyWhereClauses()
		{
			fc.Add(new DummyFilter("X"));
			fc.Add(new DummyFilter(""));
			AssertEquals("Empty where clause parts should be dropped", "(X)", fc.WhereClause());
		}

		public void TestSqlParameters()
		{
			fc.Add(new DummyFilter("parm1"));
			fc.Add(new DummyFilter("parm2"));
			fc.Add(new DummyFilter("parm3"));
			AssertEquals("Should have 3 params", 3, fc.SqlParameters().Count);
			AssertEquals("Param 1 name", "parm1", fc.SqlParameters()[0].ToString());
			AssertEquals("Param 1 value", "parm1", fc.SqlParameters()[0].Value.ToString());
			AssertEquals("Param 2 name", "parm2", fc.SqlParameters()[1].ToString());
			AssertEquals("Param 2 value", "parm2", fc.SqlParameters()[1].Value.ToString());
			AssertEquals("Param 3 name", "parm3", fc.SqlParameters()[2].ToString());
			AssertEquals("Param 3 value", "parm3", fc.SqlParameters()[2].Value.ToString());
		}

		public void TestIndexer()
		{
			DummyFilter a = new DummyFilter("");
			DummyFilter b = new DummyFilter("");
			DummyFilter c = new DummyFilter("");

			fc.Add(a);
			fc.Add(b);
			fc.Add(c);

			AssertEquals("Index 0", a, fc[0]);
			AssertEquals("Index 1", b, fc[1]);
			AssertEquals("Index 2", c, fc[2]);
		}

		public void TestDisplayNameIndexer()
		{
			TextField a = new TextField(new BusinessObjectFactory());
			a.DisplayName = "a";
			TextField b = new TextField(new BusinessObjectFactory());
			b.DisplayName = "b";
			TextField c = new TextField(new BusinessObjectFactory());
			c.DisplayName = "C";

			fc.Add(a);
			fc.Add(b);
			fc.Add(c);

			AssertEquals("Index a", a, fc["a"]);
			AssertEquals("Index B", b, fc["B"]);
			AssertEquals("Index c", c, fc["c"]);

			AssertNull("Index Z", fc["Z"]);
		}

		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestIndexerOutOfBounds()
		{
			object o = fc[69];
		}

		[ExpectException(typeof(FilterFieldDuplicatedException))]
		public void TestDisplayNameIndexerWithDuplicates()
		{
			TextField a = new TextField(new BusinessObjectFactory());
			a.DisplayName = "a";
			TextField b = new TextField(new BusinessObjectFactory());
			b.DisplayName = "a";
			TextField c = new TextField(new BusinessObjectFactory());
			c.DisplayName = "a";

			fc.Add(a);
			fc.Add(b);
			fc.Add(c);

			FilterField dummy = fc["a"];
		}

		public void TestCount()
		{
			AssertEquals(0, fc.Count);
			fc.Add(new DummyFilter(""));
			fc.Add(new DummyFilter(""));
			fc.Add(new DummyFilter(""));
			AssertEquals(3, fc.Count);
		}

		public void TestClear()
		{
			fc.Add(new DummyFilter(""));
			fc.Clear();
			AssertEquals("Clear should empty the collection", 0, fc.Count);
		}

		public void TestClearValues()
		{
			TextField field1 = new TextField(Factory);
			field1.DisplayName = "field";
			field1.Value = "SOMETEXT";
			NumberField field2 = new NumberField(Factory);
			field2.DisplayName = "field2";
			field2.Value = 10;
			fc.AddRange(new List<IFilter>() { field1, field2 });
			fc.ClearValues();
			AssertEquals(ZString.Empty, field1.Value);
			AssertEquals(ZDecimal.Zero, field2.Value);
		}

		public void TestClearValuesWhenIsWeb()
		{
			var pk1 = Guid.NewGuid();

			Globals.IsWeb = true;

			var field1 = new LookupField(Factory);
			field1.DisplayName = "field1";
			field1.Value = pk1;

			var field2 = new NumberField(Factory);
			field2.DisplayName = "field2";
			field2.Value = 10;

			fc.AddRange(new List<IFilter>() { field1, field2 });
			fc.ClearValues(field1);

			AssertEquals(pk1, field1.Value);
			AssertEquals(ZDecimal.Zero, field2.Value);

			Globals.IsWeb = false;
			fc.ClearValues(field1);
			AssertEquals(ZGuid.Empty, field1.Value);
			AssertEquals(ZDecimal.Zero, field2.Value);
		}

		public void TestInsert()
		{
			AssertEquals("Prerequisite: collection should be empty", 0, fc.Count);
			TextField filter = new TextField(Factory);
			fc.Insert(0, filter);
			AssertEquals("Should have added the filter to the collection", 1, fc.Count);
			AssertEquals("Should have redistered the added filter as editable child", true, fc.IsRegisteredEditableChildObject(filter));
		}

		public void TestToDictionary()
		{
			TextField field1 = new TextField(Factory) { DisplayName = "filter1", Value = "SOMETEXT" };
			NumberField field2 = new NumberField(Factory) { DisplayName = "filter2", Value = 10 };
			fc.AddRange(new IFilter[] { field1, field2 });
			Dictionary<string, object> dictionary = fc.ToDictionary();
			AssertEquals(2, dictionary.Count);
			AssertEquals(dictionary["filter1"], "SOMETEXT");
			AssertEquals(dictionary["filter2"], 10);
		}
	}
}
