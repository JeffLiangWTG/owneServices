using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Support.Testing
{
	internal sealed class ModuleSelectionTest : TestCaseWithFactory
	{
		internal class DummyChildBusinessObjectExcludable : DummyChildBusinessObject, ICanBeExcludedFromOperationalActions
		{
			public DummyChildBusinessObjectExcludable(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool ShouldExclude { get; set; }

			public string ReasonForExclusion
			{
				get
				{
					return "!?!";
				}
			}
		}

		[RequiresSTA]
		public void TestGetSelectedRecordPKs()
		{
			using (ZFilterGridModule module = new DummyModuleWithActionsSupport<DummyBusinessObjectWithDocumentSupport>())
			using (EmbeddedModulePopup popup = new EmbeddedModulePopup(module))
			{
				popup.Show();
				Application.DoEvents();
				ModuleSelection selection = new ModuleSelection(module);
				AssertContainsExactElementsInAnyOrder("precondition:", System.Array.Empty<ZGuid>(), selection.GetSelectedRecords().PrimaryKeys);
				DummyChildBusinessObjectExcludable dummy1 = Factory.New<DummyChildBusinessObjectExcludable>();
				module.GridCollection.Add(dummy1);
				DummyChildBusinessObjectExcludable dummy2 = Factory.New<DummyChildBusinessObjectExcludable>();
				module.GridCollection.Add(dummy2);
				DummyChildBusinessObjectExcludable dummy3 = Factory.New<DummyChildBusinessObjectExcludable>();
				module.GridCollection.Add(dummy3);
				DummyChildBusinessObjectExcludable dummy4 = Factory.New<DummyChildBusinessObjectExcludable>();
				module.GridCollection.Add(dummy4);
				DummyChildBusinessObjectExcludable dummy5 = Factory.New<DummyChildBusinessObjectExcludable>();
				module.GridCollection.Add(dummy5);
				Dictionary<ZGuid, string> map = new Dictionary<ZGuid, string>();
				map[dummy1.PK] = "dummy1";
				map[dummy2.PK] = "dummy2";
				map[dummy3.PK] = "dummy3";
				map[dummy4.PK] = "dummy4";
				map[dummy5.PK] = "dummy5";
				// PKs should always appear in the same order as the grid.
				const string expected1 = @"
AutoSelectedAllKeys: True
  dummy1
  dummy2
  dummy3
  dummy4
  dummy5
";
				const string expected2 = @"
AutoSelectedAllKeys: False
  dummy2
  dummy3
  dummy4
";
				AssertMultilineASCIIEquals("", expected1, TestHelper.RenderSelection(map, selection.GetSelectedRecords()));
				AssertEquals(0, selection.ExclusionReasons.Count);
				TestHelper.SelectExact((ZDisplayGrid)module.DisplayGrid, dummy2, dummy3, dummy4);
				AssertMultilineASCIIEquals("", expected2, TestHelper.RenderSelection(map, selection.GetSelectedRecords()));
				AssertEquals(0, selection.ExclusionReasons.Count);
				dummy2.ShouldExclude = true;
				dummy4.ShouldExclude = true;
				const string expected3 = @"
AutoSelectedAllKeys: True
  dummy1
  dummy3
  dummy5
";
				TestHelper.SelectExact((ZDisplayGrid)module.DisplayGrid);
				AssertMultilineASCIIEquals("", expected3, TestHelper.RenderSelection(map, selection.GetSelectedRecords()));
				AssertEquals(2, selection.ExclusionReasons.Count);
				AssertEquals("!?!", selection.ExclusionReasons[0]);
				AssertEquals("!?!", selection.ExclusionReasons[1]);
				const string expected4 = @"
AutoSelectedAllKeys: False
  dummy3
";
				TestHelper.SelectExact((ZDisplayGrid)module.DisplayGrid, dummy2, dummy3, dummy4);
				AssertMultilineASCIIEquals("", expected4, TestHelper.RenderSelection(map, selection.GetSelectedRecords()));
				AssertEquals(2, selection.ExclusionReasons.Count);
				AssertEquals("!?!", selection.ExclusionReasons[0]);
				AssertEquals("!?!", selection.ExclusionReasons[1]);
				const string expected5 = @"
AutoSelectedAllKeys: False
";
				TestHelper.SelectExact((ZDisplayGrid)module.DisplayGrid, dummy2, dummy4);
				AssertMultilineASCIIEquals("", expected5, TestHelper.RenderSelection(map, selection.GetSelectedRecords()));
				AssertEquals(2, selection.ExclusionReasons.Count);
				AssertEquals("!?!", selection.ExclusionReasons[0]);
				AssertEquals("!?!", selection.ExclusionReasons[1]);
			}
		}

		[RequiresSTA]
		public void TestGetAllFilterRecords_WithoutRelationshipFilter()
		{
			using (var module = new DummyFilterGridModule())
			using (var popup = new EmbeddedModulePopup(module))
			{
				popup.Show();
				var moduleSelection = new ModuleSelection(module);
				var selectedRecords = moduleSelection.GetAllFilterRecords(typeof(DummyBusinessObject));
				AssertEquals("Precondition: module has nothing.", false, selectedRecords.Any());
				var dummy1 = Factory.New<DummyBusinessObject>();
				dummy1.Z0_Bool = true;
				var dummy2 = Factory.New<DummyBusinessObject>();
				dummy2.Z0_Bool = false;
				var dummy3 = Factory.New<DummyBusinessObject>();
				dummy3.Z0_Bool = true;
				Factory.Save();
				selectedRecords = moduleSelection.GetAllFilterRecords(typeof(DummyBusinessObject));
				AssertEquals(true, selectedRecords.Any());
				AssertEquals("GetAllFilterRecords should return all 3 records.", 3, selectedRecords.Single().PrimaryKeys.Length);
			}
		}

		[RequiresSTA]
		public void TestGetAllFilterRecords_WithRelationshipFilter()
		{
			using (var module = new DummyFilterGridModuleWithActiveBOC())
			{
				module.RelationshipFilter = new ZQuery(DummyBizoSchema.Z0_Bool, true);
				using (var popup = new EmbeddedModulePopup(module))
				{
					popup.Show();
					var moduleSelection = new ModuleSelection(module);
					var selectedRecords = moduleSelection.GetAllFilterRecords(typeof(DummyBusinessObject));
					AssertEquals("Precondition: module has nothing.", false, selectedRecords.Any());
					var dummy1 = Factory.New<DummyBusinessObject>();
					dummy1.Z0_Bool = true;
					var dummy2 = Factory.New<DummyBusinessObject>();
					dummy2.Z0_Bool = false;
					var dummy3 = Factory.New<DummyBusinessObject>();
					dummy3.Z0_Bool = true;
					Factory.Save();
					selectedRecords = moduleSelection.GetAllFilterRecords(typeof(DummyBusinessObject));
					AssertEquals(true, selectedRecords.Any());
					AssertEquals("GetAllFilterRecords should only return 2 records due to the relationship filter.", 2, selectedRecords.Single().PrimaryKeys.Length);
					AssertContainsExactElementsInAnyOrder("Should return 2 records - dummy1 and dummy3.", new ZGuid[] { dummy1.PK, dummy3.PK }, selectedRecords.Single().PrimaryKeys);
				}
			}
		}

		[RequiresSTA]
		public void TestGetAllFilterRecords_InvalidSQL()
		{
			using (var module = new DummyFilterGridModule())
			using (var popup = new EmbeddedModulePopup(module))
			{
				popup.Show();
				var dummy = Factory.New<DummyBusinessObject>();
				dummy.Z0_Bool = true;
				Factory.Save();
				var filterBizO = module.FilterBusinessObject;
				var query = $"(1=1";
				var sqlFilter = new ModuleSQLFilter($"Custom SQL ({query})", filterBizO.QueryObjectType)
				{ Property1 = query, Category = FilterCategories.Other, IsActive = true };
				filterBizO.ModuleFilters.AddFilter(sqlFilter);
				filterBizO.RegisterEditableChildObject(sqlFilter);
				var moduleSelection = new ModuleSelection(module);
				var selectedRecords = moduleSelection.GetAllFilterRecords(typeof(DummyBusinessObject)).ToList();
				AssertEquals(0, selectedRecords.Count);
				AssertContains("There are errors in the filter", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestFilterRowCount_ValidSQL()
		{
			using (var module = new DummyFilterGridModule())
			using (var popup = new EmbeddedModulePopup(module))
			{
				popup.Show();
				var dummy = Factory.New<DummyBusinessObject>();
				dummy.Z0_Bool = true;
				Factory.Save();
				var filterBizO = module.FilterBusinessObject;
				var query = $"1=1";
				var sqlFilter = new ModuleSQLFilter($"Custom SQL ({query})", filterBizO.QueryObjectType)
				{ Property1 = query, Category = FilterCategories.Other, IsActive = true };
				filterBizO.ModuleFilters.AddFilter(sqlFilter);
				filterBizO.RegisterEditableChildObject(sqlFilter);
				var moduleSelection = new ModuleSelection(module);
				var selectedRecordsCount = moduleSelection.FilterRowCount;
				AssertEquals(1, selectedRecordsCount);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestFilterRowCount_InvalidSQL()
		{
			using (var module = new DummyFilterGridModule())
			using (var popup = new EmbeddedModulePopup(module))
			{
				popup.Show();
				var dummy = Factory.New<DummyBusinessObject>();
				dummy.Z0_Bool = true;
				Factory.Save();
				var filterBizO = module.FilterBusinessObject;
				var query = $"(1=1";
				var sqlFilter = new ModuleSQLFilter($"Custom SQL ({query})", filterBizO.QueryObjectType)
				{ Property1 = query, Category = FilterCategories.Other, IsActive = true };
				filterBizO.ModuleFilters.AddFilter(sqlFilter);
				filterBizO.RegisterEditableChildObject(sqlFilter);
				var moduleSelection = new ModuleSelection(module);
				var selectedRecordsCount = moduleSelection.FilterRowCount;
				AssertEquals(0, selectedRecordsCount);
				AssertContains("There are errors in the filter", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
