using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IWhsInventoryDutyAndTaxCalculator
		{
			BusinessObjectFactory Factory { get; }
			void Calculate(Dictionary<ZString, IZType> data, ZDecimal ratio, ZDateTime arrivalDate, ZDateTime valuationDate, ZString tariff, ZString countryOfOrigin, ZDecimal customsValue, ZDecimal customsQty1, ZString customsUQ1, ZDecimal customsQty2, ZString customsUQ2, ZDecimal customsQty3, ZString customsUQ3);
		}

		public interface IWhsInventoryDutyAndTaxCalculatorProvider
		{
			IWhsInventoryDutyAndTaxCalculator GetProviderFor(BusinessObjectFactory factory, ZString countryCode);
		}

		public interface IZAWhsInventoryDutyAndTaxCalculatorTestHelper
		{
			ZDateTime StartDate { get; }
			ZDateTime EndDate { get; }
			ZString TariffCode { get; }
			void SetupTestData(BusinessObjectFactory factory);
		}
	}
}
