using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitConsignmentPivot))]
	sealed class CusExitConsignmentPivotTest : EnterpriseBusinessObjectTestCase
	{
		public static (CusExitHeader header, CusExitConsignment consignment, CusExitConsignmentItem item, CusExitConsignmentPivot pivot) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var (header, consignment, item) = CusExitConsignmentItemTest.GetNewBusinessObject(factory);
			var pivot = (CusExitConsignmentPivot)item.CusExitConsignmentPackagePivots.AddNew();
			pivot.CNP_CXN_Container = header.CusExitContainers.AddNew().PK;
			return (header, consignment, item, pivot);
		}

		public void TestPackage()
		{
			AssertType<CusExitConsignmentPackage>(pivot.Package);
		}

		public void TestContainer()
		{
			Factory.Save();
			pivot = new BusinessObjectFactory().Load<CusExitConsignmentPivot>(pivot.PK);
			AssertType<CusExitContainer>(pivot.Container);
		}

		public void TestConsignmentItem()
		{
			AssertType<CusExitConsignmentItem>(pivot.ConsignmentItem);
		}

		public void TestSequenceNumber()
		{
			var (header, consignment, item, pivot1) = GetNewBusinessObject(Factory);
			var pivot2 = (CusExitConsignmentPivot)item.CusExitConsignmentPackagePivots.AddNew();
			AssertEquals("Expected SequenceNumber for pivot1", new CargoWise.Types.ZShort(1), pivot1.SequenceNumber);
			AssertEquals("Expected SequenceNumber for pivot2", new CargoWise.Types.ZShort(2), pivot2.SequenceNumber);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).pivot;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => pivot;

		protected override BusinessObject GetNewBusinessObject() => pivot;

		protected override void SetUp()
		{
			base.SetUp();
			pivot = GetNewBusinessObject(Factory).pivot;
		}
		CusExitConsignmentPivot pivot;
	}
}
