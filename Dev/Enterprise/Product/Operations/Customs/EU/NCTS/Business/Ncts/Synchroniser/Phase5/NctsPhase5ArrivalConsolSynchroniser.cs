using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPhase5ArrivalConsolSynchroniser : BusinessObjectSynchroniser
	{
		public NctsPhase5ArrivalConsolSynchroniser(NctsHeader nctsHeader, ICusInBondParent source)
			: base(nctsHeader, (BusinessObject)source)
		{
			Source.OnUpdatedByDataRefreshForCustomsSynchronisation += HookedConsol_OnUpdatedByDataRefreshForCustomsSynchronisation;
		}

		protected new NctsHeader Destination => (NctsHeader)base.Destination;

		protected new ForwardingConsol Source => (ForwardingConsol)base.Source;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			if (!Destination.DefaultTraderAtDestinationManager.IsEnabled())
			{
				Synchronisers.Add(new JobDocAddressAddressOnlySynchroniser(Destination.DestinationTrader, (ZPropertyInfoGuid)Source.JK_OA_ReceivingForwarderAddressInfo));
			}
		}

		void HookedConsol_OnUpdatedByDataRefreshForCustomsSynchronisation(object sender, EventArgs e)
		{
			Source.OnUpdatedByDataRefreshForCustomsSynchronisation -= HookedConsol_OnUpdatedByDataRefreshForCustomsSynchronisation;
		}

		protected override void DisposeCore()
		{
			Source.OnUpdatedByDataRefreshForCustomsSynchronisation -= HookedConsol_OnUpdatedByDataRefreshForCustomsSynchronisation;
			base.DisposeCore();
		}
	}
}
