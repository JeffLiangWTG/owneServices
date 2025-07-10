using Enterprise.Client.JAS.Business;
using Enterprise.Metadata.Business.Tests;
using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.JAS.Metadata.Testing
{
	public class JASForwardingShipmentTest : CommonShipmentTest
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
				return new JASForwardingShipment();
			}
		}
	}
}
