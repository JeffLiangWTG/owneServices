using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;

namespace Enterprise.Customs.IE.ExitControl.Business.AES
{
	public class TransportEquipmentsProvider : ITransportEquipmentWithSeals
	{
		public TransportEquipmentsProvider(CusExitContainer container, IEnumerable<string> goodsReferences)
		{
			this.container = container;
			this.GoodsReferences = goodsReferences.ToArray();
		}
		readonly CusExitContainer container;

		public IReadOnlyCollection<string> Seals => seals ?? (seals = container.AllSealNumbers.Select(s => s.BK_SealNumber.ToString()).ToArray());
		IReadOnlyCollection<string> seals;

		public string ContainerIdentificationNumber => container.CXN_IsEquipment ? null : container.CXN_ContainerNumber;

		public IReadOnlyCollection<string> GoodsReferences { get; }

		public bool ContainerIsFull => false;
	}
}
