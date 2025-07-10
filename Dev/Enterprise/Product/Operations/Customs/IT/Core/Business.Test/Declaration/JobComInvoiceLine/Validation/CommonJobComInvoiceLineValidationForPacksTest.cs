using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CommonJobComInvoiceLineValidationForPacksTest : BusinessObjectValidationTestCase
{
	public void TestMessageErrorNoPackageDetails()
	{
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "1";
		var missingPacksInfoErrorMessage = "This line has no packaging details";

		var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine_ForTest>();
		invoiceLine.SetDeclarationForTesting(declaration);
		var validation = invoiceLine.GetValidationForTest();

		validation.ValidateAll();
		AssertHasRowMessageErrorContaining(invoiceLine, missingPacksInfoErrorMessage);

		var packagePivot = MergeTestHelper.GetNewPackagePivot(invoiceLine, packageCT, 0);
		AssertNoRowErrorContaining(invoiceLine, missingPacksInfoErrorMessage);
	}

	public void TestHasValidPackagePivots()
	{
		var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine_ForTest>();
		invoiceLine.SetDeclarationForTesting(declaration);
		var validation = invoiceLine.GetValidationForTest();

		AssertEquals("When no package pivot is available, HasValidPackagePivots_Exposed is false", false, validation.HasValidPackagePivots_Exposed);

		var packagePivot = MergeTestHelper.GetNewPackagePivot(invoiceLine, packageCT, 0);
		AssertEquals("When A package pivot is available, HasValidPackagePivots_Exposed is true", true, validation.HasValidPackagePivots_Exposed);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.NewWithValidTestData<JobDeclaration>();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.DisableDefaultPackingInformation = true;
		var container = declaration.CusContainers.AddNew();
		container.CO_ContainerNumber = "OOCL0000006";

		var declarationBill = declaration.Bills.AddNew();
		declarationBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
		declarationBill.CU_BillNum = "999";

		packageCT = (EU.Business.Declaration.Package)declaration.Packages.AddNew();
		packageCT.CW_PackQty = 1;
		packageCT.CW_PackType = "CT";
		packageCT.CW_MarksAndNos = "IND";
		packageCT.CW_HouseBill = declarationBill.CU_BillUniqueCode;
	}

	JobDeclaration declaration;
	BasePackage packageCT;

	class JobComInvoiceLine_ForTest : JobComInvoiceLine
	{
		public JobComInvoiceLine_ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public CommonJobComInvoiceLineValidation_ForTest GetValidationForTest() => new CommonJobComInvoiceLineValidation_ForTest(this);
	}

	class CommonJobComInvoiceLineValidation_ForTest : CommonJobComInvoiceLineValidation
	{
		public CommonJobComInvoiceLineValidation_ForTest(JobComInvoiceLine parent) : base(parent)
		{
		}

		public bool HasValidPackagePivots_Exposed => base.HasValidPackagePivots;
	}
}
