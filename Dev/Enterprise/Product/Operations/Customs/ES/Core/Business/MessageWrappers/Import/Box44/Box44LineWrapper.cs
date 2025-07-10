using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class Box44LineWrapper : IBox44Line
	{
		public Box44LineWrapper(CusEntryLine cusEntryLine)
		{
			entryLine = Argument.NotNull(cusEntryLine, nameof(cusEntryLine));
			Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));
		}

		readonly CusEntryLine entryLine;

		public ZInt LineNumber => entryLine.CL_LineNumber;

		public IReadOnlyCollection<IImportCommonC44CertificateDocument> DocumentsAndCertificates
		{
			get
			{
				if (documentsAndCertificates == null)
				{
					var documentsAndCertificatesList = new List<ImportCommonC44CertificateDocumentWrapper>();

					var supDocsInCL = entryLine.GetPreviouslySentSupportingDocuments();

					documentsAndCertificatesList.AddRange(entryLine.SupportingDocuments.Cast<SupportingDocument>()
																						.Where(doc => !doc.MatchesAnyPreviouslySentDocument(supDocsInCL))
																							.Select(doc => new ImportCommonC44CertificateDocumentWrapper(doc)));

					documentsAndCertificatesList.AddRange(entryLine.Header.SupportingDocuments.Cast<SupportingDocument>()
																						.Where(doc => !doc.MatchesAnyPreviouslySentDocument(supDocsInCL))
																							.Select(doc => new ImportCommonC44CertificateDocumentWrapper(doc)));

					documentsAndCertificates = documentsAndCertificatesList.AsReadOnly();
				}
				return documentsAndCertificates;
			}
		}
		IReadOnlyCollection<ImportCommonC44CertificateDocumentWrapper> documentsAndCertificates;
	}
}
