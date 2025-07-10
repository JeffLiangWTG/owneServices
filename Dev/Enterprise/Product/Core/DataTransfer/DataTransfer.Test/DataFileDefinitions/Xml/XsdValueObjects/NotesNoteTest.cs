using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.NotesNote))]
	sealed class NotesNoteTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.NotesNote value = null;
			value = new Xsd.NotesNoteCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
