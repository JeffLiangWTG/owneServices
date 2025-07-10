using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.GUI.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Module.Test
{
	[TestedType(typeof(NetworkDiagramFilterBusinessObject))]
	class NetworkDiagramFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestDiagramTypeFilter_Diagram()
		{
			using (var module = new NetworkDiagramModule())
			{
				var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
				var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "StopWorkFlow");

				// Searchable shapes
				var defaultDiagram = NetworkTestCase.CreateDefaultDiagram(jobHeader, name: "defaultDiagram");
				var diagram = NetworkTestCase.CreateDiagram(jobHeader, name: "diagram");
				var shape = NetworkTestCase.CreateShape(workflow, diagram, name: "shape");
				var buffer = NetworkTestCase.CreateShape(diagram, name: "buffer", shapeType: ShapeTypeList.Codes.Buffer);

				// Non-searchable shapes
				var annotation = NetworkTestCase.CreateShape(diagram, name: "annotation", shapeType: ShapeTypeList.Codes.Annotation);
				var defaultWorkflow = NetworkTestCase.CreateShape(defaultDiagram, name: "defaultWorkflow", shapeType: ShapeTypeList.Codes.DefaultWorkflow);

				var allShapes = new[] { defaultDiagram, diagram, shape, annotation, defaultWorkflow, buffer };
				AssertEquals("All shape types should be tested here. If any new types are added, consider whether they should appear in the Network Diagrams module by default or excluded.", allShapes.Length, new ShapeTypeList().Count);

				Factory.Save();

				var bizo = module.FilterBusinessObject;

				var filter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.DiagramType];
				filter.Property = ShapeTypeList.Codes.DefaultDiagram;
				filter.IsActive = true;

				AssertEquals(filter.Visibility, FilterVisibility.AlwaysApplied);
				AssertEquals(DiagramShapeTypeList.Codes.Diagram, filter.DefaultProperty);
				AssertEquals(true, module.AllowNew);

				AssertEquals("Shape Type", filter.MultilingualDescription);

				var results = module.FindMatchingResults_ForTest<BMNCNShape>(Factory);
				AssertContainsExactElementsInAnyOrder(new[] { defaultDiagram }, results);

				filter.Property = ShapeTypeList.Codes.Diagram;
				results = module.FindMatchingResults_ForTest<BMNCNShape>(Factory);
				AssertContainsExactElementsInAnyOrder(new[] { diagram }, results);

				filter.Property = ShapeTypeList.Codes.Shape;
				results = module.FindMatchingResults_ForTest<BMNCNShape>(Factory);
				AssertContainsExactElementsInAnyOrder("It's possible to specify that shapes are included in the search", new[] { shape }, results);

				filter.Property = ShapeTypeList.Codes.Buffer;
				results = module.FindMatchingResults_ForTest<BMNCNShape>(Factory);
				AssertContainsExactElementsInAnyOrder("It's possible to specify that buffers are included in the search", new[] { buffer }, results);

				filter.Property = string.Empty;
				results = module.FindMatchingResults_ForTest<BMNCNShape>(Factory);
				AssertContainsExactElementsInAnyOrder(new[] { defaultDiagram, diagram, shape, buffer }, results);

				filter.IsActive = false;
				results = module.FindMatchingResults_ForTest<BMNCNShape>(Factory);
				AssertContainsExactElementsInAnyOrder("The module should include only diagrams by default", new[] { diagram }, results);
			}
		}

		public void TestDiagramTypeFilter_Shape()
		{
			using (var module = new NetworkDiagramModule())
			{
				var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
				var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "StopWorkFlow");

				// Searchable shapes
				var defaultDiagram = NetworkTestCase.CreateDefaultDiagram(jobHeader, name: "defaultDiagram");
				var diagram = NetworkTestCase.CreateDiagram(jobHeader, name: "diagram");
				var shape = NetworkTestCase.CreateShape(workflow, diagram, name: "shape");
				var buffer = NetworkTestCase.CreateShape(diagram, name: "buffer", shapeType: ShapeTypeList.Codes.Buffer);

				// Non-searchable shapes
				var annotation = NetworkTestCase.CreateShape(diagram, name: "annotation", shapeType: ShapeTypeList.Codes.Annotation);
				var defaultWorkflow = NetworkTestCase.CreateShape(defaultDiagram, name: "defaultWorkflow", shapeType: ShapeTypeList.Codes.DefaultWorkflow);

				module.AddAdditionalDisplayFilter = q => new ZQuery();

				Factory.Save();

				var bizo = module.FilterBusinessObject;

				var filter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.DiagramType];
				filter.Property = ShapeTypeList.Codes.DefaultDiagram;
				filter.IsActive = true;

				AssertEquals(filter.Visibility, FilterVisibility.Visible);
				AssertEquals(DiagramShapeTypeList.Codes.Shape, filter.DefaultProperty);
				AssertEquals(false, module.AllowNew);

				AssertEquals("Shape Type", filter.MultilingualDescription);

				var results = module.FindMatchingResults_ForTest<BMNCNShape>(Factory);
				AssertContainsExactElementsInAnyOrder(new[] { defaultDiagram }, results);

				filter.Property = ShapeTypeList.Codes.Diagram;
				results = module.FindMatchingResults_ForTest<BMNCNShape>(Factory);
				AssertContainsExactElementsInAnyOrder(new[] { diagram }, results);

				filter.Property = ShapeTypeList.Codes.Shape;
				results = module.FindMatchingResults_ForTest<BMNCNShape>(Factory);
				AssertContainsExactElementsInAnyOrder("It's possible to specify that shapes are included in the search", new[] { shape }, results);

				filter.Property = ShapeTypeList.Codes.Buffer;
				results = module.FindMatchingResults_ForTest<BMNCNShape>(Factory);
				AssertContainsExactElementsInAnyOrder("It's possible to specify that buffers are included in the search", new[] { buffer }, results);

				filter.Property = string.Empty;
				results = module.FindMatchingResults_ForTest<BMNCNShape>(Factory);
				AssertContainsExactElementsInAnyOrder(new[] { defaultDiagram, diagram, shape, buffer }, results);

				filter.IsActive = false;
				results = module.FindMatchingResults_ForTest<BMNCNShape>(Factory);
				AssertContainsExactElementsInAnyOrder("The module should still return all the shapes since the filter is not 'always applied'",
					new[] { defaultDiagram, diagram, shape, buffer }, results);
			}
		}

		public void TestChildEntityFilter()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var diagram = NetworkTestCase.CreateDiagram(jobHeader);
			var childShape = NetworkTestCase.CreateShape(workflow1, diagram);

			Factory.Save();

			var bizo = new NetworkDiagramFilterBusinessObject();

			var filter = (ModuleGuidFilter)bizo[BMNCNShape.ModuleFilterConstants.ChildEntity];
			filter.Property = workflow1.PK;
			filter.IsActive = true;

			var results = Factory.Load<BMNCNShape>(bizo.Filter);
			AssertEquals(1, results.Length);
			AssertEquals(diagram, results[0]);

			filter.Property = workflow2.PK;
			results = Factory.Load<BMNCNShape>(bizo.Filter);
			AssertEquals(0, results.Length);
		}

		public void TestChildNameFilter_ShapeName()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var diagram = NetworkTestCase.CreateDiagram(jobHeader);
			var childShape = NetworkTestCase.CreateShape(workflow1, diagram, name: "Booo");

			Factory.Save();

			var bizo = new NetworkDiagramFilterBusinessObject();

			var filter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.ChildName];
			filter.Property = "Booo";
			filter.IsActive = true;

			var results = Factory.Load<BMNCNShape>(bizo.Filter);
			AssertEquals(1, results.Length);
			AssertEquals(diagram, results[0]);

			filter.Property = "Nope";
			results = Factory.Load<BMNCNShape>(bizo.Filter);
			AssertEquals(0, results.Length);
		}

		public void TestChildNameFilter_WorkflowName()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "Foo";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Bar";

			var diagram = NetworkTestCase.CreateDiagram(jobHeader);
			var childShape = NetworkTestCase.CreateShape(workflow1, diagram, name: "Booo");

			Factory.Save();

			var bizo = new NetworkDiagramFilterBusinessObject();

			var filter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.ChildName];
			filter.Property = "Booo";
			filter.IsActive = true;

			var results = Factory.Load<BMNCNShape>(bizo.Filter);
			AssertEquals(1, results.Length);
			AssertEquals(diagram, results[0]);

			filter.Property = "Foo";
			results = Factory.Load<BMNCNShape>(bizo.Filter);
			AssertEquals(1, results.Length);
			AssertEquals(diagram, results[0]);

			filter.Property = "Bar";
			results = Factory.Load<BMNCNShape>(bizo.Filter);
			AssertEquals(0, results.Length);
		}

		#region Status Filter

		public void TestStatusFilter()
		{
			var assignedDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Cool");
			assignedDiagram.BNS_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var closedDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Grump");
			closedDiagram.BNS_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			var bizo = new NetworkDiagramFilterBusinessObject();

			var filter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.Status];
			filter.Property = ProcessTaskStatusCodeList.Codes.Assigned;
			filter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(new[] { assignedDiagram }, Factory.Load<BMNCNShape>(bizo.Filter));
		}

		public void TestStatusFilter_BackedByProcessHeader_Complete()
		{
			AssertStatus(NetworkDiagramStatusFilterList.Codes.Complete, "closed", "cancelledAndClosed", "cancelled");
			AssertUnboundShapeStatus(NetworkDiagramStatusFilterList.Codes.Complete, ShapeStatusList.Codes.Closed, ShapeStatusList.Codes.Cancelled);
		}

		public void TestStatusFilter_BackedByProcessHeader_NotComplete()
		{
			AssertStatus(NetworkDiagramStatusFilterList.Codes.NotComplete, "open", "job of assigned and closed", "assigned", "working", "suspended");
			AssertUnboundShapeStatus(NetworkDiagramStatusFilterList.Codes.NotComplete, ShapeStatusList.Codes.Open, ShapeStatusList.Codes.Assigned, ShapeStatusList.Codes.Working, ShapeStatusList.Codes.Suspended);
		}

		public void TestStatusFilter_BackedByProcessHeader_NotSpecified()
		{
			AssertStatus(NetworkDiagramStatusFilterList.Codes.NotSpecified, "unknown");
			AssertUnboundShapeStatus(NetworkDiagramStatusFilterList.Codes.NotSpecified, ShapeStatusList.Codes.Unknown);
		}

		public void TestStatusFilter_BackedByProcessHeader_Assigned()
		{
			AssertStatus(ShapeStatusList.Codes.Assigned, "job of assigned and closed", "assigned", "suspended", "working");
		}

		public void TestStatusFilter_BackedByProcessHeader_Open()
		{
			AssertStatus(ShapeStatusList.Codes.Open, "open");
		}

		public void TestStatusFilter_BackedByProcessHeader_Suspended()
		{
			AssertStatus(ShapeStatusList.Codes.Suspended, "suspended");
		}

		public void TestStatusFilter_BackedByProcessHeader_Working()
		{
			AssertStatus(ShapeStatusList.Codes.Working, "working");
		}

		public void TestStatusFilter_BackedByProcessHeader_Cancelled()
		{
			AssertStatus(ShapeStatusList.Codes.Cancelled, "cancelled");
		}

		public void TestStatusFilter_BackedByProcessHeader_Closed()
		{
			AssertStatus(ShapeStatusList.Codes.Closed, "closed", "cancelledAndClosed", "cancelled");
		}

		public void TestStatusFilter_BackedByProcessHeader_Unknown()
		{
			AssertStatus(ShapeStatusList.Codes.Unknown, "unknown");
		}

		public void TestStatusFilter_BackedByProcessHeader_Invalid()
		{
			var shapes = SetupStatusTestData();
			var bizo = new NetworkDiagramFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.Status];
			statusFilter.Property = "ZZW";
			statusFilter.IsActive = true;

			var diagramTypeFilter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.DiagramType];
			diagramTypeFilter.IsActive = true;
			diagramTypeFilter.Property = "";

			AssertContainsExactElementsInAnyOrder(new Converter<BMNCNShape, string>(s => s.BNS_Name), shapes.Select(s => s.Value).ToArray(), Factory.Load<BMNCNShape>(bizo.Filter));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		Dictionary<string, BMNCNShape> SetupStatusTestData()
		{
			var staff = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var assignedHeader = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "assignedHeader");
			VisualBoardsTestHelper.CreateTask(assignedHeader, staff.GS_Code, 60);

			var closedHeader = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "closedHeader");
			VisualBoardsTestHelper.CreateTask(closedHeader, staff.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			var workingJobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workingWorkflow = VisualBoardsTestHelper.CreateWorkflow(workingJobHeader, "WorkingWorkflow");
			VisualBoardsTestHelper.CreateTask(workingWorkflow, staff.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working);

			var cancelledAndClosedJobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var cancelledAndClosedWorkflow = VisualBoardsTestHelper.CreateWorkflow(cancelledAndClosedJobHeader, "cancelledAndClosedWorkflow");
			VisualBoardsTestHelper.CreateTask(cancelledAndClosedWorkflow, staff.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);
			VisualBoardsTestHelper.CreateTask(cancelledAndClosedWorkflow, staff.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			var cancelledJobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var cancelledWorkflow = VisualBoardsTestHelper.CreateWorkflow(cancelledJobHeader, "cancelledWorkflow");
			VisualBoardsTestHelper.CreateTask(cancelledWorkflow, staff.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);

			var openJobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var openWorkflow = VisualBoardsTestHelper.CreateWorkflow(openJobHeader, "openWorkflow");
			VisualBoardsTestHelper.CreateTask(openWorkflow, staff.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			var suspendedJobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var suspendedWorkflow = VisualBoardsTestHelper.CreateWorkflow(suspendedJobHeader, "suspendedWorkflow");
			VisualBoardsTestHelper.CreateTask(suspendedWorkflow, staff.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended);

			var jobDiagram = NetworkTestCase.CreateDiagram(jobHeader, name: "job of assigned and closed");

			var result = new[]
			{
				jobDiagram,
				NetworkTestCase.CreateShape(assignedHeader, parentShape: jobDiagram, name: "assigned"),
				NetworkTestCase.CreateShape(closedHeader, parentShape: jobDiagram, name: "closed"),
				NetworkTestCase.CreateDiagram(workingJobHeader, name: "working"),
				NetworkTestCase.CreateDiagram(openJobHeader, name: "open"),
				NetworkTestCase.CreateDiagram(cancelledAndClosedJobHeader, name: "cancelledAndClosed"),
				NetworkTestCase.CreateDiagram(cancelledJobHeader, name: "cancelled"),
				NetworkTestCase.CreateDiagram(suspendedJobHeader, name: "suspended"),
				NetworkTestCase.CreateDiagram(Factory, name: "unknown"),
			}.ToDictionary(s => (string)s.BNS_Name);

			Factory.Save();

			return result;
		}

		void AssertStatus(string status, params string[] expectedShapes)
		{
			var shapes = SetupStatusTestData();
			var bizo = new NetworkDiagramFilterBusinessObject();

			var statusFilter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.Status];
			statusFilter.Property = status;
			statusFilter.IsActive = true;

			var diagramTypeFilter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.DiagramType];
			diagramTypeFilter.IsActive = true;
			diagramTypeFilter.Property = "";

			AssertContainsExactElementsInAnyOrder(new Converter<BMNCNShape, string>(s => s.BNS_Name), expectedShapes.Select(e => shapes[e]).ToArray(), Factory.Load<BMNCNShape>(bizo.Filter));
		}

		void AssertUnboundShapeStatus(string filterStripCode, params string[] expectedStatus)
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var diagram = NetworkTestCase.CreateDiagram(jobHeader);

			foreach (var status in new ShapeStatusList().GetAllCodes())
			{
				var shape = NetworkTestCase.CreateShape(diagram, name: "Bonk");
				shape.BNS_Status = status;
			}

			Factory.Save();

			var bizo = new NetworkDiagramFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.Status];
			statusFilter.Property = filterStripCode;
			statusFilter.IsActive = true;

			var diagramTypeFilter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.DiagramType];
			diagramTypeFilter.IsActive = true;
			diagramTypeFilter.Property = "";

			var nameFilter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.Name];
			nameFilter.Property = "Bonk";
			nameFilter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(expectedStatus, Factory.Load<BMNCNShape>(bizo.Filter).Select(s => (string)s.BNS_Status).Distinct().ToArray());
		}

		#endregion

		#endregion

		#region Scaled Filter
		public void TestScaledFilterForScaledDiagrams()
		{
			var scaledDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Scaled Diagram", isScaled: true);
			var nonScaledDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Non-Scaled Diagram");

			Factory.Save();

			var bizo = new NetworkDiagramFilterBusinessObject();

			var filter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.Scaled];
			filter.Property = NetworkDiagramScaledFilterOptions.Codes.Scaled;
			filter.IsActive = true;

			var results = Factory.Load<BMNCNShape>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("Using this query: " + bizo.Filter.LiteralTextSqlFormatted + "has returned the incorrect diagram(s)", new[] { scaledDiagram }, results);
		}

		public void TestScaledFilterForNonScaledDiagrams()
		{
			var scaledDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Scaled Diagram", isScaled: true);
			var nonScaledDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Non-Scaled Diagram");

			Factory.Save();

			var bizo = new NetworkDiagramFilterBusinessObject();

			var filter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.Scaled];
			filter.Property = NetworkDiagramScaledFilterOptions.Codes.NonScaled;
			filter.IsActive = true;

			var results = Factory.Load<BMNCNShape>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("Using this query: " + bizo.Filter.LiteralTextSqlFormatted + "has returned the incorrect diagram(s)", new[] { nonScaledDiagram }, results);
		}

		public void TestScaledFiltersForBoth()
		{
			var scaledDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Scaled Diagram", isScaled: true);
			var nonScaledDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Non-Scaled Diagram");

			Factory.Save();

			var bizo = new NetworkDiagramFilterBusinessObject();

			var filter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.Scaled];
			filter.Property = NetworkDiagramScaledFilterOptions.Codes.Both;
			filter.IsActive = true;

			var results = Factory.Load<BMNCNShape>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("Using this query: " + bizo.Filter.LiteralTextSqlFormatted + "has returned the incorrect diagram(s)", new[] { scaledDiagram, nonScaledDiagram }, results);
		}

		public void TestScaledFilterForBothUsesZQuery()
		{
			var bizo = new NetworkDiagramFilterBusinessObject();

			var filter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.Scaled];
			filter.Property = NetworkDiagramScaledFilterOptions.Codes.Both;
			filter.IsActive = true;

			Assert("The query used for this filter is of ZDBOnlyQuery type, meaning it could be optimised.", filter.Query.GetType() == typeof(ZQuery));
		}

		public void TestScaledFilterForScaledOrNonScaledUsesZDBOnlyQuery()
		{
			var bizo = new NetworkDiagramFilterBusinessObject();

			var filter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.Scaled];
			filter.Property = NetworkDiagramScaledFilterOptions.Codes.Scaled;
			filter.IsActive = true;

			Assert("The query used for this filter is not of ZDBOnlyQuery type as it should be.", filter.Query.GetType() == typeof(ZDBOnlyQuery));

			filter.Property = NetworkDiagramScaledFilterOptions.Codes.NonScaled;

			Assert("The query used for this filter is not of ZDBOnlyQuery type as it should be", filter.Query.GetType() == typeof(ZDBOnlyQuery));
		}

		public void TestEdgeCaseDiagramsWithScheduleButAreNotScaledScaledFilter()
		{
			var scaledThenNonScaledDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Scaled Then Non-Scaled Diagram", isScaled: true);

			Factory.Save();

			scaledThenNonScaledDiagram.IsScaled = false;

			Factory.Save();

			var bizo = new NetworkDiagramFilterBusinessObject();

			var filter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.Scaled];
			filter.Property = NetworkDiagramScaledFilterOptions.Codes.Scaled;
			filter.IsActive = true;

			var results = Factory.Load<BMNCNShape>(bizo.Filter);
			Assert("Using this query: " + bizo.Filter.LiteralTextSqlFormatted + "has returned the incorrect diagram(s)", !results.Contains(scaledThenNonScaledDiagram));
		}

		public void TestEdgeCaseDiagramsWithScheduleButAreNotScaledNonScaledFilter()
		{
			var scaledThenNonScaledDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Scaled Then Non-Scaled Diagram", isScaled: true);

			Factory.Save();

			scaledThenNonScaledDiagram.IsScaled = false;

			Factory.Save();

			var bizo = new NetworkDiagramFilterBusinessObject();

			var filter = (ModuleTextFilter)bizo[BMNCNShape.ModuleFilterConstants.Scaled];
			filter.Property = NetworkDiagramScaledFilterOptions.Codes.NonScaled;
			filter.IsActive = true;

			var results = Factory.Load<BMNCNShape>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("Using this query: " + bizo.Filter.LiteralTextSqlFormatted + "has returned the incorrect diagram(s)", new[] { scaledThenNonScaledDiagram }, results);
		}

		#endregion

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new NetworkDiagramFilterBusinessObject();
		}
	}
}
