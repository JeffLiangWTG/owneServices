using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.ConsolIdentifier))]
	sealed class ConsolIdentifierTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.ConsolIdentifier value = null;
			value = new Xsd.ConsolIdentifierCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
