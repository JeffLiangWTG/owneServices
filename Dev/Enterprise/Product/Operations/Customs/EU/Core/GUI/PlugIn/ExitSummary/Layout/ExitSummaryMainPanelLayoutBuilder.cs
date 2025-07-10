using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public class ExitSummaryMainPanelLayoutBuilder<T> : ColumnLayoutBuilder<T, ExitSummaryMainPanelControlBag> where T : CusExitControlHeader
	{
		public override ExitSummaryMainPanelControlBag CommonBag { get; } = ExitSummaryMainPanelControlBag.Instance;

		protected override int MaxColumns => 4;

		public override bool NarrowColumnForMediumControls => true;
	}
}
