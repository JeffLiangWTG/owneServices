using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.TelephoneNumber))]
	sealed class TelephoneNumberTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.TelephoneNumber value = null;
			value = new Xsd.TelephoneNumberCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
