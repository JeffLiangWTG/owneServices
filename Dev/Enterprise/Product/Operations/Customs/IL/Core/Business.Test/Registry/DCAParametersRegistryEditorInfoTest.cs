using System;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DCAParametersRegistryEditorInfoTest : TestCase
	{
		public void TestBaseDataType()
		{
			AssertEquals(typeof(DCAParametersRegistryDataType), new DCAParametersRegistryEditorInfo().BaseDataTypeToBeEdited);
		}

		public void TestEditorClassAndAssembly()
		{
			var expectedType = Type.GetType("Enterprise.Customs.IL.GUI.DCAParametersRegistryEditor, Enterprise.Customs.IL.GUI");
			AssertNotNull("AttributeMapRegistryEditor should be a valid type", expectedType);

			var attribute = typeof(DCAParametersRegistryEditorInfo).GetCustomAttributes(typeof(RegistryEditorAttribute), inherit: false)[0] as RegistryEditorAttribute;
			AssertNotNull("RegistryEditorAttribute should be present", attribute);

			string expectedName = string.Format("{0}, {1}", expectedType.FullName, expectedType.Assembly.GetName().Name);
			AssertEquals("Attribute should specify fully-qualified type with assembly", expectedName, attribute.TypeName);
		}
	}
}
