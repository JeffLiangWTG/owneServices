using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM428GoodsItemProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationGoodsItemNumber()
		{
			AssertEquals("1", provider.DeclarationGoodsItemNumber);
		}

		public void TestTaxes()
		{
			AssertType<DutiesAndTaxesTypeProvider[]>(provider.TaxTypes);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var xmlObject = new Im428GoodsShipmentItemType();
			xmlObject.DeclarationGoodsItemNumber = "1";
			xmlObject.CalculationOfTaxes = new MCalculationOfTaxesType03();
			xmlObject.CalculationOfTaxes.DutiesAndTaxes = new Collection<MDutiesAndTaxesType03>
			{
				new MDutiesAndTaxesType03()
			};

			provider = new IM428GoodsItemProvider(xmlObject);
		}
		IM428GoodsItemProvider provider;
	}
}
