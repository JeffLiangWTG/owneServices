using System;
using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(UriRegistryDataType))]
	sealed class UriRegistryDataTypeWithAutoPrefixingTest : StringRegistryDataTypeTest
	{
		protected override StringRegistryDataType GetNewDataType()
		{
			return new UriRegistryDataType(Uri.UriSchemeFtp) { AllowAutoProtocolPrefixing = true };
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			string server1 = "ftp://ftp.nuzhnov.com";
			string server2 = "ftp.nuzhnov.com";
			string server3 = "ftp://ftp.nuzhnov.com/igor";
			string server4 = "ftp.nuzhnov.com/igor";
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(server1, Encoding.Unicode.GetBytes(server1)),
				new ValidSampleAndBinaryValueInDB(server2, Encoding.Unicode.GetBytes(server2)),
				new ValidSampleAndBinaryValueInDB(server3, Encoding.Unicode.GetBytes(server3)),
				new ValidSampleAndBinaryValueInDB(server4, Encoding.Unicode.GetBytes(server4)),
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { @"q:\\sdljhsidudjaowjew", @"\\splatysplatsplat\c$\blah", "http://aaa.vvv" };
		}
	}
}
