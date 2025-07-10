using System;
using System.Text;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public class Base64UrlUtilityTest : TestCase
	{
		const string initialString = "hello base64 world!♫▓Һǿǿ";    // Generates '+' and '/' and '=' as base64.

		public void TestToUrlSafeBase64()
		{
			var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(initialString));
			var expectedUrlBase64 = "aGVsbG8gYmFzZTY0IHdvcmxkIeKZq-KWk9K6x7_Hvw";
			AssertEquals(expectedUrlBase64, Base64UrlUtility.ToUrlSafeBase64(base64));
		}

		public void TestFromUrlSafeBase64()
		{
			var urlBase64 = "aGVsbG8gYmFzZTY0IHdvcmxkIeKZq-KWk9K6x7_Hvw";
			var expectedBase64 = "aGVsbG8gYmFzZTY0IHdvcmxkIeKZq+KWk9K6x7/Hvw==";
			AssertEquals(expectedBase64, Base64UrlUtility.FromUrlSafeBase64(urlBase64));
		}

		public void TestRoundTrip()
		{
			var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(initialString));
			var urlBase64 = Base64UrlUtility.ToUrlSafeBase64(base64);
			var roundTrippedBase64 = Base64UrlUtility.FromUrlSafeBase64(urlBase64);
			AssertEquals("Calling To and From should give you the same base64 string", base64, roundTrippedBase64);
		}
	}
}
