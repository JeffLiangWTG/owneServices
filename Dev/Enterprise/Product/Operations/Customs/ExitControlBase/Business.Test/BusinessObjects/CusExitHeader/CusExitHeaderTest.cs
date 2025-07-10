using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitHeader))]
sealed class CusExitHeaderTest : EnterpriseBusinessObjectTestCase
{
	public void TestSingleBusinessObjectAroundARow()
	{
		AssertEquals(1, typeof(CusExitHeader).GetCustomAttributes(typeof(SingleObjectAroundARow), false).Length);
	}

	public void TestValidation()
	{
		AssertType<CusExitHeaderValidation>(ExitHeader.Validation);
	}

	public void TestLookups()
	{
		AssertType<CusExitHeaderLookups>(ExitHeader.Lookups);
	}

	public void TestCusExitReports()
	{
		var report1 = Factory.New<CusExitReport>();
		report1.CER_CXH_Header = ExitHeader.PK;
		var report2 = Factory.New<CusExitReport>();
		report2.CER_CXH_Header = ExitHeader.PK;
		var report3 = Factory.New<CusExitReport>();
		report3.CER_CXH_Header = Factory.New<CusExitHeader>().PK;
		AssertContainsExactElementsInAnyOrder(new[] { report1.PK, report2.PK }, ExitHeader.CusExitReports.Select(x => x.PK));
	}

	public void TestCusExitContainers()
	{
		var container1 = Factory.New<CusExitContainer>();
		container1.CXN_CXH_Header = ExitHeader.PK;
		var container2 = Factory.New<CusExitContainer>();
		container2.CXN_CXH_Header = ExitHeader.PK;
		var container3 = Factory.New<CusExitContainer>();
		container3.CXN_CXH_Header = Factory.New<CusExitHeader>().PK;
		AssertContainsExactElementsInAnyOrder(new[] { container1.PK, container2.PK }, ExitHeader.CusExitContainers.Select(x => x.PK));
	}

	public void TestCusExitConsignments()
	{
		var consignment1 = Factory.New<CusExitConsignment>();
		consignment1.CXC_CXH_Header = ExitHeader.PK;
		var consignment2 = Factory.New<CusExitConsignment>();
		consignment2.CXC_CXH_Header = ExitHeader.PK;
		var consignment3 = Factory.New<CusExitConsignment>();
		consignment3.CXC_CXH_Header = Factory.New<CusExitHeader>().PK;
		AssertContainsExactElementsInAnyOrder(new[] { consignment1.PK, consignment2.PK }, ExitHeader.CusExitConsignments.Select(x => x.PK));
	}

	public void TestCusExitConsignmentPackages()
	{
		var package1 = Factory.New<CusExitConsignmentPackage>();
		package1.CXP_CXH_Header = ExitHeader.PK;
		var package2 = Factory.New<CusExitConsignmentPackage>();
		package2.CXP_CXH_Header = ExitHeader.PK;
		var package3 = Factory.New<CusExitConsignmentPackage>();
		package3.CXP_CXH_Header = Factory.New<CusExitHeader>().PK;
		AssertContainsExactElementsInAnyOrder(new[] { package1.PK, package2.PK }, ExitHeader.CusExitConsignmentPackages.Select(x => x.PK));
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

	public static CusExitHeader GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<CusExitHeader>();
		header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
		header.CXH_ApplicationCode = Common.CusExitHeaderApplicationCodeList.Codes.ExitControl;
		return header;
	}

	CusExitHeader ExitHeader => exitHeader ??= Factory.New<CusExitHeader>();
	CusExitHeader exitHeader;
}
