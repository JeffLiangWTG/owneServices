using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.BorderWise;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.AddressCleansing.Common;
using static Enterprise.ZClientWebCargoWiseEDI.BorderWise.BorderWiseRegistrationController;

namespace Enterprise.ZClientWebCargoWiseEDI.BorderWise.Testing
{
	public class BorderWiseRegistrationControllerTest : TestCaseWithFactory
	{
		class BorderWiseRegistrationControllerForTest : BorderWiseRegistrationController
		{
			protected override HttpClientHandler GetHttpClientHandler()
			{
				return HttpClientHandlerMock;
			}

			HttpClientHandlerMock HttpClientHandlerMock { get; set; }

			public void SetHttpClientHandlerMock(HttpStatusCode httpStatusCode, string content, string exceptionMessage = default)
			{
				HttpClientHandlerMock = new HttpClientHandlerMock(new HttpResponseMessage(httpStatusCode)
				{
					Content = new StringContent(content)
					{ Headers = { ContentType = new MediaTypeHeaderValue("application/json") } }
				}, exceptionMessage);
			}
		}

		class HttpClientHandlerMock : HttpClientHandler
		{
			readonly HttpResponseMessage response;
			readonly string exceptionMessage;
			public HttpClientHandlerMock(HttpResponseMessage response, string exceptionMessage)
			{
				this.response = response;
				this.exceptionMessage = exceptionMessage;
			}

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				if (!string.IsNullOrWhiteSpace(exceptionMessage))
				{
					throw new Exception(exceptionMessage);
				}

				return Task.FromResult(response);
			}
		}

		#region GetRegistrationOneTimeLink V4

		RegistrationRequest RegistrationRequest => new RegistrationRequest { EmailAddress = "rylan@zayden.com", IsActiveUserAllowedToRegisterToNewCompany = false };
		RegistrationRequest FreeTrialRequest => new RegistrationRequest
		{
			EmailAddress = "rylan@zayden.com",
			IsActiveUserAllowedToRegisterToNewCompany = false,
			IsFreeTrial = true,
			FirstName = "John",
			LastName = "Smith"
		};

		public void TestGetRegistrationOneTimeLinkV4_Error_InvalidParams()
		{
			var controller = new BorderWiseRegistrationController();
			var requestNoName = new RegistrationRequest
			{
				EmailAddress = "rylan@zayden.com",
				IsActiveUserAllowedToRegisterToNewCompany = false,
				IsFreeTrial = true,
				FirstName = string.Empty,
				LastName = "Smith"
			};
			var requestNoEmail = new RegistrationRequest
			{
				EmailAddress = string.Empty,
				IsActiveUserAllowedToRegisterToNewCompany = false,
			};

			AssertEquals("RegistrationRequest cannot be null.", ((BadRequestErrorMessageResult)controller.GetRegistrationOneTimeLinkV4(null)).Message);
			AssertEquals("All arguments must be provided and be valid.", ((BadRequestErrorMessageResult)controller.GetRegistrationOneTimeLinkV4(requestNoName)).Message);
			AssertEquals("All arguments must be provided and be valid.", ((BadRequestErrorMessageResult)controller.GetRegistrationOneTimeLinkV4(requestNoEmail)).Message);
		}

		public void TestGetRegistrationOneTimeLinkV4_Error_ExistingContacts()
		{
			var contact = CreateOrgAndContact();
			var contact2 = CreateOrgAndContact("ZUB2");
			contact2.OC_PasswordHash = ZBlob.Empty;
			var contact3 = CreateOrgAndContact("ZUB3");
			contact3.OC_WebAccessEnabled = false;
			contact3.OC_PasswordHash = ZBlob.Empty;
			contact2.OC_PhoneExtension = "123";
			contact3.OC_PhoneExtension = "1234567890";
			Factory.Save();
			var result = (OkNegotiatedContentResult<string>)new BorderWiseRegistrationController().GetRegistrationOneTimeLinkV4(RegistrationRequest);
			AssertEquals(CommonConstants.userRegistrationOneTimeLinkResponse, result.Content);
			AssertEquals(true, contact.OC_WebAccessEnabled);
			AssertEquals(true, contact2.OC_WebAccessEnabled);
			AssertLoginDetailsEmailSend(2);

			AssertEquals("", contact.OC_PhoneExtension);
			AssertEquals("123", contact2.OC_PhoneExtension);
			AssertEquals("1234567890", contact3.OC_PhoneExtension);
		}

		public void TestGetRegistrationOneTimeLinkV4_Error_ExistingContacts_NoneWebEnabled()
		{
			var orgContacts = CreateOrgAndContacts(numberOfContacts: 2);
			orgContacts.ForEach(c =>
			{
				c.OC_PasswordHash = ZBlob.Empty;
				c.OC_WebAccessEnabled = false;
			});
			var adminContact = orgContacts[0].ParentOrg.Contacts.AddNew();
			adminContact.OC_ContactName = "admin";
			adminContact.OC_Email = "admin@zayden.com";
			adminContact.SetHashedPassword("zubin123");
			adminContact.OC_WebAccessEnabled = true;
			adminContact.OC_IsActive = true;
			Factory.Save();
			var controller = new BorderWiseRegistrationControllerForTest();
			var adminInfos = new List<OrgHeaderAdminInfo> { new OrgHeaderAdminInfo { OrgHeaderPk = adminContact.OC_OH.ToGuid(), AdminContactEmail = adminContact.OC_Email } };
			var contentString = JsonConvert.SerializeObject(adminInfos);
			controller.SetHttpClientHandlerMock(HttpStatusCode.OK, contentString);
			var registrationRequest = new RegistrationRequest { EmailAddress = "contact1@gmail.com", IsActiveUserAllowedToRegisterToNewCompany = false };
			var result = (OkNegotiatedContentResult<string>)controller.GetRegistrationOneTimeLinkV4(registrationRequest);
			AssertEquals(CommonConstants.userRegistrationOneTimeLinkResponse, result.Content);
			AssertEquals(false, orgContacts[0].OC_WebAccessEnabled);
			AssertEquals(false, orgContacts[1].OC_WebAccessEnabled);
			AssertConfirmAccessEmailSend(1);
			AssertNoActiveAccountsEmailSend(true, 1, "contact1@gmail.com", 1);
			AssertEmailsSent(2);
		}

		public void TestGetRegistrationOneTimeLinkV4_Error_ExistingContacts_NoneWebEnabled_No_Admin_Found()
		{
			var orgContacts = CreateOrgAndContacts(numberOfContacts: 2);
			orgContacts.ForEach(c =>
			{
				c.OC_PasswordHash = ZBlob.Empty;
				c.OC_WebAccessEnabled = false;
			});
			Factory.Save();
			var controller = new BorderWiseRegistrationControllerForTest();
			controller.SetHttpClientHandlerMock(HttpStatusCode.NotFound, "Not found");
			var registrationRequest = new RegistrationRequest { EmailAddress = "contact1@gmail.com", IsActiveUserAllowedToRegisterToNewCompany = false };
			var result = (OkNegotiatedContentResult<string>)controller.GetRegistrationOneTimeLinkV4(registrationRequest);
			AssertEquals(CommonConstants.userRegistrationOneTimeLinkResponse, result.Content);
			AssertEquals(false, orgContacts[0].OC_WebAccessEnabled);
			AssertEquals(false, orgContacts[1].OC_WebAccessEnabled);
			AssertConfirmAccessEmailSend(0);
			AssertNoActiveAccountsEmailSend(false, 1, "contact1@gmail.com");
			AssertEmailsSent(1);
			ErrorReporter.Clear();
		}

		public void TestGetRegistrationOneTimeLinkV4_Should_SetWebAccess_When_SingleActiveContact_NotWebEnabled()
		{
			var contact = CreateOrgAndContact(email: "contact1@gmail.com");
			contact.OC_WebAccessEnabled = false;
			contact.OC_PasswordHash = ZBlob.Empty;
			Factory.Save();
			var controller = new BorderWiseRegistrationControllerForTest();
			controller.SetHttpClientHandlerMock(HttpStatusCode.NotFound, "Not found");
			var registrationRequest = new RegistrationRequest { EmailAddress = "contact1@gmail.com", IsActiveUserAllowedToRegisterToNewCompany = false };
			var result = (OkNegotiatedContentResult<string>)controller.GetRegistrationOneTimeLinkV4(registrationRequest);
			AssertEquals(CommonConstants.userRegistrationOneTimeLinkResponse, result.Content);
			AssertEquals(true, contact.OC_WebAccessEnabled);
			AssertLoginDetailsEmailSend(1, "contact1@gmail.com");
		}

		public void TestGetRegistrationOneTimeLinkV4_Should_DenyWebAccess_When_NoneWebEnabledAndEmailAddressIsUsedByMultipleCompanies()
		{
			var singleContact = CreateOrgAndContact(orgCode: "ZUB1", email: "contact1@gmail.com");
			singleContact.OC_WebAccessEnabled = false;
			singleContact.OC_PasswordHash = ZBlob.Empty;

			var multipleContacts = CreateOrgAndContacts(orgCode: "ZUB2", numberOfContacts: 2);
			multipleContacts.ForEach(c =>
				{
					c.OC_PasswordHash = ZBlob.Empty;
					c.OC_WebAccessEnabled = false;
				});
			var adminContact = multipleContacts[0].ParentOrg.Contacts.AddNew();
			adminContact.OC_ContactName = "admin";
			adminContact.OC_Email = "admin@zayden.com";
			adminContact.SetHashedPassword("zubin123");
			adminContact.OC_WebAccessEnabled = true;
			adminContact.OC_IsActive = true;
			Factory.Save();
			var controller = new BorderWiseRegistrationControllerForTest();
			var adminInfos = new List<OrgHeaderAdminInfo> { new OrgHeaderAdminInfo { OrgHeaderPk = adminContact.OC_OH.ToGuid(), AdminContactEmail = adminContact.OC_Email } };
			var contentString = JsonConvert.SerializeObject(adminInfos);
			controller.SetHttpClientHandlerMock(HttpStatusCode.OK, contentString);
			var registrationRequest = new RegistrationRequest { EmailAddress = "contact1@gmail.com", IsActiveUserAllowedToRegisterToNewCompany = false };
			var result = (OkNegotiatedContentResult<string>)controller.GetRegistrationOneTimeLinkV4(registrationRequest);
			AssertEquals(CommonConstants.userRegistrationOneTimeLinkResponse, result.Content);
			AssertEquals(false, singleContact.OC_WebAccessEnabled);
			AssertEquals(false, multipleContacts[0].OC_WebAccessEnabled);
			AssertEquals(false, multipleContacts[1].OC_WebAccessEnabled);
			AssertConfirmAccessEmailSend(1, orgCode: "ZUB2");
			AssertNoActiveAccountsEmailSend(true, 1, "contact1@gmail.com", 1);
			AssertEmailsSent(2);
		}

		[TestDate(2014, 9, 26, 1, 10, 13)]
		public void TestGetRegistrationOneTimeLinkV4_Success()
		{
			EDIDataRegistry.Instance.BorderWiseRegistrationUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://www.zubin.com/token=");
			var result = (OkNegotiatedContentResult<string>)new BorderWiseRegistrationController().GetRegistrationOneTimeLinkV4(RegistrationRequest);
			AssertRegistrationOneTimeLinkSuccess(result);
		}

		[TestDate(2014, 9, 26, 1, 10, 13)]
		public void TestGetRegistrationOneTimeLinkV4_FreeTrial_Success()
		{
			EDIDataRegistry.Instance.BorderWiseRegistrationUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://www.borderwise.com/token=");
			var result = (OkNegotiatedContentResult<string>)new BorderWiseRegistrationController().GetRegistrationOneTimeLinkV4(FreeTrialRequest);
			AssertRegistrationOneTimeLinkSuccess(result, isFreeTrial: true);
		}

		[TestDate(2014, 9, 26, 1, 10, 13)]
		public void TestGetRegistrationOneTimeLinkV4_Success_ExistingActiveContacts_AllowedToRegisterToNewCompany()
		{
			EDIDataRegistry.Instance.BorderWiseRegistrationUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://www.zubin.com/token=");
			var contact = CreateOrgAndContact();
			var request = new RegistrationRequest
			{
				EmailAddress = contact.OC_Email,
				IsActiveUserAllowedToRegisterToNewCompany = true
			};
			var result = (OkNegotiatedContentResult<string>)new BorderWiseRegistrationController().GetRegistrationOneTimeLinkV4(request);
			AssertRegistrationOneTimeLinkSuccess(result);
		}

		void AssertRegistrationOneTimeLinkSuccess(OkNegotiatedContentResult<string> result, bool isFreeTrial = false)
		{
			AssertEquals(CommonConstants.userRegistrationOneTimeLinkResponse, result.Content);
			var tokenQuery = new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.StartsWith, "BorderWiseReg-");
			var token = Factory.LoadTop1<StmData>(tokenQuery);
			var tokenDetails = token.SD_BinaryValue.ToAscii();
			AssertEquals(new ZDateTime(2014, 9, 26, 2, 10, 13).ToString(ZDateTime.LongTimeFormat, CultureInfo.InvariantCulture) + "|rylan@zayden.com", tokenDetails);
			if (isFreeTrial)
			{
				AssertOneTimeRegistrationLinkEmail(string.Format("https://www.borderwise.com/token={0}&contactEmail=rylan%40zayden.com&firstName=John&lastName=Smith", token.SD_Name.Replace("BorderWiseReg-", string.Empty)));
			}
			else
			{
				AssertOneTimeRegistrationLinkEmail(string.Format("https://www.zubin.com/token={0}&contactEmail=rylan%40zayden.com", token.SD_Name.Replace("BorderWiseReg-", string.Empty)));
			}
		}

		void AssertOneTimeRegistrationLinkEmail(string expectedActivationLink)
		{
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("support@borderwise.com", email.FromAddress);
			AssertEquals("support@borderwise.com", email.ReplyTo);
			AssertEquals("BorderWise Support", email.FromDisplayName);
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("rylan@zayden.com", email.Recipients[0].Email);
			AssertEquals("Activate your BorderWise account", email.Subject);
			var expectedBody = $@"Welcome to BorderWise.<br /><br />
To activate your BorderWise account, we need you to provide additional details via a secure link. Please click <a href='{expectedActivationLink}'>Activate Account</a> to complete this process.<br /><br />
This link can only be used once and will expire in one hour. A new link can be requested by beginning the registration process again.";
			AssertContains(expectedBody, email.Body);
		}

		#endregion

		#region GroupRegistrationInvitation

		GroupRegistrationInvitationRequest GetGroupRegistrationInvitationRequest(Guid orgPk, IEnumerable<string> emailAddresses, string adminName = "admin", string adminEmail = "admin@email.com")
		{
			return new GroupRegistrationInvitationRequest { OrgPk = orgPk, EmailAddresses = emailAddresses, AdminName = adminName, AdminEmail = adminEmail };
		}

		public void TestGroupRegistrationInvitation_ShouldReturnBadRequest_When_InvalidParams()
		{
			var controller = new BorderWiseRegistrationController();
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.GroupRegistrationInvitation(GetGroupRegistrationInvitationRequest(Guid.Empty, new List<string> { "test@email.com" }))).Message);
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.GroupRegistrationInvitation(GetGroupRegistrationInvitationRequest(Guid.NewGuid(), new List<string>()))).Message);
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.GroupRegistrationInvitation(GetGroupRegistrationInvitationRequest(Guid.NewGuid(), new List<string> { "test@email.com" }, "", "admin@email.com"))).Message);
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.GroupRegistrationInvitation(GetGroupRegistrationInvitationRequest(Guid.NewGuid(), new List<string> { "test@email.com" }, "admin", ""))).Message);
		}

		public void TestGroupRegistrationInvitation_ShouldReturnNotFound_When_OrgHeaderNotFound()
		{
			var result = new BorderWiseRegistrationController().GroupRegistrationInvitation(GetGroupRegistrationInvitationRequest(Guid.NewGuid(), new List<string> { "test@email.com" }));
			AssertEquals(typeof(NotFoundResult), result.GetType());
		}

		public void TestGroupRegistrationInvitation_ShouldReturnBadRequest_When_OrgHeaderIsInactive()
		{
			var contact = CreateOrgAndContact();
			contact.Header.OH_IsActive = false;
			Factory.Save();
			var controller = new BorderWiseRegistrationController();
			var result = (BadRequestErrorMessageResult)controller.GroupRegistrationInvitation(GetGroupRegistrationInvitationRequest(contact.Header.PK.ToGuid(), new List<string> { contact.Email }));
			AssertEquals("Organization is not active.", result.Message);
		}

		[TestDate(2014, 9, 26, 1, 10, 13)]
		public void TestGroupRegistrationInvitation_ShouldSendRegistrationEmail_When_ContactDoesNotExists()
		{
			var contact = CreateOrgAndContact();
			var controller = new BorderWiseRegistrationController();
			var result = (OkNegotiatedContentResult<string>)controller.GroupRegistrationInvitation(GetGroupRegistrationInvitationRequest(contact.Header.PK.ToGuid(), new List<string> { "test@mail.com" }));
			AssertEquals("Registration invitation(s) are processed successfully.", result.Content);
			var tokenQuery = new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.StartsWith, "BorderWiseReg-");
			var token = Factory.LoadTop1<StmData>(tokenQuery);
			var tokenDetails = token.SD_BinaryValue.ToAscii();
			AssertEquals(new ZDateTime(2014, 9, 29, 1, 10, 13).ToString(ZDateTime.LongTimeFormat, CultureInfo.InvariantCulture) + "|test@mail.com|" + contact.Header.PK, tokenDetails);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			var url = $"https://app.borderwise.com/account/activate?token={token.SD_Name.Replace("BorderWiseReg-", string.Empty)}&contactEmail=test%40mail.com&orgCode={WebUtility.UrlEncode(contact.OrgCode)}&orgName={WebUtility.UrlEncode(contact.Header.OH_FullName)}";

			AssertEquals("support@borderwise.com", email.FromAddress);
			AssertEquals("support@borderwise.com", email.ReplyTo);
			AssertEquals("BorderWise Support", email.FromDisplayName);
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("test@mail.com", email.Recipients[0].Email);
			AssertEquals("You are invited to BorderWise", email.Subject);
			var expectedBody = $@"Welcome to <a href='https://app.borderwise.com/'>BorderWise</a>.<br /><br />
You have been invited by admin (<a href='mailto:admin@email.com'>admin@email.com</a>) to register for <a href='https://app.borderwise.com/'>BorderWise</a>.<br /><br />
In order to activate your account, you need to provide further details about yourself via a secure link. Please click <a href='{url}'>Activate Account</a> to provide these details.<br /><br />
This link can only be used once and will expire in 3 days. If required, you can ask your company's BorderWise Administrator to send you a new invitation link.";
			AssertContains(expectedBody, email.Body);
		}

		public void TestGroupRegistrationInvitation_ShouldSendInvitationEmail_When_ContactExists()
		{
			ArrangeAndAssertGroupRegistrationInvitation(true);
		}

		public void TestGroupRegistrationInvitation_ShouldSendInvitationEmail_When_ContactExistsWithNoWebAccess()
		{
			ArrangeAndAssertGroupRegistrationInvitation(false);
		}

		void ArrangeAndAssertGroupRegistrationInvitation(bool webAccessEnabled)
		{
			var requestEmail = "test@email.com";
			var contact = CreateOrgAndContact(webAccessEnabled: webAccessEnabled, email: requestEmail);
			var controller = new BorderWiseRegistrationController();
			var result = (OkNegotiatedContentResult<string>)controller.GroupRegistrationInvitation(GetGroupRegistrationInvitationRequest(contact.Header.PK.ToGuid(), new List<string> { requestEmail }));
			AssertEquals("Registration invitation(s) are processed successfully.", result.Content);
			AssertEquals(true, contact.OC_WebAccessEnabled);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("support@borderwise.com", email.FromAddress);
			AssertEquals("support@borderwise.com", email.ReplyTo);
			AssertEquals("BorderWise Support", email.FromDisplayName);
			AssertEquals(1, email.Recipients.Count);
			AssertEquals(requestEmail, email.Recipients[0].Email);
			AssertEquals("You are invited to BorderWise", email.Subject);
			var expectedBody = $@"Dear {contact.OC_ContactName},<br /><br />
You have been invited by admin (<a href='mailto:admin@email.com'>admin@email.com</a>) to use <a href='https://app.borderwise.com/'>BorderWise</a>.<br /><br />
Your access details to BorderWise are:<br />
Company Code: <b>{contact.OrganisationCode}</b><br />
User Name: <b>{requestEmail}</b><br /><br />
Forgotten your password? Use the <a href='https://app.borderwise.com/account/forgot-password/'>Forgot Password</a> link to reset your password.";
			AssertContains(expectedBody, email.Body);
		}

		#endregion

		#region GetOrgBorderWiseAdminInfo

		public void TestGetOrgBorderWiseAdminInfo_Success_When_GetAdminInfoFromUmp()
		{
			var controller = new BorderWiseRegistrationControllerForTest();
			var expectedAdminInfo = new OrgHeaderAdminInfo { OrgHeaderPk = Guid.NewGuid(), AdminContactEmail = "admin@email.com" };
			var expectedAdminInfos = new List<OrgHeaderAdminInfo> { expectedAdminInfo };
			var contentString = JsonConvert.SerializeObject(expectedAdminInfos);
			controller.SetHttpClientHandlerMock(HttpStatusCode.OK, contentString);
			var adminInfos = controller.GetOrgBorderWiseAdminInfo(new List<Guid> { expectedAdminInfo.OrgHeaderPk });
			AssertEquals(1, adminInfos.Count);
			var adminInfo = adminInfos.First();
			AssertEquals(expectedAdminInfo.AdminContactEmail, adminInfo.AdminContactEmail);
			AssertEquals(expectedAdminInfo.OrgHeaderPk, adminInfo.OrgHeaderPk);
		}

		public void TestGetOrgBorderWiseAdminInfo_ShouldReturnNull_When_GetAdminInfoFromUmpNotSuccess()
		{
			var controller = new BorderWiseRegistrationControllerForTest();
			controller.SetHttpClientHandlerMock(HttpStatusCode.NotFound, "Not found");
			var orgPk = Guid.NewGuid();
			var adminInfos = controller.GetOrgBorderWiseAdminInfo(new List<Guid> { orgPk });
			AssertEquals(0, adminInfos.Count);
			var reportedErrorMessage = ErrorReporter.LastMessageReported;
			AssertEquals($"Get OrgHeader admin info from BorderWise UMP failed. OrgPks: {orgPk}, StatusCode: NotFound, Reason: Not Found, Message: Not found", reportedErrorMessage);
			ErrorReporter.Clear();
		}

		public void TestGetOrgBorderWiseAdminInfo_ShouldReportError_When_GetAdminInfoFromUmpThrowException()
		{
			var controller = new BorderWiseRegistrationControllerForTest();
			controller.SetHttpClientHandlerMock(HttpStatusCode.NotFound, "error", "exception");
			var orgPk = Guid.NewGuid();
			var adminInfos = controller.GetOrgBorderWiseAdminInfo(new List<Guid> { orgPk });
			AssertEquals(0, adminInfos.Count);
			var reportedErrorMessage = ErrorReporter.LastMessageReported;
			AssertEquals($"Get OrgHeader admin info from BorderWise UMP failed. OrgPks: {orgPk}", reportedErrorMessage);
			var exception = ErrorReporter.LastExceptionReported;
			AssertEquals("exception", exception.Message);
			ErrorReporter.Clear();
		}

		#endregion

		#region Confirm Access
		public void TestConfirmBorderWiseAccess_Error_InvalidParameter()
		{
			var controller = new BorderWiseRegistrationController();
			AssertEquals("Invalid argument.", ((BadRequestErrorMessageResult)controller.ConfirmAccess(Guid.Empty)).Message);
		}

		public void TestConfirmBorderWiseAccess_Error_Invalid_Token()
		{
			//Arrange
			var controller = new BorderWiseRegistrationController();
			var contact = CreateOrgAndContact(webAccessEnabled: false);
			var token = CreateTestConfirmAccessToken(contact.PK.ToGuid(), true, false);
			//Action
			var result = (NegotiatedContentResult<string>)controller.ConfirmAccess(token);
			//Assert
			AssertEquals(HttpStatusCode.BadRequest, result.StatusCode);
			AssertEquals("Invalid token. Can not confirm access.", result.Content);
		}

		public void TestConfirmBorderWiseAccess_Error_Token_Not_Found()
		{
			//Arrange
			var controller = new BorderWiseRegistrationController();
			//Action
			var result = (NegotiatedContentResult<string>)controller.ConfirmAccess(Guid.NewGuid());
			//Assert
			AssertEquals(HttpStatusCode.BadRequest, result.StatusCode);
			AssertEquals("Token not found. Can not confirm access.", result.Content);
		}

		public void TestConfirmBorderWiseAccess_Error_Token_Date_Invalid()
		{
			//Arrange
			var controller = new BorderWiseRegistrationController();
			var token = CreateTestConfirmAccessToken(Guid.NewGuid(), false);
			//Action
			var result = (NegotiatedContentResult<string>)controller.ConfirmAccess(token);
			//Assert
			AssertEquals(HttpStatusCode.Forbidden, result.StatusCode);
			AssertEquals("Invalid token expiry date. Can not confirm access.", result.Content);
		}

		public void TestConfirmBorderWiseAccess_Error_Token_Expired()
		{
			//Arrange
			var controller = new BorderWiseRegistrationController();
			var token = CreateTestConfirmAccessToken(Guid.NewGuid(), true, true, true);
			//Action
			var result = (NegotiatedContentResult<string>)controller.ConfirmAccess(token);
			//Assert
			AssertEquals(HttpStatusCode.Forbidden, result.StatusCode);
			AssertEquals("The token has expired. Can not confirm access.", result.Content);
		}

		public void TestConfirmBorderWiseAccess_Error_Token_Invalid_ContactPk()
		{
			//Arrange
			var controller = new BorderWiseRegistrationController();
			var token = CreateTestConfirmAccessToken(Guid.NewGuid(), true, true, false, false);
			//Action
			var result = (NegotiatedContentResult<string>)controller.ConfirmAccess(token);
			//Assert
			AssertEquals(HttpStatusCode.Forbidden, result.StatusCode);
			AssertEquals("Invalid token data. Can not confirm access.", result.Content);
		}

		public void TestConfirmBorderWiseAccess_Error_Contact_Not_Found()
		{
			//Arrange
			var controller = new BorderWiseRegistrationController();
			var token = CreateTestConfirmAccessToken(Guid.NewGuid());
			//Action
			var result = (NegotiatedContentResult<string>)controller.ConfirmAccess(token);
			//Assert
			AssertEquals(HttpStatusCode.NotFound, result.StatusCode);
			AssertEquals("The user is not found. Please contact WiseTech support to enable web access and grant BorderWise access right.", result.Content);
		}

		public void TestConfirmBorderWiseAccess_Error_Contact_Not_Active()
		{
			//Arrange
			var controller = new BorderWiseRegistrationController();
			var contact = CreateOrgAndContact(isActive: false);
			var token = CreateTestConfirmAccessToken(contact.PK.ToGuid());
			//Action
			var result = (NegotiatedContentResult<string>)controller.ConfirmAccess(token);
			//Assert
			AssertEquals(HttpStatusCode.Forbidden, result.StatusCode);
			AssertEquals("The user is not active. Please active the user and try again.", result.Content);
		}

		public void TestConfirmBorderWiseAccess_ShouldReturnForbidden_WhenContactWithSameEmailAndIsWebAccessEnabledAndIsActiveExists()
		{
			//Arrange
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "ZUB";
			var contact1 = CreateContact(org, isActive: true, webAccessEnabled: true, name: "user1", email: "user@company.com");
			var contact2 = CreateContact(org, isActive: true, webAccessEnabled: false, name: "user2", email: "user@company.com");

			var controller = new BorderWiseRegistrationController();
			var token = CreateTestConfirmAccessToken(contact2.PK.ToGuid());

			//Action
			var result = (NegotiatedContentResult<string>)controller.ConfirmAccess(token);
			//Assert
			AssertEquals(HttpStatusCode.Forbidden, result.StatusCode);
			AssertEquals("Failed to confirm access for the user ( user2 / user@company.com ). Each active contact with web access in this organization must have a unique email address.", result.Content);
		}

		public void TestConfirmBorderWiseAccess_Success_Contact_Access_Already_Confirmed()
		{
			//Arrange
			var controller = new BorderWiseRegistrationController();
			var contact = CreateOrgAndContact();
			BorderWiseUtilities.ChangeSecurityRight(contact, true, Factory);
			var token = CreateTestConfirmAccessToken(contact.PK.ToGuid());
			//Action
			var result = (NegotiatedContentResult<string>)controller.ConfirmAccess(token);
			//Assert
			AssertEquals(HttpStatusCode.OK, result.StatusCode);
			AssertEquals("The user's access to BorderWise has been granted already", result.Content);
		}

		public void TestConfirmBorderWiseAccess_Success()
		{
			//Arrange
			var controller = new BorderWiseRegistrationController();
			var contact = CreateOrgAndContact(webAccessEnabled: false);
			var token = CreateTestConfirmAccessToken(contact.PK.ToGuid());
			//Action
			var result = (OkNegotiatedContentResult<string>)controller.ConfirmAccess(token);
			//Assert
			AssertEquals("Access to BorderWise is granted.", result.Content);
		}

		public void TestConfirmBorderWiseAccess_NoPersonPassword()
		{
			//Arrange
			var relatedContact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			var controller = new BorderWiseRegistrationController();
			var contact = CreateOrgAndContact(webAccessEnabled: false);
			contact.OC_PER = relatedContact.OC_PER;
			var token = CreateTestConfirmAccessToken(contact.PK.ToGuid());
			Factory.Save();
			//Action
			var result = (OkNegotiatedContentResult<string>)controller.ConfirmAccess(token);
			//Assert
			AssertEquals("Precondition", "Access to BorderWise is granted.", result.Content);
			AssertEquals("Precondition: Should have sent password email.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var tokenQuery = new ZQuery(StmAccessTokenSchema.SAT_Scope, SQLComparisonOperator.Contains, contact.OC_Email);
			tokenQuery.AddToFilter(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.ResetPassword);
			AssertEquals("Should create contact password token", true, Factory.ExistsInDatabase(StmAccessTokenSchema.Constants.TableName, tokenQuery));
		}

		public void TestConfirmBorderWiseAccess_HasPersonPassword()
		{
			//Arrange
			var controller = new BorderWiseRegistrationController();
			var contact = CreateOrgAndContact(webAccessEnabled: false);
			contact.Person.SetHashedPassword("12345");
			Factory.Save();
			var token = CreateTestConfirmAccessToken(contact.PK.ToGuid());
			//Action
			var result = (OkNegotiatedContentResult<string>)controller.ConfirmAccess(token);
			//Assert
			AssertEquals("Precondition", "Access to BorderWise is granted.", result.Content);
			AssertEquals("Precondition: Should have sent password email.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var tokenQuery = new ZQuery(StmAccessTokenSchema.SAT_Scope, SQLComparisonOperator.Contains, contact.OC_Email);
			tokenQuery.AddToFilter(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.ResetMasterPassword);
			AssertEquals("Should create person password token", true, Factory.ExistsInDatabase(StmAccessTokenSchema.Constants.TableName, tokenQuery));
		}

		#endregion

		#region Register V5
		public void TestRegisterV5_Error_InvalidParams()
		{
			var controller = new BorderWiseRegistrationController();
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterV5(Guid.NewGuid(), null)).Message);
			var contactInfo = new ContactInfo();
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterV5(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.OrganisationName = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterV5(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.OrganisationAddress1 = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterV5(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.OrganisationCity = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterV5(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.OrganisationState = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterV5(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.OrganisationPostCode = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterV5(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.OrganisationCountryCode = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterV5(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.OrganisationPhone = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterV5(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.OrgCode = "Test";
			contactInfo.OrganisationName = "";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterV5(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.ContactName = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterV5(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.ContactDOB = DateTime.Now;
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterV5(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.ContactPassword = "SomePassword$123";
			var result = ((NegotiatedContentResult<string>)controller.RegisterV5(Guid.NewGuid(), contactInfo));
			AssertEquals(HttpStatusCode.BadRequest, result.StatusCode);
			AssertEquals("The registration link is not valid or has already been used. Please log in to BorderWise using the login details that were emailed to you, or request a new registration token.", result.Content);
		}

		public void TestRegisterV5_Error_InvalidPassword()
		{
			var controller = new BorderWiseRegistrationController();
			var contactInfo = GetValidContactInfo();
			contactInfo.ContactPassword = "password";
			var result = (BadRequestErrorMessageResult)controller.RegisterV5(Guid.NewGuid(), contactInfo);
			AssertEquals("Password must be at least 12 characters long.", result.Message);
			contactInfo.ContactPassword = "somepassword";
			result = (BadRequestErrorMessageResult)controller.RegisterV5(Guid.NewGuid(), contactInfo);
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", result.Message);
			contactInfo.ContactName = "some one";
			contactInfo.ContactPassword = "SomePassword#123";
			result = (BadRequestErrorMessageResult)controller.RegisterV5(Guid.NewGuid(), contactInfo);
			AssertEquals("The password appears to contain part of your name, email or a word which has been disallowed by your system administrator.", result.Message);
		}

		public void TestRegisterV5_Error_NoToken()
		{
			var controller = new BorderWiseRegistrationController();
			var result = (NegotiatedContentResult<string>)controller.RegisterV5(Guid.NewGuid(), GetValidContactInfo());
			AssertEquals(HttpStatusCode.BadRequest, result.StatusCode);
			AssertEquals("The registration link is not valid or has already been used. Please log in to BorderWise using the login details that were emailed to you, or request a new registration token.", result.Content);
		}

		public void TestRegisterV5_Error_CorruptToken()
		{
			AssertTokenValidation(null, "1");
		}

		public void TestRegisterV5_Error_MissingDate()
		{
			AssertTokenValidation("test@example.com", "1");
		}

		public void TestRegisterV5_Error_CorruptDate()
		{
			AssertTokenValidation("corrupt|test@example.com", "2");
		}

		public void TestRegisterV5_Error_CorruptOrgPk()
		{
			AssertTokenValidation(ZDateTime.UtcNow.AddHours(3).ToString(ZDateTime.LongTimeFormat, CultureInfo.InvariantCulture) + "|test@example.com|blah", "3");
		}

		public void TestRegisterV5_Should_ReturnForbidden_When_UserInputCompanyNameContainsRestrictedKeyWords()
		{
			var tokenPk = Guid.NewGuid();
			var result = (NegotiatedContentResult<string>)new BorderWiseRegistrationController().RegisterV5(tokenPk, GetValidContactInfo(orgName: "WiseTech Global test"));
			AssertEquals(HttpStatusCode.Forbidden, result.StatusCode);
			AssertEquals("'WiseTech' is reserved word for company name.", result.Content);
		}

		public void TestRegisterV5_Should_ReturnForbidden_When_SelectedOrgHasSameLicenseDatabaseAsWiseTechGlobal()
		{
			var token = CreateRegToken();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "WiseTech global related Org";
			orgHeader.OH_Code = "ABC";

			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_OH = orgHeader.PK;
			licenceEnterprise.LE_EnterpriseCode = "DEF";

			var licenceDatabase = Factory.New<LicenceDatabase>();
			licenceDatabase.LD_DatabaseNumber = 1;
			licenceDatabase.LD_LE = licenceEnterprise.PK;
			licenceDatabase.LD_ServerCode = "XXX";

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_OH = orgHeader.PK;
			clientCompany.LCC_LD = licenceDatabase.PK;
			clientCompany.LCC_Code = "XYZ";
			Factory.Save();

			var result = (NegotiatedContentResult<string>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(token.SD_Name.Replace("BorderWiseReg-", string.Empty)), GetValidContactInfo(orgPk: orgHeader.PK.ToGuid()));
			AssertEquals(HttpStatusCode.Forbidden, result.StatusCode);
			AssertEquals("Wrong organization selected.", result.Content);
		}

		public void TestRegisterV5_Should_Proceed_When_TheOrgNotLinkedByClientCompany()
		{
			var token = CreateRegToken();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Org";
			orgHeader.OH_Code = "ABC";

			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_OH = orgHeader.PK;
			licenceEnterprise.LE_EnterpriseCode = "DEF";

			var licenceDatabase = Factory.New<LicenceDatabase>();
			licenceDatabase.LD_DatabaseNumber = 1;
			licenceDatabase.LD_LE = licenceEnterprise.PK;
			licenceDatabase.LD_ServerCode = "XXX";

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_LD = licenceDatabase.PK;
			clientCompany.LCC_Code = "ABC";
			Factory.Save();

			var contactInfo = GetValidContactInfo(orgPk: orgHeader.PK.ToGuid());
			ValidateAddressActionForTesting = address => address.ValidationStatus = AddressValidationStatus.Verified;

			var result = new BorderWiseRegistrationController().RegisterV5(Guid.Parse(token.SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, orgHeader.PK));
			var contacts = org.Contacts.Cast<OrgContact>().Where(x => x.OC_ContactName == contactInfo.ContactName);
			AssertEquals(1, contacts.Count());
		}

		public void TestRegisterV5_Success_BrokerId()
		{
			AssertRegistrationSuccess(brokerId: "BrokerID-123");
		}

		[TestDate(2014, 9, 26)]
		public void TestRegisterV5_Success_Student_IdOnly()
		{
			AssertRegistrationSuccess(studentId: "Student-XYZ");
		}

		[TestDate(2014, 9, 26)]
		public void TestRegisterV5_Success_Student_IdAndInstitute()
		{
			AssertRegistrationSuccess(studentId: "Student-XYZ", studentInstitute: "UTS");
		}

		public void TestRegisterV5_Success_When_OrgPkExistsInToken_And_NotInContactInfo()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var existingOrgPk = orgHeader.PK.ToGuid();
			var tokenRecord = CreateRegToken(existingOrgPk);
			AssertRegistrationSuccess(existingOrgPk: existingOrgPk, orgNameForContactInfo: null);
		}

		[TestDate(2014, 9, 26)]
		public void TestRegisterV5_Student_WhenSucceded_ShouldExpireInSpecifiedTimeAndExplanationSent()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Indy and Lola Org";
			var contactInfo = GetValidContactInfo(orgPk: orgHeader.PK.ToGuid(), studentId: "Student-XYZ", studentInsitute: "UTS");
			ValidateAddressActionForTesting = address => address.ValidationStatus = AddressValidationStatus.Verified;
			Factory.Save();
			var tokenRecord = CreateRegToken(null);
			var result = (OkNegotiatedContentResult<RegistrationSuccessResponse>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(tokenRecord.SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);
			var orgName = string.IsNullOrEmpty(contactInfo.OrganisationName) ? contactInfo.ContactName : contactInfo.OrganisationName;
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, orgName));
			var contacts = org.Contacts.Cast<OrgContact>().Where(x => x.OC_ContactName == contactInfo.ContactName);
			AssertEquals(1, contacts.Count());
			var contact = contacts.Single();
			var cert = contact.Certificates.SingleOrDefault(x => x.XZ_Type == "MSC");
			AssertEquals(ZDateTime.Today.AddDays(CommonConstants.studentCertificateExpiryDays), cert.XZ_ExpiryOrDueDate);
			AssertStudentExplanationEmailSent();
		}

		[TestDate(2014, 9, 26)]
		public void TestRegisterV5_Success_Student_InstituteOnly()
		{
			AssertRegistrationSuccess(studentInstitute: "UTS");
		}

		[TestDate(2014, 9, 26)]
		public void TestRegisterV5_Success_StudentAndBroker()
		{
			AssertRegistrationSuccess(studentId: "Student-XYZ", studentInstitute: "UTS", brokerId: "Broker345");
		}

		public void TestRegisterV5_Success_NoBrokerOrStudent()
		{
			AssertRegistrationSuccess();
		}

		public void TestRegisterV5_Success_NoOrgName_UsesContactName()
		{
			AssertRegistrationSuccess(orgNameForContactInfo: string.Empty);
		}

		public void TestRegisterV5_Success_ExistingOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Latitude = -33;
			org.MainAddress.OA_Longitude = 151;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Someone Else";
			Factory.Save();
			AssertRegistrationSuccess(existingOrgPk: org.PK.ToGuid());
		}

		public void TestRegisterV5_Success_ExistingContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Latitude = -33;
			org.MainAddress.OA_Longitude = 151;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Aleera";
			Factory.Save();
			AssertRegistrationSuccess(organisationPk: org.PK.ToGuid(), isExistingContactName: true);
		}

		public void TestRegisterV5_Success_CloseAddress_GeoDistance()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Address1 = "Hello";
			Factory.Save();
			AssertRegistrationSuccess(organisationPk: org.PK.ToGuid(), shouldUpdateExistingAddress: false);
		}

		public void TestRegisterV5_Success_CloseAddress_Fuzzy()
		{
			var contactInfo = GetValidContactInfo();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Test Org";
			org.MainAddress.OA_Address1 = contactInfo.OrganisationAddress1;
			org.MainAddress.OA_Address2 = contactInfo.OrganisationAddress2.Substring(0, OrgAddressSchema.OA_Address2.MaxLength);
			org.MainAddress.OA_City = contactInfo.OrganisationCity;
			org.MainAddress.OA_State = contactInfo.OrganisationState;
			org.MainAddress.Postcode = contactInfo.OrganisationPostCode;
			org.MainAddress.OA_RN_NKCountryCode = contactInfo.OrganisationCountryCode;
			// to force it not to do a geo match
			org.MainAddress.OA_Latitude = -33;
			org.MainAddress.OA_Longitude = 151;
			Factory.Save();
			AssertRegistrationSuccess(organisationPk: org.PK.ToGuid(), shouldUpdateExistingAddress: true);
		}

		public void TestRegisterV5_Success_ExistingContact_NonTrimmedContactName()
		{
			var contact = CreateOrgAndContact();
			contact.OC_ContactName = "Aleera Appoo";
			contact.OC_Email = "aleera@appoo.net";
			contact.OC_WebAccessEnabled = false;
			contact.OC_PasswordHash = ZBlob.Empty;
			Factory.Save();
			var tokenRecord = CreateRegToken();
			var contactInfo = GetValidContactInfo(orgPk: contact.Header.PK.ToGuid());
			contactInfo.ContactName = "Aleera Appoo  ";
			ValidateAddressActionForTesting = address => address.ValidationStatus = AddressValidationStatus.Verified;
			var result = (OkNegotiatedContentResult<RegistrationSuccessResponse>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(tokenRecord.SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);
			AssertEquals("You have successfully registered as a BorderWise user. Your login details have been emailed to test@example.com.", result.Content.Message);
			AssertEquals("test@example.com", result.Content.RegisteredEmail);
			var reloadedContact = new BusinessObjectFactory().Load<OrgContact>(contact.PK);
			AssertEquals("Email updated with email used for registration", "aleera@appoo.net", reloadedContact.OC_Email);
			AssertEquals(false, reloadedContact.OC_WebAccessEnabled);
			AssertEquals(true, reloadedContact.OC_PasswordHash.IsEmpty);
			AssertEquals("new contact created", 2, reloadedContact.Header.Contacts.Count);
			var createdContact = new BusinessObjectFactory().Load<OrgContact>(result.Content.RegistrationInfo.OrgContactPk);
			AssertEquals(true, createdContact.OC_WebAccessEnabled);
			AssertEquals("Aleera Appoo (1)", createdContact.OC_ContactName);
		}

		public void TestRegisterV5_Success_Found_Matching_Org()
		{
			AssertRegistrationOrgDeduplication(null);
		}

		public void TestRegisterV5_Success_Matching_Org_For_Create_New_Org()
		{
			AssertRegistrationOrgDeduplication(Guid.Empty);
		}

		public void TestRegisterV5_Success_Found_Matching_Org_Should_Filter_Out_Existing_Active_Contact_Org()
		{
			var contact = CreateOrgAndContact();
			var existingOrgPk = contact.OC_OH.ToGuid();
			PotentialMatchOrgResultForTesting = GetOrgSimilarMatchesResult(existingOrgPk);
			var testOrg = PotentialMatchOrgResultForTesting.OrgMatches.First(oh => oh.PK != existingOrgPk);
			var tokenRecord = CreateRegToken(null, contact.OC_Email);
			var contactInfo = GetValidContactInfo(orgName: testOrg.FullName);
			var result = (OkNegotiatedContentResult<OrgSimilarMatchesResult>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(tokenRecord.SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);
			AssertNotNull(result.Content);
			AssertNotNull(result.Content.OrgMatches);
			AssertEquals(PotentialMatchOrgResultForTesting.OrgMatches.Count(oh => oh.PK != existingOrgPk), result.Content.OrgMatches.Count());
		}

		public void TestRegisterV5_Success_Skip_Matching_Org_For_Create_New_Org()
		{
			var testOrg = new OrgDetail { PK = Guid.NewGuid(), FullName = "Test Org" };
			PotentialMatchOrgResultForTesting = new OrgSimilarMatchesResult
			{
				OrgMatches = new List<OrgDetail>() { testOrg }
			};
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Latitude = -33;
			org.MainAddress.OA_Longitude = 151;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Someone Else";
			Factory.Save();
			AssertRegistrationSuccess(organisationPk: org.PK.ToGuid());
		}

		public void TestRegisterV5_AddressValidation_TopRecommendedAddress()
		{
			var validationResult = new WebAddressValidationResult();
			validationResult.ResultAddress = new ValidationResultItem { Address1 = "Myrtle Street", City = "Prospect", ResultStatusCode = ValidationResultStatusCode.PointClose, AvailableData = AvailableData.StreetNumber };
			AddressValidationResultForTesting = validationResult;
			ValidateAddressActionForTesting = address => address.ValidationStatus = AddressValidationStatus.Invalid;
			var token = CreateRegToken();
			var result = (OkNegotiatedContentResult<RegistrationValidationResponse>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(token.SD_Name.Replace("BorderWiseReg-", string.Empty)), GetValidContactInfo());
			AssertEquals(1, result.Content.AddressSuggestions.Count());
			AssertEquals(0, result.Content.Messages.Count());
			AssertEquals(0, result.Content.PhoneValidationMessages.Count());
			AssertNotNull(result.Content.AddressSuggestions.SingleOrDefault(x => x.Address1 == "Myrtle Street"));
			AssertEquals(false, token.IsDeleted);
		}

		public void TestRegisterV5_AddressValidation_Suggestions()
		{
			var validationResult = new WebAddressValidationResult();
			var suggestions = new List<ValidationResultItem>();
			suggestions.Add(new ValidationResultItem { Address1 = "Myrtle Street", City = "Prospect" });
			suggestions.Add(new ValidationResultItem { Address1 = "Henricks Avenue", City = "Newington" });
			validationResult.SuggestedResults = suggestions;
			AddressValidationResultForTesting = validationResult;
			ValidateAddressActionForTesting = address => address.ValidationStatus = AddressValidationStatus.Invalid;
			var token = CreateRegToken();
			var result = (OkNegotiatedContentResult<RegistrationValidationResponse>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(token.SD_Name.Replace("BorderWiseReg-", string.Empty)), GetValidContactInfo());
			AssertEquals(2, result.Content.AddressSuggestions.Count());
			AssertEquals(0, result.Content.Messages.Count());
			AssertEquals(0, result.Content.PhoneValidationMessages.Count());
			AssertNotNull(result.Content.AddressSuggestions.SingleOrDefault(x => x.Address1 == "Myrtle Street"));
			AssertNotNull(result.Content.AddressSuggestions.SingleOrDefault(x => x.Address1 == "Henricks Avenue"));
			AssertEquals(false, token.IsDeleted);
		}

		public void TestRegisterV5_AddressValidation_NoSuggestions()
		{
			var validationResult = new WebAddressValidationResult();
			validationResult.SuggestedResults = new List<ValidationResultItem>();
			AddressValidationResultForTesting = validationResult;
			ValidateAddressActionForTesting = address => address.ValidationStatus = AddressValidationStatus.Invalid;
			var token = CreateRegToken();
			var result = (OkNegotiatedContentResult<RegistrationValidationResponse>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(token.SD_Name.Replace("BorderWiseReg-", string.Empty)), GetValidContactInfo());
			AssertNull(result.Content.AddressSuggestions);
			AssertEquals(1, result.Content.Messages.Count());
			AssertEquals(0, result.Content.PhoneValidationMessages.Count());
			AssertEquals("The address you entered appears to be incorrect. Please check it carefully.", result.Content.Messages.Single());
			AssertEquals(false, token.IsDeleted);
		}

		public void TestRegisterV5_AddressValidation_ExactMatch()
		{
			var validationResult = new WebAddressValidationResult();
			validationResult.SuggestedResults = new List<ValidationResultItem>();
			AddressValidationResultForTesting = validationResult;
			ValidateAddressActionForTesting = address => address.ValidationStatus = AddressValidationStatus.Verified;
			var token = CreateRegToken();
			var result = (OkNegotiatedContentResult<RegistrationSuccessResponse>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(token.SD_Name.Replace("BorderWiseReg-", string.Empty)), GetValidContactInfo());
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "Indy and Lola Org"));
			AssertEquals(AddressValidationStatus.Verified, org.MainAddress.ValidationStatus);
			AssertEquals(true, token.IsDeleted);
		}

		public void TestRegisterV5_AddressValidation_ManuallyVerified()
		{
			var validationResult = new WebAddressValidationResult();
			validationResult.SuggestedResults = new List<ValidationResultItem>();
			AddressValidationResultForTesting = validationResult;
			ValidateAddressActionForTesting = address => address.ValidationStatus = AddressValidationStatus.Invalid;
			var result = (OkNegotiatedContentResult<RegistrationSuccessResponse>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(CreateRegToken().SD_Name.Replace("BorderWiseReg-", string.Empty)), GetValidContactInfo(), acceptUserAddress: true);
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "Indy and Lola Org"));
			AssertEquals(AddressValidationStatus.ManuallyVerified, org.MainAddress.ValidationStatus);
		}

		public void TestRegisterV5_PhoneValidation_Error()
		{
			var contactInfo = GetValidContactInfo();
			contactInfo.ContactMobilePhone = "04151";
			contactInfo.ContactWorkPhone = "931";
			contactInfo.OrganisationPhone = "23 5";
			var token = CreateRegToken();
			var result = (OkNegotiatedContentResult<RegistrationValidationResponse>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(token.SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);
			AssertEquals(@"Contact Phone: The phone number as entered has a high probability of being incorrect.

Please check the format and see below examples:
(02) 1234 5678 (local format)
+61 2 1234 5678 (international format)

---

Contact Mobile: The phone number as entered has a high probability of being incorrect.

Please check the format and see below examples:
(02) 1234 5678 (local format)
+61 2 1234 5678 (international format)

---

Organisation Phone: The phone number as entered has a high probability of being incorrect.

Please check the format and see below examples:
(02) 1234 5678 (local format)
+61 2 1234 5678 (international format)", string.Join("\r\n\r\n---\r\n\r\n", result.Content.PhoneValidationMessages));
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "Indy and Lola Org"));
			AssertNull(org);
			AssertEquals(false, token.IsDeleted);
		}

		public void TestRegisterV5_PhoneValidation_Error_PhoneBlank()
		{
			var contactInfo = GetValidContactInfo();
			contactInfo.ContactMobilePhone = "04151";
			contactInfo.ContactWorkPhone = "";
			contactInfo.OrganisationPhone = "23 5";
			var token = CreateRegToken();
			var result = (OkNegotiatedContentResult<RegistrationValidationResponse>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(token.SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);
			AssertEquals(@"Contact Mobile: The phone number as entered has a high probability of being incorrect.

Please check the format and see below examples:
(02) 1234 5678 (local format)
+61 2 1234 5678 (international format)

---

Organisation Phone: The phone number as entered has a high probability of being incorrect.

Please check the format and see below examples:
(02) 1234 5678 (local format)
+61 2 1234 5678 (international format)", string.Join("\r\n\r\n---\r\n\r\n", result.Content.PhoneValidationMessages));
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "Indy and Lola Org"));
			AssertNull(org);
			AssertEquals(false, token.IsDeleted);
		}

		public void TestRegisterV5_PhoneValidation_Error_ManuallyVerified()
		{
			var contactInfo = GetValidContactInfo();
			contactInfo.ContactMobilePhone = "04151";
			contactInfo.ContactWorkPhone = "71745";
			contactInfo.OrganisationPhone = "23 5";
			ValidateAddressActionForTesting = address => address.ValidationStatus = AddressValidationStatus.Verified;
			var token = CreateRegToken();
			var result = (OkNegotiatedContentResult<RegistrationSuccessResponse>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(token.SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo, acceptUserPhone: true);
			AssertEquals("You have successfully registered as a BorderWise user. Your login details have been emailed to test@example.com.", result.Content.Message);
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "Indy and Lola Org"));
			AssertEquals(true, org.MainAddress.OA_Phone_IsManuallyVerified);
			AssertEquals(true, org.Contacts[0].OC_Phone_IsManuallyVerified);
			AssertEquals(true, org.Contacts[0].OC_Mobile_IsManuallyVerified);
			AssertEquals(true, token.IsDeleted);
		}

		public void TestRegisterV5_AddressValidation_And_PhoneValidation()
		{
			var validationResult = new WebAddressValidationResult();
			var suggestions = new List<ValidationResultItem>();
			suggestions.Add(new ValidationResultItem { Address1 = "Myrtle Street", City = "Prospect" });
			suggestions.Add(new ValidationResultItem { Address1 = "Henricks Avenue", City = "Newington" });
			validationResult.SuggestedResults = suggestions;
			AddressValidationResultForTesting = validationResult;
			ValidateAddressActionForTesting = address => address.ValidationStatus = AddressValidationStatus.Invalid;
			var contactInfo = GetValidContactInfo();
			contactInfo.ContactMobilePhone = "04151";
			var token = CreateRegToken();
			var result = (OkNegotiatedContentResult<RegistrationValidationResponse>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(token.SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);
			AssertEquals(2, result.Content.AddressSuggestions.Count());
			AssertNotNull(result.Content.AddressSuggestions.SingleOrDefault(x => x.Address1 == "Myrtle Street"));
			AssertNotNull(result.Content.AddressSuggestions.SingleOrDefault(x => x.Address1 == "Henricks Avenue"));
			AssertEquals(@"Contact Mobile: The phone number as entered has a high probability of being incorrect.

Please check the format and see below examples:
(02) 1234 5678 (local format)
+61 2 1234 5678 (international format)", string.Join("\r\n\r\n---\r\n\r\n", result.Content.PhoneValidationMessages));
			AssertEquals(false, token.IsDeleted);
		}

		public void TestRegisterV5_BillingContact_Specified_NewOrg()
		{
			var contactInfo = GetValidContactInfo();
			contactInfo.BillingContactName = "John Smith";
			contactInfo.BillingContactEmail = "billing@example.com";
			ValidateAddressActionForTesting = address => address.ValidationStatus = AddressValidationStatus.Verified;
			var result = (OkNegotiatedContentResult<RegistrationSuccessResponse>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(CreateRegToken().SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);
			AssertEquals("You have successfully registered as a BorderWise user. Your login details have been emailed to test@example.com.", result.Content.Message);
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "Indy and Lola Org"));
			AssertEquals(2, org.Contacts.Count);
			var billingContact = org.Contacts.Cast<OrgContact>().Single(x => x.OC_Email == "billing@example.com");
			AssertEquals("John Smith", billingContact.OC_ContactName);
			AssertEquals("BorderWise Reg", billingContact.OC_ContactSource);
			AssertNotNull(billingContact.Documents.Cast<OrgDocument>().Single(x => x.OD_DocumentGroup == ContactType.Receivables.ToString() && x.OD_DefaultContact));
			AssertNotNull(billingContact.Documents.Cast<OrgDocument>().Single(x => x.OD_DocumentGroup == EDIOrgDocumentGroupTypes.Codes.BorderWiseAdministrator && x.OD_DefaultContact));
			AssertEquals(billingContact.PK, result.Content.RegistrationInfo.BillingContactPk);
			var mainContact = org.Contacts.Cast<OrgContact>().Single(x => x.OC_Email == "test@example.com");
			AssertEquals(0, mainContact.Documents.Count);
		}

		public void TestRegisterV5_BillingContact_Specified_ExistingOrg()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Indy and Lola Org";
			var contactInfo = GetValidContactInfo(orgPk: orgHeader.PK.ToGuid());
			contactInfo.BillingContactName = "John Smith";
			contactInfo.BillingContactEmail = "billing@example.com";
			ValidateAddressActionForTesting = address => address.ValidationStatus = AddressValidationStatus.Verified;
			Factory.Save();
			var result = (OkNegotiatedContentResult<RegistrationSuccessResponse>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(CreateRegToken(orgHeader.PK.ToGuid()).SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);
			AssertEquals("You have successfully registered as a BorderWise user. Your login details have been emailed to test@example.com.", result.Content.Message);
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "Indy and Lola Org"));
			AssertEquals(1, org.Contacts.Count);
			AssertEquals(0, org.Contacts.Cast<OrgContact>().Single().Documents.Count);
			AssertEquals(Guid.Empty, result.Content.RegistrationInfo.BillingContactPk);
		}

		public void TestRegisterV5_BillingContact_NotSpecified_NewOrg()
		{
			var contactInfo = GetValidContactInfo();
			ValidateAddressActionForTesting = address => address.ValidationStatus = AddressValidationStatus.Verified;
			var result = (OkNegotiatedContentResult<RegistrationSuccessResponse>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(CreateRegToken().SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);
			AssertEquals("You have successfully registered as a BorderWise user. Your login details have been emailed to test@example.com.", result.Content.Message);
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "Indy and Lola Org"));
			AssertEquals(1, org.Contacts.Count);
			var contact = org.Contacts.Cast<OrgContact>().Single(x => x.OC_Email == "test@example.com");
			AssertNotNull(contact.Documents.Cast<OrgDocument>().Single(x => x.OD_DocumentGroup == ContactType.Receivables.ToString() && x.OD_DefaultContact));
			AssertNotNull(contact.Documents.Cast<OrgDocument>().Single(x => x.OD_DocumentGroup == EDIOrgDocumentGroupTypes.Codes.BorderWiseAdministrator && x.OD_DefaultContact));
			AssertEquals(Guid.Empty, result.Content.RegistrationInfo.BillingContactPk);
		}

		public void TestRegisterV5_BillingContact_NotSpecified_ExistingOrg()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Indy and Lola Org";
			var contactInfo = GetValidContactInfo(orgPk: orgHeader.PK.ToGuid());
			ValidateAddressActionForTesting = address => address.ValidationStatus = AddressValidationStatus.Verified;
			Factory.Save();
			var result = (OkNegotiatedContentResult<RegistrationSuccessResponse>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(CreateRegToken(orgHeader.PK.ToGuid()).SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);
			AssertEquals("You have successfully registered as a BorderWise user. Your login details have been emailed to test@example.com.", result.Content.Message);
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "Indy and Lola Org"));
			AssertEquals(1, org.Contacts.Count);
			var contact = org.Contacts.Cast<OrgContact>().Single(x => x.OC_Email == "test@example.com");
			AssertEquals(0, contact.Documents.Count);
			AssertEquals(Guid.Empty, result.Content.RegistrationInfo.BillingContactPk);
		}

		public void TestRegisterV5_Error_When_UserIsInactiveInSelectedOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_IsActive = false;
			contact.OC_ContactName = "Inactive user";
			contact.OC_Email = "test@example.com";
			Factory.Save();
			var contactInfo = GetValidContactInfo(orgPk: org.PK.ToGuid(), contactName: contact.OC_ContactName);
			var result = (BadRequestErrorMessageResult)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(CreateRegToken(org.PK.ToGuid()).SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);
			var expectedErrorMessage = $"The account associated with the email (test@example.com) was deactivated in the organization ({contactInfo.OrganisationName}) and can't be used to register to BorderWise. Please contact the organization administrator or register with the correct organization/email address. You can also contact support@borderwise.com if further support is needed.";
			AssertEquals(expectedErrorMessage, result.Message);
		}

		public void TestRegisterV5_Success_When_InactiveUserWithNewOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_IsActive = false;
			contact.OC_ContactName = "Inactive user";
			contact.OC_Email = "test@example.com";
			Factory.Save();
			AssertRegistrationSuccess(contactName: "Inactive user");
		}

		public void TestRegisterV5_Success_When_InactiveUserSelectDifferentOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_IsActive = false;
			contact.OC_ContactName = "Inactive user";
			contact.OC_Email = "test@example.com";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			AssertRegistrationSuccess(organisationPk: org2.PK.ToGuid(), contactName: "Inactive user");
		}

		public void TestRegisterV5_Success_For_NewOrg()
		{
			// Arrange
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.MainAddress.OA_Email = "organisation@example.com";
			org2.OH_FullName = "Existing Org";
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_IsActive = true;
			contact2.OC_ContactName = "Active user";
			contact2.OC_Email = "another@example.com";
			Factory.Save();

			var contactInfo = new ContactInfo
			{
				OrganisationName = "New Org",
				OrganisationAddress1 = "123 New Street",
				OrganisationCity = "Newington",
				OrganisationState = "NSW",
				OrganisationPostCode = "2127",
				OrganisationCountryCode = "AU",
				OrganisationPhone = "+61 2 1234 5678",
				OrganisationEmail = "test@example.com",
				ContactName = "John Doe",
				ContactWorkPhone = "+61 2 9876 5432",
				ContactMobilePhone = "+61 400 123 456",
				ContactPassword = "SecurePassword#123",
				ContactJobTitle = "Manager",
				ContactDOB = new DateTime(1985, 5, 15),
			};

			var token = CreateRegToken();

			// Act
			var controller = new BorderWiseRegistrationController();
			var successResult = (OkNegotiatedContentResult<RegistrationSuccessResponse>)controller.RegisterV5(Guid.Parse(token.SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo, acceptUserAddress: true, acceptUserPhone: true);

			// Assert
			AssertNotNull(successResult);
			AssertEquals("You have successfully registered as a BorderWise user. Your login details have been emailed to test@example.com.", successResult.Content.Message);
			AssertEquals("test@example.com", successResult.Content.RegisteredEmail);

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "New Org"));
			AssertNotNull(org);
			AssertEquals("New Org", org.OH_FullName);
			AssertEquals("123 New Street", org.MainAddress.OA_Address1);
			AssertEquals("Newington", org.MainAddress.OA_City);
			AssertEquals("NSW", org.MainAddress.OA_State);
			AssertEquals("2127", org.MainAddress.OA_PostCode);
			AssertEquals("AU", org.MainAddress.OA_RN_NKCountryCode);
			AssertEquals("+61 2 1234 5678", org.MainAddress.OA_Phone);
			AssertEquals("test@example.com", org.MainAddress.OA_Email);

			var contact = org.Contacts.Cast<OrgContact>().SingleOrDefault(c => c.OC_ContactName == "John Doe");
			AssertNotNull(contact);
			AssertEquals("John Doe", contact.OC_ContactName);
			AssertEquals("+61298765432", contact.OC_Phone);
			AssertEquals("+61400123456", contact.OC_Mobile);
			AssertEquals("Manager", contact.OC_Title);
			AssertEquals(new DateTime(1985, 5, 15), contact.OC_Birthday);
			AssertEquals("test@example.com", contact.OC_Email);
			AssertEquals(true, contact.OC_WebAccessEnabled);

			// Verify the email notification
			var email = Env.OutgoingMailManager.EmailsCreated.SingleOrDefault(e => e.Subject.Contains("BorderWise New Organisation - New Org"));
			AssertNotNull(email);
			AssertContains("New Org", email.Body);
			AssertContains("test@example.com", email.Body);
			AssertContains(org2.OH_FullName, email.Body);
			AssertContains(org2.OH_Code, email.Body);
			AssertContains("has been used in 1 organization(s)", email.Body);
		}

		#endregion

		#region RegisterByAdmin
		public void TestRegisterByAdmin_Should_ReturnBadRequest_When_InvalidParams()
		{
			var controller = new BorderWiseRegistrationController();
			var contactInfo = new ContactInfo();
			contactInfo.OrganisationPk = Factory.NewWithValidTestData<OrgHeader>().PK.ToGuid();
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterByAdmin(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.OrganisationName = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterByAdmin(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.OrganisationAddress1 = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterByAdmin(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.OrganisationCity = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterByAdmin(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.OrganisationState = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterByAdmin(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.OrganisationPostCode = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterByAdmin(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.OrganisationCountryCode = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterByAdmin(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.OrganisationPhone = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterByAdmin(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.OrgCode = "Test";
			contactInfo.OrganisationName = "";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterByAdmin(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.ContactName = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterByAdmin(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.ContactDOB = DateTime.Now;
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.RegisterByAdmin(Guid.NewGuid(), contactInfo)).Message);
			contactInfo.ContactPassword = "SomePassword$123";
			contactInfo.OrganisationPk = new Guid("{cdfbed2d-06ab-4e6a-b250-d24a38024f67}");
			Factory.Save();
			AssertEquals(DecodeHtml($"Organisation not found with Organisation Pk: {contactInfo.OrganisationPk.Value}"), DecodeHtml(((BadRequestErrorMessageResult)controller.RegisterByAdmin(Guid.Parse(CreateRegToken(contactInfo.OrganisationPk).SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo)).Message));
		}

		public void TestRegisterByAdmin_Should_ReturnBadRequest_When_InvalidPassword()
		{
			var controller = new BorderWiseRegistrationController();
			var contactInfo = GetValidContactInfo();
			contactInfo.OrganisationPk = Factory.NewWithValidTestData<OrgHeader>().PK.ToGuid();
			contactInfo.ContactPassword = "password";
			var result = (BadRequestErrorMessageResult)controller.RegisterByAdmin(Guid.NewGuid(), contactInfo);
			AssertEquals("Password must be at least 12 characters long.", result.Message);
			contactInfo.ContactPassword = "somepassword";
			result = (BadRequestErrorMessageResult)controller.RegisterByAdmin(Guid.NewGuid(), contactInfo);
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", result.Message);
			contactInfo.ContactName = "some one";
			contactInfo.ContactPassword = "SomePassword#123";
			result = (BadRequestErrorMessageResult)controller.RegisterByAdmin(Guid.NewGuid(), contactInfo);
			AssertEquals("The password appears to contain part of your name, email or a word which has been disallowed by your system administrator.", result.Message);
		}

		public void TestRegisterByAdmin_Should_ReturnForbidden_When_UserInputCompanyNameContainsRestrictedKeyWords()
		{
			var contactInfo = GetValidContactInfo();
			contactInfo.OrganisationPk = Factory.NewWithValidTestData<OrgHeader>().PK.ToGuid();
			contactInfo.OrganisationName = "WiseTech Global test";
			var result = (NegotiatedContentResult<string>)new BorderWiseRegistrationController().RegisterByAdmin(Guid.NewGuid(), contactInfo);
			AssertEquals(HttpStatusCode.Forbidden, result.StatusCode);
			AssertEquals("'WiseTech' is reserved word for company name.", result.Content);
		}

		public void TestRegisterByAdmin_Should_ReturnForbidden_When_SelectedOrgHasSameLicenseDatabaseAsWiseTechGlobal()
		{
			var token = CreateRegToken();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "WiseTech global related Org";
			orgHeader.OH_Code = "ABC";

			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_OH = orgHeader.PK;
			licenceEnterprise.LE_EnterpriseCode = "DEF";

			var licenceDatabase = Factory.New<LicenceDatabase>();
			licenceDatabase.LD_DatabaseNumber = 1;
			licenceDatabase.LD_LE = licenceEnterprise.PK;
			licenceDatabase.LD_ServerCode = "XXX";

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_OH = orgHeader.PK;
			clientCompany.LCC_LD = licenceDatabase.PK;
			clientCompany.LCC_Code = "XYZ";
			Factory.Save();

			var result = (NegotiatedContentResult<string>)new BorderWiseRegistrationController().RegisterByAdmin(Guid.Parse(token.SD_Name.Replace("BorderWiseReg-", string.Empty)), GetValidContactInfo(orgPk: orgHeader.PK.ToGuid()));
			AssertEquals(HttpStatusCode.Forbidden, result.StatusCode);
			AssertEquals("Wrong organization selected.", result.Content);
		}

		public void TestRegisterByAdmin_Should_ReturnBadRequest_When_UserIsInactiveInSelectedOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_IsActive = false;
			contact.OC_ContactName = "Inactive user";
			contact.OC_Email = "test@example.com";
			Factory.Save();

			var contactInfo = GetValidContactInfo(orgPk: org.PK.ToGuid(), contactName: contact.OC_ContactName);
			var result = (BadRequestErrorMessageResult)new BorderWiseRegistrationController().RegisterByAdmin(Guid.Parse(CreateRegToken(org.PK.ToGuid()).SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);
			var expectedErrorMessage = $"The account associated with the email (test@example.com) was deactivated in the organization ({contactInfo.OrganisationName}) and can't be used to register to BorderWise. Please contact the organization administrator or register with the correct organization/email address. You can also contact support@borderwise.com if further support is needed.";
			AssertEquals(expectedErrorMessage, result.Message);
		}

		public void TestRegisterByAdmin_Should_ReturnBadRequest_When_OrgDoesNotExist()
		{
			var invalidOrgPk = new Guid("{cdfb3d2d-06a9-4e6a-b250-d24a380aef67}");
			var contactInfo = GetValidContactInfo(orgPk: invalidOrgPk);
			var result = ((BadRequestErrorMessageResult)new BorderWiseRegistrationController().RegisterByAdmin(Guid.Parse(CreateRegToken(invalidOrgPk).SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo));
			AssertEquals($"Organisation not found with Organisation Pk: {invalidOrgPk}", result.Message);
		}

		public void TestRegisterByAdmin_Should_ReturnBadRequest_When_OrganisationPkIsNull()
		{
			var contactInfo = GetValidContactInfo();
			contactInfo.OrganisationPk = null;
			AssertEquals("OrganisationPk cannot be null.", ((BadRequestErrorMessageResult)new BorderWiseRegistrationController().RegisterByAdmin(Guid.NewGuid(), contactInfo)).Message);
		}

		public void TestRegisterByAdmin_Should_Succeed()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contactInfo = GetValidContactInfo(orgPk: org.PK.ToGuid());
			Factory.Save();

			var result = (OkNegotiatedContentResult<RegistrationSuccessResponse>)new BorderWiseRegistrationController().RegisterByAdmin(Guid.Parse(CreateRegToken(org.PK.ToGuid()).SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);

			AssertEquals(DecodeHtml("You have successfully registered as a BorderWise user. Your login details have been emailed to test@example.com."), DecodeHtml(result.Content.Message));
			AssertEquals("test@example.com", result.Content.RegisteredEmail);
			ValidateAddressActionForTesting = address => address.ValidationStatus = AddressValidationStatus.Verified;

			org = Factory.Load<OrgHeader>(contactInfo.OrganisationPk.Value);
			var contacts = org.Contacts.Cast<OrgContact>().Where(x => x.OC_ContactName == contactInfo.ContactName);
			var contact = contacts.Single();
			AssertEquals(org.MainAddress.PK, contact.OC_OA_OrgAddress);
			var email = Env.OutgoingMailManager.EmailsCreated.SingleOrDefault(x => x.Subject == "BorderWise New Organisation - " + org.OH_FullName);
			AssertNull(email);

			AssertEquals(true, contact.OC_WebAccessEnabled);
			AssertEquals("BorderWise Reg", contact.OC_ContactSource);
			AssertEquals(contactInfo.ContactWorkPhone.Replace(" ", string.Empty), contact.OC_Phone);
			AssertEquals(contactInfo.ContactMobilePhone.Replace(" ", string.Empty), contact.OC_Mobile);
			AssertEquals(contactInfo.ContactJobTitle, contact.OC_Title);
			AssertEquals(contactInfo.ContactDOB, contact.OC_Birthday);
			AssertEquals("test@example.com", contact.OC_Email);
			AssertNotNull(result.Content.RegistrationInfo);
			var registrationInfo = result.Content.RegistrationInfo;
			AssertEquals(contact.PK, registrationInfo.OrgContactPk);
			AssertEquals(org.PK, registrationInfo.OrgHeaderPk);
			AssertEquals(org.OH_Code, registrationInfo.OhCode);
			AssertEquals(contact.OrgAddress.PK, registrationInfo.OrgHeaderAddressPk);
			AssertEquals(contact.OC_PasswordHash, registrationInfo.PasswordHash.ToArray());
			AssertEquals(contact.OC_PasswordSalt, registrationInfo.PasswordSalt.ToArray());
			AssertEquals(contact.OC_PasswordHashIterations, registrationInfo.PasswordHashIterations);
			AssertEquals(contact.OC_Email, registrationInfo.ContactEmail);
			AssertEquals(true, contact.OC_IsActive);
		}

		#endregion

		#region ValidateRegistrationInfo
		public void TestValidateRegistrationInfo_Should_NotBeSuccessful_When_InvalidParams()
		{
			var token = CreateRegToken();
			var tokenValue = Guid.Parse(token.SD_Name.Replace("BorderWiseReg-", string.Empty));
			var controller = new BorderWiseRegistrationController();
			AssertEquals("All arguments must be provided.", ((NegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(tokenValue, null)).Content.Message);
			var contactInfo = new ContactInfo();
			AssertEquals("All arguments must be provided.", ((NegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(tokenValue, contactInfo)).Content.Message);
			contactInfo.OrganisationName = "Test";
			AssertEquals("All arguments must be provided.", ((NegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(tokenValue, contactInfo)).Content.Message);
			contactInfo.OrganisationAddress1 = "Test";
			AssertEquals("All arguments must be provided.", ((NegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(tokenValue, contactInfo)).Content.Message);
			contactInfo.OrganisationCity = "Test";
			AssertEquals("All arguments must be provided.", ((NegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(tokenValue, contactInfo)).Content.Message);
			contactInfo.OrganisationState = "Test";
			AssertEquals("All arguments must be provided.", ((NegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(tokenValue, contactInfo)).Content.Message);
			contactInfo.OrganisationPostCode = "Test";
			AssertEquals("All arguments must be provided.", ((NegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(tokenValue, contactInfo)).Content.Message);
			contactInfo.OrganisationCountryCode = "Test";
			AssertEquals("All arguments must be provided.", ((NegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(tokenValue, contactInfo)).Content.Message);
			contactInfo.OrganisationPhone = "Test";
			AssertEquals("All arguments must be provided.", ((NegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(tokenValue, contactInfo)).Content.Message);
			contactInfo.OrgCode = "Test";
			contactInfo.OrganisationName = "";
			AssertEquals("All arguments must be provided.", ((NegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(tokenValue, contactInfo)).Content.Message);
			contactInfo.ContactName = "Test";
			AssertEquals("All arguments must be provided.", ((NegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(tokenValue, contactInfo)).Content.Message);
			contactInfo.ContactDOB = DateTime.Now;
			AssertEquals("All arguments must be provided.", ((NegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(tokenValue, contactInfo)).Content.Message);
			contactInfo.ContactPassword = "SomePassword$123";
			var result = (NegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(Guid.NewGuid(), contactInfo);
			AssertEquals("The registration link is not valid or has already been used. Please log in to BorderWise using the login details that were emailed to you, or request a new registration token.", result.Content.Message);
		}

		public void TestValidateRegistrationInfo_Should_NotBeSuccessful_When_InvalidPassword()
		{
			var token = CreateRegToken();
			var tokenValue = Guid.Parse(token.SD_Name.Replace("BorderWiseReg-", string.Empty));
			var controller = new BorderWiseRegistrationController();
			var contactInfo = GetValidContactInfo();
			contactInfo.ContactPassword = "password";
			var result = (NegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(tokenValue, contactInfo);
			AssertEquals(true, result.Content.HasErrors);
			AssertEquals("Password must be at least 12 characters long.", result.Content.Message);
			contactInfo.ContactPassword = "somepassword";
			result = (NegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(tokenValue, contactInfo);
			AssertEquals(true, result.Content.HasErrors);
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", result.Content.Message);
			contactInfo.ContactName = "some one";
			contactInfo.ContactPassword = "SomePassword#123";
			result = (NegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(tokenValue, contactInfo);
			AssertEquals(true, result.Content.HasErrors);
			AssertEquals("The password appears to contain part of your name, email or a word which has been disallowed by your system administrator.", result.Content.Message);
		}

		public void TestValidateRegistrationInfo_Should_NotBeSuccessful_When_OrganisationPkIsInvalid()
		{
			var token = CreateRegToken();
			var tokenValue = Guid.Parse(token.SD_Name.Replace("BorderWiseReg-", string.Empty));
			var controller = new BorderWiseRegistrationController();
			var contactInfo = GetValidContactInfo();

			contactInfo.OrganisationPk = null;
			var result = (NegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(tokenValue, contactInfo);
			AssertEquals(true, result.Content.HasErrors);
			AssertEquals("Organisation identifier is missing or invalid.", result.Content.Message);

			contactInfo.OrganisationPk = Guid.Empty;
			result = (NegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(tokenValue, contactInfo);
			AssertEquals(true, result.Content.HasErrors);
			AssertEquals("Organisation identifier is missing or invalid.", result.Content.Message);
		}

		public void TestValidateRegistrationInfo_Should_NotBeSuccessful_When_ExistingOrgPkIsNotSameAsOrgPk()
		{
			var contactInfo = GetValidContactInfo();
			contactInfo.OrganisationPk = Guid.NewGuid();
			var existingOrgPk = Guid.NewGuid();
			var token = CreateRegToken(existingOrgPk);

			var result = (NegotiatedContentResult<ValidationResponse>)new BorderWiseRegistrationController().ValidateRegistrationInfo(Guid.Parse(token.SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);
			AssertEquals(true, result.Content.HasErrors);
			AssertEquals("Account invitation linked to a different organisation.", result.Content.Message);
		}

		public void TestValidateRegistrationInfo_Should_NotBeSuccessful_When_UserInputCompanyNameContainsRestrictedKeyWords()
		{
			var token = CreateRegToken();
			var tokenValue = Guid.Parse(token.SD_Name.Replace("BorderWiseReg-", string.Empty));
			var result = (NegotiatedContentResult<ValidationResponse>)new BorderWiseRegistrationController().ValidateRegistrationInfo(tokenValue, GetValidContactInfo(orgName: "WiseTech Global test"));
			AssertEquals(true, result.Content.HasErrors);
			AssertEquals("'WiseTech' is reserved word for company name.", result.Content.Message);
		}

		public void TestValidateRegistrationInfo_Should_NotBeSuccessful_When_SelectedOrgHasSameLicenseDatabaseAsWiseTechGlobal()
		{
			var token = CreateRegToken();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "WiseTech global related Org";
			orgHeader.OH_Code = "ABC";

			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_OH = orgHeader.PK;
			licenceEnterprise.LE_EnterpriseCode = "DEF";

			var licenceDatabase = Factory.New<LicenceDatabase>();
			licenceDatabase.LD_DatabaseNumber = 1;
			licenceDatabase.LD_LE = licenceEnterprise.PK;
			licenceDatabase.LD_ServerCode = "XXX";

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_OH = orgHeader.PK;
			clientCompany.LCC_LD = licenceDatabase.PK;
			clientCompany.LCC_Code = "XYZ";
			Factory.Save();

			var excludedOrgPks = new BorderWiseRegistrationController().GetAllExcludedOrganizationPks(Factory, string.Empty, string.Empty);
			var result = (NegotiatedContentResult<ValidationResponse>)new BorderWiseRegistrationController().ValidateRegistrationInfo(Guid.Parse(token.SD_Name.Replace("BorderWiseReg-", string.Empty)), GetValidContactInfo(orgPk: orgHeader.PK.ToGuid()));
			AssertEquals(true, result.Content.HasErrors);
			AssertEquals("Wrong organization selected.", result.Content.Message);
		}

		public void TestValidateRegistrationInfo_Should_ReturnBadRequest_When_UserIsInactiveInSelectedOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_IsActive = false;
			contact.OC_ContactName = "Inactive user";
			contact.OC_Email = "test@example.com";
			Factory.Save();

			var contactInfo = GetValidContactInfo(orgPk: org.PK.ToGuid(), contactName: contact.OC_ContactName);
			var result = (NegotiatedContentResult<ValidationResponse>)new BorderWiseRegistrationController().ValidateRegistrationInfo(Guid.Parse(CreateRegToken(org.PK.ToGuid()).SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);
			AssertEquals(true, result.Content.HasErrors);
			var expectedErrorMessage = $"The account associated with the email (test@example.com) was deactivated in the organization ({contactInfo.OrganisationName}) and can't be used to register to BorderWise. Please contact the organization administrator or register with the correct organization/email address. You can also contact support@borderwise.com if further support is needed.";
			AssertEquals(expectedErrorMessage, result.Content.Message);
		}

		public void TestValidateRegistrationInfo_Should_Succeed()
		{
			// Arrange
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contactEmail = "testuser@testorg.com";
			var registrationToken = CreateRegToken(org.PK.ToGuid(), contactEmail);
			Factory.Save();

			var contactInfo = new ContactInfo
			{
				ContactName = "Test Contact",
				OrganisationName = "Valid Organisation",
				OrganisationPk = org.PK.ToGuid(),
				OrganisationAddress1 = "Test",
				OrganisationCity = "Test",
				OrganisationState = "Test",
				OrganisationPostCode = "Test",
				OrganisationCountryCode = "Test",
				OrganisationPhone = "Test",
				OrgCode = "Test",
				ContactDOB = DateTime.Now,
				ContactPassword = "SomePassword$123"
			};

			// Act
			var controller = new BorderWiseRegistrationController();
			var result = (OkNegotiatedContentResult<ValidationResponse>)controller.ValidateRegistrationInfo(Guid.Parse(registrationToken.SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);

			// Assert
			AssertEquals("Registration info validation succeeded.", result.Content.Message);
			AssertEquals(false, result.Content.HasErrors);
			AssertEquals(contactEmail, result.Content.RegisterEmail);
			AssertEquals(false, registrationToken.IsDeleted);
		}

		#endregion

		#region GetOrganizationPksUnderWiseTechGlobalCompany

		public void TestGetOrganizationPksUnderWiseTechGlobalCompany_Should_ReturnCorrectResult()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "WiseTech global related Org";
			orgHeader.OH_Code = "ABC";

			var orgHeaderNotLinked = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderNotLinked.OH_FullName = "Not linked Org";
			orgHeaderNotLinked.OH_Code = "XYZ";

			var orgHeaderNotRelated = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderNotRelated.OH_FullName = "Not related Org";
			orgHeaderNotRelated.OH_Code = "ABCD";

			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_OH = orgHeader.PK;
			licenceEnterprise.LE_EnterpriseCode = "DEF";

			var licenceDatabase = Factory.New<LicenceDatabase>();
			licenceDatabase.LD_DatabaseNumber = 1;
			licenceDatabase.LD_LE = licenceEnterprise.PK;
			licenceDatabase.LD_ServerCode = "XXX";

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_OH = orgHeader.PK;
			clientCompany.LCC_LD = licenceDatabase.PK;
			clientCompany.LCC_Code = "ABC";

			var clientCompany1 = Factory.New<ClientCompany>();
			clientCompany1.LCC_LD = licenceDatabase.PK;
			clientCompany1.LCC_Code = "XYZ";

			Factory.Save();

			var result = new BorderWiseRegistrationController().GetOrganizationPksUnderWiseTechGlobalCompany(string.Empty, string.Empty);

			AssertEquals(1, result.Count());
			AssertEquals(orgHeader.PK.ToGuid(), result.First());
		}

		#endregion

		#region GetPksForExcludedOrganizationList

		public void TestGetPksForExcludedOrganizationList_Should_ReturnCorrectResult()
		{
			var orgHeaderNotRelated = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderNotRelated.OH_FullName = "Not related Org";
			orgHeaderNotRelated.OH_Code = "ABCD";

			var orgWiseLearning = Factory.NewWithValidTestData<OrgHeader>();
			orgWiseLearning.OH_FullName = "WiseTech Learning";
			orgWiseLearning.OH_Code = "EDUSYDACC";

			Factory.Save();

			var result = new BorderWiseRegistrationController().GetPksForExcludedOrganizationList(Factory);

			AssertEquals(1, result.Count());
			AssertEquals(orgWiseLearning.PK.ToGuid(), result.First());
		}

		#endregion

		#region Org Deduplication service
		public void TestFindMatchingOrganisation_Error_InvalidParameters()
		{
			var controller = new BorderWiseRegistrationController();
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.FindMatchingOrganisation(null)).Message);
			var orgInfo = new OrganisationInfo();
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.FindMatchingOrganisation(orgInfo)).Message);
			orgInfo.OrganisationAddress1 = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.FindMatchingOrganisation(orgInfo)).Message);
			orgInfo.OrganisationCity = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.FindMatchingOrganisation(orgInfo)).Message);
			orgInfo.OrganisationState = "Test";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.FindMatchingOrganisation(orgInfo)).Message);
			orgInfo.OrganisationPostCode = "2000";
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.FindMatchingOrganisation(orgInfo)).Message);
			orgInfo.OrganisationCountryCode = "Test";
			orgInfo.OrganisationPk = Guid.NewGuid();
			var result = ((OkNegotiatedContentResult<OrgSimilarMatchesResult>)controller.FindMatchingOrganisation(orgInfo));
			AssertEquals(null, result.Content);
		}

		public void TestFindMatchingOrganisation_ShouldBeSuccess_WhenOrgPkExists()
		{
			var controller = new BorderWiseRegistrationController();
			var orgInfo = new OrganisationInfo()
			{ OrganisationAddress1 = "Test", OrganisationCity = "Sydney", OrganisationState = "NSW", OrganisationCountryCode = "AU", OrganisationPostCode = "2000", OrganisationPk = Guid.NewGuid() };
			var result = ((OkNegotiatedContentResult<OrgSimilarMatchesResult>)controller.FindMatchingOrganisation(orgInfo));
			AssertEquals(null, result.Content);
		}

		public void TestFindMatchingOrganisation_ShouldBeSuccess_WhenNoOrgPkAndOrgName()
		{
			var controller = new BorderWiseRegistrationController();
			var orgInfo = new OrganisationInfo()
			{ OrganisationAddress1 = "Test", OrganisationCity = "Sydney", OrganisationState = "NSW", OrganisationCountryCode = "AU", OrganisationPostCode = "2000", };
			var result = ((OkNegotiatedContentResult<OrgSimilarMatchesResult>)controller.FindMatchingOrganisation(orgInfo));
			AssertEquals(null, result.Content);
		}

		public void TestFindPotentialOrgs_No_Org_Found_Success()
		{
			var master = GetOrgHeader();
			Factory.Save();
			var contactInfo = new ContactInfo { OrganisationName = "New Org", OrganisationAddress1 = "New address", OrganisationCountryCode = "AU", OrganisationCity = master.MainAddress.OA_City, OrganisationPostCode = master.MainAddress.OA_PostCode, OrganisationState = master.MainAddress.OA_State, OrganisationPhone = "123456" };
			var controller = new BorderWiseRegistrationController();
			var result = controller.FindPotentialMatchOrgs(Factory, contactInfo);
			AssertNull(result);
		}

		public void TestFindPotentialOrgs_Org_Found_Success()
		{
			var master = GetOrgHeader();
			Factory.Save();
			var contactInfo = new ContactInfo { OrganisationName = master.OH_FullName, OrganisationAddress1 = master.MainAddress.OA_Address1, OrganisationCountryCode = "AU", OrganisationCity = master.MainAddress.OA_City, OrganisationPostCode = master.MainAddress.OA_PostCode, OrganisationState = master.MainAddress.OA_State, OrganisationPhone = "123456" };
			var controller = new BorderWiseRegistrationController();
			var result = controller.FindPotentialMatchOrgs(Factory, contactInfo);
			AssertNotNull(result);
			AssertNotNull(result.OrgMatches);
			AssertEquals(1, result.OrgMatches.Count());
			AssertEquals(master.PK, result.OrgMatches.First().PK);
			AssertEquals($"{master.OH_FullName} ({master.OH_Code})", result.OrgMatches.First().FullName);
		}

		public void TestFindPotentialOrgs_Org_Found_Success_When_OrgName_Not_Provided()
		{
			var master = GetOrgHeader();
			Factory.Save();
			var contactInfo = new ContactInfo { OrganisationAddress1 = master.MainAddress.OA_Address1, OrganisationAddress2 = master.MainAddress.OA_Address2, OrganisationCountryCode = "AU", OrganisationCity = master.MainAddress.OA_City, OrganisationPostCode = master.MainAddress.OA_PostCode, OrganisationState = master.MainAddress.OA_State, OrganisationPhone = "123456" };
			var controller = new BorderWiseRegistrationController();
			var result = controller.FindPotentialMatchOrgs(Factory, contactInfo);
			AssertNotNull(result);
			AssertNotNull(result.OrgMatches);
			AssertEquals(1, result.OrgMatches.Count());
			AssertEquals(master.PK, result.OrgMatches.First().PK);
			AssertEquals($"{master.OH_FullName} ({master.OH_Code})", result.OrgMatches.First().FullName);
		}

		public void TestFindPotentialOrgs_Duplicate_Orgs_Found_Success()
		{
			var isWeb = Globals.IsWeb;
			Globals.IsWeb = true;
			var master = GetOrgHeader();
			var org1 = GetOrgHeader();
			Factory.Save();
			var contactInfo = new ContactInfo { OrganisationName = master.OH_FullName, OrganisationAddress1 = master.MainAddress.OA_Address1, OrganisationCountryCode = "AU", OrganisationCity = master.MainAddress.OA_City, OrganisationPostCode = master.MainAddress.OA_PostCode, OrganisationState = master.MainAddress.OA_State, OrganisationPhone = "123456" };
			var controller = new BorderWiseRegistrationController();
			var result = controller.FindPotentialMatchOrgs(Factory, contactInfo);
			AssertNotNull(result);
			AssertNotNull(result.OrgMatches);
			AssertEquals(2, result.OrgMatches.Count());
			AssertEquals(1, result.OrgMatches.Count(o => o.PK == master.PK));
			AssertEquals(1, result.OrgMatches.Count(o => o.PK == org1.PK));
			Globals.IsWeb = isWeb;
		}

		public void TestFindPotentialOrgs_Duplicate_Found_Active_Orgs_Success()
		{
			var master = GetOrgHeader();
			var org1 = GetOrgHeader(isActive: false);
			Factory.Save();
			var contactInfo = new ContactInfo { OrganisationName = master.OH_FullName, OrganisationAddress1 = master.MainAddress.OA_Address1, OrganisationCountryCode = "AU", OrganisationCity = master.MainAddress.OA_City, OrganisationPostCode = master.MainAddress.OA_PostCode, OrganisationState = master.MainAddress.OA_State, OrganisationPhone = "123456" };
			var controller = new BorderWiseRegistrationController();
			var result = controller.FindPotentialMatchOrgs(Factory, contactInfo);
			AssertNotNull(result);
			AssertNotNull(result.OrgMatches);
			AssertEquals(1, result.OrgMatches.Count());
			AssertEquals(1, result.OrgMatches.Count(o => o.PK == master.PK));
		}

		public void TestFindPotentialOrgs_Should_FilterOutOrgsWithExclusiveOrgPks_When_FoundMatchingOrgs()
		{
			var exclusiveOrg = GetOrgHeader("Exlusive Org");
			Factory.Save();
			var contactInfo = new ContactInfo { OrganisationName = exclusiveOrg.OH_FullName, OrganisationAddress1 = exclusiveOrg.MainAddress.OA_Address1, OrganisationCountryCode = "AU", OrganisationCity = exclusiveOrg.MainAddress.OA_City, OrganisationPostCode = exclusiveOrg.MainAddress.OA_PostCode, OrganisationState = exclusiveOrg.MainAddress.OA_State, OrganisationPhone = "123456" };
			var controller = new BorderWiseRegistrationController();
			var result = controller.FindPotentialMatchOrgs(Factory, contactInfo, new List<Guid> { exclusiveOrg.PK.ToGuid() });
			AssertNull(result);
		}

		PatternMatchingName CreatePatternMatchingName(OrgHeader orgheader, int hashedValue)
		{
			var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_OH = orgheader.PK;
			patternMatchingName.PMN_ParentId = orgheader.PK;
			patternMatchingName.PMN_HashedValue = hashedValue;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";
			patternMatchingName.PMN_ParentTableCode = "OH";
			patternMatchingName.PMN_IsActive = true;
			return patternMatchingName;
		}

		void CreatePatternMatchingAddress(OrgHeader orgheader)
		{
			var addressString = orgheader.MainAddress.OA_Address1 + orgheader.MainAddress.OA_Address2 + orgheader.MainAddress.OA_City + orgheader.MainAddress.OA_PostCode + orgheader.MainAddress.OA_State;
			var patternMatchingAddress = Factory.NewWithValidTestData<PatternMatchingAddress>();
			patternMatchingAddress.PMA_OH = orgheader.PK;
			patternMatchingAddress.PMA_ParentId = orgheader.MainAddress.PK;
			patternMatchingAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(addressString.ToUpper());
			patternMatchingAddress.PMA_RN_NKCountryCode = "AU";
			patternMatchingAddress.PMA_ParentTableCode = "OA";
			patternMatchingAddress.PMA_IsActive = true;
		}

		OrgHeader GetOrgHeader(string orgName = "TestOrg", string suffix = "", bool isActive = true)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = orgName;
			org.MainAddress.OA_Address1 = "72 O'Riordan Street" + suffix;
			org.MainAddress.OA_City = "SYDNEY" + suffix;
			org.MainAddress.OA_PostCode = "2015" + suffix;
			org.MainAddress.OA_State = "NSW" + suffix;
			org.OH_IsActive = isActive;
			var masterNameHashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(orgName, "AU"));
			CreatePatternMatchingName(org, masterNameHashedValue);
			CreatePatternMatchingAddress(org);
			return org;
		}

		#endregion
		#region SimilarOrgnisation
		public void TestGetSimilarOrganisations_Error_InvalidParameters()
		{
			var controller = new BorderWiseRegistrationController();
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.GetSimilarOrganisations(null)).Message);
			AssertEquals("All arguments must be provided.", ((BadRequestErrorMessageResult)controller.GetSimilarOrganisations(string.Empty)).Message);
		}

		public void TestGetSimilarOrganisations_Success_ExistingEmailDomainMatches()
		{
			var matchingOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			matchingOrg1.OH_FullName = "Matching Org 1";
			matchingOrg1.OH_RL_NKClosestPort = "INBOM";
			Factory.NewWithValidTestData<LicenceCompany>().LC_OH = matchingOrg1.PK;
			var matchingOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			matchingOrg2.OH_FullName = "Matching Org 2";
			matchingOrg2.OH_RL_NKClosestPort = "USORD";
			matchingOrg2.MainAddress.City = "Town";
			Factory.NewWithValidTestData<LicenceCompany>().LC_OH = matchingOrg2.PK;
			var nonMatchingOrg = Factory.NewWithValidTestData<OrgHeader>();
			nonMatchingOrg.OH_FullName = "Non matching org";
			nonMatchingOrg.OH_RL_NKClosestPort = "NZAKL";
			Factory.NewWithValidTestData<LicenceCompany>().LC_OH = nonMatchingOrg.PK;
			var matchingOrgNoLicence = Factory.NewWithValidTestData<OrgHeader>();
			matchingOrgNoLicence.OH_FullName = "Matching Org - No Licence";
			matchingOrgNoLicence.OH_RL_NKClosestPort = "USORD";
			var inactiveOrg = Factory.NewWithValidTestData<OrgHeader>();
			inactiveOrg.OH_FullName = "Inactive Org";
			inactiveOrg.OH_IsActive = false;
			inactiveOrg.OH_RL_NKClosestPort = "SGSIN";
			Factory.NewWithValidTestData<LicenceCompany>().LC_OH = inactiveOrg.PK;
			var patternMatch = Factory.New<PatternMatchingDomain>();
			patternMatch.PMD_OH = matchingOrg1.PK;
			patternMatch.PMD_ParentId = matchingOrg1.PK;
			patternMatch.PMD_ParentTableCode = OrgAddressSchema.Constants.Prefix;
			patternMatch.PMD_HashedValue = TextStandardizerHelper.ComputeStringHashFast("ZAYDEN.COM");
			var patternMatch2 = Factory.New<PatternMatchingDomain>();
			patternMatch2.PMD_OH = matchingOrg2.PK;
			patternMatch2.PMD_ParentId = matchingOrg2.PK;
			patternMatch2.PMD_ParentTableCode = OrgAddressSchema.Constants.Prefix;
			patternMatch2.PMD_HashedValue = TextStandardizerHelper.ComputeStringHashFast("ZAYDEN.COM");
			var patternMatch2b = Factory.New<PatternMatchingDomain>();
			patternMatch2b.PMD_OH = matchingOrg2.PK;
			patternMatch2b.PMD_ParentId = matchingOrg2.PK;
			patternMatch2b.PMD_ParentTableCode = OrgAddressSchema.Constants.Prefix;
			patternMatch2b.PMD_HashedValue = TextStandardizerHelper.ComputeStringHashFast("ZAYDEN.COM");
			var patternMatch3 = Factory.New<PatternMatchingDomain>();
			patternMatch3.PMD_ParentId = matchingOrg2.PK;
			patternMatch3.PMD_ParentTableCode = OrgAddressSchema.Constants.Prefix;
			patternMatch3.PMD_HashedValue = TextStandardizerHelper.ComputeStringHashFast("ZAYDEN.COM");
			var patternMatch4 = Factory.New<PatternMatchingDomain>();
			patternMatch4.PMD_OH = nonMatchingOrg.PK;
			patternMatch4.PMD_ParentId = nonMatchingOrg.PK;
			patternMatch4.PMD_ParentTableCode = OrgAddressSchema.Constants.Prefix;
			patternMatch4.PMD_HashedValue = TextStandardizerHelper.ComputeStringHashFast("GMAIL.COM");
			var patternMatch5 = Factory.New<PatternMatchingDomain>();
			patternMatch5.PMD_OH = inactiveOrg.PK;
			patternMatch5.PMD_ParentId = inactiveOrg.PK;
			patternMatch5.PMD_ParentTableCode = OrgAddressSchema.Constants.Prefix;
			patternMatch5.PMD_HashedValue = TextStandardizerHelper.ComputeStringHashFast("ZAYDEN.COM");
			var patternMatch6 = Factory.New<PatternMatchingDomain>();
			patternMatch6.PMD_OH = matchingOrgNoLicence.PK;
			patternMatch6.PMD_ParentId = matchingOrgNoLicence.PK;
			patternMatch6.PMD_ParentTableCode = OrgAddressSchema.Constants.Prefix;
			patternMatch6.PMD_HashedValue = TextStandardizerHelper.ComputeStringHashFast("ZAYDEN.COM");
			Factory.Save();
			var result = (OkNegotiatedContentResult<OrgSimilarMatchesResult>)new BorderWiseRegistrationController().GetSimilarOrganisations("rylan@zayden.com");
			AssertEquals(2, result.Content.OrgMatches.Count());
			AssertNotNull(result.Content.OrgMatches.Single(x => x.PK == matchingOrg1.PK));
			AssertNotNull(result.Content.OrgMatches.Single(x => x.PK == matchingOrg2.PK));
			AssertNotNull(result.Content.OrgMatches.Single(x => x.FullName == "Matching Org 1 (India, Mumbai (ex Bombay))"));
			AssertNotNull(result.Content.OrgMatches.Single(x => x.FullName == "Matching Org 2 (United States, Town)"));
		}

		public void TestGetSimilarOrganisations_Success_No_ExistingEmailDomainMatches()
		{
			var result = (OkNegotiatedContentResult<OrgSimilarMatchesResult>)new BorderWiseRegistrationController().GetSimilarOrganisations("rylan@zayden.com");
			AssertNull(result.Content);
		}

		string DecodeHtml(string s)
		{
			return s.Replace("&nbsp;", " ").Replace(" ", " ").Trim();
		}

		#endregion
		#region Implementation
		void AssertRegistrationSuccess(string brokerId = "", string studentId = "", string studentInstitute = "", string orgNameForContactInfo = "Indy and Lola Org", Guid? existingOrgPk = null, bool shouldUpdateExistingAddress = false, int version = 5, string contactName = "", Guid? organisationPk = null, bool isExistingContactName = false)
		{
			ValidateAddressActionForTesting = address => address.ValidationStatus = AddressValidationStatus.Verified;
			var tokenRecord = CreateRegToken(existingOrgPk);
			var contactInfo = GetValidContactInfo(brokerId, studentId, studentInstitute, orgNameForContactInfo, orgPk: organisationPk, contactName);
			OkNegotiatedContentResult<RegistrationSuccessResponse> result;
			switch (version)
			{
				case 5:
					result = (OkNegotiatedContentResult<RegistrationSuccessResponse>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(tokenRecord.SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);
					break;
				default:
					throw new InvalidOperationException("Register version is incorrect.");
			}

			AssertEquals("You have successfully registered as a BorderWise user. Your login details have been emailed to test@example.com.", result.Content.Message);
			AssertEquals("test@example.com", result.Content.RegisteredEmail);
			AssertEquals(true, tokenRecord.IsDeleted);
			ValidateAddressActionForTesting = address => address.ValidationStatus = AddressValidationStatus.Verified;
			OrgHeader org;
			OrgContact contact;
			var expectedContactName = isExistingContactName ? contactInfo.ContactName + " (1)" : contactInfo.ContactName;
			if (existingOrgPk != null && existingOrgPk != Guid.Empty)
			{
				org = Factory.Load<OrgHeader>(existingOrgPk.Value);
				var contacts = org.Contacts.Cast<OrgContact>().Where(x => x.OC_ContactName == expectedContactName);

				contact = contacts.Single();
				AssertEquals(org.MainAddress.PK, contact.OC_OA_OrgAddress);
				var email = Env.OutgoingMailManager.EmailsCreated.SingleOrDefault(x => x.Subject == "BorderWise New Organisation - " + org.OH_FullName);
				AssertNull(email);
			}
			else
			{
				if (contactInfo.OrganisationPk.HasValue && contactInfo.OrganisationPk.Value != Guid.Empty)
				{
					org = Factory.Load<OrgHeader>(contactInfo.OrganisationPk.Value);
					var contacts = org.Contacts.Cast<OrgContact>().Where(x => x.OC_ContactName == expectedContactName);
					AssertEquals(1, contacts.Count());
					contact = contacts.Single();
					if (shouldUpdateExistingAddress)
					{
						AssertEquals(1, org.Addresses.Count);
						AssertEquals(org.Addresses[0].PK, contact.OC_OA_OrgAddress);
					}
					else
					{
						AssertEquals(2, org.Addresses.Count);
						AssertEquals(org.Addresses[1].PK, contact.OC_OA_OrgAddress);
						AssertEquals(true, org.Addresses[1].IsAddressOfType(OrgAddressType.Office));
					}
				}
				else
				{
					var orgName = string.IsNullOrEmpty(contactInfo.OrganisationName) ? contactInfo.ContactName : contactInfo.OrganisationName;
					org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, orgName));
					var contacts = org.Contacts.Cast<OrgContact>().Where(x => x.OC_ContactName == expectedContactName);
					AssertEquals(1, contacts.Count());
					contact = contacts.Single();
					AssertEquals(1, org.Addresses.Count);
					AssertEquals(false, org.OH_Code.IsEmpty);
					AssertEquals(contactInfo.OrganisationBusinessNumber, org.LocalBusinessRegNo);
					AssertEquals(contactInfo.OrganisationAddress1, org.MainAddress.OA_Address1);
					AssertEquals(contactInfo.OrganisationAddress2.Substring(0, OrgAddressSchema.OA_Address2.MaxLength), org.MainAddress.OA_Address2);
					AssertEquals(contactInfo.OrganisationCity, org.MainAddress.OA_City);
					AssertEquals(contactInfo.OrganisationState, org.MainAddress.OA_State);
					AssertEquals(contactInfo.OrganisationPostCode, org.MainAddress.OA_PostCode);
					AssertEquals(contactInfo.OrganisationCountryCode, org.MainAddress.OA_RN_NKCountryCode);
					AssertEquals(contactInfo.OrganisationPhone.Replace(" ", string.Empty), org.MainAddress.OA_Phone);
					AssertEquals(contactInfo.OrganisationEmail, org.MainAddress.OA_Email);
					AssertEquals("BW- " + contactInfo.OrganisationAddress1, org.MainAddress.OA_Code);
					AssertEquals(contactInfo.OrganisationWebSite, org.MainWebURL.PU_URL);
					var email = Env.OutgoingMailManager.EmailsCreated.Single(x => x.Subject == "BorderWise New Organisation - " + orgName);
					AssertEquals("BorderWise Support", email.FromDisplayName);
					AssertEquals("support@borderwise.com", email.FromAddress);
					AssertEquals("support@borderwise.com", email.ReplyTo);
					AssertEquals("lola@appoo.net", email.Recipients[0].Email);
					AssertContains(string.Format("&ControllerID=Organisation&BusinessEntityPK={0}&VersionNumber={1}&Hash=", org.PK, new EnterpriseInformationRetriever().VersionNumber), email.Body);
					AssertContains(">" + orgName, email.Body);
					AssertContains("was created through the BorderWise Registration process and should be verified.", email.Body);
					AssertEquals(org.MainAddress.PK, contact.OC_OA_OrgAddress);
					AssertNotNull(contact.Documents.Cast<OrgDocument>().Single(x => x.OD_DocumentGroup == EDIOrgDocumentGroupTypes.Codes.BorderWiseAdministrator && x.OD_DefaultContact));
				}
			}

			AssertEquals(true, contact.OC_WebAccessEnabled);
			AssertEquals("BorderWise Reg", contact.OC_ContactSource);
			AssertEquals(contactInfo.ContactWorkPhone.Replace(" ", string.Empty), contact.OC_Phone);
			AssertEquals(contactInfo.ContactMobilePhone.Replace(" ", string.Empty), contact.OC_Mobile);
			AssertEquals(contactInfo.ContactJobTitle, contact.OC_Title);
			AssertEquals(contactInfo.ContactDOB, contact.OC_Birthday);
			AssertEquals("test@example.com", contact.OC_Email);
			AssertNotNull(result.Content.RegistrationInfo);
			var registrationInfo = result.Content.RegistrationInfo;
			AssertEquals(contact.PK, registrationInfo.OrgContactPk);
			AssertEquals(org.PK, registrationInfo.OrgHeaderPk);
			AssertEquals(org.OH_Code, registrationInfo.OhCode);
			AssertEquals(contact.OrgAddress.PK, registrationInfo.OrgHeaderAddressPk);
			AssertEquals(contact.OC_PasswordHash, registrationInfo.PasswordHash.ToArray());
			AssertEquals(contact.OC_PasswordSalt, registrationInfo.PasswordSalt.ToArray());
			AssertEquals(contact.OC_PasswordHashIterations, registrationInfo.PasswordHashIterations);
			AssertEquals(contact.OC_Email, registrationInfo.ContactEmail);
			AssertEquals(true, contact.OC_IsActive);

			var brokerCert = contact.Certificates.SingleOrDefault(x => x.XZ_Type == "BRK");
			if (string.IsNullOrEmpty(brokerId))
			{
				AssertNull(brokerCert);
			}
			else
			{
				AssertEquals(brokerId, brokerCert.XZ_RefNumber);
			}

			var cert = contact.Certificates.SingleOrDefault(x => x.XZ_Type == "MSC");
			if (string.IsNullOrEmpty(studentId) && string.IsNullOrEmpty(studentInstitute))
			{
				AssertNull(cert);
			}
			else
			{
				AssertEquals(studentId, cert.XZ_RefNumber);
				if (string.IsNullOrEmpty(studentInstitute))
				{
					AssertEquals("Student", cert.XZ_Comment);
				}
				else
				{
					AssertEquals("Student - UTS", cert.XZ_Comment);
				}

				AssertEquals(ZDateTime.Today.AddDays(CommonConstants.studentCertificateExpiryDays), cert.XZ_ExpiryOrDueDate);
			}
		}

		void AssertRegistrationOrgDeduplication(Guid? orgPk)
		{
			PotentialMatchOrgResultForTesting = GetOrgSimilarMatchesResult();
			var testOrg = PotentialMatchOrgResultForTesting.OrgMatches.First();
			var tokenRecord = CreateRegToken(null);
			var contactInfo = GetValidContactInfo(orgName: testOrg.FullName, orgPk: orgPk);
			var result = (OkNegotiatedContentResult<OrgSimilarMatchesResult>)new BorderWiseRegistrationController().RegisterV5(Guid.Parse(tokenRecord.SD_Name.Replace("BorderWiseReg-", string.Empty)), contactInfo);
			AssertNotNull(result.Content);
			AssertNotNull(result.Content.OrgMatches);
			AssertEquals(PotentialMatchOrgResultForTesting.OrgMatches.Count(), result.Content.OrgMatches.Count());
			AssertEquals(testOrg.PK, result.Content.OrgMatches.First().PK);
			AssertEquals(testOrg.FullName, result.Content.OrgMatches.First().FullName);
		}

		OrgSimilarMatchesResult GetOrgSimilarMatchesResult(Guid? existingOrgPk = null)
		{
			var testOrg = new OrgDetail { PK = Guid.NewGuid(), FullName = "Test Org" };
			var orgMatches = new List<OrgDetail> { testOrg };
			if (existingOrgPk.HasValue)
			{
				orgMatches.Add(new OrgDetail() { PK = existingOrgPk.Value, FullName = "Existing Org" });
			}

			return new OrgSimilarMatchesResult
			{
				OrgMatches = orgMatches
			};
		}

		StmData CreateRegToken(Guid? existingOrgPk = null, string contactEmail = "test@example.com")
		{
			var tokenValue = ZDateTime.UtcNow.AddHours(1).ToString(ZDateTime.LongTimeFormat, CultureInfo.InvariantCulture) + "|" + contactEmail;
			if (existingOrgPk.HasValue)
			{
				tokenValue += "|" + existingOrgPk.Value;
			}
			return CreateTokenCore(RegTokenName, tokenValue);
		}

		Guid CreateTestConfirmAccessToken(Guid contactPk, bool validExpiryDate = true, bool includeContactPk = true, bool isExpired = false, bool validContactPk = true)
		{
			var tokenValue = string.Empty;
			if (validExpiryDate)
			{
				tokenValue += ZDateTime.UtcNow.AddHours(isExpired ? -1 : 1).ToString(ZDateTime.LongTimeFormat, CultureInfo.InvariantCulture);
			}
			else
			{
				tokenValue += "not valid date";
			}

			if (includeContactPk)
			{
				tokenValue += "|" + (validContactPk ? contactPk.ToString() : "invalid pk");
			}

			var token = CreateTokenCore(ConfirmAccessTokenName, tokenValue);
			return Guid.Parse(token.SD_Name.Replace(ConfirmAccessTokenName, string.Empty));
		}

		StmData CreateTokenCore(string tokenName, string tokenValue)
		{
			var token = Guid.NewGuid();
			var tokenRecord = Factory.New<StmData>();
			tokenRecord.SD_Name = tokenName + token;
			tokenRecord.SD_BinaryValue = ZBlob.FromAscii(tokenValue);
			Factory.Save();
			return tokenRecord;
		}

		void AssertStudentExplanationEmailSent()
		{
			bool emailWasSent = false;
			if (Env.OutgoingMailManager.EmailsCreated.Count == 1)
			{
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				if (email.Subject.Equals("BorderWise student verification"))
				{
					AssertEquals("BorderWise Support", email.FromDisplayName);
					AssertEquals("support@borderwise.com", email.FromAddress);
					AssertContains("As per our Terms and Conditions you are required to provide evidence of your student status.", email.Body);
					emailWasSent = true;
				}
			}

			AssertEquals(true, emailWasSent);
		}

		void AssertLoginDetailsEmailSend(int expectedCount = 1, string emailAddress = "rylan@zayden.com")
		{
			AssertEquals(expectedCount, Env.OutgoingMailManager.EmailsCreated.Count);
			for (var i = 0; i < expectedCount; i++)
			{
				var email = Env.OutgoingMailManager.EmailsCreated[i];
				AssertEquals("BorderWise Support", email.FromDisplayName);
				AssertEquals("support@borderwise.com", email.FromAddress);
				AssertEquals(emailAddress, email.Recipients[0].Email);
				AssertContains("Dear John Smith,<br /><br />", email.Body);
				AssertEquals("Your BorderWise access details", email.Subject);
				AssertContains("The access details to your <a href='https://app.borderwise.com/'>BorderWise</a> account are:<br /><br />", email.Body);
				AssertContains("Company Code: <b>ZUB", email.Body);
				AssertContains($"User Name: <b>{emailAddress}</b>", email.Body);
				AssertContains("Forgotten your password? Use the <a href=\"https://app.borderwise.com/account/forgot-password/\">Forgot Password</a> link to reset your password.", email.Body);
				AssertNotContains("(*: ", email.Body);
			}
		}

		void AssertNoActiveAccountsEmailSend(bool emailToAdminSent = true, int expectedCount = 1, string emailAddress = "rylan@zayden.com", int emailIndex = 0)
		{
			for (var i = emailIndex; i < expectedCount; i++)
			{
				var email = Env.OutgoingMailManager.EmailsCreated[i];
				AssertEquals("BorderWise Support", email.FromDisplayName);
				AssertEquals("support@borderwise.com", email.FromAddress);
				AssertEquals(emailAddress, email.Recipients[0].Email);
				AssertContains("Dear John Smith1,<br /><br />", email.Body);
				AssertEquals("No BorderWise Access Rights Assigned to Your Account", email.Subject);
				AssertContains("This email address is associated with one or more accounts. However, none of them has BorderWise access rights.<br /><br />", email.Body);
				AssertContains(emailToAdminSent ? "An email has been sent to your company's admin to confirm your access to BorderWise." : "Please contact WiseTech support to enable BorderWise access.", email.Body);
			}
		}

		void AssertEmailsSent(int expectedCount)
		{
			AssertEquals(expectedCount, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		void AssertConfirmAccessEmailSend(int expectedCount = 1, string orgCode = "ZUB")
		{
			for (var i = 0; i < expectedCount; i++)
			{
				var email = Env.OutgoingMailManager.EmailsCreated[i];
				AssertEquals("BorderWise Support", email.FromDisplayName);
				AssertEquals("support@borderwise.com", email.FromAddress);
				AssertEquals("support@borderwise.com", email.ReplyTo);
				AssertEquals("admin@zayden.com", email.Recipients[0].Email);
				AssertEquals("A User request access to BorderWise", email.Subject);
				AssertContains("Dear admin,<br /><br />", email.Body);
				AssertContains($"You are receiving this email because you are the BorderWise administrator for your organization ({orgCode}).<br /><br />", email.Body);
				AssertContains("A new user, <a href='mailto:contact1@gmail.com'>contact1@gmail.com</a> is requesting an access to <a href='https://app.borderwise.com/'>BorderWise</a>. <br />", email.Body);
				AssertContains("It is required for a technical contact to approve this request. If you believe this request is genuine please <a href='", email.Body);
				AssertContains(">click here to approve the access</a>.<br /><br />", email.Body);
				AssertContains("Please discard this email if you do not approve this request.<br /><br />", email.Body);
				AssertContains("This link will be expired in 24 hours.", email.Body);
			}
		}

		ContactInfo GetValidContactInfo(string brokerId = null, string studentId = null, string studentInsitute = null, string orgName = "Indy and Lola Org", Guid? orgPk = null, string contactName = "")
		{
			var result = new ContactInfo();
			result.OrganisationPk = orgPk;
			result.OrganisationName = orgName;
			result.OrganisationAddress1 = "Add 1";
			result.OrganisationAddress2 = new string('Z', OrgAddressSchema.OA_Address2.MaxLength + 10);
			result.OrganisationCity = "Newington";
			result.OrganisationState = "NSW";
			result.OrganisationPostCode = "2127";
			result.OrganisationCountryCode = "AU";
			result.OrganisationWebSite = "www.altavista.com";
			result.OrganisationPhone = "+61 2 1234 5678";
			result.OrganisationEmail = "cooee@enternet.com.au";
			result.OrganisationBusinessNumber = "1231414145";
			result.ContactName = string.IsNullOrWhiteSpace(contactName) ? "Aleera" : contactName;
			result.ContactWorkPhone = "+61 2 9481 1111";
			result.ContactMobilePhone = "+61 403 009 671";
			result.ContactJobTitle = "Cute Kid";
			result.ContactPassword = "SomePassword$123";
			result.ContactDOB = new DateTime(1981, 5, 28);
			if (!string.IsNullOrEmpty(brokerId))
			{
				result.ContactBrokerID = brokerId;
			}

			if (!string.IsNullOrEmpty(studentId))
			{
				result.ContactStudentID = studentId;
			}

			if (!string.IsNullOrEmpty(studentInsitute))
			{
				result.ContactStudentInstitution = studentInsitute;
			}

			return result;
		}

		OrgContact CreateOrgAndContact(string orgCode = "ZUB", bool isActive = true, bool webAccessEnabled = true, bool includeBorderWiseAdmin = false, string email = "rylan@zayden.com")
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = orgCode;

			var contact = CreateContact(org, isActive, webAccessEnabled, includeBorderWiseAdmin, email);
			return contact;
		}

		List<OrgContact> CreateOrgAndContacts(string orgCode = "ZUB", bool isActive = true, bool webAccessEnabled = true, bool includeBorderWiseAdmin = false, int numberOfContacts = 2)
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = orgCode;

			var orgContacts = new List<OrgContact>(numberOfContacts);

			for (int i = 0; i < numberOfContacts; i++)
			{
				orgContacts.Add(CreateContact(org, isActive, webAccessEnabled, name: $"John Smith{i + 1}", email: $"contact{i + 1}@gmail.com"));
			}

			return orgContacts;
		}

		OrgContact CreateContact(EDIOrgHeader org, bool isActive = true, bool webAccessEnabled = true, bool includeBorderWiseAdmin = false, string email = "rylan@zayden.com", string name = "John Smith")
		{
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = name;
			contact.OC_Email = email;
			contact.SetHashedPassword("zubin123");
			contact.OC_WebAccessEnabled = webAccessEnabled;
			contact.OC_IsActive = isActive;
			if (includeBorderWiseAdmin)
			{
				var adminContact = org.Contacts.AddNew();
				adminContact.OC_ContactName = "admin";
				adminContact.OC_Email = "admin@zayden.com";
				adminContact.SetHashedPassword("zubin123");
				adminContact.OC_WebAccessEnabled = true;
				adminContact.OC_IsActive = isActive;
				var borDocumentGroup = adminContact.Documents.AddNew();
				borDocumentGroup.OD_DocumentGroup = EDIOrgDocumentGroupTypes.Codes.BorderWiseAdministrator;
				borDocumentGroup.OD_DefaultContact = true;
				var adminContact2 = org.Contacts.AddNew();
				adminContact2.OC_ContactName = "admin2";
				adminContact2.OC_Email = "admin2@zayden.com";
				adminContact2.SetHashedPassword("zubin123");
				adminContact2.OC_WebAccessEnabled = true;
				adminContact2.OC_IsActive = isActive;
				var borDocumentGroup2 = adminContact2.Documents.AddNew();
				borDocumentGroup2.OD_DocumentGroup = EDIOrgDocumentGroupTypes.Codes.BorderWiseAdministrator;
				borDocumentGroup2.OD_DefaultContact = false;
			}

			Factory.Save();
			return contact;
		}

		void AssertTokenValidation(string tokenString, string expectedErrorCode, int version = 5)
		{
			var token = Guid.NewGuid();
			var tokenRecord = Factory.New<StmData>();
			tokenRecord.SD_Name = "BorderWiseReg-" + token;
			if (!string.IsNullOrEmpty(tokenString))
			{
				tokenRecord.SD_BinaryValue = ZBlob.FromAscii(tokenString);
			}

			Factory.Save();
			NegotiatedContentResult<string> result;
			switch (version)
			{
				case 5:
					result = (NegotiatedContentResult<string>)new BorderWiseRegistrationController().RegisterV5(token, GetValidContactInfo());
					break;
				default:
					throw new InvalidOperationException("Register version is incorrect.");
			}

			AssertEquals(HttpStatusCode.BadRequest, result.StatusCode);
			AssertEquals(string.Format("The registration link is invalid. Please request a new registration token and try again. [{0}]", expectedErrorCode), result.Content);
			AssertEquals(true, tokenRecord.IsDeleted);
		}

		IDisposable instanceDetailsDisposable;

		protected override void SetUp()
		{
			base.SetUp();

			instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "lola@appoo.net";
			group.Staff.Add(staff);
			Factory.Save();
			EDIDataRegistry.Instance.BorderWiseNewOrganisationEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			AddressValidationResultForTesting = new WebAddressValidationResult();
			PotentialMatchOrgResultForTesting = null;
		}

		protected override void TearDown()
		{
			instanceDetailsDisposable?.Dispose();
			base.TearDown();
		}

		#endregion
	}
}
