using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitConsignment))]
	sealed class CusExitConsignmentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCusExitConsignmentItems()
		{
			AssertType<CusExitConsignmentItemCollection<CusExitConsignmentItem>>(GetNewBusinessObject(Factory).consignment.CusExitConsignmentItems);
		}

		public void TestAddExitReportOnSaving()
		{
			var (consignment, header) = GetNewBusinessObject(Factory);

			CombineAssertions(() =>
			{
				AssertEquals("Before saving there are no reports", false, header.CusExitReports.Any());

				Factory.Save();
				AssertEquals("After saving there is one new report created with the correct data", 1, header.CusExitReports.Count);
				var report = header.CusExitReports[0];
				AssertEquals("Report is associated to the consignment", consignment.PK, report.CER_CXC_Consignment);
				AssertEquals("Report has correct Type", ExitReportTypeList.Codes.Presentation, report.CER_Type);

				report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
				Factory.Save();
				AssertEquals("After saving, if there is already a report associated nothing is done", 1, header.CusExitReports.Count);
				report = header.CusExitReports[0];
				AssertEquals("Report has not changed Type when it already exists associated to the consignment", ExitReportTypeList.Codes.ExitNotification, report.CER_Type);
			});
		}

		public void TestCreateOrUpdateReport_NoItems()
		{
			var (consignment, header) = GetNewBusinessObject(Factory);
			Factory.Save();

			CombineAssertions(() =>
			{
				var report = header.CusExitReports[0];
				AssertEquals("Report created after saving has no items", 0, report.CusExitReportItems.Count);

				consignment.CreateOrUpdateReport();
				AssertEquals("After calling CreateOrUpdateReport no items were added to the report", 0, report.CusExitReportItems.Count);
			});
		}

		public void TestCreateOrUpdateReport_ItemsWithoutChildren()
		{
			var (consignment, header) = GetNewBusinessObject(Factory);

			var consignmentItem1 = CreateConsignmentItem(consignment, 1, ZString.Empty, 50m, 25m);

			var consignmentItem2 = CreateConsignmentItem(consignment, 2, DiscrepanciesStatusCodeList.Codes.Missing, 50m, 25m);

			var consignmentItem3 = CreateConsignmentItem(consignment, 3, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 50m, 25m);

			var consignmentItem4 = CreateConsignmentItem(consignment, 4, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 100m, 45m);

			var consignmentItem5 = CreateConsignmentItem(consignment, 5, ZString.Empty, 120m, 75m);
			consignmentItem5.CCI_UniqueConsignmentReferenceStatus = DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;

			var consignmentItem6 = CreateConsignmentItem(consignment, 6, DiscrepanciesStatusCodeList.Codes.Missing, 110m, 35m);
			consignmentItem6.CCI_UniqueConsignmentReferenceStatus = DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			Factory.Save();

			CombineAssertions(() =>
			{
				var report = header.CusExitReports[0];
				AssertEquals("Report created after saving has no items", 0, report.CusExitReportItems.Count);
				AssertEquals("Report created after saving has no items for binding", 0, report.CusExitReportItemsForBinding.Count);

				consignment.CreateOrUpdateReport();
				AssertEquals("After calling CreateOrUpdateReport there are 2 items added to the report, only the ones with status DIF", 2, report.CusExitReportItems.Count);
				AssertEquals("After calling CreateOrUpdateReport there are 2 items for binding added to the report, only the ones with status DIF", 2, report.CusExitReportItemsForBinding.Count);

				var reportItem1 = report.CusExitReportItems.FirstOrDefault(x => x.ERI_CCI_ConsignmentItem == consignmentItem3.PK);
				AssertEquals("reportItem1.ERI_CCI_ConsignmentItem", consignmentItem3.PK, reportItem1.ERI_CCI_ConsignmentItem);
				AssertEquals("reportItem1.ERI_GrossMass", 50m, reportItem1.ERI_GrossMass);
				AssertEquals("reportItem1.ERI_NetMass", 25m, reportItem1.ERI_NetMass);
				AssertEquals("reportItem1 has no additional infos", 0, reportItem1.AdditionalInfos.Count);
				AssertEquals("reportItem1 has no package associated", ZGuid.Empty, reportItem1.ERI_CXP_Package);

				var reportItem2 = report.CusExitReportItems.FirstOrDefault(x => x.ERI_CCI_ConsignmentItem == consignmentItem4.PK);
				AssertEquals("reportItem2.ERI_CCI_ConsignmentItem", consignmentItem4.PK, reportItem2.ERI_CCI_ConsignmentItem);
				AssertEquals("reportItem2.ERI_GrossMass", 100m, reportItem2.ERI_GrossMass);
				AssertEquals("reportItem2.ERI_NetMass", 45m, reportItem2.ERI_NetMass);
				AssertEquals("reportItem2 has no additional infos", 0, reportItem2.AdditionalInfos.Count);
				AssertEquals("reportItem2 has no package associated", ZGuid.Empty, reportItem2.ERI_CXP_Package);
			});
		}

		public void TestCreateOrUpdateReport_OneItemWithPackages_DiscrepancyStatusNotMIS()
		{
			var (consignment, header) = GetNewBusinessObject(Factory);
			var consignmentItem = CreateConsignmentItem(consignment, 1, ZString.Empty, 50m, 25m);

			var package1 = AddPackageToConsignmentItem(consignmentItem, ZString.Empty, 3);
			var package2 = AddPackageToConsignmentItem(consignmentItem, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 4);
			var package3 = AddPackageToConsignmentItem(consignmentItem, DiscrepanciesStatusCodeList.Codes.Missing, 5);
			var package4 = AddPackageToConsignmentItem(consignmentItem, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 6);

			Factory.Save();

			CombineAssertions(() =>
			{
				var report = header.CusExitReports[0];
				AssertEquals("Report created after saving has no items", 0, report.CusExitReportItems.Count);
				AssertEquals("Report created after saving has no items for binding", 0, report.CusExitReportItemsForBinding.Count);

				consignment.CreateOrUpdateReport();
				AssertEquals("After calling CreateOrUpdateReport there are 2 items added to the report, one for each package with DIF", 2, report.CusExitReportItems.Count);
				AssertEquals("After calling CreateOrUpdateReport there is 1 item for binding added to the report", 1, report.CusExitReportItemsForBinding.Count);

				var reportItem1 = package2.CusExitReportItems[0];
				AssertEquals("reportItem1.ERI_CCI_ConsignmentItem", consignmentItem.PK, reportItem1.ERI_CCI_ConsignmentItem);
				AssertEquals("reportItem1.ERI_GrossMass", 50m, reportItem1.ERI_GrossMass);
				AssertEquals("reportItem1.ERI_NetMass", 25m, reportItem1.ERI_NetMass);
				AssertEquals("reportItem1.ERI_Quantity", 4, reportItem1.ERI_Quantity);
				AssertEquals("reportItem1 is associated correctly to package2", package2.PK, reportItem1.ERI_CXP_Package);

				var reportItem2 = package4.CusExitReportItems[0];
				AssertEquals("reportItem2.ERI_CCI_ConsignmentItem", consignmentItem.PK, reportItem2.ERI_CCI_ConsignmentItem);
				AssertEquals("reportItem2.ERI_GrossMass", 50m, reportItem2.ERI_GrossMass);
				AssertEquals("reportItem2.ERI_NetMass", 25m, reportItem2.ERI_NetMass);
				AssertEquals("reportItem2.ERI_Quantity", 6, reportItem2.ERI_Quantity);
				AssertEquals("reportItem2 is associated correctly to package4", package4.PK, reportItem2.ERI_CXP_Package);
			});
		}

		public void TestCreateOrUpdateReport_OneItemWithPackages_DiscrepancyStatusMIS()
		{
			var (consignment, header) = GetNewBusinessObject(Factory);
			var consignmentItem = CreateConsignmentItem(consignment, 1, DiscrepanciesStatusCodeList.Codes.Missing, 50m, 25m);

			var package1 = AddPackageToConsignmentItem(consignmentItem, ZString.Empty, 3);
			var package2 = AddPackageToConsignmentItem(consignmentItem, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 4);
			var package3 = AddPackageToConsignmentItem(consignmentItem, DiscrepanciesStatusCodeList.Codes.Missing, 5);

			Factory.Save();

			CombineAssertions(() =>
			{
				var report = header.CusExitReports[0];
				AssertEquals("Report created after saving has no items", 0, report.CusExitReportItems.Count);

				consignment.CreateOrUpdateReport();
				AssertEquals("After calling CreateOrUpdateReport no items were added to the report", 0, report.CusExitReportItems.Count);
			});
		}

		public void TestCreateOrUpdateReport_OneItemWithAdditionalInfos_DiscrepancyStatusNotMIS()
		{
			var (consignment, header) = GetNewBusinessObject(Factory);
			var consignmentItem = CreateConsignmentItem(consignment, 1, ZString.Empty, 50m, 25m);

			var addInfo1 = AddDocumentToCollection(consignmentItem.AdditionalInfos, 1, "9001", "REF1", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);
			var addInfo2 = AddDocumentToCollection(consignmentItem.AdditionalInfos, 2, "9002", "REF2", ZString.Empty);
			var addInfo3 = AddDocumentToCollection(consignmentItem.AdditionalInfos, 3, "9003", "REF3", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);
			var addInfo4 = AddDocumentToCollection(consignmentItem.AdditionalInfos, 4, "9004", "REF4", DiscrepanciesStatusCodeList.Codes.Missing);

			Factory.Save();

			CombineAssertions(() =>
			{
				var report = header.CusExitReports[0];
				AssertEquals("Report created after saving has no items", 0, report.CusExitReportItems.Count);
				AssertEquals("Report created after saving has no items for binding", 0, report.CusExitReportItemsForBinding.Count);

				consignment.CreateOrUpdateReport();
				AssertEquals("After calling CreateOrUpdateReport there is 1 item added to the report", 1, report.CusExitReportItems.Count);
				AssertEquals("After calling CreateOrUpdateReport there is 1 item for binding added to the report", 1, report.CusExitReportItemsForBinding.Count);

				var reportItem = report.CusExitReportItems[0];
				AssertEquals("reportItem.ERI_CCI_ConsignmentItem", consignmentItem.PK, reportItem.ERI_CCI_ConsignmentItem);
				AssertEquals("reportItem.ERI_GrossMass", 50m, reportItem.ERI_GrossMass);
				AssertEquals("reportItem.ERI_NetMass", 25m, reportItem.ERI_NetMass);

				AssertEquals("reportItem.AdditionalInfos is 2, the ones with status DIF", 2, reportItem.AdditionalInfos.Count);
				AssertContainsExactElementsInExactOrder("reportItem.AdditionalInfos with correct data, CSI_ItemNumber", new ZInt[] { 1, 3 }, reportItem.AdditionalInfos.Select(x => x.CSI_ItemNumber));
				AssertContainsExactElementsInExactOrder("reportItem.AdditionalInfos with correct data, CSI_Code", new ZString[] { "9001", "9003" }, reportItem.AdditionalInfos.Select(x => x.CSI_Code));
				AssertContainsExactElementsInExactOrder("reportItem.AdditionalInfos with correct data, CSI_ReferenceNumber", new ZString[] { "REF1", "REF3" }, reportItem.AdditionalInfos.Select(x => x.CSI_ReferenceNumber));
				AssertContainsExactElementsInExactOrder("reportItem.AdditionalInfos with correct data, CSI_SubType", new ZString[] { "TRA", "TRA" }, reportItem.AdditionalInfos.Select(x => x.CSI_SubType));
				AssertContainsExactElementsInExactOrder("reportItem.AdditionalInfos with correct data, CSI_Status", new ZString[] { "DIF", "DIF" }, reportItem.AdditionalInfos.Select(x => x.CSI_Status));
			});
		}

		public void TestCreateOrUpdateReport_OneItemWithAdditionalInfos_DiscrepancyStatusMIS()
		{
			var (consignment, header) = GetNewBusinessObject(Factory);
			var consignmentItem = CreateConsignmentItem(consignment, 1, DiscrepanciesStatusCodeList.Codes.Missing, 50m, 25m);

			var addInfo1 = AddDocumentToCollection(consignmentItem.AdditionalInfos, 1, "9001", "REF1", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);
			var addInfo2 = AddDocumentToCollection(consignmentItem.AdditionalInfos, 2, "9002", "REF2", ZString.Empty);
			var addInfo3 = AddDocumentToCollection(consignmentItem.AdditionalInfos, 3, "9003", "REF3", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);

			Factory.Save();

			CombineAssertions(() =>
			{
				var report = header.CusExitReports[0];
				AssertEquals("Report created after saving has no items", 0, report.CusExitReportItems.Count);

				consignment.CreateOrUpdateReport();
				AssertEquals("After calling CreateOrUpdateReport no items were added to the report", 0, report.CusExitReportItems.Count);
			});
		}

		public void TestCreateOrUpdateReport_OneItemWithChildren_DiscrepancyStatusNotMIS()
		{
			var (consignment, header) = GetNewBusinessObject(Factory);
			var consignmentItem = CreateConsignmentItem(consignment, 1, ZString.Empty, 50m, 25m);

			var addInfo1 = AddDocumentToCollection(consignmentItem.AdditionalInfos, 1, "9001", "REF1", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);
			var addInfo2 = AddDocumentToCollection(consignmentItem.AdditionalInfos, 2, "9002", "REF2", ZString.Empty);
			var addInfo3 = AddDocumentToCollection(consignmentItem.AdditionalInfos, 3, "9003", "REF3", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);
			var addInfo4 = AddDocumentToCollection(consignmentItem.AdditionalInfos, 4, "9004", "REF4", DiscrepanciesStatusCodeList.Codes.Missing);

			var package1 = AddPackageToConsignmentItem(consignmentItem, ZString.Empty, 3);
			var package2 = AddPackageToConsignmentItem(consignmentItem, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 4);
			var package3 = AddPackageToConsignmentItem(consignmentItem, DiscrepanciesStatusCodeList.Codes.Missing, 5);
			var package4 = AddPackageToConsignmentItem(consignmentItem, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 6);

			Factory.Save();

			CombineAssertions(() =>
			{
				var report = header.CusExitReports[0];
				AssertEquals("Report created after saving has no items", 0, report.CusExitReportItems.Count);
				AssertEquals("Report created after saving has no items for binding", 0, report.CusExitReportItemsForBinding.Count);

				consignment.CreateOrUpdateReport();
				AssertEquals("After calling CreateOrUpdateReport there are 2 items added to the report, one for each package with DIF", 2, report.CusExitReportItems.Count);
				AssertEquals("After calling CreateOrUpdateReport there is 1 item for binding added to the report", 1, report.CusExitReportItemsForBinding.Count);

				var reportItem1 = package2.CusExitReportItems[0];
				AssertEquals("reportItem1.ERI_CCI_ConsignmentItem", consignmentItem.PK, reportItem1.ERI_CCI_ConsignmentItem);
				AssertEquals("reportItem1.ERI_GrossMass", 50m, reportItem1.ERI_GrossMass);
				AssertEquals("reportItem1.ERI_NetMass", 25m, reportItem1.ERI_NetMass);
				AssertEquals("reportItem1.ERI_Quantity", 4, reportItem1.ERI_Quantity);
				AssertEquals("reportItem1 is associated correctly to package2", package2.PK, reportItem1.ERI_CXP_Package);

				var reportItem2 = package4.CusExitReportItems[0];
				AssertEquals("reportItem2.ERI_CCI_ConsignmentItem", consignmentItem.PK, reportItem2.ERI_CCI_ConsignmentItem);
				AssertEquals("reportItem2.ERI_GrossMass", 50m, reportItem2.ERI_GrossMass);
				AssertEquals("reportItem2.ERI_NetMass", 25m, reportItem2.ERI_NetMass);
				AssertEquals("reportItem2.ERI_Quantity", 6, reportItem2.ERI_Quantity);
				AssertEquals("reportItem2 is associated correctly to package4", package4.PK, reportItem2.ERI_CXP_Package);

				AssertContainsExactElementsInAnyOrder("Only one reportItem has AdditionalInfos, it has 2, the ones with status DIF", new int[] { 2, 0 }, new int[] { reportItem1.AdditionalInfos.Count, reportItem2.AdditionalInfos.Count });
				var reportItemAdditionalInfos = reportItem1.AdditionalInfos.Any() ? reportItem1.AdditionalInfos : reportItem2.AdditionalInfos;
				AssertEquals("reportItemAdditionalInfos is 2, the ones with status DIF", 2, reportItemAdditionalInfos.Count);
				AssertContainsExactElementsInExactOrder("reportItemAdditionalInfos with correct data, CSI_ItemNumber", new ZInt[] { 1, 3 }, reportItemAdditionalInfos.Select(x => x.CSI_ItemNumber));
				AssertContainsExactElementsInExactOrder("reportItemAdditionalInfos with correct data, CSI_Code", new ZString[] { "9001", "9003" }, reportItemAdditionalInfos.Select(x => x.CSI_Code));
				AssertContainsExactElementsInExactOrder("reportItemAdditionalInfos with correct data, CSI_ReferenceNumber", new ZString[] { "REF1", "REF3" }, reportItemAdditionalInfos.Select(x => x.CSI_ReferenceNumber));
				AssertContainsExactElementsInExactOrder("reportItemAdditionalInfos with correct data, CSI_SubType", new ZString[] { "TRA", "TRA" }, reportItemAdditionalInfos.Select(x => x.CSI_SubType));
				AssertContainsExactElementsInExactOrder("reportItemAdditionalInfos with correct data, CSI_Status", new ZString[] { "DIF", "DIF" }, reportItemAdditionalInfos.Select(x => x.CSI_Status));
			});
		}

		public void TestCreateOrUpdateReport_OneItemWithChildren_DiscrepancyStatusMIS()
		{
			var (consignment, header) = GetNewBusinessObject(Factory);
			var consignmentItem = CreateConsignmentItem(consignment, 1, DiscrepanciesStatusCodeList.Codes.Missing, 50m, 25m);

			var addInfo1 = AddDocumentToCollection(consignmentItem.AdditionalInfos, 1, "9001", "REF1", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);
			var addInfo2 = AddDocumentToCollection(consignmentItem.AdditionalInfos, 2, "9002", "REF2", ZString.Empty);
			var addInfo3 = AddDocumentToCollection(consignmentItem.AdditionalInfos, 3, "9003", "REF3", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);

			var package1 = AddPackageToConsignmentItem(consignmentItem, ZString.Empty, 3);
			var package2 = AddPackageToConsignmentItem(consignmentItem, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 4);
			var package3 = AddPackageToConsignmentItem(consignmentItem, DiscrepanciesStatusCodeList.Codes.Missing, 5);

			Factory.Save();

			CombineAssertions(() =>
			{
				var report = header.CusExitReports[0];
				AssertEquals("Report created after saving has no items", 0, report.CusExitReportItems.Count);

				consignment.CreateOrUpdateReport();
				AssertEquals("After calling CreateOrUpdateReport no items were added to the report", 0, report.CusExitReportItems.Count);
			});
		}

		public void TestCreateOrUpdateReport_MultipleItemsWithChildren()
		{
			var (consignment, header) = GetNewBusinessObject(Factory);

			var consignmentItem1 = CreateConsignmentItem(consignment, 1, ZString.Empty, 50m, 25m);

			var addInfo1 = AddDocumentToCollection(consignmentItem1.AdditionalInfos, 1, "9001", "REF1", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);
			var addInfo2 = AddDocumentToCollection(consignmentItem1.AdditionalInfos, 2, "9002", "REF2", ZString.Empty);

			var package1 = AddPackageToConsignmentItem(consignmentItem1, ZString.Empty, 3);
			var package2 = AddPackageToConsignmentItem(consignmentItem1, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 4);
			var package3 = AddPackageToConsignmentItem(consignmentItem1, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 5);

			var consignmentItem2 = CreateConsignmentItem(consignment, 2, DiscrepanciesStatusCodeList.Codes.Missing, 40m, 10m);

			var addInfo3 = AddDocumentToCollection(consignmentItem2.AdditionalInfos, 3, "9003", "REF3", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);

			var consignmentItem3 = CreateConsignmentItem(consignment, 3, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 70m, 56m);

			var consignmentItem4 = CreateConsignmentItem(consignment, 4, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 100m, 45m);

			var addInfo4 = AddDocumentToCollection(consignmentItem4.AdditionalInfos, 4, "9004", "REF4", DiscrepanciesStatusCodeList.Codes.Missing);

			var package4 = AddPackageToConsignmentItem(consignmentItem4, DiscrepanciesStatusCodeList.Codes.Missing, 6);

			Factory.Save();

			CombineAssertions(() =>
			{
				var report = header.CusExitReports[0];
				AssertEquals("Report created after saving has no items", 0, report.CusExitReportItems.Count);

				consignment.CreateOrUpdateReport();
				AssertEquals("After calling CreateOrUpdateReport there are 4 items added to the report, one for each consignmentItem with DIF + each package with DIF", 4, report.CusExitReportItems.Count);

				AssertEquals("there are 2 reportItems associated to consignmentItem1 because there are 2 packages with DIF", 2, report.CusExitReportItems.Count(x => x.ERI_CCI_ConsignmentItem == consignmentItem1.PK));

				var reportItem1 = package2.CusExitReportItems[0];
				AssertEquals("reportItem1.ERI_CCI_ConsignmentItem", consignmentItem1.PK, reportItem1.ERI_CCI_ConsignmentItem);
				AssertEquals("reportItem1.ERI_GrossMass", 50m, reportItem1.ERI_GrossMass);
				AssertEquals("reportItem1.ERI_NetMass", 25m, reportItem1.ERI_NetMass);
				AssertEquals("reportItem1.ERI_Quantity", 4, reportItem1.ERI_Quantity);
				AssertEquals("reportItem1 is associated correctly to package2", package2.PK, reportItem1.ERI_CXP_Package);

				var reportItem2 = package3.CusExitReportItems[0];
				AssertEquals("reportItem2.ERI_CCI_ConsignmentItem", consignmentItem1.PK, reportItem2.ERI_CCI_ConsignmentItem);
				AssertEquals("reportItem2.ERI_GrossMass", 50m, reportItem2.ERI_GrossMass);
				AssertEquals("reportItem2.ERI_NetMass", 25m, reportItem2.ERI_NetMass);
				AssertEquals("reportItem2.ERI_Quantity", 5, reportItem2.ERI_Quantity);
				AssertEquals("reportItem2 is associated correctly to package3", package3.PK, reportItem2.ERI_CXP_Package);

				AssertContainsExactElementsInAnyOrder("Only one reportItem has AdditionalInfos, it has 1, the one with status DIF", new int[] { 1, 0 }, new int[] { reportItem1.AdditionalInfos.Count, reportItem2.AdditionalInfos.Count });
				var reportItemAdditionalInfos = reportItem1.AdditionalInfos.Any() ? reportItem1.AdditionalInfos : reportItem2.AdditionalInfos;
				AssertEquals("reportItemAdditionalInfos is 1, the one with status DIF", 1, reportItemAdditionalInfos.Count);
				var reportItemAdditionalInfo = reportItemAdditionalInfos[0];
				AssertEquals("reportItemAdditionalInfos with correct data, CSI_ItemNumber", (ZInt)1, reportItemAdditionalInfo.CSI_ItemNumber);
				AssertEquals("reportItemAdditionalInfos with correct data, CSI_Code", "9001", reportItemAdditionalInfo.CSI_Code);
				AssertEquals("reportItemAdditionalInfos with correct data, CSI_ReferenceNumber", "REF1", reportItemAdditionalInfo.CSI_ReferenceNumber);
				AssertEquals("reportItemAdditionalInfos with correct data, CSI_SubType", "TRA", reportItemAdditionalInfo.CSI_SubType);
				AssertEquals("reportItemAdditionalInfos with correct data, CSI_Status", "DIF", reportItemAdditionalInfo.CSI_Status);

				var reportItem3 = report.CusExitReportItems.FirstOrDefault(x => x.ERI_CCI_ConsignmentItem == consignmentItem3.PK);
				AssertEquals("reportItem3.ERI_CCI_ConsignmentItem", consignmentItem3.PK, reportItem3.ERI_CCI_ConsignmentItem);
				AssertEquals("reportItem3.ERI_GrossMass", 70m, reportItem3.ERI_GrossMass);
				AssertEquals("reportItem3.ERI_NetMass", 56m, reportItem3.ERI_NetMass);
				AssertEquals("reportItem3 has no additional infos", 0, reportItem3.AdditionalInfos.Count);
				AssertEquals("reportItem3 has no package associated", ZGuid.Empty, reportItem3.ERI_CXP_Package);

				var reportItem4 = report.CusExitReportItems.FirstOrDefault(x => x.ERI_CCI_ConsignmentItem == consignmentItem4.PK);
				AssertEquals("reportItem4.ERI_CCI_ConsignmentItem", consignmentItem4.PK, reportItem4.ERI_CCI_ConsignmentItem);
				AssertEquals("reportItem4.ERI_GrossMass", 100m, reportItem4.ERI_GrossMass);
				AssertEquals("reportItem4.ERI_NetMass", 45m, reportItem4.ERI_NetMass);
				AssertEquals("reportItem4 has no additional infos because the only one declared is MIS", 0, reportItem4.AdditionalInfos.Count);
				AssertEquals("reportItem4 has no package associated because the only one declared is MIS", ZGuid.Empty, reportItem4.ERI_CXP_Package);
			});
		}

		public void TestCreateOrUpdateReport_ItemsWithoutChildren_WithExistingReportItems()
		{
			var (consignment, header) = GetNewBusinessObject(Factory);

			var consignmentItem1 = CreateConsignmentItem(consignment, 1, ZString.Empty, 50m, 25m);

			var consignmentItem2 = CreateConsignmentItem(consignment, 2, DiscrepanciesStatusCodeList.Codes.Missing, 50m, 25m);

			var consignmentItem3 = CreateConsignmentItem(consignment, 3, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 50m, 25m);

			var consignmentItem4 = CreateConsignmentItem(consignment, 4, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 100m, 45m);
			Factory.Save();

			var report = header.CusExitReports[0];

			var reportItem1 = CreateReportItem(report, consignmentItem2, ZDecimal.Zero, ZDecimal.Zero, ZGuid.Empty);

			var reportItem2 = CreateReportItem(report, consignmentItem3, 20m, 5m, ZGuid.Empty);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Report has 2 items before calling CreateOrUpdateReport", 2, report.CusExitReportItems.Count);
				AssertEquals("Report has 2 items for binding before calling CreateOrUpdateReport", 2, report.CusExitReportItemsForBinding.Count);

				consignment.CreateOrUpdateReport();
				AssertEquals("After calling CreateOrUpdateReport there are 2 items in the report, the existing DIF one and a new DIF one, the MIS was removed", 2, report.CusExitReportItems.Count);
				AssertEquals("After calling CreateOrUpdateReport there are 2 items for binding in the report, the existing DIF one and a new DIF one, the MIS was removed", 2, report.CusExitReportItemsForBinding.Count);

				reportItem1 = report.CusExitReportItems.FirstOrDefault(x => x.ERI_CCI_ConsignmentItem == consignmentItem3.PK);
				AssertEquals("reportItem1.ERI_CCI_ConsignmentItem", consignmentItem3.PK, reportItem1.ERI_CCI_ConsignmentItem);
				AssertEquals("reportItem1.ERI_GrossMass (not changed)", 20m, reportItem1.ERI_GrossMass);
				AssertEquals("reportItem1.ERI_NetMass (not changed)", 5m, reportItem1.ERI_NetMass);
				AssertEquals("reportItem1 has no additional infos", 0, reportItem1.AdditionalInfos.Count);
				AssertEquals("reportItem1 has no package associated", ZGuid.Empty, reportItem1.ERI_CXP_Package);

				reportItem2 = report.CusExitReportItems.FirstOrDefault(x => x.ERI_CCI_ConsignmentItem == consignmentItem4.PK);
				AssertEquals("reportItem2.ERI_CCI_ConsignmentItem", consignmentItem4.PK, reportItem2.ERI_CCI_ConsignmentItem);
				AssertEquals("reportItem2.ERI_GrossMass", 100m, reportItem2.ERI_GrossMass);
				AssertEquals("reportItem2.ERI_NetMass", 45m, reportItem2.ERI_NetMass);
				AssertEquals("reportItem2 has no additional infos", 0, reportItem2.AdditionalInfos.Count);
				AssertEquals("reportItem2 has no package associated", ZGuid.Empty, reportItem2.ERI_CXP_Package);
			});
		}

		public void TestCreateOrUpdateReport_ItemWithChildren_WithExistingReportItems()
		{
			var (consignment, header) = GetNewBusinessObject(Factory);

			var consignmentItem1 = CreateConsignmentItem(consignment, 1, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 50m, 25m);

			var addInfo1 = AddDocumentToCollection(consignmentItem1.AdditionalInfos, 1, "9001", "REF1", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);
			var addInfo2 = AddDocumentToCollection(consignmentItem1.AdditionalInfos, 2, "9002", "REF2", ZString.Empty);
			var addInfo3 = AddDocumentToCollection(consignmentItem1.AdditionalInfos, 3, "9003", "REF3", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);
			var addInfo4 = AddDocumentToCollection(consignmentItem1.AdditionalInfos, 4, "9004", "REF4", DiscrepanciesStatusCodeList.Codes.Missing);

			var package1 = AddPackageToConsignmentItem(consignmentItem1, ZString.Empty, 3);
			var package2 = AddPackageToConsignmentItem(consignmentItem1, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 4);
			var package3 = AddPackageToConsignmentItem(consignmentItem1, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 5);
			var package4 = AddPackageToConsignmentItem(consignmentItem1, DiscrepanciesStatusCodeList.Codes.Missing, 6);
			var package5 = AddPackageToConsignmentItem(consignmentItem1, DiscrepanciesStatusCodeList.Codes.Missing, 10);

			Factory.Save();

			var report = header.CusExitReports[0];

			var reportItem1 = CreateReportItem(report, consignmentItem1, 20m, 5m, package3.PK);
			reportItem1.ERI_Quantity = 20;

			var addInfo5 = AddDocumentToCollection(reportItem1.AdditionalInfos, 3, "9003", "REF3", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);
			var addInfo6 = AddDocumentToCollection(reportItem1.AdditionalInfos, 4, "9004", "REF4", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);

			var reportItem2 = CreateReportItem(report, consignmentItem1, 20m, 5m, package4.PK);

			var reportItem3 = CreateReportItem(report, consignmentItem1, 20m, 5m, package5.PK);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Report has 3 items before calling CreateOrUpdateReport", 3, report.CusExitReportItems.Count);
				AssertEquals("Report has 1 item for binding before calling CreateOrUpdateReport", 1, report.CusExitReportItemsForBinding.Count);

				consignment.CreateOrUpdateReport();
				AssertEquals("After calling CreateOrUpdateReport there are 2 items in the report, the existing DIF package one and a new DIF package one, the MIS package was removed", 2, report.CusExitReportItems.Count);
				AssertEquals("After calling CreateOrUpdateReport there is 1 item for binding in the report", 1, report.CusExitReportItemsForBinding.Count);

				AssertEquals("There are 2 reportItems associated to consignmentItem1 because there are 2 packages with DIF", 2, report.CusExitReportItems.Count(x => x.ERI_CCI_ConsignmentItem == consignmentItem1.PK));

				reportItem1 = (CusExitReportItem)package3.CusExitReportItems[0];
				AssertEquals("reportItem1.ERI_CCI_ConsignmentItem", consignmentItem1.PK, reportItem1.ERI_CCI_ConsignmentItem);
				AssertEquals("reportItem1.ERI_GrossMass (not changed)", 20m, reportItem1.ERI_GrossMass);
				AssertEquals("reportItem1.ERI_NetMass (not changed)", 5m, reportItem1.ERI_NetMass);
				AssertEquals("reportItem1.ERI_Quantity (not changed)", 20, reportItem1.ERI_Quantity);
				AssertEquals("reportItem1 is associated correctly to package3", package3.PK, reportItem1.ERI_CXP_Package);

				reportItem2 = (CusExitReportItem)package2.CusExitReportItems[0];
				AssertEquals("reportItem2.ERI_CCI_ConsignmentItem", consignmentItem1.PK, reportItem2.ERI_CCI_ConsignmentItem);
				AssertEquals("reportItem2.ERI_GrossMass (not changed)", 20m, reportItem2.ERI_GrossMass);
				AssertEquals("reportItem2.ERI_NetMass (not changed)", 5m, reportItem2.ERI_NetMass);
				AssertEquals("reportItem2.ERI_Quantity", 4, reportItem2.ERI_Quantity);
				AssertEquals("reportItem2 is associated correctly to package2", package2.PK, reportItem2.ERI_CXP_Package);

				AssertContainsExactElementsInAnyOrder("Only one reportItem has AdditionalInfos, it has 2, the existing DIF one and a new DIF one, the MIS was removed", new int[] { 2, 0 }, new int[] { reportItem1.AdditionalInfos.Count, reportItem2.AdditionalInfos.Count });
				var reportItemAdditionalInfos = reportItem1.AdditionalInfos.Any() ? reportItem1.AdditionalInfos : reportItem2.AdditionalInfos;
				AssertEquals("reportItemAdditionalInfos is 2, the existing DIF one and a new DIF one, the MIS was removed", 2, reportItemAdditionalInfos.Count);
				AssertContainsExactElementsInAnyOrder("reportItem.AdditionalInfos with correct data, CSI_ItemNumber", new ZInt[] { 1, 3 }, reportItemAdditionalInfos.Select(x => x.CSI_ItemNumber));
				AssertContainsExactElementsInAnyOrder("reportItem.AdditionalInfos with correct data, CSI_Code", new ZString[] { "9001", "9003" }, reportItemAdditionalInfos.Select(x => x.CSI_Code));
				AssertContainsExactElementsInAnyOrder("reportItem.AdditionalInfos with correct data, CSI_ReferenceNumber", new ZString[] { "REF1", "REF3" }, reportItemAdditionalInfos.Select(x => x.CSI_ReferenceNumber));
				AssertContainsExactElementsInAnyOrder("reportItem.AdditionalInfos with correct data, CSI_SubType", new ZString[] { "TRA", "TRA" }, reportItemAdditionalInfos.Select(x => x.CSI_SubType));
				AssertContainsExactElementsInAnyOrder("reportItem.AdditionalInfos with correct data, CSI_Status", new ZString[] { "DIF", "DIF" }, reportItemAdditionalInfos.Select(x => x.CSI_Status));
			});
		}

		public void TestCreateOrUpdateReport_MultipleItemsWithChildren_WithExistingReportItems()
		{
			var (consignment, header) = GetNewBusinessObject(Factory);

			var consignmentItem1 = CreateConsignmentItem(consignment, 1, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 50m, 25m);

			var addInfo1 = AddDocumentToCollection(consignmentItem1.AdditionalInfos, 1, "9001", "REF1", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);
			var addInfo2 = AddDocumentToCollection(consignmentItem1.AdditionalInfos, 2, "9002", "REF2", ZString.Empty);
			var addInfo3 = AddDocumentToCollection(consignmentItem1.AdditionalInfos, 3, "9003", "REF3", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);
			var addInfo4 = AddDocumentToCollection(consignmentItem1.AdditionalInfos, 4, "9004", "REF4", DiscrepanciesStatusCodeList.Codes.Missing);

			var package1 = AddPackageToConsignmentItem(consignmentItem1, ZString.Empty, 3);
			var package2 = AddPackageToConsignmentItem(consignmentItem1, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 4);
			var package3 = AddPackageToConsignmentItem(consignmentItem1, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 5);
			var package4 = AddPackageToConsignmentItem(consignmentItem1, DiscrepanciesStatusCodeList.Codes.Missing, 6);

			var consignmentItem2 = CreateConsignmentItem(consignment, 2, DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared, 75m, 5m);

			var addInfo5 = AddDocumentToCollection(consignmentItem2.AdditionalInfos, 5, "9005", "REF5", ZString.Empty);
			var addInfo6 = AddDocumentToCollection(consignmentItem2.AdditionalInfos, 1, "9006", "REF6", DiscrepanciesStatusCodeList.Codes.Missing);

			var package5 = AddPackageToConsignmentItem(consignmentItem2, DiscrepanciesStatusCodeList.Codes.Missing, 10);
			var package6 = AddPackageToConsignmentItem(consignmentItem2, ZString.Empty, 11);

			Factory.Save();

			var report = header.CusExitReports[0];

			var reportItem1 = CreateReportItem(report, consignmentItem1, 20m, 5m, package3.PK);
			reportItem1.ERI_Quantity = 20;

			var addInfo7 = AddDocumentToCollection(reportItem1.AdditionalInfos, 3, "9003", "REF3", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);
			var addInfo8 = AddDocumentToCollection(reportItem1.AdditionalInfos, 4, "9004", "REF4", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);

			var reportItem2 = CreateReportItem(report, consignmentItem1, 20m, 5m, package4.PK);

			var reportItem3 = CreateReportItem(report, consignmentItem2, 25m, 15m, package5.PK);

			var addInfo9 = AddDocumentToCollection(reportItem3.AdditionalInfos, 5, "9005", "REF5", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);
			var addInfo10 = AddDocumentToCollection(reportItem3.AdditionalInfos, 1, "9006", "REF6", DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared);

			var reportItem4 = CreateReportItem(report, consignmentItem2, 25m, 15m, package6.PK);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Report has 4 items before calling CreateOrUpdateReport", 4, report.CusExitReportItems.Count);
				AssertEquals("Report has 2 items for binding before calling CreateOrUpdateReport", 2, report.CusExitReportItemsForBinding.Count);

				consignment.CreateOrUpdateReport();
				AssertEquals("After calling CreateOrUpdateReport there are 3 items in the report, the existing DIF package one and a new DIF package one, the MIS package was removed", 3, report.CusExitReportItems.Count);
				AssertEquals("After calling CreateOrUpdateReport there are 2 items for binding in the report", 2, report.CusExitReportItemsForBinding.Count);

				AssertEquals("There are 2 reportItems associated to consignmentItem1 because there are 2 packages with DIF", 2, report.CusExitReportItems.Count(x => x.ERI_CCI_ConsignmentItem == consignmentItem1.PK));
				AssertEquals("There is 1 reportItem associated to consignmentItem2 because it has status DIF even though the packages are MIS or empty", 1, report.CusExitReportItems.Count(x => x.ERI_CCI_ConsignmentItem == consignmentItem2.PK));

				reportItem1 = (CusExitReportItem)package3.CusExitReportItems[0];
				AssertEquals("reportItem1.ERI_CCI_ConsignmentItem", consignmentItem1.PK, reportItem1.ERI_CCI_ConsignmentItem);
				AssertEquals("reportItem1.ERI_GrossMass (not changed)", 20m, reportItem1.ERI_GrossMass);
				AssertEquals("reportItem1.ERI_NetMass (not changed)", 5m, reportItem1.ERI_NetMass);
				AssertEquals("reportItem1.ERI_Quantity (not changed)", 20, reportItem1.ERI_Quantity);
				AssertEquals("reportItem1 is associated correctly to package3", package3.PK, reportItem1.ERI_CXP_Package);

				reportItem2 = (CusExitReportItem)package2.CusExitReportItems[0];
				AssertEquals("reportItem2.ERI_CCI_ConsignmentItem", consignmentItem1.PK, reportItem2.ERI_CCI_ConsignmentItem);
				AssertEquals("reportItem2.ERI_GrossMass (not changed)", 20m, reportItem2.ERI_GrossMass);
				AssertEquals("reportItem2.ERI_NetMass (not changed)", 5m, reportItem2.ERI_NetMass);
				AssertEquals("reportItem2.ERI_Quantity", 4, reportItem2.ERI_Quantity);
				AssertEquals("reportItem2 is associated correctly to package2", package2.PK, reportItem2.ERI_CXP_Package);

				reportItem3 = report.CusExitReportItems.FirstOrDefault(x => x.ERI_CCI_ConsignmentItem == consignmentItem2.PK);
				AssertEquals("reportItem3.ERI_CCI_ConsignmentItem", consignmentItem2.PK, reportItem3.ERI_CCI_ConsignmentItem);
				AssertEquals("reportItem3.ERI_GrossMass (not changed)", 25m, reportItem3.ERI_GrossMass);
				AssertEquals("reportItem3.ERI_NetMass (not changed)", 15m, reportItem3.ERI_NetMass);
				AssertEquals("reportItem3.ERI_Quantity", 0, reportItem3.ERI_Quantity);
				AssertEquals("reportItem3 is not associated to a package", ZGuid.Empty, reportItem3.ERI_CXP_Package);
				AssertEquals("reportItem3 has no AdditionalInfos because neither was DIF", 0, reportItem3.AdditionalInfos.Count);

				AssertContainsExactElementsInAnyOrder("Only one reportItem has AdditionalInfos, it has 2, the existing DIF one and a new DIF one, the MIS was removed", new int[] { 2, 0 }, new int[] { reportItem1.AdditionalInfos.Count, reportItem2.AdditionalInfos.Count });
				var reportItemAdditionalInfos = reportItem1.AdditionalInfos.Any() ? reportItem1.AdditionalInfos : reportItem2.AdditionalInfos;
				AssertEquals("reportItemAdditionalInfos is 2, the existing DIF one and a new DIF one, the MIS was removed", 2, reportItemAdditionalInfos.Count);
				AssertContainsExactElementsInAnyOrder("reportItem.AdditionalInfos with correct data, CSI_ItemNumber", new ZInt[] { 1, 3 }, reportItemAdditionalInfos.Select(x => x.CSI_ItemNumber));
				AssertContainsExactElementsInAnyOrder("reportItem.AdditionalInfos with correct data, CSI_Code", new ZString[] { "9001", "9003" }, reportItemAdditionalInfos.Select(x => x.CSI_Code));
				AssertContainsExactElementsInAnyOrder("reportItem.AdditionalInfos with correct data, CSI_ReferenceNumber", new ZString[] { "REF1", "REF3" }, reportItemAdditionalInfos.Select(x => x.CSI_ReferenceNumber));
				AssertContainsExactElementsInAnyOrder("reportItem.AdditionalInfos with correct data, CSI_SubType", new ZString[] { "TRA", "TRA" }, reportItemAdditionalInfos.Select(x => x.CSI_SubType));
				AssertContainsExactElementsInAnyOrder("reportItem.AdditionalInfos with correct data, CSI_Status", new ZString[] { "DIF", "DIF" }, reportItemAdditionalInfos.Select(x => x.CSI_Status));
			});
		}

		CusExitConsignmentItem CreateConsignmentItem(CusExitConsignment consignment, ZShort lineNumber, ZString status, ZDecimal grossMass, ZDecimal netMass)
		{
			var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem.CCI_LineNumber = lineNumber;
			consignmentItem.CCI_DiscrepancyStatus = status;
			consignmentItem.CCI_Calc_ReportGrossMass = grossMass;
			consignmentItem.CCI_Calc_ReportNetMass = netMass;
			return consignmentItem;
		}

		CusExitReportItem CreateReportItem(CusExitReport report, CusExitConsignmentItem consignmentItem, ZDecimal grossMass, ZDecimal netMass, ZGuid packagePK)
		{
			var reportItem = report.CusExitReportItems.AddNew();
			reportItem.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			reportItem.ERI_GrossMass = grossMass;
			reportItem.ERI_NetMass = netMass;
			reportItem.ERI_CXP_Package = packagePK;
			return reportItem;
		}

		EU.ExitControl.Business.CusExitConsignmentPackage AddPackageToConsignmentItem(CusExitConsignmentItem consignmentItem, ZString status, ZInt quantity)
		{
			var consignmentPackagePivot = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var package = consignmentPackagePivot.Package;
			package.CXP_MarksAndNumbersStatus = status;
			package.CXP_Calc_ReportQuantity = quantity;
			return package;
		}

		EU.ExitControl.Business.AdditionalInfo AddDocumentToCollection(IAdditionalInfoCollection<EU.ExitControl.Business.AdditionalInfo> addInfoCollection, ZInt itemNumber, ZString code, ZString reference, ZString status)
		{
			var addInfo = addInfoCollection.AddNew();
			addInfo.CSI_ItemNumber = itemNumber;
			addInfo.CSI_Code = code;
			addInfo.CSI_ReferenceNumber = reference;
			addInfo.CSI_SubType = "TRA";
			addInfo.CSI_Status = status;
			return addInfo;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).consignment;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).consignment;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory).consignment;

		public static (CusExitConsignment consignment, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = CusExitHeaderTest.GetNewBusinessObject(factory);
			var consignment = header.CusExitConsignments.AddNew();
			return (consignment, header);
		}
	}
}
