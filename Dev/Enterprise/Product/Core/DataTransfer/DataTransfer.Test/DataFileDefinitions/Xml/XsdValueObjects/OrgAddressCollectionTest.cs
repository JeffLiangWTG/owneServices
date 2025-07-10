using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.OrgAddressCollection))]
	sealed class OrgAddressCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.OrgAddressCollection value = null;
			value = new Xsd.OrganisationDetail().Addresses;
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestAddNew()
		{
			Xsd.OrgAddressCollection value = new Xsd.OrgAddressCollection();
			Xsd.OrgAddress newAddress = value.AddNew(Xsd.AddressCapabilityAddressType.DLV);
			AssertEquals("New address should be created with the specified type", true, newAddress.AddressCapabilityTypeSpecified);
			AssertEquals("New address should be created with the specified type", true, newAddress.AddressCapabilities.HasCapabilityOfType(Xsd.AddressCapabilityAddressType.DLV));
		}

		public void TestAddNewOld()
		{
			Xsd.OrgAddressCollection value = new Xsd.OrgAddressCollection();
			Xsd.OrgAddress newAddress = value.AddNew(Xsd.OrgAddressAddressType.DLV);
			AssertEquals("New address should be created with the specified type", true, newAddress.AddressTypeSpecified);
			AssertEquals("New address should be created with the specified type", Xsd.OrgAddressAddressType.DLV, newAddress.AddressType);
		}

		public void TestAddressTypeInOrgAddress()
		{
			Xsd.OrgAddressCollection value = new Xsd.OrgAddressCollection();
			Xsd.OrgAddress newAddress = value.AddNew();
			newAddress.AddressType = Xsd.OrgAddressAddressType.MAIN;
			newAddress.AddressTypeSpecified = true;

			AssertEquals("NewAddress is main address", newAddress, value.GetMainAddress());
		}

		public void TestGetOrCreateMainAddress()
		{
			Xsd.OrgAddressCollection collection = new Xsd.OrgAddressCollection();

			Xsd.OrgAddress createdMainAddress = collection.GetOrCreateMainAddress();
			AssertNotNull("New main address should be created if it doesn't exist", createdMainAddress);
			AssertEquals("AddressType should be MAIN on new main address", true, createdMainAddress.AddressCapabilityTypeSpecified);
			AssertEquals("AddressType should be MAIN on new main address", true, createdMainAddress.AddressCapabilities.HasCapabilityOfType(Xsd.AddressCapabilityAddressType.MAIN));

			Xsd.OrgAddress foundMainAddress = collection.GetOrCreateMainAddress();
			AssertEquals("Main address should be returned if it already exists", true, foundMainAddress == createdMainAddress);
		}

		public void TestGetAddressWithMainCapability()
		{
			Xsd.OrgAddressCollection collection = new Xsd.OrgAddressCollection();

			Xsd.OrgAddress address1 = collection.AddNew();
			AddressCapability capability = address1.AddressCapabilities.AddNew();
			capability.AddressType = Xsd.AddressCapabilityAddressType.DLV;
			capability.AddressTypeSpecified = true;

			AssertNull("Null should be returned if there is no address with main capability", collection.GetAddressWithMainCapability());

			Xsd.OrgAddress address2 = collection.AddNew();
			AddressCapability capability2 = address2.AddressCapabilities.AddNew();
			capability2.AddressType = Xsd.AddressCapabilityAddressType.MAIN;
			capability2.AddressTypeSpecified = true;

			Assert("Address with main capability should be returned", collection.GetAddressWithMainCapability() == address2);
		}

		public void TestGetMainAddress()
		{
			Xsd.OrgAddressCollection collection = new Xsd.OrgAddressCollection();

			Xsd.OrgAddress address1 = collection.AddNew();
			AddressCapability capability = address1.AddressCapabilities.AddNew();
			capability.AddressType = Xsd.AddressCapabilityAddressType.DLV;
			capability.AddressTypeSpecified = true;

			Xsd.OrgAddress address2 = collection.AddNew();
			AddressCapability capability2 = address2.AddressCapabilities.AddNew();
			capability2.AddressType = Xsd.AddressCapabilityAddressType.MAIN;
			capability2.AddressTypeSpecified = true;

			Assert("Should return the main address", collection.GetMainAddress() == address2);

			capability.AddressTypeSpecified = false;
			capability2.AddressTypeSpecified = false;
			Assert("Should return the first address if a) no main address is found and b) the first address has no AddressType", collection.GetMainAddress() == address1);

			capability.AddressType = Xsd.AddressCapabilityAddressType.DLV;
			capability.AddressTypeSpecified = true;
			capability2.AddressTypeSpecified = false;
			AssertNull("Null should be returned if there is no main address found", collection.GetMainAddress());
		}

		public void TestGetMainOrFirstAddress()
		{
			Xsd.OrgAddressCollection collection = new Xsd.OrgAddressCollection();

			Xsd.OrgAddress address1 = collection.AddNew();
			AddressCapability capability = address1.AddressCapabilities.AddNew();
			capability.AddressType = Xsd.AddressCapabilityAddressType.DLV;
			capability.AddressTypeSpecified = true;
			Xsd.OrgAddress address2 = collection.AddNew();
			AddressCapability capability2 = address2.AddressCapabilities.AddNew();
			capability2.AddressType = Xsd.AddressCapabilityAddressType.MAIN;
			capability2.AddressTypeSpecified = true;
			AssertEquals("Should return the main address if it exists", true, address2 == collection.GetMainOrFirstAddress());

			capability2.AddressTypeSpecified = false;
			AssertEquals("Should return the first address if no main address", true, address1 == collection.GetMainOrFirstAddress());

			collection.Clear();
			AssertNull("Should return null if there no addresses exist", collection.GetMainOrFirstAddress());
		}

		public void TestSequenceAutoGenerated()
		{
			Xsd.OrgAddressCollection collection = new Xsd.OrgAddressCollection();
			Xsd.OrgAddress address1 = collection.AddNew();
			Xsd.OrgAddress address2 = collection.AddNew();
			AssertEquals("First address sequence", 1, address1.Sequence);
			AssertEquals("Second address sequence", 2, address2.Sequence);
		}

		public void TestSequenceNotGeneratedIfAlreadyExists()
		{
			Xsd.OrgAddressCollection collection = new Xsd.OrgAddressCollection();
			Xsd.OrgAddress address = new Xsd.OrgAddress();
			address.Sequence = 99;
			collection.Add(address);
			AssertEquals("Sequence should not be changed if already set", 99, address.Sequence);
		}
	}
}
