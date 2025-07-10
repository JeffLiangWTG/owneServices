using DEReferenceConstants = Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.NCTS.Business
{
	static class AdditionalDocumentExtension
	{
		internal static bool IsNotificationToCustomsOffice(this EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo additionalDocument)
		{
			return additionalDocument.CSI_Code == DEReferenceConstants.AdditionalInfoCodes.T0000 && additionalDocument.IsAnAdditionalInformation;
		}
	}
}
