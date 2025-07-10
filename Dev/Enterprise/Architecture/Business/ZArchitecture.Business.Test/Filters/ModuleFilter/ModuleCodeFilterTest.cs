using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class ModuleCodeFilterTest : ModuleFilterTestCase<ModuleCodeFilter>
	{
		#region TestProperty1Validation

		public void TestProperty1Validation()
		{
			var errorText = "You cant hug your children with atomic arms!";
			Filter.Property1Validation = null;
			Filter.Property1 = ZString.Empty;

			Filter.Validation.ValidateProperty1();
			AssertNoError(Filter.Property1Info, errorText);

			Filter.Property1Validation = delegate(ZPropertyInfo info)
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
			AssertNoError(Filter.Property2Info, errorText);

			Filter.Property2Validation = delegate(ZPropertyInfo info)
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

		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region TestClearResetsToDefaults

		public void TestClearResetsToDefaults()
		{
			ZString defaultValue1 = "foo";
			ZString defaultValue2 = "bar";

			AssertEquals("Precondition", "", Filter.Property1);
			AssertEquals("Precondition", "", Filter.Property2);
			AssertEquals("Precondition", "", Filter.DefaultProperty1);
			AssertEquals("Precondition", "", Filter.DefaultProperty2);

			Filter.DefaultProperty1 = defaultValue1;
			Filter.DefaultProperty2 = defaultValue2;
			AssertEquals(defaultValue1, Filter.Property1);
			AssertEquals(defaultValue2, Filter.Property2);

			Filter.Property1 = "";
			Filter.Property2 = "";
			AssertEquals("Precondition", "", Filter.Property1);
			AssertEquals("Precondition", "", Filter.Property2);

			Filter.Clear();
			AssertEquals(defaultValue1, Filter.Property1);
			AssertEquals(defaultValue2, Filter.Property2);
			AssertEquals(defaultValue1, Filter.DefaultProperty1);
			AssertEquals(defaultValue2, Filter.DefaultProperty2);

			// test Clear when ReadOnly
			Filter.Property1 = "oink";
			Filter.Property2 = "moo";
			Filter.ReadOnly = true;
			Filter.Clear();
			AssertEquals("Filter was ReadOnly thus Clear() should have no effect.", "oink", Filter.Property1);
			AssertEquals("Filter was ReadOnly thus Clear() should have no effect.", "moo", Filter.Property2);
		}

		#endregion

		#region TestDeserializePropertiesFromXml

		public void TestDeserializePropertiesFromXml()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new DummyModuleCodeFilter((ZString)"moo", DummyBizoSchema.Z0_Description, new StmNoteNonDependentCollection(Factory), DummyBizoSchema.Z0_Code, new StmNoteNonDependentCollection(Factory));

			filter.Property1 = "someValue";
			filter.Property2 = "anotherValue";

			filterStripBizO.AddModuleFilterForTest(filter);

			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			strip.FilterDescription = "";
			strip.Delete();

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleCodeFilter)filterStripBizO[filter.Description];

			AssertEquals(ZString.Empty, loadedFilter.Property1);
			AssertEquals("anotherValue", loadedFilter.Property2);
		}

		#endregion

		protected override ModuleCodeFilter GetNewModuleFilter()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			return new DummyModuleCodeFilter("moo", DummyBizoSchema.Z0_Description, list, DummyBizoSchema.Z0_Code, list);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(ModuleCodeFilter filter)
		{
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { nameof(filter.DefaultProperty1), nameof(filter.DefaultProperty2) }).ToArray();
		}

		#region DummyModuleCodeFilter

		public class DummyModuleCodeFilter : ModuleCodeFilter
		{
			protected DummyModuleCodeFilter(FilterCategory category, ModuleFilterCollection parentCollection)
				: base(category, parentCollection)
			{
			}

			public DummyModuleCodeFilter(ZString description, SchemaStringColumn filterColumn1, IBusinessObjectCollection list1, SchemaStringColumn filterColumn2, IBusinessObjectCollection list2)
				: base(description, filterColumn1, list1, filterColumn2, list2)
			{
			}

			protected override FilterCategory DefaultCategory
			{
				get { return FilterCategories.Other; }
			}

			#region GetNewCommonModuleFilter, CopyPropertiesToFilter

			protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			{
				return new DummyModuleCodeFilter(category, parentCollection);
			}

			protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
			{
				var filter = (DummyModuleCodeFilter)filterToCopyFrom;
				Property1 = filter.Property1;
				Property2 = filter.Property2;
			}

			#endregion

			public override bool IsExpensiveQuery
			{
				get { return false; }
			}

			protected override void SerializePropertiesToXml(XmlWriter writer)
			{
				writer.WriteElementString("Property2", Property2);
			}
		}

		#endregion

	}
}
