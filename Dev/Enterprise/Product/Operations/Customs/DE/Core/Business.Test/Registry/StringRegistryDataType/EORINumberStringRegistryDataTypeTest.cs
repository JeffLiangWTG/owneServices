using System.Text;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Registry.Testing
{
	[TestedType(typeof(EORINumberStringRegistryDataType))]
	public class EORINumberStringRegistryDataTypeTest : StringRegistryDataTypeTest
	{
		protected override StringRegistryDataType GetNewDataType() => new EORINumberStringRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples() => new[]
		{
				new ValidSampleAndBinaryValueInDB(null, Encoding.Unicode.GetBytes("*** NULL ***")),
				new ValidSampleAndBinaryValueInDB("DE1", Encoding.Unicode.GetBytes("DE1")),
				new ValidSampleAndBinaryValueInDB("DE123456789012345", Encoding.Unicode.GetBytes("DE123456789012345")),
				new ValidSampleAndBinaryValueInDB("GR1-Z", Encoding.Unicode.GetBytes("GR1-Z")),
				new ValidSampleAndBinaryValueInDB("GR123456789012345", Encoding.Unicode.GetBytes("GR123456789012345")),
		};

		protected override object[] GetInvalidSamples() => new object[]
		{
			"DE",
			"DE1234567890123456"
		};
	}
}
