using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NCTSDocumentProvider : INCTSDocument
	{
		public static NCTSDocumentProvider NewOrNull(CusSupportingInfo document) => document != null ? new NCTSDocumentProvider(document) : null;

		NCTSDocumentProvider(CusSupportingInfo document)
		{
			this.document = Argument.NotNull(document, nameof(document));
		}

		public string Type => document.CSI_Code.ValueOrNullIfEmpty();

		public string ReferenceNumber => document.CSI_ReferenceNumber.ValueOrNullIfEmpty();

		public int? DocumentLineItemNumber => document.CSI_ItemNumber;

		public string ComplementOfInformation => document.CSI_ReferenceNumber2.ValueOrNullIfEmpty();

		readonly CusSupportingInfo document;
	}
}
