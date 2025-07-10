#if DEBUG
using System;
using System.ComponentModel;
using System.Reflection;
using CargoWise.ComponentModel.Design;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class CompileTimeCheckBindingMemberCollectionTests : TestCase
	{
		public void TestAppliedCorrectlyInArchitectureAssemblies()
		{
			foreach (Assembly assembly in AssembliesToCheckAttributesOn.GetAssemblies())
			{
				foreach (Type type in assembly.GetExportedTypes())
				{
					if (!type.IsGenericTypeDefinition)
					{
						foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(type))
						{
							if (property.PropertyType == typeof(CompileTimeCheckBindingMemberCollection))
							{
								DesignerSerializationVisibilityAttribute attr = (DesignerSerializationVisibilityAttribute)property.Attributes[typeof(DesignerSerializationVisibilityAttribute)];
								AssertEquals("Should serialize property " + property.Name + " on component " + type.FullName + " with DesignerSerializationVisibility.Content", DesignerSerializationVisibility.Content, attr.Visibility);
							}
						}
					}
				}
			}
			Assert(true);
		}
	}
}
#endif
