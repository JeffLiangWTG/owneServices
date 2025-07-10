using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	sealed class EXTNOTHeaderProviderTest : Customs.Business.Testing.DataProviderTestCase<EXTNOTHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXTNOTHeaderProvider(null));
		}

		protected override EXTNOTHeaderProvider GetProvider() => new EXTNOTHeaderProvider(report);

		public void TestIntendedExitCustomsOffice() => CombineAssertions(() =>
		{
			report.CER_OfficeOfExport = "DEABC";
			report.CER_Calc_Discrepancies = false;
			CreateConsignmentLevelReport();
			var provider = GetProvider();
			AssertEquals("ShipmentType = 'WV'", A0131ATLASTypeOfShipment.Codes.WV, provider.ShipmentType);
			AssertEquals("IntendedExitCustomsOffice when ShipmentType = 'WV'", "DEABC", provider.IntendedExitCustomsOffice);

			report.CusExitReportItems.DeleteAll();
			CreateItemLevelReport();
			provider = GetProvider();
			AssertEquals("ShipmentType = 'WW'", A0131ATLASTypeOfShipment.Codes.WW, provider.ShipmentType);
			AssertEquals("IntendedExitCustomsOffice when ShipmentType = 'WV'", "DEABC", provider.IntendedExitCustomsOffice);

			report.CER_Calc_Discrepancies = true;
			AssertEquals("IntendedExitCustomsOffice when ShipmentType != (WV, WW)", null, GetProvider().IntendedExitCustomsOffice);
		});

		public void TestExitDateAndTime() => CombineAssertions(() =>
		{
			report.CER_DateTime = new ZDateTimeOffset(2022, 02, 15, 16, 43, 27);

			report.CER_OfficeOfExport = string.Empty;
			report.CER_Calc_Discrepancies = false;
			AssertNull("No items", GetProvider().ExitDateAndTime);

			CreateConsignmentLevelReport();
			AssertEquals("Items present ", new DateTime(2022, 02, 15, 15, 43, 27), GetProvider().ExitDateAndTime);

			report.CER_DateTime = ZDateTimeOffset.Empty;
			AssertNull("Null", GetProvider().ExitDateAndTime);

			report.CER_DateTime = new ZDateTimeOffset(2022, 02, 15, 16, 43, 27);
			report.CER_OfficeOfExport = "ABC";
			AssertNull("Office set ", GetProvider().ExitDateAndTime);

			report.CER_OfficeOfExport = string.Empty;
			report.CER_Calc_Discrepancies = true;
			AssertNull("Discrepansies", GetProvider().ExitDateAndTime);
		});

		public void TestForwardingDateAndTime() => CombineAssertions(() =>
		{
			report.CER_DateTime = new ZDateTimeOffset(2022, 02, 15, 16, 43, 27);
			CreateConsignmentLevelReport();
			report.CER_OfficeOfExport = string.Empty;
			AssertNull("Office empty", GetProvider().ForwardingDateAndTime);

			report.CER_OfficeOfExport = "ABC";
			AssertEquals("Office set", new DateTime(2022, 02, 15, 15, 43, 27), GetProvider().ForwardingDateAndTime);

			report.CER_DateTime = ZDateTimeOffset.Empty;
			AssertNull("Null", GetProvider().ForwardingDateAndTime);
		});

		public void TestFinalization()
		{
			CombineAssertions(() =>
			{
				var provider = GetProvider();
				report.CER_IsFinalized = true;
				AssertEquals("CER_IsFinalized = true", "1", provider.Finalization);
				report.CER_IsFinalized = false;
				AssertEquals("CER_IsFinalized = false", "0", provider.Finalization);
			});
		}

		public void TestShipmentType_AP()
		{
			report.CER_OfficeOfExport = string.Empty;
			report.CER_Calc_Discrepancies = false;
			CreatePackageLevelReport();

			AssertEquals(A0131ATLASTypeOfShipment.Codes.AP, GetProvider().ShipmentType);
		}

		public void TestShipmentType_AV()
		{
			report.CER_OfficeOfExport = string.Empty;
			report.CER_Calc_Discrepancies = false;
			CreateConsignmentLevelReport();

			AssertEquals(A0131ATLASTypeOfShipment.Codes.AV, GetProvider().ShipmentType);
		}

		public void TestShipmentType_AW()
		{
			report.CER_OfficeOfExport = string.Empty;
			report.CER_Calc_Discrepancies = false;
			CreateItemLevelReport();

			AssertEquals(A0131ATLASTypeOfShipment.Codes.AW, GetProvider().ShipmentType);
		}

		public void TestShipmentType_FP()
		{
			report.CER_OfficeOfExport = string.Empty;
			report.CER_Calc_Discrepancies = true;
			CreatePackageLevelReport();

			AssertEquals(A0131ATLASTypeOfShipment.Codes.FP, GetProvider().ShipmentType);
		}

		public void TestShipmentType_FV()
		{
			report.CER_OfficeOfExport = string.Empty;
			report.CER_Calc_Discrepancies = true;
			CreateConsignmentLevelReport();

			AssertEquals(A0131ATLASTypeOfShipment.Codes.FV, GetProvider().ShipmentType);
		}

		public void TestShipmentType_FW()
		{
			report.CER_OfficeOfExport = string.Empty;
			report.CER_Calc_Discrepancies = true;
			CreateItemLevelReport();

			AssertEquals(A0131ATLASTypeOfShipment.Codes.FW, GetProvider().ShipmentType);
		}

		public void TestShipmentType_NV()
		{
			report.CER_OfficeOfExport = string.Empty;
			report.CER_Calc_Discrepancies = false;

			AssertEquals(A0131ATLASTypeOfShipment.Codes.NV, GetProvider().ShipmentType);
		}

		public void TestShipmentType_WV()
		{
			report.CER_OfficeOfExport = "DEABC";
			report.CER_Calc_Discrepancies = false;
			CreateConsignmentLevelReport();

			AssertEquals(A0131ATLASTypeOfShipment.Codes.WV, GetProvider().ShipmentType);
		}

		public void TestShipmentType_WW()
		{
			report.CER_OfficeOfExport = "DEABC";
			report.CER_Calc_Discrepancies = false;
			CreateItemLevelReport();

			AssertEquals(A0131ATLASTypeOfShipment.Codes.WW, GetProvider().ShipmentType);
		}

		public void TestLines() => CombineAssertions(() =>
		{
			report.CER_Calc_Discrepancies = false;
			CreateConsignmentLevelReport();
			var provider = GetProvider();
			AssertEquals("ShipmentType = 'AV'", A0131ATLASTypeOfShipment.Codes.AV, provider.ShipmentType);
			AssertEquals("Lines when ShipmentType != (FW, FP, AW, AP, WW)", 0, provider.Lines.Count);

			report.CusExitReportItems.DeleteAll();
			report.CER_Calc_Discrepancies = true;
			CreateItemLevelReport();
			provider = GetProvider();
			AssertEquals("ShipmentType = 'FW'", A0131ATLASTypeOfShipment.Codes.FW, provider.ShipmentType);
			AssertContainsExactElementsInAnyOrder("Lines when ShipmentType = 'FW'", new[] { "1" }, provider.Lines.Select(x => x.SequenceNumber));

			report.CusExitReportItems.DeleteAll();
			CreatePackageLevelReport();
			provider = GetProvider();
			AssertEquals("ShipmentType = 'FP'", A0131ATLASTypeOfShipment.Codes.FP, provider.ShipmentType);
			AssertContainsExactElementsInAnyOrder("Lines when ShipmentType = 'FP'", new[] { "1", "2" }, provider.Lines.Select(x => x.SequenceNumber));

			report.CusExitReportItems.DeleteAll();
			report.CER_Calc_Discrepancies = false;
			CreateItemLevelReport();
			provider = GetProvider();
			AssertEquals("ShipmentType = 'AW'", A0131ATLASTypeOfShipment.Codes.AW, provider.ShipmentType);
			AssertContainsExactElementsInAnyOrder("Lines when ShipmentType = 'AW'", new[] { "1" }, provider.Lines.Select(x => x.SequenceNumber));

			report.CusExitReportItems.DeleteAll();
			CreatePackageLevelReport();
			provider = GetProvider();
			AssertEquals("ShipmentType = 'AP'", A0131ATLASTypeOfShipment.Codes.AP, provider.ShipmentType);
			AssertContainsExactElementsInAnyOrder("Lines when ShipmentType = 'AP'", new[] { "1", "2" }, provider.Lines.Select(x => x.SequenceNumber));

			report.CusExitReportItems.DeleteAll();
			report.CER_OfficeOfExport = "DEABC";
			CreateItemLevelReport();
			provider = GetProvider();
			AssertEquals("ShipmentType = 'WW'", A0131ATLASTypeOfShipment.Codes.WW, provider.ShipmentType);
			AssertContainsExactElementsInAnyOrder("Lines when ShipmentType = 'WW'", new[] { "1" }, provider.Lines.Select(x => x.SequenceNumber));
		});

		void CreateReportItem(CusExitConsignmentItem item, CusExitConsignmentPackage package)
		{
			var reportItem = report.CusExitReportItems.AddNew();
			reportItem.ERI_CCI_ConsignmentItem = item.PK;
			reportItem.ERI_CXP_Package = package.PK;
		}

		void CreatePackageLevelReport()
		{
			CreateReportItem(item1, package1);
			CreateReportItem(item2, package3);
		}

		void CreateConsignmentLevelReport()
		{
			CreateReportItem(item1, package1);
			CreateReportItem(item1, package2);
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
		CusExitReport report;
		CusExitHeader header;
		CusExitConsignment consignment;
		CusExitConsignmentItem item1;
		CusExitConsignmentItem item2;
		CusExitConsignmentPackage package1;
		CusExitConsignmentPackage package2;
		CusExitConsignmentPackage package3;
	}
}
