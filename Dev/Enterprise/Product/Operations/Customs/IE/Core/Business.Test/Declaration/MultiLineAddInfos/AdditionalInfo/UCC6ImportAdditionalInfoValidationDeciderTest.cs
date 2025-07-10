using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(UCC6ImportAdditionalInfoValidationDecider))]
	public sealed class UCC6ImportAdditionalInfoValidationDeciderTest : AdditionalInfoValidationDeciderTest<UCC6ImportAdditionalInfoValidationDecider>
	{
		protected override bool ExpectedIsBR2038RuleActive => true;

		protected override bool ExpectedIsC0612RuleActive => true;
	}
}
