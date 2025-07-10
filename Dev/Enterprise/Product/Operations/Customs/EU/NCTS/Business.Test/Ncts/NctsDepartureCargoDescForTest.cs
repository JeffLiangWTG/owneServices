using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public class NctsDepartureCargoDescForTest : NctsDepartureCargoDesc
	{
		public NctsDepartureCargoDescForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			exciseRateForTesting = factory.New<RateView>();
		}

		protected override IEnumerable<RateView> ExciseRates
		{
			get
			{
				exciseRateForTesting.ZZ2_RateFormula = $"{ExciseAmountForTesting} * [FLAT]";
				yield return exciseRateForTesting;
			}
		}

		public ZDecimal ExciseAmountForTesting
		{
			get => exciseAmountForTesting;
			set
			{
				SetNonPersistentPropertyValue(ExciseAmountForTestingInfo, ref exciseAmountForTesting, value);
				UpdateFeesFromTariffRates(updateExcise: true);
			}
		}

		ZDecimal exciseAmountForTesting;

		public ZPropertyInfo ExciseAmountForTestingInfo => GetZPropertyInfo(nameof(ExciseAmountForTesting));

		readonly RateView exciseRateForTesting;
	}
}
