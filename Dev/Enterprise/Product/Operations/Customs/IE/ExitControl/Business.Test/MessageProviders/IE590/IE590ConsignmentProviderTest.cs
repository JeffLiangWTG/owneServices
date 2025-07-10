using Enterprise.Customs.Common.EU;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.ExitControl.Business.AES.Testing
{
	sealed class IE590ConsignmentProviderTest : Customs.Business.Testing.DataProviderTestCase<IE590ConsignmentProvider>
	{
		public void TestExitCarrier()
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
			var provider = new IE590ConsignmentProvider(report, header);
			CombineAssertions(() =>
			{
				AssertEquals("No Eori", null, provider.ExitCarrier);

				carrier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "002", "ES");
				provider = new IE590ConsignmentProvider(report, header);
				var exitCarrier = provider.ExitCarrier;
				AssertEquals("ID", "ES002", exitCarrier.Id);
				AssertEquals("Contact", "Test Contact 2", exitCarrier.Contact.Name);
				AssertSame("ExitCarrier Cached", exitCarrier, provider.ExitCarrier);
			});
		}

		public void TestContainerNumbers()
		{
			var container1 = header.CusExitContainers.AddNew();
			container1.CXN_IsEquipment = true;
			container1.CXN_ContainerNumber = "001";
			var container2 = header.CusExitContainers.AddNew();
			container2.CXN_ContainerNumber = "002";
			var container3 = header.CusExitContainers.AddNew();
			container3.CXN_ContainerNumber = "003";

			var pivot1 = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			pivot1.CNP_CXN_Container = container1.PK;
			var package1 = pivot1.Package;

			var consignment2 = header.CusExitConsignments.AddNew();
			var consignmentItem2 = consignment2.CusExitConsignmentItems.AddNew();
			var pivot2 = consignmentItem2.CusExitConsignmentPackagePivots.AddNew();
			pivot2.CNP_CXN_Container = container2.PK;
			var package2 = pivot2.Package;

			var item1 = report.CusExitReportItems.AddNew();
			item1.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			item1.ERI_CXP_Package = package1.PK;
			var item2 = report.CusExitReportItems.AddNew();
			item2.ERI_CCI_ConsignmentItem = consignmentItem2.PK;
			item2.ERI_CXP_Package = package2.PK;

			var provider = new IE590ConsignmentProvider(report, header);
			CombineAssertions(() =>
			{
				AssertEquals("CER_Behavior Not DIS", 0, provider.ContainerNumbers.Count);

				report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
				provider = new IE590ConsignmentProvider(report, header);
				var containers = provider.ContainerNumbers;
				AssertContainsExactElementsInExactOrder("CER_Behavior is DIS. container1 is Equipment and container3 not linked to report, so only container2 selected out", new[] { "002" }, containers);
			});
		}

		protected override IE590ConsignmentProvider GetProvider() => new IE590ConsignmentProvider(report, header);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusExitHeader>();
			consignment = header.CusExitConsignments.AddNew();
			consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem.CCI_LineNumber = 10;
			report = header.CusExitReports.AddNew();
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
