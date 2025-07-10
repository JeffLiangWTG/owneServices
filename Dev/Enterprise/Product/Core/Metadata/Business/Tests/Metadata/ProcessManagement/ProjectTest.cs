using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business.Tests
{
	public class ProjectTest : EnterpriseBusinessObjectTest
	{
		protected internal override NoteTypeCollection ExpectedNoteTypes()
		{
			var noteTypes = base.ExpectedNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.ProjectLog);

			return noteTypes;
		}

		protected override IMetadata NewMetadata => new Project();
	}
}
