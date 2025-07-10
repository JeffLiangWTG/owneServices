using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPhase5DeparturePackageContainerPivotSynchroniser : BusinessObjectSynchroniser
	{
		public NctsPhase5DeparturePackageContainerPivotSynchroniser(NctsCusInBondContainerPackageGenPivot destination, ForwardingContainer source)
			: base(destination, source)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.XX_Relation2IDInfo, Destination.XX_Relation2IDInfo));
		}

		protected internal new NctsCusInBondContainerPackageGenPivot Destination => (NctsCusInBondContainerPackageGenPivot)base.Destination;
		protected internal new ForwardingContainer Source => (ForwardingContainer)base.Source;
	}
}
