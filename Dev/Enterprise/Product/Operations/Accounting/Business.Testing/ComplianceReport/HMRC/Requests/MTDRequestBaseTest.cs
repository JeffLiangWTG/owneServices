using System;
using System.Linq;
using System.Net.Http.Headers;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC.Testing
{
	public abstract class MTDRequestBaseTest : TestCaseWithFactory
	{
		public abstract void TestSuccessfulResponse();

		public abstract void TestFailedResponse();

		public abstract void TestGetAsString();

		public virtual void TestRequestHeaderContent()
		{
			var company = CreateUKCompany();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = new MTDClient(report);
				var request = GetRequest(client);

				using (var httpRequest = request.GetAsHttpRequest(Helper.TestHttpClient))
				{
					var requestHeader = httpRequest.Headers;
					AssertHeaderContainsKey(requestHeader, "Accept", "application/vnd.hmrc.1.0+json");
					AssertHeaderContainsKey(requestHeader, "Authorization", "Bearer");
				}
			}
		}

		public virtual void TestRequestBodyContent()
		{
			var company = CreateUKCompany();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = new MTDClient(report);
				var request = GetRequest(client);

				using (var httpRequest = request.GetAsHttpRequest(Helper.TestHttpClient))
				{
					var requestBody = httpRequest.Content;
					AssertNull(requestBody);
				}
			}
		}

		protected abstract MTDRequestBase GetRequest(MTDClient client);

		protected void AssertHeaderContainsKey(HttpRequestHeaders headers, string key, string expectedValue)
		{
			if (headers.TryGetValues(key, out var values))
			{
				AssertEquals(1, values.Count());
				AssertEquals(expectedValue, values.First());
			}
			else
			{
				Assert(FormattableString.Invariant($"Missing {key}"), false);
			}
		}

		protected void AssertContentHeaderContainsKey(HttpContentHeaders contentHeaders, string key, string expectedValue)
		{
			//Assert Content Header
			if (contentHeaders.TryGetValues(key, out var contentTypes))
			{
				AssertEquals(1, contentTypes.Count());
				AssertEquals(expectedValue, contentTypes.First());
			}
			else
			{
				Assert(FormattableString.Invariant($"Missing {key}"), false);
			}
		}

		protected void AssertErrorInfo(MTDErrorInfo errorInfo
			, string expectedHeaderErrorCode, string expectedHeaderErrorMessage
			, params (string expectedItemErrorCode, string expectedItemErrorMessage, string expectedItemErrorPath)[] expectedErrorItems)
		{
			AssertEquals("code", expectedHeaderErrorCode, errorInfo.code);
			AssertEquals("message", expectedHeaderErrorMessage, errorInfo.message);

			if (expectedErrorItems?.Any() ?? false)
			{
				var errorItems = errorInfo.errors?.ToArray();
				AssertEquals(expectedErrorItems.Length, errorItems.Length);

				for (int i = 0; i < expectedErrorItems.Length; i++)
				{
					var expectedErrorItem = expectedErrorItems[i];
					var errorItem = errorItems[i];
					AssertEquals("code", expectedErrorItem.expectedItemErrorCode, errorItem.code);
					AssertEquals("message", expectedErrorItem.expectedItemErrorMessage, errorItem.message);
					AssertEquals("path", expectedErrorItem.expectedItemErrorPath, errorItem.path);
				}
			}
			else
			{
				AssertNull(errorInfo.errors);
			}
		}

		protected AccComplianceReport SetupReportAndConfiguration(ZDate from, ZDate to)
		{
			return MTDTestHelper.SetupReportAndConfiguration(ObjectCreator, Factory, from, to);
		}

		protected MTDClient SetupClient(AccComplianceReport report)
		{
			return MTDTestHelper.SetupClient(report, ClientHandlerMock);
		}

		protected GlbCompany CreateUKCompany()
		{
			return MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ClientHandlerMock = Helper.MTDHttpClientHandler;
			Helper.SetupMockHttpClientForMTD();
		}

		protected MTDTestHelper Helper => helper ?? (helper = new MTDTestHelper());
		MTDTestHelper helper;

		internal MTDHttpClientHandlerMock ClientHandlerMock { get; private set; }

		protected TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;
	}
}
