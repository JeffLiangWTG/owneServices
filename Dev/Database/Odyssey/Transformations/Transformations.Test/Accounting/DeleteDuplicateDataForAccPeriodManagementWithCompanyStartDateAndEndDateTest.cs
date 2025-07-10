using System;
using System.Data;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Accounting;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting
{
	[TestedType(typeof(DeleteDuplicateDataForAccPeriodManagementWithCompanyStartDateAndEndDate))]
	public sealed class DeleteDuplicateDataForAccPeriodManagementWithCompanyStartDateAndEndDateTest : DataTransformationTestCase
	{
		public DeleteDuplicateDataForAccPeriodManagementWithCompanyStartDateAndEndDateTest()
		{
			testDbHelper = new TestDbHelper(TestConnection);
		}

		public void TestSkipTransformationWhenAccPeriodManagementNotExists()
		{
			DropTableDependencies();
			TestConnection.ExecuteNonQuery($"DROP TABLE {AccPeriodManagementSchema.Constants.SqlSchemaName}.{tableName}");
			AssertEquals(false, DbObjectCreator.TableExists(TestConnection, tableName));

			AssertNoExceptionThrown(() => RunTransformation());
		}

		public void TestSkipTransformationWhenAccPeriodManagementColumnsNotExists()
		{
			AssertSkipTransformationWhenColumnNotExists("AM_GC_Company");
			AssertSkipTransformationWhenColumnNotExists("AM_StartDate");
			AssertSkipTransformationWhenColumnNotExists("AM_EndDate");
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new DeleteDuplicateDataForAccPeriodManagementWithCompanyStartDateAndEndDate();

		protected override void AssertTransformationResults()
		{
			CombineAssertions(() =>
			{
				AssertAM_PKWithPeriod(periodPK_Vaild, TestDbHelper.DefaultCompanyPK, 202401);
				AssertAM_PKWithPeriod(periodPK_Duplicate_oldPeriod, TestDbHelper.DefaultCompanyPK, 202403);
				AssertAM_PKWithPeriod(periodPK_Duplicate_oldPeriod1, TestDbHelper.DefaultCompanyPK, 202404);
				AssertNoRecordWithPK(periodPK_Duplicate_newPeriod);
				AssertNoRecordWithPK(periodPK_Duplicate_newPeriod1);

				AssertAM_PKWithPeriod(periodPK_VaildDAU, companyPK, 202401);
				AssertAM_PKWithPeriod(periodPK_Duplicate_oldPeriodDAU, companyPK, 202403);
				AssertAM_PKWithPeriod(periodPK_Duplicate_oldPeriodDAU1, companyPK, 202404);
				AssertNoRecordWithPK(periodPK_Duplicate_newPeriodDAU);
				AssertNoRecordWithPK(periodPK_Duplicate_newPeriodDAU1);
			});
		}

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropIndexIfExists(tableName, "NR_UX__AM_GC_Company_AM_StartDate_AM_EndDate");
			companyPK = testDbHelper.InsertCompany("DAU", "AUD DEMO", "AUD", "AU", false, false);

			periodPK_Vaild = testDbHelper.InsertAccPeriod(2024, 1, null, new DateTime(2024, 1, 1));
			periodPK_VaildDAU = testDbHelper.InsertAccPeriod(2024, 1, companyPK, new DateTime(2024, 1, 1));

			periodPK_Duplicate_oldPeriod = testDbHelper.InsertAccPeriod(2024, 3, null, new DateTime(2024, 3, 1));
			periodPK_Duplicate_newPeriod = testDbHelper.InsertAccPeriod(2025, 3, null, new DateTime(2024, 3, 1));

			periodPK_Duplicate_oldPeriod1 = testDbHelper.InsertAccPeriod(2024, 4, null, new DateTime(2024, 4, 1));
			periodPK_Duplicate_newPeriod1 = testDbHelper.InsertAccPeriod(2025, 4, null, new DateTime(2024, 4, 1));

			periodPK_Duplicate_oldPeriodDAU = testDbHelper.InsertAccPeriod(2024, 3, companyPK, new DateTime(2024, 3, 1));
			periodPK_Duplicate_newPeriodDAU = testDbHelper.InsertAccPeriod(2025, 3, companyPK, new DateTime(2024, 3, 1));

			periodPK_Duplicate_oldPeriodDAU1 = testDbHelper.InsertAccPeriod(2024, 4, companyPK, new DateTime(2024, 4, 1));
			periodPK_Duplicate_newPeriodDAU1 = testDbHelper.InsertAccPeriod(2025, 4, companyPK, new DateTime(2024, 4, 1));
		}

		void AssertAM_PKWithPeriod(Guid expectedAM_PK, Guid companyPK, int period)
		{
			var sql = @$"SELECT AM_PK
						FROM {tableName}
						WHERE AM_Period = @period AND AM_GC_Company = @companyPK";

			using (var cmd = TestConnection.Command(sql))
			{
				cmd.AddParameter("period", SqlDbType.Int, period);
				cmd.AddParameter("companyPK", SqlDbType.UniqueIdentifier, companyPK);
				var dataTable = DataUtils.GetDataTableFromCommand(cmd);

				AssertEquals(1, dataTable.Rows.Count);
				AssertEquals(expectedAM_PK, dataTable.Rows[0]["AM_PK"]);
			}
		}

		void AssertNoRecordWithPK(Guid expctedAM_PK)
		{
			var sql = @$"SELECT AM_PK
						FROM {tableName}
						WHERE AM_PK = @AM_PK";

			using (var cmd = TestConnection.Command(sql))
			{
				cmd.AddParameter("AM_PK", SqlDbType.UniqueIdentifier, expctedAM_PK);
				var dataTable = DataUtils.GetDataTableFromCommand(cmd);

				AssertEquals(0, dataTable.Rows.Count);
			}
		}

		void AssertSkipTransformationWhenColumnNotExists(string columnName)
		{
			DropTableDependencies();
			DBTransformationTestHelper.DropIndexIfExists(tableName, "FK_UC__AM_GC_Company_AM_Period");
			DBTransformationTestHelper.DropIndexIfExists(tableName, "NR_UX__AM_GC_Company_AM_StartDate_AM_EndDate");
			DBTransformationTestHelper.DropConstraintIfExists(tableName, "AccPeriodManagement_AM_GC_Company_FK2_GlbCompany_RRR_120N");
			DBTransformationTestHelper.DropColumnIfExists(tableName, columnName);

			AssertEquals(false, DbObjectCreator.ColumnsExist(
				TestConnection,
				TestConnection.CurrentDatabase,
				AccPeriodManagementSchema.Constants.SqlSchemaName,
				tableName,
				new string[] { "AM_GC_Company", "AM_StartDate", "AM_EndDate" }));

			AssertNoExceptionThrown(() => RunTransformation());
		}

		void DropTableDependencies()
		{
			DBTransformationTestHelper.DropConstraintIfExists(AccConsolidationBatchSchema.Constants.TableName, "AccConsolidationBatch_YB_AM_Period_FK2_AccPeriodManagement_RRR_120N");
			DBTransformationTestHelper.DropFunctionIfExists("JobProfitChargeCodeSummary");
			DBTransformationTestHelper.DropFunctionIfExists("JobProfitPeriodSummary");
			DBTransformationTestHelper.DropFunctionIfExists("JobProfitPeriodBase");
			DBTransformationTestHelper.DropViewIfExists("GLJournal");
			DBTransformationTestHelper.DropFunctionIfExists("citf_GLJournal");
			DBTransformationTestHelper.DropFunctionIfExists("JobProfitPeriodBase2");
			DBTransformationTestHelper.DropFunctionIfExists("GetPeriodFromDate");
			DBTransformationTestHelper.DropFunctionIfExists("tfn_ShowPeriodWholeYear");
			DBTransformationTestHelper.DropFunctionIfExists("Report_PendingInputOutputTax");
			DBTransformationTestHelper.DropFunctionIfExists("GetAgeInDaysFromDateOrPeriod");
			DBTransformationTestHelper.DropFunctionIfExists("CashAtBeginningOfPeriod");
			DBTransformationTestHelper.DropFunctionIfExists("CashFlowTypeStillInUseButNotFoundInRegistry");
			DBTransformationTestHelper.DropFunctionIfExists("Report_OutstandingARAPTransactionsListing");
			DBTransformationTestHelper.DropFunctionIfExists("Report_CashFlowDetailedListing");
			DBTransformationTestHelper.DropFunctionIfExists("Report_CashFlowStatementChina");
			DBTransformationTestHelper.DropFunctionIfExists("Report_CashFlowStatement");
			DBTransformationTestHelper.DropFunctionIfExists("Report_OrgGlobalCreditProfile");
			DBTransformationTestHelper.DropFunctionIfExists("GetGlobalCreditLimitOrganizationPerCompanyWithExRates");
			DBTransformationTestHelper.DropFunctionIfExists("GetGenericTransactions");
			DBTransformationTestHelper.DropFunctionIfExists("ARCreditNotesIssued");
			DBTransformationTestHelper.DropFunctionIfExists("GetPeriodFromDateInline");
			DBTransformationTestHelper.DropFunctionIfExists("Report_Analysis12PeriodCallCountByClientAndType");
			DBTransformationTestHelper.DropFunctionIfExists("ARAPEXXOVPDSCJNLPostingMovements");
			DBTransformationTestHelper.DropFunctionIfExists("ARAPPAYRECPostingMovements");
			DBTransformationTestHelper.DropFunctionIfExists("ARAPTRFCTRPostingMovements");
			DBTransformationTestHelper.DropFunctionIfExists("CBDPYPostingMovements");
			DBTransformationTestHelper.DropFunctionIfExists("CBDRCPostingMovements");
			DBTransformationTestHelper.DropFunctionIfExists("CBEXXPostingMovements");
			DBTransformationTestHelper.DropFunctionIfExists("AccrualPostingMovements");
			DBTransformationTestHelper.DropFunctionIfExists("CBTRFPostingMovements");
			DBTransformationTestHelper.DropFunctionIfExists("AccrualReversingMovements");
			DBTransformationTestHelper.DropFunctionIfExists("WIPPostingMovements");
			DBTransformationTestHelper.DropFunctionIfExists("WIPReversingMovements");
			DBTransformationTestHelper.DropFunctionIfExists("PeriodEndExchangeRate");
			DBTransformationTestHelper.DropFunctionIfExists("JCJNLJRJPostingMovements");
			DBTransformationTestHelper.DropFunctionIfExists("JCJNLJRJLinesReversingMovements");
			DBTransformationTestHelper.DropFunctionIfExists("APINVCRDADJPostingMovements");
			DBTransformationTestHelper.DropFunctionIfExists("ARINVCRDADJPostingMovements");
			DBTransformationTestHelper.DropFunctionIfExists("APINVCRDADJQueueMovements");
			DBTransformationTestHelper.DropFunctionIfExists("Report_CashFlowModifiedGLAccount");
			DBTransformationTestHelper.DropFunctionIfExists("ARINVCRDADJQueueMovements");
			DBTransformationTestHelper.DropFunctionIfExists("ARAPINVCRDADJLinesReversingMovements");
			DBTransformationTestHelper.DropFunctionIfExists("GetComplianceReportLines_ADH");
			DBTransformationTestHelper.DropViewIfExists("ViewComplianceReportLine");
			DBTransformationTestHelper.DropFunctionIfExists("ARAPAgedSummaryByOrganistation");
			DBTransformationTestHelper.DropFunctionIfExists("NetARAPAgedSummary");
			DBTransformationTestHelper.DropFunctionIfExists("NetARAPAgedSummaryByOrganistation");
			DBTransformationTestHelper.DropFunctionIfExists("NetAPAgedBase");
			DBTransformationTestHelper.DropFunctionIfExists("NetARAgedBase");
			DBTransformationTestHelper.DropFunctionIfExists("AddPeriod");
			DBTransformationTestHelper.DropFunctionIfExists("Report_PostedTransactionLineListingByPrincipal");
			DBTransformationTestHelper.DropFunctionIfExists("csfn_TransactionBalancesByOrgWithAgeing");
			DBTransformationTestHelper.DropFunctionIfExists("csfn_TransactionsBalances");
			DBTransformationTestHelper.DropFunctionIfExists("GetGLAggregate");
			DBTransformationTestHelper.DropFunctionIfExists("Report_OutstandingTransactionsByChargeCode");
			DBTransformationTestHelper.DropFunctionIfExists("ShippingOutstandingInvoiceAndCreditNote");
			DBTransformationTestHelper.DropFunctionIfExists("ShippingOutstandingInvoiceAndCreditNoteDetails");
			DBTransformationTestHelper.DropFunctionIfExists("CalculateDatesForAgeing");
			DBTransformationTestHelper.DropFunctionIfExists("Report_OutstandingAPBalancesCurrencySummary");
			DBTransformationTestHelper.DropFunctionIfExists("csfn_APTransactionsDetail");
			DBTransformationTestHelper.DropFunctionIfExists("Report_OutstandingBalancesCurrencySummary");
			DBTransformationTestHelper.DropFunctionIfExists("csfn_ARTransactionsDetail");
			DBTransformationTestHelper.DropFunctionIfExists("Report_ARAPTransactionsSummaryByOrganizationAndCountry");
		}

		Guid companyPK;
		Guid periodPK_Vaild;
		Guid periodPK_Duplicate_oldPeriod;
		Guid periodPK_Duplicate_newPeriod;
		Guid periodPK_Duplicate_oldPeriod1;
		Guid periodPK_Duplicate_newPeriod1;
		Guid periodPK_VaildDAU;
		Guid periodPK_Duplicate_oldPeriodDAU;
		Guid periodPK_Duplicate_newPeriodDAU;
		Guid periodPK_Duplicate_oldPeriodDAU1;
		Guid periodPK_Duplicate_newPeriodDAU1;

		readonly string tableName = AccPeriodManagementSchema.Constants.TableName;
		readonly TestDbHelper testDbHelper;
	}
}
