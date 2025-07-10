using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.HEN.Nissan.Testing
{
	public class NissanInvoiceFlatFileFormatTest : TestCaseWithFactory
	{
		public void TestFileExtensionForImport()
		{
			AssertEquals("File Extension Type could be any", FileExtensionType.All, FlatFileFormat.FileExtensionForImport);
		}

		public void TestFileExtensionForExport()
		{
			AssertExceptionThrown(typeof(NotImplementedException), delegate
			{
				FileExtensionType type = FlatFileFormat.FileExtensionForExport;
			});
		}

		public void TestConvertToLine()
		{
			AssertExceptionThrown(typeof(NotImplementedException), () => FlatFileFormat.ConvertToLine(new FlatFileDataRow(1)));
		}

		public void TestConvertToRow()
		{
			string headerRow = "DHRDPROFORMA INVOICE    20090206NISSAN MOTOR CO. (AUSTRALIA) PTY. LTD.                                NATIONAL PARTS DIVISION                 NISSAN MOTOR CO. (AUST) P/L   LOCKED BAG 1450               DANDENONG STH.                VICTORIA                      3164+61 3 9797 5000+61 3 9797 5021NISSAN NEW ZEALAND LTD                  P O BOX 98888                 SOUTH AUCKLAND MAILCENTRE     WIRI  AUCKLAND  NEW ZEALAND                                 00015NZEA1Auto Spareparts     000000139000000384AUD000011545.07000002100.00000001404.24000000001.983";
			string lineRow = "SDTL07521386        B52404M400          SW OIL PRESSURE                         00000200000005.9900000011.9800001.004298NZ      0003JAPAN";
			string crapRow = "STLR07521387        000000002000000004000000013.68000000000.00000000001.06000000000.000";
			FlatFileDataRow dataRow = FlatFileFormat.ConvertToRow(headerRow);
			AssertEquals(NissanConstants.InvoiceHeader.FieldCount, dataRow.FieldCount);
			AssertEquals(NissanConstants.InvoiceHeaderLineType, dataRow[NissanConstants.LineTypePosition]);
			AssertEquals("20090206", dataRow[NissanConstants.InvoiceHeader.InvoiceNo]);
			AssertEquals("AUD", dataRow[NissanConstants.InvoiceHeader.Currency]);
			AssertEquals("000011545.07", dataRow[NissanConstants.InvoiceHeader.TotalAmount]);
			AssertEquals("000002100.00", dataRow[NissanConstants.InvoiceHeader.GrossWeight]);
			dataRow = FlatFileFormat.ConvertToRow(lineRow);
			AssertEquals(NissanConstants.InvoiceLine.FieldCount, dataRow.FieldCount);
			AssertEquals(NissanConstants.InvoiceLineLineType, dataRow[NissanConstants.LineTypePosition]);
			AssertEquals("B52404M400", dataRow[NissanConstants.InvoiceLine.ProductCode]);
			AssertEquals("000002", dataRow[NissanConstants.InvoiceLine.InvoiceQty]);
			AssertEquals("00000011.98", dataRow[NissanConstants.InvoiceLine.InvoicePrice]);
			AssertEquals("00001.00", dataRow[NissanConstants.InvoiceLine.GrossWeight]);
			AssertEquals("4298NZ", dataRow[NissanConstants.InvoiceLine.OrderNo]);
			AssertEquals("0003", dataRow[NissanConstants.InvoiceLine.OrderLineNo]);
			AssertEquals("JAPAN", dataRow[NissanConstants.InvoiceLine.GoodsOrigin]);
			dataRow = FlatFileFormat.ConvertToRow(crapRow);
			AssertEquals(1, dataRow.FieldCount);
			AssertEquals(ZString.Empty, dataRow[0]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			FlatFileFormat = new NissanInvoiceFlatFileFormat();
		}

		NissanInvoiceFlatFileFormat FlatFileFormat;
	}
}
