using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.GUI.Login;
using WTG.Foundation.Cryptography;

namespace Enterprise.ZClientWebCargoWiseEDI.Gateway
{
	[RoutePrefix("gateway")]
	public class MyAccountGatewayLoginController : ControllerWithEnvironment
	{
		public MyAccountGatewayLoginController()
		{
		}

		#region /authorize_via_login_router

		[HttpGet]
		[Route("authorize_via_login_router")]
		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Explicitly parsed and handled.")]
		public IHttpActionResult Authorize(
			[FromUri(Name = "contact_pk")] string contactPK,
			[FromUri(Name = "nonce")] string nonce,
			[FromUri(Name = "hash")] string hash,
			[FromUri(Name = "redirect_uri")] string redirectUriString,
			[FromUri(Name = "state")] string state
			)
		{
			if (!IsValidClientIp())
			{
				return BadRequest("Request from invalid address.");
			}

			if (!Uri.TryCreate(redirectUriString, UriKind.Absolute, out _))
			{
				return BadRequest("Invalid redirect_uri value");
			}

			if (string.IsNullOrEmpty(contactPK) || string.IsNullOrEmpty(nonce) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(state))
			{
				return BadRequest("Invalid parameters");
			}

			byte[] computedHash;
			using (var hmac = new HMACSHA256(GetBinaryRegistryItemValue(EDIDataRegistry.Instance.MyAccountGatewaySecret)))
			{
				computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(contactPK + nonce));
			}

			var hashByteArray = Convert.FromBase64String(hash);
			if (!SlowCompare.AreEqual(new ArraySegment<byte>(computedHash), new ArraySegment<byte>(hashByteArray)))
			{
				return BadRequest("Invalid hash");
			}

			if (!Guid.TryParse(contactPK, out var contactPKGuid))
			{
				return BadRequest("Invalid contact");
			}

			using (Db.DisposableActionForDbConnection())
			{
				var factory = new BusinessObjectFactory();
				var contact = factory.Load<OrgContact>(contactPKGuid);

				if (contact == null)
				{
					return BadRequest("Invalid contact");
				}

				return RedirectToGatewayViaLoginRouter(redirectUriString, contact, state);
			}

			byte[] GetBinaryRegistryItemValue(StringRegistryItem binaryRegistryItem)
			{
				return binaryRegistryItem.DataType.Serialise(binaryRegistryItem.Value);
			}
		}

		#endregion

		#region Helpers

		IHttpActionResult RedirectToGatewayViaLoginRouter(string redirectUri, OrgContact contact, string state)
		{
			var parameters = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("redirect_uri", redirectUri),
				new KeyValuePair<string, string>("state", state)
			};

			var baseUrl = EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/');
			var gatewayPath = baseUrl + "/Login/GatewayLogin.aspx";
			var grantFromCredentialsBaseUrlBuilder = new UriBuilder(gatewayPath) { Port = -1 };

			var grantFromCredentialsUrl = AppendKeyValuePairsToUri(grantFromCredentialsBaseUrlBuilder.Uri, parameters);
			var router = new MyAccountLoginRouter(grantFromCredentialsUrl, contact);
			var redirectUrl = router.GetRoutingUrl();

			if (!redirectUrl.IsAbsoluteUri)
			{
				var host = Request.RequestUri.Host;
				var uriDeconstructor = new UriDeconstructor(redirectUrl);
				var path = VirtualPathUtility.ToAbsolute(uriDeconstructor.BaseUrl);
				var builder = new UriBuilder("https", host, -1, path, "?" + uriDeconstructor.Query);
				redirectUrl = builder.Uri;
			}

			return Redirect(redirectUrl);
		}

		public static Uri AppendKeyValuePairsToUri(Uri uri, IEnumerable<KeyValuePair<string, string>> values)
		{
			var builder = new UriBuilder(uri);
			var parameters = uri.ParseQueryString();

			foreach (var kvp in values)
			{
				parameters.Add(kvp.Key, kvp.Value);
			}

			var keyValuePairs = new List<string>();
			foreach (var key in parameters.AllKeys)
			{
				foreach (var value in parameters.GetValues(key))
				{
					keyValuePairs.Add(string.Format(CultureInfo.InvariantCulture, "{0}={1}", Uri.EscapeDataString(key), Uri.EscapeDataString(value ?? string.Empty)));
				}
			}

			var newQueryString = string.Join("&", keyValuePairs);
			builder.Query = newQueryString;
			var newUri = builder.Uri;

			return newUri;
		}

		protected virtual bool IsValidClientIp()
		{
			return requestIPValidationHelper.IsCurrentRequestFromValidIP().Item1;
		}

		readonly RequestIPValidationHelper requestIPValidationHelper = new RequestIPValidationHelper();

		#endregion
	}
}
