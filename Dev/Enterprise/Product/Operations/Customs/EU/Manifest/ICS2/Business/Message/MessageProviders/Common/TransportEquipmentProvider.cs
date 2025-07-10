using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class TransportEquipmentProvider : ITransportEquipment
	{
		TransportEquipmentProvider(AsycudaContainer container) => this.container = container;

		readonly AsycudaContainer container;

		public static TransportEquipmentProvider NewOrNull(AsycudaContainer container) => container != null ? new TransportEquipmentProvider(container) : null;

		public string ContainerSizeAndType => container.ContainerType?.RC_ISOEquipmentSizeTypeCode;

		public string ContainerPackedStatus => container.ACN_EmptyFullIndicator == ASYCUDA.Business.EmptyFullIndicatorList.Codes.EmptyContainer ? EmptyFullIndicatorList.Codes.Empty : EmptyFullIndicatorList.Codes.NotEmpty;

		public string ContainerSupplierType => container.ACN_IsShipperOwned ? "1" : "2";

		public string ContainerIdentificationNumber => container.ACN_ContainerNumber;

		public decimal NumberOfSealsValue => SealIdentifiers.Count;

		public IReadOnlyCollection<string> SealIdentifiers => sealIdentifiers ??= new string[] { container.ACN_Seal1, container.ACN_Seal2, container.ACN_Seal3 }.Where(s => !string.IsNullOrEmpty(s)).ToArray();
		IReadOnlyCollection<string> sealIdentifiers;
	}
}
