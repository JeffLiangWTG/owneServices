using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.AIS.UCC5;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class IM415HeaderProviderTest : DataProviderTestCase<IM415HeaderProvider>
	{
		public void TestIIM415Header()
		{
			Assert("Should implement IIM415Header", Provider is IIM415Header);
		}

		public void TestDeclarationType()
		{
			SetUpTestData();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			AssertEquals("DeclarationType", "H1", Provider.DeclarationType);
		}

		public void TestImportOperation()
		{
			SetUpTestData();
			entryInstruction.CEI_Style = "AB";
			entryInstruction.CEI_SubStyle = "AES";
			declaration.JE_EntryStyle = "AI";
			declaration.JE_PaymentMethod = "A";
			entryHeader.CH_BGMReference = "Test";

			CombineAssertions(() =>
			{
				AssertEquals("MsgType", "AB", GetProvider().ImportOperation.MsgType);
				AssertEquals("DeclarationType", "AI", Provider.ImportOperation.DeclarationType);
				AssertEquals("AdditionalDeclarationType", "AES", Provider.ImportOperation.AdditionalDeclarationType);
				AssertEquals("LanguageCode", "EN", Provider.ImportOperation.LanguageCode);
				AssertEquals("PreferredPaymentMethod", "A", Provider.ImportOperation.PreferredPaymentMethod);
				AssertEquals("LRN", AISOutboundEDIMessage.LRNPlaceHolder, Provider.ImportOperation.LRN);
			});
		}

		public void TestAuthorisation8F()
		{
			SetUpTestData();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;
			cusCode.OK_CustomsRegNo = "EU1234567890";
			Factory.Save();

			var orgAddress = orgHeader.Addresses.Cast<OrgAddress>().First();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address1-1";
			orgAddress.OA_Address2 = "Address2-1";
			orgAddress.OA_PostCode = "PostCode1";
			orgAddress.OA_City = "City1";
			orgAddress.OA_RN_NKCountryCode = "IE";
			orgAddress.OA_CompanyNameOverride = "TestCompany1";
			entryInstruction.CEI_OH_Owner = orgHeader.PK;

			entryInstruction.ZG_PeriodForDischarge = 1;
			entryInstruction.ZG_PeriodForDischargeAutoExtension = true;
			entryInstruction.PeriodForDischargeDetails = "12";
			entryInstruction.ZG_BillOfDischargeIsNecessary = true;
			entryInstruction.ZG_BillOfDischargeDeadline = 1;
			entryInstruction.BillOfDischargeDetails = "23";

			CombineAssertions(() =>
			{
				var parties = Provider.Authorisation8F.Parties;
				AssertType<AddressWithNameProvider>("Type", parties.FirstOrDefault());
				AssertEquals("Count", 1, parties.Count);
				var party1 = parties.First();
				AssertEquals("StreetAndNumber", "Address1-1, Address2-1", party1.StreetAndNumber);
				AssertEquals("Postcode", "PostCode1", party1.Postcode);
				AssertEquals("City", "City1", party1.City);
				AssertEquals("Country", "IE", party1.Country);
				AssertEquals("Name", "TestCompany1", party1.Name);

				var dateTimesPeriodsAndPlaces = Provider.Authorisation8F.DateTimesPeriodsAndPlaces;
				AssertType<DateTimesPeriodsAndPlacesProvider>("Type", dateTimesPeriodsAndPlaces);
				AssertEquals("Period", "1", dateTimesPeriodsAndPlaces.PeriodForDischarge.Period);
				AssertEquals("AutomaticExtension", true, dateTimesPeriodsAndPlaces.PeriodForDischarge.AutomaticExtension);
				AssertEquals("Details", "12", dateTimesPeriodsAndPlaces.PeriodForDischarge.Details);
				AssertEquals("Period", true, dateTimesPeriodsAndPlaces.BillOfDischarge.UseOfTheBillOfDischarge);
				AssertEquals("Period", "1", dateTimesPeriodsAndPlaces.BillOfDischarge.Deadline);
				AssertEquals("Period", "23", dateTimesPeriodsAndPlaces.BillOfDischarge.Details);

				Assert("IdentificationOfGoods will be implemented in following work items", true);
				Assert("EconomicConditions will be implemented in following work items", true);
				Assert("DetailsOfPlannedActivities will be implemented in following work items", true);
				Assert("Others will be implemented in following work items", true);
			});
		}

		public void TestImporter()
		{
			SetUpTestData();
			var importerAddress = Factory.NewWithValidTestData<OrgAddress>();
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importerAddress.PK;

			importerAddress.OA_CompanyNameOverride = "ImporterName";
			importerAddress.OA_Address1 = "Addr1";
			importerAddress.OA_Address2 = "Addr2";
			importerAddress.OA_PostCode = "1234";
			importerAddress.OA_City = "DUBLIN";
			importerAddress.OA_RN_NKCountryCode = "IE";

			AssertNull(Provider.Importer.Id);
			AssertEquals("ImporterName", Provider.Importer.Name);
			AssertEquals("Addr1, Addr2", Provider.Importer.Address.StreetAndNumber);
			AssertEquals("1234", Provider.Importer.Address.Postcode);
			AssertEquals("DUBLIN", Provider.Importer.Address.City);
			AssertEquals("IE", Provider.Importer.Address.Country);
		}

		public void TestDeclarant()
		{
			SetUpTestData();
			var declarantAddress = Factory.NewWithValidTestData<OrgAddress>();
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

			declarantAddress.OA_CompanyNameOverride = "DeclarantName";
			declarantAddress.OA_Address1 = "Addr1";
			declarantAddress.OA_Address2 = "Addr2";
			declarantAddress.OA_PostCode = "1234";
			declarantAddress.OA_City = "DUBLIN";
			declarantAddress.OA_RN_NKCountryCode = "IE";

			var docAddress = declarantAddress.Header.Contacts.AddNew();
			docAddress.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			docAddress.OC_ContactName = "Joe Bloggs";
			docAddress.OC_Phone = "555 12345";
			docAddress.OC_Email = "test@example.com";

			AssertNull(Provider.Declarant.Id);
			AssertEquals("DeclarantName", Provider.Declarant.Name);
			AssertEquals("Addr1, Addr2", Provider.Declarant.Address.StreetAndNumber);
			AssertEquals("1234", Provider.Declarant.Address.Postcode);
			AssertEquals("DUBLIN", Provider.Declarant.Address.City);
			AssertEquals("IE", Provider.Declarant.Address.Country);
			AssertEquals("Joe Bloggs", Provider.Declarant.ContactPerson.Name);
			AssertEquals("555 12345", Provider.Declarant.ContactPerson.PhoneNumber);
			AssertEquals("test@example.com", Provider.Declarant.ContactPerson.EmailAddress);
		}

		public void TestPersonProvidingAGuaranteeID()
		{
			SetUpTestData();
			declaration.DefermentPartyDocAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			declaration.DefermentPartyDocAddress.Address.CustomsCodes.AddNew("EOR", "IE123456789", "IE");
			AssertEquals("IE123456789", Provider.PersonProvidingAGuaranteeID);
		}

		public void TestPersonPayingCustomsDutyID()
		{
			SetUpTestData();
			var dutyPayer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_DutyPayer = dutyPayer.PK;
			dutyPayer.CustomsCodes.AddNew("EOR", "IE123456789", "IE");
			AssertEquals("IE123456789", Provider.PersonPayingCustomsDutyID);
		}

		public void TestRepresentative()
		{
			SetUpTestData();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew("EOR", "IE123456789", "IE");
			var address = orgHeader.MainAddress;
			address.OA_CompanyNameOverride = "Test Co";
			address.OA_Address1 = "Address 1";
			address.OA_Address2 = "Address 2";
			address.OA_City = "Dublin";
			address.OA_PostCode = "A12B3C4";
			address.OA_RN_NKCountryCode = "IE";

			declaration.JE_OA_Representative = address.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			var contact = orgHeader.Contacts.AddNew();
			contact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			contact.OC_ContactName = "Joe Bloggs";
			contact.OC_Phone = "555 12345";
			contact.OC_Email = "test@example.com";

			var representative = Provider.Representative;
			AssertEquals("EORI", "IE123456789", representative.Id);
			AssertEquals("Status", "2", representative.Status);
			var contactPerson = Provider.Representative.ContactPerson;
			AssertEquals("Contact : Name", "Joe Bloggs", contactPerson.Name);
			AssertEquals("Contact : Phone", "555 12345", contactPerson.PhoneNumber);
			AssertEquals("Contact : Email", "test@example.com", contactPerson.EmailAddress);
		}

		protected override IM415HeaderProvider GetProvider()
		{
			SetUpTestData();
			return new IM415HeaderProvider(new AISMessageSendingAction(entryHeader));
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
				entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
