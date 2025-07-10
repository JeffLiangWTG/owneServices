using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(WorkflowStmNote))]
	class WorkflowStmNoteTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetOrCreateForParent()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Jimminy Jillikers");
			var note = WorkflowStmNote.GetForParent(workflow);
			AssertNull("Should not create note until explicitly called for", note);

			note = WorkflowStmNote.GetOrCreateForParent(workflow);

			AssertNotNull(note);
			AssertEquals(workflow.PK, note.ST_ParentID);
			AssertEquals(workflow, note.Parent);
			AssertEquals(ProcessHeader.Schema.TableName, note.ST_Table);
			AssertEquals(WorkflowStmNote.WorkflowNoteType, note.ST_NoteType);
			AssertEquals(WorkflowStmNote.WorkflowNoteType, note.ST_Description);
			AssertEquals(true, note.ST_IsCustomDescription);

			note.ST_NoteDataAsText = "Booo";

			Factory.Save();
			AssertEquals(false, note.IsDeleted);

			var loadedWorkflow = Factory.CreateNewFactory().Load<ProcessHeader>(workflow.PK);
			var loadedNote = WorkflowStmNote.GetForParent(loadedWorkflow);

			AssertNotNull(loadedNote);
		}

		public void TestSave_WhenEmpty_ShouldDelete()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Jimminy Jillikers");
			var note = workflow.WorkflowNote;

			note.ST_NoteDataAsText = "Blaaaaargh";

			Factory.Save();
			AssertEquals(false, note.IsDeleted);

			note.ST_NoteDataAsText = ZString.Empty;

			Factory.Save();
			AssertEquals(true, note.IsDeleted);

			var newNote = workflow.WorkflowNote;
			AssertNotEquals("Should re-create note", note, newNote);
			AssertEquals(false, newNote.IsDeleted);
		}

		public void TestReloadedNote()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Jimminy Jillikers");
			var note = workflow.WorkflowNote;

			note.ST_NoteDataAsText = "Blaaaaargh";

			Factory.Save();

			var reloadedWorkflow = Factory.CreateNewFactory().Load<ProcessHeader>(workflow.PK);
			AssertEquals("Blaaaaargh", reloadedWorkflow.WorkflowNote.ST_NoteDataAsText);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(factory, "Workflow");
			workflow.WorkflowNote.ST_NoteDataAsText = "Dat Note";

			return workflow.WorkflowNote;
		}
	}
}
