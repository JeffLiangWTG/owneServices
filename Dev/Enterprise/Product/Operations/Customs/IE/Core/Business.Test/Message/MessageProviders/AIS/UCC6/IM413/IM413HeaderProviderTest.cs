using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class IM413HeaderProviderTest : DataProviderTestCase<IM413HeaderProvider>
	{
		public void TestIIM413Header()
		{
			Assert("Should implement IIM413Header", Provider is IIM413Header);
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
				AssertEquals("LRN", "Test", Provider.ImportOperation.LRN);
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

		protected override IM413HeaderProvider GetProvider()
		{
			SetUpTestData();
			return new IM413HeaderProvider(new AISMessageSendingAction(entryHeader));
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
				entryInstruction = declaration.CustomsEntryInstructions.AddNew();
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
