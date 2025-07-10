using System.Net;
using CargoWise.EntityFramework.Testing;
using Enterprise.Dash.Business.Extensions;

namespace Enterprise.Dash.Business.Tests.Extensions
{
	sealed class HttpStatusCodeExtensionsTests : TestCaseWithFactory
	{
		public void TestCanRetryShouldReturnTrueForRetryableStatusCodes()
		{
			AssertEquals(true, HttpStatusCode.RequestTimeout.CanRetry());
			AssertEquals(true, HttpStatusCode.InternalServerError.CanRetry());
			AssertEquals(true, HttpStatusCode.ServiceUnavailable.CanRetry());
			AssertEquals(true, HttpStatusCode.GatewayTimeout.CanRetry());
		}

		public void TestCanRetryShouldReturnFalseForNonRetryableStatusCodes()
		{
			AssertEquals(false, HttpStatusCode.OK.CanRetry());
			AssertEquals(false, HttpStatusCode.BadRequest.CanRetry());
			AssertEquals(false, HttpStatusCode.NotFound.CanRetry());
		}
	}
}
