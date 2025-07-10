using NUnit.Framework;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class SerializableNoteElementAttributeTest : TestCase
	{
		public void TestConstructor()
		{
			SerializableNoteElementAttribute attrib = new SerializableNoteElementAttribute(true);
			AssertEquals("Hidden element", true, attrib.HiddenElement);
			AssertEquals("Display name", string.Empty, attrib.DisplayName);

			attrib = new SerializableNoteElementAttribute(false);
			AssertEquals("Hidden element", false, attrib.HiddenElement);

			attrib = new SerializableNoteElementAttribute("display name");
			AssertEquals("Display name", "display name", attrib.DisplayName);
			AssertEquals("Hidden element", false, attrib.HiddenElement);
		}
	}
}
