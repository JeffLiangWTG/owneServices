using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ModuleFilterWithSelectedFiltersTest : TestCaseWithFactory
	{
		public void TestCacheIsThreadStatic()
		{
			var filter = new ModuleGuidFilter("AAA", DummyModuleIDs.Dummy, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
			var sf1 = filter.SelectedFilters;

			var thread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var filter2 = new ModuleGuidFilter("AAA", DummyModuleIDs.Dummy, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
					var sf2 = filter2.SelectedFilters;
					Assert(filter2.SelectedFilters.Factory.ThreadSentry.IsOwner);
				}
			});
			thread.Start();
			thread.Join();
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestCacheThreadDoesNotKeepFactoriesWhenInWebOrWebServiceMode()
		{
			void TestCase()
			{
				var filter = new ModuleGuidFilter("AAA", DummyModuleIDs.Dummy, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
				var sf1 = filter.SelectedFilters;
				AssertNotNull(sf1);

				GC.Collect();
				GC.WaitForFullGCComplete();
				GC.WaitForPendingFinalizers();

				var persistentFactoryCacheManager = PersistentFactoryCacheManager.Instance;
				var initialFactoryCount = persistentFactoryCacheManager.GetBusinessObjectFactories().Count(f => f.NameForDebugging == "FilterStripBusinessObject (Constructor)");

				var thread = new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						var filter2 = new ModuleGuidFilter("AAA", DummyModuleIDs.Dummy, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
						var sf2 = filter2.SelectedFilters;
						AssertNotNull(sf2);
					}
				});
				thread.Start();
				thread.Join();

				GC.Collect();
				GC.WaitForFullGCComplete();
				GC.WaitForPendingFinalizers();

				var actualFactoryCount = persistentFactoryCacheManager.GetBusinessObjectFactories().Count(f => f.NameForDebugging == "FilterStripBusinessObject (Constructor)");

				AssertEquals(initialFactoryCount, actualFactoryCount);
			}

			Globals.IsWebService = true;
			Globals.IsWeb = false;

			TestCase();

			Globals.IsWebService = false;
			Globals.IsWeb = true;

			TestCase();
		}

		public void TestUpdateSelectedFilters_MakesANewFSBO()
		{
			var filter = new ModuleGuidFilter("AAA", DummyModuleIDs.Dummy, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
			var sf1 = filter.SelectedFilters;

			var filter2 = new ModuleGuidFilter("AAA", DummyModuleIDs.Dummy, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
			var sf2 = filter2.SelectedFilters;

			sf1.AddTextFilterStrip("Z0_Description", "A journey of a thousand miles begins with a single step (Confucius)");
			AssertEquals("same filters successfully acquired from cache cross-thread", sf1, sf2);
			AssertEquals(1, sf1.ActiveModuleFilters.Count);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var newFSBO = module.FilterBusinessObject;
				newFSBO.AddTextFilterStrip("Z0_Description", "A");
				newFSBO.AddTextFilterStrip("Z0_Description", "B");
				filter2.UpdateSelectedFilters(newFSBO);
			}
			var sf3 = filter2.SelectedFilters;
			AssertNotEquals("calling ReplaceStripsWithOthers on filter in cache caused new instance to be created", sf1, sf3);
			AssertEquals("old filters were not clobbered", 1, sf1.ActiveModuleFilters.Count);
			AssertEquals("new filters has correct count", 2, sf3.ActiveModuleFilters.Count);
		}

		public void TestGetNewSelectedFiltersDoesNotUseCacheIfDuplicateDefault()
		{
			var filter = new ModuleGuidFilter("AAA", DummyModuleIDs.Dummy, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
			var selectedFilters1 = filter.SelectedFilters;

			var filter2 = new ModuleGuidFilter("AAA", DummyModuleIDs.Dummy, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
			filter2.IsDuplicateDefault = true;
			var selectedFilters2 = filter2.SelectedFilters;

			selectedFilters1.AddTextFilterStrip("Z0_Description", "Blah");
			AssertNotEquals("Should not use selected filter cache when IsDuplicateDefault", selectedFilters1, selectedFilters2);
		}

		public void TestFindDropButton_DoesNotContainFilterRuleFilters()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			FilterStripsTestHelper.DropStmModuleFilterRuleConstraints();
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				var strip1 = filterBizo.FilterStrips.AddNew("Z0_Description");
				var filter1 = (ModuleTextFilter)strip1.CurrentModuleFilter;
				filter1.Property = "Keyokuk";
				var stmFilter1 = filterBizo.SaveLayout("Keyokuk");
				stmFilter1.S9_FilterType = StmModuleFilterTypes.Codes.Module;

				var strip2 = filterBizo.FilterStrips.AddNew("Z0_Description");
				var filter2 = (ModuleTextFilter)strip2.CurrentModuleFilter;
				filter2.Property = "Walla Walla";
				var stmFilter2 = filterBizo.SaveLayout("Walla Walla");
				stmFilter2.S9_FilterType = StmModuleFilterTypes.Codes.FilterRule;

				Factory.Save();

				using (module.ShowPopup())
				{
					var filterControl = module.DisplayGrid.GetParentFilterControl();
					var dropDownItemList = filterControl.ToolStripFindDropButton_ForTest.DropDownItems.Cast<ToolStripDropDownItem>().ToList().Select(x => x.Text.Trim());

					AssertEquals("List should have the 'My Filter Layouts' title and only one of the filters for this module", 2, dropDownItemList.Count());

					AssertEquals("Only the non-FRU filter should be in the drop down list", true, dropDownItemList.Contains("Keyokuk"));
					AssertEquals("The FRU filter should NOT be in the drop down list", false, dropDownItemList.Contains("Walla Walla"));
				}
			}
		}

		public void TestFiltersMatch_ShouldNotResetLastSavedLayoutInSubModule()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			using (var subModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = subModule.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Z0_Description");
				var filter = (ModuleTextFilter)strip.CurrentModuleFilter;
				filter.Property = "Keyokuk";
				filterBizo.SaveLayout("Keyokuk");

				filter.Property = "Walla Walla";
				var lastLayout = filterBizo.SaveLayout("Walla Walla");
				filterBizo.SaveLastUsedLayout(lastLayout.PK);

				Factory.Save();

				using (subModule.ShowPopup())
				{
					var filterControl = subModule.DisplayGrid.GetParentFilterControl();
					AssertEquals("Find (Walla Walla)", filterControl.ToolStripFindDropButton_ForTest.Text);
				}
			}

			using (var parentModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.DummyDependent))
			using (var popup = new EmbeddedModulePopup(parentModule))
			{
				popup.Show();

				var helper = new EmbeddedModuleTestHelper(popup);
				var filter = (ModuleGuidFilter)helper.AddFilter<ModuleGuidFilter>("Dummy", f =>
				{
					f.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				}).CurrentModuleFilter;

				filter.SelectedFilters.AddTextFilterStrip("Z0_Description", "Cucamonga");

				Application.DoEvents();

				var filterControl = parentModule.DisplayGrid.GetParentFilterControl();
				var findBox = (ZFilterCollectionFindBox)filterControl.Controls.Find("FilterCollectionFindBox", true).Single();
				findBox.PopupButton.PerformClick();
			}

			using (var subModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (subModule.ShowPopup())
			{
				var filterControl = subModule.DisplayGrid.GetParentFilterControl();
				AssertEquals("The last used layout should not reset to the first option, and yet...", "Find (Walla Walla)", filterControl.ToolStripFindDropButton_ForTest.Text);
			}
		}

		public void TestSelectedFiltersDescription()
		{
			var filter = new ModuleGuidFilter("AAA", DummyModuleIDs.Dummy, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
			var filterBizo = new DummyFilterStripBusinessObject();
			filterBizo.ModuleFilters.AddCustomFilter(filter);

			filter.SelectedFilters.AddTextFilterStrip("Z0_Description", "A journey of a thousand miles begins with a single step (Confucius)");

			AssertEquals("Precondition: subfilter business object must contain 1 filter", 1, filter.SelectedFilters.ActiveModuleFilters.Count);
			AssertEquals("Precondition: subfilter business object must contain 1 filter", 1, filter.SelectedFilterCount);
			AssertEquals("The filter description must be set", "1 filter applied", filter.SelectedFiltersDescription);
		}

		public void TestClearSelectedFilters_ShouldSetFilterCountToZero()
		{
			var filter = new ModuleGuidFilter_ForTest("AAA", DummyModuleIDs.Dummy, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
			filter.SelectedFilters.AddTextFilterStrip("Z0_Description", "Shalala");

			AssertEquals(1, filter.SelectedFilterCount);
			AssertEquals("1 filter applied", filter.SelectedFiltersDescription);

			filter.ClearSelectedFilters_Exposed();
			AssertEquals(0, filter.SelectedFilterCount);
			AssertEquals("0 filters applied", filter.SelectedFiltersDescription);
		}

		public void TestCheckSelectedFilters_WithNonFiltersMatchOperatorSelected_ShouldNotValidateSelectedFilters()
		{
			var filter = new ModuleGuidFilter("AAA", DummyModuleIDs.Dummy, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
			var guidFilter = filter.SelectedFilters.AddGuidFilterStrip("Z0_Guid", ZGuid.Invalid);

			guidFilter.Validation.ValidateAll();
			AssertHasErrors(guidFilter.PropertyInfo);
			AssertEquals(1, filter.SelectedFilterCount);

			AssertNoErrors("Selected filters should not validate because 'filters match' was not selected, and yet...", filter.SelectedFiltersDescriptionInfo);
		}

		public void TestSelectedFiltersCacheIsClearedAfterSwitchingUserContext()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks))
			{
				var groupFilter = module.FilterBusinessObject.AddGuidFilterStrip("Group");
				groupFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;

				var key = new ModuleFilter.SelectedFiltersCacheKey(ZBlob.Empty, ZBlob.Empty, ModuleIDs.GlbGroup, false);
				ModuleFilter.SelectedFiltersCache.TryGetValue(key, out var cachedFilter);
				AssertNotNull(cachedFilter);

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					ModuleFilter.SelectedFiltersCache.TryGetValue(key, out cachedFilter);
					AssertNull(cachedFilter);
				}
			}
		}

		public void TestQueryCachingWhenWebServiceOrWeb()
		{
			Globals.IsWebService = true;
			Globals.IsWeb = false;

			TestQueryCaching();

			Globals.IsWebService = false;
			Globals.IsWeb = true;

			TestQueryCaching();

			var persistentFactoryCacheManager = PersistentFactoryCacheManager.Instance;
			var factories = persistentFactoryCacheManager.GetBusinessObjectFactories()
				.GroupBy(f => f.NameForDebugging)
				.Select(f => new
				{
					Name = f.First().NameForDebugging,
					Count = f.Count()
				}).ToArray();
		}

		public void TestQueryCaching()
		{
			var filter = new ModuleGuidFilter_ForTest("AAA", DummyModuleIDs.Dummy, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.ModuleFilters.AddCustomFilter(new ModuleTextFilterThatDoesNotSupportCaching("NoCache", DummyBizoSchema.Z0_Description));

			var subFilter = filter.SelectedFilters.AddTextFilterStrip("Z0_Description", "Accurêêêê");
			AssertEquals(0, filter.GetQueryCount);

			var query1 = filter.Query;
			AssertEquals(1, filter.GetQueryCount);

			var query2 = filter.Query;
			AssertEquals(1, filter.GetQueryCount);
			AssertEquals(query1.LiteralTextSqlFormatted, query2.LiteralTextSqlFormatted);

			subFilter.Property = "Ooo-wee!";
			var query3 = filter.Query;
			AssertEquals(2, filter.GetQueryCount);
			AssertNotEquals(query2.LiteralTextSqlFormatted, query3.LiteralTextSqlFormatted);

			var query4 = filter.Query;
			AssertEquals(2, filter.GetQueryCount);
			AssertEquals(query3.LiteralTextSqlFormatted, query4.LiteralTextSqlFormatted);

			var subFilter2 = filter.SelectedFilters.AddTextFilterStrip("Z0_Code", "HSH");

			var query5 = filter.Query;
			AssertEquals(3, filter.GetQueryCount);
			AssertNotEquals(query4.LiteralTextSqlFormatted, query5.LiteralTextSqlFormatted);

			var query6 = filter.Query;
			AssertEquals(3, filter.GetQueryCount);
			AssertEquals(query5.LiteralTextSqlFormatted, query6.LiteralTextSqlFormatted);

			subFilter2.IsActive = false;
			var query7 = filter.Query;
			AssertEquals(4, filter.GetQueryCount);
			AssertNotEquals(query6.LiteralTextSqlFormatted, query7.LiteralTextSqlFormatted);

			var query8 = filter.Query;
			AssertEquals(4, filter.GetQueryCount);
			AssertEquals(query7.LiteralTextSqlFormatted, query8.LiteralTextSqlFormatted);

			filter.SelectedFilters.ModuleFilters.SupportsQueryCaching = false;
			var query9 = filter.Query;
			AssertEquals(5, filter.GetQueryCount);
			AssertEquals(query8.LiteralTextSqlFormatted, query9.LiteralTextSqlFormatted);

			var query10 = filter.Query;
			AssertEquals(6, filter.GetQueryCount);
			AssertEquals(query9.LiteralTextSqlFormatted, query10.LiteralTextSqlFormatted);

			filter.SelectedFilters.ModuleFilters.SupportsQueryCaching = true;
			var query11 = filter.Query;
			AssertEquals(6, filter.GetQueryCount);
			AssertEquals(query10.LiteralTextSqlFormatted, query11.LiteralTextSqlFormatted);

			filter.SelectedFilters.AddTextFilterStrip("NoCache", "You can run but you can't hide, snitch!");

			var query12 = filter.Query;
			AssertEquals(7, filter.GetQueryCount);
			AssertNotEquals(query11.LiteralTextSqlFormatted, query12.LiteralTextSqlFormatted);

			var query13 = filter.Query;
			AssertEquals(8, filter.GetQueryCount);
			AssertEquals(query12.LiteralTextSqlFormatted, query13.LiteralTextSqlFormatted);
		}

		[TestDate(2017, 9, 1)]
		public void TestIsQueryStale_WhenSelectedFiltersCachedAfterMainQuery_ShouldNeedReevaluation()
		{
			var filter = new ModuleGuidFilter_ForTest("AAA", DummyModuleIDs.Dummy, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			var subFilter = filter.SelectedFilters.AddTextFilterStrip("Z0_Description", "Squanch");
			var fullQuery1 = filter.Query;

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			subFilter.Property = "Schwifty";
			var subQuery = filter.SelectedFilters.Filter;
			var fullQuery2 = filter.Query;

			AssertNotEquals("The query should have been reevaluated because one of the sub filters changed (even though manually calling the sub filter's Query property would have cached that query), and yet...", fullQuery1.LiteralTextSqlFormatted, fullQuery2.LiteralTextSqlFormatted);
		}

		public void TestIsEmpty()
		{
			var filterBizo = new DummyFilterBusinessObject();
			var filterWithColumn = filterBizo.AddGuidFilterStrip("Z0_Guid");
			AssertTestIsEmpty(filterWithColumn);
		}

		public void TestIsEmpty_NoFilterColumn()
		{
			var filterBizo = new DummyFilterBusinessObject();
			var filterWithoutColumn = filterBizo.ModuleFilters.AddGuidFilter("Dummy", DummyModuleIDs.Dummy, new GetGuidQueryWithOperatorSupportsFiltersMatch((a, b, c) => new ZQuery()), new DummyBusinessObjectCollection(Factory), DummyBizoSchema.Z0_Guid);
			AssertTestIsEmpty(filterWithoutColumn);
		}

		void AssertTestIsEmpty(ModuleGuidFilter filter)
		{
			AssertEquals(true, filter.IsEmpty);

			filter.Property = ZGuid.NewZGuid();
			AssertEquals(false, filter.IsEmpty);
			AssertEquals(false, filter.IsExpensiveQuery);

			filter.Clear();
			AssertEquals(true, filter.IsEmpty);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			AssertEquals(false, filter.IsEmpty);
			AssertEquals(false, filter.IsExpensiveQuery);
		}

		public void TestSerialize_WhenFiltersMatchNotSelected_ShouldNotSerialiseSelectedFilterData()
		{
			var filterBizo = new DummyFilterBusinessObject();
			var filter = filterBizo.AddGuidFilterStrip("Z0_Guid");
			var valuesXml = filterBizo.FilterStrips.GetLayoutValuesAsXml();

			const string selectedFiltersIdentifier = "SelectedFilters";
			AssertNotContains(selectedFiltersIdentifier, valuesXml.ToUTF8());

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			valuesXml = filterBizo.FilterStrips.GetLayoutValuesAsXml();
			AssertContains(selectedFiltersIdentifier, valuesXml.ToUTF8());
		}

		public void TestDeserialize_FromFiltersSavedWhenSelectedFiltersWasStoredAsStmModuleFilter_ShouldDeserializeCorrectly()
		{
			var filterBizo = new DummyFilterBusinessObjectWithGuidFilterWithOldSchoolSerialisation();
			var filter = filterBizo.AddFilterStrip<ModuleGuidFilterWithOldSchoolSerialisation>("Shalala");
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.AddTextFilterStrip("Z0_Description", "Squanch");

			var layout = filterBizo.SaveLayout("Squanchy query");

			filterBizo = new DummyFilterBusinessObjectWithGuidFilterWithOldSchoolSerialisation();
			filter = filterBizo.AddFilterStrip<ModuleGuidFilterWithOldSchoolSerialisation>("Shalala");
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.AddTextFilterStrip("Z0_Description", "Squanch 2");

			var layout2 = filterBizo.SaveLayout("Squanchy query 2");

			filterBizo = new DummyFilterBusinessObjectWithGuidFilterWithOldSchoolSerialisation();
			filterBizo.LoadLayout(layout);

			AssertContainsExactElementsInAnyOrder(new[] { "Shalala" }, filterBizo.ActiveModuleFilters.Select(x => x.Description));

			var loadedFilter = (ModuleGuidFilterWithOldSchoolSerialisation)filterBizo.ActiveModuleFilters.Single();
			AssertContainsExactElementsInAnyOrder(new[] { "Squanch" }, loadedFilter.SelectedFilters.ActiveModuleFilters.Cast<ModuleTextFilter>().Select(x => x.Property));

			//test cache
			var filterBizo2 = new DummyFilterBusinessObjectWithGuidFilterWithOldSchoolSerialisation();
			filterBizo2.LoadLayout(new BusinessObjectFactory().Load<StmModuleFilter>(layout.PK));
			var loadedFilter2 = (ModuleGuidFilterWithOldSchoolSerialisation)filterBizo2.ActiveModuleFilters.Single();
			AssertEquals(loadedFilter.SelectedFilters, loadedFilter2.SelectedFilters);

			var filterBizo3 = new DummyFilterBusinessObjectWithGuidFilterWithOldSchoolSerialisation();
			filterBizo3.LoadLayout(new BusinessObjectFactory().Load<StmModuleFilter>(layout2.PK));
			var loadedFilter3 = (ModuleGuidFilterWithOldSchoolSerialisation)filterBizo3.ActiveModuleFilters.Single();
			AssertNotEquals(loadedFilter.SelectedFilters, loadedFilter3.SelectedFilters);
			AssertContainsExactElementsInAnyOrder(new[] { "Squanch 2" }, loadedFilter3.SelectedFilters.ActiveModuleFilters.Cast<ModuleTextFilter>().Select(x => x.Property));
		}

		public void TestSerializeFiltersMatchFilter_WithAuditSubFilter_WhenDeserialized_ShouldIncludeSubFilter()
		{
			StmModuleFilter savedLayout;

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks))
			{
				var tasksFilterBizo = module.FilterBusinessObject;

				var groupFilter = tasksFilterBizo.AddGuidFilterStrip("Group");
				groupFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;

				groupFilter.SelectedFilters.AddNkFilterStrip("Creating User", "A");
				AssertEquals(1, groupFilter.SelectedFilterCount);

				savedLayout = tasksFilterBizo.SaveLayout("Group layout");
			}

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks))
			{
				var tasksFilterBizo = module.FilterBusinessObject;
				tasksFilterBizo.LoadLayout(savedLayout);
				AssertEquals(1, tasksFilterBizo.ActiveModuleFilters.Count);

				var groupFilter = (ModuleGuidFilter)tasksFilterBizo.ActiveModuleFilters.Single();
				AssertEquals(ModuleTextFilter.ComparisonConstants.FiltersMatch, groupFilter.ComparisonOperator);
				AssertEquals(1, groupFilter.SelectedFilterCount);

				var userFilter = (ModuleNkFilter)groupFilter.SelectedFilters.ActiveModuleFilters.Single();
				AssertEquals("Creating User", userFilter.Description);
				AssertEquals("A", userFilter.Property);
			}
		}

		public void TestUpdateSelectedFilters_WithAuditFilterIncluded_ShouldSaveAuditFilter()
		{
			using (var taskModule = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks))
			using (var groupModule = ZFilterModule.GetZFilterModule(ModuleIDs.GlbGroup))
			{
				var groupFilter = taskModule.FilterBusinessObject.AddGuidFilterStrip("Group");
				groupFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;

				groupModule.FilterBusinessObject.AddDateFilterStrip("Created Time");
				groupFilter.UpdateSelectedFilters(groupModule.FilterBusinessObject);

				AssertContainsExactElementsInAnyOrder("Updating the selected filters should have actually added the audit filter. SAD!", new[] { "Created Time" }, groupFilter.SelectedFilters.ActiveModuleFilters.Select(x => x.Description));
			}
		}

		public void TestValidate_WhenUsedOnFilterRule_WithNestedUnpublishedUserDefinedFilter_ShouldShowError()
		{
			FilterStripsTestHelper.SaveFilterLayout(ModuleIDs.GlbGroup, "I'm not published, sadly", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(ModuleIDs.GlbStaff, "Neither am I!", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(ModuleIDs.GlbStaff, "Well I am, so suck it!", true, false, true);

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks))
			{
				var filterBizo = module.FilterBusinessObject;
				var groupFilter = filterBizo.AddGuidFilterStrip("Group");
				groupFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;

				var unpublishedFilter1 = groupFilter.SelectedFilters.AddFilterStrip<ModuleUserDefinedFilter>("[USR]I'm not published, sadly");
				var staffFilter = groupFilter.SelectedFilters.AddNkFilterStrip("Creating User");
				staffFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;

				var unpublishedFilter2 = staffFilter.SelectedFilters.AddFilterStrip<ModuleUserDefinedFilter>("[USR]Neither am I!");
				var publishedFilter = staffFilter.SelectedFilters.AddFilterStrip<ModuleUserDefinedFilter>("[USR]Well I am, so suck it!");

				filterBizo.RunPreSaveValidation();
				AssertNoErrors("The filter bizo is not in filter rule mode, so there shouldn't be vaildation errors. SAD!", groupFilter);
				AssertNoErrors(staffFilter);
				AssertNoErrors(unpublishedFilter1);
				AssertNoErrors(unpublishedFilter2);
				AssertNoErrors(publishedFilter);

				((IRelatedModuleFilterBusinessObject)filterBizo).IsInFilterRuleMode = true;
				filterBizo.RunPreSaveValidation();
				AssertHasError(groupFilter.SelectedFiltersDescriptionInfo, @"The following non-published user-defined filters have been included in the selected filters of this filter strip:

Group -> I'm not published, sadly
Group -> Creating User -> Neither am I!

Filter rules cannot contain non-published user-defined filters.");
				AssertNoErrors(staffFilter);
				AssertNoErrors(unpublishedFilter1);
				AssertNoErrors(unpublishedFilter2);
				AssertNoErrors(publishedFilter);

				unpublishedFilter1.IsActive = false;
				filterBizo.RunPreSaveValidation();
				AssertHasError(groupFilter.SelectedFiltersDescriptionInfo, @"The following non-published user-defined filter has been included in the selected filters of this filter strip:

Group -> Creating User -> Neither am I!

Filter rules cannot contain non-published user-defined filters.");
				AssertNoErrors(staffFilter);
				AssertNoErrors(unpublishedFilter1);
				AssertNoErrors(unpublishedFilter2);
				AssertNoErrors(publishedFilter);

				unpublishedFilter2.IsActive = false;
				filterBizo.RunPreSaveValidation();
				AssertNoErrors(groupFilter);
				AssertNoErrors(staffFilter);
				AssertNoErrors(unpublishedFilter1);
				AssertNoErrors(unpublishedFilter2);
				AssertNoErrors(publishedFilter);
			}
		}

		public void TestCategoryForInitialModuleId()
		{
			var filter1 = new UserDefinedModuleFilter_ForTest("OrganisationModule", ModuleIDs.Organisation, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
			AssertEquals(FilterCategories.UserDefined, filter1.Category);

			var filter2 = new ModuleGuidFilter_ForTest("OrganisationModule", ModuleIDs.Organisation, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
			AssertEquals(FilterCategories.Organisations, filter2.Category);

			var filter3 = new UserDefinedModuleFilter_ForTest("OtherModule", ModuleIDs.AdministrationPanel, DummyDependentBizoSchema.ZD1_Z0, new DummyBusinessObjectCollection(Factory));
			AssertEquals(FilterCategories.UserDefined, filter3.Category);
		}

		public void TestGetSubFilterQueryIncludingCollectionFilters_WhenInvalidModuleSpecified_ShouldThrowException()
		{
			var filter = new ModuleGuidFilter("Bad Filter!", ModuleIDs.NotAssigned, DummyBizoSchema.Z0_Guid, () => new DummyBusinessObjectCollection(Factory));
			AssertExceptionThrown<InvalidFilterConfigurationException>("A custom exception should be thrown when the module is invalid, rather than a NRE. SAD!", $@"Unable to load the query for this filter's selected filters. Please ensure that the module associated with the filter is available in the current context.
Filter: Bad Filter!
Module: NotAssigned", () => ((IModuleFilterWithSelectedFilters)filter).GetSubFilterQueryIncludingCollectionFilters());
		}

		#region Implementation

		class ModuleGuidFilter_ForTest : ModuleGuidFilter
		{
			public ModuleGuidFilter_ForTest(ZString description, ModuleIdentifier id, SchemaGuidColumn filterColumn, IBusinessObjectCollection list)
				: base(description, id, filterColumn, list)
			{
			}

			public void ClearSelectedFilters_Exposed()
			{
				ClearSelectedFilters();
			}

			#region Overrides of ModuleFilter

			protected override ZQuery GetQuery()
			{
				GetQueryCount++;
				return base.GetQuery();
			}

			public int GetQueryCount { get; private set; }

			#endregion
		}

		class UserDefinedModuleFilter_ForTest : ModuleGuidFilter_ForTest
		{
			public UserDefinedModuleFilter_ForTest(ZString description, ModuleIdentifier id, SchemaGuidColumn filterColumn, IBusinessObjectCollection list) : base(description, id, filterColumn, list)
			{
			}

			protected override FilterCategory DefaultCategory => FilterCategories.UserDefined;
		}

		class ModuleTextFilterThatDoesNotSupportCaching : ModuleTextFilter
		{
			public ModuleTextFilterThatDoesNotSupportCaching(ZString description, SchemaStringColumn filterColumn) : base(description, filterColumn)
			{
			}

			protected override bool ShouldReevaluateQuery()
			{
				return true;
			}
		}

		class ModuleGuidFilterWithOldSchoolSerialisation : ModuleGuidFilter
		{
			public ModuleGuidFilterWithOldSchoolSerialisation(ZString description, ModuleIdentifier id, SchemaGuidColumn filterColumn, IBusinessObjectCollection list)
				: base(description, id, filterColumn, list)
			{
			}

			protected override void SerializePropertiesToXml(XmlWriter writer)
			{
				if (SupportsComparisonOperatorSet)
				{
					writer.WriteElementString(XmlComparerElement, ComparisonOperator);
				}

				writer.WriteElementString(XmlPropertyElement, Property.ToString());

				if (IsFilterCollectionComparisonOperatorSelected() && SelectedFilters != null)
				{
					var layout = SelectedFilters.SaveLayout("Selected Filter Layout");
					var layoutData = layout.S9_FilterData.ToUTF8();
					var layoutValues = layout.GetLayoutUserData(new FilterStripLayoutsHelper()).S0_FilterDataValues.ToUTF8();

					writer.WriteElementString(XmlPropertySelectedFiltersDataElement, layoutData);
					writer.WriteElementString(XmlPropertySelectedFiltersValuesElement, layoutValues);
					writer.WriteElementString(XmlPropertySelectedFiltersModuleIdElement, layout.S9_ModuleID);
				}
			}
		}

		class DummyFilterBusinessObjectWithGuidFilterWithOldSchoolSerialisation : DummyFilterBusinessObject
		{
			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				var filters = base.GetModuleFiltersCore();
				filters.AddFilter(new ModuleGuidFilterWithOldSchoolSerialisation("Shalala", DummyModuleIDs.Dummy, DummyBizoSchema.Z0_Guid, new DummyBusinessObjectCollection(Factory)));
				return filters;
			}
		}

		#endregion
	}
}
