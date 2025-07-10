using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.RF409;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	[TestedType(typeof(GoodsInformationProvider))]
	sealed class GoodsInformationProviderTest : TestCase
	{
		public void TestProperties()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Amount", 100.59m, provider.Amount);
				AssertEquals("Currency", Core.Constants.CurrencyCodes.Ireland, provider.Currency);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new GoodsInformationProvider(
				new GoodsInformationTypeCustomsValue
				{
					Amount = 100.59m,
					Currency = Core.Constants.CurrencyCodes.Ireland,
				});
		}

		GoodsInformationProvider provider;
	}
}
