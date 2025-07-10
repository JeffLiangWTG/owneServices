using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.DocAddressCollection))]
	sealed class DocAddressCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestAddNew()
		{
			Xsd.DocAddressCollection value = new Xsd.DocAddressCollection();
			Xsd.DocAddress newAddress = value.AddNew(Xsd.DocAddressAddressType.CED);
			AssertEquals("New address should be created with the specified type", true, newAddress.AddressTypeSpecified);
			AssertEquals("New address should be created with the specified type", Xsd.DocAddressAddressType.CED, newAddress.AddressType);
		}
	}
}
