using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Forwarding.Business.Test
{
	[TestedType(typeof(ForwardingShipmentCustomsInformationValidation))]
	class ForwardingShipmentCustomsInformationValidationTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		[TestDate(2025, 01, 01)]
		public void TestCheckDestinationExchangeRate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(Core.Constants.CurrencyCodes.UnitedStates))
			{
				var exchangeRate_06 = Factory.New<RefExchangeRate>();
				exchangeRate_06.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Canada;
				exchangeRate_06.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRate_06.RE_StartDate = new ZDateTime(2024, 06, 01);
				exchangeRate_06.RE_ExpiryDate = new ZDateTime(2024, 06, 30);
				exchangeRate_06.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				exchangeRate_06.RE_SellRate = 0.736431m;
				Factory.Save();

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_GoodsValue = 1000m;
				shipment.JS_RX_NKGoodsValueCurr = Core.Constants.CurrencyCodes.UnitedStates;
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "USPHL";
				shipment.JS_E_DEP = new ZDateTime(2024, 06, 15);

				var customsInformation = new ForwardingShipmentCustomsInformation(shipment);
				customsInformation.Validation.ValidateDestinationExchangeRate();
				NUnit.Framework.Assert.That(customsInformation.DestinationExchangeRate, Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "Destination Exchange Rate is 1.");
				NUnit.Framework.Assert.That(customsInformation.DestinationExchangeRateInfo.Notifications.Count(), Is.EqualTo(0), "There is no notification on Destination Exchange Rate field");

				shipment.JS_RL_NKDestination = "CAVAN";
				customsInformation.Validation.ValidateDestinationExchangeRate();
				NUnit.Framework.Assert.That(customsInformation.DestinationExchangeRate, Is.EqualTo(1.3579m).Using(CustomComparers.TypeComparison), "Destination Exchange Rate is 1.3579.");
				NUnit.Framework.Assert.That(customsInformation.DestinationExchangeRateInfo.Notifications.Count(), Is.EqualTo(0), "There is no notification on Destination Exchange Rate field");

				shipment.JS_E_DEP = new ZDateTime(2024, 08, 20);
				customsInformation.Validation.ValidateDestinationExchangeRate();
				NUnit.Framework.Assert.That(customsInformation.DestinationExchangeRate, Is.EqualTo(1.3579m).Using(CustomComparers.TypeComparison), "Destination Exchange Rate is 1.3579.");
				NUnit.Framework.Assert.That(customsInformation.DestinationExchangeRateInfo.Notifications.Count(), Is.EqualTo(1), "There is a notification on Destination Exchange Rate field");
				NUnit.Framework.Assert.That(customsInformation.DestinationExchangeRateInfo.HasWarning("There is no exchange rate in database for 20/08/2024. The closest match of the exchange rate for destination currency CAD is the rate for the date 30/06/2024. This exchange rate will be used in calculating the destination value."), Is.EqualTo(true));

				shipment.JS_E_DEP = new ZDateTime(2036, 01, 01);
				customsInformation.Validation.ValidateDestinationExchangeRate();
				NUnit.Framework.Assert.That(customsInformation.DestinationExchangeRate, Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "Destination Exchange Rate is 0.");
				NUnit.Framework.Assert.That(customsInformation.DestinationExchangeRateInfo.Notifications.Count(), Is.EqualTo(1), "There is a notification on Destination Exchange Rate field");
				NUnit.Framework.Assert.That(customsInformation.DestinationExchangeRateInfo.HasWarning("There is no valid exchange rate found for destination currency CAD for 1/01/2036."), Is.EqualTo(true));
			}
		}
	}
}
