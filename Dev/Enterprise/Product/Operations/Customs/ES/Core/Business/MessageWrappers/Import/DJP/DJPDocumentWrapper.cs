using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DJPDocumentWrapper : DocumentCommonWrapper, IDJPDocument
	{
		public DJPDocumentWrapper(SupportingDocument doc)
			: base(doc.CSI_Code, doc.CSI_ReferenceNumber)
		{
			document = Argument.NotNull(doc, nameof(doc));
		}
		readonly SupportingDocument document;

		public ZDateTime Date => !document.CSI_DateOfExpiry.IsEmpty ? document.CSI_DateOfExpiry : document.CSI_DateOfIssue;

		public ZString Indicator => document.CSI_Procedure;
	}
}
