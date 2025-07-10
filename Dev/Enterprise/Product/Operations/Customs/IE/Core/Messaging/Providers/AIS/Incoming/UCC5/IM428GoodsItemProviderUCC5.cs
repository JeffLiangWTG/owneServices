using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM428;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class IM428GoodsItemProviderUCC5 : IGoodsItemProvider
	{
		public IM428GoodsItemProviderUCC5(GoodsShipmentTypeItem xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly GoodsShipmentTypeItem xmlObject;

		public ZString DeclarationGoodsItemNumber => xmlObject.GoodsItemNumber16;

		public IReadOnlyCollection<ITaxTypeProvider> TaxTypes => taxTypes ?? (taxTypes = xmlObject.Taxes?.TaxBox43Bis?.Select(x => new UCC5DutiesAndTaxesTypeProvider(x)).ToArray() ?? Array.Empty<UCC5DutiesAndTaxesTypeProvider>());
		UCC5DutiesAndTaxesTypeProvider[] taxTypes;
	}
}
