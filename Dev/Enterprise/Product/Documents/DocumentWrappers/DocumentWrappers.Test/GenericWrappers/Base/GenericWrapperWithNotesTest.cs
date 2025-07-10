using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Base.Testing
{
	public abstract class GenericWrapperWithNotesTest : GenericWrapperTest
	{
		public abstract void TestWrapperNotes();

		public virtual void TestWrapperNotesIncludingRelated()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GBLON";
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Honey Badger");
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, "Squirrel");
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.BookingNotes.Description, "Sciurus carolinensis");

			AssertEquals("Pre-condition: Expected no related business objects with notes attached to the shipment", 0, shipment.BusinessObjectsWithRelatedNotes.Length);

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			consignor.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Meerkats");
			consignor.Notes.AddNew(false, PredefinedNoteTypes.Instance.AgentNotes.Description, "Ferret");

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			consignee.Notes.AddNew(false, PredefinedNoteTypes.Instance.AgentNotes.Description, "Mustela putorius furo");
			consignee.Notes.AddNew(false, PredefinedNoteTypes.Instance.AdditionalSecurityInformation.Description, "Mustela putorius furo");

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
				AssertEquals("Expected two related business object with notes attached to the shipment", 2, shipment.BusinessObjectsWithRelatedNotes.Length);
				Assert("Expected shipment to be export", shipment.IsExport());

				BusinessObject[] relatedBOs = new BusinessObject[] { consignor, consignee };
				AssertContainsExactElementsInAnyOrder("Expected shipment's related parties should include both consignee and consignor", relatedBOs, shipment.BusinessObjectsWithRelatedNotes);

				NoteWrapperCollection noteWrapper = new NoteWrapperCollection(shipment, Factory);
				AssertEquals("Expected to find the 3 notes on the shipment itself ", 3, noteWrapper.Count);

				NoteWrapperCollection noteWrapperWithRelatedNotes = new NoteWrapperCollection(shipment, true, Factory);

				AssertEquals("Expected to only include the note from the shipment. Honey badger don't care about Meerkats", "Honey Badger", noteWrapper[PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description].Text);
				AssertContains("Expected to include the notes from the shipment.", "Squirrel", noteWrapperWithRelatedNotes[PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description].Text);
				AssertContains("Expected to include the notes from the shipment.", "Sciurus carolinensis", noteWrapperWithRelatedNotes[PredefinedNoteTypes.Instance.BookingNotes.Description].Text);

				NoteWrapper agentNotes = noteWrapper[PredefinedNoteTypes.Instance.AgentNotes.Description];
				AssertNull("Expected to be null as Notes only searches the top level business object, will not add notes from shipment's related business objects", agentNotes);

				agentNotes = noteWrapperWithRelatedNotes[PredefinedNoteTypes.Instance.AgentNotes.Description];
				AssertNotNull("Expected to include the note text from the related BOs", agentNotes.Text);
				AssertContains("Expected to include the note text from the related BOs.", "Ferret", agentNotes.Text);
				AssertContains("Expected to include the note text from the related BOs.", "Mustela putorius furo", agentNotes.Text);

				var additionalSecurityInformationNotes = noteWrapperWithRelatedNotes[PredefinedNoteTypes.Instance.AdditionalSecurityInformation.Description];
				AssertNotNull("Expected to include the note text from the related BOs", additionalSecurityInformationNotes.Text);
				AssertContains("Expected to include the note text from the related BOs.", "Mustela putorius furo", additionalSecurityInformationNotes.Text);

				NoteWrapper specialNotes = noteWrapper[PredefinedNoteTypes.Instance.SpecialInstructions.Description];
				AssertNull("Expected to be null as Notes only searches the top level business object, will not add notes from shipment's related business objects", specialNotes);

				specialNotes = noteWrapperWithRelatedNotes[PredefinedNoteTypes.Instance.SpecialInstructions.Description];
				AssertNotNull("Expected to have special instructions notes from related BOs", specialNotes.Text);
				AssertContains("Expected to include notes with an export context.", "Special Instructions for EXPORT only", specialNotes.Text);
				AssertNotContains("Expected to not include notes with an import context.", "Special Instructions for IMPORT only", specialNotes.Text);
			}
		}
	}
}
