namespace Enterprise.Customs.CA.Business
{
	using CargoWise.Types;
	using Enterprise.Freight.Business;
	using Enterprise.Freight.Forwarding.Business;

	static class PackLineExtensions
	{
		public static ZString ContainerNumberForConsol(this PackLine packLine, CommonConsol consol)
		{
			return packLine?.GetContainer(consol)?.JC_ContainerNum ?? ZString.Empty;
		}

		public static (ZInt Qty, ZString UQ) GetEffectivePackLineQuantity(this PackLine packLine)
		{
			var qty = ZInt.Zero;
			var uq = ZString.Empty;
			if (packLine != null)
			{
				qty = packLine.JL_PackageCount;
				uq = packLine.JL_F3_NKPackType;

				if (qty.IsEmpty)
				{
					if (packLine?.Shipment is ForwardingShipment shipment
					&& shipment.JS_TotalPackageCount is ZInt totalPackageCount
					&& totalPackageCount > 0
					&& shipment.OuterPackLines.Count == 1
					&& (packLine.IsOuterPackType || shipment.InnerPackLines.Count == 1))
					{
						qty = totalPackageCount;
						uq = shipment.JS_F3_NKTotalCountPackType;
					}
				}
			}

			return (qty, uq);
		}
	}
}
