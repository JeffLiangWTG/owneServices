using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class UriRegistryDataTypeTest : TestCase
	{
		public void TestAllowAutoProtocolPrefixingDefaultsToTrue()
		{
			var schemesToTest = new[]
			{
				Uri.UriSchemeFile,
				Uri.UriSchemeFtp,
				Uri.UriSchemeGopher,
				Uri.UriSchemeHttp,
				Uri.UriSchemeHttps,
				Uri.UriSchemeMailto,
				Uri.UriSchemeNetPipe,
				Uri.UriSchemeNetTcp,
				Uri.UriSchemeNews,
				Uri.UriSchemeNntp,
			};

			foreach (var scheme in schemesToTest)
			{
				var dataType = new UriRegistryDataType(scheme);
				AssertEquals(true, dataType.AllowAutoProtocolPrefixing);
			}
		}

		public void TestAllowQueryStringDefaultsToTrue()
		{
			var schemesToTest = new[]
			{
				Uri.UriSchemeFile,
				Uri.UriSchemeFtp,
				Uri.UriSchemeGopher,
				Uri.UriSchemeHttp,
				Uri.UriSchemeHttps,
				Uri.UriSchemeMailto,
				Uri.UriSchemeNetPipe,
				Uri.UriSchemeNetTcp,
				Uri.UriSchemeNews,
				Uri.UriSchemeNntp,
			};

			foreach (var scheme in schemesToTest)
			{
				var dataType = new UriRegistryDataType(scheme);
				AssertEquals(true, dataType.AllowQueryString);
			}
		}
	}
}
