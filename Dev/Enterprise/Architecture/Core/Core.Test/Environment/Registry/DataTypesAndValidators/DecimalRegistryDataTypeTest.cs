using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(DecimalRegistryDataType))]
	sealed class DecimalRegistryDataTypeTest : RegistryDataTypeTestCase<DecimalRegistryDataType>
	{
		protected override DecimalRegistryDataType GetNewDataType()
		{
			return new DecimalRegistryDataType(-1000, 2000);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(0m, Encoding.Unicode.GetBytes("0")),
				new ValidSampleAndBinaryValueInDB(-3.3333m, Encoding.Unicode.GetBytes("-3.3333"))
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { -1000.001m, 2000.001m };
		}
	}
}
