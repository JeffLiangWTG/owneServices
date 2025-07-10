using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.CompactGLAggragate;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CompactGLAggragateServiceTask.Code,
	"Compact General Ledger Aggregate Service Task",
	"ACC",
	typeof(CompactGLAggragateServiceTask),
	DefaultScheduleRunEvery = "7days",
	IsMandatory = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1hour",
	ActiveByDefault = true)
]

namespace Enterprise.Accounting.ServiceTasks.CompactGLAggragate
{
	public class CompactGLAggragateServiceTask : ServiceProviderImpl
	{
		public const string Code = "CGA";
		TimeSpan CompactionTimeout { get => TimeSpan.FromHours(1); }

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			var stopwatch = ObjectFactory.Get<IStopwatch>();
			stopwatch.Start();

			var factory = new BusinessObjectFactory();
			var connection = Db.Connection;

			var runDurationMS = TimeSpan.FromHours((double)AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateRunDuration.Value).TotalMilliseconds;
			var (lastCompletedCompany, lastCompletedPeriod) = GetRegistryCompanyPeriod();
			var threshold = (double)AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateThreshold.Value;

			var skippedCompanyPeriods = 0;
			var compactedCompanyPeriods = 0;
			var compacted = 0L;
			var averageCompactingTimeMS = 0;

			foreach (var (company, period, companyCode) in GetRingOfCompanyPeriods(FetchCompanyPeriods(factory), lastCompletedCompany, lastCompletedPeriod))
			{
				try
				{
					var (compactedRatio, numCompacted) = FetchCompactedRatioTotal(factory, company, period, companyCode);
					if (compactedRatio >= threshold)
					{
						var compactWatch = ObjectFactory.Get<IStopwatch>();

						compactWatch.Start();
						CompactCompanyPeriod(connection, company, period);
						compactWatch.Stop();

						ServiceLogger.Log(LogType.Debug, $"Compacting took {compactWatch.ElapsedMilliseconds}ms");

						compactedCompanyPeriods++;
						compacted += numCompacted;

						averageCompactingTimeMS += ((int)compactWatch.ElapsedMilliseconds - averageCompactingTimeMS) / compactedCompanyPeriods;

						if (youMustReactToThisToken.IsCancellationRequested)
						{
							SetRegistryCompanyPeriod(company, period);
							youMustReactToThisToken.ThrowIfCancellationRequested();
						}
					}
					else
					{
						skippedCompanyPeriods++;
					}

					lastCompletedCompany = company;
					lastCompletedPeriod = period;
				}
				catch (System.Data.Common.DbException ex)
				{
					if (new SqlExceptionWrapper(ex).Number == -2)
					{
						ServiceLogger.Log(LogType.Warning, $"SQL timeout occured after {stopwatch.ElapsedMilliseconds}ms");
						break;
					}
				}

				if (stopwatch.ElapsedMilliseconds >= runDurationMS)
				{
					ServiceLogger.Log(LogType.Warning, $"Exit due to time out ({runDurationMS}ms). If the duration is too small, please check registry {AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateRunDuration.HumanReadableRegistryPath()}");
					break;
				}
			}

			SetRegistryCompanyPeriod(lastCompletedCompany, lastCompletedPeriod);

			stopwatch.Stop();

			var informationLog = $@"{compacted} records compacted over {compactedCompanyPeriods} company/periods
Company/periods skipped: {skippedCompanyPeriods}";
			ServiceLogger.Log(LogType.Information, informationLog);

			var debugLog = $@"Service task finished in {stopwatch.ElapsedMilliseconds}ms
Average compacting time: {averageCompactingTimeMS}ms";
			ServiceLogger.Log(LogType.Debug, debugLog);
		}

		(Guid company, int period) GetRegistryCompanyPeriod()
		{
			var company = Guid.Empty;
			var period = 0;

			var rawString = AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateLastCompanyPeriod.Value;
			if (rawString != null)
			{
				var splits = rawString.Split('|');
				if (splits.Length == 2)
				{
					_ = Guid.TryParse(splits[0], out company);
					_ = int.TryParse(splits[1], out period);
				}
			}

			return (company, period);
		}

		void SetRegistryCompanyPeriod(Guid company, int period)
		{
			AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateLastCompanyPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, $"{company}|{period}");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Direct Stored Procedure Call")]
		protected virtual void CompactCompanyPeriod(DbConnection connection, Guid companyPK, int period)
		{
			using (var transactionManager = connection.BeginTransactionWithManager())
			{
				using (var cmd = connection.Command("EXEC Compact_AccGLAggregate @Company, @Period"))
				{
					cmd.CommandTimeout = (int)CompactionTimeout.TotalSeconds;
					cmd.AddParameter("@Company", SqlDbType.UniqueIdentifier, companyPK);
					cmd.AddParameter("@Period", SqlDbType.Int, period);

					cmd.ExecuteNonQuery();
				}
				transactionManager.CommitTransaction();
			}
		}

		(double ratio, int totalCompacted) FetchCompactedRatioTotal(BusinessObjectFactory factory, ZGuid companyPk, int lastPeriod, string companyCode)
		{
			var details = new DynamicBusinessObjectCollection(factory);
			var sql = $@"
WITH Res AS
(
SELECT Count(1) AS Total
FROM AccGLAggregate
WHERE
	AA_GC = @companyPk
	AND AA_Period = @period
GROUP BY AA_TransactionCategory, AA_Period, AA_AG, AA_GB, AA_GE, AA_GC
)
SELECT Count(1) AS CompactedRecords, COALESCE(SUM(Total), 0) AS TotalRecords FROM Res
";
			var company = ZSqlParameter.New("@companyPk", companyPk, AccGLAggregateSchema.AA_GC);
			var period = ZSqlParameter.New((NoResString)"@period", lastPeriod, AccGLAggregateSchema.AA_Period);
			var stopwatch = ObjectFactory.Get<IStopwatch>();
			stopwatch.Start();
			details.Load(sql, new ZSqlParameter[] { company, period });
			stopwatch.Stop();

			var ratio = 0d;
			var totalCompacted = 0;

			if (details.Count > 0)
			{
				var compactedRecords = int.Parse(details[0]["CompactedRecords"].ToString());
				var totalRecords = int.Parse(details[0]["TotalRecords"].ToString());

				if (totalRecords != 0)
				{
					totalCompacted = totalRecords - compactedRecords;
					ratio = totalCompacted / (double)totalRecords;
					ServiceLogger.Log(LogType.Debug, $@"Compacting company: {companyPk} {companyCode}, period: {lastPeriod}
Calculation of compacting took {stopwatch.ElapsedMilliseconds}ms
Total records: {totalRecords}, Compacted records: {compactedRecords}, Ratio: {ratio}
"); 
				}
			}

			return (ratio, totalCompacted);
		}

		List<(Guid company, int period, string companyCode)> FetchCompanyPeriods(BusinessObjectFactory factory)
		{
			var result = new DynamicBusinessObjectCollection(factory);

			var sql = $@"
SELECT {AccPeriodManagementSchema.AM_GC_Company.Name}, {AccPeriodManagementSchema.AM_Period.Name}, {GlbCompanySchema.GC_Code.Name}
FROM {AccPeriodManagementSchema.Constants.TableName}
JOIN {GlbCompanySchema.Constants.TableName} ON {GlbCompanySchema.PK.Name} = {AccPeriodManagementSchema.AM_GC_Company.Name}
ORDER BY {GlbCompanySchema.GC_Code.Name}, {AccPeriodManagementSchema.AM_Period.Name}";

			var stopwatch = ObjectFactory.Get<IStopwatch>();
			stopwatch.Start();
			result.Load(sql);
			stopwatch.Stop();

			ServiceLogger.Log(LogType.Debug, $"Generating ring took {stopwatch.ElapsedMilliseconds}ms"); 

			var companyPeriods = result.Select(r => (
				Guid.Parse(r[AccPeriodManagementSchema.AM_GC_Company.Name].ToString()),
				int.Parse(r[AccPeriodManagementSchema.AM_Period.Name].ToString()),
				r[GlbCompanySchema.GC_Code.Name].ToString())).ToList();

			return companyPeriods;
		}

		IEnumerable<(Guid company, int period, string companyCode)> GetRingOfCompanyPeriods(
			List<(Guid company, int period, string companyCode)> companyPeriods,
			Guid lastCompletedCompany, int lastCompletedPeriod)
		{
			if (companyPeriods.Count > 0)
			{
				var lastIndex = (lastCompletedCompany != Guid.Empty && lastCompletedPeriod != 0) ?
					companyPeriods.FindIndex(a => a.company == lastCompletedCompany
					&& a.period == lastCompletedPeriod) : -1;
				if (lastIndex == -1)
				{
					lastIndex = companyPeriods.Count - 1;
				}

				var index = lastIndex;

				do
				{
					++index;
					if (index >= companyPeriods.Count)
					{
						index = 0;
					}

					yield return companyPeriods[index];
				} while (index != lastIndex);
			}
		}
	}
}
