using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ShownOnDiagramFilter))]
	class ShownOnDiagramFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFilter()
		{
			var processHeaders = Factory.Load<ProcessHeader>(new ZQuery());
			processHeaders.DeleteAll();

			//diagram 1
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 1");
			var workflow1_jobHeader1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader1, "Workflow 1 for jobHeader1");

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 2");
			var workflow1_jobHeader2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader2, "Workflow 1 for jobHeader2");

			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 3");
			var workflow1_jobHeader3 = VisualBoardsTestHelper.CreateWorkflow(jobHeader3, "Workflow 1 for jobHeader3");

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.LinkEntity(diagram, jobHeader1);

			var shape1 = networkViewModel.CreateNewShape(diagram);
			network.LinkEntity(shape1, jobHeader2);

			var shape2 = networkViewModel.CreateNewShape(diagram);
			network.LinkEntity(shape2, workflow1_jobHeader3);

			//diagram 2
			var jobHeader4 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 4");
			var workflow1_jobHeader4 = VisualBoardsTestHelper.CreateWorkflow(jobHeader4, "Workflow 1 for jobHeader4");

			var diagram2 = NetworkTestCase.CreateDiagram(Factory);
			var network2 = NetworkTestCase.CreateNetwork(diagram2);
			network2.LinkEntity(diagram2, jobHeader4);

			// create Process Header which is not shown on any diagram or on that diagram's descendent shape
			var jobHeaderStandalone = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header Standalone");
			var workflow1_jobHeaderStandalone = VisualBoardsTestHelper.CreateWorkflow(jobHeaderStandalone, "Workflow 1 for jobHeaderStandalone");

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var shownOnDiagramFilterStrip = filterBizo.FilterStrips.AddNew();
			shownOnDiagramFilterStrip.FilterDescription = ProcessHeader.ModuleFilterConstants.ShownOnNetworkDiagram;

			// Test 'exact' filter option
			// Finds ProcessHeaders that are shown on the specified network diagram, or any of that diagram's descendent shapes
			var shownOnDiagramFilter = (ShownOnDiagramFilter)shownOnDiagramFilterStrip.CurrentModuleFilter;
			shownOnDiagramFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			shownOnDiagramFilter.Property = diagram.PK;
			shownOnDiagramFilter.IsActive = true;

			processHeaders = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader1, jobHeader2, workflow1_jobHeader3 }, processHeaders);

			//Test 'not equal' filter option
			//Finds ProcessHeaders that are not shown on the specified network diagram, or any of that diagram's descendent shapes
			shownOnDiagramFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			shownOnDiagramFilter.Property = diagram.PK;
			shownOnDiagramFilter.IsActive = true;

			processHeaders = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader3, jobHeader4, workflow1_jobHeader1, workflow1_jobHeader2, workflow1_jobHeader4, jobHeaderStandalone, workflow1_jobHeaderStandalone }, processHeaders);

			//Test 'is blank' filter option
			//Finds ProcessHeaders that are not shown on any network diagram, or any diagram's descendent shapes
			shownOnDiagramFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsBlank;
			shownOnDiagramFilter.IsActive = true;

			processHeaders = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { workflow1_jobHeader1, workflow1_jobHeader2, jobHeader3, workflow1_jobHeader4, jobHeaderStandalone, workflow1_jobHeaderStandalone }, processHeaders);

			//Test 'is not blank' filter option
			//Finds ProcessHeaders that are shown on any network diagram, or any diagram's descendent shapes
			shownOnDiagramFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsNotBlank;
			shownOnDiagramFilter.IsActive = true;

			processHeaders = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader1, jobHeader2, workflow1_jobHeader3, jobHeader4 }, processHeaders);
		}

		public void TestExactNotEqualQueryForMultipleShownOnNetworkDiagramFilters()
		{
			#region test objects setup

			var processHeaders = Factory.Load<ProcessHeader>(new ZQuery());
			processHeaders.DeleteAll();

			//diagram 1: job header 1 is linked to the diagram and workflow1_jobHeader2 is linked to the diagram shape
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 1");
			var workflow1_jobHeader1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader1, "Workflow 1 for jobHeader1");

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 2");
			var workflow1_jobHeader2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader2, "Workflow 1 for jobHeader2");

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.LinkEntity(diagram, jobHeader1);

			var shape1 = networkViewModel.CreateNewShape(diagram);
			network.LinkEntity(shape1, workflow1_jobHeader2);

			//diagram 2: jobHeader3 is linked to the diagram
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 3");
			var workflow1_jobHeader3 = VisualBoardsTestHelper.CreateWorkflow(jobHeader3, "Workflow 1 for jobHeader3");

			var diagram2 = NetworkTestCase.CreateDiagram(Factory);
			var networkViewModel2 = NetworkTestCase.CreateNetworkViewModel(diagram2);
			var network2 = networkViewModel2.GetJobNetwork();
			network2.LinkEntity(diagram2, jobHeader3);

			//diagram 3: jobHeader4 is linked to the diagram shape
			var jobHeader4 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 4");
			var workflow1_jobHeader4 = VisualBoardsTestHelper.CreateWorkflow(jobHeader4, "Workflow 1 for jobHeader4");

			var diagram3 = NetworkTestCase.CreateDiagram(Factory);
			var network3 = NetworkTestCase.CreateNetwork(diagram3);
			network3.LinkEntity(diagram3, jobHeader4);

			// create Process Header which is not shown on any diagram or on that diagram's descendent shape
			var jobHeaderStandalone = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header Standalone");
			var workflow1_jobHeaderStandalone = VisualBoardsTestHelper.CreateWorkflow(jobHeaderStandalone, "Workflow 1 for jobHeaderStandalone");

			Factory.Save();

			#endregion

			var filterBizo = new ProcessHeaderFilterBusinessObject();

			// Test two 'ShownOnNetworkDiagram' filters, both have 'exact' filter option selected
			//filter strip 1
			var shownOnDiagramFilterStrip1 = filterBizo.FilterStrips.AddNew();
			shownOnDiagramFilterStrip1.FilterDescription = ProcessHeader.ModuleFilterConstants.ShownOnNetworkDiagram;

			var shownOnDiagramFilter1 = (ShownOnDiagramFilter)shownOnDiagramFilterStrip1.CurrentModuleFilter;
			shownOnDiagramFilter1.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			shownOnDiagramFilter1.Property = diagram.PK;
			shownOnDiagramFilter1.IsActive = true;

			//filter strip 2
			var shownOnDiagramFilterStrip2 = filterBizo.FilterStrips.AddNew();
			shownOnDiagramFilterStrip2.FilterDescription = ProcessHeader.ModuleFilterConstants.ShownOnNetworkDiagram;

			var shownOnDiagramFilter2 = (ShownOnDiagramFilter)shownOnDiagramFilterStrip2.CurrentModuleFilter;
			shownOnDiagramFilter2.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			shownOnDiagramFilter2.Property = diagram2.PK;
			shownOnDiagramFilter2.IsActive = true;

			#region test 'exact' filter option

			//load filter
			processHeaders = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertEquals("Filter will return empty result because no common workflows on these diagrams", 0, processHeaders.Length);

			//set green category for these filter strips, it means module filter will return workflows are on both diagrams
			shownOnDiagramFilter1.OrCategory = FilterOrCategory.Green;
			shownOnDiagramFilter2.OrCategory = FilterOrCategory.Green;

			processHeaders = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Green filter category: result should contains workflows from diagram1 and diagram2 and should not contains worflows from diagram3",
				new[] { jobHeader1, workflow1_jobHeader2, jobHeader3 }, processHeaders);

			shownOnDiagramFilter1.OrCategory = FilterOrCategory.None;
			shownOnDiagramFilter2.OrCategory = FilterOrCategory.None;

			//have another workflow which is shown on both diagram1 and diagram2
			var commonJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header shown on two diagrams");
			var workflow_commonJobHeader = VisualBoardsTestHelper.CreateWorkflow(jobHeader1, "Workflow for common job");

			//two additional shapes for diagram 1
			var shape2 = networkViewModel.CreateNewShape(diagram);
			network.LinkEntity(shape2, workflow_commonJobHeader);

			var shape3 = networkViewModel.CreateNewShape(diagram);
			network.LinkEntity(shape3, commonJobHeader);

			//additional shape for diagram2
			var shape4 = networkViewModel2.CreateNewShape(diagram2);
			network2.LinkEntity(shape4, workflow_commonJobHeader);
			Factory.Save();

			processHeaders = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Result should contains workflows which are common for diagram 1 and diagram 2",
				new[] { workflow_commonJobHeader }, processHeaders);

			#endregion

			#region test 'NotEqual' filter option

			// Test two 'ShownOnNetworkDiagram' filters, both have 'not equal' filter option selected
			// Filter finds process headers are not shown on these two diagrams
			shownOnDiagramFilter1.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			shownOnDiagramFilter2.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;

			processHeaders = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { workflow1_jobHeader1, jobHeader2, workflow1_jobHeader3, jobHeader4, workflow1_jobHeader4, jobHeaderStandalone, workflow1_jobHeaderStandalone }, processHeaders);

			#endregion

			#region test mixed filter options

			// Test two 'ShownOnNetworkDiagram' filters, one of them has 'exact' filter and second has 'not equal' filter option selected
			// Filter finds process headers are shown on the diagram 1 and not shown on the diagram 2
			shownOnDiagramFilter1.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			shownOnDiagramFilter2.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;

			processHeaders = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { commonJobHeader, workflow1_jobHeader2, jobHeader1 }, processHeaders);

			#endregion
		}

		public void TestFilter_FilterMatch()
		{
			//header attached to the root diagram only and no child chapes
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 1");
			var workflow1_jobHeader1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader1, "Workflow 1 for jobHeader1");

			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Diag 1");
			var network = NetworkTestCase.CreateNetwork(diagram);
			network.LinkEntity(diagram, jobHeader1);

			//header attached to diagram
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 2");
			var workflow1_jobHeader2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader2, "Workflow 1 for jobHeader2");

			//workflow of this header attached to diagram's child shape
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 3");
			var workflow1_jobHeader3 = VisualBoardsTestHelper.CreateWorkflow(jobHeader3, "Workflow 1 for jobHeader3");

			var diagram2 = NetworkTestCase.CreateDiagram(Factory, name: "Diag 2");
			var networkViewModel2 = NetworkTestCase.CreateNetworkViewModel(diagram2);
			var network2 = networkViewModel2.GetJobNetwork();

			var shape1 = networkViewModel2.CreateNewShape(diagram2);
			network2.LinkEntity(shape1, workflow1_jobHeader3);
			network2.LinkEntity(diagram2, jobHeader2);

			//header attached to the child chape
			var jobHeader4 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 4");
			var workflow1_jobHeader4 = VisualBoardsTestHelper.CreateWorkflow(jobHeader4, "Workflow 1 for jobHeader4");

			var diagram3 = NetworkTestCase.CreateDiagram(Factory, name: "Diag 3");
			var networkViewModel3 = NetworkTestCase.CreateNetworkViewModel(diagram3);
			var network3 = networkViewModel3.GetJobNetwork();

			var shape1_diagram3 = networkViewModel3.CreateNewShape(diagram3);
			network3.LinkEntity(shape1_diagram3, jobHeader4);

			//additional level of nesting - header attached to diagram and header atached to sub-diagram and workflow attached to the child chape of this sub-diagram
			var jobHeader5 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 5");
			var workflow1_jobHeader5 = VisualBoardsTestHelper.CreateWorkflow(jobHeader5, "Workflow 1 for jobHeader5");

			var jobHeader6 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 6");
			var workflow1_jobHeader6 = VisualBoardsTestHelper.CreateWorkflow(jobHeader6, "Workflow 1 for jobHeader6");

			var diagram4 = NetworkTestCase.CreateDiagram(Factory, name: "Diag 4");
			var networkViewModel4 = NetworkTestCase.CreateNetworkViewModel(diagram4);
			var network4 = networkViewModel4.GetJobNetwork();

			var parentShape = networkViewModel4.CreateNewShape(diagram4);
			var shape1_parentShape = networkViewModel4.CreateNewShape(parentShape);

			network4.LinkEntity(diagram4, jobHeader5);
			network4.LinkEntity(parentShape, jobHeader6);
			network4.LinkEntity(shape1_parentShape, workflow1_jobHeader6);

			Factory.Save();

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.NetworkDiagram))
			{
				var networkDiagramFilterBusinessObject = module.FilterBusinessObject;

				var networkDiagramNameFilterStrip = networkDiagramFilterBusinessObject.FilterStrips.AddNew("Name");
				var networkDiagramNameFilterStripCurrentFilter = (ModuleTextFilter)networkDiagramNameFilterStrip.CurrentModuleFilter;

				AssertFilterResults(networkDiagramNameFilterStripCurrentFilter, networkDiagramFilterBusinessObject, "Diag 2",
					new ProcessHeader[] { jobHeader2, workflow1_jobHeader3 }, "BNS_Name = 'Diag 2' and BNS_ShapeType = 'DIA'");

				AssertFilterResults(networkDiagramNameFilterStripCurrentFilter, networkDiagramFilterBusinessObject, "Diag 1",
					new ProcessHeader[] { jobHeader1 }, "BNS_Name = 'Diag 1' and BNS_ShapeType = 'DIA'");

				AssertFilterResults(networkDiagramNameFilterStripCurrentFilter, networkDiagramFilterBusinessObject, "Diag 3",
					new ProcessHeader[] { jobHeader4 }, "BNS_Name = 'Diag 3' and BNS_ShapeType = 'DIA'");

				AssertFilterResults(networkDiagramNameFilterStripCurrentFilter, networkDiagramFilterBusinessObject, "Diag 4",
					new ProcessHeader[] { jobHeader5, jobHeader6, workflow1_jobHeader6 }, "BNS_Name = 'Diag 4' and BNS_ShapeType = 'DIA'");

				AssertFilterResults(networkDiagramNameFilterStripCurrentFilter, networkDiagramFilterBusinessObject, "Diag",
					new ProcessHeader[] { jobHeader2, workflow1_jobHeader3, jobHeader1, jobHeader4, jobHeader5, jobHeader6, workflow1_jobHeader6 },
					"BNS_Name like 'Diag%' and BNS_ShapeType = 'DIA'", ModuleTextFilter.ComparisonConstants.StartsWith);
			}
		}

		void AssertFilterResults(ModuleTextFilter filter, FilterStripBusinessObject filterBusinessObject, ZString property, IEnumerable<ProcessHeader> elementsIn, ZString expectedFilterPart, string comparisonOperator = ModuleTextFilter.ComparisonConstants.Exact)
		{
			filter.ComparisonOperator = comparisonOperator;
			filter.Property = property;
			AssertEquals("This should give us a filter by Name for Network Diagrams", expectedFilterPart, filterBusinessObject.Filter.LiteralTextADO);

			var query = SetUpProcessHeaderFilterBOAndGetShownOnDiagramFilter(filterBusinessObject);
			var processHeaders = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder(elementsIn, processHeaders);
		}

		static ZQuery SetUpProcessHeaderFilterBOAndGetShownOnDiagramFilter(FilterStripBusinessObject subFilters)
		{
			var processHeaderFilterBO = new ProcessHeaderFilterBusinessObject();
			var shownOnDiagramFilter = (ShownOnDiagramFilter)processHeaderFilterBO[ProcessHeader.ModuleFilterConstants.ShownOnNetworkDiagram];
			shownOnDiagramFilter.IsActive = true;
			shownOnDiagramFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.FiltersMatch;
			shownOnDiagramFilter.UpdateSelectedFilters(subFilters);
			return processHeaderFilterBO.Filter;
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			return (ShownOnDiagramFilter)filterBizo[ProcessHeader.ModuleFilterConstants.ShownOnNetworkDiagram];
		}

		#endregion
	}
}
