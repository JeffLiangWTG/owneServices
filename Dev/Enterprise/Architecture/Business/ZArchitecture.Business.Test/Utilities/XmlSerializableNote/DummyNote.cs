using CargoWise.Types;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.ZArchitecture.Testing
{
	public class DummyNote : XmlSerializableSetting
	{
		public DummyNote()
		{
			ElementA = ZString.Empty;
			ElementB = ZString.Empty;
			ElementC = ZString.Empty;
		}

		[SerializableNoteElement("Element A")]
		public ZString ElementA { get; set; }

		[SerializableNoteElement("Element B")]
		public ZString ElementB { get; set; }

		[SerializableNoteElement(true)]
		public ZString ElementC { get; set; }
	}
}
