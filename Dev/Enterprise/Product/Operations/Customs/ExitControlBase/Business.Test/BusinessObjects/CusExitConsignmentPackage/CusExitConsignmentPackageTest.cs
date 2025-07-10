using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitConsignmentPackage))]
sealed class CusExitConsignmentPackageTest : EnterpriseBusinessObjectTestCase
{
	public void TestSingleBusinessObjectAroundARow()
	{
		AssertEquals(1, typeof(CusExitConsignmentPackage).GetCustomAttributes(typeof(SingleObjectAroundARow), false).Length);
	}

	public void TestHeader()
	{
		(var consignmentPackage, _, _, _, var header) = GetNewBusinessObject(Factory);
		AssertEquals(header.PK, consignmentPackage.Header.PK);
	}

	public void TestCusExitReportItems()
	{
		(var consignmentPackage, _, _, _,_) = GetNewBusinessObject(Factory);
		var reportItem1 = Factory.New<CusExitReportItem>();
		reportItem1.ERI_CXP_Package = consignmentPackage.PK;
		var reportItem2 = Factory.New<CusExitReportItem>();
		reportItem2.ERI_CXP_Package = consignmentPackage.PK;
		var reportItem3 = Factory.New<CusExitReportItem>();
		reportItem3.ERI_CXP_Package = Factory.New<CusExitConsignmentPackage>().PK;
		AssertContainsExactElementsInAnyOrder(new[] { reportItem1.PK, reportItem2.PK }, consignmentPackage.CusExitReportItems.Select(x => x.PK));
	}

	public void TestValidation()
	{
		(var consignmentPackage, _, _, _, _) = GetNewBusinessObject(Factory);
		AssertType<CusExitConsignmentPackageValidation>(consignmentPackage.Validation);
	}

	public void TestLookups()
	{
		(var consignmentPackage, _, _, _, _) = GetNewBusinessObject(Factory);
		AssertType<CusExitConsignmentPackageLookups>(consignmentPackage.Lookups);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).package;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

	public static (CusExitConsignmentPackage package, CusExitConsignmentPivot pivot, CusExitConsignmentItem consignmentItem, CusExitConsignment consignment, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
	{
		(var pivot, var consignmentItem, var consignment, var header) = CusExitConsignmentPivotTest.GetNewBusinessObject(factory);
		return (pivot.Package, pivot, consignmentItem, consignment, header);
	}
}
