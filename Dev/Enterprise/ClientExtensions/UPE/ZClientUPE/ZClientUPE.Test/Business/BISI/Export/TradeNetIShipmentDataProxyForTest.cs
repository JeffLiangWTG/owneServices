using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.Asycuda;
using Enterprise.Customs.SG.V4.Business;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	sealed class TradeNetIShipmentDataProxyForTest : TradeNetIShipmentDataProxy, IShipmentData
	{
		public TradeNetIShipmentDataProxyForTest(ZString customsStatus, ZDateTime entryDate, JobDeclaration declaration) : base(customsStatus, entryDate, declaration)
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
