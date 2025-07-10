using System;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class SupportingDocumentProvider : ISupportingDocument
	{
		public SupportingDocumentProvider(SupportingDocument supportingDocument)
		{
			this.supportingDocument = Argument.NotNull(supportingDocument, nameof(supportingDocument));
		}
		readonly SupportingDocument supportingDocument;

		public string DocumentLineItemNumber => supportingDocument.CSI_ItemNumber.ToString();

		public string IssuingAuthorityName => supportingDocument.CSI_AdditionalDescription;

		public DateTime DateOfValidity => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(supportingDocument.CSI_DateOfExpiry);

		public string Type => supportingDocument.CSI_Code;

		public string Reference => supportingDocument.CSI_ReferenceNumber;

		public string CcQualifier => null;
	}
}
