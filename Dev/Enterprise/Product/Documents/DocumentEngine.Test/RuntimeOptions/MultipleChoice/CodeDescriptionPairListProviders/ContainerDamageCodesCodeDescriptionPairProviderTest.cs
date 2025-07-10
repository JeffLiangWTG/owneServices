namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class ContainerDamageCodesCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			var collection = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			var codes = collection.GetAllCodes();
			AssertCollectionContains("AVL", codes);
			AssertCollectionContains("AWI", codes);
			AssertCollectionContains("DAM", codes);
		}

		#region Implementation

		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new ContainerDamageCodesCodeDescriptionPairProvider();
		}

		#endregion
	}
}
