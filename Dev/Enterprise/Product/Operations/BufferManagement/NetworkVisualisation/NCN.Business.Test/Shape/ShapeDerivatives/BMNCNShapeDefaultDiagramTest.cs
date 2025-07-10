using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(BMNCNShapeDefaultDiagram))]
	class BMNCNShapeDefaultDiagramTest : BaseShapeTestCase
	{
		public void TestClone()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var defaultDiagram = jobHeader.GetDefaultDiagram();
			var shape = workflow1.GetDefaultShape(defaultDiagram);
			var clonedDiagram = (BMNCNShape)defaultDiagram.Clone();

			AssertEquals(true, typeof(BMNCNShapeDefaultDiagram).IsAssignableFrom(defaultDiagram.GetType()));
			AssertEquals(false, typeof(BMNCNShapeDefaultDiagram).IsAssignableFrom(clonedDiagram.GetType()));
		}

		public void TestDeleteWorkflowThatIsAlreadyDeletedShouldStillRemoveFromCache()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var defaultDiagram = jobHeader.GetDefaultDiagram();
			var shape = workflow1.GetDefaultShape(defaultDiagram);
			var viewModel = new WorkflowManagementViewModel(jobHeader);
			AssertEquals(1, defaultDiagram.defaultShapeCache.Count);
			workflow1.Delete();
			AssertEquals(0, defaultDiagram.defaultShapeCache.Count);
		}

		public void TestDefaultShape()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Ano");

			var jobHeaderShape = jobHeader.GetDefaultDiagram();
			AssertNotNull(jobHeaderShape);
			AssertEquals(ShapeTypeList.Codes.DefaultDiagram, jobHeaderShape.BNS_ShapeType);

			var workflowShape = workflow.GetDefaultShape(jobHeaderShape);
			AssertNotNull(workflowShape);
			AssertEquals(ShapeTypeList.Codes.DefaultWorkflow, workflowShape.BNS_ShapeType);

			workflow.Delete();
			AssertEquals(true, workflowShape.IsDeleted);

			jobHeader.Delete();
			AssertEquals(true, jobHeaderShape.IsDeleted);
		}

		public void TestMaxLengthOfCompletionStatement()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Doop");

			var twoFiveSix = new string(Enumerable.Range(0, 256).SelectMany(x => "a").ToArray());
			var twoFiveSeven = new string(Enumerable.Range(0, 257).SelectMany(x => "a").ToArray());
			var twoFiveFive = new string(Enumerable.Range(0, 255).SelectMany(x => "a").ToArray());

			var diagram = jobHeader.GetDefaultDiagram();
			var network = NetworkTestCase.CreateNetwork(diagram);
			var workflowShape = workflow.GetDefaultShape(diagram).AsEntity(network);

			AssertEquals(256, twoFiveSix.Length);
			AssertEquals(257, twoFiveSeven.Length);

			workflow.FH_CompletionStatement = twoFiveSix;

			AssertEquals(twoFiveSix, workflowShape.Name);

			workflow.FH_CompletionStatement = twoFiveSeven;
			AssertEquals(twoFiveSix, workflowShape.Name);

			workflow.FH_CompletionStatement = twoFiveFive;
			AssertEquals(twoFiveFive, workflowShape.Name);
		}

		public void TestSetCompletionStatement_ShouldAddToLastWorkflowInJob()
		{
			VisualBoardsTestHelper.MakeCompletionStatementTaskType("ORG", "COM");

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Ano");
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Ana");
			var workflow3 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Ani");

			workflow1.MakePrerequisiteOf(workflow2);
			workflow2.MakePrerequisiteOf(workflow3);

			workflow1.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew().P9_NotesAsString = "Thing 1 is Done";
			workflow2.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew().P9_NotesAsString = "Thing 2 is Done";
			workflow3.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew().P9_NotesAsString = "Thing 3 is Done";

			var diagram = (IProposedNetworkEntity)jobHeader.GetDefaultDiagram();
			diagram.CompletionCriteria += System.Environment.NewLine + "Dat other thing is done";

			AssertEquals(1, workflow1.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(1, workflow2.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(2, workflow3.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(4, jobHeader.CompletionStatementTasksIncludingChildWorkflowTasks.Count);

			AssertCollectionContains(workflow3.CompletionStatementTasksIncludingChildWorkflowTasks.Cast<ProcessTask>(), t => t.P9_NotesAsString == "Thing 3 is Done");
			AssertCollectionContains(workflow3.CompletionStatementTasksIncludingChildWorkflowTasks.Cast<ProcessTask>(), t => t.P9_NotesAsString == "Dat other thing is done");
		}

		#region Implementation

		protected override IEnumerable<string> XmlMemberNames
		{
			get { return BMNCNShapeTest.BMNCNShape_XmlMemberNames; }
		}

		public override void TestBizObjectFields()
		{
			// TODO: Remove this when BMNCNShapeDefaultDiagram doesn't inherit from dbo.BMNCNShape (can't have a schedule created).
			// Because there are a bunch of fields on the base class that do not get used, this test is an unhappy one, but we can get around this.
			var bizo = (BMNCNShapeDefaultDiagram)GetNewBusinessObject();
			using (bizo.SuspendSettingHasChanges())
			{
				bizo.BNS_ShapeType = ShapeTypeList.Codes.Diagram;
			}
			TestBizObjectFieldsCore(bizo);
			ErrorReporter.Clear();
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
