using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAPivotMessageStateTest : SeaCargoTestCase
	{
		public void Test2SeaCargoFCLJobsWithSameContainerFailsHouseBillView()
		{
			CusSCAOceanBill oceanBill = CreateTestOriginalOceanBill();
			oceanBill.HouseBills[0].RegisterEditableChildObject(oceanBill.HouseBills[0].Pivot);
			oceanBill.HouseBills[1].RegisterEditableChildObject(oceanBill.HouseBills[1].Pivot);

			CreatePivot(oceanBill.HouseBills[0], ContainerNumber4);
			CreatePivot(oceanBill.HouseBills[1], ContainerNumber4);
			oceanBill.HouseBills[0].Pivot.FromContainerNumber(ContainerNumber4).Container.CN_ContainerMode = Core.Constants.ContainerModes.FCL;
			Factory.Save();
			oceanBill.HouseBills[0].RunPreSaveValidation();
			oceanBill.HouseBills[1].RunPreSaveValidation();
			AssertEquals("OceanBill.HouseBill[0].Pivot Message Errors", true, oceanBill.HouseBills[0].Pivot.FromContainerNumber(ContainerNumber4).CV_AssociatedContainerInfo.HasMessageErrors());
			AssertEquals("OceanBill.HouseBill[0].Pivot Errors", false, oceanBill.HouseBills[0].Pivot.FromContainerNumber(ContainerNumber4).CV_AssociatedContainerInfo.HasErrors());
		}

		public void Test2SeaCargoFCLJobsWithSameContainerFailsOceanBillView()
		{
			var oceanBill = CreateTestOriginalOceanBill();

			CreatePivot(oceanBill.HouseBills[0], ContainerNumber4);
			CreatePivot(oceanBill.HouseBills[1], ContainerNumber4);
			oceanBill.HouseBills[0].Pivot.FromContainerNumber(ContainerNumber4).Container.CN_ContainerMode = Core.Constants.ContainerModes.FCL;
			Factory.Save();
			oceanBill.RunPreSaveValidation();
			AssertEquals("OceanBill.HouseBill[0].Pivot Message Errors", true, oceanBill.HouseBills[0].Pivot.FromContainerNumber(ContainerNumber4).CV_AssociatedContainerInfo.HasMessageErrors());
			AssertEquals("OceanBill.HouseBill[0].Pivot Errors", false, oceanBill.HouseBills[0].Pivot.FromContainerNumber(ContainerNumber4).CV_AssociatedContainerInfo.HasErrors());
		}

		public void TestMarksAndNumbersAreConvertedToUppercase()
		{
			CusSCAOceanBill oceanBill = CreateTestOriginalOceanBill();
			CusSCAPivot pivot = CreatePivot(oceanBill.HouseBills[0], ContainerNumber4);
			pivot.CV_MarksAndNumbers = "abc";
			AssertEquals("ABC", pivot.CV_MarksAndNumbers);
		}

		public void TestMessages()
		{
			CusSCAPivot pivot = Factory.New<CusSCAPivot>();
			AssertNotNull(pivot.Messages);
		}

		[ExpectNoExceptions()]
		public void TestDeleteWithNoContainerNumber()
		{
			CusSCAOceanBill oceanBill = CreateTestOriginalOceanBill();
			CusSCAHouse testHouse = oceanBill.HouseBills[0];
			CreatePivot(testHouse, ContainerNumber2);
			oceanBill.Containers[0].CN_ContainerNumber = "";
			testHouse.Delete();
		}

		public void TestCV_ContainerShipmentStatusDoesnBlowUpIfCMRMessagesInCollection()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = cMROceanBill.Containers.AddNew();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			CusSCAPivot pivot = house.Pivot.AddNew();
			pivot.CV_CN = container.PK;
			CMRSEACRRMessage message = Factory.New<CMRSEACRRMessage>();
			house.Messages.Add(message);
			AssertNotNull(pivot.CV_ContainerShipmentStatus);
		}

		public void TestOnlyOneBulkContainer()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			CusSCAPivot pivot = house.Pivot.AddNew();
			CusSCAHouse house2 = cMROceanBill.HouseBills.AddNew();
			CusSCAPivot pivot2 = house.Pivot.AddNew();

			pivot.CV_AssociatedContainer = "BULK";
			pivot2.CV_AssociatedContainer = "BULK";

			AssertEquals("Containers", 1, cMROceanBill.Containers.Count);
			AssertEquals("Container is bulk", "BULK", cMROceanBill.Containers[0].CN_ContainerNumber);
			AssertEquals("Container is bulk", "BLK", cMROceanBill.Containers[0].CN_ContainerMode);
		}

		public void TestUnderbonds()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = cMROceanBill.Containers.AddNew();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			CusSCAPivot pivot = house.Pivot.AddNew();
			ICusUnderbondDependentCollectionParent pivotUnder = pivot;
			AssertNotNull(pivotUnder.Underbonds);
			Assert(pivot.IsRegisteredEditableChildObject(pivotUnder.Underbonds));
		}

		public void TestUnderbondHumanReadableName()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = cMROceanBill.Containers.AddNew();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			house.CA_HouseBill = "321";
			CusSCAPivot pivot = house.Pivot.AddNew();
			pivot.CV_CN = container.PK;
			ICusUnderbondDependentCollectionParent pivotUnder = pivot;
			pivot.CN_ContainerNumber = "123";
			AssertEquals("Pivot UnderbondHumanReadableName is Container", "Container 123 - Housebill 321", pivotUnder.UnderbondHumanReadableName);
		}

		public void TestUnderbondHumanReadableNameForMultiOceanUnpack()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			cMROceanBill.CB_MultiOBLUnpack = true;
			CusSCAContainer container = cMROceanBill.Containers.AddNew();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			house.CA_HouseBill = "321";
			CusSCAPivot pivot = house.Pivot.AddNew();
			pivot.CV_CN = container.PK;
			ICusUnderbondDependentCollectionParent pivotUnder = pivot;
			pivot.CN_ContainerNumber = "123";
			AssertEquals("Pivot UnderbondHumanReadableName is Container", "Container 123 - Oceanbill 321", pivotUnder.UnderbondHumanReadableName);
		}

		public void TestOutturnableLines()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = cMROceanBill.Containers.AddNew();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			CusSCAPivot pivot = house.Pivot.AddNew();
			pivot.CV_CN = container.PK;
			IOutturnableLine[] result = ((ICusUnderbondDependentCollectionParent)pivot).OutturnableLines;
			AssertEquals("Result.Length", 1, result.Length);
			AssertEquals("Result[0]", pivot, result[0]);
		}

		public void TestISeaOutturnReportHeaderInformationProvider_GetHeader()
		{
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = cMROceanBill.Containers.AddNew();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			CusSCAPivot pivot = house.Pivot.AddNew();
			pivot.CV_CN = container.PK;
			AssertNotNull(((ISeaOutturnReportHeaderInformationProvider)pivot).GetHeader(underbond));
		}

		public void TestCV_NetWeight()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			CusSCAContainer container = oceanBill.Containers.AddNew();
			CusSCAPivot pivot = house.Pivot.AddNew();
			container.Pivots.Add(pivot);
			AssertEquals("Pre-Condition CV_NetWeight empty", 0m, pivot.CV_NetWeight);
			pivot.CV_Weight = 100m;
			AssertEquals("Net weight should default to Gross Weight if Empty", pivot.CV_Weight, pivot.CV_NetWeight);
			pivot.CV_NetWeight = 0m;
			pivot.CV_Weight = 0;
			pivot.CV_NetWeight = 200m;
			AssertEquals("Gross Weight (CV_Weight) Should default from Net Weight", pivot.CV_NetWeight, pivot.CV_Weight);
		}

		public void TestLCLUnderbondDefaults()
		{
			CommonConsol consol = CreateLCLConsol();
			consol.JK_OA_ArrivalCTOAddress = ArrivalCTO.MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = UnpackDepot.MainAddress.PK;
			CommonContainer container = consol.Containers.AddNew();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 10;
			shipment.JS_F3_NKPackType = "BOX";
			shipment.JS_RL_NKDestination = houseBillDestinationPort;

			CMRSeaCargoSynchroniser synchroniser = new CMRSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			synchroniser.LoadHouseBills();

			CusUnderbond underbondFromCTO = oceanBill.Containers[0].Underbonds.AddNew();
			underbondFromCTO.C4_ParentID = oceanBill.Containers[0].PK;
			underbondFromCTO.C4_ParentTableCode = CusSCAContainerSchema.Constants.Prefix;
			((IUnderbondDefaultValueProvider)oceanBill.Containers[0]).SetUnderbondDefaultValues(underbondFromCTO);
			AssertEquals("Pre Condition: Underbond 1 Origin Address", ArrivalCTO.MainAddress.PK, underbondFromCTO.C4_OA_OriginAddress);
			AssertEquals("Pre Condition: Underbond 1 Origin Premise Code ", arrivalCTOPremiseCode, underbondFromCTO.C4_OriginPremiseID);
			AssertEquals("Pre Condition: Underbond 1 Destination Address", UnpackDepot.MainAddress.PK, underbondFromCTO.C4_OA_DestinationAddress);
			AssertEquals("Pre Condition: Underbond 1 Destination Premise Code", localDepotPremiseCode, underbondFromCTO.C4_DestinationPremiseID);
			AssertEquals("Pre Condition: Underbond 1 Is move from Discharge", true, underbondFromCTO.C4_IsMoveFromDischarge);
			AssertEquals("Pre Condition: Underbond 1 Package Count", 1, (int)underbondFromCTO.C4_PiecesManifested);
			AssertEquals("Pre Condition: Underbond 1 Package Type", CMRPackageTypes.Codes.UnpackedOrPacked, underbondFromCTO.C4_PackageType);
			AssertEquals("Pre Condition: Underbond 1 Reason", CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination, underbondFromCTO.C4_MovementReason);

			var underbondFromDepot = oceanBill.HouseBills[0].Pivot[0].Underbonds.AddNew();
			underbondFromDepot.C4_ParentID = oceanBill.HouseBills[0].Pivot[0].PK;
			underbondFromDepot.C4_ParentTableCode = CusSCAPivotSchema.Constants.Prefix;
			((IUnderbondDefaultValueProvider)oceanBill.HouseBills[0].Pivot[0]).SetUnderbondDefaultValues(underbondFromDepot);

			AssertEquals("Underbond 2 Origin Address", UnpackDepot.MainAddress.PK, underbondFromDepot.C4_OA_OriginAddress);
			AssertEquals("Underbond 2 Origin Premise Code ", localDepotPremiseCode, underbondFromDepot.C4_OriginPremiseID);
			AssertEquals("Underbond 2 Is move from Discharge", false, underbondFromDepot.C4_IsMoveFromDischarge);
			AssertEquals("Underbond 2 Package Count", 10, (int)underbondFromDepot.C4_PiecesManifested);
			AssertEquals("Underbond 2 Package Type", CMRPackageTypes.Codes.Box, underbondFromDepot.C4_PackageType);
			AssertEquals("Underbond 2 Reason", CMRUnderbondRequestCodes.Codes.DeliveryToFinalDestination, underbondFromDepot.C4_MovementReason);
		}

		public void TestBBKUnderbondDefaults()
		{
			CommonConsol consol = CreateBreakBulkConsol();
			consol.JK_OA_ArrivalCTOAddress = ArrivalCTO.MainAddress.PK;
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 10;
			shipment.JS_F3_NKPackType = "BOX";
			shipment.JS_RL_NKDestination = houseBillDestinationPort;

			CMRSeaCargoSynchroniser synchroniser = new CMRSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			synchroniser.LoadHouseBills();

			var underbondFromCTO = oceanBill.HouseBills[0].Pivot[0].Underbonds.AddNew();
			underbondFromCTO.C4_ParentID = oceanBill.HouseBills[0].Pivot[0].PK;
			underbondFromCTO.C4_ParentTableCode = CusSCAPivotSchema.Constants.Prefix;
			((IUnderbondDefaultValueProvider)oceanBill.HouseBills[0].Pivot[0]).SetUnderbondDefaultValues(underbondFromCTO);

			AssertEquals("Underbond Origin Address", ArrivalCTO.MainAddress.PK, underbondFromCTO.C4_OA_OriginAddress);
			AssertEquals("Underbond Origin Premise Code ", arrivalCTOPremiseCode, underbondFromCTO.C4_OriginPremiseID);
			AssertEquals("Underbond Is move from Discharge", true, underbondFromCTO.C4_IsMoveFromDischarge);
			AssertEquals("Underbond Package Count", 10, (int)underbondFromCTO.C4_PiecesManifested);
			AssertEquals("Underbond Package Type", CMRPackageTypes.Codes.Box, underbondFromCTO.C4_PackageType);
			AssertEquals("Underbond Reason", CMRUnderbondRequestCodes.Codes.DeliveryToFinalDestination, underbondFromCTO.C4_MovementReason);
		}

		#region Implementation

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
		}

		#endregion
	}
}
