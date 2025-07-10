using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(ManifestClientIDDataType))]
	sealed class ManifestClientIDDataTypeTest : StringRegistryDataTypeTest
	{
		protected override StringRegistryDataType GetNewDataType()
		{
			return new ManifestClientIDDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB("a123456789", Encoding.Unicode.GetBytes("a123456789")),
				new ValidSampleAndBinaryValueInDB("f098765432", Encoding.Unicode.GetBytes("f098765432")),
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { "1234567890", "aa12345678", "a1234567890" };
		}
	}
}
