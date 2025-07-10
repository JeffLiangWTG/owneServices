using NUnit.Framework;

namespace CargoWise.Application.InversionOfControl.Testing
{
	class ObjectConstructorArgumentDefinitionTest : TestCase
	{
		public void TestClone()
		{
			var objectListValueDefinition = new ObjectListValueDefinition()
			{
				ObjectName = "foo2",
			};
			var cloned = new ObjectConstructorArgumentDefinition()
			{
				Index = 4,
				ObjectName = "foo",
				ListValues = new[]
				{
					objectListValueDefinition,
					null,
					new ObjectListValueDefinition()
					{
						ObjectName = "foo3",
					}
				}
			}.Clone();

			CombineAssertions(() =>
			{
				AssertEquals("cloned.Index", 4, cloned.Index);
				AssertEquals("cloned.ObjectName", "foo", cloned.ObjectName);
				AssertEquals("cloned.ListValues.Length", 3, cloned.ListValues.Length);
				var clonedObjectListValueDefinition = cloned.ListValues[0];
				Assert("Should be cloned", !object.ReferenceEquals(objectListValueDefinition, clonedObjectListValueDefinition));
				AssertEquals("clonedObjectListValueDefinition.ObjectName", "foo2", clonedObjectListValueDefinition.ObjectName);
				AssertNull("cloned.ListValues[1]", cloned.ListValues[1]);
				AssertEquals("cloned.ListValues[2].ObjectName", "foo3", cloned.ListValues[2].ObjectName);
			});
		}
	}
}
