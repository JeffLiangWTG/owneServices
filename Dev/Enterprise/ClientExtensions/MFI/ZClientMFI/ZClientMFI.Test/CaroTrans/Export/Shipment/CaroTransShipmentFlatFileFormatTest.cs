using System;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.MFI.CaroTrans.Testing
{
	public class CaroTransShipmentFlatFileFormatTest : FlatFileFormatTestCase
	{
		protected override FlatFileFormat GetFlatFileFormat()
		{
			return new CaroTransShipmentFlatFileFormat();
		}

		[ExpectException(typeof(NotImplementedException))]
		public void TestConvertToRow()
		{
			CaroTransShipmentFlatFileFormat format = new CaroTransShipmentFlatFileFormat();
			format.ConvertToRow("");
		}

		public void TestConvertToLine()
		{
			FlatFileDataRow row = new FlatFileDataRow(Constants.ShipmentRecord.FieldsCount);
			for (int i = 0; i < row.FieldCount; i++)
			{
				row[i] = i.ToString();
			}

			CaroTransShipmentFlatFileFormat format = new CaroTransShipmentFlatFileFormat();
			string line = format.ConvertToLine(row);
			AssertEquals("Formatted line", "0 ,1            ,2                   ,3                     ,4    ,5              ,6       ,7       ,8       ,9       ", line);
		}

		public void TestFileExtensionForImport()
		{
			CaroTransShipmentFlatFileFormat format = new CaroTransShipmentFlatFileFormat();
			AssertEquals("No file extension", FileExtensionType.None, format.FileExtensionForImport);
		}

		public void TestFileExtensionForExport()
		{
			CaroTransShipmentFlatFileFormat format = new CaroTransShipmentFlatFileFormat();
			AssertEquals("Client specific file extension", FileExtensionType.None, format.FileExtensionForExport);
		}
	}
}
