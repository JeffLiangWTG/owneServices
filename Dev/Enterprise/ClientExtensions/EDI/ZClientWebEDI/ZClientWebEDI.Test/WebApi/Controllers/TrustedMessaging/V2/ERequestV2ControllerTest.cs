using System;
using System.Net;
using CargoWise.Types;
using Enterprise.Client.EDI;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class ERequestV2ControllerTest : TrustedMessagingV2ControllerBaseTest
	{
		public void TestGetAutoLoginUrl()
		{
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://unit-testing/GlowPortal/");
			EDIDataRegistry.Instance.GlowNewERequestPageUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "eRequestPortal#/workflow");

			var uri = new Uri("https://unit-testing/api/sso/v2/erequest/auto-login-url");
			var reqObj = new ERequestInfo()
			{
				LandingPageId = UserPortal.UserPortalLauncher.eRequestNewLandingPageId,
				Product = "CW1",
				SystemId = "8000",
				TenantId = "DDDABCSYD",
				UserId = "U048173",
				FullName = "User One",
				Email = "user.one@test.com",
				Module = "RAT",
				SubModule = "AIRFreightRate",
				Criticality = "CR4",
				ReferenceId = "6E6B6A65-609E-41BE-97F6-19C743D790FC",
				IncidentNumber = "",
				LicenceCode = "",
				InfoTimestamp = ZDateTime.UtcNow.ToDateTime()
			};

			var (statusCode, content) = CallWebApi(uri, reqObj);
			AssertEquals(HttpStatusCode.OK, statusCode);
			Assert(content.StartsWith("{\"url\":\"https://myaccount-portal.cargowise.com/myaccount/Login/GlowPortalAutoLogin.aspx?qdata="));
		}

		public void TestUpload()
		{
			var uri = new Uri("https://unit-testing/api/sso/v2/erequest/upload");
			var reqObj = new ERequestInfo()
			{
				Product = "CW1",
				SystemId = "8000",
				ERequestDocument = ERequestBaseControllerTest.CreateERequestDocumentAsXmString(ZGuid.NewZGuid(), "20200218_000000"),
				InfoTimestamp = ZDateTime.UtcNow.ToDateTime()
			};

			var (statusCode, content) = CallWebApi(uri, reqObj);
			AssertEquals(HttpStatusCode.OK, statusCode);
			AssertEquals("", content);
		}

		protected override TrustedController GetTrustedController(NLogWrapper logger)
		{
			return new ERequestV2Controller(logger);
		}
	}
}
