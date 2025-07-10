using NUnit.Framework;

namespace CargoWise.Application.InversionOfControl.Testing
{
	class ObjectListValueDefinitionTest : TestCase
	{
		public void TestClone()
		{
			var cloned = new ObjectListValueDefinition()
			{
				ObjectName = "Value1"
			};
			AssertEquals("cloned.ObjectName", "Value1", cloned.ObjectName);
		}
	}
}
