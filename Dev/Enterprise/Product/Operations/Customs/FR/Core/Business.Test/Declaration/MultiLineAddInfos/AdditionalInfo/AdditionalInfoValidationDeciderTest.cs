namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public abstract class AdditionalInfoValidationDeciderTest<T> : EU.Business.Declaration.MultiLineAddInfos.Testing.AdditionalInfoValidationDeciderTest<T>
		where T : class, IAdditionalInfoValidationDecider
	{
		public void TestIsRuleC0834_N01Active()
		{
			AssertEquals(ExpectedIsRuleC0834_N01Active, validationDecider.IsRuleC0834_N01Active);
		}

		public void TestIsRuleNAT_088BisActive()
		{
			AssertEquals(ExpectedIsRuleNAT_088BisActive, validationDecider.IsRuleNAT_088BisActive);
		}

		public void TestIsRuleNAT_228Active()
		{
			AssertEquals(ExpectedIsRuleNAT_228Active, validationDecider.IsRuleNAT_228Active);
		}

		public void TestIsRuleNAT_041Active()
		{
			AssertEquals(ExpectedIsRuleNAT_041Active, validationDecider.IsRuleNAT_041Active);
		}

		protected abstract bool ExpectedIsRuleC0834_N01Active { get; }

		protected abstract bool ExpectedIsRuleNAT_088BisActive { get; }

		protected abstract bool ExpectedIsRuleNAT_228Active { get; }

		protected abstract bool ExpectedIsRuleNAT_041Active { get; }
	}
}
