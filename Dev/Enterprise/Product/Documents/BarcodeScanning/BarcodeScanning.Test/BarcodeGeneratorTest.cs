using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Barcode.Business.Testing
{
	sealed class BarcodeGeneratorTest : TestCase
	{
		public void TestCreateShipmentBarcode()
		{
			TextBarcode bC = Generator.CreateShipmentBarcode("ABC", "DEF", "", "GHI", "");
			AssertEquals("Text barcode is empty barcode - parameters missing", ZString.Empty, bC.TextAs128sFontString);

			bC = Generator.CreateShipmentBarcode("ABCH", "DEF", "AA", "GHI", "DKFJ");
			AssertEquals("Text barcode is empty barcode - parameters not the correct length", ZString.Empty, bC.TextAs128sFontString);

			bC = Generator.CreateShipmentBarcode("ABC", "DEF", "GHI", "JKL", "H1234");
			AssertEquals("Barcode text", "È[ABCDEFGHIJKLH1234]5Ê", bC.TextAs128sFontString);
		}

		public void TestCreateDocumentBarcodeOneParam()
		{
			TextBarcode bC = Generator.CreateDocumentBarcode("SHP");
			AssertEquals("Text barcode should be empty", ZString.Empty, bC.TextAs128sFontString);

			bC = Generator.CreateDocumentBarcode("");
			AssertEquals("Text barcode should be an empty barcode", ZString.Empty, bC.TextAs128sFontString);
		}

		public void TestCreateDocumentBarcodeTwoParams()
		{
			TextBarcode bC = Generator.CreateDocumentBarcode("SHP", "1000");
			Assert("Text barcode should not be an empty barcode", !bC.TextAs128sFontString.IsEmpty);
			AssertEquals("Text should not be empty", "È^SHP=1000|gÊ", bC.TextAs128sFontString);

			bC = Generator.CreateDocumentBarcode("SHP", "");
			AssertEquals("Text barcode should be an empty barcode - can't have just a Ref Type barcode", ZString.Empty, bC.TextAs128sFontString);

			bC = Generator.CreateDocumentBarcode("", "1000");
			AssertEquals("Can't have a refvalue without a reftype. Text barcode should be an empty barcode", ZString.Empty, bC.TextAs128sFontString);
		}

		public void TestCreateDocumentBarcodeThreeParams()
		{
			TextBarcode bC = Generator.CreateDocumentBarcode("SHP", "1000", "MSC");
			Assert("Text barcode should not be an empty barcode", !bC.TextAs128sFontString.IsEmpty);
			AssertEquals("Document type barcode text", "È^SHP=1000;MSC|%Ê", bC.TextAs128sFontString);

			bC = Generator.CreateDocumentBarcode("SHP", "", "MSC");
			AssertEquals("Text barcode should be an empty barcode", ZString.Empty, bC.TextAs128sFontString);

			bC = Generator.CreateDocumentBarcode("SHP", "", "");
			AssertEquals("Text barcode should be an empty barcode", ZString.Empty, bC.TextAs128sFontString);

			bC = Generator.CreateDocumentBarcode("", "1000", "MSC");
			AssertEquals("invalid barcode - can't have refvalue without reftype. Text barcode should be an empty barcode", ZString.Empty, bC.TextAs128sFontString);

			bC = Generator.CreateDocumentBarcode("", "1000", "");
			AssertEquals("invalid barcode - can't have refvalue without reftype. Text barcode should be an empty barcode", ZString.Empty, bC.TextAs128sFontString);

			bC = Generator.CreateDocumentBarcode("", "", "MSC");
			Assert("Creates a DocType barcode. Text barcode should not be an empty barcode", !bC.TextAs128sFontString.IsEmpty);
			AssertEquals("Document type barcode text", "È^DOC=MSC|>Ê", bC.TextAs128sFontString);
		}

		public void TestCreateDocumentBarcodeFourParams()
		{
			TextBarcode bC = Generator.CreateDocumentBarcode("SHP", "1000", "MSC", "EDI");
			Assert("Text barcode should not be an empty barcode", !bC.TextAs128sFontString.IsEmpty);
			AssertEquals("Document type barcode text", "È^SHP=1000;MSC@EDI|jÊ", bC.TextAs128sFontString);
		}

		public void TestCreateDocTypeBarcode()
		{
			TextBarcode bC = Generator.CreateDocTypeBarcode("ABC");
			Assert("Text barcode should not be an empty barcode", !bC.TextAs128sFontString.IsEmpty);
			AssertEquals("Document type barcode text", "È^DOC=ABC|MÊ", bC.TextAs128sFontString);

			bC = Generator.CreateDocTypeBarcode("");
			Assert("Text barcode should be an empty barcode", bC.TextAs128sFontString.IsEmpty);
		}

		public void TestCreateDocumentBarcodeWithNullValuesStillReturnsValidObject()
		{
			TextBarcode barcode = Generator.CreateDocumentBarcode(null, null, null);
			AssertNotNull("Even when passing in null values to generate a barcode, the return value is not null", barcode);
			Assert("Should be equal to an empty barcode", barcode.TextAs128sFontString.IsEmpty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Generator = new BarcodeGenerator();
		}

		BarcodeGenerator Generator;
	}
}
