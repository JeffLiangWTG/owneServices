using System.Globalization;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class PreviousDocumentProvider : ISimplifiedDeclarationDocumentWritingOff
	{
		public static PreviousDocumentProvider New(PreviousDocument previousDocument) =>
			new PreviousDocumentProvider(previousDocument.CSI_Code, previousDocument.CSI_ReferenceNumber, previousDocument.CSI_LineNo);

		PreviousDocumentProvider(string type, string reference, int lineID)
		{
			PreviousDocumentType = type;
			PreviousDocumentIdentifier = reference;
			PreviousDocumentLineId = lineID.ToString(CultureInfo.InvariantCulture);
		}

		public string PreviousDocumentType { get; }

		public string PreviousDocumentIdentifier { get; }

		public string PreviousDocumentLineId { get; }
	}
}
