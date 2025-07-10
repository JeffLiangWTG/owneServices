using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSTSTGoodsItemCustodyPlaceAddressProvider))]
	sealed class CUSTSTGoodsItemCustodyPlaceAddressProviderTest : InboundDataProviderTestCase<IUnderCustomsControlGoodsItemAddress, CUSTSTGoodsItemCustodyPlaceAddressProvider>
	{
		[ExpectNoExceptions]
		public void TestLine()
		{
			NUnit.Framework.Assert.That(dataProvider.Line, Is.EqualTo("Line 1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCountry()
		{
			NUnit.Framework.Assert.That(dataProvider.Country, Is.EqualTo(ZString.Empty));
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
				CustodyPlace = new SCTSTJGoodsItemCustodyPlace()
				{
					Address = new SCTSTJGoodsItemCustodyPlaceAddress()
					{
						Line = "Line 1",
						Postcode = "123456",
						City = "Berlin",
						District = "District 1"
					}
				}
			};
			dataProvider = new CUSTSTGoodsItemCustodyPlaceAddressProvider(goodsItem.CustodyPlace.Address);
		}
		IUnderCustomsControlGoodsItemAddress dataProvider;

		protected override CUSTSTGoodsItemCustodyPlaceAddressProvider GetProvider() => (CUSTSTGoodsItemCustodyPlaceAddressProvider)dataProvider;
	}
}
