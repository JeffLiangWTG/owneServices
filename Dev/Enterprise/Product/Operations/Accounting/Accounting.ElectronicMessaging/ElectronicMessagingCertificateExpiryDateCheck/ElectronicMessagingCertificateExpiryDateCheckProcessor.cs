using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.ElectronicMessagingCertificateExpiryDateCheck
{
	public interface IElectronicMessagingCertificateExpiryDateCheckProcessor
	{
		void AlertForAlmostExpiryCredentials(GlbCompany company, IEInvoicingCredentialSettings credentialSettings, ZDateTime utcNow, ILogger logger = null, IElectronicMessagingNotificationEmailCreator emailCreator = null, IElectronicMessagingNotificationQueryProvider notificationQueryProvider = null);
	}

	public class ElectronicMessagingCertificateExpiryDateCheckProcessor : IElectronicMessagingCertificateExpiryDateCheckProcessor
	{
		public void AlertForAlmostExpiryCredentials(GlbCompany company, IEInvoicingCredentialSettings credentialSettings, ZDateTime utcNow, ILogger logger = null, IElectronicMessagingNotificationEmailCreator emailCreator = null, IElectronicMessagingNotificationQueryProvider notificationQueryProvider = null)
		{
			Argument.NotNull(company, nameof(company));
			Argument.NotNull(credentialSettings, nameof(credentialSettings));

			if (credentialSettings.IsCompanyCredentialsRequired)
			{
				AlertForAlmostExpiryCredentialsInCompanyLevel(company, emailCreator, notificationQueryProvider, utcNow, logger);
			}

			if (credentialSettings.IsBranchCredentialsRequired)
			{
				AlertForAlmostExpiryCredentialsInBranchLevel(company, emailCreator, notificationQueryProvider, utcNow, logger);
			}
		}

		void AlertForAlmostExpiryCredentialsInCompanyLevel(GlbCompany company, IElectronicMessagingNotificationEmailCreator emailCreator, IElectronicMessagingNotificationQueryProvider notificationQueryProvider, ZDateTime utcNow, ILogger logger = null)
		{
			var registrySetting = (EInvoicingCertificateExpiryNotificationGroup)AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup
				.GetValueWithFallbackDefault(company.PK.ToGuid(), Guid.Empty, Guid.Empty);

			if (registrySetting.AlertDays != 0)
			{
				using (DisposableEnvironment.ForCompany(company.GC_Code))
				{
					var localTimeNow = TimeFactory.Instance.GetUnlocoTimeFromUtc(company.OrgProxy.OH_RL_NKClosestPort, utcNow.ToDateTime());
					logger?.Information($"Running alerting for '{company.GC_Code}' ({localTimeNow:yyyy-MM-dd HH:mm:ss}), alerting days before expiry:{registrySetting.AlertDays}.");

					var query = notificationQueryProvider?.GetQueryForCompany(company, localTimeNow, registrySetting.AlertDays)
						?? GetQuery(localTimeNow);

					var almostExpiryCredentials = company.Factory.Load<GlbCompanyEInvoicingCertificateCredential>(query);
					if (almostExpiryCredentials.Length > 0)
					{
						logger?.Information($"Find almost expiry credential(s) for company.");
						var email = emailCreator?.Create(null, company, registrySetting.NotificationGroup.ToGuid(), almostExpiryCredentials.OrderBy(x => x.GP_MailBoxID))
									?? new ElectronicMessagingCertificateExpiryDateCheckEmail(company, registrySetting.NotificationGroup.ToGuid(), almostExpiryCredentials.OrderBy(x => x.GP_MailBoxID));
						email.Send();
					}
					else
					{
						logger?.Information($"None of credential is almost expiry for company.");
					}
				}
			}

			ZQuery GetQuery(DateTime localTimeNow)
			{
				var query = new ZQuery(GlbExternalPasswordSchema.GP_GC, company.PK)
					.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.EIM)
					.AddToFilter(GlbExternalPasswordSchema.GP_Certificate, SQLComparisonOperator.NotEqual, null)
					.AddToFilter(GlbExternalPasswordSchema.GP_GB, SQLComparisonOperator.Equal, DBNull.Value)
					.AddToFilter(GlbExternalPasswordSchema.GP_ExpiryDate, SQLComparisonOperator.LessThanOrEqualTo, localTimeNow.AddDays(registrySetting.AlertDays))
					.AddToFilter(GlbExternalPasswordSchema.GP_ExpiryDate, SQLComparisonOperator.GreaterThan, localTimeNow);

				return query;
			}
		}

		void AlertForAlmostExpiryCredentialsInBranchLevel(GlbCompany company, IElectronicMessagingNotificationEmailCreator emailCreator, IElectronicMessagingNotificationQueryProvider notificationQueryProvider, ZDateTime utcNow, ILogger logger = null)
		{
			var query = notificationQueryProvider?.GetQueryForBranch(company) ?? GetQuery();
			var allBranchCredentials = company.Factory.Load<GlbBranchEInvoicingCertificateCredential>(query);
			if (allBranchCredentials.Length != 0)
			{
				foreach (var branch in company.ActiveBranches.OrderBy(x => x.GB_Code))
				{
					using (DisposableEnvironment.ForBranch(branch.GB_Code))
					{
						var registrySetting = AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup
							.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);

						if (registrySetting.AlertDays <= 0)
						{
							continue;
						}

						var localTimeNow = TimeFactory.Instance.GetUnlocoTimeFromUtc(branch.GB_RL_NKHomePort, utcNow.ToDateTime());

						logger?.Information($"Running alerting for '{company.GC_Code} - {branch.GB_Code}' ({localTimeNow:yyyy-MM-dd HH:mm:ss}), alerting days before expiry:{registrySetting.AlertDays}.");
						var almostExpiryCredentials = allBranchCredentials.Where(x =>
						{
							return x.GP_ExpiryDate <= localTimeNow.AddDays(registrySetting.AlertDays)
								&& x.GP_ExpiryDate > localTimeNow
								&& x.GP_GB == branch.PK;
						});

						if (almostExpiryCredentials.Any())
						{
							logger?.Information($"Find almost expiry credential(s) for branch.");
							var email = emailCreator?.Create(branch, null, registrySetting.NotificationGroup.ToGuid(), almostExpiryCredentials.OrderBy(x => x.GP_MailBoxID))
										?? new ElectronicMessagingCertificateExpiryDateCheckEmail(branch, registrySetting.NotificationGroup.ToGuid(), almostExpiryCredentials.OrderBy(x => x.GP_MailBoxID));
							email.Send();
						}
						else
						{
							logger?.Information($"None of credential is almost expiry for branch.");
						}
					}
				}
			}
			else
			{
				logger?.Information($"None of credential was saved to company[{company.GC_Code}].");
			}

			ZQuery GetQuery()
			{
				var query = new ZQuery(GlbExternalPasswordSchema.GP_GC, company.PK);
				query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.EIM);
				query.AddToFilter(GlbExternalPasswordSchema.GP_Certificate, SQLComparisonOperator.NotEqual, null);
				query.AddToFilter(GlbExternalPasswordSchema.GP_GB, SQLComparisonOperator.NotEqual, DBNull.Value);
				return query;
			}
		}
	}
}
