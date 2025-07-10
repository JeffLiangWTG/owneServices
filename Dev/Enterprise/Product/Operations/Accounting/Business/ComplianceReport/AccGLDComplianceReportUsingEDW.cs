using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public partial class AccGLDComplianceReportUsingEDW : AccComplianceReport
	{
		public AccGLDComplianceReportUsingEDW(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override DbConnection LoadDbConnection => EdwConnection;

		protected override string GetLoadBalanceDetailsSql()
		{
			return string.Format(LoadBalanceDetails, LoadCompanyKeyForEdw, GLDAggregateQueryForEdw, HeaderDetailsQueryForEdw);
		}

		public override AccComplianceReportLineCollectionBase<GeneralLedgerBalanceLine> GLMovementDetailsExport
		{
			get
			{
				CheckIfLoadingLimitApplied();
				
				if (gLMovementDetailsExport == null)
				{
					CalculateGlMovementDetailsIfApplicable(ref gLMovementDetailsExport, ReportLinesExport);
				}
				return gLMovementDetailsExport;
			}
		}
		AccComplianceReportLineCollectionBase<GeneralLedgerBalanceLine> gLMovementDetailsExport;

		public override string SAFTSingleXmlExportedEventLog => FormattableString.Invariant($"Purpose: SAFT Single XML Exported. This report is generated from EDW report data source last updated at {GetLastDateTransformRunUtcForEdw()}.");

		public override string SAFTAnnualXmlExportedEventLog => FormattableString.Invariant($"Purpose: SAFT Annual XML Exported. This report is generated from EDW report data source last updated at {GetLastDateTransformRunUtcForEdw()}.");

		public override AccComplianceReportLineCollectionBase<AccComplianceReportLine> ReportLinesExport
		{
			get
			{
				CheckIfLoadingLimitApplied();
				
				LoadReportLines(ref reportLinesExport, GetReportLinesLoadSqlFromEDWDataBase, LoadDbConnection);

				return reportLinesExport;
			}
		}
		AccComplianceReportLineCollectionBase<AccComplianceReportLine> reportLinesExport;

		string GetReportLinesLoadSqlFromEDWDataBase()
		{
			var columnNames = GetReportLinesColumnNames(reportLinesExport);
			var getLinesTVF = GetComplianceReportLinesBasedOnGroupByAndTablePrefix();

			return FormattableString.Invariant($@"SELECT {(ApplyLinesLoadingLimit ? $"TOP {MaxReportLinesToLoad} " : string.Empty)}{columnNames}
FROM {Db.EdwDatabaseName}.dbo.{getLinesTVF} (@PK, @ReportTablePrefix, @ReportDateFrom, @ReportDateTo_PlusOneDay, @RegType, @ReportCountry, @RepCountryRegistrationCodeType, @GS, @RoundingType, @Rounding, @ReverseSign)
ORDER BY ACL_ReportSequence, AH_TransactionType, AG_AccountNum, GB_Code, GE_Code, SIGN(GeneralLedgerAmount) OPTION(RECOMPILE)");
		}

		DbConnection EdwConnection
		{
			get
			{
				if (edwConnection == null)
				{
					var eDWServerName = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
					edwConnection = !string.IsNullOrEmpty(eDWServerName) ? Db.NewExtraConnectionWithMainDbCredentials(eDWServerName, Db.EdwDatabaseName) : null;
				}
				return edwConnection;
			}
		}
		DbConnection edwConnection;

		string GLDAggregateQueryForEdw => string.Format(@"
			SELECT
				GLAccountID AS AG_PK,
				Amount AS AA_Amount,
				AccountTypeCode AS AG_AccountType,
				PostPeriod AS AA_Period
			FROM 
				[{0}].dbo.GetGLAggregate(@CompanyKey, null, null) Aggregate
				INNER JOIN [{0}].[Finance].[BAS__GLAccount] GlHeader ON Aggregate.GLAccountKey = GlHeader.GLAccountKey", Db.EdwDatabaseName);

		string LoadCompanyKeyForEdw => string.Format(@"DECLARE @CompanyKey BIGINT
SET @CompanyKey = (SELECT CompanyKey FROM [{0}].[Organization].[BAS__Company] WHERE CompanyID = @CompanyPK);", Db.EdwDatabaseName);

		protected string HeaderDetailsQueryForEdw => string.Format(@"
			SELECT
				header.AccountNo AS AG_AccountNum,
				header.AccountTypeCode AS AG_AccountType,
				header.DebitCreditCode AS AG_DebitCredit,
				header.GLAccountID AS AG_PK,
				header.Description AS AG_Description,
				consolidationHeader.GLAccountID AS AG_AG_ConsolidationNum,
				consolidationHeader.AccountNo AS AG_ConsolidationAccountNum,
				header.AccountTypeCode AS AG_ConsolidationAccountType, 
				header.Description AS AG_ConsolidationDescription
			FROM
				[{0}].[Finance].[BAS__GLAccount] header
				LEFT JOIN [{0}].[Finance].[BAS__GLAccount] consolidationHeader ON consolidationHeader.GLAccountKey = header.ConsolidationAccountKey", Db.EdwDatabaseName);

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		string GetLastDateTransformRunUtcForEdw()
		{
			if (EdwConnection == null)
			{
				return "";
			}
			var lastDateTransformRunUtcForEdwSql = string.Format($"SELECT CONVERT(varchar(16), [ParamValue], 120) AS LastDateTransformRunUtc FROM [{Db.EdwDatabaseName}].[biadmin].[MasterState] WHERE [ParamName] = 'LAST_DATE_TRANSFORM_RUN_UTC'");
			var cmd = EdwConnection.Command(lastDateTransformRunUtcForEdwSql);
			var result = cmd.ExecuteScalar();

			return result == null ? "" : (string)result;
		}
	}
}
