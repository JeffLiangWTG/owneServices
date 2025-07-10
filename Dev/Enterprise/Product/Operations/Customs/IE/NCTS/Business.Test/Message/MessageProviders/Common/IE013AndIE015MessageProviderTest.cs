using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using CusAuthorizationUsage = Enterprise.Customs.EU.NCTS.Business.CusAuthorizationUsage;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE013AndIE015MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<IE013AndIE015MessageProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("NctsHeader missing", () => new IE015MessageProvider(null));

				var header = Factory.New<NctsHeader>();
				AssertExceptionThrown<ArgumentException>("MovementHeader missing", () => new IE015MessageProvider(header));
			});
		}

		public void TestIsInTransitionPeriod()
		{
			AssertEquals("Not In Transition Period", false, GetProvider().IsInTransitionPeriod);
			NCTSConditionalFunctionalityTestHelper.CombineAssertionsInPhase5TransitionPeriod(() =>
			{
				AssertEquals("In Transition Period", true, GetProvider().IsInTransitionPeriod);
			});
		}

		public void TestAuthorisations()
		{
			TestAuthorizationsInner("Phase5, CusAuthorizationUsages are on NctsDepartureMovementHeader", nctsHeader.MovementHeader.CusAuthorizationUsages);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			TestAuthorizationsInner("Phase4, CusAuthorizationUsages are on NctsHeader", nctsHeader.CusAuthorizationUsages);

			void TestAuthorizationsInner(string testCaseDescription, IBusinessObjectCollection<CusAuthorizationUsage> cusAuthorizationUsages)
			{
				var auth1 = cusAuthorizationUsages.AddNew();
				auth1.AGC_Code = "ACR";
				auth1.AGC_Number = "ABCD1234";

				var auth2 = cusAuthorizationUsages.AddNew();
				auth2.AGC_Code = "SSE";
				auth2.AGC_Number = "WXYZ5678";

				var provider = GetProvider();
				CombineAssertions(testCaseDescription, () =>
				{
					AssertEquals("Authorisation Count", 2, provider.Authorisations.Count);
					var auths = provider.Authorisations.ToArray();

					AssertEquals("Auth 1 Type", "ACR", auths[0].IdentificationType);
					AssertEquals("Auth 1 Number", "ABCD1234", auths[0].ReferenceNumber);

					AssertEquals("Auth 2 Type", "SSE", auths[1].IdentificationType);
					AssertEquals("Auth 2 Number", "WXYZ5678", auths[1].ReferenceNumber);
				});
			}
		}

		public void TestDepartureOffice()
		{
			AssertEquals("Departure office", "IEDUB100", Provider.DepartureOffice);
		}

		public void TestDestinationOffice()
		{
			movementHeader.CustomsOffices.AddNew("DES", "IEGRN100");

			var provider = GetProvider();
			AssertEquals("Destination Office Code", "IEGRN100", provider.DestinationOffice);
		}

		public void TestTransitOffices()
		{
			var transitOffices = Provider.TransitOffices.ToArray();
			AssertEquals("Two transit offices", 2, transitOffices.Length);
			var transitOffice1 = transitOffices[0];
			var transitOffice2 = transitOffices[1];
			if (transitOffice2.OfficeCode == "IEABC222")
			{
				transitOffice2 = transitOffices[0];
				transitOffice1 = transitOffices[1];
			}

			AssertEquals("transitOffice1.OfficeCode", "IEABC222", transitOffice1.OfficeCode);
			AssertEquals("transitOffice1.ETA", ZDateTime.BrettsBirthday, transitOffice1.ETA);
			AssertEquals("transitOffice2.OfficeCode", "GBBLK246", transitOffice2.OfficeCode);
			AssertEquals("transitOffice2.ETA", ZDateTime.BrettsBirthday.AddDays(2), transitOffice2.ETA);
		}

		public void TestExitOffices()
		{
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
			AssertSame("Do not send element when BM_InBondEntryType = TIR", Array.Empty<string>(), GetProvider().ExitOffices);
			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
			AssertSame("Do not send element when BM_InBondEntryType = T2", Array.Empty<string>(), GetProvider().ExitOffices);
			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2F;
			AssertContainsExactElementsInAnyOrder("Exit office codes", new[] { "AAA999", "BBB888" }, GetProvider().ExitOffices);
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertSame("Do not send element when BM_TypeOfSecurity is NON", Array.Empty<string>(), GetProvider().ExitOffices);
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertSame("Do not send element when BM_TypeOfSecurity is ENT", Array.Empty<string>(), GetProvider().ExitOffices);
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			AssertContainsExactElementsInAnyOrder("Exit office codes", new[] { "AAA999", "BBB888" }, GetProvider().ExitOffices);
		}

		public void TestHolderOfTheTransit()
		{
			CombineAssertions(() =>
			{
				var holder = Provider.HolderOfTheTransit;
				AssertType<HolderOfTransitProcedureProvider>(holder);
				AssertEquals("Identification Number", "IE0123456789000", holder.Id);
				AssertNull("TIR ID", holder.HolderId);
				AssertEquals("Name", "Test Company Limited", holder.Name);
				AssertEquals("Address.Line", "123 Test Street", holder.Address.StreetAndNumber);
				AssertEquals("Address.PostcodeID", "A12B3C4", holder.Address.Postcode);
				AssertEquals("Address.CityName", "City", holder.Address.City);
				AssertEquals("Address.CountryCode", "IE", holder.Address.Country);
				AssertEquals("Contact", "Joe Bloggs", holder.Contact.Name);
				AssertEquals("Phone", "5551234", holder.Contact.PhoneNumber);
				AssertEquals("Email", "test@example.com", holder.Contact.EmailAddress);
			});
		}

		public void TestHolderOfTheTransit_HolderId()
		{
			CombineAssertions("Declaration Type = TIR", () =>
			{
				movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
				var holder = GetProvider().HolderOfTheTransit;
				AssertEquals("TIR ID", "TIR123", holder.HolderId);
			});

			CombineAssertions("Declaration Type = AAA", () =>
			{
				movementHeader.BM_InBondEntryType = "AAA";
				var holder = GetProvider().HolderOfTheTransit;
				AssertNull("TIR ID", holder.HolderId);
			});
		}

		public void TestHolderOfTheTransit_NoEORI()
		{
			CombineAssertions("Declaration Type = TIR", () =>
			{
				nctsHeader.Principal.Organisation.CustomsCodes.Where(c => c.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori)?.DeleteAll();
				var holder = Provider.HolderOfTheTransit;
				AssertEquals("Identification Number", string.Empty, holder.Id);
				AssertEquals("Name", "Test Company Limited", holder.Name);
				AssertEquals("Address.Line", "123 Test Street", holder.Address.StreetAndNumber);
				AssertEquals("Address.PostcodeID", "A12B3C4", holder.Address.Postcode);
				AssertEquals("Address.CityName", "City", holder.Address.City);
				AssertEquals("Address.CountryCode", "IE", holder.Address.Country);
				AssertEquals("Contact", "Joe Bloggs", holder.Contact.Name);
				AssertEquals("Phone", "5551234", holder.Contact.PhoneNumber);
				AssertEquals("Email", "test@example.com", holder.Contact.EmailAddress);
			});
		}

		public void TestRepresentative()
		{
			AssertEquals("Representative ID", "IE0123456789000", Provider.Representative.Id);
			AssertEquals("Representative Status", "2", Provider.Representative.Status);
			AssertEquals("Representative Name", "Joe Bloggs", Provider.Representative.Contact.Name);
			AssertEquals("Representative Phone", "5551234", Provider.Representative.Contact.PhoneNumber);
			AssertEquals("Representative eMail", "test@example.com", Provider.Representative.Contact.EmailAddress);
		}

		public void TestGuarantees()
		{
			var guarantees = Provider.Guarantees.ToArray();
			AssertEquals("Number of guarantees", 2, guarantees.Length);
			var guaranteeData1 = guarantees[0];
			var guaranteeData2 = guarantees[1];
			if (guaranteeData2.GuaranteeType == "1")
			{
				guaranteeData2 = guarantees[0];
				guaranteeData1 = guarantees[1];
			}
			CombineAssertions("guaranteeData1", () =>
			{
				AssertNull("guaranteeData1 - Other guarantee reference", guaranteeData1.OtherGuaranteeReference);
				var providerReference = guaranteeData1.GuaranteeReferences.Single();
				AssertEquals("guaranteeData1 - GRN", "12345", providerReference.Grn);
				AssertEquals("guaranteeData1 - Access Code", "pass", providerReference.AccessCode);
				AssertEquals("guaranteeData1 - Amount", 123.45m, providerReference.AmountToBeCovered);
				AssertEquals("guaranteeData1 - Currency", "EUR", providerReference.Currency);
			});
			CombineAssertions("guaranteeData2", () =>
			{
				AssertEquals("guaranteeData2 - Other guarantee reference", "DEF456", guaranteeData2.OtherGuaranteeReference);
				var providerReference = guaranteeData2.GuaranteeReferences.Single();
				AssertEquals("guaranteeData2 - GRN", "67890", providerReference.Grn);
				AssertEquals("guaranteeData2 - Access Code", "word", providerReference.AccessCode);
				AssertEquals("guaranteeData2 - Amount", 98.76m, providerReference.AmountToBeCovered);
				AssertEquals("guaranteeData2 - Currency", "EUR", providerReference.Currency);
			});
		}

		public void TestConsignment()
		{
			movementHeader.BM_PortOfPresentationCode = "IE";
			movementHeader.BM_ForeignDestPortKCode = "GB";
			AssertEquals("Consignment dispatch", "IE", Provider.Consignment.PlaceOfLoading.Country);
			AssertEquals("Consignment destination", "GB", Provider.Consignment.PlaceOfUnloading.Country);
		}

		public void TestPreparationDateAndTime()
		{
			// TODO: TO BE IMPLEMENTED IN FUTURE WI
			Assert(true);
		}

		protected override IE013AndIE015MessageProvider GetProvider() => new IE013AndIE015MessageProviderForTest(nctsHeader);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			movementHeader = nctsHeader.MovementHeader;

			NCTSTestHelper.CreateCustomsOfficeForTest(movementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "IEDUB100", ZDateTime.Empty, true);
			NCTSTestHelper.CreateCustomsOfficeForTest(movementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "IEABC222", ZDateTime.BrettsBirthday, false);
			NCTSTestHelper.CreateCustomsOfficeForTest(movementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "GBBLK246", ZDateTime.BrettsBirthday.AddDays(2), false);
			NCTSTestHelper.CreateCustomsOfficeForTest(movementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit, "AAA999", ZDateTime.Empty, false);
			NCTSTestHelper.CreateCustomsOfficeForTest(movementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit, "BBB888", ZDateTime.Empty, false);
			var org = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", nctsHeader.Principal, string.Empty, "Test Company Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", "0123456789000", "TIR123");
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Joe Bloggs";
			contact.OC_Phone = "5551234";
			contact.OC_Email = "test@example.com";
			contact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			nctsHeader.Principal.Address.OA_Email = "test@example.com";
			nctsHeader.Principal.Address.OA_Phone = "5551234";
			nctsHeader.Principal.E2_Contact = "Joe Bloggs";
			movementHeader.Representative.OrganisationPK = nctsHeader.Principal.OrganisationPK;
			movementHeader.Representative.E2_Contact = "Joe Bloggs";
			movementHeader.Representative.Address.OA_Phone = "5551234";
			movementHeader.Representative.Address.OA_Email = "test@example.com";
			movementHeader.BM_RN_NKCountryOfDispatch = "IE";
			movementHeader.BM_RL_NKDestinationPort = "GB";
			SetUpGuarantees();
		}

		void SetUpGuarantees()
		{
			guarantee1 = movementHeader.Guarantees.AddNew();
			guarantee1.PW_BondType = "1";
			guarantee1.PW_BondNumber2 = "ABC123";
			guarantee1.PW_BondNumber = "12345";
			guarantee1.PW_Password = "pass";
			guarantee1.PW_BondAmount = 123.45M;
			guarantee1.PW_RX_NKCurrency = "EUR";

			guarantee2 = movementHeader.Guarantees.AddNew();
			guarantee2.PW_BondType = "3";
			guarantee2.PW_BondNumber2 = "DEF456";
			guarantee2.PW_BondNumber = "67890";
			guarantee2.PW_Password = "word";
			guarantee2.PW_BondAmount = 98.76M;
			guarantee2.PW_RX_NKCurrency = "GBP";
		}

		NctsHeader nctsHeader;
		NctsGuarantee guarantee1;
		NctsGuarantee guarantee2;
		NctsDepartureMovementHeader movementHeader;
	}

	class IE013AndIE015MessageProviderForTest : IE013AndIE015MessageProvider
	{
		public IE013AndIE015MessageProviderForTest(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}
	}
}
