using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing;

[TestedType(typeof(CusOtherLawReferenceForInvoiceLinesValidation))]
sealed class CusOtherLawReferenceForInvoiceLinesValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCFR_Reference_RoadTranportVehicleLaw()
	{
		var info = cusOtherLawReferenceForInvoiceLines.CFR_ReferenceInfo;
		entryInstruction.CEI_Style = "R";
		cusOtherLawReferenceForInvoiceLines.CFR_Reference = "MS";
		var expectedMessage = "Value is incompatible with Declaration Type.";
		var bondedAreaCode = JPRefCusCodeListTypes.GetJapanBondedAreaCode(Factory, TestDataHelper.CreateJapanBondedAreaCode(Factory));
		bondedAreaCode.Attributes.AddNew().ZZE_Value = "洋上";

		AssertHasMessageError(info, expectedMessage);

		entryInstruction.CEI_Style = "E";
		cusOtherLawReferenceForInvoiceLines.Validation.ValidateCFR_Reference();
		AssertNoMessageError(info, expectedMessage);

		expectedMessage = "Other Laws and Regulations Codes MS should not be entered when direct landing (when a Bonded Location Code of Type 洋上 is entered).";
		AssertNoMessageError(info, expectedMessage);
		entryInstruction.CEI_BondedLocationCode = bondedAreaCode.ZZD_Code;
		cusOtherLawReferenceForInvoiceLines.Validation.ValidateCFR_Reference();
		AssertHasMessageError(info, expectedMessage);

		expectedMessage = "Approval Certificate MOTS is required when Other Laws and Regulations Code MS is entered.";
		AssertEquals(1, entryInstruction.ApprovalCertificateInfos.Count);
		AssertEquals("MOTS", entryInstruction.ApprovalCertificateInfos[0].CSI_Code);
		AssertNoMessageError(info, expectedMessage);

		entryInstruction.ApprovalCertificateInfos.RemoveAll();
		cusOtherLawReferenceForInvoiceLines.Validation.ValidateCFR_Reference();
		AssertHasMessageError(info, expectedMessage);

		expectedMessage = "The MS and MM codes in the Other Laws and Regulations are mutually exclusive. Please remove one of them.";
		AssertNoMessageError(info, expectedMessage);
		var cusOtherLawReferenceForInvoiceLines2 = invoiceLine.OtherLaws.AddNew();
		cusOtherLawReferenceForInvoiceLines2.CFR_Reference = "MM";
		AssertHasMessageError(info, expectedMessage);
	}

	protected override void SetUp()
	{
		var declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		cusOtherLawReferenceForInvoiceLines = invoiceLine.OtherLaws.AddNew();
	}

	CusOtherLawReferenceForInvoiceLines cusOtherLawReferenceForInvoiceLines;
	CusEntryInstruction entryInstruction;
	JobComInvoiceLine invoiceLine;
}
