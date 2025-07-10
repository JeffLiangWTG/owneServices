using System;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.Design.Testing
{
	class NoCodeDomSerializerInCargoWiseDesignTestCase : TestCase
	{
		public void TestIt()
		{
			Assembly cargowiseDesign = GetType().Assembly;
			AssertEquals("Expect assembly to be correct", "CargoWise.Design.Test", cargowiseDesign.GetName().Name);
			foreach (Type type in cargowiseDesign.GetTypes())
			{
				if (typeof(CodeDomSerializer).IsAssignableFrom(type) && type.FullName.IndexOf("Test") == -1)
				{
					Fail(type.FullName + ": Subclasses of CodeDomSerializer cannot exist in CargoWise.Design because ITypeResolutionService doesn't load it in the ProjectAssemblies folder.");
				}
			}
		}
	}
}
