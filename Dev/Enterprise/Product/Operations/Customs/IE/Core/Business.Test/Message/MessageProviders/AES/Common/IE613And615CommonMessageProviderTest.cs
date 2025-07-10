using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	abstract class IE613And615CommonMessageProviderTest<T> : Customs.Business.Testing.DataProviderTestCase<T>
		where T : IE613And615CommonMessageProvider
	{
		public void TestSpecificCircumstanceIndicator()
		{
			declaration.ZG_SpecificCircumstanceIndicator = "A20";
			AssertEquals("SpecificCircumstanceIndicator", "A20", Provider.SpecificCircumstanceIndicator);
		}

		public void TestCustomsOfficeOfExitDeclared()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "IE000003");
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.SupervisingOffice, "IE000002");
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.ActualExitOffice, "IE000001");
			AssertEquals("CustomsOfficeOfExitDeclared", "IE000003", Provider.CustomsOfficeOfExitDeclared);
		}

		public void TestCustomsOfficeOfLodgement()
		{
			declaration.JE_CustomsOffice = "Export22";
			AssertEquals("CustomsOfficeOfLodgement", "Export22", Provider.CustomsOfficeOfLodgement);
		}

		public void TestConsignment()
		{
			AssertType<IE613And615ConsignmentProvider>("Consignment", Provider.Consignment);
		}

		public void TestDeclarant()
		{
			var declarantHeader = Factory.New<OrgHeader>();
			declarantHeader.OH_FullName = "TestDeclarantHeader";
			var declarant = Factory.New<OrgAddress>();
			declaration.JE_OA_DeclarantAddress = declarant.PK;
			declarant.OA_OH = declarantHeader.PK;
			AssertEquals("Declarant", "TestDeclarantHeader", Provider.Declarant.Name);
		}

		public void TestRepresentative()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TestRepresentativeHeader";
			var address = Factory.New<OrgAddress>();
			address.OA_CompanyNameOverride = "TestRepresentativeHeader Override";
			declaration.JE_OA_Representative = address.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			address.OA_OH = org.PK;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "BOB THE BUILDER";
			CombineAssertions("Representantive", () =>
			{
				var representative = Provider.Representative;
				AssertEquals("Status", "2", representative.Status);
				AssertEquals("Contact.Name", "BOB THE BUILDER", representative.Contact.Name);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			(entryHeaderWrapper, _) = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			entryHeader = entryHeaderWrapper.EntryHeader;
			declaration = entryHeaderWrapper.Declaration;
			invoice = entryHeaderWrapper.RandomInvoiceHeader;
			instruction = entryHeaderWrapper.Instruction;
		}

		protected EntryHeaderWrapper entryHeaderWrapper;
		protected CusEntryHeader entryHeader;
		protected JobDeclaration declaration;
		protected CusEntryInstruction instruction;
		protected JobComInvoiceHeader invoice;
	}
}
