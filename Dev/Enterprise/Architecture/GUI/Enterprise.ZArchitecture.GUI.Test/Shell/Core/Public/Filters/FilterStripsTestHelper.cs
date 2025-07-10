using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public static class FilterStripsTestHelper
	{
		#region Create Filter Strips

		public static void AddFilterStrips(StmModuleFilter stmFilter, ModuleIdentifier moduleId, params FilterStripDefinition[] filterStripDefs)
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(moduleId))
			{
				var filters = module.FilterBusinessObject;

				foreach (var filterStripDef in filterStripDefs)
				{
					var filterStrip = filters.FilterStrips.AddNew(filterStripDef.FilterStripName);
					filterStrip.OrCategory = filterStripDef.OrCategory;
					filterStrip.GroupOrCategory = filterStripDef.GroupOrCategory;
					filterStrip.GroupName = filterStripDef.GroupName;

					var filter = filterStrip.CurrentModuleFilter;

					filterStripDef.FilterStripValueSetter?.Invoke(filter);
					filterStripDef.ComparisonOperatorSetter?.Invoke(filter);
				}

				filters.WriteFilterStripsToXml(stmFilter, filters.FilterStrips, new EmptyLayoutsHelper());
			}
		}

		public static void AddFilterStrips(StmModuleFilter stmFilter, params FilterStripDefinition[] filterStripDefs)
		{
			var matches = ModuleIDs.AllExcludingClientModules.Where(x => x.Name == stmFilter.S9_ModuleID).ToArray();

			if (matches.Length > 1)
			{
				throw new InvalidOperationException("Multiple module ids found for this filter. Please use AddFilterStrips(StmModuleFilter, ModuleIdentifier, FilterStripDefinition[]) instead.");
			}

			AddFilterStrips(stmFilter, matches.Single(), filterStripDefs);
		}

		public class FilterStripDefinition
		{
			public string FilterStripName { get; set; }
			public Action<ModuleFilter> FilterStripValueSetter { get; set; }
			public FilterOrCategory OrCategory { get; set; }
			public FilterOrCategory GroupOrCategory { get; set; }
			public ZString GroupName { get; set; }
			public Action<ModuleFilter> ComparisonOperatorSetter { get; set; }
		}

		public static T AddFilterStrip<T>(FilterStripBusinessObject filterBizo, string filterName, Action<T> filterStripPropertySetter)
			where T : ModuleFilter
		{
			var subStrip = filterBizo.FilterStrips.AddNew(filterName);
			var subFilter = (T)subStrip.CurrentModuleFilter;

			subFilter.IsActive = true;
			filterStripPropertySetter(subFilter);

			return subFilter;
		}

		public static ModuleSQLFilter AddCustomSQLFilterStrip(FilterStripBusinessObject filterBizo, string sqlPredicate)
		{
			return (ModuleSQLFilter)AddFilterStrip<ModuleFilter>(filterBizo, FilterStripBusinessObject.CustomSqlFilterDescription, f => ((ModuleSQLFilter)f).Property1 = sqlPredicate);
		}

		public static void AddFilterStrip<T>(StmModuleFilter layout, string filterName, Action<T> valueSetter = null, Action<T> comparisonOperatorSetter = null, FilterOrCategory orCategory = FilterOrCategory.None)
			where T : ModuleFilter
		{
			var definition = new FilterStripDefinition
			{
				FilterStripName = filterName,
				FilterStripValueSetter = f => valueSetter?.Invoke((T)f),
				ComparisonOperatorSetter = f => comparisonOperatorSetter?.Invoke((T)f),
				OrCategory = orCategory
			};

			AddFilterStrips(layout, definition);
		}

		public static void AddCustomSQLFilterStrip(StmModuleFilter layout, string sqlPredicate)
		{
			AddFilterStrip<ModuleFilter>(layout, FilterStripBusinessObject.CustomSqlFilterDescription, f => ((ModuleSQLFilter)f).Property1 = sqlPredicate);
		}

		public static void AddStartsWithFilter(StmModuleFilter filter, string filterName, string startsWithText)
		{
			AddFilterStrips(filter, new FilterStripDefinition
			{
				FilterStripName = filterName,
				FilterStripValueSetter = (f) => ((ModuleTextFilter)f).Property = startsWithText
			});
		}

		#region Strips for Module UserDefined Filter

		public static StmModuleFilter AddUserDefinedFilterLayoutToAnotherFilterLayout(string innerLayoutName, string outerLayoutName, ModuleIdentifier moduleToCreateFilterFor = null)
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(moduleToCreateFilterFor ?? DummyModuleIDs.Dummy))
			{
				module.FilterBusinessObject.AddFilterStrip<ModuleUserDefinedFilter>(ModuleUserDefinedFilter.GetPrefixedDescription(innerLayoutName));

				return SaveFilterLayout(module.FilterBusinessObject, outerLayoutName, true, true, true);
			}
		}

		public static StmModuleFilter CreateUserDefinedFilterStrip_TwoProperties(
			string filterName = "Me filter",
			bool isPublished = false,
			bool isPublishedGlobal = false,
			string property1Name = "Z0_Description",
			string property1Value = "Keyokuk",
			string property2Name = "Z0_Code",
			string property2Value = "BBB",
			ModuleIdentifier moduleToCreateFilterFor = null)
		{
			return CreateUserDefinedFilterStrip_TwoProperties(moduleToCreateFilterFor ?? DummyModuleIDs.Dummy, filterName, isPublished, isPublishedGlobal, property1Name, property1Value, property2Name, property2Value);
		}

		public static StmModuleFilter CreateUserDefinedFilterStrip_TwoProperties(
			ModuleIdentifier moduleToCreateFilterFor,
			string filterName = "Me filter",
			bool isPublished = false,
			bool isPublishedGlobal = false,
			string property1Name = "Z0_Description",
			string property1Value = "Keyokuk",
			string property2Name = "Z0_Code",
			string property2Value = "BBB")
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(moduleToCreateFilterFor))
			{
				AddModuleTextFilterToModule(module, property1Name, property1Value);
				AddModuleTextFilterToModule(module, property2Name, property2Value);

				return FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, filterName, isPublished, isPublishedGlobal, true);
			}
		}

		public static StmModuleFilter CreateUserDefinedFilterStrip(
			string filterName,
			ModuleIdentifier moduleToCreateFilterFor = null,
			string propertyName = "Z0_Description",
			string propertyValue = "DummyDescription",
			bool isPublished = true,
			bool isPublishedGlobal = false)
		{
			return CreateUserDefinedFilterStrip<ModuleTextFilter>(filterName, moduleToCreateFilterFor, propertyName, propertyValue, isPublished, isPublishedGlobal);
		}

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "We need the T for when we want to make filters of different types in a painless way.")]
		public static StmModuleFilter CreateUserDefinedFilterStrip<T>(
			string filterName,
			ModuleIdentifier moduleToCreateFilterFor = null,
			string propertyName = "Z0_Description",
			string propertyValue = "DummyDescription",
			bool isPublished = true,
			bool isPublishedGlobal = false)
			where T : ModuleTextBaseFilter
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(moduleToCreateFilterFor ?? DummyModuleIDs.Dummy))
			{
				var newStrip = module.FilterBusinessObject.FilterStrips.AddNew(propertyName);
				((T)newStrip.CurrentModuleFilter).Property = propertyValue;

				return FilterStripsTestHelper.SaveFilterLayout(
					module.FilterBusinessObject,
					filterName,
					isPublished,
					isPublishedGlobal,
					true);
			}
		}

		static void AddModuleTextFilterToModule(ZFilterGridModule module, string propertyName, string propertyValue)
		{
			var strip = module.FilterBusinessObject.FilterStrips.AddNew(propertyName);
			((ModuleTextFilter)strip.CurrentModuleFilter).Property = propertyValue;
		}

		public static ModuleUserDefinedFilter CreateModuleUserDefinedFilter(StmModuleFilter savedFilterLayout)
		{
			return new ModuleUserDefinedFilter(savedFilterLayout, DummyModuleIDs.Dummy, DummyBizoSchema.PK);
		}

		public static ModuleUserDefinedFilter CreateModuleUserDefinedFilterAndSavedFilterLayout(string filterName = "Me filter", bool isPublished = false)
		{
			return CreateModuleUserDefinedFilter(CreateUserDefinedFilterStrip_TwoProperties(filterName, isPublished));
		}

		#endregion

		#endregion

		#region Save Filter Layouts

		public static StmModuleFilter SaveFilterLayout(ModuleIdentifier moduleId, string layoutName, bool isPublished, bool isPublishedGlobal, bool isUserDefinedFilter)
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(moduleId))
			{
				return SaveFilterLayout(module.FilterBusinessObject, layoutName, isPublished, isPublishedGlobal, isUserDefinedFilter);
			}
		}

		public static StmModuleFilter SaveFilterLayout(FilterStripBusinessObject filterBizo, string layoutName, bool isPublished, bool isPublishedGlobal, bool isUserDefinedFilter)
		{
			return new DataGridLayoutManager().SavePreconfiguredLayout(filterBizo, layoutName, isPublished, isPublishedGlobal, SaveColumnLayout.No, SaveGridColourLayout.No, isUserDefinedFilter);
		}

		public static void DropStmModuleFilterRuleConstraints()
		{
			Db.Connection.ExecuteNonQuery(@"
ALTER TABLE dbo.StmModuleFilter DROP CONSTRAINT Constraint_FRUHasParentID
ALTER TABLE dbo.StmModuleFilter DROP CONSTRAINT Constraint_S9_ParentTableCode
");
		}

		#endregion

		#region Assertions

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Testing")]
		internal static void AssertFindBoxText(string message, ZFilterCollectionFindBox filterFindBox, string expectedText)
		{
			// Updating the text can take up to a second and even Application.DoEvents doesn't force it along. So this is to prevent intermittent failures.
			var waitIterations = 0;

			do
			{
				Thread.Sleep(100);
				Application.DoEvents();
				waitIterations++;
			}
			while (string.IsNullOrEmpty(filterFindBox.DescriptionBox.Text) && waitIterations < 50);

			Assertion.AssertEquals(message, expectedText, filterFindBox.DescriptionBox.Text);
		}

		public static void AssertFilterResults(BusinessObjectFactory factory, string filterName, ZGuid[] expectedResults, ModuleIdentifier moduleToSearchIn = null)
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(moduleToSearchIn ?? DummyModuleIDs.Dummy))
			{
				module.FilterBusinessObject.FilterStrips.AddNew(filterName);
				var filter = module.FilterBusinessObject.Filter;
				var result = factory.Load<DummyBusinessObject>(filter);

				NUnit.Framework.Assertion.AssertContainsExactElementsInAnyOrder(expectedResults, result.Select(x => x.PK));
			}
		}

		#endregion
	}
}
