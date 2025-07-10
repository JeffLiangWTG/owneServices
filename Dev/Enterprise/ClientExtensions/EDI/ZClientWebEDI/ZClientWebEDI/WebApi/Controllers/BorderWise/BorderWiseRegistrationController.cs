using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.FuzzyComparer;
using CargoWise.Tools.Telephony;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.BorderWise;
using Newtonsoft.Json;
using NLog;
using WTG.AddressCleansing.Common;

namespace Enterprise.ZClientWebCargoWiseEDI.BorderWise
{
	[RoutePrefix("api/BorderWiseRegistration")]
	public class BorderWiseRegistrationController : BorderWiseApiBaseController
	{
		readonly List<string> excludedOrgCodes = new() { "EDUSYDACC" };

		[Route("")]
		public string Get() => "BorderWise Customer Registration Controller";

		#region One Time Link

		[Route("v4/RegistrationOneTimeLink")]
		[HttpPost]
		public IHttpActionResult GetRegistrationOneTimeLinkV4(RegistrationRequest registrationRequest)
		{
			if (registrationRequest == null)
			{
				return BadRequest("RegistrationRequest cannot be null.");
			}

			if (string.IsNullOrWhiteSpace(registrationRequest.EmailAddress)
				|| (registrationRequest.IsFreeTrial && (string.IsNullOrWhiteSpace(registrationRequest.FirstName) || string.IsNullOrWhiteSpace(registrationRequest.LastName))))
			{
				return BadRequest("All arguments must be provided and be valid.");
			}

			BorderWiseUtilities.RandomDelay();

			using (Db.DisposableActionForDbConnection())
			{
				var factory = SetupEnvironmentAndGetFactory();

				// Existing contacts - simply send them their existing details
				if (CheckExistingContactsAndNotify(factory, registrationRequest.EmailAddress, registrationRequest.IsActiveUserAllowedToRegisterToNewCompany))
				{
					return Ok(CommonConstants.userRegistrationOneTimeLinkResponse);
				}

				return SendRegistrationToken(registrationRequest, factory);
			}
		}

		[Route("GroupRegistrationInvitation")]
		[HttpPost]
		public IHttpActionResult GroupRegistrationInvitation(GroupRegistrationInvitationRequest groupRegistrationInvitationRequest)
		{
			if (groupRegistrationInvitationRequest.OrgPk == Guid.Empty || !groupRegistrationInvitationRequest.EmailAddresses.Any() || string.IsNullOrWhiteSpace(groupRegistrationInvitationRequest.AdminEmail) || string.IsNullOrWhiteSpace(groupRegistrationInvitationRequest.AdminName))
			{
				return BadRequest("All arguments must be provided.");
			}

			BorderWiseUtilities.RandomDelay();

			using (Db.DisposableActionForDbConnection())
			{
				var factory = SetupEnvironmentAndGetFactory();

				var orgHeader = factory.Load<OrgHeader>(groupRegistrationInvitationRequest.OrgPk);

				if (orgHeader == null)
				{
					return NotFound();
				}

				if (!orgHeader.OH_IsActive)
				{
					return BadRequest("Organization is not active.");
				}

				foreach (var emailAddress in groupRegistrationInvitationRequest.EmailAddresses)
				{
					var existingContact = GetContacts(factory, emailAddress, groupRegistrationInvitationRequest.OrgPk).FirstOrDefault();
					if (existingContact == null)
					{
						SendGroupRegistrationToken(emailAddress, orgHeader, groupRegistrationInvitationRequest.AdminName, groupRegistrationInvitationRequest.AdminEmail, factory);
						continue;
					}

					if (!existingContact.OC_WebAccessEnabled)
					{
						existingContact.OC_WebAccessEnabled = true;
					}
					factory.Save();

					var body = $@"Dear {existingContact.OC_ContactName},<br /><br />
You have been invited by {groupRegistrationInvitationRequest.AdminName} (<a href='mailto:{groupRegistrationInvitationRequest.AdminEmail}'>{groupRegistrationInvitationRequest.AdminEmail}</a>) to use <a href='https://app.borderwise.com/'>BorderWise</a>.<br /><br />
Your access details to BorderWise are:<br />
Company Code: <b>{existingContact.OrganisationCode}</b><br />
User Name: <b>{emailAddress}</b><br /><br />
Forgotten your password? Use the <a href='https://app.borderwise.com/account/forgot-password/'>Forgot Password</a> link to reset your password.";

					BorderWiseUtilities.SendEmail(BorderWiseEmailFromAddress, emailAddress, "You are invited to BorderWise", body);
				}

				return Ok("Registration invitation(s) are processed successfully.");
			}
		}

		#endregion

		#region Confirm Access

		[Route("ConfirmAccess/{token}")]
		[HttpPut]
		public IHttpActionResult ConfirmAccess(Guid token)
		{
			if (token == Guid.Empty)
			{
				return BadRequest("Invalid argument.");
			}

			OrgContact contact = null;

			try
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factory = SetupEnvironmentAndGetFactory();

					var tokenValidationResult = ValidateConfirmAccessToken(token, factory, out var contactPk);
					if (tokenValidationResult != null)
					{
						DeleteToken(ConfirmAccessTokenName, token);
						return tokenValidationResult;
					}

					contact = factory.Load<OrgContact>(contactPk);
					var contactValidationResult = ValidateConfirmAccessContact(contact);
					if (contactValidationResult != null)
					{
						DeleteToken(ConfirmAccessTokenName, token);
						return contactValidationResult;
					}

					contact.OC_WebAccessEnabled = true;
					BorderWiseUtilities.ChangeSecurityRight(contact, true, factory);

					SendBorderWiseResetPasswordEmail(contact);
				}
				return Ok("Access to BorderWise is granted.");
			}
			catch (ZSaveException ex) when (ex.Message.Contains(CommonConstants.duplicateEmailForWebEnabledContact))
			{
				var errorMessage = $"Failed to confirm access for the user ( {contact.Name} / {contact.Email} ). Each active contact with web access in this organization must have a unique email address.";
				Logger.AddLog(LogLevel.Info, errorMessage, ((int)HttpStatusCode.Forbidden), sessionId: "", routingPath: "api/BorderWiseRegistration/ConfirmAccess", ex: ex);
				return Content(HttpStatusCode.Forbidden, errorMessage);
			}
			catch (Exception)
			{
				throw;
			}
		}

		#endregion

		#region Register

		[Route("RegisterByAdmin/{oneTimeRegistrationToken}")]
		[HttpPost]
		public IHttpActionResult RegisterByAdmin(Guid oneTimeRegistrationToken, [FromBody] ContactInfo contactInfo)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = "api/BorderWiseRegistration/RegisterByAdmin";

			var infoMessage = $"Registration started. Token: {oneTimeRegistrationToken}";
			Logger.AddLog(LogLevel.Info, infoMessage, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);

			if (contactInfo.OrganisationPk == null || contactInfo.OrganisationPk == Guid.Empty)
			{
				return BadRequest("OrganisationPk cannot be null.");
			}

			var basicValidationResult = ValidateBasicContactInfo(contactInfo, sessionId, routingPath);
			if (basicValidationResult != null)
			{
				return basicValidationResult;
			}

			using (Db.DisposableActionForDbConnection())
			{
				var factory = SetupEnvironmentAndGetFactory();

				var tokenValidationResult = ValidateRegToken(oneTimeRegistrationToken, factory, out var contactEmail, out Guid existingOrgPk);
				if (tokenValidationResult != null)
				{
					var warningMessage = $"Invalid registration token detected: {tokenValidationResult}. Token will be deleted.";
					Logger.AddLog(LogLevel.Warn, warningMessage, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
					DeleteToken(RegTokenName, oneTimeRegistrationToken);
					return tokenValidationResult;
				}

				var excludedOrgPks = GetAllExcludedOrganizationPks(factory, sessionId, routingPath);
				var orgAndContactValidationResult = ValidateOrgExclusionAndInactiveContact(contactInfo, sessionId, routingPath, factory, contactInfo.OrganisationPk.Value, excludedOrgPks, contactEmail, out var existingOrg);
				if (orgAndContactValidationResult != null)
				{
					return orgAndContactValidationResult;
				}

				if (existingOrg == null)
				{
					Logger.AddLog(LogLevel.Warn, "No such organisation found.", ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
					return BadRequest($"Organisation not found with Organisation Pk: {contactInfo.OrganisationPk.Value}");
				}

				infoMessage = $"Create contact, existingOrgCode: {existingOrg.OH_Code}";
				Logger.AddLog(LogLevel.Info, infoMessage, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);

				var (org, contact) = CreateContact(contactInfo, existingOrg, contactEmail, existingOrg.MainAddress, factory);

				try
				{
					Logger.AddLog(LogLevel.Info, "Save registration changes.", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
					ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null, true);
				}
				catch (ZSaveException ex)
				{
					Logger.AddLog(LogLevel.Error, "Failed to save registration changes.", ((int)HttpStatusCode.Forbidden), sessionId, routingPath: routingPath, ex: ex);
					ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Exception occurred when save user with email: {0}", contactEmail), ex);
					return Content(HttpStatusCode.Forbidden, ex.ToString());
				}

				return SendLoginDetailsAndRegistrationSuccessResponse(contactInfo, contact, contactEmail, sessionId, routingPath, isApprovedByAdmin: true);
			}
		}

		[Route("v5/Register/{oneTimeRegistrationToken}")]
		[Route("v5/Register/{oneTimeRegistrationToken}/{acceptUserAddress}/{acceptUserPhone}")]
		[HttpPost]
		public IHttpActionResult RegisterV5(Guid oneTimeRegistrationToken, [FromBody] ContactInfo contactInfo, bool acceptUserAddress = false, bool acceptUserPhone = false)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = "api/BorderWiseRegistration/v5/Register";

			var infoMessage = $"Registration started. Token: {oneTimeRegistrationToken}";
			Logger.AddLog(LogLevel.Info, infoMessage, (int)HttpStatusCode.OK, sessionId, routingPath: routingPath);

			var basicValidationResult = ValidateBasicContactInfo(contactInfo, sessionId, routingPath);
			if (basicValidationResult != null)
			{
				return basicValidationResult;
			}

			using (Db.DisposableActionForDbConnection())
			{
				var factory = SetupEnvironmentAndGetFactory();

				OrgHeader existingOrg = null;
				var contactEmail = string.Empty;
				var tokenValidationResult = ValidateRegToken(oneTimeRegistrationToken, factory, out contactEmail, out Guid existingOrgPk);
				if (tokenValidationResult != null)
				{
					var warningMessage = $"Invalid registration token detected: {tokenValidationResult}. Token will be deleted.";
					Logger.AddLog(LogLevel.Warn, warningMessage, (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
					DeleteToken(RegTokenName, oneTimeRegistrationToken);
					return tokenValidationResult;
				}

				var excludedOrgPks = GetAllExcludedOrganizationPks(factory, sessionId, routingPath);

				if (existingOrgPk == Guid.Empty && (!contactInfo.OrganisationPk.HasValue || contactInfo.OrganisationPk.Value == Guid.Empty))
				{
					var potentialMatchingOrgResult = FindPotentialMatchOrgs(factory, contactInfo, excludedOrgPks);

					var filteredPotentialMatchingOrgResult = GetFilteredOrgMatchesResult(potentialMatchingOrgResult, factory, contactEmail, routingPath, sessionId);

					if (filteredPotentialMatchingOrgResult != null && filteredPotentialMatchingOrgResult.OrgMatches.Any())
					{
						Logger.AddLog(LogLevel.Info, "Matching organization found.", (int)HttpStatusCode.OK, sessionId, routingPath: routingPath);
						return Ok(filteredPotentialMatchingOrgResult);
					}

					Logger.AddLog(LogLevel.Info, "No matching organizations found.", (int)HttpStatusCode.OK, sessionId, routingPath: routingPath);
				}
				else
				{
					var orgAndContactValidationResult = ValidateOrgExclusionAndInactiveContact(contactInfo, sessionId, routingPath, factory, existingOrgPk, excludedOrgPks, contactEmail, out existingOrg);
					if (orgAndContactValidationResult != null)
					{
						return orgAndContactValidationResult;
					}
				}

				infoMessage = $"Create or update organization and contact, existingOrgCode: {existingOrg?.OH_Code}";
				Logger.AddLog(LogLevel.Info, infoMessage, (int)HttpStatusCode.OK, sessionId, routingPath: routingPath);

				var (org, contact) = CreateOrUpdateOrganisationAndContact(contactInfo, factory, contactEmail, existingOrg);
				if (existingOrgPk == Guid.Empty)
				{
					var validationResult = ValidateAddressAndPhone(contact, org, acceptUserAddress, acceptUserPhone, routingPath, sessionId);
					if (validationResult != null)
					{
						var warningMessage = $"Failed to validate the contact address and phone number. Message: {validationResult}";
						Logger.AddLog(LogLevel.Warn, warningMessage, (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
						return validationResult;
					}
				}

				infoMessage = $"Delete token: {RegTokenName}+{oneTimeRegistrationToken}";
				Logger.AddLog(LogLevel.Info, infoMessage, (int)HttpStatusCode.OK, sessionId, routingPath: routingPath);
				DeleteToken(RegTokenName, oneTimeRegistrationToken);

				OrgContact billingContact = null;
				if (!org.IsInDatabase)
				{
					if (!string.IsNullOrEmpty(contactInfo.BillingContactName))
					{
						Logger.AddLog(LogLevel.Info, "Create billing contact.", (int)HttpStatusCode.OK, sessionId, routingPath: routingPath);
						billingContact = CreateBillingContact(contactInfo, org, factory);
					}
					else
					{
						Logger.AddLog(LogLevel.Info, "Set contact document group.", (int)HttpStatusCode.OK, sessionId, routingPath: routingPath);
						SetContactDocumentGroups(contact);
					}

					Logger.AddLog(LogLevel.Info, "Notify new organization registration.", (int)HttpStatusCode.OK, sessionId, routingPath: routingPath);
					NotifyNewOrganisationRegistration(org, contact, factory);
				}

				try
				{
					Logger.AddLog(LogLevel.Info, "Save registration changes.", (int)HttpStatusCode.OK, sessionId, routingPath: routingPath);
					ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null, true);
				}
				catch (ZSaveException ex)
				{
					Logger.AddLog(LogLevel.Error, "Failed to save registration changes.", (int)HttpStatusCode.InternalServerError, sessionId, routingPath: routingPath, ex: ex);
					ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Exception occurred when saving user with email: {0}", contactEmail), ex);
					return Content(HttpStatusCode.InternalServerError, ex.ToString());
				}

				return SendLoginDetailsAndRegistrationSuccessResponse(contactInfo, contact, contactEmail, sessionId, routingPath, billingContact);
			}
		}

		[Route("ValidateRegistrationInfo/{oneTimeRegistrationToken}")]
		[HttpPost]
		public IHttpActionResult ValidateRegistrationInfo(Guid oneTimeRegistrationToken, [FromBody] ContactInfo contactInfo)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var factory = SetupEnvironmentAndGetFactory();

				var sessionId = Guid.NewGuid().ToString();
				var routingPath = "api/BorderWiseRegistration/ValidateRegistrationInfo";

				var infoMessage = $"ValidateRegistrationInfo started. Token: {oneTimeRegistrationToken}";
				Logger.AddLog(LogLevel.Info, infoMessage, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);

				var validationResponse = new ValidationResponse { RegisterByAdminToken = Guid.Empty };

				var tokenValidationResult = ValidateRegToken(oneTimeRegistrationToken, factory, out var contactEmail, out var existingOrgPk);
				if (tokenValidationResult != null)
				{
					var warningMessage = $"Invalid registration token detected: {tokenValidationResult}. Token will be deleted.";
					Logger.AddLog(LogLevel.Warn, warningMessage, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
					DeleteToken(RegTokenName, oneTimeRegistrationToken);
					validationResponse.HasErrors = true;
					validationResponse.Message = ExtractMessageFromActionResult(tokenValidationResult);
					return Content(HttpStatusCode.BadRequest, validationResponse);
				}

				validationResponse.RegisterEmail = contactEmail;

				var basicValidationResult = ValidateBasicContactInfo(contactInfo, sessionId, routingPath: routingPath);
				if (basicValidationResult != null)
				{
					Logger.AddLog(LogLevel.Info, "Basic ContactInfo validation failed.", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
					validationResponse.HasErrors = true;
					validationResponse.Message = ExtractMessageFromActionResult(basicValidationResult);
					return Content(HttpStatusCode.BadRequest, validationResponse);
				}

				if (contactInfo.OrganisationPk == null || contactInfo.OrganisationPk == Guid.Empty)
				{
					var warningMessage = "Organisation identifier is missing or invalid.";
					validationResponse.HasErrors = true;
					validationResponse.Message = warningMessage;
					Logger.AddLog(LogLevel.Warn, $"{warningMessage} OrganisationPk: {contactInfo.OrganisationPk}", ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
					return Content(HttpStatusCode.BadRequest, validationResponse);
				}

				if (existingOrgPk != Guid.Empty && existingOrgPk != contactInfo.OrganisationPk)
				{
					validationResponse.HasErrors = true;
					validationResponse.Message = "Account invitation linked to a different organisation.";
					Logger.AddLog(LogLevel.Warn, $"Organisation mismatch detected. ExistingOrgPk: {existingOrgPk}, OrganisationPk: {contactInfo.OrganisationPk}", ((int)HttpStatusCode.Forbidden), sessionId, routingPath: routingPath);
					return Content(HttpStatusCode.Forbidden, validationResponse);
				}

				var excludedOrgPks = GetAllExcludedOrganizationPks(factory, sessionId, routingPath);

				var orgAndContactValidationResult = ValidateOrgExclusionAndInactiveContact(contactInfo, sessionId, routingPath, factory, existingOrgPk, excludedOrgPks, contactEmail, out _);
				if (orgAndContactValidationResult != null)
				{
					Logger.AddLog(LogLevel.Warn, "Organisation exclusion and inactive contact validation failed.", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
					validationResponse.HasErrors = true;
					validationResponse.Message = ExtractMessageFromActionResult(orgAndContactValidationResult);
					return Content(HttpStatusCode.Forbidden, validationResponse);
				}

				var registerByAdminToken = CreateRegToken(contactEmail, contactInfo.OrganisationPk, 720, factory);
				var successMessage = "Registration info validation succeeded.";

				validationResponse.RegisterByAdminToken = registerByAdminToken;
				validationResponse.Message = successMessage;
				Logger.AddLog(LogLevel.Info, successMessage, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);

				return Ok(validationResponse);
			}
		}

		IHttpActionResult ValidateBasicContactInfo(ContactInfo contactInfo, string sessionId, string routingPath)
		{
			if (contactInfo == null || string.IsNullOrEmpty(contactInfo.ContactName) || string.IsNullOrEmpty(contactInfo.ContactPassword) || contactInfo.ContactDOB == null ||
							string.IsNullOrEmpty(contactInfo.OrgCode) && (string.IsNullOrEmpty(contactInfo.OrganisationAddress1) || string.IsNullOrEmpty(contactInfo.OrganisationCity) || string.IsNullOrEmpty(contactInfo.OrganisationState) ||
							string.IsNullOrEmpty(contactInfo.OrganisationPostCode) || string.IsNullOrEmpty(contactInfo.OrganisationCountryCode) || string.IsNullOrEmpty(contactInfo.OrganisationPhone)))
			{
				return BadRequest("All arguments must be provided.");
			}

			var infoMessage = $"Registration for ContactName: {contactInfo.ContactName} with organizationPk / orgCode: {contactInfo.OrganisationPk ?? Guid.Empty} / {contactInfo.OrgCode}";
			Logger.AddLog(LogLevel.Info, infoMessage, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
			Logger.AddLog(LogLevel.Info, "Checking validity of organization name.", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
			if (!string.IsNullOrEmpty(contactInfo.OrganisationName) && contactInfo.OrganisationName.Contains(CommonConstants.restrictedCompanyNameKeyWord, StringComparison.CurrentCultureIgnoreCase))
			{
				var warningMessage = $"Forbidden organization name detected: {contactInfo.OrganisationName}";
				Logger.AddLog(LogLevel.Warn, warningMessage, ((int)HttpStatusCode.Forbidden), sessionId, routingPath: routingPath);
				return Content(HttpStatusCode.Forbidden, "'WiseTech' is reserved word for company name.");
			}

			Logger.AddLog(LogLevel.Info, "Validate password.", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
			var (isValid, message) = ContactPasswordValidator.IsValidPassword(null, contactInfo.ContactPassword, new List<string>() { contactInfo.ContactName });
			if (!isValid)
			{
				Logger.AddLog(LogLevel.Warn, "Invalid password detected.", ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
				return BadRequest(message);
			}

			return null;
		}

		IHttpActionResult ValidateOrgExclusionAndInactiveContact(ContactInfo contactInfo, string sessionId, string routingPath, BusinessObjectFactory factory, Guid existingOrgPk, IEnumerable<Guid> excludedOrgPks, string contactEmail, out OrgHeader existingOrg)
		{
			existingOrg = null;
			var orgPk = existingOrgPk == Guid.Empty ? contactInfo.OrganisationPk.Value : existingOrgPk;

			var infoMessage = $"OrgPk: {orgPk}, existingOrgPk: {existingOrgPk}, OrganisationPk exists: {contactInfo.OrganisationPk.HasValue}";
			Logger.AddLog(LogLevel.Info, infoMessage, (int)HttpStatusCode.OK, sessionId, routingPath: routingPath);

			if (excludedOrgPks != null && excludedOrgPks.Contains(orgPk))
			{
				Logger.AddLog(LogLevel.Warn, "Wrong organization selected.", (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
				return Content(HttpStatusCode.Forbidden, "Wrong organization selected.");
			}

			if (orgPk != Guid.Empty)
			{
				var inactiveContactQuery = new ZDBOnlyQuery(typeof(OrgContact));
				inactiveContactQuery.AddToFilter(OrgContactSchema.OC_OH, orgPk);
				inactiveContactQuery.AddToFilter(OrgContactSchema.OC_Email, contactEmail);
				inactiveContactQuery.AddToFilter(OrgContactSchema.OC_IsActive, false);
				var isInactiveContact = factory.Exists(typeof(OrgContact), inactiveContactQuery);

				if (isInactiveContact)
				{
					var warningMessage =
						$"The account associated with the email ({contactEmail}) was deactivated in the organization ({contactInfo.OrganisationName}) and can't be used to register to BorderWise. Please contact the organization administrator or register with the correct organization/email address. You can also contact support@borderwise.com if further support is needed.";

					Logger.AddLog(LogLevel.Warn, warningMessage, (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
					return BadRequest(warningMessage);
				}
				existingOrg = factory.Load<OrgHeader>(orgPk);
			}
			return null;
		}

		[Route("GetSimilarOrganisation/{emailAddress}")]
		[HttpGet]
		public IHttpActionResult GetSimilarOrganisations(string emailAddress)
		{
			if (string.IsNullOrWhiteSpace(emailAddress))
			{
				return BadRequest("All arguments must be provided.");
			}

			var matches = GetSimilarOrgsByEmailDomain(emailAddress);

			return Ok(matches);
		}

		OrgSimilarMatchesResult GetFilteredOrgMatchesResult(OrgSimilarMatchesResult orgSimilarMatchesResult, BusinessObjectFactory factory, string contactEmail, string routingPath = "", string sessionId = "")
		{
			var infoMessage = $"filter out contacts' org from matched orgs by contactEmail: {contactEmail}";
			Logger.AddLog(LogLevel.Info, infoMessage, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
			if (orgSimilarMatchesResult == null)
			{
				return null;
			}

			var activeUserOrgs = GetContacts(factory, contactEmail).Where(c => c.OC_WebAccessEnabled).Select(c => c.OC_OH).ToList();

			return new OrgSimilarMatchesResult
			{
				OrgMatches = orgSimilarMatchesResult.OrgMatches.Where(o => !activeUserOrgs.Contains(o.PK))
			};
		}

		public IEnumerable<Guid> GetAllExcludedOrganizationPks(BusinessObjectFactory factory, string sessionId, string routingPath)
		{
			var orgPksUnderWiseTechGlobal = GetOrganizationPksUnderWiseTechGlobalCompany(sessionId, routingPath);
			var excludedOrgPks = GetPksForExcludedOrganizationList(factory);
			var allExcluded = orgPksUnderWiseTechGlobal != null && excludedOrgPks != null ? orgPksUnderWiseTechGlobal.Concat(excludedOrgPks)
				: orgPksUnderWiseTechGlobal ?? excludedOrgPks;
			return allExcluded;
		}

		public IEnumerable<Guid> GetPksForExcludedOrganizationList(BusinessObjectFactory factory)
		{
			var orgQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			orgQuery.DefaultJoinCondition = JoinCondition.Or;

			foreach (var excludedOrgCode in excludedOrgCodes)
			{
				orgQuery.AddToFilter(OrgHeaderSchema.OH_Code, excludedOrgCode);
			}

			var excludedOrgs = factory.Load<OrgHeader>(orgQuery);

			if (!(excludedOrgs.Length > 0))
			{
				return default;
			}

			return excludedOrgs.Select(c => c.PK.ToGuid());
		}

		#endregion

		#region Organisation de-duplication

		[Route("organisation")]
		[HttpPost]
		public IHttpActionResult FindMatchingOrganisation([FromBody] OrganisationInfo organisationInfo)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = "api/BorderWiseRegistration/organisation";
			if (organisationInfo == null ||
				string.IsNullOrEmpty(organisationInfo.OrganisationAddress1) || string.IsNullOrEmpty(organisationInfo.OrganisationCity) || string.IsNullOrEmpty(organisationInfo.OrganisationState) ||
				string.IsNullOrEmpty(organisationInfo.OrganisationPostCode) || string.IsNullOrEmpty(organisationInfo.OrganisationCountryCode))
			{
				return BadRequest("All arguments must be provided.");
			}

			var factory = SetupEnvironmentAndGetFactory();
			var excludedOrgPks = GetAllExcludedOrganizationPks(factory, sessionId, routingPath);

			return Ok(FindPotentialMatchOrgs(factory, organisationInfo, excludedOrgPks));
		}

		#endregion

		#region Data Structures

		public class RegistrationValidationResponse
		{
			public IEnumerable<string> Messages { get; set; }
			public IEnumerable<string> PhoneValidationMessages { get; set; }
			public IEnumerable<ValidationResultItem> AddressSuggestions { get; set; }
		}

		public class ValidationResponse
		{
			public string RegisterEmail { get; set; }
			public Guid RegisterByAdminToken { get; set; }
			public bool HasErrors { get; set; }
			public string Message { get; set; }
		}

		public class OrgSimilarMatchesResult
		{
			public IEnumerable<OrgDetail> OrgMatches { get; set; }
		}

		public class OrgDetail
		{
			public Guid PK { get; set; }
			public string FullName { get; set; }
		}

		public class RegistrationSuccessResponse
		{
			public string Message { get; set; }
			public string RegisteredEmail { get; set; }

			public RegistrationInfo RegistrationInfo { get; set; }
		}

		public class RegistrationInfo
		{
			public Guid OrgContactPk { get; set; }
			public Guid OrgHeaderPk { get; set; }
			public Guid OrgHeaderAddressPk { get; set; }
			public Guid BillingContactPk { get; set; }
			public ReadOnlyMemory<byte> PasswordHash { get; set; }
			public ReadOnlyMemory<byte> PasswordSalt { get; set; }
			public int PasswordHashIterations { get; set; }
			public string OhCode { get; set; }
			public string ContactEmail { get; set; }
		}

		public class ContactInfo : OrganisationInfo
		{
			public string ContactName { get; set; }
			public string ContactWorkPhone { get; set; }
			public string ContactMobilePhone { get; set; }
			public string ContactBrokerID { get; set; }
			public string ContactJobTitle { get; set; }
			public string ContactPassword { get; set; }
			public DateTime? ContactDOB { get; set; }
			public string ContactStudentID { get; set; }
			public string ContactStudentInstitution { get; set; }

			public string BillingContactName { get; set; }
			public string BillingContactEmail { get; set; }

			public string PromoCode { get; set; }
		}

		public class OrganisationInfo
		{
			public Guid? OrganisationPk { get; set; }
			public string OrganisationName { get; set; }
			public string OrganisationAddress1 { get; set; }
			public string OrganisationAddress2 { get; set; }
			public string OrganisationCity { get; set; }
			public string OrganisationState { get; set; }
			public string OrganisationPostCode { get; set; }
			public string OrganisationCountryCode { get; set; }
			public string OrganisationWebSite { get; set; }
			public string OrganisationPhone { get; set; }
			public string OrganisationEmail { get; set; }
			public string OrganisationBusinessNumber { get; set; }
			public string OrgCode { get; set; }
		}

		public class RegistrationRequest
		{
			public string EmailAddress { get; set; }
			public bool IsActiveUserAllowedToRegisterToNewCompany { get; set; }
			public bool IsFreeTrial { get; set; }
			public string FirstName { get; set; }
			public string LastName { get; set; }
		}

		public class GroupRegistrationInvitationRequest
		{
			public Guid OrgPk { get; set; }
			public IEnumerable<string> EmailAddresses { get; set; }
			public string AdminEmail { get; set; }
			public string AdminName { get; set; }
		}

		#endregion

		#region Implementation

		#region Token Management / Email

		IHttpActionResult ValidateRegToken(Guid oneTimeRegistrationToken, BusinessObjectFactory factory, out string contactEmail, out Guid existingOrgPk)
		{
			var tokenRecord = factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, RegTokenName + oneTimeRegistrationToken));
			contactEmail = string.Empty;
			existingOrgPk = Guid.Empty;

			if (tokenRecord != null)
			{
				var otherDataString = tokenRecord.SD_BinaryValue.ToAscii();
				var otherData = otherDataString.Split('|');

				if (otherData.Length < 2)
				{
					return Content(HttpStatusCode.BadRequest, InvalidRegistrationToken + " [1]");
				}

				if (!ZDateTime.TryParseExact(otherData[0], out ZDateTime expiryTime, ZDateTime.LongTimeFormat))
				{
					return Content(HttpStatusCode.BadRequest, InvalidRegistrationToken + " [2]");
				}

				if (expiryTime < ZDateTime.UtcNow)
				{
					return Content(HttpStatusCode.BadRequest, "The registration link has expired. Please request a new registration token and try again.");
				}

				contactEmail = otherData[1];

				if (otherData.Length > 2 && !Guid.TryParse(otherData[2], out existingOrgPk))
				{
					return Content(HttpStatusCode.BadRequest, InvalidRegistrationToken + " [3]");
				}

				return null;
			}

			return Content(HttpStatusCode.BadRequest, "The registration link is not valid or has already been used. Please log in to BorderWise using the login details that were emailed to you, or request a new registration token.");
		}

		IHttpActionResult ValidateConfirmAccessToken(Guid token, BusinessObjectFactory factory, out Guid contactPk)
		{
			var tokenRecord =
				factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, ConfirmAccessTokenName + token));
			contactPk = Guid.Empty;

			if (tokenRecord == null)
			{
				return Content(HttpStatusCode.BadRequest,
					"Token not found. Can not confirm access.");
			}

			var otherDataString = tokenRecord.SD_BinaryValue.ToAscii();
			var otherData = otherDataString.Split('|');

			if (otherData.Length < 2)
			{
				return Content(HttpStatusCode.BadRequest,
					"Invalid token. Can not confirm access.");
			}

			if (!ZDateTime.TryParseExact(otherData[0], out ZDateTime expiryTime, ZDateTime.LongTimeFormat))
			{
				return Content(HttpStatusCode.Forbidden, "Invalid token expiry date. Can not confirm access.");
			}

			if (expiryTime < ZDateTime.UtcNow)
			{
				return Content(HttpStatusCode.Forbidden,
					"The token has expired. Can not confirm access.");
			}

			if (!Guid.TryParse(otherData[1], out contactPk))
			{
				return Content(HttpStatusCode.Forbidden, "Invalid token data. Can not confirm access.");
			}

			return null;
		}

		IHttpActionResult ValidateConfirmAccessContact(OrgContact contact)
		{
			if (contact == null)
			{
				return Content(HttpStatusCode.NotFound,
					"The user is not found. Please contact WiseTech support to enable web access and grant BorderWise access right.");
			}

			if (!contact.OC_IsActive)
			{
				return Content(HttpStatusCode.Forbidden,
					"The user is not active. Please active the user and try again.");
			}

			if (contact.OC_WebAccessEnabled && OrgContactWebUser.IsRightGrantedWithoutCache(EDIWebSecurityRightsList.BorderWise, contact))
			{
				return Content(HttpStatusCode.OK,
					"The user's access to BorderWise has been granted already");
			}

			return null;
		}

		const string InvalidRegistrationToken = "The registration link is invalid. Please request a new registration token and try again.";

		void DeleteToken(string tokenName, Guid token)
		{
			var newFactory = new BusinessObjectFactory { NameForDebugging = nameof(BorderWiseRegistrationController) + "-" + nameof(DeleteToken) };
			var tokenRecord = newFactory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, tokenName + token));

			if (tokenRecord == null)
			{
				return;
			}

			tokenRecord.Delete();
			newFactory.Save();
		}

		bool CheckExistingContactsAndNotify(BusinessObjectFactory factory, string emailAddress, bool isActiveUserAllowedToRegister)
		{
			var existingContacts = GetContacts(factory, emailAddress);
			if (existingContacts == null || !existingContacts.Any())
			{
				return false;
			}

			foreach (var contact in existingContacts)
			{
				var originalPhoneExtension = contact.OC_PhoneExtension;
				contact.OC_PhoneExtension = originalPhoneExtension.Length == OrgContactSchema.OC_PhoneExtension.MaxLength ?
					contact.OC_PhoneExtension.Remove(OrgContactSchema.OC_PhoneExtension.MaxLength - 1, 1) : (ZString)(contact.OC_PhoneExtension + "1");
				factory.Save();
				contact.OC_PhoneExtension = originalPhoneExtension;
				factory.Save();
			}

			var accessEnabledContacts = existingContacts.Where(x => x.OC_WebAccessEnabled).ToArray();
			if (accessEnabledContacts.Any())
			{
				if (!isActiveUserAllowedToRegister)
				{
					accessEnabledContacts.ForEach(c => SendBorderWiseLoginDetailsEmail(c));
				}
				else
				{
					return false;
				}
			}
			else
			{
				// for each org, select the first contact
				var contacts = existingContacts.GroupBy(c => c.OC_OH).Select(g => g.First());
				if (contacts.Count() == 1)
				{
					var contact = contacts.First();
					var allActiveContacts = GetContacts(factory, orgPk: contact.OC_OH.ToGuid());
					if (allActiveContacts.Count() == 1)
					{
						contact.OC_WebAccessEnabled = true;
						factory.Save();
						SendBorderWiseLoginDetailsEmail(contact);
						return true;
					}
				}

				var emailToAdminSent = SendConfirmAccessEmailToAdmins(factory, contacts);
				SendNoActiveAccountEmailToUser(contacts.First(), emailToAdminSent);
			}

			return true;
		}

		void SendBorderWiseLoginDetailsEmail(OrgContact orgContact, bool isStudent = false, bool isApprovedByAdmin = false)
		{
			var subject = isStudent ? "BorderWise student verification" : "Your BorderWise access details";
			var body = $@"Dear {orgContact.OC_ContactName},<br /><br />
{(isStudent ? StudentWarning : string.Empty)}
{(isApprovedByAdmin ? "Your admin has approved you for BorderWise.<br/>" : string.Empty)}
The access details to your <a href='https://app.borderwise.com/'>BorderWise</a> account are:<br /><br />
Company Code: <b>{orgContact.OrgCode}</b><br />
User Name: <b>{orgContact.OC_Email}</b><br /><br />
Forgotten your password? Use the <a href=""https://app.borderwise.com/account/forgot-password/"">Forgot Password</a> link to reset your password.";

			BorderWiseUtilities.SendEmail(BorderWiseEmailFromAddress, orgContact.Email, subject, body);
		}

		void SendNoActiveAccountEmailToUser(OrgContact orgContact, bool emailToAdminSent = false)
		{
			var subject = "No BorderWise Access Rights Assigned to Your Account";
			var body = $@"Dear {orgContact.OC_ContactName},<br /><br />
This email address is associated with one or more accounts. However, none of them has BorderWise access rights.<br /><br />
{(emailToAdminSent ? "An email has been sent to your company's admin to confirm your access to BorderWise." : "Please contact WiseTech support to enable BorderWise access.")}";

			BorderWiseUtilities.SendEmail(BorderWiseEmailFromAddress, orgContact.Email, subject, body);
		}

		void SendBorderWiseResetPasswordEmail(OrgContact orgContact)
		{
			var resetPasswordUrl = BorderWiseUtilities.GenerateResetPasswordUrl(orgContact);
			var body = $@"Dear {orgContact.OC_ContactName},<br /><br />
A request was recently submitted to reset the password for your <a href='https://app.borderwise.com/'>BorderWise</a> account. <br /><br />

Company Code: <b>{orgContact.OrgCode}</b><br />
Please Click <a href='{resetPasswordUrl}'>Reset Password</a> to complete this action. <br /><br />

You can also paste the below link in your browser to reset your password. <br /><br />

<a href='{resetPasswordUrl}'>{resetPasswordUrl}</a>";

			BorderWiseUtilities.SendEmail(BorderWiseEmailFromAddress, orgContact.Email, "Your BorderWise password reset details", body);
		}

		bool SendConfirmAccessEmailToAdmins(BusinessObjectFactory factory, IEnumerable<OrgContact> requestContacts)
		{
			var adminContactInfos = GetOrgBorderWiseAdminInfo(requestContacts.Select(c => c.OC_OH.ToGuid()).ToList());

			if (!adminContactInfos.Any())
			{
				// TODO: decide what to do in here. may be in a different WI
				return false;
			}

			foreach (var adminInfo in adminContactInfos)
			{
				//create token
				var requestContact = requestContacts.Single(c => c.OC_OH == adminInfo.OrgHeaderPk);
				var confirmAccessToken = CreateConfirmAccessToken(requestContact.PK, factory);

				factory.Save();

				var adminContact = GetContacts(factory, adminInfo.AdminContactEmail, adminInfo.OrgHeaderPk).First();

				var url = EDIDataRegistry.Instance.BorderWiseConfirmWebAccessUrl.Value + confirmAccessToken;

				var body = $@"Dear {adminContact.OC_ContactName},<br /><br />
You are receiving this email because you are the BorderWise administrator for your organization ({requestContact.Header.OH_Code}).<br /><br />
A new user, <a href='mailto:{requestContact.OC_Email}'>{requestContact.OC_Email}</a> is requesting an access to <a href='https://app.borderwise.com/'>BorderWise</a>. <br />
It is required for a technical contact to approve this request. If you believe this request is genuine please <a href='{url}'>click here to approve the access</a>.<br /><br />
Please discard this email if you do not approve this request.<br /><br />
This link will be expired in 24 hours.";

				BorderWiseUtilities.SendEmail(BorderWiseEmailFromAddress, adminInfo.AdminContactEmail, "A User request access to BorderWise", body);
			}
			return true;
		}

		public List<OrgHeaderAdminInfo> GetOrgBorderWiseAdminInfo(List<Guid> orgPks)
		{
			var result = new List<OrgHeaderAdminInfo>();
			var httpClientHandler = GetHttpClientHandler();
			var relativeUri = "/v1/ump/organizations/admins";

			if (!Uri.TryCreate(EDIDataRegistry.Instance.BorderWiseUmpApiAddress.Value.TrimEnd('/') + relativeUri, UriKind.Absolute, out var requestUri))
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Invalid URI settings in Registry '{0}'.", EDIDataRegistry.Instance.BorderWiseUmpApiAddress.HumanReadableRegistryPath()));
				return result;
			}

			using (var httpClient = new HttpClient(httpClientHandler))
			{
				var getOrgsAdminsRequest = new GetOrgsAdminsRequest
				{
					ApiKey = EDIDataRegistry.Instance.BorderWiseUmpApiUpdatePasswordApiKey.Value,
					OrgPks = orgPks
				};

				var httpRequestMessage = new HttpRequestMessage
				{
					RequestUri = requestUri,
					Method = HttpMethod.Post,
					Content = new StringContent(JsonConvert.SerializeObject(getOrgsAdminsRequest), Encoding.UTF8,
						"application/json")
				};

				try
				{
					var response = httpClient.SendAsync(httpRequestMessage).Result;
					var responseString = response.Content.ReadAsStringAsync().Result;

					if (!response.IsSuccessStatusCode)
					{
						ErrorReporter.ReportOnce(
							$"Get OrgHeader admin info from BorderWise UMP failed. OrgPks: {string.Join(";", orgPks)}, StatusCode: {response.StatusCode}, Reason: {response.ReasonPhrase}, Message: {responseString}");
						return result;
					}

					result = JsonConvert.DeserializeObject<List<OrgHeaderAdminInfo>>(responseString);
				}
				catch (Exception exception)
				{
					ErrorReporter.ReportOnce(
						$"Get OrgHeader admin info from BorderWise UMP failed. OrgPks: {string.Join(";", orgPks)}",
						exception);
				}
			}
			return result;
		}

		protected virtual HttpClientHandler GetHttpClientHandler()
		{
			return new HttpClientHandler();
		}

		IHttpActionResult SendRegistrationToken(RegistrationRequest registrationRequest, BusinessObjectFactory factory)
		{
			var url = CreateTokenAndGetRegistrationUrl(registrationRequest, null, factory);

			var body = $@"Welcome to BorderWise.<br /><br />
To activate your BorderWise account, we need you to provide additional details via a secure link. Please click <a href='{url}'>Activate Account</a> to complete this process.<br /><br />
This link can only be used once and will expire in one hour. A new link can be requested by beginning the registration process again.";

			BorderWiseUtilities.SendEmail(BorderWiseEmailFromAddress, registrationRequest.EmailAddress, "Activate your BorderWise account", body);

			factory.Save();

			return Ok(CommonConstants.userRegistrationOneTimeLinkResponse);
		}

		void SendGroupRegistrationToken(string emailAddress, OrgHeader orgHeader, string adminContactName, string adminEmail, BusinessObjectFactory factory)
		{
			var registrationRequest = new RegistrationRequest { EmailAddress = emailAddress };
			var url = CreateTokenAndGetRegistrationUrl(registrationRequest, orgHeader, factory, GroupRegistrationTokenExpiryInHours);

			var body = $@"Welcome to <a href='https://app.borderwise.com/'>BorderWise</a>.<br /><br />
You have been invited by {adminContactName} (<a href='mailto:{adminEmail}'>{adminEmail}</a>) to register for <a href='https://app.borderwise.com/'>BorderWise</a>.<br /><br />
In order to activate your account, you need to provide further details about yourself via a secure link. Please click <a href='{url}'>Activate Account</a> to provide these details.<br /><br />
This link can only be used once and will expire in 3 days. If required, you can ask your company's BorderWise Administrator to send you a new invitation link.";

			BorderWiseUtilities.SendEmail(BorderWiseEmailFromAddress, emailAddress, "You are invited to BorderWise", body);

			factory.Save();
		}

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		string CreateTokenAndGetRegistrationUrl(RegistrationRequest registrationRequest, OrgHeader orgHeader, BusinessObjectFactory factory, int expiryInHours = 1)
		{
			var token = CreateRegToken(registrationRequest.EmailAddress, orgHeader == null ? default(Guid?) : orgHeader.PK.ToGuid(), expiryInHours, factory);

			var url = $"{EDIDataRegistry.Instance.BorderWiseRegistrationUrl.Value}{token}&contactEmail={WebUtility.UrlEncode(registrationRequest.EmailAddress)}";

			if (registrationRequest.IsFreeTrial)
			{
				url += $"&firstName={WebUtility.UrlEncode(registrationRequest.FirstName)}&lastName={WebUtility.UrlEncode(registrationRequest.LastName)}";
			}

			if (orgHeader != null)
			{
				url += "&orgCode=" + WebUtility.UrlEncode(orgHeader.OH_Code) + "&orgName=" + WebUtility.UrlEncode(orgHeader.OH_FullName);
			}

			return url;
		}

		const string BorderWiseEmailFromAddress = "support@borderwise.com";
		const int GroupRegistrationTokenExpiryInHours = 72;

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		static Guid CreateRegToken(string emailAddress, Guid? existingOrgPk, int expiryInHours, BusinessObjectFactory factory)
		{
			var expiryTime = ZDateTime.UtcNow.AddHours(expiryInHours).ToString(ZDateTime.LongTimeFormat, CultureInfo.InvariantCulture);
			var tokenValue = expiryTime + "|" + emailAddress;
			if (existingOrgPk != null)
			{
				tokenValue += "|" + existingOrgPk.Value;
			}

			return BorderWiseUtilities.CreateTokenCore(RegTokenName, tokenValue, factory);
		}

		static Guid CreateConfirmAccessToken(ZGuid requestContactPk, BusinessObjectFactory factory)
		{
			var expiryTime = ZDateTime.UtcNow.AddHours(24).ToString(ZDateTime.LongTimeFormat, CultureInfo.InvariantCulture);
			var tokenValue = expiryTime + "|" + requestContactPk.ToGuid();

			return BorderWiseUtilities.CreateTokenCore(ConfirmAccessTokenName, tokenValue, factory);
		}

		internal const string RegTokenName = "BorderWiseReg-";
		internal const string ConfirmAccessTokenName = "BorderWiseConfirmAccess-";

		#endregion
		#region Org / Address / Contact

		IHttpActionResult ValidateAddressAndPhone(OrgContact contact, OrgHeader header, bool acceptUserAddress, bool acceptUserPhone, string routingPath = "", string sessionId = "")
		{
			Logger.AddLog(LogLevel.Info, "Start validating address and phone number.", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
			var result = new RegistrationValidationResponse();

			var messages = new List<string>();

			var phoneValidationMessages = new List<string>();
			phoneValidationMessages.Add(ValidatePhone(acceptUserPhone, contact.OC_Phone_FormattedInfo, contact.OC_Phone_IsManuallyVerifiedInfo, "Contact Phone", header.MainAddress.OA_RN_NKCountryCode));
			phoneValidationMessages.Add(ValidatePhone(acceptUserPhone, contact.OC_Mobile_FormattedInfo, contact.OC_Mobile_IsManuallyVerifiedInfo, "Contact Mobile", header.MainAddress.OA_RN_NKCountryCode));

			var newAddress = (OrgAddress)header.Addresses.SingleOrDefault(x => !x.IsInDatabase);
			if (newAddress != null)
			{
				phoneValidationMessages.Add(ValidatePhone(acceptUserPhone, newAddress.OA_Phone_FormattedInfo, newAddress.OA_Phone_IsManuallyVerifiedInfo, "Organisation Phone", newAddress.OA_RN_NKCountryCode));

				if (!acceptUserAddress)
				{
					var validationResult = ValidateAddress(newAddress);
					if (!string.IsNullOrEmpty(validationResult.Message))
					{
						messages.Add(validationResult.Message);
					}

					if (newAddress.ValidationStatus != AddressValidationStatus.Verified)
					{
						if (validationResult.TopRecommendedAddress != null)
						{
							result.AddressSuggestions = new[] { validationResult.TopRecommendedAddress };
						}
						else if (validationResult.SuggestedResults != null && validationResult.SuggestedResults.Any())
						{
							result.AddressSuggestions = validationResult.SuggestedResults;
						}
						else
						{
							messages.Add("The address you entered appears to be incorrect. Please check it carefully.");
						}
					}
				}
				else
				{
					newAddress.ValidationStatus = AddressValidationStatus.ManuallyVerified;
				}
			}

			result.Messages = messages.Where(x => !string.IsNullOrEmpty(x));
			result.PhoneValidationMessages = phoneValidationMessages.Where(x => !string.IsNullOrEmpty(x));
			if (result.PhoneValidationMessages.Any() || result.Messages.Any() || result.AddressSuggestions != null)
			{
				Logger.AddLog(LogLevel.Info, "Failed to validate address and phone number.", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
				return Ok(result);
			}

			Logger.AddLog(LogLevel.Info, "Finished validating address and phone number.", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
			return null;
		}

		static string ValidatePhone(bool acceptUserValue, ZPropertyInfo phoneInfo, ZPropertyInfo manuallyVerifiedInfo, string description, string countryCode)
		{
			if (!phoneInfo.Value.IsEmpty)
			{
				if (!acceptUserValue)
				{
					var result = (ZString)new PhoneNumberFormatter().FormatE164((ZString)phoneInfo.Value, countryCode);
					if (!result.IsEmpty)
					{
						phoneInfo.Value = result;
					}
					else
					{
						((IBusinessObjectInternals)phoneInfo.BizObj).Validate(phoneInfo);
						return string.Join(System.Environment.NewLine, phoneInfo.Notifications.Select(x => description + ": " + x.Message));
					}
				}
				else
				{
					manuallyVerifiedInfo.Value = ZBool.True;
				}
			}
			return null;
		}
		IHttpActionResult SendLoginDetailsAndRegistrationSuccessResponse(ContactInfo contactInfo, OrgContact contact, string contactEmail, string sessionId, string routingPath, OrgContact billingContact = null, bool isApprovedByAdmin = false)
		{
			var isStudent = !string.IsNullOrEmpty(contactInfo.ContactStudentID) || !string.IsNullOrEmpty(contactInfo.ContactStudentInstitution);

			var infoMessage = $"Send BorderWise login details. IsStudent: {isStudent}";
			Logger.AddLog(LogLevel.Info, infoMessage, (int)HttpStatusCode.OK, sessionId, routingPath: routingPath);
			SendBorderWiseLoginDetailsEmail(contact, isStudent, isApprovedByAdmin);

			Logger.AddLog(LogLevel.Info, "Complete registration.", (int)HttpStatusCode.OK, sessionId, routingPath: routingPath);
			return Ok(new RegistrationSuccessResponse
			{
				Message = string.Format(CultureInfo.CurrentCulture, "You have successfully registered as a BorderWise user. Your login details have been emailed to {0}.", contactEmail),
				RegisteredEmail = contactEmail,
				RegistrationInfo = new RegistrationInfo
				{
					OrgContactPk = contact.PK.ToGuid(),
					OrgHeaderPk = contact.OC_OH.ToGuid(),
					OrgHeaderAddressPk = contact.OrgAddress.PK.ToGuid(),
					PasswordHash = new ReadOnlyMemory<byte>(contact.OC_PasswordHash),
					PasswordSalt = new ReadOnlyMemory<byte>(contact.OC_PasswordSalt),
					PasswordHashIterations = contact.OC_PasswordHashIterations,
					BillingContactPk = billingContact != null ? billingContact.PK.ToGuid() : Guid.Empty,
					OhCode = contact.OrgCode,
					ContactEmail = contact.OC_Email,
				},
			});
		}

		IEnumerable<OrgContact> GetContacts(BusinessObjectFactory factory, string contactEmail = "", Guid orgPk = default)
		{
			var contactQuery = new ZDBOnlyQuery(typeof(OrgContact));
			if (!string.IsNullOrEmpty(contactEmail))
			{
				contactQuery.AddToFilter(OrgContactSchema.OC_Email, contactEmail);
			}
			contactQuery.AddToFilter(OrgContactSchema.OC_IsActive, true);
			contactQuery.OrderBy = OrgContactSchema.OC_WebAccessEnabled.Name + " DESC";

			var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgContactSchema.OC_OH);
			orgSubQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
			if (orgPk != default)
			{
				orgSubQuery.AddToFilter(OrgHeaderSchema.PK, orgPk);
			}
			contactQuery.AddSubQuery(orgSubQuery, JoinCondition.And);

			return factory.Load<OrgContact>(contactQuery);
		}

		OrgSimilarMatchesResult GetSimilarOrgsByEmailDomain(string emailAddress)
		{
			var orgDomainMatches = GetSimilarOrgsByEmailDomainCore(emailAddress);
			if (orgDomainMatches.Any())
			{
				var matches = orgDomainMatches.Select(x =>
				{
					var countryName = x.CountryName;
					var cityName = x.CityName;
					if (cityName.IsEmpty)
					{
						cityName = x.PortName;
					}
					var location = countryName.ToString();
					if (!cityName.IsEmpty)
					{
						location = location + ", " + cityName;
					}

					return new OrgDetail { PK = x.PK.ToGuid(), FullName = x.OH_FullName.ToString() + " (" + location + ")" };
				});
				return new OrgSimilarMatchesResult { OrgMatches = matches.OrderBy(x => x.FullName) };
			}
			return null;
		}

		public OrgSimilarMatchesResult FindPotentialMatchOrgs(BusinessObjectFactory factory, OrganisationInfo organisationInfo, IEnumerable<Guid> excludedOrgPks = default, string routingPath = "", string sessionId = "")
		{
			var infoMessage = $"Attempt to find potential matching organizations for OrganisationName: {organisationInfo.OrganisationName}.";
			Logger.AddLog(LogLevel.Info, infoMessage, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
#if DEBUG
			if (Globals.IsTest)
			{
				if (PotentialMatchOrgResultForTesting != null)
				{
					return PotentialMatchOrgResultForTesting;
				}
			}
#endif
			if (organisationInfo.OrganisationPk.HasValue && organisationInfo.OrganisationPk != Guid.Empty)
			{
				return null;
			}

			var orgName = string.IsNullOrWhiteSpace(organisationInfo.OrganisationName)
				? DummyOrgName
				: organisationInfo.OrganisationName;
			var dedupOrgHeader = new DeduplicationOrgHeader(orgName)
			{
				OH_PK = Guid.NewGuid(),
				OH_Code = ZString.Empty
			};

			var mainAddress = new DeduplicationOrgAddress
			{
				OA_PK = Guid.NewGuid(),
				OA_OH = dedupOrgHeader.OH_PK,
				OA_RL_NKRelatedPortCode = string.Empty,
				OA_RN_NKCountryCode = organisationInfo.OrganisationCountryCode,
				OA_AdditionalAddressInformation = string.Empty,
				OA_Address1 = organisationInfo.OrganisationAddress1,
				OA_Address2 = GetDefaultValue(organisationInfo.OrganisationAddress2),
				OA_City = organisationInfo.OrganisationCity,
				OA_Code = string.Empty,
				OA_PostCode = organisationInfo.OrganisationPostCode,
				OA_State = organisationInfo.OrganisationState,
				OA_Email = GetDefaultValue(organisationInfo.OrganisationEmail),
				OA_Fax = string.Empty,
				OA_Phone = organisationInfo.OrganisationPhone ?? string.Empty,
				OA_Mobile = string.Empty,
				OA_ValidationStatus = "NTC"
			};
			dedupOrgHeader.OrgAddresses = new[] { mainAddress };

			var finder = new BorderWiseOrgHeaderMatchEngineFinder(dedupOrgHeader, true, true);

			var potentialDuplicates = finder.FindPotentialDuplicates(false);

			if (potentialDuplicates == null || !potentialDuplicates.Any())
			{
				Logger.AddLog(LogLevel.Info, "No matching organizations found.", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
				return null;
			}

			var orgPks = potentialDuplicates.Select(o => o.TargetPK).ToArray();

			var filteredOrgPks = excludedOrgPks != null ? orgPks.Where(p => !excludedOrgPks.Contains(p)).ToArray() : orgPks;

			if (!filteredOrgPks.Any())
			{
				Logger.AddLog(LogLevel.Info, "No matching organizations found after filtering out excluded organizations.", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
				return null;
			}

			var orgQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			orgQuery.AddToFilter(OrgHeaderSchema.PK, filteredOrgPks);
			orgQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);

			var activeOrgs = factory.Load<OrgHeader>(orgQuery);

			var orgDetails = activeOrgs.Select(o => new OrgDetail
			{
				PK = o.PK.ToGuid(),
				FullName = $"{o.OH_FullName} ({o.OH_Code})"
			}).OrderBy(o => o.FullName);

			return new OrgSimilarMatchesResult()
			{
				OrgMatches = orgDetails
			};
		}

		class BorderWiseOrgHeaderMatchEngineFinder : OrgHeaderMatchEngineFinder
		{
			public BorderWiseOrgHeaderMatchEngineFinder(DeduplicationOrgHeader header, bool shouldUseCache, bool useMaxRecords) : base(header, shouldUseCache, useMaxRecords)
			{
			}

			protected override bool ShouldFindDuplications => true;
		}

#if DEBUG
		[ThreadStatic] static internal OrgSimilarMatchesResult PotentialMatchOrgResultForTesting;
#endif

		static string GetDefaultValue(string input)
		{
			return string.IsNullOrWhiteSpace(input) ? string.Empty : input;
		}

		string ExtractMessageFromActionResult(IHttpActionResult result)
		{
			return result switch
			{
				BadRequestErrorMessageResult badRequest => badRequest.Message,
				NegotiatedContentResult<string> content => content.Content,
				_ => "Unknown validation error."
			};
		}

		IEnumerable<OrgHeader> GetSimilarOrgsByEmailDomainCore(string emailAddress)
		{
			Argument.NotNullOrEmpty(emailAddress, nameof(emailAddress));

			var result = new List<EDIOrgHeader>();
			if (!TextStandardizerHelper.IsGenericDomain(emailAddress))
			{
				var matches = MasterDataMatchFinder.GetOrgHeaderByEmailDomain(emailAddress);
				result.AddRange(matches.Cast<EDIOrgHeader>().Where(x => x != null && x.OH_IsActive && x.LicCompany != null).DistinctBy(x => x.PK));
			}

			return result;
		}

		static (OrgHeader, OrgContact) CreateOrUpdateOrganisationAndContact(ContactInfo contactInfo, BusinessObjectFactory factory, string contactEmail, OrgHeader existingOrg)
		{
			OrgHeader org;
			OrgAddress addressForContact;

			org = existingOrg ?? CreateNewOrg(contactInfo, factory);

			var userSelectedOrgPk = contactInfo.OrganisationPk.HasValue && contactInfo.OrganisationPk.Value != Guid.Empty ? contactInfo.OrganisationPk.Value : Guid.Empty;
			if (userSelectedOrgPk != Guid.Empty)
			{
				addressForContact = AddAddressIfNoMatch(contactInfo, factory, org);
			}
			else
			{
				addressForContact = org.MainAddress;
			}

			return CreateContact(contactInfo, org, contactEmail, addressForContact, factory);
		}

		[SuppressMessage("CargoWiseOne", "CW1062:DoNotUseDateTimeToday", Justification = "Baseline")]
		static (OrgHeader, OrgContact) CreateContact(ContactInfo contactInfo, OrgHeader org, string contactEmail, OrgAddress addressForContact, BusinessObjectFactory factory)
		{
			var contact = factory.New<OrgContact>();

			contact.OC_OH = org.PK;
			contact.OC_ContactName =
				OrgContactUniqueNameHelper.GenerateUniqueContactName(org, org.Contacts, contactInfo.ContactName.TrimEnd());

			contact.OC_WebAccessEnabled = true;

			contact.OC_ContactSource = ContactSource;

			contact.OC_Email = contactEmail;

			if (!string.IsNullOrEmpty(contactInfo.ContactWorkPhone))
			{
				contact.OC_Phone = BorderWiseUtilities.SubStringSafe(contactInfo.ContactWorkPhone, OrgContactSchema.OC_Phone.MaxLength).Replace(" ", string.Empty);
			}

			if (!string.IsNullOrEmpty(contactInfo.ContactMobilePhone))
			{
				contact.OC_Mobile = BorderWiseUtilities.SubStringSafe(contactInfo.ContactMobilePhone, OrgContactSchema.OC_Mobile.MaxLength).Replace(" ", string.Empty);
			}

			if (!string.IsNullOrEmpty(contactInfo.ContactJobTitle))
			{
				contact.OC_Title = BorderWiseUtilities.SubStringSafe(contactInfo.ContactJobTitle, OrgContactSchema.OC_Title.MaxLength);
			}

			if (!string.IsNullOrEmpty(contactInfo.ContactPassword))
			{
				contact.SetHashedPassword(contactInfo.ContactPassword);
			}

			if (contactInfo.ContactDOB != null)
			{
				contact.OC_Birthday = contactInfo.ContactDOB.Value;
			}

			if (addressForContact != null)
			{
				contact.OC_OA_OrgAddress = addressForContact.PK;
			}

			if (!string.IsNullOrEmpty(contactInfo.ContactBrokerID))
			{
				BorderWiseUtilities.AddContactCertificate(contact, CertificateTypePairList.Codes.BR1, contactInfo.ContactBrokerID);
			}

			if (!string.IsNullOrEmpty(contactInfo.ContactStudentID) || !string.IsNullOrEmpty(contactInfo.ContactStudentInstitution))
			{
				BorderWiseUtilities.AddContactCertificate(contact, CertificateTypePairList.Codes.MS1, contactInfo.ContactStudentID, ZDateTime.Empty, new ZDateTime(ZDateTime.Today.AddDays(CommonConstants.studentCertificateExpiryDays)), "Student" + (!string.IsNullOrEmpty(contactInfo.ContactStudentInstitution) ? " - " + contactInfo.ContactStudentInstitution : string.Empty));
			}

			return (org, contact);
		}

		static OrgAddress AddAddressIfNoMatch(ContactInfo contactInfo, BusinessObjectFactory factory, OrgHeader org)
		{
			var newAddress = factory.New<OrgAddress>();
			SetAddressFields(newAddress, contactInfo);
			newAddress.AddAddressType(OrgAddressType.Office);

			// Simple Fuzzy Match
			foreach (var existingAddress in org.Addresses.Cast<OrgAddress>())
			{
				var comparisonResult = ComparisonRunner.CompareAddresses(newAddress.AddressAsASingleLine, existingAddress.AddressAsASingleLineWithoutCompanyName, newAddress.OA_Language);
				if (comparisonResult.ConfidenceRating == ConfidenceRating.High)
				{
					newAddress.Delete();
					return existingAddress;
				}
			}

			// Geo Distance Match
			var newAddressGeo = GetGeographyForAddress(newAddress);
			if (IsValidLocation(newAddressGeo))
			{
				foreach (var existingAddress in org.Addresses.Cast<OrgAddress>())
				{
					var existingAddressGeo = GetGeographyForAddress(existingAddress);
					if (IsValidLocation(existingAddressGeo))
					{
						var distance = RefLatLongPostcode.CalculateDistance(existingAddressGeo.Latitude.Value, existingAddressGeo.Longitude.Value, newAddressGeo.Latitude.Value, newAddressGeo.Longitude.Value);
						if (distance < 0.1)    // Km
						{
							newAddress.Delete();
							return existingAddress;
						}
					}
				}
			}

			// No Match
			org.Addresses.Add(newAddress);
			return newAddress;
		}

		static ZGeography GetGeographyForAddress(OrgAddress address)
		{
			if (IsValidLocation(address.GeoLocation))
			{
				return address.GeoLocation;
			}
			else
			{
				var result = ValidateAddress(address);
				if (result.ResultAddress != null &&
					(result.ResultAddress.ResultStatusCode == ValidationResultStatusValues.PointExact.Code ||
							result.ResultAddress.ResultStatusCode == ValidationResultStatusValues.PointClose.Code ||
							result.ResultAddress.ResultStatusCode == ValidationResultStatusValues.StreetExact.Code ||
							result.ResultAddress.ResultStatusCode == ValidationResultStatusValues.StreetClose.Code))
				{
					return ZGeography.CreatePoint(result.ResultAddress.Longitude, result.ResultAddress.Latitude);
				}

				return ZGeography.Empty;
			}
		}

		static WebAddressValidationResult ValidateAddress(OrgAddress address)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				ValidateAddressActionForTesting?.Invoke(address);

				if (AddressValidationResultForTesting != null)
				{
					return AddressValidationResultForTesting;
				}
			}
#endif

			using (var cancellationToken = new CancellationTokenSource())
			{
				// Must call this sync, as otherwise ZSecurity blows up with a StackOverflowException
				return AddressValidationService.ValidateAddress(address, cancellationToken);
			}
		}

#if DEBUG
		[ThreadStatic] static internal WebAddressValidationResult AddressValidationResultForTesting;
		[ThreadStatic] static internal Action<OrgAddress> ValidateAddressActionForTesting;
#endif

		static bool IsValidLocation(ZGeography geography)
		{
			// IsEmpty returns false for 0,0 - MDM team is fixing
			return !geography.IsEmpty && geography.Longitude != null && geography.Longitude != 0 && geography.Latitude != null && geography.Latitude != 0;
		}

		static OrgHeader CreateNewOrg(ContactInfo contactInfo, BusinessObjectFactory factory)
		{
			var org = factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = GetUNLOCOFromCountryAndCity(contactInfo.OrganisationCity, contactInfo.OrganisationCountryCode, factory);

			var orgName = string.IsNullOrEmpty(contactInfo.OrganisationName) ? contactInfo.ContactName : contactInfo.OrganisationName;
			if (!string.IsNullOrEmpty(orgName))
			{
				org.OH_FullName = BorderWiseUtilities.SubStringSafe(orgName, OrgHeaderSchema.OH_FullName.MaxLength);
			}

			if (!string.IsNullOrEmpty(contactInfo.OrganisationBusinessNumber))
			{
				org.LocalBusinessRegNo = BorderWiseUtilities.SubStringSafe(contactInfo.OrganisationBusinessNumber, OrgCusCodeSchema.OK_CustomsRegNo.MaxLength);
			}

			if (!string.IsNullOrEmpty(contactInfo.OrganisationWebSite))
			{
				org.MainWebURL.PU_URL = BorderWiseUtilities.SubStringSafe(contactInfo.OrganisationWebSite, OrgWebURLSchema.PU_URL.MaxLength);
			}

			SetAddressFields(org.MainAddress, contactInfo);
			return org;
		}

		static string GetUNLOCOFromCountryAndCity(string city, string countryCode, BusinessObjectFactory factory)
		{
			var result = RefUNLOCO.GetPortFromNameAndCountryCode(factory, city, countryCode)?.RL_Code.ToString();
			if (string.IsNullOrEmpty(result))
			{
				result = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, countryCode))?.RL_Code.ToString();
				if (string.IsNullOrEmpty(result))
				{
					result = Env.CurrentBranch.NKUNLOCO;
				}
			}

			return result;
		}

		static void NotifyNewOrganisationRegistration(OrgHeader org, OrgContact contact, BusinessObjectFactory factory)
		{
			var groupRegistryItem = EDIDataRegistry.Instance.BorderWiseNewOrganisationEmailNotificationGroup;

			var subject = $"BorderWise New Organisation - {org.OH_FullName}";

			var emailDomain = contact.OC_Email.Split('@').Last();
			var orgQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			orgQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);

			var contactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_OH);
			contactSubQuery.AddToFilter(OrgContactSchema.OC_Email, SQLComparisonOperator.EndsWith, $"@{emailDomain}");
			contactSubQuery.AddToFilter(OrgContactSchema.OC_IsActive, true);
			orgQuery.AddSubQuery(contactSubQuery, JoinCondition.And);

			// Load all results into memory
			var allOrgs = factory.Load<OrgHeader>(orgQuery)
				.OrderBy(o => o.OH_Code)
				.Select(o => new
				{
					o.PK,
					o.OH_Code,
					o.OH_FullName
				})
				.ToList();

			var topOrgs = allOrgs.Take(10).ToList();

			var totalOrgsCount = allOrgs.Count;

			var url = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.Organisation, org.PK);
			var bodyBuilder = new StringBuilder();
			bodyBuilder.AppendLine($@"<a href='{url}'>{org.OH_FullName} ({org.OH_Code})</a> was created through the BorderWise Registration process and should be verified.<br>
ContactName: {contact.OC_ContactName} <br>
ContactEmail: {contact.OC_Email} <br>
The email domain has {(totalOrgsCount > 0 ? $"been used in {totalOrgsCount} organization(s):" : "not been used in any organization.")} <br>");
			if (totalOrgsCount > 10)
			{
				bodyBuilder.AppendLine("First ten organizations ordered by their code are shown.<br>");
			}
			bodyBuilder.AppendLine("<ul>");

			foreach (var activeOrg in topOrgs)
			{
				var orgUrl = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.Organisation, activeOrg.PK);
				bodyBuilder.AppendLine($"<li><a href='{orgUrl}'>{activeOrg.OH_FullName} ({activeOrg.OH_Code})</a></li>");
			}

			bodyBuilder.AppendLine("</ul>");

			// Send the email
			BorderWiseUtilities.SendEmail(BorderWiseEmailFromAddress, string.Empty, subject, bodyBuilder.ToString(), groupRegistryItem, false, factory);
		}

		static void SetAddressFields(OrgAddress address, ContactInfo contactInfo)
		{
			if (!string.IsNullOrEmpty(contactInfo.OrganisationCountryCode))
			{
				address.OA_RN_NKCountryCode = BorderWiseUtilities.SubStringSafe(contactInfo.OrganisationCountryCode, OrgAddressSchema.OA_RN_NKCountryCode.MaxLength);
			}

			if (!string.IsNullOrEmpty(contactInfo.OrganisationAddress1))
			{
				address.OA_Address1 = BorderWiseUtilities.SubStringSafe(contactInfo.OrganisationAddress1, OrgAddressSchema.OA_Address1.MaxLength);
			}

			if (!string.IsNullOrEmpty(contactInfo.OrganisationAddress2))
			{
				address.OA_Address2 = BorderWiseUtilities.SubStringSafe(contactInfo.OrganisationAddress2, OrgAddressSchema.OA_Address2.MaxLength);
			}

			if (!string.IsNullOrEmpty(contactInfo.OrganisationCity))
			{
				address.OA_City = BorderWiseUtilities.SubStringSafe(contactInfo.OrganisationCity, OrgAddressSchema.OA_City.MaxLength);
			}

			if (!string.IsNullOrEmpty(contactInfo.OrganisationState))
			{
				address.OA_State = BorderWiseUtilities.SubStringSafe(contactInfo.OrganisationState, OrgAddressSchema.OA_State.MaxLength);
			}

			if (!string.IsNullOrEmpty(contactInfo.OrganisationPostCode))
			{
				address.OA_PostCode = BorderWiseUtilities.SubStringSafe(contactInfo.OrganisationPostCode, OrgAddressSchema.OA_PostCode.MaxLength);
			}

			if (!string.IsNullOrEmpty(contactInfo.OrganisationPhone))
			{
				address.OA_Phone = BorderWiseUtilities.SubStringSafe(contactInfo.OrganisationPhone, OrgAddressSchema.OA_Phone.MaxLength);
			}

			if (!string.IsNullOrEmpty(contactInfo.OrganisationEmail))
			{
				address.OA_Email = BorderWiseUtilities.SubStringSafe(contactInfo.OrganisationEmail, OrgAddressSchema.OA_Email.MaxLength);
			}

			address.OA_Code = BorderWiseUtilities.SubStringSafe("BW- " + address.OA_Code, OrgAddressSchema.OA_Code.MaxLength);
		}

		static OrgContact CreateBillingContact(ContactInfo contactInfo, OrgHeader org, BusinessObjectFactory factory)
		{
			var contact = org.Contacts.Cast<OrgContact>().FirstOrDefault(x => x.OC_ContactName == contactInfo.BillingContactName.TrimEnd());
			if (contact == null)
			{
				contact = factory.New<OrgContact>();

				contact.OC_OH = org.PK;
				contact.OC_ContactName = contactInfo.BillingContactName;
				contact.OC_ContactSource = ContactSource;
			}

			contact.OC_Email = contactInfo.BillingContactEmail;

			SetContactDocumentGroups(contact);

			return contact;
		}

		static void SetContactDocumentGroups(OrgContact contact)
		{
			//Set Contact as Receivables Contact
			var arDocumentGroup = contact.Documents.AddNew();
			arDocumentGroup.OD_DocumentGroup = ContactType.Receivables.ToString();
			arDocumentGroup.OD_DefaultContact = true;

			//Set Contact as Default BorderWise admin
			var borDocumentGroup = contact.Documents.AddNew();
			borDocumentGroup.OD_DocumentGroup = EDIOrgDocumentGroupTypes.Codes.BorderWiseAdministrator;
			borDocumentGroup.OD_DefaultContact = true;
		}

		const string ContactSource = "BorderWise Reg";

		const string DummyOrgName = "FF0D4E6B-8BAB-47D3-B27D-FBDEF44428CC";

		#endregion

		BusinessObjectFactory SetupEnvironmentAndGetFactory()
		{
			if (Env.CurrentCompany == null)
			{
				WebAppEnvironment.Setup();
			}

			return new BusinessObjectFactory { NameForDebugging = nameof(BorderWiseRegistrationController), RefreshEnabled = true };
		}

		#endregion

		#region Password Email

		const string StudentWarning = @"
You’re receiving this email because you identified yourself as a student.<br /><br />

As per our Terms and Conditions you are required to provide evidence of your student status. Once you have confirmed you are a student, you can have the privilege of using BorderWise for free.
Please Upload an official document via eRequest / Support Request as soon as possible so we can verify that you’re a valid student.<br /><br />
Your document must:<br /><br />

✅ Be issued by a Registered Training Organisation (RTO)<br />
✅ Show your full name<br />
✅ Show the “Diploma of Customs Broking” as your choice of course<br />
✅ Be valid for your current semester (please provide payment confirmation).<br /><br />

Raise an eRequest / Support Request via the <b>More</b> button in BorderWise:<br />
Product: <b>BOR</b><br />
Criticality: <b>CR9</b><br />
Module/Service: <b>STU | Student License Verification</b><br /><br />

If no documents are provided your license will be billed during next billing cycle.
If you would like to continue using BorderWise as a paid user, you do not need to provide any additional document.<br /><br />
<hr />";
		#endregion
	}
}
