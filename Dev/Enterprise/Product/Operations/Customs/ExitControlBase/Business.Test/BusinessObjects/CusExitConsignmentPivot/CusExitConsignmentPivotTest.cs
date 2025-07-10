using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitConsignmentPivot))]
sealed class CusExitConsignmentPivotTest : EnterpriseBusinessObjectTestCase
{
	public void TestSingleBusinessObjectAroundARow()
	{
		AssertEquals(1, typeof(CusExitConsignmentPivot).GetCustomAttributes(typeof(SingleObjectAroundARow), false).Length);
	}

	public void TestConsignmentItem()
	{
		(var consignmentPivot, var consignmentItem, _, _) = GetNewBusinessObject(Factory);
		AssertEquals(consignmentItem.PK, consignmentPivot.ConsignmentItem.PK);
	}

	public void TestContainer()
	{
		(var consignmentPivot, _, _, _) = GetNewBusinessObject(Factory);
		var container = Factory.New<CusExitContainer>();
		consignmentPivot.CNP_CXN_Container = container.PK;
		AssertEquals(container.PK, consignmentPivot.Container.PK);
	}

	public void TestPackage()
	{
		(var consignmentPivot, _, _, _) = GetNewBusinessObject(Factory);
		AssertSame("Should return the correct Package for CusExitConsignmentPackage.Package.",
			consignmentPivot.ConsignmentItem.Consignment.Header.CusExitConsignmentPackages.FirstOrDefault(),
			consignmentPivot.Package
		);
	}

	public void TestValidation()
	{
		(var consignmentPivot, _, _, _) = GetNewBusinessObject(Factory);
		AssertType<CusExitConsignmentPivotValidation>(consignmentPivot.Validation);
	}

	public void TestLookups()
	{
		(var consignmentPivot, _, _, _) = GetNewBusinessObject(Factory);
		AssertType<CusExitConsignmentPivotLookups>(consignmentPivot.Lookups);
	}

	public void TestISequenceNumberLine()
	{
		CombineAssertions(() =>
		{
			(var consignmentPivot, var consignmentItem, _, _) = GetNewBusinessObject(Factory);
			var sequenceLine = (IShortSequenceNumberLine)consignmentPivot;
			AssertEquals("FKToHeader", consignmentItem.PK, sequenceLine.FKToHeader);
			AssertEquals("SequenceNumber", (ZShort)1, sequenceLine.SequenceNumber);
			AssertEquals("Package.CXP_Sequence", (ZShort)1, consignmentPivot.Package.CXP_Sequence);
		});
	}

	public void TestDelete()
	{
		CombineAssertions(() =>
		{
			(var consignmentPivot, var consignmentItem, _, _) = GetNewBusinessObject(Factory);
			var package = consignmentPivot.Package;
			consignmentPivot.Delete();
			AssertEquals("Package is deleted", true, package.IsDeleted);

			var consignmentContainerPivot = consignmentItem.CusExitConsignmentContainerPivots.AddNew();
			var container = consignmentContainerPivot.Container;
			consignmentContainerPivot.Delete();
			AssertEquals("Container should not be deleted", false, container.IsDeleted);
		});
	}

	public void TestSequenceNumber()
	{
		CombineAssertions(() =>
		{
			(var consignmentPivot, var consignmentItem, _, _) = GetNewBusinessObject(Factory);
			var consignmentPackagePivot2 = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			AssertEquals("CXP_Sequence of consignmentPackagePivot2.Package is 2", (ZShort)2, consignmentPackagePivot2.Package.CXP_Sequence);

			consignmentPivot.Delete();
			AssertEquals("consignmentPivot is deleted, CXP_Sequence of consignmentPackagePivot2.Package is 1", (ZShort)1, consignmentPackagePivot2.Package.CXP_Sequence);

			var consignmentContainerPivot = consignmentItem.CusExitConsignmentContainerPivots.AddNew();
			AssertNoExceptionThrown("No exception when deleting consignmentContainerPivot", () => consignmentContainerPivot.Delete());
		});
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).pivot;
	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

	public static (CusExitConsignmentPivot pivot, CusExitConsignmentItem consignmentItem, CusExitConsignment consignment, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
	{
		(var consignmentItem, var consignment, var header) = CusExitConsignmentItemTest.GetNewBusinessObject(factory);
		var pivot = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
		return (pivot, consignmentItem, consignment, header);
	}
}
