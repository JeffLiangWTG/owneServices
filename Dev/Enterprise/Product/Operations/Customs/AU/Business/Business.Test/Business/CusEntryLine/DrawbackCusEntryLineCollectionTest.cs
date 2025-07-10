using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DrawbackCusEntryLineCollection))]
	public class DrawbackCusEntryLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			InitialiseGlobals();
			return DrawbackCusEntryLineCollection.CreateDrawbackCusEntryLineCollection(line1, Factory);
		}

		public void TestLoadFromNote()
		{
			StmNote drawbackNote = Factory.New<StmNote>();
			drawbackNote.ST_ParentID = line1.PK;
			drawbackNote.ST_Table = "JOBCOMINVOICELINE";
			drawbackNote.ST_Description = DrawbackCusEntryLineCollection.DrawbackEntryLinesNoteDescription;
			drawbackNote.ST_NoteType = nameof(StmNoteVisibility.DOC);
			drawbackNote.ST_NoteDataAsText = "ENTRY1*1*5,ENTRY2*5*40";
			Factory.Save();

			drawbackCusEntryLineCollection = DrawbackCusEntryLineCollection.CreateDrawbackCusEntryLineCollection(line1, Factory);
			AssertEquals("2 lines loaded", 2, drawbackCusEntryLineCollection.Count);
			AssertEquals("Collection line 1 entry number", "ENTRY1", drawbackCusEntryLineCollection[0].Header.EntryNumber);
			AssertEquals("Collection line 1 entry line", (ZShort)1, drawbackCusEntryLineCollection[0].CL_LineNumber);
			AssertEquals("Collection line 1 claim quantity", (ZDecimal)5, drawbackCusEntryLineCollection[0].DrawbackClaimQuantity);
			AssertEquals("Collection line 2 entry number", "ENTRY2", drawbackCusEntryLineCollection[1].Header.EntryNumber);
			AssertEquals("Collection line 2 entry line", (ZShort)5, drawbackCusEntryLineCollection[1].CL_LineNumber);
			AssertEquals("Collection line 2 claim quantity", (ZDecimal)40, drawbackCusEntryLineCollection[1].DrawbackClaimQuantity);
		}

		public void TestSaveToNote()
		{
			drawbackCusEntryLineCollection = line1.DrawbackCusEntryLineCollection;
			drawbackCusEntryLineCollection.Add(referenceDecEntryLine1);
			referenceDecEntryLine1.DrawbackClaimQuantity = 123m;
			drawbackCusEntryLineCollection.Add(referenceDecEntryLine2);
			referenceDecEntryLine2.DrawbackClaimQuantity = 456m;
			Factory.Save();

			ZQuery filter = new ZQuery(StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.DOC));
			filter.AddToFilter(StmNoteSchema.ST_Description, DrawbackCusEntryLineCollection.DrawbackEntryLinesNoteDescription);
			filter.AddToFilter(StmNoteSchema.ST_ParentID, line1.PK);
			filter.AddToFilter(StmNoteSchema.ST_Table, line1.TableName);
			StmNote drawbackNote = Factory.LoadTop1<StmNote>(filter);
			AssertNotNull(drawbackNote);
			AssertEquals("SaveToNote", "ENTRY2*5*123,ENTRY1*1*456", drawbackNote.ST_NoteDataAsText);
		}

		public void TestSaveToNoteNull()
		{
			drawbackCusEntryLineCollection = line1.DrawbackCusEntryLineCollection;
			referenceDecEntryLine1.DrawbackClaimQuantity = 123m;
			drawbackCusEntryLineCollection.Add(referenceDecEntryLine1);
			referenceDecEntryLine2.DrawbackClaimQuantity = 456m;
			drawbackCusEntryLineCollection.Add(referenceDecEntryLine2);
			Factory.Save();

			ZQuery filter = new ZQuery(StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.DOC));
			filter.AddToFilter(StmNoteSchema.ST_Description, DrawbackCusEntryLineCollection.DrawbackEntryLinesNoteDescription);
			filter.AddToFilter(StmNoteSchema.ST_ParentID, line1.PK);
			filter.AddToFilter(StmNoteSchema.ST_Table, line1.TableName);
			StmNote drawbackNote = Factory.LoadTop1<StmNote>(filter);
			AssertNotNull(drawbackNote);

			drawbackCusEntryLineCollection.RemoveAll();
			Factory.Save();
			AssertEquals("No Notes", 0, line1.Notes.GetAllNotes().Count);

			drawbackNote = Factory.LoadTop1<StmNote>(filter);
			AssertNull(drawbackNote);
		}

		public void TestEmptyCollectionHasNoNote()
		{
			ZQuery filter = new ZQuery(StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.DOC));
			filter.AddToFilter(StmNoteSchema.ST_Description, DrawbackCusEntryLineCollection.DrawbackEntryLinesNoteDescription);
			filter.AddToFilter(StmNoteSchema.ST_Table, JobComInvoiceLine.Schema.TableName);
			filter.AddToFilter(StmNoteSchema.ST_ParentID, line1.PK);

			var drawbackNotes = Factory.Load<StmNote>(filter);
			AssertEquals("Precondition No Notes.", 0, drawbackNotes.Length);

			var collection = DrawbackCusEntryLineCollection.CreateDrawbackCusEntryLineCollection(line1, Factory);
			AssertEquals("Empty collection.", 0, collection.Count);
			AssertEquals("Has changes.", false, collection.HasChanges);
			Factory.Save();

			drawbackNotes = Factory.Load<StmNote>(filter);
			AssertEquals("No Notes for empty collection.", 0, drawbackNotes.Length);

			referenceDecEntryLine1.DrawbackClaimQuantity = 123m;
			collection.Add(referenceDecEntryLine1);
			AssertEquals("Empty collection.", 1, collection.Count);
			AssertEquals("Has changes.", true, collection.HasChanges);
			collection.SaveToNote();
			Factory.Save();

			drawbackNotes = Factory.Load<StmNote>(filter);
			AssertEquals("Note is in Database.", 1, drawbackNotes.Length);

			collection.Remove(referenceDecEntryLine1);
			AssertEquals("Empty collection.", 0, collection.Count);
			collection.SaveToNote();

			Factory.Save();

			drawbackNotes = Factory.Load<StmNote>(filter);
			AssertEquals("No Notes for empty collection.", 0, drawbackNotes.Length);
		}

		#region Implementation

		JobDeclaration testDec;
		JobComInvoiceHeader invoice1;
		JobComInvoiceLine line1;
		DrawbackCusEntryLineCollection drawbackCusEntryLineCollection;
		JobDeclaration referenceDec;
		CusEntryHeader referenceDecEntryHeader1;
		CusEntryHeader referenceDecEntryHeader2;
		CusEntryLine referenceDecEntryLine1;
		CusEntryLine referenceDecEntryLine2;

		protected override void SetUp()
		{
			base.SetUp();
			InitialiseGlobals();
		}

		protected void InitialiseGlobals()
		{
			testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			invoice1 = testDec.Invoices.AddNew();
			line1 = invoice1.JobComInvoiceLines.AddNew();
			referenceDec = JobDeclaration.New(Factory);
			referenceDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			referenceDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			referenceDecEntryHeader1 = referenceDec.CustomsEntryHeaders.AddNew();
			referenceDecEntryHeader1.EntryNumber = "ENTRY2";
			referenceDecEntryLine1 = referenceDecEntryHeader1.MergedLines.AddNew();
			referenceDecEntryLine1.CL_LineNumber = 5;
			referenceDecEntryHeader2 = referenceDec.CustomsEntryHeaders.AddNew();
			referenceDecEntryHeader2.EntryNumber = "ENTRY1";
			referenceDecEntryLine2 = referenceDecEntryHeader2.MergedLines.AddNew();
			referenceDecEntryLine2.CL_LineNumber = 1;
		}
		#endregion
	}
}
