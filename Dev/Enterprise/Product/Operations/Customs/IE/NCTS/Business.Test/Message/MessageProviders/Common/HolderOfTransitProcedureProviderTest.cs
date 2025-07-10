using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class HolderOfTransitProcedureProviderTest : Customs.Business.Testing.DataProviderTestCase<HolderOfTransitProcedureProvider>
	{
		public void TestHolderId()
		{
			CombineAssertions("Declaration Type = TIR", () =>
			{
				movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
				AssertEquals("Holder Id should be set when BM_InBondEntryType = TIR", "TIR123", GetProvider().HolderId);
			});

			CombineAssertions("Declaration Type = TIR, No EORI", () =>
			{
				nctsHeader.Principal.Organisation.CustomsCodes.Where(c => c.OK_CodeType == NctsPhase5DeclarationTypeList.Codes.TIR)?.DeleteAll();
				AssertNull("Holder Id should be null when Ok_CodeType not TIR", GetProvider().HolderId);
			});

			CombineAssertions("Declaration Type = AAA", () =>
			{
				movementHeader.BM_InBondEntryType = "AAA";
				AssertNull("Holder Id should be null when BM_InBondEntryType not TIR", GetProvider().HolderId);
			});
		}

		public void TestName()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Name should only be sent when Id is null", string.Empty, Provider.Id);
				AssertEquals("Name", "Test Company Limited", Provider.Name);
			});
		}

		public void TestAddress()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Address should only be sent when Id is null", string.Empty, Provider.Id);
				AssertEquals("Address.Line", "123 Test Street", Provider.Address.StreetAndNumber);
				AssertEquals("Address.PostcodeID", "A12B3C4", Provider.Address.Postcode);
				AssertEquals("Address.CityName", "City", Provider.Address.City);
				AssertEquals("Address.CountryCode", "IE", Provider.Address.Country);
			});
		}

		public void TestId()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Id should be empty when principal id is empty", string.Empty, GetProvider().Id);

				SetPrincipal("0123456789000");
				AssertEquals("Id should be set when code type = EOR", "IE0123456789000", GetProvider().Id);

				var codes = nctsHeader.Principal.Organisation.CustomsCodes.Find((OrgCusCode x) => x.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
				var code = codes.First();
				code.OK_CodeType = "XXX";
				AssertEquals("Id should be empty when code type does not match EOR or TCU", string.Empty, GetProvider().Id);

				code.OK_CodeType = OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU;
				AssertEquals("Id should be set when code type = TCU", "IE0123456789000", GetProvider().Id);

				SetPrincipal(string.Empty);
				AssertEquals("Id should be empty when principal is null", string.Empty, GetProvider().Id);
			});
		}

		public void TestContact()
		{
			var org = nctsHeader.Principal.Organisation;
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

		protected override HolderOfTransitProcedureProvider GetProvider() => HolderOfTransitProcedureProvider.New(nctsHeader.Principal, nctsHeader.MovementHeader.BM_InBondEntryType);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
			SetPrincipal(string.Empty);
		}

		void SetPrincipal(string id)
		{
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", nctsHeader.Principal, string.Empty, "Test Company Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", id, "TIR123");
		}

		NctsHeader nctsHeader;
		NctsDepartureMovementHeader movementHeader;
	}
}
