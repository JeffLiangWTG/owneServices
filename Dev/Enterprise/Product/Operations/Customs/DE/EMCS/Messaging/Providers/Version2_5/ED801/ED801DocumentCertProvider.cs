using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED801DocumentCertProvider : IEMCSDocumentCert
	{
		public ED801DocumentCertProvider(ED801EBodyEadContainerDocumentCertificate document)
		{
			this.document = Argument.NotNull(document, nameof(document));
		}

		readonly ED801EBodyEadContainerDocumentCertificate document;

		public ZString Description => document.DocumentDescription;

		public ZString Reference => document.DocumentReference;

		public ZString Type => document.DocumentType;
	}
}
