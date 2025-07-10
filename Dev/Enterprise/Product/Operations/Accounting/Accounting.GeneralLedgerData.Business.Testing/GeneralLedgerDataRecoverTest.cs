using System;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class GeneralLedgerDataRecoverTest : TestCaseWithFactory
	{
		[SuspendCriticalValidation]
		[TestDate(2023, 10, 20)]
		public void TestRecoverPartOfGLDForPks()
		{
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateAPSuspenseControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateInputTaxReceivablePendingAccount().PK.ToGuid());
			var apInvoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1, 100, 10, 100, 10);
			apInvoice.AH_FullyPaidDate = ZDateTime.Today;
			var line = apInvoice.Lines[0];
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;

			var cashBasisVAT = TestObjectCreator.CreateCashBasisVAT(line, -90, -9);

			var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxTransaction.ATT_AH = apInvoice.PK;
			taxTransaction.ATT_LocalTaxAmount = 100m;
			taxTransaction.ATT_OSTaxAmount = 200m;
			taxTransaction.ATT_AT_TaxID = Factory.NewWithValidTestData<AccTaxRate>().PK;
			taxTransaction.ATT_Basis = "MAT";
			taxTransaction.ATT_RateNumerator = 2;
			taxTransaction.ATT_RateDenominator = 1;
			taxTransaction.ATT_A9_TaxMessage = Factory.NewWithValidTestData<AccInvMsg>().PK;
			taxTransaction.ATT_GB = GlbBranch.CurrentBranch.PK;
			taxTransaction.ATT_GC = GlbCompany.CurrentCompany.PK;
			taxTransaction.ATT_GE_Department = GlbDepartment.CurrentDepartment.PK;
			taxTransaction.ATT_PostDate = ZDate.Today;
			taxTransaction.ATT_AG_LedgerControlAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;
			taxTransaction.ATT_AG_TaxPendingControlAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;

			var apJournal = TestObjectCreator.CreateJournal<APJournal>(100m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
			Factory.Save();

			var taxGLMovement = Factory.LoadTop1<AccTaxGLMovement>(new ZQuery(AccTaxGLMovementSchema.ATM_ATT_TaxTransaction, taxTransaction.PK));

			var gldDatas = Factory.Load<AccGeneralLedgerData>(new ZQuery());
			AssertEquals(false, gldDatas.Any());

			TestObjectCreator.MockNudgeGLDProcessData([apInvoice.Lines[0], cashBasisVAT, apJournal, taxGLMovement]);

			var newFactory = Factory.CreateNewFactory();
			gldDatas = newFactory.Load<AccGeneralLedgerData>(new ZQuery());
			AssertEquals(12, gldDatas.Length);

			var apInvoiceGld = gldDatas.FirstOrDefault(x => x.GLD_AL_TransactionLine == apInvoice.Lines[0].PK && x.GLD_LocalCreditAmount == 100);
			AssertNotNull(apInvoiceGld);

			var cashBasisVATGld = gldDatas.FirstOrDefault(x => x.GLD_YC_CashBasisVAT == cashBasisVAT.PK && x.GLD_LocalCreditAmount == 9);
			AssertNotNull(cashBasisVATGld);

			var apJournalGld = gldDatas.FirstOrDefault(x => x.GLD_AH_TransactionHeader == apJournal.PK && x.GLD_LocalCreditAmount == 100);
			AssertNotNull(apJournalGld);

			var taxGLMovementsGld = gldDatas.FirstOrDefault(x => x.GLD_ATM_TaxGLMovement == taxGLMovement.PK && x.GLD_LocalCreditAmount == 2000);
			AssertNotNull(taxGLMovementsGld);

			apInvoice.Lines[0].AL_LineAmount = 200;
			cashBasisVAT.YC_TaxAmount = 5;
			apJournal.AH_InvoiceAmount = 1000;
			Factory.Save();
			taxGLMovement.ATM_Amount = 5000;

			MockNudgeGLDRecover([apInvoice.Lines[0]], [apJournal], [cashBasisVAT], [taxGLMovement]);

			newFactory = Factory.CreateNewFactory();
			gldDatas = newFactory.Load<AccGeneralLedgerData>(new ZQuery());
			AssertEquals(12, gldDatas.Length);

			apInvoiceGld = gldDatas.FirstOrDefault(x => x.GLD_AL_TransactionLine == apInvoice.Lines[0].PK && x.GLD_LocalCreditAmount == 200);
			AssertNotNull(apInvoiceGld);

			cashBasisVATGld = gldDatas.FirstOrDefault(x => x.GLD_YC_CashBasisVAT == cashBasisVAT.PK && x.GLD_LocalCreditAmount == 5);
			AssertNotNull(cashBasisVATGld);

			apJournalGld = gldDatas.FirstOrDefault(x => x.GLD_AH_TransactionHeader == apJournal.PK && x.GLD_LocalCreditAmount == 1000);
			AssertNotNull(apJournalGld);

			taxGLMovementsGld = gldDatas.FirstOrDefault(x => x.GLD_ATM_TaxGLMovement == taxGLMovement.PK && x.GLD_LocalCreditAmount == 5000);
			AssertNotNull(taxGLMovementsGld);
		}

		[UseSnapshotProtection(skipTransaction: true)]
		[TestDate(2023, 5, 18)]
		public void TestRecoverPartOfGLD_ConfirmExecuteInOneTransaction()
		{
			var apJournal = TestObjectCreator.CreateJournal<APJournal>(100m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
			Factory.Save();
			var gldDatas = Factory.Load<AccGeneralLedgerData>(new ZQuery());
			AssertEquals(false, gldDatas.Any());

			TestObjectCreator.MockNudgeGLDProcessData([apJournal]);

			gldDatas = Factory.Load<AccGeneralLedgerData>(new ZDBOnlyQuery(typeof(AccGeneralLedgerData)));
			AssertEquals(2, gldDatas.Length);

			var apJournalGld = gldDatas.FirstOrDefault(x => x.GLD_AH_TransactionHeader == apJournal.PK && x.GLD_LocalCreditAmount == 100);
			AssertNotNull(apJournalGld);

			var mockGeneralLedgerDataProcessor = new Mock<IGeneralLedgerDataProcessor>();
			mockGeneralLedgerDataProcessor.Setup(m => m.ProcessData(It.IsAny<DataRow[]>())).Throws(new DatabaseUpgradedException());
			var testGeneralLedgerDataRecover = new GeneralLedgerDataRecover();
			var field = typeof(GeneralLedgerDataRecover).GetField("generalLedgerDataProcessor", BindingFlags.Instance | BindingFlags.NonPublic);
			field.SetValue(testGeneralLedgerDataRecover, mockGeneralLedgerDataProcessor.Object);

			AssertExceptionThrown<DatabaseUpgradedException>(() => testGeneralLedgerDataRecover.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, new[] { apJournal }));

			mockGeneralLedgerDataProcessor.Verify(x => x.ProcessData(It.IsAny<DataRow[]>()), Times.Once);

			var gldDatasAfterRollBack = Factory.Load<AccGeneralLedgerData>(new ZDBOnlyQuery(typeof(AccGeneralLedgerData)));
			AssertEquals(2, gldDatasAfterRollBack.Length);
			AssertArrayEqualsByElements(gldDatas.Select(x => x.PK).ToArray(), gldDatasAfterRollBack.Select(x => x.PK).ToArray());
		}

		[SuspendCriticalValidation]
		[TestDate(2023, 10, 20)]
		public void TestRecoverPartOfGLDForPks_WhenTransactionsAreReceivables()
		{
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateARSuspenseControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateInputTaxReceivablePendingAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateOutputTaxPayablePendingAccount().PK.ToGuid());
			var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1, 100, 10, 100, 10);
			arInvoice.AH_FullyPaidDate = ZDateTime.Today;
			var line = arInvoice.Lines[0];
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;

			var cashBasisVAT = TestObjectCreator.CreateCashBasisVAT(line, -90, -9);

			var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxTransaction.ATT_AH = arInvoice.PK;
			taxTransaction.ATT_LocalTaxAmount = 100m;
			taxTransaction.ATT_OSTaxAmount = 200m;
			taxTransaction.ATT_AT_TaxID = Factory.NewWithValidTestData<AccTaxRate>().PK;
			taxTransaction.ATT_Basis = "MAT";
			taxTransaction.ATT_RateNumerator = 2;
			taxTransaction.ATT_RateDenominator = 1;
			taxTransaction.ATT_A9_TaxMessage = Factory.NewWithValidTestData<AccInvMsg>().PK;
			taxTransaction.ATT_GB = GlbBranch.CurrentBranch.PK;
			taxTransaction.ATT_GC = GlbCompany.CurrentCompany.PK;
			taxTransaction.ATT_GE_Department = GlbDepartment.CurrentDepartment.PK;
			taxTransaction.ATT_PostDate = ZDate.Today;
			taxTransaction.ATT_AG_LedgerControlAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;
			taxTransaction.ATT_AG_TaxPendingControlAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;

			var arJournal = TestObjectCreator.CreateJournal<ARJournal>(100m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
			Factory.Save();

			var taxGLMovement = Factory.LoadTop1<AccTaxGLMovement>(new ZQuery(AccTaxGLMovementSchema.ATM_ATT_TaxTransaction, taxTransaction.PK));

			var gldDatas = Factory.Load<AccGeneralLedgerData>(new ZQuery());
			AssertEquals(false, gldDatas.Any());

			TestObjectCreator.MockNudgeGLDProcessData([arInvoice.Lines[0], cashBasisVAT, arJournal, taxGLMovement]);

			var newFactory = Factory.CreateNewFactory();
			gldDatas = newFactory.Load<AccGeneralLedgerData>(new ZQuery());
			AssertEquals(12, gldDatas.Length);

			var apInvoiceGld = gldDatas.FirstOrDefault(x => x.GLD_AL_TransactionLine == arInvoice.Lines[0].PK && x.GLD_LocalCreditAmount == 100);
			AssertNotNull(apInvoiceGld);

			var cashBasisVATGld = gldDatas.FirstOrDefault(x => x.GLD_YC_CashBasisVAT == cashBasisVAT.PK && x.GLD_LocalCreditAmount == 9);
			AssertNotNull(cashBasisVATGld);

			var arJournalGld = gldDatas.FirstOrDefault(x => x.GLD_AH_TransactionHeader == arJournal.PK && x.GLD_LocalCreditAmount == 100);
			AssertNotNull(arJournalGld);

			var taxGLMovementsGld = gldDatas.FirstOrDefault(x => x.GLD_ATM_TaxGLMovement == taxGLMovement.PK && x.GLD_LocalCreditAmount == 2000);
			AssertNotNull(taxGLMovementsGld);

			arInvoice.Lines[0].AL_LineAmount = 200;
			cashBasisVAT.YC_TaxAmount = 5;
			arJournal.AH_InvoiceAmount = 1000;
			Factory.Save();

			taxGLMovement.ATM_Amount = 5000;

			MockNudgeGLDRecover([arInvoice.Lines[0]], [arJournal], [cashBasisVAT], [taxGLMovement]);

			newFactory = Factory.CreateNewFactory();
			gldDatas = newFactory.Load<AccGeneralLedgerData>(new ZQuery());
			AssertEquals(12, gldDatas.Length);

			apInvoiceGld = gldDatas.FirstOrDefault(x => x.GLD_AL_TransactionLine == arInvoice.Lines[0].PK && x.GLD_LocalCreditAmount == 200);
			AssertNotNull(apInvoiceGld);

			cashBasisVATGld = gldDatas.FirstOrDefault(x => x.GLD_YC_CashBasisVAT == cashBasisVAT.PK && x.GLD_LocalCreditAmount == 5);
			AssertNotNull(cashBasisVATGld);

			arJournalGld = gldDatas.FirstOrDefault(x => x.GLD_AH_TransactionHeader == arJournal.PK && x.GLD_LocalCreditAmount == 1000);
			AssertNotNull(arJournalGld);

			taxGLMovementsGld = gldDatas.FirstOrDefault(x => x.GLD_ATM_TaxGLMovement == taxGLMovement.PK && x.GLD_LocalCreditAmount == 5000);
			AssertNotNull(taxGLMovementsGld);
		}

		[SuspendCriticalValidation]
		[TestDate(2023, 10, 20)]

		public void TestHandleGLDComplianceReport_WhenGLDRecovery()
		{
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateAPSuspenseControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateInputTaxReceivablePendingAccount().PK.ToGuid());
			var apInvoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1, 100, 10, 100, 10);
			apInvoice.AH_FullyPaidDate = ZDateTime.Today;
			var line = apInvoice.Lines[0];
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;

			var cashBasisVAT = TestObjectCreator.CreateCashBasisVAT(line, -90, -9);

			var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxTransaction.ATT_AH = apInvoice.PK;
			taxTransaction.ATT_LocalTaxAmount = 100m;
			taxTransaction.ATT_OSTaxAmount = 200m;
			taxTransaction.ATT_AT_TaxID = Factory.NewWithValidTestData<AccTaxRate>().PK;
			taxTransaction.ATT_Basis = "MAT";
			taxTransaction.ATT_RateNumerator = 2;
			taxTransaction.ATT_RateDenominator = 1;
			taxTransaction.ATT_A9_TaxMessage = Factory.NewWithValidTestData<AccInvMsg>().PK;
			taxTransaction.ATT_GB = GlbBranch.CurrentBranch.PK;
			taxTransaction.ATT_GC = GlbCompany.CurrentCompany.PK;
			taxTransaction.ATT_GE_Department = GlbDepartment.CurrentDepartment.PK;
			taxTransaction.ATT_PostDate = ZDate.Today;
			taxTransaction.ATT_AG_LedgerControlAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;
			taxTransaction.ATT_AG_TaxPendingControlAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;

			var apJournal = TestObjectCreator.CreateJournal<APJournal>(100m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
			Factory.Save();

			var taxGLMovement = Factory.LoadTop1<AccTaxGLMovement>(new ZQuery(AccTaxGLMovementSchema.ATM_ATT_TaxTransaction, taxTransaction.PK));

			var gldDatas = Factory.Load<AccGeneralLedgerData>(new ZQuery());
			AssertEquals(false, gldDatas.Any());

			TestObjectCreator.MockNudgeGLDProcessData([apInvoice.Lines[0], cashBasisVAT, apJournal, taxGLMovement]);

			var newFactory = Factory.CreateNewFactory();
			gldDatas = newFactory.Load<AccGeneralLedgerData>(new ZQuery());
			AssertEquals(12, gldDatas.Length);

			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = "TST";
			reportConfig.ReportTitle = "Test Tax Report";
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
			reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
			reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			reportConfig.TaxRegistrationType = "ABN";
			reportConfig.ReportLineGrouping = ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.TaxReporting;

			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceConfig);

			var complianceReportFIN = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReportFIN.ACR_ReportType = "TST";
			complianceReportFIN.ACR_DateFrom = ZDate.Today.AddDays(-2);
			complianceReportFIN.ACR_DateTo = ZDate.Today.AddDays(2);
			complianceReportFIN.ACR_Status = AccComplianceReport.Status.ReportFinalised;

			var complianceReportGEN = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReportGEN.ACR_ReportType = "TST";
			complianceReportGEN.ACR_DateFrom = ZDate.Today.AddDays(-2);
			complianceReportGEN.ACR_DateTo = ZDate.Today.AddDays(2);
			complianceReportGEN.ACR_Status = AccComplianceReport.Status.ReportGenerated;

			var complianceReportADD = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReportADD.ACR_ReportType = "TST";
			complianceReportADD.ACR_DateFrom = ZDate.Today.AddDays(-2);
			complianceReportADD.ACR_DateTo = ZDate.Today.AddDays(2);
			complianceReportADD.ACR_Status = AccComplianceReport.Status.ReportCreated;

			Factory.Save();

			var headerGLD = newFactory.LoadTop1<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, apJournal.PK));
			var lineGLD = newFactory.LoadTop1<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, apInvoice.Lines[0].PK));
			var sql = $@"INSERT INTO AccComplianceReportTransactionPivot (ACL_PK, ACL_ParentID, ACL_ParentTableCode, ACL_GC_Company, ACL_ACR_Report, ACL_ReportSequence) VALUES
(NEWID(), '{headerGLD.PK}', 'GLD', '{GlbCompany.CurrentCompany.PK}', '{complianceReportFIN.PK}', 1),
(NEWID(), '{lineGLD.PK}', 'GLD', '{GlbCompany.CurrentCompany.PK}','{complianceReportGEN.PK}', 2),
(NEWID(), NEWID(), 'AL', '{GlbCompany.CurrentCompany.PK}', '{complianceReportADD.PK}', 3)
";

			TestConnection.ExecuteNonQuery(sql);

			MockNudgeGLDRecover([apInvoice.Lines[0]], [apJournal], [cashBasisVAT], [taxGLMovement]);

			newFactory = Factory.CreateNewFactory();
			var complianceReports = newFactory.Load<AccComplianceReport>(new ZQuery());
			AssertEquals(false, complianceReports.Any(x => x.ACR_Status == AccComplianceReport.Status.ReportFinalised));

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM AccComplianceReportTransactionPivot");
			AssertEquals(1, result.Rows.Count);
			AssertNotEquals("GLD", result.Rows[0]["ACL_ParentTableCode"]);
		}

		void MockNudgeGLDRecover(BusinessObject[] lines, BusinessObject[] headers, BusinessObject[] cashBasisVATs, BusinessObject[] taxGLMovements)
		{
			var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
			{
				var generalLedgerDataRecover = new GeneralLedgerDataRecover();
				generalLedgerDataRecover.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, lines);
				generalLedgerDataRecover.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, headers);
				generalLedgerDataRecover.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_YC_CashBasisVAT, cashBasisVATs);
				generalLedgerDataRecover.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_ATM_TaxGLMovement, taxGLMovements);
			}
		}

		protected override void SetUp()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(GlbCompany.CurrentCompany, 2021);
			TestObjectCreator.CreateTestPeriodsForEntireYear(GlbCompany.CurrentCompany, 2022);
			TestObjectCreator.CreateTestPeriodsForEntireYear(GlbCompany.CurrentCompany, 2023);
			Factory.Save();
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
