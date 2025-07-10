using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Client.YAS.YASInvoiceImporter.Testing
{
	public class YASInvoiceFlatFileFormatTest : FlatFileFormatTestCase
	{
		#region Implementation
		public void TestConvertToRow()
		{
			YASInvoiceFlatFileFormat format = new YASInvoiceFlatFileFormat();
			ZString sampleRow = "9026932   ABA-3JK73-00-04     XV250 CLASSIC SADDLEBAG,PLAIN 00000001000000012592";
			FlatFileDataRow testRow = format.ConvertToRow(sampleRow);
			AssertEquals("Invoice Number", "9026932", testRow.GetField(InvoiceConstants.FixedFieldPosition.InvoiceNo));
			AssertEquals("Invoice Number", "ABA-3JK73-00-04", testRow.GetField(InvoiceConstants.FixedFieldPosition.PartNo));
			AssertEquals("Invoice Number", "XV250 CLASSIC SADDLEBAG,PLAIN", testRow.GetField(InvoiceConstants.FixedFieldPosition.PartDescription));
			AssertEquals("Invoice Number", "00000001", testRow.GetField(InvoiceConstants.FixedFieldPosition.Qty));
			AssertEquals("Invoice Number", "000000012592", testRow.GetField(InvoiceConstants.FixedFieldPosition.Value));
			sampleRow = "";
			testRow = format.ConvertToRow(sampleRow);
			AssertEquals("Should have no fields", 0, testRow.FieldCount);
		}

		public void TestClientSpecificExtension()
		{
			YASInvoiceFlatFileFormatForTest format = new YASInvoiceFlatFileFormatForTest();
			AssertEquals("Extension Type", FileExtensionType.ClientSpecific, format.FileExtensionForImport);
			AssertEquals("Extension", InvoiceConstants.FileExtension, format.ExtensionForTest);
		}

		protected override FlatFileFormat GetFlatFileFormat()
		{
			return new YASInvoiceFlatFileFormat();
		}

		public class YASInvoiceFlatFileFormatForTest : YASInvoiceFlatFileFormat
		{
			public ZString ExtensionForTest
			{
				get
				{
					return base.GetClientSpecificFileExtension();
				}
			}
		}
		#endregion
	}
}
