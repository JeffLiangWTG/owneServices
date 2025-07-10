using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public class SupportingDocumentFieldsLayoutBuilder<T> : ColumnLayoutBuilder<T, SupportingDocumentFieldsControlBag> where T : Business.Declaration.MultiLineAddInfos.SupportingDocument
	{
		public override SupportingDocumentFieldsControlBag CommonBag => SupportingDocumentFieldsControlBag.Instance;
	}
}
