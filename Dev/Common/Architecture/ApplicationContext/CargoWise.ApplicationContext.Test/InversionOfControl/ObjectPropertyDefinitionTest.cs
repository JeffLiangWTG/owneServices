using NUnit.Framework;

namespace CargoWise.Application.InversionOfControl.Testing
{
	class ObjectPropertyDefinitionTest : TestCase
	{
		public void TestClone()
		{
			var objectDictionaryValueDefinition = new ObjectDictionaryValueDefinition()
			{
				Value = "Value4"
			};
			var objectListValueDefinition = new ObjectListValueDefinition()
			{
				ObjectName = "Value7"
			};
			var cloned = new ObjectPropertyDefinition()
			{
				Name = "Name1",
				EnableParallelInit = true,
				IsSubSet = true,
				ObjectValue = new ObjectDefinition()
				{
					Name = "Name2"
				},
				Value = "Value1",
				DictionaryValues = new[]
				{
					objectDictionaryValueDefinition,
					null,
					new ObjectDictionaryValueDefinition()
					{
						Value = "Value6"
					}
				},
				ListValues = new[]
				{
					objectListValueDefinition,
					null,
					new ObjectListValueDefinition()
					{
						ObjectName = "Value8"
					}
				}
			}.Clone();
			CombineAssertions(() =>
			{
				AssertEquals("cloned.Name", "Name1", cloned.Name);
				AssertEquals("cloned.EnableParallelInit", true, cloned.EnableParallelInit);
				AssertEquals("cloned.IsSubSet", true, cloned.IsSubSet);
				AssertEquals("cloned.ObjectValue.Name", "Name2", cloned.ObjectValue.Name);
				AssertEquals("cloned.Value", "Value1", cloned.Value);
				AssertEquals("cloned.DictionaryValues.Length", 3, cloned.DictionaryValues.Length);
				var clonedObjectDictionaryValueDefinition = cloned.DictionaryValues[0];
				Assert("cloned.DictionaryValues[0] should be cloned", !object.ReferenceEquals(objectDictionaryValueDefinition, clonedObjectDictionaryValueDefinition));
				AssertEquals("clonedObjectDictionaryValueDefinition.Value", "Value4", clonedObjectDictionaryValueDefinition.Value);
				AssertNull("cloned.DictionaryValues[1]", cloned.DictionaryValues[1]);
				AssertEquals("cloned.DictionaryValues[2].Value", "Value6", cloned.DictionaryValues[2].Value);
				AssertEquals("cloned.ListValues.Length", 3, cloned.ListValues.Length);
				var clonedObjectListValueDefinition = cloned.ListValues[0];
				Assert("cloned.ListValues[0] should be cloned", !object.ReferenceEquals(objectListValueDefinition, clonedObjectListValueDefinition));
				AssertEquals("clonedObjectListValueDefinition.ObjectName", "Value7", clonedObjectListValueDefinition.ObjectName);
				AssertNull("cloned.ListValues[1].ObjectName", cloned.ListValues[1]);
				AssertEquals("cloned.ListValues[2].ObjectName", "Value8", cloned.ListValues[2].ObjectName);
			});
		}
	}
}
