using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class EntryHeaderSupportingDocument : NonPersistentBusinessObject, ICusSupportingDocument
	{
		public EntryHeaderSupportingDocument(IEnumerable<CusSupportingDocument> supportingDocuments)
		{
			documents = Argument.NotNull(supportingDocuments, nameof(supportingDocuments));
			randomDocument = Argument.NotNull(supportingDocuments.FirstOrDefault(), nameof(randomDocument), "supportingDocuments should contains at least one element");
		}
		readonly IEnumerable<CusSupportingDocument> documents;
		readonly CusSupportingDocument randomDocument;

		public ZString DocumentType => randomDocument.DisplayCode;

		public ZString DocumentTypeDesc => randomDocument.RefCusCode?.ZZD_Description ?? ZString.Empty;

		public ZString DocumentNumber => (IsCertificateOfOrigin ? $"<{randomDocument.Parent.TradeAgreementCode}>" : string.Empty)
			+ (IsCertificateOfOriginX ? ZString.Empty : randomDocument.CSI_SubType)
			+ randomDocument.CSI_ReferenceNumber;

		public bool IsCertificateOfOrigin => randomDocument.IsCertificateOfOrigin;
		bool IsCertificateOfOriginX => randomDocument.IsCertificateOfOriginX;

		public IEnumerable<ZInt> ItemNumbers
		{
			get
			{
				if (fItemNumbers == null)
				{
					fItemNumbers = documents.Select(doc => doc.CSI_LineNo).Distinct().Where(no => !no.IsEmpty).OrderBy(n => n).ToList();
				}
				return fItemNumbers;
			}
		}
		List<ZInt> fItemNumbers;
	}
}
