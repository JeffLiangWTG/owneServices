using Enterprise.Customs.AE.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AE.GUI;

public sealed class ShipmentDetailsLayoutBuilder : ShipmentDetailsLayoutBuilder<JobDeclaration>
{
	protected override int MaxColumns => 2;
}
