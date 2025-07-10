using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM428GoodsItemProvider : IGoodsItemProvider
	{
		public IM428GoodsItemProvider(Im428GoodsShipmentItemType xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im428GoodsShipmentItemType xmlObject;

		public ZString DeclarationGoodsItemNumber => xmlObject.DeclarationGoodsItemNumber;

		public IReadOnlyCollection<ITaxTypeProvider> TaxTypes => taxTypes ?? (taxTypes = xmlObject.CalculationOfTaxes?.DutiesAndTaxes?.Select(x => new DutiesAndTaxesTypeProvider(x)).ToArray() ?? Array.Empty<DutiesAndTaxesTypeProvider>());
		DutiesAndTaxesTypeProvider[] taxTypes;
	}
}
