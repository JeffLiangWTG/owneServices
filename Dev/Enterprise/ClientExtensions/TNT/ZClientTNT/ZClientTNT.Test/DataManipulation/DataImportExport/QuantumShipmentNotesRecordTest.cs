using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.Testing
{
	public class QuantumShipmentNotesRecordTest : ConsignmentNoteRecordTest
	{
		public override void TestFieldProperty()
		{
			QuantumShipmentNotesRecord record = new QuantumShipmentNotesRecord(DataString);
			AssertEquals("HouseBill", "940432180", record.HouseBill);
			record.HouseBill = " HouseBill1 232 ";
			AssertEquals("HouseBill", "HouseBill1 232", record.HouseBill);
			AssertEquals("ECN Number", "EX2456789012", record.ECN);
			record.ECN = " ECN Number ";
			AssertEquals("ECN Number", "ECN Number", record.ECN);
		}

		public void TestCreateShipmentNoteIfNotExists()
		{
			QuantumShipmentNotesRecord record = new QuantumShipmentNotesRecord(DataString);
			record.Description1 = "TEST NOTE LINE 1";
			record.Description2 = "TEST NOTE LINE 2";
			record.Description3 = "TEST NOTE LINE 3";
			ZQuery noteFilter = new ZQuery(StmNoteSchema.ST_Description, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description);
			int noteCount = Factory.GetDatabaseCount(typeof(StmNote), noteFilter);
			record.CreateShipmentNoteIfNotExists(Factory, null);
			AssertEquals("No new Note added as shipment with housebill'" + record.HouseBill + "' doesn't exist", noteCount, Factory.GetDatabaseCount(typeof(StmNote), noteFilter));
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = record.HouseBill;
			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_Description = "Pack Line Description";
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GoodsDescription = "Goods Description";
			declaration.JE_JS = shipment.PK;
			Factory.Save();
			AssertEquals("PreCondition: Shipment should not have any Detailed Goods Description Note", 0, shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description).Length);
			Assert("No short goods description.", shipment.JS_GoodsDescription.IsEmpty);
			record.CreateShipmentNoteIfNotExists(Factory, shipment);
			Factory.Save();
			AssertEquals("1 new Short Goods Description Note added to Shipment", true, !shipment.JS_GoodsDescription.IsEmpty);
			AssertEquals("1 new Detailed Goods Description Note added to Shipment", 1, shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description).Length);
			AssertEquals("1 new Note added", noteCount + 1, Factory.GetDatabaseCount(typeof(StmNote), noteFilter));
			AssertEquals("Pack Line Description", record.NoteText.SubstringSafe(0, 65), packLine.JL_Description);
			AssertEquals("Shipment Goods Description", record.NoteText.Left(shipment.JS_GoodsDescriptionInfo.MaxLength), shipment.JS_GoodsDescription);
			AssertEquals("Declaration Goods Description", record.NoteText.Left(JobDeclarationSchema.JE_GoodsDescription.MaxLength), declaration.JE_GoodsDescription);
		}

#region Implementation
		protected override IQDownBaseRecord GetRecord(ZString rawData)
		{
			return new QuantumShipmentNotesRecord(rawData);
		}

		protected override Type ExpectedRecordType
		{
			get
			{
				return typeof(QuantumShipmentNotesRecord);
			}
		}
#endregion
	}
}
