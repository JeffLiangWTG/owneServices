using System;
using System.Threading;
using Azure.Identity;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.ServiceTasks;
using Enterprise.Client.EDI.ServiceTasks.B2CSecretCheck;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(B2CSecretCheckServiceTask.Code,
	B2CSecretCheckServiceTask.Description,
	"CSP",
	typeof(B2CSecretCheckServiceTask),
	MinimumPeriod = "7day",
	DefaultScheduleRunEvery = "7day",
	ActiveByDefault = false,
	CanRunInAnyBranch = true
	)]

namespace Enterprise.Client.EDI
{
	public class B2CSecretCheckServiceTask : ServiceProviderImpl
	{
		public const string Code = "BSC";
		public const string Description = "B2C shared secret expiry date check";

		public B2CSecretCheckServiceTask()
		{
		}

		internal B2CSecretCheckServiceTask(IB2CSecretChecker secretChecker)
		{
			this.secretChecker = secretChecker;
		}

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			if (B2CSecretChecker.IsCheckerReady(out var notReadyMessage))
			{
				try
				{
					var expiryDate = B2CSecretChecker.GetExpiryDate();

					if (expiryDate - ZDateTime.UtcNow < TimeSpan.FromDays(30))
					{
						ServiceTaskEmailNotification.SendEmailToGroup("Warning: Azure B2C Secret about to expire", $"The WTG B2C Secret Checker service task (code: BSC) on ediProd has detected an impending expiry of one or more secrets used by Azure B2C services. Please take the appropriate actions to renew the secret(s). Expiry date: {expiryDate}", EDIDataRegistry.Instance.CertProcessingNotificationGroup, ServiceLogger);
						ServiceLogger.Information("B2C secret is about to expired, a notification email has been sent.");
					}
					else
					{
						ServiceLogger.Information($"B2C secret is not about to expired yet, expiry date: {expiryDate}");
					}
				}
				catch (Microsoft.Graph.ServiceException ex) when(ex.InnerException is AuthenticationFailedException)
				{
					ServiceLogger.Error("Authentication failed while checking B2C secret expiry date", ex.InnerException);
				}
			}
			else
			{
				ServiceLogger.Warning("The task cannot run due to: " + notReadyMessage);
			}
		}

		IB2CSecretChecker B2CSecretChecker => secretChecker ??= new B2CSecretChecker();
		IB2CSecretChecker secretChecker;
	}
}
