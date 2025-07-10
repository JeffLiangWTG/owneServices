using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.UnapprovedTransaction
{
	public class TaxOverriddenLinesInfoProviderTest : LinesInfoProviderTest
	{
		public override void TestSetHasTaxOverriddenAndHasTaxOverridden()
		{
			var header = new TransactionCreator().CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, numberOfLines: 2) as APInvoice;
			var line1 = header.Lines[0];
			var line2 = header.Lines[1];

			AssertEquals(true, Provider.IsTaxOverridden(line1));
			AssertEquals(true, Provider.IsTaxOverridden(line2));

			var ex = AssertExceptionThrown<NotImplementedException>(() => Provider.SetTaxOverridden(line1));
			AssertEquals("Does NOT support this feature.", ex.Message);

			AssertEquals(true, Provider.IsTaxOverridden(line1));
			AssertEquals(true, Provider.IsTaxOverridden(line2));
		}

		protected override LinesInfoProvider CreateLinesInfoProvider() => new TaxOverriddenLinesInfoProvider();
	}
}
