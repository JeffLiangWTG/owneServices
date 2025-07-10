using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Newtonsoft.Json.Linq;
using NLog;
using WTG.TrustedMessaging;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class HandshakeController : ControllerWithEnvironment
	{
		const string SecretKeyGenerationLockKey = "SecretKeyGenerationLockKey";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Share logger across requests to avoid overhead")]
		internal static NLogWrapper Logger { get; private set; }

		public HandshakeController()
		{
			if (Logger == null)
			{
				Logger = new NLogWrapper(typeof(HandshakeController));
			}
		}

		public HandshakeController(NLogWrapper logger)
		{
			Logger = logger;
		}

		[Route("api/Handshake")]
		[HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IHttpActionResult Post()
		{
			IHttpActionResult actionResult = NotFound();
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = $"api/Handshake";

			string signHeader = GetHeaderValueSafe(Request, "SIGNED");
			string product = GetHeaderValueSafe(Request, "PRODUCT");
			string systemId = GetHeaderValueSafe(Request, "SYSTEMID");
			var infoMsg = $"{routingPath} start";
			Logger.AddLog(LogLevel.Info, infoMsg, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath, product: product, systemId: systemId);

			if (string.IsNullOrWhiteSpace(signHeader) || string.IsNullOrWhiteSpace(product))
			{
				var warnMsg = $"{NLogWrapper.Status.NotFound}";
				Logger.AddLog(LogLevel.Warn, warnMsg, ((int)HttpStatusCode.NotFound), sessionId, routingPath: routingPath, product: product, systemId: systemId);
				return actionResult;
			}

			using (Db.DisposableActionForDbConnection())
			{
				var factory = new BusinessObjectFactory() { RefreshEnabled = false };
				var trustedSystem = EdiTrustedSystem.Load(factory, product, systemId);
				if (trustedSystem == null)
				{
					var warnMsg = $"{ErrorCodes.Descriptions.Validation_InvalidSystem}";
					Logger.AddLog(LogLevel.Warn, warnMsg, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: product, systemId: systemId);
					return new BadRequestWithErrorMessages(new WTG.TrustedMessaging.Models.ErrorMessages(ErrorCodes.Codes.Validation_InvalidSystem, ErrorCodes.Descriptions.Validation_InvalidSystem), this);
				}

				var certProider = ObjectFactory.New<ICertificatesProvider>(trustedSystem);
				RSA privateRsa = certProider.LocalCertificate?.GetRSAPrivateKey();
				RSA publicRsa = certProider.RemoteCertificate?.GetRSAPublicKey();

				if (publicRsa == null)
				{
					var warnMsg = $"{ErrorCodes.Descriptions.Critical_CertificateMismatched}";
					Logger.AddLog(LogLevel.Warn, warnMsg, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: product, systemId: systemId);
					return BadRequestCertificateMismatched();
				}

				byte[] data = Request.Content.ReadAsByteArrayAsync().Result;
				string dataString = Encoding.UTF8.GetString(data);
				try
				{
					data = Convert.FromBase64String(dataString);
					byte[] signData = Convert.FromBase64String(signHeader);
					if (!publicRsa.VerifyData(data, signData, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
					{
						var warnMsg = $"{ErrorCodes.Descriptions.Critical_CertificateMismatched}";
						Logger.AddLog(LogLevel.Warn, warnMsg, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: product, systemId: systemId);
						return BadRequestCertificateMismatched();
					}
				}
				catch (FormatException)
				{
					var warnMsg = $"{ErrorCodes.Descriptions.Critical_MessageMalformed}";
					Logger.AddLog(LogLevel.Warn, warnMsg, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: product, systemId: systemId);
					return new BadRequestWithErrorMessages(new WTG.TrustedMessaging.Models.ErrorMessages(ErrorCodes.Codes.Critical_MessageMalformed, ErrorCodes.Descriptions.Critical_MessageMalformed), this);
				}

				var dbPkLock = new List<Guid>(1) { trustedSystem.PK.ToGuid() }.ApplyAppLocks(SecretKeyGenerationLockKey);
				if (!dbPkLock.ItemsWithLocks.Any())
				{
					var errorMessage = "Another handshake is in progress.";
					var warnMsg = $"{NLogWrapper.Status.BadRequest}: {errorMessage}";
					Logger.AddLog(LogLevel.Warn, warnMsg, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: product, systemId: systemId);
					return new BadRequestWithErrorMessages(new WTG.TrustedMessaging.Models.ErrorMessages(ErrorCodes.Codes.Server_Error, errorMessage), this);
				}

				try
				{
					byte[] encryptedBytes = Convert.FromBase64String(dataString);
					byte[] decryptedBytes = privateRsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
					string msg = Encoding.UTF8.GetString(decryptedBytes);

					var jresult = JObject.Parse(msg);
					if (null != jresult && jresult.HasValues && jresult.ContainsKey("key") && !string.IsNullOrWhiteSpace(jresult["key"].ToString()))
					{
						var remoteKeyString = jresult["key"].Value<string>();
						byte[] remoteKey = Convert.FromBase64String(remoteKeyString);

						using (Db.DisposableActionForDbConnection())
						{
							using var aes = Aes.Create();
							aes.KeySize = 128;
							aes.Padding = PaddingMode.PKCS7;
							byte[] myAccountKey = aes.Key;
							var myAccountKeyString = Convert.ToBase64String(myAccountKey);

							var responseMessage = $"{{ \"key\": \"{myAccountKeyString}\" }}";
							var responseMessageBytes = Encoding.UTF8.GetBytes(responseMessage);
							var encryptedResponseMessageBytes = publicRsa.Encrypt(responseMessageBytes, RSAEncryptionPadding.OaepSHA256);
							var encryptedResponseMessage = Convert.ToBase64String(encryptedResponseMessageBytes);
							byte[] responseSigned = privateRsa.SignData(encryptedResponseMessageBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
							var responseSignedString = Convert.ToBase64String(responseSigned);

							actionResult = new MessageWithSignatureResult(encryptedResponseMessage, responseSignedString, this);
							Logger.AddLog(LogLevel.Info, "success", ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath, product: product, systemId: systemId);

							StoreLocalSecretKey(trustedSystem, remoteKey, myAccountKey);
						}
					}
					else
					{
						var errorMessage = "Secret key is missing.";
						Logger.AddLog(LogLevel.Warn, errorMessage, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: product, systemId: systemId);
						return new BadRequestWithErrorMessages(new WTG.TrustedMessaging.Models.ErrorMessages(ErrorCodes.Codes.Validation_MissingRequiredField, errorMessage), this);
					}
				}
				catch (CryptographicException e)
				{
					var warnMsg = $"{ErrorCodes.Descriptions.Critical_CertificateMismatched}";
					Logger.AddLog(LogLevel.Warn, warnMsg, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: product, systemId: systemId, ex: e);
					return BadRequestCertificateMismatched();
				}
				catch (Exception ex)
				{
					var errorMessage = "Exception trying to decrypt message";
					ErrorReporter.ReportOnce("Exception trying to decrypt message", ex);
					Logger.AddLog(LogLevel.Error, errorMessage, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: product, systemId: systemId, ex: ex);
					return BadRequest();
				}
				finally
				{
					dbPkLock.Dispose();
				}
			}

			return actionResult;
		}

		static void StoreLocalSecretKey(EdiTrustedSystem trustedSystem, byte[] remoteKey, byte[] myAccountKey)
		{
			var secretKey = myAccountKey.Concat(remoteKey).ToArray();
			var ecretKeyExpiryUtc = ZDateTime.UtcNow.AddMinutes(EDIDataRegistry.Instance.MyAccountKeyRotationIntervalMinutes.Value);

			using (var command = Db.Connection.Command("EdiUpdateSecretKey"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@SystemPk", SqlDbType.UniqueIdentifier, trustedSystem.PK.ToGuid());
				command.AddParameter("@SecretKey", SqlDbType.VarBinary, secretKey);
				command.AddParameter("@SecretKeyExpiry", SqlDbType.SmallDateTime, ecretKeyExpiryUtc.ToDateTime());
				command.ExecuteNonQuery();
			}
		}

		static string GetHeaderValueSafe(HttpRequestMessage request, string key)
		{
			request.Headers.TryGetValues(key, out var headerValues);
			return headerValues?.FirstOrDefault() ?? string.Empty;
		}

		BadRequestWithErrorMessages BadRequestCertificateMismatched()
		{
			return new BadRequestWithErrorMessages(new WTG.TrustedMessaging.Models.ErrorMessages(ErrorCodes.Codes.Critical_CertificateMismatched, ErrorCodes.Descriptions.Critical_CertificateMismatched), this);
		}

		class MessageWithSignatureResult : OkNegotiatedContentResult<string>
		{
			public MessageWithSignatureResult(string responseMessage, string signature, ApiController controller)
				: base(string.Empty, controller)
			{
				this.responseMessage = responseMessage;
				this.signature = signature;
			}
			readonly string responseMessage;
			readonly string signature;

			public override async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
			{
				var response = await base.ExecuteAsync(cancellationToken);
				response.Content = new StringContent(responseMessage);
				response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
				response.Headers.Add("SIGNED", signature);
				return response;
			}
		}
	}
}
