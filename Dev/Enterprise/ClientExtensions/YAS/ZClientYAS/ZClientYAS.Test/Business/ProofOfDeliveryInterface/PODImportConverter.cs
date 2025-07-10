using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.YAS.Testing;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.YAS.Business.ProofOfDeliveryInterface.Testing
{
	public class PODImportConverterTest : TestCaseWithFactory
	{
		public void TestMapImport_Declaration()
		{
			FlatFileDataRowCollection rows = new FlatFileDataRowCollection();
			rows.Add(new FlatFileDataRow(FileFormat.ConvertToRow(",,-")));
			Importer.MapImport(rows);
			AssertContains("Row not valid", "Row #1  is invalid.", TestHelper.Notifications.AsString);

			rows = new FlatFileDataRowCollection();
			rows.Add(new FlatFileDataRow(FileFormat.ConvertToRow("HouseBill,Pavlo,Completed,7.00,,,,VEH-987,SYSDBA,2009-03-06 11:13:37.0000,2009-03-05,,,,,,,")));
			Importer.MapImport(rows);
			AssertContains("Declaration not found", "Shipment or Declaration (HouseBill) is not found.", TestHelper.Notifications.AsString);

			TestHelper.Notifications.Clear();
			BaseJobDeclaration dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			dec.JE_HouseBill = "HouseBill";
			Importer.MapImport(rows);
			AssertContains("Declaration delivered", "Declaration  HouseBill is now delivered.\r\n", TestHelper.Notifications.AsString);

			Factory.Save();
			AssertEquals(new ZDateTime(2009, 3, 6, 11, 13, 37), dec.JE_CartageCompleted);
			ZQuery query = new ZQuery(StmALogSchema.SL_Parent, dec.PK);
			query.AddToFilter(JoinCondition.And, StmALogSchema.SL_SE_NKEvent, Events.DeliveryCartageCompleteFinalised.Code);
			StmALog[] log = Factory.Load<StmALog>(query);
			AssertEquals(1, log.Length);
			AssertEquals(new ZDateTime(2009, 3, 6, 11, 13, 37), log[0].SL_EventTime);
			AssertEquals("Signed By: Pavlo; Vehicle Number: VEH-987", log[0].SL_Reference);
		}

		public void TestMapImport_Shipment()
		{
			FlatFileDataRowCollection rows = new FlatFileDataRowCollection();
			rows.Add(new FlatFileDataRow(FileFormat.ConvertToRow(",,-")));
			Importer.MapImport(rows);
			AssertContains("Row not valid", "Row #1  is invalid.", TestHelper.Notifications.AsString);

			rows = new FlatFileDataRowCollection();
			rows.Add(new FlatFileDataRow(FileFormat.ConvertToRow("HouseBill,Dean,Completed,7.00,,,,VEH-987654321,SYSDBA,2009-03-06 11:13:37.0000,2009-03-05,,,,,,,")));
			Importer.MapImport(rows);
			AssertContains("Shipment not found", "Shipment or Declaration (HouseBill) is not found.", TestHelper.Notifications.AsString);

			TestHelper.Notifications.Clear();
			TestHelper.SetuupPODImport();
			Importer.MapImport(rows);
			AssertContains("Shipment delivered", "Shipment HouseBill is now delivered.", TestHelper.Notifications.AsString);

			Factory.Save();
			AssertEquals("Shipment delivery date", new ZDateTime(2009, 3, 6, 11, 13, 37), TestHelper.Shipment1.DocsAndCartage.JP_DeliveryCartageCompleted);
			AssertEquals("Delivery confirmation", 1, TestHelper.Shipment1.DeliveryConfirms.Count);
			AssertEquals("Signed in", "Dean", TestHelper.Shipment1.DeliveryConfirms[0].EU_GoodsSignForBy);
			AssertEquals("Vehicle rego", "VEH-9876", TestHelper.Shipment1.DeliveryConfirms[0].EU_VehicleRegistration);
			AssertEquals("Confirmation delivery date", new ZDateTime(2009, 3, 6, 11, 13, 37), TestHelper.Shipment1.DeliveryConfirms[0].EU_PickupDeliveryTime);
		}

		public void TestMapImport_ASMShipment()
		{
			FlatFileDataRowCollection rows = new FlatFileDataRowCollection();
			rows.Add(new FlatFileDataRow(FileFormat.ConvertToRow("HouseBill,Dean,Completed,7.00,,,,VEH-987654321,SYSDBA,2009-03-06 11:13:37.0000,2009-03-05,,,,,,,")));

			TestHelper.SetuupPODImport();
			TestHelper.Shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			AssertEquals("PRE: shipment does not have any packline", 0, TestHelper.Shipment1.OuterPackLines.Count);
			Importer.MapImport(rows);
			AssertContains("ASM Shipment not created", "The confirmation is not created as the type of shipment (HouseBill) is Assembly Master.", TestHelper.Notifications.AsString);
			AssertContains("ASM Shipment not created with packline error", "The confirmation is not created as the shipment (HouseBill) has no packing line.", TestHelper.Notifications.AsString);
			AssertEquals("Delivery confirmation", 0, TestHelper.Shipment1.DeliveryConfirms.Count);
		}

		public void TestMapImport_ShipmentWithoutPackLine()
		{
			FlatFileDataRowCollection rows = new FlatFileDataRowCollection();
			rows.Add(new FlatFileDataRow(FileFormat.ConvertToRow("HouseBill,Dean,Completed,7.00,,,,VEH-987654321,SYSDBA,2009-03-06 11:13:37.0000,2009-03-05,,,,,,,")));

			TestHelper.Shipment1.JS_HouseBill = "HouseBill";
			AssertEquals("PRE: shipment does not have any packline", 0, TestHelper.Shipment1.OuterPackLines.Count);
			Importer.MapImport(rows);
			AssertContains("ASM Shipment not created with packline error", "The confirmation is not created as the shipment (HouseBill) has no packing line.", TestHelper.Notifications.AsString);
			AssertEquals("Delivery confirmation", 0, TestHelper.Shipment1.DeliveryConfirms.Count);
		}

		public void TestMapImport_Shipment_RowExceedsMaxLength()
		{
			// Arrange
			const string signedIn = "VITTORIO VIN STEFANO BARROS";
			var rows = new FlatFileDataRowCollection
			{
				new FlatFileDataRow(FileFormat.ConvertToRow($"HouseBill,{signedIn},Completed,7.00,,,,VEH-987654321,SYSDBA,2009-03-06 11:13:37.0000,2009-03-05,,,,,,,"))
			};
			TestHelper.SetuupPODImport();
			TestHelper.Notifications.Clear();
			// Act & Assert
			AssertNoExceptionThrown(() => Importer.MapImport(rows));
			AssertContains("Shipment delivered", "Shipment HouseBill is now delivered.", TestHelper.Notifications.AsString);
			AssertEquals("Delivery confirmation", 1, TestHelper.Shipment1.DeliveryConfirms.Count);
			AssertEquals("Signed in", signedIn.Substring(0, JobPickupDeliveryConfirmSchema.EU_GoodsSignForBy.MaxLength), TestHelper.Shipment1.DeliveryConfirms[0].EU_GoodsSignForBy);
		}

		protected override void TearDown()
		{
			TestHelper.TidyUp();
			base.TearDown();
		}

		FlatFileFormat FileFormat
		{
			get { return (fileFormat) ?? (fileFormat = new CsvFlatFileFormat()); }
		}
		FlatFileFormat fileFormat;

		PODImportConverter Importer
		{
			get { return importer ?? (importer = new PODImportConverter(TestHelper.Notifications, Factory)); }
		}
		PODImportConverter importer;

		YASTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new YASTestHelper(Factory)); }
		}
		YASTestHelper testHelper;
	}
}
