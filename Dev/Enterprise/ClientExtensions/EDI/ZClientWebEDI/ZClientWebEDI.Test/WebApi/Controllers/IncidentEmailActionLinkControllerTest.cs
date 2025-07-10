using System.Net;
using System.Net.Http;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.BatchProcessor;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class IncidentEmailActionLinkControllerTest : TestCaseWithFactory
	{
		public void TestInvalidToken()
		{
			var invalidToken = "EA92034";
			using (var request = GetNewResolveIncidentRequest(invalidToken))
			{
				using (var response = Execute(request))
				{
					AssertEquals(HttpStatusCode.Redirect, response.StatusCode);
					AssertEquals(Global.IncidentActionLinkInvalidLinkPage, response.Headers.Location);
				}
			}
		}

		public void TestConsumeToken()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.Request.INC_IsCustomerResolved = false;
			incident.Request.INC_Status = "CSV";
			Factory.Save();

			var accessToken = GetNewToken(incident);
			var token = accessToken.SAT_Token;
			Factory.Save();

			AssertTokenConsumed(token, false);
			AssertCustomerResolved(incident, false);

			using (var request = GetNewResolveIncidentRequest(token))
			{
				using (var response = Execute(request))
				{
					AssertEquals(HttpStatusCode.Redirect, response.StatusCode);
					AssertEquals(Global.IncidentActionLinkConfirmResolvedPage, response.Headers.Location);
				}
			}

			AssertTokenConsumed(token, true);
			AssertCustomerResolved(incident, true);
		}

		public void TestWebsiteRequest_ShouldNotConsumeToken()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.Request.INC_IsCustomerResolved = false;
			incident.Request.INC_Status = "CSV";
			Factory.Save();

			var accessToken = GetNewToken(incident);
			var token = accessToken.SAT_Token;
			Factory.Save();

			AssertTokenConsumed(token, false);
			AssertCustomerResolved(incident, false);

			using (var request = GetNewWebsiteRequest(token))
			{
				using (var response = Execute(request))
				{
					AssertEquals(HttpStatusCode.Redirect, response.StatusCode);
					AssertEquals(Global.IncidentActionLinkConfirmResolvedPage + $"?token={token}", response.Headers.Location);
				}
			}

			AssertTokenConsumed(token, false);
			AssertCustomerResolved(incident, false);
		}

		public void TestRedirectAfterTokenConsumed()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.Request.INC_IsCustomerResolved = false;
			incident.Request.INC_Status = "CSV";
			Factory.Save();

			var accessToken = GetNewToken(incident);
			var token = accessToken.SAT_Token;
			Factory.Save();

			AssertTokenConsumed(token, false);
			AssertCustomerResolved(incident, false);

			using (var request = GetNewResolveIncidentRequest(token))
			{
				using (var response = Execute(request))
				{
					AssertEquals(HttpStatusCode.Redirect, response.StatusCode);
					AssertEquals(Global.IncidentActionLinkConfirmResolvedPage, response.Headers.Location);
				}
			}

			AssertTokenConsumed(token, true);
			AssertCustomerResolved(incident, true);

			using (var request = GetNewWebsiteRequest(token))
			{
				using (var response = Execute(request))
				{
					AssertEquals(HttpStatusCode.Redirect, response.StatusCode);
					AssertEquals(Global.IncidentActionLinkInvalidLinkPage, response.Headers.Location);
				}
			}
		}

		StmAccessToken GetNewToken(SupportIncident incident)
		{
			var accessToken = Factory.New<StmAccessToken>();
			accessToken.SAT_Type = AccessTokenTypes.IncidentEmailActionLink;
			accessToken.SAT_Token = "AD8327UEWQ";
			accessToken.SAT_ParentId = incident.PK;
			accessToken.SAT_ParentTableCode = IncidentMainSchema.Constants.Prefix;
			accessToken.SAT_ExpiresAt = ZDateTime.UtcNow.AddMinutes(5);
			accessToken.SAT_RemainingUseCount = 1;
			accessToken.SAT_Scope = SupportIncidentEmailBodyGeneralControls.ConfirmResolvedButtonID;
			return accessToken;
		}

		void AssertTokenConsumed(string token, bool expectedConsumed)
		{
			var isTokenConsumed = !Factory.ExistsInDatabase(StmAccessTokenSchema.Constants.TableName, new ZQuery(StmAccessTokenSchema.SAT_Token, token));
			AssertEquals(expectedConsumed, isTokenConsumed);
		}

		void AssertCustomerResolved(SupportIncident incident, bool expectedCustomerResolved)
		{
			var inDatabase = Factory.ExistsInDatabase(IncidentRequestSchema.Constants.TableName, new ZQuery(IncidentRequestSchema.PK, incident.IM_INC_Request)
				.AddToFilter(IncidentRequestSchema.INC_IsCustomerResolved, expectedCustomerResolved)
				.AddToFilter(IncidentRequestSchema.INC_Status, expectedCustomerResolved ? "CLS" : "CSV"));
			AssertEquals(true, inDatabase);

			var logQuery = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, SupportRequestProcessor.DeemedResolvedLogReference);
			logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.MiscellaneousEventCode);
			logQuery.AddToFilter(StmALogSchema.SL_Parent, incident.IM_INC_Request);

			AssertEquals(expectedCustomerResolved, Factory.ExistsInDatabase(StmALogSchema.Constants.TableName, logQuery));
		}

		HttpRequestMessage GetNewResolveIncidentRequest(string token) => new HttpRequestMessage(HttpMethod.Get, $"http://unit-testing/api/incident/resolve?token={token}");

		HttpRequestMessage GetNewWebsiteRequest(string token) => new HttpRequestMessage(HttpMethod.Get, $"http://unit-testing/api/incident/confirm-resolve?token={token}");

		HttpResponseMessage Execute(HttpRequestMessage request)
		{
			HttpResponseMessage response;
			using (var controller = new IncidentEmailActionLinkController())
			{
				response = ControllerTestHelper.Execute(controller, request);
			}

			return response;
		}
	}
}
