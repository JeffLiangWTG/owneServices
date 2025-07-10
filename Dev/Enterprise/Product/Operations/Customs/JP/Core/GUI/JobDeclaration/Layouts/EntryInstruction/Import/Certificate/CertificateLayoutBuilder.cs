using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class CertificateLayoutBuilder : ColumnLayoutBuilder<CusEntryInstruction, CertificateControlBag>
	{
		public override CertificateControlBag CommonBag => CertificateControlBag.Instance;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long + 80;
	}
}
