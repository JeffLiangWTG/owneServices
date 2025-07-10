using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class ConsolTypePairListProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new ConsolTypePairListProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			var expectedList = new CodeDescriptionPairList(OLookUpEditType.AgentType);
			expectedList.AddPair(Core.Constants.AgentType.AWBCoload, Core.Constants.AgentTypeDescriptions.AWBCoload);
			expectedList.AddPair(Core.Constants.AgentType.AWBMaster, Core.Constants.AgentTypeDescriptions.AWBMaster);

			AssertListEqual((CreateCodeDescriptionPairListProvider()).GetCodeDescriptionPairList(), expectedList);
		}
	}
}
