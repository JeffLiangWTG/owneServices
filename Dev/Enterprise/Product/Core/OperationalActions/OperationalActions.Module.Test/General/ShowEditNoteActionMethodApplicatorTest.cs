using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	[TestedType(typeof(ShowEditNoteActionMethodApplicator))]
	sealed class ShowEditNoteActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestViewNote()
		{
			ShowEditNoteActionMethodApplicator.ShownPopup = false;
			var applicator = new ShowEditNoteActionMethodApplicator(Settings, Factory);
			var log = new DummyOperationalActionSectionLog();
			Assert("Precondition", !ShowEditNoteActionMethodApplicator.ShownPopup);
			applicator.Apply(log, new[] { GetDataBusinessObject() });
			Assert(string.IsNullOrEmpty(log.MessagesString()));
			Assert(ShowEditNoteActionMethodApplicator.ShownPopup);
		}

		public void TestViewNoteNotFound()
		{
			var settings = Settings;
			settings.NoteDescription = "XYZ";
			var applicator = new ShowEditNoteActionMethodApplicator(settings, Factory);
			var log = new DummyOperationalActionSectionLog();
			applicator.Apply(log, new[] { GetDataBusinessObject() });
			AssertEquals("WARNING: No note found for DummyBizo.", log.MessagesString());
		}

		#region Implementation

		public ShowEditNoteActionMethodSettings Settings
		{
			get { return new ShowEditNoteActionMethodSettings { NoteDescription = "ABCDE" }; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ShowEditNoteActionMethodApplicator(Settings, Factory);
		}

		BusinessObject GetDataBusinessObject()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var note = dummy.Notes.AddNew();
			note.ST_Description = "ABCDE";
			return dummy;
		}

		#endregion
	}
}
