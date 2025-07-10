using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ApprovedWorkflowScheduledTimeFilter))]
	class ApprovedWorkflowScheduledTimeFilterTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2014, 6, 12)]
		public void TestFilter()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader, name: "diagram");
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram, "shape2");

			var arrow1_2 = shape1.MakeVisiblePrerequisiteOf(shape2);

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			var shape3 = networkViewModel.CreateNewShape(diagram);
			network.CreateRelationship(shape2, shape3);

			shape3.X = shape2.X + shape2.Width;
			shape3.Width = shape2.Width;

			AssertEquals(0.0, shape1.X);
			AssertEquals(300.0, shape2.X);
			AssertEquals(600.0, shape3.X);

			AssertEquals(ZDateTime.UtcNow, shape1.Shape.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(5), shape2.Shape.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(8), shape3.Shape.ScheduledStartTimeUtc);

			AssertEquals(ZDateTime.UtcNow.AddDays(5), shape1.Shape.ScheduledFinishTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(8), shape2.Shape.ScheduledFinishTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(13), shape3.Shape.ScheduledFinishTimeUtc);

			Factory.Save();

			var startTimeFilter = (ApprovedWorkflowScheduledTimeFilter)new ProcessHeaderFilterBusinessObject()[ProcessHeader.ModuleFilterConstants.ApprovedScheduledStartTime];
			var finishTimeFilter = (ApprovedWorkflowScheduledTimeFilter)new ProcessHeaderFilterBusinessObject()[ProcessHeader.ModuleFilterConstants.ApprovedScheduledFinishTime];

			startTimeFilter.PropertySearch = ModuleDateFilter.Future;
			var startTimeResults = Factory.Load<ProcessHeader>(startTimeFilter.Query);
			AssertEquals("Only one shape starting in the future is approved", 1, startTimeResults.Length);
			AssertEquals(workflow2, startTimeResults[0]);

			startTimeFilter.PropertySearch = ModuleDateFilter.Past;
			startTimeResults = Factory.Load<ProcessHeader>(startTimeFilter.Query);

			AssertContainsExactElementsInAnyOrder("Job-level workflow and workflow1 both scheduled to start with the start time of the diagram, which is in the past", new[] { jobHeader, workflow1 }, startTimeResults);

			finishTimeFilter.PropertySearch = ModuleDateFilter.Future;
			var finishTimeResults = Factory.Load<ProcessHeader>(finishTimeFilter.Query);
			AssertEquals("Both approved shapes are finishing in the future", 2, finishTimeResults.Length);
			AssertCollectionContains(workflow1, finishTimeResults);
			AssertCollectionContains(workflow2, finishTimeResults);
		}

		[TestDate(2014, 6, 12)]
		public void TestQuery_HasNoDateOption()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "INQ");

			var approvedWorkflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Approved!");
			var nonApprovedWorkflowOnADiagram = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Non-Approved but on a diagram");
			var nonApprovedWorkflowNotOnADiagram = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Non-Approved and not on a diagram");

			void CreateShape(ProcessHeader workflow, bool shouldApprove)
			{
				var diagramShape = NetworkTestCase.CreateDiagram(workflow.JobHeader, name: "diagram");
				var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
				var network = networkViewModel.GetJobNetwork();
				var diagram = network.DiagramEntity;
				NetworkTestCase.CreateShape(workflow, diagram, "shape1");

				if (shouldApprove)
				{
					network.SwitchToScaled();
					networkViewModel.SuggestAndAcceptAllBuffers();
					networkViewModel.ToggleApproval();
				}
			}

			CreateShape(approvedWorkflow, true);
			CreateShape(nonApprovedWorkflowOnADiagram, false);
			Factory.Save();

			void AssertQueryResults(ApprovedWorkflowScheduledTimeFilter filter)
			{
				filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
				var results = Factory.Load<ProcessHeader>(filter.FilterBusinessObject.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { nonApprovedWorkflowOnADiagram.FH_CompletionStatement, nonApprovedWorkflowNotOnADiagram.FH_CompletionStatement }, results.Select(x => x.FH_CompletionStatement));
			}

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow).SetWorkflowOnly(); // just to make the results clearer
			var startTimeFilter = filterBizo.AddFilterStrip<ApprovedWorkflowScheduledTimeFilter>(ProcessHeader.ModuleFilterConstants.ApprovedScheduledStartTime);
			var finishTimeFilter = filterBizo.AddFilterStrip<ApprovedWorkflowScheduledTimeFilter>(ProcessHeader.ModuleFilterConstants.ApprovedScheduledFinishTime);

			AssertQueryResults(startTimeFilter);
			AssertQueryResults(finishTimeFilter);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return ApprovedWorkflowScheduledTimeFilter.GetScheduledStartTimeFilter();
		}
	}
}
