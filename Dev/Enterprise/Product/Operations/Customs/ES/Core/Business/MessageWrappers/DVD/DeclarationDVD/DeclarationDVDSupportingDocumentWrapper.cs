using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DeclarationDVDSupportingDocumentWrapper : DeclarationDVDEUAndNationalCodesWrapper, IDeclarationDVDSupportingDocument
	{
		public DeclarationDVDSupportingDocumentWrapper(SupportingDocument doc) : base(doc.CSI_Code)
		{
			document = Argument.NotNull(doc, nameof(doc));
		}
		protected readonly SupportingDocument document;

		public ZString Number => document.CSI_ReferenceNumber;

		public ZDateTime DocumentDate => document.CSI_DateOfExpiry.IsEmpty ? document.CSI_DateOfIssue : document.CSI_DateOfExpiry;
	}
}
