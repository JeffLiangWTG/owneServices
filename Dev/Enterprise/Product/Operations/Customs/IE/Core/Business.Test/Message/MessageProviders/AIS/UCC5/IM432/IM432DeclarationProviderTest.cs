using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	public class IM432DeclarationProviderTest : DataProviderTestCase<IM432DeclarationProvider>
	{
		public void TestCustomsOfficeLodgement()
		{
			declaration.JE_CustomsOffice = "DUB";
			AssertEquals("CustomsOfficeLodgement", "DUB", Provider.CustomsOfficeLodgement);
		}

		public void TestPresentationCustomsOffice()
		{
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation, "IEDUB100");
			AssertEquals("", "IEDUB100", Provider.PresentationCustomsOffice);
		}

		public void TestParties()
		{
			AssertType<IM432DeclarationProvider>("Type of Parties", Provider.Parties);
		}

		public void TestGrossMass()
		{
			invoiceLine.JI_Weight = 0.325m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Tonnes;

			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoiceLine2 = entryLine2.InvoiceLines.AddNew();
			invoiceLine2.JI_Weight = 0.300m;
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			invoiceLine2.JI_CL = entryLine.PK;

			AssertEquals("GrossMass", 625m, Provider.GrossMass);
		}

		public void TestLRN()
		{
			entryHeader.CH_BGMReference = "LRN2343234242";
			AssertEquals("LRN", "LRN2343234242", Provider.LRN);
		}

		public void TestGoodsPresentationPerson()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_FullName = "Test Declarant";
			declarant.CustomsCodes.AddNew("EOR", "IE123456781", "IE");
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			AssertEquals("GoodsPresentationPerson should return Declarant EORI", "IE123456781", Provider.GoodsPresentationPerson);
		}

		public void TestRepresentative()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew("EOR", "IE293847584930295", "IE");
			var address = orgHeader.MainAddress;
			address.OA_CompanyNameOverride = "Company Name";
			address.OA_Address1 = "Address 1";
			address.OA_Address2 = "Address 2";
			address.OA_City = "Dublin";
			address.OA_PostCode = "D02 PD90";
			address.OA_RN_NKCountryCode = "IE";

			declaration.JE_OA_Representative = address.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;

			var representative = Provider.Representative;
			AssertType<RepresentativeProvider>("Type of Representative", representative);
			AssertSame("Cached", representative, Provider.Representative);
		}

		public void TestAuthorisationHolder()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;
			cusCode.OK_CustomsRegNo = "1234567890";
			Factory.Save();
			var usages = entryInstruction.CusAuthorizationUsages.AddNew();
			usages.AGC_Code = "AS";
			usages.AGC_Number = "123";
			usages.AGC_OH_Owner = orgHeader.PK;

			AssertType<AuthorisationHolderProvider[]>("Type of AuthorisationHolder", Provider.AuthorisationHolder);
			AssertEquals("Count", 1, Provider.AuthorisationHolder.Count);
			var auth = Provider.AuthorisationHolder.First();
			AssertEquals("Code", "AS", auth.Code);
			AssertEquals("ID", "IE1234567890", auth.ID);
		}

		protected override void SetUp() => SetUpTestDataIfNeeded();

		protected override IM432DeclarationProvider GetProvider() => new IM432DeclarationProvider(new EntryHeaderWrapper(entryHeader));

		void SetUpTestDataIfNeeded()
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
