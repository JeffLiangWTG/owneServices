using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public class InvoiceLineExportSupportingDocumentsFieldsLayoutBuilder<T> : ColumnLayoutBuilder<T, InvoiceLineExportSupportingDocumentsFieldsControlBag> where T : JobDeclaration
	{
		public override InvoiceLineExportSupportingDocumentsFieldsControlBag CommonBag => InvoiceLineExportSupportingDocumentsFieldsControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
