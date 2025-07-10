using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ArchiveManager.Integration.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Res = Enterprise.ArchiveManager.Business.Res;

namespace Enterprise.ArchiveManager.Test.Schedule
{
	[TestedType(typeof(ArchiveScheduleTask))]
	[UseSnapshotProtection]
	sealed class ArchiveScheduleTaskTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2007, 5, 15)] // To prevent a leap-year to appear in a 2-year period
		public void TestValidation()
		{
			var task = Factory.New<ArchiveScheduleTask>();
			task.MaxRunDurationInMinutes = 0;

			task.Validation.ValidateAll();

			CombineAssertions(delegate
			{
				AssertHasError(task.S5_ScheduleTypeInfo, "Please enter a value.");
				AssertHasError(task.S5_ScheduleDescriptionInfo, "Please enter a Schedule Task Description.");
				AssertHasError(task.MaxRunDurationInMinutesInfo, "Please enter a value.");

				AssertHasError(task.ArchiveRecordsOnOrBeforeNumberInfo, "Please enter a value.");
				AssertHasError(task.ArchiveRecordsOnOrBeforeTypeInfo, "Please enter a value.");

				AssertNoError(task.ArchiveRecordsOnOrBeforeDateInfo, "Please enter a value.");

				AssertNoError(task.DateParameterInfo, "Please enter a value.");
			});

			task.S5_ScheduleType = "123";
			task.Validation.ValidateAll();
			AssertHasError(task.S5_ScheduleTypeInfo, "Enter a valid selection.");

			task.IsArchiveRecordsOnOrBeforeDate = true;
			task.IsArchiveRecordsOnOrBeforeRelativeDate = false;
			task.ArchiveRecordsOnOrBeforeDate = ZDateTime.Today.AddYears(-1).AddDays(1);
			task.Validation.ValidateAll();
			AssertNoErrors(task.ArchiveRecordsOnOrBeforeDateInfo);

			task.ArchiveRecordsOnOrBeforeDate = ZDateTime.Today.AddYears(-2);
			task.Validation.ValidateAll();
			AssertNoErrors(task.ArchiveRecordsOnOrBeforeDateInfo);

			task.IsArchiveRecordsOnOrBeforeDate = false;
			task.IsArchiveRecordsOnOrBeforeRelativeDate = true;
			task.ArchiveRecordsOnOrBeforeType = "2";
			task.Validation.ValidateAll();
			AssertHasError(task.ArchiveRecordsOnOrBeforeTypeInfo, "Enter a valid selection.");
			task.ArchiveRecordsOnOrBeforeType = "D";
			task.Validation.ValidateAll();
			AssertNoErrors(task.ArchiveRecordsOnOrBeforeTypeInfo);

			task.ArchiveRecordsOnOrBeforeNumber = 309;
			task.Validation.ValidateAll();
			AssertNoErrors(task.ArchiveRecordsOnOrBeforeTypeInfo);
			task.ArchiveRecordsOnOrBeforeNumber = 730;
			task.Validation.ValidateAll();
			AssertNoErrors(task.ArchiveRecordsOnOrBeforeTypeInfo);

			task.S5_ScheduleType = ArchiveManagerConstants.Codes.OPS;
			task.ShouldArchiveDeclaration = false;
			task.Validation.ValidateAll();
			AssertNoWarnings(task.ShouldArchiveDeclarationInfo);
			task.ShouldArchiveDeclaration = true;
			task.Validation.ValidateAll();
			AssertHasWarning(task.ShouldArchiveDeclarationInfo, "The Customs Office of each country/region has strict data retention rules for compliance auditing. In some cases, Customs Office may require jobs to be retained online for more than 5 years. You should consult the relevant compliance legislations before choosing to archive Customs Jobs.");

			AssertHasError(task.DateParameterInfo, "Please enter a value.");
			task.DateParameter = "123";
			task.Validation.ValidateAll();
			AssertHasError(task.DateParameterInfo, "Enter a valid selection.");
			task.DateParameter = DateParameterStrings.GetCode(DateParameterType.JOP);
			task.Validation.ValidateAll();
			AssertHasWarning(task.DateParameterInfo, "Please note that you have chosen to process records based on Job Open Date and this is not the default parameter for archiving.");
			task.S5_ScheduleType = ArchiveManagerConstants.Codes.PDR;
			task.Validation.ValidateAll();
			AssertHasWarning(task.DateParameterInfo, "Please note that you have chosen to process records based on Job Open Date and this is not the default parameter for purging.");
			task.DateParameter = "";
			task.Validation.ValidateAll();
			AssertHasError(task.DateParameterInfo, "Please enter a value.");
			task.DateParameter = DateParameterStrings.GetCode(DateParameterType.JCL);
			task.Validation.ValidateAll();
			AssertNoErrors(task.DateParameterInfo);
			task.S5_ScheduleType = ArchiveManagerConstants.Codes.OPS;
			task.Validation.ValidateAll();
			AssertNoErrors(task.DateParameterInfo);
		}

		[TestDate(2020, 12, 12, 12, 10, 0)]
		public void TestWatermark()
		{
			var task = Factory.New<ArchiveScheduleTask>();
			task.S5_ScheduleType = "ACC";
			Assert("IsArchiveRecordsOnOrBeforeRelativeDate", task.IsArchiveRecordsOnOrBeforeRelativeDate);
			Assert("IsArchiveRecordsOnOrBeforeDate", !task.IsArchiveRecordsOnOrBeforeDate);
			Assert("IsActive", task.S5_IsActive);

			task.IsArchiveRecordsOnOrBeforeRelativeDate = ZBool.True;
			task.ArchiveRecordsOnOrBeforeNumber = 6;
			task.ArchiveRecordsOnOrBeforeType = "M";
			task.MaxRunDurationInMinutes = 60;
			Factory.Save();

			var watermarkAtStart = task.GetWatermark("test stage");
			AssertNull("Watermark at start", watermarkAtStart);

			var newPK = Guid.NewGuid();
			task.SetWatermark("test stage", new ArchiveWatermark { WatermarkDate = ZDateTime.Now.AddHours(1), WatermarkNK = "test", WatermarkPK = newPK });
			var setWatermark = task.GetWatermark("test stage");

			CombineAssertions("Watermark", () =>
			{
				AssertEquals("Watermark date", ZDateTime.Now.AddHours(1), setWatermark.WatermarkDate);
				AssertEquals("Watermark NK", "test", setWatermark.WatermarkNK);
				AssertEquals("Watermark PK", newPK, setWatermark.WatermarkPK);
			});

			task.SetWatermark("test stage", new ArchiveWatermark { WatermarkDate = ZDateTime.MinSmallDateTimeValue, WatermarkNK = string.Empty, WatermarkPK = Guid.Empty });
			var finalWatermark = task.GetWatermark("test stage");
			AssertNull("Final watermark", finalWatermark);
		}

		public void TestSetWatermark_WhenWatermarkIsEmpty()
		{
			var archiveScheduleTask = Factory.New<ArchiveScheduleTask>();
			archiveScheduleTask.S5_ScheduleType = ArchiveManagerConstants.Codes.OPS;
			archiveScheduleTask.IsArchiveRecordsOnOrBeforeRelativeDate = ZBool.True;
			archiveScheduleTask.ArchiveRecordsOnOrBeforeNumber = 6;
			archiveScheduleTask.ArchiveRecordsOnOrBeforeType = "M";
			archiveScheduleTask.MaxRunDurationInMinutes = 60;

			Factory.Save();

			var logger = new TestArchiveLogger();
			archiveScheduleTask.Run(logger, CancellationToken.None);
			var watermark = archiveScheduleTask.GetWatermark(new OPSArchiveStageDescriptor().Name);

			AssertCollectionContains("Reset Watermark Log", "Information|OPS|Watermark was reset to '01-Jan-00 00:00:00'", logger.ListOfMessages);
			AssertNull("Watermark is set correctly", watermark);
		}

		[TestDate(2023, 6, 10, 9, 50, 13)]
		public void TestSetWatermark_WhenWatermarkIsNotEmpty()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK);

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "JOB1";
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipmentPK;
			jobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			jobHeader.JH_A_JCL = ZDateTime.Now;

			var archiveScheduleTask = Factory.New<ArchiveScheduleTask>();
			archiveScheduleTask.S5_ScheduleType = ArchiveManagerConstants.Codes.OPS;
			archiveScheduleTask.IsArchiveRecordsOnOrBeforeRelativeDate = ZBool.True;
			archiveScheduleTask.ArchiveRecordsOnOrBeforeNumber = 0;
			archiveScheduleTask.ArchiveRecordsOnOrBeforeType = "Y";
			archiveScheduleTask.MaxRunDurationInMinutes = 10;

			Factory.Save();

			var logger = new TestArchiveLogger();

			archiveScheduleTask.Run(logger, CancellationToken.None);

			var watermark = archiveScheduleTask.GetWatermark(new OPSArchiveStageDescriptor().Name);
			AssertCollectionContains("Update Watermark Log", "Information|OPS|Watermark was updated to '10-Jun-23 09:50:00'", logger.ListOfMessages);
			AssertNull("Watermark is once again empty at the end because there's no more records", watermark);
		}

		public void TestSerialiseDeserialise()
		{
			var task = Factory.New<ArchiveScheduleTask>();
			task.S5_ScheduleType = ArchiveManagerConstants.Codes.PDO;
			Assert("IsArchiveRecordsOnOrBeforeRelativeDate", task.IsArchiveRecordsOnOrBeforeRelativeDate);
			Assert("IsArchiveRecordsOnOrBeforeDate", !task.IsArchiveRecordsOnOrBeforeDate);
			Assert("IsActive", task.S5_IsActive);
			Assert("ShouldArchiveDeclaration", !task.ShouldArchiveDeclaration);
			Assert("IsVerboseLog", !task.IsVerboseLog);
			Assert("ShouldIncludeRecordsWithJobs", task.ShouldIncludeRecordsWithJobs);
			Assert("ShouldIncludeRecordsWithoutJobs", !task.ShouldIncludeRecordsWithoutJobs);
			Assert("UsingOnOrBeforeDateWhenWatermarkReset", !task.UseOnOrBeforeDateWhenWatermarkReset);

			task.IsArchiveRecordsOnOrBeforeRelativeDate = ZBool.True;
			task.ArchiveRecordsOnOrBeforeNumber = 6;
			task.ArchiveRecordsOnOrBeforeType = "M";
			task.MaxRunDurationInMinutes = 60;
			task.IsVerboseLog = true;
			task.ShouldArchiveDeclaration = true;
			task.ShouldIncludeRecordsWithJobs = false;
			task.ShouldIncludeRecordsWithoutJobs = true;
			task.UseOnOrBeforeDateWhenWatermarkReset = true;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var taskRetrieved = factory2.Load<ArchiveScheduleTask>(task.PK);
			Assert("IsArchiveRecordsOnOrBeforeRelativeDate", taskRetrieved.IsArchiveRecordsOnOrBeforeRelativeDate);
			Assert("IsArchiveRecordsOnOrBeforeDate", !taskRetrieved.IsArchiveRecordsOnOrBeforeDate);
			Assert("IsActive", taskRetrieved.S5_IsActive);
			AssertEquals("ArchiveRecordsOnOrBeforeNumber", 6, taskRetrieved.ArchiveRecordsOnOrBeforeNumber);
			AssertEquals("ArchiveRecordsOnOrBeforeType", "M", taskRetrieved.ArchiveRecordsOnOrBeforeType);
			AssertEquals("MaxRunDuractionInMinutes", 60, taskRetrieved.MaxRunDurationInMinutes);
			Assert("IsVerboseLog", taskRetrieved.IsVerboseLog);
			Assert("ShouldArchiveDeclaration", taskRetrieved.ShouldArchiveDeclaration);
			Assert("ShouldIncludeRecordsWithJobs", !taskRetrieved.ShouldIncludeRecordsWithJobs);
			Assert("ShouldIncludeRecordsWithoutJobs", taskRetrieved.ShouldIncludeRecordsWithoutJobs);
			Assert("UsingOnOrBeforeDateWhenWatermarkReset", taskRetrieved.UseOnOrBeforeDateWhenWatermarkReset);

			task.IsArchiveRecordsOnOrBeforeDate = ZBool.True;
			task.IsArchiveRecordsOnOrBeforeRelativeDate = ZBool.False;
			task.ArchiveRecordsOnOrBeforeDate = new ZDateTime(2009, 1, 14);
			Factory.Save();

			var factory3 = new BusinessObjectFactory();
			var taskRetrieved2 = factory3.Load<ArchiveScheduleTask>(task.PK);
			Assert("IsArchiveRecordsOnOrBeforeRelativeDate", !taskRetrieved2.IsArchiveRecordsOnOrBeforeRelativeDate);
			Assert("IsArchiveRecordsOnOrBeforeDate", taskRetrieved2.IsArchiveRecordsOnOrBeforeDate);
			Assert("IsActive", taskRetrieved2.S5_IsActive);
			AssertEquals("ArchiveRecordsOnOrBeforeNumber", 6, taskRetrieved2.ArchiveRecordsOnOrBeforeNumber);
			AssertEquals("ArchiveRecordsOnOrBeforeType", "M", taskRetrieved2.ArchiveRecordsOnOrBeforeType);
			AssertEquals("ArchiveRecordsOnOrBeforeDate", new ZDateTime(2009, 1, 14), taskRetrieved2.ArchiveRecordsOnOrBeforeDate);
		}

		[UseSnapshotProtection]
		public void TestDeserialise_WhenDateParameterIsSet()
		{
			var task = Factory.New<ArchiveScheduleTask>();
			task.S5_ScheduleType = ArchiveManagerConstants.Codes.OPS;
			task.IsArchiveRecordsOnOrBeforeRelativeDate = ZBool.True;
			task.ArchiveRecordsOnOrBeforeNumber = 7;
			task.ArchiveRecordsOnOrBeforeType = "Y";
			task.MaxRunDurationInMinutes = 2;
			task.DateParameter = DateParameterStrings.GetCode(DateParameterType.JOP);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var taskRetrieved = factory.Load<ArchiveScheduleTask>(task.PK);
			AssertEquals("DateParameter", DateParameterStrings.GetCode(DateParameterType.JOP), taskRetrieved.DateParameter);

			task.DateParameter = DateParameterStrings.GetCode(DateParameterType.JCL);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var taskRetrieved2 = factory2.Load<ArchiveScheduleTask>(task.PK);
			AssertEquals("DateParameter", DateParameterStrings.GetCode(DateParameterType.JCL), taskRetrieved2.DateParameter);
		}

		[UseSnapshotProtection]
		public void TestDeserialise_WhenDateParameterIsNotSet()
		{
			var task = Factory.New<ArchiveScheduleTask>();
			task.S5_ScheduleType = ArchiveManagerConstants.Codes.OPS;
			task.IsArchiveRecordsOnOrBeforeRelativeDate = ZBool.True;
			task.ArchiveRecordsOnOrBeforeNumber = 7;
			task.ArchiveRecordsOnOrBeforeType = "Y";
			task.MaxRunDurationInMinutes = 2;
			task.IsVerboseLog = true;
			task.ShouldArchiveDeclaration = true;
			task.DateParameter = DateParameterStrings.GetCode(DateParameterType.JCL);
			Factory.Save();

			CombineAssertions("Preconditions", () =>
			{
				Assert("IsArchiveRecordsOnOrBeforeRelativeDate", task.IsArchiveRecordsOnOrBeforeRelativeDate);
				Assert("IsArchiveRecordsOnOrBeforeDate", !task.IsArchiveRecordsOnOrBeforeDate);
				Assert("IsActive", task.S5_IsActive);
				AssertEquals("ArchiveRecordsOnOrBeforeNumber", 7, task.ArchiveRecordsOnOrBeforeNumber);
				AssertEquals("ArchiveRecordsOnOrBeforeType", "Y", task.ArchiveRecordsOnOrBeforeType);
				AssertEquals("MaxRunDuractionInMinutes", 2, task.MaxRunDurationInMinutes);
				Assert("IsVerboseLog", task.IsVerboseLog);
				Assert("ShouldArchiveDeclaration", task.ShouldArchiveDeclaration);
				AssertEquals("DateParameter", DateParameterStrings.GetCode(DateParameterType.JCL), task.DateParameter);
			});

			AssertDateParameterIsSetCorrectlyInS5_ScheduleState(task, out var jsonText_DeserializedDictionary);
			_ = jsonText_DeserializedDictionary.Remove("DateParameter");

			Assert("DateParameter is no longer in S5_ScheduleState", !jsonText_DeserializedDictionary.ContainsKey("DateParameter"));

			var jsonText_Reserialized = ArchiveScheduleTask.VersionFlag + JsonSerializer.Serialize(jsonText_DeserializedDictionary);
			var sqlText_UpdatedScheduleState = @"
DECLARE @scheduleState VARBINARY(MAX)
SELECT @scheduleState = dbo.ClrCompressStringAsBytes(@jsonText) 

UPDATE dbo.StmScheduleTask 
	SET S5_ScheduleState = @scheduleState
	WHERE S5_PK = @pk

SELECT dbo.ClrUncompressAsString(S5_ScheduleState)
	FROM StmScheduleTask
	WHERE S5_PK = @pk";

			var updatedJsonText = "";
			using (var command = Db.Connection.Command(sqlText_UpdatedScheduleState))
			{
				updatedJsonText = Db.Connection.ExecuteScalar<string>(command.CommandText, command =>
				{
					_ = command.AddParameter("@jsonText", SqlDbType.NVarChar, jsonText_Reserialized);
					_ = command.AddParameter("@pk", SqlDbType.UniqueIdentifier, task.PK.ToGuid());
				});
			}

			var updatedJsonText_VersionFlagRemoved = updatedJsonText.Substring(ArchiveScheduleTask.VersionFlag.Length);
			var updatedJsonText_DeserializedDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(updatedJsonText_VersionFlagRemoved);

			Assert("Date Parameter is no longer in S5_ScheduleState", !updatedJsonText_DeserializedDictionary.ContainsKey("DateParameter"));

			var archiveLogger = new TestArchiveLogger();
			task.Run(archiveLogger, CancellationToken.None);

			AssertDateParameterIsSetCorrectlyInS5_ScheduleState(task, out _);
		}

		public void TestCalculatedPropertyHasChanges()
		{
			var task = Factory.New<ArchiveScheduleTask>();
			Assert("HasChanges", !task.HasChanges);
			task.IsArchiveRecordsOnOrBeforeDate = true;
			Assert("HasChanges", task.HasChanges);

			task.HasChanges = false;
			Assert("HasChanges", !task.HasChanges);
			task.IsArchiveRecordsOnOrBeforeDate = true;
			Assert("HasChanges", !task.HasChanges);

			task.IsArchiveRecordsOnOrBeforeRelativeDate = false;
			Assert("HasChanges", task.HasChanges);

			task.HasChanges = false;
			Assert("HasChanges", !task.HasChanges);
			task.IsArchiveRecordsOnOrBeforeRelativeDate = false;
			Assert("HasChanges", !task.HasChanges);

			task.ArchiveRecordsOnOrBeforeDate = ZDateTime.Today;
			Assert("HasChanges", task.HasChanges);

			task.HasChanges = false;
			Assert("HasChanges", !task.HasChanges);
			task.ArchiveRecordsOnOrBeforeDate = ZDateTime.Today;
			Assert("HasChanges", !task.HasChanges);

			task.ArchiveRecordsOnOrBeforeNumber = 5;
			Assert("HasChanges", task.HasChanges);

			task.HasChanges = false;
			Assert("HasChanges", !task.HasChanges);
			task.ArchiveRecordsOnOrBeforeNumber = 5;
			Assert("HasChanges", !task.HasChanges);

			task.ArchiveRecordsOnOrBeforeType = "M";
			Assert("HasChanges", task.HasChanges);

			task.HasChanges = false;
			Assert("HasChanges", !task.HasChanges);
			task.ArchiveRecordsOnOrBeforeType = "M";
			Assert("HasChanges", !task.HasChanges);
		}

		public void TestArchiveSystemList()
		{
			var archiveScheduleTask = Factory.New<ArchiveScheduleTask>();
			var archiveSystemList = archiveScheduleTask.ArchiveSystemList;

			var listOfCodes = new List<string> {
				ArchiveManagerConstants.Codes.OPS,
				ArchiveManagerConstants.Codes.PDR,
				ArchiveManagerConstants.Codes.PAR,
				ArchiveManagerConstants.Codes.IPS,
				ArchiveManagerConstants.Codes.STA,
				ArchiveManagerConstants.Codes.PDO,
				ArchiveManagerConstants.Codes.HAR,
			};

			var debugCodes = new List<string> { "DMS", "DMC", "DME", "DMR", "DMM", "DEP", "DMO", "DMP", "DNS", "DMD", "DMA", "DML", "DNN" };
			listOfCodes.AddRange(debugCodes);

			AssertContainsExactElementsInAnyOrder(listOfCodes, archiveSystemList.ToArray().Select(pair => pair.Code));
		}

		public void TestArchiveSystemListForDummyArchiveManager()
		{
			var dummyArchiveScheduleTask = Factory.New<DummyArchiveScheduleTask>();
			var archiveSystemList = dummyArchiveScheduleTask.ArchiveSystemList;

			CombineAssertions(() =>
			{
				AssertEquals("ArchiveSystemList.Count", 2, archiveSystemList.Count);

				AssertEquals("ArchiveSystemList[0].Code", "DM1", archiveSystemList[0].Code);
				AssertEquals("ArchiveSystemList[0].Code", "Dummy Archive System", archiveSystemList[0].Description);

				AssertEquals("ArchiveSystemList[1].Code", "DM2", archiveSystemList[1].Code);
				AssertEquals("ArchiveSystemList[1].Code", "Dummy Archive System", archiveSystemList[1].Description);
			});
		}

		[TestDate(2009, 1, 14, 20, 0, 0)]
		public void TestRunArchiveOnOrBefore2Weeks()
		{
			var dummyTask = Factory.New<DummyArchiveScheduleTask>();
			dummyTask.S5_ScheduleType = "DM1";
			dummyTask.IsArchiveRecordsOnOrBeforeDate = false;
			dummyTask.IsArchiveRecordsOnOrBeforeRelativeDate = true;
			dummyTask.ArchiveRecordsOnOrBeforeNumber = 2;
			dummyTask.ArchiveRecordsOnOrBeforeType = "W";

			dummyTask.Run();

			AssertEquals("ArchiveJobsBeforeThisDate", new DateTime(2008, 12, 31), DummyArchiveManager.LastConfigProcessed.ArchiveJobsOnOrBeforeThisDate);
			AssertEquals("LastSystemCodeProcessed", "DM1", DummyArchiveManager.LastSystemCodeProcessed);
		}

		[TestDate(2009, 1, 14, 20, 0, 0)]
		public void TestRunArchiveOnOrBefore12Days()
		{
			var dummyTask = Factory.New<DummyArchiveScheduleTask>();
			dummyTask.S5_ScheduleType = "DM2";
			dummyTask.IsArchiveRecordsOnOrBeforeDate = false;
			dummyTask.IsArchiveRecordsOnOrBeforeRelativeDate = true;
			dummyTask.ArchiveRecordsOnOrBeforeNumber = 12;
			dummyTask.ArchiveRecordsOnOrBeforeType = "D";

			dummyTask.Run();

			AssertEquals("ArchiveJobsBeforeThisDate", new DateTime(2009, 1, 2), DummyArchiveManager.LastConfigProcessed.ArchiveJobsOnOrBeforeThisDate);
			AssertEquals("LastSystemCodeProcessed", "DM2", DummyArchiveManager.LastSystemCodeProcessed);
		}

		[TestDate(2009, 1, 14, 20, 0, 0)]
		public void TestRunArchiveOnOrBefore3Months()
		{
			var dummyTask = Factory.New<DummyArchiveScheduleTask>();
			dummyTask.S5_ScheduleType = "DM2";
			dummyTask.IsArchiveRecordsOnOrBeforeDate = false;
			dummyTask.IsArchiveRecordsOnOrBeforeRelativeDate = true;
			dummyTask.ArchiveRecordsOnOrBeforeNumber = 3;
			dummyTask.ArchiveRecordsOnOrBeforeType = "M";

			dummyTask.Run();

			AssertEquals("ArchiveJobsBeforeThisDate", new DateTime(2008, 10, 14), DummyArchiveManager.LastConfigProcessed.ArchiveJobsOnOrBeforeThisDate);
			AssertEquals("LastSystemCodeProcessed", "DM2", DummyArchiveManager.LastSystemCodeProcessed);
		}

		[TestDate(2009, 1, 14, 20, 0, 0)]
		public void TestRunArchiveOnOrBefore2Years()
		{
			var dummyTask = Factory.New<DummyArchiveScheduleTask>();
			dummyTask.S5_ScheduleType = "DM2";
			dummyTask.IsArchiveRecordsOnOrBeforeDate = false;
			dummyTask.IsArchiveRecordsOnOrBeforeRelativeDate = true;
			dummyTask.ArchiveRecordsOnOrBeforeNumber = 2;
			dummyTask.ArchiveRecordsOnOrBeforeType = "Y";

			dummyTask.Run();

			AssertEquals("ArchiveJobsBeforeThisDate", new DateTime(2007, 1, 14), DummyArchiveManager.LastConfigProcessed.ArchiveJobsOnOrBeforeThisDate);
			AssertEquals("LastSystemCodeProcessed", "DM2", DummyArchiveManager.LastSystemCodeProcessed);
		}

		[TestDate(2009, 1, 14, 20, 0, 0)]
		public void TestRunArchiveOlderAbsoluteDate()
		{
			var dummyTask = Factory.New<DummyArchiveScheduleTask>();
			dummyTask.S5_ScheduleType = "DM1";
			dummyTask.IsArchiveRecordsOnOrBeforeDate = true;
			dummyTask.ArchiveRecordsOnOrBeforeDate = new ZDateTime(2008, 1, 1);
			dummyTask.IsArchiveRecordsOnOrBeforeRelativeDate = false;
			dummyTask.ArchiveRecordsOnOrBeforeNumber = 2;
			dummyTask.ArchiveRecordsOnOrBeforeType = "Y";

			dummyTask.Run();

			AssertEquals("ArchiveJobsBeforeThisDate", new DateTime(2008, 1, 1), DummyArchiveManager.LastConfigProcessed.ArchiveJobsOnOrBeforeThisDate);
			AssertEquals("LastSystemCodeProcessed", "DM1", DummyArchiveManager.LastSystemCodeProcessed);
		}

		public void TestS5_ScheduleDescription()
		{
			var dummyTask = Factory.New<DummyArchiveScheduleTask>();
			using var mockData = Res.UseMockData();
			mockData.Put("c79f900d-1dc1-46b0-8f26-ac4bbb77d629", new ResourceStringData("c79f900d-1dc1-46b0-8f26-ac4bbb77d629", "系统"));
			mockData.Put("58d88cf2-717f-4804-9f03-01de1bfe2eB7", new ResourceStringData("58d88cf2-717f-4804-9f03-01de1bfe2eB7", "系统"));
			dummyTask.S5_ScheduleDescription = string.Empty;
			dummyTask.S5_ScheduleType = "DM1";
			AssertEquals("Description should not be translated text", "Dummy Archive System", dummyTask.S5_ScheduleDescription);
		}

		public void TestS5_ScheduleDescriptionMultilingual()
		{
			var task = Factory.NewWithValidTestData<ArchiveScheduleTask>();

			using var mockData = Res.UseMockData();
			var customDataResourceStrings = task.S5_ScheduleDescriptionInfo.CustomizableDataResourceStrings;
			var key1 = customDataResourceStrings.GetMultilingualString(task, "Data Import Service").ResourceKey;
			var key2 = customDataResourceStrings.GetMultilingualString(task, "Test Services").ResourceKey;
			mockData.Put(key1, new ResourceStringData(key1, "数据导入服务"));
			mockData.Put(key2, new ResourceStringData(key2, "测试服务"));

			task.S5_ScheduleDescription = "Data Import Service";
			AssertEquals("数据导入服务", task.S5_ScheduleDescriptionMultilingual);

			task.S5_ScheduleDescription = "Test Services";
			AssertEquals("测试服务", task.S5_ScheduleDescriptionMultilingual);
		}

		[TestDate(2014, 10, 10)]
		public void TestArchiveRecordsOnOrBeforeTypeOutOfBoundsValidation()
		{
			var task = Factory.NewWithValidTestData<ArchiveScheduleTask>();
			task.ArchiveRecordsOnOrBeforeType = "Y";
			task.ArchiveRecordsOnOrBeforeNumber = 2015; // sometimes user is confused between "age" and "year" of the job. we assume age, so in this case he will try to archive job created 2015 years ago
			var validator = new ArchiveScheduleTaskValidation(task);
			AssertNoExceptionThrown(validator.ValidateAll);
		}

		[TestDate(2022, 02, 22)]
		[UseSnapshotProtection]
		public void TestWatermarkCorrectlyUpdates_WhenGivenAnExistingWatermark_AndArchiveScheduleRunCompletes()
		{
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			schedule.IsArchiveRecordsOnOrBeforeRelativeDate = true;
			schedule.ArchiveRecordsOnOrBeforeDate = DateTime.Now;
			schedule.ArchiveRecordsOnOrBeforeType = "Y";

			ArchiveManagerDataRegistry.Instance.SetWatermark(schedule.PK.ToGuid(), new OPSArchiveStageDescriptor().Name, new ArchiveWatermark { WatermarkDate = new ZDateTime(2020, 1, 18) });

			var archiveLogger = new TestArchiveLogger();

			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK1);

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipmentPK1;
			jobHeader.JH_Status = JobHeaderStatus.Codes.Closed;

			Factory.Save();

			AssertEquals(1, Factory.GetDatabaseCount(typeof(StmData), new ZQuery(StmDataSchema.SD_Name, "ArchiveWatermark|Operational Jobs Archive")));

			schedule.Run(archiveLogger, CancellationToken.None);

			var watermark = schedule.GetWatermark(new OPSArchiveStageDescriptor().Name);
			var errorCount = ErrorReporter.TotalErrorCount;
			var errors = GetErrors(errorCount);

			CombineAssertions("Logs are correct", () =>
			{
				Assert("Archiving Records on or Before: 22-Feb-2022", archiveLogger.ListOfMessages.Exists(log => log.Contains("Archiving Records on or Before: 22-Feb-2022")));
				Assert("Archived Data Dated Between 18-Jan-20 and 22-Feb-22", archiveLogger.ListOfMessages.Exists(log => log.Contains("Archived Data Dated Between 18-Jan-20 and 22-Feb-22")));
				Assert("Watermark was reset to '01-Jan-00 00:00:00'", archiveLogger.ListOfMessages.Exists(log => log.Contains("Watermark was reset to '01-Jan-00 00:00:00'")));
				Assert($"There should be no errors but there was at least one ({errorCount}), error(s) was: {errors}", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			});

			AssertNull("Watermark is reset correctly", watermark);
		}

		[TestDate(2022, 02, 22)]
		[UseSnapshotProtection]
		public void TestWatermarkUsesNKToBreakTiesCorrectly()
		{
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			schedule.IsArchiveRecordsOnOrBeforeRelativeDate = true;
			schedule.ArchiveRecordsOnOrBeforeDate = ZDateTime.Now;
			schedule.ArchiveRecordsOnOrBeforeType = "Y";

			var archiveLogger = new TestArchiveLogger();

			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK1);

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipmentPK1;
			jobHeader.JH_Status = JobHeaderStatus.Codes.Closed;

			Factory.Save();

			schedule.SetWatermark(new OPSArchiveStageDescriptor().Name, new ArchiveWatermark { WatermarkDate = ZDateTime.Now, WatermarkNK = "ZZ" });

			schedule.Run(archiveLogger, CancellationToken.None);

			var errorCount = ErrorReporter.TotalErrorCount;
			var errors = GetErrors(errorCount);

			Assert("Nothing should be loaded or archived", archiveLogger.ListOfMessages.Exists(log => log.Contains("Time taken to load 0 Archive Set")));
			Assert($"There should be no errors but there was at least one ({errorCount}), error(s) was: {errors}", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));

			schedule.SetWatermark(new OPSArchiveStageDescriptor().Name, new ArchiveWatermark { WatermarkDate = ZDateTime.Now, WatermarkNK = "A" });
			archiveLogger = new TestArchiveLogger();
			schedule.Run(archiveLogger, CancellationToken.None);

			errorCount = ErrorReporter.TotalErrorCount;
			errors = GetErrors(errorCount);

			Assert("Should now load correctly", archiveLogger.ListOfMessages.Exists(log => log.Contains("Time taken to load 1 Archive Set")));
			Assert($"There should be no errors but there was at least one ({errorCount}), error(s) was: {errors}", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
		}

		[TestDate(2022, 02, 22)]
		[UseSnapshotProtection]
		public void TestPKIsUsedToBreakWatermarkBetweenSameNK()
		{
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			schedule.IsArchiveRecordsOnOrBeforeRelativeDate = true;
			schedule.ArchiveRecordsOnOrBeforeDate = ZDateTime.Now;
			schedule.ArchiveRecordsOnOrBeforeType = "Y";

			var archiveLogger = new TestArchiveLogger();

			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK1);

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipmentPK1;
			jobHeader.JH_Status = JobHeaderStatus.Codes.Closed;

			Factory.Save();

			schedule.SetWatermark(new OPSArchiveStageDescriptor().Name, new ArchiveWatermark
			{
				WatermarkDate = ZDateTime.Now,
				WatermarkNK = jobHeader.JH_JobNum,
				WatermarkPK = Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
			});

			schedule.Run(archiveLogger, CancellationToken.None);

			var errorCount = ErrorReporter.TotalErrorCount;
			var errors = GetErrors(errorCount);

			Assert("Nothing should be loaded or archived", archiveLogger.ListOfMessages.Exists(log => log.Contains("Time taken to load 0 Archive Set")));
			Assert($"There should be no errors but there was at least one ({errorCount}), error(s) was: {errors}", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));

			schedule.SetWatermark(new OPSArchiveStageDescriptor().Name, new ArchiveWatermark
			{
				WatermarkDate = ZDateTime.Now,
				WatermarkNK = jobHeader.JH_JobNum,
				WatermarkPK = Guid.Parse("00000000-0000-0000-0000-000000000000")
			});
			archiveLogger = new TestArchiveLogger();
			schedule.Run(archiveLogger, CancellationToken.None);

			errorCount = ErrorReporter.TotalErrorCount;
			errors = GetErrors(errorCount);

			Assert("Should now load correctly", archiveLogger.ListOfMessages.Exists(log => log.Contains("Time taken to load 1 Archive Set")));
			Assert($"There should be no errors but there was at least one ({errorCount}), error was: {errors}", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
		}

		[TestDate(2022, 02, 22)]
		[UseSnapshotProtection]
		public void TestCanHandleUsingPKToTieBreakWhenThereIsNoNKColumn()
		{
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.STA);
			schedule.IsArchiveRecordsOnOrBeforeRelativeDate = true;
			schedule.ArchiveRecordsOnOrBeforeDate = ZDateTime.Now;
			schedule.ArchiveRecordsOnOrBeforeType = "Y";
			schedule.ShouldArchiveDeclaration = true;

			var archiveLogger = new TestArchiveLogger();
			var testGuid = Guid.NewGuid();
			var companyPK = Guid.NewGuid();
			_ = Db.Connection.ExecuteNonQuery(
				$"INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES('{companyPK}', 'AU', 'AUD', 'PPP', 'AU company');" +
				$"INSERT INTO dbo.CusIntrastatHeader " +
				$"(CIH_PK, CIH_GC_Company, CIH_TransactionDate, CIH_ClusterKey, CIH_CountryOfReceipt, CIH_CountryOfSupply, CIH_ConsigneeName, CIH_SupplierName, CIH_TradersReference," +
				$"CIH_SystemCreateTimeUtc, CIH_SystemCreateUser, CIH_SystemLastEditTimeUtc, CIH_SystemLastEditUser)" +
				$"VALUES ('{testGuid}', '{companyPK}', GETDATE(), 1, 'AU', 'AU', '~BP', '~BP', 'A', '{SqlFormatInfo.ToSqlDateTimeString(ZDateTime.Now.ToDateTime())}', '~BP', GETDATE(), '~BP')");

			schedule.SetWatermark(new STACusIntrastatHeaderArchiveStageDescriptor().Name, new ArchiveWatermark
			{
				WatermarkDate = ZDateTime.Now,
				WatermarkPK = Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
			});

			schedule.Run(archiveLogger, CancellationToken.None);

			var errorCount = ErrorReporter.TotalErrorCount;
			var errors = GetErrors(errorCount);

			Assert("Nothing should be loaded or archived", archiveLogger.ListOfMessages.Exists(log => log.Contains("Time taken to load 0 Archive Set(s) of CusIntrastatHeader")));
			Assert($"There should be no errors but there was at least one ({errorCount}), error(s) was: {errors}", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));

			schedule.SetWatermark(new STACusIntrastatHeaderArchiveStageDescriptor().Name, new ArchiveWatermark
			{
				WatermarkDate = ZDateTime.Now,
				WatermarkPK = Guid.Parse("00000000-0000-0000-0000-000000000000")
			});

			archiveLogger = new TestArchiveLogger();
			schedule.Run(archiveLogger, CancellationToken.None);

			Assert("Should now load correctly", archiveLogger.ListOfMessages.Exists(log => log.Contains("Time taken to load 1 Archive Set(s) of CusIntrastatHeader")));
			Assert($"There should be no errors but there was at least one ({errorCount}), error(s) was: {errors}", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
		}

		[UseSnapshotProtection]
		public void TestMultipleConcurrentCallsToAddEdocDoNotError()
		{
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.STA);
			const int numThreads = 20;
			var threads = new List<Thread>();

			for (var i = 0; i < numThreads; i++)
			{
				threads.Add(new Thread(() =>
					{
						using (Db.DisposableActionForDbConnection())
						using (var tempFile = TempFile.NewWithExtension("xls"))
						{
							System.IO.File.WriteAllBytes(tempFile.Filename, new byte[] { 1, 2, 3 });
							AssertNoExceptionThrown(() =>
							{
								schedule.AttachEDoc(tempFile.Filename, "Test", Core.Constants.RefDocTypes.MiscellaneousDocument, "test document");
							});
						}
					})
				);
			}

			threads.ForEach(t => t.Start());
			threads.ForEach(t => t.Join());
		}

		[UseSnapshotProtection]
		[TestDate(2016, 7, 10, 0, 0, 0)]
		public void TestNextRunTimeIsNotSetIfAlreadyInTheFuture()
		{
			var scheduleTask = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.STA);
			scheduleTask.S5_StartDate = new ZDateTime(2011, 10, 1);
			scheduleTask.S5_DailyStartTime = new ZDateTime(1901, 1, 1, 8, 0, 0);
			scheduleTask.Recurrence.TaskPeriod = ScheduleRecurrenceType.Daily;
			scheduleTask.Recurrence.WeekDaysOnly = false;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			scheduleTask.S5_IsActive = true;

			scheduleTask.Factory.Save();
			AssertEquals(new ZDateTime(2016, 7, 10, 8, 0, 0), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
		}

		[UseSnapshotProtection]
		[TestDate(2016, 7, 10, 0, 0, 0)]
		public void TestRunWithExceptionDoesNotDeactivateSilently()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "andytest@wisetechglobal.com";
			var notifications = new NotificationBuffer();
			var scheduleTask = GetArchiveScheduleTaskForTestingErrorNotifications(staff.GS_Code);

			Factory.Save();

			_ = AssertExceptionThrown<Exception>(() => { scheduleTask.RunSafe(notifications, CancellationToken.None); });

			var expectedErrorLog =
@"Archive Schedule Task was deactivated due to an exception. An email was sent to andytest@wisetechglobal.com. The exception was: 'Test Exception'";

			AssertErrorNotifications(notifications, expectedErrorLog);
		}

		[UseSnapshotProtection]
		[TestDate(2016, 7, 10, 0, 0, 0)]
		public void TestEmailExceptionWhenUserCannotBeFound()
		{
			var postMastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			var staff = postMastersGroup.Staff.AddNew();
			staff.GS_EmailAddress = "andytest@wisetechglobal.com";
			staff.GS_Code = "AY2";
			var notifications = new NotificationBuffer();
			var scheduleTask = GetArchiveScheduleTaskForTestingErrorNotifications("ABC");

			Factory.Save();

			_ = AssertExceptionThrown<Exception>(() => { scheduleTask.RunSafe(notifications, CancellationToken.None); });

			var expectedErrorLog =
@"Archive Schedule Task was deactivated due to an exception. An email was sent to the postmaster group, because the user with code 'ABC' could not be found, is inactive or has no email address. The exception was: 'Test Exception'";

			AssertErrorNotifications(notifications, expectedErrorLog);
		}

		[UseSnapshotProtection]
		[TestDate(2016, 7, 10, 0, 0, 0)]
		public void TestEmailExceptionWhenCreatingUserIsInactive()
		{
			var postMastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			var staff = postMastersGroup.Staff.AddNew();
			staff.GS_EmailAddress = "andytest@wisetechglobal.com";
			staff.GS_Code = "AY2";
			Factory.Save();
			var inactiveUser = Factory.NewWithValidTestData<GlbStaff>();
			inactiveUser.GS_EmailAddress = "andyothertest@wisetechglobal.com";
			inactiveUser.GS_IsActive = false;
			inactiveUser.GS_Code = "ABC";
			var notifications = new NotificationBuffer();
			var scheduleTask = GetArchiveScheduleTaskForTestingErrorNotifications(inactiveUser.GS_Code);

			Factory.Save();

			_ = AssertExceptionThrown<Exception>(() => { scheduleTask.RunSafe(notifications, CancellationToken.None); });

			var expectedErrorLog =
@"Archive Schedule Task was deactivated due to an exception. An email was sent to the postmaster group, because the user with code 'ABC' could not be found, is inactive or has no email address. The exception was: 'Test Exception'";

			AssertErrorNotifications(notifications, expectedErrorLog);
		}

		[UseSnapshotProtection]
		[TestDate(2016, 7, 10, 0, 0, 0)]
		public void TestEmailExceptionWhenCreatingUserHasNoEmailAddress()
		{
			var postMastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			var staff = postMastersGroup.Staff.AddNew();
			staff.GS_EmailAddress = "andytest@wisetechglobal.com";
			staff.GS_Code = "AY2";
			Factory.Save();
			var noEmailUser = Factory.NewWithValidTestData<GlbStaff>();
			noEmailUser.GS_IsActive = true;
			noEmailUser.GS_Code = "ABC";
			var notifications = new NotificationBuffer();
			var scheduleTask = GetArchiveScheduleTaskForTestingErrorNotifications(noEmailUser.GS_Code);

			Factory.Save();

			_ = AssertExceptionThrown<Exception>(() => { scheduleTask.RunSafe(notifications, CancellationToken.None); });

			var expectedErrorLog =
@"Archive Schedule Task was deactivated due to an exception. An email was sent to the postmaster group, because the user with code 'ABC' could not be found, is inactive or has no email address. The exception was: 'Test Exception'";

			AssertErrorNotifications(notifications, expectedErrorLog);
		}

		ArchiveScheduleTaskForTestingErrorNotifications GetArchiveScheduleTaskForTestingErrorNotifications(string gs_code)
		{
			var scheduleTask = Factory.New<ArchiveScheduleTaskForTestingErrorNotifications>();
			scheduleTask.MaxRunDurationInMinutes = 1;
			scheduleTask.S5_ScheduleType = "OPS";
			scheduleTask.S5_ScheduleDescription = "Description";
			scheduleTask.S5_StartDate = new ZDateTime(2011, 10, 1);
			scheduleTask.S5_DailyStartTime = new ZDateTime(1901, 1, 1, 8, 0, 0);
			scheduleTask.Recurrence.TaskPeriod = ScheduleRecurrenceType.Daily;
			scheduleTask.Recurrence.WeekDaysOnly = false;
			scheduleTask.Recurrence.TaskPeriodCount = 1;
			scheduleTask.S5_IsActive = true;
			scheduleTask.S5_SystemCreateTimeUtc = new ZDateTime(2011, 10, 1);
			scheduleTask.S5_SystemCreateUser = gs_code;

			return scheduleTask;
		}

		void AssertErrorNotifications(NotificationBuffer notifications, string expectedErrorLog)
		{
			AssertEquals("1 email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var createdEmail = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("Number of recipients is correct", 1, createdEmail.Recipients.Count);
			AssertEquals("Recipient is correct", "andytest@wisetechglobal.com", createdEmail.Recipients[0].Email);
			AssertEquals("Subject is correct", "Archive Schedule Task Was Deactivated", createdEmail.Subject);

			var body =
@"Archive Schedule Task Description was canceled due to an Exception with message: 'Test Exception'.
Please check the error message and reactivate the task.
If the error persists, consider lodging an eRequest.";

			AssertEquals("Email body should be correct", body, createdEmail.Body);
			Assert("Error notification should exist", notifications.HasErrors);

			var errorNotification = notifications.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).FirstOrDefault();
			AssertNotNull("Error should have been notified", errorNotification);

			AssertEquals("Error notification message should be correct", expectedErrorLog, errorNotification.Message);
		}

		sealed class ArchiveScheduleTaskForTestingErrorNotifications : ArchiveScheduleTask
		{
			public ArchiveScheduleTaskForTestingErrorNotifications(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void RunCore(INotifications notifications, CancellationToken token)
			{
				throw new Exception("Test Exception");
			}
		}

		protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory)
		{
			var task = factory.NewWithValidTestData<ArchiveScheduleTask>();
			_ = task.Recipients.AddNew();
			return task;
		}

		string GetErrors(int errorCount)
		{
			var errors = new StringBuilder();
			if (errorCount > 0)
			{
				foreach (var error in ErrorReporter.LastExceptionsReported())
				{
					_ = errors.AppendLine(error);
				}
			}
			return errors.ToString();
		}

		void AssertDateParameterIsSetCorrectlyInS5_ScheduleState(ArchiveScheduleTask task, out Dictionary<string, string> dictionary)
		{
			var sqlText_GetScheduleState = @"SELECT dbo.ClrUncompressAsString(S5_ScheduleState) FROM dbo.StmScheduleTask WHERE S5_PK = @pk";
			var jsonText = "";

			using (var command = Db.Connection.Command(sqlText_GetScheduleState))
			{
				jsonText = Db.Connection.ExecuteScalar<string>(command.CommandText, command =>
				{
					_ = command.AddParameter("@pk", SqlDbType.UniqueIdentifier, task.PK.ToGuid());
				});
			}

			var jsonText_VersionFlagRemoved = jsonText.Substring(ArchiveScheduleTask.VersionFlag.Length);
			var jsonText_DeserializedDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonText_VersionFlagRemoved);
			dictionary = jsonText_DeserializedDictionary;

			AssertDateParameterIsSetCorrectlyInS5_ScheduleState(jsonText_DeserializedDictionary);

			var jsonText_VersionFlagRemoved_WithParameterValuesFilledIn = Encoding.UTF8.GetString(task.S5_ScheduleState).Substring(ArchiveScheduleTask.VersionFlag.Length);
			var jsonText_DeserializedDictionary_WithParameterValuesFilledIn = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonText_VersionFlagRemoved_WithParameterValuesFilledIn);

			AssertDateParameterIsSetCorrectlyInS5_ScheduleState(jsonText_DeserializedDictionary_WithParameterValuesFilledIn);
		}

		void AssertDateParameterIsSetCorrectlyInS5_ScheduleState(Dictionary<string, string> dictionary)
		{
			Assert("Date Parameter is in S5_ScheduleState", dictionary.ContainsKey("DateParameter"));
			AssertEquals("DateParameter should be set to JCL", DateParameterStrings.GetCode(DateParameterType.JCL), dictionary["DateParameter"]);
		}

		class DummyArchiveScheduleTask : ArchiveScheduleTask
		{
			public DummyArchiveScheduleTask(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			protected override IArchiveManager NewArchiveManager()
				=> new DummyArchiveManager();
		}

		class DummyArchiveManager : IArchiveManager
		{
			public DummyArchiveManager()
			{
				ArchiveSystemDescriptors = new List<IArchiveSystemDescriptor>();
				ArchiveSystemDescriptors.Add(new DummyArchiveSystem("DM1"));
				ArchiveSystemDescriptors.Add(new DummyArchiveSystem("DM2"));
			}

			#region IArchiveManager Members

			public void Run(string systemCode, IArchiveConfiguration config, IArchiveLogger logger, IArchiveSchedule schedule, CancellationToken token)
				=> Run(systemCode, config, token);

			public void Run(string systemCode, IArchiveConfiguration config, CancellationToken token)
			{
				LastConfigProcessed = config;
				LastSystemCodeProcessed = systemCode;
			}

			#endregion

			public static IArchiveConfiguration LastConfigProcessed
			{
				get;
				private set;
			}

			public static string LastSystemCodeProcessed
			{
				get;
				private set;
			}

			#region IArchiveManager Members

			public List<IArchiveSystemDescriptor> ArchiveSystemDescriptors
			{
				get;
				private set;
			}

			#endregion

			public void Stop()
				=> throw new NotImplementedException();
		}
	}
}
