using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.InterchangeInfoSource))]
	sealed class InterchangeInfoSourceTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.InterchangeInfoSource value = null;
			value = new Xsd.InterchangeInfoSourceCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
