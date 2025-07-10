using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions.Authentication;
using CargoWise.Types;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.PortalAuth;

namespace Enterprise.ZClientWebCargoWiseEDI.PortalAuth
{
	[RoutePrefix("api/PortalAuth")]
	public class PortalAuthController : ControllerWithEnvironment
	{
		#region Preamble

		public PortalAuthController()
			: this(new Lazy<ITokenizedAccessControl>(() => new TokenizedAccessControl()))
		{
		}

		public PortalAuthController(Lazy<ITokenizedAccessControl> lazyAccessControl)
		{
			Argument.NotNull(lazyAccessControl, nameof(lazyAccessControl));

			this.lazyAccessControl = lazyAccessControl;
		}

		readonly Lazy<ITokenizedAccessControl> lazyAccessControl;

		ITokenizedAccessControl AccessControl => lazyAccessControl.Value;

		#endregion

		#region AutoLoginUrl

		[HttpGet]
		[Route("AutoLoginUrl")]
		public HttpResponseMessage GetAutoLoginUrl(string qdata)
		{
			SecureQueryString queryString;
			try
			{
				queryString = new SecureQueryString(qdata);
			}
			catch (QueryStringException)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Query string not valid.");
			}

			using (Db.DisposableActionForDbConnection())
			{
				var helper = new StaffContactHelper(qdata);

				if (helper.Database == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Empty);
				}

				var password = queryString[StaffContactValueObjectHelper.QueryStringKeys.Password];
				if (!helper.Database.ValidateClientSecret(password))
				{
					return Request.CreateErrorResponse(HttpStatusCode.Forbidden, string.Empty);
				}

				var urlHelper = new GlowPortalUrlHelper(AccessControl);

				string landingPageId = queryString[StaffContactValueObjectHelper.QueryStringKeys.LandingPageId];
				if (!helper.Database.LD_AllowAutoLogin)
				{
					if (landingPageId == UserPortal.UserPortalLauncher.eRequestNewLandingPageId
						|| landingPageId == UserPortal.UserPortalLauncher.eRequestPortalLandingPageId
						|| landingPageId == UserPortal.UserPortalLauncher.eRequestEditLandingPageId)
					{
						var url = urlHelper.GetRequestPortalUrl();
						return Request.CreateResponse(url);
					}
					else
					{
						return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Landing page not valid.");
					}
				}

				var contactImportResult = helper.FindOrCreateContact();
				var (user, contact) = (contactImportResult.UserAccount, contactImportResult.Contact);

				if (user != null && user.EUA_IsEmailVerificationRequired)
				{
					var router = new MyAccountLoginRouter(null, user);
					var redirectUrl = router.GetRoutingUrl();
					return Request.CreateResponse(redirectUrl);
				}
				else if (contact == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User not valid.");
				}

				HttpResponseMessage httpResponse;

				var contactPK = contact.PK.ToGuid();
				var userAccountPK = user?.PK.ToGuid() ?? Guid.Empty;

				if (landingPageId == UserPortal.UserPortalLauncher.eRequestNewLandingPageId)
				{
					var url = urlHelper.GetNewRequestUrl(contactPK, userAccountPK, helper.LicenceCode,
						queryString[GlowPortalUrlHelper.ServerOnlyQueryStringKeys.Product],
						queryString[StaffContactValueObjectHelper.QueryStringKeys.Module],
						queryString[StaffContactValueObjectHelper.QueryStringKeys.SubModule],
						queryString[GlowPortalUrlHelper.ServerOnlyQueryStringKeys.Criticality],
						queryString[StaffContactValueObjectHelper.QueryStringKeys.ReferenceId]);
					httpResponse = Request.CreateResponse(url);
				}
				else if (landingPageId == UserPortal.UserPortalLauncher.eRequestPortalLandingPageId)
				{
					var url = urlHelper.GetRequestPortalUrl(contactPK, userAccountPK);
					httpResponse = Request.CreateResponse(url);
				}
				else if (landingPageId == UserPortal.UserPortalLauncher.eRequestEditLandingPageId)
				{
					ZGuid requestPK = GlowPortalUrlHelper.IncidentRequestPkFromNumber(queryString[StaffContactValueObjectHelper.QueryStringKeys.IncidentNumber]);
					if (requestPK.IsEmpty)
					{
						httpResponse = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Incident not valid.");
					}
					else
					{
						var url = urlHelper.GetEditRequestUrl(contactPK, userAccountPK, requestPK.ToGuid());
						httpResponse = Request.CreateResponse(url);
					}
				}
				else
				{
					httpResponse = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Landing page not valid.");
				}

				return httpResponse;
			}
		}

		[HttpGet]
		[Route("AutoLoginContactUrl")]
		public HttpResponseMessage GetAutoLoginContactUrl(string landingPageId, Guid contactPK, string product, string module, string subModule, string criticality, string referenceId)
		{
			var urlHelper = new GlowPortalUrlHelper(AccessControl);
			using (Db.DisposableActionForDbConnection())
			{
				if (landingPageId == UserPortal.UserPortalLauncher.eRequestNewLandingPageId)
				{
					var url = urlHelper.GetNewRequestUrl(contactPK, Guid.Empty, null, product, module, subModule, criticality, referenceId);
					return Request.CreateResponse(url);
				}
				else if (landingPageId == UserPortal.UserPortalLauncher.eRequestPortalLandingPageId)
				{
					var url = urlHelper.GetRequestPortalUrl(contactPK, Guid.Empty);
					return Request.CreateResponse(url);
				}
				else
				{
					return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Landing page not valid.");
				}
			}
		}

		[HttpGet]
		[Route("AutoLoginContactUrl")]
		public HttpResponseMessage GetAutoLoginContactUrl(string landingPageId, Guid contactPK)
		{
			var urlHelper = new GlowPortalUrlHelper(AccessControl);
			using (Db.DisposableActionForDbConnection())
			{
				if (landingPageId == UserPortal.UserPortalLauncher.AccreditationAttemptPortalLadingPageId)
				{
					var url = urlHelper.GetAccreditationPortalUrl(contactPK, Guid.Empty);
					return Request.CreateResponse(url);
				}
				else
				{
					return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Landing page not valid.");
				}
			}
		}

		[HttpPost]
		[Route("AutoLoginToken")]
		public HttpResponseMessage GetAutoLoginToken([FromBody] AutoLoginContactData parameters)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var helper = new StaffContactHelper(parameters.licence, parameters.dbnumber, parameters.contact);
				if (helper.Database == null || !helper.Database.ValidateClientSecret(parameters.password) || !helper.Database.LD_AllowAutoLogin)
				{
					return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Auto login is not allowed for this user");
				}

				var contactImportResult = helper.FindOrCreateContact();
				var contact = contactImportResult.Contact;
				if (contact == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.BadRequest, contactImportResult.ErrorMessage);
				}

				var baseUrl = parameters.baseurl;
				var urlHelper = new GlowPortalUrlHelper(AccessControl);
				if (urlHelper.IsOwnGlowPortalUrl(baseUrl))
				{
					var contactPK = contact.PK.ToGuid();
					var user = contactImportResult.UserAccount;
					var userAccountPK = user?.PK.ToGuid() ?? Guid.Empty;

					return Request.CreateResponse(urlHelper.GetGenericPortalWithAutoLogin(baseUrl, contactPK, userAccountPK));
				}
				else
				{
					var scope = new AutoLoginTokenScope
					{
						OrgCode = contact.Header.OH_Code,
						DatabaseNumber = helper.Database?.LD_DatabaseNumber.ToString() ?? string.Empty,
						ReturnUrl = baseUrl
					};
					var serializedScope = AutoLoginHelper.SerializeToXml(scope);
					var info = new AccessTokenInfo(serializedScope, contact.PK.ToGuid(), OrgContactSchema.Constants.Prefix);
					var token = AccessControl.CreateLimitedToken(AccessTokenTypes.MyAccountAutoLogin, info, maxUses: 1);

					var queryString = string.Format(CultureInfo.InvariantCulture, "?token={0}", token);
					return Request.CreateResponse(string.Format(CultureInfo.InvariantCulture, "{0}{1}", Url.Content("~/Login/AutoLogin.aspx"), queryString));
				}
			}
		}

		#endregion
	}
}
