using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class ImportDocumentProvider : IImportDocument
	{
		public ImportDocumentProvider(SupportingDocument document)
		{
			this.document = Argument.NotNull(document, nameof(document));
		}
		readonly SupportingDocument document;

		public string Type => document.CSI_Code;

		public string ReferenceNumber => document.CSI_ReferenceNumber;

		public DateTime? IssuingDate => document.CSI_DateOfIssue.ToNullableDateTime()?.Date;
	}
}
