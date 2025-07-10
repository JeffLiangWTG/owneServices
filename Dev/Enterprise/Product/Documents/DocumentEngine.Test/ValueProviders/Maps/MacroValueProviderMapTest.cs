using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.ValueProviders.Testing
{
	[TestedType(typeof(MacroValueProviderMap))]
	sealed class MacroValueProviderMapTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var macroValueProviderMap = new MacroValueProviderMap("TestMacro", "<TestMacro>", "A theoretical macro that does not actaully exist and is not used in the system");
			AssertEquals("MacroValueProviderMap Usage cannot be set or read correctly", "<TestMacro>", macroValueProviderMap.Usage);
			AssertEquals("MacroValueProviderMap Description cannot be set or read correctly", "A theoretical macro that does not actaully exist and is not used in the system", macroValueProviderMap.Description);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MacroValueProviderMap(ZString.Empty, ZString.Empty, ZString.Empty);
		}
	}
}
