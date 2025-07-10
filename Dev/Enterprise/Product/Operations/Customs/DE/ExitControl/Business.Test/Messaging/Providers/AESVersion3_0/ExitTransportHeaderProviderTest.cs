using System;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	sealed class ExitTransportHeaderProviderTest : Customs.Business.Testing.DataProviderTestCase<ExitTransportHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ExitTransportHeaderProvider(null));
		}

		public void TestFinalization()
		{
			CombineAssertions(() =>
			{
				var provider = GetProvider();
				report.CER_IsFinalized = true;
				AssertEquals("CER_IsFinalized = true", "1", provider.Finalization);
				report.CER_IsFinalized = false;
				AssertEquals("CER_IsFinalized = false", "0", provider.Finalization);

				consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._353;
				report.CER_Calc_Discrepancies = false;
				CreateAllSelectedReportItems();
				provider = GetProvider();
				AssertEquals("InformationType = 'UV'", A0132ReportInformationType.Codes.UV, provider.InformationType);
				AssertEquals("Finalization when InformationType = 'UV'", "1", provider.Finalization);

				report.CusExitReportItems.DeleteAll();
				CreateReportItemsWithUnselectedConsignmentItem();
				provider = GetProvider();
				AssertEquals("InformationType = 'UW'", A0132ReportInformationType.Codes.UW, provider.InformationType);
				AssertEquals("Finalization when InformationType = 'UW'", "1", provider.Finalization);

				report.CusExitReportItems.DeleteAll();
				CreateReportItemsWithUnselectedPackage();
				provider = GetProvider();
				AssertEquals("InformationType = 'UP'", A0132ReportInformationType.Codes.UP, provider.InformationType);
				AssertEquals("Finalization when InformationType = 'UP'", "1", provider.Finalization);
			});
		}

		public void TestInformationType_FP()
		{
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
			report.CER_Calc_Discrepancies = true;
			CreateReportItemsWithUnselectedPackage();

			AssertEquals(A0132ReportInformationType.Codes.FP, GetProvider().InformationType);
		}

		public void TestInformationType_FP_CXC_StatusIsEmpty()
		{
			consignment.CXC_Status = ZString.Empty;
			report.CER_Calc_Discrepancies = true;
			CreateReportItemsWithUnselectedPackage();

			AssertEquals(A0132ReportInformationType.Codes.FP, GetProvider().InformationType);
		}

		public void TestInformationType_FV()
		{
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
			report.CER_Calc_Discrepancies = true;
			CreateAllSelectedReportItems();

			AssertEquals(A0132ReportInformationType.Codes.FV, GetProvider().InformationType);
		}

		public void TestInformationType_FV_CXC_StatusIsEmpty()
		{
			report.CER_Calc_Discrepancies = true;
			CreateAllSelectedReportItems();

			AssertEquals(A0132ReportInformationType.Codes.FV, GetProvider().InformationType);
		}

		public void TestInformationType_FW()
		{
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
			report.CER_Calc_Discrepancies = true;
			CreateReportItemsWithUnselectedConsignmentItem();

			AssertEquals(A0132ReportInformationType.Codes.FW, GetProvider().InformationType);
		}
		public void TestInformationType_FW_CXC_StatusIsEmpty()
		{
			report.CER_Calc_Discrepancies = true;
			CreateReportItemsWithUnselectedConsignmentItem();

			AssertEquals(A0132ReportInformationType.Codes.FW, GetProvider().InformationType);
		}

		public void TestInformationType_LP_OneOrMorePackagesNotSelected()
		{
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
			report.CER_Calc_Discrepancies = false;
			CreateReportItemsWithUnselectedPackage();

			AssertEquals(A0132ReportInformationType.Codes.LP, GetProvider().InformationType);
		}

		public void TestInformationType_LP_OneOrMorePackagesNotEqualInQuantity()
		{
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
			report.CER_Calc_Discrepancies = false;

			var reportItem = CreateAllSelectedReportItems().ReportItem1;
			reportItem.ERI_Quantity = 10;

			package1.CXP_Quantity = 8;

			AssertEquals(A0132ReportInformationType.Codes.LP, GetProvider().InformationType);
		}

		public void TestInformationType_LP_OneOrMorePackagesNotSelected_CXC_StatusIsEmpty()
		{
			report.CER_Calc_Discrepancies = false;
			CreateReportItemsWithUnselectedPackage();

			AssertEquals(A0132ReportInformationType.Codes.LP, GetProvider().InformationType);
		}

		public void TestInformationType_LP_OneOrMorePackagesNotEqualInQuantity_CXC_StatusIsEmpty()
		{
			report.CER_Calc_Discrepancies = false;

			var reportItem = CreateAllSelectedReportItems().ReportItem1;
			reportItem.ERI_Quantity = 10;

			package1.CXP_Quantity = 8;

			AssertEquals(A0132ReportInformationType.Codes.LP, GetProvider().InformationType);
		}

		public void TestInformationType_LV()
		{
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
			report.CER_Calc_Discrepancies = false;
			CreateAllSelectedReportItems();

			AssertEquals(A0132ReportInformationType.Codes.LV, GetProvider().InformationType);
		}

		public void TestInformationType_LV_CXC_StatusIsEmpty()
		{
			report.CER_Calc_Discrepancies = false;
			CreateAllSelectedReportItems();

			AssertEquals(A0132ReportInformationType.Codes.LV, GetProvider().InformationType);
		}

		public void TestInformationType_LW_OneOrMoreConsignmentItemsNotSelected()
		{
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
			report.CER_Calc_Discrepancies = false;
			CreateReportItemsWithUnselectedConsignmentItem();

			AssertEquals(A0132ReportInformationType.Codes.LW, GetProvider().InformationType);
		}

		public void TestInformationType_LW_OneOrMoreConsignmentItemsNotEqualInWeight_GrossMass()
		{
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
			report.CER_Calc_Discrepancies = false;

			var reportItem = CreateAllSelectedReportItems().ReportItem1;
			reportItem.ERI_GrossMass = 3.21M;

			item1.CCI_GrossMass = 1.23M;

			AssertEquals(A0132ReportInformationType.Codes.LW, GetProvider().InformationType);
		}

		public void TestInformationType_LW_OneOrMoreConsignmentItemsNotEqualInWeight_NetMass()
		{
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
			report.CER_Calc_Discrepancies = false;

			var reportItem = CreateAllSelectedReportItems().ReportItem1;
			reportItem.ERI_NetMass = 3.21M;

			item1.CCI_NetMass = 1.23M;

			AssertEquals(A0132ReportInformationType.Codes.LW, GetProvider().InformationType);
		}

		public void TestInformationType_LW_OneOrMoreConsignmentItemsNotSelected_CXC_StatusIsEmpty()
		{
			report.CER_Calc_Discrepancies = false;
			CreateReportItemsWithUnselectedConsignmentItem();

			AssertEquals(A0132ReportInformationType.Codes.LW, GetProvider().InformationType);
		}

		public void TestInformationType_LW_OneOrMoreConsignmentItemsNotEqualInWeight_GrossMass_CXC_StatusIsEmpty()
		{
			report.CER_Calc_Discrepancies = false;

			var reportItem = CreateAllSelectedReportItems().ReportItem1;
			reportItem.ERI_GrossMass = 3.21M;

			item1.CCI_GrossMass = 1.23M;

			AssertEquals(A0132ReportInformationType.Codes.LW, GetProvider().InformationType);
		}

		public void TestInformationType_LW_OneOrMoreConsignmentItemsNotEqualInWeight_NetMass_CXC_StatusIsEmpty()
		{
			report.CER_Calc_Discrepancies = false;

			var reportItem = CreateAllSelectedReportItems().ReportItem1;
			reportItem.ERI_NetMass = 3.21M;

			item1.CCI_NetMass = 1.23M;

			AssertEquals(A0132ReportInformationType.Codes.LW, GetProvider().InformationType);
		}

		public void TestInformationType_NV()
		{
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
			report.CER_Calc_Discrepancies = false;

			AssertEquals(A0132ReportInformationType.Codes.NV, GetProvider().InformationType);
		}

		public void TestInformationType_NV_CXC_StatusIsEmpty()
		{
			report.CER_Calc_Discrepancies = false;

			AssertEquals(A0132ReportInformationType.Codes.NV, GetProvider().InformationType);
		}

		public void TestInformationType_UP_OneOrMorePackagesNotSelected()
		{
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._353;
			report.CER_Calc_Discrepancies = false;
			CreateReportItemsWithUnselectedPackage();

			AssertEquals(A0132ReportInformationType.Codes.UP, GetProvider().InformationType);
		}

		public void TestInformationType_UP_OneOrMorePackagesNotEqualInQuantity()
		{
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._353;
			report.CER_Calc_Discrepancies = false;

			var reportItem = CreateAllSelectedReportItems().ReportItem1;
			reportItem.ERI_Quantity = 10;

			package1.CXP_Quantity = 8;

			AssertEquals(A0132ReportInformationType.Codes.UP, GetProvider().InformationType);
		}

		public void TestInformationType_UV()
		{
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._353;
			report.CER_Calc_Discrepancies = false;
			CreateAllSelectedReportItems();

			AssertEquals(A0132ReportInformationType.Codes.UV, GetProvider().InformationType);
		}

		public void TestInformationType_UW_OneOrMoreConsignmentItemsNotSelected()
		{
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._353;
			report.CER_Calc_Discrepancies = false;
			CreateReportItemsWithUnselectedConsignmentItem();

			AssertEquals(A0132ReportInformationType.Codes.UW, GetProvider().InformationType);
		}

		public void TestInformationType_UW_OneOrMoreConsignmentItemsNotEqualInWeight()
		{
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._353;
			report.CER_Calc_Discrepancies = false;

			var reportItem = CreateAllSelectedReportItems().ReportItem1;
			reportItem.ERI_GrossMass = 3.21M;

			item1.CCI_GrossMass = 1.23M;

			AssertEquals(A0132ReportInformationType.Codes.UW, GetProvider().InformationType);
		}

		public void TestInformationType_NV_Default()
		{
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._353;
			report.CER_Calc_Discrepancies = false;

			AssertEquals("No Items Selected, Default value is NV", A0132ReportInformationType.Codes.NV, GetProvider().InformationType);
		}

		protected override ExitTransportHeaderProvider GetProvider() => new ExitTransportHeaderProvider(report);

		CusExitReportItem CreateReportItem(CusExitConsignmentItem item, CusExitConsignmentPackage package)
		{
			var reportItem = report.CusExitReportItems.AddNew();
			reportItem.ERI_CCI_ConsignmentItem = item.PK;
			reportItem.ERI_CXP_Package = package.PK;
			return reportItem;
		}

		void CreateReportItemsWithUnselectedPackage()
		{
			CreateReportItem(item1, package1);
			CreateReportItem(item2, package3);
		}

		(CusExitReportItem ReportItem1, CusExitReportItem, CusExitReportItem) CreateAllSelectedReportItems()
		{
			var reportItem = CreateReportItem(item1, package1);
			var reportItem2 = CreateReportItem(item1, package2);
			var reportItem3 = CreateReportItem(item2, package3);
			return (reportItem, reportItem2, reportItem3);
		}

		void CreateReportItemsWithUnselectedConsignmentItem()
		{
			CreateReportItem(item1, package1);
			CreateReportItem(item1, package2);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<CusExitHeader>();
			consignment = header.CusExitConsignments.AddNew();
			item1 = consignment.CusExitConsignmentItems.AddNew();
			item2 = consignment.CusExitConsignmentItems.AddNew();
			package1 = item1.CusExitConsignmentPackagePivots.AddNew().Package;
			package2 = item1.CusExitConsignmentPackagePivots.AddNew().Package;
			package3 = item2.CusExitConsignmentPackagePivots.AddNew().Package;
			report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
		}
		CusExitConsignment consignment;
		CusExitConsignmentItem item1;
		CusExitConsignmentItem item2;
		CusExitConsignmentPackage package1;
		CusExitConsignmentPackage package2;
		CusExitConsignmentPackage package3;
		CusExitReport report;
	}
}
