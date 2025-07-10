using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;
using TemporaryStorageContainer = Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageContainer;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	class TransportEquipmentProvider : ITransportEquipment
	{
		public static TransportEquipmentProvider New(TemporaryStorageContainer container) => new TransportEquipmentProvider(container);

		readonly TemporaryStorageContainer container;
		TransportEquipmentProvider(TemporaryStorageContainer container)
		{
			this.container = Argument.NotNull(container, nameof(container));
		}

		public string ContainerPackedStatus => container.ACN_EmptyFullIndicator;

		public string ContainerId => container.ACN_ContainerNumber;

		public string NumberOfSeals => (Seal?.Count ?? ZInt.Zero).ToString(CultureInfo.InvariantCulture);

		IReadOnlyCollection<string> sealsCached;
		public IReadOnlyCollection<string> Seal => sealsCached ?? (sealsCached = container.GetSeals());

		public IReadOnlyCollection<string> GoodsReference => Array.Empty<string>();
	}
}
