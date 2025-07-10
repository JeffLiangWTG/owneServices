using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Declaration.Testing;
using Enterprise.MasterFiles.Business;
using Moq.Protected;

namespace Enterprise.Customs.GB.CDS.Declaration.Testing
{
	public class CDSCusEntryInstructionValidationTests : CusEntryInstructionValidationTests
	{
		public void TestValidationErrorIsShowWhenEntryInstructionNotUsed()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invoice = dec.Invoices.AddNew();
			var inv1 = invoice.InvoiceLines.AddNew();
			var inv2 = invoice.InvoiceLines.AddNew();

			var cusEntryInstruction = dec.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_Style = "H1";

			AssertHasMessageError(cusEntryInstruction.CEI_StyleInfo, "This Entry Instruction has not been used on an invoice line");

			inv1.JI_CEI = cusEntryInstruction.PK;

			cusEntryInstruction.Validation.ValidateCEI_Style();
			AssertNoMessageError(cusEntryInstruction.CEI_StyleInfo, "This Entry Instruction has not been used on an invoice line");
		}

		public void TestCheck_CurrentLocationFor523()
		{
			var mockCEI = Factory.NewMoq<CusEntryInstruction>();
			mockCEI.Setup(m => m.CurrentLocationFor523).Returns("1234");
			mockCEI.Protected().Setup<Customs.Business.CusEntryInstructionValidation>("GetNewValidation")
				.Returns(new CDSCusEntryInstructionValidation(mockCEI.Object));

			mockCEI.Object.RunPreSaveValidation();
			AssertHasMessageError(mockCEI.Object.CurrentLocationFor523Info, locationOfGoodValidationWarning);

			mockCEI.Setup(m => m.CurrentLocationFor523).Returns("12345");
			mockCEI.Object.RunPreSaveValidation();
			AssertNoMessageError(mockCEI.Object.CurrentLocationFor523Info, locationOfGoodValidationWarning);

			mockCEI.Setup(m => m.CurrentLocationFor523).Returns("1");
			mockCEI.Object.RunPreSaveValidation();
			AssertHasMessageError(mockCEI.Object.CurrentLocationFor523Info, locationOfGoodValidationWarning);

			mockCEI.Setup(m => m.CurrentLocationFor523).Returns("");
			mockCEI.Object.RunPreSaveValidation();
			AssertNoMessageError(mockCEI.Object.CurrentLocationFor523Info, locationOfGoodValidationWarning);

			mockCEI.Setup(m => m.CurrentLocationFor523).Returns("A B C D");
			mockCEI.Object.RunPreSaveValidation();
			AssertNoMessageError(mockCEI.Object.CurrentLocationFor523Info, locationOfGoodWhitespaceValidationWarning);

			mockCEI.Setup(m => m.CurrentLocationFor523).Returns("AB   CD");
			mockCEI.Object.RunPreSaveValidation();
			AssertHasMessageError(mockCEI.Object.CurrentLocationFor523Info, locationOfGoodWhitespaceValidationWarning);
			mockCEI.VerifyAll();
		}

		public void TestCheck_CEI_Style_H8_ImporterHas_TrustedTraderNumber()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			_ = importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TST");
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_OH_Importer = importer.PK;
			Factory.Save();

			var cusEntryInstruction = dec.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.ReducedDataSetDeclaration;
			AssertHasMessageError(cusEntryInstruction.CEI_StyleInfo, "H8 declaration requires that the importer is a Trusted Trader, indicated by the presence of a TTD configuration record.");

			_ = importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TrustedTrader, "TTD");
			cusEntryInstruction.Validation.ValidateCEI_Style();
			AssertNoMessageError(cusEntryInstruction.CEI_StyleInfo, "H8 declaration requires that the importer is a Trusted Trader, indicated by the presence of a TTD configuration record.");
		}

		public void TestCheck_CEIPackageCount()
		{
			cei.CEI_Style = "H1";
			cei.CEI_PackageCount = 0;
			AssertHasMessageError(cei.CEI_PackageCountInfo, "Package Count must not be zero.");
			cei.CEI_PackageCount = 10;
			AssertNoMessageError(cei.CEI_PackageCountInfo, "Package Count must not be zero.");
		}

		const string locationOfGoodValidationWarning = "Location of goods for 5/23 is required.For warehouses, select an organisation in the warehouse field(s) below; for other locations such as ports and transit sheds, supply data on the main declaration screen.";
		const string locationOfGoodWhitespaceValidationWarning = "Location of goods for 5/23 appears incorrect";

		protected override void SetUp()
		{
			base.SetUp();
			dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			cei = dec.CustomsEntryInstructions.AddNew();
		}
	}
}
