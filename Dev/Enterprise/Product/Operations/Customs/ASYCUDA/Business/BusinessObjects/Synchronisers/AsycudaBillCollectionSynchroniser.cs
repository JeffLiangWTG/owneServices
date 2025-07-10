using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaBillCollectionSynchroniser : Customs.Business.ManifestBillCollectionSynchroniser<AsycudaManifestHeader, AsycudaBill>
	{
		public AsycudaBillCollectionSynchroniser(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override Customs.Business.ConsolDataCalculator ConsolDataCalculator
		{
			get { return new ConsolDataCalculator(Source, ""); }
		}

		protected override Customs.Business.ManifestBillSynchroniser<AsycudaBill> GetNewManifestBillSynchroniser(AsycudaBill destination, ForwardingShipment source)
		{
			return new AsycudaBillSynchroniser(destination, source);
		}

		protected override ZString GetBillNumber(ForwardingShipment shipment)
		{
			return base.GetBillNumber(shipment).Right(AsycudaBillSchema.ABL_BillNumber.MaxLength);  // To help base work out whether we need new, or can reuse existing, bills for a shipment
		}

		protected override bool AreAdditionalKeysMatching(ForwardingShipment shipment, AsycudaBill bill)
		{
			return true;
		}
	}
}
