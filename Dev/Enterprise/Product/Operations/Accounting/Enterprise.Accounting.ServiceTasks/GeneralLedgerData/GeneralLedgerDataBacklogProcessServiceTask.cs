using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	GeneralLedgerDataBacklogProcessServiceTask.Code,
	"General Ledger Data Backlog Process Service Task",
	"ACC",
	typeof(GeneralLedgerDataBacklogProcessServiceTask),
	ActiveByDefault = true,
	IsMandatory = true,
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "6hour",
	DefaultScheduleRunEvery = "6hours")
]
namespace Enterprise.Accounting.ServiceTasks
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "<Pending>")]
	public class GeneralLedgerDataBacklogProcessServiceTask : ServiceProviderImpl
	{
		public const string Code = "GLP";

		bool shouldNudgeGLQ;

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			ServiceLogger.Debug($"General Ledger Data Backlog Process Start.");

			if (!AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.Value)
			{
				return;
			}

			try
			{
				ClearRegistryCache();

				shouldNudgeGLQ = false;
				var processedCount = 0;
				foreach (var company in GetToBeHandledCompanies())
				{
					youMustReactToThisToken.ThrowIfCancellationRequested();
					HandleBacklogDataForGLD(company);
					processedCount++;
				}

				if (processedCount > 0)
				{
					ServiceLogger.Information($"General ledger data backlog process completed for {processedCount} companies.");
				}

				if (shouldNudgeGLQ)
				{
					NudgeGLQServiceTask();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ServiceLogger.Error($"General Ledger Data Backlog Process error: {ex.Message}");
			}
			finally
			{
				ServiceLogger.Debug($"General Ledger Data Backlog Process End.");
			}
		}

		void NudgeGLQServiceTask()
		{
			ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask("GLQ");
			ServiceLogger.Debug($"Service Task GLQ has been nudged.");
		}

		void ClearRegistryCache()
		{
			AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.Inner.ClearCache();
			AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.Inner.ClearCache();
			AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.Inner.ClearCache();
		}

		IEnumerable<GlbCompany> GetToBeHandledCompanies()
		{
			var allActiveCompanies = AccountingUtils.GetAllActiveCompanies(Factory);
			foreach (var company in allActiveCompanies)
			{
				var journalEntriesStartDate = AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				var lastQueuedJournalEntryDate = AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);

				if (journalEntriesStartDate != DateTime.MinValue)
				{
					if (lastQueuedJournalEntryDate == DateTime.MinValue)
					{
						shouldNudgeGLQ = true;
						ServiceLogger.Debug($"Skip processing company: {company.GC_Code} because this company has never run GLQ task before.");
					}
					else if (lastQueuedJournalEntryDate != journalEntriesStartDate)
					{
						shouldNudgeGLQ = true;
						ServiceLogger.Debug($"Company: {company.GC_Code} failed to run because Last Queued Date: {lastQueuedJournalEntryDate.ToString(ZDateTime.ISO8601ShortDateFormat)} is not equal to Generate Journal Entries Start Date: {journalEntriesStartDate.ToString(ZDateTime.ISO8601ShortDateFormat)}.");
					}
					else
					{
						yield return company;
					}
				}
			}
		}

		void HandleBacklogDataForGLD(GlbCompany company)
		{
			var companyPK = company.PK.ToGuid();
			ServiceLogger.Debug($"General ledger data backlog process start: {company.GC_Code}.");

			try
			{
				while (true)
				{
					var sqlText = string.Format(CultureInfo.InvariantCulture, GLDQueueSqlScript);
					using (var cmd = Db.Connection.Command(sqlText))
					{
						cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
						var queueDataTable = DataUtils.GetDataTableFromCommand(cmd);

						if (queueDataTable.Rows.Count == 0)
						{
							var lastProcessedDate = AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
							var startDate = AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
							if (lastProcessedDate != startDate)
							{
								AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetValue(companyPK, Guid.Empty, Guid.Empty, startDate);
								if (GenerateAndStoreJournalEntriesForPostedAccountingTransactionsItemImpl.SetLatestUnprocessedFinanceYearStartDate(company.PK.ToGuid(), startDate.AddDays(-1)))
								{
									shouldNudgeGLQ = true;
								}
							}

							break;
						}

						var waitDeletedList = new HashSet<Guid>();
						var isSuccess = true;
						isSuccess &= HandleQueueDataForAccTransactionLine(queueDataTable, waitDeletedList);
						isSuccess &= HandleQueueDataForAccTransactionHeader(queueDataTable, waitDeletedList);
						isSuccess &= HandleQueueDataForAccCashBasisVat(queueDataTable, waitDeletedList);
						isSuccess &= HandleQueueDataForAccTaxGLMovement(queueDataTable, waitDeletedList);

						PurgeQueueDataAndUpdateLastProcessDate(waitDeletedList);

						if (!isSuccess)
						{
							break;
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ServiceLogger.Error($"General ledger data backlog process failed-> company : {company.GC_Code}, error: {ex.Message}");
			}

			ServiceLogger.Debug($"General ledger data backlog process end: {company.GC_Code}.");
		}

		#region Handle queue data for each table

		bool HandleQueueDataForAccTransactionLine(DataTable queueDataTable, HashSet<Guid> waitToDeletedPK)
		{
			var pkRowList = queueDataTable.Rows.Cast<DataRow>().Where(row => (string)row["APQ_ParentTableCode"] == AccTransactionLinesSchema.Constants.Prefix);
			return HandleDataRows(GetDataRowArrayByTable(LinesSqlScript, AccTransactionLinesSchema.Constants.TableName, pkRowList), pkRowList, waitToDeletedPK);
		}

		bool HandleQueueDataForAccTransactionHeader(DataTable queueDataTable, HashSet<Guid> waitToDeletedPK)
		{
			var pkRowList = queueDataTable.Rows.Cast<DataRow>().Where(row => (string)row["APQ_ParentTableCode"] == AccTransactionHeaderSchema.Constants.Prefix);
			return HandleDataRows(GetDataRowArrayByTable(HeaderSqlScript, AccTransactionHeaderSchema.Constants.TableName, pkRowList), pkRowList, waitToDeletedPK);
		}

		bool HandleQueueDataForAccCashBasisVat(DataTable queueDataTable, HashSet<Guid> waitToDeletedPK)
		{
			var pkRowList = queueDataTable.Rows.Cast<DataRow>().Where(row => (string)row["APQ_ParentTableCode"] == AccCashBasisVATSchema.Constants.Prefix);
			return HandleDataRows(GetDataRowArrayByTable(CashBasisVATSqlScript, AccCashBasisVATSchema.Constants.TableName, pkRowList), pkRowList, waitToDeletedPK);
		}

		bool HandleQueueDataForAccTaxGLMovement(DataTable queueDataTable, HashSet<Guid> waitToDeletedPK)
		{
			var pkRowList = queueDataTable.Rows.Cast<DataRow>().Where(row => (string)row["APQ_ParentTableCode"] == AccTaxGLMovementSchema.Constants.Prefix);
			return HandleDataRows(GetDataRowArrayByTable(TaxGLMovementSqlScript, AccTaxGLMovementSchema.Constants.TableName, pkRowList), pkRowList, waitToDeletedPK);
		}

		bool HandleDataRows(DataRow[] dataRows, IEnumerable<DataRow> pkRowList, HashSet<Guid> waitToDeletedPK)
		{
			if (dataRows.Any())
			{
				try
				{
					GeneralLedgerDataProcessor.ProcessData(dataRows);
					waitToDeletedPK.UnionWith(pkRowList.Select(row => (Guid)row["APQ_ParentID"]));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ServiceLogger.Error(ex.Message);
					return false;
				}
			}
			return true;
		}

		DataRow[] GetDataRowArrayByTable(string tableSqlScript, string tableName, IEnumerable<DataRow> pkRowList)
		{
			if (pkRowList.Any())
			{
				var sql = string.Format(CultureInfo.InvariantCulture, tableSqlScript);

				using (var cmd = Db.Connection.Command(sql))
				{
					cmd.AddTableValuedParameter("@PKs", "dbo.TVP_uniqueidentifier", pkRowList.Select(row => (Guid)row["APQ_ParentID"]).Distinct());
					var dataTable = DataUtils.GetDataTableFromCommand(cmd);
					if (dataTable.Rows.Count > 0)
					{
						dataTable.TableName = tableName;
						return dataTable.Rows.Cast<DataRow>().ToArray();
					}
				}
			}

			return Array.Empty<DataRow>();
		}

		#endregion

		#region Purge data

		void PurgeQueueDataAndUpdateLastProcessDate(HashSet<Guid> waitDeletedList)
		{
			if (waitDeletedList.Count > 0)
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, "DELETE dbo.AccTransactionPostingToGLDQueue WHERE APQ_ParentID IN (SELECT value FROM @PKs)");
				using (var cmd = Db.Connection.Command(sqlText))
				{
					cmd.AddTableValuedParameter("@PKs", "dbo.TVP_uniqueidentifier", waitDeletedList);
					cmd.ExecuteNonQuery();
				}
			}
		}

		#endregion

		static readonly int BatchNum =
#if DEBUG
			ZArchitecture.Environment.Globals.IsTest ? 5 :
#endif
			500;

		IGeneralLedgerDataProcessor GeneralLedgerDataProcessor => generalLedgerDataProcessor ?? (generalLedgerDataProcessor = ObjectFactory.Get<IGeneralLedgerDataProcessor>());
		IGeneralLedgerDataProcessor generalLedgerDataProcessor;

		ReadOnlyBusinessObjectFactory Factory => factory ?? (factory = new ReadOnlyBusinessObjectFactory());
		ReadOnlyBusinessObjectFactory factory;

		#region SqlScript

		static readonly string GLDQueueSqlScript = $@"
SELECT TOP {BatchNum} APQ_ParentID, APQ_ParentTableCode, APQ_GC_Company, APQ_JournalDate, APQ_IsReverse
FROM dbo.AccTransactionPostingToGLDQueue
WHERE APQ_GC_Company = @companyPK
ORDER BY APQ_JournalDate DESC
";
		const string HeaderSqlScript = @"
SELECT
	AH_PK,
	AH_Ledger,
	AH_TransactionType,
	AH_InvoiceAmount,
	AH_OSTotal,
	AH_PostDate,
	AH_GC,
	AH_ExchangeRate,
	AH_RX_NKTransactionCurrency,
	AH_OH,
	AH_GB,
	AH_GB_TaxBranch,
	AH_GE,
	AH_AG,
	AH_AB,
	AH_TransactionCount
FROM dbo.AccTransactionHeader
WHERE AH_PK IN (SELECT value FROM @PKs)
";

		const string CashBasisVATSqlScript = @"
SELECT
	YC_PK,
	YC_PostDate,
	YC_TaxBaseAmount,
	YC_TaxAmount,
	YC_AL_TransactionLine,
	YC_GC,
	YC_MatchGroupNum
FROM dbo.AccCashBasisVAT
WHERE YC_PK IN (SELECT value FROM @PKs)
";
		const string TaxGLMovementSqlScript = @"
SELECT
	ATM_PK,
	ATM_AG_CreditAccount,
	ATM_AG_DebitAccount,
	ATM_Amount,
	ATM_ATT_TaxTransaction,
	ATM_Date
FROM dbo.AccTaxGLMovement
WHERE ATM_PK IN (SELECT value FROM @PKs)
;";

		const string LinesSqlScript = @"
SELECT
	AL_PK,
	AL_LineType,
	AL_AH,
	AL_AG,
	AL_GSTVAT,
	AL_OSAmount,
	AL_LineAmount,
	AL_GSTVATBasis,
	AL_LineType,
	AL_InputGSTVATRecoverable,
	AL_ExchangeRate,
	AL_GC,
	AL_RX_NKTransactionCurrency,
	AL_OH,
	AL_GB,
	AL_GB_TaxBranch,
	AL_GE,
	AL_AC,
	CASE 
	WHEN APQ_IsReverse = 1
		THEN AL_ReverseDate
	ELSE NULL
	END AS AL_ReverseDate,
	CASE 
	WHEN APQ_IsReverse = 1 AND AL_LineType NOT IN ('AJL', 'RJL')
		THEN NULL
	ELSE AL_PostDate
	END AS AL_PostDate
FROM dbo.AccTransactionLines
INNER JOIN dbo.AccTransactionPostingToGLDQueue ON AL_PK = APQ_ParentID AND APQ_ParentTableCode = 'AL'
WHERE AL_PK IN (SELECT value FROM @PKs)
";

		#endregion
	}
}
