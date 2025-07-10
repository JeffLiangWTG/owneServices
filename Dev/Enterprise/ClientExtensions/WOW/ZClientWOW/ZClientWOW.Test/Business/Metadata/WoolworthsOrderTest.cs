using Enterprise.Metadata.Business.Tests;
using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.Wow.Metadata.Testing
{
	public class WoolworthsOrderTest : OrderTest
	{
		protected override NoteTypeCollection ExpectedNoteTypes()
		{
			var noteTypes = new NoteTypeCollection();
			noteTypes.Add(PredefinedNoteTypes.Instance.ClientVisibleJobNotes);
			return noteTypes;
		}

		protected override IMetadata NewMetadata
		{
			get
			{
				return new WoolworthsOrder();
			}
		}
	}
}
