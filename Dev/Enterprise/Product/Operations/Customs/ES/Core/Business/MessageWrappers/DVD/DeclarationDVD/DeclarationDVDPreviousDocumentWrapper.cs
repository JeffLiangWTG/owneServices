using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DeclarationDVDPreviousDocumentWrapper : DocumentCommonWrapper, IDeclarationDVDPreviousDocument
	{
		public DeclarationDVDPreviousDocumentWrapper(PreviousDocument doc) : base(doc.CSI_Code, doc.CSI_ReferenceNumber)
		{
			document = Argument.NotNull(doc, nameof(doc));
		}
		readonly PreviousDocument document;

		public ZString LineNumber => document.CSI_LineNo.IsEmpty ? string.Empty : document.CSI_LineNo.ToString();

		public ZString UnitOfMeasure => document.CSI_UnitOfQuantity;

		public ZDecimal Quantity => document.CSI_Quantity;
	}
}
