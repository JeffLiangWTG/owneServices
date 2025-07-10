using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPhase5ArrivalShipmentSynchroniser : BusinessObjectSynchroniser
	{
		public NctsPhase5ArrivalShipmentSynchroniser(NctsHeader nctsHeader, ICusInBondParent source)
			: base(nctsHeader, (BusinessObject)source)
		{
			Source.OnJobDeclarationCreated += Shipment_OnJobDeclarationCreated;
		}

		protected new NctsHeader Destination => (NctsHeader)base.Destination;

		protected new ForwardingShipment Source => (ForwardingShipment)base.Source;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			if (Source.JobHeader is JobHeader jobHeader && jobHeader.LocalChargesAddr != null && !Destination.DefaultTraderAtDestinationManager.IsEnabled())
			{
				Synchronisers.Add(new JobDocAddressAddressOnlySynchroniser(Destination.DestinationTrader, (ZPropertyInfoGuid)jobHeader.JH_OA_LocalChargesAddrInfo));
			}
		}

		void Shipment_OnJobDeclarationCreated(object sender, JobDeclarationCreationEventArgs e)
		{
			Source.OnJobDeclarationCreated -= Shipment_OnJobDeclarationCreated;
		}

		protected override void DisposeCore()
		{
			Source.OnJobDeclarationCreated -= Shipment_OnJobDeclarationCreated;
			base.DisposeCore();
		}
	}
}
