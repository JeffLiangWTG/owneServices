using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class SupportingDocumentLayoutBuilder<T> : ColumnLayoutBuilder<T, SupportingDocumentControlBag> where T : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument
	{
		public override SupportingDocumentControlBag CommonBag => SupportingDocumentControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
