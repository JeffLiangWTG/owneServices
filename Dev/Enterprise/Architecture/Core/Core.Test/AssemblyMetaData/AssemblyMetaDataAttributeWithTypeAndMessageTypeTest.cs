using System;
using CargoWise.Definitions;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Test
{
	[TestedType(typeof(AssemblyMetaDataAttributeWithTypeAndMessageType))]
	sealed class AssemblyMetaDataAttributeWithTypeAndMessageTypeTest : AssemblyMetaDataAttributeTestCase<AssemblyMetaDataAttributeWithTypeAndMessageTypeForTesting>
	{
		public void TestEquals_AllPropertiesInEquals()
		{
			var attribute1 = GetAssemblyMetaDataAttributeForTesting();
			var attribute2 = GetAssemblyMetaDataAttributeForTesting();
			Assert(attribute1.Equals(attribute2));

			attribute1.MessageType = "MessageType";
			Assert(!attribute1.Equals(attribute2));

			attribute2.MessageType = "MessageType";
			Assert(attribute1.Equals(attribute2));
		}
	}

	[Serializable]
	sealed class
		AssemblyMetaDataAttributeWithTypeAndMessageTypeForTesting : AssemblyMetaDataAttributeWithTypeAndMessageType
	{ }
}
