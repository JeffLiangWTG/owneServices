using NUnit.Framework;

namespace CargoWise.Application.InversionOfControl.Testing
{
	class ObjectDictionaryValueDefinitionTest : TestCase
	{
		public void TestClone()
		{
			var key = new ObjectDictionaryValueKeyDefinition()
			{
				Value = "Value1"
			};
			var cloned = new ObjectDictionaryValueDefinition()
			{
				Key = key,
				Value = "Value2"
			};
			AssertEquals("cloned.Value", "Value2", cloned.Value);
			Assert("Should be cloned", !object.ReferenceEquals(key, cloned));
			AssertEquals("cloned.Key.Value", "Value1", cloned.Key.Value);
		}
	}
}
