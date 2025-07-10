using Enterprise.Client.EDI.Business.Test;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(FreightWrapperFromProject))]
	internal sealed class FreightWrapperFromProjectTest : FreightWrapperEDITest<EDIProject>
	{
		protected override GenericWrapper GetNewFreightWrapperCore()
		{
			return new FreightWrapperFromProject(ediBusinessObject, Factory);
		}

		public override void TestWrapperNotes()
		{
			var project = Factory.New<EDIProject>();
			AssertNotNull("Precondition: new project has a created project log as note", project.Notes.FindByDescription(Enterprise.ZArchitecture.Business.PredefinedNoteTypes.Instance.ProjectLog.Description));
			var wrapper = new FreightWrapperFromProject(project, Factory);
			AssertEquals("Notes", 1, wrapper.Notes.Count);
		}

		public override void TestWrapperNotesIncludingRelated()
		{
			var project = Factory.New<EDIProject>();
			AssertNotNull("Precondition: new project has a created project log as note", project.Notes.FindByDescription(Enterprise.ZArchitecture.Business.PredefinedNoteTypes.Instance.ProjectLog.Description));
			var wrapper = new FreightWrapperFromProject(project, Factory);
			AssertEquals("Notes", 1, wrapper.NotesIncludingRelated.Count);
		}
	}
}
