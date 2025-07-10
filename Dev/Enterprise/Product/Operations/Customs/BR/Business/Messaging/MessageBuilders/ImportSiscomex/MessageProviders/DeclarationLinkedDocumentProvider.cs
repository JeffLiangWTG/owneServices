using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationLinkedDocumentProvider : IDeclarationLinkedDocument
	{
		public DeclarationLinkedDocumentProvider(PreviousDocument previousDocument)
		{
			this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
		}

		readonly PreviousDocument previousDocument;

		public static DeclarationLinkedDocumentProvider New(PreviousDocument previousDocument) => previousDocument == null ? null : new DeclarationLinkedDocumentProvider(previousDocument);

		public string ReferenceTypeCode => ImportSiscomexPreviousDocumentList.MapToCustomsCode(previousDocument.CSI_Code);

		public string ReferenceNumber => previousDocument.CSI_ReferenceNumber;

		public override bool Equals(object obj)
		{
			var result = false;
			if (obj is DeclarationLinkedDocumentProvider other && other != null)
			{
				result = other.GetType() == GetType()
					&& other.ReferenceTypeCode == ReferenceTypeCode
					&& other.ReferenceNumber == ReferenceNumber;
			}
			return result;
		}

		public override int GetHashCode()
		{
			return ReferenceTypeCode.GetHashCode() ^ ReferenceNumber.GetHashCode();
		}
	}
}


