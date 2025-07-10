using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public class ExportSupportingDocumentsFieldsLayoutBuilder<T> : ColumnLayoutBuilder<T, ExportSupportingDocumentsFieldsControlBag> where T : JobDeclaration
	{
		public override ExportSupportingDocumentsFieldsControlBag CommonBag => ExportSupportingDocumentsFieldsControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
