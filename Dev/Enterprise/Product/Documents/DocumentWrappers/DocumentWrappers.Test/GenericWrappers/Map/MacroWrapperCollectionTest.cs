using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map.Testing
{
	[TestedType(typeof(MacroWrapperCollection))]
	sealed class MacroWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<MacroWrapperCollection>
	{
		public void TestCollectionSortOrder()
		{
			var providerA = new MacroValueProviderMap("A", "<A>", "A is the way...");
			var providerAA = new MacroValueProviderMap("AA", "<AA>", "AA is the way...");
			var providerB = new MacroValueProviderMap("B", "<B>", "B is the way...");
			var providerC = new MacroValueProviderMap("C", "<C>", "C is the way...");

			var valueProviders = new MacroValueProviderMapCollection();
			valueProviders.Add(providerB);
			valueProviders.Add(providerA);
			valueProviders.Add(providerC);
			valueProviders.Add(providerAA);

			MacroWrapperCollection collection = new MacroWrapperCollection(valueProviders, Factory);
			AssertEquals("collection[0].Useage", "<A>", collection[0].Useage);
			AssertEquals("collection[1].Useage", "<AA>", collection[1].Useage);
			AssertEquals("collection[2].Useage", "<B>", collection[2].Useage);
			AssertEquals("collection[3].Useage", "<C>", collection[3].Useage);
		}

		protected override MacroWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new MacroWrapperCollection(null, Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new MacroWrapper(null, Factory);
		}
	}
}
