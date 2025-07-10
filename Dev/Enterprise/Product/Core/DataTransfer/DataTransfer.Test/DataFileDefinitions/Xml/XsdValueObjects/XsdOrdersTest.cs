using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestedType(typeof(Xsd.Orders))]
	sealed class XsdOrdersTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.OrderCollection value = null;
			value = new Xsd.Orders().Order;
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
