using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.TrustedMessaging;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.TrustedMessaging.Business
{
	public class RemoteApiService : MyAccountClient
	{
		public RemoteApiService(ITrustedClientConfiguration configuration) : base(configuration)
		{
			DatabaseNumber = ObjectFactory.Get<IProductRegistration>().Key.DatabaseNumber.ToString(CultureInfo.CurrentCulture);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public async Task<TrustedResponse<bool>> DownloadCertificatesAsync(CancellationToken cancellationToken)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var reg = ObjectFactory.Get<IProductRegistration>();
				var regKey = reg?.Key;

				if (reg == null || regKey == null ||
					regKey.DatabaseNumber == 0 || string.IsNullOrEmpty(regKey.Password) ||
					reg.LocalVerify() != ProductRegistrationVerifyResult.OK)
				{
					return new TrustedResponse<bool>() { Success = false };
				}
				else
				{
					var result = await ActivationAsyncCore(regKey.DatabaseNumber.ToString(CultureInfo.InvariantCulture), regKey.Password, cancellationToken);

					if (result.Success)
					{
						var certs = result.Response;

						if (certs.LocalCertificate != null && certs.LocalCertificate.GetRSAPrivateKey() != null && certs.LocalCertificate.GetRSAPublicKey() != null &&
							certs.RemoteCertificate != null && certs.RemoteCertificate.GetRSAPublicKey() != null)
						{
							var certPassword = string.IsNullOrEmpty(certs.LocalCertificatePassword) ? ZGuid.NewZGuid().ToString() : certs.LocalCertificatePassword;
							var localCert = certs.LocalCertificate.Export(X509ContentType.Pfx, certPassword);
							var remoteCert = certs.RemoteCertificate.Export(X509ContentType.Cert);

							if (localCert != null && localCert.Any() && remoteCert != null && remoteCert.Any())
							{
								WebDataRegistry.Instance.TrustedMessagingCentralSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, remoteCert);
								WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, localCert);
								WebDataRegistry.Instance.TrustedMessagingClientSystemCertificatePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certPassword);
								return new TrustedResponse<bool>() { Success = true, Response = true };
							}
						}

						return new TrustedResponse<bool>() { Messages = new[] { new ErrorMessage() { Code = "500", Message = (NoResString)"Invalid Response Received" } } };
					}
					else
					{
						return new TrustedResponse<bool>() { Messages = result.Messages };
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		protected async virtual Task<TrustedResponse<CertificatePair>> ActivationAsyncCore(string databaseNumber, string password, CancellationToken token)
			=> await ActivationAsync(databaseNumber, password, new ActivationQueryString(), token).AwaitSmart();

		#region FeatureControl

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "logs only, literal string is safe to use in this context.")]
		public async Task<TrustedResponse<bool>> DownloadFeatureControlRuleAsync(ILogger logger, CancellationToken token)
		{
			var featureControlRuleRepository = ObjectFactory.Get<IFeatureControlRuleRepository>();
			var ruleTimestampUtc = featureControlRuleRepository.GetFeatureControlTimestampUtc();

			var info = new FeatureControlRequest()
			{
				Product = "CW1",
				SystemId = DatabaseNumber,
				InfoExpires = ZDateTime.UtcNow.AddDays(1).ToDateTime(),
				RuleTimestampUtc = ruleTimestampUtc
			};
			var result = await SendRequestCore<FeatureControlRequest, FeatureControlResponse>(info, info.Product, info.SystemId, "/FeatureControl/Rule", token).AwaitSmart();

			if (result.Success)
			{
				if (ruleTimestampUtc != result.Response.RuleTimestampUtc)
				{
					if (featureControlRuleRepository.SaveFeatureControlRuleContent(result.Response.RuleContent))
					{
						logger?.Information("Successfully downloaded the latest feature control rules.");
					}
					else
					{
						logger?.Error("Encountered an invalid format in the feature control rules.");
					}
				}
				else
				{
					logger?.Information("Feature control rules are already up to date; skipping download.");
				}

				return new TrustedResponse<bool>() { Success = true, Response = true };
			}
			else
			{
				result.Log(logger);
				return new TrustedResponse<bool>() { Messages = result.Messages };
			}
		}

		protected async virtual Task<TrustedResponse<TResponse>> SendRequestCore<TRequest, TResponse>(TRequest request, string product, string systemId, string relativeUrl, CancellationToken token)
			=> await SendRequest<TRequest, TResponse>(request, product, systemId, relativeUrl, token).AwaitSmart();

		#endregion FeatureControl	

		protected override void OnCertificateMismatched()
		{
			base.OnCertificateMismatched();

			WebDataRegistry.Instance.TrustedMessagingCentralSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<byte>());
			WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<byte>());
			WebDataRegistry.Instance.TrustedMessagingClientSystemCertificatePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
		}

		class ActivationQueryString : SecureQueryString, ISecureQueryString
		{
		}

		public string DatabaseNumber { get; }
	}
}
