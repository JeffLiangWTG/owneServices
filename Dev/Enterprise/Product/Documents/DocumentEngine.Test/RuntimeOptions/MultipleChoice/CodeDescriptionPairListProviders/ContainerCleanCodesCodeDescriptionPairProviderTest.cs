namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class ContainerCleanCodesCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			var collection = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			var codes = collection.GetAllCodes();
			AssertCollectionContains("DRC", codes);
			AssertCollectionContains("DRT", codes);
			AssertCollectionContains("POL", codes);
		}

		#region Implementation

		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new ContainerCleanCodesCodeDescriptionPairProvider();
		}

		#endregion
	}
}
