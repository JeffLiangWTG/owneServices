using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public class UniqueNoteTest : TestCaseWithFactory
	{
		public void TestText()
		{
			EnterpriseBusinessObject bizo = Factory.NewWithValidTestData<OrgHeader>();
			UniqueNote note = new UniqueNote(bizo, PredefinedNoteTypes.Instance.SpecialInstructions);
			AssertEquals("", note.Text);
			note.Text = "foo";
			AssertEquals("foo", note.Text);
			note.Text = "x";
			AssertEquals("x", note.Text);
			note.Text = "";
			AssertEquals("", note.Text);
			note.Text = "bar";
			AssertEquals("bar", note.Text);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			EnterpriseBusinessObject bizoReloaded = factory2.Load<OrgHeader>(bizo.PK);
			UniqueNote noteReloaded = new UniqueNote(bizoReloaded, PredefinedNoteTypes.Instance.SpecialInstructions);
			AssertEquals("bar", noteReloaded.Text);
			noteReloaded.Text = "gum";
			AssertEquals("gum", noteReloaded.Text);
			noteReloaded.Text = "";
			factory2.Save();

			factory2 = new BusinessObjectFactory();
			bizoReloaded = factory2.Load<OrgHeader>(bizo.PK);
			noteReloaded = new UniqueNote(bizoReloaded, PredefinedNoteTypes.Instance.SpecialInstructions);
			AssertEquals("", noteReloaded.Text);
			AssertEquals("note was deleted", false, bizoReloaded.Notes.HasNotes);
		}

		public void TestSynchronise()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			UniqueNote note1 = new UniqueNote(org, PredefinedNoteTypes.Instance.SpecialInstructions);
			note1.Text = "Comment 1";

			var loadFactory = new BusinessObjectFactory();
			var loadedOrg = loadFactory.Load<OrgHeader>(org.PK);
			UniqueNote note2 = new UniqueNote(loadedOrg, PredefinedNoteTypes.Instance.SpecialInstructions);
			note2.Text = "Comment 2";
			loadFactory.Save();

			bool isModified = note1.Synchronise();
			Assert(isModified);
			AssertEquals("Comment 1", note1.Text); // only latest stays

			ZQuery query = new ZDBOnlyQuery(typeof(StmNote));
			query.AddToFilter(StmNoteSchema.ST_ParentID, org.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, org.TableName);
			var description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			query.AddToFilter(StmNoteSchema.ST_Description, description);
			StmNote[] notes = new BusinessObjectFactory().Load<StmNote>(query);
			AssertEquals(1, notes.Length);
		}

		public void TestEmptyBlob()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			var note = new UniqueNoteForTest(incident, EDIPredefinedNoteTypes.Instance.BusinessRequirements);
			note.Blob = ORtfTextUtil.TextToRtfBytes("blabla");

			var internalNote = note.Note;

			AssertEquals(false, note.Blob.IsEmpty);
			AssertEquals(false, internalNote.IsDeleted);

			note.Blob = ORtfTextUtil.EmptyRtfByteArray;
			AssertEquals(true, internalNote.IsDeleted);
		}

		class UniqueNoteForTest : UniqueNote
		{
			public UniqueNoteForTest(EnterpriseBusinessObject bizo, PredefinedNoteType noteType) : base(bizo, noteType)
			{
			}

			public StmNote Note
			{
				get { return note; }
			}
		}
	}
}
