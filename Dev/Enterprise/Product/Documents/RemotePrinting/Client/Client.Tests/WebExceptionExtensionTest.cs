using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using WTG.ErrorReporting.Extensibility;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class WebExceptionExtensionTest : TestCase
	{
		public void TestAnalyze()
		{
			var response = new WebResponseForTest();
			response.Headers.Add("name1", "value11");
			response.Headers.Add("name1", "value12");
			response.Headers.Add("name2", "value2");

			var webException = new WebException("Test error", null, WebExceptionStatus.UnknownError, response);

			var writer = new ExceptionDetailsWriterForTest();
			var context = new ExceptionContext(webException, writer);

			var webExceptionExtension = new WebExceptionExtension();
			((IAdditionalDetailContributor)webExceptionExtension).Analyze(context);

			AssertEquals("http://some.url/address", writer.WrittenDetails["ResponseUri"]);

			const string expectedHeaders =
@"name1: value11,value12
name2: value2";
			AssertEquals(expectedHeaders, writer.WrittenDetails["ResponseHeaders"]);
		}
	}

	class ExceptionDetailsWriterForTest : IExceptionDetailsWriter
	{
		public Dictionary<string, string> WrittenDetails { get; } = new Dictionary<string, string>();

		public void WriteDetail(string key, string value)
		{
			WrittenDetails.Add(key, value); // Will fail if same key is used twice, this fill then fail unit test.
		}
	}

	class WebResponseForTest : WebResponse
	{
		public override Uri ResponseUri => new Uri("http://some.url/address");

		public override bool SupportsHeaders => true;

		public override WebHeaderCollection Headers { get; } = new WebHeaderCollection();
	}
}
