using System;
using CargoWise.Definitions;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Test
{
	[TestedType(typeof(AssemblyMetaDataAttributeWithTypeAndApplicationCode))]
	sealed class AssemblyMetaDataAttributeWithTypeAndApplicationCodeTest : AssemblyMetaDataAttributeTestCase<AssemblyMetaDataAttributeWithTypeAndApplicationCodeForTesting>
	{
		public void TestEquals_AllPropertiesInEquals()
		{
			var attribute1 = GetAssemblyMetaDataAttributeForTesting();
			var attribute2 = GetAssemblyMetaDataAttributeForTesting();
			Assert(attribute1.Equals(attribute2));

			attribute1.ApplicationCode = "ApplicationCode";
			Assert(!attribute1.Equals(attribute2));

			attribute2.ApplicationCode = "ApplicationCode";
			Assert(attribute1.Equals(attribute2));
		}
	}

	[Serializable]
	sealed class
		AssemblyMetaDataAttributeWithTypeAndApplicationCodeForTesting :
		AssemblyMetaDataAttributeWithTypeAndApplicationCode
	{ }
}
