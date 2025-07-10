using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.OrgWebURL))]
	sealed class OrgWebURLTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.OrgWebURL value = null;
			value = new Xsd.OrgWebURLCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
