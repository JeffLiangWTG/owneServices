using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class UnloadingDifferencesDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, UnloadingDifferencesDetailsControlBag>
		where T : NctsArrivalMovementHeader
	{
		public override UnloadingDifferencesDetailsControlBag CommonBag => UnloadingDifferencesDetailsControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
