using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class HouseConsignmentDifferencesLayoutBuilder<T> : ColumnLayoutBuilder<T, HouseConsignmentDifferencesControlBag>
		where T : NctsBill
	{
		public override HouseConsignmentDifferencesControlBag CommonBag => HouseConsignmentDifferencesControlBag.Instance;

		protected override int MaxColumns => 3;
	}
}
