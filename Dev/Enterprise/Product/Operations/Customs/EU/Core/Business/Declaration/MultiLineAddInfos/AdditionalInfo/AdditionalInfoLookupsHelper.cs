using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public static class AdditionalInfoLookupsHelper
	{
		public static ZString GetImportRefCusCodeListType(ZString subType)
		{
			return (string)subType switch
			{
				AdditionalInfoSubTypeList.Codes.AdditionalInformation => UniversalReferenceConstants.RefCusCodeListType.Code.ImportAdditionalInformation,
				AdditionalInfoSubTypeList.Codes.AdditionalReference => UniversalReferenceConstants.RefCusCodeListType.Code.ImportAdditionalReference,
				AdditionalInfoSubTypeList.Codes.TransportDocument => UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument,
				_ => ZString.Empty
			};
		}

		public static ZString GetUCCExportRefCusCodeListType(ZString subType)
		{
			return (string)subType switch
			{
				AdditionalInfoSubTypeList.Codes.AdditionalInformation => UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalInformation,
				AdditionalInfoSubTypeList.Codes.AdditionalReference => UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalReference,
				AdditionalInfoSubTypeList.Codes.TransportDocument => UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument,
				_ => ZString.Empty
			};
		}
	}
}
