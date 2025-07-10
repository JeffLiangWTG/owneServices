using System.Data;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public class GLJournalAllTransactionComplianceReportAction : SaveInTransactionActionWithMainConnection
	{
		public GLJournalAllTransactionComplianceReportAction(GLJournal gLJournal)
		{
			this.gLJournal = gLJournal;
		}

		readonly GLJournal gLJournal;

		protected override IChangedTableNames SaveInTransaction()
		{
			var clearComplianceReportAndTransactionPivotSql = @"
DECLARE @UpdatedReport TABLE (
	Report_PK UNIQUEIDENTIFIER,
	Report_Status varchar(3),
	Report_CodeType varchar(3),
	Report_Branch uniqueidentifier,
	Report_DateFrom date,
	Report_DateTo date
)

INSERT INTO @UpdatedReport (Report_PK, Report_Status, Report_CodeType, Report_Branch, Report_DateFrom, Report_DateTo)
SELECT DISTINCT ACL_ACR_Report, ACR_Status, ACR_ReportType, ACR_GB_Branch, ACR_DateFrom, ACR_DateTo FROM
	dbo.AccComplianceReportTransactionPivot
INNER JOIN dbo.AccTransactionLines
	ON ACL_ParentID = AL_PK
INNER JOIN dbo.AccComplianceReport
	ON ACL_ACR_Report = ACR_PK
WHERE ACL_ParentID IN (SELECT Value FROM @LinePKs)
	AND ACL_GC_Company = @CompanyPK
	AND ACL_ParentTableCode = @TableCode
	AND ACR_Status IN ('GEN', 'FIN')

UNION ALL

SELECT DISTINCT ACR_PK, ACR_Status, ACR_ReportType, ACR_GB_Branch, ACR_DateFrom, ACR_DateTo FROM
	dbo.AccTransactionComplianceReportQueue
INNER JOIN dbo.AccTransactionLines
	ON ACQ_ParentID = AL_PK
INNER JOIN dbo.AccComplianceReport
	ON ACQ_ReportType = ACR_ReportType
WHERE ACQ_ParentID IN (SELECT Value FROM @LinePKs)
	AND ACQ_GC_Company = @CompanyPK
	AND ACR_Status = 'QUE'
	AND (ACR_GB_Branch IS NULL OR ACQ_GB_Branch = ACR_GB_Branch)
	AND ACQ_Date >= ACR_DateFrom
	AND ACQ_Date < DATEADD(day, 1, ACR_DateTo)

DECLARE @FinComplianceReportCode varchar(500)
DECLARE @ErrorMsg varchar(500)
DECLARE @RowCount INT, @BatchSize INT

IF EXISTS(SELECT 1 FROM @UpdatedReport WHERE Report_Status = 'FIN')
BEGIN
	SELECT @FinComplianceReportCode = STRING_AGG(Report_CodeType, ', ') FROM @UpdatedReport WHERE Report_Status = 'FIN'
	SET @ErrorMsg = CONCAT('The journal entries of this GL Journal have been included in the finalized compliance report <', @FinComplianceReportCode,'> and cannot be edited.')
	RAISERROR (@ErrorMsg, 16, 1) WITH NOWAIT;
END
ELSE
BEGIN
	UPDATE AccComplianceReport SET
		ACR_Status = 'INV',
		ACR_SystemLastEditTimeUtc = CONVERT(smalldatetime, GETDATE()),
		ACR_SystemLastEditUser = @EditUser
	WHERE ACR_Status IN ('QUE', 'GEN')
		AND ACR_PK IN (SELECT Report_PK FROM @UpdatedReport)
		AND ACR_GC_Company = @CompanyPK

	IF @@ROWCOUNT != (SELECT COUNT(1) FROM @UpdatedReport)
	BEGIN
		SELECT @FinComplianceReportCode = STRING_AGG(Report_CodeType, ', ') FROM @UpdatedReport WHERE Report_Status = 'FIN'
		SET @ErrorMsg = CONCAT('The journal entries of this GL Journal have been included in the finalized compliance report <', @FinComplianceReportCode,'> and cannot be edited.')
		RAISERROR (@ErrorMsg, 16, 2) WITH NOWAIT;
	END
	ELSE
	BEGIN
		IF EXISTS(SELECT 1 FROM @UpdatedReport WHERE Report_Status IN ('QUE', 'GEN'))
		BEGIN
			SET @RowCount = 1
			SET @BATCHSIZE = 1000
			WHILE @RowCount > 0
			BEGIN
				DELETE top (@BatchSize) p FROM dbo.AccTransactionComplianceReportQueue p WITH (READPAST, READCOMMITTEDLOCK)
				JOIN @UpdatedReport u ON ACQ_ReportType = Report_CodeType
				WHERE ACQ_GC_Company = @CompanyPK
					AND (u.Report_Branch IS NULL OR ACQ_GB_Branch = u.Report_Branch)
					AND ACQ_Date >= u.Report_DateFrom
					AND ACQ_Date < DATEADD(day, 1, u.Report_DateTo)
				SET @RowCount = @@ROWCOUNT
			END
		END

		IF EXISTS(SELECT 1 FROM @UpdatedReport WHERE Report_Status = 'GEN')
		BEGIN
			SET @RowCount = 1
			SET @BATCHSIZE = 1000
			WHILE @RowCount > 0
			BEGIN
				DELETE top (@BatchSize) FROM dbo.AccComplianceReportTransactionPivot WITH (READPAST, READCOMMITTEDLOCK)
				WHERE ACL_GC_Company = @CompanyPK
					AND ACL_ParentTableCode = @TableCode
					AND ACL_ACR_Report IN (SELECT Report_PK FROM @UpdatedReport WHERE Report_Status = 'GEN')
				SET @RowCount = @@ROWCOUNT
			END
		END
	END
END
";
			Db.Connection.ExecuteNonQuery(clearComplianceReportAndTransactionPivotSql, command =>
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
				command.AddParameter("@TableCode", SqlDbType.VarChar, ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.TransactionLine);
				command.AddParameter("@EditUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
				command.AddTableValuedParameter("@LinePKs", TVPHelper.TVP_uniqueidentifier, gLJournal.GLJournalLines.Select(line => line.PK.ToGuid()));
			});

			return new ChangedTableNames(new[] { AccComplianceReportSchema.Constants.TableName, AccTransactionComplianceReportQueueSchema.Constants.TableName, AccComplianceReportTransactionPivotSchema.Constants.TableName });
		}
	}
}
