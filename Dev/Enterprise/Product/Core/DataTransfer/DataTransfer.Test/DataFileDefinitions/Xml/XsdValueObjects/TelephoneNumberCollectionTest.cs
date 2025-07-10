using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.TelephoneNumberCollection))]
	sealed class TelephoneNumberCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.TelephoneNumberCollection value = null;
			value = new Xsd.OrgAddress().TelephoneNumbers;
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
