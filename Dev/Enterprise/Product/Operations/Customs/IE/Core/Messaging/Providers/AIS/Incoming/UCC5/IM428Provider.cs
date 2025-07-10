using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM428;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class IM428Provider
	{
		public IM428Provider(Im428 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im428 xmlObject;

		public ZString LocalReferenceNumber => xmlObject.Declaration.Lrn25;

		public ZString MovementReferenceNumber => xmlObject.Declaration.Mrn;
		public ZString DeclarationAcceptanceDate => xmlObject.Declaration.AcceptanceDate;

		public IReadOnlyCollection<IM428GoodsItemProviderUCC5> GoodsItems => goodsItems ?? (goodsItems = xmlObject.GoodsShipment.GoodsShipmentItem?.Select(x => new IM428GoodsItemProviderUCC5(x)).ToArray() ?? Array.Empty<IM428GoodsItemProviderUCC5>());
		IM428GoodsItemProviderUCC5[] goodsItems;
	}
}
