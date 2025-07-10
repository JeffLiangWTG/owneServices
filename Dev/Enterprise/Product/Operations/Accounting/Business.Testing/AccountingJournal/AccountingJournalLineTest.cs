using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccountingJournalLine))]
	public class AccountingJournalLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPublicProperties()
		{
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AC = Creator.CC1.PK;
			transactionLine.AL_AG = Creator.GLHeader1.PK;
			transactionLine.AL_Desc = "Test Description";
			transactionLine.AL_RevRecognitionType = "IMM";
			transactionLine.MultiSubAccountTypeCode = "Test MultiSubAccountTypeCode";
			transactionLine.AL_LineAmount = 200M;
			transactionLine.AL_ExchangeRate = 0.5M;
			transactionLine.AL_RX_NKTransactionCurrency = "USD";
			transactionLine.AL_GSTVAT = 20M;
			transactionLine.AL_OSAmount = 110M;
			transactionLine.AL_GSTVATBasis = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;

			var ajline = new AccountingJournalLine(transactionLine);
			AssertEquals("AL_AC", Creator.CC1.PK, ajline.AL_AC);
			AssertEquals("ChargeCode", Creator.CC1.AC_Code, ajline.ChargeCode.AC_Code);
			AssertEquals("AL_AG", Creator.GLHeader1.PK, ajline.AL_AG);
			AssertEquals("GLHeader", Creator.GLHeader1.AG_Description, ajline.GLAccountDescription);
			AssertEquals("AL_RevRecognitionType", "IMM", ajline.AL_RevRecognitionType);
			AssertEquals("MultiSubAccountTypeCode", "Test MultiSubAccountTypeCode", ajline.MultiSubAccountTypeCode);
			AssertEquals("AL_LineAmount", 200M, ajline.AL_LineAmount);
			AssertEquals("AL_GSTVAT", 20M, ajline.AL_GSTVAT);
			AssertEquals("AL_OSAmount", 110M, ajline.AL_OSAmount);
			AssertEquals("AL_OSExTaxAmount", 100M, ajline.AL_OSExTaxAmount);
			AssertEquals("AL_OSTaxAmount", 10M, ajline.AL_OSTaxAmount);
			AssertEquals("TaxBasis", Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Description, ajline.TaxBasis);
		}

		public void TestAL_OSExTaxAmountWithMinusculeExChangeRate()
		{
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AC = Creator.CC1.PK;
			transactionLine.AL_AG = Creator.GLHeader1.PK;
			transactionLine.AL_Desc = "Test Description";
			transactionLine.AL_RevRecognitionType = "IMM";
			transactionLine.AL_LineAmount = 56751.26M;
			transactionLine.AL_ExchangeRate = 0.00055M;
			transactionLine.AL_RX_NKTransactionCurrency = "USD";
			transactionLine.AL_GSTVAT = 0M;
			transactionLine.AL_OSAmount = 103184100M;
			transactionLine.AL_GSTVATBasis = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			transactionLine.Company.GC_IsReciprocal = true;

			var ajline1 = new AccountingJournalLine(transactionLine);
			AssertEquals("AL_OSExTaxAmount", 103184100M, ajline1.AL_OSExTaxAmount);

			transactionLine.AL_GSTVAT = 10M;
			var ajline2 = new AccountingJournalLine(transactionLine);
			AssertEquals("AL_OSExTaxAmount", 103184109.09M, ajline2.AL_OSExTaxAmount);
		}

		public virtual void TestCurrency()
		{
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			var glAccountingJournalLine = new AccountingJournalLine(transactionLine);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("Precondition:currency", transactionLine.TransactionCurrency, glAccountingJournalLine.Currency);
				AssertEquals("Precondition:local currency", GlbCompany.CurrentCompany.LocalCurrency, glAccountingJournalLine.LocalCurrency);

				glAccountingJournalLine.SetCurrency(transactionLine.TransactionCurrency);
				AssertEquals("currency", transactionLine.TransactionCurrency, glAccountingJournalLine.Currency);
				AssertEquals("local currency", GlbCompany.CurrentCompany.LocalCurrency, glAccountingJournalLine.LocalCurrency);

				glAccountingJournalLine.SetLocalCurrency(GlbCompany.CurrentCompany.LocalCurrency);
				AssertEquals("currency", transactionLine.TransactionCurrency, glAccountingJournalLine.Currency);
				AssertEquals("local currency", GlbCompany.CurrentCompany.LocalCurrency, glAccountingJournalLine.LocalCurrency);
			}
		}

		public virtual void TestLocalCurrency()
		{
			var glAccountingJournalLine = (AccountingJournalLine)GetNewBusinessObject();
			AssertEquals("local currency", GlbCompany.CurrentCompany.LocalCurrency, glAccountingJournalLine.LocalCurrency);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AC = Creator.CC1.PK;
			transactionLine.AL_LineAmount = 200M;
			return new AccountingJournalLine(transactionLine);
		}

		protected TestObjectCreator Creator
		{
			get
			{
				if (creator == null)
				{
					creator = new TestObjectCreator(Factory);
				}
				return creator;
			}
		}

		TestObjectCreator creator;
	}

	[TestedType(typeof(AccountingJournalLineWithDifferentPostDateAndPeriod))]
	public class AccountingJournalLineWithDifferentPostDateAndPeriodTest : AccountingJournalLineTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AC = Creator.CC1.PK;
			transactionLine.AL_LineAmount = 200M;
			transactionLine.AL_PostDate = periodCalculator.GetLastDayForPeriod(ZDateTime.Today);
			return new AccountingJournalLineWithDifferentPostDateAndPeriod(transactionLine, periodCalculator.GetPeriodFromDate(ZDateTime.Today.AddMonths(2)));
		}

		public void TestPostDate()
		{
			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var ajline = GetNewBusinessObject() as AccountingJournalLineWithDifferentPostDateAndPeriod;
			AssertEquals("Post Period", periodCalculator.GetPeriodFromDate(ZDateTime.Today), ajline.AL_PostPeriod);
		}
	}
}
