using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class FinalPriceExtensionRequestNewLayoutsBuilder : ColumnLayoutBuilder<FinalPriceReportByDateExtensionHeader, FinalPriceExtensionRequestNewControlBag>
	{
		public override FinalPriceExtensionRequestNewControlBag CommonBag => FinalPriceExtensionRequestNewControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
