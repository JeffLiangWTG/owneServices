using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(NoteIndividualWrapper))]
	sealed class NoteIndividualWrapperTest : NoteWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			NoteWrapper emptywrapper = new NoteIndividualWrapper(note, Factory);
			AssertEquals("emptywrapper.Text", ZString.Empty, emptywrapper.Text);
			AssertEquals("emptywrapper.Description", ZString.Empty, emptywrapper.Description);
			AssertEquals("emptywrapper.CreatedDate", ZDateTime.Empty, emptywrapper.CreatedDate);
		}

		[TestDate(2011, 01, 25)]
		public override void TestWrapperMappingFull()
		{
			note.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			note.ST_NoteDataAsText = "This is the simplest form of notes wrapper, each note is returned and you have text and description. There is no amalgamation.";
			Factory.Save();

			NoteWrapper emptywrapper = new NoteIndividualWrapper(note, Factory);
			AssertEquals("emptywrapper.Text", "This is the simplest form of notes wrapper, each note is returned and you have text and description. There is no amalgamation.", emptywrapper.Text);
			AssertEquals("emptywrapper.CreatedDate", "Agent Notes", emptywrapper.Description);
			AssertEquals("emptywrapper.CreatedDate", new ZDateTime(2011, 01, 25), emptywrapper.CreatedDate);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new NoteIndividualWrapper(null, Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			note.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			note.ST_NoteDataAsText = "This is the simplest form of notes wrapper, each note is returned and you have text and description. There is no amalgamation.";

			return new NoteIndividualWrapper(note, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			note = Factory.New<StmNote>();
			note.ST_Table = "DummyBizo";
		}
		StmNote note;
	}
}
