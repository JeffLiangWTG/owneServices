using System;
using System.Linq;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	sealed class EXTINFLineProviderTest : Customs.Business.Testing.DataProviderTestCase<EXTINFLineProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXTINFLineProvider(null, ""));
		}

		public void TestReferenceNumberUCR() => CombineAssertions(() =>
		{
			consignmentItem.CCI_UniqueConsignmentReference = "UCR123";
			AssertEquals("ReferenceNumberUCR when InformationType != 'FW'", "UCR123", GetProvider().ReferenceNumberUCR);

			informationType = A0132ReportInformationType.Codes.FW;
			AssertEquals("ReferenceNumberUCR when InformationType = 'FW'", string.Empty, GetProvider().ReferenceNumberUCR);
		});

		public void TestRegistrationNumberExternal() => CombineAssertions(() =>
		{
			consignmentItem.CCI_ReferenceNumber = "REF123";
			AssertEquals("RegistrationNumberExternal when InformationType != 'FW'", "REF123", GetProvider().RegistrationNumberExternal);

			informationType = A0132ReportInformationType.Codes.FW;
			AssertEquals("RegistrationNumberExternal when InformationType = 'FW'", string.Empty, GetProvider().RegistrationNumberExternal);
		});

		public void TestActiveBorderTransportMeans() => CombineAssertions(() =>
		{
			report.CER_Location = "Sydney";
			informationType = A0132ReportInformationType.Codes.LW;
			AssertEquals("ActiveBorderTransportMeans when InformationType = 'LW'", "Sydney", GetProvider().ActiveBorderTransportMeans.Location);

			informationType = A0132ReportInformationType.Codes.UW;
			AssertEquals("ActiveBorderTransportMeans when InformationType = 'LW'", "Sydney", GetProvider().ActiveBorderTransportMeans.Location);

			informationType = A0132ReportInformationType.Codes.UP;
			AssertNull("ActiveBorderTransportMeans when InformationType != (LW, UW)", GetProvider().ActiveBorderTransportMeans);
		});

		public void TestCommodityGrossMass()
		{
			CombineAssertions(() =>
			{
				reportItem.ERI_GrossMass = 12.72572m;
				AssertEquals("12.726", GetProvider().CommodityGrossMass.ToString());
				reportItem.ERI_GrossMass = 12.72000m;
				AssertEquals("12.72", GetProvider().CommodityGrossMass.ToString());
				reportItem.ERI_GrossMass = 12.70000m;
				AssertEquals("12.7", GetProvider().CommodityGrossMass.ToString());
				reportItem.ERI_GrossMass = 12.00000m;
				AssertEquals("12", GetProvider().CommodityGrossMass.ToString());

				informationType = A0132ReportInformationType.Codes.FW;
				AssertEquals("CommodityGrossMass when InformationType = 'FW'", null, GetProvider().CommodityGrossMass);

				informationType = A0132ReportInformationType.Codes.LV;
				AssertEquals("CommodityGrossMass when InformationType = 'LV'", null, GetProvider().CommodityGrossMass);

				informationType = A0132ReportInformationType.Codes.UV;
				AssertEquals("CommodityGrossMass when InformationType = 'UV'", null, GetProvider().CommodityGrossMass);

				consignment.CXC_MovementReference = "";
				AssertEquals("CommodityGrossMass when CXC_MovementReference = ''", 12m, GetProvider().CommodityGrossMass);
			});
		}

		public void TestCommodityNetMass()
		{
			CombineAssertions(() =>
			{
				reportItem.ERI_NetMass = 4.82719m;
				AssertEquals("4.82719", GetProvider().CommodityNetMass.ToString());
				reportItem.ERI_NetMass = 4.82000m;
				AssertEquals("4.82", GetProvider().CommodityNetMass.ToString());
				reportItem.ERI_NetMass = 4.80000m;
				AssertEquals("4.8", GetProvider().CommodityNetMass.ToString());
				reportItem.ERI_NetMass = 4.00000m;
				AssertEquals("4", GetProvider().CommodityNetMass.ToString());
			});
		}

		public void TestPackaging() => CombineAssertions(() =>
		{
			var package1 = consignmentItem.CusExitConsignmentPackagePivots.AddNew().Package;
			package1.CXP_Sequence = 3;
			reportItem.ERI_CXP_Package = package1.PK;
			var package2 = consignmentItem.CusExitConsignmentPackagePivots.AddNew().Package;
			package2.CXP_Sequence = 4;
			var reportItem2 = report.CusExitReportItems.AddNew();
			reportItem2.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			reportItem2.ERI_CXP_Package = package2.PK;

			var consignmentItem2 = consignment.CusExitConsignmentItems.AddNew();
			var package3 = consignmentItem2.CusExitConsignmentPackagePivots.AddNew().Package;
			package3.CXP_Sequence = 1;
			var reportItem3 = report.CusExitReportItems.AddNew();
			reportItem3.ERI_CCI_ConsignmentItem = consignmentItem2.PK;
			reportItem3.ERI_CXP_Package = package3.PK;

			informationType = A0132ReportInformationType.Codes.FP;
			AssertContainsExactElementsInAnyOrder("Packaging when InformationType = 'FP'", new[] { 3, 4 }, GetProvider().Packaging.Select(x => x.PositionNumber));

			informationType = A0132ReportInformationType.Codes.LP;
			AssertContainsExactElementsInAnyOrder("Packaging when InformationType = 'LP'", new[] { 3, 4 }, GetProvider().Packaging.Select(x => x.PositionNumber));

			informationType = A0132ReportInformationType.Codes.UP;
			AssertContainsExactElementsInAnyOrder("Packaging when InformationType = 'UP'", new[] { 3, 4 }, GetProvider().Packaging.Select(x => x.PositionNumber));

			informationType = A0132ReportInformationType.Codes.UV;
			AssertEquals("Packaging when InformationType != (FP, LP, UP)", 0, GetProvider().Packaging.Count);
		});

		protected override EXTINFLineProvider GetProvider() => new EXTINFLineProvider(reportItem, informationType);

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<CusExitHeader>();
			consignment = header.CusExitConsignments.AddNew();
			consignment.CXC_MovementReference = "123";
			consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			reportItem = report.CusExitReportItems.AddNew();
			reportItem.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			informationType = "";
		}
		CusExitConsignment consignment;
		CusExitConsignmentItem consignmentItem;
		CusExitReport report;
		CusExitReportItem reportItem;
		string informationType;
	}
}
