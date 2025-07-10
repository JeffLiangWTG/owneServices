using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	sealed class StandardManualAndBatchImportAddressValueObjectHelperTest : AddressValueObjectHelperTest
	{
		#region TestAddressMatching

		public void TestAddressMatching()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress orgAddress = org.Addresses.AddNew();
			org.OH_FullName = "TEST ADDRESS ORG";
			org.OH_Code = "TESADD";
			orgAddress.OA_Address1 = "Address 1";
			orgAddress.OA_Address2 = "Address 2";
			orgAddress.OA_Code = "aDDR1";

			Factory.Save();

			Xsd.AddressReference reference = new Xsd.AddressReference();
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.EDICode = org.OH_Code;
			reference.Organisation.OrganisationDetails.Name = org.OH_FullName;
			Xsd.OrgAddressCollection addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;

			Xsd.OrgAddress decoyXsdAddress = addressCollection.AddNew();
			decoyXsdAddress.AddressLine1 = "Test Address1";
			decoyXsdAddress.Sequence = 1;

			Xsd.OrgAddress xsdAddress = addressCollection.AddNew();
			xsdAddress.AddressCode = "aDDR1";
			xsdAddress.AddressLine1 = "Address 1111";
			xsdAddress.AddressLine2 = "Address 2222";
			xsdAddress.AddressType = Xsd.OrgAddressAddressType.DLV;
			xsdAddress.Sequence = 2;

			reference.AddressSequenceRef = 2;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			OrgAddress matchedAddress = (OrgAddress)AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None);
			Factory.Save();

			AssertEquals("Should be the same address", orgAddress.PK, matchedAddress.PK);
		}

		public override void TestFromAddressReferenceWithMatchingLine1MatchingShortCode_ButWithOneLine2Blank()
		{
			// Test of parent is irrelevant here as in this class we are matching addresses based on Address Short Code only.
			Assert(true);
		}

		public override void TestFromAddressReferenceRegeneratesAddressShortCodeIfDuplicateExistsOnOrg()
		{
			// Test of parent is irrelevant here as in this class we are matching addresses based on Address Short Code only.
			Assert(true);
		}

		public void TestAddressMatchWithEmptyCode()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST ADDRESS ORG";
			org.OH_Code = "TESADD";

			var address1 = org.Addresses.AddNew();
			address1.OA_Address1 = "Address 1";
			address1.OA_Address2 = "Address 2";
			address1.OA_Code = "ADDR1";

			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "Address A";
			address2.OA_Address2 = "Address B";
			address2.OA_Code = "ADDR2";

			Factory.Save();

			var reference = new Xsd.AddressReference();
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.EDICode = org.OH_Code;
			reference.Organisation.OrganisationDetails.Name = org.OH_FullName;

			var addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;

			var decoyXsdAddress = addressCollection.AddNew();
			decoyXsdAddress.AddressLine1 = "Test Address1";
			decoyXsdAddress.Sequence = 1;

			var xsdAddress1 = addressCollection.AddNew();
			xsdAddress1.AddressCode = "ADDR1";
			xsdAddress1.AddressLine1 = "Address 1111";
			xsdAddress1.AddressLine2 = "Address 2222";
			xsdAddress1.AddressType = Xsd.OrgAddressAddressType.DLV;
			xsdAddress1.Sequence = 2;

			var xsdAddress2 = addressCollection.AddNew();
			xsdAddress2.AddressLine1 = "Address A";
			xsdAddress2.AddressLine2 = "Address B";
			xsdAddress2.AddressType = Xsd.OrgAddressAddressType.PIC;
			xsdAddress2.Sequence = 3;

			var xsdAddress3 = addressCollection.AddNew();
			xsdAddress3.AddressCode = "NEW_ADDR";
			xsdAddress3.AddressLine1 = "Address 1";
			xsdAddress3.AddressLine2 = "Address 2";
			xsdAddress3.AddressType = Xsd.OrgAddressAddressType.DLV;
			xsdAddress3.Sequence = 4;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			reference.AddressSequenceRef = 2;
			AssertEquals("Should be the same address", address1.PK, AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None).PK);

			reference.AddressSequenceRef = 3;
			var matchedAddress = (OrgAddress)AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None);
			AssertNotNull("Should find an address even with empty code", matchedAddress);
			AssertEquals("Should be the same address even with empty code", address2.PK, AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None).PK);

			reference.AddressSequenceRef = 4;
			matchedAddress = (OrgAddress)AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None);
			AssertNull("There is no address for new code", matchedAddress);
		}

		#endregion

		#region Implementation

		protected override bool SetMainAddressOnCapabilitiesTest
		{
			get { return false; }
		}

		protected override AddressValueObjectHelper GetNewHelper()
		{
			return new StandardManualAndBatchImportAddressValueObjectHelper("");
		}

		#endregion
	}
}
