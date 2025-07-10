using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM404GoodsItemProvider : IGoodsItemProvider
	{
		public IM404GoodsItemProvider(GoodsShipmentItemIm404Type xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly GoodsShipmentItemIm404Type xmlObject;

		public ZString DeclarationGoodsItemNumber => xmlObject.DeclarationGoodsItemNumber;

		public IReadOnlyCollection<ITaxTypeProvider> TaxTypes => taxTypes ?? (taxTypes = xmlObject.CalculationOfTaxes?.DutiesAndTaxes?.Select(x => new DutiesAndTaxesTypeProvider(x)).ToArray() ?? Array.Empty<DutiesAndTaxesTypeProvider>());

		DutiesAndTaxesTypeProvider[] taxTypes;
	}
}
