using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class IE570And573CommonMessageProviderBaseOnlyTest : Customs.Business.Testing.DataProviderTestCase<IE570And573CommonMessageProvider>
	{
		public void TestCustomsOfficeOfExitDeclared()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "IE000003");
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.SupervisingOffice, "IE000002");
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.ActualExitOffice, "IE000001");
			AssertEquals("CustomsOfficeOfExitDeclared", "IE000003", Provider.CustomsOfficeOfExitDeclared);
		}

		public void TestDeclarant()
		{
			var declarantHeader = Factory.New<OrgHeader>();
			declarantHeader.OH_FullName = "TestDeclarantHeader";
			declarantHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "REG111");
			declarantHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222");
			var declarant = Factory.New<OrgAddress>();
			declaration.JE_OA_DeclarantAddress = declarant.PK;
			declarant.OA_OH = declarantHeader.PK;
			AssertEquals("Declarant.Id", "IEREG222", Provider.Declarant.Id);
			AssertEquals("Declarant.Name", "TestDeclarantHeader", Provider.Declarant.Name);
		}

		public void TestRepresentative()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TestRepresentativeHeader";
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "REG111");
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222");
			var address = Factory.New<OrgAddress>();
			address.OA_CompanyNameOverride = "TestRepresentativeHeader Override";
			declaration.JE_OA_Representative = address.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			address.OA_OH = org.PK;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "BOB THE BUILDER";
			contact.OC_Phone = "+61 (2) 1234 5678";
			contact.OC_Email = "test@test.com";
			CombineAssertions("Representantive", () =>
			{
				var representative = Provider.Representative;
				AssertEquals("Declarant.Id", "IEREG222", representative.Id);
				AssertEquals("Status", "2", representative.Status);
				AssertEquals("Contact.Name", "BOB THE BUILDER", representative.Contact.Name);
				AssertEquals("Contact.Name", "+61 (2) 1234 5678", representative.Contact.PhoneNumber);
				AssertEquals("Contact.Name", "test@test.com", representative.Contact.EmailAddress);
			});
		}

		protected override IE570And573CommonMessageProvider GetProvider() => new IE570And573CommonMessageProviderForTest(entryHeader);

		protected override void SetUp()
		{
			base.SetUp();
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

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}

	class IE570And573CommonMessageProviderForTest : IE570And573CommonMessageProvider
	{
		public IE570And573CommonMessageProviderForTest(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}
	}
}
