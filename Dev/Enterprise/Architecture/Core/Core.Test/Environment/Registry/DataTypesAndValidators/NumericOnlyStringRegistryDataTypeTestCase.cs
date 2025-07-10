using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(NumericOnlyStringRegistryDataType))]
	sealed class NumericOnlyStringRegistryDataTypeTestCase : RegistryDataTypeTestCase<NumericOnlyStringRegistryDataType>
	{
		protected override NumericOnlyStringRegistryDataType GetNewDataType()
		{
			return new NumericOnlyStringRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(null, Encoding.Unicode.GetBytes("*** NULL ***")),
				new ValidSampleAndBinaryValueInDB("12345", Encoding.Unicode.GetBytes("12345")),
				new ValidSampleAndBinaryValueInDB("0011", Encoding.Unicode.GetBytes("0011"))
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { "1234A", "QWERTY", "!@#." };
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}
	}
}
