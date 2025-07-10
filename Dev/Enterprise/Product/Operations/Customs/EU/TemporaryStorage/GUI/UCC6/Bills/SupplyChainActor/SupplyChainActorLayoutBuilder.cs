using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class SupplyChainActorLayoutBuilder : ColumnLayoutBuilder<CusSupplyChainActorReference, SupplyChainActorControlBag>
	{
		public override SupplyChainActorControlBag CommonBag => SupplyChainActorControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
