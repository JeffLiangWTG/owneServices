using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using static Enterprise.Customs.FR.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.FR.Business.MasterFiles.Testing
{
	public class SupplementaryCodeValidationTest : EU.Business.Testing.SupplementaryCodeValidationTest
	{
		public void TestCheckSupplementaryCode2ForRuleNat_043()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_SupplementaryCode2 = "0090";

			var supplementaryCodeLoader = new Customs.Business.BaseSupplementaryCode.Loader(Factory);

			var supplement2 = supplementaryCodeLoader.Load<SupplementaryCode, JobComInvoiceLine>(invoiceLine, 2);
			AssertHasMessageError(supplement2.CY_CodeInfo, "You have applied for CANA 0090, so the special mention \"G0008 unidentified VAT payable in France\" must be served at GS level.");

			var addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "G0007";
			supplement2.Validation.ValidateCY_Code();
			AssertHasMessageError(supplement2.CY_CodeInfo, "You have applied for CANA 0090, so the special mention \"G0008 unidentified VAT payable in France\" must be served at GS level.");

			addInfo.CSI_Code = RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;
			supplement2.Validation.ValidateCY_Code();
			AssertNoMessageError(supplement2.CY_CodeInfo, "You have applied for CANA 0090, so the special mention \"G0008 unidentified VAT payable in France\" must be served at GS level.");

			addInfo.Delete();
			invoiceLine.JI_SupplementaryCode2 = "0091";
			AssertNoMessageError(supplement2.CY_CodeInfo, "You have applied for CANA 0090, so the special mention \"G0008 unidentified VAT payable in France\" must be served at GS level.");

			invoiceLine.JI_SupplementaryCode1 = "0090";
			var supplement1 = supplementaryCodeLoader.Load<SupplementaryCode, JobComInvoiceLine>(invoiceLine, 1);
			AssertNoMessageError(supplement1.CY_CodeInfo, "You have applied for CANA 0090, so the special mention \"G0008 unidentified VAT payable in France\" must be served at GS level.");

			invoiceLine.JI_SupplementaryCode2 = "0090";
			var addInfo2 = invoiceHeader.AdditionalInfos.AddNew();
			addInfo2.CSI_Code = "G0007";
			supplement2.Validation.ValidateCY_Code();
			AssertHasMessageError(supplement2.CY_CodeInfo, "You have applied for CANA 0090, so the special mention \"G0008 unidentified VAT payable in France\" must be served at GS level.");

			addInfo2.CSI_Code = RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;
			supplement2.Validation.ValidateCY_Code();
			AssertNoMessageError(supplement2.CY_CodeInfo, "You have applied for CANA 0090, so the special mention \"G0008 unidentified VAT payable in France\" must be served at GS level.");

			addInfo2.Delete();

			var addInfo3 = declaration.AdditionalInfos.AddNew();
			addInfo3.CSI_Code = "G0007";
			supplement2.Validation.ValidateCY_Code();
			AssertHasMessageError(supplement2.CY_CodeInfo, "You have applied for CANA 0090, so the special mention \"G0008 unidentified VAT payable in France\" must be served at GS level.");

			addInfo3.CSI_Code = RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;
			supplement2.Validation.ValidateCY_Code();
			AssertNoMessageError(supplement2.CY_CodeInfo, "You have applied for CANA 0090, so the special mention \"G0008 unidentified VAT payable in France\" must be served at GS level.");
		}
	}
}
