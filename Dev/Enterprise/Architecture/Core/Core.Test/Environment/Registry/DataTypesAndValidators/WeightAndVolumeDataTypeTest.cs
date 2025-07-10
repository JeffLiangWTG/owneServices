using System.Text;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(WeightAndVolumeDataType))]
	sealed class WeightAndVolumeDataTypeTest : StringRegistryDataTypeTest
	{
		protected override StringRegistryDataType GetNewDataType()
		{
			return new WeightAndVolumeDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB("", Encoding.Unicode.GetBytes("")),
				new ValidSampleAndBinaryValueInDB(WeightAndVolumeDisplayTypes.Codes.Actual, Encoding.Unicode.GetBytes(WeightAndVolumeDisplayTypes.Codes.Actual)),
				new ValidSampleAndBinaryValueInDB(WeightAndVolumeDisplayTypes.Codes.Carrier, Encoding.Unicode.GetBytes(WeightAndVolumeDisplayTypes.Codes.Carrier)),
				new ValidSampleAndBinaryValueInDB(WeightAndVolumeDisplayTypes.Codes.Client, Encoding.Unicode.GetBytes(WeightAndVolumeDisplayTypes.Codes.Client)),
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { "xxx" };
		}

		protected override void AssertValuesEqualForCheckingDefault(string message, object lhs, object rhs)
		{
			if (lhs is string && rhs == null ||
				rhs is string && lhs == null)
			{
			}
			else
			{
				base.AssertValuesEqual(message, lhs, rhs);
			}
		}
	}
}
