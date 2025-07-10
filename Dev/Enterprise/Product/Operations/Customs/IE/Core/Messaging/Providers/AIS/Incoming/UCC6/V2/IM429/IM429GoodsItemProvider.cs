using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM429GoodsItemProvider : IGoodsItemProvider
	{
		public IM429GoodsItemProvider(MGoodsShipmentItemType01 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly MGoodsShipmentItemType01 xmlObject;

		public ZString DeclarationGoodsItemNumber => xmlObject.DeclarationGoodsItemNumber;

		public IReadOnlyCollection<ITaxTypeProvider> TaxTypes => taxTypes ?? (taxTypes = xmlObject.Taxes?.TaxBoxbis?.Select(x => new TaxBoxTypeProvider(x)).ToArray() ?? Array.Empty<TaxBoxTypeProvider>());
		TaxBoxTypeProvider[] taxTypes;
	}
}
