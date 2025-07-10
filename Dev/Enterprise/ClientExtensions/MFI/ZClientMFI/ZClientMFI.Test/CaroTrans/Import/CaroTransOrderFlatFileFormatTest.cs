using System;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.MFI.CaroTrans.Testing
{
	public class CaroTransOrderFlatFileFormatTest : FlatFileFormatTestCase
	{
		protected override FlatFileFormat GetFlatFileFormat()
		{
			return new CaroTransOrderFlatFileFormat();
		}

		public void TestConvertOrderDataToFlatFileDataRow()
		{
			CaroTransOrderFlatFileFormat formater = new CaroTransOrderFlatFileFormat();
			FlatFileDataRow row = formater.ConvertToRow("01,JERAUC0252002,A24200390,JER,AUC,20021209,BURNARD INTERNATIONAL,4,1438,2.9,20021211,20021216,NEW YORK,20021224,LOS ANGELES,COLUMBUS WAIKATO,694SB,TCKU9369958,20030112,20030127");
			AssertEquals("Record ID", "01", row.GetField(Constants.OrderRecord.RecordID));
			AssertEquals("Reference", "A24200390", row.GetField(Constants.OrderRecord.Reference));
			AssertEquals("BL Number", "JERAUC0252002", row.GetField(Constants.OrderRecord.BLNumber));
			AssertEquals("Consignee Name", "BURNARD INTERNATIONAL", row.GetField(Constants.OrderRecord.ConsigneeName));
			AssertEquals("Origin", "JER", row.GetField(Constants.OrderRecord.Origin));
			AssertEquals("Destination", "AUC", row.GetField(Constants.OrderRecord.Dest));
			AssertEquals("Book Date", "20021209", row.GetField(Constants.OrderRecord.BookDate));
			AssertEquals("Dock Receipts Pieces", "4", row.GetField(Constants.OrderRecord.DockReceiptsPieces));
			AssertEquals("Dock Receipts Weight", "1438", row.GetField(Constants.OrderRecord.DockReceiptsWeight));
			AssertEquals("Dock Receipts Cubic", "2.9", row.GetField(Constants.OrderRecord.DockReceiptsCubic));
			AssertEquals("Vessel Name", "COLUMBUS WAIKATO", row.GetField(Constants.OrderRecord.VesselName));
			AssertEquals("Voyage No", "694SB", row.GetField(Constants.OrderRecord.VoyageNo));
			AssertEquals("Container No", "TCKU9369958", row.GetField(Constants.OrderRecord.ContainerNo));
			AssertEquals("Dock Receipted", "20021211", row.GetField(Constants.OrderRecord.DockReceipted));
			AssertEquals("Repo Dispatch", "20021216", row.GetField(Constants.OrderRecord.RepoDispatch));
			AssertEquals("Repo Arrive", "20021224", row.GetField(Constants.OrderRecord.RepoArrive));
			AssertEquals("On Board Date", "20030112", row.GetField(Constants.OrderRecord.OnBoardDate));
			AssertEquals("Arrival Date", "20030127", row.GetField(Constants.OrderRecord.ArrivalDate));
		}

		public void TestFileExtensionType()
		{
			CaroTransOrderFlatFileFormat formater = new CaroTransOrderFlatFileFormat();
			AssertEquals("File Extension", FileExtensionType.ClientSpecific, formater.FileExtensionForImport);
		}

		public void TestClientSpecificExtension()
		{
			CaroTransOrderFlatFileFormat formatter = new CaroTransOrderFlatFileFormat();
			AssertEquals("Client Specific File Extension", Constants.CaroTransOrderClientSpecificFileExtension, formatter.ClientSpecificExtensionForTesting);
		}

		[ExpectException(typeof(NotImplementedException))]
		public void TestNoExportFunctionality()
		{
			CaroTransOrderFlatFileFormat formater = new CaroTransOrderFlatFileFormat();
			formater.ConvertToLine(new FlatFileDataRow(1));
		}
	}
}
