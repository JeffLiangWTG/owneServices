using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CTransportEquipmentProvider
	{
		public CC043CTransportEquipmentProvider(TransportEquipmentType05 transportEquipment)
		{
			transportEquipmentType = Argument.NotNull(transportEquipment, nameof(transportEquipment));
		}
		readonly TransportEquipmentType05 transportEquipmentType;

		public ZShort SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCached, () => ZShort.ParseSafe(transportEquipmentType.SequenceNumber, ZShort.Zero));
		CachedValue<ZShort> sequenceNumberCached;

		public ZString ContainerIdentificationNumber => transportEquipmentType.ContainerIdentificationNumber ?? ZString.Empty;

		public ZShort NumberOfSeals => CachedValueHelper.GetValue(ref numberOfSealsCached, () => ZShort.ParseSafe(transportEquipmentType.NumberOfSeals, (ZShort)Seals.Count));
		CachedValue<ZShort> numberOfSealsCached;

		public IReadOnlyCollection<CC043CSealProvider> Seals => sealsCached ?? (sealsCached = transportEquipmentType.Seal.Select(x => new CC043CSealProvider(x)).ToArray() ?? Array.Empty<CC043CSealProvider>());
		IReadOnlyCollection<CC043CSealProvider> sealsCached;
	}
}
