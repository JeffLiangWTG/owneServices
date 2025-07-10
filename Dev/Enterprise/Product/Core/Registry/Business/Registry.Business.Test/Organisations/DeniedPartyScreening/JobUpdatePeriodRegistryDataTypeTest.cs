using System.Text;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(JobUpdatePeriodRegistryDataType))]
	sealed class JobUpdatePeriodRegistryDataTypeTest : IntRegistryDataTypeTest
	{
		protected override IntRegistryDataType GetNewDataType() => new JobUpdatePeriodRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(1, Encoding.Unicode.GetBytes("1")),
				new ValidSampleAndBinaryValueInDB(5, Encoding.Unicode.GetBytes("5")),
				new ValidSampleAndBinaryValueInDB(24, Encoding.Unicode.GetBytes("24")),
			};
		}
	}
}
