using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class ModuleNumberRangeFilterTest : ModuleFilterTestCase<ModuleNumberRangeFilter>
	{
		#region TestPropertyShouldNotBeSetInSpecificPropertySearch

		public void TestPropertyShouldNotBeSetInSpecificPropertySearch()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter1 = new DummyModuleNumberRangeFilter("filter1", DummyBizoSchema.Z0_AnotherDecimal);
			var filter2 = new DummyModuleNumberRangeFilter("filter2", DummyBizoSchema.Z0_AnotherDecimal);
			filter1.ShouldOverrideSerializeFunction = false;
			filter2.ShouldOverrideSerializeFunction = false;

			var lessThanOrEqualTo = ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo.GetUnresolvedString();
			var greaterThanOrEqualTo = ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo.GetUnresolvedString();

			filter1.SetOldVersionValueToProperty(lessThanOrEqualTo, new ZDecimal(-99999999999999.99), null);
			filter2.SetOldVersionValueToProperty(greaterThanOrEqualTo, null, new ZDecimal(99999999999999.99));
			filterStripBizO.AddModuleFilterForTest(filter1);
			filterStripBizO.AddModuleFilterForTest(filter2);

			var strip1 = filterStripBizO.FilterStrips.AddNew();
			strip1.FilterDescription = filter1.Description;

			var strip2 = filterStripBizO.FilterStrips.AddNew();
			strip2.FilterDescription = filter2.Description;

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			strip1.FilterDescription = "";
			strip1.Delete();

			strip2.FilterDescription = "";
			strip2.Delete();

			AssertNoExceptionThrown(() => { filterStripBizO.LoadLayout(savedFilter); });

			AssertEquals(new ZDecimal(-99999999999999.99m), ((ModuleNumberRangeFilter)filterStripBizO["filter1"]).Property1);

			AssertEquals(new ZDecimal(99999999999999.99m), ((ModuleNumberRangeFilter)filterStripBizO["filter2"]).Property2);
		}

		#endregion

		#region TestMaxValueForPrecisionAndScale

		public void TestMaxValueForPrecisionAndScale()
		{
			var schema1 = new SchemaDecimalColumn(DummyBizoSchema.Instance, "TestColumn", 0, SqlDbType.Decimal, (decimal)0, false, 16, 3, false);

			var filter1 = new ModuleNumberRangeFilter("TestDummy", schema1);
			AssertEquals("minValue", new ZDecimal(-9999999999999.999m), filter1.MinValue);
			AssertEquals("minValue", new ZDecimal(9999999999999.999m), filter1.MaxValue);
		}

		#endregion

		#region ModuleFilterChanged event test

		public void TestChangesFireFilterChangedEvent()
		{
			var filter = new ModuleNumberRangeFilter("desc", delegate
			{ return new ZQuery(); });

			filter.ModuleFilterChanged += new EventHandler(filter_ModuleFilterChanged);

			moduleFilterChangedEventFired = false;
			filter.PropertyType = ZCalcEditPropertyType.Decimal;
			AssertEquals(true, moduleFilterChangedEventFired);

			moduleFilterChangedEventFired = false;
			filter.Decimals = byte.MaxValue;
			AssertEquals(true, moduleFilterChangedEventFired);
		}

		void filter_ModuleFilterChanged(object sender, EventArgs e)
		{
			moduleFilterChangedEventFired = true;
		}

		bool moduleFilterChangedEventFired;

		#endregion

		#region TestDecimalsAndPropertyTypeUsingFilterColumn

		public void TestDecimalsAndPropertyTypeUsingFilterColumn()
		{
			var filter = new ModuleNumberRangeFilter("Moo", DummyBizoSchema.Z0_Byte);
			AssertEquals("Decimals", (byte)0, filter.Decimals);
			AssertEquals("PropertyType", ZCalcEditPropertyType.Byte, filter.PropertyType);

			filter = new ModuleNumberRangeFilter("Moo", DummyBizoSchema.Z0_Decimal);
			AssertEquals("Decimals", DummyBizoSchema.Z0_Decimal.Scale, filter.Decimals);
			AssertEquals("PropertyType", ZCalcEditPropertyType.Decimal, filter.PropertyType);

			filter.Decimals = 4;
			AssertEquals("Decimals", (byte)4, filter.Decimals);

			filter = new ModuleNumberRangeFilter("Moo", DummyBizoSchema.Z0_Number);
			AssertEquals("Decimals", (byte)0, filter.Decimals);
			AssertEquals("PropertyType", ZCalcEditPropertyType.Int, filter.PropertyType);

			filter = new ModuleNumberRangeFilter("Moo", DummyBizoSchema.Z0_Short);
			AssertEquals("Decimals", (byte)0, filter.Decimals);
			AssertEquals("PropertyType", ZCalcEditPropertyType.Short, filter.PropertyType);
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ filter.PropertyType = ZCalcEditPropertyType.Byte; });

			filter = new ModuleNumberRangeFilter("Moo", DummyBizoSchema.Z0_Long);
			AssertEquals("Decimals", (byte)0, filter.Decimals);
			AssertEquals("PropertyType", ZCalcEditPropertyType.Long, filter.PropertyType);
		}

		#endregion

		#region TestDecimalsAndPropertyTypeUsingQueryDelegate

		public void TestDecimalsAndPropertyTypeUsingQueryDelegate()
		{
			var filter = new ModuleNumberRangeFilter("Moo", (INumericZType value1, INumericZType value2) => { return null; });

			AssertEquals("Decimals", (byte)2, filter.Decimals);
			AssertEquals("PropertyType", ZCalcEditPropertyType.Decimal, filter.PropertyType);

			filter.Decimals = 3;
			AssertEquals("Decimals", (byte)3, filter.Decimals);

			filter.PropertyType = ZCalcEditPropertyType.Byte;
			AssertEquals("Decimals", (byte)0, filter.Decimals);
			AssertEquals("PropertyType", ZCalcEditPropertyType.Byte, filter.PropertyType);

			filter.Decimals = 0;
			AssertEquals("Decimals", (byte)0, filter.Decimals);
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ filter.Decimals = 2; });
		}

		#endregion

		#region TestIntegerBoundsValidation

		public void TestIntegerBoundsValidation()
		{
			var filter = new ModuleNumberRangeFilter("Moo", DummyBizoSchema.Z0_Decimal);
			filter.Property1 = 99999999999998.99m;
			filter.Property2 = 99999999999998.99m;
			AssertNoErrors(filter.Property1Info);
			AssertNoErrors(filter.Property2Info);

			filter.Property1 = -99999999999998.99m;
			filter.Property2 = -99999999999998.99m;
			AssertNoErrors(filter.Property1Info);
			AssertNoErrors(filter.Property2Info);

			filter.Property1 = 999999999999999m;
			filter.Property2 = 999999999999999m;
			AssertHasError(filter.Property1Info, "Please enter a value less than or equal to 100,000,000,000,000.");
			AssertHasError(filter.Property2Info, "Please enter a value less than or equal to 100,000,000,000,000.");

			filter.Property1 = -999999999999999m;
			filter.Property2 = -999999999999999m;
			AssertHasError(filter.Property1Info, "Please enter a value greater than or equal to -100,000,000,000,000.");
			AssertHasError(filter.Property2Info, "Please enter a value greater than or equal to -100,000,000,000,000.");

			filter = new ModuleNumberRangeFilter("Moo", new SchemaDecimalColumn(DummyBizoSchema.Instance, "Z0_Decimal", 0, SqlDbType.Decimal, (decimal)0, false, 9, 3, false, "dbo.TVP_decimal_9_3"));

			filter.Property1 = 999999.999m;
			filter.Property2 = 999999.999m;
			AssertNoErrors(filter.Property1Info);
			AssertNoErrors(filter.Property2Info);

			filter.Property1 = -999999.999m;
			filter.Property2 = -999999.999m;
			AssertNoErrors(filter.Property1Info);
			AssertNoErrors(filter.Property2Info);

			filter.Property1 = 1000000m;
			filter.Property2 = 1000000m;
			AssertHasError(filter.Property1Info, "Please enter a value less than or equal to 999,999.999.");
			AssertHasError(filter.Property2Info, "Please enter a value less than or equal to 999,999.999.");

			filter.Property1 = -1000000m;
			filter.Property2 = -1000000m;
			AssertHasError(filter.Property1Info, "Please enter a value greater than or equal to -999,999.999.");
			AssertHasError(filter.Property2Info, "Please enter a value greater than or equal to -999,999.999.");
		}

		#endregion

		#region TestMaxMinValues

		public void TestMaxMinValues()
		{
			var filter = new ModuleNumberRangeFilter("Moo", DummyBizoSchema.Z0_Decimal);
			filter.Property1 = new ZDecimal("-1000.00");
			filter.Property2 = new ZDecimal("10000.00");
			filter.Validation.ValidateProperty1();
			filter.Validation.ValidateProperty2();
			Assert(!filter.Property1Info.HasErrors());
			Assert(!filter.Property2Info.HasErrors());

			filter.MaxValue = 999;
			filter.MinValue = 0;
			filter.Property1 = new ZDecimal("-1000.00");
			filter.Property2 = new ZDecimal("10000.00");
			filter.Validation.ValidateProperty1();
			filter.Validation.ValidateProperty2();
			AssertHasError(filter.Property1Info, "Please enter a value greater than or equal to 0.");
			AssertHasError(filter.Property2Info, "Please enter a value less than or equal to 999.");

			AssertExceptionThrown<InvalidOperationException>(() =>
			{
				filter = new ModuleNumberRangeFilter("Moo", DummyBizoSchema.Z0_Number);
				filter.MaxValue = 999;
				filter.MinValue = 10000;
			});

			AssertExceptionThrown<InvalidOperationException>(() =>
			{
				filter = new ModuleNumberRangeFilter("Moo", DummyBizoSchema.Z0_Byte);
				filter.MaxValue = 999;
			});

			AssertExceptionThrown<InvalidOperationException>(() =>
			{
				filter = new ModuleNumberRangeFilter("Moo", DummyBizoSchema.Z0_Byte);
				filter.MinValue = -500;
			});
		}

		public void TestMaxMinValues_Delegate()
		{
			var filter1 = new ModuleNumberRangeFilter("Moo1", (value1, value2) => new ZQuery(), () => 1M, () => 2M);
			AssertEquals(1M, filter1.MinValue);
			AssertEquals(2M, filter1.MaxValue);

			var collection = new List<int> { 1, 2, 3 };
			var filter = new ModuleNumberRangeFilter("Moo", (value1, value2) => new ZQuery(), () => collection.Min(), () => collection.Max());
			AssertEquals(1M, filter.MinValue);
			AssertEquals(3M, filter.MaxValue);
			collection.AddRange(new List<int> { 4, 5, 6 });
			AssertEquals(6M, filter.MaxValue);
			collection.Remove(1);
			AssertEquals(2M, filter.MinValue);
		}

		public void TestMaxMinValues_Decimals()
		{
			var filter1 = new ModuleNumberRangeFilter("Moo1", (value1, value2) => new ZQuery(), 5, 2);
			AssertEquals(999.99m, filter1.MaxValue);
			AssertEquals(-999.99m, filter1.MinValue);

			filter1.PropertySearch = "Greater than or equal to";
			AssertEquals(0m, filter1.Property1);
			AssertEquals(filter1.MaxValue, filter1.Property2);

			filter1.PropertySearch = "Less than or equal to";
			AssertEquals(filter1.MinValue, filter1.Property1);
			AssertEquals(0m, filter1.Property2);
		}

		#endregion

		#region TestProperty1Validation

		public void TestProperty1Validation()
		{
			var errorText = "You cant hug your children with atomic arms!";
			Filter.Property1Validation = null;
			Filter.Property1 = 0m;

			Filter.Validation.ValidateProperty1();
			AssertNoError(Filter.Property1Info, errorText);

			Filter.Property1Validation = (ZPropertyInfo info) =>
		   {
			   if (info.Value.Equals(0m))
			   {
				   info.AddError(errorText);
			   }
		   };

			Filter.Validation.ValidateProperty1();
			AssertHasError(Filter.Property1Info, errorText);
		}

		#endregion

		#region TestProperty2Validation

		public void TestProperty2Validation()
		{
			var errorText = "You cant hug your children with atomic arms!";
			Filter.Property2Validation = null;
			Filter.Property2 = 0m;

			Filter.Validation.ValidateProperty2();
			AssertNoError(Filter.Property2Info, errorText);

			Filter.Property2Validation = (ZPropertyInfo info) =>
			{
				if (info.Value.Equals(0m))
				{
					info.AddError(errorText);
				}
			};

			Filter.Validation.ValidateProperty2();
			AssertHasError(Filter.Property2Info, errorText);
		}

		#endregion

		#region TestQueryIsEmptyByDefault

		public override void TestQueryIsEmptyByDefault()
		{
			AssertEquals("ModuleNumberRangeFilter.Query is never empty (as the default value of 0 has meaning).", false, Filter.IsEmpty);
		}

		#endregion

		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region TestClearInNonEnglishLanguage

		public void TestClearInNonEnglishLanguage()
		{
			using (var grmMockData = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.German).UseMockData())
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.German))
			{
				grmMockData.Put("Filter|NumberRangeSearchList|GreaterThanFilter", new ResourceStringData("Filter|NumberRangeSearchList|GreaterThanFilter", "test Greater than or equal to"));
				grmMockData.Put("Filter|NumberRangeSearchList|LessThanFilter", new ResourceStringData("Filter|NumberRangeSearchList|LessThanFilter", "test Less than or equal to"));
				grmMockData.Put("Filter|NumberRangeSearchList|BetweenFilter", new ResourceStringData("Filter|NumberRangeSearchList|BetweenFilter", "test Between"));
				grmMockData.Put("Filter|NumberRangeSearchList|EqualToFilter", new ResourceStringData("Filter|NumberRangeSearchList|EqualToFilter", "test Equal to"));

				Filter.PropertySearch = "Equal to";
				AssertEquals("Equal to", Filter.PropertySearch);
				Filter.Property1 = 3;
				AssertEquals(3m, Filter.Property1);
				AssertEquals(3m, Filter.Property1Info.Value);

				Filter.Clear();
				AssertEquals("Equal to", Filter.PropertySearch);
				AssertEquals(0m, Filter.Property1);
				AssertEquals(0m, Filter.Property1Info.Value);

				Filter.PropertySearch = "Greater than or equal to";
				AssertEquals("Greater than or equal to", Filter.PropertySearch);
				Filter.Property1 = 3;
				AssertEquals(3m, Filter.Property1);
				AssertEquals(3m, Filter.Property1Info.Value);

				Filter.Clear();
				AssertEquals("Greater than or equal to", Filter.PropertySearch);
				AssertEquals(0m, Filter.Property1);
				AssertEquals(0m, Filter.Property1Info.Value);

				Filter.PropertySearch = "Less than or equal to";
				AssertEquals("Less than or equal to", Filter.PropertySearch);
				Filter.Property2 = 3;
				AssertEquals(3m, Filter.Property2);
				AssertEquals(3m, Filter.Property2Info.Value);

				Filter.Clear();
				AssertEquals("Less than or equal to", Filter.PropertySearch);
				AssertEquals(0m, Filter.Property2);
				AssertEquals(0m, Filter.Property2Info.Value);

				Filter.PropertySearch = "Between";
				AssertEquals("Between", Filter.PropertySearch);
				Filter.Property1 = 4;
				Filter.Property2 = 3;
				AssertEquals(4m, Filter.Property1);
				AssertEquals(4m, Filter.Property1Info.Value);
				AssertEquals(3m, Filter.Property2);
				AssertEquals(3m, Filter.Property2Info.Value);

				Filter.Clear();
				AssertEquals("Between", Filter.PropertySearch);
				AssertEquals(0m, Filter.Property1);
				AssertEquals(0m, Filter.Property1Info.Value);
				AssertEquals(0m, Filter.Property2);
				AssertEquals(0m, Filter.Property2Info.Value);
			}
		}

		#endregion

		#region TestDeserializePropertiesFromXml

		public void TestDeserializePropertiesFromXml()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new DummyModuleNumberRangeFilter();

			filter.Property1 = (ZDecimal)1;
			filter.Property2 = (ZDecimal)2;

			filterStripBizO.AddModuleFilterForTest(filter);

			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			strip.FilterDescription = "";
			strip.Delete();

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleNumberRangeFilter)filterStripBizO[filter.Description];

			AssertEquals((ZDecimal)0, loadedFilter.Property1);
			AssertEquals((ZDecimal)2, loadedFilter.Property2);
		}

		#endregion

		#region TestDeserializePropertiesFromXml_GreaterThanOrEqualTo

		public void TestDeserializePropertiesFromXml_GreaterThanOrEqualTo()
		{
			var filter = new ModuleNumberRangeFilter("Filter", DummyBizoSchema.Z0_Number);
			filter.PropertySearch = "Greater than or equal to";
			filter.Property1 = 13;

			var filterStripBizO = new DummyFilterStripBusinessObject();
			filterStripBizO.AddModuleFilterForTest(filter);

			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			strip.FilterDescription = "";
			strip.Delete();

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleNumberRangeFilter)filterStripBizO[filter.Description];

			AssertEquals((ZDecimal)13, loadedFilter.Property1);
			AssertEquals((ZDecimal)int.MaxValue, loadedFilter.Property2);
			AssertEquals("Greater than or equal to", loadedFilter.PropertySearch);
		}

		#endregion

		#region TestDeserializeInvalidDataFromXml

		public void TestDeserializeInvalidDataFromXml()
		{
			var filter = new ModuleNumberRangeFilter("Filter", DummyBizoSchema.Z0_Decimal);

			using (var stringReader = new StringReader("<Property1>somethingInvalid</Property1>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				filter.DeserializeProperties(xmlReader);
				AssertEquals("Nothing should happen when deserializing invalid data", (ZDecimal)0, filter.Property1);
			}

			using (var stringReader = new StringReader("<Property2>somethingInvalid</Property2>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				filter.DeserializeProperties(xmlReader);
				AssertEquals("Nothing should happen when deserializing invalid data", (ZDecimal)0, filter.Property2);
			}
		}

		#endregion

		#region TestPropertySearch_ListContainsIsZero

		public void TestPropertySearch_ListContainsSpecifiedDateRange()
		{
			AssertEquals(true, Filter.PropertySearch_List.ContainsCode(ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo));
			AssertEquals(true, Filter.PropertySearch_List.ContainsCode(ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo));
			AssertEquals(true, Filter.PropertySearch_List.ContainsCode(ModuleNumberRangeFilter.SearchTexts.Between));
			AssertEquals(true, Filter.PropertySearch_List.ContainsCode(ModuleNumberRangeFilter.SearchTexts.EqualTo));
			AssertEquals(CountOfPropertySearch_List, Filter.PropertySearch_List.Count);
		}

		protected virtual int CountOfPropertySearch_List => 4;

		#endregion

		#region TestInvalidPropertySearchThrowInvalidOperationException

		public void TestInvalidPropertySearchThrowInvalidOperationException()
		{
			var filter = new ModuleNumberRangeFilter("Filter", DummyBizoSchema.Z0_Number);
			var filterStripBizO = new DummyFilterStripBusinessObject();
			filterStripBizO.AddModuleFilterForTest(filter);

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleNumberRangeFilter)filterStripBizO[filter.Description];
			loadedFilter.DefaultPropertySearch = new ZString(ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo.GetUnresolvedString());
			loadedFilter.DefaultPropertySearch = new ZString(ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo.GetUnresolvedString());
			loadedFilter.DefaultPropertySearch = new ZString(ModuleNumberRangeFilter.SearchTexts.EqualTo.GetUnresolvedString());
			loadedFilter.DefaultPropertySearch = new ZString(ModuleNumberRangeFilter.SearchTexts.Between.GetUnresolvedString());
			AssertExceptionThrown<InvalidOperationException>
			(
				string.Format
					(CultureInfo.InvariantCulture, "Set DefaultPropertySearch with an invalid value: value = {0}. Valid values include: Greater than or equal to, Less than or equal to, Equal to, Between.",
					new ZString("TestString")
					),
				() => { loadedFilter.DefaultPropertySearch = new ZString("TestString"); }
			);
		}

		#endregion

		#region TestPropertiesIfSetToGreaterThanOrEqualTo

		public void TestPropertiesIfSetToGreaterThanOrEqualTo()
		{
			var filter = new ModuleNumberRangeFilter("Filter", DummyBizoSchema.Z0_Number);
			var filterStripBizO = new DummyFilterStripBusinessObject();
			filterStripBizO.AddModuleFilterForTest(filter);

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleNumberRangeFilter)filterStripBizO[filter.Description];
			AssertNotEquals("PreCondition", new ZString(ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo.GetUnresolvedString()), loadedFilter.PropertySearch);
			AssertNotEquals("PreCondition", 3m, loadedFilter.Property1);
			AssertNotEquals("PreCondition", (ZDecimal)int.MaxValue, loadedFilter.Property2);

			filter.PropertySearch = new ZString(ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo.GetUnresolvedString());
			filter.GreaterThanOrEqualToDefaultProperty = 3;

			AssertEquals(new ZString(ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo.GetUnresolvedString()), loadedFilter.PropertySearch);
			AssertEquals(3m, loadedFilter.Property1);
			AssertEquals((ZDecimal)int.MaxValue, loadedFilter.Property2);
		}

		#endregion

		#region TestPropertiesIfSetToLessThanOrEqualTo

		public void TestPropertiesIfSetToLessThanOrEqualTo()
		{
			var filter = new ModuleNumberRangeFilter("Filter", DummyBizoSchema.Z0_Number);
			var filterStripBizO = new DummyFilterStripBusinessObject();
			filterStripBizO.AddModuleFilterForTest(filter);

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleNumberRangeFilter)filterStripBizO[filter.Description];

			AssertNotEquals("PreCondition", new ZString(ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo.GetUnresolvedString()), loadedFilter.PropertySearch);
			AssertNotEquals("PreCondition", (ZDecimal)int.MinValue, loadedFilter.Property1);
			AssertNotEquals("PreCondition", 6m, loadedFilter.Property2);

			filter.PropertySearch = new ZString(ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo.GetUnresolvedString());
			filter.LessThanOrEqualToDefaultProperty = 6;

			AssertEquals(new ZString(ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo.GetUnresolvedString()), loadedFilter.PropertySearch);
			AssertEquals((ZDecimal)int.MinValue, loadedFilter.Property1);
			AssertEquals(6m, loadedFilter.Property2);
		}

		#endregion

		#region TestPropertiesIfSetToEqualTo

		public void TestPropertiesIfSetToEqualTo()
		{
			var filter = new ModuleNumberRangeFilter("Filter", DummyBizoSchema.Z0_Number);
			var filterStripBizO = new DummyFilterStripBusinessObject();
			filterStripBizO.AddModuleFilterForTest(filter);

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleNumberRangeFilter)filterStripBizO[filter.Description];

			AssertNotEquals("PreCondition", new ZString(ModuleNumberRangeFilter.SearchTexts.EqualTo.GetUnresolvedString()), loadedFilter.PropertySearch);
			AssertNotEquals("PreCondition", 2m, loadedFilter.Property1);
			AssertNotEquals("PreCondition", 2m, loadedFilter.Property2);

			filter.PropertySearch = new ZString(ModuleNumberRangeFilter.SearchTexts.EqualTo.GetUnresolvedString());
			filter.EqualToDefaultProperty = 2;

			AssertEquals(new ZString(ModuleNumberRangeFilter.SearchTexts.EqualTo.GetUnresolvedString()), loadedFilter.PropertySearch);
			AssertEquals(2m, loadedFilter.Property1);
			AssertEquals(2m, loadedFilter.Property2);
		}

		#endregion

		#region TestPropertiesIfSetToBetween

		public void TestPropertiesIfSetToBetween()
		{
			var filter = new ModuleNumberRangeFilter("Filter", DummyBizoSchema.Z0_Number);
			var filterStripBizO = new DummyFilterStripBusinessObject();
			filterStripBizO.AddModuleFilterForTest(filter);

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleNumberRangeFilter)filterStripBizO[filter.Description];

			filter.PropertySearch = new ZString(ModuleNumberRangeFilter.SearchTexts.EqualTo.GetUnresolvedString());

			AssertNotEquals("PreCondition", new ZString(ModuleNumberRangeFilter.SearchTexts.Between.GetUnresolvedString()), loadedFilter.PropertySearch);
			AssertNotEquals("PreCondition", 8m, loadedFilter.Property1);
			AssertNotEquals("PreCondition", 9m, loadedFilter.Property2);

			filter.PropertySearch = new ZString(ModuleNumberRangeFilter.SearchTexts.Between.GetUnresolvedString());
			filter.BetweenDefaultProperty1 = 8;
			filter.BetweenDefaultProperty2 = 9;

			AssertEquals(new ZString(ModuleNumberRangeFilter.SearchTexts.Between.GetUnresolvedString()), loadedFilter.PropertySearch);
			AssertEquals(8m, loadedFilter.Property1);
			AssertEquals(9m, loadedFilter.Property2);
		}

		#endregion

		#region TestClearResetsToDefaults

		public void TestClearResetsToDefaults()
		{
			var filter = new ModuleNumberRangeFilter("Filter", DummyBizoSchema.Z0_Number);
			filter.GreaterThanOrEqualToDefaultProperty = 5;
			filter.LessThanOrEqualToDefaultProperty = 9;
			filter.EqualToDefaultProperty = 3;
			filter.BetweenDefaultProperty1 = 2;
			filter.BetweenDefaultProperty2 = 7;

			var filterStripBizO = new DummyFilterStripBusinessObject();
			filterStripBizO.AddModuleFilterForTest(filter);

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleNumberRangeFilter)filterStripBizO[filter.Description];

			loadedFilter.Clear();
			AssertEquals("PreCondition", loadedFilter.PropertySearch, new ZString(ModuleNumberRangeFilter.SearchTexts.Between.GetUnresolvedString()));
			AssertEquals("PreCondition", 2m, loadedFilter.Property1);
			AssertEquals("PreCondition", 7m, loadedFilter.Property2);

			loadedFilter.DefaultPropertySearch = new ZString(ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo.GetUnresolvedString());
			loadedFilter.Clear();
			AssertEquals(loadedFilter.PropertySearch, new ZString(ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo.GetUnresolvedString()));
			AssertEquals(5m, loadedFilter.Property1);
			AssertEquals((ZDecimal)int.MaxValue, loadedFilter.Property2);

			loadedFilter.DefaultPropertySearch = new ZString(ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo.GetUnresolvedString());
			loadedFilter.Clear();
			AssertEquals(loadedFilter.PropertySearch, new ZString(ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo.GetUnresolvedString()));
			AssertEquals((ZDecimal)int.MinValue, loadedFilter.Property1);
			AssertEquals(9m, loadedFilter.Property2);

			loadedFilter.MinValue = 1m;
			loadedFilter.DefaultPropertySearch = new ZString(ModuleNumberRangeFilter.SearchTexts.EqualTo.GetUnresolvedString());
			loadedFilter.Clear();
			AssertEquals(loadedFilter.PropertySearch, new ZString(ModuleNumberRangeFilter.SearchTexts.EqualTo.GetUnresolvedString()));
			AssertEquals(3m, loadedFilter.Property1);
			AssertEquals(3m, loadedFilter.Property2);

			loadedFilter.DefaultPropertySearch = new ZString(ModuleNumberRangeFilter.SearchTexts.Between.GetUnresolvedString());
			loadedFilter.Clear();
			AssertEquals(loadedFilter.PropertySearch, new ZString(ModuleNumberRangeFilter.SearchTexts.Between.GetUnresolvedString()));
			AssertEquals(2m, loadedFilter.Property1);
			AssertEquals(7m, loadedFilter.Property2);
		}

		#endregion

		#region TestInvalidOperations

		public void TestInvalidOperations()
		{
			AssertExceptionThrown<InvalidOperationException>(() =>
			{
				Filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo;
				Filter.Property2 = 3.0;
			});

			AssertExceptionThrown<InvalidOperationException>(() =>
			{
				Filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo;
				Filter.Property1 = 3.0;
			});
		}

		#endregion

		#region TestShowUpAndDownArrows

		public void TestShowUpAndDownArrows()
		{
			var filter = new ModuleNumberRangeFilter("moo", DummyBizoSchema.Z0_Byte);
			AssertEquals(false, filter.ShowUpAndDownArrows);
			AssertEquals(0M, filter.Property1);
			filter.ClickUpArrow();
			AssertEquals(1M, filter.Property1);
			filter.ClickDownArrow();
			AssertEquals(0M, filter.Property1);
		}

		#endregion

		#region TestQuery

		public void TestQueryWithByteType()
		{
			Dummy1.Z0_Byte = 0;
			Dummy2.Z0_Byte = 10;
			var filter = new ModuleNumberRangeFilter("moo", DummyBizoSchema.Z0_Byte);

			DoTest(filter, 3, 15);
		}

		public void TestQueryWithShortType()
		{
			Dummy1.Z0_Short = 0;
			Dummy2.Z0_Short = 10;
			var filter = new ModuleNumberRangeFilter("moo", DummyBizoSchema.Z0_Short);

			DoTest(filter, 3, 15);
		}

		public void TestQueryWithNumberType()
		{
			Dummy1.Z0_Number = 0;
			Dummy2.Z0_Number = 10;
			var filter = new ModuleNumberRangeFilter("moo", DummyBizoSchema.Z0_Number);

			DoTest(filter, 3, 15);
		}

		public void TestQueryWithDecimalType()
		{
			Dummy1.Z0_Decimal = 0;
			Dummy2.Z0_Decimal = 10;
			var filter = new ModuleNumberRangeFilter("moo", DummyBizoSchema.Z0_Decimal);

			DoTest(filter, 3, 15);
		}

		public void TestQueryWithLongType()
		{
			Dummy1.Z0_Long = 0;
			Dummy2.Z0_Long = 10;

			var filter = new ModuleNumberRangeFilter("moo", DummyBizoSchema.Z0_Long);
			DoTest(filter, 3, 15);
		}

		void DoTest(ModuleNumberRangeFilter filter, int fromValue, int toValue)
		{
			Factory.Save();
			var dummies = new DummyBusinessObjectCollection(Factory);

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo;
			filter.Property1 = fromValue;
			dummies.Load(filter.Query);
			AssertCollectionNotContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo;
			filter.Property2 = fromValue;
			dummies.Load(filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
			filter.Property1 = fromValue;
			dummies.Load(filter.Query);
			AssertCollectionNotContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.Between;
			filter.Property1 = fromValue;
			filter.Property2 = toValue;
			dummies.Load(filter.Query);
			AssertCollectionNotContains(Dummy1, dummies);
			AssertCollectionContains(Dummy2, dummies);
		}

		#endregion

		#region Implementation

		protected override ModuleNumberRangeFilter GetNewModuleFilter()
		{
			return new ModuleNumberRangeFilter("moo", DummyBizoSchema.Z0_Number);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.NumbersAndReferences; }
		}

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(ModuleNumberRangeFilter filter)
		{
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { nameof(filter.MinValue), nameof(filter.MaxValue), nameof(filter.DefaultPropertySearch), nameof(filter.GreaterThanOrEqualToDefaultProperty), nameof(filter.LessThanOrEqualToDefaultProperty), nameof(filter.EqualToDefaultProperty), nameof(filter.BetweenDefaultProperty1), nameof(filter.BetweenDefaultProperty2) }).ToArray();
		}

		protected override Dictionary<string, IZType> GetDummyValuesForCacheInvalidationTest(ModuleNumberRangeFilter filter)
		{
			var values = base.GetDummyValuesForCacheInvalidationTest(filter);
			values.Add(nameof(filter.Property1), new ZDecimal(10m));
			values.Add(nameof(filter.Property2), new ZDecimal(20m));

			return values;
		}

		protected override void SetUp()
		{
			base.SetUp();

			Dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			Dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
		}

		DummyBusinessObject Dummy1;
		DummyBusinessObject Dummy2;

		#endregion

		#region DummyModuleNumberRangeFilter

		public class DummyModuleNumberRangeFilter : ModuleNumberRangeFilter
		{
			public DummyModuleNumberRangeFilter()
				: base("DummyNumberRangeFilter", DummyBizoSchema.Z0_Number)
			{
			}

			public DummyModuleNumberRangeFilter(string description, SchemaNumericColumn filterColumn)
				: base(description, filterColumn)
			{
			}

			protected override void SerializePropertiesToXml(XmlWriter writer)
			{
				if (ShouldOverrideSerializeFunction)
				{
					writer.WriteElementString("Property2", Property2.ToString());
				}
				else
				{
					base.SerializePropertiesToXml(writer);
				}
			}

			internal bool ShouldOverrideSerializeFunction = true;

			internal void SetOldVersionValueToProperty(string propertySearch, ZDecimal? value1, ZDecimal? value2)
			{
				PropertySearch = propertySearch;
				if (value1.HasValue)
				{
					Property1_Exposed = value1.Value;
				}
				if (value2.HasValue)
				{
					Property2_Exposed = value2.Value;
				}
			}
		}

		#endregion
	}
}
