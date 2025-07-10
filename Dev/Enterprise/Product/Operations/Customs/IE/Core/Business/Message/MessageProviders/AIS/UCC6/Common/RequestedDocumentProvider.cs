using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class RequestedDocumentProvider : IRequestedDocument
	{
		readonly AdditionalInfoSendingObject addInfo;
		readonly IEnumerable<IeDoc> documents;

		public RequestedDocumentProvider(AdditionalInfoSendingObject addInfoSendingObject, IEnumerable<IeDoc> documents)
		{
			this.addInfo = Argument.NotNull(addInfoSendingObject, nameof(addInfoSendingObject));
			this.documents = Argument.NotNull(documents, nameof(documents));
		}
		public string Type => addInfo.DocumentType;

		public string Qualifier => addInfo.CCQualifier;

		public string Reference => addInfo.ReferenceNumber;

		public string Description => addInfo.DocumentInformation;

		public IReadOnlyCollection<IDocumentImage> DocumentsImage => documentsImage ?? (documentsImage = documents.Select(d => new DocumentImageProvider(d)).ToArray());
		IReadOnlyCollection<IDocumentImage> documentsImage;
	}
}
