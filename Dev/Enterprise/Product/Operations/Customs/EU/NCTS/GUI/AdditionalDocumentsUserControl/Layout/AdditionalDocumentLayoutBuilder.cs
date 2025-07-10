using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class AdditionalDocumentLayoutBuilder<T> : ColumnLayoutBuilder<T, AdditionalDocumentControlBag> where T : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo
	{
		public override AdditionalDocumentControlBag CommonBag => AdditionalDocumentControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
