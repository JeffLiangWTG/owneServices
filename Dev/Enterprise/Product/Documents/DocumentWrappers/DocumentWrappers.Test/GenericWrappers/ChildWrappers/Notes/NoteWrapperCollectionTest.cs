using System;
using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(NoteWrapperCollection))]
	sealed class NoteWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<NoteWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new NoteIndividualWrapper(null, Factory);
		}

		protected override NoteWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new NoteWrapperCollection(null, Factory);
		}

		public void TestIndexer()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			StmNote note1 = shipment.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			note1.ST_NoteDataAsText = "This is where the note types are amalgamated into one.";

			StmNote note2 = shipment.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			note2.ST_NoteDataAsText = "This is the second note which should be combined with the first in the result from the wrapper.";

			StmNote note3 = shipment.Notes.AddNew();
			note3.ST_Description = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;
			note3.ST_NoteDataAsText = "This is a single note as some notes can only ever have one entered.";

			NoteWrapperCollection wrapperCollection = new NoteWrapperCollection(shipment, Factory);
			NoteWrapper wrapper = wrapperCollection[PredefinedNoteTypes.Instance.AgentNotes.Description];
			AssertNotNull("Indexer didn't return null", wrapper);
			AssertEquals("wrapper.Text", "This is where the note types are amalgamated into one.\r\nThis is the second note which should be combined with the first in the result from the wrapper.", wrapper.Text);
			AssertEquals("wrapper.Description", "Agent Notes", wrapper.Description);
			wrapper = wrapperCollection[PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description];
			AssertNotNull("Indexer didn't return null", wrapper);
			AssertEquals("wrapper.Text", "This is a single note as some notes can only ever have one entered.", wrapper.Text);
			AssertEquals("wrapper.Description", "Import Delivery Instructions", wrapper.Description);
			wrapper = wrapperCollection[PredefinedNoteTypes.Instance.BookingNotes.Description];
			AssertNull("Indexer didn't find anything", wrapper);
			wrapper = wrapperCollection["1"];
			AssertNotNull("Int Indexer", wrapper);
			AssertEquals("wrapper.Text", "This is where the note types are amalgamated into one.", wrapper.Text);
			AssertEquals("wrapper.Description", "Agent Notes", wrapper.Description);
		}

		public void TestNotesIncludingRelatedBusinessObjectsNotes()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;

			StmNote note1 = shipment.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			note1.ST_NoteDataAsText = "Main note.";

			StmNote note2 = consignor.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			note2.ST_NoteDataAsText = "Related note.";

			Factory.Save();

			AssertEquals("Pre-condition: consignor is in the related business objects", true, shipment.BusinessObjectsWithRelatedNotes.Contains(consignor));

			var wrapperCollection = new NoteWrapperCollection(shipment, true, Factory);
			var wrapper = wrapperCollection[PredefinedNoteTypes.Instance.SpecialInstructions.Description];
			AssertNotNull("Indexer didn't return null", wrapper);
			AssertEquals("Combined expected note ", "Main note.", wrapper.Text);
			AssertEquals("wrapper.Description", "Special Instructions", wrapper.Description);

			wrapper = wrapperCollection[PredefinedNoteTypes.Instance.SpecialInstructions.Description + ":All"];
			AssertNotNull("Indexer didn't return null", wrapper);
			AssertEquals("Combined expected note ", "Main note.\r\nRelated note.", wrapper.Text);
			AssertEquals("wrapper.Description", "Special Instructions", wrapper.Description);
		}

		public void TestItemsIndividually()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			StmNote note1 = shipment.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			note1.ST_NoteDataAsText = "Some agent notes stuff aint goin anywhere";

			StmNote note2 = shipment.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;
			note2.ST_NoteDataAsText = "Some delivery instructions that are really only for testing so the goods will never be deleted";

			StmNote note3 = shipment.Notes.AddNew();
			note3.ST_Description = PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description;
			note3.ST_NoteDataAsText = "EXDOC the best part of the system, ha, ha, ha";

			NoteWrapperCollection wrapperCollection = new NoteWrapperCollection(shipment, Factory);
			AssertEquals("wrapperCollection.Count", 3, wrapperCollection.Count);
			AssertEquals("wrapperCollection[0].Text", "Some agent notes stuff aint goin anywhere", wrapperCollection["1"].Text);
			AssertEquals("wrapperCollection[0].Text", "Agent Notes", wrapperCollection["1"].Description);
			AssertEquals("wrapperCollection[1].Text", "Some delivery instructions that are really only for testing so the goods will never be deleted", wrapperCollection["2"].Text);
			AssertEquals("wrapperCollection[1].Text", "Import Delivery Instructions", wrapperCollection["2"].Description);
			AssertEquals("wrapperCollection[2].Text", "EXDOC the best part of the system, ha, ha, ha", wrapperCollection["3"].Text);
			AssertEquals("wrapperCollection[2].Text", "EXDOC Letter Of Credit", wrapperCollection["3"].Description);
		}

		[TestDate]
		public void TestIndexerSuffixes()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			string noteDescription1 = PredefinedNoteTypes.Instance.OrderManagementUpdate.Description;
			string noteDescription2 = PredefinedNoteTypes.Instance.AgentNotes.Description;

			StmNote randomNote = CreateNote(shipment, noteDescription2, "hello", DateTime.Today.AddDays(-10));
			StmNote note1 = CreateNote(shipment, noteDescription1, "update note #1", DateTime.Today.AddDays(-3));
			StmNote note2 = CreateNote(shipment, noteDescription1, "update note #2", DateTime.Today.AddDays(-2));
			StmNote note3 = CreateNote(shipment, noteDescription1, "update note #3", DateTime.Today.AddDays(-1));

			NoteWrapperCollection wrapperCollection = new NoteWrapperCollection(shipment, Factory);

			AssertEquals("Suffix 'First' should return first update note.", note1.ST_NoteDataAsText, wrapperCollection[noteDescription1 + ":First"].Text);
			AssertEquals("Suffix 'Last' should return third update note.", note3.ST_NoteDataAsText, wrapperCollection[noteDescription1 + ":Last"].Text);
			AssertEquals("No suffix should return all update notes as a summary.", "update note #3\r\nupdate note #2\r\nupdate note #1", wrapperCollection[noteDescription1].Text);
			AssertEquals("Invalid suffix should return null.", null, wrapperCollection[noteDescription1 + ":FirstOrLastWithTypo"]);

			AssertEquals("Suffix 'First' should return the only one note.", "hello", wrapperCollection[noteDescription2 + ":First"].Text);
			AssertEquals("Suffix 'Last' should return the only one note.", "hello", wrapperCollection[noteDescription2 + ":Last"].Text);
			AssertEquals("No suffix should return the only one note as a summary.", "hello", wrapperCollection[noteDescription2].Text);

			AssertEquals("Non-existing note description and no suffix should return null.", null, wrapperCollection["Some Note Type"]);
			AssertEquals("Non-existing note description and suffix 'First' should return null.", null, wrapperCollection["Some Note Type:First"]);
			AssertEquals("Non-existing note description and suffix 'Last' should return null.", null, wrapperCollection["Some Note Type:Last"]);
		}

		public void TestAddNotes()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var booking = Factory.New<DtbBooking>();

			var note1 = shipment.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			note1.ST_NoteDataAsText = "This is where the note types are amalgamated into one.";

			var note2 = shipment.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			note2.ST_NoteDataAsText = "This is the second note which should be combined with the first in the result from the wrapper.";

			var note3 = shipment.Notes.AddNew();
			note3.ST_Description = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;
			note3.ST_NoteDataAsText = "This is a single note on a shipment.";

			var note4 = booking.Notes.AddNew();
			note4.ST_Description = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;
			note4.ST_NoteDataAsText = "This is a single note on a booking. Should replace the shipment one.";

			var wrapperCollection = new NoteWrapperCollection(booking, Factory);
			wrapperCollection.AddNotes(shipment);

			var wrapper = wrapperCollection[PredefinedNoteTypes.Instance.AgentNotes.Description];
			AssertNotNull("Indexer didn't return null", wrapper);
			AssertEquals("wrapper.Text", "This is where the note types are amalgamated into one.\r\nThis is the second note which should be combined with the first in the result from the wrapper.", wrapper.Text);
			AssertEquals("wrapper.Description", "Agent Notes", wrapper.Description);

			wrapper = wrapperCollection[PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description];
			AssertNotNull("Indexer didn't return null", wrapper);
			AssertEquals("wrapper.Text", "This is a single note on a booking. Should replace the shipment one.", wrapper.Text);
			AssertEquals("wrapper.Description", "Import Delivery Instructions", wrapper.Description);

			wrapper = wrapperCollection[PredefinedNoteTypes.Instance.BookingNotes.Description];
			AssertNull("Indexer didn't find anything", wrapper);
		}

		public void TestAddNotesWithRelatedBusinessObjects()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;

			var shipmentAgentNote1 = shipment.Notes.AddNew();
			shipmentAgentNote1.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			shipmentAgentNote1.ST_NoteDataAsText = "Note on the shipment for Agent.";
			shipmentAgentNote1.ST_NoteContext = "AAA";

			var shipmentAgentNote2 = shipment.Notes.AddNew();
			shipmentAgentNote2.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			shipmentAgentNote2.ST_NoteDataAsText = "Should not duplicate identical lines.";
			shipmentAgentNote2.ST_NoteContext = "AAB";

			var shipmentAgentNote3 = shipment.Notes.AddNew();
			shipmentAgentNote3.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			shipmentAgentNote3.ST_NoteDataAsText = "Should not duplicate identical lines.";
			shipmentAgentNote3.ST_NoteContext = "AAC";

			var shipmentDeliveryNote = shipment.Notes.AddNew();
			shipmentDeliveryNote.ST_Description = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;
			shipmentDeliveryNote.ST_NoteDataAsText = "Should not duplicate identical lines.";

			var consignorAgentNote = consignor.Notes.AddNew();
			consignorAgentNote.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			consignorAgentNote.ST_NoteDataAsText = "Not to be merged as it's attached to the consignor, not the shipment.";

			var consignorDeliveryNote1 = consignor.Notes.AddNew();
			consignorDeliveryNote1.ST_Description = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;
			consignorDeliveryNote1.ST_NoteDataAsText = "This is a single note on the consignor.";
			consignorDeliveryNote1.ST_NoteContext = "AAA";

			var consignorDeliveryNote2 = consignor.Notes.AddNew();
			consignorDeliveryNote2.ST_Description = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;
			consignorDeliveryNote2.ST_NoteDataAsText = "This note should not appear as the shipment note should take precendence over related parties notes.";
			consignorDeliveryNote2.ST_NoteContext = "AAB";

			Factory.Save();

			AssertEquals("Pre-condition: consignor is in the related business objects", true, shipment.BusinessObjectsWithRelatedNotes.Contains(consignor));

			var wrapperCollection = new NoteWrapperCollection(shipment, true, Factory);
			var wrapper = wrapperCollection[PredefinedNoteTypes.Instance.AgentNotes.Description];
			AssertNotNull("Indexer didn't return null", wrapper);
			AssertEquals("Expected the agent notes from the shipment", "Note on the shipment for Agent.\r\nShould not duplicate identical lines.", wrapper.Text);
			AssertEquals("wrapper.Description", "Agent Notes", wrapper.Description);

			wrapper = wrapperCollection[PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description];
			AssertNotNull("Expected to find note for Delivery Instructions", wrapper);
			AssertEquals("Expected the note from the shipment. Notes with the same text but not description are not duplicates.", "Should not duplicate identical lines.", wrapper.Text);
			AssertEquals("wrapper.Description", "Import Delivery Instructions", wrapper.Description);

			wrapper = wrapperCollection[PredefinedNoteTypes.Instance.BookingNotes.Description];
			AssertNull("Expected not to find any notes for Booking", wrapper);
		}

		public void TestAddingRichTextFormattedNotes()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var note1 = shipment.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			note1.ST_NoteDataAsText = "First Note";
			note1.ST_NoteContext = "AAA";

			var note2 = shipment.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			note2.ST_NoteDataAsText = "Second Note";
			note2.ST_NoteContext = "AAB";

			var wrapperCollection = new NoteWrapperCollection(shipment, true, Factory);
			var wrapper = wrapperCollection[PredefinedNoteTypes.Instance.InternalWorkNotes.Description];

			AssertNotNull(wrapper);
			AssertEquals(PredefinedNoteTypes.Instance.InternalWorkNotes.Description, wrapper.Description);
			AssertEquals("Expected text from both RTF based notes to appear on the wrapper", "First Note\r\nSecond Note", wrapper.Text);

			var note3 = shipment.Notes.AddNew();
			note3.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			note3.ST_NoteDataAsText = "Second Note";
			note3.ST_NoteContext = "AAC";

			var note4 = shipment.Notes.AddNew();
			note4.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			note4.ST_NoteDataAsText = "Another Note";
			note4.ST_NoteContext = "AAD";

			wrapperCollection = new NoteWrapperCollection(shipment, true, Factory);
			wrapper = wrapperCollection[PredefinedNoteTypes.Instance.InternalWorkNotes.Description];

			AssertNotNull(wrapper);
			AssertEquals(PredefinedNoteTypes.Instance.InternalWorkNotes.Description, wrapper.Description);
			AssertEquals("Expected 'Second Note' text not to be duplicated", "First Note\r\nSecond Note\r\nAnother Note", wrapper.Text);
		}

		public void TestAddNotesWithContext()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GBLON";

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;

			StmNote importInstructions = Factory.New<StmNote>();
			importInstructions.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			importInstructions.ST_NoteDataAsText = "Special Instructions for IMPORT only";
			importInstructions.ST_NoteContextDirection = nameof(StmNoteContextDirection.I);
			consignor.Notes.Add(importInstructions);

			StmNote exportInstructions = Factory.New<StmNote>();
			exportInstructions.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			exportInstructions.ST_NoteDataAsText = "Special Instructions for EXPORT only";
			exportInstructions.ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			consignor.Notes.Add(exportInstructions);

			shipment.Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				Assert("Pre-condition: Expected shipment to be export", shipment.IsExport());
				AssertEquals("Pre-condition: one related business object with notes attached to the shipment", 1, shipment.BusinessObjectsWithRelatedNotes.Length);

				NoteWrapperCollection noteWrapper = new NoteWrapperCollection(shipment, true, Factory);
				NoteWrapper specialNotes = noteWrapper[PredefinedNoteTypes.Instance.SpecialInstructions.Description];

				AssertNotNull("Expected to have special instructions notes from related BOs", specialNotes);
				AssertContains("Expected to include notes with an export context.", "Special Instructions for EXPORT only", specialNotes.Text);
				AssertNotContains("Expected to not include notes with an import context.", "Special Instructions for IMPORT only", specialNotes.Text);
			}
		}

		public void TestAutoRatingAuditLogNotesAreNotIncluded()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var note1 = shipment.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			note1.ST_NoteDataAsText = "Agent Notes are OK to include on documents.";

			var note2 = shipment.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description;
			note2.ST_NoteDataAsText = "Auto Rating Audit Logs should never be printed on documents.";

			var note3 = shipment.Notes.AddNew();
			note3.ST_Description = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;
			note3.ST_NoteDataAsText = "Delivery Instructions are OK to include on documents.";

			var wrapperCollection = new NoteWrapperCollection(shipment, Factory);
			AssertEquals("Collection contains 2 notes", 2, wrapperCollection.Count);

			var wrapper = wrapperCollection[PredefinedNoteTypes.Instance.AgentNotes.Description];
			AssertNotNull("Indexer didn't return null", wrapper);
			AssertEquals("wrapper.Text", "Agent Notes are OK to include on documents.", wrapper.Text);
			AssertEquals("wrapper.Description", "Agent Notes", wrapper.Description);

			wrapper = wrapperCollection[PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description];
			AssertNull("Indexer didn't find the AutoRatingAuditLog", wrapper);

			wrapper = wrapperCollection[PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description];
			AssertNotNull("Indexer didn't return null", wrapper);
			AssertEquals("wrapper.Text", "Delivery Instructions are OK to include on documents.", wrapper.Text);
			AssertEquals("wrapper.Description", "Import Delivery Instructions", wrapper.Description);
		}

		public void TestIndexerIsLanguageIndependent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var note = shipment.Notes.AddNew();
			note.ST_Description = "Detailed Goods Description";
			note.ST_NoteDataAsText = "Here is the description.";

			using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.German))
			{
				AssertNotEquals("Precondition: Note description should be localized", "Detailed Goods Description", note.ST_Description);
				var wrapperCollection = new NoteWrapperCollection(shipment, Factory);
				var wrapper = wrapperCollection["Detailed Goods Description"];
				AssertNotNull(wrapper);
				AssertEquals("wrapper.Text", "Here is the description.", wrapper.Text);
				AssertEquals("wrapper.Description", note.ST_Description, wrapper.Description);
			}
		}

		#region Implementation

		StmNote CreateNote(IStmNoteParent noteParent, string description, string text, DateTime createdDate)
		{
			TestDateAttribute.Date = createdDate;

			StmNote result = noteParent.Notes.AddNew();
			result.ST_Description = description;
			result.ST_NoteDataAsText = text;
			Factory.Save();

			return result;
		}

		#endregion
	}
}
