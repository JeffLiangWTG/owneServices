using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class UnipassCertificateLayoutBuilder : ColumnLayoutBuilder<GlbCompanyWrapper, UnipassCertificateControlBag>
	{
		public override UnipassCertificateControlBag CommonBag => UnipassCertificateControlBag.Instance;
		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
