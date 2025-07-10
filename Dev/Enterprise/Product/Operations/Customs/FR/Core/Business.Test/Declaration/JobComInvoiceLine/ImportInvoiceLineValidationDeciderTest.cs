using System;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public abstract class ImportInvoiceLineValidationDeciderTest<T> : EU.Business.Declaration.Testing.ImportInvoiceLineValidationDeciderTest<T>
		where T : class, IImportInvoiceLineValidationDecider
	{
		protected override T GetNewValidationDecider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			return (T)Activator.CreateInstance(typeof(T), invoiceLine);
		}

		public void TestIsRuleC0699_N01Active()
		{
			AssertEquals(ExpectedIsRuleC0699_N01Active, decider.IsRuleC0699_N01Active);
		}
		protected abstract bool ExpectedIsRuleC0699_N01Active { get; }

		public void TestIsRuleC0699_N02Active()
		{
			AssertEquals(ExpectedIsRuleC0699_N02Active, decider.IsRuleC0699_N02Active);
		}
		protected abstract bool ExpectedIsRuleC0699_N02Active { get; }

		public void TestIsRuleC0710_N01Active()
		{
			AssertEquals(ExpectedIsRuleC0710_N01Active, decider.IsRuleC0710_N01Active);
		}
		protected abstract bool ExpectedIsRuleC0710_N01Active { get; }

		public void TestIsRuleC0834_N02Active()
		{
			AssertEquals(ExpectedIsRuleC0834_N02Active, decider.IsRuleC0834_N02Active);
		}

		protected abstract bool ExpectedIsRuleC0834_N02Active {  get; }

		public void TestIsRuleNAT_105Active()
		{
			AssertEquals(ExpectedIsRuleNAT_105Active, decider.IsRuleNAT_105Active);
		}

		protected abstract bool ExpectedIsRuleNAT_105Active { get; }

		public void TestIsRuleNAT_235Active()
		{
			AssertEquals(ExpectedIsRuleNAT_235Active, decider.IsRuleNAT_235Active);
		}
		protected abstract bool ExpectedIsRuleNAT_235Active { get; }

		public void TestIsRuleNAT_240Active()
		{
			AssertEquals(ExpectedIsRuleNAT_240Active, decider.IsRuleNAT_240Active);
		}
		protected abstract bool ExpectedIsRuleNAT_240Active { get; }

		public void TestIsRuleNAT_254Active()
		{
			AssertEquals(ExpectedIsRuleNAT_254Active, decider.IsRuleNAT_254Active);
		}

		protected abstract bool ExpectedIsRuleNAT_254Active { get; }

		public void TestIsRuleNAT_030Active()
		{
			AssertEquals(ExpectedIsRuleNAT_030Active, decider.IsRuleNAT_030Active);
		}
		protected abstract bool ExpectedIsRuleNAT_030Active { get; }

		public void TestIsRuleNAT_154Active()
		{
			AssertEquals(ExpectedIsRuleNAT_154Active, decider.IsRuleNAT_154Active);
		}
		protected abstract bool ExpectedIsRuleNAT_154Active { get; }

		public void TestIsRuleNAT_237Active()
		{
			AssertEquals(ExpectedIsRuleNAT_237Active, decider.IsRuleNAT_237Active);
		}
		protected abstract bool ExpectedIsRuleNAT_237Active { get; }

		protected JobDeclaration declaration;
		protected JobComInvoiceLine invoiceLine;
	}
}
