using System.IO;
using System.Xml;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class CommercialInvoiceLinesXmlReaderTest : TestCase
	{
		public void TestInvoiceQty()
		{
			AssertEquals("44.25", reader.InvoiceQty);
		}

		public void TestInvoiceQtyUnit()
		{
			AssertEquals("KG", reader.InvoiceQtyUnit);
		}

		public void TestLinePrice()
		{
			AssertEquals("875.35", reader.LinePrice);
		}

		public void TestLinePriceCurrency()
		{
			AssertEquals("NZD", reader.LinePriceCurrency);
		}

		public void TestProductNumber()
		{
			AssertEquals("Prod No 1", reader.ProductNumber);
		}

		public void TestProductDescription()
		{
			AssertEquals("Desc 1", reader.ProductDescription);
		}

		public void TestCustomsInvoiceQty()
		{
			AssertEquals("25.33", reader.CustomsInvoiceQty);
		}

		public void TestCustomsInvoiceQtyUnit()
		{
			AssertEquals("KG", reader.CustomsInvoiceQtyUnit);
		}

		public void TestOrderNumber()
		{
			AssertEquals("Ord No 1", reader.OrderNumber);
		}

		public void TestTariffCode()
		{
			AssertEquals("1234.12.34", reader.TariffCode);
		}

		public void TestTariffLookup()
		{
			AssertEquals("Ornamental Fish", reader.TariffLookup);
		}

		public void TestOriginOfGoods()
		{
			AssertEquals("KR", reader.OriginOfGoods);
		}

		public void TestTreatmentCode()
		{
			AssertEquals("Y", reader.TreatmentCode);
		}

		public void TestPreference()
		{
			AssertEquals("X", reader.Preference);
		}

		public void TestConcession()
		{
			AssertEquals("Concession", reader.Concession);
		}

		public void TestVolume()
		{
			AssertEquals("12.21", reader.Volume);
		}

		public void TestVolumeUnit()
		{
			AssertEquals("M3", reader.VolumeUnit);
		}

		public void TestWeight()
		{
			AssertEquals("354.25", reader.Weight);
		}

		public void TestWeightUnit()
		{
			AssertEquals("KG", reader.WeightUnit);
		}

		public void TestCustomText1()
		{
			AssertEquals("CustomText1_1", reader.CustomText1);
		}

		public void TestCustomText2()
		{
			AssertEquals("CustomText2_1", reader.CustomText2);
		}

		public void TestCustomText3()
		{
			AssertEquals("CustomText3_1", reader.CustomText3);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var xmlDoc = new XmlDocument();

			using (Stream xmlStream = new FileStream(BaseSourcePath + @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\Xml\Legacy\TestFiles\CommercialInvoice.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				xmlDoc.Load(xmlStream);
				var commercialInvoiceLineNode = xmlDoc.SelectSingleNode(@"//" + CommercialInvoiceXsd.XPath.InvoiceHeader.InvoiceLine);

				reader = new CommercialInvoiceLinesXmlReader(commercialInvoiceLineNode, xmlDoc);
			}
		}
		CommercialInvoiceLinesXmlReader reader;
	}
}
