using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ExchangeRateToleranceConfiguration))]
	public class ExchangeRateToleranceConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestExchangeRateToleranceDefault()
		{
			var config = new ExchangeRateToleranceConfiguration();
			AssertEquals("Default settings should be empty", 0, config.ExchangeRateToleranceCollection.Count);
		}

		public void TestReloadSort()
		{
			var config = new ExchangeRateToleranceConfiguration();
			config.ExchangeRateToleranceCollection.Add(new ExchangeRateTolerance()
			{
				Currency = "USD",
				ExchangeRateTolerancePercentage = 2
			});
			config.ExchangeRateToleranceCollection.Add(new ExchangeRateTolerance()
			{
				Currency = "EUR",
				ExchangeRateTolerancePercentage = 2
			});
			config.ExchangeRateToleranceCollection.Add(new ExchangeRateTolerance()
			{
				Currency = "AUD",
				ExchangeRateTolerancePercentage = 2
			});
			config.ExchangeRateToleranceCollection.Add(new ExchangeRateTolerance()
			{
				Currency = "NZD",
				ExchangeRateTolerancePercentage = 2
			});
			config.ExchangeRateToleranceCollection.Add(new ExchangeRateTolerance()
			{
				Currency = "HKD",
				ExchangeRateTolerancePercentage = 2
			});
			AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, config);
			Factory.Save();

			var currencyCodeSortResult =
				AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.Value.ExchangeRateToleranceCollection
				.Cast<ExchangeRateTolerance>()
				.Select(x => x.Currency)
				.ToArray();

			AssertArrayEqualsByElements(new ZString[] {
				ExchangeRateToleranceLookups.AllCurrencyCode,
				"AUD",
				"EUR",
				"HKD",
				"NZD",
				"USD"
			}, currencyCodeSortResult);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ExchangeRateToleranceConfiguration result = new ExchangeRateToleranceConfiguration();
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new ExchangeRateToleranceConfiguration BizObj
		{
			get { return (ExchangeRateToleranceConfiguration)base.BizObj; }
		}

		#endregion

	}
}
