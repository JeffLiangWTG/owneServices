using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.WCB.DaimlerChrysler.Testing
{
	public class DCImportFlatFileFormatTest : FlatFileFormatTestCase
	{
		protected override FlatFileFormat GetFlatFileFormat()
		{
			return new DCImportFlatFileFormat();
		}

		public void TestConvertToRow()
		{
			DCImportFlatFileFormat formatter = new DCImportFlatFileFormat();
			FlatFileDataRow row = formatter.ConvertToRow("***BOF:DaimlerChrysler<20040623><130027>***");
			AssertNotNull("Row should not be null", row);
			AssertEquals("Field Count", 1, row.FieldCount);
			AssertEquals("Header", "***BOF:DaimlerChrysler<20040623><130027>***", row.GetField(0));
			row = formatter.ConvertToRow("D1EE415 MANON               EE415 90130 000100010404200HBL1234567                    ");
			AssertNotNull("Row should not be null", row);
			AssertEquals("Header Type", typeof(DecInvoiceHeaderDataRow), row.GetType());
			DecInvoiceHeaderDataRow headerRow = (DecInvoiceHeaderDataRow)row;
			AssertEquals("Field Count", 8, headerRow.FieldCount);
			AssertEquals("Record Type", DecInvoiceHeaderDataRow.InvoiceHeaderCode, headerRow.RecordType);
			AssertEquals("Vessel Code", "EE415", headerRow.VesselCode);
			AssertEquals("Vessel Name", "MANON", headerRow.VesselName);
			AssertEquals("Voyage", "EE415", headerRow.Voyage);
			AssertEquals("Regional Allocation", "90130", headerRow.RegionalAllocation);
			AssertEquals("Count Of Vehicles", 1, headerRow.CountOfVehicles);
			AssertEquals("FOB Amount", 104042.00m, headerRow.FOBAmount);
			AssertEquals("Ocean Bill", "HBL1234567", headerRow.OceanBill);
			row = formatter.ConvertToRow("D1EE415 MANON               EE415 90130 000100010404200");
			AssertNotNull("Row should not be null", row);
			AssertEquals("Header Type", typeof(DecInvoiceHeaderDataRow), row.GetType());
			headerRow = (DecInvoiceHeaderDataRow)row;
			AssertEquals("Field Count", 8, headerRow.FieldCount);
			AssertEquals("Record Type", DecInvoiceHeaderDataRow.InvoiceHeaderCode, headerRow.RecordType);
			AssertEquals("Vessel Code", "EE415", headerRow.VesselCode);
			AssertEquals("Vessel Name", "MANON", headerRow.VesselName);
			AssertEquals("Voyage", "EE415", headerRow.Voyage);
			AssertEquals("Regional Allocation", "90130", headerRow.RegionalAllocation);
			AssertEquals("Count Of Vehicles", 1, headerRow.CountOfVehicles);
			AssertEquals("FOB Amount", 104042.00m, headerRow.FOBAmount);
			AssertEquals("Ocean Bill", ZString.Empty, headerRow.OceanBill);
			row = formatter.ConvertToRow("D1 blah blah");
			AssertNotNull("Row should not be null", row);
			row = formatter.ConvertToRow("D2060/037992     1490100152  92373193414122-AU1      9147 00010404200");
			AssertNotNull("Row should not be null", row);
			AssertEquals("Line Type", typeof(DecInvoiceLineDataRow), row.GetType());
			DecInvoiceLineDataRow lineRow = (DecInvoiceLineDataRow)row;
			AssertEquals("Field Count", 7, lineRow.FieldCount);
			AssertEquals("Record Type", DecInvoiceLineDataRow.InvoiceLineCode, lineRow.RecordType);
			AssertEquals("Invoice/Order No", "060/037992", lineRow.InvoiceOrderNo);
			AssertEquals("Commission No", "1490100152", lineRow.CommissionNo);
			AssertEquals("Chassis No", "923731", lineRow.ChassisNo);
			AssertEquals("Model", "93414122-AU1", lineRow.Model);
			AssertEquals("Colour", "9147", lineRow.Colour);
			AssertEquals("FOB Amount", 104042.00m, lineRow.FOBAmount);
			row = formatter.ConvertToRow("***EOF:DaimlerChrysler<20040623><130027>***");
			AssertEquals("Field Count", 1, row.FieldCount);
			AssertEquals("Footer", "***EOF:DaimlerChrysler<20040623><130027>***", row.GetField(0));
		}

		public static void TestConvertInvalidRow()
		{
			DCImportFlatFileFormat formatter = new DCImportFlatFileFormat();
			FlatFileDataRow row = formatter.ConvertToRow("xxEE415 MANON               EE415 90130 000100010404200HBL1234567                    ");
			AssertNull("Row should be null", row);
		}

		[ExpectException(typeof(NotImplementedException))]
		public void TestNoExportFunctionality()
		{
			DCImportFlatFileFormat formatter = new DCImportFlatFileFormat();
			formatter.ConvertToLine(new FlatFileDataRow(1));
		}

		public void TestFileExtensionForImport()
		{
			DCImportFlatFileFormat formatter = new DCImportFlatFileFormat();
			AssertEquals("File Extension For Import", FileExtensionType.Txt, formatter.FileExtensionForImport);
		}

		public void TestFileExtensionForExport()
		{
			DCImportFlatFileFormat formatter = new DCImportFlatFileFormat();
			AssertEquals("File Extension For Export", FileExtensionType.Txt, formatter.FileExtensionForExport);
		}
	}
}
