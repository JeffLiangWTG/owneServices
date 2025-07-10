using System;
using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(UriRegistryDataType))]
	sealed class UriRegistryDataTypeWithoutQueryStringTest : StringRegistryDataTypeTest
	{
		protected override StringRegistryDataType GetNewDataType()
		{
			return new UriRegistryDataType(Uri.UriSchemeHttps) { AllowQueryString = false };
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			string server1 = "https://wisetechglobal.com";
			string server2 = "https://edidatglow.wisegrid.net/EnterpriseServices/Blah.svc";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(server1, Encoding.Unicode.GetBytes(server1)),
				new ValidSampleAndBinaryValueInDB(server2, Encoding.Unicode.GetBytes(server2)),
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { @"https://wisetechglobal.com?lang=en", @"https://wisetechglobal.com?", @"q:\\sdljhsidudjaowjew", @"\\splatysplatsplat\c$\blah", "http://aaa.vvv" };
		}
	}
}
