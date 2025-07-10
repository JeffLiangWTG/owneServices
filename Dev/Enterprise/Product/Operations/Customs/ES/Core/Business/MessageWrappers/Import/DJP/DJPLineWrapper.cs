using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DJPLineWrapper : IDJPLine
	{
		public DJPLineWrapper(CusEntryLine cusEntryLine)
		{
			entryLine = Argument.NotNull(cusEntryLine, nameof(cusEntryLine));
			Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));
		}

		readonly CusEntryLine entryLine;

		public ZInt LineNumber => entryLine.CL_LineNumber;

		public IReadOnlyCollection<IDJPDocument> Documents
		{
			get
			{
				if (documents == null)
				{
					documents = entryLine.SupportingDocuments
						.Cast<SupportingDocument>()
						.Where(doc => !doc.CSI_Procedure.IsEmpty)
						.Select(doc => new DJPDocumentWrapper(doc))
						.ToList().AsReadOnly();
				}
				return documents;
			}
		}
		IReadOnlyCollection<DJPDocumentWrapper> documents;
	}
}
