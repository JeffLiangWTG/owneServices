using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.Notes))]
	sealed class NotesTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.Notes value = null;
			value = new Xsd.NotesCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
