using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class GeneralLedgerDataScriptHelperTest : TestCaseWithFactory
	{
		public void TestGetEmptyTVPGeneralLedgerData()
		{
			var tvpGeneralLedgerData = GeneralLedgerDataScriptHelper.GetEmptyTVPGeneralLedgerData();

			AssertEquals(tvpGeneralLedgerData.TableName, "dbo.TVP_GeneralLedgerData");
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_GC_Company));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_PostDate));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_PostPeriod));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_AG_GLAccount));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_OSDebitAmount));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_OSCreditAmount));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_LocalDebitAmount));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_LocalCreditAmount));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_AH_TransactionHeader));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_AL_TransactionLine));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_GB_Branch));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_GB_TaxBranch));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_GE_Department));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_Currency));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_ExchangeRate));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_YC_CashBasisVAT));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_ATM_TaxGLMovement));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_GLAccountType));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_Type));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_JournalEntriesNumber));
			Assert(tvpGeneralLedgerData.Columns.Contains(AccGeneralLedgerDataSchema.Constants.GLD_JournalEntriesNumberRuleCode));
		}

		public void TestPopulateTVPGeneralLedgerData()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var tvpGeneralLedgerData = GeneralLedgerDataScriptHelper.GetEmptyTVPGeneralLedgerData();
			var dRLine = new DebitCreditEntryItem()
			{
				AccountPK = testObjectCreator.GLHeader1.PK,
				DRCRSign = DebitCredit.DR,
				GLDAccountType = GLDAccountTypes.APControlAccount,
				GLDType = AccountingConstants.GLDTypeCodes.Posting,
				JournalDate = new ZDateTime(2023, 6, 29),
				LocalAmount = 10m,
				OSAmount = 20m,
				Period = 202306
			};

			var cRLine = new DebitCreditEntryItem()
			{
				AccountPK = testObjectCreator.GLHeader2.PK,
				DRCRSign = DebitCredit.CR,
				GLDAccountType = GLDAccountTypes.ARControlAccount,
				GLDType = AccountingConstants.GLDTypeCodes.Recognition,
				JournalDate = new ZDateTime(2023, 5, 29),
				LocalAmount = 40m,
				OSAmount = 30m,
				Period = 202305
			};

			var headerPK = Guid.NewGuid();
			var linePK = Guid.NewGuid();
			var cashBasisVatPK = Guid.NewGuid();
			var taxGLMovementPK = Guid.NewGuid();
			var entry = new DebitCreditEntry()
			{
				TransactionHeaderPK = headerPK,
				TransactionLinePK = linePK,
				CashBasisVatPK = cashBasisVatPK,
				TaxGLMovementPK = taxGLMovementPK,
				Currency = "AUD",
				ExchangeRate = 1,
				CompanyPK = GlbCompany.CurrentCompany.PK.ToGuid(),
				BranchPK = GlbBranch.CurrentBranch.PK.ToGuid(),
				TaxBranchPK = testObjectCreator.NonCurrentBranch.PK.ToGuid(),
				DepartmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid(),
				EntryItems = new[] { dRLine, cRLine }
			};

			GeneralLedgerDataScriptHelper.PopulateTVPGeneralLedgerData(tvpGeneralLedgerData, entry, ZString.Empty, ZString.Empty);

			AssertEquals(tvpGeneralLedgerData.Rows.Count, 2);

			foreach (var line in entry.EntryItems)
			{
				Assert(tvpGeneralLedgerData.Rows.Cast<DataRow>().Any(row =>
					(Guid)row[AccGeneralLedgerDataSchema.Constants.GLD_AG_GLAccount] == line.AccountPK.ToGuid() &&
					(string)row[AccGeneralLedgerDataSchema.Constants.GLD_Type] == line.GLDType &&
					(string)row[AccGeneralLedgerDataSchema.Constants.GLD_GLAccountType] == line.GLDAccountType &&
					(Guid)row[AccGeneralLedgerDataSchema.Constants.GLD_AH_TransactionHeader] == entry.TransactionHeaderPK &&
					(Guid)row[AccGeneralLedgerDataSchema.Constants.GLD_AL_TransactionLine] == entry.TransactionLinePK &&
					(Guid)row[AccGeneralLedgerDataSchema.Constants.GLD_ATM_TaxGLMovement] == entry.TaxGLMovementPK &&
					(Guid)row[AccGeneralLedgerDataSchema.Constants.GLD_YC_CashBasisVAT] == entry.CashBasisVatPK &&
					(string)row[AccGeneralLedgerDataSchema.Constants.GLD_Currency] == entry.Currency &&
					(decimal)row[AccGeneralLedgerDataSchema.Constants.GLD_ExchangeRate] == entry.ExchangeRate &&
					(Guid)row[AccGeneralLedgerDataSchema.Constants.GLD_GB_Branch] == entry.BranchPK &&
					(Guid)row[AccGeneralLedgerDataSchema.Constants.GLD_GC_Company] == entry.CompanyPK &&
					(Guid)row[AccGeneralLedgerDataSchema.Constants.GLD_GB_TaxBranch] == entry.TaxBranchPK &&
					(Guid)row[AccGeneralLedgerDataSchema.Constants.GLD_GE_Department] == entry.DepartmentPK &&
					(DateTime)row[AccGeneralLedgerDataSchema.Constants.GLD_PostDate] == line.JournalDate.ToDateTime() &&
					(int)row[AccGeneralLedgerDataSchema.Constants.GLD_PostPeriod] == line.Period &&
					string.IsNullOrEmpty((string)row[AccGeneralLedgerDataSchema.Constants.GLD_JournalEntriesNumber]) &&
					string.IsNullOrEmpty((string)row[AccGeneralLedgerDataSchema.Constants.GLD_JournalEntriesNumberRuleCode])));
			}
		}

		public void TestPopulateTVPGeneralLedgerData_LineBranchDepartmentSet()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var tvpGeneralLedgerData = GeneralLedgerDataScriptHelper.GetEmptyTVPGeneralLedgerData();
			var dRLine = new DebitCreditEntryItem()
			{
				AccountPK = testObjectCreator.GLHeader1.PK,
				JournalDate = ZDateTime.Today,
				BranchPK = testObjectCreator.NonCurrentBranch.PK.ToGuid(),
				DepartmentPK = testObjectCreator.NonCurrentDepartment.PK.ToGuid()
			};

			var entry = new DebitCreditEntry()
			{
				BranchPK = GlbBranch.CurrentBranch.PK.ToGuid(),
				DepartmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid(),
				EntryItems = new[] { dRLine }
			};
			GeneralLedgerDataScriptHelper.PopulateTVPGeneralLedgerData(tvpGeneralLedgerData, entry, ZString.Empty, ZString.Empty);
			Assert(tvpGeneralLedgerData.Rows.Cast<DataRow>().All(x =>
				(Guid)x[AccGeneralLedgerDataSchema.Constants.GLD_GB_Branch] == testObjectCreator.NonCurrentBranch.PK.ToGuid() &&
				(Guid)x[AccGeneralLedgerDataSchema.Constants.GLD_GE_Department] == testObjectCreator.NonCurrentDepartment.PK.ToGuid()));
		}

		public void TestExcuteUpdateGLDPeriodScript()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);
			TestObjectCreator.CreateTestPeriodsForEntireYear(TestObjectCreator.NonCurrentCompany, 2022);

			SetUpControlAccount();

			var transaction1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1100", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
			transaction1.AH_PostDate = new ZDateTime(2022, 12, 5, 15, 0, 0);

			var transaction2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1100", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
			transaction2.AH_PostDate = new ZDateTime(2022, 12, 1, 15, 0, 0);

			var transaction3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1100", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
			transaction3.AH_PostDate = new ZDateTime(2022, 12, 5, 15, 0, 0);
			transaction3.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			transaction3.AH_GB = TestObjectCreator.NonCurrentCompany.FirstActiveBranch.PK;
			transaction3.Lines[0].AL_GC = TestObjectCreator.NonCurrentCompany.PK;
			transaction3.Lines[0].AL_GB = TestObjectCreator.NonCurrentCompany.FirstActiveBranch.PK;

			Factory.Save();

			ProcessGeneralLedgerData(transaction1.Lines[0]);
			ProcessGeneralLedgerData(transaction2.Lines[0]);
			ProcessGeneralLedgerData(transaction3.Lines[0]);

			var gldDatasBeforeUpdate1 = Factory.Load<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, transaction1.PK));
			var gldDatasBeforeUpdate2 = Factory.Load<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, transaction2.PK));
			var gldDatasBeforeUpdate3 = Factory.Load<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, transaction3.PK));

			Assert(gldDatasBeforeUpdate1.All(x => x.GLD_PostPeriod == 202212));
			Assert(gldDatasBeforeUpdate2.All(x => x.GLD_PostPeriod == 202212));
			Assert(gldDatasBeforeUpdate3.All(x => x.GLD_PostPeriod == 202212));

			var newFactory = new BusinessObjectFactory();

			var periodsNovemberForCurrentCompany = newFactory.Load<AccPeriodManagement>(new ZQuery().AddToFilter(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK).AddToFilter(AccPeriodManagementSchema.AM_Period, 202211));
			var periodsNovemberForNonCurrentCompany = newFactory.Load<AccPeriodManagement>(new ZQuery().AddToFilter(AccPeriodManagementSchema.AM_GC_Company, TestObjectCreator.NonCurrentCompany.PK).AddToFilter(AccPeriodManagementSchema.AM_Period, 202211));
			periodsNovemberForCurrentCompany[0].AM_EndDate = new ZDateTime(2022, 12, 05, 23, 59, 0);
			periodsNovemberForNonCurrentCompany[0].AM_EndDate = new ZDateTime(2022, 12, 05, 23, 59, 0);
			var periodsDecemberForCurrentCompany = newFactory.Load<AccPeriodManagement>(new ZQuery().AddToFilter(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK).AddToFilter(AccPeriodManagementSchema.AM_Period, 202212));
			var periodsDecemberForNonCurrentCompany = newFactory.Load<AccPeriodManagement>(new ZQuery().AddToFilter(AccPeriodManagementSchema.AM_GC_Company, TestObjectCreator.NonCurrentCompany.PK).AddToFilter(AccPeriodManagementSchema.AM_Period, 202212));
			periodsDecemberForCurrentCompany[0].AM_StartDate = new ZDateTime(2022, 12, 06, 0, 0, 0);
			periodsDecemberForNonCurrentCompany[0].AM_StartDate = new ZDateTime(2022, 12, 06, 0, 0, 0);

			newFactory.Save();

			GeneralLedgerDataScriptHelper.ExecuteUpdateGLDPeriodScript(new ZDateTime(2022, 12, 04, 0, 0, 0), new ZDateTime(2022, 12, 6, 23, 59, 00));

			var gldDatasAfterUpdate1 = newFactory.Load<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, transaction1.PK));
			var gldDatasAfterUpdate2 = newFactory.Load<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, transaction2.PK));
			var gldDatasAfterUpdate3 = newFactory.Load<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, transaction3.PK));

			Assert(gldDatasAfterUpdate1.All(x => x.GLD_PostPeriod == 202211));
			Assert(gldDatasAfterUpdate2.All(x => x.GLD_PostPeriod == 202212));
			Assert(gldDatasAfterUpdate3.All(x => x.GLD_PostPeriod == 202212));

			GeneralLedgerDataScriptHelper.ExecuteUpdateGLDPeriodScript(ZDateTime.Empty, ZDateTime.Empty);

			gldDatasAfterUpdate1 = new BusinessObjectFactory().Load<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, transaction1.PK));
			gldDatasAfterUpdate2 = new BusinessObjectFactory().Load<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, transaction2.PK));
			gldDatasAfterUpdate3 = new BusinessObjectFactory().Load<AccGeneralLedgerData>(new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, transaction3.PK));

			Assert(gldDatasAfterUpdate1.All(x => x.GLD_PostPeriod == 202211));
			Assert(gldDatasAfterUpdate2.All(x => x.GLD_PostPeriod == 202211));
			Assert(gldDatasAfterUpdate3.All(x => x.GLD_PostPeriod == 202212));
		}

		public void TestPurgeGeneralLedgerDataRegistry()
		{
			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();

			InsertGeneralLedgerDataRegistry(currentCompanyPK, "GenerateJournalEntriesCDCStartDate");
			InsertGeneralLedgerDataRegistry(currentCompanyPK, "GenerateJournalEntriesStartDate");
			InsertGeneralLedgerDataRegistry(currentCompanyPK, "JournalEntriesLastProcessedDate");
			InsertGeneralLedgerDataRegistry(currentCompanyPK, "JournalEntriesLastQueuedDate");
			InsertGeneralLedgerDataRegistry(currentCompanyPK, "GenerateAndStoreJournalEntriesForPostedAccountingTransactions");

			GeneralLedgerDataScriptHelper.PurgeGeneralLedgerData();

			var selectGLRegistry =
				@"
				SELECT COUNT(*) FROM dbo.StmData
				WHERE SD_Owner = @CompanyPK 
				AND SD_Name IN (@CdcStartDateName, @GldStartDateName, @LastProcessDateName, @LastQueueDateName, @GenerateAndStoreJournalEntriesForPostedAccountingTransactions)";
			using (var command = Db.Connection.Command(selectGLRegistry))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, currentCompanyPK);
				command.AddParameter("@CdcStartDateName", SqlDbType.VarChar, "GenerateJournalEntriesCDCStartDate");
				command.AddParameter("@GldStartDateName", SqlDbType.VarChar, "GenerateJournalEntriesStartDate");
				command.AddParameter("@LastProcessDateName", SqlDbType.VarChar, "JournalEntriesLastProcessedDate");
				command.AddParameter("@LastQueueDateName", SqlDbType.VarChar, "JournalEntriesLastQueuedDate");
				command.AddParameter("@GenerateAndStoreJournalEntriesForPostedAccountingTransactions", SqlDbType.VarChar, "GenerateAndStoreJournalEntriesForPostedAccountingTransactions");

				var ob = command.ExecuteScalar();
				AssertEquals("Total GL Registry Enabled after Purge", 0, Convert.ToInt32(ob));
			}
		}

		void InsertGeneralLedgerDataRegistry(Guid currentCompanyPK, String sdName)
		{
			var sql =
				@"
				DELETE FROM dbo.StmData WHERE SD_Owner = @CompanyPK AND SD_Name = @sdName
				INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_PreserveTestValue, SD_BinaryValue, SD_SystemCreateTimeUtc)
				VALUES (newid(), @SdName, @CompanyPK, 0, @BinaryVal, GETUTCDATE())";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, currentCompanyPK);
				command.AddParameter("@BinaryVal", SqlDbType.Binary, System.Text.Encoding.Unicode.GetBytes(Db.ServerName));
				command.AddParameter("@SdName", SqlDbType.VarChar, sdName);
				command.ExecuteNonQuery();
			}
		}

		void ProcessGeneralLedgerData(INeedRow gLDSource)
		{
			gLDSource.Row.SetAdded();
			AssertNotNull(GeneralLedgerDataProcessor);

			TestObjectCreator.MockNudgeGLDProcessData([gLDSource.Row]);
		}

		void SetUpControlAccount()
		{
			TestCaseHelper.ClearTable(AccGeneralLedgerData.Schema.TableName);
			var account1 = TestObjectCreator.CreateARControlAccount();
			var account2 = TestObjectCreator.CreateARSuspenseControlAccount();
			var account3 = TestObjectCreator.CreateAPSuspenseControlAccount();
			var account4 = TestObjectCreator.CreateJobRevenueJournalControlAccount();
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, account1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, account2.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, account3.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, account4.PK.ToGuid());
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;

		IGeneralLedgerDataProcessor GeneralLedgerDataProcessor
		{
			get
			{
				if (fGeneralLedgerDataProcessor == null)
				{
					fGeneralLedgerDataProcessor = new GeneralLedgerDataProcessor();
				}
				return fGeneralLedgerDataProcessor;
			}
		}
		IGeneralLedgerDataProcessor fGeneralLedgerDataProcessor;
	}
}
