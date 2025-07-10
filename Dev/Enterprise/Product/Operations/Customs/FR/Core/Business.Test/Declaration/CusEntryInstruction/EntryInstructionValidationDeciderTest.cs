using System;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public abstract class EntryInstructionValidationDeciderTest<T> : EU.Business.Declaration.Testing.EntryInstructionValidationDeciderTest<T>
		where T : class, IEntryInstructionValidationDecider
	{
		public void TestIsRuleR0012Active()
		{
			AssertEquals(ExpectedIsRuleR0012Active, validationDecider.IsRuleR0012Active);
		}
		protected abstract bool ExpectedIsRuleR0012Active { get; }

		public void TestIsRuleC0002Active()
		{
			AssertEquals(ExpectedIsRuleC0002Active, validationDecider.IsRuleC0002Active);
		}
		protected abstract bool ExpectedIsRuleC0002Active { get; }

		public void TestIsRuleC0627Active()
		{
			AssertEquals(ExpectedIsRuleC0627Active, validationDecider.IsRuleC0627Active);
		}
		protected abstract bool ExpectedIsRuleC0627Active { get; }

		public void TestIsRuleC0810_N01Active()
		{
			AssertEquals(ExpectedIsRuleC0810_N01Active, validationDecider.IsRuleC0810_N01Active);
		}
		protected abstract bool ExpectedIsRuleC0810_N01Active { get; }

		public void TestIsRuleC0834_N02Active()
		{
			AssertEquals(ExpectedIsRuleC0834_N02Active, validationDecider.IsRuleC0834_N02Active);
		}
		protected abstract bool ExpectedIsRuleC0834_N02Active { get; }

		public void TestIsRuleNAT_004BisActive()
		{
			AssertEquals(ExpectedIsRuleNAT_004BisActive, validationDecider.IsRuleNAT_004BisActive);
		}
		protected abstract bool ExpectedIsRuleNAT_004BisActive { get; }

		public void TestIsRuleNAT_130BisActive()
		{
			AssertEquals(ExpectedIsRuleNAT_130BisActive, validationDecider.IsRuleNAT_130BisActive);
		}
		protected abstract bool ExpectedIsRuleNAT_130BisActive { get; }

		public void TestIsRuleR0933_N03Active()
		{
			AssertEquals(ExpectedIsRuleR0933_N03Active, validationDecider.IsRuleR0933_N03Active);
		}
		protected abstract bool ExpectedIsRuleR0933_N03Active { get; }

		public void TestIsRuleNAT_030Active()
		{
			AssertEquals(ExpectedIsRuleNAT_030Active, validationDecider.IsRuleNAT_030Active);
		}
		protected abstract bool ExpectedIsRuleNAT_030Active { get; }

		protected override T GetNewValidationDecider()
		{
			declaration = Factory.New<JobDeclaration>();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			return (T)Activator.CreateInstance(typeof(T), entryInstruction);
		}

		protected JobDeclaration declaration;
		protected CusEntryInstruction entryInstruction;
	}
}
