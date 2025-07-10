using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class ValidationRuleCodeConstantsTest : TestCase
	{
		public void TestGetRuleCodeMessagePrefix() => CombineAssertions(() =>
		{
			AssertEquals("addSpaceAtEnd=default", "[R0850-1]", ValidationRuleCodeConstants.R0850_1.GetRuleCodeMessagePrefix());
			AssertEquals("addSpaceAtEnd=false", "[R0850-1]", ValidationRuleCodeConstants.R0850_1.GetRuleCodeMessagePrefix(false));
			AssertEquals("addSpaceAtEnd=true", "[R0850-1] ", ValidationRuleCodeConstants.R0850_1.GetRuleCodeMessagePrefix(true));
		});
	}
}
