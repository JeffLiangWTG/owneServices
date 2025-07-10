using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityApplicationPermission.Business;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using WTG.AWSCertificateIntegration;
using WTG.IdentitySecurity;
using WTG.OpenIDConnect.Token;
using WTG.TrustedMessaging.MyAccount.Models;
using WTG.TrustedMessaging.MyAccount.Models.DistributedData;
using Certificate = WTG.TrustedMessaging.MyAccount.Models.Certificate;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/SystemTrust")]
	public class SystemToSystemTrustController : TrustedController
	{
		readonly string badRequestCode = ((int)HttpStatusCode.BadRequest).ToString();

		public SystemToSystemTrustController()
		{
		}

		internal SystemToSystemTrustController(NLogWrapper logger) : base(logger)
		{
		}

		[HttpPost]
		[Route("certificate/request")]
		public IHttpActionResult RequestCertificate()
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = Request.RequestUri.AbsolutePath;

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					var bodyContent = Request.Content.ReadAsStringAsync().Result;

					if (string.IsNullOrWhiteSpace(bodyContent))
					{
						var warn = $"{NLogWrapper.Status.BadRequest}: {ErrorCodes.Descriptions.Critical_MessageMalformed}";
						AddWarnLog(warn, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
						return BadRequest(ErrorCodes.Codes.Critical_MessageMalformed, ErrorCodes.Descriptions.Critical_MessageMalformed);
					}

					return RequestCertificate(sessionId, bodyContent, routingPath);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var error = $"{NLogWrapper.Status.InternalError}";
					AddErrorLog(error, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, ex: ex);
					return InternalServerError(StatusMessages.ServiceInternalError);
				}
			}
		}

		[HttpPost]
		[Route("certificate/register")]
		public IHttpActionResult RegisterCertificate([FromBody] IdentityCertificateRegisterRequest identityCertificateRegisterRequest)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = Request.RequestUri.AbsolutePath;

			if (!ValidateRequestBearerToken(Request, routingPath, sessionId, out var actionResult, out var token))
			{
				return actionResult;
			}

			if (identityCertificateRegisterRequest == null
				|| string.IsNullOrEmpty(identityCertificateRegisterRequest.CertificateSignRequest)
				|| string.IsNullOrEmpty(identityCertificateRegisterRequest.Module)
				|| string.IsNullOrEmpty(identityCertificateRegisterRequest.ApplicationDescription)
				|| string.IsNullOrEmpty(identityCertificateRegisterRequest.CARoot))
			{
				AddWarnLog($"{ErrorCodes.Descriptions.Validation_MissingRequiredField}", ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
				return BadRequest(ErrorCodes.Codes.Validation_MissingRequiredField, ErrorCodes.Descriptions.Validation_MissingRequiredField);
			}

			if (!EDIDataRegistry.Instance.AWSPrivateCAListManager.Value.Cast<AWSPrivateCA>().Any(x => x.IssuingCA == identityCertificateRegisterRequest.CARoot && x.IsEnabled))
			{
				AddErrorLog($"{StatusMessages.InvalidCertificateRegisterRequestData}", ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
				return BadRequest(badRequestCode, StatusMessages.InvalidCertificateRegisterRequestData);
			}

			if (!AwsPcaManager.IsCertificateSignRequestValid(identityCertificateRegisterRequest.CertificateSignRequest))
			{
				AddWarnLog($"{StatusMessages.InvalidCertificateSignRequest}", ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
				return BadRequest(badRequestCode, StatusMessages.InvalidCertificateSignRequest);
			}

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					var application = QueryApplicationFromToken(sessionId, routingPath, token);

					if (application == null)
					{
						return BadRequest(badRequestCode, StatusMessages.InvalidAccessToken);
					}

					var database = application.LicenceDatabase;
					if (database == null || !ProductTypes.IsEnterpriseFamily(database.LD_Product))
					{
						AddErrorLog($"Invalid licence info {application.IDA_ClientID}", (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
						return BadRequest(badRequestCode, StatusMessages.InvalidAccessToken);
					}

					var applicationName = $"{database.EnterpriseCode}.{database.LD_ServerCode}.{identityCertificateRegisterRequest.Module}.{identityCertificateRegisterRequest.ApplicationDescription}";

					var applicationExistedQuery = new ZQuery(EdiIdentityApplicationSchema.IDA_ApplicationName, applicationName);
					var applicationToRegister = Factory.LoadTop1<EdiIdentityApplication>(applicationExistedQuery);

					string operationId;
					if (applicationToRegister == null)
					{
						applicationToRegister = Factory.New<EdiIdentityApplication>();
						applicationToRegister.IDA_ApplicationName = applicationName;
						applicationToRegister.IDA_ApplicationModule = identityCertificateRegisterRequest.Module;
						applicationToRegister.IDA_IDA_ParentApplication = application.PK;

						var permission = applicationToRegister.Permissions.AddNew();
						permission.IAP_Scope = "*";

						operationId = AddCertificateAndReturnPk(applicationToRegister, identityCertificateRegisterRequest.CertificateSignRequest, identityCertificateRegisterRequest.CARoot);
					}
					else
					{
						var existingCertificate = applicationToRegister.Certificates.FirstOrDefault(x => x.ICE_CertificateData.IsEmpty && x.ICE_CertificateSigningRequest == identityCertificateRegisterRequest.CertificateSignRequest);
						if (existingCertificate != null)
						{
							operationId = existingCertificate.PK.ToString();
						}
						else
						{
							operationId = AddCertificateAndReturnPk(applicationToRegister, identityCertificateRegisterRequest.CertificateSignRequest, identityCertificateRegisterRequest.CARoot);
						}
					}
					Factory.Save();

					var infoMsg = $"{application.IDA_ClientID} | {identityCertificateRegisterRequest.Module} | {identityCertificateRegisterRequest.ApplicationDescription}";
					AddInfoLog(infoMsg, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
					var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
					httpResponse.Content = new StringContent(operationId);
					return new ResponseMessageResult(httpResponse);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					AddErrorLog("error", ((int)HttpStatusCode.InternalServerError), sessionId: sessionId, routingPath: routingPath, ex: ex);
					return InternalServerError(StatusMessages.ErrorSavingRecord);
				}
			}
		}

		[HttpGet]
		[Route("application/{clientId}")]
		public IHttpActionResult CheckIfApplicationExists([FromUri] string clientId)
		{
			var routingPath = Request.RequestUri.AbsolutePath;

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					var applicationExists = CheckIfApplicationExistsInDb(clientId);
					if (!applicationExists)
					{
						return NotFound();
					}

					return Ok();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var error = $"{NLogWrapper.Status.InternalError}";
					AddErrorLog(error, ((int)HttpStatusCode.InternalServerError), routingPath: routingPath, ex: ex);
					return InternalServerError(StatusMessages.ErrorQueryRecord);
				}
			}
		}

		[HttpPost]
		[Route("certificate/rollover")]
		public IHttpActionResult RolloverCertificate([FromBody] IdentityCertificateRolloverRequest rolloverRequest)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = Request.RequestUri.AbsolutePath;

			if (!ValidateRequestBearerToken(Request, routingPath, sessionId, out var actionResult, out var securityToken))
			{
				return actionResult;
			}

			if (string.IsNullOrEmpty(rolloverRequest.ClientId) || string.IsNullOrEmpty(rolloverRequest.CertificateSignRequest))
			{
				var warn = $"{NLogWrapper.Status.BadRequest}: {ErrorCodes.Descriptions.Validation_MissingRequiredField}";
				AddWarnLog(warn, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
				return BadRequest(ErrorCodes.Codes.Validation_MissingRequiredField, ErrorCodes.Descriptions.Validation_MissingRequiredField);
			}

			if (!AwsPcaManager.IsCertificateSignRequestValid(rolloverRequest.CertificateSignRequest))
			{
				var warn = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.InvalidCertificateSignRequest}";
				AddWarnLog(warn, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
				return BadRequest(badRequestCode, StatusMessages.InvalidCertificateSignRequest);
			}

			if (!Guid.TryParse(rolloverRequest.ClientId, out _))
			{
				var warn = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.InvalidClientId}";
				AddWarnLog(warn, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
				return BadRequest(badRequestCode, StatusMessages.InvalidClientId);
			}

			if (!string.IsNullOrEmpty(rolloverRequest.CaRootType) && !EDIDataRegistry.Instance.AWSPrivateCAListManager.Value.Cast<AWSPrivateCA>().Any(x => x.IssuingCA == rolloverRequest.CaRootType && x.IsEnabled))
			{
				var warn = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.InvalidCaRoot}";
				AddWarnLog(warn, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
				return BadRequest(badRequestCode, StatusMessages.InvalidCaRoot);
			}

			using (Db.DisposableActionForDbConnection())
			{
				var s2stApplication = QueryApplicationFromToken(sessionId, routingPath, securityToken);
				if (s2stApplication == null)
				{
					return BadRequest(badRequestCode, StatusMessages.InvalidAccessToken);
				}

				var operationId = string.Empty;
				EdiIdentityApplication application = null;
				if (rolloverRequest.ClientId == s2stApplication.IDA_ClientID)
				{
					application = s2stApplication;
				}
				else
				{
					application = QueryApplicationFromTenantIdAndClientId(s2stApplication.TenantID, rolloverRequest.ClientId);

					if (application == null)
					{
						var message = $"The application with client id '{rolloverRequest.ClientId}' isn't active or doesn't exist.";
						var warn = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.InvalidClientId} - {message}";
						AddWarnLog(warn, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
						return BadRequest(badRequestCode, $"{StatusMessages.InvalidClientId}: {message}");
					}

					if (application.IDA_IDA_ParentApplication != s2stApplication.PK)
					{
						var message = $"The roll overed application '{rolloverRequest.ClientId}' does not belong to application {s2stApplication.IDA_ClientID}.";
						var warn = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.InvalidClientId} - {message}";
						AddWarnLog(warn, (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
						return BadRequest(badRequestCode, $"{StatusMessages.InvalidClientId}: {message}");
					}
				}

				var certificateQuery = new ZQuery(EdiIdentityCertificateSchema.ICE_IDA, application.PK);
				certificateQuery.AddToFilter(EdiIdentityCertificateSchema.ICE_CertificateData, null);
				certificateQuery.AddToFilter(EdiIdentityCertificateSchema.ICE_CertificateSigningRequest, rolloverRequest.CertificateSignRequest);

				var certificate = Factory.LoadTop1<EdiIdentityCertificate>(certificateQuery);
				if (certificate == null)
				{
					try
					{
						operationId = AddCertificateAndReturnPk(application, rolloverRequest.CertificateSignRequest, rolloverRequest.CaRootType);
						Factory.Save();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						var error = $"{NLogWrapper.Status.InternalError}";
						AddErrorLog(error, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, ex: ex);
						return InternalServerError(StatusMessages.ErrorSavingRecord);
					}
				}

				var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
				httpResponse.Content = new StringContent(!operationId.IsNullOrEmpty() ? operationId : certificate.PK.ToString());
				var info = $"{rolloverRequest.ClientId} | {NLogWrapper.Status.Ok}";
				AddInfoLog(info, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
				return new ResponseMessageResult(httpResponse);
			}
		}

		protected virtual IHttpActionResult RequestCertificate(string session, string queryString, string routingPath)
		{
			var certificateRequestInfo = JsonConvert.DeserializeObject<IdentityCertificateInitialRequest>(new SecureQueryString(queryString)[nameof(IdentityCertificateInitialRequest)]);
			if (certificateRequestInfo == null
				|| string.IsNullOrEmpty(certificateRequestInfo.DatabaseNumber)
				|| !int.TryParse(certificateRequestInfo.DatabaseNumber, out var dbNumberAsInt)
				|| dbNumberAsInt <= 0
				|| string.IsNullOrEmpty(certificateRequestInfo.Password)
				|| string.IsNullOrEmpty(certificateRequestInfo.CertificateSignRequest))
			{
				var warn = $"{NLogWrapper.Status.BadRequest}: {ErrorCodes.Descriptions.Validation_MissingRequiredField}";
				AddWarnLog(warn, ((int)HttpStatusCode.BadRequest), session, routingPath: routingPath);
				return BadRequest(ErrorCodes.Codes.Validation_MissingRequiredField, ErrorCodes.Descriptions.Validation_MissingRequiredField);
			}

			var database = Factory.LoadTop1<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, dbNumberAsInt));
			if (database == null || database.LicEnterprise == null || !ProductTypes.IsEnterpriseFamily(database.LD_Product))
			{
				var warn = $"{dbNumberAsInt} | {NLogWrapper.Status.BadRequest}: {ErrorCodes.Descriptions.Validation_InvalidSystem}";
				AddWarnLog(warn, ((int)HttpStatusCode.BadRequest), session, routingPath: routingPath);
				return BadRequest(ErrorCodes.Codes.Validation_InvalidSystem, ErrorCodes.Descriptions.Validation_InvalidSystem);
			}

			if (!database.LD_IsActive)
			{
				var error = $"{dbNumberAsInt} | {NLogWrapper.Status.BadRequest}: {StatusMessages.DatabaseNotActive}";
				AddErrorLog(error, ((int)HttpStatusCode.BadRequest), session, routingPath: routingPath);
				return BadRequest(badRequestCode, StatusMessages.DatabaseNotActive);
			}

			if (!new[] { DatabaseStatusList.Codes.REG, DatabaseStatusList.Codes.Preregistered }.Any(x => database.LD_Status.EqualsIgnoringCase(x)))
			{
				var error = $"{dbNumberAsInt} | {NLogWrapper.Status.BadRequest}: {StatusMessages.DatabaseNotRegistered}";
				AddErrorLog(error, ((int)HttpStatusCode.BadRequest), session, routingPath: routingPath);
				return BadRequest(badRequestCode, StatusMessages.DatabaseNotRegistered);
			}

			if (!database.ValidateClientSecret(certificateRequestInfo.Password))
			{
				var error = $"{dbNumberAsInt} | {NLogWrapper.Status.BadRequest}: {StatusMessages.PasswordMismatch}";
				AddErrorLog(error, ((int)HttpStatusCode.BadRequest), session, routingPath: routingPath);
				return BadRequest(badRequestCode, StatusMessages.PasswordMismatch);
			}

			if (!AwsPcaManager.IsCertificateSignRequestValid(certificateRequestInfo.CertificateSignRequest))
			{
				var warn = $"{dbNumberAsInt} | {NLogWrapper.Status.BadRequest}: {StatusMessages.InvalidCertificateSignRequest}";
				AddWarnLog(warn, ((int)HttpStatusCode.BadRequest), session, routingPath: routingPath);
				return BadRequest(badRequestCode, StatusMessages.InvalidCertificateSignRequest);
			}

			var dbOnlyQuery = new ZDBOnlyQuery(typeof(EdiIdentityApplication));
			dbOnlyQuery.AddToFilter(EdiIdentityApplicationSchema.IDA_LD, database.PK);
			var subQuery = new ZDBOnlySubQuery(typeof(EdiIdentityTenant), EdiIdentityApplicationSchema.IDA_IDT);
			subQuery.AddToFilter(EdiIdentityTenantSchema.IDT_TenantId, EDIDataRegistry.Instance.AzureApplicationManagementTenantID.Value);
			dbOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);

			var application = Factory.LoadTop1<EdiIdentityApplication>(dbOnlyQuery);

			if (application != null && !application.IDA_IsActive)
			{
				var warn = $"{dbNumberAsInt} | {NLogWrapper.Status.BadRequest}: {StatusMessages.ApplicationExistsAndNotActive}";
				AddWarnLog(warn, ((int)HttpStatusCode.BadRequest), session, routingPath: routingPath);
				return BadRequest(badRequestCode, StatusMessages.ApplicationExistsAndNotActive);
			}

			string operationId;

			try
			{
				if (application == null)
				{
					application = Factory.New<EdiIdentityApplication>();
					application.IDA_LD = database.PK;
					application.IDA_ApplicationName = $"{database.EnterpriseID}.{database.LD_Product}.{database.DatabaseId}";

					var permission = application.Permissions.AddNew();
					permission.IAP_Scope = "*";

					operationId = AddCertificateAndReturnPk(application, certificateRequestInfo.CertificateSignRequest);
				}
				else
				{
					var existingCertificate = application.Certificates.FirstOrDefault(x => x.ICE_CertificateData.IsEmpty && x.ICE_CertificateSigningRequest == certificateRequestInfo.CertificateSignRequest);
					if (existingCertificate != null)
					{
						operationId = existingCertificate.PK.ToString();
					}
					else
					{
						operationId = AddCertificateAndReturnPk(application, certificateRequestInfo.CertificateSignRequest);
					}
				}
				Factory.Save();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var error = $"{NLogWrapper.Status.InternalError}";
				AddErrorLog("error", ((int)HttpStatusCode.InternalServerError), session, routingPath: routingPath, ex: ex);
				return InternalServerError(StatusMessages.ErrorSavingRecord);
			}

			var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
			httpResponse.Content = new StringContent(operationId);
			var info = $"{dbNumberAsInt} | {NLogWrapper.Status.Ok}";
			AddInfoLog(info, ((int)HttpStatusCode.OK), session, routingPath: routingPath);
			return new ResponseMessageResult(httpResponse);
		}

		string AddCertificateAndReturnPk(EdiIdentityApplication application, string csr, string caRootType = null)
		{
			var certificate = application.Certificates.AddNew();
			certificate.ICE_CertificateSigningRequest = csr;
			certificate.ICE_CARoot = caRootType.IsNullOrEmpty() ? CARootCodeDescriptionList.Codes.SystemToSystemTrust : caRootType;
			return certificate.PK.ToString();
		}

		[HttpGet]
		[Route("certificate/{operationId}")]
		public IHttpActionResult DownloadCertificate([FromUri] string operationId)
		{
			var routingPath = Request.RequestUri.AbsolutePath;

			EdiIdentityCertificate certificate = null;

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					bool isOperationIdInvalid;
					if (!Guid.TryParse(operationId, out var certificateRequestId))
					{
						isOperationIdInvalid = true;
					}
					else
					{
						var factory = new BusinessObjectFactory();
						certificate = factory.Load<EdiIdentityCertificate>(certificateRequestId);
						isOperationIdInvalid = certificate == null || !certificate.ICE_IsActive;
					}

					if (isOperationIdInvalid)
					{
						var error = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.InvalidOperationId}";
						AddErrorLog(error, ((int)HttpStatusCode.BadRequest), routingPath: routingPath);
						return BadRequest(StatusMessages.InvalidOperationId);
					}

					var certificateResponse = BuildIdentityCertificateResponse(certificate);

					return Json(certificateResponse);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var error = $"{NLogWrapper.Status.InternalError}";
					AddErrorLog(error, ((int)HttpStatusCode.InternalServerError), routingPath: routingPath, ex: ex);
					return InternalServerError(StatusMessages.ErrorQueryRecord);
				}
			}
		}

		[HttpGet]
		[Route("certificates/{clientId}")]
		public IHttpActionResult DownloadCertificates([FromUri] string clientId)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = Request.RequestUri.AbsolutePath;

			if (!ValidateRequestBearerToken(Request, routingPath, sessionId, out var actionResult, out var securityToken))
			{
				return actionResult;
			}

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					var s2stApplication = QueryApplicationFromToken(sessionId, routingPath, securityToken);
					if (s2stApplication == null)
					{
						return BadRequest(badRequestCode, StatusMessages.InvalidAccessToken);
					}

					EdiIdentityApplication application = null;
					if (clientId == s2stApplication.IDA_ClientID)
					{
						application = s2stApplication;
					}
					else
					{
						application = QueryApplicationFromTenantIdAndClientId(s2stApplication.TenantID, clientId);

						if (application == null)
						{
							var message = $"The application with client id '{clientId}' isn't active or doesn't exist.";
							var warn = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.InvalidClientId} - {message}";
							AddWarnLog(warn, (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
							return BadRequest(badRequestCode, $"{StatusMessages.InvalidClientId}: {message}");
						}
					}

					var certificates = application.Certificates.Where(x => x.ICE_IsActive && x.ICE_CertificateData != ZBlob.Empty && x.ICE_ProcessingStatus == EdiIdentityCertificateProcessingStatus.Codes.COM);

					var certificateResponse = new IdentityCertificateResponse
					{
						ClientId = clientId,
						TenantId = EDIDataRegistry.Instance.AzureApplicationManagementTenantID.Value,
						Status = new EdiIdentityCertificateProcessingStatus().GetDescriptionFromCode(EdiIdentityCertificateProcessingStatus.Codes.COM),
						StatusCode = EdiIdentityCertificateProcessingStatus.Codes.COM
					};

					var newCertificateDataList = new List<IdentityCertificateData>();

					foreach (var certificate in certificates)
					{
						var identityCertificateData = new IdentityCertificateData
						{
							CertificateData = certificate.ICE_CertificateData,
						};
						newCertificateDataList.Add(identityCertificateData);
					}

					certificateResponse.CertificateDataArray = newCertificateDataList.ToArray();

					return Json(certificateResponse);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					AddErrorLog(NLogWrapper.Status.InternalError, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, ex: ex);
					return InternalServerError(StatusMessages.ErrorQueryRecord);
				}
			}
		}

		#region Sync data to DDC

		[HttpGet]
		[Route("sync/certificates")]
		public IHttpActionResult SyncAllCertificates(long seqNum, int maxRows)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = Request.RequestUri.AbsolutePath;

			if (!ValidateRequestBearerToken(Request, routingPath, sessionId, out var actionResult, out _))
			{
				return actionResult;
			}

			var returnedRows = Math.Min(maxRows, EDIDataRegistry.Instance.MaxRowsForDistributedDataSyncRequest.Value);

			var certificateQuery = new ZDBOnlyQuery(typeof(EdiIdentityCertificate));
			certificateQuery.AddToFilter(EdiIdentityCertificateSchema.ICE_SequenceNumber, SQLComparisonOperator.GreaterThan, seqNum);
			certificateQuery.AddToFilter(EdiIdentityCertificateSchema.ICE_ProcessingStatus, new[] { EdiIdentityCertificateProcessingStatus.Codes.COM, EdiIdentityCertificateProcessingStatus.Codes.CAN });
			certificateQuery.OrderBy = EdiIdentityCertificateSchema.Constants.ICE_SequenceNumber + OrderByClause.Ascending;
			certificateQuery.MaximumRows = returnedRows;

			using (Db.DisposableActionForDbConnection())
			{
				var ediCertificates = Factory.Load<EdiIdentityCertificate>(certificateQuery);

				var certificateSyncResponse = new CertificatesSyncResponse();

				if (ediCertificates.Any())
				{
					certificateSyncResponse.LastSequenceNumber = ediCertificates.Max(cert => cert.ICE_SequenceNumber);
				}
				else
				{
					certificateSyncResponse.LastSequenceNumber = seqNum;
				}

				certificateSyncResponse.SyncEnded = ediCertificates.Length < returnedRows;

				foreach (var ediCertificate in ediCertificates)
				{
					if (!ediCertificate.ICE_IsActive)
					{
						certificateSyncResponse.DeletedCertificates.Add(ediCertificate.PK.ToGuid());
					}
					else
					{
						using (var x509 = new X509Certificate2(ediCertificate.ICE_CertificateData))
						{
							var pem = x509.ToPem();

							var certificate = new Certificate()
							{
								PK = ediCertificate.PK.ToGuid(),
								CertificatePem = pem,
								Category = GetCertificateCategory(ediCertificate)
							};

							if (Guid.TryParse(ediCertificate.Application.IDA_ClientID, out var clientId))
							{
								certificate.ClientId = clientId;
							}

							certificateSyncResponse.CreatedCertificates.Add(certificate);
						}
					}
				}

				return Json(certificateSyncResponse);
			}
		}

		[HttpGet]
		[Route("sync/applicationPermissions")]
		public IHttpActionResult SyncApplicationPermissions(Guid lastSyncPK, int maxRows)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = Request.RequestUri.AbsolutePath;

			if (!ValidateRequestBearerToken(Request, routingPath, sessionId, out var actionResult, out _))
			{
				return actionResult;
			}

			var returnedRows = Math.Min(maxRows, EDIDataRegistry.Instance.MaxRowsForDistributedDataSyncRequest.Value);

			var query = new ZDBOnlyQuery(typeof(EdiIdentityApplicationPermission));
			query.AddToFilter(EdiIdentityApplicationPermissionSchema.PK, SQLComparisonOperator.GreaterThan, lastSyncPK);
			query.OrderBy = EdiIdentityApplicationPermissionSchema.Constants.PK;
			query.MaximumRows = returnedRows;

			using (Db.DisposableActionForDbConnection())
			{
				var applicationPermissions = Factory.Load<EdiIdentityApplicationPermission>(query);

				var applicationPermissionsSyncResponse = new DistributedDataSyncResponse<ApplicationPermissionModel> { SyncEnded = applicationPermissions.Length < returnedRows };

				foreach (var applicationPermission in applicationPermissions)
				{
					var applicationPermissionModel = new ApplicationPermissionModel
					{
						PK = applicationPermission.PK.ToGuid(),
						IdentityApplicationPK = applicationPermission.IAP_IDA.ToGuid(),
						Scope = applicationPermission.IAP_Scope,
					};

					applicationPermissionsSyncResponse.Items.Add(applicationPermissionModel);
				}

				return Json(applicationPermissionsSyncResponse);
			}
		}

		[HttpGet]
		[Route("sync/licenceDatabases")]
		public IHttpActionResult SyncLicenceDatabases(Guid lastSyncPK, int maxRows)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = Request.RequestUri.AbsolutePath;

			if (!ValidateRequestBearerToken(Request, routingPath, sessionId, out var actionResult, out _))
			{
				return actionResult;
			}

			var returnedRows = Math.Min(maxRows, EDIDataRegistry.Instance.MaxRowsForDistributedDataSyncRequest.Value);

			var query = new ZDBOnlyQuery(typeof(LicenceDatabase));
			query.AddToFilter(LicenceDatabaseSchema.PK, SQLComparisonOperator.GreaterThan, lastSyncPK);
			query.OrderBy = LicenceDatabaseSchema.Constants.PK;
			query.MaximumRows = returnedRows;

			using (Db.DisposableActionForDbConnection())
			{
				var licenceDatabases = Factory.Load<LicenceDatabase>(query);

				var licenceDatabasesSyncResponse = new DistributedDataSyncResponse<LicenceDatabaseModel> { SyncEnded = licenceDatabases.Length < returnedRows };

				foreach (var licenceDatabase in licenceDatabases)
				{
					var licenceDatabaseModel = new LicenceDatabaseModel
					{
						PK = licenceDatabase.PK.ToGuid(),
						LicenseEnterprisePK = licenceDatabase.LD_LE.ToGuid(),
						MasterOrgHeaderPK = licenceDatabase.LD_OH_WebAccessOrg.IsValid ? licenceDatabase.LD_OH_WebAccessOrg.ToGuid() : null,
						Product = licenceDatabase.LD_Product,
						DatabaseNumber = licenceDatabase.LD_DatabaseNumber,
						ServerCode = licenceDatabase.LD_ServerCode,
						LicenceType = licenceDatabase.LD_LicenceType,
						ReleaseRing = licenceDatabase.LD_ReleaseRing,
						HostedLocation = licenceDatabase.LD_HostedLocation
					};

					licenceDatabasesSyncResponse.Items.Add(licenceDatabaseModel);
				}

				return Json(licenceDatabasesSyncResponse);
			}
		}

		[HttpGet]
		[Route("sync/applications")]
		public IHttpActionResult GetSyncApplications(int maxRows, Guid lastSyncPK)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = Request.RequestUri.AbsolutePath;

			if (!ValidateRequestBearerToken(Request, routingPath, sessionId, out var actionResult, out var token))
			{
				return actionResult;
			}

			var returnedRows = Math.Min(maxRows, EDIDataRegistry.Instance.MaxRowsForDistributedDataSyncRequest.Value);

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					var query = new ZDBOnlyQuery(typeof(EdiIdentityApplication));
					query.AddToFilter(EdiIdentityApplicationSchema.PK, SQLComparisonOperator.GreaterThan, lastSyncPK);
					query.OrderBy = EdiIdentityApplicationSchema.Constants.PK;
					query.MaximumRows = returnedRows;

					var applications = Factory.Load<EdiIdentityApplication>(query);
					var applicationsSyncResponse = new DistributedDataSyncResponse<IdentityApplicationModel>();
					applicationsSyncResponse.SyncEnded = applications.Length < returnedRows;

					foreach (var application in applications)
					{
						var syncApplication = BuildIdentityApplicationModel(application);

						if (application.IDA_OH_ParentOrg.IsValid)
						{
							syncApplication.ParentOrgPK = application.IDA_OH_ParentOrg.ToGuid();
						}

						if (application.IDA_LD.IsValid)
						{
							syncApplication.LicenceDatabasePK = application.IDA_LD.ToGuid();
						}

						if (application.IDA_IDA_ParentApplication.IsValid)
						{
							syncApplication.ParentApplicationPK = application.IDA_IDA_ParentApplication.ToGuid();
						}

						if (Guid.TryParse(application.IDA_ClientID, out var clientId))
						{
							syncApplication.ClientID = clientId;
						}
						applicationsSyncResponse.Items.Add(syncApplication);
					}

					var infoMsg = $"{routingPath} | {lastSyncPK} | {NLogWrapper.Status.Ok}";
					AddInfoLog(infoMsg, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
					return Json(applicationsSyncResponse);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var errorMsg = $"{routingPath} | {NLogWrapper.Status.InternalError}";
					AddErrorLog(errorMsg, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, ex: ex);
					return InternalServerError(StatusMessages.ErrorQueryRecord);
				}
			}
		}

		[HttpGet]
		[Route("sync/organizations")]
		public IHttpActionResult SyncOrganizations(int maxRows, Guid lastSyncPK)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = Request.RequestUri.AbsolutePath;

			if (!ValidateRequestBearerToken(Request, routingPath, sessionId, out var actionResult, out var token))
			{
				return actionResult;
			}

			var returnedRows = Math.Min(maxRows, EDIDataRegistry.Instance.MaxRowsForDistributedDataSyncRequest.Value);

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					var query = new ZDBOnlyQuery(typeof(OrgHeader));
					query.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.GreaterThan, lastSyncPK);
					query.OrderBy = OrgHeaderSchema.Constants.PK;
					query.MaximumRows = returnedRows;

					var orgHeaders = Factory.Load<OrgHeader>(query);
					var organizationsSyncResponse = new DistributedDataSyncResponse<OrgHeaderModel>();
					organizationsSyncResponse.SyncEnded = orgHeaders.Length < returnedRows;

					foreach (var orgHeader in orgHeaders)
					{
						var syncOrganization = BuildOrgHeaderModel(orgHeader);
						organizationsSyncResponse.Items.Add(syncOrganization);
					}

					var infoMsg = $"{routingPath} | {lastSyncPK} | {NLogWrapper.Status.Ok}";
					AddInfoLog(infoMsg, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
					return Json(organizationsSyncResponse);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var errorMsg = $"{routingPath} | {NLogWrapper.Status.InternalError}";
					AddErrorLog(errorMsg, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, ex: ex);
					return InternalServerError(StatusMessages.ErrorQueryRecord);
				}
			}
		}

		[HttpGet]
		[Route("sync/licenceEnterprises")]
		public IHttpActionResult SyncLicenceEnterprises(int maxRows, Guid lastSyncPK)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = Request.RequestUri.AbsolutePath;

			if (!ValidateRequestBearerToken(Request, routingPath, sessionId, out var actionResult, out var token))
			{
				return actionResult;
			}

			var returnedRows = Math.Min(maxRows, EDIDataRegistry.Instance.MaxRowsForDistributedDataSyncRequest.Value);

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					var query = new ZDBOnlyQuery(typeof(LicenceEnterprise));
					query.AddToFilter(LicenceEnterpriseSchema.PK, SQLComparisonOperator.GreaterThan, lastSyncPK);
					query.OrderBy = LicenceEnterpriseSchema.Constants.PK;
					query.MaximumRows = returnedRows;

					var enterprises = Factory.Load<LicenceEnterprise>(query);
					var licenceEnterpriseDistributedDataSyncResponse = new DistributedDataSyncResponse<LicenceEnterpriseModel>();
					licenceEnterpriseDistributedDataSyncResponse.SyncEnded = enterprises.Length < returnedRows;

					foreach (var enterprise in enterprises)
					{
						var syncLicenceEnterprise = BuildLicenceEnterpriseModel(enterprise);
						licenceEnterpriseDistributedDataSyncResponse.Items.Add(syncLicenceEnterprise);
					}

					var infoMsg = $"{routingPath} | {lastSyncPK} | {NLogWrapper.Status.Ok}";
					AddInfoLog(infoMsg, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
					return Json(licenceEnterpriseDistributedDataSyncResponse);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var errorMsg = $"{routingPath} | {NLogWrapper.Status.InternalError}";
					AddErrorLog(errorMsg, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, ex: ex);
					return InternalServerError(StatusMessages.ErrorQueryRecord);
				}
			}
		}

		#endregion

		[HttpPost]
		[Route("application/oidcregister")]
		public IHttpActionResult RegisterOidcApplication([FromBody] IdentityOidcApplicationRequest oidcApplicationRequest)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = Request.RequestUri.AbsolutePath;

			if (oidcApplicationRequest == null)
			{
				var errorMsg = $"{NLogWrapper.Status.BadRequest}: {ErrorCodes.Descriptions.Validation_MissingRequiredField}";
				AddErrorLog(errorMsg, (int)HttpStatusCode.BadRequest, sessionId, routingPath);
				return BadRequest(ErrorCodes.Codes.Validation_MissingRequiredField, ErrorCodes.Descriptions.Validation_MissingRequiredField);
			}

			if (!ValidateRequestBearerToken(Request, routingPath, sessionId, out var actionResult, out var token))
			{
				return actionResult;
			}

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					var application = QueryApplicationFromToken(sessionId, routingPath, token);

					if (application == null)
					{
						return BadRequest(badRequestCode, StatusMessages.InvalidAccessToken);
					}

					var licenceDatabase = application.LicenceDatabase;
					if (licenceDatabase == null || !ProductTypes.IsEnterpriseFamily(licenceDatabase.LD_Product))
					{
						var errorMsg = $"{NLogWrapper.Status.BadRequest}: Invalid licence info {application.IDA_ClientID}";
						AddErrorLog(errorMsg, (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
						return BadRequest(badRequestCode, StatusMessages.InvalidAccessToken);
					}

					var tenantQuery = new ZQuery(EdiIdentityTenantSchema.IDT_AuthorityUrl, oidcApplicationRequest.AuthorityUrl);
					var authorityTenant = Factory.LoadTop1<EdiIdentityTenant>(tenantQuery);

					if (authorityTenant == null)
					{
						var errorMsg = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.InvalidAuthorityUrl} '{oidcApplicationRequest.AuthorityUrl}'";
						AddErrorLog(errorMsg, (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
						return BadRequest(badRequestCode, StatusMessages.InvalidAuthorityUrl);
					}

					var oidcApplicationQuery = new ZQuery();
					oidcApplicationQuery.AddToFilter(EdiIdentityApplicationSchema.IDA_LD, licenceDatabase.PK);
					oidcApplicationQuery.AddToFilter(EdiIdentityApplicationSchema.IDA_IDT, authorityTenant.PK);
					var oidcApplication = Factory.LoadTop1<EdiIdentityApplication>(oidcApplicationQuery);

					if (oidcApplication == null)
					{
						oidcApplication = Factory.New<EdiIdentityApplication>();
						oidcApplication.IDA_IDT = authorityTenant.PK;
						oidcApplication.IDA_LD = licenceDatabase.PK;
						oidcApplication.IDA_ApplicationName = application.IDA_ApplicationName + OidcApplicationNamePostfix;

						Factory.Save();
					}

					var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
					httpResponse.Content = new StringContent(oidcApplication.IDA_ClientID);
					var infoMsg = oidcApplication.IDA_ClientID.IsEmpty
						? $"{NLogWrapper.Status.Ok}: OIDC application is not created in Azure yet {application.IDA_ClientID}"
						: $"{NLogWrapper.Status.Ok}: {oidcApplication.IDA_ClientID} {application.IDA_ClientID}";
					AddInfoLog(infoMsg, (int)HttpStatusCode.OK, sessionId, routingPath: routingPath);
					return new ResponseMessageResult(httpResponse);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var errorMsg = $"{routingPath} | {NLogWrapper.Status.InternalError}";
					AddErrorLog(errorMsg, (int)HttpStatusCode.InternalServerError, sessionId, routingPath: routingPath, ex: ex);
					return InternalServerError(StatusMessages.ErrorSavingRecord);
				}
			}
		}

		const string OidcApplicationNamePostfix = "_OIDC";

		[HttpPost]
		[Route("application/oidcredirecturls")]
		public IHttpActionResult UpdateOidcApplicationRedirectUrls([FromBody] IdentityOidcRedirectUrlRequest oidcRedirectUrlRequest)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = Request.RequestUri.AbsolutePath;

			if (!ValidateRequestBearerToken(Request, routingPath, sessionId, out var actionResult, out var token))
			{
				return actionResult;
			}

			if (oidcRedirectUrlRequest == null || string.IsNullOrEmpty(oidcRedirectUrlRequest.AuthorityUrl) || string.IsNullOrEmpty(oidcRedirectUrlRequest.ClientId))
			{
				var warn = $"{NLogWrapper.Status.BadRequest}: {ErrorCodes.Descriptions.Validation_MissingRequiredField}";
				AddWarnLog(warn, ((int)HttpStatusCode.BadRequest), sessionId, routingPath: routingPath);
				return BadRequest(ErrorCodes.Codes.Validation_MissingRequiredField, ErrorCodes.Descriptions.Validation_MissingRequiredField);
			}

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					var ediIdentityApplication = QueryApplicationFromToken(sessionId, routingPath, token);

					if (ediIdentityApplication == null)
					{
						return BadRequest(badRequestCode, StatusMessages.InvalidAccessToken);
					}

					var licenceDatabase = ediIdentityApplication.LicenceDatabase;
					if (licenceDatabase == null || !ProductTypes.IsEnterpriseFamily(licenceDatabase.LD_Product))
					{
						var errorMsg = $"{NLogWrapper.Status.BadRequest}: Invalid licence info {ediIdentityApplication.IDA_ClientID}";
						AddErrorLog(errorMsg, (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
						return BadRequest(badRequestCode, StatusMessages.InvalidAccessToken);
					}

					var tenantQuery = new ZQuery(EdiIdentityTenantSchema.IDT_AuthorityUrl, oidcRedirectUrlRequest.AuthorityUrl);
					var authorityTenant = Factory.LoadTop1<EdiIdentityTenant>(tenantQuery);
					if (authorityTenant == null)
					{
						var errorMsg = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.InvalidAuthorityUrl} '{oidcRedirectUrlRequest.AuthorityUrl}'";
						AddErrorLog(errorMsg, (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
						return BadRequest(badRequestCode, StatusMessages.InvalidAuthorityUrl);
					}

					var oidcApplicationQuery = new ZQuery();
					oidcApplicationQuery.AddToFilter(EdiIdentityApplicationSchema.IDA_LD, licenceDatabase.PK);
					oidcApplicationQuery.AddToFilter(EdiIdentityApplicationSchema.IDA_IDT, authorityTenant.PK);
					oidcApplicationQuery.AddToFilter(EdiIdentityApplicationSchema.IDA_ClientID, oidcRedirectUrlRequest.ClientId);
					var oidcApplication = Factory.LoadTop1<EdiIdentityApplication>(oidcApplicationQuery);

					if (oidcApplication == null)
					{
						var errorMsg = $"{NLogWrapper.Status.BadRequest}: Invalid OIDC application {oidcRedirectUrlRequest.ClientId}";
						AddErrorLog(errorMsg, (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
						return BadRequest(badRequestCode, StatusMessages.InvalidClientId);
					}

					return SyncRedirectUrls(sessionId, routingPath, oidcApplication, oidcRedirectUrlRequest.RedirectUrls ?? []);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					AddErrorLog(NLogWrapper.Status.InternalError, (int)HttpStatusCode.InternalServerError, sessionId, routingPath: routingPath, ex: ex);
					return InternalServerError(StatusMessages.ErrorSavingRecord);
				}
			}
		}

		[HttpPost]
		[Route("application/redirecturls")]
		public IHttpActionResult UpdateApplicationRedirectUrls([FromBody] IdentityRedirectUrlRequest[] redirectUrls)
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = $"{Request.RequestUri.AbsolutePath}";

			if (!ValidateRequestBearerToken(Request, routingPath, sessionId, out var actionResult, out var token))
			{
				return actionResult;
			}

			if (redirectUrls == null)
			{
				var warn = $"{NLogWrapper.Status.BadRequest}: {ErrorCodes.Descriptions.Validation_MissingRequiredField}";
				AddWarnLog(warn, (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
				return BadRequest(ErrorCodes.Codes.Validation_MissingRequiredField, ErrorCodes.Descriptions.Validation_MissingRequiredField);
			}

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					var ediIdentityApplication = QueryApplicationFromToken(sessionId, routingPath, token);

					if (ediIdentityApplication == null)
					{
						return BadRequest(badRequestCode, StatusMessages.InvalidAccessToken);
					}

					return SyncRedirectUrls(sessionId, routingPath, ediIdentityApplication, redirectUrls.Select(ConvertIdentityRedirectUrlRequest).ToList());
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					AddErrorLog(NLogWrapper.Status.InternalError, (int)HttpStatusCode.InternalServerError, sessionId, routingPath: routingPath, ex: ex);
					return InternalServerError(StatusMessages.ErrorSavingRecord);
				}
			}

			IdentityRedirectUrl ConvertIdentityRedirectUrlRequest(IdentityRedirectUrlRequest identityRedirectUrlRequest)
			{
				return new IdentityRedirectUrl()
				{
					ApplicationName = identityRedirectUrlRequest.ApplicationName,
					RedirectUrl = identityRedirectUrlRequest.RedirectUrl,
					RedirectUrlType = identityRedirectUrlRequest.RedirectUrlType
				};
			}
		}

		IHttpActionResult SyncRedirectUrls(string sessionId, string routingPath, EdiIdentityApplication application, List<IdentityRedirectUrl> redirectUrls)
		{
			var redirectUrlsInDatabase = application.RedirectUrls;

			var toDeleteEdiIdentityRedirectUrls = redirectUrlsInDatabase
				.Where(irr => !redirectUrls.Any(eru => eru.ApplicationName == irr.IAR_ApplicationName && eru.RedirectUrlType == irr.IAR_RedirectType && eru.RedirectUrl == irr.IAR_RedirectUrl)).ToList();

			var toAddEdiIdentityRedirectUrls = redirectUrls
				.Where(irr => !redirectUrlsInDatabase.Any(eru => eru.IAR_ApplicationName == irr.ApplicationName && eru.IAR_RedirectType == irr.RedirectUrlType && eru.IAR_RedirectUrl == irr.RedirectUrl));

			foreach (var addEdiIdentityRedirectUrl in toAddEdiIdentityRedirectUrls)
			{
				var ediIdentityRedirectUrl = application.RedirectUrls.AddNew();
				ediIdentityRedirectUrl.IAR_ApplicationName = addEdiIdentityRedirectUrl.ApplicationName;
				ediIdentityRedirectUrl.IAR_RedirectType = addEdiIdentityRedirectUrl.RedirectUrlType;
				ediIdentityRedirectUrl.IAR_RedirectUrl = addEdiIdentityRedirectUrl.RedirectUrl;
			}

			foreach (var deleteEdiIdentityRedirectUrl in toDeleteEdiIdentityRedirectUrls)
			{
				deleteEdiIdentityRedirectUrl.Delete();
			}

			Factory.Save();

			var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
			var info = $"{application.IDA_ClientID} | {NLogWrapper.Status.Ok}.";
			AddInfoLog(info, (int)HttpStatusCode.OK, sessionId, routingPath: routingPath);
			return new ResponseMessageResult(httpResponse);
		}

		[HttpPost]
		[Route("load/databasenumber")]
		public IHttpActionResult LoadDatabaseNumberByClientId()
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = Request.RequestUri.AbsolutePath;

			if (!ValidateRequestBearerToken(Request, routingPath, sessionId, out var actionResult, out var token))
			{
				return actionResult;
			}

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					var application = QueryApplicationFromToken(sessionId, routingPath, token);

					if (application == null)
					{
						return BadRequest(badRequestCode, StatusMessages.InvalidAccessToken);
					}

					var database = application.LicenceDatabase;
					if (database == null)
					{
						AddErrorLog($"{NLogWrapper.Status.BadRequest}: Invalid licence info {application.IDA_ClientID}", (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
						return BadRequest(badRequestCode, StatusMessages.InvalidAccessToken);
					}

					var databaseNumber = database.LD_DatabaseNumber;

					var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
					httpResponse.Content = new StringContent(databaseNumber.ToString());
					var infoMsg = $"{databaseNumber} | {NLogWrapper.Status.Ok}";
					AddInfoLog(infoMsg, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
					return new ResponseMessageResult(httpResponse);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var errorMsg = $"{routingPath} | {NLogWrapper.Status.InternalError}";
					AddErrorLog(errorMsg, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, ex: ex);
					return InternalServerError(StatusMessages.ErrorQueryRecord);
				}
			}
		}

		[HttpGet]
		[Route("lsn")]
		public IHttpActionResult GetLsn()
		{
			var sessionId = Guid.NewGuid().ToString();
			var routingPath = Request.RequestUri.AbsolutePath;

			if (!ValidateRequestBearerToken(Request, routingPath, sessionId, out var actionResult, out var token))
			{
				return actionResult;
			}

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					var slnArrayBytes = QueryLsn();

					var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
					httpResponse.Content = new ByteArrayContent(slnArrayBytes);
					var infoMsg = $"{routingPath} | {NLogWrapper.Status.Ok}";
					AddInfoLog(infoMsg, ((int)HttpStatusCode.OK), sessionId, routingPath: routingPath);
					return new ResponseMessageResult(httpResponse);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var errorMsg = $"{routingPath} | {NLogWrapper.Status.InternalError}";
					AddErrorLog(errorMsg, ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, ex: ex);
					return InternalServerError(StatusMessages.ErrorQueryRecord);
				}
			}
		}

		protected virtual byte[] QueryLsn()
		{
			var getLsnQuery = "SELECT sys.fn_cdc_get_max_lsn()";
			return Db.Connection.ExecuteScalar<byte[]>(getLsnQuery);
		}

		protected virtual EdiIdentityApplication QueryApplicationFromToken(string sessionId, string routingPath, JwtSecurityToken token)
		{
			var clientId = token.Payload.Azp;
			var tenantId = token.Claims.SingleOrDefault(claim => claim.Type == "tid")?.Value;

			if (clientId.IsNullOrEmpty())
			{
				var errorMsg = $"{NLogWrapper.Status.BadRequest}: Access token client id is empty";
				AddErrorLog(errorMsg, (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
				return null;
			}

			if (tenantId.IsNullOrEmpty())
			{
				var errorMsg = $"{NLogWrapper.Status.BadRequest}: Access token tenant id is empty";
				AddErrorLog(errorMsg, (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
				return null;
			}

			var application = QueryApplicationFromTenantIdAndClientId(tenantId, clientId);

			if (application == null)
			{
				var errorMsg = $"{NLogWrapper.Status.BadRequest}: No application found from tenant id '{tenantId}' and client id '{clientId}'";
				AddErrorLog(errorMsg, (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
				return null;
			}

			return application;
		}

		protected virtual EdiIdentityApplication QueryApplicationFromTenantIdAndClientId(string tenantId, string clientId)
		{
			var dbOnlyQuery = new ZDBOnlyQuery(typeof(EdiIdentityApplication));
			dbOnlyQuery.AddToFilter(EdiIdentityApplicationSchema.IDA_ClientID, clientId);
			dbOnlyQuery.AddToFilter(EdiIdentityApplicationSchema.IDA_IsActive, true);
			var subQuery = new ZDBOnlySubQuery(typeof(EdiIdentityTenant), EdiIdentityApplicationSchema.IDA_IDT);
			subQuery.AddToFilter(EdiIdentityTenantSchema.IDT_TenantId, tenantId);
			dbOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);

			return Factory.LoadTop1<EdiIdentityApplication>(dbOnlyQuery);
		}

		string GetCertificateCategory(EdiIdentityCertificate ediIdentityCertificate)
		{
			if (ediIdentityCertificate.LicenseDatabase == null)
			{
				return CertificateCategory.Unclassified;
			}

			if (ediIdentityCertificate.LicenseDatabase.EnterpriseCode == Core.Constants.WiseTechGlobalInternalSystemCodes.EDI
				&& ediIdentityCertificate.LicenseDatabase.LD_ServerCode == "SYD")
			{
				return CertificateCategory.Production;
			}
			if (Core.Constants.WiseTechGlobalInternalSystemCodes.AllCodes.ToList().Contains(ediIdentityCertificate.LicenseDatabase.EnterpriseCode))
			{
				return CertificateCategory.Internal;
			}
			else
			{
				if (ediIdentityCertificate.LicenseDatabase.LD_LicenceType == DatabaseTypes.Codes.Production)
				{
					return CertificateCategory.Production;
				}
			}

			return CertificateCategory.Test;
		}

		protected virtual bool CheckIfApplicationExistsInDb(string clientId)
		{
			return new BusinessObjectFactory().Exists(typeof(EdiIdentityApplication), new ZQuery(EdiIdentityApplicationSchema.IDA_ClientID, clientId));
		}

		protected virtual IdentityCertificateResponse BuildIdentityCertificateResponse(EdiIdentityCertificate certificate) => new IdentityCertificateResponse
		{
			CertificateData = certificate.ICE_CertificateData,
			Status = new EdiIdentityCertificateProcessingStatus().GetDescriptionFromCode(certificate.ICE_ProcessingStatus),
			ClientId = certificate.Application.IDA_ClientID,
			TenantId = EDIDataRegistry.Instance.AzureApplicationManagementTenantID.Value,
			StatusCode = certificate.ICE_ProcessingStatus
		};

		protected virtual IdentityApplicationModel BuildIdentityApplicationModel(EdiIdentityApplication application) => new IdentityApplicationModel()
		{
			PK = application.PK.ToGuid(),
			ApplicationName = application.IDA_ApplicationName,
			ApplicationType = application.IDA_ApplicationType,
			Product = application.IDA_Product,
			ApplicationModule = application.IDA_ApplicationModule
		};

		protected virtual OrgHeaderModel BuildOrgHeaderModel(OrgHeader orgHeader) => new OrgHeaderModel()
		{
			PK = orgHeader.PK.ToGuid(),
			Code = orgHeader.OH_Code,
		};

		protected virtual LicenceEnterpriseModel BuildLicenceEnterpriseModel(LicenceEnterprise enterprise) => new LicenceEnterpriseModel()
		{
			PK = enterprise.PK.ToGuid(),
			Code = enterprise.LE_EnterpriseCode,
		};

		protected virtual bool ValidateRequestBearerToken(HttpRequestMessage request, string routingPath, string sessionId, out IHttpActionResult actionResult, out JwtSecurityToken token)
		{
			token = null;
			actionResult = null;
			var accessToken = SystemToSystemTrustHelper.GetBearerToken(request);

			if (string.IsNullOrEmpty(accessToken))
			{
				var error = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.EmptyAccessToken}";
				AddErrorLog(error, (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
				actionResult = BadRequest(badRequestCode, StatusMessages.EmptyAccessToken);
				return false;
			}

			try
			{
				var authorityUrl = TrustHelper.GetAuthorityUrl(Logger);
				if (!string.IsNullOrEmpty(authorityUrl))
				{
					var ediClientID = SystemDataRegistry.Instance.EDIClientID.Value;
					token = TokenValidator.ValidateAccessToken(authorityUrl, ediClientID, accessToken, ConfigurationHelper.ConfigurationManagerCache, new TokenValidationLogger(new BaseLogger(GetType())), CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				AddErrorLog("The token is invalid.", ((int)HttpStatusCode.InternalServerError), sessionId, routingPath: routingPath, ex: e);
			}

			if (token == null)
			{
				var warn = $"{NLogWrapper.Status.BadRequest}: {StatusMessages.InvalidAccessToken}";
				AddWarnLog(warn, (int)HttpStatusCode.BadRequest, sessionId, routingPath: routingPath);
				actionResult = BadRequest(badRequestCode, StatusMessages.InvalidAccessToken);
				return false;
			}

			return true;
		}

		protected SystemToSystemTrustHelper TrustHelper { get; set; } = new SystemToSystemTrustHelper();

		class StatusMessages
		{
			public const string PasswordMismatch = "Password Mismatch";
			public const string DatabaseNotActive = "Database Not Active";
			public const string DatabaseNotRegistered = "Database Not Registered";
			public const string InvalidOperationId = "OperationId Is Invalid";
			public const string EmptyAccessToken = "Access Token Not Provided";
			public const string InvalidAccessToken = "Invalid Access Token";
			public const string InvalidClientId = "Invalid Client Id";
			public const string InvalidCaRoot = "Invalid Ca Root";
			public const string InvalidCertificateSignRequest = "Invalid Certificate Sign Request";
			public const string ApplicationExistsAndNotActive = "The Application Already Exists And Is Not Active";
			public const string InvalidCertificateRegisterRequestData = "Invalid Certificate Register Request Data";
			public const string ErrorSavingRecord = "Error saving record. Please contact the administrator.";
			public const string ErrorQueryRecord = "Error query record. Please contact the administrator.";
			public const string ServiceInternalError = "Service internal error. Please contact the administrator.";
			public const string InvalidAuthorityUrl = "Invalid Authority URL";
		}
	}
}
