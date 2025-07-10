using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat.Testing
{
	public class _500000LineTest : TestCase
	{
		public void TestDescription()
		{
			AssertEquals("G/RIDGE B/L BANJO BOLT 7/16-24", _500000Line.Description);
		}

		public void TestQuantity()
		{
			AssertEquals(1m, _500000Line.Quantity);
		}

		public void TestUnitOfQuantity()
		{
			AssertEquals(Core.Constants.PkgUnit.Piece, _500000Line.UnitOfQuantity);
		}

		public void TestPrice()
		{
			AssertEquals(12.17m, _500000Line.Price);
		}

		public void TestCurrencyCode()
		{
			AssertEquals("USD", _500000Line.CurrencyCode);
		}

		public void TestInvoiceNumber()
		{
			AssertEquals("300", _500000Line.InvoiceNumber);
		}

		public void TestCountryOfOrigin()
		{
			AssertEquals("US", _500000Line.CountryOfOrigin);
		}

		public void TestCommodityCode()
		{
			AssertEquals("8714190060", _500000Line.CommodityCode);
		}

		public void TestLicenceNumber()
		{
			AssertEquals("NLR", _500000Line.LicenceNumber);
		}

		public void TestCountryOfUltimateDestination()
		{
			AssertEquals("AU", _500000Line.CountryOfUltimateDestination);
		}

		public void TestPartNumber()
		{
			AssertEquals("14138", _500000Line.PartNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			_500000Line = new _500000Line("US3295AU9639050704              DAT2773T8Z8W5110001   EA G/RIDGE B/L BANJO BOLT 7/16-24                                                                          1217      USD300                 US8714190060          NLR            AU14138                                                                                                                                            ");
		}

		_500000Line _500000Line;
	}
}
