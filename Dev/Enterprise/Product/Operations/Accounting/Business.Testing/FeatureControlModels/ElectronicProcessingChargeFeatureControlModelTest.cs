using System.Linq;
using CargoWise.FeatureControl;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(ElectronicProcessingChargeFeatureControlModel))]
	public class ElectronicProcessingChargeFeatureControlModelTest : TestCase
	{
		public void TestDateInElectronicProcessingChargeFeatureControlModel()
		{
			TestCase("01-Apr-25", "02-Apr-25", "01-Dec-24");
			TestCase("01-APR-25", "02-APR-25", "01-DEC-24");
			TestCase("2025-04-01", "2025-04-02", "2024-12-01");
			TestCase("01-04-2025", "02-04-2025", "01-12-2024");
			TestCase("01-04-25", "02-04-25", "01-12-24");

			void TestCase(string inputStart, string inputEnd, string validFromDate)
			{
				var model = CreateAndDeserialize(inputStart, inputEnd, validFromDate);
				AssertDates(model, "01-Apr-25", "02-Apr-25", "01-Dec-24");
			}
		}

		ElectronicProcessingChargeFeatureControlModel CreateAndDeserialize(string startDate, string endDate, string validFromDate)
		{
			var rule = new FeatureControlRule
			{
				FCM_FeatureControlCode = "ACCEPCFTR",
				FCR_Parameters = $"{{\"ElectronicProcessingChargeConfiguration\":[{{\"JobType\":\"SHP\",\"StartDate\":\"{startDate}\",\"EndDate\":\"{endDate}\"}}],\"ElectronicProcessingChargeCurrency\":[{{\"CurrencyCode\":\"USD\",\"ValidFromDate\":\"{validFromDate}\"}}]}}"
			};

			var featureData = new FeatureData(rule);
			featureData.TryDeserializeParameterAsJson(out ElectronicProcessingChargeFeatureControlModel model);
			return model;
		}

		void AssertDates(ElectronicProcessingChargeFeatureControlModel model, string expectedStart, string expectedEnd, string validFromDate)
		{
			var configurationConfig = model.ElectronicProcessingChargeConfiguration.FirstOrDefault();
			AssertNotNull(configurationConfig);
			AssertEquals(expectedStart, new ZDate(configurationConfig.StartDate).ToShortDateString());
			AssertEquals(expectedEnd, new ZDate(configurationConfig.EndDate).ToShortDateString());

			var currencyConfig = model.ElectronicProcessingChargeCurrency.FirstOrDefault();
			AssertNotNull(currencyConfig);
			AssertEquals(validFromDate, new ZDate(currencyConfig.ValidFromDate).ToShortDateString());
		}
	}
}
