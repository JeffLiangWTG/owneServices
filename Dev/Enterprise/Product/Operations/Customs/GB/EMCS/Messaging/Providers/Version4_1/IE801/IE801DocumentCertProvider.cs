using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE801DocumentCertProvider : IEMCSDocumentCert
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
