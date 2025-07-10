using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	public class GLJournalTest : TestCaseWithFactory
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<GLJournal>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		[TestDate(2020, 6, 20)]
		public void TestIsLinkedWithDSBJobCloseBatch_CachedRule()
		{
			TestObjectCreator.CreateGLJournalWithDSBJobCloseBatch(out GLJournal journal, out DsbJobCloseBatch batch);
			Factory.Save();

			AssertEquals("GL Journal is linked with DSB Job Close Batch", true, journal.IsLinkedWithDSBJobCloseBatch_Cached);
			IDbConnected conn = Factory;
			var commandCount = conn.Connection.ExecutedCommandCount;
			for (var i = 0; i < 15; i++)
			{
				AssertEquals("GL Journal is linked with DSB Job Close Batch", true, journal.IsLinkedWithDSBJobCloseBatch_Cached);
			}
			AssertEquals("result should be cached and the table loading count should not increase", commandCount, conn.Connection.ExecutedCommandCount);

			Factory.Save();
			AssertEquals("GL Journal is linked with DSB Job Close Batch", true, journal.IsLinkedWithDSBJobCloseBatch_Cached);
			AssertEquals("result should be refreshed", commandCount + 1, conn.Connection.ExecutedCommandCount);

			batch.JBB_AH_Journal = ZGuid.Empty;
			AssertEquals("The value is cached , so it do not change", true, journal.IsLinkedWithDSBJobCloseBatch_Cached);
			AssertEquals("The value is cached , so the command executing count should not be increased", commandCount + 1, conn.Connection.ExecutedCommandCount);
		}

		[TestDate(2020, 6, 20)]
		public void TestIsLinkedWithDSBJobCloseBatch()
		{
			TestObjectCreator.CreateGLJournalWithDSBJobCloseBatch(out GLJournal glJournal, out DsbJobCloseBatch batch);
			batch.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Approve;
			batch.JBB_AH_Journal = ZGuid.Empty;
			Factory.Save();

			AssertEquals("Precondition", false, glJournal.IsLinkedWithDSBJobCloseBatch_Cached);

			batch.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Close;
			batch.JBB_AH_Journal = glJournal.PK;
			Factory.Save();

			AssertEquals("GL Journal is linked with DSB Job Close Batch", true, glJournal.IsLinkedWithDSBJobCloseBatch_Cached);
		}

		public void TestGLJournalLineGLDDeleter()
		{
			var glJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			var line1 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			Factory.Save();

			AssertNotNull(glJournal.GLJournalLineGLDDeleter);
		}

		public void TestGLJournalGLDComplianceReportAction()
		{
			var glJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			var line1 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			Factory.Save();

			AssertNotNull(glJournal.GLJournalGLDComplianceReportAction);
		}

		public void TestGLJournalAllTransactionComplianceReportAction()
		{
			var glJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			var line1 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			Factory.Save();

			AssertNotNull(glJournal.GLJournalAllTransactionComplianceReportAction);
		}

		[TestDate(2021, 12, 14)]
		public void TestIsManuallySetTransactionNumber_ForTestOnly()
		{
			var journal1 = Factory.NewWithValidTestData<GLJournal>();
			var journal2 = Factory.NewWithValidTestData<GLJournal>();

			AssertNotEquals("Journal1 Transaction Number Pre-condition", "11100011", journal1.AH_TransactionNum);
			AssertNotEquals("Journal2 Transaction Number Pre-condition", "22220002", journal2.AH_TransactionNum);

			journal1.IsManuallySetTransactionNumber_ForTestOnly = true;
			journal2.IsManuallySetTransactionNumber_ForTestOnly = false;

			journal1.AH_TransactionNum = "11100011";
			journal2.AH_TransactionNum = "22220002";

			Factory.Save();

			AssertEquals("Transaction number for Journal1 will be set manually","11100011", journal1.AH_TransactionNum);
			AssertNotEquals("Transaction number for Journal2 will not be set manually", "22220002", journal2.AH_TransactionNum);
		}

		public void TestCanApplyTaxBranch() => AssertEquals(false, Factory.NewWithValidTestData<GLJournal>().CanApplyTaxBranch);

		public void TestShouldCreateEDocOnSaving()
		{
			var journal = Factory.NewWithValidTestData<GLJournal>();
			var propertyInfo = typeof(GLJournal).GetProperty("ShouldCreateEDocOnSaving", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			AssertEquals("Default Value", true, propertyInfo.GetValue(journal));

			using (GLJournal.SuspendCreateEDocOnSaving())
			{
				AssertEquals("Value when feature is disabled in UTs", false, propertyInfo.GetValue(journal));

				using (Globals.TemporaryOverrideForIsTest(false))
				{
					AssertEquals("Feature cannot be disabled in production release", true, propertyInfo.GetValue(journal));
				}
			}

			AssertEquals("Value when suspender disposed", true, propertyInfo.GetValue(journal));
		}

		public void TestGlowDataDefinitionAttribute()
		{
			var journalType = typeof(GLJournal);
			var attribute = journalType.GetCustomAttributes(typeof(GlowDataDefinitionAttribute), false);
			AssertEquals(1, attribute.Length);
			AssertEquals("IGLJournalHeader", ((GlowDataDefinitionAttribute)attribute[0]).DataDefinitionName);
		}

		public void TestUnsignedLineAmountsAfterReverseGLJournal()
		{
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, CurrencyCodes.UnitedStates);
			var vnd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, CurrencyCodes.VietNam);
			vnd.RX_SubUnitRatio = 100;

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.China))
			{
				GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = vnd.RX_Code;
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				Factory.Save();

				var gLJournal = Factory.NewWithValidTestData<GLJournal>();
				var line1 = TestObjectCreator.CreateGLJournalLine(gLJournal, 1.68m, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
				var line2 = TestObjectCreator.CreateGLJournalLine(gLJournal, 246000m, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
				var line3 = TestObjectCreator.CreateGLJournalLine(gLJournal, 0.04m, DebitCredit.DR, TestObjectCreator.GLHeaderNTE1.PK);
				line1.AL_ExchangeRate = 7.142857;
				line2.AL_ExchangeRate = 0.000050;
				line3.AL_ExchangeRate = 7.142857;
				line3.UnsignedLocalLineAmount = 0.3m;
				Factory.Save();

				gLJournal.GenerateReverseTransaction(false);
				var reverseGLJournal = (GLJournal)gLJournal.ReverseTransaction;
				var reverseGLJournalLine = (GLJournalLine)reverseGLJournal.Lines[2];
				AssertEquals(0.3m, reverseGLJournalLine.UnsignedLocalLineAmount);
				AssertEquals(-0.3m, reverseGLJournalLine.AL_LineAmount);
			}
		}

		public void TestAH_PostDate_ResetLineExchangeRate()
		{
			AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var gLJournalExchangeRateType = new GLJournalExchangeRateType() { BalanceSheetAccountTypeExchangeRateType = ExchangeRateTypes.Code.BuyRate, ProfitAndLossAccountTypeExchangeRateType = ExchangeRateTypes.Code.SellRate };
			AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, gLJournalExchangeRateType);

			TestObjectCreator.CreateUSDBuyRate(0.88m, ZDateTime.Now);
			TestObjectCreator.GLHeader1.AG_AccountType = AccountType.BalanceSheetAccount;
			var journal = Factory.NewWithValidTestData<GLJournal>();
			journal.AH_PostDate = ZDateTime.Now;
			var line = TestObjectCreator.CreateGLJournalLine(journal, 100m, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals("exRate updated to Today's rate", 0.88m, line.AL_ExchangeRate);

			TestObjectCreator.CreateUSDBuyRate(0.66m, ZDateTime.Now.AddDays(1));
			journal.AH_PostDate = ZDateTime.Now.AddDays(1);
			AssertEquals("exRate updated to 0.66", 0.66m, line.AL_ExchangeRate);
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}

				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;
	}
}
