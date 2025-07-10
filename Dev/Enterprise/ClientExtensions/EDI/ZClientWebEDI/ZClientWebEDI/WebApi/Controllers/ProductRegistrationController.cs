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
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/ProductRegistration")]
	public class ProductRegistrationController : LicenceRegistrationTrustedSystemBaseController
	{
		public ProductRegistrationController() : base()
		{
		}

		public ProductRegistrationController(NLogWrapper logger) : base(logger)
		{
		}

		#region V1 Tenant Registration

		[Route("Register/{product}")]
		[HttpPost]
		public IHttpActionResult Register(string product)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = $"api/ProductRegistration/Register";
			var message = $"{routingPath} start".TrimEnd();
			AddInfoLog(message, ((int)HttpStatusCode.OK), sessionId: sessionId, routingPath: routingPath, product: product);

			try
			{
				var encryptedRequest = Request.Content.ReadAsStringAsync().Result;

				if (string.IsNullOrWhiteSpace(product) || string.IsNullOrWhiteSpace(encryptedRequest))
				{
					var errorMessages = new ErrorMessages(ErrorCodes.Codes.Validation_MissingRequiredField, StatusMessages.NoHttpContent);
					var errorLog = errorMessages.GetSingleLineMessages().TrimEnd();
					AddWarnLog(errorLog, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: product);
					return new BadRequestWithErrorMessages(errorMessages, this);
				}

				if (Request.Headers.TryGetValues(HttpHeaderSigned, out var values) && values != null && values.Any())
				{
					var encryptor = new ProductRegistrationMessageEncryptor(product);
					var decryptedRequest = encryptor.DecryptRegistrationRequest(encryptedRequest, values.First());
					var registration = JsonConvert.DeserializeObject<LicenceDatabaseRegistration>(decryptedRequest);

					if (registration == null || string.IsNullOrWhiteSpace(registration.Product) || string.IsNullOrWhiteSpace(registration.SystemId) || string.IsNullOrWhiteSpace(registration.LicenceType))
					{
						var errorMessages = new ErrorMessages(ErrorCodes.Codes.Validation_MissingRequiredField, StatusMessages.NoRegistrationContent);
						var errorLog = errorMessages.GetSingleLineMessages().TrimEnd();
						AddWarnLog(errorLog, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: product, systemId: registration.SystemId);
						return new BadRequestWithErrorMessages(errorMessages, this);
					}

					if (product != registration.Product)
					{
						var errorMessages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidValue, StatusMessages.ProductMismatch);
						var errorLog = errorMessages.GetSingleLineMessages().TrimEnd();
						AddWarnLog(errorLog, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: product, systemId: registration.SystemId);
						return new BadRequestWithErrorMessages(errorMessages, this);
					}

					var result = LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(registration);
					if (result.Success)
					{
						var response = new ProductRegistrationResponse() { DatabaseNumber = result.DatabaseNumber, MyAccountEndpointBaseUrl = EDIDataRegistry.Instance.MyAccountEndpointBaseUrl.Value };
						var (encryptedResponse, sig) = encryptor.EncryptRegistrationResponse(JsonConvert.SerializeObject(response));
						var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
						httpResponse.Content = new StringContent(encryptedResponse);
						httpResponse.Headers.Add(HttpHeaderSigned, sig);
						var successLog = $"success | {NLogWrapper.Status.Ok}";
						AddInfoLog(successLog, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath, product: product, systemId: registration.SystemId);
						return new ResponseMessageResult(httpResponse);
					}
					else
					{
						var errorMessages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidValue, result.OutputMessage);
						var errorLog = errorMessages.GetSingleLineMessages().TrimEnd();
						AddWarnLog(errorLog, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: product, systemId: registration.SystemId);
						return new BadRequestWithErrorMessages(errorMessages, this);
					}
				}
				else
				{
					var errorMessages = new ErrorMessages(ErrorCodes.Codes.Critical_MessageMalformed, StatusMessages.MessageNotSigned);
					var errorLog = errorMessages.GetSingleLineMessages().TrimEnd();
					AddWarnLog(errorLog, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: product);
					return new BadRequestWithErrorMessages(errorMessages, this);
				}
			}
			catch (Exception ex) when (ex is CryptographicException || ex is JsonException || ex is FormatException)
			{
				var error = new ErrorMessages(ErrorCodes.Codes.Critical_MessageMalformed, ex.Message);
				var errorLog = error.GetSingleLineMessages().TrimEnd();
				AddErrorLog(errorLog, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, product: product, ex: ex);
				return new BadRequestWithErrorMessages(error, this);
			}
		}

		[Route("AppendAdditionalInfo")]
		[HttpPost]
		public IHttpActionResult AppendAdditionalInfo([FromBody]TrustedRequest trustedRequest)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = $"api/ProductRegistration/AppendAdditionalInfo";
			TrustedMessagingLogger.LogTrustedRequest(Logger, routingPath, sessionId, trustedRequest);

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var result = new MyAccountTrustedMessenger<LicenceDatabaseRegistrationAdditionalInfo>().ReadMessage(Request, trustedRequest, factory);
			if (result.Info == null || result.ProcessStatus != MyAccountTrustedMessageProcessStatus.Successful)
			{
				TrustedMessagingLogger.LogTrustedMessageDecryptionFalure(Logger, routingPath, sessionId, trustedRequest, result.ErrorMessages);
				return new BadRequestWithErrorMessages(result.ErrorMessages, this);
			}
			else
			{
				TrustedMessagingLogger.LogTrustedMessageDecryptionSuccess(Logger, routingPath, sessionId, trustedRequest, result.Info);
			}

			if (string.IsNullOrEmpty(result.Info.OrgCountry) && string.IsNullOrEmpty(result.Info.EnterpriseCode) && string.IsNullOrEmpty(result.Info.ServerCode) && string.IsNullOrEmpty(result.Info.CargowiseCompanyCode))
			{
				var errorMessages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidValue, StatusMessages.NoOrgCountryOrLicenceInfo);
				TrustedMessagingLogger.LogBadRequestWithErrorMessage(Logger, routingPath, sessionId, trustedRequest, result.Info, errorMessages);
				return new BadRequestWithErrorMessages(errorMessages, this);
			}

			var database = result.TrustedSystem?.FindTenantDatabaseByTrustedInfo(result.Info);
			if (database != null)
			{
				var targetDatabase = TryDeduplicateDatabases(database, result.Info, out var destinationDatabase) ? destinationDatabase : database;
				targetDatabase.Notes.AddNew(true, EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationAdditionalInfoNote.Description, result.Info.ToString());
				factory.Save();
			}

			TrustedMessagingLogger.LogOkResponse(Logger, routingPath, sessionId, trustedRequest, result.Info);
			return Ok();
		}

		bool TryDeduplicateDatabases(LicenceDatabase sourceDatabase, LicenceDatabaseRegistrationAdditionalInfo additionalInfo, out LicenceDatabase destinationDatabase)
		{
			var result = false;
			destinationDatabase = null;

			if (new[] { additionalInfo.OrgName, additionalInfo.OrgCountry, additionalInfo.Address1, additionalInfo.Address2,
						additionalInfo.City, additionalInfo.Postcode, additionalInfo.State }
					.Any(x => !string.IsNullOrWhiteSpace(x)))
			{
				var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				orgSubQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
				if (!string.IsNullOrEmpty(additionalInfo.OrgName))
				{
					orgSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, additionalInfo.OrgName);
				}

				var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_IsActive, true);
				if (!string.IsNullOrEmpty(additionalInfo.OrgCountry))
				{
					orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_RN_NKCountryCode, additionalInfo.OrgCountry);
				}
				if (!string.IsNullOrEmpty(additionalInfo.Address1))
				{
					orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_Address1, additionalInfo.Address1);
				}
				if (!string.IsNullOrEmpty(additionalInfo.Address2))
				{
					orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_Address2, additionalInfo.Address2);
				}
				if (!string.IsNullOrEmpty(additionalInfo.City))
				{
					orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_City, additionalInfo.City);
				}
				if (!string.IsNullOrEmpty(additionalInfo.Postcode))
				{
					orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_PostCode, additionalInfo.Postcode);
				}
				if (!string.IsNullOrEmpty(additionalInfo.State))
				{
					orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_State, additionalInfo.State);
				}

				orgSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

				var deduplicationOrgSubQuery = new ZDBOnlySubQuery(typeof(DeduplicationOrganisation), MDMAdminPanelOrganisationViewSchema.PK);
				deduplicationOrgSubQuery.AddSubQuery(orgSubQuery, JoinCondition.And);

				var destinationDatabaseQuery = new ZDBOnlyQuery(typeof(LicenceDatabase));
				destinationDatabaseQuery.AddSubQuery(LicenceDatabaseSchema.LD_OH_WebAccessOrg, deduplicationOrgSubQuery, JoinCondition.And);
				destinationDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_Product, sourceDatabase.LD_Product);
				destinationDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_LicenceType, sourceDatabase.LD_LicenceType);
				destinationDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_TenantID, string.Empty);
				destinationDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_DatabaseNumber, SQLComparisonOperator.LessThan, sourceDatabase.LD_DatabaseNumber);
				destinationDatabaseQuery.MaximumRows = 10;

				var destinationDatabases = sourceDatabase.Factory.Load<LicenceDatabase>(destinationDatabaseQuery);

				if (destinationDatabases.Any())
				{
					var databasesAsString = string.Join(",", destinationDatabases.Select(x => x.LD_DatabaseNumber).Append(sourceDatabase.LD_DatabaseNumber).OrderBy(x => x));

					if (destinationDatabases.Length > 1)
					{
						SendNotificationEmail("Duplicate Licence Databases Found", $@"Potential duplicate databases: {databasesAsString}
Please review the additional info. and set the Master Org. / merge databases manually.

{additionalInfo}");
					}
					else if (destinationDatabases.Length == 1 && destinationDatabases.Single().PK != sourceDatabase.PK)
					{
						if (sourceDatabase.Factory.ExistsInDatabase(EdiBilledUsageSchema.Constants.TableName, new ZQuery(EdiBilledUsageSchema.BU9_LD, sourceDatabase.PK)))
						{
							SendNotificationEmail("Licence Databases Merge Failed", $@"Potential duplicate databases: {databasesAsString}
The database {sourceDatabase.LD_DatabaseNumber} has billing records already, merge failed.
Please review the additional info. and set the Master Org. / merge databases manually.

{additionalInfo}");
						}
						else
						{
							destinationDatabase = destinationDatabases.Single();
							destinationDatabase.LD_ETS_TrustedSystem = sourceDatabase.LD_ETS_TrustedSystem;
							destinationDatabase.LD_TenantID = sourceDatabase.LD_TenantID;
							sourceDatabase.LD_IsActive = false;
							sourceDatabase.LD_TenantID = string.Empty;
							sourceDatabase.LD_ETS_TrustedSystem = ZGuid.Empty;
							result = true;
						}
					}
				}
			}

			return result;
		}

		static void SendNotificationEmail(string subject, string message)
		{
			var regItem = EDIDataRegistry.Instance.ProductRegistrationWebAPINotificationGroup;
			var regFullName = string.Join(" > ", regItem.Categories) + " > " + regItem.Caption;
			var mail = new EmailDef()
			{
				Subject = subject,
				Body =
$@"{message}

You are receiving this email because you are a member of the group in registry {regFullName}",
			};
			try
			{
				Env.OutgoingMailManager.CreateAndSave(mail, regItem.Value, GroupSourceLocator.GetFromRegistryItem(regItem));
			}
			catch (EmailSendFailedException)
			{
			}
		}

		#endregion V1 Tenant Registration

		#region V2 System and Tenant Registraion

		[Route("system")]
		[HttpPost]
		public IHttpActionResult RegisterTrustedSystem([FromBody] TrustedSystemRegistrationInfo registrationInfo)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = $"api/ProductRegistration/system";
			var message = $"{routingPath} start".TrimEnd();
			AddInfoLog(message, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath, product: registrationInfo.Product, systemId: registrationInfo.SystemId);

			if (string.IsNullOrWhiteSpace(registrationInfo.Product) || string.IsNullOrWhiteSpace(registrationInfo.SystemId) || string.IsNullOrWhiteSpace(registrationInfo.RegistrationKey))
			{
				var errors = new ErrorMessages(ErrorCodes.Codes.Validation_MissingRequiredField, ErrorCodes.Descriptions.Validation_MissingRequiredField);
				var errorLog = errors.GetSingleLineMessages().TrimEnd();
				AddWarnLog(errorLog, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: registrationInfo.Product, systemId: registrationInfo.SystemId);
				return new BadRequestWithErrorMessages(errors, this);
			}

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };

			var registrationCertQuery = new ZQuery(EdiTrustedMessagingConfigSchema.ETM_Product, registrationInfo.Product);
			registrationCertQuery.AddToFilter(EdiTrustedMessagingConfigSchema.ETM_CertificateType, CertificateTypeList.Codes.PreDeploymentCertificate);
			var registrationCert = factory.LoadTop1<EdiTrustedMessagingConfig>(registrationCertQuery)?.GetCertificate();
			if (registrationCert == null)
			{
				AddErrorLog(StatusMessages.RegistrationCertNotAvailable, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, product: registrationInfo.Product, systemId: registrationInfo.SystemId);
				return InternalServerError(StatusMessages.RegistrationCertNotAvailable);
			}

			byte[] trustedSystemPublicKey = null;
			try
			{
				trustedSystemPublicKey = registrationInfo.DecryptRegistrationKey(registrationCert);
			}
			catch (CryptographicException)
			{
			}
			catch (FormatException)
			{
			}

			if (trustedSystemPublicKey == null || trustedSystemPublicKey.Length == 0)
			{
				var errors = new ErrorMessages(ErrorCodes.Codes.Validation_MissingRequiredField, ErrorCodes.Descriptions.Validation_MissingRequiredField);
				var errorLog = errors.GetSingleLineMessages().TrimEnd();
				AddWarnLog(errorLog, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: registrationInfo.Product, systemId: registrationInfo.SystemId);
				return new BadRequestWithErrorMessages(errors, this);
			}

			var soapTemplate = EDIDataRegistry.Instance.MyAccountCertificateAuthoritySoapRequestTemplate.Value;
			var userAccount = EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccount.Value;
			var userAccountPassword = EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccountPassword.Value;

			if (string.IsNullOrWhiteSpace(soapTemplate) || string.IsNullOrWhiteSpace(userAccount) || string.IsNullOrWhiteSpace(userAccountPassword))
			{
				AddErrorLog(StatusMessages.CertificateAuthorityNotAvailable, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, product: registrationInfo.Product, systemId: registrationInfo.SystemId);
				return InternalServerError(StatusMessages.CertificateAuthorityNotAvailable);
			}

			var centralSystemCertQuery = new ZQuery(EdiTrustedMessagingConfigSchema.ETM_Product, registrationInfo.Product);
			centralSystemCertQuery.AddToFilter(EdiTrustedMessagingConfigSchema.ETM_CertificateType, CertificateTypeList.Codes.CentralSystemCertificate);
			var centralSystemCert = factory.LoadTop1<EdiTrustedMessagingConfig>(centralSystemCertQuery)?.GetCertificate();
			if (centralSystemCert == null)
			{
				AddErrorLog(StatusMessages.CentralSystemCertNotAvailable, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, product: registrationInfo.Product, systemId: registrationInfo.SystemId);
				return InternalServerError(StatusMessages.CentralSystemCertNotAvailable);
			}

			var trustedSystem = EdiTrustedSystem.Load(factory, registrationInfo.Product, registrationInfo.SystemId);
			if (trustedSystem == null)
			{
				trustedSystem = factory.New<EdiTrustedSystem>();
				trustedSystem.ETS_Product = registrationInfo.Product;
				trustedSystem.ETS_SystemID = registrationInfo.SystemId;
				trustedSystem.GetOrCreateCertificateConfig();

				try
				{
					factory.Save();
				}
				catch (ZSaveException ex)
				{
					AddErrorLog(StatusMessages.AnotherRegistrationInProgress, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, product: registrationInfo.Product, systemId: registrationInfo.SystemId, ex: ex);
					return InternalServerError(StatusMessages.AnotherRegistrationInProgress);
				}
			}
			else if (trustedSystem.ETS_ETM_Certificate.IsEmpty)
			{
				trustedSystem.GetOrCreateCertificateConfig();
				try
				{
					factory.Save();
				}
				catch (ZSaveException ex)
				{
					AddErrorLog(StatusMessages.AnotherRegistrationInProgress, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, product: registrationInfo.Product, systemId: registrationInfo.SystemId, ex: ex);
					return InternalServerError(StatusMessages.AnotherRegistrationInProgress);
				}
			}

			if (trustedSystem.CertificateConfig != null && trustedSystem.CertificateConfig.GetCertificate() != null)
			{
				AddWarnLog(StatusMessages.TrustedSystemAlreadyRegistered, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath, product: registrationInfo.Product, systemId: registrationInfo.SystemId);
				return BadRequest(StatusMessages.TrustedSystemAlreadyRegistered);
			}

			var subjectName = FormattableString.Invariant($"Product_SystemId:{registrationInfo.Product}_{registrationInfo.SystemId}");
			var requestContext = new EdiCertRequestContext { SystemIdentifier = FormattableString.Invariant($"{registrationInfo.Product}_{registrationInfo.SystemId}") };

			if (GetCertRequest().TrySubmitSafe(soapTemplate, subjectName, trustedSystemPublicKey,
				new NetworkCredential(userAccount, userAccountPassword), requestContext, out var output))
			{
				trustedSystem.CertificateConfig.ETM_CertificateData = Convert.FromBase64String(output);
				var trustedSystemCert = trustedSystem.CertificateConfig.GetCertificate();
				trustedSystem.Logs.AddNew(Events.CertificateReceived, new KeyValuePair<string, string>(nameof(trustedSystemCert.Thumbprint), trustedSystemCert.Thumbprint));
				factory.Save();

				var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
				var rsp = new TrustedSystemRegistrationResponse();
				rsp.Import(centralSystemCert, trustedSystemCert, trustedSystemCert.GetRSAPublicKey());
				httpResponse.Content = new StringContent(JsonConvert.SerializeObject(rsp), Encoding.UTF8, WTG.TrustedMessaging.Constants.HttpMediaTypes.Json);
				AddInfoLog("success", ((int)HttpStatusCode.OK), sessionId ,routingPath: routingPath, product: registrationInfo.Product, systemId: registrationInfo.SystemId);
				return new ResponseMessageResult(httpResponse);
			}
			else
			{
				AddErrorLog(StatusMessages.CertificateAuthorityNotAvailable, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, product: registrationInfo.Product, systemId: registrationInfo.SystemId);
				return InternalServerError(StatusMessages.CertificateAuthorityNotAvailable);
			}
		}

		protected virtual IEDICertRequest GetCertRequest() => new EDICertRequest();

		new InternalErrorWithErrorMessages InternalServerError(string message) =>
			new InternalErrorWithErrorMessages(new ErrorMessages(ErrorCodes.Codes.Server_Error, message), this);

		[Route("tenant")]
		[HttpPost]
		public IHttpActionResult RegisterTenant([FromBody]TrustedRequest trustedRequest)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var context = TrustedContextV1<TenantRegistrationInfo, ProductRegistrationResponse>.New(trustedRequest, this);
				RegisterTenantCore(context);
				return context.CreateHttpActionResult();
			}
		}

		#endregion

		const string HttpHeaderSigned = "SIGNED";
	}
}
