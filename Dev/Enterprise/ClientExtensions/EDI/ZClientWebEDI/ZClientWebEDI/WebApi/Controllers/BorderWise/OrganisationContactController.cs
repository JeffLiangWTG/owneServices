using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using BorderWise.Sync;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using NLog;

namespace Enterprise.ZClientWebCargoWiseEDI.BorderWise
{
	[RoutePrefix("api/BorderWiseContacts")]
	public class OrganisationContactController : BorderWiseApiBaseController
	{
		public OrganisationContactController() : base() { }

		public OrganisationContactController(NLogWrapper logger) : base(logger) { }

		[HttpPost]
		[Route("{contactPK}")]
		public IHttpActionResult GetContact(Guid contactPK, [FromBody] string apiKey)
		{
			if (!IsValidApiKey(apiKey))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			using (Db.DisposableActionForDbConnection())
			{
				var contact = DataFactory.Load<EDIOrgContact>(contactPK);

				if (contact == null)
				{
					return StatusCode(HttpStatusCode.NotFound);
				}
				else
				{
					var result = CreateDataObject(contact);

					return Ok(result);
				}
			}
		}

		[Route("resetPasswordUrl")]
		[HttpPost]
		public IHttpActionResult GetResetPasswordUrl([FromBody] ResetPasswordUrlRequest resetPasswordUrlRequest)
		{
			if (!IsValidApiKey(resetPasswordUrlRequest.ApiKey))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			if (resetPasswordUrlRequest.ContactPks.IsNullOrEmpty() || resetPasswordUrlRequest.ContactPks.Any(p => p == Guid.Empty))
			{
				return BadRequest("Invalid argument.");
			}

			var resetPasswordUrls = new List<KeyValuePair<Guid, string>>();
			var notFoundContacts = new List<Guid>();

			using (Db.DisposableActionForDbConnection())
			{
				foreach (var contactPk in resetPasswordUrlRequest.ContactPks)
				{
					var contact = DataFactory.Load<OrgContact>(contactPk);

					if (contact == null)
					{
						notFoundContacts.Add(contactPk);
						continue;
					}
					resetPasswordUrls.Add(new KeyValuePair<Guid, string>(contactPk, BorderWiseUtilities.GenerateResetPasswordUrl(contact)));
				}
			}

			if (notFoundContacts.Any())
			{
				return Content(HttpStatusCode.NotFound,
					$"Could not find contact(s) for following Pk(s): {string.Join(";", notFoundContacts)}");
			}

			return Ok(resetPasswordUrls);
		}

		[HttpPut]
		[Route("{contactPK}/deactivate-student")]
		public IHttpActionResult DeactivateStudentCertificate(Guid contactPk, [FromBody] string apiKey)
		{
			if (!IsValidApiKey(apiKey))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			using (Db.DisposableActionForDbConnection())
			{
				var contact = DataFactory.Load<EDIOrgContact>(contactPk);

				if (contact == null)
				{
					return Content(HttpStatusCode.NotFound, $"Could not find contact for Pk: {contactPk}");
				}

				var activeStudentCerts = contact.Certificates.Where(c =>
					StudentCerificateHelper.IsStudentCertificate(c.XZ_Comment) &&
					(c.XZ_ExpiryOrDueDate == ZDateTime.Empty || c.XZ_ExpiryOrDueDate > ZDateTime.UtcNow)).ToArray();

				activeStudentCerts.ForEach(c => c.XZ_ExpiryOrDueDate = ZDateTime.UtcNow);
				DataFactory.Save();
				return Ok();
			}
		}

		[HttpPut]
		[Route("{contactPK}/nudge-contact")]
		public IHttpActionResult AutoLoginNudgeContactByPK(Guid contactPK, [FromBody] string apiKey)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = $"{contactPK}/nudge-contact";

			var infoMsg = $"Starting the nudge contact process. PK - {contactPK}";
			Logger.AddLog(LogLevel.Info, infoMsg, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
			var result = ProcessContact(contactPK, apiKey, EnableWebAccessOrNudge, routingPath, sessionId);
			infoMsg = $"Completed the nudge contact process. PK - {contactPK}";
			Logger.AddLog(LogLevel.Info, infoMsg, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
			return result;
		}

		[HttpPut]
		[Route("{contactPK}/enable-access")]
		public IHttpActionResult ActivateUserAndEnableWebAccess(Guid contactPK, [FromBody] string apiKey)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = $"{contactPK}/enable-access";

			var infoMsg = $"Starting the ActivateUserAndEnableWebAccess process. PK - {contactPK}";
			Logger.AddLog(LogLevel.Info, infoMsg, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
			var result = ProcessContact(contactPK, apiKey, EnableWebAccessAndActivateUser, routingPath, sessionId);
			infoMsg = $"Completed the ActivateUserAndEnableWebAccess process. PK - {contactPK}";
			Logger.AddLog(LogLevel.Info, infoMsg, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
			return result;
		}

		[HttpPut]
		[Route("nudge-contact")]
		public IHttpActionResult AutoLoginNudgeContact([FromBody] NudgeContactRequest nudgeContactRequest)
		{
			if (!IsValidApiKey(nudgeContactRequest.ApiKey))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			if (string.IsNullOrWhiteSpace(nudgeContactRequest.Email))
			{
				return StatusCode(HttpStatusCode.BadRequest);
			}

			using (Db.DisposableActionForDbConnection())
			{
				var contactList = DataFactory.Load<OrgContact>(new ZQuery(OrgContactSchema.OC_Email, nudgeContactRequest.Email));

				if (!contactList.Any())
				{
					return Content(HttpStatusCode.NotFound, $"Could not find contact for email: {nudgeContactRequest.Email}");
				}

				var contact = GetOrgContact(contactList);

				EnableWebAccessOrNudge(contact);

				return Ok($"OrgContact with PK: {contact.PK}, ContactName: ({contact.OC_ContactName}) has been updated.");
			}

			OrgContact GetOrgContact(OrgContact[] contacts)
			{
				var contact = contacts.FirstOrDefault(c => c.OC_IsPrimaryContact) ?? contacts.OrderByDescending(c => c.OC_WebAccessEnabled).ThenByDescending(c => c.OC_DetailsVerified).First();

				return contact;
			}
		}

		[HttpPut]
		[Route("sync-license-status")]
		public IHttpActionResult SyncBorderWiseLicenseAndEdiCustomerUserAccount([FromBody] SyncBorderWiseUsersStatusRequest syncBorderWiseUsersStatusRequest)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = $"sync-license-status";

			if (!IsValidApiKey(syncBorderWiseUsersStatusRequest.ApiKey))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			if (syncBorderWiseUsersStatusRequest.BorderWiseUsers == null || syncBorderWiseUsersStatusRequest.BorderWiseUsers.Count == 0)
			{
				var message = "No Borderwise users data found in request.";
				Logger.AddLog(LogLevel.Info, message, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);

				return Content(HttpStatusCode.BadRequest, message);
			}

			var infoMsg = $"Starting the SyncBorderWiseLicenseAndEdiCustomerUserAccount process.";
			Logger.AddLog(LogLevel.Info, infoMsg, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);

			var processedUsers = new List<SyncBorderWiseUserResult>();

			foreach (var user in syncBorderWiseUsersStatusRequest.BorderWiseUsers)
			{
				var syncBorderWiseUserResult = new SyncBorderWiseUserResult()
				{
					User = user,
					ErrorMessage = string.Empty
				};

				var errorMessage = string.Empty;
				var validationResult = ValidateBorderWiseUserDetails(user, out errorMessage);

				if (!validationResult)
				{
					syncBorderWiseUserResult.ErrorMessage = errorMessage;
					processedUsers.Add(syncBorderWiseUserResult);

					Logger.AddLog(LogLevel.Warn, $"Skipped processing due to invalid Borderwise user details. ErrorMessage: {errorMessage}, UserId: {user.ContactPk}", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
					continue;
				}

				try
				{
					var licenceDatabaseQuery = new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, user.LicenseDatabaseNumber);
					licenceDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_IsActive, true);

					var licenceDatabase = DataFactory.LoadTop1<LicenceDatabase>(licenceDatabaseQuery);
					if (licenceDatabase == null)
					{
						errorMessage = $"No active licence database found for database number {user.LicenseDatabaseNumber}. UserId: {user.ContactPk}";
						syncBorderWiseUserResult.ErrorMessage = errorMessage;
						processedUsers.Add(syncBorderWiseUserResult);

						Logger.AddLog(LogLevel.Warn, errorMessage, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
						continue;
					}

					var userAccountQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, user.ContactPk.ToString());
					userAccountQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_LD, licenceDatabase.PK);

					var userAccount = DataFactory.LoadTop1<EdiCustomerUserAccount>(userAccountQuery);
					if (userAccount == null)
					{
						userAccount = DataFactory.New<EdiCustomerUserAccount>();
						userAccount.EUA_LD = licenceDatabase.PK;
						userAccount.EUA_UserID = user.ContactPk.ToString();
						userAccount.EUA_Email = user.Email;
						userAccount.EUA_FullName = user.ContactFullName;
						userAccount.EUA_IsActive = user.HasActiveBorderWiseLicense;
						userAccount.EUA_OC_WebAccessContact = user.EdiProdRecordId;

						DataFactory.Save();

						Logger.AddLog(
							LogLevel.Info,
							$"Created new EdiCustomerAccount record. UserId: {user.ContactPk}, Email: {user.Email}, FullName: {user.ContactFullName}, HasActiveBorderWiseLicense: {user.HasActiveBorderWiseLicense}",
							(int)HttpStatusCode.OK,
							sessionId,
							routingPath: routingPath);
					}
					else
					{
						Logger.AddLog(LogLevel.Info, $"Existing EdiCustomerAccount record found for Borderwise user. User Id: {user.ContactPk.ToString()}, DatabaseNumber: {user.LicenseDatabaseNumber}", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);

						userAccount.EUA_IsActive = user.HasActiveBorderWiseLicense;
						userAccount.EUA_Email = user.Email;
						userAccount.EUA_FullName = user.ContactFullName;
						var importer = new WebRequestContactImporter(DataFactory, licenceDatabase);
						importer.ImportFromUserAccount(userAccount);
						importer.SaveIfNeeded();

						Logger.AddLog(LogLevel.Info,
							$"Updated existing EdiCustomerAccount record. User Id: {user.ContactPk.ToString()}," +
							$" HasActiveBorderWiseLicense: {user.HasActiveBorderWiseLicense}, Email: {user.Email}, ContactFullName: {user.ContactFullName}",
							((int)HttpStatusCode.OK),
							sessionId,
							routingPath: routingPath);
					}
				}
				catch (Exception ex)
				{
					errorMessage = $"Error processing sync for Borderwise user. {user.ContactPk}: {ex.Message}";
					syncBorderWiseUserResult.ErrorMessage = errorMessage;
					processedUsers.Add(syncBorderWiseUserResult);

					Logger.AddLog(LogLevel.Warn, errorMessage, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, ex: ex);
					continue;
				}

				processedUsers.Add(syncBorderWiseUserResult);
			}

			infoMsg = $"Completed the SyncBorderWiseLicenseAndEdiCustomerUserAccount process.";
			Logger.AddLog(LogLevel.Info, infoMsg, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);

			var response = new SyncBorderWiseUsersStatusResponse
			{
				ProcessedBorderWiseUsers = processedUsers
			};
			return Ok(response);
		}

		IHttpActionResult ProcessContact(Guid contactPK, string apiKey, Func<OrgContact, string, string, IHttpActionResult> action, string routingPath = "", string sessionId = "")
		{
			Logger.AddLog(LogLevel.Info, $"Starting the contact processing. PK - {contactPK}", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
			if (!IsValidApiKey(apiKey))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			using (Db.DisposableActionForDbConnection())
			{
				var contact = DataFactory.Load<OrgContact>(contactPK);

				if (contact == null)
				{
					Logger.AddLog(LogLevel.Warn, $"Could not find contact for PK: {contactPK}", ((int)HttpStatusCode.NotFound), sessionId, routingPath: routingPath);
					return Content(HttpStatusCode.NotFound, $"Could not find contact for PK: {contactPK}");
				}

				var result = action(contact, routingPath, sessionId);

				Logger.AddLog(LogLevel.Info, $"Completed the contact processing. PK - {contactPK}", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
				return result;
			}
		}

		IHttpActionResult EnableWebAccessAndActivateUser(OrgContact contact, string routingPath = "", string sessionId = "")
		{
			Logger.AddLog(LogLevel.Info, $"Starting the EnableWebAccessAndActivateUser process. PK - {contact.PK}", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
			try
			{
				if (contact.OC_WebAccessEnabled && contact.OC_IsActive)
				{
					var errorMessage = "EnableWebAccessAndActivateUser - User is active and web access is enabled.";
					Logger.AddLog(LogLevel.Warn, errorMessage, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
					return Ok("EnableWebAccessAndActivateUser - User is active and web access is enabled.");
				}

				if (!contact.OC_WebAccessEnabled)
				{
					contact.OC_WebAccessEnabled = true;
				}

				if (!contact.OC_IsActive)
				{
					contact.OC_IsActive = true;
				}

				BorderWiseUtilities.ChangeSecurityRight(contact, true, DataFactory);

				return Ok($"OrgContact with PK: {contact.PK}, ContactName: ({contact.OC_ContactName}) has been updated.");
			}
			catch (ZSaveException ex) when (ex.Message.Contains(CommonConstants.duplicateEmailForWebEnabledContact))
			{
				var errorMessage = $"Failed to enable web access for the user ( {contact.Name} / {contact.Email} ). Each active contact with web access in this organization must have a unique email address.";
				Logger.AddLog(LogLevel.Info, errorMessage, ((int)HttpStatusCode.Forbidden), sessionId, routingPath: routingPath, ex: ex);
				return Content(HttpStatusCode.Forbidden, errorMessage);
			}
			catch (Exception ex)
			{
				Logger.AddLog(LogLevel.Error, "EnableWebAccessAndActivateUser - Failed to save changes.", ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, ex: ex);
				return Content(HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		IHttpActionResult EnableWebAccessOrNudge(OrgContact contact, string routingPath = "", string sessionId = "")
		{
			Logger.AddLog(LogLevel.Info, $"Starting the EnableWebAccessOrNudge process. PK - {contact.PK}", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
			try
			{
				if (!contact.OC_WebAccessEnabled)
				{
					contact.OC_WebAccessEnabled = true;
					BorderWiseUtilities.ChangeSecurityRight(contact, true, DataFactory);
				}
				else
				{
					var originalPhoneExtension = contact.OC_PhoneExtension;
					contact.OC_PhoneExtension = new ZString(originalPhoneExtension + "1").Left(AutoOrgContact.Schema.OC_PhoneExtensionMaxLength);
					DataFactory.Save();

					contact.OC_PhoneExtension = originalPhoneExtension;
					DataFactory.Save();
				}

				return Ok($"OrgContact with PK: {contact.PK}, ContactName: ({contact.OC_ContactName}) has been updated.");
			}
			catch (ZSaveException ex) when (ex.Message.Contains(CommonConstants.duplicateEmailForWebEnabledContact))
			{
				var errorMessage = $"Failed to enable web access for the user ( {contact.Name} / {contact.Email} ). Each active contact with web access in this organization must have a unique email address.";
				Logger.AddLog(LogLevel.Info, errorMessage, ((int)HttpStatusCode.Forbidden), sessionId, routingPath: routingPath, ex: ex);
				return Content(HttpStatusCode.Forbidden, errorMessage);
			}
			catch (Exception ex)
			{
				Logger.AddLog(LogLevel.Error, "EnableWebAccessOrNudge - Failed to save changes.", ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, ex: ex);
				return Content(HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		bool ValidateBorderWiseUserDetails(BorderWiseUser user, out string errorMessage)
		{
			errorMessage = string.Empty;
			if (string.IsNullOrEmpty(user.Email) || user.ContactPk == default || user.EdiProdRecordId == default || user.LicenseDatabaseNumber <= 0)
			{
				errorMessage = $"Invalid Borderwise user details. Email: {user.Email}, ContactPk: {user.ContactPk}, EdiProdRecordId: {user.EdiProdRecordId}, LicenseDatabaseNumber: {user.LicenseDatabaseNumber}";
				Logger.AddLog(LogLevel.Warn, errorMessage, ((int)HttpStatusCode.OK));
				return false;
			}

			return true;
		}

		static OrgContactDataObjectWithBorderWiseInfo CreateDataObject(EDIOrgContact contact)
		{
			var birthDay = contact.OC_Birthday.IsValid ? contact.OC_Birthday.ToDateTime() : default;
			birthDay = DateTime.SpecifyKind(birthDay, DateTimeKind.Unspecified);
			var contactDataObject = new OrgContactDataObjectWithBorderWiseInfo
			{
				PK = contact.PK.ToGuid(),
				ContactName = contact.OC_ContactName,
				Title = contact.OC_Title,
				IsActive = contact.OC_IsActive,
				Language = contact.OC_Language,
				Phone = contact.OC_Phone,
				PhoneExtension = contact.OC_PhoneExtension,
				Mobile = contact.OC_Mobile,
				Email = contact.OC_Email,
				Birthday = birthDay,
				OrgFk = contact.OC_OH.ToGuid(),
				PasswordHash = contact.Person.HasPassword ? contact.Person.PER_PasswordHash : contact.OC_PasswordHash,
				PasswordSalt = contact.Person.HasPassword ? contact.Person.PER_PasswordSalt : contact.OC_PasswordSalt,
				PasswordHashIterations = contact.Person.HasPassword ? contact.Person.PER_PasswordHashIterations : contact.OC_PasswordHashIterations,
				WebAccessEnabled = contact.OC_WebAccessEnabled,
				Nationality = contact.OC_RN_NKNationality,
				Gender = contact.OC_Gender,
				SecurityRightGranted = HasBorderWiseAccess(contact),
				BorderWiseRole = GetBorderWiseRole(contact),
			};

			foreach (var certificate in contact.Certificates)
			{
				var isStudentCert = StudentCerificateHelper.IsStudentCertificate(certificate.XZ_Comment);
				var comment = isStudentCert ? StudentCerificateHelper.GetCommentsWithoutStudentCertificateToken(certificate.XZ_Comment) : certificate.XZ_Comment.ToString();

				contactDataObject.OrgContactCertificates.Add(new OrgContactCertificateDataObject
				{
					PK = certificate.PK.ToGuid(),
					ContactFk = certificate.XZ_ParentID.ToGuid(),
					Comment = comment,
					ExpiryOrDueDateUtc = certificate.XZ_ExpiryOrDueDate.IsValid ? certificate.XZ_ExpiryOrDueDate.ToDateTime() : default,
					IssueDateUtc = certificate.XZ_IssueDate.IsValid ? certificate.XZ_IssueDate.ToDateTime() : default,
					RefNumber = certificate.XZ_RefNumber,
					Type = isStudentCert ? "STU" : certificate.XZ_Type.ToString(),
					IsValid = isStudentCert,
					ValidatedDateUtc = isStudentCert ? ZDateTime.UtcNow.ToDateTime() : default
				});
			}

			return contactDataObject;
		}

		static bool HasBorderWiseAccess(EDIOrgContact contact)
		{
			return contact.OC_WebAccessEnabled && OrgContactWebUser.IsRightGrantedWithoutCache(EDIWebSecurityRightsList.BorderWise, contact);
		}

		static int GetBorderWiseRole(EDIOrgContact contact)
		{
			var isAdmin = contact.Documents.Cast<OrgDocument>().Any(d => d.OD_DocumentGroup == EDIOrgDocumentGroupTypes.Codes.BorderWiseAdministrator || d.OD_DocumentGroup == ContactType.Receivables.Code);

			return isAdmin ? 2 : 1;
		}
	}

	public class ContactControllerRequest
	{
		public string ApiKey { get; set; }
	}

	public class ResetPasswordUrlRequest : ContactControllerRequest
	{
		public IEnumerable<Guid> ContactPks { get; set; }
	}

	public class NudgeContactRequest : ContactControllerRequest
	{
		public string Email { get; set; }
	}

	public class SyncBorderWiseUsersStatusRequest : ContactControllerRequest
	{
		public List<BorderWiseUser> BorderWiseUsers { get; set; }
	}

	public class BorderWiseUser
	{
		public Guid ContactPk { get; set; }

		public Guid EdiProdRecordId { get; set; }

		public int LicenseDatabaseNumber { get; set; }

		public string ContactFullName { get; set; }

		public string Email { get; set; }

		public bool HasActiveBorderWiseLicense { get; set; }
	}

	public class SyncBorderWiseUsersStatusResponse
	{
		public List<SyncBorderWiseUserResult> ProcessedBorderWiseUsers { get; set; }
	}

	public class SyncBorderWiseUserResult
	{
		public BorderWiseUser User { get; set; }

		public string ErrorMessage { get; set; }
	}
}
