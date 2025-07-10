using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	sealed class EXTINFHeaderProviderTest : Customs.Business.Testing.DataProviderTestCase<EXTINFHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXTINFHeaderProvider(null));
		}

		public void TestConsignmentModeOfTransportAtTheBorder()
		{
			CombineAssertions(() =>
			{
				report.CER_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("CER_TransportMode = 'AIR'", "4", Provider.ConsignmentModeOfTransportAtTheBorder);

				report.CER_TransportMode = TransportTypeList.Codes.OwnPropulsion;
				AssertEquals("CER_TransportMode = 'OWN'", "9", Provider.ConsignmentModeOfTransportAtTheBorder);
			});
		}

		public void TestConsignmentReferenceNumberUCR()
		{
			consignment.CXC_UniqueConsignmentReference = "DEXYZ123";
			CombineAssertions(() =>
			{
				AssertNotEquals("InformationType != 'FP'", A0132ReportInformationType.Codes.FV, Provider.InformationType);
				AssertEquals("ConsignmentReferenceNumberUCR when InformationType != 'FV'", "DEXYZ123", Provider.ConsignmentReferenceNumberUCR);

				consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
				report.CER_Calc_Discrepancies = true;
				CreateConsignmentLevelReport();
				var provider = GetProvider();
				AssertEquals("InformationType = 'FP'", A0132ReportInformationType.Codes.FV, provider.InformationType);
				AssertEquals("ConsignmentReferenceNumberUCR when InformationType = 'FV'", string.Empty, provider.ConsignmentReferenceNumberUCR);
			});
		}

		public void TestTransportEquipment_WithContainers()
		{
			var container1 = header.CusExitContainers.AddNew();
			container1.CXN_ContainerNumber = "CNT1";
			var container2 = header.CusExitContainers.AddNew();
			container2.CXN_ContainerNumber = "CNT2";
			container2.AllSealNumbers.AddNew().BK_SealNumber = "1";
			var container3 = header.CusExitContainers.AddNew();
			container3.CXN_ContainerNumber = "CNT3";
			container3.AllSealNumbers.AddNew().BK_SealNumber = "2";
			container3.AllSealNumbers.AddNew();

			var item1 = consignment.CusExitConsignmentItems.AddNew();
			item1.CCI_LineNumber = 1;
			var item2 = consignment.CusExitConsignmentItems.AddNew();
			item2.CCI_LineNumber = 2;

			item1.CusExitConsignmentPackagePivots.AddNew().CNP_CXN_Container = container2.PK;
			item2.CusExitConsignmentPackagePivots.AddNew().CNP_CXN_Container = container2.PK;
			item2.CusExitConsignmentPackagePivots.AddNew().CNP_CXN_Container = container3.PK;

			report.CER_IsFinalized = true;
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
			report.CER_Calc_Discrepancies = true;
			CreateConsignmentLevelReport();

			CombineAssertions(() =>
			{
				AssertEquals("Finalization = 1", "1", Provider.Finalization);
				AssertEquals("InformationType in (FV, FW, FP, LV, LW, LP, NV)", A0132ReportInformationType.Codes.FW, Provider.InformationType);
				AssertContainsExactElementsInAnyOrder(new[] { "CNT2", "CNT3" }, Provider.TransportEquipment.Select(x => x.ContainerIdentificationNumber));
				AssertEquals("Has SealIdentifiers when Finalization = 1 and InformationType in (FV, FW, FP, LV, LW, LP, NV)", 3, Provider.TransportEquipment.Sum(x => x.NumberOfSeals));

				report.CER_IsFinalized = false;
				var provider = GetProvider();
				AssertEquals("Finalization = 0", "0", provider.Finalization);
				AssertEquals("No SealIdentifiers when Finalization = 0", 0, provider.TransportEquipment.Sum(x => x.SealIdentifiers.Count));

				report.CER_IsFinalized = true;
				consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._353;
				report.CER_Calc_Discrepancies = false;
				CreatePackageLevelReport();
				provider = GetProvider();
				AssertEquals("InformationType != (FV, FW, FP, LV, LW, LP, NV)", A0132ReportInformationType.Codes.UW, provider.InformationType);
				AssertEquals("No SealIdentifiers when InformationType != (FV, FW, FP, LV, LW, LP, NV)", 0, provider.TransportEquipment.Sum(x => x.SealIdentifiers.Count));
			});
		}

		public void TestTransportEquipment_WithoutContainers_NumberOfSealsPopulated()
		{
			report.CER_IsFinalized = true;
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
			report.CER_Calc_Discrepancies = true;
			CreateConsignmentLevelReport();

			CombineAssertions(() =>
			{
				AssertEquals("Finalization = 1", "1", Provider.Finalization);
				AssertEquals("InformationType in (FV, FW, FP, LV, LW, LP, NV)", A0132ReportInformationType.Codes.FV, Provider.InformationType);
				AssertEquals("Finalization = 1 and InformationType in (FV, FW, FP, LV, LW, LP, NV)", 0, Provider.TransportEquipment.Single().NumberOfSeals);
			});
		}

		public void TestTransportEquipment_WithoutContainers_NumberOfSealsNull()
		{
			report.CER_IsFinalized = true;
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._353;
			report.CER_Calc_Discrepancies = false;
			CreateItemLevelReport();

			CombineAssertions(() =>
			{
				AssertEquals("InformationType != (FV, FW, FP, LV, LW, LP, NV)", A0132ReportInformationType.Codes.UW, Provider.InformationType);
				AssertNull("Finalization = 1, InformationType != (FV, FW, FP, LV, LW, LP, NV)", Provider.TransportEquipment.Single().NumberOfSeals);
			});
		}

		public void TestActiveBorderTransportMeans()
		{
			report.CER_Location = "Sydney";
			CombineAssertions(() =>
			{
				AssertNull("InformationType != ('LV', 'UV')", Provider.ActiveBorderTransportMeans);

				consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
				report.CER_Calc_Discrepancies = false;
				CreateConsignmentLevelReport();
				var provider = GetProvider();
				AssertEquals("InformationType = 'LV'", A0132ReportInformationType.Codes.LV, provider.InformationType);
				AssertEquals("ActiveBorderTransportMeans when InformationType = 'LV'", "Sydney", provider.ActiveBorderTransportMeans.Location);

				consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._353;
				provider = GetProvider();
				AssertEquals("InformationType = 'UV'", A0132ReportInformationType.Codes.UV, provider.InformationType);
				AssertEquals("ActiveBorderTransportMeans when InformationType = 'UV'", "Sydney", provider.ActiveBorderTransportMeans.Location);
			});
		}

		public void TestLines()
		{
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
			report.CER_Calc_Discrepancies = false;
			var provider = GetProvider();

			CombineAssertions(() =>
			{
				AssertEquals("InformationType = 'NV'", A0132ReportInformationType.Codes.NV, provider.InformationType);
				AssertEquals("Lines when InformationType = 'NV'", 0, provider.Lines.Count);

				report.CER_Calc_Discrepancies = true;
				CreateConsignmentLevelReport();
				provider = GetProvider();
				AssertEquals("InformationType = 'FV'", A0132ReportInformationType.Codes.FV, provider.InformationType);
				AssertEquals("Lines when InformationType = 'FV'", 0, provider.Lines.Count);

				report.CusExitReportItems.DeleteAll();
				consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._353;
				report.CER_Calc_Discrepancies = false;
				CreatePackageLevelReport();
				provider = GetProvider();
				AssertEquals("InformationType = 'UP'", A0132ReportInformationType.Codes.UP, provider.InformationType);
				AssertContainsExactElementsInAnyOrder("Lines when InformationType != (NV, FV)", new[] { "1", "2" }, provider.Lines.Select(x => x.SequenceNumber));
			});
		}

		public void TestLRN()
		{
			consignment.CXC_LocalReference = "LRN123";
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
			report.CER_Calc_Discrepancies = false;
			CreateItemLevelReport();

			CombineAssertions(() =>
			{
				AssertEquals("LRN when InformationType != (UV, UW, UP)", "LRN123", GetProvider().LRN);

				consignment.CXC_MovementReference = "MRN123";
				AssertEquals("LRN when MRN != ''", null, GetProvider().LRN);

				consignment.CXC_MovementReference = "";
				report.CusExitReportItems.DeleteAll();
				consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._353;
				CreateConsignmentLevelReport();
				var provider = GetProvider();
				AssertEquals("InformationType = 'UV'", A0132ReportInformationType.Codes.UV, provider.InformationType);
				AssertEquals("LRN when InformationType = 'UV'", null, provider.LRN);

				report.CusExitReportItems.DeleteAll();
				CreateItemLevelReport();
				provider = GetProvider();
				AssertEquals("InformationType = 'UW'", A0132ReportInformationType.Codes.UW, provider.InformationType);
				AssertEquals("LRN when InformationType = 'UW'", null, provider.LRN);

				report.CusExitReportItems.DeleteAll();
				CreatePackageLevelReport();
				provider = GetProvider();
				AssertEquals("InformationType = 'UP'", A0132ReportInformationType.Codes.UP, provider.InformationType);
				AssertEquals("LRN when InformationType = 'UP'", null, provider.LRN);
			});
		}

		public void TestRegistrationNumberExternal()
		{
			consignment.CXC_ReferenceNumber = "REF123";

			CombineAssertions(() =>
			{
				AssertNotEquals("InformationType != 'FP'", A0132ReportInformationType.Codes.FV, Provider.InformationType);
				AssertEquals("RegistrationNumberExternal when InformationType != 'FV'", "REF123", Provider.RegistrationNumberExternal);

				consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
				report.CER_Calc_Discrepancies = true;
				CreateConsignmentLevelReport();
				var provider = GetProvider();
				AssertEquals("InformationType = 'FP'", A0132ReportInformationType.Codes.FV, provider.InformationType);
				AssertEquals("RegistrationNumberExternal when InformationType = 'FV'", null, provider.RegistrationNumberExternal);
			});
		}

		public void TestDeclarant()
		{
			var org = Factory.New<OrgHeader>();
			report.Declarant.E2_OA_Address = org.MainAddress.PK;

			consignment.CXC_LocalReference = "LRN123";
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
			report.CER_Calc_Discrepancies = false;
			CreateItemLevelReport();

			CombineAssertions(() =>
			{
				AssertNotNull("Declarant when InformationType != (UV, UW, UP)", GetProvider().Declarant);

				consignment.CXC_MovementReference = "MRN123";
				AssertNull("Declarant when MRN != ''", GetProvider().Declarant);

				consignment.CXC_MovementReference = "";
				report.CusExitReportItems.DeleteAll();
				consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._353;
				CreateConsignmentLevelReport();
				var provider = GetProvider();
				AssertEquals("InformationType = 'UV'", A0132ReportInformationType.Codes.UV, provider.InformationType);
				AssertNull("Declarant when InformationType = 'UV'", provider.Declarant);

				report.CusExitReportItems.DeleteAll();
				CreateItemLevelReport();
				provider = GetProvider();
				AssertEquals("InformationType = 'UW'", A0132ReportInformationType.Codes.UW, provider.InformationType);
				AssertNull("Declarant when InformationType = 'UW'", provider.Declarant);

				report.CusExitReportItems.DeleteAll();
				CreatePackageLevelReport();
				provider = GetProvider();
				AssertEquals("InformationType = 'UP'", A0132ReportInformationType.Codes.UP, provider.InformationType);
				AssertNull("Declarant when InformationType = 'UP'", provider.Declarant);
			});
		}

		public void TestRepresentative()
		{
			var org = Factory.New<OrgHeader>();
			report.Representative.E2_OA_Address = org.MainAddress.PK;

			consignment.CXC_LocalReference = "LRN123";
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._352;
			report.CER_Calc_Discrepancies = false;
			CreateItemLevelReport();
			AssertNotNull("Representative when InformationType != (UV, UW, UP)", GetProvider().Representative);

			report.Declarant.E2_OA_Address = org.MainAddress.PK;
			AssertNull("Representative when Declarant != null", GetProvider().Representative);

			report.Declarant.E2_OA_Address = ZGuid.Empty;
			consignment.CXC_MovementReference = "MRN123";
			AssertNull("Representative when MRN != ''", GetProvider().Representative);

			consignment.CXC_MovementReference = "";
			report.CusExitReportItems.DeleteAll();
			consignment.CXC_Status = A0116ATLASStatusCodeList.Codes._353;
			CreateConsignmentLevelReport();
			var provider = GetProvider();
			AssertEquals("InformationType = 'UV'", A0132ReportInformationType.Codes.UV, provider.InformationType);
			AssertNull("Representative when InformationType = 'UV'", provider.Representative);

			report.CusExitReportItems.DeleteAll();
			CreateItemLevelReport();
			provider = GetProvider();
			AssertEquals("InformationType = 'UW'", A0132ReportInformationType.Codes.UW, provider.InformationType);
			AssertNull("Representative when InformationType = 'UW'", provider.Representative);

			report.CusExitReportItems.DeleteAll();
			CreatePackageLevelReport();
			provider = GetProvider();
			AssertEquals("InformationType = 'UP'", A0132ReportInformationType.Codes.UP, provider.InformationType);
			AssertNull("Representative when InformationType = 'UP'", provider.Representative);
		}

		protected override EXTINFHeaderProvider GetProvider() => new EXTINFHeaderProvider(report);

		void CreateReportItem(CusExitConsignmentItem item, CusExitConsignmentPackage package)
		{
			var reportItem = report.CusExitReportItems.AddNew();
			reportItem.ERI_CCI_ConsignmentItem = item.PK;
			reportItem.ERI_CXP_Package = package.PK;
		}

		void CreateConsignmentLevelReport()
		{
			CreateReportItem(item1, package1);
			CreateReportItem(item1, package2);
			CreateReportItem(item2, package3);
		}

		void CreatePackageLevelReport()
		{
			CreateReportItem(item1, package1);
			CreateReportItem(item2, package3);
		}

		void CreateItemLevelReport()
		{
			CreateReportItem(item1, package1);
			CreateReportItem(item1, package2);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusExitHeader>();
			consignment = header.CusExitConsignments.AddNew();
			item1 = consignment.CusExitConsignmentItems.AddNew();
			item1.CCI_LineNumber = 1;
			item2 = consignment.CusExitConsignmentItems.AddNew();
			item2.CCI_LineNumber = 2;
			package1 = item1.CusExitConsignmentPackagePivots.AddNew().Package;
			package2 = item1.CusExitConsignmentPackagePivots.AddNew().Package;
			package3 = item2.CusExitConsignmentPackagePivots.AddNew().Package;
			report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
		}
		CusExitHeader header;
		CusExitConsignment consignment;
		CusExitConsignmentItem item1;
		CusExitConsignmentItem item2;
		CusExitConsignmentPackage package1;
		CusExitConsignmentPackage package2;
		CusExitConsignmentPackage package3;
		CusExitReport report;
	}
}
