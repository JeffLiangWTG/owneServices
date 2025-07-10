using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(LocalDirectoryRegistryDataType))]
	sealed class LocalDirectoryRegistryDataTypeTest : StringRegistryDataTypeTest
	{
		protected override StringRegistryDataType GetNewDataType()
		{
			return new LocalDirectoryRegistryDataType();
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { @"\\splatysplatsplat\c$\blah", "C://*?Test" };
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(null, Encoding.Unicode.GetBytes("*** NULL ***")),
				new ValidSampleAndBinaryValueInDB("C:\\", Encoding.Unicode.GetBytes("C:\\")),		// Testing File Path
				new ValidSampleAndBinaryValueInDB("D:\\", Encoding.Unicode.GetBytes("D:\\")),		// Testing File Path
			};
		}
	}
}
