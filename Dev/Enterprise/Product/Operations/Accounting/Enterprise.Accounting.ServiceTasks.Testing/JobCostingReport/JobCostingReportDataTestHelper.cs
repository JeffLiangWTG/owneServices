using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	public class JobCostingReportDataTestHelper
	{
		public JobCostingReportDataTestHelper(TestObjectCreator objectCreator)
		{
			ObjectCreator = objectCreator;
		}
		readonly TestObjectCreator ObjectCreator;

		public Tuple<Guid, DateTime, DateTime, Guid> CreateTuppleForAssertion(AccTransactionLines line, bool setPostdateToNull, bool setReverseDateToNull)
		{
			return Tuple.Create(line.PK.ToGuid(), (line.AL_PostDate.IsValid && !setPostdateToNull) ? line.AL_PostDate.ToDateTime() : DateTime.MinValue, (line.AL_ReverseDate.IsValid && !setReverseDateToNull) ? line.AL_ReverseDate.ToDateTime() : DateTime.MinValue, line.AL_GC.ToGuid());
		}

		#region Transaction Line

		public APInvoiceLine CreateCSTLine(Job job, int jobNumber, ZDecimal amount, bool reverse)
		{
			APInvoiceLine apLine = null;

			using (SetupRevenueRecognition(job, reverse ? RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate : RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure))
			{
				if (reverse)
				{
					job.JH_A_JOP = ZDateTime.Today.AddDays(2);
				}

				var apTransaction = ObjectCreator.CreateAPInvoice<APInvoice>("AP0000" + jobNumber.ToString(), ObjectCreator.AUD, 1.0M, 150M + jobNumber, 0M, 0M, 150M + jobNumber, 0M, 0M, ObjectCreator.Creditor1);
				apTransaction.Lines.RemoveAndDeleteAll();
				apLine = ObjectCreator.CreateAPInvoiceLine(apTransaction, job, ObjectCreator.FRT, ObjectCreator.AUD, 1.0M, "AP Line Description" + jobNumber.ToString(), amount);
				var charge = ObjectCreator.CreateJobCharge(apLine, job, ObjectCreator.FRT, ObjectCreator.AUD);
				charge.JR_OSSellAmt = 0M;
				Factory.Save();
			}

			return apLine;
		}

		public ARInvoiceLine CreateREVLine(Job job, int jobNumber, ZDecimal amount, bool reverse, ZDateTime? reverseDate = null)
		{
			ARInvoiceLine arLine = null;

			using (SetupRevenueRecognition(job, reverse ? RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate : RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure))
			{
				if (reverse)
				{
					job.JH_A_JOP = reverseDate ?? ZDateTime.Today.AddDays(2);
				}

				var arTransaction = ObjectCreator.CreateARInvoice<ARInvoice>("AR0000" + jobNumber.ToString(), ObjectCreator.AUD, 1.0M, ObjectCreator.Debtor);
				arLine = ObjectCreator.CreateARInvoiceLine(arTransaction, job, ObjectCreator.CC1, ObjectCreator.AUD, 1.0M, "AR Line Description" + jobNumber.ToString(), amount);
				var charge = ObjectCreator.CreateJobCharge(arLine, job, ObjectCreator.CC1, ObjectCreator.AUD);
				charge.JR_OSCostAmt = 0M;
				Factory.Save();
			}

			return arLine;
		}

		public Tuple<Business.WIPAccrual.WIP, Business.WIPAccrual.Accrual> CreateACRWIPLine(Job job, int jobNumber, ZDecimal acrAmount, ZDecimal wipAmount, bool reverseWIP, bool reverseACR)
		{
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			var jobCharge = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, acrAmount, wipAmount);
			jobCharge.JR_OH_SellAccount = ObjectCreator.Debtor.PK;
			jobCharge.JR_OH_CostAccount = ObjectCreator.Creditor1.PK;
			Factory.Save();

			var wip = jobCharge.WIP;
			var acr = jobCharge.Accrual;

			if (reverseWIP)
			{
				ReverseWipAcr(wip);
			}

			if (reverseACR)
			{
				ReverseWipAcr(acr);
			}

			return Tuple.Create(wip, acr);
		}

		public void ReverseWipAcr(Business.WIPAccrual.BaseWIPAccrual wipAcr)
		{
			wipAcr.Reverse();
			Factory.Save();
		}

		public void Recognize(Job job)
		{
			job.Close(null, null);
			Factory.Save();
		}

		public Job[] CreateJobsWithPeriod(bool createNonJobCostingLine = true)
		{
			var jobs = new Job[2];

			CreatePeriods();

			//Creating Jobs
			for (int i = 0; i <= 1; i++)
			{
				var job = ObjectCreator.CreateJob("J0000" + i.ToString(), ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
				jobs[i] = job;
				if (i == 1 && createNonJobCostingLine)
				{
					CreateNonJCLine(job);
				}
			}

			return jobs;
		}

		public Job[] CreateJobs()
		{
			var jobs = new Job[2];
			//Creating Jobs
			for (int i = 0; i <= 1; i++)
			{
				var job = ObjectCreator.CreateJob("J0000" + i.ToString(), ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
				jobs[i] = job;
				if (i == 1)
				{
					CreateNonJCLine(job);
				}
			}

			return jobs;
		}

		public List<string> CreatePeriods()
		{
			var helper = new AccountingPeriodTestHelper(Factory);
			var companyPKs = new List<ZGuid>();

			//Creating Periods
			foreach (GlbCompany company in GlbCompany.GetActiveCompanies())
			{
				companyPKs.Add(company.PK);
				helper.PostPeriodsForEntireYear(ZDateTime.Today.Year - 1, company.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
				helper.PostPeriodsForEntireYear(ZDateTime.Today.Year, company.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
				helper.PostPeriodsForEntireYear(ZDateTime.Today.Year + 1, company.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			}
			Factory.Save();

			var periods = Factory.Load<AccPeriodManagement>(new ZQuery(AccPeriodManagementSchema.AM_GC_Company, companyPKs.ToArray()));
			var partitionKeys = new List<string>();
			foreach (AccPeriodManagement period in periods)
			{
				partitionKeys.Add(string.Format("{0}{1}", period.AM_Period, period.AM_GC_Company));
			}
			return partitionKeys;
		}

		public DisposableAction SetTemporaryCurrentDate(ZDateTime temporaryCurrentDate)
		{
			var currentDate = TestDateAttribute.Date;
			var createAction = new Action(() => { TestDateAttribute.Date = temporaryCurrentDate.ToDateTime(); });
			var disposeAction = new Action(() => { TestDateAttribute.Date = currentDate; });
			return new DisposableAction(createAction, disposeAction);
		}

		IDisposable SetupRevenueRecognition(Job job, ZString recognitionDateOptionCode)
		{
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			var revenueRecognitionByChargeGroupCollection = new RevenueRecognitionCollection();
			var revenueRecognition = revenueRecognitionByChargeGroupCollection.AddNew();
			revenueRecognition.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			revenueRecognition.RecognitionDateOptionCode = recognitionDateOptionCode;
			revenueRecognition.Offset = 0;

			return AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetTemporaryValue(Guid.Empty,
				Guid.Empty, Guid.Empty, revenueRecognitionByChargeGroupCollection);
		}

		public void CreateNonJCLine(Job job)
		{
			var jobCharge = ObjectCreator.CreateCharge(job, ObjectCreator.CommentChargeCode, 0, 0);
			jobCharge.JR_OH_SellAccount = ObjectCreator.Debtor.PK;
			jobCharge.JR_OH_CostAccount = ObjectCreator.Creditor1.PK;

			var arTransaction = ObjectCreator.CreateARInvoice<ARInvoice>("NOJCAR", ObjectCreator.AUD, 1.0M, ObjectCreator.Debtor);
			narline = ObjectCreator.CreateInvoiceLine(arTransaction, ObjectCreator.AUD, 1.0M, 490M, 49M, ObjectCreator.GLHeader1.PK);
			Factory.Save();
		}

		#endregion

		#region Tables and Trigger

		public void DropJobCostingDataTables()
		{
			var sql = @"IF OBJECT_ID(N'RptDtJobCostingData', N'U') IS NOT NULL
									 BEGIN
										DROP TABLE RptDtJobCostingData
									 END";

			using (var cmd = Connection.Command(sql))
			{
				cmd.CommandType = CommandType.Text;
				cmd.ExecuteNonQuery();
			}
		}

		public int GetStartingPeriodFromPartitionKey()
		{
			return Connection.ExecuteScalar<int>(@"SELECT MIN(Period)
FROM
(
	SELECT	CAST(SUBSTRING(CAST(value as char(42)), 1, 6) AS INT) as Period, CAST(SUBSTRING(CAST(value as char(42)), 7, 36) as uniqueidentifier) as GC_PK
	FROM	sys.partition_functions AS pf
			INNER JOIN sys.partition_range_values AS prv ON prv.function_id = pf.function_id
	WHERE	pf.name = 'PF_AccountingPeriodCompany'
)t");
		}

		#endregion

		public DataTable LoadDataFromReportTable()
		{
			var mainTable = DataUtils.GetDataTableFromQuery(Connection, @"SELECT	JCD_AL,
																					JCD_AH,
																					JCD_JH,
																					JCD_AC,
																					JCD_AG,
																					JCD_OH,
																					JCD_GE,
																					JCD_GB,
																					JCD_GC,
																					JCD_LineType,
																					JCD_Desc,	
																					JCD_PostDate,
																					JCD_PostPeriod,
																					JCD_RevRecognitionType,
																					JCD_LineAmount,
																					JCD_GSTVAT,
																					JCD_RX_NKLocalCurrency,
																					JCD_OSAmount,
																					JCD_RX_NKCurrency,
																					JCD_TransactionNum,
																					JCD_TransactionType,
																					JCD_Ledger,
																					JCD_ParentID,
																					JCD_ParentTableCode,
																					JCD_PeriodCompanyKey
																			FROM	RptDtJobCostingData");
			return mainTable;
		}

		public DataTable LoadDataFromRptDtUnprocessedAccTransactionLinesTable()
		{
			var tempTable = DataUtils.GetDataTableFromQuery(Connection, @"SELECT UL_RowNumber, UL_ALPK FROM RptDtUnprocessedAccTransactionLines Order by UL_RowNumber");
			return tempTable;
		}

		public DataTable LoadDataFromReportAmountByJobTable()
		{
			var table = DataUtils.GetDataTableFromQuery(Connection, @"
SELECT
	CONCAT(
		CONVERT(varchar(6), JCA_PostPeriod), '|',
		ISNULL(GC_Code, ''), '|',
		ISNULL(OH_Code, ''), '|',
		ISNULL(JH_JobNum, ''), '|',
		ISNULL(JCA_RX_NKLocalCurrency, '')
	) AS UniqueId,
	JCA_JH,
	JCA_GC,
	JCA_OH_DebtorOrCreditor,
	JCA_PostPeriod,
	JCA_RX_NKLocalCurrency,
	JCA_Revenue,
	JCA_Cost,
	JCA_RowCount
FROM dbo.RptDtJobCostingDataAmountByJob
LEFT JOIN dbo.GlbCompany ON JCA_GC = GC_PK
LEFT JOIN dbo.OrgHeader ON JCA_OH_DebtorOrCreditor = OH_PK
LEFT JOIN dbo.JobHeader ON JCA_JH = JH_PK
");
			return table;
		}

		public DbConnection Connection
		{
			get { return ((IDbConnected)Factory).Connection; }
		}

		BusinessObjectFactory Factory
		{
			get { return ObjectCreator.Factory; }
		}

		public AccTransactionLines narline;
	}
}
