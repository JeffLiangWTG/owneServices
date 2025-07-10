using System.Xml;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
//using Modules;
//using static ModuleFilterWithListAndComparisonOperators<ZString>;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleTextRangeFilter))]
	sealed class ModuleTextRangeFilterTest : ModuleFilterTestCase<ModuleTextRangeFilter>
	{
		#region Trimming properties

		public void TestTrimmingProperties()
		{
			Filter.Property1 = "HELLO ";
			Assert(Filter.Property1.EndsWith("HELLO"));

			Filter.Property1 = "AUFWIEDERSEHEN  ";
			Assert(Filter.Property1.EndsWith("AUFWIEDERSEHEN"));
		}

		#endregion

		#region TestProperty1Validation

		public void TestProperty1Validation()
		{
			var errorText = "You cant hug your children with atomic arms!";
			Filter.Property1Validation = null;
			Filter.Property1 = ZString.Empty;

			Filter.Validation.ValidateProperty1();
			AssertNoError(Filter.Property1Info, errorText);

			Filter.Property1Validation = info =>
			{
				if (info.Value.IsEmpty)
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
			Filter.Property2 = ZString.Empty;

			Filter.Validation.ValidateProperty2();
			AssertNoError(Filter.Property1Info, errorText);

			Filter.Property2Validation = info =>
			{
				if (info.Value.IsEmpty)
				{
					info.AddError(errorText);
				}
			};

			Filter.Validation.ValidateProperty2();
			AssertHasError(Filter.Property2Info, errorText);
		}

		#endregion

		#region TestBothPropertiesValidation

		public void TestBothPropertiesValidation()
		{
			AssertNoNotifications(Filter);
			Filter.Property1 = "ABC";
			Filter.Property2 = "ABC";

			AssertNoNotifications("Should not have notifications", Filter);
			Filter.Property2 = "ABD";
			AssertNoNotifications("Should not have notifications", Filter);
			Filter.Property2 = "ABB";
			AssertHasError("Should have error on property one validation", Filter.Property1Info, "The 'From' value is greater than the 'To' value.");
			AssertHasError("Should have error on property two validation", Filter.Property2Info, "The 'To' value is less than the 'From' value.");
		}

		#endregion

		#region TestQueryIsEmptyByDefault

		public override void TestQueryIsEmptyByDefault()
		{
			AssertEquals("ModuleTextRangeFilter.Query is empty by default", true, Filter.IsEmpty);
		}

		#endregion

		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region TestClearResetsToDefaults

		public void TestClearResetsToDefaults()
		{
			ZString defaultValue1 = "defautFrom";
			ZString defaultValue2 = "defaultTo";

			AssertEquals("Precondition", ZString.Empty, Filter.Property1);
			AssertEquals("Precondition", ZString.Empty, Filter.Property2);
			AssertEquals("Precondition", ZString.Empty, Filter.DefaultProperty1);
			AssertEquals("Precondition", ZString.Empty, Filter.DefaultProperty2);

			Filter.DefaultProperty1 = defaultValue1;
			Filter.DefaultProperty2 = defaultValue2;
			AssertEquals(defaultValue1, Filter.Property1);
			AssertEquals(defaultValue2, Filter.Property2);

			Filter.Property1 = "NonDefaultProperty1";
			Filter.Property2 = "NonDefaultProperty2";
			AssertEquals("Precondition", "NonDefaultProperty1", Filter.Property1);
			AssertEquals("Precondition", "NonDefaultProperty2", Filter.Property2);

			Filter.Clear();
			AssertEquals(defaultValue1, Filter.Property1);
			AssertEquals(defaultValue2, Filter.Property2);
			AssertEquals(defaultValue1, Filter.DefaultProperty1);
			AssertEquals(defaultValue2, Filter.DefaultProperty2);
		}

		#endregion

		#region TestDeserializePropertiesFromXml

		public void TestDeserializePropertiesFromXml()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new DummyModuleTextRangeFilter();

			filter.Property1 = "value1";
			filter.Property2 = "value2";

			filterStripBizO.AddModuleFilterForTest(filter);

			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			strip.FilterDescription = "";
			strip.Delete();

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleTextRangeFilter)filterStripBizO[filter.Description];

			AssertEquals(ZString.Empty, loadedFilter.Property1);
			AssertEquals("value2", loadedFilter.Property2);
		}

		#endregion

		protected override ModuleTextRangeFilter GetNewModuleFilter()
		{
			return new ModuleTextRangeFilter("moo", DummyBizoSchema.Z0_Description);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		#region DummyModuleTextRangeFilter

		class DummyModuleTextRangeFilter : ModuleTextRangeFilter
		{
			public DummyModuleTextRangeFilter()
				: base("DummyTextRangeFilter", DummyBizoSchema.Z0_Description)
			{
			}

			protected override void SerializePropertiesToXml(XmlWriter writer)
			{
				writer.WriteElementString("Property2", Property2);
			}
		}

		#endregion
	}
}
