using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface ICusSupplyChainActorReferenceCollection<out TCusSupplyChainActorReference>
		: IBusinessObjectCollection<TCusSupplyChainActorReference> where TCusSupplyChainActorReference : CusSupplyChainActorReference
	{
		new TCusSupplyChainActorReference this[int index] { get; }
	}
}
