using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.ServiceTasks;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Messaging.Tests.ServiceTasks
{
	[TestedType(typeof(EDIClientNotificationServiceTask))]
	class EDIClientNotificationServiceTaskTest : ServiceTaskTestCase<EDIClientNotificationServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		[TestDate(2024, 02, 25)]
		public void TestRunTaskWithExpiredCertInboundConfig()
		{
			string[] recipients = { "ClientCertificate@test.com", "ClientCertificate2@test.com" };
			using (EDIClientRegistry.Instance.ClientCertificateExpiryNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateGroupWithUser(recipients).group.PK.ToGuid()))
			using (EDIClientRegistry.Instance.ClientCertificateExpiryNotificationDuration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			{
				SetUpTestClientAuthentication("Test Client 1", "IN", activeConfig: true, activeClient: true);
				var task = new EDIClientNotificationServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				AssertEquals("Email sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(GetExpectedMailContent("Test Client 1", "Inbound", expired: true), recipients, Env.OutgoingMailManager.EmailsCreated[0]);
			}
		}

		[TestDate(2024, 02, 25)]
		public void TestRunTaskWithExpiredCertOutboundConfig()
		{
			string[] recipients = { "ClientCertificate@test.com", "ClientCertificate2@test.com" };
			using (EDIClientRegistry.Instance.ClientCertificateExpiryNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateGroupWithUser(recipients).group.PK.ToGuid()))
			using (EDIClientRegistry.Instance.ClientCertificateExpiryNotificationDuration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			{
				SetUpTestClientAuthentication("Test Client 1", "OUT", activeConfig: true, activeClient: true);
				var task = new EDIClientNotificationServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				AssertEquals("Email sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(GetExpectedMailContent("Test Client 1", "Outbound", expired: true), recipients, Env.OutgoingMailManager.EmailsCreated[0]);
			}
		}

		[TestDate(2024, 01, 26)]
		public void TestRunTaskWithToBeExpiredCertInboundConfig()
		{
			string[] recipients = { "ClientCertificate@test.com" };
			using (EDIClientRegistry.Instance.ClientCertificateExpiryNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateGroupWithUser(recipients).group.PK.ToGuid()))
			using (EDIClientRegistry.Instance.ClientCertificateExpiryNotificationDuration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			{
				SetUpTestClientAuthentication("Test Client 1", "IN", activeConfig: true, activeClient: true);

				var task = new EDIClientNotificationServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				AssertEquals("Email sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(GetExpectedMailContent("Test Client 1", "Inbound", expired: false), recipients, Env.OutgoingMailManager.EmailsCreated[0]);
			}
		}

		[TestDate(2024, 01, 26)]
		public void TestRunTaskWithToBeExpiredCertOutboundConfig()
		{
			string[] recipients = { "ClientCertificate@test.com" };
			using (EDIClientRegistry.Instance.ClientCertificateExpiryNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateGroupWithUser(recipients).group.PK.ToGuid()))
			using (EDIClientRegistry.Instance.ClientCertificateExpiryNotificationDuration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			{
				SetUpTestClientAuthentication("Test Client 1", "OUT", activeConfig: true, activeClient: true);

				var task = new EDIClientNotificationServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				AssertEquals("Email sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(GetExpectedMailContent("Test Client 1", "Outbound", expired: false), recipients, Env.OutgoingMailManager.EmailsCreated[0]);
			}
		}

		[TestDate(2024, 01, 25)]
		public void TestRunTaskWithNotTeBeExpiredCertConfig()
		{
			string[] recipients = { "ClientCertificate@test.com" };
			using (EDIClientRegistry.Instance.ClientCertificateExpiryNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateGroupWithUser(recipients).group.PK.ToGuid()))
			using (EDIClientRegistry.Instance.ClientCertificateExpiryNotificationDuration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			{
				SetUpTestClientAuthentication("Test Client 1", "IN", activeConfig: true, activeClient: true);
				SetUpTestClientAuthentication("Test Client 2", "OUT", activeConfig: true, activeClient: true);
				var task = new EDIClientNotificationServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				AssertEquals("No email sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		[TestDate(2024, 01, 26)]
		public void TestRunTaskWithToBeExpiredCertInactiveConfig()
		{
			string[] recipients = { "ClientCertificate@test.com" };
			using (EDIClientRegistry.Instance.ClientCertificateExpiryNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateGroupWithUser(recipients).group.PK.ToGuid()))
			using (EDIClientRegistry.Instance.ClientCertificateExpiryNotificationDuration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			{
				SetUpTestClientAuthentication("Test Client 1", "IN", activeConfig: true, activeClient: false);
				SetUpTestClientAuthentication("Test Client 2", "IN", activeConfig: false, activeClient: true);
				SetUpTestClientAuthentication("Test Client 3", "IN", activeConfig: false, activeClient: false);

				var task = new EDIClientNotificationServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				AssertEquals("Email sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		[TestDate(2024, 03, 26)]
		public void TestRunTaskWithExpiredBeforeDurationCertConfig()
		{
			string[] recipients = { "ClientCertificate@test.com" };
			using (EDIClientRegistry.Instance.ClientCertificateExpiryNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateGroupWithUser(recipients).group.PK.ToGuid()))
			using (EDIClientRegistry.Instance.ClientCertificateExpiryNotificationDuration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			{
				SetUpTestClientAuthentication("Test Client 1", "IN", activeConfig: true, activeClient: true);
				SetUpTestClientAuthentication("Test Client 2", "OUT", activeConfig: true, activeClient: true);
				var task = new EDIClientNotificationServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				AssertEquals("No email sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		[TestDate(2024, 02, 25)]
		public void TestRunTaskWithExpiredCertInboundConfigForFallbackNotificationGroup()
		{
			string[] recipients = { "ClientCertificate@test.com", "ClientCertificate3@test.com" };
			using (DataRegistry.Instance.RawRegistry.NotificationGroup.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateGroupWithUser(recipients).group.PK.ToGuid()))
			using (EDIClientRegistry.Instance.ClientCertificateExpiryNotificationDuration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			{
				SetUpTestClientAuthentication("Test Client 1", "IN", activeConfig: true, activeClient: true);
				var task = new EDIClientNotificationServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				AssertEquals("Fall back to CompanyNotificationGroup", true, ((TestServiceLogger)task.ServiceLogger).ToString().Contains("Notification Email Group is not setup or does not contain any users with email addresses, try to get email addresses from company notification group."));
				AssertEquals("Email sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(GetExpectedMailContent("Test Client 1", "Inbound", expired: true), recipients, Env.OutgoingMailManager.EmailsCreated[0]);
			}
		}

		[TestDate(2024, 02, 25)]
		public void TestRunTaskWithExpiredCertInboundConfigForFallbackAllStaff()
		{
			string[] recipients = { "ClientCertificate@test.com", "ClientCertificate2@test.com", "ClientCertificate3@test.com" };
			CreateGroupWithUser(recipients);
			using (EDIClientRegistry.Instance.ClientCertificateExpiryNotificationDuration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			{
				SetUpTestClientAuthentication("Test Client 1", "IN", true, true);
				var task = new EDIClientNotificationServiceTask();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				AssertEquals("Fallback to all staff", true, ((TestServiceLogger)task.ServiceLogger).ToString().Contains("Try to get email addresses from all staff."));
				AssertEquals("Email sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(GetExpectedMailContent("Test Client 1", "Inbound", expired: true), recipients, Env.OutgoingMailManager.EmailsCreated[0]);
			}
		}

		public IEDICommunicationPartyConfig SetUpTestClientAuthentication(string clientName, string direction, bool activeConfig, bool activeClient)
		{
			var communicationPartyConfig = Factory.New<EDICommunicationPartyConfig>();
			var communicationParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var communicationAuth = Factory.New<EDICommunicationAuth>();
			communicationAuth.ECA_Certificate = Encoding.UTF8.GetBytes(Certificate);
			communicationAuth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			communicationPartyConfig.ECC_Direction = direction;
			communicationAuth.ECA_FlowCode = (direction == "OUT") ? EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCertificate : "";
			communicationPartyConfig.ECC_IsActive = activeConfig;
			communicationParty.ECP_Name = clientName;
			communicationParty.ECP_IsActive = activeClient;
			communicationPartyConfig.ECC_ECP_Party = communicationParty.PK;
			communicationPartyConfig.ECC_ECA_Auth = communicationAuth.PK;
			Factory.Save();

			return communicationPartyConfig;
		}

		(GlbGroup group, GlbStaff[] staff) CreateGroupWithUser(string[] userEmails)
		{
			var group = CreateGroup("SGR");

			var staffArray = userEmails.Select((email, index) =>
			{
				var staff = CreateStaff($"SU{index}", $"Test user {index}", email);
				CreateGroupLink(group, staff);
				return staff;
			}).ToArray();

			return (group, staffArray);
		}
		GlbGroup CreateGroup(string groupName)
		{
			var result = Factory.New<GlbGroup>();
			result.GG_Code = groupName;
			result.GG_Desc = "Call Me " + groupName;
			Factory.Save();
			return result;
		}

		GlbStaff CreateStaff(string code, string userName, string userEmail)
		{
			var result = Factory.New<GlbStaff>();
			result.GS_Code = code;
			result.GS_LoginName = userName;
			result.GS_IsSystemAccount = false;
			result.GS_EmailAddress = userEmail;
			Factory.Save();
			return result;
		}

		void CreateGroupLink(GlbGroup group, GlbStaff staff)
		{
			var result = Factory.New<GlbGroupLink>();
			result.GK_GG = group.PK;
			result.GK_GS = staff.PK;
			Factory.Save();
		}

		(string subject, string body) GetExpectedMailContent(string name, string direction, bool expired)
		{
			var appDesc = "eAdaptorNext";
			var authority = "L=WTG NJG, CN=NJG1, S=Nanjing, OU=IdentityAndSecurity, O=jaywtgCA, C=CN";
			var expiryDate = System.TimeZoneInfo.ConvertTimeFromUtc(new DateTime(2024, 2, 24, 3, 31, 59, DateTimeKind.Utc), System.TimeZoneInfo.Local).ToString("yyyy-MM-dd HH:mm:ss");

			if (expired)
			{
				var expectedSubject = string.Format("Action Required: {0} – {1} certificate has expired", name, direction);
				var expectedBody = string.Format(@"<br><br>
    Dear Customer,
    <br><br>
    Your {0} EDI Client {1} {2} certificate has expired. This will prevent the communications on the client from working.
    <ul>
    <li>Connection name - {1}</li>
    <li>Certificate Expiry Date - {3}</li>
    <li>Certificate Issuing Authority - {4}</li>
    </ul>
    Please review your certificate and upload it at the earliest.
    <br>
    Refer to the eAdaptor Next Developer Guide for more details.
    <br><br>
    Regards,
    <br>
    CargoWise Support
    <br>", appDesc, name, direction, expiryDate, authority);
				return (expectedSubject, expectedBody);
			}
			else
			{
				var expectedSubject = string.Format("Action Required: {0} – {1} certificate will expire soon", name, direction);
				var expectedBody = string.Format(@"<br><br>
    Dear Customer,
    <br><br>
    Your {0} EDI Client {1} {2} certificate is due to expire soon.
    <ul>
    <li>Connection name - {1}</li>
    <li>Certificate Expiry Date - {3}</li>
    <li>Certificate Issuing Authority - {4}</li>
    </ul>
    Please review your certificate and upload it at the earliest.
    <br>
    Refer to the eAdaptor Next Developer Guide for more details.
    <br><br>
    Regards,
    <br>
    CargoWise Support
    <br>", appDesc, name, direction, expiryDate, authority);
				return (expectedSubject, expectedBody);
			}
		}

		public void AssertEmail((string subject, string body) expectedMailContent, string[] expectedRecipients, EmailDef email)
		{
			var body = email.Body;
			AssertEquals("Subject", expectedMailContent.subject, email.Subject);
			AssertContains("Email body should contain", expectedMailContent.body, body);
			AssertContainsExactElementsInAnyOrder("Recipients", expectedRecipients, email.Recipients.Cast<RecipientDef>().Select(x => x.Email));
		}

		const string Certificate = @"-----BEGIN CERTIFICATE-----
MIID6DCCAtCgAwIBAgIQROOIIrCQ7LRi0hKk4rLEdDANBgkqhkiG9w0BAQsFADBx
MQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lkZW50
aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5KRzEx
EDAOBgNVBAcMB1dURyBOSkcwHhcNMjMwMjI0MDIzMTU5WhcNMjQwMjI0MDMzMTU5
WjBxMQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lk
ZW50aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5K
RzExEDAOBgNVBAcMB1dURyBOSkcwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEK
AoIBAQCVJt0UDpMaAOMxUiBtstzgTVtBC541t5+mGCS8wOmOdMCspwU1jkC6w0VB
sh0ZwFkGJyu51aEOkqES2oTR/G7/ISj7iZO2Hx4C7dlShm31gDlp2sgNkWls+Acg
HsAvu1GpHEXQjMv1gtPZr5uC3ys9uY5zm5XyirCno4+AEzntZ9Bxvupu1cmuS9Z7
xFdFXuBq4pmb7s6vGp3bMMabEimvlRkg1EiaIJLlLVxbb4til3jmh+wkf6o3R2KS
V65+f4F6XZvA5vsB7rUv7HDvuSNNGVdtFSQ9RxnzKOSyemypSM+CPQyDNcWJZSjd
51o58wEEv7jQrvu5NmeggXMiFDR3AgMBAAGjfDB6MAkGA1UdEwQCMAAwHwYDVR0j
BBgwFoAUxRkU5BydYIvZdbTEsI6oOFnvAuIwHQYDVR0OBBYEFMUZFOQcnWCL2XW0
xLCOqDhZ7wLiMA4GA1UdDwEB/wQEAwIFoDAdBgNVHSUEFjAUBggrBgEFBQcDAQYI
KwYBBQUHAwIwDQYJKoZIhvcNAQELBQADggEBAFd9ujf8VlE2UxAqjTmaAddX/FKU
NHSWILGSjOZm6Lb2nz0267Al8G71NiLdwDAPEH7sBwKUIXZBKoLJU9pBxSshxkf0
lX+4bKUvHkiDf/GDg9X1RqX7sVgglPaR3FmQNxEvbs6lD+rWar6ZtHOX2Kaqrb/7
szao5Hnoo7U8CL4Lm0ZDZ+nxP9gwq3W83KDLsGHHNld0i9zl55Lzt1vCoUsqJlsJ
XPyDaHxT4m7lrI+fv8HAC2KclNkEf9xYTqDfwHpbWKQtV54gYG568D+kyptHDsgv
7qiJtBmvKf+O4g7p9NPjcQdfoHq2IrKzZO9kOANlcVE0UCXBk4Msi4VguJ0=
-----END CERTIFICATE-----";
	}
}
