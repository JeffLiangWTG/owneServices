using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public class ImportSupportingDocumentsFieldsLayoutBuilder<T> : ColumnLayoutBuilder<T, ImportSupportingDocumentsFieldsControlBag> where T : JobDeclaration
	{
		public override ImportSupportingDocumentsFieldsControlBag CommonBag => ImportSupportingDocumentsFieldsControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
