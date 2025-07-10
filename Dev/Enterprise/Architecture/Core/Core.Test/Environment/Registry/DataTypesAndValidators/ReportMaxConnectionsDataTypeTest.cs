using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(ReportMaxConnectionsDataType))]
	sealed class ReportMaxConnectionsDataTypeTest : RegistryDataTypeTestCase<ReportMaxConnectionsDataType>
	{
		protected override ReportMaxConnectionsDataType GetNewDataType()
		{
			return new ReportMaxConnectionsDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(0, Encoding.Unicode.GetBytes("0")),
				new ValidSampleAndBinaryValueInDB(5, Encoding.Unicode.GetBytes("5")),
				new ValidSampleAndBinaryValueInDB(10, Encoding.Unicode.GetBytes("10")),
			};
		}
	}
}
