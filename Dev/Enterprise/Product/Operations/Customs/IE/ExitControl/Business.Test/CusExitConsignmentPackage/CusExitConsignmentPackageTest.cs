using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitConsignmentPackage))]
	sealed class CusExitConsignmentPackageTest : EnterpriseBusinessObjectTestCase
	{
		public static CusExitConsignmentPackage GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = CusExitHeaderTest.GetNewBusinessObject(factory);
			var consignment = header.CusExitConsignments.AddNew();
			var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem.CCI_LineNumber = 1;
			var pivot = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var result = (CusExitConsignmentPackage)pivot.Package;

			return result;
		}

		public void TestCusExitReportItems()
		{
			AssertType<ExitControlBase.Business.CusExitReportItemCollection<CusExitReportItem>>(cusExitConsignmentPackage.CusExitReportItems);
		}

		public void TestCusExitConsignmentPivot()
		{
			AssertType<CusExitConsignmentPivot>(cusExitConsignmentPackage.ConsignmentPivot);
		}

		public void TestValidation()
		{
			AssertType<CusExitConsignmentPackageValidation>(cusExitConsignmentPackage.Validation);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Package will be deleted with Pivot.", true);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => cusExitConsignmentPackage;

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		protected override void SetUp()
		{
			base.SetUp();
			cusExitConsignmentPackage = GetNewBusinessObject(Factory);
		}

		CusExitConsignmentPackage cusExitConsignmentPackage;
	}
}
