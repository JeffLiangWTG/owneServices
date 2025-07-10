using NUnit.Framework;

namespace CargoWise.Application.InversionOfControl.Testing
{
	class ObjectDictionaryValueKeyDefinitionTest : TestCase
	{
		public void TestClone()
		{
			var cloned = new ObjectDictionaryValueKeyDefinition()
			{
				Value = "Value1"
			};
			AssertEquals("cloned.Value", "Value1", cloned.Value);
		}
	}
}
