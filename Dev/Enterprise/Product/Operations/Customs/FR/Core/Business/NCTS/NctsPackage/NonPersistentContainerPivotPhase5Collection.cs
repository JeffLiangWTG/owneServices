using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.Business.NCTS;

public class NonPersistentContainerPivotPhase5Collection : EU.NCTS.Business.NonPersistentContainerPivotPhase5Collection
{
	public NonPersistentContainerPivotPhase5Collection(EU.NCTS.Business.NctsPackage package) : base(package)
	{
	}

	protected override void AddNewNonPersistentContainer(NctsCusInBondContainer container)
	{
		Add(new NonPersistentContainerPivotPhase5((NctsPackage)package, container));
	}
}
