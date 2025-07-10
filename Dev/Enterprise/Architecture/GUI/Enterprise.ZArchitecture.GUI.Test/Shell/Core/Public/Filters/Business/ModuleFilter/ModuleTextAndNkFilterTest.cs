using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleTextAndNkFilter))]
	public class ModuleTextAndNkFilterTest : ModuleFilterWithSelectedFiltersTestCase<ModuleTextAndNkFilter, ZString>
	{
		public void TestNKProperty()
		{
			Filter.NkProperty = " Hello ";
			AssertEquals(" Hello", Filter.NkProperty);
		}

		#region TestPropertyValidation

		public void TestPropertyValidation()
		{
			var errorText = "You cant hug your children with atomic arms!";
			Filter.PropertyValidation = null;
			Filter.Property = ZString.Empty;

			Filter.Validation.ValidateProperty();
			AssertNoError(Filter.PropertyInfo, errorText);

			Filter.PropertyValidation = delegate(ZPropertyInfo info)
			{
				if (info.Value.IsEmpty)
				{
					info.AddError(errorText);
				}
			};

			Filter.Validation.ValidateProperty();
			AssertHasError(Filter.PropertyInfo, errorText);
		}

		#endregion

		#region TestNkPropertyValidation

		public void TestNkPropertyValidation()
		{
			var errorText = "You cant hug your children with atomic arms!";
			Filter.NkPropertyValidation = null;
			Filter.NkProperty = ZString.Empty;

			Filter.Validation.ValidateNkProperty();
			AssertNoError(Filter.NkPropertyInfo, errorText);

			Filter.NkPropertyValidation = delegate(ZPropertyInfo info)
			{
				if (info.Value.IsEmpty)
				{
					info.AddError(errorText);
				}
			};

			Filter.Validation.ValidateNkProperty();
			AssertHasError(Filter.NkPropertyInfo, errorText);
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
			ZString defaultValue = "360";
			ZString defaultNkValue = "wii";

			AssertEquals("Precondition", "", Filter.Property);
			AssertEquals("Precondition", "", Filter.NkProperty);
			AssertEquals("Precondition", "", Filter.DefaultProperty);
			AssertEquals("Precondition", "", Filter.DefaultNkProperty);

			Filter.DefaultProperty = defaultValue;
			Filter.DefaultNkProperty = defaultNkValue;
			AssertEquals(defaultValue, Filter.Property);
			AssertEquals(defaultNkValue, Filter.NkProperty);

			Filter.Property = "";
			Filter.NkProperty = "";
			AssertEquals("Precondition", "", Filter.Property);
			AssertEquals("Precondition", "", Filter.NkProperty);

			Filter.Clear();
			AssertEquals(defaultValue, Filter.Property);
			AssertEquals(defaultNkValue, Filter.NkProperty);
			AssertEquals(defaultValue, Filter.DefaultProperty);
			AssertEquals(defaultNkValue, Filter.DefaultNkProperty);

			// test Clear when ReadOnly
			Filter.Property = "moo";
			Filter.NkProperty = "oink";
			Filter.ReadOnly = true;
			Filter.Clear();
			AssertEquals("Filter was ReadOnly thus Clear() should have no effect.", "moo", Filter.Property);
			AssertEquals("Filter was ReadOnly thus Clear() should have no effect.", "oink", Filter.NkProperty);
		}

		#endregion

		#region TestDeserializePropertiesFromXml

		public void TestDeserializePropertiesFromXml()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new DummyModuleTextAndNkFilter(Factory);

			filter.NkProperty = "someNkValue";

			filterStripBizO.AddModuleFilterForTest(filter);

			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			strip.FilterDescription = "";
			strip.Delete();

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleTextAndNkFilter)filterStripBizO[filter.Description];

			AssertEquals(ZString.Empty, loadedFilter.NkProperty);
		}

		#endregion

		#region TestSetValueFromInitialCode

		public void TestSetValueFromInitialCode()
		{
			var filter = new ModuleTextAndNkFilter("Filter", DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_VarCharMax, DummyModuleIDs.Dummy, new DummyBusinessObjectCollection(Factory));

			filter.Property = "ABC";
			filter.NkProperty = "DEF";
			AssertEquals("Initial code should not be set", false, filter.SetValueFromInitialCode("Z0_Code", "XYZ"));
			AssertEquals("ABC", filter.Property);
			AssertEquals("DEF", filter.NkProperty);

			AssertEquals("Initial code should not be set", false, filter.SetValueFromInitialCode("Z0_Code", "A:XYZ"));
			AssertEquals("ABC", filter.Property);
			AssertEquals("DEF", filter.NkProperty);

			filter.Prefix = "B";
			AssertEquals("Initial code should not be set", false, filter.SetValueFromInitialCode("Z0_Code", "A:XYZ"));
			AssertEquals("ABC", filter.Property);
			AssertEquals("DEF", filter.NkProperty);

			filter.Prefix = "A";
			AssertEquals("Initial code should be set", true, filter.SetValueFromInitialCode("Z0_Code", "A:XYZ"));
			AssertEquals("XYZ", filter.Property);
			AssertEquals("DEF", filter.NkProperty);

			filter.Property = "ABC";
			filter.NkProperty = "DEF";
			AssertEquals("Initial code should be set", true, filter.SetValueFromInitialCode("Whatever", "A:XYZ/UVW"));
			AssertEquals("XYZ", filter.Property);
			AssertEquals("UVW", filter.NkProperty);
		}

		public void TestShouldSetValueFromInitialCode()
		{
			var filter = new ModuleTextAndNkFilter("Filter", DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_VarCharMax, DummyModuleIDs.Dummy, new DummyBusinessObjectCollection(Factory));

			filter.Property = "ABC";
			filter.NkProperty = "DEF";
			AssertEquals("Initial code should not be set", false, filter.ShouldSetValueFromInitialCode("Z0_Code", "XYZ"));

			AssertEquals("Initial code should not be set", false, filter.ShouldSetValueFromInitialCode("Z0_Code", "A:XYZ"));

			filter.Prefix = "B";
			AssertEquals("Initial code should not be set", false, filter.ShouldSetValueFromInitialCode("Z0_Code", "A:XYZ"));

			filter.Prefix = "A";
			AssertEquals("Initial code should be set", true, filter.ShouldSetValueFromInitialCode("Z0_Code", "A:XYZ"));
		}

		public void TestGetFormattedInitialCode_Prefix_NoNKProperty()
		{
			var filter = new ModuleTextAndNkFilter("Filter", DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_VarCharMax, DummyModuleIDs.Dummy, new DummyBusinessObjectCollection(Factory));
			filter.Property = "ABC";
			filter.Prefix = "A";

			var expectedInitialCode = filter.Prefix + ":" + filter.Property;
			AssertEquals("Initial code should be the prefix and the property value", expectedInitialCode, filter.GetFormattedInitialCode_Prefix());
		}

		public void TestGetFormattedInitialCode_Prefix_NKPropertySet()
		{
			var filter = new ModuleTextAndNkFilter("Filter", DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_VarCharMax, DummyModuleIDs.Dummy, new DummyBusinessObjectCollection(Factory));
			filter.Property = "ABC";
			filter.Prefix = "A";
			filter.NkProperty = "DEF";

			var expectedInitialCode = filter.Prefix + ":" + filter.Property + "/" + filter.NkProperty;
			AssertEquals("Initial code should be the prefix and the property value", expectedInitialCode, filter.GetFormattedInitialCode_Prefix());
		}

		#endregion

		#region Implementation

		protected override ModuleTextAndNkFilter GetNewModuleFilter()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			return new ModuleTextAndNkFilter("moo", DummyBizoSchema.Z0_Description, DummyBizoSchema.Z0_Code, ModuleIDs.SalesEnquiry, list);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		protected override string FilterDescriptionForFiltersMatchTest => "DummyTextAndNk";

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(ModuleTextAndNkFilter filter)
		{
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { nameof(filter.DefaultNkProperty) }).ToArray();
		}

		#endregion

		#region DummyModuleTextAndNkFilter

		public class DummyModuleTextAndNkFilter : ModuleTextAndNkFilter
		{
			public DummyModuleTextAndNkFilter(BusinessObjectFactory factory)
				: base("DummyTextAndNkFilter", DummyBizoSchema.Z0_Description, DummyBizoSchema.Z0_Code, ModuleIDs.Organisation, new StmNoteNonDependentCollection(factory))
			{
			}

			protected override void SerializePropertiesToXml(XmlWriter writer)
			{
			}
		}

		#endregion
	}
}
