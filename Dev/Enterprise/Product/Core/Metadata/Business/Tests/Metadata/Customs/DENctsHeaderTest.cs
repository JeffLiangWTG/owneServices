using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business.Tests
{
	public class DENctsHeaderTest : EnterpriseBusinessObjectTest
	{
		#region TestNoteTypes

		protected internal override NoteTypeCollection ExpectedNoteTypes()
		{
			var noteTypes = base.ExpectedNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.NCTSEventCancellationReason);
			return noteTypes;
		}

		#endregion

		#region Implementation

		protected override IMetadata NewMetadata
		{
			get { return new DENctsHeader(); }
		}

		#endregion
	}
}
