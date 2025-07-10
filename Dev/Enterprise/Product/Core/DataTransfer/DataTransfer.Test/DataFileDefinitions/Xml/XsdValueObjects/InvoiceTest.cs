using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.InvoiceLine))]
	sealed class InvoiceTest : ValueObjectTestCase
	{
		public void TestFieldsSpecified()
		{
			InvoiceLine value = new InvoiceLine();
			Assert(value.CustomFlag1Specified);
			Assert(value.CustomFlag2Specified);
			Assert(value.CustomFlag3Specified);

			Assert(!value.CustomDecimal1Specified);
			Assert(!value.CustomDecimal2Specified);
			Assert(!value.CustomDecimal3Specified);

			value.CustomDecimal1 = 10.10m;
			value.CustomDecimal2 = 10.11m;
			value.CustomDecimal3 = 10.11m;

			Assert(value.CustomDecimal1Specified);
			Assert(value.CustomDecimal2Specified);
			Assert(value.CustomDecimal3Specified);
		}

		public void TestLinePriceSpecified()
		{
			InvoiceLine line = new InvoiceLine();
			AssertEquals(false, line.LinePrice.IsSpecified);

			line.LinePrice.Value = 120m;
			Assert(line.LinePrice.IsSpecified);

			line.LinePrice.Value = ZDecimal.Zero;
			line.LinePrice.CurrencyCode = "XXX";
			Assert(line.LinePrice.IsSpecified);
		}

		public void TestCompileTimeCheck()
		{
			Xsd.InvoiceLine value = null;
			value = new Xsd.InvoiceLineCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestInvoiceQty_SerializedEvenIfZero()
		{
			InvoiceLine invoiceLine = new InvoiceLine();
			invoiceLine.InvoiceQty.Value = 0;
			AssertEquals("The InvoiceQty xml node should be specified even if it is zero, because it is a mandatory field", true, invoiceLine.InvoiceQty.IsSpecified);
		}
	}
}
