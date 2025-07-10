using System;
using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE170RepresentativeProviderTest : Customs.Business.Testing.DataProviderTestCase<IE170RepresentativeProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("NctsHeader missing", () => new IE170RepresentativeProvider(null));

				var header = Factory.New<NctsHeader>();
				AssertExceptionThrown<ArgumentException>("MovementHeader missing", () => new IE170RepresentativeProvider(header));
			});
		}

		public void TestId()
		{
			CombineAssertions(() =>
			{
				var codes = Rep.Organisation.CustomsCodes.Find((OrgCusCode x) => x.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
				var code = codes.First();
				code.OK_CodeType = "XXX";
				AssertNull("Id should be null when OK_CodeType not EOR", Provider.Id);

				code.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				AssertEquals("Id should be set when OK_CodeType = EOR", "IE0123456789000", Provider.Id);

				SetRepresentative(string.Empty);
				AssertNull("Id should be null when no customs codes", Provider.Id);
			});
		}

		public void TestStatus()
		{
			var address = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TEST123");
			var addressPk = address.PK;
			nctsHeader.Principal.E2_OA_Address = addressPk;

			CombineAssertions(() =>
			{
				AssertEquals("Status should equal '3' when holder (principal) and representative are different", "3", Provider.Status);
				Rep.E2_OA_Address = addressPk;
				AssertEquals("Status should equal '2' when holder (principal) and representative are the same", "2", Provider.Status);
			});
		}

		public void TestContact()
		{
			var org = Rep.Organisation;
			var orgAddress = org.MainAddress;
			orgAddress.OA_Email = "test@example.com";
			orgAddress.OA_Phone = "5551234";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Joe Bloggs";

			CombineAssertions(() =>
			{
				AssertEquals("Contact", "Joe Bloggs", Provider.Contact.Name);
				AssertEquals("Phone", "5551234", Provider.Contact.PhoneNumber);
				AssertEquals("Email", "test@example.com", Provider.Contact.EmailAddress);
			});
		}

		protected override IE170RepresentativeProvider GetProvider() => new IE170RepresentativeProvider(nctsHeader);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			SetRepresentative("0123456789000");
		}

		void SetRepresentative(string id)
		{
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", Rep, string.Empty, "Test Company Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", id);
		}

		NctsHeader nctsHeader;
		JobDocAddress Rep => nctsHeader.MovementHeader.Representative;
	}
}
