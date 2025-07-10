using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class IM429Provider
	{
		public IM429Provider(Im429 xmlObject)
		{
			declaration = Argument.NotNull(xmlObject.Declaration, nameof(xmlObject.Declaration));
			goodsShipment = Argument.NotNull(xmlObject.GoodsShipment, nameof(xmlObject.GoodsShipment));
		}
		readonly DeclarationType declaration;
		readonly GoodsShipmentType goodsShipment;

		public ZString LocalReferenceNumber => declaration.Lrn25;

		public ZString MovementReferenceNumber => declaration.Mrn;

		public ZString DeclarationType => declaration.DeclarationType11;

		public ZString AdditionalDeclarationType => declaration.AdditionalDeclarationType12;

		public ZDateTime ResponseDateLimit => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(declaration.ResponseDateLimit);

		public ZString PreferredPaymentMethod => declaration.PreferredPaymentMethod48;

		public ZString Remarks => declaration.Remarks;

		public IReadOnlyCollection<IM429GoodsItemProvider> GoodsItems => goodsItems ?? (goodsItems = goodsShipment.GoodsShipmentItem?.Select(x => new IM429GoodsItemProvider(x)).ToArray() ?? Array.Empty<IM429GoodsItemProvider>());
		IM429GoodsItemProvider[] goodsItems;

		public ZDateTime AcceptanceDate => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(goodsShipment.DatesPlaces?.AcceptanceDate531);
	}
}
