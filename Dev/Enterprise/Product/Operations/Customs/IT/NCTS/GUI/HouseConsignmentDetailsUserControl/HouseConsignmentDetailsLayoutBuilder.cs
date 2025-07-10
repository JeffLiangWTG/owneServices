using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public class HouseConsignmentDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, HouseConsignmentDetailsControlBag> where T : Business.NctsBill
{
	public override HouseConsignmentDetailsControlBag CommonBag => HouseConsignmentDetailsControlBag.Instance;

	protected override int MaxColumns => 3;
}
