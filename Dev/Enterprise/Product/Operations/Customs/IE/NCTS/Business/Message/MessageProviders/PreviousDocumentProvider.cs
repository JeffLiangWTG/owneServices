using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class PreviousDocumentProvider : DocumentProvider, IDocumentWithComplementOfInformation
	{
		public PreviousDocumentProvider(PreviousDocument previousDocument) : base(previousDocument)
		{
			this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
		}
		readonly PreviousDocument previousDocument;

		public string ComplementOfInformation => previousDocument.CSI_ReferenceNumber2;
	}
}
