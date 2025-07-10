using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleUserDefinedFilter))]
	class ModuleUserDefinedFilterTest : ModuleFilterTestCase<ModuleUserDefinedFilter>
	{
		public void TestCreatorOrEarlyestUser()
		{
			var dummyLayout = Factory.NewWithValidTestData<StmModuleFilter>();
			dummyLayout.S9_FilterName = "Me filter";
			var filter = new ModuleUserDefinedFilter(dummyLayout, DummyModuleIDs.Dummy, DummyBizoSchema.PK);

			AssertEquals(Env.CurrentUserPK, filter.CreatorOrEarlyestUser.PK);
		}

		public void TestGetQuery_ShouldReturnQueryDefinedBySelectedFilters()
		{
			var (_, dummy2, dummy3, _) = MakeDummies();

			dummy3.Z0_Description = dummy2.Z0_Description;
			dummy3.Z0_Code = dummy2.Z0_Code;

			Factory.Save();
			FilterStripsTestHelper.CreateUserDefinedFilterStrip_TwoProperties();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var strip = module.FilterBusinessObject.FilterStrips.AddNew("[USR]Me filter");
				AssertNotNull("PRE: We should have succesfully found the 'Me filter'", strip);

				var filter = module.FilterBusinessObject.Filter;
				var result = Factory.Load<DummyBusinessObject>(filter);
				AssertContainsExactElementsInAnyOrder(filter.LiteralTextSqlFormatted, new[] { dummy2.PK, dummy3.PK }, result.Select(x => x.PK));

				strip = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Number");
				((ModuleNumberRangeFilter)strip.CurrentModuleFilter).Property1 = 2;
				((ModuleNumberRangeFilter)strip.CurrentModuleFilter).Property2 = 2;

				result = Factory.Load<DummyBusinessObject>(module.FilterBusinessObject.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { dummy2.PK }, result.Select(x => x.PK));
			}
		}

		public void TestFilterUsedInOrCategory()
		{
			var (dummy1, dummy2, _, _) = MakeDummies();

			Factory.Save();

			FilterStripsTestHelper.CreateUserDefinedFilterStrip("Me filter", propertyValue: "Keyokuk");

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var strip1 = module.FilterBusinessObject.FilterStrips.AddNew("[USR]Me filter");
				AssertNotNull("PRE: We should have succesfully found the 'Me filter'", strip1);
				var result = Factory.Load<DummyBusinessObject>(module.FilterBusinessObject.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { dummy2.PK }, result.Select(x => x.PK));

				var strip2 = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Description");
				((ModuleTextFilter)strip2.CurrentModuleFilter).Property = "Walla Walla";
				AssertNotNull("PRE: We should have succesfully amde an entirely new filter", strip2);

				result = Factory.Load<DummyBusinessObject>(module.FilterBusinessObject.Filter);
				AssertContainsExactElementsInAnyOrder("The new filter should have been added with an AND condition, so nothing should match, and yet...", Array.Empty<ZGuid>(), result.Select(x => x.PK));

				strip1.CurrentModuleFilter.OrCategory = FilterOrCategory.PapayaWhip;
				strip2.CurrentModuleFilter.OrCategory = FilterOrCategory.PapayaWhip;

				result = Factory.Load<DummyBusinessObject>(module.FilterBusinessObject.Filter);
				AssertContainsExactElementsInAnyOrder("The two filters should have an OR condition, so both jobs should match, and yet...", new[] { dummy1.PK, dummy2.PK }, result.Select(x => x.PK));
			}
		}

		public void TestNoJunkUserData()
		{
			var inner = FilterStripsTestHelper.CreateUserDefinedFilterStrip("InnerStrip");
			var outerName = "SvelteStrip";
			var outer = FilterStripsTestHelper.AddUserDefinedFilterLayoutToAnotherFilterLayout(inner.S9_FilterName, outerName);

			Factory.Save();

			CombineAssertions("PRE: We have two filter strips that are valid", () =>
			{
				AssertNoErrors(inner);
				AssertNoErrors(outer);
			});

			var outerUserData = outer.GetLayoutUserData(new EmptyLayoutsHelper());
			var actualXml = outerUserData.S0_FilterDataValues.ToAscii();

			AssertEquals("Our user-defined filter xml layoutdata should match the expected format", UserDefinedFilterXml_ThatReferencesAnotherLayout, actualXml);
		}

		public void TestFilterUsesDeprecatedData_WhenItExists()
		{
			var (dummy1, dummy2, dummy3, dummy4) = MakeDummies();
			dummy1.Z0_Description = dummy2.Z0_Description = "Test1";
			dummy3.Z0_Description = dummy4.Z0_Description = "Test2";
			dummy1.Z0_Code = dummy3.Z0_Code = "AAA";
			dummy2.Z0_Code = dummy4.Z0_Code = "BBB";

			var inner = FilterStripsTestHelper.CreateUserDefinedFilterStrip("InnerStrip", propertyName: "Z0_Code", propertyValue: "AAA");
			var outerName = "FatFilter";

			StmModuleFilter outer = null;
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.FilterBusinessObject.AddFilterStrip<ModuleUserDefinedFilter>(ModuleUserDefinedFilter.GetPrefixedDescription(inner.S9_FilterName));
				module.FilterBusinessObject.AddTextFilterStrip("Z0_Description", "Test2");

				outer = FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, outerName, true, true, true);
			}

			Factory.Save();

			var outerUserData = outer.GetLayoutUserData(new EmptyLayoutsHelper());

			CombineAssertions("We made two strips that work and are nice and are not bad", () =>
			{
				AssertNoErrors(inner);
				AssertNoErrors(outer);
				AssertEquals(UserDefinedFilterXml_ThatReferencesAnotherLayout_AndAnotherFilter, outerUserData.S0_FilterDataValues.ToAscii());
			});

			// make the filterUserData match "dummy4"
			var myNewFilterUserLayoutData = outerUserData.S0_FilterDataValues.ToAscii().Replace(@"<LayoutName>InnerStrip</LayoutName>", @"<Comparer>starts with</Comparer>    <Property>BBB</Property>");
			outerUserData.S0_FilterDataValues = ZBlob.FromAscii(myNewFilterUserLayoutData);

			// make the filterUserData look at Z0_Code instead of our inner filter
			var myNewFilterData = outer.S9_FilterData.ToAscii().Replace("[USR]InnerStrip", "Z0_Code");
			outer.S9_FilterData = ZBlob.FromAscii(myNewFilterData);

			outerUserData.Factory.Save();
			outer.Factory.Save();
			Factory.Save();

			FilterStripsTestHelper.AssertFilterResults(new BusinessObjectFactory(), ModuleUserDefinedFilter.GetPrefixedDescription(outer), new[] { dummy4.PK });
		}

		#region XML

		const string UserDefinedFilterXml_ThatReferencesAnotherLayout_AndAnotherFilter =
#if NETFRAMEWORK
@"<?xml version=""1.0""?>" +
#else
@"<?xml version=""1.0"" encoding=""utf-8""?>" +
#endif
@"
<FilterLayoutValuesSerializer>
  <ModuleFilters>
    <ModuleFilter>
      <LayoutName>InnerStrip</LayoutName>
    </ModuleFilter>
    <ModuleFilter>
      <Comparer>starts with</Comparer>
      <Property>Test2</Property>
    </ModuleFilter>
  </ModuleFilters>
</FilterLayoutValuesSerializer>";

		const string UserDefinedFilterXml_ThatReferencesAnotherLayout =
#if NETFRAMEWORK
@"<?xml version=""1.0""?>" +
#else
@"<?xml version=""1.0"" encoding=""utf-8""?>" +
#endif
@"
<FilterLayoutValuesSerializer>
  <ModuleFilters>
    <ModuleFilter>
      <LayoutName>InnerStrip</LayoutName>
    </ModuleFilter>
  </ModuleFilters>
</FilterLayoutValuesSerializer>";

		#endregion

		static StmModuleFilter AssembleAnOuterLayout(string outerLayoutName, params Action<FilterStripBusinessObject>[] addToFilterActions)
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;

				foreach (var addToFilterAction in addToFilterActions)
				{
					addToFilterAction?.Invoke(filterBizo);
				}

				return FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, outerLayoutName, true, true, true);
			}
		}

		public void TestNestedLayoutWithMixedFilterOrder_ShouldRespectOrder_NormalAfterUserDefined()
		{
			var (dummy1, dummy2, _, _) = MakeDummies();

			var mutualDescription = dummy1.Z0_Description = dummy2.Z0_Description;

			var inner = FilterStripsTestHelper.CreateUserDefinedFilterStrip("InnerStrip", propertyValue: mutualDescription);
			var outerName = "henlo";

			void AddInnerFilter(FilterStripBusinessObject filterBizo) => filterBizo.AddFilterStrip<ModuleUserDefinedFilter>(ModuleUserDefinedFilter.GetPrefixedDescription(inner));
			void AddCodeFilter(FilterStripBusinessObject filterBizo) => filterBizo.AddTextFilterStrip("Z0_Code", dummy1.Z0_Code);

			var outer = AssembleAnOuterLayout(outerName, AddInnerFilter, AddCodeFilter);

			Factory.Save();

			FilterStripsTestHelper.AssertFilterResults(Factory, ModuleUserDefinedFilter.GetPrefixedDescription(outer), new[] { dummy1.PK });
		}

		public void TestNestedLayoutWithMixedFilterOrder_ShouldRespectOrder_NormalBeforeUserDefined()
		{
			var (dummy1, dummy2, _, _) = MakeDummies();

			var mutualDescription = dummy1.Z0_Description = dummy2.Z0_Description;

			var inner = FilterStripsTestHelper.CreateUserDefinedFilterStrip("InnerStrip", propertyValue: mutualDescription);
			var outerName = "henlo";

			void AddCodeFilter(FilterStripBusinessObject filterBizo) => filterBizo.AddTextFilterStrip("Z0_Code", dummy1.Z0_Code);
			void AddInnerFilter(FilterStripBusinessObject filterBizo) => filterBizo.AddFilterStrip<ModuleUserDefinedFilter>(ModuleUserDefinedFilter.GetPrefixedDescription(inner));

			var outer = AssembleAnOuterLayout(outerName, AddInnerFilter, AddCodeFilter);

			Factory.Save();

			FilterStripsTestHelper.AssertFilterResults(Factory, ModuleUserDefinedFilter.GetPrefixedDescription(outer), new[] { dummy1.PK });
		}

		public void TestNestedLayoutWithMixedFilterOrder_ShouldRespectOrder_MixedNormalAndUserDefined()
		{
			var (dummy1, dummy2, dummy3, dummy4) = MakeDummies();

			var mutualDescription = dummy1.Z0_Description = dummy2.Z0_Description = dummy3.Z0_Description = dummy4.Z0_Description;
			var mutualCode = dummy1.Z0_Code = dummy2.Z0_Code = dummy3.Z0_Code;
			var specificDescription = dummy1.Z0_Description = dummy1.Z0_Description + " plus some extra";

			var inner1 = FilterStripsTestHelper.CreateUserDefinedFilterStrip("InnerStrip1", propertyName: "Z0_Code", propertyValue: mutualCode);

			void AddDescriptionFilter(FilterStripBusinessObject filterBizo)
			{
				var newFilter = filterBizo.AddTextFilterStrip("Z0_Description", mutualDescription);
				newFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			}
			void AddInner1Filter(FilterStripBusinessObject filterBizo) => filterBizo.AddFilterStrip<ModuleUserDefinedFilter>(ModuleUserDefinedFilter.GetPrefixedDescription(inner1));
			void AddForeignCodeFilter(FilterStripBusinessObject filterBizo)
			{
				var newFilter = filterBizo.AddTextFilterStrip("Z0_Description", specificDescription);
				newFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			}

			var outerName = "henlo";
			var outer = AssembleAnOuterLayout(outerName, AddDescriptionFilter, AddInner1Filter, AddForeignCodeFilter);

			Factory.Save();

			// if the second and third filters don't work, we will get dummies 1-3
			// if the third filters doesn't work, we will get dummies 1-2
			FilterStripsTestHelper.AssertFilterResults(Factory, ModuleUserDefinedFilter.GetPrefixedDescription(outer), new[] { dummy1.PK });
		}

		public void TestLocalizedDescription_ShouldIncludeSuffix()
		{
			// Why? Because if filter descriptions for the drop down are duplicated, it makes the filters not work (you select one and you get the other),
			// and users can name these whatever they want, so we need to ensure they're always unique.

			var dummyLayout = Factory.NewWithValidTestData<StmModuleFilter>();
			dummyLayout.S9_FilterName = "Me filter";
			var filter = new ModuleUserDefinedFilter(dummyLayout, DummyModuleIDs.Dummy, DummyBizoSchema.PK);
			filter.MultilingualDescription = (NoResString)"It's a suffix, not a prefix";

			AssertEquals("It's a suffix, not a prefix [+]", filter.LocalizedDescription);
		}

		public void TestClear_ShouldDoNothing()
		{
			FilterStripsTestHelper.CreateUserDefinedFilterStrip_TwoProperties();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = "Me filter [+]";
				module.Grid.Focus();

				var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();
				FilterStripsTestHelper.AssertFindBoxText(null, filterFindBox, "2 filters applied");

				filterControl.ToolStripClearButton_ForTest.PerformClick();
				Application.DoEvents();

				FilterStripsTestHelper.AssertFindBoxText("Clear should do nothing for user-defined filters, because they aren't directly editable, and yet...", filterFindBox, "2 filters applied");
			}
		}

		public void TestDescription_ShouldHaveSuffix()
		{
			var dummyLayout = Factory.NewWithValidTestData<StmModuleFilter>();
			dummyLayout.S9_FilterName = "Shalala";
			var filter = new ModuleUserDefinedFilter(dummyLayout, DummyModuleIDs.Dummy, DummyBizoSchema.PK);
			AssertEquals("[USR]Shalala", filter.Description);
		}

		public void TestUserDefinedFilter_ShouldWorkAcrossDifferentLanguages()
		{
			var (dummy1, dummy2, dummy3, _) = MakeDummies();

			dummy3.Z0_Description = dummy2.Z0_Description;

			Factory.Save();
			ZGuid filterPk;

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var strip = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Description");
				((ModuleTextFilter)strip.CurrentModuleFilter).Property = "Keyokuk";
				var filter = FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "My English Filter", true, false, true);
				filterPk = filter.PK;
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filter = (ModuleUserDefinedFilter)module.FilterBusinessObject.FilterStrips.AddNew("[USR]My English Filter").CurrentModuleFilter;
				AssertEquals("My English Filter [+]", filter.LocalizedDescription);

				var result = Factory.Load<DummyBusinessObject>(module.FilterBusinessObject.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { dummy2.PK, dummy3.PK }, result.Select(x => x.PK));
			}

			var staff = Factory.New<IGlbStaff>();
			((BusinessObject)staff).FillWithValidTestData();
			staff.GS_WorkingLanguage = Enterprise.Core.SharedConstants.Languages.Arabic;
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(staff.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			using (var cache = Res.UseMockData())
			{
				var eng = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage);
				cache.SetResourceGetter(key => key.StartsWith("S9_FilterName@Dummy$TXkgRW5nbGlzaCBGaWx0ZXI=")
					? new ResourceStringData(key, "My Arabic Filter", "My Arabic Filter", "My Arabic Filter", "My Arabic Filter")
					: eng.Get(key));

				using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					var filter = (ModuleUserDefinedFilter)module.FilterBusinessObject.FilterStrips.AddNew("[USR]My English Filter").CurrentModuleFilter;
					AssertEquals("My Arabic Filter [+]", filter.LocalizedDescription);

					var result = Factory.Load<DummyBusinessObject>(module.FilterBusinessObject.Filter);
					AssertContainsExactElementsInAnyOrder("The filter should still work in another language, and yet...", new[] { dummy2.PK, dummy3.PK }, result.Select(x => x.PK));

					FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "A layout", true, false, false);
				}

				using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					using (var form = new ManageFiltersFormTest.TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
					{
						form.Show();
						Application.DoEvents();
						form.FiltersTreeViewExposed.SelectedNode = FindItemContainingTextInTreeNodeList(form.FiltersTreeViewExposed.Nodes.Cast<TreeNode>(), "[+]");
						form.DeleteSelectedLayoutExposed();

						AssertEquals(@"The user-defined filter [My Arabic Filter] is included in the filter layout [A layout]:

Test Module -> A layout -> My Arabic Filter

Deleting [My Arabic Filter] will remove it from that layout and may affect its query results. Are you sure you want to delete [My Arabic Filter]?", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestIsIndexSearch()
		{
			var dummyLayout = Factory.NewWithValidTestData<StmModuleFilter>();
			var filter = new ModuleUserDefinedFilter(dummyLayout, DummyModuleIDs.Dummy, DummyBizoSchema.PK);
			AssertEquals(false, filter.IsIndexSearch);

			dummyLayout.S9_IsIndexSearch = true;
			AssertEquals(true, filter.IsIndexSearch);
		}

		public void TestGetGlowIndexQuery()
		{
			using (new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var strip = module.FilterBusinessObject.FilterStrips.AddNew("CODE");
				((IndexSearchModuleTextFilter)strip.CurrentModuleFilter).Property = "HAY";
				var filter = FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "My Glow Filter", true, false, true);
			}

			using (new GlowIndexQueryEngineMock())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.FilterBusinessObject.IndexSearchFields = GlowIndexQueryEngineMock.GetTestSearchFields();
				module.FilterBusinessObject.SearchType = SearchType.Index;

				var filter = (ModuleUserDefinedFilter)module.FilterBusinessObject.FilterStrips.AddNew("[USR]My Glow Filter").CurrentModuleFilter;
				AssertEquals("My Glow Filter [+]", filter.LocalizedDescription);

				var queries = module.FilterBusinessObject.GetActiveFiltersQueries();
				AssertEquals(1, queries.Count());
				var url = queries.FirstOrDefault().ToUrlComponent();
				AssertEquals("(startswith(CODE,'HAY'))", url);
			}
		}

		TreeNode FindItemContainingTextInTreeNodeList(IEnumerable<TreeNode> treeNodes, string text)
		{
			return treeNodes.FirstOrDefault(x => x.Text.Contains(text)) ??
				treeNodes.Select(n => FindItemContainingTextInTreeNodeList(n.Nodes.Cast<TreeNode>(), text)).FirstOrDefault(n => n != null);
		}

		#region Implementation

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		public override void TestQueryIsEmptyByDefault()
		{
			AssertEquals(false, Filter.Query.IsEmpty);
		}

		#endregion

		#region Setup

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.UserDefined;

		protected override ZString ExpectedDescription => "[USR]moo";

		protected override ModuleUserDefinedFilter GetNewModuleFilter()
		{
			return new ModuleUserDefinedFilter(GetDummyPrivateLayout(), DummyModuleIDs.Dummy, DummyBizoSchema.PK);
		}

		protected StmModuleFilter GetDummyPrivateLayout()
		{
			var dummyLayout = Factory.NewWithValidTestData<StmModuleFilter>();
			dummyLayout.S9_IsPublished = false;
			dummyLayout.S9_FilterName = "moo";
			return dummyLayout;
		}

		(DummyBusinessObject Dummy1, DummyBusinessObject Dummy2, DummyBusinessObject Dummy3, DummyBusinessObject Dummy4) MakeDummies()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Description = "Walla Walla";
			dummy1.Z0_Code = "AAA";
			dummy1.Z0_Number = 1;

			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy2.Z0_Description = "Keyokuk";
			dummy2.Z0_Code = "BBB";
			dummy2.Z0_Number = 2;

			var dummy3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy3.Z0_Description = "Filterpalooza";
			dummy3.Z0_Code = "CCC";
			dummy3.Z0_Number = 3;

			var dummy4 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy4.Z0_Description = "SmolBoiHours";
			dummy4.Z0_Code = "DDD";
			dummy4.Z0_Number = 4;

			return (dummy1, dummy2, dummy3, dummy4);
		}

		#endregion

		#region TestValidation

		public void TestValidation_ShouldValidateSelectedFilters()
		{
			FilterStripsTestHelper.SaveFilterLayout(ModuleIDs.GlbGroup, "I'm company specific", true, false, true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessTasks))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.IsInFilterRuleMode = true;

				var groupFilter = filterBizo.AddGuidFilterStrip("Group");
				groupFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;

				var companySpecificUserFilter = groupFilter.SelectedFilters.AddFilterStrip<ModuleUserDefinedFilter>("[USR]I'm company specific");

				filterBizo.RunPreSaveValidation();
				AssertHasError(groupFilter.SelectedFiltersDescriptionInfo, "The selected filters have one or more errors.");
				AssertHasError(companySpecificUserFilter.SelectedFiltersDescriptionInfo, "[USR]I'm company specific: User-defined filters must be published for all companies in order for them to be used for filter rules.");
			}
		}

		#endregion

	}
}
