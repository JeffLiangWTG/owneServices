using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	class AddressValueObjectHelperTest : TestCaseWithFactory
	{
		#region Import

		#region TestImportCapabilities

		public void TestImportCapabilities()
		{
			var addressCollection = new Xsd.OrgAddressCollection();
			var address = addressCollection.AddNew();
			address.AddressLine1 = "Test Address1";
			address.Sequence = 1;
			address.AddressCapabilities = new Xsd.AddressCapabilityCollection();

			var addressCapability1 = address.AddressCapabilities.AddNew();
			addressCapability1.AddressType = Xsd.AddressCapabilityAddressType.PAD;
			addressCapability1.IsMainAddress = Xsd.TrueFalse.@true;

			var addressCapability2 = address.AddressCapabilities.AddNew();
			addressCapability2.AddressType = Xsd.AddressCapabilityAddressType.PIC;
			addressCapability2.IsMainAddress = Xsd.TrueFalse.@true;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var organisation = context.Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = "BLAH";
			organisation.OH_FullName = "Blah Blah";
			if (SetMainAddressOnCapabilitiesTest)
			{
				organisation.MainAddress.OA_Address1 = "Test Address1";
			}

			AddressHelper.ImportFromValueObjectCollection(addressCollection, organisation, context);
			Factory.Save();

			Assert(organisation.Addresses[0].AddressCapability.GetCapabilityEnabled(nameof(Xsd.AddressCapabilityAddressType.PAD)));
			Assert(organisation.Addresses[0].AddressCapability.GetCapabilityEnabled(nameof(Xsd.AddressCapabilityAddressType.PIC)));
		}

		protected virtual bool SetMainAddressOnCapabilitiesTest
		{
			get { return true; }
		}

		#endregion

		#region TestFromAddressReference_DifferentCapability

		public void TestFromAddressReference_DifferentCapability()
		{
			var reference = new Xsd.AddressReference();
			reference.AddressSequenceRef = 1;

			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.OrganisationDetails.Name = "test org";
			reference.Organisation.OrganisationDetails.RegistrationNumbers.FindOrCreateForCurrentCountry(Xsd.RegistrationNumberTypes.GST).Number = "test org";
			var addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;

			var address1 = addressCollection.AddNew(Xml.XsdVersion1.AddressCapabilityAddressType.DLV);
			address1.AddressLine1 = "Test Address1";
			address1.Sequence = 1;

			var address2 = addressCollection.AddNew(Xml.XsdVersion1.AddressCapabilityAddressType.PIC);
			address2.AddressLine1 = "Test Address2";
			address2.AddressCode = "#1";
			address2.Sequence = 2;

			reference.AddressSequenceRef = 2;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var matchedAddress = AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None);
			AssertNotNull(matchedAddress);
		}

		#endregion

		#region TestCreateOrUpdateFromValueObject_DefaultsMiscellaneousAddressCapabilityIfNoneSupplied

		public void TestCreateOrUpdateFromValueObject_DefaultsMiscellaneousAddressCapabilityIfNoneSupplied()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "test org";
			org.OH_Code = "TSTORG";

			Factory.Save();

			var reference = new Xsd.AddressReference();
			reference.AddressSequenceRef = 1;
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.OrganisationDetails.Name = "test org";
			reference.Organisation.EDICode = "TSTORG";
			reference.Organisation.OrganisationDetails.RegistrationNumbers.FindOrCreateForCurrentCountry(Xsd.RegistrationNumberTypes.GST).Number = "test org";

			var addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;

			var address1 = addressCollection.AddNew();
			address1.AddressLine1 = "Test Address1";
			address1.AddressCode = "TEST";
			address1.Sequence = 1;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var matchedAddress = AddressHelper.CreateOrUpdateFromValueObject(org, addressCollection, address1, context);
			AssertNotNull(matchedAddress);
			AssertEquals(true, ((OrgAddress)matchedAddress).AddressCapability.GetCapabilityEnabled(nameof(Xsd.AddressCapabilityAddressType.MSC)));
		}

		#endregion

		#region TestFromAddressReference

		public void TestFromAddressReference()
		{
			var reference = new Xsd.AddressReference();
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.OrganisationDetails.Name = "test org";
			reference.Organisation.OrganisationDetails.RegistrationNumbers.FindOrCreateForCurrentCountry(Xsd.RegistrationNumberTypes.GST).Number = "test org";

			var addresses = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addresses;

			var address1 = addresses.AddNew();
			address1.AddressLine1 = "Test Address1";
			address1.Sequence = 4;

			var address2 = addresses.AddNew();
			address2.AddressLine1 = "Test Address2";
			address2.AddressCode = "TESORG";
			address2.Sequence = 2;

			reference.AddressSequenceRef = 2;
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var matchedAddress = AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None);
			Factory.Save();
			AssertEquals("Should match the correct address.", "Test Address2", matchedAddress.OA_Address1);

			reference.AddressSequenceRef = 1;
			var unmatchedAddress = AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None);
			AssertEquals("Should match no address.", null, unmatchedAddress);

			reference.AddressSequenceRef = 2;
			var secondMatchedAddress = AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None);
			var secondMatchedAddressPK = AddressHelper.FromAddressReferenceGetAddressPK(reference, context);
			var secondMatchedAddressCode = AddressHelper.FromAddressReferenceGetAddressCode(reference, context);
			AssertEquals("Should match the same address as originally matched.", matchedAddress.PK, secondMatchedAddress.PK);
			AssertEquals("Should match the same address as originally matched.", matchedAddress.PK, secondMatchedAddressPK);
			AssertEquals("Should match the same address as originally matched.", matchedAddress.OA_Code, secondMatchedAddressCode);

			var org = Factory.New<OrgHeader>();
			var orgAddress = org.MainAddress;
			orgAddress.OA_Code = "!!!";
			AssertEquals("Should match no address.", ZString.Empty, AddressHelper.FromAddressReferenceGetAddressCode(reference, org, context));

			orgAddress.OA_Address1 = "Test Address2";
			address2.AddressCode = "!!!";
			orgAddress.OA_Code = "!!!";
			AssertEquals("Should match the correct address.", "!!!", AddressHelper.FromAddressReferenceGetAddressCode(reference, org, context));
		}

		#endregion

		#region TestFromAddressReferenceWithFuzzyMatching

		public void TestFromAddressReferenceWithFuzzyMatching()
		{
			var org = Factory.New<OrgHeader>();
			var orgAddress = org.Addresses.AddNew();
			org.OH_FullName = "TEST ADDRESS ORG";
			org.OH_Code = "TESADD";
			orgAddress.OA_Address1 = "3/1075 BEAUDESERT RD";
			orgAddress.OA_Address2 = "WETHERILL PARK D.C., NSW";

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

			var xsdAddress = addressCollection.AddNew();
			xsdAddress.AddressLine1 = " 3 / 1075  Beaudesert";
			xsdAddress.AddressLine2 = "WEtheRIll parK";
			xsdAddress.AddressType = Xsd.OrgAddressAddressType.DLV;
			xsdAddress.Sequence = 2;
			xsdAddress.AddressCode = "3 / 1075 BEAUDESERT RD";

			reference.AddressSequenceRef = 2;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var matchedAddress = (OrgAddress)AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None);
			Factory.Save();

			AssertEquals("Should be the same address", orgAddress.PK, matchedAddress.PK);
		}

		#endregion

		#region TestFromAddressReferenceWithFuzzyMatching_More

		public void TestFromAddressReferenceWithFuzzyMatching_More()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST ADDRESS ORG";
			org.OH_Code = "TESADD";

			var orgAddress1 = org.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Lot 96 Potassium Street";
			orgAddress1.OA_Code = "Assensi";

			var orgAddress2 = org.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Lot 23  Pilbara Street";
			orgAddress2.OA_Code = "Lot 23  Pilbara Street";

			var orgAddress3 = org.Addresses.AddNew();
			orgAddress3.OA_Address1 = "Lot 45 Peek Street";
			orgAddress3.OA_Code = "Sensei";

			Factory.Save();

			var reference = new Xsd.AddressReference();
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.EDICode = org.OH_Code;
			reference.Organisation.OrganisationDetails.Name = org.OH_FullName;
			var addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;

			var decoyXsdAddress = addressCollection.AddNew();
			decoyXsdAddress.AddressLine1 = "DUMMY Address1";
			decoyXsdAddress.Sequence = 1;

			var xsdAddress = addressCollection.AddNew();
			xsdAddress.AddressLine1 = "Lot 23  Pilbara Street";
			xsdAddress.AddressCode = "Lot 23  Pilbara Street";
			xsdAddress.Sequence = 2;

			reference.AddressSequenceRef = 2;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var matchedAddress = (OrgAddress)AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None);
			Factory.Save();

			AssertEquals("Should be the same address", orgAddress2.PK, matchedAddress.PK);
		}

		#endregion

		#region TestFromAddressReference_MatchesToMainAddressIfAddressHasSequenceOne

		public void TestFromAddressReference_MatchesToMainAddressIfAddressHasSequenceOne()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = "TSTORG";
			organisation.OH_FullName = "test org";

			Factory.Save();

			var reference = new Xsd.AddressReference();
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.EDICode = "TSTORG";
			reference.Organisation.OrganisationDetails.Name = "test org";

			var addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;

			var address1 = addressCollection.AddNew();
			address1.AddressLine1 = "Some Crap that don't exist";
			address1.AddressCode = "Crap";
			address1.Sequence = 1;

			var address2 = addressCollection.AddNew();
			address2.AddressLine1 = "To create";
			address2.AddressLine2 = "Some other crap";
			address2.Sequence = 2;

			reference.AddressSequenceRef = 1;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var matchedAddress = AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None);
			AssertEquals(organisation.MainAddress, matchedAddress);
		}

		#endregion

		#region TestFromAddressReference_WithNoMatchedAddressSetsOrganisationOnJobDocAddress

		public void TestFromAddressReference_WithNoMatchedAddressSetsOrganisationOnJobDocAddress()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = "TSTORG";
			organisation.OH_FullName = "test org";

			Factory.Save();

			var reference = new Xsd.AddressReference();
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.EDICode = "TSTORG";
			reference.Organisation.OrganisationDetails.Name = "test org";

			var addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;

			var address1 = addressCollection.AddNew();
			address1.AddressLine1 = "Some Crap that don't exist";
			address1.Sequence = 1;

			var address2 = addressCollection.AddNew();
			address2.AddressLine1 = "To create";
			address2.AddressLine2 = "Some other crap";
			address2.AddressCode = "Crap";
			address2.Sequence = 2;

			reference.AddressSequenceRef = 2;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var jobDocAddress = Factory.New<JobDocAddress>();
			var matchedAddress = AddressHelper.FromAddressReferenceSetupDocAddress(jobDocAddress, reference, context, OrganisationTypes.None);
			AssertNull(matchedAddress);
			AssertEquals(organisation.PK, jobDocAddress.OrganisationPK);
		}

		#endregion

		#region TestFromAddressReference_WithNoMatchedAddress_DoesNotCreateNewAddress

		public void TestFromAddressReference_WithNoMatchedAddress_DoesNotCreateNewAddress()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "TSTORG";
			organisation.OH_FullName = "test org";

			Factory.Save();

			var reference = new Xsd.AddressReference();
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.EDICode = "TSTORG";
			reference.Organisation.OrganisationDetails.Name = "test org";
			reference.Organisation.OrganisationDetails.RegistrationNumbers.FindOrCreateForCurrentCountry(Xsd.RegistrationNumberTypes.GST).Number = "test org";

			var addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;

			var address1 = addressCollection.AddNew();
			address1.AddressLine1 = "Some Crap that don't exist";
			address1.Sequence = 1;

			var address2 = addressCollection.AddNew();
			address2.AddressLine1 = "To create";
			address2.AddressLine2 = "Some other crap";
			address2.Sequence = 2;

			reference.AddressSequenceRef = 2;
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var matchedAddress = AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None);
			AssertNull("No matching address found", matchedAddress);
		}

		#endregion

		#region TestFromAddressReference_WithNoMatchedAddress_CreatesNewAddress_IfJobSupportsIt

		public void TestFromAddressReference_WithNoMatchedAddress_CreatesNewAddress_IfJobSupportsIt()
		{
			var reference = new Xsd.AddressReference();
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.OrganisationDetails.Name = "test org";
			reference.Organisation.OrganisationDetails.RegistrationNumbers.FindOrCreateForCurrentCountry(Xsd.RegistrationNumberTypes.GST).Number = "test org";

			var addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;

			var address1 = addressCollection.AddNew();
			address1.AddressLine1 = "Some Crap that don't exist";
			address1.Sequence = 1;

			var address2 = addressCollection.AddNew();
			address2.AddressLine1 = "To create";
			address2.AddressLine2 = "Some other crap";
			address2.Sequence = 2;

			reference.AddressSequenceRef = 2;
			IValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var dummy = Factory.New<DummyWithDocAddressAndUnmatchedOrgNote>();
			dummy.CreateOrgAddressOnUnmatch = true;
			context.ImportingJob = dummy;
			var matchedAddress = AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None);
			AssertNotNull("No matching address found - a new address should be created", matchedAddress);
			AssertEquals("To create", matchedAddress.OA_Address1);
			AssertEquals("Some other crap", matchedAddress.OA_Address2);
		}

		#endregion

		#region TestFromAddressReference_WhenCreatingNewOrgAddsFirstAddressAsMain

		public void TestFromAddressReference_WhenCreatingNewOrgAddsFirstAddressAsMain()
		{
			var reference = new Xsd.AddressReference();
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.EDICode = "TSTORG";
			reference.Organisation.OrganisationDetails.Name = "test org";
			reference.Organisation.OrganisationDetails.RegistrationNumbers.FindOrCreateForCurrentCountry(Xsd.RegistrationNumberTypes.GST).Number = "test org";

			var addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;

			var address1 = addressCollection.AddNew();
			address1.AddressLine1 = "Some Crap that don't exist";
			address1.Sequence = 1;

			reference.AddressSequenceRef = 1;
			IValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var dummy = Factory.New<DummyWithDocAddressAndUnmatchedOrgNote>();
			dummy.CreateOrgAddressOnUnmatch = true;
			context.ImportingJob = dummy;
			var matchedAddress = (OrgAddress)AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None);
			AssertNotNull("No matching address found - a new address should be created", matchedAddress);
			AssertEquals("Some Crap that don't exist", matchedAddress.OA_Address1);
			AssertEquals(true, matchedAddress.IsMainAddress);
			AssertEquals(true, matchedAddress.AddressCapability.GetCapabilityEnabled(Enterprise.MasterFiles.Business.OrgConstants.AddressType.Office));
			AssertEquals(false, matchedAddress.AddressCapability.GetCapabilityEnabled(Enterprise.MasterFiles.Business.OrgConstants.AddressType.Miscellaneous));
			AssertEquals(1, matchedAddress.Header.Addresses.Count);
		}

		#endregion

		#region TestCreateOrUpdateFromValueObjectWithMissingLine1

		public void TestCreateOrUpdateFromValueObjectWithMissingLine1()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "TSTORG";
			organisation.OH_FullName = "test org";

			var reference = new Xsd.AddressReference();
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.OrganisationDetails.Name = "test org";
			reference.Organisation.EDICode = "TSTORG";
			reference.Organisation.OrganisationDetails.RegistrationNumbers.FindOrCreateForCurrentCountry(Xsd.RegistrationNumberTypes.GST).Number = "test org";

			var addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;
			reference.Organisation.OrganisationDetails.Location = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			reference.Organisation.OrganisationDetails.Location.IsSpecified = true;

			var address1 = addressCollection.AddNew();
			address1.AddressLine1 = "";
			address1.Sequence = 4;
			reference.AddressSequenceRef = 4;

			var holdWarnings = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, holdWarnings);
			var matchedAddress = AddressHelper.CreateOrUpdateFromValueObject(organisation, addressCollection, address1, context) as OrgAddress;

			AssertEquals(true, matchedAddress.IsMainAddress);
			AssertEquals("Should have a warning for missing AddressLine1", true, holdWarnings.HasWarnings);
			AssertContains("Warning: " + "Address Line 1 is required on Organization TSTORG", holdWarnings.AsString);
		}

		#endregion

		#region TestCreateOrUpdateFromValueObjectWithMissingLine1_AttachedToTransactionNotInDatabase

		public void TestCreateOrUpdateFromValueObjectWithMissingLine1_AttachedToTransactionNotInDatabase()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "TSTORG";
			organisation.OH_FullName = "test org";

			var reference = new Xsd.AddressReference();
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.OrganisationDetails.Name = "test org";
			reference.Organisation.EDICode = "TSTORG";
			reference.Organisation.OrganisationDetails.RegistrationNumbers.FindOrCreateForCurrentCountry(Xsd.RegistrationNumberTypes.GST).Number = "test org";

			var addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;
			reference.Organisation.OrganisationDetails.Location = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			reference.Organisation.OrganisationDetails.Location.IsSpecified = true;

			var address1 = addressCollection.AddNew();
			address1.AddressLine1 = "";
			address1.Sequence = 4;
			reference.AddressSequenceRef = 4;

			var holdWarnings = new NotificationBuffer();
			IValueObjectImportContext context = new ValueObjectImportContext(Factory, holdWarnings);
			var dummyBizo = Factory.New<DummyBusinessObject>();
			dummyBizo.HumanReadableNameForTest = "Dummy 123";
			context.ImportingJob = dummyBizo;
			var matchedAddress = AddressHelper.CreateOrUpdateFromValueObject(organisation, addressCollection, address1, context) as OrgAddress;

			AssertEquals(true, matchedAddress.IsMainAddress);
			AssertEquals("Should have a warning for missing AddressLine1", true, holdWarnings.HasWarnings);
			AssertContains("Warning: " + "Attempted to import an invalid Address (Missing Address Line 1) for Organization TSTORG (test org). Address set to Main Address for this Organization.", holdWarnings.AsString);
		}

		#endregion

		#region TestCreateOrUpdateFromValueObjectWithMissingLine1_AttachedToTransactionInDatabase

		public void TestCreateOrUpdateFromValueObjectWithMissingLine1_AttachedToTransactionInDatabase()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "TSTORG";
			organisation.OH_FullName = "test org";

			var reference = new Xsd.AddressReference();
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.OrganisationDetails.Name = "test org";
			reference.Organisation.EDICode = "TSTORG";
			reference.Organisation.OrganisationDetails.RegistrationNumbers.FindOrCreateForCurrentCountry(Xsd.RegistrationNumberTypes.GST).Number = "test org";

			var addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;
			reference.Organisation.OrganisationDetails.Location = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			reference.Organisation.OrganisationDetails.Location.IsSpecified = true;

			var address1 = addressCollection.AddNew();
			address1.AddressLine1 = "";
			address1.Sequence = 4;
			reference.AddressSequenceRef = 4;

			var holdWarnings = new NotificationBuffer();
			IValueObjectImportContext context = new ValueObjectImportContext(Factory, holdWarnings);
			var dummyBizo = Factory.New<DummyBusinessObject>();
			dummyBizo.HumanReadableNameForTest = "Dummy 123";

			Factory.Save();

			context.ImportingJob = dummyBizo;
			var matchedAddress = AddressHelper.CreateOrUpdateFromValueObject(organisation, addressCollection, address1, context) as OrgAddress;

			AssertEquals(true, matchedAddress.IsMainAddress);
			AssertEquals("Should have a warning for missing AddressLine1", true, holdWarnings.HasWarnings);
			AssertContains("Warning: " + "Attempted to import an invalid Address (Missing Address Line 1) for Organization TSTORG (test org) on transaction Dummy 123. Address set to Main Address for this Organization.", holdWarnings.AsString);
		}

		#endregion

		#region TestCreateOrUpdateFromValueObjectWithMissingLine1_ButWithLine2

		public void TestCreateOrUpdateFromValueObjectWithMissingLine1_ButWithLine2()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "TSTORG";
			organisation.OH_FullName = "test org";

			var reference = new Xsd.AddressReference();
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.OrganisationDetails.Name = "test org";
			reference.Organisation.EDICode = "TSTORG";
			reference.Organisation.OrganisationDetails.RegistrationNumbers.FindOrCreateForCurrentCountry(Xsd.RegistrationNumberTypes.GST).Number = "test org";

			var addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;
			reference.Organisation.OrganisationDetails.Location = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			reference.Organisation.OrganisationDetails.Location.IsSpecified = true;

			var address1 = addressCollection.AddNew();
			address1.AddressLine1 = "";
			address1.AddressLine2 = "Make me!";
			address1.Sequence = 4;
			reference.AddressSequenceRef = 4;

			var holdWarnings = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, holdWarnings);
			var matchedAddress = AddressHelper.CreateOrUpdateFromValueObject(organisation, addressCollection, address1, context) as OrgAddress;

			AssertEquals(false, matchedAddress.IsMainAddress);
			AssertEquals("Make me!", matchedAddress.OA_Address1);
			AssertEquals("", matchedAddress.OA_Address2);
			AssertEquals("Should have no warning for missing AddressLine1", false, holdWarnings.HasWarnings);
		}

		#endregion

		#region TestFromAddressReferenceWithMatchingLine1MatchingShortCode_ButWithOneLine2Blank

		public virtual void TestFromAddressReferenceWithMatchingLine1MatchingShortCode_ButWithOneLine2Blank()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST ADDRESS ORG";
			org.OH_Code = "TESADD";

			var orgAddress = ResetOrgAddress(org);

			Factory.Save();

			var reference = new Xsd.AddressReference();
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.OrganisationDetails.Name = "test org";
			reference.Organisation.EDICode = "TSTORG";
			reference.Organisation.OrganisationDetails.RegistrationNumbers.FindOrCreateForCurrentCountry(Xsd.RegistrationNumberTypes.GST).Number = "test org";

			var addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;
			reference.Organisation.OrganisationDetails.Location = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			reference.Organisation.OrganisationDetails.Location.IsSpecified = true;

			var address1 = addressCollection.AddNew();
			address1.AddressLine1 = "33/1-3 Dudley street, Coogee";
			address1.AddressCode = orgAddress.OA_Code;
			reference.AddressSequenceRef = 1;

			AssertEquals("Precondition: Address Short Code are equal", address1.AddressCode, orgAddress.OA_Code);
			AssertEquals("Precondition: Address1 are equal", address1.AddressLine1, orgAddress.OA_Address1);
			AssertEquals("Precondition: Address2 is blank", address1.AddressLine2, string.Empty);

			var holdWarnings = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, holdWarnings);
			var matchedAddress = AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None) as OrgAddress;

			AssertEquals("Should match addresses", orgAddress.PK, matchedAddress.PK);

			orgAddress = ResetOrgAddress(org);

			address1.AddressCode = "XXX" + orgAddress.OA_Code;

			AssertNotEquals("Precondition: Address Short Code are NOT equal", address1.AddressCode, orgAddress.OA_Code);
			AssertEquals("Precondition: Address1 are equal", address1.AddressLine1, orgAddress.OA_Address1);
			AssertEquals("Precondition: Address2 is blank", address1.AddressLine2, string.Empty);
			matchedAddress = AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None) as OrgAddress;

			AssertNotEquals("Should not match addresses", orgAddress.PK, matchedAddress.PK);

			orgAddress = ResetOrgAddress(org);

			address1.AddressCode = orgAddress.OA_Code;
			address1.AddressLine2 = "Randwick";
			orgAddress.OA_Address2 = "Coogee";

			AssertEquals("Precondition: Address Short Code are equal", address1.AddressCode, orgAddress.OA_Code);
			AssertEquals("Precondition: Address1 are equal", address1.AddressLine1, orgAddress.OA_Address1);
			AssertNotEquals("Precondition: Address2 are NOT equal and none is blank", address1.AddressLine2, orgAddress.OA_Address2);

			matchedAddress = AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None) as OrgAddress;
			AssertNotEquals("Should not match addresses", orgAddress.PK, matchedAddress.PK);
		}

		OrgAddress ResetOrgAddress(OrgHeader org)
		{
			org.Addresses.RemoveAndDeleteAll();
			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_Address1 = "33/1-3 Dudley street, Coogee";
			orgAddress.OA_Address2 = "some irrelevant crap";
			return orgAddress;
		}

		#endregion

		#region TestFromAddressReferenceRegeneratesAddressShortCodeIfDuplicateExistsOnOrg

		[ExpectNoExceptions]
		public virtual void TestFromAddressReferenceRegeneratesAddressShortCodeIfDuplicateExistsOnOrg()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST ADDRESS ORG";
			org.OH_Code = "TESADD";

			var orgAddress = ResetOrgAddress(org);

			Factory.Save();

			var reference = new Xsd.AddressReference();
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.OrganisationDetails.Name = "test org";
			reference.Organisation.EDICode = "TSTORG";
			reference.Organisation.OrganisationDetails.RegistrationNumbers.FindOrCreateForCurrentCountry(Xsd.RegistrationNumberTypes.GST).Number = "test org";

			var addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;
			reference.Organisation.OrganisationDetails.Location = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			reference.Organisation.OrganisationDetails.Location.IsSpecified = true;
			reference.AddressSequenceRef = 1;

			var address1 = addressCollection.AddNew();
			address1.AddressLine1 = "33/1-3 Dudley street, Coogee";
			address1.AddressCode = orgAddress.OA_Code;
			address1.AddressLine2 = "Randwick";
			orgAddress.OA_Address2 = "Coogee";

			AssertEquals("Precondition: Address Short Code are equal", address1.AddressCode, orgAddress.OA_Code);
			AssertEquals("Precondition: Address1 are equal", address1.AddressLine1, orgAddress.OA_Address1);
			AssertNotEquals("Precondition: Address2 are NOT equal and none is blank", address1.AddressLine2, orgAddress.OA_Address2);
			AssertEquals("Precondition: Org has two addresses", org.Addresses.Count, 2);

			var holdWarnings = new NotificationBuffer();
			IValueObjectImportContext context = new ValueObjectImportContext(Factory, holdWarnings);

			var dummy = Factory.New<DummyWithDocAddressAndUnmatchedOrgNote>();
			dummy.CreateOrgAddressOnUnmatch = true;
			context.ImportingJob = dummy;

			var matchedAddress = AddressHelper.FromAddressReference(reference, context, OrganisationTypes.None) as OrgAddress;
			AssertNotEquals("Should not match addresses", orgAddress.PK, matchedAddress.PK);
			AssertEquals("Org has three addresses - one new has been added coz match was not found", org.Addresses.Count, 3);
			Factory.Save();
		}
		#endregion

		#region TestFromXmlValueObject_MainAddresseshouldPopulate

		public void TestFromXmlValueObject_MainAddresseshouldPopulate()
		{
			NotificationBuffer notify = new NotificationBuffer();
			OrgHeader organisation = Factory.New<OrgHeader>();
			Xsd.OrgAddressCollection allAddressValues = new Xsd.OrgAddressCollection();

			Xsd.OrgAddress otherAddressValue = allAddressValues.AddNew();
			Xsd.AddressCapability capability = otherAddressValue.AddressCapabilities.AddNew();
			capability.AddressType = Xsd.AddressCapabilityAddressType.OFC;
			capability.AddressTypeSpecified = true;
			Xsd.OrgAddress mainAddressValue = allAddressValues.AddNew();
			Xsd.AddressCapability mainAddressCapability = mainAddressValue.AddressCapabilities.AddNew();
			mainAddressCapability.AddressType = Xsd.AddressCapabilityAddressType.MAIN;
			mainAddressCapability.AddressTypeSpecified = true;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			var mainAddress = AddressHelper.CreateOrUpdateFromValueObject(organisation, allAddressValues, mainAddressValue, context);
			AssertEquals("Address marked as main should be main address", true, mainAddress.IsMainAddress);
		}

		#endregion

		#region TestImportFromValueObjectCollection

		public void TestImportFromValueObjectCollection()
		{
			var notify = new NotificationBuffer();
			var addressValueCollection = new Xsd.OrgAddressCollection();
			var mainAddressValue = addressValueCollection.AddNew(Xsd.AddressCapabilityAddressType.MAIN);
			mainAddressValue.AddressLine1 = "Main";

			var otherExistingAddressValue = addressValueCollection.AddNew(Xsd.AddressCapabilityAddressType.DLV);
			otherExistingAddressValue.AddressLine1 = "ExistingOther";
			otherExistingAddressValue.AddressCode = "ExistingOther";
			var otherNewAddressValue = addressValueCollection.AddNew(Xsd.AddressCapabilityAddressType.PAD);
			otherNewAddressValue.AddressLine1 = "NewOther";

			var organisation = Factory.New<OrgHeader>();
			var existingAddress = organisation.Addresses.AddNew();
			existingAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery.Code);
			existingAddress.OA_Address1 = "ExistingOther";

			var context = new ValueObjectImportContext(Factory, notify);
			AddressHelper.ImportFromValueObjectCollection(addressValueCollection, organisation, context);
			AssertEquals("There should be 3 addresses imported", 3, organisation.Addresses.Count);
			AssertEquals("Main address", "Main", organisation.Addresses[0].OA_Address1);
			AssertEquals("Already existing address should be updated", "ExistingOther", organisation.Addresses[1].OA_Address1);
			AssertEquals("New address should be added", "NewOther", organisation.Addresses[2].OA_Address1);
		}

		#endregion

		#region TestPhoneNumbers

		public void TestPhoneNumbers()
		{
			var notify = new NotificationBuffer();
			var organisation = Factory.New<OrgHeader>();
			var allAddressValues = new Xsd.OrgAddressCollection();
			var mainAddressValue = allAddressValues.AddNew();
			mainAddressValue.AddressLine1 = "Smuff";
			mainAddressValue.TelephoneNumbers = new Xsd.TelephoneNumberCollection();

			var busPhone = mainAddressValue.TelephoneNumbers.AddNew();
			busPhone.NumberType = Xsd.TelephoneNumberNumberType.Business;
			busPhone.Value = "1233333333333333333333333333333333333333333123";

			var faxPhone = mainAddressValue.TelephoneNumbers.AddNew();
			faxPhone.NumberType = Xsd.TelephoneNumberNumberType.Fax;
			faxPhone.Value = "01234567890123456789";

			var mobPhone = mainAddressValue.TelephoneNumbers.AddNew();
			mobPhone.NumberType = Xsd.TelephoneNumberNumberType.Mobile;
			mobPhone.Value = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";

			var context = new ValueObjectImportContext(Factory, notify);
			var mainAddress = AddressHelper.CreateOrUpdateFromValueObject(organisation, allAddressValues, mainAddressValue, context);

			var expected = new ZString("1233333333333333333333333333333333333333333123").SubstringSafe(0, OrgAddressSchema.OA_Phone.MaxLength);
			AssertEquals(expected, mainAddress.OA_Phone);
			expected = new ZString("01234567890123456789").SubstringSafe(0, OrgAddressSchema.OA_Fax.MaxLength);
			AssertEquals(expected, mainAddress.OA_Fax);
			expected = new ZString("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX").SubstringSafe(0, OrgAddressSchema.OA_Mobile.MaxLength);
			AssertEquals(expected, mainAddress.OA_Mobile);
		}

		#endregion

		#region TestUNLOCOCityAndCountry_RealOrg

		public void TestUNLOCOCityAndCountry_RealOrg()
		{
			var notify = new NotificationBuffer();

			var organisation = Factory.New<OrgHeader>();
			var allAddressValues = new Xsd.OrgAddressCollection();
			var mainAddressValue = allAddressValues.AddNew();
			mainAddressValue.AddressLine1 = "Import";
			mainAddressValue.Location.Value = "AUSYD";

			var context = new ValueObjectImportContext(Factory, notify);
			var mainAddress = AddressHelper.CreateOrUpdateFromValueObject(organisation, allAddressValues, mainAddressValue, context);

			AssertEquals("Sydney", mainAddress.PortName);
			AssertEquals("Australia", mainAddress.CountryName);
		}

		#endregion

		#region TestUNLOCOCityAndCountry_OrgForMatching

		public void TestUNLOCOCityAndCountry_OrgForMatching()
		{
			var notify = new NotificationBuffer();

			var organisation = new OrgHeaderForMatching(Factory);
			var allAddressValues = new Xsd.OrgAddressCollection();
			var mainAddressValue = allAddressValues.AddNew();
			mainAddressValue.AddressLine1 = "Yellow";
			mainAddressValue.Location.Value = "AUSYD";
			mainAddressValue.Location.City = "Sydney";
			mainAddressValue.Location.Country = "Australia";

			var context = new ValueObjectImportContext(Factory, notify);
			var mainAddress = AddressHelper.CreateOrUpdateFromValueObject(organisation, allAddressValues, mainAddressValue, context);

			AssertEquals("Sydney", mainAddress.PortName);
			AssertEquals("Australia", mainAddress.CountryName);
		}

		#endregion

		#region TestImportNoteUnmatchedOrganisations

		public void TestImportNoteUnmatchedOrganisations()
		{
			AssertImportNoteUnmatchedOrganisations();
		}

		public void TestImportNoteUnmatchedOrganisations_WithOrganisationType()
		{
			AssertImportNoteUnmatchedOrganisations(OrganisationTypes.WarehouseClient);
		}

		public void TestImportNoteUnmatchedOrganisations_WithOrganisationTypeAndSubType()
		{
			AssertImportNoteUnmatchedOrganisations(OrganisationTypes.WarehouseClient, OrganisationsSubTypeList.Codes.ReceivingForwarder);
		}

		void AssertImportNoteUnmatchedOrganisations(OrganisationTypes organisationType = OrganisationTypes.None, string organisationSubType = "")
		{
			var unmatchedOrg = new UnmatchedOrganisation(Factory);
			unmatchedOrg.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrg);

			var docAddress = new Xsd.DocAddress();
			docAddress.AddressType = Xsd.DocAddressAddressType.CRD;
			docAddress.AddressTypeSpecified = true;

			var uNLOCO = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");

			var org = docAddress.AddressReference.Organisation;
			org.EDICode = "TSTSYD_1";
			org.OwnerCode = "TSTSYD_1";
			org.OrganisationDetails.Name = "TST_1";
			org.OrganisationDetails.Location = uNLOCO;

			var orgAddress = org.OrganisationDetails.Addresses.AddNew();
			orgAddress.AddressType = Xsd.OrgAddressAddressType.MAIN;
			orgAddress.AddressLine1 = "TEST ADDRESS_1";
			orgAddress.AddressCode = "TEST_1";
			orgAddress.CityOrSuburb = "SYDNEY";
			orgAddress.StateOrProvince = "NSW";
			orgAddress.PostCode = "2000_1";
			orgAddress.Language = Core.SharedConstants.Languages.English;
			orgAddress.Location = uNLOCO;
			orgAddress.Sequence = 1;

			var addressCapability = orgAddress.AddressCapabilities.AddNew();
			addressCapability.AddressType = Xsd.AddressCapabilityAddressType.MAIN;
			addressCapability = orgAddress.AddressCapabilities.AddNew();
			addressCapability.AddressType = Xsd.AddressCapabilityAddressType.OFC;
			addressCapability.IsMainAddress = Xsd.TrueFalse.@true;

			var reference = new Xsd.AddressReference();
			reference.Organisation = org;

			var dummy = Factory.NewWithValidTestData<DummyWithDocAddressAndUnmatchedOrgNote>();
			var jobDocAddress = JobDocAddress.New(dummy);
			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			var builder = new ZStringBuilder();
			if (string.IsNullOrEmpty(organisationSubType))
			{
				builder.Append("Organisation Type: " + (organisationType == OrganisationTypes.None ? nameof(OrganisationTypes.Consignor) : organisationType.ToString()));
			}
			else
			{
				builder.Append("Organisation Type: " + organisationSubType);
			}
			builder.Append("Owner Code: " + org.OwnerCode);
			builder.Append("EDI Code: " + org.EDICode);
			builder.Append("Organisation Name: " + org.OrganisationDetails.Name);
			builder.Append("Address Line 1: " + orgAddress.AddressLine1);
			builder.Append("Address Line 2: " + orgAddress.AddressLine2);
			builder.Append("City: " + orgAddress.CityOrSuburb);
			builder.Append("Post Code: " + orgAddress.PostCode);
			builder.Append("State or Province: " + orgAddress.StateOrProvince);
			builder.Append("Country: " + orgAddress.Location.Value.SubstringSafe(0, 2));

			var docAddressType = organisationType == OrganisationTypes.None ? jobDocAddress.E2_AddressType : ZString.Empty;
			builder.Append("Doc Address Type: " + docAddressType);
			builder.Append(" ");

			AddressHelper.FromAddressReferenceSetupDocAddress(jobDocAddress, reference, context, organisationType, organisationSubType);
			var notes = dummy.GetNotes();
			AssertEquals("Should be Unmatched Org Details Note", builder.ToStringWithNewLineBetweenAppends(), notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description)[0].ST_NoteText);
		}

		#endregion

		#endregion

		#region Export

		#region TestToAddressReference

		public void TestToAddressReference()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			OrgHeader organisation = Factory.New<OrgHeader>();

			OrgAddress address1 = organisation.Addresses.MainAddress;
			address1.OA_Address1 = "address1";
			address1.OA_Language = Core.Constants.Languages.Albanian;

			OrgAddress address2 = organisation.Addresses.AddNew();
			address2.OA_Address1 = "address2";
			address2.OA_Language = Core.Constants.Languages.Basque;

			OrgAddress address3 = organisation.Addresses.AddNew();
			address3.OA_Address1 = "address3";
			address3.OA_Language = Core.Constants.Languages.English;

			OrgAddress address4 = organisation.Addresses.AddNew();
			address4.OA_Address1 = "address4";
			address4.OA_Language = Core.Constants.Languages.English;

			var buffer = new NotificationBuffer();
			var exportContext = new ValueObjectExportContext(buffer);
			Xsd.AddressReference reference = AddressHelper.ToAddressReference(address3, exportContext);
			AssertEquals("4 addresses should have been exported", 4, reference.Organisation.OrganisationDetails.Addresses.Count);

			Xsd.OrgAddress referredAddress = null;
			foreach (Xsd.OrgAddress address in reference.Organisation.OrganisationDetails.Addresses)
			{
				if (address.Sequence == reference.AddressSequenceRef)
				{
					referredAddress = address;
					break;
				}
			}
			AssertEquals("Should find the referenced address", "address3", referredAddress.AddressLine1);

			buffer.Clear();
			reference = AddressHelper.ToAddressReference(address1, exportContext);
			AssertEquals("AddressSequenceRef", 1, reference.AddressSequenceRef);

			referredAddress = null;
			foreach (Xsd.OrgAddress address in reference.Organisation.OrganisationDetails.Addresses)
			{
				if (address.Sequence == reference.AddressSequenceRef)
				{
					referredAddress = address;
					break;
				}
			}
			Assert("Should find the referenced address", referredAddress.AddressLine1 == "address1");
		}

		#endregion

		#region TestExportPhoneNumbers

		public void TestExportPhoneNumbers()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.MainAddress.OA_Phone = "02 9999 8888";
			organisation.MainAddress.OA_Mobile = "0405 234 987";
			organisation.MainAddress.OA_Fax = "02 4444 9999";

			NotificationBuffer notify = new NotificationBuffer();
			Xsd.OrgAddressCollection addressValueCollection = new Xsd.OrgAddressCollection();
			AddressHelper.ExportToValueObjectCollection(organisation.Addresses, addressValueCollection, notify);

			AssertEquals("There should be 1 Address exported", 1, addressValueCollection.Count);
			Xsd.OrgAddress xsdAddress = addressValueCollection[0];
			AssertEquals("Phone", "02 9999 8888", xsdAddress.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Business));
			AssertEquals("Mobile", "0405 234 987", xsdAddress.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Mobile));
			AssertEquals("Fax", "02 4444 9999", xsdAddress.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Fax));
		}

		#endregion

		#region TestExportToValueObjectCollection

		public void TestExportToValueObjectCollection()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			var payablesAddress = organisation.Addresses.AddNew();
			var mainAddress = organisation.MainAddress;

			payablesAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Payables.Code);
			payablesAddress.OA_Address1 = "PayablesAddress";
			mainAddress.OA_Address1 = "MainAddress";

			NotificationBuffer notify = new NotificationBuffer();
			Xsd.OrgAddressCollection addressValueCollection = new Xsd.OrgAddressCollection();
			AddressHelper.ExportToValueObjectCollection(organisation.Addresses, addressValueCollection, notify);

			AssertEquals("There should be 2 Addresses exported", 2, addressValueCollection.Count);
			AssertEquals("Sequence", 1, addressValueCollection[0].Sequence);
			AssertEquals("MainAddress must be exported first", "MainAddress", addressValueCollection[0].AddressLine1);
			AssertEquals("Sequence", 2, addressValueCollection[1].Sequence);
			AssertEquals("PayablesAddress other addresses are exported subsequently", "PayablesAddress", addressValueCollection[1].AddressLine1);
		}

		#endregion

		#region TestExportToValueObjectCollectionWithParentIDocAddress

		public void TestExportToValueObjectCollectionWithParentIDocAddress()
		{
			var organisation = Factory.New<OrgHeader>();
			var payablesAddress = organisation.Addresses.AddNew();
			var otherAddress = organisation.Addresses.AddNew();
			var mainAddress = organisation.MainAddress;

			payablesAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Payables.Code);
			payablesAddress.OA_Address1 = "PayablesAddress";
			otherAddress.OA_Address1 = "OtherAddress";
			mainAddress.OA_Address1 = "MainAddress";

			var notify = new NotificationBuffer();
			var addressValueCollection = new Xsd.OrgAddressCollection();
			var dummy = Factory.New<Enterprise.MasterFiles.Business.Testing.DummyWithDocAddress>();

			var addr1 = JobDocAddress.New(dummy);
			addr1.E2_OA_Address = payablesAddress.PK;
			(dummy as IDocAddresses).DocAddresses.Add(addr1);

			AddressHelper.ExportToValueObjectCollection(null, organisation.Addresses, dummy, addressValueCollection, notify);
			AssertEquals("There should be 2 Addresses exported", 2, addressValueCollection.Count);
			AssertEquals("Sequence", 1, addressValueCollection[0].Sequence);
			AssertEquals("MainAddress must be exported first", "MainAddress", addressValueCollection[0].AddressLine1);
			AssertEquals("Sequence", 2, addressValueCollection[1].Sequence);
			AssertEquals("PayablesAddress other addresses are exported subsequently", "PayablesAddress", addressValueCollection[1].AddressLine1);
		}

		#endregion

		#region TestExportToValueObjectCollection_IncludeNonEnglish

		public void TestExportToValueObjectCollection_IncludeNonEnglish()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();

			OrgAddress address1 = organisation.MainAddress;
			address1.OA_Language = Core.Constants.Languages.English;
			address1.OA_Address1 = "English";

			OrgAddress address2 = organisation.Addresses.AddNew();
			address2.OA_Language = "";
			address2.OA_Address1 = "EnglishBecauseLanguageEmpty";

			OrgAddress address3 = organisation.Addresses.AddNew();
			address3.OA_Language = Core.Constants.Languages.ChineseSimplified;
			address3.OA_Address1 = "Chinese";

			OrgAddress address4 = organisation.Addresses.AddNew();
			address4.OA_Language = Core.Constants.Languages.Malay;
			address4.OA_Address1 = "Chinese";

			NotificationBuffer notify = new NotificationBuffer();
			Xsd.OrgAddressCollection addressValueCollection = new Xsd.OrgAddressCollection();
			AddressHelper.ExportToValueObjectCollection(organisation.Addresses, addressValueCollection, notify);
			AssertEquals("Add Addresses should be exported", 4, addressValueCollection.Count);
		}

		#endregion

		#region TestExportToValueObject

		public void TestExportToValueObject()
		{
			OrgAddress address = Factory.NewWithValidTestData<OrgAddress>(TestBusinessObjectKind.All);
			Xsd.OrgAddress addressValue = AddressHelper.ExportToValueObject(address, new NotificationBuffer());

			AssertEquals("#1", addressValue.AddressLine1);
			AssertEquals("Address2", addressValue.AddressLine2);
			AssertEquals("City", addressValue.CityOrSuburb);
			AssertEquals("State", addressValue.StateOrProvince);
			AssertEquals("PostCode", addressValue.PostCode);
			AssertEquals("Email", addressValue.Email);
			AssertEquals("Languag", addressValue.Language);
			AssertEquals("#1", addressValue.AddressCode);
		}

		#endregion

		#endregion

		#region Implementation

		#region class DummyWithDocAddressAndUnmatchedOrgNote

		class DummyWithDocAddressAndUnmatchedOrgNote : DummyWithDocAddress, IAutoCreateAddressOnUnmatch
		{
			public DummyWithDocAddressAndUnmatchedOrgNote(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override NoteTypeCollection NoteTypesCore
			{
				get
				{
					NoteTypeCollection result = new NoteTypeCollection();
					result.Add(base.NoteTypesCore);
					result.Add(PredefinedNoteTypes.Instance.UnmatchedOrgDetails);

					return result;
				}
			}

			public bool CreateOrgAddressOnUnmatch { get; set; }
		}

		#endregion

		#region AddressHelper

		internal AddressValueObjectHelper AddressHelper
		{
			get { return addressHelper ?? (addressHelper = GetNewHelper()); }
		}

		protected virtual AddressValueObjectHelper GetNewHelper()
		{
			return new AddressValueObjectHelper("");
		}

		AddressValueObjectHelper addressHelper;

		#endregion

		#endregion
	}
}
