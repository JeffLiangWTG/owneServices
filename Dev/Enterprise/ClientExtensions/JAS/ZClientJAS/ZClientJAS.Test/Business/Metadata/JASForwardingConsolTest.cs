using Enterprise.Client.JAS.Business;
using Enterprise.Metadata.Business.Tests;
using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.JAS.Metadata.Testing
{
	public class JASForwardingConsolTest : ForwardingConsolTest
	{
		protected override NoteTypeCollection ExpectedNoteTypes()
		{
			var noteTypes = base.ExpectedNoteTypes();
			noteTypes.Add(JASPredefinedNoteTypes.Instance.JXCExportLog);
			return noteTypes;
		}

		protected override IMetadata NewMetadata
		{
			get
			{
				return new JASForwardingConsol();
			}
		}
	}
}
