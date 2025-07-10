using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Service
{
	public class AcceptabilityBandsServiceTest : TestCaseWithFactory
	{
		#region Setup and Helpers

		AcceptabilityBandsService acceptabilityBandsService;
		bool IsUserInteractive;
		SchematicTestConfig schematicTestConfig;

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
			IsUserInteractive = Globals.IsUserInteractive;
			Globals.IsUserInteractive = false;
			schematicTestConfig = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			acceptabilityBandsService = new AcceptabilityBandsService();
			BMSRegistry.Instance.AcceptabilityBandClientCacheTimePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			BMSRegistry.Instance.AcceptabilityBandServerCacheTimePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
		}

		protected override void TearDown()
		{
			Globals.IsUserInteractive = IsUserInteractive;
			MasterFilesTestHelper.ClearIterationReasonsFromRegistry(WorkflowDescriptors.DummyWorkflowDescriptorCode);
			base.TearDown();
		}

		public Guid CreateNewBoard(
			Guid bmSystemPK,
			Guid releaseGroupPk,
			List<IBMComponent> components,
			List<IGlbStaff> channels,
			List<AcceptabilityBandConfigurationDto> bands,
			string name = "Board")
		{
			var boardInfo = new
			{
				Components = components.Select(c => new
				{
					Id = c.PK,
					Name = c.FC_Name,
					Description = c.FC_Type,
				}).ToArray(),
				Channels = channels.Select(c => new
				{
					Id = c.PK,
					Name = c.GS_Code,
					Description = c.FullName,
				}).ToArray(),
				Settings = new
				{
					EnableTimeRecording = true,
					EnableAcceptabilityBands = true,
				},
				AcceptabilityBands = bands
			};

			var boardInfoJson = JsonConvert.SerializeObject(boardInfo, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() } );

			var boardId = Guid.NewGuid();
			Db.Connection.ExecuteNonQuery(
@$"INSERT INTO [dbo].[BMBoardConfiguration]
           ([BMB_PK]
           ,[BMB_Name]
           ,[BMB_Description]
           ,[BMB_FS_System]
           ,[BMB_GG_ReleaseGroup]
           ,[BMB_BoardInfo]
           ,[BMB_SystemCreateTimeUtc]
           ,[BMB_SystemCreateUser]
           ,[BMB_SystemLastEditTimeUtc]
           ,[BMB_SystemLastEditUser])
     VALUES
           ('{boardId}'
           ,'New Board Name {boardId}'
           ,'New Board Description {boardId}'
           ,'{bmSystemPK}'
           ,'{releaseGroupPk}'
           ,'{boardInfoJson}'
           ,getdate()
           ,'AAA'
           ,getdate()
           ,'AAA')");

			return boardId;
		}

		#endregion

		#region TryGetAcceptabilityBand

		[TestDate(2025, 05, 18, 1, 2, 3)]
		public void Test_TryGetAcceptabilityBand_ShouldGetCorrectResult()
		{
			var system = schematicTestConfig.System;
			var releaseGroup = schematicTestConfig.ReleaseGroup;
			var buffer = schematicTestConfig.Buffer;
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow1", buffer);
			var task1 = VisualBoardsTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow2", buffer);
			var task2 = VisualBoardsTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var band = BMSTestHelper.CreateAcceptabilityBand(buffer, 1, 2, 3, 4, 5, 6, "Band", type: AcceptabilityBandTypes.Codes.Count);

			Factory.Save();

			var boardId = CreateNewBoard(
				system.PK.ToGuid(),
				releaseGroup.PK.ToGuid(),
				[buffer],
				[],
				[
					new() {
						AcceptabilityBandId = band.PK.ToGuid(),
						Name = band.BAB_Name,
						DisplayName = "My first band",
						DisplayUnits = "Units",
						FilterByReleaseGroup = false,
						FilterByComponents = false,
						OverrideBoundaryValues = false,
						Boundaries = null
					}
				]);

			var result = acceptabilityBandsService.TryGetAcceptabilityBand(boardId, band.PK.ToGuid(), out var acceptabilityBandResultDto, out var businessResponse);

			Assert(result);
			AssertNull(businessResponse);
			AssertNotNull(acceptabilityBandResultDto);

			AssertEquals(acceptabilityBandResultDto.AcceptabilityBandId, band.PK.ToGuid());
			AssertEquals(acceptabilityBandResultDto.DisplayName, "My first band");
			AssertEquals(acceptabilityBandResultDto.DisplayUnits, "Units");
			AssertNotNull(acceptabilityBandResultDto.Boundaries);
			AssertEquals(acceptabilityBandResultDto.Boundaries.CautionLowerBoundary, 1);
			AssertEquals(acceptabilityBandResultDto.Boundaries.GoodLowerBoundary, 2);
			AssertEquals(acceptabilityBandResultDto.Boundaries.ExcellentLowerBoundary, 3);
			AssertEquals(acceptabilityBandResultDto.Boundaries.ExcellentUpperBoundary, 4);
			AssertEquals(acceptabilityBandResultDto.Boundaries.GoodUpperBoundary, 5);
			AssertEquals(acceptabilityBandResultDto.Boundaries.CautionUpperBoundary, 6);
			AssertEquals(acceptabilityBandResultDto.Result, 2.0m);
			AssertEquals(acceptabilityBandResultDto.CalculatedAtUtc, ZDateTime.Now);
		}

		[TestDate(2025, 05, 18, 1, 2, 3)]
		public void Test_TryGetAcceptabilityBand_ShouldGetNoResult()
		{
			var system = schematicTestConfig.System;
			var releaseGroup = schematicTestConfig.ReleaseGroup;
			var buffer = schematicTestConfig.Buffer;
			var band = BMSTestHelper.CreateAcceptabilityBand(buffer, 1, 2, 3, 4, 5, 6, "Band", type: AcceptabilityBandTypes.Codes.Count);

			Factory.Save();

			var boardId = CreateNewBoard(
				system.PK.ToGuid(),
				releaseGroup.PK.ToGuid(),
				[buffer],
				[],
				[
					new() {
						AcceptabilityBandId = band.PK.ToGuid(),
						Name = band.BAB_Name,
						DisplayName = "No result AB",
						DisplayUnits = null,
						FilterByReleaseGroup = false,
						FilterByComponents = false,
						OverrideBoundaryValues = false,
						Boundaries = null
					}
				]);

			var result = acceptabilityBandsService.TryGetAcceptabilityBand(boardId, band.PK.ToGuid(), out var acceptabilityBandResultDto, out var businessResponse);

			Assert(result);
			AssertNull(businessResponse);
			AssertNotNull(acceptabilityBandResultDto);

			AssertEquals(acceptabilityBandResultDto.AcceptabilityBandId, band.PK.ToGuid());
			AssertEquals(acceptabilityBandResultDto.DisplayName, "No result AB");
			AssertNull(acceptabilityBandResultDto.DisplayUnits);
			AssertNotNull(acceptabilityBandResultDto.Boundaries);
			AssertEquals(acceptabilityBandResultDto.Boundaries.CautionLowerBoundary, 1);
			AssertEquals(acceptabilityBandResultDto.Boundaries.GoodLowerBoundary, 2);
			AssertEquals(acceptabilityBandResultDto.Boundaries.ExcellentLowerBoundary, 3);
			AssertEquals(acceptabilityBandResultDto.Boundaries.ExcellentUpperBoundary, 4);
			AssertEquals(acceptabilityBandResultDto.Boundaries.GoodUpperBoundary, 5);
			AssertEquals(acceptabilityBandResultDto.Boundaries.CautionUpperBoundary, 6);
			AssertEquals(0m ,acceptabilityBandResultDto.Result);
			AssertEquals(acceptabilityBandResultDto.CalculatedAtUtc, ZDateTime.Now);
		}

		[TestDate(2025, 05, 18, 1, 2, 3)]
		public void Test_TryGetAcceptabilityBand_WhenFilterByReleaseGroup_ShouldGetCorrectResult()
		{
			var system = schematicTestConfig.System;
			var releaseGroup = schematicTestConfig.ReleaseGroup;
			var buffer = schematicTestConfig.Buffer;
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow1", buffer, releaseGroupPK: releaseGroup.PK);
			var task1 = VisualBoardsTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow2", buffer, releaseGroupPK: releaseGroup.PK);
			var task2 = VisualBoardsTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var otherReleaseGroup = BMSTestHelper.CreateGroup(Factory, "OTH", "Other Release Group");
			var workflowInOtherRG = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow2", buffer, releaseGroupPK: otherReleaseGroup.PK);
			var taskInOtherRG = VisualBoardsTestHelper.CreateTask(workflowInOtherRG, GlbStaff.CurrentUser.GS_Code, 60);

			var band = BMSTestHelper.CreateAcceptabilityBand(buffer, 1, 2, 3, 4, 5, 6, "Band", type: AcceptabilityBandTypes.Codes.Count);

			Factory.Save();

			var boardId = CreateNewBoard(
				system.PK.ToGuid(),
				releaseGroup.PK.ToGuid(),
				[buffer],
				[],
				[
					new() {
						AcceptabilityBandId = band.PK.ToGuid(),
						Name = band.BAB_Name,
						DisplayName = "Count on board RG AB",
						DisplayUnits = "Units",
						FilterByReleaseGroup = true,
						FilterByComponents = false,
						OverrideBoundaryValues = true,
						Boundaries = new AcceptabilityBandBoundariesDto
						{
							CautionLowerBoundary = 10,
							GoodLowerBoundary = 20,
							ExcellentLowerBoundary = 30,
							ExcellentUpperBoundary = 40,
							GoodUpperBoundary = 50,
							CautionUpperBoundary = 60
						}
					}
				]);

			var result = acceptabilityBandsService.TryGetAcceptabilityBand(boardId, band.PK.ToGuid(), out var acceptabilityBandResultDto, out var businessResponse);

			Assert(result);
			AssertNull(businessResponse);
			AssertNotNull(acceptabilityBandResultDto);

			AssertEquals(acceptabilityBandResultDto.AcceptabilityBandId, band.PK.ToGuid());
			AssertEquals(acceptabilityBandResultDto.DisplayName, "Count on board RG AB");
			AssertEquals(acceptabilityBandResultDto.DisplayUnits, "Units");
			AssertNotNull(acceptabilityBandResultDto.Boundaries);
			AssertEquals(acceptabilityBandResultDto.Boundaries.CautionLowerBoundary, 10);
			AssertEquals(acceptabilityBandResultDto.Boundaries.GoodLowerBoundary, 20);
			AssertEquals(acceptabilityBandResultDto.Boundaries.ExcellentLowerBoundary, 30);
			AssertEquals(acceptabilityBandResultDto.Boundaries.ExcellentUpperBoundary, 40);
			AssertEquals(acceptabilityBandResultDto.Boundaries.GoodUpperBoundary, 50);
			AssertEquals(acceptabilityBandResultDto.Boundaries.CautionUpperBoundary, 60);
			AssertEquals(acceptabilityBandResultDto.Result, 2.0m);
			AssertEquals(acceptabilityBandResultDto.CalculatedAtUtc, ZDateTime.Now);
		}

		[TestDate(2025, 05, 18, 1, 2, 3)]
		public void Test_TryGetAcceptabilityBand_WhenFilterByComponents_ShouldGetCorrectResult()
		{
			var system = schematicTestConfig.System;
			var releaseGroup = schematicTestConfig.ReleaseGroup;
			var buffer = schematicTestConfig.Buffer;
			var buffer2 = BMSTestHelper.CreateBuffer(system,"Buffer 2");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow1", buffer);
			var task1 = VisualBoardsTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow2", buffer2);
			var task2 = VisualBoardsTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var workflow3 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow3", buffer2);
			var task3 = VisualBoardsTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);

			var otherBuffer = BMSTestHelper.CreateBuffer(system, "Other Buffer");
			var workflowInOtherBuffer = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow2", otherBuffer);
			var taskInOtherBuffer = VisualBoardsTestHelper.CreateTask(workflowInOtherBuffer, GlbStaff.CurrentUser.GS_Code, 60);

			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 1, 2, 3, 4, 5, 6, "Band", type: AcceptabilityBandTypes.Codes.Count);

			Factory.Save();

			var boardId = CreateNewBoard(
				system.PK.ToGuid(),
				releaseGroup.PK.ToGuid(),
				[buffer, buffer2],
				[],
				[
					new() {
						AcceptabilityBandId = band.PK.ToGuid(),
						Name = band.BAB_Name,
						DisplayName = "Count on board Components AB",
						DisplayUnits = "Units",
						FilterByReleaseGroup = false,
						FilterByComponents = true,
						OverrideBoundaryValues = false,
						Boundaries = new AcceptabilityBandBoundariesDto
						{
							CautionLowerBoundary = 11,
							GoodLowerBoundary = 22,
							ExcellentLowerBoundary = 33,
							ExcellentUpperBoundary = 44,
							GoodUpperBoundary = 55,
							CautionUpperBoundary = 66
						}
					}
				]);

			var result = acceptabilityBandsService.TryGetAcceptabilityBand(boardId, band.PK.ToGuid(), out var acceptabilityBandResultDto, out var businessResponse);

			Assert(result);
			AssertNull(businessResponse);
			AssertNotNull(acceptabilityBandResultDto);

			AssertEquals(acceptabilityBandResultDto.AcceptabilityBandId, band.PK.ToGuid());
			AssertEquals(acceptabilityBandResultDto.DisplayName, "Count on board Components AB");
			AssertEquals(acceptabilityBandResultDto.DisplayUnits, "Units");
			AssertNotNull(acceptabilityBandResultDto.Boundaries);

			//Even though the band setup has different boundaries, the result is calculated based on the band values because OverrideBoundaryValues is false
			AssertEquals(acceptabilityBandResultDto.Boundaries.CautionLowerBoundary, 1);
			AssertEquals(acceptabilityBandResultDto.Boundaries.GoodLowerBoundary, 2);
			AssertEquals(acceptabilityBandResultDto.Boundaries.ExcellentLowerBoundary, 3);
			AssertEquals(acceptabilityBandResultDto.Boundaries.ExcellentUpperBoundary, 4);
			AssertEquals(acceptabilityBandResultDto.Boundaries.GoodUpperBoundary, 5);
			AssertEquals(acceptabilityBandResultDto.Boundaries.CautionUpperBoundary, 6);

			AssertEquals(acceptabilityBandResultDto.Result, 3.0m);
			AssertEquals(acceptabilityBandResultDto.CalculatedAtUtc, ZDateTime.Now);
		}

		[TestDate(2025, 05, 18, 1, 2, 3)]
		public void Test_TryGetAcceptabilityBand_ShouldReturnFalse()
		{
			var result = acceptabilityBandsService.TryGetAcceptabilityBand(Guid.NewGuid(), Guid.NewGuid(), out var dto, out var businessResponse);

			Assert(!result);
			AssertNotNull(businessResponse);
			AssertNull(dto);
			AssertEquals(businessResponse.Message.Text, AcceptabilityBandsService.BusinessMessages.ErrorLoadingBoardConfigurationInfo.Text);

			var system = schematicTestConfig.System;
			var releaseGroup = schematicTestConfig.ReleaseGroup;
			var buffer = schematicTestConfig.Buffer;
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow1", buffer);
			var task1 = VisualBoardsTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow2", buffer);
			var task2 = VisualBoardsTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var band = BMSTestHelper.CreateAcceptabilityBand(buffer, 1, 2, 3, 4, 5, 6, "Band", type: AcceptabilityBandTypes.Codes.Count);

			Factory.Save();

			var boardId = CreateNewBoard(
				system.PK.ToGuid(),
				releaseGroup.PK.ToGuid(),
				[buffer],
				[],
				[
					new() {
						AcceptabilityBandId = band.PK.ToGuid(),
					}
				]);

			result = acceptabilityBandsService.TryGetAcceptabilityBand(boardId, Guid.NewGuid(), out var boardHealthWorkflowDto, out businessResponse);

			Assert(!result);
			AssertNotNull(businessResponse);
			AssertNull(dto);
			AssertEquals(businessResponse.Message.Text, AcceptabilityBandsService.BusinessMessages.AcceptabilityBandNotInBoardConfiguration.Text);
		}

		#endregion

		#region TryGetWorkflowsMatchingAcceptabilityBand

		[TestDate(2025, 05, 18, 1, 2, 3)]
		public void Test_TryGetWorkflowsMatchingAcceptabilityBand_ShouldGetCorrectResult()
		{
			var system = schematicTestConfig.System;
			var releaseGroup = schematicTestConfig.ReleaseGroup;
			var buffer = schematicTestConfig.Buffer;
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "Job1", addDefaultProcessHeaderIfNone: false);
			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader1, "workflow1", buffer);
			var task1 = VisualBoardsTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "Job2", addDefaultProcessHeaderIfNone: false);
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader2, "workflow2", buffer);
			var task2 = VisualBoardsTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var band = BMSTestHelper.CreateAcceptabilityBand(buffer, 1, 2, 3, 4, 5, 6, "Band", type: AcceptabilityBandTypes.Codes.Count);
			Factory.Save();

			var boardId = CreateNewBoard(
				system.PK.ToGuid(),
				releaseGroup.PK.ToGuid(),
				[buffer],
				[],
				[
					new() {
						AcceptabilityBandId = band.PK.ToGuid(),
						Name = band.BAB_Name,
						DisplayName = "My first band",
						DisplayUnits = "Units",
						FilterByReleaseGroup = false,
						FilterByComponents = false,
						OverrideBoundaryValues = false,
						Boundaries = null
					}
				]);

			var result = acceptabilityBandsService.TryGetWorkflowsMatchingAcceptabilityBand(boardId, band.PK.ToGuid(), out var boardHealthWorkflowDto, out var businessResponse);

			Assert(result);
			AssertNull(businessResponse);
			AssertNotNull(boardHealthWorkflowDto);

			AssertEquals(boardHealthWorkflowDto.Length, 2);

			var workflow2Dto = boardHealthWorkflowDto[0];
			AssertEquals(workflow2Dto.WorkflowId, workflow2.PK.ToGuid());
			AssertEquals(workflow2Dto.WorkflowTitle, workflow2.FH_CompletionStatement);
			AssertEquals(workflow2Dto.JobId, workflow2.FH_ParentId);
			AssertEquals(workflow2Dto.JobCode, jobHeader1.FH_JobCode);
			AssertEquals(workflow2Dto.JobTitle, jobHeader1.FH_JobDescription);

			var workflow1Dto = boardHealthWorkflowDto[1];
			AssertEquals(workflow1Dto.WorkflowId, workflow1.PK.ToGuid());
			AssertEquals(workflow1Dto.WorkflowTitle, workflow1.FH_CompletionStatement);
			AssertEquals(workflow1Dto.JobId, workflow1.FH_ParentId);
			AssertEquals(workflow1Dto.JobCode, jobHeader1.FH_JobCode);
			AssertEquals(workflow1Dto.JobTitle, jobHeader1.FH_JobDescription);
		}

		[TestDate(2025, 05, 18, 1, 2, 3)]
		public void Test_TryGetWorkflowsMatchingAcceptabilityBand_ShouldGetNoResult()
		{
			var system = schematicTestConfig.System;
			var releaseGroup = schematicTestConfig.ReleaseGroup;
			var buffer = schematicTestConfig.Buffer;
			var band = BMSTestHelper.CreateAcceptabilityBand(buffer, 1, 2, 3, 4, 5, 6, "Band", type: AcceptabilityBandTypes.Codes.Count);

			Factory.Save();

			var boardId = CreateNewBoard(
				system.PK.ToGuid(),
				releaseGroup.PK.ToGuid(),
				[buffer],
				[],
				[
					new() {
						AcceptabilityBandId = band.PK.ToGuid(),
						Name = band.BAB_Name,
						DisplayName = "No result AB",
						DisplayUnits = null,
						FilterByReleaseGroup = false,
						FilterByComponents = false,
						OverrideBoundaryValues = false,
						Boundaries = null
					}
				]);

			var result = acceptabilityBandsService.TryGetWorkflowsMatchingAcceptabilityBand(boardId, band.PK.ToGuid(), out var acceptabilityBandResultDto, out var businessResponse);

			Assert(result);
			AssertNull(businessResponse);
			AssertNotNull(acceptabilityBandResultDto);
			AssertEquals(0, acceptabilityBandResultDto.Length);
		}

		[TestDate(2025, 05, 18, 1, 2, 3)]
		public void Test_TryGetWorkflowsMatchingAcceptabilityBand_WhenFilterByReleaseGroup_ShouldGetCorrectResult()
		{
			var system = schematicTestConfig.System;
			var releaseGroup = schematicTestConfig.ReleaseGroup;
			var buffer = schematicTestConfig.Buffer;
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow1", buffer, releaseGroupPK: releaseGroup.PK);
			var task1 = VisualBoardsTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow2", buffer, releaseGroupPK: releaseGroup.PK);
			var task2 = VisualBoardsTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var otherReleaseGroup = BMSTestHelper.CreateGroup(Factory, "OTH", "Other Release Group");
			var workflowInOtherRG = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow2", buffer, releaseGroupPK: otherReleaseGroup.PK);
			var taskInOtherRG = VisualBoardsTestHelper.CreateTask(workflowInOtherRG, GlbStaff.CurrentUser.GS_Code, 60);

			var band = BMSTestHelper.CreateAcceptabilityBand(buffer, 1, 2, 3, 4, 5, 6, "Band", type: AcceptabilityBandTypes.Codes.Count);

			Factory.Save();

			var boardId = CreateNewBoard(
				system.PK.ToGuid(),
				releaseGroup.PK.ToGuid(),
				[buffer],
				[],
				[
					new() {
						AcceptabilityBandId = band.PK.ToGuid(),
						Name = band.BAB_Name,
						DisplayName = "Count on board RG AB",
						DisplayUnits = "Units",
						FilterByReleaseGroup = true,
						FilterByComponents = false,
					}
				]);

			var result = acceptabilityBandsService.TryGetWorkflowsMatchingAcceptabilityBand(boardId, band.PK.ToGuid(), out var acceptabilityBandResultDto, out var businessResponse);

			Assert(result);
			AssertNull(businessResponse);
			AssertNotNull(acceptabilityBandResultDto);
			AssertEquals(2, acceptabilityBandResultDto.Length);
		}

		[TestDate(2025, 05, 18, 1, 2, 3)]
		public void Test_TryGetWorkflowsMatchingAcceptabilityBand_WhenFilterByComponents_ShouldGetCorrectResult()
		{
			var system = schematicTestConfig.System;
			var releaseGroup = schematicTestConfig.ReleaseGroup;
			var buffer = schematicTestConfig.Buffer;
			var buffer2 = BMSTestHelper.CreateBuffer(system, "Buffer 2");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow1", buffer);
			var task1 = VisualBoardsTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow2", buffer2);
			var task2 = VisualBoardsTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var workflow3 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow3", buffer2);
			var task3 = VisualBoardsTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);

			var otherBuffer = BMSTestHelper.CreateBuffer(system, "Other Buffer");
			var workflowInOtherBuffer = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow2", otherBuffer);
			var taskInOtherBuffer = VisualBoardsTestHelper.CreateTask(workflowInOtherBuffer, GlbStaff.CurrentUser.GS_Code, 60);

			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 1, 2, 3, 4, 5, 6, "Band", type: AcceptabilityBandTypes.Codes.Count);

			Factory.Save();

			var boardId = CreateNewBoard(
				system.PK.ToGuid(),
				releaseGroup.PK.ToGuid(),
				[buffer, buffer2],
				[],
				[
					new() {
						AcceptabilityBandId = band.PK.ToGuid(),
						Name = band.BAB_Name,
						DisplayName = "Count on board Components AB",
						DisplayUnits = "Units",
						FilterByReleaseGroup = false,
						FilterByComponents = true,
						OverrideBoundaryValues = false,
					}
				]);

			var result = acceptabilityBandsService.TryGetWorkflowsMatchingAcceptabilityBand(boardId, band.PK.ToGuid(), out var acceptabilityBandResultDto, out var businessResponse);

			Assert(result);
			AssertNull(businessResponse);
			AssertNotNull(acceptabilityBandResultDto);
			AssertEquals(3, acceptabilityBandResultDto.Length);
		}

		[TestDate(2025, 05, 18, 1, 2, 3)]
		public void Test_TryGetWorkflowsMatchingAcceptabilityBand_ShouldReturnFalse()
		{
			var result = acceptabilityBandsService.TryGetWorkflowsMatchingAcceptabilityBand(Guid.NewGuid(), Guid.NewGuid(), out var dto, out var businessResponse);

			Assert(!result);
			AssertNotNull(businessResponse);
			AssertNull(dto);
			AssertEquals(businessResponse.Message.Text, AcceptabilityBandsService.BusinessMessages.ErrorLoadingBoardConfigurationInfo.Text);

			var system = schematicTestConfig.System;
			var releaseGroup = schematicTestConfig.ReleaseGroup;
			var buffer = schematicTestConfig.Buffer;
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow1", buffer);
			var task1 = VisualBoardsTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow2", buffer);
			var task2 = VisualBoardsTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var band = BMSTestHelper.CreateAcceptabilityBand(buffer, 1, 2, 3, 4, 5, 6, "Band", type: AcceptabilityBandTypes.Codes.Count);

			Factory.Save();

			var boardId = CreateNewBoard(
				system.PK.ToGuid(),
				releaseGroup.PK.ToGuid(),
				[buffer],
				[],
				[
					new() {
						AcceptabilityBandId = band.PK.ToGuid(),
					}
				]);

			result = acceptabilityBandsService.TryGetWorkflowsMatchingAcceptabilityBand(boardId, Guid.NewGuid(), out var boardHealthWorkflowDto, out businessResponse);

			Assert(!result);
			AssertNotNull(businessResponse);
			AssertNull(dto);
			AssertEquals(businessResponse.Message.Text, AcceptabilityBandsService.BusinessMessages.AcceptabilityBandNotInBoardConfiguration.Text);
		}

		#endregion
	}
}
