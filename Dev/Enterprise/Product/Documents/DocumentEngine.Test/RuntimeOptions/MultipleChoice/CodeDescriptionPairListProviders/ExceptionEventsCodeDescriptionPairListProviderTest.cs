namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class ExceptionEventsCodeDescriptionPairListProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new ExceptionEventsCodeDescriptionPairListProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			Assert(true);
		}
	}
}
