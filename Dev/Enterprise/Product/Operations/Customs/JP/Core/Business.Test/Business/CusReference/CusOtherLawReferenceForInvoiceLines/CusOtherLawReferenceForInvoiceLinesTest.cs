using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing;

[TestedType(typeof(CusOtherLawReferenceForInvoiceLines))]
sealed class CusOtherLawReferenceForInvoiceLinesTest : CusOtherLawReferenceTest
{
	public override void TestValidationType()
	{
		AssertType<CusOtherLawReferenceForInvoiceLinesValidation>(cusOtherLawReference.Validation);
	}

	public void TestCFR_Reference()
	{
		cusOtherLawReference.CFR_Reference = "MS";

		AssertEquals(1, entryInstruction.ApprovalCertificateInfos.Count);
		AssertEquals("MOTS", entryInstruction.ApprovalCertificateInfos[0].CSI_Code);

		cusOtherLawReference.CFR_Reference = "MM";
		AssertEquals(1, entryInstruction.ApprovalCertificateInfos.Count);
	}

	public override void TestGetCodeDescription()
	{
		var newFactory = new BusinessObjectFactory();
		var helper = new UniversalReferenceTestDataHelper(newFactory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "TestDescritpion", Core.Constants.CountryCodes.Japan);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "C1", "Description1", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2), "IsExport", "Y");
		newFactory.Save();

		cusOtherLawReference.CFR_Reference = "C1";
		AssertEquals("Description1", cusOtherLawReference.CodeDescription);
	}

	protected override void SetUp()
	{
		var declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		cusOtherLawReference = BusinessObject as CusOtherLawReferenceForInvoiceLines;
		cusOtherLawReference.CFR_ParentTableCode = invoiceLine.TablePrefix;
		cusOtherLawReference.CFR_ParentID = invoiceLine.PK;
	}

	CusEntryInstruction entryInstruction;
}
