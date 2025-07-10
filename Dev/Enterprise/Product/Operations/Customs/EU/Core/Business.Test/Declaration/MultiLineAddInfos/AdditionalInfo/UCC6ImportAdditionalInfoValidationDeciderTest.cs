using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	[TestedType(typeof(UCC6ImportAdditionalInfoValidationDecider))]
	sealed class UCC6ImportAdditionalInfoValidationDeciderTest : AdditionalInfoValidationDeciderTest<UCC6ImportAdditionalInfoValidationDecider>
	{
		protected override bool ExpectedIsBR2038RuleActive => true;

		protected override bool ExpectedIsC0612RuleActive => false;
	}
}
