using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(DirectoryRegistryDataType))]
	sealed class DirectoryDataTypeTest : StringRegistryDataTypeTest
	{
		protected override StringRegistryDataType GetNewDataType()
		{
			return new DirectoryRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(EnvProxy.Instance.TempPath, Encoding.Unicode.GetBytes(EnvProxy.Instance.TempPath)),
				new ValidSampleAndBinaryValueInDB(EnvProxy.Instance.ApplicationStartupPath, Encoding.Unicode.GetBytes(EnvProxy.Instance.ApplicationStartupPath))
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { @"q:\\sdljhsidudjaowjew", @"\\splatysplatsplat\c$\blah" };
		}
	}
}
