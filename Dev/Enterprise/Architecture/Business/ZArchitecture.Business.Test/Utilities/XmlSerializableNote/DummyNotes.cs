using System.Collections.Immutable;
using System.Xml;
using System.Xml.Serialization;

namespace Enterprise.ZArchitecture.Testing
{
	public class DummyNotes : SerializableNoteText
	{
		public DummyNotes()
		{
			DummyNoteList = ImmutableList.Create<DummyNote>();
		}

		[XmlElement("DummyNote")]
		public IImmutableList<DummyNote> DummyNoteList { get; set; }
	}
}
