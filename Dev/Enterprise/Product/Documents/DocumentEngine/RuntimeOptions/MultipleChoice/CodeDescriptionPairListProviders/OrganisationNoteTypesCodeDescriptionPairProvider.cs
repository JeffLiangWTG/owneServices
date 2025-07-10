using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class OrganisationNoteTypesCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			OrgHeader header = new BusinessObjectFactory().New<OrgHeader>();
			((IStmNoteParent)header).CustomNoteTypesDelegate = new GetValueDelegate<NoteTypeCollection>(delegate { return SystemDataRegistry.Instance.CustomNotes.Value.GetNoteTypesForModuleAndThisCountry(ModuleIDs.Organisation.Name); });

			foreach (PredefinedNoteType noteType in header.NoteTypes)
			{
				if (!result.ContainsCode(noteType.Description))
				{
					result.AddPair(noteType.Description, noteType.Description);
				}
			}
			result.Sort();

			return result;
		}
	}
}
