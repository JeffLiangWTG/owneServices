using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(StringRegistryDataTypeWithExcludedCharactersDataType))]
	sealed class StringRegistryDataTypeWithExcludedCharactersDataTypeTest : StringRegistryDataTypeTest
	{
		protected override StringRegistryDataType GetNewDataType()
		{
			return new StringRegistryDataTypeWithExcludedCharactersDataType(0, 20, new char[] { '.' });
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(null, Encoding.Unicode.GetBytes("*** NULL ***")),
				new ValidSampleAndBinaryValueInDB("abc", Encoding.Unicode.GetBytes("abc")),
				new ValidSampleAndBinaryValueInDB("123", Encoding.Unicode.GetBytes("123")),
				new ValidSampleAndBinaryValueInDB("abc123", Encoding.Unicode.GetBytes("abc123")),
				new ValidSampleAndBinaryValueInDB("abc123!\"#;%:?*(),'", Encoding.Unicode.GetBytes("abc123!\"#;%:?*(),'"))
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { "abc.", ".", "...", "abc.abc" };
		}
	}
}
