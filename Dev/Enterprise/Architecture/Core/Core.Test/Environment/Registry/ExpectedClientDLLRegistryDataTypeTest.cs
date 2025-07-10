using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(ExpectedClientDLLRegistryDataType))]
	class ExpectedClientDLLRegistryDataTypeTest : RegistryDataTypeTestCase<ExpectedClientDLLRegistryDataType>
	{
		protected override ExpectedClientDLLRegistryDataType GetNewDataType()
		{
			return new ExpectedClientDLLRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(null, Encoding.Unicode.GetBytes("*** NULL ***")),
				new ValidSampleAndBinaryValueInDB("", Encoding.Unicode.GetBytes("")),
				new ValidSampleAndBinaryValueInDB("ZClientABC", Encoding.Unicode.GetBytes("ZClientABC"))
			};
		}

		protected override object GetNullRepresentation()
		{
			return Encoding.Unicode.GetBytes(StringRegistryDataType.MagicNullString);
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue => true;
	}
}
