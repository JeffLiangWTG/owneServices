using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	class EALAESTransportEquipmentWrapperTest : WrapperHelperTest<EALAESTransportEquipmentWrapper>
	{
		public void TestGetTransportEquipmentList_NullExitHeader()
		{
			var wrapperList = EALAESTransportEquipmentWrapper.GetTransportEquipmentList(Factory.New<CusExitReport>());
			AssertEquals("Expected empty wrapper list", 0, wrapperList.Count);
		}

		public void TestGetTransportEquipmentList_NoDiscrepancies()
		{
			exitReport.CER_Calc_Discrepancies = ZBool.False;
			var wrapperList = EALAESTransportEquipmentWrapper.GetTransportEquipmentList(exitReport);
			AssertEquals("Expected empty wrapper list", 0, wrapperList.Count);
		}

		public void TestGetTransportEquipmentList_NoContainers()
		{
			exitHeader.CusExitContainers.DeleteAll();
			var wrapperList = EALAESTransportEquipmentWrapper.GetTransportEquipmentList(exitReport);
			AssertEquals("Expected empty wrapper list", 0, wrapperList.Count);
		}

		public void TestGetTransportEquipmentList()
		{
			var consignment = exitHeader.CusExitConsignments.AddNew();

			var consignmentItem1 = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem1.CCI_LineNumber = 1;
			consignmentItem1.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;

			var consignmentItem2 = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem2.CCI_LineNumber = 2;
			consignmentItem2.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;

			var consignmentItem3 = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem3.CCI_LineNumber = 3;
			consignmentItem3.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;

			var consignmentItem4 = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem4.CCI_LineNumber = 4;
			consignmentItem4.CCI_DiscrepancyStatus = ZString.Empty;

			AddPackageToConsignmentItem(consignmentItem1, container.PK);

			var container2 = exitHeader.CusExitContainers.AddNew();
			container2.CXN_Sequence = 3;
			container2.CXN_ContainerNumber = "CONT2";
			container2.CXN_SealCount = 2;
			container2.CXN_Status = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			var seal21 = container2.AllSealNumbers.AddNew();
			seal21.BK_UnloadingState = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			seal21.BK_SequenceNumber = 1;
			seal21.BK_SealNumber = "SEAL21";
			var seal22 = container2.AllSealNumbers.AddNew();
			seal22.BK_UnloadingState = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			seal22.BK_SequenceNumber = 2;
			seal22.BK_SealNumber = "SEAL22";
			var seal23 = container2.AllSealNumbers.AddNew();
			seal23.BK_UnloadingState = ZString.Empty;
			seal23.BK_SequenceNumber = 3;
			seal23.BK_SealNumber = "SEAL23";
			AddPackageToConsignmentItem(consignmentItem1, container2.PK);
			AddPackageToConsignmentItem(consignmentItem2, container2.PK);
			AddPackageToConsignmentItem(consignmentItem3, container2.PK);
			AddPackageToConsignmentItem(consignmentItem4, container2.PK);

			var container3 = exitHeader.CusExitContainers.AddNew();
			container3.CXN_Sequence = 4;
			container3.CXN_ContainerNumber = "CONT3";
			container3.CXN_Status = ZString.Empty;
			var seal31 = container3.AllSealNumbers.AddNew();
			seal31.BK_UnloadingState = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			seal31.BK_SequenceNumber = 2;
			seal31.BK_SealNumber = "SEAL31";
			var seal32 = container3.AllSealNumbers.AddNew();
			seal32.BK_UnloadingState = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
			seal32.BK_SequenceNumber = 3;
			seal32.BK_SealNumber = "SEAL32";
			AddPackageToConsignmentItem(consignmentItem1, container3.PK);

			var container4 = exitHeader.CusExitContainers.AddNew();
			container4.CXN_Sequence = 5;
			container4.CXN_ContainerNumber = "CONT4";
			container4.CXN_Status = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			var seal41 = container4.AllSealNumbers.AddNew();
			seal41.BK_UnloadingState = ZString.Empty;
			seal41.BK_SequenceNumber = 1;
			seal41.BK_SealNumber = "SEAL41";
			AddPackageToConsignmentItem(consignmentItem1, container4.PK);
			AddPackageToConsignmentItem(consignmentItem2, container4.PK);

			var container5 = exitHeader.CusExitContainers.AddNew();
			container5.CXN_Sequence = 6;
			container5.CXN_ContainerNumber = "CONT5";
			container5.CXN_SealCount = 3;
			container5.CXN_Status = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			AddPackageToConsignmentItem(consignmentItem3, container5.PK);
			AddPackageToConsignmentItem(consignmentItem4, container5.PK);

			var container6 = exitHeader.CusExitContainers.AddNew();
			container6.CXN_Sequence = 7;
			container6.CXN_ContainerNumber = "CONT6";
			container6.CXN_SealCount = 1;
			container6.CXN_IsEquipment = true;
			container6.CXN_Status = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			var seal61 = container6.AllSealNumbers.AddNew();
			seal61.BK_UnloadingState = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			seal61.BK_SequenceNumber = 5;
			seal61.BK_SealNumber = "SEAL61";

			CombineAssertions(() =>
			{
				var wrapperList = EALAESTransportEquipmentWrapper.GetTransportEquipmentList(exitReport).ToList();

				AssertEquals("There are 6 TransportEquipments declared", 6, wrapperList.Count);

				AssertEquals("For first transportEquipment filled SequenceNumber", "1", wrapperList[0].SequenceNumber);
				AssertEquals("For first transportEquipment empty ContainerNumber when CXN_Status is MIS", ZString.Empty, wrapperList[0].ContainerNumber);
				AssertEquals("For first transportEquipment empty NumberOfSeals when CXN_Status is MIS", ZString.Empty, wrapperList[0].NumberOfSeals);
				AssertEquals("For first transportEquipment empty Seals when CXN_Status is MIS", 0, wrapperList[0].Seals.Count);
				AssertEquals("For first transportEquipment empty GoodsReference when CXN_Status is MIS", 0, wrapperList[0].GoodsReference.Count);

				AssertEquals("For second transportEquipment filled SequenceNumber", "3", wrapperList[1].SequenceNumber);
				AssertEquals("For second transportEquipment filled ContainerNumber when CXN_Status is DIF", "CONT2", wrapperList[1].ContainerNumber);
				AssertEquals("For second transportEquipment filled NumberOfSeals when CXN_Status is DIF and CXN_SealCount is not 0", "2", wrapperList[1].NumberOfSeals);
				AssertEquals("For second transportEquipment filled Seals when CXN_Status is DIF and there are seals with BK_UnloadingState MIS or DIF", 2, wrapperList[1].Seals.Count);
				AssertContainsExactElementsInExactOrder("For second transportEquipment expected filled Seals with correct data, SequenceNumber", new ZString[] { "1", "2" }, wrapperList[1].Seals.Select(x => x.SequenceNumber));
				AssertContainsExactElementsInExactOrder("For second transportEquipment expected filled Seals with correct data, SealNumber", new ZString[] { "SEAL21", "SEAL22" }, wrapperList[1].Seals.Select(x => x.SealNumber));
				AssertContainsExactElementsInExactOrder("For second transportEquipment expected filled GoodsReference with correct data (DIF consignment items with container in packages), SequenceNumber", new ZString[] { "1", "2" }, wrapperList[1].GoodsReference.Select(x => x.SequenceNumber));
				AssertContainsExactElementsInExactOrder("For second transportEquipment expected filled GoodsReference with correct data (DIF consignment items with container in packages), GoodsItemNumber", new ZString[] { "1", "3" }, wrapperList[1].GoodsReference.Select(x => x.GoodsItemNumber));

				AssertEquals("For third transportEquipment filled SequenceNumber", "4", wrapperList[2].SequenceNumber);
				AssertEquals("For third transportEquipment empty ContainerNumber when CXN_Status is empty", ZString.Empty, wrapperList[2].ContainerNumber);
				AssertEquals("For third transportEquipment empty NumberOfSeals when CXN_Status is empty and CXN_SealCount is 0", ZString.Empty, wrapperList[2].NumberOfSeals);
				AssertEquals("For third transportEquipment filled Seals when CXN_Status is empty and there are seals with BK_UnloadingState MIS or DIF", 2, wrapperList[2].Seals.Count);
				AssertContainsExactElementsInExactOrder("For third transportEquipment expected filled Seals with correct data, SequenceNumber", new ZString[] { "2", "3" }, wrapperList[2].Seals.Select(x => x.SequenceNumber));
				AssertContainsExactElementsInExactOrder("For third transportEquipment expected filled Seals with correct data, SealNumber", new ZString[] { "SEAL31", "" }, wrapperList[2].Seals.Select(x => x.SealNumber));
				AssertEquals("For third transportEquipment empty GoodsReference when CXN_Status is empty", 0, wrapperList[2].GoodsReference.Count);

				AssertEquals("For fourth transportEquipment filled SequenceNumber", "5", wrapperList[3].SequenceNumber);
				AssertEquals("For fourth transportEquipment filled ContainerNumber when CXN_Status is DIF", "CONT4", wrapperList[3].ContainerNumber);
				AssertEquals("For fourth transportEquipment empty NumberOfSeals when CXN_Status is DIF and CXN_SealCount is 0", ZString.Empty, wrapperList[3].NumberOfSeals);
				AssertEquals("For fourth transportEquipment empty Seals when CXN_Status is DIF but there are no seals with BK_UnloadingState MIS or DIF", 0, wrapperList[3].Seals.Count);
				AssertContainsExactElementsInExactOrder("For fourth transportEquipment expected filled GoodsReference with correct data (DIF consignment items with container in packages), SequenceNumber", new ZString[] { "1" }, wrapperList[3].GoodsReference.Select(x => x.SequenceNumber));
				AssertContainsExactElementsInExactOrder("For fourth transportEquipment expected filled GoodsReference with correct data (DIF consignment items with container in packages), GoodsItemNumber", new ZString[] { "1" }, wrapperList[3].GoodsReference.Select(x => x.GoodsItemNumber));

				AssertEquals("For fifth transportEquipment filled SequenceNumber", "6", wrapperList[4].SequenceNumber);
				AssertEquals("For fifth transportEquipment filled ContainerNumber when CXN_Status is DIF", "CONT5", wrapperList[4].ContainerNumber);
				AssertEquals("For fifth transportEquipment filled NumberOfSeals when CXN_Status is DIF and CXN_SealCount is not 0", "3", wrapperList[4].NumberOfSeals);
				AssertEquals("For fifth transportEquipment empty Seals when CXN_Status is DIF but there are no seals", 0, wrapperList[4].Seals.Count);
				AssertContainsExactElementsInExactOrder("For fifth transportEquipment expected filled GoodsReference with correct data (DIF consignment items with container in packages), SequenceNumber", new ZString[] { "1" }, wrapperList[4].GoodsReference.Select(x => x.SequenceNumber));
				AssertContainsExactElementsInExactOrder("For fifth transportEquipment expected filled GoodsReference with correct data (DIF consignment items with container in packages), GoodsItemNumber", new ZString[] { "3" }, wrapperList[4].GoodsReference.Select(x => x.GoodsItemNumber));

				AssertEquals("For sixth transportEquipment filled SequenceNumber", "7", wrapperList[5].SequenceNumber);
				AssertEquals("For sixth transportEquipment empty ContainerNumber when CXN_Status is DIF but CXN_IsEquipment is true", ZString.Empty, wrapperList[5].ContainerNumber);
				AssertEquals("For sixth transportEquipment filled NumberOfSeals when CXN_Status is DIF and CXN_SealCount is not 0", "1", wrapperList[5].NumberOfSeals);
				AssertEquals("For sixth transportEquipment filled Seals when CXN_Status is DIF and there are seals with BK_UnloadingState MIS or DIF", 1, wrapperList[5].Seals.Count);
				AssertContainsExactElementsInExactOrder("For sixth transportEquipment expected filled Seals with correct data, SequenceNumber", new ZString[] { "5" }, wrapperList[5].Seals.Select(x => x.SequenceNumber));
				AssertContainsExactElementsInExactOrder("For sixth transportEquipment expected filled Seals with correct data, SealNumber", new ZString[] { "SEAL61" }, wrapperList[5].Seals.Select(x => x.SealNumber));
				AssertEquals("For sixth transportEquipment empty GoodsReference when no package with container in consignment items", 0, wrapperList[5].GoodsReference.Count);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitReport = exitHeader.CusExitReports.AddNew();

			exitReport.CER_Calc_Discrepancies = ZBool.True;
			container = (CusExitContainer)exitHeader.CusExitContainers.AddNew();
			container.CXN_Status = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
			container.CXN_Sequence = 1;
			container.CXN_ContainerNumber = "CONT1";
			var seal11 = container.AllSealNumbers.AddNew();
			seal11.BK_UnloadingState = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			seal11.BK_SequenceNumber = 1;
			seal11.BK_SealNumber = "SEAL11";

			var wrapperList = EALAESTransportEquipmentWrapper.GetTransportEquipmentList(exitReport).ToList();
			wrapper = wrapperList[0];
		}
		CusExitHeader exitHeader;
		CusExitReport exitReport;
		CusExitContainer container;
		EALAESTransportEquipmentWrapper wrapper;

		protected override EALAESTransportEquipmentWrapper GetProvider() => wrapper;

		EU.ExitControl.Business.CusExitConsignmentPackage AddPackageToConsignmentItem(CusExitConsignmentItem consignmentItem, ZGuid containerPK)
		{
			var consignmentPackagePivot = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var package = consignmentPackagePivot.Package;
			consignmentPackagePivot.CNP_CXN_Container = containerPK;
			return package;
		}
	}
}
