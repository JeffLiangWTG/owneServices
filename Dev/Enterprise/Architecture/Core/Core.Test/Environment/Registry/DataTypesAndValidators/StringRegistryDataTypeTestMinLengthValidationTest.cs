using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(StringRegistryDataType))]
	sealed class StringRegistryDataTypeTestMinLengthValidationTest : StringRegistryDataTypeTest
	{
		protected override StringRegistryDataType GetNewDataType()
		{
			return new StringRegistryDataType(5, 5);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(null, Encoding.Unicode.GetBytes("*** NULL ***")),
				new ValidSampleAndBinaryValueInDB("12345", Encoding.Unicode.GetBytes("12345")),
				new ValidSampleAndBinaryValueInDB("chars", Encoding.Unicode.GetBytes("chars"))
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { "1234", "123456" };
		}
	}
}
