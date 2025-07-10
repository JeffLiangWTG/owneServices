using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class GLRollupLevelCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new GLRollupLevelCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			GLRollupLevelCodeDescriptionPairProvider testPairProvider = new GLRollupLevelCodeDescriptionPairProvider();
			CodeDescriptionPairList testPair = testPairProvider.AddGLRollupLevels();

			AssertEquals(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList().GetType(), testPair.GetType());

			AssertEquals("Number of list values", 3, CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList().Count);
			AssertEquals("First Value (Default)", "0", CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList()[0].Code);
			AssertEquals("First Value Description", "No Roll up", CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList()[0].Description);
			AssertEquals("Second Value", "1", CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList()[1].Code);
			AssertEquals("Second Value Description", "XXXX.--.--", CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList()[1].Description);
			AssertEquals("Third Value", "2", CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList()[2].Code);
			AssertEquals("Second Value Description", "XXXX.XX.--", CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList()[2].Description);
		}
	}
}
