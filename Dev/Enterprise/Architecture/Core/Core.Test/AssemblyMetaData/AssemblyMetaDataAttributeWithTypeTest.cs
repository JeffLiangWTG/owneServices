using System;
using CargoWise.Definitions;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Test
{
	[TestedType(typeof(AssemblyMetaDataAttributeWithType))]
	sealed class AssemblyMetaDataAttributeWithTypeTest : AssemblyMetaDataAttributeTestCase<AssemblyMetaDataAttributeWithTypeForTesting>
	{
		public void TestEquals_AllPropertiesInEquals()
		{
			var attribute1 = GetAssemblyMetaDataAttributeForTesting();
			var attribute2 = GetAssemblyMetaDataAttributeForTesting();
			Assert(attribute1.Equals(attribute2));

			attribute1.TypeName = "TypeName";
			Assert(!attribute1.Equals(attribute2));

			attribute2.TypeName = "TypeName";
			Assert(attribute1.Equals(attribute2));

			attribute1.TypeAssemblyName = "TypeAssemblyName";
			Assert(!attribute1.Equals(attribute2));

			attribute2.TypeAssemblyName = "TypeAssemblyName";
			Assert(attribute1.Equals(attribute2));
		}
	}

	[Serializable]
	sealed class AssemblyMetaDataAttributeWithTypeForTesting : AssemblyMetaDataAttributeWithType
	{ }
}
