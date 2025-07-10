using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(BooleanRegistryDataType))]
	public class BooleanRegistryDataTypeTest : RegistryDataTypeTestCase<BooleanRegistryDataType>
	{
		public void TestValues()
		{
			string[] trueValues = { "Y", "1", "true", "yeS", "Hai", "YA" };
			foreach (string trueValue in trueValues)
			{
				AssertEquals("Deserialise()", true, DataType.Deserialise(Encoding.Unicode.GetBytes(trueValue)));
			}

			string[] falseValues = { "N", "0", "false", "nO" };
			foreach (string falseValue in falseValues)
			{
				AssertEquals("Deserialise()", false, DataType.Deserialise(Encoding.Unicode.GetBytes(falseValue)));
			}
		}

		protected override BooleanRegistryDataType GetNewDataType()
		{
			return new BooleanRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(true, Encoding.Unicode.GetBytes("1")),
				new ValidSampleAndBinaryValueInDB(false, Encoding.Unicode.GetBytes("0")),
				new ValidSampleAndBinaryValueInDB(true, Encoding.Unicode.GetBytes("true")),
				new ValidSampleAndBinaryValueInDB(false, Encoding.Unicode.GetBytes("false")),
				new ValidSampleAndBinaryValueInDB(true, Encoding.Unicode.GetBytes("True")),
				new ValidSampleAndBinaryValueInDB(false, Encoding.Unicode.GetBytes("False")),
				new ValidSampleAndBinaryValueInDB(true, Encoding.Unicode.GetBytes("Y")),
				new ValidSampleAndBinaryValueInDB(false, Encoding.Unicode.GetBytes("N")),
			};
		}
	}
}
