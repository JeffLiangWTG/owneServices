using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitContainer))]
sealed class CusExitContainerTest : EnterpriseBusinessObjectTestCase
{
	public void TestSingleBusinessObjectAroundARow()
	{
		AssertEquals(1, typeof(CusExitContainer).GetCustomAttributes(typeof(SingleObjectAroundARow), false).Length);
	}

	public void TestHeader()
	{
		(var exitContainer, var exitHeader) = GetNewBusinessObject(Factory);
		AssertEquals(exitHeader.PK, exitContainer.Header.PK);
	}

	public void TestValidation()
	{
		(var exitContainer, _) = GetNewBusinessObject(Factory);
		AssertType<CusExitContainerValidation>(exitContainer.Validation);
	}

	public void TestLookups()
	{
		(var exitContainer, _) = GetNewBusinessObject(Factory);
		AssertType<CusExitContainerLookups>(exitContainer.Lookups);
	}

	public void TestCusExitConsignmentPivots()
	{
		var item = ExitContainer.Header.CusExitConsignments.AddNew().CusExitConsignmentItems.AddNew();
		var consignmentPivot1 = item.CusExitConsignmentPivots.AddNew();
		consignmentPivot1.CNP_CXN_Container = ExitContainer.PK;
		var consignmentPivot2 = item.CusExitConsignmentPivots.AddNew();
		consignmentPivot2.CNP_CXN_Container = ExitContainer.PK;
		var consignmentPivot3 = item.CusExitConsignmentPivots.AddNew();
		consignmentPivot3.CNP_CXN_Container = Factory.New<CusExitContainer>().PK;
		AssertContainsExactElementsInAnyOrder(new[] { consignmentPivot1.PK, consignmentPivot2.PK }, ExitContainer.CusExitConsignmentPivots.Select(x => x.PK));
	}

	CusExitContainer ExitContainer => exitContainer ??= GetNewBusinessObject(Factory).container;
	CusExitContainer exitContainer;

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).container;
	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

	public static (CusExitContainer container, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<CusExitHeader>();
		header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
		header.CXH_ApplicationCode = Common.CusExitHeaderApplicationCodeList.Codes.ExitControl;
		var container = header.CusExitContainers.AddNew();
		return (container, header);
	}
}
