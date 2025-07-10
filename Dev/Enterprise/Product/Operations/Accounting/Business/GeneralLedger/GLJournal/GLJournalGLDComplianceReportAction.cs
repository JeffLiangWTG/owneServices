using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public class GLJournalGLDComplianceReportAction : SaveInTransactionActionWithMainConnection
	{
		public GLJournalGLDComplianceReportAction(GLJournal gLJournal)
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
	Report_CodeType varchar(3)
)

INSERT INTO @UpdatedReport (Report_PK, Report_Status, Report_CodeType)
SELECT DISTINCT ACL_ACR_Report, ACR_Status, ACR_ReportType FROM
	dbo.AccComplianceReportTransactionPivot
INNER JOIN dbo.AccGeneralLedgerData
	ON ACL_ParentID = GLD_PK
INNER JOIN dbo.AccComplianceReport
	ON ACL_ACR_Report = ACR_PK
WHERE GLD_AH_TransactionHeader = @PK
	AND ACL_GC_Company = @CompanyPK
	AND ACL_ParentTableCode = @TableCode
	AND ACR_Status IN ('GEN', 'FIN')

DECLARE @FinComplianceReportCode varchar(500)
DECLARE @ErrorMsg varchar(500)

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
	WHERE ACR_Status = 'GEN'
		AND ACR_PK IN (SELECT Report_PK FROM @UpdatedReport)
		AND ACR_GC_Company = @CompanyPK

	IF @@ROWCOUNT != (SELECT COUNT(1) FROM @UpdatedReport)
	BEGIN
		SELECT @FinComplianceReportCode = STRING_AGG(Report_CodeType, ', ') FROM @UpdatedReport WHERE Report_Status = 'FIN'
		SET @ErrorMsg = CONCAT('The journal entries of this GL Journal have been included in the finalized compliance report <', @FinComplianceReportCode,'> and cannot be edited.')
		RAISERROR (@ErrorMsg, 16, 1) WITH NOWAIT;
	END
	ELSE
	BEGIN
		DECLARE @RowCount INT, @BatchSize INT
		SET @RowCount = 1
		SET @BATCHSIZE = 1000
		WHILE @RowCount > 0
		BEGIN
			DELETE top (@BatchSize) p FROM dbo.AccComplianceReportTransactionPivot p WITH (READPAST, READCOMMITTEDLOCK)
			INNER JOIN AccComplianceReport ON ACR_PK = ACL_ACR_Report
			WHERE ACR_GC_Company = @CompanyPK
				AND ACL_ParentTableCode = @TableCode
				AND ACL_ACR_Report IN (SELECT Report_PK FROM @UpdatedReport)
			SET @RowCount = @@ROWCOUNT
		END
	END
END
";
			Db.Connection.ExecuteNonQuery(clearComplianceReportAndTransactionPivotSql, command =>
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, gLJournal.PK.ToGuid());
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
				command.AddParameter("@TableCode", SqlDbType.VarChar, ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.GeneralLedgerData);
				command.AddParameter("@EditUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
			});

			return new ChangedTableNames(new[] { AccComplianceReportSchema.Constants.TableName, AccComplianceReportTransactionPivotSchema.Constants.TableName });
		}
	}
}
