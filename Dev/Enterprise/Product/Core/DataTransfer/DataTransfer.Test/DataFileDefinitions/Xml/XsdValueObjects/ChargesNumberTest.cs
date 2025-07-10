using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.Charges))]
	sealed class ChargesNumberTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.Charges value = null;
			value = new Xsd.ChargesCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
