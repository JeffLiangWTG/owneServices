using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class InvoiceLineCopyDocumentLayoutBuilder : ColumnLayoutBuilder<CopyDocumentsSelectionHeader,
		InvoiceLineCopyDocumentControlBag>
	{
		public override InvoiceLineCopyDocumentControlBag CommonBag => InvoiceLineCopyDocumentControlBag.Instance;
	}
}

