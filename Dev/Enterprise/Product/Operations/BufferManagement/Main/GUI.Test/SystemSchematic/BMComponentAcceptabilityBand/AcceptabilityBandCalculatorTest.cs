using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Module;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class AcceptabilityBandCalculatorTest : BMSTestCaseWithFactory
	{
		#region AllowCompanyFiltersInTagRules and Filtered by Tag

		[TestDate(2000, 1, 1, 8, 0, 0)]
		public void TestCalculate_WhenAllowCompanyFiltersInTagRulesRegistryIsTrueOrFalse_ShouldAllowCompanyFilters()
		{
			VisualBoardsTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.DummyWorkflowDescriptorCode, "COM", "BUN");

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Workflow N");
			var task = VisualBoardsTestHelper.CreateTask(workflow, description: "Task N", taskType: "BUN", taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 2, 3, 5, 7, 11, 13, "Prime Band", type: AcceptabilityBandTypes.Codes.Count);

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleGuidModuleSpecifiedFilter)filterBizo["Parent Job"];

			IEnumerable<string> AssertCalculateStatusForSingleResultAndGetSQLCommands(string moduleName, BMComponentAcceptabilityBand b, bool allowCompanyFilter)
			{
				//We're about to modify the filters of an FSBO mid-test, so clear cache.
				ModuleFilter.ClearSelectedFiltersCache();

				BMSRegistry.Instance.AllowCompanyFiltersInTagRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowCompanyFilter);
				using (TestConnection.TrackExecutedCommands())
				{
					var result = CalculateStatusForSingleResult(b, new AcceptabilityBandSqlBuilderParameters(b));
					var message = $"Specified-module = '{moduleName}': ";
					AssertEquals($"{message}: Filter should have found results", true, result.ResultFound);
					Assert($"{message}: Should have result", result.Value > 0); // Organisation value = 2

					return TestConnection.ExecutedCommands.Where(sqlCommand => sqlCommand.Contains("Workflow N"));
				}
			}

			foreach (var moduleName in filter.ModuleOptions.GetAllCodes())
			{
				TagRuleRunnerCompanyFilterTest.SetFilterStrips(band.FilterRule, moduleName);
				Factory.Save();

				var notAllowCompanyFiltersSQLCommands = AssertCalculateStatusForSingleResultAndGetSQLCommands(moduleName, band, allowCompanyFilter: false);
				var allowCompanyFiltersSQLCommands = AssertCalculateStatusForSingleResultAndGetSQLCommands(moduleName, band, allowCompanyFilter: true);

				AssertContainsExactElementsInAnyOrder(
					$"Regardless of AllowCompanyFiltersInTagRulesRegistry value, company filter should be allowed",
					notAllowCompanyFiltersSQLCommands,
					allowCompanyFiltersSQLCommands);
			}
		}

		#endregion

		#region Specific Filter Strips
		public void TestCalculateStatus_DoesNotTriggerSaveLastUsedLayout_WhenParentJobFiltersMatchWorkItemAcceptabilityBand()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var testBand = VisualBoardsTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 1, 2, 3, 4, 5, "Haruhi Band", type: AcceptabilityBandTypes.Codes.Count);

			FilterStripsTestHelper.AddFilterStrips(testBand.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Parent Job",
				FilterStripValueSetter = f =>
				{
					var filter = (ModuleGuidModuleSpecifiedFilter)f;
					filter.SelectedModule = ModuleIDs.WorkItem.Name;
				},
				ComparisonOperatorSetter = f => ((ModuleGuidFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch
			});

			var parameters = new AcceptabilityBandSqlBuilderParameters(testBand);

			Factory.Save();

			var preCalculateStatusSaveCount = BusinessObjectFactory.GlobalSaveCount;
			var bandResult = CalculateStatusForSingleResult(testBand, parameters);
			AssertNotNull(bandResult);
			AssertEquals("Should not save last used layout when calculating acceptability bands", preCalculateStatusSaveCount, BusinessObjectFactory.GlobalSaveCount);
		}

		[TestDate(2019, 1, 1)]
		public void TestBandCalculation_WithBufferZoneFilterStrip_ShouldNotThrowException()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow_zone3 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow_zone3", config.Buffer, ZDateTime.UtcNow);
			var workflow_zone2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow_zone2", config.Buffer, ZDateTime.UtcNow.AddDays(-2));
			var workflow_zone1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow_zone1", config.Buffer, ZDateTime.UtcNow.AddDays(-5));
			var workflow_zone0 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow_zone0", config.Buffer, ZDateTime.UtcNow.AddDays(-7));

			VisualBoardsTestHelper.CreateTask(workflow_zone3);
			VisualBoardsTestHelper.CreateTask(workflow_zone2);
			VisualBoardsTestHelper.CreateTask(workflow_zone1);
			VisualBoardsTestHelper.CreateTask(workflow_zone0);

			CombineAssertions("Workflow buffer zones", () =>
			{
				AssertEquals("workflow_zone3", 3, workflow_zone3.BufferZone);
				AssertEquals("workflow_zone2", 2, workflow_zone2.BufferZone);
				AssertEquals("workflow_zone1", 1, workflow_zone1.BufferZone);
				AssertEquals("workflow_zone0", 0, workflow_zone0.BufferZone);
			});

			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 2, 4, 6, "Zone 0s", type: AcceptabilityBandTypes.Codes.Count);
			FilterStripsTestHelper.AddFilterStrips(band.FilterRule, new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.BufferZone });

			Factory.Save();

			AssertAcceptabilityBandResult(band, ComponentAcceptabilityStatus.Excellent, AcceptabilityStatusPolarity.Middle, 1m);
		}

		public void TestBandCalculation_WithTaskStatusFilterStrip_AndMultipleTypeSet_RetreivesValueWithoutError()
		{
			VisualBoardsTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.DummyWorkflowDescriptorCode, "COM", "BUN");

			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 1");
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 2");
			var workflow3 = VisualBoardsTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 3");

			var task1 = VisualBoardsTestHelper.CreateTask(workflow1, description: "Task1", taskType: "BUN", taskStatus: ProcessTaskStatusCodeList.Codes.Open);
			var task2 = VisualBoardsTestHelper.CreateTask(workflow2, description: "Task2", taskType: "BUN", taskStatus: ProcessTaskStatusCodeList.Codes.Open);
			var task3 = VisualBoardsTestHelper.CreateTask(workflow2, description: "Task3", taskType: "COM", taskStatus: ProcessTaskStatusCodeList.Codes.Open);
			var task4 = VisualBoardsTestHelper.CreateTask(workflow3, description: "Task4", taskType: "UDF", taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 2, 3, 5, 7, 11, 13, "Prime Band", type: AcceptabilityBandTypes.Codes.Count);

			FilterStripsTestHelper.AddFilterStrips(band.FilterRule,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Aggregated Task Status",
					FilterStripValueSetter = (f) =>
					{
						var filter = (ITaskStatusFilter)f;
						filter.TaskAggregator = "ALL";
						filter.TaskStatusCheckList.First(l => l.Description == ProcessTaskStatusCodeList.Codes.Open).Value = true;
						filter.TaskTypeCheckList.First(l => l.Description.StartsWith("BUN")).Value = true;
						filter.TaskTypeCheckList.First(l => l.Description.StartsWith("COM")).Value = true;
					}
				},
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.JobOrWorkflow,
					FilterStripValueSetter = (f) =>
					{
						var filter = (IJobOrWorkflowFilter)f;
						filter.SetWorkflowOnly();
					}
				}
			);

			Factory.Save();

			CombineAssertions(() =>
			{
				AcceptabilityBandResult result = null;
				AssertNoExceptionThrown(() => result = CalculateStatusForSingleResult(band, new AcceptabilityBandSqlBuilderParameters(band)));
				AssertEquals("Filter should have found results", true, result.ResultFound);
				AssertEquals("Should have 2 results", 2m, result.Value);
			});
		}

		public void TestBandCalculation_WithTaskStatusFilterStrip_AndSingleTypeSet_RetreivesValueWithoutError()
		{
			VisualBoardsTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.DummyWorkflowDescriptorCode, "COM", "BUN");

			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 1");
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 2");
			var workflow3 = VisualBoardsTestHelper.CreateWorkflow(jobHeader1, "Test Workflow 3");

			var task1 = VisualBoardsTestHelper.CreateTask(workflow1, description: "Task1", taskType: "BUN", taskStatus: ProcessTaskStatusCodeList.Codes.Open);
			var task2 = VisualBoardsTestHelper.CreateTask(workflow2, description: "Task2", taskType: "BUN", taskStatus: ProcessTaskStatusCodeList.Codes.Open);
			var task3 = VisualBoardsTestHelper.CreateTask(workflow2, description: "Task3", taskType: "COM", taskStatus: ProcessTaskStatusCodeList.Codes.Open);
			var task4 = VisualBoardsTestHelper.CreateTask(workflow3, description: "Task4", taskType: "UDF", taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 1, 1, 2, 3, 5, 8, "Spiral Band", type: AcceptabilityBandTypes.Codes.Count);

			FilterStripsTestHelper.AddFilterStrips(band.FilterRule,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Aggregated Task Status",
					FilterStripValueSetter = (f) =>
					{
						var filter = (ITaskStatusFilter)f;
						filter.TaskAggregator = "ALL";
						filter.TaskStatusCheckList.First(l => l.Description == ProcessTaskStatusCodeList.Codes.Open).Value = true;
						filter.TaskTypeCheckList.First(l => l.Description.StartsWith("BUN")).Value = true;
					}
				},
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.JobOrWorkflow,
					FilterStripValueSetter = (f) =>
					{
						var filter = (IJobOrWorkflowFilter)f;
						filter.SetWorkflowOnly();
					}
				}
			);

			Factory.Save();

			CombineAssertions(() =>
			{
				AcceptabilityBandResult result = null;
				AssertNoExceptionThrown(() => result = CalculateStatusForSingleResult(band, new AcceptabilityBandSqlBuilderParameters(band)));
				AssertEquals("Filter should have found results", true, result.ResultFound);
				AssertEquals("Should have 1 result", 1m, result.Value);
			});
		}

		public void TestFilterAsAcceptabilityBand_ShouldProperlyCountApplicableWorkflows()
		{
			var capability = BMSTestHelper.CreateCapability(Factory, "TSM", "The Butte");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "MRT", "Mister Tea", capability);
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var section = config.BufferSection;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, true);
			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "Boop", type: AcceptabilityBandTypes.Codes.Count);

			var filter = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = OpenTaskEstimateRangeFilter.Schema.Identifier,
				FilterStripValueSetter = f =>
				{
					((OpenTaskEstimateRangeFilter)f).MinStdEstimate = new ZDateTime(2015, 1, 1, 0, 0, 0);
					((OpenTaskEstimateRangeFilter)f).MaxStdEstimate = new ZDateTime(2015, 1, 1, 2, 0, 0);
				},
			};

			FilterStripsTestHelper.AddFilterStrips(band.FilterRule, filter);

			var param = new AcceptabilityBandSqlBuilderParameters(band);
			var provider = new AcceptabilityBandDataProvider(SecondaryServerConnectionProviderProvider.GetProvider());

			Factory.Save();

			AssertNoExceptionThrown("We want to make sure that the filter works in an acceptability band (throws no dev exception), AND returns a proper result.", () => AssertAcceptabilityBandResult(band, ComponentAcceptabilityStatus.Excellent, AcceptabilityStatusPolarity.Middle, expectedValue: 0));
		}

		#endregion

		#region Exception Handling

		public void TestLockTimeoutExpiredException_ShouldReturnNullAndNotSendEmail()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 1, 2, 3, 4, 5, "Haruhi Band", type: AcceptabilityBandTypes.Codes.Count);
			var parameters = new AcceptabilityBandSqlBuilderParameters(band);
			var provider = new AcceptabilityBandDataProvider(SecondaryServerConnectionProviderProvider.GetProvider());

			provider.BeforeScalarResultCommandExecuted_ForTest += (_, __) =>
			{
				throw SqlExceptionBuilder.CreateSqlException(1222, 51, 13, Db.ServerName, "If you're seeing this message in a test failure it means we're not catching this SQL exception properly.", string.Empty, 1);
			};

			var result = CalculateStatusForSingleResult(band, parameters, provider);

			AssertNull(result.Value);
			AssertEquals(false, result.ResultFound);
		}

		public void TestExecutionTimeoutExpiredException_ShouldReturnNullAndNotSendEmail()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 1, 2, 3, 4, 5, "Antonio Band Eras", type: AcceptabilityBandTypes.Codes.Count);
			var parameters = new AcceptabilityBandSqlBuilderParameters(band);
			var provider = new AcceptabilityBandDataProvider(SecondaryServerConnectionProviderProvider.GetProvider());
			provider.BeforeScalarResultCommandExecuted_ForTest += (_, __) =>
			{
				throw SqlExceptionBuilder.CreateSqlException(3617, 51, 13, Db.ServerName, "If you're seeing this message in a test failure it means we're not catching this SQL exception properly.", string.Empty, 1);
			};

			var result = CalculateStatusForSingleResult(band, parameters, provider);

			AssertNull(result.Value);
			AssertEquals(false, result.ResultFound);
		}

		public void TestBandWithFilterWithCountrySpecificModule_WhenCountryNotAvailableInCurrentContext_ShouldReturnNullAndNotSendEmailOrReportError()
		{
			BMSTestHelper.SetupEmailAndGroup(Factory);

			var company = Factory.New<IGlbCompany>();
			((BusinessObject)company).FillWithValidTestData();
			company.GC_RN_NKCountryCode = "DE";

			var branch = Factory.New<IGlbBranch>();
			((BusinessObject)branch).FillWithValidTestData();
			branch.GB_GC = company.PK;

			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var band = VisualBoardsTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 1, 2, 3, 4, 5, "Antonio Band Eras", type: AcceptabilityBandTypes.Codes.Count);
			FilterStripsTestHelper.AddFilterStrip<ModuleGuidModuleSpecifiedFilter>(band.FilterRule, "Parent Job",
				(filter) => filter.SelectedModule = ModuleIDs.Customs.AU.AirCargoOutturnBills.Name,
				(filter) => filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var calculator = new AcceptabilityBandCalculator();
				var provider = new AcceptabilityBandDataProvider(SecondaryServerConnectionProviderProvider.GetProvider());
				var parameters = new AcceptabilityBandSqlBuilderParameters(band);
				AcceptabilityBandResult result = null;

				AssertNoExceptionThrown("", () => result = CalculateStatusForSingleResult(band, parameters, provider));

				AssertEquals("No results should have been returned because the filters were invalid. SAD!", false, result.ResultFound);
				AssertContainsExactElementsInAnyOrder("We should not email anyone about this... the user in the other country can notify their BMS admins if it's a problem. SAD!", Array.Empty<string>(), Env.OutgoingMailManager.EmailsCreated.Select(x => x.Subject));
				AssertNull("Should not report error", ErrorReporter.LastExceptionReported);
			}
		}

		#endregion

		#region Helpers

		static AcceptabilityBandResult CalculateStatusForSingleResult(
			BMComponentAcceptabilityBand band,
			AcceptabilityBandSqlBuilderParameters parameters,
			AcceptabilityBandDataProvider provider = null)
		{
			if (provider == null)
			{
				provider = new AcceptabilityBandDataProvider(SecondaryServerConnectionProviderProvider.GetProvider());
			}

			return new AcceptabilityBandCalculator().CalculateStatus(band, provider, parameters).Single();
		}

		void AssertAcceptabilityBandResult(
			BMComponentAcceptabilityBand band,
			ComponentAcceptabilityStatus status,
			AcceptabilityStatusPolarity polarity, decimal? expectedValue = null)
		{
			var result = CalculateStatusForSingleResult(band, new AcceptabilityBandSqlBuilderParameters(band));

			CombineAssertions(() =>
			{
				AssertEquals("Status", status, result.Status);
				AssertEquals("StatusPolarity", polarity, result.StatusPolarity);

				if (expectedValue != null)
				{
					AssertEquals("Value", expectedValue.Value, result.Value);
				}
			});
		}

		#endregion
	}
}
