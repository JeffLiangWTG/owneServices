using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.DocumentWrappers;

public class ProducedDocumentsCertificatesBuilder : Enterprise.DocumentWrappers.Customs.EU.ProducedDocumentsCertificatesBuilder
{
	protected override void AppendSupportingDocumentCore(ZStringBuilder lineSb, EU.Business.Declaration.MultiLineAddInfos.SupportingDocument supportingDocument, bool isPhase5Departure = false)
	{
		if (supportingDocument is SupportingDocument frSupportingDocument && !frSupportingDocument.CSI_IsDTP)
		{
			var line = new ZStringBuilder();
			line.AppendIfNotEmpty(frSupportingDocument.CSI_Code + (frSupportingDocument.CSI_ReferenceNumber.IsEmpty ? "" : " " + frSupportingDocument.CSI_ReferenceNumber)
				+ (frSupportingDocument.CSI_DateOfIssue.IsEmpty ? "" : " " + frSupportingDocument.CSI_DateOfIssue.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.CurrentCulture))
				+ (frSupportingDocument.IsD48AndNotClosed ? $" (D48 : {frSupportingDocument.CSI_Value.Round(0)} - {frSupportingDocument.CSI_Quantity3.Round(0)})" : ""));
			lineSb.Append(line);
		}
	}
}
