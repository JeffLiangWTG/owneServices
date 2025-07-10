using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class IM429GoodsItemProvider : IGoodsItemProvider
	{
		public IM429GoodsItemProvider(GoodsShipmentItemType xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly GoodsShipmentItemType xmlObject;

		public ZString DeclarationGoodsItemNumber => xmlObject.GoodsItemNumber16;

		public IReadOnlyCollection<ITaxTypeProvider> TaxTypes => taxTypes ??= xmlObject.Taxes?.TaxBox43Bis?.Select(x => new IM429TaxBoxTypeProvider(x)).ToArray() ?? Array.Empty<IM429TaxBoxTypeProvider>();
		IM429TaxBoxTypeProvider[] taxTypes;
	}
}
