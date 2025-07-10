using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing.UCC5
{
	class IM429GoodsItemProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationGoodsItemNumber()
		{
			AssertEquals("1", provider.DeclarationGoodsItemNumber);
		}

		public void TestTaxTypes()
		{
			AssertType<IM429TaxBoxTypeProvider[]>(provider.TaxTypes);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new Messaging.UCC5.IM429GoodsItemProvider(new GoodsShipmentItemType
			{
				GoodsItemNumber16 = "1",
				Taxes = new TaxesType()
			});
		}
		Messaging.UCC5.IM429GoodsItemProvider provider;
	}
}
