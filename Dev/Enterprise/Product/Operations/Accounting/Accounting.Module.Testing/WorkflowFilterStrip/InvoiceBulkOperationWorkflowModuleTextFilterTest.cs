using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(InvoiceBulkOperationWorkflowModuleTextFilter))]
	public class InvoiceBulkOperationWorkflowModuleTextFilterTest : WorkflowModuleTextFilterTest
	{
		public void TestMilestoneQueryGetsTransportBookingJobParent()
		{
			var creator = new TestObjectCreator(Factory);

			var job1 = creator.CreateJob(creator.LocalClient, 0m, creator.Agent, 0m);
			var bizO = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone1 = bizO.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = "EV2";
			job1.JH_ParentID = milestone1.P9_ParentID;

			var transportBookingTestHelper = new TransportBookingTestHelper(Factory);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job2 = creator.CreateJob(shipment);
			var consolidation = transportBookingTestHelper.CreateConsolidation(shipment);
			var booking = transportBookingTestHelper.CreateBooking(consolidation);
			var milestone2 = booking.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = "EV1";
			job2.JH_ParentID = consolidation.KB_ParentID;
			Factory.Save();

			var filterStrip = new DummyFilterStripBusinessObject();
			var filter = new InvoiceBulkOperationWorkflowModuleTextFilter("filter", GetMilestoneTextFilter, new CodeDescriptionPairList(), typeof(JobHeader), "JOB");
			filterStrip.AddModuleFilterForTest(filter);

			var collection = new JobHeaderCollection(Factory);
			collection.Load(filterStrip.Filter);
			AssertEquals("There should be all jobs", 2, collection.Count);

			var milestoneFilter = (InvoiceBulkOperationWorkflowModuleTextFilter)filterStrip["filter"];
			milestoneFilter.MilestoneEvent = "EV1";
			milestoneFilter.Property = "Completed";
			milestoneFilter.IsActive = true;

			collection.Load(filterStrip.Filter);
			AssertEquals("There should be only one element", 1, collection.Count);
			Assert(collection.Contains(job2));
		}

		public void TestMilestoneQueryGetsCorrectAccruals()
		{
			var accrual1 = CreateAccrualsWithWorkflow(new ZDateTime(2013, 06, 06), "EV1");
			var accrual2 = CreateAccrualsWithWorkflow(new ZDateTime(2013, 06, 10), "EV1");
			var accrual3 = CreateAccrualsWithWorkflow(ZDateTime.Empty, "EV1");
			var accrual4 = CreateAccrualsWithWorkflow(new ZDateTime(2013, 06, 06), "EV2");
			var accrual5 = CreateAccrualsWithWorkflow(ZDateTime.Empty);
			Factory.Save();

			var filterStrip = new DummyFilterStripBusinessObject();

			var filter = new InvoiceBulkOperationWorkflowModuleTextFilter("filter", GetMilestoneTextFilter, new CodeDescriptionPairList(), typeof(AccTransactionLines), "JOB");
			filterStrip.AddModuleFilterForTest(filter);

			var collection = new AccTransactionLinesCollection(Factory);
			collection.Load(filterStrip.Filter);
			AssertEquals("There should be all accruals", 5, collection.Count);

			var milestoneFilter = (InvoiceBulkOperationWorkflowModuleTextFilter)filterStrip["filter"];
			milestoneFilter.MilestoneEvent = "EV1";
			milestoneFilter.Property = "Completed";
			milestoneFilter.IsActive = true;

			collection.Load(filterStrip.Filter);
			AssertEquals("There should be only two elements", 2, collection.Count);
			Assert("Accrual1 matches filter", collection.Contains(accrual1));
			Assert("Accrual2 matches filter", collection.Contains(accrual2));
		}

		public void TestMilestoneQueryGetsCorrectJobs()
		{
			var job1 = CreateJobsWithWorkflow(new ZDateTime(2013, 06, 06), "EV1");
			var job2 = CreateJobsWithWorkflow(new ZDateTime(2013, 06, 10), "EV1");
			var job3 = CreateJobsWithWorkflow(ZDateTime.Empty, "EV1");
			var job4 = CreateJobsWithWorkflow(new ZDateTime(2013, 06, 06), "EV2");
			var job5 = CreateJobsWithWorkflow(ZDateTime.Empty);
			Factory.Save();

			var filterStrip = new DummyFilterStripBusinessObject();

			var filter = new InvoiceBulkOperationWorkflowModuleTextFilter("filter", GetMilestoneTextFilter, new CodeDescriptionPairList(), typeof(JobHeader), "JOB");
			filterStrip.AddModuleFilterForTest(filter);

			var collection = new JobHeaderCollection(Factory);
			collection.Load(filterStrip.Filter);
			AssertEquals("There should be all jobs", 5, collection.Count);

			var milestoneFilter = (InvoiceBulkOperationWorkflowModuleTextFilter)filterStrip["filter"];
			milestoneFilter.MilestoneEvent = "EV1";
			milestoneFilter.Property = "Completed";
			milestoneFilter.IsActive = true;

			collection.Load(filterStrip.Filter);
			AssertEquals("There should be only two elements", 2, collection.Count);
			Assert("Job1 matches filter", collection.Contains(job1));
			Assert("Job2 matches filter", collection.Contains(job2));
		}

		#region Implementation

		AccTransactionLines CreateAccrualsWithWorkflow(ZDateTime actualDate, string eventType = null)
		{
			var job = creator.CreateJob(creator.LocalClient, 0m, creator.Agent, 0m);
			var accrual = creator.CreateAccrual(job, creator.CC1, 1m, "Bulk Test", 100m);
			var bizO = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.JH_ParentID = bizO.PK;
			job.JH_ParentTableCode = bizO.TablePrefix;

			if (!actualDate.IsEmpty && !string.IsNullOrEmpty(eventType))
			{
				var milestone = bizO.WorkflowItems.Milestones.AddNew();
				milestone.SetMilestoneActualDateForTest(actualDate);
				milestone.TriggerConditions.TriggerEventCode = eventType;
				milestone.P9_ParentTableCode = bizO.TablePrefix;
			}

			return accrual;
		}

		JobHeader CreateJobsWithWorkflow(ZDateTime actualDate, string eventType = null)
		{
			var job = creator.CreateJob(creator.LocalClient, 0m, creator.Agent, 0m);
			var bizO = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.JH_ParentID = bizO.PK;
			job.JH_ParentTableCode = bizO.TablePrefix;

			if (!actualDate.IsEmpty && !string.IsNullOrEmpty(eventType))
			{
				var milestone = bizO.WorkflowItems.Milestones.AddNew();
				milestone.SetMilestoneActualDateForTest(actualDate);
				milestone.TriggerConditions.TriggerEventCode = eventType;
				milestone.P9_ParentTableCode = bizO.TablePrefix;
			}

			return job;
		}

		TestObjectCreator creator;

		protected override void SetUp()
		{
			base.SetUp();

			creator = new TestObjectCreator(Factory);
		}

		protected override WorkflowModuleTextFilter GetNewModuleFilter()
		{
			return new InvoiceBulkOperationWorkflowModuleTextFilter("moo", GetMilestoneTextFilter, new CodeDescriptionPairList(), typeof(DummyBusinessObject), "JOB");
		}

		#endregion
	}
}
