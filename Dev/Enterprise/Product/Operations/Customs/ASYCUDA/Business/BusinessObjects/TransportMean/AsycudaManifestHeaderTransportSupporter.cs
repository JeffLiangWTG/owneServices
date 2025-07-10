using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Security;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaManifestHeaderTransportSupporter : TransportSupporter<AsycudaManifestHeader>
	{
		public AsycudaManifestHeaderTransportSupporter(AsycudaManifestHeader parent)
			: base(parent) { }

		public override ZGuid ShippingLine
		{
			get
			{
				return Parent.ShippingAgentOrgPK;
			}
			set
			{
				if (!base.Parent.HasManifestBeenSubmittedToCustoms && value.IsValid)
				{
					base.Parent.ShippingAgentOrgPK = value;
				}
			}
		}

		public override ZString Description => Parent.AMA_ManifestDescription;

		public override ZString ConsignmentRef => Parent.AMA_JobReference;

		public override ZString TransportMode => Parent.AMA_TransportMode;

		public override ZString ContainerMode => Parent.AMA_ContainerMode;

		public override ZString BillOfLading => Parent.AMA_MasterBill;

		public override SecurityCheckpoint DistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceCustoms;

		public override bool SupportETD => false;

		public override bool SupportVoyageFlight => false;
	}
}
