using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitConsignmentItem))]
	sealed class CusExitConsignmentItemTest : EnterpriseBusinessObjectTestCase
	{
		public static (CusExitHeader header, CusExitConsignment consignment, CusExitConsignmentItem item) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			(var header, var consignment) = CusExitConsignmentTest.GetNewBusinessObject(factory);
			var item = consignment.CusExitConsignmentItems.AddNew();
			item.CCI_LineNumber = 1;
			return (header, consignment, item);
		}

		public void TestCanDelete()
		{
			(var exitHeader, var exitConsignment) = CusExitConsignmentTest.GetNewBusinessObject(Factory);
			var item = exitConsignment.CusExitConsignmentItems.AddNew();
			item.CCI_LineNumber = 1;
			AssertEquals("Can delete item not attached to exit report", true, item.CanDelete);
			var exitReport = exitHeader.CusExitReports.AddNew();
			var reportItem = exitReport.CusExitReportItems.AddNew();
			reportItem.ERI_CCI_ConsignmentItem = item.PK;
			reportItem.ERI_CER_Report = exitReport.PK;
			AssertEquals("Can not delete item attached to exit report", false, item.CanDelete);
		}

		public void TestValidation()
		{
			AssertType<CusExitConsignmentItemValidation>(item.Validation);
		}

		public void TestCusExitReportItems()
		{
			AssertType<ExitControlBase.Business.CusExitReportItemCollection<CusExitReportItem>>(item.CusExitReportItems);
		}

		public void TestCusExitConsignmentPivots()
		{
			AssertType<ExitControlBase.Business.CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>(item.CusExitConsignmentPivots);
		}

		public void TestCusExitConsignmentPackagePivots()
		{
			AssertType<ExitControlBase.Business.CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>(item.CusExitConsignmentPackagePivots);
		}

		public void TestCusExitConsignmentContainerPivots()
		{
			AssertType<ExitControlBase.Business.CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>(item.CusExitConsignmentContainerPivots);
		}

		public void TestCusAuthorizationUsages()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var authorizationUsage1 = item.CusAuthorizationUsages.AddNew();
			authorizationUsage1.AGC_Code = "abc";
			authorizationUsage1.AGC_Number = "123";
			authorizationUsage1.AGC_OH_Owner = orgHeader.PK;
			var authorizationUsage2 = item.CusAuthorizationUsages.AddNew();
			authorizationUsage2.AGC_Code = "def";
			authorizationUsage2.AGC_Number = "456";
			authorizationUsage2.AGC_OH_Owner = orgHeader.PK;
			Factory.Save();

			var authorizationUsages = NewFactory().Load<CusExitConsignmentItem>(item.PK).CusAuthorizationUsages.Select(x => x.PK).ToArray();
			AssertEquals("authorizationUsages.Length", 2, authorizationUsages.Length);
			AssertContainsExactElementsInAnyOrder(new[] { authorizationUsage1.PK, authorizationUsage2.PK }, authorizationUsages);

			item.Delete();
			AssertEquals("authorizationUsage1.IsDeleted", true, authorizationUsage1.IsDeleted);
			AssertEquals("authorizationUsage2.IsDeleted", true, authorizationUsage2.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).item;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => item;

		protected override BusinessObject GetNewBusinessObject() => item;

		protected override void SetUp()
		{
			base.SetUp();
			(_, _, item) = GetNewBusinessObject(Factory);
		}
		CusExitConsignmentItem item;
	}
}
