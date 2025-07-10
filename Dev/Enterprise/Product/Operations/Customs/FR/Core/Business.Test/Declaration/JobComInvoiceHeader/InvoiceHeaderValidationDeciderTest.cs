using System;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public abstract class InvoiceHeaderValidationDeciderTest<T> : EU.Business.Declaration.Testing.InvoiceHeaderValidationDeciderTest<T>
		where T : class, IInvoiceHeaderValidationDecider
	{
		protected abstract bool ExpectedIsRuleTNAT_078Active { get; }
		protected abstract bool ExpectedIsRuleNAT_154Active { get; }
		protected abstract bool ExpectedIsRuleNAT_237Active { get; }

		public void TestIsRuleTNAT_078Active()
		{
			AssertEquals(ExpectedIsRuleTNAT_078Active, validationDecider.IsRuleTNAT_078Active);
		}

		public void TestIsRuleTNAT_154Active()
		{
			AssertEquals(ExpectedIsRuleNAT_154Active, validationDecider.IsRuleNAT_154Active);
		}

		public void TestIsRuleTNAT_237Active()
		{
			AssertEquals(ExpectedIsRuleNAT_237Active, validationDecider.IsRuleNAT_237Active);
		}

		protected override T GetNewValidationDecider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			return (T)Activator.CreateInstance(typeof(T), invoice);
		}

		public void TestIsRuleNAT_240Active()
		{
			AssertEquals(ExpectedIsRuleNAT_240Active, validationDecider.IsRuleNAT_240Active);
		}

		protected abstract bool ExpectedIsRuleNAT_240Active { get; }

		protected JobDeclaration declaration;
		protected JobComInvoiceHeader invoice;
	}
}
