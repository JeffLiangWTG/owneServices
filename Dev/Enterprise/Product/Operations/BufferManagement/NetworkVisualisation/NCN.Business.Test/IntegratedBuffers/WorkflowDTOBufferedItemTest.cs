using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WorkflowDTO = Enterprise.BufferManagement.Business.WorkflowDTO;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	#region NonNetwork Test

	[TestedType(typeof(WorkflowDTO))]
	[TestDate(2014, 9, 2)]
	class WorkflowDTOBufferedItemTest_NonNetworkItem : BufferedItemTestCase
	{
		protected override IBufferedItem GetBufferedItem()
		{
			return workflowDTO;
		}

		protected override IEnumerable<IBuffer> GetExpectedRelatedBuffers()
		{
			yield return config.Buffer;
		}

		protected override ZDateTime GetExpectedStartableTime()
		{
			return ZDateTime.UtcNow.AddDays(-1);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		protected override int GetExpectedPlannedDurationInMinutes()
		{
			return 450;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		protected override int GetExpectedRemainingEstimateInMinutes()
		{
			return 360;
		}

		SchematicTestConfig config;
		WorkflowDTO workflowDTO;

		protected override void SetUp()
		{
			BMSRegistry.Instance.EnableSimpleCapacityCalculationForAllSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var resource1 = CreateStaffInCurrentBranchDept("BEN", "Benedict Cumberbatch");
			var resource2 = CreateStaffInCurrentBranchDept("MTN", "Martin Freeman");

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);

			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer, ZDateTime.UtcNow.AddDays(-1));
			var task1_1 = CreateTask(workflow1, resource1.GS_Code, 60);
			var task1_2 = CreateTask(workflow1, resource1.GS_Code, 60);
			var task1_3 = CreateTask(workflow1, resource1.GS_Code, 60);
			var task1_4 = CreateTask(workflow1, resource2.GS_Code, 120);

			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Bucket, ZDateTime.UtcNow.AddDays(-2));
			var task2_1 = CreateTask(workflow2, resource1.GS_Code, 60);
			var task2_2 = CreateTask(workflow2, resource1.GS_Code, 60);
			var task2_3 = CreateTask(workflow2, resource1.GS_Code, 60);
			var task2_4 = CreateTask(workflow2, resource2.GS_Code, 120);

			Factory.Save();

			task1_3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2_3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			var contents = CapacityCalculator.GetComponentContents_ForTest(config.Buffer, new[]
			{
				resource1
			});
			workflowDTO = contents.Single();

			base.SetUp();
		}
	}

	#endregion

	#region Network Test

	[TestedType(typeof(WorkflowDTO))]
	class WorkflowDTOBufferedItemTest_NetworkItem : BufferedItemTestCase
	{
		protected override IBufferedItem GetBufferedItem()
		{
			return workflowDTO;
		}

		protected override IEnumerable<IBuffer> GetExpectedRelatedBuffers()
		{
			yield return new BufferDTO
			{
				SizeInMinutes = buffer.SizeInMinutes
			};
		}

		protected override ZDateTime GetExpectedStartableTime()
		{
			return ZDateTime.UtcNow.AddDays(-10);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		protected override int GetExpectedPlannedDurationInMinutes()
		{
			return 1440;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		protected override int GetExpectedRemainingEstimateInMinutes()
		{
			return 360;
		}

		IBuffer buffer;
		WorkflowDTO workflowDTO;

		protected override void SetUp()
		{
			base.SetUp();

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var resource1 = CreateStaffInCurrentBranchDept("BEN", "Benedict Cumberbatch");
			var resource2 = CreateStaffInCurrentBranchDept("MTN", "Martin Freeman");

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-10);

			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer, ZDateTime.UtcNow.AddDays(-1), config.ReleaseGroup.PK);
			var task1_1 = CreateTask(workflow1, resource1.GS_Code, 60);
			var task1_2 = CreateTask(workflow1, resource1.GS_Code, 60);
			var task1_3 = CreateTask(workflow1, resource1.GS_Code, 60);
			var task1_4 = CreateTask(workflow1, resource2.GS_Code, 120);

			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Bucket, ZDateTime.UtcNow.AddDays(-2), config.ReleaseGroup.PK);
			var task2_1 = CreateTask(workflow2, resource1.GS_Code, 60);
			var task2_2 = CreateTask(workflow2, resource1.GS_Code, 60);
			var task2_3 = CreateTask(workflow2, resource1.GS_Code, 60);
			var task2_4 = CreateTask(workflow2, resource2.GS_Code, 120);

			var diagramShape = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var shape1 = CreateShape(workflow1, diagram, "shape1");
			var shape2 = CreateShape(workflow2, diagram, "shape2");

			shape1.MakeVisiblePrerequisiteOf(shape2);

			network.SwitchToScaled();

			shape1.Width = 300;
			shape2.Width = 300;
			networkViewModel.PushAsLateAsPossible();

			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			buffer = network.Shapes.OfType<IBuffer>().Single();

			Factory.Save();

			task1_3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2_3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			RunBufferPenetrationUpdater();

			var contents = CapacityCalculator.GetComponentContents_ForTest(config.Buffer, new[]
			{
				resource1
			});
			workflowDTO = contents.Single();
		}
	}

	#endregion
}
