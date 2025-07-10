using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class HouseConsignmentDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, HouseConsignmentDetailsControlBag> where T : Business.NctsBill
	{
		public override HouseConsignmentDetailsControlBag CommonBag => HouseConsignmentDetailsControlBag.Instance;

		protected override int MaxColumns => 3;

		public override bool NarrowColumnForMediumControls => true;
	}
}
