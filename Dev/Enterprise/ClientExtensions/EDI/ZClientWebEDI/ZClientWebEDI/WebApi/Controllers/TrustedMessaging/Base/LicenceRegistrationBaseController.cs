using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Interfaces;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public abstract class LicenceRegistrationBaseController<TRequest> : TrustedController where TRequest : ITenantRegistrationInfo
	{
		public LicenceRegistrationBaseController() : base()
		{
		}

		public LicenceRegistrationBaseController(NLogWrapper logger) : base(logger)
		{
		}

		protected ILicenceRegistrationResponse RegisterTenantCore(ITrustedContext context, TRequest requestInfo)
		{
			RunTenantRegistrationValidation(context, requestInfo);

			if (context.Messages?.Messages.Count > 0)
			{
				return null;
			}

			if (string.IsNullOrEmpty(requestInfo.TenantId))
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidValue, StatusMessages.NoTenantId);
				return null;
			}

			var duplicateDatabase = FindDuplicateDatabase(Factory, requestInfo);

			if (duplicateDatabase != null)
			{
				duplicateDatabase.LD_TenantID = requestInfo.TenantId;
				Factory.Save();
			}

			var importResult = TryImportLicenceDatabase(requestInfo, duplicateDatabase);

			if (!importResult.Success)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidValue, importResult.OutputMessage);
				return null;
			}

			return GetProductRegistrationResponse(importResult.DatabaseNumber);
		}

		protected abstract ILicenceRegistrationResponse GetProductRegistrationResponse(int databaseNumber);

		protected virtual void RunTenantRegistrationValidation(ITrustedContext context, TRequest requestInfo)
		{
			if (!context.Success)
			{
				return;
			}

			if (context.Product != requestInfo.Product)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidValue, StatusMessages.ProductMismatch);
				return;
			}

			if (string.IsNullOrWhiteSpace(requestInfo.Product))
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_MissingRequiredField, StatusMessages.NoRegistrationContent);
				return;
			}

			if (string.IsNullOrEmpty(requestInfo.OrgCountry) && string.IsNullOrEmpty(requestInfo.EnterpriseCode) && string.IsNullOrEmpty(requestInfo.ServerCode) && string.IsNullOrEmpty(requestInfo.CargowiseCompanyCode))
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidValue, StatusMessages.NoOrgCountryOrLicenceInfo);
				return;
			}
		}

		protected static LicenceDatabase FindDuplicateDatabase(BusinessObjectFactory factory, TRequest tenantRegistrationInfo)
		{
			LicenceDatabase result = null;

			if (new[] { tenantRegistrationInfo.OrgName, tenantRegistrationInfo.OrgCountry, tenantRegistrationInfo.Address1, tenantRegistrationInfo.Address2,
						tenantRegistrationInfo.City, tenantRegistrationInfo.Postcode, tenantRegistrationInfo.State }
					.Any(x => !string.IsNullOrWhiteSpace(x)))
			{
				var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				orgSubQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
				if (!string.IsNullOrEmpty(tenantRegistrationInfo.OrgName))
				{
					orgSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, tenantRegistrationInfo.OrgName);
				}

				var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_IsActive, true);
				if (!string.IsNullOrEmpty(tenantRegistrationInfo.OrgCountry))
				{
					orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_RN_NKCountryCode, tenantRegistrationInfo.OrgCountry);
				}
				if (!string.IsNullOrEmpty(tenantRegistrationInfo.Address1))
				{
					orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_Address1, tenantRegistrationInfo.Address1);
				}
				if (!string.IsNullOrEmpty(tenantRegistrationInfo.Address2))
				{
					orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_Address2, tenantRegistrationInfo.Address2);
				}
				if (!string.IsNullOrEmpty(tenantRegistrationInfo.City))
				{
					orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_City, tenantRegistrationInfo.City);
				}
				if (!string.IsNullOrEmpty(tenantRegistrationInfo.Postcode))
				{
					orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_PostCode, tenantRegistrationInfo.Postcode);
				}
				if (!string.IsNullOrEmpty(tenantRegistrationInfo.State))
				{
					orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_State, tenantRegistrationInfo.State);
				}

				orgSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

				var deduplicationOrgSubQuery = new ZDBOnlySubQuery(typeof(DeduplicationOrganisation), MDMAdminPanelOrganisationViewSchema.PK);
				deduplicationOrgSubQuery.AddSubQuery(orgSubQuery, JoinCondition.And);

				var destinationDatabaseQuery = new ZDBOnlyQuery(typeof(LicenceDatabase));
				destinationDatabaseQuery.AddSubQuery(LicenceDatabaseSchema.LD_OH_WebAccessOrg, deduplicationOrgSubQuery, JoinCondition.And);
				destinationDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_Product, tenantRegistrationInfo.Product);
				destinationDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_LicenceType, tenantRegistrationInfo.LicenceType ?? DatabaseTypes.Codes.Production);
				destinationDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_TenantID, string.Empty);
				destinationDatabaseQuery.OrderBy = LicenceDatabaseSchema.Constants.LD_DatabaseNumber;

				result = factory.LoadTop1<LicenceDatabase>(destinationDatabaseQuery);
			}

			return result;
		}

		protected abstract LicenceDatabaseRegistrationImporter.ImportResult TryImportLicenceDatabase(TRequest tenantRegistrationInfo, LicenceDatabase duplicateDatabase = null);

		protected static class StatusMessages
		{
			public const string MessageNotSigned = "Message Not Signed";
			public const string NoHttpContent = "No Http Content";
			public const string NoRegistrationContent = "No Registration Content";
			public const string ProductMismatch = "Product Mismatch";
			public const string NoOrgCountryOrLicenceInfo = "No Country or Licencing Info";
			public const string NoTenantId = "The tenant_id is blank";
			public const string TenantAlreadyRegistered = "The tenant_id has been already registered";
			public const string TrustedSystemAlreadyRegistered = "The system_id has been already registered";
			public const string CertificateAuthorityNotAvailable = "Certificate Authority Not Available";
			public const string AnotherRegistrationInProgress = "Another Registration Is In Progress";
			public const string CentralSystemCertNotAvailable = "Central System Cert Not Available";
			public const string RegistrationCertNotAvailable = "Registration Cert Not Available";
		}
	}
}
