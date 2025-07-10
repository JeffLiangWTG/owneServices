using System;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.ExitControl.Business.AES.Testing
{
	sealed class IE590MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<IE590MessageProvider>
	{
		public void TestExportOperation()
		{
			var exportOperation = Provider.ExportOperation;
			AssertNotNull("ExportOperation", exportOperation);
		}

		public void TestCustomsOfficeOfExitActual()
		{
			report.CER_OfficeOfExit = "OFF";
			var provider = new IE590MessageProvider(report);
			AssertEquals("CustomsOfficeOfExitActual", "OFF", provider.CustomsOfficeOfExitActual);
		}

		public void TestPassageExitDate()
		{
			var provider = new IE590MessageProvider(report);
			AssertEquals("PassageExitDate", DateTime.MinValue, provider.PassageExitDate);

			report.CER_DateTime = ZDateTimeOffset.Today;
			provider = new IE590MessageProvider(report);
			AssertEquals("PassageExitDate", report.CER_DateTime.ToDateTime(), provider.PassageExitDate);
		}

		public void TestPersonConfirmingExit()
		{
			var personConfirmingExit = Provider.PersonConfirmingExit;
			AssertNotNull("PersonConfirmingExit", personConfirmingExit);
		}

		public void TestGoodsShipment()
		{
			var goodsShipment = Provider.GoodsShipment;
			AssertNotNull("GoodsShipment", goodsShipment);
		}

		public void TestAdditionalDeclarationType()
		{
			report.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			AssertEquals("AdditionalDeclarationType", "A", Provider.AdditionalDeclarationType);
		}

		public void TestManifestNumber()
		{
			AssertEquals("ManifestNumber", null, Provider.ManifestNumber);
		}

		public void TestDiscrepanciesExistAtExit()
		{
			CombineAssertions(() =>
			{
				var provider = new IE590MessageProvider(report);
				AssertEquals("CER_Behavior != DIS", false, provider.DiscrepanciesExistAtExit);

				report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
				provider = new IE590MessageProvider(report);
				AssertEquals("CER_Behavior == DIS", true, provider.DiscrepanciesExistAtExit);
			});
		}

		public void TestMRN()
		{
			consignment.CXC_MovementReference = "REF1";

			var provider = new IE590MessageProvider(report);
			AssertEquals("When has consignment", "REF1", provider.MRN);

			report.CER_CXC_Consignment = ZGuid.Empty;
			AssertEquals("When no consignment", ZString.Empty, provider.MRN);
		}

		public void TestConsignment()
		{
			AssertNotNull("Consignment", Provider.Consignment);
			AssertType<IE590ConsignmentProvider>(Provider.Consignment);
		}

		public void TestGoodsItems()
		{
			var pivot1 = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var package1 = pivot1.Package;
			package1.CXP_PackageType = "P1";
			package1.CXP_Quantity = 10;
			package1.CXP_MarksAndNumbers = "MARK1";
			var pivot2 = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var package2 = pivot2.Package;
			package2.CXP_PackageType = "P2";
			package2.CXP_Quantity = 20;
			package2.CXP_MarksAndNumbers = "MARK2";

			consignmentItem.CCI_LineNumber = 1;
			var consignmentItem2 = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem2.CCI_LineNumber = 2;
			var pivot3 = consignmentItem2.CusExitConsignmentPackagePivots.AddNew();
			var package3 = pivot3.Package;
			package3.CXP_PackageType = "P2";
			package3.CXP_Quantity = 20;
			package3.CXP_MarksAndNumbers = "MARK2";

			var exitReportItem1 = report.CusExitReportItems.AddNew();
			exitReportItem1.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			exitReportItem1.ERI_NetMass = 10m;
			exitReportItem1.ERI_GrossMass = 70m;
			exitReportItem1.ERI_Quantity = 3;
			var exitReportItem2 = report.CusExitReportItems.AddNew();
			exitReportItem2.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			exitReportItem2.ERI_NetMass = 10m;
			exitReportItem2.ERI_GrossMass = 70m;
			exitReportItem2.ERI_Quantity = 4;
			var exitReportItem3 = report.CusExitReportItems.AddNew();
			exitReportItem3.ERI_CCI_ConsignmentItem = consignmentItem2.PK;
			exitReportItem3.ERI_NetMass = 4m;
			exitReportItem3.ERI_GrossMass = 20m;
			exitReportItem3.ERI_Quantity = 5;

			var provider = new IE590MessageProvider(report);
			CombineAssertions(() =>
			{
				AssertEquals("No package linked to the report", 0, provider.GoodsItems.Count);

				exitReportItem1.ERI_CXP_Package = package1.PK;
				provider = new IE590MessageProvider(report);
				var goodsItems = provider.GoodsItems.ToArray();
				AssertEquals("One package linked to the report", 1, goodsItems.Length);
				AssertGoodsItems(goodsItems[0], 1, 10m, 70m, new[] { ("P1", "3", "MARK1") });

				exitReportItem2.ERI_CXP_Package = package2.PK;
				provider = new IE590MessageProvider(report);
				goodsItems = provider.GoodsItems.ToArray();
				AssertEquals("two package linked to the same item of the report", 1, goodsItems.Length);
				AssertGoodsItems(goodsItems[0], 1, 10m, 70m, new[] { ("P1", "3", "MARK1"), ("P2", "4", "MARK2") });

				exitReportItem3.ERI_CXP_Package = package3.PK;
				provider = new IE590MessageProvider(report);
				goodsItems = provider.GoodsItems.ToArray();
				AssertEquals("three package linked to two different item of the report", 2, goodsItems.Length);
				AssertGoodsItems(goodsItems[0], 1, 10m, 70m, new[] { ("P1", "3", "MARK1"), ("P2", "4", "MARK2") });
				AssertGoodsItems(goodsItems[1], 2, 4m, 20m, new[] { ("P2", "5", "MARK2") });
			});

			void AssertGoodsItems(IIE507And590CommonGoodsItem goodItem, short expectedGoodsItemNumber, ZDecimal expectedNetMass, ZDecimal expectedGrossMass, (string, string, string)[] expectedPackageData)
			{
				AssertEquals("GoodsItemNumber", expectedGoodsItemNumber, goodItem.GoodsItemNumber);
				AssertEquals("GrossMass", expectedGrossMass, goodItem.GrossMass);
				AssertEquals("NetMass", expectedNetMass, goodItem.NetMass);
				AssertContainsExactElementsInAnyOrder("Packages", expectedPackageData, goodItem.Packages.Select(x => (x.PackageType, x.PackageQuantity.ToString(), x.ShippingMarks)));
			}
		}

		public void TestRole()
		{
			AssertEquals("Role", "1", Provider.Role);
		}

		public void TestIdentificationNumber()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "CarrierName";
			var carrierAddress = carrier.MainAddress;
			carrierAddress.OA_City = "TestCity";

			var orgContact = Factory.New<OrgContact>();
			orgContact.OC_OH = carrier.PK;
			orgContact.OC_OA_OrgAddress = carrierAddress.PK;
			orgContact.OC_ContactName = "Test Contact 2";

			header.CXH_OA_Carrier = carrierAddress.PK;

			CombineAssertions(() =>
			{
				var provider = new IE590MessageProvider(report);
				AssertEquals("EORI not exist", "", provider.IdentificationNumber);

				carrier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "002", "ES");
				provider = new IE590MessageProvider(report);
				AssertEquals("EORI exist", "ES002", provider.IdentificationNumber);
			});
		}

		public void TestReference()
		{
			AssertEquals("Reference", null, Provider.Reference);
		}

		protected override IE590MessageProvider GetProvider() => new IE590MessageProvider(report);

		protected override void SetUp()
		{
			base.SetUp();
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
