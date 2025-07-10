using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.CustomerService.Business.Testing
{
	[TestedType(typeof(ConversationNoteCollection))]
	internal sealed class ConversationNoteCollectionTestCase : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ConversationNoteCollection(Factory.New<DummyEnterpriseBusinessObject>(), Factory);
		}

		public void TestIndexer()
		{
			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			ConversationNoteCollection collection = new ConversationNoteCollection(bizo, Factory);
			collection.AddNew();
			var note = collection[0];
			Assert("type is HiddenStmNote", note is HiddenStmNote);
		}

		public void TestLoadFilter()
		{
			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			CreateNote(bizo, StmNoteVisibility.PUB, ConversationNote.Description);
			CreateNote(bizo, StmNoteVisibility.PRV, ConversationNote.Description);
			CreateNote(bizo, StmNoteVisibility.INT, ConversationNote.Description);
			CreateNote(bizo, StmNoteVisibility.DOC);
			CreateNote(bizo, StmNoteVisibility.AGV, ConversationNote.Description);

			var noteWithText = Factory.New<ConversationNote>();
			noteWithText.ST_ParentID = bizo.PK;
			noteWithText.ST_Table = bizo.TableName;
			noteWithText.ST_NoteText = "<Message><SentTimeInUtc>2012-05-21 16:26:31.000</SentTimeInUtc><UserCode>US1</UserCode><UserName>User 1</UserName><MessageType>LIN</MessageType><MessageSubType>USR</MessageSubType><Body>hello</Body><AdditionalNote></AdditionalNote><Id>1111ffff-ffff-ffff-ffff-ffffffffffff</Id></Message>";

			var noteWithoutText = Factory.New<ConversationNote>();
			noteWithoutText.ST_ParentID = bizo.PK;
			noteWithoutText.ST_Table = bizo.TableName;

			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory(); // ensure results are from the db (not cached)
			DummyEnterpriseBusinessObject bizo2 = factory2.Load<DummyEnterpriseBusinessObject>(bizo.PK);
			ConversationNoteCollection notes = new ConversationNoteCollection(bizo2, factory2);
			notes.Load();

			AssertEquals("There should be 1 note.", 1, notes.Count);
			AssertEquals("ST_NoteType", nameof(StmNoteVisibility.DOC), notes[0].ST_NoteType);
		}

		#region Implementation

		StmNote CreateNote(BusinessObject parent, StmNoteVisibility visibility, string description = "")
		{
			StmNote note = Factory.New<StmNote>();

			note.ST_NoteType = visibility.ToString();
			note.ST_ParentID = parent.PK;
			note.ST_Table = parent.TableName;
			if (!string.IsNullOrEmpty(description))
			{
				note.ST_Description = description;
			}

			return note;
		}

		#endregion
	}
}
