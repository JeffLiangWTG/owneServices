using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(NoteSummaryWrapper))]
	sealed class NoteSummaryWrapperTest : NoteWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			List<StmNote> notes = new List<StmNote>();
			NoteSummaryWrapper wrapper = new NoteSummaryWrapper(notes.ToArray(), Factory);
			AssertEquals("wrapper.Description", ZString.Empty, wrapper.Description);
			AssertEquals("wrapper.Text", ZString.Empty, wrapper.Text);
			AssertEquals("wrapper.CreatedDate", ZDateTime.Empty, wrapper.CreatedDate);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new NoteSummaryWrapper(null, Factory);
		}

		[TestDate]
		public override void TestWrapperMappingFull()
		{
			DateTime testDate1 = DateTime.Today.AddDays(-10);
			TestDateAttribute.Date = testDate1;
			StmNote note1 = Factory.New<StmNote>();
			note1.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			note1.ST_NoteDataAsText = "This is where the note types are amalgamated into one.";
			note1.ST_Table = "DummyBizo";
			Factory.Save();

			DateTime testDate2 = DateTime.Today.AddDays(-7);
			TestDateAttribute.Date = testDate2;
			StmNote note2 = Factory.New<StmNote>();
			note2.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			note2.ST_NoteDataAsText = "This is the second note which should be combined with the first in the result from the wrapper.";
			note2.ST_Table = "DummyBizo";
			Factory.Save();

			List<StmNote> notes = new List<StmNote>();
			notes.Add(note1);
			notes.Add(note2);

			NoteSummaryWrapper wrapper = new NoteSummaryWrapper(notes.ToArray(), Factory);
			AssertEquals("wrapper.Description", "Agent Notes", wrapper.Description);
			AssertEquals("wrapper.Text", "This is where the note types are amalgamated into one.\r\nThis is the second note which should be combined with the first in the result from the wrapper.", wrapper.Text);
			AssertEquals("wrapper.Description", testDate2, wrapper.CreatedDate);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			List<StmNote> notes = new List<StmNote>();
			return new NoteSummaryWrapper(notes.ToArray(), Factory);
		}
	}
}
