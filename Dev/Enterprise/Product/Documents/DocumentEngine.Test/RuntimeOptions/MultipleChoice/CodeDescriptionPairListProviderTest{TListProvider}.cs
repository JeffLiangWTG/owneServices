using CargoWise.Integration;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	public abstract class CodeDescriptionPairListProviderTest<TListProvider> : CodeDescriptionPairListProviderTest
			where TListProvider : ICodeDescriptionPairListProvider, new()
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
			=> new TListProvider();

		protected abstract ICodeDescriptionPairList GetExpectedCodeDescriptionPairList();

		public override void TestIsReturningCorrectCollection()
		{
			var expectedList = GetExpectedCodeDescriptionPairList();
			var listProvider = CreateCodeDescriptionPairListProvider();
			var list = listProvider.GetCodeDescriptionPairList();

			AssertListEqual(list, expectedList);
		}
	}
}
