using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM404GoodsItemProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationGoodsItemNumber()
		{
			AssertEquals("1", provider.DeclarationGoodsItemNumber);
		}

		public void TestTaxBse()
		{
			AssertType<DutiesAndTaxesTypeProvider[]>(provider.TaxTypes);
		}

		protected override void SetUp()
		{
			base.SetUp();

			GoodsShipmentItemIm404Type xmlObject = new GoodsShipmentItemIm404Type();
			xmlObject.DeclarationGoodsItemNumber = "1";
			xmlObject.CalculationOfTaxes = new MCalculationOfTaxesType04();
			xmlObject.CalculationOfTaxes.DutiesAndTaxes = new Collection<MDutiesAndTaxesType03>
			{
				new MDutiesAndTaxesType03()
			};

			provider = new IM404GoodsItemProvider(xmlObject);
		}
		IM404GoodsItemProvider provider;
	}
}
