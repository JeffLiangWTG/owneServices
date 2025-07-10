using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using PeriodicityCodes = Enterprise.Registry.Business.ComplianceReportConfigurationLookups.ReportPeriodicityCodes;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	[TestedType(typeof(AccGLDComplianceReportUsingEDW))]
	public class AccGLDComplianceReportUsingEDWTest : AccComplianceReportTest
	{
		[TestDate(2024, 01, 01, 01, 01, 01)]
		public void TestGLOpeningBalanceDetails_IsUsingGLDTablePrefix_DR()
		{
			AssertGLOpeningBalanceDetails_IsUsingGLDTablePrefix(true);
		}

		[TestDate(2024, 01, 01, 01, 01, 01)]
		public void TestGLOpeningBalanceDetails_IsUsingGLDTablePrefix_CR()
		{
			AssertGLOpeningBalanceDetails_IsUsingGLDTablePrefix(false);
		}

		void AssertGLOpeningBalanceDetails_IsUsingGLDTablePrefix(bool isDR)
		{
			var creator = new TestObjectCreator(Factory);
			var reportConfiguration = creator.EnsureComplianceReportConfigInRegistry("TST", "ABN",
				PeriodicityCodes.AccountingPeriod,
				ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.AllTransactions,
				ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBook);
			var reportConfigurations = new ComplianceReportConfigurationCollection();
			reportConfigurations.Add(reportConfiguration);

			var gldReportConfig = reportConfigurations.AddNew();
			gldReportConfig.ReportCode = "TSG";
			gldReportConfig.ReportTitle = "Test Tax Report";
			gldReportConfig.ReportBaseTablePrefix = ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.GeneralLedgerData;
			gldReportConfig.ReportPeriodicity = PeriodicityCodes.AccountingPeriod;
			gldReportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gldReportConfig.TaxRegistrationType = "ABN";
			gldReportConfig.ReportLineGrouping = ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBook;

			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations))
			using (ObjectFactory.Substitute(SetGldComplianceReportUsingEDWFeatureControl(true)))
			{
				var accountPK = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "1000.00.00")).PK;
				AccountingConfigurationRegistry.Instance.PLAppropriationAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK.ToGuid());
				var invoice = creator.CreateInvoice(typeof(ARInvoice), "I0001", creator.AUD, 1m, Creator.ABIGAS);
				var report = creator.CreateGLDComplianceReportUsingEDW("TST", AccComplianceReport.Status.ReportGenerated, PeriodicityCodes.AccountingPeriod);
				report.ACR_ReportType = gldReportConfig.ReportCode;
				Factory.Save();

				creator.CreateComplianceReportTransactionPivot(report, invoice);
				Factory.Save();

				using (var connection = Db.NewExtraConnectionWithMainDbCredentials(BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection), Db.SqlMasterDb))
				using (connection.BeginTransactionWithManager())
				{
					report.EDWConnectionForTest = connection;
					PrepareData(connection, isDR);
					var reportDetail = report.GLOpeningBalanceDetails[0];
					AssertEquals(reportDetail.AG_AccountNum, "1000.00.00");
					AssertEquals(reportDetail.AG_AccountType, "P&L");
					AssertEquals(reportDetail.AG_DebitCredit, "DR");
					AssertEquals(reportDetail.AG_PK, accountPK);
					AssertEquals(reportDetail.AG_Description, "ACC1");
					AssertNotEquals(reportDetail.AG_AG_ConsolidationNum, ZGuid.Empty);
					AssertEquals(reportDetail.AG_ConsolidationAccountNum, "1111.00.00");
					AssertEquals(reportDetail.AG_ConsolidationAccountType, "P&L");
					AssertEquals(reportDetail.AG_ConsolidationDescription, "ACC1");
					AssertEquals(reportDetail.GeneralLedgerAmountDR, isDR ? 2.0000m : 0.0000m);
					AssertEquals(reportDetail.GeneralLedgerAmountCR, isDR ? 0.0000m : 2.0000m);
				}
			}

			IFeatureControlManager SetGldComplianceReportUsingEDWFeatureControl(bool isEnabled)
			{
				var mockIFeatureData = new Mock<IFeatureData>();
				var gLDFeatureControlData = new GeneralLedgerDataFeatureControlModel() { EnableGLDComplianceReportUsingEDW = isEnabled };
				var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
				mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out gLDFeatureControlData)).Returns(true);
				mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingGeneralLedgerDataFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

				return mockIFeatureControlManager.Object;
			}

			void PrepareData(DbConnection connection, bool isDR)
			{
				var sqlText = new StringBuilder();
				for (var i = 1; i <= 12; i++)
				{
					sqlText.AppendLine(GetInsertAccPeriodScript(2024, i));
				}

				connection.ExecuteNonQuery(sqlText.ToString());

				connection.ExecuteNonQuery($@"
				INSERT [{Db.EdwDatabaseName}].[Finance].[BAS__StmDataDate]
					([StmDataDateKey], [StmDataDateID], [Name], [CompanyID], [Value], [CompanyKey])
				VALUES
					(
						(SELECT ISNull(MAX(StmDataDateKey), 0) + 1 FROM [{Db.EdwDatabaseName}].[Finance].[BAS__StmDataDate]),
						newid(),
						'JournalEntriesLastProcessedDate',
						(SELECT TOP 1 CompanyID FROM [{Db.EdwDatabaseName}].[Organization].[BAS__Company] WHERE CompanyKey = 1),
						'{new DateTime(2024, 1, 1).ToString("yyyy-MM-dd")}',
						1
					);");

				connection.ExecuteNonQuery(
					$@"INSERT [{Db.EdwDatabaseName}].[Organization].[BAS__Company]([CompanyKey], [CompanyID], [LocalCurrency], [CompanyCode], [IsGSTCashBasis], [IsGSTRegistered] )VALUES(1, '{Env.CurrentCompany.PK}', 'CNY', 'DCN', 1, 1);");

				connection.ExecuteNonQuery($@"
				INSERT [{Db.EdwDatabaseName}].[Organization].[BAS__Branch]
					([BranchKey], [BranchID], [CompanyKey], [HomePortKey], [BranchCode], [OrganizationKey])
					VALUES
						(1, newid(), 1, 1, 'BRN', 1);");

				connection.ExecuteNonQuery($@"INSERT [{Db.EdwDatabaseName}].[Organization].[BAS__Department]
					([DepartmentKey], [DepartmentID], [Code])
					VALUES
						(1, newid(), 'DEP');");

				var accountPk = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "1000.00.00"))
					.PK;
				connection.ExecuteNonQuery($@"
					INSERT [{Db.EdwDatabaseName}].[Finance].[BAS__GLAccount]
						(GLAccountKey, GLAccountID, AccountTypeCode, AccountNo, AccountGroup, Description, DebitCreditCode, Units, SectionType, ConsolidationAccountKey)
					VALUES
						(1, '{accountPk}', 'P&L', '1000.00.00', 'G', 'ACC1', 'DR', '', 'AR', 2);");

				connection.ExecuteNonQuery($@"
					INSERT [{Db.EdwDatabaseName}].[Finance].[BAS__GLAccount]
						(GLAccountKey, GLAccountID, AccountTypeCode, AccountNo, AccountGroup, Description, DebitCreditCode, Units, SectionType)
					VALUES
						(2, newid(), 'P&L', '1111.00.00', 'G', 'ACC1', 'DR', '', 'AR');");

				int amount = isDR ? 1 : -1;

				connection.ExecuteNonQuery($@"
INSERT [{Db.EdwDatabaseName}].[Finance].[GRP__GeneralLedgerAggregateData]
	([GLAccountKey], [PostPeriod], [GLAmountLocalBalance], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
VALUES
	(1, 202401, {amount}, 1, '', 1, 1);");

				connection.ExecuteNonQuery($@"
					INSERT [{Db.EdwDatabaseName}].[Finance].[BAS__GLAggregate] 
						([GLAggregateKey], [GLAggregateID], [GLAccountKey], [Period], [Amount], [CompanyKey], [TransactionCategory], [DepartmentKey], [BranchKey])
					VALUES
						(1, newid(), 2, 202301, {amount}, 1, 'AR', 1, 1);");
			}

			string GetInsertAccPeriodScript(int year, int month)
			{
				var startDate = new DateTime(year, month, 1).ToString("yyyy-MM-dd");
				var endDate = new DateTime(year, month, 1).AddMonths(1).AddMinutes(-1).ToString("yyyy-MM-dd");
				return $@"
					INSERT {Db.EdwDatabaseName}.Finance.BAS__PeriodManagement
						([PeriodManagementKey], [PeriodManagementID], [CompanyKey], [StartDate], [EndDate],  [Period], [Year])
					VALUES
						(1, newid(), 1, '{startDate}', '{endDate}', {year * 100 + month}, {year});";
			}
		}

		[DeveloperOnlyTest]
		[TestDate(2018, 01, 01)]
		[UseSnapshotProtection(true)]
		public void TestGLMovementDetailsExportWhenDataInEDW_GLDComplianceReportUsingEDWDisabled()
		{
			TestGLDReportGLMovementDetailsExport(false, true, false);
		}

		[DeveloperOnlyTest]
		[TestDate(2018, 01, 01)]
		[UseSnapshotProtection(true)]
		public void TestGLMovementDetailsExportWhenDataInEDW_GLDComplianceReportUsingEDWEnabled()
		{
			TestGLDReportGLMovementDetailsExport(true, true, false);
		}

		[DeveloperOnlyTest]
		[TestDate(2018, 01, 01)]
		public void TestGLMovementDetailsExportWhenDataNotInEDW_GLDComplianceReportUsingEDWDisabled()
		{
			TestGLDReportGLMovementDetailsExport(false, false, false);
		}

		[DeveloperOnlyTest]
		[TestDate(2018, 01, 01)]
		public void TestGLMovementDetailsExportWhenDataNotInEDW_GLDComplianceReportUsingEDWEnabled()
		{
			TestGLDReportGLMovementDetailsExport(true, false, true);
		}

		[DeveloperOnlyTest]
		[TestDate(2023, 01, 1)]
		[SuspendCriticalValidation]
		[UseSnapshotProtection(true)]
		public void TestGLDReportLinesExportWhenDataInEDW_GLDComplianceReportUsingEDWDisabled()
		{
			TestGLDReportLinesExport(false, true, false);
		}

		[DeveloperOnlyTest]
		[TestDate(2023, 01, 1)]
		[SuspendCriticalValidation]
		[UseSnapshotProtection(true)]
		public void TestGLDReportLinesExportWhenDataInEDW_GLDComplianceReportUsingEDWEnabled()
		{
			TestGLDReportLinesExport(true, true, false);
		}

		[TestDate(2024, 01, 01, 01, 01, 01)]
		public void TestGLOpeningBalanceDetails_EdwConnectionIsNull()
		{
			using (BiServers.TemporarilySetDataWarehouseServerToNull())
			using (ObjectFactory.Substitute(SetGldComplianceReportUsingEDWFeatureControl(true)))
			{
				var creator = new TestObjectCreator(Factory);
				var reportConfiguration = creator.EnsureComplianceReportConfigInRegistry("TST", "ABN", PeriodicityCodes.AccountingPeriod, ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.AllTransactions, ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBook);
				var reportConfigurations = new ComplianceReportConfigurationCollection();
				reportConfigurations.Add(reportConfiguration);

				var gldReportConfig = reportConfigurations.AddNew();
				gldReportConfig.ReportCode = "TSG";
				gldReportConfig.ReportTitle = "Test Tax Report";
				gldReportConfig.ReportBaseTablePrefix = ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.GeneralLedgerData;
				gldReportConfig.ReportPeriodicity = PeriodicityCodes.AccountingPeriod;
				gldReportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				gldReportConfig.TaxRegistrationType = "ABN";
				gldReportConfig.ReportLineGrouping = ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBook;

				using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations))
				{
					var accountPK = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "1000.00.00")).PK;
					AccountingConfigurationRegistry.Instance.PLAppropriationAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK.ToGuid());
					var invoice = creator.CreateInvoice(typeof(ARInvoice), "I0001", creator.AUD, 1m, Creator.ABIGAS);
					var report = creator.CreateGLDComplianceReportUsingEDW("TST", AccComplianceReport.Status.ReportGenerated, PeriodicityCodes.AccountingPeriod);
					report.ACR_ReportType = gldReportConfig.ReportCode;
					Factory.Save();

					creator.CreateComplianceReportTransactionPivot(report, invoice);
					Factory.Save();

					var reportDetail = report.GLOpeningBalanceDetails[0];
					AssertNull(reportDetail);
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestGetLastDateTransformRunUtc()
		{
			var report = Report as AccGLDComplianceReportUsingEDW;
			var sAFTSingleXmlExportedEventLog = (Report as AccGLDComplianceReportUsingEDW).SAFTSingleXmlExportedEventLog;
			var sAFTAnnualXmlExportedEventLog = (Report as AccGLDComplianceReportUsingEDW).SAFTAnnualXmlExportedEventLog;

			AssertEquals("Purpose: SAFT Single XML Exported. This report is generated from EDW report data source last updated at .", report.SAFTSingleXmlExportedEventLog);
			AssertEquals("Purpose: SAFT Annual XML Exported. This report is generated from EDW report data source last updated at .", report.SAFTAnnualXmlExportedEventLog);

			var edwServerName = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
			var edWConnection = !string.IsNullOrEmpty(edwServerName) ? Db.NewExtraConnectionWithMainDbCredentials(edwServerName, Db.EdwDatabaseName) : null;
			AssertNotNull(edWConnection);

			var sql = string.Format("INSERT INTO {0}.biadmin.MasterState (ParamName, ParamValue) VALUES ('LAST_DATE_TRANSFORM_RUN_UTC', '2024-01-01 00:00:00.000')", Db.EdwDatabaseName);
			var cmd = edWConnection.Command(sql);
			_ = cmd.ExecuteScalar();
			var result = (Report as AccGLDComplianceReportUsingEDW).SAFTSingleXmlExportedEventLog;
			AssertEquals("Purpose: SAFT Single XML Exported. This report is generated from EDW report data source last updated at 2024-01-01 00:00.", report.SAFTSingleXmlExportedEventLog);
			AssertEquals("Purpose: SAFT Annual XML Exported. This report is generated from EDW report data source last updated at 2024-01-01 00:00.", report.SAFTAnnualXmlExportedEventLog);
		}

		public override void TestReportLinesAndTotals_FromAllTransactionsDayBookWithoutGrouping()
		{
			Assert("Not suitable as this class is only for GLD EDW", true);
		}

		public override void TestReportLinesExportWhenTablePrefixIsAll()
		{
			Assert("Not suitable as this class is only for GLD EDW", true);
		}

		public override void TestReportOpeningGLBalanceDetailsDetectDRCRBasedOnBalanceSign_AllTransactionsDayBookWithoutGrouping()
		{
			Assert("Not suitable as this class is only for GLD EDW", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<AccGLDComplianceReportUsingEDW>();
		}

		protected override Type BusinessObjectType => typeof(AccGLDComplianceReportUsingEDW);
	}
}
