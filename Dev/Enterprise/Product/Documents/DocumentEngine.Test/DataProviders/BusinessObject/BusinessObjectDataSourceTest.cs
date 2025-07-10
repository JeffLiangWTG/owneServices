using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class BusinessObjectDataSourceTest : TestCaseWithFactory
	{
		public void TestGetRowsFromIndexs()
		{
			var collection = new DocumentWrapperCollectionForTesting();
			collection.Add(new DocumentWrapperForTesting("Row 1"));
			collection.Add(new DocumentWrapperForTesting("Row 2"));
			collection.Add(new DocumentWrapperForTesting("Row 3"));
			collection.Add(new DocumentWrapperForTesting("Row 4"));
			var children = new BusinessObjectDataSource("Children", collection);
			AssertEquals(4, children.RowCount);
			var newSource = children.GetRowsFromIndexes(new int[3] { 1, 2, 3 });
			AssertEquals(3, newSource.RowCount);
		}

		public void TestGroupBy2ColumnsSortsNumerically()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{A}-[#GroupBy:Collection.Number+Collection.Text]
{B}-[<Collection.Number>, <Collection.Text>]
{A}-[#EndOfReport]", "UnitTest");

			var dummy = Factory.New<DummyDocumentSupportable>();

			var child1 = dummy.Collection.AddNew();
			child1.Z0_Number = 18;
			child1.Z0_VarCharMax = "D";

			var child2 = dummy.Collection.AddNew();
			child2.Z0_Number = 2;
			child2.Z0_VarCharMax = "C";

			var child3 = dummy.Collection.AddNew();
			child3.Z0_Number = 18;
			child3.Z0_VarCharMax = "B";

			var child4 = dummy.Collection.AddNew();
			child4.Z0_Number = 2;
			child4.Z0_VarCharMax = "A";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverDocument(documentCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var workSheet = excelInterface.WorkSheets.First();

				AssertMultilineASCIIEquals("The values should be sorted.",
@"{B}-[2, A]
{B}-[2, C]
{B}-[18, B]
{B}-[18, D]", workSheet.ToString());
			}
		}

		public void TestRowCount()
		{
			DocumentWrapperCollectionForTesting collection = new DocumentWrapperCollectionForTesting();
			collection.Add(new DocumentWrapperForTesting("Row 1"));
			collection.Add(new DocumentWrapperForTesting("Row 2"));
			collection.Add(new DocumentWrapperForTesting("Row 3"));
			collection.Add(new DocumentWrapperForTesting("Row 4"));
			IDataRowSource children = new BusinessObjectDataSource("Children", collection);
			AssertEquals(4, children.RowCount);
		}

		public void TestGroupBy()
		{
			var docWrapper = new DocumentWrapperForTesting("Top Level");
			DocumentWrapperCollectionForTesting collection = docWrapper.Children;
			collection.RemoveAll();

			for (int i = 0; i < 5; i++)
			{
				collection.Add(new DocumentWrapperForTesting("Group 4"));
			}
			for (int i = 0; i < 7; i++)
			{
				collection.Add(new DocumentWrapperForTesting("Group 3"));
			}
			for (int i = 0; i < 2; i++)
			{
				collection.Add(new DocumentWrapperForTesting("Group 1"));
			}
			for (int i = 0; i < 6; i++)
			{
				collection.Add(new DocumentWrapperForTesting("Group 2"));
			}

			IDataRowSource dS = new BusinessObjectDataSource("Children", collection);
			IDataRowSource[] groups = dS.GroupBy(new string[] { "TestString" });

			BusinessObjectDataProvider provider = new BusinessObjectDataProvider(new DataProviderList(docWrapper), null);
			AssertEquals(4, groups.Length);
			AssertEquals(2, groups[0].RowCount);
			AssertEquals("Group 1", provider.GetColumnValue(groups[0], 0, "Children.TestString"));
			AssertEquals(6, groups[1].RowCount);
			AssertEquals("Group 2", provider.GetColumnValue(groups[1], 0, "Children.TestString"));
			AssertEquals(7, groups[2].RowCount);
			AssertEquals("Group 3", provider.GetColumnValue(groups[2], 0, "Children.TestString"));
			AssertEquals(5, groups[3].RowCount);
			AssertEquals("Group 4", provider.GetColumnValue(groups[3], 0, "Children.TestString"));
		}

		public void TestGroupByKeepsSortOrder()
		{
			var docWrapper = new DocumentWrapperForTesting("Top Level");
			DocumentWrapperCollectionForTesting collection = docWrapper.Children;
			collection.RemoveAll();

			for (int i = 0; i < 100; i++)
			{
				collection.Add(new DocumentWrapperForTesting("String " + i, i / 10));
			}

			IDataRowSource dS = new BusinessObjectDataSource("Children", collection);
			IDataRowSource[] groups = dS.GroupBy(new string[] { "TestInt" });

			BusinessObjectDataProvider provider = new BusinessObjectDataProvider(new DataProviderList(docWrapper), null);
			AssertEquals(10, groups.Length);
			AssertEquals(10, groups[0].RowCount);
			for (int i = 0; i < 10; i++)
			{
				AssertEquals("String " + i, provider.GetColumnValue(groups[0], i, "Children.TestString"));
			}
		}

		public void TestGroupByNotUsingDefaultSortWhenApplyingCustomSorting()
		{
			var docWrapper = new DocumentWrapperForTesting("Top Level");
			DocumentWrapperCollectionForTesting collection = docWrapper.Children;
			collection.RemoveAll();

			collection.Add(new DocumentWrapperForTesting("String2", 2));
			collection.Add(new DocumentWrapperForTesting("String1", 2));
			collection.Add(new DocumentWrapperForTesting("String5", 2));
			collection.Add(new DocumentWrapperForTesting("String0", 1));
			collection.Add(new DocumentWrapperForTesting("String4", 1));
			collection.Add(new DocumentWrapperForTesting("String3", 1));

			IDataRowSource dS = new BusinessObjectDataSource("Children", collection, true);
			IDataRowSource[] groups = dS.GroupBy(new string[] { "TestInt" });

			BusinessObjectDataProvider provider = new BusinessObjectDataProvider(new DataProviderList(docWrapper), null);
			AssertEquals(2, groups.Length);
			AssertEquals(3, groups[0].RowCount);
			AssertEquals("String2", provider.GetColumnValue(groups[0], 0, "Children.TestString"));
			AssertEquals("String1", provider.GetColumnValue(groups[0], 1, "Children.TestString"));
			AssertEquals("String5", provider.GetColumnValue(groups[0], 2, "Children.TestString"));
			AssertEquals("String0", provider.GetColumnValue(groups[1], 0, "Children.TestString"));
			AssertEquals("String4", provider.GetColumnValue(groups[1], 1, "Children.TestString"));
			AssertEquals("String3", provider.GetColumnValue(groups[1], 2, "Children.TestString"));
		}

		public void TestGroupByIndirectPropertyNameWithCollectionPrefix()
		{
			TestGroupByIndirectProperty("ChildrenValueHolderActualValue");
		}

		public void TestGroupByIndirectPropertyNameWithCollectionPrefixAndDot()
		{
			TestGroupByIndirectProperty("Children.ValueHolderActualValue");
		}

		public void TestGroupByMultipleColumns()
		{
			var docWrapper = new DocumentWrapperForTesting("Top Level");
			DocumentWrapperCollectionForTesting collection = docWrapper.Children;
			collection.RemoveAll();

			for (int i = 0; i < 3; i++)
			{
				collection.Add(new DocumentWrapperForTesting("Group 1", 1));
			}
			for (int i = 0; i < 4; i++)
			{
				collection.Add(new DocumentWrapperForTesting("Group 1", 2));
			}
			for (int i = 0; i < 5; i++)
			{
				collection.Add(new DocumentWrapperForTesting("Group 2", 1));
			}
			for (int i = 0; i < 6; i++)
			{
				collection.Add(new DocumentWrapperForTesting("Group 2", 2));
			}

			IDataRowSource dS = new BusinessObjectDataSource("Children", collection);
			IDataRowSource[] groups = dS.GroupBy(new string[] { "TestString", "TestInt" });

			BusinessObjectDataProvider provider = new BusinessObjectDataProvider(new DataProviderList(docWrapper), null);
			AssertEquals(4, groups.Length);
			AssertEquals(3, groups[0].RowCount);
			AssertEquals("Group 1 TestIntValue 1 Teststring", "Group 1", provider.GetColumnValue(groups[0], 1, "Children.TestString"));
			AssertEquals("Group 1 TestIntValue 1 TestInt", 1, provider.GetColumnValue(groups[0], 1, "Children.TestInt"));
			AssertEquals(4, groups[1].RowCount);
			AssertEquals("Group 1 TestIntValue 2 Teststring", "Group 1", provider.GetColumnValue(groups[1], 1, "Children.TestString"));
			AssertEquals("Group 1 TestIntValue 2 TestInt", 2, provider.GetColumnValue(groups[1], 2, "Children.TestInt"));
			AssertEquals(5, groups[2].RowCount);
			AssertEquals("Group 2 TestIntValue 1 Teststring", "Group 2", provider.GetColumnValue(groups[2], 2, "Children.TestString"));
			AssertEquals("Group 2 TestIntValue 1 TestInt", 1, provider.GetColumnValue(groups[2], 1, "Children.TestInt"));
			AssertEquals(6, groups[3].RowCount);
			AssertEquals("Group 2 TestIntValue 2 TestIntValue", "Group 2", provider.GetColumnValue(groups[3], 2, "Children.TestString"));
			AssertEquals("Group 2 TestIntValue 2 TestInt", 2, provider.GetColumnValue(groups[3], 2, "Children.TestInt"));
		}

		public void TestGroupByNestedOnlyGetsNestedRows()
		{
			DocumentWrapperCollectionForTesting collection = new DocumentWrapperCollectionForTesting();
			collection.Add(new DocumentWrapperForTesting("Group1"));
			collection.Add(new DocumentWrapperForTesting("Group1"));
			collection.Add(new DocumentWrapperForTesting("Group1"));

			collection.Add(new DocumentWrapperForTesting("Group2"));

			IDataRowSource dataSource = new BusinessObjectDataSource("Children", collection);
			IDataRowSource[] groups = dataSource.GroupBy(new string[] { "TestString" });
			AssertEquals("Groups.Length", 2, groups.Length);

			BusinessObjectDataSource nestedDataSource = (BusinessObjectDataSource)groups[1];
			IDataRowSource[] nestedGroups = nestedDataSource.GroupBy(new string[] { "TestString" });
			AssertEquals("nestedGroups.Length", 1, nestedGroups.Length);
		}

		public void TestSplit()
		{
			DocumentWrapperCollectionForTesting collection = new DocumentWrapperCollectionForTesting();
			collection.Add(new DocumentWrapperForTesting("Row 1"));
			collection.Add(new DocumentWrapperForTesting("Row 2"));
			collection.Add(new DocumentWrapperForTesting("Row 3"));
			collection.Add(new DocumentWrapperForTesting("Row 4"));
			IDataRowSource dS1 = new BusinessObjectDataSource("Children", collection);
			IDataRowSource dS2 = dS1.Split(1);
			AssertEquals(1, dS1.RowCount);
			AssertEquals(3, dS2.RowCount);
		}

		[ExpectNoExceptions]
		public void TestGetFirstNRowsForNotSupportAddNewCollection()
		{
			DocumentWrapperCollectionForTestingNotAllowNew collection = new DocumentWrapperCollectionForTestingNotAllowNew();
			IDataRowSource dS1 = new BusinessObjectDataSource("Children", collection);
			IDataRowSource dS2 = dS1.GetFirstNRows(3);
		}

		public void TestGetFirstNRows()
		{
			DocumentWrapperCollectionForTesting collection = new DocumentWrapperCollectionForTesting();
			collection.Add(new DocumentWrapperForTesting("Row 1"));
			collection.Add(new DocumentWrapperForTesting("Row 2"));
			collection.Add(new DocumentWrapperForTesting("Row 3"));
			collection.Add(new DocumentWrapperForTesting("Row 4"));
			IDataRowSource dS1 = new BusinessObjectDataSource("Children", collection);
			IDataRowSource dS2 = dS1.GetFirstNRows(3);
			AssertEquals(4, dS1.RowCount);
			AssertEquals(3, dS2.RowCount);
		}

		public void TestGetFirstNRowsWithMoreRowsThanInDataSource()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(factory);
			DataTable tbl = new DataTable();
			tbl.Columns.Add("Col", typeof(string));
			tbl.Columns.Add("Z0_PK", typeof(Guid));
			tbl.Rows.Add(new object[] { "Row 0", Guid.NewGuid() });
			tbl.Rows.Add(new object[] { "Row 1", Guid.NewGuid() });
			tbl.Rows.Add(new object[] { "Row 2", Guid.NewGuid() });
			tbl.Rows.Add(new object[] { "Row 3", Guid.NewGuid() });
			collection.Add(new DummyBusinessObject(factory, tbl.Rows[0]));
			collection.Add(new DummyBusinessObject(factory, tbl.Rows[1]));
			collection.Add(new DummyBusinessObject(factory, tbl.Rows[2]));
			collection.Add(new DummyBusinessObject(factory, tbl.Rows[3]));
			IDataRowSource dS1 = new BusinessObjectDataSource("Children", collection);
			IDataRowSource dS2 = dS1.GetFirstNRows(6);
			AssertEquals(4, dS1.RowCount);
			AssertEquals(6, dS2.RowCount);
		}

		void TestGroupByIndirectProperty(string groupByName)
		{
			var docWrapper = new DocumentWrapperForTesting("Top Level");
			DocumentWrapperCollectionForTesting collection = docWrapper.Children;
			collection.RemoveAll();

			DocumentWrapperForTesting temp;
			for (int i = 0; i < 5; i++)
			{
				temp = new DocumentWrapperForTesting("A");
				temp.TestValue = "4";
				collection.Add(temp);
			}
			for (int i = 0; i < 7; i++)
			{
				temp = new DocumentWrapperForTesting("B");
				temp.TestValue = "3";
				collection.Add(temp);
			}
			for (int i = 0; i < 2; i++)
			{
				temp = new DocumentWrapperForTesting("C");
				temp.TestValue = "1";
				collection.Add(temp);
			}
			for (int i = 0; i < 6; i++)
			{
				temp = new DocumentWrapperForTesting("D");
				temp.TestValue = "2";
				collection.Add(temp);
			}

			IDataRowSource dS = new BusinessObjectDataSource("Children", collection);
			IDataRowSource[] groups = dS.GroupBy(new string[] { groupByName });

			BusinessObjectDataProvider provider = new BusinessObjectDataProvider(new DataProviderList(docWrapper), null);
			AssertEquals(4, groups.Length);
			AssertEquals(2, groups[0].RowCount);
			AssertEquals("1", provider.GetColumnValue(groups[0], 0, groupByName));
			AssertEquals(6, groups[1].RowCount);
			AssertEquals("2", provider.GetColumnValue(groups[1], 0, groupByName));
			AssertEquals(7, groups[2].RowCount);
			AssertEquals("3", provider.GetColumnValue(groups[2], 0, groupByName));
			AssertEquals(5, groups[3].RowCount);
			AssertEquals("4", provider.GetColumnValue(groups[3], 0, groupByName));
		}

		public void TestGroupByWithDateTimeColumn()
		{
			var parent = new BusinessObjectCollectionForTestingParent();
			var collection = parent.Children;
			collection.Add(new BusinessObjectForTestingWith3ZTypedFields(1, "Mohsen", new ZDateTime(2005, 1, 5)));
			collection.Add(new BusinessObjectForTestingWith3ZTypedFields(2, "Ali", new ZDateTime(2005, 2, 1)));
			collection.Add(new BusinessObjectForTestingWith3ZTypedFields(2, "Reza", new ZDateTime(2005, 1, 5)));
			collection.Add(new BusinessObjectForTestingWith3ZTypedFields(1, "Ahmad", new ZDateTime(2005, 2, 1)));

			IDataRowSource dS = new BusinessObjectDataSource("Children", collection);
			IDataRowSource[] groups = dS.GroupBy(new string[] { "Field3" });
			BusinessObjectDataProvider provider = new BusinessObjectDataProvider(new DataProviderList(parent), null);
			AssertEquals(2, groups.Length);
			AssertEquals(new ZDateTime(2005, 1, 5), provider.GetColumnValue(groups[0], 0, "Children.Field3"));
			AssertEquals(new ZDateTime(2005, 2, 1), provider.GetColumnValue(groups[1], 0, "Children.Field3"));
		}

		class BusinessObjectCollectionForTestingParent : IBODocDataProvider
		{
			public BusinessObjectCollectionForTesting Children
			{
				get { return children ?? (children = new BusinessObjectCollectionForTesting()); }
			}
			BusinessObjectCollectionForTesting children;

			#region IBODocDataProvider Members

			DocWrapperCopyInfo IBODocDataProvider.AdditionalCopyInfo
			{
				get { return null; }
			}

			BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst
			{
				get { return null; }
			}

			ZString IBODocDataProvider.GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue)
			{
				return ZString.Empty;
			}

			string[] IBODocDataProvider.ImageNamesToRemove
			{
				get { return null; }
			}

			BusinessObject IBODocDataProvider.ParentBusinessObject
			{
				get { return null; }
			}

			void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants)
			{
			}

			IZType IBODocDataProvider.GetCustomField(string fieldName, string typeName)
			{
				return ZString.Empty;
			}

			string IBODocDataProvider.GetCustomFieldCodeDescription(string fieldName, string typeName)
			{
				return string.Empty;
			}

			ZDateTime IBODocDataProvider.GetEventLastDateTime(string eventCode)
			{
				return ZDateTime.Empty;
			}

			#endregion
		}

		public void TestFilter()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			DummyBusinessObject obj = collection.AddNew();
			obj.Z0_VarCharMax = "Blah";
			obj.Z0_Bool = false;
			obj.Z0_Number = 5;
			obj = collection.AddNew();
			obj.Z0_VarCharMax = "Blah";
			obj.Z0_Bool = true;
			obj.Z0_Number = 5;
			obj = collection.AddNew();
			obj.Z0_VarCharMax = "Tom";
			obj.Z0_Bool = false;
			obj.Z0_Number = 1;
			AssertEquals(2, new BusinessObjectDataSource("Collection", collection).Filter("\"<Z0_VarCharMax>\"==\"Tom\" || (\"<Z0_VarCharMax>\"==\"Blah\" && <Z0_Bool> == false)").RowCount);
		}

		public void TestFilterWithBackSlash()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			DummyBusinessObject obj = collection.AddNew();
			obj = collection.AddNew();
			obj.Z0_VarCharMax = "D\\";
			obj.Z0_Bool = true;
			obj.Z0_Number = 5;
			AssertNoExceptionThrown(() => { new BusinessObjectDataSource("Collection", collection).Filter("\"<Z0_VarCharMax>\"!=\"\""); });
		}

		public void TestFilterInFrenchCulture()
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var obj = collection.AddNew();
			obj = collection.AddNew();
			obj.Z0_VarCharMax = "D\\";
			obj.Z0_Bool = true;
			obj.Z0_Decimal = 5.0000m;
			obj = collection.AddNew();
			obj.Z0_VarCharMax = "D\\";
			obj.Z0_Bool = true;
			obj.Z0_Decimal = 0.0000m;

			using (Culture.SetTemporarily(new CultureInfo("FR-fr")))
			{
				AssertNotEquals(0, new BusinessObjectDataSource("Collection", collection).Filter("<Z0_Decimal> != 0").RowCount);
				AssertEquals(1, new BusinessObjectDataSource("Collection", collection).Filter("<Z0_Decimal> != 0").RowCount);
			}
		}
	}
}
