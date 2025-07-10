using System;
using System.Linq;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class IE044ConsignmentProviderTest : Customs.Business.Testing.DataProviderTestCase<IE044ConsignmentProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("NctsHeader missing", () => new IE044MessageProvider(null));
			});
		}

		protected override IE044ConsignmentProvider GetProvider() => new IE044ConsignmentProvider(nctsHeader);

		public void TestGrossMass()
		{
			arrivalMovementHeader.BM_GrossWeightUnloaded = 1.234;
			AssertEquals(1.234m, Provider.GrossMass);
		}

		public void TestTransportEquipments()
		{
			var transportEquipment1 = nctsHeader.ArrivalHeaderContainers.AddNew();
			transportEquipment1.BC_UnloadedState = "NEW";
			transportEquipment1.BC_ContainerNum = "CONT1";
			var transportEquipment2 = nctsHeader.ArrivalHeaderContainers.AddNew();
			transportEquipment2.BC_UnloadedState = "DIS";
			transportEquipment2.BC_ContainerNum = "CONT2";
			var transportEquipment3 = nctsHeader.ArrivalHeaderContainers.AddNew();
			transportEquipment3.BC_UnloadedState = "DEC";
			transportEquipment3.BC_ContainerNum = "CONT3";
			var transportEquipment4 = nctsHeader.ArrivalHeaderContainers.AddNew();
			transportEquipment4.BC_UnloadedState = "DEC";
			transportEquipment4.BC_ContainerNum = "CONT4";
			var seal1 = transportEquipment3.Seals.AddNew();
			seal1.BK_SealNumber = "S1";
			seal1.BK_UnloadingState = "DEC";
			var seal2 = transportEquipment4.Seals.AddNew();
			seal2.BK_SealNumber = "S2";
			seal2.BK_UnloadingState = "NEW";

			AssertEquals(3, Provider.TransportEquipments.Count);
			AssertEquals("DepartureTransportMeans - NEW", "CONT1", Provider.TransportEquipments.First().ContainerIdentificationNumber);
			AssertEquals("DepartureTransportMeans - DEC with seal NEW", "CONT4", Provider.TransportEquipments.Last().ContainerIdentificationNumber);
		}

		public void TestDepartureTransportMeans()
		{
			var transportMeans1 = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
			transportMeans1.TPM_IdentificationNumber = "ID001";
			transportMeans1.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
			var transportMeans2 = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
			transportMeans2.TPM_IdentificationNumber = "ID002";
			transportMeans2.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
			var transportMeans3 = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
			transportMeans3.TPM_IdentificationNumber = "ID003";
			transportMeans3.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;

			AssertEquals(2, Provider.DepartureTransportMeans.Count);
			AssertEquals("DepartureTransportMeans - NEW", "ID002", Provider.DepartureTransportMeans.First().IdentificationNumber);
			AssertEquals("DepartureTransportMeans - MIS", "ID003", Provider.DepartureTransportMeans.Last().IdentificationNumber);
		}

		public void TestSupportingDocuments()
		{
			var doc1 = arrivalMovementHeader.SupportingDocuments.AddNew();
			doc1.CSI_Code = CusSupportingInfoTypeList.Codes.AdditionalInfo;
			doc1.CSI_Status = NctsUnloadedStateList.Codes.NEW;
			doc1.CSI_ReferenceNumber = "REF111";
			doc1.CSI_ReferenceNumber2 = "Info 1";

			var doc2 = arrivalMovementHeader.SupportingDocuments.AddNew();
			doc2.CSI_Code = CusSupportingInfoTypeList.Codes.SupportingDocument;
			doc2.CSI_Status = NctsUnloadedStateList.Codes.MIS;
			doc2.CSI_ReferenceNumber = "REF222";
			doc2.CSI_ReferenceNumber2 = "Info 2";

			var doc3 = arrivalMovementHeader.SupportingDocuments.AddNew();
			doc3.CSI_Code = CusSupportingInfoTypeList.Codes.SupportingDocument;
			doc3.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			doc3.CSI_ReferenceNumber = "REF333";
			doc3.CSI_ReferenceNumber2 = "Info 3";

			var provider = GetProvider();
			AssertEquals("Supporting document count", 2, provider.SupportingDocuments.Count);

			AssertEquals("Doc 1 Type", "OTH", provider.SupportingDocuments.First().Type);
			AssertEquals("Doc 1 Reference", "REF111", provider.SupportingDocuments.First().Reference);
			AssertEquals("Doc 1 Information", "Info 1", provider.SupportingDocuments.First().ComplementOfInformation);

			AssertEquals("Doc 2 Type", "SUP", provider.SupportingDocuments.Last().Type);
			AssertEquals("Doc 2 Reference", "REF222", provider.SupportingDocuments.Last().Reference);
			AssertEquals("Doc 2 Information", "Info 2", provider.SupportingDocuments.Last().ComplementOfInformation);
		}

		public void TestTransportDocuments()
		{
			var doc1 = arrivalMovementHeader.AdditionalDocuments.AddNew();
			doc1.CSI_SubType = "TRA";
			doc1.CSI_Status = NctsUnloadedStateList.Codes.NEW;
			doc1.CSI_Code = "AAA";
			doc1.CSI_ReferenceNumber = "A1";

			var doc2 = arrivalMovementHeader.AdditionalDocuments.AddNew();
			doc2.CSI_SubType = "TRA";
			doc2.CSI_Status = NctsUnloadedStateList.Codes.MIS;
			doc2.CSI_Code = "BBB";
			doc2.CSI_ReferenceNumber = "B2";

			var doc3 = arrivalMovementHeader.AdditionalDocuments.AddNew();
			doc3.CSI_SubType = "TRA";
			doc3.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			doc3.CSI_Code = "CCC";
			doc3.CSI_ReferenceNumber = "B3";

			var provider = GetProvider();
			AssertEquals("Transport Document Count", 2, provider.TransportDocuments.Count);

			AssertEquals("Doc 1 Type", "AAA", provider.TransportDocuments.First().Type);
			AssertEquals("Doc 1 Reference", "A1", provider.TransportDocuments.First().Reference);

			AssertEquals("Doc 2 Type", "BBB", provider.TransportDocuments.Last().Type);
			AssertEquals("Doc 2 Reference", "B2", provider.TransportDocuments.Last().Reference);
		}

		public void TestAdditionalReferences()
		{
			var doc1 = arrivalMovementHeader.AdditionalDocuments.AddNew();
			doc1.CSI_Code = "INV";
			doc1.CSI_Status = NctsUnloadedStateList.Codes.NEW;
			doc1.CSI_ReferenceNumber = "1";

			var doc2 = arrivalMovementHeader.AdditionalDocuments.AddNew();
			doc2.CSI_Code = "OTH";
			doc2.CSI_Status = NctsUnloadedStateList.Codes.MIS;
			doc2.CSI_ReferenceNumber = "2";

			var doc3 = arrivalMovementHeader.AdditionalDocuments.AddNew();
			doc3.CSI_Code = "OTH";
			doc3.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			doc3.CSI_ReferenceNumber = "2";

			var provider = GetProvider();

			AssertEquals("Additional Reference Count", 2, provider.AdditionalReferences.Count);

			AssertEquals("Doc 1 Type", "INV", provider.AdditionalReferences.First().Type);
			AssertEquals("Doc 1 Reference", "1", provider.AdditionalReferences.First().Reference);

			AssertEquals("Doc 2 Type", "OTH", provider.AdditionalReferences.Last().Type);
			AssertEquals("Doc 2 Reference", "2", provider.AdditionalReferences.Last().Reference);
		}

		public void TestHouseConsignments()
		{
			Assert(true);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
		}

		NctsHeader nctsHeader;
		NctsArrivalMovementHeader arrivalMovementHeader;
	}
}
