using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business
{
	public class MessageSignatureXtInfoResolver
	{
		public MessageSignatureXtInfoResolver(EDIMessage ediMessage, string staffCode = null)
		{
			this.ediMessage = Argument.NotNull(ediMessage, nameof(ediMessage));
			factory = ediMessage.Factory;
			this.staffCode = staffCode;
		}

		internal (string messageAlert, GlbILStaffExternalPassword externalPassword, string messageNote) GetMessageAttributes()
		{
			var digitalSignatureIsDisabledByRegistry = Res.GetString("7F8473C8-1E51-47C9-BFB1-A20BE06EF3E5", "Digital Signature is disabled by Registry");
			GlbILStaffExternalPassword externalPassword = null;
			var sendMessagesWithoutDigitalSignature = ILCustomsDataRegistry.Instance.SendILCustomsMessagesWithoutDigitalSignature.Value;
			var personalDigitalSignature = ILCustomsDataRegistry.Instance.PersonalDigitalSignatureFallbackConfiguration.Value;
			var userWhoQueuedThisRecord = !ediMessage.EM_SystemCreateUser.IsEmpty
				? ediMessage.EM_SystemCreateUser
				: staffCode;

			var messageSignType = MessageSignatureTypeProvider.GetSignType(ediMessage.EM_MessageSubType);
			switch (messageSignType)
			{
				case MessageSignatureType.NoneSignatureType:
					return (null, null, null);

				case MessageSignatureType.PersonalSignature:
					if (sendMessagesWithoutDigitalSignature)
					{
						return (null, null, digitalSignatureIsDisabledByRegistry);
					}

					externalPassword = personalDigitalSignature == DigitalSignatureFallbackList.Codes.StaffOnly
						? GetValidStaffExternalPassword(userWhoQueuedThisRecord)
						: ResolveStaffOrDirectReportMessageAttribute(userWhoQueuedThisRecord);

					break;
				case MessageSignatureType.CompanySignature:
					if (sendMessagesWithoutDigitalSignature)
					{
						return (null, null, digitalSignatureIsDisabledByRegistry);
					}

					var glbCompanyWrapper = new GlbCompanyWrapper(ediMessage.Company);
					externalPassword =
						ResolveStaffOrDirectReportMessageAttribute(userWhoQueuedThisRecord)
						?? GetValidStaffExternalPassword(glbCompanyWrapper.GlbExternalPassword.GP_UserID);
					break;
			}

			if (externalPassword == null)
			{
				var certificateErrorMessage = Res.GetString("94B0A8D8-8773-4E36-9F3E-F66FAD6ECA11", "No Valid digital sign certificate found for signing this message – please review your staff or company configuration");
				return (certificateErrorMessage, null, null);
			}

			return (null, externalPassword, null);
		}

		GlbILStaffExternalPassword ResolveStaffOrDirectReportMessageAttribute(string staffCode)
		{
			var password = GetValidStaffExternalPassword(staffCode);
			password ??= GetValidStaffExternalPasswordForDirectManager(staffCode);
			return password;
		}

		GlbILStaffExternalPassword GetValidStaffExternalPasswordForDirectManager(string staffCode)
		{
			if (staffCode.IsNullOrEmpty())
			{
				return null;
			}

			var (glbILStaffExternalPassword, _) = factory.FetchStaffExternalPasswordForManager(staffCode);
			return glbILStaffExternalPassword;
		}

		GlbILStaffExternalPassword GetValidStaffExternalPassword(string staffCode)
		{
			if (staffCode.IsNullOrEmpty())
			{
				return null;
			}

			var (glbILStaffExternalPassword, _) = factory.FetchStaffExternalPassword(staffCode);
			return glbILStaffExternalPassword;
		}

		readonly EDIMessage ediMessage;
		readonly BusinessObjectFactory factory;
		readonly ZString staffCode;
	}
}
