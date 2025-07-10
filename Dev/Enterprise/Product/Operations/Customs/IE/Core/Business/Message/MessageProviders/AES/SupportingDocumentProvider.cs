using System;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IE.Business.AES
{
	public class SupportingDocumentProvider : ISupportingDocument
	{
		readonly SupportingDocument supportingDocument;

		public SupportingDocumentProvider(SupportingDocument supportingDocument)
		{
			this.supportingDocument = Argument.NotNull(supportingDocument, nameof(supportingDocument));
		}

		public string LineNumber => supportingDocument.CSI_ItemNumber.ToString();

		public string Type => supportingDocument.CSI_Code;

		public string Reference => supportingDocument.CSI_ReferenceNumber;

		public string IssuingAuthority => supportingDocument.CSI_AdditionalDescription;

		public DateTime ExpirationDate => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(supportingDocument.CSI_DateOfExpiry);
	}
}
