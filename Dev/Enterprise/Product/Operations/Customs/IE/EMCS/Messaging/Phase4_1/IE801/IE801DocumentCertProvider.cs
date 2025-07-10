using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE801DocumentCertProvider : IEMCSDocumentCert
	{
		public IE801DocumentCertProvider(DocumentCertificateType document)
		{
			this.document = Argument.NotNull(document, nameof(document));
		}

		readonly DocumentCertificateType document;

		public ZString Description => document.DocumentDescription?.Value;

		public ZString Reference => document.DocumentReference;

		public ZString Type => document.DocumentType;
	}
}
