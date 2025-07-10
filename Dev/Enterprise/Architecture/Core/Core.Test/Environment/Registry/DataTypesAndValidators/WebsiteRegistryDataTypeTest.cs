using System;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(WebsiteRegistryDataType))]
	sealed class WebsiteRegistryDataTypeTest : StringRegistryDataTypeTest
	{
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var http = "http://wisetechglobal.com/Website";
			var https = "https://wisetechglobal.com/Website";
			var withQuery = "http://wisetechglobal.com/Website?key=value";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(http, Encoding.Unicode.GetBytes(http)),
				new ValidSampleAndBinaryValueInDB(https, Encoding.Unicode.GetBytes(https)),
				new ValidSampleAndBinaryValueInDB(withQuery, Encoding.Unicode.GetBytes(withQuery)),
			};
		}

		protected override object[] GetInvalidSamples()
		{
			var relativeUri = "~/Website/Login.aspx";
			var invalidSchemes = new[]
			{
				Uri.UriSchemeFile,
				Uri.UriSchemeFtp,
				Uri.UriSchemeGopher,
				Uri.UriSchemeMailto,
				Uri.UriSchemeNetPipe,
				Uri.UriSchemeNetTcp,
				Uri.UriSchemeNews,
				Uri.UriSchemeNntp,
			};

			var invalidUris = invalidSchemes.Select(x => (object)$"{x}://wisetechglobal.com/Website").ToList();
			invalidUris.Add(relativeUri);

			return invalidUris.ToArray();
		}

		protected override StringRegistryDataType GetNewDataType() => new WebsiteRegistryDataType();
	}
}
