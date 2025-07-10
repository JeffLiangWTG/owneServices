using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.AmnestyReporter.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.ServiceTask.Testing
{
	[TestedType(typeof(AmnestiesProcessorServiceTask))]
	class AmnestiesProcessorServiceTaskTest : ServiceTaskTestCase<AmnestiesProcessorServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));

			var serviceTask = new AmnestiesProcessorServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;

			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(serviceTask.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => serviceTask.RunTask());
			}
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		static IDisposable ClearUserContext()
		{
			var userContext = EnvProxy.Instance.CurrentUserContext;
			EnvProxy.Instance.ClearUserContext();
			(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();
			return new DisposableAction(() =>
			{
				EnvProxy.Instance.SetUserContext(userContext);
			});
		}

		public void TestDatabaseFilling()
		{
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				AssertEquals("Count of AmnestyFailures", 17, (int)connection.ExecuteScalar("SELECT COUNT(*) FROM [dbo].[AmnestyFailures]"));
				AssertEquals("Count of TestMethod     ", 11, (int)connection.ExecuteScalar("SELECT COUNT(*) FROM [dbo].[TestMethod]     "));
				AssertEquals("Count of TestClass      ", 11, (int)connection.ExecuteScalar("SELECT COUNT(*) FROM [dbo].[TestClass]      "));
				AssertEquals("Count of Assembly       ", 10, (int)connection.ExecuteScalar("SELECT COUNT(*) FROM [dbo].[Assembly]       "));
				AssertEquals("Count of User           ", 7, (int)connection.ExecuteScalar("SELECT COUNT(*) FROM [dbo].[User]           "));
			}
		}

		public void TestDownloadAmnestiesWithAdditions()
		{
			var task = new AmnestiesProcessorServiceTask();
			InsertStaffTableRow();
			var res = task.Download().OrderBy(failure => failure.AF_PK).ToArray();
			AssertEquals(nameof(res), 6, res.Length);
			AssertEquals(nameof(res), 6, res.Count(failure => !failure.AF_IM.HasValue || failure.AF_IM.Value == Guid.Empty));
			AssertResult(res[0], new DatAmnestyFailure { AF_PK = Guid.Parse("150c1489-6a86-4dfa-994a-12a9a8bce159"), AF_StartDate = new DateTime(2015, 11, 18, 20, 0, 18, 613), E6_PK = Guid.Parse("820a3ded-f1d4-4843-8f7a-7cdbd5760e03"), E6_MethodName = "TestTransform_PartiallyCommittedPartiallyTaken", E2_TestClass = "Enterprise.DbUpgrader.Transformations.Testing.CreateMatchingDocketLineForEachInventoryTest", E8_AssemblyName = "Enterprise.DbUpgrader.Transformations", ST_ResponsibleUser = "CORP\\Bret.Ehlert", ST_Product = "POI", ST_ProductArea = "LKJ", ST_Module = "YHN" });
			AssertResult(res[1], new DatAmnestyFailure { AF_PK = Guid.Parse("84DA21DA-41F4-4FCA-BA44-12797CB7AC8A"), AF_StartDate = new DateTime(2014, 5, 29, 7, 0, 24, 977, DateTimeKind.Local), E6_PK = Guid.Parse("7cb454c7-ddd6-45f2-9045-e8e42c10c22e"), E6_MethodName = "TestUnitDutiableWGTVOLQTYUnit", E2_TestClass = "Enterprise.Customs.SG.V4.Business.Testing.AddInfoJobComInvoiceLineValidation_IPTTest", E8_AssemblyName = "Enterprise.Customs.SG.V4.Business", ST_ResponsibleUser = "CORP\\Bret.Ehlert", ST_Product = "123", ST_ProductArea = "456", ST_Module = "789" });
			AssertResult(res[2], new DatAmnestyFailure { AF_PK = Guid.Parse("972a8499-421b-45e4-a35e-12bdd4162a65"), AF_StartDate = new DateTime(2014, 5, 29, 7, 0, 35, 740, DateTimeKind.Local), E6_PK = Guid.Parse("3b3faea0-07c5-4d72-95f2-9173c55c9456"), E6_MethodName = "TestTotalDutiableWGTVOLQTYUnit", E2_TestClass = "Enterprise.Customs.SG.V4.Business.Testing.AddInfoJobComInvoiceLineValidationTest", E8_AssemblyName = "Enterprise.Customs.SG.V4.Business", ST_ResponsibleUser = "CORP\\Bret.Ehlert", ST_Product = "123", ST_ProductArea = "456", ST_Module = "789" });
			AssertResult(res[3], new DatAmnestyFailure { AF_PK = Guid.Parse("a0e281b4-31ba-4696-8bc9-1283a6f77a8f"), AF_StartDate = new DateTime(2016, 4, 24, 4, 28, 1, 617, DateTimeKind.Local), E6_PK = Guid.Parse("bb73d5f6-95c8-408b-b6d4-04d0536f0aff"), E6_MethodName = "SuccessfulShelfCheckIn", E2_TestClass = "Dat.Testing.EndToEndAutoDeployTestedShelfTests", E8_AssemblyName = "DAT\\Testing\\Dat.Testing.dll", ST_ResponsibleUser = "", ST_Product = "GLW", ST_ProductArea = "", ST_Module = "" });
			AssertResult(res[4], new DatAmnestyFailure { AF_PK = Guid.Parse("d0884e31-834c-4552-8df6-1290c8f13526"), AF_StartDate = new DateTime(2015, 10, 4, 0, 28, 0, 287, DateTimeKind.Local), E6_PK = Guid.Parse("496ca45f-76fd-4669-8cf3-fed342ebe114"), E6_MethodName = "TestPacking_GUI", E2_TestClass = "AnalyzersRunner.RunAnalyzers.Runner", E8_AssemblyName = "AnalyzersRunner.RunAnalyzers", ST_ResponsibleUser = "Bret.Ehlert", ST_Product = "ENT", ST_ProductArea = "PER", ST_Module = "APP" });
			AssertResult(res[5], new DatAmnestyFailure { AF_PK = Guid.Parse("ed2b1145-4f8f-4f6d-bea4-12a1b7d5d004"), AF_StartDate = new DateTime(2015, 7, 25, 3, 7, 59, 680, DateTimeKind.Local), E6_PK = Guid.Parse("2cdbf5a2-810a-4d04-8936-5b4d017f7882"), E6_MethodName = "TestRunEnterpriseOnEmptyDatabase", E2_TestClass = "Enterprise.Startup.UpgradeAssuranceTest", E8_AssemblyName = "CargoWiseOne", ST_ResponsibleUser = "Bret.Ehlert", ST_Product = "ENT", ST_ProductArea = "ARC", ST_Module = "" });
		}

		static void AssertResult(DatAmnestyFailure value, DatAmnestyFailure expected)
		{
			AssertEquals(nameof(DatAmnestyFailure.AF_PK), expected.AF_PK, value.AF_PK);
			AssertEquals(nameof(DatAmnestyFailure.AF_StartDate), expected.AF_StartDate, value.AF_StartDate);
			AssertEquals(nameof(DatAmnestyFailure.AF_IM), expected.AF_IM, value.AF_IM);
			AssertEquals(nameof(DatAmnestyFailure.E6_MethodName), expected.E6_MethodName, value.E6_MethodName);
			AssertEquals(nameof(DatAmnestyFailure.E2_TestClass), expected.E2_TestClass, value.E2_TestClass);
			AssertEquals(nameof(DatAmnestyFailure.E8_AssemblyName), expected.E8_AssemblyName, value.E8_AssemblyName);
			AssertEquals(nameof(DatAmnestyFailure.ST_ResponsibleUser), expected.ST_ResponsibleUser, value.ST_ResponsibleUser);
			AssertEquals(nameof(DatAmnestyFailure.ST_Product), expected.ST_Product, value.ST_Product);
			AssertEquals(nameof(DatAmnestyFailure.ST_ProductArea), expected.ST_ProductArea, value.ST_ProductArea);
			AssertEquals(nameof(DatAmnestyFailure.ST_Module), expected.ST_Module, value.ST_Module);
		}

		public void TestSaveWorkItemsToAmnesties()
		{
			var task = new AmnestiesProcessorServiceTask();
			var failures = task.Download().Where(failure => !failure.AF_IM.HasValue).ToArray();
			AssertEquals(6, failures.Length);
			var workItemToAmnesties = new Dictionary<ZGuid, List<Guid>> { { ZGuid.NewZGuid(), new List<Guid> { failures[0].AF_PK, failures[1].AF_PK } }, { ZGuid.NewZGuid(), new List<Guid> { failures[2].AF_PK } }, { ZGuid.BrettsGuid, new List<Guid> { failures[3].AF_PK, failures[4].AF_PK, failures[5].AF_PK } } };
			// save
			AmnestiesProcessorServiceTask.SaveWorkItemsToAmnesties(workItemToAmnesties);
			var newFailures = task.Download().Where(failure => !failure.AF_IM.HasValue).ToArray();
			AssertEquals("Should be now unassigned Amnesty Failures", 0, newFailures.Length);
			var amnesties = new List<DatAmnestyFailure>();
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			using (var cmd = connection.Command($@"
SELECT [AF_PK], [AF_E6], [AF_StartDate], [AF_ExpiryDate], [AF_IM]
FROM [dbo].[AmnestyFailures]
WHERE [AF_PK] IN ({AmnestiesProcessorServiceTask.JoinGuidsToSafeSqlInClause(failures, failure => failure.AF_PK)})"))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						amnesties.Add(new DatAmnestyFailure { AF_PK = reader.GetGuid(0), AF_IM = reader.GetGuid(4) });
					}
				}
			}

			AssertEquals("Amnesties count", failures.Length, amnesties.Count);
			foreach (var failure in amnesties)
			{
				AssertNotNull("Not NULL WorkItem PK", failure.AF_IM);
				AssertEquals("Assigned WorkItem PK test", workItemToAmnesties.Single(pair => pair.Value.Contains(failure.AF_PK)).Key, failure.AF_IM);
			}
		}

		public void TestProcessWrongCall()
		{
			var task = new AmnestiesProcessorServiceTask();
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: datBundle", () => task.Process(null, TimeSpan.FromMinutes(1), CancellationToken.None));
		}

		public void TestProcessFailuresInLog()
		{
			var logger = new TestServiceLogger();
			var serviceTask = new AmnestiesProcessorServiceTask { ServiceLogger = logger };
			var dat = serviceTask.Download().ToArray();
			dat[0].ST_Product = "";
			serviceTask.Process(dat, TimeSpan.FromMinutes(1), CancellationToken.None);
			var expectedValues = new[] { Tuple.Create("Information|Processing Amnesty Failure '84da21da-41f4-4fca-ba44-12797cb7ac8a'.", false), Tuple.Create("Error|Missing product code for Amnesty Failure Enterprise.Customs.SG.V4.Business/Enterprise.Customs.SG.V4.Business.Testing.AddInfoJobComInvoiceLineValidation_IPTTest/TestUnitDutiableWGTVOLQTYUnit).", false), Tuple.Create("Information|Processing Amnesty Failure 'a0e281b4-31ba-4696-8bc9-1283a6f77a8f'.", false), Tuple.Create(@"Warning|Missing product module for Amnesty Failure DAT\Testing\Dat.Testing.dll/Dat.Testing.EndToEndAutoDeployTestedShelfTests/SuccessfulShelfCheckIn).", false), Tuple.Create(@"Warning|Missing product area for Amnesty Failure DAT\Testing\Dat.Testing.dll/Dat.Testing.EndToEndAutoDeployTestedShelfTests/SuccessfulShelfCheckIn).", false), Tuple.Create("Information|Processing Amnesty Failure 'd0884e31-834c-4552-8df6-1290c8f13526'.", false), Tuple.Create("Information|Processing Amnesty Failure 'ed2b1145-4f8f-4f6d-bea4-12a1b7d5d004'.", false), Tuple.Create("Warning|Missing product module for Amnesty Failure CargoWiseOne/Enterprise.Startup.UpgradeAssuranceTest/TestRunEnterpriseOnEmptyDatabase).", false), Tuple.Create("Information|Processing Amnesty Failure '150c1489-6a86-4dfa-994a-12a9a8bce159'.", false), Tuple.Create("Information|Processing Amnesty Failure '972a8499-421b-45e4-a35e-12bdd4162a65'.", false), Tuple.Create("Information|Processed amnesty failures: 5 items in ", true), Tuple.Create("Information|Saving amnesty failures: 5 items in ", true) };
			CombineAssertions(() =>
			{
				AssertEquals("Log count", expectedValues.Length, logger.Count);
				for (var i = 0; i < expectedValues.Length; i++)
				{
					if (expectedValues[i].Item2)
					{
						AssertStartsWith($"Log row {i + 1}", expectedValues[i].Item1, logger[i]);
					}
					else
					{
						AssertEquals($"Log row {i + 1}", expectedValues[i].Item1, logger[i]);
					}
				}
			});
		}

		public void TestProcessEmptyResults()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			// Leave only Bret Ehlert :)
			var dat = serviceTask.Download();
			serviceTask.Process(dat, TimeSpan.FromMinutes(1), CancellationToken.None);
			var logs = Factory.Load<HelpErrorLog>(new ZQuery());
			AssertEquals("Log should be empty", 0, logs.Length);
		}

		[TestDate(2016, 3, 16, 16, 20, 0)]
		public void TestAmnestiesDoNotAutoAssignWhenTheWorkflowTemplateHasCapabilityTasks()
		{
			var serviceTask = new AmnestiesProcessorServiceTask();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "WKI";
			var task1 = template1.WorkflowItems.Tasks.AddNew();
			task1.P9_G4_RequiredCapability = capability.PK;
			Factory.Save();
			InsertStaffTableRow();
			var dat = serviceTask.Download().ToArray();
			serviceTask.Process(dat, TimeSpan.FromMinutes(1), CancellationToken.None);
			var workItem = Factory.Load<WorkItem>(new ZQuery()).First();
			AssertEquals("Should use template rather than auto assigning.", string.Empty, workItem.WorkflowItems.Tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should use template rather than auto assigning.", capability.PK, workItem.WorkflowItems.Tasks[0].P9_G4_RequiredCapability);
		}

		[TestDate(2016, 3, 16, 16, 20, 0)]
		public void TestProcessResults()
		{
			var expectedEditDate = ZDateTime.Now;
			var serviceTask = new AmnestiesProcessorServiceTask();
			InsertStaffTableRow();
			var dat = serviceTask.Download().ToArray();
			serviceTask.Process(dat, TimeSpan.FromMinutes(1), CancellationToken.None);
			var workItems = Factory.Load<WorkItem>(new ZQuery()).OrderBy(workItem => workItem.Number).ToArray();
			AssertEquals("WorkItems contains results", 5, workItems.Length);
			{
				var workItem = workItems[0];
				AssertNotNullOrEmpty(nameof(workItem.PK), workItem.PK.ToString());
				AssertNotNullOrEmpty(nameof(workItem.PK), workItem.PK.ToString());
				AssertEquals(nameof(workItem.WKI_WorkItemType), "123", workItem.WKI_WorkItemType);
				AssertEquals(nameof(workItem.WKI_WorkItemArea), "456", workItem.WKI_WorkItemArea);
				AssertEquals(nameof(workItem.WKI_ActivityType), "789", workItem.WKI_ActivityType);
				AssertEquals(nameof(workItem.WKI_ActivitySubtype), "AMN", workItem.WKI_ActivitySubtype);
				AssertEquals(nameof(workItem.WKI_Priority), "GPR", workItem.WKI_Priority);
				AssertEquals(nameof(workItem.WKI_SystemCreateUser), "~BP", workItem.WKI_SystemCreateUser); // Default for empty user - CargoWise Support
				AssertEquals(nameof(workItem.WKI_SystemLastEditUser), "~BP", workItem.WKI_SystemLastEditUser); // Default for empty user - CargoWise Support
				AssertNotEquals(nameof(workItem.WKI_SystemCreateTimeUtc), new DateTime(2014, 5, 29, 7, 0, 24, 977), workItem.WKI_SystemCreateTimeUtc);
				AssertEquals(nameof(workItem.WKI_SystemLastEditTimeUtc), expectedEditDate, workItem.WKI_SystemLastEditTimeUtc);
				AssertEquals(nameof(workItem.WKI_Status), "CLS", workItem.WKI_Status);
				AssertEquals(nameof(workItem.WKI_Details), "Assembly: Enterprise.Customs.SG.V4.Business\r\nClass: Enterprise.Customs.SG.V4.Business.Testing.AddInfoJobComInvoiceLineValidation_IPTTest\r\nMethod: TestUnitDutiableWGTVOLQTYUnit\r\n" + "http://crikey.wtg.zone/failures/testFailureHistory/7cb454c7-ddd6-45f2-9045-e8e42c10c22e\r\n\r\n" + "Assembly: Enterprise.Customs.SG.V4.Business\r\nClass: Enterprise.Customs.SG.V4.Business.Testing.AddInfoJobComInvoiceLineValidationTest\r\nMethod: TestTotalDutiableWGTVOLQTYUnit\r\n" + "http://crikey.wtg.zone/failures/testFailureHistory/3b3faea0-07c5-4d72-95f2-9173c55c9456\r\n\r\n", workItem.WKI_Details.ToUTF8());
			}

			{
				var workItem = workItems[1];
				AssertNotNullOrEmpty(nameof(workItem.PK), workItem.PK.ToString());
				AssertNotNullOrEmpty(nameof(workItem.PK), workItem.PK.ToString());
				AssertEquals(nameof(workItem.WKI_WorkItemType), "GLW", workItem.WKI_WorkItemType);
				Assert(nameof(workItem.WKI_WorkItemArea), workItem.WKI_WorkItemArea.IsEmpty);
				Assert(nameof(workItem.WKI_ActivityType), workItem.WKI_ActivityType.IsEmpty);
				AssertEquals(nameof(workItem.WKI_ActivitySubtype), "AMN", workItem.WKI_ActivitySubtype);
				AssertEquals(nameof(workItem.WKI_Priority), "GPR", workItem.WKI_Priority);
				AssertEquals(nameof(workItem.WKI_SystemCreateUser), "~BP", workItem.WKI_SystemCreateUser); // Default for empty user - CargoWise Support
				AssertEquals(nameof(workItem.WKI_SystemLastEditUser), "~BP", workItem.WKI_SystemLastEditUser); // Default for empty user - CargoWise Support
				AssertNotEquals(nameof(workItem.WKI_SystemCreateTimeUtc), new DateTime(2016, 4, 24, 4, 28, 1, 617), workItem.WKI_SystemCreateTimeUtc);
				AssertEquals(nameof(workItem.WKI_SystemLastEditTimeUtc), expectedEditDate, workItem.WKI_SystemLastEditTimeUtc);
				AssertEquals(nameof(workItem.WKI_Status), "CLS", workItem.WKI_Status);
				AssertEquals(nameof(workItem.WKI_Details), "Assembly: DAT\\Testing\\Dat.Testing.dll\r\nClass: Dat.Testing.EndToEndAutoDeployTestedShelfTests\r\nMethod: SuccessfulShelfCheckIn\r\n" + "http://crikey.wtg.zone/failures/testFailureHistory/bb73d5f6-95c8-408b-b6d4-04d0536f0aff\r\n\r\n", workItem.WKI_Details.ToUTF8());
			}

			{
				var workItem = workItems[2];
				AssertNotNullOrEmpty(nameof(workItem.PK), workItem.PK.ToString());
				AssertNotNullOrEmpty(nameof(workItem.PK), workItem.PK.ToString());
				AssertEquals(nameof(workItem.WKI_WorkItemType), "ENT", workItem.WKI_WorkItemType);
				AssertEquals(nameof(workItem.WKI_WorkItemArea), "PER", workItem.WKI_WorkItemArea);
				AssertEquals(nameof(workItem.WKI_ActivityType), "APP", workItem.WKI_ActivityType);
				AssertEquals(nameof(workItem.WKI_ActivitySubtype), "AMN", workItem.WKI_ActivitySubtype);
				AssertEquals(nameof(workItem.WKI_Priority), "GPR", workItem.WKI_Priority);
				AssertEquals(nameof(workItem.WKI_SystemCreateUser), "~BP", workItem.WKI_SystemCreateUser); // Default for empty user - CargoWise Support
				AssertEquals(nameof(workItem.WKI_SystemLastEditUser), "~BP", workItem.WKI_SystemLastEditUser); // Default for empty user - CargoWise Support
				AssertNotEquals(nameof(workItem.WKI_SystemCreateTimeUtc), new DateTime(2015, 10, 4, 0, 28, 0, 287), workItem.WKI_SystemCreateTimeUtc);
				AssertEquals(nameof(workItem.WKI_SystemLastEditTimeUtc), expectedEditDate, workItem.WKI_SystemLastEditTimeUtc);
				AssertEquals(nameof(workItem.WKI_Status), "CLS", workItem.WKI_Status);
				AssertEquals(nameof(workItem.WKI_Details), "Assembly: AnalyzersRunner.RunAnalyzers\r\nClass: AnalyzersRunner.RunAnalyzers.Runner\r\nMethod: TestPacking_GUI\r\n" + "http://crikey.wtg.zone/failures/testFailureHistory/496ca45f-76fd-4669-8cf3-fed342ebe114\r\n\r\n", workItem.WKI_Details.ToUTF8());
			}

			{
				var workItem = workItems[3];
				AssertNotNullOrEmpty(nameof(workItem.PK), workItem.PK.ToString());
				AssertNotNullOrEmpty(nameof(workItem.PK), workItem.PK.ToString());
				AssertEquals(nameof(workItem.WKI_WorkItemType), "ENT", workItem.WKI_WorkItemType);
				AssertEquals(nameof(workItem.WKI_WorkItemArea), "ARC", workItem.WKI_WorkItemArea);
				Assert(nameof(workItem.WKI_ActivityType), workItem.WKI_ActivityType.IsEmpty);
				AssertEquals(nameof(workItem.WKI_ActivitySubtype), "AMN", workItem.WKI_ActivitySubtype);
				AssertEquals(nameof(workItem.WKI_Priority), "GPR", workItem.WKI_Priority);
				AssertEquals(nameof(workItem.WKI_SystemCreateUser), "~BP", workItem.WKI_SystemCreateUser); // Default for empty user - CargoWise Support
				AssertEquals(nameof(workItem.WKI_SystemLastEditUser), "~BP", workItem.WKI_SystemLastEditUser); // Default for empty user - CargoWise Support
				AssertNotEquals(nameof(workItem.WKI_SystemCreateTimeUtc), new DateTime(2015, 7, 25, 3, 7, 59, 680), workItem.WKI_SystemCreateTimeUtc);
				AssertEquals(nameof(workItem.WKI_SystemLastEditTimeUtc), expectedEditDate, workItem.WKI_SystemLastEditTimeUtc);
				AssertEquals(nameof(workItem.WKI_Status), "CLS", workItem.WKI_Status);
				AssertEquals(nameof(workItem.WKI_Details), "Assembly: CargoWiseOne\r\nClass: Enterprise.Startup.UpgradeAssuranceTest\r\nMethod: TestRunEnterpriseOnEmptyDatabase\r\n" + "http://crikey.wtg.zone/failures/testFailureHistory/2cdbf5a2-810a-4d04-8936-5b4d017f7882\r\n\r\n", workItem.WKI_Details.ToUTF8());
			}

			{
				var workItem = workItems[4];
				AssertNotNullOrEmpty(nameof(workItem.PK), workItem.PK.ToString());
				AssertNotNullOrEmpty(nameof(workItem.PK), workItem.PK.ToString());
				AssertEquals(nameof(workItem.WKI_WorkItemType), "POI", workItem.WKI_WorkItemType);
				AssertEquals(nameof(workItem.WKI_WorkItemArea), "LKJ", workItem.WKI_WorkItemArea);
				AssertEquals(nameof(workItem.WKI_ActivityType), "YHN", workItem.WKI_ActivityType);
				AssertEquals(nameof(workItem.WKI_ActivitySubtype), "AMN", workItem.WKI_ActivitySubtype);
				AssertEquals(nameof(workItem.WKI_Priority), "GPR", workItem.WKI_Priority);
				AssertEquals(nameof(workItem.WKI_SystemCreateUser), "~BP", workItem.WKI_SystemCreateUser); // Default for empty user - CargoWise Support
				AssertEquals(nameof(workItem.WKI_SystemLastEditUser), "~BP", workItem.WKI_SystemLastEditUser); // Default for empty user - CargoWise Support
				AssertNotEquals(nameof(workItem.WKI_SystemCreateTimeUtc), new DateTime(2015, 11, 18, 20, 0, 18, 613), workItem.WKI_SystemCreateTimeUtc);
				AssertEquals(nameof(workItem.WKI_SystemLastEditTimeUtc), expectedEditDate, workItem.WKI_SystemLastEditTimeUtc);
				AssertEquals(nameof(workItem.WKI_Status), "CLS", workItem.WKI_Status);
				AssertEquals(nameof(workItem.WKI_Details), "Assembly: Enterprise.DbUpgrader.Transformations\r\nClass: Enterprise.DbUpgrader.Transformations.Testing.CreateMatchingDocketLineForEachInventoryTest\r\nMethod: TestTransform_PartiallyCommittedPartiallyTaken\r\n" + "http://crikey.wtg.zone/failures/testFailureHistory/820a3ded-f1d4-4843-8f7a-7cdbd5760e03\r\n\r\n", workItem.WKI_Details.ToUTF8());
			}
		}

		public void TestNullCrikeyConnectionDoesNotThrow()
		{
			DbConnectionCrikey.NoCrikeyConnection.Value = true;
			var serviceLogger = new TestServiceLogger();
			new AmnestiesProcessorServiceTask()
			{ ServiceLogger = serviceLogger }.RunTask();
			AssertContains("Crikey db server not configured, this is probably not the production instance of ediProd", serviceLogger.ToString());
		}

		public void TestGitTeamProjectCollectionResponsibility()
		{
			var amnesty = AddGitAmnesty("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode/myfunction");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection", "A", "B", "C");
			var result = new AmnestiesProcessorServiceTask().Download().Single(a => a.AF_PK == amnesty);
			AssertEquals("A", result.ST_Product);
			AssertEquals("B", result.ST_ProductArea);
			AssertEquals("C", result.ST_Module);
		}

		public void TestGitTeamProjectResponsibility()
		{
			var amnesty = AddGitAmnesty("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode/myfunction");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject", "A", "B", "C");
			var result = new AmnestiesProcessorServiceTask().Download().Single(a => a.AF_PK == amnesty);
			AssertEquals("A", result.ST_Product);
			AssertEquals("B", result.ST_ProductArea);
			AssertEquals("C", result.ST_Module);
		}

		public void TestGitRepositoryResponsibility()
		{
			var amnesty = AddGitAmnesty("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode/myfunction");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository", "A", "B", "C");
			var result = new AmnestiesProcessorServiceTask().Download().Single(a => a.AF_PK == amnesty);
			AssertEquals("A", result.ST_Product);
			AssertEquals("B", result.ST_ProductArea);
			AssertEquals("C", result.ST_Module);
		}

		public void TestGitRepositoryPathResponsibility()
		{
			var amnesty = AddGitAmnesty("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode/myfunction");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode", "A", "B", "C");
			var result = new AmnestiesProcessorServiceTask().Download().Single(a => a.AF_PK == amnesty);
			AssertEquals("A", result.ST_Product);
			AssertEquals("B", result.ST_ProductArea);
			AssertEquals("C", result.ST_Module);
		}

		public void TestGitHubRepositoryPathResponsibility()
		{
			var amnesty = AddGitAmnesty("https://github.com/WiseTechGlobal/MyProject?path=mycode/myfunction");
			AddSourceTreeResponsibility("https://github.com/WiseTechGlobal/MyProject?path=mycode", "A", "B", "C");
			var result = new AmnestiesProcessorServiceTask().Download().Single(a => a.AF_PK == amnesty);
			AssertEquals("A", result.ST_Product);
			AssertEquals("B", result.ST_ProductArea);
			AssertEquals("C", result.ST_Module);
		}

		public void TestGitRepositoryPathResponsibilityExactMatch()
		{
			var amnesty = AddGitAmnesty("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode/myfunction");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode/myfunction", "A", "B", "C");
			var result = new AmnestiesProcessorServiceTask().Download().Single(a => a.AF_PK == amnesty);
			AssertEquals("A", result.ST_Product);
			AssertEquals("B", result.ST_ProductArea);
			AssertEquals("C", result.ST_Module);
		}

		public void TestMoreSpecificGitSourceTreeReposibilityUsed()
		{
			var amnesty = AddGitAmnesty("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode/myfunction");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject", "A", "A", "A");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository", "B", "B", "B");
			var result = new AmnestiesProcessorServiceTask().Download().Single(a => a.AF_PK == amnesty);
			AssertEquals("B", result.ST_Product);
			AssertEquals("B", result.ST_ProductArea);
			AssertEquals("B", result.ST_Module);
		}

		public void TestMoreSpecificGitSourceTreeReposibilityUsedWithPath()
		{
			var amnesty = AddGitAmnesty("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode/myfunction");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode", "A", "A", "A");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository", "B", "B", "B");
			var result = new AmnestiesProcessorServiceTask().Download().Single(a => a.AF_PK == amnesty);
			AssertEquals("A", result.ST_Product);
			AssertEquals("A", result.ST_ProductArea);
			AssertEquals("A", result.ST_Module);
		}

		public void TestGitSourceTreePartialPathNotMatched()
		{
			var amnesty = AddGitAmnesty("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode/myfunction");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode", "A", "A", "A");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/my", "B", "B", "B");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/My", "C", "C", "C");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/My", "D", "D", "D");
			var result = new AmnestiesProcessorServiceTask().Download().Single(a => a.AF_PK == amnesty);
			AssertEquals("A", result.ST_Product);
			AssertEquals("A", result.ST_ProductArea);
			AssertEquals("A", result.ST_Module);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		static void InsertStaffTableRow()
		{
			var factory = new BusinessObjectFactory(Db.Connection);
			var row = factory.New<GlbStaff>();
			row.GS_Code = "BRE";
			row.GS_LoginName = "bret.ehlert";
			row = factory.New<GlbStaff>();
			row.GS_Code = "DS ";
			row.GS_LoginName = "corp\\Dalmo.Serravalle";
			// empty login names for resources
			row = factory.New<GlbStaff>();
			row.GS_Code = "TS1";
			row.GS_LoginName = "";
			row.GS_IsResource = true;
			row = factory.New<GlbStaff>();
			row.GS_Code = "TS2";
			row.GS_LoginName = "";
			row.GS_IsResource = true;
			factory.Save();
		}

		static Guid AddGitAmnesty(string sourcePath)
		{
			return AddAmnesty(AddMethod(AddClass(AddAssembly("MyAssembly.dll", sourcePath), "MyClass"), "MyFunction"));
		}

		static Guid AddAssembly(string assemblyName, string sourcePath)
		{
			var assemblyId = Guid.NewGuid();
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			using (var cmd = connection.Command(@"insert into [Assembly] (E8_PK, E8_AssemblyName, E8_IsActive, E8_SourcePath) values (@assemblyId, @assemblyName, 1, @sourcePath)"))
			{
				cmd.AddParameter("assemblyId", SqlDbType.UniqueIdentifier, assemblyId);
				cmd.AddParameter("assemblyName", SqlDbType.VarChar, assemblyName);
				cmd.AddParameter("sourcePath", SqlDbType.VarChar, sourcePath);
				cmd.ExecuteNonQuery();
			}

			return assemblyId;
		}

		static Guid AddClass(Guid assemblyId, string className)
		{
			var classId = Guid.NewGuid();
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			using (var cmd = connection.Command(@"insert into [TestClass] (E2_PK, E2_TestClass, E2_E8) values (@classId, @className, @assemblyId)"))
			{
				cmd.AddParameter("classId", SqlDbType.UniqueIdentifier, classId);
				cmd.AddParameter("className", SqlDbType.VarChar, className);
				cmd.AddParameter("assemblyId", SqlDbType.UniqueIdentifier, assemblyId);
				cmd.ExecuteNonQuery();
			}

			return classId;
		}

		static Guid AddMethod(Guid classId, string methodName)
		{
			var methodId = Guid.NewGuid();
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			using (var cmd = connection.Command(@"insert into [TestMethod] (E6_PK, E6_MethodName, E6_E2, E6_DateCreated) values (@methodId, @methodName, @classId, getdate())"))
			{
				cmd.AddParameter("methodId", SqlDbType.UniqueIdentifier, methodId);
				cmd.AddParameter("methodName", SqlDbType.VarChar, methodName);
				cmd.AddParameter("classId", SqlDbType.UniqueIdentifier, classId);
				cmd.ExecuteNonQuery();
			}

			return methodId;
		}

		static Guid AddAmnesty(Guid classId)
		{
			var amnestyId = Guid.NewGuid();
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			using (var cmd = connection.Command(@"insert into [AmnestyFailures] (AF_PK, AF_E6, AF_StartDate) values (@amnestyId, @classId, getdate())"))
			{
				cmd.AddParameter("amnestyId", SqlDbType.UniqueIdentifier, amnestyId);
				cmd.AddParameter("classId", SqlDbType.UniqueIdentifier, classId);
				cmd.ExecuteNonQuery();
			}

			return amnestyId;
		}

		static void AddSourceTreeResponsibility(string path, string product, string productArea, string module)
		{
			string sourceTreeResponsibilityInsert = @"INSERT INTO SourceTreeResponsibility
				(ST_Path, ST_Product, ST_Product_Area, ST_Module)
				VALUES
				(@Path, @Product, @ProductArea, @Module)";
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				using (var command = connection.Command(sourceTreeResponsibilityInsert))
				{
					command.AddParameter("@Path", SqlDbType.VarChar, path);
					command.AddParameter("@Product", SqlDbType.VarChar, 3, product);
					command.AddParameter("@ProductArea", SqlDbType.VarChar, 3, productArea);
					command.AddParameter("@Module", SqlDbType.VarChar, 3, module);
					command.ExecuteNonQuery();
				}
			}
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				connection.ExecuteNonQuery(@"
IF OBJECT_ID('[dbo].[SourceTreeResponsibility]') IS NULL
BEGIN
    CREATE TABLE [dbo].SourceTreeResponsibility(
	    [ST_Path] [varchar](128) PRIMARY KEY WITH (ALLOW_PAGE_LOCKS = OFF),
	    [ST_User_Responsible] [varchar](128) NULL,
	    [ST_Product] [varchar](128) NULL,
	    [ST_Product_Area] [varchar](128) NULL,
	    [ST_Module] [varchar](128) NULL,
    );
END

IF COL_LENGTH('Assembly','E8_SourcePath') IS NULL
BEGIN
		ALTER TABLE Assembly
	ADD [E8_SourcePath] [nvarchar](256) NULL
END


");
				connection.ExecuteNonQuery(@"
IF COL_LENGTH('SourceTreeResponsibility','ST_Amnesty_WI_Grouping') IS NULL
BEGIN
	ALTER TABLE SourceTreeResponsibility
ADD [ST_Amnesty_WI_Grouping] [char](3) NOT NULL DEFAULT 'DLL',
CONSTRAINT IX_ST_Amnesty_WI_Grouping CHECK (ST_Amnesty_WI_Grouping IN ('STN', 'DLL', 'CLS', 'MET'))
END
");
				connection.ExecuteNonQuery(@"


INSERT INTO [dbo].[User] ([U1_PK],[U1_Name])
		 VALUES
('FF94D9D8-C026-472E-BEE3-6854ABA562A5', 'CORP\Gary.Odea'),
('E64591C8-2C15-4E42-8BB4-7477D05E2D01', 'bret.ehlert'),
('F53E554B-FE0F-4572-9878-9D0A405BE8F8', 'CORP\Baaber.Khan'),
('E0E7B041-6803-4632-976A-B00308DB073D', 'CORP\Alexander.Korotun'),
('FE9A1EDF-8030-416A-A657-B03E7E9D93EA', 'CORP\Dalmo.Serravalle'),
('B680CF41-E46B-4882-B33E-F3A8CDDA705C', 'CORP\dmitry.rodionov')

INSERT INTO [dbo].[Assembly] ([E8_PK],[E8_AssemblyName],[E8_IsActive],[E8_SourcePath])
		 VALUES
('8529FB4A-9B31-4BD1-97FF-1424CA9FA031', 'WebClient\CargoWise.Glow.HTML.Client.Test.dll', 1, ''),
('5C41431D-B7C8-4E45-B4E0-2A3E780C0AFF', 'Enterprise.DbUpgrader.Transformations'        , 1, '$/Path/Test'),
('6C611385-ADAD-4505-A2E8-367D72925AAA', 'Enterprise.Customs.SG.V4.GUI'                 , 1, ''),
('304499E1-97B6-46AB-A148-5035E2E46ECD', 'Enterprise.DocumentEngine'                    , 1, ''),
('D0B6D68A-B7C3-458F-9094-57DD1E4C83C8', 'DAT\Testing\Dat.Testing.dll'                  , 1, '$/Cat'),
('8BCB99F9-1D80-412A-BC6C-AF3657DDAD47', 'Enterprise.Accounting.Business'               , 1, ''),
('858A83BF-9939-4E84-AB8F-B503FC54B681', 'CargoWiseOne'                                 , 1, '$/Test'),
('61A6D8F8-E74A-4721-9971-C181AD7AEC15', 'Enterprise.Customs.SG.V4.Business'            , 1, '$/Trial'),
('866B5D5F-CDFF-44A1-B157-CA1D5CFC8E90', 'Enterprise.Accounting.ReportTableProviders'   , 1, ''),
('8E4F79BB-0C8A-4D7D-8136-FD94A1EAF1FE', 'AnalyzersRunner.RunAnalyzers'                 , 1, '$/DevTools')

INSERT INTO [dbo].[SourceTreeResponsibility] ([ST_Path],[ST_User_Responsible],[ST_Product],[ST_Product_Area],[ST_Module],[ST_Amnesty_WI_Grouping])
		 VALUES
('$/Path'		, 'CORP\Bret.Ehlert'   , 'POI', 'LKJ'		, 'YHN'	, 'DLL'),
('$/Cat'		, ''						, 'GLW', ''			, ''	, 'DLL'),
('$/Test'		, 'Bret.Ehlert'				, 'ENT', 'ARC'		, ''	, 'MET'),
('$/Trial'		, 'CORP\Bret.Ehlert'	, '123', '456'		, '789'	, 'STN'),
('$'			, 'Bret.Ehlert'             , 'ENT', 'PER'		, 'APP'	, 'CLS'),
('$/Dev'		, 'Fail'					, 'FAL', 'FAI'		, 'NOO'	, 'STN')


INSERT INTO [dbo].[TestClass] ([E2_PK],[E2_TestClass],[E2_E8])
	VALUES
('11899F5C-AC42-4813-B283-71B53119A7C9', 'Dat.Testing.EndToEndAutoDeployTestedShelfTests'                                                            , 'D0B6D68A-B7C3-458F-9094-57DD1E4C83C8'),
('4E11D0FD-2F72-4CE4-94B6-BFF668C646C7', 'Enterprise.DocumentEngine.DocBuilder.Testing.TemplateCacheTest'                                            , '304499E1-97B6-46AB-A148-5035E2E46ECD'),
('5D1365E8-2A6B-478C-B147-C0613073727C', 'Enterprise.Startup.UpgradeAssuranceTest'                                                                   , '858A83BF-9939-4E84-AB8F-B503FC54B681'),
('815F7A1F-5709-48E9-A8E3-C456B00123DF', 'CargoWise.Glow.HTML.Client.Test.NUnitJSTest'                                                               , '8529FB4A-9B31-4BD1-97FF-1424CA9FA031'),
('71351072-2C3A-42E5-86ED-DC76C8447911', 'Enterprise.Accounting.ReportTableProviders.GLAccountDocumentDataProvider+GLAccountDocumentDataProviderTest', '866B5D5F-CDFF-44A1-B157-CA1D5CFC8E90'),
('26F2AED9-C6DB-434F-A9A3-E0C398B4761F', 'Enterprise.Accounting.Business.CashBook.DirectDebitBatch.DirectDebitBatchHeader+DirectDebitBatchHeaderTest', '8BCB99F9-1D80-412A-BC6C-AF3657DDAD47'),
('75814E91-F8A2-43EF-ADE6-E294388BAB78', 'Enterprise.Customs.SG.V4.Business.Testing.AddInfoJobComInvoiceLineValidation_IPTTest'                      , '61A6D8F8-E74A-4721-9971-C181AD7AEC15'),
('149721F4-8E55-4B67-86B7-E9748E9F92A8', 'AnalyzersRunner.RunAnalyzers.Runner'                                                                       , '8E4F79BB-0C8A-4D7D-8136-FD94A1EAF1FE'),
('176629C0-0D56-4804-BA43-E98677C81E6D', 'Enterprise.Customs.SG.V4.GUI.Testing.SGTariffFormTest'                                                     , '6C611385-ADAD-4505-A2E8-367D72925AAA'),
('DCE77923-AE06-4020-9C64-FCF6ABCD1CC7', 'Enterprise.DbUpgrader.Transformations.Testing.CreateMatchingDocketLineForEachInventoryTest'                , '5C41431D-B7C8-4E45-B4E0-2A3E780C0AFF'),
('9C251E48-2CC5-47D2-89BB-FE11F844088B', 'Enterprise.Customs.SG.V4.Business.Testing.AddInfoJobComInvoiceLineValidationTest'                          , '61A6D8F8-E74A-4721-9971-C181AD7AEC15')

INSERT INTO [dbo].[TestMethod]([E6_PK],[E6_MethodName],[E6_E2],[E6_DateCreated])
		 VALUES
('BB73D5F6-95C8-408B-B6D4-04D0536F0AFF', 'SuccessfulShelfCheckIn'                                                    , '11899F5C-AC42-4813-B283-71B53119A7C9', '2016-03-04 15:18:25.933'),
('CB02F1DD-4A2F-46ED-8B1C-476FFAEBC99C', 'NUnitJS - HTML\Client\Client.Test\JS\Mobile\LegacyUIServiceProviderSpec.js', '815F7A1F-5709-48E9-A8E3-C456B00123DF', '2016-03-17 15:01:26.717'),
('2CDBF5A2-810A-4D04-8936-5B4D017F7882', 'TestRunEnterpriseOnEmptyDatabase'                                          , '5D1365E8-2A6B-478C-B147-C0613073727C', '2014-02-10 21:51:12.380'),
('CA872A2F-1F85-4C53-BC87-68D27FA377C1', 'TestShowHeaderDescription'                                                 , '71351072-2C3A-42E5-86ED-DC76C8447911', '2008-04-29 08:42:13.793'),
('820A3DED-F1D4-4843-8F7A-7CDBD5760E03', 'TestTransform_PartiallyCommittedPartiallyTaken'                            , 'DCE77923-AE06-4020-9C64-FCF6ABCD1CC7', '2014-10-29 08:43:33.180'),
('21CCB763-CD21-44EC-AA1E-87315BF16FA8', 'TestTemplatesWithDifferentSectionFiltersAreCachedSeparately'               , '4E11D0FD-2F72-4CE4-94B6-BFF668C646C7', '2012-03-15 19:03:29.977'),
('3B3FAEA0-07C5-4D72-95F2-9173C55C9456', 'TestTotalDutiableWGTVOLQTYUnit'                                            , '9C251E48-2CC5-47D2-89BB-FE11F844088B', '2007-07-04 18:28:49.583'),
('3848F888-AD2A-4EC2-B2A2-A49679105AFE', 'TestAH_PostDate'                                                           , '26F2AED9-C6DB-434F-A9A3-E0C398B4761F', '2014-11-13 11:33:26.697'),
('C9210B44-2F06-40DD-864A-C8F64EA41C6E', 'TestRemoveSystemCommodity'                                                 , '176629C0-0D56-4804-BA43-E98677C81E6D', '2007-06-09 06:31:40.463'),
('7CB454C7-DDD6-45F2-9045-E8E42C10C22E', 'TestUnitDutiableWGTVOLQTYUnit'                                             , '75814E91-F8A2-43EF-ADE6-E294388BAB78', '2007-07-04 18:28:49.600'),
('496CA45F-76FD-4669-8CF3-FED342EBE114', 'TestPacking_GUI'                                                           , '149721F4-8E55-4B67-86B7-E9748E9F92A8', '2015-06-04 17:56:20.360')

INSERT INTO [dbo].[AmnestyFailures] ([AF_PK],[AF_E6],[AF_StartDate],[AF_ExpiryDate],[AF_IM])
		 VALUES
('84DA21DA-41F4-4FCA-BA44-12797CB7AC8A', '7CB454C7-DDD6-45F2-9045-E8E42C10C22E', '2014-05-29 07:00:24.977', NULL                     , NULL                                  ),
('84DA21DA-41F4-4FCA-BA44-12797CB7AC8B', '7CB454C7-DDD6-45F2-9045-E8E42C10C22E', '2014-05-29 07:00:24.977', '2014-06-07 04:21:04.923', NULL                                  ),
('A0E281B4-31BA-4696-8BC9-1283A6F77A8F', 'BB73D5F6-95C8-408B-B6D4-04D0536F0AFF', '2016-04-24 04:28:01.617', NULL                     , NULL                                  ),
('A0E281B4-31BA-4696-8BC9-1283A6F77A8E', 'BB73D5F6-95C8-408B-B6D4-04D0536F0AFF', '2016-04-24 04:28:01.617', '2016-04-25 09:41:10.880', NULL                                  ),
('C0BE5433-C0DF-41D8-BEF7-12872512EA8D', '21CCB763-CD21-44EC-AA1E-87315BF16FA8', '2013-09-23 01:18:02.913', '2013-11-04 13:32:18.660', 'AE1E3C35-E390-4899-A554-7A18D5A3710D'),
('D0884E31-834C-4552-8DF6-1290C8F13526', '496CA45F-76FD-4669-8CF3-FED342EBE114', '2015-10-04 00:28:00.287', NULL                     , NULL                                  ),
('D0884E31-834C-4552-8DF6-1290C8F13527', '496CA45F-76FD-4669-8CF3-FED342EBE114', '2015-10-04 00:28:00.287', '2015-11-02 10:43:45.483', NULL                                  ),
('8E08520B-C798-4749-B2C9-129477A32AF0', '3848F888-AD2A-4EC2-B2A2-A49679105AFE', '2014-12-01 10:25:12.593', '2015-01-09 00:10:15.473', '53512B5B-50B7-46DC-B453-3D621A6B242E'),
('F81C4FED-BF7D-4E51-9341-129BA901075C', 'C9210B44-2F06-40DD-864A-C8F64EA41C6E', '2015-08-16 02:45:42.760', '2015-09-01 11:48:25.777', '6CCEF788-F242-4080-9A36-8D15564D5E05'),
('ED2B1145-4F8F-4F6D-BEA4-12A1B7D5D004', '2CDBF5A2-810A-4D04-8936-5B4D017F7882', '2015-07-25 03:07:59.680', NULL                     , NULL                                  ),
('ED2B1145-4F8F-4F6D-BEA4-12A1B7D5D005', '2CDBF5A2-810A-4D04-8936-5B4D017F7882', '2015-07-25 03:07:59.680', '2015-08-07 08:09:34.207', NULL                                  ),
('150C1489-6A86-4DFA-994A-12A9A8BCE159', '820A3DED-F1D4-4843-8F7A-7CDBD5760E03', '2015-11-18 20:00:18.613', NULL                     , NULL                                  ),
('150C1489-6A86-4DFA-994A-12A9A8BCE15A', '820A3DED-F1D4-4843-8F7A-7CDBD5760E03', '2015-11-18 20:00:18.613', '2015-12-07 08:26:49.913', NULL                                  ),
('9A6A718C-F570-454B-8F96-12ABF9746516', 'CB02F1DD-4A2F-46ED-8B1C-476FFAEBC99C', '2016-04-30 21:15:05.093', '2016-05-02 10:52:10.037', 'BFF3F620-8421-4BCD-82CD-D734B8419546'),
('0C82C742-749D-4488-AB5E-12B18069DD26', 'CA872A2F-1F85-4C53-BC87-68D27FA377C1', '2015-04-28 00:43:18.937', '2015-05-21 14:56:13.600', 'A49818A6-69F5-4FDE-91FF-A68FDA976B39'),
('972A8499-421B-45E4-A35E-12BDD4162A65', '3B3FAEA0-07C5-4D72-95F2-9173C55C9456', '2014-05-29 07:00:35.740', NULL                     , NULL                                  ),
('972A8499-421B-45E4-A35E-12BDD4162A66', '3B3FAEA0-07C5-4D72-95F2-9173C55C9456', '2014-05-29 07:00:35.740', '2014-06-07 04:21:04.923', NULL    )


");
			}
		}
	}
}
