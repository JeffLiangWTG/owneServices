using CargoWise.Common.Testing;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public interface ICustomNotesProvider
	{
		NoteTypeCollection CustomNoteTypesForModuleAndThisCountry(string moduleIDName);
		NoteTypeCollection CustomNoteTypesForModuleAndAllCountries(string moduleIDName);
		bool NoteTypeExistsByName(string noteName);
		NoteTypeCollection AllCustomNoteTypes { get; }
		IRegistryItem RegistryItem { get; }
	}

	public static class CustomNotesProvider
	{
		public static ICustomNotesProvider Instance
		{
			get { return instance; }
			set { instance = value; }
		}
		[SuppressThreadStaticFieldMessage]
		static ICustomNotesProvider instance;
	}
}
