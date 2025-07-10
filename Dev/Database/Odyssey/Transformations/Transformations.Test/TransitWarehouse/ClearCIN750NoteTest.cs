using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(ClearCIN750Note))]
	public class ClearCIN750NoteTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();

			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK) { WW_TransitSecurityProcessingRequired = false }.WithDockDoor(TestConnection);

			var rcn1 = new WhsItemReceiveConsignment(whs, "RCN1", "RCN1", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var cinNoteForRCN = new StmNote(rcn1.PK, "WhsItemReceiveConsignment") { ST_Description = "CIN 750 Message Notes", ST_NoteText = "CIN 750 Note For RCN1", ST_NoteContext = "AAA" }.AppendInsertAndReturnObject(sql);
			cinNoteForRCNPK = cinNoteForRCN.PK;

			var rcn2 = new WhsItemReceiveConsignment(whs, "RCN2", "RCN2", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var otherNoteForRCN = new StmNote(rcn2.PK, "WhsItemReceiveConsignment") { ST_Description = "Other Notes", ST_NoteText = "Other Note For RCN2", ST_NoteContext = "AAA" }.AppendInsertAndReturnObject(sql);
			otherNoteForRCNPK = otherNoteForRCN.PK;

			var dcn1 = new WhsItemDispatchConsignment(whs, "DCN1", "DCN1", "STD").AppendInsertAndReturnObject(sql);
			var cinNoteForDCN = new StmNote(dcn1.PK, "WhsItemDispatchConsignment") { ST_Description = "CIN 750 Message Notes", ST_NoteText = "CIN 750 Note For DCN1", ST_NoteContext = "AAA" }.AppendInsertAndReturnObject(sql);
			cinNoteForDCNPK = cinNoteForDCN.PK;

			var dcn2 = new WhsItemDispatchConsignment(whs, "DCN2", "DCN2", "STD").AppendInsertAndReturnObject(sql);
			var otherNoteForDCN = new StmNote(dcn2.PK, "WhsItemDispatchConsignment") { ST_Description = "Other Notes", ST_NoteText = "Other Note For DCN2", ST_NoteContext = "AAA" }.AppendInsertAndReturnObject(sql);
			otherNoteForDCNPK = otherNoteForDCN.PK;

			var jobShipment1 = new JobShipment("JS1").AppendInsertAndReturnObject(sql);
			var cinNoteForJS = new StmNote(jobShipment1.PK, "JobShipment") { ST_Description = "CIN 750 Message Notes", ST_NoteText = "CIN 750 Note For JS1", ST_NoteContext = "AAA" }.AppendInsertAndReturnObject(sql);
			cinNoteForJSPK = cinNoteForJS.PK;

			var jobShipment2 = new JobShipment("JS2").AppendInsertAndReturnObject(sql);
			var otherNoteForJS = new StmNote(jobShipment2.PK, "JobShipment") { ST_Description = "Other Notes", ST_NoteText = "Other Note For JS2", ST_NoteContext = "AAA" }.AppendInsertAndReturnObject(sql);
			otherNoteForJSPK = otherNoteForJS.PK;

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override void AssertTransformationResults()
		{
			StmNote.AssertFromDB(TestConnection, cinNoteForRCNPK)
				.ExpectEquals("ST_Description", note => note.ST_Description, "CIN 750 Message Notes")
				.ExpectEquals("ST_NoteText", note => note.ST_NoteText, "CIN 750 Note For RCN1")
				.VerifyAll();

			StmNote.AssertFromDB(TestConnection, otherNoteForRCNPK)
				.ExpectEquals("ST_Description", note => note.ST_Description, "Other Notes")
				.ExpectEquals("ST_NoteText", note => note.ST_NoteText, "Other Note For RCN2")
				.VerifyAll();

			StmNote.AssertFromDB(TestConnection, cinNoteForDCNPK)
				.ExpectEquals("ST_Description", note => note.ST_Description, "CIN 750 Message Notes")
				.ExpectEquals("ST_NoteText", note => note.ST_NoteText, "CIN 750 Note For DCN1")
				.VerifyAll();

			StmNote.AssertFromDB(TestConnection, otherNoteForDCNPK)
				.ExpectEquals("ST_Description", note => note.ST_Description, "Other Notes")
				.ExpectEquals("ST_NoteText", note => note.ST_NoteText, "Other Note For DCN2")
				.VerifyAll();

			StmNote.AssertFromDB(TestConnection, otherNoteForJSPK)
				.ExpectEquals("ST_Description", note => note.ST_Description, "Other Notes")
				.ExpectEquals("ST_NoteText", note => note.ST_NoteText, "Other Note For JS2")
				.VerifyAll();

			AssertEquals(0, StmNote.CountInDB(TestConnection, note => note.PK == cinNoteForJSPK));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new ClearCIN750Note();

		Guid cinNoteForRCNPK;
		Guid otherNoteForRCNPK;
		Guid cinNoteForDCNPK;
		Guid otherNoteForDCNPK;
		Guid cinNoteForJSPK;
		Guid otherNoteForJSPK;
	}
}
