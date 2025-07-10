using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.DocumentWrappers.Customs.EU;

namespace Enterprise.Customs.IT.Business;

public class ITCompactProducedDocumentsCertificatesBuilder : CompactProducedDocumentsCertificatesBuilder
{
	protected override void AppendCountrySpecificFields(ZStringBuilder lineSb, SupportingDocument supportingDocument)
	{
		lineSb.Append(supportingDocument.CSI_Code);
		lineSb.AppendIfNotEmpty(supportingDocument.CSI_RN_NKCountryCode);

		if (supportingDocument is ISupportingDocument localSupportingDocument)
		{
			lineSb.AppendIfNotEmpty(localSupportingDocument.YearOfIssue);
		}

		lineSb.AppendIfNotEmpty(supportingDocument.CSI_ReferenceNumber);
		lineSb.AppendIfNotEmpty(supportingDocument.CSI_UnitOfQuantity);

		var quantity = supportingDocument.CSI_Quantity;
		if (!quantity.IsEmpty)
		{
			lineSb.Append(quantity.ToString("#0.#####", CultureInfo.InvariantCulture));
		}
	}
}
