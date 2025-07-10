using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.ExitControl.Business.AES.Testing
{
	sealed class IE507ConsignmentProviderTest : Customs.Business.Testing.DataProviderTestCase<IE507ConsignmentProvider, IIE507Consignment>
	{
		public void TestBorderModeOfTransport()
		{
			report.CER_TransportMode = Customs.Business.TransportTypeList.Codes.Air;
			AssertEquals("BorderModeOfTransport", TransportModeCodeList.Codes.Air, IProvider.BorderModeOfTransport);
			report.CER_TransportMode = Customs.Business.TransportTypeList.Codes.Road;
			AssertEquals("BorderModeOfTransport", TransportModeCodeList.Codes.Road, IProvider.BorderModeOfTransport);
		}

		public void TestUCR()
		{
			report.CER_CXC_Consignment = consignment.PK;
			consignment.CXC_UniqueConsignmentReference = "UCR1";
			AssertEquals("UCR", "UCR1", IProvider.UCR);
		}

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

			CombineAssertions(() =>
			{
				var exitCarrier = IProvider.ExitCarrier;
				AssertEquals("Name", "CarrierName", exitCarrier.Name);
				AssertEquals("CityName", "TestCity", exitCarrier.Address.City);
				AssertEquals("Contact", "Test Contact 2", exitCarrier.Contact.Name);
			});
		}

		public void TestTransportEquipments_Container()
		{
			var container1 = header.CusExitContainers.AddNew();
			container1.CXN_ContainerNumber = "ABC123";
			var container2 = header.CusExitContainers.AddNew();
			container2.CXN_ContainerNumber = "ABC456";
			var container3 = header.CusExitContainers.AddNew();
			container3.CXN_ContainerNumber = "ABC789";
			var pivot1 = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			pivot1.CNP_CXN_Container = container1.PK;
			var package1 = pivot1.Package;
			var pivot2 = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			pivot2.CNP_CXN_Container = container3.PK;
			var package2 = pivot2.Package;
			var pivot3 = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			pivot3.CNP_CXN_Container = container2.PK;
			var package3 = pivot3.Package;
			reportItem.ERI_CXP_Package = package1.PK;
			var reportItem2 = report.CusExitReportItems.AddNew();
			reportItem2.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			reportItem2.ERI_CXP_Package = package2.PK;
			var report2 = header.CusExitReports.AddNew();
			var report2Item1 = report2.CusExitReportItems.AddNew();
			report2Item1.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			report2Item1.ERI_CXP_Package = package3.PK;

			var consignmentItem2 = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem2.CCI_LineNumber = 20;
			var pivot4 = consignmentItem2.CusExitConsignmentPackagePivots.AddNew();
			pivot4.CNP_CXN_Container = container3.PK;
			var package4 = pivot4.Package;

			var reportItem3 = report.CusExitReportItems.AddNew();
			reportItem3.ERI_CCI_ConsignmentItem = consignmentItem2.PK;
			reportItem3.ERI_CXP_Package = package4.PK;

			var transportEquipments = IProvider.TransportEquipments.ToArray();
			AssertEquals("transportEquipments.Length", 2, transportEquipments.Length);
			var transportEquipment1 = transportEquipments[0];
			var transportEquipment2 = transportEquipments[1];
			if (transportEquipment2.ContainerIdentificationNumber == "ABC123")
			{
				transportEquipment1 = transportEquipments[1];
				transportEquipment2 = transportEquipments[0];
			}
			CombineAssertions("transportEquipment1", () => AssertTransportEquipment(transportEquipment1, "ABC123", new[] { "10" }));
			CombineAssertions("transportEquipment2", () => AssertTransportEquipment(transportEquipment2, "ABC789", new[] { "10", "20" }));
		}

		public void TestTransportEquipments_Equipment()
		{
			var equipment1 = header.CusExitContainers.AddNew();
			equipment1.CXN_IsEquipment = true;
			equipment1.CXN_ContainerNumber = "ABC";
			var equipment2 = header.CusExitContainers.AddNew();
			equipment2.CXN_ContainerNumber = "XYZ";
			equipment2.CXN_IsEquipment = true;
			var additionalSeal = equipment2.AllSealNumbers.AddNew();
			additionalSeal.BK_SealNumber = "TE2AddSeal";
			var pivot1 = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			pivot1.CNP_CXN_Container = equipment1.PK;
			var package1 = pivot1.Package;
			var pivot2 = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			pivot2.CNP_CXN_Container = equipment2.PK;
			var package2 = pivot2.Package;
			reportItem.ERI_CXP_Package = package1.PK;
			var reportItem2 = report.CusExitReportItems.AddNew();
			reportItem2.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			reportItem2.ERI_CXP_Package = package2.PK;
			var report2 = header.CusExitReports.AddNew();
			var report2Item1 = report2.CusExitReportItems.AddNew();
			report2Item1.ERI_CCI_ConsignmentItem = consignmentItem.PK;

			var transportEquipments = IProvider.TransportEquipments.ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("Equipment with no seals is not included", 1, transportEquipments.Length);
				var transportEquipment = transportEquipments[0];
				AssertEquals("Container Identifier not included", string.Empty, transportEquipment.ContainerIdentificationNumber);
				AssertEquals("Seals Count", 1, transportEquipment.Seals.Count);
				AssertContainsExactElementsInAnyOrder("Seals", new[] { "TE2AddSeal" }, transportEquipment.Seals);
			});
		}

		void AssertTransportEquipment(ITransportEquipmentWithSeals transportEquipment, string containerNumber, string[] goodsReferences)
		{
			AssertEquals("ContainerIdentificationNumber", containerNumber, transportEquipment.ContainerIdentificationNumber);
			AssertArrayEqualsByElements("GoodsReference", goodsReferences, transportEquipment.GoodsReferences.ToArray());
		}

		public void TestLocationOfGoods()
		{
			report.GoodsLocation.CGL_AdditionalIdentifier = "AI123";
			AssertType<LocationOfGoodsProvider>(IProvider.LocationOfGoods);
		}

		public void TestTransportDocuments()
		{
			var reportItem = report.CusExitReportItems.AddNew();
			var additionalInfo = reportItem.AdditionalInfos.AddNew();
			additionalInfo.CSI_Code = "1Q12";
			additionalInfo.CSI_SubType = "TRA";
			additionalInfo.CSI_ReferenceNumber = "TRANSDOC123";
			var transportDocument = IProvider.TransportDocuments.Single();
			AssertEquals("transportDocument.Type", "1Q12", transportDocument.Type);
			AssertEquals("transportDocument.Reference", "TRANSDOC123", transportDocument.Reference);
		}

		public void TestActiveBorderTransportMeans()
		{
			report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
			report.CER_TransportID = "ABC";
			report.CER_TransportType = Customs.Business.TransportTypeList.Codes.Air;
			report.CER_RN_NKTransportNationality = "IE";

			var transportMeans = IProvider.ActiveBorderTransportMeans;
			AssertEquals("Transport ID set correctly", "ABC", transportMeans.IdentificationNumber);
			AssertEquals("Transport type set correctly", Customs.Business.TransportTypeList.Codes.Air, transportMeans.TypeOfIdentification);
			AssertEquals("Transport nationality set correctly", "IE", transportMeans.Nationality);
		}

		protected override IE507ConsignmentProvider GetProvider()
		{
			return new IE507ConsignmentProvider(report, header);
		}

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
