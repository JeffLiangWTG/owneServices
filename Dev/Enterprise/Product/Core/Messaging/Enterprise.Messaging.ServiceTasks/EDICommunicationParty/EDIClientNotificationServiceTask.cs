using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"ECS",
	"EDI Client Notification Service",
	"ESV",
	typeof(Enterprise.Messaging.ServiceTasks.EDIClientNotificationServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1day",
	DefaultScheduleRunEvery = "3day"
	)]

namespace Enterprise.Messaging.ServiceTasks
{
	public class EDIClientNotificationServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			ServiceLogger?.Log(LogType.Information, (NoResString)"Checking EDI client Certificates Started.");
			var toBeNotifiedInboundCommunicationPartyConfigs = GetToBeNotifiedInboundCommunicationPartyConfigs();
			if (toBeNotifiedInboundCommunicationPartyConfigs.Count > 0)
			{
				ServiceLogger?.Log(LogType.Information, (NoResString)"Found expired or to be expired cert.");
				ServiceLogger?.Log(LogType.Information, (NoResString)"Sending Reminder Started.");
				Notify(toBeNotifiedInboundCommunicationPartyConfigs);
			}
			ServiceLogger?.Log(LogType.Information, (NoResString)"Checking EDI client Certificates Completed.");
		}
		void Notify(List<EDICommunicationPartyConfig> configs)
		{
			var emailSender = new HtmlNotificationEmailSender();
			var groupEmails = TryGetEHubNotificationEmails();
			if (groupEmails.Count == 0)
			{
				ServiceLogger?.Log(LogType.Warning, (NoResString)"No email address has been found.");
			}
			else
			{
				configs.ForEach(config =>
				{
					try
					{
						ServiceLogger?.Log(LogType.Information, $"Sending mail for {config.Party.ECP_Name} – {config.ECC_Direction}");
						var cert = new X509Certificate2(config.Auth.ECA_Certificate);
						var expired = cert.NotAfter <= ZDateTime.Now;
						var direction = config.ECC_Direction == EDICommunicationPartyConfigDirectionsList.Codes.Inbound ? EDICommunicationPartyConfigDirectionsList.Descriptions.Inbound : EDICommunicationPartyConfigDirectionsList.Descriptions.Outbound;
						var subject = expired ? Res.GetString("987479a6-538c-442b-b0fb-eda65f8af2c9", "Action Required: {0} – {1} certificate has expired", config.Party.ECP_Name, direction)
						: Res.GetString("907d96d6-69e5-4ea4-922b-15b098f6689f", "Action Required: {0} – {1} certificate will expire soon", config.Party.ECP_Name, direction);

						var body = PrepareBody(config.Party.ApplicationDescriptor.Description, config.Party.ECP_Name, direction, cert.NotAfter.ToString("yyyy-MM-dd HH:mm:ss"), cert.Issuer, expired);

						var email = emailSender.CreateEmail(subject, body);
						email.AddRecipientForUserCommunication(groupEmails);
						Env.OutgoingMailManager.CreateAndSave(email);
					}
					catch (EmailSendFailedException ex)
					{
						ServiceLogger?.Log(LogType.Error, (NoResString)"Email send failed for {config.Party.ECP_Name} – {config.ECC_Direction}", ex);
					}
				});
			}
		}

		List<EDICommunicationPartyConfig> GetToBeNotifiedInboundCommunicationPartyConfigs()
		{
			var query = EDICommunicationAuthSchemaQuery();
			var subquery = new ZDBOnlySubQuery(typeof(EDICommunicationAuth), EDICommunicationAuthSchema.PK);
			subquery.AddToFilter(EDICommunicationAuthSchema.ECA_AuthorizationMode, EDICommunicationAuthModesList.Codes.OAuthAuthentication);
			query.AddSubQuery(EDICommunicationPartyConfigSchema.ECC_ECA_Auth, subquery, JoinCondition.And);
			var fetchedCommunicationPartyConfigs = new ReadOnlyBusinessObjectFactory().Load<EDICommunicationPartyConfig>(query);
			var toBeNotifiedInboundCommunicationPartyConfigs = fetchedCommunicationPartyConfigs
				.Where(config => config.Auth.ECA_Certificate != null && WithinNotificationWindow(new X509Certificate2(config.Auth.ECA_Certificate).NotAfter))
				.ToList();
			return toBeNotifiedInboundCommunicationPartyConfigs;
		}

		ZDBOnlyQuery EDICommunicationAuthSchemaQuery()
		{
			var query = new ZDBOnlyQuery(typeof(EDICommunicationPartyConfig));
			query.AddToFilter(EDICommunicationPartyConfigSchema.ECC_IsActive, true);

			var partySubQuery = new ZDBOnlySubQuery(typeof(EDICommunicationParty), EDICommunicationPartyConfigSchema.ECC_ECP_Party);
			partySubQuery.AddToFilter(EDICommunicationPartySchema.ECP_IsActive, true);
			query.AddSubQuery(partySubQuery, JoinCondition.And);

			return query;
		}

		bool WithinNotificationWindow(DateTime expireDate) => expireDate <= ZDateTime.Today.AddDays(NotificationDuration) && expireDate >= ZDateTime.Today.AddDays(-NotificationDuration);

		public Guid NotificationGroup => EDIClientRegistry.Instance.ClientCertificateExpiryNotificationGroup.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);

		public int NotificationDuration => EDIClientRegistry.Instance.ClientCertificateExpiryNotificationDuration.Value;

		StringCollection TryGetEHubNotificationEmails()
		{
			var emailGroupUtility = new EmailGroupUtility();
			ServiceLogger?.Log(LogType.Information, $"Try to get email addresses from notification group {NotificationGroup}.");
			var groupEmails = emailGroupUtility.GetGroupEmailCollection(NotificationGroup, false);

			if (groupEmails.Count == 0)
			{
				ServiceLogger?.Log(LogType.Information, (NoResString)"Notification Email Group is not setup or does not contain any users with email addresses, try to get email addresses from company notification group.");
				groupEmails = emailGroupUtility.GetCompanyNotificationGroupEmails();
			}

			if (groupEmails.Count == 0)
			{
				ServiceLogger?.Log(LogType.Information, (NoResString)"Try to get email addresses from all staff.");
				groupEmails = emailGroupUtility.GetStaffEmailCollection(false);
			}

			return groupEmails;
		}

		string PrepareBody(string appDesc, string name, string direction, string expiryDate, string authority, bool expired)
		{
			if (expired)
			{
				return Res.GetString("f6b97cbe-c5e1-4f5f-b947-a64e3717508e", @"<br><br>
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
			}
			else
			{
				return Res.GetString("9cc05583-68e1-4e23-a83b-e9401edf14ca", @"<br><br>
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
			}
		}
	}
}
