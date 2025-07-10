using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(AsycudaBillScreening))]
	sealed class AsycudaBillScreeningTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadOrCreateFacilityPlace()
		{
			var facilityPlace = billScreening.FacilityPlace;
			facilityPlace.E2_OA_Address = Factory.NewWithValidTestData<OrgAddress>().PK;
			CombineAssertions(() =>
			{
				AssertType<ICS2JobDocAddress>("Type", facilityPlace);
				AssertEquals("E2_ParentID", billScreening.PK, facilityPlace.E2_ParentID);
				AssertEquals("E2_ParentTableCode", billScreening.TablePrefix, facilityPlace.E2_ParentTableCode);
				AssertEquals("E2_AddressType", DocAddressTypes.Codes.ICS2FacilityPlace, facilityPlace.E2_AddressType);
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var loadedBillScreening = newFactory.Load<AsycudaBillScreening>(billScreening.PK);
				var loadedFacilityPlace = loadedBillScreening.FacilityPlace;
				AssertEquals("Same Location Address", facilityPlace.PK, loadedFacilityPlace.PK);
			});
		}

		public void TestCaptionResourceString()
		{
			billScreening = Factory.New<AsycudaBillScreening>();

			CombineAssertions("fields caption", () =>
			{
				var resultResAttribute = billScreening.ASR_ResultInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Result", resultResAttribute.Caption);

				var personTypeResAttribute = billScreening.ASR_AuthorizedPersonTypeInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Authorized Person Type", personTypeResAttribute.Caption);
				AssertEquals("Person Type", personTypeResAttribute.ShortCaption);

				var personResAttribute = billScreening.ASR_PER_AuthorizedPersonInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Authorized Person", personResAttribute.Caption);
				AssertEquals("Person", personResAttribute.ShortCaption);

				var personIdentifierAttribute = billScreening.ASR_AuthorizedPersonIdentifierInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Identifier", personIdentifierAttribute.Caption);

				var personNameAttribute = billScreening.ASR_AuthorizedPersonNameInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Name", personNameAttribute.Caption);

				var transportDocumentTypeResAttribute = billScreening.ASR_TransportNumberTypeInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Transport Document (House) Type", transportDocumentTypeResAttribute.Caption);
				AssertEquals("Document Type", transportDocumentTypeResAttribute.ShortCaption);

				var transportDocumentReferenceNumber = billScreening.ASR_TransportNumberInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Transport Document (House) Reference Number", transportDocumentReferenceNumber.Caption);
				AssertEquals("Reference Number", transportDocumentReferenceNumber.ShortCaption);
			});
		}

		public void TestAuthorizedPersonReadOnlyBasedOnAuthorizedPersonType()
		{
			billScreening = Factory.New<AsycudaBillScreening>();

			billScreening.ASR_AuthorizedPersonType = EUICS2ScreeningAuthorizedPersonTypes.Codes.AP1;
			Assert(!billScreening.ASR_PER_AuthorizedPersonInfo.ReadOnly);

			billScreening.ASR_AuthorizedPersonType = EUICS2ScreeningAuthorizedPersonTypes.Codes.AP2;
			Assert(!billScreening.ASR_PER_AuthorizedPersonInfo.ReadOnly);

			billScreening.ASR_AuthorizedPersonType = EUICS2ScreeningAuthorizedPersonTypes.Codes.AP3;
			Assert(billScreening.ASR_PER_AuthorizedPersonInfo.ReadOnly);
		}

		public void TestAuthorizedPersonNameReadOnlyIfAuthorizedPersonIsNotBlank()
		{
			billScreening = Factory.New<AsycudaBillScreening>();
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();

			billScreening.ASR_PER_AuthorizedPerson = Guid.Empty;
			Assert(!billScreening.ASR_AuthorizedPersonNameInfo.ReadOnly);

			billScreening.ASR_PER_AuthorizedPerson = glbPerson.PK;
			Assert(billScreening.ASR_AuthorizedPersonNameInfo.ReadOnly);
		}

		public void TestCheckAuthorizedPersonNameDefaultsFromGlbPerson()
		{
			var billScreening = Factory.New<AsycudaBillScreening>();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Full Name";

			AssertEquals("Precondition: has no name by default", string.Empty, billScreening.ASR_AuthorizedPersonName);

			billScreening.ASR_AuthorizedPersonType = "1";
			billScreening.ASR_PER_AuthorizedPerson = person.PK;
			AssertEquals("The name field is populated from the person's full name", "Full Name", billScreening.ASR_AuthorizedPersonName);
		}

		public void TestCheckAuthorizedPersonIdentifierDefaultsFromPassportNumber()
		{
			var billScreening = Factory.New<AsycudaBillScreening>();
			var person = Factory.NewWithValidTestData<GlbPerson>();

			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_PER = person.PK;

			var certificate = orgContact.Certificates.AddNew();
			certificate.XZ_Type = "PAS";
			certificate.XZ_RefNumber = "123456";

			AssertEquals("Precondition: identifier is empty by default", string.Empty, billScreening.ASR_AuthorizedPersonIdentifier);

			billScreening.ASR_AuthorizedPersonType = "1";
			billScreening.ASR_PER_AuthorizedPerson = person.PK;
			AssertEquals("The identifier field is populated from the passport number", "123456", billScreening.ASR_AuthorizedPersonIdentifier);
		}

		public void TestCheckAuthorizedPersonIdentifierOnlyReadsPASTypeCertificates()
		{
			var billScreening = Factory.New<AsycudaBillScreening>();
			var person = Factory.NewWithValidTestData<GlbPerson>();

			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_PER = person.PK;

			var certificate = orgContact.Certificates.AddNew();
			certificate.XZ_Type = "CAR";
			certificate.XZ_RefNumber = "123456";

			AssertEquals("Precondition: identifier is empty by default", string.Empty, billScreening.ASR_AuthorizedPersonIdentifier);

			billScreening.ASR_AuthorizedPersonType = "1";
			billScreening.ASR_PER_AuthorizedPerson = person.PK;
			AssertEquals("Defaults to empty string since there are no certificates with PAS XZ_Type", string.Empty, billScreening.ASR_AuthorizedPersonIdentifier);
		}

		public void TestCheckAuthorizedPersonNameAndIdentifierAutoTruncates()
		{
			var billScreening = Factory.New<AsycudaBillScreening>();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ";

			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_PER = person.PK;

			var certificate = orgContact.Certificates.AddNew();
			certificate.XZ_Type = "PAS";
			certificate.XZ_RefNumber = "1234567890111213141516171";

			CombineAssertions("Precondition: both fields exceed the character maximum on the bill screening", () =>
			{
				Assert(person.PER_FullName.Length > AsycudaBillScreeningSchema.ASR_AuthorizedPersonName.MaxLength);
				Assert(certificate.XZ_RefNumber.Length > AsycudaBillScreeningSchema.ASR_AuthorizedPersonIdentifier.MaxLength);
			});

			billScreening.ASR_AuthorizedPersonType = "1";
			billScreening.ASR_PER_AuthorizedPerson = person.PK;

			CombineAssertions("Both fields on the bill screening will be automatically truncated to fit", () =>
			{
				Assert(person.PER_FullName.Length > AsycudaBillScreeningSchema.ASR_AuthorizedPersonName.MaxLength);
				Assert(certificate.XZ_RefNumber.Length > AsycudaBillScreeningSchema.ASR_AuthorizedPersonIdentifier.MaxLength);
			});
		}

		public void TestLookups()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var billScreening = bill.BillScreenings.AddNew();
			AssertType<AsycudaBillScreeningLookups>(billScreening.Lookups);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.BillScreenings.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			billScreening = Factory.NewWithValidTestData<AsycudaBillScreening>();
			billScreening.ASR_ClusterKey = 1;
		}
		AsycudaBillScreening billScreening;
	}
}
