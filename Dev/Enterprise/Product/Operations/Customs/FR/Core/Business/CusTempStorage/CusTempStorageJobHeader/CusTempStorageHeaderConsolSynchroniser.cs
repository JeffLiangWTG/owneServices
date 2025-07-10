using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageHeaderConsolSynchroniser : BusinessObjectSynchroniser
	{
		public CusTempStorageHeaderConsolSynchroniser(CusTempStorageJobHeader header)
			: base(header, header.RelatedBusinessObject)
		{
		}

		protected new CusTempStorageJobHeader Destination
		{
			get { return (CusTempStorageJobHeader)base.Destination; }
		}

		protected new ForwardingConsol Source
		{
			get { return (ForwardingConsol)base.Source; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.ReferenceNumberInfo, Source.JK_UniqueConsignRefInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.TransportModeInfo, GetTransportModeInfos()));
			Synchronisers.Add(new FieldSynchroniser(Destination.TransportRegNoInfo, GetJW_VoyageFlightForBindingInfo()));
			Synchronisers.Add(new FieldSynchroniser(Destination.NKLoadingInfo, GetJW_RL_NKLoadPortForBindingInfo()));
		}

		ZPropertyInfo GetTransportModeInfos()
		{
			return Source.JK_TransportModeInfo;
		}

		ZPropertyInfo GetJW_VoyageFlightForBindingInfo()
		{
			return GetMostInterestingTransport().JW_VoyageFlightInfo;
		}

		ZPropertyInfo GetJW_RL_NKLoadPortForBindingInfo()
		{
			return GetMostInterestingTransport().JW_RL_NKLoadPortInfo;
		}

		Transport GetMostInterestingTransport()
		{
			var transportMode = Source.Transports.MostInterestingTransport;
			return transportMode;
		}
	}
}
