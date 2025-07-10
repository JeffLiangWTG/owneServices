namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class GLTransactionCategoryCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new GLPresentationCategoryCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			GLPresentationCategoryCodeDescriptionPairProvider testPairProvider = new GLPresentationCategoryCodeDescriptionPairProvider();

			Assert(true);
		}
	}
}
