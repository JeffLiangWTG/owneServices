using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public class PreviousDocumentsFieldsLayoutBuilder : ColumnLayoutBuilder<PreviousDocument, PreviousDocumentsFieldsControlBag>
	{
		public override PreviousDocumentsFieldsControlBag CommonBag => PreviousDocumentsFieldsControlBag.Instance;
	}
}
