using System.Net.Http;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.Dash.Business.Extensions;

namespace Enterprise.Dash.Business.Tests.Extensions
{
	sealed class HttpContentExtensionsTests : TestCaseWithFactory
	{
		public void TestReadAsStringShouldReturnDummyContent()
		{
			var content = new StringContent("Dummy Content", Encoding.UTF8, "text/plain");
			var result = content.ReadAsString();
			AssertEquals("Dummy Content", result);
		}

		public void TestReadAsStringShouldReturnEmptyContent()
		{
			var content = new StringContent(string.Empty, Encoding.UTF8, "text/plain");
			var result = content.ReadAsString();
			AssertEquals(string.Empty, result);
		}
	}
}
