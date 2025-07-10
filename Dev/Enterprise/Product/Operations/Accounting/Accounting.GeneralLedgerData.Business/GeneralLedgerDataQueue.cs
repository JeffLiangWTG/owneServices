using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class GeneralLedgerDataQueue : IGeneralLedgerDataQueue
	{
		public void RemoveNarrowGLD(DbConnection dbConnection, Guid companyPK, DateTime startDate, DateTime endDate)
		{
			using (var manager = Db.Connection.BeginTransactionWithManager())
			{
				HandleComplianceReportFromGLD(dbConnection, companyPK, startDate, endDate);

				dbConnection.ExecuteNonQuery(GetSQL(), command =>
				{
					command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
					command.AddParameter("@StartDate", SqlDbType.DateTime, GetSqlDateTimeSafe(startDate));
					command.AddParameter("@EndDate", SqlDbType.DateTime, GetSqlDateTimeSafe(endDate));
				});

				manager.CommitTransaction();
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Partition Query in SQL")]
			string GetSQL() => @"
DELETE AccGeneralLedgerData
WHERE GLD_GC_Company = @CompanyPK
AND GLD_PostDate >= @StartDate
AND GLD_PostDate < @EndDate";	// Triggers CW1161 - recommend converting to interpolated string or casting to (NoResString)
		}

		void HandleComplianceReportFromGLD(DbConnection dbConnection, Guid companyPK, DateTime startDate, DateTime endDate)
		{
			var queryComplianceReportSql = @"
				SELECT DISTINCT ACL_ACR_Report AS Compliance_Report_PK FROM dbo.AccComplianceReportTransactionPivot
					JOIN dbo.AccGeneralLedgerData ON ACL_ParentID = GLD_PK
				WHERE GLD_GC_Company = @CompanyPK
					AND GLD_PostDate >= @StartDate
					AND GLD_PostDate < @EndDate
				";
			Action<DbCommand> commandAction = command =>
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@StartDate", SqlDbType.DateTime, GetSqlDateTimeSafe(startDate));
				command.AddParameter("@EndDate", SqlDbType.DateTime, GetSqlDateTimeSafe(endDate));
			};

			GeneralLedgerDataScriptHelper.HandleComplianceReportFromGLD(queryComplianceReportSql, commandAction);
		}

		public void QueueTransaction(DbConnection dbConnection, Guid companyPK, DateTime startDate, DateTime endDate, DateTime endSystemCreateTime, bool isCheckDuplicated)
		{
			Db.Connection.ExecuteNonQuery(GetSqlText(), command =>
			{
				var isFirstToRun = endDate == DateTime.MinValue;

				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@StartDate", SqlDbType.DateTime, GetSqlDateTimeSafe(startDate));
				command.AddParameter("@EndDate", SqlDbType.DateTime, GetSqlDateTimeSafe(endDate));
				command.AddParameter("@User", SqlDbType.VarChar, Env.CurrentUser.Initials);
				command.AddParameter("@EndSystemCreateTime", SqlDbType.DateTime, GetSqlDateTimeSafe(endSystemCreateTime));
				command.AddParameter("@isFirstTimeToRun", SqlDbType.Bit, isFirstToRun);
				command.AddParameter("@isCheckDuplicated", SqlDbType.Bit, isCheckDuplicated);
			});

			string GetSqlText()
			{
				return $@"
INSERT INTO AccTransactionPostingToGLDQueue (APQ_ParentID, APQ_ParentTableCode, APQ_GC_Company, APQ_JournalDate, APQ_IsReverse, APQ_SystemCreateTimeUtc, APQ_SystemCreateUser)
SELECT APQ_ParentID, APQ_ParentTableCode, APQ_GC_Company, APQ_JournalDate, APQ_IsReverse, GetUtcDate() as APQ_SystemCreateTimeUtc, @User as APQ_SystemCreateUser
FROM({GetSqlDataSourceText()}
) AS QueueList (APQ_ParentID, APQ_ParentTableCode, APQ_GC_Company, APQ_JournalDate, APQ_IsReverse, SystemCreateTimeUtc)
WHERE
	APQ_GC_Company = @CompanyPK
AND APQ_JournalDate >= @StartDate
AND SystemCreateTimeUtc <= @EndSystemCreateTime
AND (@isFirstTimeToRun = 1 OR APQ_JournalDate < @EndDate)
AND (@isCheckDuplicated = 0 OR NOT EXISTS(
		SELECT 1
		FROM AccTransactionPostingToGLDQueue
		WHERE AccTransactionPostingToGLDQueue.APQ_ParentID = QueueList.APQ_ParentID
		AND AccTransactionPostingToGLDQueue.APQ_IsReverse = QueueList.APQ_IsReverse
	)
)";
			}

			string GetSqlDataSourceText()
			{
				return @"
SELECT AH_PK, 'AH', AH_GC, AH_PostDate, 0, AH_SystemCreateTimeUtc
FROM AccTransactionHeader
WHERE AH_Ledger IN ('AR', 'AP')
AND (AH_TransactionType IN ('TRF', 'CTR', 'REC', 'PAY', 'DSC', 'OVP', 'EXX') OR AH_TransactionType = 'JNL' AND AH_TransactionCategory != 'PBW')

UNION ALL

SELECT AH_PK, 'AH', AH_GC, AH_PostDate, 0, AH_SystemCreateTimeUtc
FROM AccTransactionHeader
WHERE AH_Ledger IN ('CB') AND AH_TransactionType IN ('EXX', 'TRF')

UNION ALL

SELECT AL_PK, 'AL', AL_GC, AL_PostDate, 0, AL_SystemCreateTimeUtc
FROM AccTransactionLines
INNER JOIN AccTransactionHeader ON AL_AH = AH_PK
WHERE AH_Ledger = 'JC' AND AH_TransactionType IN ('JRJ', 'JNL')

UNION ALL

SELECT AL_PK, 'AL', AL_GC, AL_PostDate, 0, AL_SystemCreateTimeUtc
FROM AccTransactionLines
WHERE AL_LineType IN ('ACR', 'WIP', 'DRC','DPY', 'GJL', 'NJL')

UNION ALL

SELECT YC_PK, 'YC', YC_GC, YC_PostDate, 0, YC_SystemCreateTimeUtc
FROM AccCashBasisVAT

UNION ALL

SELECT ATM_PK, 'ATM', ATT_GC, ATM_Date, 0, ATM_SystemCreateTimeUtc
FROM AccTaxGLMovement
INNER JOIN AccTaxTransaction ON ATM_ATT_TaxTransaction = ATT_PK

UNION ALL

SELECT AL_PK, 'AL', AL_GC, AL_PostDate, 0, AL_SystemCreateTimeUtc
FROM AccTransactionLines
INNER JOIN AccTransactionHeader ON AL_AH = AH_PK
WHERE AL_LineType IN ('CST', 'REV') AND AH_Ledger in ('AR', 'AP') AND AH_TransactionType in ('INV', 'CRD', 'ADJ')

UNION ALL

SELECT AL_PK, 'AL', AL_GC, AL_ReverseDate, 1, AL_SystemCreateTimeUtc
FROM AccTransactionLines
INNER JOIN AccTransactionHeader ON AL_AH = AH_PK
WHERE AL_LineType IN ('CST', 'REV') AND AH_Ledger IN ('AR', 'AP') AND AH_TransactionType IN ('INV', 'CRD', 'ADJ')

UNION ALL

SELECT AL_PK, 'AL', AL_GC, AL_ReverseDate, 1, AL_SystemCreateTimeUtc
FROM AccTransactionLines
WHERE AL_LineType IN ('RJL','AJL')

UNION ALL

SELECT AL_PK, 'AL', AL_GC, AL_ReverseDate, 1, AL_SystemCreateTimeUtc
FROM AccTransactionLines
INNER JOIN AccTransactionHeader ON AL_AH = AH_PK
WHERE AH_Ledger = 'JC' AND AH_TransactionType IN ('JRJ', 'JNL')

UNION ALL

SELECT AL_PK, 'AL', AL_GC, AL_ReverseDate, 1, AL_SystemCreateTimeUtc
FROM AccTransactionLines
WHERE  AL_LineType IN ('ACR', 'WIP')";
			}
		}

		SqlDateTime GetSqlDateTimeSafe(DateTime dateTime) => new[] { dateTime, SqlDateTime.MinValue.Value }.Max();
	}
}
