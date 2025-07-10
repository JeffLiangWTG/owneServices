using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleTimeFilter))]
	sealed class ModuleTimeFilterTest : ModuleFilterTestCase<ModuleTimeFilter>
	{
		public void TestCopyTransientProperties()
		{
			var copiedFilter = GetNewModuleFilter();
			copiedFilter.Property1 = new ZTime(22, 10);
			copiedFilter.Property2 = new ZTime(22, 11);

			Filter.Property1 = new ZTime(22, 13);
			Filter.Property2 = new ZTime(22, 14);
			copiedFilter.CopyTransientProperties(Filter);

			AssertEquals(new ZTime(22, 13), copiedFilter.Property1);
			AssertEquals(new ZTime(22, 14), copiedFilter.Property2);
		}

		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region TestClearResetsToDefaults

		public void TestClearResetsToDefaults()
		{
			ZString searchValue = "Today";
			var dateValue1 = new ZTime(6, 25);
			var dateValue2 = new ZTime(6, 26);

			Filter.PropertySearch = searchValue;
			Filter.Property1 = dateValue1;
			Filter.Property2 = dateValue2;

			AssertEquals("Precondition", searchValue, Filter.PropertySearch);
			AssertEquals("Precondition", dateValue1, Filter.Property1);
			AssertEquals("Precondition", dateValue2, Filter.Property2);

			Filter.Clear();
			AssertEquals("", Filter.PropertySearch);
			AssertEquals(ZTime.Empty, Filter.Property1);
			AssertEquals(ZTime.Empty, Filter.Property2);
		}

		#endregion

		#region TestIsEmpty

		public void TestIsEmpty()
		{
			Filter.Property1 = new ZTime(1,2);
			Filter.Property2 = new ZTime(1,2);

			Filter.PropertySearch = "";
			AssertEquals(true, Filter.IsEmpty);

			Filter.PropertySearch = ModuleTimeFilter.HasTimeEntered;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleTimeFilter.HasNoTimeEntered;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = "crap data";
			AssertEquals(true, Filter.IsEmpty);

			Filter.PropertySearch = ModuleTimeFilter.SpecifiedTimeRange;
			AssertEquals(false, Filter.IsEmpty);

			Filter.Property1 = ZTime.Empty;
			Filter.Property2 = ZTime.Empty;
			AssertEquals(true, Filter.IsEmpty);
		}

		#endregion

		#region TestPropertySearch_ListContainsSpecifiedTimeRange

		public void TestPropertySearch_ListContainsSpecifiedTimeRange()
		{
			AssertEquals(true, Filter.PropertySearch_List.ContainsCode(ModuleTimeFilter.SpecifiedTimeRange));
		}

		#endregion

		#region TestIsPropertySearchUsingSpecifiedTimeRange

		public void TestIsPropertySearchUsingSpecifiedTimeRange()
		{
			Filter.PropertySearch = ModuleTimeFilter.HasNoTimeEntered;
			AssertEquals(false, Filter.IsPropertySearchUsingSpecifiedTimeRange);

			Filter.PropertySearch = ModuleTimeFilter.SpecifiedTimeRange;
			AssertEquals(true, Filter.IsPropertySearchUsingSpecifiedTimeRange);
		}

		#endregion

		#region TestPropertySearch_List

		string[] ExpectedPropertySearchListCore
		{
			get
			{
				return new string[]
				{
					string.Empty,
					"Time Ranges Category",
					ModuleTimeFilter.SpecifiedTimeRange,

					string.Empty,
					"Time Entered Category",
					ModuleTimeFilter.HasTimeEntered,
					ModuleTimeFilter.HasNoTimeEntered
				};
			}
		}

		public void TestPropertySearch_List()
		{
			var dateRangePairList = Filter.PropertySearch_List;

			AssertEquals("List count", ExpectedPropertySearchListCore.Length, dateRangePairList.Count);

			foreach (var code in ExpectedPropertySearchListCore)
			{
				Assert("Search List contains code: " + code, dateRangePairList.ContainsCode(code));
			}

			foreach (CodeDescriptionPair pair in dateRangePairList)
			{
				if (pair.MultilingualCode.IsEmpty || pair.GetType().Equals(typeof(CategoryCodeDescriptionPair)))
				{
					continue;
				}
				Assert("Code of every element in ProperySearch_List of ModuleTimeFilter should be MultilingualString.", !pair.MultilingualCode.GetType().Equals(typeof(NoResString)));
			}
		}

		#endregion

		#region TestPropertySearch_ListContainsTimeEntered

		ZQuery DummyDelegate(TimeComparisonOperator comparisonOperator, ZTime date1, ZTime date2)
		{
			return new ZQuery();
		}

		public void TestPropertySearch_ListContainsTimeEntered()
		{
			var filterNullable = new ModuleTimeFilter("Nullable", DummyDelegate, true);
			var filterNotNullable = new ModuleTimeFilter("Not Nullable", DummyDelegate, false);

			AssertEquals(true, filterNullable.PropertySearch_List.ContainsCode(ModuleTimeFilter.HasNoTimeEntered));
			AssertEquals(true, filterNullable.PropertySearch_List.ContainsCode(ModuleTimeFilter.HasTimeEntered));
			AssertEquals(false, filterNotNullable.PropertySearch_List.ContainsCode(ModuleTimeFilter.HasNoTimeEntered));
			AssertEquals(false, filterNotNullable.PropertySearch_List.ContainsCode(ModuleTimeFilter.HasTimeEntered));
		}

		#endregion

		#region TestIsPropertySearchUsingHasTimeEntered

		public void TestIsPropertySearchUsingHasTimeEntered()
		{
			Filter.PropertySearch = ModuleTimeFilter.HasNoTimeEntered;
			AssertEquals(false, Filter.IsPropertySearchUsingHasTimeEntered);

			Filter.PropertySearch = ModuleTimeFilter.HasTimeEntered;
			AssertEquals(true, Filter.IsPropertySearchUsingHasTimeEntered);
		}

		#endregion

		#region TestIsPropertySearchUsingHasNoTimeEntered

		public void TestIsPropertySearchUsingHasNoTimeEntered()
		{
			Filter.PropertySearch = ModuleTimeFilter.HasTimeEntered;
			AssertEquals(false, Filter.IsPropertySearchUsingHasNoTimeEntered);

			Filter.PropertySearch = ModuleTimeFilter.HasNoTimeEntered;
			AssertEquals(true, Filter.IsPropertySearchUsingHasNoTimeEntered);
		}

		#endregion

		#region Testing the Query results

		#region TestQueryWithSpecifiedTimes

		[TestDate(2007, 1, 1, 1, 2, 3)]
		public void TestQueryWithSpecifiedTimes()
		{
			Filter.PropertySearch = ModuleTimeFilter.SpecifiedTimeRange;
			ZTime baseTime = new ZTime(1, 2);
			ZTime timeToFind = baseTime;
			ZTime timeToFind2 = baseTime.AddMinutes(1);
			ZTime timeToFind3 = baseTime.AddHours(1).AddMinutes(-1);
			ZTime timeToNotFind = baseTime.AddHours(1);

			var dummy1b = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy1c = Factory.NewWithValidTestData<DummyBusinessObject>();

			Dummy1.Z0_SmallDateTime = new ZDate(1900, 1, 1).Add(timeToFind.ToTimeSpan());
			dummy1b.Z0_SmallDateTime = new ZDate(1900, 1, 1).Add(timeToFind2.ToTimeSpan());
			dummy1c.Z0_SmallDateTime = new ZDate(1900, 1, 1).Add(timeToFind3.ToTimeSpan());
			Dummy2.Z0_SmallDateTime = new ZDate(1900, 1, 1).Add(timeToNotFind.ToTimeSpan());
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);

			// from date only
			Filter.Property1 = timeToFind;
			Filter.Property2 = ZTime.Empty;
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionContains(dummy1b, dummies);
			AssertCollectionContains(dummy1c, dummies);
			AssertCollectionContains(Dummy2, dummies);

			// from date only (not all included)
			Filter.Property1 = timeToFind2;
			Filter.Property2 = ZTime.Empty;
			dummies.Load(Filter.Query);
			AssertCollectionNotContains(Dummy1, dummies);
			AssertCollectionContains(dummy1b, dummies);
			AssertCollectionContains(dummy1c, dummies);
			AssertCollectionContains(Dummy2, dummies);

			// to date only
			Filter.Property1 = ZTime.Empty;
			Filter.Property2 = timeToFind3;
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionContains(dummy1b, dummies);
			AssertCollectionContains(dummy1c, dummies);
			AssertCollectionNotContains(Dummy2, dummies);

			// both to and from dates
			Filter.Property1 = timeToFind2;
			Filter.Property2 = timeToFind3;
			dummies.Load(Filter.Query);
			AssertCollectionNotContains(Dummy1, dummies);
			AssertCollectionContains(dummy1b, dummies);
			AssertCollectionContains(dummy1c, dummies);
			AssertCollectionNotContains(Dummy2, dummies);

			// both to and from dates, reversed
			Filter.Property1 = timeToFind3;
			Filter.Property2 = timeToFind;
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionNotContains(dummy1b, dummies);
			AssertCollectionContains(dummy1c, dummies);
			AssertCollectionContains(Dummy2, dummies);
		}

		#endregion

		#region TestQueryUsingHasTimeEntered

		public void TestQueryUsingHasTimeEntered()
		{
			Filter.PropertySearch = ModuleTimeFilter.HasTimeEntered;

			Dummy1.Z0_SmallDateTime = new ZDateTime(1900, 1, 1, 13, 35, 0);
			Dummy2.Z0_SmallDateTime = ZDateTime.Empty;
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);

			Filter.Property1 = new ZTime(1, 13);
			Filter.Property2 = ZTime.Empty;
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);

			Filter.Property1 = ZTime.Empty;
			Filter.Property2 = new ZTime(13, 35);
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);
		}

		public void TestQueryUsingHasTimeEnteredNullable()
		{
			Filter.PropertySearch = ModuleTimeFilter.HasTimeEntered;

			Dummy1.Z0_Time = new ZTime(13, 35);
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);
			Filter.Property1 = new ZTime(13, 35);

			var originalValue = Filter.FilterColumn.IsNullable;

			try
			{
				typeof(SchemaTimeColumn)
						 .GetField("IsNullable", BindingFlags.Instance | BindingFlags.Public)
						 .SetValue(Filter.FilterColumn, false);

				dummies.Load(Filter.Query);
				AssertCollectionContains(Dummy1, dummies);
			}
			finally
			{
				typeof(SchemaTimeColumn)
						 .GetField("IsNullable", BindingFlags.Instance | BindingFlags.Public)
						 .SetValue(Filter.FilterColumn, originalValue);
			}
		}

		#endregion

		#region TestQueryUsingHasNoTimeEntered

		public void TestQueryUsingHasNoTimeEntered()
		{
			Filter.PropertySearch = ModuleTimeFilter.HasNoTimeEntered;

			Dummy1.Z0_SmallDateTime = new ZDateTime(1900, 1, 1, 13, 35, 0);
			Dummy2.Z0_SmallDateTime = ZDateTime.Empty;
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);

			Filter.Property1 = new ZTime(13, 35);
			Filter.Property2 = ZTime.Empty;
			dummies.Load(Filter.Query);
			AssertCollectionNotContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);

			Filter.Property1 = ZTime.Empty;
			Filter.Property2 = new ZTime(13, 35);
			dummies.Load(Filter.Query);
			AssertCollectionNotContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);
		}

		#endregion

		#region TestQueryUsingHasNoTimeEnteredUsingDataView

		public void TestQueryUsingHasNoTimeEnteredUsingDataView()
		{
			var rowFactory = Factory.GetType().GetProperty("RowFactory",
				BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Factory, null) as RowFactory;
			typeof(RowFactory).GetProperty("MaximumRowsBeforeUsingIndex").SetValue(rowFactory, 0, null);//To make sure the query will use dataview

			Filter.PropertySearch = ModuleTimeFilter.HasNoTimeEntered;
			Dummy1.Z0_SmallDateTime = new ZDateTime(1900, 1, 1, 13, 35, 0);
			Dummy2.Z0_SmallDateTime = ZDateTime.Empty;
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);

			dummies.Load(Filter.Query);
			AssertCollectionNotContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);
		}

		#endregion

		public void AssertResultsUsingCurrentFilter(ZTime timeToFind, ZTime timeToNotFind, ZString propertySearchText)
		{
			Filter.PropertySearch = propertySearchText;
			Dummy1.Z0_Time = timeToFind;
			Dummy2.Z0_Time = timeToNotFind;
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			dummies.Load(Filter.Query);

			AssertCollectionContains(Dummy1.PK, dummies.Select(x => x.PK));
			AssertCollectionNotContains(Dummy2.PK, dummies.Select(x => x.PK));
		}

		#endregion

		#region Serialisation

		public void TestDeserializePropertiesFromXml()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new DummyModuleTimeFilter();

			ZString searchValue = ModuleTimeFilter.HasNoTimeEntered;
			var dateValue1 = new ZTime(7, 08);
			var dateValue2 = new ZTime(6, 08);

			filter.PropertySearch = searchValue;
			filter.Property1 = dateValue1;
			filter.Property2 = dateValue2;

			filterStripBizO.AddModuleFilterForTest(filter);

			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			strip.FilterDescription = "";
			strip.Delete();

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleTimeFilter)filterStripBizO[filter.Description];

			AssertEquals(ModuleTimeFilter.SpecifiedTimeRange, loadedFilter.PropertySearch);
			AssertEquals(ZTime.Empty, loadedFilter.Property1);
			AssertEquals(dateValue2, loadedFilter.Property2);
		}

		public void TestDeserializeInvalidTimeFromXml()
		{
			var filter = new ModuleTimeFilter("Filter", DummyBizoSchema.Z0_SmallDateTime);

			using (var stringReader = new StringReader("<Property1>somethingInvalid</Property1>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				filter.DeserializeProperties(xmlReader);
				AssertEquals("Nothing should happen when deserializing invalid data", ZTime.Empty, filter.Property1);
			}

			using (var stringReader = new StringReader("<Property2>somethingInvalid</Property2>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				filter.DeserializeProperties(xmlReader);
				AssertEquals("Nothing should happen when deserializing invalid data", ZTime.Empty, filter.Property2);
			}

			using (var stringReader = new StringReader("<Property1>10:00:00</Property1>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				filter.DeserializeProperties(xmlReader);
				AssertEquals("Valid value should be deserialised correctly", new ZTime(10, 0), filter.Property1);
			}

			using (var stringReader = new StringReader("<Property2>10:00:00</Property2>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				filter.DeserializeProperties(xmlReader);
				AssertEquals("Valid value should be deserialised correctly", new ZTime(10, 0), filter.Property2);
			}
		}

		public void TestDeserialise_WithAndWithoutFilterOptionProperty()
		{
			var filter = new ModuleTimeFilter("Iiii'm the ahh Prime Minister", DummyBizoSchema.Z0_SmallDateTime);

			using (var stringReader = new StringReader("<FilterBlah><Property1>15:07:14</Property1></FilterBlah>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				xmlReader.Read(); // Move past the root element
				filter.DeserializeProperties(xmlReader);
				AssertEquals(new ZTime(15, 7), filter.Property1);
			}
		}

		public void TestSerialise_ShouldSerialiseFilterOption()
		{
			var filter = new ModuleTimeFilter("Iiii'm the ahh Prime Minister", DummyBizoSchema.Z0_SmallDateTime);
			filter.PropertySearch = ModuleTimeFilter.SpecifiedTimeRange;
			filter.Property1 = new ZTime(15, 7);
			filter.Property2 = new ZTime(15, 7);

			var result = new StringBuilder();

			using (var xmlWriter = XmlWriter.Create(result, new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment }))
			{
				((IXmlSerializable)filter).WriteXml(xmlWriter);
			}

			AssertEquals("<SearchProperty>Time range</SearchProperty><Property1>15:07:00</Property1><Property2>15:07:00</Property2>", result.ToString());
		}

		#endregion

		#region TestToAndFromTime
		public void TestFromTime_RemoveTimeComponentWhenIsPropertySearchUsingSpecifiedTimeRange()
		{
			var testFromTime = new ZTime(12, 34);
			Filter.Property1 = testFromTime;

			Filter.PropertySearch = ModuleTimeFilter.SpecifiedTimeRange;
			AssertEquals(true, Filter.IsPropertySearchUsingSpecifiedTimeRange);

			var expectedResult = new ZDateTime(1900, 1, 1, 12, 34, 0);
			AssertEquals(expectedResult, Filter.FromTime);
		}
		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			Dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
		}

		DummyBusinessObject Dummy1;
		DummyBusinessObject Dummy2;

		protected override ModuleTimeFilter GetNewModuleFilter()
		{
			return new ModuleTimeFilter("moo", DummyBizoSchema.Z0_SmallDateTime);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Times; }
		}

		#endregion
	}
}
