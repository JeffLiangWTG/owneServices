using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.IT.Business;

public static class SupportingDocumentExtension
{
	public static bool HasDocument(this IEnumerable<SupportingDocument> supportingDocuments, ZString code) => supportingDocuments?.Any(x => x.CSI_Code == code) ?? false;

	public static ZString GetYearOfIssue(this ICusSupportingInfo supportingDocument) => supportingDocument.CSI_DateOfIssue.ToYearDateString();

	public static void SetYearOfIssue(this ICusSupportingInfoWithYearOfIssue supportingDocument, ZString yearOfIssue)
	{
		var oldValue = supportingDocument.GetYearOfIssue();
		supportingDocument.CSI_DateOfIssue = ZDateTimeHelper.ParseDateTimeFromYear(yearOfIssue);
		supportingDocument.CSI_YearOfIssueInfo.RefreshBinding(oldValue);
	}

	public static string GetReferenceNumberWithYearOfIssueAndCountry(this ICusSupportingInfo supportingDocument)
	{
		const string referenceNumberDelimiter = "-";

		if (supportingDocument == null)
		{
			return null;
		}

		return new ZStringBuilder()
			.AppendIfNotEmpty(supportingDocument.GetYearOfIssue())
			.AppendIfNotEmpty(supportingDocument.CSI_RN_NKCountryCode)
			.AppendIfNotEmpty(supportingDocument.CSI_ReferenceNumber)
			.AppendIfBuilderIsEmpty(referenceNumberDelimiter)
			.ToStringWithDelimiterBetweenAppends(referenceNumberDelimiter);
	}
}
