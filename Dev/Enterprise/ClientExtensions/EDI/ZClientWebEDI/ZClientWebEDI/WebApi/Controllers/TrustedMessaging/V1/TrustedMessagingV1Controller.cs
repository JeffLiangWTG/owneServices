using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/TrustedMessaging")]
	public class TrustedMessagingV1Controller : TrustedMessagingBaseController
	{
		public TrustedMessagingV1Controller() : base()
		{
		}

		public TrustedMessagingV1Controller(NLogWrapper logger) : base(logger)
		{
		}

		#region Activation

		const string CertificationGenerationLockKey = "CertificationGenerationLockKey";

		[HttpPost]
		[Route("Activation")]
		public IHttpActionResult Activation()
		{
			const string product = ProductTypes.Codes.CargoWiseOne;
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = $"api/TrustedMessaging/Activation";
			AddInfoLog("Request received", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath, product: product);

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					var bodyContent = Request.Content.ReadAsStringAsync().Result;

					if (!string.IsNullOrWhiteSpace(bodyContent))
					{
						return Activation(bodyContent, sessionId);
					}
					else
					{
						var warn = $"{ErrorCodes.Codes.Critical_MessageMalformed} {ErrorCodes.Descriptions.Critical_MessageMalformed}";
						AddWarnLog(warn, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: product);
						return BadRequest(ErrorCodes.Codes.Critical_MessageMalformed, ErrorCodes.Descriptions.Critical_MessageMalformed);
					}
				}
				catch (Exception ex) when (ex is CryptographicException || ex is JsonException || ex is FormatException)
				{
					var error = $"requset {routingPath} get error";
					AddErrorLog(error, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, product: product, ex: ex);
					return InternalServerError(ex.Message);
				}
			}
		}

		IHttpActionResult Activation(string queryString, string sessionId)
		{
			const string product = ProductTypes.Codes.CargoWiseOne;
			var routingPath = $"api/TrustedMessaging/Activation";

			var cw1ActivationInfo = JsonConvert.DeserializeObject<CW1ActivationInfo>(new SecureQueryString(queryString)[nameof(CW1ActivationInfo)]);

			if (cw1ActivationInfo == null || string.IsNullOrWhiteSpace(cw1ActivationInfo.DatabaseNumber) || !int.TryParse(cw1ActivationInfo.DatabaseNumber, out var dbNumberAsInt)
				|| string.IsNullOrWhiteSpace(cw1ActivationInfo.Password) || null == cw1ActivationInfo.PublicKey || !cw1ActivationInfo.PublicKey.Any())
			{
				var warn = $"{ErrorCodes.Codes.Validation_MissingRequiredField} {ErrorCodes.Descriptions.Validation_MissingRequiredField}";
				AddWarnLog(warn, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: product);
				return BadRequest(ErrorCodes.Codes.Validation_MissingRequiredField, ErrorCodes.Descriptions.Validation_MissingRequiredField);
			}

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var database = factory.LoadTop1<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, dbNumberAsInt));
			if (database == null || database.LicEnterprise == null)
			{
				var warn = $"{ErrorCodes.Codes.Validation_InvalidSystem} {ErrorCodes.Descriptions.Validation_InvalidSystem}";
				AddWarnLog(warn, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: product, systemId: dbNumberAsInt.ToString());
				return BadRequest(ErrorCodes.Codes.Validation_InvalidSystem, ErrorCodes.Descriptions.Validation_InvalidSystem);
			}

			var isInternalSystem = database.LicEnterprise.LE_IsInternal;

			var soapTemplate = EDIDataRegistry.Instance.MyAccountCertificateAuthoritySoapRequestTemplate.Value;
			var userAccount = EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccount.Value;
			var userAccountPassword = EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccountPassword.Value;
			var internalActivationCert = EDIDataRegistry.Instance.InternalCW1ActivationCertificate.Value;

			if (isInternalSystem && internalActivationCert.Length == 0)
			{
				var message = $"Internal systems under Enterprise Code {database.EnterpriseCode} are not enabled to register trusted messaging.";
				var warn = $"{ErrorCodes.Codes.Authorization_ActionNotPermitted} {message}";
				AddWarnLog(warn, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: product, systemId: dbNumberAsInt.ToString());
				return BadRequest(ErrorCodes.Codes.Authorization_ActionNotPermitted, message);
			}
			else if (!isInternalSystem && (string.IsNullOrWhiteSpace(soapTemplate) || string.IsNullOrWhiteSpace(userAccount) || string.IsNullOrWhiteSpace(userAccountPassword)))
			{
				AddErrorLog(StatusMessages.CertificateAuthorityNotAvailable, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, product: product);
				return InternalServerError(StatusMessages.CertificateAuthorityNotAvailable);
			}

			var serverCertQuery = new ZQuery(EdiTrustedMessagingConfigSchema.ETM_Product, product);
			serverCertQuery.AddToFilter(EdiTrustedMessagingConfigSchema.ETM_CertificateType, CertificateTypeList.Codes.CentralSystemCertificate);
			var serverCert = factory.LoadTop1<EdiTrustedMessagingConfig>(serverCertQuery)?.GetCertificate();
			if (serverCert == null)
			{
				AddErrorLog(StatusMessages.CentralSystemCertNotAvailable, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, product: product, systemId: dbNumberAsInt.ToString());
				return InternalServerError(StatusMessages.CentralSystemCertNotAvailable);
			}

			if (!database.LD_IsActive)
			{
				AddErrorLog(StatusMessages.DatabaseNotActive, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, product: product, systemId: dbNumberAsInt.ToString());
				return InternalServerError(StatusMessages.DatabaseNotActive);
			}

			if (!new[] { DatabaseStatusList.Codes.REG, DatabaseStatusList.Codes.Preregistered }.Any(x => database.LD_Status.EqualsIgnoringCase(x)))
			{
				AddErrorLog(StatusMessages.DatabaseNotRegistered, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, product: product, systemId: dbNumberAsInt.ToString());
				return InternalServerError(StatusMessages.DatabaseNotRegistered);
			}

			if (!database.ValidateClientSecret(cw1ActivationInfo.Password))
			{
				AddErrorLog(StatusMessages.PasswordMismatch, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, product: product, systemId: dbNumberAsInt.ToString());
				return InternalServerError(StatusMessages.PasswordMismatch);
			}

			var dbPkLock = new List<Guid>(1) { database.PK.ToGuid() }.ApplyAppLocks(CertificationGenerationLockKey);
			if (!dbPkLock.ItemsWithLocks.Any())
			{
				AddErrorLog(StatusMessages.AnotherActivationInProgress, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, product: product, systemId: dbNumberAsInt.ToString());
				return InternalServerError(StatusMessages.AnotherActivationInProgress);
			}

			try
			{
				if (isInternalSystem)
				{
					database.Reload();
					database.GetOrCreateTrustedSystem();
					var internalCertPassword = EDIDataRegistry.Instance.InternalCW1ActivationCertificatePassword.Value;
					var internalCert = new X509Certificate2(EDIDataRegistry.Instance.InternalCW1ActivationCertificate.Value, internalCertPassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
					database.TrustedSystem.GetOrCreateCertificateConfig().ETM_CertificateData = internalCert.Export(X509ContentType.Cert);
					database.TrustedSystem.ETS_Product = ProductTypes.Codes.CargoWiseOne;
					database.Logs.AddNew(Events.CertificateReceived, new KeyValuePair<string, string>(nameof(internalCert.Thumbprint), internalCert.Thumbprint));
					factory.Save();

					var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
					var rsp = new TrustedSystemRegistrationResponse();
					using (var rsa = new RSACryptoServiceProvider())
					{
						rsa.ImportCspBlob(cw1ActivationInfo.PublicKey);
						rsp.Import(serverCert, internalCert, internalCertPassword, rsa);
						httpResponse.Content = new StringContent(JsonConvert.SerializeObject(rsp), Encoding.UTF8, WTG.TrustedMessaging.Constants.HttpMediaTypes.Json);
						var info = $"success | {NLogWrapper.Status.Ok}";
						AddInfoLog(info, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath, product: product, systemId: dbNumberAsInt.ToString());
						return new ResponseMessageResult(httpResponse);
					}
				}
				else
				{
					var subjectName = FormattableString.Invariant($"LD_DatabaseNumber:{database.LD_DatabaseNumber}");
					var requestContext = new EdiCertRequestContext { SystemIdentifier = database.LD_DatabaseNumber.ToString() };
					if (GetCertRequest().TrySubmitSafe(soapTemplate, subjectName, cw1ActivationInfo.PublicKey,
						new NetworkCredential(userAccount, userAccountPassword), requestContext, out var output))
					{
#if DEBUG
						DoAdditionalStuffForTesting(database);
#endif
						var clientCert = ReloadDatabaseAndSetConfig(database, output);
						if (clientCert == null)
						{
							AddErrorLog(StatusMessages.TrustedSystemCertNotAvailable, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, product: product, systemId: dbNumberAsInt.ToString());
							return InternalServerError(StatusMessages.TrustedSystemCertNotAvailable);
						}

						try
						{
							factory.Save();
						}
						catch (ZSaveConcurrencyException)
						{
							clientCert = ReloadDatabaseAndSetConfig(database, output);
							if (clientCert == null)
							{
								AddErrorLog(StatusMessages.TrustedSystemCertNotAvailable, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, product: product, systemId: dbNumberAsInt.ToString());
								return InternalServerError(StatusMessages.TrustedSystemCertNotAvailable);
							}

							factory.Save();
						}

						var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
						var rsp = new TrustedSystemRegistrationResponse();
						rsp.Import(serverCert, clientCert, clientCert.GetRSAPublicKey());
						httpResponse.Content = new StringContent(JsonConvert.SerializeObject(rsp), Encoding.UTF8, WTG.TrustedMessaging.Constants.HttpMediaTypes.Json);
						var info = $"success | {NLogWrapper.Status.Ok}";
						AddInfoLog(info, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath, product: product, systemId: dbNumberAsInt.ToString());
						return new ResponseMessageResult(httpResponse);
					}
					else
					{
						AddErrorLog(output, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, product: product, systemId: dbNumberAsInt.ToString());
						return InternalServerError(output);
					}
				}
			}
			finally
			{
				dbPkLock.Dispose();
			}
		}

		X509Certificate2 ReloadDatabaseAndSetConfig(LicenceDatabase database, string output)
		{
			database.Reload();
			database.GetOrCreateTrustedSystem();

			X509Certificate2 clientCert;
			database.TrustedSystem.GetOrCreateCertificateConfig().ETM_CertificateData = Convert.FromBase64String(output);

			clientCert = database.TrustedSystem?.CertificateConfig?.GetCertificate();
			if (clientCert == null)
			{
				return null;
			}

			database.TrustedSystem.ETS_Product = ProductTypes.Codes.CargoWiseOne;
			database.Logs.AddNew(Events.CertificateReceived, new KeyValuePair<string, string>(nameof(clientCert.Thumbprint), clientCert.Thumbprint));

			return clientCert;
		}

#if DEBUG

		protected virtual void DoAdditionalStuffForTesting(LicenceDatabase database)
		{ }

#endif

		protected virtual IEDICertRequest GetCertRequest() => new EDICertRequest();

		class StatusMessages
		{
			public const string PasswordMismatch = "Password Mismatch";
			public const string CentralSystemCertNotAvailable = "Central System Cert Not Available";
			public const string TrustedSystemCertNotAvailable = "Trusted System Cert Not Available";
			public const string CertificateAuthorityNotAvailable = "Certificate Authority Not Available";
			public const string AnotherActivationInProgress = "Another Activation Is In Progress";
			public const string DatabaseNotActive = "Database Not Active";
			public const string DatabaseNotRegistered = "Database Not Registered";
		}

		#endregion Activation

		#region Certificate Lookup

		[HttpPost]
		[Route("Certificate")]
		public IHttpActionResult Certificate(TrustedRequest trustedRequest)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV1<CertificateInfo, CertificateResponse>.New(trustedRequest, this);
				CertificateCore(context);

				if (context.ResponseInfo == null && context.Messages == null)
				{
					return NotFound();
				}
				else
				{
					return context.CreateHttpActionResult();
				}
			}
		}

		#endregion Certificate Lookup
	}
}
