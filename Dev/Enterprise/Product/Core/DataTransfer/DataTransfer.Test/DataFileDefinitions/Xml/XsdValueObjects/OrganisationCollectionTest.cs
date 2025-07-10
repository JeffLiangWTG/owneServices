using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.OrganisationCollection))]
	sealed class OrganisationCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.OrganisationCollection value = null;
			value = new Xsd.Organisations().Organisation;
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
