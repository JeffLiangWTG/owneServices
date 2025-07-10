using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitConsignmentPivot))]
	class CusExitConsignmentPivotTest : EnterpriseBusinessObjectTestCase
	{
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

		public void TestConsignmentItem()
		{
			(var consignmentPivot, _, _, _) = GetNewBusinessObject(Factory);
			AssertType<CusExitConsignmentItem>(consignmentPivot.ConsignmentItem);
		}

		public void TestContainer()
		{
			(var consignmentPivot, _, _, _) = GetNewBusinessObject(Factory);
			var container = Factory.New<CusExitContainer>();
			consignmentPivot.CNP_CXN_Container = container.PK;
			AssertEquals(container.PK, consignmentPivot.Container.PK);
			AssertType<CusExitContainer>(consignmentPivot.Container);
		}

		public void TestPackage()
		{
			(var consignmentPivot, _, _, _) = GetNewBusinessObject(Factory);
			AssertType<CusExitConsignmentPackage>(consignmentPivot.Package);
		}

		public void TestCNP_CXN_Container_Caption()
		{
			(var consignmentPivot, _, _, _) = GetNewBusinessObject(Factory);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(consignmentPivot.CNP_CXN_ContainerInfo, (string)null, "Container/Equipment");
		}

		public void TestSequenceNumberUcc6() => CombineAssertions(() =>
		{
			var header = Factory.GetUcc6ExitHeader();
			var consignment = header.CusExitConsignments.AddNew();
			var item = consignment.CusExitConsignmentItems.AddNew();
			var pivot1 = item.CusExitConsignmentPackagePivots.AddNew();
			var pivot2 = item.CusExitConsignmentPackagePivots.AddNew();
			AssertEquals("Expected SequenceNumber for pivot1", (ZShort)1, pivot1.SequenceNumber);
			AssertEquals("Expected SequenceNumber for pivot2", (ZShort)2, pivot2.SequenceNumber);
		});

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
}
