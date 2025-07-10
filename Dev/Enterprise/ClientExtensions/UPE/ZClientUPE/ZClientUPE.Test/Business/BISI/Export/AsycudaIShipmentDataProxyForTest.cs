using System.Collections.Generic;
using Enterprise.Client.UPE.Business.Asycuda;
using Enterprise.Customs.SG.Access.Business;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	sealed class AsycudaIShipmentDataProxyForTest : AsycudaIShipmentDataProxy, IShipmentData
	{
		public AsycudaIShipmentDataProxyForTest(AsycudaBill bill) : base(bill)
		{
		}

		public IReadOnlyList<ShipmentChargeData> ChargesData
		{
			get => chargesData;

			set => chargesData = value;
		}

		IReadOnlyList<ShipmentChargeData> chargesData;
	}
}
