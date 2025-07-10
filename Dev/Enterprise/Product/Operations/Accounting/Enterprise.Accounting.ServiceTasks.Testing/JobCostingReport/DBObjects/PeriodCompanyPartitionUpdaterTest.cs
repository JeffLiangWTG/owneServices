using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	class PeriodCompanyPartitionUpdaterTest : TestCaseWithFactory
	{
		[TestDate(2019, 02, 19)]
		public void TestNewPartitionCanBeCreatedBySplittingNonEmptyPartition_GCPKSequenceDescending()
		{
			CreatePartitionBySplitting(orderByDescending: true, makeConstraintsTrusted: false);
			Assert(true);
		}

		[TestDate(2019, 02, 19)]
		public void TestNewPartitionCanBeCreatedBySplittingNonEmptyPartition_GCPKSequenceAscending()
		{
			CreatePartitionBySplitting(orderByDescending: false, makeConstraintsTrusted: false);
			Assert(true);
		}

		[TestDate(2019, 02, 19)]
		public void TestRecordsAreNotProcessedIfPeriodIsMissing()
		{
			#region Setup

			var newCompany1 = ObjectCreator.CreateNewCompany("CO1", orgProxy: ObjectCreator.TestOrganisation);
			var newBranch1 = ObjectCreator.CreateNewBranch(newCompany1, "BR1");
			var sortedCompanyBranchPairPKs = new[] { Tuple.Create(newCompany1.PK, newBranch1.PK) }.OrderBy(x => x.Item1.ToString()).ToArray();
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			#endregion

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask();

				#region Run JCD Service Task for the first time

				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 2, Array.Empty<string>());
				((IDbConnected)Factory).Connection.ExecuteNonQuery("ALTER TABLE RptDtJobCostingData WITH CHECK CHECK CONSTRAINT ALL");

				#endregion

				#region Create Period and Post a Transaction for one Company

				CreatePeriods(ZDateTime.Today.Year, sortedCompanyBranchPairPKs[0].Item1);
				var expectedNonEmptyPartitonInfo = new List<string>();
				for (int i = 1; i <= 12; i++)
				{
					var offset = i - ZDateTime.Today.Month;
					PostATransactionForCompany(sortedCompanyBranchPairPKs[0].Item2, "S0001_" + i.ToString(), ZDateTime.Today.AddMonths(offset));

					if (offset == 0)
					{
						expectedNonEmptyPartitonInfo.Add(ZDateTime.Today.Year.ToString() + ZDateTime.Today.Month.ToString("00") + sortedCompanyBranchPairPKs[0].Item1.ToString() + "->13");
					}
					else
					{
						expectedNonEmptyPartitonInfo.Add(ZDateTime.Today.Year.ToString() + ZDateTime.Today.AddMonths(offset).Month.ToString("00") + sortedCompanyBranchPairPKs[0].Item1.ToString() + "->1");
					}
				}

				#endregion

				var queuedRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCostingDataQueue WHERE JCQ_HasNoPeriod = 0");
				AssertEquals("Initially all queued record will have JCQ_HasNoPeriod = 0", 24, queuedRecordCount);
				TestConnection.ExecuteNonQuery("DELETE FROM dbo.AccPeriodManagement");

				#region Run JCD Service task which populates RptDtJobCostingData Table 

				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 2, Array.Empty<string>());
				queuedRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCOstingDataQueue WHERE JCQ_HasNoPeriod = 1");
				AssertEquals("JCD will set JCQ_HasNoPeriod = 1 as period is missing", 24, queuedRecordCount);

				var processedRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM RptDtJobCostingData");
				AssertEquals("RptDtJobCostingData is Empty", 0, processedRecordCount);

				#endregion

			}
		}

		[TestDate(2019, 02, 19)]
		public void TestJCQHasNoPeriodColumnIsUpdatedIfMissingPeriodIsCreated()
		{
			#region Setup

			var newCompany1 = ObjectCreator.CreateNewCompany("CO1", orgProxy: ObjectCreator.TestOrganisation);
			var newBranch1 = ObjectCreator.CreateNewBranch(newCompany1, "BR1");
			var sortedCompanyBranchPairPKs = new[] { Tuple.Create(newCompany1.PK, newBranch1.PK) }.OrderBy(x => x.Item1.ToString()).ToArray();
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			#endregion

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask();

				#region Run JCD Service Task for the first time

				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 2, Array.Empty<string>());
				((IDbConnected)Factory).Connection.ExecuteNonQuery("ALTER TABLE RptDtJobCostingData WITH CHECK CHECK CONSTRAINT ALL");

				#endregion

				#region Create Period and Post a Transaction for one Company

				CreatePeriods(ZDateTime.Today.Year, sortedCompanyBranchPairPKs[0].Item1);
				var expectedNonEmptyPartitonInfo = new List<string>();
				for (int i = 1; i <= 12; i++)
				{
					var offset = i - ZDateTime.Today.Month;
					PostATransactionForCompany(sortedCompanyBranchPairPKs[0].Item2, "S0001_" + i.ToString(), ZDateTime.Today.AddMonths(offset));

					if (offset == 0)
					{
						expectedNonEmptyPartitonInfo.Add(ZDateTime.Today.Year.ToString() + ZDateTime.Today.Month.ToString("00") + sortedCompanyBranchPairPKs[0].Item1.ToString() + "->13");
					}
					else
					{
						expectedNonEmptyPartitonInfo.Add(ZDateTime.Today.Year.ToString() + ZDateTime.Today.AddMonths(offset).Month.ToString("00") + sortedCompanyBranchPairPKs[0].Item1.ToString() + "->1");
					}
				}

				#endregion

				TestConnection.ExecuteNonQuery("UPDATE dbo.JobCostingDataQueue SET JCQ_HasNoPeriod = 1");
				var queuedRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCostingDataQueue WHERE JCQ_HasNoPeriod = 1");
				AssertEquals("Initially all queued record will have JCQ_HasNoPeriod = 1", 24, queuedRecordCount);

				#region Run JCD Service task which populates RptDtJobCostingData Table

				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 14, expectedNonEmptyPartitonInfo.ToArray());
				queuedRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCostingDataQueue");
				AssertEquals("queue table should be empty", 0, queuedRecordCount);

				var processedRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM RptDtJobCostingData");
				AssertEquals("RptDtJobCostingData is Non Empty", 24, processedRecordCount);

				#endregion
			}
		}

		[TestDate(2019, 02, 19)]
		public void TestNonPartitionedStagingTableIsDroppedBeforeCreatingNewPartitionKey()
		{
			var companyBranchPairPKs = SetupCompanyAndRegistry();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				#region Intialize JCD service task

				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask();
				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 2, Array.Empty<string>());

				#endregion

				#region Create first Non-Empty parition

				var paritionKey = ZDateTime.Today.Year.ToString() + ZDateTime.Today.Month.ToString("00") + companyBranchPairPKs[0].CompanyPK.ToString();
				CreatePeriods(ZDateTime.Today.Year, companyBranchPairPKs[0].CompanyPK);
				PostATransactionForCompany(companyBranchPairPKs[0].BranchPK, "S0001");

				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 14, new string[] { $"{paritionKey}->2" });

				#endregion

				MoveAllNonEmptyParitionToStagingTableAndAssertResult(paritionKey);
				((IDbConnected)Factory).Connection.ExecuteNonQuery("DROP INDEX JCDSTG_CI_JCD_PostDate ON RptDtJobCostingDataStaging; CREATE CLUSTERED INDEX JCDSTG_CI_JCD_PostDate ON RptDtJobCostingDataStaging(JCD_PostDate) ON REPORTGROUP");

				#region Create second Non-Empty Partition

				CreatePeriods(ZDateTime.Today.Year, companyBranchPairPKs[1].CompanyPK);
				PostATransactionForCompany(companyBranchPairPKs[1].BranchPK, "S0002");
				var expectedNonEmptyPartitonInfo = new string[] {
															ZDateTime.Today.Year.ToString() + ZDateTime.Today.Month.ToString("00") + companyBranchPairPKs[0].CompanyPK.ToString() + "->2",
															ZDateTime.Today.Year.ToString() + ZDateTime.Today.Month.ToString("00") + companyBranchPairPKs[1].CompanyPK.ToString() + "->2"
														};
				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 26, expectedNonEmptyPartitonInfo);

				#endregion
			}
		}

		[TestDate(2019, 02, 19)]
		public void TestNonEmptyParitionCanBeSwitchedToNonEmptyStagingTable()
		{
			var companyBranchPairPKs = SetupCompanyAndRegistry();

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				#region Intialize JCD service task

				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask();
				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 2, Array.Empty<string>());

				#endregion

				#region Create first Non-Empty parition

				var paritionKey = ZDateTime.Today.Year.ToString() + ZDateTime.Today.Month.ToString("00") + companyBranchPairPKs[0].CompanyPK.ToString();
				CreatePeriods(ZDateTime.Today.Year, companyBranchPairPKs[0].CompanyPK);
				PostATransactionForCompany(companyBranchPairPKs[0].BranchPK, "S0001");

				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 14, new string[] { $"{paritionKey}->2" });

				#endregion

				MoveAllNonEmptyParitionToStagingTableAndAssertResult(paritionKey);

				#region Create second Non-Empty Partition

				CreatePeriods(ZDateTime.Today.Year, companyBranchPairPKs[1].CompanyPK);
				PostATransactionForCompany(companyBranchPairPKs[1].BranchPK, "S0002");
				var expectedNonEmptyPartitonInfo = new string[] {
															ZDateTime.Today.Year.ToString() + ZDateTime.Today.Month.ToString("00") + companyBranchPairPKs[0].CompanyPK.ToString() + "->2",
															ZDateTime.Today.Year.ToString() + ZDateTime.Today.Month.ToString("00") + companyBranchPairPKs[1].CompanyPK.ToString() + "->2"
														};
				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 26, expectedNonEmptyPartitonInfo);

				#endregion

				MoveAllNonEmptyParitionToStagingTableAndAssertResult(
					ZDateTime.Today.Year.ToString() + ZDateTime.Today.Month.ToString("00") + companyBranchPairPKs[0].CompanyPK.ToString(),
					ZDateTime.Today.Year.ToString() + ZDateTime.Today.Month.ToString("00") + companyBranchPairPKs[1].CompanyPK.ToString());

				#region Create 3rd Non-Empty Partition

				CreatePeriods(ZDateTime.Today.Year, companyBranchPairPKs[2].CompanyPK);
				PostATransactionForCompany(companyBranchPairPKs[2].BranchPK, "S0003");
				expectedNonEmptyPartitonInfo = new string[] {
															ZDateTime.Today.Year.ToString() + ZDateTime.Today.Month.ToString("00") + companyBranchPairPKs[0].CompanyPK.ToString() + "->2",
															ZDateTime.Today.Year.ToString() + ZDateTime.Today.Month.ToString("00") + companyBranchPairPKs[1].CompanyPK.ToString() + "->2",
															ZDateTime.Today.Year.ToString() + ZDateTime.Today.Month.ToString("00") + companyBranchPairPKs[2].CompanyPK.ToString() + "->2"
														};
				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 38, expectedNonEmptyPartitonInfo);

				#endregion
			}
		}

		[ExpectNoExceptions]
		[TestDate(2019, 02, 19)]
		public void TestNewPartitionCanBeCreatedBySplittingNonEmptyPartitionWithTrustedConstraints()
		{
			CreatePartitionBySplitting(orderByDescending: true, makeConstraintsTrusted: true);
			Assert(true);
		}

		[TestDate(2022, 02, 19)]
		public void TestOldPartitionsAreDropped_NewCompany()
		{
			var newCompany1 = ObjectCreator.CreateNewCompany("CO1", orgProxy: ObjectCreator.TestOrganisation);
			var newBranch1 = ObjectCreator.CreateNewBranch(newCompany1, "BR1");
			newBranch1.Factory.Save();

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20);

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();

				CreatePeriods(ZDateTime.Today.Year, newCompany1.PK);
				dataPopulatingTask.RunTask();
				dataPopulatingTask.RunTask();

				var nonEmptyPartitions = new List<string>();
				nonEmptyPartitions.AddRange(GetExpectedPeriodKeys("SCOM1_1", (newCompany1.PK, newBranch1.PK)));
				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 14, nonEmptyPartitions.ToArray());
				AssertEquals("Starting Period", Convert.ToInt32(FormattableString.Invariant($"{ZDateTime.Today.Year}01")), AccountingConfigurationRegistry.Instance.JCDReportDataCollectionStartingPeriod.Value);

				var newCompany2 = ObjectCreator.CreateNewCompany("CO2", orgProxy: ObjectCreator.Creditor1);
				var newBranch2 = ObjectCreator.CreateNewBranch(newCompany2, "BR2");
				newBranch2.Factory.Save();

				var allPartitionsOfThe2ndCompany = GetExpectedPeriodKeys("SCOM2_1", (newCompany2.PK, newBranch2.PK));
				nonEmptyPartitions.AddRange(allPartitionsOfThe2ndCompany);

				var oldPartitions = nonEmptyPartitions.Where(x => x.StartsWith(FormattableString.Invariant($"{ZDateTime.Today.Year}01")) ||
																			x.StartsWith(FormattableString.Invariant($"{ZDateTime.Today.Year}02")));
				var nonEmptyPartitionsWithoutTheOldOnes = nonEmptyPartitions.Except(oldPartitions);

				CreatePeriods(ZDateTime.Today.Year, newCompany2.PK);
				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 22, nonEmptyPartitionsWithoutTheOldOnes.ToArray());
				AssertEquals("Starting Period", Convert.ToInt32(FormattableString.Invariant($"{ZDateTime.Today.Year}03")), AccountingConfigurationRegistry.Instance.JCDReportDataCollectionStartingPeriod.Value);
			}
		}

		[TestDate(2022, 02, 19)]
		public void TestStartingPeriodIsNotChangedWhenMaximumAllowedPartitionsIsChangedToHigherValue()
		{
			var newCompany1 = ObjectCreator.CreateNewCompany("CO1", orgProxy: ObjectCreator.TestOrganisation);
			var newBranch1 = ObjectCreator.CreateNewBranch(newCompany1, "BR1");
			newBranch1.Factory.Save();

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 9);
			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();

				CreatePeriods(ZDateTime.Today.Year, newCompany1.PK);
				dataPopulatingTask.RunTask();
				dataPopulatingTask.RunTask();

				var nonEmptyPartitions = new List<string>();
				nonEmptyPartitions.AddRange(GetExpectedPeriodKeys("SCOM1_1", (newCompany1.PK, newBranch1.PK)));
				var oldPartitions = nonEmptyPartitions.Where(x => x.StartsWith(FormattableString.Invariant($"{ZDateTime.Today.Year}01")) ||
																			x.StartsWith(FormattableString.Invariant($"{ZDateTime.Today.Year}02")) ||
																			x.StartsWith(FormattableString.Invariant($"{ZDateTime.Today.Year}03")));
				var nonEmptyPartitionsWithoutTheOldOnes = nonEmptyPartitions.Except(oldPartitions);

				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 11, nonEmptyPartitionsWithoutTheOldOnes.ToArray());
				AssertEquals("Starting Period", Convert.ToInt32(FormattableString.Invariant($"{ZDateTime.Today.Year}04")), AccountingConfigurationRegistry.Instance.JCDReportDataCollectionStartingPeriod.Value);

				var reportTableRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM RptDtJobCostingData");
				AssertEquals("Report table row count", 9, reportTableRecordCount);

				var queueRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCostingDataQueue");
				AssertEquals("Report table row count", 0, queueRecordCount);

				AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);
				for (int i = 1; i <= 3; i++)
				{
					var offset = i - ZDateTime.Today.Month;
					PostATransactionForCompany(newBranch1.PK, "SC00" + i.ToString(), ZDateTime.Today.AddMonths(offset));
				}

				queueRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCostingDataQueue");
				AssertEquals("Report table row count", 6, queueRecordCount);

				AccountingMasterFilesRegistry.Instance.NewCompanyPartitionKeyNeedsToBeCreated.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				dataPopulatingTask.RunTask();
				reportTableRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM RptDtJobCostingData");
				AssertEquals("Report table row count", 9, reportTableRecordCount);

				queueRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCostingDataQueue");
				AssertEquals("Report table row count", 0, queueRecordCount);
				AssertEquals("Starting Period", Convert.ToInt32(FormattableString.Invariant($"{ZDateTime.Today.Year}04")), AccountingConfigurationRegistry.Instance.JCDReportDataCollectionStartingPeriod.Value);
			}
		}

		[TestDate(2022, 02, 19)]
		public void TestStartingPeriodIsNotChangedWhenMaximumAllowedPartitionsIsChangedToLowerValue()
		{
			var newCompany1 = ObjectCreator.CreateNewCompany("CO1", orgProxy: ObjectCreator.TestOrganisation);
			var newBranch1 = ObjectCreator.CreateNewBranch(newCompany1, "BR1");
			newBranch1.Factory.Save();

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);
			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();

				CreatePeriods(ZDateTime.Today.Year, newCompany1.PK);
				dataPopulatingTask.RunTask();
				dataPopulatingTask.RunTask();

				var nonEmptyPartitions = new List<string>();
				nonEmptyPartitions.AddRange(GetExpectedPeriodKeys("SCOM1_1", (newCompany1.PK, newBranch1.PK)));

				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 14, nonEmptyPartitions.ToArray());
				AssertEquals("Starting Period", Convert.ToInt32(FormattableString.Invariant($"{ZDateTime.Today.Year}01")), AccountingConfigurationRegistry.Instance.JCDReportDataCollectionStartingPeriod.Value);

				var reportTableRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM RptDtJobCostingData");
				AssertEquals("Report table row count", 24, reportTableRecordCount);

				var queueRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCostingDataQueue");
				AssertEquals("Report table row count", 0, queueRecordCount);

				AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 9);
				AccountingMasterFilesRegistry.Instance.NewCompanyPartitionKeyNeedsToBeCreated.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var oldPartitions = nonEmptyPartitions.Where(x => x.StartsWith(FormattableString.Invariant($"{ZDateTime.Today.Year}01")) ||
																			x.StartsWith(FormattableString.Invariant($"{ZDateTime.Today.Year}02")) ||
																			x.StartsWith(FormattableString.Invariant($"{ZDateTime.Today.Year}03")));
				var nonEmptyPartitionsWithoutTheOldOnes = nonEmptyPartitions.Except(oldPartitions);

				queueRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCostingDataQueue");
				AssertEquals("Report table row count", 0, queueRecordCount);

				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 11, nonEmptyPartitionsWithoutTheOldOnes.ToArray());

				reportTableRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM RptDtJobCostingData");
				AssertEquals("Report table row count", 9, reportTableRecordCount);

				queueRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCostingDataQueue");
				AssertEquals("Report table row count", 0, queueRecordCount);
				AssertEquals("Starting Period", Convert.ToInt32(FormattableString.Invariant($"{ZDateTime.Today.Year}04")), AccountingConfigurationRegistry.Instance.JCDReportDataCollectionStartingPeriod.Value);
			}
		}

		[TestDate(2022, 02, 19)]
		public void TestQueueRecordsBelongingToOldPartitionsAreNotProcessed()
		{
			var newCompany1 = ObjectCreator.CreateNewCompany("CO1", orgProxy: ObjectCreator.TestOrganisation);
			var newBranch1 = ObjectCreator.CreateNewBranch(newCompany1, "BR1");
			newBranch1.Factory.Save();

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 9);

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();

				CreatePeriods(ZDateTime.Today.Year, newCompany1.PK);
				dataPopulatingTask.RunTask();
				dataPopulatingTask.RunTask();

				var nonEmptyPartitions = new List<string>();
				nonEmptyPartitions.AddRange(GetExpectedPeriodKeys("SCOM1_1", (newCompany1.PK, newBranch1.PK)));
				var oldPartitions = nonEmptyPartitions.Where(x => x.StartsWith(FormattableString.Invariant($"{ZDateTime.Today.Year}01")) ||
																			x.StartsWith(FormattableString.Invariant($"{ZDateTime.Today.Year}02")) ||
																			x.StartsWith(FormattableString.Invariant($"{ZDateTime.Today.Year}03")));
				var nonEmptyPartitionsWithoutTheOldOnes = nonEmptyPartitions.Except(oldPartitions);

				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 11, nonEmptyPartitionsWithoutTheOldOnes.ToArray());
				AssertEquals("Starting Period", Convert.ToInt32(FormattableString.Invariant($"{ZDateTime.Today.Year}04")), AccountingConfigurationRegistry.Instance.JCDReportDataCollectionStartingPeriod.Value);

				var reportTableRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.RptDtJobCostingData");
				AssertEquals("Report table row count", 9, reportTableRecordCount);
				var reportDependentTableRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.RptDtJobCostingDataAmountByJob");
				AssertEquals("Report dependent table row count", 9, reportDependentTableRecordCount);
				var queueRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCostingDataQueue");
				AssertEquals("Report table row count", 0, queueRecordCount);

				for (int i = 1; i <= 3; i++)
				{
					var offset = i - ZDateTime.Today.Month;
					PostATransactionForCompany(newBranch1.PK, "SC00" + i.ToString(), ZDateTime.Today.AddMonths(offset));
				}

				queueRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCostingDataQueue");
				AssertEquals("Report table row count", 6, queueRecordCount);

				dataPopulatingTask.RunTask();

				reportTableRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.RptDtJobCostingData");
				AssertEquals("Report table row count", 9, reportTableRecordCount);
				reportDependentTableRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.RptDtJobCostingDataAmountByJob");
				AssertEquals("Report dependent table row count", 9, reportDependentTableRecordCount);
				queueRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.JobCostingDataQueue");
				AssertEquals("Report table row count", 0, queueRecordCount);
			}
		}

		[TestDate(2022, 02, 19)]
		public void TestQueueRecordsBelongingToDeletedPartitionsAreRemoved()
		{
			var newCompany1 = ObjectCreator.CreateNewCompany("CO1", orgProxy: ObjectCreator.TestOrganisation);
			var newBranch1 = ObjectCreator.CreateNewBranch(newCompany1, "BR1");
			newBranch1.Factory.Save();

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 9);

			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();

				CreatePeriods(ZDateTime.Today.Year, newCompany1.PK);
				dataPopulatingTask.RunTask();
				dataPopulatingTask.RunTask();

				var nonEmptyPartitions = new List<string>();
				nonEmptyPartitions.AddRange(GetExpectedPeriodKeys("SCOM1_1", (newCompany1.PK, newBranch1.PK)));
				var oldPartitions = nonEmptyPartitions.Where(x => x.StartsWith(FormattableString.Invariant($"{ZDateTime.Today.Year}01")) ||
																			x.StartsWith(FormattableString.Invariant($"{ZDateTime.Today.Year}02")) ||
																			x.StartsWith(FormattableString.Invariant($"{ZDateTime.Today.Year}03")));
				var nonEmptyPartitionsWithoutTheOldOnes = nonEmptyPartitions.Except(oldPartitions);

				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 11, nonEmptyPartitionsWithoutTheOldOnes.ToArray());
				AssertEquals("Starting Period", Convert.ToInt32(FormattableString.Invariant($"{ZDateTime.Today.Year}04")), AccountingConfigurationRegistry.Instance.JCDReportDataCollectionStartingPeriod.Value);

				var reportTableRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.RptDtJobCostingData");
				AssertEquals("Precondition: Report table row count", 9, reportTableRecordCount);
				var reportDependentTableRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.RptDtJobCostingDataAmountByJob");
				AssertEquals("Precondition: Report dependent table row count", 9, reportDependentTableRecordCount);

				// Need to reduce partition count by 2 before partitions are dropped.
				AccountingConfigurationRegistry.Instance.MaximumNumberOfJCDPartitionKeys.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 7);
				AccountingMasterFilesRegistry.Instance.NewCompanyPartitionKeyNeedsToBeCreated.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				dataPopulatingTask.RunTask();

				reportTableRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.RptDtJobCostingData");
				AssertEquals("Report table row count should be reduced when maximum partition count is reduced", 7, reportTableRecordCount);
				reportDependentTableRecordCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.RptDtJobCostingDataAmountByJob");
				AssertEquals("Report dependent table row count should be reduced when maximum partition count is reduced", 7, reportDependentTableRecordCount);
			}
		}

		[TestDate(2024, 06, 25)]
		public void TestOrphanPartitionsAreDeleted()
		{
			var newCompany1 = ObjectCreator.CreateNewCompany("CO1", orgProxy: ObjectCreator.TestOrganisation);
			var newBranch1 = ObjectCreator.CreateNewBranch(newCompany1, "BR1");
			newBranch1.Factory.Save();

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);
			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();

				//will create all JCD DB objects
				dataPopulatingTask.RunTask();

				//Creating partitons that are orphan as there is no corresponding acc. period or company
				CreateAndAssertOrphanPartitions();

				//Creating new valid acc. periods
				CreatePeriods(ZDateTime.Today.Year, newCompany1.PK);

				//Running service task again. All orphan partitions should be removed and new partitions should get created.
				dataPopulatingTask.RunTask();

				var expectedPartitionKeys = new List<string>() { };
				for (var i = 1; i <= 12; i++)
				{
					expectedPartitionKeys.Add($"'2024{i:00}{newCompany1.PK}'");
				}

				AssertPartitionInfo("All orphan partitions should be removed.", expectedPartitionKeys);
			}

			void CreateAndAssertOrphanPartitions()
			{
				var fakeCompanyPK1 = Guid.NewGuid();
				var fakeCompanyPK2 = Guid.NewGuid();
				var fakePeriodKeys = Enumerable.Range(1, 12)
				  .SelectMany(period => new[] {
						$"'2022{period:00}{fakeCompanyPK1}'",
						$"'2022{period:00}{fakeCompanyPK2}'",
						$"'2023{period:00}{fakeCompanyPK1}'",
						$"'2023{period:00}{fakeCompanyPK2}'" });

				//I am using brute force to create these partitons faster. Using swicth and split mechanism for partitoning will be time consuming.
				var sql = FormattableString.Invariant($@"

	DROP Table RptDtJobCostingData

	DROP PARTITION SCHEME PS_AccountingPeriodCompany;

	DROP PARTITION FUNCTION PF_AccountingPeriodCompany;

	CREATE PARTITION FUNCTION PF_AccountingPeriodCompany(char(42)) AS RANGE LEFT FOR VALUES ('99999900000000-0000-0000-0000-000000000000', {string.Join(",", fakePeriodKeys)}) ;

	CREATE PARTITION SCHEME PS_AccountingPeriodCompany AS PARTITION PF_AccountingPeriodCompany ALL TO ([REPORTGROUP]);

	EXEC CreateJobCostingDataTableAndCCI @isStagingTable = 0");

				TestConnection.ExecuteNonQuery(sql);

				AssertPartitionInfo("A number of orphan partitions (i.e. there is no corresponsing acc. period) are created.", fakePeriodKeys);
			}

			void AssertPartitionInfo(string message, IEnumerable<string> expectedPartitionKeys)
			{
				var pi = LoadPartitionInfo().Select("UpperBoundaryValue NOT IN ('99999900000000-0000-0000-0000-000000000000', '')"); //Excluding partitions that we need to have by default to support partitioning process
				var actualPartitionInfo = pi.Select(x => string.Concat("'", Convert.ToString(x["UpperBoundaryValue"]).ToLower(), "'"));
				AssertContainsExactElementsInAnyOrder(message, expectedPartitionKeys, actualPartitionInfo);
			}
		}

		string[] GetExpectedPeriodKeys(string prefix, params (ZGuid companyPK, ZGuid branchPK)[] companyBranchPairPKs)
		{
			var expectedNonEmptyPartitonInfo = new List<string>();
			foreach (var companyBranchPairPK in companyBranchPairPKs)
			{
				for (int i = 1; i <= 12; i++)
				{
					var offset = i - ZDateTime.Today.Month;
					PostATransactionForCompany(companyBranchPairPK.branchPK, prefix + i.ToString(), ZDateTime.Today.AddMonths(offset));
					if (offset == 0)
					{
						expectedNonEmptyPartitonInfo.Add(ZDateTime.Today.Year.ToString() + ZDateTime.Today.Month.ToString("00") + companyBranchPairPK.companyPK.ToString() + "->13");
					}
					else
					{
						expectedNonEmptyPartitonInfo.Add(ZDateTime.Today.Year.ToString() + ZDateTime.Today.AddMonths(offset).Month.ToString("00") + companyBranchPairPK.companyPK.ToString() + "->1");
					}
				}
			}
			return expectedNonEmptyPartitonInfo.ToArray();
		}

		(ZGuid CompanyPK, ZGuid BranchPK)[] SetupCompanyAndRegistry()
		{
			var newCompany1 = ObjectCreator.CreateNewCompany("CO1", orgProxy: ObjectCreator.TestOrganisation);
			var newBranch1 = ObjectCreator.CreateNewBranch(newCompany1, "BR1");

			var newCompany2 = ObjectCreator.CreateNewCompany("CO2", orgProxy: ObjectCreator.Creditor1);
			var newBranch2 = ObjectCreator.CreateNewBranch(newCompany2, "BR2");

			var newCompany3 = ObjectCreator.CreateNewCompany("CO3", orgProxy: ObjectCreator.Creditor1);
			var newBranch3 = ObjectCreator.CreateNewBranch(newCompany3, "BR3");

			Factory.Save();

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			return new (ZGuid CompanyPK, ZGuid BranchPK)[]
						{
								(newCompany1.PK, newBranch1.PK),
								(newCompany2.PK, newBranch2.PK),
								(newCompany3.PK, newBranch3.PK)
						}.OrderBy(x => x.CompanyPK.ToString()).ToArray();
		}

		void MoveAllNonEmptyParitionToStagingTableAndAssertResult(params string[] keys)
		{
			foreach (var key in keys)
			{
				((IDbConnected)Factory).Connection.ExecuteNonQuery("EXEC CreateJobCostingDataTableAndCCI @isStagingTable = 1");
				((IDbConnected)Factory).Connection.ExecuteNonQuery($@"ALTER TABLE RptDtJobCostingData 
SWITCH PARTITION $PARTITION.PF_AccountingPeriodCompany('{key}') TO 
RptDtJobCostingDataStaging PARTITION $PARTITION.PF_AccountingPeriodCompany('{key}');");
			}
			((IDbConnected)Factory).Connection.ExecuteNonQuery("EXEC CreateJobCostingDataTableAndCCI @isStagingTable = 1");

			var pi = LoadPartitionInfo();
			var nonEmptyPartitions = pi.Select("Rows > 0");
			AssertEquals("Non-Empty Partition Info", 0, nonEmptyPartitions.Length);
		}

		void CreatePartitionBySplitting(bool orderByDescending, bool makeConstraintsTrusted)
		{
			#region Setup

			var newCompany1 = ObjectCreator.CreateNewCompany("CO1", orgProxy: ObjectCreator.TestOrganisation);
			var newBranch1 = ObjectCreator.CreateNewBranch(newCompany1, "BR1");

			var newCompany2 = ObjectCreator.CreateNewCompany("CO2", orgProxy: ObjectCreator.Creditor1);
			var newBranch2 = ObjectCreator.CreateNewBranch(newCompany2, "BR2");

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.InitializeJobCostingDataQueueServiceTask);

			#endregion

			//Populating RptDtUnprocessedAccTransactionLines table
			using (var dataPopulatingTask = new JCDActionStrategyTestBase.JobCostingDataPopulationServiceTask_ForTest(TestConnection))
			{
				dataPopulatingTask.ServiceLogger = new TestServiceLogger();
				dataPopulatingTask.RunTask();

				var companyBranchPairPKs = new[] {
												Tuple.Create(newCompany1.PK, newBranch1.PK),
												Tuple.Create(newCompany2.PK, newBranch2.PK)
											  }.OrderByDescending(x => x.Item1.ToString()).ToArray();

				var sortedCompanyBranchPairPKs = orderByDescending ? companyBranchPairPKs.OrderByDescending(x => x.Item1.ToString()).ToArray() : companyBranchPairPKs.OrderBy(x => x.Item1.ToString()).ToArray();

				#region Run JCD Service Task for the first time

				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 2, Array.Empty<string>());

				if (makeConstraintsTrusted)
				{
					((IDbConnected)Factory).Connection.ExecuteNonQuery("ALTER TABLE RptDtJobCostingData WITH CHECK CHECK CONSTRAINT ALL");
				}

				#endregion

				#region Create Period and Post a Transaction for one Company

				CreatePeriods(ZDateTime.Today.Year, sortedCompanyBranchPairPKs[0].Item1);
				var expectedNonEmptyPartitonInfo = new List<string>();
				for (int i = 1; i <= 12; i++)
				{
					var offset = i - ZDateTime.Today.Month;
					PostATransactionForCompany(sortedCompanyBranchPairPKs[0].Item2, "S0001_" + i.ToString(), ZDateTime.Today.AddMonths(offset));

					if (offset == 0)
					{
						expectedNonEmptyPartitonInfo.Add(ZDateTime.Today.Year.ToString() + ZDateTime.Today.Month.ToString("00") + sortedCompanyBranchPairPKs[0].Item1.ToString() + "->13");
					}
					else
					{
						expectedNonEmptyPartitonInfo.Add(ZDateTime.Today.Year.ToString() + ZDateTime.Today.AddMonths(offset).Month.ToString("00") + sortedCompanyBranchPairPKs[0].Item1.ToString() + "->1");
					}
				}

				#endregion

				#region Run JCD Service task which populates RptDtJobCostingData Table 

				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 14, expectedNonEmptyPartitonInfo.ToArray());

				#endregion

				#region Now Create Period and post a Transaction for the Other Company 

				CreatePeriods(ZDateTime.Today.Year, sortedCompanyBranchPairPKs[1].Item1);

				for (int i = 1; i <= 12; i++)
				{
					var offset = i - ZDateTime.Today.Month;
					PostATransactionForCompany(sortedCompanyBranchPairPKs[1].Item2, "S0002_" + i.ToString(), ZDateTime.Today.AddMonths(offset));

					if (offset == 0)
					{
						expectedNonEmptyPartitonInfo.Add(ZDateTime.Today.Year.ToString() + ZDateTime.Today.Month.ToString("00") + sortedCompanyBranchPairPKs[1].Item1.ToString() + "->13");
					}
					else
					{
						expectedNonEmptyPartitonInfo.Add(ZDateTime.Today.Year.ToString() + ZDateTime.Today.AddMonths(offset).Month.ToString("00") + sortedCompanyBranchPairPKs[1].Item1.ToString() + "->1");
					}
				}

				#endregion

				#region Run JCD Service task again

				RunJCDAndAssertPartitionInfo(dataPopulatingTask, 26, expectedNonEmptyPartitonInfo.ToArray());

				#endregion
			}
		}

		void RunJCDAndAssertPartitionInfo(JobCostingDataPopulationServiceTask dataPopulatingTask, int expectedPartitionCount, string[] expectedNonEmptyPartitionInfo)
		{
			dataPopulatingTask.RunTask();

			var pi = LoadPartitionInfo();
			AssertEquals("Total Partition Count", expectedPartitionCount, pi.Rows.Count);

			var nonEmptyPartitions = pi.Select("Rows > 0");
			AssertEquals("Nonempty Partition Info", expectedNonEmptyPartitionInfo.Length, nonEmptyPartitions.Length);
			if (expectedNonEmptyPartitionInfo.Length > 0)
			{
				var actualNonEmptyPartitionInfo = nonEmptyPartitions.Select(x => Convert.ToString(x["UpperBoundaryValue"]).ToLower() + "->" + Convert.ToInt32(x["Rows"]));
				AssertContainsExactElementsInAnyOrder(expectedNonEmptyPartitionInfo, actualNonEmptyPartitionInfo);
			}
		}

		void CreatePeriods(int year, ZGuid companyPK)
		{
			var periodTestHelper = new AccountingPeriodTestHelper(Factory);
			periodTestHelper.PostPeriodsForEntireYear(year, companyPK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
		}

		void PostATransactionForCompany(ZGuid branchPK, string shipmentNumber)
		{
			PostATransactionForCompany(branchPK, shipmentNumber, ZDateTime.Today);
		}

		void PostATransactionForCompany(ZGuid branchPK, string shipmentNumber, ZDateTime postDate)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchPK.ToGuid(), objectCreator.FEADepartment.PK.ToGuid()))
			{
				var shipment = ObjectCreator.CreateShipment(shipmentNumber);
				var job = ObjectCreator.CreateJob(shipment, ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
				var apInvoice = ObjectCreator.CreateInvoice(typeof(APInvoice), objectCreator.AUD, 1.0M, ObjectCreator.Creditor2);
				apInvoice.Lines.RemoveAndDeleteAll();
				apInvoice.AH_PostDate = postDate;
				var apInvoiceLine = ObjectCreator.CreateInvoiceLine(apInvoice, job, ObjectCreator.CC1, 250M);
				var jobCharge = ObjectCreator.CreateCharge(apInvoiceLine);
				jobCharge.JR_OH_SellAccount = ZGuid.Empty;
				Factory.Save();
			}
		}

		DataTable LoadPartitionInfo()
		{
			var table = DataUtils.GetDataTableFromQuery(TestConnection,
			@"SELECT  OBJECT_NAME(p.object_id) AS ObjectName,
					  i.name AS IndexName,
					  p.index_id AS IndexID,
					  ds.name AS PartitionScheme,    
					  p.partition_number AS PartitionNumber,
					  fg.name  AS FileGroupName,
					  prv_left.value AS LowerBoundaryValue,
					  prv_right.value AS UpperBoundaryValue,
					  CASE pf.boundary_value_on_right WHEN 1 THEN 'RIGHT' ELSE 'LEFT' END AS Range,
					  p.rows AS Rows 
			  FROM	  sys.partitions AS p
					  JOIN sys.indexes AS i ON i.object_id = p.object_id AND i.index_id = p.index_id
					  JOIN sys.data_spaces AS ds ON ds.data_space_id = i.data_space_id
					  JOIN sys.partition_schemes AS ps ON ps.data_space_id = ds.data_space_id
					  JOIN sys.partition_functions AS pf ON pf.function_id = ps.function_id
					  JOIN sys.destination_data_spaces AS dds2 ON dds2.partition_scheme_id = ps.data_space_id AND dds2.destination_id = p.partition_number
					  JOIN sys.filegroups AS fg ON fg.data_space_id = dds2.data_space_id
					  LEFT JOIN sys.partition_range_values AS prv_left ON ps.function_id = prv_left.function_id AND prv_left.boundary_id = p.partition_number - 1
					  LEFT JOIN sys.partition_range_values AS prv_right ON ps.function_id = prv_right.function_id AND prv_right.boundary_id = p.partition_number  
			  WHERE   OBJECT_NAME(p.object_id) = 'RptDtJobCostingData' and i.name = 'CI_JCD_PostDate'
			  ORDER BY PartitionNumber");

			return table;
		}

		TestObjectCreator ObjectCreator
		{
			get
			{
				if (objectCreator == null)
				{
					objectCreator = new TestObjectCreator(Factory);
				}
				return objectCreator;
			}
		}
		TestObjectCreator objectCreator;
	}
}
