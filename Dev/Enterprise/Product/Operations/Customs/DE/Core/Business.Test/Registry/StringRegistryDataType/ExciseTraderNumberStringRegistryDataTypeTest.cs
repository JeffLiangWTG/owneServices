using System.Text;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Registry.Testing
{
	[TestedType(typeof(ExciseTraderNumberStringRegistryDataType))]
	public class ExciseTraderNumberStringRegistryDataTypeTest : StringRegistryDataTypeTest
	{
		protected override StringRegistryDataType GetNewDataType() => new ExciseTraderNumberStringRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples() => new[]
		{
				new ValidSampleAndBinaryValueInDB(null, Encoding.Unicode.GetBytes("*** NULL ***")),
				new ValidSampleAndBinaryValueInDB("DE12345678901", Encoding.Unicode.GetBytes("DE12345678901")),
				new ValidSampleAndBinaryValueInDB("DE11111111111", Encoding.Unicode.GetBytes("DE11111111111")),
		};

		protected override object[] GetInvalidSamples() => new object[] { "%$!" };
	}
}
