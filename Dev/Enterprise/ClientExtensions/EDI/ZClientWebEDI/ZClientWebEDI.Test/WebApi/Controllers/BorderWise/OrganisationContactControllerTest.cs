using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using BorderWise.Sync;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZClientWebCargoWiseEDI.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.BorderWise.Testing
{
	public class OrganisationContactControllerTest : TestCaseWithFactory
	{
		#region GetContact
		public void TestGetContact_WithWrongApiKey_ShouldBeForbidden()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact = (EDIOrgContact)org.Contacts.AddNew();
			Factory.Save();
			var controller = new OrganisationContactController();
			AssertStatusCode(HttpStatusCode.Forbidden, controller.GetContact(Guid.NewGuid(), "Mah Key"));
			AssertStatusCode(HttpStatusCode.NotFound, controller.GetContact(Guid.NewGuid(), BorderWiseApiBaseController.ApiKey));
			var result = controller.GetContact(contact.PK.ToGuid(), BorderWiseApiBaseController.ApiKey);
			AssertType<OkNegotiatedContentResult<OrgContactDataObjectWithBorderWiseInfo>>(result);
		}

		[TestDate(2020, 6, 14)]
		public void TestGetContact_ShouldFindCorrectContact()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact1 = (EDIOrgContact)org.Contacts.AddNew();
			var contact2 = (EDIOrgContact)org.Contacts.AddNew();
			var contact3 = (EDIOrgContact)org.Contacts.AddNew();
			SetAttributes(contact1, true, "Contact 1", "en-AU", "Mister", "AU", "O", "000", "69", "111", ZDateTime.BrettsBirthday.ToDateTime(), "Ned@FlancrestEnterprises.net", new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 }, 69);
			SetAttributes(contact2, false, "Contact 2", "en-UK", "Master", "UK", "F", "01189998819991197253", "70", "222", new DateTime(2015, 7, 14), "ChunkyLover53@aol.com", new byte[] { 2, 3, 4 }, new byte[] { 5, 6, 7 }, 70);
			SetAttributes(contact3, false, "Contact 3", "en-US", "Messir", "US", "N", "911", "71", "333", new DateTime(2019, 1, 1), "HoJu@aol.com", new byte[] { 3, 4, 5 }, new byte[] { 6, 7, 8 }, 71);
			SetBorderWiseAccess(contact1, true);
			SetBorderWiseAccess(contact2, false);
			SetRole(contact1, "BOR");
			SetRole(contact2, "A/R");
			SetRole(contact3, "NOP");
			CreateCertificate(contact1, "Student - Harry Potter", new DateTime(2019, 1, 1), new DateTime(2015, 7, 14), "Boop", "TYP");
			CreateCertificate(contact1, "Stupent - 'Arry Po'err", new DateTime(2019, 1, 2), new DateTime(2015, 7, 15), "Hrgh", "TYP");
			Factory.Save();
			var controller = new OrganisationContactController();
			var contactInfo1 = GetContactInfo(controller, contact1);
			var contactInfo2 = GetContactInfo(controller, contact2);
			var contactInfo3 = GetContactInfo(controller, contact3);
			AssertContactDetails(contactInfo1, true, "Contact 1", "en-AU", "Mister", "AU", "O", "000", "69", "111", ZDateTime.BrettsBirthday.ToDateTime(), "Ned@FlancrestEnterprises.net", new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 }, 69, true, 2);
			AssertContactDetails(contactInfo2, false, "Contact 2", "en-UK", "Master", "UK", "F", "01189998819991197253", "70", "222", new DateTime(2015, 7, 14), "ChunkyLover53@aol.com", new byte[] { 2, 3, 4 }, new byte[] { 5, 6, 7 }, 70, false, 2);
			AssertContactDetails(contactInfo3, false, "Contact 3", "en-US", "Messir", "US", "N", "911", "71", "333", new DateTime(2019, 1, 1), "HoJu@aol.com", new byte[] { 3, 4, 5 }, new byte[] { 6, 7, 8 }, 71, false, 1);
			AssertEquals(2, contactInfo1.OrgContactCertificates.Count);
			AssertEquals(0, contactInfo2.OrgContactCertificates.Count);
			AssertEquals(0, contactInfo3.OrgContactCertificates.Count);
			var cert1 = contactInfo1.OrgContactCertificates.Single(c => c.Comment == "Harry Potter");
			var cert2 = contactInfo1.OrgContactCertificates.Single(c => c.Comment == "Stupent - 'Arry Po'err");
			AssertCertificateDetails(cert1, new DateTime(2019, 1, 1), new DateTime(2015, 7, 14), "Boop", "STU");
			AssertCertificateDetails(cert2, new DateTime(2019, 1, 2), new DateTime(2015, 7, 15), "Hrgh", "TYP");
		}

		public void TestGetContact_ShouldGetPasswordHashFromPerson()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact1 = (EDIOrgContact)org.Contacts.AddNew();
			var passwordHash = new byte[] { 1, 2, 3 };
			var passwordSalt = new byte[] { 4, 5, 6 };
			var passwordHashIterations = 2000;
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Contact 1";
			person.PER_RN_NKCountry = "AU";
			person.PER_NameSuffix = "sfx";
			person.PER_NameTitle = "title";
			person.PER_Gender = "M";
			person.PER_HomeAddress1 = "address 1";
			person.PER_HomeAddress2 = "address 2";
			person.PER_City = "city";
			person.PER_Postcode = "12345";
			person.PER_State = "state";
			person.PER_RN_NKCountry = "AU";
			person.PER_MobilePhone = "123456789";
			person.PER_HomePhone = "987654321";
			person.PER_FaxNumber = "111";
			person.PER_BirthDate = ZDateTime.BrettsBirthday.Date;
			person.PER_FriendlyName = "friendly";
			person.PER_RN_NKNationalityCodeISO = "AU";
			person.PER_PreferredLanguage = "RSN";
			person.PER_DriversLicenseNumber = "drivers";
			person.PER_EmailAddress = "email@contact.com";
			person.PER_EmailAddress2 = "email@contact.com";
			person.PER_MobilePhone2 = "123456789";
			person.PER_Passport = "passport";
			person.PER_PasswordHash = passwordHash;
			person.PER_PasswordSalt = passwordSalt;
			person.PER_PasswordHashIterations = passwordHashIterations;
			contact1.OC_PER = person.PK;
			Factory.Save();
			var controller = new OrganisationContactController();
			var contactInfo = GetContactInfo(controller, contact1);
			AssertSequencesEqual("PasswordHash", passwordHash, contactInfo.PasswordHash);
			AssertSequencesEqual("PasswordSalt", passwordSalt, contactInfo.PasswordSalt);
			AssertEquals("PasswordHashIterations", passwordHashIterations, contactInfo.PasswordHashIterations);
		}

		OrgContactDataObjectWithBorderWiseInfo GetContactInfo(OrganisationContactController controller, OrgContact contact)
		{
			var result = controller.GetContact(contact.PK.ToGuid(), BorderWiseApiBaseController.ApiKey);
			return ((OkNegotiatedContentResult<OrgContactDataObjectWithBorderWiseInfo>)result).Content;
		}

		#region Object Creation
		static void SetAttributes(OrgContact contact, bool isActive, string contactName, string language, string title, string nationality, string gender, string phone, string phoneExtension, string mobile, DateTime birthday, string email, byte[] passwordHash, byte[] passwordSalt, int passwordIterations)
		{
			contact.OC_WebAccessEnabled = true;
			contact.OC_IsActive = isActive;
			contact.OC_ContactName = contactName;
			contact.OC_Language = language;
			contact.OC_Title = title;
			contact.OC_RN_NKNationality = nationality;
			contact.OC_Gender = gender;
			contact.OC_Phone = phone;
			contact.OC_PhoneExtension = phoneExtension;
			contact.OC_Mobile = mobile;
			contact.OC_Birthday = birthday;
			contact.OC_Email = email;
			contact.OC_PasswordHash = passwordHash;
			contact.OC_PasswordSalt = passwordSalt;
			contact.OC_PasswordHashIterations = passwordIterations;
		}

		static void SetBorderWiseAccess(EDIOrgContact contact, bool hasAccess)
		{
			BorderWiseUtilities.ChangeSecurityRight(contact, hasAccess, contact.Factory);
		}

		static void SetRole(OrgContact contact, string role)
		{
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = role;
		}

		static void CreateCertificate(OrgContact contact, string comment, DateTime expiryDate, DateTime issueDate, string reference, string type)
		{
			var cert = contact.Certificates.AddNew();
			cert.XZ_Comment = comment;
			cert.XZ_ExpiryOrDueDate = expiryDate;
			cert.XZ_IssueDate = issueDate;
			cert.XZ_RefNumber = reference;
			cert.XZ_Type = type;
		}

		#endregion
		#region Assertions
		static void AssertStatusCode(HttpStatusCode expectedStatusCode, IHttpActionResult actionResult)
		{
			AssertType<StatusCodeResult>(actionResult);
			AssertEquals(expectedStatusCode, ((StatusCodeResult)actionResult).StatusCode);
		}

		static void AssertContactDetails(OrgContactDataObjectWithBorderWiseInfo contactInfo, bool isActive, string contactName, string language, string title, string nationality, string gender, string phone, string phoneExtension, string mobile, DateTime birthday, string email, byte[] passwordHash, byte[] passwordSalt, int passwordIterations, bool hasAccess, int role)
		{
			CombineAssertions(() =>
			{
				AssertEquals("IsActive", isActive, contactInfo.IsActive);
				AssertEquals("ContactName", contactName, contactInfo.ContactName);
				AssertEquals("Language", language, contactInfo.Language);
				AssertEquals("Title", title, contactInfo.Title);
				AssertEquals("Nationality", nationality, contactInfo.Nationality);
				AssertEquals("Gender", gender, contactInfo.Gender);
				AssertEquals("Phone", phone, contactInfo.Phone);
				AssertEquals("PhoneExtension", phoneExtension, contactInfo.PhoneExtension);
				AssertEquals("Mobile", mobile, contactInfo.Mobile);
				AssertEquals("Birthday", birthday, contactInfo.Birthday);
				AssertEquals("Birthday Kind", DateTimeKind.Unspecified, contactInfo.Birthday.Kind);
				AssertEquals("Email", email, contactInfo.Email);
				AssertSequencesEqual("PasswordHash", passwordHash, contactInfo.PasswordHash);
				AssertSequencesEqual("PasswordSalt", passwordSalt, contactInfo.PasswordSalt);
				AssertEquals("PasswordHashIterations", passwordIterations, contactInfo.PasswordHashIterations);
				AssertEquals("SecurityRightGranted", hasAccess, contactInfo.SecurityRightGranted);
				AssertEquals("BorderWiseRole", role, contactInfo.BorderWiseRole);
			});
		}

		static void AssertCertificateDetails(OrgContactCertificateDataObject cert, DateTime expiryDate, DateTime issueDate, string reference, string type)
		{
			CombineAssertions(() =>
			{
				AssertEquals("ExpiryOrDueDateUtc", expiryDate, cert.ExpiryOrDueDateUtc);
				AssertEquals("IssueDateUtc", issueDate, cert.IssueDateUtc);
				AssertEquals("RefNumber", reference, cert.RefNumber);
				AssertEquals("Type", type, cert.Type);
			});
		}

		#endregion
		#endregion
		#region GetResetPasswordUrls
		public void TestGetResetPasswordUrl_Success()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact1 = (EDIOrgContact)org.Contacts.AddNew();
			contact1.OC_ContactName = "contact1";
			var contact2 = (EDIOrgContact)org.Contacts.AddNew();
			contact2.OC_ContactName = "contact2";
			Factory.Save();
			var request = GetResetPasswordUrlRequest();
			request.ContactPks = new List<Guid>()
			{ contact1.PK.ToGuid(), contact2.PK.ToGuid() };
			var controller = new OrganisationContactController();
			//Action
			var result = (OkNegotiatedContentResult<List<KeyValuePair<Guid, string>>>)controller.GetResetPasswordUrl(request);
			//Assert
			AssertEquals(2, result.Content.Count);
			var contactResetPasswordUrlInfo = result.Content.SingleOrDefault(p => p.Key == contact1.PK.ToGuid());
			AssertNotNull(contactResetPasswordUrlInfo);
			AssertNotNullOrEmpty(contactResetPasswordUrlInfo.Value);
			contactResetPasswordUrlInfo = result.Content.SingleOrDefault(p => p.Key == contact2.PK.ToGuid());
			AssertNotNull(contactResetPasswordUrlInfo);
			AssertNotNullOrEmpty(contactResetPasswordUrlInfo.Value);
		}

		public void TestGetResetPasswordUrl_SuccessHasPersonPassword()
		{
			var relatedContact1 = Factory.NewWithValidTestData<OrgContact>();
			var relatedContact2 = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact1 = (EDIOrgContact)org.Contacts.AddNew();
			contact1.OC_ContactName = "contact1";
			contact1.OC_Email = "a@1.com";
			contact1.OC_PER = relatedContact1.OC_PER;
			var contact2 = (EDIOrgContact)org.Contacts.AddNew();
			contact2.OC_ContactName = "contact2";
			contact2.OC_Email = "b@2.com";
			contact2.OC_PER = relatedContact2.OC_PER;
			Factory.Save();
			contact1.Person.SetHashedPassword("1234");
			Factory.Save();
			var request = GetResetPasswordUrlRequest();
			request.ContactPks = new List<Guid>()
			{ contact1.PK.ToGuid(), contact2.PK.ToGuid() };
			var controller = new OrganisationContactController();
			//Action
			var result = (OkNegotiatedContentResult<List<KeyValuePair<Guid, string>>>)controller.GetResetPasswordUrl(request);
			//Assert
			AssertEquals(2, result.Content.Count);
			var contactResetPasswordUrlInfo = result.Content.SingleOrDefault(p => p.Key == contact1.PK.ToGuid());
			AssertNotNull(contactResetPasswordUrlInfo);
			AssertNotNullOrEmpty(contactResetPasswordUrlInfo.Value);
			var personTokenQuery = new ZQuery(StmAccessTokenSchema.SAT_Scope, SQLComparisonOperator.Contains, contact1.OC_Email);
			personTokenQuery.AddToFilter(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.ResetMasterPassword);
			AssertEquals("Should create person password token", true, Factory.ExistsInDatabase(StmAccessTokenSchema.Constants.TableName, personTokenQuery));
			contactResetPasswordUrlInfo = result.Content.SingleOrDefault(p => p.Key == contact2.PK.ToGuid());
			AssertNotNull(contactResetPasswordUrlInfo);
			AssertNotNullOrEmpty(contactResetPasswordUrlInfo.Value);
			var contactTokenQuery = new ZQuery(StmAccessTokenSchema.SAT_Scope, SQLComparisonOperator.Contains, contact2.OC_Email);
			contactTokenQuery.AddToFilter(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.ResetPassword);
			AssertEquals("Should create person password token", true, Factory.ExistsInDatabase(StmAccessTokenSchema.Constants.TableName, contactTokenQuery));
		}

		public void TestGetResetPasswordUrl_Error_InvalidArgument()
		{
			var request = GetResetPasswordUrlRequest();
			var controller = new OrganisationContactController();
			//Action and Assert
			AssertType<BadRequestErrorMessageResult>(controller.GetResetPasswordUrl(request));
			request.ContactPks = new List<Guid>()
			{ Guid.Empty };
			AssertType<BadRequestErrorMessageResult>(controller.GetResetPasswordUrl(request));
		}

		public void TestGetResetPasswordUrl_Error_InvalidApiKey()
		{
			var request = GetResetPasswordUrlRequest(false);
			var controller = new OrganisationContactController();
			//Action and Assert
			AssertStatusCode(HttpStatusCode.Forbidden, controller.GetResetPasswordUrl(request));
		}

		public void TestGetResetPasswordUrl_Error_ContactNotFound()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact = (EDIOrgContact)org.Contacts.AddNew();
			Factory.Save();
			var request = GetResetPasswordUrlRequest();
			var nonExistContactPk = Guid.NewGuid();
			request.ContactPks = new List<Guid>()
			{ nonExistContactPk, contact.PK.ToGuid() };
			var controller = new OrganisationContactController();
			//Action
			var result = (NegotiatedContentResult<string>)controller.GetResetPasswordUrl(request);
			//Assert
			AssertEquals(HttpStatusCode.NotFound, result.StatusCode);
			AssertEquals($"Could not find contact(s) for following Pk(s): {nonExistContactPk}", result.Content);
		}

		static ResetPasswordUrlRequest GetResetPasswordUrlRequest(bool validApiKey = true)
		{
			return new ResetPasswordUrlRequest()
			{ ApiKey = validApiKey ? BorderWiseApiBaseController.ApiKey : string.Empty };
		}

		#endregion
		#region DeactivateStudentCertificate
		public void TestDeactivateStudentCertificate_Error_InvalidApiKey()
		{
			var controller = new OrganisationContactController();
			//Action and Assert
			AssertStatusCode(HttpStatusCode.Forbidden, controller.DeactivateStudentCertificate(Guid.NewGuid(), "Mah Key"));
		}

		public void TestDeactivateStudentCertificate_Error_ContactNotFound()
		{
			var controller = new OrganisationContactController();
			var contactPk = Guid.NewGuid();
			//Action
			var result = (NegotiatedContentResult<string>)controller.DeactivateStudentCertificate(contactPk, BorderWiseApiBaseController.ApiKey);
			//Assert
			AssertEquals(HttpStatusCode.NotFound, result.StatusCode);
			AssertEquals($"Could not find contact for Pk: {contactPk}", result.Content);
		}

		public void TestDeactivateStudentCertificate_Success_CertificateNotFound()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact = (EDIOrgContact)org.Contacts.AddNew();
			Factory.Save();
			var controller = new OrganisationContactController();
			//Action
			var result = controller.DeactivateStudentCertificate(contact.PK.ToGuid(), BorderWiseApiBaseController.ApiKey);
			//Assert
			AssertEquals(typeof(OkResult), result.GetType());
		}

		[TestDate(2022, 01, 01)]
		public void TestDeactivateStudentCertificate_Success()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact = (EDIOrgContact)org.Contacts.AddNew();
			var cert = contact.Certificates.AddNew();
			cert.XZ_Comment = "Student - UTS";
			cert.XZ_RefNumber = "1212";
			cert.XZ_Type = "MSC";
			Factory.Save();
			AssertEquals("Precondition", ZDateTime.Empty, cert.XZ_ExpiryOrDueDate);
			var controller = new OrganisationContactController();
			//Action
			var result = controller.DeactivateStudentCertificate(contact.PK.ToGuid(), BorderWiseApiBaseController.ApiKey);
			TestDateAttribute.AddDays(2);
			//Assert
			AssertEquals(typeof(OkResult), result.GetType());
			AssertEquals(new ZDateTime(2022, 01, 01), cert.XZ_ExpiryOrDueDate);
		}

		#endregion
		#region AutoLoginNudgeContact
		public void TestAutoLoginNudgeContact_Error_InvalidApiKey()
		{
			var request = GetNudgeContactRequest(false);
			var controller = new OrganisationContactController();
			//Action
			var result = (StatusCodeResult)controller.AutoLoginNudgeContact(request);
			//Assert
			AssertEquals(HttpStatusCode.Forbidden, result.StatusCode);
		}

		public void TestAutoLoginNudgeContact_Error_InvalidEmail()
		{
			var request = GetNudgeContactRequest(email: string.Empty);
			var controller = new OrganisationContactController();
			//Action
			var result = (StatusCodeResult)controller.AutoLoginNudgeContact(request);
			//Assert
			AssertEquals(HttpStatusCode.BadRequest, result.StatusCode);
		}

		public void TestAutoLoginNudgeContact_ShouldReturnNotFound_WhenContactNotFound()
		{
			var request = GetNudgeContactRequest();
			var controller = new OrganisationContactController();
			//Action
			var result = (NegotiatedContentResult<string>)controller.AutoLoginNudgeContact(request);
			//Assert
			AssertEquals(HttpStatusCode.NotFound, result.StatusCode);
			AssertEquals($"Could not find contact for email: {request.Email}", result.Content);
		}

		public void TestAutoLoginNudgeContact_ShouldUpdate_WhenContactIsPrimary()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact1 = (EDIOrgContact)org.Contacts.AddNew();
			contact1.OC_ContactName = "test1";
			contact1.OC_Email = "test@email.com";
			contact1.OC_WebAccessEnabled = false;
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact2 = (EDIOrgContact)org2.Contacts.AddNew();
			contact2.OC_ContactName = "test2";
			contact2.OC_Email = "test@email.com";
			contact2.OC_WebAccessEnabled = false;
			Factory.Save();
			contact2.Person.RemovePrimaryRelationship();
			Factory.Save();
			var controller = new OrganisationContactController();
			var request = GetNudgeContactRequest();
			//Action
			var result = (OkNegotiatedContentResult<string>)controller.AutoLoginNudgeContact(request);
			//Assert
			AssertEquals($"OrgContact with PK: {contact1.PK}, ContactName: ({contact1.OC_ContactName}) has been updated.", result.Content);
			AssertEquals(true, contact1.OC_WebAccessEnabled);
			AssertEquals(false, contact2.OC_WebAccessEnabled);
		}

		public void TestAutoLoginNudgeContact_ShouldUpdate_WhenContactIsWebAccessEnabled()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact1 = (EDIOrgContact)org.Contacts.AddNew();
			contact1.OC_ContactName = "test1";
			contact1.OC_Email = "test@email.com";
			contact1.OC_WebAccessEnabled = false;
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact2 = (EDIOrgContact)org2.Contacts.AddNew();
			contact2.OC_ContactName = "test2";
			contact2.OC_Email = "test@email.com";
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_PhoneExtension = "2";
			Factory.Save();
			contact1.Person.RemovePrimaryRelationship();
			contact2.Person.RemovePrimaryRelationship();
			Factory.Save();
			var controller = new OrganisationContactController();
			var request = GetNudgeContactRequest();
			//Action
			var result = (OkNegotiatedContentResult<string>)controller.AutoLoginNudgeContact(request);
			//Assert
			AssertEquals($"OrgContact with PK: {contact2.PK}, ContactName: ({contact2.OC_ContactName}) has been updated.", result.Content);
			AssertEquals("2", contact2.OC_PhoneExtension);
		}

		public void TestAutoLoginNudgeContact_ShouldUpdate_WhenContactIsVerified()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact1 = (EDIOrgContact)org.Contacts.AddNew();
			contact1.OC_ContactName = "test1";
			contact1.OC_Email = "test@email.com";
			contact1.OC_WebAccessEnabled = false;
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact2 = (EDIOrgContact)org2.Contacts.AddNew();
			contact2.OC_ContactName = "test2";
			contact2.OC_Email = "test@email.com";
			contact2.OC_WebAccessEnabled = false;
			contact2.OC_PhoneExtension = "2";
			contact2.OC_DetailsVerified = DateTime.Now.AddDays(-10);
			Factory.Save();
			contact1.Person.RemovePrimaryRelationship();
			contact2.Person.RemovePrimaryRelationship();
			Factory.Save();
			var controller = new OrganisationContactController();
			var request = GetNudgeContactRequest();
			//Action
			var result = (OkNegotiatedContentResult<string>)controller.AutoLoginNudgeContact(request);
			//Assert
			AssertEquals($"OrgContact with PK: {contact2.PK}, ContactName: ({contact2.OC_ContactName}) has been updated.", result.Content);
			AssertEquals("2", contact2.OC_PhoneExtension);
		}

		static NudgeContactRequest GetNudgeContactRequest(bool validApiKey = true, string email = "test@email.com")
		{
			return new NudgeContactRequest()
			{ ApiKey = validApiKey ? BorderWiseApiBaseController.ApiKey : string.Empty, Email = email };
		}
		#endregion
		#region AutoLoginNudgeContactByPK
		public void TestAutoLoginNudgeContactByPK_Error_InvalidApiKey()
		{
			var controller = new OrganisationContactController();
			//Action
			var result = (StatusCodeResult)controller.AutoLoginNudgeContactByPK(Guid.NewGuid(), "invalid key");
			//Assert
			AssertEquals(HttpStatusCode.Forbidden, result.StatusCode);
		}

		public void TestAutoLoginNudgeContactByPK_ShouldReturnNotFound_WhenContactNotFound()
		{
			var logger = new NLogWrapperForTest(GetType());
			var controller = new OrganisationContactController(logger);
			var pk = Guid.NewGuid();
			//Action
			var result = (NegotiatedContentResult<string>)controller.AutoLoginNudgeContactByPK(pk, BorderWiseApiBaseController.ApiKey);
			//Assert
			AssertEquals(HttpStatusCode.NotFound, result.StatusCode);
			AssertEquals($"Could not find contact for PK: {pk}", result.Content);
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Could not find contact for PK: {pk}")));
			logger.ClearLog();
		}

		public void TestAutoLoginNudgeContactByPK_ShouldUpdate_WhenContactIsWebAccessDisabled()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var logger = new NLogWrapperForTest(GetType());
			var contact = (EDIOrgContact)org.Contacts.AddNew();
			contact.OC_ContactName = "test1";
			contact.OC_Email = "test@email.com";
			contact.OC_WebAccessEnabled = false;
			Factory.Save();
			var controller = new OrganisationContactController(logger);

			//Action
			var result = (OkNegotiatedContentResult<string>)controller.AutoLoginNudgeContactByPK(contact.PK.ToGuid(), BorderWiseApiBaseController.ApiKey);
			//Assert
			AssertEquals($"OrgContact with PK: {contact.PK}, ContactName: ({contact.OC_ContactName}) has been updated.", result.Content);
			AssertEquals(true, contact.OC_WebAccessEnabled);

			AssertEquals(5, logger.LogEntries.Count);
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Starting the nudge contact process. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Starting the EnableWebAccessOrNudge process. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Completed the nudge contact process. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Starting the contact processing. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Completed the contact processing. PK - {contact.PK}")));
			logger.ClearLog();
		}

		public void TestAutoLoginNudgeContactByPK_ShouldReturnForbidden_WhenContactWithSameEmailAndIsWebAccessEnabledAndIsActiveExists()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var logger = new NLogWrapperForTest(GetType());
			var contact = (EDIOrgContact)org.Contacts.AddNew();
			var contact1 = (EDIOrgContact)org.Contacts.AddNew();
			contact.OC_ContactName = "user1";
			contact.OC_Email = "test@email.com";
			contact.OC_WebAccessEnabled = false;
			contact.OC_IsActive = true;
			contact1.OC_ContactName = "user2";
			contact1.OC_Email = "test@email.com";
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_IsActive = true;
			Factory.Save();
			var controller = new OrganisationContactController(logger);

			//Action
			var result = (NegotiatedContentResult<string>)controller.AutoLoginNudgeContactByPK(contact.PK.ToGuid(), BorderWiseApiBaseController.ApiKey);
			//Assert
			AssertEquals(HttpStatusCode.Forbidden, result.StatusCode);
			AssertContains("Failed to enable web access for the user ( user1 / test@email.com ). Each active contact with web access in this organization must have a unique email address.", result.Content);

			AssertEquals(6, logger.LogEntries.Count);
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Starting the nudge contact process. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Starting the contact processing. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Starting the EnableWebAccessOrNudge process. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains("Failed to enable web access for the user ( user1 / test@email.com ). Each active contact with web access in this organization must have a unique email address.")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Completed the contact processing. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Completed the nudge contact process. PK - {contact.PK}")));
			logger.ClearLog();
		}

		public void TestActivateUserAndEnableWebAccess_ShouldUpdate_WhenContactIsWebAccessDisabledOrIsDeactive()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var logger = new NLogWrapperForTest(GetType());
			var contact = (EDIOrgContact)org.Contacts.AddNew();
			contact.OC_ContactName = "test1";
			contact.OC_Email = "test@email.com";
			contact.OC_WebAccessEnabled = false;
			contact.OC_IsActive = false;
			Factory.Save();
			var controller = new OrganisationContactController(logger);

			//Action
			var result = (OkNegotiatedContentResult<string>)controller.ActivateUserAndEnableWebAccess(contact.PK.ToGuid(), BorderWiseApiBaseController.ApiKey);
			//Assert
			AssertEquals($"OrgContact with PK: {contact.PK}, ContactName: ({contact.OC_ContactName}) has been updated.", result.Content);
			AssertEquals(true, contact.OC_WebAccessEnabled);
			AssertEquals(true, contact.OC_IsActive);

			AssertEquals(5, logger.LogEntries.Count);
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Starting the ActivateUserAndEnableWebAccess process. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Starting the EnableWebAccessAndActivateUser process. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Completed the ActivateUserAndEnableWebAccess process. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Starting the contact processing. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Completed the contact processing. PK - {contact.PK}")));
			logger.ClearLog();
		}

		public void TestActivateUserAndEnableWebAccess_ShouldReturnNotFound_WhenContactNotFound()
		{
			var logger = new NLogWrapperForTest(GetType());
			var controller = new OrganisationContactController(logger);
			var pk = Guid.NewGuid();
			//Action
			var result = (NegotiatedContentResult<string>)controller.ActivateUserAndEnableWebAccess(pk, BorderWiseApiBaseController.ApiKey);
			//Assert
			AssertEquals(HttpStatusCode.NotFound, result.StatusCode);
			AssertEquals($"Could not find contact for PK: {pk}", result.Content);
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Could not find contact for PK: {pk}")));
			logger.ClearLog();
		}

		public void TestActivateUserAndEnableWebAccess_ShouldLogAndReturn_WhenContactIsWebAccessEnabledAndIsActive()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var logger = new NLogWrapperForTest(GetType());
			var contact = (EDIOrgContact)org.Contacts.AddNew();
			contact.OC_ContactName = "test1";
			contact.OC_Email = "test@email.com";
			contact.OC_WebAccessEnabled = true;
			contact.OC_IsActive = true;
			Factory.Save();
			var controller = new OrganisationContactController(logger);

			//Action
			var result = (OkNegotiatedContentResult<string>)controller.ActivateUserAndEnableWebAccess(contact.PK.ToGuid(), BorderWiseApiBaseController.ApiKey);
			//Assert
			AssertEquals("EnableWebAccessAndActivateUser - User is active and web access is enabled.", result.Content);

			AssertEquals(6, logger.LogEntries.Count);
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Starting the ActivateUserAndEnableWebAccess process. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Starting the EnableWebAccessAndActivateUser process. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Completed the ActivateUserAndEnableWebAccess process. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"EnableWebAccessAndActivateUser - User is active and web access is enabled.")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Starting the contact processing. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Completed the contact processing. PK - {contact.PK}")));
			logger.ClearLog();
		}

		public void TestActivateUserAndEnableWebAccess_ShouldReturnForbidden_WhenContactWithSameEmailAndIsWebAccessEnabledAndIsActiveExists()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var logger = new NLogWrapperForTest(GetType());
			var contact = (EDIOrgContact)org.Contacts.AddNew();
			var contact1 = (EDIOrgContact)org.Contacts.AddNew();
			contact.OC_ContactName = "user1";
			contact.OC_Email = "test@email.com";
			contact.OC_WebAccessEnabled = false;
			contact.OC_IsActive = true;
			contact1.OC_ContactName = "user2";
			contact1.OC_Email = "test@email.com";
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_IsActive = true;
			Factory.Save();
			var controller = new OrganisationContactController(logger);

			//Action
			var result = (NegotiatedContentResult<string>)controller.ActivateUserAndEnableWebAccess(contact.PK.ToGuid(), BorderWiseApiBaseController.ApiKey);
			//Assert
			AssertEquals(HttpStatusCode.Forbidden, result.StatusCode);
			AssertContains("Failed to enable web access for the user ( user1 / test@email.com ). Each active contact with web access in this organization must have a unique email address.", result.Content);
			
			AssertEquals(6, logger.LogEntries.Count);
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Starting the ActivateUserAndEnableWebAccess process. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Starting the EnableWebAccessAndActivateUser process. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Completed the ActivateUserAndEnableWebAccess process. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains("Failed to enable web access for the user ( user1 / test@email.com ). Each active contact with web access in this organization must have a unique email address.")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Starting the contact processing. PK - {contact.PK}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Completed the contact processing. PK - {contact.PK}")));
			logger.ClearLog();
		}

		#endregion

		#region SyncBorderWiseLicenseAndEdiCustomerUserAccount

		public void TestSyncBorderWiseLicense_Error_InvalidApiKey()
		{
			var controller = new OrganisationContactController();

			var request = new SyncBorderWiseUsersStatusRequest()
			{
				ApiKey = "invalid key"
			};

			//Action
			var result = (StatusCodeResult)controller.SyncBorderWiseLicenseAndEdiCustomerUserAccount(request);

			//Assert
			AssertEquals(HttpStatusCode.Forbidden, result.StatusCode);
		}

		public void TestSyncBorderWiseLicense_ShouldLogAndReturnNoUsersFound_WhenUsersIsEmpty()
		{
			var logger = new NLogWrapperForTest(GetType());
			var controller = new OrganisationContactController(logger);

			var request = new SyncBorderWiseUsersStatusRequest()
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
				BorderWiseUsers = new List<BorderWiseUser>()
			};

			var expectedMessage = "No Borderwise users data found in request.";

			//Action
			var result = (NegotiatedContentResult<string>)controller.SyncBorderWiseLicenseAndEdiCustomerUserAccount(request);

			//Assert
			AssertEquals(HttpStatusCode.BadRequest, result.StatusCode);
			AssertContains(expectedMessage, result.Content);

			AssertEquals(1, logger.LogEntries.Count);
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains(expectedMessage)));
		}

		public void TestSyncBorderWiseLicense_ShouldReturnErrorMessagesCorrectly_WhenUserDetailsAreInvalid()
		{
			var logger = new NLogWrapperForTest(GetType());
			var controller = new OrganisationContactController(logger);

			var invalidUser1 = new BorderWiseUser
			{
				ContactFullName = "Invalid User 1",
				Email = "sample1@test.com",
				ContactPk = default,
				EdiProdRecordId = Guid.NewGuid()
			};

			var invalidUser2 = new BorderWiseUser
			{
				ContactFullName = "Invalid User 2",
				Email = "",
				ContactPk = Guid.NewGuid(),
				EdiProdRecordId = Guid.NewGuid()
			};

			var invalidUser3 = new BorderWiseUser
			{
				ContactFullName = "Invalid User 3",
				Email = "sample2@test.com",
				ContactPk = Guid.NewGuid(),
				EdiProdRecordId = default
			};

			var request = new SyncBorderWiseUsersStatusRequest()
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
				BorderWiseUsers = new List<BorderWiseUser>() { invalidUser1, invalidUser2, invalidUser3 }
			};

			//Action
			var response = (OkNegotiatedContentResult<SyncBorderWiseUsersStatusResponse>)controller.SyncBorderWiseLicenseAndEdiCustomerUserAccount(request);

			//Assert
			AssertNotNull(response);
			AssertNotNull(response.Content);
			var processedUsers = response.Content.ProcessedBorderWiseUsers;
			AssertEquals($"Invalid Borderwise user details. Email: {invalidUser1.Email}, ContactPk: {invalidUser1.ContactPk}, EdiProdRecordId: {invalidUser1.EdiProdRecordId}, LicenseDatabaseNumber: {invalidUser1.LicenseDatabaseNumber}", processedUsers[0].ErrorMessage);
			AssertEquals($"Invalid Borderwise user details. Email: {invalidUser2.Email}, ContactPk: {invalidUser2.ContactPk}, EdiProdRecordId: {invalidUser2.EdiProdRecordId}, LicenseDatabaseNumber: {invalidUser2.LicenseDatabaseNumber}", processedUsers[1].ErrorMessage);
			AssertEquals($"Invalid Borderwise user details. Email: {invalidUser3.Email}, ContactPk: {invalidUser3.ContactPk}, EdiProdRecordId: {invalidUser3.EdiProdRecordId}, LicenseDatabaseNumber: {invalidUser3.LicenseDatabaseNumber}", processedUsers[2].ErrorMessage);

			AssertEquals(8, logger.LogEntries.Count);
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains("Starting the SyncBorderWiseLicenseAndEdiCustomerUserAccount process.")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Invalid Borderwise user details. Email: {invalidUser1.Email}, ContactPk: {invalidUser1.ContactPk}, EdiProdRecordId: {invalidUser1.EdiProdRecordId}, LicenseDatabaseNumber: {invalidUser1.LicenseDatabaseNumber}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Invalid Borderwise user details. Email: {invalidUser2.Email}, ContactPk: {invalidUser2.ContactPk}, EdiProdRecordId: {invalidUser2.EdiProdRecordId}, LicenseDatabaseNumber: {invalidUser2.LicenseDatabaseNumber}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Invalid Borderwise user details. Email: {invalidUser3.Email}, ContactPk: {invalidUser3.ContactPk}, EdiProdRecordId: {invalidUser3.EdiProdRecordId}, LicenseDatabaseNumber: {invalidUser3.LicenseDatabaseNumber}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains("Completed the SyncBorderWiseLicenseAndEdiCustomerUserAccount process.")));
		}

		public void TestSyncBorderWiseLicense_ShouldReturnErrorMessageAndLog_WhenLicenceDatabaseFoundIsInactive()
		{
			var logger = new NLogWrapperForTest(GetType());
			var controller = new OrganisationContactController(logger);

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_DatabaseNumber = 123;
			database.LD_IsActive = false;
			Factory.Save();

			var request = new SyncBorderWiseUsersStatusRequest()
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
				BorderWiseUsers = new List<BorderWiseUser>()
				{
					new BorderWiseUser
					{
						ContactFullName = "Valid User",
						Email = "validemail@test.com",
						ContactPk = Guid.NewGuid(),
						EdiProdRecordId = Guid.NewGuid(),
						LicenseDatabaseNumber = database.LD_DatabaseNumber
					}
				}
			};

			var expectedErrorMessage = $"No active licence database found for database number {request.BorderWiseUsers[0].LicenseDatabaseNumber}. UserId: {request.BorderWiseUsers[0].ContactPk}";

			//Action
			var response = (OkNegotiatedContentResult<SyncBorderWiseUsersStatusResponse>)controller.SyncBorderWiseLicenseAndEdiCustomerUserAccount(request);

			//Assert
			AssertNotNull(response);
			AssertNotNull(response.Content);
			var processedUsers = response.Content.ProcessedBorderWiseUsers;
			AssertEquals(expectedErrorMessage, processedUsers[0].ErrorMessage);

			AssertEquals(3, logger.LogEntries.Count);
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains("Starting the SyncBorderWiseLicenseAndEdiCustomerUserAccount process.")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains(expectedErrorMessage)));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains("Completed the SyncBorderWiseLicenseAndEdiCustomerUserAccount process.")));
		}

		public void TestSyncBorderWiseLicense_CreatesNewEdiCustomerAccount_WhenNoMatchingAccountExists()
		{
			var logger = new NLogWrapperForTest(GetType());
			var controller = new OrganisationContactController(logger);

			var ediOrgContact = Factory.NewWithValidTestData<EDIOrgContact>();
			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_DatabaseNumber = 123;
			database.LD_IsActive = true;
			
			Factory.Save();

			var contactPK = Guid.NewGuid();
			var ediProdRecordId = ediOrgContact.PK;
			var email = "validemail@test.com";
			var contactFullName = "Valid User";

			var request = new SyncBorderWiseUsersStatusRequest()
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
				BorderWiseUsers = new List<BorderWiseUser>()
				{
					new BorderWiseUser
					{
						ContactFullName = contactFullName,
						Email = email,
						ContactPk = contactPK,
						EdiProdRecordId = ediProdRecordId.ToGuid(),
						LicenseDatabaseNumber = database.LD_DatabaseNumber,
						HasActiveBorderWiseLicense = true
					}
				}
			};

			//Action
			var response = (OkNegotiatedContentResult<SyncBorderWiseUsersStatusResponse>)controller.SyncBorderWiseLicenseAndEdiCustomerUserAccount(request);

			var userAccountQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, contactPK.ToString());
			userAccountQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_LD, database.PK);
			var ediCustomerAccount = Factory.LoadTop1<EdiCustomerUserAccount>(userAccountQuery);

			//Assert
			AssertNotNull(ediCustomerAccount);
			AssertEquals(contactPK.ToString(), ediCustomerAccount.EUA_UserID.ToString());
			AssertEquals(database.PK, ediCustomerAccount.EUA_LD);
			AssertEquals(email, ediCustomerAccount.EUA_Email);
			AssertEquals(contactFullName, ediCustomerAccount.EUA_FullName);
			AssertEquals(ediProdRecordId, ediCustomerAccount.EUA_OC_WebAccessContact);
			AssertEquals(true, ediCustomerAccount.EUA_IsActive);

			AssertNotNull(response);
			AssertNotNull(response.Content);
			var processedUsers = response.Content.ProcessedBorderWiseUsers;
			AssertNotNull(processedUsers[0].ErrorMessage);

			AssertEquals(3, logger.LogEntries.Count);
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains("Starting the SyncBorderWiseLicenseAndEdiCustomerUserAccount process.")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Created new EdiCustomerAccount record. UserId: {contactPK}, Email: {email}, FullName: {contactFullName}, HasActiveBorderWiseLicense: True")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains("Completed the SyncBorderWiseLicenseAndEdiCustomerUserAccount process.")));
		}

		public void TestSyncBorderWiseLicense_UpdateEdiCustomerAccount_WhenMatchingAccountExists()
		{
			var logger = new NLogWrapperForTest(GetType());
			var controller = new OrganisationContactController(logger);

			var ediOrgContact = Factory.NewWithValidTestData<EDIOrgContact>();
			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_DatabaseNumber = 123;
			database.LD_IsActive = true;

			var contactPK = Guid.NewGuid();
			var ediProdRecordId = ediOrgContact.PK;
			var email = "updatedEmail@test.com";
			var contactFullName = "Updated Name";

			var ediCustomerAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			ediCustomerAccount.EUA_UserID = contactPK.ToString();
			ediCustomerAccount.EUA_LD = database.PK;
			ediCustomerAccount.EUA_IsActive = true;

			Factory.Save();

			var request = new SyncBorderWiseUsersStatusRequest()
			{
				ApiKey = BorderWiseApiBaseController.ApiKey,
				BorderWiseUsers = new List<BorderWiseUser>()
				{
					new BorderWiseUser
					{
						ContactFullName = contactFullName,
						Email = email,
						ContactPk = contactPK,
						EdiProdRecordId = ediProdRecordId.ToGuid(),
						LicenseDatabaseNumber = database.LD_DatabaseNumber,
						HasActiveBorderWiseLicense = false
					}
				}
			};

			//Action
			var response = (OkNegotiatedContentResult<SyncBorderWiseUsersStatusResponse>)controller.SyncBorderWiseLicenseAndEdiCustomerUserAccount(request);

			//Assert
			AssertNotNull(ediCustomerAccount);
			AssertEquals(contactPK.ToString(), ediCustomerAccount.EUA_UserID.ToString());
			AssertEquals(database.PK, ediCustomerAccount.EUA_LD);
			AssertEquals(email, ediCustomerAccount.EUA_Email);
			AssertEquals(contactFullName, ediCustomerAccount.EUA_FullName);
			AssertEquals(false, ediCustomerAccount.EUA_IsActive);

			AssertNotNull(response);
			AssertNotNull(response.Content);
			var processedUsers = response.Content.ProcessedBorderWiseUsers;
			AssertNotNull(processedUsers[0].ErrorMessage);

			AssertEquals(4, logger.LogEntries.Count);
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains("Starting the SyncBorderWiseLicenseAndEdiCustomerUserAccount process.")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Existing EdiCustomerAccount record found for Borderwise user. User Id: {contactPK.ToString()}, DatabaseNumber: {database.LD_DatabaseNumber}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains($"Updated existing EdiCustomerAccount record. User Id: {contactPK.ToString()}, HasActiveBorderWiseLicense: False, Email: {email}, ContactFullName: {contactFullName}")));
			AssertEquals(true, logger.LogEntries.Any(l => l.Contains("Completed the SyncBorderWiseLicenseAndEdiCustomerUserAccount process.")));
		}

		#endregion
	}
}
