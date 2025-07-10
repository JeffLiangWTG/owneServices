using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class OrganisationNoteTypesCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new OrganisationNoteTypesCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			var list = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			OrgHeader header = new BusinessObjectFactory().New<OrgHeader>();
			AssertEquals(header.NoteTypes.Count, list.Count);
			foreach (PredefinedNoteType noteType in header.NoteTypes)
			{
				AssertNotNull(PredefinedNoteTypes.Instance.NoteTypeByDescription(noteType.Description));
			}
		}
	}
}
