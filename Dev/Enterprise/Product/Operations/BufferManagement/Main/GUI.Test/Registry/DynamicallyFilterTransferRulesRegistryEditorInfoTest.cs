using Enterprise.BufferManagement.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class DynamicallyFilterTransferRulesRegistryEditorInfoTest : TestCase
	{
		public void TestDynamicallyFilterTransferRulesRegistryEditorInfo_ShouldHaveRegistryEditorAttribute()
		{
			var attributes = typeof(DynamicallyFilterTransferRulesRegistryEditorInfo).GetCustomAttributes(typeof(RegistryEditorAttribute), false);

			AssertEquals("Has the RegistryEditor attribute", 1, attributes.Length);

			var atribute = attributes[0] as RegistryEditorAttribute;
			var assemblyQualifiedName = typeof(DynamicallyFilterTransferRulesEditorInfoRegistryEditor).AssemblyQualifiedName;
			var secondCommaIndex = assemblyQualifiedName.IndexOf(',', assemblyQualifiedName.IndexOf(',') + 1);
			var expectTypeName = assemblyQualifiedName.Substring(0, secondCommaIndex);

			AssertEquals("has correct typeName", expectTypeName, atribute.TypeName);
		}
	}
}
