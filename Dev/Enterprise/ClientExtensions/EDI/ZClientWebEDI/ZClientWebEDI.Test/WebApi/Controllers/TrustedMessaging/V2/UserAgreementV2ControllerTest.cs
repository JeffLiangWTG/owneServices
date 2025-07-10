using System;
using System.Net;
using System.Net.Http;
using CargoWise.Types;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class UserAgreementV2ControllerTest : TrustedMessagingV2ControllerBaseTest
	{
		public void TestGetRequiredUserAgreement()
		{
			var uri = new Uri("https://unit-testing/api/sso/v2/user-agreement/agreement");
			var reqObj = new UserAgreementInfo()
			{
				Product = "CW1",
				SystemId = "8000",
				TenantId = "DDDABCSYD",
				UserId = "U048173",
				FullName = "User One",
				UserCountry = "AU",
				UserAgreementType = "MYA",
				InfoTimestamp = ZDateTime.UtcNow.ToDateTime()
			};
			AssertWebApi(uri, reqObj, HttpStatusCode.OK, "{\"required\":false,\"allow_online_clickthrough\":false,\"title\":\"\",\"content\":\"\",\"version_number\":0,\"minor_version_number\":0,\"variant\":\"\",\"level\":\"\"}");
		}

		public void TestAcknowledgeUserAgreement()
		{
			var uri = new Uri("https://unit-testing/api/sso/v2/user-agreement/acknowledge");
			var reqObj = new UserAgreementInfo()
			{
				Product = "CW1",
				SystemId = "8000",
				TenantId = "DDDABCSYD",
				UserId = "U048173",
				FullName = "User One",
				UserCountry = "AU",
				UserAgreementType = "MYA",
				InfoTimestamp = ZDateTime.UtcNow.ToDateTime()
			};
			AssertWebApi(uri, reqObj, HttpStatusCode.BadRequest, "{\"errors\":[{\"code\":\"2003\",\"message\":\"There are no current User Agreements for the given UserAgreementType and UserCountry combination.\"}]}");
		}

		public void TestGetAcceptances()
		{
			var uri = new Uri("https://unit-testing/api/sso/v2/user-agreement/acceptances");
			var reqObj = new GetAcceptancesInfo()
			{
				Product = "CW1",
				SystemId = "8000",
				TenantId = "DDDABCSYD",
				UserAgreementType = "MYA",
				InfoTimestamp = ZDateTime.UtcNow.ToDateTime()
			};
			AssertWebApi(uri, HttpMethod.Get, reqObj, HttpStatusCode.OK, "{\"Acceptances\":[]}");
		}

		public void TestGetRequiredEnterpriseUserAgreementUrl()
		{
			var uri = new Uri("https://unit-testing/api/sso/v2/user-agreement/enterprise-agreement");
			var reqObj = new EnterpriseAgreementInfo()
			{
				Product = "CW1",
				SystemId = "8000",
				TenantId = "DDDABCSYD",
				UserAgreementType = "MYA",
				InfoTimestamp = ZDateTime.UtcNow.ToDateTime()
			};
			AssertWebApi(uri, reqObj, HttpStatusCode.OK, "{\"required\":false,\"url\":\"\"}");
		}

		protected override TrustedController GetTrustedController(NLogWrapper logger)
		{
			return new UserAgreementV2Controller(logger);
		}
	}
}
