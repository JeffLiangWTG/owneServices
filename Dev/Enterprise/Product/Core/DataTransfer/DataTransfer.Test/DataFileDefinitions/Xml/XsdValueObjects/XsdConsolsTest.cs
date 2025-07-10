using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestedType(typeof(Xsd.Consols))]
	sealed class XsdConsolsTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.ConsolCollection value = null;
			value = new Xsd.Consols().Consol;
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
