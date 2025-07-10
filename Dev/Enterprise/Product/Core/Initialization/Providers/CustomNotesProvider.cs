using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Initialisation
{
	public sealed class CustomNotesProvider : ICustomNotesProvider
	{
		#region ICustomNotesProvider Members

		NoteTypeCollection ICustomNotesProvider.CustomNoteTypesForModuleAndThisCountry(string moduleIDName)
		{
			return Registry.Business.SystemDataRegistry.Instance.CustomNotes.Value.GetNoteTypesForModuleAndThisCountry(moduleIDName);
		}

		NoteTypeCollection ICustomNotesProvider.CustomNoteTypesForModuleAndAllCountries(string moduleIDName)
		{
			return Registry.Business.SystemDataRegistry.Instance.CustomNotes.Value.GetNoteTypesForModuleAndAllCountries(moduleIDName);
		}

		bool ICustomNotesProvider.NoteTypeExistsByName(string noteName)
		{
			return Registry.Business.SystemDataRegistry.Instance.CustomNotes.Value.NoteTypeExistsByName(noteName);
		}

		NoteTypeCollection ICustomNotesProvider.AllCustomNoteTypes
		{
			get { return Registry.Business.SystemDataRegistry.Instance.CustomNotes.Value.AllCustomNoteTypes; }
		}

		IRegistryItem ICustomNotesProvider.RegistryItem
		{
			get { return Registry.Business.SystemDataRegistry.Instance.CustomNotes; }
		}

		#endregion
	}
}
