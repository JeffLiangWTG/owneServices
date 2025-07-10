using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using CargoWise.Authentication.Glow.Ticketing;
using CargoWise.Authentication.Primitives;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	sealed class GlowAuthenticationTicketAuthorizationAttribute : AuthorizationFilterAttribute
	{
		public override void OnAuthorization(HttpActionContext actionContext)
		{
			var cookieState = actionContext.Request.Headers.GetCookies()?.SelectMany(header => header.Cookies).FirstOrDefault(cookie => string.Equals(cookie.Name, "Glow-Auth", StringComparison.Ordinal));
			if (cookieState?.Value == null)
			{
				actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Authentication token not provided.");
			}
			else
			{
				var encryptionKey = GetBinaryRegistryItemValue(GlowRegistry.Instance.GlowAuthenticationEncryptionKey);
				var hmacKey = GetBinaryRegistryItemValue(GlowRegistry.Instance.GlowAuthenticationHmacKey);
				var ticket = AuthenticationTicket.FromCookieValue(cookieState.Value, encryptionKey, hmacKey);
				if (ticket == null)
				{
					actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Authentication token was invalid.");
				}
				else
				{
					if (ticket.AuthenticationResult != AuthenticationResult.Success)
					{
						actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Authentication was not successful: " + ticket.AuthenticationResult);
					}
					else
					{
						var identity = new GlowAuthenticationTicketIdentity(ticket.Username, ticket.ProviderKey, ticket.ProviderType);
						var principal = new GenericPrincipal(identity, null);
						actionContext.RequestContext.Principal = principal;
						base.OnAuthorization(actionContext);
					}
				}
			}
		}

		static byte[] GetBinaryRegistryItemValue(StringRegistryItem binaryRegistryItem)
		{
			return binaryRegistryItem.DataType.Serialise(binaryRegistryItem.Value);
		}
	}

	public sealed class GlowAuthenticationTicketIdentity : IIdentity
	{
		public GlowAuthenticationTicketIdentity(string username, Guid providerKey, string providerType)
		{
			Name = username;
			ProviderKey = providerKey;
			ProviderType = providerType;
		}

		public Guid ProviderKey { get; }

		public string Name { get; }

		public string ProviderType { get; }

		public string AuthenticationType => "Ticket";

		public bool IsAuthenticated => true;
	}
}
