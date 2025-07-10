using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class CustomsDetailsLayoutBuilder : ColumnLayoutBuilder<JobComInvoiceHeader, CustomsDetailsControlBag>
	{
		public override CustomsDetailsControlBag CommonBag { get; } = CustomsDetailsControlBag.Instance;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int MaxColumns => 2;
	}
}
