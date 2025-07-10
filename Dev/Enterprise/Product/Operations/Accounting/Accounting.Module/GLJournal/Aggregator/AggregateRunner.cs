using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Accounting.Module.Res;

namespace Enterprise.Accounting.Aggregator
{
	#region Progress Bar related Event Args

	public class AggregateEventArg : EventArgs
	{
		public int PercentageCompletetd;

		public AggregateEventArg(int percentageCompletetd) : base()
		{
			this.PercentageCompletetd = percentageCompletetd;
		}
	}

	#endregion

	public partial class AggregateRunner : IAggregateRunner
	{
		public event EventHandler ProcessingProgressed;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this is just the name of a virtual table")]
		public const string TempAggregate = "#TempAggregate";
		public const int TimeOutLimit = 7200;

		public AggregateRunner()
		{
			TotalStages = 1;

			MainAggregator = new BatchAggregator();

			TotalStages = 1;
			CompanyList = GetCompanyList();
		}

		public bool Aggregate()
		{
			bool result = false;
			FailedWithDeadlock = false;

			using (var mutex = new ZGlobalMutex(MutexIDs.BatchAggregtorRunning, Env.CurrentCompany.PK.ToString()))
			{
				if (!mutex.Lock())
				{
					HandleUnableToLockMutex(mutex);
				}
				else
				{
					result = MainAggregator.Aggregate();
					AggregateResult = MainAggregator.AggregateResult;

					if (!result)
					{
						var lastException = MainAggregator.LastException;
						if (lastException != null && lastException is System.Data.Common.DbException && new DbErrorMatch(lastException as System.Data.Common.DbException).ExceptionType == DbErrorType.DeadlockError)
						{
							FailedWithDeadlock = true;
						}
					}
				}
			}

			return result;
		}

		public bool FailedWithDeadlock { get; private set; }

		#region Public Reaggregation Utility Methods

		public bool ReAggregate()
		{
			return ReAggregateBody(new ProcessResult(ProcessAndReport));
		}

		public bool ReAggregateAndReportAsXML()
		{
			string fileName = Env.TempPath + "\\" + "CountLog.txt";
			LogFile = new StreamWriter(fileName);
			bool result = ReAggregateBody(new ProcessResult(ProcessAndReportAsXML));
			LogFile.Close();
			return result;
		}

		#endregion

		public string AggregateResult
		{
			get { return fAggregateResult; }

			set { fAggregateResult = value; }
		}

		#region Implementation

		protected IBatchAggregator MainAggregator;
		protected decimal TotalStages;
		protected decimal StepsProgressed;
		protected GlbCompany[] CompanyList;
		protected int fPeriodToExclude;
		protected int fReversePeriod;
		protected StreamWriter LogFile;

		protected bool ReAggregateBody(ProcessResult processResult)
		{
			bool isSucceeded = false;
			using (new AccountingUtils.CommandTimeoutInitializer(TimeOutLimit))
			{
				try
				{
					DropTempTable();

					foreach (GlbCompany company in CompanyList)
					{
						MainAggregator = new BatchAggregator();
						Aggregate();
					}

					DumpAggregateData();

					var reAggregator = ObjectFactory.New<IReAggregator>();
					reAggregator.ReAggregate();

					foreach (GlbCompany company in CompanyList)
					{
						MainAggregator = new BatchAggregator();
						Aggregate();
						processResult(company);
					}

					isSucceeded = true;
				}
				finally
				{
					DropTempTable();
				}
			}
			return isSucceeded;
		}

		protected void AggrgeateBeforeCleanup()
		{
			foreach (GlbCompany company in CompanyList)
			{
				MainAggregator = new BatchAggregator();

				Aggregate();
			}
		}

		protected delegate void ProcessResult(GlbCompany company);

		#region Delegated Utility Methods

		protected void ProcessAndReportAsXML(GlbCompany company)
		{
			string periodList = GetClauseToExclude(fPeriodToExclude, company);
			WriteDifferenceCountToFile(company, periodList);
			GenerateReportAsXML(company);
		}

		protected void ProcessAndReport(GlbCompany company)
		{
			string result = GenerateReport(company);
			if (result.Length != 0)
			{
				ErrorReporter.ReportOnce(result);
			}
		}

		#endregion

		#region Report Wrapper methods

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		protected void GenerateReportAsXML(GlbCompany company)
		{
			string periodList = GetClauseToExclude(fPeriodToExclude, company);
			DataSet resultData = GetDifferenceResult(company, periodList);

			string fileName = Env.TempPath + "\\" + company.GC_Code + ".xml";

			SaveAsXML(fileName, resultData);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		protected string GenerateReport(GlbCompany company)
		{
			StringBuilder resultBuilder = new StringBuilder();
			string periodList = GetClauseToExclude(fPeriodToExclude, company);
			DataSet resultData = GetDifferenceResult(company, periodList);

			foreach (DataRow row in resultData.Tables[0].Rows)
			{
				resultBuilder.AppendLine(GetRowDetail(row));
			}

			if (resultBuilder.Length != 0)
			{
				resultBuilder.Insert(0, GetErrorMsgHeader(company) + System.Environment.NewLine);
			}
			return resultBuilder.ToString();
		}

		#endregion

		#region Report Util Methods

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is just the error message header")]
		protected string GetErrorMsgHeader(GlbCompany company)
		{
			return company.GC_Name + System.Environment.NewLine + (NoResString)"After Aggregation".PadRight(63) + (NoResString)"Before Aggregation".PadRight(63);
		}

		protected string GetRowDetail(DataRow row)
		{
			string result = "";

			foreach (object columnValue in row.ItemArray)
			{
				result += columnValue.ToString() + " ";
				result = row[0].ToString().PadRight(12) + " " + row[1].ToString().PadRight(3) + " " + row[2].ToString().PadRight(3) + " " + row[3].ToString().PadRight(10) + " " + row[4].ToString().PadRight(30) + " " +
					row[8].ToString().PadRight(12) + " " + row[9].ToString().PadRight(3) + " " + row[10].ToString().PadRight(3) + " " + row[11].ToString().PadRight(10) + " " + row[12].ToString().PadRight(30) + " ";
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		protected void SaveAsXML(string filename, DataSet result)
		{
			//Create the FileStream to write with.
			FileStream fs = new FileStream(filename, FileMode.Create);

			//Create an XmlTextWriter for the FileStream.
			XmlTextWriter xtw = new XmlTextWriter(fs, Encoding.Unicode);

			//Add processing instructions to the beginning of the XML file, one of which indicates a style sheet.
			xtw.WriteProcessingInstruction((NoResString)"xml", (NoResString)"version='1.0'");
			//xtw.WriteProcessingInstruction("xml-stylesheet", "type='text/xsl' href='TrialBalance.xsl'");

			//Write the XML from the dataset to the file.
			result.WriteXml(xtw);
			xtw.Close();
		}

		protected BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}
				return fFactory;
			}
		}
		BusinessObjectFactory fFactory;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected void WriteDifferenceCountToFile(GlbCompany company, string periodList)
		{
			string includeCurrentPeriodCondition = !string.IsNullOrEmpty(periodList) ? $" AND AA_Period NOT IN ( {periodList} )" : string.Empty;
			string sQL = @"Select count(*) From 
							(
							Select AG_AccountNum, GB_Code, GE_Code, AA_Period, sum(AA_Amount) as Total, AG_PK, GB_PK, GE_PK From dbo.accGLaggregate
							Inner Join dbo.AccGLHeader on AA_AG = AG_PK
							Inner Join dbo.GlbBranch on AA_GB = GB_PK
							Inner Join dbo.GlbDepartment on AA_GE = GE_PK
							WHERE GB_GC = '" + company.PK.ToString() + "' " +
				includeCurrentPeriodCondition + @"
							Group By AG_AccountNum, GB_Code, GE_Code, AA_Period, AG_PK, GB_PK, GE_PK
							) NewAggregate
							Full join 
							(
							Select AG_AccountNum, GB_Code, GE_Code, AA_Period, sum(AA_Amount) as Total, AG_PK, GB_PK, GE_PK From " + TempAggregate +
				@" Inner Join dbo.AccGLHeader on AA_AG = AG_PK
							Inner Join dbo.GlbBranch on AA_GB = GB_PK
							Inner Join dbo.GlbDepartment on AA_GE = GE_PK
							WHERE GB_GC = '" + company.PK.ToString() + "' " +
				includeCurrentPeriodCondition + @"
							Group By AG_AccountNum, GB_Code, GE_Code, AA_Period, AG_PK, GB_PK, GE_PK
							) OldAggregate On 
							NewAggregate.AG_AccountNum = OldAggregate.AG_AccountNum and
							NewAggregate.GB_Code = OldAggregate.GB_Code and
							NewAggregate.GE_Code = OldAggregate.GE_Code and
							NewAggregate.AA_Period = OldAggregate.AA_Period " + @"
							WHERE NewAggregate.Total <> OldAggregate.Total
							OR NewAggregate.AG_AccountNum is null
							OR OldAggregate.AG_AccountNum is null ";

			DbCommand command = ((IDbConnected)Factory).Connection.Command(sQL);
			//Command.CommandTimeout = Int32.MaxValue;
			object result = command.ExecuteScalar();
			int count = result == null ? 0 : (int)result;
			LogFile.WriteLine(company.GC_Code + ": " + count.ToString(), true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		protected internal DataSet GetDifferenceResult(GlbCompany company, string periodList)
		{
			string includeCurrentPeriodCondition = !string.IsNullOrEmpty(periodList) ? $" AND AA_Period NOT IN ( {periodList} )" : string.Empty;
			string sQL = @"Select NewAggregate.*, OldAggregate.* From 
							(
							Select AG_AccountNum, GB_Code, GE_Code, AA_Period, sum(AA_Amount) as Total, AG_PK, GB_PK, GE_PK From dbo.accGLaggregate
							Inner Join dbo.AccGLHeader on AA_AG = AG_PK
							Inner Join dbo.GlbBranch on AA_GB = GB_PK
							Inner Join dbo.GlbDepartment on AA_GE = GE_PK
							WHERE GB_GC = '" + company.PK.ToString() + "' " +
				includeCurrentPeriodCondition + @"
							Group By AG_AccountNum, GB_Code, GE_Code, AA_Period, AG_PK, GB_PK, GE_PK
							) NewAggregate
							Full join 
							(
							Select AG_AccountNum, GB_Code, GE_Code, AA_Period, sum(AA_Amount) as Total, AG_PK, GB_PK, GE_PK From " + TempAggregate +
				@" Inner Join dbo.AccGLHeader on AA_AG = AG_PK
							Inner Join dbo.GlbBranch on AA_GB = GB_PK
							Inner Join dbo.GlbDepartment on AA_GE = GE_PK
							WHERE GB_GC = '" + company.PK.ToString() + "' " +
				includeCurrentPeriodCondition + @"
							Group By AG_AccountNum, GB_Code, GE_Code, AA_Period, AG_PK, GB_PK, GE_PK
							) OldAggregate On 
							NewAggregate.AG_AccountNum = OldAggregate.AG_AccountNum and
							NewAggregate.GB_Code = OldAggregate.GB_Code and
							NewAggregate.GE_Code = OldAggregate.GE_Code and
							NewAggregate.AA_Period = OldAggregate.AA_Period " + @"
							WHERE NewAggregate.Total <> OldAggregate.Total
							OR NewAggregate.AG_AccountNum is null
							OR OldAggregate.AG_AccountNum is null " +
				@" Order By NewAggregate.AG_AccountNum, NewAggregate.GB_Code, NewAggregate.GE_Code, NewAggregate.AA_Period,
							OldAggregate.AG_AccountNum, OldAggregate.GB_Code, OldAggregate.GE_Code, OldAggregate.AA_Period
							";

			DataSet resultData = new DataSet();
			using (var dataAdpter = ((IDbConnected)Factory).Connection.Command(sQL).NewDataAdapter())
			{
				dataAdpter.Fill(resultData);
			}

			return resultData;
		}

		#endregion

		#region Temporary Aggregate Table related

		protected internal void DumpAggregateData()
		{
			string sQL = "SELECT " + CargoWise.Schema.Schema.CsvColumnList(AccGLAggregateSchema.Instance) + " INTO " + TempAggregate + " From dbo.AccGLAggregate";
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sQL);
		}

		protected void DropTempTable()
		{
			string sQL = String.Format((NoResString)"IF (object_id('tempdb..{0}') IS NOT NULL) DROP TABLE {0}", TempAggregate);
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sQL);
		}

		protected bool DoesTempTableExist()
		{
			string sQL = String.Format((NoResString)"SELECT object_id('tempdb..{0}')", TempAggregate);
			object tempAggregateId = ((IDbConnected)Factory).Connection.ExecuteScalar(sQL);

			return (tempAggregateId != DBNull.Value);
		}

		#endregion

		#region Company List Related Methods

		protected GlbCompany[] GetCompanyList()
		{
			ZGuid demoCompanyPK = GetDemoCompanyPK();

			ZQuery filter = new ZQuery();
			if (demoCompanyPK.IsValid)
			{
				filter.AddToFilter(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, demoCompanyPK.ToGuid());
			}
			GlbCompany[] companyList = new BusinessObjectFactory().Load(typeof(GlbCompany), filter) as GlbCompany[];

			return companyList;
		}

		protected ZGuid GetDemoCompanyPK()
		{
			return new ZGuid("03052ED3-2C64-49AC-97D8-C6079D5015B5");
			//return ZGuid.Empty;
		}

		#endregion

		#region Period Related Methods

		protected bool IsAggregateOnlyForSinglePeriod(GlbCompany company)
		{
			int[] periods = GetDistinctPeriodsFromAggregate(company);

			return (periods.Length <= 1);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected int[] GetDistinctPeriodsFromAggregate(GlbCompany company)
		{
			Guid companyPK = company.PK.ToGuid();
			string periodListToExclude = GetClauseToExclude(fPeriodToExclude, company);

			ArrayList periods = new ArrayList();
			string sQL = @"SELECT distinct AA_Period
							From dbo.AccGLAggregate
							Inner Join dbo.GlbBranch on AA_GB = GB_PK
							WHERE GB_GC = @CompanyPK " + (!string.IsNullOrEmpty(periodListToExclude) ? $" AND AA_Period NOT IN ( {periodListToExclude} )" : string.Empty);
			DbCommand command = ((IDbConnected)Factory).Connection.Command(sQL);
			command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);

			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					int period = reader.GetInt32(0);
					periods.Add(period);
				}
			}

			return (int[])periods.ToArray(typeof(int));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected bool IsPeriodTotalEqualsZero(Guid companyPK, int period)
		{
			string sQL = @"SELECT sum(aa_amount)
							From dbo.AccGLAggregate
							Inner Join dbo.GlbBranch on AA_GB = GB_PK
							WHERE GB_GC = @CompanyPK
							And aa_period = @Period";
			DbCommand command = ((IDbConnected)Factory).Connection.Command(sQL);
			command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
			command.AddParameter("@Period", SqlDbType.Int, period);

			decimal total = Utilities.ConvertToInt32(command.ExecuteScalar());
			return total == 0m;
		}

		protected int[] GetPeriodListToExclude(int lastPeriodCount, GlbCompany company)
		{
			ArrayList periodListToExclude = new ArrayList();

			if (lastPeriodCount > 0)
			{
				AccountingPeriodCalculator periodCalc = new AccountingPeriodCalculator(new BusinessObjectFactory(),company);
				int currentPeriod = periodCalc.GetPeriodFromDate(Env.Time.CurrentLocalDateTime, company.PK);
				periodListToExclude.Add(currentPeriod);
				for (int i = 0; i < lastPeriodCount - 1; i++)
				{
					currentPeriod = periodCalc.GetPreviousPeriod(currentPeriod);
					periodListToExclude.Add(currentPeriod);
				}
			}
			return periodListToExclude.ToArray(typeof(int)) as int[];
		}

		protected string GetClauseToExclude(int lastPeriodCount, GlbCompany company)
		{
			string periodCount = "";
			if (lastPeriodCount > 0)
			{
				int[] periodToExcludeList = GetPeriodListToExclude(lastPeriodCount, company);
				for (int i = 0; i < periodToExcludeList.Length; i++)
				{
					int currentPeriod = periodToExcludeList[i];
					periodCount += (string.IsNullOrEmpty(periodCount) ? string.Empty : ",") + currentPeriod.ToString();
				}
			}
			return periodCount;
		}

		#endregion

		#region Validation Before Correction

		protected bool ValidatePeriodTotal(Guid companyPK, int[] periodList)
		{
			foreach (int period in periodList)
			{
				if (!IsPeriodTotalEqualsZero(companyPK, period))
				{
					return false;
				}
			}

			return true;
		}

		protected bool ValidateAggregateBeforeRunning()
		{
			bool periodTotalNotEqualToZero = true;
			bool allCompanyAggregateAreForOnePeriod = true;
			bool validateResult = true;

			foreach (GlbCompany company in CompanyList)
			{
				int[] periodList = GetDistinctPeriodsFromAggregate(company);

				if (periodList.Length > 1)
				{
					allCompanyAggregateAreForOnePeriod = false;
				}

				if (!ValidatePeriodTotal(company.PK.ToGuid(), periodList))
				{
					periodTotalNotEqualToZero = false;
					break;
				}
			}

			if (!periodTotalNotEqualToZero)
			{
				Globals.Message.Show(Res.GetString("3eafe203-08b8-4f4e-915f-84e492ff8426", "Aggregate for the period total is not zero."));
				validateResult = false;
			}

			if (allCompanyAggregateAreForOnePeriod)
			{
				Globals.Message.Show(Res.GetString("3e26a81c-4850-4f47-84a6-bb446fde1436", "All Aggregate Records are for single period.") + " ");
				validateResult = false;
			}
			return validateResult;
		}

		#endregion

		#region Mutex Related

		protected string fAggregateResult;

		bool fFailedToAquireMutex;
		string fFailedToAquireMutexReason;

		public bool FailedToAquireMutex
		{
			get { return fFailedToAquireMutex; }
		}

		public string FailedToAquireMutexReason
		{
			get { return fFailedToAquireMutexReason; }
		}

		void HandleUnableToLockMutex(ZGlobalMutex mutex)
		{
#if DEBUG
			if (ReleaseLock_ForTest != null)
			{
				ReleaseLock_ForTest.Invoke();
			}
#endif

			var lockInfo = mutex.GetLockInfo();
			var who = lockInfo != null && lockInfo.UserWithLock != null ? lockInfo.UserWithLock.GS_FullName.ToString() : Res.GetString("89ec9f55-2b84-4cbf-9515-ef854335791e", "*unknown user*");
			var when = lockInfo != null ? lockInfo.LockStartTime.ToDateTime().ToLocalTime().ToLongTimeString() : Res.GetString("09e433a3-1def-4494-aea8-29f740521364", "*unknown time*");

			AggregateResult += Res.GetString("9dc8c9ca-69f9-43be-af8b-0d088ba07fc0", "GL Account update has been canceled because a GL Account update is currently being run by user '{0}' since {1}", who, when);
			fFailedToAquireMutexReason = Res.GetString("2e7d0802-6d45-4622-9001-8152eb2d77fc", "GL Account update is currently being run by user '{0}' since {1}", who, when);
			fFailedToAquireMutex = true;
		}

#if DEBUG
		internal Action ReleaseLock_ForTest;
#endif

		#endregion

		void Aggregator_ProcessingProgressed(object sender, EventArgs e)
		{
			StepsProgressed++;
			if (ProcessingProgressed != null)
			{
				int progress = (int)(StepsProgressed / TotalStages * 100m);
				ProcessingProgressed(this, new AggregateEventArg(progress));
			}
		}

		#endregion
	}
}
