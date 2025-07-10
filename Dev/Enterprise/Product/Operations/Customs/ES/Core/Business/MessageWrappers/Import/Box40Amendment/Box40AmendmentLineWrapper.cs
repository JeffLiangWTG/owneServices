using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class Box40AmendmentLineWrapper : IBox40AmendmentLine
	{
		public Box40AmendmentLineWrapper(CusEntryLine cusEntryLine)
		{
			entryLine = Argument.NotNull(cusEntryLine, nameof(cusEntryLine));
			Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));

			var previousDocuments = entryLine.RandomLine.GetPreviousDocumentsFromSelfOrParentsForMergeKeyOnly();
			Argument.NotNull(previousDocuments, nameof(previousDocuments));
			previousDocumentToSend = previousDocuments[0];
		}

		readonly CusEntryLine entryLine;
		readonly PreviousDocument previousDocumentToSend;

		public ZInt LineNumber => entryLine.CL_LineNumber;

		public ZString PrecedentDocumentType => previousDocumentToSend.CSI_SubType;

		public ZString PrecedentDocumentClass => previousDocumentToSend.CSI_Code;

		public ZString PrecedentDocumentReference => PreviousDocumentHelper.GetSUMReferenceNumberToSend(previousDocumentToSend);
	}
}
