using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	sealed class ReportManagerTest : TestCaseWithFactory
	{
		public void TestSetupReportData()
		{
			(var consignment, var header) = CusExitConsignmentTest.GetNewBusinessObject(Factory);
			var consignmentItem1 = consignment.CusExitConsignmentItems.AddNew();
			var consignmentItem2 = consignment.CusExitConsignmentItems.AddNew();

			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			var reportItem1 = report.CusExitReportItems.AddNew();

			var consignmentPackagePivot1 = consignmentItem1.CusExitConsignmentPackagePivots.AddNew();
			var consignmentPackagePivot2 = consignmentItem2.CusExitConsignmentPackagePivots.AddNew();
			var package1 = consignmentPackagePivot1.Package;
			var reportItem2 = report.CusExitReportItems.AddNew();
			reportItem2.ERI_CXP_Package = package1.PK;
			var package2 = consignmentPackagePivot2.Package;
			var reportItem3 = report.CusExitReportItems.AddNew();
			reportItem3.ERI_CXP_Package = package2.PK;

			var refreshBindingCalled_consignmentItem1 = 0;
			var refreshBindingCalled_consignmentItem2 = 0;
			var refreshBindingCalled_package1 = 0;
			var refreshBindingCalled_package2 = 0;
			consignmentItem1.CCI_Calc_ShouldReportItemInfo.ValueChanged += (s, e) => refreshBindingCalled_consignmentItem1++;
			consignmentItem2.CCI_Calc_ShouldReportItemInfo.ValueChanged += (s, e) => refreshBindingCalled_consignmentItem2++;
			package1.CXP_Calc_ShouldReportItemInfo.ValueChanged += (s, e) => refreshBindingCalled_package1++;
			package2.CXP_Calc_ShouldReportItemInfo.ValueChanged += (s, e) => refreshBindingCalled_package2++;
			using (ReportManager.SetupReportData(consignment, report))
			{
				CombineAssertions(() =>
				{
					AssertEquals(1, refreshBindingCalled_consignmentItem1);
					AssertEquals(1, refreshBindingCalled_consignmentItem2);
					AssertEquals(1, refreshBindingCalled_package1);
					AssertEquals(1, refreshBindingCalled_package2);
				});
			}
		}

		public void TestSetupReportDataClearNotificationOnDisposed()
		{
			(var consignment, var header) = CusExitConsignmentTest.GetNewBusinessObject(Factory);
			var consignmentItem1 = consignment.CusExitConsignmentItems.AddNew();
			var consignmentItem2 = consignment.CusExitConsignmentItems.AddNew();

			var consignmentPackagePivot1 = consignmentItem1.CusExitConsignmentPackagePivots.AddNew();
			var consignmentPackagePivot2 = consignmentItem2.CusExitConsignmentPackagePivots.AddNew();
			var package1 = consignmentPackagePivot1.Package;
			var package2 = consignmentPackagePivot2.Package;

			using (ReportManager.SetupReportData(consignment, null))
			{
				CombineAssertions("While setup", () =>
				{
					consignmentItem1.CCI_Calc_ShouldReportItem = true;
					consignmentItem1.CCI_Calc_ReportGrossMass = ZDecimal.Zero;
					consignmentItem1.CCI_Calc_ReportNetMass = ZDecimal.Zero;
					AssertHasMessageErrors(consignmentItem1.CCI_Calc_ReportGrossMassInfo);
					AssertHasMessageErrors(consignmentItem1.CCI_Calc_ReportNetMassInfo);
					package1.CXP_Calc_ShouldReportItem = true;
					package1.CXP_Calc_ReportQuantity = ZInt.Zero;
					AssertHasMessageErrors(package1.CXP_Calc_ReportQuantityInfo);
					consignmentItem2.CCI_Calc_ShouldReportItem = true;
					consignmentItem2.CCI_Calc_ReportGrossMass = ZDecimal.Zero;
					consignmentItem2.CCI_Calc_ReportNetMass = ZDecimal.Zero;
					AssertHasMessageErrors(consignmentItem2.CCI_Calc_ReportGrossMassInfo);
					AssertHasMessageErrors(consignmentItem2.CCI_Calc_ReportNetMassInfo);
					package2.CXP_Calc_ShouldReportItem = true;
					package2.CXP_Calc_ReportQuantity = ZInt.Zero;
					AssertHasMessageErrors(package2.CXP_Calc_ReportQuantityInfo);
				});
			}
			CombineAssertions("after setup", () =>
			{
				AssertNoMessageErrors(consignmentItem1.CCI_Calc_ReportGrossMassInfo);
				AssertNoMessageErrors(consignmentItem1.CCI_Calc_ReportNetMassInfo);
				AssertNoMessageErrors(package1.CXP_Calc_ReportQuantityInfo);
				AssertNoMessageErrors(consignmentItem2.CCI_Calc_ReportGrossMassInfo);
				AssertNoMessageErrors(consignmentItem2.CCI_Calc_ReportNetMassInfo);
				AssertNoMessageErrors(package2.CXP_Calc_ReportQuantityInfo);
			});
		}

		public void TestCreateOrUpdateReport()
		{
			(var consignment, var header) = CusExitConsignmentTest.GetNewBusinessObject(Factory);
			var consignmentItem1 = consignment.CusExitConsignmentItems.AddNew();
			var consignmentItem2 = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem1.CCI_Calc_ShouldReportItem = true;
			consignmentItem1.CCI_Calc_ReportGrossMass = 100m;
			consignmentItem1.CCI_Calc_ReportNetMass = 50m;
			consignmentItem2.CCI_Calc_ShouldReportItem = true;
			consignmentItem2.CCI_Calc_ReportGrossMass = 600m;
			consignmentItem2.CCI_Calc_ReportNetMass = 400m;

			var consignmentPackagePivot1 = consignmentItem1.CusExitConsignmentPackagePivots.AddNew();
			var consignmentPackagePivot2 = consignmentItem1.CusExitConsignmentPackagePivots.AddNew();
			var package1 = consignmentPackagePivot1.Package;
			package1.CXP_Calc_ShouldReportItem = true;
			package1.CXP_Quantity = 3;
			var package2 = consignmentPackagePivot2.Package;
			package2.CXP_Calc_ShouldReportItem = true;
			package2.CXP_Quantity = 9;

			var consignmentPackagePivot3 = consignmentItem2.CusExitConsignmentPackagePivots.AddNew();
			var package3 = consignmentPackagePivot3.Package;
			package3.CXP_Calc_ShouldReportItem = true;
			package3.CXP_Quantity = 12;
			AssertEquals("PreCondition", 0, header.CusExitReports.Count);
			ReportManager.CreateOrUpdateReport(consignment, null);
			var report = header.CusExitReports[0];
			var reportPK = report.PK;
			var reportItem1 = package1.CusExitReportItems[0];
			var reportItem2 = package2.CusExitReportItems[0];
			var reportItem3 = package3.CusExitReportItems[0];
			CombineAssertions(() =>
			{
				AssertEquals(1, header.CusExitReports.Count);
				AssertEquals(3, report.CusExitReportItems.Count);
				AssertEquals(consignmentItem1.PK, reportItem1.ERI_CCI_ConsignmentItem);
				AssertEquals(package1.PK, reportItem1.ERI_CXP_Package);
				AssertEquals(100m, reportItem1.ERI_GrossMass);
				AssertEquals(50m, reportItem1.ERI_NetMass);
				AssertEquals(3, reportItem1.ERI_Quantity);
				AssertSame(package1.FirstReportItem, reportItem1);

				AssertEquals(package2.PK, reportItem2.ERI_CXP_Package);
				AssertEquals(100m, reportItem2.ERI_GrossMass);
				AssertEquals(50m, reportItem2.ERI_NetMass);
				AssertEquals(9, reportItem2.ERI_Quantity);
				AssertSame(package2.FirstReportItem, reportItem2);

				AssertEquals(package3.PK, reportItem3.ERI_CXP_Package);
				AssertEquals(600m, reportItem3.ERI_GrossMass);
				AssertEquals(400m, reportItem3.ERI_NetMass);
				AssertEquals(12, reportItem3.ERI_Quantity);
				AssertSame(package3.FirstReportItem, reportItem3);
			});

			consignmentItem1.CCI_Calc_ReportGrossMass = 50m;
			consignmentItem1.CCI_Calc_ReportNetMass = 25m;
			package1.CXP_Calc_ReportQuantity = 2;
			ReportManager.CreateOrUpdateReport(consignment, report);
			CombineAssertions(() =>
			{
				AssertEquals(1, header.CusExitReports.Count);
				AssertEquals(50m, reportItem1.ERI_GrossMass);
				AssertEquals(25m, reportItem1.ERI_NetMass);
				AssertEquals(2, reportItem1.ERI_Quantity);
				AssertSame(package1.FirstReportItem, reportItem1);
			});

			CombineAssertions(() =>
			{
				package1.CXP_Calc_ShouldReportItem = false;
				ReportManager.CreateOrUpdateReport(consignment, report);
				AssertEquals(0, package1.GetMatchingExitReportItems().Count());

				package1.CXP_Calc_ShouldReportItem = true;
				ReportManager.CreateOrUpdateReport(consignment, report);
				AssertEquals(1, package1.GetMatchingExitReportItems().Count());

				AssertEquals(2, consignmentItem1.GetMatchingExitReportItems().Count());
				AssertEquals(1, consignmentItem2.GetMatchingExitReportItems().Count());

				consignmentItem1.CCI_Calc_ShouldReportItem = false;
				ReportManager.CreateOrUpdateReport(consignment, report);
				AssertEquals(0, consignmentItem1.GetMatchingExitReportItems().Count());
				AssertEquals(0, package1.GetMatchingExitReportItems().Count());
				AssertEquals(0, package2.GetMatchingExitReportItems().Count());
				AssertEquals(1, consignmentItem2.GetMatchingExitReportItems().Count());
			});
		}

		public void TestCreateOrUpdateReport_SetCER_Behavior()
		{
			var (consignment, header) = CusExitConsignmentTest.GetNewBusinessObject(Factory);
			var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			var report = header.CusExitReports.AddNew();
			ReportManager.CreateOrUpdateReport(consignment, report);
			AssertEquals("No consignment item selected", CusExitReportBehaviorList.Codes.STD, report.CER_Behavior);

			consignmentItem.CCI_Calc_ShouldReportItem = true;
			ReportManager.CreateOrUpdateReport(consignment, report);
			AssertEquals("Has consignment item selected", CusExitReportBehaviorList.Codes.DIS, report.CER_Behavior);
		}

		public void TestReportDataSetWhenCreateOrUpdateReportInvoked()
		{
			var (consignment, header) = CusExitConsignmentTest.GetNewBusinessObject(Factory);

			var report = Factory.NewMoq<CusExitReport>();
			report.CallBase = true;

			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			header.CXH_ParentID = jobDeclaration.PK;
			header.CXH_ParentTableCode = jobDeclaration.TablePrefix;

			var protectedMock = report.Protected();
			protectedMock.Setup("DefaultDataFromDeclaration", true, jobDeclaration).Verifiable();
			protectedMock.Setup("SetReportBehaviorFromConsignmentItemsCore", true).Verifiable();
			header.CusExitReports.Add(report.Object);

			ReportManager.CreateOrUpdateReport(consignment, report.Object);
			protectedMock.Verify("DefaultDataFromDeclaration", Times.Once(), jobDeclaration);
			protectedMock.Verify("SetReportBehaviorFromConsignmentItemsCore", Times.Once());

			Assert("CreateOrUpdateReport() should call both report.SetReportBehaviorFromConsignmentItems() and report.DefaultDataFromDeclaration()", true);
		}

		public void TestCreateOrUpdateReport_ShipmentIsNotNull()
		{
			var (consignment, header) = CusExitConsignmentTest.GetNewBusinessObject(Factory);

			var report = Factory.NewMoq<CusExitReport>();
			report.CallBase = true;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			header.Parent = shipment;

			var protectedMock = report.Protected();
			protectedMock.Setup("DefaultDataFromShipment", true, shipment).Verifiable();
			header.CusExitReports.Add(report.Object);

			ReportManager.CreateOrUpdateReport(consignment, report.Object);
			protectedMock.Verify("DefaultDataFromShipment", Times.Once(), shipment);

			Assert("CreateOrUpdateReport() should call report.DefaultDataFromShipment()", true);
		}
	}
}
