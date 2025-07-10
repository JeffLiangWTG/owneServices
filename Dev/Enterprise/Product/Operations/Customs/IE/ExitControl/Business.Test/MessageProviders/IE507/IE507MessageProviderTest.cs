using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.ExitControl.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.AES.Testing
{
	[TestDate(2022, 5, 16)]
	class IE507MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<IE507MessageProvider>
	{
		public void TestExportOperation()
		{
			AssertSame("ExportOperation", Provider, Provider.ExportOperation);
		}

		public void TestCustomsOfficeOfExitActual()
		{
			report.CER_OfficeOfExit = "OFF";
			AssertEquals("CustomsOfficeOfExitActual", "OFF", Provider.CustomsOfficeOfExitActual);
		}

		public void TestAuthorisations()
		{
			var usage1 = consignment.CusAuthorizationUsages.AddNew();
			usage1.AGC_Number = "AC1";
			var usage2 = consignment.CusAuthorizationUsages.AddNew();
			usage2.AGC_Number = "AC2";
			var authorisations = Provider.Authorisations.ToArray();
			AssertEquals("Authorisations", 2, authorisations.Length);
			AssertEquals("authorisations[0].UCR", "AC1", authorisations[0].UCR);
			AssertEquals("authorisations[1].UCR", "AC2", authorisations[1].UCR);
		}

		public void TestGoodsShipment()
		{
			AssertSame("GoodsShipment", Provider, Provider.GoodsShipment);
		}

		public void TestArrivalNotificationDateAndTime()
		{
			report.CER_DateTime = ZDateTimeOffset.Today;
			AssertEquals("ArrivalNotificationDateAndTime", ZDateTimeOffset.Today.ToDateTime(), Provider.ArrivalNotificationDateAndTime);

			report.CER_DateTime = ZDateTimeOffset.Empty;
			AssertEquals("ArrivalNotificationDateAndTime", DateTime.MinValue, Provider.ArrivalNotificationDateAndTime);
		}

		public void TestArrivalNotificationPlace()
		{
			report.CER_Location = "IEL";
			AssertEquals("ArrivalNotificationPlace", "IEL", Provider.ArrivalNotificationPlace);
		}

		public void TestDiscrepanciesExist()
		{
			report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.NoDiscrepancies;

			var provider = new IE507MessageProvider(report);
			CombineAssertions(() =>
			{
				AssertEquals("DiscrepanciesExist: false", false, provider.DiscrepanciesExist);

				report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
				provider = new IE507MessageProvider(report);
				AssertEquals("DiscrepanciesExist: true", true, provider.DiscrepanciesExist);
			});
		}

		public void TestConsignment()
		{
			AssertType<IE507ConsignmentProvider>("Consignment", Provider.Consignment);
		}

		public void TestGoodsItems()
		{
			report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
			reportItem.Delete();
			var provider = new IE507MessageProvider(report);
			AssertEquals("Goods Items should not yet be populated", 0, provider.GoodsItems.Count);

			reportItem = report.CusExitReportItems.AddNew();
			var pivot1 = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var package1 = pivot1.Package;
			reportItem.ERI_CXP_Package = package1.PK;
			reportItem.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			provider = new IE507MessageProvider(report);
			AssertEquals("Goods Items should be populated", 1, provider.GoodsItems.Count);

			var pivot2 = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var package2 = pivot2.Package;
			var reportItem2 = report.CusExitReportItems.AddNew();
			reportItem2.ERI_CCI_ConsignmentItem = reportItem.ERI_CCI_ConsignmentItem;
			reportItem2.ERI_CXP_Package = package2.PK;
			provider = new IE507MessageProvider(report);
			AssertEquals("Goods Items count should still be 1", 1, provider.GoodsItems.Count);

			var reportItem3 = report.CusExitReportItems.AddNew();
			var consignmentItem2 = consignment.CusExitConsignmentItems.AddNew();
			var pivot3 = consignmentItem2.CusExitConsignmentPackagePivots.AddNew();
			var package3 = pivot3.Package;
			reportItem3.ERI_CCI_ConsignmentItem = consignmentItem2.PK;
			reportItem3.ERI_CXP_Package = package3.PK;
			provider = new IE507MessageProvider(report);
			AssertEquals("Goods Items count should be 2", 2, provider.GoodsItems.Count);

			var reportItem4 = report.CusExitReportItems.AddNew();
			reportItem4.ERI_CCI_ConsignmentItem = reportItem3.ERI_CCI_ConsignmentItem;
			provider = new IE507MessageProvider(report);
			AssertEquals("Goods Items count should still be 2", 2, provider.GoodsItems.Count);
		}

		public void TestGoodsItemsPackagesQuantity_Bulk()
		{
			report.CER_OfficeOfExit = "EXT001";
			var pivot1 = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var package1 = pivot1.Package;
			reportItem.ERI_CXP_Package = package1.PK;
			package1.CXP_PackageType = "BX";
			reportItem.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			reportItem.ERI_Quantity = 0;
			var provider = new IE507MessageProvider(report);
			var package = provider.GoodsItems.FirstOrDefault().Packages.FirstOrDefault();
			AssertEquals("Quantity - Not Bulk", 0, package.PackageQuantity);

			package1.CXP_PackageType = "VG";
			provider = new IE507MessageProvider(report);
			package = provider.GoodsItems.FirstOrDefault().Packages.FirstOrDefault();
			AssertEquals("Quantity - Bulk", 0, package.PackageQuantity);
		}

		public void TestMRN()
		{
			consignment.CXC_MovementReference = "MRN123";
			var provider = new IE507MessageProvider(report);
			AssertEquals("MRN", "MRN123", provider.MRN);
		}

		public void TestStoringFlag()
		{
			AssertEquals("StoringFlag", "0", Provider.StoringFlag);
		}

		protected override IE507MessageProvider GetProvider() => new IE507MessageProvider(report);

		protected override void SetUp()
		{
			base.SetUp();
			Factory.SetupBulkCusCode();
			header = Factory.New<CusExitHeader>();
			report = header.CusExitReports.AddNew();
			consignment = header.CusExitConsignments.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem.CCI_LineNumber = 10;

			reportItem = report.CusExitReportItems.AddNew();
			reportItem.ERI_CCI_ConsignmentItem = consignmentItem.PK;
		}
		CusExitHeader header;
		CusExitConsignment consignment;
		CusExitConsignmentItem consignmentItem;
		CusExitReport report;
		CusExitReportItem reportItem;
	}
}
