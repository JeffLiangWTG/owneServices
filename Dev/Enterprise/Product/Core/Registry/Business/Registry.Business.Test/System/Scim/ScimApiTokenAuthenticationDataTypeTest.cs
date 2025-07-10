using System;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ScimApiTokenAuthenticationDataType))]
	internal class ScimApiTokenAuthenticationDataTypeTest : StringRegistryDataTypeTest
	{
		protected override StringRegistryDataType GetNewDataType()
		{
			return new ScimApiTokenAuthenticationDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB("29BFE669-A68D-4C57-9A42-4696C6A18CCA", DataType.Serialise("29BFE669-A68D-4C57-9A42-4696C6A18CCA")),
				new ValidSampleAndBinaryValueInDB("AD9E9612-D5F9-45AF-AB14-15F0D7347A74", DataType.Serialise("AD9E9612-D5F9-45AF-AB14-15F0D7347A74"))
			};
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}

		protected override object[] GetInvalidSamples()
		{
			return Array.Empty<object>();
		}
	}
}
