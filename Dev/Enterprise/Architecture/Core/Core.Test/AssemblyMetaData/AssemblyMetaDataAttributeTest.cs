using System;
using CargoWise.Definitions;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Test
{
	[TestedType(typeof(AssemblyMetaDataAttributeForTesting))]
	sealed class AssemblyMetaDataAttributeTest : AssemblyMetaDataAttributeTestCase<AssemblyMetaDataAttributeForTesting>
	{
		public void TestEquals_AllPropertiesInEquals()
		{
			var attribute1 = GetAssemblyMetaDataAttributeForTesting();
			var attribute2 = GetAssemblyMetaDataAttributeForTesting();
			Assert(attribute1.Equals(attribute2));

			attribute1.ClientSpecificCode = Clients.AAP;
			Assert(!attribute1.Equals(attribute2));

			attribute2.ClientSpecificCode = Clients.AAP;
			Assert(attribute1.Equals(attribute2));
		}
	}

	[Serializable]
	sealed class AssemblyMetaDataAttributeForTesting : AssemblyMetaDataAttribute
	{ }
}
