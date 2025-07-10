namespace Enterprise.Registry.Business.Testing
{
	sealed class CodeDescriptionBoolCollectionToTVPTestCase : RegistryCollectionToTVPTestCase<CodeDescriptionBoolCollection>
	{
		protected override CodeDescriptionBoolCollection GetCollectionToTest()
		{
			return new CodeDescriptionBoolCollection();
		}

		protected override CodeDescriptionBoolCollection PopulateCollection()
		{
			var collection = new CodeDescriptionBoolCollection();
			collection.Add(new CodeDescriptionBool() { Code = "CD1", EnglishDescription = "Code Description Bool 1" });
			collection.Add(new CodeDescriptionBool() { Code = "CD2", EnglishDescription = "Code Description Bool 2" });

			return collection;
		}
	}
}
