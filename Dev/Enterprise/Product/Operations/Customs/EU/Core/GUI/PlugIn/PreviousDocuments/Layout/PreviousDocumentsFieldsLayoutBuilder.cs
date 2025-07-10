using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public class PreviousDocumentsFieldsLayoutBuilder<T> : ColumnLayoutBuilder<T, PreviousDocumentsFieldsControlBag>
		where T : Business.Declaration.MultiLineAddInfos.PreviousDocument
	{
		public override PreviousDocumentsFieldsControlBag CommonBag => PreviousDocumentsFieldsControlBag.Instance;
	}
}
