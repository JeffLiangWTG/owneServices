using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(IntRegistryDataType))]
	public class IntRegistryDataTypeTest : RegistryDataTypeTestCase<IntRegistryDataType>
	{
		protected override IntRegistryDataType GetNewDataType()
		{
			return new IntRegistryDataType(-1000, 2000);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(-1000, Encoding.Unicode.GetBytes("-1000")),
				new ValidSampleAndBinaryValueInDB(0, Encoding.Unicode.GetBytes("0")),
				new ValidSampleAndBinaryValueInDB(2000, Encoding.Unicode.GetBytes("2000"))
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { -1001, 2001 };
		}
	}
}
