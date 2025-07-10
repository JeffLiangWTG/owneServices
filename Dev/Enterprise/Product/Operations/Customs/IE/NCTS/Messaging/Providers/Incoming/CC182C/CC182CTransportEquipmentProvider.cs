using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC182CTransportEquipmentProvider
	{
		public CC182CTransportEquipmentProvider(TransportEquipmentType07 transportEquipment)
		{
			this.transportEquipment = Argument.NotNull(transportEquipment, nameof(transportEquipment));
		}
		readonly TransportEquipmentType07 transportEquipment;

		public ZString ContainerNumber  => transportEquipment.ContainerIdentificationNumber;

		public ZString NumberofSeals => transportEquipment.NumberOfSeals;

		public IReadOnlyCollection<string> SealsIdentifier  => sealsIdentifier ?? (sealsIdentifier = transportEquipment.Seal?.Select(x => x.Identifier).ToArray());
		IReadOnlyCollection<string> sealsIdentifier;
	}
}
