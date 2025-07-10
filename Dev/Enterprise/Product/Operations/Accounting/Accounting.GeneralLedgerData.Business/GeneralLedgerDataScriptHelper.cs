using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public static class GeneralLedgerDataScriptHelper
	{
		static string UserCode => GlbStaff.CurrentUser.GS_Code;
		static string UserCodeParameter => "@userCode";
		static string TVPGeneralLedgerDataParameter => "@tvpGeneralLedgerData";
		static string TVPGeneralLedgerDataForGLJournalParameter => "@tvpGeneralLedgerDataForGLJournal";
		static string GeneralLedgerDataTVPName => "dbo.TVP_GeneralLedgerData";
		static string UniqueidentifierTVPName => "dbo.TVP_uniqueidentifier";
		static string TVPToDeleteGLJournalGLD => "@toDeleteGLDGLJournalPKs";

		public static DataTable GetEmptyTVPGeneralLedgerData()
		{
			var dataTable = new DataTable(GeneralLedgerDataTVPName);
			dataTable.Locale = CultureInfo.InvariantCulture;

			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_GC_Company, typeof(Guid));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_PostDate, typeof(DateTime));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_PostPeriod, typeof(int));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_AG_GLAccount, typeof(Guid));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_OSDebitAmount, typeof(decimal));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_OSCreditAmount, typeof(decimal));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_LocalDebitAmount, typeof(decimal));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_LocalCreditAmount, typeof(decimal));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_AH_TransactionHeader, typeof(Guid));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_AL_TransactionLine, typeof(Guid));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_GB_Branch, typeof(Guid));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_GB_TaxBranch, typeof(Guid));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_GE_Department, typeof(Guid));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_Currency, typeof(string));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_ExchangeRate, typeof(decimal));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_YC_CashBasisVAT, typeof(Guid));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_ATM_TaxGLMovement, typeof(Guid));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_GLAccountType, typeof(string));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_Type, typeof(string));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_JournalEntriesNumber, typeof(string));
			dataTable.Columns.Add(AccGeneralLedgerDataSchema.Constants.GLD_JournalEntriesNumberRuleCode, typeof(string));

			return dataTable;
		}

		public static void PopulateTVPGeneralLedgerData(DataTable tVPGeneralLedgerData, DebitCreditEntry debitCreditEntry, ZString ledger, ZString transactionType)
		{
			if (debitCreditEntry != null)
			{
				foreach (var drCrLine in debitCreditEntry.EntryItems)
				{
					var row = tVPGeneralLedgerData.NewRow();
					row[AccGeneralLedgerDataSchema.Constants.GLD_AG_GLAccount] = drCrLine.AccountPK.ToGuid();
					row[AccGeneralLedgerDataSchema.Constants.GLD_Type] = drCrLine.GLDType;
					row[AccGeneralLedgerDataSchema.Constants.GLD_GLAccountType] = drCrLine.GLDAccountType;
					if (drCrLine.DRCRSign == DebitCredit.DR)
					{
						row[AccGeneralLedgerDataSchema.Constants.GLD_LocalDebitAmount] = Math.Abs(drCrLine.LocalAmount);
						row[AccGeneralLedgerDataSchema.Constants.GLD_OSDebitAmount] = Math.Abs(drCrLine.OSAmount);
						row[AccGeneralLedgerDataSchema.Constants.GLD_LocalCreditAmount] = 0;
						row[AccGeneralLedgerDataSchema.Constants.GLD_OSCreditAmount] = 0;
					}
					else if (drCrLine.DRCRSign == DebitCredit.CR)
					{
						row[AccGeneralLedgerDataSchema.Constants.GLD_LocalDebitAmount] = 0;
						row[AccGeneralLedgerDataSchema.Constants.GLD_OSDebitAmount] = 0;
						row[AccGeneralLedgerDataSchema.Constants.GLD_LocalCreditAmount] = Math.Abs(drCrLine.LocalAmount);
						row[AccGeneralLedgerDataSchema.Constants.GLD_OSCreditAmount] = Math.Abs(drCrLine.OSAmount);
					}

					row[AccGeneralLedgerDataSchema.Constants.GLD_AH_TransactionHeader] = ConvertEmptyGuidToDBNull(debitCreditEntry.TransactionHeaderPK);
					row[AccGeneralLedgerDataSchema.Constants.GLD_AL_TransactionLine] = ConvertEmptyGuidToDBNull(debitCreditEntry.TransactionLinePK);
					row[AccGeneralLedgerDataSchema.Constants.GLD_ATM_TaxGLMovement] = ConvertEmptyGuidToDBNull(debitCreditEntry.TaxGLMovementPK);
					row[AccGeneralLedgerDataSchema.Constants.GLD_YC_CashBasisVAT] = ConvertEmptyGuidToDBNull(debitCreditEntry.CashBasisVatPK);

					row[AccGeneralLedgerDataSchema.Constants.GLD_Currency] = debitCreditEntry.Currency;
					row[AccGeneralLedgerDataSchema.Constants.GLD_ExchangeRate] = debitCreditEntry.ExchangeRate;
					row[AccGeneralLedgerDataSchema.Constants.GLD_GB_Branch] = drCrLine.BranchPK != Guid.Empty ? drCrLine.BranchPK : debitCreditEntry.BranchPK;
					row[AccGeneralLedgerDataSchema.Constants.GLD_GC_Company] = debitCreditEntry.CompanyPK;
					row[AccGeneralLedgerDataSchema.Constants.GLD_GB_TaxBranch] = ConvertEmptyGuidToDBNull(debitCreditEntry.TaxBranchPK);
					row[AccGeneralLedgerDataSchema.Constants.GLD_GE_Department] = drCrLine.DepartmentPK != Guid.Empty ? drCrLine.DepartmentPK : debitCreditEntry.DepartmentPK;
					row[AccGeneralLedgerDataSchema.Constants.GLD_PostDate] = drCrLine.JournalDate.ToDateTime();
					row[AccGeneralLedgerDataSchema.Constants.GLD_PostPeriod] = drCrLine.Period;

					row[AccGeneralLedgerDataSchema.Constants.GLD_JournalEntriesNumber] = string.Empty;
					row[AccGeneralLedgerDataSchema.Constants.GLD_JournalEntriesNumberRuleCode] = string.Empty;

					tVPGeneralLedgerData.Rows.Add(row);
				}
			}
		}

		static object ConvertEmptyGuidToDBNull(Guid valueGuid)
		{
			if (valueGuid == Guid.Empty)
			{
				return DBNull.Value;
			}
			else
			{
				return valueGuid;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "<Pending>")]
		public static void ExecuteGeneralLedgerDataNonGLJournalScript(DataTable tvpGeneralLedgerData)
		{
			var sql = new StringBuilder(InsertSqlWithTVP).ToString();
			using (var command = Db.Connection.Command(sql))
			{
				command.AddTableValuedParameter(TVPGeneralLedgerDataParameter, GeneralLedgerDataTVPName, tvpGeneralLedgerData);
				command.AddParameter(UserCodeParameter, SqlDbType.VarChar, UserCode);
				command.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "<Pending>")]
		public static void ExecuteGeneralLedgerDataForGLJournalScript(DataTable tvpGeneralLedgerDataForGLJournal, Guid[] toDeleteGLDGLJournalLinePKs)
		{
			var sql = new StringBuilder();
			var hasGLJournalGLDToDelete = toDeleteGLDGLJournalLinePKs.Any();
			if (hasGLJournalGLDToDelete)
			{
				sql.Append(DeleteGLJournalDataScript);
			}
			sql.AppendLine(InsertSqlWithTVPForGLJournal);

			using (var command = Db.Connection.Command(sql.ToString()))
			{
				if (hasGLJournalGLDToDelete)
				{
					command.AddTableValuedParameter(TVPToDeleteGLJournalGLD, UniqueidentifierTVPName, toDeleteGLDGLJournalLinePKs);
				}

				command.AddTableValuedParameter(TVPGeneralLedgerDataForGLJournalParameter, GeneralLedgerDataTVPName, tvpGeneralLedgerDataForGLJournal);
				command.AddParameter(UserCodeParameter, SqlDbType.VarChar, UserCode);
				command.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Partition Query in SQL")]
		public static string GetBacklogQueueSqlText()
		{
			var transactionHeaderDateFilter = @"AND AH_PostDate >= @StartDate
AND AH_SystemCreateTimeUtc <= @CDCStartDate
AND (@isFirstTimeToRun = 1 OR AH_PostDate < @EndDate)";

			var transactionLineReverseDateFilter = @"AND AL_ReverseDate >= @StartDate
AND AL_SystemCreateTimeUtc <= @CDCStartDate
AND (@isFirstTimeToRun = 1 OR AL_ReverseDate < @EndDate)";

			var transactionLinePostDateFilter = @"AND AL_PostDate >= @StartDate
AND AL_SystemCreateTimeUtc <= @CDCStartDate
AND (@isFirstTimeToRun = 1 OR AL_PostDate < @EndDate)";

			var cashBasisVATDateFilter = @"AND YC_PostDate >= @StartDate
AND YC_SystemCreateTimeUtc <= @CDCStartDate
AND (@isFirstTimeToRun = 1 OR YC_PostDate < @EndDate)";

			var accTaxGLMovementFilter = @"AND ATM_Date >= @StartDate
AND ATM_SystemCreateTimeUtc <= @CDCStartDate
AND (@isFirstTimeToRun = 1 OR ATM_Date < @EndDate)";	// Triggers CW1161 - recommend converting to interpolated string or casting to (NoResString)

			return $@"
SELECT AH_PK, 'AH', AH_GC, AH_PostDate, 0, GetUtcDate(), @User FROM dbo.AccTransactionHeader
WHERE AH_GC = @CompanyPK
AND AH_Ledger IN ('AR', 'AP')
AND (AH_TransactionType IN ('TRF', 'CTR', 'REC', 'PAY', 'DSC', 'OVP', 'EXX') OR AH_TransactionType = 'JNL' AND AH_TransactionCategory != 'PBW')
{transactionHeaderDateFilter}

UNION ALL

SELECT AH_PK, 'AH', AH_GC, AH_PostDate, 0, GetUtcDate(), @User FROM dbo.AccTransactionHeader
WHERE AH_GC = @CompanyPK
AND AH_Ledger IN ('CB')
AND AH_TransactionType IN ('EXX', 'TRF')
{transactionHeaderDateFilter}

UNION ALL

SELECT AL_PK, 'AL', AL_GC, AL_PostDate, 0, GetUtcDate(), @User FROM dbo.AccTransactionLines
INNER JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK AND AH_Ledger = 'JC' AND AH_TransactionType IN ('JRJ', 'JNL')
WHERE AL_GC = @CompanyPK
{transactionLinePostDateFilter}

UNION ALL

SELECT AL_PK, 'AL', AL_GC, AL_PostDate, 0, GetUtcDate(), @User FROM dbo.AccTransactionLines
WHERE AL_GC = @CompanyPK
AND AL_LineType IN ('ACR', 'WIP', 'DRC','DPY', 'GJL', 'NJL')
{transactionLinePostDateFilter}

UNION ALL

SELECT YC_PK, 'YC', YC_GC, YC_PostDate, 0, GetUtcDate(), @User FROM dbo.AccCashBasisVAT
WHERE YC_GC = @CompanyPK
{cashBasisVATDateFilter}

UNION ALL

SELECT ATM_PK, 'ATM', ATT_GC, ATM_Date, 0, GetUtcDate(), @User FROM dbo.AccTaxGLMovement INNER JOIN dbo.AccTaxTransaction ON ATM_ATT_TaxTransaction = ATT_PK
WHERE ATT_GC = @CompanyPK
{accTaxGLMovementFilter}

UNION ALL

SELECT AL_PK, 'AL', AL_GC, AL_PostDate, 0, GetUtcDate(), @User FROM dbo.AccTransactionLines
INNER JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK AND AH_Ledger in ('AR', 'AP') AND AH_TransactionType in ('INV', 'CRD', 'ADJ')
WHERE AL_GC = @CompanyPK
AND AL_LineType IN ('CST', 'REV')
{transactionLinePostDateFilter}

UNION ALL

SELECT AL_PK, 'AL', AL_GC, AL_ReverseDate, 1, GetUtcDate(), @User FROM dbo.AccTransactionLines
INNER JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK AND AH_Ledger IN ('AR', 'AP') AND AH_TransactionType IN ('INV', 'CRD', 'ADJ')
WHERE AL_GC = @CompanyPK
AND AL_LineType IN ('CST', 'REV')
{transactionLineReverseDateFilter}

UNION ALL

SELECT AL_PK, 'AL', AL_GC, AL_ReverseDate, 1, GetUtcDate(), @User FROM dbo.AccTransactionLines
WHERE AL_GC = @CompanyPK
AND AL_LineType IN ('RJL','AJL')
{transactionLineReverseDateFilter}

UNION ALL

SELECT AL_PK, 'AL', AL_GC, AL_ReverseDate, 1, GetUtcDate(), @User FROM dbo.AccTransactionLines
INNER JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK AND AH_Ledger = 'JC' AND AH_TransactionType IN ('JRJ', 'JNL')
WHERE AL_GC = @CompanyPK
{transactionLineReverseDateFilter}

UNION ALL

SELECT AL_PK, 'AL', AL_GC, AL_ReverseDate, 1, GetUtcDate(), @User FROM dbo.AccTransactionLines
WHERE AL_GC = @CompanyPK
AND AL_LineType IN ('ACR', 'WIP')
{transactionLineReverseDateFilter}";
		}

		static string DeleteGLJournalDataScript => $@"DELETE dbo.AccGeneralLedgerData WHERE GLD_AL_TransactionLine IN (SELECT Value FROM {TVPToDeleteGLJournalGLD})";

		static string InsertSqlWithTVPBase => FormattableString.Invariant($@"
		INSERT INTO dbo.AccGeneralLedgerData
		([GLD_GC_Company],
		[GLD_PostDate],
		[GLD_PostPeriod],
		[GLD_AG_GLAccount],
		[GLD_OSDebitAmount],
		[GLD_OSCreditAmount],
		[GLD_LocalDebitAmount],
		[GLD_LocalCreditAmount],
		[GLD_AH_TransactionHeader],
		[GLD_AL_TransactionLine],
		[GLD_GB_Branch],
		[GLD_GB_TaxBranch],
		[GLD_GE_Department],
		[GLD_Currency],
		[GLD_ExchangeRate],
		[GLD_YC_CashBasisVAT],
		[GLD_ATM_TaxGLMovement],
		[GLD_SystemCreateTimeUtc],
		[GLD_SystemCreateUser],
		[GLD_SystemLastEditTimeUtc],
		[GLD_SystemLastEditUser],
		[GLD_GLAccountType],
		[GLD_JournalEntriesNumber],
		[GLD_JournalEntriesNumberRuleCode],
		[GLD_Type],
		[GLD_PK])
		SELECT
		tvp.GLD_GC_Company,
		tvp.GLD_PostDate,
		tvp.GLD_PostPeriod,
		tvp.GLD_AG_GLAccount,
		tvp.GLD_OSDebitAmount,
		tvp.GLD_OSCreditAmount,
		tvp.GLD_LocalDebitAmount,
		tvp.GLD_LocalCreditAmount,
		tvp.GLD_AH_TransactionHeader,
		tvp.GLD_AL_TransactionLine,
		tvp.GLD_GB_Branch,
		tvp.GLD_GB_TaxBranch,
		tvp.GLD_GE_Department,
		tvp.GLD_Currency,
		tvp.GLD_ExchangeRate,
		tvp.GLD_YC_CashBasisVAT,
		tvp.GLD_ATM_TaxGLMovement,
		GetUtcDate(),
		{UserCodeParameter},
		GetUtcDate(),
		{UserCodeParameter},
		tvp.GLD_GLAccountType,
		tvp.GLD_JournalEntriesNumber,
		tvp.GLD_JournalEntriesNumberRuleCode,
		tvp.GLD_Type,
		NEWID()
		FROM ");

		static string InsertSqlWithTVP => FormattableString.Invariant($@"
		{InsertSqlWithTVPBase} {TVPGeneralLedgerDataParameter} AS tvp
WHERE NOT EXISTS (SELECT 1 FROM dbo.AccGeneralLedgerData gld
WHERE gld.GLD_GC_Company = tvp.GLD_GC_Company
AND gld.GLD_PostDate = tvp.GLD_PostDate
AND (gld.GLD_AH_TransactionHeader = tvp.GLD_AH_TransactionHeader OR (gld.GLD_AH_TransactionHeader IS NULL AND tvp.GLD_AH_TransactionHeader IS NULL))
AND (gld.GLD_AL_TransactionLine = tvp.GLD_AL_TransactionLine OR (gld.GLD_AL_TransactionLine IS NULL AND tvp.GLD_AL_TransactionLine IS NULL))
AND (gld.GLD_YC_CashBasisVAT = tvp.GLD_YC_CashBasisVAT OR (gld.GLD_YC_CashBasisVAT IS NULL AND tvp.GLD_YC_CashBasisVAT IS NULL))
AND (gld.GLD_ATM_TaxGLMovement = tvp.GLD_ATM_TaxGLMovement OR (gld.GLD_ATM_TaxGLMovement IS NULL AND tvp.GLD_ATM_TaxGLMovement IS NULL))
AND gld.GLD_Type = tvp.GLD_Type
AND gld.GLD_GLAccountType= tvp.GLD_GLAccountType);");

		static string InsertSqlWithTVPForGLJournal => FormattableString.Invariant($@"
		{InsertSqlWithTVPBase} {TVPGeneralLedgerDataForGLJournalParameter} AS tvp
INNER JOIN dbo.AccTransactionLines ON tvp.GLD_AL_TransactionLine = AL_PK
WHERE NOT EXISTS (SELECT 1 FROM dbo.AccGeneralLedgerData gld
					WHERE gld.GLD_GC_Company = tvp.GLD_GC_Company
					AND gld.GLD_AL_TransactionLine = tvp.GLD_AL_TransactionLine
					AND gld.GLD_Type = tvp.GLD_Type);");

		#region Update GLD Period

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "<Pending>")]
		public static void ExecuteUpdateGLDPeriodScript(ZDateTime startDateTime, ZDateTime endDateTime)
		{
			var endDateTimeValue = new ZDateTime(endDateTime.Year, endDateTime.Month, endDateTime.Day, 23, 59, 0);

			using (var manager = Db.Connection.BeginTransactionWithManager())
			{
				using (var sqlCommand = Db.Connection.Command(GetUpdateGLDPeriodSQL(startDateTime, endDateTime)))
				{
					sqlCommand.AddParameter(UserCodeParameter, SqlDbType.VarChar, UserCode);
					sqlCommand.AddParameterBasedOnDbColumn("@CompanyPK", GlbCompany.CurrentCompany.PK.ToGuid(), AccGeneralLedgerDataSchema.GLD_GC_Company);
					if (!startDateTime.IsEmpty)
					{
						sqlCommand.AddParameterBasedOnDbColumn("@StartDate", startDateTime, AccGeneralLedgerDataSchema.GLD_PostDate);
					}
					if (!endDateTime.IsEmpty)
					{
						sqlCommand.AddParameterBasedOnDbColumn("@EndDate", endDateTimeValue, AccGeneralLedgerDataSchema.GLD_PostDate);
					}

					sqlCommand.ExecuteNonQuery();
				}
				manager.CommitTransaction();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Partition Query in SQL")]
		static string GetUpdateGLDPeriodSQL(ZDateTime startDateTime, ZDateTime endDateTime)
		{
			var sqlBuilder = new ZStringBuilder();

			sqlBuilder.AppendLine($@"
			UPDATE dbo.AccGeneralLedgerData
			SET GLD_PostPeriod = AM_Period, GLD_SystemLastEditTimeUtc = GetUtcDate(), GLD_SystemLastEditUser = {UserCodeParameter}
			FROM dbo.AccGeneralLedgerData
			CROSS APPLY dbo.GetPeriodFromDateInline (GLD_PostDate, GLD_GC_Company) as GetPeriod
			WHERE GLD_GC_Company = @CompanyPK
			AND GLD_PostPeriod <> AM_Period");

			if (!startDateTime.IsEmpty)
			{
				sqlBuilder.AppendLine("AND GLD_PostDate >= @StartDate");	// Triggers CW1161 - recommend converting to interpolated string or casting to (NoResString)
			}

			if (!endDateTime.IsEmpty)
			{
				sqlBuilder.AppendLine("AND GLD_PostDate <= @EndDate");		// Triggers CW1161 - recommend converting to interpolated string or casting to (NoResString)
			}

			return sqlBuilder.ToString();
		}

		#endregion

		#region Purge GLD Data

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "<Pending>")]
		public static void PurgeGeneralLedgerData()
		{
			var cdcStartDateName = AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.Name;
			var gldStartDateName = AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.Name;
			var lastProcessDateName = AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.Name;
			var lastQueueDateName = AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.Name;
			var generateAndStoreJournalEntriesForPostedAccountingTransactions = AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.Name;
			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();

			HandleAllComplianceReportWhenPurgeGLD(currentCompanyPK);

			var clearRegistrySettingsSql = @"
DELETE dbo.StmData
WHERE SD_Owner = @CompanyPK
AND SD_Name IN (@CdcStartDateName, @GldStartDateName, @LastProcessDateName, @LastQueueDateName, @GenerateAndStoreJournalEntriesForPostedAccountingTransactions)";
			using (var command = Db.Connection.Command(clearRegistrySettingsSql))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, currentCompanyPK);
				command.AddParameter("@CdcStartDateName", SqlDbType.VarChar, cdcStartDateName);
				command.AddParameter("@GldStartDateName", SqlDbType.VarChar, gldStartDateName);
				command.AddParameter("@LastProcessDateName", SqlDbType.VarChar, lastProcessDateName);
				command.AddParameter("@LastQueueDateName", SqlDbType.VarChar, lastQueueDateName);
				command.AddParameter("@GenerateAndStoreJournalEntriesForPostedAccountingTransactions", SqlDbType.VarChar, generateAndStoreJournalEntriesForPostedAccountingTransactions);
				command.ExecuteNonQuery();
			}

			var clearGeneralLedgerDataSql = @"
DECLARE @RowCount INT, @BatchSize INT
SET @RowCount = 1
SET @BATCHSIZE = 1000
WHILE @RowCount > 0
BEGIN
	DELETE top (@BatchSize) FROM dbo.AccGeneralLedgerData WITH (READPAST, READCOMMITTEDLOCK) WHERE GLD_GC_Company = @CompanyPK
	SET @RowCount = @@ROWCOUNT
END

SET @RowCount = 1
WHILE @RowCount > 0
BEGIN
	DELETE top (@BatchSize) FROM dbo.AccTransactionPostingToGLDQueue WITH (READPAST, READCOMMITTEDLOCK) WHERE APQ_GC_Company = @CompanyPK
	SET @RowCount = @@ROWCOUNT
END
";
			using (var command = Db.Connection.Command(clearGeneralLedgerDataSql))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, currentCompanyPK);
				command.ExecuteNonQuery();
			}
		}

		static void HandleAllComplianceReportWhenPurgeGLD(Guid currentCompanyPK)
		{
			var gldReportCodeList = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Cast<ComplianceReportConfiguration>()
				.Where(x => x.ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.GeneralLedgerData).Select(x => x.ReportCode.ToString());

			if (gldReportCodeList.Any() )
			{
				var queryComplianceReportSql = @"SELECT ACR_PK AS Compliance_Report_PK FROM dbo.AccComplianceReport
									WHERE ACR_ReportType IN (SELECT value FROM @ReportType)
									AND ACR_Status IN ('FIN','GEN')
									AND ACR_GC_Company = @CompanyPK";
				Action<DbCommand> commandAction = command =>
				{
					command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, currentCompanyPK);
					command.AddTableValuedParameter("@ReportType", TVPHelper.TVP_varchar, gldReportCodeList);
				};
				HandleComplianceReportFromGLD(queryComplianceReportSql, commandAction);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Partition Query in SQL")]
		public static void HandleComplianceReportFromGLD(string queryComplianceReportSql, Action<DbCommand> commandAction)
		{
			var needInvalidateComplianceReportPks = new List<Guid>();
			Db.Connection.ExecuteReader(
				queryComplianceReportSql,
				commandAction,
				data =>
				{
					needInvalidateComplianceReportPks.Add((Guid)data["Compliance_Report_PK"]);
				});

			if (needInvalidateComplianceReportPks.Count == 0)
			{
				return;
			}

			var clearComplianceReportAndTransactionPivotSql = @"
DECLARE @RowCount INT, @BatchSize INT
SET @RowCount = 1
SET @BATCHSIZE = 1000
WHILE @RowCount > 0
BEGIN
	DELETE top (@BatchSize) p FROM dbo.AccComplianceReportTransactionPivot p WITH (READPAST, READCOMMITTEDLOCK)
	INNER JOIN dbo.AccComplianceReport ON ACR_PK = ACL_ACR_Report
	WHERE ACR_GC_Company = @CompanyPK
		AND ACL_ParentTableCode = @TableCode
		AND ACL_ACR_Report IN (SELECT value FROM @ReportPks)
	SET @RowCount = @@ROWCOUNT
END

UPDATE dbo.AccComplianceReport SET
	ACR_Status = 'INV',
	ACR_SystemLastEditTimeUtc = CONVERT(smalldatetime, GETDATE()),
	ACR_SystemLastEditUser = @EditUser
WHERE ACR_Status IN ('GEN','FIN')
	AND ACR_PK IN (SELECT value FROM @ReportPks)
	AND ACR_GC_Company = @CompanyPK
";
			Db.Connection.ExecuteNonQuery(clearComplianceReportAndTransactionPivotSql, command =>
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
				command.AddParameter("@TableCode", SqlDbType.VarChar, ReportBaseTablePrefixListCodes.GeneralLedgerData);
				command.AddParameter("@EditUser", SqlDbType.VarChar, Db.GetCurrentUserOrDefault(defaultUser: "~BP"));	// Triggers CW1161 - recommend converting to interpolated string or casting to (NoResString)
				command.AddTableValuedParameter("@ReportPks", TVPHelper.TVP_uniqueidentifier, needInvalidateComplianceReportPks.Distinct());
			});
		}
		#endregion
	}
}
