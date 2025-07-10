using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.CustomerService.Business.Testing
{
	[TestedType(typeof(ConversationNote))]
	sealed class ConversationNoteTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var note = (ConversationNote)GetNewBusinessObject();
			AssertEquals("Conversation", note.ST_Description);
			Factory.Save();
			AssertEquals("Conversation", note.ST_Description);
			var factory2 = new BusinessObjectFactory();
			var noteInFactory2 = factory2.Load<ConversationNote>(note.PK);
			AssertEquals("Conversation", noteInFactory2.ST_Description);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var bizo = Factory.New<DummyEnterpriseBusinessObject>();
			var note = Factory.New<ConversationNote>();
			note.ST_ParentID = bizo.PK;
			note.ST_Table = bizo.TableName;
			note.ST_NoteText = "<Message />";
			bizo.Notes.Add(note);
			return note;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			StmNote note = (StmNote)base.GetNewBusinessObjectForDeleteTest(factory);
			note.ST_Table = "ConversationMessage";
			return note;
		}

		#endregion
	}
}
