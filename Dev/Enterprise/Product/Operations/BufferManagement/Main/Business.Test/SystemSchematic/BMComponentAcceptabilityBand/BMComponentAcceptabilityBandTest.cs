using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Utilities.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMComponentAcceptabilityBand))]
	class BMComponentAcceptabilityBandTest : EnterpriseBusinessObjectTestCase
	{
		#region Boundary Values

		public void TestBoundaryValues()
		{
			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band.BAB_CautionLowerBound = 1;
			band.BAB_GoodLowerBound = 2;
			band.BAB_ExcellentLowerBound = 3;
			band.BAB_ExcellentUpperBound = 4;
			band.BAB_GoodUpperBound = 5;
			band.BAB_CautionUpperBound = 6;

			var expected = new AcceptabilityBandBoundaryValues(1, 2, 3, 4, 5, 6);
			AssertEquals(expected, band.BoundaryValues);
		}

		public void TestBoundaryValuesHintLabel()
		{
			var band = Factory.New<BMComponentAcceptabilityBand>();
			AssertEquals("Specify the values used to calculate the status for this Acceptability Band.", band.BoundaryValuesHintLabel);

			band.BAB_Type = AcceptabilityBandTypes.Codes.Count;
			AssertEquals("Specify the values used to calculate the status for this Acceptability Band. These values represent the number of items returned by the filter strips.", band.BoundaryValuesHintLabel);

			band.BAB_Type = AcceptabilityBandTypes.Codes.Aggregate;
			AssertEquals("Specify the values used to calculate the status for this Acceptability Band. These values represent the value returned by the SQL statement.", band.BoundaryValuesHintLabel);

			band.BAB_Type = AcceptabilityBandTypes.Codes.SQL;
			AssertEquals("Specify the values used to calculate the status for this Acceptability Band. These values represent the value returned by the SQL statement.", band.BoundaryValuesHintLabel);

			band.BAB_Type = AcceptabilityBandTypes.Codes.TotalPlannedDuration;
			AssertEquals("Specify the values used to calculate the status for this Acceptability Band. These values represent the total number of hours in duration.", band.BoundaryValuesHintLabel);

			band.BAB_Type = AcceptabilityBandTypes.Codes.NumberAsPercentage;
			AssertEquals("Specify the values used to calculate the status for this Acceptability Band. These values represent percentages in whole numbers.", band.BoundaryValuesHintLabel);

			band.BAB_Type = AcceptabilityBandTypes.Codes.PlannedDurationPercentage;
			AssertEquals("Specify the values used to calculate the status for this Acceptability Band. These values represent percentages in whole numbers.", band.BoundaryValuesHintLabel);
		}

		#endregion

		#region Delete

		public void TestDelete_NoExceptionWhenNoBandForBoardSection()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var board = BMSTestHelper.CreateBoard(system, name: "Test Board");
			var section1 = BMSTestHelper.CreateBoardSection(bucket, board);
			var section2 = BMSTestHelper.CreateBoardSection(bucket, board);

			var band1 = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(bucket, 10, 12, 14, 16, 18, 22);
			var band2 = BMSTestHelper.CreateAcceptabilityBand_AverageNumberOfTasksPerWorkflow(bucket, 2, 3, 4, 5, 6, 7);

			section1.SectionConfiguration.AcceptabilityBands.AddNew().AcceptabilityBandPK = band1.PK;
			section1.SectionConfiguration.AcceptabilityBands.AddNew().AcceptabilityBandPK = band2.PK;

			Factory.Save();
			AssertEquals("Precondition: two Acceptability Bands for 'Test Board' section 1.", 2, section1.SectionConfiguration.AcceptabilityBands.Count);
			AssertEquals("Precondition: no Acceptability Bands for 'Test Board' section 2.", 0, section2.SectionConfiguration.AcceptabilityBands.Count);

			AssertNoExceptionThrown(() => band1.Delete());
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedSection1 = newFactory.Load<BMBoardSection>(section1.PK);
			var reloadedSection2 = newFactory.Load<BMBoardSection>(section2.PK);

			AssertEquals("Acceptability Band 'band1' should be removed from the 'Test Board' section 1.", 0, reloadedSection1.SectionConfiguration.AcceptabilityBands.Cast<BoardSectionAcceptabilityBand>().Count(b => b.AcceptabilityBandPK == band1.PK));
			AssertEquals("It should not remove other AcceptabilityBands from the 'Test Board' section 1.", 1, reloadedSection1.SectionConfiguration.AcceptabilityBands.Count);

			AssertEquals("No changes for acceptability bands in 'Test Board' section 2.", 0, reloadedSection2.SectionConfiguration.AcceptabilityBands.Count);
		}

		#endregion

		#region Default Values

		public void TestNew_DefaultPropertyValues()
		{
			var band = Factory.New<BMComponentAcceptabilityBand>();
			AssertEquals(false, band.BAB_FiltersByReleaseGroup);
			AssertEquals(true, band.BAB_FiltersBySection);
		}

		#endregion

		#region Filters

		public void TestFilters()
		{
			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			var filter = band.FilterRule;
			var supersetFilter = band.SupersetItemsFilterRule;

			Factory.Save();

			var loadedBand = Factory.CreateNewFactory().Load<BMComponentAcceptabilityBand>(band.PK);
			var loadedFilter = loadedBand.FilterRule;
			var loadedSupersetFilter = loadedBand.SupersetItemsFilterRule;

			AssertEquals(filter.PK, loadedFilter.PK);
			AssertEquals(supersetFilter.PK, loadedSupersetFilter.PK);
		}

		public void TestFilterNames()
		{
			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			var filter = band.FilterRule;
			var supersetFilter = band.SupersetItemsFilterRule;

			band.BAB_Name = "RupleDuple";
			AssertEquals("VAL", filter.S9_FilterName);
			AssertEquals("SUP", supersetFilter.S9_FilterName);
		}

		public void TestUsesFilterStrips()
		{
			foreach (ICodeDescription type in new AcceptabilityBandTypes())
			{
				var band = Factory.New<BMComponentAcceptabilityBand>();
				band.BAB_Type = type.Code;

				var parameters = new AcceptabilityBandSqlBuilderParameters(band);
				var strategy = band.GetSqlStrategy(parameters);

				if (band.UsesFilterStrips)
				{
					AssertNoExceptionThrown("Band types that use filter strips should use strategies that inherit from FilterStripAccessabilityBandSqlStrategy or should override GetMatchingWorkflowSqlCore, and yet...", () => strategy.GetMatchingWorkflowsSql());
				}
				else
				{
					AssertExceptionThrown<InvalidOperationException>("Band types that do not use filter strips should not use strategies that inherit from FilterStripAccessabilityBandSqlStrategy and should throw an exception when GetMatchingWorkflowSql is attempted, and yet...", () => strategy.GetMatchingWorkflowsSql());
				}
			}
		}

		public void TestFilterForNewBand_DbHits()
		{
			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();

			AssertNotNull(band.FilterRule);
			AssertNotNull(band.SupersetItemsFilterRule);

			AssertDbHits(new Dictionary<string, int>(), Factory);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedBand = newFactory.Load<BMComponentAcceptabilityBand>(band.PK);

			AssertNotNull(loadedBand.FilterRule);
			AssertNotNull(loadedBand.SupersetItemsFilterRule);

			AssertDbHits(new Dictionary<string, int>
			{
				{ BMComponentAcceptabilityBandSchema.Constants.TableName, 1 },
				{ StmModuleFilterSchema.Constants.TableName, 2 },
			}, newFactory);
		}

		#endregion

		#region SQL Description

		public void TestSqlDescription()
		{
			var band = Factory.New<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.Aggregate;

			AssertContains("Value Calculation Filters", band.SQLDescription);
			AssertContains("Workflows", band.SQLDescription);
			AssertContains("SectionWorkflows", band.SQLDescription);

			band.BAB_Type = AcceptabilityBandTypes.Codes.SQL;

			AssertNotContains("Value Calculation Filters", band.SQLDescription);
			AssertNotContains("Workflows", band.SQLDescription);
			AssertNotContains("SectionWorkflows", band.SQLDescription);
		}

		#endregion

		#region GetAcceptabilityBandCommand

		public void TestGetAcceptabilityBandSql_ForSqlBandType_ShouldNotIncludeWorkflowsOrSectionWorkflowsInQuery()
		{
			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "SAD", "SELECT 1 as VALUE, NULL as Component, NULL as ReleaseGroup FROM dbo.ProcessHeader");
			var parameters = new AcceptabilityBandSqlBuilderParameters(band);
			var sql = band.GetAcceptabilityBandSqlCommand(parameters, TestConnection).CommandText;

			AssertNotContains("Workflows", sql, ignoreCase: true);
			AssertNotContains("SectionWorkflows", sql, ignoreCase: true);
		}

		public void TestGetAcceptabilityBandCommand_LocalExecutionTimeoutSet()
		{
			using (BMSRegistry.Instance.AcceptabilityBandCalculationServiceExecutionTimeout.SetTemporaryValue(default, default, default, 123))
			using (BMSRegistry.Instance.AcceptabilityBandLocalCalculationExecutionTimeout.SetTemporaryValue(default, default, default, 456))
			{
				var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "Joey Joe Joe!", type: AcceptabilityBandTypes.Codes.Count);
				var parameters = new AcceptabilityBandSqlBuilderParameters(band);
				var command = band.GetAcceptabilityBandSqlCommand(parameters, TestConnection);
				AssertEquals("Command timeout should have been set to mirror the Registry value", 456, command.CommandTimeout);
			}
		}

		public void TestGetAcceptabilityBandCommand_WebExecutionTimeoutSet()
		{
			using (new TemporaryWebEnvironment())
			{
				try
				{
					Db.Connection.BeginTransaction();

					using (BMSRegistry.Instance.AcceptabilityBandCalculationServiceExecutionTimeout.SetTemporaryValue(default, default, default, 123))
					using (BMSRegistry.Instance.AcceptabilityBandLocalCalculationExecutionTimeout.SetTemporaryValue(default, default, default, 456))
					{
						var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "Joey Joe Joe!", type: AcceptabilityBandTypes.Codes.Count);
						var parameters = new AcceptabilityBandSqlBuilderParameters(band);
						var command = band.GetAcceptabilityBandSqlCommand(parameters, TestConnection);
						AssertEquals("Command timeout should have been set to mirror the Registry value", 123, command.CommandTimeout);
					}
				}
				finally
				{
					Db.Connection.RollbackTransaction();
				}
			}
		}

		public void TestGetAcceptabilityBandCommand_ShouldContainAcceptabilityBandComment()
		{
			//This test is written to ensure 'Acceptability Band' comment is present in our sql query formation. This will correctly classify it as Acceptability Band Query Type for CPU Usage info.
			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "Joey Joe Joe!", type: AcceptabilityBandTypes.Codes.Count);
			var parameters = new AcceptabilityBandSqlBuilderParameters(band);
			var command = band.GetAcceptabilityBandSqlCommand(parameters, TestConnection);

			AssertContains("-- Acceptability Band", command.CommandText);
		}

		sealed class TemporaryWebEnvironment : IDisposable
		{
			public TemporaryWebEnvironment()
			{
				var (userPK, branchPK, deptPK) = (Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK);

				temporaryProvider = new WebServicesEnvironmentProviderForTest();
				originalProvider = Env.GetCurrentProvider();
				temporaryProvider.Enable();

				databaseCleanup = Db.DisposableActionForDbConnection();

				userContext = Env.SetTemporaryUserContext(userPK, branchPK, deptPK);
			}

			readonly EnvProvider temporaryProvider;
			readonly EnvProvider originalProvider;
			readonly IDisposable userContext;
			readonly IDisposable databaseCleanup;

			public void Dispose()
			{
				userContext.Dispose();
				databaseCleanup.Dispose();
				temporaryProvider.Dispose();
				originalProvider.Enable();
			}
		}

		#endregion

		#region GetMatchingWorkflowSql

		public void TestGetMatchingWorkflowSql_ForPlannedDurationPercentageAcceptability_WhenShouldFilterBySection_ShouldIncludeSectionWorkflowInQuery()
		{
			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "NEW", string.Empty, AcceptabilityBandTypes.Codes.PlannedDurationPercentage);
			var parameters = new AcceptabilityBandSqlBuilderParameters(band)
			{
				ShouldFilterBySection = true,
				WorkflowPKs = new HashSet<ZGuid>()
			};

			var strategy = band.GetSqlStrategy(parameters);
			var cmdTuple = strategy.GetMatchingWorkflowsSql();
			AssertContains("SectionWorkflows", cmdTuple.Item1);
		}

		public void TestGetMatchingWorkflowSql_ForNUPAcceptability_WithSuperSetFilter_ShouldNotThrowException()
		{
			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "NEW", string.Empty, AcceptabilityBandTypes.Codes.NumberAsPercentage);
			var parameters = new AcceptabilityBandSqlBuilderParameters(band)
			{
				ShouldFilterBySection = true,
				WorkflowPKs = new HashSet<ZGuid>()
			};

			var strategy = band.GetSqlStrategy(parameters);
			var cmdTuple = strategy.GetMatchingWorkflowsSql();

			var command = new ZSqlConnectionInfo(TestConnection, null).GetNewDbCommandForSelect(cmdTuple.Item1, cmdTuple.Item2);
			using (command)
			{
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}
		}

		public void TestIsAdditionalAggregatorColumnPresent_DoesNotCallOverloadOfItselfWhenConstructingDbCommand()
		{
			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.Aggregate;

			var commandText = band.Validation.GetCommandForColumnValidation().CommandText;

			CombineAssertions(() =>
			{
				AssertNotContains(@"	SELECT
	@Sum = SUM(CONVERT(decimal(10, 2), Value)),
	@RowCount = COUNT(*)
	FROM Data WHERE 1 = 1", commandText);
				AssertEquals(0, new Regex(@"\s*SELECT TOP 1?[0-9]*
										\s*CONVERT\(decimal\(10, 2\), Value\) Value,
										\s*AdditionalAggregator").Matches(commandText).Count);
				AssertEquals(0, new Regex(@"\s*SELECT 
											\s*convert\(decimal\(10,2\), Value\) Value,
											\s*CAST\(Component AS uniqueidentifier\) Component,
											\s*CAST\(ReleaseGroup AS uniqueidentifier\) ReleaseGroup
											\s*INTO \w+
											\s*FROM Data
											\s*WHERE 1 = 1").Matches(commandText).Count);
				AssertNotContains("IF @RowCount > 0 SELECT @Sum AS Value", commandText);
				AssertNotContains("ELSE 0 AS Value", commandText);
			});
		}

		#endregion

		#region Logs

		public void TestNoStmALogs()
		{
			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, band.PK);
			AssertEquals("Should not create Add event", 0, Factory.Load<StmALog>(query).Length);

			band.BAB_Name = "New name";
			Factory.Save();
			AssertEquals("Should not create Edit event", 0, Factory.Load<StmALog>(query).Length);

			band.Delete();
			Factory.Save();
			AssertEquals("Should not create Delete event", 0, Factory.Load<StmALog>(query).Length);
		}

		#endregion
	}
}
