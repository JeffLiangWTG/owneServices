using System;
#if NETFRAMEWORK
using System.Net;
#else
using System.Net.Sockets;
#endif
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement.Test
{
	public class SystemToSystemTrustApiHelperTest : TransactionedTestCase
	{
		public void TestFormatApiErrorMessages()
		{
			var response = string.Empty;

			AssertNoExceptionThrown(() => response = SystemToSystemTrustApiHelper.FormatApiErrorMessages(400, null));
			AssertEquals("There was an error when calling the MyAccount Web Api. 400 : BadRequest", response);

			AssertNoExceptionThrown(() => response = SystemToSystemTrustApiHelper.FormatApiErrorMessages(400, ""));
			AssertEquals("There was an error when calling the MyAccount Web Api. 400 : BadRequest", response);

			AssertNoExceptionThrown(() => response = SystemToSystemTrustApiHelper.FormatApiErrorMessages(400, "{\"errors\":[null]}"));
			AssertEquals("There was an error when calling the MyAccount Web Api. 400 : BadRequest", response);

			AssertNoExceptionThrown(() => response = SystemToSystemTrustApiHelper.FormatApiErrorMessages(400, "{\"errors\":null}"));
			AssertEquals("There was an error when calling the MyAccount Web Api. 400 : BadRequest", response);

			AssertNoExceptionThrown(() => response = SystemToSystemTrustApiHelper.FormatApiErrorMessages(400, "{\"other_property\":[\"value\"]"));
			AssertEquals("There was an error when calling the MyAccount Web Api. 400 : {\"other_property\":[\"value\"]", response);

			AssertNoExceptionThrown(() => response = SystemToSystemTrustApiHelper.FormatApiErrorMessages(400, "Not a JSON"));
			AssertEquals("There was an error when calling the MyAccount Web Api. 400 : Not a JSON", response);

			AssertNoExceptionThrown(() => response = SystemToSystemTrustApiHelper.FormatApiErrorMessages(400, "{\"errors\":[{\"code\":\"500\",\"message\":\"Something wrong\"}]}"));
			AssertEquals("There was an error when calling the MyAccount Web Api. 500 : Something wrong", response);

			AssertNoExceptionThrown(() => response = SystemToSystemTrustApiHelper.FormatApiErrorMessages(400, "{\"errors\":[{\"code\":\"500\",\"message\":\"Something else wrong\"}, null]}"));
			AssertEquals("There was an error when calling the MyAccount Web Api. 500 : Something else wrong", response);
		}
#if NETFRAMEWORK
		public void TestHandleWebException()
		{
			using (WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://invalidwtg.wtg"))
			{
				var exception = AssertExceptionThrown<SystemToSystemTrustCertificateManagementException>(() => new SystemToSystemTrustApiHelper().SystemToSystemTrustApiGet("invalidurl"));

				AssertEquals("An error occurred while sending the request.", exception.Message);
				AssertEquals(typeof(WebException), exception.InnerException.GetType());

				exception = AssertExceptionThrown<SystemToSystemTrustCertificateManagementException>(() => new SystemToSystemTrustApiHelper().SystemToSystemTrustApiPost("invalidurl", new System.Net.Http.StringContent("")));

				AssertEquals("An error occurred while sending the request.", exception.Message);
				AssertEquals(typeof(WebException), exception.InnerException.GetType());
			}
		}
#else
		public void TestHandleSocketException()
		{
			using (WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://invalidwtg.wtg"))
			{
				var exception = AssertExceptionThrown<SystemToSystemTrustCertificateManagementException>(() => new SystemToSystemTrustApiHelper().SystemToSystemTrustApiGet("invalidurl"));

				AssertContains("No such host is known.", exception.Message);
				AssertEquals(typeof(SocketException), exception.InnerException.GetType());

				exception = AssertExceptionThrown<SystemToSystemTrustCertificateManagementException>(() => new SystemToSystemTrustApiHelper().SystemToSystemTrustApiPost("invalidurl", new System.Net.Http.StringContent("")));

				AssertContains("No such host is known.", exception.Message);
				AssertEquals(typeof(SocketException), exception.InnerException.GetType());
			}
		}
#endif

		public void TestSystemToSystemTrustApiEndpoint()
		{
			AssertEquals("https://myaccount-portal.cargowise.com/myaccount/api/SystemTrust/", new SystemToSystemTrustApiHelper().SystemToSystemTrustApiEndpoint);

			using (WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://wtg.wtg"))
			{
				AssertEquals("https://wtg.wtg/api/SystemTrust/", new SystemToSystemTrustApiHelper().SystemToSystemTrustApiEndpoint);
			}
		}
	}
}
