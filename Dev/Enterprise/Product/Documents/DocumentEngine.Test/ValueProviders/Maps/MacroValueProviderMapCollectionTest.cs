using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.ValueProviders.Testing
{
	[TestedType(typeof(MacroValueProviderMapCollection))]
	sealed class MacroValueProviderMapCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MacroValueProviderMapCollection>
	{
		protected override MacroValueProviderMapCollection GetCollectionToTest()
		{
			return new MacroValueProviderMapCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MacroValueProviderMap(ZString.Empty, ZString.Empty, ZString.Empty);
		}
	}
}
