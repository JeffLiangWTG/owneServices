using System;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ExchangeRateTolerance))]
	public class ExchangeRateToleranceTest : Enterprise.Registry.Business.Testing.RegistryBusinessObjectTemplateTestCase<ExchangeRateTolerance>
	{
		protected override ExchangeRateTolerance GetBusinessObjectToClone()
		{
			var result = new ExchangeRateTolerance();

			return result;
		}

		public override void TestBizObjectFields()
		{
			Assert(true);
		}

		protected override ExchangeRateTolerance GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		public void TestValueReload()
		{
			var config = new ExchangeRateToleranceConfiguration();
			config.ExchangeRateToleranceCollection.RemoveAndDeleteAll();
			var newExchnageRateTolerance = new ExchangeRateTolerance
			{
				Currency = "AUD",
				ExchangeRateTolerancePercentage = 5.49M
			};
			config.ExchangeRateToleranceCollection.Add(newExchnageRateTolerance);
			AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, config);

			var registryReload = AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.Value.ExchangeRateToleranceCollection.Cast<ExchangeRateTolerance>();
			var reloadExchnageRateTolerance = registryReload.FirstOrDefault(x => x.Currency == newExchnageRateTolerance.Currency);
			AssertNotNull($"Reload exchange rate tolerance must be existed,{nameof(ExchangeRateTolerance.Currency)}", reloadExchnageRateTolerance);
			AssertEquals($"Reload exchange rate tolerance must be same as original,{nameof(ExchangeRateTolerance.ExchangeRateTolerancePercentage)}", 5.49M, reloadExchnageRateTolerance.ExchangeRateTolerancePercentage);
		}

		public void TestAllCurrency()
		{
			var registryCollection = AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.Value.ExchangeRateToleranceCollection;
			CombineAssertions("Precondition, must have All currency setting", () => {
				AssertEquals(1, registryCollection.Count);
				AssertEquals(ExchangeRateToleranceLookups.AllCurrencyCode, registryCollection[0].Currency);
			});

			AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.Value.ExchangeRateToleranceCollection.RemoveAndDeleteAll();
			Factory.Save();

			var registryCollectionReload = AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.Value.ExchangeRateToleranceCollection;
			CombineAssertions("Must have All currency setting even we deleted before", () => {
				AssertEquals(1, registryCollectionReload.Count);
				AssertEquals(ExchangeRateToleranceLookups.AllCurrencyCode, registryCollectionReload[0].Currency);
				AssertEquals(true, registryCollectionReload[0].Currency_ReadOnly);

				AssertEquals(false, registryCollectionReload[0].CanDelete);
				AssertEquals("This is a system defined value and cannot be deleted.", registryCollectionReload[0].ReasonForNotAbleToDelete);
			});
		}

		public void TestPreSaveValidation()
		{
			var registryCollection = AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.Value.ExchangeRateToleranceCollection;

			var duplicatedItem = registryCollection.AddNew();
			duplicatedItem.Currency = ExchangeRateToleranceLookups.AllCurrencyCode;
			duplicatedItem.ExchangeRateTolerancePercentage = -1m;

			var inValidCodeItem = registryCollection.AddNew();
			inValidCodeItem.Currency = "AAA";
			inValidCodeItem.ExchangeRateTolerancePercentage = 0m;

			var outRangeItem = registryCollection.AddNew();
			outRangeItem.Currency = "USD";
			outRangeItem.ExchangeRateTolerancePercentage = -1m;

			AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.Value.RunPreSaveValidation();

			AssertHasError(duplicatedItem.CurrencyInfo, "Only single configuration per currency should be allowed.");
			AssertHasError(inValidCodeItem.CurrencyInfo, "Enter a valid selection.");
			AssertHasError(outRangeItem.ExchangeRateTolerancePercentageInfo, "Value should be between 0 to 100.");
		}
	}
}
