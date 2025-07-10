using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Testing
{
	class DocBaseConsolTest : TestCaseWithFactory
	{
		public void TestPortOfDischargeIATA()
		{
			Consol.JK_RL_NKDischargePort = "GBABD";
			AssertEquals("ABZ", BaseConsolWrapper.PortOfDischargeIATA);
			Consol.JK_RL_NKDischargePort = "GBABC";
			AssertEquals("GBABC", BaseConsolWrapper.PortOfDischargeIATA);
		}

		public void TestPortOfLoadingIATA()
		{
			Consol.JK_RL_NKLoadPort = "GBABD";
			AssertEquals("ABZ", BaseConsolWrapper.PortOfLoadingIATA);
			Consol.JK_RL_NKLoadPort = "GBABC";
			AssertEquals("GBABC", BaseConsolWrapper.PortOfLoadingIATA);
		}

		#region TestTransportOrder

		public void TestTransportOrder()
		{
			Transport transport0 = Consol.Transports[0];
			transport0.JW_LegOrder = 4;

			Transport transport1 = Consol.Transports.AddNew();
			transport1.JW_LegOrder = 2;

			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_LegOrder = 6;

			AssertEquals(transport0.JW_LegOrder, BaseConsolWrapper.TransportPlanning[1].LegOrder);
			AssertEquals(transport1.JW_LegOrder, BaseConsolWrapper.TransportPlanning[0].LegOrder);
			AssertEquals(transport2.JW_LegOrder, BaseConsolWrapper.TransportPlanning[2].LegOrder);
		}

		#endregion

		#region TestFirstLoadPort

		public void TestFirstLoadPort()
		{
			Consol.JK_RL_NKLoadPort = "AUBNE";
			AssertEquals("Should proxy through", "AUBNE", BaseConsolWrapper.FirstLoadPort.Code);

			Consol.JK_RL_NKLoadPort = "SGSIN";
			AssertEquals("Should proxy trough", "SGSIN", BaseConsolWrapper.FirstLoadPort.Code);

			Consol.Transports.DepartureTransport.JW_RL_NKLoadPort = "NZAKL";
			AssertEquals("Only proxy through from the consol, not the transport", "SGSIN", BaseConsolWrapper.FirstLoadPort.Code);
		}

		#endregion

		#region TestLastDischargePort

		public void TestLastDischargePort()
		{
			Consol.JK_RL_NKDischargePort = "AUBNE";
			AssertEquals("Should proxy through", "AUBNE", BaseConsolWrapper.LastDischargePort.Code);

			Consol.JK_RL_NKDischargePort = "SGSIN";
			AssertEquals("Should proxy through", "SGSIN", BaseConsolWrapper.LastDischargePort.Code);

			Consol.Transports.ArrivalTransport.JW_RL_NKDiscPort = "NZAKL";
			AssertEquals("Should proxy through", "SGSIN", BaseConsolWrapper.LastDischargePort.Code);
		}

		#endregion

		#region Common Consol

		public void TestSetFromDocumentCommonConsol()
		{
			var consol = Factory.New<CommonConsol>();
			DocumentCommonConsol documentCommonConsol = new DocumentCommonConsol(consol, Core.Constants.DataContext.LoadListDocument);
			documentCommonConsol.IncludeConsignee = false;
			documentCommonConsol.IncludeConsignor = false;
			documentCommonConsol.IncludeCustomsBroker = false;
			documentCommonConsol.IncludeAllShipments = false;
			documentCommonConsol.IncludePacked = false;
			documentCommonConsol.IncludeUnPacked = false;
			BaseConsolWrapper.SetFromDocumentCommonConsolTest(documentCommonConsol);
			AssertEquals("Include consignee should be true", false, BaseConsolWrapper.IncludeConsignee);
			AssertEquals("Include Consignor should be true", false, BaseConsolWrapper.IncludeConsignor);
			AssertEquals("Include Customs Broker should be true", false, BaseConsolWrapper.IncludeCustomsBroker);
			AssertEquals("Include All Shipments should be true", false, BaseConsolWrapper.IncludeAllShipments);
			AssertEquals("Include UnPacked should be true", false, BaseConsolWrapper.IncludeUnPacked);
			AssertEquals("Include Packed should be true", false, BaseConsolWrapper.IncludePacked);

			documentCommonConsol.IncludeConsignee = true;
			documentCommonConsol.IncludeConsignor = true;
			documentCommonConsol.IncludeCustomsBroker = true;
			documentCommonConsol.IncludeAllShipments = true;
			documentCommonConsol.IncludePacked = true;
			documentCommonConsol.IncludeUnPacked = true;
			BaseConsolWrapper.SetFromDocumentCommonConsolTest(documentCommonConsol);
			AssertEquals("Include consignee should be true", true, BaseConsolWrapper.IncludeConsignee);
			AssertEquals("Include Consignor should be true", true, BaseConsolWrapper.IncludeConsignor);
			AssertEquals("Include Customs Broker should be true", true, BaseConsolWrapper.IncludeCustomsBroker);
			AssertEquals("Include All Shipments should be true", true, BaseConsolWrapper.IncludeAllShipments);
			AssertEquals("Include UnPacked should be true", true, BaseConsolWrapper.IncludeUnPacked);
			AssertEquals("Include Packed should be true", true, BaseConsolWrapper.IncludePacked);
		}

		#endregion

		#region Document Constants

		public void TestSetTemplateConstants()
		{
			AssertEquals("Sort shipments on HBL", 1, BaseConsolWrapper.SortShipmentsOnHBL);
			AssertEquals("Number of transport planning rows", 1, BaseConsolWrapper.NumberOfTransportPlanningRows);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.SortShipmentsOnHBL, 0);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfTransportPlanningRows, 5);

			BaseConsolWrapper.SetTemplateConstants(constants);
			AssertEquals("Sort shipments on HBL", 0, BaseConsolWrapper.SortShipmentsOnHBL);
			AssertEquals("Number of transport planning rows", 5, BaseConsolWrapper.NumberOfTransportPlanningRows);
		}

		public void TestReportName()
		{
			AssertEquals("Report name", ZString.Empty, BaseConsolWrapper.ReportName);

			BaseConsolWrapper.SetReportNameForTesting("Report name");
			AssertEquals("Report name", "Report name", BaseConsolWrapper.ReportName);
		}

		public void TestDocumentDirection()
		{
			AssertEquals("Document direction", ZString.Empty, BaseConsolWrapper.DocumentDirection);

			BaseConsolWrapper.SetDocumentDirectionForTesting("IMP");
			AssertEquals("Document direction", "IMP", BaseConsolWrapper.DocumentDirection);
		}

		public void TestSortShipmentsOnHBL()
		{
			AssertEquals("Sort shipments on HBL", 1, BaseConsolWrapper.SortShipmentsOnHBL);

			BaseConsolWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.SortShipmentsOnHBL, 0);
			AssertEquals("Sort shipments on HBL", 0, BaseConsolWrapper.SortShipmentsOnHBL);
		}

		public void TestNumberOfTransportPlanningRows()
		{
			AssertEquals("Number of transport planning rows", 1, BaseConsolWrapper.NumberOfTransportPlanningRows);

			BaseConsolWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfTransportPlanningRows, 4);
			AssertEquals("Number of transport planning rows", 4, BaseConsolWrapper.NumberOfTransportPlanningRows);
		}

		#endregion

		#region Properties

		#region TestTransportMode

		public void TestReleaseType()
		{
			CodeDescriptionPairList releaseTypeList = new CodeDescriptionPairList(OLookUpEditType.ShipmentReleaseType);

			DocumentsDataRegistry.Instance.ReleaseType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			AssertEquals("Default release type", ZString.Empty, BaseConsolWrapper.DefaultReleaseType);

			ZString expectedResult = releaseTypeList.GetDescriptionFromCode(Core.Constants.ShipmentReleaseTypes.ExpressBofL);
			DocumentsDataRegistry.Instance.ReleaseType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ShipmentReleaseTypes.ExpressBofL);
			AssertEquals("Release type", expectedResult, BaseConsolWrapper.DefaultReleaseType);

			Consol.JK_ReleaseType = "";
			AssertEquals("Release type", "", BaseConsolWrapper.ReleaseType);

			expectedResult = releaseTypeList.GetDescriptionFromCode(Core.Constants.ShipmentReleaseTypes.Cheque);
			Consol.JK_ReleaseType = Core.Constants.ShipmentReleaseTypes.Cheque;
			AssertEquals("Release type", expectedResult, BaseConsolWrapper.ReleaseType);
		}

		public void TestTransportMode()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("AIR", BaseConsolWrapper.TransportMode);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("SEA", BaseConsolWrapper.TransportMode);
		}

		#endregion

		#region TestTransportModeDecription

		public void TestTransportModeDecription()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(Core.Constants.TransportModeDescriptions.Air, BaseConsolWrapper.TransportModeDescription);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(Core.Constants.TransportModeDescriptions.Sea, BaseConsolWrapper.TransportModeDescription);
		}

		public void TestHeadingTransportMode()
		{
			AssertEquals("Transport is empty", ZString.Empty, BaseConsolWrapper.HeadingTransportMode);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Transport mode SEA", Core.Constants.TransportModeDescriptions.Sea.Replace(" Freight", ""), BaseConsolWrapper.HeadingTransportMode);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Transport mode AIR", Core.Constants.TransportModeDescriptions.Air.Replace(" Freight", ""), BaseConsolWrapper.HeadingTransportMode);
		}

		#endregion

		#endregion

		#region Icelandic

		public void TestTotalShipments()
		{
			Consol.Shipments.RemoveAll();
			AssertEquals("Precondition", 0, BaseConsolWrapper.TotalShipments);
			Consol.Shipments.AddNew();
			AssertEquals("One shipment added", 1, BaseConsolWrapper.TotalShipments);
			Consol.Shipments.AddNew();
			AssertEquals("Two shipments added", 2, BaseConsolWrapper.TotalShipments);
		}

		public void TestFirstLastAbbreviatedCustomsShipmentNumber()
		{
			Consol.Shipments.RemoveAll();
			AssertEquals("Precondition", 0, BaseConsolWrapper.TotalShipments);
			Consol.Shipments.AddNew();
			Consol.Shipments.AddNew();
			Consol.Shipments.AddNew();
			Consol.Shipments[0].CustomsEntryNumber = "f-098-0808-8-au-syd-J002-V";
			Consol.Shipments[1].CustomsEntryNumber = "f-098-0808-8-au-syd-J001-V";
			Consol.Shipments[2].CustomsEntryNumber = "f-098-0808-8-au-syd-J003-V";
			AssertEquals("J001", BaseConsolWrapper.FirstAbbreviatedCustomsShipmentNumber);
			AssertEquals("J003", BaseConsolWrapper.LastAbbreviatedCustomsShipmentNumber);
		}

		#endregion

		public void TestMasterBillNumberWithHyphen()
		{
			AssertEquals("", BaseConsolWrapper.MasterBillNumberWithHyphen);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_MasterBillNum = "12";
			AssertEquals("Should be 12", "12", BaseConsolWrapper.MasterBillNumberWithHyphen);

			Consol.JK_MasterBillNum = "123456";
			AssertEquals("Should be 123456", "123456", BaseConsolWrapper.MasterBillNumberWithHyphen);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.JK_MasterBillNum = "12";
			AssertEquals("Should be 12", "12", BaseConsolWrapper.MasterBillNumberWithHyphen);

			Consol.JK_MasterBillNum = "123-456";
			AssertEquals("Should be 123-456", "123-456", BaseConsolWrapper.MasterBillNumberWithHyphen);
		}

		public void TestMasterBillIssueDate()
		{
			AssertEquals(ZDateTime.Empty, BaseConsolWrapper.MasterBillIssueDate);

			Consol.JK_MasterBillIssueDate = ZDateTime.Today;
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Only show for air", ZDateTime.Empty, BaseConsolWrapper.MasterBillIssueDate);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("JK_MasterBillIssueDate should be today", ZDateTime.Today, BaseConsolWrapper.MasterBillIssueDate);
		}

		public void TestMasterBillAndIssueDate()
		{
			AssertEquals("", BaseConsolWrapper.MasterBillAndIssueDate);

			Consol.JK_MasterBillNum = "12345";
			Consol.JK_MasterBillIssueDate = ZDateTime.Today;
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Only show for air", "12345", BaseConsolWrapper.MasterBillAndIssueDate);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.JK_MasterBillNum = "12345";
			AssertEquals("MasterBillAndIssueDate should be 12345 / Today.short", "12345 / " + ZDateTime.Today.ToShortDateString(), BaseConsolWrapper.MasterBillAndIssueDate);
		}

		public void TestFreightChargeType()
		{
			AssertEquals("No freight charge type", ZString.Empty, BaseConsolWrapper.FreightChargeType);

			Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			AssertEquals("Freight charge type", "FREIGHT COLLECT", BaseConsolWrapper.FreightChargeType);

			Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			AssertEquals("Freight charge type", "FREIGHT PREPAID", BaseConsolWrapper.FreightChargeType);
		}

		public void TestEmailSubjectNumber()
		{
			Consol.JK_UniqueConsignRef = "C12345678";
			AssertEquals("EmailSubjectNumber", "C12345678", BaseConsolWrapper.EmailSubjectNumber);
		}

		public void TestReportNameWithoutCFS()
		{
			AssertEquals("ReportNameWithoutCFS", ZString.Empty, BaseConsolWrapper.ReportNameWithoutCFS);

			BaseConsolWrapper.SetReportNameForTesting("Report name");
			AssertEquals("ReportNameWithoutCFS", "Report name", BaseConsolWrapper.ReportNameWithoutCFS);

			BaseConsolWrapper.SetReportNameForTesting("Report name - CFS");
			AssertEquals("ReportNameWithoutCFS", "Report name", BaseConsolWrapper.ReportNameWithoutCFS);
		}

		public void TestFormatTransportDetails()
		{
			ZString expectedResult = "    " + " /    " + " /    ";
			AssertEquals(expectedResult, BaseConsolWrapper.FormatTransportDetails(ZString.Empty, ZString.Empty, ZString.Empty));

			expectedResult = "String 1" + " /    " + " /    ";
			AssertEquals(expectedResult, BaseConsolWrapper.FormatTransportDetails("String 1", ZString.Empty, ZString.Empty));

			expectedResult = "    " + " / String 2" + " /    ";
			AssertEquals(expectedResult, BaseConsolWrapper.FormatTransportDetails(ZString.Empty, "String 2", ZString.Empty));

			expectedResult = "    " + " /    " + " / String 3";
			AssertEquals(expectedResult, BaseConsolWrapper.FormatTransportDetails(ZString.Empty, ZString.Empty, "String 3"));

			expectedResult = "String 1" + " /    " + " / String 3";
			AssertEquals(expectedResult, BaseConsolWrapper.FormatTransportDetails("String 1", ZString.Empty, "String 3"));

			expectedResult = "String 1" + " / String 2" + " /    ";
			AssertEquals(expectedResult, BaseConsolWrapper.FormatTransportDetails("String 1", "String 2", ZString.Empty));

			expectedResult = "    " + " / String 2" + " / String 3";
			AssertEquals(expectedResult, BaseConsolWrapper.FormatTransportDetails(ZString.Empty, "String 2", "String 3"));

			expectedResult = "String 1" + " / String 2" + " / String 3";
			AssertEquals(expectedResult, BaseConsolWrapper.FormatTransportDetails("String 1", "String 2", "String 3"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestGetAirTransportDetails()
		{
			Transport.JW_RL_NKLoadPort = "USLAX";
			Transport.JW_RL_NKDiscPort = "AUSYD";
			Transport.JW_ETD = DateTime.Today;
			Transport.JW_VoyageFlight = "111";
			ZString depTime = Consol.JK_JX_JA_E_DEP.ToString("dd-MMM-yy");
			ZString expected = BaseConsolWrapper.FormatTransportDetails(Consol.JK_JX_JV_VoyageFlight, Consol.JK_JX_JB_RL_NKPortOfDischarge, depTime);
			AssertEquals(expected, BaseConsolWrapper.GetAirTransportDetails);

			SuppressionTest.SetSuppressingFields(DocumentsDataRegistry.Instance.SuppressPrintingOfFlightDate, true);
			AssertEquals(expected, BaseConsolWrapper.GetAirTransportDetails);

			Transport.JW_RL_NKLoadPort = "AUSYD";
			Transport.JW_RL_NKDiscPort = "USLAX";
			Transport.JW_ETD = DateTime.Today.AddDays(1);
			expected = BaseConsolWrapper.FormatTransportDetails(Consol.JK_JX_JV_VoyageFlight, Consol.JK_JX_JB_RL_NKPortOfDischarge, ZString.Empty);

			Transport.JW_ETD = DateTime.Today.AddDays(-1);
			depTime = Consol.JK_JX_JA_E_DEP.ToString("dd-MMM-yy");
			expected = BaseConsolWrapper.FormatTransportDetails(Consol.JK_JX_JV_VoyageFlight, Consol.JK_JX_JB_RL_NKPortOfDischarge, depTime);
		}

		public void TestGetFirstAndSecondLegAirTransportDetails()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			ZString test = "First and Second Leg Transport Details";
			Transport originalTransport = Consol.Transports[0];

			Transport primariyTransport = Consol.Transports[0];
			primariyTransport.JW_TransportMode = Core.Constants.TransportModes.Air;
			primariyTransport.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			AssertEquals("precondition:", Core.Constants.TransportModes.Air, primariyTransport.JW_TransportMode);
			AssertEquals("precondition:", Core.Constants.TransportPlanningType.Flight1, primariyTransport.JW_TransportType);

			primariyTransport.JW_RL_NKDiscPort = "";
			primariyTransport.JW_VoyageFlight = "CON111";
			primariyTransport.JW_ETD = DateTime.Today;
			ZString expected = BaseConsolWrapper.FormatFlightDetails("CON111", "", DateTime.Today);
			AssertEquals(test, expected, BaseConsolWrapper.GetFirstAndSecondLegAirTransportDetails());

			originalTransport.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Other;
			BaseConsolWrapper = new DocBaseConsolTestClass(Consol, Factory);
			AssertEquals(test, expected, BaseConsolWrapper.GetFirstAndSecondLegAirTransportDetails());

			originalTransport.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Flight1;
			Transport transport1 = Consol.Transports.AddNew();
			transport1.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Flight2;
			transport1.JW_VoyageFlight = "Trans111";
			primariyTransport.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_ETD = DateTime.Today.AddDays(2);
			BaseConsolWrapper = new DocBaseConsolTestClass(Consol, Factory);
			originalTransport.JW_ETD = DateTime.Today;
			originalTransport.JW_RL_NKDiscPort = "SGSIN";
			expected = BaseConsolWrapper.FormatFlightDetails("CON111", "SGSIN", DateTime.Today) + " -> ";
			expected += BaseConsolWrapper.FormatFlightDetails(transport1.JW_VoyageFlight, ZString.Empty, transport1.JW_ETD);
			AssertEquals(test, expected, BaseConsolWrapper.GetFirstAndSecondLegAirTransportDetails());

			transport1.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Other;
			BaseConsolWrapper = new DocBaseConsolTestClass(Consol, Factory);
			expected = BaseConsolWrapper.FormatFlightDetails("CON111", "SGSIN", DateTime.Today) + " -> ";
			expected += BaseConsolWrapper.FormatFlightDetails(transport1.JW_VoyageFlight, ZString.Empty, transport1.JW_ETD);
			AssertEquals(test, expected, BaseConsolWrapper.GetFirstAndSecondLegAirTransportDetails());

			transport1.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Flight2;

			transport1.JW_RL_NKDiscPort = "AUSYD";
			originalTransport.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Flight3;
			BaseConsolWrapper = new DocBaseConsolTestClass(Consol, Factory);
			expected = BaseConsolWrapper.FormatFlightDetails(transport1.JW_VoyageFlight, transport1.JW_RL_NKDiscPort, transport1.JW_ETD) + " -> ";
			expected += BaseConsolWrapper.FormatFlightDetails("CON111", "SGSIN", DateTime.Today);
			AssertEquals(test, expected, BaseConsolWrapper.GetFirstAndSecondLegAirTransportDetails());

			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Flight2;
			transport2.JW_VoyageFlight = "Trans222";
			transport2.JW_RL_NKDiscPort = "AUBNE";
			transport1.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Other;
			transport2.JW_ETD = DateTime.Today.AddDays(1);
			BaseConsolWrapper = new DocBaseConsolTestClass(Consol, Factory);
			expected = BaseConsolWrapper.FormatFlightDetails(transport2.JW_VoyageFlight, transport2.JW_RL_NKDiscPort, transport2.JW_ETD) + " -> ";
			expected += BaseConsolWrapper.FormatFlightDetails("CON111", "SGSIN", DateTime.Today);
			AssertEquals(test, expected, BaseConsolWrapper.GetFirstAndSecondLegAirTransportDetails());

			transport2.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Other;
			BaseConsolWrapper = new DocBaseConsolTestClass(Consol, Factory);
			expected = BaseConsolWrapper.FormatFlightDetails("CON111", "SGSIN", DateTime.Today) + " -> ";
			expected += BaseConsolWrapper.FormatFlightDetails(transport2.JW_VoyageFlight, transport2.JW_RL_NKDiscPort, transport2.JW_ETD);
			AssertEquals(test, expected, BaseConsolWrapper.GetFirstAndSecondLegAirTransportDetails());

			originalTransport.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Other;
			originalTransport.JW_ETD = DateTime.Today.AddDays(5);
			Transport transport3 = Consol.Transports.AddNew();
			transport3.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Other;
			transport3.JW_VoyageFlight = "Trans333";
			transport3.JW_RL_NKDiscPort = "AUMEL";
			transport3.JW_ETD = DateTime.Today;
			BaseConsolWrapper = new DocBaseConsolTestClass(Consol, Factory);
			expected = BaseConsolWrapper.FormatFlightDetails(transport3.JW_VoyageFlight, transport3.JW_RL_NKDiscPort, transport3.JW_ETD) + " -> ";
			expected += BaseConsolWrapper.FormatFlightDetails(transport2.JW_VoyageFlight, transport2.JW_RL_NKDiscPort, transport2.JW_ETD);
			AssertEquals(test, expected, BaseConsolWrapper.GetFirstAndSecondLegAirTransportDetails());
		}

		public void TestTransportTimes()
		{
			Transport.JW_ETA = Env.Time.CurrentLocalDate.AddDays(-3);
			Transport.JW_ETD = Env.Time.CurrentLocalDate.AddDays(-5);
			Transport.JW_ATA = ZDateTime.Empty;
			Transport.JW_ATD = ZDateTime.Empty;

			AssertEquals("Departure time taken from ETD", Env.Time.CurrentLocalDate.AddDays(-5), BaseConsolWrapper.ATD.ToDateTime());
			AssertEquals("Arrival time taken from ETA", Env.Time.CurrentLocalDate.AddDays(-3), BaseConsolWrapper.ATA.ToDateTime());
		}

		public void TestPreCarriageVessel()
		{
			Transport preCarriageVessel = Consol.Transports.AddNew();
			preCarriageVessel.JW_TransportMode = Core.Constants.TransportModes.Sea;
			preCarriageVessel.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			preCarriageVessel.JW_Vessel = "VESSEL 123";
			preCarriageVessel.JW_VoyageFlight = "NO123";

			AssertEquals("PreCarriage vessel is 'VESSEL 123'", preCarriageVessel.JW_Vessel, BaseConsolWrapper.PreCarriageVessel);

			Consol.Transports.Remove(preCarriageVessel);
			Transport otherVessel = Consol.Transports.AddNew();
			otherVessel.JW_TransportMode = Core.Constants.TransportModes.Sea;
			otherVessel.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			otherVessel.JW_Vessel = "VESSEL 555";
			otherVessel.JW_VoyageFlight = "NO333";

			BaseConsolWrapper = new DocBaseConsolTestClass(Consol, Factory);

			AssertEquals("PreCarriage vessel is empty", "", BaseConsolWrapper.PreCarriageVessel);
		}

		public void TestETDAndETA()
		{
			Transport.JW_ETD = Env.Time.CurrentLocalDate.AddDays(-5);
			Transport.JW_ETA = Env.Time.CurrentLocalDate.AddDays(-3);
			Transport.JW_ATD = Env.Time.CurrentLocalDate.AddDays(-4);
			Transport.JW_ATA = Env.Time.CurrentLocalDate.AddDays(-2);

			AssertEquals("ETD", Env.Time.CurrentLocalDate.AddDays(-5), BaseConsolWrapper.ETD.ToDateTime());
			AssertEquals("ETA", Env.Time.CurrentLocalDate.AddDays(-3), BaseConsolWrapper.ETA.ToDateTime());
			AssertEquals("ATD", Env.Time.CurrentLocalDate.AddDays(-4), BaseConsolWrapper.ATD.ToDateTime());
			AssertEquals("ATA", Env.Time.CurrentLocalDate.AddDays(-2), BaseConsolWrapper.ATA.ToDateTime());
		}

		public void TestFirstLegPortOfLoadingAndLastLegPortOfDischarge()
		{
			var consol = Factory.New<CommonConsol>();
			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "KRSEL";
			transport1.JW_RL_NKDiscPort = "USNYC";
			transport1.JW_ETD = new ZDateTime(2013, 07, 01);
			transport1.JW_ETA = new ZDateTime(2013, 07, 02);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "USNYC";
			transport2.JW_RL_NKDiscPort = "AUSYD";
			transport2.JW_ETD = new ZDateTime(2013, 07, 03);
			transport2.JW_ETA = new ZDateTime(2013, 07, 04);
			Factory.Save();

			BaseConsolWrapper = new DocBaseConsolTestClass(consol, Factory);

			AssertEquals("Port of Loading of First Leg", "KRSEL", BaseConsolWrapper.FirstLegPortOfLoading.Code);
			AssertEquals("Port of Discharge of Last Leg", "AUSYD", BaseConsolWrapper.LastLegPortOfDischarge.Code);
		}

		public void TestFirstLegETDAndLastLegETA()
		{
			var consol = Factory.New<CommonConsol>();
			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "KRSEL";
			transport1.JW_RL_NKDiscPort = "USNYC";
			transport1.JW_ETD = new ZDateTime(2013, 07, 01);
			transport1.JW_ETA = new ZDateTime(2013, 07, 02);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "USNYC";
			transport2.JW_RL_NKDiscPort = "AUSYD";
			transport2.JW_ETD = new ZDateTime(2013, 07, 03);
			transport2.JW_ETA = new ZDateTime(2013, 07, 04);
			Factory.Save();

			BaseConsolWrapper = new DocBaseConsolTestClass(consol, Factory);

			AssertEquals("ETD of First Leg", new ZDateTime(2013, 07, 01), BaseConsolWrapper.FirstLegETD);
			AssertEquals("ETA of Last Leg", new ZDateTime(2013, 07, 04), BaseConsolWrapper.LastLegETA);
		}

		public void TestMasterBillNum()
		{
			AssertEquals("Master bill", ZString.Empty, BaseConsolWrapper.MasterBillNum);

			Consol.JK_MasterBillNum = "IFC-111";
			AssertEquals("Masterbill", "IFC-111", BaseConsolWrapper.MasterBillNum);
		}

		public void TestAgentsReference()
		{
			AssertEquals("Agent Ref is empty", ZString.Empty, BaseConsolWrapper.AgentsReference);

			Consol.JK_AgentsReference = "AGENTREF-555";
			AssertEquals("AgentRef", "AGENTREF-555", BaseConsolWrapper.AgentsReference);
		}

		public void TestGoodsAvailableAt()
		{
			var arrivalCTOHeader = Factory.New<OrgHeader>();
			arrivalCTOHeader.OH_FullName = "Arrival CTO Ltd";
			var arrivalCTOAddress = arrivalCTOHeader.Addresses.AddNew();
			arrivalCTOAddress.OA_Address1 = "333 Three Street";
			arrivalCTOAddress.OA_Address2 = "Building C";
			Consol.JK_OA_ArrivalCTOAddress = arrivalCTOAddress.PK;
			AssertEquals("Goods Available At Arrival CTO for CTO Consol", "Arrival CTO Ltd", BaseConsolWrapper.GoodsAvailableAt.CompanyName);

			var unpackDepotHeader = Factory.New<OrgHeader>();
			unpackDepotHeader.OH_FullName = "Unpack depot Ltd";
			var unpackDepotAddress = unpackDepotHeader.Addresses.AddNew();
			unpackDepotAddress.OA_Address1 = "222 Two Street";
			unpackDepotAddress.OA_Address2 = "Building B";
			Consol.JK_OA_UnpackDepotAddress = unpackDepotAddress.PK;

			Consol.JK_ConsolMode = "LCL";
			AssertEquals("Goods Available At Unpack Depot for CFS Consol", "Unpack depot Ltd", BaseConsolWrapper.GoodsAvailableAt.CompanyName);

			Consol.JK_ConsolMode = "FCL";
			Consol.ArrivalCTOAddress.Delete();
			AssertEquals("Goods Available At defaulted to Unpack Depot for CFS Consol with empty Arrival CTO address", "Unpack depot Ltd", BaseConsolWrapper.GoodsAvailableAt.CompanyName);
		}

		public void TestSendingForwarder()
		{
			AssertNull("No sending forwarder", BaseConsolWrapper.SendingForwarder);

			var headerBisObject = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.NotEqual, ""));
			headerBisObject.MainAddress.OA_Address1 = "MAIN ADDRESS";
			var addr2 = headerBisObject.Addresses.AddNew();
			addr2.OA_Address1 = "SF SECOND ADDRESS";
			addr2.OA_RN_NKCountryCode = ZString.Empty;

			Consol.JK_OA_SendingForwarderAddress = addr2.PK;
			AssertEquals("Sending Forwarder name", headerBisObject.OH_FullName, BaseConsolWrapper.SendingForwarder.Name);
			AssertEquals("Sending Forwarder address", headerBisObject.OH_FullName + "\nSF SECOND ADDRESS\n" + headerBisObject.Country.Description.ToUpper(), BaseConsolWrapper.SendingForwarder.SelectedAddress.ToString());
		}

		public void TestReceivingForwarder()
		{
			AssertNull("No Receiving Forwarder", BaseConsolWrapper.ReceivingForwarder);

			var headerBisObject = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.NotEqual, ""));
			headerBisObject.MainAddress.OA_Address1 = "MAIN ADDRESS";
			var addr2 = headerBisObject.Addresses.AddNew();
			addr2.OA_Address1 = "RF SECOND ADDRESS";
			addr2.OA_RN_NKCountryCode = ZString.Empty;

			Consol.JK_OA_ReceivingForwarderAddress = addr2.PK;
			AssertEquals("Receiving Forwarder name", headerBisObject.OH_FullName, BaseConsolWrapper.ReceivingForwarder.Name);
			AssertEquals("Receiving Forwarder address 1", headerBisObject.OH_FullName + "\nRF SECOND ADDRESS\n" + headerBisObject.Country.Description.ToUpper(), BaseConsolWrapper.ReceivingForwarder.SelectedAddress.ToString());
		}

		public void TestNotifyPartyDocumentaryAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			BaseConsolWrapper = new DocBaseConsolTestClass(consol, Factory);
			BaseConsolWrapper.SetTemplateConstants(new Dictionary<string, object> { { DocumentEngineIntegration.Constants.TemplateDefined.ContactType, ContactType.Consignor.Code } });

			AssertNull("No Notify Party", BaseConsolWrapper.NotifyPartyOrganisation);

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "CCC";
			org2.OH_FullName = "DDD";
			org2.MainAddress.OA_Address1 = "NOTIFY SECOND ADDRESS";

			OrgContact contact = org2.Contacts.AddNew();
			contact.OC_ContactName = "fff";
			OrgDocument document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.Marketing.Code;

			OrgContact contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "eee";
			document = contact2.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.NotifyParty.Code;

			consol.NotifyPartyDocumentaryAddress.OrganisationPK = org2.PK;
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;
			AssertEquals("Notify Party name", org2.OH_FullName, BaseConsolWrapper.NotifyPartyOrganisation.Name);
			AssertEquals("Notify Party address 1", org2.OH_FullName + "\nNOTIFY SECOND ADDRESS", BaseConsolWrapper.NotifyPartyOrganisation.SelectedAddress.ToString());
		}

		public void TestCRN()
		{
			AssertEquals("CRN is empty", ZString.Empty, BaseConsolWrapper.CustomsEntryNumberForExportOnly);

			Transport transport = Consol.Transports[0];
			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			transport.JW_RL_NKLoadPort = uNLOCO.RL_Code;
			AssertEquals("CRN is empty", ZString.Empty, BaseConsolWrapper.CustomsEntryNumberForExportOnly);

			var cRN = Consol.CusEntryNums.AddNew();
			cRN.CE_EntryNum = "TEST ENTRY NUM";
			cRN.CE_EntryType = "CRN";
			AssertEquals("CRN", "TEST ENTRY NUM", BaseConsolWrapper.CustomsEntryNumberForExportOnly);
			AssertEquals("ConsolCRN", "TEST ENTRY NUM", BaseConsolWrapper.ConsolCRN);
			transport.JW_RL_NKLoadPort = "HKHKG";

			AssertEquals("CRN", ZString.Empty, BaseConsolWrapper.CustomsEntryNumberForExportOnly);
			AssertEquals("ConsolCRN", "TEST ENTRY NUM", BaseConsolWrapper.ConsolCRN);
			RefCountry oldCountry = GlbCompany.CurrentCompany.Country;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);

				transport.JW_RL_NKLoadPort = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode))).RL_Code;

				AssertEquals("CRN", "TEST ENTRY NUM", BaseConsolWrapper.CustomsEntryNumberForExportOnly);
				AssertEquals("ConsolCRN", "TEST ENTRY NUM", BaseConsolWrapper.ConsolCRN);

				transport.JW_RL_NKLoadPort = "HKHKG";

				AssertEquals("CRN", "TEST ENTRY NUM", BaseConsolWrapper.CustomsEntryNumberForExportOnly);
				AssertEquals("ConsolCRN", "TEST ENTRY NUM", BaseConsolWrapper.ConsolCRN);

				Consol.JK_CRN = "F-W98-0808-8-AU-SYD-W987";
				AssertEquals("W987", BaseConsolWrapper.CRNCarrierNumer);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry.Code);
			}
		}

		public void TestIsAir()
		{
			Consol.JK_TransportMode = "";
			AssertEquals("IsAir", false, BaseConsolWrapper.IsAir);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("IsAir", true, BaseConsolWrapper.IsAir);
		}

		public void TestIsSea()
		{
			Consol.JK_TransportMode = "";
			AssertEquals("IsSea", false, BaseConsolWrapper.IsSea);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("IsSea", true, BaseConsolWrapper.IsSea);
		}

		public void TestIsNeutralMaster()
		{
			Consol.JK_IsNeutralMaster = ZBool.False;
			AssertEquals("IsNeutralMaster", false, BaseConsolWrapper.IsNeutralMaster);

			Consol.JK_IsNeutralMaster = ZBool.True;
			AssertEquals("IsNeutralMaster", true, BaseConsolWrapper.IsNeutralMaster);
		}

		public void TestIsDirect()
		{
			Consol.JK_AgentType = "";
			AssertEquals("IsDirect", false, BaseConsolWrapper.IsDirect);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("IsDirect", true, BaseConsolWrapper.IsDirect);
		}

		public void TestIsCoLoad()
		{
			Consol.JK_AgentType = "";
			AssertEquals("IsCoLoad", false, BaseConsolWrapper.IsCoLoad);

			Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AssertEquals("IsCoLoad", true, BaseConsolWrapper.IsCoLoad);
		}

		public void TestIsGatewayCoLoad()
		{
			Consol.JK_AgentType = ZString.Empty;
			AssertEquals("IsGatewayCoLoad should be false", false, BaseConsolWrapper.IsGatewayCoLoad);

			Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			AssertEquals("IsGatewayCoLoad should be true", true, BaseConsolWrapper.IsGatewayCoLoad);
		}

		public void TestIsCoLoadOrGatewayCoLoad()
		{
			Consol.JK_AgentType = ZString.Empty;
			AssertEquals("IsCoLoadOrGatewayCoLoad should be false", false, BaseConsolWrapper.IsCoLoadOrGatewayCoLoad);

			Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AssertEquals("IsCoLoadOrGatewayCoLoad should be true", true, BaseConsolWrapper.IsCoLoadOrGatewayCoLoad);

			Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			AssertEquals("IsCoLoadOrGatewayCoLoad should be true", true, BaseConsolWrapper.IsCoLoadOrGatewayCoLoad);
		}

		public void TestIsAgent()
		{
			Consol.JK_AgentType = "";
			AssertEquals("IsAgent", false, BaseConsolWrapper.IsAgent);

			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AssertEquals("IsAgent", true, BaseConsolWrapper.IsAgent);
		}

		public void TestIsAgentGateway()
		{
			Consol.JK_AgentType = "";
			Assert(!BaseConsolWrapper.IsAgentGateway);

			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			Assert(BaseConsolWrapper.IsAgentGateway);
		}

		public void TestIsGatewayConsolType_GatewayAgent()
		{
			TestIsGatewayConsolType(Core.Constants.AgentType.Agent);
		}

		public void TestIsGatewayConsolType_GatewayCoload()
		{
			TestIsGatewayConsolType(Core.Constants.AgentType.CoLoad);
		}

		void TestIsGatewayConsolType(string gatewayType)
		{
			Consol.JK_AgentType = "";
			Assert(!BaseConsolWrapper.IsGatewayConsolType);

			Consol.JK_AgentType = gatewayType;
			Assert(!BaseConsolWrapper.IsGatewayConsolType);

			Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			Assert(BaseConsolWrapper.IsGatewayConsolType);
		}

		public void TestIsCharter()
		{
			Consol.JK_AgentType = "";
			AssertEquals("IsCharter", false, BaseConsolWrapper.IsCharter);

			Consol.JK_AgentType = Core.Constants.AgentType.Charter;
			AssertEquals("IsCharter", true, BaseConsolWrapper.IsCharter);
		}

		public void TestIsOther()
		{
			Consol.JK_AgentType = "";
			AssertEquals("IsOther", false, BaseConsolWrapper.IsOther);

			Consol.JK_AgentType = Core.Constants.AgentType.Other;
			AssertEquals("IsOther", true, BaseConsolWrapper.IsOther);
		}

		public void TestIsImportConsol()
		{
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKDiscPort = "";
			AssertEquals("IsImportConsol", false, BaseConsolWrapper.IsImportConsol);

			transport.JW_RL_NKDiscPort = GetAUnLocoCodeForCurrentCountry();
			AssertEquals("IsImportConsol", true, BaseConsolWrapper.IsImportConsol);
		}

		public void TestIsExportConsol()
		{
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "";
			AssertEquals("IsExportConsol", false, BaseConsolWrapper.IsExportConsol);

			transport.JW_RL_NKLoadPort = GetAUnLocoCodeForCurrentCountry();
			AssertEquals("IsExportConsol", true, BaseConsolWrapper.IsExportConsol);
		}

		public void TestIsPrepaid()
		{
			Consol.JK_PrepaidCollect = "";
			AssertEquals("IsPrepaid", false, BaseConsolWrapper.IsPrepaid);

			Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			AssertEquals("IsPrepaid", true, BaseConsolWrapper.IsPrepaid);
		}

		public void TestIsCollect()
		{
			Consol.JK_PrepaidCollect = "";
			AssertEquals("IsCollect", false, BaseConsolWrapper.IsCollect);

			Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			AssertEquals("IsCollect", true, BaseConsolWrapper.IsCollect);
		}

		public void TestShippingLine()
		{
			AssertNull("No shipping line", BaseConsolWrapper.ShippingLine);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.NotEqual, ""));
			header.MainAddress.OA_Address1 = "MAIN ADDRESS";
			OrgAddress addr2 = header.Addresses.AddNew();
			addr2.OA_Address1 = "SECOND ADDRESS";

			Consol.JK_OA_ShippingLineAddress = addr2.PK;
			AssertNotNull("Shipping line shouldn't be null", BaseConsolWrapper.ShippingLine);
			AssertEquals("Shipping line name", header.OH_FullName, BaseConsolWrapper.ShippingLine.Name);
			AssertEquals("Shipping line address 1", "SECOND ADDRESS", BaseConsolWrapper.ShippingLine.SelectedAddress.Address1);
		}

		public void TestCreditor()
		{
			AssertNull("Creditor should be null", BaseConsolWrapper.Creditor);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Consol.JK_OA_CreditorAddress = header.MainAddress.PK;
			AssertNotNull("Creditor shouldn't be null", BaseConsolWrapper.Creditor);
			AssertEquals("Creditor name", header.OH_FullName, BaseConsolWrapper.Creditor.Name);
			AssertEquals("Creditor address 1", header.MainAddress.OA_Address1, BaseConsolWrapper.Creditor.Address1);
		}

		public void TestShippingLineFromSailing()
		{
			AssertNull(BaseConsolWrapper.ShippingLine);

			var sailing = Factory.New<JobSailing>();
			var voyage = Factory.New<JobVoyage>();
			var origin = Factory.New<VoyageOrigin>();
			var destination = Factory.New<VoyageDestination>();

			var line = Factory.New<OrgHeader>();
			line.OH_IsShippingLine = ZBool.True;
			line.OH_Code = "SHP TEST";

			voyage.JV_OH_Line = line.PK;
			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			Transport transport = Consol.Transports[0];
			transport.JW_JX = sailing.PK;
			AssertEquals("SHP TEST", BaseConsolWrapper.ShippingLine.Code);
		}

		public void TestHBLOrHAWBHeading()
		{
			AssertEquals("Heading should be Sea heading", "HBL:", BaseConsolWrapper.HBLOrHAWBHeading);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Heading should be Air heading", "HAWB:", BaseConsolWrapper.HBLOrHAWBHeading);
		}

		public void TestCRNHeading()
		{
			AssertEquals("CRN heading is blank", ZString.Empty, BaseConsolWrapper.CRNHeading);

			var uSUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, "US"));

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = uSUNLOCO.RL_Code;

			RefUNLOCO uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			transport.JW_RL_NKDiscPort = uNLOCO.RL_Code;
			AssertEquals("CRN Heading is blank as it is an import consol", ZString.Empty, BaseConsolWrapper.CRNHeading);

			transport.JW_RL_NKLoadPort = uNLOCO.RL_Code;
			transport.JW_RL_NKDiscPort = uSUNLOCO.RL_Code;

			AssertEquals("CRN Heading not blank as it is an export consol", "CRN", BaseConsolWrapper.CRNHeading);
			RefCountry oldCountry = GlbCompany.CurrentCompany.Country;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);

				RefUNLOCO icelandUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));

				transport.JW_RL_NKLoadPort = icelandUNLOCO.RL_Code;
				transport.JW_RL_NKDiscPort = uSUNLOCO.RL_Code;

				AssertEquals("CRN Heading not blank", "CRN", BaseConsolWrapper.CRNHeading);

				transport.JW_RL_NKLoadPort = uSUNLOCO.RL_Code;
				transport.JW_RL_NKDiscPort = icelandUNLOCO.RL_Code;

				AssertEquals("CRN Heading not blank", "CRN", BaseConsolWrapper.CRNHeading);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry.Code);
			}
		}

		public void TestTransportHeading()
		{
			AssertEquals("Transport heading", ZString.Empty, BaseConsolWrapper.TransportHeading);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Transport heading", "FLIGHT & DATE", BaseConsolWrapper.TransportHeading);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Transport heading", "VESSEL / VOYAGE / IMO(Lloyds)", BaseConsolWrapper.TransportHeading);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("Transport heading", "JOURNEY NAME / JOURNEY NUMBER", BaseConsolWrapper.TransportHeading);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("Transport heading", "JOURNEY NAME / JOURNEY NUMBER", BaseConsolWrapper.TransportHeading);
		}

		public void TestMasterBillHeading()
		{
			AssertEquals("Default master bill heading", "MASTER", BaseConsolWrapper.MasterBillHeading);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("master bill heading for air", "MAWB", BaseConsolWrapper.MasterBillHeading);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("master bill heading for sea", "OCEAN BILL OF LADING", BaseConsolWrapper.MasterBillHeading);
		}

		public void TestMasterBillAndIssueHeading()
		{
			AssertEquals("Default master bill heading", "MASTER", BaseConsolWrapper.MasterBillAndIssueHeading);

			Consol.JK_MasterBillIssueDate = ZDateTime.Today;
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Default master bill heading for sea - no issue", "OCEAN BILL OF LADING", BaseConsolWrapper.MasterBillAndIssueHeading);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("master bill and issue heading", "MAWB / ISSUE", BaseConsolWrapper.MasterBillAndIssueHeading);
		}

		public void TestPrepaidCollect()
		{
			ZString prepaidCollect = new ZString("PPP");
			Consol.JK_PrepaidCollect = prepaidCollect;
			AssertEquals("PrepaidCollect", prepaidCollect, BaseConsolWrapper.PrepaidCollect);
		}

		public void TestPrepaidCollectDescription()
		{
			Consol.JK_PrepaidCollect = "";
			AssertEquals("Prepaid collect description", ZString.Empty, BaseConsolWrapper.PrepaidCollectDescription);

			Consol.JK_PrepaidCollect = "CCX";
			ZString expectedResult = Consol.JK_PrepaidCollect_List.GetDescriptionFromCode(Consol.JK_PrepaidCollect).ToUpper();
			AssertEquals("Prepaid collect description", expectedResult, BaseConsolWrapper.PrepaidCollectDescription);
		}

		public void TestConsolTransport()
		{
			AssertEquals("Transport", ZString.Empty, BaseConsolWrapper.ConsolTransportInfo);
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport.JW_VoyageFlight = "VOY123";

			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";
			Transport.JW_Vessel = vessel.RV_Code;
			AssertEquals("Transport", vessel.RV_Code + " / " + "VOY123" + " / " + vessel.RV_LloydsNumber, BaseConsolWrapper.ConsolTransportInfo);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Rail;
			Transport.JW_VoyageFlight = "JOU123";
			Transport.JW_Vessel = vessel.RV_Code;
			AssertEquals("Transport", vessel.RV_Code + " / " + "JOU123", BaseConsolWrapper.ConsolTransportInfo);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			Transport.JW_VoyageFlight = "JOU123";
			Transport.JW_Vessel = vessel.RV_Code;
			AssertEquals("Transport", vessel.RV_Code + " / " + "JOU123", BaseConsolWrapper.ConsolTransportInfo);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Transport.JW_VoyageFlight = "FL123";
			RefUNLOCO uNLOCOBisObject = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			Transport.JW_RL_NKDiscPort = uNLOCOBisObject.RL_Code;
			AssertEquals("Transport", "FL123 / " + uNLOCOBisObject.RL_Code + " /    ", BaseConsolWrapper.ConsolTransportInfo);
		}

		public void TestTransportInfoWithDate()
		{
			AssertEquals("Transport", ZString.Empty, BaseConsolWrapper.TransportInfoWithDate);
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport.JW_VoyageFlight = "VOY123";
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			Transport.JW_Vessel = vessel.RV_Code;
			Transport.JW_ETD = new ZDateTime(2009, 1, 2, 3, 4, 5);
			AssertEquals("Transport", vessel.RV_Code + " / " + "VOY123" + " / 02-Jan-09", BaseConsolWrapper.TransportInfoWithDate);

			Transport.JW_ATD = new ZDateTime(2009, 6, 7, 8, 9, 10);
			AssertEquals("Transport", vessel.RV_Code + " / " + "VOY123" + " / 07-Jun-09", BaseConsolWrapper.TransportInfoWithDate);

			Transport.JW_ATD = ZDateTime.Empty;

			Consol.JK_TransportMode = Core.Constants.TransportModes.Rail;
			Transport.JW_VoyageFlight = "JOU123";
			Transport.JW_Vessel = vessel.RV_Code;
			AssertEquals("Transport", vessel.RV_Code + " / " + "JOU123 / 02-Jan-09 03:04", BaseConsolWrapper.TransportInfoWithDate);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			Transport.JW_VoyageFlight = "JOU123";
			Transport.JW_Vessel = vessel.RV_Code;
			AssertEquals("Transport", vessel.RV_Code + " / " + "JOU123 / 02-Jan-09 03:04", BaseConsolWrapper.TransportInfoWithDate);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Transport.JW_VoyageFlight = "FL123";
			RefUNLOCO uNLOCOBisObject = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			Transport.JW_RL_NKDiscPort = uNLOCOBisObject.RL_Code;
			AssertEquals("Transport", "FL123 / " + uNLOCOBisObject.RL_Code + " / 02-Jan-09 03:04", BaseConsolWrapper.TransportInfoWithDate);

			Transport.JW_ATD = new ZDateTime(2009, 6, 7, 8, 9, 10);
			AssertEquals("Transport", "FL123 / " + uNLOCOBisObject.RL_Code + " / 07-Jun-09 08:09", BaseConsolWrapper.TransportInfoWithDate);
		}

		public void TestDepartureTime()
		{
			AssertEquals(ZDateTime.Empty, BaseConsolWrapper.DepartureTime);
			Transport.JW_ETD = DateTime.Today;
			AssertEquals(Consol.JK_JX_JA_E_DEP, BaseConsolWrapper.DepartureTime);

			Transport.JW_ATD = DateTime.Today.AddDays(5);
			AssertEquals(Consol.JK_JX_JA_A_DEP, BaseConsolWrapper.DepartureTime);
		}

		public void TestSailing()
		{
			AssertNull(BaseConsolWrapper.Sailing);

			var sailing = Factory.New<JobSailing>();
			var voyage = Factory.New<JobVoyage>();
			var origin = Factory.New<VoyageOrigin>();
			var destination = Factory.New<VoyageDestination>();

			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			voyage.JV_VoyageFlight = "VOY123";

			Transport.JW_JX = sailing.PK;
			AssertEquals("VOY123", BaseConsolWrapper.Sailing.Voyage.VoyageFlight);
		}

		public void TestFCLCutOff()
		{
			AssertEquals(ZDateTime.Empty, BaseConsolWrapper.FCLCutOff);
			Transport.JW_TerminalCutOff = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, BaseConsolWrapper.FCLCutOff);
		}

		public void TestLCLCutOff()
		{
			AssertEquals(ZDateTime.Empty, BaseConsolWrapper.LCLCutOff);
			Transport.JW_DepotCutOff = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, BaseConsolWrapper.LCLCutOff);
		}

		public void TestAvailabilityDate()
		{
			AssertEquals(ZDateTime.Empty, BaseConsolWrapper.AvailabilityDate);
			Transport.JW_TerminalAvailabilityDate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, BaseConsolWrapper.AvailabilityDate);
		}

		public void TestStorageCommences()
		{
			AssertEquals(ZDateTime.Empty, BaseConsolWrapper.StorageCommences);
			Transport.JW_TerminalStorageDate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, BaseConsolWrapper.StorageCommences);
		}

		public void TestLCLAvailabilityDate()
		{
			AssertEquals(ZDateTime.Empty, BaseConsolWrapper.LCLAvailabilityDate);
			Transport.JW_DepotAvailabilityDate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, BaseConsolWrapper.LCLAvailabilityDate);
		}

		public void TestLCLStorageCommences()
		{
			AssertEquals(ZDateTime.Empty, BaseConsolWrapper.LCLStorageCommences);
			Transport.JW_DepotStorageDate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, BaseConsolWrapper.LCLStorageCommences);
		}

		public void TestBondDate()
		{
			ZDateTime eTADate;
			ZDateTime expectedDate;
			ZDateTime.TryParseExact("17/04/1982", out eTADate, "dd/MM/yyyy");
			ZDateTime.TryParseExact("31/05/1982", out expectedDate, "dd/MM/yyyy");
			Transport.JW_ETA = eTADate;
			AssertEquals("Bond Date", expectedDate, BaseConsolWrapper.BondDate);
		}
		public void TestContainerCollections()
		{
			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTC";
			CommonContainer container2 = Consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTB";
			CommonContainer container3 = Consol.Containers.AddNew();
			container3.JC_ContainerNum = "CONTA";

			AssertEquals("CONTC", BaseConsolWrapper.ContainersNoSortingOrder[0].ContainerNumber);
			AssertEquals("CONTB", BaseConsolWrapper.ContainersNoSortingOrder[1].ContainerNumber);
			AssertEquals("CONTA", BaseConsolWrapper.ContainersNoSortingOrder[2].ContainerNumber);

			AssertEquals("CONTA", BaseConsolWrapper.Containers[0].ContainerNumber);
			AssertEquals("CONTB", BaseConsolWrapper.Containers[1].ContainerNumber);
			AssertEquals("CONTC", BaseConsolWrapper.Containers[2].ContainerNumber);
		}

		public void TestDateFirstForeignPort()
		{
			AssertEquals(ZDateTime.Empty, BaseConsolWrapper.DateFirstForeignPort);
			Consol.JK_DateFirstForeignPort = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, BaseConsolWrapper.DateFirstForeignPort);
		}

		public void TestDateLastForeignPort()
		{
			AssertEquals(ZDateTime.Empty, BaseConsolWrapper.DateLastForeignPort);
			Consol.JK_DateLastForeignPort = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, BaseConsolWrapper.DateLastForeignPort);
		}

		public void TestDatePortOfFirstArrival()
		{
			AssertEquals(ZDateTime.Empty, BaseConsolWrapper.DatePortOfFirstArrival);
			Consol.JK_DatePortOfFirstArrival = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, BaseConsolWrapper.DatePortOfFirstArrival);
		}

		public void TestLCLReceivalCommences()
		{
			AssertEquals(ZDateTime.Empty, BaseConsolWrapper.LCLReceivalCommences);
			Transport.JW_DepotReceivalCommences = ZDateTime.Today;
			AssertEquals("Should come from the export transport", Transport.JW_DepotReceivalCommences, BaseConsolWrapper.LCLReceivalCommences);
		}

		public void TestFCLReceivalCommences()
		{
			AssertEquals(ZDateTime.Empty, BaseConsolWrapper.FCLReceivalCommences);
			Transport.JW_TerminalReceivalCommences = ZDateTime.Today;
			AssertEquals("Should come from the export transport", Transport.JW_TerminalReceivalCommences, BaseConsolWrapper.FCLReceivalCommences);
		}

		public void TestStorageDate()
		{
			AssertEquals(ZDateTime.Empty, BaseConsolWrapper.StorageDate);
			Transport.JW_TerminalStorageDate = ZDateTime.Today;
			AssertEquals("Should come from the import transport", ZDateTime.Today, BaseConsolWrapper.StorageDate);
		}

		public void TestTransportPlanningCount()
		{
			AssertEquals("TransportPlanningCount", 1, BaseConsolWrapper.TransportPlanningCount);

			Consol.Transports.AddNew();
			AssertEquals("TransportPlanningCount", 2, BaseConsolWrapper.TransportPlanningCount);

			Consol.Transports.RemoveAndDeleteAll();
			AssertEquals("TransportPlanningCount", 1, BaseConsolWrapper.TransportPlanningCount);
		}

		public void TestTransportPlanningSortedByTransportType()
		{
			DocBaseConsolWithTransportPlanningExposed baseConsolWrapper = new DocBaseConsolWithTransportPlanningExposed(Consol, Factory);

			Transport originalTransport = Consol.Transports[0];
			originalTransport.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Flight1;
			Transport transport1 = Consol.Transports.AddNew();
			transport1.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Flight2;
			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Other;
			AssertEquals("Count of Transports should be 2", 2, baseConsolWrapper.TransportPlanningSortedByTransportType.Count);
			AssertEquals("First Transport should be Original Transport", originalTransport.JW_TransportType, baseConsolWrapper.TransportPlanningSortedByTransportType[0].TransportType);
			AssertEquals("Second Transport should beTransport1", transport1.JW_TransportType, baseConsolWrapper.TransportPlanningSortedByTransportType[1].TransportType);

			originalTransport.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Other;
			transport2.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Flight1;
			baseConsolWrapper = new DocBaseConsolWithTransportPlanningExposed(Consol, Factory);
			AssertEquals("First Transport should be Transport2", transport2.JW_TransportType, baseConsolWrapper.TransportPlanningSortedByTransportType[0].TransportType);
			AssertEquals("Second Transport should beTransport1", transport1.JW_TransportType, baseConsolWrapper.TransportPlanningSortedByTransportType[1].TransportType);
		}

		public void TestTransportPlanningOtherSortedByETD()
		{
			DocBaseConsolWithTransportPlanningExposed baseConsolWrapper = new DocBaseConsolWithTransportPlanningExposed(Consol, Factory);

			Transport originalTransport = Consol.Transports[0];
			originalTransport.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Flight1;
			Transport transport1 = Consol.Transports.AddNew();
			transport1.JW_ETD = DateTime.Today;
			transport1.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Other;
			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_ETD = DateTime.Today.AddDays(1);
			transport2.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Other;
			AssertEquals("Count of Transports should be 2", 2, baseConsolWrapper.TransportPlanningOtherSortedByETD.Count);
			AssertEquals("First Transport should be Transport1", transport1.JW_ETD, baseConsolWrapper.TransportPlanningOtherSortedByETD[0].ETD);
			AssertEquals("Second Transport should beTransport2", transport2.JW_ETD, baseConsolWrapper.TransportPlanningOtherSortedByETD[1].ETD);

			transport1.JW_ETD = DateTime.Today.AddDays(2);
			originalTransport.JW_ETD = DateTime.Today;
			originalTransport.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.Other;
			baseConsolWrapper = new DocBaseConsolWithTransportPlanningExposed(Consol, Factory);
			AssertEquals("First Transport should be OriginalTransport", originalTransport.JW_ETD, baseConsolWrapper.TransportPlanningOtherSortedByETD[0].ETD);
			AssertEquals("Second Transport should beTransport2", transport2.JW_ETD, baseConsolWrapper.TransportPlanningOtherSortedByETD[1].ETD);
		}

		public void TestNKFirstForeignPort()
		{
			ZString nKFirstForeignPort = new ZString("AUSYD");
			Consol.JK_RL_NKFirstForeignPort = nKFirstForeignPort;
			AssertEquals("NKFirstForeignPort", nKFirstForeignPort, BaseConsolWrapper.NKFirstForeignPort);
		}

		public void TestNKLastForeignPort()
		{
			ZString nKLastForeignPort = new ZString("AUSYD");
			Consol.JK_RL_NKLastForeignPort = nKLastForeignPort;
			AssertEquals("NKLastForeignPort", nKLastForeignPort, BaseConsolWrapper.NKLastForeignPort);
		}

		public void TestNKPortOfFirstArrival()
		{
			ZString nKPortOfFirstArrival = new ZString("AUSYD");
			Consol.JK_RL_NKPortOfFirstArrival = nKPortOfFirstArrival;
			AssertEquals("NKPortOfFirstArrival", nKPortOfFirstArrival, BaseConsolWrapper.NKPortOfFirstArrival);
		}

		public void TestSecondJobNumber()
		{
			AssertEquals("", BaseConsolWrapper.SecondJobNumber);
		}

		public void TestSecondJobHeading()
		{
			AssertEquals("", BaseConsolWrapper.SecondJobNumberHeading);
		}

		public void TestRequestForProfitShareOpeningText()
		{
			string value = "Consol Opening Text Request for Profit Share";
			DocumentsDataRegistry.Instance.RequestForProfitShareOpeningText_Consol.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			AssertEquals("Opening Text", "Consol Opening Text Request for Profit Share", BaseConsolWrapper.RequestForProfitShareOpeningText);
		}

		public void TestRequestForProfitShareDocumentHeader()
		{
			AssertEquals("Request for Consol Profit Share Credit Note", BaseConsolWrapper.RequestForProfitShareDocumentHeader);
		}

		public void TestContainerInfoForInvoice()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CON1111";
			container1.JC_ContainerMode = "FCL";
			DocBaseConsolTestClass consolWrapper = new DocBaseConsolTestClass(consol, Factory);
			ZString result = "CON1111/FCL";
			AssertEquals("Container Info For Invoice should be entered", result, consolWrapper.ContainerInfoForInvoice);
			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CON2222";
			container2.JC_ContainerMode = "FCL";
			RefContainer ref1 = Factory.New<RefContainer>();
			ref1.RC_Code = "20FR";
			container2.JC_RC = ref1.PK;
			result = "CON1111/FCL, CON2222/FCL/20FR";
			AssertEquals("Container Info For Invoice should be entered", result, consolWrapper.ContainerInfoForInvoice);
		}

		public void TestContainerInfoForInvoiceLine()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			DocBaseConsolTestClass consolWrapper = new DocBaseConsolTestClass(consol, Factory);
			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CON1111";
			container1.JC_ContainerMode = "FCL";
			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CON2222";
			container2.JC_ContainerMode = "ULD";
			CommonContainer container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "CON3333";
			container3.JC_ContainerMode = "FCL";
			RefContainer ref1 = Factory.New<RefContainer>();
			ref1.RC_Code = "20FR";
			container3.JC_RC = ref1.PK;
			CommonContainer container4 = consol.Containers.AddNew();
			container4.JC_ContainerNum = "CON4444";
			container4.JC_ContainerMode = "ULD";
			ZString result = "CON1111 (FCL), CON2222 (ULD), CON3333 (FCL/20FR), CON4444 (ULD)";
			AssertEquals("Container Info For Invoice Line should be entered", result, consolWrapper.ContainerInfoForInvoiceLine);
			CommonContainer container5 = consol.Containers.AddNew();
			container5.JC_ContainerNum = "CON5555";
			container5.JC_ContainerMode = "ULD";
			result = "CON1111 (FCL), CON2222 (ULD), CON3333 (FCL/20FR), CON4444 (ULD) ...";
			AssertEquals("Container Info For Invoice Line should be entered", result, consolWrapper.ContainerInfoForInvoiceLine);

			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			result = "See attached Container Details list.";
			AssertEquals("Container Info For Invoice Line should be entered", result, consolWrapper.ContainerInfoForInvoiceLine);
		}

		public void TestContainerNumberOnNewLine()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			CommonContainer container1 = consol.Containers.AddNew();
			consol.Containers.AddNew().JC_ContainerNum = "CON2222";
			DocBaseConsolTestClass consolWrapper = new DocBaseConsolTestClass(consol, Factory);
			ZString result = System.Environment.NewLine + "CON2222" + System.Environment.NewLine;
			AssertEquals("Container Number On New Line should be entered", result, consolWrapper.ContainerNumberOnNewLine);
			consol.Containers.AddNew().JC_ContainerNum = "CON3333";
			result = System.Environment.NewLine + "CON2222" + System.Environment.NewLine + "CON3333" + System.Environment.NewLine;
			AssertEquals("Container Number On New Line should be entered", result, consolWrapper.ContainerNumberOnNewLine);
		}

		public void TestContainerModeOnNewLine()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CON1111";
			container1.JC_ContainerMode = "FCL";
			DocBaseConsolTestClass consolWrapper = new DocBaseConsolTestClass(consol, Factory);
			ZString result = "FCL" + System.Environment.NewLine;
			AssertEquals("Container Mode On New Line should be entered", result, consolWrapper.ContainerModeOnNewLine);
			consol.Containers.AddNew().JC_ContainerNum = "CON2222";
			CommonContainer container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "CON3333";
			container3.JC_ContainerMode = "LCL";
			result = "FCL" + System.Environment.NewLine + System.Environment.NewLine + "LCL" + System.Environment.NewLine;
			AssertEquals("Container Mode On New Line should be entered", result, consolWrapper.ContainerModeOnNewLine);
		}

		public void TestContainerTypeOnNewLine()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CON1111";
			RefContainer ref1 = Factory.New<RefContainer>();
			ref1.RC_Code = "40FR";
			container1.JC_RC = ref1.PK;
			DocBaseConsolTestClass consolWrapper = new DocBaseConsolTestClass(consol, Factory);
			ZString result = "40FR" + System.Environment.NewLine;
			AssertEquals("Container Type On New Line should be entered", result, consolWrapper.ContainerTypeOnNewLine);
			consol.Containers.AddNew().JC_ContainerNum = "CON2222";
			CommonContainer container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "CON3333";
			RefContainer ref3 = Factory.New<RefContainer>();
			ref3.RC_Code = "30FR";
			container3.JC_RC = ref3.PK;
			result = "40FR" + System.Environment.NewLine + System.Environment.NewLine + "30FR" + System.Environment.NewLine;
			AssertEquals("Container Type On New Line should be entered", result, consolWrapper.ContainerTypeOnNewLine);
		}

		public void TestPrintPageWithContainerNumber()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			DocBaseConsolTestClass consolWrapper = new DocBaseConsolTestClass(consol, Factory);
			consol.Containers.AddNew().JC_ContainerNum = "CON1111";
			consol.Containers.AddNew().JC_ContainerNum = "CON2222";
			consol.Containers.AddNew().JC_ContainerNum = "CON3333";
			consol.Containers.AddNew().JC_ContainerNum = "CON4444";
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("PrintPageWithContainerNumber must be false", false, (bool)consolWrapper.PrintPageWithContainerNumber);
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("PrintPageWithContainerNumber must be false", false, (bool)consolWrapper.PrintPageWithContainerNumber);
			consol.Containers.AddNew().JC_ContainerNum = "CON5555";
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("PrintPageWithContainerNumber must be false", false, (bool)consolWrapper.PrintPageWithContainerNumber);
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("PrintPageWithContainerNumber must be true", true, (bool)consolWrapper.PrintPageWithContainerNumber);
		}

		public void TestContainerInfoForNotes()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CON1111";
			container1.JC_ContainerMode = "FCL";
			container1.JC_SealNum = "SS11";
			DocBaseConsolTestClass consolWrapper = new DocBaseConsolTestClass(consol, Factory);

			ZString result = "CON1111/FCL/SS11";
			AssertEquals("Container Info For Notes should be entered", result, consolWrapper.ContainerInfoForNotes);

			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CON2222";
			container2.JC_ContainerMode = "FCL";
			container2.JC_SealNum = "SS22";
			var ref1 = Factory.New<RefContainer>();
			ref1.RC_Code = "20FR";
			container2.JC_RC = ref1.PK;

			result = "CON1111/FCL/SS11, CON2222/FCL/SS22/20FR";
			AssertEquals("Container Info For Notes should be entered", result, consolWrapper.ContainerInfoForNotes);
		}

		#region STC Label For US Bound

		public void TestSTC_Label()
		{
			Transport originalTransport = Consol.Transports[0];
			Transport transport1 = Consol.Transports.AddNew();
			Transport transport2 = Consol.Transports.AddNew();
			originalTransport.JW_RL_NKLoadPort = "AUSYD";
			originalTransport.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_RL_NKLoadPort = "NZAKL";
			transport1.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_RL_NKLoadPort = "USLAX";
			transport2.JW_RL_NKDiscPort = "CACAD";

			AssertEquals("STC Label should not have STC", "", BaseConsolWrapper.STC_Label);

			transport1.JW_RL_NKDiscPort = "ZAAAM";
			transport2.JW_RL_NKLoadPort = "ZAAAM";

			AssertEquals("STC Label should have STC", "STC ", BaseConsolWrapper.STC_Label);

			Transport transport3 = Consol.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "USLAX";
			transport3.JW_RL_NKDiscPort = "AGANU";
			AssertEquals("STC Label should not have STC", "", BaseConsolWrapper.STC_Label);

			transport3.JW_RL_NKLoadPort = "AGANU";
			transport3.JW_RL_NKDiscPort = "AGANU";
			AssertEquals("STC Label should have STC", "STC ", BaseConsolWrapper.STC_Label);

			transport3.JW_RL_NKLoadPort = "AGANU";
			transport3.JW_RL_NKDiscPort = "USLAX";
			AssertEquals("STC Label should not have STC", "", BaseConsolWrapper.STC_Label);
		}

		#endregion

		#region DocBaseConsolWithTransportPlanningExposed

		class DocBaseConsolWithTransportPlanningExposed : DocBaseConsol
		{
			public DocBaseConsolWithTransportPlanningExposed(CommonConsol consol, BusinessObjectFactory factoryToWrap)
				: base(consol, factoryToWrap)
			{
			}
			public new DocTransportCollection TransportPlanningSortedByTransportType
			{
				get { return base.TransportPlanningSortedByTransportType; }
			}

			public new DocTransportCollection TransportPlanningOtherSortedByETD
			{
				get { return base.TransportPlanningOtherSortedByETD; }
			}
		}

		#endregion

		#region Implementation

		protected DocBaseConsolTestClass BaseConsolWrapper;
		protected CommonConsol Consol;
		protected Transport Transport;

		protected override void SetUp()
		{
			Consol = Factory.New<CommonConsol>();

			Transport = Consol.Transports[0];
			Transport.JW_IsLinked = true;

			BaseConsolWrapper = new DocBaseConsolTestClass(Consol, Factory);
			AssertNotNull("PreCondition: Valid DocBaseConsol", BaseConsolWrapper);
			BaseConsolWrapper.SetTemplateConstants(new Dictionary<string, object> { { DocumentEngineIntegration.Constants.TemplateDefined.ContactType, ContactType.Consignor.Code } });

			base.SetUp();
		}

		protected override void TearDown()
		{
			SuppressionForTest.CacheObjectClear();
			base.TearDown();
		}

		protected ZString GetAUnLocoCodeForCurrentCountry()
		{
			RefUNLOCO uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			return uNLOCO.RL_Code;
		}

		#endregion
	}
}
