using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	class EALAESConsignmentWrapperTest : WrapperHelperTest<EALAESConsignmentWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if Exit Report is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "exitReport"), () => GetWrapper(null));

				AssertExceptionThrown("Constructor Throws Exception if Consignment is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "Consignment"), () => GetWrapper(Factory.New<CusExitReport>()));
			});
		}

		public void TestModeOfTransportAtTheBorder()
		{
			CombineAssertions(() =>
			{
				exitReport.CER_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Expected empty if no discrepancies", ZString.Empty, wrapper.ModeOfTransportAtTheBorder);

				exitReport.CER_Calc_Discrepancies = true;
				AssertEquals("Expected filled ModeOfTransportAtBorder Sea (1)", "1", wrapper.ModeOfTransportAtTheBorder);

				exitReport.CER_TransportMode = Core.Constants.TransportModes.Rail;
				AssertEquals("Expected filled ModeOfTransportAtBorder Rail (2)", "2", wrapper.ModeOfTransportAtTheBorder);

				exitReport.CER_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals("Expected filled ModeOfTransportAtBorder Road (3)", "3", wrapper.ModeOfTransportAtTheBorder);

				exitReport.CER_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Expected filled ModeOfTransportAtBorder Air (4)", "4", wrapper.ModeOfTransportAtTheBorder);

				exitReport.CER_TransportMode = Core.Constants.TransportModes.Mail;
				AssertEquals("Expected filled ModeOfTransportAtBorder Mail (5)", "5", wrapper.ModeOfTransportAtTheBorder);

				exitReport.CER_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
				AssertEquals("Expected filled ModeOfTransportAtBorder FixedTransportInstallations (7)", "7", wrapper.ModeOfTransportAtTheBorder);

				exitReport.CER_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
				AssertEquals("Expected filled ModeOfTransportAtBorder InlandWaterwayTransport (8)", "8", wrapper.ModeOfTransportAtTheBorder);

				exitReport.CER_TransportMode = Core.Constants.TransportModes.OwnPropulsion;
				AssertEquals("Expected filled ModeOfTransportAtBorder OwnPropulsion (9)", "9", wrapper.ModeOfTransportAtTheBorder);
			});
		}

		public void TestReferenceNumberUCR()
		{
			CombineAssertions(() =>
			{
				exitConsignment.CXC_UniqueConsignmentReference = "reference";

				exitReport.CER_Calc_Discrepancies = ZBool.True;
				AssertEquals("Expected filled ReferenceNumberUCR when there are discrepacies", "reference", wrapper.ReferenceNumberUCR);

				exitReport.CER_Calc_Discrepancies = ZBool.False;
				wrapper = GetWrapper(exitReport);
				AssertEquals("Expected empty ReferenceNumberUCR when no discrepancies", ZString.Empty, wrapper.ReferenceNumberUCR);
			});
		}

		public void TestExitCarrier()
		{
			CombineAssertions(() =>
			{
				var orgAddress = Factory.New<OrgAddress>();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgAddress.OA_OH = orgHeader.PK;
				exitHeader.CXH_OA_Carrier = orgHeader.MainAddress.PK;

				wrapper = GetWrapper(exitReport);
				var exitCarrier = wrapper.ExitCarrier;

				AssertNotNull("Expected filled ExitCarrier", exitCarrier);
				AssertSame("Cached ExitCarrier", wrapper.ExitCarrier, exitCarrier);
			});
		}

		public void TestLocationOfGoods()
		{
			var locationOfGoods = wrapper.LocationOfGoods;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled LocationOfGoods", locationOfGoods);
				AssertSame("Cached LocationOfGoods", wrapper.LocationOfGoods, locationOfGoods);
			});
		}

		public void TestTransportEquipment()
		{
			CombineAssertions(() =>
			{
				var container1 = exitHeader.CusExitContainers.AddNew();
				var seal11 = container1.AllSealNumbers.AddNew();
				var seal12 = container1.AllSealNumbers.AddNew();
				var container2 = exitHeader.CusExitContainers.AddNew();
				var seal21 = container2.AllSealNumbers.AddNew();
				var seal22 = container2.AllSealNumbers.AddNew();

				exitReport.CER_Calc_Discrepancies = ZBool.True;
				wrapper = GetWrapper(exitReport);
				AssertEquals("Expected empty TransportEquipment when there are discrepancies but all containers status is empty and sealscount is 0 and all the associated seals have status is empty", 0, wrapper.TransportEquipment.Count);

				container1.CXN_Status = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
				wrapper = GetWrapper(exitReport);
				var transportEquipment = wrapper.TransportEquipment;
				AssertEquals("Expected filled TransportEquipment with 1 element when there are discrepacies and one container has CXN_Status MIS or DIF", 1, transportEquipment.Count);
				AssertSame("Cached TransportEquipment", wrapper.TransportEquipment, transportEquipment);

				container1.CXN_Status = ZString.Empty;
				container1.CXN_SealCount = 10;
				container2.CXN_SealCount = 5;
				wrapper = GetWrapper(exitReport);
				AssertEquals("Expected filled TransportEquipment with 2 elements when there are discrepacies and 2 containers have CXN_SealCount != 0", 2, wrapper.TransportEquipment.Count);

				container1.CXN_SealCount = 0;
				container2.CXN_SealCount = 0;
				seal21.BK_UnloadingState = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
				wrapper = GetWrapper(exitReport);
				AssertEquals("Expected filled TransportEquipment with 1 element when there are discrepacies and one container has one seal associated with BK_UnloadingState MIS or DIF", 1, wrapper.TransportEquipment.Count);

				exitReport.CER_Calc_Discrepancies = ZBool.False;
				wrapper = GetWrapper(exitReport);
				AssertEquals("Expected empty TransportEquipment when no discrepancies", 0, wrapper.TransportEquipment.Count);

				exitHeader.CusExitContainers.DeleteAll();
				exitReport.CER_Calc_Discrepancies = ZBool.True;
				wrapper = GetWrapper(exitReport);
				AssertEquals("Expected empty TransportEquipment when there are discrepacies but no containers exist", 0, wrapper.TransportEquipment.Count);
			});
		}

		public void TestActiveBorderTransportMeans()
		{
			CombineAssertions(() =>
			{
				exitReport.CER_Calc_Discrepancies = ZBool.True;
				AssertNull("Expected null ActiveBorderTransportMeans when no data declared (even if are discrepacies)", wrapper.ActiveBorderTransportMeans);

				exitReport.CER_TransportType = "00";
				wrapper = GetWrapper(exitReport);
				var activeBorderTransportMeans = wrapper.ActiveBorderTransportMeans;
				AssertNotNull("Expected filled ActiveBorderTransportMeans when discrepancies", activeBorderTransportMeans);
				AssertSame("Cached ActiveBorderTransportMeans", wrapper.ActiveBorderTransportMeans, activeBorderTransportMeans);

				exitReport.CER_Calc_Discrepancies = ZBool.False;
				wrapper = GetWrapper(exitReport);
				AssertNull("Expected null ActiveBorderTransportMeans when no discrepancies", wrapper.ActiveBorderTransportMeans);
			});
		}

		public void TestTransportDocument()
		{
			CombineAssertions(() =>
			{
				exitReport.CER_Calc_Discrepancies = ZBool.True;
				AssertEquals("Expected empty TransportDocument list when there are discrepancies but there are no documents", 0, wrapper.TransportDocument.Count);

				var supdoc1 = exitReport.AdditionalInfos.AddNew();
				supdoc1.CSI_ItemNumber = 1;
				supdoc1.CSI_Code = "9001";
				supdoc1.CSI_ReferenceNumber = "REF1";
				supdoc1.CSI_SubType = "TRA";
				supdoc1.CSI_Status = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;

				var supdoc2 = exitReport.AdditionalInfos.AddNew();
				supdoc2.CSI_ItemNumber = 2;
				supdoc2.CSI_Code = "9002";
				supdoc2.CSI_ReferenceNumber = "REF2";
				supdoc2.CSI_SubType = "TRA";

				var supdoc3 = exitReport.AdditionalInfos.AddNew();
				supdoc3.CSI_ItemNumber = 3;
				supdoc3.CSI_Code = "9003";
				supdoc3.CSI_ReferenceNumber = "REF3";
				supdoc3.CSI_SubType = "INF";
				supdoc3.CSI_Status = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;

				var supdoc4 = exitReport.AdditionalInfos.AddNew();
				supdoc4.CSI_ItemNumber = 4;
				supdoc4.CSI_Code = "9004";
				supdoc4.CSI_ReferenceNumber = "REF4";
				supdoc4.CSI_SubType = "TRA";
				supdoc4.CSI_Status = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;

				wrapper = GetWrapper(exitReport);
				var transportDocument = wrapper.TransportDocument;
				AssertEquals("Expected filled TransportDocument with 2 elements when there are discrepancies and there are 2 TRA documents with CSI_Status MIS or DIF", 2, transportDocument.Count);
				AssertSame("Cached TransportDocument", wrapper.TransportDocument, transportDocument);

				AssertContainsExactElementsInExactOrder("For TransportDocument with correct data, SequenceNumber", new ZString[] { "1", "4" }, transportDocument.Select(x => x.SequenceNumber));
				AssertContainsExactElementsInExactOrder("For TransportDocument with correct data, Name", new ZString[] { "9001", "" }, transportDocument.Select(x => x.Name));
				AssertContainsExactElementsInExactOrder("For TransportDocument with correct data, Number", new ZString[] { "REF1", "" }, transportDocument.Select(x => x.Number));

				supdoc1.CSI_Status = ZString.Empty;
				supdoc4.CSI_Status = ZString.Empty;
				wrapper = GetWrapper(exitReport);
				AssertEquals("Expected empty TransportDocument list when there are discrepancies but all TRA documents have CSI_Status empty", 0, wrapper.TransportDocument.Count);

				exitReport.CER_Calc_Discrepancies = ZBool.False;
				wrapper = GetWrapper(exitReport);
				AssertEquals("Expected null TransportDocument when no discrepancies", 0, wrapper.TransportDocument.Count);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitConsignment = exitHeader.CusExitConsignments.AddNew();
			exitReport = exitHeader.CusExitReports.AddNew();
			exitReport.CER_CXC_Consignment = exitConsignment.PK;

			wrapper = GetWrapper(exitReport);
		}
		CusExitHeader exitHeader;
		CusExitConsignment exitConsignment;
		CusExitReport exitReport;
		EALAESConsignmentWrapper wrapper;

		EALAESConsignmentWrapper GetWrapper(CusExitReport exitReport) => new EALAESConsignmentWrapper(exitReport);

		protected override EALAESConsignmentWrapper GetProvider() => wrapper;
	}
}
