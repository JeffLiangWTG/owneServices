using System;
using Enterprise.Customs.DE.ExitControl.Business.Testing;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	sealed class EXTNOTLineProviderTest : Customs.Business.Testing.DataProviderTestCase<EXTNOTLineProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXTNOTLineProvider(null, ""));
		}

		public void TestPackages() => CombineAssertions(() =>
		{
			shipmentType = A0131ATLASTypeOfShipment.Codes.AP;
			AssertEquals("Packaging when ShipmentType = 'AP'", 2, GetProvider().Packages.Count);

			shipmentType = A0131ATLASTypeOfShipment.Codes.FP;
			AssertEquals("Packaging when ShipmentType = 'FP'", 2, GetProvider().Packages.Count);

			shipmentType = A0131ATLASTypeOfShipment.Codes.FW;
			AssertEquals("Packaging when ShipmentType != (AP, FP)", 0, GetProvider().Packages.Count);
		});

		protected override EXTNOTLineProvider GetProvider() => new EXTNOTLineProvider(reportItem1, shipmentType);

		protected override void SetUp()
		{
			base.SetUp();
			shipmentType = "";
			var consignmentItem1 = Factory.New<CusExitConsignmentItem>();
			var consignmentItem2 = Factory.New<CusExitConsignmentItem>();
			var (report, _) = CusExitReportTest.GetNewBusinessObject(Factory);
			reportItem1 = report.CusExitReportItems.AddNew();
			reportItem1.ERI_CCI_ConsignmentItem = consignmentItem1.PK;
			reportItem1.ERI_CXP_Package = Factory.New<CusExitConsignmentPackage>().PK;
			var reportItem2 = report.CusExitReportItems.AddNew();
			reportItem2.ERI_CCI_ConsignmentItem = consignmentItem1.PK;
			reportItem2.ERI_CXP_Package = Factory.New<CusExitConsignmentPackage>().PK;
			var reportItem3 = report.CusExitReportItems.AddNew();
			reportItem3.ERI_CCI_ConsignmentItem = consignmentItem2.PK;
			reportItem3.ERI_CXP_Package = Factory.New<CusExitConsignmentPackage>().PK;
		}
		CusExitReportItem reportItem1;
		string shipmentType;
	}
}
