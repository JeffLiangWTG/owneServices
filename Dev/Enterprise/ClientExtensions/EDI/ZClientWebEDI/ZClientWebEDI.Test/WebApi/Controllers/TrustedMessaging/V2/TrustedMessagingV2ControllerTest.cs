using System;
using System.Net;
using CargoWise.Types;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class TrustedMessagingV2ControllerTest : TrustedMessagingV2ControllerBaseTest
	{
		public void TestCertificate()
		{
			var uri = new Uri("https://unit-testing/api/sso/v2/trusted-messaging/certificate");
			var reqObj = new CertificateInfo()
			{
				Product = "CW1",
				SystemId = "8000",
				InfoTimestamp = ZDateTime.UtcNow.ToDateTime(),
				CertificateOwnerProduct = "CW1",
				CertificateOwnerSystemId = "XXX"
			};
			AssertWebApi(uri, reqObj, HttpStatusCode.NotFound, "");

			reqObj.CertificateOwnerSystemId = "8000";
			var (statusCode, content) = CallWebApi(uri, reqObj);
			AssertEquals(HttpStatusCode.OK, statusCode);
			Assert(content.StartsWith("{\"cert_data\":\""));
		}

		protected override TrustedController GetTrustedController(NLogWrapper logger)
		{
			return new TrustedMessagingV2Controller(logger);
		}
	}
}
