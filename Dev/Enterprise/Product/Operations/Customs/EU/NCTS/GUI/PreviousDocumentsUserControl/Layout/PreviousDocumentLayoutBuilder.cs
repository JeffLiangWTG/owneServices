using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class PreviousDocumentLayoutBuilder<T> : ColumnLayoutBuilder<T, PreviousDocumentControlBag> where T : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument
	{
		public override PreviousDocumentControlBag CommonBag => PreviousDocumentControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
