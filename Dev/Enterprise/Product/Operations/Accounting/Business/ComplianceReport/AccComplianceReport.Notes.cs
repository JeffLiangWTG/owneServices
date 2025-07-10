using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public partial class AccComplianceReport
	{
		public void SetupNotesFactoryToBeSavedWithMainFactory()
		{
			var notesFactory = this.GetNotes().Factory;
			if (notesFactory.ChildFactories.Contains(Factory))
			{
				notesFactory.ChildFactories.Remove(Factory);
			}
			Factory.ChildFactories.Add(notesFactory);
			Factory.Saved += RemoveNotesFactoryFromChildrenOnFactorySaved;
		}

		void RemoveNotesFactoryFromChildrenOnFactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			factory.Saved -= RemoveNotesFactoryFromChildrenOnFactorySaved;
			var notesFactoryToRemove = this.GetNotes().Factory;
			factory.ChildFactories.Remove(notesFactoryToRemove);
		}

		public void DeleteNotes(string description)
		{
			SetupNotesFactoryToBeSavedWithMainFactory();
			var existingNote = this.GetNotes().FindByDescription(description);
			foreach (var oldNote in existingNote)
			{
				oldNote.Delete();
			}
		}

		public void AddNote(string description, ZString notesText)
		{
			SetupNotesFactoryToBeSavedWithMainFactory();
			var newNote = this.GetNotes().AddNew(false, description, notesText);
			newNote.ST_ForceRead = true;
		}
	}
}
