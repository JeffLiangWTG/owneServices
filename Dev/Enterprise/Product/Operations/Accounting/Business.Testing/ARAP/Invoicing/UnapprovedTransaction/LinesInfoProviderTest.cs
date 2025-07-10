using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.UnapprovedTransaction
{
	public class LinesInfoProviderTest : TestCaseWithFactory
	{
		public virtual void TestSetHasTaxOverriddenAndHasTaxOverridden()
		{
			var header = new TransactionCreator().CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, numberOfLines: 2) as APInvoice;
			var line1 = header.Lines[0];
			var line2 = header.Lines[1];

			AssertEquals(false, Provider.IsTaxOverridden(line1));
			AssertEquals(false, Provider.IsTaxOverridden(line2));

			Provider.SetTaxOverridden(line1);

			AssertEquals(true, Provider.IsTaxOverridden(line1));
			AssertEquals(false, Provider.IsTaxOverridden(line2));

			Provider.SetTaxOverridden(line2);

			AssertEquals(true, Provider.IsTaxOverridden(line1));
			AssertEquals(true, Provider.IsTaxOverridden(line2));
		}

		public void TestSetIsLocalValueApplicableAndIsLocalValueApplicable()
		{
			var header = new TransactionCreator().CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, numberOfLines: 2) as APInvoice;
			var line1 = header.Lines[0];
			var line2 = header.Lines[1];

			AssertEquals(false, Provider.IsLocalAmountApplicable(line1));
			AssertEquals(false, Provider.IsLocalAmountApplicable(line2));

			Provider.SetLocalAmountApplicable(line1);

			AssertEquals(true, Provider.IsLocalAmountApplicable(line1));
			AssertEquals(false, Provider.IsLocalAmountApplicable(line2));

			Provider.SetLocalAmountApplicable(line2);

			AssertEquals(true, Provider.IsLocalAmountApplicable(line1));
			AssertEquals(true, Provider.IsLocalAmountApplicable(line2));
		}

		protected override void SetUp()
		{
			Provider = CreateLinesInfoProvider();
		}

		protected virtual LinesInfoProvider CreateLinesInfoProvider() => new LinesInfoProvider();
		protected LinesInfoProvider Provider;

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
