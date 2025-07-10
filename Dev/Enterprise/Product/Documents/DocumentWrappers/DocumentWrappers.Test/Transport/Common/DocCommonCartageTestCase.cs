using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCommonCartage))]
	sealed class DocCommonCartageTestCase : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocCommonCartage.New(Cartage, Factory) };
		}

		#region Basic Cartage Fields

		public void TestDescription()
		{
			Cartage.JJ_GoodsDescription = "NY09";
			AssertEquals("NY09", CartageWrapper.Description);
		}

		[TestDate(2006, 1, 1)]
		public void TestPickupRequiredBy()
		{
			AssertEquals(ZDateTime.Empty, CartageWrapper.PickupRequiredBy);
			Cartage.JJ_EstimatedPickup = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, CartageWrapper.PickupRequiredBy);
		}

		[TestDate(2006, 1, 1)]
		public void TestActualJCL()
		{
			AssertEquals(ZDateTime.Empty, CartageWrapper.ActualJCL);
			Cartage.JJ_A_JCL = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, CartageWrapper.ActualJCL);
		}

		public void TestCartageType()
		{
			Cartage.JJ_E3_NKJobType = "FCI";
			AssertEquals("FCI", CartageWrapper.CartageType);
		}

		public void TestConsignmentID()
		{
			Cartage.JJ_ConsignmentID = "T938473";
			AssertEquals("T938473", CartageWrapper.ConsignmentID);
		}

		public void TestBranch()
		{
			Cartage.JJ_GB = GlbBranch.CurrentBranch.PK;
			AssertNotNull(CartageWrapper.Branch);
		}

		public void TestGoodsDescription()
		{
			Cartage.JJ_GoodsDescription = "Anatani Bakayaro";
			AssertEquals("Anatani Bakayaro", CartageWrapper.GoodsDescription);
		}

		public void TestAddresses()
		{
			AssertNull(Cartage.FirstDocAddress.Address);
			AssertNull(Cartage.SecondDocAddress.Address);
			AssertNull(Cartage.ThirdDocAddress.Address);
			AssertNull(Cartage.FourthDocAddress);

			var header1 = Factory.New<OrgHeader>();
			header1.OH_Code = "CODE1";
			OrgAddress address1 = header1.Addresses.AddNew();
			address1.OA_Address1 = "Address 1";

			var header2 = Factory.New<OrgHeader>();
			header2.OH_Code = "CODE2";
			OrgAddress address2 = header2.Addresses.AddNew();
			address2.OA_Address1 = "Address 2";

			var header3 = Factory.New<OrgHeader>();
			header3.OH_Code = "CODE2";
			OrgAddress address3 = header3.Addresses.AddNew();
			address3.OA_Address1 = "Address 3";

			var header4 = Factory.New<OrgHeader>();
			header4.OH_Code = "CODE2";
			OrgAddress address4 = header4.Addresses.AddNew();
			address4.OA_Address1 = "Address 4";

			Cartage.FirstDocAddress.E2_OA_Address = address3.PK;
			Cartage.SecondDocAddress.E2_OA_Address = address1.PK;
			Cartage.ThirdDocAddress.E2_OA_Address = address2.PK;

			AssertEquals("Address 3", CartageWrapper.FirstAddress.Address1);
			AssertEquals("Address 1", CartageWrapper.SecondAddress.Address1);
			AssertEquals("Address 2", CartageWrapper.ThirdAddress.Address1);
			AssertNull(CartageWrapper.FourthAddress);

			AssertEquals("CTO", CartageWrapper.FirstAddressHeading);
			AssertEquals("Consignee", CartageWrapper.SecondAddressHeading);
			AssertEquals("Container Yard", CartageWrapper.ThirdAddressHeading);
			AssertEquals("", CartageWrapper.FourthAddressHeading);
		}

		public void TestOrderReferenceNumber()
		{
			AssertEquals("", CartageWrapper.OrderReferenceNumber);
			Cartage.JJ_OrderReferenceNumber = "Order 384092";
			AssertEquals("Order 384092", CartageWrapper.OrderReferenceNumber);
		}

		public void TestOuterPacks()
		{
			AssertEquals(0, CartageWrapper.OuterPacks);
			Cartage.JJ_OuterPacks = 10;
			AssertEquals(10, CartageWrapper.OuterPacks);
		}

		public void TestOuterPacksType()
		{
			Cartage.JJ_F3_NKPackType = "BOX";
			AssertEquals("BOX", CartageWrapper.OuterPacksType);
		}

		public void TestQuoteNumber()
		{
			AssertEquals("", CartageWrapper.QuoteNumber);
			Cartage.JJ_QuoteNumber = "Q93929";
			AssertEquals("Q93929", CartageWrapper.QuoteNumber);
		}

		public void TestVessel()
		{
			AssertNull(CartageWrapper.Vessel);
			Cartage.JJ_JX_Sailing = Sailing.PK;
			AssertEquals(Sailing.JX_JV_NKVessel, CartageWrapper.Vessel.Code);
		}

		public void TestVolume()
		{
			AssertEquals(0M, CartageWrapper.Volume);
			Cartage.JJ_Volume = 300M;
			AssertEquals(300M, CartageWrapper.Volume);
		}

		public void TestVolumeUQ()
		{
			Cartage.JJ_VolumeUQ = "M3";
			AssertEquals("M3", CartageWrapper.VolumeUQ);
		}

		public void TestVoyage()
		{
			AssertEquals("", CartageWrapper.Voyage);
			Cartage.JJ_JX_Sailing = Sailing.PK;
			AssertEquals(Sailing.JX_JV_VoyageFlight, CartageWrapper.Voyage);
		}

		public void TestWaybillNumber()
		{
			AssertEquals("", CartageWrapper.WaybillNumber);
			Cartage.JJ_WaybillNumber = "WAYTOGOBILL";
			AssertEquals("WAYTOGOBILL", CartageWrapper.WaybillNumber);
		}

		public void TestWeight()
		{
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.DomesticLooseDelivery;
			AssertEquals("Precondition - Starting weight 0.", 0M, CartageWrapper.Weight);
			Cartage.JJ_Weight = 100M;
			AssertEquals("Expected weight to match goods weight for loose job.", 100M, CartageWrapper.Weight);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportUnpack;
			Cartage.ContainerBookedMoves.AddNew().Container.JC_GrossWeight = 235;
			AssertEquals("Expected weight to match gross weight for containerised job.", 235M, CartageWrapper.Weight);
		}

		/// <summary>
		/// Mixed Mode Job Type should return GoodsWeight when GrossWeight is not available - WI30852
		/// </summary>
		public void TestWeight_MixedJobTypeFallsBackToGoodsWeight()
		{
			var mixedJobType = Factory.New<CommonCartageType>();
			mixedJobType.E3_JobType = "LAM5";
			mixedJobType.E3_Description = "Some Description";

			var tiaDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "TIA"));
			mixedJobType.E3_GE = tiaDepartment.PK;

			var cnr = mixedJobType.CommonCartageOrganisations.AddNew();
			cnr.E5_OrgType = LocalCartageJobOrgTypeList.Codes.CNR;

			var cne = mixedJobType.CommonCartageOrganisations.AddNew();
			cne.E5_OrgType = LocalCartageJobOrgTypeList.Codes.CNE;

			var containerMove = mixedJobType.ContainerizedBooking;
			containerMove.E4_E5_FromOrg = cnr.PK;
			containerMove.E4_E5_ToOrg = cne.PK;
			var containerLeg = mixedJobType.ContainerizedCartageLegTypes.AddNew();
			containerLeg.E4_E5_FromOrg = cnr.PK;
			containerLeg.E4_E5_ToOrg = cne.PK;

			var looseMove = mixedJobType.LooseBooking;
			looseMove.E4_E5_FromOrg = cnr.PK;
			looseMove.E4_E5_ToOrg = cne.PK;
			var looseLeg = mixedJobType.LooseCartageLegTypes.AddNew();
			looseLeg.E4_E5_FromOrg = cnr.PK;
			looseLeg.E4_E5_ToOrg = cne.PK;

			Factory.Save();

			Cartage.JJ_E3_NKJobType = "LAM5";
			AssertEquals("Precondition - Mixed Job is Containerised.", true, Cartage.IsContainerised);
			AssertEquals("Precondition - Mixed Job is Loose.", true, Cartage.IsLoose);
			Cartage.JJ_Weight = 100m;
			AssertEquals("Expected weight to match goods weight for mixed job.", 100m, CartageWrapper.Weight);
		}

		public void TestWeightUQ()
		{
			Cartage.JJ_WeightUQ = "G";
			AssertEquals("G", CartageWrapper.WeightUQ);
		}

		public void TestAddress1Type()
		{
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			AssertEquals(DocAddressTypes.GetDescription(Factory, DocAddressType.LocalCartageCTO), CartageWrapper.Address1Type);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLImport;
			AssertEquals(DocAddressTypes.GetDescription(Factory, DocAddressType.LocalCartageCFS), CartageWrapper.Address1Type);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			AssertEquals(DocAddressTypes.GetDescription(Factory, DocAddressType.LocalCartageYard), CartageWrapper.Address1Type);
		}

		public void TestAddress2Type()
		{
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			AssertEquals(DocAddressTypes.GetDescription(Factory, DocAddressType.LocalCartageImporter), CartageWrapper.Address2Type);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			AssertEquals(DocAddressTypes.GetDescription(Factory, DocAddressType.LocalCartageCFS), CartageWrapper.Address2Type);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_EmptyCFStoCYD;
			AssertEquals(DocAddressTypes.GetDescription(Factory, DocAddressType.LocalCartageYard), CartageWrapper.Address2Type);
		}

		public void TestAddress3Type()
		{
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			AssertEquals(DocAddressTypes.GetDescription(Factory, DocAddressType.LocalCartageCTO), CartageWrapper.Address3Type);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirImport;
			AssertEquals(true, CartageWrapper.Address3Type.IsEmpty);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			AssertEquals(DocAddressTypes.GetDescription(Factory, DocAddressType.LocalCartageYard), CartageWrapper.Address3Type);
		}

		public void TestContainerLine()
		{
			AssertEquals("No containers", "", CartageWrapper.ContainerLine);

			var cont1 = Cartage.ContainerBookedMoves.AddNew().Container;
			var cont2 = Cartage.ContainerBookedMoves.AddNew().Container;
			var cont3 = Cartage.ContainerBookedMoves.AddNew().Container;
			var cont4 = Cartage.ContainerBookedMoves.AddNew().Container;
			var cont5 = Cartage.ContainerBookedMoves.AddNew().Container;
			var cont6 = Cartage.ContainerBookedMoves.AddNew().Container;
			var cont7 = Cartage.ContainerBookedMoves.AddNew().Container;
			var cont8 = Cartage.ContainerBookedMoves.AddNew().Container;

			cont1.JC_ContainerNum = "CONTNMBR1";
			cont2.JC_ContainerNum = "CONTNMBR2";
			cont3.JC_ContainerNum = "CONTNMBR3";
			cont4.JC_ContainerNum = "CONTNMBR4";
			cont5.JC_ContainerNum = "CONTNMBR5";
			cont6.JC_ContainerNum = "CONTNMBR6";

			ZString expectedContainerLine = "CONTNMBR1, CONTNMBR2, CONTNMBR3, CONTNMBR4, CONTNMBR5, CONTNMBR6";
			AssertEquals("Container Numbers (ContainerLine)", expectedContainerLine, CartageWrapper.ContainerLine);

			cont7.JC_ContainerNum = "CONTNMBR7";
			expectedContainerLine = "CONTNMBR1, CONTNMBR2, CONTNMBR3, CONTNMBR4, CONTNMBR5, CONTNMBR6, CONTNMBR7";
			AssertEquals("Container Numbers (ContainerLine)", expectedContainerLine, CartageWrapper.ContainerLine);

			cont8.JC_ContainerNum = "CONTNMBR8";
			expectedContainerLine = "CONTNMBR1, CONTNMBR2, CONTNMBR3, CONTNMBR4, CONTNMBR5, CONTNMBR6, CONTNMBR7 ...";
			AssertEquals("Container Numbers (ContainerLine)", expectedContainerLine, CartageWrapper.ContainerLine);
		}

		public void TestTransportHeading()
		{
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			AssertEquals("FLIGHT NO", CartageWrapper.TransportHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			AssertEquals("VESSEL / VOYAGE / IMO(Lloyds)", CartageWrapper.TransportHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirImport;
			AssertEquals("FLIGHT NO", CartageWrapper.TransportHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			AssertEquals("VESSEL / VOYAGE / IMO(Lloyds)", CartageWrapper.TransportHeading);
		}

		public void TestTransportInfo()
		{
			AssertEquals("Sailing.IsInDatabase", false, Sailing.IsInDatabase);
			Factory.Save();
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			AssertEquals("No flight entered", "", CartageWrapper.TransportInfo);
			Cartage.JJ_JX_Sailing = Flight.PK;
			AssertEquals("flight number", "QF33", CartageWrapper.TransportInfo);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			Cartage.JJ_JX_Sailing = Sailing.PK;
			Cartage.Vessel = "";
			AssertEquals(" / 234234 / ", CartageWrapper.TransportInfo);

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			Sailing.Voyage.JV_RV_NKVessel = vessel.RV_FK;
			Cartage.Vessel = vessel.RV_Code;
			AssertEquals(vessel.RV_Code + " / " + "234234" + " / " + vessel.RV_LloydsNumber, CartageWrapper.TransportInfo);
		}

		public void TestDropMode()
		{
			AssertEquals("Drop Mode", true, CartageWrapper.DropMode.IsEmpty);

			Cartage.JJ_DropMode = Cartage.Lookups.DropModes[0].Code;
			AssertEquals("EquipmentType", Cartage.Lookups.DropModes[0].Code + " - " + Cartage.Lookups.DropModes[0].Description, CartageWrapper.DropMode);
		}

		public void TestCutOffOrAvailableDate()
		{
			AssertEquals("Cut Off or Available Date", ZDateTime.Empty, CartageWrapper.CutOffOrAvailableDate);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			Cartage.JJ_JX_Sailing = Sailing.PK;
			Cartage.LCLCutOff = ZDateTime.Today;
			AssertEquals("Export + Loose", ZDateTime.Today, CartageWrapper.CutOffOrAvailableDate);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirImport;
			Cartage.JJ_JX_Sailing = Sailing.PK;
			Cartage.LCLAvailabilityDate = ZDateTime.Today.AddDays(1);
			AssertEquals("Not Export + Loose", ZDateTime.Today.AddDays(1), CartageWrapper.CutOffOrAvailableDate);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_DomesticContainerizedDelivery;
			Cartage.JJ_JX_Sailing = Sailing.PK;
			Cartage.FCLAvailabilityDate = ZDateTime.Today.AddDays(2);
			AssertEquals("Not Export + Not Loose", ZDateTime.Today.AddDays(2), CartageWrapper.CutOffOrAvailableDate);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_DomesticContainerizedPickup;
			Cartage.JJ_JX_Sailing = Sailing.PK;
			Cartage.FCLCutOff = ZDateTime.Today.AddDays(3);
			AssertEquals("Export + Not Loose", ZDateTime.Today.AddDays(3), CartageWrapper.CutOffOrAvailableDate);
		}

		public void TestPickupOrStorageCommenceDate()
		{
			AssertEquals("Cut Off or Available Date", ZDateTime.Empty, CartageWrapper.PickupOrStorageCommenceDate);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			Cartage.JJ_EstimatedPickup = ZDateTime.Today;
			AssertEquals("Export", ZDateTime.Today, CartageWrapper.PickupOrStorageCommenceDate);
			AssertEquals("Export Heading", "PICKUP DATE", CartageWrapper.PickupOrStorageCommenceDateHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirImport;
			Cartage.JJ_JX_Sailing = Sailing.PK;
			Cartage.LCLStorageDate = ZDateTime.Today.AddDays(1);
			AssertEquals("Not Export + Loose", ZDateTime.Today.AddDays(1), CartageWrapper.PickupOrStorageCommenceDate);
			AssertEquals("Not Export + Loose Heading", "STORAGE STARTS", CartageWrapper.PickupOrStorageCommenceDateHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_DomesticContainerizedDelivery;
			Cartage.JJ_JX_Sailing = Sailing.PK;
			Cartage.FCLStorageDate = ZDateTime.Today.AddDays(2);
			AssertEquals("Not Export + Not Loose", ZDateTime.Today.AddDays(2), CartageWrapper.PickupOrStorageCommenceDate);
			AssertEquals("Not Export + Not Loose Heading", "STORAGE STARTS", CartageWrapper.PickupOrStorageCommenceDateHeading);
		}

		public void TestIDocCartageAdviceDates()
		{
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			VoyageDestination destination = Factory.New<VoyageDestination>();
			Sailing.JX_JA = origin.PK;
			Sailing.JX_JB = destination.PK;

			origin.JA_CutOff = new ZDateTime(2011, 1, 18);
			origin.JA_ReceivalCommences = new ZDateTime(2011, 1, 22);
			destination.JB_AvailabilityDate = new ZDateTime(2011, 4, 18);
			destination.JB_StorageDate = new ZDateTime(2011, 4, 22);

			Sailing.JX_DepotCutOff = new ZDateTime(2011, 1, 25);
			Sailing.JX_DepotReceivalCommences = new ZDateTime(2011, 1, 20);
			Sailing.JX_DepotAvailabilityDate = new ZDateTime(2011, 4, 20);
			Sailing.JX_DepotStorageDate = new ZDateTime(2011, 4, 25);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_DomesticContainerizedDelivery;
			Cartage.JJ_JX_Sailing = Sailing.PK;
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 1, 18), CartageWrapper.CartageCutOffDate);
			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 4, 18), CartageWrapper.CartageAvailableDate);
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 1, 22), CartageWrapper.CartageReceivalDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 4, 22), CartageWrapper.CartageStorageCommenceDate);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirImport;
			Cartage.JJ_JX_Sailing = Sailing.PK;
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 1, 25), CartageWrapper.CartageCutOffDate);
			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 4, 20), CartageWrapper.CartageAvailableDate);
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 1, 20), CartageWrapper.CartageReceivalDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 4, 25), CartageWrapper.CartageStorageCommenceDate);
		}

		#endregion

		#region IDoc Cartage Advice

		#region Headings

		public void TestJourneyOnePickUpHeading()
		{
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_EmptyCFStoCYD;
			Cartage.ContainerBookedMoves.AddNew();
			AssertEquals("JourneyOnePickUpHeading: Empty container", "PICKUP EMPTY", CartageWrapper.JourneyOnePickUpHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLCTOtoCFS;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyOnePickUpHeading: Full container", "PICKUP FULL", CartageWrapper.JourneyOnePickUpHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportUnpack;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyOnePickUpHeading: Full container", "PICKUP FULL", CartageWrapper.JourneyOnePickUpHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyOnePickUpHeading: Empty container", "PICKUP EMPTY", CartageWrapper.JourneyOnePickUpHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyOnePickUpHeading: Full container", "PICKUP EMPTY", CartageWrapper.JourneyOnePickUpHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyOnePickUpHeading: Empty container", "PICKUP FULL", CartageWrapper.JourneyOnePickUpHeading);

			Cartage.JJ_EstimatedPickup = ZDateTime.Now;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyTwoPickUpHeading: Full container", "PICKUP FULL DATE: " + Cartage.JJ_EstimatedPickup.ToLongTimeString(), CartageWrapper.JourneyOnePickUpHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyTwoPickUpHeading: Empty container", "PICKUP EMPTY", CartageWrapper.JourneyOnePickUpHeading);
		}

		public void TestJourneyOneDeliverToHeading()
		{
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_EmptyCFStoCYD;
			Cartage.ContainerBookedMoves.AddNew();
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyOneDeliverToHeading: Empty container with date", "DELIVER TO EMPTY", CartageWrapper.JourneyOneDeliverToHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLCTOtoCFS;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyOneDeliverToHeading: Full container with date", "DELIVER TO FULL", CartageWrapper.JourneyOneDeliverToHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportUnpack;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyOneDeliverToHeading: Full container", "DELIVER TO FULL", CartageWrapper.JourneyOneDeliverToHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyOneDeliverToHeading: Empty container", "DELIVER TO EMPTY", CartageWrapper.JourneyOneDeliverToHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyOneDeliverToHeading: Full container", "DELIVER TO EMPTY", CartageWrapper.JourneyOneDeliverToHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyOneDeliverToHeading: Empty container", "DELIVER TO FULL", CartageWrapper.JourneyOneDeliverToHeading);

			Cartage.JJ_EstimatedDelivery = ZDateTime.Now;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyTwoPickUpHeading: Full container", "DELIVER TO FULL DATE: " + Cartage.JJ_EstimatedDelivery.ToLongTimeString(), CartageWrapper.JourneyOneDeliverToHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyTwoPickUpHeading: Empty container", "DELIVER TO EMPTY", CartageWrapper.JourneyOneDeliverToHeading);
		}

		public void TestJourneyTwoPickUpHeading()
		{
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_EmptyCFStoCYD;
			Cartage.ContainerBookedMoves.AddNew();
			AssertEquals("JourneyTwoPickUpHeading: Empty container, 1 journey", "", CartageWrapper.JourneyTwoPickUpHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLCTOtoCFS;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyTwoPickUpHeading: Full container, 1 journey", "", CartageWrapper.JourneyTwoPickUpHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportUnpack;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyTwoPickUpHeading: Empty container", "PICKUP EMPTY", CartageWrapper.JourneyTwoPickUpHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyTwoPickUpHeading: Full container", "PICKUP FULL", CartageWrapper.JourneyTwoPickUpHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyTwoPickUpHeading: Empty container", "PICKUP FULL", CartageWrapper.JourneyTwoPickUpHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyTwoPickUpHeading: Full container", "PICKUP EMPTY", CartageWrapper.JourneyTwoPickUpHeading);

			Cartage.JJ_EstimatedPickup = ZDateTime.Now;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyTwoPickUpHeading: Empty container", "PICKUP EMPTY", CartageWrapper.JourneyTwoPickUpHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyTwoPickUpHeading: Full container", "PICKUP FULL DATE: " + Cartage.JJ_EstimatedPickup.ToLongTimeString(), CartageWrapper.JourneyTwoPickUpHeading);
		}

		public void TestJourneyTwoDeliverToHeading()
		{
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_EmptyCFStoCYD;
			Cartage.ContainerBookedMoves.AddNew();
			AssertEquals("JourneyTwoDeliverToHeading: Empty container, 1 journey", "", CartageWrapper.JourneyTwoDeliverToHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLCTOtoCFS;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyTwoDeliverToHeading: Full container, 1 journey", "", CartageWrapper.JourneyTwoDeliverToHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportUnpack;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyTwoDeliverToHeading: Empty container", "DELIVER TO EMPTY", CartageWrapper.JourneyTwoDeliverToHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyTwoDeliverToHeading: Full container", "DELIVER TO FULL", CartageWrapper.JourneyTwoDeliverToHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyTwoDeliverToHeading: Empty container", "DELIVER TO FULL", CartageWrapper.JourneyTwoDeliverToHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyTwoDeliverToHeading: Full container", "DELIVER TO EMPTY", CartageWrapper.JourneyTwoDeliverToHeading);

			Cartage.JJ_EstimatedDelivery = ZDateTime.Now;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyTwoDeliverToHeading: Empty container", "DELIVER TO EMPTY", CartageWrapper.JourneyTwoDeliverToHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("JourneyTwoDeliverToHeading: Full container", "DELIVER TO FULL DATE: " + Cartage.JJ_EstimatedDelivery.ToLongTimeString(), CartageWrapper.JourneyTwoDeliverToHeading);
		}
		#endregion

		#region Contact Details

		public void TestJourneyOnePickUpContactDetails()
		{
			AssertEquals("FirstAddressContactName", "", CartageWrapper.JourneyOnePickUpContactName);
			AssertEquals("FirstAddressContactPhone", "", CartageWrapper.JourneyOnePickUpContactPhone);

			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			SetupForContactTests(Constants.CartageJobType.NEW_FCLImportToCNE);
			AssertEquals("FirstAddressContactName", "FirstName", CartageWrapper.JourneyOnePickUpContactName);
			AssertEquals("FirstAddressContactPhone", "FirstPhone", CartageWrapper.JourneyOnePickUpContactPhone);
		}

		public void TestJourneyOneDeliverToContactDetails()
		{
			AssertEquals("SecondAddressContactName", "", CartageWrapper.JourneyOneDeliverToContactName);
			AssertEquals("SecondAddressContactPhone", "", CartageWrapper.JourneyOneDeliverToContactPhone);

			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			SetupForContactTests(Constants.CartageJobType.NEW_FCLImportToCNE);
			AssertEquals("SecondAddressContactName", "SecondName", CartageWrapper.JourneyOneDeliverToContactName);
			AssertEquals("SecondAddressContactPhone", "SecondPhone", CartageWrapper.JourneyOneDeliverToContactPhone);
		}

		public void TestJourneyTwoPickUpContactDetails()
		{
			AssertEquals("SecondAddressContactName", "", CartageWrapper.JourneyTwoPickUpContactName);
			AssertEquals("SecondAddressContactPhone", "", CartageWrapper.JourneyTwoPickUpContactPhone);

			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			SetupForContactTests(Constants.CartageJobType.NEW_FCLImportToCNE);
			AssertEquals("SecondAddressContactName", "SecondName", CartageWrapper.JourneyTwoPickUpContactName);
			AssertEquals("SecondAddressContactPhone", "SecondPhone", CartageWrapper.JourneyTwoPickUpContactPhone);
		}

		public void TestJourneyTwoDeliverToContactDetails()
		{
			AssertEquals("ThirdAddressContactName", "", CartageWrapper.JourneyTwoDeliverToContactName);
			AssertEquals("ThirdAddressContactPhone", "", CartageWrapper.JourneyTwoDeliverToContactPhone);

			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			SetupForContactTests(Constants.CartageJobType.NEW_FCLImportToCNE);
			AssertEquals("ThirdAddressContactName", "ThirdName", CartageWrapper.JourneyTwoDeliverToContactName);
			AssertEquals("ThirdAddressContactPhone", "ThirdPhone", CartageWrapper.JourneyTwoDeliverToContactPhone);
		}

		#endregion

		#region TestJourneyAddresses

		public void TestJourneyAddresses_FCLPackLooseFromSHP()
		{
			var esmyCartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLPackLooseFromSHP, 2);
			AssertEquals("Pickup Loose from Consignor.", DocAddressType.LocalCartageExporter, esmyCartage.FirstDocAddress.DocAddressType);
			AssertEquals("Deliver Loose to CFS, also for packing Container.", DocAddressType.LocalCartageCFS, esmyCartage.SecondDocAddress.DocAddressType);
			AssertEquals("Pickup Empty Container from Container Yard.", DocAddressType.LocalCartageYard, esmyCartage.ThirdDocAddress.DocAddressType);
			AssertEquals("Deliver Full Container to CTO.", DocAddressType.LocalCartageCTO, esmyCartage.FourthDocAddress.DocAddressType);

			var esmyWrapper = DocCommonCartage.New(esmyCartage, Factory);
			AssertEquals("Should be CYD.", esmyCartage.ThirdDocAddress, esmyWrapper.JourneyOnePickUpAddress.WrappedObject);
			AssertEquals("Should be CFS.", esmyCartage.SecondDocAddress, esmyWrapper.JourneyOneDeliverToAddress.WrappedObject);
			AssertEquals("Should be CFS.", esmyCartage.SecondDocAddress, esmyWrapper.JourneyTwoPickUpAddress.WrappedObject);
			AssertEquals("Should be CTO.", esmyCartage.FourthDocAddress, esmyWrapper.JourneyTwoDeliverToAddress.WrappedObject);
		}

		public void TestJourneyAddresses_FCLUnpackLooseToCNE()
		{
			var ismyCartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLUnpackLooseToCNE, 2);
			AssertEquals("Pickup Full Container from CTO.", DocAddressType.LocalCartageCTO, ismyCartage.FirstDocAddress.DocAddressType);
			AssertEquals("Deliver Full Container to CFS for Unpacking, also for picking up Loose.", DocAddressType.LocalCartageCFS, ismyCartage.SecondDocAddress.DocAddressType);
			AssertEquals("Deliver Empty Container to Container Yard.", DocAddressType.LocalCartageYard, ismyCartage.ThirdDocAddress.DocAddressType);
			AssertEquals("Deliver Loose to CNE.", DocAddressType.LocalCartageImporter, ismyCartage.FourthDocAddress.DocAddressType);

			var ismyWrapper = DocCommonCartage.New(ismyCartage, Factory);
			AssertEquals("Should be CTO.", ismyCartage.FirstDocAddress, ismyWrapper.JourneyOnePickUpAddress.WrappedObject);
			AssertEquals("Should be CFS.", ismyCartage.SecondDocAddress, ismyWrapper.JourneyOneDeliverToAddress.WrappedObject);
			AssertEquals("Should be CFS.", ismyCartage.SecondDocAddress, ismyWrapper.JourneyTwoPickUpAddress.WrappedObject);
			AssertEquals("Should be CYD.", ismyCartage.ThirdDocAddress, ismyWrapper.JourneyTwoDeliverToAddress.WrappedObject);
		}

		public void TestJourneyAddresses_FCLExportToSHP()
		{
			var esccCartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLExportToSHP, 2);
			AssertEquals("Pickup Empty Container from Container Yard.", DocAddressType.LocalCartageYard, esccCartage.FirstDocAddress.DocAddressType);
			AssertEquals("Deliver Empty Container to Consignor, also Pick Up Full Container.", DocAddressType.LocalCartageExporter, esccCartage.SecondDocAddress.DocAddressType);
			AssertEquals("Deliver Full Container to CTO.", DocAddressType.LocalCartageCTO, esccCartage.ThirdDocAddress.DocAddressType);
			AssertNull("No fourth address.", esccCartage.FourthDocAddress);

			var esccWrapper = DocCommonCartage.New(esccCartage, Factory);
			AssertEquals("Should be CTO.", esccCartage.FirstDocAddress, esccWrapper.JourneyOnePickUpAddress.WrappedObject);
			AssertEquals("Should be Consignor.", esccCartage.SecondDocAddress, esccWrapper.JourneyOneDeliverToAddress.WrappedObject);
			AssertEquals("Should be Consignor.", esccCartage.SecondDocAddress, esccWrapper.JourneyTwoPickUpAddress.WrappedObject);
			AssertEquals("Should be CYD.", esccCartage.ThirdDocAddress, esccWrapper.JourneyTwoDeliverToAddress.WrappedObject);
		}

		public void TestJourneyAddresses_FCLImportToCNE()
		{
			var isccCartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 2);
			AssertEquals("Pickup Full Container from CTO.", DocAddressType.LocalCartageCTO, isccCartage.FirstDocAddress.DocAddressType);
			AssertEquals("Deliver Full Container to Consignee for Unpacking, also for Pick Up Empty Container.", DocAddressType.LocalCartageImporter, isccCartage.SecondDocAddress.DocAddressType);
			AssertEquals("Deliver Empty Container to Container Yard.", DocAddressType.LocalCartageYard, isccCartage.ThirdDocAddress.DocAddressType);
			AssertNull("No fourth address.", isccCartage.FourthDocAddress);

			var isccWrapper = DocCommonCartage.New(isccCartage, Factory);
			AssertEquals("Should be CTO.", isccCartage.FirstDocAddress, isccWrapper.JourneyOnePickUpAddress.WrappedObject);
			AssertEquals("Should be Consignee.", isccCartage.SecondDocAddress, isccWrapper.JourneyOneDeliverToAddress.WrappedObject);
			AssertEquals("Should be Consignee.", isccCartage.SecondDocAddress, isccWrapper.JourneyTwoPickUpAddress.WrappedObject);
			AssertEquals("Should be CYD.", isccCartage.ThirdDocAddress, isccWrapper.JourneyTwoDeliverToAddress.WrappedObject);
		}

		public void TestJourneyAddresses_AirExport()
		{
			var airExportCartage = Helper.CreateCartage(Constants.CartageJobType.NEW_AirExport, 2);
			AssertEquals("Pickup Loose Goods from Consignor.", DocAddressType.LocalCartageExporter, airExportCartage.FirstDocAddress.DocAddressType);
			AssertEquals("Deliver Loose Goods from CFS.", DocAddressType.LocalCartageCFS, airExportCartage.SecondDocAddress.DocAddressType);
			AssertNull("No third address.", airExportCartage.ThirdDocAddress);
			AssertNull("No fourth address.", airExportCartage.FourthDocAddress);

			var airExportWrapper = DocCommonCartage.New(airExportCartage, Factory);
			AssertEquals("Should be Consignor.", airExportCartage.FirstDocAddress, airExportWrapper.JourneyOnePickUpAddress.WrappedObject);
			AssertEquals("Should be CFS.", airExportCartage.SecondDocAddress, airExportWrapper.JourneyOneDeliverToAddress.WrappedObject);
			AssertNull("No Journey 2", airExportWrapper.JourneyTwoPickUpAddress);
			AssertNull("No Journey 2", airExportWrapper.JourneyTwoDeliverToAddress);
		}

		public void TestJourneyAddresses_AirImport()
		{
			var airImportCartage = Helper.CreateCartage(Constants.CartageJobType.NEW_AirImport, 2);
			AssertEquals("Pickup Loose Goods from CFS.", DocAddressType.LocalCartageCFS, airImportCartage.FirstDocAddress.DocAddressType);
			AssertEquals("Deliver Loose Goods from Consignee.", DocAddressType.LocalCartageImporter, airImportCartage.SecondDocAddress.DocAddressType);
			AssertNull("No third address.", airImportCartage.ThirdDocAddress);
			AssertNull("No fourth address.", airImportCartage.FourthDocAddress);

			var airImportWrapper = DocCommonCartage.New(airImportCartage, Factory);
			AssertEquals("Should be CFS.", airImportCartage.FirstDocAddress, airImportWrapper.JourneyOnePickUpAddress.WrappedObject);
			AssertEquals("Should be Consignee.", airImportCartage.SecondDocAddress, airImportWrapper.JourneyOneDeliverToAddress.WrappedObject);
			AssertNull("No Journey 2", airImportWrapper.JourneyTwoPickUpAddress);
			AssertNull("No Journey 2", airImportWrapper.JourneyTwoDeliverToAddress);
		}

		#endregion

		#region Pickup and Delivery ContactName and Phone

		#region JourneyOne

		public void TestJourneyOnePickUpContactName()
		{
			SetupCartageForPickupAndDeliveryContactNameAndPhone();
			JobDocAddress journeyOnePickupDocAddress = (JobDocAddress)CartageWrapper.JourneyOnePickUpAddress.WrappedObject;
			journeyOnePickupDocAddress.E2_Contact = "fAAA";
			AssertEquals("JourneyOnePickUpContactName", "fAAA", CartageWrapper.JourneyOnePickUpContactName);
			journeyOnePickupDocAddress.E2_Contact = "fBBB";
			AssertEquals("JourneyOnePickUpContactName", "fBBB", CartageWrapper.JourneyOnePickUpContactName);
		}

		public void TestJourneyOnePickUpContactPhone()
		{
			SetupCartageForPickupAndDeliveryContactNameAndPhone();
			JobDocAddress journeyOnePickupDocAddress = (JobDocAddress)CartageWrapper.JourneyOnePickUpAddress.WrappedObject;
			journeyOnePickupDocAddress.E2_Contact = "fAAA";
			AssertEquals("JourneyOnePickUpContactPhone", "f11", CartageWrapper.JourneyOnePickUpContactPhone);
			journeyOnePickupDocAddress.E2_Contact = "fBBB";
			AssertEquals("JourneyOnePickUpContactPhone", "f22", CartageWrapper.JourneyOnePickUpContactPhone);
		}

		public void TestJourneyOneDeliverToContactName()
		{
			SetupCartageForPickupAndDeliveryContactNameAndPhone();
			JobDocAddress journeyOneDeliverToDocAddress = (JobDocAddress)CartageWrapper.JourneyOneDeliverToAddress.WrappedObject;
			journeyOneDeliverToDocAddress.E2_Contact = "sAAA";
			AssertEquals("JourneyOneDeliverToContactName", "sAAA", CartageWrapper.JourneyOneDeliverToContactName);
			journeyOneDeliverToDocAddress.E2_Contact = "sBBB";
			AssertEquals("JourneyOneDeliverToContactName", "sBBB", CartageWrapper.JourneyOneDeliverToContactName);
		}

		public void TestJourneyOneDeliverToContactPhone()
		{
			SetupCartageForPickupAndDeliveryContactNameAndPhone();
			JobDocAddress journeyOneDeliverToDocAddress = (JobDocAddress)CartageWrapper.JourneyOneDeliverToAddress.WrappedObject;
			journeyOneDeliverToDocAddress.E2_Contact = "sAAA";
			AssertEquals("JourneyOneDeliverToContactPhone", "s11", CartageWrapper.JourneyOneDeliverToContactPhone);
			journeyOneDeliverToDocAddress.E2_Contact = "sBBB";
			AssertEquals("JourneyOneDeliverToContactPhone", "s22", CartageWrapper.JourneyOneDeliverToContactPhone);
		}

		#endregion

		#region Journey Two

		public void TestJourneyTwoPickUpContactName()
		{
			SetupCartageForPickupAndDeliveryContactNameAndPhone();
			JobDocAddress journeyTwoPickupDocAddress = (JobDocAddress)CartageWrapper.JourneyTwoPickUpAddress.WrappedObject;
			journeyTwoPickupDocAddress.E2_Contact = "sAAA";
			AssertEquals("JourneyTwoPickUpContactName", "sAAA", CartageWrapper.JourneyTwoPickUpContactName);
			journeyTwoPickupDocAddress.E2_Contact = "sBBB";
			AssertEquals("JourneyTwoPickUpContactName", "sBBB", CartageWrapper.JourneyTwoPickUpContactName);
		}

		public void TestJourneyTwoPickUpContactPhone()
		{
			SetupCartageForPickupAndDeliveryContactNameAndPhone();
			JobDocAddress journeyTwoPickupDocAddress = (JobDocAddress)CartageWrapper.JourneyTwoPickUpAddress.WrappedObject;
			journeyTwoPickupDocAddress.E2_Contact = "sAAA";
			AssertEquals("JourneyTwoPickUpContactPhone", "s11", CartageWrapper.JourneyTwoPickUpContactPhone);
			journeyTwoPickupDocAddress.E2_Contact = "sBBB";
			AssertEquals("JourneyTwoPickUpContactPhone", "s22", CartageWrapper.JourneyTwoPickUpContactPhone);
		}

		public void TestJourneyTwoDeliverToContactName()
		{
			SetupCartageForPickupAndDeliveryContactNameAndPhone();
			JobDocAddress journeyTwoDeliverToDocAddress = (JobDocAddress)CartageWrapper.JourneyTwoDeliverToAddress.WrappedObject;
			journeyTwoDeliverToDocAddress.E2_Contact = "tAAA";
			AssertEquals("JourneyTwoDeliverToContactName", "tAAA", CartageWrapper.JourneyTwoDeliverToContactName);
			journeyTwoDeliverToDocAddress.E2_Contact = "tBBB";
			AssertEquals("JourneyTwoDeliverToContactName", "tBBB", CartageWrapper.JourneyTwoDeliverToContactName);
		}

		public void TestJourneyTwoDeliverToContactPhone()
		{
			SetupCartageForPickupAndDeliveryContactNameAndPhone();
			JobDocAddress journeyTwoDeliverToDocAddress = (JobDocAddress)CartageWrapper.JourneyTwoDeliverToAddress.WrappedObject;
			journeyTwoDeliverToDocAddress.E2_Contact = "tAAA";
			AssertEquals("JourneyTwoDeliverToContactPhone", "t11", CartageWrapper.JourneyTwoDeliverToContactPhone);
			journeyTwoDeliverToDocAddress.E2_Contact = "tBBB";
			AssertEquals("JourneyTwoDeliverToContactPhone", "t22", CartageWrapper.JourneyTwoDeliverToContactPhone);
		}

		#endregion

		void SetupCartageForPickupAndDeliveryContactNameAndPhone()
		{
			OrgHeader firstOrg = Factory.New<OrgHeader>();
			OrgContact firstContact1 = firstOrg.Contacts.AddNew();
			firstContact1.OC_ContactName = "fAAA";
			firstContact1.OC_Phone = "f11";
			OrgContact firstContact2 = firstOrg.Contacts.AddNew();
			firstContact2.OC_ContactName = "fBBB";
			firstContact2.OC_Phone = "f22";

			OrgHeader secondOrg = Factory.New<OrgHeader>();
			OrgContact secondContact1 = secondOrg.Contacts.AddNew();
			secondContact1.OC_ContactName = "sAAA";
			secondContact1.OC_Phone = "s11";
			OrgContact secondContact2 = secondOrg.Contacts.AddNew();
			secondContact2.OC_ContactName = "sBBB";
			secondContact2.OC_Phone = "s22";

			OrgHeader thirdOrg = Factory.New<OrgHeader>();
			OrgContact thirdContact1 = thirdOrg.Contacts.AddNew();
			thirdContact1.OC_ContactName = "tAAA";
			thirdContact1.OC_Phone = "t11";
			OrgContact thirdContact2 = thirdOrg.Contacts.AddNew();
			thirdContact2.OC_ContactName = "tBBB";
			thirdContact2.OC_Phone = "t22";

			Cartage.FirstDocAddress.OrganisationPK = firstOrg.PK;
			Cartage.SecondDocAddress.OrganisationPK = secondOrg.PK;
			Cartage.ThirdDocAddress.OrganisationPK = thirdOrg.PK;

			Cartage.ContainerBookedMoves.AddNew();
		}

		#endregion

		void SetupForContactTests(string cartageType)
		{
			Cartage.JJ_E3_NKJobType = cartageType;
			Cartage.ContainerBookedMoves.AddNew();

			Cartage.FirstDocAddress.E2_OA_Address = GetNewAddyWithPhoneAndContactName("FirstPhone", "FirstName").PK;
			Cartage.SecondDocAddress.E2_OA_Address = GetNewAddyWithPhoneAndContactName("SecondPhone", "SecondName").PK;
			Cartage.ThirdDocAddress.E2_OA_Address = GetNewAddyWithPhoneAndContactName("ThirdPhone", "ThirdName").PK;
		}

		OrgAddress GetNewAddyWithPhoneAndContactName(ZString phone, ZString contactName)
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_Phone = phone;
			contact.OC_ContactName = contactName;
			contact.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;

			OrgAddress addy = org.Addresses.AddNew();
			addy.OA_Address1 = "abcd";
			addy.OA_Phone = "123";

			return addy;
		}

		public void TestHandlingInstructions()
		{
			AssertEquals(true, CartageWrapper.HandlingInstructions.IsEmpty);

			Cartage.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Do not break stuff. Carry carefully.");
			Cartage.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Danger! Danger!");

			AssertEquals(@"Do not break stuff. Carry carefully.
Danger! Danger!", CartageWrapper.HandlingInstructions);
		}

		public void TestCartageInstructions()
		{
			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			AssertEquals("", CartageWrapper.CartageInstructions);

			FreightHelperClass.AddNote(Cartage, pickupDesc, "Cartage Pickup Instructions");
			FreightHelperClass.AddNote(Cartage, deliveryDesc, "Cartage Delivery Instructions");

			CartageWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Cartage Delivery Instructions", CartageWrapper.CartageInstructions);

			CartageWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Cartage Pickup Instructions", CartageWrapper.CartageInstructions);

			CartageWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ANY));
			AssertEquals("Cartage Pickup Instructions\nCartage Delivery Instructions", CartageWrapper.CartageInstructions);

			Cartage.Notes.RemoveAndDeleteAll();
			FreightHelperClass.AddNote(Cartage, deliveryDesc, "Cartage Delivery Instructions");
			AssertEquals("Cartage Delivery Instructions", CartageWrapper.CartageInstructions);
		}

		public void TestFullHandlingInstructions()
		{
			AssertEquals(true, CartageWrapper.FullHandlingInstructions.IsEmpty);

			var note = Cartage.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			note.ST_NoteDataAsText = "Do not break stuff.\nCarry carefully.\n";
			AssertEquals("Do not break stuff.\nCarry carefully.", CartageWrapper.FullHandlingInstructions);
		}

		public void TestFullCartageInstructions()
		{
			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			AssertEquals("", CartageWrapper.FullCartageInstructions);

			FreightHelperClass.AddNote(Cartage, pickupDesc, "Cartage Pickup Instructions");
			FreightHelperClass.AddNote(Cartage, deliveryDesc, "Cartage Delivery Instructions");

			CartageWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Cartage Delivery Instructions", CartageWrapper.FullCartageInstructions);

			CartageWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Cartage Pickup Instructions", CartageWrapper.FullCartageInstructions);

			CartageWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ANY));
			AssertEquals("Cartage Pickup Instructions\nCartage Delivery Instructions", CartageWrapper.FullCartageInstructions);
		}

		public void TestAddressesWithWareHousing()
		{
			AssertEquals("AddressesWithWareHousing should be", 0, CartageWrapper.AddressesWithWareHousing.Count);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;

			var firstAddress = Factory.NewWithValidTestData<OrgAddress>();
			var secondAddress = Factory.NewWithValidTestData<OrgAddress>();
			var thirdAddress = Factory.NewWithValidTestData<OrgAddress>();

			Cartage.FirstDocAddress.E2_OA_Address = firstAddress.PK;
			Cartage.SecondDocAddress.E2_OA_Address = secondAddress.PK;
			Cartage.ThirdDocAddress.E2_OA_Address = thirdAddress.PK;

			firstAddress.OA_ForkLift = true;
			secondAddress.OA_DockLeveler = true;
			thirdAddress.OA_ForkLift = true;

			Cartage.ContainerBookedMoves.AddNew();

			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("AddressesWithWareHousing should have 3", 3, CartageWrapper.AddressesWithWareHousing.Count);

			Hashtable hashtable = new Hashtable();
			hashtable.Add(firstAddress.PK, firstAddress);
			hashtable.Add(secondAddress.PK, secondAddress);
			hashtable.Add(thirdAddress.PK, thirdAddress);
			foreach (DocDocAddress dAddress in CartageWrapper.AddressesWithWareHousing)
			{
				Assert("is in col", hashtable.ContainsKey(((JobDocAddress)dAddress.WrappedObject).Address.PK));
			}

			Cartage.SecondDocAddress.E2_OA_Address = thirdAddress.PK;
			AssertEquals("AddressesWithWareHousing has 2 same, should have 2, not 3", 2, CartageWrapper.AddressesWithWareHousing.Count);
		}

		public void TestPrintAsContainers()
		{
			Cartage.JJ_E3_NKJobType = "";
			Cartage.JJ_ContainerMode = "";
			AssertEquals("No Cartage type, should not Print as Containers", false, CartageWrapper.PrintAsContainers);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			AssertEquals(Constants.CartageJobType.NEW_FCLImportToCNE, true, CartageWrapper.PrintAsContainers);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			AssertEquals(Constants.CartageJobType.NEW_LCLExport, false, CartageWrapper.PrintAsContainers);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			AssertEquals(Constants.CartageJobType.NEW_FCLExportToSHP, true, CartageWrapper.PrintAsContainers);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLImport;
			AssertEquals(Constants.CartageJobType.NEW_LCLImport, false, CartageWrapper.PrintAsContainers);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			AssertEquals(Constants.CartageJobType.NEW_FCLExportPack, true, CartageWrapper.PrintAsContainers);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportUnpack;
			AssertEquals(Constants.CartageJobType.NEW_FCLImportUnpack, true, CartageWrapper.PrintAsContainers);
		}

		public void TestPrintTwoJourneys()
		{
			Cartage.JJ_E3_NKJobType = "";
			AssertEquals("No Cartage type, should not Print as Containers", false, CartageWrapper.PrintTwoJourneys);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			AssertEquals(Constants.CartageJobType.NEW_FCLImportToCNE, true, CartageWrapper.PrintTwoJourneys);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			AssertEquals(Constants.CartageJobType.NEW_LCLExport, false, CartageWrapper.PrintTwoJourneys);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			AssertEquals(Constants.CartageJobType.NEW_FCLExportToSHP, true, CartageWrapper.PrintTwoJourneys);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLImport;
			AssertEquals(Constants.CartageJobType.NEW_LCLImport, false, CartageWrapper.PrintTwoJourneys);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			AssertEquals(Constants.CartageJobType.NEW_FCLExportPack, true, CartageWrapper.PrintTwoJourneys);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportUnpack;
			AssertEquals(Constants.CartageJobType.NEW_FCLImportUnpack, true, CartageWrapper.PrintTwoJourneys);
		}

		public void TestEmailSubjectNumber()
		{
			Cartage.JJ_ConsignmentID = "T938473";
			AssertEquals("EmailSubjectNumber", "T938473", CartageWrapper.EmailSubjectNumber);
		}

		public void TestEquipmentType()
		{
			AssertEquals("Equipment Type", true, CartageWrapper.EquipmentType.IsEmpty);

			Cartage.JJ_DropMode = Cartage.Lookups.DropModes[0].Code;
			AssertEquals("Equipment Type", CartageWrapper.DropMode, CartageWrapper.EquipmentType);
		}

		public void TestIsEmptyLeg()
		{
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			Cartage.ContainerBookedMoves.AddNew();

			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("Journey One, Two Journeys, Export: Should be True", true, CartageWrapper.IsEmptyLeg(true));
			AssertEquals("Journey Two, Two Journeys, Export: Should be False", false, CartageWrapper.IsEmptyLeg(false));

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("Journey One, Two Journeys, Import: Should be False", false, CartageWrapper.IsEmptyLeg(true));
			AssertEquals("Journey Two, Two Journeys, Import: Should be True", true, CartageWrapper.IsEmptyLeg(false));

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("Journey One, One Journey, Import: Should be False", false, CartageWrapper.IsEmptyLeg(true));
			AssertEquals("Journey Two, One Journey, Import: Should be False", false, CartageWrapper.IsEmptyLeg(false));

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLImport;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("Journey One, One Journey, Import: Should be False", false, CartageWrapper.IsEmptyLeg(true));
			AssertEquals("Journey Two, One Journey, Import: Should be False", false, CartageWrapper.IsEmptyLeg(false));

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_EmptyCNEtoCYD;
			Cartage.ContainerBookedMoves.AddNew();

			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("Journey One, One Journey, Import: Should be False", true, CartageWrapper.IsEmptyLeg(true));
			AssertEquals("Journey Two, One Journey, Import: Should be False", false, CartageWrapper.IsEmptyLeg(false));

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLCTOtoCFS;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("Journey One, One Journey, Import: Should be False", false, CartageWrapper.IsEmptyLeg(true));
			AssertEquals("Journey Two, One Journey, Import: Should be False", false, CartageWrapper.IsEmptyLeg(false));

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLPackLooseFromSHP;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("Journey One, Two Journeys, Export: Should be True", true, CartageWrapper.IsEmptyLeg(true));
			AssertEquals("Journey Two, Two Journeys, Export: Should be False", false, CartageWrapper.IsEmptyLeg(false));

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("Journey One, Two Journeys, Import: Should be False", false, CartageWrapper.IsEmptyLeg(true));
			AssertEquals("Journey Two, Two Journeys, Import: Should be True", true, CartageWrapper.IsEmptyLeg(false));
		}

		public void TestIsFullLeg()
		{
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			Cartage.ContainerBookedMoves.AddNew();

			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("Journey One, Two Journeys, Export: Should be False", false, CartageWrapper.IsFullLeg(true));
			AssertEquals("Journey Two, Two Journeys, Export: Should be True", true, CartageWrapper.IsFullLeg(false));

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("Journey One, Two Journeys, Import: Should be True", true, CartageWrapper.IsFullLeg(true));
			AssertEquals("Journey Two, Two Journeys, Import: Should be False", false, CartageWrapper.IsFullLeg(false));

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("Journey One, One Journey, Import: Should be False", false, CartageWrapper.IsFullLeg(true));
			AssertEquals("Journey Two, One Journey, Import: Should be False", false, CartageWrapper.IsFullLeg(false));

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLImport;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("Journey One, One Journey, Import: Should be False", false, CartageWrapper.IsFullLeg(true));
			AssertEquals("Journey Two, One Journey, Import: Should be False", false, CartageWrapper.IsFullLeg(false));

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLCTOtoCFS;
			Cartage.ContainerBookedMoves.AddNew();

			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("Journey One, One Journey, Import: Should be True", true, CartageWrapper.IsFullLeg(true));
			AssertEquals("Journey Two, One Journey, Import: Should be False", true, CartageWrapper.IsFullLeg(false));

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_EmptyCNEtoCYD;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("Journey One, One Journey, Import: Should be False", false, CartageWrapper.IsFullLeg(true));
			AssertEquals("Journey Two, One Journey, Import: Should be False", true, CartageWrapper.IsFullLeg(false));

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLPackLooseFromSHP;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("Journey One, Two Journeys, Export: Should be False", false, CartageWrapper.IsFullLeg(true));
			AssertEquals("Journey Two, Two Journeys, Export: Should be True", true, CartageWrapper.IsFullLeg(false));

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			AssertEquals("Journey One, Two Journeys, Import: Should be True", true, CartageWrapper.IsFullLeg(true));
			AssertEquals("Journey Two, Two Journeys, Import: Should be False", false, CartageWrapper.IsFullLeg(false));
		}

		#endregion

		#region Cartage Cover Sheet Fields

		public void TestCartageParentOfItself()
		{
			Cartage.JJ_ConsignmentID = "ABC123";
			AssertEquals("CartageParent not pointing to itself.", CartageWrapper.ConsignmentID, CartageWrapper.Cartage.ConsignmentID);
		}

		public void TestCartageDocContainersIsSynchronisedWithContainers()
		{
			Cartage.ContainerBookedMoves.AddNew();
			int beforeContainerCount = CartageWrapper.Containers.Count;
			Cartage.ContainerBookedMoves.AddNew();
			AssertEquals("Cartage DocContainers cache should be refreshed.", 1, CartageWrapper.Containers.Count - beforeContainerCount);
		}

		public void TestCartageIsImport()
		{
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			AssertEquals("Cartage as export not showing 'N'.", false, CartageWrapper.IsImport);
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLImport;
			AssertEquals("Cartage as import not showing 'Y'.", true, CartageWrapper.IsImport);
		}

		public void TestCartageIsAir()
		{
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			AssertEquals("Cartage not as Air not showing 'N'.", false, CartageWrapper.IsAir);
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			AssertEquals("Cartage as Air not showing 'Y'.", true, CartageWrapper.IsAir);
		}

		public void TestSlotAsArrivalOrDeparture()
		{
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			AssertEquals("Cartage as departure not showing 'D'.", "D", CartageWrapper.SlotAsArrivalOrDeparture);
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			AssertEquals("Cartage as arrival not showing 'A'.", "A", CartageWrapper.SlotAsArrivalOrDeparture);
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirImport;
			AssertEquals("Non container Cartage not showing '-'.", " - ", CartageWrapper.SlotAsArrivalOrDeparture);
		}

		public void TestCartageContainerRequiredFields()
		{
			Cartage.Containers.DeleteAll();
			CommonContainer container = Cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_ContainerNum = "ABC";

			AssertEquals("No Cartage/Containers Accessed.", 1, CartageWrapper.Containers.Count);
			AssertEquals("No Container Number found.", "ABC", CartageWrapper.Containers[0].ContainerNumber);
		}

		public void TestCartageSailing()
		{
			Cartage.JJ_JX_Sailing = Sailing.PK;
			Sailing.Voyage.JV_VoyageFlight = "BINGO";
			AssertEquals("CommonCartage Sailing object not set.", "BINGO", Sailing.Voyage.JV_VoyageFlight);
			AssertNotNull("CommonCartage Voyage object not exposed.", CartageWrapper.Sailing.Voyage);
			AssertEquals("CommonCartage Voyage object not passing correct value.", "BINGO", CartageWrapper.Sailing.Voyage.VoyageFlight);
		}

		public void TestCartageSailingDenormalisations()
		{
			Cartage.JJ_JX_Sailing = Sailing.PK;
			Sailing.Voyage.JV_VoyageFlight = "BINGO";
			Cartage.VoyageFlight = "BINGO";
			AssertEquals("CommonCartage Voyage object not passing denormalised Voyage value.", "BINGO", CartageWrapper.Voyage);
			AssertEquals("CommonCartage Voyage object not passing denormalised Vessel value.", Cartage.Vessel, CartageWrapper.VesselName);
			AssertEquals("CommonCartage Voyage object not passing denormalised LoadPort value.", Cartage.PortOfLoading, CartageWrapper.SailingPortOfLoading);
			AssertEquals("CommonCartage Voyage object not passing denormalised DischargePort value.", Cartage.PortOfDischarge, CartageWrapper.SailingPortOfArrival);
			AssertEquals("CommonCartage Voyage object not passing denormalised ETD value.", Cartage.E_DEP.ToShortDateString(), CartageWrapper.SailingETD);
			AssertEquals("CommonCartage Voyage object not passing denormalised ETA value.", Cartage.E_ARV.ToShortDateString(), CartageWrapper.SailingETA);
		}

		public void TestCartageSailingFromDeclaration()
		{
			BusinessObject declaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			Cartage.JJ_ParentID = declaration.PK;
			Cartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			Cartage.JJ_ConsignmentID = "/E";
			declaration[JobDeclarationSchema.Constants.JE_VoyageFlightNo] = "BINGO";
			AssertEquals("CommonCartage Voyage object not passing denormalised Voyage value.", "BINGO", CartageWrapper.Voyage);
			AssertEquals("CommonCartage Voyage object not passing denormalised Vessel value.", Cartage.Vessel, CartageWrapper.VesselName);
			AssertEquals("CommonCartage Voyage object not passing denormalised LoadPort value.", Cartage.PortOfLoading, CartageWrapper.SailingPortOfLoading);
			AssertEquals("CommonCartage Voyage object not passing denormalised DischargePort value.", Cartage.PortOfDischarge, CartageWrapper.SailingPortOfArrival);
			AssertEquals("CommonCartage Voyage object not passing denormalised ETD value.", Cartage.E_DEP.ToShortDateString(), CartageWrapper.SailingETD);
			AssertEquals("CommonCartage Voyage object not passing denormalised ETA value.", Cartage.E_ARV.ToShortDateString(), CartageWrapper.SailingETA);
		}

		public void TestHasSailingDetails()
		{
			AssertEquals("Wrapper indicating presence of sailing details", false, CartageWrapper.HasSailingDetails);
			AssertEquals("Wrapper indicating presence of sailing details", false, CartageWrapper.HasSailingDetailsOrDeclarationAsParent);
			Cartage.JJ_JX_Sailing = Sailing.PK;
			Sailing.Voyage.JV_VoyageFlight = "BINGO";
			AssertEquals("CommonCartage Sailing object not set.", "BINGO", Sailing.Voyage.JV_VoyageFlight);
			AssertNotNull("CommonCartage Voyage object not exposed.", CartageWrapper.Sailing.Voyage);
			AssertEquals("Wrapper not indicating presence of sailing details", true, CartageWrapper.HasSailingDetails);
			AssertEquals("Wrapper not indicating presence of sailing details", true, CartageWrapper.HasSailingDetailsOrDeclarationAsParent);
		}

		public void TestCartageSailingHeading()
		{
			AssertEquals("With no sailing, heading should be empty", "", CartageWrapper.SailingDatesHeading);

			Cartage.JJ_JX_Sailing = Sailing.PK;
			Cartage.ContainerBookedMoves.AddNew();

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			AssertEquals("Heading should be FCL RECEIVING.", "FCL RECEIVING", CartageWrapper.SailingDatesHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			AssertEquals("Heading should be FCL PICKUP.", "FCL PICKUP", CartageWrapper.SailingDatesHeading);

			Cartage.Containers.DeleteAll();
			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			AssertEquals("Heading should be LCL RECEIVING.", "LCL RECEIVING", CartageWrapper.SailingDatesHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLImport;
			AssertEquals("Heading should be LCL PICKUP.", "LCL PICKUP", CartageWrapper.SailingDatesHeading);
		}

		public void TestSailingReceivalPickupDateFromHeading()
		{
			AssertEquals("With no sailing, heading should be empty", "", CartageWrapper.SailingReceivalPickupDateFromHeading);

			Cartage.JJ_JX_Sailing = Sailing.PK;

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			AssertEquals("Heading should be CTO RECEIVING.", "CTO RCV. STARTS", CartageWrapper.SailingReceivalPickupDateFromHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			AssertEquals("Heading should be CTO AVAILABLE", "CTO AVAILABLE", CartageWrapper.SailingReceivalPickupDateFromHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			AssertEquals("Heading should be CFS RCV. STARTS", "CFS RCV. STARTS", CartageWrapper.SailingReceivalPickupDateFromHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLImport;
			AssertEquals("Heading should be CFS AVAILABLE", "CFS AVAILABLE", CartageWrapper.SailingReceivalPickupDateFromHeading);
		}

		public void TestSailingReceivalPickupDateToHeading()
		{
			AssertEquals("With no sailing, heading should be empty", "", CartageWrapper.SailingReceivalPickupDateToHeading);

			Cartage.JJ_JX_Sailing = Sailing.PK;

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			AssertEquals("Heading should be CTO CUTOFF", "CTO CUTOFF", CartageWrapper.SailingReceivalPickupDateToHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			AssertEquals("Heading should be CTO STOR. STARTS", "CTO STOR. STARTS", CartageWrapper.SailingReceivalPickupDateToHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			AssertEquals("Heading should be CFS CUTOFF", "CFS CUTOFF", CartageWrapper.SailingReceivalPickupDateToHeading);

			Cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLImport;
			AssertEquals("Heading should be CFS STOR. STARTS", "CFS STOR. STARTS", CartageWrapper.SailingReceivalPickupDateToHeading);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());

			Cartage = Factory.New<CommonCartage>();
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);

			base.SetUp();
		}

		public LocalCartageTestHelper Helper
		{
			get { return helper ?? (helper = new LocalCartageTestHelper(Factory)); }
		}
		LocalCartageTestHelper helper;

		JobSailing Sailing
		{
			get
			{
				if (fSailing == null)
				{
					var voyage = Factory.New<JobVoyage>();
					voyage.JV_RV_NKVessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_FK;
					voyage.JV_VoyageFlight = "234234";

					voyage.Origins.AddNew();
					voyage.Origins[0].JA_RL_NKPortOfLoading = "HKHKG";
					voyage.Origins[0].JA_E_DEP = ZDateTime.Today;

					voyage.Destinations.AddNew();
					voyage.Destinations[0].JB_RL_NKPortOfDischarge = "USSFO";
					voyage.Destinations[0].JB_E_ARV = ZDateTime.Today.AddDays(20);

					fSailing = voyage.Sailings[0];
					//fSailing.JX_AvailabilityDate = ZDateTime.Today.AddDays(21);
					//fSailing.JX_StorageDate = ZDateTime.Today.AddDays(23);
				}
				return fSailing;
			}
		}

		JobSailing Flight
		{
			get
			{
				if (fFlight == null)
				{
					var voyage = Factory.New<JobVoyage>();
					voyage.JV_AirSeaRoad = Enterprise.Core.Constants.TransportModes.Air;
					voyage.JV_VoyageFlight = "QF33";

					voyage.Origins.AddNew();
					voyage.Origins[0].JA_RL_NKPortOfLoading = "HKHKG";
					voyage.Origins[0].JA_E_DEP = ZDateTime.Today;

					voyage.Destinations.AddNew();
					voyage.Destinations[0].JB_RL_NKPortOfDischarge = "USSFO";
					voyage.Destinations[0].JB_E_ARV = ZDateTime.Today.AddDays(20);

					fFlight = voyage.Sailings[0];
					//fFlight.JX_AvailabilityDate = ZDateTime.Today.AddDays(21);
					//fFlight.JX_StorageDate = ZDateTime.Today.AddDays(23);
				}
				return fFlight;
			}
		}

		JobSailing fFlight;
		JobSailing fSailing;

		CommonCartage Cartage;
		DocCommonCartage CartageWrapper;

		#endregion

		#region Containers

		CommonContainer MakeNewContainerForCartage(CommonCartage cartage, ZString containerNumber, RefContainer refContainer)
		{
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_ContainerNum = containerNumber;
			container.JC_RC = refContainer.PK;
			return container;
		}

		public void TestPrintPageWithContainerNumber()
		{
			RefContainer ref1 = Factory.New<RefContainer>();
			ref1.RC_Code = "10FR";
			RefContainer ref2 = Factory.New<RefContainer>();
			ref2.RC_Code = "20FR";
			RefContainer ref3 = Factory.New<RefContainer>();
			ref3.RC_Code = "30FR";
			RefContainer ref4 = Factory.New<RefContainer>();
			ref4.RC_Code = "40FR";
			RefContainer ref5 = Factory.New<RefContainer>();
			ref5.RC_Code = "50FR";

			CommonContainer container1 = MakeNewContainerForCartage(Cartage, "CONTNMBR1", ref1);
			CommonContainer container2 = MakeNewContainerForCartage(Cartage, "CONTNMBR2", ref2);
			CommonContainer container3 = MakeNewContainerForCartage(Cartage, "CONTNMBR3", ref3);
			CommonContainer container4 = MakeNewContainerForCartage(Cartage, "CONTNMBR4", ref4);
			CommonContainer container5 = MakeNewContainerForCartage(Cartage, "CONTNMBR5", ref5);

			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			CartageWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("PrintPageWithContainerNumber must be false", false, (bool)CartageWrapper.PrintPageWithContainerNumber);
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("PrintPageWithContainerNumber must be false", false, (bool)CartageWrapper.PrintPageWithContainerNumber);
			CommonContainer container6 = MakeNewContainerForCartage(Cartage, "CONTNMBR6", ref5);
			CartageWrapper = DocCommonCartage.New(Cartage, Factory);
			CartageWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("PrintPageWithContainerNumber must be false", false, (bool)CartageWrapper.PrintPageWithContainerNumber);
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("PrintPageWithContainerNumber must be true", true, (bool)CartageWrapper.PrintPageWithContainerNumber);
		}
		#endregion

	}
}
