using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public class Phase5ArrivalSummaryDeclarationLayoutBuilder<T> : ColumnLayoutBuilder<T, Phase5ArrivalSummaryDeclarationControlBag> where T : Business.NctsHeader
	{
		public override Phase5ArrivalSummaryDeclarationControlBag CommonBag
			=> Phase5ArrivalSummaryDeclarationControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
