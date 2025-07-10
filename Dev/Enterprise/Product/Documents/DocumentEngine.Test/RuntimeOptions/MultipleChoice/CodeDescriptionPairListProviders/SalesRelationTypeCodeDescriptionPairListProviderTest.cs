using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class SalesRelationTypeCodeDescriptionPairListProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new SalesRelationTypeCodeDescriptionPairListProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("ANY", "Any Sales Relation");
			expectedList.AddRange(SalesRelationTypeList.New());

			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), expectedList);
		}
	}
}
