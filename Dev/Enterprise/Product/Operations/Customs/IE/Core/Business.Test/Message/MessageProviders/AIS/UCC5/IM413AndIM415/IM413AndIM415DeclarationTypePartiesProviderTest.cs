using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415DeclarationTypePartiesProviderTest : DataProviderTestCase<IM413AndIM415DeclarationTypePartiesProvider>
	{
		public void TestExporter()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "E007");
			var orgAddress = orgHeader.MainAddress;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = orgAddress.PK;
			AssertEquals("Exporter", "IEE007", Provider.Exporter.ID);
		}

		public void TestExporter_WithGBEORI()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TEST001";
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
			cusCode.OK_CustomsRegNo = "1234567890";
			var orgAddress = orgHeader.MainAddress;
			orgHeader.MainAddress.OA_City = "TestCity";
			declaration.SupplierDocumentaryAddress.E2_OA_Address = orgAddress.PK;
			AssertNull("Exporter", Provider.Exporter.ID);
			AssertEquals("Address", "TestCity", Provider.Exporter.Address.City);
		}

		public void TestDeclarant()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "D007");
			var orgAddress = orgHeader.MainAddress;
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			AssertEquals("Declarant", "IED007", Provider.Declarant.ID);
		}

		public void TestRepresentative()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "R007");
			var orgAddress = orgHeader.MainAddress;
			declaration.JE_OA_Representative = orgAddress.PK;
			declaration.JE_DeclarantType = "DIR";
			AssertEquals("Representative", "IER007", Provider.Representative.ID);
			AssertEquals("Representative.Status", "2", Provider.Representative.Status);
		}

		public void TestAuthorisationHolder()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "R007");

			var usage = entryInstruction.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = "UAT";
			usage.AGC_Number = "3";
			usage.AGC_OH_Owner = orgHeader.PK;

			AssertType<AuthorisationHolderProvider[]>(Provider.AuthorisationHolder);
			AssertEquals(1, Provider.AuthorisationHolder.Count);
			var auth = Provider.AuthorisationHolder.First();
			AssertEquals("Code", "UAT", auth.Code);
			AssertEquals("ID", "IER007", auth.ID);
		}

		public void TestPersonProvidingGuarantee()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "R007");
			var orgAddress = orgHeader.MainAddress;
			declaration.DefermentPartyDocAddress.E2_AddressOverride = false;
			declaration.DefermentPartyDocAddress.E2_OA_Address = orgAddress.PK;
			AssertEquals("Person Providing Guarantee", "IER007", Provider.PersonProvidingGuarantee);

			declaration.DefermentPartyDocAddress.E2_AddressOverride = true;
			declaration.DefermentPartyDocAddress.E2_GovRegNum = "IETEST";
			AssertEquals("Person Providing Guarantee", "IETEST", GetProvider().PersonProvidingGuarantee);
		}

		public void TestPersonPayingCustomsDuty()
		{
			SetUpTestData();
			AssertEquals("Person Paying Customs Duty not set", string.Empty, Provider.PersonPayingCustomsDuty);

			var orgHeader = Factory.New<OrgHeader>();
			declaration.JE_OH_DutyPayer = orgHeader.PK;
			AssertEquals("Person Paying Customs Duty EORI not set", string.Empty, Provider.PersonPayingCustomsDuty);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345678");
			AssertEquals("Person Paying Customs Duty EORI set", "IE12345678", Provider.PersonPayingCustomsDuty);
		}

		protected override IM413AndIM415DeclarationTypePartiesProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415DeclarationTypePartiesProvider(entryHeader);
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
