using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestedType(typeof(Xsd.Order))]
	sealed class XsdOrderTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.Order value = null;
			value = new Xsd.OrderCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
