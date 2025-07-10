using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using CargoWise.Authentication.Glow.Ticketing;
using CargoWise.Authentication.Primitives;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class GlowAuthenticationTicketAuthroizationAttributeTest : TestCaseWithFactory
	{
		public void TestOnAuthorization_NoAuthCookie()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/");
			var attribute = new GlowAuthenticationTicketAuthorizationAttribute();
			var response = ExecuteFilterAndReturnResponse(attribute, request);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals("Authentication token not provided.", ((HttpError)((ObjectContent<HttpError>)response.Content).Value).Single().Value);
		}

		public void TestOnAuthorization_InvalidAuthCookie()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/");
			request.Headers.Add("Cookie", "Glow-Auth=ZayRyAleeLolaIndy");
			var attribute = new GlowAuthenticationTicketAuthorizationAttribute();
			var response = ExecuteFilterAndReturnResponse(attribute, request);
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals("Authentication token was invalid.", ((HttpError)((ObjectContent<HttpError>)response.Content).Value).Single().Value);
		}

		public void TestOnAuthorization_ResponseNotSuccess()
		{
			var authenticationTicket = new AuthenticationTicket
			{
				ProviderType = "OC",
				Username = "ABCAAAADA/abc@example.org",
				ProviderKey = Guid.NewGuid(),
				AuthenticationResult = AuthenticationResult.SessionExpired,
				ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
			};
			var encryptionKey = GlowRegistry.Instance.GlowAuthenticationEncryptionKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Value);
			var hmacKey = GlowRegistry.Instance.GlowAuthenticationHmacKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationHmacKey.Value);
			var validCookieValue = authenticationTicket.ToCookieValue(encryptionKey, hmacKey);
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/");
			request.Headers.Add("Cookie", "Glow-Auth=" + WebUtility.UrlEncode(validCookieValue));
			var attribute = new GlowAuthenticationTicketAuthorizationAttribute();
			var response = ExecuteFilterAndReturnResponse(attribute, request);
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals("Authentication was not successful: " + AuthenticationResult.SessionExpired, ((HttpError)((ObjectContent<HttpError>)response.Content).Value).Single().Value);
		}

		public void TestOnAuthorization_Success()
		{
			var contact = Factory.NewWithValidTestData<MasterFiles.Business.OrgContact>();
			Factory.Save();
			var authenticationTicket = new AuthenticationTicket
			{
				ProviderType = "OC", Username = "ABCAAAADA/abc@example.org",
				ProviderKey = contact.PK.ToGuid(),
				AuthenticationResult = AuthenticationResult.Success,
				ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
			};
			var encryptionKey = GlowRegistry.Instance.GlowAuthenticationEncryptionKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationEncryptionKey.Value);
			var hmacKey = GlowRegistry.Instance.GlowAuthenticationHmacKey.DataType.Serialise(GlowRegistry.Instance.GlowAuthenticationHmacKey.Value);
			var validCookieValue = authenticationTicket.ToCookieValue(encryptionKey, hmacKey);
			var request = new HttpRequestMessage(HttpMethod.Get, "http://unit-testing/");
			request.Headers.Add("Cookie", "Glow-Auth=" + WebUtility.UrlEncode(validCookieValue));
			var attribute = new GlowAuthenticationTicketAuthorizationAttribute();
			var controllerContext = new HttpControllerContext { Request = request };
			var actionContext = new HttpActionContext { ControllerContext = controllerContext };
			var task = ((IAuthorizationFilter)attribute).ExecuteAuthorizationFilterAsync(actionContext, CancellationToken.None, () => Task.FromResult<HttpResponseMessage>(null));
			var response = task.Result;
			AssertNull(response);
			var identity = actionContext.RequestContext?.Principal?.Identity as GlowAuthenticationTicketIdentity;
			AssertNotNull(identity);
			AssertEquals("OC", identity.ProviderType);
			AssertEquals("Ticket", identity.AuthenticationType);
			AssertEquals(contact.PK.ToGuid(), identity.ProviderKey);
			AssertEquals("ABCAAAADA/abc@example.org", identity.Name);
			AssertEquals(true, identity.IsAuthenticated);
		}

		#region Implementation
		HttpResponseMessage ExecuteFilterAndReturnResponse(IAuthorizationFilter filter, HttpRequestMessage request)
		{
			var controllerContext = new HttpControllerContext { Request = request };
			var actionContext = new HttpActionContext { ControllerContext = controllerContext };
			var task = filter.ExecuteAuthorizationFilterAsync(actionContext, CancellationToken.None, () => Task.FromResult<HttpResponseMessage>(null));
			var response = task.Result;
			return response;
		}
		#endregion
	}
}
