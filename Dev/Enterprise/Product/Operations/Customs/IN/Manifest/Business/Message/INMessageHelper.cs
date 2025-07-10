using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IN.Manifest.Business;

static class INMessageHelper
{
	public static string GetMessageRecipient(Messaging.Business.EDIMessage message)
	{
		return message.EM_IsTestMessage ? Constants.Messaging.IceGate.TestName : Constants.Messaging.IceGate.ProdName;
	}

	public static string GetMessageSenderEmailId(Messaging.Business.EDIMessage message)
	{
		return message.ExternalPassword?.GP_MailBoxID ?? ZString.Empty;
	}

	public static string GetMessageRecipientEmailId(Messaging.Business.EDIMessage message)
	{
		var linkedObject = message.EM_LinkedObject;
		if (linkedObject is AsycudaManifestHeader manifestHeader)
		{
			var customsOfficeCode = manifestHeader.AMA_CustomsOffice;
			return GetEmailIdForCustomsOffice(message.Factory, customsOfficeCode);
		}
		return string.Empty;
	}

	public static string GetMessageCopyToEmailId(Messaging.Business.EDIMessage message)
	{
		return message.ExternalPassword is GlbLoginPassword loginPassword ? loginPassword.CopyToMailBox : string.Empty;
	}

	static string GetEmailIdForCustomsOffice(BusinessObjectFactory factory, string officeCode)
	{
		var customsOffice = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, officeCode, Core.Constants.CountryCodes.India,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);

		return customsOffice?.GetAttribute(Constants.RefCusCodeList.Attributes.EmailAddress) ?? string.Empty;
	}
}
