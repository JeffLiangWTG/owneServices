using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class BarcodeParserTest : TransactionedTestCase
	{
		public void TestIsDocTypeBarcode()
		{
			Assert("Has correct prefix/suffix, is DocType barcode", BarcodeHelper.IsDocTypeBarcode("^SHP=123456;INV|"));
			Assert("Doesn't have correct prefix/suffix", !BarcodeHelper.IsDocTypeBarcode("^SHP=123456;INV"));
		}

		public void TestIsRefTypeOnlyBarcode()
		{
			Assert(BarcodeHelper.IsRefTypeOnlyBarcode("^SHP=123456|"));
			Assert(!BarcodeHelper.IsRefTypeOnlyBarcode("^SHP=12345;CIV|"));
			Assert(!BarcodeHelper.IsRefTypeOnlyBarcode("^DOC=CIV|"));
		}

		public void TestIsDocTypeOnlyBarcode()
		{
			Assert(!BarcodeHelper.IsDocTypeOnlyBarcode("^SHP=123456|"));
			Assert(!BarcodeHelper.IsDocTypeOnlyBarcode("^SHP=12345;CIV|"));
			Assert(BarcodeHelper.IsDocTypeOnlyBarcode("^DOC=CIV|"));
		}

		public void TestIsDocTypeAndRefTypeBarcode()
		{
			Assert(!BarcodeHelper.IsDocTypeAndRefTypeBarcode("^SHP=123456|"));
			Assert(BarcodeHelper.IsDocTypeAndRefTypeBarcode("^SHP=12345;CIV|"));
			Assert(!BarcodeHelper.IsDocTypeAndRefTypeBarcode("^DOC=CIV|"));
		}

		public void TestIsPlaceholderBarcode()
		{
			AssertEquals("IsPlaceholderBarcode", true, BarcodeHelper.IsPlaceholderBarcode("^SHP=12345|"));
			AssertEquals("IsPlaceholderBarcode", true, BarcodeHelper.IsPlaceholderBarcode("^SHP=12345;INS|"));
			AssertEquals("IsPlaceholderBarcode", true, BarcodeHelper.IsPlaceholderBarcode("^DOC=INS|"));
			AssertEquals("IsPlaceholderBarcode", false, BarcodeHelper.IsPlaceholderBarcode(""));
			AssertEquals("IsPlaceholderBarcode", false, BarcodeHelper.IsPlaceholderBarcode("^SHP=12345;|"));
			AssertEquals("IsPlaceholderBarcode", false, BarcodeHelper.IsPlaceholderBarcode("^SHP=12345;INS;|"));
			AssertEquals("IsPlaceholderBarcode", false, BarcodeHelper.IsPlaceholderBarcode("^DOC=INS;|"));
		}

		public void TestIsRefTypeEncoded()
		{
			Assert("A shipment barcode (ReferenceType barcode) will have reftype", BarcodeHelper.IsRefTypeEncoded("[EDICIVSYDMEL12345]"));
			Assert("Doctype barcode with reftype", BarcodeHelper.IsRefTypeEncoded("^SHP=123456|"));
			Assert("Doctype barcode with reftype and doctype", BarcodeHelper.IsRefTypeEncoded("^SHP=12345;CIV|"));
			Assert("Doctype barcode with doctype only", !BarcodeHelper.IsRefTypeEncoded("^DOC=CIV|"));
		}

		public void TestGetReferenceTypeFromBarCodeText()
		{
			AssertEquals("Good SHP 1", "ICJ12345", BarcodeHelper.GetValueForCode("^SHP=ICJ12345|", Core.Constants.DocManagerCodes.Shipment));

			AssertEquals("No Doc Type", "", BarcodeHelper.GetValueForCode("^SHP=ICJ12345|", "DOC"));

			AssertEquals("Good SHP 2", "K", BarcodeHelper.GetValueForCode("^SHP=K|", Core.Constants.DocManagerCodes.Shipment));

			AssertEquals("No terminator", "", BarcodeHelper.GetValueForCode("^SHP=ICJ12345", Core.Constants.DocManagerCodes.Shipment));

			AssertEquals("No SHP number", "", BarcodeHelper.GetValueForCode("^SHP=|", Core.Constants.DocManagerCodes.Shipment));

			AssertEquals("Bad Header", "", BarcodeHelper.GetValueForCode("^SH=WSE|", Core.Constants.DocManagerCodes.Shipment));

			AssertEquals("Empty data", "", BarcodeHelper.GetValueForCode("", Core.Constants.DocManagerCodes.Shipment));
		}

		public void TestGetDocumentTypeFromBarCodeText()
		{
			AssertEquals("Good Document type 1", "ARN", BarcodeHelper.GetValueForCode("^DOC=ARN|", "DOC"));
			AssertEquals("No Job", "", BarcodeHelper.GetValueForCode("^DOC=ARN|", "JOB"));

			AssertEquals("Good Document type 2", "X", BarcodeHelper.GetValueForCode("^DOC=X|", "DOC"));

			AssertEquals("No terminator", "", BarcodeHelper.GetValueForCode("^DOC=X", "DOC"));

			AssertEquals("No document type", "", BarcodeHelper.GetValueForCode("^DOC=|", "DOC"));

			AssertEquals("Bad header", "", BarcodeHelper.GetValueForCode("^DOD=AWS|", "DOC"));

			AssertEquals("Empty data", "", BarcodeHelper.GetValueForCode("", "DOC"));
		}

		public void TestGetGoodDocumentTypeAndJobFromBarCodeText()
		{
			AssertEquals("Good Doc type", "XYZ", BarcodeHelper.GetValueForCode("^SHP=123;DOC=XYZ|", "DOC"));
			AssertEquals("Good SHP", "123", BarcodeHelper.GetValueForCode("^SHP=123;DOC=XYZ|", Core.Constants.DocManagerCodes.Shipment));
		}

		public void TestGetGoodDocumentTypeAndJobFromBarCodeTextReverseOrder()
		{
			AssertEquals("Good Doc type", "X", BarcodeHelper.GetValueForCode("^DOC=X;SHP=1|", "DOC"));
			AssertEquals("Good SHP", "1", BarcodeHelper.GetValueForCode("^DOC=X;SHP=1|", Core.Constants.DocManagerCodes.Shipment));
		}

		public void TestGetValueForCode()
		{
			AssertEquals("using reftype directly", "ABC123", BarcodeHelper.GetValueForCode("^SHP=ABC123;INV|", Core.Constants.DocManagerCodes.Shipment));
		}

		public void TestGetEncodedReferenceType()
		{
			switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				case "AU":
					AssertEquals("In Country AU where CLS not valid, doc Barcode should return nothing", "", BarcodeHelper.EncodedRefType("^CLS=ABC123|"));
					AssertEquals("In Country AU where DEC is valid, doc Barcode should return correctly", "DEC", BarcodeHelper.EncodedRefType("^DEC=111111|"));
					break;

				case "SG":
					AssertEquals("In Country SG where CLS is valid, doc Barcode should return correctly", "CLS", BarcodeHelper.EncodedRefType("^CLS=ABC123|"));
					AssertEquals("In Country SG where DEC is not valid, doc Barcode should return nothing", "", BarcodeHelper.EncodedRefType("^DEC=111111|"));
					break;
			}

			AssertEquals("Invalid ref code in any country, doc Barcode should return nothing", "", BarcodeHelper.EncodedRefType("^AAA=111111|"));
			AssertEquals("Globally valid ref type, Barcode should return correctly", Core.Constants.DocManagerCodes.Shipment, BarcodeHelper.EncodedRefType("^SHP=222222|"));
			AssertEquals("ref type only barcode should return a value", Core.Constants.DocManagerCodes.Organisation, BarcodeHelper.EncodedRefType("^REF=ORG|"));
		}

		public void TestJoinValidBarcodes()
		{
			//Arrange
			var barcodes = new List<string>()
			{
				"^SHP=S15SMEX0001886|",	//valid, shipment barcode
				"02320308401007773627700616284820", //invalid
				"^DDR=00001636;DDR@MFH|", //valid, doctype barcode with company
				"^DOC=HBL|", //valid, doctype barcode
				"IAH00000579", //invalid
				"^CON=C15AIAH00000308;MAN;|", //valid consol+doctype
				"[SHAHBLPVGFMOSHA00001980]" //valid new shipment barcode
			};

			//Act
			var validBarcodesString = BarcodeHelper.JoinValidBarcodes(barcodes);

			//Assert
			AssertEquals("Should remove the two invalid formatted barcodes",
				"^SHP=S15SMEX0001886| , ^DDR=00001636;DDR@MFH| , ^DOC=HBL| , ^CON=C15AIAH00000308;MAN;| , [SHAHBLPVGFMOSHA00001980]",
				validBarcodesString);
		}
	}
}
