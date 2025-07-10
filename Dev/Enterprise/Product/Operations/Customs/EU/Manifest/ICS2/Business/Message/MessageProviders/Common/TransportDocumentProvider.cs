using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class TransportDocumentProvider : IIdentifierTypePair
	{
		internal TransportDocumentProvider(string documentNumber, string type)
		{
			Identifier = documentNumber;
			Type = type;
		}

		public static TransportDocumentProvider NewOrNull(ZString documentNumber, ZString type) => !documentNumber.IsEmpty ? new TransportDocumentProvider(documentNumber, type) : null;

		public string Identifier { get; }

		public string Type { get; }
	}
}
