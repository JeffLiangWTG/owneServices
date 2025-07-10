using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class SupplyChainActorLayoutBuilder<T> : ColumnLayoutBuilder<T, SupplyChainActorControlBag> where T : CusSupplyChainActorReference
	{
		public override SupplyChainActorControlBag CommonBag => SupplyChainActorControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
