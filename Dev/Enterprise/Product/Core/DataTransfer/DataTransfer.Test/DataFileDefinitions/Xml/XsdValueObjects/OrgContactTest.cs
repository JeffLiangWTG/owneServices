using System.Xml.Serialization;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.OrgContact))]
	sealed class OrgContactTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.OrgContact value = null;
			value = new Xsd.OrgContactCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestXmlRoot()
		{
			XmlRootAttribute[] xmlRoot = (XmlRootAttribute[])typeof(Xsd.OrgContact).GetCustomAttributes(typeof(XmlRootAttribute), false);
			AssertEquals("XmlRootAttribute must be specified on the concrete type being serialized for XmlRootAttribute to be effective (this is currently used for CustomerService)", 1, xmlRoot.Length);
		}
	}
}
