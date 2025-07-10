using System;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Licensing.ServiceTasks;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using WTG.TrustedMessaging.MyAccount.Models;

[assembly: HostedService(
	LicenseAgreementService.Code,
	LicenseAgreementService.Description,
	"SYS",
	typeof(LicenseAgreementService),
	MinimumPeriod = "15Minutes",
	DefaultScheduleRunEvery = "12hours",
	ActiveByDefault = true,
	IsMandatory = true)]
namespace Enterprise.Licensing.ServiceTasks
{
	public class LicenseAgreementService : ServiceProviderImpl
	{
		public const string Code = "LAG";
		public const string Description = "License Agreement Service";
		string ProductCode => LicenseAgreementTypeList.Codes.CargoWiseNext;
		IUserPortalClient Client => clientDoNotAccessThis ?? (clientDoNotAccessThis = ObjectFactory.Get<IUserPortalClient>());
		IUserPortalClient clientDoNotAccessThis;

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Debug, "Start");

			if (!AreCertificatesReady)
			{
				ServiceLogger.Log(LogType.Debug, "End - The certificates have not yet been downloaded by the TMS service task.");
				return;
			}

			if (!ImportExistingAcceptedLicenseAgreements())
			{
				// Avoid bug if service is temporarily down. We don't want to force signing
				return;
			}

			if (!ImportNewUnsignedLicenseAgreements())
			{
				return;
			}

			UploadNewlySignedLicenseAgreements();
			ServiceLogger.Log(LogType.Debug, "End");
		}

		bool ImportExistingAcceptedLicenseAgreements()
		{
			var existingAgreementMessages = Client.GetAcceptancesAsync(ProductCode)
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();

			if (!existingAgreementMessages.Success)
			{
				ServiceLogger.Log(LogType.Warning, "GetAcceptances Request failed");
				return false;
			}

			ServiceLogger.Log(LogType.Debug, "GetAcceptances Request succeeded");
			ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
			{
				var acceptances = existingAgreementMessages.Response?.Acceptances ?? Enumerable.Empty<LicenseAcceptanceResponse>();
				var acceptanceFactory = GetFactory();
				var query = new ZQuery(LicenseAgreementSchema.LAG_Type, ProductCode);
				var existing = acceptanceFactory.Load<LicenseAgreement>(query);
				bool hasNewRemoteAcceptances = false;

				foreach (var acceptance in acceptances)
				{
					foreach (var item in existing.Where(l => MajorVersionPending(l, acceptance)))
					{
						// Cancel the existing pending items on same major version and variant that were already signed.
						item.LAG_Status = LicenseAgreementStatusList.Codes.Cancelled;
					}

					if (!existing.Any(l => AcceptanceEqual(l, acceptance)))
					{
						var newAgreement = acceptanceFactory.New<LicenseAgreement>();
						using (newAgreement.GetValidationSuspender())
						{
							newAgreement.LAG_MajorVersion = acceptance.MajorVersion;
							newAgreement.LAG_MinorVersion = acceptance.MinorVersion;
							newAgreement.LAG_VariantCode = acceptance.Variant ?? string.Empty;
							newAgreement.LAG_Content = acceptance.Content;
							newAgreement.LAG_Title = acceptance.Title;
							newAgreement.LAG_Type = ProductCode;
							newAgreement.LAG_Status = LicenseAgreementStatusList.Codes.Accepted;
							newAgreement.LAG_AcceptedIPAddress = acceptance.AcceptedIPAddresss;
							newAgreement.LAG_AcceptedByName = acceptance.AcceptedByName;
							newAgreement.LAG_AcceptedByEmail = acceptance.AcceptedByEmail;
							newAgreement.LAG_AcceptedTimeUtc = new ZDateTime(acceptance.AcceptedTimeUtc, DateTimeKind.Utc);
							newAgreement.LAG_EffectiveStartUtc = new ZDateTime(acceptance.EffectiveStartUtc, DateTimeKind.Utc);
						}
						hasNewRemoteAcceptances = true;
						ServiceLogger.Log(LogType.Information, FormattableString.Invariant($"Imported acceptance. Version number: {newAgreement.VersionNumber}, Variant: {newAgreement.LAG_VariantCode}, Party: {newAgreement.LAG_AcceptedByName}"));
					}
					else
					{
						// Agreement is already in the system and has been accepted.
					}
				}

				if (hasNewRemoteAcceptances)
				{
					// Cancel all other pending agreements as they are superseded by new acceptance version
					CancelPendingAgreements(existing);
				}

				acceptanceFactory.Save();
			},
			() => { ServiceLogger.Log(LogType.Debug, "Some kind of concurrency error occured on save"); });

			return true;
		}

		bool ImportNewUnsignedLicenseAgreements()
		{
			var agreementMessage = Client.GetUserAgreementAsync(ProductCode)
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();

			if (!agreementMessage.Success)
			{
				ServiceLogger.Log(LogType.Warning, "GetUserAgreement Request failed", agreementMessage.InnerException);
				return false;
			}

			var response = agreementMessage.Response;
			if (response == null)
			{
				return false;
			}
			ServiceLogger.Log(LogType.Debug, "GetUserAgreement Request succeeded");

			// Cancellation of existing records could collide with acceptance, so we need retry.
			ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
			{
				var newAgreementFactory = GetFactory();
				var query = new ZQuery(LicenseAgreementSchema.LAG_Type, ProductCode);
				var existing = newAgreementFactory.Load<LicenseAgreement>(query);

				if (!response.Required)
				{
					ServiceLogger.Log(LogType.Debug, "Skipping response since it isn't required");
					CancelPendingAgreements(existing);
					newAgreementFactory.Save();
				}
				else if (!existing.Any(l => VersionsMatch(l, response)))
				{
					CancelPendingAgreements(existing);
					var newAgreement = LicenseAgreement.ImportNewAgreement(newAgreementFactory, response, ProductCode);

					//Major version has been accepted before, new downloaded agreement has minor version updated, and is set as cancelled so no need to accept again
					if (existing.Any(l => MajorVersionAccepted(l, response)))
					{
						newAgreement.LAG_Status = LicenseAgreementStatusList.Codes.Cancelled;
					}

					newAgreementFactory.Save(); // Don't ever expect exceptions during save here.
					ServiceLogger.Log(LogType.Information, FormattableString.Invariant($"Downloaded new license. Version number: {newAgreement.VersionNumber}, Variant: {newAgreement.LAG_VariantCode}"));
				}
				else
				{
					// Agreement is already in the system but hasn't been accepted yet.
					if (!existing.Any(l => MajorVersionAccepted(l, response)))
					{
						// Agreement is already in the system but hasn't been accepted yet.
						// This is a retry of a previously failed download (e.g. due to network issues
						var existingCancelled = existing.Where(l => l.LAG_Status == LicenseAgreementStatusList.Codes.Cancelled && VersionsMatch(l, response))
							.OrderByDescending(c => c.LAG_SystemCreateTimeUtc)
							.ToList().FirstOrDefault();
						if (existingCancelled != null)
						{
							//Agreement is cancelled previously but is required now, reset to pending and get title/content up to date
							existingCancelled.LAG_Status = LicenseAgreementStatusList.Codes.Pending;
							existingCancelled.LAG_Content = response.Content;
							existingCancelled.LAG_Title = response.Title;
							newAgreementFactory.Save();
						}
					}
				}
			},
			() => { ServiceLogger.Log(LogType.Debug, "Some kind of concurrency error occured on save"); });

			return true;
		}

		void UploadNewlySignedLicenseAgreements()
		{
			var uploadFactory = GetFactory();
			var uploadQuery = new ZQuery(LicenseAgreementSchema.LAG_Status, LicenseAgreementStatusList.Codes.Queued);
			var agreements = uploadFactory.Load<LicenseAgreement>(uploadQuery);

			if (agreements.Length == 0)
			{
				return;
			}

			ServiceLogger.Log(LogType.Information, FormattableString.Invariant($"Begin uploading {agreements.Length} license agreements"));
			foreach (var queuedAgreement in agreements)
			{
				var version = new IUserAgreementSignerDetails.AgreementVersion(queuedAgreement.LAG_MajorVersion, queuedAgreement.LAG_MinorVersion, queuedAgreement.LAG_VariantCode);
				var details = new SignerDetails
				{
					Type = queuedAgreement.LAG_Type,
					Name = queuedAgreement.LAG_AcceptedByName,
					Email = queuedAgreement.LAG_AcceptedByEmail,
					AgreementDateUtc = queuedAgreement.LAG_AcceptedTimeUtc.ToDateTime(),
					IPAddress = queuedAgreement.LAG_AcceptedIPAddress,
					Version = version,
				};
				var result = Client.SignAgreementAsync(details)
						.ConfigureAwait(false)
						.GetAwaiter()
						.GetResult();

				if (result != null && result.Success)
				{
					queuedAgreement.LAG_Status = LicenseAgreementStatusList.Codes.Accepted;
					uploadFactory.Save();
					ServiceLogger.Log(LogType.Information, FormattableString.Invariant($"Uploaded license agreement acceptance"));
				}
				else
				{
					ServiceLogger.Log(LogType.Warning, FormattableString.Invariant($"Unable to upload agreement {string.Join(System.Environment.NewLine, result?.Messages.Select(s => s.Message) ?? Enumerable.Empty<string>())}"));
				}
			}
		}

		static bool MajorVersionPending(LicenseAgreement local, LicenseAcceptanceResponse remote)
		{
			return local.LAG_Status == LicenseAgreementStatusList.Codes.Pending
				&& local.LAG_MajorVersion.EqualsIgnoringCase(remote.MajorVersion)
				&& local.LAG_VariantCode.EqualsIgnoringCase(remote.Variant ?? string.Empty);
		}

		static bool AcceptanceEqual(LicenseAgreement local, LicenseAcceptanceResponse remote)
		{
			// We make the assumption that minor and major version and variant are always unique for title + content.
			// We also assume that the same person accepting the same license multiple times isn't meaningful.
			return local.LAG_MajorVersion.EqualsIgnoringCase(remote.MajorVersion)
				&& local.LAG_MinorVersion.EqualsIgnoringCase(remote.MinorVersion)
				&& local.LAG_VariantCode.EqualsIgnoringCase(remote.Variant ?? string.Empty)
				&& local.LAG_AcceptedByName.EqualsIgnoringCase(remote.AcceptedByName);
		}

		static bool MajorVersionAccepted(LicenseAgreement local, UserAgreementResponseData remote)
		{
			return (local.LAG_Status == LicenseAgreementStatusList.Codes.Accepted
				|| local.LAG_Status == LicenseAgreementStatusList.Codes.Queued)
				&& local.LAG_MajorVersion.EqualsIgnoringCase(remote.VersionNumber.ToString())
				&& local.LAG_VariantCode.EqualsIgnoringCase(remote.Variant ?? string.Empty);
		}

		static bool VersionsMatch(LicenseAgreement local, UserAgreementResponseData remote)
		{
			return local.LAG_MajorVersion.Equals(remote.VersionNumber.ToString())
				&& local.LAG_MinorVersion.Equals(remote.MinorVersionNumber.ToString())
				&& local.LAG_VariantCode.EqualsIgnoringCase(remote.Variant ?? string.Empty);
		}

		static void CancelPendingAgreements(LicenseAgreement[] existing)
		{
			foreach (var item in existing.Where(item => item.LAG_Status == LicenseAgreementStatusList.Codes.Pending))
			{
				item.LAG_Status = LicenseAgreementStatusList.Codes.Cancelled;
			}
		}

		static BusinessObjectFactory GetFactory()
		{
			var factory = new BusinessObjectFactory()
			{
				NameForDebugging = FormattableString.Invariant($"{nameof(LicenseAgreementService)} Factory"),
				RefreshEnabled = false,
			};
			factory.SuspendValidation();
			return factory;
		}

		bool AreCertificatesReady
			=> !IsNullOrEmpty(WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.Value) &&
				!string.IsNullOrWhiteSpace(WebDataRegistry.Instance.TrustedMessagingClientSystemCertificatePassword.Value) &&
				!IsNullOrEmpty(WebDataRegistry.Instance.TrustedMessagingCentralSystemCertificate.Value);

		bool IsNullOrEmpty(byte[] bytes) => bytes == null || !bytes.Any();

		class SignerDetails : IUserAgreementSignerDetails
		{
			public string Type { get; set; }
			public string Name { get; set; }
			public string Email { get; set; }
			public string IPAddress { get; set; }
			public DateTime AgreementDateUtc { get; set; }
			public IUserAgreementSignerDetails.AgreementVersion Version { get; set; }
		}
	}
}
