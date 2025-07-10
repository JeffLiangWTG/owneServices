using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(OrgPatternMatchAddressDataAdapter))]
	sealed class OrgPatternMatchAddressDataAdapterTest : ValueObjectDataAdapterTest<OrgPatternMatchAddress, Xsd.Organisation>
	{
		[ExpectException(typeof(ZException))]
		public void TestInvalidTypeParameterPassedToConstructor()
		{
			new OrgPatternMatchAddressDataAdapter(ZGuid.NewZGuid(), typeof(string), OrgPatternMatchAddress.Constants.AddressType.AirCargoImporter);
		}

		public void TestFindBusinessObject()
		{
			OrgPatternMatchAddress orgPatternMatchAddress = Factory.New<OrgPatternMatchAddress>();
			orgPatternMatchAddress.P3_AddressType = AddressType;
			orgPatternMatchAddress.P3_ParentID = ParentID;
			orgPatternMatchAddress.P3_ParentTableCode = "OH";
			Factory.Save();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			BusinessObject foundOrgPatternMatchAddress = Adapter.CreateOrUpdateFromValueObject(GetPopulatedOrganisationValueObject(), context);
			AssertEquals(orgPatternMatchAddress.PK, foundOrgPatternMatchAddress.PK);
		}

		public void TestImportFromValueObject()
		{
			Xsd.Organisation organisation = GetPopulatedOrganisationValueObject();

			ValueObjectImportContext context = new ValueObjectImportContext(OrgPatternMatchAddress.Factory, new NotificationBuffer());
			Adapter.ImportFromValueObject(OrgPatternMatchAddress, organisation, context);

			AssertEquals(AddressType, OrgPatternMatchAddress.P3_AddressType);
			AssertEquals("OH", OrgPatternMatchAddress.P3_ParentTableCode);
			AssertEquals(ParentID, OrgPatternMatchAddress.P3_ParentID);
			AssertEquals("TEST", OrgPatternMatchAddress.P3_Code);
			AssertEquals("OrganisationName", OrgPatternMatchAddress.P3_CompanyName);
			AssertEquals("AddressLine1", OrgPatternMatchAddress.P3_Address1);
			AssertEquals("AddressLine2", OrgPatternMatchAddress.P3_Address2);
			AssertEquals("Chatswood", OrgPatternMatchAddress.P3_City);
			AssertEquals("1234", OrgPatternMatchAddress.P3_PostCode);
			AssertEquals("NSW", OrgPatternMatchAddress.P3_State);
			AssertEquals("1234", OrgPatternMatchAddress.P3_Phone);
			AssertEquals("9876", OrgPatternMatchAddress.P3_Fax);
			AssertEquals("someone@somewhere.com", OrgPatternMatchAddress.P3_Email);
			AssertEquals("ContactName", OrgPatternMatchAddress.P3_ContactName);
		}

		public void TestImportFromValueObject_AddressCompanyNameIsPopulated()
		{
			Xsd.Organisation organisation = GetPopulatedOrganisationValueObject();
			organisation.OrganisationDetails.Addresses[0].CompanyName = "CompanyName";

			ValueObjectImportContext context = new ValueObjectImportContext(OrgPatternMatchAddress.Factory, new NotificationBuffer());
			Adapter.ImportFromValueObject(OrgPatternMatchAddress, organisation, context);

			AssertEquals(AddressType, OrgPatternMatchAddress.P3_AddressType);
			AssertEquals("OH", OrgPatternMatchAddress.P3_ParentTableCode);
			AssertEquals(ParentID, OrgPatternMatchAddress.P3_ParentID);
			AssertEquals("TEST", OrgPatternMatchAddress.P3_Code);
			AssertEquals("CompanyName", OrgPatternMatchAddress.P3_CompanyName);
			AssertEquals("AddressLine1", OrgPatternMatchAddress.P3_Address1);
			AssertEquals("AddressLine2", OrgPatternMatchAddress.P3_Address2);
			AssertEquals("Chatswood", OrgPatternMatchAddress.P3_City);
			AssertEquals("1234", OrgPatternMatchAddress.P3_PostCode);
			AssertEquals("NSW", OrgPatternMatchAddress.P3_State);
			AssertEquals("1234", OrgPatternMatchAddress.P3_Phone);
			AssertEquals("9876", OrgPatternMatchAddress.P3_Fax);
			AssertEquals("someone@somewhere.com", OrgPatternMatchAddress.P3_Email);
			AssertEquals("ContactName", OrgPatternMatchAddress.P3_ContactName);
		}

		Xsd.Organisation GetPopulatedOrganisationValueObject()
		{
			Xsd.Organisation organisation = new Xsd.Organisation();
			organisation.EDICode = "TEST";
			organisation.OrganisationDetails.Name = "OrganisationName";
			Xsd.OrgAddress address = organisation.OrganisationDetails.Addresses.AddNew();
			Xsd.AddressCapability capability = address.AddressCapabilities.AddNew();
			capability.AddressType = Xsd.AddressCapabilityAddressType.MAIN;
			capability.AddressTypeSpecified = true;
			capability = address.AddressCapabilities.AddNew();
			capability.AddressType = Xsd.AddressCapabilityAddressType.OFC;
			capability.AddressTypeSpecified = true;

			address.AddressLine1 = "AddressLine1";
			address.AddressLine2 = "AddressLine2";
			address.CityOrSuburb = "Chatswood";
			address.PostCode = "1234";
			address.StateOrProvince = "NSW";
			address.Email = "someone@somewhere.com";
			Xsd.TelephoneNumber telNo = address.TelephoneNumbers.AddNew();
			telNo.NumberType = Xsd.TelephoneNumberNumberType.Business;
			telNo.Value = "1234";

			telNo = address.TelephoneNumbers.AddNew();
			telNo.NumberType = Xsd.TelephoneNumberNumberType.Fax;
			telNo.Value = "9876";

			Xsd.OrgContact contact = organisation.OrganisationDetails.Contacts.AddNew();
			contact.Name = "ContactName";
			return organisation;
		}

		protected override void SetUp()
		{
			base.SetUp();
			ParentID = ZGuid.NewZGuid();
			AddressType = "IMP";

			Adapter = new OrgPatternMatchAddressDataAdapter(ParentID, typeof(OrgHeader), AddressType);
			OrgPatternMatchAddress = Factory.New<OrgPatternMatchAddress>();
		}

		ZGuid ParentID;
		string AddressType;
		OrgPatternMatchAddressDataAdapter Adapter;
		OrgPatternMatchAddress OrgPatternMatchAddress;

		#region Base Test Overrides

		protected override bool IsExportToValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return true; }
		}

		protected override ValueObjectDataAdapter<OrgPatternMatchAddress, Xsd.Organisation> GetNewBizObjXmlDataAdapter()
		{
			return new OrgPatternMatchAddressDataAdapter(ZGuid.NewZGuid(), typeof(OrgHeader), "IMP");
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "Organisations"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "Organisation"; }
		}

		protected override bool IsExportToCollectionSupported
		{
			get { return false; }
		}

		protected override Enterprise.DataTransfer.Xml.Testing.ValueObjectDataAdapterTest<OrgPatternMatchAddress, Xsd.Organisation>.BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return null;
		}

		#endregion
	}
}
