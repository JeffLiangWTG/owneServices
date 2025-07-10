using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.AccountingVoucherPrint
{
	public class OptionalFilterCriteriaTest : TestCaseWithFactory
	{
		OptionalFilterCriteria fTestFilterCriteria;
		public void TestDescriptionNotEmpty()
		{
			fTestFilterCriteria = new OptionalFilterCriteria("TEST", new ZQuery());
			AssertEquals("Description", "TEST", fTestFilterCriteria.Description);
		}

		public void TestToString()
		{
			fTestFilterCriteria = new OptionalFilterCriteria("TEST", new ZQuery());
			AssertEquals("Description", "TEST", fTestFilterCriteria.ToString());
		}

		public virtual void TestFilterNotEmptyIfSelected()
		{
			fTestFilterCriteria = new OptionalFilterCriteria("TEST", new ZQuery(AccTransactionHeaderSchema.AH_Desc, "TEST"));
			fTestFilterCriteria.Enabled = true;
			Assert("Filter should not be Empty", !fTestFilterCriteria.Filter.IsEmpty);
			AssertEquals("Filter", new ZQuery(AccTransactionHeaderSchema.AH_Desc, "TEST").LiteralTextADO, fTestFilterCriteria.Filter.LiteralTextADO);
		}

		public virtual void TestFilterEmptyIfNotSelected()
		{
			fTestFilterCriteria = new OptionalFilterCriteria("TEST", new ZQuery(AccTransactionHeaderSchema.AH_Desc, "TEST"));
			fTestFilterCriteria.Enabled = false;
			Assert("Filter should be Empty", fTestFilterCriteria.Filter.IsEmpty);
		}

		public void TestEmptyConstructor()
		{
			fTestFilterCriteria = new OptionalFilterCriteria();
			Assert("Description", fTestFilterCriteria.Description.IsEmpty);
			Assert("Filter should be Empty", fTestFilterCriteria.Filter.IsEmpty);
		}

		public void TestNullFilter()
		{
			fTestFilterCriteria = new OptionalFilterCriteria("TEST", null);
			fTestFilterCriteria.Enabled = true;
			Assert("Filter should be Empty", fTestFilterCriteria.Filter.IsEmpty);
		}

		public void TestEmptyFilter()
		{
			fTestFilterCriteria = new OptionalFilterCriteria("TEST", new ZQuery());
			fTestFilterCriteria.Enabled = true;
			Assert("Filter should be Empty", fTestFilterCriteria.Filter.IsEmpty);
		}

		public void TestFilterConstructor()
		{
			fTestFilterCriteria = new OptionalFilterCriteria("TEST", AccTransactionHeaderSchema.AH_Desc, "TESTDescr");
			fTestFilterCriteria.Enabled = true;
			AssertEquals("Filter Value", new ZQuery(AccTransactionHeaderSchema.AH_Desc, "TESTDescr").LiteralTextADO, fTestFilterCriteria.Filter.LiteralTextADO);
		}
	}

	public class LedgerFilterCriteriaTest : TestCaseWithFactory
	{
		public void TestFilterGenerated()
		{
			LedgerFilterCriteria testFilter = new LedgerFilterCriteria(LedgerDescription.AccountsReceivable, LedgerTypes.AccountsReceivable);
			testFilter.Enabled = true;
			AssertEquals(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable).LiteralTextADO, testFilter.Filter.LiteralTextADO);
		}

		public void TestFilterEmptyIfNotEnabled()
		{
			LedgerFilterCriteria testFilter = new LedgerFilterCriteria(LedgerDescription.AccountsReceivable, LedgerTypes.AccountsReceivable);
			testFilter.Enabled = false;
			Assert(testFilter.Filter.IsEmpty);
		}
	}

	public class TransactionFilterCriteriaTest : TestCaseWithFactory
	{
		public void TestFilterGenerated()
		{
			TransactionFilterCriteria testFilter = new TransactionFilterCriteria(TransactionDescription.Invoice, TransactionTypes.Invoice);
			testFilter.Enabled = true;
			AssertEquals(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice).LiteralTextADO, testFilter.Filter.LiteralTextADO);
		}

		public void TestFilterEmptyIfNotEnabled()
		{
			TransactionFilterCriteria testFilter = new TransactionFilterCriteria(TransactionDescription.Invoice, TransactionTypes.Invoice);
			testFilter.Enabled = false;
			Assert(testFilter.Filter.IsEmpty);
		}
	}
}