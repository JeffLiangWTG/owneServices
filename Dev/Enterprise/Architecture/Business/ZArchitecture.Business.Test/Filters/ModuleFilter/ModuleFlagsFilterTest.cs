using System;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class ModuleFlagsFilterTest : ModuleFilterTestCase<ModuleFlagsFilter>
	{
		#region TestWhenMutuallyExclusive

		public void TestWhenMutuallyExclusive()
		{
			var filter = new ModuleFlagsFilter((ZString)"moo", new string[] { "FlagA", "FlagB" }, new GetFlagsQuery[] { delegate { return new ZQuery(); }, delegate { return new ZQuery(); } });
			filter.ArePropertiesMutuallyExclusive = true;

			filter.Property0 = true;
			AssertEquals(true, filter.Property0);
			AssertEquals(false, filter.Property1);

			filter.Property1 = true;
			AssertEquals("Property0 is off automatically", false, filter.Property0);
			AssertEquals(true, filter.Property1);
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
			var defaultValue = true;

			AssertEquals("Precondition", false, Filter["Flag"]);
			AssertEquals("Precondition", false, Filter.DefaultProperties["Flag"]);

			Filter.DefaultProperties["Flag"] = defaultValue;
			AssertEquals(defaultValue, Filter["Flag"]);

			Filter["Flag"] = false;
			AssertEquals("Precondition", false, Filter["Flag"]);

			Filter.Clear();
			AssertEquals(defaultValue, Filter["Flag"]);
			AssertEquals(defaultValue, Filter.DefaultProperties["Flag"]);

			// test Clear when ReadOnly
			Filter["Flag"] = false;
			Filter.ReadOnly = true;
			Filter.Clear();
			AssertEquals("Filter was ReadOnly thus Clear() should have no effect.", false, Filter["Flag"]);
		}

		#endregion

		#region ShouldCheck

		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestShouldCheckMaximumFlags()
		{
			new ModuleFlagsFilter("moo", new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11" }, new GetFlagsQuery[] { delegate { return new ZQuery(); } });
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestShouldCheckFlagAmountEqualsDelegateAmount()
		{
			new ModuleFlagsFilter("moo", new string[] { "1", "2" }, new GetFlagsQuery[] { delegate { return new ZQuery(); } });
		}

		#endregion

		#region TestDeserializePropertiesFromXml

		public void TestDeserializePropertiesFromXml()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new DummyModuleFlagsFilter();

			filter.Property0 = ZBool.True;
			filter.Property1 = ZBool.True;

			filterStripBizO.AddModuleFilterForTest(filter);

			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			strip.FilterDescription = "";
			strip.Delete();

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleFlagsFilter)filterStripBizO[filter.Description];

			AssertEquals(ZBool.False, loadedFilter.Property0);
			AssertEquals(ZBool.True, loadedFilter.Property1);
		}

		#endregion

		#region TestJoinConditionForQueryDelegates

		public void TestJoinConditionForQueryDelegates()
		{
			var andFlags = new ModuleFlagsFilter("and",
				new string[] { "Flag1", "Flag2" },
				new GetFlagsQuery[] { delegate { return new ZQuery(DummyBizoSchema.Z0_Bool, true); }, delegate { return new ZQuery(DummyBizoSchema.Z0_Bool, false); } });
			andFlags["Flag1"] = true;
			andFlags["Flag2"] = true;
			var expectedAndQuery = new ZQuery(new ZQuery(DummyBizoSchema.Z0_Bool, true), new ZQuery(DummyBizoSchema.Z0_Bool, false));
			AssertEquals(expectedAndQuery, andFlags.Query);

			var orFlags = new ModuleFlagsFilter("and",
				new string[] { "Flag1", "Flag2" },
				new GetFlagsQuery[] { delegate { return new ZQuery(DummyBizoSchema.Z0_Bool, true); }, delegate { return new ZQuery(DummyBizoSchema.Z0_Bool, false); } },
				JoinCondition.Or);

			orFlags["Flag1"] = true;
			orFlags["Flag2"] = true;
			var expectedOrQuery = new ZQuery(new ZQuery(DummyBizoSchema.Z0_Bool, true), JoinCondition.Or, new ZQuery(DummyBizoSchema.Z0_Bool, false));
			AssertEquals(expectedOrQuery, orFlags.Query);
		}

		#endregion

		#region Add/Or Condition

		public void TestAddOrConditionDefaultValue()
		{
			Assert(!Filter.ShowAddOrRadioBox);
			Assert(Filter.AndJoinCondition);
			Assert(!Filter.OrJoinCondition);
		}

		public void TestAddOrCondition()
		{
			var filter = new ModuleFlagsFilter("filter",
				new string[] { "Flag1", "Flag2" },
				new SchemaBoolColumn[] { DummyBizoSchema.Z0_Bool, DummyBizoSchema.Z0_Bool });
			filter["Flag1"] = true;
			filter["Flag2"] = true;
			filter.ShowAddOrRadioBox = false;
			var expectedOrQuery = new ZQuery(new ZQuery(DummyBizoSchema.Z0_Bool, true), JoinCondition.And, new ZQuery(DummyBizoSchema.Z0_Bool, true));
			AssertEquals(expectedOrQuery.LiteralTextADO, filter.Query.LiteralTextADO);

			filter.ShowAddOrRadioBox = true;
			filter.AndJoinCondition = true;
			filter.OrJoinCondition = false;
			expectedOrQuery = new ZQuery(new ZQuery(DummyBizoSchema.Z0_Bool, true), JoinCondition.And, new ZQuery(DummyBizoSchema.Z0_Bool, true));
			AssertEquals(expectedOrQuery.LiteralTextADO, filter.Query.LiteralTextADO);

			filter.AndJoinCondition = false;
			filter.OrJoinCondition = true;
			expectedOrQuery = new ZQuery(new ZQuery(DummyBizoSchema.Z0_Bool, true), JoinCondition.Or, new ZQuery(DummyBizoSchema.Z0_Bool, true));
			AssertEquals(expectedOrQuery.LiteralTextADO, filter.Query.LiteralTextADO);
		}

		#endregion

		public void TestPropertyValidation()
		{
			var testFlags = new ModuleFlagsFilter("test",
				new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" },
				new SchemaBoolColumn[]
				{
					DummyBizoSchema.Z0_Bool, DummyBizoSchema.Z0_Bool, DummyBizoSchema.Z0_Bool, DummyBizoSchema.Z0_Bool, DummyBizoSchema.Z0_Bool,
					DummyBizoSchema.Z0_Bool, DummyBizoSchema.Z0_Bool, DummyBizoSchema.Z0_Bool, DummyBizoSchema.Z0_Bool, DummyBizoSchema.Z0_Bool
				});
			testFlags.PropertyValidation = (info) =>
			{
				if (!info.Value.IsEmpty)
				{
					info.AddError("ERROR " + info.Name);
				}
			};

			AssertPropertyValidionInfo(testFlags.Property0Info);
			AssertPropertyValidionInfo(testFlags.Property1Info);
			AssertPropertyValidionInfo(testFlags.Property2Info);
			AssertPropertyValidionInfo(testFlags.Property3Info);
			AssertPropertyValidionInfo(testFlags.Property4Info);
			AssertPropertyValidionInfo(testFlags.Property5Info);
			AssertPropertyValidionInfo(testFlags.Property6Info);
			AssertPropertyValidionInfo(testFlags.Property7Info);
			AssertPropertyValidionInfo(testFlags.Property8Info);
			AssertPropertyValidionInfo(testFlags.Property9Info);
		}

		static void AssertPropertyValidionInfo(ZPropertyInfo info)
		{
			info.Value = ZBool.False;
			var message = "ERROR " + info.Name;
			AssertNoError(info, message);
			info.Value = ZBool.True;
			AssertHasError(info, message);
		}

		#region Implementation

		protected override ModuleFlagsFilter GetNewModuleFilter()
		{
			return new ModuleFlagsFilter("moo", new string[] { "Flag" }, new SchemaBoolColumn[] { DummyBizoSchema.Z0_Bool });
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.StatusAndFlags; }
		}

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(ModuleFlagsFilter filter)
		{
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { "Item" }).ToArray(); // This will cause the test to skip the indexer, which is problematic and sets values for properties that are tested independently anyway.
		}

		#endregion

		#region DummyModuleFlagsFilter

		public class DummyModuleFlagsFilter : ModuleFlagsFilter
		{
			public DummyModuleFlagsFilter()
				: base("DummyFlagsFilter", new string[] { "1", "2" }, new GetFlagsQuery[] { delegate { return new ZQuery(); }, delegate { return new ZQuery(); } })
			{
			}

			protected override void SerializePropertiesToXml(XmlWriter writer)
			{
				for (var i = 1; i < FlagNames.Length; i++)
				{
					writer.WriteElementString("Property" + i, SaveBrokenData ? "invalid bool" : GetPropertyValue(i).ToString());
				}
			}

			public bool SaveBrokenData { get; set; }
		}

		#endregion
	}
}
