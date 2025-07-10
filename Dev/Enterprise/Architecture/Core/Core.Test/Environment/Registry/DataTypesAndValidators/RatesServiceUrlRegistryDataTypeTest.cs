using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(RatesServiceUrlRegistryDataType))]
	sealed class RatesServiceUrlRegistryDataTypeTest : RegistryDataTypeTestCase<RatesServiceUrlRegistryDataType>
	{
		protected override RatesServiceUrlRegistryDataType GetNewDataType()
		{
			return new RatesServiceUrlRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB("http://www.google.com.au/", Encoding.Unicode.GetBytes("http://www.google.com.au/")),
				new ValidSampleAndBinaryValueInDB(string.Empty, Encoding.Unicode.GetBytes(StringRegistryDataType.MagicNullString)),
			};
		}
	}
}
