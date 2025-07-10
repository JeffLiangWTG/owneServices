using CargoWise.Types;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRDepotContainerOutturnSynchroniser : CMRDepotOutturnSynchroniser
	{
		public CMRDepotContainerOutturnSynchroniser(CusOutturn destination, CFSContainer source)
			: base(destination, source)
		{
			this.Container = source;
		}

		public readonly CFSContainer Container;

		protected override void HookSynchronisers()
		{
			if (Container.DestinationCFSArrival != null)
			{
				Customs.Business.FieldSynchroniser arrivalTimeSynchroniser = new Customs.Business.FieldSynchroniser(Outturn.C5_PackagesOutturnedInfo, Container.DestinationCFSArrival.EU_PickupDeliveryTimeInfo);
				arrivalTimeSynchroniser.Format += ArrivalTimeSynchroniser_Format;
				Synchronisers.Add(arrivalTimeSynchroniser);
			}
		}

		void ArrivalTimeSynchroniser_Format(object sender, Customs.Business.FieldSynchroniser.ConvertEventArgs e)
		{
			e.Value = new ZInt(1);
		}
	}
}
