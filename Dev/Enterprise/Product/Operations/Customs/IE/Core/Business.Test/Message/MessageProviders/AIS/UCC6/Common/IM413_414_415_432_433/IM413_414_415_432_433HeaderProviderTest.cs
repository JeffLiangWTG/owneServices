using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class IM413_414_415_432_433HeaderProviderTest : DataProviderTestCase<IM413_414_415_432_433HeaderProvider>
	{
		public void TestFallbackProcedure()
		{
			AssertType<FallbackProcedureProvider>("IM415HeaderProvider.FallbackProcedure should return object of FallbackProcedure", Provider.FallbackProcedure);
		}

		public void TestCustomsOfficeOfPresentation()
		{
			SetUpTestData();
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation, "IEDUB100");
			AssertEquals("IEDUB100", Provider.CustomsOfficeOfPresentation);
		}

		public void TestSupervisingCustomsOffice()
		{
			SetUpTestData();
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.SupervisingOffice, "IEDUB100");
			AssertEquals("IEDUB100", Provider.SupervisingCustomsOffice);
		}

		public void TestCustomsOfficeLodgement()
		{
			SetUpTestData();
			declaration.JE_CustomsOffice = "IEDUB100";
			AssertEquals("IEDUB100", Provider.CustomsOfficeLodgement);
		}

		public void TestGoodsShipments()
		{
			SetUpTestData();
			var goodsShipment = Provider.GoodsShipments;
			AssertEquals("Count", 1, goodsShipment.Count);
			AssertSame("Cached", goodsShipment, Provider.GoodsShipments);
		}

		public void TestAuthorisations()
		{
			SetUpTestData();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;
			cusCode.OK_CustomsRegNo = "EU1234567890";
			Factory.Save();
			var usages = entryInstruction.CusAuthorizationUsages.AddNew();
			usages.AGC_Code = "AS";
			usages.AGC_Number = "123";
			usages.AGC_OH_Owner = orgHeader.PK;

			AssertEquals("Count", 1, Provider.Authorisations.Count);
			AssertEquals("Count", "AS", Provider.Authorisations.First().Type);
			AssertEquals("Count", "123", Provider.Authorisations.First().Reference);
			AssertEquals("Count", "EU1234567890", Provider.Authorisations.First().HolderOfTheAuthorisation);
		}

		public void TestImporter()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "E007");
			var orgAddress = orgHeader.MainAddress;
			declaration.JE_OH_Importer = orgHeader.PK;

			var importer = GetProvider().Importer;
			CombineAssertions(() =>
			{
				Assert("Should be IImporter", importer is IImporter);
				AssertType<ImporterProvider>(importer);
				AssertEquals("Id", "IEE007", importer.Id);
				AssertNull("Name should be null when EORI populated", importer.Name);
				AssertNull("Address should be null when EORI populated", importer.Address);
			});
		}

		public void TestDeclarant()
		{
			SetUpTestData();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IE293847584930295");
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			var declarant = Provider.Declarant;
			AssertEquals("Use data of declaration.Declarant", "IE293847584930295", declarant.Id);
			AssertType<MDeclarantProvider>(declarant);
			AssertSame("Cached", declarant, Provider.Declarant);
		}

		public void TestPersonProvidingAGuaranteeID()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "E008");
			var orgAddress = orgHeader.MainAddress;

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressType = "DFP";
			docAddress.E2_OA_Address = orgAddress.PK;
			declaration.DocAddresses.Add(docAddress);
			AssertEquals("Id", "IEE008", Provider.PersonProvidingAGuaranteeID);
		}

		public void TestPersonPayingCustomsDutyID()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "E009");
			declaration.JE_OH_DutyPayer = orgHeader.PK;
			AssertEquals("Id", "IEE009", Provider.PersonPayingCustomsDutyID);
		}

		public void TestRepresentative()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "E007");
			var customsContact = orgHeader.Contacts.AddNew();
			customsContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			customsContact.OC_ContactName = "Joe Bloggs";
			customsContact.OC_Phone = "+35312345678";
			customsContact.OC_Email = "joe@bloggs.ie";
			var orgAddress = orgHeader.MainAddress;
			declaration.JE_OA_Representative = orgAddress.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;

			var representative = GetProvider().Representative;
			CombineAssertions(() =>
			{
				Assert("Should be IMRepresentative", representative is IMRepresentative);
				AssertType<MRepresentativeProvider>(representative);
				AssertEquals("Id no address", "IEE007", representative.Id);
				AssertEquals("Status", "2", representative.Status);
				AssertEquals("Contact Name", "Joe Bloggs", representative.ContactPerson.Name);
				AssertEquals("Contact Phone Number", "+35312345678", representative.ContactPerson.PhoneNumber);
				AssertEquals("Contact Email Address", "joe@bloggs.ie", representative.ContactPerson.EmailAddress);
				orgAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IE293847584930295");
				representative = GetProvider().Representative;
				AssertEquals("Id address", "IE293847584930295", representative.Id);
			});
		}

		public void TestGuarantees()
		{
			SetUpTestData();
			entryInstruction.Guarantees.AddNew();
			var guarantee = Provider.Guarantees;
			AssertEquals("Count", 1, guarantee.Count);
			AssertSame("Cached", guarantee, Provider.Guarantees);
		}

		public void TestCurrencyExchange()
		{
			AssertEquals("CurrencyExchange", "EUR", Provider.CurrencyExchange);
		}

		public void TestDeferredPayments()
		{
			AssertType<DeferredPaymentProvider[]>(Provider.DeferredPayments);
		}

		protected override IM413_414_415_432_433HeaderProvider GetProvider()
		{
			SetUpTestData();
			return new IM413_414_415_432_433HeaderProvider(new AISMessageSendingAction(entryHeader));
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
