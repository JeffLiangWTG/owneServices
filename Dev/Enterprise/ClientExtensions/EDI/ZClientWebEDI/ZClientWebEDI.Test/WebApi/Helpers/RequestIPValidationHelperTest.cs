using System;
using System.Web;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Moq;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class RequestIPValidationHelperTest : TestCaseWithFactory
	{
		public void TestValidWithWhitelistingDisabled()
		{
			EDIDataRegistry.Instance.DisableMyAccountWhitelisting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var testCases = new[]
			{
				(true, "10.61.178.129"),
				(true , "9.0.0.1"),
			};

			CombineAssertions(() =>
			{
				foreach (var (valid, ipAddress) in testCases)
				{
					var mockRequest = new Mock<HttpWorkerRequest>();
					mockRequest.Setup(r => r.GetRemoteAddress()).Returns(ipAddress);
					mockRequest.Setup(o => o.GetRawUrl()).Returns("/api/mocked");
					HttpContext.Current = new HttpContext(mockRequest.Object);

					var result = requestIPValidationHelper.IsCurrentRequestFromValidIP();

					AssertEquals(valid, result.Item1);
					AssertEquals(ipAddress, result.Item2);
				}
			});
		}

		public void TestWithWhitelistingEnabled()
		{
			EDIDataRegistry.Instance.DisableMyAccountWhitelisting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var testCases = new[]
			{
				(true, "10.61.178.129"),
				(false , "9.0.0.1"),
			};

			CombineAssertions(() =>
			{
				foreach (var (valid, ipAddress) in testCases)
				{
					var mockRequest = new Mock<HttpWorkerRequest>();
					mockRequest.Setup(r => r.GetRemoteAddress()).Returns(ipAddress);
					mockRequest.Setup(o => o.GetRawUrl()).Returns("/api/mocked");
					HttpContext.Current = new HttpContext(mockRequest.Object);

					var result = requestIPValidationHelper.IsCurrentRequestFromValidIP();

					AssertEquals(valid, result.Item1);
					AssertEquals(ipAddress, result.Item2);
				}
			});
		}

		public void TestInvalidAddressInConfigShouldReportErrorButAllowAccess()
		{
			EDIDataRegistry.Instance.DisableMyAccountWhitelisting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var testCases = new[]
			{
				(true, "10.61.178.129"),
				(false , "9.0.0.1"),
			};

			var requestValidationHelper = new RequestIPValidationHelperForDeadAddressTest();

			CombineAssertions(() =>
			{
				foreach (var (valid, ipAddress) in testCases)
				{
					var mockRequest = new Mock<HttpWorkerRequest>();
					mockRequest.Setup(r => r.GetRemoteAddress()).Returns(ipAddress);
					mockRequest.Setup(o => o.GetRawUrl()).Returns("/api/mocked");
					HttpContext.Current = new HttpContext(mockRequest.Object);

					Tuple<bool, string> result = null;
					AssertNoExceptionThrown(() =>
					{
						result = requestValidationHelper.IsCurrentRequestFromValidIP();
					});

					AssertEquals(valid, result.Item1);
					AssertEquals(ipAddress, result.Item2);
					AssertEquals("WebServiceAccessList has an invalid hostname: invalid-dead-address. It should be removed from web.config.", ErrorReporter.LastMessageReported);
					AssertEquals("No such host is known", ErrorReporter.LastExceptionReported.Message);
					ErrorReporter.Clear();
				}
			});
		}

		readonly RequestIPValidationHelper requestIPValidationHelper = new RequestIPValidationHelperForTest();

		class RequestIPValidationHelperForTest : RequestIPValidationHelper
		{
			protected override string[] GetHostnames() => ["10.61.178.129", "localhost"];
		}

		class RequestIPValidationHelperForDeadAddressTest : RequestIPValidationHelper
		{
			protected override string[] GetHostnames() => ["10.61.178.129", "localhost", "invalid-dead-address"];
		}
	}
}
