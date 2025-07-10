using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business;

public static class CusAuthorisationHeaderExtensions
{
	public static ZBool SupportImportSupportingDocumentsRule(this CusAuthorisationHeader authorisationHeader)
	{
		var authorisationType = authorisationHeader.GetAuthorizationTypeOrEmptyIfNull();
		return authorisationType == CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP || authorisationType == CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1
				|| authorisationType == CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2 || authorisationType == CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
	}

	public static ZBool SupportExportSupportingDocumentsRule(this CusAuthorisationHeader authorisationHeader)
	{
		return authorisationHeader.GetAuthorizationTypeOrEmptyIfNull() == CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
	}

	public static ZBool SupportNctsSupportingDocumentsRule(this CusAuthorisationHeader authorisationHeader)
	{
		return authorisationHeader.GetAuthorizationTypeOrEmptyIfNull() == CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
	}

	public static ZString GetSupportedDocumentRefCusCodeListTypeCode(this CusAuthorisationHeader authorisationHeader)
	{
		if (authorisationHeader.SupportImportSupportingDocumentsRule())
		{
			return Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
		}
		if (authorisationHeader.SupportExportSupportingDocumentsRule())
		{
			return Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
		}
		if (authorisationHeader.SupportNctsSupportingDocumentsRule())
		{
			return EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS;
		}
		return ZString.Empty;
	}

	public static ZBool SupportImportLocationQualifierLB(this CusAuthorisationHeader authorisationHeader)
	{
		var authorisationType = authorisationHeader.GetAuthorizationTypeOrEmptyIfNull();
		return authorisationType == CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP
				|| authorisationType == CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1
				|| authorisationType == CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
	}

	public static ZBool SupportImportLocationQualifierLC(this CusAuthorisationHeader authorisationHeader)
	{
		var authorisationType = authorisationHeader.GetAuthorizationTypeOrEmptyIfNull();
		return authorisationType == CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport
				|| authorisationType == CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport;
	}

	#region Implementation

	static ZString GetAuthorizationTypeOrEmptyIfNull(this CusAuthorisationHeader authorisationHeader) => authorisationHeader?.CPH_Type ?? ZString.Empty;

	#endregion
}
