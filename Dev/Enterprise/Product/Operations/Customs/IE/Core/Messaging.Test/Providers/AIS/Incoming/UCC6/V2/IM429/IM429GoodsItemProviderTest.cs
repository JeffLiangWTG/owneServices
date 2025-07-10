using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM429GoodsItemProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationGoodsItemNumber()
		{
			AssertEquals("1", provider.DeclarationGoodsItemNumber);
		}

		public void TestTaxes()
		{
			AssertType<TaxBoxTypeProvider[]>(provider.TaxTypes);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM429GoodsItemProvider(new MGoodsShipmentItemType01
			{
				DeclarationGoodsItemNumber = "1",
				Taxes = new TaxesItemType
				{
					TaxBoxbis = new Collection<TaxBoxType>
					{
						new TaxBoxType()
					}
				}
			});
		}
		IM429GoodsItemProvider provider;
	}
}
