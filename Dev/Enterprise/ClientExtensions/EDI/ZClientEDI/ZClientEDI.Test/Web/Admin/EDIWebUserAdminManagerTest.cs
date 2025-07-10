using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Moq;
using Moq.Protected;
using ZClientEDI.Business;

namespace Enterprise.Client.EDI.Web.Admin.Testing
{
	public class EDIWebUserAdminManagerTest : WebUserAdminManagerTest
	{
		class EDIWebUserAdminManagerForTest : EDIWebUserAdminManager
		{
			public EDIWebUserAdminManagerForTest(OrgContact contact, HttpClient httpClient) : base(contact, httpClient)
			{
			}

			protected override bool IsBorderWiseUserManagementPortalAccessible => true; // Let's make all tests directly covering this class use the UMP by default.
		}

		protected override WebUserAdminManager GetNewWebUserAdminManagerForTest(OrgContact newContact)
		{
			var handlerMock = GetHttpClientHandler(true, HttpStatusCode.OK, "OK", false, newContact);
			return new EDIWebUserAdminManagerForTest(newContact, new HttpClient(handlerMock.Object));
		}

		protected override string ExpectedFromContactName => IncidentConstants.SupportDisplayName;
		protected override string ExpectedFromEmailAddress => SupportIncidentLookups.SupportEmailAddress;
		protected override string AccountInfo => "account";
		public void TestChangePassword_WhenUMPIsEnabled_ShouldCallUmpApi_WhenUmpReturnsSuccess()
		{
			ChangePasswordSuccessfully_AssertingUmpServiceCall(true, true, HttpStatusCode.OK, "BorderWise Response");
		}

		public void TestChangePassword_WhenUMPIsEnabled_ShouldCallUmpApi_WhenExceptionThrown()
		{
			ChangePasswordSuccessfully_AssertingUmpServiceCall(true, true, null, null);
		}

		public void TestChangePassword_WhenUMPIsEnabled_ShouldCallUmpApi_WhenTimeout()
		{
			ChangePasswordSuccessfully_AssertingUmpServiceCall(true, true, HttpStatusCode.BadRequest, "BorderWise Response", true);
		}

		public void TestChangePassword_WhenUMPIsEnabled_ShouldCallUmpApi_WhenUmpReturnsError()
		{
			ChangePasswordSuccessfully_AssertingUmpServiceCall(true, true, HttpStatusCode.InternalServerError, "BorderWise Response");
		}

		public void TestChangePassword_WhenUMPIsEnabled_ShouldCallUmpApi_WhenUmpReturnsNotFound()
		{
			ChangePasswordSuccessfully_AssertingUmpServiceCall(true, true, HttpStatusCode.NotFound, "BorderWise Response");
		}

		public void TestChangePassword_WhenUMPIsEnabled_AndPasswordChangeIsUnsuccessful_ShouldNotCallUmpApi()
		{
			ChangePasswordSuccessfully_AssertingUmpServiceCall(false, false, null, null);
		}

		public void TestSetMasterPassword_ShouldCallUmpApi_When_UMPIsEnabled()
		{
			ChangePasswordSuccessfully_AssertingUmpServiceCall(true, true, HttpStatusCode.OK, "BorderWise Response", shouldSetMasterPassword: true);
		}

		public void TestSetMasterPassword_ShouldNotCallUmpApi_When_UMPIsEnabled()
		{
			ChangePasswordSuccessfully_AssertingUmpServiceCall(false, false, null, null, isProductionSystem: false);
		}

		void ChangePasswordSuccessfully_AssertingUmpServiceCall(bool isPasswordChangeSuccessful, bool shouldCallUmpService, HttpStatusCode? umpServiceResponse, string umpResponseMessage, bool timeout = false, bool shouldSetMasterPassword = false, bool isProductionSystem = true)
		{
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			GlbPerson person = null;
			if (shouldSetMasterPassword)
			{
				person = Factory.NewWithValidTestData<GlbPerson>();
				contact.OC_PER = person.PK;
			}

			var licenceType = isProductionSystem ? DatabaseTypes.Codes.Production : DatabaseTypes.Codes.Test;
			LicenceTypeChanger.SetSystemLicence(licenceType);
			var objectFactory = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			if (isProductionSystem)
			{
				objectFactory.EnterpriseCodeForTest = "EDI";
				objectFactory.ServerCodeForTest = "SYD";
			}

			AssertEquals(isProductionSystem, Env.Instance.IsProductionSystem);
			AssertEquals(isProductionSystem, EdiProdDbHelper.IsRunningOnEdiProdDatabase);

			var handlerMock = GetHttpClientHandler(shouldCallUmpService, umpServiceResponse, umpResponseMessage, timeout, contact, person);
			var httpClient = new HttpClient(handlerMock.Object);
			var manager = shouldSetMasterPassword ? new EDIWebUserAdminManagerForTest(contact, httpClient) : new EDIWebUserAdminManagerForTest(contact, httpClient);
			EDIDataRegistry.Instance.BorderWiseUmpApiEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, shouldCallUmpService);
			var newPassword = isPasswordChangeSuccessful ? "Th1sIsMyPassword!" : string.Empty;
			var result = shouldSetMasterPassword ? manager.SetMasterPassword(newPassword, newPassword) : manager.ChangePassword(newPassword, newPassword).Message;
			var umpServiceResultShouldBeInvalid = umpServiceResponse != null && umpServiceResponse.Value != HttpStatusCode.OK;
			if (isPasswordChangeSuccessful)
			{
				AssertEquals(manager.PasswordChangeSuccess, result);
			}
			else
			{
				AssertNotEquals(manager.PasswordChangeSuccess, result);
			}

			if (umpServiceResultShouldBeInvalid)
			{
				if (umpServiceResponse == HttpStatusCode.NotFound || timeout)
				{
					AssertEquals(0, ErrorReporter.TotalErrorCount);
				}
				else
				{
					AssertEquals($"Update BorderWise UMP{(shouldSetMasterPassword ? " users" : string.Empty)} password failed for record {(shouldSetMasterPassword ? person.PK : contact.PK)}. StatusCode: {umpServiceResponse.Value}, Reason: Internal Server Error, Message: BorderWise Response", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			}

			if (!isProductionSystem)
			{
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}

			if (shouldCallUmpService && umpServiceResponse == null)
			{
				AssertEquals($"Update BorderWise UMP{(shouldSetMasterPassword ? " users" : string.Empty)} password failed for record {(shouldSetMasterPassword ? person.PK : contact.PK)}.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}

			handlerMock.VerifyAll();
		}

		static bool RequestMatchesUmpUpdatePassword(HttpRequestMessage message, ZGuid contactPK)
		{
			var expectedUri = FormattableString.Invariant($"https://notarealdomain/v1/ump/users/{contactPK}/password");
			if (message.RequestUri.ToString() == expectedUri && message.Method.Method == "POST")
			{
				var expectedBodyRegex = new Regex("{\"ApiKey\":\"MahKey\",\"PasswordHash\":\".+\",\"PasswordSalt\":\".+\",\"PasswordHashIterations\":200000,\"ContactPks\":null}");
				var body = message.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				return expectedBodyRegex.IsMatch(body);
			}

			return false;
		}

		static bool RequestMatchesUmpUpdateUsersPassword(HttpRequestMessage message)
		{
			var expectedUri = FormattableString.Invariant($"https://notarealdomain/v1/ump/users/password");
			if (message.RequestUri.ToString() == expectedUri && message.Method.Method == "POST")
			{
				var expectedBodyRegex = new Regex("{\"ApiKey\":\"MahKey\",\"PasswordHash\":\".+\",\"PasswordSalt\":\".+\",\"PasswordHashIterations\":200000,\"ContactPks\".+}");
				var body = message.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				return expectedBodyRegex.IsMatch(body);
			}

			return false;
		}

		Mock<HttpClientHandler> GetHttpClientHandler(bool shouldCallUmpService, HttpStatusCode? umpServiceResponse, string umpResponseMessage, bool timeout, OrgContact contact, GlbPerson person = null)
		{
			EDIDataRegistry.Instance.BorderWiseUmpApiAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://notARealDomain/");
			EDIDataRegistry.Instance.BorderWiseUmpApiUpdatePasswordApiKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "MahKey");
			var handlerMock = new Mock<HttpClientHandler>(MockBehavior.Strict);
			if (!shouldCallUmpService)
			{
				return handlerMock;
			}

			var handlerSetup = handlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => person == null ? RequestMatchesUmpUpdatePassword(m, contact.PK) : RequestMatchesUmpUpdateUsersPassword(m)), ItExpr.IsAny<CancellationToken>());
			if (umpServiceResponse.HasValue)
			{
				if (!timeout)
				{
					var response = new HttpResponseMessage(umpServiceResponse.Value)
					{
						Content = new StringContent(umpResponseMessage)
						{ Headers = { ContentType = new MediaTypeHeaderValue("application/json") } }
					};
					handlerSetup.Returns(Task.FromResult(response));
				}
				else
				{
					handlerSetup.Throws(new TaskCanceledException());
				}
			}
			else
			{
				handlerSetup.Throws(new Exception("Error"));
			}

			return handlerMock;
		}
	}
}
