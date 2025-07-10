using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ImportCommonC44CertificateDocumentWrapper : DocumentCommonWrapper, IImportCommonC44CertificateDocument
	{
		public ImportCommonC44CertificateDocumentWrapper(SupportingDocument doc)
			: base(doc.CSI_Code, doc.CSI_ReferenceNumber)
		{
			document = Argument.NotNull(doc, nameof(doc));
		}
		readonly SupportingDocument document;

		public ZString CertQuantityUnit => document.CSI_UnitOfQuantity.ConvertCargoWiseToES(document.Factory);

		public ZDecimal CertQuantityAmount => document.CSI_Quantity;

		public ZDateTime CertDate => !document.CSI_DateOfExpiry.IsEmpty ? document.CSI_DateOfExpiry : document.CSI_DateOfIssue;
	}
}
