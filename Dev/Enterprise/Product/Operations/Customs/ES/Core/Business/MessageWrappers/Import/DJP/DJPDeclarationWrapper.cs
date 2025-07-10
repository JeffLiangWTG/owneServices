using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DJPDeclarationWrapper : IDJPDeclaration
	{
		public DJPDeclarationWrapper(CusEntryHeader cusEntryHeader)
		{
			entryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
			declaration = entryHeader.Declaration;

			Argument.GreaterThan(entryHeader.MergedLines.Count, 0, nameof(entryHeader.MergedLines));
		}

		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;

		public ZString MRN => entryHeader.MovementReferenceNumber;

		public IReadOnlyCollection<IDJPDocument> Documents
		{
			get
			{
				if (documents == null)
				{
					var documentsList = new List<DJPDocumentWrapper>();

					documentsList.AddRange(declaration.SupportingDocuments
														.Cast<SupportingDocument>()
														.Where(doc => !doc.CSI_Procedure.IsEmpty)
														.Select(doc => new DJPDocumentWrapper(doc)));

					documentsList.AddRange(entryHeader.EntryInstruction.SupportingDocuments
																		.Cast<SupportingDocument>()
																		.Where(doc => !doc.CSI_Procedure.IsEmpty)
																		.Select(doc => new DJPDocumentWrapper(doc)));
					documents = documentsList.AsReadOnly();
				}
				return documents;
			}
		}
		IReadOnlyCollection<DJPDocumentWrapper> documents;

		public IReadOnlyCollection<IDJPLine> Lines
		{
			get
			{
				if (lines == null)
				{
					lines = entryHeader.MergedLines
						.Cast<CusEntryLine>()
						.Select(entryLine => new DJPLineWrapper(entryLine))
						.ToList().AsReadOnly();
				}
				return lines;
			}
		}
		IReadOnlyCollection<DJPLineWrapper> lines;
	}
}
