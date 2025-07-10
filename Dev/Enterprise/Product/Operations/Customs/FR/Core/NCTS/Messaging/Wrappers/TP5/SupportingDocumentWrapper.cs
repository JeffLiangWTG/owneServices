using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class SupportingDocumentWrapper : ISupportingDocument
	{
		protected SupportingDocumentWrapper(NctsSupportingDocument document)
		{
			this.document = Argument.NotNull(document, nameof(document));
		}

		protected readonly NctsSupportingDocument document;

		public static SupportingDocumentWrapper New(NctsSupportingDocument document) => document == null ? null : new SupportingDocumentWrapper(document);

		public int? DocumentLineItemNumber => documentLineItemNumber ?? (documentLineItemNumber = document.CSI_ItemNumber);
		int? documentLineItemNumber;

		public virtual string ComplementOfInformation => complementOfInformation ?? (complementOfInformation = document.CSI_ReferenceNumber2);
		string complementOfInformation;

		public virtual string Type => type ?? (type = document.CSI_Code);
		string type;

		public virtual string ReferenceNumber => referenceNumber ?? (referenceNumber = document.CSI_ReferenceNumber);
		string referenceNumber;
	}
}
