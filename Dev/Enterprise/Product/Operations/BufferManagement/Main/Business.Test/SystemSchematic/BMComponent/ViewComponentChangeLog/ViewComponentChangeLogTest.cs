using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class ViewComponentChangeLogTest : BMSTestCaseWithFactory
	{
		[TestDate(2015, 7, 14)]
		public void TestView_BucketToBucketTransfer()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "B1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "B2");

			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);
			link.FL_Name = "Shalala";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			BMSTestHelper.CreateTask(workflow, "CDL", 1);

			Factory.Save();

			AssertEquals(bucket1, workflow.CurrentComponent);

			BMSTestCaseWithFactory.RunTransferRules(system);
			Factory.Save();
			AssertEquals(bucket2, workflow.CurrentComponent);
			var log = BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.SchematicTransfer, bucket1.PK, bucket2.PK, link.PK);

			AssertEquals("Moved from component [B1] to [B2]. It was moved by the BMS service task along component link [Shalala].", log.DisplayEventReference); // add link name string

			AssertViewContents(workflow.PK, bucket1.PK, bucket2.PK, workflow.FH_ReleaseDateTime, "XFR", "OPN", "", -1m, -1);
		}

		public void TestView_BucketToBucketTransfer_IsResponsive()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "B1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "B2");

			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);
			link.FL_Name = "Shalala";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			BMSTestHelper.CreateTask(workflow, "CDL", 1);

			Factory.Save();

			AssertEquals(bucket1, workflow.CurrentComponent);

			var mock = new Mock<ITransferRuleRunnerParams>();
			mock.Setup(b => b.IsResponsive).Returns(true);
			mock.Setup(b => b.IsCdcEnabled).Returns(true);

			BMSTestCaseWithFactory.RunTransferRules(system, mock.Object);
			Factory.Save();
			AssertEquals(bucket2, workflow.CurrentComponent);
			var log = BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.ResponsiveTransfer, bucket1.PK, bucket2.PK, link.PK);

			AssertEquals("Moved from component [B1] to [B2]. It was moved by the Responsive Transfer along component link [Shalala].", log.DisplayEventReference); // add link name string

			AssertViewContents(workflow.PK, bucket1.PK, bucket2.PK, workflow.FH_ReleaseDateTime, "RXR", "OPN", "", -1m, -1);
		}

		[TestDate(2015, 7, 14)]
		public void TestView_BufferToBufferTransfer()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "B1", timespanMinutes: 100);
			var bucket = BMSTestHelper.CreateBucket(system, "B2");

			BMSTestHelper.LinkComponents(buffer, bucket);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow;
			BMSTestHelper.CreateTask(workflow, "CDL", 1);

			Factory.Save();

			AssertEquals(buffer, workflow.CurrentComponent);

			BMSTestCaseWithFactory.RunTransferRules(system);
			Factory.Save();
			AssertEquals(bucket, workflow.CurrentComponent);

			AssertViewContents(workflow.PK, buffer.PK, bucket.PK, workflow.FH_ReleaseDateTime, "XFR", "OPN", "NON", 0, 3);
		}

		[TestDate(2015, 7, 14)]
		public void TestView_BufferToBufferTransfer_TwoDigitBufferPenetration()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "B1", timespanMinutes: 100);
			var bucket = BMSTestHelper.CreateBucket(system, "B2");

			BMSTestHelper.LinkComponents(buffer, bucket);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-10);
			BMSTestHelper.CreateTask(workflow, "CDL", 1);

			Factory.Save();

			AssertEquals(buffer, workflow.CurrentComponent);

			BMSTestCaseWithFactory.RunTransferRules(system);
			AssertEquals(bucket, workflow.CurrentComponent);

			AssertViewContents(workflow.PK, buffer.PK, bucket.PK, workflow.FH_ReleaseDateTime, "XFR", "OPN", "NON", 0.1m, 3);
		}

		[TestDate(2015, 7, 14)]
		public void TestView_BufferToBufferTransfer_ThreeDigitBufferPenetration()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "B1", timespanMinutes: 100);
			var bucket = BMSTestHelper.CreateBucket(system, "B2");

			BMSTestHelper.LinkComponents(buffer, bucket);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-100);
			BMSTestHelper.CreateTask(workflow, "CDL", 1);

			Factory.Save();

			AssertEquals(buffer, workflow.CurrentComponent);

			BMSTestCaseWithFactory.RunTransferRules(system);
			Factory.Save();
			AssertEquals(bucket, workflow.CurrentComponent);

			AssertViewContents(workflow.PK, buffer.PK, bucket.PK, workflow.FH_ReleaseDateTime, "XFR", "OPN", "NON", 1, 0);
		}

		[TestDate(2015, 7, 14)]
		public void TestView_BufferToBufferTransfer_FourDigitBufferPenetration()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "B1", timespanMinutes: 100);
			var bucket = BMSTestHelper.CreateBucket(system, "B2");

			BMSTestHelper.LinkComponents(buffer, bucket);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-1000);
			BMSTestHelper.CreateTask(workflow, "CDL", 1);

			Factory.Save();

			AssertEquals(buffer, workflow.CurrentComponent);

			BMSTestCaseWithFactory.RunTransferRules(system);
			Factory.Save();
			AssertEquals(bucket, workflow.CurrentComponent);

			AssertViewContents(workflow.PK, buffer.PK, bucket.PK, workflow.FH_ReleaseDateTime, "XFR", "OPN", "NON", 10, 0);
		}

		[TestDate(2016, 11, 25)]
		public void TestClosingWorkflowCalculatesBufferPenetration_ShouldSetToPercentageWhenClosed()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "B1", timespanMinutes: 100);
			var bucket = BMSTestHelper.CreateBucket(system, "B2");

			BMSTestHelper.LinkComponents(buffer, bucket);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-1000);
			BMSTestHelper.CreateTask(workflow, "BAM", 1, taskStatus: "CLS");
			Factory.Save();

			BMSTestCaseWithFactory.RunTransferRules(system);
			Factory.Save();

			decimal expectedBufferPercentage = 9.98m;
			AssertViewContents(workflow.PK, buffer.PK, bucket.PK, workflow.FH_ReleaseDateTime, "XFR", "CLS", "", expectedBufferPercentage, 0);
		}

		[TestDate(2016, 11, 25)]
		public void TestClosingWorkflowDTOCalculatesBufferPenetration_ShouldSetToPercentageWhenClosed()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "B1", timespanMinutes: 100);
			var bucket = BMSTestHelper.CreateBucket(system, "B2");

			BMSTestHelper.LinkComponents(buffer, bucket);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-1000);
			BMSTestHelper.CreateTask(workflow, "BAM", 1, taskStatus: "CLS");

			Factory.Save();

			var workflowDTO = new WorkflowDTO(workflow.PK.ToGuid(), buffer);

			BMSTestCaseWithFactory.RunTransferRules(system);
			Factory.Save();

			decimal expectedBufferPercentage = 9.98m;
			AssertViewContents(workflowDTO.PK, buffer.PK, bucket.PK, workflow.FH_ReleaseDateTime, "XFR", "CLS", "", expectedBufferPercentage, 0);
		}

		[TestDate(2019, 1, 23)]
		public void TestDeferWithReason()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucketBefore = BMSTestHelper.CreateBucket(system, "B0", sequence: 1);
			var buffer = BMSTestHelper.CreateBuffer(system, "B1", timespanMinutes: 100, sequence: 2);
			var bucketAfter = BMSTestHelper.CreateBucket(system, "B2", sequence: 3);

			BMSTestHelper.LinkComponents(bucketBefore, buffer);
			BMSTestHelper.LinkComponents(buffer, bucketAfter);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", currentComponent: buffer);
			workflow.FH_DoNotStartBeforeDate = ZDateTime.Now.AddDays(2);

			Factory.Save();

			BMSTestHelper.Defer(workflow, WorkflowDeferralReasonsList.Codes.PrioritiesChanged);
			Factory.Save();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var workflowDTO = new WorkflowDTO(loadedWorkflow.PK.ToGuid(), bucketBefore);
			AssertEquals(bucketBefore.PK, loadedWorkflow.CurrentComponent.PK);

			AssertViewContents(loadedWorkflow.PK, buffer.PK, bucketBefore.PK, loadedWorkflow.FH_ReleaseDateTime, "DFR", "CLS", "", 0.0m, 3, WorkflowDeferralReasonsList.Codes.PrioritiesChanged);
		}

		#region Implementation

		void AssertViewContents(ZGuid cCL_ParentId, ZGuid cCL_FC_ComponentFrom, ZGuid cCL_FC_ComponentTo, ZDateTime cCL_TransferTimeUtc, string cCL_TransferType, string cCL_WorkflowStatus, string cCL_CcrStatus, decimal cCL_BufferPenetrationPercent, int cCL_BufferZone, string cCL_DeferReason = "")
		{
			using (var reader = Db.Connection.Command(string.Format("SELECT * FROM dbo.ViewComponentChangeLog WHERE CCL_ParentId = '{0}' AND CCL_TransferType = '{1}'", cCL_ParentId, cCL_TransferType)).ExecuteReader())
			{
				if (reader.Read())
				{
					AssertEquals(cCL_FC_ComponentFrom, reader.GetGuid(2));
					AssertEquals(cCL_FC_ComponentTo, reader.GetGuid(3));
					NUnit.Framework.Assert.That(reader.GetDateTime(4), NUnit.Framework.Is.EqualTo(cCL_TransferTimeUtc.ToDateTime()).Within(30).Seconds);
					AssertEquals(cCL_TransferType, reader.GetString(5));
					AssertEquals(cCL_WorkflowStatus, reader.GetString(6));
					AssertEquals(cCL_CcrStatus, reader.GetString(7));
					AssertEquals(cCL_DeferReason, reader.GetString(8));
					AssertEquals(cCL_BufferPenetrationPercent, reader.GetValue(9) == DBNull.Value ? null : (decimal?)reader.GetDecimal(9));
					AssertEquals(cCL_BufferZone, reader.GetValue(10) == DBNull.Value ? null : (int?)reader.GetInt32(10));
				}
				else
				{
					Fail("Expected ViewComponentChangeLog record was not found");
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();
		}

		#endregion
	}

	[TestedType(typeof(ViewComponentChangeLog))]
	public class ViewComponentChangeLogEnterpriseBizoTest : EnterpriseBusinessObjectTestCase
	{
		protected override bool IsDeleteSupported()
		{
			return false; // This bizo is for a view and that view should never be updated.
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var system = BMSystem.GetSystemForWorkflowType("ORG", Factory)
				?? BMSTestHelper.CreateSystem(Factory, "ORG");

			if (system.Components.Count < 2)
			{
				BMSTestHelper.CreateBucket(system, "bucket1");
				BMSTestHelper.CreateBucket(system, "bucket2");
			}

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Jetpack Navigator");

			workflow.FH_FC_CurrentComponent = system.Components[0].PK;
			workflow.FH_FC_CurrentComponent = system.Components[1].PK;

			Factory.Save();

			return Factory.LoadTop1<ViewComponentChangeLog>(new ZQuery(ViewComponentChangeLogSchema.CCL_ParentId, workflow.PK));
		}
	}
}
