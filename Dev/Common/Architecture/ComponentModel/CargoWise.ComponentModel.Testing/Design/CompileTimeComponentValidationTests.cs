#if DEBUG
using System;
using System.ComponentModel;
using System.Reflection;
using CargoWise.ComponentModel.Design;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class CompileTimeComponentValidationTests : TestCase
	{
		public void TestAppliedCorrectlyInArchitectureAssemblies()
		{
			Assert(true); // in case there are no exported generic types to test

			foreach (Assembly assembly in AssembliesToCheckAttributesOn.GetAssemblies())
			{
				foreach (Type type in assembly.GetExportedTypes())
				{
					if (!type.IsGenericTypeDefinition)
					{
						foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(type))
						{
							if (typeof(CompileTimeComponentValidation).IsAssignableFrom(property.PropertyType))
							{
								AssertEquals(
									"Any property of type " + nameof(CompileTimeComponentValidation) + " should have a dummy setter so the error can actually be serialized",
									false, property.IsReadOnly);
							}
						}
					}
				}
			}
		}
	}
}
#endif
