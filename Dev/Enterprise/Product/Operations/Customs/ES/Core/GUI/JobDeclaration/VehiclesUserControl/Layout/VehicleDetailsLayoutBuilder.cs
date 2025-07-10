using Enterprise.Customs.ES.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

class VehicleDetailsLayoutBuilder : ColumnLayoutBuilder<CusVehicle, VehicleDetailsControlBag>
{
	public override VehicleDetailsControlBag CommonBag => VehicleDetailsControlBag.Instance;

	protected override int MaxColumns => 1;
}
