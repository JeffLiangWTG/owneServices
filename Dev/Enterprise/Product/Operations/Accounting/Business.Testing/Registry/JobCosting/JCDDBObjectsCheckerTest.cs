using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Testing.Registry.JobCosting
{
	class JCDDBObjectsCheckerTest : TestCaseWithFactory
	{
		public void TestJCDDBObjectList()
		{
			var expectedDBObjectInfo = new string[]
			{
				"PeriodEndExchangeRate-IF-Permanent-Live-1-S",
				"CreateJobCostingDataTableAndCCI-P-Permanent-Live-2-S",
				"SwitchNonEmptyPartitionFromJobCostingDataTable-P-Permanent-Live-3-S",
				"CreatePeriodCompanyPartitionKeys-P-Permanent-Live-4-S",
				"JobCostingDataQueue-U-Permanent-Live-5-S",
				"NR_RC__JCQ_HasNoPeriod_JCQ_RowNumber-I-Permanent-Live-6-S",

				"RptDtUnprocessedAccTransactionLines-U-Temporary-Live-7-J",
				"NR_RC__Clustered_UL_RowNumber-I-Temporary-Live-8-J",
				"RptDtUnprocessedReversedAL-U-Temporary-Live-9-J",
				"FK_UC__Clustered_URL_ALPK-I-Temporary-Live-10-J",
				"RptDt_TG_AccTransactionLines_InsertToReversedLinesTable-T-Temporary-Live-11-J",
				"RptDt_TG_AccTransactionLines_InsertToJobCostingDataQueue-T-Permanent-Live-12-J",
				"RptDtTransformAccTransactionLineToJobCostingQueueRecord-P-Temporary-Live-13-J",

				"PF_AccountingPeriodCompany-PF-Permanent-Live-20-J",
				"PS_AccountingPeriodCompany-PS-Permanent-Live-21-J",
				"RptDtJobCostingData-U-Permanent-Live-22-J",
				"CI_JCD_PostDate-I-Permanent-Live-23-J",
				"NCI_JCD_OH-I-Permanent-Live-24-J",

				"RptDtJobCostingDataAmountByJob-DU-Permanent-Live-30-J",
				"RptDt_UC_JCA_PostPeriod_JCA_GC_JCA_OH_JCA_JH-I-Permanent-Obsolete-31-J",
				"RptDt_UC_JCA_PostPeriod_JCA_GC_JCA_OH_JCA_JH_JCA_RX_NKLocalCurrency-I-Permanent-Live-31-J",
				"PopulateJobCostingDataAmountByJob-IN-Permanent-Live-32-J",
				"RptDt_NX_JCA_JH_JCA_GC_JCA_OH-I-Permanent-Live-33-J",
				"RptDt_NX_JCA_OH_JCA_GC_JCA_JH-I-Permanent-Live-34-J",

				"RptDtPopulateJobCostingDataFromQueue-P-Permanent-Live-40-J",

				"RptDt_GetPeriodKeys-TF-Permanent-Live-50-J",
				"RptDt_GlobalJobProfitReportCore-IF-Permanent-Live-51-J",
				"RptDt_Report_GlobalJobProfitSummaryByJob-IF-Permanent-Live-52-J",
				"RptDt_Report_GlobalForwardingAndCustomsSummary-IF-Permanent-Live-53-J",

				"RptDt_ViewJobCostingDataAmountByJob-V-Permanent-Obsolete-60-J",
				"RptDt_CI_JCD_PostPeriod_JCD_GC_JCD_OH_JCD_JH-I-Permanent-Obsolete-61-J",
				"RptDt_NI_JCD_JH_JCD_GC_JCD_OH-I-Permanent-Obsolete-62-J",
				"RptDt_NI_JCD_OH_JCD_GC_JCD_JH-I-Permanent-Obsolete-63-J",
			};

			var dbObjectInfo = JCDDBObjectInfoList.Instance.OrderBy(x => x.DependencySequence).Select(x => x.ToString());
			AssertContainsExactElementsInAnyOrder("JCD DB Object Info", expectedDBObjectInfo, dbObjectInfo);
		}

		public void TestAllTableTriggerFunctionAndProcedureNameStartWithRptDt()
		{
			var dbObjectInfo = JCDDBObjectInfoList.Instance.Where(x => new string[] { "U", "T", "IF", "P" }.Contains(x.Type) && x.CreatedBy == "J").ToList();
			dbObjectInfo.ForEach(x => AssertStartsWith("Name must start with RptDt", "RptDt", x.Name));
		}

		//More unit tests are available at 
		//...\Enterprise\Product\Operations\Accounting\Enterprise.Accounting.ServiceTasks.Testing\JobCostingReport\Registry\JCDDBObjectFunctionsTest.cs
		//As these unit tests require references to classes that belong to Enterprise.Accounting.ServiceTasks project, I couldn't place those tests here. 
	}
}
