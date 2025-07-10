using System.IO;
using System.Xml;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class CommercialInvoiceXmlReaderTest : TestCase
	{
		public void TestInvoiceNumber()
		{
			AssertEquals("Inv No 1", reader.InvoiceNumber);
		}

		public void TestInvoiceAmount()
		{
			AssertEquals("23.44", reader.InvoiceAmount);
		}

		public void TestInvoiceAmountCurrency()
		{
			AssertEquals("ZAR", reader.InvoiceAmountCurrency);
		}

		public void TestInvoiceDate()
		{
			AssertEquals(new ZDateTime(2004, 2, 15, 0, 0, 0), reader.InvoiceDate);
		}

		public void TestValuationDate()
		{
			AssertEquals(new ZDateTime(2004, 2, 16, 0, 0, 0), reader.ValuationDate);
		}

		public void TestConsignorNode()
		{
			AssertEquals("Consignor", reader.ConsignorNode.Name);
		}

		public void TestIsGroupInvoice()
		{
			AssertEquals("false", reader.IsGroupInvoice);
		}

		public void TestRelatedGroupInvoiceNumber()
		{
			AssertEquals("", reader.RelatedGroupInvoiceNumber);
		}

		public void TestIncoTerm()
		{
			AssertEquals("DDU", reader.IncoTerm);
		}

		public void TestVolume()
		{
			AssertEquals("99.88", reader.Volume);
		}

		public void TestVolumeUnit()
		{
			AssertEquals("M3", reader.VolumeUnit);
		}

		public void TestWeight()
		{
			AssertEquals("11.11", reader.Weight);
		}

		public void TestWeightUnit()
		{
			AssertEquals("KG", reader.WeightUnit);
		}

		public void TestCommercialInvoiceChargesXmlReaders()
		{
			AssertEquals(2, reader.CommercialInvoiceChargesXmlReaders.Length);
		}

		public void TestCommercialInvoiceLinesXmlReaders()
		{
			AssertEquals(2, reader.CommercialInvoiceLinesXmlReaders.Length);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var xmlDoc = new XmlDocument();

			using (Stream xmlStream = new FileStream(BaseSourcePath + @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\Xml\Legacy\TestFiles\CommercialInvoice.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				xmlDoc.Load(xmlStream);
				var commercialInvoiceHeaderNode = xmlDoc.SelectSingleNode(@"//" + CommercialInvoiceXsd.XPath.Invoices.InvoiceHeader);

				reader = new CommercialInvoiceXmlReader(commercialInvoiceHeaderNode, xmlDoc);
			}
		}
		CommercialInvoiceXmlReader reader;
	}
}
