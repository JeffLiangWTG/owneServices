using System;
using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(UriRegistryDataType))]
	sealed class UriRegistryDataTypeWithoutAutoPrefixingTest : StringRegistryDataTypeTest
	{
		protected override StringRegistryDataType GetNewDataType()
		{
			return new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false };
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			string server1 = "https://wisetechglobal.com";
			string server2 = "https://edidatglow.wisegrid.net/EnterpriseServices/Blah.svc";
			string server3 = "https://wisetechglobal.com?lang=en";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(server1, Encoding.Unicode.GetBytes(server1)),
				new ValidSampleAndBinaryValueInDB(server2, Encoding.Unicode.GetBytes(server2)),
				new ValidSampleAndBinaryValueInDB(server3, Encoding.Unicode.GetBytes(server3)),
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { @"q:\\sdljhsidudjaowjew", @"\\splatysplatsplat\c$\blah", "http://aaa.vvv", "ftp.nuzhnov.com", "ftp.nuzhnov.com/igor", "https.nuzhnov.com", "https.nuzhnov.com/igor" };
		}
	}
}
