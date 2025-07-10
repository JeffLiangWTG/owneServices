using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.WiseTechAcademy;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class WiseTechAcademyControllerTestCase : TestCaseWithFactory
	{
		public void TestInvalidToken()
		{
			var invalidToken = "EA92034";
			using (var request = GetNewRequest())
			{
				request.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", invalidToken), new KeyValuePair<string, string>("contactPk", Guid.NewGuid().ToString()), });
				using (var response = Execute(request))
				{
					AssertEquals(HttpStatusCode.NotFound, response.StatusCode);
				}
			}
		}

		public void TestInvalidContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.FillWithValidTestData();
			var accessToken = GetNewToken(contact);
			Factory.Save();
			using (var request = GetNewRequest())
			{
				request.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", accessToken.SAT_Token), new KeyValuePair<string, string>("contactPk", Guid.NewGuid().ToString()), });
				using (var response = Execute(request))
				{
					AssertEquals(HttpStatusCode.NotFound, response.StatusCode);
				}
			}
		}

		public void TestExpiredToken()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.FillWithValidTestData();
			var accessToken = GetNewToken(contact);
			accessToken.SAT_ExpiresAt = ZDateTime.UtcNow.AddDays(-1);
			Factory.Save();
			using (var request = GetNewRequest())
			{
				request.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", accessToken.SAT_Token), new KeyValuePair<string, string>("contactPk", contact.PK.ToString()), });
				using (var response = Execute(request))
				{
					AssertEquals(HttpStatusCode.NotFound, response.StatusCode);
				}
			}
		}

		public void TestConsumeToken()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.FillWithValidTestData();
			var accessToken = GetNewToken(contact);
			var token = accessToken.SAT_Token;
			Factory.Save();
			using (var request = GetNewRequest())
			{
				request.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token), new KeyValuePair<string, string>("contactPk", contact.PK.ToString()), });
				using (var response = Execute(request))
				{
					AssertEquals(HttpStatusCode.OK, response.StatusCode);
				}
			}

			var isTokenConsumed = !Factory.ExistsInDatabase(StmAccessTokenSchema.Constants.TableName, new ZQuery(StmAccessTokenSchema.SAT_Token, token));
			Assert(isTokenConsumed);
		}

		public void TestEnableJWTAuth()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.FillWithValidTestData();
			var accessToken = GetNewToken(contact);
			var token = accessToken.SAT_Token;
			Factory.Save();
			using (EDIDataRegistry.Instance.EnableAcademyJWTAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var request = GetNewRequest())
			{
				request.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token), new KeyValuePair<string, string>("contactPk", contact.PK.ToString()), });
				using (var response = Execute(request))
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				}
			}

			var isTokenConsumed = !Factory.ExistsInDatabase(StmAccessTokenSchema.Constants.TableName, new ZQuery(StmAccessTokenSchema.SAT_Token, token));
			Assert(!isTokenConsumed);
		}

		StmAccessToken GetNewToken(OrgContact contact)
		{
			var accessToken = Factory.New<StmAccessToken>();
			accessToken.SAT_Type = WiseTechAcademyAccessTokenData.TokenType;
			accessToken.SAT_Token = "AD8327UEWQ";
			accessToken.SAT_ParentId = contact.PK;
			accessToken.SAT_ParentTableCode = OrgContactSchema.Constants.Prefix;
			accessToken.SAT_ExpiresAt = ZDateTime.UtcNow.AddMinutes(5);
			accessToken.SAT_RemainingUseCount = 1;
			return accessToken;
		}

		HttpRequestMessage GetNewRequest() => new HttpRequestMessage(HttpMethod.Post, "http://unit-testing/api/WiseTechAcademy/token");
		HttpResponseMessage Execute(HttpRequestMessage request)
		{
			HttpResponseMessage response;
			using (var controller = new WiseTechAcademyController())
			{
				response = ControllerTestHelper.Execute(controller, request);
			}

			return response;
		}
	}
}
