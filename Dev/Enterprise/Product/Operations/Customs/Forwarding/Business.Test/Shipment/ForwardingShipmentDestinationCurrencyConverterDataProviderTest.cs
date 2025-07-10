using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Forwarding.Business.Test
{
	[TestedType(typeof(ForwardingShipmentDestinationCurrencyConverterDataProvider))]
	class ForwardingShipmentDestinationCurrencyConverterDataProviderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCurrencyConverterDataProvider()
		{
			var dataProvider = new ForwardingShipmentDestinationCurrencyConverterDataProvider(Factory, new ZDateTime(2024, 11, 18), "USD");
			NUnit.Framework.Assert.That(dataProvider.DateForRate.ToDateTime(), Is.EqualTo(new DateTime(2024, 11, 18)));
			NUnit.Framework.Assert.That(dataProvider.LocalCurrencyCode, Is.EqualTo("USD").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(dataProvider.LocalCurrencyCodeOverride, Is.EqualTo("USD").Using(CustomComparers.TypeComparison));

			dataProvider.LocalCurrencyCode = "CAD";
			NUnit.Framework.Assert.That(dataProvider.LocalCurrencyCode, Is.EqualTo("CAD").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(dataProvider.LocalCurrencyCodeOverride, Is.EqualTo("CAD").Using(CustomComparers.TypeComparison));
		}
	}
}
