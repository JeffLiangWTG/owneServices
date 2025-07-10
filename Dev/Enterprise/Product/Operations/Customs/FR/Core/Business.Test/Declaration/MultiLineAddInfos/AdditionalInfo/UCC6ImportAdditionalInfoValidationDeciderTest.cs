using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(UCC6ImportAdditionalInfoValidationDecider))]
	sealed class UCC6ImportAdditionalInfoValidationDeciderTest : AdditionalInfoValidationDeciderTest<UCC6ImportAdditionalInfoValidationDecider>
	{
		protected override bool ExpectedIsBR2038RuleActive => true;

		protected override bool ExpectedIsC0612RuleActive => false;

		protected override bool ExpectedIsRuleC0834_N01Active => true;

		protected override bool ExpectedIsRuleNAT_088BisActive => true;

		protected override bool ExpectedIsRuleNAT_228Active => true;

		protected override bool ExpectedIsRuleNAT_041Active => true;
	}
}
