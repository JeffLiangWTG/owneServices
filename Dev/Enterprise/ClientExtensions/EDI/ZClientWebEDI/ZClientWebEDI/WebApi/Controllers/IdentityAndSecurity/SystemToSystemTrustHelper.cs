using System;
using System.Net;
using System.Net.Http;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NLog;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class SystemToSystemTrustHelper
	{
		public string GetAuthorityUrl(NLogWrapper logger)
		{
			return GetAuthorityUrlCore(logger);
		}

		protected virtual string GetAuthorityUrlCore(NLogWrapper logger)
		{
			var tenantId = EDIDataRegistry.Instance.AzureApplicationManagementTenantID.Value;
			if (string.IsNullOrEmpty(tenantId))
			{
				var errorMsg = $"Registry '{EDIDataRegistry.Instance.AzureApplicationManagementTenantID.Location()}' is not overridden.";
				logger?.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.BadRequest));
				return string.Empty;
			}

			return $"https://login.microsoftonline.com/{tenantId}/v2.0/";
		}

		public static string GetBearerToken(HttpRequestMessage request)
		{
			if (request.Headers.Authorization != null && request.Headers.Authorization.Scheme.Equals(BearerKey, StringComparison.OrdinalIgnoreCase))
			{
				return request.Headers.Authorization.Parameter;
			}

			return null;
		}

		public const string BearerKey = "Bearer";

		public static LicenceDatabase GetLicenceDatabase(BusinessObjectFactory factory, ITrustedContext context, SystemToSystemTrustedRegisteredInfo info)
		{
			var success = int.TryParse(info.DatabaseNumber, out var databaseNumber);
			if (!success)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidValue, DatabaseNumberNotValidMessage);
				return null;
			}

			var licenceDatabase = factory.LoadTop1<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, databaseNumber));

			if (licenceDatabase == null)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidValue, DatabaseNumberNotValidMessage);
				return null;
			}

			return licenceDatabase;
		}

		public const string DatabaseNumberNotValidMessage = "Database Number is not valid.";
	}
}
