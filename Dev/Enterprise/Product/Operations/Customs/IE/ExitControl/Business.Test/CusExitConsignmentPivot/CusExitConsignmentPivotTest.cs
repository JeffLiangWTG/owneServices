using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitConsignmentPivot))]
	sealed class CusExitConsignmentPivotTest : EnterpriseBusinessObjectTestCase
	{
		public static (CusExitHeader header, CusExitConsignment consignment, CusExitConsignmentItem item, CusExitConsignmentPivot pivot) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			(var header, var consignment, var item) = CusExitConsignmentItemTest.GetNewBusinessObject(factory);
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

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).pivot;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => pivot;

		protected override BusinessObject GetNewBusinessObject() => pivot;

		protected override void SetUp()
		{
			base.SetUp();
			(_, _, _, pivot) = GetNewBusinessObject(Factory);
		}
		CusExitConsignmentPivot pivot;
	}
}
