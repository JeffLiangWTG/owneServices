using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.AIS.CusTempStorage;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	class TSPresentationProvider : ITSPresentation
	{
		public static TSPresentationProvider New(TemporaryStorageHeader temporaryStorageHeader)
		=> temporaryStorageHeader == null ? null : new TSPresentationProvider(temporaryStorageHeader);

		TSPresentationProvider(TemporaryStorageHeader header)
		{
			this.header = header;
		}
		protected readonly TemporaryStorageHeader header;

		public string PresentationTrader => OrganizationProvider.GetEori(header.Presenter);

		public string FirstEntryCustomsOffice => header.CustomsOfficeOfFirstEntry;

		public IIdType ActiveBorderTransportMeansId
			=> CachedValueHelper.GetValue(ref activeBorderTransportMeansIdCached, () => IdTypeProvider.New(header.AMA_TransportMeans, header.AMA_VesselName));
		CachedValue<IIdType> activeBorderTransportMeansIdCached;
	}
}
