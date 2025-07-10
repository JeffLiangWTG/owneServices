using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public class JournalEntriesNumberFountainWrapperTest : TestCaseWithFactory
	{
		[TestDate(2024, 11, 20)]
		public void TestGenerateNumberMustContainAccountingYear()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2024);
			TestObjectCreator.CreateTestPeriodsForEntireYear(2025);
			TestObjectCreator.SetTemporaryControlAccounts();

			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN,
				NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL
			};

			var accountingYearSetting = journalEntriesNumberCustomisationSetting.NumberSequenceCustomisations.Cast<JournalEntriesNumberCustomisation>().FirstOrDefault(x => x.ElementName == TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits);
			accountingYearSetting.Include = false;
			accountingYearSetting.Fountain = false;

			AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, journalEntriesNumberCustomisationSetting);

			var generalLedgerCombinedDataSource1 = new GeneralLedgerCombinedDataSource(Guid.NewGuid(), new ZDateTime(2024, 01, 01), "AR", "INV", "00001", Env.CurrentBranchPK, Env.CurrentDepartmentPK, Guid.NewGuid(), Guid.Empty, 1, Factory);
			var generalLedgerCombinedDataSource2 = new GeneralLedgerCombinedDataSource(Guid.NewGuid(), new ZDateTime(2024, 02, 01), "AR", "INV", "00002", Env.CurrentBranchPK, Env.CurrentDepartmentPK, Guid.NewGuid(), Guid.Empty, 1, Factory);
			var generalLedgerCombinedDataSource3 = new GeneralLedgerCombinedDataSource(Guid.NewGuid(), new ZDateTime(2025, 01, 01), "AR", "INV", "00003", Env.CurrentBranchPK, Env.CurrentDepartmentPK, Guid.NewGuid(), Guid.Empty, 1, Factory);

			var journalEntriesNumberPool = new AccountingNumberFountainPooler(AccountingConstants.JournalEntriesNumberFountainPoolConstants.JournalEntriesNumberPoolName, ZString.Empty, 1000L);
			var journalEntriesNumberFountainWrapper = new JournalEntriesNumberFountainWrapper(journalEntriesNumberPool,
					AccountingConstants.JournalEntriesNumberFountainPoolConstants.JournalEntriesNumberPoolName, "ARINV");

			AssertEquals("Generate Number of new accounting year should start with 1", "0000000001", journalEntriesNumberFountainWrapper.Generate(generalLedgerCombinedDataSource1));
			AssertEquals("0000000002", journalEntriesNumberFountainWrapper.Generate(generalLedgerCombinedDataSource2));
			AssertEquals("Generate Number of new accounting year should start with 1", "0000000001", journalEntriesNumberFountainWrapper.Generate(generalLedgerCombinedDataSource3));
		}

		[TestDate(2024, 11, 20)]
		public void TestGenerateNumberUsingSequenceResetOptionIsMonth()
		{
			var testBranch = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "TST");
			TestObjectCreator.CreateTestPeriodsForEntireYear(2024);
			TestObjectCreator.SetTemporaryControlAccounts();

			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN,
				NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL,
				SequenceResetOption = AccountingConstants.JournalEntriesNumberCustomisationSequenceResetOption.MONTH
			};

			var monthDigitsSetting = journalEntriesNumberCustomisationSetting.NumberSequenceCustomisations.Cast<JournalEntriesNumberCustomisation>().FirstOrDefault(x => x.ElementName == JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.CalendarMonthAs2Digits);
			monthDigitsSetting.Include = false;
			monthDigitsSetting.Fountain = false;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			using (AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, journalEntriesNumberCustomisationSetting))
			{
				var generalLedgerCombinedDataSource1 = new GeneralLedgerCombinedDataSource(Guid.NewGuid(), new ZDateTime(2024, 01, 01), "AR", "INV", "00001", Env.CurrentBranchPK, Env.CurrentDepartmentPK, Guid.NewGuid(), Guid.Empty, 1, Factory);
				var generalLedgerCombinedDataSource2 = new GeneralLedgerCombinedDataSource(Guid.NewGuid(), new ZDateTime(2024, 01, 02), "AR", "INV", "00002", Env.CurrentBranchPK, Env.CurrentDepartmentPK, Guid.NewGuid(), Guid.Empty, 1, Factory);
				var generalLedgerCombinedDataSource3 = new GeneralLedgerCombinedDataSource(Guid.NewGuid(), new ZDateTime(2024, 02, 01), "AR", "INV", "00003", Env.CurrentBranchPK, Env.CurrentDepartmentPK, Guid.NewGuid(), Guid.Empty, 1, Factory);

				var journalEntriesNumberPool = new AccountingNumberFountainPooler(AccountingConstants.JournalEntriesNumberFountainPoolConstants.JournalEntriesNumberPoolName, ZString.Empty, 1000L);
				var journalEntriesNumberFountainWrapper = new JournalEntriesNumberFountainWrapper(journalEntriesNumberPool,
						AccountingConstants.JournalEntriesNumberFountainPoolConstants.JournalEntriesNumberPoolName, "ARINV");

				AssertEquals("Generate Number should reset from 1 with Sequence Reset Option is MTH", "0000000001", journalEntriesNumberFountainWrapper.Generate(generalLedgerCombinedDataSource1));
				AssertEquals("0000000002", journalEntriesNumberFountainWrapper.Generate(generalLedgerCombinedDataSource2));
				AssertEquals("Generate Number should reset from 1 with Sequence Reset Option is MTH", "0000000001", journalEntriesNumberFountainWrapper.Generate(generalLedgerCombinedDataSource3));
			}
		}

		[TestDate(2025, 10, 20)]
		public void TestGenerateNumber()
		{
			TestObjectCreator.SetTemporaryControlAccounts();
			var testBranch = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "TST");
			var postDate = new ZDateTime(2025, 10, 20);
			PeriodTestHelper.SetupSinglePeriod(202501, new ZDateTime(2025, 10, 1), new ZDateTime(2025, 10, 30));
			Factory.Save();

			var journalEntriesNumberPool = new AccountingNumberFountainPooler(AccountingConstants.JournalEntriesNumberFountainPoolConstants.JournalEntriesNumberPoolName, ZString.Empty, 1000L);
			var journalEntriesNumberFountainWrapper = new JournalEntriesNumberFountainWrapper(journalEntriesNumberPool,
				AccountingConstants.JournalEntriesNumberFountainPoolConstants.JournalEntriesNumberPoolName, "ARINV");
			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN,
				NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL
			};

			var calendarYearAsDigits = journalEntriesNumberCustomisationSetting.NumberSequenceCustomisations.Cast<JournalEntriesNumberCustomisation>().FirstOrDefault(x => x.ElementName == JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.CalendarYearAsDigits);
			calendarYearAsDigits.Include = ZBool.True;
			calendarYearAsDigits.Fountain = ZBool.True;
			calendarYearAsDigits.Code = "1";
			calendarYearAsDigits.Order = 1;

			var sequenceNumber = journalEntriesNumberCustomisationSetting.NumberSequenceCustomisations.Cast<JournalEntriesNumberCustomisation>().FirstOrDefault(x => x.ElementName == TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber);
			sequenceNumber.Include = false;

			AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			using (AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, journalEntriesNumberCustomisationSetting))
			{
				var result = journalEntriesNumberFountainWrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("5", result);
			}

			calendarYearAsDigits.Code = "2";
			using (AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, journalEntriesNumberCustomisationSetting))
			{
				var result = journalEntriesNumberFountainWrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("25", result);
			}

			calendarYearAsDigits.Code = "4";
			using (AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, journalEntriesNumberCustomisationSetting))
			{
				var result = journalEntriesNumberFountainWrapper.Generate(new AccountingNumberFountainDataSourceForTest(Factory, postDate, testBranch, TestObjectCreator.FESDepartment));
				AssertEquals("2025", result);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);
			PeriodTestHelper = new AccountingPeriodTestHelper();
		}

		protected AccountingPeriodTestHelper PeriodTestHelper;
		protected TestObjectCreator TestObjectCreator;
	}
}
