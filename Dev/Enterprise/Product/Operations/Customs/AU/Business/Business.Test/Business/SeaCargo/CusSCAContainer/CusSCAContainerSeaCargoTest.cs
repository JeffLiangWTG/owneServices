using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CusSCAContainerSeaCargoTest : SeaCargoTestCase
	{
		#region TestAddressSplitProperties

		public void TestAddressSplitProperties()
		{
			OrgHeader org1 = CreateOrganisation("ORG111", "AUSYD");
			OrgAddress address11 = org1.Addresses.AddNew();
			address11.AddressCapability.SetCapabilityEnabled("PAD");
			address11.OA_Code = "Test PAD 11";
			OrgAddress address12 = org1.Addresses.AddNew();
			address12.AddressCapability.SetCapabilityEnabled("PAD");
			address12.OA_Code = "Test PAD 12";

			OrgHeader org2 = CreateOrganisation("ORG222", "AUSYD");
			OrgAddress address21 = org2.Addresses.AddNew();
			address21.AddressCapability.SetCapabilityEnabled("PAD");
			address21.OA_Code = "Test PAD 21";
			OrgAddress address22 = org2.Addresses.AddNew();
			address22.AddressCapability.SetCapabilityEnabled("PAD");
			address22.OA_Code = "Test PAD 22";

			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer testContainer = oceanBill.Containers.AddNew();
			// Underbond To Address

			testContainer.CN_OA_UnderbondToOrg = org1.PK;
			testContainer.CN_OA_UnderbondToCode = address11.OA_Code;
			AssertEquals("Address 11", address11.PK, testContainer.CN_OA_UnderbondTo);
			testContainer.CN_OA_UnderbondToCode = address12.OA_Code;
			AssertEquals("Address 12", address12.PK, testContainer.CN_OA_UnderbondTo);

			// Underbond From
			testContainer.CN_OA_UnderbondFromOrg = org2.PK;
			testContainer.CN_OA_UnderbondFromCode = address21.OA_Code;
			AssertEquals("Address 21", address21.PK, testContainer.CN_OA_UnderbondFrom);
			testContainer.CN_OA_UnderbondFromCode = address22.OA_Code;
			AssertEquals("Address 22", address22.PK, testContainer.CN_OA_UnderbondFrom);
		}

		#endregion

		public void TestSetDefaultValuesFromFreightConsol()
		{
			CommonConsol consol = CreateFCLConsol();
			CommonContainer jobContainer = consol.Containers.AddNew();
			consol.JK_OA_UnpackDepotAddress = CreateDepot("Depot").MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = CreateCTO("CTO").MainAddress.PK;
			CMRSeaCargoSynchroniser synchroniser = new CMRSeaCargoSynchroniser(consol);
			var container = synchroniser.OceanBill.Containers[0];
			var pivot = container.Pivots.AddNew();
			pivot.CV_PackageCount = 125;
			CusUnderbond testUnderbond = container.Underbonds.AddNew();
			testUnderbond.SetDefaultValuesFromParent();
			AssertEquals("Underbond Origin Address", consol.JK_OA_ArrivalCTOAddress, testUnderbond.C4_OA_OriginAddress);
			AssertEquals("Underbond Destination Address", consol.JK_OA_UnpackDepotAddress, testUnderbond.C4_OA_DestinationAddress);
			AssertEquals("Controlled Premise Code", DepotCode, testUnderbond.C4_DestinationPremiseID);
			AssertEquals("Controlled Premise Code", CTOCode, testUnderbond.C4_OriginPremiseID);
		}

		public void TestSetDefaultValuesFromCusSCAContainer()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();
			CusSCAPivot pivot = container.Pivots.AddNew();
			pivot.CV_PackageCount = 125;
			CusUnderbond testUnderbond = container.Underbonds.AddNew();
			testUnderbond.C4_ParentID = container.PK;
			testUnderbond.C4_ParentTableCode = "CN";
			testUnderbond.SetDefaultValuesFromParent();
			AssertEquals("1 Container - not 125 containers", 1u, testUnderbond.C4_PiecesManifested);
		}

		public void TestSetDefaultValuesFromCusSCAContainerMaxLengthExceeded()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();
			CusSCAPivot pivot1 = container.Pivots.AddNew();
			pivot1.CV_CN = container.PK;
			pivot1.CV_PackageCount = 2000000000;
			CusSCAPivot pivot2 = container.Pivots.AddNew();
			pivot2.CV_CN = container.PK;
			pivot2.CV_PackageCount = 2000000000;
			CusUnderbond testUnderbond = container.Underbonds.AddNew();
			testUnderbond.C4_ParentID = container.PK;
			testUnderbond.C4_ParentTableCode = "CN";
			testUnderbond.SetDefaultValuesFromParent();
			AssertEquals("It is still only 1 container", 1u, testUnderbond.C4_PiecesManifested);
		}

		public void TestUnderbonds()
		{
			CusSCAOceanBill oB = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oB.Containers.AddNew();
			AssertNotNull("Underbonds", ((ICusUnderbondDependentCollectionParent)container).Underbonds);
		}

		public void TestUnderbondHumanReadableName()
		{
			CusSCAOceanBill oB = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oB.Containers.AddNew();
			AssertEquals("HumanReadableName", "Container", ((ICusUnderbondDependentCollectionParent)container).UnderbondHumanReadableName);
			container.CN_ContainerNumber = "123";
			AssertEquals("HumanReadableName", "Container 123", ((ICusUnderbondDependentCollectionParent)container).UnderbondHumanReadableName);
			container.CN_ContainerMode = CMRCargoTypes.Codes.BreakBulk;
			ZString breakBulkOceanBillNumber = "BREAKBULK12390";
			oB.CB_OceanBill = breakBulkOceanBillNumber;
			AssertEquals("HumanReadableName - Break Bulk", "Break Bulk: " + breakBulkOceanBillNumber, ((ICusUnderbondDependentCollectionParent)container).UnderbondHumanReadableName);
		}

		public void TestCusUnderbondIsRegisteredEditable()
		{
			CusSCAContainer container = Factory.GetNull<CusSCAContainer>();
			Assert("IsRegisteredEditableChildObject", container.IsRegisteredEditableChildObject(((ICusUnderbondDependentCollectionParent)container).Underbonds));
		}

		public void TestISeaOutturnReportHeaderInformationProvider_GetHeader()
		{
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();
			AssertNotNull(((ISeaOutturnReportHeaderInformationProvider)container).GetHeader(underbond));
		}

		public void TestOutturnableLinesForFCLFCXBBKOrBLK()
		{
			TestOutturnableLinesExpectOnlyContainer(Core.Constants.ContainerModes.FCL);
			TestOutturnableLinesExpectOnlyContainer(Core.Constants.ContainerModes.FCLMixedShipper);
			TestOutturnableLinesExpectOnlyContainer(Core.Constants.ContainerModes.BreakBulk);
			TestOutturnableLinesExpectOnlyContainer(Core.Constants.ContainerModes.Bulk);
		}

		void TestOutturnableLinesExpectOnlyContainer(string containerMode)
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerMode = containerMode;
			CusSCAPivot pivot = house.Pivot.AddNew();
			pivot.CV_CN = container.PK;
			IOutturnableLine[] result = ((ICusUnderbondDependentCollectionParent)container).OutturnableLines;
			AssertEquals("Lenght", 1, result.Length);
			AssertEquals("Result[0]", container, result[0]);
		}

		public void TestOutturnableLinesForLCL()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			CusSCAContainer container = oceanBill.Containers.AddNew();
			container.CN_ContainerMode = Core.Constants.ContainerModes.LCL;
			CusSCAPivot pivot = house.Pivot.AddNew();
			pivot.CV_CN = container.PK;
			IOutturnableLine[] result = ((ICusUnderbondDependentCollectionParent)container).OutturnableLines;
			AssertEquals("Lenght", 2, result.Length);
			AssertEquals("Result[0]", container, result[0]);
			AssertEquals("Result[1]", pivot, result[1]);
		}

		public void TestUnderbondDefaultsMovemeantReasonToMOV()
		{
			var container = Factory.New<CusSCAContainer>();
			CusUnderbond underbond = container.Underbonds.AddNew();
			((IUnderbondDefaultValueProvider)container).SetUnderbondDefaultValues(underbond);
			AssertEquals("MOV", underbond.C4_MovementReason);
		}

		public void TestFCLUnderbondDefaults()
		{
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.MainAddress.LocalControlledPremisesID = localDepotPremiseCode;

			UnpackDepot.OH_RL_NKClosestPort = houseBillDestinationPort;
			CommonConsol consol = CreateFCLConsol();
			consol.JK_OA_ArrivalCTOAddress = ArrivalCTO.MainAddress.PK;
			CommonContainer container = consol.Containers.AddNew();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 10;
			shipment.JS_RL_NKDestination = houseBillDestinationPort;

			CMRSeaCargoSynchroniser synchroniser = new CMRSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			synchroniser.LoadHouseBills();
			var underbondParent = oceanBill.Containers[0];

			CusUnderbond underbondFromCTO = underbondParent.Underbonds.AddNew();
			underbondFromCTO.C4_ParentID = underbondParent.PK;
			underbondFromCTO.C4_ParentTableCode = CusSCAContainerSchema.Constants.Prefix;
			((IUnderbondDefaultValueProvider)underbondParent).SetUnderbondDefaultValues(underbondFromCTO);

			AssertEquals("Underbond 1 Origin Address", ArrivalCTO.MainAddress.PK, underbondFromCTO.C4_OA_OriginAddress);
			AssertEquals("Underbond 1 Origin Premise Code ", arrivalCTOPremiseCode, underbondFromCTO.C4_OriginPremiseID);
			AssertEquals("Underbond 1 Destination Address", ZGuid.Empty, underbondFromCTO.C4_OA_DestinationAddress);
			AssertEquals("Underbond 1 Destination Premise Code", localDepotPremiseCode, underbondFromCTO.C4_DestinationPremiseID);
			AssertEquals("Underbond 1 Is move from Discharge", true, underbondFromCTO.C4_IsMoveFromDischarge);
			AssertEquals("Underbond 1 Package Count", 1, (int)underbondFromCTO.C4_PiecesManifested);
			AssertEquals("Underbond 1 Package Type", CMRPackageTypes.Codes.UnpackedOrPacked, underbondFromCTO.C4_PackageType);
			AssertEquals("Underbond 1 Reason", CMRUnderbondRequestCodes.Codes.DeliveryToFinalDestination, underbondFromCTO.C4_MovementReason);
		}

		public void TestFCLUnderbondDefaultsCTOToLocalDepotToFinalDestination()
		{
			CommonConsol consol = CreateFCLConsol();
			consol.JK_OA_ArrivalCTOAddress = ArrivalCTO.MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = UnpackDepot.MainAddress.PK;
			CommonContainer container = consol.Containers.AddNew();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 10;

			CMRSeaCargoSynchroniser synchroniser = new CMRSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			synchroniser.LoadHouseBills();

			CusUnderbond underbondFromCTO = oceanBill.Containers[0].Underbonds.AddNew();
			underbondFromCTO.C4_ParentID = oceanBill.Containers[0].PK;
			underbondFromCTO.C4_ParentTableCode = CusSCAContainerSchema.Constants.Prefix;
			((IUnderbondDefaultValueProvider)oceanBill.Containers[0]).SetUnderbondDefaultValues(underbondFromCTO);

			AssertEquals("Underbond 1 Origin Address", ArrivalCTO.MainAddress.PK, underbondFromCTO.C4_OA_OriginAddress);
			AssertEquals("Underbond 1 Origin Premise Code ", arrivalCTOPremiseCode, underbondFromCTO.C4_OriginPremiseID);
			AssertEquals("Underbond 1 Destination Address", UnpackDepot.MainAddress.PK, underbondFromCTO.C4_OA_DestinationAddress);
			AssertEquals("Underbond 1 Destination Premise Code", localDepotPremiseCode, underbondFromCTO.C4_DestinationPremiseID);
			AssertEquals("Underbond 1 Is move from Discharge", true, underbondFromCTO.C4_IsMoveFromDischarge);
			AssertEquals("Underbond 1 Package Count", 1, (int)underbondFromCTO.C4_PiecesManifested);
			AssertEquals("Underbond 1 Package Type", CMRPackageTypes.Codes.UnpackedOrPacked, underbondFromCTO.C4_PackageType);
			AssertEquals("Underbond 1 Reason", CMRUnderbondRequestCodes.Codes.OtherMovement, underbondFromCTO.C4_MovementReason);

			Factory.Save();
			CusUnderbond underbondMoveToOtherDepot = oceanBill.Containers[0].Underbonds.AddNew();
			underbondMoveToOtherDepot.C4_ParentID = oceanBill.Containers[0].PK;
			underbondMoveToOtherDepot.C4_ParentTableCode = CusSCAContainerSchema.Constants.Prefix;
			((IUnderbondDefaultValueProvider)oceanBill.Containers[0]).SetUnderbondDefaultValues(underbondMoveToOtherDepot);
			AssertEquals("Underbond 2 Origin Address", UnpackDepot.MainAddress.PK, underbondMoveToOtherDepot.C4_OA_OriginAddress);
			AssertEquals("Underbond 2 origin Premise Code", localDepotPremiseCode, underbondMoveToOtherDepot.C4_OriginPremiseID);
			AssertEquals("Underbond 2 Is move from Discharge", false, underbondMoveToOtherDepot.C4_IsMoveFromDischarge);
			Assert("Underbond 2 destination should equal the origin", underbondMoveToOtherDepot.C4_OA_DestinationAddress != underbondMoveToOtherDepot.C4_OA_OriginAddress);
			Assert("Underbond 2 destination should equal the origin code", underbondMoveToOtherDepot.C4_DestinationPremiseID != underbondMoveToOtherDepot.C4_OriginPremiseID);

			AssertEquals("Underbond 1 Package Count", 1, (int)underbondMoveToOtherDepot.C4_PiecesManifested);
			AssertEquals("Underbond 1 Package Type", CMRPackageTypes.Codes.UnpackedOrPacked, underbondMoveToOtherDepot.C4_PackageType);
			AssertEquals("Underbond 1 Reason", CMRUnderbondRequestCodes.Codes.DeliveryToFinalDestination, underbondMoveToOtherDepot.C4_MovementReason);
		}

		public void TestLCLUnderbondDefaults()
		{
			CommonConsol consol = CreateLCLConsol();
			consol.JK_OA_ArrivalCTOAddress = ArrivalCTO.MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = UnpackDepot.MainAddress.PK;
			CommonContainer container = consol.Containers.AddNew();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 10;
			shipment.JS_RL_NKDestination = houseBillDestinationPort;

			CMRSeaCargoSynchroniser synchroniser = new CMRSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			synchroniser.LoadHouseBills();

			CusUnderbond underbondFromCTO = oceanBill.Containers[0].Underbonds.AddNew();
			underbondFromCTO.C4_ParentID = oceanBill.Containers[0].PK;
			underbondFromCTO.C4_ParentTableCode = CusSCAContainerSchema.Constants.Prefix;
			((IUnderbondDefaultValueProvider)oceanBill.Containers[0]).SetUnderbondDefaultValues(underbondFromCTO);

			AssertEquals("Underbond 1 Origin Address", ArrivalCTO.MainAddress.PK, underbondFromCTO.C4_OA_OriginAddress);
			AssertEquals("Underbond 1 Origin Premise Code ", arrivalCTOPremiseCode, underbondFromCTO.C4_OriginPremiseID);
			AssertEquals("Underbond 1 Destination Address", UnpackDepot.MainAddress.PK, underbondFromCTO.C4_OA_DestinationAddress);
			AssertEquals("Underbond 1 Destination Premise Code", localDepotPremiseCode, underbondFromCTO.C4_DestinationPremiseID);
			AssertEquals("Underbond 1 Is move from Discharge", true, underbondFromCTO.C4_IsMoveFromDischarge);
			AssertEquals("Underbond 1 Package Count", 1, (int)underbondFromCTO.C4_PiecesManifested);
			AssertEquals("Underbond 1 Package Type", CMRPackageTypes.Codes.UnpackedOrPacked, underbondFromCTO.C4_PackageType);
			AssertEquals("Underbond 1 Reason", CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination, underbondFromCTO.C4_MovementReason);
		}
	}
}
