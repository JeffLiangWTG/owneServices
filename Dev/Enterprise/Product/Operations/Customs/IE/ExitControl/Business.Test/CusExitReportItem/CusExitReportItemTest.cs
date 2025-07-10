using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitReportItem))]
	sealed class CusExitReportItemTest : EnterpriseBusinessObjectTestCase
	{
		public static (CusExitReportItem reportItem, CusExitReport report, CusExitConsignmentItem consignmentItem, CusExitConsignment consignment, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			(var report, var consignment, var header) = CusExitReportTest.GetNewBusinessObject(factory);
			var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem.CCI_LineNumber = 1;
			var reportItem = report.CusExitReportItems.AddNew();
			reportItem.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			return (reportItem, report, consignmentItem, consignment, header);
		}

		public void TestPackage()
		{
			var package = consignment.CusExitConsignmentItems.AddNew().CusExitConsignmentPackagePivots.AddNew().Package;
			reportItem.ERI_CXP_Package = package.PK;
			AssertType<CusExitConsignmentPackage>("Should return IE CusExitConsignmentPackage for package.", reportItem.Package);
		}

		public void TestAdditionalInfos()
		{
			AssertType<EU.ExitControl.Business.AdditionalInfoCollection<AdditionalInfo>>(GetNewBusinessObject(Factory).reportItem.AdditionalInfos);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).reportItem;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => reportItem;

		protected override BusinessObject GetNewBusinessObject() => reportItem;

		protected override void SetUp()
		{
			base.SetUp();
			(reportItem, _, _, consignment, _) = GetNewBusinessObject(Factory);
		}
		CusExitConsignment consignment;
		CusExitReportItem reportItem;
	}
}
