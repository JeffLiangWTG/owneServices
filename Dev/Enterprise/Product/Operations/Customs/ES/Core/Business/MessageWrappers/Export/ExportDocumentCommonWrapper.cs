using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ExportDocumentCommonWrapper : DocumentCommonWrapper, IExportDocumentCommon
	{
		public ExportDocumentCommonWrapper(SupportingDocument doc)
			: base(doc.CSI_Code, doc.CSI_ReferenceNumber)
		{
			document = doc;
		}
		protected readonly SupportingDocument document;

		public ZDateTime DateOfIssue => document.CSI_DateOfIssue;

		public ZDateTime DateOfExpiry => document.CSI_DateOfExpiry;
	}
}
