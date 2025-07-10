using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM428;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Messaging.Testing.Providers.AIS.Incoming.UCC5
{
	sealed class IM428GoodsItemProviderUCC5Test : TestCaseWithFactory
	{
		public void TestDeclarationGoodsItemNumber()
		{
			AssertEquals("1", provider.DeclarationGoodsItemNumber);
		}

		public void TestTaxes()
		{
			AssertType<UCC5DutiesAndTaxesTypeProvider[]>(provider.TaxTypes);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var xmlObject = new GoodsShipmentTypeItem();
			xmlObject.GoodsItemNumber16 = "1";
			xmlObject.Taxes = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.TaxesType();
			xmlObject.Taxes.TaxBox43Bis = new Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.TaxBoxType>
			{
				new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.TaxBoxType
				{
					BoxTaxType = "B00",
					BoxAmount = 9000m,
					BoxTaxRate = 4.7m,
					BoxTaxPayableAmount = 423m,
					BoxTaxPaymentMethod = "A"
				}
			};

			provider = new IM428GoodsItemProviderUCC5(xmlObject);
		}
		IM428GoodsItemProviderUCC5 provider;
	}
}
