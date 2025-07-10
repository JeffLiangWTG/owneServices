using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.CusTempStorage;
using ArrivalTransportMeans = Enterprise.Customs.EU.Business.CusTempStorage.ArrivalTransportMeans;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	class TS313AndTS315GoodsShipmentTypeTransportInformationProvider : ITS313AndTS315GoodsShipmentTypeTransportInformation, IMeansIdentityAtBorderMandatory
	{
		internal protected TS313AndTS315GoodsShipmentTypeTransportInformationProvider(TemporaryStorageHeader header, ArrivalTransportMeans arrivalTransportMeans)
		{
			this.header = header;
			this.arrivalTransportMeans = arrivalTransportMeans;
		}
		protected readonly TemporaryStorageHeader header;
		protected readonly ArrivalTransportMeans arrivalTransportMeans;

		public IMeansIdentityAtBorderMandatory ActiveBorderTransportMeans => this;

		public string Type => arrivalTransportMeans.TPM_TypeOfIdentification;

		public string Number => arrivalTransportMeans.TPM_IdentificationNumber;

		public IReadOnlyCollection<string> ContainerIdentificationNumber { get; private set; }
		internal void SetContainerIDs(IReadOnlyCollection<string> input) => ContainerIdentificationNumber = input;

		public ISeal Seal => CachedValueHelper.GetValue(ref sealCached, () => new SealProvider(header));
		CachedValue<ISeal> sealCached;
	}
}
