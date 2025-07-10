using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	[TestedType(typeof(PODDataLoader))]
	class PODDataLoaderTest : DataLoadTestCase<PODDataLoader>
	{
		[ExpectException(typeof(FileNotFoundException))]
		public void TestValidationOfFile()
		{
			Loader.ImportPODData("non-existant file");
		}

		public void TestValidationOfContent()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("This is the header for the file.");
					sw.WriteLine("This is the first part - invalid text");
				}
				Loader.ImportPODData(testFileName.Filename);
				AssertEquals(2, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, Loader.RunCounters.RecsCreated);
				AssertEquals(0, Loader.RunCounters.RecsUpdated);
				AssertEquals(0, Loader.RunCounters.RecsExcluded);
				AssertEquals("PODS to Import = 1", Loader.Log[0]);
				AssertEquals("Unknown column heading : This is the header for the file.", Loader.Log[1]);
				AssertEquals("File Header information is incorrect. The import of POD data requires a specific .CSV format file.", Loader.Log[2]);
				AssertEquals("FileHeaderIsValid", false, Loader.FileHeaderIsValid);
				AssertEquals("\r\nT O T A L : PODS created = 0, PODS updated = 0, PODS excluded = 0\r\n", Loader.Log[3]);
				AssertEquals(4, Loader.Log.Count);
			}
		}

		public void TestValidationOfHeader()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(GetWrongHeader());
				}

				Loader.ImportPODData(testFileName.Filename);
				AssertEquals(1, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, Loader.RunCounters.RecsCreated);
				AssertEquals(0, Loader.RunCounters.RecsUpdated);
				AssertEquals(0, Loader.RunCounters.RecsExcluded);

				AssertEquals("PODS to Import = 0", Loader.Log[0]);
				AssertEquals("Unknown column heading : Wrong Header Info", Loader.Log[1]);
				AssertEquals("File Header information is incorrect. The import of POD data requires a specific .CSV format file.", Loader.Log[2]);
				AssertEquals("FileHeaderIsValid", false, Loader.FileHeaderIsValid);
				AssertEquals("\r\nT O T A L : PODS created = 0, PODS updated = 0, PODS excluded = 0\r\n", Loader.Log[3]);
				AssertEquals(4, Loader.Log.Count);
			}

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(GetCorrectHeader());
					sw.Flush();
				}

				Loader.ImportPODData(testFileName.Filename);
				AssertEquals(1, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, Loader.RunCounters.RecsCreated);
				AssertEquals(0, Loader.RunCounters.RecsUpdated);
				AssertEquals(0, Loader.RunCounters.RecsExcluded);

				AssertEquals("PODS to Import = 0", Loader.Log[0]);
				AssertEquals("Unknown column heading : Wrong Header Info", Loader.Log[1]);
				AssertEquals("File Header information is incorrect. The import of POD data requires a specific .CSV format file.", Loader.Log[2]);
				AssertEquals("\r\nT O T A L : PODS created = 0, PODS updated = 0, PODS excluded = 0\r\n", Loader.Log[3]);
				AssertEquals("PODS to Import = 0", Loader.Log[4]);
				AssertEquals("\r\nT O T A L : PODS created = 0, PODS updated = 0, PODS excluded = 0\r\n", Loader.Log[5]);
				AssertEquals(6, Loader.Log.Count);
			}
		}

		public void TestShipmentIsUpdated()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("JobNumber,DeliveryDateTime");
					sw.WriteLine("S00001001,20080202020202");
					sw.WriteLine("S00001002,20080303333333");
					sw.WriteLine("");
					sw.WriteLine("S00001003,20080404040404");
					sw.WriteLine("S00001004,18991231235959");
					sw.WriteLine("S00001005,19000101000001");
					sw.Flush();
				}

				Loader.ImportPODData(testFileName.Filename);

				AssertEquals(6, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, Loader.RunCounters.RecsCreated);
				AssertEquals(0, Loader.RunCounters.RecsUpdated);
				AssertEquals(4, Loader.RunCounters.RecsExcluded);
				AssertEquals(6, Loader.Log.Count);

				string expected =
@$"PODS to Import = 5
Row 3 ignored: Delivery Date Time '20080303333333' is invalid. The Delivery Date Time is mandatory and needs to be provided in the format 'yyyyMMddHHmmss'.
Row 4 ignored: Job # 'S00001003' could not be found in {Core.Constants.ProductName}.
Row 5 ignored: Delivery Date Time '18991231235959' is invalid. The Delivery Date Time is mandatory and needs to be provided in the format 'yyyyMMddHHmmss'.
Row 6 ignored: Job # 'S00001005' could not be found in {Core.Constants.ProductName}.

T O T A L : PODS created = 1, PODS updated = 0, PODS excluded = 4

";
				AssertMultilineEquals("Loader Log", expected, PODFlatFileDataImporter.GetLogMessage(Loader), '\n');

				ForwardingShipment updatedShipment1 = LoadShipment("S00001001");
				AssertNotNull("Expecting shipment S00001001 to be found", updatedShipment1);
				AssertEquals("Shipment Delivery Date", new ZDateTime(2008, 2, 2, 2, 2, 0), updatedShipment1.DocsAndCartage.JP_DeliveryCartageCompleted);

				ForwardingShipment updatedShipment2 = LoadShipment("S00001002");
				AssertNotNull("Expecting shipment S00001002 to be found", updatedShipment2);
				AssertEquals("Shipment Delivery Date", ZDateTime.Empty, updatedShipment2.DocsAndCartage.JP_DeliveryCartageCompleted);

				ForwardingShipment updatedShipment3 = LoadShipment("S00001003");
				AssertNull(updatedShipment3);

				ForwardingShipment updatedShipment4 = LoadShipment("S00001004");
				AssertNull(updatedShipment4);

				ForwardingShipment updatedShipment5 = LoadShipment("S00001005");
				AssertNull(updatedShipment5);
			}
		}

		public void TestShipmentIsUpdated_FCL()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("JobNumber,DeliveryDateTime");
					sw.WriteLine("S00001001,20080202020202");
					sw.WriteLine("S00001002,20080303333333");
					sw.WriteLine("");
					sw.WriteLine("S00001003,20080404040404");
					sw.WriteLine("S00001045,20080505050505");
					sw.Flush();
				}

				Loader.ImportPODData(testFileName.Filename);

				AssertEquals(5, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(3, Loader.RunCounters.RecsCreated);
				AssertEquals(0, Loader.RunCounters.RecsUpdated);
				AssertEquals(2, Loader.RunCounters.RecsExcluded);
				AssertEquals(4, Loader.Log.Count);

				ForwardingShipment updatedShipment1 = LoadShipment("S00001001");
				AssertNotNull("Expecting shipment S00001001 to be found", updatedShipment1);
				AssertEquals("Shipment Delivery Date", new ZDateTime(2008, 2, 2, 2, 2, 0), updatedShipment1.DocsAndCartage.JP_DeliveryCartageCompleted);

				ForwardingShipment updatedShipment2 = LoadShipment("S00001002");
				AssertNotNull("Expecting shipment S00001002 to be found", updatedShipment2);
				AssertEquals("Shipment Delivery Date", ZDateTime.Empty, updatedShipment2.DocsAndCartage.JP_DeliveryCartageCompleted);

				ForwardingShipment updatedShipment3 = LoadShipment("S00001003");
				AssertNull(updatedShipment3);

				Factory.Save();

				ForwardingShipment updatedShipment4 = LoadShipment("S00001045");
				AssertNotNull("Expecting shipment S00001045 to be found", updatedShipment4);
				AssertEquals("Shipment Delivery Date", new ZDateTime(2008, 5, 5, 5, 5, 0), updatedShipment4.DocsAndCartage.JP_DeliveryCartageCompleted);
			}
		}

		#region NoSplit

		public void TestShipmentIsUpdated_NoSplit_1ConfirmComplete()
		{
			Shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			Shipment1.JS_UniqueConsignRef = "S12345678";
			Shipment1.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Now;
			Shipment1.JS_OuterPacks = 10;
			Shipment1.JS_ActualWeight = 12m;
			Shipment1.JS_ActualVolume = 14m;
			CommonPickupDeliveryConfirm confirm = Shipment1.DeliveryConfirms.AddNew();
			confirm.EU_PickupDeliveryTime = ZDateTime.Now;
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("JobNumber,DeliveryDateTime");
					sw.WriteLine("S12345678,20080202020202");
					sw.Flush();
				}

				Loader.ImportPODData(testFileName.Filename);

				AssertEquals(2, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, Loader.RunCounters.RecsCreated);
				AssertEquals(0, Loader.RunCounters.RecsUpdated);
				AssertEquals(1, Loader.RunCounters.RecsExcluded);

				string expected =
@"PODS to Import = 1
Row 2 ignored: Shipment 'S12345678' is already complete.

T O T A L : PODS created = 0, PODS updated = 0, PODS excluded = 1

";
				AssertMultilineEquals("Loader Log", expected, PODFlatFileDataImporter.GetLogMessage(Loader), '\n');
				AssertEquals(3, Loader.Log.Count);
			}
		}

		public void TestShipmentIsUpdated_NoSplit_1ConfirmNotComplete()
		{
			Shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			Shipment1.JS_UniqueConsignRef = "S12345678";
			Shipment1.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;
			Shipment1.JS_OuterPacks = 10;
			Shipment1.JS_ActualWeight = 12m;
			Shipment1.JS_ActualVolume = 14m;
			CommonPickupDeliveryConfirm confirm = Shipment1.DeliveryConfirms.AddNew();
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("JobNumber,DeliveryDateTime");
					sw.WriteLine("S12345678,20080202020202");
					sw.Flush();
				}

				Loader.ImportPODData(testFileName.Filename);

				AssertEquals(2, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, Loader.RunCounters.RecsCreated);
				AssertEquals(1, Loader.RunCounters.RecsUpdated);
				AssertEquals(0, Loader.RunCounters.RecsExcluded);

				string expected =
@"PODS to Import = 1

T O T A L : PODS created = 0, PODS updated = 1, PODS excluded = 0

";
				AssertMultilineEquals("Loader Log", expected, PODFlatFileDataImporter.GetLogMessage(Loader), '\n');
				AssertEquals(2, Loader.Log.Count);

				ForwardingShipment updatedShipment1 = LoadShipment("S12345678");
				AssertNotNull("Expecting shipment S12345678 to be found", updatedShipment1);
				AssertEquals("Shipment Delivery Date", new ZDateTime(2008, 2, 2, 2, 2, 0), updatedShipment1.DocsAndCartage.JP_DeliveryCartageCompleted);
				AssertEquals(1, updatedShipment1.DeliveryConfirms.Count);
				AssertEquals(new ZDateTime(2008, 2, 2, 2, 2, 0), updatedShipment1.DeliveryConfirms[0].EU_PickupDeliveryTime);
			}
		}

		public void TestShipmentIsUpdated_NoSplit_1SplitConfirmNotComplete()
		{
			Shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			Shipment1.JS_UniqueConsignRef = "S12345678";
			Shipment1.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;
			Shipment1.JS_OuterPacks = 10;
			Shipment1.JS_ActualWeight = 12m;
			Shipment1.JS_ActualVolume = 14m;
			CommonPickupDeliveryConfirm confirm = Shipment1.DeliveryConfirms.AddNew();
			confirm.Divots[0].J8_PackagesDelivered = 5;
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("JobNumber,DeliveryDateTime");
					sw.WriteLine("S12345678,20080202020202");
					sw.Flush();
				}

				Loader.ImportPODData(testFileName.Filename);

				AssertEquals(2, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, Loader.RunCounters.RecsCreated);
				AssertEquals(1, Loader.RunCounters.RecsUpdated);
				AssertEquals(0, Loader.RunCounters.RecsExcluded);

				string expected =
@"PODS to Import = 1

T O T A L : PODS created = 1, PODS updated = 1, PODS excluded = 0

";
				AssertMultilineEquals("Loader Log", expected, PODFlatFileDataImporter.GetLogMessage(Loader), '\n');
				AssertEquals(2, Loader.Log.Count);

				ForwardingShipment updatedShipment1 = LoadShipment("S12345678");
				AssertNotNull("Expecting shipment S12345678 to be found", updatedShipment1);
				AssertEquals("Shipment Delivery Date", new ZDateTime(2008, 2, 2, 2, 2, 0), updatedShipment1.DocsAndCartage.JP_DeliveryCartageCompleted);
				AssertEquals(2, updatedShipment1.DeliveryConfirms.Count);
				CommonPickupDeliveryConfirm confirm1 = updatedShipment1.DeliveryConfirms[0];
				CommonPickupDeliveryConfirm confirm2 = updatedShipment1.DeliveryConfirms[1];

				AssertEquals(new ZDateTime(2008, 2, 2, 2, 2, 0), confirm1.EU_PickupDeliveryTime);
				AssertEquals(5, confirm1.Divots[0].J8_PackagesDelivered);
				AssertEquals(6m, confirm1.Divots[0].J8_DeliveryWeight);
				AssertEquals(7m, confirm1.Divots[0].J8_DeliveryVolume);

				AssertEquals(new ZDateTime(2008, 2, 2, 2, 2, 0), confirm2.EU_PickupDeliveryTime);
				AssertEquals(5, confirm2.Divots[0].J8_PackagesDelivered);
				AssertEquals(6m, confirm2.Divots[0].J8_DeliveryWeight);
				AssertEquals(7m, confirm2.Divots[0].J8_DeliveryVolume);
			}
		}

		#endregion

		#region Split

		public void TestShipmentIsUpdated_Split_1ConfirmComplete()
		{
			Shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			Shipment1.JS_UniqueConsignRef = "S12345678";
			Shipment1.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Now;
			Shipment1.JS_OuterPacks = 10;
			Shipment1.JS_ActualWeight = 12m;
			Shipment1.JS_ActualVolume = 14m;
			CommonPickupDeliveryConfirm confirm = Shipment1.DeliveryConfirms.AddNew();
			confirm.EU_PickupDeliveryTime = ZDateTime.Now;
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("JobNumber,DeliveryDateTime,Signature,DeliveredPacks,DeliveredWeight,DeliveredVolume");
					sw.WriteLine("S12345678,20080202020202,Bob,10,12,14");
					sw.Flush();
				}

				Loader.ImportPODData(testFileName.Filename);

				AssertEquals(2, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, Loader.RunCounters.RecsCreated);
				AssertEquals(0, Loader.RunCounters.RecsUpdated);
				AssertEquals(1, Loader.RunCounters.RecsExcluded);

				string expected =
@"PODS to Import = 1
Row 2 ignored: Shipment 'S12345678' is already complete.

T O T A L : PODS created = 0, PODS updated = 0, PODS excluded = 1

";
				AssertMultilineEquals("Loader Log", expected, PODFlatFileDataImporter.GetLogMessage(Loader), '\n');
				AssertEquals(3, Loader.Log.Count);
			}
		}

		public void TestShipmentIsUpdated_Split_1ConfirmNotComplete_Match()
		{
			Shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			Shipment1.JS_UniqueConsignRef = "S12345678";
			Shipment1.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;
			Shipment1.JS_OuterPacks = 10;
			Shipment1.JS_ActualWeight = 12m;
			Shipment1.JS_ActualVolume = 14m;
			CommonPickupDeliveryConfirm confirm = Shipment1.DeliveryConfirms.AddNew();
			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("JobNumber,DeliveryDateTime,Signature,DeliveredPacks,DeliveredWeight,DeliveredVolume");
					sw.WriteLine("S12345678,20080202020202,Bob,10,12,14");
					sw.Flush();
				}

				Loader.ImportPODData(testFileName.Filename);

				AssertEquals(2, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, Loader.RunCounters.RecsCreated);
				AssertEquals(1, Loader.RunCounters.RecsUpdated);
				AssertEquals(0, Loader.RunCounters.RecsExcluded);

				string expected =
@"PODS to Import = 1

T O T A L : PODS created = 0, PODS updated = 1, PODS excluded = 0

";
				AssertMultilineEquals("Loader Log", expected, PODFlatFileDataImporter.GetLogMessage(Loader), '\n');
				AssertEquals(2, Loader.Log.Count);

				ForwardingShipment updatedShipment1 = LoadShipment("S12345678");
				AssertNotNull("Expecting shipment S12345678 to be found", updatedShipment1);
				AssertEquals("Shipment Delivery Date", new ZDateTime(2008, 2, 2, 2, 2, 0), updatedShipment1.DocsAndCartage.JP_DeliveryCartageCompleted);
				AssertEquals(1, updatedShipment1.DeliveryConfirms.Count);
				CommonPickupDeliveryConfirm confirm1 = updatedShipment1.DeliveryConfirms[0];
				AssertEquals(new ZDateTime(2008, 2, 2, 2, 2, 0), confirm1.EU_PickupDeliveryTime);
				AssertEquals("Bob", confirm1.EU_GoodsSignForBy);
				AssertEquals(1, confirm1.Divots.Count);
				AssertEquals(10, confirm1.Divots[0].J8_PackagesDelivered);
				AssertEquals(12m, confirm1.Divots[0].J8_DeliveryWeight);
				AssertEquals(14m, confirm1.Divots[0].J8_DeliveryVolume);
			}
		}

		public void TestShipmentIsUpdated_Split_1ConfirmNotComplete_NoMatch()
		{
			Shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			Shipment1.JS_UniqueConsignRef = "S12345678";
			Shipment1.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;
			Shipment1.JS_OuterPacks = 10;
			Shipment1.JS_ActualWeight = 12m;
			Shipment1.JS_ActualVolume = 14m;
			CommonPickupDeliveryConfirm confirm = Shipment1.DeliveryConfirms.AddNew();
			confirm.Divots[0].J8_PackagesDelivered = 4;
			confirm.Divots[0].J8_DeliveryWeight = 5m;
			confirm.Divots[0].J8_DeliveryVolume = 6m;

			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("JobNumber,DeliveryDateTime,Signature,DeliveredPacks,DeliveredWeight,DeliveredVolume");
					sw.WriteLine("S12345678,20080202020202,Bob,6,7,8");
					sw.WriteLine("S12345678,20080203020202,Bill,4,5,6");
					sw.Flush();
				}

				Loader.ImportPODData(testFileName.Filename);

				AssertEquals(3, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(2, Loader.RunCounters.RecsCreated);
				AssertEquals(1, Loader.RunCounters.RecsUpdated);
				AssertEquals(0, Loader.RunCounters.RecsExcluded);

				string expected =
@"PODS to Import = 2

T O T A L : PODS created = 2, PODS updated = 1, PODS excluded = 0

";
				AssertMultilineEquals("Loader Log", expected, PODFlatFileDataImporter.GetLogMessage(Loader), '\n');
				AssertEquals(2, Loader.Log.Count);

				ForwardingShipment updatedShipment1 = LoadShipment("S12345678");
				AssertNotNull("Expecting shipment S12345678 to be found", updatedShipment1);
				AssertEquals("Shipment Delivery Date", new ZDateTime(2008, 2, 3, 2, 2, 0), updatedShipment1.DocsAndCartage.JP_DeliveryCartageCompleted);
				AssertEquals(3, updatedShipment1.DeliveryConfirms.Count);

				foreach (CommonPickupDeliveryConfirm con in updatedShipment1.DeliveryConfirms)
				{
					AssertEquals(1, con.Divots.Count);
					CommonConfirmDivot divot = con.Divots[0];
					if (new ZDateTime(2008, 2, 2, 2, 2, 0) == con.EU_PickupDeliveryTime && divot.J8_PackagesDelivered == 4)
					{
						AssertEquals("Bob", con.EU_GoodsSignForBy);
						AssertEquals(5m, divot.J8_DeliveryWeight);
						AssertEquals(6m, divot.J8_DeliveryVolume);
					}
					else if (new ZDateTime(2008, 2, 2, 2, 2, 0) == con.EU_PickupDeliveryTime && divot.J8_PackagesDelivered == 2)
					{
						AssertEquals("Bob", con.EU_GoodsSignForBy);
						AssertEquals(2m, divot.J8_DeliveryWeight);
						AssertEquals(2m, divot.J8_DeliveryVolume);
					}
					else if (new ZDateTime(2008, 2, 3, 2, 2, 0) == con.EU_PickupDeliveryTime)
					{
						AssertEquals("Bill", con.EU_GoodsSignForBy);
						AssertEquals(4, divot.J8_PackagesDelivered);
						AssertEquals(5m, divot.J8_DeliveryWeight);
						AssertEquals(6m, divot.J8_DeliveryVolume);
					}
					else
					{
						Assert("Could not find Confirm", false);
					}
				}
			}
		}

		public void TestShipmentIsUpdated_Split_1ConfirmNotComplete_NoMatch_HasLessRemainingPacks()
		{
			Shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			Shipment1.JS_UniqueConsignRef = "S12345678";
			Shipment1.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;
			Shipment1.JS_OuterPacks = 10;
			Shipment1.JS_ActualWeight = 12m;
			Shipment1.JS_ActualVolume = 14m;
			CommonPickupDeliveryConfirm confirm = Shipment1.DeliveryConfirms.AddNew();
			confirm.Divots[0].J8_PackagesDelivered = 4;
			confirm.Divots[0].J8_DeliveryWeight = 5m;
			confirm.Divots[0].J8_DeliveryVolume = 6m;

			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("JobNumber,DeliveryDateTime,Signature,DeliveredPacks,DeliveredWeight,DeliveredVolume");
					sw.WriteLine("S12345678,20080202020202,Bob,7,8,9");
					sw.Flush();
				}

				Loader.ImportPODData(testFileName.Filename);

				AssertEquals(2, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, Loader.RunCounters.RecsCreated);
				AssertEquals(1, Loader.RunCounters.RecsUpdated);
				AssertEquals(0, Loader.RunCounters.RecsExcluded);

				string expected =
@"PODS to Import = 1

T O T A L : PODS created = 1, PODS updated = 1, PODS excluded = 0

";
				AssertMultilineEquals("Loader Log", expected, PODFlatFileDataImporter.GetLogMessage(Loader), '\n');
				AssertEquals(2, Loader.Log.Count);

				ForwardingShipment updatedShipment1 = LoadShipment("S12345678");
				AssertNotNull("Expecting shipment S12345678 to be found", updatedShipment1);
				AssertEquals("Shipment Delivery Date", ZDateTime.Empty, updatedShipment1.DocsAndCartage.JP_DeliveryCartageCompleted);
				AssertEquals(2, updatedShipment1.DeliveryConfirms.Count);

				foreach (CommonPickupDeliveryConfirm con in updatedShipment1.DeliveryConfirms)
				{
					AssertEquals(1, con.Divots.Count);
					CommonConfirmDivot divot = con.Divots[0];
					if (new ZDateTime(2008, 2, 2, 2, 2, 0) == con.EU_PickupDeliveryTime && divot.J8_PackagesDelivered == 4)
					{
						AssertEquals("Bob", con.EU_GoodsSignForBy);
						AssertEquals(5m, divot.J8_DeliveryWeight);
						AssertEquals(6m, divot.J8_DeliveryVolume);
					}
					else if (new ZDateTime(2008, 2, 2, 2, 2, 0) == con.EU_PickupDeliveryTime && divot.J8_PackagesDelivered == 3)
					{
						AssertEquals("Bob", con.EU_GoodsSignForBy);
						AssertEquals(3m, divot.J8_DeliveryWeight);
						AssertEquals(3m, divot.J8_DeliveryVolume);
					}
					else
					{
						Assert("Could not find Confirm", false);
					}
				}
			}
		}

		public void TestShipmentIsUpdated_Split_1ConfirmNotComplete_NoMatch_NotEnoughRemainingPacks()
		{
			Shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			Shipment1.JS_UniqueConsignRef = "S12345678";
			Shipment1.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;
			Shipment1.JS_OuterPacks = 10;
			Shipment1.JS_ActualWeight = 12m;
			Shipment1.JS_ActualVolume = 14m;
			CommonPickupDeliveryConfirm confirm = Shipment1.DeliveryConfirms.AddNew();

			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("JobNumber,DeliveryDateTime,Signature,DeliveredPacks,DeliveredWeight,DeliveredVolume");
					sw.WriteLine("S12345678,20080202020202,Bob,7,8,9");
					sw.Flush();
				}

				Loader.ImportPODData(testFileName.Filename);

				AssertEquals(2, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, Loader.RunCounters.RecsCreated);
				AssertEquals(1, Loader.RunCounters.RecsUpdated);
				AssertEquals(0, Loader.RunCounters.RecsExcluded);

				string expected =
@"PODS to Import = 1

T O T A L : PODS created = 1, PODS updated = 1, PODS excluded = 0

";

				AssertMultilineEquals("Loader Log", expected, PODFlatFileDataImporter.GetLogMessage(Loader), '\n');
				AssertEquals(2, Loader.Log.Count);

				ForwardingShipment updatedShipment1 = LoadShipment("S12345678");
				AssertNotNull("Expecting shipment S12345678 to be found", updatedShipment1);
				AssertEquals("Shipment Delivery Date", ZDateTime.Empty, updatedShipment1.DocsAndCartage.JP_DeliveryCartageCompleted);
				AssertEquals(2, updatedShipment1.DeliveryConfirms.Count);

				foreach (CommonPickupDeliveryConfirm con in updatedShipment1.DeliveryConfirms)
				{
					AssertEquals(1, con.Divots.Count);
					CommonConfirmDivot divot = con.Divots[0];
					if (new ZDateTime(2008, 2, 2, 2, 2, 0) == con.EU_PickupDeliveryTime && divot.J8_PackagesDelivered == 10)
					{
						AssertEquals("Bob", con.EU_GoodsSignForBy);
						AssertEquals(12m, divot.J8_DeliveryWeight);
						AssertEquals(14m, divot.J8_DeliveryVolume);
					}
					else if (con.EU_PickupDeliveryTime.IsEmpty && divot.J8_PackagesDelivered == 3)
					{
						AssertEquals("", con.EU_GoodsSignForBy);
						AssertEquals(4m, divot.J8_DeliveryWeight);
						AssertEquals(5m, divot.J8_DeliveryVolume);
					}
					else
					{
						Assert("Could not find Confirm", false);
					}
				}
			}
		}
		#endregion

		public void TestDeclarationIsUpdated()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("JobNumber,DeliveryDateTime");
					sw.WriteLine("B00001001,20080202020202");
					sw.WriteLine("B00001002,20080303333333");
					sw.WriteLine("");
					sw.WriteLine("B00001003,20080404040404");
					sw.WriteLine("B00001004,17521231235959");
					sw.Flush();
				}

				Loader.ImportPODData(testFileName.Filename);

				AssertEquals(5, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, Loader.RunCounters.RecsCreated);
				AssertEquals(1, Loader.RunCounters.RecsUpdated);
				AssertEquals(3, Loader.RunCounters.RecsExcluded);

				string expected =
@$"PODS to Import = 4
Row 3 ignored: Delivery Date Time '20080303333333' is invalid. The Delivery Date Time is mandatory and needs to be provided in the format 'yyyyMMddHHmmss'.
Row 4 ignored: Job # 'B00001003' could not be found in {Core.Constants.ProductName}.
Row 5 ignored: Delivery Date Time '17521231235959' is invalid. The Delivery Date Time is mandatory and needs to be provided in the format 'yyyyMMddHHmmss'.

T O T A L : PODS created = 0, PODS updated = 1, PODS excluded = 3

";

				AssertMultilineEquals("Loader Log", expected, PODFlatFileDataImporter.GetLogMessage(Loader), '\n');
				AssertEquals(5, Loader.Log.Count);

				BaseJobDeclaration updatedDeclaration1 = LoadDeclaration("B00001001");
				AssertNotNull("Expecting declaration B00001001 to be found", updatedDeclaration1);
				AssertEquals("Delivery Date", new ZDateTime(2008, 2, 2, 2, 2, 0), updatedDeclaration1.JE_CartageCompleted);

				BaseJobDeclaration updatedDeclaration2 = LoadDeclaration("B00001002");
				AssertNotNull("Expecting declaration B00001002 to be found", updatedDeclaration2);
				AssertEquals("Delivery Date", ZDateTime.Empty, updatedDeclaration2.JE_CartageCompleted);

				BaseJobDeclaration updatedDeclaration3 = LoadDeclaration("B00001003");
				AssertNull(updatedDeclaration3);

				BaseJobDeclaration updatedDeclaration4 = LoadDeclaration("B00001004");
				AssertNull(updatedDeclaration4);
			}
		}

		public void TestMaximumLengthOfNotExceedForEU_GoodsSignForBy()
		{
			using (TempFile testFileName = TempFile.New())
			{
				int maxlengthOfGoodsSignForBy = JobPickupDeliveryConfirmSchema.EU_GoodsSignForBy.MaxLength;
				var longSignature = string.Join("", Enumerable.Repeat('A', maxlengthOfGoodsSignForBy + 10));
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("JobNumber,DeliveryDateTime,Signature,DeliveredPacks,DeliveredWeight,DeliveredVolume");
					sw.WriteLine("S00001001,20080202020202," + longSignature + ",10,12,14");
					sw.Flush();
				}

				Loader.ImportPODData(testFileName.Filename);

				var expectedGoodsSignForBy = longSignature.Substring(0, maxlengthOfGoodsSignForBy);
				ForwardingShipment updatedShipment1 = LoadShipment("S00001001");
				CommonPickupDeliveryConfirm confirm1 = updatedShipment1.DeliveryConfirms[0];
				AssertEquals(expectedGoodsSignForBy, confirm1.EU_GoodsSignForBy);
				AssertEquals("[S00001001] Goods Signature has been truncated to fit shipment limit of 25 characters. ", Loader.Log[1]);
			}
		}

		[ExpectNoExceptions]
		public void TestPackagesDeliveredToMinusIssue()
		{
			Shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			Shipment1.JS_UniqueConsignRef = "S12345678";
			Shipment1.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;
			Shipment1.JS_OuterPacks = 10;
			Shipment1.JS_ActualWeight = 12m;
			Shipment1.JS_ActualVolume = 14m;
			CommonPickupDeliveryConfirm confirm = Shipment1.DeliveryConfirms.AddNew();
			confirm.Divots[0].J8_PackagesDelivered = 12;
			confirm.Divots[0].J8_DeliveryWeight = 5m;
			confirm.Divots[0].J8_DeliveryVolume = 6m;

			Factory.Save();

			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("JobNumber,DeliveryDateTime,Signature,DeliveredPacks,DeliveredWeight,DeliveredVolume");
					sw.WriteLine("S12345678,20080202020202,Bob,-2,8,9");
					sw.Flush();
				}

				Loader.ImportPODData(testFileName.Filename);
			}
		}

		#region Implementation

		protected PODDataLoader Loader;

		protected ForwardingShipment Shipment1;
		protected ForwardingShipment Shipment2;
		protected ForwardingShipment ShipmentFCL1;
		protected BaseJobDeclaration Declaration1;
		protected BaseJobDeclaration Declaration2;

		CommonConsol consol1Dep;
		CommonConsol consol1Arv;
		CommonContainer container1;
		CommonContainer container2;
		CommonContainer container3;
		CommonContainer container4;
		ForwardingPackLine packline1;
		ForwardingPackLine packline2;

		protected override void SetUp()
		{
			base.SetUp();

			Shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			Shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			ShipmentFCL1 = Factory.NewWithValidTestData<ForwardingShipment>();

			ShipmentFCL1.JS_TransportMode = Constants.TransportModes.Sea;
			ShipmentFCL1.JS_PackingMode = Constants.ContainerModes.FCL;
			ShipmentFCL1.JS_RL_NKOrigin = "AUSYD";
			ShipmentFCL1.JS_RL_NKDestination = "SGSIN";

			Shipment1.JS_UniqueConsignRef = "S00001001";
			Shipment2.JS_UniqueConsignRef = "S00001002";
			ShipmentFCL1.JS_UniqueConsignRef = "S00001045";

			Shipment1.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;
			Shipment2.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;
			ShipmentFCL1.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;

			Shipment1.JS_OuterPacks = 10;
			Shipment1.JS_ActualWeight = 12m;
			Shipment1.JS_ActualVolume = 14m;
			Shipment2.JS_OuterPacks = 20;
			Shipment2.JS_ActualWeight = 24;
			Shipment2.JS_ActualVolume = 28;

			consol1Dep = ShipmentFCL1.Consols.AddNew();
			consol1Arv = ShipmentFCL1.Consols.AddNew();

			consol1Dep.JK_RL_NKLoadPort = "AUSYD";
			consol1Dep.JK_RL_NKDischargePort = "NZAKL";
			consol1Arv.JK_RL_NKLoadPort = "NZAKL";
			consol1Arv.JK_RL_NKLoadPort = "SGSIN";

			container1 = consol1Dep.Containers.AddNew();
			container2 = consol1Dep.Containers.AddNew();
			container3 = consol1Arv.Containers.AddNew();
			container4 = consol1Arv.Containers.AddNew();

			packline1 = ShipmentFCL1.OuterPackLines.AddNew();
			packline2 = ShipmentFCL1.OuterPackLines.AddNew();
			packline1.SetContainer(consol1Dep, container1);
			packline2.SetContainer(consol1Dep, container2);

			packline1.SetContainer(consol1Arv, container3);
			packline2.SetContainer(consol1Arv, container4);

			Declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();

			Declaration1.JE_DeclarationReference = "B00001001";
			Declaration2.JE_DeclarationReference = "B00001002";

			Declaration1.JE_CartageCompleted = ZDateTime.Empty;
			Declaration2.JE_CartageCompleted = ZDateTime.Empty;

			Factory.Save();

			Loader = new PODDataLoader();
		}

		protected override PODDataLoader GetNewDataLoader()
		{
			return new PODDataLoader();
		}

		protected virtual string GetWrongHeader()
		{
			return "Wrong Header Info";
		}

		protected virtual string GetCorrectHeader()
		{
			return "JobNumber,DeliveryDateTime";
		}

		ForwardingShipment LoadShipment(ZString jobNumber)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			return factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, jobNumber));
		}

		BaseJobDeclaration LoadDeclaration(ZString jobNumber)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			return factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, jobNumber));
		}

		#endregion
	}
}
