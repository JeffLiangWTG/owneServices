using System;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public abstract class DeclarationValidationDeciderTest<T> : EU.Business.Declaration.Testing.DeclarationValidationDeciderTest<T>
		where T : class, IDeclarationValidationDecider
	{
		protected abstract bool ExpectedIsRuleNAT_020Active { get; }

		public void TestIsRuleNAT_041QuinquiesActive()
		{
			AssertEquals(ExpectedIsRuleNAT_041QuinquiesActive, validationDecider.IsRuleNAT_041QuinquiesActive);
		}
		protected abstract bool ExpectedIsRuleNAT_041QuinquiesActive { get; }

		public void TestIsRuleNAT_021Active()
		{
			AssertEquals(ExpectedIsRuleNAT_021Active, validationDecider.IsRuleNAT_021Active);
		}
		protected abstract bool ExpectedIsRuleNAT_021Active { get; }

		public void TestIsRuleNAT_130BisActive()
		{
			AssertEquals(ExpectedIsRuleNAT_130BisActive, validationDecider.IsRuleNAT_130BisActive);
		}
		protected abstract bool ExpectedIsRuleNAT_130BisActive { get; }

		public void TestIsRuleNat_145BisActive()
		{
			AssertEquals(ExpectedIsRuleNat_145BisActive, validationDecider.IsRuleNat_145BisActive);
		}
		protected abstract bool ExpectedIsRuleNat_145BisActive { get; }

		protected override T GetNewValidationDecider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			return (T)Activator.CreateInstance(typeof(T), declaration);
		}

		protected JobDeclaration declaration;
	}
}
