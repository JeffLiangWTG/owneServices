using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(Package))]
sealed class PackageTest : EnterpriseBusinessObjectTestCase
{
	public void TestValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		var package = declaration.Packages.AddNew();
		AssertType<PackageValidation>(package.Validation);
	}

	public void TestCheckCW_MarksAndNos_MaximumLength()
	{
		var package = Factory.New<Package>();
		AssertEquals("Max Length of MarksAndNos", 512, package.CW_MarksAndNosInfo.MaxLength);
	}

	public void TestInvoiceLinePivotCollection()
	{
		var package = Factory.New<Package>();
		AssertType<PackagePivotsCollection>("Type", package.InvoiceLinePivotCollection);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MasterBill = "MB1";
		declaration.JE_TotalNoOfPacks = 123;
		declaration.JE_TotalNoOfPacksPackType = "AE";
		var pack = declaration.Packages[0];
		return pack;
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
}
