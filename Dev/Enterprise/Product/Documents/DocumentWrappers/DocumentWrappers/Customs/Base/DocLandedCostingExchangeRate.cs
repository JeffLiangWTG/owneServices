
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class DocLandedCostingExchangeRate : NonPersistentBusinessObject
	{
		public DocLandedCostingExchangeRate(BusinessObjectFactory factory, ZString currencyCode, ZDecimal sellRate)
			: base(factory)
		{
			this.fCurrencyCode = currencyCode;
			this.fSellRate = sellRate;
		}

		public ZString CurrencyCode
		{
			get { return fCurrencyCode; }
		}
		readonly ZString fCurrencyCode;

		public ZDecimal SellRate
		{
			get { return fSellRate; }
		}
		readonly ZDecimal fSellRate;
	}
}
