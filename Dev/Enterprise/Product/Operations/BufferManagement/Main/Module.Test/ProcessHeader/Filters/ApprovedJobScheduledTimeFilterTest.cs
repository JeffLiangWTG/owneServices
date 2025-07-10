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
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ApprovedJobScheduledTimeFilter))]
	class ApprovedJobScheduledTimeFilterTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2014, 6, 12)]
		public void TestFilter()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			BMSTestHelper.EnableBMSInRegistry();
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var diagramJobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var jobHeader1 = ProcessJobHeader.GetForParent(org1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(org2, Factory);
			var jobHeader3 = ProcessJobHeader.GetForParent(org3, Factory);

			var link1_2 = jobHeader1.GetOrCreateDependencyLink(jobHeader2);
			var link2_3 = jobHeader2.GetOrCreateDependencyLink(jobHeader3);

			var diagramShape = NetworkTestCase.CreateDiagram(diagramJobHeader, name: "diagram");
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var subDiagram1 = NetworkTestCase.CreateShape(jobHeader1, diagram, "subDiagram1");
			var subDiagram2 = NetworkTestCase.CreateShape(jobHeader2, diagram, "subDiagram2");
			var subDiagram3 = NetworkTestCase.CreateShape(jobHeader3, diagram, "subDiagram3");

			var arrow1_2 = subDiagram1.MakeVisiblePrerequisiteOf(subDiagram2);
			var arrow2_3 = subDiagram2.MakeVisiblePrerequisiteOf(subDiagram3);

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			network.Shapes.Single(s => s.BNS_RelatedEntityID == jobHeader3.PK).UnApprove();

			AssertEquals(0.0, subDiagram1.X);
			AssertEquals(300.0, subDiagram2.X);
			AssertEquals(600.0, subDiagram3.X);

			AssertEquals(ZDateTime.UtcNow, subDiagram1.Shape.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(5), subDiagram2.Shape.ScheduledStartTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(8), subDiagram3.Shape.ScheduledStartTimeUtc);

			AssertEquals(ZDateTime.UtcNow.AddDays(5), subDiagram1.Shape.ScheduledFinishTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(8), subDiagram2.Shape.ScheduledFinishTimeUtc);
			AssertEquals(ZDateTime.UtcNow.AddDays(13), subDiagram3.Shape.ScheduledFinishTimeUtc);

			Factory.Save();

			var startTimeFilter = ApprovedJobScheduledTimeFilter.GetScheduledStartTimeFilter(typeof(OrgHeader));
			var finishTimeFilter = ApprovedJobScheduledTimeFilter.GetScheduledFinishTimeFilter(typeof(OrgHeader));

			startTimeFilter.PropertySearch = ModuleDateFilter.Future;
			var startTimeResults = Factory.Load<OrgHeader>(startTimeFilter.Query);
			AssertEquals("Only one org starting in the future is approved", 1, startTimeResults.Length);
			AssertEquals(org2, startTimeResults[0]);

			startTimeFilter.PropertySearch = ModuleDateFilter.Past;
			startTimeResults = Factory.Load<OrgHeader>(startTimeFilter.Query);
			AssertEquals("Only one org starts in the past", 1, startTimeResults.Length);
			AssertCollectionContains(org1, startTimeResults);

			finishTimeFilter.PropertySearch = ModuleDateFilter.Future;
			var finishTimeResults = Factory.Load<OrgHeader>(finishTimeFilter.Query);
			AssertEquals("Both approved org shapes are finishing in the future", 2, finishTimeResults.Length);
			AssertCollectionContains(org1, finishTimeResults);
			AssertCollectionContains(org2, finishTimeResults);
		}

		[TestDate(2014, 6, 12)]
		public void TestQuery_HasNoDateOption()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "INQ");

			var approvedWorkflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "workflow1");
			var nonApprovedWorkflowOnADiagram = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "workflow1");
			var nonApprovedWorkflowNotOnADiagram = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "workflow1");

			var approvedJob = (SalesEnquiry)approvedWorkflow.Parent;
			var nonApprovedJobWithWorkflowOnDiagram = (SalesEnquiry)nonApprovedWorkflowOnADiagram.Parent;
			var nonApprovedJobWithNoDiagram = (SalesEnquiry)nonApprovedWorkflowNotOnADiagram.Parent;
			approvedJob.Address1 = "Approved!";
			nonApprovedJobWithWorkflowOnDiagram.Address1 = "Non-Approved but on a diagram";
			nonApprovedJobWithNoDiagram.Address1 = "Non-Approved and not on a diagram";

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

			void AssertQueryResults(ApprovedJobScheduledTimeFilter filter)
			{
				filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
				var results = Factory.Load<SalesEnquiry>(filter.Query);
				AssertContainsExactElementsInAnyOrder(new[] { nonApprovedJobWithWorkflowOnDiagram.Address1, nonApprovedJobWithNoDiagram.Address1 }, results.Select(x => x.Address1));
			}

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.SalesEnquiry))
			{
				var startTimeFilter = module.FilterBusinessObject.AddFilterStrip<ApprovedJobScheduledTimeFilter>(ProcessHeader.ModuleFilterConstants.ApprovedScheduledStartTime);
				var finishTimeFilter = module.FilterBusinessObject.AddFilterStrip<ApprovedJobScheduledTimeFilter>(ProcessHeader.ModuleFilterConstants.ApprovedScheduledFinishTime);
				AssertQueryResults(startTimeFilter);
				AssertQueryResults(finishTimeFilter);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return ApprovedJobScheduledTimeFilter.GetScheduledStartTimeFilter(typeof(OrgHeader));
		}
	}
}
