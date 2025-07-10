using System.IO;
using System.Xml;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class CommercialInvoiceChargesXmlReaderTest : TestCase
	{
		public void TestChargeType()
		{
			AssertEquals("COM", reader.ChargeType);
		}

		public void TestChargeValue()
		{
			AssertEquals("7.89", reader.ChargeValue);
		}

		public void TestChargeValueCurrency()
		{
			AssertEquals("AUD", reader.ChargeValueCurrency);
		}

		public void TestGSTApplies()
		{
			AssertEquals("true", reader.GSTApplies);
		}

		public void TestDutyApplies()
		{
			AssertEquals("true", reader.DutyApplies);
		}

		public void TestIsIncludedInTotal()
		{
			AssertEquals("true", reader.IsIncludedInTotal);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var xmlDoc = new XmlDocument();

			using (Stream xmlStream = new FileStream(BaseSourcePath + @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\Xml\Legacy\TestFiles\CommercialInvoice.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				xmlDoc.Load(xmlStream);
				var commercialInvoiceChargesNode = xmlDoc.SelectSingleNode(@"//" + CommercialInvoiceXsd.XPath.InvoiceHeader.InvoiceCharge);

				reader = new CommercialInvoiceChargesXmlReader(commercialInvoiceChargesNode, xmlDoc);
			}
		}
		CommercialInvoiceChargesXmlReader reader;
	}
}
