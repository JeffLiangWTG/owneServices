using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IE.Business.AES
{
	public class PreviousDocumentProvider : IDocument
	{
		readonly PreviousDocument previousDocument;

		public PreviousDocumentProvider(PreviousDocument previousDocument)
		{
			this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
		}

		public string Type => previousDocument.CSI_Code;

		public string Reference => previousDocument.CSI_ReferenceNumber;
	}
}
