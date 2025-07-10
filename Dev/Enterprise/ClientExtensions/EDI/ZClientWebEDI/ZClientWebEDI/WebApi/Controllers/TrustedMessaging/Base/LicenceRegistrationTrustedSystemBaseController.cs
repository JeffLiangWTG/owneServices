using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class LicenceRegistrationTrustedSystemBaseController : LicenceRegistrationBaseController<TenantRegistrationInfo>
	{
		public LicenceRegistrationTrustedSystemBaseController() : base()
		{
		}

		public LicenceRegistrationTrustedSystemBaseController(NLogWrapper logger) : base(logger)
		{
		}

		protected void RegisterTenantCore(TrustedContext<TenantRegistrationInfo, ProductRegistrationResponse> context)
		{
			var response = RegisterTenantCore(context, context.RequestInfo);

			if (response != null)
			{
				context.ResponseInfo = (ProductRegistrationResponse)response;
			}
		}

		protected override void RunTenantRegistrationValidation(ITrustedContext context, TenantRegistrationInfo requestInfo)
		{
			if (!context.Success)
			{
				return;
			}

			var concreteContext = context as TrustedContext<TenantRegistrationInfo, ProductRegistrationResponse>;

			if (string.IsNullOrWhiteSpace(requestInfo.SystemId))
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_MissingRequiredField, StatusMessages.NoRegistrationContent);
				return;
			}

			if (string.IsNullOrWhiteSpace(requestInfo.LicenceType))
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_MissingRequiredField, StatusMessages.NoRegistrationContent);
				return;
			}

			base.RunTenantRegistrationValidation(context, requestInfo);

			if (context.Messages != null && context.Messages.Messages.Count > 0)
			{
				return;
			}

			var database = concreteContext.TrustedSystem?.FindTenantDatabaseByTrustedInfo(requestInfo);
			if (database != null)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidValue, StatusMessages.TenantAlreadyRegistered);
				return;
			}
		}

		protected override LicenceDatabaseRegistrationImporter.ImportResult TryImportLicenceDatabase(TenantRegistrationInfo tenantRegistrationInfo, LicenceDatabase duplicateDatabase = null)
		{
			return LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(tenantRegistrationInfo);
		}

		protected override ILicenceRegistrationResponse GetProductRegistrationResponse(int databaseNumber)
		{
			return new ProductRegistrationResponse() { DatabaseNumber = databaseNumber };
		}
	}
}
