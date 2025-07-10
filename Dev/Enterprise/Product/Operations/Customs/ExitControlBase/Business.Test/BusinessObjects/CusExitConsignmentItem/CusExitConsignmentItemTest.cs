using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitConsignmentItem))]
sealed class CusExitConsignmentItemTest : EnterpriseBusinessObjectTestCase
{
	public void TestSingleBusinessObjectAroundARow()
	{
		AssertEquals(1, typeof(CusExitConsignmentItem).GetCustomAttributes(typeof(SingleObjectAroundARow), false).Length);
	}

	public void TestConsignment()
	{
		(var consignmentItem, var consignment, _) = GetNewBusinessObject(Factory);
		AssertEquals(consignment.PK, consignmentItem.Consignment.PK);
	}

	public void TestValidation()
	{
		(var consignmentItem, _, _) = GetNewBusinessObject(Factory);
		AssertType<CusExitConsignmentItemValidation>(consignmentItem.Validation);
	}

	public void TestLookups()
	{
		(var consignmentItem, _, _) = GetNewBusinessObject(Factory);
		AssertType<CusExitConsignmentItemLookups>(consignmentItem.Lookups);
	}

	public void TestCusExitReportItems()
	{
		var reportItem1 = Factory.New<CusExitReportItem>();
		reportItem1.ERI_CCI_ConsignmentItem = ConsignmentItem.PK;
		var reportItem2 = Factory.New<CusExitReportItem>();
		reportItem2.ERI_CCI_ConsignmentItem = ConsignmentItem.PK;
		var reportItem3 = Factory.New<CusExitReportItem>();
		reportItem3.ERI_CCI_ConsignmentItem = Factory.New<CusExitConsignmentItem>().PK;
		AssertContainsExactElementsInAnyOrder(new[] { reportItem1.PK, reportItem2.PK }, ConsignmentItem.CusExitReportItems.Select(x => x.PK));
	}

	public void TestCusExitConsignmentPivots()
	{
		var consignmentPivot1 = Factory.New<CusExitConsignmentPivot>();
		consignmentPivot1.CNP_CCI_ConsignmentItem = ConsignmentItem.PK;
		var consignmentPivot2 = Factory.New<CusExitConsignmentPivot>();
		consignmentPivot2.CNP_CCI_ConsignmentItem = ConsignmentItem.PK;
		var consignmentPivot3 = Factory.New<CusExitConsignmentPivot>();
		consignmentPivot3.CNP_CCI_ConsignmentItem = Factory.New<CusExitConsignmentItem>().PK;
		var exitConsignmentPackagePivot = ConsignmentItem.CusExitConsignmentPackagePivots.AddNew();
		var exitConsignmentContainerPivot = ConsignmentItem.CusExitConsignmentContainerPivots.AddNew();
		AssertContainsExactElementsInAnyOrder(new[] { consignmentPivot1.PK, consignmentPivot2.PK, exitConsignmentPackagePivot.PK, exitConsignmentContainerPivot.PK }, consignmentItem.CusExitConsignmentPivots.Select(x => x.PK));
	}

	public void TestCusExitConsignmentPackagePivots()
	{
		ConsignmentItem.CusExitConsignmentPivots.AddNew();
		ConsignmentItem.CusExitConsignmentContainerPivots.AddNew();
		var exitConsignmentPackagePivot = ConsignmentItem.CusExitConsignmentPackagePivots.AddNew();
		var exitConsignmentPackagePivot2 = ConsignmentItem.CusExitConsignmentPackagePivots.AddNew();
		var packagePivots = ConsignmentItem.CusExitConsignmentPackagePivots;

		CombineAssertions(() =>
		{
			AssertType<CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>("Type", packagePivots);
			consignmentItem.IsRegisteredEditableChildObject(packagePivots);
			AssertContainsExactElementsInAnyOrder("Elements", new[] { exitConsignmentPackagePivot.PK, exitConsignmentPackagePivot2.PK }, packagePivots.Select(x => x.PK));
		});
	}

	public void TestCusExitConsignmentContainerPivots()
	{
		ConsignmentItem.CusExitConsignmentPivots.AddNew();
		ConsignmentItem.CusExitConsignmentPackagePivots.AddNew();
		var exitConsignmentContainerPivot = ConsignmentItem.CusExitConsignmentContainerPivots.AddNew();
		var exitConsignmentContainerPivot2 = ConsignmentItem.CusExitConsignmentContainerPivots.AddNew();
		var containerPivots = ConsignmentItem.CusExitConsignmentContainerPivots;

		CombineAssertions(() =>
		{
			AssertType<CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>("Type", containerPivots);
			consignmentItem.IsRegisteredEditableChildObject(containerPivots);
			AssertContainsExactElementsInAnyOrder("Elements", new[] { exitConsignmentContainerPivot.PK, exitConsignmentContainerPivot2.PK }, containerPivots.Select(x => x.PK));
		});
	}

	CusExitConsignmentItem ConsignmentItem => consignmentItem ??= GetNewBusinessObject(Factory).consignmentItem;
	CusExitConsignmentItem consignmentItem;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(Factory).consignmentItem;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => ConsignmentItem;

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).consignmentItem;

	public static (CusExitConsignmentItem consignmentItem, CusExitConsignment consignment, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
	{
		(var consignment, var header) = CusExitConsignmentTest.GetNewBusinessObject(factory);
		var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
		consignmentItem.CCI_LineNumber = 1;
		return (consignmentItem, consignment, header);
	}
}
