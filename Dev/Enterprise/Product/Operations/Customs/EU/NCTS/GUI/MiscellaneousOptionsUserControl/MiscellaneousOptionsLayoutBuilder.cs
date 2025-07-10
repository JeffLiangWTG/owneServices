using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class MiscellaneousOptionsLayoutBuilder<T> : ColumnLayoutBuilder<T, MiscellaneousOptionsControlBag> where T : Business.NctsHeader
	{
		public override MiscellaneousOptionsControlBag CommonBag => MiscellaneousOptionsControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
