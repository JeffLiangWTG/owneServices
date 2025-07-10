using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(InvoiceLinePackagePivot))]
internal class InvoiceLinePackagePivotTest : EnterpriseBusinessObjectTestCase
{
	public void TestPackage()
	{
		var packagePivot = GetNewBusinessObject() as InvoiceLinePackagePivot;
		AssertType<Package>(packagePivot.Package);
	}

	public void TestGetNewValidation()
	{
		var validation = Factory.New<InvoiceLinePackagePivotForTest>().GetNewValidation_Exposed();
		AssertType<InvoiceLinePackagePivotValidation>(validation);
	}

	public void TestCHC_NumberOfPacksHumanReadableName()
	{
		var packagePivot = GetNewBusinessObject() as InvoiceLinePackagePivot;
		AssertEquals("HumanReadableName", "Invoice Line Pack Quantity", packagePivot.CHC_NumberOfPacksInfo.HumanReadableName);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		=> GetNewBusinessObject();

	protected override BusinessObject GetBusinessObjectForFetchForLoad()
	{
		return GetNewBusinessObject();
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var declarationBill = declaration.Bills.AddNew();
		declarationBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
		declarationBill.CU_BillNum = "999";

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		var package = declaration.Packages.AddNew();
		package.CW_PackQty = 1;
		package.CW_HouseBill = declarationBill.CU_BillUniqueCode;

		var packagePivot = invoiceLine.PackagesPivot.AddNew();
		packagePivot.CHC_JE = declaration.PK;
		packagePivot.CHC_CW = package.PK;
		return packagePivot;
	}
}

class InvoiceLinePackagePivotForTest : InvoiceLinePackagePivot
{
	public InvoiceLinePackagePivotForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public CusHouseContPackInvoiceLinePivotValidation GetNewValidation_Exposed() => base.GetNewValidation();
}
