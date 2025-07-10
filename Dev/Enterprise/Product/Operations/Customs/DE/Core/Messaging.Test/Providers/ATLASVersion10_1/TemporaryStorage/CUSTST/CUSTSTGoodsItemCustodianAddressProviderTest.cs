using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSTSTGoodsItemCustodianAddressProvider))]
	sealed class CUSTSTGoodsItemCustodianAddressProviderTest : InboundDataProviderTestCase<IUnderCustomsControlGoodsItemAddress, CUSTSTGoodsItemCustodianAddressProvider>
	{
		[ExpectNoExceptions]
		public void TestLine()
		{
			NUnit.Framework.Assert.That(dataProvider.Line, Is.EqualTo("Line 1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCountry()
		{
			NUnit.Framework.Assert.That(dataProvider.Country, Is.EqualTo("DE").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPostcode()
		{
			NUnit.Framework.Assert.That(dataProvider.Postcode, Is.EqualTo("123456").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCity()
		{
			NUnit.Framework.Assert.That(dataProvider.City, Is.EqualTo("Berlin").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDistrict()
		{
			NUnit.Framework.Assert.That(dataProvider.District, Is.EqualTo("District 1").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var goodsItem = new SCTSTJGoodsItem()
			{
				Custodian = new SCTSTJGoodsItemCustodian()
				{
					Address = new SCTSTJGoodsItemCustodianAddress()
					{
						Line = "Line 1",
						Country = "DE",
						Postcode = "123456",
						City = "Berlin",
						District = "District 1"
					}
				}
			};
			dataProvider = new CUSTSTGoodsItemCustodianAddressProvider(goodsItem.Custodian.Address);
		}
		IUnderCustomsControlGoodsItemAddress dataProvider;

		protected override CUSTSTGoodsItemCustodianAddressProvider GetProvider() => (CUSTSTGoodsItemCustodianAddressProvider)dataProvider;
	}
}
